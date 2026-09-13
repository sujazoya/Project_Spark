#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.Animation;

namespace ProjectSpark.UI.Animation.Editor
{
    public sealed class UIAnimationWindow : EditorWindow
    {
        private UIAnimator animator;
        private UIAnimationPreset preset;

        private SerializedObject presetSerializedObject;

        private Vector2 scrollPosition;

        private bool followSelection = true;

        private bool showPosition = true;
        private bool showScale = true;
        private bool showRotation;
        private bool showAlpha = true;
        private bool showTiming = true;
        private bool showInteraction = true;
        private bool showPunch;
        private bool showShake;
        private bool showLoop;

        private bool previewPlaying;
        private bool previewEntering = true;

        private float previewTime;
        private double lastPreviewTime;

        private const float HeaderHeight = 44f;
        private const float ButtonHeight = 28f;

        // ============================================================
        // WINDOW
        // ============================================================

        [MenuItem(
            "Project Spark/UI/Animation Editor",
            priority = 200)]
        public static void Open()
        {
            UIAnimationWindow window =
                GetWindow<UIAnimationWindow>(
                    "Project Spark UI Animation");

            window.minSize =
                new Vector2(480f, 650f);

            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged +=
                OnSelectionChanged;

            EditorApplication.update +=
                EditorUpdate;

            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -=
                OnSelectionChanged;

            EditorApplication.update -=
                EditorUpdate;

            StopPreview();
        }

        // ============================================================
        // SELECTION
        // ============================================================

        private void OnSelectionChanged()
        {
            if (!followSelection)
                return;

            if (Selection.activeGameObject == null)
                return;

            UIAnimator selected =
                Selection.activeGameObject
                    .GetComponent<UIAnimator>();

            if (selected == null)
            {
                selected =
                    Selection.activeGameObject
                        .GetComponentInParent<UIAnimator>();
            }

            if (selected == null)
                return;

            SetAnimator(selected);
        }

        private void SetAnimator(
            UIAnimator target)
        {
            animator = target;

            preset =
                animator != null
                    ? animator.Preset
                    : null;

            RefreshSerializedObject();

            Repaint();
        }

        private void RefreshSerializedObject()
        {
            presetSerializedObject =
                preset != null
                    ? new SerializedObject(preset)
                    : null;
        }

        // ============================================================
        // GUI
        // ============================================================

        private void OnGUI()
        {
            DrawHeader();

            scrollPosition =
                EditorGUILayout.BeginScrollView(
                    scrollPosition);

            DrawTargetSection();

            if (animator == null)
            {
                DrawNoTarget();
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawPresetToolbar();

            if (preset == null)
            {
                DrawNoPreset();
                EditorGUILayout.EndScrollView();
                return;
            }

            UpdateSerializedObject();

            DrawAnimationChannels();

            DrawTiming();

            DrawInteraction();

            DrawEffects();

            DrawLoop();

            DrawTimeline();

            DrawPreviewControls();

            DrawValidation();

            EditorGUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        // ============================================================
        // HEADER
        // ============================================================

        private void DrawHeader()
        {
            Rect rect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.Height(HeaderHeight));

            EditorGUI.DrawRect(
                rect,
                new Color(
                    0.025f,
                    0.035f,
                    0.045f));

            GUIStyle style =
                new GUIStyle(
                    EditorStyles.boldLabel)
                {
                    fontSize = 17,
                    alignment =
                        TextAnchor.MiddleLeft
                };

            GUI.Label(
                new Rect(
                    rect.x + 12f,
                    rect.y,
                    rect.width - 24f,
                    rect.height),
                "PROJECT SPARK  •  UI ANIMATION",
                style);
        }

        // ============================================================
        // TARGET
        // ============================================================

        private void DrawTargetSection()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "ANIMATION TARGET",
                EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            UIAnimator newAnimator =
                (UIAnimator)EditorGUILayout.ObjectField(
                    "UI Animator",
                    animator,
                    typeof(UIAnimator),
                    true);

            if (EditorGUI.EndChangeCheck())
            {
                SetAnimator(newAnimator);
            }

            followSelection =
                EditorGUILayout.ToggleLeft(
                    "Follow Unity Selection",
                    followSelection);

            if (animator != null)
            {
                EditorGUILayout.LabelField(
                    "GameObject",
                    animator.gameObject.name);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNoTarget()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Select a GameObject containing UIAnimator.",
                MessageType.Info);
        }

