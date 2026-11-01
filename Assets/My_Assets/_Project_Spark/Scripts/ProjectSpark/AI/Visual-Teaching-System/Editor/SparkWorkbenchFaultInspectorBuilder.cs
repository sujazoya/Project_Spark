#if UNITY_EDITOR
using System;
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Non-destructive one-click UI builder. Never touches electrical solver,
    /// existing HUDs, layout assets, levels, or probe components.
    /// </summary>
    public static class SparkWorkbenchFaultInspectorBuilder
    {
        private const string RootName = "SparkWorkbenchFaultInspectorCanvas";
        private static readonly Color Background = C("#071A2B");
        private static readonly Color Header = C("#0D344D");
        private static readonly Color Surface = C("#0E293C");
        private static readonly Color Teal = C("#59DFFF");
        private static readonly Color Mint = C("#B0FFD5");
        private static readonly Color Dim = C("#A6C2D1");

        [MenuItem("Tools/Project Spark/Electrical/Create Workbench Fault Inspector")]
        public static void Create()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                EditorGUIUtility.PingObject(existing);
                EditorUtility.DisplayDialog("Project Spark", "The Fault Inspector already exists in this scene. Selected it instead of duplicating it.", "OK");
                return;
            }

            GameObject root = new GameObject(RootName,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(SparkWorkbenchFaultInspector));
            Undo.RegisterCreatedObjectUndo(root, "Create Spark Workbench Fault Inspector");
            root.transform.SetAsLastSibling();
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1700;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            RectTransform markerLayer = Rect("WorldMarkerLayer", root.transform);
            Stretch(markerLayer, 0f, 0f, 0f, 0f);
            RectTransform markerTemplate = Image("MarkerTemplate", markerLayer, Header);
            Center(markerTemplate, new Vector2(215f, 34f), Vector2.zero);
            markerTemplate.GetComponent<Image>().raycastTarget = false;
            TMP_Text markerText = Text("MarkerLabel", markerTemplate,
                "VERIFIED FAULT", 15, Teal, TextAlignmentOptions.Center);
            Stretch((RectTransform)markerText.transform, 7f, 4f, 7f, 4f);
            markerTemplate.gameObject.SetActive(false);

            RectTransform tab = Image("CollapsedTab", root.transform, Header);
            BottomLeft(tab, new Vector2(177f, 42f), new Vector2(145f, 24f));
            Button tabButton = tab.gameObject.AddComponent<Button>();
            TintButton(tabButton);
            TMP_Text tabText = Text("TabLabel", tab, "⚡  FAULT INSPECTOR", 17, Teal,
                TextAlignmentOptions.Center);
            Stretch((RectTransform)tabText.transform, 7f, 4f, 7f, 4f);

            RectTransform panel = Image("ExpandedPanel", root.transform, Background);
            BottomLeft(panel, new Vector2(392f, 452f), new Vector2(145f, 24f));

            RectTransform header = Image("Header", panel, Header);
            TopStretch(header, 53f, 0f);
            TMP_Text title = Text("Title", header, "SPARK / WORKBENCH FAULT INSPECTOR",
                17, Teal, TextAlignmentOptions.MidlineLeft);
            Stretch((RectTransform)title.transform, 15f, 4f, 47f, 4f);
            Button closeButton = MakeButton("Close", header, "X", 16, new Color(.28f,.17f,.21f,1f));
            TopRight((RectTransform)closeButton.transform, new Vector2(32, 29), new Vector2(-9, -11));

            TMP_Text state = Text("State", panel, "WAITING FOR ACTIVE LEVEL",
                17, Mint, TextAlignmentOptions.MidlineLeft);
            TopStretch((RectTransform)state.transform, 27f, 60f, 15f, 15f);
            TMP_Text level = Text("Level", panel, "LEVEL  (none)",
                13, Dim, TextAlignmentOptions.MidlineLeft);
            TopStretch((RectTransform)level.transform, 22f, 92f, 15f, 15f);
            TMP_Text count = Text("FindingCount", panel, "0 findings",
                13, Teal, TextAlignmentOptions.MidlineRight);
            TopStretch((RectTransform)count.transform, 22f, 92f, 230f, 15f);

            RectTransform viewport = Image("FindingsViewport", panel, Surface);
            Stretch(viewport, 13f, 127f, 13f, 140f);
            viewport.gameObject.AddComponent<RectMask2D>();
            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 22f;
            scroll.viewport = viewport;
            RectTransform content = Rect("FindingsContent", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(.5f, 1f);
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 7, 7);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            ContentSizeFitter fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            Button rowTemplate = MakeButton("FindingRowTemplate", content, "VERIFIED FINDING", 14, Header);
            LayoutElement rowHeight = rowTemplate.gameObject.AddComponent<LayoutElement>();
            rowHeight.preferredHeight = 46f;
            rowHeight.minHeight = 46f;
            rowTemplate.gameObject.SetActive(false);

            RectTransform detailBack = Image("ExplanationBackground", panel, Surface);
            detailBack.anchorMin = new Vector2(0f, 0f);
            detailBack.anchorMax = new Vector2(1f, 0f);
            detailBack.pivot = new Vector2(.5f, 0f);
            detailBack.sizeDelta = new Vector2(-26f, 75f);
            detailBack.anchoredPosition = new Vector2(0f, 56f);
            TMP_Text explanation = Text("Explanation", detailBack,
                "Select a verified finding for details. Global warnings have no guessed 3D location.",
                13, Dim, TextAlignmentOptions.MidlineLeft);
            Stretch((RectTransform)explanation.transform, 9f, 6f, 9f, 6f);

            Button refresh = MakeButton("Refresh", panel, "REFRESH", 13, Header);
            BottomLeft((RectTransform)refresh.transform, new Vector2(108f, 37f), new Vector2(13f, 10f));
            Button pinToggle = MakeButton("TogglePins", panel, "PINS ON", 13, Header);
            BottomLeft((RectTransform)pinToggle.transform, new Vector2(111f, 37f), new Vector2(130f, 10f));
            TMP_Text pinText = pinToggle.GetComponentInChildren<TMP_Text>(true);
            TMP_Text hint = Text("Hint", panel, "READ ONLY / VERIFIED EVIDENCE",
                12, Dim, TextAlignmentOptions.MidlineRight);
            BottomLeft((RectTransform)hint.transform, new Vector2(140f, 34f), new Vector2(242f, 11f));

            SerializedObject serialized = new SerializedObject(root.GetComponent<SparkWorkbenchFaultInspector>());
            Assign(serialized, "canvas", canvas);
            Assign(serialized, "expandedPanel", panel.gameObject);
            Assign(serialized, "collapsedTab", tab.gameObject);
            Assign(serialized, "stateText", state);
            Assign(serialized, "levelText", level);
            Assign(serialized, "countText", count);
            Assign(serialized, "explanationText", explanation);
            Assign(serialized, "findingsContainer", content);
            Assign(serialized, "findingRowTemplate", rowTemplate);
            Assign(serialized, "markerLayer", markerLayer);
            Assign(serialized, "markerTemplate", markerTemplate);
            Assign(serialized, "tabButton", tabButton);
            Assign(serialized, "closeButton", closeButton);
            Assign(serialized, "refreshButton", refresh);
            Assign(serialized, "markersButton", pinToggle);
            Assign(serialized, "markersButtonText", pinText);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);
            tab.gameObject.SetActive(true);
            EnsureEventSystem();
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Project Spark] Fault Inspector created. Save scene, enter Play Mode, then click FAULT INSPECTOR.", root);
        }

        private static void Assign(SerializedObject serialized, string field, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                Debug.LogError("[Project Spark] Missing serialized field: " + field);
            else
                property.objectReferenceValue = value;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static RectTransform Image(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return (RectTransform)go.transform;
        }

        private static TMP_Text Text(string name, Transform parent, string value, float size,
            Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.color = color;
            text.fontSize = size;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.enableWordWrapping = true;
            return text;
        }

        private static Button MakeButton(string name, Transform parent, string caption,
            float size, Color background)
        {
            RectTransform rect = Image(name, parent, background);
            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            TintButton(button);
            TMP_Text label = Text("Label", rect, caption, size, Color.white, TextAlignmentOptions.Center);
            Stretch((RectTransform)label.transform, 7f, 4f, 7f, 4f);
            return button;
        }

        private static void TintButton(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = C("#B0F2FF");
            colors.pressedColor = C("#60BFD8");
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.gray;
            button.colors = colors;
        }

        private static void BottomLeft(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void TopRight(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Center(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void TopStretch(RectTransform rect, float height, float offsetTop,
            float marginLeft = 0f, float marginRight = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.sizeDelta = new Vector2(-(marginLeft + marginRight), height);
            rect.anchoredPosition = new Vector2((marginLeft - marginRight) * .5f, -offsetTop);
        }

        private static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Color C(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.white;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
            Type inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null)
                go.AddComponent(inputSystemModule);
            else
                go.AddComponent<StandaloneInputModule>();
        }
    }
}
#endif
