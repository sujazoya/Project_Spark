using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireMeshBuilder
    {
        public Mesh Build(WireMeshData data)
        {
            Mesh mesh = new Mesh();

            mesh.SetVertices(data.Vertices);
            mesh.SetTriangles(data.Triangles, 0);
            mesh.SetUVs(0, data.UVs);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
