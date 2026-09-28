using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;
using UnityEngine;

namespace ProjectSpark.Measurement
{
    public enum SparkMeasurementType
    {
        Voltage,
        Current,
        Resistance,
        Continuity,
        Power
    }

    public readonly struct SparkMeasurementReading
    {
        public bool Valid { get; }
        public float Value { get; }
        public SparkMeasurementType Type { get; }
        public string Unit { get; }
        public string Reason { get; }

        public SparkMeasurementReading(
            bool valid,
            float value,
            SparkMeasurementType type,
            string unit,
            string reason = null)
        {
            Valid = valid;
            Value = value;
            Type = type;
            Unit = unit;
            Reason = reason;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkMeasurementSystem : MonoBehaviour
    {
        [SerializeField]
        private SparkCircuitSystem circuit;

        [SerializeField]
        private SparkElectricalSolver solver;


        private readonly Queue<SparkTerminal>
    resistanceQueue =
        new Queue<SparkTerminal>(32);

private readonly HashSet<SparkTerminal>
    resistanceVisited =
        new HashSet<SparkTerminal>();

private readonly Dictionary<SparkTerminal, float>
    resistanceDistances =
        new Dictionary<SparkTerminal, float>(32);

        [SerializeField, Min(0.001f)]
        private float continuityResistanceThreshold = 10f;

        private readonly List<SparkCircuitConnection>
            connectionBuffer =
                new List<SparkCircuitConnection>(8);

        public event Action<SparkMeasurementReading>
            ReadingProduced;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (circuit == null)
            {
                circuit =
                    GetComponent<SparkCircuitSystem>();
            }

            if (solver == null)
            {
                solver =
                    GetComponent<SparkElectricalSolver>();
            }
        }


        // =========================================================
        // MEASUREMENT
        // =========================================================

        public SparkResult TryMeasureTerminal(
            SparkTerminal terminal,
            SparkMeasurementType type,
            out SparkMeasurementReading reading)
        {
            reading = default;

            if (terminal == null)
            {
                return SparkResult.Invalid(
                    "Measurement terminal missing.");
            }

            /*
             * A multimeter probe is not itself an electrical
             * component. Resolve the probe through the existing
             * circuit topology first.
             */
            SparkTerminal measurementTerminal =
                ResolveMeasurementTerminal(
                    terminal);                 

            if (measurementTerminal == null)
            {
                return SparkResult.Unavailable(
                    "Measurement terminal is not connected to an electrical terminal.");
            }

            /*
             * Make sure the existing electrical solver has the
             * latest circuit state before reading the terminal.
             */
            solver?.SolveNow();

            SparkTerminalElectricalState state =
                measurementTerminal.ElectricalState;

            switch (type)
            {
                case SparkMeasurementType.Voltage:

                    reading =
                        new SparkMeasurementReading(
                            true,
                            state.Voltage,
                            type,
                            "V");

                    break;


                case SparkMeasurementType.Current:

                    reading =
                        new SparkMeasurementReading(
                            true,
                            state.Current,
                            type,
                            "A");

                    break;


                case SparkMeasurementType.Power:

                    reading =
                        new SparkMeasurementReading(
                            true,
                            state.Power,
                            type,
                            "W");

                    break;


                case SparkMeasurementType.Resistance:

                    if (measurementTerminal.Owner
                        is SparkResistor resistor)
                    {
                        reading =
                            new SparkMeasurementReading(
                                true,
                                resistor.ResistanceOhms,
                                type,
                                "Ω");
                    }
                    else
                    {
                        return SparkResult.Unavailable(
                            "Target has no direct resistance measurement.");
                    }

                    break;

case SparkMeasurementType.Continuity:
    bool closed =
        measurementTerminal.ActiveConnectionCount > 0 &&
        Math.Abs(state.Current) > 0.000001f;

    reading = new SparkMeasurementReading(
        true,
        closed ? 0f : float.PositiveInfinity,
        type,
        "Ω");
    break;

                default:

                    return SparkResult.Invalid(
                        "Unsupported measurement type.");
            }

            ReadingProduced?.Invoke(
                reading);

            return SparkResult.Success();
        }

       public SparkResult TryMeasureCurrent(
    SparkTerminal redProbe,
    SparkTerminal blackProbe,
    out SparkMeasurementReading reading)
{
    reading = default;

    if (redProbe == null || blackProbe == null)
    {
        return SparkResult.Unavailable(
            "Both measurement probes are required.");
    }

    SparkTerminal redTerminal =
        ResolveMeasurementTerminal(redProbe);

    SparkTerminal blackTerminal =
        ResolveMeasurementTerminal(blackProbe);

    Debug.Log(
        $"[MULTIMETER CURRENT] " +
        $"RED PROBE={redProbe.name} → " +
        $"RED TERMINAL={redTerminal?.name ?? "NULL"} | " +
        $"BLACK PROBE={blackProbe.name} → " +
        $"BLACK TERMINAL={blackTerminal?.name ?? "NULL"}",
        this);

    if (redTerminal == null ||
        blackTerminal == null)
    {
        return SparkResult.Unavailable(
            "Both probes must be connected to electrical terminals.");
    }

    solver?.SolveNow();

    Debug.Log(
        $"[MULTIMETER CURRENT] " +
        $"RED STATE: V={redTerminal.ElectricalState.Voltage:F6} " +
        $"I={redTerminal.ElectricalState.Current:F6} | " +
        $"BLACK STATE: V={blackTerminal.ElectricalState.Voltage:F6} " +
        $"I={blackTerminal.ElectricalState.Current:F6}",
        this);

    if (redTerminal.Owner is SparkElectricalComponent redComponent)
    {
        Debug.Log(
            $"[MULTIMETER CURRENT] RED OWNER → " +
            $"{redComponent.name} | " +
            $"V={redComponent.ElectricalState.Voltage:F6} | " +
            $"I={redComponent.ElectricalState.Current:F6}",
            redComponent);
    }

    if (blackTerminal.Owner is SparkElectricalComponent blackComponent)
    {
        Debug.Log(
            $"[MULTIMETER CURRENT] BLACK OWNER → " +
            $"{blackComponent.name} | " +
            $"V={blackComponent.ElectricalState.Voltage:F6} | " +
            $"I={blackComponent.ElectricalState.Current:F6}",
            blackComponent);
    }

    if (TryGetMeasuredComponentCurrent(
            redTerminal,
            blackTerminal,
            out float current))
    {
        Debug.Log(
            $"[MULTIMETER CURRENT] DIRECT BRANCH → " +
            $"{current:F6} A",
            this);

        reading =
            new SparkMeasurementReading(
                true,
                current,
                SparkMeasurementType.Current,
                "A");

        ReadingProduced?.Invoke(reading);

        return SparkResult.Success();
    }

    Debug.LogWarning(
        "[MULTIMETER CURRENT] NO DIRECT COMPONENT BRANCH FOUND.",
        this);

    return SparkResult.Unavailable(
        "No measurable current path exists between the probes.");
}


private bool TryGetMeasuredComponentCurrent(
    SparkTerminal redTerminal,
    SparkTerminal blackTerminal,
    out float current)
{
    current = 0f;

    if (redTerminal == null ||
        blackTerminal == null)
    {
        return false;
    }

    /*
     * ---------------------------------------------------------
     * RED TERMINAL OWNER
     * ---------------------------------------------------------
     */

    if (redTerminal.Owner is SparkElectricalComponent
        redComponent)
    {
        if (TryGetTwoTerminalComponent(
                redComponent,
                out SparkTerminal a,
                out SparkTerminal b))
        {
            if ((a == redTerminal &&
                 b == blackTerminal) ||
                (a == blackTerminal &&
                 b == redTerminal))
            {
                float componentCurrent =
                    redComponent.ElectricalState.Current;

                current =
                    a == redTerminal
                        ? componentCurrent
                        : -componentCurrent;

                return true;
            }
        }
    }

    /*
     * ---------------------------------------------------------
     * BLACK TERMINAL OWNER
     * ---------------------------------------------------------
     */

    if (blackTerminal.Owner is SparkElectricalComponent
        blackComponent)
    {
        if (TryGetTwoTerminalComponent(
                blackComponent,
                out SparkTerminal a,
                out SparkTerminal b))
        {
            if ((a == redTerminal &&
                 b == blackTerminal) ||
                (a == blackTerminal &&
                 b == redTerminal))
            {
                float componentCurrent =
                    blackComponent.ElectricalState.Current;

                current =
                    a == redTerminal
                        ? componentCurrent
                        : -componentCurrent;

                return true;
            }
        }
    }

    return false;
}


private static bool TryGetTwoTerminalComponent(
    SparkElectricalComponent component,
    out SparkTerminal a,
    out SparkTerminal b)
{
    a = null;
    b = null;

    if (component == null)
    {
        return false;
    }

    if (component is SparkSwitch sparkSwitch)
    {
        a = sparkSwitch.InputTerminal;
        b = sparkSwitch.OutputTerminal;
    }
    else if (component is SparkLED sparkLED)
    {
        a = sparkLED.AnodeTerminal;
        b = sparkLED.CathodeTerminal;
    }
    else
    {
        SparkTerminal[] terminals =
            component.GetComponentsInChildren<SparkTerminal>(
                true);

        if (terminals.Length < 2)
        {
            return false;
        }

        a = terminals[0];
        b = terminals[1];
    }

    return a != null &&
           b != null;
}
       public SparkResult TryMeasureContinuity(
    SparkTerminal redProbe,
    SparkTerminal blackProbe,
    out SparkMeasurementReading reading)
{
    reading = default;

    if (redProbe == null ||
        blackProbe == null)
    {
        return SparkResult.Unavailable(
            "Both measurement probes are required.");
    }

    SparkTerminal redTerminal =
        ResolveMeasurementTerminal(
            redProbe);

    SparkTerminal blackTerminal =
        ResolveMeasurementTerminal(
            blackProbe);

    // -------------------------------------------------
    // BOTH PROBES ARE PHYSICALLY CONNECTED TO THE SAME
    // ELECTRICAL TERMINAL
    // -------------------------------------------------

    if (redTerminal != null &&
        blackTerminal != null &&
        redTerminal == blackTerminal)
    {
        reading =
            new SparkMeasurementReading(
                true,
                0f,
                SparkMeasurementType.Continuity,
                "Ω");

        ReadingProduced?.Invoke(
            reading);

        return SparkResult.Success();
    }

    // -------------------------------------------------
    // NORMAL TWO-POINT CONTINUITY TEST
    // -------------------------------------------------

    if (redTerminal == null ||
        blackTerminal == null)
    {
        return SparkResult.Unavailable(
            "Both probes must be connected to electrical terminals.");
    }

    solver?.SolveNow();

    if (!TryFindResistance(
            redTerminal,
            blackTerminal,
            out float resistance))
    {
        return SparkResult.Unavailable(
            "No conductive path exists between the probes.");
    }

    bool continuous =
        resistance <= continuityResistanceThreshold;

    reading =
        new SparkMeasurementReading(
            true,
            continuous
                ? 0f
                : float.PositiveInfinity,
            SparkMeasurementType.Continuity,
            "Ω");

    ReadingProduced?.Invoke(
        reading);

    return SparkResult.Success();
}


        // =========================================================
        // MEASUREMENT TERMINAL RESOLUTION
        // =========================================================

       private SparkTerminal ResolveMeasurementTerminal(SparkTerminal probe)
{
    if (probe == null)
    {
        Debug.Log("[MULTIMETER] Resolve FAILED: probe is NULL");
        return null;
    }

   /*Debug.Log(
        $"[MULTIMETER] Resolve START → " +
        $"Probe={probe.name} | " +
        $"Owner={probe.Owner?.name ?? "NULL"} | " +
        $"Connections={probe.ActiveConnectionCount} | " +
        $"Circuit={(circuit != null ? circuit.name : "NULL")}",
        probe);*/

    // Direct electrical terminal.
    if (probe.Owner is SparkElectricalComponent)
    {
        Debug.Log(
            $"[MULTIMETER] Resolve DIRECT → {probe.name}",
            probe);

        return probe;
    }

    if (circuit == null)
    {
        Debug.LogError(
            "[MULTIMETER] Resolve FAILED: SparkCircuitSystem reference is NULL.",
            this);

        return null;
    }

    connectionBuffer.Clear();

    circuit.GetConnections(probe, connectionBuffer);

   Debug.Log(
        $"[MULTIMETER] Resolve CONNECTIONS → " +
        $"Probe={probe.name} | " +
        $"Found={connectionBuffer.Count} | " +
        $"ActiveConnectionCount={probe.ActiveConnectionCount}",
        probe);

    for (int i = 0; i < connectionBuffer.Count; i++)
    {
        SparkCircuitConnection connection = connectionBuffer[i];

        SparkTerminal other = connection.GetOther(probe);

        Debug.Log(
            $"[MULTIMETER] Connection[{i}] → " +
            $"{connection} | " +
            $"Other={(other != null ? other.name : "NULL")} | " +
            $"OtherOwner={(other != null && other.Owner != null ? other.Owner.name : "NULL")}",
            probe);

        if (other == null)
            continue;

        if (other.Owner is SparkElectricalComponent)
        {
           /* Debug.Log(
                $"[MULTIMETER] Resolve SUCCESS → " +
                $"Probe={probe.name} → Terminal={other.name} → Owner={other.Owner.name}",
                other);*/

            return other;
        }
    }

   /* Debug.LogWarning(
        $"[MULTIMETER] Resolve FAILED → " +
        $"Probe={probe.name} has no connected electrical terminal.",
        probe);*/

    return null;
}

public SparkResult TryMeasureResistance(
    SparkTerminal redProbe,
    SparkTerminal blackProbe,
    out SparkMeasurementReading reading)
{
    reading = default;

    if (redProbe == null || blackProbe == null)
    {
        return SparkResult.Unavailable(
            "Both measurement probes are required.");
    }

    SparkTerminal redTerminal =
        ResolveMeasurementTerminal(redProbe);

    SparkTerminal blackTerminal =
        ResolveMeasurementTerminal(blackProbe);

    if (redTerminal == null || blackTerminal == null)
    {
        return SparkResult.Unavailable(
            "Both probes must be connected to electrical terminals.");
    }

    if (redTerminal == blackTerminal)
    {
        reading =
            new SparkMeasurementReading(
                true,
                0f,
                SparkMeasurementType.Resistance,
                "Ω");

        ReadingProduced?.Invoke(reading);

        return SparkResult.Success();
    }

    solver?.SolveNow();

    /*
     * Find the electrical resistance between the two probe
     * terminals by walking the existing circuit topology.
     */
   /* if (!TryFindResistance(
            redTerminal,
            blackTerminal,
            out float resistance))
    {
        return SparkResult.Unavailable(
            "No resistive path exists between the probes.");
    }*/
    if (!TryFindResistance(
        redTerminal,
        blackTerminal,
        out float resistance))
{
    Debug.LogWarning(
        $"[MULTIMETER RESISTANCE] NO PATH → " +
        $"Red={redTerminal.name} | " +
        $"Black={blackTerminal.name}");

    return SparkResult.Unavailable(
        "No resistive path exists between the probes.");
}

Debug.Log(
    $"[MULTIMETER RESISTANCE] PATH FOUND → " +
    $"Red={redTerminal.name} | " +
    $"Black={blackTerminal.name} | " +
    $"Resistance={resistance} Ω");

    reading =
        new SparkMeasurementReading(
            true,
            resistance,
            SparkMeasurementType.Resistance,
            "Ω");

    ReadingProduced?.Invoke(reading);

    return SparkResult.Success();
        }

       private bool TryFindResistance(
    SparkTerminal start,
    SparkTerminal target,
    out float resistance)
{
    resistance = 0f;

    if (start == null || target == null)
        return false;

    if (start == target)
    {
        resistance = 0f;
        return true;
    }

    resistanceQueue.Clear();
    resistanceVisited.Clear();
    resistanceDistances.Clear();

    resistanceQueue.Enqueue(start);
    resistanceVisited.Add(start);
    resistanceDistances[start] = 0f;

    while (resistanceQueue.Count > 0)
    {
        SparkTerminal current = resistanceQueue.Dequeue();

        float currentResistance =
            resistanceDistances[current];

        // -------------------------------------------------
        // COMPONENT PATH
        // -------------------------------------------------

        SparkResistor resistor =
            current.Owner as SparkResistor;

        if (resistor != null)
        {
            SparkTerminal resistorOther = null;

            if (current == resistor.TerminalA)
            {
                resistorOther = resistor.TerminalB;
            }
            else if (current == resistor.TerminalB)
            {
                resistorOther = resistor.TerminalA;
            }

            if (resistorOther != null &&
                resistor.CanConductBetween(
                    current,
                    resistorOther))
            {
                float totalResistance =
                    currentResistance +
                    resistor.ResistanceOhms;

                if (resistorOther == target)
                {
                    resistance = totalResistance;
                    return true;
                }

                if (!resistanceVisited.Contains(resistorOther))
                {
                    resistanceVisited.Add(resistorOther);

                    resistanceDistances[
                        resistorOther] =
                        totalResistance;

                    resistanceQueue.Enqueue(
                        resistorOther);
                }
            }
        }

        // -------------------------------------------------
        // NORMAL CIRCUIT CONNECTIONS
        // -------------------------------------------------

        connectionBuffer.Clear();

        circuit.GetConnections(
            current,
            connectionBuffer);

        for (int i = 0;
             i < connectionBuffer.Count;
             i++)
        {
            SparkCircuitConnection connection =
                connectionBuffer[i];

            // Measurement probes are not
            // electrical resistance paths.
            if (connection.Kind ==
                SparkConnectionKind.Probe)
            {
                continue;
            }

            SparkTerminal other =
                connection.GetOther(current);

            if (other == null)
                continue;

            if (resistanceVisited.Contains(other))
                continue;

            if (other == target)
            {
                resistance = currentResistance;
                return true;
            }

            resistanceVisited.Add(other);

            resistanceDistances[other] =
                currentResistance;

            resistanceQueue.Enqueue(other);
        }
    }

    return false;
}
    }
}