using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class WireRenderer : MonoBehaviour
    {
        private MeshFilter _meshFilter;

        private WireMeshGenerator _generator;

        private void Awake()
        {
            _meshFilter =
                GetComponent<MeshFilter>();

            _generator =
                new WireMeshGenerator();
        }

        public void Render(
            WireSpline spline)
        {
            _meshFilter.sharedMesh =
                _generator.Generate(spline);
        }
    }
}
