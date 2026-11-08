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
        private static readonly Color Panel = Hex("#0B1221");
        private static readonly Color Header = Hex("#14233B");
        private static readonly Color Surface = Hex("#101D2E");
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

            ApplyCardTheme(panel);

            SparkLevelVoltageMonitor monitor =
                canvasObject.AddComponent<SparkLevelVoltageMonitor>();
            SerializedObject so = new SerializedObject(monitor);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("summaryText").objectReferenceValue = summary;
            so.FindProperty("readingsText").objectReferenceValue = readings;
            so.FindProperty("historyText").objectReferenceValue = history;
            so.FindProperty("pauseButtonText").objectReferenceValue = pauseText;
            so.ApplyModifiedPropertiesWithoutUndo();
            WireLiveCards(canvasObject, readings);

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

        // Safe for existing scenes: changes visuals without replacing the monitor,
        // listeners, serialized monitor references, or voltage-solver behavior.
        [MenuItem("Tools/Project Spark/Monitoring/Restyle Existing Voltage Monitor (Scoreboard)")]
        public static void RestyleExisting()
        {
            SparkLevelVoltageMonitor monitor = Object.FindFirstObjectByType<SparkLevelVoltageMonitor>();
            if (monitor == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "No voltage monitor exists in the open scene. Create one first.", "OK");
                return;
            }
            RectTransform panel = monitor.transform.Find("VoltageMonitor_Panel") as RectTransform;
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "The monitor panel could not be found under the monitor object.", "OK");
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(monitor.gameObject, "Restyle Voltage Monitor");
            ApplyCardTheme(panel);
            TextMeshProUGUI source = FindLiveText(panel);
            WireLiveCards(monitor.gameObject, source);
            EditorUtility.SetDirty(monitor.gameObject);
            Selection.activeGameObject = panel.gameObject;
            Debug.Log("[Project Spark] Voltage monitor refreshed with metallic scoreboard cards.", panel);
        }

        // Restyling is idempotent. Generated objects are always placed BEHIND the
        // functional scroll panels, so glossy decoration cannot cover the values.
        private static void ApplyCardTheme(RectTransform panel)
        {
            if (panel == null) return;
            string[] obsolete = {
                "Chrome_Left", "Chrome_Right", "Chrome_Top_Light", "Chrome_Top_Shadow",
                "Chrome_Bottom", "Chrome_Bottom_Glint", "Card_01_Level_Status",
                "Card_02_Live_Readings", "Card_03_Voltage_History",
                "Status_Backplate", "Readings_Backplate", "History_Backplate",
                "Frame_Left", "Frame_Right", "Frame_Top", "Frame_Bottom"
            };
            foreach (string name in obsolete) DestroyGenerated(panel.Find(name));
            for (int i = panel.childCount - 1; i >= 0; i--)
                if (panel.GetChild(i).name.StartsWith("PS_")) DestroyGenerated(panel.GetChild(i));

            RectTransform header = panel.Find("Header") as RectTransform;
            if (header != null)
            {
                string[] obsoleteHeader = { "Red_Scoreboard_Segment", "Red_Light_Band",
                    "Dark_Scoreboard_Segment", "Glassy_Divider", "Red_Sheen" };
                foreach (string name in obsoleteHeader) DestroyGenerated(header.Find(name));
                for (int i = header.childCount - 1; i >= 0; i--)
                    if (header.GetChild(i).name.StartsWith("PS_")) DestroyGenerated(header.GetChild(i));
            }

            SetImage(panel, Hex("#091529"));
            SetOutline(panel, Hex("#A8C2E4"));
            Trim(panel, "PS_ChromeLeft", 0, 0, 6, 730, "#9BAEC8");
            Trim(panel, "PS_ChromeRight", 644, 0, 6, 730, "#718DAB");
            Trim(panel, "PS_ChromeTop", 0, 0, 650, 5, "#E2EEFF");
            Trim(panel, "PS_ChromeBottom", 0, -725, 650, 5, "#889FBF");
            Trim(panel, "PS_InnerLeft", 8, -8, 2, 712, "#123B60");
            Trim(panel, "PS_InnerRight", 640, -8, 2, 712, "#123B60");

            if (header != null)
            {
                SetImage(header, Hex("#A51026"));
                SetOutline(header, Hex("#FF5264"));
                Trim(header, "PS_RedFlash", 0, 0, 650, 5, "#FFB0BD");
                Trim(header, "PS_RedGloss", 7, -6, 636, 17, "#DA243C");
                Trim(header, "PS_RedBottom", 0, -61, 650, 5, "#660D22");
                Trim(header, "PS_LiveFlag", 461, -14, 86, 36, "#033F65");
                Trim(header, "PS_LiveFlagTop", 461, -14, 86, 3, "#32F6FF");
                TextMeshProUGUI title = GetText(header.Find("Title"));
                if (title != null)
                {
                    title.text = "VOLTAGE MONITOR";
                    title.color = Color.white;
                    title.fontSize = 24f;
                    title.fontStyle = FontStyles.Bold;
                    title.enableAutoSizing = false;
                    title.rectTransform.anchoredPosition = new Vector2(17f, -13f);
                    title.rectTransform.sizeDelta = new Vector2(438f, 43f);
                }
                // "LIVE" is independent of the title because the monitor updates
                // its own title at runtime; the badge stays visible and legible.
                RectTransform flag = header.Find("PS_LiveFlag") as RectTransform;
                if (flag != null)
                {
                    TextMeshProUGUI live = Label("PS_LiveBadge", flag, "LIVE", 18f,
                        Hex("#6AF8FF"), new Vector2(13f, -5f), new Vector2(70f, 27f));
                    live.fontStyle = FontStyles.Bold;
                    live.alignment = TextAlignmentOptions.Center;
                }
            }

            // Entire card backgrounds sit below all original UI elements.
            Card(panel, "PS_StatusCard", 14, -76, 622, 62,
                "#08382E", "#2AEEB6", "#139D75");
            Card(panel, "PS_ReadingsCard", 14, -142, 622, 351,
                "#061F3F", "#2EAFFF", "#147BC9");
            Card(panel, "PS_HistoryCard", 14, -500, 622, 199,
                "#25113D", "#DD67FF", "#862EB6");
            Trim(panel, "PS_ReadingsDivider", 22, -170, 606, 3, "#32D0FF");
            Trim(panel, "PS_HistoryDivider", 22, -531, 606, 3, "#EB8AFF");
            Trim(panel, "PS_FooterBack", 14, -704, 622, 20, "#18304B");

            TextMeshProUGUI summary = GetText(panel.Find("Summary"));
            if (summary != null)
            {
                summary.color = Hex("#E0FFF1");
                summary.fontSize = 16f;
                summary.fontStyle = FontStyles.Bold;
                summary.rectTransform.anchoredPosition = new Vector2(28f, -82f);
                summary.rectTransform.sizeDelta = new Vector2(588f, 51f);
            }
            StyleSectionLabel(panel.Find("ReadingsLabel"), "#DDF7FF", 27f, -147f);
            StyleSectionLabel(panel.Find("HistoryLabel"), "#FBE0FF", 27f, -504f);
            TextMeshProUGUI readingsTitle = GetText(panel.Find("ReadingsLabel"));
            TextMeshProUGUI historyTitle = GetText(panel.Find("HistoryLabel"));
            if (readingsTitle != null) readingsTitle.fontStyle = FontStyles.Bold;
            if (historyTitle != null) historyTitle.fontStyle = FontStyles.Bold;

            Transform readingsFrame = panel.Find("Readings_Frame");
            Transform historyFrame = panel.Find("History_Frame");
            FitFrame(readingsFrame, -178f, 306f, 26f, 26f);
            FitFrame(historyFrame, -540f, 151f, 26f, 26f);
            StyleScroll(readingsFrame, "#081B32", "#F1FAFF", "#226EAB", 15f);
            StyleScroll(historyFrame, "#180E2B", "#FBEFFF", "#8E43BA", 14f);

            TextMeshProUGUI footer = GetText(panel.Find("Footer"));
            if (footer != null)
            {
                footer.color = Hex("#CFDEFC");
                footer.fontSize = 11.5f;
                footer.rectTransform.anchoredPosition = new Vector2(25f, -705f);
                footer.rectTransform.sizeDelta = new Vector2(597f, 18f);
            }

            StyleButton(panel.Find("Pause_Button"), "#04749D", "#65EEFF");
            StyleButton(panel.Find("Clear_Button"), "#84285A", "#FF8CC4");
            // The functional controls and actual readings must always render last.
            string[] foreground = { "Summary", "ReadingsLabel", "Readings_Frame",
                "HistoryLabel", "History_Frame", "Footer", "Header",
                "Pause_Button", "Clear_Button" };
            foreach (string name in foreground)
            {
                Transform t = panel.Find(name);
                if (t != null) t.SetAsLastSibling();
            }
            if (header != null)
            {
                Transform title = header.Find("Title");
                if (title != null) title.SetAsLastSibling();
                Transform live = header.Find("PS_LiveFlag");
                if (live != null) live.SetAsLastSibling();
            }
        }

        private static void FitFrame(Transform target, float y, float height, float insetL, float insetR)
        {
            RectTransform r = target as RectTransform;
            if (r == null) return;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(insetL, y);
            r.sizeDelta = new Vector2(-(insetL + insetR), height);
        }

        private static TextMeshProUGUI FindLiveText(RectTransform panel)
        {
            Transform t = panel.Find("Readings_Frame/Viewport/Content");
            return GetText(t);
        }

        private static void WireLiveCards(GameObject monitorOwner, TextMeshProUGUI source)
        {
            if (source == null || monitorOwner == null) return;
            ScrollRect scroll = source.GetComponentInParent<ScrollRect>();
            if (scroll == null) return;
            SparkVoltageLiveCards cards = monitorOwner.GetComponent<SparkVoltageLiveCards>();
            if (cards == null) cards = Undo.AddComponent<SparkVoltageLiveCards>(monitorOwner);
            cards.Bind(source, scroll);
            EditorUtility.SetDirty(cards);
        }

        private static void DestroyGenerated(Transform t)
        {
            if (t == null) return;
            // Undo-aware destruction works for existing scene objects and during creation.
            if (t.gameObject.scene.IsValid()) Undo.DestroyObjectImmediate(t.gameObject);
            else Object.DestroyImmediate(t.gameObject);
        }

        private static RectTransform Trim(Transform parent, string name,
            float x, float y, float w, float h, string hex)
        {
            if (parent == null) return null;
            RectTransform r = parent.Find(name) as RectTransform;
            if (r == null)
            {
                r = Box(name, parent, Hex(hex), Vector2.up, Vector2.up,
                    new Vector2(w, h), new Vector2(x, y));
            }
            r.anchorMin = r.anchorMax = Vector2.up;
            r.pivot = Vector2.up;
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
            SetImage(r, Hex(hex));
            r.SetAsFirstSibling();
            return r;
        }

        private static void Card(Transform parent, string name, float x, float y,
            float w, float h, string baseColor, string accent, string topColor)
        {
            RectTransform r = Trim(parent, name, x, y, w, h, baseColor);
            SetOutline(r, Hex(accent));
            Trim(r, "Accent_Left", 0, 0, 5, h, accent);
            Trim(r, "Top_Tint", 5, 0, w - 5, 5, topColor);
            Trim(r, "Bottom_Shine", 5, -(h - 2), w - 5, 2, accent);
        }

        private static TextMeshProUGUI GetText(Transform t)
        {
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static void StyleSectionLabel(Transform target, string color, float x, float y)
        {
            var t = GetText(target);
            if (t == null) return;
            t.color = Hex(color);
            t.fontSize = 15;
            t.fontStyle = FontStyles.Bold;
            t.rectTransform.anchoredPosition = new Vector2(x, y);
        }

        private static void StyleScroll(Transform target, string background,
            string foreground, string border, float fontSize)
        {
            if (target == null) return;
            SetImage(target, Hex(background));
            SetOutline(target, Hex(border));
            Transform viewport = target.Find("Viewport");
            SetImage(viewport, Hex(background));
            if (viewport == null) return;
            var text = GetText(viewport.Find("Content"));
            if (text == null) return;
            text.color = Hex(foreground);
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
            text.enableWordWrapping = true;
            text.margin = new Vector4(4f, 6f, 4f, 6f);
        }

        private static void StyleButton(Transform target, string fill, string outline)
        {
            SetImage(target, Hex(fill));
            SetOutline(target, Hex(outline));
        }

        private static void SetImage(Transform target, Color color)
        {
            if (target != null && target.TryGetComponent(out Image image)) image.color = color;
        }

        private static void SetTextColor(Transform target, Color color)
        {
            if (target != null && target.TryGetComponent(out TextMeshProUGUI tmp)) tmp.color = color;
        }

        private static void SetFrameViewport(Transform frame, Color background, Color foreground)
        {
            if (frame == null) return;
            Transform viewport = frame.Find("Viewport");
            SetImage(viewport, background);
            if (viewport != null) SetTextColor(viewport.Find("Content"), foreground);
        }

        private static void SetOutline(Transform target, Color color)
        {
            if (target == null) return;
            Outline border = target.GetComponent<Outline>();
            if (border == null) border = target.gameObject.AddComponent<Outline>();
            border.effectColor = color;
            border.effectDistance = new Vector2(1f, -1f);
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
