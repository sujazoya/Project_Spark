using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.HolographicViewer
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class HolographicWireframe : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private MeshFilter source;

        [Header("Material")]
        [SerializeField] private Material wireframeMaterial;

        private MeshFilter targetMeshFilter;
        private MeshRenderer targetRenderer;

        private Mesh generatedMesh;

        private void Awake()
        {
            targetMeshFilter = GetComponent<MeshFilter>();
            targetRenderer = GetComponent<MeshRenderer>();

            if (source == null)
            {
                Debug.LogError(
                    $"[{nameof(HolographicWireframe)}] Source MeshFilter is not assigned on {name}.",
                    this
                );

                enabled = false;
                return;
            }

            if (source.sharedMesh == null)
            {
                Debug.LogError(
                    $"[{nameof(HolographicWireframe)}] Source MeshFilter has no mesh on {source.name}.",
                    source
                );

                enabled = false;
                return;
            }

            BuildWireframe();
        }

        private void BuildWireframe()
        {
            Mesh sourceMesh = source.sharedMesh;

            Vector3[] sourceVertices = sourceMesh.vertices;
            int[] triangles = sourceMesh.triangles;

            if (sourceVertices == null ||
                sourceVertices.Length == 0 ||
                triangles == null ||
                triangles.Length < 3)
            {
                Debug.LogError(
                    $"[{nameof(HolographicWireframe)}] Invalid source mesh: {sourceMesh.name}.",
                    source
                );

                return;
            }

            var edges = new HashSet<Edge>();

            for (int i = 0; i <= triangles.Length - 3; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                if (a < sourceVertices.Length &&
                    b < sourceVertices.Length)
                {
                    edges.Add(new Edge(a, b));
                }

                if (b < sourceVertices.Length &&
                    c < sourceVertices.Length)
                {
                    edges.Add(new Edge(b, c));
                }

                if (c < sourceVertices.Length &&
                    a < sourceVertices.Length)
                {
                    edges.Add(new Edge(c, a));
                }
            }

            if (edges.Count == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(HolographicWireframe)}] No edges generated from {sourceMesh.name}.",
                    source
                );

                return;
            }

            Vector3[] vertices =
                new Vector3[edges.Count * 2];

            int[] indices =
                new int[edges.Count * 2];

            int vertexIndex = 0;

            foreach (Edge edge in edges)
            {
                vertices[vertexIndex] =
                    sourceVertices[edge.A];

                vertices[vertexIndex + 1] =
                    sourceVertices[edge.B];

                indices[vertexIndex] =
                    vertexIndex;

                indices[vertexIndex + 1] =
                    vertexIndex + 1;

                vertexIndex += 2;
            }

            generatedMesh = new Mesh
            {
                name = sourceMesh.name + "_Wireframe"
            };

            generatedMesh.indexFormat =
                vertices.Length > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16;

            generatedMesh.vertices = vertices;

            generatedMesh.SetIndices(
                indices,
                MeshTopology.Lines,
                0,
                true
            );

            generatedMesh.RecalculateBounds();

            targetMeshFilter.sharedMesh =
                generatedMesh;

            targetRenderer.sharedMaterial =
                wireframeMaterial;
        }

        private void OnDestroy()
        {
            if (generatedMesh != null)
            {
                Destroy(generatedMesh);
            }
        }

        private readonly struct Edge
        {
            public readonly int A;
            public readonly int B;

            public Edge(int a, int b)
            {
                if (a < b)
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is not Edge other)
                    return false;

                return A == other.A &&
                       B == other.B;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (A * 397) ^ B;
                }
            }
        }
    }
}