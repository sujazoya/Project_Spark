using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// ART ONLY. Drives FBX groups from measured gameplay values supplied by another system.
    /// Does not calculate tension, write electrical state, or complete objectives.
    /// Pair each reference to the appropriately named FBX group / pivot in Animation_Rigs.csv.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel47VisualRig : MonoBehaviour
    {
        [Header("Feed / unwind — animated FBX groups")]
        [SerializeField] private Transform feedMotorShaft;
        [SerializeField] private Transform feedSpool;
        [SerializeField] private Transform feedWoundMaterial;
        [SerializeField] private Transform feedEncoderDisc;
        [SerializeField] private Transform feedBrakeRotor;

        [Header("Take-up / wind — animated FBX groups")]
        [SerializeField] private Transform takeupMotorShaft;
        [SerializeField] private Transform takeupSpool;
        [SerializeField] private Transform takeupWoundMaterial;
        [SerializeField] private Transform takeupEncoderDisc;

        [Header("Dancer feedback")]
        [SerializeField] private Transform dancerArm;
        [SerializeField] private Transform dancerRoller;
        [SerializeField] private Transform tensionSensorWiper;
        [SerializeField] private Transform brakeLever;
        [SerializeField] private Transform brakePad;

        [Header("Visual tuning — not physical parameters")]
        [SerializeField] private float feedRotationSign = 1f;
        [SerializeField] private float takeupRotationSign = -1f;
        [SerializeField, Range(0f, 45f)] private float maximumDancerAngle = 17f;
        [SerializeField, Range(0f, 45f)] private float maximumBrakeAngle = 22f;
        [SerializeField, Min(0f)] private float maximumBrakePadTravel = 0.14f;
        [SerializeField, Range(.5f, 1f)] private float minimumFeedMaterialScale = .78f;
        [SerializeField, Range(1f, 1.5f)] private float maximumTakeupMaterialScale = 1.18f;
        [SerializeField, Min(.01f)] private float presentationSmoothing = 7f;

        // Inputs supplied by the actual Level 47 simulation. Never treated as authoritative.
        private float feedRpm;
        private float takeupRpm;
        private float targetDancer;
        private float targetBrake;
        private float windingProgress;
        private float feedRadiusScale=1f, takeupRadiusScale=1f;
        private bool useRadiusScales;

        public void SetMaterialScales(float feedRatio,float takeupRatio)
        {
            feedRadiusScale=feedRatio;
            takeupRadiusScale=takeupRatio;
            useRadiusScales=true;
        }
        private bool hasLiveInputs;

        private Quaternion dancerRest, wiperRest, brakeRest;
        private Vector3 padRest, feedMaterialRest, takeupMaterialRest;
        private float dancerShown, brakeShown;

        private void Awake()
        {
            if (dancerArm != null) dancerRest = dancerArm.localRotation;
            if (tensionSensorWiper != null) wiperRest = tensionSensorWiper.localRotation;
            if (brakeLever != null) brakeRest = brakeLever.localRotation;
            if (brakePad != null) padRest = brakePad.localPosition;
            if (feedWoundMaterial != null) feedMaterialRest = feedWoundMaterial.localScale;
            if (takeupWoundMaterial != null) takeupMaterialRest = takeupWoundMaterial.localScale;
        }

        /// <summary>
        /// Call after the mechanical simulation advances. Measured RPM and normalized values
        /// must originate from real Level 47 state (not this art controller).
        /// dancerSigned: -1 to +1; brake01 and materialTransfer01: 0 to 1.
        /// </summary>
        public void SetVisualState(float measuredFeedRpm, float measuredTakeupRpm,
            float dancerSigned, float brake01, float materialTransfer01)
        {
            feedRpm = Mathf.Clamp(measuredFeedRpm, -10000f, 10000f);
            takeupRpm = Mathf.Clamp(measuredTakeupRpm, -10000f, 10000f);
            targetDancer = Mathf.Clamp(dancerSigned, -1f, 1f);
            targetBrake = Mathf.Clamp01(brake01);
            windingProgress = Mathf.Clamp01(materialTransfer01);
            hasLiveInputs = true;
        }

        public void StopVisualMotion()
        {
            feedRpm = 0f;
            takeupRpm = 0f;
            hasLiveInputs = true;
        }

        private void Update()
        {
            if (!hasLiveInputs) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Unity RPM conversion: 360 degrees / minute = 6 degrees / second per RPM.
            float feedDegrees = feedRpm * 6f * dt * feedRotationSign;
            float takeupDegrees = takeupRpm * 6f * dt * takeupRotationSign;
            RotateLocalZ(feedMotorShaft, feedDegrees);
            RotateLocalZ(feedSpool, feedDegrees);
            RotateLocalZ(feedWoundMaterial, feedDegrees);
            RotateLocalZ(feedEncoderDisc, feedDegrees);
            RotateLocalZ(feedBrakeRotor, feedDegrees);
            RotateLocalZ(takeupMotorShaft, takeupDegrees);
            RotateLocalZ(takeupSpool, takeupDegrees);
            RotateLocalZ(takeupWoundMaterial, takeupDegrees);
            RotateLocalZ(takeupEncoderDisc, takeupDegrees);
            RotateLocalZ(dancerRoller, (feedDegrees - takeupDegrees) * .5f);

            float alpha = 1f - Mathf.Exp(-presentationSmoothing * dt);
            dancerShown = Mathf.Lerp(dancerShown, targetDancer, alpha);
            brakeShown = Mathf.Lerp(brakeShown, targetBrake, alpha);

            if (dancerArm != null)
                dancerArm.localRotation = dancerRest *
                    Quaternion.Euler(0f, 0f, maximumDancerAngle * dancerShown);
            if (tensionSensorWiper != null)
                tensionSensorWiper.localRotation = wiperRest *
                    Quaternion.Euler(0f, maximumDancerAngle * dancerShown, 0f);
            if (brakeLever != null)
                brakeLever.localRotation = brakeRest *
                    Quaternion.Euler(0f, 0f, maximumBrakeAngle * brakeShown);
            if (brakePad != null)
                brakePad.localPosition = padRest + Vector3.left *
                    (maximumBrakePadTravel * brakeShown);

            // Scale only the wound material groups, never the metal spool flange/shaft.
            if (feedWoundMaterial != null)
            {
                float s = useRadiusScales ? feedRadiusScale : Mathf.Lerp(1f, minimumFeedMaterialScale, windingProgress);
                feedWoundMaterial.localScale = Vector3.Scale(feedMaterialRest,
                    new Vector3(s, s, 1f));
            }
            if (takeupWoundMaterial != null)
            {
                float s = useRadiusScales ? takeupRadiusScale : Mathf.Lerp(1f, maximumTakeupMaterialScale, windingProgress);
                takeupWoundMaterial.localScale = Vector3.Scale(takeupMaterialRest,
                    new Vector3(s, s, 1f));
            }
        }

        private static void RotateLocalZ(Transform pivot, float degrees)
        {
            if (pivot != null)
                pivot.Rotate(0f, 0f, degrees, Space.Self);
        }
    }
}
