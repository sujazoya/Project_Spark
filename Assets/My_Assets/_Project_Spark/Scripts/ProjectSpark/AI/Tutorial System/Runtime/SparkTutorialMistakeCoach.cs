using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.AI;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Displays guidance only for mistakes reported by gameplay systems.
    /// Does not modify components, terminals, wires, or level state.
    /// Supports guided tutorial and free-gameplay coaching.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialMistakeCoach : MonoBehaviour
    {
        public enum Mistake
        {
            WrongComponent,
            NoSelection,
            OccupiedSlot,
            IncompatibleSlot,
            UnregisteredSlot,
            ComponentMoving,
            WiredComponent,
            InvalidWire,
            CircuitOpen,
            ResistorOverpower,
            GenericPlacementFailure
        }

        [Header("References")]
        [SerializeField] private SparkTutorialDirector director;
        [SerializeField] private SparkAIVoiceController voice;
        [SerializeField] private SparkTutorialGhostDemonstrator ghost;

        [Header("Coach")]
        [SerializeField] private bool speakCorrections = false;
        [SerializeField] private bool replayGhostAfterRepeatedMistakes = true;
        [SerializeField, Range(2, 6)] private int replayAfterCount = 3;
        [SerializeField, Min(0f)] private float minSecondsBetweenMessages = 1f;
        [SerializeField, Min(1f)] private float showSeconds = 6f;
        [SerializeField] private bool logMistakes = true;

        [Header("Tutorial Context")]
        [SerializeField] private bool coachOutsideTutorial = true;
        [SerializeField] private bool filterMistakesByStep = true;
        [SerializeField] private bool alwaysExplainUnavailableComponents = true;
        [SerializeField] private bool clearOnStepChange = true;
        [SerializeField] private bool resetCooldownOnStepChange = true;

        [Header("Presentation")]
        [SerializeField] private int sortingOrder = 550;
        [SerializeField] private Color panelColor = new Color(0.025f, 0.055f, 0.09f, 0.96f);
        [SerializeField] private Color accentColor = new Color(1f, 0.68f, 0.21f, 1f);
        [SerializeField, Min(200f)] private float panelWidth = 650f;
        [SerializeField, Min(100f)] private float panelHeight = 130f;
        [SerializeField] private Vector2 panelBottomRightOffset = new Vector2(-24f, 24f);

        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        private Canvas canvas;
        private CanvasGroup group;
        private Text title;
        private Text message;
        private float hideAt;
        private float lastMessageTime = -999f;
        private string stepId;
        private SparkTutorialDirector subscribedDirector;

        private void Awake()
        {
            ResolveReferences();
            BuildUI();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeDirector();
            SynchronizeCurrentStep();
        }

        private void Start()
        {
            ResolveReferences();
            SubscribeDirector();
            SynchronizeCurrentStep();
        }

        private void Update()
        {
            if (director == null)
            {
                ResolveReferences();
                SubscribeDirector();
            }

            bool tutorialRunning = IsTutorialRunning();
            string currentId = tutorialRunning ? director.CurrentStep.id : "free_gameplay";
            if (stepId != currentId)
                HandleStepTransition(currentId);

            if (canvas == null || !canvas.gameObject.activeSelf)
                return;

            if (!tutorialRunning && !coachOutsideTutorial)
            {
                Hide();
                return;
            }

            if (Time.unscaledTime >= hideAt)
                Hide();
        }

        private void OnDisable()
        {
            UnsubscribeDirector();
            Hide();
        }

        private void OnDestroy()
        {
            UnsubscribeDirector();
            if (canvas != null)
                Destroy(canvas.gameObject);
        }

        private bool IsTutorialRunning()
        {
            return director != null && director.IsRunning && director.CurrentStep != null;
        }

        private void ResolveReferences()
        {
            if (director == null)
                director = GetComponent<SparkTutorialDirector>();
            if (director == null)
                director = FindFirstObjectByType<SparkTutorialDirector>();
            if (voice == null)
                voice = FindFirstObjectByType<SparkAIVoiceController>();
            if (ghost == null)
                ghost = GetComponent<SparkTutorialGhostDemonstrator>();
            if (ghost == null)
                ghost = FindFirstObjectByType<SparkTutorialGhostDemonstrator>();
        }

        private void SubscribeDirector()
        {
            if (subscribedDirector == director)
                return;

            UnsubscribeDirector();
            if (director == null)
                return;

            director.StepChanged += OnStepChanged;
            director.TutorialCompleted += OnCompleted;
            subscribedDirector = director;
        }

        private void UnsubscribeDirector()
        {
            if (subscribedDirector == null)
                return;

            subscribedDirector.StepChanged -= OnStepChanged;
            subscribedDirector.TutorialCompleted -= OnCompleted;
            subscribedDirector = null;
        }

        private void SynchronizeCurrentStep()
        {
            string currentId = IsTutorialRunning() ? director.CurrentStep.id : "free_gameplay";
            if (stepId != currentId)
                HandleStepTransition(currentId);
        }

        private void OnStepChanged(SparkTutorialDefinition definition, int index)
        {
            HandleStepTransition(IsTutorialRunning() ? director.CurrentStep.id : "free_gameplay");
        }

        private void OnCompleted(SparkTutorialDefinition definition)
        {
            HandleStepTransition("free_gameplay");
        }

        private void HandleStepTransition(string newStepId)
        {
            stepId = newStepId;
            counts.Clear();
            if (clearOnStepChange)
                Hide();
            if (resetCooldownOnStepChange)
                lastMessageTime = -999f;

            if (logMistakes)
                Debug.Log("[TutorialCoach] Coaching context: " + stepId, this);
        }

        /// <summary>
        /// Call only after a real gameplay rejection or authoritative error.
        /// Do not call for a terminal selection or cancelled wire gesture.
        /// </summary>
        public void Report(Mistake mistake, string detail = null)
        {
            if (!isActiveAndEnabled)
                return;

            ResolveReferences();
            SubscribeDirector();

            bool tutorialRunning = IsTutorialRunning();
            if (!tutorialRunning && !coachOutsideTutorial)
            {
                Log("IGNORED (tutorial inactive): " + mistake);
                return;
            }

            string currentId = tutorialRunning ? director.CurrentStep.id : "free_gameplay";
            if (stepId != currentId)
                HandleStepTransition(currentId);

            bool priorityComponentWarning =
                alwaysExplainUnavailableComponents && mistake == Mistake.WrongComponent;

            if (tutorialRunning && filterMistakesByStep &&
                !priorityComponentWarning && !IsMistakeRelevant(currentId, mistake))
            {
                Log("FILTERED: " + mistake + " | Step=" + currentId);
                return;
            }

            string key = currentId + ":" + mistake;
            counts.TryGetValue(key, out int count);
            count++;
            counts[key] = count;

            Log("ACCEPTED: " + mistake + " | Step=" + currentId +
                " | Count=" + count +
                (string.IsNullOrEmpty(detail) ? "" : " | Detail=" + detail));

            if (Time.unscaledTime - lastMessageTime < minSecondsBetweenMessages)
            {
                Log("COOLDOWN: " + mistake);
                return;
            }

            lastMessageTime = Time.unscaledTime;
            string correction = CorrectionFor(mistake, count);
            Show(correction, count);

            if (speakCorrections && voice != null)
                voice.SpeakText(correction);

            if (tutorialRunning && replayGhostAfterRepeatedMistakes && ghost != null &&
                count >= replayAfterCount && count % replayAfterCount == 0)
            {
                ghost.Replay();
            }
        }

        public void ReportPlacementRejected(string reason)
        {
            Report(Mistake.GenericPlacementFailure, reason);
        }

        private static bool IsMistakeRelevant(string currentStep, Mistake mistake)
        {
            if (string.IsNullOrWhiteSpace(currentStep))
                return false;

            switch (currentStep.Trim().ToLowerInvariant())
            {
                case "select":
                    return mistake == Mistake.WrongComponent ||
                           mistake == Mistake.ComponentMoving ||
                           mistake == Mistake.WiredComponent;
                case "workstation":
                case "replace":
                    return IsPlacementMistake(mistake);
                case "switch":
                    return mistake == Mistake.CircuitOpen;
                case "wire":
                    return mistake == Mistake.InvalidWire || mistake == Mistake.CircuitOpen;
                case "power":
                case "resistor_power":
                case "overpower":
                    return mistake == Mistake.ResistorOverpower || mistake == Mistake.CircuitOpen;
                case "finish":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsPlacementMistake(Mistake mistake)
        {
            switch (mistake)
            {
                case Mistake.WrongComponent:
                case Mistake.NoSelection:
                case Mistake.OccupiedSlot:
                case Mistake.IncompatibleSlot:
                case Mistake.UnregisteredSlot:
                case Mistake.ComponentMoving:
                case Mistake.WiredComponent:
                case Mistake.GenericPlacementFailure:
                    return true;
                default:
                    return false;
            }
        }

        private static string CorrectionFor(Mistake mistake, int count)
        {
            switch (mistake)
            {
                case Mistake.WrongComponent:
                    return "This component is not available in the current level. Choose one of the enabled components on the shelf.";
                case Mistake.NoSelection:
                    return "Select an available component from the shelf, then choose its workstation slot.";
                case Mistake.OccupiedSlot:
                    return "That workstation slot is occupied. Return the existing component to storage before placing another.";
                case Mistake.IncompatibleSlot:
                    return "This component doesn't belong in that slot. Look for a compatible workstation position.";
                case Mistake.UnregisteredSlot:
                    return "That workstation slot isn't registered for this level. Use the highlighted destination.";
                case Mistake.ComponentMoving:
                    return "That component is still moving. Let it settle before trying again.";
                case Mistake.WiredComponent:
                    return "Disconnect the component's wires safely before moving it.";
                case Mistake.InvalidWire:
                    return count >= 2
                        ? "That wire was rejected again. Check the terminal pair and follow the wiring example."
                        : "The wire connection was rejected. Check the two terminal endpoints.";
                case Mistake.CircuitOpen:
                    return "The electrical path is incomplete. Check the switch and both sides of the circuit.";
                case Mistake.ResistorOverpower:
                    return "The resistor is dissipating excessive power. Check voltage, current, and the wattage rating.";
                default:
                    return count >= 2
                        ? "The placement was rejected again. Check the selected item, slot, and connected wires."
                        : "That placement was not accepted. Check the target and try again.";
            }
        }

        private void Show(string correction, int count)
        {
            if (canvas == null)
                BuildUI();
            if (canvas == null || title == null || message == null)
                return;

            title.text = count >= replayAfterCount
                ? "SPARK AI  /  WATCH THE EXAMPLE"
                : "SPARK AI  /  TRY THIS";
            message.text = correction;
            canvas.gameObject.SetActive(true);
            group.alpha = 1f;
            hideAt = Time.unscaledTime + showSeconds;
        }

        private void Hide()
        {
            hideAt = 0f;
            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }

        private void BuildUI()
        {
            if (canvas != null)
                return;

            GameObject root = new GameObject(
                "SparkTutorialMistakeCoach_UI",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup));

            // Standalone overlay: don't parent it under a tutorial panel that may be hidden.
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.GetComponent<GraphicRaycaster>().enabled = false;
            group = root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            GameObject panel = new GameObject("CoachPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = panelBottomRightOffset;
            rect.sizeDelta = new Vector2(panelWidth, panelHeight);

            Image background = panel.GetComponent<Image>();
            background.color = panelColor;
            background.raycastTarget = false;

            // Children stretch with the panel, so text doesn't spill outside at different widths.
            title = AddStretchText("Title", panel.transform,
                new Vector2(20f, -42f), new Vector2(-20f, -10f),
                22, FontStyle.Bold, accentColor);
            message = AddStretchText("Message", panel.transform,
                new Vector2(20f, 14f), new Vector2(-20f, -46f),
                20, FontStyle.Normal, Color.white);

            root.SetActive(false);
        }

        private static Text AddStretchText(
            string name, Transform parent,
            Vector2 offsetMin, Vector2 offsetMax,
            int fontSize, FontStyle style, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 13;
            text.resizeTextMaxSize = fontSize;
            text.raycastTarget = false;
            return text;
        }

        private void Log(string text)
        {
            if (logMistakes)
                Debug.Log("[TutorialCoach] " + text, this);
        }

        [ContextMenu("Diagnostics / Print Coach State")]
        private void PrintCoachState()
        {
            Debug.Log("[TutorialCoach DEBUG] Tutorial=" + IsTutorialRunning() +
                " | Context=" + (stepId ?? "NULL") +
                " | Outside=" + coachOutsideTutorial +
                " | Panel=" + (canvas != null && canvas.gameObject.activeSelf), this);
        }

        [ContextMenu("Debug / Test Unavailable Component")]
        private void DebugTestWrongComponent()
        {
            Report(Mistake.WrongComponent, "Manual test");
        }

        [ContextMenu("Debug / Test Invalid Wire")]
        private void DebugTestInvalidWire()
        {
            Report(Mistake.InvalidWire, "Manual test");
        }

        [ContextMenu("Debug / Test Occupied Slot")]
        private void DebugTestOccupied()
        {
            Report(Mistake.OccupiedSlot, "Manual test");
        }
    }
}
