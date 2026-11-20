using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Cinemachine;
using ProjectSpark.Cameras;
using ProjectSpark.HolographicViewer;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel110Process
    {
        private Text progressText, meterText, taskText;
        private RectTransform scope;
        private Image[] scopeSamples;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private PhysicsRaycaster footprintRaycaster;
        private bool brainEnabled, rigEnabled, legacyEnabled, cameraOwned, savedOrtho;
        private float savedSize;
        private Vector3 savedPosition;
        private Quaternion savedRotation;

        private void FitTable()
        {
            var table = GameObject.Find("PS_PRO_GAME_ROOM")?.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            if (!table || !table.TryGetComponent<BoxCollider>(out var surface)) return;
            Vector3 size = Vector3.Scale(surface.size, table.lossyScale);
            float fit = Mathf.Min(Mathf.Abs(size.x) / .310f, Mathf.Abs(size.z) / .230f) * .88f;
            board.localScale = Vector3.one * fit;
            board.SetPositionAndRotation(table.TransformPoint(surface.center + Vector3.up * surface.size.y * .5f)
                + Vector3.up * .035f, Quaternion.LookRotation(table.forward, Vector3.up));
        }

        private float BoardCameraSize => board.TransformVector(Vector3.forward * .230f).magnitude * .65f;

        public void Home()
        {
            SetCopperView(false);
            if (cameraOwned) PositionCamera(board.position, BoardCameraSize);
        }

        private void SetCopperView(bool visible)
        {
            showCopper = visible;
            if (ready) ReadTopology();
            if (visible && cameraOwned) PositionCamera(board.position, BoardCameraSize);
        }

        private void PositionCamera(Vector3 focus, float size)
        {
            camera.orthographic = true;
            camera.orthographicSize = size;
            var boardRight = board.right;
            camera.transform.SetPositionAndRotation(focus - boardRight * (size * .70f) + Vector3.up * 3.5f,
                Quaternion.LookRotation(Vector3.down, board.forward));
        }

        private void TakeCamera()
        {
            if (!camera) return;
            brain = camera.GetComponent<CinemachineBrain>();
            rig = camera.GetComponent<SparkGameCameraRig>();
            legacy = camera.GetComponent<HolographicViewerCamera>();
            savedPosition = camera.transform.position;
            savedRotation = camera.transform.rotation;
            savedOrtho = camera.orthographic;
            savedSize = camera.orthographicSize;
            if (rig) { rigEnabled = rig.enabled; rig.enabled = false; }
            if (brain) { brainEnabled = brain.enabled; brain.enabled = false; }
            if (legacy) { legacyEnabled = legacy.enabled; legacy.enabled = false; }
            cameraOwned = true;
        }

        private void RestoreCamera()
        {
            if (footprintRaycaster) Destroy(footprintRaycaster);
            if (!cameraOwned || !camera) return;
            if (brain) brain.enabled = brainEnabled;
            if (rig) rig.enabled = rigEnabled;
            if (legacy) legacy.enabled = legacyEnabled;
            camera.transform.SetPositionAndRotation(savedPosition, savedRotation);
            camera.orthographic = savedOrtho;
            camera.orthographicSize = savedSize;
        }

        public void FocusRoute()
        {
            SetCopperView(true);
            if (!cameraOwned) return;
            var route = Selected;
            PositionCamera((route.from.transform.position + route.to.transform.position) / 2, 1.6f);
        }

        private void BuildPanel()
        {
            TakeCamera();
            if (camera && !camera.GetComponent<PhysicsRaycaster>())
            {
                footprintRaycaster = camera.gameObject.AddComponent<PhysicsRaycaster>();
                footprintRaycaster.eventMask = LayerMask.GetMask("UI");
            }
            var ui = new GameObject("Voltage Dominion controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(transform, false);
            var canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 48;
            var scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;

            var panel = Node("Power integrity lab", (RectTransform)ui.transform, 18, 72, 436, 757);
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(.024f, .06f, .082f, .98f);
            panelImage.raycastTarget = false;
            Label(panel, "Voltage Dominion", 14, 12, 406, 32, 24);
            progressText = Label(panel, "", 14, 53, 406, 48, 16);
            meterText = Label(panel, "", 14, 105, 406, 86, 15);
            meterText.color = new Color(.38f, .90f, .97f);
            taskText = Label(panel, "", 14, 194, 406, 105, 15);
            scope = Node("VCORE oscilloscope", panel, 14, 302, 406, 103);
            var background = scope.gameObject.AddComponent<Image>();
            background.color = new Color(.01f, .028f, .035f);
            background.raycastTarget = false;
            scopeSamples = new Image[96];
            for (int i = 0; i < scopeSamples.Length; i++)
            {
                var rect = Node("Scope segment", scope, 0, 0, 1, 1);
                var segment = rect.gameObject.AddComponent<Image>();
                segment.color = new Color(.34f, .89f, .68f);
                segment.raycastTarget = false;
                scopeSamples[i] = segment;
            }
            Label(scope, "VCORE     0–1.4 V     10 ms peak samples", 7, 5, 385, 17, 12);

            (string text, UnityAction action)[] controls =
            {
                ("Footprint −", () => SelectFootprint((footprint + parts.Length - 1) % parts.Length)),
                ("Footprint +", () => SelectFootprint((footprint + 1) % parts.Length)),
                ("Shelf −", () => SelectInventory((inventory + parts.Length - 1) % parts.Length)),
                ("Shelf +", () => SelectInventory((inventory + 1) % parts.Length)),
                ("Rotate part", RotatePart), ("Fit part", () => Fit()), ("Flux", ApplyFlux), ("Solder", () => Solder()),
                ("Route −", () => SelectRoute((routeIndex + routes.Length) % (routes.Length + 1))),
                ("Route +", () => SelectRoute((routeIndex + 1) % (routes.Length + 1))),
                ("Cycle layer", () => { layer = layer % 12 + 1; SetCopperView(true); }),
                ("Plate via", () => PlateVia()),
                ("Connect pads", () => ConnectRoute()), ("Wire tool", SelectWireTool), ("Pad view", FocusRoute), ("Board view", Home),
                ("12 V input", () => Power()), ("Calibrate", () => Calibrate()), ("Capture step", () => Capture()), ("Protection", () => TestProtection()),
                ("Inject fault", () => InjectNextFault()), ("Diagnose", () => Diagnose()), ("Repair", () => Repair()),
                ("Find target", () => { if (fault >= 0 && faultEvidence == 3) SelectFootprint(faultParts[fault]); }),
                ("Select W173", () => SelectRoute(routes.Length)), ("Break W173", () => BreakJumper()),
                ("Safety reset", () => ResetSafety()), ("Save / Resume", ResumeOrSave),
                ("Mount cooling", () => MountCooling())
            };
            for (int i = 0; i < controls.Length; i++)
            {
                var rect = Node(controls[i].text, panel, 14 + i % 4 * 102, 421 + i / 4 * 40, 98, 34);
                var image = rect.gameObject.AddComponent<Image>();
                image.color = new Color(.053f, .23f, .29f);
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.onClick.AddListener(controls[i].action);
                var label = Label(rect, controls[i].text, 0, 0, 98, 34, 13);
                label.alignment = TextAnchor.MiddleCenter;
            }
            RefreshPanel();
        }

        private void SelectWireTool()
        {
            if (gameplay && gameplay.ToolController.TrySetTool(SparkToolType.Wire, out var reason))
            {
                FocusRoute();
                message = "Drag between the highlighted pads or use Connect pads.";
            }
        }

        private void RefreshPanel()
        {
            if (!progressText) return;
            if (footprintRaycaster) footprintRaycaster.enabled = stage == 0 && !showCopper;
            progressText.text = $"LEVEL 110   {stage}/6 OBJECTIVES   PWR18-EDU-1\n{Installed}/130 soldered   {routed}/172 routes   {plated}/56 vias";
            meterText.text = $"VIN {(powered ? nativeInput : 0):F2} V   VCORE {model.Voltage:F3} / 1.050 V   {model.Load:F1} A\n"
                + $"P1 {model.PhaseCurrent(0):F2}   P2 {model.PhaseCurrent(1):F2}   P3 {model.PhaseCurrent(2):F2}   P4 {model.PhaseCurrent(3):F2} A\n"
                + $"Ripple est. {model.Ripple * 1000:F1} mVpp   {model.Temperature:F1} °C   Trip: {model.Trip}\n"
                + $"PGOOD {(model.PowerGood ? "OK" : "—")}   W173 {(HealthPresent && jumperPresent ? "FRESH" : "UNKNOWN")}   CPU {(inhibited ? "ISOLATED" : "ENABLED")}";
            string task = stage == 0 ? $"Footprint {parts[footprint].label} · Shelf {parts[inventory].label} · {orientation}°"
                : $"{Selected.id}  {Selected.group.Replace('_', ' ')} · L{layer:D2}"
                + (Selected.via && !Selected.via.IsConducting ? $" · Plate {Selected.viaId}" : "");
            taskText.text = $"{task}\n{message}\n{lastReading}";
            for (int i = 0; i < scopeSamples.Length; i++)
            {
                var image = scopeSamples[i];
                bool visible = i < traceCount;
                image.enabled = visible;
                if (!visible) continue;
                int oldest = (traceHead + trace.Length - traceCount) % trace.Length;
                float x = 4 + i * 4.1f;
                Vector2 sample = trace[(oldest + i) % trace.Length];
                float low = -98 + Mathf.Clamp01(sample.x / 1.4f) * 72;
                float high = -98 + Mathf.Clamp01(sample.y / 1.4f) * 72;
                var line = image.rectTransform;
                line.anchoredPosition = new Vector2(x, high);
                line.sizeDelta = new Vector2(4.4f, Mathf.Max(1.6f, high - low));
            }
            UpdateHighlight();
        }

        public Renderer selectionMarker, fromMarker, toMarker;
        private void UpdateHighlight()
        {
            selectionMarker.enabled = !showCopper && stage == 0;
            if (selectionMarker.enabled)
                selectionMarker.transform.position = parts[footprint].visual.transform.position + Vector3.up * .12f;
            var route = Selected;
            fromMarker.enabled = toMarker.enabled = showCopper && layer == route.layer;
            fromMarker.transform.position = route.from.transform.position + Vector3.up * .03f;
            toMarker.transform.position = route.to.transform.position + Vector3.up * .03f;
        }

        private static RectTransform Node(string name, RectTransform parent, float x, float y, float width, float height)
        {
            var node = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            node.SetParent(parent, false);
            node.anchorMin = node.anchorMax = node.pivot = Vector2.up;
            node.anchoredPosition = new Vector2(x, -y);
            node.sizeDelta = new Vector2(width, height);
            return node;
        }

        private static Text Label(RectTransform parent, string value, int x, int y, int width, int height, int size)
        {
            var text = Node("Label", parent, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }
    }
}
