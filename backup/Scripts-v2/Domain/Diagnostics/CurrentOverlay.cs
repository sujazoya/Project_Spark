using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class CurrentOverlay
    {
        public float Normalize(
            float current)
        {
            return Mathf.Clamp01(
                current / 10f);
        }
    }
}
