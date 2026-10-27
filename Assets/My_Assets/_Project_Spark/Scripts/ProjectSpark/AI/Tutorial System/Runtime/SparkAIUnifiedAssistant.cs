using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.AI;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Phase 13: unified Ask Spark chat and assistant controls. All electrical claims are read from
    /// authoritative gameplay or SparkResistor state. No circuit modifications.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIUnifiedAssistant : MonoBehaviour
    {
        [Header("Existing Systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkTutorialDirector tutorial;
        [SerializeField] private SparkAIAdaptiveTroubleshooter troubleshooter;
        [SerializeField] private SparkAIVoiceController voice;
        [SerializeField] private Camera sceneCamera;

        [Header("Controls")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F7;
        [SerializeField] private bool speakReplies = false;
        [SerializeField] private bool logActions = true;
        [SerializeField, Min(1f)] private float inspectDistance = 25f;
        [SerializeField] private LayerMask inspectLayers = ~0;

        [Header("Chat / Optional External Brain")]
        [SerializeField] private bool routeQuestionsToExternalBrain = false;
        [SerializeField, Min(4f)] private float externalAnswerTimeout = 15f;

        // Subscribe from your own SparkAIBrain adapter (Unity main thread).
        // No brain API names or fabricated measurements are assumed.
        public event Action<string, string> ExternalQuestionRequested;
        public event Action<string, string> AnswerReady;

        [Header("Appearance")]
        [SerializeField] private Vector2 panelSize = new Vector2(535f, 445f);
        [SerializeField] private int sortingOrder = 650;

        private ScrollRect answerScroll;

        private Canvas canvas;
        private InputField questionInput;
        private string pendingQuestion;
        private float pendingUntil;
        private Text heading;
        private Text answer;
        private Text status;
        private int hintStage;
        private string lastStateSignature = string.Empty;
        private LevelGamePlayManager subscribed;

        private void Awake()
        {
            Resolve();
            BuildUI();
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
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
            if (Input.GetKeyDown(toggleKey) &&
                (questionInput == null || !questionInput.isFocused)) Toggle();

            if (pendingQuestion != null && Time.unscaledTime >= pendingUntil)
            {
                string timedOutQuestion = pendingQuestion;
                pendingQuestion = null;
               string fallback = LocalAnswer(timedOutQuestion);

            SetAnswer(
                "SPARK AI · VERIFIED FALLBACK",
                fallback);

            AnswerReady?.Invoke(
                timedOutQuestion,
                fallback);
            }
        }

        private void Resolve()
        {
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (tutorial == null) tutorial = FindFirstObjectByType<SparkTutorialDirector>();
            if (troubleshooter == null) troubleshooter = FindFirstObjectByType<SparkAIAdaptiveTroubleshooter>();
            if (voice == null) voice = FindFirstObjectByType<SparkAIVoiceController>();
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

      
private void Subscribe()
{
    if (subscribed == levelManager)
        return;

    Unsubscribe();

    if (levelManager == null)
        return;

    subscribed = levelManager;

    subscribed.LevelStarted += OnLevelReloaded;
    subscribed.LevelReset += OnLevelReloaded;

    subscribed.LevelCompleted += OnLevelChanged;
    subscribed.LevelEvaluationChanged += OnLevelChanged;
    subscribed.LevelFailed += OnLevelFailed;
}

private void Unsubscribe()
{
    if (subscribed == null)
        return;

    subscribed.LevelStarted -= OnLevelReloaded;
    subscribed.LevelReset -= OnLevelReloaded;

    subscribed.LevelCompleted -= OnLevelChanged;
    subscribed.LevelEvaluationChanged -= OnLevelChanged;
    subscribed.LevelFailed -= OnLevelFailed;

    subscribed = null;
}


        private void OnLevelReloaded(SparkLevelDefinition level)
        {
            ClearRememberedComponent();
            RefreshStatus();
        }

        
private string ExplainRememberedResistor()
{
    SparkResistor resistor = GetRememberedResistor();

    if (resistor == null)
        return "No resistor is currently remembered.";

    SparkShelfItem item = GetRememberedShelfItem();

    string name = item != null
        ? item.ItemId
        : resistor.name;

    if (item != null && !item.IsAtWorkbench)
    {
        return name +
            " is still on the shelf. " +
            "It is not installed in the active circuit.";
    }

    float resistance = Mathf.Abs(resistor.ResistanceOhms);
    float voltage = Mathf.Abs(resistor.Voltage);
    float current = Mathf.Abs(resistor.Current);
    float power = Mathf.Abs(resistor.PowerWatts);
    float rating = resistor.MaximumPowerWatts;

    string explanation =
        name + " is installed.\n\n" +
        "Voltage magnitude: " + voltage.ToString("F3") + " V\n" +
        "Current magnitude: " +
            (current * 1000f).ToString("F2") + " mA\n" +
        "Power dissipated: " +
            (power * 1000f).ToString("F2") + " mW\n";

    if (resistor.Voltage < 0f || resistor.Current < 0f)
    {
        explanation +=
            "\nThe negative measurement sign indicates " +
            "the solver's reference direction.\n";
    }

    if (resistor.IsBurned)
    {
        explanation +=
            "\nThe resistor is marked as burned.";
    }
    else if (!resistor.IsConducting)
    {
        explanation +=
            "\nThe solver does not currently report " +
            "conduction through this resistor.";
    }
    else
    {
        explanation +=
            "\nCurrent is flowing through this resistor.";
    }

    if (rating > 0f)
    {
        float percent = power / rating * 100f;

        explanation +=
            "\nPower rating usage: " +
            percent.ToString("F1") + "%.";

        if (power > rating)
        {
            explanation +=
                "\nWarning: Dissipated power exceeds " +
                "the rated limit.";
        }
        else
        {
            explanation +=
                "\nDissipated power is below " +
                "the rated limit.";
        }
    }

    if (levelManager != null &&
        levelManager.ActiveLevel != null)
    {
        explanation +=
            "\n\nLevel objectives: " +
            levelManager.SatisfiedTargetCount +
            "/" +
            levelManager.ActiveTargetCount +
            ".";
    }

    return explanation;
}


        private void OnLevelChanged(SparkLevelDefinition _) => RefreshStatus();
        private void OnLevelFailed(SparkLevelDefinition _, string __) => RefreshStatus();

        [ContextMenu("Ask Spark / Toggle")]
        public void Toggle()
        {
            if (canvas == null) BuildUI();
            if (canvas.gameObject.activeSelf) Hide();
            else Show();
        }

        [ContextMenu("Ask Spark / Show")]
        public void Show()
        {
            if (canvas == null) BuildUI();
            canvas.gameObject.SetActive(true);
            RefreshStatus();
            SetAnswer("ASK SPARK", "Ask about your resistor, current, voltage, LED, or wiring. Or use Inspect, Explain, Hint, and Tutorial.");
        }

        public void Hide()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
        }

        /// <summary>One chat endpoint used by both the Ask button and other gameplay scripts.</summary>
        public void Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return;
            question = question.Trim();
            Resolve();
            if (questionInput != null) questionInput.text = string.Empty;

            if (routeQuestionsToExternalBrain && ExternalQuestionRequested != null)
            {
                pendingQuestion = question;
                pendingUntil = Time.unscaledTime + externalAnswerTimeout;
                SetAnswer("SPARK AI · THINKING", "Checking verified circuit context for: " + question);
                ExternalQuestionRequested.Invoke(question, BuildVerifiedContext());
                return;
            }

            pendingQuestion = null;
            string reply = LocalAnswer(question);
            SetAnswer("SPARK AI · ANSWER", reply);
            AnswerReady?.Invoke(question, reply);
        }

        /// <summary>External brain adapter calls this on the Unity main thread.</summary>
        public void DeliverExternalAnswer(string reply)
        {
            if (pendingQuestion == null) return;
            string question = pendingQuestion;
            pendingQuestion = null;
            if (string.IsNullOrWhiteSpace(reply)) reply = LocalAnswer(question);
            SetAnswer("SPARK AI · ANSWER", reply);
            AnswerReady?.Invoke(question, reply);
        }

        public string BuildVerifiedContext()
        {
            Resolve();
            var m = levelManager;
            if (m == null || m.ActiveLevel == null)
                return "No active level; solver readings unavailable.";
            return "Level: " + m.ActiveLevel.LevelId +
                "\nCompleted: " + m.IsCompleted +
                "\nFailed: " + m.IsFailed +
                "\nValid source: " + m.HasValidPowerSource +
                "\nClosed return: " + m.ClosedReturn +
                "\nSource short: " + m.SourceShorted +
                "\nTarget short: " + m.TargetShorted +
                "\nOverloaded: " + m.Overloaded +
                "\nWrong connection: " + m.WrongConnection +
                "\nObjectives: " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount +
                "\nTarget voltage: " + m.TargetVoltage.ToString("F4") + " V" +
                "\nTarget current: " + m.TargetCurrent.ToString("F6") + " A" +
                "\nTarget power: " + m.TargetPower.ToString("F5") + " W" +
                "\nNOTE: Target readings are not necessarily readings across a specific resistor.";
        }

        
private string ExplainPendingObjectives()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "Start a level to check pending objectives.";
    }

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    int pending = 0;

    if (levelManager.TargetResults != null)
    {
        foreach (var target in levelManager.TargetResults)
        {
            if (target.satisfied)
                continue;

            pending++;

            string name =
                string.IsNullOrWhiteSpace(target.displayName)
                    ? target.targetId
                    : target.displayName;

            sb.AppendLine("[PENDING] " + name);

            // Specific explanation for the known
            // Level 03 current-safety objective.
            if (name.IndexOf("safe current",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sb.AppendLine(
                    "The evaluator has not confirmed " +
                    "that the circuit current meets " +
                    "the required safety condition.");

                sb.AppendLine(
                    "Check the resistor value, current " +
                    "through the circuit, and the level's " +
                    "configured current limit.");
            }
        }
    }

    if (pending == 0)
    {
        return levelManager.IsCompleted
            ? "All objectives are satisfied. Level complete."
            : "No pending targets are listed, but the " +
              "level is not yet marked complete. " +
              "Check the evaluator's completion conditions.";
    }

    return
        "PENDING OBJECTIVES\n\n" +
        sb.ToString() +
        "\nProgress: " +
        levelManager.SatisfiedTargetCount + "/" +
        levelManager.ActiveTargetCount;
}


private string ExplainProgressOnly()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "No active level.";
    }

    int completed = levelManager.SatisfiedTargetCount;
    int total = levelManager.ActiveTargetCount;

    float percent = total > 0
        ? 100f * completed / total
        : 0f;

    return string.Format(
        "LEVEL PROGRESS\n\n" +
        "Completed: {0}/{1}\n" +
        "Progress: {2:F0}%\n" +
        "Level status: {3}\n\n" +
        "{4}",
        completed,
        total,
        percent,
        levelManager.RuntimeState,
        levelManager.IsCompleted
            ? "All required objectives are satisfied."
            : "Ask 'Which objective is pending?' " +
              "to see what remains.");
}


