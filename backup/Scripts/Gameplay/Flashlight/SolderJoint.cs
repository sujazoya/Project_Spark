// ============================================================================
// SolderJoint.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class SolderJoint : MonoBehaviour
    {
        [SerializeField]
        MeshRenderer renderer;

        Material mat;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        public bool Completed { get; private set; }

        void Awake()
        {
            mat = renderer.material;
        }

        public void Heat()
        {
            if (Completed)
                return;

            Completed = true;

            mat.EnableKeyword("_EMISSION");

            mat.SetColor(
                Emission,
                Color.yellow * 4f);
        }
    }
}