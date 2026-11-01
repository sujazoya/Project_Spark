#if UNITY_EDITOR
using ProjectSpark.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>Creates a standalone circuit experiment workbench, NOT an AI Learning Panel.</summary>
    public static class SparkCircuitExperimentLabBuilder
    {
        private static readonly Color Navy = new Color(.027f, .075f, .125f, .985f);
        private static readonly Color Dark = new Color(.055f, .135f, .205f, 1f);
        private static readonly Color Cyan = new Color(.27f, .88f, .98f, 1f);
        private static readonly Color White = new Color(.95f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.67f, .80f, .87f, 1f);
        private static readonly Color ButtonBlue = new Color(.08f, .31f, .45f, 1f);

        [MenuItem("Tools/Project Spark/Circuit/Create Experiment Lab")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Circuit Lab] Build in Edit Mode, not Play Mode.");
                return;
            }

            SparkCircuitExperimentLab existing = Object.FindFirstObjectByType<SparkCircuitExperimentLab>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[Circuit Lab] Existing experiment lab selected. No duplicate created.", existing);
                return;
            }

            var publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);

            GameObject root = new GameObject("SparkCircuitExperimentLabCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "Create Circuit Experiment Lab");

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 760;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            GameObject openerGo = UI("OpenExperimentLab", root.transform);
            SetTopRight(openerGo.GetComponent<RectTransform>(), -28, -27, 210, 43);
            Button opener = ButtonOn(openerGo, "EXPERIMENT LAB", 14);

            GameObject panel = UI("ExperimentWindow", root.transform);
            SetTopRight(panel.GetComponent<RectTransform>(), -28, -82, 618, 706);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = Navy;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(.24f, .75f, .92f, .45f);
            outline.effectDistance = new Vector2(1f, -1f);

            Text(panel.transform, "Title", "PROJECT SPARK  /  CIRCUIT EXPERIMENT LAB", 16, Cyan, 23, -17, 530, 28);
            Button close = Button(panel.transform, "Close", "×", 560, -13, 34, 32);
            TMP_Text sourceStatus = Text(panel.transform, "SourceStatus", "VERIFIED SOURCE  /  WAITING", 12,
                Muted, 23, -54, 567, 21);
            TMP_Text experimentStatus = Text(panel.transform, "ExperimentStatus",
                "Capture a verified baseline, change the circuit, then compare.", 13,
                White, 23, -89, 570, 60);
            Block(panel.transform, "SummaryBackground", 20, -158, 579, 42, Dark);
            TMP_Text summary = Text(panel.transform, "Summary", "BEFORE —  AFTER —  HISTORY 0", 13,
                Cyan, 31, -168, 560, 23);

            Button before = Button(panel.transform, "CaptureBefore", "1  CAPTURE BEFORE", 20, -214, 180, 42);
            Button after = Button(panel.transform, "CaptureAfter", "2  CAPTURE AFTER", 206, -214, 180, 42);
            Button reset = Button(panel.transform, "ResetExperiment", "RESET", 392, -214, 99, 42);
            Button export = Button(panel.transform, "ExportResults", "EXPORT", 499, -214, 100, 42);

            Text(panel.transform, "ComparisonHeading", "MEASURED CHANGES", 13, Cyan, 23, -275, 570, 22);

            GameObject scrolling = UI("ComparisonScroll", panel.transform);
            At(scrolling.GetComponent<RectTransform>(), 20, -304, 579, 325);
            BlockOn(scrolling, Dark);
            var scrollingRect = scrolling.GetComponent<RectTransform>();
            var scroll = scrolling.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;

            GameObject viewport = UI("Viewport", scrolling.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            GameObject contentGo = UI("ComparisonContent", viewport.transform);
            RectTransform contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 310f);
            scroll.content = contentRect;

            TMP_Text compare = Text(contentGo.transform, "ComparisonText",
                "1. Capture before\n2. Edit the circuit\n3. Capture after", 14,
                White, 12, -12, 548, 290);
            compare.richText = true;
            compare.enableAutoSizing = false;
            compare.overflowMode = TextOverflowModes.Overflow;
            compare.raycastTarget = false;

            TMP_Text footer = Text(panel.transform, "Footer",
                "Read-only observations • Solver values only • No gameplay changes", 12,
                Muted, 22, -648, 572, 42);
            footer.alignment = TextAlignmentOptions.MidlineLeft;

            var controller = Undo.AddComponent<SparkCircuitExperimentLab>(root);
            SerializedObject so = new SerializedObject(controller);
            Set(so, "publisher", publisher);
            Set(so, "levelManager", level);
            Set(so, "window", panel);
            Set(so, "sourceStatusText", sourceStatus);
            Set(so, "experimentStatusText", experimentStatus);
            Set(so, "summaryText", summary);
            Set(so, "comparisonText", compare);
            Set(so, "comparisonContent", contentRect);
            Set(so, "comparisonScroll", scroll);
            Set(so, "captureBeforeButton", before);
            Set(so, "captureAfterButton", after);
            Set(so, "exportButton", export);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(before.onClick, controller.CaptureBefore);
            UnityEventTools.AddPersistentListener(after.onClick, controller.CaptureAfter);
            UnityEventTools.AddPersistentListener(reset.onClick, controller.ResetExperiment);
            UnityEventTools.AddPersistentListener(export.onClick, controller.ExportExperiments);

            panel.SetActive(false);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            Debug.Log("[Circuit Lab] Created independent read-only Experiment Lab. " +
                (publisher != null ? "Verified publisher connected." : "WARNING: Verified publisher missing; add it and assign in Inspector.") +
                (level == null ? " Evaluator missing: official progress comparison unavailable." : " Evaluator connected."), root);
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

        private static void SetTopRight(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void At(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Block(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            GameObject go = UI(name, parent);
            At(go.GetComponent<RectTransform>(), x, y, w, h);
            BlockOn(go, color);
        }

        private static void BlockOn(GameObject go, Color color)
        {
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static TMP_Text Text(Transform parent, string name, string message, int size, Color color,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            At(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(Transform parent, string name, string label, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            At(go.GetComponent<RectTransform>(), x, y, w, h);
            return ButtonOn(go, label, 13);
        }

        private static Button ButtonOn(GameObject go, string label, int size)
        {
            Image bg = go.AddComponent<Image>();
            bg.color = ButtonBlue;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(.7f, .86f, .93f, 1f);
            button.colors = colors;
            GameObject labelGo = UI("Label", go.transform);
            Stretch(labelGo.GetComponent<RectTransform>());
            TMP_Text labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = size;
            labelText.color = White;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.raycastTarget = false;
            return button;
        }

        private static void Set(SerializedObject obj, string name, Object value)
        {
            SerializedProperty p = obj.FindProperty(name);
            if (p == null) Debug.LogError("[Circuit Lab] Missing serialized field " + name);
            else p.objectReferenceValue = value;
        }
    }
}
#endif
