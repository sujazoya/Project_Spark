using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public abstract class DiagnosticTool : MonoBehaviour
    {
        public abstract MeasurementResult Measure(
            InspectionPoint point);
    }
}
