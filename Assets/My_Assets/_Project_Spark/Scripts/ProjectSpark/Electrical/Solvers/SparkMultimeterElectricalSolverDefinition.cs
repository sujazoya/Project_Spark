using System;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Electrical solver definition for the multimeter internal
    /// current shunt.
    ///
    /// The shunt is NOT permanently connected.
    ///
    /// CurrentMeasurementEnabled == false:
    ///
    ///     No electrical branch is added.
    ///
    /// CurrentMeasurementEnabled == true:
    ///
    ///     RED ── 0.05Ω ── BLACK
    ///
    /// Current:
    ///
    ///     I = (Vred - Vblack) / R
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkMultimeterElectricalSolverDefinition",
        menuName = "Project Spark/Electrical/Definitions/Multimeter")]
    public sealed class SparkMultimeterElectricalSolverDefinition
        : SparkElectricalSolverDefinition
    {
        // =====================================================================
        // COMPONENT TYPE
        // =====================================================================

        public override Type SupportedComponentType =>
            typeof(SparkMultimeterElectricalComponent);

        // =====================================================================
        // CAN HANDLE
        // =====================================================================

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkMultimeterElectricalComponent;
        }

        // =====================================================================
        // TOPOLOGY
        // =====================================================================

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component
                is SparkMultimeterElectricalComponent multimeter))
            {
                return;
            }

            if (context == null)
                return;

            /*
             * The shunt is a resistor.
             *
             * It does not require an MNA voltage-source variable.
             *
             * IMPORTANT:
             *
             * We deliberately do NOT register anything here.
             *
             * The branch is added in Stamp() only when the
             * ammeter is actively enabled.
             */
        }

        // =====================================================================
        // STAMP
        // =====================================================================

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component
                is SparkMultimeterElectricalComponent multimeter))
            {
                return;
            }

            if (context == null)
                return;

            /*
             * IMPORTANT:
             *
             * Voltage / resistance / continuity mode must NOT
             * introduce the 0.05Ω shunt.
             */
            if (!multimeter.CurrentMeasurementEnabled)
                return;

            if (!multimeter.TryGetTerminals(
                    out SparkTerminal redTerminal,
                    out SparkTerminal blackTerminal))
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    context.MinimumResistance,
                    multimeter.ShuntResistanceOhms);

            double conductance =
                1.0 / resistance;

            /*
             * RED ── R ── BLACK
             */
            context.AddConductance(
                redTerminal,
                blackTerminal,
                conductance);
        }

        // =====================================================================
        // TERMINALS
        // =====================================================================

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component
                is SparkMultimeterElectricalComponent multimeter))
            {
                return false;
            }

            if (!multimeter.TryGetTerminals(
                    out SparkTerminal red,
                    out SparkTerminal black))
            {
                return false;
            }

            terminalA = red;
            terminalB = black;

            return true;
        }

        // =====================================================================
        // SOLVED STATE
        // =====================================================================

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            /*
             * The multimeter does not need a persistent electrical
             * component state.
             *
             * Current is calculated directly from the completed
             * solve result.
             */
        }

        // =====================================================================
        // CURRENT
        // =====================================================================

        /// <summary>
        /// Calculates current through the internal shunt.
        ///
        /// Positive direction:
        ///
        ///     RED -> BLACK
        ///
        /// Formula:
        ///
        ///     I = (Vred - Vblack) / R
        /// </summary>
        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component
                is SparkMultimeterElectricalComponent multimeter))
            {
                return false;
            }

            if (result == null)
                return false;

            /*
             * If the shunt is disabled, there is no valid
             * ammeter current.
             */
            if (!multimeter.CurrentMeasurementEnabled)
                return false;

            if (!multimeter.TryGetTerminals(
                    out SparkTerminal redTerminal,
                    out SparkTerminal blackTerminal))
            {
                return false;
            }

            if (!result.TryGetTerminalVoltage(
                    redTerminal,
                    out float redVoltage))
            {
                return false;
            }

            if (!result.TryGetTerminalVoltage(
                    blackTerminal,
                    out float blackVoltage))
            {
                return false;
            }

            float shuntVoltage =
                redVoltage - blackVoltage;

            float resistance =
                Mathf.Max(
                    result.Context.MinimumResistance,
                    multimeter.ShuntResistanceOhms);

            current =
                shuntVoltage / resistance;

            return true;
        }
    }
}