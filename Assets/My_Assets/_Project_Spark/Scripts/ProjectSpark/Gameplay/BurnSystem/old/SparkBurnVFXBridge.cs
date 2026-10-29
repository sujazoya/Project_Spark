using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Connects existing SparkResistorBurn thermal state to procedural burn particles.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkBurnVFXBridge : MonoBehaviour
    {
        [SerializeField] private SparkResistorBurn burn;
        [SerializeField] private SparkBurnParticleVFX particles;
        [SerializeField] private float hotTemperatureC = 90f;
        [SerializeField] private float failureTemperatureC = 230f;

        private void Awake()
        {
            if (burn == null) burn = GetComponentInParent<SparkResistorBurn>();
            if (particles == null) particles = GetComponentInChildren<SparkBurnParticleVFX>(true);
        }

        private void OnEnable()
        {
            if (burn != null) burn.Failed += OnFailed;
        }

        private void OnDisable()
        {
            if (burn != null) burn.Failed -= OnFailed;
        }

        private void Update()
        {
            if (burn == null || particles == null) return;
            particles.SetHeat(Mathf.InverseLerp(hotTemperatureC, failureTemperatureC, burn.TemperatureC));
            if (burn.IsFailed) particles.SetFailed(true);
            else if (wasFailed) { particles.ResetEffect(); wasFailed = false; }
        }

        private bool wasFailed;
        private void OnFailed(SparkResistorBurn source)
        {
            if (particles != null) particles.SetFailed(true);
            wasFailed = true;
        }
    }
}
