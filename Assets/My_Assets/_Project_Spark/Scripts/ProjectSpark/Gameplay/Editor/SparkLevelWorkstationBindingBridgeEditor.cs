#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.Gameplay.Editor
{
    /// <summary>
    /// Edit-mode authoring for per-level workstation runtime IDs.
    /// Never binds a physical shelf item and never modifies level prefabs during Play Mode.
    /// Compatible with an independent SparkLevelSceneBindings custom Inspector.
    /// </summary>
    [CustomEditor(typeof(SparkLevelWorkstationBindingBridge))]
    public sealed class SparkLevelWorkstationBindingBridgeEditor : UnityEditor.Editor
    {
        private const string ComponentArray = "runtimeComponentIds";
        private const string TerminalArray = "runtimeTerminalIds";
        private const string ElectronicArray = "runtimeElectronicObjectIds";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            bool changed = EditorGUI.EndChangeCheck();
            bool applied = serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(7f);
            EditorGUILayout.HelpBox(
                "Configure this bridge on the ORIGINAL level-layout prefab. " +
                "Runtime IDs are declared in its PRIMARY SparkLevelSceneBindings, " +
                "not in the persistent Component_Shelf bindings. " +
                "Shelf objects are resolved automatically during Play Mode.",
                MessageType.Info);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Play Mode: declarations are read-only. Changes to a runtime clone " +
                    "will not be saved to the original level prefab.",
                    MessageType.Warning);
                if (GUILayout.Button("SYNCHRONIZE + REPORT NOW", GUILayout.Height(29f)))
                    ((SparkLevelWorkstationBindingBridge)target).SynchronizeNow();
                return;
            }

            // Only modify serialized binding declarations when a user actually edits a rule.
            // Never write on ordinary Inspector repaint, preventing dirty/Undo spam.
            if (changed && applied)
                DeclareIds(false);

            if (GUILayout.Button("DECLARE / CHECK RUNTIME IDS IN PRIMARY BINDINGS", GUILayout.Height(32f)))
                DeclareIds(true);

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField("Mode", "Edit Mode — prefab declarations");
        }

        private void DeclareIds(bool showReport)
        {
            if (Application.isPlaying) return;

            var bridge = (SparkLevelWorkstationBindingBridge)target;
            var issues = new List<string>();
            SparkLevelSceneBindings bindings = FindPrimaryBindings(bridge, issues);
            if (bindings == null)
            {
                Report(showReport, bridge, 0, issues);
                return;
            }

            var source = new SerializedObject(bridge);
            source.Update();
            SerializedProperty rules = source.FindProperty("workstations");
            if (rules == null)
            {
                issues.Add("Cannot locate 'workstations' serialized array on the bridge.");
                Report(showReport, bridge, 0, issues);
                return;
            }

            var destination = new SerializedObject(bindings);
            destination.Update();

            // Validate all owners before altering serialized data. Different slots must
            // not claim the same logical binding ID in the same category.
            var owners = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < rules.arraySize; i++)
            {
                SerializedProperty rule = rules.GetArrayElementAtIndex(i);
                string slot = GetString(rule, "slotId").Trim();
                if (string.IsNullOrEmpty(slot))
                {
                    issues.Add("Rule " + i + ": Slot Id is empty.");
                    continue;
                }
                Claim(owners, "C:", GetString(rule, "componentBindingId"), i, issues);
                Claim(owners, "T:", GetString(rule, "inputTerminalBindingId"), i, issues);
                Claim(owners, "T:", GetString(rule, "outputTerminalBindingId"), i, issues);
                Claim(owners, "E:", GetString(rule, "electronicObjectBindingId"), i, issues);
            }
            if (issues.Count > 0)
            {
                Report(showReport, bridge, 0, issues);
                return;
            }

            int added = 0;
            for (int i = 0; i < rules.arraySize; i++)
            {
                SerializedProperty rule = rules.GetArrayElementAtIndex(i);
                Add(destination, ComponentArray, "components", GetString(rule, "componentBindingId"), issues, ref added);
                Add(destination, TerminalArray, "terminals", GetString(rule, "inputTerminalBindingId"), issues, ref added);
                Add(destination, TerminalArray, "terminals", GetString(rule, "outputTerminalBindingId"), issues, ref added);
                Add(destination, ElectronicArray, "electronicObjects", GetString(rule, "electronicObjectBindingId"), issues, ref added);
            }

            // SerializedObject.ApplyModifiedProperties supplies Undo recording.
            // No new prefab/scene copy and no forced prefab save.
            bool applied = destination.ApplyModifiedProperties();
            if (applied)
            {
                EditorUtility.SetDirty(bindings);
                if (PrefabUtility.IsPartOfPrefabInstance(bindings))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(bindings);
                if (bindings.gameObject.scene.IsValid() && bindings.gameObject.scene.isLoaded)
                    EditorSceneManager.MarkSceneDirty(bindings.gameObject.scene);
            }
            Report(showReport, bridge, added, issues);
        }

        private static SparkLevelSceneBindings FindPrimaryBindings(
            SparkLevelWorkstationBindingBridge bridge, List<string> issues)
        {
            SparkLevelLayout layout = bridge.GetComponentInParent<SparkLevelLayout>(true);
            if (layout == null)
            {
                issues.Add("Bridge must be on or below the ORIGINAL SparkLevelLayout prefab root.");
                return null;
            }

            // Read authored reference without relying on runtime discovery/cache.
            var layoutData = new SerializedObject(layout);
            layoutData.Update();
            SerializedProperty primary = layoutData.FindProperty("sceneBindings");
            SparkLevelSceneBindings bindings = primary != null
                ? primary.objectReferenceValue as SparkLevelSceneBindings
                : null;

            // Handle layouts where the component exists but the primary field
            // has not yet been assigned, without selecting a scene-wide binding.
            if (bindings == null)
                bindings = layout.GetComponent<SparkLevelSceneBindings>();
            if (bindings == null)
            {
                var found = layout.GetComponentsInChildren<SparkLevelSceneBindings>(true);
                if (found.Length == 1) bindings = found[0];
                else if (found.Length > 1)
                    issues.Add("Multiple owned SceneBindings found. Assign the primary 'Scene Bindings' reference on SparkLevelLayout.");
            }

            if (bindings == null)
            {
                issues.Add("No primary SparkLevelSceneBindings on this level prefab. Add/assign it first.");
                return null;
            }
            if (bindings.transform != layout.transform && !bindings.transform.IsChildOf(layout.transform))
            {
                issues.Add("The primary SceneBindings is outside this level prefab. Choose a layout-owned binding.");
                return null;
            }
            return bindings;
        }

        private static void Claim(Dictionary<string, int> owners, string prefix,
            string id, int ruleIndex, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            string key = prefix + id.Trim();
            if (owners.TryGetValue(key, out int previous))
                issues.Add("Binding '" + id + "' is assigned more than once (rules " + previous + " and " + ruleIndex + ").");
            else owners.Add(key, ruleIndex);
        }

        private static string GetString(SerializedProperty parent, string name)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            return property != null && property.propertyType == SerializedPropertyType.String
                ? property.stringValue : string.Empty;
        }

        private static void Add(SerializedObject destination, string runtimeField,
            string fixedField, string id, List<string> issues, ref int added)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            id = id.Trim();
            SerializedProperty runtime = destination.FindProperty(runtimeField);
            SerializedProperty fixedArray = destination.FindProperty(fixedField);
            if (runtime == null || fixedArray == null || !runtime.isArray || !fixedArray.isArray)
            {
                issues.Add("Missing '" + runtimeField + "' or '" + fixedField + "'. Install the Universal SceneBindings script.");
                return;
            }

            for (int i = 0; i < fixedArray.arraySize; i++)
            {
                if (string.Equals(GetString(fixedArray.GetArrayElementAtIndex(i), "id"), id, StringComparison.Ordinal))
                {
                    issues.Add("'" + id + "' exists as a FIXED binding. Remove/migrate the conflicting fixed entry first.");
                    return;
                }
            }
            for (int i = 0; i < runtime.arraySize; i++)
                if (string.Equals(runtime.GetArrayElementAtIndex(i).stringValue, id, StringComparison.Ordinal))
                    return;

            int next = runtime.arraySize;
            runtime.InsertArrayElementAtIndex(next);
            runtime.GetArrayElementAtIndex(next).stringValue = id;
            added++;
        }

        private static void Report(bool showReport, UnityEngine.Object context, int added, List<string> issues)
        {
            if (issues.Count > 0)
            {
                string message = string.Join("\n", issues);
                Debug.LogWarning("[Workstation ID Authoring] " + message, context);
                if (showReport)
                    EditorUtility.DisplayDialog("Workstation Runtime IDs", "Added " + added + " ID(s).\n\n" + message, "OK");
            }
            else if (showReport)
            {
                EditorUtility.DisplayDialog("Workstation Runtime IDs",
                    "Configuration checked. Added " + added + " missing ID(s).\n\n" +
                    "Save the ORIGINAL level prefab. Runtime binding is automatic in Play Mode.", "OK");
            }
        }
    }
}
#endif
