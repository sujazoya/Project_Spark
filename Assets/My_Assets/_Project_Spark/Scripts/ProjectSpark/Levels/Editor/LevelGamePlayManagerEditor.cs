#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Gameplay.Editor
{
    [CustomEditor(typeof(LevelGamePlayManager))]
    public sealed class LevelGamePlayManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty circuitSystem;
        private SerializedProperty electricalSolver;
        private SerializedProperty levels;
        private SerializedProperty activeLevelIndex;

        private SerializedProperty runtimeState;
        private SerializedProperty autoStartFirstLevel;
        private SerializedProperty autoEvaluate;
        private SerializedProperty autoAdvanceOnCompletion;
        private SerializedProperty preventInteractionAfterCompletion;

        private SerializedProperty debugLogging;
        private SerializedProperty verboseLogging;
        private SerializedProperty showRuntimeState;

        private bool showSystems = true;
        private bool showLevels = true;
        private bool showRuntime = true;
        private bool showDiagnostics = false;
        private bool showValidation = true;

        private bool[] levelFoldouts;

        private void OnEnable()
        {
            circuitSystem =
                serializedObject.FindProperty("circuitSystem");

            electricalSolver =
                serializedObject.FindProperty("electricalSolver");

            levels =
                serializedObject.FindProperty("levels");

            activeLevelIndex =
                serializedObject.FindProperty("activeLevelIndex");

            runtimeState =
                serializedObject.FindProperty("runtimeState");

            autoStartFirstLevel =
                serializedObject.FindProperty("autoStartFirstLevel");

            autoEvaluate =
                serializedObject.FindProperty("autoEvaluate");

            autoAdvanceOnCompletion =
                serializedObject.FindProperty(
                    "autoAdvanceOnCompletion");

            preventInteractionAfterCompletion =
                serializedObject.FindProperty(
                    "preventInteractionAfterCompletion");

            debugLogging =
                serializedObject.FindProperty("debugLogging");

            verboseLogging =
                serializedObject.FindProperty("verboseLogging");

            showRuntimeState =
                serializedObject.FindProperty("showRuntimeState");

            SyncFoldouts();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SyncFoldouts();

            LevelGamePlayManager manager =
                (LevelGamePlayManager)target;

            DrawHeader(manager);

            EditorGUILayout.Space(5);

            DrawSystems();

            EditorGUILayout.Space(5);

            DrawLevels(manager);

            EditorGUILayout.Space(5);

            DrawRuntime(manager);

            EditorGUILayout.Space(5);

            DrawDiagnostics(manager);

            EditorGUILayout.Space(5);

            DrawValidation(manager);

            serializedObject.ApplyModifiedProperties();
        }

        // ============================================================
        // SAFE SECTION UI
        // ============================================================
        //
        // Do not use BeginFoldoutHeaderGroup here. Unity's IMGUI layout
        // stack can become unbalanced when nested/custom property drawers
        // are involved. A normal Foldout has no header-group stack state.
        // ============================================================

        private bool DrawSectionHeader(
            string title,
            bool expanded)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool result = EditorGUILayout.Foldout(
                expanded,
                title,
                true,
                EditorStyles.foldoutHeader);

            EditorGUILayout.EndHorizontal();

            return result;
        }

        private void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // HEADER
        // ============================================================

        private void DrawHeader(
            LevelGamePlayManager manager)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "PROJECT SPARK",
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "LEVEL GAMEPLAY MANAGER",
                EditorStyles.largeLabel);

            EditorGUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Active Level",
                GUILayout.Width(90));

            EditorGUILayout.LabelField(
                GetActiveLevelName(manager));

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(
                $"{manager.ActiveLevelIndex + 1} / " +
                $"{manager.LevelCount}",
                GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private string GetActiveLevelName(
            LevelGamePlayManager manager)
        {
            if (manager.ActiveLevel == null)
                return "NONE";

            return manager.ActiveLevel.DisplayName;
        }

        // ============================================================
        // SYSTEMS
        // ============================================================

        private void DrawSystems()
        {
            showSystems = DrawSectionHeader(
                "SIMULATION SYSTEMS",
                showSystems);

            if (showSystems)
            {
                EditorGUILayout.PropertyField(
                    circuitSystem,
                    new GUIContent(
                        "Circuit System"));

                EditorGUILayout.PropertyField(
                    electricalSolver,
                    new GUIContent(
                        "Electrical Solver"));

                if (circuitSystem.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "Circuit System is required.",
                        MessageType.Error);
                }

                if (electricalSolver.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "Electrical Solver is not assigned.",
                        MessageType.Warning);
                }
            }

            EndSection();
        }

        // ============================================================
        // LEVELS
        // ============================================================

     
        private void DrawLevels(
    LevelGamePlayManager manager)
{
    showLevels = DrawSectionHeader(
        "LEVELS",
        showLevels);

    if (showLevels)
    {
        DrawLevelControls(manager);

        EditorGUILayout.Space(5);

        DrawLevelArray(manager);
    }

    EndSection();
}



        // ============================================================
        // LEVEL CONTROLS
        // ============================================================

        private void DrawLevelControls(
            LevelGamePlayManager manager)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                "＋ Add Level",
                GUILayout.Height(26)))
            {
                AddLevel();
            }

            GUI.enabled =
                levels.arraySize > 0;

            if (GUILayout.Button(
                "Duplicate Active",
                GUILayout.Height(26)))
            {
                DuplicateActiveLevel(manager);
            }

            if (GUILayout.Button(
                "Remove Active",
                GUILayout.Height(26)))
            {
                RemoveActiveLevel(manager);
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled =
                levels.arraySize > 0;

            if (GUILayout.Button("◀"))
            {
                ChangeActiveLevel(
                    manager.ActiveLevelIndex - 1);
            }

            EditorGUILayout.PropertyField(
                activeLevelIndex,
                GUIContent.none);

            if (GUILayout.Button("▶"))
            {
                ChangeActiveLevel(
                    manager.ActiveLevelIndex + 1);
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled =
                Application.isPlaying &&
                manager.ActiveLevel != null;

            if (GUILayout.Button("START"))
            {
                manager.StartLevel(
                    manager.ActiveLevelIndex);
            }

            if (GUILayout.Button("EVALUATE"))
            {
                manager.EvaluateNow();
            }

            if (GUILayout.Button("RESET"))
            {
                manager.ResetLevel();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying &&
                manager.ActiveLevel != null)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("PREVIOUS"))
                    manager.PreviousLevel();

                if (GUILayout.Button("NEXT"))
                    manager.NextLevel();

                if (GUILayout.Button("COMPLETE"))
                    manager.CompleteCurrentLevel();

                if (GUILayout.Button("FAIL"))
                    manager.FailCurrentLevel(
                        "Manual failure from editor.");

                EditorGUILayout.EndHorizontal();
            }
        }

        // ============================================================
        // LEVEL ARRAY
        // ============================================================

        private void DrawLevelArray(
            LevelGamePlayManager manager)
        {
            if (levels.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No levels exist. Click Add Level.",
                    MessageType.Info);

                return;
            }

            for (int i = 0;
                 i < levels.arraySize;
                 i++)
            {
                DrawSingleLevel(
                    manager,
                    i,
                    levels.GetArrayElementAtIndex(i));
            }
        }

        // ============================================================
        // SINGLE LEVEL
        // ============================================================

      
