#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay.Editor
{
    /// <summary>
    /// Shared editor-only declaration importer for both Level Scene Bindings
    /// and Workstation Bridge inspectors. No runtime binding or solver changes.
    /// Use on the ORIGINAL level-layout prefab opened in Prefab Mode.
    /// </summary>
    public static class SparkLevelRuntimeIdImporter
    {
        private sealed class Rule
        {
            public string slot, component, a, b, electronic;
            public string origin;
        }

        public sealed class ImportResult
        {
            public int SmartRules;
            public int LegacyRules;
            public int AddedComponents;
            public int AddedTerminals;
            public int AddedElectronics;
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Notes = new List<string>();
            public bool Success => Errors.Count == 0;

            public override string ToString()
            {
                string text = "Smart Slots: " + SmartRules +
                    " | Legacy Rules: " + LegacyRules +
                    "\nAdded: " + AddedComponents + " components, " +
                    AddedTerminals + " terminals, " + AddedElectronics + " electronic objects.";
                if (Notes.Count > 0) text += "\nNotes:\n- " + string.Join("\n- ", Notes);
                if (Errors.Count > 0) text += "\nERRORS:\n- " + string.Join("\n- ", Errors);
                return text;
            }
        }

        public static ImportResult Import(SparkLevelWorkstationBindingBridge bridge)
        {
            var result = new ImportResult();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                result.Errors.Add("Exit Play Mode before importing declarations.");
                return result;
            }
            if (bridge == null)
            {
                result.Errors.Add("Missing workstation bridge.");
                return result;
            }
            SparkLevelLayout layout = bridge.GetComponentInParent<SparkLevelLayout>(true);
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (layout == null || stage == null || stage.prefabContentsRoot == null ||
                layout.gameObject != stage.prefabContentsRoot ||
                !bridge.transform.IsChildOf(layout.transform))
            {
                result.Errors.Add("Open the ORIGINAL level layout prefab in Prefab Mode and select its bridge.");
                return result;
            }

            SparkLevelSceneBindings bindings = ResolveBindings(layout, result);
            if (bindings == null) return result;

            var bridgeSO = new SerializedObject(bridge);
            bridgeSO.Update();
            SerializedProperty useSmart = bridgeSO.FindProperty("useSmartSlots");
            SerializedProperty legacy = bridgeSO.FindProperty("workstations");
            if (useSmart == null || legacy == null || !legacy.isArray)
            {
                result.Errors.Add("Bridge serialized fields useSmartSlots/workstations not found.");
                return result;
            }

            var rules = new List<Rule>();
            var smartSlotIds = new HashSet<string>(StringComparer.Ordinal);
            if (useSmart.boolValue)
            {
                foreach (var slot in layout.GetComponentsInChildren<SparkShelfSlot>(true))
                {
                    if (slot == null || slot.Kind != SparkShelfSlotKind.Workstation || !slot.EnableSmartBinding)
                        continue;
                    string id = Clean(slot.SlotId);
                    if (id.Length == 0)
                    {
                        result.Errors.Add("Smart workstation '" + slot.name + "' has an empty Slot ID.");
                        continue;
                    }
                    if (!smartSlotIds.Add(id))
                    {
                        result.Errors.Add("Duplicate smart workstation Slot ID: " + id);
                        continue;
                    }
                    rules.Add(new Rule {
                        slot=id, component=Clean(slot.RuntimeComponentId),
                        a=Clean(slot.RuntimeTerminalAId), b=Clean(slot.RuntimeTerminalBId),
                        electronic="", origin="Smart Slot " + slot.name
                    });
                    result.SmartRules++;
                }
            }
            for (int i = 0; i < legacy.arraySize; i++)
            {
                var item = legacy.GetArrayElementAtIndex(i);
                string id = Read(item, "slotId");
                if (id.Length == 0)
                {
                    result.Errors.Add("Legacy workstation rule " + i + " has empty Slot ID.");
                    continue;
                }
                // A smart slot owns its mapping and takes priority over legacy.
                if (smartSlotIds.Contains(id)) continue;
                rules.Add(new Rule {
                    slot=id, component=Read(item,"componentBindingId"),
                    a=Read(item,"inputTerminalBindingId"),
                    b=Read(item,"outputTerminalBindingId"),
                    electronic=Read(item,"electronicObjectBindingId"),
                    origin="Legacy rule " + i
                });
                result.LegacyRules++;
            }
            if (rules.Count == 0)
            {
                result.Errors.Add("No rules found. Ensure workstation SparkShelfSlot children have Enable Smart Binding and IDs, or configure legacy rules.");
                return result;
            }

            var usedSlots = new HashSet<string>(StringComparer.Ordinal);
            var usedAliases = new HashSet<string>(StringComparer.Ordinal);
            foreach (Rule r in rules)
            {
                if (!usedSlots.Add(r.slot)) result.Errors.Add("Duplicate Slot ID: " + r.slot);
                if (r.component.Length + r.a.Length + r.b.Length + r.electronic.Length == 0)
                    result.Errors.Add(r.origin + " has no runtime IDs configured.");
                if (r.a.Length != 0 && r.a == r.b)
                    result.Errors.Add(r.origin + " has identical A/B terminal IDs.");
                CheckAlias(usedAliases, "C:", r.component, r.origin, result);
                CheckAlias(usedAliases, "T:", r.a, r.origin, result);
                CheckAlias(usedAliases, "T:", r.b, r.origin, result);
                CheckAlias(usedAliases, "E:", r.electronic, r.origin, result);
            }
            if (!result.Success) return result;

            var destination = new SerializedObject(bindings);
            destination.Update();
            var cp = destination.FindProperty("runtimeComponentIds");
            var tp = destination.FindProperty("runtimeTerminalIds");
            var ep = destination.FindProperty("runtimeElectronicObjectIds");
            var cf = destination.FindProperty("components");
            var tf = destination.FindProperty("terminals");
            var ef = destination.FindProperty("electronicObjects");
            if (!IsStringArray(cp) || !IsStringArray(tp) || !IsStringArray(ep) ||
                cf == null || !cf.isArray || tf == null || !tf.isArray || ef == null || !ef.isArray)
            {
                result.Errors.Add("SparkLevelSceneBindings schema mismatch: expected runtime*Ids and fixed bindings arrays.");
                return result;
            }
            var pendingC = new HashSet<string>(StringComparer.Ordinal);
            var pendingT = new HashSet<string>(StringComparer.Ordinal);
            var pendingE = new HashSet<string>(StringComparer.Ordinal);
            foreach (Rule r in rules)
            {
                ValidateDeclaration(cp, cf, r.component, "component", pendingC, result);
                ValidateDeclaration(tp, tf, r.a, "terminal", pendingT, result);
                ValidateDeclaration(tp, tf, r.b, "terminal", pendingT, result);
                ValidateDeclaration(ep, ef, r.electronic, "electronic object", pendingE, result);
            }
            if (!result.Success) return result; // All-or-nothing; no partial writes.

            if (pendingC.Count + pendingT.Count + pendingE.Count > 0)
            {
                Undo.RecordObject(bindings, "Project Spark: Import Smart Slot Runtime IDs");
                result.AddedComponents = Append(cp, pendingC);
                result.AddedTerminals = Append(tp, pendingT);
                result.AddedElectronics = Append(ep, pendingE);
                destination.ApplyModifiedProperties();
                EditorUtility.SetDirty(bindings);
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
            else
                result.Notes.Add("All discovered workstation IDs were already declared.");
            result.Notes.Add("Smart Slot mode and legacy mappings were not modified.");
            return result;
        }

        public static ImportResult Import(SparkLevelSceneBindings bindings)
        {
            var result = new ImportResult();
            if (bindings == null)
            {
                result.Errors.Add("Missing scene bindings.");
                return result;
            }
            var layout = bindings.GetComponentInParent<SparkLevelLayout>(true);
            if (layout == null)
            {
                result.Errors.Add("Scene bindings is not under a SparkLevelLayout.");
                return result;
            }
            var bridges = layout.GetComponentsInChildren<SparkLevelWorkstationBindingBridge>(true);
            if (bridges.Length != 1)
            {
                result.Errors.Add("Expected exactly one workstation bridge on this level layout; found " + bridges.Length + ".");
                return result;
            }
            return Import(bridges[0]);
        }

        private static SparkLevelSceneBindings ResolveBindings(SparkLevelLayout layout, ImportResult result)
        {
            var so = new SerializedObject(layout);
            var reference = so.FindProperty("sceneBindings");
            var bindings = reference != null ? reference.objectReferenceValue as SparkLevelSceneBindings : null;
            if (bindings == null) bindings = layout.GetComponent<SparkLevelSceneBindings>();
            if (bindings == null)
            {
                var matches = layout.GetComponentsInChildren<SparkLevelSceneBindings>(true);
                if (matches.Length == 1) bindings = matches[0];
                else result.Errors.Add("Assign the PRIMARY Scene Bindings on SparkLevelLayout (found " + matches.Length + " possible bindings).");
            }
            if (bindings != null && bindings.transform != layout.transform &&
                !bindings.transform.IsChildOf(layout.transform))
            {
                result.Errors.Add("Primary scene bindings is outside the level prefab.");
                return null;
            }
            if (bindings == null && result.Errors.Count == 0)
                result.Errors.Add("No primary Scene Bindings found.");
            return bindings;
        }

        private static string Clean(string value) => (value ?? "").Trim();
        private static string Read(SerializedProperty rule, string field)
        {
            var p = rule.FindPropertyRelative(field);
            return p != null && p.propertyType == SerializedPropertyType.String ? Clean(p.stringValue) : "";
        }
        private static void CheckAlias(HashSet<string> aliases, string prefix, string id,
            string origin, ImportResult result)
        {
            if (id.Length != 0 && !aliases.Add(prefix + id))
                result.Errors.Add(origin + " repeats binding ID '" + id + "'.");
        }
        private static bool IsStringArray(SerializedProperty p)
        {
            return p != null && p.isArray && p.propertyType != SerializedPropertyType.String &&
                (p.arraySize == 0 ? p.arrayElementType == "string" :
                 p.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.String);
        }
        private static bool ContainsRuntime(SerializedProperty array, string id)
        {
            for (int i = 0; i < array.arraySize; i++)
                if (string.Equals(array.GetArrayElementAtIndex(i).stringValue, id, StringComparison.Ordinal)) return true;
            return false;
        }
        private static bool ContainsFixed(SerializedProperty array, string id)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                var p = array.GetArrayElementAtIndex(i).FindPropertyRelative("id");
                if (p != null && string.Equals(p.stringValue, id, StringComparison.Ordinal)) return true;
            }
            return false;
        }
        private static void ValidateDeclaration(SerializedProperty runtime, SerializedProperty fixedList,
            string id, string label, HashSet<string> pending, ImportResult result)
        {
            if (id.Length == 0) return;
            if (ContainsFixed(fixedList, id))
            {
                result.Errors.Add("ID '" + id + "' is already a FIXED " + label + " binding; cannot declare as runtime.");
                return;
            }
            if (!ContainsRuntime(runtime, id)) pending.Add(id);
        }
        private static int Append(SerializedProperty array, HashSet<string> items)
        {
            int count = 0;
            foreach (string id in items)
            {
                int index = array.arraySize;
                array.InsertArrayElementAtIndex(index);
                array.GetArrayElementAtIndex(index).stringValue = id;
                count++;
            }
            return count;
        }

        public static void Report(ImportResult result, UnityEngine.Object owner, bool popup)
        {
            if (result.Success) Debug.Log("[Workstation ID Import] SUCCESS\n" + result, owner);
            else Debug.LogError("[Workstation ID Import] BLOCKED\n" + result, owner);
            if (popup)
                EditorUtility.DisplayDialog("Project Spark — Runtime IDs",
                    (result.Success ? "Import completed.\n\n" : "Import blocked; no declarations changed.\n\n") + result,
                    "OK");
        }
    }
}
#endif
