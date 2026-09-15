using System;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;
using UnityEngine;

namespace ProjectSpark.Measurement
{
    public enum SparkMeasurementType { Voltage, Current, Resistance, Continuity, Power }

    public readonly struct SparkMeasurementReading
    {
        public bool Valid { get; }
        public float Value { get; }
        public SparkMeasurementType Type { get; }
        public string Unit { get; }
        public string Reason { get; }
        public SparkMeasurementReading(bool valid, float value, SparkMeasurementType type, string unit, string reason = null)
        { Valid = valid; Value = value; Type = type; Unit = unit; Reason = reason; }
    }

    [DisallowMultipleComponent]
    public sealed class SparkMeasurementSystem : MonoBehaviour
    {
        [SerializeField] private SparkCircuitSystem circuit;
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField, Min(0.001f)] private float continuityResistanceThreshold = 10f;

        public event Action<SparkMeasurementReading> ReadingProduced;

        private void Awake()
        {
            if (circuit == null) circuit = GetComponent<SparkCircuitSystem>();
            if (solver == null) solver = GetComponent<SparkElectricalSolver>();
        }

        public SparkResult TryMeasureTerminal(SparkTerminal terminal, SparkMeasurementType type, out SparkMeasurementReading reading)
        {
            reading = default;
            if (terminal == null) return SparkResult.Invalid("Measurement terminal missing.");
            if (terminal.Owner == null) return SparkResult.Unavailable("Terminal owner unavailable.");
            solver?.SolveNow();

            if (terminal.Owner is not SparkElectricalComponent electrical)
                return SparkResult.Unavailable("Target is not an electrical component.");

            var state = electrical.ElectricalState;
            switch (type)
            {
                case SparkMeasurementType.Voltage:
                    reading = new SparkMeasurementReading(true, state.Voltage, type, "V"); break;
                case SparkMeasurementType.Current:
                    reading = new SparkMeasurementReading(true, state.Current, type, "A"); break;
                case SparkMeasurementType.Power:
                    reading = new SparkMeasurementReading(true, state.Power, type, "W"); break;
                case SparkMeasurementType.Resistance:
                    if (terminal.Owner is SparkResistor r) reading = new SparkMeasurementReading(true, r.ResistanceOhms, type, "Ω");
                    else return SparkResult.Unavailable("Target has no direct resistance measurement.");
                    break;
                case SparkMeasurementType.Continuity:
                    bool closed = terminal.ActiveConnectionCount > 0 && state.Conduction != SparkConductionState.Unknown && Math.Abs(state.Current) > 0.000001f;
                    reading = new SparkMeasurementReading(true, closed ? 0f : float.PositiveInfinity, type, "Ω"); break;
                default: return SparkResult.Invalid("Unsupported measurement type.");
            }
            ReadingProduced?.Invoke(reading);
            return SparkResult.Success();
        }
    }
}
