using System.Collections.Generic;
using UnityEngine;

namespace AAAUI.VFX
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SignalPathMesh : MonoBehaviour
    {
        [Header("Wire")]
        [SerializeField, Min(0.001f)]
        private float radius = 0.018f;

        [SerializeField, Range(6, 24)]
        private int sides = 12;

        [Header("Curve")]
        [SerializeField]
        private bool smoothCurve = true;

        [SerializeField, Range(2, 32)]
        private int subdivisionsPerSegment = 8;

        [SerializeField, Range(0f, 1f)]
        private float curveTension = 0.35f;

        [Header("Cable")]
        [SerializeField, Min(0f)]
        private float sag = 0.015f;

        [Header("Mesh")]
        [SerializeField]
        private bool recalculateBounds = true;

        // THIS wire's path.
        // No dependency on SignalWireBuilder.
        private SignalPath path;

        private Mesh mesh;

        private readonly List<Vector3> curvePoints =
            new List<Vector3>();

        private readonly List<float> curveDistances =
            new List<float>();

        // =========================================================
        // PATH
        // =========================================================

        public void SetPath(
            SignalPath newPath)
        {
            path =
                newPath;

            Rebuild();
        }

        public SignalPath GetPath()
        {
            return path;
        }

        // =========================================================
        // LIFECYCLE
        // =========================================================

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            sides =
                Mathf.Clamp(
                    sides,
                    6,
                    24
                );

            subdivisionsPerSegment =
                Mathf.Clamp(
                    subdivisionsPerSegment,
                    2,
                    32
                );

            curveTension =
                Mathf.Clamp01(
                    curveTension
                );

            radius =
                Mathf.Max(
                    0.001f,
                    radius
                );

            Rebuild();
        }

        // =========================================================
        // REBUILD
        // =========================================================

        public void Rebuild()
        {
            if (path == null)
            {
                ClearMesh();
                return;
            }

            Vector3[] sourcePoints =
                path.GetRenderPoints();

            if (sourcePoints == null ||
                sourcePoints.Length < 2)
            {
                ClearMesh();
                return;
            }

            BuildCurve(
                sourcePoints
            );

            if (curvePoints.Count < 2)
            {
                ClearMesh();
                return;
            }

            EnsureMesh();

            mesh.Clear();

            BuildTube();

            if (recalculateBounds)
            {
                mesh.RecalculateBounds();
            }
        }

        // =========================================================
        // CURVE
        // =========================================================


        private void BuildCurve(
            Vector3[] points)
        {
            curvePoints.Clear();
            curveDistances.Clear();

            if (points == null ||
                points.Length < 2)
            {
                return;
            }

            if (!smoothCurve ||
                points.Length == 2)
            {
                for (int i = 0;
                     i < points.Length;
                     i++)
                {
                    curvePoints.Add(
                        points[i]
                    );
                }

                CalculateDistances();

                return;
            }

            int count =
                points.Length;

            for (int segment = 0;
                 segment < count - 1;
                 segment++)
            {
                Vector3 p0 =
                    segment == 0
                        ? points[segment]
                        : points[segment - 1];

                Vector3 p1 =
                    points[segment];

                Vector3 p2 =
                    points[segment + 1];

                Vector3 p3 =
                    segment + 2 < count
                        ? points[segment + 2]
                        : points[segment + 1];

                for (int step = 0;
                     step < subdivisionsPerSegment;
                     step++)
                {
                    if (segment > 0 &&
                        step == 0)
                    {
                        continue;
                    }

                    float t =
                        step /
                        (float)subdivisionsPerSegment;

                    Vector3 position =
                        CatmullRom(
                            p0,
                            p1,
                            p2,
                            p3,
                            t,
                            curveTension
                        );

                    float sagFactor =
                        Mathf.Sin(
                            t * Mathf.PI
                        );

                    position -=
                        transform.up *
                        sag *
                        sagFactor;

                    curvePoints.Add(
                        position
                    );
                }
            }

            curvePoints.Add(
                points[count - 1]
            );

            CalculateDistances();
        }

        private static Vector3 CatmullRom(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float t,
            float tension)
        {
            float t2 =
                t * t;

            float t3 =
                t2 * t;

            float tangentScale =
                (1f - tension) * 0.5f;

            Vector3 m1 =
                (p2 - p0) *
                tangentScale;

            Vector3 m2 =
                (p3 - p1) *
                tangentScale;

            float h00 =
                2f * t3 -
                3f * t2 +
                1f;

            float h10 =
                t3 -
                2f * t2 +
                t;

            float h01 =
                -2f * t3 +
                3f * t2;

            float h11 =
                t3 -
                t2;

            return
                h00 * p1 +
                h10 * m1 +
                h01 * p2 +
                h11 * m2;
        }

        private void CalculateDistances()
        {
            curveDistances.Clear();

            float total =
                0f;

            curveDistances.Add(
                0f
            );

            for (int i = 1;
                 i < curvePoints.Count;
                 i++)
            {
                total +=
                    Vector3.Distance(
                        curvePoints[i - 1],
                        curvePoints[i]
                    );

                curveDistances.Add(
                    total
                );
            }
        }

        // =========================================================
        // TUBE
        // =========================================================

        private void BuildTube()
        {
            int pointCount =
                curvePoints.Count;

            int vertexCount =
                pointCount *
                sides;

            Vector3[] vertices =
                new Vector3[vertexCount];

            Vector3[] normals =
                new Vector3[vertexCount];

            Vector2[] uv =
                new Vector2[vertexCount];

            int[] triangles =
                new int[
                    (pointCount - 1) *
                    sides *
                    6
                ];

            Vector3 previousNormal =
                Vector3.zero;

            Vector3 previousTangent =
                Vector3.zero;

            for (int i = 0;
                 i < pointCount;
                 i++)
            {
                Vector3 tangent =
                    GetTangent(i);

                tangent.Normalize();

                Vector3 normal;
                Vector3 binormal;

                if (i == 0)
                {
                    BuildInitialFrame(
                        tangent,
                        out normal,
                        out binormal
                    );
                }
                else
                {
                    normal =
                        TransportNormal(
                            previousNormal,
                            previousTangent,
                            tangent
                        );

                    binormal =
                        Vector3.Cross(
                            tangent,
                            normal
                        ).normalized;

                    normal =
                        Vector3.Cross(
                            binormal,
                            tangent
                        ).normalized;
                }

                previousNormal =
                    normal;

                previousTangent =
                    tangent;

                float u =
                    curveDistances[i] /
                    Mathf.Max(
                        curveDistances[
                            curveDistances.Count - 1
                        ],
                        0.0001f
                    );

                for (int side = 0;
                     side < sides;
                     side++)
                {
                    float angle =
                        side /
                        (float)sides *
                        Mathf.PI *
                        2f;

                    float cos =
                        Mathf.Cos(angle);

                    float sin =
                        Mathf.Sin(angle);

                    Vector3 radial =
                        normal * cos +
                        binormal * sin;

                    Vector3 worldPosition =
                        curvePoints[i] +
                        radial * radius;

                    int index =
                        i * sides +
                        side;

                    vertices[index] =
                        transform.InverseTransformPoint(
                            worldPosition
                        );

                    normals[index] =
                        transform.InverseTransformDirection(
                            radial
                        ).normalized;

                    // 0 → 1 along the actual wire length.
                    uv[index] =
                        new Vector2(
                            u,
                            side /
                            (float)sides
                        );
                }
            }

            BuildTriangles(
                triangles,
                pointCount
            );

            mesh.vertices =
                vertices;

            mesh.normals =
                normals;

            mesh.uv =
                uv;

            mesh.triangles =
                triangles;
        }

        private Vector3 GetTangent(
            int index)
        {
            if (index == 0)
            {
                return
                    curvePoints[1] -
                    curvePoints[0];
            }

            if (index ==
                curvePoints.Count - 1)
            {
                return
                    curvePoints[index] -
                    curvePoints[index - 1];
            }

            return
                curvePoints[index + 1] -
                curvePoints[index - 1];
        }

        private static void BuildInitialFrame(
            Vector3 tangent,
            out Vector3 normal,
            out Vector3 binormal)
        {
            Vector3 reference =
                Vector3.up;

            if (Mathf.Abs(
                    Vector3.Dot(
                        tangent,
                        reference
                    )) > 0.95f)
            {
                reference =
                    Vector3.right;
            }

            binormal =
                Vector3.Cross(
                    tangent,
                    reference
                ).normalized;

            normal =
                Vector3.Cross(
                    binormal,
                    tangent
                ).normalized;
        }

        private static Vector3 TransportNormal(
            Vector3 previousNormal,
            Vector3 previousTangent,
            Vector3 currentTangent)
        {
            Vector3 axis =
                Vector3.Cross(
                    previousTangent,
                    currentTangent
                );

            float axisLength =
                axis.magnitude;

            if (axisLength < 0.000001f)
            {
                return previousNormal.normalized;
            }

            axis /=
                axisLength;

            float angle =
                Vector3.Angle(
                    previousTangent,
                    currentTangent
                );

            Quaternion rotation =
                Quaternion.AngleAxis(
                    angle,
                    axis
                );

            Vector3 result =
                rotation *
                previousNormal;

            result =
                Vector3.ProjectOnPlane(
                    result,
                    currentTangent
                );

            if (result.sqrMagnitude <
                0.000001f)
            {
                BuildInitialFrame(
                    currentTangent,
                    out result,
                    out _
                );
            }

            return result.normalized;
        }

        private void BuildTriangles(
            int[] triangles,
            int pointCount)
        {
            int index = 0;

            for (int i = 0;
                 i < pointCount - 1;
                 i++)
            {
                for (int side = 0;
                     side < sides;
                     side++)
                {
                    int nextSide =
                        (side + 1) % sides;

                    int current =
                        i * sides +
                        side;

                    int currentNext =
                        i * sides +
                        nextSide;

                    int next =
                        (i + 1) * sides +
                        side;

                    int nextNext =
                        (i + 1) * sides +
                        nextSide;

                    triangles[index++] =
                        current;

                    triangles[index++] =
                        next;

                    triangles[index++] =
                        currentNext;

                    triangles[index++] =
                        currentNext;

                    triangles[index++] =
                        next;

                    triangles[index++] =
                        nextNext;
                }
            }
        }

        // =========================================================
        // MESH
        // =========================================================

        private void EnsureMesh()
        {
            MeshFilter filter =
                GetComponent<MeshFilter>();

            if (filter.sharedMesh == null)
            {
                mesh =
                    new Mesh
                    {
                        name =
                            "SignalPathMesh"
                    };

                filter.sharedMesh =
                    mesh;
            }
            else
            {
                mesh =
                    filter.sharedMesh;
            }
        }

        private void ClearMesh()
        {
            MeshFilter filter =
                GetComponent<MeshFilter>();

            if (filter != null &&
                filter.sharedMesh != null)
            {
                filter.sharedMesh.Clear();
            }
        }

        // =========================================================
        // MATERIAL
        // =========================================================

        public void SetMaterial(
            Material material)
        {
            MeshRenderer renderer =
                GetComponent<MeshRenderer>();

            if (renderer == null)
                return;

            renderer.sharedMaterial =
                material;
        }
        public void SetScannerOverlayMaterial(
     Material scannerMaterial)
        {
            MeshRenderer renderer =
                GetComponent<MeshRenderer>();

            if (renderer == null ||
                scannerMaterial == null)
                return;

            Material[] materials =
                renderer.sharedMaterials;

            if (materials.Length == 0)
                return;

            if (materials.Length == 1)
            {
                renderer.sharedMaterials = new[]
                {
            materials[0],
            scannerMaterial
        };

                return;
            }

            materials[1] = scannerMaterial;
            renderer.sharedMaterials = materials;
        }
    }
}