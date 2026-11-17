using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UIManagement
{
    /// <summary>
    /// Creates an interactive UI manager console over the supplied monitor artwork.
    /// This is an ADMIN/DEBUG dashboard; do not show it to players unless intended.
    /// Add this to a separate scene GameObject and assign the manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkUIMonitorDashboard : MonoBehaviour
    {
        [SerializeField] private SparkUIManager manager;
        [SerializeField] private Sprite monitorFrame;
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F8;
        [SerializeField] private int dashboardSortOrder = 30000;
        [SerializeField] private bool keepConsoleOnTop = true;
        [SerializeField] private Color cyan = new Color(.05f, .83f, 1f);
        [SerializeField] private Color baseBlue = new Color(.015f, .065f, .12f, .92f);
        [SerializeField] private Color panelBlue = new Color(.025f, .10f, .16f, .94f);
        [SerializeField] private Color textWhite = new Color(.84f, .96f, 1f);
        private Canvas canvas;
        private RectTransform root;
        private RectTransform registryContent;
        private Text selectedLabel, countLabel, stackLabel, focusLabel, modalLabel, inputLabel, eventLabel, layerLabel, stateLabel;
        private string selectedId = "";
        private string filter = "All";
        private readonly List<string> events = new List<string>();
        private readonly List<GameObject> registryRows = new List<GameObject>();
        private Font uiFont;
        private float refreshAt;
        private bool initialized;
        private string registrySignature = "";
        private GameObject launcher;
        private RectTransform workshopScreen;
        private RectTransform closeBar;
        private Action locateMaster;
        private Action leaveMaster;
        private Action locateWorkshop;
        public bool IsWallMounted => canvas && canvas.renderMode == RenderMode.WorldSpace;
        private string closeSignature;
        private static SparkUIMonitorDashboard current;

        public static bool IsConsoleVisible => current && !current.IsWallMounted && current.canvas && current.canvas.gameObject.activeInHierarchy;

        private void Awake()
        {
            current = this;
            if (!manager) manager = SparkUIManager.Instance;
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!uiFont) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Build();
            if (manager) { manager.Changed += Refresh; manager.LogEvent += OnEvent; }
            canvas.gameObject.SetActive(showOnStart);
        }
        private void OnDestroy()
        {
            if (current == this) current = null;
            if (launcher) Destroy(launcher);
            if (IsWallMounted) Destroy(canvas.gameObject);
            else if (workshopScreen) Destroy(workshopScreen.gameObject);
            if (manager) { manager.Changed -= Refresh; manager.LogEvent -= OnEvent; }
        }
        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsWallMounted) locateMaster?.Invoke();
                else SetVisible(!canvas.gameObject.activeSelf);
            }
            if (IsConsoleVisible && Input.GetKeyDown(KeyCode.Escape)) SetVisible(false);
