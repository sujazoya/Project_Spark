using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicCircularFeature
        : MonoBehaviour
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        [Header("Identity")]

        [SerializeField]
        private string featureId = "CircularFeature";

        [SerializeField]
        private HolographicCircularFeatureType featureType =
            HolographicCircularFeatureType.Circle;


        // ============================================================
        // TRANSFORM
        // ============================================================

        [Header("Geometry Transform")]

        [SerializeField]
        private Transform featureTransform;

        [SerializeField]
        private Vector3 localCenter =
            Vector3.zero;

        [SerializeField]
        private Vector3 localAxis =
            Vector3.up;


        // ============================================================
        // SIZE
        // ============================================================

        [Header("Dimensions")]

        [SerializeField]
        [Min(0.00001f)]
        private float radius = 0.01f;

        [SerializeField]
        [Min(0f)]
        private float depth = 0.01f;


        // ============================================================
        // MEASUREMENT
        // ============================================================

        [Header("Measurement")]

        [SerializeField]
        private bool enabledForMeasurement = true;

        [SerializeField]
        private HolographicCircularMeasurementCapabilities
            measurementCapabilities =
            HolographicCircularMeasurementCapabilities.All;


        // ============================================================
        // VISUAL
        // ============================================================

        [Header("Visual")]

        [SerializeField]
        private bool visibleInViewer = true;

        [SerializeField]
        private bool showCenterMarker = true;

        [SerializeField]
        private bool showAxis = true;

        [SerializeField]
        private bool showRim = true;


        // ============================================================
        // PROPERTIES
        // ============================================================

        public string FeatureId =>
            featureId;

        public HolographicCircularFeatureType FeatureType =>
            featureType;

        public Transform FeatureTransform =>
            featureTransform != null
                ? featureTransform
                : transform;

        public float Radius =>
            radius;

        public float Diameter =>
            radius * 2f;

        public float Depth =>
            depth;

        public bool EnabledForMeasurement =>
            enabledForMeasurement;

        public bool VisibleInViewer =>
            visibleInViewer;

        public HolographicCircularMeasurementCapabilities
            MeasurementCapabilities =>
            measurementCapabilities;


        // ============================================================
        // WORLD CENTER
        // ============================================================

        public Vector3 WorldCenter
        {
            get
            {
                return FeatureTransform.TransformPoint(
                    localCenter
                );
            }
        }


        // ============================================================
        // WORLD AXIS
        // ============================================================

        public Vector3 WorldAxis
        {
            get
            {
                Vector3 axis =
                    FeatureTransform.TransformDirection(
                        localAxis
                    );

                if (axis.sqrMagnitude <
                    0.000001f)
                {
                    return FeatureTransform.up;
                }

                return axis.normalized;
            }
        }


        // ============================================================
        // FEATURE START
        // ============================================================

        public Vector3 WorldStart
        {
            get
            {
                return WorldCenter -
                       WorldAxis *
                       (depth * 0.5f);
            }
        }


        // ============================================================
        // FEATURE END
        // ============================================================

        public Vector3 WorldEnd
        {
            get
            {
                return WorldCenter +
                       WorldAxis *
                       (depth * 0.5f);
            }
        }


        // ============================================================
        // FEATURE MIDPOINT
        // ============================================================

        public Vector3 WorldMidpoint =>
            (WorldStart + WorldEnd) * 0.5f;


        // ============================================================
        // CAP CENTERS
        // ============================================================

        public Vector3 WorldFrontCenter =>
            WorldStart;

        public Vector3 WorldBackCenter =>
            WorldEnd;


        // ============================================================
        // CAP NORMALS
        // ============================================================

        public Vector3 WorldFrontNormal =>
            -WorldAxis;

        public Vector3 WorldBackNormal =>
            WorldAxis;


        // ============================================================
        // CAPABILITY CHECK
        // ============================================================

        public bool Supports(
            HolographicCircularMeasurementCapabilities
                capability)
        {
            return
                (measurementCapabilities &
                 capability) != 0;
        }


        // ============================================================
        // RIM POINT
        // ============================================================

        public Vector3 GetWorldRimPoint(
            Vector3 direction)
        {
            Vector3 axis =
                WorldAxis;

            Vector3 projected =
                Vector3.ProjectOnPlane(
                    direction,
                    axis
                );

            if (projected.sqrMagnitude <
                0.000001f)
            {
                projected =
                    Vector3.ProjectOnPlane(
                        FeatureTransform.right,
                        axis
                    );
            }

            if (projected.sqrMagnitude <
                0.000001f)
            {
                projected =
                    Vector3.Cross(
                        axis,
                        Vector3.forward
                    );
            }

            projected.Normalize();

            return WorldCenter +
                   projected *
                   radius;
        }


        // ============================================================
        // RIM POINT WITH ANGLE
        // ============================================================

        public Vector3 GetWorldRimPoint(
            float angleDegrees)
        {
            GetWorldBasis(
                out Vector3 right,
                out Vector3 forward
            );

            float radians =
                angleDegrees *
                Mathf.Deg2Rad;

            Vector3 direction =
                right *
                Mathf.Cos(radians) +
                forward *
                Mathf.Sin(radians);

            return WorldCenter +
                   direction *
                   radius;
        }


        // ============================================================
        // BASIS
        // ============================================================

        public void GetWorldBasis(
            out Vector3 right,
            out Vector3 forward)
        {
            Vector3 axis =
                WorldAxis;

            right =
                Vector3.ProjectOnPlane(
                    FeatureTransform.right,
                    axis
                );

            if (right.sqrMagnitude <
                0.000001f)
            {
                right =
                    Vector3.ProjectOnPlane(
                        Vector3.right,
                        axis
                    );
            }

            if (right.sqrMagnitude <
                0.000001f)
            {
                right =
                    Vector3.ProjectOnPlane(
                        Vector3.forward,
                        axis
                    );
            }

            right.Normalize();

            forward =
                Vector3.Cross(
                    axis,
                    right
                ).normalized;
        }


        // ============================================================
        // PROJECT POINT ON AXIS
        // ============================================================

        public Vector3 ProjectPointToAxis(
            Vector3 worldPoint)
        {
            Vector3 axis =
                WorldAxis;

            Vector3 center =
                WorldCenter;

            float distance =
                Vector3.Dot(
                    worldPoint - center,
                    axis
                );

            return
                center +
                axis * distance;
        }


        // ============================================================
        // AXIAL DISTANCE
        // ============================================================

        public float GetSignedAxialDistance(
            Vector3 worldPoint)
        {
            return Vector3.Dot(
                worldPoint - WorldCenter,
                WorldAxis
            );
        }


        // ============================================================
        // RADIAL DISTANCE
        // ============================================================

        public float GetRadialDistance(
            Vector3 worldPoint)
        {
            Vector3 projected =
                ProjectPointToAxis(
                    worldPoint
                );

            return Vector3.Distance(
                projected,
                worldPoint
            );
        }


        // ============================================================
        // DISTANCE TO AXIS
        // ============================================================

        public float GetDistanceToAxis(
            Vector3 worldPoint)
        {
            return GetRadialDistance(
                worldPoint
            );
        }


        // ============================================================
        // CLOSEST POINT ON RIM
        // ============================================================

        public Vector3 GetClosestPointOnRim(
            Vector3 worldPoint)
        {
            Vector3 axis =
                WorldAxis;

            Vector3 offset =
                worldPoint -
                WorldCenter;

            Vector3 projected =
                Vector3.ProjectOnPlane(
                    offset,
                    axis
                );

            if (projected.sqrMagnitude <
                0.000001f)
            {
                return GetWorldRimPoint(
                    FeatureTransform.right
                );
            }

            return
                WorldCenter +
                projected.normalized *
                radius;
        }


        // ============================================================
        // CLOSEST POINT ON AXIS SEGMENT
        // ============================================================

        public Vector3 GetClosestPointOnAxisSegment(
            Vector3 worldPoint)
        {
            Vector3 start =
                WorldStart;

            Vector3 axis =
                WorldAxis;

            float distance =
                Vector3.Dot(
                    worldPoint - start,
                    axis
                );

            float clamped =
                Mathf.Clamp(
                    distance,
                    0f,
                    depth
                );

            return
                start +
                axis * clamped;
        }


        // ============================================================
        // POINT INSIDE CYLINDRICAL VOLUME
        // ============================================================

        public bool ContainsWorldPoint(
            Vector3 worldPoint)
        {
            Vector3 axisPoint =
                GetClosestPointOnAxisSegment(
                    worldPoint
                );

            float axialDistance =
                Vector3.Distance(
                    axisPoint,
                    ProjectPointToAxis(
                        worldPoint
                    )
                );

            return
                axialDistance <= radius;
        }


        // ============================================================
        // CHECK AXIAL RANGE
        // ============================================================

        public bool IsWithinDepth(
            Vector3 worldPoint)
        {
            float axial =
                Mathf.Abs(
                    GetSignedAxialDistance(
                        worldPoint
                    )
                );

            return
                axial <=
                depth * 0.5f;
        }


        // ============================================================
        // UPDATE
        // ============================================================

        private void OnValidate()
        {
            if (featureTransform == null)
            {
                featureTransform =
                    transform;
            }

            if (localAxis.sqrMagnitude <
                0.000001f)
            {
                localAxis =
                    Vector3.up;
            }

            localAxis.Normalize();

            radius =
                Mathf.Max(
                    0.00001f,
                    radius
                );

            depth =
                Mathf.Max(
                    0f,
                    depth
                );
        }
    }
}