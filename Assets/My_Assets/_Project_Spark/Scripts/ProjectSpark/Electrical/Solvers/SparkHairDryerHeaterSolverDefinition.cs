using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkHairDryerHeaterSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Hair Dryer Heater")]
    public sealed class SparkHairDryerHeaterSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000001f)]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkHairDryerHeaterElectrical);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkHairDryerHeaterElectrical;
        }

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            if (heater.LiveTerminal == null ||
                heater.NeutralTerminal == null)
            {
                return false;
            }

            terminalA = heater.LiveTerminal;
            terminalB = heater.NeutralTerminal;

            return true;
        }

        public override void Stamp(
            SparkElectricalComponent component,
            SparkElectricalSolveContext context)
        {
            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return;
            }

            if (context == null)
            {
                return;
            }

            if (!heater.ElectricalEnabled)
            {
                return;
            }

            if (!TryGetTerminals(
                    heater,
                    context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    liveTerminal,
                    out int liveNode))
            {
                return;
            }

            if (!context.Graph.TryGetNode(
                    neutralTerminal,
                    out int neutralNode))
            {
                return;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    heater.Resistance);

            double conductance =
                1.0 / resistance;

            context.AddConductance(
                liveNode,
                neutralNode,
                conductance);
        }

       public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkHairDryerHeaterElectrical heater))
    {
        return;
    }

    if (result == null)
    {
        return;
    }

    /*
     * ============================================================
     * DISABLED
     * ============================================================
     *
     * The heater is electrically disabled.
     * Clear any previous voltage/current/power state.
     */
    if (!heater.ElectricalEnabled)
    {
        heater.ApplyElectricalState(
            new SparkElectricalState());

        Debug.Log(
            $"[HEATER ELECTRICAL] " +
            $"Name={heater.name} | " +
            $"DISABLED | " +
            $"LiveV=0.000 | " +
            $"NeutralV=0.000 | " +
            $"Voltage=0.000 | " +
            $"Current=0.000000 | " +
            $"Power=0.000 | " +
            $"Heating=False"
        );

        return;
    }

    /*
     * ============================================================
     * TERMINALS
     * ============================================================
     */
    if (!TryGetTerminals(
            heater,
            result.Context.Graph,
            out SparkTerminal liveTerminal,
            out SparkTerminal neutralTerminal))
    {
        heater.ApplyElectricalState(
            new SparkElectricalState());

        Debug.LogWarning(
            $"[HEATER ELECTRICAL] " +
            $"Name={heater.name} | " +
            $"FAILED: Live/Neutral terminals could not be resolved."
        );

        return;
    }

    /*
     * ============================================================
     * TERMINAL VOLTAGES
     * ============================================================
     *
     * The heater voltage is the actual differential between
     * its live and neutral terminals.
     *
     *     Vheater = Vlive - Vneutral
     */
    float liveVoltage = 0f;
    float neutralVoltage = 0f;

    result.TryGetTerminalVoltage(
        liveTerminal,
        out liveVoltage);

    result.TryGetTerminalVoltage(
        neutralTerminal,
        out neutralVoltage);

    float voltage =
        liveVoltage - neutralVoltage;

    /*
     * ============================================================
     * RESISTANCE
     * ============================================================
     */
    float resistance =
        Mathf.Max(
            minimumResistance,
            heater.Resistance);

    /*
     * ============================================================
     * CURRENT
     * ============================================================
     *
     * Resistive heater:
     *
     *     I = V / R
     */
    float current =
        voltage / resistance;

    /*
     * ============================================================
     * POWER
     * ============================================================
     *
     * Resistive load:
     *
     *     P = V × I
     *
     * Equivalent:
     *
     *     P = V² / R
     */
    float power =
        voltage * current;

    /*
     * ============================================================
     * CONDUCTION
     * ============================================================
     */
    SparkConductionState conduction =
        Mathf.Abs(current) > 0.000001f
            ? SparkConductionState.Conducting
            : SparkConductionState.NonConducting;

    /*
     * ============================================================
     * APPLY ELECTRICAL STATE
     * ============================================================
     */
    SparkElectricalState state =
        new SparkElectricalState(
            voltage,
            current,
            power,
            conduction);

    heater.ApplyElectricalState(
        state);

    /*
     * ============================================================
     * DEBUG
     * ============================================================
     *
     * This lets us verify the actual heater terminals.
     *
     * Example with 230 V:
     *
     *     LiveV    = +137.9 V
     *     NeutralV = -92.1 V
     *
     *     HeaterV  = 230 V
     *
     * With 29.5 ohms:
     *
     *     Current ≈ 7.80 A
     *     Power   ≈ 1794 W
     *
     * If LiveV and NeutralV are nearly equal:
     *
     *     HeaterV ≈ 0 V
     *     Current ≈ 0 A
     *     Power   ≈ 0 W
     */
    Debug.Log(
        $"[HEATER ELECTRICAL] " +
        $"Name={heater.name} | " +
        $"LiveV={liveVoltage:F3} | " +
        $"NeutralV={neutralVoltage:F3} | " +
        $"Voltage={voltage:F3} | " +
        $"Resistance={resistance:F3} | " +
        $"Current={current:F6} | " +
        $"Power={power:F3} | " +
        $"Heating={heater.IsHeating}"
    );
}

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!heater.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    heater,
                    result.Context.Graph,
                    out SparkTerminal liveTerminal,
                    out SparkTerminal neutralTerminal))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    heater.Resistance);

            float voltage =
                result.GetVoltage(
                    liveTerminal,
                    neutralTerminal);

            current =
                voltage / resistance;

            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            if (!(component is SparkHairDryerHeaterElectrical heater))
            {
                return false;
            }

            return heater.ElectricalEnabled;
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