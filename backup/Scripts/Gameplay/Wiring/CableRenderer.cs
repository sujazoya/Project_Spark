// ============================================================================
// CableRenderer.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(SplineMeshBuilder))]
    public sealed class CableRenderer : MonoBehaviour
    {
        [SerializeField]
        Renderer cableRenderer;

        Material material;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            material = cableRenderer.material;
        }

        public void SetIdle()
        {
            material.SetColor(
                Emission,
                Color.black);
        }

        public void SetDragging()
        {
            material.EnableKeyword("_EMISSION");

            material.SetColor(
                Emission,
                Color.cyan * 2f);
        }

        public void SetPowered()
        {
            material.EnableKeyword("_EMISSION");

            material.SetColor(
                Emission,
                Color.yellow * 8f);
        }
    }
}