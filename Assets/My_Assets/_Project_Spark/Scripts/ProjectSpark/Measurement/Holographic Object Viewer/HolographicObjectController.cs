using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicObjectController : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float dragSensitivity = 0.25f;
        [SerializeField] private float autoRotateSpeed = 12f;
        [SerializeField] private float smoothTime = 0.08f;

        [Header("Limits")]
        [SerializeField] private float pitchMin = -80f;
        [SerializeField] private float pitchMax = 80f;

        private Quaternion initialRotation;

        private float targetYaw;
        private float targetPitch;

        private float yawVelocity;
        private float pitchVelocity;

        private bool isDragging;
        private bool autoRotate;

        public bool AutoRotate => autoRotate;

        private void Awake()
        {
            initialRotation = transform.localRotation;

            Vector3 euler = transform.localEulerAngles;

            targetYaw = NormalizeAngle(euler.y);
            targetPitch = NormalizeAngle(euler.x);
        }

        private void Update()
        {
            if (autoRotate && !isDragging)
            {
                targetYaw += autoRotateSpeed * Time.deltaTime;
            }

            float yaw = Mathf.SmoothDampAngle(
                transform.localEulerAngles.y,
                targetYaw,
                ref yawVelocity,
                smoothTime
            );

            float pitch = Mathf.SmoothDampAngle(
                transform.localEulerAngles.x,
                targetPitch,
                ref pitchVelocity,
                smoothTime
            );

            transform.localRotation =
                Quaternion.Euler(pitch, yaw, transform.localEulerAngles.z);
        }

        public void BeginDrag()
        {
            isDragging = true;
            autoRotate = false;
        }

        public void EndDrag()
        {
            isDragging = false;
        }

        public void RotateFromDrag(Vector2 delta)
        {
            targetYaw += delta.x * dragSensitivity;
            targetPitch -= delta.y * dragSensitivity;

            targetPitch = Mathf.Clamp(
                targetPitch,
                pitchMin,
                pitchMax
            );
        }

        public void ToggleAutoRotate()
        {
            autoRotate = !autoRotate;
        }

        public void SetAutoRotate(bool value)
        {
            autoRotate = value;
        }

        public void ResetView()
        {
            autoRotate = false;
            isDragging = false;

            transform.localRotation = initialRotation;

            Vector3 euler = initialRotation.eulerAngles;

            targetYaw = NormalizeAngle(euler.y);
            targetPitch = NormalizeAngle(euler.x);

            yawVelocity = 0f;
            pitchVelocity = 0f;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;

            if (angle > 180f)
                angle -= 360f;

            return angle;
        }
    }
}