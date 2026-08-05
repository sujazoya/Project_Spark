// ============================================================================
// InspectionHighlighter.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class InspectionHighlighter : MonoBehaviour
    {
        [SerializeField]
        Renderer target;

        Material material;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            material = target.material;
        }

        public void Highlight()
        {
            material.EnableKeyword("_EMISSION");

            material.SetColor(
                Emission,
                Color.cyan * 5);
        }

        public void Clear()
        {
            material.SetColor(
                Emission,
                Color.black);
        }
    }
}