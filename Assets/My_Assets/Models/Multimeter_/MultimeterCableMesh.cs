using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Production-grade procedural cable mesh generator.
    ///
    /// Designed for Project Spark:
    /// - Multimeter probe cables
    /// - Test leads
    /// - USB-style cables
    /// - Power cables
    /// - Sensor cables
    /// - Connector cables
    ///
    /// The component is intentionally visual-only.
    /// Electrical connectivity should remain handled by the
    /// Project Spark terminal / plug / socket / circuit systems.
    ///
    /// Features:
    /// - Stable procedural tube generation
    /// - Endpoint transforms
    /// - Endpoint direction control
    /// - Smooth cubic cable path
    /// - Configurable sag
    /// - Custom sag direction
    /// - Cable taper
    /// - Radial quality
    /// - Curve quality
    /// - Cable twist
    /// - UV generation
    /// - Optional end caps
    /// - Automatic transform-change detection
    /// - Manual rebuild support
    /// - Runtime rebuild control
    /// - Editor validation
    /// - Dynamic mesh optimization
    /// - Correct local-space normals
    /// - Stable parallel-frame generation
    /// - Bounds recalculation
    /// - Optional shadow/cast configuration
    /// - Safe null handling
    /// - No hierarchy searches
    /// - No LINQ
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class MultimeterCableMesh : MonoBehaviour
    {
        // ============================================================
        // ENUMS
        // ============================================================

        public enum CableQuality
        {
            Custom,
            Low,
            Medium,
            High,
            Ultra
        }

        public enum UpdateMode
        {
            Manual,
            OnTransformChange,
            EveryFrame
        }

        public enum EndpointDirectionMode
        {
            None,
            TransformForward,
            TransformUp,
            Custom
        }

        // ============================================================
        // CONNECTION
        // ============================================================

        [Header("Connection")]

        [Tooltip("Beginning of the cable.")]
        [SerializeField]
        private Transform startPoint;

        [Tooltip("End of the cable.")]
        [SerializeField]
        private Transform endPoint;

        // ============================================================
        // QUALITY
        // ============================================================

        [Header("Quality")]

        [SerializeField]
        private CableQuality quality = CableQuality.High;

        [Tooltip("Number of segments along the cable.")]
        [Min(2)]
        [SerializeField]
        private int curveSegments = 32;

        [Tooltip("Number of vertices around the cable.")]
        [Min(3)]
        [SerializeField]
        private int radialSegments = 12;

        // ============================================================
        // SIZE
        // ============================================================

        [Header("Cable Size")]

        [Tooltip("Base cable radius in world units.")]
        [Min(0.0001f)]
        [SerializeField]
        private float cableRadius = 0.012f;

        [Tooltip("Radius multiplier at the start.")]
        [Min(0.01f)]
        [SerializeField]
        private float startRadiusMultiplier = 1f;

        [Tooltip("Radius multiplier at the end.")]
        [Min(0.01f)]
        [SerializeField]
        private float endRadiusMultiplier = 1f;

        // ============================================================
        // PATH
        // ============================================================

        [Header("Cable Path")]

        [Tooltip("Controls how strongly the cable bends between endpoints.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float curveSmoothness = 0.5f;

        [Tooltip("Controls the amount of downward sag.")]
        [Min(0f)]
        [SerializeField]
        private float sagAmount = 0.15f;

        [Tooltip("Direction used for cable sag.")]
        [SerializeField]
        private Vector3 sagDirection = Vector3.down;

        [Tooltip("How strongly the sag follows the cable's middle.")]
        [Range(0.1f, 4f)]
        [SerializeField]
        private float sagPower = 1.5f;

        // ============================================================
        // ENDPOINT DIRECTIONS
        // ============================================================

        [Header("Endpoint Direction")]

        [SerializeField]
        private EndpointDirectionMode startDirectionMode =
            EndpointDirectionMode.TransformForward;

        [SerializeField]
        private EndpointDirectionMode endDirectionMode =
            EndpointDirectionMode.TransformForward;

        [Tooltip("Custom start tangent direction.")]
        [SerializeField]
        private Vector3 customStartDirection = Vector3.forward;

        [Tooltip("Custom end tangent direction.")]
        [SerializeField]
        private Vector3 customEndDirection = Vector3.back;

        [Tooltip("Influence of the endpoint directions.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float endpointDirectionInfluence = 0.35f;

        // ============================================================
        // TWIST
        // ============================================================

        [Header("Cable Twist")]

        [Tooltip("Total twist around the cable from start to end.")]
        [Range(-720f, 720f)]
        [SerializeField]
        private float twistDegrees = 0f;

        // ============================================================
        // CAPS
        // ============================================================

        [Header("Cable Ends")]

        [SerializeField]
        private bool generateStartCap = true;

        [SerializeField]
        private bool generateEndCap = true;

        // ============================================================
        // UV
        // ============================================================

        [Header("UV")]

        [Tooltip("Number of cable lengths represented by one V coordinate.")]
        [Min(0.001f)]
        [SerializeField]
        private float uvTiling = 1f;

        [Tooltip("Rotates the UV around the cable.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float uvRotation = 0f;

        // ============================================================
        // UPDATE
        // ============================================================

        [Header("Update")]

        [SerializeField]
        private UpdateMode updateMode = UpdateMode.OnTransformChange;

        [Tooltip("Force the cable to rebuild after initialization.")]
        [SerializeField]
        private bool generateOnEnable = true;

        // ============================================================
        // RENDERING
        // ============================================================

        [Header("Rendering")]

        [SerializeField]
        private bool castShadows = true;

        [SerializeField]
        private bool receiveShadows = true;

        [Tooltip("Optional material override.")]
        [SerializeField]
        private Material cableMaterial;

        // ============================================================
        // DEBUG
        // ============================================================

        [Header("Diagnostics")]

        [SerializeField]
        private bool drawDebugPath = false;

        [SerializeField]
        private bool logGenerationWarnings = false;

        // ============================================================
        // PRIVATE
        // ============================================================

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh cableMesh;

        private Vector3 lastStartPosition;      

        private Vector3 lastEndPosition;        

        private Vector3 lastLossyScale;

        private bool initialized;
        private bool rebuildRequested;

        private int activeCurveSegments;
        private int activeRadialSegments;

        // ============================================================
        // CONSTANTS
        // ============================================================

        private const string MeshName =
            "Project Spark - Dynamic Multimeter Cable";

        private const float MinimumDirectionSqrMagnitude =
            0.000001f;

        private const float ParallelThreshold =
            0.985f;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();

            if (generateOnEnable)
            {
                RequestRebuild();
            }
        }

        private void Start()
        {
            if (!initialized)
            {
                Initialize();
            }

            RequestRebuild();

            if (rebuildRequested)
            {
                GenerateCable();
            }
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                return;
            }

            switch (updateMode)
            {
                case UpdateMode.Manual:
                    break;

                case UpdateMode.OnTransformChange:

                    if (HasConnectionChanged())
                    {
                        GenerateCable();
                    }

                    break;

                case UpdateMode.EveryFrame:

                    GenerateCable();

                    break;
            }
        }

        private void OnDisable()
        {
            rebuildRequested = false;
        }

        private void OnDestroy()
        {
            ReleaseMesh();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            ClampValues();

            if (!Application.isPlaying)
            {
                return;
            }

            RequestRebuild();
        }

#endif

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            CreateMesh();

            ApplyRendererSettings();

            initialized = true;
        }

        private void CreateMesh()
        {
            if (meshFilter == null)
            {
                meshFilter =
                    GetComponent<MeshFilter>();
            }

            if (cableMesh != null)
            {
                return;
            }

            cableMesh =
                new Mesh
                {
                    name = MeshName
                };

            cableMesh.MarkDynamic();

            meshFilter.sharedMesh = cableMesh;
        }

        private void ReleaseMesh()
        {
            if (cableMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(cableMesh);
            }
            else
            {
                DestroyImmediate(cableMesh);
            }

            cableMesh = null;
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Forces the cable to rebuild immediately.
        /// </summary>
        public void GenerateCable()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (!ValidateRuntimeState())
            {
                return;
            }

            ResolveQuality();

            int segmentCount =
                activeCurveSegments;

            int radialCount =
                activeRadialSegments;

            int ringVertexCount =
                (segmentCount + 1) *
                radialCount;

            int capVertexCount =
                0;

            if (generateStartCap)
            {
                capVertexCount += radialCount + 1;
            }

            if (generateEndCap)
            {
                capVertexCount += radialCount + 1;
            }

            int vertexCount =
                ringVertexCount +
                capVertexCount;

            int sideTriangleCount =
                segmentCount *
                radialCount *
                6;

            int capTriangleCount =
                0;

            if (generateStartCap)
            {
                capTriangleCount +=
                    radialCount * 3;
            }

            if (generateEndCap)
            {
                capTriangleCount +=
                    radialCount * 3;
            }

            Vector3[] vertices =
                new Vector3[vertexCount];

            Vector3[] normals =
                new Vector3[vertexCount];

            Vector2[] uv =
                new Vector2[vertexCount];

            int[] triangles =
                new int[
                    sideTriangleCount +
                    capTriangleCount
                ];

            Vector3 start =
                startPoint.position;

            Vector3 end =
                endPoint.position;

            Vector3 startTangent =
                GetEndpointTangent(
                    true,
                    start,
                    end);

            Vector3 endTangent =
                GetEndpointTangent(
                    false,
                    start,
                    end);

            Vector3 previousNormal =
                GetInitialFrameNormal(
                    startTangent);

            int vertexIndex = 0;

            float totalDistance =
                Vector3.Distance(
                    start,
                    end);

            if (totalDistance < 0.0001f)
            {
                totalDistance = 1f;
            }

            // --------------------------------------------------------
            // SIDE RINGS
            // --------------------------------------------------------

            for (int i = 0;
                 i <= segmentCount;
                 i++)
            {
                float t =
                    (float)i /
                    segmentCount;

                Vector3 center =
                    EvaluateCablePoint(
                        start,
                        end,
                        startTangent,
                        endTangent,
                        t);

                Vector3 tangent =
                    EvaluateCableTangent(
                        start,
                        end,
                        startTangent,
                        endTangent,
                        t);

                tangent =
                    NormalizeSafe(
                        tangent,
                        i == 0
                            ? startTangent
                            : Vector3.forward);

                Vector3 normal =
                    BuildStableNormal(
                        tangent,
                        previousNormal);

                previousNormal = normal;

                Vector3 binormal =
                    Vector3.Cross(
                        tangent,
                        normal);

                binormal =
                    NormalizeSafe(
                        binormal,
                        Vector3.right);

                normal =
                    Vector3.Cross(
                        binormal,
                        tangent);

                normal.Normalize();

                float twist =
                    twistDegrees *
                    t;

                RotateFrame(
                    ref normal,
                    ref binormal,
                    tangent,
                    twist);

                float radius =
                    EvaluateRadius(t);

                for (int j = 0;
                     j < radialCount;
                     j++)
                {
                    float radialT =
                        (float)j /
                        radialCount;

                    float angle =
                        radialT *
                        Mathf.PI *
                        2f;

                    angle +=
                        uvRotation *
                        Mathf.PI *
                        2f;

                    Vector3 radial =
                        Mathf.Cos(angle) *
                        normal
                        +
                        Mathf.Sin(angle) *
                        binormal;

                    Vector3 worldPosition =
                        center +
                        radial *
                        radius;

                    Vector3 localPosition =
                        transform.InverseTransformPoint(
                            worldPosition);

                    Vector3 localNormal =
                        transform.InverseTransformDirection(
                            radial);

                    localNormal =
                        NormalizeSafe(
                            localNormal,
                            Vector3.up);

                    vertices[vertexIndex] =
                        localPosition;

                    normals[vertexIndex] =
                        localNormal;

                    float distanceT =
                        t *
                        uvTiling;

                    uv[vertexIndex] =
                        new Vector2(
                            radialT,
                            distanceT);

                    vertexIndex++;
                }
            }

            // --------------------------------------------------------
            // SIDE TRIANGLES
            // --------------------------------------------------------

            int triangleIndex = 0;

            for (int i = 0;
                 i < segmentCount;
                 i++)
            {
                int currentRing =
                    i *
                    radialCount;

                int nextRing =
                    (i + 1) *
                    radialCount;

                for (int j = 0;
                     j < radialCount;
                     j++)
                {
                    int nextRadial =
                        (j + 1) %
                        radialCount;

                    int current =
                        currentRing +
                        j;

                    int next =
                        currentRing +
                        nextRadial;

                    int currentNext =
                        nextRing +
                        j;

                    int nextNext =
                        nextRing +
                        nextRadial;

                    triangles[triangleIndex++] =
                        current;

                    triangles[triangleIndex++] =
                        currentNext;

                    triangles[triangleIndex++] =
                        nextNext;

                    triangles[triangleIndex++] =
                        current;

                    triangles[triangleIndex++] =
                        nextNext;

                    triangles[triangleIndex++] =
                        next;
                }
            }

            // --------------------------------------------------------
            // CAPS
            // --------------------------------------------------------

            if (generateStartCap)
            {
                AddStartCap(
                    vertices,
                    normals,
                    uv,
                    triangles,
                    ref vertexIndex,
                    ref triangleIndex,
                    radialCount,
                    start,
                    startTangent,
                    EvaluateRadius(0f));
            }

            if (generateEndCap)
            {
                AddEndCap(
                    vertices,
                    normals,
                    uv,
                    triangles,
                    ref vertexIndex,
                    ref triangleIndex,
                    radialCount,
                    end,
                    endTangent,
                    EvaluateRadius(1f));
            }

            // --------------------------------------------------------
            // APPLY
            // --------------------------------------------------------

            cableMesh.Clear();

            cableMesh.vertices =
                vertices;

            cableMesh.normals =
                normals;

            cableMesh.uv =
                uv;

            cableMesh.triangles =
                triangles;

            cableMesh.RecalculateBounds();

            SaveTransformState();

            rebuildRequested = false;
        }

        /// <summary>
        /// Requests a rebuild on the next generation pass.
        /// </summary>
        public void RequestRebuild()
        {
            rebuildRequested = true;
        }

        /// <summary>
        /// Clears the generated cable mesh.
        /// </summary>
        public void ClearCable()
        {
            if (cableMesh == null)
            {
                return;
            }

            cableMesh.Clear();

            rebuildRequested = false;
        }

        /// <summary>
        /// Returns the current generated mesh.
        /// </summary>
        public Mesh GetCableMesh()
        {
            return cableMesh;
        }

        /// <summary>
        /// Returns the start connection transform.
        /// </summary>
        public Transform GetStartPoint()
        {
            return startPoint;
        }

        /// <summary>
        /// Returns the end connection transform.
        /// </summary>
        public Transform GetEndPoint()
        {
            return endPoint;
        }

        // ============================================================
        // PATH EVALUATION
        // ============================================================

        private Vector3 EvaluateCablePoint(
            Vector3 start,
            Vector3 end,
            Vector3 startTangent,
            Vector3 endTangent,
            float t)
        {
            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(t));

            float distance =
                Vector3.Distance(
                    start,
                    end);

            float handleLength =
                Mathf.Max(
                    distance *
                    Mathf.Lerp(
                        0.15f,
                        0.5f,
                        curveSmoothness),
                    0.001f);

            Vector3 controlA =
                start +
                startTangent *
                handleLength *
                endpointDirectionInfluence;

            Vector3 controlB =
                end +
                endTangent *
                handleLength *
                endpointDirectionInfluence;

            Vector3 point =
                CubicBezier(
                    start,
                    controlA,
                    controlB,
                    end,
                    smooth);

            float sagFactor =
                Mathf.Sin(
                    t *
                    Mathf.PI);

            sagFactor =
                Mathf.Pow(
                    Mathf.Clamp01(sagFactor),
                    sagPower);

            Vector3 sag =
                NormalizeSafe(
                    sagDirection,
                    Vector3.down)
                *
                sagAmount *
                sagFactor;

            return point + sag;
        }

        private Vector3 EvaluateCableTangent(
            Vector3 start,
            Vector3 end,
            Vector3 startTangent,
            Vector3 endTangent,
            float t)
        {
            float sample =
                Mathf.Clamp01(t);

            float delta =
                0.001f;

            float a =
                Mathf.Max(
                    0f,
                    sample - delta);

            float b =
                Mathf.Min(
                    1f,
                    sample + delta);

            if (Mathf.Approximately(a, b))
            {
                return end - start;
            }

            Vector3 previous =
                EvaluateCablePoint(
                    start,
                    end,
                    startTangent,
                    endTangent,
                    a);

            Vector3 next =
                EvaluateCablePoint(
                    start,
                    end,
                    startTangent,
                    endTangent,
                    b);

            return next - previous;
        }

        private Vector3 CubicBezier(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float t)
        {
            float oneMinusT =
                1f - t;

            float oneMinusT2 =
                oneMinusT *
                oneMinusT;

            float t2 =
                t *
                t;

            return
                oneMinusT2 *
                oneMinusT *
                p0
                +
                3f *
                oneMinusT2 *
                t *
                p1
                +
                3f *
                oneMinusT *
                t2 *
                p2
                +
                t2 *
                t *
                p3;
        }

        // ============================================================
        // TANGENTS
        // ============================================================

        private Vector3 GetEndpointTangent(
            bool start,
            Vector3 startPosition,
            Vector3 endPosition)
        {
            EndpointDirectionMode mode =
                start
                    ? startDirectionMode
                    : endDirectionMode;

            Transform point =
                start
                    ? startPoint
                    : endPoint;

            Vector3 direction;

            switch (mode)
            {
                case EndpointDirectionMode.TransformForward:

                    direction =
                        point != null
                            ? point.forward
                            : Vector3.forward;

                    break;

                case EndpointDirectionMode.TransformUp:

                    direction =
                        point != null
                            ? point.up
                            : Vector3.up;

                    break;

                case EndpointDirectionMode.Custom:

                    direction =
                        start
                            ? customStartDirection
                            : customEndDirection;

                    if (!start)
                    {
                        direction = -direction;
                    }

                    break;

                case EndpointDirectionMode.None:
                default:

                    direction =
                        start
                            ? endPosition -
                              startPosition
                            : startPosition -
                              endPosition;

                    break;
            }

            direction =
                NormalizeSafe(
                    direction,
                    start
                        ? endPosition -
                          startPosition
                        : startPosition -
                          endPosition);

            return direction;
        }

        // ============================================================
        // FRAME
        // ============================================================

        private Vector3 GetInitialFrameNormal(
            Vector3 tangent)
        {
            Vector3 reference =
                Vector3.up;

            if (Mathf.Abs(
                Vector3.Dot(
                    tangent,
                    reference))
                > ParallelThreshold)
            {
                reference =
                    Vector3.forward;
            }

            Vector3 normal =
                Vector3.Cross(
                    reference,
                    tangent);

            if (normal.sqrMagnitude <
                MinimumDirectionSqrMagnitude)
            {
                normal =
                    Vector3.Cross(
                        Vector3.right,
                        tangent);
            }

            return NormalizeSafe(
                normal,
                Vector3.right);
        }

        private Vector3 BuildStableNormal(
            Vector3 tangent,
            Vector3 previousNormal)
        {
            Vector3 projected =
                Vector3.ProjectOnPlane(
                    previousNormal,
                    tangent);

            if (projected.sqrMagnitude <
                MinimumDirectionSqrMagnitude)
            {
                projected =
                    GetInitialFrameNormal(
                        tangent);
            }

            return NormalizeSafe(
                projected,
                Vector3.up);
        }

        private void RotateFrame(
            ref Vector3 normal,
            ref Vector3 binormal,
            Vector3 tangent,
            float degrees)
        {
            if (Mathf.Abs(degrees) <
                0.0001f)
            {
                return;
            }

            Quaternion rotation =
                Quaternion.AngleAxis(
                    degrees,
                    tangent);

            normal =
                rotation *
                normal;

            binormal =
                rotation *
                binormal;
        }

        // ============================================================
        // CAPS
        // ============================================================

        private void AddStartCap(
            Vector3[] vertices,
            Vector3[] normals,
            Vector2[] uv,
            int[] triangles,
            ref int vertexIndex,
            ref int triangleIndex,
            int radialCount,
            Vector3 center,
            Vector3 tangent,
            float radius)
        {
            Vector3 capNormal =
                -NormalizeSafe(
                    tangent,
                    Vector3.forward);

            int centerIndex =
                vertexIndex++;

            vertices[centerIndex] =
                transform.InverseTransformPoint(
                    center);

            normals[centerIndex] =
                transform.InverseTransformDirection(
                    capNormal);

            uv[centerIndex] =
                new Vector2(
                    0.5f,
                    0f);

            Vector3 frameNormal =
                GetInitialFrameNormal(
                    tangent);

            Vector3 frameBinormal =
                Vector3.Cross(
                    tangent,
                    frameNormal);

            frameBinormal.Normalize();

            int ringStart =
                vertexIndex;

            for (int j = 0;
                 j < radialCount;
                 j++)
            {
                float t =
                    (float)j /
                    radialCount;

                float angle =
                    t *
                    Mathf.PI *
                    2f;

                Vector3 radial =
                    Mathf.Cos(angle) *
                    frameNormal
                    +
                    Mathf.Sin(angle) *
                    frameBinormal;

                vertices[vertexIndex] =
                    transform.InverseTransformPoint(
                        center +
                        radial *
                        radius);

                normals[vertexIndex] =
                    transform.InverseTransformDirection(
                        capNormal);

                uv[vertexIndex] =
                    new Vector2(
                        0.5f +
                        Mathf.Cos(angle) *
                        0.5f,
                        0.5f +
                        Mathf.Sin(angle) *
                        0.5f);

                vertexIndex++;
            }

            for (int j = 0;
                 j < radialCount;
                 j++)
            {
                int next =
                    (j + 1) %
                    radialCount;

                triangles[triangleIndex++] =
                    centerIndex;

                triangles[triangleIndex++] =
                    ringStart +
                    next;

                triangles[triangleIndex++] =
                    ringStart +
                    j;
            }
        }

        private void AddEndCap(
            Vector3[] vertices,
            Vector3[] normals,
            Vector2[] uv,
            int[] triangles,
            ref int vertexIndex,
            ref int triangleIndex,
            int radialCount,
            Vector3 center,
            Vector3 tangent,
            float radius)
        {
            Vector3 capNormal =
                NormalizeSafe(
                    tangent,
                    Vector3.forward);

            int centerIndex =
                vertexIndex++;

            vertices[centerIndex] =
                transform.InverseTransformPoint(
                    center);

            normals[centerIndex] =
                transform.InverseTransformDirection(
                    capNormal);

            uv[centerIndex] =
                new Vector2(
                    0.5f,
                    1f);

            Vector3 frameNormal =
                GetInitialFrameNormal(
                    tangent);

            Vector3 frameBinormal =
                Vector3.Cross(
                    tangent,
                    frameNormal);

            frameBinormal.Normalize();

            int ringStart =
                vertexIndex;

            for (int j = 0;
                 j < radialCount;
                 j++)
            {
                float t =
                    (float)j /
                    radialCount;

                float angle =
                    t *
                    Mathf.PI *
                    2f;

                Vector3 radial =
                    Mathf.Cos(angle) *
                    frameNormal
                    +
                    Mathf.Sin(angle) *
                    frameBinormal;

                vertices[vertexIndex] =
                    transform.InverseTransformPoint(
                        center +
                        radial *
                        radius);

                normals[vertexIndex] =
                    transform.InverseTransformDirection(
                        capNormal);

                uv[vertexIndex] =
                    new Vector2(
                        0.5f +
                        Mathf.Cos(angle) *
                        0.5f,
                        0.5f +
                        Mathf.Sin(angle) *
                        0.5f);

                vertexIndex++;
            }

            for (int j = 0;
                 j < radialCount;
                 j++)
            {
                int next =
                    (j + 1) %
                    radialCount;

                triangles[triangleIndex++] =
                    centerIndex;

                triangles[triangleIndex++] =
                    ringStart +
                    j;

                triangles[triangleIndex++] =
                    ringStart +
                    next;
            }
        }

        // ============================================================
        // RADIUS
        // ============================================================

        private float EvaluateRadius(
            float t)
        {
            float multiplier =
                Mathf.Lerp(
                    startRadiusMultiplier,
                    endRadiusMultiplier,
                    Mathf.Clamp01(t));

            return
                Mathf.Max(
                    0.0001f,
                    cableRadius *
                    multiplier);
        }

        // ============================================================
        // QUALITY
        // ============================================================

        private void ResolveQuality()
        {
            switch (quality)
            {
                case CableQuality.Low:

                    activeCurveSegments = 16;
                    activeRadialSegments = 6;

                    break;

                case CableQuality.Medium:

                    activeCurveSegments = 24;
                    activeRadialSegments = 8;

                    break;

                case CableQuality.High:

                    activeCurveSegments = 40;
                    activeRadialSegments = 12;

                    break;

                case CableQuality.Ultra:

                    activeCurveSegments = 64;
                    activeRadialSegments = 16;

                    break;

                case CableQuality.Custom:
                default:

                    activeCurveSegments =
                        Mathf.Max(
                            2,
                            curveSegments);

                    activeRadialSegments =
                        Mathf.Max(
                            3,
                            radialSegments);

                    break;
            }
        }

        // ============================================================
        // CHANGE DETECTION
        // ============================================================

       private bool HasConnectionChanged()
{
    if (startPoint == null ||
        endPoint == null)
    {
        return false;
    }

    if (rebuildRequested)
    {
        return true;
    }

    // ------------------------------------------------------------
    // POSITION CHANGES
    // ------------------------------------------------------------

    // Endpoint movement changes the actual cable path.
    if (startPoint.position != lastStartPosition)
    {
        return true;
    }

    if (endPoint.position != lastEndPosition)
    {
        return true;
    }

    // ------------------------------------------------------------
    // IMPORTANT:
    // DO NOT rebuild the cable when endpoint rotation changes.
    //
    // The endpoint may be a child/grandchild cable head.
    // Rotating that head should rotate only the head itself,
    // not regenerate the complete cable body.
    // ------------------------------------------------------------

    // Cable object's own scale still affects the generated mesh.
    if (transform.lossyScale != lastLossyScale)
    {
        return true;
    }

    return false;
}

      private void SaveTransformState()
{
    if (startPoint != null)
    {
        lastStartPosition =
            startPoint.position;
    }

    if (endPoint != null)
    {
        lastEndPosition =
            endPoint.position;
    }

    lastLossyScale =
        transform.lossyScale;
}

        // ============================================================
        // VALIDATION
        // ============================================================

        private bool ValidateRuntimeState()
        {
            if (startPoint == null)
            {
                Warn(
                    "Start Point is not assigned.");

                return false;
            }

            if (endPoint == null)
            {
                Warn(
                    "End Point is not assigned.");

                return false;
            }

            if (meshFilter == null)
            {
                meshFilter =
                    GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer =
                    GetComponent<MeshRenderer>();
            }

            if (meshFilter == null)
            {
                Warn(
                    "MeshFilter is missing.");

                return false;
            }

            return true;
        }

        private void ClampValues()
        {
            curveSegments =
                Mathf.Max(
                    2,
                    curveSegments);

            radialSegments =
                Mathf.Max(
                    3,
                    radialSegments);

            cableRadius =
                Mathf.Max(
                    0.0001f,
                    cableRadius);

            startRadiusMultiplier =
                Mathf.Max(
                    0.01f,
                    startRadiusMultiplier);

            endRadiusMultiplier =
                Mathf.Max(
                    0.01f,
                    endRadiusMultiplier);

            sagAmount =
                Mathf.Max(
                    0f,
                    sagAmount);

            sagPower =
                Mathf.Max(
                    0.1f,
                    sagPower);

            endpointDirectionInfluence =
                Mathf.Clamp(
                    endpointDirectionInfluence,
                    0f,
                    2f);

            uvTiling =
                Mathf.Max(
                    0.001f,
                    uvTiling);
        }

        // ============================================================
        // RENDERER
        // ============================================================

        private void ApplyRendererSettings()
        {
            if (meshRenderer == null)
            {
                return;
            }

            meshRenderer.shadowCastingMode =
                castShadows
                    ? UnityEngine.Rendering.ShadowCastingMode.On
                    : UnityEngine.Rendering.ShadowCastingMode.Off;

            meshRenderer.receiveShadows =
                receiveShadows;

            if (cableMaterial != null)
            {
                meshRenderer.sharedMaterial =
                    cableMaterial;
            }
        }

        // ============================================================
        // UTILITIES
        // ============================================================

        private Vector3 NormalizeSafe(
            Vector3 value,
            Vector3 fallback)
        {
            if (value.sqrMagnitude <
                MinimumDirectionSqrMagnitude)
            {
                value = fallback;
            }

            if (value.sqrMagnitude <
                MinimumDirectionSqrMagnitude)
            {
                value = Vector3.forward;
            }

            return value.normalized;
        }

        private void Warn(
            string message)
        {
            if (!logGenerationWarnings)
            {
                return;
            }

            Debug.LogWarning(
                "[MultimeterCableMesh] " +
                message,
                this);
        }

        // ============================================================
        // DEBUG
        // ============================================================

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugPath ||
                startPoint == null ||
                endPoint == null)
            {
                return;
            }

            Vector3 start =
                startPoint.position;

            Vector3 end =
                endPoint.position;

            Vector3 startTangent =
                GetEndpointTangent(
                    true,
                    start,
                    end);

            Vector3 endTangent =
                GetEndpointTangent(
                    false,
                    start,
                    end);

            Vector3 previous =
                start;

            int segments =
                Mathf.Max(
                    8,
                    curveSegments);

            for (int i = 1;
                 i <= segments;
                 i++)
            {
                float t =
                    (float)i /
                    segments;

                Vector3 current =
                    EvaluateCablePoint(
                        start,
                        end,
                        startTangent,
                        endTangent,
                        t);

                Handles.DrawLine(
                    previous,
                    current);

                previous =
                    current;
            }

            Handles.SphereHandleCap(
                0,
                start,
                Quaternion.identity,
                cableRadius * 2f,
                EventType.Repaint);

            Handles.SphereHandleCap(
                0,
                end,
                Quaternion.identity,
                cableRadius * 2f,
                EventType.Repaint);
        }

#endif
    }
}