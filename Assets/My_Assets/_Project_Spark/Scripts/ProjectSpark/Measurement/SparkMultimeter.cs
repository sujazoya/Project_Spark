using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Measurement
{
    public enum SparkMultimeterMode { Off, Voltage, Current, Resistance, Continuity }

    [DisallowMultipleComponent]
    public sealed class SparkMultimeter : SparkInstrument
    {
        [SerializeField] private SparkMeasurementSystem measurementSystem;
        [SerializeField] private SparkTerminal redProbeTerminal;
        [SerializeField] private SparkTerminal blackProbeTerminal;
        [SerializeField] private SparkMultimeterMode mode = SparkMultimeterMode.Voltage;

        public SparkMultimeterMode Mode => mode;
        public SparkMeasurementReading LastReading { get; private set; }

        public bool SetMode(SparkMultimeterMode value, out string reason)
        {
            if (value == SparkMultimeterMode.Off) { reason = "Use power state to turn the meter off."; return false; }
            mode = value; reason = null; return true;
        }

        public SparkResult Measure(SparkTerminal target, in SparkInteractionContext context)
        {
            if (measurementSystem == null) return SparkResult.Unavailable("Measurement system is not configured.");
            if (mode == SparkMultimeterMode.Off) return SparkResult.Blocked("Multimeter is off.");
            var type = mode switch
            {
                SparkMultimeterMode.Voltage => SparkMeasurementType.Voltage,
                SparkMultimeterMode.Current => SparkMeasurementType.Current,
                SparkMultimeterMode.Resistance => SparkMeasurementType.Resistance,
                SparkMultimeterMode.Continuity => SparkMeasurementType.Continuity,
                _ => SparkMeasurementType.Voltage
            };
            var result = measurementSystem.TryMeasureTerminal(target, type, out var reading);
            if (result.Succeeded) LastReading = reading;
            return result;
        }
    }
}
