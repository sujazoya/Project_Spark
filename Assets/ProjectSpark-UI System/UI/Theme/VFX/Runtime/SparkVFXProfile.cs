using UnityEngine;

namespace ProjectSpark.VFX
{
    /// <summary>
    /// Reusable configuration asset for Project Spark VFX.
    ///
    /// This class contains only authoring data.
    /// Runtime animation is handled by SparkVFXController.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkVFXProfile",
        menuName = "Project Spark/VFX/VFX Profile"
    )]
    public sealed class SparkVFXProfile : ScriptableObject
    {
        // ============================================================
        // PROFILE
        // ============================================================

        [Header("Profile")]

        [TextArea(2, 4)]
        [SerializeField]
        private string description;


        // ============================================================
        // BASE
        // ============================================================

        [Header("Base")]

        [Min(0f)]
        [SerializeField]
        private float glowIntensity = 4f;

        [Range(0f, 1f)]
        [SerializeField]
        private float alpha = 1f;


        // ============================================================
        // PULSE
        // ============================================================

        [Header("Pulse")]

        [Min(0f)]
        [SerializeField]
        private float pulseIntensity = 3f;

        [Min(0.01f)]
        [SerializeField]
        private float pulseDuration = 0.15f;


        // ============================================================
        // SCAN
        // ============================================================

        [Header("Scan")]

        [SerializeField]
        private bool scanEnabled = true;

        [Min(0f)]
        [SerializeField]
        private float scanIntensity = 4f;


        // ============================================================
        // SWEEP
        // ============================================================

        [Header("Sweep")]

        [SerializeField]
        private bool sweepEnabled = true;

        [Min(0f)]
        [SerializeField]
        private float sweepIntensity = 5f;


        // ============================================================
        // NOISE
        // ============================================================

        [Header("Noise")]

        [SerializeField]
        private bool noiseEnabled;

        [Min(0f)]
        [SerializeField]
        private float noiseIntensity = 2f;


        // ============================================================
        // DISSOLVE
        // ============================================================

        [Header("Dissolve")]

        [SerializeField]
        private bool dissolveEnabled;

        [Range(0f, 1f)]
        [SerializeField]
        private float dissolveAmount;


        // ============================================================
        // DISTORTION
        // ============================================================

        [Header("Distortion")]

        [SerializeField]
        private bool distortionEnabled;

        [Min(0f)]
        [SerializeField]
        private float distortionStrength = 0.02f;


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
        // TRANSITION
        // ============================================================

        [Header("Transition")]

        [Min(0f)]
        [SerializeField]
        private float transitionSpeed = 8f;


        // ============================================================
        // FLASH
        // ============================================================

        [Header("Flash")]

        [Min(0f)]
        [SerializeField]
        private float flashIntensity = 1f;

        [Min(0.01f)]
        [SerializeField]
        private float flashDuration = 0.12f;


        // ============================================================
        // PARTICLES
        // ============================================================

        [Header("Particles")]

        [SerializeField]
        private bool allowParticles = true;


        // ============================================================
        // PUBLIC ACCESS
        // ============================================================

        public float GlowIntensity
        {
            get { return glowIntensity; }
        }

        public float Alpha
        {
            get { return alpha; }
        }

        public float PulseIntensity
        {
            get { return pulseIntensity; }
        }

        public float PulseDuration
        {
            get { return pulseDuration; }
        }

        public bool ScanEnabled
        {
            get { return scanEnabled; }
        }

        public float ScanIntensity
        {
            get { return scanIntensity; }
        }

        public bool SweepEnabled
        {
            get { return sweepEnabled; }
        }

        public float SweepIntensity
        {
            get { return sweepIntensity; }
        }

        public bool NoiseEnabled
        {
            get { return noiseEnabled; }
        }

        public float NoiseIntensity
        {
            get { return noiseIntensity; }
        }

        public bool DissolveEnabled
        {
            get { return dissolveEnabled; }
        }

        public float DissolveAmount
        {
            get { return dissolveAmount; }
        }

        public bool DistortionEnabled
        {
            get { return distortionEnabled; }
        }

        public float DistortionStrength
        {
            get { return distortionStrength; }
        }

        public float HoverGlow
        {
            get { return hoverGlow; }
        }

        public float SelectedGlow
        {
            get { return selectedGlow; }
        }

        public float WarningGlow
        {
            get { return warningGlow; }
        }

        public float TransitionSpeed
        {
            get { return transitionSpeed; }
        }

        public float FlashIntensity
        {
            get { return flashIntensity; }
        }

        public float FlashDuration
        {
            get { return flashDuration; }
        }

        public bool AllowParticles
        {
            get { return allowParticles; }
        }
    }
}