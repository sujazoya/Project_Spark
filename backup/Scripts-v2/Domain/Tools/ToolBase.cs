using ProjectSpark.Gameplay.Diagnostics;
using UnityEngine;

namespace ProjectSpark.Domain.Tools
{
    public abstract class ToolBase : MonoBehaviour
    {
        [SerializeField]
        private ToolType toolType;

        public ToolType Type => toolType;

        public virtual void Equip()
        {
        }

        public virtual void Unequip()
        {
        }

        public abstract MeasurementResult Measure();
    }
}