using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.AI;
using ProjectSpark.Gameplay.Shelf;
using TMPro;
using UnityEngine.EventSystems;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

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
        private SparkAIWorldContext worldContext;

        private GameObject lastHoveredInspectable;

        private readonly SparkAILocalBrain localBrain =
    new SparkAILocalBrain();

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

        
        [Header("Ask Spark UI - Assigned in Inspector")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI heading;
        [SerializeField] private TextMeshProUGUI status;
        [SerializeField] private TextMeshProUGUI answer;
       [SerializeField] private TMP_InputField questionInput;
        [SerializeField] private ScrollRect answerScroll;

        [Header("Ask Spark Buttons")]
        [SerializeField] private Button inspectButton;
        [SerializeField] private Button explainButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button askButton;
        [SerializeField] private Button closeButton;


       
        private string pendingQuestion;
        private float pendingUntil;
       
        private int hintStage;
        private string lastStateSignature = string.Empty;
        private LevelGamePlayManager subscribed;

       
private void Awake()
{
    Resolve();
    BindButtons();
}

private void OnDestroy()
{
    UnbindButtons();
    Unsubscribe();
}

[ContextMenu("Ask Spark / Toggle")]
public void Toggle()
{
    if (canvas == null)
    {
        Debug.LogWarning(
            "[Ask Spark] Canvas reference is missing.",
            this);
        return;
    }

    if (canvas.gameObject.activeSelf)
        Hide();
    else
        Show();
}


private void BindButtons()
{
    if (inspectButton != null)
    {
        inspectButton.onClick.RemoveListener(
            InspectUnderCursor);

        inspectButton.onClick.AddListener(
            InspectUnderCursor);
    }

    if (explainButton != null)
    {
        explainButton.onClick.RemoveListener(
            ExplainCircuit);

        explainButton.onClick.AddListener(
            ExplainCircuit);
    }

    if (hintButton != null)
    {
        hintButton.onClick.RemoveListener(
            GiveHint);

        hintButton.onClick.AddListener(
            GiveHint);
    }

    if (tutorialButton != null)
    {
        tutorialButton.onClick.RemoveListener(
            OpenTutorial);

        tutorialButton.onClick.AddListener(
            OpenTutorial);
    }

    if (askButton != null)
    {
        askButton.onClick.RemoveListener(
            AskFromInput);

        askButton.onClick.AddListener(
            AskFromInput);
    }

    if (closeButton != null)
    {
        closeButton.onClick.RemoveListener(
            Hide);

        closeButton.onClick.AddListener(
            Hide);
    }
}


[ContextMenu("Ask Spark / Show")]
public void Show()
{
    if (canvas == null)
        return;

    canvas.gameObject.SetActive(true);

    RefreshStatus();

    SetAnswer(
        "ASK SPARK",
        "Ask about the current level, objectives, " +
        "components, current, voltage, or wiring.");
}

public void Hide()
{
    if (canvas != null)
        canvas.gameObject.SetActive(false);
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

        private void Update()
        {
            TrackInspectableUnderCursor();
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
        
private void TrackInspectableUnderCursor()
{
    if (sceneCamera == null)
        return;

    // Keep the previous world selection while the
    // player moves the cursor over a UI button.
    if (EventSystem.current != null &&
        EventSystem.current.IsPointerOverGameObject())
    {
        return;
    }

    Ray ray = sceneCamera.ScreenPointToRay(
        Input.mousePosition);

    if (TryFindInspectable(ray, out GameObject found))
    {
        lastHoveredInspectable = found;
    }
}

private bool TryFindInspectable(
    Ray ray,
    out GameObject found)
{
    found = null;

    RaycastHit[] hits = Physics.RaycastAll(
        ray,
        inspectDistance,
        inspectLayers,
        QueryTriggerInteraction.Ignore);

    Array.Sort(
        hits,
        (a, b) => a.distance.CompareTo(b.distance));

    foreach (RaycastHit hit in hits)
    {
        if (hit.collider == null)
            continue;

        GameObject candidate =
            hit.collider.gameObject;

        if (IsInspectable(candidate))
        {
            found = candidate;
            return true;
        }
    }

    return false;
}

private static bool IsInspectable(
    GameObject obj)
{
    if (obj == null)
        return false;

    return
        obj.GetComponentInParent<SparkResistor>() != null ||
        obj.GetComponentInParent<SparkLED>() != null ||
        obj.GetComponentInParent<SparkSwitch>() != null ||
        obj.GetComponentInParent<SparkPowerSupply>() != null ||
        obj.GetComponentInParent<SparkElectricalComponent>() != null;
}


        private void Resolve()
        {
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (tutorial == null) tutorial = FindFirstObjectByType<SparkTutorialDirector>();
            if (troubleshooter == null) troubleshooter = FindFirstObjectByType<SparkAIAdaptiveTroubleshooter>();
            if (voice == null) voice = FindFirstObjectByType<SparkAIVoiceController>();
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (worldContext == null)
                {
                    worldContext =
                        GetComponent<SparkAIWorldContext>();

                    if (worldContext == null)
                    {
                        worldContext =
                            FindFirstObjectByType<SparkAIWorldContext>();
                    }
                }
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


        
private void OnLevelReloaded(
    SparkLevelDefinition level)
{
    // Forget conversations about the previous
    // circuit configuration or level attempt.
    localBrain.Clear();
    lastHoveredInspectable = null;

    // A selected component may have been destroyed
    // or replaced by the level layout loader.
    ClearRememberedComponent();

    // Reset progressive hint history.
    hintStage = 0;
    lastStateSignature = string.Empty;

    // Discard any pending external response.
    pendingQuestion = null;
    

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

    // =====================================================
    // 1. GET GLOBAL AI WORLD CONTEXT
    // =====================================================

    if (worldContext == null)
    {
        worldContext =
            GetComponent<SparkAIWorldContext>();

        if (worldContext == null)
        {
            worldContext =
                FindFirstObjectByType<SparkAIWorldContext>();
        }
    }

    if (worldContext == null)
    {
        return
            "Spark AI World Context is unavailable. " +
            "Add SparkAIWorldContext to the persistent " +
            "AI_TutorialSystem GameObject.";
    }

    // =====================================================
    // 2. BUILD AUTHORITATIVE GLOBAL SNAPSHOT
    // =====================================================

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine(worldContext.BuildSnapshot());

    // =====================================================
    // 3. CONVERSATIONAL COMPONENT MEMORY
    // =====================================================

    SparkShelfItem rememberedItem =
        GetRememberedShelfItem();

    SparkResistor rememberedResistor =
        GetRememberedResistor();

    sb.AppendLine();
    sb.AppendLine("CURRENT CONVERSATION CONTEXT");

    if (rememberedComponent == null)
    {
        sb.AppendLine(
            "No component is currently selected " +
            "in AI memory.");
    }
    else
    {
        sb.AppendLine(
            "Remembered object: " +
            rememberedComponent.name);

        if (rememberedItem != null)
        {
            sb.AppendLine(
                "Shelf item ID: " +
                rememberedItem.ItemId);

            bool registered =
                rememberedItem.IsAtWorkbench &&
                rememberedItem.CurrentSlot != null &&
                rememberedItem.CurrentSlot.Occupant ==
                    rememberedItem;

            sb.AppendLine(
                "Registered on workstation: " +
                registered);

            if (registered)
            {
                sb.AppendLine(
                    "Current slot: " +
                    rememberedItem.CurrentSlot.SlotId);
            }
        }

        if (rememberedResistor != null)
        {
            sb.AppendLine(
                "Remembered component type: Resistor");

            sb.AppendLine(
                "Resistance: " +
                rememberedResistor.ResistanceOhms
                    .ToString("F2") +
                " ohms");

            bool canUseCircuitReadings =
                rememberedItem != null &&
                rememberedItem.IsAtWorkbench &&
                rememberedItem.CurrentSlot != null &&
                rememberedItem.CurrentSlot.Occupant ==
                    rememberedItem;

            if (canUseCircuitReadings)
            {
                sb.AppendLine(
                    "Solver voltage: " +
                    rememberedResistor.Voltage
                        .ToString("F4") + " V");

                sb.AppendLine(
                    "Solver current: " +
                    (rememberedResistor.Current * 1000f)
                        .ToString("F3") + " mA");

                sb.AppendLine(
                    "Solver power: " +
                    (rememberedResistor.PowerWatts * 1000f)
                        .ToString("F3") + " mW");

                sb.AppendLine(
                    "Conducting: " +
                    rememberedResistor.IsConducting);

                sb.AppendLine(
                    "Burned: " +
                    rememberedResistor.IsBurned);
            }
            else
            {
                sb.AppendLine(
                    "The remembered resistor is not " +
                    "verified as installed. Do not " +
                    "attribute circuit current to it.");
            }
        }
    }

    // =====================================================
    // 4. GLOBAL REASONING RULES
    // =====================================================

    sb.AppendLine();
    sb.AppendLine("SPARK AI REASONING RULES");

    sb.AppendLine(
        "Use the active level definition for " +
        "objective requirements.");

    sb.AppendLine(
        "Use the level manager for progression " +
        "and official completion status.");

    sb.AppendLine(
        "Use solved electrical component state " +
        "for individual measurements.");

    sb.AppendLine(
        "Never assume the remembered component " +
        "is the objective-bound component.");

    sb.AppendLine(
        "Never infer a specific component binding " +
        "from its name or workstation slot.");

    sb.AppendLine(
        "Report unknown or unresolved measurements " +
        "instead of inventing values.");

    return sb.ToString();
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


private static bool IsAskingCurrentLevel(string question)
{
    if (string.IsNullOrWhiteSpace(question))
        return false;

    string q = question.Trim().ToLowerInvariant();

    // Normalize punctuation and extra spaces.
    q = q.Replace("?", " ")
         .Replace("!", " ")
         .Replace(".", " ")
         .Replace(",", " ")
         .Replace("'", " ");

    q = string.Join(
        " ",
        q.Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries));

    // Avoid confusing a progression report request
    // with a request for the current level.
    if (ContainsAny(q,
        "next level",
        "previous level",
        "all levels",
        "level progression",
        "unlocked levels",
        "locked levels",
        "how many levels"))
    {
        return false;
    }

    bool mentionsLevel = q.Contains("level");

    bool asksCurrent =
        ContainsAny(q,
            "what level",
            "which level",
            "current level",
            "level am i",
            "level i am",
            "level im",
            "level i'm",
            "level are we",
            "level is this",
            "level now",
            "my level",
            "playing now",
            "currently playing");

    return mentionsLevel && asksCurrent;
}

       
public string LocalAnswer(string question)
{
    Resolve();

    if (string.IsNullOrWhiteSpace(question))
        return "Ask Spark a question about your circuit.";

        
string diagnosticQuestion =
    question.Trim().ToLowerInvariant();

if (ContainsAny(
    diagnosticQuestion,
    "troubleshoot",
    "diagnose my circuit",
    "diagnose circuit",
    "why is my circuit not working",
    "why is this circuit not working",
    "why haven't i won",
    "why have i not won",
    "why is the led glowing but",
    "what is blocking completion",
    "what should i fix next"))
{
    return SparkAILiveTroubleshooter.Diagnose(
        levelManager);
}


        if (localBrain.TryAnswer(
        question,
        levelManager,
        out string advancedAnswer))
{
    return advancedAnswer;
}

    
string q = question.Trim().ToLowerInvariant();


if (IsAskingCurrentLevel(q))
{
    return ExplainCurrentLevelBriefly();
}


if (ContainsAny(q,
    "what level am i on",
    "which level am i on",
    "what is my current level",
    "which level is this",
    "current level name"))
{
    return ExplainCurrentLevelBriefly();
}

if (ContainsAny(q,
    "show level progression",
    "level progression",
    "show all levels",
    "which levels are unlocked",
    "locked levels",
    "can i play the next level",
    "what is the next level",
    "how many levels"))
{
    return SparkAIProgressionReasoner.Explain(
        levelManager);
}


if (ContainsAny(q,
    "next level",
    "level progression",
    "which level is unlocked",
    "which levels are unlocked",
    "locked levels",
    "what level am i on",
    "how many levels",
    "can i play the next level"))
{
    return SparkAIProgressionReasoner.Explain(
        levelManager);
}


if (ContainsAny(q,
    "audit bindings",
    "check runtime bindings",
    "binding audit",
    "wrong component binding",
    "stale binding",
    "why is my component not detected",
    "why is the resistor not detected"))
{
    return SparkAIRuntimeBindingAudit.BuildReport(
        levelManager);
}



if (ContainsAny(q,
    "diagnose level",
    "analyze level",
    "analyze my circuit",
    "why is the level not complete",
    "why is my level incomplete",
    "why is the objective failing",
    "why is safe current failing",
    "diagnose current objective",
    "current objective details",
    "measure safe current",
    "exact current",
    "bound current",
    "diagnose target",
    "show objective evidence"))
{
    return SparkAIObjectiveReasoner.Diagnose(
        levelManager);
}

if (ContainsAny(q,
    "diagnose pending",
    "explain pending objectives",
    "why are targets pending"))
{
    return SparkAIObjectiveReasoner.Diagnose(
        levelManager,
        pendingOnly: true);
}


if (ContainsAny(q,
    "give me a hint",
    "i need a hint",
    "help me solve",
    "what should i check next",
    "next hint"))
{
    return BuildGlobalObjectiveHint(2);
}



if (ContainsAny(q,
    "diagnose objective",
    "diagnose target",
    "why is this objective failing",
    "why is the objective failing",
    "current objective",
    "voltage objective",
    "power objective",
    "safe current",
    "current limit",
    "current requirement",
    "voltage requirement",
    "power requirement",
    "which resistor",
    "recommend resistor",
    "suggest resistor",
    "how to reduce current",
    "how to increase current",
    "which component should i use",
    "what should i change"))
{
    return ExplainActiveLevelTargetsGlobal();
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

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        SetAnswer(
            "CIRCUIT STATUS",
            "Start a level to inspect and diagnose its circuit.");

        return;
    }

    // Read-only diagnosis based on:
    // - Active level
    // - Authoritative evaluator results
    // - Runtime scene bindings
    // - Solved component electrical states
    string diagnosis =
        SparkAILiveTroubleshooter.Diagnose(
            levelManager);

    SetAnswer(
        "SPARK AI · LIVE CIRCUIT",
        diagnosis);
}


       
public void InspectUnderCursor()
{
    Resolve();

    if (sceneCamera == null)
    {
        SetAnswer(
            "INSPECT",
            "Assign the scene camera in the Inspector.");

        return;
    }

    GameObject target = null;

    bool pointerOverUI =
        EventSystem.current != null &&
        EventSystem.current.IsPointerOverGameObject();

    // If INSPECT was triggered while looking at
    // the 3D world, use the object under the cursor.
    if (!pointerOverUI)
    {
        Ray ray = sceneCamera.ScreenPointToRay(
            Input.mousePosition);

        TryFindInspectable(ray, out target);
    }

    // Clicking the UI button moves the mouse away
    // from the world object. Use our last valid hover.
    if (target == null)
        target = lastHoveredInspectable;

    if (target == null)
    {
        SetAnswer(
            "INSPECT",
            "Point at an electrical component, " +
            "then click INSPECT.");

        return;
    }

    // Ensure the remembered object tracks the
    // selected component for later questions.
    RememberComponent(target);

    string explanation =
        BuildUniversalComponentInspection(target);

    SetAnswer(
        "COMPONENT INSPECTOR",
        explanation);
}

private string BuildUniversalComponentInspection(
    GameObject target)
{
    if (target == null)
    {
        return "The selected object is unavailable.";
    }

   
SparkResistor resistor =
    target.GetComponentInParent<SparkResistor>();

if (resistor != null)
{
    System.Text.StringBuilder sb =
        new System.Text.StringBuilder(512);

    sb.AppendLine("RESISTOR: " + resistor.name);
    sb.AppendLine();

    // =========================================
    // ELECTRICAL MEASUREMENTS
    // =========================================

    sb.AppendLine("ELECTRICAL MEASUREMENTS");

    sb.AppendLine(
        "Resistance: " +
        resistor.ResistanceOhms.ToString("F2") +
        " Ω");

    sb.AppendLine(
        "Voltage: " +
        resistor.Voltage.ToString("F3") +
        " V");

    sb.AppendLine(
        "Current: " +
        (resistor.Current * 1000f).ToString("F3") +
        " mA");

    sb.AppendLine(
        "Power: " +
        (resistor.PowerWatts * 1000f).ToString("F3") +
        " mW");

    sb.AppendLine(
        "Maximum rated power: " +
        (resistor.MaximumPowerWatts * 1000f)
            .ToString("F2") +
        " mW");

    sb.AppendLine(
        "Conducting: " + resistor.IsConducting);

    // =========================================
    // BURN SYSTEM
    // =========================================

    SparkResistorBurn burn = resistor.BurnSystem;

    if (burn != null)
    {
        sb.AppendLine();
        sb.AppendLine("THERMAL / BURN STATUS");

        sb.AppendLine(
            "Temperature: " +
            burn.TemperatureC.ToString("F1") +
            " °C");

        sb.AppendLine(
            "Thermal power: " +
            (burn.PowerWatts * 1000f).ToString("F2") +
            " mW");

        sb.AppendLine(
            "Burn rating: " +
            (burn.RatedWatts * 1000f).ToString("F2") +
            " mW");

        sb.AppendLine(
            "Thermal state: " + burn.State);

        sb.AppendLine(
            "Mesh damage: " +
            (burn.MeshBurnProgress * 100f)
                .ToString("F0") +
            "%");

        if (burn.IsFailed)
        {
            sb.AppendLine();
            sb.AppendLine("DIAGNOSIS");

            sb.AppendLine(
                "The resistor has burned and failed.");

            if (burn.ElectricalFailureMode ==
                SparkResistorBurn.FailureMode.OpenCircuit)
            {
                sb.AppendLine(
                    "Failure mode: Open circuit.");

                sb.AppendLine(
                    "Current can no longer pass " +
                    "through this resistor.");
            }
            else
            {
                sb.AppendLine(
                    "Short-circuit failure is configured.");

                sb.AppendLine(
                    "Verify that the electrical solver " +
                    "supports this failure mode.");
            }

            sb.AppendLine(
                "NEXT: Replace or repair the resistor.");
        }
        else
        {
            if (burn.PowerWatts > burn.RatedWatts)
            {
                float overload =
                    burn.PowerWatts /
                    Mathf.Max(burn.RatedWatts, 0.000001f);

                sb.AppendLine(
                    "Power overload: " +
                    overload.ToString("F2") +
                    "x rating");
            }

            if (burn.State ==
                SparkResistorBurn.BurnState.Smoking)
            {
                sb.AppendLine();
                sb.AppendLine(
                    "WARNING: The resistor is smoking.");

                sb.AppendLine(
                    "NEXT: Reduce dissipated power " +
                    "before thermal failure.");
            }
            else if (burn.State ==
                     SparkResistorBurn.BurnState.Hot)
            {
                sb.AppendLine();
                sb.AppendLine(
                    "WARNING: The resistor is overheating.");

                sb.AppendLine(
                    "NEXT: Check its current, " +
                    "resistance and power rating.");
            }
            else
            {
                sb.AppendLine(
                    "No thermal failure detected.");
            }
        }
    }
    else
    {
        sb.AppendLine();
        sb.AppendLine(
            "Thermal monitoring: Unavailable");
    }

    // =========================================
    // SHELF INSTALLATION STATUS
    // =========================================

    SparkShelfItem shelfItem =
        resistor.GetComponentInParent<SparkShelfItem>();

    if (shelfItem != null)
    {
        bool installed =
            shelfItem.IsAtWorkbench &&
            shelfItem.CurrentSlot != null &&
            shelfItem.CurrentSlot.Occupant ==
                shelfItem;

        sb.AppendLine();
        sb.AppendLine("PLACEMENT");

        sb.AppendLine(
            "Installed: " + installed);

        if (!installed)
        {
            sb.AppendLine(
                "NOTE: This resistor is not installed " +
                "at the workbench.");

            sb.AppendLine(
                "Its electrical readings should not " +
                "be treated as active circuit current.");
        }
    }

    return sb.ToString();
}


    SparkLED led =
        target.GetComponentInParent<SparkLED>();

    if (led != null)
    {
        SparkElectricalState state =
            led.ElectricalState;

        return
            "LED: " + led.name + "\n\n" +
            "Light on: " + led.IsOn + "\n" +
            "Electrical enabled: " +
            led.ElectricalEnabled + "\n" +
            "Conducting: " +
            (state.Conduction ==
                SparkConductionState.Conducting) +
            "\nVoltage: " +
            state.Voltage.ToString("F3") +
            " V\nCurrent: " +
            (state.Current * 1000f).ToString("F3") +
            " mA\nPower: " +
            (state.Power * 1000f).ToString("F3") +
            " mW\n\n" +
            "The level evaluator determines whether " +
            "the LED objective has passed.";
    }

    SparkSwitch sparkSwitch =
        target.GetComponentInParent<SparkSwitch>();

    if (sparkSwitch != null)
    {
        return
            "SWITCH: " + sparkSwitch.name + "\n\n" +
            "Conducting: " +
            sparkSwitch.IsConducting + "\n" +
            "Input terminal: " +
            (sparkSwitch.InputTerminal != null
                ? sparkSwitch.InputTerminal.name
                : "Unassigned") +
            "\nOutput terminal: " +
            (sparkSwitch.OutputTerminal != null
                ? sparkSwitch.OutputTerminal.name
                : "Unassigned") +
            "\n\n" +
            (sparkSwitch.IsConducting
                ? "The switch currently permits " +
                  "conduction according to its state."
                : "The switch is not currently " +
                  "conducting.");
    }

    SparkPowerSupply supply =
        target.GetComponentInParent<SparkPowerSupply>();

    if (supply != null)
    {
        SparkElectricalState state =
            supply.ElectricalState;

        return
            "POWER SUPPLY: " + supply.name + "\n\n" +
            "Electrical enabled: " +
            supply.ElectricalEnabled + "\n" +
            "Output active: " +
            supply.IsOutputActive + "\n" +
            "Overloaded: " +
            supply.IsOverloaded + "\n" +
            "Solved voltage: " +
            state.Voltage.ToString("F3") +
            " V\nSolved current: " +
            (state.Current * 1000f).ToString("F3") +
            " mA\n\n" +
            "These are solved component readings, " +
            "not assumed nominal supply settings.";
    }

    SparkElectricalComponent component =
        target.GetComponentInParent<
            SparkElectricalComponent>();

    if (component != null)
    {
        SparkElectricalState state =
            component.ElectricalState;

        return
            "ELECTRICAL COMPONENT: " +
            component.name + "\n\n" +
            "Type: " +
            component.GetType().Name + "\n" +
            "Electrical enabled: " +
            component.ElectricalEnabled + "\n" +
            "Conducting: " +
            (state.Conduction ==
                SparkConductionState.Conducting) +
            "\nVoltage: " +
            state.Voltage.ToString("F3") +
            " V\nCurrent: " +
            (state.Current * 1000f).ToString("F3") +
            " mA\nPower: " +
            (state.Power * 1000f).ToString("F3") +
            " mW";
    }

    return
        "This object has no supported " +
        "electrical component.";
}



private string FormatCompactAnswer(string source)
{
    if (string.IsNullOrWhiteSpace(source))
        return string.Empty;

    // If the TextMeshPro reference is missing,
    // preserve the original answer.
    if (answer == null)
        return source;

    string[] lines = source
        .Replace("\r\n", "\n")
        .Replace('\r', '\n')
        .Split('\n');

    System.Text.StringBuilder output =
        new System.Text.StringBuilder(source.Length);

    string pendingValue = null;

    // Use the real width of the existing
    // Inspector-created explanation text.
    float usableWidth =
        answer.rectTransform.rect.width -
        answer.margin.x -
        answer.margin.z -
        8f;

    // If layout has not established a valid
    // width, keep the original formatting.
    if (usableWidth <= 40f)
        return source;

    void FlushPending()
    {
        if (pendingValue == null)
            return;

        output.AppendLine(pendingValue);
        pendingValue = null;
    }

    foreach (string rawLine in lines)
    {
        string line = rawLine.Trim();

        if (line.Length == 0)
        {
            FlushPending();

            if (output.Length > 0)
                output.AppendLine();

            continue;
        }

        // Section titles remain on their own line.
        bool isHeading =
            line.Length <= 50 &&
            !line.Contains(":") &&
            line == line.ToUpperInvariant();

        // Only combine short key:value entries.
        // Longer advice and explanations remain
        // separate to preserve readability.
        bool isCompactValue =
            line.Contains(":") &&
            line.Length <= 48 &&
            !line.StartsWith(
                "NEXT:",
                StringComparison.OrdinalIgnoreCase) &&
            !line.StartsWith(
                "WARNING:",
                StringComparison.OrdinalIgnoreCase) &&
            !line.StartsWith(
                "FAILED REQUIREMENT:",
                StringComparison.OrdinalIgnoreCase);

        if (isHeading || !isCompactValue)
        {
            FlushPending();
            output.AppendLine(line);
            continue;
        }

        if (pendingValue == null)
        {
            pendingValue = line;
            continue;
        }

        string combined =
            pendingValue + "   |   " + line;

        // Measure the proposed line using the
        // actual TextMeshPro font and font size.
        float preferredWidth =
            answer.GetPreferredValues(
                combined,
                Mathf.Infinity,
                Mathf.Infinity).x;

        if (preferredWidth <= usableWidth)
        {
            output.AppendLine(combined);
            pendingValue = null;
        }
        else
        {
            FlushPending();
            pendingValue = line;
        }
    }

    FlushPending();

    return output.ToString().TrimEnd();
}

        

public void GiveHint()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        SetAnswer(
            "SPARK AI · HINT",
            "Start a level to receive circuit-specific hints.");

        return;
    }

    string currentSignature =
        GetStateSignature();

    // If the player changed the circuit, begin a
    // fresh hint sequence using the new evidence.
    if (currentSignature != lastStateSignature)
    {
        lastStateSignature = currentSignature;
        hintStage = 0;
    }

    hintStage = Mathf.Clamp(
        hintStage + 1,
        1,
        3);

    string response =
        BuildGlobalObjectiveHint(hintStage);

    SetAnswer(
        "SPARK AI · HINT " + hintStage + "/3",
        response);
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
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "no-active-level";
    }

    var manager = levelManager;

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder(512);

    // ==================================================
    // 1. ACTIVE LEVEL AND GAMEPLAY STATE
    // ==================================================

    sb.Append(manager.ActiveLevel.LevelId);
    sb.Append('|').Append(manager.RuntimeState);
    sb.Append('|').Append(manager.HasValidPowerSource);
    sb.Append('|').Append(manager.ClosedReturn);
    sb.Append('|').Append(manager.SourceShorted);
    sb.Append('|').Append(manager.TargetShorted);
    sb.Append('|').Append(manager.Overloaded);
    sb.Append('|').Append(manager.WrongConnection);

    sb.Append('|').Append(manager.SatisfiedTargetCount);
    sb.Append('/').Append(manager.ActiveTargetCount);

    // ==================================================
    // 2. ACTIVE RUNTIME LAYOUT IDENTITY
    // ==================================================

    var bindings = manager.SceneBindings;

    if (bindings != null)
    {
        sb.Append("|layout=");
        sb.Append(bindings.GetInstanceID());
    }

    // ==================================================
    // 3. AUTHORITATIVE OBJECTIVE RESULTS
    // ==================================================

    if (manager.TargetResults != null)
    {
        foreach (var result in manager.TargetResults)
        {
            sb.Append("|target=");
            sb.Append(result.targetId);
            sb.Append(':');
            sb.Append(result.satisfied ? '1' : '0');
        }
    }

    // ==================================================
    // 4. SOLVED OBJECTIVE-BOUND COMPONENTS
    // ==================================================

    if (bindings != null &&
        manager.ActiveLevel.Targets != null)
    {
        foreach (SparkLevelTarget target
                 in manager.ActiveLevel.Targets)
        {
            if (target == null ||
                string.IsNullOrWhiteSpace(
                    target.TargetComponentId))
            {
                continue;
            }

            sb.Append("|binding=");
            sb.Append(target.TargetId);
            sb.Append(':');
            sb.Append(target.TargetComponentId);

            if (!bindings.TryGetComponent(
                    target.TargetComponentId,
                    out var component) ||
                component == null)
            {
                sb.Append(":unresolved");
                continue;
            }

            var state = component.ElectricalState;

            sb.Append(":object=");
            sb.Append(component.GetInstanceID());

            sb.Append(":enabled=");
            sb.Append(component.ElectricalEnabled);

            sb.Append(":conduction=");
            sb.Append(state.Conduction);

            // Rounded signatures avoid resetting hints
            // for insignificant solver fluctuations.
            sb.Append(":v=");
            sb.Append(
                state.Voltage.ToString(
                    "F3",
                    System.Globalization.CultureInfo
                        .InvariantCulture));

            sb.Append(":i=");
            sb.Append(
                state.Current.ToString(
                    "F5",
                    System.Globalization.CultureInfo
                        .InvariantCulture));

            sb.Append(":p=");
            sb.Append(
                state.Power.ToString(
                    "F4",
                    System.Globalization.CultureInfo
                        .InvariantCulture));
        }
    }

    // ==================================================
    // 5. INSTALLED SHELF COMPONENTS
    // ==================================================

    if (worldContext != null)
    {
        SparkShelfItem[] installed =
            worldContext.GetPlacedComponents();

        System.Collections.Generic.List<string> items =
            new System.Collections.Generic.List<string>();

        foreach (SparkShelfItem item in installed)
        {
            if (item == null)
                continue;

            if (!item.IsAtWorkbench ||
                item.CurrentSlot == null ||
                item.CurrentSlot.Occupant != item)
            {
                continue;
            }

            System.Text.StringBuilder itemKey =
                new System.Text.StringBuilder();

            itemKey.Append(item.ItemId);
            itemKey.Append('@');
            itemKey.Append(item.CurrentSlot.SlotId);
            itemKey.Append('#');
            itemKey.Append(item.GetInstanceID());

            SparkResistor resistor =
                item.GetComponent<SparkResistor>();

            if (resistor == null)
            {
                resistor =
                    item.GetComponentInChildren<
                        SparkResistor>(true);
            }

            if (resistor != null)
            {
                itemKey.Append(":r=");
                itemKey.Append(
                    resistor.ResistanceOhms.ToString(
                        "F2",
                        System.Globalization.CultureInfo
                            .InvariantCulture));

                itemKey.Append(":burned=");
                itemKey.Append(resistor.IsBurned);

                itemKey.Append(":conducting=");
                itemKey.Append(resistor.IsConducting);
            }

            items.Add(itemKey.ToString());
        }

        // FindObjectsByType does not guarantee order.
        // Sorting prevents false changes when discovery
        // returns the same items in a different order.
        items.Sort(System.StringComparer.Ordinal);

        foreach (string itemKey in items)
        {
            sb.Append("|shelf=");
            sb.Append(itemKey);
        }
    }

    return sb.ToString();
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
    if (canvas == null)
    {
        Debug.LogWarning(
            "[Ask Spark] Assign Canvas_AskSpark " +
            "in the Inspector.",
            this);

        return;
    }

    // Show the existing UI.
    // Do not create any runtime UI objects.
    if (!canvas.gameObject.activeSelf)
    {
        canvas.gameObject.SetActive(true);
    }

    // Update the existing title.
    if (heading != null)
    {
        heading.text = title ?? string.Empty;
    }

    // Update the current level and objective status.
    RefreshStatus();

    // Ensure existing RectTransforms have their
    // correct dimensions before measuring text.
    Canvas.ForceUpdateCanvases();

    // Format and display the explanation.
    if (answer != null)
    {
        answer.textWrappingMode =
            TextWrappingModes.Normal;

        // Combine short values horizontally only
        // when they fit inside the available width.
        answer.text = FormatCompactAnswer(text);

        // Refresh TextMeshPro's existing text geometry.
        answer.ForceMeshUpdate();
    }

    // Update the existing ScrollRect and move
    // to the beginning of the new explanation.
    if (answerScroll != null)
    {
        answerScroll.StopMovement();

        Canvas.ForceUpdateCanvases();

        answerScroll.verticalNormalizedPosition = 1f;
    }

    // Optional diagnostic logging.
    if (logActions)
    {
        Debug.Log(
            "[AskSpark] " +
            (title ?? "ANSWER") +
            ": " +
            (text ?? string.Empty),
            this);
    }

    // Optional voice output.
    if (speakReplies &&
        voice != null &&
        !string.IsNullOrWhiteSpace(text))
    {
        voice.SpeakText(text);
    }
}


private static string BuildResistorBurnDiagnosis(
    SparkResistor resistor)
{
    if (resistor == null)
        return "No resistor selected.";

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine("RESISTOR DIAGNOSTICS");

    sb.AppendLine(
        "Resistance: " +
        resistor.ResistanceOhms.ToString("F1") +
        " Ω");

    sb.AppendLine(
        "Voltage: " +
        Mathf.Abs(resistor.Voltage).ToString("F3") +
        " V");

    sb.AppendLine(
        "Current: " +
        (Mathf.Abs(resistor.Current) * 1000f)
            .ToString("F3") +
        " mA");

    SparkResistorBurn burn = resistor.BurnSystem;

    if (burn == null)
    {
        sb.AppendLine(
            "Thermal monitoring: Unavailable");

        return sb.ToString();
    }

    sb.AppendLine(
        "Temperature: " +
        burn.TemperatureC.ToString("F1") +
        " °C");

    sb.AppendLine(
        "Power: " +
        (burn.PowerWatts * 1000f).ToString("F2") +
        " mW");

    sb.AppendLine(
        "Rating: " +
        (burn.RatedWatts * 1000f).ToString("F2") +
        " mW");

    sb.AppendLine(
        "Thermal state: " + burn.State);

    if (burn.IsFailed)
    {
        sb.AppendLine(
            "Mesh damage: " +
            (burn.MeshBurnProgress * 100f)
                .ToString("F0") +
            "%");

        sb.AppendLine(
            "DIAGNOSIS: The resistor has failed.");

        sb.AppendLine(
            burn.ElectricalFailureMode ==
                SparkResistorBurn.FailureMode.OpenCircuit
                ? "The resistor is now an open circuit. " +
                  "Current cannot flow through it."
                : "Short-circuit failure is configured, " +
                  "but verify that the electrical solver " +
                  "supports this failure mode.");

        sb.AppendLine(
            "NEXT: Replace or repair the resistor.");

        return sb.ToString();
    }

    float rating = burn.RatedWatts;

    if (burn.PowerWatts > rating)
    {
        float overload =
            burn.PowerWatts / rating;

        sb.AppendLine(
            "Power overload: " +
            overload.ToString("F2") +
            "x rated power");
    }

    switch (burn.State)
    {
        case SparkResistorBurn.BurnState.Hot:

            sb.AppendLine(
                "DIAGNOSIS: The resistor is heating up.");

            sb.AppendLine(
                "NEXT: Check its power dissipation " +
                "and resistor rating.");
            break;

        case SparkResistorBurn.BurnState.Smoking:

            sb.AppendLine(
                "DIAGNOSIS: The resistor has reached " +
                "the smoking temperature.");

            sb.AppendLine(
                "NEXT: Reduce the electrical load " +
                "before it fails.");
            break;

        default:

            sb.AppendLine(
                "DIAGNOSIS: No thermal failure detected.");
            break;
    }

    return sb.ToString();
}

public void AskFromInput()
{
    if (questionInput == null)
        return;

    Ask(questionInput.text);
}

private void UnbindButtons()
{
    if (inspectButton != null)
        inspectButton.onClick.RemoveListener(InspectUnderCursor);

    if (explainButton != null)
        explainButton.onClick.RemoveListener(ExplainCircuit);

    if (hintButton != null)
        hintButton.onClick.RemoveListener(GiveHint);

    if (tutorialButton != null)
        tutorialButton.onClick.RemoveListener(OpenTutorial);

    if (askButton != null)
        askButton.onClick.RemoveListener(AskFromInput);

    if (closeButton != null)
        closeButton.onClick.RemoveListener(Hide);
}




private string ExplainCurrentLevelBriefly()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "No level is currently active.";
    }

    int number = levelManager.ActiveLevelIndex + 1;
    int total = levelManager.LevelCount;

    string response =
        "You are currently playing Level " +
        number + " of " + total + ".\n\n" +
        levelManager.ActiveLevel.DisplayName +
        "\n\nStatus: " +
        levelManager.RuntimeState +
        "\nObjectives: " +
        levelManager.SatisfiedTargetCount + "/" +
        levelManager.ActiveTargetCount;

    if (levelManager.IsCompleted)
        response += "\n\nThis level is complete.";

    if (levelManager.IsFailed)
        response += "\n\nThis level has failed.";

    return response;
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
        
