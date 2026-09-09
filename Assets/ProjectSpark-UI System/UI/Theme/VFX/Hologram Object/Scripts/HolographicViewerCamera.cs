using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicViewerCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance")]
        [SerializeField] private float defaultDistance = 4f;
        [SerializeField] private float minDistance = 2.2f;
        [SerializeField] private float maxDistance = 7f;

        [Header("Zoom")]
        [SerializeField] private float wheelZoomSpeed = 0.0025f;
        [SerializeField] private float zoomSmoothTime = 0.12f;

        [Header("Pan")]
        [SerializeField] private float panLimit = 1.5f;

        [Header("Offset")]
        [SerializeField] private Vector3 defaultOffset =
            new Vector3(0f, 0.15f, 0f);

        private float targetDistance;
        private float currentDistance;
        private float distanceVelocity;

        private Vector3 targetOffset;

        private void Awake()
        {
            targetDistance =
                Mathf.Clamp(
                    defaultDistance,
                    minDistance,
                    maxDistance
                );

            currentDistance =
                targetDistance;

            targetOffset =
                defaultOffset;
        }

        private void LateUpdate()
        {
            UpdateDistance();
            UpdatePosition();
        }

        private void UpdateDistance()
        {
            currentDistance =
                Mathf.SmoothDamp(
                    currentDistance,
                    targetDistance,
                    ref distanceVelocity,
                    zoomSmoothTime
                );
        }

        private void UpdatePosition()
        {
            if (target == null)
                return;

            Vector3 center =
                target.position + targetOffset;

            transform.position =
                center -
                transform.forward *
                currentDistance;
        }

        public void Zoom(float wheelDelta)
        {
            targetDistance -=
                wheelDelta * wheelZoomSpeed;

            targetDistance =
                Mathf.Clamp(
                    targetDistance,
                    minDistance,
                    maxDistance
                );
        }

        public void ZoomIn()
        {
            targetDistance -= 0.5f;

            targetDistance =
                Mathf.Clamp(
                    targetDistance,
                    minDistance,
                    maxDistance
                );
        }

        public void ZoomOut()
        {
            targetDistance += 0.5f;

            targetDistance =
                Mathf.Clamp(
                    targetDistance,
                    minDistance,
                    maxDistance
                );
        }

        public void Pan(Vector2 delta)
        {
            Vector3 right =
                transform.right * delta.x;

            Vector3 up =
                transform.up * delta.y;

            targetOffset +=
                right + up;

            targetOffset.x =
                Mathf.Clamp(
                    targetOffset.x,
                    -panLimit,
                    panLimit
                );

            targetOffset.y =
                Mathf.Clamp(
                    targetOffset.y,
                    defaultOffset.y - panLimit,
                    defaultOffset.y + panLimit
                );
        }

        public void ResetView()
        {
            targetDistance =
                Mathf.Clamp(
                    defaultDistance,
                    minDistance,
                    maxDistance
                );

            targetOffset =
                defaultOffset;
        }

        public float GetDistance()
        {
            return targetDistance;
        }
    }
}