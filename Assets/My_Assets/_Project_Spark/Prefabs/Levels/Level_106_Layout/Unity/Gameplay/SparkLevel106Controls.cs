using System;
using ProjectSpark.Cameras;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel106Process
    {
        private static readonly Color Accent = new(.37f, .92f, .88f);
        private static readonly Color Amber = new(1f, .71f, .33f);
        private static readonly Color Ink = new(.018f, .038f, .056f, .985f);
        private Text progressText, instructionText, statusText, railText, faultText, hintText;
        private Image progressFill;
        private GameObject assemblyControls, copperControls, postControls;
        private Button resumeButton;
        private Camera boardCamera;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private PhysicsRaycaster addedRaycaster;
        private MaterialPropertyBlock lampBlock;
        private SparkTutorialCircuitAwareness generalCoach;
        private bool viewOwned, brainEnabled, rigEnabled, legacyEnabled, oldOrtho, focused, routingView, showCopper = true;
        private float oldSize;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Bounds boardBounds;

        private static Bounds BoundsOf(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            Bounds result = renderers[0].bounds;
            foreach (var renderer in renderers) result.Encapsulate(renderer.bounds);
            return result;
        }

        private void FitTable()
        {
            boardBounds = BoundsOf(boardArt);
            var table = GameObject.Find("PS_PRO_GAME_ROOM")?.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            if (!table || !table.TryGetComponent<BoxCollider>(out var surface)) return;
            float lift = fixture.position.y - boardBounds.min.y;
            var top = table.TransformPoint(surface.center + Vector3.up * surface.size.y * .5f);
            fixture.SetPositionAndRotation(top + Vector3.up * (lift + .022f), Quaternion.LookRotation(table.forward, Vector3.up));
            boardBounds = BoundsOf(boardArt);
            fixture.position += Vector3.ProjectOnPlane(top - boardBounds.center, Vector3.up);
            boardBounds = BoundsOf(boardArt);
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
                addedRaycaster = boardCamera.gameObject.AddComponent<PhysicsRaycaster>();
                addedRaycaster.eventMask = LayerMask.GetMask("UI");
            }
            viewOwned = true;
        }

        private void RestoreView()
        {
            if (generalCoach) generalCoach.enabled = true;
            if (addedRaycaster) Destroy(addedRaycaster);
            if (!viewOwned || !boardCamera) return;
            if (brain) brain.enabled = brainEnabled;
            if (rig) rig.enabled = rigEnabled;
            if (legacy) legacy.enabled = legacyEnabled;
            boardCamera.transform.SetPositionAndRotation(oldPosition, oldRotation);
            boardCamera.orthographic = oldOrtho;
            boardCamera.orthographicSize = oldSize;
            viewOwned = false;
        }

        public void BoardView()
        {
            focused = false;
            routingView = stage == 1;
            RefreshCopper();
            if (!viewOwned) return;
            float size = Mathf.Max(boardBounds.size.z / 1.78f,
                boardBounds.size.x / (1.24f * boardCamera.aspect));
            PlaceCamera(boardBounds.center, size);
        }

        public void PadView()
        {
            focused = routingView = true;
            if (!viewOwned) return;
            Vector3 a = Selected.from.transform.position, b = Selected.to.transform.position;
            PlaceCamera((a + b) * .5f, Mathf.Max(.72f, Vector3.Distance(a, b) * .82f));
        }

        public void FocusPart()
        {
            focused = false;
            if (viewOwned) PlaceCamera(parts[footprint].footprint.position, 1.15f);
        }

        private void PlaceCamera(Vector3 point, float size)
        {
            var target = point - fixture.right * (size * boardCamera.aspect * .235f);
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = size;
            boardCamera.transform.SetPositionAndRotation(target + Vector3.up * 3.20f,
                Quaternion.LookRotation(Vector3.down, fixture.forward));
        }

        private void SelectWireTool()
        {
            var ctrl = FindFirstObjectByType<SparkGameplayController>();
            if (!ctrl || !ctrl.ToolController.TrySetTool(SparkToolType.Wire, out _))
            { Reject("The native Wire tool could not be activated."); return; }
            PadView();
            message = "Drag between the two highlighted contacts.";
        }

        private void BuildControls()
        {
            TakeView();
            generalCoach = FindFirstObjectByType<SparkTutorialCircuitAwareness>();
            if (generalCoach && generalCoach.enabled) generalCoach.enabled = false;
            else generalCoach = null;
            var overlay = new GameObject("SFX-14 service console", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 47;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
            var panel = Panel((RectTransform)overlay.transform, "Service console", 20, 67, 370, 655, Ink);
            Panel(panel, "Accent", 0, 0, 370, 3, Accent);
            Label(panel, "FIRST BOOT", 15, 13, 340, 38, 29, Color.white);
            Label(panel, "LEVEL 106   /   SFX14-EDU-1 TRAINING BOARD", 16, 49, 339, 19, 13, Accent);
            progressText = Label(panel, "", 16, 75, 340, 48, 14, Color.white);
            Panel(panel, "Progress track", 16, 124, 338, 4, new Color(.12f, .22f, .27f));
            progressFill = Panel(panel, "Objective progress", 16, 124, 338, 4, Accent).GetComponent<Image>();
            progressFill.type = Image.Type.Filled;

            statusText = Label(panel, "", 16, 141, 340, 24, 16, Accent);
            instructionText = Label(panel, "", 16, 172, 337, 75, 15, Color.white);
            var diagnosis = Panel(panel, "Live measurements", 12, 256, 346, 124, new Color(.037f, .082f, .104f));
            railText = Label(diagnosis, "", 9, 8, 326, 53, 14, Accent);
            faultText = Label(diagnosis, "", 9, 69, 326, 46, 14, Color.white);

            assemblyControls = Buttons(panel, "Assembly", new (string, Action)[]
            {
                ("Previous target", () => SelectFootprint((footprint + PartCount - 1) % PartCount)),
                ("Next target", () => SelectFootprint((footprint + 1) % PartCount)),
                ("Matching part", () => SelectPart(footprint)),
                ("Previous part", () => SelectPart((selectedPart + PartCount - 1) % PartCount)),
                ("Next part", () => SelectPart((selectedPart + 1) % PartCount)),
                ("Rotate 90°", RotatePart),
                ("Fit part", () => FitPart()), ("Solder", () => Solder()),
                ("Focus part", FocusPart), ("Board view", BoardView), ("Resume", () => Resume())
            });
            resumeButton = assemblyControls.transform.Find("Resume").GetComponent<Button>();
            copperControls = Buttons(panel, "Copper", new (string, Action)[]
            {
                ("Previous route", () => SelectRoute((selectedRoute + RouteCount) % (RouteCount + 1))),
                ("Next route", () => SelectRoute((selectedRoute + 1) % (RouteCount + 1))),
                ("Change layer", () => SetLayer(layer % 10 + 1)),
                ("Plate via", () => PlateVia()), ("Connect pads", () => ConnectPads()), ("Wire tool", SelectWireTool),
                ("Pad view", PadView), ("Board view", BoardView), ("Remove route", () => RemoveSelectedRoute()),
                ("Copper view", () => { showCopper = !showCopper; RefreshCopper(); }),
                ("Layer reference", ToggleLayerReference)
            });
            postControls = Buttons(panel, "POST", new (string, Action)[]
            {
                ("Bench power", () => TogglePower()), ("Run POST", () => StartPost()), ("Fault trial", () => BeginFaultTrial()),
                ("Locate fault", () => LocateFault()), ("Repair part", () => Repair()), ("Board view", BoardView),
                ("Open W141", () => OpenW141()), ("Restore W141", () => RestoreW141()), ("Reset safety", () => ResetSafety()),
                ("W141 pad view", () => { SelectRoute(routes.Length); PadView(); }), ("Wire tool", SelectWireTool)
            });
            hintText = Label(panel, "", 16, 597, 338, 47, 13, new Color(.65f, .78f, .84f));
        }

        private void ToggleLayerReference()
        {
            if (!referenceLayers) return;
            bool show = !referenceLayers.gameObject.activeSelf;
            foreach (Transform child in referenceLayers)
                child.gameObject.SetActive(child.name.StartsWith($"{107 + layer:D3}_L106_Copper_", StringComparison.Ordinal));
            referenceLayers.gameObject.SetActive(show);
        }

        private void RefreshCopper()
        {
            if (!manager) return;
            if (referenceLayers && referenceLayers.gameObject.activeSelf)
                referenceLayers.gameObject.SetActive(false);
            foreach (var route in routes)
                route.copper.enabled = routingView && showCopper && route.layer == layer && HasWire(route);
            jumper.copper.enabled = linked;
            parts[^1].model.SetActive(linked && installed == PartCount);
        }

        private void RefreshPanel()
        {
            if (!progressText) return;
            if (addedRaycaster) addedRaycaster.enabled = stage == 0 || stage == 4;
            assemblyControls.SetActive(stage == 0);
            copperControls.SetActive(stage == 1 || stage is >= 2 and <= 5 && !Construction && !powered);
            postControls.SetActive(stage >= 2 && !copperControls.activeSelf);
            if (assemblyControls.activeSelf) resumeButton.interactable = installed == 0 && PlayerPrefs.HasKey(CheckpointKey);
            progressText.text = $"{stage}/6 OBJECTIVES\n{installed}/106 parts    {routed}/140 routes    {plated}/42 vias";
            progressFill.rectTransform.sizeDelta = new Vector2(338f * stage / 6f, 4f);
            var route = Selected;
            statusText.text = stage == 0 ? "ASSEMBLY" : copperControls.activeSelf ? "COPPER & VIAS" : postCode;
            statusText.color = post == Post.Halt ? Amber : Accent;

            if (stage == 0)
                instructionText.text = $"Footprint {parts[footprint].label}   /   Tray {parts[selectedPart].label} · {rotation}°\n{message}";
            else if (copperControls.activeSelf)
                instructionText.text = $"{route.id} · {route.group.Replace('_', ' ')} · L{route.layer:D2}\n" +
                    (route.barrel && !route.barrel.IsClosed ? $"{route.viaId} spans L{route.layer:D2} to L{route.exitLayer:D2}.\n" : "") + message;
            else instructionText.text = message;
            railText.text = $"STBY {Voltage(standby):F2} V    CORE {Voltage(core):F2} V    DDR {Voltage(ddr):F2} V\n" +
                $"RESET {Voltage(resetSense):F2} V     DDR4 {lanes}/4     PCIe {(Voltage(linkSense) > 2.7f ? "LINK" : "IDLE")}\n" +
                $"SPI {Voltage(flashSense):F2} V    eMMC {Voltage(storageSense):F2} V";
            faultText.text = $"W141 {(linked ? Fresh ? "FRESH" : "LINKED" : "OPEN")}    U15 {(Inhibited ? "INHIBITED" : "ENABLED")}\n" +
                (activeFault >= 0 ? FaultNames[activeFault] : $"{repaired}/3 repairs verified");
            hintText.text = activeFault >= 0 && faultWitnessed && !reworked ? RepairNames[activeFault] :
                copperControls.activeSelf ? $"Viewing L{layer:D2}  •  Gold start / cyan destination" : "";
            locationMarker.enabled = stage == 0 || stage == 4 && activeFault >= 0 && faultWitnessed;
            if (locationMarker.enabled) locationMarker.transform.position = parts[footprint].footprint.position + Vector3.up * .17f;
            bool highlight = routingView && layer == route.layer && (copperControls.activeSelf || route == jumper);
            startMarker.enabled = endMarker.enabled = highlight;
            if (highlight)
            {
                startMarker.transform.position = route.from.transform.position + Vector3.up * .026f;
                endMarker.transform.position = route.to.transform.position + Vector3.up * .026f;
            }
            for (int i = 0; i < faultOverlays.Length; i++)
            {
                bool visible = activeFault == i && faultWitnessed && !reworked;
                if (faultOverlays[i].activeSelf != visible) faultOverlays[i].SetActive(visible);
            }
            Color glow = !Inhibited ? Accent : post == Post.Halt ? Amber : new Color(.08f, .26f, .32f);
            lampBlock ??= new MaterialPropertyBlock();
            lampBlock.SetColor("_BaseColor", glow);
            lampBlock.SetColor("_EmissionColor", glow * .35f);
            statusLamp.SetPropertyBlock(lampBlock);
        }

        private static GameObject Buttons(RectTransform panel, string name, (string caption, Action action)[] actions)
        {
            var group = Box(panel, name, 13, 398, 344, 187);
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var rect = Panel(group, action.caption, i % 3 * 116, i / 3 * 46, 110, 38, new Color(.049f, .194f, .235f));
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = rect.GetComponent<Image>();
                button.targetGraphic.raycastTarget = true;
                button.onClick.AddListener(() => action.action());
                Label(rect, action.caption, 3, 0, 104, 38, 13, Color.white).alignment = TextAnchor.MiddleCenter;
            }
            return group.gameObject;
        }

        private static RectTransform Box(RectTransform parent, string name, float x, float y, float w, float h)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        private static RectTransform Panel(RectTransform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = Box(parent, name, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static Text Label(RectTransform parent, string value, float x, float y, float w, float h, int size, Color color)
        {
            var label = Box(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = value;
            label.fontSize = size;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.color = color;
            return label;
        }
    }
}
