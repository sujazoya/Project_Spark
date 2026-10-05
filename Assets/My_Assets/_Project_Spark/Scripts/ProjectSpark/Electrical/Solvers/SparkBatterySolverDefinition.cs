using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Electrical solver definition for SparkBattery.
    ///
    /// Battery model:
    ///
    ///     + ──[ Internal Resistance ]── Ideal Voltage Source ── -
    ///
    /// The solver context represents this Thevenin source using
    /// the equivalent Norton form:
    ///
    ///     Conductance = 1 / InternalResistance
    ///     SourceCurrent = Conductance * NominalVoltage
    ///
    /// This allows the battery to participate directly in the
    /// existing MNA network without creating an additional
    /// internal battery node.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkBatterySolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Battery")]
    public sealed class SparkBatterySolverDefinition :
        SparkElectricalSolverDefinition
    {
        [Header("Battery Solver")]
        [SerializeField]
        private bool requireOutputEnabled = true;

        [SerializeField]
        private bool requireElectricalEnabled = true;

        public override Type SupportedComponentType =>
            typeof(SparkBattery);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkBattery;
        }

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkBattery battery))
                return;

            if (!IsActive(battery))
                return;

            if (battery.PositiveTerminal == null ||
                battery.NegativeTerminal == null)
            {
                return;
            }

            /*
             * The battery is represented as a Norton-equivalent
             * conductance/current source, so it does not require
             * a separate MNA voltage-source index.
             */
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkBattery battery))
                return;

            if (!IsActive(battery))
                return;

            SparkTerminal positive =
                battery.PositiveTerminal;

            SparkTerminal negative =
                battery.NegativeTerminal;

            if (positive == null ||
                negative == null)
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    context.MinimumResistance,
                    battery.InternalResistance);

            float voltage =
                Mathf.Max(
                    0f,
                    battery.NominalVoltage);

            double conductance =
                1.0 / resistance;

            /*
             * The 4-argument overload creates the Norton equivalent:
             *
             *     G = 1 / R
             *     I = G * V
             *
             * which is electrically equivalent to:
             *
             *     ideal V source + series R
             */
            context.AddConductance(
                positive,
                negative,
                conductance,
                voltage);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkBattery battery))
                return;

            if (result == null)
                return;

            if (battery.PositiveTerminal == null ||
                battery.NegativeTerminal == null)
            {
                return;
            }

            /*
             * Battery current is the current delivered from the
             * positive terminal toward the external circuit.
             *
             * I = (Vbattery - Vterminal) / R
             *
             * where Vterminal is the solved voltage difference
             * between the battery's positive and negative terminals.
             */
            float terminalVoltage =
                result.GetVoltage(
                    battery.PositiveTerminal,
                    battery.NegativeTerminal);

            float resistance =
                Mathf.Max(
                    result.Context.MinimumResistance,
                    battery.InternalResistance);

            float current = 0f;

            if (IsActive(battery))
            {
                current =
                    (battery.NominalVoltage -
                     terminalVoltage) /
                    resistance;
            }

            /*
             * The base electrical component owns the actual
             * ElectricalState storage in Project Spark.
             *
             * The solver's normal state-application pipeline can
             * use the solver-definition current calculation.
             *
             * No direct mutation is required here.
             */
        }

        public override bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            voltage = 0f;

            if (!(component is SparkBattery battery))
                return false;

            if (!IsActive(battery))
                return false;

            voltage =
                Mathf.Max(
                    0f,
                    battery.NominalVoltage);

            return true;
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkBattery battery))
                return false;

            if (result == null)
                return false;

            if (!IsActive(battery))
                return true;

            if (battery.PositiveTerminal == null ||
                battery.NegativeTerminal == null)
            {
                return false;
            }

            float terminalVoltage =
                result.GetVoltage(
                    battery.PositiveTerminal,
                    battery.NegativeTerminal);

            float resistance =
                Mathf.Max(
                    result.Context.MinimumResistance,
                    battery.InternalResistance);

            current =
                (battery.NominalVoltage -
                 terminalVoltage) /
                resistance;

            return IsFinite(current);
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkBattery battery))
                return false;

            terminalA = battery.PositiveTerminal;
            terminalB = battery.NegativeTerminal;

            return
                terminalA != null &&
                terminalB != null;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            return false;
        }

        public override void ResetRuntimeState(
            SparkElectricalComponent component)
        {
            /*
             * Battery has no nonlinear or transient internal
             * solver state to reset.
             */
        }

        private bool IsActive(
            SparkBattery battery)
        {
            if (battery == null)
                return false;

            if (requireElectricalEnabled &&
                !battery.ElectricalEnabled)
            {
                return false;
            }

            if (requireOutputEnabled &&
                !battery.OutputEnabled)
            {
                return false;
            }

            return
                battery.PositiveTerminal != null &&
                battery.NegativeTerminal != null;
        }

        private static bool IsFinite(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}