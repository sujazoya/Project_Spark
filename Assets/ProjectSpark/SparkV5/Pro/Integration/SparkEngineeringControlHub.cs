using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// v14 control lease and reviewed-plan executor. Does not create topology or visual wires.
    /// Every physical step is delegated to SparkNativeWireDirector -> the PLAYER's Wire Tool.
    /// Completion remains exclusively under LevelGamePlayManager / the circuit evaluator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringControlHub : MonoBehaviour
    {
        public enum OperatingMode { Manual, Assisted, Auto }
        public enum ExecutionPhase { Idle, Running, Paused, Completed, Failed, Cancelled }

        [Header("Default user ownership")]
        [SerializeField] private OperatingMode startingMode = OperatingMode.Manual;
        [SerializeField] private bool createRuntimePanel = true;
        [Header("Verified native cable sequencing")]
        [SerializeField, Range(20f, 240f)] private float maxSecondsPerCable = 120f;
        [SerializeField] private bool requireVirtualPowerOff = true;
        [Header("Explanations (text always available)")]
        [SerializeField] private bool spokenActionNarration;
        [SerializeField] private SparkAIVoiceController optionalVoiceController;

        private SparkProExecutive executive;
        private SparkNativeWireDirector director;
        private SparkNativeWireCinematic performer;
        private SparkNativeWireAutoEngineer existingRecorder;
        private SparkEngineeringDeviceOperator deviceOperator;
        private SparkNativeWiringPlan[] loadedPlans;
        private Coroutine activeRoutine;
        private SparkCircuitSystem activeCircuit;
        private Transform activeRoot;
        private SparkNativeWiringPlan activePlan;
        private bool cancelRequested;
        private bool pauseRequested;
        private int completedSteps;
        private int totalSteps;
        private string pendingFrom;
        private string pendingTo;
        private string dispatchFrom, dispatchTo;
        private Transform pendingRoot;
        private SparkNativeWiringPlan pendingPlan;
        private string lastMessage = "Ready. Player owns all tools in Manual mode.";
        private string currentAction = "None";
        private OperatingMode mode;
        private ExecutionPhase phase = ExecutionPhase.Idle;
        private readonly List<string> recentMessages = new List<string>(18);
        private const int MaxRecentMessages = 18;
        private static readonly Regex DirectWire = new Regex(
            @"^(?:spark[ ,]+)?(?:physically\s+)?(?:draw|create|connect|route|make)\s+(?:a\s+)?(?:real\s+|native\s+)?wire\s+from\s+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public event Action StateChanged;
        public OperatingMode Mode => mode;
        internal int ModeAcknowledgement { get; private set; }
        internal int WiringOperationRevision { get; private set; }
        public ExecutionPhase Phase => phase;
        public bool IsExecuting => activeRoutine != null;
        public bool IsPaused => pauseRequested;
        public int CompletedSteps => completedSteps;
        public int TotalSteps => totalSteps;
        public string CurrentAction => currentAction;
        public string LastMessage => lastMessage;
        public bool SpokenActionNarration => spokenActionNarration;
        public string PendingStep => string.IsNullOrEmpty(pendingFrom) ? "None" : pendingFrom + " -> " + pendingTo;
        public IReadOnlyList<string> RecentMessages => recentMessages;

        internal bool OwnsApprovedDispatch(string from, string to) =>
            IsExecuting && phase == ExecutionPhase.Running && mode != OperatingMode.Manual && !cancelRequested &&
            string.Equals(dispatchFrom, from, StringComparison.Ordinal) &&
            string.Equals(dispatchTo, to, StringComparison.Ordinal) && ContextMatches(out _);

        private bool DispatchReviewedWire(string from, string to, out string detail)
        {
            dispatchFrom = from;
            dispatchTo = to;
            try { return director.TryStartApprovedStep(from, to, out detail); }
            finally { dispatchFrom = dispatchTo = null; }
        }

        private void Awake()
        {
            executive = GetComponent<SparkProExecutive>();
            director = GetComponent<SparkNativeWireDirector>();
            performer = GetComponent<SparkNativeWireCinematic>();
            existingRecorder = GetComponent<SparkNativeWireAutoEngineer>();
            deviceOperator = GetComponent<SparkEngineeringDeviceOperator>();
            mode = startingMode;
            Reload();
        }

        private void Start()
        {
            if (createRuntimePanel && !GetComponent<SparkEngineeringControlPanel>())
                gameObject.AddComponent<SparkEngineeringControlPanel>();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            if (!director) director = GetComponent<SparkNativeWireDirector>();
            if (director) director.StatusChanged += OnNativeStatus;
            if (performer) performer.StageChanged += OnNativeStage;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            if (director) director.StatusChanged -= OnNativeStatus;
            if (performer) performer.StageChanged -= OnNativeStage;
            CancelInternal("Control hub disabled; unfinished AI work cancelled.", true);
        }

        private void OnSceneChanged(Scene previous, Scene next)
        {
            CancelInternal("Scene changed. AI tools released.", true);
            ClearPending();
            Reload();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            if (IsExecuting || director && director.IsBusy)
                CancelInternal("Android suspended Spark. Unfinished native wiring cancelled.", true);
            if (deviceOperator && (deviceOperator.IsBusy || deviceOperator.Proposal != "None"))
                deviceOperator.Cancel();
            if (PendingStep != "None") ClearPending();
        }
#endif

        private void OnNativeStatus(string information)
        {
            if (IsExecuting && !string.IsNullOrWhiteSpace(information))
                Note("Native tool: " + information, false);
        }

        private void OnNativeStage() => StateChanged?.Invoke();

        private void Note(string message, bool narrate = true)
        {
            lastMessage = message;
            recentMessages.Add(message);
            if (recentMessages.Count > MaxRecentMessages) recentMessages.RemoveAt(0);
            Debug.Log("[Spark Engineering v14] " + message, this);
            StateChanged?.Invoke();
            if (narrate && spokenActionNarration && optionalVoiceController)
                optionalVoiceController.SpeakText(message);
        }

        private void SetPhase(ExecutionPhase value)
        {
            phase = value;
            StateChanged?.Invoke();
        }

        private void ClearPending()
        {
            pendingFrom = null;
            pendingTo = null;
            pendingRoot = null;
            pendingPlan = null;
            StateChanged?.Invoke();
        }

        public string SelectMode(OperatingMode next)
        {
            if (executive && executive.ExamMode && next != OperatingMode.Manual)
                return "Independent examination active: AI action modes are not available.";
            if (next != mode)
            {
                // Changing owner cancels only the AI's unfinished native operation;
                // confirmed connections remain in the actual player wire registry.
                CancelInternal("Tool ownership changed; unfinished Spark action cancelled.", true);
                if (deviceOperator) deviceOperator.Cancel();
                ClearPending();
            }
            mode = next;
            ModeAcknowledgement++;
            if (phase == ExecutionPhase.Cancelled) SetPhase(ExecutionPhase.Idle);
            return Message("Mode: " + next + ". " +
                (next == OperatingMode.Manual ? "Player controls construction; Spark cannot wire on its own." :
                 next == OperatingMode.Assisted ? "Spark proposes one reviewed wire and waits for explicit approval." :
                 "Spark may execute reviewed practice plans; power and exam safety remain mandatory."));
        }

        public string Status()
        {
            return "Engineering mode=" + mode + ", phase=" + phase +
                ", tool owner=" + (activeRoutine != null ? "Spark" : "Player") +
                ", progress=" + completedSteps + "/" + totalSteps +
                ", pending=" + PendingStep + ", action=" + currentAction +
                ". " + lastMessage;
        }

        private string Message(string message)
        {
            Note(message, false); // SparkProExecutive already narrates interactive answers.
            return message;
        }

        public void SetSpokenNarration(bool enabled)
        {
            spokenActionNarration = enabled;
            Note(enabled ? "Spoken action narration enabled where a voice controller is bound." :
                           "Spoken action narration disabled; text narration remains available.", false);
        }

        public string Reload()
        {
            loadedPlans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
            return "Wiring plans refreshed: " + (loadedPlans == null ? 0 : loadedPlans.Length) + " assets found.";
        }

        private bool GetContext(out LevelGamePlayManager manager,
            out SparkAILevelContext level, out string error)
        {
            manager = executive ? executive.ActiveLevelManager : null;
            level = manager ? SparkAILevelContext.Capture(manager) : default;
            error = null;
            if (!executive || !manager || !level.Definition || !level.Layout ||
                !level.Layout.gameObject.activeInHierarchy || !level.Layout.SceneBindings || !manager.CircuitSystem ||
                !manager.LayoutLoader || manager.LayoutLoader.ActiveLayout != level.Layout)
                error = "No active authored level/layout, bindings, or circuit system.";
            else if (executive.ExamMode)
                error = "Independent exam mode forbids AI construction.";
            else if (manager.IsFailed)
                error = "The official evaluator reports level failure.";
            return error == null;
        }

        private static string LayoutKey(string name) =>
            (name ?? string.Empty).Replace("(Clone)", string.Empty).Trim();

        private SparkNativeWiringPlan FindApprovedPlan(SparkAILevelContext level, out string error, bool requireRevision = false)
        {
            error = "No unique approved, unchanged practice plan matches this level/layout.";
            SparkNativeWiringPlan match = null;
            if (loadedPlans == null) Reload();
            for (int i = 0; loadedPlans != null && i < loadedPlans.Length; i++)
            {
                SparkNativeWiringPlan p = loadedPlans[i];
                if (!p || !p.IsApproved || p.LevelNumber != level.Definition.LevelNumber ||
                    !string.Equals(p.LayoutName, LayoutKey(level.Layout.name), StringComparison.Ordinal))
                    continue;
                if (!p.Validate(out string invalid))
                { error = "Approved plan is invalid: " + invalid; return null; }
                if (match) { error = "More than one approved plan matches: remove the ambiguity."; return null; }
                match = p;
            }
            if (match && (requireRevision || match.HasRevisionReview))
            {
                var prefab = executive.ActiveLevelManager.LayoutLoader.ActiveSourcePrefab;
                if (!match.MatchesReviewedLayout(level.Definition, prefab, out error)) return null;
#if UNITY_EDITOR
                if (SparkNativeWiringPlan.AuthoredRevision(level.Definition, prefab, match.SchematicReference) != match.ReviewedRevision)
                { error = "Authored layout or level definition changed after review."; return null; }
#endif
            }
            if (match) error = null;
            return match;
        }

        internal SparkNativeWiringPlan GetNexusConstructionPlan(out string reason)
        {
            return GetContext(out _, out var level, out reason)
                ? FindApprovedPlan(level, out reason, requireRevision: true)
                : null;
        }

        public string DrawCurrentLevel()
        {
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            if (mode == OperatingMode.Manual)
                return Message($"Level {level.Definition.LevelNumber} is in Manual mode. Select Assisted or Auto to authorize Spark construction.");
            return mode == OperatingMode.Assisted ? SuggestNext(requireRevision: true) : StartAuto(requireRevision: true);
        }

        private string ConstructionBlock(SparkAILevelContext level, string reason) =>
            $"Level {level.Definition.LevelNumber} ({level.Definition.DisplayName}) has {level.Definition.TargetCount} authored objectives. " +
            $"Construction blocked: {reason} Use Preview and the Universal Auto Draw review workflow for authored evidence.";

        public string ExplainConstruction()
        {
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            var plan = FindApprovedPlan(level, out reason, requireRevision: true);
            string verdict = manager.HasCurrentElectricalEvaluation
                ? $"Evaluator: {manager.SatisfiedTargetCount}/{level.Definition.TargetCount} satisfied."
                : "Awaiting a current electrical evaluation.";
            if (!plan)
                return Message($"Level {level.Definition.LevelNumber}: {reason} {verdict} " +
                    "Review the authored layout, task requirements, and native tool capabilities before approving construction.");
            return Message($"Level {level.Definition.LevelNumber}: {plan.Wires.Count} reviewed wires. Evidence: " +
                $"{plan.SchematicReference}. {verdict} " + PreviewDetails(plan, manager, level));
        }

        public string ConstructionStatus()
        {
            var manager = executive ? executive.ActiveLevelManager : null;
            var level = manager ? manager.ActiveLevel : null;
            string stage = performer && performer.IsPerforming ? performer.Stage.ToString() : "Idle";
            var layout = manager && manager.LayoutLoader ? manager.LayoutLoader.ActiveLayout : null;
            string power = layout ? (HasActivePower(layout) ? "On" : "Off") : "Unavailable";
            return $"Level {(level ? level.LevelNumber.ToString() : "—")}, {mode}, {phase}, " +
                $"{completedSteps}/{totalSteps} reviewed wires. {currentAction}. Hand: {stage}. " +
                $"Native tool: {(director && director.IsBusy ? "Busy" : "Idle")}. Power: {power}.";
        }

        private bool PowerSafe(SparkAILevelContext level) => !requireVirtualPowerOff || !HasActivePower(level.Layout);

        private static bool HasActivePower(SparkLevelLayout layout)
        {
            SparkPowerSupply[] supplies = layout.GetComponentsInChildren<SparkPowerSupply>(true);
            for (int i = 0; i < supplies.Length; i++)
                if (supplies[i] && supplies[i].IsOutputActive) return true;
            return false;
        }

        private bool ValidPair(SparkAILevelContext level, SparkNativeWiringPlan.WireStep step,
            out SparkTerminal a, out SparkTerminal b, out string error)
        {
            a = b = null;
            if (step == null)
            { error = "The reviewed wire step is missing."; return false; }
            return level.Layout.SceneBindings.TryResolveLivePair(level.Layout, step.fromId, step.toId, out a, out b, out error);
        }

        public string Preview()
        {
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            var plan = FindApprovedPlan(level, out reason);
            if (!plan)
            {
                SparkNativeWiringPlan candidate = null;
                foreach (var draft in loadedPlans)
                {
                    if (!draft || draft.LevelNumber != level.Definition.LevelNumber ||
                        draft.LayoutName != LayoutKey(level.Layout.name) || !draft.ValidateStructure(out _))
                        continue;
                    if (candidate)
                        return Message(ConstructionBlock(level, reason + " Multiple candidate assets require Editor review."));
                    candidate = draft;
                }
                return Message(candidate ? ConstructionBlock(level, reason) + "\n" +
                    PreviewDetails(candidate, manager, level, reviewed: false) : ConstructionBlock(level, reason));
            }
            return Message(PreviewDetails(plan, manager, level));
        }

        private string PreviewDetails(SparkNativeWiringPlan plan, LevelGamePlayManager manager, SparkAILevelContext level,
            bool reviewed = true)
        {
            var result = new StringBuilder(reviewed ? "APPROVED PRACTICE PLAN: " : "REVIEW REQUIRED: ").Append(plan.name);
            for (int i = 0; i < plan.Wires.Count; i++)
            {
                var step = plan.Wires[i];
                if (!ValidPair(level, step, out var a, out var b, out string reason))
                {
                    result.Append('\n').Append(i + 1).Append(". ").Append(step?.fromId)
                        .Append(" -> ").Append(step?.toId).Append(" [blocked: ").Append(reason).Append(']');
                    continue;
                }
                result.Append('\n').Append(i + 1).Append(". ").Append(step.fromId)
                    .Append(" -> ").Append(step.toId)
                    .Append(manager.CircuitSystem.HasConnectionBetween(a, b) ? " [connected]" : " [pending]");
            }
            return result.ToString();
        }

        public string SuggestNext() => SuggestNext(requireRevision: false);

        private string SuggestNext(bool requireRevision)
        {
            if (mode != OperatingMode.Assisted)
                return Message("Switch to Assisted mode to request one suggested reviewed wire.");
            if (IsExecuting || (director && director.IsBusy)) return Message("A native wiring action is already in progress.");
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            var plan = FindApprovedPlan(level, out reason, requireRevision);
            if (!plan) return Message(requireRevision ? ConstructionBlock(level, reason) : reason);
            ClearPending();
            for (int i = 0; i < plan.Wires.Count; i++)
            {
                var step = plan.Wires[i];
                if (!ValidPair(level, step, out var a, out var b, out reason))
                    return Message("Cannot suggest step " + (i + 1) + ": " + reason);
                if (manager.CircuitSystem.HasConnectionBetween(a, b)) continue;
                pendingFrom = step.fromId;
                pendingTo = step.toId;
                pendingRoot = level.Layout.transform;
                pendingPlan = plan;
                currentAction = "Review: " + PendingStep;
                return Message("SUGGESTION ONLY: " + PendingStep + ". Confirm the authored schematic, then explicitly approve this step. No cable created.");
            }
            return Message("All reviewed wiring connections are already present. The official evaluator determines remaining objectives.");
        }

        private string ShowSuggestedSource()
        {
            if (!GetContext(out _, out var level, out string reason)) return Message(reason);
            if (!pendingRoot || pendingRoot != level.Layout.transform || !pendingPlan || !pendingPlan.IsApproved)
                return Message("Suggest a reviewed wire for the active level first.");
            if (!level.Layout.SceneBindings.TryResolveLivePair(level.Layout, pendingFrom, pendingTo, out var source, out _, out reason))
                return Message(reason);
            var character = GetComponent<SparkProCharacter>();
            if (!character || !character.isActiveAndEnabled || IsExecuting)
                return Message("The character is unavailable to point at the suggested source.");
            character.PointAt(source.transform, false, 1f);
            return Message("The reviewed source is " + pendingFrom + ". I'm pointing at its live terminal.");
        }

        public string ApproveSuggested()
        {
            if (mode != OperatingMode.Assisted) return Message("This command requires Assisted mode.");
            if (deviceOperator && deviceOperator.IsBusy) return Message("A device action already owns Spark's hands.");
            if (IsExecuting || (director && director.IsBusy)) return Message("A wiring action is already running.");
            if (string.IsNullOrEmpty(pendingFrom) || !pendingRoot || !pendingPlan)
                return Message("No current suggestion. Use 'spark suggest next wire' first.");
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            if (level.Layout.transform != pendingRoot || FindApprovedPlan(level, out reason) != pendingPlan)
            { ClearPending(); return Message("Plan or level changed. Suggest again before approval."); }
            if (!PowerSafe(level)) return Message("Turn the virtual power output OFF before wiring.");
            SparkNativeWiringPlan.WireStep found = null;
            for (int i = 0; i < pendingPlan.Wires.Count; i++)
            {
                var step = pendingPlan.Wires[i];
                if (step.fromId == pendingFrom && step.toId == pendingTo) { found = step; break; }
            }
            if (!ValidPair(level, found, out var a, out var b, out reason)) return Message(reason);
            if (manager.CircuitSystem.HasConnectionBetween(a, b))
            { ClearPending(); return Message("This wire is already connected. Nothing changed."); }
            if (!director) return Message("Native player wire director is missing.");
            if (existingRecorder && (existingRecorder.IsExecuting || existingRecorder.IsRecording))
                return Message("Finish player-wire recording or the existing action before approving a wire.");
            string sourceId = pendingFrom, destinationId = pendingTo;
            SparkNativeWiringPlan confirmedPlan = pendingPlan;
            ClearPending();
            activePlan = confirmedPlan;
            activeRoot = level.Layout.transform;
            activeCircuit = manager.CircuitSystem;
            currentAction = sourceId + " -> " + destinationId;
            completedSteps = 0;
            totalSteps = 1;
            cancelRequested = false;
            pauseRequested = false;
            WiringOperationRevision++;
            SetPhase(ExecutionPhase.Running);
            activeRoutine = StartCoroutine(ExecuteSingle(sourceId, destinationId, a, b));
            return Message("Approved one reviewed wire: " + currentAction + ". Spark is using the actual player tool.");
        }

       
private IEnumerator ExecuteSingle(
    string from,
    string to,
    SparkTerminal a,
    SparkTerminal b)
{
    yield return null;

    // Validate the native wiring controller first.
    if (!director)
    {
        FinishFailure(
            "Native Wire Director is missing or unavailable.");
        yield break;
    }

    // Execute the approved connection using the real
    // player-native wiring tool.
    if (!DispatchReviewedWire(
        from, to, out string detail))
    {
        FinishFailure(
            "Native tool rejected this approved connection: " +
            detail);
        yield break;
    }

    float until =
        Time.realtimeSinceStartup + maxSecondsPerCable;

    float nextSafetyProbe = 0f;

    while (director &&
           director.IsBusy &&
           !cancelRequested &&
           Time.realtimeSinceStartup < until)
    {
        if (Time.realtimeSinceStartup >= nextSafetyProbe)
        {
            if (!ContextMatches(out string error))
            {
                FinishFailure(
                    "Safety changed while wiring: " + error);
                yield break;
            }

            nextSafetyProbe =
                Time.realtimeSinceStartup + 0.15f;
        }

        yield return null;
    }

    if (cancelRequested)
    {
        FinishCancelled();
        yield break;
    }

    if (director && director.IsBusy)
    {
        FinishFailure(
            "Physical wire action timed out.");
        yield break;
    }

    // Verify the authoritative electrical graph.
    if (!activeCircuit ||
        !a ||
        !b ||
        !activeCircuit.HasConnectionBetween(a, b))
    {
        FinishFailure(
            "Native tool did not confirm a physical circuit connection.");
        yield break;
    }

    completedSteps = 1;

    FinishSuccess(
        "Assisted wire electrically registered. " +
        "Further objectives remain evaluator-controlled.");
}

        public string StartAuto() => StartAuto(requireRevision: false);

        private string StartAuto(bool requireRevision)
        {
            if (mode != OperatingMode.Auto) return Message("Select Auto AI mode first.");
            if (deviceOperator && deviceOperator.IsBusy) return Message("A device action already owns Spark's hands.");
            if (IsExecuting || (director && director.IsBusy)) return Message("A wiring operation is already active.");
            if (existingRecorder && (existingRecorder.IsExecuting || existingRecorder.IsRecording))
                return Message("Stop the v11 recorder or existing automation before starting v14.");
            if (!director) return Message("Native player wire director is missing.");
            if (!GetContext(out var manager, out var level, out string reason)) return Message(reason);
            var plan = FindApprovedPlan(level, out reason, requireRevision);
            if (!plan) return Message(requireRevision ? ConstructionBlock(level, reason) : reason);
            if (!PowerSafe(level)) return Message("Turn the virtual power output OFF before AI wiring.");
            for (int i = 0; i < plan.Wires.Count; i++)
                if (!ValidPair(level, plan.Wires[i], out _, out _, out reason))
                    return Message("Approved step " + (i + 1) + " invalid: " + reason);
            cancelRequested = false;
            pauseRequested = false;
            completedSteps = 0;
            totalSteps = plan.Wires.Count;
            activeRoot = level.Layout.transform;
            activeCircuit = manager.CircuitSystem;
            activePlan = plan;
            currentAction = "Preparing approved plan: " + plan.name;
            WiringOperationRevision++;
            SetPhase(ExecutionPhase.Running);
            activeRoutine = StartCoroutine(ExecutePlan(plan));
            return Message("AUTO started: " + totalSteps + " reviewed wires using the real PLAYER Wire Tool. Pause operates between cables.");
        }

        private IEnumerator ExecutePlan(SparkNativeWiringPlan plan)
        {
            yield return null;
            for (int i = 0; i < plan.Wires.Count; i++)
            {
                while (pauseRequested && !cancelRequested)
                {
                    if (phase != ExecutionPhase.Paused) SetPhase(ExecutionPhase.Paused);
                    if (!ContextMatches(out string pausedError))
                    { FinishFailure("While paused: " + pausedError); yield break; }
                    yield return new WaitForSecondsRealtime(0.2f);
                }
                if (cancelRequested) { FinishCancelled(); yield break; }
                if (!ContextMatches(out string validationError))
                { FinishFailure("Safety interrupted the plan: " + validationError); yield break; }
                var level = SparkAILevelContext.Capture(executive.ActiveLevelManager);
                var step = plan.Wires[i];
                if (!ValidPair(level, step, out var a, out var b, out validationError))
                { FinishFailure("Step " + (i + 1) + ": " + validationError); yield break; }
                currentAction = (i + 1) + "/" + plan.Wires.Count + ": " + step.fromId + " -> " + step.toId;
                SetPhase(ExecutionPhase.Running);
                if (activeCircuit.HasConnectionBetween(a, b))
                {
                    completedSteps++;
                    Note("Already connected; verified " + currentAction + ".", false);
                    continue;
                }
                Note("I will approach " + step.fromId + " and carry the native cable to " + step.toId + ".");
                if (!DispatchReviewedWire(step.fromId, step.toId, out string startInfo))
                { FinishFailure("Native director rejected step " + (i + 1) + ": " + startInfo); yield break; }
                float deadline = Time.realtimeSinceStartup + maxSecondsPerCable;
                float nextSafetyProbe = 0f;
                while (director && director.IsBusy && !cancelRequested && Time.realtimeSinceStartup < deadline)
                {
                    if (Time.realtimeSinceStartup >= nextSafetyProbe)
                    {
                        if (!ContextMatches(out string runningError))
                        { FinishFailure("Safety interrupted native wiring: " + runningError); yield break; }
                        nextSafetyProbe = Time.realtimeSinceStartup + 0.15f;
                    }
                    yield return null;
                }
                if (cancelRequested) { FinishCancelled(); yield break; }
                if (director && director.IsBusy) { FinishFailure("Wire " + (i + 1) + " timed out."); yield break; }
                if (!activeCircuit || !a || !b || !activeCircuit.HasConnectionBetween(a, b))
                { FinishFailure("Native cable for step " + (i + 1) + " was not registered by SparkCircuitSystem."); yield break; }
                completedSteps++;
                Note("Native cable confirmed: " + currentAction + ".");
                yield return new WaitForFixedUpdate();
                if (executive.ActiveLevelManager.IsFailed)
                { FinishFailure("Official evaluator reported a failed level. No override attempted."); yield break; }
            }
            if (!ContextMatches(out string finalError))
            { FinishFailure("Final evaluation interrupted: " + finalError); yield break; }
            var evaluator = executive ? executive.ActiveLevelManager : null;
            string verdict = evaluator && evaluator.HasCurrentElectricalEvaluation
                ? evaluator.IsCompleted ? "Official level evaluator reports completed."
                    : $"Official evaluator reports {evaluator.SatisfiedTargetCount}/{evaluator.ActiveLevel.TargetCount} objectives. Turn on power or complete remaining tasks through the native controls."
                : "Wiring finished. Await a current official evaluation.";
            FinishSuccess("All approved practice wires checked. " + verdict);
        }

        private bool ContextMatches(out string error)
        {
            if (!GetContext(out var manager, out var level, out error)) return false;
            if (!activeRoot || level.Layout.transform != activeRoot ||
                !activeCircuit || manager.CircuitSystem != activeCircuit)
            { error = "Active level, layout or circuit system changed."; return false; }
            if (activePlan && !activePlan.IsApproved)
            { error = "Approved plan was edited or revoked."; return false; }
            if (activePlan && activePlan.HasRevisionReview &&
                !activePlan.MatchesReviewedLayout(level.Definition, manager.LayoutLoader.ActiveSourcePrefab, out error))
                return false;
            if (!PowerSafe(level))
            { error = "Virtual supply became active while wiring."; return false; }
            return true;
        }

        public string Pause()
        {
            if (mode != OperatingMode.Auto || !IsExecuting) return Message("No Auto AI plan is currently running.");
            pauseRequested = true;
            return Message("Pause requested. Spark will finish the current native cable safely, then pause before the next wire.");
        }

        public string Resume()
        {
            if (!IsExecuting || !pauseRequested) return Message("No paused Auto AI operation exists.");
            if (!ContextMatches(out string error)) return Message("Cannot resume: " + error);
            pauseRequested = false;
            SetPhase(ExecutionPhase.Running);
            return Message("Resumed reviewed plan after rechecking safety and active level.");
        }

        public string CancelAutomation()
        {
            CancelInternal("Player cancelled. Confirmed player-native connections were not removed.", true);
            if (deviceOperator && (deviceOperator.IsBusy || deviceOperator.Proposal != "None")) deviceOperator.Cancel();
            ClearPending();
            return Message("AI wiring cancelled; native tool released. Previously confirmed connections are retained.");
        }

        private void CancelInternal(string why, bool cancelWire)
        {
            cancelRequested = true;
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = null;
            if (cancelWire && director && director.IsBusy) director.Cancel();
            activeCircuit = null;
            activeRoot = null;
            activePlan = null;
            pauseRequested = false;
            currentAction = "None";
            SetPhase(ExecutionPhase.Cancelled);
            if (!string.IsNullOrEmpty(why)) Note(why, false);
        }

        private void FinishCancelled()
        {
            activeRoutine = null;
            activeCircuit = null;
            activeRoot = null;
            activePlan = null;
            currentAction = "None";
            SetPhase(ExecutionPhase.Cancelled);
            Note("Cancelled. Only confirmed native cables remain.", false);
        }

        private void FinishFailure(string problem)
        {
            if (director && director.IsBusy) director.Cancel();
            activeRoutine = null;
            activeCircuit = null;
            activeRoot = null;
            activePlan = null;
            currentAction = "None";
            SetPhase(ExecutionPhase.Failed);
            Note("STOPPED: " + problem, false);
        }

        private void FinishSuccess(string result)
        {
            activeRoutine = null;
            activeCircuit = null;
            activeRoot = null;
            activePlan = null;
            currentAction = "None";
            SetPhase(ExecutionPhase.Completed);
            Note(result);
        }

        private bool HasNativeAIWireCommand(string q)
        {
            return DirectWire.IsMatch(q) || q == "confirm wire" || q == "spark connect wire" ||
                q == "auto wire practice level" || q == "spark wire this practice level" ||
                q == "spark start approved plan";
        }

        /// <summary>
        /// Called at the beginning of SparkProExecutive.Ask (inside its existing try/finally).
        /// Recognized action commands never fall through to legacy AI mutation routes.
        /// </summary>
        public static bool TryHandleFrom(SparkProExecutive caller, string question, out string response)
        {
            response = null;
            if (!caller || string.IsNullOrWhiteSpace(question)) return false;
            string q = question.Trim().TrimEnd('?', '.', '!').ToLowerInvariant();
            if (q.StartsWith("hey spark ")) q = q.Substring(10).Trim();
            if (q.StartsWith("spark,", StringComparison.Ordinal)) q = "spark " + q.Substring(6).TrimStart();
            var hub = caller.GetComponent<SparkEngineeringControlHub>();
            if (!hub) return false; // Existing game behavior remains available until bound.

            switch (q)
            {
                case "draw current level": case "spark draw current level": case "spark draw this level":
                case "wire current level": case "spark wire current level": case "spark auto draw":
                    response = hub.DrawCurrentLevel(); return true;
                case "spark continue drawing":
                    response = hub.IsPaused ? hub.Resume() : hub.DrawCurrentLevel(); return true;
                case "spark pause drawing":
                    response = hub.Pause(); return true;
                case "spark resume drawing":
                    response = hub.Resume(); return true;
                case "spark stop drawing":
                    response = hub.CancelAutomation(); return true;
                case "spark drawing status":
                    response = hub.ConstructionStatus(); return true;
                case "spark explain current construction":
                    response = hub.ExplainConstruction(); return true;
                case "spark manual mode": case "manual mode":
                    response = hub.SelectMode(OperatingMode.Manual); return true;
                case "spark assisted mode": case "assisted mode":
                    response = hub.SelectMode(OperatingMode.Assisted); return true;
                case "spark auto mode": case "auto ai mode":
                    response = hub.SelectMode(OperatingMode.Auto); return true;
                case "spark engineering status": case "engineering status":
                case "auto wiring status":
                    response = hub.Status(); return true;
                case "spark preview practice plan": case "preview wiring plan":
                case "preview approved wiring plan":
                    response = hub.Preview(); return true;
                case "spark suggest next wire": case "spark suggest next step":
                case "spark suggest the next wire": case "spark inspect the next connection":
                    response = hub.SuggestNext(); return true;
                case "spark show me where the source terminal is":
                    response = hub.ShowSuggestedSource(); return true;
                case "spark explain why this connection failed":
                    response = hub.Message(hub.director ? hub.director.LastStatus : "The native wire director is unavailable.");
                    return true;
                case "spark approve suggested wire": case "spark perform approved step": case "spark connect the approved wire":
                    response = hub.ApproveSuggested(); return true;
                case "spark start approved plan": case "auto wire practice level":
                case "spark wire this practice level":
                    response = hub.StartAuto(); return true;
                case "spark pause automation": case "pause auto wiring":
                    response = hub.Pause(); return true;
                case "spark resume automation": case "resume auto wiring":
                    response = hub.Resume(); return true;
                case "spark cancel automation": case "cancel auto wiring": case "spark stop":
                    response = hub.CancelAutomation(); return true;
                case "cancel wire": case "stop wiring":
                    response = hub.CancelAutomation(); return true;
                case "spark reload engineering plans":
                    response = hub.Message(hub.Reload()); return true;
                case "spark engineering narration on":
                    hub.SetSpokenNarration(true);
                    response = hub.Message("Action speech enabled when a voice controller is assigned; status text always remains visible.");
                    return true;
                case "spark engineering narration off":
                    hub.SetSpokenNarration(false); response = hub.Message("Action speech disabled; visual explanations remain enabled.");
                    return true;
                case "confirm wire": case "spark connect wire":
                    response = hub.Message("Direct AI wiring is controlled by mode. In Assisted mode, use 'spark suggest next wire' then 'spark approve suggested wire'. In Auto mode, execute an approved plan.");
                    return true;
            }
            if (hub.HasNativeAIWireCommand(q))
            {
                response = hub.Message("Unreviewed direct AI wiring is blocked by the v14 mode controller. Use Assisted approval or an approved Auto practice plan.");
                return true;
            }
            // Original flight, teaching, recording and player-independent commands still work.
            return false;
        }
    }
}
