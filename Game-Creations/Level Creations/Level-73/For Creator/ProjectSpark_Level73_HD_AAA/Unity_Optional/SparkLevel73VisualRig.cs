using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Level 73 ART-ONLY rig: consumes actual simulated values. No CMP physics, no wires, no objectives.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel73VisualRig : MonoBehaviour
    {
        [Header("Pivot names from Animation_Rigs.csv / FBX parent null groups")]
        [SerializeField] private Transform platen;
        [SerializeField] private Transform pad;
        [SerializeField] private Transform platenServo;
        [SerializeField] private Transform carrierSpindle;
        [SerializeField] private Transform carrierZ;
        [SerializeField] private Transform conditionerArm;
        [SerializeField] private Transform conditionerDisc;
        [SerializeField] private Transform slurryPump;
        [SerializeField] private Transform slurryArm;
        [SerializeField] private Transform slurryNozzle;
        [SerializeField] private Transform waferLift;
        [SerializeField] private Transform robotBase;
        [SerializeField] private Transform robotArm;
        [SerializeField] private Transform robotTool;
        [SerializeField] private Transform cleaningBrush;
        [SerializeField] private Transform spinDryer;
        [SerializeField] private Transform profilometer;
        [SerializeField] private ParticleSystem slurryVFX;
        [SerializeField] private Light processLight;
        private float platenRPM,carrierRPM,conditionerRPM,slurryPumpRPM,brushRPM,dryerRPM;
        private float carrierHeight,armFraction,nozzleHeight,waferLiftFraction;
        private float robotBaseAngle,robotArmAngle,robotToolAngle,inspectionFraction;
        private bool slurryOn, polishOn, faulted;
        private Vector3 homeCarrierZ,homeNozzle,homeLift,homeProfilometer;
        private Quaternion homeCondArm,homeSlurryArm,homeRobotBase,homeRobotArm,homeRobotTool;
        private void Awake()
        {
            AutoBindByPivotName();
            if(carrierZ!=null) homeCarrierZ=carrierZ.localPosition;
            if(slurryNozzle!=null) homeNozzle=slurryNozzle.localPosition;
            if(waferLift!=null) homeLift=waferLift.localPosition;
            if(profilometer!=null) homeProfilometer=profilometer.localPosition;
            homeCondArm=LocalRotation(conditionerArm);
            homeSlurryArm=LocalRotation(slurryArm);
            homeRobotBase=LocalRotation(robotBase);
            homeRobotArm=LocalRotation(robotArm);
            homeRobotTool=LocalRotation(robotTool);
        }
        private static Quaternion LocalRotation(Transform t) => t!=null?t.localRotation:Quaternion.identity;
        private void AutoBindByPivotName()
        {
            var ts=GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                switch(t.name)
                {
                    case "ANIM_PLATEN_ROTATION": if(platen==null)platen=t;break;
                    case "ANIM_PAD_ROTATION": if(pad==null)pad=t;break;
                    case "ANIM_PLATEN_SERVO_ROTOR": if(platenServo==null)platenServo=t;break;
                    case "ANIM_CARRIER_ROTATION": if(carrierSpindle==null)carrierSpindle=t;break;
                    case "ANIM_CARRIER_Z": if(carrierZ==null)carrierZ=t;break;
                    case "ANIM_CONDITIONER_ARM": if(conditionerArm==null)conditionerArm=t;break;
                    case "ANIM_CONDITIONER_DISC": if(conditionerDisc==null)conditionerDisc=t;break;
                    case "ANIM_SLURRY_PUMP": if(slurryPump==null)slurryPump=t;break;
                    case "ANIM_SLURRY_ARM": if(slurryArm==null)slurryArm=t;break;
                    case "ANIM_SLURRY_NOZZLE": if(slurryNozzle==null)slurryNozzle=t;break;
                    case "ANIM_WAFER_LIFT": if(waferLift==null)waferLift=t;break;
                    case "ANIM_WAFER_ROBOT_BASE": if(robotBase==null)robotBase=t;break;
                    case "ANIM_WAFER_ROBOT_ARM": if(robotArm==null)robotArm=t;break;
                    case "ANIM_WAFER_ROBOT_TOOL": if(robotTool==null)robotTool=t;break;
                    case "ANIM_CLEANING_BRUSH": if(cleaningBrush==null)cleaningBrush=t;break;
                    case "ANIM_SPIN_DRYER": if(spinDryer==null)spinDryer=t;break;
                    case "ANIM_PROFILOMETER": if(profilometer==null)profilometer=t;break;
                }
            }
        }
        /// <summary>RPM values are measured/derived by external simulation; not commands.</summary>
        public void SetActualRPM(float platenRpm,float carrierRpm,float conditionerRpm,float slurryPumpRpm,float brushRpm,float dryerRpm)
        {
            platenRPM=Mathf.Max(0,platenRpm);carrierRPM=Mathf.Max(0,carrierRpm);
            conditionerRPM=Mathf.Max(0,conditionerRpm);slurryPumpRPM=Mathf.Max(0,slurryPumpRpm);
            brushRPM=Mathf.Max(0,brushRpm);dryerRPM=Mathf.Max(0,dryerRpm);
        }
        /// <summary>Positions in normalized 0..1 range. Actual force state is owned externally.</summary>
        public void SetActualActuatorPositions(float carrierZFraction,float conditionerArmFraction,float slurryArmFraction,float nozzleFraction,float waferLiftFraction)
        {
            carrierHeight=Mathf.Clamp01(carrierZFraction);armFraction=Mathf.Clamp01(conditionerArmFraction);
            nozzleHeight=Mathf.Clamp01(nozzleFraction);waferLiftFraction=Mathf.Clamp01(waferLiftFraction);
            this.slurryArmFraction=Mathf.Clamp01(slurryArmFraction);
        }
        private float slurryArmFraction;
        public void SetWaferRobotPose(float baseDeg,float armDeg,float toolDeg)
        {robotBaseAngle=baseDeg;robotArmAngle=armDeg;robotToolAngle=toolDeg;}
        public void SetProcessVisualState(bool measuredSlurryFlowPresent,bool actualPolishingContact,bool criticalFault,float scanFraction)
        {
            slurryOn=measuredSlurryFlowPresent;polishOn=actualPolishingContact;faulted=criticalFault;inspectionFraction=Mathf.Clamp01(scanFraction);
        }
        private static void SpinY(Transform t,float rpm,float dt){if(t!=null)t.Rotate(Vector3.up,rpm*6f*dt,Space.Self);}
        private void Update()
        {
            float dt=Time.deltaTime;
            SpinY(platen,platenRPM,dt);SpinY(pad,platenRPM,dt);SpinY(platenServo,platenRPM,dt);
            SpinY(carrierSpindle,carrierRPM,dt);SpinY(conditionerDisc,conditionerRPM,dt);SpinY(slurryPump,slurryPumpRPM,dt);
            SpinY(cleaningBrush,brushRPM,dt);SpinY(spinDryer,dryerRPM,dt);
            if(carrierZ!=null)carrierZ.localPosition=homeCarrierZ+Vector3.up*(-.22f*carrierHeight);
            if(conditionerArm!=null)conditionerArm.localRotation=homeCondArm*Quaternion.Euler(0f,45f*armFraction,0f);
            if(slurryArm!=null)slurryArm.localRotation=homeSlurryArm*Quaternion.Euler(0f,35f*slurryArmFraction,0f);
            if(slurryNozzle!=null)slurryNozzle.localPosition=homeNozzle+Vector3.up*(-.16f*nozzleHeight);
            if(waferLift!=null)waferLift.localPosition=homeLift+Vector3.up*(.17f*waferLiftFraction);
            if(robotBase!=null)robotBase.localRotation=homeRobotBase*Quaternion.Euler(0f,robotBaseAngle,0f);
            if(robotArm!=null)robotArm.localRotation=homeRobotArm*Quaternion.Euler(0f,0f,robotArmAngle);
            if(robotTool!=null)robotTool.localRotation=homeRobotTool*Quaternion.Euler(0f,0f,robotToolAngle);
            if(profilometer!=null)profilometer.localPosition=homeProfilometer+Vector3.right*(.7f*inspectionFraction);
            if(slurryVFX!=null)
            {if(slurryOn && !slurryVFX.isPlaying)slurryVFX.Play();else if(!slurryOn && slurryVFX.isPlaying)slurryVFX.Stop(true,ParticleSystemStopBehavior.StopEmitting);}
            if(processLight!=null){processLight.color=faulted?new Color(1f,.23f,.14f):(polishOn?new Color(.09f,.89f,1f):Color.white);processLight.intensity=polishOn?2f:.45f;}
        }
    }
}
