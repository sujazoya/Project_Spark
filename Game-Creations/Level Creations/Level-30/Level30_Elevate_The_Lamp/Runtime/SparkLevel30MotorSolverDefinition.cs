using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(menuName = "Project Spark/Electrical/Solver Definitions/Level 30 Reversible Motor")]
    public sealed class SparkLevel30MotorSolverDefinition : SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType => typeof(SparkMotor);
        public override bool CanHandle(SparkElectricalComponent component) => component is SparkMotor;

        public override bool TryGetTerminals(SparkElectricalComponent component, SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA, out SparkTerminal terminalB)
        {
            var motor = component as SparkMotor;
            terminalA = motor != null ? motor.PositiveTerminal : null;
            terminalB = motor != null ? motor.NegativeTerminal : null;
            return graph != null && terminalA != null && terminalB != null && terminalA != terminalB &&
                graph.TryGetNode(terminalA, out _) && graph.TryGetNode(terminalB, out _);
        }

        public override void Stamp(SparkElectricalComponent component, SparkElectricalSolveContext context)
        {
            if (component is SparkMotor motor && motor.isActiveAndEnabled && motor.ElectricalEnabled &&
                context != null && TryGetTerminals(motor, context.Graph, out var a, out var b))
                context.AddConductance(a, b, 1d / Resistance(motor, context));
        }

        public override void ApplySolvedState(SparkElectricalComponent component, SparkElectricalSolveResult result)
        {
            if (!(component is SparkMotor motor)) return;
            if (TryRead(motor, result, out float voltage, out float current))
                motor.ApplyElectricalState(new SparkElectricalState(voltage, current, voltage * current,
                    Mathf.Abs(current) > 0.000001f ? SparkConductionState.Conducting : SparkConductionState.NonConducting));
            else
                motor.ApplyElectricalState(new SparkElectricalState());
        }

        public override bool TryCalculateCurrent(SparkElectricalComponent component, SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            return component is SparkMotor motor && TryRead(motor, result, out _, out current);
        }

        private bool TryRead(SparkMotor motor, SparkElectricalSolveResult result, out float voltage, out float current)
        {
            voltage = current = 0f;
            if (!motor.isActiveAndEnabled || !motor.ElectricalEnabled || result?.Context == null ||
                !TryGetTerminals(motor, result.Context.Graph, out var a, out var b) ||
                !result.TryGetVoltage(a, b, out voltage) || float.IsNaN(voltage) || float.IsInfinity(voltage))
                return false;
            current = (float)(voltage / Resistance(motor, result.Context));
            return !float.IsNaN(current) && !float.IsInfinity(current);
        }

        private static double Resistance(SparkMotor motor, SparkElectricalSolveContext context) =>
            Math.Max(context.MinimumResistance, motor.WindingResistance);
    }
}
