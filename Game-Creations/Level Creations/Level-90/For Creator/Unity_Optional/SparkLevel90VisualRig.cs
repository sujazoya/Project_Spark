using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Read-only visual adapter for original Level 90 FBX pivot groups.
    /// Contains NO circuit, plasma, xenon, flight dynamics, safety, or objective authority.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel90VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light healthyEnginesLight;
        [SerializeField] private Light criticalFaultLight;
        [SerializeField] private Light missionQualificationLight;
        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> homesP = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> homesQ = new Dictionary<string, Quaternion>();
        private readonly float[] gimbalYaw = new float[4], gimbalPitch = new float[4];
        private readonly float[] discharge01 = new float[4];
        private readonly float[] ppuPower01 = new float[4];
        private float solarPort01, solarStarboard01, coolantPump01, actualXenonA01, actualXenonB01;
        private float actualDepartureProgress01, measuredDeltaV01;
        private bool trustedW157FeedbackValid, criticalFaultLatched, missionQualified;
        private float pumpAngle;
        private void Awake() {
            if(artRoot == null)artRoot = transform.Find("L90_HD_VISUAL_ART");
            if(artRoot == null)artRoot = transform;
            BindPivots();
        }
        public void BindPivots() {
            pivots.Clear();homesP.Clear();homesQ.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true)) {
                if(!t.name.StartsWith("ANIM_") || pivots.ContainsKey(t.name))continue;
                pivots[t.name]=t;homesP[t.name]=t.localPosition;homesQ[t.name]=t.localRotation;
            }
        }
        private void Rotate(string name,Vector3 axis,float degrees) {
            Transform t; Quaternion q;
            if(pivots.TryGetValue(name,out t)&&homesQ.TryGetValue(name,out q))t.localRotation=q*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string name,Vector3 localOffset) {
            Transform t;Vector3 v;
            if(pivots.TryGetValue(name,out t)&&homesP.TryGetValue(name,out v))t.localPosition=v+localOffset;
        }
        public void SetActualEngineStates(float actualA01,float actualB01,float actualC01,float actualD01,
            float measuredPPUA01,float measuredPPUB01,float measuredPPUC01,float measuredPPUD01) {
            discharge01[0]=Mathf.Clamp01(actualA01);discharge01[1]=Mathf.Clamp01(actualB01);
            discharge01[2]=Mathf.Clamp01(actualC01);discharge01[3]=Mathf.Clamp01(actualD01);
            ppuPower01[0]=Mathf.Clamp01(measuredPPUA01);ppuPower01[1]=Mathf.Clamp01(measuredPPUB01);
            ppuPower01[2]=Mathf.Clamp01(measuredPPUC01);ppuPower01[3]=Mathf.Clamp01(measuredPPUD01);
        }
        public void SetActualGimbals(float actualAYawDeg,float actualAPitchDeg,float actualBYawDeg,float actualBPitchDeg,
            float actualCYawDeg,float actualCPitchDeg,float actualDYawDeg,float actualDPitchDeg) {
            gimbalYaw[0]=actualAYawDeg;gimbalPitch[0]=actualAPitchDeg;
            gimbalYaw[1]=actualBYawDeg;gimbalPitch[1]=actualBPitchDeg;
            gimbalYaw[2]=actualCYawDeg;gimbalPitch[2]=actualCPitchDeg;
            gimbalYaw[3]=actualDYawDeg;gimbalPitch[3]=actualDPitchDeg;
        }
        public void SetActualPropellantAndMechanisms(float actualTankA01,float actualTankB01,
            float actualPortArrayDeploy01,float actualStarboardArrayDeploy01,float actualCoolantPumpSpeed01) {
            actualXenonA01=Mathf.Clamp01(actualTankA01);actualXenonB01=Mathf.Clamp01(actualTankB01);
            solarPort01=Mathf.Clamp01(actualPortArrayDeploy01);solarStarboard01=Mathf.Clamp01(actualStarboardArrayDeploy01);
            coolantPump01=Mathf.Clamp01(actualCoolantPumpSpeed01);
        }
        public void SetActualMissionAndSafety(float actualDepartureProgress01,float measuredDeltaVNormalized01,
            bool trustedW157FeedbackValid,bool actualCriticalFaultLatched,bool actualMissionQualified) {
            this.actualDepartureProgress01=Mathf.Clamp01(actualDepartureProgress01);
            measuredDeltaV01=Mathf.Clamp01(measuredDeltaVNormalized01);
            this.trustedW157FeedbackValid=trustedW157FeedbackValid;
            criticalFaultLatched=actualCriticalFaultLatched;
            missionQualified=actualMissionQualified;
        }
        private void LateUpdate() {
            const string alphabet="ABCD";
            for(int i=0;i<4;i++) {
                char c=alphabet[i];
                Rotate("ANIM_ENGINE_"+c+"_GIMBAL_YAW",Vector3.up,gimbalYaw[i]);
                Rotate("ANIM_ENGINE_"+c+"_GIMBAL_PITCH",Vector3.right,gimbalPitch[i]);
                // Pivot is an indicator; only shift its decorative glow geometry, never create thrust here.
                Move("ANIM_ENGINE_"+c+"_PLASMA_DISCHARGE",Vector3.forward*(discharge01[i]*.06f));
                Move("ANIM_PPU_"+c+"_OUTPUT_INDICATOR",Vector3.forward*(ppuPower01[i]*.04f));
            }
            Rotate("ANIM_PORT_SOLAR_WING",Vector3.forward,solarPort01*85f);
            Rotate("ANIM_STARBOARD_SOLAR_WING",Vector3.forward,-solarStarboard01*85f);
            pumpAngle=Mathf.Repeat(pumpAngle+Time.deltaTime*coolantPump01*440f,360f);
            Rotate("ANIM_THERMAL_PUMP_ROTOR",Vector3.forward,pumpAngle);
            Move("ANIM_XENON_TANK_A_GAUGE",Vector3.up*(actualXenonA01*.04f));
            Move("ANIM_XENON_TANK_B_GAUGE",Vector3.up*(actualXenonB01*.04f));
            Move("ANIM_TRAJECTORY_VECTOR",Vector3.forward*(actualDepartureProgress01*.06f));
            Move("ANIM_PROPULSION_SCADA",Vector3.forward*(measuredDeltaV01*.035f));
            if(healthyEnginesLight!=null)healthyEnginesLight.enabled=discharge01[0]>.5f||discharge01[1]>.5f||discharge01[2]>.5f||discharge01[3]>.5f;
            if(criticalFaultLight!=null)criticalFaultLight.enabled=criticalFaultLatched||!trustedW157FeedbackValid;
            if(missionQualificationLight!=null)missionQualificationLight.enabled=missionQualified&&!criticalFaultLatched;
        }
    }
}
