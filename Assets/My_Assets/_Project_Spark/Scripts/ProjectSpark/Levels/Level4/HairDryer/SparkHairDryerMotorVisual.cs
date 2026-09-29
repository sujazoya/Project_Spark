using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual motor controller for the hair dryer.
    ///
    /// Responsibilities:
    /// - Smooth motor startup.
    /// - Smooth motor shutdown.
    /// - Rotor rotation.
    /// - RPM control.
    /// - Provides runtime motor state.
    ///
    /// Visual only.
    /// Actual motor physics/electrical behavior comes later.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerMotorVisual : MonoBehaviour
    {
        [Header("Rotor")]
        [SerializeField]
        private Transform rotor;

        [SerializeField]
        private Vector3 rotationAxis = Vector3.forward;

        [Header("Motor Speed")]
        [SerializeField]
        [Min(0f)]
        private float maximumRPM = 9000f;

        [SerializeField]
        [Min(0f)]
        private float accelerationRPMPerSecond = 18000f;

        [SerializeField]
        [Min(0f)]
        private float decelerationRPMPerSecond = 24000f;

        [Header("Runtime")]
        [SerializeField]
        private float currentRPM;

        [SerializeField]
        private float targetRPM;

        public float CurrentRPM => currentRPM;

        public float TargetRPM => targetRPM;

        public bool IsRunning =>
            currentRPM > 10f;

        public bool IsAtSpeed =>
            Mathf.Abs(currentRPM - targetRPM) < 10f;

        private void Awake()
        {
            rotationAxis =
                rotationAxis.sqrMagnitude > 0.0001f
                    ? rotationAxis.normalized
                    : Vector3.forward;
        }

        private void Update()
        {
            UpdateRPM();
            RotateMotor();
        }

        private void UpdateRPM()
        {
            float speed =
                targetRPM > currentRPM
                    ? accelerationRPMPerSecond
                    : decelerationRPMPerSecond;

            currentRPM =
                Mathf.MoveTowards(
                    currentRPM,
                    targetRPM,
                    speed * Time.deltaTime);
        }

        private void RotateMotor()
        {
            if (rotor == null)
                return;

            if (currentRPM <= 0.01f)
                return;

            float degreesPerSecond =
                currentRPM * 6f;

            rotor.Rotate(
                rotationAxis,
                degreesPerSecond * Time.deltaTime,
                Space.Self);
        }

        public void Stop()
        {
            targetRPM = 0f;
        }

        public void SetLowSpeed()
        {
            SetRPM(maximumRPM * 0.55f);
        }

        public void SetHighSpeed()
        {
            SetRPM(maximumRPM);
        }

        public void SetRPM(float rpm)
        {
            targetRPM =
                Mathf.Clamp(
                    rpm,
                    0f,
                    maximumRPM);
        }

        public void SetRPMImmediate(float rpm)
        {
            currentRPM =
                Mathf.Clamp(
                    rpm,
                    0f,
                    maximumRPM);

            targetRPM = currentRPM;
        }

        [ContextMenu("Test / Motor OFF")]
        private void TestOff()
        {
            Stop();
        }

        [ContextMenu("Test / Motor LOW")]
        private void TestLow()
        {
            SetLowSpeed();
        }

        [ContextMenu("Test / Motor HIGH")]
        private void TestHigh()
        {
            SetHighSpeed();
        }

        [ContextMenu("Test / Motor Immediate Stop")]
        private void TestImmediateStop()
        {
            SetRPMImmediate(0f);
        }
    }
}