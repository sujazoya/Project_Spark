using System;
using UnityEngine;
using TMPro;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual-only animation adapter for Level 46 KEEP IN STEP.
    /// Does NOT simulate a motor, generate encoder pulses, modify the circuit,
    /// or report objective completion. Feed authentic runtime readings with
    /// ApplyReadings() from your existing motor/synchronization simulation.
    ///
    /// Import the individual FBX models and assign their named animated groups.
    /// Both roller and motor shaft artwork use local Z as the shaft axis.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel46VisualRig : MonoBehaviour
    {
        [Header("MASTER / cyan rotating groups (local Z)")]
        [SerializeField] private Transform masterMotorShaft;
        [SerializeField] private Transform masterRoller;
        [SerializeField] private Transform masterEncoderDisc;

        [Header("FOLLOWER / amber rotating groups (local Z)")]
        [SerializeField] private Transform followerMotorShaft;
        [SerializeField] private Transform followerRoller;
        [SerializeField] private Transform followerEncoderDisc;
        [SerializeField] private Transform followerBrakeRotor;

        [Header("FOLLOWER brake mechanism (visual only)")]
        [SerializeField] private Transform brakeArm;
        [SerializeField] private Transform brakePad;
        [SerializeField, Range(0f, 65f)] private float brakeArmAngle = 28f;
        [SerializeField, Min(0f)] private float brakePadTravel = 0.10f;

        [Header("Optional live UI (TMP)")]
        [SerializeField] private TMP_Text masterRpmText;
        [SerializeField] private TMP_Text followerRpmText;
        [SerializeField] private TMP_Text differenceText;
        [SerializeField] private TMP_Text modeText;

        [Header("Optional status lens renderers")]
        [SerializeField] private Renderer synchronizedLamp;
        [SerializeField] private Renderer loadLamp;
        [SerializeField] private Renderer faultLamp;
        [SerializeField] private string emissionProperty = "_EmissionColor";
        [SerializeField] private Color onColor = new Color(0.15f, 1f, 0.78f, 1f);
        [SerializeField] private Color loadColor = new Color(1f, 0.56f, 0.12f, 1f);
        [SerializeField] private Color faultColor = new Color(1f, 0.20f, 0.25f, 1f);
        [SerializeField] private Color offColor = new Color(0.035f, 0.075f, 0.10f, 1f);

        private float masterRPM;
        private float followerRPM;
        private float brakeAmount;
        private bool synchronized;
        private bool fault;
        private string modeName = "OFF";
        private Quaternion initialArmRotation;
        private Vector3 initialPadPosition;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            initialArmRotation = brakeArm ? brakeArm.localRotation : Quaternion.identity;
            initialPadPosition = brakePad ? brakePad.localPosition : Vector3.zero;
            block = new MaterialPropertyBlock();
            RefreshUI();
        }

        /// <summary>
        /// Called by real Level 46 simulation after new solved/measured state is available.
        /// RPM is signed to permit negative rotation during direction tests.
        /// Never synthesize readings merely for the level evaluator.
        /// </summary>
        public void ApplyReadings(float measuredMasterRPM, float measuredFollowerRPM,
            float measuredBrakeFraction, bool isSynchronized, bool hasFault, string controlMode)
        {
            masterRPM = SanitizeRPM(measuredMasterRPM);
            followerRPM = SanitizeRPM(measuredFollowerRPM);
            brakeAmount = float.IsNaN(measuredBrakeFraction) ? 0f : Mathf.Clamp01(measuredBrakeFraction);
            synchronized = isSynchronized;
            fault = hasFault;
            modeName = string.IsNullOrEmpty(controlMode) ? "UNKNOWN" : controlMode;
            RefreshUI();
        }

        private static float SanitizeRPM(float rpm)
        {
            if (float.IsNaN(rpm) || float.IsInfinity(rpm)) return 0f;
            return Mathf.Clamp(rpm, -5000f, 5000f);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            float dt = Time.deltaTime;
            float masterDegrees = masterRPM * 6f * dt;
            float followerDegrees = followerRPM * 6f * dt;

            Spin(masterMotorShaft, masterDegrees);
            Spin(masterRoller, masterDegrees);
            Spin(masterEncoderDisc, masterDegrees);

            Spin(followerMotorShaft, followerDegrees);
            Spin(followerRoller, followerDegrees);
            Spin(followerEncoderDisc, followerDegrees);
            Spin(followerBrakeRotor, followerDegrees);

            if (brakeArm)
                brakeArm.localRotation = initialArmRotation *
                    Quaternion.Euler(0f, brakeAmount * brakeArmAngle, 0f);
            if (brakePad)
                brakePad.localPosition = initialPadPosition +
                    Vector3.left * (brakeAmount * brakePadTravel);
        }

        private static void Spin(Transform tr, float angleDegrees)
        {
            if (tr && !Mathf.Approximately(angleDegrees, 0f))
                tr.Rotate(0f, 0f, angleDegrees, Space.Self);
        }

        private void RefreshUI()
        {
            if (masterRpmText) masterRpmText.text = masterRPM.ToString("F1") + " RPM";
            if (followerRpmText) followerRpmText.text = followerRPM.ToString("F1") + " RPM";
            if (differenceText) differenceText.text =
                Mathf.Abs(masterRPM - followerRPM).ToString("F1") + " RPM DIFF";
            if (modeText) modeText.text = modeName;

            if (block == null) block = new MaterialPropertyBlock();
            SetLens(synchronizedLamp, !fault && synchronized ? onColor : offColor);
            SetLens(loadLamp, brakeAmount > 0.01f ? loadColor : offColor);
            SetLens(faultLamp, fault ? faultColor : offColor);
        }

        private void SetLens(Renderer renderer, Color color)
        {
            if (!renderer || !renderer.sharedMaterial ||
                !renderer.sharedMaterial.HasProperty(emissionProperty)) return;

            renderer.GetPropertyBlock(block);
            block.SetColor(emissionProperty, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