private string DescribeSafeCurrentConfiguration()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null ||
        levelManager.ActiveLevel.Targets == null)
    {
        return "Current-limit configuration is unavailable.";
    }

    System.Text.StringBuilder result =
        new System.Text.StringBuilder();

    bool foundTarget = false;

    foreach (SparkLevelTarget target
             in levelManager.ActiveLevel.Targets)
    {
        if (target == null)
            continue;

        string targetName =
            string.IsNullOrWhiteSpace(target.DisplayName)
                ? target.TargetId
                : target.DisplayName;

        bool isSafeCurrent =
            !string.IsNullOrWhiteSpace(targetName) &&
            targetName.IndexOf(
                "safe current",
                StringComparison.OrdinalIgnoreCase) >= 0;

        if (!isSafeCurrent)
            continue;

        foundTarget = true;

        result.AppendLine("CURRENT OBJECTIVE REQUIREMENTS");
        result.AppendLine();

        result.AppendLine("Objective: " + targetName);

        result.AppendLine(
            "Target type: " + target.Type);

        result.AppendLine(
            "Target component ID: " +
            (string.IsNullOrWhiteSpace(target.TargetComponentId)
                ? "NOT CONFIGURED"
                : target.TargetComponentId));

        result.AppendLine(
            "Minimum current: " +
            (target.MinimumCurrent * 1000f)
                .ToString("F3") + " mA");

        if (target.UseMaximumCurrent)
        {
            result.AppendLine(
                "Maximum current: " +
                (target.MaximumCurrent * 1000f)
                    .ToString("F3") + " mA");
        }
        else
        {
            result.AppendLine(
                "Maximum current: NOT ENABLED");
        }

        result.AppendLine(
            "Electrical enabled required: " +
            target.RequireElectricalEnabled);

        result.AppendLine(
            "Conduction required: " +
            target.RequireConduction);

        result.AppendLine(
            "Inverted result: " +
            target.Inverted);

        if (!target.IsConfigured())
        {
            result.AppendLine(
                "WARNING: Target configuration is incomplete.");
        }

        if (!target.IsPolarityConfigurationValid())
        {
            result.AppendLine(
                "WARNING: Target polarity configuration is invalid.");
        }

        result.AppendLine();
    }

    if (!foundTarget)
    {
        return
            "No objective named Safe current was found " +
            "in the active level definition.";
    }

    result.AppendLine(
        "NOTE: These are configured requirements. " +
        "The evaluator determines the actual pass/fail result.");

    return result.ToString();
}

