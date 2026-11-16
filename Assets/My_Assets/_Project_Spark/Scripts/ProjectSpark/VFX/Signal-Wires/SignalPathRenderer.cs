using UnityEngine;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SignalPathRenderer : MonoBehaviour
    {
        private static readonly int PulsePositionID =
            Shader.PropertyToID("_PulsePosition");

        [SerializeField]
        private Renderer targetRenderer;

        [SerializeField]
        private float speed = 0.5f;

        private MaterialPropertyBlock propertyBlock;
        private float progress;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            progress += speed * Time.deltaTime;

            if (progress > 1f)
                progress -= 1f;

            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                PulsePositionID,
                progress
            );

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}