using AAAUI.VFX;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Self-contained uGUI wire browser. Attach to a GameObject next to
    /// SparkWireRegistry. Creates its own compact overlay if no Canvas is set.
    /// No TextMeshPro prefab, editor tooling, or wire-specific canvas required.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkWireRegistry), typeof(SparkWireRouteEditor))]
    public sealed class SparkWireListUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SparkWireRegistry registry;
        [SerializeField] private SparkWireRouteEditor routeEditor;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Font optionalFont;
        [Header("Overlay")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private Vector2 panelSize = new Vector2(410f, 590f);
        [SerializeField] private Vector2 insetFromTopRight = new Vector2(22f, 22f);
        [SerializeField] private int sortingOrder = 150;

        private Font runtimeFont;
        private Canvas generatedCanvas;
        private RectTransform panel;
        private RectTransform listContent;
        private ScrollRect scrollRect;
        private Text titleText;
        private Text statusText;
        private Button undoButton;
        private Button restoreButton;
        private Button removeButton;
        private Button editButton;
        private Text editButtonText;
        private Button undoShapeButton;
        private Button redoShapeButton;
        private Button resetShapeButton;
        private bool initialized;

        private static readonly Color PanelColor = new Color(0.055f, 0.074f, 0.105f, 0.96f);
        private static readonly Color RowColor = new Color(0.115f, 0.145f, 0.19f, 0.97f);
        private static readonly Color SelectedColor = new Color(0.13f, 0.29f, 0.39f, 0.99f);
        private static readonly Color MainText = new Color(0.94f, 0.96f, 0.98f, 1f);
        private static readonly Color MutedText = new Color(0.64f, 0.72f, 0.80f, 1f);

        private void Awake()
        {
            if (registry == null) registry = GetComponent<SparkWireRegistry>();
            if (registry == null) registry = FindFirstObjectByType<SparkWireRegistry>();
            if (routeEditor == null) routeEditor = GetComponent<SparkWireRouteEditor>();
            BuildIfNeeded();
            if (panel != null) panel.gameObject.SetActive(showOnStart);
        }

        private void OnEnable()
        {
            if (registry == null) registry = GetComponent<SparkWireRegistry>();
            if (registry == null) registry = FindFirstObjectByType<SparkWireRegistry>();
            if (routeEditor == null) routeEditor = GetComponent<SparkWireRouteEditor>();
            if (routeEditor != null)
            {
                routeEditor.EditorStateChanged -= OnEditorStateChanged;
                routeEditor.EditorStateChanged += OnEditorStateChanged;
            }
            if (registry != null)
            {
                registry.WiresChanged -= Refresh;
                registry.SelectionChanged -= OnSelectionChanged;
                registry.WiresChanged += Refresh;
                registry.SelectionChanged += OnSelectionChanged;
                registry.RefreshNow();
            }
            BuildIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (routeEditor != null)
                routeEditor.EditorStateChanged -= OnEditorStateChanged;
            if (registry == null) return;
            registry.WiresChanged -= Refresh;
            registry.SelectionChanged -= OnSelectionChanged;
        }

        private void OnDestroy()
        {
            if (generatedCanvas != null) Destroy(generatedCanvas.gameObject);
        }

        private void OnSelectionChanged(string wireId) { Refresh(); }
        private void OnEditorStateChanged() { Refresh(); }
        public void ToggleEditSelected() { if (routeEditor != null) routeEditor.ToggleEditingSelected(); }
        public void UndoShape() { if (routeEditor != null) routeEditor.UndoRoute(); }
        public void RedoShape() { if (routeEditor != null) routeEditor.RedoRoute(); }
        public void ResetShape() { if (routeEditor != null) routeEditor.ResetSelectedRoute(); }

        public void Show() { BuildIfNeeded(); if (panel != null) panel.gameObject.SetActive(true); }
        public void Hide() { if (panel != null) panel.gameObject.SetActive(false); }
        public void Toggle() { BuildIfNeeded(); if (panel != null) panel.gameObject.SetActive(!panel.gameObject.activeSelf); }
        public void RemoveSelected() { if (registry != null) registry.RemoveSelectedWire(); }
        public void UndoLast() { if (registry != null) registry.UndoLastWire(); }
        public void RestoreLast() { if (registry != null) registry.RestoreLastRemovedWire(); }

        private void BuildIfNeeded()
        {
            if (initialized || !Application.isPlaying) return;
            initialized = true;
            runtimeFont = optionalFont;
            if (runtimeFont == null)
            {
                // Unity 6 ships this legacy uGUI runtime font. If a project
                // strips it, the optionalFont Inspector slot can override it.
                runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (runtimeFont == null)
                runtimeFont = Font.CreateDynamicFontFromOSFont("Arial", 14);

            Canvas canvas = targetCanvas;
            if (canvas == null)
            {
                GameObject go = new GameObject("ProjectSpark Wire Manager UI",
                    typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                generatedCanvas = go.GetComponent<Canvas>();
                generatedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                generatedCanvas.overrideSorting = true;
                generatedCanvas.sortingOrder = sortingOrder;
                CanvasScaler scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvas = generatedCanvas;
            }

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject events = new GameObject("ProjectSpark Wire Manager EventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule));
                // If using New Input System exclusively, replace the input
                // module with InputSystemUIInputModule in the Unity Inspector.
            }

            panel = CreateRect("Wire Registry Panel", canvas.transform);
            panel.anchorMin = panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-insetFromTopRight.x, -insetFromTopRight.y);
            panel.sizeDelta = panelSize;
            AddImage(panel.gameObject, PanelColor);
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.20f, 0.38f, 0.48f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);

            titleText = CreateText("Title", panel, 20, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            Place(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -12f), new Vector2(panelSize.x - 32f, 34f), new Vector2(0f, 1f));

            float buttonWidth = (panelSize.x - 44f) / 3f;
            float top = -58f;
            undoButton = CreateButton("Undo Last", panel, new Color(0.16f, 0.30f, 0.41f, 1f), UndoLast);
            Place(undoButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, top), new Vector2(buttonWidth, 34f), new Vector2(0f, 1f));

            restoreButton = CreateButton("Restore", panel, new Color(0.17f, 0.37f, 0.31f, 1f), RestoreLast);
            Place(restoreButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f + buttonWidth, top), new Vector2(buttonWidth, 34f), new Vector2(0f, 1f));

            removeButton = CreateButton("Remove Selected", panel,
                new Color(0.48f, 0.22f, 0.23f, 1f), RemoveSelected);
            Place(removeButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f + buttonWidth * 2f, top),
                new Vector2(buttonWidth, 34f), new Vector2(0f, 1f));

            // Post-draw route editing: editing changes cable geometry only,
            // never the SparkCircuitSystem electrical connection.
            editButton = CreateButton("Edit Selected", panel,
                new Color(0.18f, 0.42f, 0.56f, 1f), ToggleEditSelected);
            editButtonText = editButton.GetComponentInChildren<Text>();
            Place(editButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, -102f), new Vector2(buttonWidth, 34f),
                new Vector2(0f, 1f));
            undoShapeButton = CreateButton("Undo Bend", panel,
                new Color(0.17f, 0.31f, 0.42f, 1f), UndoShape);
            Place(undoShapeButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f + buttonWidth, -102f), new Vector2(buttonWidth, 34f),
                new Vector2(0f, 1f));
            redoShapeButton = CreateButton("Redo Bend", panel,
                new Color(0.17f, 0.31f, 0.42f, 1f), RedoShape);
            Place(redoShapeButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f + buttonWidth * 2f, -102f),
                new Vector2(buttonWidth, 34f), new Vector2(0f, 1f));
            resetShapeButton = CreateButton("Reset Selected Route", panel,
                new Color(0.25f, 0.30f, 0.40f, 1f), ResetShape);
            Place(resetShapeButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, -144f), new Vector2(panelSize.x - 24f, 32f),
                new Vector2(0f, 1f));

            RectTransform viewport = CreateRect("List Viewport", panel);
            Place(viewport, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(12f, 52f), new Vector2(-24f, -205f), new Vector2(0f, 0f), true);
            AddImage(viewport.gameObject, new Color(0.035f, 0.055f, 0.084f, 0.93f));
            viewport.gameObject.AddComponent<RectMask2D>();

            listContent = CreateRect("Wire Rows", viewport);
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.anchoredPosition = Vector2.zero;
            listContent.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 5, 5);
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = listContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = listContent;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            statusText = CreateText("Status", panel, 13, FontStyle.Normal, MutedText, TextAnchor.MiddleLeft);
            Place(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(15f, 6f), new Vector2(-30f, 40f), new Vector2(0f, 0f), true);
        }

        public void Refresh()
        {
            if (!initialized || listContent == null || registry == null) return;
            titleText.text = "WIRE MANAGER    " + registry.Count + " ACTIVE";
            bool isEditing = routeEditor != null && routeEditor.IsEditing;
            statusText.fontSize = 11;
            statusText.text = isEditing
                ? "EDIT ON: drag cyan bends | Shift+click wire: add | Del: remove\n" +
                  "Ctrl+Z/Y undo/redo | V: vertical | Alt+wheel: depth | G: snap | Esc: exit"
                : (registry.SelectedWireId != null
                    ? "Selected " + registry.SelectedWireId + "   |   Edit Selected to reshape it"
                    : "Choose a wire or click Edit Selected to pick cables in-scene.");
            if (editButton != null)
            {
                editButton.interactable = isEditing || registry.Count > 0;
                if (editButtonText != null)
                    editButtonText.text = isEditing ? "Finish Edit" : "Edit Selected";
            }
            if (undoShapeButton != null)
                undoShapeButton.interactable = routeEditor != null && routeEditor.UndoCount > 0;
            if (redoShapeButton != null)
                redoShapeButton.interactable = routeEditor != null && routeEditor.RedoCount > 0;
            if (resetShapeButton != null)
                resetShapeButton.interactable = registry.SelectedWireId != null;
            undoButton.interactable = registry.Count > 0;
            restoreButton.interactable = registry.UndoHistoryCount > 0;
            removeButton.interactable = !string.IsNullOrEmpty(registry.SelectedWireId);

            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                Transform row = listContent.GetChild(i);
                row.gameObject.SetActive(false); // immediately exclude from layout/render
                row.SetParent(null, false);
                Destroy(row.gameObject);
            }

            if (registry.Count == 0)
            {
                Text empty = CreateText("Empty", listContent, 14, FontStyle.Italic,
                    MutedText, TextAnchor.MiddleCenter);
                empty.text = "No completed wires yet.\nDrag Terminal 1 to Terminal 2.";
                LayoutElement emptyLayout = empty.gameObject.AddComponent<LayoutElement>();
                emptyLayout.preferredHeight = 72f;
            }

            for (int i = 0; i < registry.Wires.Count; i++)
                CreateWireRow(registry.Wires[i]);

            // Removed wires retain their readable ID in a separate archive.
            // Each can be restored independently, not only in LIFO order.
            if (registry.UndoHistoryCount > 0)
            {
                Text heading = CreateText("Undo History", listContent, 13,
                    FontStyle.Bold, MutedText, TextAnchor.MiddleLeft);
                heading.text = "REMOVED / CAN RESTORE (" + registry.UndoHistoryCount + ")";
                LayoutElement headingLayout = heading.gameObject.AddComponent<LayoutElement>();
                headingLayout.preferredHeight = 28f;
                for (int i = registry.UndoHistoryCount - 1; i >= 0; i--)
                    CreateRemovedRow(registry.GetRemovedWireIdAt(i));
            }
        }

        private void CreateWireRow(SparkWireRecord wire)
        {
            string id = wire.WireId;
            bool selected = registry.SelectedWireId == id;
            RectTransform row = CreateRect(id, listContent);
            Image background = AddImage(row.gameObject, selected ? SelectedColor : RowColor);
            Button rowButton = row.gameObject.AddComponent<Button>();
            rowButton.targetGraphic = background;
            rowButton.onClick.AddListener(() => registry.SelectWire(id));
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 68f;

            Text primary = CreateText("Route", row, 14, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            primary.text = id + "    " + Limit(wire.FromLabel, 13) + "  ->  " + Limit(wire.ToLabel, 13);
            Place(primary.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -32f), new Vector2(-194f, -7f), new Vector2(0f, 1f), true);

            Text secondary = CreateText("Details", row, 12, FontStyle.Normal, MutedText, TextAnchor.MiddleLeft);
            secondary.text = wire.IsElectricallyConnected
                ? "Circuit #" + wire.ConnectionId + "     " + wire.WaypointCount + " route points"
                : "Visual-only wire     " + wire.WaypointCount + " route points";
            Place(secondary.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(12f, 7f), new Vector2(-194f, 22f), new Vector2(0f, 0f), true);

            Button editRow = CreateButton("Edit", row,
                new Color(0.18f, 0.39f, 0.52f, 1f), () =>
                {
                    registry.SelectWire(id);
                    if (routeEditor != null && !routeEditor.IsEditing)
                        routeEditor.EnableEditing();
                });
            Place(editRow.GetComponent<RectTransform>(),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-92f, 0f), new Vector2(76f, 32f),
                new Vector2(1f, 0.5f));

            Button remove = CreateButton("Remove", row, new Color(0.52f, 0.22f, 0.23f, 1f),
                () => registry.RemoveWire(id));
            Place(remove.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-9f, 0f), new Vector2(78f, 32f), new Vector2(1f, 0.5f));
        }

        private void CreateRemovedRow(string id)
        {
            RectTransform row = CreateRect("Removed " + id, listContent);
            AddImage(row.gameObject, new Color(0.13f, 0.12f, 0.15f, 0.95f));
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 44f;

            Text text = CreateText("Removed ID", row, 13, FontStyle.Normal,
                MutedText, TextAnchor.MiddleLeft);
            text.text = id + "    Removed";
            Place(text.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(12f, 0f), new Vector2(-96f, 0f), new Vector2(0f, 0f), true);
            Button restore = CreateButton("Restore", row,
                new Color(0.17f, 0.37f, 0.31f, 1f),
                () => registry.RestoreRemovedWire(id));
            Place(restore.GetComponent<RectTransform>(),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-9f, 0f), new Vector2(78f, 30f), new Vector2(1f, 0.5f));
        }

        private static string Limit(string name, int maxLength)
        {
            return string.IsNullOrEmpty(name) || name.Length <= maxLength
                ? name : name.Substring(0, maxLength - 1) + "…";
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image AddImage(GameObject gameObject, Color color)
        {
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Transform parent, int size,
            FontStyle fontStyle, Color color, TextAnchor align)
        {
            RectTransform rect = CreateRect(name, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = runtimeFont;
            text.fontSize = size;
            text.fontStyle = fontStyle;
            text.color = color;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(string caption, Transform parent, Color color,
            UnityEngine.Events.UnityAction click)
        {
            RectTransform rect = CreateRect(caption + " Button", parent);
            Image image = AddImage(rect.gameObject, color);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            Text label = CreateText("Label", rect, 12, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            label.text = caption;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(3f, 0f);
            label.rectTransform.offsetMax = new Vector2(-3f, 0f);
            return button;
        }

        private static void Place(RectTransform rect, Vector2 min, Vector2 max,
            Vector2 posOrOffsetMin, Vector2 sizeOrOffsetMax, Vector2 pivot,
            bool stretch = false)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            if (stretch)
            {
                rect.offsetMin = posOrOffsetMin;
                rect.offsetMax = sizeOrOffsetMax;
            }
            else
            {
                rect.anchoredPosition = posOrOffsetMin;
                rect.sizeDelta = sizeOrOffsetMax;
            }
        }
    }
}
