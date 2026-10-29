using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual-only companion for SparkResistorBurn. Does not change electrical state.
    /// Assign existing scene/prefab assets in Inspector. Creates nothing at runtime.
    /// Only this component should drive the smoke graph, heat light and _Heat/_Char visuals.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBurnCinematicVFX : MonoBehaviour
    {
        [Header("Source / Mesh")]
        [SerializeField] private SparkResistorBurn burn;
        [SerializeField] private Renderer resistorBody;
        [SerializeField] private int materialIndex = 0;

        [Header("VFX / Particle Systems (already created in Editor)")]
        [SerializeField] private VisualEffect smokeGraph;
        [SerializeField] private ParticleSystem flames;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private ParticleSystem flash;
        [SerializeField] private ParticleSystem debris;
        [SerializeField] private Light flashLight;

        [Header("Smoke / Flame")]
        [SerializeField, Min(0)] private float maxSmokeRate = 48f;
        [SerializeField, Min(0)] private float maxFlameRate = 28f;
        [SerializeField, Min(0)] private float flameStartC = 175f;
        [SerializeField, Min(0)] private float failureFlameSeconds = 1.4f;
        [SerializeField, Min(0)] private float failureSmokeSeconds = 4f;

        [Header("Failure Burst")]
        [SerializeField, Min(0)] private int sparkCount = 55;
        [SerializeField, Min(0)] private int debrisCount = 16;
        [SerializeField, Min(0)] private int flashCount = 2;
        [SerializeField, Min(0)] private float flashIntensity = 7f;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.16f;
        [SerializeField] private Color flashColor = new Color(1f, 0.55f, 0.14f);

        [Header("Shader Calibration")]
        [SerializeField] private float hotC = 90f;
        [SerializeField] private float charStartC = 150f;
        [SerializeField] private float failC = 230f;

        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int CharId = Shader.PropertyToID("_Char");
        private static readonly int SmokeRateId = Shader.PropertyToID("SmokeRate");
        private MaterialPropertyBlock block;
        private bool burstPlayed;
        private float failedAt;
        private float previewUntil;
        private bool previewActive;

        private void Awake()
        {
            if (burn == null) burn = GetComponentInParent<SparkResistorBurn>();
            block = new MaterialPropertyBlock();
            StopEmission(flames);
            StopEmission(sparks);
            StopEmission(flash);
            StopEmission(debris);
            if (flashLight != null) flashLight.enabled = false;
            SetSmoke(0f);
        }

        private void OnEnable()
        {
            if (burn != null)
            {
                burn.Failed -= OnFailed;
                burn.Failed += OnFailed;
                if (burn.IsFailed && !burstPlayed)
                    OnFailed(burn);
            }
        }

        private void OnDisable()
        {
            if (burn != null) burn.Failed -= OnFailed;
            SetRate(flames, 0f);
            SetSmoke(0f);
            if (flashLight != null) flashLight.enabled = false;
        }

        private void Update()
        {
            if (burn == null) return;
            if (!burn.IsFailed && burstPlayed)
            {
                burstPlayed = false; // ResetBurn() / repair
                previewActive = false;
                SetSmoke(0f);
            }

            float now = Time.time;
            float heat = Mathf.InverseLerp(hotC, failC, burn.TemperatureC);
            float scorching = Mathf.InverseLerp(charStartC, failC, burn.TemperatureC) * 0.65f;
            float smoke = Mathf.InverseLerp(charStartC, failC, burn.TemperatureC);
            float fire = Mathf.InverseLerp(flameStartC, failC, burn.TemperatureC);

            if (burn.IsFailed || previewActive)
            {
                float elapsed = now - failedAt;
                scorching = 1f;
                heat = Mathf.Exp(-elapsed * 1.25f);
                smoke = Mathf.Clamp01(1f - elapsed / Mathf.Max(0.01f, failureSmokeSeconds));
                fire = Mathf.Clamp01(1f - elapsed / Mathf.Max(0.01f, failureFlameSeconds));
                if (previewActive && now >= previewUntil) previewActive = false;
            }

            ApplyMesh(heat, scorching);
            SetSmoke(Mathf.Max(0f, smoke) * maxSmokeRate);
            SetRate(flames, Mathf.Max(0f, fire) * maxFlameRate);

            if (flashLight != null)
            {
                float t = now - failedAt;
                bool lit = (burstPlayed || previewActive) && t >= 0f && t < flashDuration;
                flashLight.enabled = lit;
                if (lit)
                {
                    flashLight.color = flashColor;
                    flashLight.intensity = flashIntensity * Mathf.Pow(1f - t / flashDuration, 2f);
                }
            }
        }

        private void ApplyMesh(float heat, float charring)
        {
            if (resistorBody == null) return;
            // IMPORTANT: materialIndex only affects the selected material slot.
            resistorBody.GetPropertyBlock(block, materialIndex);
            block.SetFloat(HeatId, Mathf.Clamp01(heat));
            block.SetFloat(CharId, Mathf.Clamp01(charring));
            resistorBody.SetPropertyBlock(block, materialIndex);
        }

        private void SetSmoke(float rate)
        {
            if (smokeGraph != null && smokeGraph.HasFloat(SmokeRateId))
                smokeGraph.SetFloat(SmokeRateId, rate);
        }

        private static void SetRate(ParticleSystem ps, float rate)
        {
            if (ps == null) return;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = rate;
            if (!ps.isPlaying && rate > 0f) ps.Play();
        }

        private static void StopEmission(ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var emission = ps.emission;
            emission.rateOverTime = 0f;
        }

        private void OnFailed(SparkResistorBurn source)
        {
            if (burstPlayed) return;
            burstPlayed = true;
            failedAt = Time.time;
            Emit(sparks, sparkCount);
            Emit(flash, flashCount);
            Emit(debris, debrisCount);
        }

        private static void Emit(ParticleSystem ps, int count)
        {
            if (ps == null || count <= 0) return;
            if (!ps.isPlaying) ps.Play();
            ps.Emit(count);
        }

        [ContextMenu("VFX / Visual-Only Failure Preview (Play Mode)")]
        private void Preview()
        {
            if (!Application.isPlaying) return;
            if (burn == null || burn.IsFailed) return;
            previewActive = true;
            previewUntil = Time.time + Mathf.Max(failureSmokeSeconds, failureFlameSeconds);
            failedAt = Time.time;
            Emit(sparks, sparkCount);
            Emit(flash, flashCount);
            Emit(debris, debrisCount);
        }
    }
}
