using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// LEVEL 53 — THE HIDDEN COUPLE (VISUALS ONLY).
    /// Receives already-solved motor/encoder speeds, correction angles, guard state,
    /// and sensor validity from the authoritative Level 53 gameplay systems.
    /// Does not alter electrical components, solved measurements, physical vibration,
    /// objective gates or completion. All exported art groups are separate siblings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel53VisualRig : MonoBehaviour
    {
        [Header("FBX: rotor shaft, motor, correction planes, encoder")]
        [SerializeField] private Transform motorShaftRotateX;
        [SerializeField] private Transform longRotorShaftRotateX;
        [SerializeField] private Transform planeARotorRotateX;
        [SerializeField] private Transform planeBRotorRotateX;
        [SerializeField] private Transform encoderABZDiscRotateX;

        [Header("FBX: independently adjustable correction weights")]
        [SerializeField] private Transform planeACorrectionWeightOrbitX;
        [SerializeField] private Transform planeBCorrectionWeightOrbitX;
        [SerializeField] private float authoredWeightAngleA = 35f;
        [SerializeField] private float authoredWeightAngleB = 225f;

        [Header("FBX: safety enclosure and indicator lenses")]
        [SerializeField] private Transform safetyFrontGuardDoorHingeY;
        [SerializeField] private Renderer leftSensorIndicator;
        [SerializeField] private Renderer rightSensorIndicator;
        [SerializeField] private Renderer safetyIndicator;
        [SerializeField] private Color healthyColor = new Color(0.12f, 0.96f, 0.68f);
        [SerializeField] private Color faultyColor = new Color(1.0f, 0.23f, 0.27f);
        [SerializeField, Range(0f, 130f)] private float guardOpenAngle = 84f;
        [SerializeField, Min(0.01f)] private float guardAnimationSpeed = 7f;

        private Quaternion motorRest, shaftRest, planeARest, planeBRest, encoderRest;
        private Quaternion weightARest, weightBRest, doorRest;
        private MaterialPropertyBlock block;
        private float measuredRotorRpm, measuredEncoderRpm, shaftAngle;
        private float weightAngleA = 35f, weightAngleB = 225f;
        private bool useExternalShaftAngle, guardOpen, leftSensorValid = true,
            rightSensorValid = true, safeCondition = true;

        private static Quaternion GetRest(Transform t) =>
            t != null ? t.localRotation : Quaternion.identity;

        private void Awake()
        {
            motorRest = GetRest(motorShaftRotateX);
            shaftRest = GetRest(longRotorShaftRotateX);
            planeARest = GetRest(planeARotorRotateX);
            planeBRest = GetRest(planeBRotorRotateX);
            encoderRest = GetRest(encoderABZDiscRotateX);
            weightARest = GetRest(planeACorrectionWeightOrbitX);
            weightBRest = GetRest(planeBCorrectionWeightOrbitX);
            doorRest = GetRest(safetyFrontGuardDoorHingeY);
            block = new MaterialPropertyBlock();
            UpdateIndicators();
        }

        private void Update()
        {
            float dt = Mathf.Max(0f, Time.deltaTime);
            if (!useExternalShaftAngle)
                shaftAngle = Mathf.Repeat(shaftAngle + measuredRotorRpm * 6f * dt, 360f);

            RotateX(motorShaftRotateX, motorRest, shaftAngle);
            RotateX(longRotorShaftRotateX, shaftRest, shaftAngle);
            RotateX(planeARotorRotateX, planeARest, shaftAngle);
            RotateX(planeBRotorRotateX, planeBRest, shaftAngle);

            // The encoder is modelled as a separate rotating disc.
            // Only its ART rotation is derived here.
            if (encoderABZDiscRotateX != null)
                encoderABZDiscRotateX.Rotate(Vector3.right, measuredEncoderRpm * 6f * dt, Space.Self);

            // The weight orbit groups are siblings of each plane's rotating disc.
            // Thus each weight receives full shaft phase + its own correction offset.
            RotateX(planeACorrectionWeightOrbitX, weightARest,
                shaftAngle + weightAngleA - authoredWeightAngleA);
            RotateX(planeBCorrectionWeightOrbitX, weightBRest,
                shaftAngle + weightAngleB - authoredWeightAngleB);

            if (safetyFrontGuardDoorHingeY != null)
            {
                Quaternion desired = doorRest * Quaternion.Euler(
                    0f, guardOpen ? guardOpenAngle : 0f, 0f);
                safetyFrontGuardDoorHingeY.localRotation = Quaternion.Slerp(
                    safetyFrontGuardDoorHingeY.localRotation,
                    desired, Mathf.Clamp01(guardAnimationSpeed * dt));
            }
        }

        private static void RotateX(Transform t, Quaternion rest, float degrees)
        {
            if (t != null)
                t.localRotation = rest * Quaternion.AngleAxis(degrees, Vector3.right);
        }

        /// <summary>Supply REAL, externally computed motor and encoder RPM.</summary>
        public void SetMeasuredRPM(float rotorRpm, float encoderRpm)
        {
            measuredRotorRpm = Finite(rotorRpm) ? rotorRpm : 0f;
            measuredEncoderRpm = Finite(encoderRpm) ? encoderRpm : 0f;
        }

        /// <summary>
        /// Optional: supply externally measured shaft position (degrees).
        /// This keeps the visuals phase-locked to the real simulated shaft.
        /// Call DisableExternalShaftAngle() to revert to visual RPM integration.
        /// </summary>
        public void SetMeasuredShaftAngle(float degrees)
        {
            if (!Finite(degrees)) return;
            shaftAngle = Mathf.Repeat(degrees, 360f);
            useExternalShaftAngle = true;
        }

        public void DisableExternalShaftAngle() => useExternalShaftAngle = false;

        /// <summary>
        /// Supply correction weight angles FROM YOUR ACTUAL actuator system,
        /// not guessed ideal angles. Angles measured from local +Y toward +Z.
        /// </summary>
        public void SetCorrectionAngles(float planeA, float planeB)
        {
            if (Finite(planeA)) weightAngleA = Mathf.Repeat(planeA, 360f);
            if (Finite(planeB)) weightAngleB = Mathf.Repeat(planeB, 360f);
        }

        public void SetGuardOpen(bool open) => guardOpen = open;

        public void SetSensorValidity(bool leftValid, bool rightValid)
        {
            leftSensorValid = leftValid;
            rightSensorValid = rightValid;
            UpdateIndicators();
        }

        public void SetSafetyCondition(bool safe)
        {
            safeCondition = safe;
            UpdateIndicators();
        }

        private void UpdateIndicators()
        {
            SetIndicator(leftSensorIndicator, leftSensorValid ? healthyColor : faultyColor);
            SetIndicator(rightSensorIndicator, rightSensorValid ? healthyColor : faultyColor);
            SetIndicator(safetyIndicator, safeCondition ? healthyColor : faultyColor);
        }

        private void SetIndicator(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var material = renderer.sharedMaterial;
            if (material != null)
            {
                if (material.HasProperty("_BaseColor")) block.SetColor("_BaseColor", color);
                if (material.HasProperty("_Color")) block.SetColor("_Color", color);
                if (material.HasProperty("_EmissionColor")) block.SetColor("_EmissionColor", color);
            }
            renderer.SetPropertyBlock(block);
            block.Clear();
        }

        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