private string ExplainActiveLevelTargetsGlobal()
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "Start a level to inspect its objectives.";
    }

    var manager = levelManager;

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine("SPARK AI - ACTIVE LEVEL ANALYSIS");
    sb.AppendLine();

    sb.AppendLine(
        "Level: " + manager.ActiveLevel.DisplayName);

    sb.AppendLine(
        "Progress: " +
        manager.SatisfiedTargetCount + "/" +
        manager.ActiveTargetCount);

    sb.AppendLine(
        "State: " + manager.RuntimeState);

    if (manager.ActiveLevel.Targets == null)
    {
        sb.AppendLine("No target definitions are available.");
        return sb.ToString();
    }

    foreach (SparkLevelTarget target
             in manager.ActiveLevel.Targets)
    {
        if (target == null)
            continue;

        bool? passed = null;

        if (manager.TargetResults != null)
        {
            foreach (var result in manager.TargetResults)
            {
                if (result.targetId == target.TargetId)
                {
                    passed = result.satisfied;
                    break;
                }
            }
        }

        sb.AppendLine();

        sb.AppendLine(
            (passed.HasValue
                ? passed.Value ? "[PASS] " : "[PENDING] "
                : "[UNKNOWN] ") +
            (string.IsNullOrWhiteSpace(target.DisplayName)
                ? target.TargetId
                : target.DisplayName));

        sb.AppendLine("Type: " + target.Type);

        if (!string.IsNullOrWhiteSpace(
                target.Description))
        {
            sb.AppendLine(
                "Goal: " + target.Description);
        }

        switch (target.Type)
        {
            case SparkLevelTarget.TargetType.CurrentRange:
                sb.AppendLine(
                    "Minimum current: " +
                    (target.MinimumCurrent * 1000f)
                        .ToString("F3") + " mA");

                if (target.UseMaximumCurrent)
                {
                    sb.AppendLine(
                        "Maximum current: " +
                        (target.MaximumCurrent * 1000f)
                            .ToString("F3") + " mA");
                }
                break;

            case SparkLevelTarget.TargetType.VoltageRange:
                sb.AppendLine(
                    "Minimum voltage: " +
                    target.MinimumVoltage.ToString("F3") +
                    " V");

                if (target.UseMaximumVoltage)
                {
                    sb.AppendLine(
                        "Maximum voltage: " +
                        target.MaximumVoltage.ToString("F3") +
                        " V");
                }
                break;

            case SparkLevelTarget.TargetType.PowerRange:
                sb.AppendLine(
                    "Minimum power: " +
                    (target.MinimumPower * 1000f)
                        .ToString("F3") + " mW");

                if (target.UseMaximumPower)
                {
                    sb.AppendLine(
                        "Maximum power: " +
                        (target.MaximumPower * 1000f)
                            .ToString("F3") + " mW");
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(
                target.TargetComponentId))
        {
            sb.AppendLine(
                "Component binding ID: " +
                target.TargetComponentId);
        }

        if (target.RequireConduction)
        {
            sb.AppendLine(
                "Conduction is required.");
        }

        if (target.RequireElectricalEnabled)
        {
            sb.AppendLine(
                "Electrical enabled state is required.");
        }

        if (target.Inverted)
        {
            sb.AppendLine(
                "This objective uses inverted evaluation.");
        }

        if (!target.IsConfigured())
        {
            sb.AppendLine(
                "Warning: Objective configuration incomplete.");
        }
    }

    sb.AppendLine();
    sb.AppendLine("RUNTIME WORKSTATION COMPONENTS");

    if (worldContext == null)
    {
        sb.AppendLine(
            "Global world context is unavailable.");
    }
    else
    {
        SparkShelfItem[] installed =
            worldContext.GetPlacedComponents();

        sb.AppendLine(
            "Installed items: " + installed.Length);

        foreach (SparkShelfItem item in installed)
        {
            if (item == null)
                continue;

            sb.AppendLine();
            sb.AppendLine(
                "Item: " + item.ItemId);

            if (item.CurrentSlot != null)
            {
                sb.AppendLine(
                    "Slot: " +
                    item.CurrentSlot.SlotId);
            }

            SparkResistor resistor =
                item.GetComponent<SparkResistor>();

            if (resistor == null)
            {
                resistor =
                    item.GetComponentInChildren<SparkResistor>(true);
            }

            if (resistor != null)
            {
                sb.AppendLine(
                    "Resistance: " +
                    resistor.ResistanceOhms.ToString("F1") +
                    " ohms");

                sb.AppendLine(
                    "Solver current: " +
                    (Mathf.Abs(resistor.Current) * 1000f)
                        .ToString("F3") + " mA");

                sb.AppendLine(
                    "Solver voltage: " +
                    Mathf.Abs(resistor.Voltage)
                        .ToString("F3") + " V");

                sb.AppendLine(
                    "Conducting: " +
                    resistor.IsConducting);
            }
        }
    }

    sb.AppendLine();
    sb.AppendLine(
        "The evaluator determines objective results. " +
        "Installed components are listed independently; " +
        "their readings are not automatically bound " +
        "to a particular objective.");

    return sb.ToString();
}

