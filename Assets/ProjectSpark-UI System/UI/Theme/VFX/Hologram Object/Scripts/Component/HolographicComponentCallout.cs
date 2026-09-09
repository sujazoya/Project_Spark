using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentCallout
        : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewerCamera;
        [SerializeField] private RectTransform calloutRoot;
        [SerializeField] private RectTransform line;

        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text partNumberText;

        private Transform currentAnchor;

        private void LateUpdate()
        {
            if (currentAnchor == null)
            {
                if (calloutRoot != null)
                    calloutRoot.gameObject.SetActive(false);

                return;
            }

            if (viewerCamera == null ||
                calloutRoot == null)
                return;

            UpdateCallout();
        }

        public void Show(
            HolographicComponentData data,
            Transform anchor)
        {
            if (data == null ||
                anchor == null)
            {
                Hide();
                return;
            }

            currentAnchor = anchor;

            calloutRoot.gameObject.SetActive(true);

            if (nameText != null)
            {
                nameText.text =
                    data.ComponentName;
            }

            if (partNumberText != null)
            {
                partNumberText.text =
                    data.PartNumber;
            }
        }

        public void Hide()
        {
            currentAnchor = null;

            if (calloutRoot != null)
                calloutRoot.gameObject.SetActive(false);
        }

        private void UpdateCallout()
        {
            Vector3 screenPosition =
                viewerCamera.WorldToScreenPoint(
                    currentAnchor.position
                );

            if (screenPosition.z <= 0f)
            {
                calloutRoot.gameObject.SetActive(false);
                return;
            }

            calloutRoot.gameObject.SetActive(true);

            RectTransform canvasRect =
                calloutRoot.GetComponentInParent<
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

            Vector2 panelPosition =
                localPosition +
                new Vector2(140f, 60f);

            calloutRoot.anchoredPosition =
                panelPosition;

            UpdateLine(
                localPosition,
                panelPosition
            );
        }

        private void UpdateLine(
            Vector2 target,
            Vector2 panel)
        {
            if (line == null)
                return;

            Vector2 direction =
                target - panel;

            float length =
                direction.magnitude;

            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x
                ) * Mathf.Rad2Deg;

            line.sizeDelta =
                new Vector2(
                    length,
                    line.sizeDelta.y
                );

            line.anchoredPosition =
                panel;

            line.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );
        }
    }
}