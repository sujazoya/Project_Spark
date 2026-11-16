using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 87 ART-ONLY visual adapter. External gameplay authority must simulate the
    /// angular momentum, actual bearing gaps, vibration, motor torque, safety interlocks,
    /// power conservation and six objectives. No success logic lives in this class.
    /// Eight separate modular habitat pieces rotate together about an identical shared pivot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel87VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private bool bindAutomatically = true;
        [SerializeField] private Light levitationLamp;
        [SerializeField] private Light driveHealthyLamp;
        [SerializeField] private Light criticalFaultLamp;
        private readonly Dictionary<string,Transform> pivots = new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> baseRot = new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> basePos = new Dictionary<string,Vector3>();
        private static readonly string[] RotatingParts = new string[] {
            "ANIM_GRAVITY_RING_MAIN_ROTATION", "ANIM_GRAVITY_RING_SPOKES",
            "ANIM_HABITAT_SEGMENT_A", "ANIM_HABITAT_SEGMENT_B", "ANIM_HABITAT_SEGMENT_C",
            "ANIM_HABITAT_SEGMENT_D", "ANIM_HABITAT_SEGMENT_E", "ANIM_HABITAT_SEGMENT_F",
            "ANIM_HABITAT_PRESSURE_SHELL", "ANIM_ROTOR_MAGNET_ARRAY"
        };
        private float actualRingAngleDeg;
        private float actualCountermomentDeg;
        private float actualGapQuality01;
        private float actualVibration01;
        private float weightA01;
        private float weightB01;
        private float touchdownEngaged01;
        private bool levitationQualified;
        private bool driveQualified;
        private bool trustedW137Valid;
        private bool criticalFaultLatched;

        private void Awake()
        {
            if (artRoot == null) artRoot = transform.Find("L87_HD_VISUAL_ART");
            if (artRoot == null) artRoot = transform;
            if (bindAutomatically) BindVisuals();
        }
        public void BindVisuals()
        {
            pivots.Clear(); baseRot.Clear(); basePos.Clear();
            foreach (Transform t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_")) continue;
                if (pivots.ContainsKey(t.name))
                {
                    Debug.LogWarning("[Level87VisualRig] Duplicate pivot name " + t.name);
                    continue;
                }
                pivots[t.name] = t;
                baseRot[t.name] = t.localRotation;
                basePos[t.name] = t.localPosition;
            }
        }
        private void Turn(string name, Vector3 axis, float degrees)
        {
            Transform t; Quaternion home;
            if (pivots.TryGetValue(name, out t) && baseRot.TryGetValue(name, out home))
                t.localRotation = home * Quaternion.AngleAxis(degrees, axis);
        }
        private void Move(string name, Vector3 displacement)
        {
            Transform t; Vector3 home;
            if (pivots.TryGetValue(name, out t) && basePos.TryGetValue(name, out home))
                t.localPosition = home + displacement;
        }
        /// <summary>
        /// Drive visual ring angle using measured authoritative angle in degrees.
        /// Never integrate RPM from a displayed HUD label, transform or user command.
        /// </summary>
        public void SetActualRingDynamics(float actualAngleDegrees, float actualCountermomentAngleDegrees,
            float measuredRadialGapQuality01, float measuredVibrationSeverity01)
        {
            actualRingAngleDeg = actualAngleDegrees;
            actualCountermomentDeg = actualCountermomentAngleDegrees;
            actualGapQuality01 = Mathf.Clamp01(measuredRadialGapQuality01);
            actualVibration01 = Mathf.Clamp01(measuredVibrationSeverity01);
        }
        /// <summary>Actual balancing carriage positions and emergency touchdown support.</summary>
        public void SetActualBearingMechanisms(float measuredWeightA01, float measuredWeightB01,
            float actualTouchdownBearingEngagement01)
        {
            weightA01 = Mathf.Clamp01(measuredWeightA01);
            weightB01 = Mathf.Clamp01(measuredWeightB01);
            touchdownEngaged01 = Mathf.Clamp01(actualTouchdownBearingEngagement01);
        }
        public void SetActualSafety(bool measuredLevitationQualified, bool measuredDriveQualified,
            bool measuredW137FreshAndValid, bool actualCriticalFaultLatched)
        {
            levitationQualified = measuredLevitationQualified;
            driveQualified = measuredDriveQualified;
            trustedW137Valid = measuredW137FreshAndValid;
            criticalFaultLatched = actualCriticalFaultLatched;
        }
        private void LateUpdate()
        {
            foreach (string n in RotatingParts) Turn(n, Vector3.forward, actualRingAngleDeg);
            Turn("ANIM_COUNTERTORQUE_FLYWHEEL", Vector3.right, actualCountermomentDeg);
            Move("ANIM_BALANCE_WEIGHT_A", Vector3.right * (weightA01 * .18f));
            Move("ANIM_BALANCE_WEIGHT_B", Vector3.right * (weightB01 * .18f));
            Move("ANIM_EMERGENCY_TOUCHDOWN_RACE", Vector3.right * (touchdownEngaged01 * .12f));
            // The measured quality/oscillation numbers are for optional effects only.
            if (levitationLamp != null) levitationLamp.enabled = levitationQualified && trustedW137Valid && !criticalFaultLatched;
            if (driveHealthyLamp != null) driveHealthyLamp.enabled = driveQualified && actualGapQuality01 > .65f && actualVibration01 < .35f && !criticalFaultLatched;
            if (criticalFaultLamp != null) criticalFaultLamp.enabled = criticalFaultLatched || !trustedW137Valid;
        }
    }
}
