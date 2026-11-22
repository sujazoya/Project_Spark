using System;
using ProjectSpark.AI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel94Process
    {
        private static readonly Color ConsoleInk = new(.019f, .049f, .065f, .98f);
        private static readonly Color ConsoleTile = new(.06f, .15f, .19f, 1f);
        private static readonly Color ConsoleCyan = new(.40f, .91f, .96f, 1f);
        private static readonly Color ConsoleMuted = new(.63f, .75f, .79f, 1f);
        private static readonly string[] StageTitles =
        {
            "Netlist review", "Part placement", "Copper construction",
            "Bench commissioning", "Fault repair", "W035 safety recovery", "Board qualified"
        };
        private Text queueText, routeText, statusText;
        private RectTransform meter;
        private GameObject[] pages;
        private Font consoleFont;
        private int shownStage = -1;
        private Button resumeButton;
        private SparkTutorialCircuitAwareness generalCoach;

        private void BuildConsole()
        {
            consoleFont = heading.font;
            heading.GetComponentInParent<Canvas>(true).gameObject.SetActive(false);
            generalCoach = FindFirstObjectByType<SparkTutorialCircuitAwareness>();
            if (generalCoach && generalCoach.enabled)
            {
                generalCoach.SetPanelOpen(false);
                generalCoach.enabled = false;
            }
            else generalCoach = null;
            var overlay = new GameObject("Board Foundry service console", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 48;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var panel = Place(overlay.transform, "Service panel", 24, 65, 410, 705);
            Tile(panel, ConsoleInk);
            Tile(Place(panel, "Accent", 0, 0, 410, 3), ConsoleCyan);
            Label(panel, "Level label", "LEVEL 94   /   TWO-LAYER PCB", 14, ConsoleCyan, 19, 15, 370, 19);
            heading = Label(panel, "Level title", "The Board Foundry", 29, Color.white, 19, 38, 370, 39);
            queueText = Label(panel, "Stage", "", 18, ConsoleCyan, 19, 89, 370, 26);
            var track = Place(panel, "Certification progress", 19, 124, 372, 4);
            Tile(track, ConsoleTile);
            meter = Place(track, "Verified progress", 0, 0, 1, 4);
            Tile(meter, ConsoleCyan);

            var evidence = Place(panel, "Measurement card", 19, 144, 372, 123);
            Tile(evidence, ConsoleTile);
            readings = Label(evidence, "Bench measurements", "", 15, Color.white, 12, 9, 348, 100);
            routeText = Label(panel, "Selected task", "", 16, Color.white, 19, 283, 372, 64);

            pages = new GameObject[StageTitles.Length];
            AddPage(panel, 0, ("Inspect net", InspectNet), ("Next net", NextRoute), ("Previous net", PreviousRoute));
            AddPage(panel, 1, ("Next footprint", NextPart), ("Browse stock", NextStockPart), ("Match stock", MatchPart),
                ("Rotate 90°", RotatePart), ("Fit part", FitPart), ("Solder", SolderPart));
            AddPage(panel, 2, ("Previous route", PreviousRoute), ("Next route", NextRoute), ("Wire tool", SelectWireTool),
                ("Front copper", FrontCopper), ("Back copper", BackCopper), ("Plate via", FitVia), ("Remove route", UndoCopper));
            AddPage(panel, 3, ("Protected power", TogglePower), ("Wire tool", SelectWireTool),
                ("Measure", Measure), ("Qualify", TestBoard));
            AddPage(panel, 4, ("Seed V05 fault", InjectVia), ("Seed R2 fault", InjectPullup), ("Seed Q1 fault", InjectBridge),
                ("Measure", Measure), ("Protected power", TogglePower), ("Repair", Repair), ("Qualify", TestBoard));
            AddPage(panel, 5, ("Arm W035", ArmJumper), ("Remove W035", UndoCopper), ("Wire tool", SelectWireTool),
                ("Measure", Measure), ("Reset latch", ResetSafety), ("Qualify", TestBoard), ("Protected power", TogglePower));
            AddPage(panel, 6, ("Protected power", TogglePower));

            AddButton(panel, "Board view", 19, 520, 117, BoardView);
            AddButton(panel, "Focus point", 146, 520, 117, FocusCurrent);
            AddButton(panel, "Angle view", 273, 520, 118, Showcase);
            resumeButton = AddButton(panel, "Resume checkpoint", 19, 563, 372, ResumeCheckpoint);
            var feedbackCard = Place(panel, "Operator feedback", 19, 612, 372, 73);
            Tile(feedbackCard, ConsoleTile);
            detail = Label(feedbackCard, "Instruction", "", 15, ConsoleCyan, 12, 8, 348, 57);
            statusText = Label(panel, "Final certification", "", 13, ConsoleMuted, 19, 576, 372, 23);
        }

        private void RestoreGeneralCoach()
        {
            if (generalCoach) generalCoach.enabled = true;
        }

        private void AddPage(RectTransform parent, int index, params (string title, Action invoke)[] actions)
        {
            var page = Place(parent, StageTitles[index], 19, 360, 372, 152);
            pages[index] = page.gameObject;
            for (int i = 0; i < actions.Length; i++)
                AddButton(page, actions[i].title, (i % 2) * 190, (i / 2) * 39, 182, actions[i].invoke);
            page.gameObject.SetActive(false);
        }

        private static RectTransform Place(Transform root, string label, float x, float y, float width, float height)
        {
            var rect = (RectTransform)new GameObject(label, typeof(RectTransform)).transform;
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static Image Tile(RectTransform rect, Color color)
        {
            var result = rect.gameObject.AddComponent<Image>();
            result.color = color;
            result.raycastTarget = false;
            return result;
        }

        private Text Label(Transform root, string name, string value, int size, Color color,
            float x, float y, float width, float height)
        {
            var caption = Place(root, name, x, y, width, height).gameObject.AddComponent<Text>();
            caption.font = consoleFont;
            caption.fontSize = size;
            caption.fontStyle = FontStyle.Bold;
            caption.color = color;
            caption.text = value;
            caption.raycastTarget = false;
            return caption;
        }

        private Button AddButton(Transform root, string title, float x, float y, float width, Action invoke)
        {
            var rect = Place(root, title, x, y, width, 34);
            var image = Tile(rect, ConsoleTile);
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = ColorBlock.defaultColorBlock;
            colors.highlightedColor = new Color(.63f, .96f, 1f, 1f);
            colors.pressedColor = new Color(.33f, .66f, .72f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(() => invoke());
            Label(rect, "Caption", title, 14, Color.white, 4, 0, width - 8, 34).alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private void SelectWireTool()
        {
            var game = FindFirstObjectByType<SparkGameplayController>();
            if (!game || !game.ToolController.TrySetTool(SparkToolType.Wire, out _))
            {
                notice = "The workbench Wire tool is unavailable.";
                return;
            }
            followRoute = true;
            if (routes[route].via && !routes[route].via.IsClosed)
                notice = $"Plate {routes[route].viaName} on Front copper before routing.";
            else if (backLayer != NeedsBackLayer)
                notice = NeedsBackLayer ? "Select Back copper for the next segment." : "Select Front copper for this route.";
            else
            {
                notice = "Drag between the two highlighted native pads.";
                FocusCurrent();
            }
        }

        private void FocusCurrent()
        {
            if (!cameraView) return;
            if (stage == 1)
            {
                Vector3 position = PartAnchor(part).position;
                cameraView.UseSocketView(position - arena.right * .55f, position + arena.right * .55f);
            }
            else if (PendingPads(out var a, out var b)) cameraView.UseSocketView(a.transform.position, b.transform.position);
            else PadView();
        }

        private void Present()
        {
            if (pages == null) return;
            if (shownStage != stage)
            {
                if (shownStage >= 0) pages[shownStage].SetActive(false);
                pages[stage].SetActive(true);
                shownStage = stage;
            }
            int passed = manager.SatisfiedTargetCount;
            queueText.text = $"{passed}/{manager.ActiveTargetCount}  ·  {StageTitles[stage]}";
            meter.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 372 * Mathf.Clamp01(passed / 6f));
            string protectedInputText = supply.IsOutputActive ? $"{inputVoltage:F2} V" : "isolated";
            string logicText = validRail ? $"{railVoltage:F2} V" : "unqualified";
            string feedbackText = trusted ? $"{feedbackVoltage:F2} V" : "invalid";
            string sampleText = hasShuntSample && load ? $"{shuntVoltage * 1000:F2} mV" : "unavailable";
            readings.text = $"INPUT   {protectedInputText}    /    LOGIC   {logicText}\n" +
                $"W035 FEEDBACK   {feedbackText}\n" +
                $"LOAD   {(load ? "PULSING" : !trusted || lost ? "INHIBITED" : "STANDBY")}    /    SHUNT   {sampleText}\n" +
                $"PARTS {InstalledCount}/{parts.Length}     TRACES {routed}/{CopperCount}     VIAS {PlatedCount}/{ViaCount}";
            var selected = routes[route];
            routeText.text = stage switch
            {
                1 => $"FOOTPRINT   {references[part]} · {values[part]}\nSTOCK   {references[partPick]} · {values[partPick]}  /  {orientation * 90}°",
                0 or 2 => $"W{route + 1:000}   {selected.net}   {(backLayer ? "B.Cu" : "F.Cu")}\n" +
                    $"{selected.from.name} > {selected.to.name}   {selected.viaName}",
                4 => $"FAULT CASES   {Array.FindAll(repairs, value => value).Length}/3 qualified\n" +
                    (fault >= 0 ? "Diagnosis required" : repairedPending >= 0 ? "Retest required" : "Select the next fault case"),
                _ => "W035 SERVICE JUMPER\nJ3.A > J3.B"
            };
            resumeButton.gameObject.SetActive(CanResume);
            statusText.text = stage == 6 && manager.IsCompleted ? "CERTIFIED  /  ALL OBJECTIVES PASSED" : "";
            detail.text = notice;
        }

        private const int ViaCount = 6;
        private int PlatedCount => Array.FindAll(routes, item => item.via && item.via.IsClosed).Length;
    }
}
