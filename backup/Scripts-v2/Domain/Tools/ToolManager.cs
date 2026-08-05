using UnityEngine;
using ProjectSpark.Gameplay.Diagnostics;

namespace ProjectSpark.Domain.Tools
{
    public sealed class ToolManager
        : MonoBehaviour
    {
        public ToolBase CurrentTool
        {
            get;
            private set;
        }

        public void Equip(
            ToolBase tool)
        {
            CurrentTool?.Unequip();

            CurrentTool = tool;

            CurrentTool?.Equip();
        }

        public MeasurementResult Measure()
        {
            if (CurrentTool == null)
                return default;

            return CurrentTool.Measure();
        }
    }
}
