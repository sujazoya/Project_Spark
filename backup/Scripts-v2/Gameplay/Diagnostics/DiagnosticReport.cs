using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class DiagnosticReport
    {
        public readonly List<MeasurementResult>
            Measurements = new();

        public bool FaultDetected;
        public string FaultId;
    }
}
