using System;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public static class DiagnosticsEvents
    {
        public static event Action<MeasurementResult>
            MeasurementTaken;

        public static event Action<DiagnosticReport>
            ReportCompleted;

        public static void RaiseMeasurement(
            MeasurementResult result)
        {
            MeasurementTaken?.Invoke(result);
        }

        public static void RaiseReport(
            DiagnosticReport report)
        {
            ReportCompleted?.Invoke(report);
        }
    }
}
