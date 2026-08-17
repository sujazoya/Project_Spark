using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Collects all target meshes and generates a fixed set of
    /// world-space points distributed across their actual triangle surfaces.
    ///
    /// The generated Vector3 positions are uploaded to a GraphicsBuffer
    /// and consumed by one VFX Graph reconstruction system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScannerReconstructionPointGenerator : MonoBehaviour
    {
        [Serializable]
        private struct MeshSource
        {
            public Mesh mesh;
            public Matrix4x4 localToWorld;
            public bool temporaryMesh;
        }

        private struct TriangleSample
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            public float cumulativeArea;
        }

        [Header("Target")]
        [SerializeField]
        private Transform targetRoot;

        [Header("Sampling")]
        [SerializeField, Min(256)]
        private int pointCount = 30000;

        [SerializeField]
        private int randomSeed = 12345;

        [SerializeField, Min(0f)]
        private float surfaceOffset = 0.0015f;

        [SerializeField]
        private bool includeInactiveObjects;

        [SerializeField]
        private bool includeSkinnedMeshRenderers;

        [SerializeField]
        private bool includeDisabledRenderers;

        private GraphicsBuffer positionBuffer;

        private Vector3[] positions;

        private Bounds worldBounds;

        private bool built;

        private readonly List<MeshSource> meshSources =
            new List<MeshSource>(128);

        private readonly List<TriangleSample> triangles =
            new List<TriangleSample>(65536);

        private System.Random random;

        public GraphicsBuffer PositionBuffer =>
            positionBuffer;

        public int PointCount =>
            positions != null
                ? positions.Length
                : 0;

        public Bounds WorldBounds =>
            worldBounds;

        public bool IsBuilt =>
            built;

        public int RequestedPointCount =>
            pointCount;

        /// <summary>
        /// Explicitly builds the scanner point data.
        /// This should NOT be called every frame.
        /// </summary>
        /// 

        public void SetTargetRoot(Transform root)
        {
            targetRoot = root;
        }

        public void SetPointCount(int count)
        {
            pointCount =
                Mathf.Max(256, count);
        }
        public bool Build()
        {
            ReleaseBuffer();

            built = false;

            meshSources.Clear();
            triangles.Clear();

            if (targetRoot == null)
            {
                Debug.LogError(
                    $"{name}: Target Root is not assigned.",
                    this);

                return false;
            }

            pointCount =
                Mathf.Max(
                    256,
                    pointCount);

            random =
                new System.Random(
                    randomSeed);

            CollectMeshSources();

            if (meshSources.Count == 0)
            {
                Debug.LogError(
                    $"{name}: No valid mesh sources were found under " +
                    $"'{targetRoot.name}'.",
                    this);

                CleanupTemporaryMeshes();

                return false;
            }

            BuildTriangleTable();

            if (triangles.Count == 0)
            {
                Debug.LogError(
                    $"{name}: No valid triangles were found.",
                    this);

                CleanupTemporaryMeshes();

                return false;
            }

            BuildWorldBounds();

            if (worldBounds.size.sqrMagnitude <=
                Mathf.Epsilon)
            {
                Debug.LogError(
                    $"{name}: Generated bounds are invalid.",
                    this);

                CleanupTemporaryMeshes();

                return false;
            }

            positions =
                new Vector3[pointCount];

            GeneratePoints();

            positionBuffer =
                new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    positions.Length,
                    sizeof(float) * 3);

            positionBuffer.name =
                "ProjectSpark_Effect04_ReconstructionPositions";

            positionBuffer.SetData(
                positions);

            CleanupTemporaryMeshes();

            built = true;

            return true;
        }

        /// <summary>
        /// Releases the GPU buffer and generated CPU data.
        /// </summary>
        public void ReleaseBuffer()
        {
            if (positionBuffer != null)
            {
                positionBuffer.Release();
                positionBuffer = null;
            }

            positions = null;
            built = false;

            CleanupTemporaryMeshes();

            meshSources.Clear();
            triangles.Clear();
        }

        private void OnDestroy()
        {
            ReleaseBuffer();
        }

        // =========================================================
        // MESH COLLECTION
        // =========================================================

        private void CollectMeshSources()
        {
            if (targetRoot == null)
                return;

            // ---------------------------------------------------------
            // MeshFilter sources
            // ---------------------------------------------------------

            MeshFilter[] meshFilters =
                targetRoot.GetComponentsInChildren<MeshFilter>(
                    includeInactiveObjects);

            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];

                if (meshFilter == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;

                if (mesh == null)
                    continue;

                // A scanner needs geometry, not necessarily a renderer.
                if (mesh.vertexCount < 3)
                    continue;

                if (mesh.triangles == null ||
                    mesh.triangles.Length < 3)
                    continue;

                // Unity requires readable mesh data for vertices/triangles.
                if (!mesh.isReadable)
                {
                    Debug.LogWarning(
                        $"{name}: Skipping unreadable mesh '{mesh.name}' " +
                        $"on '{meshFilter.name}'. " +
                        "Enable Read/Write on the source model asset.",
                        mesh);

                    continue;
                }

                meshSources.Add(
                    new MeshSource
                    {
                        mesh = mesh,
                        localToWorld =
                            meshFilter.transform.localToWorldMatrix,
                        temporaryMesh = false
                    });
            }

            // ---------------------------------------------------------
            // Skinned Mesh Renderer sources
            // ---------------------------------------------------------

            if (!includeSkinnedMeshRenderers)
                return;

            SkinnedMeshRenderer[] skinnedRenderers =
                targetRoot.GetComponentsInChildren<SkinnedMeshRenderer>(
                    includeInactiveObjects);

            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer skinned =
                    skinnedRenderers[i];

                if (skinned == null)
                    continue;

                if (!includeDisabledRenderers &&
                    !skinned.enabled)
                {
                    continue;
                }

                Mesh bakedMesh = new Mesh();

                bakedMesh.name =
                    $"RuntimeScannerMesh_{skinned.name}";

                skinned.BakeMesh(bakedMesh);

                if (bakedMesh.vertexCount < 3 ||
                    bakedMesh.triangles == null ||
                    bakedMesh.triangles.Length < 3)
                {
                    DestroyRuntimeMesh(bakedMesh);
                    continue;
                }

                meshSources.Add(
                    new MeshSource
                    {
                        mesh = bakedMesh,
                        localToWorld =
                            skinned.transform.localToWorldMatrix,
                        temporaryMesh = true
                    });
            }
        }

        // =========================================================
        // TRIANGLE TABLE
        // =========================================================

        private void BuildTriangleTable()
        {
            double totalArea = 0.0;

            for (int sourceIndex = 0;
                 sourceIndex < meshSources.Count;
                 sourceIndex++)
            {
                MeshSource source =
                    meshSources[sourceIndex];

                Vector3[] vertices =
                    source.mesh.vertices;

                int[] indices =
                    source.mesh.triangles;

                for (int i = 0;
                     i <= indices.Length - 3;
                     i += 3)
                {
                    Vector3 a =
                        source.localToWorld.MultiplyPoint3x4(
                            vertices[indices[i]]);

                    Vector3 b =
                        source.localToWorld.MultiplyPoint3x4(
                            vertices[indices[i + 1]]);

                    Vector3 c =
                        source.localToWorld.MultiplyPoint3x4(
                            vertices[indices[i + 2]]);

                    float area =
                        Vector3.Cross(
                            b - a,
                            c - a).magnitude *
                        0.5f;

                    if (area <= 0.00000001f)
                        continue;

                    totalArea += area;

                    triangles.Add(
                        new TriangleSample
                        {
                            a = a,
                            b = b,
                            c = c,
                            cumulativeArea =
                                (float)totalArea
                        });
                }
            }
        }

        // =========================================================
        // WORLD BOUNDS
        // =========================================================

        private void BuildWorldBounds()
        {
            worldBounds =
                new Bounds();

            bool initialized =
                false;

            for (int i = 0;
                 i < triangles.Count;
                 i++)
            {
                TriangleSample triangle =
                    triangles[i];

                EncapsulatePoint(
                    triangle.a,
                    ref worldBounds,
                    ref initialized);

                EncapsulatePoint(
                    triangle.b,
                    ref worldBounds,
                    ref initialized);

                EncapsulatePoint(
                    triangle.c,
                    ref worldBounds,
                    ref initialized);
            }
        }

        private static void EncapsulatePoint(
            Vector3 point,
            ref Bounds bounds,
            ref bool initialized)
        {
            if (!initialized)
            {
                bounds =
                    new Bounds(
                        point,
                        Vector3.zero);

                initialized = true;

                return;
            }

            bounds.Encapsulate(
                point);
        }
        [Header("Output Space")]
        [SerializeField]
        private Transform outputSpace;
        public void SetOutputSpace(Transform space)
        {
            outputSpace = space;
        }

        // =========================================================
        // POINT GENERATION
        // =========================================================

        private void GeneratePoints()
        {
            if (triangles.Count == 0)
                return;

            float totalArea =
                triangles[
                    triangles.Count - 1]
                    .cumulativeArea;

            for (int i = 0;
                 i < positions.Length;
                 i++)
            {
                TriangleSample triangle =
                    SelectTriangle(
                        totalArea);

                Vector3 point =
                    SampleTriangle(
                        triangle.a,
                        triangle.b,
                        triangle.c);

                Vector3 normal =
                    Vector3.Cross(
                        triangle.b - triangle.a,
                        triangle.c - triangle.a)
                    .normalized;

                if (surfaceOffset > 0f)
                {
                    point +=
                        normal *
                        surfaceOffset;
                }

                if (outputSpace != null)
                {
                    point =
                        outputSpace.InverseTransformPoint(point);
                }

                positions[i] = point;
            }
        }

        private TriangleSample SelectTriangle(
            float totalArea)
        {
            float randomArea =
                (float)random.NextDouble() *
                totalArea;

            int low = 0;
            int high =
                triangles.Count - 1;

            while (low < high)
            {
                int middle =
                    low +
                    ((high - low) >> 1);

                if (randomArea <=
                    triangles[middle]
                        .cumulativeArea)
                {
                    high = middle;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return triangles[low];
        }

        private Vector3 SampleTriangle(
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            float r1 =
                Mathf.Sqrt(
                    (float)random.NextDouble());

            float r2 =
                (float)random.NextDouble();

            float u =
                1f - r1;

            float v =
                r1 * (1f - r2);

            float w =
                r1 * r2;

            return
                a * u +
                b * v +
                c * w;
        }

        // =========================================================
        // TEMPORARY MESH CLEANUP
        // =========================================================

        private void CleanupTemporaryMeshes()
        {
            for (int i = 0;
                 i < meshSources.Count;
                 i++)
            {
                MeshSource source =
                    meshSources[i];

                if (!source.temporaryMesh ||
                    source.mesh == null)
                {
                    continue;
                }

                DestroyRuntimeMesh(
                    source.mesh);
            }
        }

        private static void DestroyRuntimeMesh(
            Mesh mesh)
        {
            if (mesh == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
        }
    }
}