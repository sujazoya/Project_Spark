using UnityEngine;
using ProjectSpark.Domain.Tools;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class DigitalMultimeter
        : Instrument
    {
        [SerializeField]
        private MeasurementType mode =
            MeasurementType.Voltage;

        public MeasurementType Mode
            => mode;

        public void SetMode(
            MeasurementType value)
        {
            mode = value;
        }

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
                Type = mode,
                Passed = false
            };
        }
    }
}
