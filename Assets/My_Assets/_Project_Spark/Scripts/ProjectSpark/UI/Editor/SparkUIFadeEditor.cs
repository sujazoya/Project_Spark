#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.Editor
{
    [CustomEditor(typeof(SparkUIFade))]
    [CanEditMultipleObjects]
    public sealed class SparkUIFadeEditor : UnityEditor.Editor
    {
        // ============================================================
        // TARGET
        // ============================================================

        private SerializedProperty targetProperty;
        private SerializedProperty canvasGroupProperty;
        private SerializedProperty graphicProperty;

        // ============================================================
        // FADE RANGE
        // ============================================================

        private SerializedProperty fromPercentageProperty;
        private SerializedProperty toPercentageProperty;

        // ============================================================
        // TIMING
        // ============================================================

        private SerializedProperty fadeModeProperty;
        private SerializedProperty speedProperty;
        private SerializedProperty durationProperty;
        private SerializedProperty delayProperty;

        // ============================================================
        // ON ENABLE
        // ============================================================

        private SerializedProperty showOnEnableProperty;
        private SerializedProperty onEnableFromPercentageProperty;
        private SerializedProperty onEnableToPercentageProperty;
        private SerializedProperty onEnableDelayProperty;

        // ============================================================
        // STYLE
        // ============================================================

        private SerializedProperty styleProperty;
        private SerializedProperty customCurveProperty;

        // ============================================================
        // TIME
        // ============================================================

        private SerializedProperty ignoreTimeScaleProperty;

        // ============================================================
        // AUDIO
        // ============================================================

        private SerializedProperty useAudioProperty;
        private SerializedProperty audioSourceProperty;
        private SerializedProperty fadeInClipProperty;
        private SerializedProperty fadeOutClipProperty;
        private SerializedProperty audioVolumeProperty;
        private SerializedProperty fadeAudioProperty;

        // ============================================================
        // LOOP
        // ============================================================

        private SerializedProperty loopProperty;
        private SerializedProperty loopDelayProperty;

        // ============================================================
        // EVENTS
        // ============================================================

        private SerializedProperty onFadeStartedProperty;
        private SerializedProperty onFadeCompletedProperty;
        private SerializedProperty onFadeStoppedProperty;

        // ============================================================
        // EDITOR STYLES
        // ============================================================

        private GUIStyle sectionHeaderStyle;
        private GUIStyle titleStyle;

        private static readonly Color HeaderColor =
            new Color(0.10f, 0.75f, 1.00f);

        private static readonly Color SectionColor =
            new Color(0.12f, 0.12f, 0.14f);

        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void OnEnable()
        {
            // --------------------------------------------------------
            // TARGET
            // --------------------------------------------------------

            targetProperty =
                serializedObject.FindProperty("target");

            canvasGroupProperty =
                serializedObject.FindProperty("canvasGroup");

            graphicProperty =
                serializedObject.FindProperty("graphic");

            // --------------------------------------------------------
            // RANGE
            // --------------------------------------------------------

            fromPercentageProperty =
                serializedObject.FindProperty("fromPercentage");

            toPercentageProperty =
                serializedObject.FindProperty("toPercentage");

            // --------------------------------------------------------
            // TIMING
            // --------------------------------------------------------

            fadeModeProperty =
                serializedObject.FindProperty("fadeMode");

            speedProperty =
                serializedObject.FindProperty("speed");

            durationProperty =
                serializedObject.FindProperty("duration");

            delayProperty =
                serializedObject.FindProperty("delay");

            // --------------------------------------------------------
            // ON ENABLE
            // --------------------------------------------------------

            showOnEnableProperty =
                serializedObject.FindProperty("showOnEnable");

            onEnableFromPercentageProperty =
                serializedObject.FindProperty(
                    "onEnableFromPercentage");

            onEnableToPercentageProperty =
                serializedObject.FindProperty(
                    "onEnableToPercentage");

            onEnableDelayProperty =
                serializedObject.FindProperty(
                    "onEnableDelay");

            // --------------------------------------------------------
            // STYLE
            // --------------------------------------------------------

            styleProperty =
                serializedObject.FindProperty("style");

            customCurveProperty =
                serializedObject.FindProperty("customCurve");

            // --------------------------------------------------------
            // TIME
            // --------------------------------------------------------

            ignoreTimeScaleProperty =
                serializedObject.FindProperty(
                    "ignoreTimeScale");

            // --------------------------------------------------------
            // AUDIO
            // --------------------------------------------------------

            useAudioProperty =
                serializedObject.FindProperty("useAudio");

            audioSourceProperty =
                serializedObject.FindProperty("audioSource");

            fadeInClipProperty =
                serializedObject.FindProperty("fadeInClip");

            fadeOutClipProperty =
                serializedObject.FindProperty("fadeOutClip");

            audioVolumeProperty =
                serializedObject.FindProperty("audioVolume");

            fadeAudioProperty =
                serializedObject.FindProperty("fadeAudio");

            // --------------------------------------------------------
            // LOOP
            // --------------------------------------------------------

            loopProperty =
                serializedObject.FindProperty("loop");

            loopDelayProperty =
                serializedObject.FindProperty("loopDelay");

            // --------------------------------------------------------
            // EVENTS
            // --------------------------------------------------------

            onFadeStartedProperty =
                serializedObject.FindProperty(
                    "onFadeStarted");

            onFadeCompletedProperty =
                serializedObject.FindProperty(
                    "onFadeCompleted");

            onFadeStoppedProperty =
                serializedObject.FindProperty(
                    "onFadeStopped");
        }

        // ============================================================
        // INSPECTOR
        // ============================================================

        public override void OnInspectorGUI()
        {
            EnsureStyles();

            serializedObject.Update();

            EditorGUILayout.Space(4);

            DrawTitle();

            EditorGUILayout.Space(6);

            DrawTargetSection();

            DrawRangeSection();

            DrawTimingSection();

            DrawOnEnableSection();

            DrawStyleSection();

            DrawTimeSection();

            DrawAudioSection();

            DrawLoopSection();

            DrawEventsSection();

            DrawRuntimeButtons();

            serializedObject.ApplyModifiedProperties();
        }

        // ============================================================
        // STYLE INITIALIZATION
        // ============================================================

        private void EnsureStyles()
        {
            // IMPORTANT:
            // Do not access EditorStyles from OnEnable().
            // Unity may not have initialized them yet.

            if (sectionHeaderStyle == null)
            {
                sectionHeaderStyle =
                    new GUIStyle(EditorStyles.boldLabel);

                sectionHeaderStyle.fontSize = 11;
                sectionHeaderStyle.alignment =
                    TextAnchor.MiddleLeft;

                sectionHeaderStyle.normal.textColor =
                    Color.white;
            }

            if (titleStyle == null)
            {
                titleStyle =
                    new GUIStyle(EditorStyles.boldLabel);

                titleStyle.fontSize = 15;
                titleStyle.alignment =
                    TextAnchor.MiddleLeft;

                titleStyle.normal.textColor =
                    HeaderColor;
            }
        }

        // ============================================================
        // TITLE
        // ============================================================

        private void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(4);

            EditorGUILayout.LabelField(
                "PROJECT SPARK",
                titleStyle);

            EditorGUILayout.LabelField(
                "UI FADE CONTROLLER",
                EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            EditorGUILayout.HelpBox(
                "Production-ready UI opacity controller with percentage control, " +
                "automatic On Enable playback, timing, easing, audio, looping " +
                "and UnityEvents.",
                MessageType.None);
        }

        // ============================================================
        // TARGET
        // ============================================================

        private void DrawTargetSection()
        {
            BeginSection("TARGET");

            EditorGUILayout.PropertyField(
                targetProperty,
                new GUIContent("Fade Target"));

            EditorGUILayout.Space(3);

            SparkUIFade.FadeTarget target =
                (SparkUIFade.FadeTarget)
                targetProperty.enumValueIndex;

            if (target ==
                SparkUIFade.FadeTarget.CanvasGroup ||
                target ==
                SparkUIFade.FadeTarget.CanvasGroupAndGraphic)
            {
                EditorGUILayout.PropertyField(
                    canvasGroupProperty,
                    new GUIContent("Canvas Group"));
            }

            if (target ==
                SparkUIFade.FadeTarget.Graphic ||
                target ==
                SparkUIFade.FadeTarget.CanvasGroupAndGraphic)
            {
                EditorGUILayout.PropertyField(
                    graphicProperty,
                    new GUIContent("Graphic / Image"));
            }

            EditorGUILayout.Space(3);

            EditorGUILayout.HelpBox(
                GetTargetHelpText(target),
                MessageType.Info);

            EndSection();
        }

        private string GetTargetHelpText(
            SparkUIFade.FadeTarget target)
        {
            switch (target)
            {
                case SparkUIFade.FadeTarget.CanvasGroup:

                    return
                        "Controls the opacity of the CanvasGroup. " +
                        "Recommended for complete UI panels.";

                case SparkUIFade.FadeTarget.Graphic:

                    return
                        "Controls the alpha of one Graphic/Image.";

                case SparkUIFade.FadeTarget.CanvasGroupAndGraphic:

                    return
                        "Controls both CanvasGroup and Graphic alpha.";

                default:

                    return string.Empty;
            }
        }

        // ============================================================
        // RANGE
        // ============================================================

        private void DrawRangeSection()
        {
            BeginSection("FADE RANGE");

            EditorGUILayout.PropertyField(
                fromPercentageProperty,
                new GUIContent(
                    "From",
                    "Starting opacity percentage."));

            EditorGUILayout.PropertyField(
                toPercentageProperty,
                new GUIContent(
                    "To",
                    "Ending opacity percentage."));

            EditorGUILayout.Space(4);

            float from =
                fromPercentageProperty.floatValue;

            float to =
                toPercentageProperty.floatValue;

            Rect rect =
                EditorGUILayout.GetControlRect(
                    false,
                    EditorGUIUtility.singleLineHeight);

            EditorGUI.ProgressBar(
                rect,
                Mathf.Clamp01(to / 100f),
                $"{from:0}% → {to:0}%");

            EndSection();
        }

        // ============================================================
        // TIMING
        // ============================================================

        private void DrawTimingSection()
        {
            BeginSection("TIMING");

            EditorGUILayout.PropertyField(
                fadeModeProperty,
                new GUIContent("Mode"));

            SparkUIFade.FadeMode mode =
                (SparkUIFade.FadeMode)
                fadeModeProperty.enumValueIndex;

            EditorGUILayout.Space(3);

            if (mode == SparkUIFade.FadeMode.Speed)
            {
                EditorGUILayout.PropertyField(
                    speedProperty,
                    new GUIContent(
                        "Speed",
                        "Opacity percentage points changed per second."));

                EditorGUILayout.HelpBox(
                    "Speed controls how many opacity percentage points " +
                    "are changed per second.",
                    MessageType.None);
            }
            else
            {
                EditorGUILayout.PropertyField(
                    durationProperty,
                    new GUIContent(
                        "Duration",
                        "Total fade duration in seconds."));

                EditorGUILayout.HelpBox(
                    "Duration defines the total time required to complete " +
                    "the fade.",
                    MessageType.None);
            }

            EditorGUILayout.PropertyField(
                delayProperty,
                new GUIContent(
                    "Delay",
                    "Delay before normal Play/FadeIn/FadeOut calls."));

            EndSection();
        }

        // ============================================================
        // ON ENABLE
        // ============================================================

        private void DrawOnEnableSection()
        {
            BeginSection("ON ENABLE");

            EditorGUILayout.PropertyField(
                showOnEnableProperty,
                new GUIContent(
                    "Show On Enable",
                    "Automatically play the fade whenever this " +
                    "GameObject becomes enabled."));

            if (showOnEnableProperty.boolValue)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(
                    onEnableFromPercentageProperty,
                    new GUIContent(
                        "From",
                        "Opacity percentage when the GameObject " +
                        "becomes enabled."));

                EditorGUILayout.PropertyField(
                    onEnableToPercentageProperty,
                    new GUIContent(
                        "To",
                        "Target opacity percentage."));

                EditorGUILayout.PropertyField(
                    onEnableDelayProperty,
                    new GUIContent(
                        "Delay",
                        "Delay before the automatic fade starts."));

                EditorGUILayout.Space(5);

                float from =
                    onEnableFromPercentageProperty.floatValue;

                float to =
                    onEnableToPercentageProperty.floatValue;

                Rect previewRect =
                    EditorGUILayout.GetControlRect(
                        false,
                        EditorGUIUtility.singleLineHeight);

                EditorGUI.ProgressBar(
                    previewRect,
                    Mathf.Clamp01(to / 100f),
                    $"AUTO: {from:0}% → {to:0}%");

                EditorGUILayout.Space(4);

                EditorGUILayout.HelpBox(
                    "SELF APPLY ENABLED\n\n" +
                    "Whenever this GameObject becomes active, " +
                    "the fade automatically starts from the configured " +
                    "On Enable percentage and moves to the target percentage.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Disabled. The component will not automatically " +
                    "fade when the GameObject becomes enabled.",
                    MessageType.None);
            }

            EndSection();
        }

        // ============================================================
        // STYLE
        // ============================================================

        private void DrawStyleSection()
        {
            BeginSection("FADE STYLE");

            EditorGUILayout.PropertyField(
                styleProperty,
                new GUIContent("Style"));

            SparkUIFade.FadeStyle style =
                (SparkUIFade.FadeStyle)
                styleProperty.enumValueIndex;

            if (style ==
                SparkUIFade.FadeStyle.CustomCurve)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(
                    customCurveProperty,
                    new GUIContent("Custom Curve"));

                EditorGUILayout.HelpBox(
                    "The X axis represents normalized time. " +
                    "The Y axis represents normalized fade progress.",
                    MessageType.None);
            }

            EndSection();
        }

        // ============================================================
        // TIME
        // ============================================================

        private void DrawTimeSection()
        {
            BeginSection("TIME");

            EditorGUILayout.PropertyField(
                ignoreTimeScaleProperty,
                new GUIContent(
                    "Ignore Time Scale",
                    "Continue fading when Time.timeScale is 0."));

            if (ignoreTimeScaleProperty.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Fade uses unscaled time. " +
                    "Useful for UI, pause screens and menus.",
                    MessageType.Info);
            }

            EndSection();
        }

        // ============================================================
        // AUDIO
        // ============================================================

        private void DrawAudioSection()
        {
            BeginSection("AUDIO");

            EditorGUILayout.PropertyField(
                useAudioProperty,
                new GUIContent("Use Audio"));

            if (useAudioProperty.boolValue)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(
                    audioSourceProperty,
                    new GUIContent("Audio Source"));

                EditorGUILayout.PropertyField(
                    fadeInClipProperty,
                    new GUIContent("Fade In Clip"));

                EditorGUILayout.PropertyField(
                    fadeOutClipProperty,
                    new GUIContent("Fade Out Clip"));

                EditorGUILayout.PropertyField(
                    audioVolumeProperty,
                    new GUIContent("Volume"));

                EditorGUILayout.PropertyField(
                    fadeAudioProperty,
                    new GUIContent(
                        "Fade Audio",
                        "Synchronize audio volume with fade progress."));

                EditorGUILayout.Space(3);

                EditorGUILayout.HelpBox(
                    "Fade In Clip is used when opacity increases. " +
                    "Fade Out Clip is used when opacity decreases.",
                    MessageType.None);
            }

            EndSection();
        }

        // ============================================================
        // LOOP
        // ============================================================

        private void DrawLoopSection()
        {
            BeginSection("LOOP");

            EditorGUILayout.PropertyField(
                loopProperty,
                new GUIContent(
                    "Loop",
                    "Continuously alternate between the target " +
                    "and opposite opacity."));

            if (loopProperty.boolValue)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(
                    loopDelayProperty,
                    new GUIContent(
                        "Loop Delay",
                        "Delay between each fade cycle."));

                EditorGUILayout.Space(3);

                EditorGUILayout.HelpBox(
                    "Loop continuously alternates between the fade " +
                    "endpoints. Useful for diagnostic indicators, " +
                    "attention states and subtle UI pulsing.",
                    MessageType.Info);
            }

            EndSection();
        }

        // ============================================================
        // EVENTS
        // ============================================================

        private void DrawEventsSection()
        {
            BeginSection("EVENTS");

            EditorGUILayout.PropertyField(
                onFadeStartedProperty,
                new GUIContent("On Fade Started"));

            EditorGUILayout.PropertyField(
                onFadeCompletedProperty,
                new GUIContent("On Fade Completed"));

            EditorGUILayout.PropertyField(
                onFadeStoppedProperty,
                new GUIContent("On Fade Stopped"));

            EndSection();
        }

        // ============================================================
        // RUNTIME BUTTONS
        // ============================================================

        private void DrawRuntimeButtons()
        {
            if (!Application.isPlaying)
                return;

            BeginSection("RUNTIME TEST");

            SparkUIFade fade =
                target as SparkUIFade;

            if (fade == null)
            {
                EndSection();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "SHOW",
                    GUILayout.Height(28)))
            {
                fade.Show();
            }

            if (GUILayout.Button(
                    "HIDE",
                    GUILayout.Height(28)))
            {
                fade.Hide();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "FADE IN",
                    GUILayout.Height(28)))
            {
                fade.FadeIn();
            }

            if (GUILayout.Button(
                    "FADE OUT",
                    GUILayout.Height(28)))
            {
                fade.FadeOut();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "PLAY",
                    GUILayout.Height(28)))
            {
                fade.Play();
            }

            if (GUILayout.Button(
                    "TOGGLE",
                    GUILayout.Height(28)))
            {
                fade.Toggle();
            }

            if (GUILayout.Button(
                    "STOP",
                    GUILayout.Height(28)))
            {
                fade.Stop();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            float current =
                fade.CurrentPercentage;

            Rect progressRect =
                EditorGUILayout.GetControlRect(
                    false,
                    EditorGUIUtility.singleLineHeight);

            EditorGUI.ProgressBar(
                progressRect,
                Mathf.Clamp01(current / 100f),
                $"CURRENT: {current:0.0}%");

            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField(
                $"State: {(fade.IsFading ? "FADING" : "IDLE")}",
                EditorStyles.miniLabel);

            EditorGUILayout.LabelField(
                $"Visible: {(fade.IsVisible ? "YES" : "NO")}",
                EditorStyles.miniLabel);

            EndSection();

            Repaint();
        }

        // ============================================================
        // SECTION HELPERS
        // ============================================================

        private void BeginSection(string title)
        {
            EditorGUILayout.Space(6);

            Rect rect =
                EditorGUILayout.GetControlRect(
                    false,
                    22f);

            EditorGUI.DrawRect(
                rect,
                SectionColor);

            Rect labelRect =
                new Rect(
                    rect.x + 8f,
                    rect.y,
                    rect.width - 16f,
                    rect.height);

            EditorGUI.LabelField(
                labelRect,
                title,
                sectionHeaderStyle);

            EditorGUILayout.Space(2);
        }

        private void EndSection()
        {
            EditorGUILayout.Space(4);
        }
    }
}

#endif