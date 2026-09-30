
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual representation of the hair-dryer motor/fan.
    ///
    /// Electrical behavior belongs to SparkHairDryerMotorElectrical.
    /// This component only handles the visual fan rotation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerMotorVisual : MonoBehaviour
    {
        // ============================================================
        // FAN
        // ============================================================

        [Header("Fan")]
        [SerializeField]
        private Transform fanTransform;

        [SerializeField]
        private Vector3 localRotationAxis = Vector3.forward;

        [SerializeField, Min(0f)]
        private float idleRotationSpeed = 0f;

        [SerializeField, Min(0f)]
        private float runningRotationSpeed = 1800f;

        [SerializeField]
        private bool reverseRotation = false;

        // ============================================================
        // ELECTRICAL SOURCE
        // ============================================================

        [Header("Motor Electrical")]
        [SerializeField]
        private SparkHairDryerMotorElectrical motorElectrical;

        // ============================================================
        // STATE
        // ============================================================

        private bool isRunning;

        public bool IsRunning => isRunning;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (fanTransform == null)
                fanTransform = transform;

            if (motorElectrical == null)
                motorElectrical =
                    GetComponent<SparkHairDryerMotorElectrical>();

            localRotationAxis.Normalize();

            if (localRotationAxis.sqrMagnitude <= 0.0001f)
                localRotationAxis = Vector3.forward;
        }

        private void Update()
        {
            if (fanTransform == null)
                return;

            if (motorElectrical == null)
                return;

            bool shouldRun = motorElectrical.IsRunning;

            if (shouldRun != isRunning)
                isRunning = shouldRun;

            if (!isRunning)
            {
                if (idleRotationSpeed <= 0f)
                    return;

                RotateFan(idleRotationSpeed);
                return;
            }

            RotateFan(runningRotationSpeed);
        }

        // ============================================================
        // ROTATION
        // ============================================================

        private void RotateFan(float degreesPerSecond)
        {
            float direction = reverseRotation ? -1f : 1f;

            fanTransform.Rotate(
                localRotationAxis,
                degreesPerSecond * direction * Time.deltaTime,
                Space.Self);
        }

        // ============================================================
        // PUBLIC
        // ============================================================

        public void SetMotorElectrical(
            SparkHairDryerMotorElectrical motor)
        {
            motorElectrical = motor;
        }

        public void SetRunning(bool running)
        {
            isRunning = running;
        }
    }
}
