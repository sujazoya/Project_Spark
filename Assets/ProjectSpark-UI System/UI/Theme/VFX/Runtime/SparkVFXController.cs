using UnityEngine;

namespace ProjectSpark.VFX
{
    /// <summary>
    /// Main runtime VFX controller for Project Spark.
    ///
    /// Designed to remain intentionally small and easy to debug.
    ///
    /// Controls:
    ///
    /// Base:
    /// - Glow
    /// - Alpha
    ///
    /// Animation:
    /// - Pulse
    /// - Scan
    /// - Sweep
    /// - Flash
    ///
    /// Surface:
    /// - Noise
    /// - Dissolve
    /// - Distortion
    ///
    /// Interaction:
    /// - Hover
    /// - Selected
    /// - Warning
    ///
    /// Utility:
    /// - Signal
    /// - Spark
    /// - Error
    ///
    /// Uses MaterialPropertyBlock.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXController
        : MonoBehaviour
    {
        // ============================================================
        // PROFILE
        // ============================================================

        [Header("Profile")]

        [SerializeField]
        private SparkVFXProfile profile;

        [SerializeField]
        private bool applyProfileOnAwake = true;


        // ============================================================
        // RENDERERS
        // ============================================================

        [Header("Renderers")]

        [SerializeField]
        private Renderer[] targetRenderers;

        [SerializeField]
        private bool autoFindRenderers = true;


        // ============================================================
        // PARTICLES
        // ============================================================

        [Header("Particles")]

        [SerializeField]
        private ParticleSystem sparkParticles;


        // ============================================================
        // TRANSITION
        // ============================================================

        [Header("Transition")]

        [Min(0f)]
        [SerializeField]
        private float transitionSpeed = 8f;

        [SerializeField]
        private bool applyInLateUpdate;


        // ============================================================
        // BASE TARGETS
        // ============================================================

        [Header("Base Targets")]

        [Min(0f)]
        [SerializeField]
        private float glowIntensity = 4f;

        [Range(0f, 1f)]
        [SerializeField]
        private float alpha = 1f;


        // ============================================================
        // STATE
        // ============================================================

        [Header("State Glow")]

        [Min(0f)]
        [SerializeField]
        private float hoverGlow = 0.2f;

        [Min(0f)]
        [SerializeField]
        private float selectedGlow = 0.45f;

        [Min(0f)]
        [SerializeField]
        private float warningGlow = 0.75f;


        // ============================================================
        // EFFECT SETTINGS
        // ============================================================

        [Header("Pulse")]

        [Min(0f)]
        [SerializeField]
        private float pulseIntensity = 3f;

        [Min(0.01f)]
        [SerializeField]
        private float pulseDuration = 0.15f;


        [Header("Flash")]

        [Min(0f)]
        [SerializeField]
        private float flashIntensity = 1f;

        [Min(0.01f)]
        [SerializeField]
        private float flashDuration = 0.12f;
       
        // ============================================================
        // SWEEP POSITION
        // ============================================================

        [Range(0f, 1f)]
        [SerializeField]
        private float sweepPosition = 0f;





        // ============================================================
        // DEBUG
        // ============================================================

        [Header("Debug")]

        [SerializeField]
        private bool debugLogs;


        // ============================================================
        // SHADER PROPERTY IDS
        // ============================================================

        private static readonly int GlowIntensityID =
            Shader.PropertyToID(
                "_GlowIntensity"
            );

        private static readonly int PulseID =
            Shader.PropertyToID(
                "_Pulse"
            );

        private static readonly int ScanEnabledID =
            Shader.PropertyToID(
                "_ScanEnabled"
            );

        private static readonly int ScanIntensityID =
            Shader.PropertyToID(
                "_ScanIntensity"
            );

        private static readonly int SweepEnabledID =
            Shader.PropertyToID(
                "_SweepEnabled"
            );

        private static readonly int SweepIntensityID =
            Shader.PropertyToID(
                "_SweepIntensity"
            );

        private static readonly int NoiseEnabledID =
            Shader.PropertyToID(
                "_NoiseEnabled"
            );

        private static readonly int NoiseIntensityID =
            Shader.PropertyToID(
                "_NoiseIntensity"
            );

        private static readonly int DissolveEnabledID =
            Shader.PropertyToID(
                "_DissolveEnabled"
            );

        private static readonly int DissolveAmountID =
            Shader.PropertyToID(
                "_DissolveAmount"
            );

        private static readonly int DistortionEnabledID =
            Shader.PropertyToID(
                "_DistortionEnabled"
            );

        private static readonly int DistortionStrengthID =
            Shader.PropertyToID(
                "_DistortionStrength"
            );

        private static readonly int AlphaID =
            Shader.PropertyToID(
                "_Alpha"
            );

        private static readonly int FlashID =
            Shader.PropertyToID(
                "_Flash"
            );

        private static readonly int SweepPositionID =
    Shader.PropertyToID(
        "_SweepPosition"
    );


        // ============================================================
        // PROPERTY BLOCK
        // ============================================================

        private MaterialPropertyBlock propertyBlock;


        // ============================================================
        // CURRENT VALUES
        // ============================================================

        private float currentGlow;

        private float currentPulse;

        private float currentScanIntensity;

        private float currentSweepIntensity;

        private float currentSweepPosition;

        private float currentNoiseIntensity;

        private float currentDissolveAmount;

        private float currentDistortionStrength;

        private float currentAlpha;

        private float currentFlash;


        // ============================================================
        // TARGET VALUES
        // ============================================================

        private float targetGlow;

        private float targetScanIntensity;

        private float targetSweepIntensity;

        private float targetSweepPosition;

        private float targetNoiseIntensity;

        private float targetDissolveAmount;

        private float targetDistortionStrength;

        private float targetAlpha;


        // ============================================================
        // STATE
        // ============================================================

        private bool isHovered;

        private bool isSelected;

        private bool isWarning;


        // ============================================================
        // EFFECT STATES
        // ============================================================

        private bool scanEnabled;

        private bool sweepEnabled;

        private bool noiseEnabled;

        private bool dissolveEnabled;

        private bool distortionEnabled;


        // ============================================================
        // TIMERS
        // ============================================================

        private float pulseTimer;

        private float flashTimer;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        private bool initialized;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }
        }


        private void Update()
        {
            if (applyInLateUpdate)
            {
                return;
            }

            Tick(
                Time.deltaTime
            );
        }


        private void LateUpdate()
        {
            if (!applyInLateUpdate)
            {
                return;
            }

            Tick(
                Time.deltaTime
            );
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            propertyBlock =
                new MaterialPropertyBlock();

            ResolveRenderers();

            if (applyProfileOnAwake)
            {
                ApplyProfile();
            }

            CopyTargetsToCurrent();

            initialized = true;

            ApplyToRenderers();
        }


        // ============================================================
        // PROFILE
        // ============================================================

        public void ApplyProfile()
        {
            if (profile == null)
            {
                return;
            }

            glowIntensity =
                Mathf.Max(
                    0f,
                    profile.GlowIntensity
                );

            alpha =
                Mathf.Clamp01(
                    profile.Alpha
                );

            pulseIntensity =
                Mathf.Max(
                    0f,
                    profile.PulseIntensity
                );

            pulseDuration =
                Mathf.Max(
                    0.01f,
                    profile.PulseDuration
                );

            scanEnabled =
                profile.ScanEnabled;

            targetScanIntensity =
                Mathf.Max(
                    0f,
                    profile.ScanIntensity
                );

            sweepEnabled =
                profile.SweepEnabled;

            targetSweepIntensity =
                Mathf.Max(
                    0f,
                    profile.SweepIntensity
                );

            noiseEnabled =
                profile.NoiseEnabled;

            targetNoiseIntensity =
                Mathf.Max(
                    0f,
                    profile.NoiseIntensity
                );

            dissolveEnabled =
                profile.DissolveEnabled;

            targetDissolveAmount =
                Mathf.Clamp01(
                    profile.DissolveAmount
                );

            distortionEnabled =
                profile.DistortionEnabled;

            targetDistortionStrength =
                Mathf.Max(
                    0f,
                    profile.DistortionStrength
                );

            hoverGlow =
                Mathf.Max(
                    0f,
                    profile.HoverGlow
                );

            selectedGlow =
                Mathf.Max(
                    0f,
                    profile.SelectedGlow
                );

            warningGlow =
                Mathf.Max(
                    0f,
                    profile.WarningGlow
                );

            transitionSpeed =
                Mathf.Max(
                    0f,
                    profile.TransitionSpeed
                );

            flashIntensity =
                Mathf.Max(
                    0f,
                    profile.FlashIntensity
                );

            flashDuration =
                Mathf.Max(
                    0.01f,
                    profile.FlashDuration
                );
            targetSweepPosition =
            Mathf.Clamp01(profile.SweepIntensity);
        }


        // ============================================================
        // RENDERERS
        // ============================================================

        private void ResolveRenderers()
        {
            if (!autoFindRenderers)
            {
                return;
            }

            if (
                targetRenderers != null &&
                targetRenderers.Length > 0
            )
            {
                return;
            }

            targetRenderers =
                GetComponentsInChildren<Renderer>(
                    true
                );
        }


        public void RefreshRenderers()
        {
            targetRenderers =
                GetComponentsInChildren<Renderer>(
                    true
                );

            ApplyToRenderers();
        }



        /// <summary>
        /// Sets the horizontal position of the sweep effect.
        ///
        /// 0 = left side
        /// 0.5 = center
        /// 1 = right side
        ///
        /// This is intended for animated scan sequences.
        /// </summary>
        public void SetSweepPosition(float position)
        {
            targetSweepPosition =
                Mathf.Clamp01(position);
        }



        // ============================================================
        // TICK
        // ============================================================

        private void Tick(
            float deltaTime)
        {
            UpdatePulse(
                deltaTime
            );

            UpdateFlash(
                deltaTime
            );

            float factor =
                GetTransitionFactor(
                    deltaTime
                );

            currentGlow =
                Mathf.Lerp(
                    currentGlow,
                    GetStateGlow(),
                    factor
                );

            currentScanIntensity =
                Mathf.Lerp(
                    currentScanIntensity,
                    targetScanIntensity,
                    factor
                );

            currentSweepIntensity =
                Mathf.Lerp(
                    currentSweepIntensity,
                    targetSweepIntensity,
                    factor
                        );
                    currentSweepPosition =
            Mathf.Lerp(
                currentSweepPosition,
                targetSweepPosition,
                factor
            );
            currentSweepPosition =
             targetSweepPosition;

            currentNoiseIntensity =
                Mathf.Lerp(
                    currentNoiseIntensity,
                    targetNoiseIntensity,
                    factor
                );

            currentDissolveAmount =
                Mathf.Lerp(
                    currentDissolveAmount,
                    targetDissolveAmount,
                    factor
                );

            currentDistortionStrength =
                Mathf.Lerp(
                    currentDistortionStrength,
                    targetDistortionStrength,
                    factor
                );

            currentAlpha =
                Mathf.Lerp(
                    currentAlpha,
                    targetAlpha,
                    factor
                );

            ApplyToRenderers();
        }


        private float GetTransitionFactor(
            float deltaTime)
        {
            if (transitionSpeed <= 0f)
            {
                return 1f;
            }

            return 1f -
                   Mathf.Exp(
                       -transitionSpeed *
                       deltaTime
                   );
        }


        // ============================================================
        // STATE
        // ============================================================

        private float GetStateGlow()
        {
            float stateGlow =
                glowIntensity;

            if (isHovered)
            {
                stateGlow +=
                    hoverGlow;
            }

            if (isSelected)
            {
                stateGlow +=
                    selectedGlow;
            }

            if (isWarning)
            {
                stateGlow +=
                    warningGlow;
            }

            return stateGlow;
        }


        // ============================================================
        // GLOW
        // ============================================================

        public void SetGlow(
            float value)
        {
            glowIntensity =
                Mathf.Max(
                    0f,
                    value
                );
        }


        public void SetPower(
            float value)
        {
            SetGlow(
                value
            );
        }


        // ============================================================
        // SCAN
        // ============================================================

        public void SetScan(
            float intensity)
        {
            targetScanIntensity =
                Mathf.Max(
                    0f,
                    intensity
                );
        }


        public void SetScanEnabled(
            bool enabled)
        {
            scanEnabled =
                enabled;
        }


        // ============================================================
        // SWEEP
        // ============================================================

        public void SetSweep(
            float intensity)
        {
            targetSweepIntensity =
                Mathf.Max(
                    0f,
                    intensity
                );
        }


        public void SetSweepEnabled(
            bool enabled)
        {
            sweepEnabled =
                enabled;
        }      


        // ============================================================
        // NOISE
        // ============================================================

        public void SetNoise(
            float intensity)
        {
            targetNoiseIntensity =
                Mathf.Max(
                    0f,
                    intensity
                );
        }


        public void SetNoiseEnabled(
            bool enabled)
        {
            noiseEnabled =
                enabled;
        }


        // ============================================================
        // DISSOLVE
        // ============================================================

        public void SetDissolve(
            float amount)
        {
            targetDissolveAmount =
                Mathf.Clamp01(
                    amount
                );
        }


        public void SetDissolveEnabled(
            bool enabled)
        {
            dissolveEnabled =
                enabled;
        }


        // ============================================================
        // DISTORTION
        // ============================================================

        public void SetDistortion(
            float strength)
        {
            targetDistortionStrength =
                Mathf.Max(
                    0f,
                    strength
                );
        }


        public void SetDistortionEnabled(
            bool enabled)
        {
            distortionEnabled =
                enabled;
        }


        // ============================================================
        // ALPHA
        // ============================================================

        public void SetAlpha(
            float value)
        {
            targetAlpha =
                Mathf.Clamp01(
                    value
                );
        }


        // ============================================================
        // SIGNAL
        // ============================================================

        public void SetSignal(
            float value)
        {
            value =
                Mathf.Clamp01(
                    value
                );

            SetScan(
                value *
                4f
            );

            SetSweep(
                value *
                5f
            );

            SetNoise(
                value *
                2f
            );
        }


        // ============================================================
        // HOVER
        // ============================================================

        public void SetHovered(
            bool value)
        {
            isHovered =
                value;
        }


        // ============================================================
        // SELECTED
        // ============================================================

        public void SetSelected(
            bool value)
        {
            isSelected =
                value;
        }


        // ============================================================
        // WARNING
        // ============================================================

        public void SetWarning(
            bool value)
        {
            isWarning =
                value;
        }


        // ============================================================
        // PULSE
        // ============================================================

        public void PlayPulse()
        {
            pulseTimer =
                pulseDuration;

            currentPulse =
                1f;
        }


        private void UpdatePulse(
            float deltaTime)
        {
            if (pulseTimer <= 0f)
            {
                currentPulse =
                    Mathf.MoveTowards(
                        currentPulse,
                        0f,
                        deltaTime /
                        Mathf.Max(
                            0.01f,
                            pulseDuration
                        )
                    );

                return;
            }

            pulseTimer -=
                deltaTime;

            currentPulse =
                Mathf.Clamp01(
                    pulseTimer /
                    pulseDuration
                );
        }


        // ============================================================
        // FLASH
        // ============================================================

        public void PlayFlash()
        {
            flashTimer =
                flashDuration;

            currentFlash =
                flashIntensity;
        }


        private void UpdateFlash(
            float deltaTime)
        {
            if (flashTimer <= 0f)
            {
                currentFlash =
                    Mathf.MoveTowards(
                        currentFlash,
                        0f,
                        deltaTime /
                        Mathf.Max(
                            0.01f,
                            flashDuration
                        )
                    );

                return;
            }

            flashTimer -=
                deltaTime;

            currentFlash =
                flashIntensity *
                Mathf.Clamp01(
                    flashTimer /
                    flashDuration
                );
        }


        // ============================================================
        // SPARK
        // ============================================================

        public void PlaySpark()
        {
            if (sparkParticles == null)
            {
                return;
            }

            sparkParticles.Play(
                true
            );
        }


        // ============================================================
        // ERROR
        // ============================================================

        public void PlayError()
        {
            SetWarning(
                true
            );

            PlayPulse();

            PlayFlash();

            PlaySpark();
        }


        // ============================================================
        // APPLY
        // ============================================================

        private void ApplyToRenderers()
        {
            if (
                targetRenderers == null ||
                propertyBlock == null
            )
            {
                return;
            }

            for (
                int i = 0;
                i < targetRenderers.Length;
                i++
            )
            {
                Renderer renderer =
                    targetRenderers[i];

                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(
                    propertyBlock
                );

                propertyBlock.SetFloat(
                    GlowIntensityID,
                    currentGlow
                );

                propertyBlock.SetFloat(
                    PulseID,
                    currentPulse
                );

                propertyBlock.SetFloat(
                    ScanEnabledID,
                    scanEnabled
                        ? 1f
                        : 0f
                );

                propertyBlock.SetFloat(
                    ScanIntensityID,
                    currentScanIntensity
                );

                propertyBlock.SetFloat(
                    SweepEnabledID,
                    sweepEnabled
                        ? 1f
                        : 0f
                );

                propertyBlock.SetFloat(
                    SweepIntensityID,
                    currentSweepIntensity
                );
                    propertyBlock.SetFloat(
                    SweepPositionID,
                    currentSweepPosition
                );

                propertyBlock.SetFloat(
                    NoiseEnabledID,
                    noiseEnabled
                        ? 1f
                        : 0f
                );

                propertyBlock.SetFloat(
                    NoiseIntensityID,
                    currentNoiseIntensity
                );

                propertyBlock.SetFloat(
                    DissolveEnabledID,
                    dissolveEnabled
                        ? 1f
                        : 0f
                );

                propertyBlock.SetFloat(
                    DissolveAmountID,
                    currentDissolveAmount
                );

                propertyBlock.SetFloat(
                    DistortionEnabledID,
                    distortionEnabled
                        ? 1f
                        : 0f
                );

                propertyBlock.SetFloat(
                    DistortionStrengthID,
                    currentDistortionStrength
                );

                propertyBlock.SetFloat(
                    AlphaID,
                    currentAlpha
                );

                propertyBlock.SetFloat(
                    FlashID,
                    currentFlash
                );

                renderer.SetPropertyBlock(
                    propertyBlock
                );
            }
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetVFX()
        {
            glowIntensity =
                0f;

            targetScanIntensity =
                0f;

            targetSweepIntensity =
                0f;

            targetNoiseIntensity =
                0f;

            targetDissolveAmount =
                0f;

            targetDistortionStrength =
                0f;
            targetSweepPosition = 0f;
            currentSweepPosition = 0f;


            targetAlpha =
                1f;

            currentPulse =
                0f;

            currentFlash =
                0f;

            pulseTimer =
                0f;

            flashTimer =
                0f;

            isHovered =
                false;

            isSelected =
                false;

            isWarning =
                false;

            scanEnabled =
                false;

            sweepEnabled =
                false;

            noiseEnabled =
                false;

            dissolveEnabled =
                false;

            distortionEnabled =
                false;

            CopyTargetsToCurrent();

            ApplyToRenderers();
        }


        private void CopyTargetsToCurrent()
        {
            currentGlow =
                glowIntensity;

            currentScanIntensity =
                targetScanIntensity;

            currentSweepIntensity =
                targetSweepIntensity;

            currentNoiseIntensity =
                targetNoiseIntensity;

            currentDissolveAmount =
                targetDissolveAmount;

            currentDistortionStrength =
                targetDistortionStrength;

            currentAlpha =
                alpha;

            targetAlpha =
                alpha;
        }


        // ============================================================
        // PUBLIC READ-ONLY VALUES
        // ============================================================

        public float CurrentGlow
        {
            get { return currentGlow; }
        }

        public float CurrentPulse
        {
            get { return currentPulse; }
        }

        public float CurrentScan
        {
            get { return currentScanIntensity; }
        }

        public float CurrentSweep
        {
            get { return currentSweepIntensity; }
        }

        public float CurrentNoise
        {
            get { return currentNoiseIntensity; }
        }

        public float CurrentDissolve
        {
            get { return currentDissolveAmount; }
        }

        public float CurrentFlash
        {
            get { return currentFlash; }
        }

        public bool IsHovered
        {
            get { return isHovered; }
        }

        public bool IsSelected
        {
            get { return isSelected; }
        }

        public bool IsWarning
        {
            get { return isWarning; }
        }
    }
}