private void DrawSingleLevel(
    LevelGamePlayManager manager,
    int index,
    SerializedProperty level)
{
    if (level == null)
        return;

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    string title =
        $"LEVEL {index + 1}";

    if (levelDefinition != null)
    {
        if (!string.IsNullOrWhiteSpace(
                levelDefinition.LevelId))
        {
            title +=
                $"  [{levelDefinition.LevelId}]";
        }

        if (!string.IsNullOrWhiteSpace(
                levelDefinition.DisplayName))
        {
            title +=
                $"  {levelDefinition.DisplayName}";
        }
    }
    else
    {
        title += "  [MISSING LEVEL]";
    }

    bool active =
        index == manager.ActiveLevelIndex;

    if (active)
        title += "  ★ ACTIVE";

    EditorGUILayout.BeginVertical(
        EditorStyles.helpBox);

    levelFoldouts[index] =
        EditorGUILayout.Foldout(
            levelFoldouts[index],
            title,
            true);

    if (levelFoldouts[index])
    {
        EditorGUILayout.Space(3);

        DrawLevelIdentity(level);

        EditorGUILayout.Space(4);

        DrawLevelRules(level);

        EditorGUILayout.Space(4);

        DrawPowerConfiguration(level);

        EditorGUILayout.Space(4);

        DrawTargets(level);

        EditorGUILayout.Space(4);

        DrawFailureConfiguration(level);

        EditorGUILayout.Space(4);

        DrawOutputs(level);

        EditorGUILayout.Space(4);

        DrawProgression(level);

        EditorGUILayout.Space(4);

        DrawLevelValidation(level);
    }

    EditorGUILayout.EndVertical();

    EditorGUILayout.Space(3);
}

        // ============================================================
        // IDENTITY
        // ============================================================

private void DrawLevelIdentity(
    SerializedProperty level)
{
    EditorGUILayout.LabelField(
        "IDENTITY",
        EditorStyles.boldLabel);

    if (level == null)
    {
        EditorGUILayout.HelpBox(
            "Level reference is missing.",
            MessageType.Warning);

        return;
    }

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    if (levelDefinition == null)
    {
        EditorGUILayout.HelpBox(
            "Assign a SparkLevelDefinition asset.",
            MessageType.Warning);

        EditorGUILayout.PropertyField(
            level,
            new GUIContent("Level Definition"));

        return;
    }

    // The LevelGamePlayManager stores a reference to the
    // ScriptableObject. Its internal fields are serialized
    // by the SparkLevelDefinition asset itself.

    EditorGUILayout.PropertyField(
        level,
        new GUIContent("Level Definition"));

    EditorGUILayout.Space(4);

    using (new EditorGUI.DisabledScope(true))
    {
        EditorGUILayout.TextField(
            "Level ID",
            levelDefinition.LevelId);

        EditorGUILayout.TextField(
            "Display Name",
            levelDefinition.DisplayName);

        EditorGUILayout.TextField(
            "Description",
            levelDefinition.Description);
    }
}



        // ============================================================
        // RULES
        // ============================================================

