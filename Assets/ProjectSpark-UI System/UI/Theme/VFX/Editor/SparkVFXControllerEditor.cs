
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

// Explicit alias prevents conflict with UnityEngine.WSA.Application
using UnityApplication = UnityEngine.Application;

namespace ProjectSpark.VFX.Editor
{
    /// <summary>
    /// Custom Inspector for SparkVFXController.
    ///
    /// Provides:
    /// - Clean grouped Inspector
    /// - Profile controls
    /// - Renderer controls
    /// - Runtime preview controls
    /// - Quick VFX state testing
    ///
    /// This editor intentionally stays lightweight.
    /// </summary>
    [CustomEditor(typeof(SparkVFXController))]
    public sealed class SparkVFXControllerEditor
        : UnityEditor.Editor
    {
        // ============================================================
        // TARGET
        // ============================================================

        private SparkVFXController controller;


        // ============================================================
        // FOLDOUTS
        // ============================================================

        private bool showProfile = true;

        private bool showRenderers = true;

        private bool showParticles = true;

        private bool showTransition = true;

        private bool showBase = true;

        private bool showState = true;

        private bool showEffects = true;

        private bool showDebug = false;

        private bool showPreview = true;


        // ============================================================
        // PREVIEW VALUES
        // ============================================================

        private float previewGlow = 4f;

        private float previewScan = 4f;

        private float previewSweep = 5f;

        private float previewNoise = 2f;

        private float previewDissolve = 0f;

        private float previewDistortion = 0.02f;

        private float previewAlpha = 1f;


        // ============================================================
        // GUI CONTENT
        // ============================================================

        private static readonly GUIContent
            ApplyProfileContent =
                new GUIContent(
                    "Apply Profile",
                    "Apply the assigned SparkVFXProfile immediately."
                );


        private static readonly GUIContent
            RefreshRenderersContent =
                new GUIContent(
                    "Refresh Renderers",
                    "Find all Renderer components under this object."
                );


        private static readonly GUIContent
            ResetContent =
                new GUIContent(
                    "Reset VFX",
                    "Reset all runtime VFX values and states."
                );


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            controller =
                target as SparkVFXController;
        }


        // ============================================================
        // INSPECTOR
        // ============================================================

        public override void OnInspectorGUI()
        {
            if (controller == null)
            {
                return;
            }

            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(
                4f
            );

            DrawProfileSection();

            DrawRendererSection();

            DrawParticleSection();

            DrawTransitionSection();

            DrawBaseSection();

            DrawStateSection();

            DrawEffectSection();

            DrawDebugSection();

            DrawPreviewSection();

            serializedObject.ApplyModifiedProperties();
        }


        // ============================================================
        // HEADER
        // ============================================================

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(
                "HelpBox"
            );

