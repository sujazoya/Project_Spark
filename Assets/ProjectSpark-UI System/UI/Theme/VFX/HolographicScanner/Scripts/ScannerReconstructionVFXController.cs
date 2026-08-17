using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerReconstructionVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform targetRoot;

        [SerializeField]
        private VisualEffect visualEffect;

        [SerializeField]
        private ScannerReconstructionPointGenerator pointGenerator;

        [Header("Geometry Sampling")]
        [SerializeField, Min(256)]
        private int pointCount = 30000;

        [SerializeField]
        private bool buildOnStart;

        [Header("Particle System")]
        [SerializeField, Min(0f)]
        private float particleRate = 30000f;

        [Header("VFX Property Names")]
        [SerializeField]
        private string positionsProperty =
            "_ReconstructionPositions";

        [SerializeField]
        private string pointCountProperty =
            "_PointCount";

        [SerializeField]
        private string reconstructingProperty =
            "_IsReconstructing";

        [SerializeField]
        private string particleRateProperty =
            "_ParticleRate";

        [SerializeField]
        private string revealHeightProperty =
            "_RevealHeight";

        [SerializeField]
        private string boundsMinProperty =
            "_BoundsMin";

        [SerializeField]
        private string boundsMaxProperty =
            "_BoundsMax";

        private bool initialized;
        private bool playing;
        private float revealHeight;

        public float RevealHeight =>
            revealHeight;

        public bool IsPlaying =>
            playing;

        public int PointCount =>
            pointGenerator != null
                ? pointGenerator.PointCount
                : 0;

        private void Awake()
        {
            ValidateReferences();

            // Deliberately do NOT build here.
            // Building 30,000+ reconstruction points can be expensive.
        }

        private void Start()
        {
            if (buildOnStart)
            {
                BuildReconstructionData();
                ResetVFX();
            }
        }

        // =========================================================
        // BUILD
        // =========================================================

        public bool BuildReconstructionData()
        {
            if (!ValidateReferences())
                return false;

            ConfigureGenerator();

            bool success =
                pointGenerator.Build();

            if (!success)
            {
                initialized = false;
                return false;
            }

            GraphicsBuffer buffer =
                pointGenerator.PositionBuffer;

            if (buffer == null)
            {
                Debug.LogError(
                    $"{name}: Point generator returned no GraphicsBuffer.",
                    this);

                initialized = false;
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    positionsProperty))
            {
                Debug.LogError(
                    $"{name}: Positions property name is empty.",
                    this);

                initialized = false;
                return false;
            }

            // Bind the Vector3 GraphicsBuffer.
            visualEffect.SetGraphicsBuffer(
                positionsProperty,
                buffer);

            visualEffect.SetUInt(
                pointCountProperty,
                (uint)pointGenerator.PointCount);

            Bounds bounds =
                pointGenerator.WorldBounds;

            visualEffect.SetVector3(
                boundsMinProperty,
                bounds.min);

            visualEffect.SetVector3(
                boundsMaxProperty,
                bounds.max);

            initialized = true;

            return true;
        }

        private void ConfigureGenerator()
        {
            if (pointGenerator == null)
                return;

            // Runtime value is copied here so the controller
            // remains the single place controlling point density.
            pointGenerator.SetPointCount(
                pointCount);

            pointGenerator.SetTargetRoot(
                targetRoot);
        }

        // =========================================================
        // START
        // =========================================================

        public void StartReconstruction()
        {
            if (!initialized)
            {
                if (!BuildReconstructionData())
                    return;
            }

            if (visualEffect == null)
                return;

            playing = true;
            revealHeight = 0f;

            ApplyRevealHeight();

            visualEffect.SetBool(
                reconstructingProperty,
                true);

            visualEffect.SetFloat(
                particleRateProperty,
                particleRate);

            visualEffect.Reinit();
            visualEffect.Play();
        }

        // =========================================================
        // STOP
        // =========================================================

        public void StopReconstruction()
        {
            playing = false;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);
        }

        // =========================================================
        // RESET
        // =========================================================

        public void ResetVFX()
        {
            playing = false;
            revealHeight = 0f;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);

            visualEffect.SetFloat(
                revealHeightProperty,
                0f);

            visualEffect.Reinit();
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        public void CompleteReconstruction()
        {
            if (!initialized)
            {
                if (!BuildReconstructionData())
                    return;
            }

            revealHeight = 1f;

            ApplyRevealHeight();

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                true);

            visualEffect.SetFloat(
                particleRateProperty,
                particleRate);

            playing = true;
        }

        // =========================================================
        // PROGRESS
        // =========================================================

        public void SetProgress(
            float normalized)
        {
            revealHeight =
                Mathf.Clamp01(
                    normalized);

            ApplyRevealHeight();
        }

        public void SetRevealHeight(
            float normalized)
        {
            SetProgress(
                normalized);
        }

        private void ApplyRevealHeight()
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                revealHeightProperty,
                revealHeight);
        }

        // =========================================================
        // REBUILD
        // =========================================================

        public void Rebuild()
        {
            bool wasPlaying =
                playing;

            if (wasPlaying)
            {
                StopReconstruction();
            }

            initialized =
                BuildReconstructionData();

            revealHeight = 0f;

            if (initialized)
            {
                ResetVFX();
            }

            if (wasPlaying)
            {
                StartReconstruction();
            }
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private bool ValidateReferences()
        {
            bool valid = true;

            if (targetRoot == null)
            {
                Debug.LogError(
                    $"{name}: Target Root is not assigned.",
                    this);

                valid = false;
            }

            if (visualEffect == null)
            {
                Debug.LogError(
                    $"{name}: Visual Effect is not assigned.",
                    this);

                valid = false;
            }

            if (pointGenerator == null)
            {
                Debug.LogError(
                    $"{name}: Point Generator is not assigned.",
                    this);

                valid = false;
            }

            return valid;
        }
    }
}