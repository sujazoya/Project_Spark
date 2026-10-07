using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for a two-terminal resistive component.
    ///
    /// Responsibilities:
    /// - Identify SparkResistor components.
    /// - Expose their two electrical terminals.
    /// - Stamp their conductance into the solver matrix.
    /// - Apply solved voltage/current/power state.
    /// - Calculate resistor current for the runtime electrical state.
    ///
    /// Runtime state is NOT stored in this ScriptableObject.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkResistorSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Resistor")]
    public sealed class SparkResistorSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkResistor);

        // ---------------------------------------------------------------------
        // Component matching
        // ---------------------------------------------------------------------

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkResistor;
        }

        // ---------------------------------------------------------------------
        // Terminal access
        // ---------------------------------------------------------------------

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkResistor resistor))
            {
                return false;
            }

            if (resistor.TerminalA == null ||
                resistor.TerminalB == null)
            {
                return false;
            }

            terminalA = resistor.TerminalA;
            terminalB = resistor.TerminalB;

            return true;
        }

        // ---------------------------------------------------------------------
        // Matrix stamping
        // ---------------------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkResistor resistor))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!resistor.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    resistor,
                    context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    terminalA,
                    out int nodeA))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    terminalB,
                    out int nodeB))
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    resistor.EffectiveResistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                nodeA,
                nodeB,
                conductance);
        }

        // ---------------------------------------------------------------------
        // Solved electrical state
        // ---------------------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkResistor resistor))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!resistor.ElectricalEnabled)
            {
                resistor.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    resistor,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                resistor.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    terminalA,
                    terminalB);

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    resistor.EffectiveResistance);

            float current =
                voltage / resistance;

            float power =
                voltage * current;

            SparkConductionState conduction =
                Mathf.Abs(current) > 0.000001f
                    ? SparkConductionState.Conducting
                    : SparkConductionState.NonConducting;

            SparkElectricalState state =
                new SparkElectricalState(
                    voltage,
                    current,
                    power,
                    conduction);

            resistor.ApplyElectricalState(
                state);
        }

        // ---------------------------------------------------------------------
        // Current calculation
        // ---------------------------------------------------------------------

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkResistor resistor))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!resistor.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    resistor,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    resistor.EffectiveResistance);

            float voltage =
                result.GetVoltage(
                    terminalA,
                    terminalB);

            current =
                voltage / resistance;

            return true;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------

        protected override void OnValidate()
        {
            base.OnValidate();

            minimumResistance =
                Mathf.Max(
                    0.000001f,
                    minimumResistance);
        }
    }
}