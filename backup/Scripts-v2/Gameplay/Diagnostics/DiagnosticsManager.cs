using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class DiagnosticsManager
        : MonoBehaviour
    {
        private readonly MeasurementRecorder
            recorder = new();

        private readonly FaultDetector
            detector = new();

        public void RegisterMeasurement(
            MeasurementResult result)
        {
            recorder.Record(result);

            DiagnosticsEvents
                .RaiseMeasurement(result);
        }

        public DiagnosticReport Finish()
        {
            DiagnosticReport report =
                recorder.Build();

            detector.Analyze(report);

            DiagnosticsEvents
                .RaiseReport(report);

            return report;
        }
    }
}
