using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class VoltageOverlay
        : MonoBehaviour
    {
        [SerializeField]
        private Gradient voltageGradient;

        public Color Evaluate(float voltage)
        {
            return voltageGradient.Evaluate(
                Mathf.Clamp01(
                    voltage / 12f));
        }
    }
}
