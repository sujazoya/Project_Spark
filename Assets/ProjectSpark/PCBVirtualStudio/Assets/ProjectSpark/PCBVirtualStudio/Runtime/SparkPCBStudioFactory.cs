using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    /// <summary>
    /// Generates the entire modular AAA-style workstation using world-space uGUI elements.
    /// Can be called by the editor menu or in a standalone runtime scene.
    /// No FBX, MeshRenderer, TextMeshPro package, or texture atlas is required.
    /// </summary>
    public static class SparkPCBStudioFactory
    {
        private static Font uiFont;
        private static readonly Color Navy = new Color(.018f, .036f, .071f, .98f);
        private static readonly Color PanelFill = new Color(.027f, .071f, .113f, .97f);
        private static readonly Color PanelBorder = new Color(.13f, .47f, .61f, .76f);
        private static readonly Color Cyan = new Color(.24f, .91f, 1f, 1f);
        private static readonly Color Amber = new Color(1f, .70f, .32f, 1f);
        private static readonly Color White = new Color(.91f, .97f, 1f, 1f);
        private static readonly Color Muted = new Color(.54f, .70f, .78f, 1f);
        private static readonly Color Green = new Color(.35f, .97f, .67f, 1f);

        public static GameObject Create(Transform parent = null, Camera viewer = null)
        {
            if (uiFont == null) uiFont = ResolveFont();
            var root = new GameObject("ProjectSpark_AAA_VirtualPCB_Studio");
            if (viewer == null) viewer = Camera.main;
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
                root.transform.localPosition = new Vector3(0, 1.65f, 0);
                if (viewer == null) viewer = CreatePreviewCamera(root.transform);
            }
            else if (viewer != null)
            {
                root.transform.position = viewer.transform.position + viewer.transform.forward * 5f;
                root.transform.rotation = viewer.transform.rotation;
            }
            else
            {
                root.transform.position = new Vector3(0, 1.65f, 0);
                viewer = CreatePreviewCamera(root.transform);
            }

            GameObject ui = new GameObject("WORLD_SPACE_CANVAS", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(root.transform, false);
            Canvas canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = viewer != null ? viewer : Camera.main;
            canvas.sortingOrder = 55;
            RectTransform canvasRect = ui.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(2200, 1240);
            canvasRect.localScale = Vector3.one * .0022f;
            ui.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            SparkVirtualPCBWorkstation studio = root.AddComponent<SparkVirtualPCBWorkstation>();
            EnsureEventSystem();

            // Triple-layer frame and embossed metal/laser accent styling.
            Round(ui.transform, "OuterGlow", 0, 0, 2200, 1240,
                new Color(.015f, .10f, .16f, .50f), new Color(.18f,.78f, .96f,.18f), 24f, 3.5f);
            Round(ui.transform, "Foundation", 8, 8, 2184, 1224,
                Navy, new Color(.20f, .69f, .89f,.72f), 24f, 2f);
            Bar(ui.transform, "TopLaserStripe", 25, 14, 2150, 3, Cyan);
            Bar(ui.transform, "BottomLaserStripe", 25, 1225, 2150, 2, new Color(.20f,.50f,.65f,.6f));
            BuildHeader(ui.transform, studio);

            Text toolName, toolHint;
            BuildToolDeck(ui.transform, studio, out toolName, out toolHint);
            BuildBoard(ui.transform, studio);

            Text title, info, measured, log, status;
            SparkPCBWaveformGraphic graph;
            BuildDiagnosticDeck(ui.transform, out title, out info, out measured, out log, out status, out graph);
            Text footer = BuildFooter(ui.transform);
            Text session = FindChildText(ui.transform, "SESSION_STATUS");

            GameObject fxObject = new GameObject("CONTACT_VFX_OVERLAY", typeof(RectTransform));
            RectTransform fxRoot = fxObject.GetComponent<RectTransform>();
            fxRoot.SetParent(ui.transform, false);
            Place(fxRoot, 0, 0, 2200, 1240);
            // Overlay must never intercept clicks.
            SparkPCBTouchVFX fx = root.AddComponent<SparkPCBTouchVFX>();
            fx.Initialize(fxRoot);
            studio.Configure(fx, graph, toolName, toolHint, title, info, measured, log, footer, status, session);
            studio.SelectToolGroup(0);
            studio.SelectTool(SparkVirtualTool.Inspect);
            return root;
        }

        private static Camera CreatePreviewCamera(Transform root)
        {
            GameObject obj = new GameObject("PCB_STUDIO_DEMO_CAMERA", typeof(Camera));
            Camera cam = obj.GetComponent<Camera>();
            cam.transform.SetParent(root, false);
            cam.transform.localPosition = new Vector3(0, 0, -6.5f);
            cam.transform.localRotation = Quaternion.identity;
            cam.fieldOfView = 55;
            cam.nearClipPlane = .05f;
            cam.farClipPlane = 100;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.006f, .017f, .032f);
            obj.tag = "MainCamera";
            return cam;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;
            var obj = new GameObject("PCB_UI_EventSystem", typeof(EventSystem));
            Type newModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newModuleType != null && typeof(BaseInputModule).IsAssignableFrom(newModuleType))
                obj.AddComponent(newModuleType);
            else
                obj.AddComponent<StandaloneInputModule>();
        }

        private static Font ResolveFont()
        {
            Font f = null;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null) { try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 16);
            return f;
        }

        private static void BuildHeader(Transform root, SparkVirtualPCBWorkstation studio)
        {
            Round(root, "HeaderPanel", 22, 22, 2156, 100, PanelFill, PanelBorder, 17, 1.4f);
            Bar(root, "HeaderAmberLine", 34, 31, 6, 75, Amber);
            Label(root, "PROJECT_SPARK_BRAND", "PROJECT  SPARK", 54, 39, 480, 48, 40, White, true);
            Label(root, "BOARD_STUDIO_SUBTITLE", "AAA / VIRTUAL PCB INSPECTION & REPAIR", 60, 85, 710, 20, 15, Cyan, false);
            Label(root, "HeaderTitle", "ELECTRONICS  ENGINEERING  DECK", 648, 42, 920, 43, 31, White, true);
            Label(root, "SESSION_STATUS", "DEMO TRAINING  /  NOT CIRCUIT-CERTIFIED", 1550, 54, 592, 25, 17, Amber, true, TextAnchor.MiddleRight);
            Label(root, "HeaderMicro", "WORLD SPACE UI  /  CLICK + TOUCH  /  NO 3D TOOL MESHES", 1495, 91, 640, 16, 12, Muted, false, TextAnchor.MiddleRight);
        }

        private static void BuildToolDeck(Transform root, SparkVirtualPCBWorkstation studio,
            out Text toolName, out Text toolHint)
        {
            RectTransform deck = Round(root, "VirtualToolDeck", 22, 140, 328, 955, PanelFill, PanelBorder, 17, 1.3f);
            Label(deck, "DeckLabel", "01 / VIRTUAL TOOL ARMORY", 20, 20, 290, 30, 20, Cyan, true);
            Bar(deck, "DeckDivider", 20, 57, 285, 2, new Color(.14f,.58f,.69f,.65f));
            toolName = Label(deck, "ActiveToolName", "INSPECT", 20, 73, 290, 38, 28, White, true);
            toolHint = Label(deck, "ActiveToolInstruction", "Tap a target to inspect.", 20, 120, 290, 88, 15, Muted);
            Label(deck, "ArmingHint", "SELECT A MODE", 20, 206, 280, 20, 14, Amber, true);

            string[] categories = { "SCAN", "REPAIR", "ADVANCED" };
            for (int i = 0; i < categories.Length; i++)
            {
                RectTransform tab = Round(deck, "Tab_" + categories[i], 20 + i * 98, 234, 94, 46,
                    new Color(.06f,.11f,.19f), new Color(.19f,.39f,.48f), 10, 1);
                var panel = tab.GetComponent<SparkPCBRoundedPanelGraphic>();
                panel.raycastTarget = true;
                var button = tab.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = panel;
                Text t = Label(tab, "Text", categories[i], 2, 9, 90, 27, 14, White, true, TextAnchor.MiddleCenter);
                var bind = tab.gameObject.AddComponent<SparkPCBToolTab>();
                bind.Configure(i, studio, panel, t);
                studio.AddTab(bind);
            }

            SparkVirtualTool[][] groups = {
                new [] { SparkVirtualTool.Inspect, SparkVirtualTool.Multimeter, SparkVirtualTool.Continuity,
                    SparkVirtualTool.Oscilloscope, SparkVirtualTool.LogicAnalyzer, SparkVirtualTool.ThermalCamera,
                    SparkVirtualTool.NetTracer, SparkVirtualTool.FaultScanner, SparkVirtualTool.Microscope },
                new [] { SparkVirtualTool.Flux, SparkVirtualTool.Solder, SparkVirtualTool.Desolder,
                    SparkVirtualTool.Reflow, SparkVirtualTool.HotAir, SparkVirtualTool.TracePatch,
                    SparkVirtualTool.ViaRepair, SparkVirtualTool.ReplaceComponent, SparkVirtualTool.Clean },
                new [] { SparkVirtualTool.BGAReball, SparkVirtualTool.SolderMask, SparkVirtualTool.Calibrate }
            };
            for (int g = 0; g < groups.Length; g++)
                for (int i = 0; i < groups[g].Length; i++)
                {
                    SparkVirtualTool tool = groups[g][i];
                    int col = i % 2, row = i / 2;
                    RectTransform rect = Round(deck, "TOOL_" + tool, 20 + col * 147, 296 + row * 68,
                        138, 58, new Color(.07f,.12f,.21f), new Color(.18f,.41f,.54f), 11, 1.3f);
                    var frame = rect.GetComponent<SparkPCBRoundedPanelGraphic>();
                    frame.raycastTarget = true;
                    Button button = rect.gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                    button.targetGraphic = frame;
                    Text t = Label(rect, "Label", SparkPCBToolCatalog.DisplayName(tool), 6, 9, 126, 40,
                        14, Muted, true, TextAnchor.MiddleCenter);
                    var bind = rect.gameObject.AddComponent<SparkPCBToolButton>();
                    bind.Configure(tool, g, studio, frame, t);
                    studio.AddToolButton(bind);
                }
            Round(deck, "HintCard", 20, 698, 287, 212, new Color(.045f,.11f,.16f),
                new Color(.19f,.50f,.55f), 13, 1);
            Label(deck, "HintTitle", "CONTACT-BASED WORKFLOW", 32, 716, 270, 26, 16, Green, true);
            Label(deck, "HintBody", "1   SELECT a virtual instrument\n2   TAP the component / trace\n3   INSPECT the result\n4   PREPARE + REPAIR if needed\n5   VERIFY with live authority", 32, 756, 260, 135, 16, White);
        }

        private static void BuildBoard(Transform root, SparkVirtualPCBWorkstation studio)
        {
            RectTransform surface = Round(root, "PCB_DigitalTwinPanel", 365, 140, 1257, 955,
                PanelFill, PanelBorder, 17, 1.3f);
            Label(surface, "BoardTitle", "02 / INTERACTIVE MOTHERBOARD DIGITAL TWIN", 24, 20, 945, 35, 21, Cyan, true);
            Label(surface, "BoardTag", "16 VIRTUAL TOUCH TARGETS", 948, 23, 273, 29, 14, Amber, true, TextAnchor.MiddleRight);
            Bar(surface, "BoardTopLine", 24, 63, 1208, 2, new Color(.18f,.60f,.74f,.6f));
            Label(surface, "BoardInstructions", "Click or touch any pad, copper path, via, component or BGA package.",
                29, 76, 1100, 25, 17, Muted);
            RectTransform board = Round(surface, "BOARD_UI_ONLY", 40, 112, 1175, 742,
                new Color(.015f,.12f,.115f, 1f), new Color(.42f,.69f,.51f,.70f), 18, 3.5f);
            // Embedded circuit visual motif; these are decorative, NEVER solver-connected routes.
            DrawDecorativeRoutes(board);
            for (int i = 0; i < 12; i++)
            {
                Bar(board, "GoldTestPad_" + i, 26 + i * 92, 14, 12, 10, new Color(.93f,.66f,.23f,.85f));
                Bar(board, "ConnectorPad_" + i, 26 + i * 92, 718, 12, 10, new Color(.93f,.66f,.23f,.85f));
            }

            TargetDefinition[] definitions = {
                new TargetDefinition("U1", "MAIN MCU", "NET_CORE", SparkPCBTargetKind.BGA, SparkBoardDefect.BGABallFault, 3.3f, 42f, 256f),
                new TargetDefinition("U2", "PRECISION ADC", "NET_ADC", SparkPCBTargetKind.IC, SparkBoardDefect.CalibrationDrift, 2.5f, 35f, 128f),
                new TargetDefinition("U3", "BUCK VRM", "NET_3V3", SparkPCBTargetKind.Regulator, SparkBoardDefect.OverTemperature, 3.3f, 97f, 500f),
                new TargetDefinition("U4", "LOGIC GATE", "NET_CLK", SparkPCBTargetKind.IC, SparkBoardDefect.None, 3.3f, 32f, 1024f),
                new TargetDefinition("R12", "10K RESISTOR", "NET_SENSE", SparkPCBTargetKind.Resistor, SparkBoardDefect.WrongValue, 1.2f, 31f, 0),
                new TargetDefinition("C18", "FILTER CAP", "NET_FILTER", SparkPCBTargetKind.Capacitor, SparkBoardDefect.ColdJoint, 1.8f, 28f, 64f),
                new TargetDefinition("J4", "I/O HEADER", "NET_SDA", SparkPCBTargetKind.Connector, SparkBoardDefect.SolderBridge, 3.3f, 31f, 100f),
                new TargetDefinition("V07", "LAYER VIA", "NET_GND", SparkPCBTargetKind.Via, SparkBoardDefect.OpenVia, 0f, 30f, 0f),
                new TargetDefinition("T07", "COPPER TRACE", "NET_5V", SparkPCBTargetKind.CopperTrace, SparkBoardDefect.OpenTrace, 5f, 32f, 0f),
                new TargetDefinition("Q3", "MOSFET DRIVER", "NET_LOAD", SparkPCBTargetKind.MOSFET, SparkBoardDefect.FailedComponent, 5f, 76f, 256f),
                new TargetDefinition("TP5V", "5V TEST PAD", "NET_5V", SparkPCBTargetKind.TestPad, SparkBoardDefect.None, 5f, 30f, 0f),
                new TargetDefinition("TPG", "GROUND PAD", "NET_GND", SparkPCBTargetKind.TestPad, SparkBoardDefect.None, 0f, 30f, 0f),
                new TargetDefinition("X1", "RTC CLOCK", "NET_RTC", SparkPCBTargetKind.Clock, SparkBoardDefect.ColdJoint, 1.3f, 33f, 32f),
                new TargetDefinition("S4", "SENSOR REF", "NET_REF", SparkPCBTargetKind.Sensor, SparkBoardDefect.CalibrationDrift, 2.5f, 34f, 128f),
                new TargetDefinition("J2", "SOLDER PAD", "NET_IO", SparkPCBTargetKind.TestPad, SparkBoardDefect.OpenPad, 3.3f, 31f, 20f),
                new TargetDefinition("U5", "SAFE IO", "NET_SAFE", SparkPCBTargetKind.IC, SparkBoardDefect.Contamination, 3.3f, 31f, 20f)
            };
            for (int i = 0; i < definitions.Length; i++)
            {
                TargetDefinition d = definitions[i];
                int col = i % 4, row = i / 4;
                RectTransform card = Round(board, "TAP_" + d.id, 64 + col * 286, 102 + row * 151,
                    243, 101, new Color(.05f,.18f,.22f), new Color(.18f,.58f,.57f), 14, 1.5f);
                SparkPCBRoundedPanelGraphic panel = card.GetComponent<SparkPCBRoundedPanelGraphic>();
                panel.raycastTarget = true;
                Button button = card.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = panel;
                Round(card, "ComponentGlyph", 11, 15, 32, 33,
                    new Color(.17f,.32f,.34f), new Color(.78f,.64f,.32f), 5f, 1f);
                Text idLabel = Label(card, "ID", d.id, 52, 11, 175, 23, 17, Amber, true);
                Text nameLabel = Label(card, "Name", d.name, 13, 54, 217, 23, 15, White, true);
                Text st = Label(card, "Status", "CHECK", 115, 14, 113, 22, 12, Green, true, TextAnchor.MiddleRight);
                var target = card.gameObject.AddComponent<SparkVirtualPCBTarget>();
                target.Configure(d.id, d.name, d.net, d.kind, d.fault, d.voltage, d.heat, d.hz,
                    studio, panel, nameLabel, st);
                studio.AddTarget(target);
            }
            Label(surface, "BoardFoot", "VISUAL ROUTES ARE DECORATIVE. ONLY A CONNECTED AUTHORITY MAY MODIFY TRUE PCB NETS.",
                52, 877, 1165, 34, 15, Amber);
        }

        private static void DrawDecorativeRoutes(Transform board)
        {
            System.Random rand = new System.Random(95);
            for (int i = 0; i < 48; i++)
            {
                int y = 55 + i * 13;
                int x = 28 + rand.Next(20, 180);
                int span = rand.Next(210, 720);
                Color trace = i % 4 == 0 ? new Color(.84f,.56f,.21f,.28f) : new Color(.59f,.62f,.28f,.24f);
                Bar(board, "SchematicCopper_H_" + i, x, y, Mathf.Min(span, 1120 - x), 2.2f, trace);
                if (i % 3 == 0)
                    Bar(board, "SchematicCopper_V_" + i, x + span / 2, y, 2.2f, rand.Next(18, 70), trace);
            }
            for (int i = 0; i < 45; i++)
            {
                float x = 36 + rand.Next(1096);
                float y = 22 + rand.Next(690);
                Bar(board, "HoloVia_" + i, x, y, 6.0f, 6.0f, new Color(.93f,.72f,.31f,.45f));
            }
        }

        private static void BuildDiagnosticDeck(Transform root, out Text targetTitle, out Text targetInfo,
            out Text measured, out Text log, out Text status, out SparkPCBWaveformGraphic graph)
        {
            RectTransform deck = Round(root, "VirtualInstrumentDeck", 1638, 140, 540, 955,
                PanelFill, PanelBorder, 17, 1.3f);
            Label(deck, "DiagTitle", "03 / SMART INSTRUMENTS", 22, 20, 490, 32, 21, Cyan, true);
            Bar(deck, "DiagDivider", 22, 61, 495, 2, new Color(.18f,.60f,.74f,.65f));
            targetTitle = Label(deck, "SelectedTarget", "NO TARGET SELECTED", 22, 80, 500, 32, 20, White, true);
            targetInfo = Label(deck, "TargetDetail", "Tap a virtual PCB target to inspect it.",
                22, 125, 485, 90, 16, Muted);
            Round(deck, "ScopeFrame", 20, 231, 500, 290,
                new Color(.014f,.044f,.071f), new Color(.21f,.56f,.68f), 14, 1.5f);
            Label(deck, "ScopeTitle", "SCOPE / LOGIC / THERMAL", 36, 245, 475, 31, 16, Cyan, true);
            Label(deck, "NotLive", "ILLUSTRATIVE SIGNAL - CONNECT PROBE AUTHORITY", 32, 485, 470, 21, 12, Amber, false);
            GameObject trace = new GameObject("Animated_SignalGraph", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(SparkPCBWaveformGraphic));
            trace.transform.SetParent(deck, false);
            RectTransform tr = trace.GetComponent<RectTransform>();
            Place(tr, 37, 282, 467, 197);
            graph = trace.GetComponent<SparkPCBWaveformGraphic>();
            graph.raycastTarget = false;

            Round(deck, "ReadoutFrame", 20, 535, 500, 125,
                new Color(.048f,.13f,.18f), new Color(.22f,.58f,.57f), 13, 1.2f);
            Label(deck, "ReadoutHeader", "ACTIVE TOOL READOUT", 35, 548, 450, 25, 15, Green, true);
            measured = Label(deck, "ReadoutText", "READY  /  Virtual instrument idle", 35, 584, 470, 64, 16, White);

            Round(deck, "EventFrame", 20, 674, 500, 218,
                new Color(.024f,.063f,.105f), new Color(.17f,.43f,.56f), 13, 1.2f);
            Label(deck, "EventTitle", "ACTION TELEMETRY", 34, 688, 480, 23, 15, Cyan, true);
            log = Label(deck, "EventText", "Pick a tool and tap a PCB surface to start.",
                33, 719, 466, 158, 13, Muted);
            status = Label(deck, "StatusFooter", "READY / BOARD INSPECTION", 26, 905, 489, 29,
                16, Green, true, TextAnchor.MiddleCenter);
        }

        private static Text BuildFooter(Transform root)
        {
            RectTransform footer = Round(root, "FooterStatusRail", 22, 1112, 2156, 105,
                new Color(.025f,.07f,.112f), PanelBorder, 15, 1.3f);
            Label(footer, "FooterHeading", "VIRTUAL PCB REPAIR SYSTEM   /   AAA UI", 24, 14, 820, 28, 19, White, true);
            Label(footer, "FooterCaption", "No physical tool models  /  Camera-friendly World Space Canvas  /  Touch and mouse ready", 26, 53, 1240, 26, 15, Muted);
            return Label(footer, "PointerStatus", "TAP TARGET TO BEGIN", 1295, 38, 826, 38, 16, Amber, true, TextAnchor.MiddleRight);
        }

        private struct TargetDefinition
        {
            public readonly string id, name, net;
            public readonly SparkPCBTargetKind kind;
            public readonly SparkBoardDefect fault;
            public readonly float voltage, heat, hz;
            public TargetDefinition(string id, string name, string net, SparkPCBTargetKind kind,
                SparkBoardDefect fault, float v, float t, float hz)
            { this.id=id; this.name=name; this.net=net; this.kind=kind; this.fault=fault;
              voltage=v; heat=t; this.hz=hz; }
        }

        private static RectTransform Round(Transform parent, string name, float x, float y, float w, float h,
            Color fill, Color edge, float radius, float border)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(SparkPCBRoundedPanelGraphic));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            Place(rect, x, y, w, h);
            SparkPCBRoundedPanelGraphic p = go.GetComponent<SparkPCBRoundedPanelGraphic>();
            p.SetStyle(fill, edge, radius, border);
            p.raycastTarget = false;
            return rect;
        }

        private static void Bar(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            RectTransform rt = Round(parent, name, x, y, w, h, color, color, 1f, 0);
            rt.GetComponent<SparkPCBRoundedPanelGraphic>().raycastTarget = false;
        }

        private static Text Label(Transform parent, string name, string text, float x, float y,
            float w, float h, int fontSize, Color color, bool bold = false,
            TextAnchor align = TextAnchor.UpperLeft)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            Place(rt, x, y, w, h);
            Text t = obj.GetComponent<Text>();
            t.font = uiFont;
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            t.supportRichText = false;
            return t;
        }

        private static Text FindChildText(Transform root, string name)
        {
            Text[] labels = root.GetComponentsInChildren<Text>(true);
            foreach (var t in labels) if (t.name == name) return t;
            return null;
        }

        private static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
        }
    }
}
