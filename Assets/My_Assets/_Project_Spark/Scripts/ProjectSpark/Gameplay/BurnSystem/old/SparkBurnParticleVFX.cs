using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Self-contained procedural resistor smoke / sparks. No prefab or VFX Graph asset required.
    /// Attach to a child of the resistor, position it just above the ceramic body.
    /// Optional: connect SetHeat() to SparkResistorBurn temperature, and Burst() to its Failed event.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBurnParticleVFX : MonoBehaviour
    {
        [Header("Smoke")]
        [SerializeField, Range(0f, 1f)] private float heat;
        [SerializeField, Min(0f)] private float maximumSmokePerSecond = 45f;
        [SerializeField] private Color smokeColor = new Color(.18f, .18f, .18f, .45f);
        [Header("Sparks")]
        [SerializeField, Min(1)] private int burstSparkCount = 32;
        [SerializeField] private Color sparkColor = new Color(1f, .38f, .06f, 1f);
        [Header("Lighting")]
        [SerializeField] private Light heatLight;
        [SerializeField, Min(0f)] private float maxLightIntensity = 2f;
        private ParticleSystem smoke;
        private ParticleSystem sparks;
        private float smokeAccumulator;
        private bool failed;
        private Material runtimeMaterial;
        private Texture2D runtimeTexture;

        public void SetHeat(float normalizedHeat)
        {
            heat = failed ? 0f : Mathf.Clamp01(normalizedHeat);
        }

        public void SetFailed(bool value)
        {
            if (value && !failed) Burst();
            failed = value;
            if (failed) heat = 0f;
        }

        public void ResetEffect()
        {
            failed = false;
            heat = 0f;
            smokeAccumulator = 0f;
            if (smoke != null) smoke.Clear();
            if (sparks != null) sparks.Clear();
        }

        [ContextMenu("VFX / Test Burn Burst")]
        public void Burst()
        {
            EnsureCreated();
            for (int i = 0; i < burstSparkCount; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y) + .1f;
                dir.Normalize();
                var p = new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = dir * Random.Range(.65f, 2.3f),
                    startLifetime = Random.Range(.18f, .65f),
                    startSize = Random.Range(.009f, .027f),
                    startColor = sparkColor
                };
                sparks.Emit(p, 1);
            }
            for (int i = 0; i < 24; i++) EmitSmoke(true);
        }

        private void Awake() => EnsureCreated();

        private void Update()
        {
            EnsureCreated();
            if (heatLight != null)
            {
                heatLight.intensity = failed ? 0f : maxLightIntensity * heat * heat;
                heatLight.color = new Color(1f, .28f + .25f * (1f - heat), .04f);
            }
            float smokeFactor = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.35f, 1f, heat));
            smokeAccumulator += smokeFactor * maximumSmokePerSecond * Time.deltaTime;
            int count = Mathf.Min(100, Mathf.FloorToInt(smokeAccumulator));
            smokeAccumulator -= count;
            for (int i = 0; i < count; i++) EmitSmoke(false);
        }

        private void EmitSmoke(bool burst)
        {
            if (smoke == null) return;
            var p = new ParticleSystem.EmitParams
            {
                position = Random.insideUnitSphere * (burst ? .065f : .025f),
                velocity = new Vector3(Random.Range(-.09f, .09f), Random.Range(.2f, .48f), Random.Range(-.09f, .09f)),
                startLifetime = Random.Range(.85f, 1.9f),
                startSize = Random.Range(burst ? .055f : .025f, burst ? .12f : .065f),
                startColor = smokeColor
            };
            smoke.Emit(p, 1);
        }

        private void EnsureCreated()
        {
            if (smoke != null && sparks != null) return;
            if (runtimeMaterial == null) MakeMaterial();
            smoke = MakeSystem("Burn Smoke", 350, false);
            sparks = MakeSystem("Burn Sparks", 160, true);
        }

        private ParticleSystem MakeSystem(string label, int maxParticles, bool isSpark)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startLifetime = 1f;
            main.startSize = .05f;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = false;
            var noise = ps.noise;
            noise.enabled = !isSpark;
            if (!isSpark)
            {
                noise.strength = .12f;
                noise.frequency = .7f;
                noise.scrollSpeed = .35f;
            }
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, isSpark ? 1f : .45f),
                                   new Keyframe(1f, isSpark ? .15f : 2.3f)));
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .14f),
                        new GradientAlphaKey(.75f, .7f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            if (isSpark)
            {
                var gravity = ps.main;
                gravity.gravityModifier = .9f;
            }
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = runtimeMaterial;
            ps.Play();
            return ps;
        }

        private void MakeMaterial()
        {
            runtimeTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            runtimeTexture.name = "SparkBurn Soft Particle (Runtime)";
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dx = (x - 31.5f) / 31.5f;
                float dy = (y - 31.5f) / 31.5f;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - dx * dx - dy * dy), 2.5f);
                runtimeTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            runtimeTexture.Apply();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("[SparkBurnVFX] No compatible particle shader found.", this);
                return;
            }
            runtimeMaterial = new Material(shader);
            runtimeMaterial.name = "SparkBurn Runtime Particles";
            if (runtimeMaterial.HasProperty("_BaseMap")) runtimeMaterial.SetTexture("_BaseMap", runtimeTexture);
            if (runtimeMaterial.HasProperty("_MainTex")) runtimeMaterial.SetTexture("_MainTex", runtimeTexture);
            if (runtimeMaterial.HasProperty("_Surface")) runtimeMaterial.SetFloat("_Surface", 1f);
            if (runtimeMaterial.HasProperty("_Blend")) runtimeMaterial.SetFloat("_Blend", 0f);
            if (runtimeMaterial.HasProperty("_ZWrite")) runtimeMaterial.SetFloat("_ZWrite", 0f);
            runtimeMaterial.renderQueue = 3000;
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
            if (runtimeTexture != null) Destroy(runtimeTexture);
        }
    }
}
