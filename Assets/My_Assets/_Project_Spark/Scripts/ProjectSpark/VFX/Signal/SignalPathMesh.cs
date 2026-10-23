using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAAUI.VFX
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class SignalPathMesh : MonoBehaviour
    {
        [Header("Physical Insulation")]
        [SerializeField, Min(0.001f)] private float radius = 0.018f;
        [SerializeField, Range(6, 32)] private int sides = 16;
        [SerializeField, Min(0.001f)] private float uvMetersPerTile = 0.25f;
        [SerializeField] private bool capEnds = true;

        [Header("Curve")]
        [SerializeField] private bool smoothCurve = true;
        [SerializeField, Range(2, 32)] private int subdivisionsPerSegment = 8;
        [SerializeField, Range(0f, 1f)] private float curveTension = 0.35f;
        [SerializeField, Min(0f)] private float sag = 0.015f;

        [Header("Mesh")]
        [SerializeField] private bool recalculateBounds = true;

        [SerializeField, HideInInspector] private SignalPath path;
        private Mesh mesh;
        private readonly List<Vector3> curve = new List<Vector3>(128);
        private readonly List<float> distances = new List<float>(128);
        private const float Epsilon = 0.000001f;

        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        public MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
        public MeshRenderer ScannerRenderer => MeshRenderer;
        public Mesh GeneratedMesh => mesh;

                [Header("Cable Routing")]
        [SerializeField, Min(0f)]
        private float cableArchHeight = 0.08f;

        public void SetPath(SignalPath newPath)
        {
            path = newPath;
            Rebuild();
        }

        public SignalPath GetPath() => path;

        private void OnEnable()
        {
            if (path == null) path = GetComponent<SignalPath>();
            Rebuild();
        }

        private void OnValidate()
        {
            radius = Mathf.Max(0.001f, radius);
            sides = Mathf.Clamp(sides, 6, 32);
            subdivisionsPerSegment = Mathf.Clamp(subdivisionsPerSegment, 2, 32);
            curveTension = Mathf.Clamp01(curveTension);
            uvMetersPerTile = Mathf.Max(0.001f, uvMetersPerTile);
            sag = Mathf.Max(0f, sag);
            // Do not create meshes while Unity is importing a prefab asset.
            if (isActiveAndEnabled && gameObject.scene.IsValid()) Rebuild();
        }

        public void Rebuild()
        {
            if (!gameObject.scene.IsValid()) return;
            if (path == null) path = GetComponent<SignalPath>();
            if (path == null) { ClearMesh(); return; }
            Vector3[] points = path.GetRenderPoints();
            if (points == null || points.Length < 2) { ClearMesh(); return; }
            BuildCurve(points);
            if (curve.Count < 2) { ClearMesh(); return; }
            EnsureMesh();
            if (mesh == null) return;
            BuildTube();
        }

       
private void BuildCurve(Vector3[] points)
{
    curve.Clear();
    distances.Clear();

    if (points == null || points.Length < 2)
        return;

    // =========================================================
    // 1. REMOVE DUPLICATE POINTS
    // =========================================================

    List<Vector3> controls =
        new List<Vector3>(points.Length);

    for (int i = 0; i < points.Length; i++)
    {
        Vector3 point = points[i];

        if (controls.Count == 0 ||
            (controls[controls.Count - 1] - point)
                .sqrMagnitude > Epsilon)
        {
            controls.Add(point);
        }
    }

    if (controls.Count < 2)
        return;

    // =========================================================
    // 2. AUTOMATIC PHYSICAL CABLE ARCH
    // =========================================================
    // When there are only two endpoints, insert one gentle
    // elevated control point.
    //
    // This produces a natural cable shape without creating
    // unwanted zigzags.
    // =========================================================

    if (smoothCurve &&
        controls.Count == 2 &&
        cableArchHeight > 0f)
    {
        Vector3 start = controls[0];
        Vector3 end = controls[1];

        Vector3 midpoint =
            (start + end) * 0.5f;

        midpoint += Vector3.up * cableArchHeight;

        controls.Insert(1, midpoint);
    }

    // =========================================================
    // 3. GENERATE SMOOTH CURVE
    // =========================================================

    int steps = smoothCurve
        ? Mathf.Max(2, subdivisionsPerSegment)
        : 1;

    for (int segment = 0;
         segment < controls.Count - 1;
         segment++)
    {
        Vector3 p0 =
            controls[Mathf.Max(0, segment - 1)];

        Vector3 p1 =
            controls[segment];

        Vector3 p2 =
            controls[segment + 1];

        Vector3 p3 =
            controls[Mathf.Min(
                controls.Count - 1,
                segment + 2)];

        for (int step = 0; step < steps; step++)
        {
            float t = (float)step / steps;

            Vector3 position;

            if (smoothCurve)
            {
                position = CatmullRom(
                    p0,
                    p1,
                    p2,
                    p3,
                    t);
            }
            else
            {
                position = Vector3.Lerp(
                    p1,
                    p2,
                    t);
            }

            // Optional sag.
            // Keep at zero for a clean elevated cable.
            if (sag > 0f)
            {
                float sagFactor =
                    Mathf.Sin(t * Mathf.PI);

                position += Vector3.down *
                    sag * sagFactor;
            }

            if (curve.Count == 0 ||
                (curve[curve.Count - 1] - position)
                    .sqrMagnitude > Epsilon)
            {
                curve.Add(position);
            }
        }
    }

    // =========================================================
    // 4. PRESERVE EXACT ENDPOINT
    // =========================================================

    Vector3 finalPoint =
        controls[controls.Count - 1];

    if (curve.Count == 0 ||
        (curve[curve.Count - 1] - finalPoint)
            .sqrMagnitude > Epsilon)
    {
        curve.Add(finalPoint);
    }

    // =========================================================
    // 5. CALCULATE ACTUAL CABLE LENGTH
    // =========================================================

    if (curve.Count == 0)
        return;

    float totalDistance = 0f;

    distances.Add(0f);

    for (int i = 1; i < curve.Count; i++)
    {
        totalDistance += Vector3.Distance(
            curve[i - 1],
            curve[i]);

        distances.Add(totalDistance);
    }
}


        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            float s = (1f - curveTension) * 0.5f;
            Vector3 m1 = (p2 - p0) * s;
            Vector3 m2 = (p3 - p1) * s;
            return (2f*t3 - 3f*t2 + 1f)*p1 + (t3 - 2f*t2 + t)*m1 + (-2f*t3 + 3f*t2)*p2 + (t3 - t2)*m2;
        }

        private Vector3 Tangent(int i)
        {
            Vector3 d = i == 0 ? curve[1] - curve[0] :
                i == curve.Count - 1 ? curve[i] - curve[i - 1] : curve[i + 1] - curve[i - 1];
            return d.sqrMagnitude > Epsilon ? d.normalized : Vector3.forward;
        }

        private static Vector3 InitialNormal(Vector3 tangent)
        {
            Vector3 reference = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;
            return Vector3.ProjectOnPlane(reference, tangent).normalized;
        }

        private void BuildTube()
        {
            int rings = curve.Count;
            int stride = sides + 1; // duplicated seam vertex for correct UV wrapping
            int bodyVertices = rings * stride;
            int extraVertices = capEnds ? 2 : 0;
            var vertices = new Vector3[bodyVertices + extraVertices];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(rings - 1) * sides * 6 + (capEnds ? sides * 6 : 0)];

            Vector3 prevTangent = Tangent(0);
            Vector3 normal = InitialNormal(prevTangent);
            for (int i = 0; i < rings; i++)
            {
                Vector3 tangent = Tangent(i);
                if (i > 0)
                {
                    Quaternion q = Quaternion.FromToRotation(prevTangent, tangent);
                    normal = Vector3.ProjectOnPlane(q * normal, tangent);
                    if (normal.sqrMagnitude < Epsilon) normal = InitialNormal(tangent);
                    normal.Normalize();
                }
                Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
                for (int side = 0; side <= sides; side++)
                {
                    float v = (float)side / sides;
                    float angle = v * Mathf.PI * 2f;
                    Vector3 radial = normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle);
                    int index = i * stride + side;
                    vertices[index] =
    transform.InverseTransformPoint(
        curve[i] +
        Vector3.up * 0.08f +
        radial * radius);
                    normals[index] = transform.InverseTransformDirection(radial).normalized;
                    uv[index] = new Vector2(distances[i] / uvMetersPerTile, v);
                }
                prevTangent = tangent;
            }           
           
                int tri = 0;

                for (int i = 0; i < rings - 1; i++)
                {
                    for (int side = 0; side < sides; side++)
                    {
                        int a = i * stride + side;
                        int b = a + stride;

                        // Outward-facing tube triangles.
                        triangles[tri++] = a;
                        triangles[tri++] = a + 1;
                        triangles[tri++] = b;

                        triangles[tri++] = a + 1;
                        triangles[tri++] = b + 1;
                        triangles[tri++] = b;
                    }
                }


            