private string RecommendResistorForSafeCurrent()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null ||
        levelManager.ActiveLevel.Targets == null)
    {
        return "Start a level to check current requirements.";
    }

    SparkLevelTarget currentTarget = null;

    foreach (SparkLevelTarget target
             in levelManager.ActiveLevel.Targets)
    {
        if (target == null)
            continue;

        string name = string.IsNullOrWhiteSpace(target.DisplayName)
            ? target.TargetId
            : target.DisplayName;

        if (!string.IsNullOrWhiteSpace(name) &&
            name.IndexOf("safe current",
                StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentTarget = target;
            break;
        }
    }

    if (currentTarget == null)
        return "No Safe current objective was found.";

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine("SPARK AI - RESISTOR GUIDANCE");
    sb.AppendLine();

    sb.AppendLine("Objective: " + currentTarget.DisplayName);
    sb.AppendLine("Type: " + currentTarget.Type);

    float minimum = currentTarget.MinimumCurrent;
    float maximum = currentTarget.MaximumCurrent;
    bool hasMaximum = currentTarget.UseMaximumCurrent;

    sb.AppendLine("Minimum: " +
        (minimum * 1000f).ToString("F3") + " mA");

    sb.AppendLine(hasMaximum
        ? "Maximum: " +
          (maximum * 1000f).ToString("F3") + " mA"
        : "Maximum: Not enabled");

    if (hasMaximum && maximum < minimum)
    {
        sb.AppendLine();
        sb.AppendLine(
            "WARNING: The configured maximum is below " +
            "the minimum. Review the level definition.");
    }

    SparkResistor resistor = GetRememberedResistor();
    SparkShelfItem item = GetRememberedShelfItem();

    if (resistor == null)
    {
        sb.AppendLine();
        sb.AppendLine(
            "Select the workstation resistor so I can " +
            "compare its actual current with these limits.");
        return sb.ToString();
    }

    if (item != null && !item.IsAtWorkbench)
    {
        sb.AppendLine();
        sb.AppendLine(
            item.ItemId + " is still on the shelf. " +
            "It is not the installed circuit resistor.");
        return sb.ToString();
    }

    float current = Mathf.Abs(resistor.Current);
    float voltage = Mathf.Abs(resistor.Voltage);
    float resistance = resistor.ResistanceOhms;

    sb.AppendLine();
    sb.AppendLine("SELECTED RESISTOR");
    sb.AppendLine("Resistance: " +
        resistance.ToString("F1") + " ohms");
    sb.AppendLine("Current: " +
        (current * 1000f).ToString("F3") + " mA");
    sb.AppendLine("Voltage across it: " +
        voltage.ToString("F3") + " V");

    bool belowMinimum = current < minimum;
    bool aboveMaximum = hasMaximum && current > maximum;

    sb.AppendLine();
    sb.AppendLine("SELECTED RESISTOR COMPARISON");

    if (belowMinimum)
    {
        sb.AppendLine(
            "Its measured current is below the " +
            "configured minimum.");

        sb.AppendLine(
            "In a simple resistive circuit, lowering " +
            "resistance may increase current when " +
            "the applied voltage stays constant.");
    }
    else if (aboveMaximum)
    {
        sb.AppendLine(
            "Its measured current is above the " +
            "configured maximum.");

        sb.AppendLine(
            "In a simple resistive circuit, increasing " +
            "resistance may reduce current when " +
            "the applied voltage stays constant.");
    }
    else
    {
        sb.AppendLine(
            "Its measured current is within the " +
            "configured numeric current range.");

        sb.AppendLine(
            "If Safe current is still pending, the " +
            "evaluator may be checking a different " +
            "component or another required condition.");
    }

    // Calculate a teaching example only.
    // This is NOT an authoritative target result.
    if (voltage > 0.0001f &&
        resistance > 0f &&
        !resistor.IsBurned)
    {
        sb.AppendLine();
        sb.AppendLine("OHM'S LAW ESTIMATE");

        if (aboveMaximum && maximum > 0f)
        {
            float estimatedMinimumResistance =
                voltage / maximum;

            sb.AppendLine(
                "At the same resistor voltage, " +
                "an estimated resistance of at least " +
                estimatedMinimumResistance.ToString("F1") +
                " ohms would bring the resistor current " +
                "to the configured maximum or below.");
        }
        else if (belowMinimum && minimum > 0f)
        {
            float estimatedMaximumResistance =
                voltage / minimum;

            sb.AppendLine(
                "At the same resistor voltage, " +
                "an estimated resistance of no more than " +
                estimatedMaximumResistance.ToString("F1") +
                " ohms would bring the resistor current " +
                "to the configured minimum or above.");
        }

        sb.AppendLine(
            "This estimate assumes the resistor voltage " +
            "remains unchanged after replacement.");
    }

    sb.AppendLine();
    sb.AppendLine(
        "IMPORTANT: This comparison uses the remembered " +
        "resistor, not necessarily the objective's " +
        "bound component. Only the level evaluator " +
        "determines whether the objective passes.");

    return sb.ToString();
}

