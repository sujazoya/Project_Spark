
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

        [Header("Level Layout Loading")]
        [SerializeField]
        private SparkLevelLayoutLoader layoutLoader;

        private bool pendingAutoAdvance;

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

        private bool forceReloadOnNextStart;

        private string activeSourceName = string.Empty;
        private string lastFailureReason = string.Empty;
        private string lastEvaluationMessage = string.Empty;

        private SparkLevelValidationResult validationResult;




                [Header("Progression Access")]
        [SerializeField]
        private SparkLevelProgression levelProgression;

        [SerializeField]
        private bool enforceProgressionAccess = true;

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


            public SparkLevelProgression LevelProgression =>
    levelProgression;

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

    if (enforceProgressionAccess)
    {
        if (levelProgression == null ||
            !levelProgression.CanPlayLevel(index))
        {
            Log($"Level {index} is locked.");
            return false;
        }
    }

    // Capture the restart request.
    bool forceReload = forceReloadOnNextStart;
    forceReloadOnNextStart = false;

    // PrepareLevelLayout must accept the optional
    // bool forceReload parameter.
    if (!PrepareLevelLayout(level, forceReload))
    {
        Log(
            $"Cannot start level {index}: " +
            "level layout could not be prepared.");

        return false;
    }

    activeLevelIndex = index;

    // Reset level completion and failure state.
    currentLevelCompleted = false;
    currentLevelFailed = false;
    pendingAutoAdvance = false;

    satisfiedTargetCount = 0;

    // Reset electrical validation state.
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

    // Reset scene outputs for the newly loaded layout.
    SetLevelOutputs(false, false);

    // Evaluate the new level.
    QueueEvaluation();

    LevelStarted?.Invoke(level);

    Log(
        $"Started level {index}: " +
        $"{level.DisplayName}");

    return true;
}
 

