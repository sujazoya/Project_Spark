using System;
using AAAUI.VFX;
using ProjectSpark.Cameras;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel108Process
    {
        private Text summary, task, readings, activity;
        private GameObject assemblyControls, routeControls, diagnosticControls;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private PhysicsRaycaster raycaster;
        private bool ownCamera, brainEnabled, rigEnabled, legacyEnabled, oldOrtho, focused, notebookOpen;
        private float oldSize;
        private Vector3 oldPosition;
        private Quaternion oldRotation;

        private void FitTable()
        {
            var table = GameObject.Find("PS_PRO_GAME_ROOM")?.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            if (!table || !table.TryGetComponent<BoxCollider>(out var surface)) return;
            var baseArt = modelSpace.Find("L108_BoardArtRoot");
            var bounds = BoundsOf(baseArt);
            float lift = fixture.position.y - bounds.min.y;
            fixture.SetPositionAndRotation(table.TransformPoint(surface.center + Vector3.up * surface.size.y * .5f)
                + Vector3.up * (lift + .02f), Quaternion.LookRotation(table.forward, Vector3.up));
            bounds = BoundsOf(baseArt);
            Vector3 center = table.TransformPoint(surface.center);
            fixture.position += Vector3.ProjectOnPlane(center - bounds.center, Vector3.up);
        }

        private static Bounds BoundsOf(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        private void TakeView()
        {
            if (!boardCamera) return;
            brain = boardCamera.GetComponent<CinemachineBrain>();
            rig = boardCamera.GetComponent<SparkGameCameraRig>();
            legacy = boardCamera.GetComponent<HolographicViewerCamera>();
            oldPosition = boardCamera.transform.position;
            oldRotation = boardCamera.transform.rotation;
            oldOrtho = boardCamera.orthographic;
            oldSize = boardCamera.orthographicSize;
            if (rig) { rigEnabled = rig.enabled; rig.enabled = false; }
            if (brain) { brainEnabled = brain.enabled; brain.enabled = false; }
            if (legacy) { legacyEnabled = legacy.enabled; legacy.enabled = false; }
            if (!boardCamera.GetComponent<PhysicsRaycaster>())
            {
                raycaster = boardCamera.gameObject.AddComponent<PhysicsRaycaster>();
                raycaster.eventMask = LayerMask.GetMask("UI");
            }
            ownCamera = true;
        }

        private void RestoreView()
        {
            if (raycaster) Destroy(raycaster);
            if (!ownCamera || !boardCamera) return;
            if (brain) brain.enabled = brainEnabled;
            if (rig) rig.enabled = rigEnabled;
            if (legacy) legacy.enabled = legacyEnabled;
            boardCamera.transform.SetPositionAndRotation(oldPosition, oldRotation);
            boardCamera.orthographic = oldOrtho;
            boardCamera.orthographicSize = oldSize;
            ownCamera = false;
        }

        public void BoardView()
        {
            focused = false;
            routing = stage == 1;
            if (manager) ReadGraph();
            if (!ownCamera) return;
            var bounds = BoundsOf(modelSpace.Find("L108_BoardArtRoot"));
            float size = Mathf.Max(bounds.size.z / 1.74f, bounds.size.x / (1.32f * boardCamera.aspect));
            PlaceCamera(bounds.center, size);
        }

        public void PadView()
        {
            focused = routing = true;
            if (ownCamera) PlaceCamera((Selected.from.transform.position + Selected.to.transform.position) * .5f, 1.55f);
        }

        private void PlaceCamera(Vector3 target, float size)
        {
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = size;
            boardCamera.transform.SetPositionAndRotation(target - fixture.right * (size * .52f) + Vector3.up * 3.15f,
                Quaternion.LookRotation(Vector3.down, fixture.forward));
        }

        private void SelectWireTool()
        {
            var controller = FindFirstObjectByType<SparkGameplayController>();
            if (controller && controller.ToolController.TrySetTool(SparkToolType.Wire, out _))
            {
                PadView();
                message = "Drag between the highlighted pads.";
            }
        }

        private void BuildControls()
        {
            TakeView();
            var overlay = new GameObject("DFX-16 diagnostic station", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 47;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = .5f;
            var panel = Panel((RectTransform)overlay.transform, "Workbench controls", 18, 78, 407, 628, new Color(.024f, .065f, .085f, .97f));
            Label(panel, "Ghost in the Circuit", 16, 14, 375, 34, 26);
            summary = Label(panel, "", 16, 53, 375, 57, 16, new Color(.35f, .89f, .88f));
            task = Label(panel, "", 16, 117, 375, 75, 16);
            var meterPanel = Panel(panel, "Measurements", 12, 201, 383, 118, new Color(.014f, .036f, .051f));
            readings = Label(meterPanel, "", 10, 8, 366, 104, 15);
            activity = Label(panel, "", 16, 329, 375, 60, 14);
            assemblyControls = Buttons(panel, "Assembly", new (string, Action)[]
            {
                ("Footprint −", () => SelectFootprint((footprint + parts.Length - 1) % parts.Length)),
                ("Footprint +", () => SelectFootprint((footprint + 1) % parts.Length)),
                ("Shelf −", () => SelectPart((selectedPart + parts.Length - 1) % parts.Length)),
                ("Shelf +", () => SelectPart((selectedPart + 1) % parts.Length)),
                ("Matching part", () => SelectPart(footprint)), ("Fit part", () => FitPart()),
                ("Solder", () => Solder()), ("Board view", BoardView)
            });
            routeControls = Buttons(panel, "Routing", new (string, Action)[]
            {
                ("Previous route", () => SelectRoute((selectedRoute + Count - 1) % Count)),
                ("Next route", () => SelectRoute((selectedRoute + 1) % Count)),
                ("Copper layer", () => { layer = layer % 10 + 1; routing = true; ReadGraph(); }),
                ("Plate via", () => PlateVia()), ("Connect pads", () => ConnectPads()),
                ("Wire tool", SelectWireTool), ("Pad view", PadView), ("Board view", BoardView)
            });
            diagnosticControls = Buttons(panel, "Diagnostics", new (string, Action)[]
            {
                ("Bench power", () => TogglePower()), ("Standby", () => Run(Test.Standby)),
                ("Isolate branch", () => Run(Test.Isolate)), ("Cold boot", () => Run(Test.ColdBoot)),
                ("Warm memory", () => Run(Test.WarmMemory)), ("PCIe stress", () => Run(Test.PCIeClock)),
                ("Confirm cause", () => ConfirmDiagnosis()), ("Find repair", LocateRepair),
                ("Rework part", () => Repair()), ("Open W157", () => OpenW157()),
                ("Restore W157", () => { SelectRoute(routes.Length); ConnectPads(); }), ("Safety reset", () => ResetSafety()),
                ("Board view", BoardView), ("W157 pad view", () => { SelectRoute(routes.Length); PadView(); }),
                ("Notebook", () => notebookOpen = !notebookOpen)
            });
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            if (!summary) return;
            if (raycaster) raycaster.enabled = stage == 0 || stage == 3 || stage == 4;
            assemblyControls.SetActive(stage == 0);
            routeControls.SetActive(stage == 1);
            diagnosticControls.SetActive(stage >= 2);
            summary.text = $"LEVEL 108   ·   {stage}/6 OBJECTIVES\n{Installed}/118 parts   {Routed}/156 routes   {Plated}/50 vias\n{Design}   ·   TRAINING CIRCUIT";
            var route = Selected;
            string current = stage == 0 ? $"Footprint {parts[footprint].label}    Shelf {parts[selectedPart].label}" :
                stage == 1 ? $"{route.id}   {route.group.Replace('_', ' ')}   L{layer:D2}" +
                    (route.via && !route.via.IsClosed ? $"   •   {route.viaId} → L{route.exitLayer:D2}" : "") :
                testing ? $"Capturing {runningTest}   {Mathf.Clamp01(testAge / 1.65f):P0}" : NextInstruction();
            task.text = current + "\n" + message;
            readings.text = $"BENCH   {(powered ? "ON" : "ISOLATED")}     W157   {(jumperPresent ? Fresh ? "FRESH" : "LINKED" : "OPEN")}\n"
                + $"BOARD   {heat:F0} °C     SUPERVISOR   {(Inhibited ? "INHIBITED" : "ENABLED")}\n"
                + $"STBY {Voltage(shuntTo, ground):F2} V    DDR {Voltage(memory, ground):F2} V    CLK {Voltage(clock, ground):F2} V\n"
                + $"POST   {BootLabel}     RETESTS   {(qualification & 1) + ((qualification >> 1) & 1) + ((qualification >> 2) & 1)}/3";
            activity.text = notebookOpen && notebook.Count > 0
                ? string.Join("\n", notebook.GetRange(Math.Max(0, notebook.Count - 2), Math.Min(2, notebook.Count))) : measurement;
            placementMarker.enabled = stage == 0 || (stage is 3 or 4 && incident < 3 && supported[incident]);
            if (placementMarker.enabled) placementMarker.transform.position = parts[footprint].footprint.position + Vector3.up * .05f;
            fromMarker.enabled = toMarker.enabled = routing && layer == route.layer;
            fromMarker.transform.position = route.from.transform.position + Vector3.up * .035f;
            toMarker.transform.position = route.to.transform.position + Vector3.up * .035f;
        }

        private string BootLabel => bootStage switch
        {
            1 => "STANDBY", 2 => "MAIN POWER", 3 => "VRM / RESET", 4 => "EC INIT", 5 => "SPI",
            6 => "DDR4", 7 => "PCIe", 8 => "READY", _ => "WAITING"
        };

        private string NextInstruction()
        {
            if (stage == 2) return "01   Capture the powered standby baseline";
            if (stage == 6) return "DFX-16 QUALIFIED";
            if (incident < 3)
            {
                if (supported[incident]) return $"F{incident + 1}   Isolate power and rework the confirmed footprint";
                return incident switch
                {
                    0 => "Compare baseline and isolated standby current",
                    1 => "Compare a cold boot with warm memory stress",
                    _ => "Compare a cold boot with PCIe clock stress"
                };
            }
            if (stage < 5) return "Capture cold boot, warm memory and PCIe qualification";
            if (!interrupted) return "Open the trusted jumper under supervised operation";
            if (!jumperPresent) return "Reconnect W157 at U16 and U17";
            return "Collect all three fresh captures, then reset the supervisor";
        }

        private static GameObject Buttons(RectTransform panel, string name, (string caption, Action action)[] actions)
        {
            var container = Box(panel, name, 13, 393, 381, 212);
            for (int i = 0; i < actions.Length; i++)
            {
                var entry = actions[i];
                var rect = Panel(container, entry.caption, i % 4 * 96, i / 4 * 46, 92, 39, new Color(.055f, .235f, .29f));
                rect.GetComponent<Image>().raycastTarget = true;
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = rect.GetComponent<Image>();
                button.onClick.AddListener(() => entry.action());
                Label(rect, entry.caption, 3, 1, 86, 37, 12).alignment = TextAnchor.MiddleCenter;
            }
            return container.gameObject;
        }

        private static RectTransform Box(RectTransform parent, string name, float x, float y, float width, float height)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static RectTransform Panel(RectTransform parent, string name, float x, float y, float width, float height, Color color)
        {
            var rect = Box(parent, name, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static Text Label(RectTransform parent, string value, float x, float y, float width, float height, int size, Color? color = null)
        {
            var label = Box(parent, "Text", x, y, width, height).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = value;
            label.fontSize = size;
            label.raycastTarget = false;
            label.color = color ?? Color.white;
            return label;
        }
    }
}
