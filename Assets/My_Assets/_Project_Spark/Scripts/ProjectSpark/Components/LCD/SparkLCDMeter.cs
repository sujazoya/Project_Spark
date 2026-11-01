using UnityEngine;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Two-terminal, solver-stamped, reusable electronic meter.
    /// Voltage/analysis modes are high impedance; InlineCurrent is a series shunt.
    /// Analysis of other components is handled by SparkLCDMeterDisplay.
    /// </summary>
    public enum SparkLCDMeasurementMode
    {
        AutoVoltageAndComponentCurrent,
        DCVoltage,
        VoltageToGround,
        VoltageDrop,
        InlineCurrent,
        ComponentCurrent,
        SourceCurrent,
        CurrentSum,
        Resistance,
        SeriesResistance,
        ParallelResistance,
        VoltageSum,
        ComponentPower,
        TotalPower,
        VoltageDivider,
        SameElectricalNode
    }

    [DisallowMultipleComponent]
    public sealed class SparkLCDMeter : SparkElectricalComponent
    {
        [Header("Real wire terminals (both required)")]
        [SerializeField] private SparkTerminal terminalA;
        [SerializeField] private SparkTerminal terminalB;

        [Header("Measurement mode")]
        [SerializeField] private SparkLCDMeasurementMode measurementMode =
            SparkLCDMeasurementMode.AutoVoltageAndComponentCurrent;
        [Tooltip("High impedance in volts/analysis modes. Still presents a tiny modeled load.")]
        [SerializeField, Min(1000f)] private float voltageInputResistanceOhms = 10000000f;
        [Tooltip("Series shunt for current measurement. NEVER connect across a voltage source in InlineCurrent mode.")]
        [SerializeField, Min(0.0001f)] private float inlineShuntResistanceOhms = 0.01f;

        public SparkTerminal TerminalA => terminalA;
        public SparkTerminal TerminalB => terminalB;
        public SparkLCDMeasurementMode MeasurementMode => measurementMode;
        public bool IsInlineCurrentMode => measurementMode == SparkLCDMeasurementMode.InlineCurrent;
        public float MeterResistanceOhms => IsInlineCurrentMode
            ? Mathf.Max(0.0001f, inlineShuntResistanceOhms)
            : Mathf.Max(1000f, voltageInputResistanceOhms);

        // Cached from the last completed and successfully applied solve.
        public float LastTerminalVoltage { get; private set; }
        public float LastMeterCurrent { get; private set; }
        public bool HasAppliedSolve { get; private set; }
        public bool HasStampedResistance { get; private set; }
        public float LastStampedResistance { get; private set; }
        public bool NeedsTopologyRefresh => HasStampedResistance &&
            !Mathf.Approximately(LastStampedResistance, MeterResistanceOhms);

        public void ConfigureTerminals(SparkTerminal a, SparkTerminal b)
        {
            terminalA = a;
            terminalB = b;
        }

        /// <summary>Use for UI mode buttons; restamps the network when shunt impedance changes.</summary>
        public void SetMeasurementMode(SparkLCDMeasurementMode next)
        {
            if (measurementMode == next) return;
            float before = MeterResistanceOhms;
            measurementMode = next;
            if (Mathf.Approximately(before, MeterResistanceOhms)) return;
            HasAppliedSolve = false;
            if (!Application.isPlaying) return;
            SparkElectricalSolver solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (solver == null) return;
            solver.MarkTopologyDirty();
            solver.SolveNow();
        }

        public void SetVoltageMode() => SetMeasurementMode(SparkLCDMeasurementMode.DCVoltage);
        public void SetCurrentMode() => SetMeasurementMode(SparkLCDMeasurementMode.InlineCurrent);
        public void SetAutoMode() => SetMeasurementMode(SparkLCDMeasurementMode.AutoVoltageAndComponentCurrent);

        public void NextMode()
        {
            int count = System.Enum.GetValues(typeof(SparkLCDMeasurementMode)).Length;
            SetMeasurementMode((SparkLCDMeasurementMode)(((int)measurementMode + 1) % count));
        }

        public void RecordStampedResistance()
        {
            LastStampedResistance = MeterResistanceOhms;
            HasStampedResistance = true;
        }

        public void ClearSolvedMeterState()
        {
            HasAppliedSolve = false;
            LastTerminalVoltage = 0f;
            LastMeterCurrent = 0f;
        }

        public void ReceiveSolvedMeterState(float voltage, float current)
        {
            LastTerminalVoltage = voltage;
            LastMeterCurrent = current;
            HasAppliedSolve = true;
        }
    }
}
