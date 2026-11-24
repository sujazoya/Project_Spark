using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RedundancyCitadel;
using SparkLevel113Process = ProjectSpark.Gameplay.RedundancyCitadel.SparkLevel113Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.RedundancyCitadel
{
    internal static partial class SparkLevel113Builder
    {
        private static SparkLevelDefinition BuildDefinition()
        {
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool exists = level;
            if (!exists) level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level, ("levelId", "level_113"), ("displayName", "The Redundancy Citadel"),
                ("description", "Assemble the RDX-21, qualify independent power and ECC, repair three faults and prove the W233 interlock."),
                ("completionMode", 0), ("requiredTargetCount", 6), ("requireClosedReturn", false), ("requireMinimumVoltage", false),
                ("rejectShortCircuit", true), ("rejectTargetShort", true), ("unlockedByDefault", true),
                ("unlockNextLevelOnCompletion", true), ("autoUnlockNextLevel", true), ("autoAdvanceOnCompletion", false));
            var data = new SerializedObject(level);
            var supplies = data.FindProperty("powerSources");
            supplies.arraySize = 2;
            string[] names = { "PRIMARY_12V", "RESERVE_12V" };
            string[] positive = { "SUPPLY_POS", "RESERVE_POS" };
            for (int i = 0; i < names.Length; i++)
            {
                var item = supplies.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("sourceName").stringValue = names[i];
                item.FindPropertyRelative("positiveTerminalId").stringValue = positive[i];
                item.FindPropertyRelative("negativeTerminalId").stringValue = "SUPPLY_RET";
                item.FindPropertyRelative("nominalVoltage").floatValue = 12f;
            }
            var targets = data.FindProperty("targets");
            targets.arraySize = 0;
            targets.arraySize = 6;
            var titles = new[] { "Assemble 166 RDX-21 stations", "Verify the 18-layer training network",
                "Commission redundant power", "Qualify failover, ECC and timing",
                "Diagnose and repair three faults", "Qualify the W233 safety interlock" };
            var criteria = new[] { "Fit and secure each original footprint, inspecting the critical packages and jumper station.",
                "Build 232 copper connections and 76 vias. Verify continuity in eight independent test domains.",
                "Measure both 12 V inputs, check reverse isolation and confirm protected 5 V and 3.3 V rails.",
                "Time reserve transfer under load, classify single- and double-bit ECC errors, and audit both clocks and the watchdog.",
                "Localize and repair the primary OR-ing contact, the ECC via and the clock reference. Reacquire the affected evidence.",
                "Open W233 under supervision, confirm protected-load inhibition with diagnostics powered, reconnect, requalify, explicitly reset and finish." };
            for (int i = 0; i < targets.arraySize; i++)
            {
                var target = targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue = $"l113_{i + 1}";
                target.FindPropertyRelative("displayName").stringValue = titles[i];
                target.FindPropertyRelative("description").stringValue = criteria[i];
                target.FindPropertyRelative("targetComponentId").stringValue = SparkLevel113Process.Proof(i);
                target.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (!exists) AssetDatabase.CreateAsset(level, Definition);
            AssetDatabase.SaveAssetIfDirty(level);
            return level;
        }

        private static void Bind(SparkLevel113Process process)
        {
            var data = new SerializedObject(process.bindings);
            var terminals = data.FindProperty("terminals");
            var pins = new[] { process.input, process.reserveInput, process.ground, process.bus, process.rail5,
                process.rail33, process.monitorSupply, process.protectedFeed, process.jumper.from, process.jumper.to };
            terminals.arraySize = pins.Length;
            for (int i = 0; i < pins.Length; i++)
            {
                var terminal = terminals.GetArrayElementAtIndex(i);
                terminal.FindPropertyRelative("id").stringValue = pins[i].name;
                terminal.FindPropertyRelative("terminal").objectReferenceValue = pins[i];
            }
            var components = data.FindProperty("components");
            components.arraySize = 2;
            components.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "PRIMARY_12V";
            components.GetArrayElementAtIndex(0).FindPropertyRelative("component").objectReferenceValue = process.source;
            components.GetArrayElementAtIndex(1).FindPropertyRelative("id").stringValue = "RESERVE_12V";
            components.GetArrayElementAtIndex(1).FindPropertyRelative("component").objectReferenceValue = process.reserve;
            var proofs = data.FindProperty("runtimeComponentIds");
            proofs.arraySize = 6;
            for (int i = 0; i < proofs.arraySize; i++) proofs.GetArrayElementAtIndex(i).stringValue = SparkLevel113Process.Proof(i);
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

            RegisterChapter(level);
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
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Level 113 campaign registration.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static void RegisterChapter(SparkLevelDefinition level)
        {
            const string path = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Chapter/Chapter_11_HardwareQualification.asset";
            var chapter = new SerializedObject(AssetDatabase.LoadAssetAtPath<SparkChapterDefinition>(path));
            var levels = chapter.FindProperty("levels");
            int successor = -1;
            for (int i = 0; i < levels.arraySize; i++)
            {
                var entry = levels.GetArrayElementAtIndex(i).objectReferenceValue as SparkLevelDefinition;
                if (entry == level) return;
                if (entry && entry.LevelId == "level_114") successor = i;
            }
            if (successor < 1 || !(levels.GetArrayElementAtIndex(successor - 1).objectReferenceValue is SparkLevelDefinition predecessor) ||
                predecessor.LevelId != "level_112") throw new InvalidOperationException("The 112–114 campaign order could not be resolved.");
            levels.InsertArrayElementAtIndex(successor);
            levels.GetArrayElementAtIndex(successor).objectReferenceValue = level;
            chapter.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(chapter.targetObject);
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
                throw new InvalidOperationException("The Level 113 definition has no persistent asset ID.");
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
