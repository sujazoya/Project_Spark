using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Art-only motion/VFX adapter for Level 75. DO NOT use this for implantation physics,
    /// beam current measurements, circuit connectivity, dose integration or safety decisions.
    /// Setters must receive *actual measured/authorized* states from the game's simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel75VisualRig : MonoBehaviour
    {
        [Header("Mechanical pivots / optional explicit bindings")]
        [SerializeField] private Transform turboRotor, backingRotor, waferRotation, waferTilt;
        [SerializeField] private Transform robotBase, robotArm, robotTool;
        [SerializeField] private Transform blanker, loadLockGate, faradayCup, activationDoor;
        [SerializeField] private Transform beamScanX, beamScanY, profileScan, junctionMetrology;
        [SerializeField] private Transform emergencyStop;
        [Header("Diagnostic VFX (non-physical representation)")]
        [SerializeField] private Light optionalBeamLight;
        [SerializeField] private ParticleSystem optionalBeamDiagnosticVFX;
        [SerializeField] private Light optionalSourceGlow;
        [SerializeField] private Light optionalActivationGlow;

        private Vector3 blanker0, gate0, faraday0, scanX0, scanY0, profile0, depth0;
        private Quaternion tilt0, robotBase0, robotArm0, robotTool0, door0;
        private float turboRpm, backingRpm, waferRpm;
        private float tiltDeg, robotBaseDeg, robotArmDeg, robotToolDeg;
        private float blankerClosed01 = 1f, loadLockClosed01 = 1f, faradayInserted01;
        private float beamTransmission01, implantProgress01, scanU, scanV, activationFraction;
        private float metrologyFraction;
        private bool authorizedBeam, sourceActive, protectionFault;

        private static Vector3 Pos(Transform t) => t != null ? t.localPosition : Vector3.zero;
        private static Quaternion Rot(Transform t) => t != null ? t.localRotation : Quaternion.identity;

        private void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                switch (t.name)
                {
                    case "ANIM_TURBO_ROTOR": if (turboRotor == null) turboRotor = t; break;
                    case "ANIM_BACKING_ROTOR": if (backingRotor == null) backingRotor = t; break;
                    case "ANIM_WAFER_ROTATION": if (waferRotation == null) waferRotation = t; break;
                    case "ANIM_WAFER_TILT": if (waferTilt == null) waferTilt = t; break;
                    case "ANIM_WAFER_ROBOT_BASE": if (robotBase == null) robotBase = t; break;
                    case "ANIM_WAFER_ROBOT_ARM": if (robotArm == null) robotArm = t; break;
                    case "ANIM_WAFER_ROBOT_TOOL": if (robotTool == null) robotTool = t; break;
                    case "ANIM_BEAM_BLANKER": if (blanker == null) blanker = t; break;
                    case "ANIM_LOADLOCK_GATE": if (loadLockGate == null) loadLockGate = t; break;
                    case "ANIM_FARADAY_CUP": if (faradayCup == null) faradayCup = t; break;
                    case "ANIM_ACTIVATION_DOOR": if (activationDoor == null) activationDoor = t; break;
                    case "ANIM_BEAM_SCAN_X": if (beamScanX == null) beamScanX = t; break;
                    case "ANIM_BEAM_SCAN_Y": if (beamScanY == null) beamScanY = t; break;
                    case "ANIM_BEAM_PROFILE_SCAN": if (profileScan == null) profileScan = t; break;
                    case "ANIM_JUNCTION_METROLOGY": if (junctionMetrology == null) junctionMetrology = t; break;
                    case "ANIM_IMPLANT_ESTOP": if (emergencyStop == null) emergencyStop = t; break;
                }
            }
            blanker0=Pos(blanker); gate0=Pos(loadLockGate); faraday0=Pos(faradayCup);
            scanX0=Pos(beamScanX);scanY0=Pos(beamScanY); profile0=Pos(profileScan);depth0=Pos(junctionMetrology);
            tilt0=Rot(waferTilt);robotBase0=Rot(robotBase);robotArm0=Rot(robotArm);
            robotTool0=Rot(robotTool); door0=Rot(activationDoor);
        }

        /// <summary>All RPM values must be actual shaft RPM, not motor setpoints.</summary>
        public void SetActualRPM(float turbo, float backing, float wafer)
        {
            turboRpm=Mathf.Max(0f,turbo);
            backingRpm=Mathf.Max(0f,backing);
            waferRpm=wafer;
        }

        /// <summary>Pose in degrees, provided by authoritative wafer handling / endstation mechanics.</summary>
        public void SetWaferHandlingPose(float tiltDegrees, float robotBaseDegrees, float robotArmDegrees, float robotToolDegrees)
        {
            tiltDeg=tiltDegrees; robotBaseDeg=robotBaseDegrees; robotArmDeg=robotArmDegrees; robotToolDeg=robotToolDegrees;
        }

        /// <summary>Normalized actual actuator position fractions.</summary>
        public void SetActuatorPositions(float blankerClosed, float loadLockClosed, float faradayCupInserted)
        {
            blankerClosed01=Mathf.Clamp01(blankerClosed);
            loadLockClosed01=Mathf.Clamp01(loadLockClosed);
            faradayInserted01=Mathf.Clamp01(faradayCupInserted);
        }

        /// <summary>Actual calibrated beam state; visible beam is a diagnostic exaggeration.</summary>
        public void SetBeamlineVisualState(bool sourceActuallyActive, bool beamDeliveryAuthorized,
            float actualSelectedBeamTransmission01, float scanNormalizedX, float scanNormalizedY, bool criticalFault)
        {
            sourceActive=sourceActuallyActive; authorizedBeam=beamDeliveryAuthorized;
            beamTransmission01=Mathf.Clamp01(actualSelectedBeamTransmission01);
            scanU=Mathf.Clamp(scanNormalizedX,-1f,1f);scanV=Mathf.Clamp(scanNormalizedY,-1f,1f);
            protectionFault=criticalFault;
        }

        public void SetProcessVisualization(float actualImplantProgress01, float actualActivationProgress01, float actualMetrologyProgress01)
        {
            implantProgress01=Mathf.Clamp01(actualImplantProgress01);
            activationFraction=Mathf.Clamp01(actualActivationProgress01);
            metrologyFraction=Mathf.Clamp01(actualMetrologyProgress01);
        }

        private static void Spin(Transform t,float rpm,float dt)
        { if(t!=null) t.Rotate(Vector3.up,rpm*6f*dt,Space.Self); }

        private void Update()
        {
            float dt=Time.deltaTime;
            Spin(turboRotor,turboRpm,dt);Spin(backingRotor,backingRpm,dt);Spin(waferRotation,waferRpm,dt);
            if(waferTilt!=null)waferTilt.localRotation=tilt0*Quaternion.Euler(tiltDeg,0f,0f);
            if(robotBase!=null)robotBase.localRotation=robotBase0*Quaternion.Euler(0f,robotBaseDeg,0f);
            if(robotArm!=null)robotArm.localRotation=robotArm0*Quaternion.Euler(0f,0f,robotArmDeg);
            if(robotTool!=null)robotTool.localRotation=robotTool0*Quaternion.Euler(0f,0f,robotToolDeg);
            if(blanker!=null)blanker.localPosition=blanker0+Vector3.up*(.42f*(1f-blankerClosed01));
            if(loadLockGate!=null)loadLockGate.localPosition=gate0+Vector3.up*(.68f*(1f-loadLockClosed01));
            if(faradayCup!=null)faradayCup.localPosition=faraday0+Vector3.up*(.34f*faradayInserted01);
            if(beamScanX!=null)beamScanX.localPosition=scanX0+Vector3.forward*(scanU*.07f);
            if(beamScanY!=null)beamScanY.localPosition=scanY0+Vector3.up*(scanV*.07f);
            if(profileScan!=null)profileScan.localPosition=profile0+Vector3.up*(metrologyFraction*.23f);
            if(junctionMetrology!=null)junctionMetrology.localPosition=depth0+Vector3.right*(metrologyFraction*.36f);
            if(activationDoor!=null)activationDoor.localRotation=door0*Quaternion.Euler(0f,activationFraction*35f,0f);
            bool beamVisible=sourceActive && authorizedBeam && beamTransmission01>.001f && blankerClosed01<.05f && !protectionFault;
            if(optionalBeamLight!=null)
            {
                optionalBeamLight.enabled=beamVisible;
                optionalBeamLight.intensity=beamTransmission01*3f;
                optionalBeamLight.color=new Color(.34f,.87f,1f);
            }
            if(optionalSourceGlow!=null)optionalSourceGlow.enabled=sourceActive && !protectionFault;
            if(optionalActivationGlow!=null)
            {
                optionalActivationGlow.enabled=activationFraction>.005f;
                optionalActivationGlow.intensity=activationFraction*2f;
            }
            if(optionalBeamDiagnosticVFX!=null)
            {
                if(beamVisible && !optionalBeamDiagnosticVFX.isPlaying)optionalBeamDiagnosticVFX.Play();
                else if(!beamVisible && optionalBeamDiagnosticVFX.isPlaying)
                    optionalBeamDiagnosticVFX.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
