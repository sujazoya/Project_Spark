using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkLEDSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/LED")]
    public sealed class SparkLEDSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkLED);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkLED;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkLED led))
            {
                return false;
            }

            if (led.AnodeTerminal == null ||
                led.CathodeTerminal == null)
            {
                return false;
            }

            terminalA = led.AnodeTerminal;
            terminalB = led.CathodeTerminal;

            return true;
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkLED led))
            {
                return;
            }

            if (context == null ||
                !led.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    led,
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

            float ledVoltage =
                anodeVoltage -
                cathodeVoltage;

            bool wasConducting =
                context.GetNonlinearState(
                    led);

            float conductionThreshold =
                led.ForwardVoltage;

            if (wasConducting)
            {
                conductionThreshold -=
                    Mathf.Max(
                        0f,
                        led.ConductionHysteresis);
            }

            bool conducting =
                ledVoltage >=
                conductionThreshold;

            context.SetNonlinearState(
                led,
                conducting);

            if (!conducting)
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    led.OnResistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                anodeNode,
                cathodeNode,
                conductance);

            context.AddForwardDrop(
                anodeTerminal,
                cathodeTerminal,
                led.ForwardVoltage,
                conductance);
        }

        public override void ApplySolvedState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkLED led))
            {
                return;
            }

            if (result == null)
            {
                return;
            }

            if (!led.ElectricalEnabled)
            {
                led.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            if (!TryGetTerminals(
                    led,
                    result.Context.Graph,
                    out SparkTerminal anodeTerminal,
                    out SparkTerminal cathodeTerminal))
            {
                led.ApplyElectricalState(
                    new SparkElectricalState());

                return;
            }

            float voltage =
                result.GetVoltage(
                    anodeTerminal,
                    cathodeTerminal);

            bool conducting =
                result.GetNonlinearState(
                    led);

            float current = 0f;

            if (conducting)
            {
                float effectiveVoltage =
                    voltage -
                    led.ForwardVoltage;

                float resistance =
                    Mathf.Max(
                        minimumResistance,
                        led.OnResistance);

                current =
                    Mathf.Max(
                        0f,
                        effectiveVoltage / resistance);

                if (led.MaximumForwardCurrent > 0f)
                {
                    current =
                        Mathf.Min(
                            current,
                            led.MaximumForwardCurrent);
                }
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

            led.ApplyElectricalState(
                state);
        }

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkLED led))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!led.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    led,
                    result.Context.Graph,
                    out SparkTerminal anodeTerminal,
                    out SparkTerminal cathodeTerminal))
            {
                return false;
            }

            bool conducting =
                result.GetNonlinearState(
                    led);

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
                    led.OnResistance);

            float effectiveVoltage =
                voltage -
                led.ForwardVoltage;

            current =
                Mathf.Max(
                    0f,
                    effectiveVoltage / resistance);

            if (led.MaximumForwardCurrent > 0f)
            {
                current =
                    Mathf.Min(
                        current,
                        led.MaximumForwardCurrent);
            }

            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkLED led))
            {
                return false;
            }

            return led.ElectricalEnabled;
        }

        public override void ResetRuntimeState(
            SparkElectricalComponent component)
        {
            if (!(component is SparkLED))
            {
                return;
            }

            // The nonlinear state is owned by the solver runtime.
            // Nothing is stored in this ScriptableObject definition.
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