private string ExplainCurrentObjectivePhase22()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null ||
        levelManager.ActiveLevel.Targets == null)
    {
        return "Start a level to inspect its current objectives.";
    }

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine("SPARK AI - CURRENT OBJECTIVE");
    sb.AppendLine();

    bool found = false;

    foreach (SparkLevelTarget target
             in levelManager.ActiveLevel.Targets)
    {
        if (target == null)
            continue;

        string name =
            string.IsNullOrWhiteSpace(target.DisplayName)
                ? target.TargetId
                : target.DisplayName;

        bool currentTarget =
            target.Type ==
                SparkLevelTarget.TargetType.CurrentRange ||
            (!string.IsNullOrEmpty(name) &&
             name.IndexOf(
                 "safe current",
                 StringComparison.OrdinalIgnoreCase) >= 0);

        if (!currentTarget)
            continue;

        found = true;

        bool? passed = null;

        if (levelManager.TargetResults != null)
        {
            foreach (var result in levelManager.TargetResults)
            {
                if (result.targetId == target.TargetId)
                {
                    passed = result.satisfied;
                    break;
                }
            }
        }

        sb.AppendLine("OBJECTIVE: " + name);
        sb.AppendLine(
            "Evaluator: " +
            (passed.HasValue
                ? passed.Value ? "PASSED" : "PENDING"
                : "Result unavailable"));

        sb.AppendLine(
            "Type: " + target.Type);

        sb.AppendLine(
            "Minimum current: " +
            (target.MinimumCurrent * 1000f)
                .ToString("F3") + " mA");

        sb.AppendLine(
            target.UseMaximumCurrent
                ? "Maximum current: " +
                  (target.MaximumCurrent * 1000f)
                      .ToString("F3") + " mA"
                : "Maximum current: Not enabled");

        sb.AppendLine(
            "Target component binding: " +
            (string.IsNullOrWhiteSpace(
                target.TargetComponentId)
                ? "Not configured"
                : target.TargetComponentId));

        sb.AppendLine(
            "Requires conduction: " +
            target.RequireConduction);

        sb.AppendLine(
            "Requires electrical enabled: " +
            target.RequireElectricalEnabled);

        sb.AppendLine(
            "Inverted objective: " +
            target.Inverted);

        if (!target.IsConfigured())
        {
            sb.AppendLine(
                "WARNING: Objective configuration " +
                "is incomplete.");
        }

        if (target.UseMaximumCurrent &&
            target.MaximumCurrent < target.MinimumCurrent)
        {
            sb.AppendLine(
                "WARNING: Maximum current is below " +
                "minimum current.");
        }

        sb.AppendLine();
    }

    if (!found)
    {
        return "This level has no configured " +
               "current-range objective.";
    }

    SparkResistor resistor = GetRememberedResistor();
    SparkShelfItem item = GetRememberedShelfItem();

    if (resistor != null)
    {
        sb.AppendLine("REMEMBERED RESISTOR");

        sb.AppendLine(
            item != null ? item.ItemId : resistor.name);

        if (item != null && !item.IsAtWorkbench)
        {
            sb.AppendLine(
                "This resistor is still on the shelf.");
        }
        else
        {
            sb.AppendLine(
                "Measured resistor current: " +
                (Mathf.Abs(resistor.Current) * 1000f)
                    .ToString("F3") + " mA");

            sb.AppendLine(
                "Measured resistor voltage: " +
                Mathf.Abs(resistor.Voltage)
                    .ToString("F3") + " V");

            sb.AppendLine(
                "Resistor power: " +
                (Mathf.Abs(resistor.PowerWatts) * 1000f)
                    .ToString("F2") + " mW");
        }

        sb.AppendLine();
    }

    sb.AppendLine(
        "NEXT STEP: Compare the current measured " +
        "at the objective's bound component with " +
        "the configured limits.");

    sb.AppendLine(
        "The selected resistor's readings are " +
        "informative, but may not be the readings " +
        "used by this objective.");

    sb.AppendLine();
    sb.AppendLine(
        "Level progress: " +
        levelManager.SatisfiedTargetCount + "/" +
        levelManager.ActiveTargetCount);

    return sb.ToString();
}


       
public string LocalAnswer(string question)
{
    Resolve();

    if (string.IsNullOrWhiteSpace(question))
        return "Ask Spark a question about your circuit.";

    
string q = question.Trim().ToLowerInvariant();


if (ContainsAny(q,
    "why is safe current failing",
    "why safe current",
    "safe current still pending",
    "diagnose current objective",
    "current objective details",
    "show current requirements"))
{
    return ExplainCurrentObjectivePhase22();
}


if (ContainsAny(q,
    "which resistor",
    "recommend resistor",
    "suggest resistor",
    "should i change resistor",
    "higher resistance",
    "lower resistance",
    "how to reduce current",
    "how to increase current"))
{
    return RecommendResistorForSafeCurrent();
}


if (ContainsAny(q,
    "safe current",
    "current safety",
    "safe current failing",
    "current too high",
    "current limit",
    "reduce current",
    "higher resistance",
    "lower resistance",
    "resistor overloaded"))
{
    return ExplainSafeCurrentObjective();
}



// =====================================================
// 1. WHICH OBJECTIVE IS STILL PENDING?
// =====================================================

if (ContainsAny(q,
    "which objective",
    "which target",
    "pending objective",
    "pending target",
    "what is missing",
    "what's missing",
    "what remains",
    "remaining objective"))
{
    return ExplainPendingObjectives();
}

// =====================================================
// 2. SHOW LEVEL PROGRESS
// =====================================================

if (ContainsAny(q,
    "level progress",
    "my progress",
    "show progress",
    "show my level progress",
    "how many targets",
    "how many objectives",
    "percent complete",
    "percentage complete"))
{
    return ExplainProgressOnly();
}

// =====================================================
// 3. WHY IS THE LEVEL NOT COMPLETE?
// =====================================================

if (ContainsAny(q,
    "objective",
    "target",
    "level complete",
    "level completed",
    "not complete",
    "not completed",
    "not winning",
    "why haven't i completed",
    "why is my led glowing",
    "why is the led glowing",
    "why am i not winning"))
{
    return ExplainLevelObjectives();
}



    if (GetRememberedResistor() != null &&
    (q.Contains("explain this resistor") ||
     q.Contains("what is happening") ||
     q.Contains("is it safe") ||
     q.Contains("overheating")))
{
    return ExplainRememberedResistor();
}

    SparkShelfItem rememberedItem = GetRememberedShelfItem();
    SparkResistor rememberedResistor = GetRememberedResistor();

    bool aboutRememberedComponent =
        q.Contains("resistor") ||
        q.Contains("component") ||
        q.Contains("selected") ||
        q.Contains("this one") ||
        q.Contains("through it") ||
        q.Contains("across it") ||
        q.Contains("is it ") ||
        q.Contains("does it ") ||
        q.Contains("its ") ||
        q.Contains("it placed");

    bool asksPlacement =
        ContainsAny(
            q,
            "placed",
            "workbench",
            "workstation",
            "on the shelf",
            "installed",
            "slot");

    bool asksCurrent =
        ContainsAny(
            q,
            "current",
            "amp",
            "milliamp",
            "ma through");

    bool asksVoltage =
        ContainsAny(
            q,
            "voltage",
            "volt",
            "potential difference");

    bool asksPower =
        ContainsAny(
            q,
            "power",
            "watt",
            "overheat",
            "burn");

    bool asksResistance =
        ContainsAny(
            q,
            "resistance",
            "ohm",
            "value");

    // =====================================================
    // REMEMBERED COMPONENT PLACEMENT
    // =====================================================

    if (rememberedItem != null &&
        asksPlacement &&
        aboutRememberedComponent)
    {
        if (rememberedItem.IsAtWorkbench)
        {
            string slotName =
                rememberedItem.CurrentSlot != null
                    ? rememberedItem.CurrentSlot.SlotId
                    : "an unreported workstation slot";

            return
                "You selected " +
                rememberedItem.ItemId +
                ".\n\n" +
                "It is currently on the workbench.\n" +
                "Registered slot: " + slotName + ".";
        }

        return
            "You selected " +
            rememberedItem.ItemId +
            ".\n\n" +
            "It is currently in shelf storage, " +
            "not placed on the workstation.";
    }

    // =====================================================
    // REMEMBERED RESISTOR ELECTRICAL READINGS
    // =====================================================

    if (rememberedResistor != null &&
        (aboutRememberedComponent ||
         asksCurrent ||
         asksVoltage ||
         asksPower ||
         asksResistance))
    {
        string name =
            rememberedItem != null
                ? rememberedItem.ItemId
                : rememberedResistor.name;

        bool onWorkbench =
            rememberedItem == null ||
            rememberedItem.IsAtWorkbench;

        string placement =
            rememberedItem == null
                ? "Workbench placement is not registered."
                : onWorkbench
                    ? "Installed on the workbench."
                    : "Currently in shelf storage.";

        // A stored resistor should not be described as
        // carrying current through the active circuit.
        if (!onWorkbench)
        {
            return string.Format(
                "{0}\n" +
                "Resistance: {1:F1} Ω\n" +
                "{2}\n\n" +
                "This resistor is not installed in the " +
                "workstation circuit. The power source's " +
                "voltage cannot be assumed to appear " +
                "across this resistor.",
                name,
                rememberedResistor.ResistanceOhms,
                placement);
        }

        string electricalState =
            rememberedResistor.IsBurned
                ? "The resistor is marked as burned."
                : rememberedResistor.IsConducting
                    ? "The solver reports conduction through this resistor."
                    : "The solver does not currently report conduction through this resistor.";

        return string.Format(
            "{0}\n" +
            "{1}\n\n" +
            "Resistance: {2:F1} Ω\n" +
            "Voltage across resistor: {3:F3} V\n" +
            "Current through resistor: {4:F6} A\n" +
            "Power dissipated: {5:F4} W\n" +
            "Power rating: {6:F3} W\n\n" +
            "{7}",
            name,
            placement,
            rememberedResistor.ResistanceOhms,
            rememberedResistor.Voltage,
            rememberedResistor.Current,
            rememberedResistor.PowerWatts,
            rememberedResistor.MaximumPowerWatts,
            electricalState);
    }

    // =====================================================
    // RESISTOR UNDER CURSOR - FALLBACK
    // =====================================================

    if (ContainsAny(q, "resistor", "resistance", "ohm"))
    {
        SparkResistor r = ResistorUnderCursor();

        if (r != null)
        {
            RememberComponent(r.gameObject);

            return string.Format(
                "{0}\n" +
                "Resistance: {1:F1} Ω\n" +
                "Solver voltage: {2:F3} V\n" +
                "Solver current: {3:F6} A\n" +
                "Power: {4:F4} W",
                r.name,
                r.ResistanceOhms,
                r.Voltage,
                r.Current,
                r.PowerWatts);
        }

        return
            "A resistor limits current according to " +
            "Ohm's law: I = V / R.\n\n" +
            "Select a resistor to ask about its " +
            "specific measurements.\n\n" +
            BriefCircuitState();
    }

    // =====================================================
    // LEVEL TARGET MEASUREMENTS
    // =====================================================

    if (asksVoltage || asksCurrent || asksPower)
    {
        if (levelManager == null ||
            levelManager.ActiveLevel == null)
        {
            return "Start a level to view live electrical readings.";
        }

        return string.Format(
            "Evaluated level target readings:\n" +
            "Voltage: {0:F3} V\n" +
            "Current: {1:F6} A\n" +
            "Power: {2:F4} W\n\n" +
            "These are level-target measurements, " +
            "not necessarily measurements across " +
            "the selected resistor.",
            levelManager.TargetVoltage,
            levelManager.TargetCurrent,
            levelManager.TargetPower);
    }

    // =====================================================
    // OTHER COMPONENTS
    // =====================================================

    if (ContainsAny(q, "led", "lamp", "bulb", "glow", "light"))
    {
        return
            "An LED requires correct polarity " +
            "and an adequate current path.\n\n" +
            BriefCircuitState();
    }

    if (ContainsAny(
            q,
            "wire",
            "connect",
            "terminal",
            "switch",
            "circuit",
            "why",
            "wrong",
            "help",
            "hint",
            "work",
            "complete"))
    {
        return BriefCircuitState();
    }

    return
        "Ask me about the selected component, " +
        "resistance, current, voltage, power, " +
        "LED polarity, wiring, or level objectives.\n\n" +
        BriefCircuitState();
}


