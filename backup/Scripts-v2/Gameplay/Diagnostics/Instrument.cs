using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public abstract class Instrument : MonoBehaviour
    {
        [SerializeField]
        protected ToolState state;

        public ToolState State => state;

        public abstract void BeginMeasurement();

        public abstract void EndMeasurement();

        public abstract MeasurementResult Measure();
    }
}
