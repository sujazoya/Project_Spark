using UnityEngine;

namespace AAAUI.Runtime.Rendering.Tests
{
    public sealed class UIShaderPropertyTest : MonoBehaviour
    {
        [SerializeField]
        private Renderer targetRenderer;

        [SerializeField]
        [Range(0f, 1f)]
        private float glitchIntensity;

        [SerializeField]
        [Range(0f, 0.1f)]
        private float glitchAmount = 0.005f;

        [SerializeField]
        private float glitchSpeed = 8f;

        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(_block);

            _block.SetFloat(
                MaterialPropertyIds.GlitchIntensity,
                glitchIntensity);

            _block.SetFloat(
                MaterialPropertyIds.GlitchAmount,
                glitchAmount);

            _block.SetFloat(
                MaterialPropertyIds.GlitchSpeed,
                glitchSpeed);

            targetRenderer.SetPropertyBlock(_block);
        }
    }
}