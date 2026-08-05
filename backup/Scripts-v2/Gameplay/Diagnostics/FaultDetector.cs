namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class FaultDetector
    {
        public void Analyze(
            DiagnosticReport report)
        {
            report.FaultDetected =
                report.Measurements.Count > 0;
        }
    }
}
