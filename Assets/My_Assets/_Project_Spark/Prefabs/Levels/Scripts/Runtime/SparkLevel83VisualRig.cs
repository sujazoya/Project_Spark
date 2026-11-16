using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// LEVEL 83 ART-ONLY ANIMATION RIG.
    /// Consumes actual simulated EPS/orbital/mission/safety state. It does NOT model
    /// PV power, battery energy, bus voltage, SparkTerminal connections or objectives.
    /// All 56 ANIM_ pivots are part of the imported Level 83 FBX hierarchy.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel83VisualRig : MonoBehaviour
    {
        [Header("Optional actual-state visual effects")]
        [SerializeField] private Light solarIlluminationLight;
        [SerializeField] private Light batteryChargeLight;
        [SerializeField] private Light dcBusHealthyLight;
        [SerializeField] private Light eclipseLight;
        [SerializeField] private Light feedbackFaultLight;
        [SerializeField] private Light trustedFeedbackHealthyLight;
        [SerializeField] private ParticleSystem energyFlowParticles;

        private readonly Dictionary<string,Transform> pivots = new Dictionary<string,Transform>();
        private readonly Dictionary<string,Vector3> homePosition = new Dictionary<string,Vector3>();
        private readonly Dictionary<string,Quaternion> homeRotation = new Dictionary<string,Quaternion>();
        private float orbitAngle, sunlight, eclipse;
        private float portDeploy, starboardDeploy, portAzimuth, portElevation, starboardAzimuth, starboardElevation;
        private float portMPPT, starboardMPPT, busVoltageQuality, packASOC, packBSOC, busTransient;
        private float batteryTrayA, batteryTrayB, balancing, actualContactor, supercapActivity;
        private float essentialLoad, attitudeLoad, opticalLoad, scienceLoad, thermalLoad;
        private float jamFault, imbalanceFault, opticalBurstFault, certification;
        private bool trustedW104, criticalFault, powerSafeMode, sourceSolarAvailable;
        private bool actualBusQualified, actualChargePermitted;

        private void Awake()
        {
            foreach(Transform t in GetComponentsInChildren<Transform>(true))
            {
                if(!t.name.StartsWith("ANIM_",StringComparison.Ordinal)) continue;
                if(pivots.ContainsKey(t.name))
                {
                    Debug.LogWarning("[Level83VisualRig] Duplicate named pivot: "+t.name,this);
                    continue;
                }
                pivots[t.name]=t;
                homePosition[t.name]=t.localPosition;
                homeRotation[t.name]=t.localRotation;
            }
            if(pivots.Count!=56)
                Debug.LogWarning("[Level83VisualRig] Found "+pivots.Count+"/56 ANIM_ pivot parents. Attach to L83_HD_VISUAL_ART with 74 imported FBXs.",this);
        }

        /// <summary>Actual orbit simulator outputs; eclipse/sunlight are not inferred from art.</summary>
        public void SetActualOrbitalState(float actualOrbitAngleDegrees,float actualSunlightFraction01,float actualEclipseFraction01)
        {
            orbitAngle=actualOrbitAngleDegrees;
            sunlight=Mathf.Clamp01(actualSunlightFraction01);
            eclipse=Mathf.Clamp01(actualEclipseFraction01);
        }

        /// <summary>Actuator measured wing deployment and gimbal angles, NOT requested setpoints.</summary>
        public void SetActualSolarArrays(float actualPortDeploy01,float actualStarboardDeploy01,
            float actualPortAzimuthDegrees,float actualPortElevationDegrees,
            float actualStarboardAzimuthDegrees,float actualStarboardElevationDegrees,
            bool actualSolarInputAvailable)
        {
            portDeploy=Mathf.Clamp01(actualPortDeploy01);
            starboardDeploy=Mathf.Clamp01(actualStarboardDeploy01);
            portAzimuth=actualPortAzimuthDegrees;portElevation=actualPortElevationDegrees;
            starboardAzimuth=actualStarboardAzimuthDegrees;starboardElevation=actualStarboardElevationDegrees;
            sourceSolarAvailable=actualSolarInputAvailable;
        }

        /// <summary>True measured/qualified PV and EPS state normalized by gameplay adapter, not physics here.</summary>
        public void SetActualPowerMeasurements(float actualPortMPPTPerformance01,float actualStarboardMPPTPerformance01,
            float measuredBusVoltageQuality01,float actualPackASOC01,float actualPackBSOC01,
            float actualBusTransientActivity01,bool measuredBusQualified,bool actualChargeEnabled)
        {
            portMPPT=Mathf.Clamp01(actualPortMPPTPerformance01);
            starboardMPPT=Mathf.Clamp01(actualStarboardMPPTPerformance01);
            busVoltageQuality=Mathf.Clamp01(measuredBusVoltageQuality01);
            packASOC=Mathf.Clamp01(actualPackASOC01);packBSOC=Mathf.Clamp01(actualPackBSOC01);
            busTransient=Mathf.Clamp01(actualBusTransientActivity01);
            actualBusQualified=measuredBusQualified;actualChargePermitted=actualChargeEnabled;
        }

        /// <summary>Actual battery mechanism positions and activity, independent of state-of-charge art.</summary>
        public void SetActualBatteryMechanisms(float measuredTrayA01,float measuredTrayB01,float actualBalancing01,
            float actualContactorClosed01,float actualSupercapacitorActivity01)
        {
            batteryTrayA=Mathf.Clamp01(measuredTrayA01);batteryTrayB=Mathf.Clamp01(measuredTrayB01);
            balancing=Mathf.Clamp01(actualBalancing01);actualContactor=Mathf.Clamp01(actualContactorClosed01);
            supercapActivity=Mathf.Clamp01(actualSupercapacitorActivity01);
        }

        /// <summary>Actual energized load fractions for five mission branches.</summary>
        public void SetActualLoadDistribution(float essential01,float attitude01,float optical01,float science01,float thermal01)
        {
            essentialLoad=Mathf.Clamp01(essential01);attitudeLoad=Mathf.Clamp01(attitude01);
            opticalLoad=Mathf.Clamp01(optical01);scienceLoad=Mathf.Clamp01(science01);
            thermalLoad=Mathf.Clamp01(thermal01);
        }

        /// <summary>Only the gameplay simulation decides W104 validity, faults, safety and certification.</summary>
        public void SetActualSafetyAndFaults(bool actualW104FeedbackValid,bool actualCriticalFaultLatched,
            bool actualPowerSafeMode,float actualGimbalJam01,float actualCellImbalance01,
            float actualOpticalBurst01,float actualCertificationProgress01)
        {
            trustedW104=actualW104FeedbackValid;criticalFault=actualCriticalFaultLatched;
            powerSafeMode=actualPowerSafeMode;
            jamFault=Mathf.Clamp01(actualGimbalJam01);imbalanceFault=Mathf.Clamp01(actualCellImbalance01);
            opticalBurstFault=Mathf.Clamp01(actualOpticalBurst01);
            certification=Mathf.Clamp01(actualCertificationProgress01);
        }

        private void SetRotate(string name,Vector3 degrees)
        {
            Transform t;Quaternion h;
            if(!pivots.TryGetValue(name,out t)||!homeRotation.TryGetValue(name,out h))return;
            t.localRotation=h*Quaternion.Euler(degrees);
        }
        private void SetMove(string name,Vector3 localDelta)
        {
            Transform t;Vector3 h;
            if(!pivots.TryGetValue(name,out t)||!homePosition.TryGetValue(name,out h))return;
            t.localPosition=h+localDelta;
        }
        private static void LightOn(Light l,bool state,float intensity=1f)
        {
            if(l!=null){l.enabled=state;if(state)l.intensity=intensity;}
        }
        private void LateUpdate()
        {
            // Orbital holograms and solar mechanism positions.
            SetRotate("ANIM_ORBIT_EARTH_TURN",new Vector3(0f,orbitAngle*.12f,0f));
            SetRotate("ANIM_ORBIT_TIME_MARKER",new Vector3(0f,orbitAngle,0f));
            SetMove("ANIM_ECLIPSE_OCCLUDER",new Vector3((eclipse-.5f)*1.15f,0f,0f));
            SetMove("ANIM_SHADOW_BOUNDARY",new Vector3((eclipse-.5f)*1.25f,0f,0f));
            SetRotate("ANIM_PORT_WING_DEPLOY",new Vector3(0f,0f,-(1f-portDeploy)*72f));
            SetRotate("ANIM_STARBOARD_WING_DEPLOY",new Vector3(0f,0f,(1f-starboardDeploy)*72f));
            SetRotate("ANIM_PORT_GIMBAL_AZIMUTH",new Vector3(0f,portAzimuth,0f));
            SetRotate("ANIM_STARBOARD_GIMBAL_AZIMUTH",new Vector3(0f,starboardAzimuth,0f));
            SetRotate("ANIM_PORT_GIMBAL_ELEVATION",new Vector3(portElevation,0f,0f));
            SetRotate("ANIM_STARBOARD_GIMBAL_ELEVATION",new Vector3(starboardElevation,0f,0f));

            // Measured plant performance only; knobs do not drive an electrical solver.
            SetRotate("ANIM_MPPT_PORT_CONTROL",new Vector3(0f,0f,portMPPT*200f));
            SetRotate("ANIM_MPPT_STARBOARD_CONTROL",new Vector3(0f,0f,starboardMPPT*200f));
            SetMove("ANIM_MPPT_SWEEP_PROBE",new Vector3(portMPPT*.32f,0f,0f));
            SetMove("ANIM_MAIN_BUS_PRECHARGE",new Vector3(actualContactor*.26f,0f,0f));
            SetMove("ANIM_BATTERY_PACK_A_TRAY",new Vector3(0f,0f,batteryTrayA*.23f));
            SetMove("ANIM_BATTERY_PACK_B_TRAY",new Vector3(0f,0f,batteryTrayB*.23f));
            SetMove("ANIM_CELL_BALANCE_SCAN",new Vector3((balancing-.5f)*.32f,0f,0f));
            SetMove("ANIM_BATTERY_CONTACTOR",new Vector3(0f,actualContactor*.16f,0f));
            SetMove("ANIM_LOAD_PRIORITY_SWITCHBANK",new Vector3((scienceLoad+opticalLoad)*.12f,0f,0f));
            SetMove("ANIM_GIMBAL_JAM_TEST",new Vector3(jamFault*.24f,0f,0f));
            SetMove("ANIM_CELL_IMBALANCE_TEST",new Vector3(imbalanceFault*.24f,0f,0f));
            SetMove("ANIM_BURST_LOAD_TEST",new Vector3(opticalBurstFault*.24f,0f,0f));
            SetRotate("ANIM_POWER_SAFE_MODE_SWITCH",new Vector3(0f,0f,powerSafeMode?65f:0f));
            LightOn(solarIlluminationLight,sourceSolarAvailable&&sunlight>.02f,1.2f*sunlight);
            LightOn(eclipseLight,eclipse>.30f,.65f+eclipse);
            LightOn(batteryChargeLight,actualChargePermitted, .65f+.75f*(packASOC+packBSOC)*.5f);
            LightOn(dcBusHealthyLight,actualBusQualified&&trustedW104,.7f+busVoltageQuality);
            LightOn(feedbackFaultLight,criticalFault||!trustedW104,1.25f);
            LightOn(trustedFeedbackHealthyLight,trustedW104&&!criticalFault,1.1f);
            if(energyFlowParticles!=null)
            {
                bool active=(sourceSolarAvailable&&sunlight>.04f)||(essentialLoad+attitudeLoad+opticalLoad+scienceLoad+thermalLoad)>.05f;
                if(active && !energyFlowParticles.isPlaying)energyFlowParticles.Play();
                else if(!active && energyFlowParticles.isPlaying)energyFlowParticles.Stop();
            }
        }
    }
}
