// ============================================================================
// BurnMark.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class BurnMark : MonoBehaviour
    {
        [SerializeField]
        MeshRenderer renderer;

        Material material;

        static readonly int Alpha =
            Shader.PropertyToID("_Alpha");

        void Awake()
        {
            material = renderer.material;
        }

        public void Show()
        {
            material.SetFloat(Alpha,1);
        }

        public void Hide()
        {
            material.SetFloat(Alpha,0);
        }
    }
}