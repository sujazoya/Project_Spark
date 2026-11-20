using System;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.HiddenLayers;
using SparkLevel101Process = ProjectSpark.Gameplay.HiddenLayers.SparkLevel101Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.HiddenLayers
{
    internal static partial class SparkLevel101Builder
    {
        private static SparkLevelDefinition BuildDefinition()
        {
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Definition);
            bool exists = level;
            if (!exists) level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level, ("levelId", "level_101_hidden_layers"), ("displayName", "The Hidden Layers"),
                ("description", "Commission a six-layer MLX-9, diagnose three internal signal faults and recover W101 authorization."),
                ("completionMode", 0), ("requiredTargetCount", 6), ("requireClosedReturn", false), ("requireMinimumVoltage", false),
                ("rejectShortCircuit", true), ("rejectTargetShort", true), ("unlockedByDefault", true), ("autoAdvanceOnCompletion", false));
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
            var titles = new[] { "Assemble 76 components", "Route 100 traces and plate 24 vias", "Commission the MCU and memory",
                "Qualify signal integrity", "Recover three signal faults", "Prove W101 recovery" };
            var criteria = new[] { "Fit every component and close its native assembly contact.",
                "Verify all required native route pairs, plated spans and W101 continuity.",
                "Verify all three rails, clock/reset, a qualified SPI flash image and RAM readback.",
                "Train both native lanes and verify return impedance, skew, TDR and a finite PRBS interval.",
                "Repair V09 continuity, the W073 ground stitch and W053 pair skew, then qualify each in a fresh epoch.",
                "Open W101, verify independent inhibition, reconnect, acquire new edges, explicitly reset and certify a new training epoch." };
            for (int i = 0; i < targets.arraySize; i++)
            {
                var target = targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue = $"l101_{i + 1}";
                target.FindPropertyRelative("displayName").stringValue = titles[i];
                target.FindPropertyRelative("description").stringValue = criteria[i];
                target.FindPropertyRelative("targetComponentId").stringValue = SparkLevel101Process.Proof(i);
                target.FindPropertyRelative("targetType").enumValueIndex = (int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            if (!exists) AssetDatabase.CreateAsset(level, Definition);
            AssetDatabase.SaveAssetIfDirty(level);
            return level;
        }

        private static void Bind(SparkLevel101Process process)
        {
            var data = new SerializedObject(process.bindings);
            var terminals = data.FindProperty("terminals");
            var pins = new[] { process.input, process.ground, process.standby, process.vdd, process.clockSense, process.resetSense,
                process.laneRail, process.flashTx, process.flashRx, process.returnProbe, process.lanes[0].receiveP, process.lanes[0].receiveN, process.lanes[1].receiveP,
                process.lanes[1].receiveN, process.monitorSupply, process.protectedFeed, process.jumper.from, process.jumper.to };
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
            for (int i = 0; i < proofs.arraySize; i++) proofs.GetArrayElementAtIndex(i).stringValue = SparkLevel101Process.Proof(i);
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
                if (scene.isDirty && !registeredOnDisk)
                    throw new InvalidOperationException("MainMenu has unsaved changes. Save it before campaign registration.");

                int index = levels.arraySize;
                for (int i = 0; i < levels.arraySize; i++)
                {
                    var current = levels.GetArrayElementAtIndex(i).objectReferenceValue as SparkLevelDefinition;
                    if (!current) continue;
                    string path = AssetDatabase.GetAssetPath(current);
                    if (path.Contains("/Level_100_Layout/")) { index = i + 1; break; }
                    if (current.LevelId is "level_105" or "level_106" or "level_107") { index = i; break; }
                }
                levels.InsertArrayElementAtIndex(index);
                levels.GetArrayElementAtIndex(index).objectReferenceValue = level;
                var active = campaign.FindProperty("activeLevelIndex");
                if (active.intValue >= index && active.intValue >= 0) active.intValue++;
                campaign.ApplyModifiedPropertiesWithoutUndo();
                if (registeredOnDisk) return;
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Level 101 campaign registration.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
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
