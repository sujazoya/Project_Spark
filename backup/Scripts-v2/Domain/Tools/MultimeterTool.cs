using UnityEngine;
using ProjectSpark.Gameplay.Diagnostics;


namespace ProjectSpark.Domain.Tools
{
    public sealed class MultimeterTool
        : ToolBase
    {
        [SerializeField]
        private Probe positiveProbe;

        [SerializeField]
        private Probe negativeProbe;

        public override MeasurementResult Measure()
        {
            float voltage = 0f;

            return new MeasurementResult(
             MeasurementType.Voltage,
            voltage,
             true
         );
        }
    }
}
