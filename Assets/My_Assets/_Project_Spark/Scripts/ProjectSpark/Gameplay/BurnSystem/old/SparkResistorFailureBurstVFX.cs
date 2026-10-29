using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// One-shot high-impact failure burst for SparkResistorBurn.
    /// Assign three existing, editor-created ParticleSystems and an optional Light.
    /// Creates NO GameObjects, materials, UI, or particle systems at runtime.
    /// Does not change electrical state or circuit wiring.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkResistorFailureBurstVFX : MonoBehaviour
    {
        [Header("Existing References (drag from Inspector)")]
        [SerializeField] private SparkResistorBurn burn;
        [SerializeField] private ParticleSystem sparkEmitter;
        [SerializeField] private ParticleSystem flashEmitter;
        [SerializeField] private ParticleSystem debrisEmitter;
        [SerializeField] private Light flashLight;

        [Header("Burst Amounts")]
        [SerializeField, Range(0, 150)] private int sparkCount = 46;
        [SerializeField, Range(0, 30)] private int debrisCount = 12;
        [SerializeField, Range(0, 10)] private int flashCount = 2;

        [Header("Effect Scale (Unity world units)")]
        [SerializeField, Min(0.01f)] private float intensityScale = 1f;
        [SerializeField] private float sparkSpeed = 1.55f;
        [SerializeField] private float sparkLifetime = 0.55f;
        [SerializeField] private float sparkSize = 0.014f;
        [SerializeField] private float debrisSpeed = 0.68f;
        [SerializeField] private float debrisSize = 0.012f;
        [SerializeField] private float flashSize = 0.16f;

        [Header("Colors")]
        [SerializeField] private Color sparkColor = new Color(1f, 0.57f, 0.12f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 0.86f, 0.55f, 0.85f);
        [SerializeField] private Color debrisColor = new Color(0.12f, 0.075f, 0.05f, 1f);

        [Header("Optional Flash Light")]
        [SerializeField, Min(0f)] private float peakLightIntensity = 5f;
        [SerializeField, Range(0.01f, 1f)] private float lightFadeSeconds = 0.17f;

        private float lightTimer;
        private bool subscribed;

        private void Awake()
        {
            if (burn == null)
                burn = GetComponentInParent<SparkResistorBurn>();

            if (flashLight != null)
                flashLight.intensity = 0f;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            lightTimer = 0f;
            if (flashLight != null) flashLight.intensity = 0f;
        }

        private void Subscribe()
        {
            if (burn == null)
                burn = GetComponentInParent<SparkResistorBurn>();

            if (burn == null || subscribed)
                return;

            burn.Failed += OnBurnFailed;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (subscribed && burn != null)
                burn.Failed -= OnBurnFailed;
            subscribed = false;
        }

        private void OnBurnFailed(SparkResistorBurn source)
        {
            PlayFailureBurst();
        }

        [ContextMenu("VFX / Preview Failure Burst")]
        public void PlayFailureBurst()
        {
            EmitSparks();
            EmitDebris();
            EmitFlash();

            if (flashLight != null)
            {
                flashLight.color = flashColor;
                flashLight.intensity = peakLightIntensity;
                lightTimer = lightFadeSeconds;
            }
        }

        private void EmitSparks()
        {
            if (sparkEmitter == null) return;
            EnsurePlaying(sparkEmitter);

            for (int i = 0; i < sparkCount; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                direction.y = Mathf.Abs(direction.y) * 0.55f + 0.15f;
                direction.Normalize();

                var e = new ParticleSystem.EmitParams
                {
                    position = Random.insideUnitSphere * (0.014f * intensityScale),
                    velocity = direction * Random.Range(0.55f, 1.25f) * sparkSpeed * intensityScale,
                    startLifetime = Random.Range(0.45f, 1.1f) * sparkLifetime,
                    startSize = Random.Range(0.55f, 1.5f) * sparkSize * intensityScale,
                    startColor = sparkColor
                };
                sparkEmitter.Emit(e, 1);
            }
        }

        private void EmitDebris()
        {
            if (debrisEmitter == null) return;
            EnsurePlaying(debrisEmitter);

            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                direction.y = Mathf.Abs(direction.y) * 0.8f + 0.25f;
                direction.Normalize();

                var e = new ParticleSystem.EmitParams
                {
                    position = Random.insideUnitSphere * (0.025f * intensityScale),
                    velocity = direction * Random.Range(0.45f, 1.25f) * debrisSpeed * intensityScale,
                    startLifetime = Random.Range(0.45f, 1.5f),
                    startSize = Random.Range(0.6f, 1.45f) * debrisSize * intensityScale,
                    startColor = debrisColor
                };
                debrisEmitter.Emit(e, 1);
            }
        }

        private void EmitFlash()
        {
            if (flashEmitter == null) return;
            EnsurePlaying(flashEmitter);

            for (int i = 0; i < flashCount; i++)
            {
                var e = new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = Vector3.zero,
                    startLifetime = Random.Range(0.065f, 0.12f),
                    startSize = Random.Range(0.8f, 1.35f) * flashSize * intensityScale,
                    startColor = flashColor
                };
                flashEmitter.Emit(e, 1);
            }
        }

        private static void EnsurePlaying(ParticleSystem ps)
        {
            if (!ps.isPlaying)
                ps.Play(true);
        }

        private void Update()
        {
            if (flashLight == null || lightTimer <= 0f)
                return;

            lightTimer = Mathf.Max(0f, lightTimer - Time.deltaTime);
            float t = lightTimer / Mathf.Max(0.01f, lightFadeSeconds);
            flashLight.intensity = peakLightIntensity * t * t;
        }

        private void OnValidate()
        {
            intensityScale = Mathf.Max(0.01f, intensityScale);
            sparkSpeed = Mathf.Max(0f, sparkSpeed);
            sparkLifetime = Mathf.Max(0.01f, sparkLifetime);
            sparkSize = Mathf.Max(0f, sparkSize);
            debrisSpeed = Mathf.Max(0f, debrisSpeed);
            debrisSize = Mathf.Max(0f, debrisSize);
            flashSize = Mathf.Max(0f, flashSize);
        }
    }
}
