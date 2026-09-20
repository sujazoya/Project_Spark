using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Evaluates a SparkLevelDefinition against the current
    /// circuit topology and solved electrical state.
    ///
    /// This class contains reusable level rules.
    /// It does not control level progression or UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelEvaluator : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField] private SparkElectricalSolver electricalSolver;

        [Header("Diagnostics")]
        [SerializeField] private bool debugLogging;
        [SerializeField] private bool verboseLogging;

        private readonly List<PowerSourceRuntime> validSources =
            new List<PowerSourceRuntime>(8);

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>(32);

        private readonly List<TargetRuntimeResult> targetResults =
            new List<TargetRuntimeResult>(16);

        private readonly Queue<SparkTerminal> traversalQueue =
            new Queue<SparkTerminal>(32);

        private readonly HashSet<SparkTerminal> visitedTerminals =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> positiveReachable =
            new HashSet<SparkTerminal>();

        private readonly HashSet<SparkTerminal> negativeReachable =
            new HashSet<SparkTerminal>();

        private int satisfiedTargetCount;

        private bool hasValidPowerSource;
        private bool closedReturn;
        private bool sourceShorted;
        private bool targetShorted;
        private bool wrongConnection;
        private bool overloaded;
        private bool solverEvaluationFailed;

        private float targetVoltage;
        private float targetCurrent;
        private float targetPower;

        private string activeSourceName = string.Empty;

        public SparkCircuitSystem CircuitSystem =>
            circuitSystem;

        public SparkElectricalSolver ElectricalSolver =>
            electricalSolver;

        public int SatisfiedTargetCount =>
            satisfiedTargetCount;

        public int TargetCount =>
            targetResults.Count;

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

        public bool IsOverloaded =>
            overloaded;

        public float TargetVoltage =>
            targetVoltage;

        public float TargetCurrent =>
            targetCurrent;

        public float TargetPower =>
            targetPower;

        public string ActiveSourceName =>
            activeSourceName;

        public IReadOnlyList<TargetRuntimeResult> TargetResults =>
            targetResults;

        /// <summary>
        /// Evaluates the supplied level using the current solved state.
        ///
        /// The evaluator does not call CompleteCurrentLevel() or
        /// FailCurrentLevel(). The caller decides what to do with
        /// the returned result.
        /// </summary>
        public SparkLevelValidationResult Evaluate(
            SparkLevelDefinition level,
            bool solverFailed = false)
        {
            ResetEvaluation();

            solverEvaluationFailed = solverFailed;

            if (level == null)
            {
                return CreateResult(
                    SparkLevelValidationState.InvalidConfiguration,
                    "No level definition is assigned.",
                    null,
                    null);
            }

            level.Normalize();

            if (circuitSystem == null)
            {
                return CreateResult(
                    SparkLevelValidationState.InvalidConfiguration,
                    "Circuit system is not assigned.",
                    null,
                    null);
            }

            EvaluatePowerSources(level);
            EvaluateTargets(level);
            EvaluateTopologyState(level);
            EvaluateFailureConditions(level);

            if (HasFailure(level))
            {
                SparkLevelValidationState failureState =
                    DetermineFailureValidationState();

                string failureMessage =
                    BuildFailureMessage(failureState);

                return CreateResult(
                    failureState,
                    failureMessage,
                    GetAffectedTerminal(failureState, level),
                    GetAffectedTarget(failureState, level));
            }

            if (EvaluateCompletion(level))
            {
                return CreateResult(
                    SparkLevelValidationState.Completed,
                    BuildCompletedMessage(level),
                    null,
                    null);
            }

            SparkLevelValidationState playingState =
                DeterminePlayingValidationState();

            string message =
                BuildEvaluationMessage(level);

            return CreateResult(
                playingState,
                message,
                GetAffectedTerminal(playingState, level),
                GetAffectedTarget(playingState, level));
        }

        // ============================================================
        // RESET
        // ============================================================

        private void ResetEvaluation()
        {
            validSources.Clear();
            targetResults.Clear();

            positiveReachable.Clear();
            negativeReachable.Clear();

            traversalQueue.Clear();
            visitedTerminals.Clear();

            satisfiedTargetCount = 0;

            hasValidPowerSource = false;
            closedReturn = false;
            sourceShorted = false;
            targetShorted = false;
            wrongConnection = false;
            overloaded = false;
            solverEvaluationFailed = false;

            targetVoltage = 0f;
            targetCurrent = 0f;
            targetPower = 0f;

            activeSourceName = string.Empty;
        }

        // ============================================================
        // TARGETS
        // ============================================================

        private void EvaluateTargets(
            SparkLevelDefinition level)
        {
            SparkLevelTarget[] targets =
                level.Targets;

            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                SparkLevelTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.Normalize();

                bool satisfied =
                    target.Evaluate();

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
            SparkLevelDefinition.PowerSourceDefinition[] sources =
                level.PowerSources;

            if (sources == null ||
                sources.Length == 0)
            {
                return;
            }

            for (int i = 0;
                 i < sources.Length;
                 i++)
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

            hasValidPowerSource =
                validSources.Count > 0;

            if (validSources.Count > 0)
            {
                activeSourceName =
                    validSources[0]
                        .Definition
                        .SourceName;
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

            if (source == null ||
                source.PositiveTerminal == null ||
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

            Transform root =
                terminal.transform.root;

            if (root != null)
            {
                SparkPowerSupply[] supplies =
                    root.GetComponentsInChildren<SparkPowerSupply>(true);

                for (int i = 0;
                     i < supplies.Length;
                     i++)
                {
                    SparkPowerSupply supply =
                        supplies[i];

                    if (supply == null)
                        continue;

                    SparkTerminal[] terminals =
                        supply.GetComponentsInChildren<SparkTerminal>(true);

                    for (int j = 0;
                         j < terminals.Length;
                         j++)
                    {
                        if (terminals[j] == terminal)
                            return supply;
                    }
                }
            }

            return null;
        }

        // ============================================================
        // TOPOLOGY
        // ============================================================

        private void EvaluateTopologyState(
            SparkLevelDefinition level)
        {
            if (validSources.Count == 0)
                return;

            SparkLevelTarget primaryTarget =
                GetPrimaryTarget(level);

            BuildSourceReachability();

            sourceShorted =
                HasSourceTopologyShort(level);

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

                wrongConnection =
                    (reversedPositive ||
                     reversedNegative) &&
                    !targetShorted;

                closedReturn =
                    positivePath &&
                    negativePath &&
                    IsTargetElectricallyConducting(
                        primaryTarget);

                if (targetShorted)
                    closedReturn = false;
            }
            else
            {
                closedReturn =
                    IsTargetElectricallyConducting(
                        primaryTarget);

                targetShorted =
                    IsTargetShorted(primaryTarget);

                wrongConnection = false;
            }

            overloaded =
                HasOverload();

            LogVerbose(
                $"Topology: " +
                $"ClosedReturn={closedReturn}, " +
                $"SourceShort={sourceShorted}, " +
                $"TargetShort={targetShorted}, " +
                $"WrongConnection={wrongConnection}");
        }

        private void BuildSourceReachability()
        {
            positiveReachable.Clear();
            negativeReachable.Clear();

            for (int i = 0;
                 i < validSources.Count;
                 i++)
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

                if (!allowAnyConfiguredSource)
                    break;
            }
        }

        private bool allowAnyConfiguredSource;

        private void TraverseWireNetwork(
            SparkTerminal start,
            HashSet<SparkTerminal> visited)
        {
            if (start == null ||
                visited == null)
            {
                return;
            }

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

                    if (!IsUsableTopologyConnection(
                            connection))
                    {
                        continue;
                    }

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

                TryTraverseConductiveDevice(
                    current,
                    visited);
            }
        }

        private bool TryTraverseConductiveDevice(
            SparkTerminal current,
            HashSet<SparkTerminal> visited)
        {
            if (current == null ||
                visited == null)
            {
                return false;
            }

            SparkSwitch sparkSwitch =
                ResolveSwitchFromTerminal(current);

            if (sparkSwitch == null)
                return false;

            if (!sparkSwitch.ElectricalEnabled)
                return false;

            if (!sparkSwitch.IsConducting)
                return false;

            SparkTerminal input =
                sparkSwitch.InputTerminal;

            SparkTerminal output =
                sparkSwitch.OutputTerminal;

            if (input == null ||
                output == null)
            {
                return false;
            }

            SparkTerminal next = null;

            if (current == input)
                next = output;
            else if (current == output)
                next = input;
            else
                return false;

            if (next == null)
                return false;

            if (visited.Contains(next))
                return true;

            visited.Add(next);
            traversalQueue.Enqueue(next);

            return true;
        }

        private SparkSwitch ResolveSwitchFromTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return null;

            SparkSwitch direct =
                terminal.GetComponent<SparkSwitch>();

            if (direct != null)
                return direct;

            SparkSwitch parent =
                terminal.GetComponentInParent<SparkSwitch>();

            if (parent != null)
                return parent;

            if (terminal.Owner != null)
            {
                SparkSwitch owner =
                    terminal.Owner
                        .GetComponent<SparkSwitch>();

                if (owner != null)
                    return owner;

                owner =
                    terminal.Owner
                        .GetComponentInParent<SparkSwitch>();

                if (owner != null)
                    return owner;
            }

            return null;
        }

        // ============================================================
        // FAULTS
        // ============================================================

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

                if (positive == null ||
                    negative == null)
                {
                    continue;
                }

                if (positiveReachable.Contains(negative) ||
                    negativeReachable.Contains(positive))
                {
                    return true;
                }

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

            if (Mathf.Abs(state.Current) <=
                0.000001f)
            {
                return false;
            }

            return Mathf.Abs(state.Voltage) < 0.001f &&
                   Mathf.Abs(state.Current) > 1f;
        }

        private bool HasOverload()
        {
            for (int i = 0;
                 i < validSources.Count;
                 i++)
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
            // Failure priority is intentionally preserved.

            if (sourceShorted &&
                level.RejectShortCircuit)
            {
                return;
            }

            if (targetShorted &&
                level.RejectTargetShort)
            {
                return;
            }

            if (wrongConnection &&
                level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.InvalidConnection)
            {
                return;
            }

            if (overloaded &&
                level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.Overload)
            {
                return;
            }

            if (level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.InvalidConnection)
            {
                if (HasInvalidConfiguredTargetConnection(level))
                    return;
            }
        }

        private bool HasFailure(
            SparkLevelDefinition level)
        {
            if (solverEvaluationFailed)
                return true;

            if (sourceShorted &&
                level.RejectShortCircuit)
            {
                return true;
            }

            if (targetShorted &&
                level.RejectTargetShort)
            {
                return true;
            }

            if (wrongConnection &&
                level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.InvalidConnection)
            {
                return true;
            }

            if (overloaded &&
                level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.Overload)
            {
                return true;
            }

            if (level.FailureMode ==
                SparkLevelDefinition.LevelFailureMode.InvalidConnection &&
                HasInvalidConfiguredTargetConnection(level))
            {
                return true;
            }

            return false;
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

            bool reversedPositive =
                positiveReachable.Contains(
                    targetNegative);

            bool reversedNegative =
                negativeReachable.Contains(
                    targetPositive);

            return reversedPositive ||
                   reversedNegative;
        }

        // ============================================================
        // TARGET / TOPOLOGY HELPERS
        // ============================================================

        private SparkLevelTarget GetPrimaryTarget(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return null;
            }

            for (int i = 0;
                 i < level.Targets.Length;
                 i++)
            {
                if (level.Targets[i] != null)
                    return level.Targets[i];
            }

            return null;
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
                positive =
                    target.TargetTerminal;

                return false;
            }

            if (target.TargetComponent == null)
                return false;

            SparkTerminal[] terminals =
                target.TargetComponent
                    .GetComponentsInChildren<SparkTerminal>(
                        true);

            if (terminals == null ||
                terminals.Length < 2)
            {
                return false;
            }

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

                return Mathf.Abs(state.Current) >
                       0.000001f;
            }

            if (target.TargetTerminal != null)
            {
                SparkTerminalElectricalState state =
                    target.TargetTerminal.ElectricalState;

                return Mathf.Abs(state.Current) >
                       0.000001f;
            }

            return false;
        }

        // ============================================================
        // COMPLETION
        // ============================================================

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

        // ============================================================
        // RESULT
        // ============================================================

        private SparkLevelValidationState
            DetermineFailureValidationState()
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

        private SparkLevelValidationState
            DeterminePlayingValidationState()
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

        private SparkLevelValidationResult CreateResult(
            SparkLevelValidationState state,
            string message,
            SparkTerminal affectedTerminal,
            SparkLevelTarget affectedTarget)
        {
            return new SparkLevelValidationResult(
                state,
                string.IsNullOrWhiteSpace(message)
                    ? string.Empty
                    : message,
                satisfiedTargetCount,
                GetCurrentTargetCount(),
                targetVoltage,
                targetCurrent,
                targetPower,
                activeSourceName,
                affectedTerminal,
                affectedTarget);
        }

        private int GetCurrentTargetCount()
        {
            return targetResults.Count;
        }

        private SparkTerminal GetAffectedTerminal(
            SparkLevelValidationState state,
            SparkLevelDefinition level)
        {
            if (state == SparkLevelValidationState.WrongConnection)
            {
                SparkLevelTarget target =
                    GetPrimaryTarget(level);

                SparkTerminal positive;
                SparkTerminal negative;

                if (TryGetTargetTerminalPair(
                        target,
                        out positive,
                        out negative))
                {
                    if (positive != null &&
                        negativeReachable.Contains(positive))
                    {
                        return positive;
                    }

                    if (negative != null &&
                        positiveReachable.Contains(negative))
                    {
                        return negative;
                    }
                }
            }

            if (state ==
                SparkLevelValidationState.ShortCircuit)
            {
                if (level != null &&
                    level.PowerSources != null)
                {
                    for (int i = 0;
                         i < level.PowerSources.Length;
                         i++)
                    {
                        SparkLevelDefinition.PowerSourceDefinition source =
                            level.PowerSources[i];

                        if (source == null)
                            continue;

                        if (source.PositiveTerminal != null &&
                            negativeReachable.Contains(
                                source.PositiveTerminal))
                        {
                            return source.PositiveTerminal;
                        }

                        if (source.NegativeTerminal != null &&
                            positiveReachable.Contains(
                                source.NegativeTerminal))
                        {
                            return source.NegativeTerminal;
                        }
                    }
                }
            }

            return null;
        }

        private SparkLevelTarget GetAffectedTarget(
            SparkLevelValidationState state,
            SparkLevelDefinition level)
        {
            if (state ==
                    SparkLevelValidationState.WrongConnection ||
                state ==
                    SparkLevelValidationState.TargetShort)
            {
                return GetPrimaryTarget(level);
            }

            return null;
        }

        // ============================================================
        // MESSAGES
        // ============================================================

        private string BuildEvaluationMessage(
            SparkLevelDefinition level)
        {
            string message =
                $"{satisfiedTargetCount}/" +
                $"{level.TargetCount} " +
                "targets satisfied.";

            if (level.RequireClosedReturn &&
                !closedReturn)
            {
                message +=
                    " Required circuit return is open.";
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
                message +=
                    " No valid configured power source.";
            }

            return message;
        }

        private string BuildCompletedMessage(
            SparkLevelDefinition level)
        {
            return
                $"{satisfiedTargetCount}/" +
                $"{level.TargetCount} " +
                "targets satisfied. Level completed.";
        }

        private string BuildFailureMessage(
            SparkLevelValidationState state)
        {
            switch (state)
            {
                case SparkLevelValidationState.ShortCircuit:
                    return
                        "Short circuit detected between " +
                        "the configured power-source terminals.";

                case SparkLevelValidationState.TargetShort:
                    return
                        "Target terminals are electrically shorted.";

                case SparkLevelValidationState.WrongConnection:
                    return
                        "Invalid connection: target polarity " +
                        "is connected incorrectly.";

                case SparkLevelValidationState.Overload:
                    return
                        "Electrical overload detected.";

                case SparkLevelValidationState.SolverFault:
                    return
                        "Electrical solver failed to converge.";

                default:
                    return
                        "Invalid level configuration.";
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
                $"[SPARK LEVEL EVALUATOR] {message}",
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
                $"[SPARK LEVEL EVALUATOR] {message}",
                this);
        }

        // ============================================================
        // RUNTIME DATA
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

        private readonly struct PowerSourceRuntime
        {
            public readonly SparkLevelDefinition.PowerSourceDefinition Definition;
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