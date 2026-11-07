#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates a complete read-only live voltage dashboard in the open scene.</summary>
    public static class SparkLevelVoltageMonitorEditor
    {
        private static readonly Color Panel = Hex("#101F2B");
        private static readonly Color Header = Hex("#163246");
        private static readonly Color Surface = Hex("#0B1822");
        private static readonly Color Accent = Hex("#39C4EE");
        private static readonly Color White = Hex("#E8F5FA");
        private static readonly Color Muted = Hex("#8FA7B8");

        [MenuItem("Tools/Project Spark/Monitoring/Create Live Voltage Monitor UI")]
        public static void Create()
        {
            SparkLevelVoltageMonitor existing =
                Object.FindFirstObjectByType<SparkLevelVoltageMonitor>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                EditorUtility.DisplayDialog("Project Spark", "A Live Voltage Monitor already exists in this scene. Selected the existing monitor.", "OK");
                return;
            }

            GameObject canvasObject = new GameObject(
                "Spark_VoltageMonitor_Canvas",
                typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Spark Voltage Monitor");

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform panel = Box("VoltageMonitor_Panel", canvasObject.transform,
                Panel, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(650f, 730f), new Vector2(-25f, -25f));
            panel.anchorMin = panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);

            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#2F6379");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            RectTransform header = Box("Header", panel, Header,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 66f), Vector2.zero);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            TextMeshProUGUI title = Label("Title", header, "VOLTAGE MONITOR  LIVE",
                24f, Accent, new Vector2(17f, -8f), new Vector2(420f, 43f));
            title.fontStyle = FontStyles.Bold;

            RectTransform pauseRect = Box("Pause_Button", panel, Hex("#24516A"),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(83f, 34f), new Vector2(-112f, -17f));
            pauseRect.pivot = new Vector2(1f, 1f);
            pauseRect.GetComponent<Image>().raycastTarget = true;
            Button pauseButton = pauseRect.gameObject.AddComponent<Button>();
            TextMeshProUGUI pauseText = Label("Text", pauseRect, "PAUSE",
                13f, White, Vector2.zero, Vector2.zero);
            Stretch(pauseText.rectTransform, 0f, 0f, 0f, 0f);
            pauseText.alignment = TextAlignmentOptions.Center;

            RectTransform clearRect = Box("Clear_Button", panel, Hex("#304655"),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(78f, 34f), new Vector2(-20f, -17f));
            clearRect.pivot = new Vector2(1f, 1f);
            clearRect.GetComponent<Image>().raycastTarget = true;
            Button clearButton = clearRect.gameObject.AddComponent<Button>();
            TextMeshProUGUI clearText = Label("Text", clearRect, "CLEAR",
                13f, White, Vector2.zero, Vector2.zero);
            Stretch(clearText.rectTransform, 0f, 0f, 0f, 0f);
            clearText.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI summary = Label("Summary", panel,
                "No level loaded", 15f, Muted,
                new Vector2(16f, -78f), new Vector2(620f, 55f));

            Label("ReadingsLabel", panel, "LIVE VOLTAGES  •  TERMINALS + COMPONENTS",
                14f, Accent, new Vector2(16f, -145f), new Vector2(600f, 22f));
            TextMeshProUGUI readings = CreateScroll(
                "Readings", panel, -174f, 315f, "Load a level to see voltages.");

            Label("HistoryLabel", panel, "VOLTAGE CHANGE HISTORY",
                14f, Accent, new Vector2(16f, -503f), new Vector2(600f, 22f));
            TextMeshProUGUI history = CreateScroll(
                "History", panel, -533f, 160f, "No changes yet.");

            Label("Footer", panel,
                "Read-only • Samples every solved update • Scroll to inspect channels",
                12f, Muted, new Vector2(16f, -705f), new Vector2(620f, 18f));

            SparkLevelVoltageMonitor monitor =
                canvasObject.AddComponent<SparkLevelVoltageMonitor>();
            SerializedObject so = new SerializedObject(monitor);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("summaryText").objectReferenceValue = summary;
            so.FindProperty("readingsText").objectReferenceValue = readings;
            so.FindProperty("historyText").objectReferenceValue = history;
            so.FindProperty("pauseButtonText").objectReferenceValue = pauseText;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(pauseButton.onClick, monitor.TogglePause);
            UnityEventTools.AddPersistentListener(clearButton.onClick, monitor.ClearHistory);
            EditorUtility.SetDirty(pauseButton);
            EditorUtility.SetDirty(clearButton);
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create UI EventSystem");
            }

            Selection.activeGameObject = canvasObject;
            EditorGUIUtility.PingObject(canvasObject);
            Debug.Log("[Project Spark] Live Voltage Monitor UI created. Save the scene; enter Play Mode and load any level.", canvasObject);
        }

        private static RectTransform Box(string name, Transform parent, Color background,
            Vector2 min, Vector2 max, Vector2 size, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = min;
            r.anchorMax = max;
            r.pivot = new Vector2(0f, 1f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            Image img = go.GetComponent<Image>();
            img.color = background;
            img.raycastTarget = false;
            return r;
        }

        private static TextMeshProUGUI Label(string name, Transform parent,
            string value, float size, Color color, Vector2 pos, Vector2 dimensions)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = pos;
            r.sizeDelta = dimensions;
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = value;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static TextMeshProUGUI CreateScroll(string name, Transform parent,
            float top, float height, string initial)
        {
            RectTransform frame = Box(name + "_Frame", parent, Surface,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-30f, height), new Vector2(15f, top));
            frame.gameObject.AddComponent<Outline>().effectColor = Hex("#24495E");

            GameObject viewportObject = new GameObject("Viewport",
                typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(frame, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            Stretch(viewport, 5f, 5f, 5f, 5f);
            viewportObject.GetComponent<Image>().color = Surface;

            TextMeshProUGUI text = Label("Content", viewport, initial,
                15f, White, Vector2.zero, new Vector2(0f, 30f));
            RectTransform content = text.rectTransform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = new Vector2(10f, content.offsetMin.y);
            content.offsetMax = new Vector2(-10f, content.offsetMax.y);
            content.sizeDelta = new Vector2(-20f, 30f);
            ContentSizeFitter fitter = text.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;

            ScrollRect scroll = frame.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            scroll.inertia = true;
            return text;
        }

        private static Color Hex(string text)
        {
            ColorUtility.TryParseHtmlString(text, out Color color);
            return color;
        }
    }
}
#endif