private void DrawLevelRules(
    SerializedProperty level)
{
    EditorGUILayout.LabelField(
        "COMPLETION RULES",
        EditorStyles.boldLabel);

    if (level == null)
    {
        EditorGUILayout.HelpBox(
            "Level reference is missing.",
            MessageType.Warning);

        return;
    }

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    if (levelDefinition == null)
    {
        EditorGUILayout.HelpBox(
            "Assign a SparkLevelDefinition asset before editing level rules.",
            MessageType.Warning);

        return;
    }

    /*
     * SparkLevelDefinition is a ScriptableObject.
     *
     * The LevelGamePlayManager stores only the asset reference.
     * Therefore FindPropertyRelative() cannot be used here.
     *
     * The actual level-rule fields are edited on the
     * SparkLevelDefinition asset itself.
     */

    EditorGUILayout.HelpBox(
        "Level rules are configured in the assigned SparkLevelDefinition asset.",
        MessageType.Info);

    using (new EditorGUI.DisabledScope(true))
    {
        EditorGUILayout.EnumPopup(
            new GUIContent("Completion Mode"),
            levelDefinition.CompletionModeValue);

        EditorGUILayout.IntField(
            new GUIContent("Required Target Count"),
            levelDefinition.RequiredTargetCount);

        EditorGUILayout.FloatField(
            new GUIContent("Minimum Voltage"),
            levelDefinition.MinimumVoltage);

        EditorGUILayout.Toggle(
            new GUIContent("Require Closed Return"),
            levelDefinition.RequireClosedReturn);

        EditorGUILayout.Toggle(
            new GUIContent("Reject Short Circuit"),
            levelDefinition.RejectShortCircuit);

        EditorGUILayout.Toggle(
            new GUIContent("Reject Target Short"),
            levelDefinition.RejectTargetShort);

        EditorGUILayout.Toggle(
            new GUIContent("Allow Intermediate Connections"),
            levelDefinition.AllowIntermediateConnections);

        EditorGUILayout.Toggle(
            new GUIContent("Allow Any Configured Source"),
            levelDefinition.AllowAnyConfiguredPowerSource);
    }
}


        // ============================================================
        // POWER
        // ============================================================

        private void DrawPowerConfiguration(
            SerializedProperty level)
        {
            EditorGUILayout.LabelField(
                "POWER SOURCES",
                EditorStyles.boldLabel);

            SerializedProperty sources =
                level.FindPropertyRelative(
                    "powerSources");

            DrawPowerSourceArray(sources);
        }

        private void DrawPowerSourceArray(
            SerializedProperty sources)
        {
            if (sources == null)
                return;

            EditorGUILayout.PropertyField(
                sources,
                new GUIContent(
                    "Power Sources"),
                false);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("＋ Add Source"))
            {
                sources.InsertArrayElementAtIndex(
                    sources.arraySize);

                ClearPowerSource(
                    sources.GetArrayElementAtIndex(
                        sources.arraySize - 1));
            }

            GUI.enabled = sources.arraySize > 0;

            if (GUILayout.Button("Remove Last"))
                sources.DeleteArrayElementAtIndex(
                    sources.arraySize - 1);

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (sources.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No power source configured.",
                    MessageType.Warning);

                return;
            }

            for (int i = 0;
                 i < sources.arraySize;
                 i++)
            {
                SerializedProperty source =
                    sources.GetArrayElementAtIndex(i);

                if (source == null)
                    continue;

                SerializedProperty positive =
                    source.FindPropertyRelative(
                        "positiveTerminal");

                SerializedProperty negative =
                    source.FindPropertyRelative(
                        "negativeTerminal");

                SerializedProperty voltage =
                    source.FindPropertyRelative(
                        "nominalVoltage");

                if (positive == null ||
                    negative == null)
                {
                    continue;
                }

                bool valid =
                    positive.objectReferenceValue != null &&
                    negative.objectReferenceValue != null;

                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                EditorGUILayout.LabelField(
                    $"SOURCE {i + 1}",
                    EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(
                    source.FindPropertyRelative(
                        "sourceName"),
                    new GUIContent(
                        "Name"));

                EditorGUILayout.PropertyField(
                    positive,
                    new GUIContent(
                        "Positive Terminal"));

                EditorGUILayout.PropertyField(
                    negative,
                    new GUIContent(
                        "Negative Terminal"));

                EditorGUILayout.PropertyField(
                    voltage,
                    new GUIContent(
                        "Nominal Voltage"));

                EditorGUILayout.LabelField(
                    valid
                        ? "✓ Configured"
                        : "✗ Missing terminal",
                    valid
                        ? EditorStyles.miniLabel
                        : EditorStyles.boldLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void ClearPowerSource(
            SerializedProperty source)
        {
            if (source == null)
                return;

            SerializedProperty sourceName =
                source.FindPropertyRelative("sourceName");

            SerializedProperty positive =
                source.FindPropertyRelative("positiveTerminal");

            SerializedProperty negative =
                source.FindPropertyRelative("negativeTerminal");

            SerializedProperty nominalVoltage =
                source.FindPropertyRelative("nominalVoltage");

            if (sourceName != null)
                sourceName.stringValue = "Power Source";

            if (positive != null)
                positive.objectReferenceValue = null;

            if (negative != null)
                negative.objectReferenceValue = null;

            if (nominalVoltage != null)
                nominalVoltage.floatValue = 5f;
        }

        private void ClearTarget(
            SerializedProperty target)
        {
            if (target == null)
                return;

            SerializedProperty id =
                target.FindPropertyRelative("targetId");

            SerializedProperty displayName =
                target.FindPropertyRelative("displayName");

            SerializedProperty description =
                target.FindPropertyRelative("description");

            SerializedProperty type =
                target.FindPropertyRelative("targetType");

            SerializedProperty targetTerminal =
                target.FindPropertyRelative("targetTerminal");

            SerializedProperty targetComponent =
                target.FindPropertyRelative("targetComponent");

            SerializedProperty minimumVoltage =
                target.FindPropertyRelative("minimumVoltage");

            SerializedProperty minimumCurrent =
                target.FindPropertyRelative("minimumCurrent");

            SerializedProperty minimumPower =
                target.FindPropertyRelative("minimumPower");

            SerializedProperty enabled =
                target.FindPropertyRelative(
                    "requireElectricalEnabled");

            SerializedProperty conduction =
                target.FindPropertyRelative(
                    "requireConduction");

            SerializedProperty inverted =
                target.FindPropertyRelative("inverted");

            if (id != null)
                id.stringValue = "TARGET";

            if (displayName != null)
                displayName.stringValue = "Target";

            if (description != null)
                description.stringValue = string.Empty;

            if (type != null)
            {
                type.enumValueIndex =
                    (int)SparkLevelTarget.TargetType.TerminalPowered;
            }

            if (targetTerminal != null)
                targetTerminal.objectReferenceValue = null;

            if (targetComponent != null)
                targetComponent.objectReferenceValue = null;

            if (minimumVoltage != null)
                minimumVoltage.floatValue = 0.01f;

            if (minimumCurrent != null)
                minimumCurrent.floatValue = 0f;

            if (minimumPower != null)
                minimumPower.floatValue = 0f;

            if (enabled != null)
                enabled.boolValue = true;

            if (conduction != null)
                conduction.boolValue = false;

            if (inverted != null)
                inverted.boolValue = false;
        }

        // ============================================================
        // TARGETS
        // ============================================================

        private void DrawTargets(
            SerializedProperty level)
        {
            EditorGUILayout.LabelField(
                "OBJECTIVES / TARGETS",
                EditorStyles.boldLabel);

            SerializedProperty targets =
                level.FindPropertyRelative(
                    "targets");

            if (targets == null)
                return;

            EditorGUILayout.PropertyField(
                targets,
                new GUIContent(
                    "Targets"),
                false);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("＋ Add Target"))
            {
                targets.InsertArrayElementAtIndex(
                    targets.arraySize);

                ClearTarget(
                    targets.GetArrayElementAtIndex(
                        targets.arraySize - 1));
            }

            GUI.enabled = targets.arraySize > 0;

            if (GUILayout.Button("Remove Last"))
                targets.DeleteArrayElementAtIndex(
                    targets.arraySize - 1);

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (targets.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No objectives configured.",
                    MessageType.Error);

                return;
            }

            for (int i = 0;
                 i < targets.arraySize;
                 i++)
            {
                SerializedProperty target =
                    targets.GetArrayElementAtIndex(i);

                DrawSingleTarget(
                    target,
                    i);
            }
        }

        private void DrawSingleTarget(
            SerializedProperty target,
            int index)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                $"TARGET {index + 1}",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "targetId"),
                new GUIContent(
                    "Target ID"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "displayName"),
                new GUIContent(
                    "Display Name"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "description"),
                new GUIContent(
                    "Description"));

            SerializedProperty type =
                target.FindPropertyRelative(
                    "targetType");

            EditorGUILayout.PropertyField(
                type,
                new GUIContent(
                    "Target Type"));

            SparkLevelTarget.TargetType targetType =
                (SparkLevelTarget.TargetType)
                type.enumValueIndex;

            EditorGUILayout.Space(3);

            switch (targetType)
            {
                case SparkLevelTarget.TargetType
                    .TerminalPowered:

                    DrawTerminalTarget(target);
                    break;

                case SparkLevelTarget.TargetType
                    .VoltagePresent:

                    DrawVoltageTarget(target);
                    break;

                case SparkLevelTarget.TargetType
                    .ComponentPowered:

                    DrawComponentTarget(target);
                    break;

                case SparkLevelTarget.TargetType
                    .LEDOn:

                    DrawLEDTarget(target);
                    break;

                case SparkLevelTarget.TargetType
                    .ComponentConducting:

                    DrawConductingTarget(target);
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTerminalTarget(
            SerializedProperty target)
        {
            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "targetTerminal"),
                new GUIContent(
                    "Target Terminal"));

            DrawElectricalRequirements(target);
        }

        private void DrawVoltageTarget(
            SerializedProperty target)
        {
            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "targetTerminal"),
                new GUIContent(
                    "Voltage Terminal"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "minimumVoltage"),
                new GUIContent(
                    "Minimum Voltage"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "requireElectricalEnabled"),
                new GUIContent(
                    "Require Electrical Enabled"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "inverted"),
                new GUIContent(
                    "Invert Result"));
        }

        private void DrawComponentTarget(
            SerializedProperty target)
        {
            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "targetComponent"),
                new GUIContent(
                    "Component"));

            DrawElectricalRequirements(target);
        }

        private void DrawLEDTarget(
    SerializedProperty target)
{
    EditorGUILayout.PropertyField(
        target.FindPropertyRelative(
            "targetComponent"),
        new GUIContent(
            "LED Component"));

    EditorGUILayout.HelpBox(
        "Target succeeds when the referenced " +
        "SparkLED reports IsOn.",
        MessageType.Info);

    EditorGUILayout.Space(4);

    EditorGUILayout.LabelField(
        "REQUIRED CONNECTION",
        EditorStyles.boldLabel);

    EditorGUILayout.PropertyField(
        target.FindPropertyRelative(
            "requiredPositiveTerminal"),
        new GUIContent(
            "Required Positive Terminal"));

    EditorGUILayout.PropertyField(
        target.FindPropertyRelative(
            "requiredNegativeTerminal"),
        new GUIContent(
            "Required Negative Terminal"));

    EditorGUILayout.HelpBox(
        "These terminals define the required polarity. " +
        "Source + must reach the positive terminal and " +
        "Source - must reach the negative terminal.",
        MessageType.None);

    EditorGUILayout.Space(3);

    EditorGUILayout.PropertyField(
        target.FindPropertyRelative(
            "inverted"),
        new GUIContent(
            "Invert Result"));
}

        private void DrawConductingTarget(
            SerializedProperty target)
        {
            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "targetComponent"),
                new GUIContent(
                    "Component"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "requireElectricalEnabled"),
                new GUIContent(
                    "Require Electrical Enabled"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "inverted"),
                new GUIContent(
                    "Invert Result"));
        }

        private void DrawElectricalRequirements(
            SerializedProperty target)
        {
            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "minimumVoltage"),
                new GUIContent(
                    "Minimum Voltage"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "minimumCurrent"),
                new GUIContent(
                    "Minimum Current"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "minimumPower"),
                new GUIContent(
                    "Minimum Power"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "requireElectricalEnabled"),
                new GUIContent(
                    "Require Electrical Enabled"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "requireConduction"),
                new GUIContent(
                    "Require Conduction"));

            EditorGUILayout.PropertyField(
                target.FindPropertyRelative(
                    "inverted"),
                new GUIContent(
                    "Invert Result"));
        }

        // ============================================================
        // FAILURE
        // ============================================================

    
