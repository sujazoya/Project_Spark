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
        return;

    if (context == null || !led.ElectricalEnabled)
        return;

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

    float ledVoltage =
        context.GetIterationNodeVoltage(anodeNode) -
        context.GetIterationNodeVoltage(cathodeNode);

    bool wasConducting =
        context.GetNonlinearState(led);

    float conductionThreshold = led.ForwardVoltage;

    if (wasConducting)
    {
        conductionThreshold -= Mathf.Max(
            0f,
            led.ConductionHysteresis);
    }

    bool conducting =
        ledVoltage >= conductionThreshold;

    context.SetNonlinearState(
        led,
        conducting);

    if (!conducting)
        return;

    float resistance = Mathf.Max(
        minimumResistance,
        led.OnResistance);

    // Stamp the LED exactly once.
    // AddForwardDrop handles both conductance
    // and the forward-voltage equivalent source.
    context.AddForwardDrop(
        anodeTerminal,
        cathodeTerminal,
        led.ForwardVoltage,
        resistance);
}



      public override void ApplySolvedState(
    SparkElectricalComponent component,
    SparkElectricalSolveResult result)
{
    if (!(component is SparkLED led))
        return;

    if (result == null)
        return;
        

    if (!led.ElectricalEnabled)
    {
        led.ApplyElectricalState(new SparkElectricalState());
        return;
    }

    if (!TryGetTerminals(
            led,
            result.Context.Graph,
            out SparkTerminal anodeTerminal,
            out SparkTerminal cathodeTerminal))
    {
        Debug.LogError(
            $"[LED SOLVE] Terminals unavailable for {led.name}.",
            led);

        led.ApplyElectricalState(new SparkElectricalState());
        return;
    }
if (!result.TryGetVoltage(
        anodeTerminal,
        cathodeTerminal,
        out float voltage))
{
    Debug.LogError(
        $"[LED SOLVE] Could not read solved voltage for {led.name}.",
        led);

    led.ApplyElectricalState(new SparkElectricalState());
    return;
}

       /*Debug.Log(
    "[LED CIRCUIT DIAGNOSTIC] " +
    "LED=" + led.name +
    " | AnodeID=" + anodeTerminal.GetInstanceID() +
    " | AnodeVoltage=" +
        result.GetTerminalVoltage(anodeTerminal).ToString("F4") + " V" +
    " | CathodeID=" + cathodeTerminal.GetInstanceID() +
    " | CathodeVoltage=" +
        result.GetTerminalVoltage(cathodeTerminal).ToString("F4") + " V" +
    " | LEDVoltage=" + voltage.ToString("F4") + " V",
    led);*/

    bool conducting = result.GetNonlinearState(led);
   
/*Debug.Log(
    "[LED FINAL STATE CHECK] " +
    "LED=" + led.name +
    " | FinalVoltage=" + voltage.ToString("F6") + " V" +
    " | ForwardThreshold=" +
        led.ForwardVoltage.ToString("F6") + " V" +
    " | NonlinearConducting=" + conducting,
    led);*/

    float current = 0f;

    if (conducting)
    {
        float effectiveVoltage = voltage - led.ForwardVoltage;

        float resistance = Mathf.Max(
            minimumResistance,
            led.OnResistance);

        current = Mathf.Max(0f, effectiveVoltage / resistance);

        if (led.MaximumForwardCurrent > 0f)
            current = Mathf.Min(current, led.MaximumForwardCurrent);
    }

    float power = voltage * current;

    SparkConductionState conduction =
        current > 0.000001f
            ? SparkConductionState.Conducting
            : SparkConductionState.NonConducting;

/*    Debug.Log(
        $"[LED SOLVE RESULT] LED={led.name} | " +
        $"AnodeID={anodeTerminal.GetInstanceID()} | " +
        $"CathodeID={cathodeTerminal.GetInstanceID()} | " +
        $"Voltage={voltage:F4} V | " +
        $"NonlinearConducting={conducting} | " +
        $"ForwardVoltage={led.ForwardVoltage:F4} V | " +
        $"Current={current:F6} A | " +
        $"Conduction={conduction}",
        led);
*/
        

    led.ApplyElectricalState(
        new SparkElectricalState(
            voltage,
            current,
            power,
            conduction));


           /* Debug.Log(
    "[LED APPLIED STATE] " +
    "LED=" + led.name +
    " | Voltage=" + voltage.ToString("F6") + " V" +
    " | Current=" + current.ToString("F6") + " A" +
    " | Power=" + power.ToString("F6") + " W" +
    " | Conduction=" + conduction +
    " | IsOn=" + led.IsOn,
    led);*/
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

    bool conducting = result.GetNonlinearState(led);

    if (!conducting)
    {
        current = 0f;
        return true;
    }

    // Fail explicitly if either LED terminal is missing
    // from the solved electrical graph.
    if (!result.TryGetVoltage(
            anodeTerminal,
            cathodeTerminal,
            out float voltage))
    {
        return false;
    }

    float resistance = Mathf.Max(
        minimumResistance,
        led.OnResistance);

    float effectiveVoltage =
        voltage - led.ForwardVoltage;

    current = Mathf.Max(
        0f,
        effectiveVoltage / resistance);

    if (led.MaximumForwardCurrent > 0f)
    {
        current = Mathf.Min(
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