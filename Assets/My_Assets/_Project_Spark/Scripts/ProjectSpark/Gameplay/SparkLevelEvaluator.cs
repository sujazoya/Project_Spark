using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Measurement;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Evaluates a Spark level against the current circuit/electrical state.
    ///
    /// SparkLevelDefinition contains only persistent configuration and IDs.
    /// SparkLevelSceneBindings resolves those IDs to actual scene objects.
    ///
    /// This class does not own circuit topology and does not modify the
    /// circuit system.
    /// </summary>
    public sealed class SparkLevelEvaluator
    {
        // ================================================================
        // REFERENCES
        // ================================================================

        private readonly SparkCircuitSystem circuitSystem;

        private readonly SparkLevelSceneBindings sceneBindings;

        // ================================================================
        // REUSABLE BUFFERS
        // ================================================================

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

        private readonly List<SparkLevelDefinition.PowerSourceDefinition>
            validSources =
            new List<SparkLevelDefinition.PowerSourceDefinition>(8);

        // ================================================================
        // CONSTRUCTOR
        // ================================================================

        public SparkLevelEvaluator(
            SparkCircuitSystem circuitSystem,
            SparkLevelSceneBindings sceneBindings)
        {
            this.circuitSystem = circuitSystem;
            this.sceneBindings = sceneBindings;
        }

        // ================================================================
        // MAIN EVALUATION
        // ================================================================

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

            // ------------------------------------------------------------
            // CONFIGURATION
            // ------------------------------------------------------------

            if (sceneBindings == null)
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConfiguration,
                    "No SparkLevelSceneBindings component is configured.");
            }

            if (!level.Validate(out string configurationError))
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
                    "SparkCircuitSystem is not configured.");
            }

            if (!sceneBindings.Validate(out string bindingError))
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConfiguration,
                    bindingError);
            }

            // ------------------------------------------------------------
            // POWER SOURCES
            // ------------------------------------------------------------

            validSources.Clear();

            FindValidPowerSources(
                level,
                validSources);

            if (validSources.Count == 0)
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.NoValidPowerSource,
                    "No configured power source is currently active.");
            }

            SparkLevelDefinition.PowerSourceDefinition activeSource =
                validSources[0];

            // ------------------------------------------------------------
            // RESOLVE SOURCE TERMINALS
            // ------------------------------------------------------------

            if (!TryResolvePowerSourceTerminals(
                    activeSource,
                    out SparkTerminal sourcePositive,
                    out SparkTerminal sourceNegative))
            {
                return SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConfiguration,
                    $"Power source '{activeSource.SourceName}' " +
                    "could not resolve its scene terminals.");
            }

            // ------------------------------------------------------------
            // REACHABILITY
            // ------------------------------------------------------------

            BuildReachability(
                level,
                sourcePositive,
                sourceNegative,
                out bool closedReturn);

            // ------------------------------------------------------------
            // INVALID POLARITY CONNECTION
            // ------------------------------------------------------------

            bool invalidConnection =
                DetectInvalidConnection(
                    level,
                    sourcePositive,
                    sourceNegative,
                    out SparkTerminal affectedTerminal,
                    out SparkLevelTarget affectedTarget);

            // ------------------------------------------------------------
            // SOURCE SHORT
            // ------------------------------------------------------------

            bool sourceShorted =
                DetectSourceShort(
                    activeSource,
                    sourcePositive,
                    sourceNegative);

            // ------------------------------------------------------------
            // OVERLOAD
            // ------------------------------------------------------------

            bool overloaded =
                DetectOverload(activeSource);

            // ------------------------------------------------------------
            // TARGET EVALUATION
            // ------------------------------------------------------------

            int satisfiedTargetCount =
                EvaluateTargets(level);

            int totalTargetCount =
                level.TargetCount;

            bool targetsSatisfied =
                level.IsCompletionSatisfied(
                    satisfiedTargetCount);

           
            // ------------------------------------------------------------
            // TARGET ELECTRICAL STATE
            // ------------------------------------------------------------

            GetTargetElectricalState(
                level,
                out float targetVoltage,
                out float targetCurrent,
                out float targetPower);

            // ============================================================
            // DEBUG: TARGET ELECTRICAL STATE
            // ============================================================

            SparkLevelTarget debugTarget =
                GetPrimaryTarget(level);

            Debug.Log(
                $"[LEVEL TARGET DEBUG] " +
                $"EvaluatedV={targetVoltage:F6} | " +
                $"EvaluatedI={targetCurrent:F6} | " +
                $"EvaluatedP={targetPower:F6} | " +
                $"Targets={satisfiedTargetCount}/{totalTargetCount}");

            if (debugTarget != null &&
                TryResolveTargetComponent(
                    debugTarget,
                    out SparkElectricalComponent debugComponent))
            {
                SparkElectricalState state =
                    debugComponent.ElectricalState;

                Debug.Log(
                    $"[LEVEL TARGET COMPONENT CHECK] " +
                    $"Component={debugComponent.name} | " +
                    $"Type={debugComponent.GetType().Name} | " +
                    $"Voltage={state.Voltage:F6} V | " +
                    $"Current={state.Current:F6} A | " +
                    $"Power={state.Power:F6} W | " +
                    $"Conduction={state.Conduction} | " +
                    $"Enabled={debugComponent.ElectricalEnabled}");
            }
            else
            {
                Debug.LogWarning(
                    "[LEVEL TARGET COMPONENT CHECK] " +
                    "Could not resolve the target component.");
            }

            // ------------------------------------------------------------
            // TARGET SHORT
            // ------------------------------------------------------------

            bool targetShorted =
                DetectTargetShort(level);


            // ------------------------------------------------------------
            // BASE RESULT
            // ------------------------------------------------------------

            SparkLevelEvaluation evaluation =
                SparkLevelEvaluation.Create(
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.None,
                    "Level evaluation incomplete.");

            evaluation =
                evaluation.WithElectricalState(
                    closedReturn,
                    targetShorted,
                    sourceShorted,
                    overloaded,
                    targetVoltage,
                    targetCurrent,
                    targetPower,
                    activeSource.SourceName);

            evaluation =
                evaluation.WithTargets(
                    targetsSatisfied,
                    satisfiedTargetCount,
                    totalTargetCount);

            // ------------------------------------------------------------
            // AFFECTED INFORMATION
            // ------------------------------------------------------------

            if (affectedTerminal != null ||
                affectedTarget != null)
            {
                evaluation =
                    evaluation.WithAffectedObjects(
                        affectedTarget,
                        affectedTerminal);
            }

            // ------------------------------------------------------------
            // FINAL STATE
            // ------------------------------------------------------------

            // Highest-priority electrical faults first.

            if (sourceShorted &&
                level.RejectShortCircuit)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.SourceShortCircuit,
                    "Power source is short-circuited.");
            }

            if (targetShorted &&
                level.RejectTargetShort)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.TargetShortCircuit,
                    "Target circuit is short-circuited.");
            }

            if (overloaded)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.Overload,
                    "Power source is overloaded.");
            }

            if (invalidConnection)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Failed,
                    SparkLevelFailureReason.InvalidConnection,
                    "Target is connected with incorrect polarity.");
            }

            // ------------------------------------------------------------
            // OPEN RETURN
            // ------------------------------------------------------------

            if (level.RequireClosedReturn &&
                !closedReturn)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.OpenCircuit,
                    "Circuit does not have a valid closed return path.");
            }

            // ------------------------------------------------------------
            // MINIMUM VOLTAGE
            // ------------------------------------------------------------

            if (level.RequireMinimumVoltage &&
                targetVoltage < level.MinimumVoltage)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.InsufficientVoltage,
                    $"Target voltage is below the required minimum " +
                    $"of {level.MinimumVoltage:0.###} V.");
            }

            // ------------------------------------------------------------
            // TARGETS
            // ------------------------------------------------------------

            if (!targetsSatisfied)
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.InvalidTarget,
                    "Required level targets are not satisfied.");
            }

            
            // ------------------------------------------------------------
            // LEVEL 6 — REQUIRED RESISTORS MUST BE HEALTHY
            // ------------------------------------------------------------

            if (!AreRequiredResistorsHealthy(
                    level,
                    out string resistorFaultMessage))
            {
                return CreateFinalEvaluation(
                    evaluation,
                    SparkLevelEvaluationStatus.Incomplete,
                    SparkLevelFailureReason.InvalidTarget,
                    resistorFaultMessage);
            }


            // ------------------------------------------------------------
            // COMPLETED
            // ------------------------------------------------------------

            return CreateFinalEvaluation(
                evaluation,
                SparkLevelEvaluationStatus.Completed,
                SparkLevelFailureReason.None,
                "Level completed successfully.");
        }

        // ================================================================
        // FINAL EVALUATION BUILDER
        // ================================================================

        private SparkLevelEvaluation CreateFinalEvaluation(
            SparkLevelEvaluation baseEvaluation,
            SparkLevelEvaluationStatus status,
            SparkLevelFailureReason failureReason,
            string message)
        {
            SparkLevelEvaluation result =
                SparkLevelEvaluation.Create(
                    status,
                    failureReason,
                    message);

            result =
                result.WithElectricalState(
                    baseEvaluation.ClosedReturn,
                    baseEvaluation.TargetShorted,
                    baseEvaluation.SourceShorted,
                    baseEvaluation.Overloaded,
                    baseEvaluation.TargetVoltage,
                    baseEvaluation.TargetCurrent,
                    baseEvaluation.TargetPower,
                    baseEvaluation.ActiveSource);

            result =
                result.WithTargets(
                    baseEvaluation.TargetsSatisfied,
                    baseEvaluation.SatisfiedTargetCount,
                    baseEvaluation.TotalTargetCount);

            if (baseEvaluation.HasAffectedTerminal ||
                baseEvaluation.HasAffectedTarget)
            {
                result =
                    result.WithAffectedObjects(
                        baseEvaluation.AffectedTarget,
                        baseEvaluation.AffectedTerminal);
            }

            return result;
        }

        // ================================================================
        // POWER SOURCE RESOLUTION
        // ================================================================

        private bool TryResolvePowerSourceTerminals(
            SparkLevelDefinition.PowerSourceDefinition source,
            out SparkTerminal positiveTerminal,
            out SparkTerminal negativeTerminal)
        {
            positiveTerminal = null;
            negativeTerminal = null;

            if (source == null)
                return false;

            if (sceneBindings == null)
                return false;

            bool positiveResolved =
                sceneBindings.TryGetTerminal(
                    source.PositiveTerminalId,
                    out positiveTerminal);

            bool negativeResolved =
                sceneBindings.TryGetTerminal(
                    source.NegativeTerminalId,
                    out negativeTerminal);

            return
                positiveResolved &&
                negativeResolved &&
                positiveTerminal != null &&
                negativeTerminal != null;
        }

        // ================================================================
        // FIND VALID POWER SOURCES
        // ================================================================

        private void FindValidPowerSources(
            SparkLevelDefinition level,
            List<SparkLevelDefinition.PowerSourceDefinition> results)
        {
            results.Clear();

            if (level == null ||
                sceneBindings == null)
            {
                return;
            }

            SparkLevelDefinition.PowerSourceDefinition[] sources =
                level.PowerSources;

            if (sources == null)
                return;

            for (int i = 0; i < sources.Length; i++)
            {
                SparkLevelDefinition.PowerSourceDefinition source =
                    sources[i];

                if (source == null ||
                    !source.IsConfigured)
                {
                    continue;
                }

                if (!TryResolvePowerSourceTerminals(
                        source,
                        out SparkTerminal positiveTerminal,
                        out SparkTerminal negativeTerminal))
                {
                    continue;
                }

                SparkPowerSupply powerSupply =
                    ResolvePowerSupply(positiveTerminal);

                if (powerSupply == null)
                {
                    powerSupply =
                        ResolvePowerSupply(negativeTerminal);
                }

                if (powerSupply == null)
                    continue;

                if (!powerSupply.ElectricalEnabled)
                    continue;

                if (!powerSupply.IsOutputActive)
                    continue;

                results.Add(source);

                if (!level.AllowAnyConfiguredPowerSource)
                    break;
            }
        }

        // ================================================================
        // POWER SUPPLY RESOLUTION
        // ================================================================

        private SparkPowerSupply ResolvePowerSupply(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return null;

            SparkPowerSupply supply =
                terminal.GetComponentInParent<SparkPowerSupply>();

            if (supply != null)
                return supply;

            if (terminal.Owner is SparkPowerSupply ownerSupply)
                return ownerSupply;

            return null;
        }

        // ================================================================
        // BUILD REACHABILITY
        // ================================================================

