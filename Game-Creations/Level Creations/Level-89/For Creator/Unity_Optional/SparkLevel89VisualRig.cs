using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Level 89 read-only art rig. Simulation, wiring, dosimetry, safety and objective authority are external.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel89VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light greenQualifiedLight;
        [SerializeField] private Light amberStormWarningLight;
        [SerializeField] private Light redCriticalFaultLight;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>();
        private readonly Dictionary<string,Vector3> homePos=new Dictionary<string,Vector3>();
        private readonly Dictionary<string,Quaternion> homeRot=new Dictionary<string,Quaternion>();
        private readonly float[] actualShieldPosition01 = new float[4];
        private readonly float[] actualServoPosition01 = new float[4];
        private readonly float[] actualWaterLevel01 = new float[4];
        private float actualDoorClosed01,actualLocksEngaged01,actualPumpA01,actualPumpB01,actualAirFan01,actualSurveyAzimuth;
        private float measuredStormIntensity01,measuredShieldCoverage01,measuredShelterDoseVisual01;
        private bool trustedW149Valid,criticalFaultLatched,shelterQualified;
        private float pumpAngleA,pumpAngleB,fanAngle;
        private void Awake(){
            if(artRoot==null) artRoot=transform.Find("L89_HD_VISUAL_ART");
            if(artRoot==null) artRoot=transform;
            BindVisuals();
        }
        public void BindVisuals(){
            pivots.Clear();homePos.Clear();homeRot.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true)) {
                if(t.name.StartsWith("ANIM_")&&!pivots.ContainsKey(t.name)) {
                    pivots.Add(t.name,t);homePos.Add(t.name,t.localPosition);homeRot.Add(t.name,t.localRotation);
                }
            }
        }
        private void Rotate(string key,Vector3 axis,float degrees){
            Transform t;Quaternion q;
            if(pivots.TryGetValue(key,out t)&&homeRot.TryGetValue(key,out q))t.localRotation=q*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string key,Vector3 offset){
            Transform t;Vector3 p;
            if(pivots.TryGetValue(key,out t)&&homePos.TryGetValue(key,out p))t.localPosition=p+offset;
        }
        public void SetActualShieldMechanisms(float actualTopClosed01,float actualBottomClosed01,
            float actualPortClosed01,float actualStarboardClosed01,float actualLockEngagement01,bool actualDoorClosed){
            actualShieldPosition01[0]=Mathf.Clamp01(actualTopClosed01);
            actualShieldPosition01[1]=Mathf.Clamp01(actualBottomClosed01);
            actualShieldPosition01[2]=Mathf.Clamp01(actualPortClosed01);
            actualShieldPosition01[3]=Mathf.Clamp01(actualStarboardClosed01);
            actualLocksEngaged01=Mathf.Clamp01(actualLockEngagement01);
            actualDoorClosed01=actualDoorClosed?1f:0f;
        }
        public void SetActualWaterAndPumps(float actualPortA01,float actualPortB01,
            float actualStarboardA01,float actualStarboardB01,float actualPumpASpeed01,float actualPumpBSpeed01){
            actualWaterLevel01[0]=Mathf.Clamp01(actualPortA01);
            actualWaterLevel01[1]=Mathf.Clamp01(actualPortB01);
            actualWaterLevel01[2]=Mathf.Clamp01(actualStarboardA01);
            actualWaterLevel01[3]=Mathf.Clamp01(actualStarboardB01);
            actualPumpA01=Mathf.Clamp01(actualPumpASpeed01);actualPumpB01=Mathf.Clamp01(actualPumpBSpeed01);
        }
        public void SetActualSubsystemMotion(float actualServoA01,float actualServoB01,
            float actualServoC01,float actualServoD01,float actualVentilationFan01,float actualSurveyAzimuthDeg){
            actualServoPosition01[0]=Mathf.Clamp01(actualServoA01);
            actualServoPosition01[1]=Mathf.Clamp01(actualServoB01);
            actualServoPosition01[2]=Mathf.Clamp01(actualServoC01);
            actualServoPosition01[3]=Mathf.Clamp01(actualServoD01);
            actualAirFan01=Mathf.Clamp01(actualVentilationFan01);actualSurveyAzimuth=actualSurveyAzimuthDeg;
        }
        public void SetActualRadiationAndSafety(float measuredActualStormSeverity01,float measuredShieldCoverage01,
            float measuredShelterDoseVisual01,bool trustedW149FeedbackFreshAndValid,
            bool actualCriticalFaultLatched,bool actualShelterMissionQualified){
            measuredStormIntensity01=Mathf.Clamp01(measuredActualStormSeverity01);
            this.measuredShieldCoverage01=Mathf.Clamp01(measuredShieldCoverage01);
            this.measuredShelterDoseVisual01=Mathf.Clamp01(measuredShelterDoseVisual01);
            trustedW149Valid=trustedW149FeedbackFreshAndValid;
            criticalFaultLatched=actualCriticalFaultLatched;shelterQualified=actualShelterMissionQualified;
        }
        private void LateUpdate(){
            string[] cas={"ANIM_SHIELD_CASSETTE_TOP","ANIM_SHIELD_CASSETTE_BOTTOM","ANIM_SHIELD_CASSETTE_PORT","ANIM_SHIELD_CASSETTE_STARBOARD"};
            string[] srv={"ANIM_SHIELD_ACTUATOR_A","ANIM_SHIELD_ACTUATOR_B","ANIM_SHIELD_ACTUATOR_C","ANIM_SHIELD_ACTUATOR_D"};
            for(int i=0;i<4;i++) {
                Vector3 direction=i==0?Vector3.down:i==1?Vector3.up:i==2?Vector3.right:Vector3.left;
                Move(cas[i],direction*(1f-actualShieldPosition01[i])*.42f);
                Move(srv[i],Vector3.right*(actualServoPosition01[i]*.24f));
            }
            Move("ANIM_SHELTER_MAIN_SHIELD_DOOR",Vector3.down*(1f-actualDoorClosed01)*1.25f);
            Move("ANIM_LOCKING_PIN_A",Vector3.forward*actualLocksEngaged01*.20f);
            Move("ANIM_LOCKING_PIN_B",Vector3.forward*actualLocksEngaged01*.20f);
            pumpAngleA=Mathf.Repeat(pumpAngleA+Time.deltaTime*actualPumpA01*400f,360);
            pumpAngleB=Mathf.Repeat(pumpAngleB+Time.deltaTime*actualPumpB01*400f,360);
            fanAngle=Mathf.Repeat(fanAngle+Time.deltaTime*actualAirFan01*580f,360);
            Rotate("ANIM_WATER_PUMP_A_ROTOR",Vector3.forward,pumpAngleA);
            Rotate("ANIM_WATER_PUMP_B_ROTOR",Vector3.forward,pumpAngleB);
            Rotate("ANIM_SHELTER_AIR_BLOWER",Vector3.forward,fanAngle);
            Rotate("ANIM_SURVEY_ANTENNA_AZIMUTH",Vector3.up,actualSurveyAzimuth);
            if(greenQualifiedLight!=null)greenQualifiedLight.enabled=shelterQualified&&!criticalFaultLatched;
            if(amberStormWarningLight!=null)amberStormWarningLight.enabled=measuredStormIntensity01>.50f;
            if(redCriticalFaultLight!=null)redCriticalFaultLight.enabled=criticalFaultLatched||!trustedW149Valid;
        }
    }
}
