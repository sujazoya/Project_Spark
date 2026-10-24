using System;
using UnityEngine;
using ProjectSpark.Gameplay;
namespace ProjectSpark.Electrical
{
    /// <summary>Latest solved, signed current for one electrical component (amperes).
    /// Positive direction is defined by that component's solver definition.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkElectricalComponent))]
    public sealed class SparkComponentCurrentFeedback : MonoBehaviour
    {
        [SerializeField] private bool hasReading;
        [SerializeField] private float signedCurrentAmps;
        [SerializeField, Min(0f)] private float changeThresholdAmps = 0.0001f;

        public bool HasReading => hasReading;
        public float SignedCurrentAmps => signedCurrentAmps;
        public float MagnitudeAmps => Mathf.Abs(signedCurrentAmps);

        // Raised on first reading, significant change, or invalidation.
        public event Action<SparkComponentCurrentFeedback> ReadingChanged;

        public void ApplyReading(float amps)
        {
            if (float.IsNaN(amps) || float.IsInfinity(amps))
            {
                ClearReading();
                return;
            }

            bool changed = !hasReading ||
                Mathf.Abs(amps - signedCurrentAmps) >= changeThresholdAmps;

            hasReading = true;
            signedCurrentAmps = amps;
            if (changed) ReadingChanged?.Invoke(this);
        }

        public void ClearReading()
        {
            if (!hasReading && signedCurrentAmps == 0f) return;
            hasReading = false;
            signedCurrentAmps = 0f;
            ReadingChanged?.Invoke(this);
        }

        private void OnDisable() => ClearReading();
        private void OnValidate() => changeThresholdAmps = Mathf.Max(0f, changeThresholdAmps);
    }
}
