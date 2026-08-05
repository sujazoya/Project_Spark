using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class PowerFlowOverlay
    {
        public float EvaluateSpeed(
            float current)
        {
            return current * 2f;
        }
    }
}
