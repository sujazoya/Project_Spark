using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkPowerSupplySolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Power Supply")]
    public sealed class SparkPowerSupplySolverDefinition :
        SparkElectricalSolverDefinition
    {
        public override Type SupportedComponentType =>
            typeof(SparkPowerSupply);

        // ---------------------------------------------------------------------
        // COMPONENT
        // ---------------------------------------------------------------------

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkPowerSupply;
        }

        // ---------------------------------------------------------------------
        // TERMINALS
        // ---------------------------------------------------------------------

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkPowerSupply supply))
                return false;

            if (supply.PositiveTerminal == null ||
                supply.NegativeTerminal == null)
            {
                return false;
            }

            terminalA =
                supply.PositiveTerminal;

            terminalB =
                supply.NegativeTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // TOPOLOGY
        // ---------------------------------------------------------------------

        /// <summary>
        /// Registers this power supply as a generic MNA voltage source.
        ///
        /// Registration must happen before the solver initializes
        /// its matrix because the voltage source adds an auxiliary
        /// MNA equation/current variable.
        /// </summary>
        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {

            
            if (!(component is SparkPowerSupply supply))
                return;

            if (context == null)
                return;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return;
            }

            context.RegisterVoltageSource(
                supply);
        }

        // ---------------------------------------------------------------------
        // SOURCE INFORMATION
        // ---------------------------------------------------------------------

        public override bool TryGetSourceVoltage(
            SparkElectricalComponent component,
            out float voltage)
        {
            voltage = 0f;

            if (!(component is SparkPowerSupply supply))
                return false;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return false;
            }

            voltage =
                supply.OutputVoltage;

            return true;
        }

        public override bool TryGetReferenceTerminal(
            SparkElectricalComponent component,
            out SparkTerminal terminal)
        {
            terminal = null;

            if (!(component is SparkPowerSupply supply))
                return false;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return false;
            }

            if (supply.NegativeTerminal == null)
                return false;

            terminal =
                supply.NegativeTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // STAMP
        // ---------------------------------------------------------------------

        /// <summary>
        /// Stamps the power supply as:
        ///
        ///     V(+) - V(-) = OutputVoltage
        ///
        /// using a generic MNA voltage-source equation.
        /// </summary>
        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkPowerSupply supply))
                return;

            if (context == null)
                return;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                return;
            }

            if (!context.TryGetVoltageSourceIndex(
                    supply,
                    out int sourceIndex))
            {
                return;
            }

            if (!TryGetTerminals(
                    supply,
                    context.Graph,
                    out SparkTerminal positiveTerminal,
                    out SparkTerminal negativeTerminal))
            {
                return;
            }

            context.AddFixedVoltageSource(
                positiveTerminal,
                negativeTerminal,
                sourceIndex,
                supply.OutputVoltage);
        }

        // ---------------------------------------------------------------------
        // APPLY SOLVED STATE
        // ---------------------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkPowerSupply supply))
                return;

            if (result == null)
                return;

            if (!supply.ElectricalEnabled ||
                !supply.IsOutputActive)
            {
                supply.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    supply,
                    result.Context.Graph,
                    out SparkTerminal positiveTerminal,
                    out SparkTerminal negativeTerminal))
            {
                supply.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    positiveTerminal,
                    negativeTerminal);

            float current = 0f;

            result.TryGetVoltageSourceCurrent(
                supply,
                out current);

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

            supply.ApplyElectricalState(
                state);
        }

        // ---------------------------------------------------------------------
        // CURRENT
        // ---------------------------------------------------------------------

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkPowerSupply))
                return false;

            if (result == null)
                return false;

            if (!result.TryGetVoltageSourceCurrent(
                    component,
                    out current))
            {
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // CONTINUOUS SOLVE
        // ---------------------------------------------------------------------

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkPowerSupply supply))
                return false;

            return supply.ElectricalEnabled &&
                   supply.IsOutputActive;
        }
    }
}