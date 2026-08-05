using UnityEngine;
using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class LogicProbe : Instrument
    {
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
                Value = 1f,
                Passed = true
            };
        }
    }
}