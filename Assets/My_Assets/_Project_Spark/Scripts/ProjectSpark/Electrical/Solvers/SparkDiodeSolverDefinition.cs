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
    if (!(component is SparkDiode diode) ||
        context == null ||
        context.Graph == null ||
        !diode.isActiveAndEnabled ||
        !diode.ElectricalEnabled)
    {
        return;
    }

    if (!TryGetTerminals(
            diode,
            context.Graph,
            out SparkTerminal anode,
            out SparkTerminal cathode))
    {
        return;
    }

    if (anode == null ||
        cathode == null ||
        anode == cathode ||
        !context.Graph.TryGetNode(
            anode, out int anodeNode) ||
        !context.Graph.TryGetNode(
            cathode, out int cathodeNode))
    {
        return;
    }

    float anodeVoltage =
        context.GetIterationNodeVoltage(anodeNode);

    float cathodeVoltage =
        context.GetIterationNodeVoltage(cathodeNode);

    float diodeVoltage =
        anodeVoltage - cathodeVoltage;

   
    bool wasConducting = context.GetNonlinearState(diode);
    double resistance = Math.Max(minimumResistance, diode.OnResistance);

    // A forward-drop stamp can carry reverse current if left on below Vf.
    // Limit the hysteresis by current so low-resistance diodes cannot backfeed.
    const double currentTolerance = 0.00001d;
    double voltageTolerance = resistance * currentTolerance;
    double overdrive = (double)diodeVoltage - diode.ForwardVoltage;
    bool conducting = wasConducting
        ? overdrive >= -voltageTolerance
        : overdrive > voltageTolerance;

    context.SetNonlinearState(diode, conducting);
    if (!conducting)
        return;

    context.AddForwardDrop(
        anode,
        cathode,
        diode.ForwardVoltage,
        resistance);
}



      
public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkDiode diode))
        return;

    if (result == null ||
        result.Context == null ||
        result.Context.Graph == null ||
        !diode.isActiveAndEnabled ||
        !diode.ElectricalEnabled)
    {
        diode.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    if (!TryGetTerminals(
            diode,
            result.Context.Graph,
            out SparkTerminal anode,
            out SparkTerminal cathode))
    {
        diode.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    if (!result.TryGetVoltage(
            anode,
            cathode,
            out float voltage) ||
        float.IsNaN(voltage) ||
        float.IsInfinity(voltage))
    {
        diode.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    bool conducting =
        result.GetNonlinearState(diode);

    float current = 0f;

    if (conducting)
    {
        float resistance = Mathf.Max(
            minimumResistance,
            diode.OnResistance);

        current = Mathf.Max(
            0f,
            (voltage - diode.ForwardVoltage) /
            resistance);
    }

    float power = voltage * current;

    if (float.IsNaN(current) ||
        float.IsInfinity(current) ||
        float.IsNaN(power) ||
        float.IsInfinity(power))
    {
        Debug.LogError(
            "[DIODE] Invalid solved electrical state: " +
            diode.name,
            diode);

        diode.ApplyElectricalState(
            new SparkElectricalState());
        return;
    }

    SparkConductionState conduction =
        current > 0.000001f
            ? SparkConductionState.Conducting
            : SparkConductionState.NonConducting;

    diode.ApplyElectricalState(
        new SparkElectricalState(
            voltage,
            current,
            power,
            conduction));
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

          if (!result.TryGetVoltage(
        anodeTerminal,
        cathodeTerminal,
        out float voltage))
        {
            return false;
        }

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
    // Nonlinear DC diodes are solved by the solver's
    // iterative matrix loop, not its transient scheduler.
    return false;
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