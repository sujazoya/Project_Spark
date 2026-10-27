using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Phase 10: read-only adaptive hint ladder driven by authoritative
    /// LevelGamePlayManager data. No guesses about individual faulty wires.
    /// F6 opens the compact panel; request further hints with the Hint button.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIAdaptiveTroubleshooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkShelfSlot resistorWorkstationSlot;
        [SerializeField] private string resistorSlotId = "Register_Slot";

        [Header("Behavior")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F6;
        [SerializeField] private bool resetHintLadderWhenStateChanges = true;
        [SerializeField] private bool logChanges;

        [Header("Presentation")]
        [SerializeField] private Vector2 panelSize = new Vector2(540f, 210f);
        [SerializeField] private Vector2 topLeftOffset = new Vector2(24f, -85f);
        [SerializeField] private int sortingOrder = 580;

        private LevelGamePlayManager subscribed;
        private Canvas canvas;
        private Text title;
        private Text detail;
        private Text progress;
        private Button hintButton;
        private Text hintButtonLabel;
        private string stateSignature;
        private int hintIndex;
        private readonly string[] hints = new string[3];
        private string diagnosis;

        private void Awake()
        {
            ResolveReferences();
            BuildUI();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void Start()
        {
            ResolveReferences();
            Subscribe();
            RefreshFromGame();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (canvas != null) Destroy(canvas.gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsOpen) Hide(); else Show();
            }
            if (levelManager == null)
            {
                ResolveReferences();
                Subscribe();
            }
        }

        public bool IsOpen => canvas != null && canvas.gameObject.activeSelf;

        private void ResolveReferences()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
        }

        private void Subscribe()
        {
            if (subscribed == levelManager) return;
            Unsubscribe();
            if (levelManager == null) return;
            subscribed = levelManager;
            subscribed.LevelStarted += OnLevelChange;
            subscribed.LevelReset += OnLevelChange;
            subscribed.LevelCompleted += OnLevelChange;
            subscribed.LevelEvaluationChanged += OnEvaluationChange;
            subscribed.LevelFailed += OnLevelFailed;
        }

        private void Unsubscribe()
        {
            if (subscribed == null) return;
            subscribed.LevelStarted -= OnLevelChange;
            subscribed.LevelReset -= OnLevelChange;
            subscribed.LevelCompleted -= OnLevelChange;
            subscribed.LevelEvaluationChanged -= OnEvaluationChange;
            subscribed.LevelFailed -= OnLevelFailed;
            subscribed = null;
        }

        private void OnLevelChange(SparkLevelDefinition _) => RefreshFromGame();
        private void OnEvaluationChange(SparkLevelDefinition _) => RefreshFromGame();
        private void OnLevelFailed(SparkLevelDefinition _, string __) => RefreshFromGame();

        [ContextMenu("Adaptive Coach / Show")]
        public void Show()
        {
            if (canvas == null) BuildUI();
            if (canvas == null) return;
            canvas.gameObject.SetActive(true);
            RefreshFromGame();
        }

        [ContextMenu("Adaptive Coach / Hide")]
        public void Hide()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
        }

        [ContextMenu("Adaptive Coach / Next Hint")]
        public void NextHint()
        {
            RefreshFromGame();
            hintIndex = Mathf.Min(hintIndex + 1, 2);
            UpdateTexts();
        }

        public void RefreshFromGame()
        {
            if (levelManager == null)
            {
                diagnosis = "Level manager not found.";
                hints[0] = "Assign LevelGamePlayManager in the Inspector.";
                hints[1] = hints[0];
                hints[2] = hints[0];
                ApplySignature("no-manager");
                UpdateTexts();
                return;
            }

            LevelGamePlayManager m = levelManager;
            SparkShelfSlot slot = ResolveSlot();
            SparkShelfItem occupant = slot != null ? slot.Occupant : null;
            bool placed = occupant != null && occupant.IsAtWorkbench;
            SparkResistor resistor = placed ? occupant.GetComponentInChildren<SparkResistor>(true) : null;
            if (resistor == null && placed) resistor = occupant.GetComponent<SparkResistor>();

            string levelId = m.ActiveLevel != null ? m.ActiveLevel.LevelId : "none";
            string reason;

            if (m.ActiveLevel == null)
            {
                reason = "no-level";
                Assign("No active level", "Start a level to begin circuit diagnosis.",
                    "Check LevelGamePlayManager.", "Choose an unlocked level.", "Wait for its runtime layout to load.");
            }
            else if (m.IsCompleted)
            {
                reason = "completed";
                Assign("Level completed", "The official level evaluator confirmed success.",
                    "All required objectives are satisfied.", "Review your working circuit if you want to learn more.", "Press I on a component to inspect its own electrical readings.");
            }
            else if (m.SourceShorted || m.TargetShorted)
            {
                reason = "short";
                Assign("Short circuit reported", "The level evaluator detected a short-circuit condition.",
                    "Check whether wiring bypasses a device or the power source.",
                    "Inspect the connections around the source and the affected target.",
                    "Use F5 for the full diagnosis. Change only the incorrect wire, then re-evaluate.");
            }
            else if (m.Overloaded)
            {
                reason = "overload";
                Assign("Overload reported", "The level evaluator detected an overload.",
                    "Inspect the power rating of the connected components.",
                    "Compare solved power against each component's rated wattage.",
                    "Use component inspection (I) for individual measurements; correct the circuit safely.");
            }
            else if (m.WrongConnection)
            {
                reason = "invalid-connection";
                Assign("Connection issue", "The level evaluator reported an invalid connection.",
                    "Review the required terminal pairs and polarity.",
                    "Check which terminals actually have registered wire connections.",
                    "Open F5 diagnosis and correct the connection identified by the level objective.");
            }
            else if (!m.HasValidPowerSource)
            {
                reason = "no-source";
                Assign("No valid power source detected", "The active evaluation does not recognize a valid source.",
                    "Inspect the battery or power supply.",
                    "Check its terminals and any required switch connections.",
                    "Complete the source path and inspect the next evaluation; do not assume a glowing object proves source validity.");
            }
            else if (slot != null && !placed)
            {
                reason = "empty-slot";
                Assign("Resistor workstation is empty", "The workstation slot has no registered shelf occupant.",
                    "Choose an allowed resistor on the storage shelf.",
                    "Place it into the highlighted workstation slot.",
                    "If a resistor is visible already, check SparkShelfSlot.Occupant: a decorative prefab is not a registered placement.");
            }
            else if (!m.ClosedReturn)
            {
                reason = "open-return";
                Assign("Open return path", "The level evaluator does not see a closed current path.",
                    "Check connections from the source through the required components.",
                    "Check the switch and the return wire back to the source.",
                    "Use F5 diagnosis to inspect incomplete objectives. Visual proximity is not a registered wire.");
            }
            else if (m.ActiveTargetCount > 0 && m.SatisfiedTargetCount < m.ActiveTargetCount)
            {
                reason = "pending-targets";
                Assign("Some objectives remain incomplete", "A return path exists, but not every level target is satisfied.",
                    "Review the level's target list.",
                    "Check polarity and the actual electrical readings for each target.",
                    "Use F5 to inspect pending objectives. Do not treat a glowing LED alone as level victory.");
            }
            else
            {
                reason = "evaluating";
                Assign("Circuit requires evaluation", "The current manager state does not yet confirm completion.",
                    "Inspect the latest objective results.",
                    "Check the source, wiring, switch, and target measurements.",
                    "Press F5 for the authoritative failure reason before changing components.");
            }

            string occupantId = occupant != null ? occupant.ItemId : "empty";
            string signature = levelId + ":" + m.RuntimeState + ":" + reason + ":" +
                m.SatisfiedTargetCount + "/" + m.ActiveTargetCount + ":" + occupantId;
            ApplySignature(signature);

            if (resistor != null && m.IsPlaying && reason != "empty-slot" && reason != "no-source")
            {
                // These values are read directly from the resistor's solver-fed state.
                // We do not infer individual resistor voltage from level-target voltage.
                diagnosis += "\nResistor " + resistor.ResistanceOhms.ToString("F0") + " Ω: " +
                    resistor.Voltage.ToString("F3") + " V, " +
                    resistor.Current.ToString("F4") + " A, " +
                    resistor.Power.ToString("F3") + " W (reported).";
            }

            UpdateTexts();
        }

        private void ApplySignature(string signature)
        {
            if (signature == stateSignature) return;
            stateSignature = signature;
            if (resetHintLadderWhenStateChanges) hintIndex = 0;
            if (logChanges) Debug.Log("[SparkAdaptiveCoach] State changed: " + signature, this);
        }

        private SparkShelfSlot ResolveSlot()
        {
            if (resistorWorkstationSlot != null && resistorWorkstationSlot.isActiveAndEnabled)
                return resistorWorkstationSlot;
            SparkShelfSlot[] slots = FindObjectsByType<SparkShelfSlot>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SparkShelfSlot candidate in slots)
            {
                if (candidate != null && candidate.Kind == SparkShelfSlotKind.Workstation &&
                    string.Equals(candidate.SlotId, resistorSlotId, StringComparison.Ordinal))
                    return candidate;
            }
            return null;
        }

        private void Assign(string heading, string summary, string h1, string h2, string h3)
        {
            diagnosis = summary;
            displayHeading = heading;
            hints[0] = h1;
            hints[1] = h2;
            hints[2] = h3;
        }

        private string displayHeading = "SPARK AI · TROUBLESHOOTING";

        private void UpdateTexts()
        {
            if (title == null || detail == null || progress == null) return;
            title.text = displayHeading;
            detail.text = diagnosis + "\n\nHINT " + (hintIndex + 1) + "/3: " + hints[hintIndex];
            progress.text = levelManager != null && levelManager.ActiveLevel != null
                ? "OBJECTIVES " + levelManager.SatisfiedTargetCount + "/" + levelManager.ActiveTargetCount + "  ·  F6 CLOSE"
                : "F6 CLOSE";
            if (hintButtonLabel != null) hintButtonLabel.text = hintIndex < 2 ? "NEXT HINT" : "MAX HINT";
            if (hintButton != null) hintButton.interactable = hintIndex < 2;
        }

        private void BuildUI()
        {
            if (canvas != null) return;
            GameObject root = new GameObject("Spark_AI_Adaptive_Coach", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.GetComponent<CanvasGroup>().blocksRaycasts = false;

            RectTransform panel = CreateRect("AdaptivePanel", root.transform, panelSize);
            panel.anchorMin = panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = topLeftOffset;
            Image background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.022f, 0.060f, 0.083f, 0.96f);
            background.raycastTarget = false;

            title = CreateText("Heading", panel, new Vector2(490f, 30f), new Vector2(0f, 79f), 22,
                FontStyle.Bold, new Color(0.26f, 0.90f, 0.95f), TextAnchor.MiddleLeft);
            detail = CreateText("Details", panel, new Vector2(490f, 125f), new Vector2(0f, 2f), 17,
                FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
            progress = CreateText("Progress", panel, new Vector2(325f, 28f), new Vector2(-75f, -83f), 14,
                FontStyle.Normal, new Color(0.6f, 0.8f, 0.83f), TextAnchor.MiddleLeft);

            RectTransform buttonRect = CreateRect("NextHint", panel, new Vector2(125f, 34f));
            buttonRect.anchoredPosition = new Vector2(185f, -82f);
            Image buttonBg = buttonRect.gameObject.AddComponent<Image>();
            buttonBg.color = new Color(0.03f, 0.42f, 0.50f, 1f);
            hintButton = buttonRect.gameObject.AddComponent<Button>();
            hintButton.onClick.AddListener(NextHint);
            hintButtonLabel = CreateText("Caption", buttonRect, new Vector2(116f, 30f), Vector2.zero, 14,
                FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            hintButtonLabel.raycastTarget = false;
            root.SetActive(false);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        private static Text CreateText(string name, Transform parent, Vector2 size, Vector2 position,
            int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent, size);
            rect.anchoredPosition = position;
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
    }
}
