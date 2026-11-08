#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Prefab-mode-only repair for Smart Slot runtime ID declarations.
    /// Deliberately independent of both existing custom Inspectors.
    /// Does not change legacy bridge rules, fixed IDs, target definitions or circuit code.
    /// </summary>
    public static class SparkLevelSmartBindingRepair
    {
        private const string Menu = "Tools/Project Spark/Levels/REPAIR Smart Slot IDs (Prefab)";

        private sealed class Entry
        {
            public SparkShelfSlot slot;
            public string slotId, componentId, terminalAId, terminalBId;
        }

        [MenuItem(Menu)]
        private static void Execute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Smart Slot Repair", "Exit Play Mode first.", "OK");
                return;
            }

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null || stage.prefabContentsRoot == null)
            {
                EditorUtility.DisplayDialog("Smart Slot Repair",
                    "Open the ORIGINAL Level_26_Layout prefab in Prefab Mode, then run this command.", "OK");
                return;
            }

            GameObject root = stage.prefabContentsRoot;
            SparkLevelLayout layout = root.GetComponent<SparkLevelLayout>();
            SparkLevelSceneBindings bindings = root.GetComponent<SparkLevelSceneBindings>();
            if (bindings == null)
            {
                var found = root.GetComponentsInChildren<SparkLevelSceneBindings>(true);
                if (found.Length == 1) bindings = found[0];
            }
            var bridges = root.GetComponentsInChildren<SparkLevelWorkstationBindingBridge>(true);
            if (layout == null || bindings == null || bridges.Length != 1)
            {
                EditorUtility.DisplayDialog("Smart Slot Repair",
                    "Expected one SparkLevelLayout on root, one owned SparkLevelSceneBindings, and one bridge. " +
                    "Found layout=" + (layout != null) + ", bindings=" + (bindings != null) +
                    ", bridges=" + bridges.Length + ". Nothing changed.", "OK");
                return;
            }

            var errors = new List<string>();
            var entries = new List<Entry>();
            var seenSlots = new HashSet<string>(StringComparer.Ordinal);
            var seenAliases = new HashSet<string>(StringComparer.Ordinal);
            var allSlots = root.GetComponentsInChildren<SparkShelfSlot>(true);
            int workstationCount = 0;
            foreach (SparkShelfSlot slot in allSlots)
            {
                if (slot == null || slot.Kind != SparkShelfSlotKind.Workstation) continue;
                workstationCount++;
                string id = Clean(slot.SlotId);
                if (id.Length == 0 || !seenSlots.Add(id))
                {
                    errors.Add("Blank or duplicate workstation Slot ID: '" + id + "' on " + Path(slot.transform));
                    continue;
                }
                string c = Clean(slot.RuntimeComponentId);
                string a = Clean(slot.RuntimeTerminalAId);
                string b = Clean(slot.RuntimeTerminalBId);
                if (c.Length == 0 && a.Length == 0 && b.Length == 0)
                {
                    // This is not a configured smart workstation; leave it untouched.
                    continue;
                }
                if (c.Length == 0 || a.Length == 0 || b.Length == 0 || a == b)
                {
                    errors.Add(id + ": incomplete or identical component/terminal IDs.");
                    continue;
                }
                if (!seenAliases.Add("C:" + c) || !seenAliases.Add("T:" + a) ||
                    !seenAliases.Add("T:" + b))
                {
                    errors.Add(id + ": duplicate runtime binding alias; inspect its three IDs.");
                    continue;
                }
                entries.Add(new Entry { slot = slot, slotId = id, componentId = c, terminalAId = a, terminalBId = b });
            }

            var so = new SerializedObject(bindings);
            so.Update();
            var components = so.FindProperty("runtimeComponentIds");
            var terminals = so.FindProperty("runtimeTerminalIds");
            var fixedComponents = so.FindProperty("components");
            var fixedTerminals = so.FindProperty("terminals");
            if (!IsStringArray(components) || !IsStringArray(terminals) || fixedComponents == null || fixedTerminals == null)
                errors.Add("SceneBindings field names/types don't match expected runtimeComponentIds/runtimeTerminalIds and fixed arrays.");
            else
            {
                foreach (Entry e in entries)
                {
                    if (HasFixedId(fixedComponents, e.componentId)) errors.Add(e.slotId + ": component ID conflicts with FIXED binding: " + e.componentId);
                    if (HasFixedId(fixedTerminals, e.terminalAId)) errors.Add(e.slotId + ": terminal ID conflicts with FIXED binding: " + e.terminalAId);
                    if (HasFixedId(fixedTerminals, e.terminalBId)) errors.Add(e.slotId + ": terminal ID conflicts with FIXED binding: " + e.terminalBId);
                }
            }

            if (entries.Count == 0) errors.Add("No configured Smart Slot workstations found.");
            if (errors.Count > 0)
            {
                string failure = "Workstations found: " + workstationCount +
                    "\nConfigured slots: " + entries.Count + "\n\n" + string.Join("\n", errors);
                Debug.LogError("[SmartSlotRepair] BLOCKED\n" + failure, root);
                EditorUtility.DisplayDialog("Smart Slot Repair — BLOCKED", failure, "OK");
                return;
            }

            var existingC = ReadIds(components);
            var existingT = ReadIds(terminals);
            int newC = entries.Select(e => e.componentId).Count(id => !existingC.Contains(id));
            int newT = entries.Sum(e => (existingT.Contains(e.terminalAId) ? 0 : 1) +
                                        (existingT.Contains(e.terminalBId) ? 0 : 1));
            int disabled = entries.Count(e => !e.slot.EnableSmartBinding);

            string summary = "Prefab: " + root.name +
                "\nWorkstations found: " + workstationCount +
                "\nConfigured smart slots: " + entries.Count +
                "\nMissing component IDs: " + newC +
                "\nMissing terminal IDs: " + newT +
                "\nSmart Bindings currently OFF: " + disabled +
                "\n\nThis will ENABLE Smart Binding for configured slots, enable Use Smart Slots on bridge, " +
                "and append missing runtime declarations only. Fixed bindings and legacy rules are preserved.";
            if (!EditorUtility.DisplayDialog("Smart Slot Repair — Preview", summary, "Apply Repair", "Cancel")) return;

            Undo.RecordObject(bindings, "Repair Smart Slot runtime ID declarations");
            Undo.RecordObject(bridges[0], "Enable Smart Slots");
            foreach (Entry e in entries) Undo.RecordObject(e.slot, "Enable Smart Binding");

            foreach (Entry e in entries)
            {
                Append(components, existingC, e.componentId);
                Append(terminals, existingT, e.terminalAId);
                Append(terminals, existingT, e.terminalBId);
            }
            so.ApplyModifiedProperties();

            var bridgeSerialized = new SerializedObject(bridges[0]);
            bridgeSerialized.Update();
            var useSmart = bridgeSerialized.FindProperty("useSmartSlots");
            if (useSmart != null)
            {
                useSmart.boolValue = true;
                bridgeSerialized.ApplyModifiedProperties();
            }

            foreach (Entry e in entries)
            {
                var slotSerialized = new SerializedObject(e.slot);
                slotSerialized.Update();
                var enabled = slotSerialized.FindProperty("enableSmartBinding");
                if (enabled != null)
                {
                    enabled.boolValue = true;
                    slotSerialized.ApplyModifiedProperties();
                }
                else Debug.LogWarning("[SmartSlotRepair] enableSmartBinding field not found on " + e.slotId + "; enable manually.", e.slot);
                EditorUtility.SetDirty(e.slot);
            }

            EditorUtility.SetDirty(bindings);
            EditorUtility.SetDirty(bridges[0]);
            EditorSceneManager.MarkSceneDirty(stage.scene);
            string report = "[SmartSlotRepair] APPLIED to " + root.name + "\n" + summary +
                "\n\nSave the prefab (Ctrl+S), then enter Play Mode and Synchronize AND REPORT.";
            Debug.Log(report, root);
            EditorUtility.DisplayDialog("Smart Slot Repair — Complete",
                "Applied declarations and Smart Binding settings to " + entries.Count +
                " workstations. Save the prefab, restart the level, and run Synchronize AND REPORT.", "OK");
        }

        private static string Clean(string x) => (x ?? "").Trim();
        private static string Path(Transform t)
        {
            string s = t.name;
            while (t.parent != null) { t = t.parent; s = t.name + "/" + s; }
            return s;
        }
        private static bool IsStringArray(SerializedProperty p) =>
            p != null && p.isArray && (p.arraySize == 0 ? p.arrayElementType == "string" :
            p.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.String);
        private static bool HasFixedId(SerializedProperty array, string id)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                var prop = array.GetArrayElementAtIndex(i).FindPropertyRelative("id");
                if (prop != null && prop.stringValue == id) return true;
            }
            return false;
        }
        private static HashSet<string> ReadIds(SerializedProperty array)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < array.arraySize; i++) result.Add(array.GetArrayElementAtIndex(i).stringValue);
            return result;
        }
        private static void Append(SerializedProperty array, HashSet<string> existing, string id)
        {
            if (!existing.Add(id)) return;
            int index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            array.GetArrayElementAtIndex(index).stringValue = id;
        }
    }
}
#endif
