using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// VISUAL ONLY. The actual seven-axis robot, force-torque, coolant and safety logic must
    /// exist in Project Spark's gameplay. Never use pivot transforms as certification evidence.
    /// Optional script: move seven modular pivot transforms into serial hierarchy at runtime.
    /// </summary>
    public sealed class SparkLevel86VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private bool bindAutomatically = true;
        [SerializeField] private Light contactFeedbackLamp;
        [SerializeField] private Light thermalHealthyLamp;
        [SerializeField] private Light criticalFaultLamp;
        private readonly Dictionary<string,Transform> pivots = new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> homeRotations = new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> homePositions = new Dictionary<string,Vector3>();
        private static readonly string[] JointNames = {
            "ANIM_ROBOT_BASE_ROTATE", "ANIM_ROBOT_SHOULDER_PITCH", "ANIM_ROBOT_UPPERARM_PITCH",
            "ANIM_ROBOT_ELBOW_PITCH", "ANIM_ROBOT_FOREARM_ROLL", "ANIM_ROBOT_WRIST_PITCH",
            "ANIM_ROBOT_WRIST_ROLL"};
        private static readonly Vector3[] JointAxes = {
            Vector3.up, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.right, Vector3.forward, Vector3.right};
        private bool chainBound;
        private float[] angles = new float[7];
        private float railX;
        private float gripperOpen;
        private float coolantValveDegrees;
        private float failedRelease;
        private float replacementLift;
        private bool w129Trusted;
        private bool criticalFault;
        private bool coolingQualified;

        private void Awake()
        {
            if (artRoot == null) artRoot = transform.Find("L86_HD_VISUAL_ART");
            if (artRoot == null) artRoot = transform;
            if (bindAutomatically) BindVisuals();
        }

        public void BindVisuals()
        {
            pivots.Clear();homeRotations.Clear();homePositions.Clear();
            foreach (Transform t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("ANIM_") && !pivots.ContainsKey(t.name)) pivots[t.name]=t;
            }
            if (!chainBound)
            {
                Transform previous = Find("ANIM_LINEAR_RAIL_CARRIAGE");
                Transform robotBase = artRoot.Find("L86_SevenAxisRobotBase");
                if (previous != null && robotBase != null) robotBase.SetParent(previous, true);
                for (int i=0; i<JointNames.Length; i++)
                {
                    Transform node = Find(JointNames[i]);
                    if (node == null) continue;
                    if (previous != null && !node.IsChildOf(previous)) node.SetParent(previous, true);
                    previous=node;
                }
                // Attach the gripper, wrist load cell, tool changer, fastening driver and
                // inspection probe to actual wrist visual hierarchy. Their original
                // world positions are retained on SetParent(true).
                Transform wrist = Find("ANIM_ROBOT_WRIST_ROLL");
                if (wrist != null)
                {
                    string[] toolModules = {
                        "L86_SixAxisWristForceTorqueSensor",
                        "L86_AdaptiveRadiatorHandlingGripper",
                        "L86_AutomaticEndEffectorToolChanger",
                        "L86_PrecisionTorqueFasteningDriver",
                        "L86_VacuumCompatibleInspectionProbe",
                        "L86_DualJawGripperPressureSensorArray" };
                    foreach (string module in toolModules)
                    {
                        Transform obj=artRoot.Find(module);
                        if (obj != null && !obj.IsChildOf(wrist)) obj.SetParent(wrist,true);
                    }
                }
                chainBound=true;
            }
            foreach (var pair in pivots)
            {
                homeRotations[pair.Key]=pair.Value.localRotation;
                homePositions[pair.Key]=pair.Value.localPosition;
            }
        }
        private Transform Find(string n) { Transform t; return pivots.TryGetValue(n,out t)?t:null; }
        private void Rotate(string n,Vector3 axis,float deg)
        {
            Transform t=Find(n);Quaternion q;
            if (t!=null && homeRotations.TryGetValue(n,out q)) t.localRotation=q*Quaternion.AngleAxis(deg,axis);
        }
        private void Translate(string n,Vector3 offset)
        {
            Transform t=Find(n);Vector3 pos;
            if (t!=null && homePositions.TryGetValue(n,out pos)) t.localPosition=pos+offset;
        }
        /// <summary>Set measured ACTUAL joint positions in DEGREES and rail displacement in METRES.</summary>
        public void SetActualRobotJointAngles(float j1,float j2,float j3,float j4,float j5,float j6,float j7,float railMeters)
        {
            angles[0]=j1;angles[1]=j2;angles[2]=j3;angles[3]=j4;angles[4]=j5;angles[5]=j6;angles[6]=j7;
            railX=Mathf.Clamp(railMeters,-2.5f,2.5f);
        }
        /// <summary>Normalized actual jaw opening, panel release, lift and actual coolant valve angle.</summary>
        public void SetActualServicingMechanisms(float jawOpen01,float releasedPanel01,float liftPanel01,float actualCoolantValveDegrees)
        {
            gripperOpen=Mathf.Clamp01(jawOpen01);
            failedRelease=Mathf.Clamp01(releasedPanel01);
            replacementLift=Mathf.Clamp01(liftPanel01);
            coolantValveDegrees=Mathf.Clamp(actualCoolantValveDegrees,0,90);
        }
        public void SetActualSafetyState(bool trustedW129Fresh,bool criticalFaultLatched,bool radiatorCoolingQualified)
        {
            w129Trusted=trustedW129Fresh;criticalFault=criticalFaultLatched;coolingQualified=radiatorCoolingQualified;
        }
        private void LateUpdate()
        {
            if (pivots.Count==0 || !chainBound) return;
            Translate("ANIM_LINEAR_RAIL_CARRIAGE",Vector3.right*railX);
            for(int i=0;i<7;i++) Rotate(JointNames[i],JointAxes[i],angles[i]);
            Translate("ANIM_GRIPPER_JAW_LEFT",Vector3.forward *(.08f*gripperOpen));
            Translate("ANIM_GRIPPER_JAW_RIGHT",Vector3.back *(.08f*gripperOpen));
            Translate("ANIM_FAILED_RADIATOR_RELEASE",Vector3.right*(.25f*failedRelease));
            Translate("ANIM_REPLACEMENT_RADIATOR_LIFT",Vector3.up*(.35f*replacementLift));
            Rotate("ANIM_COOLANT_VALVE",Vector3.right,coolantValveDegrees);
            if (contactFeedbackLamp!=null) contactFeedbackLamp.enabled=w129Trusted && !criticalFault;
            if (thermalHealthyLamp!=null) thermalHealthyLamp.enabled=coolingQualified && !criticalFault;
            if (criticalFaultLamp!=null) criticalFaultLamp.enabled=criticalFault;
        }
    }
}
