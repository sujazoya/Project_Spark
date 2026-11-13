using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// VISUAL ONLY. Receives position and state from the authoritative Level 48 simulation.
    /// Does not change electrical readings, wiring, encoder counts, or level objectives.
    /// The level controller decides when a mark is valid and when a print may fire.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel48VisualRig : MonoBehaviour
    {
        [Header("Moving FBX groups — assign by hierarchy name")]
        [SerializeField] private Transform measuringRollerRotateZ;
        [SerializeField] private Transform encoderDiscRotateZ;
        [SerializeField] private Transform printMotorShaftRotateX;
        [SerializeField] private Transform printHeadStrokeY;
        [SerializeField] private Transform transportWebTranslateX;

        [Header("Optical & status visuals (optional)")]
        [SerializeField] private GameObject scannerBeam;
        [SerializeField] private GameObject markDetectedGlow;
        [SerializeField] private GameObject printingGlow;

        [Header("Visual settings")]
        [SerializeField, Min(1)] private int decodedCountsPerRevolution = 80;
        [SerializeField, Min(0.001f)] private float measuringRollerRadiusMm = 20f;
        [SerializeField, Min(0.001f)] private float visibleWebRepeatMm = 251.3274f;
        [SerializeField, Min(0f)] private float webRepeatWorldUnits = 2.0f;
        [SerializeField, Min(0f)] private float maximumPrintStrokeUnits = 0.35f;
        [SerializeField] private bool invertRotation;

        private Quaternion rollerRest, encoderRest, motorRest;
        private Vector3 webRest, printRest;
        private bool initialized;

        public void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            rollerRest = measuringRollerRotateZ != null ? measuringRollerRotateZ.localRotation : Quaternion.identity;
            encoderRest = encoderDiscRotateZ != null ? encoderDiscRotateZ.localRotation : Quaternion.identity;
            motorRest = printMotorShaftRotateX != null ? printMotorShaftRotateX.localRotation : Quaternion.identity;
            webRest = transportWebTranslateX != null ? transportWebTranslateX.localPosition : Vector3.zero;
            printRest = printHeadStrokeY != null ? printHeadStrokeY.localPosition : Vector3.zero;
        }

        /// <summary>Update mechanical art using real simulation state, preferably once per rendered frame.</summary>
        /// <param name="signedEncoderCount">Authoritative unwrapped decoded encoder count.</param>
        /// <param name="webTravelMm">Distance measured by the transport system in millimetres.</param>
        /// <param name="motorShaftDegrees">Actual print motor shaft rotation since rest.</param>
        /// <param name="printStroke01">Authoritative print actuator stroke, 0=retracted and 1=extended.</param>
        public void ApplyMotion(double signedEncoderCount, double webTravelMm,
            float motorShaftDegrees, float printStroke01)
        {
            Initialize();
            float sign = invertRotation ? -1f : 1f;
            // Use double for unwrapped count history. Only the visual angle is wrapped.
            float angle = sign * (float)(((signedEncoderCount % decodedCountsPerRevolution)
                                / decodedCountsPerRevolution) * 360.0);
            if (measuringRollerRotateZ != null)
                measuringRollerRotateZ.localRotation = rollerRest * Quaternion.AngleAxis(angle, Vector3.forward);
            if (encoderDiscRotateZ != null)
                encoderDiscRotateZ.localRotation = encoderRest * Quaternion.AngleAxis(angle, Vector3.forward);
            if (printMotorShaftRotateX != null)
                printMotorShaftRotateX.localRotation = motorRest * Quaternion.AngleAxis(
                    sign * motorShaftDegrees, Vector3.right);
            if (printHeadStrokeY != null)
                printHeadStrokeY.localPosition = printRest - Vector3.up *
                    (Mathf.Clamp01(printStroke01) * maximumPrintStrokeUnits);

            if (transportWebTranslateX != null)
            {
                // A texture/UV scroll is recommended for an endless web; this modest
                // local move is a preview only and wraps every configured mark pitch.
                double phase = ((webTravelMm % visibleWebRepeatMm) + visibleWebRepeatMm) % visibleWebRepeatMm;
                float offset = (float)(phase / visibleWebRepeatMm) * webRepeatWorldUnits;
                transportWebTranslateX.localPosition = webRest + Vector3.right * offset;
            }
        }

        /// <summary>Visual signal state only; fault detection must be performed by gameplay logic.</summary>
        public void ApplyVisualSignals(bool scannerEnabled, bool validMarkDetected, bool printing)
        {
            if (scannerBeam != null) scannerBeam.SetActive(scannerEnabled);
            if (markDetectedGlow != null) markDetectedGlow.SetActive(validMarkDetected);
            if (printingGlow != null) printingGlow.SetActive(printing);
        }

        [ContextMenu("Restore Visual Rest Pose")]
        private void RestoreRestPose()
        {
            Initialize();
            if (measuringRollerRotateZ != null) measuringRollerRotateZ.localRotation = rollerRest;
            if (encoderDiscRotateZ != null) encoderDiscRotateZ.localRotation = encoderRest;
            if (printMotorShaftRotateX != null) printMotorShaftRotateX.localRotation = motorRest;
            if (printHeadStrokeY != null) printHeadStrokeY.localPosition = printRest;
            if (transportWebTranslateX != null) transportWebTranslateX.localPosition = webRest;
        }
    }
}
