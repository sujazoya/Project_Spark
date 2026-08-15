using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionVFXController : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField]
        private VisualEffect visualEffect;

        [Header("Board")]
        [SerializeField]
        private Renderer[] boardRenderers;

        [Header("Particle Settings")]
        [SerializeField, Min(0f)]
        private float particleRate = 700f;

        [Header("Spawn Height")]
        [SerializeField, Range(0.05f, 1f)]
        private float verticalThickness = 0.35f;

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

        private Bounds boardBounds;

        private bool isPlaying;
        private float revealHeight;

        public float RevealHeight => revealHeight;

        private void Awake()
        {
            RecalculateBounds();

            ResetVFX();
        }

        public void RecalculateBounds()
        {
            bool initialized = false;

            if (boardRenderers != null)
            {
                for (int i = 0;
                     i < boardRenderers.Length;
                     i++)
                {
                    Renderer renderer =
                        boardRenderers[i];

                    if (renderer == null)
                        continue;

                    if (!initialized)
                    {
                        boardBounds =
                            renderer.bounds;

                        initialized = true;
                    }
                    else
                    {
                        boardBounds.Encapsulate(
                            renderer.bounds);
                    }
                }
            }

            if (!initialized)
            {
                boardBounds =
                    new Bounds(
                        transform.position,
                        Vector3.one);
            }
        }

        public void StartReconstruction()
        {
            if (visualEffect == null)
                return;

            RecalculateBounds();

            isPlaying = true;

            revealHeight = 0f;

            ApplyVFX();

            visualEffect.Reinit();
            visualEffect.Play();
        }

        public void StopReconstruction()
        {
            isPlaying = false;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);
        }

        public void ResetVFX()
        {
            isPlaying = false;

            revealHeight = 0f;

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                false);

            visualEffect.SetFloat(
                particleRateProperty,
                0f);

            visualEffect.Reinit();
        }

        public void CompleteReconstruction()
        {
            revealHeight = 1f;

            ApplySpawnVolume();

            if (visualEffect == null)
                return;

            visualEffect.SetBool(
                reconstructingProperty,
                true);

            visualEffect.SetFloat(
                particleRateProperty,
                particleRate);
        }

        public void SetRevealHeight(float normalized)
        {
            revealHeight =
                Mathf.Clamp01(normalized);

            ApplyVFX();
        }

        public void SetProgress(float normalized)
        {
            SetRevealHeight(normalized);
        }

        private void Update()
        {
            if (!isPlaying)
                return;

            ApplySpawnVolume();
        }

        private void ApplyVFX()
        {
            if (visualEffect == null)
                return;

            ApplySpawnVolume();

            visualEffect.SetBool(
                reconstructingProperty,
                isPlaying);

            visualEffect.SetFloat(
                particleRateProperty,
                isPlaying
                    ? particleRate
                    : 0f);
        }

        private void ApplySpawnVolume()
        {
            if (visualEffect == null)
                return;

            float fullHeight =
                Mathf.Max(
                    boardBounds.size.y,
                    0.001f);

            float reconstructedHeight =
                fullHeight *
                revealHeight;

            float minY =
                boardBounds.min.y;

            float centerY =
                minY +
                reconstructedHeight * 0.5f;

            // Keep some thickness even when the scan is near zero.
            float actualHeight =
                Mathf.Max(
                    reconstructedHeight,
                    fullHeight *
                    verticalThickness *
                    0.02f);

            Vector3 spawnCenter =
                boardBounds.center;

            spawnCenter.y = centerY;

            Vector3 spawnSize =
                boardBounds.size;

            spawnSize.y = actualHeight;

            visualEffect.SetVector3(
                spawnCenterProperty,
                spawnCenter);

            visualEffect.SetVector3(
                spawnSizeProperty,
                spawnSize);
        }
    }
}