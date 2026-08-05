using ProjectSpark.Domain.Tools;
using System;

namespace ProjectSpark.Gameplay.Diagnostics
{
    [Serializable]
    public class MeasurementResult
    {
        public MeasurementType Type;

        public string ProbeA;

        public string ProbeB;

        public float Value;

        public bool Passed;

        public MeasurementResult()
        {
        }

        public MeasurementResult(
            MeasurementType type,
            float value,
            bool passed)
        {
            Type = type;
            Value = value;
            Passed = passed;
        }

        public static MeasurementResult Success(
            MeasurementType type,
            float value)
        {
            return new MeasurementResult(type, value, true);
        }

        public static MeasurementResult Failure(
            MeasurementType type,
            float value)
        {
            return new MeasurementResult(type, value, false);
        }
    }
}