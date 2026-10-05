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
        // =====================================================================
        // REFERENCES
        // =====================================================================

        [Header("References")]
        [SerializeField]
        private SparkCircuitSystem circuit;

        [SerializeField]
        private SparkElectricalSolver solver;

        // =====================================================================
        // CONTINUITY / RESISTANCE SEARCH
        // =====================================================================

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

        // =====================================================================
        // EVENTS
        // =====================================================================

        public event Action<SparkMeasurementReading>
            ReadingProduced;

        // =====================================================================
        // RUNTIME MULTIMETER
        // =====================================================================

        private SparkMultimeterElectricalComponent
            activeMultimeter;

        // =====================================================================
        // UNITY
        // =====================================================================

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

        private void OnDisable()
        {
            DisableCurrentMeasurement();
        }

        // =====================================================================
        // GENERAL TERMINAL MEASUREMENT
        // =====================================================================

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

            SparkTerminal measurementTerminal =
                ResolveMeasurementTerminal(terminal);

            if (measurementTerminal == null)
            {
                return SparkResult.Unavailable(
                    "Measurement terminal is not connected to an electrical terminal.");
            }

            /*
             * A normal measurement must never leave the current
             * shunt active.
             */
            DisableCurrentMeasurement();

            Solve();

            SparkTerminalElectricalState state =
                measurementTerminal.ElectricalState;

            switch (type)
            {
                // -------------------------------------------------------------
                // VOLTAGE
                // -------------------------------------------------------------

                case SparkMeasurementType.Voltage:

                    reading =
                        new SparkMeasurementReading(
                            true,
                            state.Voltage,
                            type,
                            "V");

                    break;

                // -------------------------------------------------------------
                // CURRENT
                // -------------------------------------------------------------

                case SparkMeasurementType.Current:

                    /*
                     * A current measurement requires the dedicated
                     * two-probe overload.
                     */
                    return SparkResult.Unavailable(
                        "Use the two-probe current measurement.");

                // -------------------------------------------------------------
                // POWER
                // -------------------------------------------------------------

                case SparkMeasurementType.Power:

                    reading =
                        new SparkMeasurementReading(
                            true,
                            state.Power,
                            type,
                            "W");

                    break;

                // -------------------------------------------------------------
                // RESISTANCE
                // -------------------------------------------------------------

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

                // -------------------------------------------------------------
                // CONTINUITY
                // -------------------------------------------------------------

                case SparkMeasurementType.Continuity:

                    bool closed =
                        measurementTerminal.ActiveConnectionCount > 0 &&
                        Mathf.Abs(state.Current) > 0.000001f;

                    reading =
                        new SparkMeasurementReading(
                            true,
                            closed ? 0f : float.PositiveInfinity,
                            type,
                            "Ω");

                    break;

                default:

                    return SparkResult.Invalid(
                        "Unsupported measurement type.");
            }

            ReadingProduced?.Invoke(reading);

            return SparkResult.Success();
        }

        // =====================================================================
        // CURRENT MEASUREMENT
        // =====================================================================

        /// <summary>
        /// Measures current through the multimeter's internal shunt.
        ///
        /// IMPORTANT:
        ///
        /// The meter must physically be inserted in series.
        ///
        /// RED -> SHUNT -> BLACK
        ///
        /// The shunt is enabled only for the duration of this
        /// measurement.
        /// </summary>
        public SparkResult TryMeasureCurrent(
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
                ResolveMeasurementTerminal(redProbe);

            SparkTerminal blackTerminal =
                ResolveMeasurementTerminal(blackProbe);

            Debug.Log(
                $"[MULTIMETER CURRENT] " +
                $"RED PROBE={redProbe.name} -> " +
                $"RED TERMINAL={redTerminal?.name ?? "NULL"} | " +
                $"BLACK PROBE={blackProbe.name} -> " +
                $"BLACK TERMINAL={blackTerminal?.name ?? "NULL"}",
                this);

            if (redTerminal == null ||
                blackTerminal == null)
            {
                return SparkResult.Unavailable(
                    "Both probes must be connected to electrical terminals.");
            }

            SparkMultimeterElectricalComponent multimeter =
                FindMultimeterComponent();

            if (multimeter == null)
            {
                return SparkResult.Unavailable(
                    "No SparkMultimeterElectricalComponent found.");
            }

            if (!multimeter.TryGetTerminals(
                    out SparkTerminal multimeterRed,
                    out SparkTerminal multimeterBlack))
            {
                return SparkResult.Unavailable(
                    "Multimeter RED/BLACK terminals are not configured.");
            }

            /*
             * The probe terminals must represent the two physical
             * ends of the meter.
             *
             * We do NOT require:
             *
             *     redProbe == multimeterRed
             *
             * because the probe can have intermediate circuit
             * connections.
             *
             * What matters is that the actual multimeter terminal
             * topology is connected to the circuit.
             */
            if (!IsMultimeterTerminalConnected(
                    multimeterRed,
                    redTerminal))
            {
                return SparkResult.Unavailable(
                    "Multimeter RED terminal is not connected to the RED probe circuit.");
            }

            if (!IsMultimeterTerminalConnected(
                    multimeterBlack,
                    blackTerminal))
            {
                return SparkResult.Unavailable(
                    "Multimeter BLACK terminal is not connected to the BLACK probe circuit.");
            }

            // -------------------------------------------------------------
            // ENABLE SHUNT
            // -------------------------------------------------------------

            DisableAllOtherMeasurementState();

            activeMultimeter = multimeter;

            multimeter.SetCurrentMeasurementEnabled(true);

            /*
             * Topology has changed because the shunt is now active.
             */
            Solve();

            SparkElectricalSolveResult result =
                solver != null
                    ? solver.LatestSolveResult
                    : null;

            if (result == null)
            {
                DisableCurrentMeasurement();

                return SparkResult.Unavailable(
                    "No completed electrical solve is available.");
            }

            // -------------------------------------------------------------
            // READ SHUNT VOLTAGE
            // -------------------------------------------------------------

            if (!result.TryGetTerminalVoltage(
                    multimeterRed,
                    out float redVoltage))
            {
                DisableCurrentMeasurement();

                return SparkResult.Unavailable(
                    "Could not read multimeter RED shunt voltage.");
            }

            if (!result.TryGetTerminalVoltage(
                    multimeterBlack,
                    out float blackVoltage))
            {
                DisableCurrentMeasurement();

                return SparkResult.Unavailable(
                    "Could not read multimeter BLACK shunt voltage.");
            }

            float shuntVoltage =
                redVoltage - blackVoltage;

            float resistance =
                Mathf.Max(
                    0.000001f,
                    multimeter.ShuntResistanceOhms);

            float current =
                shuntVoltage / resistance;

            /*
             * The probe orientation determines the displayed sign.
             */
            bool reversed =
                IsMultimeterTerminalConnected(
                    multimeterRed,
                    blackTerminal) &&
                IsMultimeterTerminalConnected(
                    multimeterBlack,
                    redTerminal);

            if (reversed)
            {
                current = -current;
            }

            Debug.Log(
                $"[MULTIMETER CURRENT DEBUG] " +
                $"Meter={multimeter.name} | " +
                $"RedShuntV={redVoltage:F6} | " +
                $"BlackShuntV={blackVoltage:F6} | " +
                $"ShuntV={shuntVoltage:F6} | " +
                $"R={resistance:F6}Ω | " +
                $"Current={current:F6} A | " +
                $"Current={current * 1000f:F3} mA",
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

        // =====================================================================
        // CURRENT MODE CONTROL
        // =====================================================================

        /// <summary>
        /// Turns off the active multimeter shunt.
        /// </summary>
        public void DisableCurrentMeasurement()
        {
            if (activeMultimeter != null)
            {
                activeMultimeter
                    .SetCurrentMeasurementEnabled(false);
            }

            activeMultimeter = null;
        }

        private void DisableAllOtherMeasurementState()
        {
            if (activeMultimeter != null)
            {
                activeMultimeter
                    .SetCurrentMeasurementEnabled(false);
            }

            activeMultimeter = null;
        }

        // =====================================================================
        // MULTIMETER FIND
        // =====================================================================

        private SparkMultimeterElectricalComponent
            FindMultimeterComponent()
        {
            if (solver == null)
                return null;

            IReadOnlyList<SparkElectricalComponent>
                components =
                    solver.ElectricalComponents;

            if (components == null)
                return null;

            for (int i = 0;
                 i < components.Count;
                 i++)
            {
                if (components[i]
                    is SparkMultimeterElectricalComponent multimeter)
                {
                    return multimeter;
                }
            }

            return null;
        }

        // =====================================================================
        // MULTIMETER TERMINAL CONNECTION TEST
        // =====================================================================

        private bool IsMultimeterTerminalConnected(
            SparkTerminal multimeterTerminal,
            SparkTerminal targetTerminal)
        {
            if (multimeterTerminal == null ||
                targetTerminal == null)
            {
                return false;
            }

            if (multimeterTerminal == targetTerminal)
                return true;

            if (circuit == null)
                return false;

            /*
             * Walk the existing circuit connections.
             *
             * Probe connections are intentionally allowed here
             * because we are determining physical connection to
             * the measurement terminal.
             */
            connectionBuffer.Clear();

            circuit.GetConnections(
                multimeterTerminal,
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                SparkTerminal other =
                    connection.GetOther(
                        multimeterTerminal);

                if (other == null)
                    continue;

                if (other == targetTerminal)
                    return true;
            }

            return false;
        }

        // =====================================================================
        // SOLVE
        // =====================================================================

        private void Solve()
        {
            if (solver == null)
                return;

            solver.SolveNow();
        }

        // =====================================================================
        // TERMINAL RESOLUTION
        // =====================================================================

        private SparkTerminal ResolveMeasurementTerminal(
            SparkTerminal probe)
        {
            if (probe == null)
            {
                return null;
            }

            /*
             * Direct electrical terminal.
             */
            if (probe.Owner is SparkElectricalComponent)
            {
                return probe;
            }

            if (circuit == null)
            {
                return null;
            }

            connectionBuffer.Clear();

            circuit.GetConnections(
                probe,
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                SparkTerminal other =
                    connection.GetOther(probe);

                if (other == null)
                    continue;

                if (other.Owner
                    is SparkElectricalComponent)
                {
                    return other;
                }
            }

            return null;
        }

        // =====================================================================
        // CONTINUITY
        // =====================================================================

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
                    "Both probes are required.");
            }

            SparkTerminal redTerminal =
                ResolveMeasurementTerminal(redProbe);

            SparkTerminal blackTerminal =
                ResolveMeasurementTerminal(blackProbe);

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

                ReadingProduced?.Invoke(reading);

                return SparkResult.Success();
            }

            if (redTerminal == null ||
                blackTerminal == null)
            {
                return SparkResult.Unavailable(
                    "Both probes must be connected to electrical terminals.");
            }

            DisableCurrentMeasurement();

            Solve();

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

            ReadingProduced?.Invoke(reading);

            return SparkResult.Success();
        }

        // =====================================================================
        // RESISTANCE
        // =====================================================================

        public SparkResult TryMeasureResistance(
            SparkTerminal redProbe,
            SparkTerminal blackProbe,
            out SparkMeasurementReading reading)
        {
            reading = default;

            if (redProbe == null ||
                blackProbe == null)
            {
                return SparkResult.Unavailable(
                    "Both probes are required.");
            }

            SparkTerminal redTerminal =
                ResolveMeasurementTerminal(redProbe);

            SparkTerminal blackTerminal =
                ResolveMeasurementTerminal(blackProbe);

            if (redTerminal == null ||
                blackTerminal == null)
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

            DisableCurrentMeasurement();

            Solve();

            if (!TryFindResistance(
                    redTerminal,
                    blackTerminal,
                    out float resistance))
            {
                return SparkResult.Unavailable(
                    "No resistive path exists between the probes.");
            }

            reading =
                new SparkMeasurementReading(
                    true,
                    resistance,
                    SparkMeasurementType.Resistance,
                    "Ω");

            ReadingProduced?.Invoke(reading);

            return SparkResult.Success();
        }

        // =====================================================================
        // RESISTANCE PATH SEARCH
        // =====================================================================

        private bool TryFindResistance(
            SparkTerminal start,
            SparkTerminal target,
            out float resistance)
        {
            resistance = 0f;

            if (start == null ||
                target == null)
            {
                return false;
            }

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
                SparkTerminal current =
                    resistanceQueue.Dequeue();

                float currentResistance =
                    resistanceDistances[current];

                // -------------------------------------------------------------
                // RESISTOR
                // -------------------------------------------------------------

                SparkResistor resistor =
                    current.Owner as SparkResistor;

                if (resistor != null)
                {
                    SparkTerminal resistorOther = null;

                    if (current == resistor.TerminalA)
                    {
                        resistorOther =
                            resistor.TerminalB;
                    }
                    else if (current == resistor.TerminalB)
                    {
                        resistorOther =
                            resistor.TerminalA;
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
                            resistance =
                                totalResistance;

                            return true;
                        }

                        if (!resistanceVisited.Contains(
                                resistorOther))
                        {
                            resistanceVisited.Add(
                                resistorOther);

                            resistanceDistances[
                                resistorOther] =
                                totalResistance;

                            resistanceQueue.Enqueue(
                                resistorOther);
                        }
                    }
                }

                // -------------------------------------------------------------
                // CIRCUIT CONNECTIONS
                // -------------------------------------------------------------

                if (circuit == null)
                    continue;

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

                    /*
                     * Probe connections are not themselves
                     * electrical resistance.
                     */
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
                        resistance =
                            currentResistance;

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