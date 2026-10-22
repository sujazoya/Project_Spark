
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

            if (!(component is SparkSingleWire wire) ||
                graph == null)
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
    if (!(component is SparkSingleWire wire) ||
        context == null ||
        context.Graph == null ||
        !wire.ElectricalEnabled)
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

    float resistance = Mathf.Max(
        context.MinimumResistance,
        wire.WireResistance);

    double conductance = 1.0 / resistance;

    // IMPORTANT:
    // Pass SparkTerminal objects, not graph node indices.
    context.AddConductance(
        terminalA,
        terminalB,
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

            if (result == null ||
                result.Context == null ||
                result.Context.Graph == null ||
                !wire.ElectricalEnabled)
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

            if (!result.TryGetVoltage(
                    terminalA,
                    terminalB,
                    out float voltage))
            {
                wire.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

           float resistance = Mathf.Max(
    result.Context.MinimumResistance,
    wire.WireResistance);

            float current = voltage / resistance;
            float power = voltage * current;

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

            wire.ApplyElectricalState(state);
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

            if (result == null ||
                result.Context == null ||
                result.Context.Graph == null)
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

            if (!result.TryGetVoltage(
                    terminalA,
                    terminalB,
                    out float voltage))
            {
                return false;
            }

           float resistance = Mathf.Max(
    result.Context.MinimumResistance,
    wire.WireResistance);

            current = voltage / resistance;

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

            wireResistance = Mathf.Max(
                0.000000001f,
                wireResistance);
        }
    }
}
