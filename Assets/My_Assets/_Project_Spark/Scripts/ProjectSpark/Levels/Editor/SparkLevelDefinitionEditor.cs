#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Focused, asset-safe authoring inspector. No dependency on scene objects,
    /// electrical solver, or the Level 7/8/9 level-specific definitions.
    /// </summary>
    [CustomEditor(typeof(SparkLevelDefinition))]
    [CanEditMultipleObjects]
    public sealed class SparkLevelDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty levelId, displayName, description;
        private SerializedProperty completionMode, requiredTargetCount;
        private SerializedProperty requireClosedReturn, rejectShortCircuit,
            rejectTargetShort, allowIntermediateConnections;
        private SerializedProperty requireMinimumVoltage, minimumVoltage,
            allowAnyConfiguredPowerSource, powerSources, targets;
        private SerializedProperty failureMode, requiredHealthyResistorIds,
            requireHealthyInstalledResistors, successOutputIds, failureOutputIds;
        private SerializedProperty unlockNextLevelOnCompletion, unlockedByDefault,
            autoUnlockNextLevel, autoAdvanceOnCompletion;

        private ReorderableList sourceList, targetList;
        private bool identityOpen = true, rulesOpen = true, powerOpen = true,
            targetsOpen = true, healthOpen, outputsOpen, progressionOpen = true;
        private GUIStyle headingStyle;

        private void OnEnable()
        {
            levelId = Get("levelId"); displayName = Get("displayName"); description = Get("description");
            completionMode = Get("completionMode"); requiredTargetCount = Get("requiredTargetCount");
            requireClosedReturn = Get("requireClosedReturn"); rejectShortCircuit = Get("rejectShortCircuit");
            rejectTargetShort = Get("rejectTargetShort"); allowIntermediateConnections = Get("allowIntermediateConnections");
            requireMinimumVoltage = Get("requireMinimumVoltage"); minimumVoltage = Get("minimumVoltage");
            allowAnyConfiguredPowerSource = Get("allowAnyConfiguredPowerSource");
            powerSources = Get("powerSources"); targets = Get("targets");
            failureMode = Get("failureMode"); requiredHealthyResistorIds = Get("requiredHealthyResistorIds");
            requireHealthyInstalledResistors = Get("requireHealthyInstalledResistors");
            successOutputIds = Get("successOutputIds"); failureOutputIds = Get("failureOutputIds");
            unlockNextLevelOnCompletion = Get("unlockNextLevelOnCompletion");
            unlockedByDefault = Get("unlockedByDefault"); autoUnlockNextLevel = Get("autoUnlockNextLevel");
            autoAdvanceOnCompletion = Get("autoAdvanceOnCompletion");

            sourceList = CreateList(powerSources, "Configured Power Sources", DrawSource, SourceHeight);
            targetList = CreateList(targets, "Level Objectives", DrawTarget, TargetHeight);
        }

        private SerializedProperty Get(string name) => serializedObject.FindProperty(name);

        private ReorderableList CreateList(SerializedProperty property, string title,
            ReorderableList.ElementCallbackDelegate draw,
            ReorderableList.ElementHeightCallbackDelegate height)
        {
            var list = new ReorderableList(serializedObject, property, true, true, true, true);
            list.drawHeaderCallback = r => EditorGUI.LabelField(r, title + "  (" + property.arraySize + ")", EditorStyles.boldLabel);
            list.drawElementCallback = draw;
            list.elementHeightCallback = height;
            return list;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureStyles();
            DrawBanner();
            if (serializedObject.isEditingMultipleObjects)
                EditorGUILayout.HelpBox("Editing multiple assets. Validation and quick fixes are available only for a single selected asset.", MessageType.Info);

            Section(ref identityOpen, "01   LEVEL IDENTITY", () =>
            {
                Field(levelId, "Level ID");
                Field(displayName, "Display Name");
                Field(description, "Player Mission / Description");
            });
            Section(ref rulesOpen, "02   COMPLETION & SAFETY", () =>
            {
                Field(completionMode, "Completion Mode");
                if (completionMode != null && (completionMode.hasMultipleDifferentValues || completionMode.enumValueIndex == 2))
                    Field(requiredTargetCount, "Required Target Count");
                Field(requireClosedReturn, "Require Closed Return");
                Field(rejectShortCircuit, "Reject Source Short");
                Field(rejectTargetShort, "Reject Target Short");
                Field(allowIntermediateConnections, "Allow Intermediate Connections");
                Field(failureMode, "Failure Mode");
            });
            Section(ref powerOpen, "03   POWER CONFIGURATION", () =>
            {
                Field(requireMinimumVoltage, "Require Minimum Voltage");
                if (requireMinimumVoltage != null && (requireMinimumVoltage.boolValue || requireMinimumVoltage.hasMultipleDifferentValues))
                    Field(minimumVoltage, "Minimum Voltage (V)");
                Field(allowAnyConfiguredPowerSource, "Allow Any Configured Source");
                if (powerSources != null) sourceList.DoLayoutList();
                else Missing("powerSources");
            });
            Section(ref targetsOpen, "04   OBJECTIVES / TARGETS", () =>
            {
                if (targets != null) targetList.DoLayoutList();
                else Missing("targets");
                EditorGUILayout.HelpBox("Electrical IDs must match SparkLevelSceneBindings exactly. Target references are asset data, not scene object references.", MessageType.None);
            });
            Section(ref healthOpen, "05   FAULT / RESISTOR HEALTH", () =>
            {
                Field(requireHealthyInstalledResistors, "Require Healthy Installed Resistors");
                Field(requiredHealthyResistorIds, "Specific Healthy Resistor IDs", true);
            });
            Section(ref outputsOpen, "06   SCENE OUTPUT IDS", () =>
            {
                Field(successOutputIds, "On Success", true);
                Field(failureOutputIds, "On Failure", true);
            });
            Section(ref progressionOpen, "07   PROGRESSION", () =>
            {
                Field(unlockedByDefault, "Unlocked By Default");
                Field(unlockNextLevelOnCompletion, "Unlock Next Level On Completion");
                Field(autoUnlockNextLevel, "Auto Unlock Next Level");
                Field(autoAdvanceOnCompletion, "Auto Advance On Completion");
            });

            serializedObject.ApplyModifiedProperties();
            if (!serializedObject.isEditingMultipleObjects)
            {
                DrawAudit();
            }
        }

        private void EnsureStyles()
        {
            if (headingStyle != null) return;
            headingStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13, alignment = TextAnchor.MiddleLeft,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.52f, .88f, .98f) : new Color(.08f, .35f, .56f) }
            };
        }

        private void DrawBanner()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PROJECT SPARK   /   LEVEL AUTHORING", headingStyle);
            string id = levelId != null && !levelId.hasMultipleDifferentValues ? levelId.stringValue : "Multiple levels";
            EditorGUILayout.LabelField(id + "     •     " + (targets != null ? targets.arraySize : 0) + " objectives     •     " +
                (powerSources != null ? powerSources.arraySize : 0) + " sources", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void Section(ref bool isOpen, string label, Action body)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isOpen = EditorGUILayout.Foldout(isOpen, label, true, EditorStyles.foldoutHeader);
            if (isOpen)
            {
                EditorGUILayout.Space(3);
                EditorGUI.indentLevel++;
                body();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(3);
            }
            EditorGUILayout.EndVertical();
        }

        private static void Field(SerializedProperty property, string label, bool includeChildren = false)
        {
            if (property == null) { Missing(label); return; }
            EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
        }
        private static void Missing(string name) => EditorGUILayout.HelpBox("Serialized field not found: " + name, MessageType.Warning);
        private static SerializedProperty Child(SerializedProperty p, string name) => p == null ? null : p.FindPropertyRelative(name);

        private float SourceHeight(int index)
        {
            if (powerSources == null || index >= powerSources.arraySize) return 25;
            var element = powerSources.GetArrayElementAtIndex(index);
            return EditorGUIUtility.singleLineHeight * (element.isExpanded ? 5 : 1) + 12;
        }
        private void DrawSource(Rect rect, int index, bool active, bool focused)
        {
            if (powerSources == null || index >= powerSources.arraySize) return;
            var element = powerSources.GetArrayElementAtIndex(index);
            float h = EditorGUIUtility.singleLineHeight + 3;
            rect.y += 2;
            var name = Child(element, "sourceName");
            string caption = "Source " + (index + 1) + (name != null ? "  •  " + name.stringValue : "");
            element.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, h), element.isExpanded, caption, true);
            if (!element.isExpanded) return;
            EditorGUI.indentLevel++;
            DrawRow(ref rect, h, Child(element, "sourceName"), "Source Name");
            DrawRow(ref rect, h, Child(element, "positiveTerminalId"), "Positive Terminal ID");
            DrawRow(ref rect, h, Child(element, "negativeTerminalId"), "Negative Terminal ID");
            DrawRow(ref rect, h, Child(element, "nominalVoltage"), "Nominal Voltage (V)");
            EditorGUI.indentLevel--;
        }
        private static void DrawRow(ref Rect r, float h, SerializedProperty property, string label)
        {
            r.y += h;
            if (property != null) EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), property, new GUIContent(label));
        }

        private float TargetHeight(int index)
        {
            if (targets == null || index >= targets.arraySize) return 25;
            SerializedProperty element = targets.GetArrayElementAtIndex(index);
            if (!element.isExpanded) return EditorGUIUtility.singleLineHeight + 9;
            float height = EditorGUIUtility.singleLineHeight + 10;
            // Iterate the serialized target properties without depending on SparkLevelTarget's private field implementation.
            var iterator = element.Copy();
            var end = iterator.GetEndProperty();
            bool enter = true;
            while (iterator.NextVisible(enter) && !SerializedProperty.EqualContents(iterator, end))
            {
                if (iterator.depth == element.depth + 1)
                    height += EditorGUI.GetPropertyHeight(iterator, false) + 3;
                enter = false;
            }
            return height + 6;
        }
        private void DrawTarget(Rect rect, int index, bool active, bool focused)
        {
            if (targets == null || index >= targets.arraySize) return;
            var element = targets.GetArrayElementAtIndex(index);
            float h = EditorGUIUtility.singleLineHeight;
            rect.y += 2;
            var display = Child(element, "displayName");
            var id = Child(element, "targetId");
            string label = "Objective " + (index + 1) + "  •  " +
                (display != null && !string.IsNullOrWhiteSpace(display.stringValue) ? display.stringValue :
                (id != null ? id.stringValue : "Configure target"));
            element.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, h), element.isExpanded, label, true);
            if (!element.isExpanded) return;
            var iterator = element.Copy();
            var end = iterator.GetEndProperty();
            bool enter = true;
            EditorGUI.indentLevel++;
            while (iterator.NextVisible(enter) && !SerializedProperty.EqualContents(iterator, end))
            {
                if (iterator.depth == element.depth + 1)
                {
                    float lineHeight = EditorGUI.GetPropertyHeight(iterator, false);
                    rect.y += h + 3;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, lineHeight), iterator, true);
                    h = lineHeight;
                }
                enter = false;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawAudit()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("CONFIGURATION AUDIT", EditorStyles.boldLabel);
            SparkLevelDefinition level = (SparkLevelDefinition)target;
            var issues = new List<string>();
            if (string.IsNullOrWhiteSpace(level.LevelId)) issues.Add("Level ID is empty.");
            if (string.IsNullOrWhiteSpace(level.DisplayName)) issues.Add("Display Name is empty.");
            if (level.SourceCount == 0) issues.Add("No power sources configured.");
            if (level.TargetCount == 0) issues.Add("No objectives configured.");
            if (level.CompletionRule == SparkLevelDefinition.CompletionMode.RequiredTargetCount &&
                level.RequiredTargetCount > level.TargetCount)
                issues.Add("Required target count exceeds number of objectives.");

            if (targets != null)
            {
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < targets.arraySize; i++)
                {
                    var p = targets.GetArrayElementAtIndex(i);
                    var id = Child(p, "targetId");
                    if (id != null && !string.IsNullOrWhiteSpace(id.stringValue) && !seen.Add(id.stringValue.Trim()))
                        issues.Add("Duplicate objective ID: " + id.stringValue);
                    var maxFlag = Child(p, "useMaximumCurrent");
                    var maxValue = Child(p, "maximumCurrent");
                    if (maxFlag != null && maxValue != null && maxFlag.boolValue && maxValue.floatValue <= 0f)
                        issues.Add("Objective " + (i + 1) + ": Maximum Current enabled with 0 A (may prevent completion).");
                    var minV = Child(p, "minimumVoltage");
                    var maxV = Child(p, "maximumVoltage");
                    var useV = Child(p, "useMaximumVoltage");
                    if (minV != null && maxV != null && useV != null && useV.boolValue && maxV.floatValue < minV.floatValue)
                        issues.Add("Objective " + (i + 1) + ": Maximum Voltage is below Minimum Voltage.");
                }
            }
            if (issues.Count == 0) EditorGUILayout.HelpBox("No obvious serialized configuration mistakes detected. Electrical IDs and actual circuit measurements still require runtime verification.", MessageType.Info);
            else foreach (string issue in issues) EditorGUILayout.HelpBox(issue, MessageType.Warning);

            if (GUILayout.Button("Run Asset Validate()"))
            {
                if (level.Validate(out string error)) Debug.Log("[Spark Level Editor] " + level.LevelId + ": Validation passed.", level);
                else Debug.LogWarning("[Spark Level Editor] " + level.LevelId + ": " + error, level);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
