#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.Transitions.Editor
{
    [CustomEditor(typeof(PS_UI_TransitionController))]
    [CanEditMultipleObjects]
    public sealed class PS_UI_TransitionControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty targetGraphic;
        private SerializedProperty targetRectTransform;
        private SerializedProperty canvasGroup;

        private SerializedProperty playOnEnable;
        private SerializedProperty startHidden;
        private SerializedProperty instantiateMaterial;
        private SerializedProperty useUnscaledTime;

        private SerializedProperty direction;

        private SerializedProperty enter;
        private SerializedProperty exit;

        private SerializedProperty animatePosition;
        private SerializedProperty animateScale;
        private SerializedProperty animateRotation;

        private SerializedProperty positionOffset;
        private SerializedProperty hiddenScale;
        private SerializedProperty hiddenRotation;

        private SerializedProperty preserveInitialTransform;

        private SerializedProperty animateCanvasGroup;
        private SerializedProperty hiddenAlpha;

        private SerializedProperty useGlitchEnvelope;
        private SerializedProperty glitchEnvelope;
        private SerializedProperty glitchPeakAmount;
        private SerializedProperty preset;

        private SerializedProperty shader;

        private bool referencesFoldout = true;
        private bool startupFoldout = true;
        private bool directionFoldout = true;
        private bool enterFoldout = true;
        private bool exitFoldout = true;
        private bool transformFoldout = true;
        private bool alphaFoldout = true;
        private bool glitchAnimationFoldout = true;
        private bool shaderFoldout = true;

        private bool coreShaderFoldout = true;
        private bool scanShaderFoldout = true;
        private bool edgeShaderFoldout = true;
        private bool dissolveShaderFoldout;
        private bool glitchShaderFoldout;
        private bool chromaticShaderFoldout;
        private bool flickerShaderFoldout;
        

        private GUIStyle sectionHeaderStyle;
        private GUIStyle smallButtonStyle;

        private void OnEnable()
        {
            targetGraphic =
                serializedObject.FindProperty(
                    "targetGraphic");

            targetRectTransform =
                serializedObject.FindProperty(
                    "targetRectTransform");

            canvasGroup =
                serializedObject.FindProperty(
                    "canvasGroup");

            playOnEnable =
                serializedObject.FindProperty(
                    "playOnEnable");

            startHidden =
                serializedObject.FindProperty(
                    "startHidden");

            instantiateMaterial =
                serializedObject.FindProperty(
                    "instantiateMaterial");

            useUnscaledTime =
                serializedObject.FindProperty(
                    "useUnscaledTime");

            direction =
                serializedObject.FindProperty(
                    "direction");

            enter =
                serializedObject.FindProperty(
                    "enter");

            exit =
                serializedObject.FindProperty(
                    "exit");

            animatePosition =
                serializedObject.FindProperty(
                    "animatePosition");

            animateScale =
                serializedObject.FindProperty(
                    "animateScale");

            animateRotation =
                serializedObject.FindProperty(
                    "animateRotation");

            positionOffset =
                serializedObject.FindProperty(
                    "positionOffset");

            hiddenScale =
                serializedObject.FindProperty(
                    "hiddenScale");

            hiddenRotation =
                serializedObject.FindProperty(
                    "hiddenRotation");

            preserveInitialTransform =
                serializedObject.FindProperty(
                    "preserveInitialTransform");

            animateCanvasGroup =
                serializedObject.FindProperty(
                    "animateCanvasGroup");

            hiddenAlpha =
                serializedObject.FindProperty(
                    "hiddenAlpha");

            useGlitchEnvelope =
                serializedObject.FindProperty(
                    "useGlitchEnvelope");

            glitchEnvelope =
                serializedObject.FindProperty(
                    "glitchEnvelope");

            glitchPeakAmount =
                serializedObject.FindProperty(
                    "glitchPeakAmount");

            shader =
                serializedObject.FindProperty(
                    "shader");

                            preset =
            serializedObject.FindProperty(
                "preset");
        }
        private void DrawPresetSection()
        {
            EditorGUILayout.LabelField(
                "TRANSITION PRESET",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Preset"));

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "CLEAN",
                    smallButtonStyle))
            {
                SetPreset(
                    0);
            }

            if (GUILayout.Button(
                    "TECH",
                    smallButtonStyle))
            {
                SetPreset(
                    1);
            }

            if (GUILayout.Button(
                    "BOOT",
                    smallButtonStyle))
            {
                SetPreset(
                    2);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "GLITCH",
                    smallButtonStyle))
            {
                SetPreset(
                    3);
            }

            if (GUILayout.Button(
                    "WARNING",
                    smallButtonStyle))
            {
                SetPreset(
                    4);
            }

            if (GUILayout.Button(
                    "DIAGNOSTIC",
                    smallButtonStyle))
            {
                SetPreset(
                    5);
            }

            EditorGUILayout.EndHorizontal();
            }
            private void SetPreset(
    int index)
        {
            preset.enumValueIndex = index;
        }

        private void InitializeStyles()
        {
            if (sectionHeaderStyle != null)
            {
                return;
            }

            sectionHeaderStyle =
                new GUIStyle(
                    EditorStyles.foldoutHeader)
                {
                    fontStyle = FontStyle.Bold
                };

            smallButtonStyle =
                new GUIStyle(
                    EditorStyles.miniButton)
                {
                    fixedHeight = 24f
                };
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            DrawPresetSection();

            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(4f);

            DrawReferences();

            EditorGUILayout.Space(2f);

            DrawStartup();

            EditorGUILayout.Space(2f);

            DrawDirection();

            EditorGUILayout.Space(2f);

            DrawAnimationSection(
                "ENTER TRANSITION",
                ref enterFoldout,
                enter);

            EditorGUILayout.Space(2f);

            DrawAnimationSection(
                "EXIT TRANSITION",
                ref exitFoldout,
                exit);

            EditorGUILayout.Space(2f);

            DrawTransform();

            EditorGUILayout.Space(2f);

            DrawAlpha();

            EditorGUILayout.Space(2f);

            DrawGlitchAnimation();

            EditorGUILayout.Space(2f);

            DrawShaderSettings();

            EditorGUILayout.Space(8f);

            DrawRuntimeControls();

            serializedObject.ApplyModifiedProperties();

            DrawValidation();
        }

        private void DrawHeader()
        {
            Rect rect =
                EditorGUILayout.GetControlRect(
                    false,
                    58f);

            EditorGUI.DrawRect(
                rect,
                new Color(
                    0.055f,
                    0.07f,
                    0.08f,
                    1f));

            Rect titleRect =
                new Rect(
                    rect.x + 14f,
                    rect.y + 8f,
                    rect.width - 28f,
                    24f);

            EditorGUI.LabelField(
                titleRect,
                "PROJECT SPARK",
                new GUIStyle(
                    EditorStyles.boldLabel)
                {
                    fontSize = 11,
                    normal =
                    {
                        textColor =
                            new Color(
                                0.1f,
                                0.85f,
                                1f)
                    }
                });

            Rect subtitleRect =
                new Rect(
                    rect.x + 14f,
                    rect.y + 29f,
                    rect.width - 28f,
                    20f);

            EditorGUI.LabelField(
                subtitleRect,
                "UI TRANSITION CONTROLLER",
                new GUIStyle(
                    EditorStyles.boldLabel)
                {
                    fontSize = 15
                });
        }

        private void DrawReferences()
        {
            referencesFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    referencesFoldout,
                    "REFERENCES",
                    sectionHeaderStyle);

            if (referencesFoldout)
            {
                EditorGUILayout.PropertyField(
                    targetGraphic,
                    new GUIContent(
                        "Graphic"));

                EditorGUILayout.PropertyField(
                    targetRectTransform,
                    new GUIContent(
                        "Rect Transform"));

                EditorGUILayout.PropertyField(
                    canvasGroup,
                    new GUIContent(
                        "Canvas Group"));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawStartup()
        {
            startupFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    startupFoldout,
                    "STARTUP / RUNTIME",
                    sectionHeaderStyle);

            if (startupFoldout)
            {
                EditorGUILayout.PropertyField(
                    playOnEnable,
                    new GUIContent(
                        "Play On Enable"));

                EditorGUILayout.PropertyField(
                    startHidden,
                    new GUIContent(
                        "Start Hidden"));

                EditorGUILayout.PropertyField(
                    instantiateMaterial,
                    new GUIContent(
                        "Instantiate Material"));

                EditorGUILayout.PropertyField(
                    useUnscaledTime,
                    new GUIContent(
                        "Use Unscaled Time"));

                EditorGUILayout.Space(4f);

                EditorGUILayout.HelpBox(
                    "Instantiate Material should normally remain enabled so each UI element receives its own transition state.",
                    MessageType.Info);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawDirection()
        {
            directionFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    directionFoldout,
                    "TRANSITION DIRECTION",
                    sectionHeaderStyle);

            if (directionFoldout)
            {
                EditorGUILayout.PropertyField(
                    direction,
                    new GUIContent(
                        "Direction"));

                EditorGUILayout.Space(4f);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(
                        "← →",
                        smallButtonStyle))
                {
                    direction.enumValueIndex = 0;
                }

                if (GUILayout.Button(
                        "→ ←",
                        smallButtonStyle))
                {
                    direction.enumValueIndex = 1;
                }

                if (GUILayout.Button(
                        "↓ ↑",
                        smallButtonStyle))
                {
                    direction.enumValueIndex = 2;
                }

                if (GUILayout.Button(
                        "↑ ↓",
                        smallButtonStyle))
                {
                    direction.enumValueIndex = 3;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAnimationSection(
            string title,
            ref bool foldout,
            SerializedProperty property)
        {
            foldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    foldout,
                    title,
                    sectionHeaderStyle);

            if (foldout)
            {
                SerializedProperty duration =
                    property.FindPropertyRelative(
                        "duration");

                SerializedProperty delay =
                    property.FindPropertyRelative(
                        "delay");

                SerializedProperty transitionCurve =
                    property.FindPropertyRelative(
                        "transitionCurve");

                SerializedProperty positionCurve =
                    property.FindPropertyRelative(
                        "positionCurve");

                SerializedProperty scaleCurve =
                    property.FindPropertyRelative(
                        "scaleCurve");

                SerializedProperty rotationCurve =
                    property.FindPropertyRelative(
                        "rotationCurve");

                SerializedProperty alphaCurve =
                    property.FindPropertyRelative(
                        "alphaCurve");

                EditorGUILayout.PropertyField(
                    duration,
                    new GUIContent(
                        "Duration"));

                EditorGUILayout.PropertyField(
                    delay,
                    new GUIContent(
                        "Delay"));

                EditorGUILayout.Space(4f);

                EditorGUILayout.PropertyField(
                    transitionCurve,
                    new GUIContent(
                        "Shader Curve"));

                EditorGUILayout.PropertyField(
                    positionCurve,
                    new GUIContent(
                        "Position Curve"));

                EditorGUILayout.PropertyField(
                    scaleCurve,
                    new GUIContent(
                        "Scale Curve"));

                EditorGUILayout.PropertyField(
                    rotationCurve,
                    new GUIContent(
                        "Rotation Curve"));

                EditorGUILayout.PropertyField(
                    alphaCurve,
                    new GUIContent(
                        "Alpha Curve"));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTransform()
        {
            transformFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    transformFoldout,
                    "RECT TRANSFORM MOTION",
                    sectionHeaderStyle);

            if (transformFoldout)
            {
                EditorGUILayout.PropertyField(
                    animatePosition,
                    new GUIContent(
                        "Animate Position"));

                if (animatePosition.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        positionOffset,
                        new GUIContent(
                            "Position Offset"));
                }

                EditorGUILayout.PropertyField(
                    animateScale,
                    new GUIContent(
                        "Animate Scale"));

                if (animateScale.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        hiddenScale,
                        new GUIContent(
                            "Hidden Scale"));
                }

                EditorGUILayout.PropertyField(
                    animateRotation,
                    new GUIContent(
                        "Animate Rotation"));

                if (animateRotation.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        hiddenRotation,
                        new GUIContent(
                            "Hidden Rotation"));
                }

                EditorGUILayout.PropertyField(
                    preserveInitialTransform,
                    new GUIContent(
                        "Preserve Initial Transform"));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAlpha()
        {
            alphaFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    alphaFoldout,
                    "CANVAS ALPHA",
                    sectionHeaderStyle);

            if (alphaFoldout)
            {
                EditorGUILayout.PropertyField(
                    animateCanvasGroup,
                    new GUIContent(
                        "Animate Canvas Group"));

                if (animateCanvasGroup.boolValue)
                {
                    EditorGUILayout.Slider(
                        hiddenAlpha,
                        0f,
                        1f,
                        new GUIContent(
                            "Hidden Alpha"));
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGlitchAnimation()
        {
            glitchAnimationFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    glitchAnimationFoldout,
                    "GLITCH ANIMATION",
                    sectionHeaderStyle);

            if (glitchAnimationFoldout)
            {
                EditorGUILayout.PropertyField(
                    useGlitchEnvelope,
                    new GUIContent(
                        "Use Glitch Envelope"));

                if (useGlitchEnvelope.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        glitchEnvelope,
                        new GUIContent(
                            "Glitch Envelope"),
                        GUILayout.MinHeight(
                            55f));

                    EditorGUILayout.Slider(
                        glitchPeakAmount,
                        0f,
                        1f,
                        new GUIContent(
                            "Glitch Peak"));
                }

                EditorGUILayout.Space(4f);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(
                        "SOFT",
                        smallButtonStyle))
                {
                    SetGlitchEnvelope(
                        0.15f,
                        0.12f);
                }

                if (GUILayout.Button(
                        "TECH",
                        smallButtonStyle))
                {
                    SetGlitchEnvelope(
                        0.35f,
                        0.22f);
                }

                if (GUILayout.Button(
                        "AGGRESSIVE",
                        smallButtonStyle))
                {
                    SetGlitchEnvelope(
                        0.65f,
                        0.4f);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void SetGlitchEnvelope(
            float peak,
            float center)
        {
            glitchPeakAmount.floatValue =
                peak;

            AnimationCurve curve =
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(
                        0.12f,
                        center),
                    new Keyframe(
                        0.35f,
                        peak),
                    new Keyframe(
                        0.55f,
                        center),
                    new Keyframe(
                        0.72f,
                        peak * 0.35f),
                    new Keyframe(1f, 0f));

            glitchEnvelope.animationCurveValue =
                curve;
        }

        private void DrawShaderSettings()
        {
            shaderFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    shaderFoldout,
                    "SHADER SYSTEM",
                    sectionHeaderStyle);

            if (shaderFoldout)
            {
                DrawShaderCore();
                DrawShaderScan();
                DrawShaderEdge();
                DrawShaderDissolve();
                DrawShaderGlitch();
                DrawShaderChromatic();
                DrawShaderFlicker();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

       private void DrawShaderCore()
{
    coreShaderFoldout =
        EditorGUILayout.Foldout(
            coreShaderFoldout,
            "CORE REVEAL",
            true);

    if (!coreShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "baseColor"),
        new GUIContent(
            "Base Color"));

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "revealSoftness"),
        0f,
        1f,
        new GUIContent(
            "Reveal Softness"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "revealOffset"),
        new GUIContent(
            "Reveal Offset"));

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "revealCurve"),
        0.01f,
        5f,
        new GUIContent(
            "Reveal Curve"));

    EditorGUI.indentLevel--;
}
    
        private void DrawShaderScan()
{
    scanShaderFoldout =
        EditorGUILayout.Foldout(
            scanShaderFoldout,
            "SCAN SYSTEM",
            true);

    if (!scanShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "scanWidth"),
        new GUIContent(
            "Scan Width"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "scanIntensity"),
        new GUIContent(
            "Scan Intensity"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "scanTrail"),
        new GUIContent(
            "Scan Trail"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "scanTrailPower"),
        new GUIContent(
            "Trail Power"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "scanColor"),
        new GUIContent(
            "Scan Color"));

    EditorGUI.indentLevel--;
}
       private void DrawShaderEdge()
{
    edgeShaderFoldout =
        EditorGUILayout.Foldout(
            edgeShaderFoldout,
            "EDGE SYSTEM",
            true);

    if (!edgeShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "edgeWidth"),
        new GUIContent(
            "Edge Width"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "edgeIntensity"),
        new GUIContent(
            "Edge Intensity"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "edgeSharpness"),
        new GUIContent(
            "Edge Sharpness"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "edgeColor"),
        new GUIContent(
            "Edge Color"));

    EditorGUI.indentLevel--;
}

       private void DrawShaderDissolve()
{
    dissolveShaderFoldout =
        EditorGUILayout.Foldout(
            dissolveShaderFoldout,
            "DISSOLVE",
            true);

    if (!dissolveShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "dissolveAmount"),
        0f,
        1f,
        new GUIContent(
            "Dissolve Amount"));

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "dissolveSoftness"),
        0.0001f,
        1f,
        new GUIContent(
            "Dissolve Softness"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "dissolveScale"),
        new GUIContent(
            "Dissolve Scale"));

    EditorGUI.indentLevel--;
}
       private void DrawShaderGlitch()
{
    glitchShaderFoldout =
        EditorGUILayout.Foldout(
            glitchShaderFoldout,
            "GLITCH",
            true);

    if (!glitchShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "glitchAmount"),
        0f,
        1f,
        new GUIContent(
            "Glitch Amount"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "glitchSpeed"),
        new GUIContent(
            "Glitch Speed"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "glitchBlockSize"),
        new GUIContent(
            "Block Size"));

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "glitchThreshold"),
        0f,
        1f,
        new GUIContent(
            "Threshold"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "glitchOffset"),
        new GUIContent(
            "Horizontal Offset"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "glitchSeed"),
        new GUIContent(
            "Seed"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "glitchColor"),
        new GUIContent(
            "Glitch Color"));

    EditorGUILayout.Space(4f);

    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button(
            "RESET",
            smallButtonStyle))
    {
        shader.FindPropertyRelative(
            "glitchAmount").floatValue =
            0f;

        shader.FindPropertyRelative(
            "glitchSpeed").floatValue =
            12f;

        shader.FindPropertyRelative(
            "glitchBlockSize").floatValue =
            18f;

        shader.FindPropertyRelative(
            "glitchThreshold").floatValue =
            0.82f;

        shader.FindPropertyRelative(
            "glitchOffset").floatValue =
            0.025f;

        shader.FindPropertyRelative(
            "glitchSeed").floatValue =
            0f;
    }

    if (GUILayout.Button(
            "RANDOM SEED",
            smallButtonStyle))
    {
        shader.FindPropertyRelative(
            "glitchSeed").floatValue =
            Random.Range(
                -1000f,
                1000f);
    }

    EditorGUILayout.EndHorizontal();

    EditorGUILayout.Space(4f);

    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button(
            "SUBTLE",
            smallButtonStyle))
    {
        ApplyGlitchPreset(
            0.15f,
            10f,
            16f,
            0.88f,
            0.012f);
    }

    if (GUILayout.Button(
            "TECHNICAL",
            smallButtonStyle))
    {
        ApplyGlitchPreset(
            0.35f,
            14f,
            20f,
            0.80f,
            0.025f);
    }

    if (GUILayout.Button(
            "HARD",
            smallButtonStyle))
    {
        ApplyGlitchPreset(
            0.75f,
            22f,
            28f,
            0.65f,
            0.055f);
    }

    EditorGUILayout.EndHorizontal();

    EditorGUI.indentLevel--;
}
        private void ApplyGlitchPreset(
            float amount,
            float speed,
            float blockSize,
            float threshold,
            float offset)
        {
            shader.FindPropertyRelative(
                "glitchAmount").floatValue =
                amount;

            shader.FindPropertyRelative(
                "glitchSpeed").floatValue =
                speed;

            shader.FindPropertyRelative(
                "glitchBlockSize").floatValue =
                blockSize;

            shader.FindPropertyRelative(
                "glitchThreshold").floatValue =
                threshold;

            shader.FindPropertyRelative(
                "glitchOffset").floatValue =
                offset;
        }

