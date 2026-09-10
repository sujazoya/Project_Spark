using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicSnapGeometry
        : MonoBehaviour
    {
        [Header("Source")]

        [SerializeField]
        private MeshFilter source;

        [Header("Build")]

        [SerializeField]
        private bool buildOnAwake = true;

        private readonly List<VertexData> vertices =
            new List<VertexData>(256);

        private readonly List<EdgeData> edges =
            new List<EdgeData>(512);

        private readonly List<FaceData> faces =
            new List<FaceData>(256);

        private struct VertexData
        {
            public Vector3 localPosition;
        }

        private struct EdgeData
        {
            public int a;

            public int b;

            public bool boundary;
        }

        private struct FaceData
        {
            public Vector3 localCenter;

            public Vector3 localNormal;

            public int a;

            public int b;

            public int c;
        }

        public MeshFilter Source =>
            source;

        public Transform SourceTransform =>
            source != null
                ? source.transform
                : transform;

        public int VertexCount =>
            vertices.Count;

        public int EdgeCount =>
            edges.Count;

        public int FaceCount =>
            faces.Count;

        private void Reset()
        {
            if (source == null)
                source =
                    GetComponent<MeshFilter>();
        }

        private void Awake()
        {
            if (source == null)
            {
                source =
                    GetComponent<MeshFilter>();
            }

            if (buildOnAwake)
                Build();
        }

        public Vector3 GetVertexLocal(
            int index)
        {
            return vertices[index]
                .localPosition;
        }

        public void GetEdge(
            int index,
            out int vertexA,
            out int vertexB,
            out Vector3 localA,
            out Vector3 localB,
            out bool boundary)
        {
            EdgeData edge =
                edges[index];

            vertexA =
                edge.a;

            vertexB =
                edge.b;

            localA =
                vertices[edge.a]
                    .localPosition;

            localB =
                vertices[edge.b]
                    .localPosition;

            boundary =
                edge.boundary;
        }

        public void GetFace(
            int index,
            out Vector3 localCenter,
            out Vector3 localNormal,
            out int a,
            out int b,
            out int c)
        {
            FaceData face =
                faces[index];

            localCenter =
                face.localCenter;

            localNormal =
                face.localNormal;

            a =
                face.a;

            b =
                face.b;

            c =
                face.c;
        }

     /*   public void Build()
        {
            vertices.Clear();

            edges.Clear();

            faces.Clear();

            if (source == null)
                return;

            Mesh mesh =
                source.sharedMesh;

            if (mesh == null)
                return;

            Vector3[] meshVertices =
                mesh.vertices;

            int[] triangles =
                mesh.triangles;

            BuildVertices(
                meshVertices
            );

            BuildFaces(
                meshVertices,
                triangles
            );

            BuildEdges(
                triangles
            );
        }*/

        public void Build()
{
    vertices.Clear();
    edges.Clear();
    faces.Clear();

    if (source == null)
        return;

    Mesh mesh = source.sharedMesh;

    if (mesh == null)
        return;

    Vector3[] meshVertices = mesh.vertices;

    if (meshVertices == null ||
        meshVertices.Length == 0)
    {
        return;
    }

    BuildVertices(meshVertices);

    BuildFromSubmeshes(mesh);
}
private void BuildFromSubmeshes(
    Mesh mesh)
{
    Dictionary<EdgeKey, int> edgeUsage =
        new Dictionary<EdgeKey, int>();

    int subMeshCount =
        mesh.subMeshCount;

    for (int subMesh = 0;
         subMesh < subMeshCount;
         subMesh++)
    {
        MeshTopology topology =
            mesh.GetTopology(subMesh);

        // ----------------------------------------------------
        // Only triangles can generate our face/edge topology.
        // Lines and points are intentionally ignored here.
        // ----------------------------------------------------

        if (topology != MeshTopology.Triangles)
        {
            continue;
        }

        int[] triangles =
            mesh.GetIndices(subMesh);

        if (triangles == null ||
            triangles.Length < 3)
        {
            continue;
        }

        BuildTriangleSubmesh(
            mesh.vertices,
            triangles,
            edgeUsage
        );
    }

    BuildEdgesFromUsage(edgeUsage);
}

private void BuildTriangleSubmesh(
    Vector3[] meshVertices,
    int[] triangles,
    Dictionary<EdgeKey, int> edgeUsage)
{
    for (int i = 0;
         i <= triangles.Length - 3;
         i += 3)
    {
        int a = triangles[i];
        int b = triangles[i + 1];
        int c = triangles[i + 2];

        if (!IsValidVertexIndex(
                a,
                meshVertices.Length) ||
            !IsValidVertexIndex(
                b,
                meshVertices.Length) ||
            !IsValidVertexIndex(
                c,
                meshVertices.Length))
        {
            continue;
        }

        Vector3 pa =
            meshVertices[a];

        Vector3 pb =
            meshVertices[b];

        Vector3 pc =
            meshVertices[c];

        Vector3 normal =
            Vector3.Cross(
                pb - pa,
                pc - pa
            );

        if (normal.sqrMagnitude <
            0.000001f)
        {
            continue;
        }

        normal.Normalize();

        Vector3 center =
            (pa + pb + pc) / 3f;

        faces.Add(
            new FaceData
            {
                localCenter = center,

                localNormal = normal,

                a = a,

                b = b,

                c = c
            }
        );

        AddEdgeUsage(
            edgeUsage,
            new EdgeKey(a, b)
        );

        AddEdgeUsage(
            edgeUsage,
            new EdgeKey(b, c)
        );

        AddEdgeUsage(
            edgeUsage,
            new EdgeKey(c, a)
        );
    }
}
private void BuildEdgesFromUsage(
    Dictionary<EdgeKey, int> edgeUsage)
{
    foreach (
        KeyValuePair<EdgeKey, int> pair
        in edgeUsage)
    {
        edges.Add(
            new EdgeData
            {
                a = pair.Key.a,

                b = pair.Key.b,

                boundary =
                    pair.Value == 1
            }
        );
    }
}
private static bool IsValidVertexIndex(
    int index,
    int vertexCount)
{
    return
        index >= 0 &&
        index < vertexCount;
}

        private void BuildVertices(
            Vector3[] meshVertices)
        {
            for (int i = 0;
                 i < meshVertices.Length;
                 i++)
            {
                vertices.Add(
                    new VertexData
                    {
                        localPosition =
                            meshVertices[i]
                    }
                );
            }
        }

        private void BuildFaces(
            Vector3[] meshVertices,
            int[] triangles)
        {
            for (int i = 0;
                 i <= triangles.Length - 3;
                 i += 3)
            {
                int a =
                    triangles[i];

                int b =
                    triangles[i + 1];

                int c =
                    triangles[i + 2];

                Vector3 pa =
                    meshVertices[a];

                Vector3 pb =
                    meshVertices[b];

                Vector3 pc =
                    meshVertices[c];

                Vector3 edgeAB =
                    pb - pa;

                Vector3 edgeAC =
                    pc - pa;

                Vector3 normal =
                    Vector3.Cross(
                        edgeAB,
                        edgeAC
                    );

                if (normal.sqrMagnitude >
                    0.000001f)
                {
                    normal.Normalize();
                }
                else
                {
                    normal = Vector3.zero;
                }

                Vector3 center =
                    (pa + pb + pc) / 3f;

                faces.Add(
                    new FaceData
                    {
                        localCenter = center,

                        localNormal = normal,

                        a = a,

                        b = b,

                        c = c
                    }
                );
            }
        }

        private void BuildEdges(
            int[] triangles)
        {
            Dictionary<EdgeKey, int>
                edgeUsage =
                    new Dictionary<EdgeKey, int>(
                        triangles.Length
                    );

            for (int i = 0;
                 i <= triangles.Length - 3;
                 i += 3)
            {
                int a =
                    triangles[i];

                int b =
                    triangles[i + 1];

                int c =
                    triangles[i + 2];

                AddEdgeUsage(
                    edgeUsage,
                    new EdgeKey(a, b)
                );

                AddEdgeUsage(
                    edgeUsage,
                    new EdgeKey(b, c)
                );

                AddEdgeUsage(
                    edgeUsage,
                    new EdgeKey(c, a)
                );
            }

            foreach (
                KeyValuePair<EdgeKey, int> pair
                in edgeUsage)
            {
                edges.Add(
                    new EdgeData
                    {
                        a = pair.Key.a,

                        b = pair.Key.b,

                        boundary =
                            pair.Value == 1
                    }
                );
            }
        }

        private static void AddEdgeUsage(
            Dictionary<EdgeKey, int> dictionary,
            EdgeKey edge)
        {
            if (dictionary.TryGetValue(
                    edge,
                    out int count))
            {
                dictionary[edge] =
                    count + 1;
            }
            else
            {
                dictionary.Add(
                    edge,
                    1
                );
            }
        }

        private readonly struct EdgeKey
        {
            public readonly int a;

            public readonly int b;

            public EdgeKey(
                int first,
                int second)
            {
                if (first < second)
                {
                    a = first;
                    b = second;
                }
                else
                {
                    a = second;
                    b = first;
                }
            }

            public override bool Equals(
                object obj)
            {
                return
                    obj is EdgeKey other &&
                    a == other.a &&
                    b == other.b;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return
                        (a * 397) ^ b;
                }
            }
        }
    }
}