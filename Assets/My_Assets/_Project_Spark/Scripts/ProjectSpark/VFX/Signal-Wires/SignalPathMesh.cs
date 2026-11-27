using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAAUI.VFX
{
    /// <summary>New cable-only shapes. Legacy mesh routing is unaffected.</summary>
    public enum TerminalCableRouting
    {
        Straight = 0,
        ElevatedArc = 1,
        Orthogonal = 2,
        // Preserve each mouse-drag waypoint, with gentle smoothing.
        FreehandDrag = 3
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class SignalPathMesh : MonoBehaviour
    {
        [Header("Physical Insulation")]
        [SerializeField, Min(0.001f)] private float radius = 0.018f;
        public float SimulatedEndDiameter => radius * 2.6f;
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
        private Vector3[] cableVertices, cableNormals;
        private Vector2[] cableUV;
        private int[] cableTriangles;
        private const float Epsilon = 0.000001f;

        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        public MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
        public MeshRenderer ScannerRenderer => MeshRenderer;
        public Mesh GeneratedMesh => mesh;

                [Header("Cable Routing")]
        [SerializeField, Min(0f)]
        private float cableArchHeight = 0.08f;

        [Header("Terminal Freehand - Visual Cleanup")]
        [Tooltip("Maximum deviation in world units when removing drag jitter.")]
        [SerializeField, Min(0f)] private float freehandSimplifyTolerance = 0.006f;
        [Tooltip("Blend each recorded bend without pulling the line far from the mouse route.")]
        [SerializeField, Range(0f, 0.45f)] private float freehandCornerRoundness = 0.18f;

        // Only enabled for newly created TerminalToTerminal wires.
        // None of these fields change legacy-prefab appearance.
        private bool useTerminalCable;
        private bool useSimulatedTerminalShape;
        private TerminalCableRouting terminalRouting;
        private Transform anchor1;
        private Transform anchor2;
        private bool followAnchors;
        private Vector3 lastAnchor1;
        private Vector3 lastAnchor2;
        private const float AnchorMoveEpsilonSquared = 0.00000004f;

        // A non-destructive world-space visual route. The original SignalPath
        // remains the electrical/legacy drawing source; editing never deletes it.
        // Registry snapshots and the editor read this override when present.
        [SerializeField, HideInInspector] private bool hasManualRoute;
        [SerializeField, HideInInspector] private Vector3[] manualRoute = Array.Empty<Vector3>();
        private bool originalManualSourceWasTerminalCable;
        private TerminalCableRouting originalManualRouting;

        public bool HasManualRoute => hasManualRoute && manualRoute != null && manualRoute.Length >= 2;
        public bool OriginalUsesTerminalCable => HasManualRoute
            ? originalManualSourceWasTerminalCable : useTerminalCable;
        public TerminalCableRouting OriginalCableRouting => HasManualRoute
            ? originalManualRouting : terminalRouting;


        /// <summary>Returns defensive world-space *control* points (not mesh vertices).</summary>
        public Vector3[] GetEffectiveRoutePoints()
        {
            Vector3[] source = HasManualRoute ? manualRoute :
                (path != null ? path.GetRenderPoints() : null);
            if (source == null) return Array.Empty<Vector3>();
            Vector3[] points = (Vector3[])source.Clone();
            if (!useTerminalCable || !followAnchors || anchor1 == null ||
                anchor2 == null || points.Length < 2) return points;

            if (!HasManualRoute && !useSimulatedTerminalShape && terminalRouting != TerminalCableRouting.FreehandDrag)
                return new[] { anchor1.position, anchor2.position };

            // Preserve bends while endpoints travel. Distribute displacement
            // by arc length so long control segments are not distorted sharply.
            Vector3 from = anchor1.position - points[0];
            Vector3 to = anchor2.position - points[points.Length - 1];
            float length = 0f;
            for (int i = 1; i < points.Length; i++)
                length += Vector3.Distance(points[i - 1], points[i]);
            float traveled = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0) traveled += Vector3.Distance(source[i - 1], source[i]);
                float t = length > 0.00001f ? Mathf.Clamp01(traveled / length)
                    : (float)i / (points.Length - 1);
                points[i] = source[i] + Vector3.Lerp(from, to, t);
            }
            points[0] = anchor1.position;
            points[points.Length - 1] = anchor2.position;
            return points;
        }

        // The legacy renderer raises the cable mesh 0.08 units. Present
        // its true visible positions to the editor so the first bend edit
        // does not make an old breadboard wire jump vertically.
        public Vector3[] GetEditableWorldPoints()
        {
            Vector3[] points = GetEffectiveRoutePoints();
            if (!useTerminalCable)
                for (int i = 0; i < points.Length; i++)
                    points[i] += Vector3.up * 0.08f;
            return points;
        }

        public int EffectiveWaypointCount => HasManualRoute
            ? manualRoute.Length : (path != null ? path.PointCount : 0);

        /// <summary>Copies the current smoothed visual centerline for scene picking.</summary>
        public Vector3[] GetRenderedCenterline()
        {
            Vector3[] points = curve.ToArray();
            if (!useTerminalCable)
                for (int i = 0; i < points.Length; i++)
                    points[i] += Vector3.up * 0.08f;
            return points;
        }

        /// <summary>Replace only the rendered route; electrical connection is untouched.</summary>
        public bool SetManualRoute(Vector3[] worldPoints)
        {
            if (worldPoints == null || worldPoints.Length < 2 || worldPoints.Length > 512)
                return false;
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Vector3 v = worldPoints[i];
                if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                    float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z))
                    return false;
            }
            if (!HasManualRoute)
            {
                originalManualSourceWasTerminalCable = useTerminalCable;
                originalManualRouting = terminalRouting;
            }
            manualRoute = (Vector3[])worldPoints.Clone();
            if (followAnchors && anchor1 != null && anchor2 != null)
            {
                manualRoute[0] = anchor1.position;
                manualRoute[manualRoute.Length - 1] = anchor2.position;
            }
            hasManualRoute = true;
            // All edited routes, including formerly auto-arched/legacy cables,
            // must follow the entire control-point sequence.
            useTerminalCable = true;
            terminalRouting = TerminalCableRouting.FreehandDrag;
            Rebuild();
            return true;
        }

        /// <summary>Restore the original drawn visual geometry and routing policy.</summary>
        public void ClearManualRoute()
        {
            if (!HasManualRoute) return;
            manualRoute = Array.Empty<Vector3>();
            hasManualRoute = false;
            useTerminalCable = originalManualSourceWasTerminalCable;
            terminalRouting = originalManualRouting;
            Rebuild();
        }

        public bool UsesTerminalCable => useTerminalCable;
        /// <summary>The route type used when this is a terminal-to-terminal cable.</summary>
        public TerminalCableRouting CableRouting => terminalRouting;
        /// <summary>Whether both endpoints follow their live component transforms.</summary>
        public bool FollowsTerminalAnchors => followAnchors;


        public void ConfigureTerminalCable(TerminalCableRouting routing)
        {
            useTerminalCable = true;
            useSimulatedTerminalShape = false;
            terminalRouting = routing;
            anchor1 = null;
            anchor2 = null;
            followAnchors = false;
            Rebuild();
        }

        public void BindTerminalAnchors(Transform first, Transform second, bool followMovement)
        {
            anchor1 = followMovement ? first : null;
            anchor2 = followMovement ? second : null;
            followAnchors = followMovement && first != null && second != null;
            if (followAnchors)
            {
                lastAnchor1 = first.position;
                lastAnchor2 = second.position;
            }
            Rebuild();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || !useTerminalCable || !followAnchors ||
                anchor1 == null || anchor2 == null) return;
            Vector3 a = anchor1.position;
            Vector3 b = anchor2.position;
            if ((a - lastAnchor1).sqrMagnitude < AnchorMoveEpsilonSquared &&
                (b - lastAnchor2).sqrMagnitude < AnchorMoveEpsilonSquared) return;
            lastAnchor1 = a;
            lastAnchor2 = b;
            Rebuild();
        }

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
            freehandSimplifyTolerance = Mathf.Max(0f, freehandSimplifyTolerance);
            freehandCornerRoundness = Mathf.Clamp(freehandCornerRoundness, 0f, 0.45f);
            // Do not create meshes while Unity is importing a prefab asset.
            if (isActiveAndEnabled && gameObject.scene.IsValid()) Rebuild();
        }

        public void Rebuild()
        {
            if (!gameObject.scene.IsValid()) return;
            if (path == null) path = GetComponent<SignalPath>();
            if (path == null) { ClearMesh(); return; }
            if (useSimulatedTerminalShape && path.HasPreview && !HasManualRoute)
            {
                path.CopyRenderPointsTo(curve);
                CalculateCurveDistances();
            }
            else BuildCurve(GetEffectiveRoutePoints());
            if (curve.Count < 2) { ClearMesh(); return; }
            EnsureMesh();
            if (mesh == null) return;
            BuildTube();
        }

        internal void UseSimulatedTerminalShape()
        {
            if (!useSimulatedTerminalShape && mesh) mesh.MarkDynamic();
            useSimulatedTerminalShape = true;
        }

       
