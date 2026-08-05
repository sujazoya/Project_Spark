using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class HeatOverlay
    {
        public Color Evaluate(
            float temperature)
        {
            return Color.Lerp(
                Color.blue,
                Color.red,
                Mathf.Clamp01(
                    temperature / 150f));
        }
    }
}
