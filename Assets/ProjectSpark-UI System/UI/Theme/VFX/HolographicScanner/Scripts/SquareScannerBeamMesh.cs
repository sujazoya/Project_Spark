using UnityEngine;

namespace ProjectSpark.VFX
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SquareScannerBeamMesh : MonoBehaviour
    {
        [Header("Geometry")]

        [Min(0.001f)]
        [SerializeField]
        private float height = 5f;

        [Min(0f)]
        [SerializeField]
        private float bottomWidth = 2f;

        [Min(0f)]
        [SerializeField]
        private float bottomDepth = 2f;

        [Min(0f)]
        [SerializeField]
        private float topWidth = 0.05f;

        [Min(0f)]
        [SerializeField]
        private float topDepth = 0.05f;

        [Header("Resolution")]

        [Range(1, 64)]
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
            Sanitize();

            Generate();
        }

        [ContextMenu("Generate Square Scanner Beam")]
        public void Generate()
        {
            CacheReferences();

            if (meshFilter == null)
            {
                return;
            }

            Sanitize();

            DestroyGeneratedMesh();

            int sideVertexCount =
                (heightSegments + 1) * 4;

            int bottomCapVertexCount =
                bottomCap
                    ? 4
                    : 0;

            int topCapVertexCount =
                topCap
                    ? 4
                    : 0;

            int totalVertexCount =
                sideVertexCount +
                bottomCapVertexCount +
                topCapVertexCount;

            int sideTriangleCount =
                heightSegments * 4 * 2;

            int bottomTriangleCount =
                bottomCap
                    ? 2
                    : 0;

            int topTriangleCount =
                topCap
                    ? 2
                    : 0;

            int totalTriangleCount =
                sideTriangleCount +
                bottomTriangleCount +
                topTriangleCount;

            Vector3[] vertices =
                new Vector3[totalVertexCount];

            Vector3[] normals =
                new Vector3[totalVertexCount];

            Vector2[] uvs =
                new Vector2[totalVertexCount];

            int[] triangles =
                new int[
                    totalTriangleCount * 3];

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
                new Mesh();

            generatedMesh.name =
                "ProjectSpark_SquareScannerBeam";

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
            float bottomHalfWidth =
                bottomWidth * 0.5f;

            float bottomHalfDepth =
                bottomDepth * 0.5f;

            float topHalfWidth =
                topWidth * 0.5f;

            float topHalfDepth =
                topDepth * 0.5f;

            for (int y = 0;
                 y <= heightSegments;
                 y++)
            {
                float t =
                    (float)y /
                    heightSegments;

                float currentY =
                    t * height;

                float halfWidth =
                    Mathf.Lerp(
                        bottomHalfWidth,
                        topHalfWidth,
                        t);

                float halfDepth =
                    Mathf.Lerp(
                        bottomHalfDepth,
                        topHalfDepth,
                        t);

                // Front-left
                vertices[vertexIndex] =
                    new Vector3(
                        -halfWidth,
                        currentY,
                        -halfDepth);

                uvs[vertexIndex] =
                    new Vector2(
                        0f,
                        t);

                vertexIndex++;

                // Front-right
                vertices[vertexIndex] =
                    new Vector3(
                        halfWidth,
                        currentY,
                        -halfDepth);

                uvs[vertexIndex] =
                    new Vector2(
                        1f,
                        t);

                vertexIndex++;

                // Back-right
                vertices[vertexIndex] =
                    new Vector3(
                        halfWidth,
                        currentY,
                        halfDepth);

                uvs[vertexIndex] =
                    new Vector2(
                        2f,
                        t);

                vertexIndex++;

                // Back-left
                vertices[vertexIndex] =
                    new Vector3(
                        -halfWidth,
                        currentY,
                        halfDepth);

                uvs[vertexIndex] =
                    new Vector2(
                        3f,
                        t);

                vertexIndex++;
            }

            GenerateSideNormals(
                vertices,
                normals);
        }

        private void GenerateSideNormals(
            Vector3[] vertices,
            Vector3[] normals)
        {
            float widthSlope =
                (bottomWidth -
                 topWidth) /
                (2f * height);

            float depthSlope =
                (bottomDepth -
                 topDepth) /
                (2f * height);

            for (int y = 0;
                 y <= heightSegments;
                 y++)
            {
                int index =
                    y * 4;

                // Front
                normals[index] =
                    new Vector3(
                        0f,
                        depthSlope,
                        -1f).normalized;

                normals[index + 1] =
                    new Vector3(
                        0f,
                        depthSlope,
                        -1f).normalized;

                // Right
                normals[index + 2] =
                    new Vector3(
                        1f,
                        widthSlope,
                        0f).normalized;

                normals[index + 3] =
                    new Vector3(
                        -1f,
                        widthSlope,
                        0f).normalized;
            }
        }

        private void GenerateSideTriangles(
            int[] triangles,
            ref int triangleIndex)
        {
            for (int y = 0;
                 y < heightSegments;
                 y++)
            {
                int current =
                    y * 4;

                int next =
                    (y + 1) * 4;

                // Front
                AddQuad(
                    triangles,
                    ref triangleIndex,
                    current,
                    current + 1,
                    next,
                    next + 1);

                // Right
                AddQuad(
                    triangles,
                    ref triangleIndex,
                    current + 1,
                    current + 2,
                    next + 1,
                    next + 2);

                // Back
                AddQuad(
                    triangles,
                    ref triangleIndex,
                    current + 2,
                    current + 3,
                    next + 2,
                    next + 3);

                // Left
                AddQuad(
                    triangles,
                    ref triangleIndex,
                    current + 3,
                    current,
                    next + 3,
                    next);
            }
        }

        private void AddQuad(
            int[] triangles,
            ref int triangleIndex,
            int bottomLeft,
            int bottomRight,
            int topLeft,
            int topRight)
        {
            triangles[triangleIndex++] =
                bottomLeft;

            triangles[triangleIndex++] =
                topRight;

            triangles[triangleIndex++] =
                topLeft;

            triangles[triangleIndex++] =
                bottomLeft;

            triangles[triangleIndex++] =
                bottomRight;

            triangles[triangleIndex++] =
                topRight;
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
            float halfWidth =
                (top
                    ? topWidth
                    : bottomWidth) * 0.5f;

            float halfDepth =
                (top
                    ? topDepth
                    : bottomDepth) * 0.5f;

            float y =
                top
                    ? height
                    : 0f;

            Vector3 normal =
                top
                    ? Vector3.up
                    : Vector3.down;

            int start =
                vertexIndex;

            vertices[vertexIndex] =
                new Vector3(
                    -halfWidth,
                    y,
                    -halfDepth);

            normals[vertexIndex] =
                normal;

            uvs[vertexIndex++] =
                new Vector2(0f, 0f);

            vertices[vertexIndex] =
                new Vector3(
                    halfWidth,
                    y,
                    -halfDepth);

            normals[vertexIndex] =
                normal;

            uvs[vertexIndex++] =
                new Vector2(1f, 0f);

            vertices[vertexIndex] =
                new Vector3(
                    halfWidth,
                    y,
                    halfDepth);

            normals[vertexIndex] =
                normal;

            uvs[vertexIndex++] =
                new Vector2(1f, 1f);

            vertices[vertexIndex] =
                new Vector3(
                    -halfWidth,
                    y,
                    halfDepth);

            normals[vertexIndex] =
                normal;

            uvs[vertexIndex++] =
                new Vector2(0f, 1f);

            if (top)
            {
                triangles[triangleIndex++] =
                    start;

                triangles[triangleIndex++] =
                    start + 2;

                triangles[triangleIndex++] =
                    start + 1;

                triangles[triangleIndex++] =
                    start;

                triangles[triangleIndex++] =
                    start + 3;

                triangles[triangleIndex++] =
                    start + 2;
            }
            else
            {
                triangles[triangleIndex++] =
                    start;

                triangles[triangleIndex++] =
                    start + 1;

                triangles[triangleIndex++] =
                    start + 2;

                triangles[triangleIndex++] =
                    start;

                triangles[triangleIndex++] =
                    start + 2;

                triangles[triangleIndex++] =
                    start + 3;
            }
        }

        private void CacheReferences()
        {
            if (meshFilter == null)
            {
                meshFilter =
                    GetComponent<MeshFilter>();
            }
        }

        private void Sanitize()
        {
            height =
                Mathf.Max(
                    0.001f,
                    height);

            bottomWidth =
                Mathf.Max(
                    0f,
                    bottomWidth);

            bottomDepth =
                Mathf.Max(
                    0f,
                    bottomDepth);

            topWidth =
                Mathf.Max(
                    0f,
                    topWidth);

            topDepth =
                Mathf.Max(
                    0f,
                    topDepth);

            heightSegments =
                Mathf.Clamp(
                    heightSegments,
                    1,
                    64);

            uvTilingX =
                Mathf.Max(
                    0.001f,
                    uvTilingX);

            uvTilingY =
                Mathf.Max(
                    0.001f,
                    uvTilingY);
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