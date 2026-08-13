using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerViewportController : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler,
        IScrollHandler
    {
        [SerializeField] private Transform target;
        [SerializeField] private float orbitSpeed = 0.2f;
        [SerializeField] private float zoomSpeed = 1.5f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 14f;
        [SerializeField] private Camera targetCamera;

        private Vector2 lastPointer;
        private bool dragging;

        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;
            lastPointer = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || target == null)
                return;

            Vector2 delta = eventData.position - lastPointer;
            lastPointer = eventData.position;

            target.Rotate(Vector3.up, delta.x * orbitSpeed, Space.World);
            target.Rotate(Vector3.right, -delta.y * orbitSpeed, Space.Self);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (targetCamera == null)
                return;

            Vector3 offset = targetCamera.transform.position - target.position;
            float distance = Mathf.Clamp(offset.magnitude - eventData.scrollDelta.y * zoomSpeed, minDistance, maxDistance);

            targetCamera.transform.position =
                target.position + offset.normalized * distance;
        }
    }
}
