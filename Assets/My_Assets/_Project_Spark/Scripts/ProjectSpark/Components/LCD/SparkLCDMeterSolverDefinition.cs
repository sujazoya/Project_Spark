using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(fileName = "SparkLCDMeterSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/LCD Meter")]
    public sealed class SparkLCDMeterSolverDefinition : SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType => typeof(SparkLCDMeter);

        public override bool CanHandle(SparkElectricalComponent component)
        {
            return component is SparkLCDMeter;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component, SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA, out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;
            if (!(component is SparkLCDMeter lcd)) return false;
            terminalA = lcd.TerminalA;
            terminalB = lcd.TerminalB;
            return terminalA != null && terminalB != null && terminalA != terminalB;
        }

        public override void Stamp(SparkElectricalComponent component, SparkElectricalSolveContext context)
        {
            if (!(component is SparkLCDMeter lcd) || context == null || !lcd.ElectricalEnabled)
                return;
            if (!TryGetTerminals(lcd, context.Graph, out SparkTerminal a, out SparkTerminal b))
                return;
            if (!context.Graph.TryGetNode(a, out _) || !context.Graph.TryGetNode(b, out _))
                return;

            // This known Project Spark API stamps a resistor plus a zero forward drop.
            // It avoids adding a second solver or calling an unverified stamp method.
            context.AddForwardDrop(a, b, 0f, lcd.MeterResistanceOhms);
            lcd.RecordStampedResistance();
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component, SparkElectricalSolveResult result)
        {
            if (!(component is SparkLCDMeter lcd)) return;
            if (result == null || !lcd.ElectricalEnabled ||
                !TryGetTerminals(lcd, result.Context.Graph, out SparkTerminal a, out SparkTerminal b) ||
                !result.TryGetVoltage(a, b, out float voltage))
            {
                lcd.ClearSolvedMeterState();
                lcd.ApplyElectricalState(new SparkElectricalState());
                return;
            }

            float current = voltage / lcd.MeterResistanceOhms;
            float watts = Mathf.Abs(voltage * current);
            lcd.ReceiveSolvedMeterState(voltage, current);
            lcd.ApplyElectricalState(new SparkElectricalState(
                voltage, current, watts,
                Mathf.Abs(current) > 0.000001f
                    ? SparkConductionState.Conducting
                    : SparkConductionState.NonConducting));
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component, SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            if (!(component is SparkLCDMeter lcd) || result == null) return false;
            if (!lcd.ElectricalEnabled) return true;
            if (!TryGetTerminals(lcd, result.Context.Graph, out SparkTerminal a, out SparkTerminal b))
                return false;
            if (!result.TryGetVoltage(a, b, out float voltage)) return false;
            current = voltage / lcd.MeterResistanceOhms;
            return true;
        }

        public override bool RequiresContinuousSolve(SparkElectricalComponent component) => false;

        public override void ResetRuntimeState(SparkElectricalComponent component)
        {
            if (component is SparkLCDMeter lcd) lcd.ClearSolvedMeterState();
        }
    }
}
