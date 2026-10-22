using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Solver definition for a two-terminal resistive component.
    ///
    /// Responsibilities:
    /// - Identify SparkResistor components.
    /// - Expose their two electrical terminals.
    /// - Stamp their conductance into the solver matrix.
    /// - Apply solved voltage/current/power state.
    /// - Calculate resistor current for the runtime electrical state.
    ///
    /// Runtime state is NOT stored in this ScriptableObject.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkResistorSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Resistor")]
    public sealed class SparkResistorSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField]
        private float minimumResistance = 0.000001f;

        public override Type SupportedComponentType =>
            typeof(SparkResistor);

        // ---------------------------------------------------------------------
        // Component matching
        // ---------------------------------------------------------------------

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkResistor;
        }

        // ---------------------------------------------------------------------
        // Terminal access
        // ---------------------------------------------------------------------

        public override bool TryGetTerminals(
            SparkElectricalComponent component,
            SparkElectricalNetworkGraph graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB)
        {
            terminalA = null;
            terminalB = null;

            if (!(component is SparkResistor resistor))
            {
                return false;
            }

            if (resistor.TerminalA == null ||
                resistor.TerminalB == null)
            {
                return false;
            }

            terminalA = resistor.TerminalA;
            terminalB = resistor.TerminalB;

            return true;
        }

        // ---------------------------------------------------------------------
        // Matrix stamping
        // ---------------------------------------------------------------------

       
public override void Stamp(
    SparkElectricalComponent component,
    SparkElectricalSolveContext context)
{
    if (!(component is SparkResistor resistor))
        return;

    if (context == null || !resistor.ElectricalEnabled)
        return;

    if (!TryGetTerminals(
            resistor,
            context.Graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB))
    {
        Debug.LogWarning(
            $"[RESISTOR STAMP] Missing terminals: {resistor.name}",
            resistor);
        return;
    }

    if (!context.Graph.TryGetNode(terminalA, out int nodeA) ||
        !context.Graph.TryGetNode(terminalB, out int nodeB))
    {
        Debug.LogWarning(
            $"[RESISTOR STAMP] Terminals not found in graph: " +
            $"{resistor.name}",
            resistor);
        return;
    }

    float resistance = Mathf.Max(
        minimumResistance,
        resistor.EffectiveResistance);

    double conductance = 1.0 / resistance;

    // IMPORTANT:
    // Use terminals, not graph node IDs.
    // The context converts terminals to reduced MNA indices.
    context.AddConductance(
        terminalA,
        terminalB,
        conductance);

    Debug.Log(
        $"[RESISTOR STAMP] {resistor.name} | " +
        $"A={terminalA.name} GraphNode={nodeA} " +
        $"MatrixIndex={context.MapTerminal(terminalA)} | " +
        $"B={terminalB.name} GraphNode={nodeB} " +
        $"MatrixIndex={context.MapTerminal(terminalB)} | " +
        $"Resistance={resistance:F3} Ohm",
        resistor);
}


        // ---------------------------------------------------------------------
        // Solved electrical state
        // ---------------------------------------------------------------------

       
public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkResistor resistor))
        return;

    if (result == null)
        return;

    if (!resistor.ElectricalEnabled)
    {
        resistor.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    if (!TryGetTerminals(
            resistor,
            result.Context.Graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB))
    {
        resistor.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    if (!result.TryGetVoltage(
            terminalA,
            terminalB,
            out float voltage) ||
        float.IsNaN(voltage) ||
        float.IsInfinity(voltage))
    {
        Debug.LogWarning(
            $"[RESISTOR STATE] Unable to read solved voltage " +
            $"for {resistor.name}.",
            resistor);

        resistor.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    float resistance = Mathf.Max(
        minimumResistance,
        resistor.EffectiveResistance);

    float current = voltage / resistance;
    float power = voltage * current;

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

    Debug.Log(
        $"[RESISTOR SOLVED] {resistor.name} | " +
        $"A={terminalA.name} | B={terminalB.name} | " +
        $"Resistance={resistance:F3} Ohm | " +
        $"Voltage={voltage:F6} V | " +
        $"Current={current:F6} A | " +
        $"Power={power:F6} W",
        resistor);
}


        // ---------------------------------------------------------------------
        // Current calculation
        // ---------------------------------------------------------------------

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;

            if (!(component is SparkResistor resistor))
            {
                return false;
            }

            if (result == null)
            {
                return false;
            }

            if (!resistor.ElectricalEnabled)
            {
                return true;
            }

            if (!TryGetTerminals(
                    resistor,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
            {
                return false;
            }

            float resistance =
                Mathf.Max(
                    minimumResistance,
                    resistor.EffectiveResistance);
if (!result.TryGetVoltage(
        terminalA,
        terminalB,
        out float voltage))
{
    return false;
}

            current =
                voltage / resistance;

            return true;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------

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