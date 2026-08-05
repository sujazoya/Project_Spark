using System;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public static class ToolEvents
    {
        public static event Action<MeasurementResult>
            MeasurementCompleted;

        public static event Action
            ToolChanged;

        public static void RaiseMeasurement(
            MeasurementResult result)
        {
            MeasurementCompleted?.Invoke(result);
        }

        public static void RaiseToolChanged()
        {
            ToolChanged?.Invoke();
        }
    }
}
