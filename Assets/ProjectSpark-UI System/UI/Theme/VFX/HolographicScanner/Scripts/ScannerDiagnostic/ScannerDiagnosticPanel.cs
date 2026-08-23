using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerDiagnosticPanel
        : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RectTransform panelRoot;

        [Header("Header")]
        [SerializeField]
        private TMP_Text headerText;

        [Header("Identity")]
        [SerializeField]
        private TMP_Text componentIdText;

        [SerializeField]
        private TMP_Text componentTypeText;

        [Header("Values")]
        [SerializeField]
        private TMP_Text row01Label;

        [SerializeField]
        private TMP_Text row01Value;

        [SerializeField]
        private TMP_Text row02Label;

        [SerializeField]
        private TMP_Text row02Value;

        [SerializeField]
        private TMP_Text row03Label;

        [SerializeField]
        private TMP_Text row03Value;

        [SerializeField]
        private TMP_Text row04Label;

        [SerializeField]
        private TMP_Text row04Value;

        [Header("Status")]
        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private Image statusIndicator;

        [Header("Placement")]
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 worldOffset =
            new Vector3(0.18f, 0.12f, 0f);

        [SerializeField, Min(0f)]
        private float followSpeed = 12f;

        private Camera targetCamera;

        public bool IsVisible =>
            canvasGroup != null &&
            canvasGroup.alpha > 0.001f;

        private void Awake()
        {
            targetCamera =
                Camera.main;

            HideImmediate();
        }

        private void LateUpdate()
        {
            UpdatePlacement();
        }

        public void Show(
            ScannerDiagnosticData data,
            Transform targetTransform)
        {
            target = targetTransform;

            ApplyData(data);

            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 1f;

            if (panelRoot != null)
                panelRoot.localScale =
                    Vector3.one;
        }

        public void Hide()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        public void HideImmediate()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        private void ApplyData(
            ScannerDiagnosticData data)
        {
            if (headerText != null)
            {
                headerText.text =
                    "COMPONENT IDENTIFIED";
            }

            if (componentIdText != null)
            {
                componentIdText.text =
                    data.componentId;
            }

            if (componentTypeText != null)
            {
                componentTypeText.text =
                    data.componentType;
            }

            SetRow(
                row01Label,
                row01Value,
                data.primaryValueLabel,
                data.primaryValue);

            SetRow(
                row02Label,
                row02Value,
                data.secondaryValueLabel,
                data.secondaryValue);

            SetRow(
                row03Label,
                row03Value,
                data.tertiaryValueLabel,
                data.tertiaryValue);

            SetRow(
                row04Label,
                row04Value,
                data.quaternaryValueLabel,
                data.quaternaryValue);

            if (statusText != null)
            {
                statusText.text =
                    data.status;
            }

            if (statusIndicator != null)
            {
                statusIndicator.enabled =
                    true;
            }
        }

        private static void SetRow(
            TMP_Text label,
            TMP_Text value,
            string labelText,
            string valueText)
        {
            if (label != null)
                label.text = labelText;

            if (value != null)
                value.text = valueText;
        }

        private void UpdatePlacement()
{
    if (target == null)
        return;

    if (targetCamera == null)
        targetCamera = Camera.main;

    if (targetCamera == null)
        return;

    // ---------------------------------------------------------
    // Position
    // ---------------------------------------------------------

    Vector3 desiredPosition =
        target.TransformPoint(worldOffset);

    if (followSpeed <= 0f)
    {
        transform.position =
            desiredPosition;
    }
    else
    {
        float blend =
            1f -
            Mathf.Exp(
                -followSpeed *
                Time.deltaTime);

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                blend);
    }

    // ---------------------------------------------------------
    // Rotation
    //
    // World-space UI should face the camera directly.
    // ---------------------------------------------------------

    transform.rotation =
        targetCamera.transform.rotation;
}
    }
}