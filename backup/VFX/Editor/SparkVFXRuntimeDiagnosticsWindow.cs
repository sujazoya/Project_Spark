#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Project Spark UI VFX Runtime Diagnostics Window.
    ///
    /// Editor-only diagnostic window for inspecting a selected
    /// Spark VFX hierarchy.
    ///
    /// This window does not modify VFX state.
    /// It does not call:
    /// - SetState()
    /// - RequestState()
    /// - Initialize()
    /// - Shader property setters
    ///
    /// It only reads the current runtime configuration.
    /// </summary>
    public sealed class SparkVFXRuntimeDiagnosticsWindow
        : EditorWindow
    {
        // ============================================================
        // WINDOW
        // ============================================================

        private Vector2 scrollPosition;


        // ============================================================
        // TARGET
        // ============================================================

        private GameObject selectedObject;


        private SparkVFXRuntimeDiagnostics diagnostics;


        private SparkVFXTarget target;


        private SparkVFXController imageController;


        private SparkTMPVFXController tmpController;


        private SparkVFXLayeredStateMachine stateMachine;


        private SparkVFXSequencePlayer sequencePlayer;


        private SparkVFXLoop loop;


        private SparkVFXUIEvents uiEvents;


        private SparkVFXEditorDebugger debugger;


        // ============================================================
        // STATUS
        // ============================================================

        private GUIStyle headerStyle;


        private GUIStyle sectionStyle;


        private GUIStyle successStyle;


        private GUIStyle warningStyle;


        private GUIStyle errorStyle;


        private GUIStyle normalStyle;


        // ============================================================
        // MENU
        // ============================================================

        [MenuItem(
            "Project Spark/UI VFX/Runtime Diagnostics"
        )]
        public static void Open()
        {
            SparkVFXRuntimeDiagnosticsWindow window =
                GetWindow<
                    SparkVFXRuntimeDiagnosticsWindow
                >();

            window.titleContent =
                new GUIContent(
                    "Spark VFX Diagnostics"
                );

            window.minSize =
                new Vector2(
                    480f,
                    500f
                );

            window.Show();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            CreateStyles();


            Selection.selectionChanged +=
                OnSelectionChanged;


            OnSelectionChanged();
        }


        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            Selection.selectionChanged -=
                OnSelectionChanged;
        }


        // ============================================================
        // STYLES
        // ============================================================

        private void CreateStyles()
        {
            headerStyle =
                new GUIStyle(
                    EditorStyles.boldLabel
                )
                {
                    fontSize = 18,
                    alignment =
                        TextAnchor.MiddleLeft
                };


            sectionStyle =
                new GUIStyle(
                    EditorStyles.boldLabel
                )
                {
                    fontSize = 13
                };


            successStyle =
                new GUIStyle(
                    EditorStyles.label
                );


            successStyle.normal.textColor =
                new Color(
                    0.2f,
                    0.75f,
                    0.3f
                );


            warningStyle =
                new GUIStyle(
                    EditorStyles.label
                );


            warningStyle.normal.textColor =
                new Color(
                    0.95f,
                    0.65f,
                    0.15f
                );


            errorStyle =
                new GUIStyle(
                    EditorStyles.label
                );


            errorStyle.normal.textColor =
                new Color(
                    0.95f,
                    0.25f,
                    0.25f
                );


            normalStyle =
                new GUIStyle(
                    EditorStyles.label
                );
        }


        // ============================================================
        // SELECTION
        // ============================================================

        private void OnSelectionChanged()
        {
            GameObject selection =
                Selection.activeGameObject;


            if (selection == null)
            {
                return;
            }


            SparkVFXRuntimeDiagnostics foundDiagnostics =
                FindInHierarchy<
                    SparkVFXRuntimeDiagnostics
                >(
                    selection
                );


            if (foundDiagnostics == null)
            {
                return;
            }


            selectedObject =
                selection;


            diagnostics =
                foundDiagnostics;


            ResolveComponents();


            Repaint();
        }


        // ============================================================
        // FIND COMPONENT
        // ============================================================

        private T FindInHierarchy<T>(
            GameObject root)
            where T : Component
        {
            if (root == null)
            {
                return null;
            }


            T component =
                root.GetComponent<T>();


            if (component != null)
            {
                return component;
            }


            component =
                root.GetComponentInChildren<T>(
                    true
                );


            if (component != null)
            {
                return component;
            }


            component =
                root.GetComponentInParent<T>(
                    true
                );


            return component;
        }


        // ============================================================
        // RESOLVE
        // ============================================================

        private void ResolveComponents()
        {
            if (selectedObject == null)
            {
                return;
            }


            target =
                FindInHierarchy<
                    SparkVFXTarget
                >(
                    selectedObject
                );


            imageController =
                FindInHierarchy<
                    SparkVFXController
                >(
                    selectedObject
                );


            tmpController =
                FindInHierarchy<
                    SparkTMPVFXController
                >(
                    selectedObject
                );


            stateMachine =
                FindInHierarchy<
                    SparkVFXLayeredStateMachine
                >(
                    selectedObject
                );


            sequencePlayer =
                FindInHierarchy<
                    SparkVFXSequencePlayer
                >(
                    selectedObject
                );


            loop =
                FindInHierarchy<
                    SparkVFXLoop
                >(
                    selectedObject
                );


            uiEvents =
                FindInHierarchy<
                    SparkVFXUIEvents
                >(
                    selectedObject
                );


            debugger =
                FindInHierarchy<
                    SparkVFXEditorDebugger
                >(
                    selectedObject
                );
        }


        // ============================================================
        // GUI
        // ============================================================

        private void OnGUI()
        {
            if (
                headerStyle == null
            )
            {
                CreateStyles();
            }


            DrawHeader();


            DrawTargetSelection();


            EditorGUILayout.Space(
                8f
            );


            if (diagnostics == null)
            {
                DrawNoDiagnostics();


                return;
            }


            scrollPosition =
                EditorGUILayout.BeginScrollView(
                    scrollPosition
                );


            DrawOverallStatus();


            EditorGUILayout.Space(
                8f
            );


            DrawControllerSection();


            EditorGUILayout.Space(
                8f
            );


            DrawRuntimeComponents();


            EditorGUILayout.Space(
                8f
            );


            DrawMaterialSection();


            EditorGUILayout.Space(
                8f
            );


            DrawActions();


            EditorGUILayout.EndScrollView();
        }


        // ============================================================
        // HEADER
        // ============================================================

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar
            );


            GUILayout.Label(
                "Project Spark UI VFX",
                headerStyle
            );


            GUILayout.FlexibleSpace();


            if (
                GUILayout.Button(
                    "Refresh",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(
                        70f
                    )
                )
            )
            {
                ResolveComponents();


                Repaint();
            }


            EditorGUILayout.EndHorizontal();
        }


        // ============================================================
        // TARGET SELECTION
        // ============================================================

        private void DrawTargetSelection()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Diagnostics Target",
                sectionStyle
            );


            GameObject newObject =
                (GameObject)
                EditorGUILayout.ObjectField(
                    "GameObject",
                    selectedObject,
                    typeof(
                        GameObject
                    ),
                    true
                );


            if (
                newObject !=
                selectedObject
            )
            {
                selectedObject =
                    newObject;


                if (
                    selectedObject != null
                )
                {
                    diagnostics =
                        FindInHierarchy<
                            SparkVFXRuntimeDiagnostics
                        >(
                            selectedObject
                        );


                    ResolveComponents();
                }
                else
                {
                    diagnostics =
                        null;
                }
            }


            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // NO DIAGNOSTICS
        // ============================================================

        private void DrawNoDiagnostics()
        {
            EditorGUILayout.HelpBox(
                "No SparkVFXRuntimeDiagnostics component found. " +
                "Add SparkVFXRuntimeDiagnostics to the VFX hierarchy " +
                "and select that GameObject.",
                MessageType.Info
            );
        }


        // ============================================================
        // OVERALL STATUS
        // ============================================================

        private void DrawOverallStatus()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Overall Runtime Status",
                sectionStyle
            );


            if (
                diagnostics.LastHealthyState
            )
            {
                EditorGUILayout.LabelField(
                    "● HEALTHY",
                    successStyle
                );
            }
            else
            {
                EditorGUILayout.LabelField(
                    "● ERRORS DETECTED",
                    errorStyle
                );
            }


            EditorGUILayout.Space(
                4f
            );


            EditorGUILayout.LabelField(
                "Last Diagnostic Result"
            );


            EditorGUILayout.SelectableLabel(
                diagnostics.LastDiagnosticMessage,
                GUILayout.MinHeight(
                    80f
                )
            );


            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // CONTROLLER
        // ============================================================

        private void DrawControllerSection()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Controller Resolution",
                sectionStyle
            );


            DrawComponentStatus(
                "SparkVFXTarget",
                target
            );


            DrawComponentStatus(
                "SparkVFXController",
                imageController
            );


            DrawComponentStatus(
                "SparkTMPVFXController",
                tmpController
            );


            if (
                target != null
            )
            {
                ISparkVFXController controller =
                    target.Controller;


                if (
                    controller != null
                )
                {
                    EditorGUILayout.Space(
                        4f
                    );


                    EditorGUILayout.LabelField(
                        "Resolved Interface",
                        successStyle
                    );


                    EditorGUILayout.LabelField(
                        controller.GetType().Name
                    );
                }
                else
                {
                    EditorGUILayout.LabelField(
                        "Resolved Interface",
                        errorStyle
                    );


                    EditorGUILayout.LabelField(
                        "NULL"
                    );
                }
            }


            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // RUNTIME COMPONENTS
        // ============================================================

        private void DrawRuntimeComponents()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Runtime Components",
                sectionStyle
            );


            DrawComponentStatus(
                "Layered State Machine",
                stateMachine
            );


            DrawComponentStatus(
                "Sequence Player",
                sequencePlayer
            );


            DrawComponentStatus(
                "VFX Loop",
                loop
            );


            DrawComponentStatus(
                "UI Events",
                uiEvents
            );


            DrawComponentStatus(
                "Editor Debugger",
                debugger
            );


            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // MATERIAL
        // ============================================================

        private void DrawMaterialSection()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Material / Shader",
                sectionStyle
            );


            Material material =
                null;


            if (
                imageController != null
            )
            {
                material =
                    imageController.RuntimeMaterial;
            }


            if (
                material == null &&
                tmpController != null
            )
            {
                material =
                    tmpController.RuntimeMaterial;
            }


            if (
                material == null
            )
            {
                EditorGUILayout.LabelField(
                    "Runtime Material",
                    errorStyle
                );


                EditorGUILayout.LabelField(
                    "NULL"
                );


                EditorGUILayout.EndVertical();


                return;
            }


            DrawObjectField(
                "Runtime Material",
                material
            );


            Shader shader =
                material.shader;


            if (
                shader == null
            )
            {
                EditorGUILayout.LabelField(
                    "Shader",
                    errorStyle
                );


                EditorGUILayout.LabelField(
                    "NULL"
                );


                EditorGUILayout.EndVertical();


                return;
            }


            DrawObjectField(
                "Shader",
                shader
            );


            EditorGUILayout.Space(
                5f
            );


            DrawShaderProperty(
                material,
                "_Glow"
            );


            DrawShaderProperty(
                material,
                "_Scan"
            );


            DrawShaderProperty(
                material,
                "_Sweep"
            );


            DrawShaderProperty(
                material,
                "_Flash"
            );


            DrawShaderProperty(
                material,
                "_Glitch"
            );


            DrawShaderProperty(
                material,
                "_Flicker"
            );


            DrawShaderProperty(
                material,
                "_Dissolve"
            );


            DrawShaderProperty(
                material,
                "_Reveal"
            );


            DrawShaderProperty(
                material,
                "_SweepPosition"
            );


            EditorGUILayout.EndVertical();
        }


        // ============================================================
        // SHADER PROPERTY
        // ============================================================

        private void DrawShaderProperty(
            Material material,
            string propertyName)
        {
            if (
                material == null
            )
            {
                return;
            }


            if (
                material.HasProperty(
                    propertyName
                )
            )
            {
                float value =
                    material.GetFloat(
                        propertyName
                    );


                EditorGUILayout.BeginHorizontal();


                EditorGUILayout.LabelField(
                    "✓ " +
                    propertyName,
                    successStyle
                );


                EditorGUILayout.LabelField(
                    value.ToString(
                        "0.000"
                    ),
                    GUILayout.Width(
                        70f
                    )
                );


                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField(
                    "✗ " +
                    propertyName +
                    "  MISSING",
                    errorStyle
                );
            }
        }


        // ============================================================
        // COMPONENT STATUS
        // ============================================================

        private void DrawComponentStatus(
            string label,
            Object component)
        {
            EditorGUILayout.BeginHorizontal();


            if (
                component != null
            )
            {
                EditorGUILayout.LabelField(
                    "✓ " +
                    label,
                    successStyle
                );
            }
            else
            {
                EditorGUILayout.LabelField(
                    "✗ " +
                    label,
                    warningStyle
                );
            }


            GUILayout.FlexibleSpace();


            if (
                component != null
            )
            {
                if (
                    GUILayout.Button(
                        "Select",
                        GUILayout.Width(
                            55f
                        )
                    )
                )
                {
                    Selection.activeObject =
                        component;
                }
            }


            EditorGUILayout.EndHorizontal();
        }


        // ============================================================
        // OBJECT FIELD
        // ============================================================

        private void DrawObjectField(
            string label,
            Object value)
        {
            EditorGUILayout.ObjectField(
                label,
                value,
                value != null
                    ? value.GetType()
                    : typeof(
                        Object
                    ),
                false
            );
        }


        // ============================================================
        // ACTIONS
        // ============================================================

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(
                "box"
            );


            EditorGUILayout.LabelField(
                "Diagnostics Actions",
                sectionStyle
            );


            EditorGUILayout.BeginHorizontal();


            if (
                GUILayout.Button(
                    "Run Diagnostics"
                )
            )
            {
                if (
                    diagnostics != null
                )
                {
                    diagnostics.RunDiagnostics();
                }
            }


            if (
                GUILayout.Button(
                    "Refresh References"
                )
            )
            {
                if (
                    diagnostics != null
                )
                {
                    diagnostics.RefreshReferences();
                }


                ResolveComponents();


                Repaint();
            }


            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(
                4f
            );


            if (
                GUILayout.Button(
                    "Select Diagnostics Component"
                )
            )
            {
                if (
                    diagnostics != null
                )
                {
                    Selection.activeObject =
                        diagnostics;
                }
            }


            EditorGUILayout.EndVertical();
        }
    }
}

#endif