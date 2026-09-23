using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevelEvaluator
    {
        private readonly SparkCircuitSystem circuitSystem;

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

        private readonly List<SparkLevelDefinition.PowerSourceDefinition> validSources =
            new List<SparkLevelDefinition.PowerSourceDefinition>(8);


        public SparkLevelEvaluator(
            SparkCircuitSystem circuitSystem)
        {
            this.circuitSystem = circuitSystem;
        }


        // ============================================================
        // PUBLIC ENTRY
        // ============================================================

        public SparkLevelEvaluation Evaluate(
            SparkLevelDefinition level)
        {
            if (level == null)
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.NoLevel,
                    "No level definition is configured.");
            }

            level.Normalize();

            if (!ValidateConfiguration(
                    level,
                    out string configurationError))
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConfiguration,
                    configurationError);
            }

            if (circuitSystem == null)
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConfiguration,
                    "Circuit system is not configured.");
            }


            // --------------------------------------------------------
            // ACTIVE POWER SOURCE
            // --------------------------------------------------------

            validSources.Clear();

            FindValidPowerSources(
                level,
                validSources);

           if (validSources.Count == 0)
{
    return SparkLevelEvaluation.Create(
        SparkLevelEvaluationStatus.Incomplete,
        SparkLevelFailureReason.NoValidPowerSource,
        "No valid power source is active.");
}

            SparkLevelDefinition.PowerSourceDefinition activeSource =
                validSources[0];


            // --------------------------------------------------------
            // TOPOLOGY
            // --------------------------------------------------------

            BuildReachability(
                level,
                activeSource,
                out bool closedReturn);


            // --------------------------------------------------------
            // CONNECTION VALIDATION
            // --------------------------------------------------------

            bool invalidConnection =
    DetectInvalidConnection(
        level,
        activeSource,
        out SparkTerminal affectedTerminal,
        out SparkLevelTarget affectedTarget);
            // --------------------------------------------------------
            // ELECTRICAL FAULTS
            // --------------------------------------------------------

            bool sourceShorted =
                DetectSourceShort(activeSource);

            bool overloaded =
                DetectOverload(activeSource);


            // --------------------------------------------------------
            // TARGETS
            // --------------------------------------------------------

            int totalTargets =
                GetTargetCount(level);

            int satisfiedTargets =
                EvaluateTargets(level);

            bool targetsSatisfied =
                level.IsCompletionSatisfied(
                    satisfiedTargets);


            // --------------------------------------------------------
            // TARGET ELECTRICAL STATE
            // --------------------------------------------------------

            GetTargetElectricalState(
                level,
                out float targetVoltage,
                out float targetCurrent,
                out float targetPower);


            // --------------------------------------------------------
            // TARGET SHORT
            // --------------------------------------------------------

            bool targetShorted =
                DetectTargetShort(level);


            // --------------------------------------------------------
            // FINAL RESULT
            // --------------------------------------------------------

            SparkLevelEvaluation evaluation;


            // --------------------------------------------------------
            // 1. SOURCE SHORT
            // --------------------------------------------------------

            if (sourceShorted)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Failed,
                        SparkLevelFailureReason.SourceShortCircuit,
                        "Power source short circuit detected.");

                evaluation =
                    evaluation.WithAffectedTerminal(
                        activeSource.PositiveTerminal);

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 2. TARGET SHORT
            // --------------------------------------------------------

            if (targetShorted)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Failed,
                        SparkLevelFailureReason.TargetShortCircuit,
                        "Target short circuit detected.");

                evaluation =
                    evaluation.WithAffectedTarget(
                        GetPrimaryTarget(level));

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 3. OVERLOAD
            // --------------------------------------------------------

            if (overloaded)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Failed,
                        SparkLevelFailureReason.Overload,
                        "Power source overload detected.");

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 4. WRONG CONNECTION
            // --------------------------------------------------------

            if (invalidConnection)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Failed,
                        SparkLevelFailureReason.InvalidConnection,
                        "Incorrect polarity connection detected.");

                evaluation =
                    evaluation.WithAffectedObjects(
                        affectedTarget,
                        affectedTerminal);

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 5. OPEN CIRCUIT
            // --------------------------------------------------------

            if (level.RequireClosedReturn &&
                !closedReturn)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Incomplete,
                        SparkLevelFailureReason.OpenCircuit,
                        "The circuit does not have a valid closed return path.");

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 6. INSUFFICIENT VOLTAGE
            // --------------------------------------------------------

            if (level.RequireMinimumVoltage &&
                targetVoltage < level.MinimumVoltage)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Incomplete,
                        SparkLevelFailureReason.InsufficientVoltage,
                        "Target voltage is below the required minimum.");

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 7. TARGETS NOT SATISFIED
            // --------------------------------------------------------

            if (!targetsSatisfied)
            {
                evaluation =
                    SparkLevelEvaluation.Create(
                        SparkLevelEvaluationStatus.Incomplete,
                        SparkLevelFailureReason.InvalidTarget,
                        "One or more level targets are not satisfied.");

                return ApplyFinalState(
                    evaluation,
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource,
                    satisfiedTargets,
                    totalTargets);
            }


            // --------------------------------------------------------
            // 8. COMPLETED
            // --------------------------------------------------------

            evaluation =
                SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Completed,
                    SparkLevelFailureReason.None,
                    "Level requirements satisfied.");

            return ApplyFinalState(
                evaluation,
                closedReturn,
                targetShorted,
                sourceShorted,
                overloaded,
                targetVoltage,
                targetCurrent,
                targetPower,
                activeSource,
                satisfiedTargets,
                totalTargets);
        }


        // ============================================================
        // FINAL RESULT ASSEMBLY
        // ============================================================

        private SparkLevelEvaluation ApplyFinalState(
            SparkLevelEvaluation evaluation,
            bool closedReturn,
            bool targetShorted,
            bool sourceShorted,
            bool overloaded,
            float targetVoltage,
            float targetCurrent,
            float targetPower,
            SparkLevelDefinition.PowerSourceDefinition activeSource,
            int satisfiedTargets,
            int totalTargets)
        {
            evaluation =
                evaluation.WithElectricalState(
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource != null
                        ? activeSource.SourceName
                        : string.Empty);

            evaluation =
                evaluation.WithTargets(
                    evaluation.IsCompleted,
                    satisfiedTargets,
                    totalTargets);

            return evaluation;
        }


        // ============================================================
        // CONFIGURATION
        // ============================================================

        private bool ValidateConfiguration(
            SparkLevelDefinition level,
            out string error)
        {
            error = string.Empty;

            if (level.TargetCount <= 0)
            {
                error = "Level has no targets configured.";
                return false;
            }

            if (level.PowerSources == null ||
                level.PowerSources.Length == 0)
            {
                error = "Level has no power sources configured.";
                return false;
            }

            for (int i = 0;
                 i < level.PowerSources.Length;
                 i++)
            {
                SparkLevelDefinition.PowerSourceDefinition source =
                    level.PowerSources[i];

                if (source == null)
                {
                    error =
                        $"Power source entry {i} is null.";

                    return false;
                }

                if (!source.IsConfigured)
                {
                    error =
                        $"Power source '{source.SourceName}' is not configured.";

                    return false;
                }
            }

            if (level.Targets == null ||
                level.Targets.Length == 0)
            {
                error = "Level has no targets configured.";
                return false;
            }

            for (int i = 0;
                i < level.Targets.Length;
                i++)
            {
                SparkLevelTarget target =
                    level.Targets[i];

                if (target == null)
                {
                    error =
                        $"Target entry {i} is null.";

                    return false;
                }

                target.Normalize();

                // ------------------------------------------------------------
                // POLARITY TARGET VALIDATION
                // ------------------------------------------------------------
                // If one polarity terminal is configured, both must be
                // configured. This prevents an incomplete + / - target
                // configuration from being evaluated as valid.
                // ------------------------------------------------------------

                if (target.HasPartialRequiredConnectionPair)
                {
                    error =
                        $"Target '{target.DisplayName}' has an incomplete " +
                        "positive/negative terminal configuration. " +
                        "Both Required Positive Terminal and " +
                        "Required Negative Terminal must be assigned.";

                    return false;
                }


                target.Normalize();
            }

            return true;
        }


        // ============================================================
        // POWER SOURCES
        // ============================================================

        private void FindValidPowerSources(
            SparkLevelDefinition level,
            List<SparkLevelDefinition.PowerSourceDefinition> results)
        {
            if (level == null ||
                results == null ||
                level.PowerSources == null)
            {
                return;
            }

            for (int i = 0;
                 i < level.PowerSources.Length;
                 i++)
            {
                SparkLevelDefinition.PowerSourceDefinition source =
                    level.PowerSources[i];

                if (source == null ||
                    !source.IsConfigured)
                {
                    continue;
                }

                SparkPowerSupply supply =
                    ResolvePowerSupply(source);

                if (supply == null)
                    continue;

                if (!supply.ElectricalEnabled)
                    continue;

                if (!supply.IsOutputActive)
                    continue;

                results.Add(source);
            }
        }


        private SparkPowerSupply ResolvePowerSupply(
            SparkLevelDefinition.PowerSourceDefinition source)
        {
            if (source == null)
                return null;

            if (source.PositiveTerminal != null)
            {
                SparkPowerSupply supply =
                    source.PositiveTerminal
                        .GetComponentInParent<SparkPowerSupply>();

                if (supply != null)
                    return supply;

                supply =
                    source.PositiveTerminal.Owner
                        as SparkPowerSupply;

                if (supply != null)
                    return supply;
            }

            if (source.NegativeTerminal != null)
            {
                SparkPowerSupply supply =
                    source.NegativeTerminal
                        .GetComponentInParent<SparkPowerSupply>();

                if (supply != null)
                    return supply;

                supply =
                    source.NegativeTerminal.Owner
                        as SparkPowerSupply;

                if (supply != null)
                    return supply;
            }

            return null;
        }


        // ============================================================
        // REACHABILITY
        // ============================================================

       private void BuildReachability(
    SparkLevelDefinition level,
    SparkLevelDefinition.PowerSourceDefinition source,
    out bool closedReturn)
{
    positiveReachable.Clear();
    negativeReachable.Clear();

    closedReturn = false;

    if (level == null || source == null)
        return;

    SparkTerminal sourcePositive =
        source.PositiveTerminal;

    SparkTerminal sourceNegative =
        source.NegativeTerminal;

    if (sourcePositive == null ||
        sourceNegative == null)
    {
        return;
    }

    // ------------------------------------------------------------
    // SOURCE +
    // ------------------------------------------------------------

    TraverseWireNetwork(
        sourcePositive,
        positiveReachable);

    // ------------------------------------------------------------
    // SOURCE -
    // ------------------------------------------------------------

    TraverseWireNetwork(
        sourceNegative,
        negativeReachable);

    // ------------------------------------------------------------
    // TARGET
    // ------------------------------------------------------------

    SparkLevelTarget target =
        GetPrimaryTarget(level);

    if (target == null)
        return;

    // ------------------------------------------------------------
    // TARGET MUST HAVE TWO TERMINALS
    // ------------------------------------------------------------

    if (!TryGetTargetTerminalPair(
            target,
            out SparkTerminal targetPositive,
            out SparkTerminal targetNegative))
    {
        return;
    }

    // ------------------------------------------------------------
    // POLARITY
    // ------------------------------------------------------------

    bool positiveCorrect =
        positiveReachable.Contains(targetPositive);

    bool negativeCorrect =
        negativeReachable.Contains(targetNegative);

    // ------------------------------------------------------------
    // CLOSED RETURN
    //
    // BOTH target terminals must be connected with
    // the correct source polarity.
    // ------------------------------------------------------------

    closedReturn =
        positiveCorrect &&
        negativeCorrect;
}


        private void TraverseWireNetwork(
    SparkTerminal start,
    HashSet<SparkTerminal> reachable)
{
    reachable.Clear();

    if (start == null)
        return;

    traversalQueue.Clear();
    visitedTerminals.Clear();

    traversalQueue.Enqueue(start);
    visitedTerminals.Add(start);

    while (traversalQueue.Count > 0)
    {
        SparkTerminal current =
            traversalQueue.Dequeue();

        reachable.Add(current);

        // --------------------------------------------------------
        // DIRECT CIRCUIT CONNECTIONS
        // --------------------------------------------------------

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

            if (!connection.IsValid)
                continue;

            SparkTerminal other =
                connection.GetOther(current);

            if (other == null)
                continue;

            if (visitedTerminals.Contains(other))
                continue;

            visitedTerminals.Add(other);
            traversalQueue.Enqueue(other);
        }


        // --------------------------------------------------------
        // CLOSED SWITCH
        // --------------------------------------------------------

        SparkSwitch sparkSwitch =
            ResolveSwitch(current);

        if (sparkSwitch == null)
            continue;

        if (!sparkSwitch.IsConducting)
            continue;


        SparkTerminal opposite = null;


        if (current == sparkSwitch.InputTerminal)
        {
            opposite =
                sparkSwitch.OutputTerminal;
        }
        else if (current == sparkSwitch.OutputTerminal)
        {
            opposite =
                sparkSwitch.InputTerminal;
        }


        if (opposite == null)
            continue;

        if (visitedTerminals.Contains(opposite))
            continue;

        visitedTerminals.Add(opposite);
        traversalQueue.Enqueue(opposite);
    }
}

        // ============================================================
        // INVALID CONNECTION
        // ============================================================
