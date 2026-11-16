using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Visual facade only. No functional navigation, clock, radiation,
    /// electrical, safety or objective logic resides in this component.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel91VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light timingQualifiedLight;
        [SerializeField] private Light timeReferenceFailureLight;
        [SerializeField] private Light missionCompleteLight;
        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Quaternion> homesR = new Dictionary<string, Quaternion>();
        private readonly Dictionary<string, Vector3> homesP = new Dictionary<string, Vector3>();
        private readonly float[] azimuthDeg = new float[4], elevationDeg = new float[4];
        private readonly float[] detectorSignalQuality01 = new float[4];
        private readonly float[] actualSunshadeExtension01 = new float[4];
        private bool trustedW163Valid, faultLatched, missionQualified;
        private float measuredClockQuality01, navigationConfidence01, coolingFanRPM01;
        private float fanAngle;
        private void Awake() {
            if(artRoot==null)artRoot=transform.Find("L91_HD_VISUAL_ART");
            if(artRoot==null)artRoot=transform;
            BindPivots();
        }
        public void BindPivots() {
            pivots.Clear();homesP.Clear();homesR.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true)) {
                if(!t.name.StartsWith("ANIM_")||pivots.ContainsKey(t.name))continue;
                pivots[t.name]=t;homesP[t.name]=t.localPosition;homesR[t.name]=t.localRotation;
            }
        }
        private void Rotate(string id, Vector3 axis, float deg) {
            Transform t;Quaternion home;
            if(pivots.TryGetValue(id,out t) && homesR.TryGetValue(id,out home))
                t.localRotation=home*Quaternion.AngleAxis(deg,axis);
        }
        private void Slide(string id, Vector3 distance) {
            Transform t;Vector3 home;
            if(pivots.TryGetValue(id,out t) && homesP.TryGetValue(id,out home))
                t.localPosition=home+distance;
        }
        /// <summary>Consume actual encoder poses, not commanded poses.</summary>
        public void SetActualTelescopeAngles(float aAz,float aEl,float bAz,float bEl,
            float cAz,float cEl,float dAz,float dEl) {
            azimuthDeg[0]=aAz;elevationDeg[0]=aEl;
            azimuthDeg[1]=bAz;elevationDeg[1]=bEl;
            azimuthDeg[2]=cAz;elevationDeg[2]=cEl;
            azimuthDeg[3]=dAz;elevationDeg[3]=dEl;
        }
        public void SetActualAcquisition(float aQuality01,float bQuality01,float cQuality01,float dQuality01,
            float aShade01,float bShade01,float cShade01,float dShade01) {
            detectorSignalQuality01[0]=Mathf.Clamp01(aQuality01);
            detectorSignalQuality01[1]=Mathf.Clamp01(bQuality01);
            detectorSignalQuality01[2]=Mathf.Clamp01(cQuality01);
            detectorSignalQuality01[3]=Mathf.Clamp01(dQuality01);
            actualSunshadeExtension01[0]=Mathf.Clamp01(aShade01);
            actualSunshadeExtension01[1]=Mathf.Clamp01(bShade01);
            actualSunshadeExtension01[2]=Mathf.Clamp01(cShade01);
            actualSunshadeExtension01[3]=Mathf.Clamp01(dShade01);
        }
        public void SetActualTimingAndNavigation(float measuredClockQuality01,
            float measuredNavigationConfidence01,float measuredCoolingFanNormalizedRPM,
            bool actualTrustedW163Valid,bool actualFaultLatched,bool actualMissionQualified) {
            this.measuredClockQuality01=Mathf.Clamp01(measuredClockQuality01);
            navigationConfidence01=Mathf.Clamp01(measuredNavigationConfidence01);
            coolingFanRPM01=Mathf.Clamp01(measuredCoolingFanNormalizedRPM);
            trustedW163Valid=actualTrustedW163Valid;faultLatched=actualFaultLatched;
            missionQualified=actualMissionQualified;
        }
        private void LateUpdate() {
            for(int i=0;i<4;i++) {
                string index="ABCD".Substring(i,1);
                Rotate("ANIM_XRAY_TELESCOPE_"+index+"_AZIMUTH",Vector3.up,azimuthDeg[i]);
                Rotate("ANIM_XRAY_GIMBAL_"+index+"_ELEVATION",Vector3.right,elevationDeg[i]);
                Slide("ANIM_XRAY_TELESCOPE_"+index+"_SUNSHADE",Vector3.forward*(actualSunshadeExtension01[i]*0.18f));
                Slide("ANIM_PHOTON_DETECTOR_"+index+"_TIMING",Vector3.forward*(detectorSignalQuality01[i]*.022f));
            }
            fanAngle=Mathf.Repeat(fanAngle+Time.deltaTime*coolingFanRPM01*300f,360f);
            Rotate("ANIM_CLOCK_COOLING_FAN",Vector3.forward,fanAngle);
            Slide("ANIM_PRIMARY_CLOCK_LOCK",Vector3.forward*(measuredClockQuality01*.025f));
            Slide("ANIM_NAVIGATIONUNCERTAINTYHOLOGRAM_LIVE",Vector3.forward*(navigationConfidence01*.04f));
            if(timingQualifiedLight!=null)timingQualifiedLight.enabled=trustedW163Valid&&!faultLatched;
            if(timeReferenceFailureLight!=null)timeReferenceFailureLight.enabled=!trustedW163Valid||faultLatched;
            if(missionCompleteLight!=null)missionCompleteLight.enabled=missionQualified&&!faultLatched;
        }
    }
}
