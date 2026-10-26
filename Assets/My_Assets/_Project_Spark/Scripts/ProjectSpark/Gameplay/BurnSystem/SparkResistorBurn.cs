
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkResistorBurn : MonoBehaviour
    {
        public enum FailureMode
        {
            OpenCircuit,
            ShortCircuit
        }

        public enum BurnState
        {
            Normal,
            Hot,
            Smoking,
            Failed
        }

        [Header("Electrical Ratings")]
        [SerializeField, Min(0.001f)]
        private float ratedWatts = 0.25f;

        [SerializeField, Min(0.001f)]
        private float thermalCapacityJPerC = 1.2f;

        [SerializeField, Min(0.001f)]
        private float coolingWattsPerC = 0.018f;

        [SerializeField]
        private float ambientC = 25f;

        [SerializeField]
        private float hotC = 90f;

        [SerializeField]
        private float smokeC = 145f;

        [SerializeField]
        private float failureC = 230f;

        [SerializeField, Min(0f)]
        private float failureExposureSeconds = 0.3f;

        [SerializeField]
        private FailureMode failureMode =
            FailureMode.OpenCircuit;

        [Header("Resistor Visuals")]
        [SerializeField]
        private Renderer[] heatedRenderers;

        [SerializeField]
        private Light heatLight;

        [SerializeField, Min(0f)]
        private float maximumHeatLightIntensity = 1.5f;

        [Header("Optional GPU VFX Graph")]
        [SerializeField]
        private VisualEffect smokeVfx;

        [SerializeField]
        private VisualEffect failureVfx;

        [SerializeField]
        private string smokeRateProperty = "SmokeRate";

        [SerializeField]
        private string heatProperty = "Heat";

        [SerializeField]
        private string burstEventName = "OnBurn";

        [Header("Runtime VFX")]
        [SerializeField]
        private bool generateRuntimeVfx = true;

        [SerializeField]
        private Transform vfxOrigin;

        [SerializeField]
        private Material particleMaterial;

        [SerializeField]
        private Vector3 localVfxOffset =
            new Vector3(0f, 0.035f, 0f);

        [SerializeField, Range(0.1f, 3f)]
        private float vfxScale = 1f;

        [SerializeField, Min(0f)]
        private float maximumSmokeRate = 24f;

        [SerializeField, Min(0f)]
        private float maximumEmberRate = 6f;

        [SerializeField, Range(1, 100)]
        private int failureSparkCount = 35;

        [SerializeField, Range(1, 100)]
        private int failureSmokeCount = 25;

        [Header("Audio / Events")]
        [SerializeField]
        private AudioSource failureAudio;

        [SerializeField]
        private UnityEvent onFailed;

        public float TemperatureC { get; private set; }
        public float PowerWatts { get; private set; }
        public bool IsFailed { get; private set; }
        public BurnState State { get; private set; }

        public float RatedWatts => ratedWatts;

        public FailureMode ElectricalFailureMode =>
            failureMode;

        public event Action<SparkResistorBurn> Failed;

        private bool electricallyEnergized;
        private float exposureSeconds;

        private MaterialPropertyBlock propertyBlock;

        private static readonly int HeatId =
            Shader.PropertyToID("_Heat");

        private static readonly int CharId =
            Shader.PropertyToID("_Char");

        private ParticleSystem smokeParticles;
        private ParticleSystem emberParticles;
        private ParticleSystem sparkParticles;
        private ParticleSystem flashParticles;

        private GameObject runtimeVfxRoot;
        private Material runtimeParticleMaterial;
        private bool ownsParticleMaterial;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (heatedRenderers == null ||
                heatedRenderers.Length == 0)
            {
                heatedRenderers =
                    GetComponentsInChildren<Renderer>(true);
            }

            TemperatureC = ambientC;

            if (generateRuntimeVfx)
                CreateRuntimeVfx();

            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (runtimeVfxRoot != null)
                runtimeVfxRoot.SetActive(true);
        }

        private void OnDisable()
        {
            if (runtimeVfxRoot != null)
                runtimeVfxRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (runtimeVfxRoot != null)
                Destroy(runtimeVfxRoot);

            if (ownsParticleMaterial &&
                runtimeParticleMaterial != null)
            {
                Destroy(runtimeParticleMaterial);
            }
        }

        // =====================================================
        // ELECTRICAL INPUT
        // =====================================================

        public void SetElectricalState(
            float voltageRms,
            float currentRms,
            bool energized = true)
        {
            electricallyEnergized = energized;

            if (IsFailed || !energized)
            {
                PowerWatts = 0f;
                return;
            }

            if (!IsFinite(voltageRms) ||
                !IsFinite(currentRms))
            {
                PowerWatts = 0f;
                return;
            }

            PowerWatts =
                Mathf.Abs(voltageRms * currentRms);
        }

        public void SetDissipatedPower(float watts)
        {
            if (IsFailed)
            {
                PowerWatts = 0f;
                return;
            }

            electricallyEnergized = true;

            PowerWatts = IsFinite(watts)
                ? Mathf.Max(0f, watts)
                : 0f;
        }

        public void SetDeenergized()
        {
            electricallyEnergized = false;
            PowerWatts = 0f;
        }

        // =====================================================
        // THERMAL SIMULATION
        // =====================================================

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);

            if (dt <= 0f)
                return;

            float inputWatts =
                IsFailed || !electricallyEnergized
                    ? 0f
                    : PowerWatts;

            float coolingWatts =
                coolingWattsPerC *
                (TemperatureC - ambientC);

            TemperatureC = Mathf.Max(
                ambientC,
                TemperatureC +
                (inputWatts - coolingWatts) *
                dt / thermalCapacityJPerC);

            if (!IsFailed && TemperatureC >= failureC)
                exposureSeconds += dt;
            else
                exposureSeconds = 0f;

            if (!IsFailed &&
                exposureSeconds >= failureExposureSeconds)
            {
                Fail();
            }

            State = IsFailed
                ? BurnState.Failed
                : TemperatureC >= smokeC
                    ? BurnState.Smoking
                    : TemperatureC >= hotC
                        ? BurnState.Hot
                        : BurnState.Normal;

            UpdateVisuals();
        }

        private void Fail()
        {
            if (IsFailed)
                return;

            IsFailed = true;
            PowerWatts = 0f;
            electricallyEnergized = false;
            State = BurnState.Failed;

            TriggerRuntimeBurnBurst();

            if (failureVfx != null)
                failureVfx.SendEvent(burstEventName);

            if (failureAudio != null)
                failureAudio.Play();

            onFailed?.Invoke();
            Failed?.Invoke(this);
        }

        // =====================================================
        // RUNTIME VFX CREATION
        // =====================================================

        private void CreateRuntimeVfx()
        {
            if (runtimeVfxRoot != null)
                return;

            runtimeVfxRoot =
                new GameObject("BurnVFX_Runtime");

            Transform parent =
                vfxOrigin != null
                    ? vfxOrigin
                    : transform;

            runtimeVfxRoot.transform.SetParent(
                parent, false);

            runtimeVfxRoot.transform.localPosition =
                localVfxOffset;

            runtimeVfxRoot.transform.localRotation =
                Quaternion.identity;

            runtimeVfxRoot.transform.localScale =
                Vector3.one * vfxScale;

            SetupParticleMaterial();

            smokeParticles = CreateSmokeSystem();
            emberParticles = CreateEmberSystem();
            sparkParticles = CreateSparkSystem();
            flashParticles = CreateFlashSystem();
        }

        private void SetupParticleMaterial()
        {
            if (particleMaterial != null)
            {
                runtimeParticleMaterial =
                    particleMaterial;

                ownsParticleMaterial = false;
                return;
            }

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
                shader = Shader.Find("Particles/Standard Unlit");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogWarning(
                    "[BurnVFX] No compatible particle shader found.",
                    this);
                return;
            }

            runtimeParticleMaterial =
                new Material(shader);

            runtimeParticleMaterial.name =
                "SparkBurn_RuntimeParticleMaterial";

            ownsParticleMaterial = true;
        }

        private ParticleSystem CreateParticleSystem(
            string objectName,
            int maxParticles,
            float lifetime,
            float speed,
            float size,
            Color color)
        {
            GameObject go = new GameObject(objectName);

            go.transform.SetParent(
                runtimeVfxRoot.transform, false);

            ParticleSystem ps =
                go.AddComponent<ParticleSystem>();

            ps.Stop(true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear);

            var main = ps.main;

            main.loop = true;
            main.playOnAwake = false;
            main.duration = 5f;
            main.maxParticles = maxParticles;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius = 0.025f * vfxScale;

            ParticleSystemRenderer renderer =
                go.GetComponent<ParticleSystemRenderer>();

            renderer.renderMode =
                ParticleSystemRenderMode.Billboard;

            renderer.sharedMaterial =
                runtimeParticleMaterial;

            renderer.sortingOrder = 10;

            ps.Play();

            return ps;
        }

        // =====================================================
        // SMOKE
        // =====================================================

        private ParticleSystem CreateSmokeSystem()
        {
            ParticleSystem ps = CreateParticleSystem(
                "Smoke",
                300,
                2.5f,
                0.12f,
                0.045f,
                new Color(0.2f, 0.2f, 0.2f, 0.4f));

            var main = ps.main;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    1.5f, 3.5f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    0.05f, 0.18f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.025f, 0.065f);
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;

            velocity.space =
                ParticleSystemSimulationSpace.Local;

            // All three axes must use the same curve mode:
            // TwoConstants.

            velocity.x = new ParticleSystem.MinMaxCurve(
                -0.035f,
                0.035f);

            velocity.y = new ParticleSystem.MinMaxCurve(
                0.12f,
                0.28f);

            velocity.z = new ParticleSystem.MinMaxCurve(
                -0.035f,
                0.035f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.12f;
            noise.frequency = 0.6f;
            noise.scrollSpeed = 0.25f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;

            AnimationCurve sizeCurve =
                new AnimationCurve(
                    new Keyframe(0f, 0.25f),
                    new Keyframe(0.3f, 0.7f),
                    new Keyframe(1f, 1.8f));

            sizeOverLifetime.size =
                new ParticleSystem.MinMaxCurve(
                    1f, sizeCurve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.28f, 0.28f, 0.28f),
                        0f),

                    new GradientColorKey(
                        new Color(0.12f, 0.12f, 0.12f),
                        0.5f),

                    new GradientColorKey(
                        new Color(0.35f, 0.35f, 0.35f),
                        1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.35f, 0.2f),
                    new GradientAlphaKey(0.2f, 0.65f),
                    new GradientAlphaKey(0f, 1f)
                });

            colorOverLifetime.color =
                new ParticleSystem.MinMaxGradient(
                    gradient);

            return ps;
        }

        // =====================================================
        // EMBERS
        // =====================================================

        private ParticleSystem CreateEmberSystem()
        {
            ParticleSystem ps = CreateParticleSystem(
                "Embers",
                120,
                0.7f,
                0.25f,
                0.008f,
                new Color(1f, 0.35f, 0.02f, 1f));

            var main = ps.main;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.25f, 0.8f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    0.08f, 0.35f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.004f, 0.012f);

          var gravity = ps.forceOverLifetime;
            gravity.enabled = true;

            // All axes use TwoConstants mode.
            gravity.x = new ParticleSystem.MinMaxCurve(
                -0.05f, 0.05f);

            gravity.y = new ParticleSystem.MinMaxCurve(
                -0.4f, -0.15f);

            gravity.z = new ParticleSystem.MinMaxCurve(
                -0.05f, 0.05f);

            var color = ps.colorOverLifetime;
            color.enabled = true;

            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(1f, 0.9f, 0.3f), 0f),

                    new GradientColorKey(
                        new Color(1f, 0.2f, 0f), 0.5f),

                    new GradientColorKey(
                        new Color(0.2f, 0.02f, 0f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });

            color.color =
                new ParticleSystem.MinMaxGradient(
                    gradient);

            return ps;
        }

        // =====================================================
        // FAILURE SPARKS
        // =====================================================

        private ParticleSystem CreateSparkSystem()
        {
            ParticleSystem ps = CreateParticleSystem(
                "FailureSparks",
                150,
                0.5f,
                1.5f,
                0.012f,
                new Color(1f, 0.6f, 0.1f, 1f));

            var main = ps.main;
            main.loop = false;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.2f, 0.7f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    0.7f, 2.2f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.004f, 0.014f);

            main.gravityModifier = 0.65f;

            var shape = ps.shape;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius = 0.015f;

            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.65f;
            trails.lifetime = 0.12f;
            trails.minVertexDistance = 0.01f;

            ps.Stop(true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear);

            return ps;
        }

        // =====================================================
        // BURNOUT FLASH
        // =====================================================

        private ParticleSystem CreateFlashSystem()
        {
            ParticleSystem ps = CreateParticleSystem(
                "BurnoutFlash",
                10,
                0.12f,
                0f,
                0.12f,
                new Color(1f, 0.45f, 0.05f, 0.9f));

            var main = ps.main;
            main.loop = false;
            main.startLifetime = 0.12f;
            main.startSpeed = 0f;
            main.startSize = 0.12f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;

            size.size =
                new ParticleSystem.MinMaxCurve(
                    1f,
                    new AnimationCurve(
                        new Keyframe(0f, 0.3f),
                        new Keyframe(0.3f, 1.5f),
                        new Keyframe(1f, 0f)));

            ps.Stop(true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear);

            return ps;
        }

        // =====================================================
        // VFX CONTROL
        // =====================================================

        private void UpdateRuntimeVfx(
            float heat,
            float smokeIntensity)
        {
            if (smokeParticles != null)
            {
                var emission = smokeParticles.emission;

                emission.rateOverTime =
                    smokeIntensity * maximumSmokeRate;
            }

            if (emberParticles != null)
            {
                var emission = emberParticles.emission;

                emission.rateOverTime =
                    IsFailed
                        ? 0f
                        : Mathf.Clamp01(
                            (heat - 0.4f) / 0.6f) *
                          maximumEmberRate;
            }
        }

        private void TriggerRuntimeBurnBurst()
        {
            if (sparkParticles != null)
            {
                sparkParticles.Emit(failureSparkCount);
            }

            if (smokeParticles != null)
            {
                smokeParticles.Emit(failureSmokeCount);
            }

            if (flashParticles != null)
            {
                flashParticles.Emit(1);
            }
        }

        // =====================================================
        // SHADER / LIGHT / GPU VFX
        // =====================================================

        private void UpdateVisuals()
        {
            float heat =
                Mathf.InverseLerp(
                    hotC, failureC, TemperatureC);

            float charring =
                IsFailed
                    ? 1f
                    : Mathf.InverseLerp(
                        smokeC, failureC,
                        TemperatureC) * 0.55f;

            if (heatedRenderers != null)
            {
                foreach (Renderer renderer
                    in heatedRenderers)
                {
                    if (renderer == null)
                        continue;

                    renderer.GetPropertyBlock(
                        propertyBlock);

                    propertyBlock.SetFloat(
                        HeatId, heat);

                    propertyBlock.SetFloat(
                        CharId, charring);

                    renderer.SetPropertyBlock(
                        propertyBlock);
                }
            }

            float smokeIntensity =
                IsFailed
                    ? 0.15f
                    : Mathf.InverseLerp(
                        smokeC, failureC,
                        TemperatureC);

            UpdateRuntimeVfx(
                heat, smokeIntensity);

            if (smokeVfx != null)
            {
                if (smokeVfx.HasFloat(
                    smokeRateProperty))
                {
                    smokeVfx.SetFloat(
                        smokeRateProperty,
                        smokeIntensity * 35f);
                }

                if (smokeVfx.HasFloat(
                    heatProperty))
                {
                    smokeVfx.SetFloat(
                        heatProperty, heat);
                }
            }

            if (heatLight != null)
            {
                heatLight.intensity =
                    IsFailed
                        ? 0f
                        : heat *
                          maximumHeatLightIntensity;

                heatLight.enabled =
                    heatLight.intensity > 0.01f;
            }
        }

        // =====================================================
        // DEBUG
        // =====================================================

        [ContextMenu("Debug / Force Burn")]
        private void DebugForceBurn()
        {
            if (IsFailed)
                return;

            TemperatureC = failureC;
            Fail();
            UpdateVisuals();
        }

        [ContextMenu("Debug / Test Smoke")]
        private void DebugTestSmoke()
        {
            if (smokeParticles != null)
                smokeParticles.Emit(40);
        }

        [ContextMenu("Debug / Test Sparks")]
        private void DebugTestSparks()
        {
            if (sparkParticles != null)
                sparkParticles.Emit(35);
        }

        [ContextMenu("Debug / Reset Burn")]
        public void ResetBurn()
        {
            IsFailed = false;
            electricallyEnergized = false;
            PowerWatts = 0f;
            exposureSeconds = 0f;
            TemperatureC = ambientC;
            State = BurnState.Normal;

            if (smokeParticles != null)
                smokeParticles.Clear();

            if (emberParticles != null)
                emberParticles.Clear();

            if (sparkParticles != null)
                sparkParticles.Clear();

            if (flashParticles != null)
                flashParticles.Clear();

            UpdateVisuals();
        }

        private static bool IsFinite(float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }

        private void OnValidate()
        {
            ratedWatts = Mathf.Max(0.001f, ratedWatts);
            thermalCapacityJPerC =
                Mathf.Max(0.001f, thermalCapacityJPerC);

            coolingWattsPerC =
                Mathf.Max(0.001f, coolingWattsPerC);

            hotC = Mathf.Max(ambientC + 1f, hotC);
            smokeC = Mathf.Max(hotC + 1f, smokeC);
            failureC = Mathf.Max(smokeC + 1f, failureC);
        }
    }
}
