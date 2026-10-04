using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for the electrical motor used by the
    /// Project Spark hair dryer.
    ///
    /// The motor is modeled as a two-terminal resistive load:
    ///
    ///     Live ─── Motor Resistance ─── Neutral
    ///
    /// Runtime electrical state remains on
    /// SparkHairDryerMotorElectrical.
    ///
    /// This ScriptableObject contains only solver configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkHairDryerMotorSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Hair Dryer Motor")]
    public sealed class SparkHairDryerMotorSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        // ---------------------------------------------------------------------
        // Identity
        // ---------------------------------------------------------------------

        public override Type SupportedComponentType =>
            typeof(SparkHairDryerMotorElectrical);

        // ---------------------------------------------------------------------
        // Component matching
        // ---------------------------------------------------------------------

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkHairDryerMotorElectrical;
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

            if (component == null ||
                graph == null)
            {
                return false;
            }

            if (!(component is SparkHairDryerMotorElectrical motor))
            {
                return false;
            }

            if (motor.LiveTerminal == null ||
                motor.NeutralTerminal == null)
            {
                return false;
            }

            terminalA = motor.LiveTerminal;
            terminalB = motor.NeutralTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // Matrix stamping
        // ---------------------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkHairDryerMotorElectrical motor))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!motor.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    motor,
                    context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    liveTerminal,
                    out int liveNode))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    neutralTerminal,
                    out int neutralNode))
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    motor.Resistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                liveNode,
                neutralNode,
                conductance);
        }

        // ---------------------------------------------------------------------
        // Solved electrical state
        // ---------------------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkHairDryerMotorElectrical motor))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!motor.ElectricalEnabled)
            {
                motor.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    motor,
                    result.Context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                motor.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    liveTerminal,
                    neutralTerminal);

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    motor.Resistance);

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

            motor.ApplyElectricalState(
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

            if (!(component is SparkHairDryerMotorElectrical motor))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!motor.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    motor,
                    result.Context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    motor.Resistance);

            float voltage =
                result.GetVoltage(
                    liveTerminal,
                    neutralTerminal);

            current =
                voltage / resistance;

            return true;
        }

        // ---------------------------------------------------------------------
        // Continuous solve
        // ---------------------------------------------------------------------

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkHairDryerMotorElectrical motor))
            {
                return false;
            }

            return motor.ElectricalEnabled;
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