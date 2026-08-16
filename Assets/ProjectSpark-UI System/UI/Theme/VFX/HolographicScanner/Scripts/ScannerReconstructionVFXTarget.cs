using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerReconstructionVFXTarget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Renderer targetRenderer;

        [SerializeField]
        private VisualEffect visualEffect;

        [Header("VFX Property Names")]
        [SerializeField]
        private string reconstructingProperty =
            "_IsReconstructing";

        [SerializeField]
        private string particleRateProperty =
            "_ParticleRate";

        [SerializeField]
        private string spawnCenterProperty =
            "_SpawnCenter";

        [SerializeField]
        private string spawnSizeProperty =
            "_SpawnSize";

        [SerializeField]
        private string revealHeightProperty =
            "_RevealHeight";

        [Header("Particle Settings")]
        [SerializeField, Min(0f)]
        private float particleRate = 700f;

        [SerializeField, Range(0.01f, 1f)]
        private float volumePadding = 0.02f;

        private Bounds targetBounds;
        private Bounds scannerBounds;

        private bool initialized;
        private bool active;

        public Renderer TargetRenderer =>
            targetRenderer;

        public VisualEffect VisualEffect =>
            visualEffect;

        public Bounds TargetBounds =>
            targetBounds;

        public void Initialize(
            Renderer renderer,
            VisualEffect effect,
            Bounds globalScannerBounds,
            float rate)
        {
            targetRenderer = renderer;
            visualEffect = effect;
            scannerBounds = globalScannerBounds;
            particleRate = Mathf.Max(0f, rate);

            if (targetRenderer == null)
            {
                Debug.LogError(
                    $"{name}: Target renderer is null.",
                    this);

                return;
            }

            if (visualEffect == null)
            {
                Debug.LogError(
                    $"{name}: VisualEffect is null.",
                    this);

                return;
            }

            initialized = true;

            RecalculateBounds();
            ConfigureTransform();
            ApplySpawnVolume();
            SetRevealHeight(0f);
            Stop();
        }

        public void Refresh(
            Bounds globalScannerBounds)
        {
            scannerBounds =
                globalScannerBounds;

            if (!initialized)
                return;

            RecalculateBounds();
            ConfigureTransform();
            ApplySpawnVolume();
        }

        public void Play()
        {
            if (!initialized ||
                visualEffect == null)
                return;

            active = true;

            visualEffect.SetBool(
                reconstructingProperty,
                true);

            visualEffect.SetFloat(
                particleRateProperty,
                particleRate);

            visualEffect.Reinit();
            visualEffect.Play();
        }

        public void Stop()
        {
            active = false;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);
        }

        public void ResetTarget()
        {
            active = false;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);

            SetRevealHeight(0f);

            visualEffect.Reinit();
        }

        public void Complete()
        {
            if (visualEffect == null)
                return;

            SetRevealHeight(1f);

            visualEffect.SetBool(
                reconstructingProperty,
                true);

            visualEffect.SetFloat(
                particleRateProperty,
                particleRate);
        }

        public void SetRevealHeight(
            float globalNormalizedProgress)
        {
            if (visualEffect == null)
                return;

            float localReveal =
                CalculateLocalRevealHeight(
                    globalNormalizedProgress);

            visualEffect.SetFloat(
                revealHeightProperty,
                localReveal);
        }

        private void RecalculateBounds()
        {
            if (targetRenderer == null)
                return;

            targetBounds =
                targetRenderer.bounds;
        }

        private void ConfigureTransform()
        {
            if (targetRenderer == null)
                return;

            /*
             * Each VFX instance gets its own world-space
             * AABB represented by a local VFX volume.
             *
             * We intentionally use:
             *
             * Position = bounds center
             * Rotation = identity
             * Scale = one
             *
             * so local Y corresponds directly to world Y.
             */

            transform.position =
                targetBounds.center;

            transform.rotation =
                Quaternion.identity;

            transform.localScale =
                Vector3.one;
        }

        private void ApplySpawnVolume()
        {
            if (visualEffect == null)
                return;

            Vector3 size =
                targetBounds.size;

            size.x =
                Mathf.Max(
                    size.x,
                    0.001f);

            size.y =
                Mathf.Max(
                    size.y,
                    0.001f);

            size.z =
                Mathf.Max(
                    size.z,
                    0.001f);

            /*
             * Slight padding prevents particles from
             * being clipped exactly at the renderer edge.
             */

            size *=
                1f + volumePadding;

            visualEffect.SetVector3(
                spawnCenterProperty,
                Vector3.zero);

            visualEffect.SetVector3(
                spawnSizeProperty,
                size);
        }

        private float CalculateLocalRevealHeight(
            float globalNormalizedProgress)
        {
            globalNormalizedProgress =
                Mathf.Clamp01(
                    globalNormalizedProgress);

            float globalMinY =
                scannerBounds.min.y;

            float globalMaxY =
                scannerBounds.max.y;

            float globalHeight =
                Mathf.Max(
                    globalMaxY - globalMinY,
                    0.0001f);

            float scanWorldY =
                Mathf.Lerp(
                    globalMinY,
                    globalMaxY,
                    globalNormalizedProgress);

            float localMinY =
                targetBounds.min.y;

            float localMaxY =
                targetBounds.max.y;

            float localHeight =
                Mathf.Max(
                    localMaxY - localMinY,
                    0.0001f);

            /*
             * Convert global scan plane into this
             * object's local normalized height.
             */

            float localReveal =
                (scanWorldY - localMinY) /
                localHeight;

            /*
             * Important:
             *
             * Objects completely below the scan plane
             * should be fully reconstructed.
             *
             * Objects completely above the scan plane
             * should not yet be reconstructed.
             */

            if (scanWorldY <= localMinY)
                return 0f;

            if (scanWorldY >= localMaxY)
                return 1f;

            return Mathf.Clamp01(
                localReveal);
        }
    }
}