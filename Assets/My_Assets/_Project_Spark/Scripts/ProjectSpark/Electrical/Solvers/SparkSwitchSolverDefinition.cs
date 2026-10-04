using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for a normal two-terminal SparkSwitch.
    ///
    /// Open switch:
    ///     No electrical conductance.
    ///
    /// Closed switch:
    ///     Conducts through ClosedResistance.
    ///
    /// Runtime electrical state remains on SparkSwitch.
    /// This ScriptableObject contains only solver configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkSwitchSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Switch")]
    public sealed class SparkSwitchSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        // ---------------------------------------------------------------------
        // Identity
        // ---------------------------------------------------------------------

        public override Type SupportedComponentType =>
            typeof(SparkSwitch);

        // ---------------------------------------------------------------------
        // Component matching
        // ---------------------------------------------------------------------

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkSwitch;
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

            if (!(component is SparkSwitch switchComponent))
            {
                return false;
            }

            if (switchComponent.InputTerminal == null ||
                switchComponent.OutputTerminal == null)
            {
                return false;
            }

            terminalA =
                switchComponent.InputTerminal;

            terminalB =
                switchComponent.OutputTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // Matrix stamping
        // ---------------------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitch switchComponent))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!switchComponent.ElectricalEnabled)
            {
                return;
            }

            /*
             * An open switch is an electrical open circuit.
             */
            if (!switchComponent.IsConducting)
            {
                return;
            }

            if (!TryGetTerminals(
                    switchComponent,
                    context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    inputTerminal,
                    out int inputNode))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    outputTerminal,
                    out int outputNode))
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    switchComponent.ClosedResistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                inputNode,
                outputNode,
                conductance);
        }

        // ---------------------------------------------------------------------
        // Solved electrical state
        // ---------------------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkSwitch switchComponent))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!switchComponent.ElectricalEnabled)
            {
                switchComponent.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    switchComponent,
                    result.Context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                switchComponent.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    inputTerminal,
                    outputTerminal);

            float current = 0f;

            /*
             * Open switch:
             * no conduction and therefore no switch current.
             */
            if (switchComponent.IsConducting)
            {
                float resistance =
                    Mathf.Max(
                        minimumResistance,
                        switchComponent.ClosedResistance);

                current =
                    voltage / resistance;
            }

            float power =
                voltage * current;

            SparkConductionState conduction =
                switchComponent.IsConducting &&
                Mathf.Abs(current) > 0.000001f
                    ? SparkConductionState.Conducting
                    : SparkConductionState.NonConducting;

            SparkElectricalState state =
                new SparkElectricalState(
                    voltage,
                    current,
                    power,
                    conduction);

            switchComponent.ApplyElectricalState(
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

            if (!(component is SparkSwitch switchComponent))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!switchComponent.ElectricalEnabled)
            {
                return true;
            }

            if (!switchComponent.IsConducting)
            {
                return true;
            }

            if (!TryGetTerminals(
                    switchComponent,
                    result.Context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    switchComponent.ClosedResistance);

            float voltage =
                result.GetVoltage(
                    inputTerminal,
                    outputTerminal);

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