private string ExplainSafeCurrentObjective()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "Start a level to inspect current safety.";
    }

    var manager = levelManager;

    System.Text.StringBuilder text =
        new System.Text.StringBuilder();

    text.AppendLine("SPARK AI - SAFE CURRENT DIAGNOSIS");
    text.AppendLine();

    bool foundObjective = false;
    bool objectiveSatisfied = false;

    if (manager.TargetResults != null)
    {
        foreach (var target in manager.TargetResults)
        {
            string name =
                string.IsNullOrWhiteSpace(target.displayName)
                    ? target.targetId
                    : target.displayName;

            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (name.IndexOf(
                    "safe current",
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            foundObjective = true;
            objectiveSatisfied = target.satisfied;

            text.AppendLine(
                "Safe current: " +
                (target.satisfied ? "PASSED" : "PENDING"));
            break;
        }
    }

    if (!foundObjective)
    {
        text.AppendLine(
            "No objective named 'Safe current' was found. " +
            "The level's configured objectives remain authoritative.");
    }

    text.AppendLine(
        "Level progress: " +
        manager.SatisfiedTargetCount + "/" +
        manager.ActiveTargetCount);

    text.AppendLine();

    // The remembered resistor may be the component
    // the player selected earlier.
    SparkResistor resistor = GetRememberedResistor();
    SparkShelfItem item = GetRememberedShelfItem();

    if (resistor != null)
    {
        string resistorName =
            item != null ? item.ItemId : resistor.name;

        text.AppendLine("SELECTED RESISTOR");
        text.AppendLine(resistorName);

        if (item != null && !item.IsAtWorkbench)
        {
            text.AppendLine(
                "This resistor is still on the shelf. " +
                "Its readings must not be treated as " +
                "the current through the active workstation.");
        }
        else
        {
            float amps = Mathf.Abs(resistor.Current);
            float milliamps = amps * 1000f;
            float volts = Mathf.Abs(resistor.Voltage);
            float watts = Mathf.Abs(resistor.PowerWatts);
            float ratedWatts = resistor.MaximumPowerWatts;

            text.AppendLine(
                "Resistance: " +
                resistor.ResistanceOhms.ToString("F1") +
                " ohms");

            text.AppendLine(
                "Current magnitude: " +
                milliamps.ToString("F3") + " mA");

            text.AppendLine(
                "Voltage magnitude: " +
                volts.ToString("F3") + " V");

            text.AppendLine(
                "Power: " +
                (watts * 1000f).ToString("F2") +
                " mW");

            if (ratedWatts > 0f)
            {
                float usage = watts / ratedWatts * 100f;

                text.AppendLine(
                    "Resistor power rating usage: " +
                    usage.ToString("F1") + "%");

                if (watts > ratedWatts)
                {
                    text.AppendLine(
                        "Warning: The reported resistor power " +
                        "exceeds its rating.");
                }
            }

            if (resistor.IsBurned)
            {
                text.AppendLine(
                    "The resistor is marked as burned.");
            }
            else if (!resistor.IsConducting)
            {
                text.AppendLine(
                    "The solver does not report conduction " +
                    "through this resistor.");
            }
        }
    }
    else
    {
        text.AppendLine(
            "No resistor is remembered. Select the " +
            "workstation resistor to inspect its readings.");
    }

    text.AppendLine();

    if (foundObjective && objectiveSatisfied)
    {
        text.AppendLine(
            "The evaluator confirms that the " +
            "Safe current objective is satisfied.");
    }
    else
    {
        text.AppendLine(
            "NEXT STEP:");
        text.AppendLine(
            "Check the current limit configured for " +
            "the Safe current objective. Compare it " +
            "with the relevant circuit current.");

        text.AppendLine(
            "If current is above the allowed maximum, " +
            "increasing resistance can reduce current " +
            "when the applied voltage remains the same.");

        text.AppendLine(
            "Do not change components until you know " +
            "which current the objective measures.");
    }

    text.AppendLine();
    text.Append(
        "Note: The selected resistor's current may " +
        "differ from the current measured by the " +
        "level's Safe current objective.");

        
        text.AppendLine();
        text.AppendLine(
            DescribeSafeCurrentConfiguration());

        return text.ToString();

}


private string ExplainLevelObjectives()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "Start a level to inspect its objectives.";
    }

    var manager = levelManager;

    System.Text.StringBuilder response =
        new System.Text.StringBuilder();

    response.AppendLine(
        "LEVEL OBJECTIVE DIAGNOSIS");

    response.AppendLine();

    response.AppendLine(
        "Completed targets: " +
        manager.SatisfiedTargetCount +
        "/" +
        manager.ActiveTargetCount);

    response.AppendLine();

    if (manager.TargetResults == null ||
        manager.TargetResults.Count == 0)
    {
        response.AppendLine(
            "Individual target results are not " +
            "available yet.");
    }
    else
    {
        foreach (var target in manager.TargetResults)
        {
            response.Append(
                target.satisfied
                    ? "[PASS] "
                    : "[PENDING] ");

            response.AppendLine(
                string.IsNullOrWhiteSpace(target.displayName)
                    ? target.targetId
                    : target.displayName);
        }
    }

    response.AppendLine();

    if (manager.IsCompleted)
    {
        response.AppendLine(
            "The level evaluator confirms completion.");
    }
    else if (manager.SourceShorted ||
             manager.TargetShorted)
    {
        response.AppendLine(
            "A short circuit is detected. " +
            "Inspect wires that bypass components.");
    }
    else if (manager.Overloaded)
    {
        response.AppendLine(
            "The evaluator reports an overload. " +
            "Inspect component power ratings.");
    }
    else if (!manager.HasValidPowerSource)
    {
        response.AppendLine(
            "The evaluator has not recognized " +
            "a valid active power source.");
    }
    else if (!manager.ClosedReturn)
    {
        response.AppendLine(
            "The return path is open. " +
            "Check switch and wire connections.");
    }
    else if (manager.WrongConnection)
    {
        response.AppendLine(
            "An invalid electrical connection " +
            "was detected.");
    }
    else
    {
        response.AppendLine(
            "The circuit has a recognized return " +
            "path, but one or more required " +
            "conditions are still unmet.");
    }

    response.AppendLine();
    response.Append(
        "A glowing LED does not by itself " +
        "confirm that every level objective passed.");

    return response.ToString();
}



        private string BriefCircuitState()
        {
            var m = levelManager;
            if (m == null || m.ActiveLevel == null) return "Start a level for circuit-specific guidance.";
            if (m.IsCompleted) return "The level evaluator confirms completion: " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount + " objectives.";
            if (m.IsFailed) return "The level has failed. " + (string.IsNullOrEmpty(m.LastFailureReason) ? "Review circuit connections and power ratings." : m.LastFailureReason);
            string diagnosis;
            if (m.SourceShorted || m.TargetShorted) diagnosis = "A short circuit is detected. Look for a path that bypasses a component.";
            else if (m.Overloaded) diagnosis = "The evaluator reports an overload. Inspect component power ratings.";
            else if (!m.HasValidPowerSource) diagnosis = "No valid active source is detected. Check power-source connections.";
            else if (!m.ClosedReturn) diagnosis = "The source is recognized, but the return path is open. Check wiring and switch.";
            else if (m.WrongConnection) diagnosis = "The evaluator reports an invalid connection. Inspect terminal endpoints.";
            else diagnosis = "A return path is detected, but some objectives remain unsatisfied.";
            return diagnosis + "\nObjectives: " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount + ".";
        }

        private SparkResistor ResistorUnderCursor()
        {
            if (sceneCamera == null) return null;
            RaycastHit[] hits = Physics.RaycastAll(sceneCamera.ScreenPointToRay(Input.mousePosition),
                inspectDistance, inspectLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                SparkResistor r = hit.collider.GetComponentInParent<SparkResistor>();
                if (r != null) return r;
            }
            return null;
        }

        private static bool ContainsAny(string input, params string[] terms)
        {
            foreach (string term in terms) if (input.Contains(term)) return true;
            return false;
        }

        public void ExplainCircuit()
        {
            Resolve();
            if (levelManager == null || levelManager.ActiveLevel == null)
            {
                SetAnswer("CIRCUIT STATUS", "There is no active level to evaluate.");
                return;
            }

            var m = levelManager;
            string text;
            if (m.IsCompleted)
                text = "The level evaluator has confirmed victory. All required objectives are satisfied.";
            else if (m.IsFailed)
                text = "The level has failed. " + (string.IsNullOrEmpty(m.LastFailureReason) ? "Review the circuit configuration." : m.LastFailureReason);
            else if (m.SourceShorted || m.TargetShorted)
                text = "The level evaluator detects a short circuit. Check connections that bypass a component or connect supply terminals incorrectly.";
            else if (m.Overloaded)
                text = "An overload is reported. Check component power ratings and the circuit configuration.";
            else if (!m.HasValidPowerSource)
                text = "The evaluator does not recognize a valid active power source. Inspect the source and its terminal connections.";
            else if (!m.ClosedReturn)
                text = "A valid source is detected, but the return path is open. Inspect wire endpoints and the switch.";
            else if (m.WrongConnection)
                text = "The level evaluator reports an invalid connection. Inspect the terminal connections.";
            else
                text = "A return path is detected, but some objectives remain incomplete. Check the pending targets.";

            text += string.Format("\n\nObjectives {0}/{1}  |  Target V {2:F3} V  |  I {3:F5} A  |  P {4:F4} W",
                m.SatisfiedTargetCount, m.ActiveTargetCount, m.TargetVoltage, m.TargetCurrent, m.TargetPower);
            SetAnswer("LIVE CIRCUIT", text);
        }

        public void InspectUnderCursor()
        {
            Resolve();
            if (sceneCamera == null)
            {
                SetAnswer("INSPECT", "Assign a scene camera in the Inspector.");
                return;
            }

            Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, inspectDistance, inspectLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                SparkResistor resistor = hit.collider.GetComponentInParent<SparkResistor>();
                if (resistor == null) continue;
                RememberComponent(resistor.gameObject);
                string text = string.Format("{0}: {1:F1} Ω.\nSolver voltage: {2:F3} V\nSolver current: {3:F6} A\nDissipated power: {4:F4} W\nRated power: {5:F3} W.",
                    resistor.name, resistor.ResistanceOhms, resistor.Voltage,
                    resistor.Current, resistor.PowerWatts, resistor.MaximumPowerWatts);
                if (resistor.IsBurned) text += "\nThe resistor has failed and should behave as an open circuit when the solver honors its failure state.";
                else if (!resistor.IsConducting) text += "\nThe resistor is not currently reported as conducting.";
                else text += "\nCurrent is flowing according to its solved electrical state.";
                SetAnswer("RESISTOR INSPECTOR", text);
                return;
            }
            SetAnswer("INSPECT", "Point at a resistor, then choose Inspect. This version supports accurate live resistor readings; other component types will be integrated separately.");
        }

        public void GiveHint()
        {
            Resolve();
            string signature = GetStateSignature();
            if (signature != lastStateSignature)
            {
                lastStateSignature = signature;
                hintStage = 0;
            }
            hintStage = Mathf.Min(3, hintStage + 1);
            if (levelManager == null || levelManager.ActiveLevel == null)
            {
                SetAnswer("HINT", "Start a level before requesting circuit guidance.");
                return;
            }
            if (levelManager.IsCompleted)
            {
                SetAnswer("LEVEL COMPLETE", "This level is already won. Use Inspect to learn from the working circuit.");
                return;
            }
            string message;
            if (hintStage == 1)
                message = "Check the level objectives and identify which electrical condition is still missing.";
            else if (hintStage == 2)
                message = !levelManager.HasValidPowerSource ? "Check the power source and its connections. The evaluator sees no valid active source."
                    : !levelManager.ClosedReturn ? "Examine the switch and the return wire. The evaluator reports an open return path."
                    : levelManager.SourceShorted || levelManager.TargetShorted ? "Trace any wire that might bypass a component or short supply terminals."
                    : "Review the unsatisfied objective targets and the solver's measurements.";
            else
                message = "Current authoritative status:\n" +
                    "Source detected: " + levelManager.HasValidPowerSource +
                    "\nClosed return: " + levelManager.ClosedReturn +
                    "\nSource short: " + levelManager.SourceShorted +
                    "\nTarget short: " + levelManager.TargetShorted +
                    "\nOverloaded: " + levelManager.Overloaded +
                    string.Format("\nObjectives: {0}/{1}", levelManager.SatisfiedTargetCount, levelManager.ActiveTargetCount);
            SetAnswer("HINT " + hintStage + " / 3", message);
        }

        public void OpenTutorial()
        {
            Resolve();
            if (tutorial == null)
            {
                SetAnswer("TUTORIAL", "Assign SparkTutorialDirector in the Inspector.");
                return;
            }
            if (levelManager != null && levelManager.IsCompleted)
            {
                SetAnswer("LEVEL COMPLETE", "This level is already completed. The beginner tutorial will not restart automatically.");
                return;
            }
            if (!tutorial.IsRunning) tutorial.BeginDefault();
            SetAnswer("TUTORIAL", "The guided lesson is available. Follow its on-screen steps; Ask Spark stays optional.");
        }

        public void OpenDetailedHints()
        {
            Resolve();
            if (troubleshooter == null)
            {
                GiveHint();
                return;
            }
            troubleshooter.Show();
            SetAnswer("DETAILED HELP", "The existing adaptive troubleshooting panel is open. Close it with F6 when finished.");
        }

        private string GetStateSignature()
        {
            if (levelManager == null || levelManager.ActiveLevel == null) return "no-level";
            var m = levelManager;
            return m.ActiveLevel.LevelId + ":" + m.RuntimeState + ":" + m.HasValidPowerSource +
                ":" + m.ClosedReturn + ":" + m.SourceShorted + ":" + m.TargetShorted +
                ":" + m.Overloaded + ":" + m.SatisfiedTargetCount;
        }

        private void RefreshStatus()
        {
            if (status == null) return;
            string next = GetStateSignature();
            if (next != lastStateSignature) { lastStateSignature = next; hintStage = 0; }
            if (levelManager == null || levelManager.ActiveLevel == null)
                status.text = "NO ACTIVE LEVEL";
            else
                status.text = levelManager.ActiveLevel.DisplayName + "  •  " +
                    levelManager.RuntimeState + "  •  " + levelManager.SatisfiedTargetCount + "/" +
                    levelManager.ActiveTargetCount + " TARGETS";
        }

        
