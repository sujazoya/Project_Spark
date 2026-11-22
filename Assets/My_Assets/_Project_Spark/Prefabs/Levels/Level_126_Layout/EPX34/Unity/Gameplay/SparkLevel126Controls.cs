using System;
using ProjectSpark.Cameras;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.EclipseCore
{
    public sealed partial class SparkLevel126Process
    {
        private static readonly Color Accent = new(.43f, .86f, .92f);
        private static readonly Color Amber = new(1f, .71f, .33f);
        private static readonly Color Ink = new(.018f, .038f, .056f, .985f);
        private readonly Text[] driveStatus = new Text[2];
        private Text progressText, instructionText, statusText, railText, faultText, hintText;
        private Image progressFill;
        private Text targetReadout;
        private SparkLevel126TrainingPlot scope;
        private GameObject faultControls, safetyControls;
        private GameObject assemblyControls, copperControls, diagnosticControls;
        private Button resumeButton;
        private Camera boardCamera;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private PhysicsRaycaster addedRaycaster;
        private MaterialPropertyBlock lampBlock;
        private SparkTutorialCircuitAwareness generalCoach;
        private bool viewOwned, brainEnabled, rigEnabled, legacyEnabled, oldOrtho, focused, routingView, showCopper = true;
        private float oldSize, oldNearClip;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Bounds boardBounds;
        private bool angled;

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
            oldNearClip = boardCamera.nearClipPlane;
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
            boardCamera.nearClipPlane = oldNearClip;
            viewOwned = false;
        }

        public void BoardView()
        {
            angled = false;
            FrameBoard(stage == 1);
        }

        private void AngleView()
        {
            angled = true;
            FrameBoard(false);
        }

        private void FrameBoard(bool showLayers)
        {
            focused = false;
            routingView = showLayers;
            RefreshCopper();
            RefreshPanel();
            if (!viewOwned) return;
            float size = Mathf.Max(boardBounds.size.z / 1.67f,
                boardBounds.size.x / (1.24f * boardCamera.aspect));
            PlaceCamera(boardBounds.center, size);
        }

        public void PadView()
        {
            focused = routingView = true;
            if (!viewOwned) return;
            angled = false;
            Vector3 a = Selected.from.transform.position, b = Selected.to.transform.position;
            PlaceCamera((a + b) * .5f, Mathf.Max(.72f, Vector3.Distance(a, b) * .82f));
        }

        public void FocusPart()
        {
            focused = false;
            angled = false;
            if (viewOwned) PlaceCamera(parts[footprint].footprint.position, 1.15f);
        }

        private void PlaceCamera(Vector3 point, float size)
        {
            var target = point - fixture.right * (size * boardCamera.aspect * .235f);
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = size;
            Vector3 offset = Vector3.up * 6f - fixture.forward * (angled ? 3.4f : 0f);
            Vector3 forward = -offset.normalized;
            Vector3 axisDepth = new(Mathf.Abs(forward.x), Mathf.Abs(forward.y), Mathf.Abs(forward.z));
            // Clip the near chair in the angled view while keeping the whole PCB in front of the camera.
            boardCamera.nearClipPlane = angled
                ? Mathf.Max(oldNearClip, offset.magnitude - Vector3.Dot(boardBounds.extents, axisDepth) - .47f)
                : oldNearClip;
            boardCamera.transform.SetPositionAndRotation(target + offset,
                Quaternion.LookRotation(forward, fixture.forward));
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
            var overlay = new GameObject("EPX-34 service console", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 47;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
            var panel = Panel((RectTransform)overlay.transform, "Service console", 20, 58, 370, 663, Ink);
            panel.GetComponent<Image>().raycastTarget = true;
            Panel(panel, "Accent", 0, 0, 370, 3, Accent);
            Label(panel, "THE ECLIPSE CORE", 15, 13, 340, 38, 24, Color.white);
            Label(panel, "LEVEL 126  /  EPX34-EDU-1  /  TRAINING CIRCUIT", 16, 49, 339, 19, 13, Accent);
            progressText = Label(panel, "", 16, 75, 340, 48, 14, Color.white);
            Panel(panel, "Progress track", 16, 126, 338, 4, new Color(.12f, .22f, .27f));
            progressFill = Panel(panel, "Objective progress", 16, 126, 338, 4, Accent).GetComponent<Image>();
            progressFill.type = Image.Type.Filled;

            statusText = Label(panel, "", 16, 141, 340, 24, 16, Accent);
            instructionText = Label(panel, "", 16, 180, 337, 80, 15, Color.white);
            var diagnosis = Panel(panel, "Live measurements", 12, 256, 346, 126, new Color(.037f, .082f, .104f));
            railText = Label(diagnosis, "", 9, 8, 360, 40, 13, Accent);
            for (int i = 0; i < driveStatus.Length; i++)
            {
                driveStatus[i] = Label(diagnosis, "", 9 + 139 * i, 43, 130, 17, 11, Accent);
            }
            targetReadout = Label(diagnosis, "", 9, 106, 326, 16, 11, Accent);
            faultText = Label(diagnosis, "", 9, 61, 326, 40, 14, Color.white);

            assemblyControls = Buttons(panel, "Assembly", new (string, Action)[]
            {
                ("Previous target", () => SelectFootprint((footprint + PartCount - 1) % PartCount)),
                ("Next target", () => SelectFootprint((footprint + 1) % PartCount)),
                ("Matching part", () => SelectPart(footprint)),
                ("Previous part", () => SelectPart((selectedPart + PartCount - 1) % PartCount)),
                ("Next part", () => SelectPart((selectedPart + 1) % PartCount)),
                ("Rotate 90°", RotatePart),
                ("Fit part", () => FitPart()), ("Inspect contacts", () => InspectContacts()), ("Secure part", () => Solder()),
                ("Focus part", FocusPart), ("Board view", BoardView), ("Resume", () => Resume())
            });
            resumeButton = assemblyControls.transform.Find("Resume").GetComponent<Button>();
            copperControls = Buttons(panel, "Copper", new (string, Action)[]
            {
                ("Previous route", () => SelectRoute((selectedRoute + RouteCount) % (RouteCount + 1))),
                ("Next route", () => SelectRoute((selectedRoute + 1) % (RouteCount + 1))),
                ("Change layer", () => SetLayer(layer % LayerCount + 1)),
                ("Plate via", () => PlateVia()), ("Connect pads", () => ConnectPads()), ("Wire tool", SelectWireTool),
                ("Pad view", PadView), ("Board view", BoardView), ("Remove route", () => RemoveSelectedRoute()),
                ("Copper view", () => { showCopper = !showCopper; RefreshCopper(); }),
                ("Layer reference", ToggleLayerReference)
            });
            diagnosticControls = Buttons(panel, "Energy bench", new (string, Action)[]
            {
                ("Bench power", () => TogglePower()), ("Measure bus", () => MeasureBench()), ("Commission", () => Calibrate()),
                ("Guard tests", () => CheckGuardRejection()), ("Eclipse trial", () => RunEnergy()), ("Power map", ToggleSensorMap),
                ("Angle view", AngleView), ("Board overview", BoardView), ("Focus target", FocusPart),
                ("Next layer", () => { angled = false; showCopper = true; SetLayer(layer % LayerCount + 1); FrameBoard(true); }),
                ("Layer reference", ToggleLayerReference)
            });
            faultControls = Buttons(panel, "Hardware repair", new (string, Action)[]
            {
                ("Fault trial", () => BeginFaultTrial()), ("Probe fault", () => ProbeFault()), ("Apply repair", () => Repair()),
                ("Select spare", SelectSpare), ("Focus fault site", FocusPart)
            }, 539);
            safetyControls = Buttons(panel, "U3 protected auxiliary", new (string, Action)[]
            {
                ("Protected trial", () => StartProtected()), ("Open W449", () => OpenW449()), ("Restore W449", () => RestoreW449()),
                ("Reset U3", () => ResetSafety()), ("W449 pad view", () => { SelectRoute(routes.Length); PadView(); }),
                ("Test U3 timer", () => TestWatchdog())
            }, 539);
            hintText = Label(panel, "", 16, 611, 338, 52, 13, new Color(.71f, .85f, .90f));
            var scopePanel = Panel((RectTransform)overlay.transform, "Energy traces", 20, 732, 370, 149, Ink);
            Label(scopePanel, "TRIAL TRACE", 12, 7, 114, 20, 13, Accent);
            Label(scopePanel, "SOC", 128, 7, 52, 20, 13, new Color(1f, .65f, .34f));
            Label(scopePanel, "Solar", 187, 7, 58, 20, 13, new Color(.37f, .87f, 1f));
            Label(scopePanel, "Aux.", 247, 7, 55, 20, 13, new Color(.78f, .63f, 1f));
            Label(scopePanel, "Bus", 311, 7, 43, 20, 13, new Color(.53f, .95f, .68f));
            scope = Box(scopePanel, "Stored energy and native solar, auxiliary and bus readings", 12, 31, 346, 95).gameObject.AddComponent<SparkLevel126TrainingPlot>();
            scope.raycastTarget = false;
            Label(scopePanel, "25% RESERVE", 259, 104, 92, 13, 9, new Color(.92f, .70f, .36f));
            Label(scopePanel, "SUN", 15, 130, 42, 16, 9, Accent);
            Label(scopePanel, "90 MIN ECLIPSE", 91, 130, 140, 16, 10, Accent);
            Label(scopePanel, "55 MIN SUNRISE", 252, 130, 110, 16, 10, Accent);
        }

        private void ToggleSensorMap()
        {
            showSensors = !showSensors;
            if (showSensors) BoardView();
        }

        private void ToggleLayerReference()
        {
            if (!referenceLayers) return;
            bool show = !referenceLayers.gameObject.activeSelf;
            foreach (Transform child in referenceLayers)
                child.gameObject.SetActive(child.name.Contains($"L{layer:00}_Reference", StringComparison.Ordinal));
            referenceLayers.gameObject.SetActive(show);
        }

        private void RefreshCopper()
        {
            if (!manager) return;
            if (referenceLayers && referenceLayers.gameObject.activeSelf)
                referenceLayers.gameObject.SetActive(false);
            foreach (var route in routes)
            {
                route.copper.enabled = routingView && showCopper && route.layer == layer && HasWire(route);
                if (route.viaVisual)
                    route.viaVisual.enabled = routingView && showCopper && route.barrel.IsClosed &&
                        (route.layer == layer || route.exitLayer == layer);
            }
            jumper.copper.enabled = routingView && linked;
            if (assemblyArt) assemblyArt.SetActive(!routingView);
        }

        private void RefreshPanel()
        {
            if (!progressText) return;
            if (addedRaycaster) addedRaycaster.enabled = stage != 1;
            assemblyControls.SetActive(stage == 0);
            copperControls.SetActive(stage == 1 || stage is >= 2 and <= 5 && !Construction && !powered);
            diagnosticControls.SetActive(stage >= 2 && !copperControls.activeSelf);
            faultControls.SetActive(stage == 4 && diagnosticControls.activeSelf);
            safetyControls.SetActive(stage >= 5 && diagnosticControls.activeSelf);
            if (assemblyControls.activeSelf) resumeButton.interactable = installed == 0 && PlayerPrefs.HasKey(CheckpointKey);
            progressText.text = $"{stage}/6 OBJECTIVES\n{installed}/{PartCount} parts    {routed}/{RouteCount} routes    {plated}/{ViaCount} vias";
            progressFill.rectTransform.sizeDelta = new Vector2(338f * stage / 6f, 4f);
            var route = Selected;
            statusText.text = stage == 0 ? "ASSEMBLY" : copperControls.activeSelf ? "COPPER & VIAS" : benchState;
            statusText.color = activeFault >= 0 && !reworked || interrupted && !safetyReset ? Amber : Accent;

            if (stage == 0)
                instructionText.text = $"Footprint {parts[footprint].label}   /   Tray {parts[selectedPart].label} · {rotation}°\n{message}";
            else if (copperControls.activeSelf)
                instructionText.text = $"{route.id} · {(route.net.StartsWith("CHECK_", StringComparison.Ordinal) ? route.group.Replace('_', ' ') : route.net.Replace('_', ' '))} · L{route.layer:D2}\n" +
                    (route.barrel && !route.barrel.IsClosed ? $"{route.viaId} spans L{route.layer:D2} to L{route.exitLayer:D2}.\n" : "") + message;
            else instructionText.text = message;
            var run = protectedReplay ?? replay;
            railText.text = $"ESS BUS {Voltage(essentialPin):F2} V   LOAD STEP {loadedBus:F2} V\n" +
                $"SOLAR {SolarWatts:F0} W    ESSENTIAL {EssentialWatts:F0} W";
            driveStatus[0].text = $"AUX {ProtectedAuxWatts + BenchWatts(testAuxPin, testAuxLoad):F0} W";
            driveStatus[0].color = !Inhibited ? Accent : new Color(.58f, .69f, .72f);
            driveStatus[1].text = run == null ? $"SENSE {SenseGain:F2}×" : $"EST. TEMP {run.Last.Temperature:F1} °C";
            driveStatus[1].color = powered && SenseGain > 1.10f ? Amber : Accent;
            faultText.text = $"ENERGY {Accepted}/{Sent}    REPAIRS {repaired}/3\n" +
                $"W449 {(linked ? Fresh ? "FRESH" : "LINKED" : "OPEN")}   AUX {(Inhibited ? "INHIBITED" : "LIVE")}";
            if (run == null) targetReadout.text = "240 Wh STORE  ·  60 Wh RESERVE";
            else
            {
                var sample = run.Last;
                string state = sample.Segment switch
                {
                    SparkLevel126Energy.Phase.Eclipse => $"{sample.HoursToReserve * 60f:F0} min to reserve",
                    SparkLevel126Energy.Phase.Sunrise => run.Complete ? "SUNRISE RECORDED" :
                        sample.Auxiliary > 40f ? "RECHARGE + AUX" : "AUX SHED",
                    _ => "SUN CHECK"
                };
                targetReadout.text = $"{sample.Stored:F1} Wh   {sample.Stored / SparkLevel126Energy.Capacity:P0} SOC   ·   {state}";
            }
            hintText.text = copperControls.activeSelf ? $"{route.from.name} → {route.to.name}\nGold start / cyan destination" : evidence;
            if (diagnosticControls.activeSelf && routingView && !focused)
                hintText.text = $"Copper layer L{layer:00} of {LayerCount}\n" + hintText.text;
            locationMarker.enabled = stage == 0 && installed < PartCount;
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
            for (int i = 0; i < sensorMarkers.Length; i++)
            {
                var marker = sensorMarkers[i];
                marker.enabled = showSensors && powered && !routingView;
                if (!marker.enabled) continue;
                lampBlock ??= new MaterialPropertyBlock();
                bool live = i < 2 ? Live(solarPins[i]) : i == 2 ? Live(essentialPin) : !Inhibited;
                var state = live ? Accent : Color.red;
                lampBlock.SetColor("_BaseColor", state);
                lampBlock.SetColor("_EmissionColor", state * .35f);
                marker.SetPropertyBlock(lampBlock);
            }
            Color glow = !Inhibited ? Accent : activeFault >= 0 && faultWitnessed ? Amber : new Color(.08f, .26f, .32f);
            lampBlock ??= new MaterialPropertyBlock();
            lampBlock.SetColor("_BaseColor", glow);
            lampBlock.SetColor("_EmissionColor", glow * .35f);
            statusLamp.SetPropertyBlock(lampBlock);
        }

        private static GameObject Buttons(RectTransform panel, string name, (string caption, Action action)[] actions, int y = 395)
        {
            var group = Box(panel, name, 13, y, 360, Mathf.CeilToInt(actions.Length / 3f) * 36);
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var rect = Panel(group, action.caption, i % 3 * 116, i / 3 * 36, 110, 32, new Color(.049f, .194f, .235f));
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = rect.GetComponent<Image>();
                button.targetGraphic.raycastTarget = true;
                button.onClick.AddListener(() => action.action());
                Label(rect, action.caption, 3, 0, 104, 32, 13, Color.white).alignment = TextAnchor.MiddleCenter;
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
