// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/SplineMeshBuilder.cs
// Production Version
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SplineMeshBuilder : MonoBehaviour
    {
        [SerializeField] private WireSpline spline;

        [Header("Shape")]
        [SerializeField] private float radius = .004f;
        [SerializeField] private int radialSegments = 10;
        [SerializeField] private int lengthSegments = 40;

        Mesh mesh;

        readonly List<Vector3> vertices = new();
        readonly List<int> triangles = new();
        readonly List<Vector2> uvs = new();
        readonly List<Vector3> normals = new();

        void Awake()
        {
            mesh = new Mesh();
            mesh.name = "Cable";

            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        void LateUpdate()
        {
            Generate();
        }

        void Generate()
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            normals.Clear();

            for (int y = 0; y <= lengthSegments; y++)
            {
                float t = y / (float)lengthSegments;

                Vector3 center = spline.GetPoint(t);

                Vector3 next =
                    spline.GetPoint(
                        Mathf.Min(1f, t + .01f));

                Vector3 forward =
                    (next - center).normalized;

                Quaternion rotation =
                    Quaternion.LookRotation(forward);

                for (int x = 0; x < radialSegments; x++)
                {
                    float angle =
                        Mathf.PI * 2f *
                        x /
                        radialSegments;

                    Vector3 normal =
                        new(
                            Mathf.Cos(angle),
                            Mathf.Sin(angle),
                            0);

                    vertices.Add(
                        center +
                        rotation *
                        normal *
                        radius);

                    normals.Add(
                        rotation *
                        normal);

                    uvs.Add(
                        new Vector2(
                            x / (float)radialSegments,
                            t));
                }
            }

            for (int y = 0; y < lengthSegments; y++)
            {
                int row = y * radialSegments;
                int next = (y + 1) * radialSegments;

                for (int x = 0; x < radialSegments; x++)
                {
                    int a = row + x;
                    int b = row + (x + 1) % radialSegments;
                    int c = next + x;
                    int d = next + (x + 1) % radialSegments;

                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);

                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            mesh.Clear();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateBounds();
        }
    }
}