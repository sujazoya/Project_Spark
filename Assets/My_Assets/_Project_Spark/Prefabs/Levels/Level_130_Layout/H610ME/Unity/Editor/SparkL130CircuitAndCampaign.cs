using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RecoveryLab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level130.Editor
{
    internal static partial class SparkL130PlayableBuilder
    {
        private static SparkLevelDefinition BuildDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool created = !definition;
            if (created) definition = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(definition, ("levelId", "level_130"), ("displayName", "The Recovery Lab"),
                ("description", "Recover a DDR5 workshop PC with successive EPS, virtual ROM and fan-feedback faults, then pass the final cold-start audit."),
                ("requiredTargetCount", 6), ("rejectShortCircuit", true), ("requireClosedReturn", false),
                ("requireMinimumVoltage", false), ("allowAnyConfiguredPowerSource", true));
            var data = new SerializedObject(definition);
            var power = data.FindProperty("powerSources");
            power.arraySize = 1;
            var first = power.GetArrayElementAtIndex(0);
            first.FindPropertyRelative("sourceName").stringValue = "SERVICE_VERIFIER";
            first.FindPropertyRelative("positiveTerminalId").stringValue = "FIXTURE_POS";
            first.FindPropertyRelative("negativeTerminalId").stringValue = "FIXTURE_RETURN";
            first.FindPropertyRelative("nominalVoltage").floatValue = 3.3f;

            string[] names = { "Complete intake and service inventory", "Restore protected CPU power", "Recover the virtual ROM and cold POST",
                "Qualify memory, PCIe and NVMe", "Restore fan feedback and prove thermal protection", "Sign the final cold-start audit" };
            string[] descriptions = { "Inspect ten original landmarks and verify five keyed service modules.",
                "Compare both sides of the EPS connector, isolate, reseat and verify fresh core measurements.",
                "Capture both lab-ROM reads. Restore the correct fictional image under isolation, then repeat initialization.",
                "After a full restart, independently verify both memory channels, both PCIe links and NVMe block integrity.",
                "Show motor power with absent tach, prove the loaded cutoff, replace the matching spare and rerun thermal qualification.",
                "After an isolated restart collect fresh power, POST, device and thermal evidence on the same boot." };
            var targets = data.FindProperty("targets");
            targets.arraySize = 0;
            targets.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                var item = targets.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("targetId").stringValue = $"l130_{i + 1}";
                item.FindPropertyRelative("displayName").stringValue = names[i];
                item.FindPropertyRelative("description").stringValue = descriptions[i];
                item.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                item.FindPropertyRelative("targetComponentId").stringValue = SparkL130Workshop.Proof(i);
                item.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (created)
            {
                AssetDatabase.CreateAsset(definition, Definition);
                AssetDatabase.ImportAsset(Definition, ImportAssetOptions.ForceSynchronousImport);
            }
            AssetDatabase.SaveAssetIfDirty(definition);
            var persistent = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            if (!persistent || !EditorUtility.IsPersistent(persistent))
                throw new InvalidOperationException("The level definition has not imported as an asset.");
            return persistent;
        }

        private static SparkTerminal[] BuildCircuit(SparkL130Workshop board, Transform root)
        {
            var owner = Child(root, "Logical training terminals").gameObject.AddComponent<SparkElectronicObject>();
            var pins = new List<SparkTerminal>();
            SparkTerminal Pin(string name, Vector3 position)
            {
                var pin = Child(root, name, position).gameObject.AddComponent<SparkTerminal>();
                Set(pin, ("owner", owner), ("maxConnections", 12), ("providesOutput", true), ("acceptsInput", true));
                pins.Add(pin);
                return pin;
            }

            SparkSwitch Contact(string name, SparkTerminal from, SparkTerminal to, bool closed)
            {
                var edge = Child(root, name).gameObject.AddComponent<SparkSwitch>();
                Set(edge, ("inputTerminal", from), ("outputTerminal", to), ("closedResistance", .002f),
                    ("state", closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
                to.transform.SetParent(edge.transform, true);
                return edge;
            }

            SparkL130Workshop.Channel Load(string name, SparkTerminal from, float resistance, Vector3 location, bool closed)
            {
                var pin = Pin(name, location);
                var edge = Contact(name + " / contact", from, pin, closed);
                var load = Child(root, name + " / load").gameObject.AddComponent<SparkResistor>();
                Set(load, ("terminalA", pin), ("terminalB", board.ground), ("resistance", resistance), ("maximumPowerWatts", 15f));
                Set(pin, ("owner", load));
                var burn = load.GetComponent<SparkResistorBurn>();
                Set(burn, ("ratedWatts", 15f), ("thermalCapacityJPerC", 14f), ("coolingWattsPerC", 1.2f));
                return new SparkL130Workshop.Channel { pin = pin, contact = edge, load = load };
            }

            board.ground = Pin("BENCH_RETURN", Vector3.zero);
            string[] names = { "STANDBY", "EPS_SOURCE", "VCORE", "DDR5", "IO" };
            float[] volts = { 5f, 12f, 1.10f, 1.10f, 3.30f };
            float[] limits = { .18f, .45f, .90f, .25f, .35f };
            float[] resistance = { 150f, 180f, 10f, 33f, 120f };
            int[] mapping = { 0, 1, 3, 4, 5 };
            board.supplies = new SparkPowerSupply[names.Length];
            board.rails = new SparkL130Workshop.Channel[mapping.Length + 1];
            for (int index = 0; index < names.Length; index++)
            {
                string id = names[index];
                Vector3 site = board.railSites[mapping[index]].localPosition;
                var sourcePin = Pin($"BENCH_{id}_SOURCE", site + Vector3.up * .002f);
                string deviceName = index == 0 ? "TRAINING_STANDBY" : $"TRAINING_{id}";
                var device = Child(root, deviceName).gameObject.AddComponent<SparkPowerSupply>();
                Set(device, ("positiveTerminal", sourcePin), ("negativeTerminal", board.ground),
                    ("outputVoltage", volts[index]), ("currentLimit", limits[index]), ("state", SparkPowerSupplyState.Standby));
                sourcePin.transform.SetParent(device.transform, true);
                Set(sourcePin, ("owner", device));
                board.rails[mapping[index]] = Load($"TP_{id}", sourcePin, resistance[index], site, true);
                board.supplies[index] = device;
            }
            board.rails[2] = Load("TP_EPS_SOCKET", board.rails[1].pin, 200f, board.railSites[2].localPosition, false);
            board.ground.transform.SetParent(board.supplies[0].transform, true);
            Set(board.ground, ("owner", board.supplies[0]));

            // The inspection fixture is electrically isolated from the motherboard.
            // Its independent power lets the manager record mechanical service while
            // every board supply is disabled.
            var verifierPositive = Pin("FIXTURE_POS", Vector3.zero);
            var verifierReturn = Pin("FIXTURE_RETURN", Vector3.zero);
            board.serviceVerifier = Child(root, "SERVICE_VERIFIER").gameObject.AddComponent<SparkPowerSupply>();
            Set(board.serviceVerifier, ("positiveTerminal", verifierPositive), ("negativeTerminal", verifierReturn),
                ("outputVoltage", 3.3f), ("currentLimit", .1f), ("state", SparkPowerSupplyState.Active));
            foreach (var pin in new[] { verifierPositive, verifierReturn })
            {
                pin.transform.SetParent(board.serviceVerifier.transform, true);
                Set(pin, ("owner", board.serviceVerifier));
            }
            var verificationLoad = Child(root, "Isolated service verifier load").gameObject.AddComponent<SparkResistor>();
            Set(verificationLoad, ("terminalA", verifierPositive), ("terminalB", verifierReturn),
                ("resistance", 660f), ("maximumPowerWatts", 1f));

            Vector3 RomSite = board.diagnostics[2].anchor.localPosition + Vector3.up * .005f;
            Vector3 FanSite = board.diagnostics[3].anchor.localPosition + Vector3.up * .007f;
            board.romPower = Load("U_BIOS.POWER", board.rails[5].pin, 160f, RomSite, false);
            board.romData = Load("U_BIOS.DATA_READY", board.romPower.pin, 250f, RomSite + Vector3.forward * .004f, false);
            board.fanMotor = Load("CPU_FAN1.MOTOR", board.rails[2].pin, 200f, FanSite, false);
            board.fanTach = Load("CPU_FAN1.TACH", board.rails[2].pin, 400f, FanSite + Vector3.forward * .004f, false);
            board.boost = Load("VCORE.SOAK_LOAD", board.rails[3].pin, 1.7f, board.service[0].anchor.localPosition, false);
            board.ports = new[]
            {
                Load("DDR5_1.WORD_TEST", board.rails[4].pin, 56f, board.diagnostics[4].anchor.localPosition, false),
                Load("DDR5_2.WORD_TEST", board.rails[4].pin, 56f, board.diagnostics[5].anchor.localPosition, false),
                Load("PCIE_X16.LANE_TEST", board.rails[5].pin, 220f, board.diagnostics[7].anchor.localPosition, false),
                Load("PCIE_X1.LANE_TEST", board.rails[5].pin, 220f, new Vector3(-.049f,.004f,-.109f), false),
                Load("M2_1.BLOCK_TEST", board.rails[5].pin, 150f, board.diagnostics[6].anchor.localPosition, false)
            };
            if (pins.Select(pin => pin.name).Distinct(StringComparer.Ordinal).Count() != pins.Count)
                throw new InvalidOperationException("Training contact IDs must be unique.");
            return pins.ToArray();
        }

        private static void Bind(SparkL130Workshop board, SparkTerminal[] pins)
        {
            var data = new SerializedObject(board.bindings);
            var terminals = data.FindProperty("terminals");
            terminals.arraySize = pins.Length;
            for (int i = 0; i < pins.Length; i++)
            {
                var item = terminals.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue = pins[i].name;
                item.FindPropertyRelative("terminal").objectReferenceValue = pins[i];
            }
            var components = data.FindProperty("components");
            components.arraySize = 2;
            components.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "TRAINING_STANDBY";
            components.GetArrayElementAtIndex(0).FindPropertyRelative("component").objectReferenceValue = board.supplies[0];
            components.GetArrayElementAtIndex(1).FindPropertyRelative("id").stringValue = "SERVICE_VERIFIER";
            components.GetArrayElementAtIndex(1).FindPropertyRelative("component").objectReferenceValue = board.serviceVerifier;
            var proof = data.FindProperty("runtimeComponentIds");
            proof.arraySize = 6;
            for (int i = 0; i < proof.arraySize; i++) proof.GetArrayElementAtIndex(i).stringValue = SparkL130Workshop.Proof(i);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(SparkLevelDefinition definition, SparkLevelLayout layout)
        {
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<SparkLevelLayoutCatalog>(Catalog));
            var entries = catalog.FindProperty("entries");
            var matches = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .Where(item => item.FindPropertyRelative("level").objectReferenceValue == definition ||
                    item.FindPropertyRelative("prefab").objectReferenceValue == layout).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("Duplicate Level 130 catalog entry.");
            var entry = matches.Length == 0 ? entries.GetArrayElementAtIndex(entries.arraySize++) : matches[0];
            entry.FindPropertyRelative("level").objectReferenceValue = definition;
            entry.FindPropertyRelative("prefab").objectReferenceValue = layout;
            catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(catalog.targetObject);

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(definition, out string guid, out long id))
                throw new InvalidOperationException("Level 130 has no persistent definition ID.");
            string yaml = File.ReadAllText(Scene);
            var arrays = Regex.Matches(yaml, @"(?m)^  levels:\r?\n(?:  - \{fileID: [^\r\n]+\}\r?\n)+");
            if (arrays.Count != 1) throw new InvalidOperationException("Cannot locate the unique MainMenu level array.");

            var scene = SceneManager.GetSceneByPath(Scene);
            if (scene.isLoaded)
            {
                var manager = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LevelGamePlayManager>(true)).Single();
                var campaign = new SerializedObject(manager);
                var levels = campaign.FindProperty("levels");
                if (!Enumerable.Range(0, levels.arraySize).Any(index => levels.GetArrayElementAtIndex(index).objectReferenceValue == definition))
                {
                    int index = levels.arraySize;
                    levels.InsertArrayElementAtIndex(index);
                    levels.GetArrayElementAtIndex(index).objectReferenceValue = definition;
                    campaign.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            // Update just the serialized campaign list on disk, preserving every other open-scene edit.
            if (!arrays[0].Value.Contains("guid: " + guid))
            {
                string newline = yaml.Contains("\r\n") ? "\r\n" : "\n";
                string patch = Path.Combine("Temp", $"Level130Campaign-{Guid.NewGuid():N}.tmp");
                try
                {
                    File.WriteAllText(patch, yaml.Insert(arrays[0].Index + arrays[0].Length,
                        $"  - {{fileID: {id}, guid: {guid}, type: 2}}" + newline));
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        // Unity can keep a read mapping for the current scene. An atomic
                        // rename preserves that mapping and the open editor state.
                        if (!MoveFileExW(Path.GetFullPath(patch), Path.GetFullPath(Scene), 1))
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else File.Replace(patch, Scene, null);
                }
                finally { if (File.Exists(patch)) File.Delete(patch); }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileExW(string source, string destination, uint flags);

        private static void Set(Object target, params (string name, object value)[] entries)
        {
            var data = new SerializedObject(target);
            foreach (var (name, value) in entries)
            {
                var field = data.FindProperty(name) ?? throw new InvalidOperationException(target.GetType().Name + "." + name);
                switch (value)
                {
                    case Object item: field.objectReferenceValue = item; break;
                    case bool flag: field.boolValue = flag; break;
                    case string label: field.stringValue = label; break;
                    case int integer: field.intValue = integer; break;
                    case float number: field.floatValue = number; break;
                    case Enum item: field.enumValueIndex = Convert.ToInt32(item); break;
                    default: throw new InvalidOperationException("Unsupported serialized field " + name);
                }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}