private void DrawFailureConfiguration(
    SerializedProperty level)
{
    EditorGUILayout.LabelField(
        "FAILURE",
        EditorStyles.boldLabel);

    if (level == null)
    {
        EditorGUILayout.HelpBox(
            "Level reference is missing.",
            MessageType.Warning);

        return;
    }

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    if (levelDefinition == null)
    {
        EditorGUILayout.HelpBox(
            "Assign a SparkLevelDefinition asset before configuring failure rules.",
            MessageType.Warning);

        return;
    }

    using (new EditorGUI.DisabledScope(true))
    {
        EditorGUILayout.EnumPopup(
            new GUIContent("Failure Mode"),
            levelDefinition.FailureMode);
    }
}



        // ============================================================
        // OUTPUTS
        // ============================================================

      
private void DrawOutputs(
    SerializedProperty level)
{
    EditorGUILayout.LabelField(
        "VISUAL OUTPUTS",
        EditorStyles.boldLabel);

    if (level == null)
    {
        EditorGUILayout.HelpBox(
            "Level reference is missing.",
            MessageType.Warning);

        return;
    }

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    if (levelDefinition == null)
    {
        EditorGUILayout.HelpBox(
            "Assign a SparkLevelDefinition asset before configuring visual outputs.",
            MessageType.Warning);

        return;
    }

    using (new EditorGUI.DisabledScope(true))
    {
        EditorGUILayout.IntField(
            new GUIContent("Success Outputs"),
            levelDefinition.SuccessOutputIds != null
                ? levelDefinition.SuccessOutputIds.Length
                : 0);

        if (levelDefinition.SuccessOutputIds != null &&
            levelDefinition.SuccessOutputIds.Length > 0)
        {
            for (int i = 0;
                 i < levelDefinition.SuccessOutputIds.Length;
                 i++)
            {
                EditorGUILayout.TextField(
                    $"  Success [{i}]",
                    levelDefinition.SuccessOutputIds[i]);
            }
        }

        EditorGUILayout.Space(3);

        EditorGUILayout.IntField(
            new GUIContent("Failure Outputs"),
            levelDefinition.FailureOutputIds != null
                ? levelDefinition.FailureOutputIds.Length
                : 0);

        if (levelDefinition.FailureOutputIds != null &&
            levelDefinition.FailureOutputIds.Length > 0)
        {
            for (int i = 0;
                 i < levelDefinition.FailureOutputIds.Length;
                 i++)
            {
                EditorGUILayout.TextField(
                    $"  Failure [{i}]",
                    levelDefinition.FailureOutputIds[i]);
            }
        }
    }
}


        // ============================================================
        // PROGRESSION
        // ============================================================

