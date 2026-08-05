using UnityEngine;
using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class BenchPowerSupply
        : Instrument
    {
        [SerializeField]
        private float outputVoltage = 5f;

        [SerializeField]
        private float currentLimit = 1f;

        public float OutputVoltage => outputVoltage;

        public float CurrentLimit => currentLimit;

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
                Type = MeasurementType.Voltage,
                ProbeA = "",
                ProbeB = "",
                Value = outputVoltage,
                Passed = true
            };
        }
    }
}
