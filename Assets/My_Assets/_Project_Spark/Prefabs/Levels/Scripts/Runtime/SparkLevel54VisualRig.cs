using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual-only art adapter for Level 54 "Cross the Critical".
    /// It does NOT calculate physics, read the electrical solver, generate sensor readings,
    /// evaluate objectives, or command motor/damper electrical components.
    /// The level gameplay coordinator supplies real simulation values through ApplyState().
    /// FBX part pivots are local Y-up; the long drive shaft spins around local X.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel54VisualRig : MonoBehaviour
    {
        [Serializable]
        public struct VisualState
        {
            public float ShaftAngleDegrees;
            public Vector2 ShaftOrbitMillimeters;       // X / Y transverse displacement in simulated mm.
            public Vector2 LeftBearingMillimeters;
            public Vector2 RightBearingMillimeters;
            public float DamperGain01;
            public bool SensorsCalibrated;
            public Vector2 DamperForceMillinewtons;
            public bool XSensorValid;
            public bool YSensorValid;
            public bool DamperEnabled;
            public bool SafetyGuardOpen;
            public bool CriticalFault;
        }

        [Header("Rotation Pivots")]
        [SerializeField] private Transform motorShaft;
        [SerializeField] private Transform probeX, probeY;
        [SerializeField] private Transform rotorDisc;
        [SerializeField] private Transform encoderDisc;
        [SerializeField] private Transform[] flexibleShaftSegments = new Transform[7];

        [Header("Structural Motion / Mechanical Actuators")]
        [SerializeField] private Transform leftFlexureBearing;
        [SerializeField] private Transform rightFlexureBearing;
        [SerializeField] private Transform activeDamperX;
        [SerializeField] private Transform activeDamperY;
        [SerializeField] private Transform safetyDoorHinge;

        [Header("Optional Indicator Meshes")]
        [SerializeField] private Renderer xSensorLamp;
        [SerializeField] private Renderer ySensorLamp;
        [SerializeField] private Renderer activeDamperLamp;
        [SerializeField] private Renderer machineFaultLamp;

        [Header("Visual Settings — NOT Simulation Values")]
        [SerializeField, Min(0f)] private float visualArtUnitsPerMillimeter = 12f;
        [SerializeField, Range(0f, 135f)] private float doorOpenDegrees = 85f;
        [SerializeField] private Vector3 flexureTranslationAxisX = Vector3.forward;
        [SerializeField] private Vector3 flexureTranslationAxisY = Vector3.up;
        [SerializeField] private bool findNamedPivotsAtAwake = true;

        private Vector3[] shaftRest;
        private Vector3 leftRest, rightRest, damperXRest, damperYRest;
        private Quaternion doorRest;
        private Vector3 rotorRest, probeXRest, probeYRest;
        private MaterialPropertyBlock propertyBlock;
        private bool initialized;

        private void Awake()
        {
            if (findNamedPivotsAtAwake) ResolveNamedPivots();
            CaptureRestState();
        }

        [ContextMenu("Find Level 54 FBX Pivots")]
        public void ResolveNamedPivots()
        {
            Transform[] all = GetComponentsInChildren<Transform>(true);
            if (motorShaft == null) motorShaft = FindNamed(all, "MOTOR_SHAFT_ROTATE_X");
            if (rotorDisc == null) rotorDisc = FindNamed(all, "TEST_ROTOR_ROTATE_X");
            if (encoderDisc == null) encoderDisc = FindNamed(all, "ENCODER_DISC_ROTATE_X");
            if (leftFlexureBearing == null) leftFlexureBearing = FindNamed(all, "LEFT_FLEXURE_BEARING_YZ");
            if (rightFlexureBearing == null) rightFlexureBearing = FindNamed(all, "RIGHT_FLEXURE_BEARING_YZ");
            if (activeDamperX == null) activeDamperX = FindNamed(all, "DAMPER_X_FORCE");
            if (activeDamperY == null) activeDamperY = FindNamed(all, "DAMPER_Y_FORCE");
            if (safetyDoorHinge == null) safetyDoorHinge = FindNamed(all, "SAFETY_DOOR_HINGE");
            if (flexibleShaftSegments == null || flexibleShaftSegments.Length != 7)
                flexibleShaftSegments = new Transform[7];
            for (int i = 0; i < 7; i++)
                if (flexibleShaftSegments[i] == null)
                    flexibleShaftSegments[i] = FindNamed(all, "SHAFT_FLEX_SEGMENT_" + i.ToString("00"));
        }

        private static Transform FindNamed(Transform[] transforms, string expected)
        {
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name == expected) return transforms[i];
            return null;
        }

        [ContextMenu("Capture Art Rest State")]
        public void CaptureRestState()
        {
            shaftRest = new Vector3[7];
            for (int i = 0; i < 7; i++)
                if (flexibleShaftSegments != null && i < flexibleShaftSegments.Length && flexibleShaftSegments[i] != null)
                    shaftRest[i] = flexibleShaftSegments[i].localPosition;
            if (leftFlexureBearing != null) leftRest = leftFlexureBearing.localPosition;
            if (rightFlexureBearing != null) rightRest = rightFlexureBearing.localPosition;
            if (activeDamperX != null) damperXRest = activeDamperX.localPosition;
            if (activeDamperY != null) damperYRest = activeDamperY.localPosition;
            if (safetyDoorHinge != null) doorRest = safetyDoorHinge.localRotation;
            if (rotorDisc != null) rotorRest = rotorDisc.localPosition;
            if (probeX != null) probeXRest = probeX.localPosition;
            if (probeY != null) probeYRest = probeY.localPosition;
            propertyBlock = new MaterialPropertyBlock();
            initialized = true;
        }

        public void ApplyState(VisualState state)
        {
            if (!initialized) CaptureRestState();
            float rotation = state.ShaftAngleDegrees;
            if (motorShaft != null) motorShaft.localRotation = Quaternion.AngleAxis(rotation, Vector3.right);
            if (rotorDisc != null) rotorDisc.localRotation = Quaternion.AngleAxis(rotation, Vector3.right);
            if (encoderDisc != null) encoderDisc.localRotation = Quaternion.AngleAxis(rotation, Vector3.right);

            float scale = Mathf.Max(0, visualArtUnitsPerMillimeter);
            if (rotorDisc != null)
                rotorDisc.localPosition = rotorRest + scale * (state.ShaftOrbitMillimeters.x * flexureTranslationAxisX +
                    state.ShaftOrbitMillimeters.y * flexureTranslationAxisY);
            if (probeX != null) probeX.localPosition = probeXRest + (state.SensorsCalibrated ? Vector3.forward * -.006f : Vector3.zero);
            if (probeY != null) probeY.localPosition = probeYRest + (state.SensorsCalibrated ? Vector3.down * .006f : Vector3.zero);
            for (int i = 0; i < 7; i++)
            {
                if (flexibleShaftSegments == null || i >= flexibleShaftSegments.Length || flexibleShaftSegments[i] == null) continue;
                // A static first-mode-shaped art displacement. Actual simulation owns the waveform and amplitude.
                float modeShape = Mathf.Sin(Mathf.PI * (i + .5f) / 7f);
                Vector3 v = flexureTranslationAxisX * (state.ShaftOrbitMillimeters.x * scale * modeShape)
                          + flexureTranslationAxisY * (state.ShaftOrbitMillimeters.y * scale * modeShape);
                flexibleShaftSegments[i].localPosition = shaftRest[i] + v;
                flexibleShaftSegments[i].localRotation = Quaternion.AngleAxis(rotation, Vector3.right);
            }
            if (leftFlexureBearing != null)
                leftFlexureBearing.localPosition = leftRest +
                    scale * (state.LeftBearingMillimeters.x * flexureTranslationAxisX + state.LeftBearingMillimeters.y * flexureTranslationAxisY);
            if (rightFlexureBearing != null)
                rightFlexureBearing.localPosition = rightRest +
                    scale * (state.RightBearingMillimeters.x * flexureTranslationAxisX + state.RightBearingMillimeters.y * flexureTranslationAxisY);

            // The damper force pivots are visualization markers only. Do not move real magnetic hardware here.
            if (activeDamperX != null) activeDamperX.localPosition = damperXRest + Vector3.forward * state.DamperForceMillinewtons.x * .00020f;
            if (activeDamperY != null) activeDamperY.localPosition = damperYRest + Vector3.up * state.DamperForceMillinewtons.y * .00020f;
            if (safetyDoorHinge != null)
                safetyDoorHinge.localRotation = doorRest * Quaternion.AngleAxis(state.SafetyGuardOpen ? doorOpenDegrees : 0f, Vector3.up);

            SetLamp(xSensorLamp, state.XSensorValid ? Color.cyan : Color.red);
            SetLamp(ySensorLamp, state.YSensorValid ? new Color(1f, .65f, .15f) : Color.red);
            SetLamp(activeDamperLamp, state.DamperEnabled ? Color.green : Color.gray);
            SetLamp(machineFaultLamp, state.CriticalFault ? Color.red : Color.green);
        }

        private void SetLamp(Renderer renderer, Color emissionColor)
        {
            if (renderer == null) return;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_EmissionColor", emissionColor);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}

