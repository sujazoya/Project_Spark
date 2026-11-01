#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectSpark.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.UI.Editor
{
    /// <summary>
    /// Editor-only, one-click three-mode AI learning drawer. Generates a self-contained
    /// UI canvas and leaves all pre-existing Project Spark objects untouched.
    /// </summary>
    public static class SparkAILearningPanelBuilder
    {
        private static readonly Color Back = Hex("071623");
        private static readonly Color Rail = Hex("0B2132");
        private static readonly Color Tile = Hex("102D40");
        private static readonly Color TileHover = Hex("1C4257");
        private static readonly Color Cyan = Hex("43D9F5");
        private static readonly Color Violet = Hex("AE8DFF");
        private static readonly Color Amber = Hex("F2C873");
        private static readonly Color Mint = Hex("62E8B1");
        private static readonly Color White = Hex("EFFAFF");
        private static readonly Color Muted = Hex("91B3C7");
        private const string SpritePath = "Assets/ProjectSpark/_Generated/AILearningPanel/RoundedPanel.png";
        private static Sprite rounded;

        [MenuItem("Tools/Project Spark/AI/Create Complete AI Learning Panel (3 Modes)")]
        public static void Create()
        {
            SparkAILearningPanel existing = Object.FindFirstObjectByType<SparkAILearningPanel>(
                FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[AI Learning Panel] Existing AILearningPanel selected. " +
                          "Use Rebuild menu if you intentionally want to regenerate the UI.", existing);
                return;
            }
            if (!EditorApplication.isPlaying) Build();
            else Debug.LogWarning("[AI Learning Panel] Exit Play Mode before using the Editor builder.");
        }

        [MenuItem("Tools/Project Spark/AI/Rebuild Complete AI Learning Panel (3 Modes)")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AI Learning Panel] Exit Play Mode before rebuilding.");
                return;
            }
            SparkAILearningPanel existing = Object.FindFirstObjectByType<SparkAILearningPanel>(
                FindObjectsInactive.Include);
            if (existing != null)
            {
                bool approved = EditorUtility.DisplayDialog("Rebuild AI Learning Panel",
                    "This replaces only the generated AILearningPanel Canvas and its children. " +
                    "Existing AI systems, shop, training panels and gameplay will not be changed.",
                    "Rebuild Panel", "Cancel");
                if (!approved) return;
                Undo.DestroyObjectImmediate(existing.gameObject);
            }
            Build();
        }

        private static void Build()
        {
            EnsureRoundedSprite();
            GameObject root = new GameObject("AILearningPanel", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "Create AI Learning Panel");

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1600;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            root.GetComponent<GraphicRaycaster>().ignoreReversedGraphics = true;

            // Create the controller before scene objects, assign all references afterwards.
            SparkAILearningPanel controller = root.AddComponent<SparkAILearningPanel>();

            // 01 / Hidden = only a slim floating handle visible.
            GameObject tab = UI("HiddenTab", root.transform);
            SetRightCenter(tab.GetComponent<RectTransform>(), 56f, 126f, -14f);
            Decorate(tab, Rail, true);
            CanvasGroup tabGroup = tab.AddComponent<CanvasGroup>();
            MakeText(tab.transform, "TinyLabel", "SPARK", 10, Cyan,
                new Vector2(4, -14), new Vector2(48, 18), TextAlignmentOptions.Center);
            Button hiddenOpen = ButtonAt(tab.transform, "ExpandToIcons", "AI", 19, White,
                new Vector2(5, -37), new Vector2(46, 62), TileHover);
            MakeText(tab.transform, "Hint", ">", 20, Cyan,
                new Vector2(12, -102), new Vector2(30, 22), TextAlignmentOptions.Center);

            // 02 / Icons = compact scrollable tool rail.
            GameObject iconRail = UI("IconRail", root.transform);
            SetRightStretch(iconRail.GetComponent<RectTransform>(), 84f, -14f, 15f);
            Decorate(iconRail, Rail, true);
            CanvasGroup iconGroup = iconRail.AddComponent<CanvasGroup>();
            MakeText(iconRail.transform, "Brand", "SP", 20, Cyan,
                new Vector2(13, -15), new Vector2(58, 33), TextAlignmentOptions.Center);
            Button iconsHide = ButtonAt(iconRail.transform, "CollapseToHidden", "<", 17, White,
                new Vector2(8, -55), new Vector2(31, 35), Tile);
            Button iconsExpand = ButtonAt(iconRail.transform, "ExpandToDetailed", ">>", 13, Cyan,
                new Vector2(44, -55), new Vector2(31, 35), Tile);
            RectTransform iconScroll = ScrollArea(iconRail.transform, "IconScroll", 103f, 82f,
                out RectTransform iconContent);
            iconScroll.GetComponent<ScrollRect>().scrollSensitivity = 26f;
            MakeBlock(iconRail.transform, "Divider", new Vector2(12, -98), new Vector2(60, 2), Cyan);
            TMP_Text railTip = MakeText(iconRail.transform, "RailStatus", "AI TOOLS", 9, Muted,
                new Vector2(8, 66), new Vector2(68, 58), TextAlignmentOptions.Center,
                false, true);
            // RailStatus is bottom-anchored (top-left helper adjusted by bottom anchor).

            // Hover name for the compact 2-3-character icon badges.
            GameObject tooltip = UI("IconTooltip", root.transform);
            RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
            tooltipRect.anchorMin = tooltipRect.anchorMax = new Vector2(1f, .5f);
            tooltipRect.pivot = new Vector2(1f, .5f);
            tooltipRect.anchoredPosition = new Vector2(-103f, 0);
            tooltipRect.sizeDelta = new Vector2(236f, 47f);
            Image tooltipBg = Decorate(tooltip, Hex("15374A"), false);
            tooltipBg.raycastTarget = false;
            TMP_Text tooltipLabel = MakeText(tooltip.transform, "Name", "", 14, White,
                new Vector2(9, -9), new Vector2(218, 29), TextAlignmentOptions.Center);
            tooltip.SetActive(false);

            // 03 / Detailed = full learning navigation center.
            GameObject detail = UI("DetailedDrawer", root.transform);
            SetRightStretch(detail.GetComponent<RectTransform>(), 440f, -14f, 15f);
            Decorate(detail, Back, true);
            CanvasGroup detailGroup = detail.AddComponent<CanvasGroup>();
            MakeBlock(detail.transform, "TopStripe", new Vector2(0, 0), new Vector2(440, 4), Cyan);
            MakeBlock(detail.transform, "BrandTile",
                new Vector2(20, -17), new Vector2(42, 43), Cyan);
            MakeText(detail.transform, "BrandIcon", "S", 27, Back,
                new Vector2(20, -17), new Vector2(42, 43), TextAlignmentOptions.Center);
            MakeText(detail.transform, "Header", "SPARK  /  AI LEARNING", 18, White,
                new Vector2(75, -16), new Vector2(271, 27));
            MakeText(detail.transform, "TagLine", "TEACH  /  PRACTICE  /  TRACK", 11, Muted,
                new Vector2(75, -44), new Vector2(280, 24));
            Button detailIcons = ButtonAt(detail.transform, "ShrinkToIcons", "<", 17, White,
                new Vector2(351, -17), new Vector2(34, 36), Tile);
            Button detailHide = ButtonAt(detail.transform, "CollapseToHidden", "X", 14, White,
                new Vector2(390, -17), new Vector2(34, 36), Tile);
            MakeBlock(detail.transform, "HeaderSeparator", new Vector2(16, -78), new Vector2(408, 1), Hex("284459"));

            TMP_InputField search = SearchField(detail.transform);
            RectTransform detailScroll = ScrollArea(detail.transform, "ActionsScroll", 154f, 114f,
                out RectTransform detailContent);
            detailScroll.GetComponent<ScrollRect>().scrollSensitivity = 29f;

            // Footer is anchored to the bottom so panel scales to screen size.
            GameObject footer = UI("Footer", detail.transform);
            RectTransform footerRect = footer.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0);
            footerRect.pivot = new Vector2(.5f, 0);
            footerRect.anchoredPosition = new Vector2(0, 0);
            footerRect.sizeDelta = new Vector2(0, 102);
            Decorate(footer, Hex("0B2535"), false);
            TMP_Text availability = MakeText(footer.transform, "AvailableCount",
                "CHECKING AVAILABLE TOOLS...", 11, Cyan,
                new Vector2(16, -11), new Vector2(277, 20));
            Button refresh = ButtonAt(footer.transform, "Refresh", "REFRESH", 11, Cyan,
                new Vector2(329, -8), new Vector2(95, 28), Tile);
            TMP_Text feedback = MakeText(footer.transform, "Feedback",
                "Choose a learning module. Missing modules are disabled.", 11, Muted,
                new Vector2(16, -45), new Vector2(406, 43));
            feedback.enableAutoSizing = true;
            feedback.fontSizeMin = 9;

            var actionBindings = new List<SparkAILearningPanel.ActionBinding>();
            var sections = new List<SparkAILearningPanel.SectionBinding>();
            string previousSection = null;
            for (int i = 0; i < SparkAILearningPanel.Catalog.Length; i++)
            {
                var spec = SparkAILearningPanel.Catalog[i];
                if (previousSection != spec.section)
                {
                    sections.Add(new SparkAILearningPanel.SectionBinding
                    {
                        section = spec.section,
                        header = SectionHeader(detailContent, spec.section, SectionAccent(spec.section))
                    });
                    previousSection = spec.section;
                }
                var binding = new SparkAILearningPanel.ActionBinding
                {
                    id = spec.id
                };
                Color groupColor = SectionAccent(spec.section);
                binding.iconsButton = RailAction(iconContent, spec, groupColor, controller);
                binding.detailedButton = DetailedAction(detailContent, spec, groupColor,
                    out TMP_Text stateLabel, out Image badgeImage);
                binding.availabilityText = stateLabel;
                binding.badgeBackground = badgeImage;
                actionBindings.Add(binding);
            }

            SerializedObject so = new SerializedObject(controller);
            Set(so, "hiddenTab", tab);
            Set(so, "iconRail", iconRail);
            Set(so, "detailedDrawer", detail);
            Set(so, "hiddenGroup", tabGroup);
            Set(so, "iconsGroup", iconGroup);
            Set(so, "detailedGroup", detailGroup);
            Set(so, "openIconsButton", hiddenOpen);
            Set(so, "iconsToHiddenButton", iconsHide);
            Set(so, "iconsToDetailedButton", iconsExpand);
            Set(so, "detailedToIconsButton", detailIcons);
            Set(so, "detailedToHiddenButton", detailHide);
            Set(so, "refreshButton", refresh);
            Set(so, "availableCountText", availability);
            Set(so, "feedbackText", feedback);
            Set(so, "railTipText", railTip);
            Set(so, "iconTooltip", tooltip);
            Set(so, "iconTooltipLabel", tooltipLabel);
            Set(so, "searchInput", search);
            SetBindings(so.FindProperty("actionBindings"), actionBindings);
            SetSections(so.FindProperty("sectionBindings"), sections);
            so.ApplyModifiedPropertiesWithoutUndo();

            tab.SetActive(true);
            iconRail.SetActive(false);
            detail.SetActive(false);

            EnsureEventSystem();
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[AI Learning Panel] Complete: " + actionBindings.Count +
                " connected action buttons in each drawer, 3 modes, runtime module discovery. " +
                "Mode is saved locally. Missing modules are disabled until installed.", root);
        }

        private static void SetBindings(SerializedProperty array,
            List<SparkAILearningPanel.ActionBinding> bindings)
        {
            array.arraySize = bindings.Count;
            for (int i = 0; i < bindings.Count; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("id").stringValue = bindings[i].id;
                element.FindPropertyRelative("iconsButton").objectReferenceValue = bindings[i].iconsButton;
                element.FindPropertyRelative("detailedButton").objectReferenceValue = bindings[i].detailedButton;
                element.FindPropertyRelative("availabilityText").objectReferenceValue = bindings[i].availabilityText;
                element.FindPropertyRelative("badgeBackground").objectReferenceValue = bindings[i].badgeBackground;
            }
        }

        private static void SetSections(SerializedProperty array,
            List<SparkAILearningPanel.SectionBinding> sections)
        {
            array.arraySize = sections.Count;
            for (int i = 0; i < sections.Count; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("section").stringValue = sections[i].section;
                element.FindPropertyRelative("header").objectReferenceValue = sections[i].header;
            }
        }

        private static void Set(SerializedObject so, string property, Object value)
        {
            SerializedProperty p = so.FindProperty(property);
            if (p != null) p.objectReferenceValue = value;
            else Debug.LogError("[AI Learning Panel] Missing serialized field: " + property);
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetRightCenter(RectTransform rect, float width, float height, float inset)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1f, .5f);
            rect.pivot = new Vector2(1f, .5f);
            rect.anchoredPosition = new Vector2(inset, 0);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetRightStretch(RectTransform rect, float width, float inset, float verticalInset)
        {
            rect.anchorMin = new Vector2(1f, 0);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, .5f);
            rect.anchoredPosition = new Vector2(inset, 0);
            rect.sizeDelta = new Vector2(width, -verticalInset * 2f);
        }

        private static Color Hex(string rgb)
        {
            return ColorUtility.TryParseHtmlString("#" + rgb, out Color c) ? c : Color.white;
        }

        private static Color SectionAccent(string section)
        {
            switch (section)
            {
                case "PRACTICE & REPAIR": return Mint;
                case "PROGRESS & ACHIEVEMENTS": return Violet;
                case "QUICK ACTIONS": return Amber;
                default: return Cyan;
            }
        }

        private static Image Decorate(GameObject go, Color color, bool outline)
        {
            Image image = go.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = true;
            if (outline)
            {
                Outline border = go.AddComponent<Outline>();
                border.effectColor = Hex("2C5970");
                border.effectDistance = new Vector2(1, -1);
                border.useGraphicAlpha = true;
            }
            return image;
        }

        private static void MakeBlock(Transform parent, string name, Vector2 position,
            Vector2 size, Color color)
        {
            GameObject go = UI(name, parent);
            RectTransform rect = go.GetComponent<RectTransform>();
            TopLeft(rect, position, size);
            Image image = go.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }

        private static void TopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static TMP_Text MakeText(Transform parent, string name, string contents, int fontSize,
            Color color, Vector2 position, Vector2 size,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft,
            bool background = false, bool bottomAnchored = false, Color? backgroundColor = null)
        {
            GameObject go = UI(name, parent);
            RectTransform rect = go.GetComponent<RectTransform>();
            if (bottomAnchored)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            else TopLeft(rect, position, size);
            if (background)
            {
                Image bg = go.AddComponent<Image>();
                bg.sprite = rounded;
                bg.type = Image.Type.Sliced;
                bg.color = backgroundColor ?? Cyan;
                bg.raycastTarget = false;
            }
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = contents;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = align;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(8, fontSize - 2);
            text.fontSizeMax = fontSize;
            text.raycastTarget = false;
            text.margin = new Vector4(2, 2, 2, 2);
            text.richText = false;
            return text;
        }

        private static Button ButtonAt(Transform parent, string name, string label, int fontSize,
            Color textColor, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = UI(name, parent);
            TopLeft(go.GetComponent<RectTransform>(), position, size);
            Image bg = Decorate(go, color, false);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock state = button.colors;
            state.normalColor = Color.white;
            state.highlightedColor = Hex("B9EDFF");
            state.pressedColor = Hex("79CDD9");
            state.selectedColor = Color.white;
            state.disabledColor = new Color(.42f, .53f, .57f, .42f);
            state.colorMultiplier = 1f;
            button.colors = state;
            TMP_Text text = MakeText(go.transform, "Label", label, fontSize, textColor,
                Vector2.zero, size, TextAlignmentOptions.Center);
            text.fontSizeMin = 9;
            return button;
        }

        private static RectTransform ScrollArea(Transform parent, string name, float topInset,
            float bottomInset, out RectTransform content)
        {
            GameObject scrollGo = UI(name, parent);
            RectTransform scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(9, bottomInset);
            scrollRect.offsetMax = new Vector2(-9, -topInset);

            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 26f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.decelerationRate = .15f;

            GameObject viewportGo = UI("Viewport", scrollGo.transform);
            RectTransform viewport = viewportGo.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            Image transparentImage = viewportGo.AddComponent<Image>();
            transparentImage.color = Color.clear;
            transparentImage.raycastTarget = true;
            viewportGo.AddComponent<RectMask2D>();

            GameObject contentGo = UI("Content", viewportGo.transform);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(3, 3, 7, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            return scrollRect;
        }

        private static GameObject SectionHeader(Transform content, string name, Color color)
        {
            GameObject header = UI("Section_" + name.Replace(" ", ""), content);
            LayoutElement le = header.AddComponent<LayoutElement>();
            le.preferredHeight = 34f;
            MakeText(header.transform, "Category", name, 12, color,
                new Vector2(8, -9), new Vector2(345, 20));
            return header;
        }

        private static Button RailAction(Transform content, SparkAILearningPanel.ActionSpec spec,
            Color accent, SparkAILearningPanel controller)
        {
            GameObject go = UI("Rail_" + spec.id, content);
            go.AddComponent<LayoutElement>().preferredHeight = 54f;
            Image bg = Decorate(go, Tile, false);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            button.colors = ButtonColors();
            GameObject mark = UI("Icon", go.transform);
            TopLeft(mark.GetComponent<RectTransform>(), new Vector2(7, -6), new Vector2(50, 42));
            Image markBg = Decorate(mark, accent, false);
            markBg.raycastTarget = false;
            MakeText(mark.transform, "Glyph", spec.badge, spec.badge.Length > 2 ? 12 : 17,
                Back, Vector2.zero, new Vector2(50, 42), TextAlignmentOptions.Center);
            // Use GameObject name for quick inspection in Unity hierarchy.
            go.name = "Rail_" + spec.title.Replace(" ", "_");
            SparkAILearningHoverHint hover = go.AddComponent<SparkAILearningHoverHint>();
            hover.Configure(controller, spec.id);
            return button;
        }

        private static Button DetailedAction(Transform content, SparkAILearningPanel.ActionSpec spec,
            Color accent, out TMP_Text stateLabel, out Image badgeBackground)
        {
            GameObject go = UI("Action_" + spec.id, content);
            go.AddComponent<LayoutElement>().preferredHeight = 61f;
            Image bg = Decorate(go, Tile, false);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            button.colors = ButtonColors();
            GameObject mark = UI("Badge", go.transform);
            TopLeft(mark.GetComponent<RectTransform>(), new Vector2(11, -11), new Vector2(42, 41));
            badgeBackground = Decorate(mark, accent, false);
            badgeBackground.raycastTarget = false;
            MakeText(mark.transform, "Glyph", spec.badge,
                spec.badge.Length > 2 ? 11 : 15, Back, Vector2.zero,
                new Vector2(42, 41), TextAlignmentOptions.Center);
            MakeText(go.transform, "Title", spec.title, 15, White,
                new Vector2(65, -9), new Vector2(266, 24));
            MakeText(go.transform, "Description", spec.description, 11, Muted,
                new Vector2(65, -33), new Vector2(312, 23));
            stateLabel = MakeText(go.transform, "Availability", "...", 9, accent,
                new Vector2(332, -10), new Vector2(68, 20), TextAlignmentOptions.Center);
            return button;
        }

        private static ColorBlock ButtonColors()
        {
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = Color.white;
            cb.highlightedColor = Hex("C4E8F8");
            cb.pressedColor = Hex("7EDBF1");
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(.34f, .48f, .55f, .47f);
            cb.fadeDuration = .07f;
            return cb;
        }

        private static TMP_InputField SearchField(Transform detail)
        {
            GameObject go = UI("SearchLessonsAndTools", detail);
            TopLeft(go.GetComponent<RectTransform>(), new Vector2(16, -92), new Vector2(408, 44));
            Decorate(go, Hex("173548"), false);
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 60;
            GameObject textArea = UI("TextArea", go.transform);
            RectTransform rt = textArea.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(13, 4);
            rt.offsetMax = new Vector2(-13, -4);
            textArea.AddComponent<RectMask2D>();
            // A TextViewport has CanvasRenderer as required for TMP clipping.
            TMP_Text placeholder = MakeText(textArea.transform, "Placeholder",
                "Search all AI learning tools...", 13, Muted,
                new Vector2(0, 0), new Vector2(382, 38), TextAlignmentOptions.MidlineLeft);
            TMP_Text value = MakeText(textArea.transform, "TypedText", "", 14, White,
                new Vector2(0, 0), new Vector2(382, 38), TextAlignmentOptions.MidlineLeft);
            input.textViewport = rt;
            input.textComponent = (TextMeshProUGUI)value;
            input.placeholder = (TextMeshProUGUI)placeholder;
            return input;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
                return;
            GameObject es = new GameObject("EventSystem_AI_LearningPanel", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(es, "Create AI Learning EventSystem");
            // Prefer the New Input System module when available; keep a legacy fallback.
            Type inputType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputType != null && typeof(BaseInputModule).IsAssignableFrom(inputType))
                es.AddComponent(inputType);
            else
                es.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureRoundedSprite()
        {
            if (rounded != null) return;
            if (!File.Exists(SpritePath))
            {
                MakeFolder("Assets/ProjectSpark");
                MakeFolder("Assets/ProjectSpark/_Generated");
                MakeFolder("Assets/ProjectSpark/_Generated/AILearningPanel");
                const int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    const float radius = 14f;
                    float cx = Mathf.Max(0f, radius - Mathf.Min(x + .5f, size - x - .5f));
                    float cy = Mathf.Max(0f, radius - Mathf.Min(y + .5f, size - y - .5f));
                    float distance = Mathf.Sqrt(cx * cx + cy * cy);
                    float alpha = Mathf.Clamp01(radius + .5f - distance);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                tex.Apply();
                File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null &&
                (importer.textureType != TextureImporterType.Sprite ||
                 importer.spriteBorder != new Vector4(14, 14, 14, 14)))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(14, 14, 14, 14);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static void MakeFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            AssetDatabase.CreateFolder(parent ?? "Assets", Path.GetFileName(path));
        }
    }
}
#endif
