
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
        // ============================================================
        // RUNTIME STATE
        // ============================================================

        public enum LevelRuntimeState
        {
            Uninitialized,
            Ready,
            Playing,
            Completed,
            Failed
        }

        // ============================================================
        // TARGET RUNTIME DATA
        // ============================================================

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

        // ============================================================
        // CORE SYSTEMS
        // ============================================================

        [Header("Core Systems")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [SerializeField]
        private SparkElectricalSolver electricalSolver;

        // ============================================================
        // SCENE BINDINGS
        // ============================================================

        [Header("Scene Bindings")]
        [Tooltip(
            "Scene-side bindings that resolve the IDs stored in " +
            "SparkLevelDefinition assets to actual scene objects.")]
        [SerializeField]
        private SparkLevelSceneBindings sceneBindings;

        // ============================================================
        // LEVELS
        // ============================================================

        [Header("Levels")]
        [SerializeField]
        private SparkLevelDefinition[] levels;

        [Min(0)]
        [SerializeField]
        private int activeLevelIndex;

        // ============================================================
        // RUNTIME SETTINGS
        // ============================================================

        [Header("Runtime")]
        [SerializeField]
        private LevelRuntimeState runtimeState =
            LevelRuntimeState.Uninitialized;

        [SerializeField]
        private bool autoStartFirstLevel = true;

        [SerializeField]
        private bool autoEvaluate = true;

        [SerializeField]
        private bool autoAdvanceOnCompletion;

        [SerializeField]
        private bool preventInteractionAfterCompletion;

        // ============================================================
        // DEBUG
        // ============================================================

        [Header("Debug")]
        [SerializeField]
        private bool debugLogging;

        [SerializeField]
        private bool verboseLogging;

        [SerializeField]
        private bool showRuntimeState = true;

        // ============================================================
        // EVALUATOR
        // ============================================================

        private SparkLevelEvaluator evaluator;

        // ============================================================
        // EVALUATION CONTROL
        // ============================================================

        private bool evaluationQueued;
        private bool evaluating;
        private bool subscribed;
        private bool solverEvaluationFailed;

        // ============================================================
        // RUNTIME RESULT
        // ============================================================

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

        private string activeSourceName = string.Empty;
        private string lastFailureReason = string.Empty;
        private string lastEvaluationMessage = string.Empty;

        private SparkLevelValidationResult validationResult;

        // ============================================================
        // COMPATIBILITY RUNTIME DATA
        // ============================================================

        private readonly List<TargetRuntimeResult> targetResults =
            new List<TargetRuntimeResult>(16);

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public SparkCircuitSystem CircuitSystem =>
            circuitSystem;

        public SparkElectricalSolver ElectricalSolver =>
            electricalSolver;

        public SparkLevelSceneBindings SceneBindings =>
            sceneBindings;

        public SparkLevelDefinition[] Levels =>
            levels;

        public int ActiveLevelIndex =>
            activeLevelIndex;

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

        public bool PreventInteractionAfterCompletion =>
            preventInteractionAfterCompletion;

        public int SatisfiedTargetCount =>
            satisfiedTargetCount;

        public int ActiveTargetCount =>
            ActiveLevel != null
                ? ActiveLevel.TargetCount
                : 0;

        public bool HasValidPowerSource =>
            hasValidPowerSource;

        public bool ClosedReturn =>
            closedReturn;

        public bool SourceShorted =>
            sourceShorted;

        public bool TargetShorted =>
            targetShorted;

        public bool WrongConnection =>
            wrongConnection;

        public bool Overloaded =>
            overloaded;

        public float TargetVoltage =>
            targetVoltage;

        public float TargetCurrent =>
            targetCurrent;

        public float TargetPower =>
            targetPower;

        public string ActiveSourceName =>
            activeSourceName;

        public string LastFailureReason =>
            lastFailureReason;

        public string LastEvaluationMessage =>
            lastEvaluationMessage;

        public SparkLevelValidationResult ValidationResult =>
            validationResult;

        public IReadOnlyList<TargetRuntimeResult> TargetResults =>
            targetResults;

        public bool ShowRuntimeState =>
            showRuntimeState;

        // ============================================================
        // EVENTS
        // ============================================================

        public event Action<SparkLevelDefinition> LevelStarted;

        public event Action<SparkLevelDefinition> LevelCompleted;

        public event Action<SparkLevelDefinition, string> LevelFailed;

        public event Action<SparkLevelDefinition> LevelReset;

        public event Action<SparkLevelDefinition>
            LevelEvaluationChanged;

        // ============================================================
        // UNITY LIFECYCLE
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
            EnsureEvaluator();

            runtimeState =
                LevelRuntimeState.Ready;

            Subscribe();

            if (autoStartFirstLevel &&
                LevelCount > 0)
            {
                StartLevel(activeLevelIndex);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureEvaluator();
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
            SparkLevelDefinition level =
                GetLevel(index);

            if (level == null)
            {
                Log(
                    $"Cannot start level {index}: level not found.");

                return false;
            }

            EnsureEvaluator();

            if (evaluator == null)
            {
                Log(
                    $"Cannot start level {index}: " +
                    "level evaluator is unavailable.");

                return false;
            }

            if (sceneBindings == null)
            {
                Log(
                    $"Cannot start level {index}: " +
                    "SparkLevelSceneBindings is missing.");

                return false;
            }

            activeLevelIndex = index;

            // IMPORTANT:
            // SparkLevelDefinition is now a ScriptableObject asset.
            // Never mutate/normalize the asset at runtime.
            //
            // Validation and runtime object resolution are handled
            // by SparkLevelEvaluator + SparkLevelSceneBindings.

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

            targetResults.Clear();

            validationResult =
                new SparkLevelValidationResult(
                    SparkLevelValidationState.Playing,
                    "Level started.",
                    0,
                    level.TargetCount,
                    0f,
                    0f,
                    0f,
                    string.Empty,
                    null,
                    null);

            runtimeState =
                LevelRuntimeState.Playing;

            // Scene outputs belong to the scene binding component,
            // not to the ScriptableObject asset.
            SetLevelOutputs(
                false,
                false);

            QueueEvaluation();

            LevelStarted?.Invoke(level);

            Log(
                $"Started level {index}: " +
                $"{level.DisplayName}");

            return true;
        }

        public bool NextLevel()
        {
            int next =
                activeLevelIndex + 1;

            if (next >= LevelCount)
                return false;

            return StartLevel(next);
        }

        public bool PreviousLevel()
        {
            int previous =
                activeLevelIndex - 1;

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

        // ============================================================
        // COMPLETE LEVEL
        // ============================================================

        public bool CompleteCurrentLevel()
        {
            return CompleteCurrentLevel(default);
        }

        private bool CompleteCurrentLevel(
            SparkLevelEvaluation evaluation)
        {
            SparkLevelDefinition level =
                ActiveLevel;

            if (level == null)
                return false;

            if (currentLevelCompleted)
                return true;

            currentLevelCompleted = true;
            currentLevelFailed = false;

            runtimeState =
                LevelRuntimeState.Completed;

            SetLevelOutputs(
                true,
                false);

            string message =
                string.IsNullOrWhiteSpace(
                    evaluation.Message)
                    ? "Level completed."
                    : evaluation.Message;

            lastEvaluationMessage =
                message;

            SetValidationResult(
                SparkLevelValidationState.Completed,
                message,
                evaluation);

            LevelCompleted?.Invoke(level);

            LevelEvaluationChanged?.Invoke(level);

            Log(
                $"LEVEL COMPLETE: " +
                $"{level.DisplayName}");

            if (autoAdvanceOnCompletion)
            {
                int next =
                    activeLevelIndex + 1;

                if (next < LevelCount)
                {
                    StartLevel(next);
                }
            }

            return true;
        }

        // ============================================================
        // FAIL LEVEL
        // ============================================================

        public void FailCurrentLevel(
            string reason)
        {
            SparkLevelDefinition level =
                ActiveLevel;

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

            lastEvaluationMessage =
                lastFailureReason;

            runtimeState =
                LevelRuntimeState.Failed;

            SetLevelOutputs(
                false,
                true);

            SetValidationResult(
                SparkLevelValidationState.InvalidConfiguration,
                lastFailureReason);

            LevelFailed?.Invoke(
                level,
                lastFailureReason);

            LevelEvaluationChanged?.Invoke(level);

            Log(
                $"LEVEL FAILED: " +
                $"{level.DisplayName} | " +
                $"{lastFailureReason}");
        }

        private void FailCurrentLevel(
            string reason,
            SparkLevelEvaluation evaluation)
        {
            SparkLevelDefinition level =
                ActiveLevel;

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

            lastEvaluationMessage =
                evaluation.Message ?? string.Empty;

            runtimeState =
                LevelRuntimeState.Failed;

            SetLevelOutputs(
                false,
                true);

            SparkLevelValidationState state =
                ConvertFailureValidationState(
                    evaluation);

            SetValidationResult(
                state,
                lastFailureReason,
                evaluation);

            LevelFailed?.Invoke(
                level,
                lastFailureReason);

            LevelEvaluationChanged?.Invoke(level);

            Log(
                $"LEVEL FAILED: " +
                $"{level.DisplayName} | " +
                $"{lastFailureReason} | " +
                $"Reason={evaluation.FailureReason}");
        }

        // ============================================================
        // RESET
        // ============================================================

        public void ResetLevel()
        {
            SparkLevelDefinition level =
                ActiveLevel;

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

            targetResults.Clear();

            runtimeState =
                LevelRuntimeState.Playing;

            SetLevelOutputs(
                false,
                false);

            SetValidationResult(
                SparkLevelValidationState.Playing,
                "Level reset.");

            QueueEvaluation();

            LevelReset?.Invoke(level);

            Log(
                $"Level reset: " +
                $"{level.DisplayName}");
        }

        // ============================================================
        // SCENE OUTPUT CONTROL
        // ============================================================

       private void SetLevelOutputs(
    bool success,
    bool failure)
{
    if (sceneBindings == null)
        return;

    SparkLevelDefinition level =
        ActiveLevel;

    if (level == null)
        return;

    SetOutputs(
        level.SuccessOutputIds,
        success);

    SetOutputs(
        level.FailureOutputIds,
        failure);
}


private void SetOutputs(
    string[] outputIds,
    bool active)
{
    if (outputIds == null ||
        outputIds.Length == 0)
    {
        return;
    }

    for (int i = 0;
         i < outputIds.Length;
         i++)
    {
        SetOutput(
            outputIds[i],
            active);
    }
}
private void SetOutput(
    string outputId,
    bool active)
{
    if (string.IsNullOrWhiteSpace(outputId))
        return;

    if (!sceneBindings.TryGetObject(
            outputId,
            out GameObject target))
    {
        LogVerbose(
            $"Scene output binding not found: " +
            $"'{outputId}'.");

        return;
    }

    if (target == null)
        return;

    target.SetActive(active);
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

            if (runtimeState !=
                LevelRuntimeState.Playing)
            {
                return;
            }

            evaluationQueued = true;
        }

        // ============================================================
        // AUTHORITATIVE EVALUATION
        // ============================================================

        private void EvaluateActiveLevel()
        {
            if (evaluating)
                return;

            SparkLevelDefinition level =
                ActiveLevel;

            if (level == null)
            {
                currentLevelCompleted = false;
                currentLevelFailed = true;

                runtimeState =
                    LevelRuntimeState.Failed;

                SetValidationResult(
                    SparkLevelValidationState.InvalidConfiguration,
                    "No active level.");

                return;
            }

            if (sceneBindings == null)
            {
                currentLevelCompleted = false;
                currentLevelFailed = true;

                runtimeState =
                    LevelRuntimeState.Failed;

                SetValidationResult(
                    SparkLevelValidationState.InvalidConfiguration,
                    "SparkLevelSceneBindings is missing.");

                return;
            }

            if (evaluator == null)
            {
                EnsureEvaluator();

                if (evaluator == null)
                {
                    currentLevelCompleted = false;
                    currentLevelFailed = true;

                    runtimeState =
                        LevelRuntimeState.Failed;

                    SetValidationResult(
                        SparkLevelValidationState.InvalidConfiguration,
                        "Level evaluator is missing.");

                    return;
                }
            }

            evaluating = true;
            solverEvaluationFailed = false;

            try
            {
                // ----------------------------------------------------
                // SOLVE ELECTRICAL NETWORK
                // ----------------------------------------------------

                if (electricalSolver != null)
                {
                    electricalSolver.SolveNow();

                    if (solverEvaluationFailed)
                    {
                        SparkLevelEvaluation solverFailure =
                            SparkLevelEvaluation.Create(
                                SparkLevelEvaluationStatus.Failed,
                                SparkLevelFailureReason.InvalidConfiguration,
                                "Electrical solver failed.");

                        ApplyEvaluation(
                            solverFailure,
                            level);

                        FailCurrentLevel(
                            solverFailure.Message,
                            solverFailure);

                        return;
                    }
                }

                // ----------------------------------------------------
                // AUTHORITATIVE LEVEL EVALUATION
                // ----------------------------------------------------

                SparkLevelEvaluation evaluation =
                    evaluator.Evaluate(level);

                // ----------------------------------------------------
                // COPY RESULT INTO MANAGER
                // IMPORTANT:
                // ApplyEvaluation DOES NOT own completion/failure
                // flags.
                // ----------------------------------------------------

                ApplyEvaluation(
                    evaluation,
                    level);

                LogVerbose(
                    $"EVALUATION | " +
                    $"Status={evaluation.Status} | " +
                    $"Reason={evaluation.FailureReason} | " +
                    $"Targets=" +
                    $"{evaluation.SatisfiedTargetCount}/" +
                    $"{evaluation.TotalTargetCount} | " +
                    $"Closed={evaluation.ClosedReturn} | " +
                    $"SourceShort=" +
                    $"{evaluation.SourceShorted} | " +
                    $"TargetShort=" +
                    $"{evaluation.TargetShorted} | " +
                    $"Overload=" +
                    $"{evaluation.Overloaded} | " +
                    $"V={evaluation.TargetVoltage:F3} | " +
                    $"I={evaluation.TargetCurrent:F3} | " +
                    $"P={evaluation.TargetPower:F3}");

                // ----------------------------------------------------
                // FINAL GAMEPLAY STATE
                // ----------------------------------------------------

                if (evaluation.IsCompleted)
                {
                    CompleteCurrentLevel(
                        evaluation);
                }
                else if (evaluation.IsFailed)
                {
                    FailCurrentLevel(
                        evaluation.Message,
                        evaluation);
                }
            }
            finally
            {
                evaluating = false;
                evaluationQueued = false;
            }
        }

        // ============================================================
        // APPLY EVALUATOR RESULT
        // ============================================================

        private void ApplyEvaluation(
            SparkLevelEvaluation evaluation,
            SparkLevelDefinition level)
        {
            if (level == null)
                return;

            // --------------------------------------------------------
            // ELECTRICAL STATE
            // --------------------------------------------------------

            hasValidPowerSource =
                !string.IsNullOrWhiteSpace(
                    evaluation.ActiveSource);

            closedReturn =
                evaluation.ClosedReturn;

            sourceShorted =
                evaluation.SourceShorted;

            targetShorted =
                evaluation.TargetShorted;

            overloaded =
                evaluation.Overloaded;

            targetVoltage =
                evaluation.TargetVoltage;

            targetCurrent =
                evaluation.TargetCurrent;

            targetPower =
                evaluation.TargetPower;

            activeSourceName =
                evaluation.ActiveSource ??
                string.Empty;

            satisfiedTargetCount =
                evaluation.SatisfiedTargetCount;

            // --------------------------------------------------------
            // FAILURE INFORMATION
            // --------------------------------------------------------

            lastFailureReason =
                evaluation.HasFailureReason
                    ? evaluation.FailureReason.ToString()
                    : string.Empty;

            lastEvaluationMessage =
                evaluation.Message ??
                string.Empty;

            wrongConnection =
                evaluation.FailureReason ==
                SparkLevelFailureReason.InvalidConnection;

            // --------------------------------------------------------
            // IMPORTANT
            //
            // DO NOT DO:
            //
            // currentLevelCompleted = evaluation.IsCompleted;
            // currentLevelFailed = evaluation.IsFailed;
            //
            // CompleteCurrentLevel() and FailCurrentLevel()
            // own those flags.
            // --------------------------------------------------------

            targetResults.Clear();

            SparkLevelValidationState validationState =
                ConvertValidationState(
                    evaluation);

            SetValidationResult(
                validationState,
                lastEvaluationMessage,
                evaluation);
        }

        // ============================================================
        // VALIDATION STATE CONVERSION
        // ============================================================

        private SparkLevelValidationState
            ConvertValidationState(
                SparkLevelEvaluation evaluation)
        {
            if (solverEvaluationFailed)
            {
                return SparkLevelValidationState.SolverFault;
            }

            switch (evaluation.FailureReason)
            {
                case SparkLevelFailureReason.TargetShortCircuit:
                    return SparkLevelValidationState.TargetShort;

                case SparkLevelFailureReason.SourceShortCircuit:
                    return SparkLevelValidationState.ShortCircuit;

                case SparkLevelFailureReason.Overload:
                    return SparkLevelValidationState.Overload;

                case SparkLevelFailureReason.InvalidConnection:
                    return SparkLevelValidationState.WrongConnection;

                case SparkLevelFailureReason.InvalidConfiguration:
                case SparkLevelFailureReason.NoLevel:
                    return SparkLevelValidationState.InvalidConfiguration;

                case SparkLevelFailureReason.NoValidPowerSource:
                case SparkLevelFailureReason.OpenCircuit:
                case SparkLevelFailureReason.InsufficientVoltage:
                case SparkLevelFailureReason.InvalidTarget:
                    return SparkLevelValidationState.Playing;

                case SparkLevelFailureReason.None:
                default:

                    if (evaluation.IsCompleted)
                    {
                        return SparkLevelValidationState.Completed;
                    }

                    return SparkLevelValidationState.Playing;
            }
        }

        private SparkLevelValidationState
            ConvertFailureValidationState(
                SparkLevelEvaluation evaluation)
        {
            if (solverEvaluationFailed)
            {
                return SparkLevelValidationState.SolverFault;
            }

            switch (evaluation.FailureReason)
            {
                case SparkLevelFailureReason.InvalidConnection:
                    return SparkLevelValidationState.WrongConnection;

                case SparkLevelFailureReason.TargetShortCircuit:
                    return SparkLevelValidationState.TargetShort;

                case SparkLevelFailureReason.SourceShortCircuit:
                    return SparkLevelValidationState.ShortCircuit;

                case SparkLevelFailureReason.Overload:
                    return SparkLevelValidationState.Overload;

                case SparkLevelFailureReason.InvalidConfiguration:
                case SparkLevelFailureReason.NoLevel:
                    return SparkLevelValidationState.InvalidConfiguration;

                case SparkLevelFailureReason.NoValidPowerSource:
                case SparkLevelFailureReason.OpenCircuit:
                case SparkLevelFailureReason.InsufficientVoltage:
                case SparkLevelFailureReason.InvalidTarget:
                    return SparkLevelValidationState.Playing;

                case SparkLevelFailureReason.None:
                default:
                    return SparkLevelValidationState.Playing;
            }
        }

        // ============================================================
        // VALIDATION RESULT
        // ============================================================

        private void SetValidationResult(
            SparkLevelValidationState state,
            string message)
        {
            validationResult =
                new SparkLevelValidationResult(
                    state,
                    string.IsNullOrWhiteSpace(message)
                        ? string.Empty
                        : message,
                    satisfiedTargetCount,
                    ActiveLevel != null
                        ? ActiveLevel.TargetCount
                        : 0,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSourceName,
                    null,
                    null);
        }

        private void SetValidationResult(
            SparkLevelValidationState state,
            string message,
            SparkLevelEvaluation evaluation)
        {
            validationResult =
                new SparkLevelValidationResult(
                    state,
                    string.IsNullOrWhiteSpace(message)
                        ? string.Empty
                        : message,
                    evaluation.SatisfiedTargetCount,
                    evaluation.TotalTargetCount,
                    evaluation.TargetVoltage,
                    evaluation.TargetCurrent,
                    evaluation.TargetPower,
                    evaluation.ActiveSource,
                    evaluation.AffectedTerminal,
                    evaluation.AffectedTarget);
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
            if (runtimeState !=
                LevelRuntimeState.Playing)
            {
                return;
            }

            if (!connection.IsValid)
                return;

            QueueEvaluation();
        }

        private void OnTopologyChanged()
        {
            if (runtimeState !=
                LevelRuntimeState.Playing)
            {
                return;
            }

            QueueEvaluation();
        }

        private void OnSolveCompleted()
        {
            if (evaluating)
                return;

            if (runtimeState !=
                LevelRuntimeState.Playing)
            {
                return;
            }

            QueueEvaluation();
        }

        private void OnSolveFailed()
        {
            solverEvaluationFailed = true;

            if (evaluating)
                return;

            if (runtimeState !=
                LevelRuntimeState.Playing)
            {
                return;
            }

            QueueEvaluation();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (circuitSystem == null)
            {
                circuitSystem =
                    FindFirstObjectByType<
                        SparkCircuitSystem>();
            }

            if (electricalSolver == null)
            {
                electricalSolver =
                    FindFirstObjectByType<
                        SparkElectricalSolver>();
            }

            if (sceneBindings == null)
            {
                sceneBindings =
                    GetComponent<SparkLevelSceneBindings>();

                if (sceneBindings == null)
                {
                    sceneBindings =
                        GetComponentInParent<
                            SparkLevelSceneBindings>();
                }
            }
        }

        private void EnsureEvaluator()
        {
            if (evaluator != null)
                return;

            if (circuitSystem == null)
                return;

            if (sceneBindings == null)
                return;

            evaluator =
                new SparkLevelEvaluator(
                    circuitSystem,
                    sceneBindings);
        }

        // ============================================================
        // LEVEL LOOKUP
        // ============================================================

        public SparkLevelDefinition GetLevel(
            int index)
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
        // DEBUG
        // ============================================================

        private void Log(
            string message)
        {
            if (!debugLogging)
                return;

            Debug.Log(
                $"[LevelGamePlayManager] {message}",
                this);
        }

        private void LogVerbose(
            string message)
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
        // UNITY VALIDATION
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

            // IMPORTANT:
            // Do not call level.Normalize() here.
            //
            // SparkLevelDefinition is a reusable ScriptableObject
            // asset. Its data must remain asset-owned and scene-
            // independent.
            //
            // Asset validation belongs to SparkLevelDefinition's
            // own OnValidate().
        }
    }
}
