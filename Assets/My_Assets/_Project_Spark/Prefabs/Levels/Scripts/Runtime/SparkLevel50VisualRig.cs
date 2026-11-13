using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Level 50 art-only animation bridge. Feed it actual positions and states from
    /// the authoritative Level 50 simulation. Does not simulate electricity,
    /// create products, stamp, cut, classify, or award level objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel50VisualRig : MonoBehaviour
    {
        [Header("Assign named FBX animation groups (from Animation_Rigs.csv)")]
        [SerializeField] private Transform webAdvanceX;
        [SerializeField] private Transform measuringRollerRotateZ;
        [SerializeField] private Transform encoderDiscRotateZ;
        [SerializeField] private Transform printHeadStrokeY;
        [SerializeField] private Transform cutterCarriageSlideX;
        [SerializeField] private Transform cutterBladeStrokeY;
        [SerializeField] private Transform cutterLeadScrewRotateX;
        [SerializeField] private Transform sortingArmPivotY;
        [SerializeField] private Transform sortMotorShaftRotateX;

        [Header("Model-space travel from actual world measurements")]
        [SerializeField, Min(0.001f)] private float measuringRollerRadiusMeters = 0.020f;
        [SerializeField, Min(0.001f)] private float webArtRepeatMeters = 0.12f;
        [SerializeField] private float webArtRepeatDistance = 1.6f;
        [SerializeField, Min(0f)] private float printArtStroke = 0.30f;
        [SerializeField, Min(0f)] private float cutArtStroke = 0.32f;
        [SerializeField, Min(0f)] private float carriageArtUnitsPerMeter = 42f;

        private Vector3 webOrigin, printOrigin, carriageOrigin, bladeOrigin;
        private Quaternion rollerOrigin, encoderOrigin, screwOrigin, diverterOrigin, sortShaftOrigin;
        private bool initialized;

        private void Awake() => CaptureRestPose();

        [ContextMenu("Level 50 / Capture Art Rest Pose")]
        public void CaptureRestPose()
        {
            if (webAdvanceX) webOrigin = webAdvanceX.localPosition;
            if (printHeadStrokeY) printOrigin = printHeadStrokeY.localPosition;
            if (cutterCarriageSlideX) carriageOrigin = cutterCarriageSlideX.localPosition;
            if (cutterBladeStrokeY) bladeOrigin = cutterBladeStrokeY.localPosition;
            if (measuringRollerRotateZ) rollerOrigin = measuringRollerRotateZ.localRotation;
            if (encoderDiscRotateZ) encoderOrigin = encoderDiscRotateZ.localRotation;
            if (cutterLeadScrewRotateX) screwOrigin = cutterLeadScrewRotateX.localRotation;
            if (sortingArmPivotY) diverterOrigin = sortingArmPivotY.localRotation;
            if (sortMotorShaftRotateX) sortShaftOrigin = sortMotorShaftRotateX.localRotation;
            initialized = true;
        }

        /// <summary>
        /// Call from the level-local simulation after it advances. webDistanceMeters
        /// is the real accumulated travel; carriageMeters is relative to home;
        /// stroke values are 0..1. sortAngleDegrees is a genuine gate command.
        /// This function changes only model Transforms.
        /// </summary>
        public void ApplyVisualState(float webDistanceMeters, float carriageMeters,
            float printStroke01, float cutStroke01, float sortAngleDegrees)
        {
            if (!initialized) CaptureRestPose();

            float revolutions = measuringRollerRadiusMeters > 0f
                ? webDistanceMeters / (2f * Mathf.PI * measuringRollerRadiusMeters) : 0f;
            float rotorDegrees = -revolutions * 360f;
            if (measuringRollerRotateZ)
                measuringRollerRotateZ.localRotation = rollerOrigin * Quaternion.Euler(0f, 0f, rotorDegrees);
            if (encoderDiscRotateZ)
                encoderDiscRotateZ.localRotation = encoderOrigin * Quaternion.Euler(0f, 0f, rotorDegrees);
            if (webAdvanceX)
            {
                float offset = Mathf.Repeat(webDistanceMeters, webArtRepeatMeters)
                    / Mathf.Max(.001f, webArtRepeatMeters) * webArtRepeatDistance;
                webAdvanceX.localPosition = webOrigin + Vector3.left * offset;
            }
            if (printHeadStrokeY)
                printHeadStrokeY.localPosition = printOrigin + Vector3.down * Mathf.Clamp01(printStroke01) * printArtStroke;
            if (cutterCarriageSlideX)
                cutterCarriageSlideX.localPosition = carriageOrigin + Vector3.right * carriageMeters * carriageArtUnitsPerMeter;
            if (cutterBladeStrokeY)
                cutterBladeStrokeY.localPosition = bladeOrigin + Vector3.down * Mathf.Clamp01(cutStroke01) * cutArtStroke;
            if (cutterLeadScrewRotateX)
                cutterLeadScrewRotateX.localRotation = screwOrigin * Quaternion.Euler(rotorDegrees, 0f, 0f);
            if (sortingArmPivotY)
                sortingArmPivotY.localRotation = diverterOrigin * Quaternion.Euler(0f, sortAngleDegrees, 0f);
            if (sortMotorShaftRotateX)
                sortMotorShaftRotateX.localRotation = sortShaftOrigin * Quaternion.Euler(sortAngleDegrees * 9f, 0f, 0f);
        }

        [ContextMenu("Level 50 / Reset Art to Rest")]
        public void ResetToRestPose()
        {
            if (!initialized) CaptureRestPose();
            if (webAdvanceX) webAdvanceX.localPosition = webOrigin;
            if (printHeadStrokeY) printHeadStrokeY.localPosition = printOrigin;
            if (cutterCarriageSlideX) cutterCarriageSlideX.localPosition = carriageOrigin;
            if (cutterBladeStrokeY) cutterBladeStrokeY.localPosition = bladeOrigin;
            if (measuringRollerRotateZ) measuringRollerRotateZ.localRotation = rollerOrigin;
            if (encoderDiscRotateZ) encoderDiscRotateZ.localRotation = encoderOrigin;
            if (cutterLeadScrewRotateX) cutterLeadScrewRotateX.localRotation = screwOrigin;
            if (sortingArmPivotY) sortingArmPivotY.localRotation = diverterOrigin;
            if (sortMotorShaftRotateX) sortMotorShaftRotateX.localRotation = sortShaftOrigin;
        }
    }
}