        // ============================================================
        // PRESET TOOLBAR
        // ============================================================

        private void DrawPresetToolbar()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "PRESET",
                EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            UIAnimationPreset newPreset =
                (UIAnimationPreset)EditorGUILayout.ObjectField(
                    "Animation Preset",
                    preset,
                    typeof(UIAnimationPreset),
                    false);

            if (EditorGUI.EndChangeCheck())
            {
                SetPreset(newPreset);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "CREATE",
                    GUILayout.Height(ButtonHeight)))
            {
                CreatePreset();
            }

            if (GUILayout.Button(
                    "DUPLICATE",
                    GUILayout.Height(ButtonHeight)))
            {
                DuplicatePreset();
            }

            if (GUILayout.Button(
                    "APPLY",
                    GUILayout.Height(ButtonHeight)))
            {
                ApplyPreset();
            }

            if (GUILayout.Button(
                    "OPEN",
                    GUILayout.Height(ButtonHeight)))
            {
                OpenPreset();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void SetPreset(
            UIAnimationPreset value)
        {
            preset = value;

            RefreshSerializedObject();

            if (animator != null)
            {
                AssignPresetToAnimator(
                    preset);
            }

            Repaint();
        }

        // ============================================================
        // CREATE
        // ============================================================

        private void CreatePreset()
        {
            string folder =
                "Assets/ProjectSpark-UI System/UI/Animation/Presets";

            EnsureFolderExists(folder);

            string path =
                EditorUtility.SaveFilePanelInProject(
                    "Create UI Animation Preset",
                    "PS_UI_Animation",
                    "asset",
                    "Create a Project Spark UI animation preset.",
                    folder);

            if (string.IsNullOrEmpty(path))
                return;

            UIAnimationPreset asset =
                ScriptableObject.CreateInstance<
                    UIAnimationPreset>();

            AssetDatabase.CreateAsset(
                asset,
                path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Undo.RegisterCreatedObjectUndo(
                asset,
                "Create UI Animation Preset");

            SetPreset(asset);

            Selection.activeObject = asset;

            EditorGUIUtility.PingObject(asset);
        }

        // ============================================================
        // DUPLICATE
        // ============================================================

        private void DuplicatePreset()
        {
            if (preset == null)
                return;

            string sourcePath =
                AssetDatabase.GetAssetPath(
                    preset);

            if (string.IsNullOrEmpty(sourcePath))
                return;

            string directory =
                Path.GetDirectoryName(sourcePath)
                ?.Replace("\\", "/");

            string fileName =
                Path.GetFileNameWithoutExtension(
                    sourcePath);

            string path =
                AssetDatabase.GenerateUniqueAssetPath(
                    $"{directory}/{fileName}_Copy.asset");

            AssetDatabase.CopyAsset(
                sourcePath,
                path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UIAnimationPreset copy =
                AssetDatabase.LoadAssetAtPath<
                    UIAnimationPreset>(
                    path);

            if (copy == null)
                return;

            SetPreset(copy);

            Selection.activeObject = copy;

            EditorGUIUtility.PingObject(copy);
        }

        // ============================================================
        // APPLY
        // ============================================================

        private void ApplyPreset()
        {
            if (animator == null ||
                preset == null)
                return;

            Undo.RecordObject(
                animator,
                "Apply UI Animation Preset");

            AssignPresetToAnimator(
                preset);

            EditorUtility.SetDirty(animator);
        }

        private void AssignPresetToAnimator(
            UIAnimationPreset value)
        {
            if (animator == null)
                return;

            SerializedObject so =
                new SerializedObject(animator);

            SerializedProperty property =
                so.FindProperty("preset");

            if (property == null)
                return;

            property.objectReferenceValue =
                value;

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(animator);
        }

        // ============================================================
        // OPEN
        // ============================================================

        private void OpenPreset()
        {
            if (preset == null)
                return;

            Selection.activeObject =
                preset;

            EditorGUIUtility.PingObject(
                preset);
        }

        // ============================================================
        // NO PRESET
        // ============================================================

        private void DrawNoPreset()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This UIAnimator has no animation preset. " +
                "Create or assign one above.",
                MessageType.Warning);
        }

        // ============================================================
        // SERIALIZED OBJECT
        // ============================================================

        private void UpdateSerializedObject()
        {
            if (presetSerializedObject == null)
                return;

            presetSerializedObject.Update();
        }

        private void ApplySerializedObject()
        {
            if (presetSerializedObject == null)
                return;

            if (presetSerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(preset);
            }
        }

        private SerializedProperty Property(
            string propertyName)
        {
            return presetSerializedObject
                ?.FindProperty(propertyName);
        }

        // ============================================================
        // CHANNELS
        // ============================================================

        private void DrawAnimationChannels()
        {
            DrawPosition();
            DrawScale();
            DrawRotation();
            DrawAlpha();

            ApplySerializedObject();
        }

        private void DrawPosition()
        {
            showPosition =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showPosition,
                    "POSITION");

            if (showPosition)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle(
                    "usePosition");

                DrawProperty(
                    "hiddenPositionOffset");

                DrawProperty(
                    "positionDuration");

                DrawProperty(
                    "positionEase");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawScale()
        {
            showScale =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showScale,
                    "SCALE");

            if (showScale)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("useScale");

                DrawProperty("hiddenScale");

                DrawProperty("scaleDuration");

                DrawProperty("scaleEase");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRotation()
        {
            showRotation =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showRotation,
                    "ROTATION");

            if (showRotation)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("useRotation");

                DrawProperty("hiddenRotation");

                DrawProperty("rotationDuration");

                DrawProperty("rotationEase");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAlpha()
        {
            showAlpha =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showAlpha,
                    "ALPHA");

            if (showAlpha)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("useAlpha");

                DrawProperty("hiddenAlpha");

                DrawProperty("fadeDuration");

                DrawProperty("fadeEase");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ============================================================
        // TIMING
        // ============================================================

        private void DrawTiming()
        {
            showTiming =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showTiming,
                    "TIMING");

            if (showTiming)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawProperty("delay");
                DrawProperty("exitMultiplier");
                DrawProperty("ignoreTimeScale");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            ApplySerializedObject();
        }