private string BuildGlobalObjectiveHint(int stage)
{
    Resolve();

    if (levelManager == null ||
        levelManager.ActiveLevel == null)
    {
        return "Start a level to receive circuit-specific hints.";
    }

    var manager = levelManager;

    if (manager.IsCompleted)
    {
        return
            "The evaluator confirms this level is complete. " +
            "All required objectives have been satisfied.";
    }

    System.Text.StringBuilder sb =
        new System.Text.StringBuilder();

    sb.AppendLine(
        "SPARK AI - LEVEL " +
        manager.ActiveLevel.DisplayName);

    sb.AppendLine(
        "Progress: " +
        manager.SatisfiedTargetCount + "/" +
        manager.ActiveTargetCount);

    sb.AppendLine();

    // =====================================================
    // 1. CRITICAL CIRCUIT CONDITIONS COME FIRST
    // =====================================================

    if (manager.IsFailed)
    {
        sb.AppendLine("LEVEL FAILED");

        sb.AppendLine(
            string.IsNullOrWhiteSpace(manager.LastFailureReason)
                ? "The manager reports level failure."
                : manager.LastFailureReason);

        return sb.ToString();
    }

    if (manager.SourceShorted || manager.TargetShorted)
    {
        sb.AppendLine("SHORT CIRCUIT DETECTED");

        sb.AppendLine(
            stage == 1
                ? "Check the circuit connections."
                : stage == 2
                    ? "Look for wires that bypass an " +
                      "electrical component."
                    : "The evaluator reports a short. " +
                      "Trace the connections between the " +
                      "power source, components, and return path.");

        return sb.ToString();
    }

    if (manager.Overloaded)
    {
        sb.AppendLine("OVERLOAD DETECTED");

        sb.AppendLine(
            stage == 1
                ? "Check the circuit's electrical load."
                : stage == 2
                    ? "Inspect component power ratings " +
                      "and the current through them."
                    : "The evaluator reports an overload. " +
                      "Compare component solver readings " +
                      "against their operating limits.");

        return sb.ToString();
    }

    if (!manager.HasValidPowerSource)
    {
        sb.AppendLine("POWER SOURCE NOT VALID");

        sb.AppendLine(
            stage == 1
                ? "Begin by checking the power supply."
                : stage == 2
                    ? "Check the source terminals and " +
                      "the connections leading to the circuit."
                    : "The level manager does not currently " +
                      "recognize a valid active power source. " +
                      "Inspect the source configuration and wiring.");

        return sb.ToString();
    }

    if (!manager.ClosedReturn)
    {
        sb.AppendLine("RETURN PATH OPEN");

        sb.AppendLine(
            stage == 1
                ? "The circuit needs a complete return path."
                : stage == 2
                    ? "Check the switch and wire endpoints " +
                      "along the return path."
                    : "The evaluator reports an open return " +
                      "path. Trace the circuit from source " +
                      "through the components and back.");

        return sb.ToString();
    }

    if (manager.WrongConnection)
    {
        sb.AppendLine("CONNECTION ISSUE DETECTED");

        sb.AppendLine(
            stage == 1
                ? "Review the component connections."
                : stage == 2
                    ? "Check whether wires are connected " +
                      "to the intended terminals."
                    : "The evaluator reports a wrong " +
                      "connection. Inspect terminal endpoints " +
                      "and any required polarity.");

        return sb.ToString();
    }

    // =====================================================
    // 2. FIND THE FIRST EVALUATOR-CONFIRMED
    //    PENDING OBJECTIVE
    // =====================================================

    SparkLevelTarget pendingTarget = null;

    if (manager.ActiveLevel.Targets != null &&
        manager.TargetResults != null)
    {
        foreach (SparkLevelTarget target
                 in manager.ActiveLevel.Targets)
        {
            if (target == null)
                continue;

            foreach (var result in manager.TargetResults)
            {
                if (result.targetId != target.TargetId)
                    continue;

                if (!result.satisfied)
                {
                    pendingTarget = target;
                }

                break;
            }

            if (pendingTarget != null)
                break;
        }
    }

    if (pendingTarget == null)
    {
        sb.AppendLine(
            "No pending objective could be matched " +
            "to the current evaluator results.");

        sb.AppendLine(
            "The manager still controls completion. " +
            "Check the current level state.");

        return sb.ToString();
    }

    string name =
        string.IsNullOrWhiteSpace(pendingTarget.DisplayName)
            ? pendingTarget.TargetId
            : pendingTarget.DisplayName;

    sb.AppendLine("PENDING OBJECTIVE: " + name);

    // =====================================================
    // HINT 1: GOAL
    // =====================================================

    if (stage <= 1)
    {
        if (!string.IsNullOrWhiteSpace(
                pendingTarget.Description))
        {
            sb.AppendLine(pendingTarget.Description);
        }
        else
        {
            sb.AppendLine(
                "This objective has not passed yet. " +
                "Inspect its required electrical condition.");
        }

        sb.AppendLine();
        sb.AppendLine(
            "Press HINT again for its exact requirements.");

        return sb.ToString();
    }

    // =====================================================
    // HINT 2: CONFIGURED REQUIREMENTS
    // =====================================================

    sb.AppendLine(
        "Objective type: " + pendingTarget.Type);

    switch (pendingTarget.Type)
    {
        case SparkLevelTarget.TargetType.CurrentRange:

            sb.AppendLine(
                "Required current: at least " +
                (pendingTarget.MinimumCurrent * 1000f)
                    .ToString("F3") + " mA");

            if (pendingTarget.UseMaximumCurrent)
            {
                sb.AppendLine(
                    "Maximum allowed: " +
                    (pendingTarget.MaximumCurrent * 1000f)
                        .ToString("F3") + " mA");
            }

            sb.AppendLine(
                "Check the current at the component " +
                "used by this objective.");
            break;

        case SparkLevelTarget.TargetType.VoltageRange:

            sb.AppendLine(
                "Required voltage: at least " +
                pendingTarget.MinimumVoltage
                    .ToString("F3") + " V");

            if (pendingTarget.UseMaximumVoltage)
            {
                sb.AppendLine(
                    "Maximum allowed: " +
                    pendingTarget.MaximumVoltage
                        .ToString("F3") + " V");
            }

            sb.AppendLine(
                "Check the voltage measurement " +
                "associated with this objective.");
            break;

        case SparkLevelTarget.TargetType.PowerRange:

            sb.AppendLine(
                "Required power: at least " +
                (pendingTarget.MinimumPower * 1000f)
                    .ToString("F3") + " mW");

            if (pendingTarget.UseMaximumPower)
            {
                sb.AppendLine(
                    "Maximum allowed: " +
                    (pendingTarget.MaximumPower * 1000f)
                        .ToString("F3") + " mW");
            }

            sb.AppendLine(
                "Check the power measurement " +
                "associated with this objective.");
            break;

        case SparkLevelTarget.TargetType.LEDOn:

            sb.AppendLine(
                "The evaluator requires the specified " +
                "LED to be on.");

            sb.AppendLine(
                "Check its polarity and electrical path.");
            break;

        case SparkLevelTarget.TargetType.ComponentConducting:

            sb.AppendLine(
                "The specified component must satisfy " +
                "the evaluator's conduction condition.");
            break;

        case SparkLevelTarget.TargetType.ComponentPowered:

            sb.AppendLine(
                "The specified component must satisfy " +
                "the evaluator's powered condition.");
            break;

        case SparkLevelTarget.TargetType.VoltagePresent:

            sb.AppendLine(
                "The specified terminal or component " +
                "must satisfy the voltage-present condition.");
            break;

        case SparkLevelTarget.TargetType.TerminalPowered:

            sb.AppendLine(
                "The specified terminal must satisfy " +
                "the evaluator's powered condition.");
            break;

        default:

            sb.AppendLine(
                "Inspect this objective's configured " +
                "electrical requirements.");
            break;
    }

    if (pendingTarget.RequireConduction)
    {
        sb.AppendLine(
            "Additional requirement: conduction.");
    }

    if (pendingTarget.RequireElectricalEnabled)
    {
        sb.AppendLine(
            "Additional requirement: electrical enabled.");
    }

    if (pendingTarget.Inverted)
    {
        sb.AppendLine(
            "Warning: this objective uses inverted evaluation.");
    }

    if (!pendingTarget.IsConfigured())
    {
        sb.AppendLine(
            "Warning: objective configuration is incomplete.");
    }

    if (stage == 2)
    {
        sb.AppendLine();
        sb.AppendLine(
            "Press HINT again to inspect runtime components.");

        return sb.ToString();
    }

   
// =====================================================
// HINT 3: AUTHORITATIVE RUNTIME EVIDENCE
// =====================================================

sb.AppendLine();
sb.AppendLine("LIVE OBJECTIVE DIAGNOSIS");
sb.AppendLine();

sb.AppendLine(
    SparkAIObjectiveReasoner.Diagnose(
        manager,
        pendingOnly: true));

sb.AppendLine();
sb.AppendLine(
    "Measurements come from the active level's " +
    "runtime scene bindings. Objective pass/fail " +
    "comes from the official level evaluator.");

return sb.ToString();

}




    }
}
