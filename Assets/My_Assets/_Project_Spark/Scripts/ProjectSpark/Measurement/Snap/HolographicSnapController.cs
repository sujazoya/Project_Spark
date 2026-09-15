using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicSnapController
        : MonoBehaviour
    {
        [Header("Camera")]

        [SerializeField]
        private Camera viewerCamera;

        [Header("Geometry")]

        [SerializeField]
        private HolographicSnapGeometry[] geometries;

        [Header("Settings")]

        [SerializeField]
        private HolographicSnapSettings settings;

        [SerializeField]
        private bool snapEnabled = true;

        public bool IsSnapEnabled =>
            snapEnabled;

        private HolographicSnapResult currentSnap;

        private bool hasCurrentSnap;

        public HolographicSnapResult CurrentSnap =>
            hasCurrentSnap
                ? currentSnap
                : HolographicSnapResult.Invalid;

        private void Reset()
        {
            viewerCamera =
                Camera.main;

            settings =
                new HolographicSnapSettings
                {
                    vertexEnabled = true,

                    edgeEnabled = true,

                    faceCenterEnabled = true,

                    surfaceEnabled = true,

                    vertexPixelDistance = 16f,

                    edgePixelDistance = 12f,

                    facePixelDistance = 18f,

                    vertexPriority = 1.00f,

                    edgePriority = 0.75f,

                    facePriority = 0.50f,

                    distanceWeight = 1.00f,

                    priorityWeight = 0.35f,

                    useDepthFiltering = true,

                    maxDepthDifference = 0.05f,

                    depthWeight = 0.25f,

                    useHysteresis = true,

                    switchMarginPixels = 3f,

                    stickyDistancePixels = 20f,

                    restrictToHitObject = true,

                    requireFrontFacingFace = true
                };
        }

        public void SetSnapEnabled(
    bool value)
{
    snapEnabled = value;

    if (!snapEnabled)
    {
        ClearSnap();
    }
}

public void StartSnapping()
{
    SetSnapEnabled(true);
}

public void StopSnapping()
{
    SetSnapEnabled(false);
}

public void ToggleSnapping()
{
    SetSnapEnabled(!snapEnabled);
}

        private void Awake()
        {
            CacheGeometries();
        }

        public void ClearSnap()
        {
            hasCurrentSnap = false;

            currentSnap =
                HolographicSnapResult.Invalid;
        }

        public HolographicSnapResult FindSnap(
            Vector2 screenPosition,
            RaycastHit hit)
        {
            if (viewerCamera == null)
            {
                ClearSnap();

                return
                    HolographicSnapResult.Invalid;
            }

            float hitDepth =
                GetHitDepth(hit);

            HolographicSnapResult best =
                HolographicSnapResult.Invalid;

            float bestScore =
                float.MaxValue;

            if (settings.vertexEnabled)
            {
                FindVertices(
                    screenPosition,
                    hit,
                    hitDepth,
                    ref best,
                    ref bestScore
                );
            }

            if (settings.edgeEnabled)
            {
                FindEdges(
                    screenPosition,
                    hit,
                    hitDepth,
                    ref best,
                    ref bestScore
                );
            }

            if (settings.faceCenterEnabled)
            {
                FindFaces(
                    screenPosition,
                    hit,
                    hitDepth,
                    ref best,
                    ref bestScore
                );
            }

            if (settings.surfaceEnabled &&
                !best.Valid)
            {
                best =
                    CreateSurfaceResult(
                        hit
                    );
            }

            best =
                ApplyHysteresis(
                    screenPosition,
                    best,
                    ref bestScore
                );

            if (best.Valid)
            {
                currentSnap =
                    best;

                hasCurrentSnap =
                    true;
            }
            else
            {
                ClearSnap();
            }

            return best;
        }

        private void CacheGeometries()
        {
            if (geometries != null &&
                geometries.Length > 0)
            {
                return;
            }

            geometries =
                GetComponentsInChildren<
                    HolographicSnapGeometry>(
                        true
                    );
        }

        // --------------------------------------------------
        // VERTICES
        // --------------------------------------------------

        private void FindVertices(
            Vector2 mousePosition,
            RaycastHit hit,
            float hitDepth,
            ref HolographicSnapResult best,
            ref float bestScore)
        {
            for (int g = 0;
                 g < geometries.Length;
                 g++)
            {
                HolographicSnapGeometry geometry =
                    geometries[g];

                if (!IsGeometryEligible(
                        geometry,
                        hit))
                {
                    continue;
                }

                Transform target =
                    geometry.SourceTransform;

                for (int i = 0;
                     i < geometry.VertexCount;
                     i++)
                {
                    Vector3 local =
                        geometry.GetVertexLocal(i);

                    Vector3 world =
                        target.TransformPoint(local);

                    Vector3 screen =
                        viewerCamera.WorldToScreenPoint(
                            world
                        );

                    if (!IsScreenPointValid(
                            screen))
                    {
                        continue;
                    }

                    float distance =
                        Vector2.Distance(
                            mousePosition,
                            new Vector2(
                                screen.x,
                                screen.y
                            )
                        );

                    if (distance >
                        settings.vertexPixelDistance)
                    {
                        continue;
                    }

                    if (IsDepthRejected(
                            screen.z,
                            hitDepth))
                    {
                        continue;
                    }

                    float score =
                        CalculateScore(
                            distance,
                            settings.vertexPixelDistance,
                            settings.vertexPriority,
                            screen.z,
                            hitDepth
                        );

                    if (score >= bestScore)
                        continue;

                    bestScore =
                        score;

                    best =
                        new HolographicSnapResult(
                            HolographicSnapType.Vertex,
                            geometry,
                            target,
                            local,
                            world,
                            Vector3.zero,
                            distance,
                            screen.z,
                            i,
                            -1,
                            0f
                        );
                }
            }
        }

        // --------------------------------------------------
        // EDGES
        // --------------------------------------------------

        private void FindEdges(
            Vector2 mousePosition,
            RaycastHit hit,
            float hitDepth,
            ref HolographicSnapResult best,
            ref float bestScore)
        {
            for (int g = 0;
                 g < geometries.Length;
                 g++)
            {
                HolographicSnapGeometry geometry =
                    geometries[g];

                if (!IsGeometryEligible(
                        geometry,
                        hit))
                {
                    continue;
                }

                Transform target =
                    geometry.SourceTransform;

                for (int i = 0;
                     i < geometry.EdgeCount;
                     i++)
                {
                    geometry.GetEdge(
                        i,
                        out int indexA,
                        out int indexB,
                        out Vector3 localA,
                        out Vector3 localB,
                        out _
                    );

                    Vector3 worldA =
                        target.TransformPoint(
                            localA
                        );

                    Vector3 worldB =
                        target.TransformPoint(
                            localB
                        );

                    Vector3 screenA =
                        viewerCamera.WorldToScreenPoint(
                            worldA
                        );

                    Vector3 screenB =
                        viewerCamera.WorldToScreenPoint(
                            worldB
                        );

                    if (!IsScreenPointValid(
                            screenA) ||
                        !IsScreenPointValid(
                            screenB))
                    {
                        continue;
                    }

                    Vector2 a =
                        new Vector2(
                            screenA.x,
                            screenA.y
                        );

                    Vector2 b =
                        new Vector2(
                            screenB.x,
                            screenB.y
                        );

                    Vector2 closest =
                        ClosestPointOnSegment(
                            mousePosition,
                            a,
                            b,
                            out float t
                        );

                    float distance =
                        Vector2.Distance(
                            mousePosition,
                            closest
                        );

                    if (distance >
                        settings.edgePixelDistance)
                    {
                        continue;
                    }

                    float depth =
                        Mathf.Lerp(
                            screenA.z,
                            screenB.z,
                            t
                        );

                    if (IsDepthRejected(
                            depth,
                            hitDepth))
                    {
                        continue;
                    }

                    Vector3 worldPoint =
                        Vector3.Lerp(
                            worldA,
                            worldB,
                            t
                        );

                    Vector3 localPoint =
                        Vector3.Lerp(
                            localA,
                            localB,
                            t
                        );

                    Vector3 edgeDirection =
                        (worldB - worldA)
                        .normalized;

                    float score =
                        CalculateScore(
                            distance,
                            settings.edgePixelDistance,
                            settings.edgePriority,
                            depth,
                            hitDepth
                        );

                    if (score >= bestScore)
                        continue;

                    bestScore =
                        score;

                    best =
                        new HolographicSnapResult(
                            HolographicSnapType.Edge,
                            geometry,
                            target,
                            localPoint,
                            worldPoint,
                            edgeDirection,
                            distance,
                            depth,
                            indexA,
                            indexB,
                            t
                        );
                }
            }
        }

        // --------------------------------------------------
        // FACE CENTERS
        // --------------------------------------------------

        private void FindFaces(
            Vector2 mousePosition,
            RaycastHit hit,
            float hitDepth,
            ref HolographicSnapResult best,
            ref float bestScore)
        {
            for (int g = 0;
                 g < geometries.Length;
                 g++)
            {
                HolographicSnapGeometry geometry =
                    geometries[g];

                if (!IsGeometryEligible(
                        geometry,
                        hit))
                {
                    continue;
                }

                Transform target =
                    geometry.SourceTransform;

                for (int i = 0;
                     i < geometry.FaceCount;
                     i++)
                {
                    geometry.GetFace(
                        i,
                        out Vector3 localCenter,
                        out Vector3 localNormal,
                        out _,
                        out _,
                        out _
                    );

                    Vector3 worldCenter =
                        target.TransformPoint(
                            localCenter
                        );

                    Vector3 worldNormal =
                        target.TransformDirection(
                            localNormal
                        ).normalized;

                    if (settings.requireFrontFacingFace)
                    {
                        Vector3 cameraDirection =
                            viewerCamera.transform.position -
                            worldCenter;

                        cameraDirection.Normalize();

                        float facing =
                            Vector3.Dot(
                                worldNormal,
                                cameraDirection
                            );

                        if (facing <= 0f)
                            continue;
                    }

                    Vector3 screen =
                        viewerCamera.WorldToScreenPoint(
                            worldCenter
                        );

                    if (!IsScreenPointValid(
                            screen))
                    {
                        continue;
                    }

                    float distance =
                        Vector2.Distance(
                            mousePosition,
                            new Vector2(
                                screen.x,
                                screen.y
                            )
                        );

                    if (distance >
                        settings.facePixelDistance)
                    {
                        continue;
                    }

                    if (IsDepthRejected(
                            screen.z,
                            hitDepth))
                    {
                        continue;
                    }

                    float score =
                        CalculateScore(
                            distance,
                            settings.facePixelDistance,
                            settings.facePriority,
                            screen.z,
                            hitDepth
                        );

                    if (score >= bestScore)
                        continue;

                    bestScore =
                        score;

                    best =
                        new HolographicSnapResult(
                            HolographicSnapType.FaceCenter,
                            geometry,
                            target,
                            localCenter,
                            worldCenter,
                            worldNormal,
                            distance,
                            screen.z,
                            i,
                            -1,
                            0f
                        );
                }
            }
        }

        // --------------------------------------------------
        // SURFACE
        // --------------------------------------------------

        private HolographicSnapResult
            CreateSurfaceResult(
                RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return
                    HolographicSnapResult.Invalid;
            }

            Transform target =
                hit.collider.transform;

            Vector3 local =
                target.InverseTransformPoint(
                    hit.point
                );

            float depth =
                GetHitDepth(hit);

            return
                new HolographicSnapResult(
                    HolographicSnapType.Surface,
                    null,
                    target,
                    local,
                    hit.point,
                    hit.normal,
                    0f,
                    depth,
                    hit.triangleIndex,
                    -1,
                    0f
                );
        }

        // --------------------------------------------------
        // HYSTERESIS
        // --------------------------------------------------

        private HolographicSnapResult
            ApplyHysteresis(
                Vector2 mousePosition,
                HolographicSnapResult best,
                ref float bestScore)
        {
            if (!settings.useHysteresis)
                return best;

            if (!hasCurrentSnap)
                return best;

            if (!currentSnap.Valid)
                return best;

            Vector3 screen =
                viewerCamera.WorldToScreenPoint(
                    currentSnap.WorldPosition
                );

            if (!IsScreenPointValid(screen))
                return best;

            float currentDistance =
                Vector2.Distance(
                    mousePosition,
                    new Vector2(
                        screen.x,
                        screen.y
                    )
                );

            if (currentDistance >
                settings.stickyDistancePixels)
            {
                return best;
            }

            if (!best.Valid)
            {
                return currentSnap;
            }

            float currentScore =
                CalculateCurrentScore(
                    currentDistance
                );

            if (currentScore <=
                bestScore +
                settings.switchMarginPixels)
            {
                return currentSnap;
            }

            return best;
        }

        private float CalculateCurrentScore(
            float screenDistance)
        {
            float normalized =
                screenDistance /
                Mathf.Max(
                    settings.stickyDistancePixels,
                    1f
                );

            return
                normalized *
                settings.distanceWeight;
        }

        // --------------------------------------------------
        // SCORING
        // --------------------------------------------------

        private float CalculateScore(
            float distance,
            float maxDistance,
            float priority,
            float depth,
            float hitDepth)
        {
            float normalizedDistance =
                distance /
                Mathf.Max(
                    maxDistance,
                    0.001f
                );

            float score =
                normalizedDistance *
                settings.distanceWeight;

            score -=
                priority *
                settings.priorityWeight;

            if (settings.useDepthFiltering)
            {
                float depthDifference =
                    Mathf.Abs(
                        depth -
                        hitDepth
                    );

                float normalizedDepth =
                    depthDifference /
                    Mathf.Max(
                        hitDepth,
                        0.001f
                    );

                score +=
                    normalizedDepth *
                    settings.depthWeight;
            }

            return score;
        }

        // --------------------------------------------------
        // FILTERING
        // --------------------------------------------------

        private bool IsGeometryEligible(
            HolographicSnapGeometry geometry,
            RaycastHit hit)
        {
            if (geometry == null)
                return false;

            if (!settings.restrictToHitObject)
                return true;

            if (hit.collider == null)
                return false;

            Transform source =
                geometry.SourceTransform;

            Transform colliderTransform =
                hit.collider.transform;

            return
                colliderTransform == source ||
                colliderTransform.IsChildOf(source) ||
                source.IsChildOf(
                    colliderTransform
                );
        }

        private bool IsDepthRejected(
            float candidateDepth,
            float hitDepth)
        {
            if (!settings.useDepthFiltering)
                return false;

            if (hitDepth <= 0f)
                return false;

            float difference =
                Mathf.Abs(
                    candidateDepth -
                    hitDepth
                );

            return
                difference >
                settings.maxDepthDifference;
        }

        private float GetHitDepth(
            RaycastHit hit)
        {
            if (hit.collider == null)
                return 0f;

            Vector3 screen =
                viewerCamera.WorldToScreenPoint(
                    hit.point
                );

            return screen.z;
        }

        private static bool IsScreenPointValid(
            Vector3 screen)
        {
            return screen.z > 0f &&
                   !float.IsNaN(screen.x) &&
                   !float.IsNaN(screen.y);
        }

        // --------------------------------------------------
        // MATH
        // --------------------------------------------------

        private static Vector2
            ClosestPointOnSegment(
                Vector2 point,
                Vector2 a,
                Vector2 b,
                out float t)
        {
            Vector2 direction =
                b - a;

            float lengthSquared =
                direction.sqrMagnitude;

            if (lengthSquared <
                0.000001f)
            {
                t = 0f;

                return a;
            }

            t =
                Vector2.Dot(
                    point - a,
                    direction
                )
                /
                lengthSquared;

            t =
                Mathf.Clamp01(t);

            return
                a +
                direction * t;
        }
    }
}