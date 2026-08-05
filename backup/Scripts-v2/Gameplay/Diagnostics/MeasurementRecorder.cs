namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class MeasurementRecorder
    {
        private readonly DiagnosticReport report =
            new();

        public void Record(
            MeasurementResult result)
        {
            report.Measurements.Add(result);
        }

        public DiagnosticReport Build()
        {
            return report;
        }
    }
}
