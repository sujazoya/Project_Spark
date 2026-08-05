using UnityEngine;
using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class SignalGenerator
        : Instrument
    {
        [SerializeField]
        private float frequency = 1000f;

        [SerializeField]
        private float amplitude = 5f;

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
