using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual controller for the hair-dryer fan.
    ///
    /// Handles:
    /// - Smooth fan acceleration
    /// - Smooth fan deceleration
    /// - Low/high speed
    /// - Rotor rotation
    ///
    /// Electrical behavior is intentionally handled elsewhere.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerFanVisual : MonoBehaviour
    {
        // ============================================================
        // INSPECTOR
        // ============================================================

        [Header("Rotor")]
        [SerializeField]
        private Transform rotor;

        [SerializeField]
        private Vector3 rotationAxis = Vector3.forward;

        [Header("Speed")]
        [SerializeField]
        private float lowRPM = 3500f;

        [SerializeField]
        private float highRPM = 6500f;

        [SerializeField]
        private float acceleration = 9000f;

        [SerializeField]
        private float deceleration = 12000f;

        [Header("Runtime")]
        [SerializeField]
        private float currentRPM;

        [SerializeField]
        private float targetRPM;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public float CurrentRPM =>
            currentRPM;

        public float TargetRPM =>
            targetRPM;

        public bool IsRunning =>
            currentRPM > 1f;

        // ============================================================
        // UNITY
        // ============================================================

        private void Update()
        {
            UpdateSpeed();
            RotateRotor();
        }

        // ============================================================
        // SPEED
        // ============================================================

        private void UpdateSpeed()
        {
            float changeSpeed =
                targetRPM > currentRPM
                    ? acceleration
                    : deceleration;

            currentRPM = Mathf.MoveTowards(
                currentRPM,
                targetRPM,
                changeSpeed * Time.deltaTime);
        }

        // ============================================================
        // ROTATION
        // ============================================================

        private void RotateRotor()
        {
            if (rotor == null)
                return;

            if (currentRPM <= 0f)
                return;

            float degreesPerSecond =
                currentRPM * 6f;

            rotor.Rotate(
                rotationAxis.normalized,
                degreesPerSecond * Time.deltaTime,
                Space.Self);
        }

        // ============================================================
        // CONTROL
        // ============================================================

        public void Stop()
        {
            targetRPM = 0f;
        }

        public void SetLowSpeed()
        {
            targetRPM = lowRPM;
        }

        public void SetHighSpeed()
        {
            targetRPM = highRPM;
        }

        public void SetSpeed(float rpm)
        {
            targetRPM = Mathf.Max(0f, rpm);
        }

        public void SetRunning(bool running)
        {
            targetRPM = running
                ? highRPM
                : 0f;
        }

        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("Test Stop")]
        private void TestStop()
        {
            Stop();
        }

        [ContextMenu("Test Low Speed")]
        private void TestLow()
        {
            SetLowSpeed();
        }

        [ContextMenu("Test High Speed")]
        private void TestHigh()
        {
            SetHighSpeed();
        }
    }
}