using UnityEngine;
using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public enum WaveformType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth,
        Pulse
    }

    public sealed class FunctionGenerator
        : Instrument
    {
        [SerializeField]
        private WaveformType waveform =
            WaveformType.Sine;

        [SerializeField]
        private float frequency = 1000f;

        public override void BeginMeasurement()
        {
        }

        public override void EndMeasurement()
        {
        }

        public override MeasurementResult Measure()
        {
            return new MeasurementResult
            {
                Type = MeasurementType.Frequency,
                Value = frequency,
                Passed = true
            };
        }
    }
}
