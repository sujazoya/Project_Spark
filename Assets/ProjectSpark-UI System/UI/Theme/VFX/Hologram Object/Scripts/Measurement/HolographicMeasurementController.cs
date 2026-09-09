using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicMeasurementController
        : MonoBehaviour
    {
        // ============================================================
        // MEASUREMENT
        // ============================================================

        [Header("Measurement")]
        [SerializeField]
        private HolographicMeasurementType measurementType =
            HolographicMeasurementType.Distance;

        public HolographicMeasurementType CurrentType =>
            measurementType;


        public void SetMeasurementType(
            HolographicMeasurementType type)
        {
            if (measurementType == type)
            {
                RefreshHUD();
                return;
            }

            measurementType = type;

            ClearMeasurement();

            RefreshHUD();
        }


        // ============================================================
        // SNAP
        // ============================================================

        [Header("Snap Integration")]
        [SerializeField]
        private HolographicSnapController snapController;

        [SerializeField]
        private bool requireSnap = true;


        // ============================================================
        // DIMENSION VISUAL
        // ============================================================

        [Header("Dimension Visual")]
        [SerializeField]
        private HolographicDimensionVisual dimensionVisual;


        // ============================================================
        // CAMERA
        // ============================================================

        [Header("Viewer")]
        [SerializeField]
        private Camera viewerCamera;


        // ============================================================
        // RAYCAST
        // ============================================================

        [Header("Raycast")]
        [SerializeField]
        private LayerMask measurementLayer;

        [SerializeField]
        private float rayDistance = 100f;


        // ============================================================
        // MEASUREMENT LINE
        // ============================================================

        [Header("Visual")]
        [SerializeField]
        private LineRenderer measurementLine;

        [SerializeField]
        private Transform pointAVisual;

        [SerializeField]
        private Transform pointBVisual;

        [SerializeField]
        private Transform pointCVisual;


        // ============================================================
        // HUD
        // ============================================================

        [Header("HUD")]
        [SerializeField]
        private TMP_Text modeText;

        [SerializeField]
        private TMP_Text valueText;


        // ============================================================
        // DISPLAY
        // ============================================================

        [Header("Display")]
        [SerializeField]
        private float unitsPerMeter = 1000f;

        [SerializeField]
        private string unitSuffix = " mm";


        // ============================================================
        // LINE
        // ============================================================

        [Header("Line")]
        [SerializeField]
        private float lineWidth = 0.008f;


        // ============================================================
        // SAFETY
        // ============================================================

        [Header("Measurement Safety")]
        [SerializeField]
        private float maxMeasurementDistance = 100f;

        [SerializeField]
        private bool rejectDegenerateMeasurements = true;


        // ============================================================
        // POINT
        // ============================================================

        private readonly struct MeasurementPoint
        {
            public readonly Transform Transform;

            public readonly Vector3 LocalPosition;

            public readonly Vector3 WorldPositionAtSelection;

            public readonly HolographicSnapType SnapType;

            public readonly int PrimaryIndex;

            public readonly int SecondaryIndex;


            public MeasurementPoint(
                Transform transform,
                Vector3 localPosition,
                Vector3 worldPosition,
                HolographicSnapType snapType,
                int primaryIndex,
                int secondaryIndex)
            {
                Transform = transform;

                LocalPosition = localPosition;

                WorldPositionAtSelection =
                    worldPosition;

                SnapType = snapType;

                PrimaryIndex = primaryIndex;

                SecondaryIndex = secondaryIndex;
            }


            public Vector3 WorldPosition
            {
                get
                {
                    if (Transform == null)
                    {
                        return WorldPositionAtSelection;
                    }

                    return Transform.TransformPoint(
                        LocalPosition
                    );
                }
            }


            public bool IsValid =>
                Transform != null &&
                IsFinite(WorldPosition);


            private static bool IsFinite(
                Vector3 value)
            {
                return
                    !float.IsNaN(value.x) &&
                    !float.IsNaN(value.y) &&
                    !float.IsNaN(value.z) &&
                    !float.IsInfinity(value.x) &&
                    !float.IsInfinity(value.y) &&
                    !float.IsInfinity(value.z);
            }
        }


        // ============================================================
        // POINT STATE
        // ============================================================

        private MeasurementPoint pointA;
        private MeasurementPoint pointB;
        private MeasurementPoint pointC;

        private bool hasPointA;
        private bool hasPointB;
        private bool hasPointC;


        // ============================================================
        // STATE
        // ============================================================

        private bool active;

        public bool IsActive =>
            active;


        public bool HasMeasurement
        {
            get
            {
                if (measurementType ==
                    HolographicMeasurementType.Angle)
                {
                    return
                        hasPointA &&
                        hasPointB &&
                        hasPointC;
                }

                return
                    hasPointA &&
                    hasPointB;
            }
        }


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            InitializeReferences();
            InitializeVisuals();
            RefreshHUD();
        }


        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            UpdateMeasurementInput();
