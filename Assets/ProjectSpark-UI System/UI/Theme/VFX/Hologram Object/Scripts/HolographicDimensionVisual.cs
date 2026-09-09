using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicDimensionVisual : MonoBehaviour
    {
        // ============================================================
        // CAMERA
        // ============================================================

        [Header("Camera")]
        [SerializeField]
        private Camera viewerCamera;


        // ============================================================
        // DISTANCE
        // ============================================================

        [Header("Distance")]
        [SerializeField]
        private LineRenderer distanceDimensionLine;

        [SerializeField]
        private LineRenderer distanceExtensionA;

        [SerializeField]
        private LineRenderer distanceExtensionB;


        // ============================================================
        // RADIUS
        // ============================================================

        [Header("Radius")]
        [SerializeField]
        private LineRenderer radiusLine;

        [SerializeField]
        private LineRenderer radiusExtension;


        // ============================================================
        // DIAMETER
        // ============================================================

        [Header("Diameter")]
        [SerializeField]
        private LineRenderer diameterLine;

        [SerializeField]
        private LineRenderer diameterExtension;


        // ============================================================
        // ANGLE
        // ============================================================

        [Header("Angle")]
        [SerializeField]
        private LineRenderer angleArmA;

        [SerializeField]
        private LineRenderer angleArmB;

        [SerializeField]
        private LineRenderer angleArc;


        // ============================================================
        // ARROWS
        // ============================================================

        [Header("Arrow Heads")]
        [SerializeField]
        private Transform arrowA;

        [SerializeField]
        private Transform arrowB;


        // ============================================================
        // LABEL
        // ============================================================

        [Header("Label")]
        [SerializeField]
        private RectTransform label;

        [SerializeField]
        private TMP_Text valueText;


        // ============================================================
        // DISTANCE SETTINGS
        // ============================================================

        [Header("Distance Layout")]
        [SerializeField]
        private float dimensionOffset = 0.18f;

        [SerializeField]
        private float extensionBackOffset = 0.0f;

        [SerializeField]
        private float distanceLabelPixelOffset = 14.0f;


        // ============================================================
        // RADIUS SETTINGS
        // ============================================================

        [Header("Radius Layout")]
        [SerializeField]
        private float radiusLabelOffset = 0.08f;

        [SerializeField]
        private float radiusExtensionLength = 0.06f;


        // ============================================================
        // DIAMETER SETTINGS
        // ============================================================

        [Header("Diameter Layout")]
        [SerializeField]
        private float diameterLabelPixelOffset = 14.0f;

        [SerializeField]
        private float diameterExtensionLength = 0.06f;


        // ============================================================
        // ANGLE SETTINGS
        // ============================================================

        [Header("Angle Layout")]
        [SerializeField]
        private float angleArmLength = 0.35f;

        [SerializeField]
        private float angleArcRadius = 0.22f;

        [SerializeField]
        [Range(4, 128)]
        private int angleArcSegments = 32;

        [SerializeField]
        private float angleLabelOffset = 0.08f;


        // ============================================================
        // STATE
        // ============================================================

        private VisualMode currentMode;

        private bool visible;

        private Vector3 pointA;
        private Vector3 pointB;
        private Vector3 pointC;

        private float displayedValue;


        // ============================================================
        // MODE
        // ============================================================

        private enum VisualMode
        {
            None,
            Distance,
            Angle,
            Radius,
            Diameter
        }


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (viewerCamera == null)
            {
                viewerCamera = Camera.main;
            }

            Hide();
        }


        private void LateUpdate()
        {
            if (!visible)
            {
                return;
            }

            if (viewerCamera == null)
            {
                viewerCamera = Camera.main;
            }

            if (viewerCamera == null)
            {
                return;
            }

            UpdateVisual();
        }


        // ============================================================
        // DISTANCE
        // ============================================================

        public void ShowDistance(
            Vector3 a,
            Vector3 b,
            float distance,
            string suffix)
        {
            pointA = a;
            pointB = b;
            displayedValue = distance;

            currentMode = VisualMode.Distance;
            visible = true;

            SetDistanceActive(true);
            SetRadiusActive(false);
            SetDiameterActive(false);
            SetAngleActive(false);

            SetLabelText(
                FormatValue(distance, suffix)
            );

            UpdateDistanceVisual();
        }


        // ============================================================
        // RADIUS
        // ============================================================

        public void ShowRadius(
            Vector3 center,
            Vector3 rim,
            float radius,
            string suffix)
        {
            pointA = center;
            pointB = rim;
            displayedValue = radius;

            currentMode = VisualMode.Radius;
            visible = true;

            SetDistanceActive(false);
            SetRadiusActive(true);
            SetDiameterActive(false);
            SetAngleActive(false);

            SetLabelText(
                "R " +
                FormatValue(radius, suffix)
            );

            UpdateRadiusVisual();
        }


        // ============================================================
        // DIAMETER
        // ============================================================

        public void ShowDiameter(
            Vector3 a,
            Vector3 b,
            float diameter,
            string suffix)
        {
            pointA = a;
            pointB = b;
            displayedValue = diameter;

            currentMode = VisualMode.Diameter;
            visible = true;

            SetDistanceActive(false);
            SetRadiusActive(false);
            SetDiameterActive(true);
            SetAngleActive(false);

            SetLabelText(
                "Ø " +
                FormatValue(diameter, suffix)
            );

            UpdateDiameterVisual();
        }


        // ============================================================
        // ANGLE
        // ============================================================

        public void ShowAngle(
            Vector3 a,
            Vector3 vertex,
            Vector3 c,
            float angle)
        {
            pointA = a;
            pointB = vertex;
            pointC = c;
            displayedValue = angle;

            currentMode = VisualMode.Angle;
            visible = true;

            SetDistanceActive(false);
            SetRadiusActive(false);
            SetDiameterActive(false);
            SetAngleActive(true);

            SetLabelText(
                angle.ToString("0.00") +
                "°"
            );

            UpdateAngleVisual();
        }


        // ============================================================
        // COMPATIBILITY
        // ============================================================

        public void Show(
            Vector3 a,
            Vector3 b,
            float distance,
            string suffix)
        {
            ShowDistance(
                a,
                b,
                distance,
                suffix
            );
        }


        // ============================================================
        // ACTIVE
        // ============================================================

        public void SetActive(bool value)
        {
            if (!value)
            {
                Hide();
                return;
            }

            if (currentMode == VisualMode.None)
            {
                return;
            }

            visible = true;

            ApplyModeVisibility();

            if (label != null)
            {
                label.gameObject.SetActive(true);
            }

            UpdateVisual();
        }


        // ============================================================
        // HIDE
        // ============================================================

        public void Hide()
        {
            visible = false;
            currentMode = VisualMode.None;

            SetDistanceActive(false);
            SetRadiusActive(false);
            SetDiameterActive(false);
            SetAngleActive(false);

            ClearRenderer(distanceDimensionLine);
            ClearRenderer(distanceExtensionA);
            ClearRenderer(distanceExtensionB);

            ClearRenderer(radiusLine);
            ClearRenderer(radiusExtension);

            ClearRenderer(diameterLine);
            ClearRenderer(diameterExtension);

            ClearRenderer(angleArmA);
            ClearRenderer(angleArmB);
            ClearRenderer(angleArc);

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );

            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }


        // ============================================================
        // UPDATE
        // ============================================================

        private void UpdateVisual()
        {
            switch (currentMode)
            {
                case VisualMode.Distance:
                    UpdateDistanceVisual();
                    break;

                case VisualMode.Radius:
                    UpdateRadiusVisual();
                    break;

                case VisualMode.Diameter:
                    UpdateDiameterVisual();
                    break;

                case VisualMode.Angle:
                    UpdateAngleVisual();
                    break;
            }
        }


        // ============================================================
        // DISTANCE VISUAL
        // ============================================================

        private void UpdateDistanceVisual()
        {
            Vector3 delta =
                pointB -
                pointA;

            float distance =
                delta.magnitude;

            if (distance <= 0.000001f)
            {
                HideDistanceGeometry();
                return;
            }

            Vector3 axis =
                delta /
                distance;

            Vector3 midpoint =
                (pointA + pointB) *
                0.5f;

            Vector3 offsetDirection =
                GetStablePerpendicular(
                    axis,
                    midpoint
                );

            Vector3 dimensionA =
                pointA +
                offsetDirection *
                dimensionOffset;

            Vector3 dimensionB =
                pointB +
                offsetDirection *
                dimensionOffset;

            if (extensionBackOffset != 0.0f)
            {
                Vector3 extensionOffset =
                    -axis *
                    extensionBackOffset;

                dimensionA += extensionOffset;
                dimensionB += extensionOffset;
            }

            SetLine(
                distanceDimensionLine,
                dimensionA,
                dimensionB
            );

            SetLine(
                distanceExtensionA,
                pointA,
                dimensionA
            );

            SetLine(
                distanceExtensionB,
                pointB,
                dimensionB
            );

            SetArrow(
                arrowA,
                dimensionA,
                axis
            );

            SetArrow(
                arrowB,
                dimensionB,
                -axis
            );

            UpdateLabelPosition(
                (dimensionA + dimensionB) *
                0.5f,
                distanceLabelPixelOffset
            );
        }


        // ============================================================
        // RADIUS VISUAL
        // ============================================================

        private void UpdateRadiusVisual()
        {
            Vector3 center =
                pointA;

            Vector3 rim =
                pointB;

            Vector3 delta =
                rim -
                center;

            float radius =
                delta.magnitude;

            if (radius <= 0.000001f)
            {
                HideRadiusGeometry();
                return;
            }

            Vector3 direction =
                delta /
                radius;

            SetLine(
                radiusLine,
                center,
                rim
            );

            Vector3 extensionEnd =
                rim +
                direction *
                radiusExtensionLength;

            SetLine(
                radiusExtension,
                rim,
                extensionEnd
            );

            SetArrow(
                arrowB,
                rim,
                -direction
            );

            SetArrowActive(
                arrowA,
                false
            );

            Vector3 labelPosition =
                center +
                direction *
                (
                    radius *
                    0.5f
                );

            Vector3 cameraDirection =
                GetCameraDirection(
                    labelPosition
                );

            Vector3 side =
                Vector3.Cross(
                    cameraDirection,
                    direction
                );

            if (side.sqrMagnitude >
                0.000001f)
            {
                side.Normalize();

                labelPosition +=
                    side *
                    radiusLabelOffset;
            }

            UpdateLabelPosition(
                labelPosition,
                0.0f
            );
        }


        // ============================================================
        // DIAMETER VISUAL
        // ============================================================

        private void UpdateDiameterVisual()
        {
            Vector3 a =
                pointA;

            Vector3 b =
                pointB;

            Vector3 delta =
                b -
                a;

            float diameter =
                delta.magnitude;

            if (diameter <= 0.000001f)
            {
                HideDiameterGeometry();
                return;
            }

            Vector3 axis =
                delta /
                diameter;

            Vector3 midpoint =
                (a + b) *
                0.5f;

            SetLine(
                diameterLine,
                a,
                b
            );

            Vector3 extensionA =
                a -
                axis *
                diameterExtensionLength;

            Vector3 extensionB =
                b +
                axis *
                diameterExtensionLength;

            SetLine(
                diameterExtension,
                extensionA,
                extensionB
            );

            SetArrow(
                arrowA,
                a,
                axis
            );

            SetArrow(
                arrowB,
                b,
                -axis
            );

            UpdateLabelPosition(
                midpoint,
                diameterLabelPixelOffset
            );
        }


        // ============================================================
        // ANGLE VISUAL
        // ============================================================

        private void UpdateAngleVisual()
        {
            Vector3 fromVertex =
                pointA -
                pointB;

            Vector3 toVertex =
                pointC -
                pointB;

            float lengthA =
                fromVertex.magnitude;

            float lengthC =
                toVertex.magnitude;

            if (lengthA <= 0.000001f ||
                lengthC <= 0.000001f)
            {
                HideAngleGeometry();
                return;
            }

            Vector3 directionA =
                fromVertex /
                lengthA;

            Vector3 directionC =
                toVertex /
                lengthC;

            float angle =
                Vector3.Angle(
                    directionA,
                    directionC
                );

            if (angle <= 0.001f)
            {
                HideAngleGeometry();
                return;
            }

            Vector3 normal =
                Vector3.Cross(
                    directionA,
                    directionC
                );

            if (normal.sqrMagnitude <=
                0.000001f)
            {
                normal =
                    GetStableAngleNormal(
                        directionA
                    );
            }

            normal.Normalize();

            Vector3 cameraToVertex =
                pointB -
                viewerCamera.transform.position;

            if (Vector3.Dot(
                    normal,
                    cameraToVertex) < 0.0f)
            {
                normal = -normal;
            }

            Vector3 armEndA =
                pointB +
                directionA *
                angleArmLength;

            Vector3 armEndC =
                pointB +
                directionC *
                angleArmLength;

            SetLine(
                angleArmA,
                pointB,
                armEndA
            );

            SetLine(
                angleArmB,
                pointB,
                armEndC
            );

            SetAngleArc(
                pointB,
                directionA,
                directionC,
                normal,
                angle
            );

            Vector3 bisector =
                directionA +
                directionC;

            if (bisector.sqrMagnitude <=
                0.000001f)
            {
                bisector =
                    Vector3.Cross(
                        normal,
                        directionA
                    );
            }

            bisector.Normalize();

            Vector3 labelPosition =
                pointB +
                bisector *
                (
                    angleArcRadius +
                    angleLabelOffset
                );

            UpdateLabelPosition(
                labelPosition,
                0.0f
            );

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );
        }


        // ============================================================
        // ANGLE ARC
        // ============================================================

        private void SetAngleArc(
            Vector3 center,
            Vector3 startDirection,
            Vector3 endDirection,
            Vector3 normal,
            float angle)
        {
            if (angleArc == null)
            {
                return;
            }

            int segments =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        angleArcSegments *
                        Mathf.Clamp01(
                            angle /
                            180.0f
                        )
                    ),
                    2,
                    128
                );

            angleArc.positionCount =
                segments + 1;

            float step =
                angle /
                segments;

            Quaternion rotation =
                Quaternion.AngleAxis(
                    step,
                    normal
                );

            Vector3 direction =
                startDirection;

            for (int i = 0;
                 i <= segments;
                 i++)
            {
                angleArc.SetPosition(
                    i,
                    center +
                    direction *
                    angleArcRadius
                );

                direction =
                    rotation *
                    direction;
            }

            angleArc.SetPosition(
                segments,
                center +
                endDirection *
                angleArcRadius
            );

            angleArc.enabled = true;
        }


        // ============================================================
        // STABLE PERPENDICULAR
        // ============================================================

        private Vector3 GetStablePerpendicular(
            Vector3 axis,
            Vector3 referencePosition)
        {
            Vector3 cameraDirection =
                GetCameraDirection(
                    referencePosition
                );

            Vector3 result =
                Vector3.ProjectOnPlane(
                    cameraDirection,
                    axis
                );

            if (result.sqrMagnitude <=
                0.000001f)
            {
                result =
                    Vector3.ProjectOnPlane(
                        viewerCamera.transform.up,
                        axis
                    );
            }

            if (result.sqrMagnitude <=
                0.000001f)
            {
                result =
                    Vector3.ProjectOnPlane(
                        viewerCamera.transform.right,
                        axis
                    );
            }

            if (result.sqrMagnitude <=
                0.000001f)
            {
                result =
                    Vector3.Cross(
                        axis,
                        Vector3.up
                    );
            }

            if (result.sqrMagnitude <=
                0.000001f)
            {
                result =
                    Vector3.Cross(
                        axis,
                        Vector3.right
                    );
            }

            result.Normalize();

            return result;
        }


        // ============================================================
        // ANGLE NORMAL
        // ============================================================

        private Vector3 GetStableAngleNormal(
            Vector3 direction)
        {
            Vector3 normal =
                Vector3.ProjectOnPlane(
                    viewerCamera.transform.forward,
                    direction
                );

            if (normal.sqrMagnitude <=
                0.000001f)
            {
                normal =
                    Vector3.ProjectOnPlane(
                        viewerCamera.transform.up,
                        direction
                    );
            }

            if (normal.sqrMagnitude <=
                0.000001f)
            {
                normal =
                    Vector3.ProjectOnPlane(
                        viewerCamera.transform.right,
                        direction
                    );
            }

            if (normal.sqrMagnitude <=
                0.000001f)
            {
                normal =
                    Vector3.Cross(
                        direction,
                        Vector3.up
                    );
            }

            if (normal.sqrMagnitude <=
                0.000001f)
            {
                normal =
                    Vector3.Cross(
                        direction,
                        Vector3.right
                    );
            }

            return normal.normalized;
        }


        // ============================================================
        // ARROW
        // ============================================================

        private void SetArrow(
            Transform arrow,
            Vector3 position,
            Vector3 direction)
        {
            if (arrow == null)
            {
                return;
            }

            if (direction.sqrMagnitude <=
                0.000001f)
            {
                return;
            }

            direction.Normalize();

            arrow.gameObject.SetActive(true);

            arrow.position = position;

            Vector3 up =
                Vector3.ProjectOnPlane(
                    viewerCamera.transform.up,
                    direction
                );

            if (up.sqrMagnitude <=
                0.000001f)
            {
                up =
                    Vector3.ProjectOnPlane(
                        viewerCamera.transform.right,
                        direction
                    );
            }

            if (up.sqrMagnitude <=
                0.000001f)
            {
                up = Vector3.up;
            }

            up.Normalize();

            arrow.rotation =
                Quaternion.LookRotation(
                    direction,
                    up
                );
        }


        // ============================================================
        // LABEL
        // ============================================================

        private void SetLabelText(
            string text)
        {
            if (valueText != null)
            {
                valueText.text = text;
            }
        }


        private void UpdateLabelPosition(
            Vector3 worldPosition,
            float pixelOffset)
        {
            if (label == null ||
                viewerCamera == null)
            {
                return;
            }

            Vector3 screenPosition =
                viewerCamera.WorldToScreenPoint(
                    worldPosition
                );

            if (screenPosition.z <= 0.0f)
            {
                label.gameObject.SetActive(false);
                return;
            }

            Canvas canvas =
                label.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                return;
            }

            RectTransform canvasRect =
                canvas.GetComponent<RectTransform>();

            if (canvasRect == null)
            {
                return;
            }

            Camera canvasCamera = null;

            if (canvas.renderMode !=
                RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera =
                    canvas.worldCamera != null
                        ? canvas.worldCamera
                        : viewerCamera;
            }

            if (!RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPosition))
            {
                return;
            }

            label.gameObject.SetActive(true);

            label.anchoredPosition =
                localPosition +
                new Vector2(
                    0.0f,
                    pixelOffset
                );
        }


        // ============================================================
        // CAMERA
        // ============================================================

        private Vector3 GetCameraDirection(
            Vector3 worldPosition)
        {
            Vector3 direction =
                viewerCamera.transform.position -
                worldPosition;

            if (direction.sqrMagnitude <=
                0.000001f)
            {
                direction =
                    -viewerCamera.transform.forward;
            }

            return direction.normalized;
        }


        // ============================================================
        // MODE VISIBILITY
        // ============================================================

        private void ApplyModeVisibility()
        {
            SetDistanceActive(false);
            SetRadiusActive(false);
            SetDiameterActive(false);
            SetAngleActive(false);

            switch (currentMode)
            {
                case VisualMode.Distance:
                    SetDistanceActive(true);
                    break;

                case VisualMode.Radius:
                    SetRadiusActive(true);
                    break;

                case VisualMode.Diameter:
                    SetDiameterActive(true);
                    break;

                case VisualMode.Angle:
                    SetAngleActive(true);
                    break;
            }
        }


        // ============================================================
        // DISTANCE ACTIVE
        // ============================================================

        private void SetDistanceActive(
            bool value)
        {
            SetRendererActive(
                distanceDimensionLine,
                value
            );

            SetRendererActive(
                distanceExtensionA,
                value
            );

            SetRendererActive(
                distanceExtensionB,
                value
            );

            SetArrowActive(
                arrowA,
                value
            );

            SetArrowActive(
                arrowB,
                value
            );
        }


        // ============================================================
        // RADIUS ACTIVE
        // ============================================================

        private void SetRadiusActive(
            bool value)
        {
            SetRendererActive(
                radiusLine,
                value
            );

            SetRendererActive(
                radiusExtension,
                value
            );

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                value
            );
        }


        // ============================================================
        // DIAMETER ACTIVE
        // ============================================================

        private void SetDiameterActive(
            bool value)
        {
            SetRendererActive(
                diameterLine,
                value
            );

            SetRendererActive(
                diameterExtension,
                value
            );

            SetArrowActive(
                arrowA,
                value
            );

            SetArrowActive(
                arrowB,
                value
            );
        }


        // ============================================================
        // ANGLE ACTIVE
        // ============================================================

        private void SetAngleActive(
            bool value)
        {
            SetRendererActive(
                angleArmA,
                value
            );

            SetRendererActive(
                angleArmB,
                value
            );

            SetRendererActive(
                angleArc,
                value
            );

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );
        }


        // ============================================================
        // GEOMETRY HIDE
        // ============================================================

        private void HideDistanceGeometry()
        {
            ClearRenderer(distanceDimensionLine);
            ClearRenderer(distanceExtensionA);
            ClearRenderer(distanceExtensionB);

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );

            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }


        private void HideRadiusGeometry()
        {
            ClearRenderer(radiusLine);
            ClearRenderer(radiusExtension);

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );

            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }


        private void HideDiameterGeometry()
        {
            ClearRenderer(diameterLine);
            ClearRenderer(diameterExtension);

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );

            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }


        private void HideAngleGeometry()
        {
            ClearRenderer(angleArmA);
            ClearRenderer(angleArmB);
            ClearRenderer(angleArc);

            SetArrowActive(
                arrowA,
                false
            );

            SetArrowActive(
                arrowB,
                false
            );

            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }


        // ============================================================
        // RENDERER HELPERS
        // ============================================================

        private static void SetLine(
            LineRenderer line,
            Vector3 start,
            Vector3 end)
        {
            if (line == null)
            {
                return;
            }

            line.useWorldSpace = true;
            line.positionCount = 2;

            line.SetPosition(
                0,
                start
            );

            line.SetPosition(
                1,
                end
            );

            line.enabled = true;
        }


        private static void ClearRenderer(
            LineRenderer line)
        {
            if (line == null)
            {
                return;
            }

            line.positionCount = 0;
            line.enabled = false;
        }


        private static void SetRendererActive(
            LineRenderer line,
            bool value)
        {
            if (line == null)
            {
                return;
            }

            line.enabled = value;
        }


        private static void SetArrowActive(
            Transform arrow,
            bool value)
        {
            if (arrow == null)
            {
                return;
            }

            arrow.gameObject.SetActive(value);
        }


        // ============================================================
        // FORMATTING
        // ============================================================

        private static string FormatValue(
            float value,
            string suffix)
        {
            return value.ToString("0.00") +
                   suffix;
        }
    }
}