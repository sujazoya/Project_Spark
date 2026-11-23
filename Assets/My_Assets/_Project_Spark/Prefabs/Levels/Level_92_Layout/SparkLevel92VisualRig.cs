using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Read-only art animation facade for Level 92. NO physics, electrical, safety,
    /// drill simulation, geophysical scan or objective certification is provided here.
    /// The gameplay implementation must drive this with AUTHORITATIVE measured state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel92VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Light normalOperationLight;
        [SerializeField] private Light criticalW171FailureLight;
        [SerializeField] private Light missionQualifiedLight;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> homeRotations=new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> homePositions=new Dictionary<string,Vector3>();
        private readonly float[] hipAngles=new float[6], kneeAngles=new float[6], footAngles=new float[6];
        private readonly float[] anchorDeployment=new float[6], anchorLatch=new float[6];
        private bool validW171, latchedFault, missionQualified;
        private float actualDrillRPM, actualFeedDepthM, actualWinchRPM, actualCarouselAngleDeg;
        private float actualDrillTorque01, actualAnchorConfidence01;
        private float drillAngle, winchAngle;
        private void Awake()
        {
            if(visualRoot==null) visualRoot=transform.Find("L92_HD_VISUAL_ART");
            if(visualRoot==null)visualRoot=transform;
            RebindArtPivots();
        }
        public void RebindArtPivots()
        {
            pivots.Clear();homeRotations.Clear();homePositions.Clear();
            foreach(Transform t in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                if(!t.name.StartsWith("ANIM_") || pivots.ContainsKey(t.name)) continue;
                pivots[t.name]=t;
                homeRotations[t.name]=t.localRotation;
                homePositions[t.name]=t.localPosition;
            }
        }
        private void Rot(string id,Vector3 axis,float degrees)
        {
            Transform t;Quaternion h;
            if(pivots.TryGetValue(id,out t) && homeRotations.TryGetValue(id,out h))
                t.localRotation=h*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string id,Vector3 displacement)
        {
            Transform t;Vector3 h;
            if(pivots.TryGetValue(id,out t) && homePositions.TryGetValue(id,out h))
                t.localPosition=h+displacement;
        }
        /// <summary>Measured leg poses, not desired servomotor commands.</summary>
        public void SetActualLegPose(int index, float hipDegrees,float kneeDegrees,
            float footDegrees,float deployed01,float latch01)
        {
            if(index<0 || index>=6)return;
            hipAngles[index]=hipDegrees;kneeAngles[index]=kneeDegrees;footAngles[index]=footDegrees;
            anchorDeployment[index]=Mathf.Clamp01(deployed01);
            anchorLatch[index]=Mathf.Clamp01(latch01);
        }
        /// <summary>Measured drill speeds in RPM, actual feed depth in metres.</summary>
        public void SetActualMechanicalState(float drillRPM,float feedDepthM,
            float winchRPM,float carouselDeg,float drillTorqueNormalized,
            float anchorConfidenceNormalized)
        {
            actualDrillRPM=Mathf.Max(0,drillRPM);
            actualFeedDepthM=Mathf.Max(0,feedDepthM);
            actualWinchRPM=winchRPM;actualCarouselAngleDeg=carouselDeg;
            actualDrillTorque01=Mathf.Clamp01(drillTorqueNormalized);
            actualAnchorConfidence01=Mathf.Clamp01(anchorConfidenceNormalized);
        }
        public void SetActualSafetyState(bool trustedW171Valid,bool actualLatchedFault,
            bool actualMissionQualified)
        {
            validW171=trustedW171Valid;latchedFault=actualLatchedFault;
            missionQualified=actualMissionQualified;
        }
        private void LateUpdate()
        {
            for(int i=0;i<6;i++)
            {
                string letter="ABCDEF".Substring(i,1);
                Rot("ANIM_LANDER_LEG_"+letter+"_HIP",Vector3.up,hipAngles[i]);
                Rot("ANIM_LANDER_LEG_"+letter+"_KNEE",Vector3.right,kneeAngles[i]);
                Rot("ANIM_LANDER_LEG_"+letter+"_FOOT_ROTATOR",Vector3.right,footAngles[i]);
                Move("ANIM_ANCHOR_"+letter+"_MICROSPINE_DEPLOY",Vector3.down*anchorDeployment[i]*.14f);
                Rot("ANIM_ANCHOR_"+letter+"_MICROSPINE_LATCH",Vector3.forward,anchorLatch[i]*35f);
            }
            drillAngle=Mathf.Repeat(drillAngle+actualDrillRPM*6f*Time.deltaTime,360f);
            winchAngle=Mathf.Repeat(winchAngle+actualWinchRPM*6f*Time.deltaTime,360f);
            Rot("ANIM_DRILL_SPINDLE",Vector3.up,drillAngle);
            Rot("ANIM_CORE_BIT",Vector3.up,drillAngle);
            Move("ANIM_AXIAL_FEED",Vector3.down*Mathf.Min(1f,actualFeedDepthM));
            Rot("ANIM_TETHER_WINCH_SPOOL",Vector3.right,winchAngle);
            Rot("ANIM_CARTRIDGE_CAROUSEL",Vector3.up,actualCarouselAngleDeg);
            Rot("ANIM_MEASURED_TORQUE_NEEDLE",Vector3.forward,actualDrillTorque01*110f);
            Move("ANIM_ANCHOR_FORCE_DISTRIBUTION",Vector3.forward*actualAnchorConfidence01*.018f);
            if(normalOperationLight!=null)normalOperationLight.enabled=validW171 && !latchedFault;
            if(criticalW171FailureLight!=null)criticalW171FailureLight.enabled=!validW171 || latchedFault;
            if(missionQualifiedLight!=null)missionQualifiedLight.enabled=missionQualified && !latchedFault;
        }
    }
}