private void BuildReachability(
    SparkLevelDefinition level,
    SparkTerminal sourcePositive,
    SparkTerminal sourceNegative,
    out bool closedReturn)
{
    positiveReachable.Clear();
    negativeReachable.Clear();

    closedReturn = false;

    if (level == null ||
        sourcePositive == null ||
        sourceNegative == null)
    {
        Debug.LogWarning(
            "[LEVEL REACHABILITY] Missing level or source terminal.");
        return;
    }

    TraverseWireNetwork(
        sourcePositive,
        positiveReachable,
        traverseResistors: true);

    TraverseWireNetwork(
        sourceNegative,
        negativeReachable,
        traverseResistors: true);

    SparkLevelTarget target = GetPrimaryTarget(level);

    if (target == null)
    {
        Debug.LogWarning(
            "[LEVEL REACHABILITY] No primary target found.");
        return;
    }

    if (!TryResolveTargetTerminalPair(
            target,
            out SparkTerminal targetPositive,
            out SparkTerminal targetNegative))
    {
        Debug.LogWarning(
            $"[LEVEL TARGET PAIR] Failed to resolve terminal pair. " +
            $"Target={target.TargetComponentId} | " +
            $"PositiveID={target.RequiredPositiveTerminalId} | " +
            $"NegativeID={target.RequiredNegativeTerminalId}");

        return;
    }

    Debug.Log(
        "[LEVEL TARGET PAIR] " +
        $"Target={target.TargetComponentId} | " +
        $"Positive={GetTerminalDebugName(targetPositive)} | " +
        $"Negative={GetTerminalDebugName(targetNegative)}");

    bool positiveHasTargetPositive =
        positiveReachable.Contains(targetPositive);

    bool negativeHasTargetNegative =
        negativeReachable.Contains(targetNegative);

    bool positiveHasTargetNegative =
        positiveReachable.Contains(targetNegative);

    bool negativeHasTargetPositive =
        negativeReachable.Contains(targetPositive);

    Debug.Log(
        "[LEVEL REACHABILITY RESULT] " +
        $"PositiveHasTargetPositive={positiveHasTargetPositive} | " +
        $"NegativeHasTargetNegative={negativeHasTargetNegative} | " +
        $"PositiveHasTargetNegative={positiveHasTargetNegative} | " +
        $"NegativeHasTargetPositive={negativeHasTargetPositive} | " +
        $"PositiveCount={positiveReachable.Count} | " +
        $"NegativeCount={negativeReachable.Count}");

    closedReturn =
        positiveHasTargetPositive &&
        negativeHasTargetNegative;

    Debug.Log(
        $"[LEVEL CLOSED RETURN] ClosedReturn={closedReturn}");
}


        // ================================================================
        // WIRE NETWORK TRAVERSAL
        // ================================================================

        private void TraverseWireNetwork(
            SparkTerminal start,
            HashSet<SparkTerminal> reachable,
            bool traverseConductingLeds = true,
            bool bypassOnly = false,
            bool traverseResistors = false)
        {
            if (start == null ||
                reachable == null)
            {
                return;
            }

            traversalQueue.Clear();
            visitedTerminals.Clear();

            traversalQueue.Enqueue(start);
            visitedTerminals.Add(start);


/*                        Debug.Log(
                $"[LEVEL PATH START] " +
                $"Start={GetTerminalDebugName(start)}");*/

            while (traversalQueue.Count > 0)
            {
                SparkTerminal current =
                    traversalQueue.Dequeue();

                reachable.Add(current);
           /* Debug.Log(
                $"[LEVEL PATH WALK] " +
                $"Start={GetTerminalDebugName(start)} | " +
                $"Current={GetTerminalDebugName(current)}");*/

                connectionBuffer.Clear();

                circuitSystem.GetConnections(
                    current,
                    connectionBuffer);

                    /*Debug.Log(
    $"[LEVEL PATH CONNECTION COUNT] " +
    $"Current={GetTerminalDebugName(current)} | " +
    $"Count={connectionBuffer.Count}");*/

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (!connection.IsValid)
                        continue;

                    // Probe connections are measurements, not conductive wires.
                    // A bypass search must never treat them as shorting links.
                    if (bypassOnly &&
                        connection.Kind == SparkConnectionKind.Probe)
                        continue;

                    SparkTerminal other =
                        connection.GetOther(current);

                    if (other == null)
                        continue;

                        /* Debug.Log(
    $"[LEVEL PATH CONNECTION] " +
    $"Current={GetTerminalDebugName(current)} | " +
    $"Other={GetTerminalDebugName(other)}");*/

                    if (visitedTerminals.Add(other))
                    {
                        traversalQueue.Enqueue(other);
                    }
                }
                // --------------------------------------------------------
                // CONDUCTING LED
                // --------------------------------------------------------

                SparkLED led =
                    current.GetComponentInParent<SparkLED>();

                if (led == null &&
                    current.Owner is SparkLED ownerLed)
                {
                    led = ownerLed;
                }

                if (traverseConductingLeds &&
                    led != null &&
                    led.ElectricalEnabled &&
                    led.ElectricalState.Conduction ==
                        SparkConductionState.Conducting)
                {
                    SparkTerminal anode =
                        led.AnodeTerminal;

                    SparkTerminal cathode =
                        led.CathodeTerminal;

                    SparkTerminal ledOpposite = null;

                    if (led.CanConductBetween(current, anode))
                    {
                        ledOpposite = cathode;
                    }

                    if (ledOpposite != null &&
                        visitedTerminals.Add(ledOpposite))
                    {
                        traversalQueue.Enqueue(ledOpposite);
                    }
                }

             
                // --------------------------------------------------------
                // RESISTOR CONTINUITY (NEVER A WIRE-ONLY SHORT)
                // --------------------------------------------------------

                if (traverseResistors && !bypassOnly)
                {
                    SparkResistor resistor =
                        current.GetComponentInParent<SparkResistor>();

                    if (resistor == null &&
                        current.Owner is SparkResistor ownerResistor)
                    {
                        resistor = ownerResistor;
                    }

                    if (resistor != null)
                    {
                        SparkTerminal opposites = null;

                        if (current == resistor.TerminalA)
                        {
                            opposites = resistor.TerminalB;
                        }
                        else if (current == resistor.TerminalB)
                        {
                            opposites = resistor.TerminalA;
                        }

                        if (opposites != null &&
                            resistor.CanConductBetween(
                                current,
                                opposites) &&
                            visitedTerminals.Add(opposites))
                        {
                            traversalQueue.Enqueue(opposites);
                        }
                    }
                }

                
                // --------------------------------------------------------
                // INLINE MULTIMETER SHUNT
                // --------------------------------------------------------
                // The current-mode multimeter is a conductive component
                // with a small internal resistance.
                //
                // Traverse it for normal circuit reachability, but NEVER
                // treat it as a wire-only bypass during short detection.

                if (!bypassOnly && traverseResistors)
                {
                    SparkMultimeterElectricalComponent meter =
                        current.GetComponentInParent<
                            SparkMultimeterElectricalComponent>();

                    if (meter == null &&
                        current.Owner is
                            SparkMultimeterElectricalComponent ownerMeter)
                    {
                        meter = ownerMeter;
                    }

                    if (meter != null &&
                        meter.ElectricalEnabled &&
                        meter.CurrentMeasurementEnabled)
                    {
                        SparkTerminal meterOpposite = null;

                        if (current == meter.RedTerminal)
                        {
                            meterOpposite = meter.BlackTerminal;
                        }
                        else if (current == meter.BlackTerminal)
                        {
                            meterOpposite = meter.RedTerminal;
                        }

                        if (meterOpposite != null &&
                            visitedTerminals.Add(meterOpposite))
                        {
                            traversalQueue.Enqueue(meterOpposite);
                        }
                    }
                }

              
            // --------------------------------------------------------
            // SINGLE WIRE INTERNAL CONTINUITY
            // --------------------------------------------------------
            SparkSingleWire singleWire =
                current.GetComponentInParent<SparkSingleWire>();

            if (singleWire == null &&
                current.Owner is SparkSingleWire ownerWire)
            {
                singleWire = ownerWire;
            }

            if (singleWire != null)
            {
                SparkTerminal wireOpposite =
                    singleWire.GetOppositeTerminal(current);

                if (wireOpposite != null &&
                    singleWire.CanConductBetween(current, wireOpposite) &&
                    visitedTerminals.Add(wireOpposite))
                {
                    traversalQueue.Enqueue(wireOpposite);
                }
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

                SparkTerminal opposite =
                    GetOtherSwitchTerminal(
                        sparkSwitch,
                        current);

                if (opposite == null)
                    continue;

                if (visitedTerminals.Add(opposite))
                {
                    traversalQueue.Enqueue(opposite);
                }
            }
        }

        // ================================================================
        // SWITCH RESOLUTION
        // ================================================================

        private SparkSwitch ResolveSwitch(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return null;

            SparkSwitch sparkSwitch =
                terminal.GetComponentInParent<SparkSwitch>();

            if (sparkSwitch != null)
                return sparkSwitch;

            if (terminal.Owner is SparkSwitch ownerSwitch)
                return ownerSwitch;

            return null;
        }

        // ================================================================
        // SWITCH OPPOSITE TERMINAL
        // ================================================================

        private SparkTerminal GetOtherSwitchTerminal(
            SparkSwitch sparkSwitch,
            SparkTerminal current)
        {
            if (sparkSwitch == null ||
                current == null)
            {
                return null;
            }

            SparkTerminal input =
                sparkSwitch.InputTerminal;

            SparkTerminal output =
                sparkSwitch.OutputTerminal;

            if (input == current)
                return output;

            if (output == current)
                return input;

            return null;
        }

        // ================================================================
        // INVALID CONNECTION
        // ================================================================

       private bool DetectInvalidConnection(
    SparkLevelDefinition level,
    SparkTerminal sourcePositive,
    SparkTerminal sourceNegative,
    out SparkTerminal affectedTerminal,
    out SparkLevelTarget affectedTarget)
{
    affectedTerminal = null;
    affectedTarget = null;

    SparkLevelTarget target =
        GetPrimaryTarget(level);

    if (target == null)
    {
        return false;
    }

    if (!TryResolveTargetTerminalPair(
            target,
            out SparkTerminal targetPositive,
            out SparkTerminal targetNegative))
    {
        return false;
    }

    positiveReachable.Clear();
    negativeReachable.Clear();

    Debug.Log(
    $"[LEVEL SOURCE TERMINALS] " +
    $"Positive={GetTerminalDebugName(sourcePositive)} | " +
    $"Negative={GetTerminalDebugName(sourceNegative)}");


   
TraverseWireNetwork(
    sourcePositive,
    positiveReachable,
    traverseConductingLeds: false,
    bypassOnly: true);

TraverseWireNetwork(
    sourceNegative,
    negativeReachable,
    traverseConductingLeds: false,
    bypassOnly: true);



    bool positiveToPositive =
        positiveReachable.Contains(
            targetPositive);

    bool negativeToNegative =
        negativeReachable.Contains(
            targetNegative);

    bool positiveToNegative =
        positiveReachable.Contains(
            targetNegative);

    bool negativeToPositive =
        negativeReachable.Contains(
            targetPositive);



              // ================================================================
    // DEBUG POLARITY / RETURN PATH
    // ================================================================

/*    Debug.Log(
        $"[LEVEL 1 PATH] " +
        $"Positive→Positive={positiveToPositive} | " +
        $"Positive→Negative={positiveToNegative} | " +
        $"Negative→Positive={negativeToPositive} | " +
        $"Negative→Negative={negativeToNegative}");*/


    // ================================================================
    // CORRECT POLARITY
    // ================================================================

    if (positiveToPositive &&
        negativeToNegative &&
        !positiveToNegative &&
        !negativeToPositive)
    {
        return false;
    }


    // ================================================================
    // WRONG POLARITY
    // ================================================================

    bool wrongPolarity =
        positiveToNegative ||
        negativeToPositive;

    if (!wrongPolarity)
    {
        return false;
    }


    affectedTarget =
        target;


    if (positiveToNegative)
    {
        affectedTerminal =
            targetNegative;
    }
    else if (negativeToPositive)
    {
        affectedTerminal =
            targetPositive;
    }


    return true;
}

        // ================================================================
        // SOURCE SHORT
        // ================================================================

        private bool DetectSourceShort(
            SparkLevelDefinition.PowerSourceDefinition source,
            SparkTerminal sourcePositive,
            SparkTerminal sourceNegative)
        {
            if (sourcePositive == null || sourceNegative == null)
                return false;

            // Do not use supply.IsShortCircuit here. In the current power
            // supply component it is an alias of the current-limit status,
            // which detects overload, not a conductive bypass.
            //
            // A source short is a wire/connector/closed-switch path that
            // reaches the negative terminal without crossing a load.
            positiveReachable.Clear();

            TraverseWireNetwork(
                sourcePositive,
                positiveReachable,
                traverseConductingLeds: false,
                bypassOnly: true);

            bool shorted = positiveReachable.Contains(sourceNegative);

            if (shorted)
            {
                Debug.LogWarning(
                    "[LEVEL SOURCE SHORT] Conductive bypass found between " +
                    $"{GetTerminalDebugName(sourcePositive)} and " +
                    $"{GetTerminalDebugName(sourceNegative)}. " +
                    "The bypass traversal excludes LED conduction and " +
                    "measurement probes. Check for a direct wire, a " +
                    "closed-switch-only path, or incorrectly shared terminals.");
            }

            return shorted;
        }

        // ================================================================
        // OVERLOAD
        // ================================================================

        private bool DetectOverload(
            SparkLevelDefinition.PowerSourceDefinition source)
        {
            if (source == null)
                return false;

            if (!sceneBindings.TryGetTerminal(
                    source.PositiveTerminalId,
                    out SparkTerminal positiveTerminal))
            {
                return false;
            }

            SparkPowerSupply supply =
                ResolvePowerSupply(positiveTerminal);

            return
                supply != null &&
                supply.IsOverloaded;
        }

        
        // ================================================================
        // LEVEL 6 — REQUIRED RESISTOR HEALTH CHECK
        // ================================================================

       
