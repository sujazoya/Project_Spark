using UnityEngine;

using ProjectSpark.Domain.Tools;
namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class ToolManager
        : MonoBehaviour
    {
        [SerializeField]
        private MeasurementType activeTool;

        private readonly MeasurementHistory
            history = new();

        public MeasurementType ActiveTool =>
            activeTool;

        public void SelectTool(
            MeasurementType tool)
        {
            activeTool = tool;

            ToolEvents.RaiseToolChanged();
        }

        public void RecordMeasurement(
            MeasurementResult result)
        {
            history.Add(result);

            ToolEvents.RaiseMeasurement(result);
        }
    }
}