        // ============================================================
        // INTERACTION
        // ============================================================

        private void DrawInteraction()
        {
            showInteraction =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showInteraction,
                    "INTERACTION");

            if (showInteraction)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawProperty(
                    "disableInteractionDuringAnimation");

                DrawProperty(
                    "disableObjectAfterHide");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            ApplySerializedObject();
        }

        // ============================================================
        // EFFECTS
        // ============================================================

        private void DrawEffects()
        {
            showPunch =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showPunch,
                    "PUNCH");

            if (showPunch)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("usePunch");

                DrawProperty("punchScale");
                DrawProperty("punchDuration");
                DrawProperty("punchVibrato");
                DrawProperty("punchElasticity");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showShake =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showShake,
                    "SHAKE");

            if (showShake)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("useShake");

                DrawProperty("shakeStrength");
                DrawProperty("shakeDuration");
                DrawProperty("shakeVibrato");
                DrawProperty("shakeRandomness");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            ApplySerializedObject();
        }

        // ============================================================
        // LOOP
        // ============================================================

        private void DrawLoop()
        {
            showLoop =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showLoop,
                    "LOOP");

            if (showLoop)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                DrawToggle("loop");

                DrawProperty("loopCount");
                DrawProperty("loopType");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            ApplySerializedObject();
        }

        // ============================================================
        // PROPERTY HELPERS
        // ============================================================

        private void DrawToggle(
            string propertyName)
        {
            SerializedProperty property =
                Property(propertyName);

            if (property == null)
                return;

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(
                    Nicify(propertyName)));
        }

        private void DrawProperty(
            string propertyName)
        {
            SerializedProperty property =
                Property(propertyName);

            if (property == null)
                return;

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(
                    Nicify(propertyName)),
                true);
        }

        private static string Nicify(
            string value)
        {
            return ObjectNames.NicifyVariableName(
                value);
        }

        // ============================================================
        // TIMELINE
        // ============================================================

        private void DrawTimeline()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "ANIMATION TIMELINE",
                EditorStyles.boldLabel);

            float duration =
                GetPreviewDuration();

            EditorGUILayout.LabelField(
                $"Duration  {duration:0.00}s",
                EditorStyles.miniLabel);

            Rect rect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.Height(95f));

            EditorGUI.DrawRect(
                rect,
                new Color(
                    0.018f,
                    0.025f,
                    0.032f));

            DrawTimelineGrid(
                rect,
                duration);

            DrawTimelineTracks(
                rect);

            DrawTimelineCursor(
                rect,
                duration);

            EditorGUILayout.EndVertical();
        }

        private void DrawTimelineGrid(
            Rect rect,
            float duration)
        {
            const int divisions = 6;

            for (int i = 0;
                 i <= divisions;
                 i++)
            {
                float normalized =
                    i / (float)divisions;

                float x =
                    Mathf.Lerp(
                        rect.x,
                        rect.xMax,
                        normalized);

                EditorGUI.DrawRect(
                    new Rect(
                        x,
                        rect.y,
                        1f,
                        rect.height),
                    new Color(
                        0.12f,
                        0.14f,
                        0.16f));

                GUI.Label(
                    new Rect(
                        x + 3f,
                        rect.y + 3f,
                        50f,
                        16f),
                    $"{duration * normalized:0.00}",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawTimelineTracks(
            Rect rect)
        {
            float y =
                rect.y + 25f;

            DrawTrack(
                rect,
                ref y,
                "POSITION",
                IsEnabled("usePosition"));

            DrawTrack(
                rect,
                ref y,
                "SCALE",
                IsEnabled("useScale"));

            DrawTrack(
                rect,
                ref y,
                "ROTATION",
                IsEnabled("useRotation"));

            DrawTrack(
                rect,
                ref y,
                "ALPHA",
                IsEnabled("useAlpha"));
        }

        private void DrawTrack(
            Rect rect,
            ref float y,
            string label,
            bool enabled)
        {
            if (!enabled)
                return;

            GUI.Label(
                new Rect(
                    rect.x + 8f,
                    y,
                    75f,
                    16f),
                label,
                EditorStyles.miniLabel);

            EditorGUI.DrawRect(
                new Rect(
                    rect.x + 85f,
                    y + 5f,
                    rect.width - 100f,
                    5f),
                new Color(
                    0.05f,
                    0.32f,
                    0.42f));

            y += 14f;
        }

        private void DrawTimelineCursor(
            Rect rect,
            float duration)
        {
            if (!previewPlaying)
                return;

            float normalized =
                duration > 0f
                    ? previewTime / duration
                    : 0f;

            float x =
                Mathf.Lerp(
                    rect.x,
                    rect.xMax,
                    Mathf.Clamp01(normalized));

            EditorGUI.DrawRect(
                new Rect(
                    x - 1f,
                    rect.y,
                    2f,
                    rect.height),
                new Color(
                    0.1f,
                    0.85f,
                    1f));
        }

        // ============================================================
        // PREVIEW
        // ============================================================

        private void DrawPreviewControls()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "PREVIEW",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled =
                animator != null &&
                preset != null;

            if (GUILayout.Button(
                    "▶ SHOW",
                    GUILayout.Height(
                        ButtonHeight)))
            {
                PlayPreview(true);
            }

            if (GUILayout.Button(
                    "▶ HIDE",
                    GUILayout.Height(
                        ButtonHeight)))
            {
                PlayPreview(false);
            }

            if (GUILayout.Button(
                    "■ STOP",
                    GUILayout.Height(
                        ButtonHeight)))
            {
                StopPreview();
            }

            if (GUILayout.Button(
                    "RESET",
                    GUILayout.Height(
                        ButtonHeight)))
            {
                ResetPreview();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (previewPlaying)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField(
                    previewEntering
                        ? "Playing SHOW"
                        : "Playing HIDE",
                    EditorStyles.boldLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void PlayPreview(
            bool entering)
        {
            if (animator == null ||
                preset == null)
                return;

            StopPreview();

            previewEntering =
                entering;

            previewPlaying = true;

            previewTime = 0f;

            lastPreviewTime =
                EditorApplication
                    .timeSinceStartup;

            animator.EditorPreview(
                0f,
                entering);

            Repaint();
        }

        private void StopPreview()
        {
            previewPlaying = false;
            previewTime = 0f;

            Repaint();
        }

        private void ResetPreview()
        {
            StopPreview();

            if (animator == null)
                return;

            Undo.RecordObject(
                animator.transform,
                "Reset UI Animation Preview");

            animator.EditorPreviewReset();

            EditorUtility.SetDirty(
                animator);
        }

        // ============================================================
        // PREVIEW UPDATE
        // ============================================================

        private void EditorUpdate()
        {
            if (!previewPlaying ||
                animator == null ||
                preset == null)
                return;

            double now =
                EditorApplication
                    .timeSinceStartup;

            float delta =
                Mathf.Clamp(
                    (float)(
                        now -
                        lastPreviewTime),
                    0f,
                    0.1f);

            lastPreviewTime = now;

            previewTime += delta;

            float duration =
                GetPreviewDuration();

            float normalized =
                duration > 0f
                    ? previewTime / duration
                    : 1f;

            normalized =
                Mathf.Clamp01(
                    normalized);

            animator.EditorPreview(
                normalized,
                previewEntering);

            Repaint();

            if (normalized >= 1f)
            {
                StopPreview();
            }
        }

        private float GetPreviewDuration()
        {
            if (preset == null)
                return 0.01f;

            float duration = 0f;

            if (IsEnabled("usePosition"))
                duration =
                    Mathf.Max(
                        duration,
                        GetFloat(
                            "positionDuration"));

            if (IsEnabled("useScale"))
                duration =
                    Mathf.Max(
                        duration,
                        GetFloat(
                            "scaleDuration"));

            if (IsEnabled("useRotation"))
                duration =
                    Mathf.Max(
                        duration,
                        GetFloat(
                            "rotationDuration"));

            if (IsEnabled("useAlpha"))
                duration =
                    Mathf.Max(
                        duration,
                        GetFloat(
                            "fadeDuration"));

            duration +=
                GetFloat("delay");

            return Mathf.Max(
                duration,
                0.01f);
        }

        // ============================================================
        // PROPERTY READ
        // ============================================================

        private bool IsEnabled(
            string propertyName)
        {
            SerializedProperty property =
                Property(propertyName);

            return property != null &&
                   property.boolValue;
        }

        private float GetFloat(
            string propertyName)
        {
            SerializedProperty property =
                Property(propertyName);

            return property != null
                ? property.floatValue
                : 0f;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void DrawValidation()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "VALIDATION",
                EditorStyles.boldLabel);

            bool valid = true;

            if (preset == null)
            {
                valid = false;

                EditorGUILayout.HelpBox(
                    "No preset assigned.",
                    MessageType.Warning);
            }

            if (preset != null &&
                !IsAnyChannelEnabled())
            {
                valid = false;

                EditorGUILayout.HelpBox(
                    "No animation channels are enabled.",
                    MessageType.Warning);
            }

            if (preset != null &&
                GetPreviewDuration() <= 0f)
            {
                valid = false;

                EditorGUILayout.HelpBox(
                    "Animation duration is invalid.",
                    MessageType.Error);
            }

            if (valid)
            {
                EditorGUILayout.HelpBox(
                    "Preset is valid.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private bool IsAnyChannelEnabled()
        {
            return
                IsEnabled("usePosition") ||
                IsEnabled("useScale") ||
                IsEnabled("useRotation") ||
                IsEnabled("useAlpha") ||
                IsEnabled("usePunch") ||
                IsEnabled("useShake");
        }

        // ============================================================
        // FOLDER
        // ============================================================

        private static void EnsureFolderExists(
            string folder)
        {
            if (AssetDatabase.IsValidFolder(
                    folder))
            {
                return;
            }

            string[] parts =
                folder.Split('/');

            string current = parts[0];

            for (int i = 1;
                 i < parts.Length;
                 i++)
            {
                string next =
                    $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(
                        next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]);
                }

                current = next;
            }
        }
    }
}

#endif