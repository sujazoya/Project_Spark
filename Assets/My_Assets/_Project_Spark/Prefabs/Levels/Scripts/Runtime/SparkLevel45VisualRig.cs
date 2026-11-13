using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Optional art-only animation for the LEVEL 45 FBX pack.
    /// Supply actual measured motor RPM and load from your authoritative gameplay controller.
    /// This never changes electrical topology, current, PWM, motor physics, or objective completion.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel45VisualRig : MonoBehaviour
    {
        [Header("Actual FBX rotating groups (X axis)")]
        [SerializeField] private Transform motorShaft;
        [SerializeField] private Transform encoderFlywheel;
        [SerializeField] private Transform driveAxle;
        [SerializeField] private Transform brakeRotor;

        [Header("Optional moving brake groups")]
        [SerializeField] private Transform brakeLever;
        [SerializeField] private Transform brakePad;
        [SerializeField, Range(0f, 45f)] private float maxBrakeLeverAngle = 25f;
        [SerializeField, Min(0f)] private float maxBrakePadTravel = .12f;

        [Header("Visual smoothing only")]
        [SerializeField, Min(1f)] private float rpmResponsePerSecond = 150f;
        [SerializeField, Min(.01f)] private float brakeResponse = 9f;
        [SerializeField] private bool animateInPlayMode = true;
        [SerializeField] private bool showDebugValues;

        [Header("Runtime display (not an electrical reading)")]
        [SerializeField] private float targetMeasuredRpm;
        [SerializeField, Range(0f, 1f)] private float requestedBrakeLoad;

        private float displayedRpm;
        private float displayedBrake;
        private Quaternion leverOrigin;
        private Vector3 padOrigin;

        private void Awake()
        {
            if (brakeLever != null) leverOrigin = brakeLever.localRotation;
            if (brakePad != null) padOrigin = brakePad.localPosition;
        }

        /// <summary>Called by the REAL encoder/observer after producing a measured RPM.</summary>
        public void SetMeasuredRpm(float measuredRpm)
        {
            if (float.IsNaN(measuredRpm) || float.IsInfinity(measuredRpm)) return;
            targetMeasuredRpm = Mathf.Clamp(measuredRpm, -10000f, 10000f);
        }

        /// <summary>Called by the REAL brake controller, 0 = free, 1 = full test load.</summary>
        public void SetBrakeLoad(float load01)
        {
            if (float.IsNaN(load01) || float.IsInfinity(load01)) return;
            requestedBrakeLoad = Mathf.Clamp01(load01);
        }

        public void StopVisualMotion() => SetMeasuredRpm(0f);

        private void Update()
        {
            if (!animateInPlayMode) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            displayedRpm = Mathf.MoveTowards(displayedRpm, targetMeasuredRpm,
                rpmResponsePerSecond * dt);

            // RPM -> degrees per second: 360 / 60 = 6.
            float deltaDegrees = displayedRpm * 6f * dt;
            RotateX(motorShaft, deltaDegrees);
            RotateX(encoderFlywheel, deltaDegrees);
            RotateX(driveAxle, deltaDegrees);
            RotateX(brakeRotor, deltaDegrees);

            displayedBrake = Mathf.MoveTowards(displayedBrake, requestedBrakeLoad,
                brakeResponse * dt);
            if (brakeLever != null)
                brakeLever.localRotation = leverOrigin *
                    Quaternion.Euler(0f, displayedBrake * maxBrakeLeverAngle, 0f);
            if (brakePad != null)
                brakePad.localPosition = padOrigin +
                    Vector3.forward * (displayedBrake * maxBrakePadTravel);
        }

        private static void RotateX(Transform part, float degrees)
        {
            if (part != null)
                part.Rotate(Vector3.right, degrees, Space.Self);
        }

        [ContextMenu("Visual Rig / Stop")]
        private void InspectorStop() => StopVisualMotion();

        [ContextMenu("Visual Rig / Preview 30 RPM")]
        private void Inspector30() => SetMeasuredRpm(30f);

        [ContextMenu("Visual Rig / Preview 60 RPM")]
        private void Inspector60() => SetMeasuredRpm(60f);

        [ContextMenu("Visual Rig / Preview Brake")]
        private void InspectorBrake() => SetBrakeLoad(1f);

        [ContextMenu("Visual Rig / Release Brake")]
        private void InspectorRelease() => SetBrakeLoad(0f);
    }
}
