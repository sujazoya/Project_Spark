using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SiliconWorkshop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level129.Editor
{
    internal static partial class SparkL129PlayableBuilder
    {
        private static SparkLevelDefinition BuildDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool created = !definition;
            if (created) definition = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(definition, ("levelId", "level_129"), ("displayName", "The Silicon Workshop"),
                ("description", "Service the factory-populated DDR5 board, isolate three hardware faults and prove thermal protection before reboot."),
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

            string[] names = { "Complete keyed service assembly", "Qualify all loaded rails", "Verify CPU and dual-channel DDR5 POST",
                "Qualify eight interfaces", "Diagnose and verify three hardware repairs", "Prove protection and complete a fresh reboot" };
            string[] descriptions = { "Lock the CPU, cross-mount the cooler, latch both memory modules and secure the workshop drive.",
                "Verify standby before main power, then measure EPS, core, I/O and memory under load.",
                "Check CPU, both memory channels, video and storage through a fresh diagnostic sequence.",
                "Select valid fixture modes for both PCIe slots, M.2, SATA, HDMI, VGA, USB and LAN.",
                "Isolate PQ04, DDR5_2 and M2_1 from native readings. Retest after every verified replacement.",
                "Open the fan under load, verify shutdown, isolate and restore cooling, reset explicitly and requalify." };
            var targets = data.FindProperty("targets");
            targets.arraySize = 0;
            targets.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                var item = targets.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("targetId").stringValue = $"l129_{i + 1}";
                item.FindPropertyRelative("displayName").stringValue = names[i];
                item.FindPropertyRelative("description").stringValue = descriptions[i];
                item.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                item.FindPropertyRelative("targetComponentId").stringValue = SparkL129Workshop.Proof(i);
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

        private static SparkTerminal[] BuildCircuit(SparkL129Workshop board, Transform root)
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

            SparkL129Workshop.Channel Load(string name, SparkTerminal from, float resistance, Vector3 location, bool closed)
            {
                var pin = Pin(name, location);
                var edge = Contact(name + " / contact", from, pin, closed);
                var load = Child(root, name + " / load").gameObject.AddComponent<SparkResistor>();
                Set(load, ("terminalA", pin), ("terminalB", board.ground), ("resistance", resistance), ("maximumPowerWatts", 15f));
                var burn = load.GetComponent<SparkResistorBurn>();
                Set(burn, ("ratedWatts", 15f), ("thermalCapacityJPerC", 14f), ("coolingWattsPerC", 1.2f));
                return new SparkL129Workshop.Channel { pin = pin, contact = edge, load = load };
            }

            board.ground = Pin("BENCH_RETURN", Vector3.zero);
            string[] names = { "STANDBY", "EPS", "VCORE", "DDR5", "IO" };
            float[] volts = { 5f, 12f, 1.10f, 1.10f, 3.30f };
            float[] limits = { .18f, .35f, .90f, .25f, .35f };
            float[] resistance = { 150f, 150f, 10f, 33f, 120f };
            board.supplies = new SparkPowerSupply[5];
            board.rails = new SparkL129Workshop.Channel[5];
            for (int index = 0; index < names.Length; index++)
            {
                string id = names[index];
                Vector3 site = board.railSites[index].localPosition;
                var sourcePin = Pin($"BENCH_{id}_SOURCE", site + Vector3.up * .002f);
                string deviceName = index == 0 ? "TRAINING_STANDBY" : $"TRAINING_{id}";
                var device = Child(root, deviceName).gameObject.AddComponent<SparkPowerSupply>();
                Set(device, ("positiveTerminal", sourcePin), ("negativeTerminal", board.ground),
                    ("outputVoltage", volts[index]), ("currentLimit", limits[index]), ("state", SparkPowerSupplyState.Standby));
                sourcePin.transform.SetParent(device.transform, true);
                Set(sourcePin, ("owner", device));
                board.rails[index] = Load($"TP_{id}", sourcePin, resistance[index], site, true);
                board.supplies[index] = device;
            }
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

            board.vrm = board.rails[2].contact;
            board.dimms = new[] {
                Load("DDR5_1.TRAIN", board.rails[3].pin, 56f, board.service[2].anchor.localPosition, false),
                Load("DDR5_2.TRAIN", board.rails[3].pin, 56f, board.service[3].anchor.localPosition, false)
            };
            board.storage = Load("M2_1.STORAGE", board.rails[4].pin, 150f, board.interfaceSites[2].anchor.localPosition, false);
            board.fan = Load("CPU_FAN1.TACH_LOAD", board.rails[1].pin, 200f, new Vector3(.048f,.007f,.108f), false);
            board.boost = Load("VCORE.SOAK_LOAD", board.rails[2].pin, 1.7f, board.service[0].anchor.localPosition, false);
            board.ports = board.interfaceSites.Select((site, i) =>
                Load($"FIXTURE_{i + 1:00}", board.rails[4].pin, 220f + 15f * i, site.anchor.localPosition, false)).ToArray();
            if (pins.Select(pin => pin.name).Distinct(StringComparer.Ordinal).Count() != pins.Count)
                throw new InvalidOperationException("Training contact IDs must be unique.");
            return pins.ToArray();
        }

        private static void Bind(SparkL129Workshop board, SparkTerminal[] pins)
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
            for (int i = 0; i < proof.arraySize; i++) proof.GetArrayElementAtIndex(i).stringValue = SparkL129Workshop.Proof(i);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(SparkLevelDefinition definition, SparkLevelLayout layout)
        {
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<SparkLevelLayoutCatalog>(Catalog));
            var entries = catalog.FindProperty("entries");
            var matches = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .Where(item => item.FindPropertyRelative("level").objectReferenceValue == definition ||
                    item.FindPropertyRelative("prefab").objectReferenceValue == layout).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("Duplicate Level 129 catalog entry.");
            var entry = matches.Length == 0 ? entries.GetArrayElementAtIndex(entries.arraySize++) : matches[0];
            entry.FindPropertyRelative("level").objectReferenceValue = definition;
            entry.FindPropertyRelative("prefab").objectReferenceValue = layout;
            catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(catalog.targetObject);

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(definition, out string guid, out long id))
                throw new InvalidOperationException("Level 129 has no persistent definition ID.");
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
                string patch = Path.Combine("Temp", $"Level129Campaign-{Guid.NewGuid():N}.tmp");
                try
                {
                    File.WriteAllText(patch, yaml.Insert(arrays[0].Index + arrays[0].Length,
                        $"  - {{fileID: {id}, guid: {guid}, type: 2}}" + newline));
                    File.Replace(patch, Scene, null);
                }
                finally { if (File.Exists(patch)) File.Delete(patch); }
            }
        }

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


