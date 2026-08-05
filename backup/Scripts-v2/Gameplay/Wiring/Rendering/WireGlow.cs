using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireGlow : MonoBehaviour
    {
        [SerializeField]
        private Renderer targetRenderer;

        private static readonly int GlowId =
            Shader.PropertyToID("_Glow");

        public void SetGlow(float value)
        {
            targetRenderer.material.SetFloat(
                GlowId,
                value);
        }
    }
}
