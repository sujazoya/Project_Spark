using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicDimensionVisual
        : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera viewerCamera;

        [Header("World Visuals")]
        [SerializeField] private LineRenderer dimensionLine;
        [SerializeField] private LineRenderer extensionLineA;
        [SerializeField] private LineRenderer extensionLineB;

        [SerializeField] private Transform arrowA;
        [SerializeField] private Transform arrowB;

        [Header("UI")]
        [SerializeField] private RectTransform label;
        [SerializeField] private TMP_Text valueText;

        [Header("Layout")]
        [SerializeField] private float dimensionOffset = 0.18f;
        [SerializeField] private float labelOffsetPixels = 14f;

        private Vector3 pointA;
        private Vector3 pointB;

        private bool visible;

        public void Show(
            Vector3 a,
            Vector3 b,
            float distance,
            string suffix)
        {
            pointA = a;
            pointB = b;

            visible = true;

            if (label != null)
                label.gameObject.SetActive(true);

            if (valueText != null)
            {
                valueText.text =
                    distance.ToString("0.00") +
                    suffix;
            }

            UpdateVisual();
        }

        public void Hide()
        {
            visible = false;

            if (dimensionLine != null)
                dimensionLine.positionCount = 0;

            if (extensionLineA != null)
                extensionLineA.positionCount = 0;

            if (extensionLineB != null)
                extensionLineB.positionCount = 0;

            if (arrowA != null)
                arrowA.gameObject.SetActive(false);

            if (arrowB != null)
                arrowB.gameObject.SetActive(false);

            if (label != null)
                label.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!visible)
                return;

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            Vector3 direction =
                pointB - pointA;

            float length =
                direction.magnitude;

            if (length < 0.0001f)
                return;

            Vector3 axis =
                direction.normalized;

            Vector3 cameraDirection =
                (viewerCamera.transform.position -
                 (pointA + pointB) * 0.5f).normalized;

            Vector3 offsetDirection =
                Vector3.Cross(
                    axis,
                    cameraDirection
                );

            if (offsetDirection.sqrMagnitude < 0.001f)
            {
                offsetDirection =
                    viewerCamera.transform.up;
            }

            offsetDirection.Normalize();

            Vector3 dimensionA =
                pointA +
                offsetDirection *
                dimensionOffset;

            Vector3 dimensionB =
                pointB +
                offsetDirection *
                dimensionOffset;

            UpdateDimensionLine(
                dimensionA,
                dimensionB
            );

            UpdateExtensionLines(
                pointA,
                dimensionA,
                pointB,
                dimensionB
            );

            UpdateArrows(
                dimensionA,
                dimensionB,
                axis
            );

            UpdateLabel(
                dimensionA,
                dimensionB
            );
        }

        private void UpdateDimensionLine(
            Vector3 a,
            Vector3 b)
        {
            if (dimensionLine == null)
                return;

            dimensionLine.positionCount = 2;

            dimensionLine.SetPosition(0, a);
            dimensionLine.SetPosition(1, b);
        }

        private void UpdateExtensionLines(
            Vector3 pointA,
            Vector3 dimensionA,
            Vector3 pointB,
            Vector3 dimensionB)
        {
            if (extensionLineA != null)
            {
                extensionLineA.positionCount = 2;

                extensionLineA.SetPosition(
                    0,
                    pointA
                );

                extensionLineA.SetPosition(
                    1,
                    dimensionA
                );
            }

            if (extensionLineB != null)
            {
                extensionLineB.positionCount = 2;

                extensionLineB.SetPosition(
                    0,
                    pointB
                );

                extensionLineB.SetPosition(
                    1,
                    dimensionB
                );
            }
        }

        private void UpdateArrows(
            Vector3 a,
            Vector3 b,
            Vector3 axis)
        {
            if (arrowA != null)
            {
                arrowA.gameObject.SetActive(true);

                arrowA.position = a;

                arrowA.rotation =
                    Quaternion.LookRotation(
                        axis
                    );
            }

            if (arrowB != null)
            {
                arrowB.gameObject.SetActive(true);

                arrowB.position = b;

                arrowB.rotation =
                    Quaternion.LookRotation(
                        -axis
                    );
            }
        }

        private void UpdateLabel(
            Vector3 a,
            Vector3 b)
        {
            if (label == null ||
                viewerCamera == null)
                return;

            Vector3 midpoint =
                (a + b) * 0.5f;

            Vector3 screenPosition =
                viewerCamera.WorldToScreenPoint(
                    midpoint
                );

            if (screenPosition.z <= 0f)
            {
                label.gameObject.SetActive(false);
                return;
            }

            label.gameObject.SetActive(true);

            RectTransform canvasRect =
                label.GetComponentInParent<
                    Canvas>()?
                    .GetComponent<RectTransform>();

            if (canvasRect == null)
                return;

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null,
                    out Vector2 localPosition
                );

            label.anchoredPosition =
                localPosition +
                new Vector2(
                    0f,
                    labelOffsetPixels
                );
        }
    }
}