using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Self-contained responsive TextMeshPro/uGUI engineering-mode console.
    /// Uses the scene EventSystem when available; it never duplicates an existing one.
    /// No Update polling: the panel refreshes only when the controller raises StateChanged.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringControlPanel : MonoBehaviour
    {
        [SerializeField] private bool visibleAtStart = true;
        private SparkEngineeringControlHub hub;
        private GameObject canvasObject;
        private GameObject panelObject;
        private TMP_Text stateText;
        private TMP_Text logText;
        private Button manualButton;
        private Button assistedButton;
        private Button autoButton;
        private Button hideButton;
        private RectTransform screenArea;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private readonly Color textColor = new Color(.85f, .95f, 1f, 1f);
        private readonly Color mutedColor = new Color(.53f, .70f, .80f, 1f);
        private readonly Color accentColor = new Color(.02f, .74f, .94f, 1f);
        private readonly Color panelColor = new Color(.025f, .065f, .10f, .96f);
        private bool expanded;

        private void Awake()
        {
            hub = GetComponent<SparkEngineeringControlHub>();
            expanded = visibleAtStart;
        }

        private void OnEnable()
        {
            if (!hub) hub = GetComponent<SparkEngineeringControlHub>();
            if (hub) hub.StateChanged += Refresh;
            Canvas.preWillRenderCanvases += ApplySafeArea;
        }

        private void Start()
        {
            Build();
            Refresh();
        }

        private void OnDisable()
        {
            if (hub) hub.StateChanged -= Refresh;
            Canvas.preWillRenderCanvases -= ApplySafeArea;
        }

        private void OnDestroy()
        {
            if (canvasObject) Destroy(canvasObject);
        }

        private static RectTransform Rect(GameObject owner, Transform parent,
            Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            owner.transform.SetParent(parent, false);
            RectTransform r = owner.GetComponent<RectTransform>();
            if (!r) r = owner.AddComponent<RectTransform>();
            r.anchorMin = min;
            r.anchorMax = max;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            return r;
        }

        private static GameObject UI(string name) => new GameObject(name, typeof(RectTransform));

        private TMP_Text Label(Transform parent, string name, string value, float size,
            Vector2 pos, Vector2 dimension, Color color, FontStyles style = FontStyles.Normal)
        {
            GameObject go = UI(name);
            Rect(go, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), pos, dimension);
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = TextAlignmentOptions.TopLeft;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            return t;
        }

        private Button MakeButton(Transform parent, string title, float x, float y, float width,
            Action command, bool secondary = false)
        {
            GameObject go = UI(title.Replace(" ", "") + "Button");
            Rect(go, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(x, y), new Vector2(width, 33f));
            Image background = go.AddComponent<Image>();
            background.color = secondary ? new Color(.11f, .19f, .26f, .97f) :
                new Color(.04f, .34f, .44f, .98f);
            Button b = go.AddComponent<Button>();
            b.targetGraphic = background;
            var colors = b.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(.65f, .80f, .90f, 1f);
            b.colors = colors;
            b.onClick.AddListener(() => { if (hub) command(); Refresh(); });
            var caption = Label(go.transform, "Label", title, 13f,
                new Vector2(8f, -7f), new Vector2(width - 12, 25), textColor, FontStyles.Bold);
            caption.alignment = TextAlignmentOptions.Center;
            return b;
        }

        private void Build()
        {
            if (canvasObject || !hub) return;
            canvasObject = new GameObject("SparkEngineeringV14_Overlay", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 145;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            screenArea = Rect(UI("SafeArea"), canvasObject.transform, Vector2.zero, Vector2.one,
                new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            ApplySafeArea();

            GameObject toggle = UI("SparkEngineering_Toggle");
            Rect(toggle, screenArea, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(16, -125), new Vector2(178, 36));
            Image toggleImage = toggle.AddComponent<Image>();
            toggleImage.color = new Color(.07f, .25f, .32f, .97f);
            Button tb = toggle.AddComponent<Button>();
            tb.targetGraphic = toggleImage;
            tb.onClick.AddListener(() =>
            {
                expanded = !expanded;
                if (panelObject) panelObject.SetActive(expanded);
            });
            var label = Label(toggle.transform, "Label", "SPARK AI  •  MODES", 15f,
                new Vector2(8, -7), new Vector2(164, 25), textColor, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;

            panelObject = UI("SparkEngineering_ControlPanel");
            Rect(panelObject, screenArea, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(16, -167), new Vector2(404, 455));
            Image panelBackground = panelObject.AddComponent<Image>();
            panelBackground.color = panelColor;
            Label(panelObject.transform, "Header", "SUPER AI  •  AUTO DRAW", 19f,
                new Vector2(15, -13), new Vector2(370, 30), accentColor, FontStyles.Bold);
            Label(panelObject.transform, "ModeLabel", "OPERATING MODE", 12f,
                new Vector2(15, -51), new Vector2(370, 20), mutedColor);
            manualButton = MakeButton(panelObject.transform, "MANUAL", 14, -75, 120,
                () => hub.SelectMode(SparkEngineeringControlHub.OperatingMode.Manual), true);
            assistedButton = MakeButton(panelObject.transform, "ASSISTED", 143, -75, 120,
                () => hub.SelectMode(SparkEngineeringControlHub.OperatingMode.Assisted), true);
            autoButton = MakeButton(panelObject.transform, "AUTO AI", 272, -75, 118,
                () => hub.SelectMode(SparkEngineeringControlHub.OperatingMode.Auto), true);
            stateText = Label(panelObject.transform, "RuntimeStatus", "Status", 13f,
                new Vector2(15, -118), new Vector2(374, 92), textColor);
            MakeButton(panelObject.transform, "PREVIEW", 14, -216, 120,
                () => hub.Preview(), true);
            MakeButton(panelObject.transform, "SUGGEST", 143, -216, 120,
                () => hub.SuggestNext(), true);
            MakeButton(panelObject.transform, "APPROVE ONE", 272, -216, 118,
                () => hub.ApproveSuggested());
            MakeButton(panelObject.transform, "DRAW LEVEL", 14, -257, 120,
                () => hub.DrawCurrentLevel());
            MakeButton(panelObject.transform, "PAUSE", 143, -257, 120,
                () => hub.Pause(), true);
            MakeButton(panelObject.transform, "RESUME", 272, -257, 118,
                () => hub.Resume(), true);
            MakeButton(panelObject.transform, "CANCEL AI", 14, -298, 120,
                () => hub.CancelAutomation(), true);
            MakeButton(panelObject.transform, "EXPLAIN", 143, -298, 120,
                () => hub.ExplainConstruction(), true);
            MakeButton(panelObject.transform, "STATUS", 272, -298, 118,
                () => hub.ConstructionStatus(), true);
            Label(panelObject.transform, "LogCaption", "LAST VERIFIED ACTIONS / SAFETY LOG", 12f,
                new Vector2(15, -345), new Vector2(375, 18), accentColor);
            logText = Label(panelObject.transform, "ActionLog", "No actions yet", 12f,
                new Vector2(15, -371), new Vector2(374, 76), mutedColor);
            panelObject.SetActive(expanded);
            EnsureEventSystem();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current) return;
            GameObject go = new GameObject("SparkEngineering_UI_EventSystem");
            go.AddComponent<EventSystem>();
            Type module = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (module != null && typeof(BaseInputModule).IsAssignableFrom(module))
                go.AddComponent(module);
            else
                go.AddComponent<StandaloneInputModule>();
            go.transform.SetParent(canvasObject.transform, false);
        }

        private void Refresh()
        {
            if (!hub || !stateText || !logText) return;
            stateText.text = hub.ConstructionStatus() +
                "\nSuggested: " + hub.PendingStep;
            var history = hub.RecentMessages;
            var sb = new StringBuilder(512);
            for (int i = Math.Max(0, history.Count - 3); i < history.Count; i++)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append("• ").Append(history[i]);
            }
            logText.text = sb.Length == 0 ? "No engineering actions yet." : sb.ToString();
            Highlight(manualButton, hub.Mode == SparkEngineeringControlHub.OperatingMode.Manual);
            Highlight(assistedButton, hub.Mode == SparkEngineeringControlHub.OperatingMode.Assisted);
            Highlight(autoButton, hub.Mode == SparkEngineeringControlHub.OperatingMode.Auto);
        }

        private void ApplySafeArea()
        {
            if (!screenArea || Screen.width <= 0 || Screen.height <= 0) return;
            var safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (lastScreenSize == size && lastSafeArea == safe) return;
            lastScreenSize = size;
            lastSafeArea = safe;
            screenArea.anchorMin = new Vector2(safe.xMin / size.x, safe.yMin / size.y);
            screenArea.anchorMax = new Vector2(safe.xMax / size.x, safe.yMax / size.y);
        }

        private void Highlight(Button b, bool active)
        {
            if (!b) return;
            Image image = b.GetComponent<Image>();
            if (image) image.color = active ? accentColor : new Color(.11f, .19f, .26f, .97f);
        }
    }
}
