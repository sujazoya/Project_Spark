using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Controls rotation of the holographic object from mouse/touch drag input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicObjectController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform rotationTarget;

        [Header("Rotation")]
        [SerializeField] private float rotationSensitivity = 0.25f;
        [SerializeField] private bool invertX = false;
        [SerializeField] private bool invertY = true;
        [SerializeField] private bool useWorldSpace = false;

        [Header("Limits")]
        [SerializeField] private bool clampVerticalRotation = false;
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;

        private bool dragging;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (rotationTarget == null)
                rotationTarget = transform;

            ReadCurrentRotation();
        }

        private void ReadCurrentRotation()
        {
            Vector3 euler = rotationTarget.localEulerAngles;

            yaw = NormalizeAngle(euler.y);
            pitch = NormalizeAngle(euler.x);
        }

        public void BeginDrag()
        {
            if (rotationTarget == null)
                rotationTarget = transform;

            ReadCurrentRotation();
            dragging = true;
        }

        public void EndDrag()
        {
            dragging = false;
        }

        public void RotateFromDrag(Vector2 delta)
        {
            if (rotationTarget == null)
                rotationTarget = transform;

            float x = delta.x;
            float y = delta.y;

            if (invertX)
                x = -x;

            if (invertY)
                y = -y;

            yaw += x * rotationSensitivity;
            pitch += y * rotationSensitivity;

            if (clampVerticalRotation)
            {
                pitch = Mathf.Clamp(
                    pitch,
                    minVerticalAngle,
                    maxVerticalAngle);
            }

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            Quaternion rotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    0f);

            if (useWorldSpace)
                rotationTarget.rotation = rotation;
            else
                rotationTarget.localRotation = rotation;
        }

        public void SetRotation(Vector3 eulerAngles)
        {
            if (rotationTarget == null)
                rotationTarget = transform;

            yaw = NormalizeAngle(eulerAngles.y);
            pitch = NormalizeAngle(eulerAngles.x);

            if (clampVerticalRotation)
            {
                pitch = Mathf.Clamp(
                    pitch,
                    minVerticalAngle,
                    maxVerticalAngle);
            }

            ApplyRotation();
        }

        public void ResetRotation()
        {
            if (rotationTarget == null)
                rotationTarget = transform;

            yaw = 0f;
            pitch = 0f;

            ApplyRotation();
        }

        public bool IsDragging => dragging;

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;

            if (angle > 180f)
                angle -= 360f;

            if (angle < -180f)
                angle += 360f;

            return angle;
        }
    }
}