#endif
            FitWorkshopScreen();
            if (Time.unscaledTime >= refreshAt) { refreshAt = Time.unscaledTime + .5f; Refresh(); }
        }
        public void SetVisible(bool visible)
        {
            if (!canvas) return;
            canvas.gameObject.SetActive(IsWallMounted || visible);
            if (launcher) launcher.SetActive(!visible);
            UpdateCloseButtons();
        }

        public void AttachToWorkshopMonitor(Canvas workshopCanvas)
        {
            if (!workshopCanvas || launcher) return;
            workshopScreen=(RectTransform)Obj("Spark Master Monitor Screen",workshopCanvas.transform).transform;
            workshopScreen.anchorMin=Vector2.zero;
            workshopScreen.anchorMax=Vector2.one;
            workshopScreen.offsetMin=workshopScreen.offsetMax=Vector2.zero;
            root.SetParent(workshopScreen,false);
            root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);
            root.pivot=new Vector2(.5f,.5f);
            root.anchoredPosition=Vector2.zero;
            root.sizeDelta=new Vector2(1672,941);
            FitWorkshopScreen();

            launcher=Obj("Spark Master Monitor Launcher",workshopCanvas.transform);
            var rect=(RectTransform)launcher.transform;
            rect.anchorMin=rect.anchorMax=new Vector2(1,1);
            rect.pivot=new Vector2(1,1);
            rect.anchoredPosition=new Vector2(-24,-22);
            rect.sizeDelta=new Vector2(252,48);
            var launcherCanvas=launcher.AddComponent<Canvas>();
            launcherCanvas.overrideSorting=true;
            launcherCanvas.sortingOrder=2100;
            launcher.AddComponent<GraphicRaycaster>();
            var input=launcher.AddComponent<Image>();
            input.color=new Color(.025f,.14f,.22f,.98f);
            var button=launcher.AddComponent<Button>();
            button.targetGraphic=input;
            button.onClick.AddListener(()=>SetVisible(true));
            var label=Obj("Monitor label",launcher.transform).AddComponent<Text>();
            label.font=uiFont;
            label.fontSize=16;
            label.fontStyle=FontStyle.Bold;
            label.alignment=TextAnchor.MiddleCenter;
            label.color=textWhite;
            label.text="MASTER MONITOR    F8";
            label.raycastTarget=false;
            label.rectTransform.anchorMin=Vector2.zero;
            label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            launcher.SetActive(!IsConsoleVisible);
        }
        internal void AttachToWall(Canvas surface, Action locate, Action leave, Action locateRight)
        {
            locateMaster = locate;
            leaveMaster = leave;
            locateWorkshop = locateRight;
            workshopScreen = (RectTransform)surface.transform;
            root.SetParent(workshopScreen, false);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = surface.worldCamera;
            canvas.overrideSorting = false;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler) scaler.enabled = false;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, .5f);
            root.anchoredPosition3D = Vector3.zero;
            root.sizeDelta = new Vector2(1672, 941);
            FitWorkshopScreen();
            canvas.gameObject.SetActive(true);
            RelabelButton("Button CLOSE  ×", "VIEW RIGHT WALL");
            RelabelButton("Button HIDE CONSOLE  [F8]", "VIEW WORKTOP");
            CreateCloseBar();
        }

        private void RelabelButton(string name, string caption)
        {
            var button = root.Find(name);
            if (button) button.GetComponentInChildren<Text>().text = caption;
        }

        private void ReturnToWorktop()
        {
            if (IsWallMounted) leaveMaster?.Invoke();
            else SetVisible(false);
        }

        internal void RegisterRecyclingHelp()
        {
            var notice = Obj("Recycling Help", transform);
            var noticeCanvas = notice.AddComponent<Canvas>();
            noticeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            noticeCanvas.sortingOrder = 2100;
            var scaler = notice.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var background = Box(notice.transform, "Recycling card", 0, 0, 432, 142, new Color(.023f, .11f, .17f, .98f));
            background.raycastTarget = false;
            var rect = background.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-24, -88);
            Label(rect, "COMPONENT RECYCLING", 18, 14, 390, 28, 19, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(rect, "Select a shelf component, then use its recycle icon.", 18, 50, 390, 58, 17)
                .horizontalOverflow = HorizontalWrapMode.Wrap;
            manager.RegisterNative("recycle", "Component Recycling", SparkUILayer.Tool, "workshop", false,
                visible => notice.SetActive(visible), () => notice.activeInHierarchy);
        }

        private void CreateCloseBar()
        {
            closeBar = (RectTransform)Obj("Open Panel Close Controls", transform).transform;
            var closeCanvas = closeBar.gameObject.AddComponent<Canvas>();
            closeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            closeCanvas.sortingOrder = dashboardSortOrder - 10;
            closeBar.gameObject.AddComponent<GraphicRaycaster>();
            var scaler = closeBar.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            closeBar.anchorMin = Vector2.zero;
            closeBar.anchorMax = Vector2.one;
            closeBar.offsetMin = closeBar.offsetMax = Vector2.zero;
            UpdateCloseButtons();
        }

        private void UpdateCloseButtons()
        {
            if (!closeBar || !manager) return;
            var panels = manager.Panels.Where(panel => panel && panel.IsVisible && !panel.AlwaysVisible).ToArray();
            closeBar.gameObject.SetActive(!IsConsoleVisible && panels.Length > 0);
            var signature = string.Join("|", panels.Select(panel => panel.Id));
            if (signature == closeSignature) return;
            closeSignature = signature;
            foreach (Transform child in closeBar)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            float width = Mathf.Min(210f, 1450f / Mathf.Max(1, panels.Length) - 8f);
            for (int i = 0; i < panels.Length; i++)
            {
                string id = panels[i].Id;
                var button = Btn(closeBar, "Close " + panels[i].DisplayName, 0, 0, width, 41,
                    () => manager.Close(id), false, 15);
                button.gameObject.name = "Close " + id;
                var rect = (RectTransform)button.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
                rect.pivot = new Vector2(.5f, 1f);
                rect.anchoredPosition = new Vector2((i - (panels.Length - 1) * .5f) * (width + 8f), -16f);

                var label = button.GetComponentInChildren<Text>();
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 10;
                label.resizeTextMaxSize = 15;
            }
        }

        private void FitWorkshopScreen()
        {
            if (!workshopScreen) return;
            Vector2 size=workshopScreen.rect.size;
            float scale=Mathf.Min(size.x/1672f,size.y/941f);
            root.localScale=new Vector3(scale,scale,1f);
        }

        public void ToggleDashboard() { if (canvas) SetVisible(!canvas.gameObject.activeSelf); }
        private void OnEvent(string entry)
        {
            events.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "  " + entry);
            if (events.Count > 8) events.RemoveAt(events.Count - 1);
        }

        private GameObject Obj(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false); return go;
        }
        private RectTransform Place(GameObject go, float x, float y, float w, float h)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h); return rt;
        }
        private Image Box(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var o = Obj(name, parent); Place(o, x, y, w, h);
            var im = o.AddComponent<Image>(); im.color = color; return im;
        }
        private Text Label(Transform parent, string content, float x, float y, float w, float h, int size = 17, Color? color = null, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var go = Obj("Text " + content, parent); Place(go, x, y, w, h);
            var t = go.AddComponent<Text>(); t.font = uiFont; t.text = content; t.fontSize = size; t.fontStyle = style;
            t.color = color ?? textWhite; t.alignment = align; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }
        private Button Btn(Transform parent, string caption, float x, float y, float w, float h, Action click, bool accent = false, int fontSize = 16)
        {
            var image = Box(parent, "Button " + caption, x, y, w, h, accent ? new Color(.015f, .28f, .39f, .98f) : new Color(.015f, .13f, .21f, .98f));
            var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = accent ? cyan : new Color(.04f, .52f, .67f, .95f); outline.effectDistance = new Vector2(1, -1);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var c = button.colors; c.highlightedColor = new Color(.10f, .35f, .47f); c.pressedColor = new Color(.03f, .55f, .64f); button.colors = c;
            Label(image.transform, caption, 5, 0, w - 10, h, fontSize, textWhite, TextAnchor.MiddleCenter, FontStyle.Bold);
            if (click != null) button.onClick.AddListener(() => click());
            return button;
        }
        private void Header(Transform parent, string title, int x, int y, int w, int h)
        {
            Box(parent, "Panel " + title, x, y, w, h, baseBlue);
            Box(parent, "Panel Line " + title, x, y, w, 2, cyan);
            Label(parent, title, x + 12, y + 7, w - 24, 25, 17, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
        }
        private void Tile(string title, int x, int width, out Text metric)
        {
            Box(root, title, x, 133, width, 55, panelBlue);
            Label(root, title, x + 10, 137, width - 20, 19, 13, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            metric = Label(root, "--", x + 10, 157, width - 20, 27, 20, textWhite, TextAnchor.MiddleLeft, FontStyle.Bold);
        }
        private void Build()
        {
            var holder = Obj("Spark UI MASTER MONITOR [Generated UI]", transform);
            canvas = holder.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.overrideSorting = true;
            canvas.sortingOrder = dashboardSortOrder;
            holder.AddComponent<GraphicRaycaster>();
            var scaler = holder.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1672, 941); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            root = (RectTransform)holder.transform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            var frame = Box(root, "Monitor Frame", 0, 0, 1672, 941, Color.white);
            var sprite = monitorFrame ? monitorFrame : Resources.Load<Sprite>("SparkMonitorFrame");
            if (sprite) { frame.sprite = sprite; frame.type = Image.Type.Simple; frame.preserveAspect = false; }
            else frame.color = new Color(.006f, .025f, .04f, 1f);
            frame.raycastTarget = false;
            Label(root, "SPARK UI MANAGER", 530, 24, 610, 44, 37, textWhite, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(root, "CENTRAL PANEL ORCHESTRATION SYSTEM", 580, 65, 510, 22, 16, cyan, TextAnchor.MiddleCenter);
            Btn(root, "VIEW LEFT WALL", 55, 30, 150, 39, () =>
            {
                if (locateMaster == null) return;
                SetVisible(false);
                locateMaster();
            }, false, 14);
            Btn(root, "CLOSE  ×", 1450, 30, 145, 39, () => { if (IsWallMounted) locateWorkshop?.Invoke(); else SetVisible(false); }, true, 16);
            Label(root, "PROJECTSPARK  //  CONTROL CONSOLE", 243, 104, 470, 25, 14, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(root, "LIVE SYSTEM", 1370, 104, 205, 25, 14, cyan, TextAnchor.MiddleRight);

            Box(root, "Sidebar fill", 56, 99, 149, 690, new Color(.015f, .065f, .10f, .78f));
            Label(root, "⚡", 83, 110, 92, 55, 37, cyan, TextAnchor.MiddleCenter);
            Label(root, "PROJECTSPARK", 62, 156, 138, 29, 15, textWhite, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(root, "UI CONTROL", 65, 182, 135, 20, 11, cyan, TextAnchor.MiddleCenter);
            string[] nav = { "Dashboard", "Layers", "Panels", "Navigation", "Tools", "Modals", "Overlay", "Debug", "Settings" };
            for (int i = 0; i < nav.Length; i++)
            {
                string n = nav[i]; Btn(root, n, 65, 211 + i * 52, 132, 42, () => SetFilter(n), i == 0, 15);
            }
            Tile("REGISTERED PANELS", 234, 185, out countLabel);
            Tile("ACTIVE STACK", 427, 175, out stackLabel);
            Tile("FOCUS OWNER", 610, 195, out focusLabel);
            Tile("TOP MODAL", 813, 193, out modalLabel);
            Tile("GAME INPUT", 1014, 173, out inputLabel);
            Text unused; Tile("LAYOUT PROFILE", 1195, 191, out unused);
            unused.text = "Workshop";
            Tile("MANAGER", 1394, 191, out unused); unused.text = "ONLINE";

            Header(root, "PANEL REGISTRY", 234, 200, 375, 391);
            Label(root, "Select any panel to inspect and control", 246, 234, 340, 26, 14, textWhite);
            Btn(root, "ALL", 247, 266, 76, 30, () => SetFilter("All"), true, 13);
            Btn(root, "MAIN", 329, 266, 78, 30, () => SetFilter("Main"), false, 13);
            Btn(root, "TOOL", 413, 266, 78, 30, () => SetFilter("Tool"), false, 13);
            Btn(root, "MODAL", 497, 266, 96, 30, () => SetFilter("Modal"), false, 13);
            Label(root, "PANEL", 250, 302, 195, 23, 13, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(root, "LAYER", 455, 302, 72, 23, 13, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(root, "STATE", 529, 302, 64, 23, 13, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            var reg = Obj("Registry Scroll View", root); Place(reg, 246, 329, 350, 247);
            var img = reg.AddComponent<Image>(); img.color = new Color(0, 0, 0, .04f);
            var mask = reg.AddComponent<RectMask2D>();
            registryContent = ((RectTransform)Obj("Rows", reg.transform).transform);
            registryContent.anchorMin = new Vector2(0, 1); registryContent.anchorMax = new Vector2(1, 1);
            registryContent.pivot = new Vector2(.5f, 1); registryContent.anchoredPosition = Vector2.zero;
            registryContent.sizeDelta = new Vector2(0, 0);
            var scroll = reg.AddComponent<ScrollRect>(); scroll.content = registryContent; scroll.viewport = (RectTransform)reg.transform;
            scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 25;

            Header(root, "UI LAYER ARCHITECTURE", 620, 200, 655, 391);
            Label(root, "FRONT  /  HIGH PRIORITY", 639, 240, 300, 25, 13, cyan, TextAnchor.MiddleLeft);
            SparkUILayer[] layers = { SparkUILayer.System, SparkUILayer.Fullscreen, SparkUILayer.Modal, SparkUILayer.Popup, SparkUILayer.Overlay, SparkUILayer.Tool, SparkUILayer.Main, SparkUILayer.HUD, SparkUILayer.Background };
            for (int i = 0; i < layers.Length; i++)
            {
                SparkUILayer layer = layers[i]; int yy = 269 + i * 32;
                Box(root, "Layer color", 640, yy + 4, 10, 22, layer == SparkUILayer.Modal ? new Color(.64f, .43f, 1f) : cyan);
                Label(root, "L" + (8 - i) + "  " + layer.ToString().ToUpperInvariant(), 660, yy, 250, 31, 17, textWhite, TextAnchor.MiddleLeft, FontStyle.Bold);
                Text number = Label(root, "0 panels", 1073, yy, 177, 30, 14, cyan, TextAnchor.MiddleRight);
                layerCounts[layer] = number;
                Box(root, "separator", 640, yy + 31, 608, 1, new Color(.05f, .36f, .47f, .7f));
            }
            Label(root, "Order: higher layers render over lower layers; within a layer, focus wins.", 639, 566, 615, 20, 12, cyan);

            Header(root, "PANEL INSPECTOR", 1286, 200, 300, 391);
            selectedLabel = Label(root, "Select a panel", 1300, 238, 270, 35, 22, textWhite, TextAnchor.MiddleLeft, FontStyle.Bold);
            layerLabel = Label(root, "Layer: --", 1300, 278, 275, 30, 16, cyan);
            stateLabel = Label(root, "State: --", 1300, 308, 275, 30, 16, textWhite);
            Label(root, "OPERATIONS", 1300, 349, 275, 24, 14, cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            Btn(root, "OPEN", 1300, 380, 82, 42, () => WithSelected(p => OpenForUse(p.Id)), false, 14);
            Btn(root, "CLOSE", 1392, 380, 82, 42, () => WithSelected(p => manager.Close(p.Id)), false, 14);
            Btn(root, "TOGGLE", 1484, 380, 88, 42, () => WithSelected(p => { if(p.IsVisible) manager.Close(p.Id); else OpenForUse(p.Id); }), false, 14);
            Btn(root, "FOCUS", 1300, 433, 82, 42, () => WithSelected(p => OpenForUse(p.Id)), false, 14);
            Btn(root, "MINIMIZE", 1392, 433, 88, 42, () => WithSelected(p => manager.Minimize(p.Id)), false, 12);
            Btn(root, "BACK", 1490, 433, 82, 42, () => { if (manager) manager.Back(); }, false, 14);
            Btn(root, "CLOSE ALL MANAGED PANELS", 1300, 493, 272, 46, () => { if (manager) manager.CloseAll(true); }, true, 13);
            Label(root, "Reopen panels from the registry", 1300, 545, 274, 28, 12, cyan);

            Header(root, "SYSTEM DIAGNOSTICS", 234, 606, 440, 180);
            Label(root, "Layer sorting / focus / input routing", 249, 646, 405, 28, 18, textWhite);
            Label(root, "State transitions use unscaled time", 249, 683, 405, 26, 16, cyan);
            Label(root, "Existing panel logic is preserved", 249, 716, 405, 26, 16, cyan);
            Header(root, "RECENT UI EVENTS", 685, 606, 590, 180);
            eventLabel = Label(root, "No activity yet", 701, 642, 558, 132, 14, textWhite, TextAnchor.UpperLeft);
            Header(root, "OPERATIONS", 1286, 606, 300, 180);
            Btn(root, "REFRESH REGISTRY", 1301, 648, 269, 42, () => { registrySignature = ""; Refresh(); }, false, 14);
            Btn(root, "HIDE CONSOLE  [F8]", 1301, 711, 269, 42, ReturnToWorktop, true, 14);
            string[] foot = { "HOME", "LAYERS", "REGISTRY", "TOOLS", "LAYOUTS", "SYSTEM" };
            for (int i = 0; i < foot.Length; i++)
            {
                var f = foot[i]; Btn(root, f, 56 + i * 256, 824, 246, 59, () => SetFilter(f == "REGISTRY" ? "All" : f == "TOOLS" ? "Tool" : f == "LAYERS" ? "All" : "All"), i == 1, 20);
            }
            initialized = true; Refresh();
        }
        private readonly Dictionary<SparkUILayer, Text> layerCounts = new Dictionary<SparkUILayer, Text>();
        private void SetFilter(string next)
        {
            if (next == "Panels" || next == "Dashboard" || next == "Layers" || next == "Debug" || next == "Settings" || next == "Navigation") next = "All";
            if (next == "Tools") next = "Tool";
            if (next == "Modals") next = "Modal";
            filter = next; Refresh();
        }
        public bool OpenForUse(string id)
        {
            if (!manager || !manager.Open(id)) return false;
            if (IsWallMounted && id == "workshop") locateWorkshop?.Invoke();
            if (id != "workshop") leaveMaster?.Invoke();
            SetVisible(false);
            return true;
        }

        private void WithSelected(Action<SparkUIPanel> action)
        {
            if (!manager) return;
            var p = manager.Find(selectedId);
            if (p != null) action(p);
            Refresh();
        }
        private void Refresh()
        {
            if (!initialized || !manager) return;
            UpdateCloseButtons();
            countLabel.text = manager.Panels.Count.ToString();
            stackLabel.text = manager.OpenCount.ToString(); focusLabel.text = manager.FocusedId;
            var modal = manager.Panels.LastOrDefault(p => p && p.IsVisible && (p.Layer == SparkUILayer.Modal || p.Layer == SparkUILayer.Fullscreen));
            modalLabel.text = modal ? modal.DisplayName : "None";
            inputLabel.text = manager.IsGameplayInputBlocked ? "BLOCKED" : "AVAILABLE";
            foreach (var entry in layerCounts)
                entry.Value.text = manager.Panels.Count(p => p && p.Layer == entry.Key && p.IsVisible) + " / " + manager.Panels.Count(p => p && p.Layer == entry.Key) + " active";
            var selected = manager.Find(selectedId);
            selectedLabel.text = selected ? selected.DisplayName : "Select a panel";
            layerLabel.text = selected ? "Layer: " + selected.Layer : "Layer: --";
            stateLabel.text = selected ? "State: " + selected.State : "State: --";
            eventLabel.text = events.Count > 0 ? string.Join("\n", events.Take(6).ToArray()) : "No activity yet";
            string signature = filter + "|" + selectedId + "|" + string.Join(";", manager.Panels.Where(p => p).Select(p => p.Id + ":" + p.State));
            if (signature != registrySignature) { registrySignature = signature; RebuildRows(); }
        }
        private void RebuildRows()
        {
            foreach (var r in registryRows) if (r) Destroy(r);
            registryRows.Clear();
            IEnumerable<SparkUIPanel> all = manager.Panels.Where(p => p);
            if (filter.Equals("Main", StringComparison.OrdinalIgnoreCase)) all = all.Where(p => p.Layer == SparkUILayer.Main);
            if (filter.Equals("Tool", StringComparison.OrdinalIgnoreCase)) all = all.Where(p => p.Layer == SparkUILayer.Tool);
            if (filter.Equals("Modal", StringComparison.OrdinalIgnoreCase)) all = all.Where(p => p.Layer == SparkUILayer.Modal);
            if (filter.Equals("Overlay", StringComparison.OrdinalIgnoreCase)) all = all.Where(p => p.Layer == SparkUILayer.Overlay);
            int i = 0;
            foreach (var p in all)
            {
                var panel = p; int yy = i * 31;
                var background = Box(registryContent, "Row " + p.Id, 0, yy, 347, 29,
                    selectedId == p.Id ? new Color(.02f, .34f, .44f, 1f) : (i % 2 == 0 ? panelBlue : baseBlue));
                registryRows.Add(background.gameObject);
                var b = background.gameObject.AddComponent<Button>(); b.onClick.AddListener(() => { selectedId = panel.Id; Refresh(); });
                Label(background.transform, p.DisplayName, 5, 0, 198, 29, 14, textWhite);
                Label(background.transform, p.Layer.ToString(), 208, 0, 68, 29, 12, cyan);
                Label(background.transform, p.State == SparkUIPanelState.Open ? "OPEN" : p.State.ToString().ToUpperInvariant(), 281, 0, 66, 29, 11,
                    p.IsVisible ? new Color(.4f, 1f, .65f) : textWhite);
                i++;
            }
            registryContent.sizeDelta = new Vector2(0, Mathf.Max(247, i * 31));
        }
    }
}
