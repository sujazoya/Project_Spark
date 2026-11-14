using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// ART ONLY: Visual adapter for Level 55 magnetic-bearing models.
    /// Does not create magnetic force, current, sensor samples, electrical stamps,
    /// level objectives, or completion events. A verified gameplay controller must
    /// supply the observed state through ApplyState(...).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel55VisualRig : MonoBehaviour
    {
        [Header("FBX animation pivot nodes")]
        [SerializeField] private Transform rotorSpin;
        [SerializeField] private Transform encoderDisc;
        [SerializeField] private Transform rotorArtRoot;
        [SerializeField] private Transform safetyDoor;
        [SerializeField] private Transform disturbancePlunger;

        [Header("Art calibration")]
        [SerializeField, Min(0f)] private float positionAmplification = 12f;
        [SerializeField, Min(0.1f)] private float supportSpanMeters = 3.13f;
        [SerializeField] private Vector3 shaftAxisLocal = Vector3.right;
        [SerializeField] private Vector3 verticalAxisLocal = Vector3.up;
        [SerializeField] private Vector3 lateralAxisLocal = Vector3.forward;
        [SerializeField, Range(0f, 95f)] private float maxDoorAngle = 72f;
        [SerializeField, Min(0f)] private float disturbanceArtTravel = 0.20f;

        private Vector3 rotorStartPosition, plungerStartPosition;
        private Quaternion rotorStartRotation, encoderStartRotation, doorStartRotation;
        private bool initialized;
        private Quaternion rootRest;
        [SerializeField] private Renderer[] leftIndicators, rightIndicators;
        private MaterialPropertyBlock glow;

        private void Awake() { CaptureRestPose(); }

        [ContextMenu("Capture current visual rest pose")]
        public void CaptureRestPose()
        {
            rotorStartPosition = rotorArtRoot != null ? rotorArtRoot.localPosition : Vector3.zero;
            rotorStartRotation = rotorSpin != null ? rotorSpin.localRotation : Quaternion.identity;
            encoderStartRotation = encoderDisc != null ? encoderDisc.localRotation : Quaternion.identity;
            doorStartRotation = safetyDoor != null ? safetyDoor.localRotation : Quaternion.identity;
            plungerStartPosition = disturbancePlunger != null ? disturbancePlunger.localPosition : Vector3.zero;
            rootRest = rotorArtRoot != null ? rotorArtRoot.localRotation : Quaternion.identity;
            glow = new MaterialPropertyBlock();
            initialized = true;
        }

        /// <summary>
        /// Visual-only input from the authoritative simulation.
        /// Shaft angle uses degrees. Left/right sensor positions are millimeters,
        /// measured in the rotor's local Y/Z axes. No values are inferred or fabricated.
        /// </summary>
        public void ApplyState(
            float shaftAngleDegrees,
            float leftXmm, float leftYmm, float rightXmm, float rightYmm,
            float safetyDoorOpen01, float disturbance01)
        {
            if (!initialized) CaptureRestPose();
            if (!Valid(shaftAngleDegrees) || !Valid(leftXmm) || !Valid(leftYmm) ||
                !Valid(rightXmm) || !Valid(rightYmm) || !Valid(safetyDoorOpen01) || !Valid(disturbance01))
                return;

            Vector3 axis = shaftAxisLocal.sqrMagnitude > 0.001f ? shaftAxisLocal.normalized : Vector3.right;
            if (rotorSpin != null)
                rotorSpin.localRotation = rotorStartRotation * Quaternion.AngleAxis(shaftAngleDegrees, axis);
            if (encoderDisc != null)
                encoderDisc.localRotation = encoderStartRotation * Quaternion.AngleAxis(shaftAngleDegrees, axis);

            if (rotorArtRoot != null)
            {
                float avgX = 0.5f * (leftXmm + rightXmm) * 0.001f * positionAmplification;
                float avgY = 0.5f * (leftYmm + rightYmm) * 0.001f * positionAmplification;
                rotorArtRoot.localPosition = rotorStartPosition +
                    lateralAxisLocal.normalized * avgX + verticalAxisLocal.normalized * avgY;
                var tilt = (verticalAxisLocal.normalized * (rightYmm-leftYmm) +
                    lateralAxisLocal.normalized * (rightXmm-leftXmm)) * (.001f * positionAmplification / supportSpanMeters);
                rotorArtRoot.localRotation = Quaternion.FromToRotation(axis,(axis+tilt).normalized) * rootRest;
                // Do not modify other stationary electrical socket or safety transforms here.
            }
            if (safetyDoor != null)
                safetyDoor.localRotation = doorStartRotation *
                    Quaternion.AngleAxis(Mathf.Clamp01(safetyDoorOpen01) * maxDoorAngle, Vector3.right);
            if (disturbancePlunger != null)
                disturbancePlunger.localPosition = plungerStartPosition +
                    Vector3.forward * (Mathf.Clamp01(disturbance01) * disturbanceArtTravel);
        }

        public void ShowStatus(float left, float right, bool fault)
        {
            glow ??= new MaterialPropertyBlock();
            void Set(Renderer[] lamps, float current, Color tint)
            {
                if (lamps == null) return;
                glow.SetColor("_EmissionColor", (fault ? Color.red : tint) * Mathf.Clamp01(fault ? 1 : current*10));
                foreach (var lamp in lamps) if (lamp) lamp.SetPropertyBlock(glow);
            }
            Set(leftIndicators,left,new Color(.18f,.94f,1));
            Set(rightIndicators,right,new Color(1,.65f,.12f));
        }

        private static bool Valid(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}


