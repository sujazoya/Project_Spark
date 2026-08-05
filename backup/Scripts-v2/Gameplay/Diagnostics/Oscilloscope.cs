using UnityEngine;
    using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class Oscilloscope
        : Instrument
    {
        [SerializeField]
        private int sampleCount = 512;

        private readonly float[] samples =
            new float[512];
        float voltage = 0f;

        public override void BeginMeasurement()
        {
            state = ToolState.Measuring;
        }

        public override void EndMeasurement()
        {
            state = ToolState.Finished;
        }

        public override MeasurementResult Measure()
        {
            return new MeasurementResult
            {
                Type = MeasurementType.Voltage,
                Value = voltage,
                Passed = true
            };
        }
    }
}