if (capEnds)
{
    int startCenter = bodyVertices;
    int endCenter = bodyVertices + 1;

    vertices[startCenter] =
        transform.InverseTransformPoint(curve[0]);

    vertices[endCenter] =
        transform.InverseTransformPoint(curve[rings - 1]);

    normals[startCenter] =
        transform.InverseTransformDirection(-Tangent(0)).normalized;

    normals[endCenter] =
        transform.InverseTransformDirection(Tangent(rings - 1)).normalized;

    uv[startCenter] = new Vector2(0.5f, 0.5f);
    uv[endCenter] = new Vector2(0.5f, 0.5f);

    int last = (rings - 1) * stride;

    for (int side = 0; side < sides; side++)
    {
        // Start cap.
        triangles[tri++] = startCenter;
        triangles[tri++] = side;
        triangles[tri++] = side + 1;

        // End cap.
        triangles[tri++] = endCenter;
        triangles[tri++] = last + side + 1;
        triangles[tri++] = last + side;
    }
}



            mesh.Clear();
            mesh.indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateTangents();
            if (recalculateBounds) mesh.RecalculateBounds();
        }

        private void EnsureMesh()
        {
            if (mesh != null) return;
            MeshFilter filter = MeshFilter;
            mesh = new Mesh { name = "ProjectSpark_Cable_" + GetInstanceID(), hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
        }

        private void ClearMesh()
        {
            if (mesh != null) mesh.Clear();
        }

        private void OnDestroy()
        {
            if (mesh == null) return;
            if (MeshFilter != null && MeshFilter.sharedMesh == mesh) MeshFilter.sharedMesh = null;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
            mesh = null;
        }

        public void SetMaterial(Material material)
        {
            if (MeshRenderer != null) MeshRenderer.sharedMaterial = material;
        }

        public void SetScannerOverlayMaterial(Material scannerMaterial)
        {
            if (MeshRenderer == null || scannerMaterial == null) return;
            Material[] materials = MeshRenderer.sharedMaterials;
            if (materials.Length == 0) return;
            if (materials.Length == 1) MeshRenderer.sharedMaterials = new[] { materials[0], scannerMaterial };
            else { materials[1] = scannerMaterial; MeshRenderer.sharedMaterials = materials; }
            // Note: the mesh has one submesh. For a reliable independent overlay use a second renderer.
        }
    }
}
