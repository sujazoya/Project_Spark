using System;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for the Project Spark multimeter.
    ///
    /// The multimeter current mode is represented electrically as
    /// a small shunt resistance between the RED and BLACK terminals.
    ///
    /// Default:
    ///
    ///     Rshunt = 0.05 ohm
    ///
    /// Therefore:
    ///
    ///     I = Vred-black / Rshunt
    ///
    /// The solver stamps the shunt directly into the circuit.
    /// The measurement system can then obtain the actual solved
    /// current through this internal shunt.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkMultimeterSolverDefinition",
        menuName =
            "Project Spark/Electrical/Solver Definitions/Multimeter")]
    public sealed class SparkMultimeterSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [Header("Multimeter Solver")]

        [Tooltip(
            "If disabled, the multimeter will not electrically "
            + "load the circuit.")]
        [SerializeField]
        private bool enableShunt = true;

        public override Type SupportedComponentType =>
            typeof(SparkMultimeterElectricalComponent);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component
                is SparkMultimeterElectricalComponent;
        }

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component
                is SparkMultimeterElectricalComponent meter))
            {
                return;
            }

            if (!IsUsable(meter))
                return;

            /*
             * The multimeter shunt is a passive conductance.
             *
             * No MNA voltage-source index is required.
             */
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component
                is SparkMultimeterElectricalComponent meter))
            {
                return;
            }

            if (!enableShunt)
                return;

            if (!meter.ElectricalEnabled)
                return;

            if (!meter.TryGetTerminals(
                    out SparkTerminal red,
                    out SparkTerminal black))
            {
                return;
            }

            if (red == null || black == null)
                return;

            float resistance =
                Mathf.Max(
                    context.MinimumResistance,
                    meter.ShuntResistanceOhms);

            double conductance =
                1.0 / resistance;

            /*
             * Internal current shunt:
             *
             *       RED
             *        |
             *       [R]
             *        |
             *      BLACK
             *
             * I = Vred-black / R
             */
            context.AddConductance(
                red,
                black,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            /*
             * The multimeter does not need to modify the circuit
             * during ApplySolvedState.
             *
             * The solved shunt voltage/current are read from the
             * completed SparkElectricalSolveResult.
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

            if (!(component
                is SparkMultimeterElectricalComponent meter))
            {
                return false;
            }

            return meter.TryGetTerminals(
                out terminalA,
                out terminalB);
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component
                is SparkMultimeterElectricalComponent meter))
            {
                return false;
            }

            if (result == null)
                return false;

            if (!meter.ElectricalEnabled)
                return true;

            if (!meter.TryGetTerminals(
                    out SparkTerminal red,
                    out SparkTerminal black))
            {
                return false;
            }

            if (red == null || black == null)
                return false;

            float resistance =
                Mathf.Max(
                    result.Context.MinimumResistance,
                    meter.ShuntResistanceOhms);

            float voltage =
                result.GetVoltage(
                    red,
                    black);

            current =
                voltage / resistance;

            return IsFinite(current);
        }

        public override bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            /*
             * The multimeter is passive.
             * It does not generate voltage.
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
             * No persistent solver state.
             */
        }

        private static bool IsUsable(
            SparkMultimeterElectricalComponent meter)
        {
            if (meter == null)
                return false;

            if (!meter.ElectricalEnabled)
                return false;

            return
                meter.RedTerminal != null &&
                meter.BlackTerminal != null;
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