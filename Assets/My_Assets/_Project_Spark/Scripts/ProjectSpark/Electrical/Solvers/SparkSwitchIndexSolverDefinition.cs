using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkSwitchIndexSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Switch Index")]
    public sealed class SparkSwitchIndexSolverDefinition :
        SparkElectricalSolverDefinition
    {
        // ---------------------------------------------------------------------
        // COMPONENT
        // ---------------------------------------------------------------------

        public override Type SupportedComponentType =>
            typeof(SparkSwitchIndex);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkSwitchIndex;
        }

        // ---------------------------------------------------------------------
        // TOPOLOGY
        // ---------------------------------------------------------------------

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return;

            if (context == null)
                return;

            if (!indexedSwitch.ElectricalEnabled)
                return;

            /*
             * IMPORTANT:
             *
             * OFF is a real OPEN circuit.
             *
             * Do NOT allocate an MNA voltage source when the
             * switch is OFF.
             *
             * If Index 0 were registered as a controlled source
             * with ratio 0, the solver would enforce:
             *
             *     Vout = 0V
             *
             * which is NOT an open switch.
             */

            if (indexedSwitch.IsOff)
                return;

            context.RegisterIndexedSource(
                indexedSwitch);
        }

        // ---------------------------------------------------------------------
        // TERMINALS
        // ---------------------------------------------------------------------

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            if (indexedSwitch.InputTerminal == null ||
                indexedSwitch.OutputTerminal == null)
            {
                return false;
            }

            terminalA =
                indexedSwitch.InputTerminal;

            terminalB =
                indexedSwitch.OutputTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // STAMP
        // ---------------------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return;

            if (context == null)
                return;

            if (!indexedSwitch.ElectricalEnabled)
                return;

            /*
             * OFF = OPEN.
             *
             * No voltage source.
             * No conductance.
             * No connection between input and output.
             */
            if (indexedSwitch.IsOff)
                return;

            if (!TryGetTerminals(
                    indexedSwitch,
                    context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                return;
            }

            if (!context.TryGetIndexedSourceIndex(
                    indexedSwitch,
                    out int sourceIndex))
            {
                return;
            }

            float ratio =
                Mathf.Clamp01(
                    indexedSwitch.CurrentIndexVoltagePercent /
                    100f);

            /*
             * Active indexed switch:
             *
             * Vout = Vin * ratio
             *
             * Index 1:
             *     Vout = Vin * 0.5
             *
             * Index 2:
             *     Vout = Vin * 1.0
             */

            context.AddControlledVoltageSource(
                inputTerminal,
                outputTerminal,
                sourceIndex,
                ratio);
        }

        // ---------------------------------------------------------------------
        // APPLY SOLVED STATE
        // ---------------------------------------------------------------------

      public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkSwitchIndex indexedSwitch))
        return;

    if (result == null)
        return;

    /*
     * ============================================================
     * OFF / DISABLED
     * ============================================================
     *
     * Index 0 is an OPEN switch.
     *
     * There is no meaningful voltage-source current to read.
     * Clear the electrical state completely so an old ON-state
     * voltage/current/power cannot remain visible.
     */
    if (!indexedSwitch.ElectricalEnabled ||
        indexedSwitch.IsOff)
    {
        indexedSwitch.ApplyElectricalState(
            new SparkElectricalState());

        Debug.Log(
            $"[INDEX SWITCH SOLVER] " +
            $"Name={indexedSwitch.name} | " +
            $"Index={indexedSwitch.CurrentIndex} | " +
            $"Percent={indexedSwitch.CurrentIndexVoltagePercent:F1}% | " +
            $"InputV=OPEN | " +
            $"OutputV=OPEN | " +
            $"SwitchV=0.000 | " +
            $"Current=0.000000 | " +
            $"Power=0.000 | " +
            $"Off=True | " +
            $"Conducting=False"
        );

        return;
    }

    /*
     * ============================================================
     * TERMINALS
     * ============================================================
     */
    if (!TryGetTerminals(
            indexedSwitch,
            result.Context.Graph,
            out SparkTerminal inputTerminal,
            out SparkTerminal outputTerminal))
    {
        indexedSwitch.ApplyElectricalState(
            new SparkElectricalState());

        Debug.LogWarning(
            $"[INDEX SWITCH SOLVER] " +
            $"Name={indexedSwitch.name} | " +
            $"Index={indexedSwitch.CurrentIndex} | " +
            $"FAILED: Input/Output terminals could not be resolved."
        );

        return;
    }

    /*
     * ============================================================
     * INPUT / OUTPUT VOLTAGE
     * ============================================================
     *
     * Input voltage:
     *     voltage at switch input terminal
     *
     * Output voltage:
     *     voltage at switch output terminal
     *
     * Switch voltage:
     *     Vin - Vout
     */
    float inputVoltage =
        result.GetTerminalVoltage(inputTerminal);

    float outputVoltage =
        result.GetTerminalVoltage(outputTerminal);

    float voltage =
        inputVoltage - outputVoltage;

    /*
     * ============================================================
     * INDEXED VOLTAGE-SOURCE CURRENT
     * ============================================================
     *
     * The current comes from the MNA controlled voltage source
     * registered for this indexed switch.
     */
    float current = 0f;

    result.TryGetIndexedSourceCurrent(
        indexedSwitch,
        out current);

    /*
     * ============================================================
     * POWER
     * ============================================================
     */
    float power =
        voltage * current;

    /*
     * ============================================================
     * CONDUCTION
     * ============================================================
     *
     * Only consider the switch conducting when there is actual
     * source current.
     */
    SparkConductionState conduction =
        Mathf.Abs(current) > 0.000001f
            ? SparkConductionState.Conducting
            : SparkConductionState.NonConducting;

    /*
     * ============================================================
     * APPLY ELECTRICAL STATE
     * ============================================================
     */
    SparkElectricalState state =
        new SparkElectricalState(
            voltage,
            current,
            power,
            conduction);

    indexedSwitch.ApplyElectricalState(state);

    /*
     * ============================================================
     * DIAGNOSTIC LOG
     * ============================================================
     *
     * This is temporary but useful for Level 4 debugging.
     *
     * Expected:
     *
     * Index 0:
     *     Off=True
     *
     * Index 1:
     *     OutputV ~= InputV * 0.5
     *
     * Index 2:
     *     OutputV ~= InputV
     *
     * Note:
     * SwitchV is the voltage DROP across the switch itself.
     * It is NOT the output voltage.
     */
    Debug.Log(
        $"[INDEX SWITCH SOLVER] " +
        $"Name={indexedSwitch.name} | " +
        $"Index={indexedSwitch.CurrentIndex} | " +
        $"Percent={indexedSwitch.CurrentIndexVoltagePercent:F1}% | " +
        $"InputV={inputVoltage:F3} | " +
        $"OutputV={outputVoltage:F3} | " +
        $"SwitchV={voltage:F3} | " +
        $"Current={current:F6} | " +
        $"Power={power:F3} | " +
        $"Off={indexedSwitch.IsOff} | " +
        $"Conducting={indexedSwitch.IsConducting}"
    );
}
        // ---------------------------------------------------------------------
        // CURRENT
        // ---------------------------------------------------------------------

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            if (result == null)
                return false;

            /*
             * OFF has no allocated indexed source.
             */
            if (indexedSwitch.IsOff)
                return true;

            return result.TryGetIndexedSourceCurrent(
                indexedSwitch,
                out current);
        }

        // ---------------------------------------------------------------------
        // CONTINUOUS SOLVE
        // ---------------------------------------------------------------------

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            /*
             * Keep the existing behaviour:
             * the indexed switch participates in continuous
             * solving whenever electrically enabled.
             */
            return indexedSwitch.ElectricalEnabled;
        }
    }
}