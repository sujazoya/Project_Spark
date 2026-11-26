using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Player-invoked advisor; never changes the circuit or gameplay evaluator.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringMissionAdvisorPanel : MonoBehaviour
    {
        [SerializeField] private bool openAtStart;
        private SparkEngineeringMissionAdvisor advisor;
        private GameObject canvasObject;
        private GameObject drawer;
        private TMP_Text reportText;
        private TMP_Text feedbackText;
        private ScrollRect scroll;
        private readonly Color titleColor = new Color(.31f, .89f, .97f);
        private readonly Color textColor = new Color(.91f, .96f, .99f);

        private void Awake() { advisor = GetComponent<SparkEngineeringMissionAdvisor>(); }
        private void OnEnable()
        {
            if (!advisor) advisor = GetComponent<SparkEngineeringMissionAdvisor>();
            if (advisor) advisor.ReportChanged += Refresh;
        }
        private void OnDisable()
        {
            if (advisor) advisor.ReportChanged -= Refresh;
        }
        private void Start() { Build(); Refresh(); }
        private void OnDestroy() { if (canvasObject) Destroy(canvasObject); }

        private static GameObject UI(string name, Transform parent, Vector2 anchors, Vector2 pivot,
            Vector2 position, Vector2 dimensions)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchors;
            rect.anchorMax = anchors;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            return obj;
        }

        private TMP_Text Text(Transform parent, string name, string content, Vector2 position,
            Vector2 size, int fontSize, Color color)
        {
            var obj = UI(name, parent, new Vector2(0, 1), new Vector2(0, 1), position, size);
            TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI>();
            t.fontSize = fontSize;
            t.color = color;
            t.text = content;
            t.raycastTarget = false;
            t.enableWordWrapping = true;
            return t;
        }

        private void Button(Transform parent, string title, Vector2 pos, Vector2 size, Action action)
        {
            var obj = UI(title + "_button", parent, new Vector2(0, 1), new Vector2(0, 1), pos, size);
            var img = obj.AddComponent<Image>();
            img.color = new Color(.08f, .31f, .42f, .98f);
            var button = obj.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() => { if (advisor) action(); });
            var t = Text(obj.transform, "Caption", title, new Vector2(0, 0), size, 13, textColor);
            var rt = t.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(.5f, .5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            t.alignment = TextAlignmentOptions.Center;
        }

        private void Build()
        {
            if (canvasObject || !advisor) return;
            canvasObject = new GameObject("SparkMissionAdvisorV15_Overlay", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 146;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            var tab = UI("MISSION INSPECTOR", canvasObject.transform, new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-18, -125), new Vector2(184, 36));
            var tabImage = tab.AddComponent<Image>();
            tabImage.color = new Color(.07f, .25f, .32f, .97f);
            var toggle = tab.AddComponent<Button>();
            toggle.targetGraphic = tabImage;
            var tabText = Text(tab.transform, "Caption", "SPARK • MISSION", new Vector2(0, 0),
                new Vector2(184, 36), 15, titleColor);
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.rectTransform.anchorMin = Vector2.zero;
            tabText.rectTransform.anchorMax = Vector2.one;
            tabText.rectTransform.offsetMin = Vector2.zero;
            tabText.rectTransform.offsetMax = Vector2.zero;

            drawer = UI("MissionInspectorDrawer", canvasObject.transform, new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-18, -167), new Vector2(460, 510));
            var bg = drawer.AddComponent<Image>();
            bg.color = new Color(.015f, .055f, .10f, .98f);
            Text(drawer.transform, "Title", "LEVEL READINESS / REVIEWED PLAN", new Vector2(14, -12),
                new Vector2(434, 28), 17, titleColor);
            Button(drawer.transform, "INSPECT LEVEL", new Vector2(14, -49), new Vector2(142, 36),
                () => { advisor.Inspect(); if (scroll) scroll.verticalNormalizedPosition = 1; });
            Button(drawer.transform, "EXPORT REPORT", new Vector2(164, -49), new Vector2(140, 36),
                () => { feedbackText.text = advisor.ExportLastReport(); });
            Button(drawer.transform, "CLOSE", new Vector2(312, -49), new Vector2(132, 36),
                () => drawer.SetActive(false));
            feedbackText = Text(drawer.transform, "Feedback", "Read-only. Does not perform wiring.",
                new Vector2(14, -94), new Vector2(430, 28), 11, titleColor);

            GameObject viewport = UI("Viewport", drawer.transform, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(12, -128), new Vector2(436, 363));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(.02f, .10f, .15f, .95f);
            viewport.AddComponent<RectMask2D>();
            GameObject content = UI("ScrollableReport", viewport.transform, new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(8, -6), new Vector2(413, 1800));
            reportText = Text(content.transform, "Report", advisor.LastReport,
                Vector2.zero, new Vector2(409, 1770), 12, textColor);
            reportText.overflowMode = TextOverflowModes.Overflow;
            scroll = viewport.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = content.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            toggle.onClick.AddListener(() => drawer.SetActive(!drawer.activeSelf));
            drawer.SetActive(openAtStart);
            EnsureInput();
        }

        private void EnsureInput()
        {
            if (EventSystem.current) return;
            GameObject eventSystem = new GameObject("SparkMissionAdvisor_EventSystem");
            eventSystem.AddComponent<EventSystem>();
            Type type = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (type != null && typeof(BaseInputModule).IsAssignableFrom(type))
                eventSystem.AddComponent(type);
            else
                eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.transform.SetParent(canvasObject.transform, false);
        }

        private void Refresh()
        {
            if (reportText && advisor)
                reportText.text = advisor.LastReport;
        }
    }
}
