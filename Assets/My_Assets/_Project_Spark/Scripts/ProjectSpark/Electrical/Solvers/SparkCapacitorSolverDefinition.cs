using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkCapacitorSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Capacitor")]
    public sealed class SparkCapacitorSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000000001f)]
        private float minimumCapacitance = 0.000000001f;       

        public override Type SupportedComponentType =>
            typeof(SparkCapacitor);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkCapacitor;
        }

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

    return graph.TryGetPrimaryComponentTerminals(
        component,
        out terminalA,
        out terminalB);
}

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkCapacitor capacitor))
            {
                return;
            }

            if (context == null ||
                !context.SimulateTransientDevices)
            {
                return;
            }

            if (!TryGetTerminals(
                    capacitor,
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

            float capacitance =
                Mathf.Max(
                    minimumCapacitance,
                    capacitor.CapacitanceFarads);

            float previousVoltage = 0f;

            if (!context.TryGetPreviousComponentVoltage(
                    capacitor,
                    out previousVoltage))
            {
                previousVoltage = 0f;
            }

            float timeStep =
                Mathf.Max(
                    0.000001f,
                    context.TimeStep);

            double conductance =
                capacitance /
                timeStep;

            context.AddConductance(
                nodeA,
                nodeB,
                conductance);

           context.AddCurrentSource(
    terminalA,
    terminalB,
    conductance * previousVoltage);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkCapacitor capacitor) || result == null)
                return;

            if (!TryGetTerminals(capacitor, result.Context.Graph,
                    out SparkTerminal terminalA, out SparkTerminal terminalB))
                return;

            if (!result.TryGetVoltage(terminalA, terminalB, out float voltage))
                return;

            float current = 0f;
            if (result.Context.SimulateTransientDevices)
            {
                // Match Stamp(): a missing history entry means an initially
                // uncharged capacitor, i.e. a previous voltage of zero.
                float previousVoltage = 0f;
                result.TryGetPreviousComponentVoltage(capacitor, out previousVoltage);

                float capacitance = Mathf.Max(minimumCapacitance,
                    capacitor.CapacitanceFarads);
                float timeStep = Mathf.Max(0.000001f, result.Context.TimeStep);
                current = capacitance * (voltage - previousVoltage) / timeStep;
            }

            float power = voltage * current;
            SparkConductionState conduction = Mathf.Abs(current) > 0.000001f
                ? SparkConductionState.Conducting
                : SparkConductionState.NonConducting;

            capacitor.ApplyElectricalState(new SparkElectricalState(
                voltage, current, power, conduction));

            // Do not advance history here. The solver calls
            // CommitTransientState after all device states are published.
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            if (!(component is SparkCapacitor capacitor) || result == null)
                return false;

            if (!TryGetTerminals(capacitor, result.Context.Graph,
                    out SparkTerminal terminalA, out SparkTerminal terminalB))
                return false;

            if (!result.TryGetVoltage(terminalA, terminalB, out float voltage))
                return false;

            if (!result.Context.SimulateTransientDevices)
                return true;

            // CommitTransientState has already advanced the history by the
            // time external callers query the accepted solution. Recomputing
            // from that updated history would incorrectly return zero.
            // ApplySolvedState stores the current for this accepted solve.
            current = capacitor.ElectricalState.Current;
            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            return component is SparkCapacitor;
        }

        public override void CommitTransientState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkCapacitor capacitor) || result == null ||
                !result.Context.SimulateTransientDevices)
                return;

            if (!TryGetTerminals(capacitor, result.Context.Graph,
                    out SparkTerminal terminalA, out SparkTerminal terminalB))
                return;

            if (!result.TryGetVoltage(terminalA, terminalB, out float voltage))
                return;

            result.CommitPreviousComponentVoltage(capacitor, voltage);
        }

        public override void ResetRuntimeState(
            SparkElectricalComponent component)
        {
            // Runtime capacitor voltage is owned by the solver.
            // This ScriptableObject contains configuration only.
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            minimumCapacitance =
                Mathf.Max(
                    0.000000001f,
                    minimumCapacitance);
        }
    }
}