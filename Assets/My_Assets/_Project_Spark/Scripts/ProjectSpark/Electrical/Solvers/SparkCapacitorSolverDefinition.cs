using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [CreateAssetMenu(
        fileName = "SparkCapacitorSolverDefinition",
        menuName = "Project Spark/Electrical/Solver Definitions/Capacitor")]
    public sealed class SparkCapacitorSolverDefinition :
        SparkElectricalSolverDefinition
    {
        [SerializeField, Min(0.000000001f)]
        private float minimumCapacitance = 0.000000001f;       

        public override Type SupportedComponentType =>
            typeof(SparkCapacitor);

        public override bool CanHandle(
            SparkElectricalComponent component)
        {
            return component is SparkCapacitor;
        }

      
public override bool TryGetTerminals(
    SparkElectricalComponent component,
    SparkElectricalNetworkGraph graph,
    out SparkTerminal terminalA,
    out SparkTerminal terminalB)
{
    terminalA = null;
    terminalB = null;

    if (!(component is SparkCapacitor capacitor) ||
        graph == null)
        return false;

    // Prefer explicitly assigned capacitor terminals,
    // but only when both belong to the active graph.
    if (capacitor.HasValidTerminals)
    {
        SparkTerminal positive = capacitor.PositiveTerminal;
        SparkTerminal negative = capacitor.NegativeTerminal;

        if (graph.TryGetNode(positive, out _) &&
            graph.TryGetNode(negative, out _))
        {
            terminalA = positive;
            terminalB = negative;
            return true;
        }

        // Explicit terminals are configured but not
        // registered in this active circuit graph.
        // Refuse to silently stamp the wrong pair.
        return false;
    }

    // Backward compatibility for existing capacitors.
    return graph.TryGetPrimaryComponentTerminals(
        component,
        out terminalA,
        out terminalB);
}


      
public override void Stamp(
    SparkElectricalComponent component,
    SparkElectricalSolveContext context)
{
    if (!(component is SparkCapacitor capacitor) ||
        context == null ||
        !context.SimulateTransientDevices ||
        context.Graph == null)
        return;

    if (!TryGetTerminals(
            capacitor,
            context.Graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB))
        return;

    if (terminalA == null ||
        terminalB == null ||
        terminalA == terminalB)
        return;

    // Use the terminal overload to ensure correct
    // graph-node to reduced-matrix mapping.
    int nodeA = context.MapTerminal(terminalA);
    int nodeB = context.MapTerminal(terminalB);

    if (nodeA == nodeB)
        return;

    double capacitance = System.Math.Max(
        minimumCapacitance,
        capacitor.CapacitanceFarads);

    double dt = System.Math.Max(
        0.000001d,
        (double)context.TimeStep);

    double conductance = capacitance / dt;

    float previousVoltage = 0f;
    context.TryGetPreviousComponentVoltage(
        capacitor,
        out previousVoltage);

    // Refuse to stamp corrupted transient history.
    if (float.IsNaN(previousVoltage) ||
        float.IsInfinity(previousVoltage))
    {
        Debug.LogError(
            "[CAPACITOR] Invalid previous voltage: " +
            previousVoltage,
            capacitor);
        return;
    }

    // Backward Euler capacitor companion model:
    //
    // G = C / dt
    // Ieq = G * Vprevious
    //
    // These terminal overloads map the real graph nodes
    // to the correct reduced MNA matrix indices.

    context.AddConductance(
        terminalA,
        terminalB,
        conductance);

    context.AddCurrentSource(
        terminalA,
        terminalB,
        conductance * previousVoltage);
}


       
public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkCapacitor capacitor) || result == null)
        return;

    if (!TryGetTerminals(
            capacitor,
            result.Context.Graph,
            out SparkTerminal terminalA,
            out SparkTerminal terminalB))
        return;

    if (!result.TryGetVoltage(
            terminalA, terminalB, out float voltage))
        return;

    double previousVoltage = 0d;
    double current = 0d;
    double dt = result.Context.TimeStep;
    double capacitance = Math.Max(
        minimumCapacitance,
        capacitor.CapacitanceFarads);

    if (result.Context.SimulateTransientDevices)
    {
        float previous = 0f;
        result.TryGetPreviousComponentVoltage(
            capacitor, out previous);

        previousVoltage = previous;
        dt = Math.Max(0.000001d, dt);

        current = capacitance *
            ((double)voltage - previousVoltage) / dt;
    }

    double power = (double)voltage * current;

    if (float.IsNaN(voltage) ||
        float.IsInfinity(voltage) ||
        double.IsNaN(current) ||
        double.IsInfinity(current) ||
        double.IsNaN(power) ||
        double.IsInfinity(power) ||
        Math.Abs(current) > float.MaxValue ||
        Math.Abs(power) > float.MaxValue)
    {
        Debug.LogError(
            "[CAPACITOR SOLVER INVALID]\n" +
            "Object: " + capacitor.name + "\n" +
            "Voltage: " + voltage + "\n" +
            "Previous Voltage: " + previousVoltage + "\n" +
            "Current: " + current + "\n" +
            "Power: " + power + "\n" +
            "Capacitance: " + capacitance + "\n" +
            "Time Step: " + dt + "\n" +
            "Transient Enabled: " +
                result.Context.SimulateTransientDevices + "\n" +
            "Terminal A: " + terminalA.name + "\n" +
            "Terminal B: " + terminalB.name,
            capacitor);

        // Do not publish an invalid electrical state.
        return;
    }

    float solvedCurrent = (float)current;
    float solvedPower = (float)power;

    SparkConductionState conduction =
        Mathf.Abs(solvedCurrent) > 0.000001f
            ? SparkConductionState.Conducting
            : SparkConductionState.NonConducting;

    capacitor.ApplyElectricalState(
        new SparkElectricalState(
            voltage,
            solvedCurrent,
            solvedPower,
            conduction));
}

        public override bool TryCalculateCurrent(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result,
            out float current)
        {
            current = 0f;
            if (!(component is SparkCapacitor capacitor) || result == null)
                return false;

            if (!TryGetTerminals(capacitor, result.Context.Graph,
                    out SparkTerminal terminalA, out SparkTerminal terminalB))
                return false;

            if (!result.TryGetVoltage(terminalA, terminalB, out float voltage))
                return false;

            if (!result.Context.SimulateTransientDevices)
                return true;

            // CommitTransientState has already advanced the history by the
            // time external callers query the accepted solution. Recomputing
            // from that updated history would incorrectly return zero.
            // ApplySolvedState stores the current for this accepted solve.
            current = capacitor.ElectricalState.Current;
            return true;
        }

        public override bool RequiresContinuousSolve(
            SparkElectricalComponent component)
        {
            return component is SparkCapacitor;
        }

       
        public override void CommitTransientState(
            SparkElectricalComponent component,
            SparkElectricalSolveResult result)
        {
            if (!(component is SparkCapacitor capacitor) ||
                result == null ||
                result.Context == null ||
                !result.Context.SimulateTransientDevices)
                return;

            if (!TryGetTerminals(
                    capacitor,
                    result.Context.Graph,
                    out SparkTerminal terminalA,
                    out SparkTerminal terminalB))
                return;

            if (!result.TryGetVoltage(
                    terminalA,
                    terminalB,
                    out float voltage))
                return;

            // Reject NaN and infinite values before committing history.
            if (float.IsNaN(voltage) || float.IsInfinity(voltage))
            {
                Debug.LogError(
                    "[CAPACITOR] Invalid transient voltage rejected: " +
                    voltage + " | Object=" + capacitor.name,
                    capacitor);
                return;
            }

            // Diagnostic safeguard against runaway solver results.
            // This is NOT a physical voltage clamp.
            float safeVoltageLimit = Mathf.Max(
                100f,
                capacitor.VoltageRating * 10f);

            if (Mathf.Abs(voltage) > safeVoltageLimit)
            {
                Debug.LogError(
                    "[CAPACITOR] Unstable transient solution rejected." +
                    "\nObject: " + capacitor.name +
                    "\nVoltage: " + voltage + " V" +
                    "\nSafety Limit: " + safeVoltageLimit + " V" +
                    "\nTime Step: " + result.Context.TimeStep +
                    "\nTransient Enabled: " +
                        result.Context.SimulateTransientDevices,
                    capacitor);
                return;
            }

            // Commit only a finite, reasonably bounded voltage.
            result.CommitPreviousComponentVoltage(capacitor, voltage);
        }

                public override void ResetRuntimeState(
            SparkElectricalComponent component)
        {
            // Runtime capacitor voltage is owned by the solver.
            // This ScriptableObject contains configuration only.
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            minimumCapacitance =
                Mathf.Max(
                    0.000000001f,
                    minimumCapacitance);
        }
    }
}