private void BuildCurve(Vector3[] points)
{
    curve.Clear();
    distances.Clear();

    if (points == null || points.Length < 2)
        return;

    List<Vector3> controls = new List<Vector3>();

    foreach (Vector3 point in points)
    {
        if (controls.Count == 0 ||
            (controls[controls.Count - 1] - point).sqrMagnitude > Epsilon)
        {
            controls.Add(point);
        }
    }

    if (controls.Count < 2)
        return;

    if (useTerminalCable)
    {
        if (useSimulatedTerminalShape && !HasManualRoute)
            curve.AddRange(controls);
        else if (terminalRouting == TerminalCableRouting.FreehandDrag && controls.Count > 2)
            BuildFreehandCurve(controls);
        else if (terminalRouting == TerminalCableRouting.FreehandDrag)
        {
            curve.Add(controls[0]);
            curve.Add(controls[controls.Count - 1]);
        }
        else
            BuildTerminalCurve(controls[0], controls[controls.Count - 1]);
        CalculateCurveDistances();
        return;
    }

    // Smooth, elevated cable between two terminals.
    if (smoothCurve && controls.Count == 2)
    {
        Vector3 start = controls[0];
        Vector3 end = controls[1];

        float length = Vector3.Distance(start, end);

        float height = Mathf.Min(
            cableArchHeight,
            length * 0.25f
        );

        Vector3 p1 = Vector3.Lerp(start, end, 0.33f)
                     + Vector3.up * height;

        Vector3 p2 = Vector3.Lerp(start, end, 0.67f)
                     + Vector3.up * height;

        int steps = Mathf.Max(12, subdivisionsPerSegment * 3);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float inv = 1f - t;

            Vector3 position =
                inv * inv * inv * start +
                3f * inv * inv * t * p1 +
                3f * inv * t * t * p2 +
                t * t * t * end;

            curve.Add(position);
        }
    }
    else
    {
        // Preserve deliberate routing waypoints.
        for (int segment = 0; segment < controls.Count - 1; segment++)
        {
            Vector3 start = controls[segment];
            Vector3 end = controls[segment + 1];

            int steps = smoothCurve
                ? Mathf.Max(2, subdivisionsPerSegment)
                : 1;

            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;

                Vector3 position = smoothCurve
                    ? CatmullRom(
                        controls[Mathf.Max(0, segment - 1)],
                        start,
                        end,
                        controls[Mathf.Min(controls.Count - 1, segment + 2)],
                        t)
                    : Vector3.Lerp(start, end, t);

                curve.Add(position);
            }
        }

        curve.Add(controls[controls.Count - 1]);
    }

    float total = 0f;
    distances.Add(0f);

    for (int i = 1; i < curve.Count; i++)
    {
        total += Vector3.Distance(curve[i - 1], curve[i]);
        distances.Add(total);
    }
}

        /// <summary>
        /// Reduce hand jitter with a distance-to-segment polyline simplifier,
        /// then round corners locally while preserving exact terminal ends.
        /// Does not touch the original SignalPath (other consumers still see
        /// the player's complete sampled route).
        /// </summary>
        private void BuildFreehandCurve(List<Vector3> controls)
        {
            int count = controls.Count;
            if (count < 2) return;

            bool[] keep = new bool[count];
            keep[0] = keep[count - 1] = true;
            // Manual bends are intentional, so never delete them as jitter.
            if (freehandSimplifyTolerance > 0f && !HasManualRoute)
                MarkImportantRoutePoints(controls, 0, count - 1,
                    freehandSimplifyTolerance * freehandSimplifyTolerance, keep);
            else
                for (int i = 0; i < count; i++) keep[i] = true;

            List<Vector3> route = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
                if (keep[i]) route.Add(controls[i]);

            if (route.Count < 2) return;
            curve.Add(route[0]);
            for (int i = 1; i < route.Count - 1; i++)
            {
                Vector3 previous = route[i - 1];
                Vector3 pivot = route[i];
                Vector3 next = route[i + 1];
                Vector3 before = previous - pivot;
                Vector3 after = next - pivot;
                float beforeLength = before.magnitude;
                float afterLength = after.magnitude;
                if (beforeLength < 0.0001f || afterLength < 0.0001f ||
                    freehandCornerRoundness <= 0f ||
                    Vector3.Dot(-before / beforeLength, after / afterLength) > 0.995f)
                {
                    curve.Add(pivot);
                    continue;
                }

                float blendLength = Mathf.Min(beforeLength, afterLength) *
                    freehandCornerRoundness;
                Vector3 entrance = pivot + before / beforeLength * blendLength;
                Vector3 exit = pivot + after / afterLength * blendLength;
                curve.Add(entrance);
                // Quadratic Bezier passes near the recorded corner.
                for (int j = 1; j <= 4; j++)
                {
                    float t = j * 0.25f;
                    float u = 1f - t;
                    curve.Add(u * u * entrance + 2f * u * t * pivot +
                        t * t * exit);
                }
            }
            curve.Add(route[route.Count - 1]);
        }

        private static void MarkImportantRoutePoints(List<Vector3> source,
            int from, int to, float toleranceSquared, bool[] keep)
        {
            if (to <= from + 1) return;
            Vector3 start = source[from];
            Vector3 segment = source[to] - start;
            float lengthSquared = segment.sqrMagnitude;
            int worst = -1;
            float largestDeviation = toleranceSquared;
            for (int i = from + 1; i < to; i++)
            {
                float t = lengthSquared > 0.00000001f
                    ? Mathf.Clamp01(Vector3.Dot(source[i] - start, segment) / lengthSquared)
                    : 0f;
                Vector3 nearest = start + segment * t;
                float deviation = (source[i] - nearest).sqrMagnitude;
                if (deviation > largestDeviation)
                {
                    largestDeviation = deviation;
                    worst = i;
                }
            }
            if (worst < 0) return;
            keep[worst] = true;
            MarkImportantRoutePoints(source, from, worst, toleranceSquared, keep);
            MarkImportantRoutePoints(source, worst, to, toleranceSquared, keep);
        }

        private void BuildTerminalCurve(Vector3 start, Vector3 end)
        {
            float separation = Vector3.Distance(start, end);
            if (separation < 0.0001f) return;

            if (terminalRouting == TerminalCableRouting.Straight)
            {
                curve.Add(start);
                curve.Add(end);
                return;
            }

            float height = Mathf.Min(cableArchHeight, separation * 0.35f);
            if (terminalRouting == TerminalCableRouting.Orthogonal)
            {
                // Four deliberate, right-angled routing anchors.
                curve.Add(start);
                if (height > 0.0001f) curve.Add(start + Vector3.up * height);
                if (height > 0.0001f) curve.Add(end + Vector3.up * height);
                curve.Add(end);
                return;
            }

            // Smooth raised cubic Bezier - retains exact endpoint contact.
            Vector3 p1 = Vector3.Lerp(start, end, 1f / 3f) + Vector3.up * height;
            Vector3 p2 = Vector3.Lerp(start, end, 2f / 3f) + Vector3.up * height;
            int steps = Mathf.Max(16, subdivisionsPerSegment * 4);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float inv = 1f - t;
                curve.Add(inv * inv * inv * start +
                          3f * inv * inv * t * p1 +
                          3f * inv * t * t * p2 +
                          t * t * t * end);
            }
        }

        private void CalculateCurveDistances()
        {
            distances.Clear();
            if (curve.Count == 0) return;
            distances.Add(0f);
            float cumulative = 0f;
            for (int i = 1; i < curve.Count; i++)
            {
                cumulative += Vector3.Distance(curve[i - 1], curve[i]);
                distances.Add(cumulative);
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
            int vertexCount = bodyVertices + extraVertices;
            int triangleCount = (rings - 1) * sides * 6 + (capEnds ? sides * 6 : 0);
            if (cableVertices == null || cableVertices.Length != vertexCount)
            {
                cableVertices = new Vector3[vertexCount];
                cableNormals = new Vector3[vertexCount];
                cableUV = new Vector2[vertexCount];
            }
            if (cableTriangles == null || cableTriangles.Length != triangleCount)
                cableTriangles = new int[triangleCount];
            var vertices = cableVertices;
            var normals = cableNormals;
            var uv = cableUV;
            var triangles = cableTriangles;

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
        (useTerminalCable ? Vector3.zero : Vector3.up * 0.08f) +
        radial * (useSimulatedTerminalShape && (i == 0 || i == rings - 1) ? radius * 1.3f : radius));
                    normals[index] = transform.InverseTransformDirection(radial).normalized;
                    float totalLength =
    Mathf.Max(
        distances[distances.Count - 1],
        0.0001f
    );

uv[index] = new Vector2(
    useTerminalCable ? distances[i] / uvMetersPerTile : distances[i] / totalLength,
    v
);
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
            if (!useSimulatedTerminalShape || !path.HasPreview)
            {
                cableVertices = cableNormals = null;
                cableUV = null;
                cableTriangles = null;
            }
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
