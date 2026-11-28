using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// User-adjustable, persistent Spark Live window. Only changes the presentation
    /// of the existing chat/voice controls. Does not touch gameplay, lessons or solver.
    /// Attach once from SparkProLivePanel.Bind(), and delegate Fit() to RefreshBounds().
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLiveAdjustableWindow : MonoBehaviour
    {
        private const string Prefs = "ProjectSpark.SparkLive.Layout.v8.";
        private const float Margin = 10f;
        private const float MinimumWidth = 425f;
        private const float MinimumHeight = 390f;
        private const float MaximumWidth = 1150f;
        private const float MaximumHeight = 980f;
        private const float CollapsedHeight = 76f;
        private const float MinimumScale = .55f;
        private const float MaximumScale = 1.65f;
        private const float SnapThreshold = 20f;
        private const float SmoothSeconds = .085f;

        private static readonly Color Accent = new Color32(93, 223, 244, 255);
        private static readonly Color Background = new Color32(20, 52, 70, 248);
        private static readonly Color TextColor = new Color32(231, 247, 252, 255);
        private static readonly Color HandleColor = new Color32(91, 190, 213, 175);

        private RectTransform shell;
        private RectTransform canvasRect;
        private RectTransform header;
        private Canvas canvas;
        private CanvasGroup shellGroup;
        private GameObject[] sections;
        private GameObject pageTabs, nexusPage;
        private Action<bool> setNexusVisible;
        private bool showNexus;
        private RectTransform menuPanel;
        private TMP_Text layoutButtonLabel;
        private TMP_Text scaleReadout;
        private TMP_Text widthReadout;
        private TMP_Text heightReadout;
        private TMP_Text opacityReadout;
        private TMP_Text snapReadout;
        private TMP_Text lockReadout;
        private Slider scaleSlider;
        private Slider widthSlider;
        private Slider heightSlider;
        private Slider opacitySlider;

        private Vector2 desiredPosition;
        private Vector2 desiredSize;
        private Vector2 positionVelocity;
        private Vector2 sizeVelocity;
        private float desiredScale;
        private float effectiveScale;
        private float scaleVelocity;
        private float desiredOpacity;
        private float opacityVelocity;
        private float expandedHeight;
        private bool collapsed;
        private bool snap = true;
        private bool locked;
        private bool initialized;
        private bool dragging;
        private bool pendingSave;
        private float nextSaveTime;
        private bool dirtyBounds = true;
        private Rect lastScreenSafeArea;
        private Vector2 lastCanvasSize;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        public static SparkLiveAdjustableWindow Attach(SparkProLivePanel panel, RectTransform console)
        {
            if (!panel || !console) return null;
            var controller = panel.GetComponent<SparkLiveAdjustableWindow>();
            if (!controller) controller = panel.gameObject.AddComponent<SparkLiveAdjustableWindow>();
            controller.Configure(console);
            return controller;
        }

        private void Configure(RectTransform console)
        {
            if (initialized) return;
            shell = console;
            canvasRect = transform as RectTransform;
            canvas = GetComponent<Canvas>();
            if (!shell || !canvasRect || !canvas) return;
            initialized = true;

            // Never allow the old full-screen dimmer to steal clicks outside the window.
            var fullscreenBackdrop = GetComponent<Image>();
            if (fullscreenBackdrop)
            {
                fullscreenBackdrop.raycastTarget = false;
                fullscreenBackdrop.color = Color.clear;
            }
            // Some prefabs keep the dimmer as a direct child of the root.
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child == shell) continue;
                var image = child.GetComponent<Image>();
                if (image)
                {
                    image.raycastTarget = false;
                    image.color = Color.clear;
                }
            }

            shell.anchorMin = shell.anchorMax = shell.pivot = new Vector2(.5f, .5f);
            shellGroup = shell.GetComponent<CanvasGroup>();
            if (!shellGroup) shellGroup = shell.gameObject.AddComponent<CanvasGroup>();
            header = shell.Find("Header") as RectTransform;
            sections = new GameObject[]
            {
                Section("Voice visualizer"), Section("Conversation"), Section("Live transcript"),
                Section("Question input"), Section("Controls"), Section("Footer")
            };

            desiredSize = new Vector2(
                Mathf.Clamp(PlayerPrefs.GetFloat(Prefs + "width", 545f), MinimumWidth, MaximumWidth),
                Mathf.Clamp(PlayerPrefs.GetFloat(Prefs + "height", 675f), MinimumHeight, MaximumHeight));
            expandedHeight = desiredSize.y;
            desiredScale = Mathf.Clamp(PlayerPrefs.GetFloat(Prefs + "scale", .92f), MinimumScale, MaximumScale);
            desiredOpacity = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefs + "opacity", 1f));
            collapsed = PlayerPrefs.GetInt(Prefs + "collapsed", 0) != 0;
            snap = PlayerPrefs.GetInt(Prefs + "snap", 1) != 0;
            locked = PlayerPrefs.GetInt(Prefs + "locked", 0) != 0;
            if (collapsed) desiredSize.y = CollapsedHeight;

            if (header)
            {
                var headerImage = header.GetComponent<Image>();
                if (!headerImage) headerImage = header.gameObject.AddComponent<Image>();
                headerImage.color = Color.clear;
                headerImage.raycastTarget = true;
                var mover = header.GetComponent<SparkLiveWindowPointer>();
                if (!mover) mover = header.gameObject.AddComponent<SparkLiveWindowPointer>();
                mover.Configure(this, SparkLiveWindowPointer.Mode.Move);
                CreateButton(header, "AAA Layout", "Layout", new Vector2(-248, -47),
                    new Vector2(-164, -13), ToggleLayoutMenu, out layoutButtonLabel);
            }

            CreateResizeHandles();
            CreateLayoutMenu();
            SetCollapsedSections();
            RefreshReadouts();
            dirtyBounds = true;
            UpdateSafeBounds();
            if (PlayerPrefs.HasKey(Prefs + "x") && PlayerPrefs.HasKey(Prefs + "y"))
            {
                Rect area = GetSafeCanvasRect();
                desiredPosition = new Vector2(
                    Mathf.Lerp(area.xMin, area.xMax, PlayerPrefs.GetFloat(Prefs + "x", .7f)),
                    Mathf.Lerp(area.yMin, area.yMax, PlayerPrefs.GetFloat(Prefs + "y", .7f)));
            }
            else
            {
                PositionInCorner(1, 1, false);
            }
            ClampTargets();
            // Immediately establish a valid layout; future changes are smoothly animated.
            shell.sizeDelta = desiredSize;
            shell.anchoredPosition = desiredPosition;
            shell.localScale = Vector3.one * effectiveScale;
            shellGroup.alpha = desiredOpacity;
        }

        private GameObject Section(string name)
        {
            var target = shell.Find(name);
            return target ? target.gameObject : null;
        }

        /// <summary>Called from the existing panel's Fit(). Does not override player layout.</summary>
        public void RefreshBounds()
        {
            if (!initialized) return;
            UpdateSafeBounds();
        }

        private void UpdateSafeBounds()
        {
            if (!initialized || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            Vector2 canvasSize = canvasRect.rect.size;
            if (!dirtyBounds && safe == lastScreenSafeArea &&
                canvasSize == lastCanvasSize && Screen.width == lastScreenWidth &&
                Screen.height == lastScreenHeight) return;
            dirtyBounds = false;
            lastScreenSafeArea = safe;
            lastCanvasSize = canvasSize;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ClampTargets();
        }

        private Rect GetSafeCanvasRect()
        {
            if (!canvasRect || Screen.width <= 0 || Screen.height <= 0)
                return new Rect(-700f, -400f, 1400f, 800f);
            Rect safe = Screen.safeArea;
            Vector2 min;
            Vector2 max;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, safe.min, null, out min);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, safe.max, null, out max);
            if (min.x > max.x) { float t = min.x; min.x = max.x; max.x = t; }
            if (min.y > max.y) { float t = min.y; min.y = max.y; max.y = t; }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void ClampTargets()
        {
            Rect area = GetSafeCanvasRect();
            desiredSize.x = Mathf.Clamp(desiredSize.x, MinimumWidth, MaximumWidth);
            desiredSize.y = collapsed ? CollapsedHeight :
                Mathf.Clamp(desiredSize.y, MinimumHeight, MaximumHeight);
            desiredScale = Mathf.Clamp(desiredScale, MinimumScale, MaximumScale);
            float availableWidth = Mathf.Max(50f, area.width - Margin * 2f);
            float availableHeight = Mathf.Max(50f, area.height - Margin * 2f);
            effectiveScale = Mathf.Min(desiredScale,
                availableWidth / Mathf.Max(1f, desiredSize.x),
                availableHeight / Mathf.Max(1f, desiredSize.y));
            effectiveScale = Mathf.Max(.1f, effectiveScale);
            Vector2 half = desiredSize * (.5f * effectiveScale);
            desiredPosition = new Vector2(
                Mathf.Clamp(desiredPosition.x, area.xMin + Margin + half.x,
                    area.xMax - Margin - half.x),
                Mathf.Clamp(desiredPosition.y, area.yMin + Margin + half.y,
                    area.yMax - Margin - half.y));
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            UpdateSafeBounds();
            float dt = Mathf.Min(.05f, Time.unscaledDeltaTime);
            if (dt <= 0f) return;
            Vector2 p = Vector2.SmoothDamp(shell.anchoredPosition, desiredPosition,
                ref positionVelocity, SmoothSeconds, Mathf.Infinity, dt);
            Vector2 size = Vector2.SmoothDamp(shell.sizeDelta, desiredSize,
                ref sizeVelocity, SmoothSeconds, Mathf.Infinity, dt);
            float s = Mathf.SmoothDamp(shell.localScale.x, effectiveScale,
                ref scaleVelocity, SmoothSeconds, Mathf.Infinity, dt);
            shell.anchoredPosition = Vector2.Distance(p, desiredPosition) < .02f ? desiredPosition : p;
            shell.sizeDelta = Vector2.Distance(size, desiredSize) < .02f ? desiredSize : size;
            shell.localScale = new Vector3(s, s, 1f);
            shellGroup.alpha = Mathf.SmoothDamp(shellGroup.alpha, desiredOpacity,
                ref opacityVelocity, SmoothSeconds, Mathf.Infinity, dt);
            if (pendingSave && Time.unscaledTime >= nextSaveTime) SaveLayout();
        }

        private void OnDisable() { if (initialized) SaveLayout(); }
        private void OnDestroy() { if (initialized) SaveLayout(); }

        private void QueueSave()
        {
            pendingSave = true;
            nextSaveTime = Time.unscaledTime + .45f;
        }

        private void SaveLayout()
        {
            pendingSave = false;
            Rect area = GetSafeCanvasRect();
            PlayerPrefs.SetFloat(Prefs + "x", Mathf.Clamp01((desiredPosition.x - area.xMin) / Mathf.Max(area.width, 1f)));
            PlayerPrefs.SetFloat(Prefs + "y", Mathf.Clamp01((desiredPosition.y - area.yMin) / Mathf.Max(area.height, 1f)));
            PlayerPrefs.SetFloat(Prefs + "width", desiredSize.x);
            PlayerPrefs.SetFloat(Prefs + "height", collapsed ? expandedHeight : desiredSize.y);
            PlayerPrefs.SetFloat(Prefs + "scale", desiredScale);
            PlayerPrefs.SetFloat(Prefs + "opacity", desiredOpacity);
            PlayerPrefs.SetInt(Prefs + "collapsed", collapsed ? 1 : 0);
            PlayerPrefs.SetInt(Prefs + "snap", snap ? 1 : 0);
            PlayerPrefs.SetInt(Prefs + "locked", locked ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void BeginPointer() { dragging = true; }
        public void EndPointer()
        {
            if (!dragging) return;
            dragging = false;
            if (snap && !locked) SnapToEdge();
            ClampTargets();
            SaveLayout();
        }

        private Vector2 ScreenToCanvasDelta(Vector2 screenDelta)
        {
            return screenDelta / Mathf.Max(.0001f, canvas ? canvas.scaleFactor : 1f);
        }

        public void Move(Vector2 screenDelta)
        {
            if (!initialized || locked) return;
            desiredPosition += ScreenToCanvasDelta(screenDelta);
            ClampTargets();
        }

        // x and y are -1, 0 or +1, indicating the edge/corner being dragged.
        public void Resize(Vector2 screenDelta, int x, int y)
        {
            if (!initialized || locked || collapsed) return;
            Vector2 delta = ScreenToCanvasDelta(screenDelta) / Mathf.Max(.1f, effectiveScale);
            Vector2 previous = desiredSize;
            desiredSize.x = Mathf.Clamp(previous.x + x * delta.x, MinimumWidth, MaximumWidth);
            desiredSize.y = Mathf.Clamp(previous.y + y * delta.y, MinimumHeight, MaximumHeight);
            Vector2 applied = desiredSize - previous;
            // Keep opposite edge fixed. The target position uses canvas units.
            desiredPosition += new Vector2(x * applied.x, y * applied.y) * (.5f * effectiveScale);
            expandedHeight = desiredSize.y;
            ClampTargets();
            RefreshReadouts();
        }

        public void Zoom(float wheelDelta)
        {
            if (!initialized || locked || Mathf.Abs(wheelDelta) < .001f) return;
            SetScale(desiredScale + Mathf.Sign(wheelDelta) * .055f);
        }

        private void SetScale(float value)
        {
            desiredScale = Mathf.Clamp(value, MinimumScale, MaximumScale);
            ClampTargets();
            RefreshReadouts();
            QueueSave();
        }

        private void SetWidth(float value)
        {
            desiredSize.x = Mathf.Clamp(value, MinimumWidth, MaximumWidth);
            ClampTargets();
            RefreshReadouts();
            QueueSave();
        }

        private void SetHeight(float value)
        {
            expandedHeight = Mathf.Clamp(value, MinimumHeight, MaximumHeight);
            if (!collapsed) desiredSize.y = expandedHeight;
            ClampTargets();
            RefreshReadouts();
            QueueSave();
        }

        private void SetOpacity(float value)
        {
            desiredOpacity = Mathf.Clamp(value, .60f, 1f);
            RefreshReadouts();
            QueueSave();
        }

        private void SnapToEdge()
        {
            Rect area = GetSafeCanvasRect();
            Vector2 half = desiredSize * (effectiveScale * .5f);
            float left = area.xMin + Margin + half.x;
            float right = area.xMax - Margin - half.x;
            float top = area.yMax - Margin - half.y;
            float bottom = area.yMin + Margin + half.y;
            if (Mathf.Abs(desiredPosition.x - left) <= SnapThreshold) desiredPosition.x = left;
            else if (Mathf.Abs(desiredPosition.x - right) <= SnapThreshold) desiredPosition.x = right;
            if (Mathf.Abs(desiredPosition.y - top) <= SnapThreshold) desiredPosition.y = top;
            else if (Mathf.Abs(desiredPosition.y - bottom) <= SnapThreshold) desiredPosition.y = bottom;
        }

        private void PositionInCorner(int x, int y, bool save = true)
        {
            Rect area = GetSafeCanvasRect();
            ClampTargets();
            Vector2 half = desiredSize * (effectiveScale * .5f);
            float px = x < 0 ? area.xMin + Margin + half.x :
                x > 0 ? area.xMax - Margin - half.x : area.center.x;
            float py = y < 0 ? area.yMin + Margin + half.y :
                y > 0 ? area.yMax - Margin - half.y : area.center.y;
            desiredPosition = new Vector2(px, py);
            ClampTargets();
            if (save) SaveLayout();
        }

        private void ResetLayout()
        {
            collapsed = false;
            expandedHeight = 675f;
            desiredSize = new Vector2(545f, expandedHeight);
            desiredScale = .92f;
            desiredOpacity = 1f;
            snap = true;
            locked = false;
            SetCollapsedSections();
            PositionInCorner(1, 1);
            RefreshReadouts();
        }

        public void ToggleCollapse()
        {
            collapsed = !collapsed;
            if (collapsed)
            {
                expandedHeight = Mathf.Clamp(desiredSize.y, MinimumHeight, MaximumHeight);
                desiredSize.y = CollapsedHeight;
                if (menuPanel) menuPanel.gameObject.SetActive(false);
            }
            else desiredSize.y = expandedHeight;
            SetCollapsedSections();
            ClampTargets();
            RefreshReadouts();
            SaveLayout();
        }

        internal void RegisterPage(GameObject tabs, GameObject page, Action<bool> setVisible)
        {
            pageTabs = tabs; nexusPage = page; setNexusVisible = setVisible;
            // Keep resize grips and the layout menu on top of page content.
            for (int i = 0; i < 8; i++)
            {
                var handle = shell.Find("AAAResize" + i);
                if (handle) handle.SetAsLastSibling();
            }
            if (menuPanel) menuPanel.SetAsLastSibling();
            SetCollapsedSections();
        }

        internal void SetNexusPage(bool visible)
        {
            showNexus = visible;
            if (visible && collapsed) ToggleCollapse();
            else SetCollapsedSections();
        }

        private void SetCollapsedSections()
        {
            if (sections != null)
                for (int i = 0; i < sections.Length; i++)
                    if (sections[i]) sections[i].SetActive(!collapsed && !showNexus);
            if (pageTabs) pageTabs.SetActive(!collapsed);
            if (nexusPage)
            {
                bool visible = !collapsed && showNexus;
                nexusPage.SetActive(visible);
                setNexusVisible?.Invoke(visible);
            }
            var voiceSettings = Section("Voice settings");
            if (collapsed && voiceSettings) voiceSettings.SetActive(false);
            if (layoutButtonLabel) layoutButtonLabel.text = collapsed ? "Restore" : "Layout";
            // Resize edges hidden in compact mode, header remains draggable.
            for (int i = 0; i < 8; i++)
            {
                var handle = shell.Find("AAAResize" + i);
                if (handle) handle.gameObject.SetActive(!collapsed);
            }
        }

        private void ToggleLayoutMenu()
        {
            if (collapsed) { ToggleCollapse(); return; }
            if (menuPanel) menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
            RefreshReadouts();
        }

        private void RefreshReadouts()
        {
            if (scaleReadout) scaleReadout.text = "UI scale  " + Mathf.RoundToInt(desiredScale * 100f) + "%";
            if (widthReadout) widthReadout.text = "Width  " + Mathf.RoundToInt(desiredSize.x);
            if (heightReadout) heightReadout.text = "Height  " + Mathf.RoundToInt(expandedHeight);
            if (opacityReadout) opacityReadout.text = "Opacity  " + Mathf.RoundToInt(desiredOpacity * 100f) + "%";
            if (snapReadout) snapReadout.text = snap ? "Snap ON" : "Snap OFF";
            if (lockReadout) lockReadout.text = locked ? "Unlock" : "Lock";
            if (scaleSlider) scaleSlider.SetValueWithoutNotify(desiredScale);
            if (widthSlider) widthSlider.SetValueWithoutNotify(desiredSize.x);
            if (heightSlider) heightSlider.SetValueWithoutNotify(expandedHeight);
            if (opacitySlider) opacitySlider.SetValueWithoutNotify(desiredOpacity);
        }

        private void CreateResizeHandles()
        {
            // Corner and edge hit targets are intentionally inside the panel so
            // the uncovered laboratory remains fully clickable.
            int[] xs = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] ys = { 1, 1, 1, 0, 0, -1, -1, -1 };
            for (int i = 0; i < xs.Length; i++)
            {
                int x = xs[i], y = ys[i];
                string name = "AAAResize" + i;
                var existing = shell.Find(name) as RectTransform;
                RectTransform rt;
                if (existing) rt = existing;
                else
                {
                    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(shell, false);
                    rt = (RectTransform)go.transform;
                }
                rt.anchorMin = rt.anchorMax = new Vector2((x + 1) * .5f, (y + 1) * .5f);
                rt.pivot = new Vector2((x + 1) * .5f, (y + 1) * .5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(x == 0 ? 64f : 18f, y == 0 ? 64f : 18f);
                var image = rt.GetComponent<Image>();
                image.color = (x != 0 && y != 0) ? HandleColor : new Color32(91, 190, 213, 42);
                image.raycastTarget = true;
                var pointer = rt.GetComponent<SparkLiveWindowPointer>();
                if (!pointer) pointer = rt.gameObject.AddComponent<SparkLiveWindowPointer>();
                pointer.Configure(this, SparkLiveWindowPointer.Mode.Resize, x, y);
                rt.SetAsLastSibling();
            }
        }

        private void CreateLayoutMenu()
        {
            var menu = new GameObject("AAALayoutMenu", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            menu.transform.SetParent(shell, false);
            menuPanel = (RectTransform)menu.transform;
            menuPanel.anchorMin = menuPanel.anchorMax = new Vector2(1f, 1f);
            menuPanel.pivot = new Vector2(1f, 1f);
            menuPanel.anchoredPosition = new Vector2(-9f, -80f);
            menuPanel.sizeDelta = new Vector2(340f, 430f);
            var image = menu.GetComponent<Image>();
            image.color = new Color32(13, 33, 47, 252);
            image.raycastTarget = true;
            var title = CreateLabel(menuPanel, "Window layout · drag header / resize edges", 13);
            Place(title.rectTransform, 10, 9, 320, 26);

            scaleSlider = AddSlider(menuPanel, "scale", 44f, MinimumScale, MaximumScale,
                desiredScale, SetScale, out scaleReadout);
            widthSlider = AddSlider(menuPanel, "width", 99f, MinimumWidth, MaximumWidth,
                desiredSize.x, SetWidth, out widthReadout);
            heightSlider = AddSlider(menuPanel, "height", 154f, MinimumHeight, MaximumHeight,
                expandedHeight, SetHeight, out heightReadout);
            opacitySlider = AddSlider(menuPanel, "opacity", 209f, .60f, 1f,
                desiredOpacity, SetOpacity, out opacityReadout);

            CreateMenuButton("TL", 11, 275, 54, 34, () => PositionInCorner(-1, 1));
            CreateMenuButton("TR", 69, 275, 54, 34, () => PositionInCorner(1, 1));
            CreateMenuButton("Center", 127, 275, 78, 34, () => PositionInCorner(0, 0));
            CreateMenuButton("BL", 209, 275, 54, 34, () => PositionInCorner(-1, -1));
            CreateMenuButton("BR", 267, 275, 54, 34, () => PositionInCorner(1, -1));

            CreateMenuButton("Snap", 11, 319, 101, 35, () => { snap = !snap; RefreshReadouts(); SaveLayout(); }, out snapReadout);
            CreateMenuButton("Lock", 119, 319, 101, 35, () => { locked = !locked; RefreshReadouts(); SaveLayout(); }, out lockReadout);
            CreateMenuButton("Reset", 227, 319, 94, 35, ResetLayout);
            CreateMenuButton("Minimize", 11, 367, 151, 37, ToggleCollapse);
            CreateMenuButton("Done", 170, 367, 151, 37, () => { menuPanel.gameObject.SetActive(false); SaveLayout(); });

            menuPanel.SetAsLastSibling();
            menuPanel.gameObject.SetActive(false);
        }

        private void CreateMenuButton(string name, float x, float y, float w, float h,
            Action action, out TMP_Text label)
        {
            var button = CreateButton(menuPanel, "AAA " + name, name, Vector2.zero, Vector2.zero, action, out label);
            Place((RectTransform)button.transform, x, y, w, h);
        }

        private void CreateMenuButton(string name, float x, float y, float w, float h, Action action)
        {
            TMP_Text unused;
            CreateMenuButton(name, x, y, w, h, action, out unused);
        }

        private static Button CreateButton(RectTransform parent, string name, string text,
            Vector2 min, Vector2 max, Action action, out TMP_Text label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.offsetMin = min;
            rt.offsetMax = max;
            var image = go.GetComponent<Image>();
            image.color = Background;
            image.raycastTarget = true;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());
            label = CreateLabel(rt, text, 13);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static TMP_Text CreateLabel(RectTransform parent, string text, float fontSize)
        {
            var go = new GameObject("Label", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.color = TextColor;
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            return label;
        }

        // Place within a menu using top-left coordinates.
        private static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static Slider AddSlider(RectTransform parent, string name, float y,
            float min, float max, float current, Action<float> callback, out TMP_Text readout)
        {
            readout = CreateLabel(parent, name, 14);
            Place(readout.rectTransform, 14f, y, 310f, 19f);
            var root = new GameObject("AAA " + name + " slider", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            Place(rt, 15f, y + 23f, 308f, 20f);
            root.GetComponent<Image>().color = new Color32(23, 58, 75, 255);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(rt, false);
            var hrt = (RectTransform)handle.transform;
            hrt.anchorMin = hrt.anchorMax = new Vector2(.5f, .5f);
            hrt.sizeDelta = new Vector2(16f, 24f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Accent;
            handleImage.raycastTarget = true;
            var slider = root.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.direction = Slider.Direction.LeftToRight;
            slider.handleRect = hrt;
            slider.targetGraphic = handleImage;
            slider.SetValueWithoutNotify(current);
            slider.onValueChanged.AddListener(v => callback(v));
            return slider;
        }
    }

    /// <summary>Pointer capture for move, resize, double-click collapse and Ctrl+wheel zoom.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLiveWindowPointer : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IScrollHandler
    {
        public enum Mode { Move, Resize }
        private SparkLiveAdjustableWindow window;
        private Mode mode;
        private int x;
        private int y;

        public void Configure(SparkLiveAdjustableWindow owner, Mode behavior, int edgeX = 0, int edgeY = 0)
        {
            window = owner;
            mode = behavior;
            x = edgeX;
            y = edgeY;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left && window) window.BeginPointer();
        }
        public void OnDrag(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || !window) return;
            if (mode == Mode.Move) window.Move(e.delta);
            else window.Resize(e.delta, x, y);
        }
        public void OnEndDrag(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left && window) window.EndPointer();
        }
        public void OnPointerClick(PointerEventData e)
        {
            if (mode == Mode.Move && e.clickCount >= 2 && e.button == PointerEventData.InputButton.Left && window)
                window.ToggleCollapse();
        }
        private static bool ControlPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                ((Keyboard.current.leftCtrlKey != null && Keyboard.current.leftCtrlKey.isPressed) ||
                 (Keyboard.current.rightCtrlKey != null && Keyboard.current.rightCtrlKey.isPressed)))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
            return false;
#endif
        }

        public void OnScroll(PointerEventData e)
        {
            if (mode != Mode.Move || !window) return;
            if (ControlPressed())
            {
                window.Zoom(e.scrollDelta.y);
                e.Use();
            }
        }
    }
}
