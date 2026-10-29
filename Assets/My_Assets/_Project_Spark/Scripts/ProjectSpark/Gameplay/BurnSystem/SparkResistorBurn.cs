using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Resistor thermal failure, progressive material charring, and Inspector-assigned VFX.
    /// Fire is driven by a VFX Graph exposing FireRate and Heat, not a ParticleSystem.
    /// Preserves SparkResistor integration; creates no runtime GameObjects or Materials.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkResistorBurn : MonoBehaviour
    {
        public enum FailureMode { OpenCircuit, ShortCircuit }
        public enum BurnState { Normal, Hot, Smoking, Failed }

        [Header("Burned Mesh Material")]
        [SerializeField] private Material burnedMaterial;
        [SerializeField] private bool swapMaterialOnFailure = true;
        [SerializeField] private Color finalBurnColor = new Color(0.045f, 0.035f, 0.032f, 1f);
        [SerializeField, Min(0.1f)] private float meshBurnDuration = 3f;
        [SerializeField, Range(0f, 1f)] private float initialBurnAmount = 0.1f;
        [SerializeField] private AnimationCurve meshBurnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public float MeshBurnProgress { get; private set; }

        [Header("Thermal Model")]
        [SerializeField, Min(0.001f)] private float ratedWatts = 0.25f;
        [SerializeField, Min(0.0001f)] private float thermalCapacityJPerC = 0.06f;
        [SerializeField, Min(0.00001f)] private float coolingWattsPerC = 0.002f;
        [SerializeField] private float ambientC = 25f;
        [SerializeField] private float hotC = 85f;
        [SerializeField] private float smokeC = 120f;
        [SerializeField] private float flameC = 170f;
        [SerializeField] private float failureC = 225f;
        [SerializeField, Min(0f)] private float failureExposureSeconds = 0.3f;
        [SerializeField] private FailureMode failureMode = FailureMode.OpenCircuit;

        [Header("Resistor Body Mesh Renderers")]
        [Tooltip("Empty = auto-discover MeshRenderer/SkinnedMeshRenderer children. Excludes particle renderers.")]
        [SerializeField] private Renderer[] heatedRenderers;
        [SerializeField] private Color hotTint = new Color(1f, 0.35f, 0.06f, 1f);
        [SerializeField] private Color charTint = new Color(0.075f, 0.055f, 0.045f, 1f);
        [SerializeField, Range(0f, 1f)] private float charAtFailure = 1f;
        [SerializeField] private Light heatLight;
        [SerializeField, Min(0f)] private float maximumHeatLightIntensity = 1.5f;

        [Header("Flame VFX Graph - VFX_ResistorFlame")]
        [SerializeField] private VisualEffect flameVfx;
        [Tooltip("Exposed float controlling Constant Spawn Rate in the flame graph.")]
        [SerializeField] private string flameRateProperty = "FireRate";
        [Tooltip("Exposed float for flame intensity/appearance, if used by the graph.")]
        [SerializeField] private string flameHeatProperty = "Heat";
        [SerializeField, Min(0f)] private float maximumFlameRate = 32f;
        [Tooltip("Optional multiplier for the Heat value sent to the flame graph.")]
        [SerializeField, Min(0f)] private float maximumFlameHeat = 1f;

        [Header("Prebuilt Particle Systems - No Flame ParticleSystem")]
        [SerializeField] private ParticleSystem smokeParticles;
        [SerializeField] private ParticleSystem emberParticles;
        [SerializeField] private ParticleSystem sparkParticles;
        [SerializeField] private ParticleSystem flashParticles;
        [SerializeField] private ParticleSystem debrisParticles;
        [SerializeField, Min(0f)] private float maximumSmokeRate = 30f;
        [SerializeField, Min(0f)] private float maximumEmberRate = 8f;
        [SerializeField, Range(1, 200)] private int failureSparkCount = 45;
        [SerializeField, Range(1, 200)] private int failureSmokeCount = 24;
        [SerializeField, Range(1, 100)] private int failureDebrisCount = 14;

        [Header("Optional Smoke / Failure VFX Graph")]
        [SerializeField] private VisualEffect smokeVfx;
        [SerializeField] private VisualEffect failureVfx;
        [SerializeField] private string smokeRateProperty = "SmokeRate";
        [SerializeField] private string heatProperty = "Heat";
        [SerializeField] private string burstEventName = "OnBurn";
        [SerializeField, Min(0f)] private float maximumGraphSmokeRate = 48f;

        [Header("Failure Timing")]
        [SerializeField, Min(0.01f)] private float failureFlameSeconds = 1.3f;
        [SerializeField, Min(0.01f)] private float failureSmokeSeconds = 4f;
        [SerializeField] private Light failureFlashLight;
        [SerializeField, Min(0f)] private float failureFlashIntensity = 7f;
        [SerializeField, Min(0.01f)] private float failureFlashSeconds = 0.17f;
        [SerializeField] private AudioSource failureAudio;
        [SerializeField] private UnityEvent onFailed;

        public float TemperatureC { get; private set; }
        public float PowerWatts { get; private set; }
        public bool IsFailed { get; private set; }
        public BurnState State { get; private set; }
        public float RatedWatts => ratedWatts;
        public FailureMode ElectricalFailureMode => failureMode;
        public event Action<SparkResistorBurn> Failed;

        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int CharId = Shader.PropertyToID("_Char");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock block;
        private Material[][] originalMaterials;
        private Color[][] originalSlotColors;
        private bool[][] supportsBaseColor;
        private bool[][] supportsLegacyColor;
        private bool[][] supportsEmission;
        private Color burnedStartColor;
        private bool burnedHasBaseColor;
        private bool burnedHasLegacyColor;
        private bool burnMaterialApplied;
        private bool energized;
        private float overTemperatureDuration;
        private float failureElapsed;
        private bool previewing;
        private float previewElapsed;
        private float previewUntil;
        private bool initialized;
        private bool warnedFlameRate;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            DiscoverBodyRenderers();
            CacheOriginalMaterials();
            CacheBurnedMaterialProperties();
            TemperatureC = ambientC;
            State = BurnState.Normal;
            initialized = true;

            StopEmission(smokeParticles, true);
            StopEmission(emberParticles, true);
            StopEmission(sparkParticles, true);
            StopEmission(flashParticles, true);
            StopEmission(debrisParticles, true);
            if (heatLight != null) heatLight.enabled = false;
            if (failureFlashLight != null) failureFlashLight.enabled = false;
            SetFlameVfx(0f, 0f);
            SetGraphSmoke(0f);
            RefreshVisuals();
        }

        private void OnEnable()
        {
            if (initialized) RefreshVisuals();
        }

        private void OnDisable()
        {
            StopEmission(smokeParticles, false);
            StopEmission(emberParticles, false);
            SetFlameVfx(0f, 0f);
            SetGraphSmoke(0f);
            if (heatLight != null) heatLight.enabled = false;
            if (failureFlashLight != null) failureFlashLight.enabled = false;
        }

        public void SetElectricalState(float voltageRms, float currentRms, bool isEnergized = true)
        {
            if (IsFailed || !isEnergized || !Finite(voltageRms) || !Finite(currentRms))
            {
                SetDeenergized();
                return;
            }
            energized = true;
            PowerWatts = Mathf.Abs(voltageRms * currentRms);
        }

        public void SetDissipatedPower(float watts)
        {
            if (IsFailed) { SetDeenergized(); return; }
            energized = true;
            PowerWatts = Finite(watts) ? Mathf.Max(0f, watts) : 0f;
        }

        public void SetDeenergized()
        {
            energized = false;
            PowerWatts = 0f;
        }

        private void Update()
        {
            float remaining = Mathf.Min(Time.deltaTime, 0.25f);
            while (remaining > 0f)
            {
                float dt = Mathf.Min(remaining, 0.02f);
                remaining -= dt;
                float heating = IsFailed || !energized ? 0f : PowerWatts;
                float cooling = coolingWattsPerC * (TemperatureC - ambientC);
                TemperatureC = Mathf.Max(ambientC,
                    TemperatureC + (heating - cooling) * dt / thermalCapacityJPerC);
                if (!IsFailed)
                {
                    overTemperatureDuration = TemperatureC >= failureC
                        ? overTemperatureDuration + dt : 0f;
                    if (TemperatureC >= failureC &&
                        overTemperatureDuration >= failureExposureSeconds)
                        Fail();
                }
            }

            if (IsFailed) failureElapsed += Time.deltaTime;
            UpdateProgressiveMeshBurn();
            if (previewing)
            {
                previewElapsed += Time.deltaTime;
                if (previewElapsed >= previewUntil) previewing = false;
            }
            State = IsFailed ? BurnState.Failed :
                TemperatureC >= smokeC ? BurnState.Smoking :
                TemperatureC >= hotC ? BurnState.Hot : BurnState.Normal;
            RefreshVisuals();
        }

        private void Fail()
        {
            if (IsFailed) return;
            IsFailed = true;
            previewing = false;
            State = BurnState.Failed;
            energized = false;
            PowerWatts = 0f;
            failureElapsed = 0f;
            MeshBurnProgress = Mathf.Clamp01(initialBurnAmount);
            ApplyBurnedMaterial();
            TriggerBurst();
            RefreshVisuals();
            onFailed?.Invoke();
            Failed?.Invoke(this); // SparkResistor requests electrical re-evaluation.
        }

        private void UpdateProgressiveMeshBurn()
        {
            if (!IsFailed) { MeshBurnProgress = 0f; return; }
            float t = Mathf.Clamp01(failureElapsed / Mathf.Max(0.1f, meshBurnDuration));
            float shaped = meshBurnCurve != null ? meshBurnCurve.Evaluate(t) : t;
            MeshBurnProgress = Mathf.Lerp(initialBurnAmount, charAtFailure, Mathf.Clamp01(shaped));
        }

        private void TriggerBurst()
        {
            Emit(sparkParticles, failureSparkCount);
            Emit(flashParticles, 1);
            Emit(debrisParticles, failureDebrisCount);
            Emit(smokeParticles, failureSmokeCount);
            if (failureVfx != null && !string.IsNullOrEmpty(burstEventName))
                failureVfx.SendEvent(burstEventName);
            if (failureAudio != null) failureAudio.Play();
        }

        private void DiscoverBodyRenderers()
        {
            if (heatedRenderers != null && heatedRenderers.Length > 0) return;
            Renderer[] found = GetComponentsInChildren<Renderer>(true);
            var meshes = new List<Renderer>();
            foreach (Renderer r in found)
                if (r is MeshRenderer || r is SkinnedMeshRenderer)
                    meshes.Add(r);
            heatedRenderers = meshes.ToArray();
        }

        private void CacheOriginalMaterials()
        {
            if (originalMaterials != null) return;
            int count = heatedRenderers != null ? heatedRenderers.Length : 0;
            originalMaterials = new Material[count][];
            originalSlotColors = new Color[count][];
            supportsBaseColor = new bool[count][];
            supportsLegacyColor = new bool[count][];
            supportsEmission = new bool[count][];
            for (int i = 0; i < count; i++)
            {
                Renderer renderer = heatedRenderers[i];
                if (renderer == null ||
                    (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)))
                    continue;
                Material[] materials = renderer.sharedMaterials;
                originalMaterials[i] = (Material[])materials.Clone();
                originalSlotColors[i] = new Color[materials.Length];
                supportsBaseColor[i] = new bool[materials.Length];
                supportsLegacyColor[i] = new bool[materials.Length];
                supportsEmission[i] = new bool[materials.Length];
                for (int j = 0; j < materials.Length; j++)
                {
                    Material mat = materials[j];
                    if (mat == null) { originalSlotColors[i][j] = Color.white; continue; }
                    supportsBaseColor[i][j] = mat.HasProperty(BaseColorId);
                    supportsLegacyColor[i][j] = mat.HasProperty(ColorId);
                    supportsEmission[i][j] = mat.HasProperty(EmissionColorId);
                    originalSlotColors[i][j] = supportsBaseColor[i][j]
                        ? mat.GetColor(BaseColorId)
                        : supportsLegacyColor[i][j] ? mat.GetColor(ColorId) : Color.white;
                }
            }
        }

        private void CacheBurnedMaterialProperties()
        {
            burnedHasBaseColor = burnedMaterial != null && burnedMaterial.HasProperty(BaseColorId);
            burnedHasLegacyColor = burnedMaterial != null && burnedMaterial.HasProperty(ColorId);
            burnedStartColor = burnedHasBaseColor ? burnedMaterial.GetColor(BaseColorId) :
                burnedHasLegacyColor ? burnedMaterial.GetColor(ColorId) : Color.white;
        }

        private void ApplyBurnedMaterial()
        {
            if (burnMaterialApplied || !swapMaterialOnFailure) return;
            if (burnedMaterial == null)
            {
                Debug.LogWarning("[Burn] Burned Material is not assigned.", this);
                return;
            }
            CacheOriginalMaterials();
            CacheBurnedMaterialProperties();
            int changed = 0;
            for (int i = 0; i < heatedRenderers.Length; i++)
            {
                Renderer r = heatedRenderers[i];
                Material[] old = originalMaterials[i];
                if (r == null || old == null || old.Length == 0) continue;
                Material[] replacement = new Material[old.Length];
                for (int slot = 0; slot < replacement.Length; slot++)
                    replacement[slot] = burnedMaterial;
                r.sharedMaterials = replacement;
                r.SetPropertyBlock(null);
                for (int slot = 0; slot < replacement.Length; slot++)
                    r.SetPropertyBlock(null, slot);
                changed++;
            }
            burnMaterialApplied = changed > 0;
            if (!burnMaterialApplied)
                Debug.LogWarning("[Burn] No body mesh renderer received burned material. Check Heated Renderers.", this);
        }

        private void RestoreOriginalMaterials()
        {
            if (!burnMaterialApplied || originalMaterials == null) return;
            for (int i = 0; i < heatedRenderers.Length; i++)
            {
                Renderer r = heatedRenderers[i];
                Material[] original = originalMaterials[i];
                if (r == null || original == null) continue;
                r.sharedMaterials = (Material[])original.Clone();
                r.SetPropertyBlock(null);
                for (int slot = 0; slot < original.Length; slot++)
                    r.SetPropertyBlock(null, slot);
            }
            burnMaterialApplied = false;
        }

        private void UpdateBurnedMeshMaterial(float fadingHeat)
        {
            if (!burnMaterialApplied || heatedRenderers == null) return;
            float charring = Mathf.Clamp01(MeshBurnProgress);
            Color burntTint = Color.Lerp(burnedStartColor, finalBurnColor, charring);
            for (int i = 0; i < heatedRenderers.Length; i++)
            {
                Renderer r = heatedRenderers[i];
                if (r == null || originalMaterials[i] == null) continue;
                for (int slot = 0; slot < originalMaterials[i].Length; slot++)
                {
                    r.GetPropertyBlock(block, slot);
                    block.SetFloat(CharId, charring);
                    block.SetFloat(HeatId, fadingHeat);
                    if (burnedHasBaseColor) block.SetColor(BaseColorId, burntTint);
                    else if (burnedHasLegacyColor) block.SetColor(ColorId, burntTint);
                    r.SetPropertyBlock(block, slot);
                }
            }
        }

        private void RefreshVisuals()
        {
            if (block == null) return;
            float heat = Mathf.InverseLerp(hotC, failureC, TemperatureC);
            float charAmount = IsFailed ? MeshBurnProgress :
                Mathf.InverseLerp(smokeC, failureC, TemperatureC) * 0.7f;
            float smoke = Mathf.InverseLerp(smokeC, failureC, TemperatureC);
            float fire = Mathf.InverseLerp(flameC, failureC, TemperatureC);
            float elapsed = IsFailed ? failureElapsed : previewElapsed;
            if (IsFailed || previewing)
            {
                charAmount = IsFailed ? MeshBurnProgress : charAtFailure;
                heat = Mathf.Exp(-elapsed * 1.8f);
                fire = Mathf.Clamp01(1f - elapsed / failureFlameSeconds);
                smoke = Mathf.Clamp01(1f - elapsed / failureSmokeSeconds);
            }

            if (burnMaterialApplied)
            {
                UpdateBurnedMeshMaterial(heat);
            }
            else if (heatedRenderers != null)
            {
                for (int i = 0; i < heatedRenderers.Length; i++)
                {
                    Renderer r = heatedRenderers[i];
                    if (r == null || originalMaterials == null || originalMaterials[i] == null) continue;
                    int slots = Mathf.Min(r.sharedMaterials.Length, originalMaterials[i].Length);
                    for (int slot = 0; slot < slots; slot++)
                    {
                        r.GetPropertyBlock(block, slot);
                        block.SetFloat(HeatId, heat);
                        block.SetFloat(CharId, charAmount);
                        Color original = originalSlotColors[i][slot];
                        Color tinted = Color.Lerp(original,
                            Color.Lerp(hotTint, charTint, charAmount),
                            Mathf.Max(heat * 0.5f, charAmount));
                        if (supportsBaseColor[i][slot]) block.SetColor(BaseColorId, tinted);
                        else if (supportsLegacyColor[i][slot]) block.SetColor(ColorId, tinted);
                        if (supportsEmission[i][slot])
                            block.SetColor(EmissionColorId,
                                new Color(1f, 0.18f, 0.01f) * (heat * 1.7f));
                        r.SetPropertyBlock(block, slot);
                    }
                }
            }

            // Fire is exclusively VFX Graph. FireRate must be connected to
            // the Spawn context's Constant Spawn Rate input in VFX_ResistorFlame.
            SetFlameVfx(fire * maximumFlameRate, heat * maximumFlameHeat);
            SetRate(smokeParticles, smoke * maximumSmokeRate);
            SetRate(emberParticles, !IsFailed && !previewing
                ? Mathf.InverseLerp(hotC, failureC, TemperatureC) * maximumEmberRate : 0f);
            SetGraphSmoke(smoke * maximumGraphSmokeRate);
            if (smokeVfx != null && !string.IsNullOrEmpty(heatProperty) && smokeVfx.HasFloat(heatProperty))
                smokeVfx.SetFloat(heatProperty, heat);
            if (heatLight != null)
            {
                heatLight.intensity = heat * maximumHeatLightIntensity;
                heatLight.enabled = heatLight.intensity > 0.01f;
            }
            if (failureFlashLight != null)
            {
                float value = IsFailed || previewing
                    ? Mathf.Clamp01(1f - elapsed / failureFlashSeconds) * failureFlashIntensity : 0f;
                failureFlashLight.intensity = value;
                failureFlashLight.enabled = value > 0.01f;
            }
        }

        private void SetFlameVfx(float rate, float heat)
        {
            if (flameVfx == null) return;
            if (!string.IsNullOrEmpty(flameRateProperty) && flameVfx.HasFloat(flameRateProperty))
            {
                flameVfx.SetFloat(flameRateProperty, Mathf.Max(0f, rate));
            }
            else if (!warnedFlameRate)
            {
                Debug.LogWarning("[Burn] Flame VFX must expose float property '" + flameRateProperty +
                    "' and connect it to Spawn > Constant Spawn Rate.", flameVfx);
                warnedFlameRate = true;
            }
            if (!string.IsNullOrEmpty(flameHeatProperty) && flameVfx.HasFloat(flameHeatProperty))
                flameVfx.SetFloat(flameHeatProperty, Mathf.Max(0f, heat));
        }

        private void SetGraphSmoke(float rate)
        {
            if (smokeVfx != null && !string.IsNullOrEmpty(smokeRateProperty) &&
                smokeVfx.HasFloat(smokeRateProperty))
                smokeVfx.SetFloat(smokeRateProperty, Mathf.Max(0f, rate));
        }

        private static void SetRate(ParticleSystem ps, float rate)
        {
            if (ps == null) return;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = Mathf.Max(0f, rate);
            if (rate > 0f && !ps.isPlaying) ps.Play();
        }

        private static void StopEmission(ParticleSystem ps, bool clear)
        {
            if (ps == null) return;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            ps.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear :
                ParticleSystemStopBehavior.StopEmitting);
        }

        private static void Emit(ParticleSystem ps, int count)
        {
            if (ps == null || count <= 0) return;
            ps.Emit(count);
        }

        [ContextMenu("Debug / Force Burn (Play Mode)")]
        private void DebugForceBurn()
        {
            if (!Application.isPlaying || IsFailed) return;
            TemperatureC = failureC;
            Fail();
        }

        [ContextMenu("Debug / Preview Cinematic VFX Only (Play Mode)")]
        private void DebugPreviewOnly()
        {
            if (!Application.isPlaying || IsFailed) return;
            previewing = true;
            previewElapsed = 0f;
            previewUntil = Mathf.Max(failureFlameSeconds, failureSmokeSeconds);
            TriggerBurst();
            RefreshVisuals();
        }

        [ContextMenu("Debug / Reset Burn")]
        public void ResetBurn()
        {
            RestoreOriginalMaterials();
            IsFailed = false;
            State = BurnState.Normal;
            TemperatureC = ambientC;
            PowerWatts = 0f;
            energized = false;
            overTemperatureDuration = 0f;
            failureElapsed = 0f;
            previewElapsed = 0f;
            previewing = false;
            MeshBurnProgress = 0f;
            StopEmission(smokeParticles, true);
            StopEmission(emberParticles, true);
            StopEmission(sparkParticles, true);
            StopEmission(flashParticles, true);
            StopEmission(debrisParticles, true);
            SetFlameVfx(0f, 0f);
            SetGraphSmoke(0f);
            RefreshVisuals();
        }

        private void OnValidate()
        {
            ratedWatts = Mathf.Max(0.001f, ratedWatts);
            thermalCapacityJPerC = Mathf.Max(0.0001f, thermalCapacityJPerC);
            coolingWattsPerC = Mathf.Max(0.00001f, coolingWattsPerC);
            hotC = Mathf.Max(ambientC + 1f, hotC);
            smokeC = Mathf.Max(hotC + 1f, smokeC);
            flameC = Mathf.Max(smokeC + 1f, flameC);
            failureC = Mathf.Max(flameC + 1f, failureC);
            failureFlameSeconds = Mathf.Max(0.01f, failureFlameSeconds);
            failureSmokeSeconds = Mathf.Max(0.01f, failureSmokeSeconds);
            failureFlashSeconds = Mathf.Max(0.01f, failureFlashSeconds);
            meshBurnDuration = Mathf.Max(0.1f, meshBurnDuration);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