private void DrawShaderChromatic()
{
    chromaticShaderFoldout =
        EditorGUILayout.Foldout(
            chromaticShaderFoldout,
            "CHROMATIC",
            true);

    if (!chromaticShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "chromaticAmount"),
        0f,
        0.1f,
        new GUIContent(
            "Chromatic Amount"));

    EditorGUI.indentLevel--;
}
        private void DrawShaderFlicker()
{
    flickerShaderFoldout =
        EditorGUILayout.Foldout(
            flickerShaderFoldout,
            "FLICKER",
            true);

    if (!flickerShaderFoldout)
    {
        return;
    }

    EditorGUI.indentLevel++;

    EditorGUILayout.Slider(
        shader.FindPropertyRelative(
            "flickerAmount"),
        0f,
        1f,
        new GUIContent(
            "Flicker Amount"));

    EditorGUILayout.PropertyField(
        shader.FindPropertyRelative(
            "flickerSpeed"),
        new GUIContent(
            "Flicker Speed"));

    EditorGUI.indentLevel--;
}

        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField(
                "EDITOR / RUNTIME CONTROLS",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "PLAY ENTER",
                    GUILayout.Height(30f)))
            {
                foreach (
                    Object targetObject
                    in targets)
                {
                    PS_UI_TransitionController controller =
                        targetObject
                        as PS_UI_TransitionController;

                    if (controller == null)
                    {
                        continue;
                    }

                    controller.PlayEnter();
                }
            }

            if (GUILayout.Button(
                    "PLAY EXIT",
                    GUILayout.Height(30f)))
            {
                foreach (
                    Object targetObject
                    in targets)
                {
                    PS_UI_TransitionController controller =
                        targetObject
                        as PS_UI_TransitionController;

                    if (controller == null)
                    {
                        continue;
                    }

                    controller.PlayExit();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "SHOW",
                    GUILayout.Height(26f)))
            {
                foreach (
                    Object targetObject
                    in targets)
                {
                    PS_UI_TransitionController controller =
                        targetObject
                        as PS_UI_TransitionController;

                    if (controller == null)
                    {
                        continue;
                    }

                    controller.SetVisibleImmediate();
                }
            }

            if (GUILayout.Button(
                    "HIDE",
                    GUILayout.Height(26f)))
            {
                foreach (
                    Object targetObject
                    in targets)
                {
                    PS_UI_TransitionController controller =
                        targetObject
                        as PS_UI_TransitionController;

                    if (controller == null)
                    {
                        continue;
                    }

                    controller.SetHiddenImmediate();
                }
            }

            if (GUILayout.Button(
                    "GLITCH BURST",
                    GUILayout.Height(26f)))
            {
                foreach (
                    Object targetObject
                    in targets)
                {
                    PS_UI_TransitionController controller =
                        targetObject
                        as PS_UI_TransitionController;

                    if (controller == null)
                    {
                        continue;
                    }

                    controller.PlayGlitchBurst();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

       private void DrawValidation()
{
    if (targetGraphic.objectReferenceValue == null)
    {
        EditorGUILayout.HelpBox(
            "Graphic reference is missing.",
            MessageType.Error);
    }

    if (targetRectTransform.objectReferenceValue == null)
    {
        EditorGUILayout.HelpBox(
            "RectTransform reference is missing.",
            MessageType.Error);
    }

    EditorGUILayout.HelpBox(
        "C# controls transition timing and RectTransform movement. Shader Graph controls the visual transition.",
        MessageType.Info);
}
    }
}

#endif