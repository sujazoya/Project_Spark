using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Visual-only Level 74 FBX rig; position and exposure data MUST be supplied by real gameplay simulation.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel74VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform resistSpinChuck, robotBase, robotArm, robotTool;
        [SerializeField] private Transform reticleX, reticleY, waferX, waferY, waferZ, waferTiltX, waferTiltY;
        [SerializeField] private Transform reticleLoader, exposureShutter, lensFocus, naDiaphragm;
        [SerializeField] private Transform bakeLift, postBakeLid, developerSpin, rinseDryer;
        [SerializeField] private Transform interferometerHead, alignmentCamera, cdScanner, overlayScanner;
        [SerializeField] private Light diagnosticBeam;
        [SerializeField] private ParticleSystem optionalVioletDiagnosticVFX;
        private Vector3 rx0,ry0,wx0,wy0,wz0,shutter0,lens0,scan0,overlay0;
        private Quaternion tiltX0,tiltY0,robotBase0,robotArm0,robotTool0;
        private float resistRpm,developerRpm,dryerRpm;
        private Vector2 reticleMetres,waferMetres;
        private float zMetres,tiltXDeg,tiltYDeg,robotBaseDeg,robotArmDeg,robotToolDeg;
        private float shutterFraction,focusMetres,metrologyFraction;
        private bool exposureEnabled,criticalFault;
        private static Quaternion Rot(Transform t) { return t != null ? t.localRotation : Quaternion.identity; }
        private static Vector3 Pos(Transform t) { return t != null ? t.localPosition : Vector3.zero; }
        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                switch(t.name)
                {
                    case "ANIM_RESIST_SPIN_CHUCK": if(resistSpinChuck==null)resistSpinChuck=t;break;
                    case "ANIM_WAFER_ROBOT_BASE": if(robotBase==null)robotBase=t;break;
                    case "ANIM_WAFER_ROBOT_ARM": if(robotArm==null)robotArm=t;break;
                    case "ANIM_WAFER_ROBOT_TOOL": if(robotTool==null)robotTool=t;break;
                    case "ANIM_RETICLE_STAGE_X": if(reticleX==null)reticleX=t;break;
                    case "ANIM_RETICLE_STAGE_Y": if(reticleY==null)reticleY=t;break;
                    case "ANIM_WAFER_STAGE_X": if(waferX==null)waferX=t;break;
                    case "ANIM_WAFER_STAGE_Y": if(waferY==null)waferY=t;break;
                    case "ANIM_WAFER_STAGE_Z": if(waferZ==null)waferZ=t;break;
                    case "ANIM_WAFER_TILT_X": if(waferTiltX==null)waferTiltX=t;break;
                    case "ANIM_WAFER_TILT_Y": if(waferTiltY==null)waferTiltY=t;break;
                    case "ANIM_EXPOSURE_SHUTTER": if(exposureShutter==null)exposureShutter=t;break;
                    case "ANIM_LENS_FOCUS": if(lensFocus==null)lensFocus=t;break;
                    case "ANIM_NA_DIAPHRAGM": if(naDiaphragm==null)naDiaphragm=t;break;
                    case "ANIM_SOFTBAKE_LIFT": if(bakeLift==null)bakeLift=t;break;
                    case "ANIM_POSTBAKE_LID": if(postBakeLid==null)postBakeLid=t;break;
                    case "ANIM_DEVELOPER_SPIN": if(developerSpin==null)developerSpin=t;break;
                    case "ANIM_RINSE_DRYER": if(rinseDryer==null)rinseDryer=t;break;
                    case "ANIM_INTERFEROMETER_HEAD": if(interferometerHead==null)interferometerHead=t;break;
                    case "ANIM_ALIGNMENT_CAMERA": if(alignmentCamera==null)alignmentCamera=t;break;
                    case "ANIM_CD_SCANNER": if(cdScanner==null)cdScanner=t;break;
                    case "ANIM_OVERLAY_METROLOGY": if(overlayScanner==null)overlayScanner=t;break;
                }
            }
            rx0=Pos(reticleX); ry0=Pos(reticleY); wx0=Pos(waferX); wy0=Pos(waferY); wz0=Pos(waferZ);
            shutter0=Pos(exposureShutter);lens0=Pos(lensFocus);scan0=Pos(cdScanner);overlay0=Pos(overlayScanner);
            tiltX0=Rot(waferTiltX);tiltY0=Rot(waferTiltY);
            robotBase0=Rot(robotBase);robotArm0=Rot(robotArm);robotTool0=Rot(robotTool);
        }
        /// <summary>Only measured actual RPM should be passed; these are not motor commands.</summary>
        public void SetActualSpindleRPM(float resistSpin, float developerSpinRPM, float dryerRPM)
        { resistRpm=Mathf.Max(0f,resistSpin);developerRpm=Mathf.Max(0f,developerSpinRPM);dryerRpm=Mathf.Max(0f,dryerRPM); }
        /// <summary>Actual stage displacement in Unity meters; never apply simulation nm errors directly to art scaling.</summary>
        public void SetActualStagePose(Vector2 reticleMeters, Vector2 waferMeters, float waferZMetres, float waferTiltXDegrees, float waferTiltYDegrees)
        { reticleMetres=reticleMeters;waferMetres=waferMeters;zMetres=waferZMetres;tiltXDeg=waferTiltXDegrees;tiltYDeg=waferTiltYDegrees; }
        public void SetWaferRobotPose(float baseDegrees, float armDegrees, float toolDegrees)
        { robotBaseDeg=baseDegrees;robotArmDeg=armDegrees;robotToolDeg=toolDegrees; }
        public void SetOpticalVisuals(float shutterOpen01, float focusCorrectionMetres, bool actualExposureAllowed, bool fault)
        { shutterFraction=Mathf.Clamp01(shutterOpen01); focusMetres=focusCorrectionMetres;exposureEnabled=actualExposureAllowed;criticalFault=fault; }
        public void SetMetrologyProgress(float normalized)
        { metrologyFraction=Mathf.Clamp01(normalized); }
        private static void Set(Transform t,Vector3 position){if(t!=null)t.localPosition=position;}
        private static void Spin(Transform t,float rpm,float dt){if(t!=null)t.Rotate(Vector3.up,rpm*6f*dt,Space.Self);}
        private void Update()
        {
            float dt=Time.deltaTime;
            Spin(resistSpinChuck,resistRpm,dt);Spin(developerSpin,developerRpm,dt);Spin(rinseDryer,dryerRpm,dt);
            Set(reticleX,rx0+Vector3.right*reticleMetres.x);Set(reticleY,ry0+Vector3.forward*reticleMetres.y);
            Set(waferX,wx0+Vector3.right*waferMetres.x);Set(waferY,wy0+Vector3.forward*waferMetres.y);
            Set(waferZ,wz0+Vector3.up*zMetres);
            if(waferTiltX!=null)waferTiltX.localRotation=tiltX0*Quaternion.Euler(tiltXDeg,0f,0f);
            if(waferTiltY!=null)waferTiltY.localRotation=tiltY0*Quaternion.Euler(0f,0f,tiltYDeg);
            Set(exposureShutter,shutter0+Vector3.right*(.42f*shutterFraction));
            Set(lensFocus,lens0+Vector3.up*focusMetres);
            if(robotBase!=null)robotBase.localRotation=robotBase0*Quaternion.Euler(0f,robotBaseDeg,0f);
            if(robotArm!=null)robotArm.localRotation=robotArm0*Quaternion.Euler(0f,0f,robotArmDeg);
            if(robotTool!=null)robotTool.localRotation=robotTool0*Quaternion.Euler(0f,0f,robotToolDeg);
            Set(cdScanner,scan0+Vector3.right*(.6f*metrologyFraction));
            Set(overlayScanner,overlay0+Vector3.up*(.14f*metrologyFraction));
            bool beamOn=exposureEnabled && shutterFraction>.9f && !criticalFault;
            if(diagnosticBeam!=null){ diagnosticBeam.enabled=beamOn;diagnosticBeam.color=criticalFault?Color.red:new Color(.64f,.32f,1f); }
            if(optionalVioletDiagnosticVFX!=null)
            {
                if(beamOn && !optionalVioletDiagnosticVFX.isPlaying)optionalVioletDiagnosticVFX.Play();
                else if(!beamOn && optionalVioletDiagnosticVFX.isPlaying)optionalVioletDiagnosticVFX.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