            EditorGUILayout.LabelField(
                "PROJECT SPARK VFX",
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField(
                "Simple Runtime VFX Controller",
                EditorStyles.miniLabel
            );

            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // PROFILE
        // ============================================================

        private void DrawProfileSection()
        {
            showProfile =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showProfile,
                    "Profile"
                );

            if (showProfile)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "profile"
                    )
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "applyProfileOnAwake"
                    )
                );

                EditorGUILayout.Space(
                    3f
                );

                using (
                    new EditorGUI.DisabledScope(
                        !UnityApplication.isPlaying
                    )
                )
                {
                    if (
                        GUILayout.Button(
                            ApplyProfileContent,
                            GUILayout.Height(
                                26f
                            )
                        )
                    )
                    {
                        controller.ApplyProfile();

                        EditorUtility.SetDirty(
                            controller
                        );
                    }
                }

                if (
                    !UnityApplication.isPlaying
                )
                {
                    EditorGUILayout.HelpBox(
                        "Apply Profile is available during Play Mode.",
                        MessageType.Info
                    );
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // RENDERERS
        // ============================================================

        private void DrawRendererSection()
        {
            showRenderers =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showRenderers,
                    "Renderers"
                );

            if (showRenderers)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "targetRenderers"
                    ),
                    true
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "autoFindRenderers"
                    )
                );

                EditorGUILayout.Space(
                    3f
                );

                using (
                    new EditorGUI.DisabledScope(
                        !UnityApplication.isPlaying
                    )
                )
                {
                    if (
                        GUILayout.Button(
                            RefreshRenderersContent,
                            GUILayout.Height(
                                24f
                            )
                        )
                    )
                    {
                        controller.RefreshRenderers();
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // PARTICLES
        // ============================================================

        private void DrawParticleSection()
        {
            showParticles =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showParticles,
                    "Particles"
                );

            if (showParticles)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "sparkParticles"
                    )
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // TRANSITION
        // ============================================================

        private void DrawTransitionSection()
        {
            showTransition =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showTransition,
                    "Transition"
                );

            if (showTransition)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "transitionSpeed"
                    )
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "applyInLateUpdate"
                    )
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // BASE
        // ============================================================

        private void DrawBaseSection()
        {
            showBase =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showBase,
                    "Base"
                );

            if (showBase)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "glowIntensity"
                    )
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "alpha"
                    )
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // STATE
        // ============================================================

        private void DrawStateSection()
        {
            showState =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showState,
                    "Interaction State"
                );

            if (showState)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "hoverGlow"
                    )
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "selectedGlow"
                    )
                );

                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "warningGlow"
                    )
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // EFFECTS
        // ============================================================

        private void DrawEffectSection()
        {
            showEffects =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showEffects,
                    "Effects"
                );

            if (showEffects)
            {
                DrawProperty(
                    "pulseIntensity"
                );

                DrawProperty(
                    "pulseDuration"
                );

                EditorGUILayout.Space(
                    3f
                );

                DrawProperty(
                    "flashIntensity"
                );

                DrawProperty(
                    "flashDuration"
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // DEBUG
        // ============================================================

        private void DrawDebugSection()
        {
            showDebug =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showDebug,
                    "Debug"
                );

            if (showDebug)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(
                        "debugLogs"
                    )
                );
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // RUNTIME PREVIEW
        // ============================================================

        private void DrawPreviewSection()
        {
            showPreview =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    showPreview,
                    "Runtime Preview"
                );

            if (showPreview)
            {
                if (
                    !UnityApplication.isPlaying
                )
                {
                    EditorGUILayout.HelpBox(
                        "Enter Play Mode to preview VFX.",
                        MessageType.Info
                    );
                }


                EditorGUILayout.Space(
                    3f
                );


                // ====================================================
                // BASE
                // ====================================================

                EditorGUILayout.LabelField(
                    "Base",
                    EditorStyles.boldLabel
                );

                previewGlow =
                    EditorGUILayout.Slider(
                        "Glow",
                        previewGlow,
                        0f,
                        20f
                    );

                previewAlpha =
                    EditorGUILayout.Slider(
                        "Alpha",
                        previewAlpha,
                        0f,
                        1f
                    );


                if (
                    GUILayout.Button(
                        "Apply Base"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetGlow(
                            previewGlow
                        );

                        controller.SetAlpha(
                            previewAlpha
                        );
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // SCAN
                // ====================================================

                EditorGUILayout.LabelField(
                    "Scan",
                    EditorStyles.boldLabel
                );

                previewScan =
                    EditorGUILayout.Slider(
                        "Intensity",
                        previewScan,
                        0f,
                        20f
                    );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Enable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetScanEnabled(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Disable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetScanEnabled(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                if (
                    GUILayout.Button(
                        "Apply Scan"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetScan(
                            previewScan
                        );
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // SWEEP
                // ====================================================

                EditorGUILayout.LabelField(
                    "Sweep",
                    EditorStyles.boldLabel
                );

                previewSweep =
                    EditorGUILayout.Slider(
                        "Intensity",
                        previewSweep,
                        0f,
                        20f
                    );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Enable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetSweepEnabled(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Disable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetSweepEnabled(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                if (
                    GUILayout.Button(
                        "Apply Sweep"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetSweep(
                            previewSweep
                        );
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // NOISE
                // ====================================================

                EditorGUILayout.LabelField(
                    "Noise",
                    EditorStyles.boldLabel
                );

                previewNoise =
                    EditorGUILayout.Slider(
                        "Intensity",
                        previewNoise,
                        0f,
                        20f
                    );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Enable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetNoiseEnabled(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Disable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetNoiseEnabled(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                if (
                    GUILayout.Button(
                        "Apply Noise"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetNoise(
                            previewNoise
                        );
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // DISSOLVE
                // ====================================================

                EditorGUILayout.LabelField(
                    "Dissolve",
                    EditorStyles.boldLabel
                );

                previewDissolve =
                    EditorGUILayout.Slider(
                        "Amount",
                        previewDissolve,
                        0f,
                        1f
                    );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Enable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDissolveEnabled(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Disable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDissolveEnabled(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                if (
                    GUILayout.Button(
                        "Apply Dissolve"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDissolve(
                            previewDissolve
                        );
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // DISTORTION
                // ====================================================

                EditorGUILayout.LabelField(
                    "Distortion",
                    EditorStyles.boldLabel
                );

                previewDistortion =
                    EditorGUILayout.Slider(
                        "Strength",
                        previewDistortion,
                        0f,
                        1f
                    );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Enable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDistortionEnabled(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Disable"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDistortionEnabled(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                if (
                    GUILayout.Button(
                        "Apply Distortion"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetDistortion(
                            previewDistortion
                        );
                    }
                }


                EditorGUILayout.Space(
                    8f
                );


                // ====================================================
                // INTERACTION
                // ====================================================

                EditorGUILayout.LabelField(
                    "Interaction",
                    EditorStyles.boldLabel
                );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Hover ON"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetHovered(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Hover OFF"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetHovered(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Selected ON"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetSelected(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Selected OFF"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetSelected(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Warning ON"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetWarning(
                            true
                        );
                    }
                }


                if (
                    GUILayout.Button(
                        "Warning OFF"
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.SetWarning(
                            false
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space(
                    8f
                );


                // ====================================================
                // ONE-SHOT EFFECTS
                // ====================================================

                EditorGUILayout.LabelField(
                    "One-Shot Effects",
                    EditorStyles.boldLabel
                );


                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button(
                        "Pulse",
                        GUILayout.Height(
                            28f
                        )
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.PlayPulse();
                    }
                }


                if (
                    GUILayout.Button(
                        "Flash",
                        GUILayout.Height(
                            28f
                        )
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.PlayFlash();
                    }
                }


                if (
                    GUILayout.Button(
                        "Spark",
                        GUILayout.Height(
                            28f
                        )
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.PlaySpark();
                    }
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space(
                    3f
                );


                if (
                    GUILayout.Button(
                        "ERROR EFFECT",
                        GUILayout.Height(
                            30f
                        )
                    )
                )
                {
                    if (
                        UnityApplication.isPlaying
                    )
                    {
                        controller.PlayError();
                    }
                }


                EditorGUILayout.Space(
                    5f
                );


                // ====================================================
                // RESET
                // ====================================================

                using (
                    new EditorGUI.DisabledScope(
                        !UnityApplication.isPlaying
                    )
                )
                {
                    if (
                        GUILayout.Button(
                            ResetContent,
                            GUILayout.Height(
                                28f
                            )
                        )
                    )
                    {
                        controller.ResetVFX();
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        // ============================================================
        // PROPERTY HELPER
        // ============================================================

        private void DrawProperty(
            string propertyName)
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                EditorGUILayout.PropertyField(
                    property
                );
            }
        }
    }
}

#endif