private void DrawProgression(
    SerializedProperty level)
{
    EditorGUILayout.LabelField(
        "PROGRESSION",
        EditorStyles.boldLabel);

    if (level == null)
    {
        EditorGUILayout.HelpBox(
            "Level reference is missing.",
            MessageType.Warning);

        return;
    }

    SparkLevelDefinition levelDefinition =
        level.objectReferenceValue as SparkLevelDefinition;

    if (levelDefinition == null)
    {
        EditorGUILayout.HelpBox(
            "Assign a SparkLevelDefinition asset before configuring progression.",
            MessageType.Warning);

        return;
    }

    using (new EditorGUI.DisabledScope(true))
    {
        EditorGUILayout.Toggle(
            new GUIContent("Unlocked By Default"),
            levelDefinition.UnlockedByDefault);

        EditorGUILayout.Toggle(
            new GUIContent("Auto Unlock Next Level"),
            levelDefinition.AutoUnlockNextLevel);

        EditorGUILayout.Toggle(
            new GUIContent("Auto Advance On Completion"),
            levelDefinition.AutoAdvanceOnCompletion);
    }
}


        // ============================================================
        // LEVEL VALIDATION
        // ============================================================

        private void DrawLevelValidation(
            SerializedProperty level)
        {
            int errors = 0;
            int warnings = 0;

            SerializedProperty id =
                level.FindPropertyRelative("levelId");

            SerializedProperty displayName =
                level.FindPropertyRelative("displayName");

            SerializedProperty sources =
                level.FindPropertyRelative("powerSources");

            SerializedProperty targets =
                level.FindPropertyRelative("targets");

            if (id == null ||
                string.IsNullOrWhiteSpace(id.stringValue))
            {
                errors++;
            }

            if (displayName == null ||
                string.IsNullOrWhiteSpace(displayName.stringValue))
            {
                warnings++;
            }

            if (sources == null ||
                sources.arraySize == 0)
            {
                warnings++;
            }
            else
            {
                for (int i = 0;
                     i < sources.arraySize;
                     i++)
                {
                    SerializedProperty source =
                        sources.GetArrayElementAtIndex(i);

                    if (source == null)
                    {
                        errors++;
                        continue;
                    }

                    SerializedProperty positive =
                        source.FindPropertyRelative(
                            "positiveTerminal");

                    SerializedProperty negative =
                        source.FindPropertyRelative(
                            "negativeTerminal");

                    if (positive == null ||
                        negative == null ||
                        positive.objectReferenceValue == null ||
                        negative.objectReferenceValue == null)
                    {
                        errors++;
                    }
                }
            }

            if (targets == null ||
                targets.arraySize == 0)
            {
                errors++;
            }
            else
            {
                for (int i = 0;
                     i < targets.arraySize;
                     i++)
                {
                    SerializedProperty item =
                        targets.GetArrayElementAtIndex(i);

                    if (item == null)
                    {
                        errors++;
                        continue;
                    }

                    SerializedProperty type =
                        item.FindPropertyRelative("targetType");

                    if (type == null)
                    {
                        errors++;
                        continue;
                    }

                    SparkLevelTarget.TargetType targetType =
                        (SparkLevelTarget.TargetType)
                        type.enumValueIndex;

                    SerializedProperty targetTerminal =
                        item.FindPropertyRelative(
                            "targetTerminal");

                    SerializedProperty targetComponent =
                        item.FindPropertyRelative(
                            "targetComponent");

                    switch (targetType)
                    {
                        case SparkLevelTarget.TargetType.TerminalPowered:
                        case SparkLevelTarget.TargetType.VoltagePresent:

                            if (targetTerminal == null ||
                                targetTerminal.objectReferenceValue == null)
                            {
                                errors++;
                            }

                            break;

                        case SparkLevelTarget.TargetType.ComponentPowered:
                        case SparkLevelTarget.TargetType.LEDOn:
                        case SparkLevelTarget.TargetType.ComponentConducting:

                            if (targetComponent == null ||
                                targetComponent.objectReferenceValue == null)
                            {
                                errors++;
                            }

                            break;
                    }
                }
            }

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "LEVEL VALIDATION",
                EditorStyles.boldLabel);

            if (errors > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{errors} configuration error(s).",
                    MessageType.Error);
            }
            else if (warnings > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{warnings} warning(s).",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Level configuration valid.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // RUNTIME
        // ============================================================

        private void DrawRuntime(
            LevelGamePlayManager manager)
        {
            showRuntime = DrawSectionHeader(
                "RUNTIME",
                showRuntime);

            if (showRuntime)
            {
                EditorGUILayout.PropertyField(runtimeState);
                EditorGUILayout.PropertyField(autoStartFirstLevel);
                EditorGUILayout.PropertyField(autoEvaluate);
                EditorGUILayout.PropertyField(autoAdvanceOnCompletion);
                EditorGUILayout.PropertyField(preventInteractionAfterCompletion);

                if (Application.isPlaying)
                {
                    EditorGUILayout.Space(4);

                    EditorGUILayout.BeginVertical(
                        EditorStyles.helpBox);

                    EditorGUILayout.LabelField(
                        "LIVE EVALUATION",
                        EditorStyles.boldLabel);

                    EditorGUILayout.LabelField(
                        "State",
                        manager.RuntimeState.ToString());

                    EditorGUILayout.LabelField(
                        "Targets",
                        $"{manager.SatisfiedTargetCount} / " +
                        $"{manager.ActiveTargetCount}");

                    EditorGUILayout.LabelField(
                        "Valid Power Source",
                        manager.HasValidPowerSource ? "YES" : "NO");

                    EditorGUILayout.LabelField(
                        "Active Source",
                        string.IsNullOrEmpty(manager.ActiveSourceName)
                            ? "NONE"
                            : manager.ActiveSourceName);

                    EditorGUILayout.LabelField(
                        "Closed Return",
                        manager.ClosedReturn ? "YES" : "NO");

                    EditorGUILayout.LabelField(
                        "Source Short",
                        manager.SourceShorted ? "YES" : "NO");

                    EditorGUILayout.LabelField(
                        "Target Short",
                        manager.TargetShorted ? "YES" : "NO");

                   /* EditorGUILayout.LabelField(
                        "Overload",
                        manager.IsOverloaded ? "YES" : "NO");*/

                    EditorGUILayout.LabelField(
                        "Target Voltage",
                        $"{manager.TargetVoltage:0.###} V");

                    EditorGUILayout.LabelField(
                        "Target Current",
                        $"{manager.TargetCurrent:0.###} A");

                    EditorGUILayout.LabelField(
                        "Target Power",
                        $"{manager.TargetPower:0.###} W");

                    if (!string.IsNullOrEmpty(
                            manager.LastFailureReason))
                    {
                        EditorGUILayout.Space(3);

                        EditorGUILayout.HelpBox(
                            manager.LastFailureReason,
                            MessageType.Error);
                    }

                    if (!string.IsNullOrEmpty(
                            manager.LastEvaluationMessage))
                    {
                        EditorGUILayout.Space(3);

                        EditorGUILayout.HelpBox(
                            manager.LastEvaluationMessage,
                            MessageType.Info);
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            EndSection();
        }

        // ============================================================
        // DIAGNOSTICS
        // ============================================================

        private void DrawDiagnostics(
            LevelGamePlayManager manager)
        {
            showDiagnostics = DrawSectionHeader(
                "DIAGNOSTICS",
                showDiagnostics);

            if (showDiagnostics)
            {
                EditorGUILayout.PropertyField(
                    debugLogging);

                EditorGUILayout.PropertyField(
                    verboseLogging);

                EditorGUILayout.PropertyField(
                    showRuntimeState);

                EditorGUILayout.Space(4);

                if (GUILayout.Button(
                    "Validate All Levels",
                    GUILayout.Height(25)))
                {
                    ValidateAllLevels(manager);
                }

                if (GUILayout.Button(
                    "Log Level Summary",
                    GUILayout.Height(25)))
                {
                    LogSummary(manager);
                }
            }

            EndSection();
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void DrawValidation(
            LevelGamePlayManager manager)
        {
            showValidation = DrawSectionHeader(
                "VALIDATION",
                showValidation);

            if (showValidation)
            {
                int errors = 0;
                int warnings = 0;

                if (manager.CircuitSystem == null)
                    errors++;

                if (manager.LevelCount == 0)
                    errors++;

                if (manager.Levels != null)
                {
                    for (int i = 0;
                         i < manager.Levels.Length;
                         i++)
                    {
                        SparkLevelDefinition level =
                            manager.Levels[i];

                        if (level == null)
                        {
                            errors++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(
                                level.LevelId))
                        {
                            errors++;
                        }

                        if (level.TargetCount == 0)
                        {
                            errors++;
                        }

                        if (level.SourceCount == 0)
                        {
                            warnings++;
                        }
                    }
                }

                if (errors > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"{errors} configuration error(s).",
                        MessageType.Error);
                }
                else if (warnings > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"{warnings} warning(s).",
                        MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "All configured levels pass basic validation.",
                        MessageType.Info);
                }
            }

            EndSection();
        }

        // ============================================================
        // ARRAY OPERATIONS
        // ============================================================

       
    
        private void AddLevel()
        {
            if (levels == null)
                return;

            LevelGamePlayManager manager =
                target as LevelGamePlayManager;

            if (manager == null)
                return;

            Undo.RecordObject(
                manager,
                "Add Level Slot");

            int index =
                levels.arraySize;

            levels.InsertArrayElementAtIndex(index);

            SerializedProperty level =
                levels.GetArrayElementAtIndex(index);

            // New slot starts empty.
            level.objectReferenceValue = null;

            // Make the newly-added slot active.
            activeLevelIndex.intValue = index;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(manager);

            SyncFoldouts();

            // Open the newly-added slot.
            if (levelFoldouts != null &&
                index >= 0 &&
                index < levelFoldouts.Length)
            {
                levelFoldouts[index] = true;
            }

            Repaint();
        }


        private void DuplicateActiveLevel(
            LevelGamePlayManager manager)
        {
            if (levels.arraySize == 0)
                return;

            int sourceIndex =
                Mathf.Clamp(
                    manager.ActiveLevelIndex,
                    0,
                    levels.arraySize - 1);

            Undo.RecordObject(
                target,
                "Duplicate Spark Level");

            levels.InsertArrayElementAtIndex(
                sourceIndex);

            int newIndex =
                sourceIndex + 1;

            SerializedProperty level =
                levels.GetArrayElementAtIndex(
                    newIndex);

            SerializedProperty id =
                level.FindPropertyRelative(
                    "levelId");

            SerializedProperty displayName =
                level.FindPropertyRelative(
                    "displayName");

            id.stringValue += "_COPY";

            displayName.stringValue +=
                " Copy";

            activeLevelIndex.intValue =
                newIndex;

            SyncFoldouts();

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }

        private void RemoveActiveLevel(
            LevelGamePlayManager manager)
        {
            if (levels.arraySize == 0)
                return;

            int index =
                Mathf.Clamp(
                    manager.ActiveLevelIndex,
                    0,
                    levels.arraySize - 1);

            SparkLevelDefinition level =
                manager.ActiveLevel;

            string name =
                level != null
                    ? level.DisplayName
                    : $"Level {index + 1}";

            if (!EditorUtility.DisplayDialog(
                "Remove Level",
                $"Remove '{name}'?",
                "Remove",
                "Cancel"))
            {
                return;
            }

            Undo.RecordObject(
                target,
                "Remove Spark Level");

            levels.DeleteArrayElementAtIndex(
                index);

            if (levels.arraySize == 0)
            {
                activeLevelIndex.intValue = 0;
            }
            else
            {
                activeLevelIndex.intValue =
                    Mathf.Clamp(
                        index,
                        0,
                        levels.arraySize - 1);
            }

            SyncFoldouts();

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }

        private void ChangeActiveLevel(
            int index)
        {
            if (levels.arraySize == 0)
                return;

            index =
                Mathf.Clamp(
                    index,
                    0,
                    levels.arraySize - 1);

            Undo.RecordObject(
                target,
                "Change Active Level");

            activeLevelIndex.intValue =
                index;

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
      
        private void ClearLevel(
            SerializedProperty level)
        {
            if (level == null)
                return;

            // The manager stores SparkLevelDefinition assets as
            // object references. There are no embedded level fields
            // to clear here.
            level.objectReferenceValue = null;
        }


        // ============================================================
        // FOLDOUT MANAGEMENT
        // ============================================================

        private void SyncFoldouts()
        {
            int count =
                levels != null
                    ? levels.arraySize
                    : 0;

            if (levelFoldouts == null ||
                levelFoldouts.Length != count)
            {
                bool[] previous =
                    levelFoldouts;

                levelFoldouts =
                    new bool[count];

                if (previous != null)
                {
                    int copy =
                        Mathf.Min(
                            previous.Length,
                            levelFoldouts.Length);

                    for (int i = 0;
                         i < copy;
                         i++)
                    {
                        levelFoldouts[i] =
                            previous[i];
                    }
                }

                if (count > 0)
                {
                    int active =
                        Mathf.Clamp(
                            activeLevelIndex.intValue,
                            0,
                            count - 1);

                    levelFoldouts[active] = true;
                }
            }
        }

        // ============================================================
        // LOGGING / VALIDATION
        // ============================================================

        private void ValidateAllLevels(
            LevelGamePlayManager manager)
        {
            int errors = 0;
            int warnings = 0;

            if (manager.CircuitSystem == null)
                errors++;

            if (manager.Levels == null ||
                manager.Levels.Length == 0)
            {
                errors++;
            }
            else
            {
                for (int i = 0;
                     i < manager.Levels.Length;
                     i++)
                {
                    SparkLevelDefinition level =
                        manager.Levels[i];

                    if (level == null)
                    {
                        errors++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(
                            level.LevelId))
                    {
                        errors++;
                    }

                    if (level.TargetCount == 0)
                    {
                        errors++;
                    }

                    if (level.SourceCount == 0)
                    {
                        warnings++;
                    }

                    if (level.Targets != null)
                    {
                        for (int t = 0;
                             t < level.Targets.Length;
                             t++)
                        {
                            if (level.Targets[t] == null)
                                errors++;
                        }
                    }

                    if (level.PowerSources != null)
                    {
                        for (int s = 0;
                             s < level.PowerSources.Length;
                             s++)
                        {
                            if (level.PowerSources[s] == null)
                            {
                                errors++;
                                continue;
                            }

                            if (!level.PowerSources[s]
                                .IsConfigured)
                            {
                                errors++;
                            }
                        }
                    }
                }
            }

            if (errors == 0 &&
                warnings == 0)
            {
                Debug.Log(
                    "[LevelGamePlayManager] " +
                    "All level validation passed.",
                    manager);
            }
            else
            {
                Debug.LogWarning(
                    "[LevelGamePlayManager] " +
                    $"Validation: " +
                    $"{errors} errors, " +
                    $"{warnings} warnings.",
                    manager);
            }
        }

        private void LogSummary(
            LevelGamePlayManager manager)
        {
            Debug.Log(
                "========== PROJECT SPARK LEVEL SUMMARY ==========",
                manager);

            Debug.Log(
                $"Total Levels: {manager.LevelCount}",
                manager);

            Debug.Log(
                $"Active Index: {manager.ActiveLevelIndex}",
                manager);

            if (manager.Levels != null)
            {
                for (int i = 0;
                     i < manager.Levels.Length;
                     i++)
                {
                    SparkLevelDefinition level =
                        manager.Levels[i];

                    if (level == null)
                    {
                        Debug.Log(
                            $"[{i}] NULL",
                            manager);

                        continue;
                    }

                    Debug.Log(
                        $"[{i}] " +
                        $"{level.LevelId} | " +
                        $"{level.DisplayName} | " +
                        $"Sources={level.SourceCount} | " +
                        $"Targets={level.TargetCount}",
                        manager);
                }
            }

            Debug.Log(
                "=================================================",
                manager);
        }
    }
}

#endif