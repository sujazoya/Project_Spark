using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicMeasurementController
        : MonoBehaviour
    {
        [SerializeField]
private HolographicDimensionVisual dimensionVisual;
        private struct MeasurementPoint
        {
            public Transform transform;
            public Vector3 localPosition;

            public MeasurementPoint(
                Transform transform,
                Vector3 localPosition)
            {
                this.transform = transform;
                this.localPosition = localPosition;
            }

            public Vector3 WorldPosition
            {
                get
                {
                    if (transform == null)
                        return Vector3.zero;

                    return transform.TransformPoint(
                        localPosition
                    );
                }
            }
        }

        [Header("Viewer")]
        [SerializeField] private Camera viewerCamera;

        [Header("Raycast")]
        [SerializeField] private LayerMask measurementLayer;
        [SerializeField] private float rayDistance = 100f;

        [Header("Visual")]
        [SerializeField] private LineRenderer measurementLine;
        [SerializeField] private Transform pointAVisual;
        [SerializeField] private Transform pointBVisual;

        [Header("HUD")]
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text valueText;

        [Header("Display")]
        [SerializeField] private float unitsPerMeter = 1000f;
        [SerializeField] private string unitSuffix = " mm";

        [Header("Line")]
        [SerializeField] private float lineWidth = 0.008f;

        private MeasurementPoint pointA;
        private MeasurementPoint pointB;

        private bool hasPointA;
        private bool hasPointB;
        private bool active;

        public bool IsActive =>
            active;

        private void Awake()
        {
            if (measurementLine != null)
            {
                measurementLine.positionCount = 0;
                measurementLine.startWidth = lineWidth;
                measurementLine.endWidth = lineWidth;
            }

            HidePointVisual(pointAVisual);
            HidePointVisual(pointBVisual);

            RefreshHUD();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            UpdateMeasurementInput();
#endif

            UpdateVisuals();
        }

#if ENABLE_INPUT_SYSTEM

        private void UpdateMeasurementInput()
        {
            if (viewerCamera == null ||
                Mouse.current == null)
            {
                return;
            }

            if (!active)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            Ray ray =
                viewerCamera.ScreenPointToRay(
                    Mouse.current.position.ReadValue()
                );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    measurementLayer,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            AddPoint(hit);
        }

#endif

        private void AddPoint(RaycastHit hit)
        {
            Transform hitTransform =
                hit.collider.transform;

            Vector3 localPoint =
                hitTransform.InverseTransformPoint(
                    hit.point
                );

            MeasurementPoint point =
                new MeasurementPoint(
                    hitTransform,
                    localPoint
                );

            if (!hasPointA)
            {
                pointA = point;
                hasPointA = true;

                hasPointB = false;

                HidePointVisual(pointBVisual);

                UpdateLine();

                RefreshHUD();
                return;
            }

            if (!hasPointB)
            {
                pointB = point;
                hasPointB = true;

                UpdateLine();
                RefreshHUD();
                return;
            }

            // Third point starts a new measurement.
            pointA = point;
            hasPointA = true;

            hasPointB = false;

            UpdateLine();
            RefreshHUD();
        }

        private void UpdateVisuals()
        {
            if (!active)
                return;

            if (!hasPointA)
                return;

            Vector3 a =
                pointA.WorldPosition;

            if (hasPointB)
            {
                Vector3 b =
                    pointB.WorldPosition;

                if (measurementLine != null)
                {
                    measurementLine.positionCount = 2;
                    measurementLine.SetPosition(0, a);
                    measurementLine.SetPosition(1, b);
                }

                SetPointVisual(
                    pointAVisual,
                    a
                );

                SetPointVisual(
                    pointBVisual,
                    b
                );

                return;
            }

            SetPointVisual(
                pointAVisual,
                a
            );

            HidePointVisual(pointBVisual);

            if (measurementLine != null)
            {
                measurementLine.positionCount = 0;
            }
        }

        private void UpdateLine()
        {
            UpdateVisuals();
        }

        private void RefreshHUD()
        {
            if (modeText != null)
            {
                modeText.text =
                    active
                        ? "MEASURE MODE"
                        : string.Empty;
            }

            if (valueText == null)
                return;

            if (!active)
            {
                valueText.text = string.Empty;
                return;
            }

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

            float distance =
                Vector3.Distance(
                    pointA.WorldPosition,
                    pointB.WorldPosition
                );
                if (dimensionVisual != null)
            {
                dimensionVisual.Show(
                    pointA.WorldPosition,
                    pointB.WorldPosition,
                    distance,
                    unitSuffix
                );
            }

            float displayed =
                distance * unitsPerMeter;

            valueText.text =
                displayed.ToString("0.00") +
                unitSuffix;

                if (dimensionVisual != null)
                {
                    dimensionVisual.Hide();
                }
        }

        private void SetPointVisual(
            Transform visual,
            Vector3 position)
        {
            if (visual == null)
                return;

            visual.gameObject.SetActive(true);
            visual.position = position;
        }

        private void HidePointVisual(
            Transform visual)
        {
            if (visual == null)
                return;

            visual.gameObject.SetActive(false);
        }

        public void SetActive(bool value)
        {
            active = value;

            if (!active)
            {
                ClearMeasurement();
            }

            RefreshHUD();
        }

        public void Toggle()
        {
            SetActive(!active);
        }

        public void ClearMeasurement()
        {
            hasPointA = false;
            hasPointB = false;

            if (measurementLine != null)
            {
                measurementLine.positionCount = 0;
            }

            HidePointVisual(pointAVisual);
            HidePointVisual(pointBVisual);

            RefreshHUD();
            if (dimensionVisual != null)
            {
                dimensionVisual.Hide();
            }
        }

        public bool HasMeasurement =>
            hasPointA && hasPointB;

        public float GetDistanceMeters()
        {
            if (!HasMeasurement)
                return 0f;

            return Vector3.Distance(
                pointA.WorldPosition,
                pointB.WorldPosition
            );
        }
    }
}