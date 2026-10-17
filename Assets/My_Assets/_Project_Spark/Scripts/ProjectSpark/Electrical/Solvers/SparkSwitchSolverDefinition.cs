
using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for the conventional two-terminal SparkSwitch.
    ///
    /// CLOSED:
    ///     Stamps a conductance between input and output.
    ///
    /// OPEN:
    ///     Stamps no connection between input and output.
    ///
    /// This is a resistive switch, not a controlled voltage source.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkSwitchSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Switch")]
    public sealed class SparkSwitchSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [Header("Solver")]
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkSwitch);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkSwitch;
        }

        // ---------------------------------------------------------
        // TERMINALS
        // ---------------------------------------------------------

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkSwitch sw))
                return false;

            if (sw.InputTerminal == null ||
                sw.OutputTerminal == null)
            {
                return false;
            }

            terminalA = sw.InputTerminal;
            terminalB = sw.OutputTerminal;

            return true;
        }

        // ---------------------------------------------------------
        // STAMP
        // ---------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitch sw))
                return;

            if (context == null ||
                context.Graph == null)
            {
                return;
            }

            // Disabled and open switches are genuine open circuits.
            if (!sw.ElectricalEnabled || !sw.IsClosed)
                return;

            if (!TryGetTerminals(
                    sw,
                    context.Graph,
                    out SparkTerminal input,
                    out SparkTerminal output))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    input,
                    out int inputGraphNode))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    output,
                    out int outputGraphNode))
            {
                return;
            }

            // Both terminals already share the same graph node.
            if (inputGraphNode == outputGraphNode)
                return;

            float resistance = Mathf.Max(
                minimumResistance,
                sw.ClosedResistance);

            double conductance = 1.0 / resistance;

            // AddConductance maps reference nodes correctly.
            context.AddConductance(
                input,
                output,
                conductance);

#if UNITY_EDITOR
            // Avoid logging the full matrix on every solve.
            // Enable this log temporarily when diagnosing a switch.
            /*
            Debug.Log(
                $"[SWITCH STAMP] {sw.name} | " +
                $"State={sw.State} | " +
                $"R={resistance:G6} Ohm | " +
                $"G={conductance:G6} S | " +
                $"InputNode={inputGraphNode} | " +
                $"OutputNode={outputGraphNode}",
                sw);
            */
#endif
        }

        // ---------------------------------------------------------
        // APPLY SOLVED STATE
        // ---------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkSwitch sw))
                return;

            if (result == null ||
                result.Context == null ||
                result.Context.Graph == null)
            {
                sw.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    sw,
                    result.Context.Graph,
                    out SparkTerminal input,
                    out SparkTerminal output))
            {
                sw.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            // The voltage across the switch is Vin - Vout.
            if (!result.TryGetVoltage(
                    input,
                    output,
                    out float voltage))
            {
                sw.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float current = 0f;

            if (sw.ElectricalEnabled && sw.IsClosed)
            {
                if (!TryCalculateCurrent(
                        sw,
                        result,
                        out current))
                {
                    current = 0f;
                }
            }

            float power = voltage * current;

            SparkConductionState conduction =
                sw.ElectricalEnabled &&
                sw.IsClosed &&
                Mathf.Abs(current) > 0.000001f
                    ? SparkConductionState.Conducting
                    : SparkConductionState.NonConducting;

            SparkElectricalState solvedState =
                new SparkElectricalState(
                    voltage,
                    current,
                    power,
                    conduction);

            sw.ApplyElectricalState(solvedState);

#if UNITY_EDITOR
            /*
            Debug.Log(
                $"[SWITCH SOLVED] {sw.name} | " +
                $"State={sw.State} | " +
                $"Enabled={sw.ElectricalEnabled} | " +
                $"Vin={input.ElectricalState.Voltage:F4} V | " +
                $"Vout={output.ElectricalState.Voltage:F4} V | " +
                $"Vswitch={voltage:F4} V | " +
                $"I={current:F6} A | " +
                $"P={power:F6} W | " +
                $"Conduction={conduction}",
                sw);
            */
#endif
        }

        // ---------------------------------------------------------
        // CURRENT
        // ---------------------------------------------------------

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkSwitch sw))
                return false;

            if (result == null ||
                result.Context == null ||
                result.Context.Graph == null)
            {
                return false;
            }

            // An open or disabled switch carries no switch current.
            if (!sw.ElectricalEnabled || !sw.IsClosed)
                return true;

            if (!TryGetTerminals(
                    sw,
                    result.Context.Graph,
                    out SparkTerminal input,
                    out SparkTerminal output))
            {
                return false;
            }

            if (!result.TryGetVoltage(
                    input,
                    output,
                    out float voltage))
            {
                return false;
            }

            float resistance = Mathf.Max(
                minimumResistance,
                sw.ClosedResistance);

            current = voltage / resistance;

            return true;
        }

        // ---------------------------------------------------------
        // CONTINUOUS SOLVE
        // ---------------------------------------------------------

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkSwitch sw))
                return false;

            // Preserve your existing continuous-solve behavior.
            return sw.ElectricalEnabled;
        }

        // ---------------------------------------------------------
        // VALIDATION
        // ---------------------------------------------------------

        protected override void OnValidate()
        {
            base.OnValidate();

            minimumResistance = Mathf.Max(
                0.000001f,
                minimumResistance);
        }
    }
}
