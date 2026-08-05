#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Editor
{
    /// <summary>
    /// Custom Unity Inspector for the Project Spark ButtonDesigner.
    ///
    /// Provides:
    /// - Global Theme support
    /// - Local Theme Color override
    /// - Main Theme inspection
    /// - Neon Controller detection
    /// - Neon Theme application
    /// - TMP child theme configuration
    /// - Preview State control
    /// - Corner configuration
    /// - Image assignment
    /// - Automatic Image assignment
    /// - Random corner generation
    /// - Live Scene View preview
    /// </summary>
    [CustomEditor(typeof(ButtonDesigner))]
    public sealed class ButtonDesignerEditor :
    UnityEditor.Editor
    {
#region Serialized Properties

    private SerializedProperty themeColor;

        private SerializedProperty useGlobalThemeColor;

        private SerializedProperty previewState;

        private SerializedProperty cornerMode;

        private SerializedProperty cornerActiveCount;

        private SerializedProperty leftTopActive;

        private SerializedProperty rightTopActive;

        private SerializedProperty leftBottomActive;

        private SerializedProperty rightBottomActive;

        private SerializedProperty backgroundImage;

        private SerializedProperty applyThemeToChildTMP;

        private SerializedProperty topImage;

        private SerializedProperty bottomImage;

        private SerializedProperty leftImage;

        private SerializedProperty rightImage;

        private SerializedProperty leftTopImage;

        private SerializedProperty rightTopImage;

        private SerializedProperty leftBottomImage;

        private SerializedProperty rightBottomImage;

        #endregion


        #region Styles

        private GUIStyle sectionHeaderStyle;

        private GUIStyle toolbarButtonStyle;

        #endregion


        #region Unity Lifecycle

        private void OnEnable()
        {
            if (serializedObject == null)
            {
                return;
            }

            CacheProperties();

            // Do not access EditorStyles here.
            // Unity's GUI styles may not be initialized yet.
            sectionHeaderStyle = null;
            toolbarButtonStyle = null;
        }


        /// <summary>
        /// Draws the custom ButtonDesigner inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                return;
            }

            if (serializedObject == null)
            {
                return;
            }

            if (sectionHeaderStyle == null ||
                toolbarButtonStyle == null)
            {
                CreateStyles();
            }

            serializedObject.Update();

            ButtonDesigner designer =
                target as ButtonDesigner;

            if (designer == null)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawToolbar(
                designer);

            EditorGUILayout.Space(5f);

            DrawMasterThemeSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawThemeSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawStateSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawNeonSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawTMPSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawCornerSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawImagesSection(
                designer);

            EditorGUILayout.Space(5f);

            DrawOptionsSection(
                designer);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion


        #region Property Cache

        /// <summary>
        /// Finds and caches all serialized properties.
        /// </summary>
        private void CacheProperties()
        {
            themeColor =
                serializedObject.FindProperty(
                    "themeColor");

            useGlobalThemeColor =
                serializedObject.FindProperty(
                    "useGlobalThemeColor");

            previewState =
                serializedObject.FindProperty(
                    "previewState");

            cornerMode =
                serializedObject.FindProperty(
                    "cornerMode");

            cornerActiveCount =
                serializedObject.FindProperty(
                    "cornerActiveCount");

            leftTopActive =
                serializedObject.FindProperty(
                    "leftTopActive");

            rightTopActive =
                serializedObject.FindProperty(
                    "rightTopActive");

            leftBottomActive =
                serializedObject.FindProperty(
                    "leftBottomActive");

            rightBottomActive =
                serializedObject.FindProperty(
                    "rightBottomActive");

            backgroundImage =
                serializedObject.FindProperty(
                    "backgroundImage");

            applyThemeToChildTMP =
                serializedObject.FindProperty(
                    "applyThemeToChildTMP");

            topImage =
                serializedObject.FindProperty(
                    "topImage");

            bottomImage =
                serializedObject.FindProperty(
                    "bottomImage");

            leftImage =
                serializedObject.FindProperty(
                    "leftImage");

            rightImage =
                serializedObject.FindProperty(
                    "rightImage");

            leftTopImage =
                serializedObject.FindProperty(
                    "leftTopImage");

            rightTopImage =
                serializedObject.FindProperty(
                    "rightTopImage");

            leftBottomImage =
                serializedObject.FindProperty(
                    "leftBottomImage");

            rightBottomImage =
                serializedObject.FindProperty(
                    "rightBottomImage");
        }

        #endregion


        #region Toolbar

        /// <summary>
        /// Draws the main editor toolbar.
        /// </summary>
        private void DrawToolbar(
     ButtonDesigner designer)
        {
            if (designer == null)
            {
                return;
            }

            if (toolbarButtonStyle == null)
            {
                CreateStyles();
            }

            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar);

            if (GUILayout.Button(
                    "Apply",
                    toolbarButtonStyle))
            {
                ApplyChanges(
                    designer);
            }

            if (GUILayout.Button(
                    "Refresh",
                    toolbarButtonStyle))
            {
                Refresh(
                    designer);
            }

            if (GUILayout.Button(
                    "Randomize",
                    toolbarButtonStyle))
            {
                Randomize(
                    designer);
            }

            if (GUILayout.Button(
                    "Auto Assign",
                    toolbarButtonStyle))
            {
                AutoAssign(
                    designer);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion


        #region Master Theme

        /// <summary>
        /// Draws the global Master Theme section.
        /// </summary>
        private void DrawMasterThemeSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Master Theme");

            ThemeManager manager =
                ThemeManager.Instance;

            if (manager == null)
            {
                EditorGUILayout.HelpBox(
                    "ThemeManager is not available in the current scene.",
                    MessageType.Warning);

                return;
            }

            UITheme activeTheme =
                manager.ActiveTheme;

            if (activeTheme == null)
            {
                EditorGUILayout.HelpBox(
                    "No Active UITheme is assigned to ThemeManager.",
                    MessageType.Warning);

                return;
            }

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "Active Theme",
                activeTheme.ThemeName);

            ThemeColor globalColor =
                manager.GlobalThemeColor;

            EditorGUILayout.LabelField(
                "Global Color",
                globalColor.ToString());

            EditorGUILayout.Space(3f);

            if (useGlobalThemeColor != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(
                    useGlobalThemeColor,
                    new GUIContent(
                        "Use Global Theme Color"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    ApplyChanges(
                        designer);
                }
            }

            if (GUILayout.Button(
                    "Apply Global Theme"))
            {
                ApplyChanges(
                    designer);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion


        #region Theme

        /// <summary>
        /// Draws the local theme configuration.
        /// </summary>
        private void DrawThemeSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Theme");

            bool useGlobal =
                useGlobalThemeColor != null &&
                useGlobalThemeColor.boolValue;

            using (new EditorGUI.DisabledScope(
                useGlobal))
            {
                EditorGUI.BeginChangeCheck();

                if (themeColor != null)
                {
                    EditorGUILayout.PropertyField(
                        themeColor,
                        new GUIContent(
                            "Local Theme Color"));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    ApplyChanges(
                        designer);
                }
            }

            if (useGlobal)
            {
                EditorGUILayout.HelpBox(
                    "This button follows the global Theme Color controlled by ThemeManager.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This button uses its own Local Theme Color.",
                    MessageType.None);
            }
        }

        #endregion


        #region State

        /// <summary>
        /// Draws the visual state preview section.
        /// </summary>
        private void DrawStateSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "State");

            EditorGUI.BeginChangeCheck();

            if (previewState != null)
            {
                EditorGUILayout.PropertyField(
                    previewState,
                    new GUIContent(
                        "Preview State"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                ApplyChanges(
                    designer);
            }

            EditorGUILayout.HelpBox(
                "State changes brightness only. The selected theme color remains unchanged.",
                MessageType.None);
        }

        #endregion


        #region Neon

        /// <summary>
        /// Draws the Advanced Neon Controller section.
        /// </summary>
        private void DrawNeonSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Neon Background");

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            if (backgroundImage != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(
                    backgroundImage,
                    new GUIContent(
                        "Background Image"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    ApplyChanges(
                        designer);
                }
            }

            EditorGUILayout.Space(3f);

            if (GUILayout.Button(
                    "Auto Find Background"))
            {
                Undo.RecordObject(
                    designer,
                    "Auto Find Button Background");

                designer.AutoFindBackground();

                MarkDirty(
                    designer);

                serializedObject.Update();

                SceneView.RepaintAll();

                Repaint();
            }

            EditorGUILayout.Space(4f);

            UI_Advanced_Neon_Controller neon =
                designer.GetBackgroundNeonController();

            if (neon != null)
            {
                EditorGUILayout.HelpBox(
                    "UI_Advanced_Neon_Controller detected on Button Background.",
                    MessageType.Info);

                EditorGUILayout.ObjectField(
                    "Neon Controller",
                    neon,
                    typeof(
                        UI_Advanced_Neon_Controller),
                    true);

                EditorGUILayout.Space(3f);

                if (GUILayout.Button(
                        "Apply Theme Neon"))
                {
                    ApplyNeon(
                        designer,
                        neon);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No UI_Advanced_Neon_Controller found on the assigned Background Image.",
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion


        #region TMP

        /// <summary>
        /// Draws the TMP theme section.
        /// </summary>
        private void DrawTMPSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "TMP Pro");

            if (applyThemeToChildTMP != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(
                    applyThemeToChildTMP,
                    new GUIContent(
                        "Apply Child TMP"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    ApplyChanges(
                        designer);
                }
            }

            EditorGUILayout.HelpBox(
                "When enabled, child TMP_Text components receive the TMP Font Asset assigned to the active Theme Color.",
                MessageType.None);
        }

        #endregion


        #region Corners

        /// <summary>
        /// Draws the corner configuration section.
        /// </summary>
        private void DrawCornerSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Corner");

            EditorGUI.BeginChangeCheck();

            if (cornerMode != null)
            {
                EditorGUILayout.PropertyField(
                    cornerMode,
                    new GUIContent(
                        "Corner Mode"));
            }

            CornerMode mode =
                cornerMode != null
                    ? (CornerMode)
                        cornerMode.enumValueIndex
                    : CornerMode.CustomActive;

            if (mode ==
                CornerMode.RandomActive)
            {
                if (cornerActiveCount != null)
                {
                    EditorGUILayout.IntSlider(
                        cornerActiveCount,
                        1,
                        4,
                        new GUIContent(
                            "Active Count"));
                }
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Custom Active Corners",
                    EditorStyles.boldLabel);

                if (leftTopActive != null)
                {
                    EditorGUILayout.PropertyField(
                        leftTopActive,
                        new GUIContent(
                            "Left Top"));
                }

                if (rightTopActive != null)
                {
                    EditorGUILayout.PropertyField(
                        rightTopActive,
                        new GUIContent(
                            "Right Top"));
                }

                if (leftBottomActive != null)
                {
                    EditorGUILayout.PropertyField(
                        leftBottomActive,
                        new GUIContent(
                            "Left Bottom"));
                }

                if (rightBottomActive != null)
                {
                    EditorGUILayout.PropertyField(
                        rightBottomActive,
                        new GUIContent(
                            "Right Bottom"));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (mode ==
                    CornerMode.RandomActive)
                {
                    designer.RandomizeCorners();
                }

                designer.ApplyCornerVisibility();

                MarkDirty(
                    designer);

                SceneView.RepaintAll();

                Repaint();
            }
        }

        #endregion


        #region Images

        /// <summary>
        /// Draws all decorative button image slots.
        /// </summary>
        private void DrawImagesSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Images");

            EditorGUI.BeginChangeCheck();

            DrawImageProperty(
                topImage,
                "Top");

            DrawImageProperty(
                bottomImage,
                "Bottom");

            DrawImageProperty(
                leftImage,
                "Left");

            DrawImageProperty(
                rightImage,
                "Right");

            EditorGUILayout.Space(3f);

            DrawImageProperty(
                leftTopImage,
                "Left Top");

            DrawImageProperty(
                rightTopImage,
                "Right Top");

            DrawImageProperty(
                leftBottomImage,
                "Left Bottom");

            DrawImageProperty(
                rightBottomImage,
                "Right Bottom");

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                ApplyChanges(
                    designer);
            }
        }

        #endregion


        #region Options

        /// <summary>
        /// Draws additional ButtonDesigner options.
        /// </summary>
        private void DrawOptionsSection(
            ButtonDesigner designer)
        {
            DrawHeader(
                "Options");

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.HelpBox(
                "Auto Assign searches child Images by GameObject name and assigns them to the correct ButtonDesigner image slot.",
                MessageType.Info);

            if (GUILayout.Button(
                    "Auto Assign Images"))
            {
                AutoAssign(
                    designer);
            }

            EditorGUILayout.Space(3f);

            if (GUILayout.Button(
                    "Apply Full Theme"))
            {
                ApplyChanges(
                    designer);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion


        #region Actions

        /// <summary>
        /// Applies the complete theme and corner state.
        /// </summary>
        private void ApplyChanges(
            ButtonDesigner designer)
        {
            if (designer == null)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();

            Undo.RecordObject(
                designer,
                "Apply Button Theme");

            designer.ApplyTheme();

            designer.ApplyCornerVisibility();

            MarkDirty(
                designer);

            SceneView.RepaintAll();

            Repaint();
        }


        /// <summary>
        /// Refreshes the complete editor preview.
        /// </summary>
        private void Refresh(
            ButtonDesigner designer)
        {
            if (designer == null)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();

            designer.RefreshEditorPreview();

            designer.ApplyCornerVisibility();

            MarkDirty(
                designer);

            SceneView.RepaintAll();

            Repaint();
        }


        /// <summary>
        /// Randomizes the active corners.
        /// </summary>
        private void Randomize(
            ButtonDesigner designer)
        {
            if (designer == null)
            {
                return;
            }

            if (cornerMode != null)
            {
                cornerMode.enumValueIndex =
                    (int)
                    CornerMode.RandomActive;
            }

            serializedObject.ApplyModifiedProperties();

            Undo.RecordObject(
                designer,
                "Randomize Button Corners");

            designer.RandomizeCorners();

            designer.ApplyCornerVisibility();

            MarkDirty(
                designer);

            SceneView.RepaintAll();

            Repaint();
        }


        /// <summary>
        /// Applies the active theme's neon configuration
        /// to the Button Background controller.
        /// </summary>
        private void ApplyNeon(
            ButtonDesigner designer,
            UI_Advanced_Neon_Controller neon)
        {
            if (designer == null ||
                neon == null)
            {
                return;
            }

            Undo.RecordObject(
                neon,
                "Apply Button Neon Theme");

            designer.ApplyNeonThemePreview();

            MarkDirty(
                neon);

            SceneView.RepaintAll();

            Repaint();
        }

        #endregion


        #region Auto Assign

        /// <summary>
        /// Automatically assigns Images by GameObject name.
        /// </summary>
        private void AutoAssign(
            ButtonDesigner designer)
        {
            if (designer == null)
            {
                return;
            }

            Undo.RecordObject(
                designer,
                "Auto Assign Button Images");

            Image[] images =
                designer.GetComponentsInChildren<Image>(
                    true);

            if (images == null)
            {
                return;
            }

            for (int i = 0;
                 i < images.Length;
                 i++)
            {
                Image image =
                    images[i];

                if (image == null)
                {
                    continue;
                }

                string objectName =
                    image.gameObject.name
                        .Trim()
                        .ToLowerInvariant();

                AssignImageByName(
                    designer,
                    objectName,
                    image);
            }

            designer.AutoFindBackground();

            MarkDirty(
                designer);

            designer.ApplyTheme();

            designer.ApplyCornerVisibility();

            serializedObject.Update();

            SceneView.RepaintAll();

            Repaint();
        }


        /// <summary>
        /// Assigns an Image to the appropriate ButtonDesigner slot.
        /// </summary>
        private static void AssignImageByName(
            ButtonDesigner designer,
            string objectName,
            Image image)
        {
            if (ContainsName(
                    objectName,
                    "left top",
                    "lefttop",
                    "lt"))
            {
                designer.SetImage(
                    ButtonImageSlot.LeftTop,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "right top",
                    "righttop",
                    "rt"))
            {
                designer.SetImage(
                    ButtonImageSlot.RightTop,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "left bottom",
                    "leftbottom",
                    "lb"))
            {
                designer.SetImage(
                    ButtonImageSlot.LeftBottom,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "right bottom",
                    "rightbottom",
                    "rb"))
            {
                designer.SetImage(
                    ButtonImageSlot.RightBottom,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "top"))
            {
                designer.SetImage(
                    ButtonImageSlot.Top,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "bottom"))
            {
                designer.SetImage(
                    ButtonImageSlot.Bottom,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "left"))
            {
                designer.SetImage(
                    ButtonImageSlot.Left,
                    image);

                return;
            }

            if (ContainsName(
                    objectName,
                    "right"))
            {
                designer.SetImage(
                    ButtonImageSlot.Right,
                    image);
            }
        }

        #endregion


        #region Drawing Helpers

        /// <summary>
        /// Draws a serialized Image property.
        /// </summary>
        private static void DrawImageProperty(
            SerializedProperty property,
            string label)
        {
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(
                    label));
        }


        /// <summary>
        /// Draws a section header.
        /// </summary>
        private void DrawHeader(
     string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return;
            }

            if (sectionHeaderStyle == null)
            {
                CreateStyles();
            }

            EditorGUILayout.LabelField(
                title,
                sectionHeaderStyle);
        }


        /// <summary>
        /// Creates custom editor styles.
        /// </summary>
        private void CreateStyles()
        {
            sectionHeaderStyle = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(
                    4,
                    4,
                    5,
                    5
                ),
                margin = new RectOffset(
                    0,
                    0,
                    4,
                    4
                )
            };

            toolbarButtonStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(
                    8,
                    8,
                    3,
                    3
                )
            };
        }
        #endregion


        #region Utility

        /// <summary>
        /// Checks whether a name contains any supported identifier.
        /// </summary>
        private static bool ContainsName(
            string value,
            params string[] names)
        {
            if (string.IsNullOrEmpty(value) ||
                names == null)
            {
                return false;
            }

            for (int i = 0;
                 i < names.Length;
                 i++)
            {
                string name =
                    names[i];

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (value == name ||
                    value.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Marks a ButtonDesigner as dirty.
        /// </summary>
        private static void MarkDirty(
            Object targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            EditorUtility.SetDirty(
                targetObject);
        }

        #endregion
    }


}

#endif
