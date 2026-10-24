using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Electrical
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkElectricalComponent))]
    public sealed class SparkElectricalSafetyLimits : MonoBehaviour
    {
        [SerializeField] private bool enableOvercurrentDetection = true;
        [SerializeField, Min(0.001f)] private float maximumSafeCurrent = 2f;
        [SerializeField, Range(0f, 1f)] private float resetHysteresis = 0.1f;
        [SerializeField] private bool overcurrentActive;

        public bool OvercurrentActive => overcurrentActive;
        internal bool WasReported { get; set; }

        public bool EvaluateCurrent(float current)
        {
            if (!enableOvercurrentDetection || float.IsNaN(current) || float.IsInfinity(current))
            {
                ResetFault();
                return false;
            }

            float magnitude = Mathf.Abs(current);
            if (overcurrentActive)
            {
                if (magnitude < maximumSafeCurrent * (1f - resetHysteresis))
                    ResetFault();
            }
            else if (magnitude > maximumSafeCurrent)
            {
                overcurrentActive = true;
            }
            return overcurrentActive;
        }

        public void ResetFault()
        {
            overcurrentActive = false;
            WasReported = false;
        }

        private void OnValidate()
        {
            maximumSafeCurrent = Mathf.Max(0.001f, maximumSafeCurrent);
        }
    }
}
