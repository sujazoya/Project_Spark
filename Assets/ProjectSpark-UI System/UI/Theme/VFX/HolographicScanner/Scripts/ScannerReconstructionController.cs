using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform targetRoot;

        [Header("Renderers")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Shader Properties")]
        [SerializeField] private string revealHeightProperty = "_RevealHeight";
        [SerializeField] private string boundsMinYProperty = "_BoundsMinY";
        [SerializeField] private string boundsMaxYProperty = "_BoundsMaxY";

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float reconstructionSpeed = 0.6f;

        [SerializeField]
        private bool loop;

        private MaterialPropertyBlock propertyBlock;

        private int revealHeightId;
        private int boundsMinYId;
        private int boundsMaxYId;

        private Bounds combinedBounds;

        private float revealHeight;
        private bool reconstructing;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            revealHeightId =
                Shader.PropertyToID(revealHeightProperty);

            boundsMinYId =
                Shader.PropertyToID(boundsMinYProperty);

            boundsMaxYId =
                Shader.PropertyToID(boundsMaxYProperty);

            BuildBounds();

            SetRevealHeight(0f);
        }

        private void Update()
        {
            if (!reconstructing)
                return;

            revealHeight +=
                reconstructionSpeed *
                Time.deltaTime;

            if (revealHeight >= 1f)
            {
                revealHeight = 1f;

                if (loop)
                {
                    revealHeight = 0f;
                }
                else
                {
                    reconstructing = false;
                }
            }

            SetRevealHeight(revealHeight);
        }

        public void StartReconstruction()
        {
            revealHeight = 0f;
            reconstructing = true;

            SetRevealHeight(revealHeight);
        }

        public void StopReconstruction()
        {
            reconstructing = false;
        }

        public void CompleteReconstruction()
        {
            reconstructing = false;
            revealHeight = 1f;

            SetRevealHeight(revealHeight);
        }

        public void ResetReconstruction()
        {
            reconstructing = false;
            revealHeight = 0f;

            SetRevealHeight(revealHeight);
        }

        public void SetProgress(float normalized)
        {
            revealHeight =
                Mathf.Clamp01(normalized);

            SetRevealHeight(revealHeight);
        }

        public float GetProgress()
        {
            return revealHeight;
        }

        private void BuildBounds()
        {
            if (targetRenderers == null ||
                targetRenderers.Length == 0)
            {
                combinedBounds =
                    new Bounds(transform.position, Vector3.one);

                return;
            }

            bool initialized = false;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer renderer =
                    targetRenderers[i];

                if (renderer == null)
                    continue;

                if (!initialized)
                {
                    combinedBounds =
                        renderer.bounds;

                    initialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(
                        renderer.bounds);
                }
            }

            if (!initialized)
            {
                combinedBounds =
                    new Bounds(
                        transform.position,
                        Vector3.one);
            }
        }

        private void SetRevealHeight(float normalized)
        {
            if (targetRenderers == null)
                return;

            float minY =
                combinedBounds.min.y;

            float maxY =
                combinedBounds.max.y;

            for (int i = 0;
                 i < targetRenderers.Length;
                 i++)
            {
                Renderer renderer =
                    targetRenderers[i];

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetFloat(
                    revealHeightId,
                    normalized);

                propertyBlock.SetFloat(
                    boundsMinYId,
                    minY);

                propertyBlock.SetFloat(
                    boundsMaxYId,
                    maxY);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }
    }
}