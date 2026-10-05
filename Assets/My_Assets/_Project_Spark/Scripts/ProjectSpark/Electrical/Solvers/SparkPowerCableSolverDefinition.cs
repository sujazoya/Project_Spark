using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for SparkPowerCable.
    ///
    /// The cable is an ideal passive bidirectional conductor.
    ///
    ///     A ───────────────── B
    ///
    /// It generates no voltage and has no intrinsic polarity.
    ///
    /// The cable is represented in the electrical solver as a very
    /// low resistance connection using the solver's configured
    /// minimum resistance.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkPowerCableSolverDefinition",
        menuName =
            "Project Spark/Electrical/Solver Definitions/Power Cable")]
    public sealed class SparkPowerCableSolverDefinition :
        SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType =>
            typeof(SparkPowerCable);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkPowerCable;
        }

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkPowerCable cable))
                return;

            if (!cable.ElectricalEnabled)
                return;

            if (!cable.HasValidTerminals)
                return;

            /*
             * The cable does not require an MNA voltage source.
             *
             * Its two terminals are connected by a very small
             * resistance during matrix stamping.
             */
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkPowerCable cable))
                return;

            if (!cable.ElectricalEnabled)
                return;

            if (!cable.HasValidTerminals)
                return;

            /*
             * Ideal cable approximation:
             *
             *     R = solver minimum resistance
             *
             *     G = 1 / R
             *
             * This creates a very strong electrical connection
             * between A and B while keeping the MNA matrix finite.
             */
            double conductance =
                1.0 /
                Mathf.Max(
                    context.MinimumResistance,
                    0.000001f);

            context.AddConductance(
                cable.TerminalA,
                cable.TerminalB,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            /*
             * No persistent runtime state is required.
             *
             * Voltage/current can be calculated from the solved
             * terminal voltages when requested.
             */
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkPowerCable cable))
                return false;

            if (!cable.HasValidTerminals)
                return false;

            terminalA = cable.TerminalA;
            terminalB = cable.TerminalB;

            return true;
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkPowerCable cable))
                return false;

            if (result == null)
                return false;

            if (!cable.ElectricalEnabled)
                return true;

            if (!cable.HasValidTerminals)
                return false;

            float resistance =
                Mathf.Max(
                    result.Context.MinimumResistance,
                    0.000001f);

            float voltage =
                result.GetVoltage(
                    cable.TerminalA,
                    cable.TerminalB);

            current =
                voltage / resistance;

            return IsFinite(current);
        }

        public override bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            /*
             * A cable is passive.
             */
            voltage = 0f;
            return false;
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
             * No runtime solver state.
             */
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