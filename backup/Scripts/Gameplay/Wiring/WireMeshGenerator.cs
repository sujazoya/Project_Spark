// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireMeshGenerator.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class WireMeshGenerator : MonoBehaviour
    {
        [SerializeField]
        private WireSpline spline;

        [SerializeField]
        private WireSettings settings;

        private Mesh mesh;

        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uv;

        private void Awake()
        {
            mesh = new Mesh();
            mesh.name = "Wire Mesh";

            GetComponent<MeshFilter>().sharedMesh = mesh;

            Build();
        }

        private void LateUpdate()
        {
            UpdateMesh();
        }

        void Build()
        {
            int rings = settings.LengthSegments + 1;
            int radial = settings.RadialSegments;

            vertices = new Vector3[rings * radial];
            uv = new Vector2[vertices.Length];

            triangles =
                new int[
                    settings.LengthSegments *
                    radial *
                    6];
        }

        void UpdateMesh()
        {
            int radial = settings.RadialSegments;

            for (int i = 0; i <= settings.LengthSegments; i++)
            {
                float t = i / (float)settings.LengthSegments;

                Vector3 center = spline.GetPoint(t);

                Vector3 next = spline.GetPoint(Mathf.Min(t + .01f, 1));

                Vector3 forward = (next - center).normalized;

                Quaternion rot =
                    Quaternion.LookRotation(forward);

                for (int j = 0; j < radial; j++)
                {
                    float a =
                        Mathf.PI *
                        2f *
                        j /
                        radial;

                    Vector3 p =
                        new Vector3(
                            Mathf.Cos(a),
                            Mathf.Sin(a),
                            0) *
                        settings.Radius;

                    vertices[i * radial + j] =
                        center +
                        rot * p;

                    uv[i * radial + j] =
                        new Vector2(
                            j / (float)radial,
                            t);
                }
            }

            int index = 0;

            for (int y = 0; y < settings.LengthSegments; y++)
            {
                for (int x = 0; x < radial; x++)
                {
                    int current = y * radial + x;

                    int next = current + radial;

                    int right = y * radial + (x + 1) % radial;

                    int nextRight = right + radial;

                    triangles[index++] = current;
                    triangles[index++] = next;
                    triangles[index++] = right;

                    triangles[index++] = right;
                    triangles[index++] = next;
                    triangles[index++] = nextRight;
                }
            }

            mesh.Clear();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
        }
    }
}