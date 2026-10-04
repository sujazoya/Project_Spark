using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkDiodeSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Diode")]
    public sealed class SparkDiodeSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkDiode);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkDiode;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkDiode diode))
            {
                return false;
            }

            if (diode.AnodeTerminal == null ||
                diode.CathodeTerminal == null)
            {
                return false;
            }

            terminalA = diode.AnodeTerminal;
            terminalB = diode.CathodeTerminal;

            return true;
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkDiode diode))
            {
                return;
            }

            if (context == null ||
                !diode.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    diode,
                    context.Graph,
                    out SparkTerminal anodeTerminal,
                    out SparkTerminal cathodeTerminal))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    anodeTerminal,
                    out int anodeNode))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    cathodeTerminal,
                    out int cathodeNode))
            {
                return;
            }

            float anodeVoltage =
                context.GetIterationNodeVoltage(
                    anodeNode);

            float cathodeVoltage =
                context.GetIterationNodeVoltage(
                    cathodeNode);

            float diodeVoltage =
                anodeVoltage -
                cathodeVoltage;

            bool conducting =
                diodeVoltage >=
                diode.ForwardVoltage;

            context.SetNonlinearState(
                diode,
                conducting);

            if (!conducting)
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    diode.OnResistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                anodeNode,
                cathodeNode,
                conductance);

            context.AddForwardDrop(
                anodeTerminal,
                cathodeTerminal,
                diode.ForwardVoltage,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkDiode diode))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!diode.ElectricalEnabled)
            {
                diode.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    diode,
                    result.Context.Graph,
                    out SparkTerminal anodeTerminal,
                    out SparkTerminal cathodeTerminal))
            {
                diode.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    anodeTerminal,
                    cathodeTerminal);

            bool conducting =
                result.GetNonlinearState(
                    diode);

            float current = 0f;

            if (conducting)
            {
                float effectiveVoltage =
                    voltage -
                    diode.ForwardVoltage;

                float resistance =
                    Mathf.Max(
                        minimumResistance,
                        diode.OnResistance);

                current =
                    Mathf.Max(
                        0f,
                        effectiveVoltage / resistance);
            }

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

            diode.ApplyElectricalState(
                state);
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkDiode diode))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!diode.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    diode,
                    result.Context.Graph,
                    out SparkTerminal anodeTerminal,
                    out SparkTerminal cathodeTerminal))
            {
                return false;
            }

            bool conducting =
                result.GetNonlinearState(
                    diode);

            if (!conducting)
            {
                current = 0f;
                return true;
            }

            float voltage =
                result.GetVoltage(
                    anodeTerminal,
                    cathodeTerminal);

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    diode.OnResistance);

            float effectiveVoltage =
                voltage -
                diode.ForwardVoltage;

            current =
                Mathf.Max(
                    0f,
                    effectiveVoltage / resistance);

            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkDiode diode))
            {
                return false;
            }

            return diode.ElectricalEnabled;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            minimumResistance =
                Mathf.Max(
                    0.000001f,
                    minimumResistance);
        }
    }
}