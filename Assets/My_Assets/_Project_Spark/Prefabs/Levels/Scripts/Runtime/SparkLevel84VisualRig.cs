using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 84 ART-ONLY actual-state visual rig. Reads 58 real imported FBX ANIM_ parents.
    /// No flight dynamics, star-tracker physics, SparkTerminal wiring, safety authority or objectives here.
    /// Must be called by the real ADCS gameplay coordinator after its own authoritative simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel84VisualRig : MonoBehaviour
    {
        [Header("Optional visually representative effects — bind in Unity")]
        [SerializeField] private Light starTrackerAcquisitionLight;
        [SerializeField] private Light fineAttitudeLockLight;
        [SerializeField] private Light opticalBoresightLight;
        [SerializeField] private Light momentumDumpLight;
        [SerializeField] private Light criticalFeedbackFaultLight;
        [SerializeField] private Light trustedAttitudeFeedbackLight;
        [SerializeField] private ParticleSystem starAcquisitionParticles;
        [SerializeField] private ParticleSystem opticalLinkParticles;

        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Quaternion> homeRotation = new Dictionary<string, Quaternion>();
        private readonly Dictionary<string, Vector3> homePosition = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Vector3> homeScale = new Dictionary<string, Vector3>();
        private readonly float[] measuredWheelRpm = new float[4];
        private readonly float[] measuredWheelMomentum01 = new float[4];
        private readonly float[] measuredWheelTemperatureC = new float[4];
        private readonly float[] rotorAngle = new float[4];
        private static readonly string[] WheelPivots =
        {
            "ANIM_WHEEL_X_ROTOR", "ANIM_WHEEL_Y_ROTOR", "ANIM_WHEEL_Z_ROTOR", "ANIM_WHEEL_SKEW_ROTOR"
        };

        private Quaternion actualAttitude = Quaternion.identity;
        private Vector3 measuredBodyRatesDegPerSecond;
        private Vector3 measuredMagneticField;
        private Vector3 actualCommandedMagneticDipole;
        private float starAcquisition01, estimatorUncertaintyDegrees, gyroBiasSeverity01;
        private float pointingErrorDegrees, jitterRmsDegrees, measuredOpticalTraffic01;
        private float wingFlexibleMode01, wheelSaturationSeverity01, certification01, missionTime01;
        private float starTrackerBaffleOpen01, sensorCalibration01, groundStationTarget01;
        private bool trustedW097, starCatalogMatched, actualFineControl, actualOpticalLinkLocked;
        private bool actualMagneticUnloading, actualCoarseSunSafe, actualCriticalFaultLatched;
        private int qualifiedGroundPasses;

        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_", StringComparison.Ordinal)) continue;
                if (pivots.ContainsKey(t.name))
                {
                    Debug.LogWarning("[Level84VisualRig] Duplicate pivot name: " + t.name, this);
                    continue;
                }
                pivots[t.name] = t;
                homeRotation[t.name] = t.localRotation;
                homePosition[t.name] = t.localPosition;
                homeScale[t.name] = t.localScale;
            }
            if (pivots.Count != 58)
                Debug.LogWarning("[Level84VisualRig] Found " + pivots.Count + "/58 named pivots. Attach to L84_HD_VISUAL_ART containing all 76 FBXs.", this);
        }

        /// <summary>Authoritative body-to-world attitude and measured gyro angular rates. Quaternion is a visual input only.</summary>
        public void SetActualAttitude(Quaternion actualBodyToWorld, Vector3 measuredBodyRatesDegreesPerSecond)
        {
            actualAttitude = actualBodyToWorld;
            measuredBodyRatesDegPerSecond = measuredBodyRatesDegreesPerSecond;
        }

        /// <summary>Measured wheel mechanical state (indices X,Y,Z,Skew), not motor requested setpoints.</summary>
        public void SetActualReactionWheels(float[] actualRpm, float[] measuredMomentumUtilization01, float[] measuredTemperaturesC)
        {
            for (int i = 0; i < 4; i++)
            {
                measuredWheelRpm[i] = actualRpm != null && i < actualRpm.Length ? actualRpm[i] : 0f;
                measuredWheelMomentum01[i] = Mathf.Clamp01(measuredMomentumUtilization01 != null && i < measuredMomentumUtilization01.Length ? measuredMomentumUtilization01[i] : 0f);
                measuredWheelTemperatureC[i] = measuredTemperaturesC != null && i < measuredTemperaturesC.Length ? measuredTemperaturesC[i] : 20f;
            }
        }

        /// <summary>Actual dipole and field, with independent real desaturation state.</summary>
        public void SetActualMagneticSystem(Vector3 actualMagneticDipoleBody, Vector3 measuredLocalMagneticFieldBody, bool actualMomentumUnloading)
        {
            actualCommandedMagneticDipole = actualMagneticDipoleBody;
            measuredMagneticField = measuredLocalMagneticFieldBody;
            actualMagneticUnloading = actualMomentumUnloading;
        }

        /// <summary>Validated primary tracker W097 feedback, estimator state and star catalog solution.</summary>
        public void SetActualNavigationSensors(float measuredStarMatchConfidence01, float measuredEstimatorUncertaintyDegrees,
            bool trustedW097StarTrackerMeasurementValid, bool actualStarCatalogMatchQualified,
            float measuredGyroBiasSeverity01, float actualTrackerBaffleOpen01, float actualCalibrationProgress01)
        {
            starAcquisition01 = Mathf.Clamp01(measuredStarMatchConfidence01);
            estimatorUncertaintyDegrees = Mathf.Max(0f, measuredEstimatorUncertaintyDegrees);
            trustedW097 = trustedW097StarTrackerMeasurementValid;
            starCatalogMatched = actualStarCatalogMatchQualified;
            gyroBiasSeverity01 = Mathf.Clamp01(measuredGyroBiasSeverity01);
            starTrackerBaffleOpen01 = Mathf.Clamp01(actualTrackerBaffleOpen01);
            sensorCalibration01 = Mathf.Clamp01(actualCalibrationProgress01);
        }

        /// <summary>Measured optical boresight quality and two real simulated qualifying pass completions.</summary>
        public void SetActualOpticalPointing(float measuredPointingErrorDegrees, float measuredJitterRmsDegrees,
            bool actualPointingControllerQualified, bool actualOpticalDownlinkLocked,
            float measuredTransmittedDataActivity01, int actualQualifiedPassCount)
        {
            pointingErrorDegrees = Mathf.Max(0f, measuredPointingErrorDegrees);
            jitterRmsDegrees = Mathf.Max(0f, measuredJitterRmsDegrees);
            actualFineControl = actualPointingControllerQualified;
            actualOpticalLinkLocked = actualOpticalDownlinkLocked;
            measuredOpticalTraffic01 = Mathf.Clamp01(measuredTransmittedDataActivity01);
            qualifiedGroundPasses = Mathf.Clamp(actualQualifiedPassCount, 0, 2);
        }

        /// <summary>Visual indicators for three real ADCS fault conditions and latched critical protection.</summary>
        public void SetActualSafetyAndFaults(bool actualCriticalW097FeedbackValid, bool actualCriticalFaultIsLatched,
            bool actualCoarseSunSafeMode, float actualGyroBiasDisturbance01,
            float actualWheelSaturationDisturbance01, float actualSolarWingFlexibleMotion01)
        {
            trustedW097 = actualCriticalW097FeedbackValid;
            actualCriticalFaultLatched = actualCriticalFaultIsLatched;
            actualCoarseSunSafe = actualCoarseSunSafeMode;
            gyroBiasSeverity01 = Mathf.Clamp01(actualGyroBiasDisturbance01);
            wheelSaturationSeverity01 = Mathf.Clamp01(actualWheelSaturationDisturbance01);
            wingFlexibleMode01 = Mathf.Clamp01(actualSolarWingFlexibleMotion01);
        }

        /// <summary>Actual mission time and target-window model, independent of decorative SCADA.</summary>
        public void SetActualMissionState(float normalizedMissionTime01, float targetTrackingProgress01, float actualCertificationProgress01)
        {
            missionTime01 = Mathf.Clamp01(normalizedMissionTime01);
            groundStationTarget01 = Mathf.Clamp01(targetTrackingProgress01);
            certification01 = Mathf.Clamp01(actualCertificationProgress01);
        }

        private void SetRotate(string name, Vector3 eulerDegrees)
        {
            Transform t; Quaternion q;
            if (pivots.TryGetValue(name, out t) && homeRotation.TryGetValue(name, out q))
                t.localRotation = q * Quaternion.Euler(eulerDegrees);
        }
        private void SetQuaternion(string name, Quaternion measuredRotation)
        {
            Transform t; Quaternion q;
            if (pivots.TryGetValue(name, out t) && homeRotation.TryGetValue(name, out q))
                t.localRotation = q * measuredRotation;
        }
        private void SetTranslate(string name, Vector3 deltaMetres)
        {
            Transform t; Vector3 origin;
            if (pivots.TryGetValue(name, out t) && homePosition.TryGetValue(name, out origin))
                t.localPosition = origin + deltaMetres;
        }
        private void ScaleIndicator(string name, float normalizedValue)
        {
            Transform t; Vector3 orig;
            if (pivots.TryGetValue(name, out t) && homeScale.TryGetValue(name, out orig))
                t.localScale = new Vector3(orig.x, orig.y * Mathf.Max(.10f, Mathf.Clamp01(normalizedValue)), orig.z);
        }
        private static void SetLight(Light light, bool enabled, float intensity)
        {
            if (light == null) return;
            light.enabled = enabled;
            if (enabled) light.intensity = intensity;
        }
        private static void SetParticles(ParticleSystem fx, bool on)
        {
            if (fx == null) return;
            if (on && !fx.isPlaying) fx.Play();
            else if (!on && fx.isPlaying) fx.Stop();
        }

        private void LateUpdate()
        {
            // Three nested gimbal rings are visualizations of attitude; they DO NOT apply spacecraft torque.
            Vector3 euler = actualAttitude.eulerAngles;
            SetRotate("ANIM_OUTER_YAW_RING", new Vector3(0f, euler.y, 0f));
            SetRotate("ANIM_MIDDLE_PITCH_RING", new Vector3(euler.x, 0f, 0f));
            SetRotate("ANIM_INNER_ROLL_RING", new Vector3(0f, 0f, euler.z));
            SetQuaternion("ANIM_SPACECRAFT_BODY_ATTITUDE", actualAttitude);

            // Flywheel rotors derive exclusively from measured RPM (deg/sec = rpm * 6).
            for (int i = 0; i < 4; i++)
            {
                rotorAngle[i] = Mathf.Repeat(rotorAngle[i] + measuredWheelRpm[i] * 6f * Time.deltaTime, 360f);
                SetRotate(WheelPivots[i], new Vector3(0f, 0f, rotorAngle[i]));
            }
            float maxMomentum = Mathf.Max(Mathf.Max(measuredWheelMomentum01[0], measuredWheelMomentum01[1]),
                                          Mathf.Max(measuredWheelMomentum01[2], measuredWheelMomentum01[3]));
            SetTranslate("ANIM_REACTION_WHEEL_MOMENTUM_BARS", new Vector3(0f, maxMomentum * .17f, 0f));
            SetTranslate("ANIM_WHEEL_SATURATION_FAULT", new Vector3(wheelSaturationSeverity01 * .16f, 0f, 0f));
            SetRotate("ANIM_WHEEL_TACHOMETER_SCAN", new Vector3(0f, 0f, Time.time * 26f));
            SetRotate("ANIM_WHEEL_COOLING_FAN", new Vector3(0f, 0f, Time.time * 170f));

            // Only visual field/dipole orientation: the actual m x B torque belongs to the external gameplay solver.
            SetRotate("ANIM_TORQUE_ROD_X_VECTOR", new Vector3(0f, 0f, actualCommandedMagneticDipole.x * 7f));
            SetRotate("ANIM_TORQUE_ROD_Y_VECTOR", new Vector3(0f, 0f, actualCommandedMagneticDipole.y * 7f));
            SetRotate("ANIM_TORQUE_ROD_Z_VECTOR", new Vector3(0f, 0f, actualCommandedMagneticDipole.z * 7f));
            SetRotate("ANIM_MAGNETIC_FIELD_LINES", new Vector3(measuredMagneticField.x * 3f, measuredMagneticField.y * 3f, measuredMagneticField.z * 3f));
            SetRotate("ANIM_MOMENTUM_DESAT_ARC", new Vector3(0f, actualMagneticUnloading ? Time.time * 34f : 0f, 0f));

            // Star-tracker and estimator visuals follow trusted samples, not desired command values.
            SetTranslate("ANIM_STAR_TRACKER_BAFFLE", new Vector3(0f, 0f, .17f * starTrackerBaffleOpen01));
            SetRotate("ANIM_PRIMARY_STAR_TRACKER_APERTURE", new Vector3(0f, 0f, starAcquisition01 * 20f));
            SetRotate("ANIM_GYRO_CALIBRATION_SPINDLE", new Vector3(0f, sensorCalibration01 * 300f, 0f));
            SetTranslate("ANIM_GYRO_BIAS_FAULT", new Vector3(gyroBiasSeverity01 * .20f, 0f, 0f));
            SetTranslate("ANIM_ESTIMATOR_UNCERTAINTY_CONE", new Vector3(0f, Mathf.Min(.4f, estimatorUncertaintyDegrees * .035f), 0f));
            SetRotate("ANIM_ATTITUDE_QUATERNION", euler);
            SetTranslate("ANIM_SOLAR_WING_FLEX_FAULT", new Vector3(0f, Mathf.Sin(Time.time * 13f) * wingFlexibleMode01 * .16f, 0f));
            SetTranslate("ANIM_STRUCTURAL_MODE_TRACE", new Vector3(Mathf.Sin(Time.time * 9f) * wingFlexibleMode01 * .16f, 0f, 0f));

            SetTranslate("ANIM_GROUND_STATION_TARGET", new Vector3((groundStationTarget01 - .5f) * .35f, 0f, 0f));
            SetTranslate("ANIM_POINTING_TARGET_RETICLE", new Vector3(Mathf.Min(pointingErrorDegrees, 5f) * .025f, 0f, 0f));
            SetTranslate("ANIM_OPTICAL_LINE_OF_SIGHT", new Vector3(0f, jitterRmsDegrees * .08f, 0f));
            SetTranslate("ANIM_GROUND_PASS_WINDOW", new Vector3(qualifiedGroundPasses * .18f, 0f, 0f));
            SetTranslate("ANIM_MISSION_TIMELINE", new Vector3(missionTime01 * .26f, 0f, 0f));
            ScaleIndicator("ANIM_OPTICAL_LINK_LOCK", actualOpticalLinkLocked ? 1f : .16f);
            ScaleIndicator("ANIM_OPTICAL_DOWNLINK_DISPLAY", measuredOpticalTraffic01);
            SetRotate("ANIM_SUN_SAFE_ACQUISITION", new Vector3(0f, actualCoarseSunSafe ? 48f : 0f, 0f));
            SetTranslate("ANIM_CRITICAL_W097_INDICATOR", new Vector3(trustedW097 ? 0f : .15f, 0f, 0f));
            ScaleIndicator("ANIM_ADCS_CERTIFICATION_BEACON", certification01);

            bool trustedStars = trustedW097 && starCatalogMatched && starAcquisition01 > .6f && !actualCriticalFaultLatched;
            bool qualifiedFine = trustedStars && actualFineControl && !actualCoarseSunSafe;
            SetLight(starTrackerAcquisitionLight, trustedStars, .65f + starAcquisition01);
            SetLight(fineAttitudeLockLight, qualifiedFine, 1.3f);
            SetLight(opticalBoresightLight, qualifiedFine && actualOpticalLinkLocked, 1.4f);
            SetLight(momentumDumpLight, actualMagneticUnloading, 1.15f);
            SetLight(criticalFeedbackFaultLight, actualCriticalFaultLatched || !trustedW097, 1.8f);
            SetLight(trustedAttitudeFeedbackLight, trustedStars, 1.1f);
            SetParticles(starAcquisitionParticles, trustedStars);
            SetParticles(opticalLinkParticles, qualifiedFine && actualOpticalLinkLocked && measuredOpticalTraffic01 > .01f);
        }
    }
}
