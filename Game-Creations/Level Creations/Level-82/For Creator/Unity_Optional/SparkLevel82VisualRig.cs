using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 82 ART-ONLY adapter. Consume actual EMC plant/safety values; no electrical topology,
    /// field physics, live SparkTerminal wiring, safety decisions or objective certification.
    /// Pivot parents are already embedded in the individual Level 82 FBX/GLB files.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel82VisualRig : MonoBehaviour
    {
        [Header("Optional actual-state indicators")]
        [SerializeField] private Light emiHotspotLight;
        [SerializeField] private Light conductedEmissionPassLight;
        [SerializeField] private Light radiatedEmissionPassLight;
        [SerializeField] private Light rfExposureLight;
        [SerializeField] private Light criticalRFFaultLight;
        [SerializeField] private Light fieldFeedbackHealthyLight;
        [SerializeField] private Light missionFunctionHealthyLight;
        [SerializeField] private ParticleSystem falseColorFieldHologram;

        private readonly Dictionary<string, Transform> pivot = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> homePos = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> homeRot = new Dictionary<string, Quaternion>();
        private float turntableDegrees, doorOpen01, antennaHeight01, antennaDistance01, polarizationDeg;
        private float nearfieldX01, nearfieldY01, nearfieldZ01, scannerActivity01, measuredHotspot01;
        private float shieldLid01, seamCompression01, ferriteLatch01, filterDrawer01, harnessRoute01;
        private float measuredConductedMargin01, measuredRadiatedMargin01, thermalCameraPanDegrees;
        private float rfGeneratorKnob01, rfAmplitude01, fieldProbeHeight01, amplifierShutterClosed01;
        private float opticalMonitor01, clockJitter01, sensorDisturbance01, certification01;
        private bool actualRFPowerAuthorized, actualTrustedW119Valid, actualCriticalFaultLatched;
        private bool actualEUTMissionHealthy, actualConductedPass, actualRadiatedPass, actualChamberDoorClosed;

        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_", StringComparison.Ordinal)) continue;
                if (pivot.ContainsKey(t.name))
                {
                    Debug.LogWarning("[Level82VisualRig] Duplicate named pivot " + t.name, this);
                    continue;
                }
                pivot.Add(t.name, t);
                homePos.Add(t.name, t.localPosition);
                homeRot.Add(t.name, t.localRotation);
            }
            if (pivot.Count != 54)
                Debug.LogWarning("[Level82VisualRig] Found " + pivot.Count + "/54 pivot parents. Attach to the common art root containing all 72 modular FBX instances.", this);
        }

        /// <summary>Actual chamber and turntable sensor feedback, not requested setpoints.</summary>
        public void SetActualChamber(float actualTurntableAzimuthDeg, float actualDoorOpen01,
            bool actualChamberDoorClosed, float actualShieldLidOpen01, float actualSeamCompression01)
        {
            turntableDegrees = actualTurntableAzimuthDeg;
            doorOpen01 = Mathf.Clamp01(actualDoorOpen01);
            this.actualChamberDoorClosed = actualChamberDoorClosed;
            shieldLid01 = Mathf.Clamp01(actualShieldLidOpen01);
            seamCompression01 = Mathf.Clamp01(actualSeamCompression01);
        }

        /// <summary>Real measurement-position states; does not create RF field strength.</summary>
        public void SetActualScanMotion(float actualAntennaHeight01, float actualPolarizationDeg,
            float actualAntennaDistance01, float actualNearfieldX01,
            float actualNearfieldY01, float actualNearfieldZ01, float actualScanActivity01)
        {
            antennaHeight01 = Mathf.Clamp01(actualAntennaHeight01);
            polarizationDeg = actualPolarizationDeg;
            antennaDistance01 = Mathf.Clamp01(actualAntennaDistance01);
            nearfieldX01 = Mathf.Clamp01(actualNearfieldX01);
            nearfieldY01 = Mathf.Clamp01(actualNearfieldY01);
            nearfieldZ01 = Mathf.Clamp01(actualNearfieldZ01);
            scannerActivity01 = Mathf.Clamp01(actualScanActivity01);
        }

        /// <summary>State of actual physical EMC repairs; not a command to mark emissions PASS.</summary>
        public void SetActualRepairs(float actualHarnessRoute01, float actualFerriteLatch01,
            float actualFilterDrawer01, float actualGasketCompression01,
            float actualThermalCameraPanDegrees)
        {
            harnessRoute01 = Mathf.Clamp01(actualHarnessRoute01);
            ferriteLatch01 = Mathf.Clamp01(actualFerriteLatch01);
            filterDrawer01 = Mathf.Clamp01(actualFilterDrawer01);
            seamCompression01 = Mathf.Clamp01(actualGasketCompression01);
            thermalCameraPanDegrees = actualThermalCameraPanDegrees;
        }

        /// <summary>Use measured emission state and actual executed measurements, never fake qualification.</summary>
        public void SetActualEmissionMeasurements(float measuredHotspotSeverity01,
            float measuredConductedMargin01, float measuredRadiatedMargin01,
            bool actualConductedPass, bool actualRadiatedPass)
        {
            measuredHotspot01 = Mathf.Clamp01(measuredHotspotSeverity01);
            this.measuredConductedMargin01 = Mathf.Clamp01(measuredConductedMargin01);
            this.measuredRadiatedMargin01 = Mathf.Clamp01(measuredRadiatedMargin01);
            this.actualConductedPass = actualConductedPass;
            this.actualRadiatedPass = actualRadiatedPass;
        }

        /// <summary>
        /// Use actual RF source dynamics and monitored W119 validity. A missing W119 never
        /// becomes a good measurement just because this art method was called.
        /// </summary>
        public void SetActualRFImmunity(float actualGeneratorDial01, float actualExposureAmplitude01,
            float actualFieldProbeHeight01, float actualAmplifierShutterClosed01,
            bool actualRFPowerAuthorized, bool trustedW119FieldFeedbackValid)
        {
            rfGeneratorKnob01 = Mathf.Clamp01(actualGeneratorDial01);
            rfAmplitude01 = Mathf.Clamp01(actualExposureAmplitude01);
            fieldProbeHeight01 = Mathf.Clamp01(actualFieldProbeHeight01);
            amplifierShutterClosed01 = Mathf.Clamp01(actualAmplifierShutterClosed01);
            this.actualRFPowerAuthorized = actualRFPowerAuthorized;
            actualTrustedW119Valid = trustedW119FieldFeedbackValid;
        }

        public void SetActualMissionMonitoring(float measuredSensorDisturbance01,
            float measuredClockJitter01, float measuredOpticalTelemetryActivity01,
            bool actualMissionComputerQualified, float actualCertificationProgress01)
        {
            sensorDisturbance01 = Mathf.Clamp01(measuredSensorDisturbance01);
            clockJitter01 = Mathf.Clamp01(measuredClockJitter01);
            opticalMonitor01 = Mathf.Clamp01(measuredOpticalTelemetryActivity01);
            actualEUTMissionHealthy = actualMissionComputerQualified;
            certification01 = Mathf.Clamp01(actualCertificationProgress01);
        }

        public void SetActualRFSafety(bool actualCriticalFaultLatched,
            bool actualW119TrustedFeedbackValid, bool actualRFSourcePermission,
            float actualAmplifierShutterClosed01)
        {
            this.actualCriticalFaultLatched = actualCriticalFaultLatched;
            actualTrustedW119Valid = actualW119TrustedFeedbackValid;
            actualRFPowerAuthorized = actualRFSourcePermission;
            amplifierShutterClosed01 = Mathf.Clamp01(actualAmplifierShutterClosed01);
        }

        private void Translate(string name, Vector3 offset)
        {
            if (pivot.TryGetValue(name, out Transform t)) t.localPosition = homePos[name] + offset;
        }
        private void Rotate(string name, Vector3 euler)
        {
            if (pivot.TryGetValue(name, out Transform t)) t.localRotation = homeRot[name] * Quaternion.Euler(euler);
        }
        private static void LightState(Light lamp, bool enabled, float intensity = 1f)
        {
            if (lamp == null) return;
            lamp.enabled = enabled;
            lamp.intensity = Mathf.Max(0f, intensity);
        }

        private void LateUpdate()
        {
            Rotate("ANIM_EUT_TURNTABLE", new Vector3(0f, turntableDegrees, 0f));
            Rotate("ANIM_AZIMUTH_ENCODER", new Vector3(0f, turntableDegrees, 0f));
            Rotate("ANIM_CHAMBER_RF_DOOR", new Vector3(0f, doorOpen01 * 92f, 0f));
            Translate("ANIM_SHIELD_CANOPY_LIFT", new Vector3(0f, shieldLid01 * .62f, 0f));
            Translate("ANIM_SEAM_GASKET_COMPRESSION", new Vector3(0f, (1f-seamCompression01)*.07f, 0f));
            Translate("ANIM_ANTENNA_MAST_VERTICAL", new Vector3(0f, (antennaHeight01-.5f)*3.5f, 0f));
            Rotate("ANIM_ANTENNA_POLARIZATION", new Vector3(0f, 0f, polarizationDeg));
            Translate("ANIM_ANTENNA_DISTANCE_SLIDE", new Vector3((antennaDistance01-.5f)*.8f, 0f, 0f));
            Translate("ANIM_NEARFIELD_SCAN_X", new Vector3((nearfieldX01-.5f)*1.7f, 0f, 0f));
            Translate("ANIM_NEARFIELD_SCAN_Y", new Vector3(0f, 0f, (nearfieldY01-.5f)*1.1f));
            Translate("ANIM_NEARFIELD_SCAN_Z", new Vector3(0f, (nearfieldZ01-.5f)*.72f, 0f));
            Translate("ANIM_HARNESS_CABLE_TRAY", new Vector3(0f, 0f, harnessRoute01*.28f));
            Rotate("ANIM_FERRITE_CLIP_LATCH", new Vector3(ferriteLatch01*92f, 0f, 0f));
            Translate("ANIM_SENSOR_FILTER_DRAWER", new Vector3(0f, 0f, filterDrawer01*.32f));
            Rotate("ANIM_THERMAL_CAMERA_PAN", new Vector3(0f,thermalCameraPanDegrees,0f));
            Rotate("ANIM_RF_GENERATOR_DIAL", new Vector3(0f,0f,rfGeneratorKnob01*300f));
            Translate("ANIM_FIELD_PROBE_CARRIAGE", new Vector3(0f,(fieldProbeHeight01-.5f)*1.0f,0f));
            Translate("ANIM_RF_AMPLIFIER_SHUTTER", new Vector3(0f,-amplifierShutterClosed01*.48f,0f));
            Rotate("ANIM_RF_PRESELECTOR_DIAL", new Vector3(0f,0f,scannerActivity01*245f));
            Rotate("ANIM_SPREAD_SPECTRUM_DIAL", new Vector3(0f,0f,clockJitter01*200f));
            Rotate("ANIM_OPTICAL_LINK_MONITOR", new Vector3(0f,0f,0f)); // indicator-only
            LightState(emiHotspotLight, measuredHotspot01>.05f, measuredHotspot01*3f);
            LightState(conductedEmissionPassLight, actualConductedPass);
            LightState(radiatedEmissionPassLight, actualRadiatedPass);
            LightState(rfExposureLight, actualRFPowerAuthorized && actualTrustedW119Valid && rfAmplitude01>.01f, rfAmplitude01*2.8f);
            LightState(criticalRFFaultLight, actualCriticalFaultLatched);
            LightState(fieldFeedbackHealthyLight, actualTrustedW119Valid);
            LightState(missionFunctionHealthyLight, actualEUTMissionHealthy);
            if (falseColorFieldHologram != null)
            {
                bool visible = actualRFPowerAuthorized && actualTrustedW119Valid && rfAmplitude01>.01f;
                if (visible && !falseColorFieldHologram.isPlaying) falseColorFieldHologram.Play();
                else if (!visible && falseColorFieldHologram.isPlaying) falseColorFieldHologram.Stop();
            }
        }
    }
}
