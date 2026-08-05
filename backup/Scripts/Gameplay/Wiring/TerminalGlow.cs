// ============================================================================
// TerminalGlow.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(Renderer))]
    public sealed class TerminalGlow : MonoBehaviour
    {
        Material material;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            material =
                GetComponent<Renderer>().material;
        }

        public void Hover()
        {
            material.EnableKeyword("_EMISSION");

            material.SetColor(
                Emission,
                Color.cyan * 4f);
        }

        public void Connected()
        {
            material.SetColor(
                Emission,
                Color.green * 6f);
        }

        public void Off()
        {
            material.SetColor(
                Emission,
                Color.black);
        }
    }
}