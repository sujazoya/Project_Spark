using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// ART-ONLY motion/VFX driver for the 48 actual ANIM_* FBX pivot parent groups.
    /// This component never computes electrical state, physical bonding, sensors,
    /// machine safety, or 6/6 progress. Call setters with authoritative data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel79VisualRig : MonoBehaviour
    {
        [Header("Optional authored bond and alignment VFX")]
        [SerializeField] private ParticleSystem bondWaveVFX;
        [SerializeField] private ParticleSystem surfaceActivationVFX;
        [SerializeField] private Light alignmentGoodLight;
        [SerializeField] private Light bondActiveLight;
        [SerializeField] private Light criticalMotionFaultLight;
        [SerializeField] private Light interferometerHealthyLight;
        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> basePositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> baseRotations = new Dictionary<string, Quaternion>();

        private float gantryX, gantryY, gantryZ, gripperDrop, headRotation;
        private float fineX, fineY, fineZ, thetaDeg, tipDeg, tiltDeg;
        private float traySlide, loadDoorOpen, waferAlignDeg, lowerChuckLift, chuckClamp;
        private float cameraA, cameraB, irCamera, interferometerHead, opticalOverlay01;
        private float waferBowScan, particleScan, surfaceActivation01, dryClean01;
        private float contactHead01, forceHead01, interfaceGap01, bondFraction01, voidFraction01;
        private float annealDoor, heaterUpper01, heaterLower01, thermalScan01;
        private float acousticX, acousticY, irInspection01, seamInspection01, adhesionInspection01;
        private float kelvinInspection01, daisyInspection01, linkInspection01, thermalCycle01, stackWarpScan01;
        private bool bondActualAuthorized, actualCriticalFault, interferometerFeedbackValid;
        private bool cleaningActualActive, motionActualSafe, bondActuallyPropagating;

        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_", StringComparison.Ordinal)) continue;
                if (pivots.ContainsKey(t.name)) { Debug.LogWarning("[Level79VisualRig] Duplicate pivot: " + t.name, this); continue; }
                pivots[t.name] = t;
                basePositions[t.name] = t.localPosition;
                baseRotations[t.name] = t.localRotation;
            }
            if (pivots.Count != 48) Debug.LogWarning("[Level79VisualRig] Found " + pivots.Count + "/48 ANIM_* pivots. Import all 66 FBXs.", this);
        }

        // Metres / degrees from the actual precision motion plant, never setpoint values.
        public void SetActualGantryPose(float actualXMeters, float actualYMeters, float actualZMeters,
            float actualGripperDrop01, float actualChuckRotationDegrees)
        {
            gantryX = Mathf.Clamp(actualXMeters, -2.5f, 2.5f);
            gantryY = Mathf.Clamp(actualYMeters, -1.5f, 1.5f);
            gantryZ = Mathf.Clamp(actualZMeters, -1.2f, 1.2f);
            gripperDrop = Mathf.Clamp01(actualGripperDrop01);
            headRotation = actualChuckRotationDegrees;
        }
        public void SetActualFineAlignment(float actualXMicromotion01, float actualYMicromotion01,
            float actualZMicromotion01, float actualThetaDegrees, float actualTipDegrees, float actualTiltDegrees)
        {
            fineX = Mathf.Clamp(actualXMicromotion01, -1f, 1f);
            fineY = Mathf.Clamp(actualYMicromotion01, -1f, 1f);
            fineZ = Mathf.Clamp(actualZMicromotion01, -1f, 1f);
            thetaDeg = actualThetaDegrees; tipDeg = actualTipDegrees; tiltDeg = actualTiltDegrees;
        }
        public void SetActualLoading(float actualTraySlide01, float actualDoorOpen01,
            float actualWaferAlignmentDegrees, float actualLowerChuckLift01, float actualUpperChuckClamp01)
        {
            traySlide = Mathf.Clamp01(actualTraySlide01);
            loadDoorOpen = Mathf.Clamp01(actualDoorOpen01);
            waferAlignDeg = actualWaferAlignmentDegrees;
            lowerChuckLift = Mathf.Clamp01(actualLowerChuckLift01);
            chuckClamp = Mathf.Clamp01(actualUpperChuckClamp01);
        }
        public void SetActualAlignmentInspection(float cameraAPosition01, float cameraBPosition01,
            float infraredCameraPosition01, float interferometerPosition01,
            float actualOpticalOverlayConfidence01, float actualParticleScan01, float actualWaferBowScan01,
            bool actualInterferometerFeedbackValid, bool actualMotionSafe)
        {
            cameraA = Mathf.Clamp01(cameraAPosition01); cameraB = Mathf.Clamp01(cameraBPosition01);
            irCamera = Mathf.Clamp01(infraredCameraPosition01);
            interferometerHead = Mathf.Clamp01(interferometerPosition01);
            opticalOverlay01 = Mathf.Clamp01(actualOpticalOverlayConfidence01);
            particleScan = Mathf.Clamp01(actualParticleScan01);
            waferBowScan = Mathf.Clamp01(actualWaferBowScan01);
            interferometerFeedbackValid = actualInterferometerFeedbackValid;
            motionActualSafe = actualMotionSafe;
        }
        public void SetActualSurfacePrep(float actualCleaning01, float actualActivation01, bool actuallyCleaning)
        {
            dryClean01 = Mathf.Clamp01(actualCleaning01);
            surfaceActivation01 = Mathf.Clamp01(actualActivation01);
            cleaningActualActive = actuallyCleaning;
        }
        public void SetActualBondState(float actualContactActuator01, float actualMicroforceProbe01,
            float actualInterfaceGap01, float actualBondedArea01, float actualVoidFraction01,
            bool actualBondContactAuthorized, bool actualBondFrontPropagating, bool actualCriticalMotionFault)
        {
            contactHead01 = Mathf.Clamp01(actualContactActuator01);
            forceHead01 = Mathf.Clamp01(actualMicroforceProbe01);
            interfaceGap01 = Mathf.Clamp01(actualInterfaceGap01);
            bondFraction01 = Mathf.Clamp01(actualBondedArea01);
            voidFraction01 = Mathf.Clamp01(actualVoidFraction01);
            bondActualAuthorized = actualBondContactAuthorized;
            bondActuallyPropagating = actualBondFrontPropagating;
            actualCriticalFault = actualCriticalMotionFault;
        }
        public void SetActualThermalConsolidation(float actualDoorOpen01,
            float actualUpperHeater01, float actualLowerHeater01, float actualThermalScan01)
        {
            annealDoor = Mathf.Clamp01(actualDoorOpen01);
            heaterUpper01 = Mathf.Clamp01(actualUpperHeater01);
            heaterLower01 = Mathf.Clamp01(actualLowerHeater01);
            thermalScan01 = Mathf.Clamp01(actualThermalScan01);
        }
        public void SetActualInspection(float actualAcousticX01, float actualAcousticY01,
            float actualIRScan01, float actualSeamScan01, float actualAdhesionTest01,
            float actualKelvinTest01, float actualDaisyTest01, float actualLinkTest01,
            float actualThermalCycle01, float actualStackWarpageScan01)
        {
            acousticX = Mathf.Clamp01(actualAcousticX01); acousticY = Mathf.Clamp01(actualAcousticY01);
            irInspection01 = Mathf.Clamp01(actualIRScan01);
            seamInspection01 = Mathf.Clamp01(actualSeamScan01);
            adhesionInspection01 = Mathf.Clamp01(actualAdhesionTest01);
            kelvinInspection01 = Mathf.Clamp01(actualKelvinTest01);
            daisyInspection01 = Mathf.Clamp01(actualDaisyTest01);
            linkInspection01 = Mathf.Clamp01(actualLinkTest01);
            thermalCycle01 = Mathf.Clamp01(actualThermalCycle01);
            stackWarpScan01 = Mathf.Clamp01(actualStackWarpageScan01);
        }
        private void Move(string key, Vector3 delta)
        {
            if (pivots.TryGetValue(key, out Transform t)) t.localPosition = basePositions[key] + delta;
        }
        private void Rotate(string key, Vector3 euler)
        {
            if (pivots.TryGetValue(key, out Transform t)) t.localRotation = baseRotations[key] * Quaternion.Euler(euler);
        }
        private static void Play(ParticleSystem ps, bool state)
        {
            if (ps == null) return;
            if (state && !ps.isPlaying) ps.Play();
            else if (!state && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        private void Update()
        {
            Move("ANIM_CHIPLET_TRAY", Vector3.right * (.32f * traySlide));
            Move("ANIM_LOADLOCK_DOOR", Vector3.forward * (.50f * loadDoorOpen));
            Rotate("ANIM_BASE_WAFER_ALIGNER", Vector3.up * waferAlignDeg);
            Move("ANIM_GANTRY_X", Vector3.right * gantryX);
            Move("ANIM_GANTRY_Y", Vector3.forward * gantryY);
            Move("ANIM_GANTRY_Z", Vector3.up * gantryZ);
            Move("ANIM_GRIPPER_SLIDE", Vector3.down * (.22f * gripperDrop));
            Rotate("ANIM_GRIPPER_ROTATION", Vector3.up * headRotation);
            Move("ANIM_UPPER_CHUCK_CLAMP", Vector3.right * (.12f * (1f - chuckClamp)));
            Rotate("ANIM_LOWER_CHUCK_THETA", Vector3.up * waferAlignDeg);
            Move("ANIM_LOWER_WAFER_LIFT", Vector3.up * (.22f * lowerChuckLift));
            Move("ANIM_FINE_HEXAPOD", Vector3.up * (.085f * fineZ));
            Move("ANIM_NANO_X", Vector3.right * (.045f * fineX));
            Move("ANIM_NANO_Y", Vector3.forward * (.045f * fineY));
            Rotate("ANIM_MICRO_THETA", Vector3.up * thetaDeg);
            Rotate("ANIM_TIP_AXIS", Vector3.right * tipDeg);
            Rotate("ANIM_TILT_AXIS", Vector3.forward * tiltDeg);
            Move("ANIM_CAMERA_A", Vector3.right * (.20f * cameraA));
            Move("ANIM_CAMERA_B", Vector3.left * (.20f * cameraB));
            Move("ANIM_INFRARED_LENS", Vector3.down * (.15f * irCamera));
            Move("ANIM_INTERFEROMETER_HEAD", Vector3.down * (.15f * interferometerHead));
            Move("ANIM_WARPAGE_SCANNER", Vector3.right * (.38f * waferBowScan));
            Move("ANIM_PARTICLE_SCANNER", Vector3.right * (.38f * particleScan));
            Move("ANIM_SURFACE_ACTIVATION", Vector3.down * (.22f * surfaceActivation01));
            Move("ANIM_DRY_CLEAN_HEAD", Vector3.down * (.25f * dryClean01));
            Move("ANIM_CONTACT_INITIATOR", Vector3.down * (.18f * contactHead01));
            Move("ANIM_MICROFORCE_ARRAY", Vector3.down * (.12f * forceHead01));
            Move("ANIM_INTERFACE_GAP_SCANNER", Vector3.right * (.18f * interfaceGap01));
            Move("ANIM_ANNEAL_CHAMBER_DOOR", Vector3.forward * (.36f * annealDoor));
            Move("ANIM_UPPER_HEATER", Vector3.down * (.12f * heaterUpper01));
            Move("ANIM_LOWER_HEATER", Vector3.up * (.12f * heaterLower01));
            Move("ANIM_THERMAL_EXPANSION_SCAN", Vector3.right * (.19f * thermalScan01));
            Move("ANIM_ACOUSTIC_SCAN_X", Vector3.right * (.36f * acousticX));
            Move("ANIM_ACOUSTIC_SCAN_Y", Vector3.forward * (.36f * acousticY));
            Rotate("ANIM_IR_TOMOGRAPHY", Vector3.up * (80f * irInspection01));
            Move("ANIM_BOND_SEAM_CAMERA", Vector3.right * (.15f * seamInspection01));
            Move("ANIM_ADHESION_COUPON_TEST", Vector3.down * (.12f * adhesionInspection01));
            Move("ANIM_KELVIN_PROBE", Vector3.down * (.14f * kelvinInspection01));
            Move("ANIM_DAISY_CHAIN_PROBE", Vector3.down * (.14f * daisyInspection01));
            Move("ANIM_HIGH_SPEED_TEST_HEAD", Vector3.down * (.14f * linkInspection01));
            Move("ANIM_THERMAL_CYCLING_TRAY", Vector3.forward * (.24f * thermalCycle01));
            Move("ANIM_STACK_WARPAGE_SCAN", Vector3.right * (.35f * stackWarpScan01));
            Move("ANIM_EMERGENCY_STOP", Vector3.back * (actualCriticalFault ? .035f : 0f));
            if (alignmentGoodLight != null) alignmentGoodLight.enabled = interferometerFeedbackValid && motionActualSafe && opticalOverlay01 > .90f && !actualCriticalFault;
            if (interferometerHealthyLight != null) interferometerHealthyLight.enabled = interferometerFeedbackValid && !actualCriticalFault;
            if (bondActiveLight != null) bondActiveLight.enabled = bondActualAuthorized && bondActuallyPropagating && !actualCriticalFault;
            if (criticalMotionFaultLight != null) criticalMotionFaultLight.enabled = actualCriticalFault;
            Play(surfaceActivationVFX, cleaningActualActive && surfaceActivation01 > .01f);
            Play(bondWaveVFX, bondActualAuthorized && bondActuallyPropagating && bondFraction01 < 1f && !actualCriticalFault);
            // Update bond-front, void, alignment and stack hologram materials separately from their real process-state data.
            // voidFraction01 is available to a dedicated material/hologram adapter.
        }
    }
}
