#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// Additive, repeat-safe creator. Reuses the user's existing course and exam assets.
    /// Creates CanvasRenderer explicitly on every UGUI element. Never edits assets.
    /// </summary>
    public static class SparkAICourseCertificationBuilder
    {
        private static readonly Color Back = new Color(.025f, .071f, .118f, .99f);
        private static readonly Color Tile = new Color(.066f, .14f, .21f, 1f);
        private static readonly Color Accent = new Color(.26f, .88f, 1f, 1f);
        private static readonly Color Ink = new Color(.95f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.70f, .83f, .90f, 1f);
        private static readonly Color ButtonColor = new Color(.09f, .31f, .46f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Course Certification")]
        public static void Create()
        {
            SparkAICourseCertification existing =
                Object.FindFirstObjectByType<SparkAICourseCertification>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[Spark Certification] Existing panel selected; no duplicate created.", existing);
                return;
            }

            SparkGuidedLearningPathAsset[] courses = FindAssets<SparkGuidedLearningPathAsset>();
            SparkAIAssessmentBank[] banks = FindAssets<SparkAIAssessmentBank>();
            SparkAIGuidedLearningPaths paths =
                Object.FindFirstObjectByType<SparkAIGuidedLearningPaths>(FindObjectsInactive.Include);
            SparkAILearningPathAssessment assessment =
                Object.FindFirstObjectByType<SparkAILearningPathAssessment>(FindObjectsInactive.Include);

            if (courses.Length == 0 || banks.Length == 0)
            {
                Debug.LogError("[Spark Certification] Create Guided Learning Paths and Learning Path Assessment assets first.");
                return;
            }

            GameObject canvasObject = new GameObject("SparkAICertificationCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Spark AI Course Certification");
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 905;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            GameObject root = UI("SparkAICourseCertificationRoot", canvasObject.transform);
            Stretch(root.GetComponent<RectTransform>());
            Button opener = MakeButton(root.transform, "OpenCertification", "CERTIFICATES", -205, -515, 188, 42, true);

            GameObject panel = UI("CertificationPanel", root.transform);
            Position(panel.GetComponent<RectTransform>(), -748, -75, 724, 690, true);
            Image background = panel.AddComponent<Image>();
            background.color = Back;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(panel.transform, "Header", "SPARK AI / COURSE CERTIFICATION", 20, Accent, 20, -14, 615, 30);
            Button close = MakeButton(panel.transform, "Close", "X", 667, -14, 36, 29);
            MakeText(panel.transform, "Caption", "LOCAL CERTIFICATES / READ-ONLY LEARNING EVIDENCE", 12, Muted, 21, -46, 665, 22);

            Button previous = MakeButton(panel.transform, "PreviousCourse", "< COURSE", 20, -78, 112, 36);
            Button next = MakeButton(panel.transform, "NextCourse", "COURSE >", 591, -78, 112, 36);
            TMP_Text title = MakeText(panel.transform, "CourseTitle", "COURSE", 19, Ink, 143, -79, 435, 33);
            TMP_Text desc = MakeText(panel.transform, "CourseDescription", "Course summary", 14, Muted, 22, -124, 673, 51);
            TMP_Text progress = MakeText(panel.transform, "ProgressText", "PATH STAGES 0/0", 13, Accent, 22, -181, 680, 25);
            MakeBlock(panel.transform, "ProgressTrack", 22, -211, 678, 9, new Color(.14f, .23f, .31f, 1f));
            GameObject fillObject = UI("ProgressFill", panel.transform);
            Position(fillObject.GetComponent<RectTransform>(), 22, -211, 678, 9);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            MakeBlock(panel.transform, "RequirementsBackground", 19, -240, 685, 123);
            TMP_Text requirements = MakeText(panel.transform, "Requirements", "Requirements", 14, Ink,
                32, -248, 661, 108);
            MakeBlock(panel.transform, "EvidenceBackground", 19, -372, 685, 54);
            TMP_Text evidence = MakeText(panel.transform, "Evidence", "Complete stages and assessment", 13, Muted,
                32, -380, 661, 42);

            MakeText(panel.transform, "LearnerNameLabel", "CERTIFICATE DISPLAY NAME (LOCAL ONLY)",
                12, Accent, 23, -438, 590, 20);
            TMP_InputField nameInput = MakeInput(panel.transform, 21, -461, 681, 43);
            MakeBlock(panel.transform, "CertificateBackground", 20, -518, 683, 66);
            TMP_Text certificate = MakeText(panel.transform, "CertificateInfo", "NO CERTIFICATE YET", 12, Ink,
                30, -526, 659, 53);
            Button issue = MakeButton(panel.transform, "Issue", "ISSUE CERTIFICATE", 22, -595, 219, 43);
            Button export = MakeButton(panel.transform, "Export", "EXPORT TXT + JSON", 249, -595, 220, 43);
            Button refresh = MakeButton(panel.transform, "Refresh", "REFRESH", 477, -595, 224, 43);
            Button pathsButton = MakeButton(panel.transform, "OpenPaths", "LEARNING PATHS", 22, -644, 215, 34);
            Button examButton = MakeButton(panel.transform, "OpenAssessment", "ASSESSMENTS", 245, -644, 221, 34);
            TMP_Text status = MakeText(panel.transform, "Status", "Check requirements and issue a local certificate.",
                11, Muted, 475, -644, 219, 34);

            SparkAICourseCertification controller = Undo.AddComponent<SparkAICourseCertification>(root);
            SerializedObject serialized = new SerializedObject(controller);
            Set(serialized, "pathsPanel", paths);
            Set(serialized, "assessmentPanel", assessment);
            Set(serialized, "panel", panel);
            Set(serialized, "courseTitle", title);
            Set(serialized, "courseDescription", desc);
            Set(serialized, "progressText", progress);
            Set(serialized, "requirementText", requirements);
            Set(serialized, "resultText", evidence);
            Set(serialized, "certificateText", certificate);
            Set(serialized, "statusText", status);
            Set(serialized, "learnerNameInput", nameInput);
            Set(serialized, "stageFill", fill);
            Set(serialized, "issueButton", issue);
            Set(serialized, "exportButton", export);
            SetArray(serialized, "courses", courses);
            SetArray(serialized, "assessments", banks);
            serialized.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(previous.onClick, controller.PreviousCourse);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextCourse);
            UnityEventTools.AddPersistentListener(issue.onClick, controller.IssueCertificate);
            UnityEventTools.AddPersistentListener(export.onClick, controller.ExportCertificate);
            UnityEventTools.AddPersistentListener(refresh.onClick, controller.Refresh);
            UnityEventTools.AddPersistentListener(pathsButton.onClick, controller.OpenLearningPaths);
            UnityEventTools.AddPersistentListener(examButton.onClick, controller.OpenAssessment);

            panel.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Certification] Panel created using " + courses.Length + " course and " +
                      banks.Length + " assessment assets. No existing assets modified.", root);
        }

        private static T[] FindAssets<T>() where T : ScriptableObject
        {
            string[] ids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            var list = new List<T>();
            var seen = new HashSet<T>();
            foreach (string id in ids)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(id));
                if (asset != null && seen.Add(asset)) list.Add(asset);
            }
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Position(RectTransform rect, float x, float y, float width, float height, bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static GameObject MakeBlock(Transform parent, string name, float x, float y, float width,
            float height, Color? tint = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, width, height);
            Image image = go.AddComponent<Image>();
            image.color = tint ?? Tile;
            image.raycastTarget = false;
            return go;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, string value,
            int size, Color color, float x, float y, float width, float height)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, width, height);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableAutoSizing = true;
            label.fontSizeMin = 10;
            label.fontSizeMax = size;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            float x, float y, float width, float height, bool topRight = false)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, width, height, topRight);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = MakeText(go.transform, "Label", label, 13, Ink, 0, 0, width, height);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static TMP_InputField MakeInput(Transform parent, float x, float y, float width, float height)
        {
            GameObject go = UI("LearnerNameInput", parent);
            Position(go.GetComponent<RectTransform>(), x, y, width, height);
            Image background = go.AddComponent<Image>();
            background.color = Tile;
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            TextMeshProUGUI text = MakeText(go.transform, "Text", "", 17, Ink, 11, -5, width - 20, height - 10);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            TextMeshProUGUI placeholder = MakeText(go.transform, "Placeholder", "Learner", 14,
                Muted, 11, -5, width - 20, height - 10);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            input.textViewport = go.GetComponent<RectTransform>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.characterLimit = 64;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            return input;
        }

        private static void Set(SerializedObject serialized, string field, Object value)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property != null) property.objectReferenceValue = value;
            else Debug.LogError("[Spark Certification] Missing serialized field: " + field);
        }

        private static void SetArray<T>(SerializedObject serialized, string field, T[] assets) where T : Object
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null) { Debug.LogError("[Spark Certification] Missing asset array: " + field); return; }
            property.arraySize = assets.Length;
            for (int i = 0; i < assets.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
        }
    }
}
#endif
