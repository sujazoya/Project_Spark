using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for SparkResistor.
    ///
    /// Owns resistor-specific electrical behavior:
    /// - MNA conductance stamping
    /// - solved voltage
    /// - solved current
    /// - solved power
    /// - conduction state
    ///
    /// SparkElectricalSolver remains device-agnostic.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkResistorSolver",
        menuName = "Project Spark/Electrical/Solvers/Resistor")]
    public sealed class SparkResistorSolverDefinition
        : SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType =>
            typeof(SparkResistor);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkResistor;
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (component is not SparkResistor resistor)
                return;

            if (!resistor.ElectricalEnabled)
                return;

            if (resistor.TerminalA == null ||
                resistor.TerminalB == null)
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    context.MinimumResistance,
                    resistor.ResistanceOhms);

            float conductance =
                1f / resistance;

            context.AddConductance(
                resistor.TerminalA,
                resistor.TerminalB,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (component is not SparkResistor resistor)
                return;

            if (resistor.TerminalA == null ||
                resistor.TerminalB == null)
            {
                return;
            }

            float voltage =
                result.GetVoltage(
                    resistor.TerminalA,
                    resistor.TerminalB);

            float resistance =
                Mathf.Max(
                    0.000001f,
                    resistor.ResistanceOhms);

            float current =
                voltage / resistance;

            float power =
                voltage * current;

            SparkConductionState conduction =
                Mathf.Abs(current) > 0.000001f
                    ? SparkConductionState.Conducting
                    : SparkConductionState.NonConducting;

            resistor.ApplyElectricalState(
                new SparkElectricalState(
                    voltage,
                    current,
                    power,
                    conduction));
        }
    }
}