private bool DetectInvalidConnection(
    SparkLevelDefinition level,
    SparkLevelDefinition.PowerSourceDefinition source,
    out SparkTerminal affectedTerminal,
    out SparkLevelTarget affectedTarget)
{
    affectedTerminal = null;
    affectedTarget = null;

    if (level == null || source == null)
        return false;

    // ------------------------------------------------------------
    // TARGET
    // ------------------------------------------------------------

    SparkLevelTarget target =
        GetPrimaryTarget(level);

    if (target == null)
        return false;

    // ------------------------------------------------------------
    // TARGET POLARITY PAIR
    // ------------------------------------------------------------

    if (!TryGetTargetTerminalPair(
            target,
            out SparkTerminal targetPositive,
            out SparkTerminal targetNegative))
    {
        return false;
    }

    // ------------------------------------------------------------
    // SOURCE POLARITY PAIR
    // ------------------------------------------------------------

    SparkTerminal sourcePositive =
        source.PositiveTerminal;

    SparkTerminal sourceNegative =
        source.NegativeTerminal;

    if (sourcePositive == null ||
        sourceNegative == null)
    {
        return false;
    }

    // ------------------------------------------------------------
    // TRACE SOURCE POSITIVE
    // ------------------------------------------------------------

    HashSet<SparkTerminal> fromPositive =
        new HashSet<SparkTerminal>();

    TraverseWireNetwork(
        sourcePositive,
        fromPositive);

    // ------------------------------------------------------------
    // TRACE SOURCE NEGATIVE
    // ------------------------------------------------------------

    HashSet<SparkTerminal> fromNegative =
        new HashSet<SparkTerminal>();

    TraverseWireNetwork(
        sourceNegative,
        fromNegative);

    // ------------------------------------------------------------
    // CONNECTION MAP
    // ------------------------------------------------------------

    bool sourcePlusToTargetPlus =
        fromPositive.Contains(targetPositive);

    bool sourcePlusToTargetMinus =
        fromPositive.Contains(targetNegative);

    bool sourceMinusToTargetPlus =
        fromNegative.Contains(targetPositive);

    bool sourceMinusToTargetMinus =
        fromNegative.Contains(targetNegative);

    // ------------------------------------------------------------
    // CORRECT POLARITY
    //
    // SOURCE + → TARGET +
    // SOURCE - → TARGET -
    // ------------------------------------------------------------

    if (sourcePlusToTargetPlus &&
        sourceMinusToTargetMinus &&
        !sourcePlusToTargetMinus &&
        !sourceMinusToTargetPlus)
    {
        return false;
    }

    // ------------------------------------------------------------
    // CROSS-CONNECTED
    //
    // SOURCE + → TARGET -
    // SOURCE - → TARGET +
    // ------------------------------------------------------------

    if (sourcePlusToTargetMinus &&
        sourceMinusToTargetPlus)
    {
        affectedTerminal =
            targetNegative;

        affectedTarget =
            target;

        return true;
    }

    // ------------------------------------------------------------
    // SOURCE + → TARGET -
    // ------------------------------------------------------------

    if (sourcePlusToTargetMinus)
    {
        affectedTerminal =
            targetNegative;

        affectedTarget =
            target;

        return true;
    }

    // ------------------------------------------------------------
    // SOURCE - → TARGET +
    // ------------------------------------------------------------

    if (sourceMinusToTargetPlus)
    {
        affectedTerminal =
            targetPositive;

        affectedTarget =
            target;

        return true;
    }

    // ------------------------------------------------------------
    // INCOMPLETE CONNECTION
    //
    // Not enough topology exists yet to call this
    // an incorrect polarity connection.
    // ------------------------------------------------------------

    return false;
}
    // ------------------------------------------------------------
    // INCOMPLETE CONNECTION
    //
    // Do not classify incomplete wiring as wrong polarity.
    // ------------------------------------------------------------


        // ============================================================
        // SOURCE SHORT
        // ============================================================

        private bool DetectSourceShort(
            SparkLevelDefinition.PowerSourceDefinition source)
        {
            if (source == null)
                return false;

            SparkPowerSupply supply =
                ResolvePowerSupply(source);

            if (supply != null &&
                supply.IsShortCircuit)
            {
                return true;
            }

            if (source.PositiveTerminal == null ||
                source.NegativeTerminal == null)
            {
                return false;
            }

            HashSet<SparkTerminal> reachable =
                new HashSet<SparkTerminal>();

            TraverseWireNetwork(
                source.PositiveTerminal,
                reachable);

            return reachable.Contains(
                source.NegativeTerminal);
        }


        // ============================================================
        // OVERLOAD
        // ============================================================

        private bool DetectOverload(
            SparkLevelDefinition.PowerSourceDefinition source)
        {
            SparkPowerSupply supply =
                ResolvePowerSupply(source);

            if (supply == null)
                return false;

            return supply.IsOverloaded;
        }


        // ============================================================
        // TARGET COUNT
        // ============================================================

        private int GetTargetCount(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0;
                 i < level.Targets.Length;
                 i++)
            {
                if (level.Targets[i] != null)
                    count++;
            }

            return count;
        }


        // ============================================================
        // TARGET EVALUATION
        // ============================================================

        private int EvaluateTargets(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return 0;
            }

            int satisfied = 0;

            for (int i = 0;
                 i < level.Targets.Length;
                 i++)
            {
                SparkLevelTarget target =
                    level.Targets[i];

                if (target == null)
                    continue;

                if (target.Evaluate())
                    satisfied++;
            }

            return satisfied;
        }


        // ============================================================
        // TARGET ELECTRICAL STATE
        // ============================================================
