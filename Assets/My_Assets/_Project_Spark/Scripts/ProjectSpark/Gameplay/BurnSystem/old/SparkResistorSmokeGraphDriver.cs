using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Optional VFX Graph driver for the existing resistor burn system.
    /// Attach manually to the resistor or its existing VFX child.
    /// Assign an existing SparkResistorBurn and VisualEffect in the Inspector.
    /// Does not create GameObjects, alter wiring, or change electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkResistorSmokeGraphDriver : MonoBehaviour
    {
        [Header("Existing References")]
        [SerializeField] private SparkResistorBurn burn;
        [SerializeField] private VisualEffect smokeEffect;

        [Header("Temperature Thresholds (C)")]
        [SerializeField] private float smokeStartTemperatureC = 90f;
        [SerializeField] private float fullSmokeTemperatureC = 230f;

        [Header("Spawn Rate (particles / second)")]
        [SerializeField, Min(0f)] private float maximumSmokeRate = 35f;
        [SerializeField, Min(0f)] private float failedSmokeRate = 55f;
        [SerializeField, Min(0f)] private float failureSmokeDuration = 2f;

        private static readonly int SmokeRateId = Shader.PropertyToID("SmokeRate");
        private float failedUntil;
        private bool wasFailed;

        private void Awake()
        {
            if (burn == null)
                burn = GetComponentInParent<SparkResistorBurn>();
            if (smokeEffect == null)
                smokeEffect = GetComponent<VisualEffect>();
        }

        private void OnEnable()
        {
            wasFailed = burn != null && burn.IsFailed;
            failedUntil = wasFailed ? Time.time + failureSmokeDuration : 0f;
            ApplyRate(0f);
        }

        private void Update()
        {
            if (burn == null || smokeEffect == null)
                return;

            bool failed = burn.IsFailed;
            if (failed && !wasFailed)
                failedUntil = Time.time + failureSmokeDuration;
            if (!failed && wasFailed)
                failedUntil = 0f;
            wasFailed = failed;

            float rate;
            if (failed)
            {
                rate = Time.time < failedUntil ? failedSmokeRate : 0f;
            }
            else
            {
                float fraction = Mathf.InverseLerp(
                    smokeStartTemperatureC,
                    Mathf.Max(smokeStartTemperatureC + 0.01f, fullSmokeTemperatureC),
                    burn.TemperatureC);
                rate = fraction * fraction * maximumSmokeRate;
            }

            ApplyRate(rate);
        }

        private void OnDisable()
        {
            ApplyRate(0f);
        }

        private void ApplyRate(float rate)
        {
            if (smokeEffect != null && smokeEffect.HasFloat(SmokeRateId))
                smokeEffect.SetFloat(SmokeRateId, Mathf.Max(0f, rate));
        }

        private void OnValidate()
        {
            maximumSmokeRate = Mathf.Max(0f, maximumSmokeRate);
            failedSmokeRate = Mathf.Max(0f, failedSmokeRate);
            failureSmokeDuration = Mathf.Max(0f, failureSmokeDuration);
            fullSmokeTemperatureC = Mathf.Max(
                smokeStartTemperatureC + 0.01f, fullSmokeTemperatureC);
        }
    }
}
