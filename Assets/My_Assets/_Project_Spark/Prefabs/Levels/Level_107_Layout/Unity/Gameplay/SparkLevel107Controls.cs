using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Cameras;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel107Process
    {
        private Text summary, task, readings, activity;
        private SparkLevel107TrustTrace trace;
        private MaterialPropertyBlock lampTint;
        private GameObject assemblyControls, routeControls, diagnosticControls, completion;
        private readonly Dictionary<Renderer, bool> quietCopper = new();
        private float nextPanelRefresh;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private PhysicsRaycaster raycaster;
        private bool ownCamera, brainEnabled, rigEnabled, legacyEnabled, oldOrtho, focused;
        private float oldSize;
        private Vector3 oldPosition;
        private Quaternion oldRotation;

        private void FitTable()
        {
            var table = GameObject.Find("PS_PRO_GAME_ROOM")?.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            if (!table || !table.TryGetComponent<BoxCollider>(out var surface)) return;
            var baseArt = modelSpace.Find("L107_BoardArtRoot");
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
            RestoreCopperRendering();
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
            routing = stage == 1 || RepairRouting;
            if (manager) ReadGraph();
            UpdateCopperRendering();
            if (!ownCamera) return;
            var bounds = BoundsOf(modelSpace.Find("L107_BoardArtRoot"));
            float size = Mathf.Max(bounds.size.z / 1.68f, bounds.size.x / (1.29f * boardCamera.aspect));
            PlaceCamera(bounds.center, size);
        }

        public void PadView()
        {
            focused = routing = true;
            UpdateCopperRendering();
            if (ownCamera) PlaceCamera((Selected.from.transform.position + Selected.to.transform.position) * .5f, 1.55f);
        }

        private void FocusPoint(Vector3 point)
        {
            focused = true;
            routing = false;
            if (manager) ReadGraph();
            UpdateCopperRendering();
            if (ownCamera) PlaceCamera(point, 1.45f);
        }

        private void PlaceCamera(Vector3 target, float size)
        {
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = size;
            Vector3 center = target - fixture.right * (size * .49f);
            Vector3 offset = Vector3.up * 3.5f - fixture.forward * (focused ? 0f : .78f);
            boardCamera.transform.SetPositionAndRotation(center + offset, Quaternion.LookRotation(-offset, fixture.forward));
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
            lampTint = new MaterialPropertyBlock();
            var overlay = new GameObject("Phoenix service console", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 47;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
            var panel = Panel((RectTransform)overlay.transform, "Firmware service console", 20, 72, 432, 679, new Color(.036f, .066f, .092f, .97f));
            Panel(panel, "Copper accent", 0, 0, 432, 3, new Color(.91f, .63f, .30f));
            Label(panel, "PHOENIX PROTOCOL", 16, 14, 390, 36, 25, new Color(.99f, .83f, .60f));
            summary = Label(panel, "", 16, 53, 399, 62, 15, new Color(.40f, .91f, .87f));
            task = Label(panel, "", 16, 122, 398, 83, 16);
            var monitor = Panel(panel, "Firmware telemetry", 12, 211, 408, 172, new Color(.014f, .033f, .051f, .97f));
            readings = Label(monitor, "", 11, 9, 390, 91, 15, new Color(.77f, .90f, .93f));
            Label(monitor, "W149 / SOLVED RECEIVER VOLTAGE", 11, 98, 390, 18, 12, new Color(.47f, .71f, .77f));
            trace = Box(monitor, "W149 scope", 12, 117, 384, 44).gameObject.AddComponent<SparkLevel107TrustTrace>();
            trace.raycastTarget = false;
            activity = Label(panel, "", 16, 391, 394, 35, 13, new Color(.82f, .85f, .86f));

            assemblyControls = Buttons(panel, "Assembly", new (string, Action)[]
            {
                ("Footprint −", () => SelectFootprint((footprint + parts.Length - 1) % parts.Length)),
                ("Footprint +", () => SelectFootprint((footprint + 1) % parts.Length)),
                ("Shelf −", () => SelectPart((selectedPart + parts.Length - 1) % parts.Length)),
                ("Shelf +", () => SelectPart((selectedPart + 1) % parts.Length)),
                ("Match part", () => SelectPart(footprint)), ("Fit part", () => FitPart()),
                ("Solder", () => Solder()), ("Board view", BoardView), ("Resume", RestoreCheckpoint)
            });
            routeControls = Buttons(panel, "Copper work", new (string, Action)[]
            {
                ("Previous route", () => SelectRoute((selectedRoute + Count - 1) % Count)),
                ("Next route", () => SelectRoute((selectedRoute + 1) % Count)),
                ("Cycle layer", () => { layer = layer % 10 + 1; routing = true; ReadGraph(); }),
                ("Plate via", () => PlateVia()), ("Connect pads", () => ConnectPads()),
                ("Wire tool", SelectWireTool), ("Pad view", PadView), ("Board view", BoardView)
            });
            diagnosticControls = Buttons(panel, "Firmware bench", new (string, Action)[]
            {
                ("Bench power", () => TogglePower()), ("Read U6 SPI", () => MeasurePrimary()),
                ("Verify U7", () => AuthenticateRecovery()), ("Write protect", () => ToggleWriteProtect()),
                ("Rewrite U6", () => RewritePrimary()), ("Run POST", () => StartBoot()),
                ("Update trial", () => BeginCutoverTrial()), ("Next fault", () => SeedFault()),
                ("Locate repair", FindRepair), ("Rework contact", () => RepairContact()),
                ("Test request", () => MeasureRequest()), ("Firmware view", () => FocusPoint((parts[primaryIndex].footprint.position + parts[recoveryIndex].footprint.position) * .5f)),
                ("Open W149", () => OpenJumper()), ("Restore W149", () => { SelectRoute(routes.Length); ConnectPads(); }),
                ("Safety reset", () => ResetSafety()), ("Board view", BoardView)
            });
            var review = Box(panel, "Qualification review", 14, 431, 404, 214);
            completion = review.gameObject;
            Label(review, "SERVICE QUALIFICATION", 3, 1, 390, 24, 15, new Color(.95f, .73f, .43f));
            string[] proofs = {"Assembly", "Electrical network", "Firmware recovery", "POST resilience", "Three repairs", "W149 supervisor"};
            for (int i = 0; i < proofs.Length; i++)
            {
                var proof = Panel(review, proofs[i], i % 2 * 202, 29 + i / 2 * 30, 192, 26, new Color(.048f, .138f, .148f));
                Label(proof, "✓  " + proofs[i], 8, 4, 179, 19, 13, new Color(.60f, .93f, .79f));
            }
            var reviewButtons = (RectTransform)Buttons(review, "Inspect qualified board", new (string, Action)[]
            {
                ("Board view", BoardView), ("Firmware view", () => FocusPoint((parts[primaryIndex].footprint.position + parts[recoveryIndex].footprint.position) * .5f)),
                ("W149 pads", () => { SelectRoute(routes.Length); PadView(); })
            }).transform;
            reviewButtons.anchoredPosition = new Vector2(0, -132);
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            if (!summary) return;
            trace.Sample(time, Voltage(receiver, ground));
            if (time < nextPanelRefresh) return;
            nextPanelRefresh = time + .065f;
            if (raycaster) raycaster.enabled = stage == 0 || stage == 4;
            assemblyControls.SetActive(stage == 0);
            routeControls.SetActive(stage == 1 || RepairRouting);
            diagnosticControls.SetActive(stage >= 2 && !RepairRouting && stage < 6);
            completion.SetActive(stage == 6);
            summary.text = $"LEVEL 107    ·    {stage}/6 OBJECTIVES\n{Installed}/112 components     {Routed}/148 routes     {Plated}/46 vias\n{Design}    ·    TRAINING CIRCUIT";
            var route = Selected;
            string current = stage == 0 ? $"Footprint {parts[footprint].label}    Shelf {parts[selectedPart].label}" :
                stage == 1 || RepairRouting ? $"{route.id}   {route.group.Replace('_', ' ')}   L{layer:D2}" +
                    (route.via && !route.via.IsClosed ? $"    {route.viaId} → L{route.exitLayer:D2}" : "") :
                ServiceInstruction();
            task.text = current + "\n" + message;
            readings.text = $"U6  {primaryText}   ·   {(writingUnlocked ? "WRITE ENABLED" : "WRITE PROTECTED")}\n" +
                $"U7  {recoveryText}\n" +
                $"POST  {(postStep == 0 ? "Waiting" : PostSteps[postStep - 1])}    ·    STBY {Voltage(standby, ground):F2} V\n" +
                $"W149  {(Fresh ? "Fresh" : jumperPresent ? "Linked" : "Open")}    ·    LOAD {(Inhibited ? "ISOLATED" : "ENABLED")}";
            activity.text = eventText;
            placementMarker.enabled = stage == 0 || (stage == 4 && causeObserved && fault is 0 or 2);
            if (placementMarker.enabled) placementMarker.transform.position = parts[footprint].footprint.position + Vector3.up * .06f;
            fromMarker.enabled = toMarker.enabled = routing && layer == route.layer;
            fromMarker.transform.position = route.from.transform.position + Vector3.up * .045f;
            toMarker.transform.position = route.to.transform.position + Vector3.up * .045f;
            Lamp(primaryLamp, primaryVerified ? new Color(.28f, .95f, .67f) : new Color(1f, .43f, .20f), powered ? flash.Busy ? .75f + .25f * Mathf.Sin(time * 21f) : 1f : .10f);
            Lamp(recoveryLamp, new Color(.30f, .94f, .83f), recoveryVerified && powered ? 1f : .14f);
            Lamp(trustLamp, Inhibited ? new Color(.98f, .41f, .20f) : new Color(.28f, .95f, .67f), powered ? 1f : .12f);
        }

        private void RestoreCopperRendering()
        {
            foreach (var renderer in quietCopper)
                if (renderer.Key) renderer.Key.forceRenderingOff = renderer.Value;
            quietCopper.Clear();
        }

        private void UpdateCopperRendering()
        {
            RestoreCopperRendering();
            if (routing || stage < 2 || !registry) return;
            registry.RefreshNow();
            foreach (var path in registry.Wires)
            {
                if (!path.Path || !path.TerminalA || !path.TerminalB ||
                    !path.TerminalA.transform.IsChildOf(transform) || !path.TerminalB.transform.IsChildOf(transform)) continue;
                foreach (var renderer in path.Path.GetComponentsInChildren<Renderer>(true))
                {
                    quietCopper[renderer] = renderer.forceRenderingOff;
                    renderer.forceRenderingOff = true;
                }
            }
        }

        private void Lamp(Renderer target, Color color, float strength)
        {
            if (!target) return;
            lampTint.SetColor("_BaseColor", color * strength);
            lampTint.SetColor("_EmissionColor", color * strength * .85f);
            target.SetPropertyBlock(lampTint);
        }

        private string ServiceInstruction()
        {
            if (stage == 2) return "Recover the corrupted primary image";
            if (stage == 3) return "Prove recovery after an interrupted update";
            if (stage == 4) return $"Fault isolation and repair    {repaired}/3";
            if (stage == 5) return safetyInterrupted ? "Revalidate both banks and reset the isolated supervisor" : "Open W149 under qualified operation";
            return "PFX-15   /   QUALIFIED";
        }

        private static GameObject Buttons(RectTransform panel, string name, (string caption, Action action)[] actions)
        {
            var container = Box(panel, name, 14, 429, 405, 240);
            for (int i = 0; i < actions.Length; i++)
            {
                var entry = actions[i];
                var rect = Panel(container, entry.caption, i % 4 * 101, i / 4 * 42, 96, 36, new Color(.122f, .196f, .242f));
                rect.GetComponent<Image>().raycastTarget = true;
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = rect.GetComponent<Image>();
                button.onClick.AddListener(() => entry.action());
                Label(rect, entry.caption, 3, 1, 90, 34, 13).alignment = TextAnchor.MiddleCenter;
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
