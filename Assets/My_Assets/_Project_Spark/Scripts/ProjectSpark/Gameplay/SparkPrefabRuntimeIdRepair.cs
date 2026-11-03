#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Writes workstation runtime ID declarations into the ORIGINAL prefab asset.
    /// Independent from the workstation bridge's custom Inspector.
    /// Does not alter runtime objects, fixed references, or electrical behaviour.
    /// </summary>
    public static class SparkPrefabRuntimeIdRepair
    {
        private const string MenuPath = "Tools/Project Spark/Bindings/Declare Workstation IDs In Selected Prefab";

        [MenuItem(MenuPath, true)]
        private static bool ValidateMenu()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode &&
                   GetSelectedPrefabPath() != null;
        }

        [MenuItem(MenuPath)]
        private static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Spark Binding Repair] Exit Play Mode first.");
                return;
            }

            string path = GetSelectedPrefabPath();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[Spark Binding Repair] Select the original level layout prefab ASSET in the Project window.");
                return;
            }

            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) throw new InvalidOperationException("Could not open prefab contents.");

                SparkLevelLayout layout = root.GetComponentInChildren<SparkLevelLayout>(true);
                if (layout == null)
                    throw new InvalidOperationException("Prefab contains no SparkLevelLayout.");

                SparkLevelSceneBindings bindings = FindPrimary(layout);
                if (bindings == null)
                    throw new InvalidOperationException("No unambiguous layout-owned primary SparkLevelSceneBindings found.");

                SparkLevelWorkstationBindingBridge[] bridges =
                    layout.GetComponentsInChildren<SparkLevelWorkstationBindingBridge>(true);
                if (bridges.Length == 0)
                    throw new InvalidOperationException("No SparkLevelWorkstationBindingBridge found in this level prefab.");

                SerializedObject destination = new SerializedObject(bindings);
                destination.Update();

                // Collect first, validate all, then write: no partial changes on conflict.
                var terminals = new HashSet<string>(StringComparer.Ordinal);
                var components = new HashSet<string>(StringComparer.Ordinal);
                var electronics = new HashSet<string>(StringComparer.Ordinal);
                var owned = new HashSet<string>(StringComparer.Ordinal);
                int ruleCount = 0;
                foreach (var bridge in bridges)
                {
                    SerializedObject bridgeData = new SerializedObject(bridge);
                    bridgeData.Update();
                    SerializedProperty rules = bridgeData.FindProperty("workstations");
                    if (rules == null || !rules.isArray)
                        throw new InvalidOperationException("Bridge has no serialized 'workstations' array.");
                    for (int i = 0; i < rules.arraySize; i++)
                    {
                        var rule = rules.GetArrayElementAtIndex(i);
                        string slot = GetId(rule, "slotId");
                        string component = GetId(rule, "componentBindingId");
                        string input = GetId(rule, "inputTerminalBindingId");
                        string output = GetId(rule, "outputTerminalBindingId");
                        string electronic = GetId(rule, "electronicObjectBindingId");
                        if (component.Length + input.Length + output.Length + electronic.Length == 0)
                            continue;
                        if (slot.Length == 0)
                            throw new InvalidOperationException("A workstation rule with binding IDs has an empty Slot Id.");
                        CheckOwner(owned, "C:", component);
                        CheckOwner(owned, "T:", input);
                        CheckOwner(owned, "T:", output);
                        CheckOwner(owned, "E:", electronic);
                        AddIfSet(components, component);
                        AddIfSet(terminals, input);
                        AddIfSet(terminals, output);
                        AddIfSet(electronics, electronic);
                        ruleCount++;
                    }
                }

                CheckNoFixedConflict(destination, "components", components);
                CheckNoFixedConflict(destination, "terminals", terminals);
                CheckNoFixedConflict(destination, "electronicObjects", electronics);

                int added = 0;
                added += AppendMissing(destination, "runtimeComponentIds", components);
                added += AppendMissing(destination, "runtimeTerminalIds", terminals);
                added += AppendMissing(destination, "runtimeElectronicObjectIds", electronics);
                destination.ApplyModifiedPropertiesWithoutUndo();

                if (!bindings.Validate(out string error))
                    throw new InvalidOperationException("SceneBindings.Validate failed: " + error);

                if (added > 0)
                {
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                    if (saved == null)
                        throw new InvalidOperationException("PrefabUtility.SaveAsPrefabAsset returned null.");
                    AssetDatabase.SaveAssets();
                }

                string summary = "Prefab: " + path + "\nRules checked: " + ruleCount +
                                 "\nNew IDs: " + added + "\nComponent IDs: " + Join(components) +
                                 "\nTerminal IDs: " + Join(terminals) +
                                 "\nElectronic IDs: " + Join(electronics) +
                                 "\n\n" + (added > 0 ? "Saved to original prefab asset." : "All IDs already exist; no changes required.");
                Debug.Log("[Spark Binding Repair] SUCCESS\n" + summary);
                EditorUtility.DisplayDialog("Project Spark — Binding IDs", summary, "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("[Spark Binding Repair] FAILED: " + e.Message);
                EditorUtility.DisplayDialog("Project Spark — Binding Repair Failed", e.Message, "OK");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static SparkLevelSceneBindings FindPrimary(SparkLevelLayout layout)
        {
            var data = new SerializedObject(layout);
            data.Update();
            var field = data.FindProperty("sceneBindings");
            var candidate = field != null ? field.objectReferenceValue as SparkLevelSceneBindings : null;
            if (candidate != null && candidate.transform.IsChildOf(layout.transform)) return candidate;
            candidate = layout.GetComponent<SparkLevelSceneBindings>();
            if (candidate != null) return candidate;
            var all = layout.GetComponentsInChildren<SparkLevelSceneBindings>(true);
            return all.Length == 1 ? all[0] : null;
        }

        private static string GetSelectedPrefabPath()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null) return null;
            string path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(path)) return null;
            return path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ? path : null;
        }

        private static string GetId(SerializedProperty parent, string name)
        {
            SerializedProperty p = parent.FindPropertyRelative(name);
            return p != null && p.propertyType == SerializedPropertyType.String
                ? (p.stringValue ?? string.Empty).Trim() : string.Empty;
        }

        private static void CheckOwner(HashSet<string> owners, string kind, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!owners.Add(kind + id))
                throw new InvalidOperationException("Two workstation entries use the same alias '" + id + "' in category " + kind);
        }

        private static void AddIfSet(HashSet<string> target, string id)
        {
            if (!string.IsNullOrEmpty(id)) target.Add(id);
        }

        private static void CheckNoFixedConflict(SerializedObject data, string field, HashSet<string> ids)
        {
            SerializedProperty array = data.FindProperty(field);
            if (array == null || !array.isArray)
                throw new InvalidOperationException("Missing fixed binding array '" + field + "'.");
            for (int i = 0; i < array.arraySize; i++)
            {
                var idField = array.GetArrayElementAtIndex(i).FindPropertyRelative("id");
                if (idField != null && ids.Contains(idField.stringValue))
                    throw new InvalidOperationException("Alias '" + idField.stringValue +
                        "' already exists as a FIXED " + field + " binding. Migrate it before declaring a runtime ID.");
            }
        }

        private static int AppendMissing(SerializedObject data, string field, HashSet<string> wanted)
        {
            SerializedProperty array = data.FindProperty(field);
            if (array == null || !array.isArray)
                throw new InvalidOperationException("Missing runtime array '" + field + "'. Verify the installed SparkLevelSceneBindings version.");
            var existing = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < array.arraySize; i++)
                existing.Add(array.GetArrayElementAtIndex(i).stringValue);
            int count = 0;
            foreach (string id in wanted)
            {
                if (!existing.Add(id)) continue;
                int index = array.arraySize;
                array.InsertArrayElementAtIndex(index);
                array.GetArrayElementAtIndex(index).stringValue = id;
                count++;
            }
            return count;
        }

        private static string Join(HashSet<string> values)
        {
            return values.Count == 0 ? "(none)" : string.Join(", ", new List<string>(values).ToArray());
        }
    }
}
#endif
