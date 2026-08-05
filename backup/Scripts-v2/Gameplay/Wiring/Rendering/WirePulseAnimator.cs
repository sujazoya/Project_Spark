using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WirePulseAnimator
        : MonoBehaviour
    {
        [SerializeField]
        private Renderer wireRenderer;

        private static readonly int Offset =
            Shader.PropertyToID("_PulseOffset");

        [SerializeField]
        private float speed = 2f;

        private void Update()
        {
            float value =
                Time.time * speed;

            wireRenderer.material.SetFloat(
                Offset,
                value);
        }
    }
}
