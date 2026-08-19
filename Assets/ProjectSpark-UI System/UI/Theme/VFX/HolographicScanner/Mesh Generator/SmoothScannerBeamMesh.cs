using UnityEngine;

namespace ProjectSpark.VFX
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SmoothScannerBeamMesh : MonoBehaviour
    {
        [Header("Geometry")]

        [Min(0.001f)]
        [SerializeField]
        private float height = 5f;

        [Min(0f)]
        [SerializeField]
        private float bottomRadius = 0.5f;

        [Min(0f)]
        [SerializeField]
        private float topRadius = 0.02f;

        [Header("Resolution")]

        [Range(16, 256)]
        [SerializeField]
        private int radialSegments = 64;

        [Range(1, 32)]
        [SerializeField]
        private int heightSegments = 8;

        [Header("Caps")]

        [SerializeField]
        private bool bottomCap;

        [SerializeField]
        private bool topCap;

        [Header("UV")]

        [SerializeField]
        private float uvTilingX = 1f;

        [SerializeField]
        private float uvTilingY = 1f;

        private MeshFilter meshFilter;
        private Mesh generatedMesh;

        private void Awake()
        {
            Generate();
        }

        private void OnEnable()
        {
            Generate();
        }

        private void OnValidate()
        {
            height =
                Mathf.Max(
                    0.001f,
                    height);

            bottomRadius =
                Mathf.Max(
                    0f,
                    bottomRadius);

            topRadius =
                Mathf.Max(
                    0f,
                    topRadius);

            radialSegments =
                Mathf.Clamp(
                    radialSegments,
                    16,
                    256);

            heightSegments =
                Mathf.Clamp(
                    heightSegments,
                    1,
                    32);

            uvTilingX =
                Mathf.Max(
                    0.001f,
                    uvTilingX);

            uvTilingY =
                Mathf.Max(
                    0.001f,
                    uvTilingY);

            Generate();
        }

        [ContextMenu("Generate Scanner Beam")]
        public void Generate()
        {
            if (meshFilter == null)
            {
                meshFilter =
                    GetComponent<MeshFilter>();
            }

            if (meshFilter == null)
            {
                return;
            }

            DestroyGeneratedMesh();

            int ringVertexCount =
                radialSegments + 1;

            int sideVertexCount =
                (heightSegments + 1) *
                ringVertexCount;

            int bottomCapVertexCount =
                bottomCap
                    ? ringVertexCount + 1
                    : 0;

            int topCapVertexCount =
                topCap
                    ? ringVertexCount + 1
                    : 0;

            int vertexCount =
                sideVertexCount +
                bottomCapVertexCount +
                topCapVertexCount;

            int sideTriangleCount =
                heightSegments *
                radialSegments *
                2;

            int bottomTriangleCount =
                bottomCap
                    ? radialSegments
                    : 0;

            int topTriangleCount =
                topCap
                    ? radialSegments
                    : 0;

            int triangleIndexCount =
                (sideTriangleCount +
                 bottomTriangleCount +
                 topTriangleCount) *
                3;

            Vector3[] vertices =
                new Vector3[vertexCount];

            Vector3[] normals =
                new Vector3[vertexCount];

            Vector2[] uvs =
                new Vector2[vertexCount];

            int[] triangles =
                new int[triangleIndexCount];

            int vertexIndex = 0;

            GenerateSideVertices(
                vertices,
                normals,
                uvs,
                ref vertexIndex);

            int triangleIndex = 0;

            GenerateSideTriangles(
                triangles,
                ref triangleIndex);

            if (bottomCap)
            {
                GenerateCap(
                    vertices,
                    normals,
                    uvs,
                    triangles,
                    ref vertexIndex,
                    ref triangleIndex,
                    false);
            }

            if (topCap)
            {
                GenerateCap(
                    vertices,
                    normals,
                    uvs,
                    triangles,
                    ref vertexIndex,
                    ref triangleIndex,
                    true);
            }

            generatedMesh =
                new Mesh
                {
                    name =
                        "ProjectSpark_SmoothScannerBeam"
                };

            generatedMesh.vertices =
                vertices;

            generatedMesh.normals =
                normals;

            generatedMesh.uv =
                uvs;

            generatedMesh.triangles =
                triangles;

            generatedMesh.RecalculateBounds();

            meshFilter.sharedMesh =
                generatedMesh;
        }

        private void GenerateSideVertices(
            Vector3[] vertices,
            Vector3[] normals,
            Vector2[] uvs,
            ref int vertexIndex)
        {
            float radiusSlope =
                (topRadius -
                 bottomRadius) /
                height;

            for (int y = 0;
                 y <= heightSegments;
                 y++)
            {
                float verticalT =
                    (float)y /
                    heightSegments;

                float localY =
                    verticalT *
                    height;

                float radius =
                    Mathf.Lerp(
                        bottomRadius,
                        topRadius,
                        verticalT);

                for (int x = 0;
                     x <= radialSegments;
                     x++)
                {
                    float angleT =
                        (float)x /
                        radialSegments;

                    float angle =
                        angleT *
                        Mathf.PI *
                        2f;

                    float cos =
                        Mathf.Cos(angle);

                    float sin =
                        Mathf.Sin(angle);

                    vertices[vertexIndex] =
                        new Vector3(
                            cos * radius,
                            localY,
                            sin * radius);

                    Vector3 normal =
                        new Vector3(
                            cos,
                            -radiusSlope,
                            sin);

                    normals[vertexIndex] =
                        normal.normalized;

                    uvs[vertexIndex] =
                        new Vector2(
                            angleT * uvTilingX,
                            verticalT * uvTilingY);

                    vertexIndex++;
                }
            }
        }

        private void GenerateSideTriangles(
            int[] triangles,
            ref int triangleIndex)
        {
            int ringVertexCount =
                radialSegments + 1;

            for (int y = 0;
                 y < heightSegments;
                 y++)
            {
                int currentRing =
                    y * ringVertexCount;

                int nextRing =
                    (y + 1) *
                    ringVertexCount;

                for (int x = 0;
                     x < radialSegments;
                     x++)
                {
                    int a =
                        currentRing + x;

                    int b =
                        currentRing + x + 1;

                    int c =
                        nextRing + x;

                    int d =
                        nextRing + x + 1;

                    triangles[triangleIndex++] =
                        a;

                    triangles[triangleIndex++] =
                        d;

                    triangles[triangleIndex++] =
                        c;

                    triangles[triangleIndex++] =
                        a;

                    triangles[triangleIndex++] =
                        b;

                    triangles[triangleIndex++] =
                        d;
                }
            }
        }

        private void GenerateCap(
            Vector3[] vertices,
            Vector3[] normals,
            Vector2[] uvs,
            int[] triangles,
            ref int vertexIndex,
            ref int triangleIndex,
            bool top)
        {
            float radius =
                top
                    ? topRadius
                    : bottomRadius;

            float y =
                top
                    ? height
                    : 0f;

            Vector3 normal =
                top
                    ? Vector3.up
                    : Vector3.down;

            int ringStart =
                vertexIndex;

            for (int x = 0;
                 x <= radialSegments;
                 x++)
            {
                float angleT =
                    (float)x /
                    radialSegments;

                float angle =
                    angleT *
                    Mathf.PI *
                    2f;

                float cos =
                    Mathf.Cos(angle);

                float sin =
                    Mathf.Sin(angle);

                vertices[vertexIndex] =
                    new Vector3(
                        cos * radius,
                        y,
                        sin * radius);

                normals[vertexIndex] =
                    normal;

                uvs[vertexIndex] =
                    new Vector2(
                        cos * 0.5f + 0.5f,
                        sin * 0.5f + 0.5f);

                vertexIndex++;
            }

            int centerIndex =
                vertexIndex;

            vertices[centerIndex] =
                new Vector3(
                    0f,
                    y,
                    0f);

            normals[centerIndex] =
                normal;

            uvs[centerIndex] =
                new Vector2(
                    0.5f,
                    0.5f);

            vertexIndex++;

            for (int x = 0;
                 x < radialSegments;
                 x++)
            {
                int a =
                    ringStart + x;

                int b =
                    ringStart + x + 1;

                if (top)
                {
                    triangles[triangleIndex++] =
                        centerIndex;

                    triangles[triangleIndex++] =
                        a;

                    triangles[triangleIndex++] =
                        b;
                }
                else
                {
                    triangles[triangleIndex++] =
                        centerIndex;

                    triangles[triangleIndex++] =
                        b;

                    triangles[triangleIndex++] =
                        a;
                }
            }
        }

        private void DestroyGeneratedMesh()
        {
            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(
                    generatedMesh);
            }

            generatedMesh = null;
        }
    }
}