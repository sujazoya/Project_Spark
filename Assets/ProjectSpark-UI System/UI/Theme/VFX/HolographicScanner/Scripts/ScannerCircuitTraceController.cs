using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerCircuitTraceController : MonoBehaviour
    {
        [Header("Wire Renderers")]
        [SerializeField]
        private Renderer[] wireRenderers;

        [Header("Shader Property")]
        [SerializeField]
        private string progressProperty = "_ScanProgress";

        [Header("Animation")]
        [SerializeField, Range(0f, 1f)]
        private float progress;

        [SerializeField]
        private bool active;

        private MaterialPropertyBlock propertyBlock;
        private int progressId;

        private void Awake()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            progressId =
                Shader.PropertyToID(
                    progressProperty);

            ApplyProgress();
        }

        public void SetProgress(float normalized)
        {
            progress =
                Mathf.Clamp01(normalized);

            ApplyProgress();
        }

        public void StartTrace()
        {
            active = true;
            ApplyProgress();
        }

        public void StopTrace()
        {
            active = false;

            progress = 0f;

            ApplyProgress();
        }

        public void CompleteTrace()
        {
            active = false;

            progress = 1f;

            ApplyProgress();
        }

        private void ApplyProgress()
        {
            if (wireRenderers == null)
                return;

            for (int i = 0;
                 i < wireRenderers.Length;
                 i++)
            {
                Renderer renderer =
                    wireRenderers[i];

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetFloat(
                    progressId,
                    active || progress > 0f
                        ? progress
                        : 0f);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }
    }
}