private bool AreRequiredResistorsHealthy(
    SparkLevelDefinition level,
    out string message)
{
    message = string.Empty;

    if (level == null || sceneBindings == null)
    {
        message = "Level bindings are unavailable.";
        return false;
    }

    // =====================================================
    // PART 1: FIXED RESISTOR BINDING IDS
    // =====================================================
    // Suitable for permanent, nonreplaceable resistors.

    string[] resistorIds =
        level.RequiredHealthyResistorIds;

    if (resistorIds != null)
    {
        for (int i = 0; i < resistorIds.Length; i++)
        {
            string resistorId = resistorIds[i];

            if (!sceneBindings.TryGetComponent(
                    resistorId,
                    out SparkElectricalComponent component))
            {
                message =
                    "Required resistor binding not found: " +
                    resistorId;
                return false;
            }

            SparkResistor resistor =
                component as SparkResistor;

            if (resistor == null)
            {
                message =
                    "Binding is not a resistor: " +
                    resistorId;
                return false;
            }

            if (!resistor.ElectricalEnabled)
            {
                message =
                    "Required resistor is disabled: " +
                    resistor.name;
                return false;
            }

            if (resistor.IsBurned)
            {
                message =
                    "Resistor is burned: " +
                    resistor.name;
                return false;
            }
        }
    }

    // =====================================================
    // PART 2: INSTALLED REPLACEABLE RESISTORS
    // =====================================================
    // Finds current resistor objects rather than relying
    // on a binding to the original 100-ohm component.

    if (level.RequireHealthyInstalledResistors)
    {
        SparkResistor[] resistors =
            UnityEngine.Object.FindObjectsByType<
                SparkResistor>(
                FindObjectsSortMode.None);

        for (int i = 0; i < resistors.Length; i++)
        {
            SparkResistor resistor = resistors[i];

            if (resistor == null)
                continue;

            SparkShelfItem shelfItem =
                resistor.GetComponentInParent<
                    SparkShelfItem>();

            // This rule checks replaceable shelf items.
            // Permanently installed objects can still
            // be checked by fixed binding IDs above.
            if (shelfItem == null)
                continue;

            bool installed =
                shelfItem.IsAtWorkbench &&
                shelfItem.CurrentSlot != null &&
                shelfItem.CurrentSlot.Occupant ==
                    shelfItem;

            if (!installed)
                continue;

            if (resistor.IsBurned)
            {
                message =
                    "Installed resistor " +
                    resistor.name +
                    " is burned. Replace or repair it.";

                return false;
            }

            if (!resistor.ElectricalEnabled)
            {
                message =
                    "Installed resistor " +
                    resistor.name +
                    " is electrically disabled.";

                return false;
            }
        }
    }

    return true;
}



        // ================================================================
        // TARGET EVALUATION
        // ================================================================

        private int EvaluateTargets(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return 0;
            }

            int satisfiedCount = 0;

            SparkLevelTarget[] targets =
                level.Targets;

            for (int i = 0; i < targets.Length; i++)
            {
                SparkLevelTarget target =
                    targets[i];

                if (target == null)
                    continue;

                if (EvaluateTarget(target))
                {
                    satisfiedCount++;
                }
            }

            return satisfiedCount;
        }

        // ================================================================
        // SINGLE TARGET EVALUATION
        // ================================================================

        private bool EvaluateTarget(
            SparkLevelTarget target)
        {
            if (target == null)
                return false;

            bool result;

            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.TerminalPowered:

                    result =
                        EvaluateTerminalPowered(target);

                    break;

                case SparkLevelTarget.TargetType.VoltagePresent:

                    result =
                        EvaluateVoltage(target);

                    break;

                case SparkLevelTarget.TargetType.ComponentPowered:

                    result =
                        EvaluateComponentPowered(target);

                    break;

                case SparkLevelTarget.TargetType.LEDOn:

                    result =
                        EvaluateLED(target);

                    break;

                case SparkLevelTarget.TargetType.ComponentConducting:

                    result =
                        EvaluateComponentConducting(target);

                    break;

                    case SparkLevelTarget.TargetType.VoltageRange:
                    result = EvaluateMeasurementRange(target);
                    break;

                case SparkLevelTarget.TargetType.CurrentRange:
                    result = EvaluateMeasurementRange(target);
                    break;

                case SparkLevelTarget.TargetType.PowerRange:
                    result = EvaluateMeasurementRange(target);
                    break;

                default:

                    result = false;

                    break;
            }

            if (target.Inverted)
                result = !result;

            return result;
        }
        private bool EvaluateMeasurementRange(SparkLevelTarget target)
{
    if (target == null)
        return false;

    if (!target.IsConfigured() ||
        !target.IsPolarityConfigurationValid())
        return false;

    float value;
    float minimum;
    float maximum;
    bool useMaximum;

    if (target.Type == SparkLevelTarget.TargetType.VoltageRange)
    {
        if (!TryReadVoltage(target, out value))
            return false;

        minimum = target.MinimumVoltage;
        maximum = target.MaximumVoltage;
        useMaximum = target.UseMaximumVoltage;
    }
    else
    {
        if (!TryResolveTargetComponent(
                target,
                out SparkElectricalComponent component))
            return false;

        if (target.RequireElectricalEnabled &&
            !component.ElectricalEnabled)
            return false;

        if (target.RequireConduction &&
            component.ElectricalState.Conduction !=
                SparkConductionState.Conducting)
            return false;

        if (target.Type == SparkLevelTarget.TargetType.CurrentRange)
        {
            value = Mathf.Abs(component.ElectricalState.Current);
            minimum = target.MinimumCurrent;
            maximum = target.MaximumCurrent;
            useMaximum = target.UseMaximumCurrent;
        }
        else
        {
            value = Mathf.Abs(component.ElectricalState.Power);
            minimum = target.MinimumPower;
            maximum = target.MaximumPower;
            useMaximum = target.UseMaximumPower;
        }
    }

    if (float.IsNaN(value) || float.IsInfinity(value))
        return false;

    if (value < minimum)
        return false;

    if (useMaximum && value > maximum)
        return false;

    return true;
}