private void SetAnswer(string title, string text)
{
    // =====================================================
    // ENSURE AI PANEL EXISTS
    // =====================================================

    if (canvas == null)
        BuildUI();

    if (canvas == null)
        return;

    if (!canvas.gameObject.activeSelf)
        canvas.gameObject.SetActive(true);

    // =====================================================
    // UPDATE AI RESPONSE
    // =====================================================

    if (heading != null)
        heading.text = title;

    if (answer != null)
        answer.text = text ?? string.Empty;

    RefreshStatus();

    // =====================================================
    // RESET ANSWER SCROLL TO TOP
    // =====================================================

    if (answerScroll != null && answer != null)
    {
        // Stop scrolling inertia from the previous reply.
        answerScroll.StopMovement();

        // Update text height before repositioning.
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            answer.GetComponent<RectTransform>());

        Canvas.ForceUpdateCanvases();

        answerScroll.verticalNormalizedPosition = 1f;
    }

    // =====================================================
    // DEBUG LOG
    // =====================================================

    if (logActions)
    {
        Debug.Log(
            "[AskSpark] " + title + ": " + text,
            this);
    }

    // =====================================================
    // OPTIONAL AI VOICE
    // =====================================================

    if (speakReplies &&
        voice != null &&
        !string.IsNullOrWhiteSpace(text))
    {
        voice.SpeakText(text);
    }
}


        private void BuildUI()
        {
            if (canvas != null) return;
            GameObject root = new GameObject("AskSpark_UnifiedUI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.GetComponent<CanvasGroup>().blocksRaycasts = true;

            GameObject panel = new GameObject("AskSparkPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = panelSize;
            rect.anchoredPosition = new Vector2(-24f, 24f);
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(.025f, .055f, .09f, .95f);
            bg.raycastTarget = false;

            heading = MakeText("Heading", panel.transform, new Vector2(0f, 186f),
                new Vector2(panelSize.x - 40f, 32f), 22, Color.cyan, FontStyle.Bold);
            status = MakeText("LevelStatus", panel.transform, new Vector2(0f, 154f),
                new Vector2(panelSize.x - 40f, 30f), 15, new Color(.68f, .82f, .89f), FontStyle.Normal);
            
GameObject scrollObject = new GameObject(
    "ExplanationScroll",
    typeof(RectTransform),
    typeof(ScrollRect));

scrollObject.transform.SetParent(
    panel.transform, false);

RectTransform scrollRect =
    scrollObject.GetComponent<RectTransform>();

scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
scrollRect.pivot = new Vector2(0.5f, 0.5f);
scrollRect.sizeDelta =
    new Vector2(panelSize.x - 44f, 208f);
scrollRect.anchoredPosition =
    new Vector2(0f, 27f);

GameObject viewport = new GameObject(
    "Viewport",
    typeof(RectTransform),
    typeof(RectMask2D));

viewport.transform.SetParent(
    scrollObject.transform, false);

RectTransform viewportRect =
    viewport.GetComponent<RectTransform>();

viewportRect.anchorMin = Vector2.zero;
viewportRect.anchorMax = Vector2.one;
viewportRect.offsetMin = Vector2.zero;
viewportRect.offsetMax = Vector2.zero;


 // =====================================================
 // SCROLLABLE AI EXPLANATION CONTENT
 // =====================================================

GameObject content = new GameObject(
    "Content",
    typeof(RectTransform));

content.transform.SetParent(
    viewport.transform, false);

RectTransform contentRect =
    content.GetComponent<RectTransform>();

contentRect.anchorMin = new Vector2(0f, 1f);
contentRect.anchorMax = new Vector2(1f, 1f);
contentRect.pivot = new Vector2(0.5f, 1f);
contentRect.anchoredPosition = Vector2.zero;
contentRect.sizeDelta = Vector2.zero;

// =====================================================
// AI ANSWER TEXT
// =====================================================

answer = MakeText(
    "Explanation",
    content.transform,
    Vector2.zero,
    new Vector2(panelSize.x - 54f, 0f),
    17,
    Color.white,
    FontStyle.Normal);

RectTransform answerRect =
    answer.GetComponent<RectTransform>();

answerRect.anchorMin = new Vector2(0f, 1f);
answerRect.anchorMax = new Vector2(1f, 1f);
answerRect.pivot = new Vector2(0.5f, 1f);

answerRect.anchoredPosition = Vector2.zero;
answerRect.sizeDelta = new Vector2(-10f, 0f);

answer.alignment = TextAnchor.UpperLeft;
answer.horizontalOverflow =
    HorizontalWrapMode.Wrap;

answer.verticalOverflow =
    VerticalWrapMode.Overflow;

answer.resizeTextForBestFit = false;

// =====================================================
// DYNAMIC TEXT HEIGHT
// =====================================================

ContentSizeFitter fitter =
    answer.gameObject.AddComponent<ContentSizeFitter>();

fitter.horizontalFit =
    ContentSizeFitter.FitMode.Unconstrained;

fitter.verticalFit =
    ContentSizeFitter.FitMode.PreferredSize;

// =====================================================
// SCROLL RECT CONFIGURATION
// =====================================================

ScrollRect scrolling =
    scrollObject.GetComponent<ScrollRect>();

scrolling.viewport = viewportRect;

// Scroll the dynamically sized Text directly.
scrolling.content = answerRect;

scrolling.horizontal = false;
scrolling.vertical = true;

scrolling.scrollSensitivity = 30f;

scrolling.movementType =
    ScrollRect.MovementType.Clamped;

// Save reference for resetting scroll position.
answerScroll = scrolling;



            float buttonY = -130f;
            float w = (panelSize.x - 50f) / 4f;
            float left = -panelSize.x * .5f + 25f + w * .5f;
            MakeButton("INSPECT", panel.transform, new Vector2(left, buttonY), new Vector2(w - 6f, 38f), InspectUnderCursor);
            MakeButton("EXPLAIN", panel.transform, new Vector2(left + w, buttonY), new Vector2(w - 6f, 38f), ExplainCircuit);
            MakeButton("HINT", panel.transform, new Vector2(left + 2f * w, buttonY), new Vector2(w - 6f, 38f), GiveHint);
            MakeButton("TUTORIAL", panel.transform, new Vector2(left + 3f * w, buttonY), new Vector2(w - 6f, 38f), OpenTutorial);
            // A single chat entry replaces the old F8 conversation overlay.
            GameObject entry = new GameObject("QuestionInput", typeof(RectTransform), typeof(Image), typeof(InputField));
            entry.transform.SetParent(panel.transform, false);
            RectTransform inputRect = entry.GetComponent<RectTransform>();
            inputRect.anchorMin = inputRect.anchorMax = new Vector2(.5f, .5f);
            inputRect.sizeDelta = new Vector2(panelSize.x - 139f, 43f);
            inputRect.anchoredPosition = new Vector2(-47f, -184f);
            entry.GetComponent<Image>().color = new Color(.10f, .18f, .24f, 1f);
            questionInput = entry.GetComponent<InputField>();
            Text inputText = MakeText("TypedQuestion", entry.transform, Vector2.zero,
                new Vector2(panelSize.x - 158f, 39f), 17, Color.white, FontStyle.Normal);
            questionInput.textComponent = inputText;
            Text placeholder = MakeText("Placeholder", entry.transform, Vector2.zero,
                new Vector2(panelSize.x - 158f, 39f), 16, Color.gray, FontStyle.Italic);
            placeholder.text = "Why is the current zero?";
            questionInput.placeholder = placeholder;
            questionInput.lineType = InputField.LineType.SingleLine;
            questionInput.onEndEdit.AddListener(s =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Ask(s);
            });
            MakeButton("ASK", panel.transform, new Vector2(panelSize.x * .5f - 58f, -184f),
                new Vector2(86f, 43f), () => Ask(questionInput.text));
            MakeButton("×", panel.transform, new Vector2(panelSize.x * .5f - 20f, panelSize.y * .5f - 19f),
                new Vector2(30f, 30f), Hide);
            root.SetActive(false);
        }

        private static Text MakeText(string name, Transform parent, Vector2 position, Vector2 size,
            int sizePx, Color color, FontStyle style)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = sizePx;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.raycastTarget = false;
            return text;
        }

        private static void MakeButton(string caption, Transform parent, Vector2 at,
            Vector2 size, UnityEngine.Events.UnityAction clicked)
        {
            GameObject go = new GameObject("Button_" + caption, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
            rt.anchoredPosition = at;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(.05f, .35f, .43f, 1f);
            go.GetComponent<Button>().onClick.AddListener(clicked);
            Text text = MakeText("Label", go.transform, Vector2.zero, size, 15, Color.white, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = caption;
        }





        
        [Header("Component Memory")]
        [SerializeField]
        private GameObject rememberedComponent;

        public GameObject RememberedComponent
        {
            get
            {
                return rememberedComponent;
            }
        }

        public void RememberComponent(GameObject componentObject)
        {
            if (componentObject == null)
                return;

            SparkShelfItem shelfItem =
                componentObject.GetComponent<SparkShelfItem>();

            if (shelfItem == null)
            {
                shelfItem =
                    componentObject.GetComponentInParent<SparkShelfItem>();
            }

            if (shelfItem == null)
            {
                shelfItem =
                    componentObject.GetComponentInChildren<SparkShelfItem>(true);
            }

            // Store the actual shelf-item object when available.
            // This keeps memory tied to the component,
            // rather than its visual child or collider.
            rememberedComponent =
                shelfItem != null
                    ? shelfItem.gameObject
                    : componentObject;

            Debug.Log(
                "[Ask Spark] Remembered component: " +
                rememberedComponent.name,
                rememberedComponent);
        }

        public void ClearRememberedComponent()
        {
            rememberedComponent = null;
        }

        
        private SparkResistor GetRememberedResistor()
        {
            if (rememberedComponent == null)
                return null;

            SparkResistor resistor =
                rememberedComponent.GetComponent<SparkResistor>();

            if (resistor == null)
            {
                resistor =
                    rememberedComponent.GetComponentInChildren<SparkResistor>(true);
            }

            if (resistor == null)
            {
                resistor =
                    rememberedComponent.GetComponentInParent<SparkResistor>();
            }

            return resistor;
        }

        private SparkShelfItem GetRememberedShelfItem()
        {
            if (rememberedComponent == null)
                return null;

            SparkShelfItem item =
                rememberedComponent.GetComponent<SparkShelfItem>();

            if (item == null)
            {
                item =
                    rememberedComponent.GetComponentInParent<SparkShelfItem>();
            }

            if (item == null)
            {
                item =
                    rememberedComponent.GetComponentInChildren<SparkShelfItem>(true);
            }

            return item;
        }


    }
}
