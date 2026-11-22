using System;
using System.Linq;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Cameras;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.RecoveryLab
{
    public sealed partial class SparkL130Workshop
    {
        private static readonly Color Mint = new(.37f, .84f, 1f);
        private static readonly Color Amber = new(1f, .71f, .32f);
        private static readonly Color TextWhite = new(.83f, .91f, .93f);
        private static readonly Color Panel = new(.025f, .047f, .066f, .98f);
        private static readonly string[] Pages = { "Intake & isolation", "EPS power isolation", "Virtual ROM recovery",
            "Memory & storage", "Tach feedback repair", "Final cold-start audit", "Recovery audit passed" };
        private Camera benchCamera, previousSelectionCamera;
        private CinemachineBrain brain;
        private SparkGameCameraRig rig;
        private HolographicViewerCamera legacy;
        private SparkTutorialCircuitAwareness coach;
        private bool brainEnabled, rigEnabled, legacyEnabled, coachEnabled, orthographic, viewOwned, angled, zoom;
        private float oldSize, oldNear;
        private Vector3 cameraPosition;
        private Quaternion cameraRotation;
        private Bounds boardBounds;
        private MaterialPropertyBlock block;
        private Text title, readout, instructions, notes, footer;
        private Image progress;
        private GameObject[] sections;
        private GameObject resumeButton, mountRow, clipRow, tray, intake;
        private bool showEvidence;
        private SparkL130Trace trace;
        private LineRenderer cursor;
        private Font font;

        private void InitializeView()
        {
            benchCamera = Camera.main;
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                var table = root.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
                if (table && table.TryGetComponent<BoxCollider>(out var collider)) FitBoard(table, collider);
                if (!coach) coach = root.GetComponentInChildren<SparkTutorialCircuitAwareness>(true);
            }
            if (coach)
            {
                coachEnabled = coach.enabled;
                coach.enabled = false;
            }

            if (benchCamera)
            {
                if (selection)
                {
                    previousSelectionCamera = selection.SelectionCamera;
                    selection.SetSelectionCamera(benchCamera);
                }
                brain = benchCamera.GetComponent<CinemachineBrain>();
                rig = benchCamera.GetComponent<SparkGameCameraRig>();
                legacy = benchCamera.GetComponent<HolographicViewerCamera>();
                if (brain) { brainEnabled = brain.enabled; brain.enabled = false; }
                if (rig) { rigEnabled = rig.enabled; rig.enabled = false; }
                if (legacy) { legacyEnabled = legacy.enabled; legacy.enabled = false; }
                orthographic = benchCamera.orthographic;
                oldNear = benchCamera.nearClipPlane;
                oldSize = benchCamera.orthographicSize;
                cameraPosition = benchCamera.transform.position;
                cameraRotation = benchCamera.transform.rotation;
                viewOwned = true;
            }
            cursor = GetComponentInChildren<LineRenderer>(true);
            block = new MaterialPropertyBlock();
            SetPost(PostSignal.Off);
            BuildPanel();
            Frame();
        }

        private void FitBoard(Transform table, BoxCollider collider)
        {
            var top = table.TransformPoint(collider.center + Vector3.up * (collider.size.y * .5f));
            board.SetPositionAndRotation(top, Quaternion.LookRotation(table.forward, Vector3.up) * Quaternion.Euler(0f, 90f, 0f));
            boardBounds = MeasureBoard();
            board.position += new Vector3(top.x - boardBounds.center.x, top.y - boardBounds.min.y + .025f, top.z - boardBounds.center.z);
        }

        private Bounds MeasureBoard()
        {
            var surfaces = substrate.GetComponentsInChildren<Renderer>();
            Bounds bounds = surfaces[0].bounds;
            foreach (var item in surfaces) bounds.Encapsulate(item.bounds);
            return bounds;
        }

        private void RestoreView()
        {
            if (coach) coach.enabled = coachEnabled;
            if (selection && viewOwned) selection.SetSelectionCamera(previousSelectionCamera);
            if (!viewOwned || !benchCamera) return;
            benchCamera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            benchCamera.orthographic = orthographic;
            benchCamera.orthographicSize = oldSize;
            benchCamera.nearClipPlane = oldNear;
            if (brain) brain.enabled = brainEnabled;
            if (rig) rig.enabled = rigEnabled;
            if (legacy) legacy.enabled = legacyEnabled;
        }

        private Transform Focus()
        {
            if (Stage == 0) return intakeView ? diagnostics[targetIndex].anchor : service[serviceIndex].anchor;
            if (Stage is 1 or 2 or 4) return diagnostics[targetIndex].anchor;
            return Stage is 3 or 5 ? ports[pickedPort].pin.transform : service[0].anchor;
        }

        private void Frame()
        {
            if (!viewOwned) return;
            boardBounds = MeasureBoard();
            Vector3 across = board.forward;
            Vector3 towardTop = -board.right;
            Vector3 target = zoom ? Focus().position : boardBounds.center;
            float height = zoom ? 1.26f : Mathf.Max(boardBounds.size.z / 1.68f, boardBounds.size.x / (1.24f * benchCamera.aspect));
            Vector3 offset = Vector3.up * 7.2f - towardTop * (angled ? 3.25f : 0f);
            Vector3 look = -offset.normalized;
            benchCamera.orthographic = true;
            benchCamera.orthographicSize = height;
            benchCamera.nearClipPlane = angled ? Mathf.Max(oldNear, offset.magnitude - boardBounds.extents.z * .48f - 1.65f) : oldNear;
            target -= across * height * benchCamera.aspect * .23f;
            benchCamera.transform.SetPositionAndRotation(target + offset, Quaternion.LookRotation(look, towardTop));
        }

        private void SetPost(PostSignal state)
        {
            post = state;
            if (block == null) return;
            for (int i = 0; i < debugLamps.Length; i++)
            {
                bool lit = running && ((int)state == i + 1 || state == PostSignal.Firmware && i == 0);
                Color color = lit ? Amber : new Color(.08f, .12f, .16f);
                block.SetColor("_BaseColor", color);
                block.SetColor("_EmissionColor", lit ? color * 1.3f : Color.black);
                debugLamps[i].SetPropertyBlock(block);
            }
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            if (!title) return;
            for (int i = 0; i < sections.Length; i++) sections[i].SetActive(i switch
            {
                0 => Stage == 0,
                1 => Stage is >= 1 and <= 5,
                2 => Stage is >= 3 and <= 5,
                3 => Stage is 1 or 2 or 4,
                _ => Stage is 4 or 5
            });
            resumeButton.SetActive(HasCheckpoint && installed == 0);
            intake.SetActive(intakeView);
            tray.SetActive(!intakeView);
            mountRow.SetActive(serviceIndex == 1);
            clipRow.SetActive(serviceIndex is 2 or 3);
            progress.rectTransform.sizeDelta = new Vector2(412f * Stage / 6f, 4f);
            title.text = $"{Stage}/6  ·  {Pages[Stage].ToUpperInvariant()}";
            instructions.text = message;
            notes.text = detail;
            if (showEvidence && journal.Count > 0)
            {
                Evidence latest = journal[journal.Count - 1];
                notes.text = $"{latest.CaseId}  ·  {latest.Target}  ·  boot {latest.Boot} / revision {latest.Revision} / {latest.Seconds:F1} s\n{latest.Criterion}";
            }
            string power = running ? "MAIN ON" : standby ? "STANDBY" : "ISOLATED";
            string ready = post == PostSignal.Off ? "—" : post.ToString().ToUpperInvariant();
            string thermal = standby ? $"{temperature:F1} °C  ·  Motor {MotorRpm} RPM  ·  Tach {TachRpm} RPM" : "Thermal data unavailable";
            readout.text = Stage == 0
                ? $"{IntakeCount}/10 landmarks   ·   {CountBits(installed, 5)}/5 fitted\n{(intakeView ? diagnostics[targetIndex].name : service[serviceIndex].name)}\n{(intakeView ? "BOARD ISOLATED" : $"{ServiceParts[pickedPart]}  ·  {rotation}°  {(seated ? "· seated" : "")}")}"
                : $"{power}  ·  POST {ready}  ·  {(faultLatched ? "GUARD LATCHED" : $"BOOT {boot}")}\nRails {RailCount}/6  ·  Devices {PortCount}/5  ·  Repairs {Repaired}/3\n{thermal}";
            footer.text = Stage switch
            {
                0 => "Inspect the original landmarks or switch to the service tray.",
                1 or 2 or 4 => $"{diagnostics[targetIndex].name}  ·  {RepairMethods[pickedRepair]}  ·  {RailNames[pickedRail]}",
                < 6 => $"{RailNames[pickedRail]}  ·  {PortNames[pickedPort]}",
                _ => "Current cold-start evidence accepted. Bench released."
            };
            for (int i = 0; i < service.Length; i++)
                service[i].anchor.GetComponent<BoxCollider>().enabled = Stage == 0 && !intakeView &&
                    (i != 1 || (installed & 1) != 0) && (i != 0 || (installed & 1) == 0);
            foreach (var site in diagnostics)
                site.anchor.GetComponent<BoxCollider>().enabled = Stage is 1 or 2 or 4 || Stage == 0 && intakeView;
            if (cursor)
            {
                cursor.enabled = Stage < 6;
                cursor.transform.position = Focus().position + Vector3.up * .28f;
                cursor.transform.rotation = board.rotation;
            }
            if (zoom) Frame();
        }

        private void RefreshTrace(float temp, int rpm, bool loaded)
        {
            if (trace) trace.Append(temp, rpm, loaded);
            RefreshPanel();
        }

        private void BuildPanel()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ui = new GameObject("Recovery Lab HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(transform, false);
            var canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 710;
            var scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var main = Rect(ui.transform, "Recovery panel", 26, 54, 450, 934);
            main.gameObject.AddComponent<Image>().color = Panel;
            var stripe = Rect(main, "Accent", 0, 0, 450, 3).gameObject.AddComponent<Image>();
            stripe.color = Mint;
            stripe.raycastTarget = false;
            Text(main, "THE RECOVERY LAB", 18, 15, 418, 42, 28, Color.white);
            Text(main, "LEVEL 130  /  " + Revision + "  /  ADVANCED BENCH", 19, 61, 415, 25, 14, Mint);
            title = Text(main, "", 19, 100, 415, 28, 20, Mint);
            Rect(main, "Progress track", 19, 139, 412, 4).gameObject.AddComponent<Image>().color = new Color(.1f, .21f, .26f);
            progress = Rect(main, "Objective progress", 19, 139, 412, 4).gameObject.AddComponent<Image>();
            progress.color = Mint;
            instructions = Text(main, "", 19, 156, 412, 70, 18, TextWhite);
            Rect(main, "Instrument well", 15, 237, 420, 99).gameObject.AddComponent<Image>().color = new Color(.047f, .095f, .13f);
            readout = Text(main, "", 27, 245, 396, 80, 17, TextWhite);

            Row(main, 352, ("Board view", () => { zoom = angled = false; Frame(); }),
                ("Angle view", () => { zoom = false; angled = true; Frame(); }),
                ("Focus point", () => { zoom = true; angled = false; Frame(); }),
                ("Evidence", () => { showEvidence = !showEvidence; RefreshPanel(); }));

            var intakePanel = Rect(main, "Intake", 0, 404, 450, 305);
            Row(intakePanel, 0, ("Landmarks", () => UseInventory(true)), ("Service tray", () => UseInventory(false)),
                ("Record intake", () => RecordIntake()));
            intake = Rect(intakePanel, "Landmark controls", 0, 43, 450, 45).gameObject;
            Row(intake.transform, 0, ("Next landmark", () => ChooseDiagnostic((targetIndex + 1) % diagnostics.Length)),
                ("Inspect landmark", () => InspectLandmark()));
            tray = Rect(intakePanel, "Fitting controls", 0, 43, 450, 239).gameObject;
            Row(tray.transform, 0, ("Next footprint", () => ChooseSite((serviceIndex + 1) % service.Length)),
                ("Next tray part", () => ChoosePart((pickedPart + 1) % ServiceParts.Length)), ("Rotate 90°", RotatePart));
            Row(tray.transform, 43, ("Open guard", () => OpenGuard()), ("Seat part", () => Seat()), ("Secure part", () => Secure()));
            Row(tray.transform, 86, ("Undo fit", () => Remove()));
            mountRow = Rect(tray.transform, "Cross-clamp", 0, 129, 450, 40).gameObject;
            Row(mountRow.transform, 0, ("Corner 1", () => Tighten(0)), ("Corner 2", () => Tighten(1)),
                ("Corner 3", () => Tighten(2)), ("Corner 4", () => Tighten(3)));
            clipRow = Rect(tray.transform, "Retention clips", 0, 129, 450, 40).gameObject;
            Row(clipRow.transform, 0, ("Close clips", () => LatchDimm()));
            resumeButton = Row(intakePanel, 218, ("Resume intake", () => Resume()))[0].gameObject;

            var electrical = Rect(main, "Loaded power", 0, 404, 450, 90);
            Row(electrical, 0, ("Apply standby", () => ApplyStandby()), ("Main power", () => PowerOn()), ("Isolate bench", () => Isolate()));
            Row(electrical, 43, ("Next rail", () => ChooseRail((pickedRail + 1) % rails.Length)),
                ("Probe rail", () => ProbeRail()), ("Run POST", () => StartPost()));

            var devices = Rect(main, "Device qualification", 0, 490, 450, 45);
            Row(devices, 0, ("Next device", () => ChoosePort((pickedPort + 1) % ports.Length)),
                ("Verify device", () => ProbeDevice()), ("Sign devices", () => SignDevices()));

            var repairs = Rect(main, "Case controls", 0, 534, 450, 128);
            Row(repairs, 0, ("Open ticket", () => OpenCase()), ("Next suspect", () => ChooseDiagnostic((targetIndex + 1) % diagnostics.Length)),
                ("Analyze suspect", () => AnalyzeSelected()));
            Row(repairs, 43, ("Inspect site", () => InspectRepair()), ("Choose remedy", () => ChooseRepair((pickedRepair + 1) % RepairMethods.Length)),
                ("Apply remedy", () => Repair()));
            Row(repairs, 86, ("Verify repair", () => VerifyRepair()));

            var thermal = Rect(main, "Thermal and audit", 0, 664, 450, 43);
            Row(thermal, 0, ("Thermal trial", () => BeginThermal()), ("Reset guard", () => ResetProtection()),
                ("Sign audit", () => SignAudit()));
            var plot = Rect(main, "Thermal evidence", 21, 712, 406, 112);
            trace = plot.gameObject.AddComponent<SparkL130Trace>();
            trace.raycastTarget = false;
            trace.gameObject.SetActive(false);
            Text(plot, "CORE TEMPERATURE", 8, 2, 200, 24, 13, Amber);
            Text(plot, "FAN TACH", 246, 2, 135, 24, 13, Mint);
            notes = Text(main, "", 19, 827, 414, 55, 15, TextWhite);
            footer = Text(main, "", 19, 884, 414, 39, 14, Mint);
            sections = new[] { intakePanel.gameObject, electrical.gameObject, devices.gameObject, repairs.gameObject, thermal.gameObject };
        }

        private Button[] Row(Transform root, int y, params (string label, Action invoke)[] controls)
        {
            float width = controls.Length == 4 ? 97 : 132;
            var result = new Button[controls.Length];
            for (int index = 0; index < controls.Length; index++)
            {
                var (label, invoke) = controls[index];
                var rect = Rect(root, label, 19 + index * (width + 8), y, width, 37);
                var background = rect.gameObject.AddComponent<Image>();
                background.color = new Color(.065f, .19f, .244f);
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                button.onClick.AddListener(() => invoke());
                var text = Text(rect, label, 4, 0, width - 8, 37, 14, Color.white);
                text.alignment = TextAnchor.MiddleCenter;
                result[index] = button;
            }
            return result;
        }

        private Text Text(Transform parent, string value, float x, float y, float width, float height, int size, Color color)
        {
            var text = Rect(parent, "Caption", x, y, width, height).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}


