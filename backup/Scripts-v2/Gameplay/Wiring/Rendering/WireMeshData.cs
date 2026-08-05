using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireMeshData
    {
        public readonly List<Vector3> Vertices = new();

        public readonly List<int> Triangles = new();

        public readonly List<Vector2> UVs = new();

        public void Clear()
        {
            Vertices.Clear();
            Triangles.Clear();
            UVs.Clear();
        }
    }
}