#endif

            if (!active)
            {
                return;
            }

            UpdateVisuals();
            RefreshHUD();
        }


        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void InitializeReferences()
        {
            if (viewerCamera == null)
            {
                viewerCamera = Camera.main;
            }

            if (snapController == null)
            {
                snapController =
                    GetComponent<HolographicSnapController>();
            }
        }


        private void InitializeVisuals()
        {
            if (measurementLine != null)
            {
                measurementLine.positionCount = 0;

                measurementLine.startWidth =
                    lineWidth;

                measurementLine.endWidth =
                    lineWidth;

                measurementLine.useWorldSpace = true;
            }

            HidePointVisual(
                pointAVisual
            );

            HidePointVisual(
                pointBVisual
            );

            HidePointVisual(
                pointCVisual
            );

            if (dimensionVisual != null)
            {
                dimensionVisual.Hide();
            }
        }


        // ============================================================
        // INPUT
        // ============================================================

#if ENABLE_INPUT_SYSTEM

        private void UpdateMeasurementInput()
        {
            if (!active)
            {
                return;
            }

            if (viewerCamera == null)
            {
                viewerCamera = Camera.main;
            }

            if (viewerCamera == null)
            {
                Debug.LogError(
                    "MEASURE: Viewer Camera is not assigned."
                );

                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    mousePosition
                );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    measurementLayer,
                    QueryTriggerInteraction.Ignore))
            {
                Debug.LogWarning(
                    "MEASURE: Raycast FAILED. " +
                    "Check Measurement Layer."
                );

                return;
            }

            HolographicSnapResult snapResult =
                FindMeasurementSnap(
                    mousePosition,
                    hit
                );

            if (!snapResult.Valid)
            {
                Debug.LogWarning(
                    "MEASURE: Snap result INVALID."
                );

                return;
            }

            AddPoint(
                snapResult
            );
        }