public bool RestartLevel()
{
    SparkLevelDefinition level = ActiveLevel;

    if (level == null)
    {
        Debug.LogError(
            "[LevelGamePlayManager] Cannot restart: " +
            "no active level.",
            this);

        return false;
    }

    if (layoutLoader == null ||
        circuitSystem == null ||
        electricalSolver == null)
    {
        Debug.LogError(
            "[LevelGamePlayManager] Cannot restart: " +
            "required layout or electrical system is missing.",
            this);

        return false;
    }

    // Force PrepareLevelLayout() to create a fresh
    // instance even though the level is unchanged.
    forceReloadOnNextStart = true;

    // StartLevel() reloads the layout, refreshes the
    // electrical solver, rebuilds the evaluator,
    // and resets the gameplay state.
    bool restarted = StartLevel(activeLevelIndex);

    // Ensure a failed restart request cannot affect
    // a later StartLevel() call.
    forceReloadOnNextStart = false;

    if (!restarted)
    {
        Debug.LogError(
            "[LevelGamePlayManager] Failed to restart level: " +
            level.LevelId,
            this);
    }

    return restarted;
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
    SparkLevelDefinition level = ActiveLevel;

    if (level == null)
        return false;

    // Prevent duplicate completion events.
    if (currentLevelCompleted)
        return true;

    // ============================================================
    // UPDATE RUNTIME STATE
    // ============================================================

    currentLevelCompleted = true;
    currentLevelFailed = false;

    runtimeState = LevelRuntimeState.Completed;

    // ============================================================
    // ACTIVATE SUCCESS OUTPUTS
    // ============================================================

    SetLevelOutputs(
        true,
        false);

    // ============================================================
    // COMPLETION MESSAGE
    // ============================================================

    string message =
        string.IsNullOrWhiteSpace(evaluation.Message)
            ? "Level completed."
            : evaluation.Message;

    lastEvaluationMessage = message;
    lastFailureReason = string.Empty;

    // ============================================================
    // VALIDATION RESULT
    // ============================================================

    SetValidationResult(
        SparkLevelValidationState.Completed,
        message,
        evaluation);

    // ============================================================
    // NOTIFY PROGRESSION AND UI
    // ============================================================

    // SparkLevelProgression listens to this event to save
    // completion and unlock the next eligible level.
    LevelCompleted?.Invoke(level);

    LevelEvaluationChanged?.Invoke(level);

    Log(
        $"LEVEL COMPLETE: {level.DisplayName}");

    // ============================================================
    // DEFER AUTOMATIC ADVANCEMENT
    // ============================================================

    if (autoAdvanceOnCompletion)
    {
        if (evaluating)
        {
            // Wait until the current electrical evaluation ends.
            pendingAutoAdvance = true;
        }
        else
        {
            // Safe when completion occurs outside evaluation.
            NextLevel();
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
        // A non-converged/incomplete electrical network is not
        // automatically a gameplay failure.
        //
        // The level evaluator will determine whether the current
        // circuit is incomplete, invalid, or actually completed.
        solverEvaluationFailed = false;
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

                if (pendingAutoAdvance)
                {
                    pendingAutoAdvance = false;
                    NextLevel();
                }
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
            if (levelProgression == null)
            {
                levelProgression =
                    GetComponent<SparkLevelProgression>();

                if (levelProgression == null)
                {
                    levelProgression =
                        FindFirstObjectByType<SparkLevelProgression>();
                }
            }
            if (layoutLoader == null)
            {
                layoutLoader =
                    FindFirstObjectByType<SparkLevelLayoutLoader>();
            }
        }
        
       

       

private bool PrepareLevelLayout(
    SparkLevelDefinition level,
    bool forceReload = false)
    {
    if (level == null ||
        layoutLoader == null ||
        circuitSystem == null ||
        electricalSolver == null)
    {
        Debug.LogError(
            "[LevelGamePlayManager] Cannot prepare layout. " +
            "Missing level, layout loader, circuit system, " +
            "or electrical solver.",
            this);

        return false;
    }

   bool needsNewLayout =
    forceReload ||
    layoutLoader.ActiveLayout == null ||
    layoutLoader.ActiveLayout.LevelDefinition != level;

    if (needsNewLayout)
    {
        // Validate the requested layout before replacing
        // the currently active level.
        if (!layoutLoader.CanLoadLevel(
                level,
                out string error))
        {
            Debug.LogError(
                "[LevelGamePlayManager] " + error,
                this);

            return false;
        }
      

        // Load and validate the new layout.
        if (!layoutLoader.LoadLevel(level))
        {
            Debug.LogError(
                "[LevelGamePlayManager] Failed to load layout: " +
                level.LevelId,
                this);

            return false;
        }      

        // Clear cached electrical results and refresh
        // component discovery for the new layout.
        electricalSolver.NotifyLevelLayoutChanged();
    }

    SparkLevelSceneBindings newBindings =
        layoutLoader.ActiveBindings;

    if (newBindings == null)
    {
        Debug.LogError(
            "[LevelGamePlayManager] Active layout has " +
            "no SparkLevelSceneBindings.",
            this);

        return false;
    }

    // Use the scene bindings from the active layout.
    sceneBindings = newBindings;

    // Rebuild the evaluator for the active level.
    evaluator = new SparkLevelEvaluator(
        circuitSystem,
        sceneBindings);

    return true;
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

        
        public bool NextLevel()
        {
            int nextIndex = activeLevelIndex + 1;

            // Check whether another level exists.
            SparkLevelDefinition nextLevel =
                GetLevel(nextIndex);

            if (nextLevel == null)
            {
                Debug.Log(
                    "[LevelGamePlayManager] Already at the last level.",
                    this);

                return false;
            }

            // StartLevel handles progression locks,
            // prefab loading, solver refresh and evaluation.
            return StartLevel(nextIndex);
        }

        public bool PreviousLevel()
        {
            int previousIndex = activeLevelIndex - 1;

            // Prevent navigating before Level 1.
            if (previousIndex < 0)
            {
                Debug.Log(
                    "[LevelGamePlayManager] Already at the first level.",
                    this);

                return false;
            }

            return StartLevel(previousIndex);
        }
        public void OnNextLevelButtonClicked()
        {
            NextLevel();
        }

        public void OnPreviousLevelButtonClicked()
        {
            PreviousLevel();
        }

    }
}
