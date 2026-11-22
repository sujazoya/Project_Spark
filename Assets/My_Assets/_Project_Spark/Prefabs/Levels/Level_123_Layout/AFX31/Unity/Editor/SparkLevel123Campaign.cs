using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.FlightSovereign;
using SparkLevel123Process = ProjectSpark.Gameplay.FlightSovereign.SparkLevel123Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.FlightSovereign
{
    internal static partial class SparkLevel123Builder
    {
        private static SparkLevelDefinition BuildDefinition()
        {
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool exists = level;
            if (!exists) level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level, ("levelId", "level_123"), ("displayName", "The Flight Sovereign"),
                ("description", "Assemble the AFX-31, qualify eight ground-stand drive stages and recover W401."),
                ("completionMode", 0), ("requiredTargetCount", 6), ("requireClosedReturn", false), ("requireMinimumVoltage", false),
                ("rejectShortCircuit", true), ("rejectTargetShort", true), ("unlockedByDefault", true),
                ("unlockNextLevelOnCompletion", true), ("autoUnlockNextLevel", true), ("autoAdvanceOnCompletion", false));
            var data = new SerializedObject(level);
            var supplies = data.FindProperty("powerSources");
            supplies.arraySize = 1;
            var item = supplies.GetArrayElementAtIndex(0);
            item.FindPropertyRelative("sourceName").stringValue = "TRAINING_INPUT";
            item.FindPropertyRelative("positiveTerminalId").stringValue = "SUPPLY_POS";
            item.FindPropertyRelative("negativeTerminalId").stringValue = "SUPPLY_RET";
            item.FindPropertyRelative("nominalVoltage").floatValue = 5f;
            var targets = data.FindProperty("targets");
            targets.arraySize = 0;
            targets.arraySize = 6;
            var titles = new[] { "Assemble 312 avionics footprints", "Complete the 38-layer training circuit",
                "Commission dual controls and eight drive stages", "Verify the 64-sample ground-stand envelope",
                "Repair three independent sensor faults", "Restore W401 protected drive authorization" };
            var criteria = new[] { "Fit exact parts and verify the precision contacts before securing them.",
                "Verify 400 native copper edges, 156 plated spans and W401.",
                "Measure dual clocks and verify reset release, three inertial channels, GNSS, altitude and eight returns.",
                "Prove spin-up, gust recovery, descent, overcurrent cut and emergency stop on the stand.",
                "Repair the M5 gate, IMU B SPI via and barometer reference. Requalify after each.",
                "Open W401 during protected replay. Verify inhibit, reconnect, requalify and explicitly reset U3." };
            for (int i = 0; i < targets.arraySize; i++)
            {
                var target = targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue = $"l123_{i + 1}";
                target.FindPropertyRelative("displayName").stringValue = titles[i];
                target.FindPropertyRelative("description").stringValue = criteria[i];
                target.FindPropertyRelative("targetComponentId").stringValue = SparkLevel123Process.Proof(i);
                target.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (!exists) AssetDatabase.CreateAsset(level, Definition);
            AssetDatabase.SaveAssetIfDirty(level);
            return level;
        }

        private static void Bind(SparkLevel123Process process)
        {
            var data = new SerializedObject(process.bindings);
            var terminals = data.FindProperty("terminals");
            var pins = new[] { process.input, process.ground, process.rail, process.monitorSupply, process.clockPin,
                process.secondaryClock, process.resetPin, process.driveRail, process.barometerReference, process.protectedFeed, process.jumper.from, process.jumper.to }
                .Concat(process.logicPins).Concat(process.sensorPins).Concat(process.motorPins).Concat(process.gatePins).ToArray();
            terminals.arraySize = pins.Length;
            for (int i = 0; i < pins.Length; i++)
            {
                var terminal = terminals.GetArrayElementAtIndex(i);
                terminal.FindPropertyRelative("id").stringValue = pins[i].name;
                terminal.FindPropertyRelative("terminal").objectReferenceValue = pins[i];
            }
            var components = data.FindProperty("components");
            components.arraySize = 1;
            components.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "TRAINING_INPUT";
            components.GetArrayElementAtIndex(0).FindPropertyRelative("component").objectReferenceValue = process.source;
            var proofs = data.FindProperty("runtimeComponentIds");
            proofs.arraySize = 6;
            for (int i = 0; i < proofs.arraySize; i++) proofs.GetArrayElementAtIndex(i).stringValue = SparkLevel123Process.Proof(i);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(SparkLevelDefinition level, SparkLevelLayout layout)
        {
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<SparkLevelLayoutCatalog>(Catalog));
            var entries = catalog.FindProperty("entries");
            var entry = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .FirstOrDefault(item => item.FindPropertyRelative("level").objectReferenceValue == level ||
                    item.FindPropertyRelative("prefab").objectReferenceValue == layout);
            if (entry == null) entry = entries.GetArrayElementAtIndex(entries.arraySize++);
            entry.FindPropertyRelative("level").objectReferenceValue = level;
            entry.FindPropertyRelative("prefab").objectReferenceValue = layout;
            catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(catalog.targetObject);

            var scene = SceneManager.GetSceneByPath(Scene);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Additive);
            try
            {
                var manager = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<LevelGamePlayManager>(true)).Single();
                var campaign = new SerializedObject(manager);
                var levels = campaign.FindProperty("levels");
                if (Enumerable.Range(0, levels.arraySize).Any(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == level)) return;
                bool registeredOnDisk = File.ReadAllText(Scene).Contains("guid: " + AssetDatabase.AssetPathToGUID(Definition));
                bool wasDirty = scene.isDirty;
                if (wasDirty && !registeredOnDisk) AppendToSavedCampaign(level);

                // Existing progression is indexed by campaign position.
                int index = levels.arraySize;
                levels.InsertArrayElementAtIndex(index);
                levels.GetArrayElementAtIndex(index).objectReferenceValue = level;
                campaign.ApplyModifiedPropertiesWithoutUndo();
                if (registeredOnDisk || wasDirty) return;
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Level 123 campaign registration.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static void AppendToSavedCampaign(SparkLevelDefinition level)
        {
            // Preserve the active user's unsaved scene state. Append only to the
            // unique disk-serialized campaign array, with the current layout format checked.
            string yaml = File.ReadAllText(Scene);
            var arrays = Regex.Matches(yaml, @"(?m)^  levels:\r?\n(?:  - \{fileID: [^\r\n]+\}\r?\n)+");
            if (arrays.Count != 1)
                throw new InvalidOperationException("The saved MainMenu campaign array could not be identified uniquely.");
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(level, out string guid, out long id))
                throw new InvalidOperationException("The Level 123 definition has no persistent asset ID.");
            string newline = yaml.Contains("\r\n") ? "\r\n" : "\n";
            int end = arrays[0].Index + arrays[0].Length;
            File.WriteAllText(Scene, yaml.Insert(end, $"  - {{fileID: {id}, guid: {guid}, type: 2}}" + newline));
        }

        private static void Set(Object target, params (string name, object value)[] values)
        {
            var data = new SerializedObject(target);
            foreach (var (name, value) in values)
            {
                var property = data.FindProperty(name) ?? throw new InvalidOperationException(target.GetType().Name + "." + name);
                switch (value)
                {
                    case Object reference: property.objectReferenceValue = reference; break;
                    case string text: property.stringValue = text; break;
                    case bool flag: property.boolValue = flag; break;
                    case float number: property.floatValue = number; break;
                    case Enum item: property.enumValueIndex = Convert.ToInt32(item); break;
                    case int number when property.propertyType == SerializedPropertyType.Enum: property.enumValueIndex = number; break;
                    case int number: property.intValue = number; break;
                    default: throw new InvalidOperationException("Unsupported field " + name);
                }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
