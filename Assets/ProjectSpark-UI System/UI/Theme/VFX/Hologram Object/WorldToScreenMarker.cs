using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class WorldToScreenMarker : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform target;
        [SerializeField] private RectTransform marker;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

        private void LateUpdate()
        {
            if (targetCamera == null ||
                target == null ||
                marker == null)
            {
                return;
            }

            Vector3 worldPosition =
                target.position + worldOffset;

            Vector3 screenPosition =
                targetCamera.WorldToScreenPoint(worldPosition);

            if (screenPosition.z <= 0f)
            {
                marker.gameObject.SetActive(false);
                return;
            }

            marker.gameObject.SetActive(true);

            RectTransform canvasRect =
                marker.GetComponentInParent<Canvas>()?
                    .GetComponent<RectTransform>();

            if (canvasRect == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out Vector2 localPosition
            );

            marker.anchoredPosition = localPosition;
        }
    }
}