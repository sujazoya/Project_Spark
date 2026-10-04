using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkSwitchIndexSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Switch Index")]
    public sealed class SparkSwitchIndexSolverDefinition :
        SparkElectricalSolverDefinition
    {
        // ---------------------------------------------------------------------
        // COMPONENT
        // ---------------------------------------------------------------------

        public override Type SupportedComponentType =>
            typeof(SparkSwitchIndex);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkSwitchIndex;
        }

        // ---------------------------------------------------------------------
        // TOPOLOGY
        // ---------------------------------------------------------------------

        public override void RegisterTopology(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return;

            if (context == null)
                return;

            if (!indexedSwitch.ElectricalEnabled)
                return;

            context.RegisterIndexedSource(
                indexedSwitch);
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

            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            if (indexedSwitch.InputTerminal == null ||
                indexedSwitch.OutputTerminal == null)
            {
                return false;
            }

            terminalA =
                indexedSwitch.InputTerminal;

            terminalB =
                indexedSwitch.OutputTerminal;

            return true;
        }

        // ---------------------------------------------------------------------
        // STAMP
        // ---------------------------------------------------------------------

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return;

            if (context == null)
                return;

            if (!indexedSwitch.ElectricalEnabled)
                return;

            if (!TryGetTerminals(
                    indexedSwitch,
                    context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                return;
            }

            if (!context.TryGetIndexedSourceIndex(
                    indexedSwitch,
                    out int sourceIndex))
            {
                return;
            }

            float ratio =
                Mathf.Clamp01(
                    indexedSwitch.CurrentIndexVoltagePercent /
                    100f);

            context.AddControlledVoltageSource(
                inputTerminal,
                outputTerminal,
                sourceIndex,
                ratio);
        }

        // ---------------------------------------------------------------------
        // APPLY SOLVED STATE
        // ---------------------------------------------------------------------

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return;

            if (result == null)
                return;

            if (!indexedSwitch.ElectricalEnabled)
            {
                indexedSwitch.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    indexedSwitch,
                    result.Context.Graph,
                    out SparkTerminal inputTerminal,
                    out SparkTerminal outputTerminal))
            {
                indexedSwitch.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    inputTerminal,
                    outputTerminal);

            float current = 0f;

            result.TryGetIndexedSourceCurrent(
                indexedSwitch,
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

            indexedSwitch.ApplyElectricalState(
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

            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            if (result == null)
                return false;

            return result.TryGetIndexedSourceCurrent(
                indexedSwitch,
                out current);
        }

        // ---------------------------------------------------------------------
        // CONTINUOUS SOLVE
        // ---------------------------------------------------------------------

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkSwitchIndex indexedSwitch))
                return false;

            return indexedSwitch.ElectricalEnabled;
        }
    }
}