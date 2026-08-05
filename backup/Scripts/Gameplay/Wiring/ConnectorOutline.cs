// ============================================================================
// ConnectorOutline.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class ConnectorOutline : MonoBehaviour
    {
        [SerializeField]
        Renderer target;

        Material material;

        static readonly int Width =
            Shader.PropertyToID("_OutlineWidth");

        void Awake()
        {
            material = target.material;
        }

        public void Show()
        {
            material.SetFloat(
                Width,
                3f);
        }

        public void Hide()
        {
            material.SetFloat(
                Width,
                0f);
        }
    }
}