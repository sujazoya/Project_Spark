using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkHairDryerHeaterSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Hair Dryer Heater")]
    public sealed class SparkHairDryerHeaterSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkHairDryerHeaterElectrical);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkHairDryerHeaterElectrical;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            if (heater.LiveTerminal == null ||
                heater.NeutralTerminal == null)
            {
                return false;
            }

            terminalA = heater.LiveTerminal;
            terminalB = heater.NeutralTerminal;

            return true;
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!heater.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    heater,
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
                    heater.Resistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                liveNode,
                neutralNode,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!heater.ElectricalEnabled)
            {
                heater.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    heater,
                    result.Context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                heater.ApplyElectricalState(
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
                    heater.Resistance);

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

            heater.ApplyElectricalState(
                state);
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!heater.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    heater,
                    result.Context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    heater.Resistance);

            float voltage =
                result.GetVoltage(
                    liveTerminal,
                    neutralTerminal);

            current =
                voltage / resistance;

            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            return heater.ElectricalEnabled;
        }

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