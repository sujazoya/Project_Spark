using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 80 original photonics ART-ONLY rig. All values MUST come from the authoritative
    /// electrical + optical + thermal + data simulation, never from optimistic UI setpoints.
    /// Does not create SparkTerminals, safety logic, optical power or 6/6 objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel80VisualRig : MonoBehaviour
    {
        [SerializeField] private ParticleSystem visibleFalseColorOpticalPaths;
        [SerializeField] private Light protectedLaserLight;
        [SerializeField] private Light channelALight;
        [SerializeField] private Light channelBLight;
        [SerializeField] private Light channelCLight;
        [SerializeField] private Light channelDLight;
        [SerializeField] private Light criticalFaultLight;
        [SerializeField] private Light feedbackHealthyLight;

        public Transform art;
        public Transform[] pivots;
        private readonly Dictionary<string, Transform> pivotLookup = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> startPosition = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> startRotation = new Dictionary<string, Quaternion>();
        private readonly float[] actualRingTemperature01 = new float[4];
        private readonly float[] actualRingDetune01 = new float[4];
        private readonly bool[] actualRingLocked = new bool[4];
        private readonly float[] actualTraffic01 = new float[4];

        private float laserDrawer01, laserTemperatureStage01, shutterClosed01 = 1f;
        private float isolatorAngleDeg, splitterAngleDeg, polarizationAngleDeg, attenuatorActual01;
        private float spectrumScan01, powerProbe01, fiberDock01, couplerArm01, testFixtureLid01;
        private float dropPortPosition01, rxProbe01, microscope01, fiberBendProbe01;
        private float referenceStage01, faultInjectionArm01, emergencyShutterClosed01 = 1f;
        private float coolingFanActualRPM, actualLaunchPower01, actualRxPower01;
        private float actualBERQuality01, actualEyeOpening01, actualLinkLock01, actualThermalCrosstalk01;
        private bool actualLaserAuthorized, actualShutterOpen, trustedReceiverPowerValid, actualCriticalFault;
        private float coolingFanDegrees;

        private void Awake()
        {
            foreach (Transform t in pivots != null && pivots.Length > 0 ? pivots : GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                int offset=t.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(offset<0)continue;
                string key=t.name.Substring(offset);
                if (pivotLookup.ContainsKey(key))
                {
                    Debug.LogWarning("[Level80VisualRig] Duplicate animation pivot " + t.name, this);
                    continue;
                }
                pivotLookup.Add(key, t);
                startPosition.Add(key, t.localPosition);
                startRotation.Add(key, t.localRotation);
            }
            if (pivotLookup.Count != 50)
                Debug.LogWarning("[Level80VisualRig] Found " + pivotLookup.Count + "/50 animation parent groups; import all 68 modular FBXs.", this);
        }

        /// <summary>Actual plant output, not a laser-enable setpoint or a claimed safe state.</summary>
        public void SetActualLaserState(float actualLaserOutput01, float actualTemperatureStage01,
            float actualShutterClosed01, float actualAttenuatorTransmission01,
            bool actualLaserEmissionAuthorized, bool actualShutterIsOpen)
        {
            actualLaunchPower01 = Mathf.Clamp01(actualLaserOutput01);
            laserTemperatureStage01 = Mathf.Clamp01(actualTemperatureStage01);
            shutterClosed01 = Mathf.Clamp01(actualShutterClosed01);
            attenuatorActual01 = Mathf.Clamp01(actualAttenuatorTransmission01);
            actualLaserAuthorized = actualLaserEmissionAuthorized;
            actualShutterOpen = actualShutterIsOpen;
        }

        /// <summary>Use indices 0..3 for Logic, Memory, I/O and Sensor ring channels.</summary>
        public void SetActualRingState(int channel, float measuredRingTemperature01,
            float measuredNormalizedDetune, bool measuredSpectralLock)
        {
            if (channel < 0 || channel >= 4) return;
            actualRingTemperature01[channel] = Mathf.Clamp01(measuredRingTemperature01);
            actualRingDetune01[channel] = Mathf.Clamp(measuredNormalizedDetune, -1f, 1f);
            actualRingLocked[channel] = measuredSpectralLock;
        }

        public void SetActualLinkMeasurements(float measuredReceiverPower01,
            float actualEyeOpening01, float actualBitErrorQuality01,
            float actualFourChannelQualification01, float measuredThermalCrosstalk01,
            bool trustedPowerFeedbackValid)
        {
            actualRxPower01 = Mathf.Clamp01(measuredReceiverPower01);
            this.actualEyeOpening01 = Mathf.Clamp01(actualEyeOpening01);
            actualBERQuality01 = Mathf.Clamp01(actualBitErrorQuality01);
            actualLinkLock01 = Mathf.Clamp01(actualFourChannelQualification01);
            actualThermalCrosstalk01 = Mathf.Clamp01(measuredThermalCrosstalk01);
            trustedReceiverPowerValid = trustedPowerFeedbackValid;
        }

        public void SetActualDataStream(int channel, float measuredActivity01)
        {
            if (channel >= 0 && channel < 4) actualTraffic01[channel] = Mathf.Clamp01(measuredActivity01);
        }

        public void SetActualMechanisms(float actualDrawer01, float actualFiberDock01,
            float actualCouplerArm01, float actualReferenceCouponSlide01,
            float actualTestFixtureLidOpen01, float actualCoolingFanRPM)
        {
            laserDrawer01 = Mathf.Clamp01(actualDrawer01);
            fiberDock01 = Mathf.Clamp01(actualFiberDock01);
            couplerArm01 = Mathf.Clamp01(actualCouplerArm01);
            referenceStage01 = Mathf.Clamp01(actualReferenceCouponSlide01);
            testFixtureLid01 = Mathf.Clamp01(actualTestFixtureLidOpen01);
            coolingFanActualRPM = Mathf.Max(0f, actualCoolingFanRPM);
        }

        public void SetActualDiagnosticPose(float actualSpectrumScan01, float actualPowerProbe01,
            float actualDropPortSlide01, float actualRxProbe01, float actualMicroscope01,
            float actualFiberBendProbe01, float actualFaultArm01)
        {
            spectrumScan01 = Mathf.Clamp01(actualSpectrumScan01);
            powerProbe01 = Mathf.Clamp01(actualPowerProbe01);
            dropPortPosition01 = Mathf.Clamp01(actualDropPortSlide01);
            rxProbe01 = Mathf.Clamp01(actualRxProbe01);
            microscope01 = Mathf.Clamp01(actualMicroscope01);
            fiberBendProbe01 = Mathf.Clamp01(actualFiberBendProbe01);
            faultInjectionArm01 = Mathf.Clamp01(actualFaultArm01);
        }

        public void SetActualSafety(bool actualCriticalFaultLatched,
            bool trustedReceiverPowerMeasurementValid, float actualEmergencyShutterClosed01)
        {
            actualCriticalFault = actualCriticalFaultLatched;
            trustedReceiverPowerValid = trustedReceiverPowerMeasurementValid;
            emergencyShutterClosed01 = Mathf.Clamp01(actualEmergencyShutterClosed01);
        }

        public void BindEffects(Light launch,Light wdm,Light receiver,Light alarm)
        {
            fittedLights=new[]{launch,wdm,receiver,alarm};
            foreach(var lamp in fittedLights)lamp.enabled=false;
        }
        private Light[] fittedLights;

        public void Apply(ProjectSpark.Gameplay.SparkLevel80Plant p,bool fresh,bool permitted,bool send,bool fault,float scan,bool armed)
        {
            SetActualLaserState(p.Launch,p.SourceTemperature,p.MainClosed,1-p.BendLoss,permitted,p.MainClosed<.05f);
            SetActualMechanisms(p.Launch,.9f*p.Coupling,p.Coupling,scan/1.05f,0,p.SourceTemperature*720);
            SetActualDiagnosticPose(scan/1.05f,scan/1.05f,p.AverageRx,scan/1.05f,0,p.BendLoss,armed?1:0);
            float eye=0,ber=0;
            for(int i=0;i<4;i++)
            {
                SetActualRingState(i,p.Temperature[i],p.Detune[i],p.Locked[i]);
                SetActualDataStream(i,send?p.Clock[i]*p.Eye[i]:0);
                eye+=p.Eye[i];ber+=p.FinalBer(i);
            }
            SetActualLinkMeasurements(fresh?p.AverageRx:0,eye*.25f,Mathf.Clamp01(1-ber*.25f),
                p.ClocksReady?1:p.AllLocked?.65f:0,p.CrossTalk,fresh);
            SetActualSafety(fault,fresh,p.EmergencyClosed);
            if(fittedLights==null)return;
            fittedLights[0].enabled=p.Launch>.08f&&p.MainClosed<.8f;
            fittedLights[0].intensity=p.Launch*.95f;
            fittedLights[1].enabled=p.ThroughPower>.12f;
            fittedLights[1].intensity=p.ThroughPower;
            fittedLights[2].enabled=fresh&&p.ClocksReady;
            fittedLights[3].enabled=fault;
        }

        private void Move(string key, Vector3 displacement)
        {
            if (pivotLookup.TryGetValue(key, out Transform t))
                t.localPosition = startPosition[key] + (art!=null?t.parent.InverseTransformVector(art.TransformVector(new Vector3(-displacement.x,displacement.y,-displacement.z)*.096f)):displacement);
        }
        private void Rotate(string key, Vector3 euler)
        {
            if (pivotLookup.TryGetValue(key, out Transform t))
                t.localRotation = startRotation[key] * Quaternion.Euler(euler);
        }
        private static void SetIndicator(Light light, bool on, float intensity = 1f)
        {
            if (light == null) return;
            light.enabled = on;
            light.intensity = Mathf.Max(0f, intensity);
        }
        private void Update()
        {
            // True mechanical pivot parents. Indicator-only parents deliberately remain stationary.
            Move("ANIM_LASER_SOURCE_DRAWER", Vector3.forward * (.28f * laserDrawer01));
            Move("ANIM_LASER_TEMPERATURE_STAGE", Vector3.up * (.08f * laserTemperatureStage01));
            Move("ANIM_SAFETY_SHUTTER", Vector3.up * (.19f * (1f - shutterClosed01)));
            Rotate("ANIM_OPTICAL_ISOLATOR", Vector3.forward * isolatorAngleDeg);
            Rotate("ANIM_TAP_SPLITTER", Vector3.up * splitterAngleDeg);
            Rotate("ANIM_POLARIZATION_CONTROLLER", Vector3.right * polarizationAngleDeg);
            Move("ANIM_OPTICAL_ATTENUATOR", Vector3.right * (.16f * (1f - attenuatorActual01)));
            Move("ANIM_SPECTRAL_REFERENCE_SCAN", Vector3.right * (.24f * spectrumScan01));
            Move("ANIM_LAUNCH_POWER_PROBE", Vector3.down * (.13f * powerProbe01));
            Move("ANIM_FIBER_DOCK", Vector3.right * (.18f * fiberDock01));
            Move("ANIM_COUPLER_ARM", Vector3.left * (.15f * couplerArm01));
            Move("ANIM_DROP_PORT_PANEL", Vector3.right * (.15f * dropPortPosition01));
            Move("ANIM_RX_POWER_MONITOR", Vector3.down * (.15f * rxProbe01));
            Move("ANIM_FIBER_BEND_SENSOR", Vector3.up * (.12f * fiberBendProbe01));
            Move("ANIM_FAULT_INJECTION_ARM", Vector3.right * (.18f * faultInjectionArm01));
            Move("ANIM_WAVEGUIDE_MICROSCOPE", Vector3.down * (.15f * microscope01));
            Move("ANIM_REFERENCE_COUPON_STAGE", Vector3.right * (.22f * referenceStage01));
            Rotate("ANIM_TEST_FIXTURE_LID", Vector3.right * (40f * testFixtureLid01));
            Move("ANIM_SAFETY_EMERGENCY_SHUTTER", Vector3.up * (.18f * (1f - emergencyShutterClosed01)));
            coolingFanDegrees = Mathf.Repeat(coolingFanDegrees + coolingFanActualRPM * 6f * Time.deltaTime, 360f);
            Rotate("ANIM_COOLING_FAN_BANK", Vector3.forward * coolingFanDegrees);
            SetIndicator(protectedLaserLight, actualLaserAuthorized && actualShutterOpen && !actualCriticalFault, actualLaunchPower01 * 3f);
            SetIndicator(channelALight, actualRingLocked[0] && !actualCriticalFault);
            SetIndicator(channelBLight, actualRingLocked[1] && !actualCriticalFault);
            SetIndicator(channelCLight, actualRingLocked[2] && !actualCriticalFault);
            SetIndicator(channelDLight, actualRingLocked[3] && !actualCriticalFault);
            SetIndicator(criticalFaultLight, actualCriticalFault, 3f);
            SetIndicator(feedbackHealthyLight, trustedReceiverPowerValid && !actualCriticalFault);
            if (visibleFalseColorOpticalPaths != null)
            {
                bool visible = actualLaserAuthorized && actualShutterOpen && !actualCriticalFault && actualLaunchPower01 > .01f;
                if (visible && !visibleFalseColorOpticalPaths.isPlaying) visibleFalseColorOpticalPaths.Play();
                else if (!visible && visibleFalseColorOpticalPaths.isPlaying)
                    visibleFalseColorOpticalPaths.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}