private bool TryReadVoltage(
    SparkLevelTarget target,
    out float voltage)
{
    voltage = 0f;

    if (target.HasRequiredConnectionPair)
    {
        if (!TryResolveTargetTerminalPair(
                target,
                out SparkTerminal positive,
                out SparkTerminal negative))
            return false;

        if (target.RequireElectricalEnabled &&
            (!positive.IsElectricalEnabled ||
             !negative.IsElectricalEnabled))
            return false;

        voltage = Mathf.Abs(
            positive.ElectricalState.Voltage -
            negative.ElectricalState.Voltage);

        return true;
    }

    if (!string.IsNullOrWhiteSpace(target.TargetComponentId))
    {
        if (!TryResolveTargetComponent(
                target,
                out SparkElectricalComponent component))
            return false;

        if (target.RequireElectricalEnabled &&
            !component.ElectricalEnabled)
            return false;

        if (target.RequireConduction &&
            component.ElectricalState.Conduction !=
                SparkConductionState.Conducting)
            return false;

        voltage = Mathf.Abs(component.ElectricalState.Voltage);
        return true;
    }

    if (!TryResolveTargetTerminal(
            target,
            out SparkTerminal terminal))
        return false;

    if (target.RequireElectricalEnabled &&
        !terminal.IsElectricalEnabled)
        return false;

    voltage = Mathf.Abs(terminal.ElectricalState.Voltage);
    return true;
}

        // ================================================================
        // TERMINAL POWERED
        // ================================================================

        private bool EvaluateTerminalPowered(
            SparkLevelTarget target)
        {
            if (!TryResolveTargetTerminal(
                    target,
                    out SparkTerminal terminal))
            {
                return false;
            }

            if (target.RequireElectricalEnabled &&
                !terminal.IsElectricalEnabled)
            {
                return false;
            }

            float voltage =
                Mathf.Abs(
                    terminal.ElectricalState.Voltage);

            float current =
                Mathf.Abs(
                    terminal.ElectricalState.Current);

            float power =
                Mathf.Abs(
                    terminal.ElectricalState.Power);

            return
                voltage >= target.MinimumVoltage &&
                current >= target.MinimumCurrent &&
                power >= target.MinimumPower;
        }

        // ================================================================
        // VOLTAGE
        // ================================================================

        private bool EvaluateVoltage(
            SparkLevelTarget target)
        {
            if (!TryResolveTargetTerminal(
                    target,
                    out SparkTerminal terminal))
            {
                return false;
            }

            if (target.HasRequiredConnectionPair)
            {
                if (!sceneBindings.TryGetTerminal(
                        target.RequiredPositiveTerminalId,
                        out SparkTerminal positive))
                {
                    return false;
                }

                if (!sceneBindings.TryGetTerminal(
                        target.RequiredNegativeTerminalId,
                        out SparkTerminal negative))
                {
                    return false;
                }

                float voltage =
                    Mathf.Abs(
                        positive.ElectricalState.Voltage -
                        negative.ElectricalState.Voltage);

                return voltage >= target.MinimumVoltage;
            }

            return
                Mathf.Abs(
                    terminal.ElectricalState.Voltage) >=
                target.MinimumVoltage;
        }

        // ================================================================
        // COMPONENT POWERED
        // ================================================================

        private bool EvaluateComponentPowered(
            SparkLevelTarget target)
        {
            if (!TryResolveTargetComponent(
                    target,
                    out SparkElectricalComponent component))
            {
                return false;
            }

            if (target.RequireElectricalEnabled &&
                !component.ElectricalEnabled)
            {
                return false;
            }

            float voltage =
                Mathf.Abs(
                    component.ElectricalState.Voltage);

            float current =
                Mathf.Abs(
                    component.ElectricalState.Current);

            float power =
                Mathf.Abs(
                    component.ElectricalState.Power);

            if (voltage < target.MinimumVoltage)
                return false;

            if (current < target.MinimumCurrent)
                return false;

            if (power < target.MinimumPower)
                return false;

            if (target.RequireConduction &&
                component.ElectricalState.Conduction !=
                SparkConductionState.Conducting)
            {
                return false;
            }

            return true;
        }

        // ================================================================
        // LED
        // ================================================================

        private bool EvaluateLED(
            SparkLevelTarget target)
        {
            if (!TryResolveTargetComponent(
                    target,
                    out SparkElectricalComponent component))
            {
                return false;
            }

            SparkLED led =
                component.GetComponentInParent<SparkLED>();

            if (led == null)
            {
                led =
                    component.GetComponentInChildren<SparkLED>();
            }

            if (led == null)
                return false;

            if (target.RequireElectricalEnabled &&
                !component.ElectricalEnabled)
            {
                return false;
            }

            if (target.RequireConduction &&
                component.ElectricalState.Conduction !=
                SparkConductionState.Conducting)
            {
                return false;
            }

            if (Mathf.Abs(
                    component.ElectricalState.Voltage) <
                target.MinimumVoltage)
            {
                return false;
            }

            if (Mathf.Abs(
                    component.ElectricalState.Current) <
                target.MinimumCurrent)
            {
                return false;
            }

            if (Mathf.Abs(
                    component.ElectricalState.Power) <
                target.MinimumPower)
            {
                return false;
            }

            return led.IsOn;
        }

        // ================================================================
        // COMPONENT CONDUCTING
        // ================================================================

        private bool EvaluateComponentConducting(
            SparkLevelTarget target)
        {
            if (!TryResolveTargetComponent(
                    target,
                    out SparkElectricalComponent component))
            {
                return false;
            }

            if (target.RequireElectricalEnabled &&
                !component.ElectricalEnabled)
            {
                return false;
            }

            if (component.ElectricalState.Conduction !=
                SparkConductionState.Conducting)
            {
                return false;
            }

            float voltage =
                Mathf.Abs(
                    component.ElectricalState.Voltage);

            float current =
                Mathf.Abs(
                    component.ElectricalState.Current);

            float power =
                Mathf.Abs(
                    component.ElectricalState.Power);

            return
                voltage >= target.MinimumVoltage &&
                current >= target.MinimumCurrent &&
                power >= target.MinimumPower;
        }

        // ================================================================
        // TARGET TERMINAL RESOLUTION
        // ================================================================

        private bool TryResolveTargetTerminal(
            SparkLevelTarget target,
            out SparkTerminal terminal)
        {
            terminal = null;

            if (target == null ||
                sceneBindings == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    target.TargetTerminalId))
            {
                return false;
            }

            return sceneBindings.TryGetTerminal(
                target.TargetTerminalId,
                out terminal);
        }

        // ================================================================
        // TARGET COMPONENT RESOLUTION
        // ================================================================

        private bool TryResolveTargetComponent(
            SparkLevelTarget target,
            out SparkElectricalComponent component)
        {
            component = null;

            if (target == null ||
                sceneBindings == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    target.TargetComponentId))
            {
                return false;
            }

            return sceneBindings.TryGetComponent(
                target.TargetComponentId,
                out component);
        }

        // ================================================================
        // TARGET TERMINAL PAIR
        // ================================================================

        private bool TryResolveTargetTerminalPair(
            SparkLevelTarget target,
            out SparkTerminal positiveTerminal,
            out SparkTerminal negativeTerminal)
        {
            positiveTerminal = null;
            negativeTerminal = null;

            if (target == null ||
                sceneBindings == null)
            {
                return false;
            }

            if (!target.HasRequiredConnectionPair)
                return false;

            bool positiveResolved =
                sceneBindings.TryGetTerminal(
                    target.RequiredPositiveTerminalId,
                    out positiveTerminal);

            bool negativeResolved =
                sceneBindings.TryGetTerminal(
                    target.RequiredNegativeTerminalId,
                    out negativeTerminal);

            return
                positiveResolved &&
                negativeResolved &&
                positiveTerminal != null &&
                negativeTerminal != null;
        }

        // ================================================================
        // TARGET ELECTRICAL STATE
        // ================================================================

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

    // Prefer the target component's solved electrical state.
    // Component state contains voltage, current and power.
    // Terminal state may contain voltage but zero current/power.
    if (TryResolveTargetComponent(
            target,
            out SparkElectricalComponent component))
    {
        voltage = Mathf.Abs(
            component.ElectricalState.Voltage);

        current = Mathf.Abs(
            component.ElectricalState.Current);

        power = Mathf.Abs(
            component.ElectricalState.Power);

        return;
    }

    // Fallback for targets configured using a terminal pair.
    if (TryResolveTargetTerminalPair(
            target,
            out SparkTerminal positive,
            out SparkTerminal negative))
    {
        voltage = Mathf.Abs(
            positive.ElectricalState.Voltage -
            negative.ElectricalState.Voltage);

        current = Mathf.Max(
            Mathf.Abs(positive.ElectricalState.Current),
            Mathf.Abs(negative.ElectricalState.Current));

        power = voltage * current;

        return;
    }

    // Fallback for single-terminal targets.
    if (TryResolveTargetTerminal(
            target,
            out SparkTerminal terminal))
    {
        voltage = Mathf.Abs(
            terminal.ElectricalState.Voltage);

        current = Mathf.Abs(
            terminal.ElectricalState.Current);

        power = Mathf.Abs(
            terminal.ElectricalState.Power);
    }
}


        // ================================================================
        // TARGET SHORT
        // ================================================================

        private bool DetectTargetShort(
            SparkLevelDefinition level)
        {
            SparkLevelTarget target =
                GetPrimaryTarget(level);

            if (target == null)
                return false;

            if (!TryResolveTargetTerminalPair(
                    target,
                    out SparkTerminal targetPositive,
                    out SparkTerminal targetNegative))
            {
                return false;
            }

            positiveReachable.Clear();

            TraverseWireNetwork(
                targetPositive,
                positiveReachable,
                traverseConductingLeds: false,
                bypassOnly: true);

            if (positiveReachable.Contains(
                    targetNegative))
            {
                return true;
            }

            // ------------------------------------------------------------
            // ELECTRICAL FALLBACK
            // ------------------------------------------------------------

            float voltage =
                Mathf.Abs(
                    targetPositive.ElectricalState.Voltage -
                    targetNegative.ElectricalState.Voltage);

            float current =
                Mathf.Max(
                    Mathf.Abs(
                        targetPositive.ElectricalState.Current),
                    Mathf.Abs(
                        targetNegative.ElectricalState.Current));

            return
                voltage <= 0.005f &&
                current > 0.01f;
        }

        // ================================================================
        // PRIMARY TARGET
        // ================================================================

        private SparkLevelTarget GetPrimaryTarget(
            SparkLevelDefinition level)
        {
            if (level == null ||
                level.Targets == null)
            {
                return null;
            }

            SparkLevelTarget[] targets =
                level.Targets;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                    return targets[i];
            }

            return null;
        }
                // ================================================================
        // TERMINAL DEBUG NAME
        // ================================================================

        private string GetTerminalDebugName(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return "NULL";
            }

            string ownerName = "NoOwner";
            string ownerObjectName = "NoOwnerObject";

            if (terminal.Owner != null)
            {
                ownerName =
                    terminal.Owner.GetType().Name;

                if (terminal.Owner is Component ownerComponent)
                {
                    ownerObjectName =
                        ownerComponent.gameObject.name;
                }
            }

            return
                $"{terminal.name}" +
                $" [ID={terminal.GetInstanceID()}]" +
                $" [Owner={ownerName}]" +
                $" [GO={ownerObjectName}]" +
                $" [TerminalGO={terminal.gameObject.name}]";
        }

        public bool IsTargetSatisfiedForDiagnostics(
            SparkLevelTarget target)
        {
            if (target == null)
                return false;

            // Reuse the same target logic used by
            // the authoritative level evaluation.
            return EvaluateTarget(target);
        }

    }
}