private void GetTargetElectricalState(
    SparkLevelDefinition level,
    out float voltage,
    out float current,
    out float power)
{
    voltage = 0f;
    current = 0f;
    power = 0f;

    SparkLevelTarget target =
        GetPrimaryTarget(level);

    if (target == null)
        return;

    // ------------------------------------------------------------
    // TWO-TERMINAL TARGET
    // ------------------------------------------------------------

    if (target.TargetComponent != null)
{
    SparkElectricalState componentState =
        target.TargetComponent.ElectricalState;

    if (TryGetTargetTerminalPair(
            target,
            out SparkTerminal positive,
            out SparkTerminal negative))
    {
        SparkTerminalElectricalState positiveState =
            positive.ElectricalState;

        SparkTerminalElectricalState negativeState =
            negative.ElectricalState;

        voltage =
            Mathf.Abs(
                positiveState.Voltage -
                negativeState.Voltage);

        current =
            Mathf.Abs(
                componentState.Current);

        power =
            voltage * current;

        return;
    }

    voltage =
        Mathf.Abs(
            componentState.Voltage);

    current =
        Mathf.Abs(
            componentState.Current);

    power =
        Mathf.Abs(
            componentState.Power);

    return;
}

    // ------------------------------------------------------------
    // SINGLE TERMINAL TARGET
    // ------------------------------------------------------------

    if (target.TargetTerminal != null)
    {
        SparkTerminalElectricalState state =
            target.TargetTerminal.ElectricalState;

        voltage =
            Mathf.Abs(state.Voltage);

        current =
            Mathf.Abs(state.Current);

        power =
            Mathf.Abs(state.Power);

        return;
    }

    // ------------------------------------------------------------
    // COMPONENT TARGET
    // ------------------------------------------------------------

    if (target.TargetComponent != null)
    {
        SparkElectricalState state =
            target.TargetComponent.ElectricalState;

        voltage =
            Mathf.Abs(state.Voltage);

        current =
            Mathf.Abs(state.Current);

        power =
            Mathf.Abs(state.Power);
    }
}

        // ============================================================
        // TARGET SHORT
        // ============================================================

        private bool DetectTargetShort(
    SparkLevelDefinition level)
{
    SparkLevelTarget target =
        GetPrimaryTarget(level);

    if (target == null)
        return false;

    if (!target.HasRequiredConnectionPair)
        return false;

    SparkTerminal positive =
        target.RequiredPositiveTerminal;

    SparkTerminal negative =
        target.RequiredNegativeTerminal;

    if (positive == null ||
        negative == null)
    {
        return false;
    }

    // ------------------------------------------------------------
    // DIRECT TOPOLOGICAL SHORT
    // ------------------------------------------------------------

    HashSet<SparkTerminal> reachable =
        new HashSet<SparkTerminal>();

    TraverseWireNetwork(
        positive,
        reachable);

    if (reachable.Contains(negative))
    {
        return true;
    }

    // ------------------------------------------------------------
    // ELECTRICAL FALLBACK
    // ------------------------------------------------------------

    GetTargetElectricalState(
        level,
        out float voltage,
        out float current,
        out float power);

    const float voltageThreshold = 0.005f;
    const float currentThreshold = 0.01f;

    return Mathf.Abs(voltage) <= voltageThreshold &&
           Mathf.Abs(current) > currentThreshold;
}
        // ============================================================
        // PRIMARY TARGET
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
                SparkLevelTarget target =
                    level.Targets[i];

                if (target != null)
                    return target;
            }

            return null;
        }


        // ============================================================
        // TARGET TERMINAL PAIR
        // ============================================================

        private bool TryGetTargetTerminalPair(
            SparkLevelTarget target,
            out SparkTerminal positive,
            out SparkTerminal negative)
        {
            positive = null;
            negative = null;

            if (target == null)
                return false;

            if (!target.HasRequiredConnectionPair)
                return false;

            positive =
                target.RequiredPositiveTerminal;

            negative =
                target.RequiredNegativeTerminal;

            return positive != null &&
                   negative != null;
        }


        // ============================================================
        // SWITCH
        // ============================================================

        private SparkSwitch ResolveSwitch(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return null;

            SparkSwitch sparkSwitch =
                terminal.GetComponentInParent<SparkSwitch>();

            if (sparkSwitch != null)
                return sparkSwitch;

            sparkSwitch =
                terminal.Owner as SparkSwitch;

            if (sparkSwitch != null)
                return sparkSwitch;

            return null;
        }


        // ============================================================
        // CONNECTION
        // ============================================================

        private SparkTerminal GetOtherTerminal(
            SparkCircuitConnection connection,
            SparkTerminal current)
        {
            if (!connection.IsValid ||
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


        // ============================================================
        // DEBUG
        // ============================================================

        private void DebugTargetConnections(
            SparkLevelTarget target)
        {
            if (target == null)
                return;

            Debug.Log(
                $"[LEVEL EVALUATOR] Incorrect target polarity: {target.DisplayName}",
                target.TargetComponent);
        }
    }
}