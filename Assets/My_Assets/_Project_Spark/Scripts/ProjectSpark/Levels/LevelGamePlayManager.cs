using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class LevelGamePlayManager : MonoBehaviour
    {
        public enum LevelRuntimeState
        {
            Uninitialized,
            Ready,
            Playing,
            Completed,
            Failed
        }

        [Serializable]
        public struct TargetRuntimeResult
        {
            public string targetId;
            public string displayName;
            public bool satisfied;

            public TargetRuntimeResult(
                string targetId,
                string displayName,
                bool satisfied)
            {
                this.targetId = targetId;
                this.displayName = displayName;
                this.satisfied = satisfied;
            }
        }

        [Header("Core Systems")]
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField] private SparkElectricalSolver electricalSolver;

        [Header("Levels")]
        [SerializeField] private SparkLevelDefinition[] levels;

        [Min(0)]
        [SerializeField] private int activeLevelIndex;

        [Header("Runtime")]
        [SerializeField] private LevelRuntimeState runtimeState =
            LevelRuntimeState.Uninitialized;

        [SerializeField] private bool autoStartFirstLevel = true;
        [SerializeField] private bool autoEvaluate = true;
        [SerializeField] private bool autoAdvanceOnCompletion;
        [SerializeField] private bool preventInteractionAfterCompletion;

        [Header("Debug")]
        [SerializeField] private bool debugLogging;
        [SerializeField] private bool verboseLogging;
        [SerializeField] private bool showRuntimeState = true;

        private bool evaluationQueued;
        private bool subscribed;
        private bool evaluating;
        private bool solverEvaluationFailed;

        // SparkCircuitSystem exposes TopologyVersion. We use this as a
        // lightweight fallback because older/current circuit mutations may
        // update the version without invoking TopologyChanged.
        private int lastTopologyVersion = -1;

        private int satisfiedTargetCount;

        private bool currentLevelCompleted;
        private bool currentLevelFailed;

        private bool hasValidPowerSource;
        private bool closedReturn;
        private bool sourceShorted;
        private bool targetShorted;
        private bool wrongConnection;
        private bool overloaded;

        private float targetVoltage;
        private float targetCurrent;
        private float targetPower;

        private string activeSourceName;
        private string lastFailureReason;
        private string lastEvaluationMessage;

        private SparkLevelValidationResult validationResult;

        private readonly List<TargetRuntimeResult> targetResults =
            new List<TargetRuntimeResult>(16);

        private readonly List<PowerSourceRuntime> validSources =
            new List<PowerSourceRuntime>(8);

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>(32);

        private readonly Queue<SparkTerminal> traversalQueue =
            new Queue<SparkTerminal>(32);

        private readonly HashSet<SparkTerminal> visitedTerminals =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> positiveReachable =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> negativeReachable =
            new HashSet<SparkTerminal>();

        public SparkCircuitSystem CircuitSystem => circuitSystem;
        public SparkElectricalSolver ElectricalSolver => electricalSolver;

        public SparkLevelDefinition[] Levels => levels;

        public int ActiveLevelIndex => activeLevelIndex;

        public int LevelCount =>
            levels != null ? levels.Length : 0;

        public SparkLevelDefinition ActiveLevel =>
            GetLevel(activeLevelIndex);

        public LevelRuntimeState RuntimeState =>
            runtimeState;

        public bool IsPlaying =>
            runtimeState == LevelRuntimeState.Playing;

        public bool IsCompleted =>
            runtimeState == LevelRuntimeState.Completed;

        public bool IsFailed =>
            runtimeState == LevelRuntimeState.Failed;

        public int SatisfiedTargetCount =>
            satisfiedTargetCount;

        public int ActiveTargetCount =>
            ActiveLevel != null ? ActiveLevel.TargetCount : 0;

        public bool HasValidPowerSource =>
            hasValidPowerSource;

        public string ActiveSourceName =>
            activeSourceName;

        public bool ClosedReturn =>
            closedReturn;

        public bool SourceShorted =>
            sourceShorted;

        public bool TargetShorted =>
            targetShorted;

        public bool WrongConnection =>
            wrongConnection;

        public bool IsOverloaded =>
            overloaded;

        public float TargetVoltage =>
            targetVoltage;

        public float TargetCurrent =>
            targetCurrent;

        public float TargetPower =>
            targetPower;

        public string LastFailureReason =>
            lastFailureReason;

        public string LastEvaluationMessage =>
            lastEvaluationMessage;

        public SparkLevelValidationResult ValidationResult =>
            validationResult;

        public SparkLevelValidationState ValidationState =>
            validationResult.state;

        public bool HasValidationFault =>
            validationResult.IsFault;

        public bool ShouldBlockInteraction =>
            preventInteractionAfterCompletion &&
            (IsCompleted || IsFailed);

        public bool CanInteract =>
            !ShouldBlockInteraction &&
            IsPlaying;

        public IReadOnlyList<TargetRuntimeResult> TargetResults =>
            targetResults;

        public event Action<SparkLevelDefinition> LevelStarted;
        public event Action<SparkLevelDefinition> LevelCompleted;
        public event Action<SparkLevelDefinition, string> LevelFailed;
        public event Action<SparkLevelDefinition> LevelReset;
        public event Action<SparkLevelDefinition> LevelEvaluationChanged;

        private void Awake()
        {
            ResolveReferences();

            runtimeState = LevelRuntimeState.Ready;

            CaptureTopologyVersion();

            Subscribe();

            if (autoStartFirstLevel && LevelCount > 0)
            {
                StartLevel(activeLevelIndex);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureTopologyVersion();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            if (circuitSystem != null)
            {
                DetectTopologyVersionChange();
            }

            if (!autoEvaluate)
                return;

            if (!evaluationQueued)
                return;

            evaluationQueued = false;

            EvaluateActiveLevel();
        }

        // ============================================================
        // LEVEL CONTROL
        // ============================================================

        public bool StartLevel(int index)
        {
            SparkLevelDefinition level = GetLevel(index);

            if (level == null)
                return false;

            activeLevelIndex = index;

            level.Normalize();

            currentLevelCompleted = false;
            currentLevelFailed = false;

            satisfiedTargetCount = 0;

            hasValidPowerSource = false;
            closedReturn = false;
            sourceShorted = false;
            targetShorted = false;
            wrongConnection = false;
            overloaded = false;

            targetVoltage = 0f;
            targetCurrent = 0f;
            targetPower = 0f;

            activeSourceName = string.Empty;

            lastFailureReason = string.Empty;
            lastEvaluationMessage = string.Empty;

            SetValidationResult(
                SparkLevelValidationState.Playing,
                "Level started.");

            targetResults.Clear();
            validSources.Clear();

            runtimeState = LevelRuntimeState.Playing;

            level.SetRuntimeOutputs(false, false);

            CaptureTopologyVersion();

            QueueEvaluation();

            LevelStarted?.Invoke(level);

            Log(
                $"Started level {index + 1}: {level.DisplayName}");

            return true;
        }

        public bool NextLevel()
        {
            int next = activeLevelIndex + 1;

            if (next >= LevelCount)
                return false;

            return StartLevel(next);
        }

        public bool PreviousLevel()
        {
            int previous = activeLevelIndex - 1;

            if (previous < 0)
                return false;

            return StartLevel(previous);
        }

        public bool RestartLevel()
        {
            if (ActiveLevel == null)
                return false;

            return StartLevel(activeLevelIndex);
        }

        public bool SetActiveLevel(int index)
        {
            return StartLevel(index);
        }

        public bool CompleteCurrentLevel()
        {
            SparkLevelDefinition level = ActiveLevel;

            if (level == null)
                return false;

            if (currentLevelCompleted)
                return true;

            currentLevelCompleted = true;
            currentLevelFailed = false;

            runtimeState = LevelRuntimeState.Completed;

            level.SetRuntimeOutputs(true, false);

            SetValidationResult(
                SparkLevelValidationState.Completed,
                "Level completed.");

            LevelCompleted?.Invoke(level);

            Log(
                $"LEVEL COMPLETE: {level.DisplayName}");

            if (autoAdvanceOnCompletion)
            {
                int next = activeLevelIndex + 1;

                if (next < LevelCount)
                    StartLevel(next);
            }

            return true;
        }

        public void FailCurrentLevel(string reason)
        {
            SparkLevelDefinition level = ActiveLevel;

            if (level == null)
                return;

            if (currentLevelFailed)
                return;

            currentLevelFailed = true;
            currentLevelCompleted = false;

            lastFailureReason =
                string.IsNullOrWhiteSpace(reason)
                    ? "Level failed."
                    : reason;

            runtimeState = LevelRuntimeState.Failed;

            level.SetRuntimeOutputs(false, true);

            SetValidationResult(
                GetFailureValidationState(),
                lastFailureReason);

            LevelFailed?.Invoke(
                level,
                lastFailureReason);

            LevelEvaluationChanged?.Invoke(level);

            Log(
                $"LEVEL FAILED: {level.DisplayName} | " +
                lastFailureReason);
        }

        public void ResetLevel()
        {
            SparkLevelDefinition level = ActiveLevel;

            if (level == null)
                return;

            currentLevelCompleted = false;
            currentLevelFailed = false;

            satisfiedTargetCount = 0;

            hasValidPowerSource = false;
            closedReturn = false;
            sourceShorted = false;
            targetShorted = false;
            wrongConnection = false;
            overloaded = false;

            targetVoltage = 0f;
            targetCurrent = 0f;
            targetPower = 0f;

            activeSourceName = string.Empty;

            lastFailureReason = string.Empty;
            lastEvaluationMessage = string.Empty;

            SetValidationResult(
                SparkLevelValidationState.Playing,
                "Level reset.");

            targetResults.Clear();
            validSources.Clear();

            runtimeState = LevelRuntimeState.Playing;

            level.SetRuntimeOutputs(false, false);

            CaptureTopologyVersion();

            QueueEvaluation();

            LevelReset?.Invoke(level);

            Log(
                $"Level reset: {level.DisplayName}");
        }

        // ============================================================
        // EVALUATION CONTROL
        // ============================================================

        public void EvaluateNow()
        {
            evaluationQueued = false;
            EvaluateActiveLevel();
        }

        public void QueueEvaluation()
        {
            if (!autoEvaluate)
                return;

            evaluationQueued = true;
        }

        private void EvaluateActiveLevel()
        {
            if (evaluating)
                return;

            SparkLevelDefinition level = ActiveLevel;

            if (level == null)
                return;

            if (runtimeState != LevelRuntimeState.Playing)
                return;

            if (circuitSystem == null)
            {
                FailCurrentLevel(
                    "Circuit system is not assigned.");

                return;
            }

            evaluating = true;

            try
            {
                targetResults.Clear();

                satisfiedTargetCount = 0;

                ResetEvaluationValues();

                // The solver is the electrical source of truth.
                // SolveNow() is synchronous in the current solver, so target
                // state is current when evaluation continues.
                solverEvaluationFailed = false;

                if (electricalSolver != null)
                {
                    electricalSolver.SolveNow();
                }

                // A synchronous solver callback can queue another evaluation.
                // The current evaluation already consumed the fresh state.
                evaluationQueued = false;

                EvaluatePowerSources(level);

                EvaluateTargets(level);

                EvaluateTopologyState(level);

                EvaluateFailureConditions(level);

                if (runtimeState == LevelRuntimeState.Failed)
                {
                    lastEvaluationMessage =
                        string.IsNullOrEmpty(lastFailureReason)
                            ? "Level failed."
                            : lastFailureReason;

                    LevelEvaluationChanged?.Invoke(level);
                    return;
                }

                bool completionSatisfied =
                    EvaluateCompletion(level);

                if (completionSatisfied)
                {
                    CompleteCurrentLevel();
                    return;
                }

                lastEvaluationMessage =
                    BuildEvaluationMessage(level);

                SetValidationResult(
                    DeterminePlayingValidationState(),
                    lastEvaluationMessage);

                LevelEvaluationChanged?.Invoke(level);

                LogVerbose(lastEvaluationMessage);
            }
            finally
            {
                evaluating = false;
            }
        }

        private SparkLevelValidationState GetFailureValidationState()
        {
            if (sourceShorted)
                return SparkLevelValidationState.ShortCircuit;

            if (targetShorted)
                return SparkLevelValidationState.TargetShort;

            if (wrongConnection)
                return SparkLevelValidationState.WrongConnection;

            if (overloaded)
                return SparkLevelValidationState.Overload;

            if (solverEvaluationFailed)
                return SparkLevelValidationState.SolverFault;

            return SparkLevelValidationState.InvalidConfiguration;
        }

        private SparkLevelValidationState DeterminePlayingValidationState()
        {
            if (sourceShorted)
                return SparkLevelValidationState.ShortCircuit;

            if (targetShorted)
                return SparkLevelValidationState.TargetShort;

            if (wrongConnection)
                return SparkLevelValidationState.WrongConnection;

            if (overloaded)
                return SparkLevelValidationState.Overload;

            if (solverEvaluationFailed)
                return SparkLevelValidationState.SolverFault;

            return SparkLevelValidationState.Playing;
        }

        private void SetValidationResult(
            SparkLevelValidationState state,
            string message)
        {
            SparkLevelTarget affectedTarget = null;
            SparkTerminal affectedTerminal = null;

            if (state == SparkLevelValidationState.WrongConnection ||
                state == SparkLevelValidationState.TargetShort)
            {
                affectedTarget = GetPrimaryTarget(ActiveLevel);
            }

            if (state == SparkLevelValidationState.WrongConnection)
            {
                SparkTerminal positive;
                SparkTerminal negative;

                if (TryGetTargetTerminalPair(
                        affectedTarget,
                        out positive,
                        out negative))
                {
                    if (positive != null &&
                        negativeReachable.Contains(positive))
                    {
                        affectedTerminal = positive;
                    }
                    else if (negative != null &&
                             positiveReachable.Contains(negative))
                    {
                        affectedTerminal = negative;
                    }
                }
            }
            else if (state == SparkLevelValidationState.ShortCircuit)
            {
                SparkLevelDefinition level = ActiveLevel;

                if (level != null &&
                    level.PowerSources != null)
                {
                    for (int i = 0; i < level.PowerSources.Length; i++)
                    {
                        SparkLevelDefinition.PowerSourceDefinition source =
                            level.PowerSources[i];

                        if (source == null)
                            continue;

                        if (source.PositiveTerminal != null &&
                            negativeReachable.Contains(source.PositiveTerminal))
                        {
                            affectedTerminal = source.PositiveTerminal;
                            break;
                        }

                        if (source.NegativeTerminal != null &&
                            positiveReachable.Contains(source.NegativeTerminal))
                        {
                            affectedTerminal = source.NegativeTerminal;
                            break;
                        }
                    }
                }
            }

            validationResult = new SparkLevelValidationResult(
                state,
                string.IsNullOrWhiteSpace(message) ? string.Empty : message,
                satisfiedTargetCount,
                ActiveLevel != null ? ActiveLevel.TargetCount : 0,
                targetVoltage,
                targetCurrent,
                targetPower,
                activeSourceName,
                affectedTerminal,
                affectedTarget);
        }

        private void ResetEvaluationValues()
        {
            hasValidPowerSource = false;
            closedReturn = false;
            sourceShorted = false;
            targetShorted = false;
            wrongConnection = false;
            overloaded = false;

            targetVoltage = 0f;
            targetCurrent = 0f;
            targetPower = 0f;

            activeSourceName = string.Empty;
        }

        private bool EvaluateCompletion(
            SparkLevelDefinition level)
        {
            if (validSources.Count == 0)
                return false;

            if (level.RequireClosedReturn &&
                !closedReturn)
            {
                return false;
            }

            if (level.MinimumVoltage > 0f &&
                targetVoltage < level.MinimumVoltage)
            {
                return false;
            }

            return level.IsCompletionSatisfied(
                satisfiedTargetCount);
        }

        private string BuildEvaluationMessage(
            SparkLevelDefinition level)
        {
            string message =
                $"{satisfiedTargetCount}/{level.TargetCount} " +
                "targets satisfied.";

            if (level.RequireClosedReturn &&
                !closedReturn)
            {
                message += " Required circuit return is open.";
            }

            if (hasValidPowerSource)
            {
                message +=
                    $" Source: {activeSourceName}.";

                if (targetVoltage > 0f)
                {
                    message +=
                        $" Target {targetVoltage:0.###} V, " +
                        $"{targetCurrent:0.###} A.";
                }
            }
            else
            {
                message += " No valid configured power source.";
            }

            return message;
        }

        // ============================================================
        // TARGETS
        // ============================================================

        private void EvaluateTargets(
            SparkLevelDefinition level)
        {
            SparkLevelTarget[] targets = level.Targets;

            if (targets == null)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                SparkLevelTarget target = targets[i];

                if (target == null)
                    continue;

                target.Normalize();

                bool satisfied = target.Evaluate();

                if (satisfied)
                    satisfiedTargetCount++;

                targetResults.Add(
                    new TargetRuntimeResult(
                        target.TargetId,
                        target.DisplayName,
                        satisfied));

                ReadTargetElectricalState(target);

                LogVerbose(
                    $"Target {target.DisplayName}: " +
                    $"{(satisfied ? "PASS" : "WAIT")}");
            }

            // If there are several targets, keep the first target's
            // electrical values for the compact runtime inspector.
        }

        private void ReadTargetElectricalState(
            SparkLevelTarget target)
        {
            if (target == null)
                return;

            if (target.TargetTerminal != null)
            {
                SparkTerminalElectricalState state =
                    target.TargetTerminal.ElectricalState;

                targetVoltage = state.Voltage;
                targetCurrent = state.Current;
                targetPower = state.Power;

                return;
            }

            if (target.TargetComponent != null)
            {
                SparkElectricalState state =
                    target.TargetComponent.ElectricalState;

                targetVoltage = state.Voltage;
                targetCurrent = state.Current;
                targetPower = state.Power;
            }
        }

        // ============================================================
        // POWER SOURCES
        // ============================================================

        private void EvaluatePowerSources(
            SparkLevelDefinition level)
        {
            validSources.Clear();

            SparkLevelDefinition.PowerSourceDefinition[] sources =
                level.PowerSources;

            if (sources == null ||
                sources.Length == 0)
            {
                return;
            }

            for (int i = 0; i < sources.Length; i++)
            {
                SparkLevelDefinition.PowerSourceDefinition source =
                    sources[i];

                if (source == null)
                    continue;

                if (!ValidatePowerSource(
                        source,
                        out PowerSourceRuntime runtime))
                {
                    LogVerbose(
                        $"Invalid source: {source.SourceName}");

                    continue;
                }

                validSources.Add(runtime);

                LogVerbose(
                    $"Valid source: {source.SourceName}");

                if (!level.AllowAnyConfiguredSource)
                    break;
            }

            hasValidPowerSource = validSources.Count > 0;

            if (validSources.Count > 0)
            {
                activeSourceName =
                    validSources[0].Definition.SourceName;
            }

            LogVerbose(
                $"Configured valid sources: " +
                $"{validSources.Count}");
        }

        private bool ValidatePowerSource(
            SparkLevelDefinition.PowerSourceDefinition source,
            out PowerSourceRuntime runtime)
        {
            runtime = default;

            if (source.PositiveTerminal == null ||
                source.NegativeTerminal == null)
            {
                return false;
            }

            SparkPowerSupply positiveSupply =
                ResolvePowerSupply(
                    source.PositiveTerminal);

            SparkPowerSupply negativeSupply =
                ResolvePowerSupply(
                    source.NegativeTerminal);

            if (positiveSupply == null ||
                negativeSupply == null)
            {
                return false;
            }

            if (positiveSupply != negativeSupply)
                return false;

            if (!positiveSupply.isActiveAndEnabled)
                return false;

            if (!positiveSupply.ElectricalEnabled)
                return false;

            if (!positiveSupply.IsOutputActive)
                return false;

            runtime =
                new PowerSourceRuntime(
                    source,
                    positiveSupply);

            return true;
        }

        private SparkPowerSupply ResolvePowerSupply(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return null;

            SparkPowerSupply direct =
                terminal.GetComponent<SparkPowerSupply>();

            if (direct != null)
                return direct;

            SparkPowerSupply parent =
                terminal.GetComponentInParent<SparkPowerSupply>();

            if (parent != null)
                return parent;

            if (terminal.Owner != null)
            {
                SparkPowerSupply owner =
                    terminal.Owner
                        .GetComponent<SparkPowerSupply>();

                if (owner != null)
                    return owner;

                owner =
                    terminal.Owner
                        .GetComponentInParent<SparkPowerSupply>();

                if (owner != null)
                    return owner;
            }

            Transform root = terminal.transform.root;

            if (root != null)
            {
                SparkPowerSupply[] supplies =
                    root.GetComponentsInChildren<SparkPowerSupply>(true);

                for (int i = 0; i < supplies.Length; i++)
                {
                    SparkPowerSupply supply = supplies[i];

                    if (supply == null)
                        continue;

                    SparkTerminal[] terminals =
                        supply.GetComponentsInChildren<SparkTerminal>(true);

                    for (int j = 0; j < terminals.Length; j++)
                    {
                        if (terminals[j] == terminal)
                            return supply;
                    }
                }
            }

            return null;
        }

        // ============================================================
        // TOPOLOGY / ELECTRICAL RETURN
        // ============================================================

        private void EvaluateTopologyState(
            SparkLevelDefinition level)
        {
            if (validSources.Count == 0)
                return;

            SparkLevelTarget primaryTarget =
                GetPrimaryTarget(level);

            // Build real wire connectivity from the configured source.
            // Components are intentionally NOT treated as wires here.
            BuildSourceReachability();

            sourceShorted = HasSourceTopologyShort(level);

            SparkTerminal targetPositive;
            SparkTerminal targetNegative;

            bool hasTargetPair =
                TryGetTargetTerminalPair(
                    primaryTarget,
                    out targetPositive,
                    out targetNegative);

            if (hasTargetPair)
            {
                targetShorted =
                    positiveReachable.Contains(targetNegative) ||
                    negativeReachable.Contains(targetPositive);

                bool positivePath =
                    positiveReachable.Contains(targetPositive);

                bool negativePath =
                    negativeReachable.Contains(targetNegative);

                bool reversedPositive =
                    positiveReachable.Contains(targetNegative);

                bool reversedNegative =
                    negativeReachable.Contains(targetPositive);

                // A wrong/reversed connection is different from a valid
                // incomplete circuit. It is reported immediately so the
                // level can fail without waiting for completion.
                wrongConnection =
                    (reversedPositive || reversedNegative) &&
                    !targetShorted;

                // A complete source-to-target return is confirmed only
                // when both polarity paths exist AND the solved target is
                // actually conducting/powered.
                closedReturn =
                    positivePath &&
                    negativePath &&
                    IsTargetElectricallyConducting(primaryTarget);

                // A direct target short is a topology fault even if the
                // solver has not yet produced a large current.
                if (targetShorted)
                    closedReturn = false;
            }
            else
            {
                // If the target does not expose two terminals, fall back to
                // the solver's actual electrical state.
                closedReturn =
                    IsTargetElectricallyConducting(primaryTarget);

                targetShorted =
                    IsTargetShorted(primaryTarget);

                wrongConnection = false;
            }

            overloaded =
                HasOverload();

            LogVerbose(
                $"Topology: ClosedReturn={closedReturn}, " +
                $"SourceShort={sourceShorted}, " +
                $"TargetShort={targetShorted}, " +
                $"WrongConnection={wrongConnection}");
        }

        private void BuildSourceReachability()
        {
            positiveReachable.Clear();
            negativeReachable.Clear();

            int count = validSources.Count;

            for (int i = 0; i < count; i++)
            {
                PowerSourceRuntime source =
                    validSources[i];

                if (source.Definition == null)
                    continue;

                TraverseWireNetwork(
                    source.Definition.PositiveTerminal,
                    positiveReachable);

                TraverseWireNetwork(
                    source.Definition.NegativeTerminal,
                    negativeReachable);

                if (!ActiveLevel.AllowAnyConfiguredSource)
                    break;
            }
        }

        private void TraverseWireNetwork(
            SparkTerminal start,
            HashSet<SparkTerminal> visited)
        {
            if (start == null || visited == null)
                return;

            traversalQueue.Clear();
            visitedTerminals.Clear();

            traversalQueue.Enqueue(start);
            visited.Add(start);
            visitedTerminals.Add(start);

            while (traversalQueue.Count > 0)
            {
                SparkTerminal current =
                    traversalQueue.Dequeue();

                if (current == null)
                    continue;

                connectionBuffer.Clear();

                circuitSystem.GetConnections(
                    current,
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (!IsUsableTopologyConnection(connection))
                        continue;

                    SparkTerminal next =
                        GetOtherTerminal(
                            connection,
                            current);

                    if (next == null ||
                        visitedTerminals.Contains(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    visitedTerminals.Add(next);
                    traversalQueue.Enqueue(next);
                }
            }
        }

        private bool HasSourceTopologyShort(
            SparkLevelDefinition level)
        {
            if (!level.RejectShortCircuit)
                return false;

            for (int i = 0;
                 i < validSources.Count;
                 i++)
            {
                PowerSourceRuntime source =
                    validSources[i];

                if (source.Definition == null)
                    continue;

                SparkTerminal positive =
                    source.Definition.PositiveTerminal;

                SparkTerminal negative =
                    source.Definition.NegativeTerminal;

                if (positive == null || negative == null)
                    continue;

                if (positiveReachable.Contains(negative) ||
                    negativeReachable.Contains(positive))
                {
                    return true;
                }

                // Also catch a solver-reported source short.
                if (source.Supply != null &&
                    source.Supply.IsShortCircuit)
                {
                    return true;
                }

                if (!level.AllowAnyConfiguredSource)
                    break;
            }

            return false;
        }

        private bool TryGetTargetTerminalPair(
            SparkLevelTarget target,
            out SparkTerminal positive,
            out SparkTerminal negative)
        {
            positive = null;
            negative = null;

            if (target == null)
                return false;

            if (target.TargetTerminal != null)
            {
                positive = target.TargetTerminal;

                // A terminal-only target has no guaranteed second terminal.
                // Do not guess a sibling as its negative terminal.
                return false;
            }

            if (target.TargetComponent == null)
                return false;

            SparkTerminal[] terminals =
                target.TargetComponent
                    .GetComponentsInChildren<SparkTerminal>(true);

            if (terminals == null ||
                terminals.Length < 2)
            {
                return false;
            }

            // Match the same two-terminal orientation convention used by
            // the existing electrical model: first two child terminals.
            positive = terminals[0];
            negative = terminals[1];

            return positive != null &&
                   negative != null &&
                   positive != negative;
        }

        private bool IsUsableTopologyConnection(
            SparkCircuitConnection connection)
        {
            if (connection == null ||
                !connection.IsValid)
            {
                return false;
            }

            // Avoid a compile-time dependency on the exact enum declaration
            // while preserving the existing Probe semantics.
            return !string.Equals(
                connection.Kind.ToString(),
                "Probe",
                StringComparison.OrdinalIgnoreCase);
        }

        private SparkTerminal GetOtherTerminal(
            SparkCircuitConnection connection,
            SparkTerminal current)
        {
            if (connection == null ||
                current == null)
            {
                return null;
            }

            if (connection.A == current)
                return connection.B;

            if (connection.B == current)
                return connection.A;

            return null;
        }

        private SparkLevelTarget GetPrimaryTarget(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return null;
            }

            for (int i = 0; i < level.Targets.Length; i++)
            {
                if (level.Targets[i] != null)
                    return level.Targets[i];
            }

            return null;
        }

        private bool IsTargetElectricallyConducting(
            SparkLevelTarget target)
        {
            if (target == null)
                return false;

            if (target.TargetComponent != null)
            {
                SparkElectricalState state =
                    target.TargetComponent.ElectricalState;

                if (state.Conduction ==
                    SparkConductionState.Conducting)
                {
                    return true;
                }

                return Mathf.Abs(state.Current) > 0.000001f;
            }

            if (target.TargetTerminal != null)
            {
                SparkTerminalElectricalState state =
                    target.TargetTerminal.ElectricalState;

                return Mathf.Abs(state.Current) > 0.000001f;
            }

            return false;
        }

        private bool IsTargetShorted(
            SparkLevelTarget target)
        {
            if (target == null ||
                target.TargetComponent == null)
            {
                return false;
            }

            SparkElectricalState state =
                target.TargetComponent.ElectricalState;

            if (Mathf.Abs(state.Current) <= 0.000001f)
                return false;

            return Mathf.Abs(state.Voltage) < 0.001f &&
                   Mathf.Abs(state.Current) > 1f;
        }

        private bool HasOverload()
        {
            for (int i = 0; i < validSources.Count; i++)
            {
                SparkPowerSupply supply =
                    validSources[i].Supply;

                if (supply == null)
                    continue;

                if (supply.IsOverloaded)
                    return true;
            }

            return false;
        }

        private void EvaluateFailureConditions(
            SparkLevelDefinition level)
        {
            // Highest priority: physical/electrical short.
            if (sourceShorted &&
                level.RejectShortCircuit)
            {
                FailCurrentLevel(
                    "Short circuit detected between the configured power-source terminals.");

                return;
            }

            if (targetShorted &&
                level.RejectTargetShort)
            {
                FailCurrentLevel(
                    "Target terminals are electrically shorted.");

                return;
            }

            // Wrong polarity / wrong target-side connection is a topology
            // error, not a successful incomplete circuit.
            if (wrongConnection &&
                level.FailureMode ==
                    SparkLevelDefinition.LevelFailureMode.InvalidConnection)
            {
                FailCurrentLevel(
                    "Invalid connection: target polarity is connected incorrectly.");

                return;
            }

            if (overloaded &&
                level.FailureMode ==
                    SparkLevelDefinition.LevelFailureMode.Overload)
            {
                FailCurrentLevel(
                    "Electrical overload detected.");

                return;
            }

            if (level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.InvalidConnection)
            {
                // Only fail for an actually wrong connection. An untouched
                // or incomplete circuit should remain Playing.
                if (HasInvalidConfiguredTargetConnection(level))
                {
                    FailCurrentLevel(
                        "Invalid or incomplete target connection.");

                    return;
                }
            }

            if (solverEvaluationFailed)
            {
                FailCurrentLevel(
                    "Electrical solver failed to converge.");

                return;
            }
        }

        private bool HasInvalidConfiguredTargetConnection(
            SparkLevelDefinition level)
        {
            if (level == null ||
                validSources.Count == 0)
            {
                return false;
            }

            SparkLevelTarget target =
                GetPrimaryTarget(level);

            SparkTerminal targetPositive;
            SparkTerminal targetNegative;

            if (!TryGetTargetTerminalPair(
                    target,
                    out targetPositive,
                    out targetNegative))
            {
                return false;
            }

            bool positivePath =
                positiveReachable.Contains(targetPositive);

            bool negativePath =
                negativeReachable.Contains(targetNegative);

            bool reversedPositive =
                positiveReachable.Contains(targetNegative);

            bool reversedNegative =
                negativeReachable.Contains(targetPositive);

            if (reversedPositive ||
                reversedNegative)
            {
                return true;
            }

            // If one side is connected and the other is not, it is an
            // incomplete connection, not automatically a failure. This
            // preserves the player's ability to build the circuit.
            return false;
        }

        // ============================================================
        // CIRCUIT / SOLVER EVENTS
        // ============================================================

        private void Subscribe()
        {
            if (subscribed)
                return;

            if (circuitSystem != null)
            {
                circuitSystem.ConnectionCreated +=
                    OnConnectionChanged;

                circuitSystem.ConnectionRemoved +=
                    OnConnectionChanged;

                circuitSystem.TopologyChanged +=
                    OnTopologyChanged;
            }

            if (electricalSolver != null)
            {
                electricalSolver.SolveCompleted +=
                    OnSolveCompleted;

                electricalSolver.SolveFailed +=
                    OnSolveFailed;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (circuitSystem != null)
            {
                circuitSystem.ConnectionCreated -=
                    OnConnectionChanged;

                circuitSystem.ConnectionRemoved -=
                    OnConnectionChanged;

                circuitSystem.TopologyChanged -=
                    OnTopologyChanged;
            }

            if (electricalSolver != null)
            {
                electricalSolver.SolveCompleted -=
                    OnSolveCompleted;

                electricalSolver.SolveFailed -=
                    OnSolveFailed;
            }

            subscribed = false;
        }

        private void OnConnectionChanged(
            SparkCircuitConnection connection)
        {
            evaluationQueued = true;
        }

        private void OnTopologyChanged()
        {
            evaluationQueued = true;
        }

        private void OnSolveCompleted()
        {
            if (!evaluating)
                evaluationQueued = true;
        }

        private void OnSolveFailed()
        {
            solverEvaluationFailed = true;

            if (!evaluating)
                evaluationQueued = true;
        }

        // ============================================================
        // TOPOLOGY VERSION FALLBACK
        // ============================================================

        private void DetectTopologyVersionChange()
        {
            if (circuitSystem == null)
                return;

            int version =
                circuitSystem.TopologyVersion;

            if (version == lastTopologyVersion)
                return;

            lastTopologyVersion = version;

            evaluationQueued = true;

            LogVerbose(
                $"Topology changed. Version = {version}");
        }

        private void CaptureTopologyVersion()
        {
            lastTopologyVersion =
                circuitSystem != null
                    ? circuitSystem.TopologyVersion
                    : -1;
        }

        // ============================================================
        // PUBLIC HELPERS
        // ============================================================

        public SparkLevelDefinition GetLevel(int index)
        {
            if (levels == null)
                return null;

            if (index < 0 ||
                index >= levels.Length)
            {
                return null;
            }

            return levels[index];
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (circuitSystem == null)
            {
                circuitSystem =
                    FindFirstObjectByType<SparkCircuitSystem>();
            }

            if (electricalSolver == null)
            {
                electricalSolver =
                    FindFirstObjectByType<SparkElectricalSolver>();
            }
        }

        // ============================================================
        // DIAGNOSTICS
        // ============================================================

        private void Log(string message)
        {
            if (!debugLogging)
                return;

            Debug.Log(
                $"[LevelGamePlayManager] {message}",
                this);
        }

        private void LogVerbose(string message)
        {
            if (!debugLogging ||
                !verboseLogging)
            {
                return;
            }

            Debug.Log(
                $"[LevelGamePlayManager] {message}",
                this);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            if (levels == null)
                return;

            if (levels.Length == 0)
            {
                activeLevelIndex = 0;
                return;
            }

            activeLevelIndex =
                Mathf.Clamp(
                    activeLevelIndex,
                    0,
                    levels.Length - 1);

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null)
                    levels[i].Normalize();
            }
        }

        // ============================================================
        // RUNTIME DATA
        // ============================================================

        private readonly struct PowerSourceRuntime
        {
            public readonly SparkLevelDefinition.PowerSourceDefinition
                Definition;

            public readonly SparkPowerSupply Supply;

            public PowerSourceRuntime(
                SparkLevelDefinition.PowerSourceDefinition definition,
                SparkPowerSupply supply)
            {
                Definition = definition;
                Supply = supply;
            }
        }
    }
}
