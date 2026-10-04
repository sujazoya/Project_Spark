using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkSingleWireSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Single Wire")]
    public sealed class SparkSingleWireSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000000001f)]
        private float wireResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkSingleWire);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkSingleWire;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkSingleWire wire))
            {
                return false;
            }

            if (graph == null)
            {
                return false;
            }

            return graph.TryGetPrimaryComponentTerminals(
                wire,
                out terminalA,
                out terminalB);
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSingleWire wire))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!wire.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    wire,
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
                    0.000000001f,
                    wireResistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                nodeA,
                nodeB,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkSingleWire wire))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!wire.ElectricalEnabled)
            {
                wire.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    wire,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                wire.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    terminalA,
                    terminalB);

            float resistance =
                Mathf.Max(
                    0.000000001f,
                    wireResistance);

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

            wire.ApplyElectricalState(
                state);
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkSingleWire wire))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!wire.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    wire,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                return false;
            }

            float voltage =
                result.GetVoltage(
                    terminalA,
                    terminalB);

            float resistance =
                Mathf.Max(
                    0.000000001f,
                    wireResistance);

            current =
                voltage / resistance;

            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkSingleWire wire))
            {
                return false;
            }

            return wire.ElectricalEnabled;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            wireResistance =
                Mathf.Max(
                    0.000000001f,
                    wireResistance);
        }
    }
}