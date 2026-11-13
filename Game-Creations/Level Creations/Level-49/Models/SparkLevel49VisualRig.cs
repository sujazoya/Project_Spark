using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 49 visual-only adapter for imported FBX groups. Never changes circuit state,
    /// safety evaluation, cutter physics, motor commands, or level objectives.
    /// Call ApplyState from your authoritative Level 49 controller after it updates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel49VisualRig : MonoBehaviour
    {
        [Header("Imported named mesh-group pivots (from Animation_Rigs.csv)")]
        [SerializeField] private Transform carriageSlideX;
        [SerializeField] private Transform cutterVerticalStrokeY;
        [SerializeField] private Transform carriageMotorShaftRotateX;
        [SerializeField] private Transform measuringRollerRotateZ;
        [SerializeField] private Transform encoderDiscRotateZ;
        [SerializeField] private Transform leadScrewRotateX;
        [SerializeField] private Transform safetyDoorHinge;
        [SerializeField] private GameObject opticalScanBeam;
        [SerializeField] private GameObject cutterReadyLamp;

        [Header("Visual dimensions: calibrate to the actual FBX and simulator")]
        [SerializeField, Min(0.0001f)] private float carriageUnitsPerMillimeter = 0.045f;
        [SerializeField, Min(0.0001f)] private float webPatternUnitsPerMillimeter = 0.012f;
        [SerializeField, Min(0.001f)] private float rollerRadiusMillimeters = 20f;
        [SerializeField, Min(0.001f)] private float cutterStrokeUnits = 0.24f;
        [SerializeField, Range(0f, 135f)] private float safetyDoorOpenAngle = 100f;
        [SerializeField, Min(0.1f)] private float carriageVisualResponsiveness = 15f;
        [SerializeField, Min(0.1f)] private float actuatorVisualResponsiveness = 18f;

        [Header("Optional moving web art group")]
        [Tooltip("Use a marks-only graphic here if possible; translating the whole web will expose its ends.")]
        [SerializeField] private Transform scrollingWebPattern;
        [SerializeField, Min(0.01f)] private float webPatternRepeatUnits = 1.5f;

        private Vector3 carriageRest;
        private Vector3 cutterRest;
        private Vector3 webRest;
        private Quaternion motorRest;
        private Quaternion rollerRest;
        private Quaternion encoderRest;
        private Quaternion leadScrewRest;
        private Quaternion guardRest;

        private float commandedCarriageMillimeters;
        private float commandedWebMillimeters;
        private float commandedMotorRpm;
        private float commandedStroke01;
        private bool commandedGuardOpen;
        private float motorPhaseDegrees;
        private bool initialized;

        private void Awake() => CaptureRestPose();

        [ContextMenu("Level 49 Visuals / Capture Imported Rest Pose")]
        public void CaptureRestPose()
        {
            if (carriageSlideX != null) carriageRest = carriageSlideX.localPosition;
            if (cutterVerticalStrokeY != null) cutterRest = cutterVerticalStrokeY.localPosition;
            if (scrollingWebPattern != null) webRest = scrollingWebPattern.localPosition;
            if (carriageMotorShaftRotateX != null) motorRest = carriageMotorShaftRotateX.localRotation;
            if (measuringRollerRotateZ != null) rollerRest = measuringRollerRotateZ.localRotation;
            if (encoderDiscRotateZ != null) encoderRest = encoderDiscRotateZ.localRotation;
            if (leadScrewRotateX != null) leadScrewRest = leadScrewRotateX.localRotation;
            if (safetyDoorHinge != null) guardRest = safetyDoorHinge.localRotation;
            initialized = true;
        }

        /// <summary>
        /// Provide actual observed values, not desired values. This method does not drive
        /// any mechanical/electrical simulation; it only animates imported art groups.
        /// </summary>
        public void ApplyState(
            float webTravelMillimeters,
            float carriageTravelMillimeters,
            float measuredCarriageMotorRpm,
            float cutterStroke01,
            bool guardIsOpen,
            bool scannerBeamActive,
            bool cutterIsReady)
        {
            if (!IsFinite(webTravelMillimeters) ||
                !IsFinite(carriageTravelMillimeters) ||
                !IsFinite(measuredCarriageMotorRpm) ||
                !IsFinite(cutterStroke01)) return;

            commandedWebMillimeters = webTravelMillimeters;
            commandedCarriageMillimeters = carriageTravelMillimeters;
            commandedMotorRpm = measuredCarriageMotorRpm;
            commandedStroke01 = Mathf.Clamp01(cutterStroke01);
            commandedGuardOpen = guardIsOpen;

            if (opticalScanBeam != null && opticalScanBeam.activeSelf != scannerBeamActive)
                opticalScanBeam.SetActive(scannerBeamActive);
            if (cutterReadyLamp != null && cutterReadyLamp.activeSelf != cutterIsReady)
                cutterReadyLamp.SetActive(cutterIsReady);
        }

        private void LateUpdate()
        {
            if (!initialized) CaptureRestPose();
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            float carriageT = 1f - Mathf.Exp(-carriageVisualResponsiveness * dt);
            float actuatorT = 1f - Mathf.Exp(-actuatorVisualResponsiveness * dt);

            if (carriageSlideX != null)
            {
                Vector3 destination = carriageRest +
                    Vector3.right * (commandedCarriageMillimeters * carriageUnitsPerMillimeter);
                carriageSlideX.localPosition = Vector3.Lerp(carriageSlideX.localPosition, destination, carriageT);
            }

            if (cutterVerticalStrokeY != null)
            {
                Vector3 destination = cutterRest +
                    Vector3.down * (commandedStroke01 * cutterStrokeUnits);
                cutterVerticalStrokeY.localPosition = Vector3.Lerp(cutterVerticalStrokeY.localPosition, destination, actuatorT);
            }

            motorPhaseDegrees = Mathf.Repeat(motorPhaseDegrees + commandedMotorRpm * 6f * dt, 360f);
            if (carriageMotorShaftRotateX != null)
                carriageMotorShaftRotateX.localRotation = motorRest * Quaternion.Euler(motorPhaseDegrees, 0f, 0f);
            if (leadScrewRotateX != null)
                leadScrewRotateX.localRotation = leadScrewRest * Quaternion.Euler(motorPhaseDegrees, 0f, 0f);

            // Art encoder disc follows traveled web distance, never an invented encoder count.
            float degrees = (commandedWebMillimeters / (2f * Mathf.PI * rollerRadiusMillimeters)) * 360f;
            if (measuringRollerRotateZ != null)
                measuringRollerRotateZ.localRotation = rollerRest * Quaternion.Euler(0f, 0f, degrees);
            if (encoderDiscRotateZ != null)
                encoderDiscRotateZ.localRotation = encoderRest * Quaternion.Euler(0f, 0f, degrees);

            if (safetyDoorHinge != null)
            {
                Quaternion goal = guardRest * Quaternion.Euler(0f, commandedGuardOpen ? safetyDoorOpenAngle : 0f, 0f);
                safetyDoorHinge.localRotation = Quaternion.Slerp(safetyDoorHinge.localRotation, goal, actuatorT);
            }

            if (scrollingWebPattern != null)
            {
                float offset = Mathf.Repeat(commandedWebMillimeters * webPatternUnitsPerMillimeter,
                    webPatternRepeatUnits);
                scrollingWebPattern.localPosition = webRest + Vector3.right * offset;
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