#endif


        // ============================================================
        // SNAP
        // ============================================================

        private HolographicSnapResult FindMeasurementSnap(
            Vector2 mousePosition,
            RaycastHit hit)
        {
            if (snapController != null)
            {
                HolographicSnapResult result =
                    snapController.FindSnap(
                        mousePosition,
                        hit
                    );

                if (result.Valid)
                {
                    return result;
                }
            }

            if (requireSnap)
            {
                return HolographicSnapResult.Invalid;
            }

            return CreateRawSurfaceResult(
                hit
            );
        }


        private HolographicSnapResult CreateRawSurfaceResult(
            RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return HolographicSnapResult.Invalid;
            }

            Transform target =
                hit.collider.transform;

            Vector3 localPosition =
                target.InverseTransformPoint(
                    hit.point
                );

            return new HolographicSnapResult(
                HolographicSnapType.Surface,
                null,
                target,
                localPosition,
                hit.point,
                hit.normal,
                0f,
                0f,
                hit.triangleIndex,
                -1,
                0f
            );
        }


        // ============================================================
        // ADD POINT
        // ============================================================

        private void AddPoint(
            HolographicSnapResult snapResult)
        {
            if (!snapResult.Valid)
            {
                return;
            }

            MeasurementPoint point =
                new MeasurementPoint(
                    snapResult.Transform,
                    snapResult.LocalPosition,
                    snapResult.WorldPosition,
                    snapResult.Type,
                    snapResult.PrimaryIndex,
                    snapResult.SecondaryIndex
                );

            if (!point.IsValid)
            {
                return;
            }

            switch (measurementType)
            {
                case HolographicMeasurementType.Distance:
                    AddDistancePoint(point);
                    break;

                case HolographicMeasurementType.Angle:
                    AddAnglePoint(point);
                    break;

                case HolographicMeasurementType.Radius:
                    AddRadiusPoint(point);
                    break;

                case HolographicMeasurementType.Diameter:
                    AddDiameterPoint(point);
                    break;
            }
        }


        // ============================================================
        // DISTANCE
        // ============================================================

        private void AddDistancePoint(
            MeasurementPoint point)
        {
            if (!hasPointA)
            {
                SetPointA(point);
                return;
            }

            if (!hasPointB)
            {
                if (!IsValidDistance(
                        pointA.WorldPosition,
                        point.WorldPosition))
                {
                    return;
                }

                SetPointB(point);
                return;
            }

            ClearMeasurement();
            SetPointA(point);
        }


        // ============================================================
        // RADIUS
        // ============================================================

        private void AddRadiusPoint(
            MeasurementPoint point)
        {
            if (!hasPointA)
            {
                SetPointA(point);
                return;
            }

            if (!hasPointB)
            {
                if (!IsValidDistance(
                        pointA.WorldPosition,
                        point.WorldPosition))
                {
                    return;
                }

                SetPointB(point);
                return;
            }

            ClearMeasurement();
            SetPointA(point);
        }


        // ============================================================
        // DIAMETER
        // ============================================================

        private void AddDiameterPoint(
            MeasurementPoint point)
        {
            if (!hasPointA)
            {
                SetPointA(point);
                return;
            }

            if (!hasPointB)
            {
                if (!IsValidDistance(
                        pointA.WorldPosition,
                        point.WorldPosition))
                {
                    return;
                }

                SetPointB(point);
                return;
            }

            ClearMeasurement();
            SetPointA(point);
        }


        // ============================================================
        // ANGLE
        // ============================================================

        private void AddAnglePoint(
            MeasurementPoint point)
        {
            if (!hasPointA)
            {
                SetPointA(point);
                return;
            }

            if (!hasPointB)
            {
                if (!IsValidDistance(
                        pointA.WorldPosition,
                        point.WorldPosition))
                {
                    return;
                }

                SetPointB(point);
                return;
            }

            if (!hasPointC)
            {
                if (!IsValidDistance(
                        pointB.WorldPosition,
                        point.WorldPosition))
                {
                    return;
                }

                SetPointC(point);
                return;
            }

            ClearMeasurement();
            SetPointA(point);
        }


        // ============================================================
        // SET POINTS
        // ============================================================

        private void SetPointA(
            MeasurementPoint point)
        {
            pointA = point;

            hasPointA = true;
            hasPointB = false;
            hasPointC = false;

            HidePointVisual(
                pointBVisual
            );

            HidePointVisual(
                pointCVisual
            );

            RefreshHUD();
        }


        private void SetPointB(
            MeasurementPoint point)
        {
            pointB = point;

            hasPointB = true;

            RefreshHUD();
        }


        private void SetPointC(
            MeasurementPoint point)
        {
            pointC = point;

            hasPointC = true;

            RefreshHUD();
        }


        // ============================================================
        // DISTANCE
        // ============================================================

        public float GetDistanceMeters()
        {
            if (!hasPointA ||
                !hasPointB)
            {
                return 0f;
            }

            return Vector3.Distance(
                pointA.WorldPosition,
                pointB.WorldPosition
            );
        }


        // ============================================================
        // RADIUS
        // ============================================================

        public float GetRadius()
        {
            if (!hasPointA ||
                !hasPointB)
            {
                return 0f;
            }

            return Vector3.Distance(
                pointA.WorldPosition,
                pointB.WorldPosition
            );
        }


        // ============================================================
        // DIAMETER
        // ============================================================

        public float GetDiameter()
        {
            if (!hasPointA ||
                !hasPointB)
            {
                return 0f;
            }

            return Vector3.Distance(
                pointA.WorldPosition,
                pointB.WorldPosition
            );
        }


        // ============================================================
        // ANGLE
        // ============================================================

        public float GetAngle()
        {
            if (!hasPointA ||
                !hasPointB ||
                !hasPointC)
            {
                return 0f;
            }

            Vector3 ba =
                pointA.WorldPosition -
                pointB.WorldPosition;

            Vector3 bc =
                pointC.WorldPosition -
                pointB.WorldPosition;

            float baLength =
                ba.magnitude;

            float bcLength =
                bc.magnitude;

            if (baLength <= 0.000001f ||
                bcLength <= 0.000001f)
            {
                return 0f;
            }

            ba /= baLength;
            bc /= bcLength;

            float dot =
                Vector3.Dot(
                    ba,
                    bc
                );

            dot =
                Mathf.Clamp(
                    dot,
                    -1f,
                    1f
                );

            return
                Mathf.Acos(dot) *
                Mathf.Rad2Deg;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private bool IsValidDistance(
            Vector3 a,
            Vector3 b)
        {
            if (!IsFinite(a) ||
                !IsFinite(b))
            {
                return false;
            }

            float distance =
                Vector3.Distance(
                    a,
                    b
                );

            if (rejectDegenerateMeasurements &&
                distance < 0.000001f)
            {
                return false;
            }

            if (maxMeasurementDistance > 0f &&
                distance > maxMeasurementDistance)
            {
                return false;
            }

            return true;
        }


        private static bool IsFinite(
            Vector3 value)
        {
            return
                !float.IsNaN(value.x) &&
                !float.IsNaN(value.y) &&
                !float.IsNaN(value.z) &&
                !float.IsInfinity(value.x) &&
                !float.IsInfinity(value.y) &&
                !float.IsInfinity(value.z);
        }


        // ============================================================
        // VISUALS
        // ============================================================

        private void UpdateVisuals()
        {
            switch (measurementType)
            {
                case HolographicMeasurementType.Distance:
                    UpdateDistanceVisuals();
                    break;

                case HolographicMeasurementType.Radius:
                    UpdateRadiusVisuals();
                    break;

                case HolographicMeasurementType.Diameter:
                    UpdateDiameterVisuals();
                    break;

                case HolographicMeasurementType.Angle:
                    UpdateAngleVisuals();
                    break;
            }
        }


        // ============================================================
        // DISTANCE VISUAL
        // ============================================================

        private void UpdateDistanceVisuals()
        {
            if (!hasPointA)
            {
                ClearMeasurementVisualState();
                return;
            }

            Vector3 a =
                pointA.WorldPosition;

            SetPointVisual(
                pointAVisual,
                a
            );

            if (!hasPointB)
            {
                HidePointVisual(
                    pointBVisual
                );

                HideDimensionVisual();

                return;
            }

            Vector3 b =
                pointB.WorldPosition;

            SetPointVisual(
                pointBVisual,
                b
            );

            DrawLine(
                a,
                b
            );

            if (dimensionVisual != null)
            {
                dimensionVisual.ShowDistance(
                    a,
                    b,
                    Vector3.Distance(a, b),
                    unitSuffix
                );
            }
        }


        // ============================================================
        // RADIUS VISUAL
        // ============================================================

        private void UpdateRadiusVisuals()
        {
            if (!hasPointA)
            {
                ClearMeasurementVisualState();
                return;
            }

            Vector3 center =
                pointA.WorldPosition;

            SetPointVisual(
                pointAVisual,
                center
            );

            if (!hasPointB)
            {
                HidePointVisual(
                    pointBVisual
                );

                HideDimensionVisual();

                return;
            }

            Vector3 rim =
                pointB.WorldPosition;

            SetPointVisual(
                pointBVisual,
                rim
            );

            DrawLine(
                center,
                rim
            );

            if (dimensionVisual != null)
            {
                dimensionVisual.ShowRadius(
                    center,
                    rim,
                    Vector3.Distance(
                        center,
                        rim
                    ),
                    unitSuffix
                );
            }
        }


        // ============================================================
        // DIAMETER VISUAL
        // ============================================================

        private void UpdateDiameterVisuals()
        {
            if (!hasPointA)
            {
                ClearMeasurementVisualState();
                return;
            }

            Vector3 a =
                pointA.WorldPosition;

            SetPointVisual(
                pointAVisual,
                a
            );

            if (!hasPointB)
            {
                HidePointVisual(
                    pointBVisual
                );

                HideDimensionVisual();

                return;
            }

            Vector3 b =
                pointB.WorldPosition;

            SetPointVisual(
                pointBVisual,
                b
            );

            DrawLine(
                a,
                b
            );

            if (dimensionVisual != null)
            {
                dimensionVisual.ShowDiameter(
                    a,
                    b,
                    Vector3.Distance(
                        a,
                        b
                    ),
                    unitSuffix
                );
            }
        }


        // ============================================================
        // ANGLE VISUAL
        // ============================================================

        private void UpdateAngleVisuals()
        {
            if (!hasPointA)
            {
                ClearMeasurementVisualState();
                return;
            }

            Vector3 a =
                pointA.WorldPosition;

            SetPointVisual(
                pointAVisual,
                a
            );

            if (!hasPointB)
            {
                HidePointVisual(
                    pointBVisual
                );

                HidePointVisual(
                    pointCVisual
                );

                HideDimensionVisual();

                return;
            }

            Vector3 b =
                pointB.WorldPosition;

            SetPointVisual(
                pointBVisual,
                b
            );

            if (!hasPointC)
            {
                HidePointVisual(
                    pointCVisual
                );

                DrawLine(
                    a,
                    b
                );

                HideDimensionVisual();

                return;
            }

            Vector3 c =
                pointC.WorldPosition;

            SetPointVisual(
                pointCVisual,
                c
            );

            DrawAngleLines(
                a,
                b,
                c
            );

            if (dimensionVisual != null)
            {
                dimensionVisual.ShowAngle(
                    a,
                    b,
                    c,
                    GetAngle()
                );
            }
        }


        // ============================================================
        // BASE LINE
        // ============================================================

        private void DrawLine(
            Vector3 start,
            Vector3 end)
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.positionCount = 2;

            measurementLine.SetPosition(
                0,
                start
            );

            measurementLine.SetPosition(
                1,
                end
            );

            measurementLine.enabled = true;
        }


        private void DrawAngleLines(
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.positionCount = 3;

            measurementLine.SetPosition(
                0,
                a
            );

            measurementLine.SetPosition(
                1,
                b
            );

            measurementLine.SetPosition(
                2,
                c
            );

            measurementLine.enabled = true;
        }


        private void ClearLine()
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.positionCount = 0;
            measurementLine.enabled = false;
        }


        // ============================================================
        // DIMENSION
        // ============================================================

        private void HideDimensionVisual()
        {
            if (dimensionVisual == null)
            {
                return;
            }

            dimensionVisual.Hide();
        }


        // ============================================================
        // HUD
        // ============================================================

        private void RefreshHUD()
        {
            if (modeText != null)
            {
                modeText.text =
                    active
                        ? GetModeText()
                        : string.Empty;
            }

            if (valueText == null)
            {
                return;
            }

            if (!active)
            {
                valueText.text =
                    string.Empty;

                return;
            }

            switch (measurementType)
            {
                case HolographicMeasurementType.Distance:
                    RefreshDistanceHUD();
                    break;

                case HolographicMeasurementType.Radius:
                    RefreshRadiusHUD();
                    break;

                case HolographicMeasurementType.Diameter:
                    RefreshDiameterHUD();
                    break;

                case HolographicMeasurementType.Angle:
                    RefreshAngleHUD();
                    break;

                default:
                    valueText.text =
                        string.Empty;
                    break;
            }
        }


        private void RefreshDistanceHUD()
        {
            if (!hasPointA)
            {
                valueText.text =
                    "SELECT POINT A";

                return;
            }

            if (!hasPointB)
            {
                valueText.text =
                    "SELECT POINT B";

                return;
            }

            valueText.text =
                FormatDistance(
                    GetDistanceMeters()
                );
        }


        private void RefreshRadiusHUD()
        {
            if (!hasPointA)
            {
                valueText.text =
                    "SELECT CENTER";

                return;
            }

            if (!hasPointB)
            {
                valueText.text =
                    "SELECT RIM";

                return;
            }

            valueText.text =
                "R " +
                FormatDistanceValue(
                    GetRadius()
                ) +
                unitSuffix;
        }


        private void RefreshDiameterHUD()
        {
            if (!hasPointA)
            {
                valueText.text =
                    "SELECT POINT A";

                return;
            }

            if (!hasPointB)
            {
                valueText.text =
                    "SELECT POINT B";

                return;
            }

            valueText.text =
                "Ø " +
                FormatDistanceValue(
                    GetDiameter()
                ) +
                unitSuffix;
        }


        private void RefreshAngleHUD()
        {
            if (!hasPointA)
            {
                valueText.text =
                    "SELECT POINT A";

                return;
            }

            if (!hasPointB)
            {
                valueText.text =
                    "SELECT VERTEX B";

                return;
            }

            if (!hasPointC)
            {
                valueText.text =
                    "SELECT POINT C";

                return;
            }

            valueText.text =
                GetAngle().ToString("0.00") +
                "°";
        }


        // ============================================================
        // FORMAT
        // ============================================================

        private string FormatDistance(
            float meters)
        {
            return
                FormatDistanceValue(
                    meters
                ) +
                unitSuffix;
        }


        private string FormatDistanceValue(
            float meters)
        {
            return
                (
                    meters *
                    unitsPerMeter
                ).ToString(
                    "0.00"
                );
        }


        // ============================================================
        // MODE TEXT
        // ============================================================

        private string GetModeText()
        {
            switch (measurementType)
            {
                case HolographicMeasurementType.Distance:
                    return "DISTANCE MEASURE";

                case HolographicMeasurementType.Angle:
                    return "ANGLE MEASURE";

                case HolographicMeasurementType.Radius:
                    return "RADIUS MEASURE";

                case HolographicMeasurementType.Diameter:
                    return "DIAMETER MEASURE";

                default:
                    return "MEASURE MODE";
            }
        }


        // ============================================================
        // ACTIVE STATE
        // ============================================================

        public void SetActive(bool value)
        {
            active = value;

            if (!active)
            {
                ClearMeasurement();
                return;
            }

            PrepareMeasurementVisuals();
            RefreshHUD();
        }


        public void Toggle()
        {
            SetActive(
                !active
            );
        }


        // ============================================================
        // CLEAR
        // ============================================================

        public void ClearMeasurement()
        {
            hasPointA = false;
            hasPointB = false;
            hasPointC = false;

            ClearLine();

            HidePointVisual(
                pointAVisual
            );

            HidePointVisual(
                pointBVisual
            );

            HidePointVisual(
                pointCVisual
            );

            HideDimensionVisual();

            RefreshHUD();
        }


        // ============================================================
        // PREPARE
        // ============================================================

        private void PrepareMeasurementVisuals()
        {
            HidePointVisual(
                pointAVisual
            );

            HidePointVisual(
                pointBVisual
            );

            HidePointVisual(
                pointCVisual
            );

            ClearLine();

            if (dimensionVisual != null)
            {
                dimensionVisual.Hide();
            }
        }


        // ============================================================
        // PUBLIC ACTIVE HELPERS
        // ============================================================

        public void ActivateMeasurement()
        {
            active = true;

            RefreshHUD();
        }


        public void DeactivateMeasurement()
        {
            active = false;

            ClearMeasurement();

            RefreshHUD();
        }


        public void ToggleMeasurement()
        {
            if (active)
            {
                DeactivateMeasurement();
            }
            else
            {
                ActivateMeasurement();
            }
        }


        // ============================================================
        // POINT VISUAL
        // ============================================================

        private void SetPointVisual(
            Transform visual,
            Vector3 position)
        {
            if (visual == null)
            {
                return;
            }

            visual.gameObject.SetActive(true);

            visual.position =
                position;
        }


        private void HidePointVisual(
            Transform visual)
        {
            if (visual == null)
            {
                return;
            }

            visual.gameObject.SetActive(false);
        }


        // ============================================================
        // CLEAR VISUAL STATE
        // ============================================================

        private void ClearMeasurementVisualState()
        {
            HidePointVisual(
                pointAVisual
            );

            HidePointVisual(
                pointBVisual
            );

            HidePointVisual(
                pointCVisual
            );

            ClearLine();

            HideDimensionVisual();
        }
    }
}