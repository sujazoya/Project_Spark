#!/usr/bin/env python3
"""Project Spark Level 86 THE ORBITAL FORGE — original modular hard-surface 3D art.
Creates real binary FBX 7400 and GLB 2 files from procedural triangle meshes.
NOT real Unity electrical components, not an animated gameplay-level implementation.
All modular assets share assembly world origin. FBX numeric coords are cm, GLB m.
"""
from pathlib import Path
import math,random,time,csv,json,hashlib,os
import numpy as np
import trimesh
from collections import defaultdict,Counter
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for d in ['Models','GLB','Textures','Reports','Unity_Optional','Source']: (ROOT/d).mkdir(exist_ok=True,parents=True)
random.seed(86086)
NAMES='''
OrbitalForgeArenaBase
PanoramicEarthMaintenanceViewport
StationExteriorServiceTruss
StationRobotServiceBayFrame
DockedCargoSpacecraft
CargoServiceBayHatch
RobotLinearTraverseRail
SevenAxisRobotBase
RobotJoint1BaseYaw
RobotJoint2Shoulder
RobotJoint3UpperArm
RobotJoint4Elbow
RobotJoint5Forearm
RobotJoint6WristPitch
RobotJoint7WristRoll
RobotCounterbalanceModule
JointServoDriveRack
JointEncoderFeedbackArray
JointTorqueCurrentSensorRack
RobotBrakeAndTorqueLimiter
SevenAxisReachEnvelopeHologram
RoboticHomingCalibrationFixture
StereoMachineVisionCameraLeft
StereoMachineVisionCameraRight
StructuredLightDepthScanner
OpticalFiducialTargetArray
PrecisionVisionCalibrationBoard
LaserAlignmentMetrologyUnit
SixAxisWristForceTorqueSensor
ForceTorqueCalibrationFixture
CompliantInsertionController
SixAxisContactForceHologram
RobotJointPathPlanningComputer
InverseKinematicsSolverDisplay
CollisionProximityScanner
RobotTrajectorySafetySupervisor
FlexiblePayloadModalSensor
ActiveVibrationDampingController
AdaptiveRadiatorHandlingGripper
AutomaticEndEffectorToolChanger
PrecisionTorqueFasteningDriver
CaptiveFastenerAndBoltRack
VacuumCompatibleInspectionProbe
RoboticToolDockAndCalibrationNest
DualJawGripperPressureSensorArray
RadiatorGrappleHandleFixture
FailedThermalRadiatorModule
ReplacementThermalRadiatorModule
RadiatorMountingSpaceTruss
RadiatorThermalInterfacePlate
CoolantQuickDisconnectA
CoolantQuickDisconnectB
CoolantValveManifold
CoolantPressureTransducer
CoolantFlowMeter
RadiatorTemperatureSensorArray
StationAvionicsHeatLoadEmulator
ThermalLoopPumpSimulator
RadiatorLeakTestController
CoolantIsolationAndVentStation
RadiatorFastenerActuatorDriver
StationThermalTrendHologram
RadiatorThermalPerformanceAnalyzer
FailedRadiatorDisposalCradle
PayloadVibrationFaultInjector
JointEncoderDriftFaultInjector
AsymmetricGripperFaultInjector
RoboticContactSafetyPLC
ManipulatorEmergencyStopStation
Protected5VControlSupply
RobotMotorPowerDistributionUnit
IndependentContactSafetyRelay
SevenAxisJointServoController
ContactForceSignalConditioner
StereoVisionFusionComputer
RadiatorServiceSequencingPLC
HolographicOrbitalRoboticsSCADA
W129CriticalFeedbackPatchStation
HardwareInterlockDiagnosticRack
ServicingMissionCertificationBeacon
ExternalInspectionCameraGantry
RoboticServicingEventRecorder'''.strip().splitlines()
NAMES=['L86_'+n for n in NAMES]
assert len(NAMES)==82
C=core.COLORS={
 'structural':(.35,.46,.57,1),'titanium':(.63,.70,.76,1),'dark':(.060,.078,.12,1),
 'graphite':(.12,.15,.20,1),'copper':(.83,.39,.15,1),'gold':(.95,.69,.22,1),
 'ceramic':(.85,.88,.88,1),'glass':(.18,.61,.86,.48),'panel':(.025,.115,.21,1),
 'pcb':(.04,.22,.17,1),'cyan':(.16,.83,.96,1),'blue':(.18,.39,.85,1),
 'green':(.10,.91,.56,1),'amber':(1.,.59,.13,1),'violet':(.64,.43,.96,1),
 'red':(1.,.16,.14,1),'orange':(1.,.38,.07,1),'white':(.95,.96,.94,1),
 'radiator':(.080,.125,.18,1),'stars':(.77,.87,1.,1),'black':(.025,.03,.04,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in C}
# World-layout homing (metres), all models still share origin (0,0,0).
P={
0:(0,.1,0),1:(0,5.8,-7.4),2:(9.9,5.4,-3.2),3:(0,5.,-2.25),
4:(-9.1,5.,-3.1),5:(-7.4,5.,-3.0),6:(-4.55,1.17,-1.52),7:(-4.55,2.08,-1.52),
8:(-4.55,2.52,-1.52),9:(-4.15,3.03,-1.52),10:(-3.30,3.67,-1.52),11:(-2.35,4.22,-1.52),
12:(-1.30,4.74,-1.52),13:(-.55,5.15,-1.52),14:(.25,5.42,-1.52),15:(-4.96,3.27,-1.98),
16:(-12.35,1.71,5.55),17:(-11.10,1.71,5.55),18:(-9.85,1.71,5.55),19:(-8.60,1.71,5.55),
20:(-3.1,7.65,-5.9),21:(-5.95,1.70,4.25),22:(-1.2,6.3,-1.1),23:(-.45,6.3,-1.1),
24:(1.05,6.25,-1.7),25:(7.5,5.85,-2.65),26:(-3.2,1.69,4.36),27:(2.4,6.5,-2.75),
28:(.9,5.3,-1.52),29:(-1.95,1.7,5.45),30:(-.70,1.7,5.45),31:(3.4,7.32,-5.95),
32:(.56,1.7,5.45),33:(1.81,1.7,5.45),34:(3.06,1.7,5.45),35:(4.31,1.7,5.45),
36:(1.15,5.55,-1.65),37:(5.56,1.7,5.45),38:(1.5,5.22,-1.52),39:(.85,5.18,-1.52),
40:(2.32,5.10,-1.52),41:(-5.2,1.45,5.35),42:(.93,5.56,-1.45),43:(-5.65,1.68,3.82),
44:(1.6,5.10,-1.5),45:(6.8,5.5,-2.17),46:(8.15,5.20,-2.35),47:(8.25,1.39,4.05),
48:(9.25,5.35,-2.40),49:(8.37,5.25,-2.25),50:(7.00,4.60,-1.85),51:(9.40,4.60,-1.85),
52:(9.15,1.64,5.45),53:(10.35,1.64,5.45),54:(11.55,1.64,5.45),55:(9.9,5.2,-2.8),
56:(-2.7,6.20,-3.35),57:(7.05,1.67,5.5),58:(8.40,1.66,5.52),59:(10.1,3.3,5.16),
60:(7.14,5.65,-2.7),61:(0.,8.65,-6.1),62:(11.28,3.3,5.16),63:(9.25,1.1,3.2),
64:(6.75,3.35,5.65),65:(-7.12,3.35,5.65),66:(-5.85,3.35,5.65),
67:(-11.1,3.50,5.52),68:(-9.85,3.50,5.52),69:(-8.60,3.50,5.52),70:(-7.35,3.50,5.52),
71:(-6.10,3.50,5.52),72:(-4.85,3.50,5.52),73:(-3.60,3.50,5.52),74:(-2.35,3.50,5.52),
75:(-1.10,3.50,5.52),76:(0.,8.80,-6.33),77:(.15,3.50,5.52),78:(1.40,3.50,5.52),
79:(0.,10.3,-6.32),80:(7.1,7.6,-.8),81:(2.65,3.50,5.52)
}
assert set(P)==set(range(82))
# Animation pivot model binding. Robot link parenting hierarchy will be created by optional Unity visual rig.
PIVOT_TEXT='''
ANIM_CARGO_HATCH|5|rotation|y
ANIM_LINEAR_RAIL_CARRIAGE|6|translation|x
ANIM_ROBOT_BASE_ROTATE|8|rotation|y
ANIM_ROBOT_SHOULDER_PITCH|9|rotation|z
ANIM_ROBOT_UPPERARM_PITCH|10|rotation|z
ANIM_ROBOT_ELBOW_PITCH|11|rotation|z
ANIM_ROBOT_FOREARM_ROLL|12|rotation|x
ANIM_ROBOT_WRIST_PITCH|13|rotation|z
ANIM_ROBOT_WRIST_ROLL|14|rotation|x
ANIM_COUNTERBALANCE_SLIDER|15|translation|x
ANIM_SERVO_DRIVE_BANK|16|indicator|none
ANIM_ENCODER_SCAN|17|indicator|none
ANIM_MOTOR_TORQUE_GAUGE|18|indicator|none
ANIM_JOINT_BRAKE_ACTUATOR|19|translation|x
ANIM_REACH_ENVELOPE|20|indicator|none
ANIM_HOMING_PROBE|21|translation|z
ANIM_STEREO_LEFT_PAN|22|rotation|y
ANIM_STEREO_RIGHT_PAN|23|rotation|y
ANIM_DEPTH_SCANNER|24|rotation|y
ANIM_ALIGNMENT_LASER|27|rotation|y
ANIM_WRIST_FORCE_SENSOR|28|indicator|none
ANIM_COMPLIANT_INSERTION|30|translation|x
ANIM_CONTACT_WRENCH_VECTOR|31|indicator|none
ANIM_COLLISION_SENSOR_SWEEP|34|rotation|y
ANIM_FLEX_MODAL_RESPONSE|36|indicator|none
ANIM_ACTIVE_DAMPING|37|indicator|none
ANIM_GRIPPER_JAW_LEFT|38|translation|y
ANIM_GRIPPER_JAW_RIGHT|38|translation|y
ANIM_TOOL_CHANGER_ROTARY|39|rotation|x
ANIM_FASTENING_DRIVER_SPINDLE|40|rotation|x
ANIM_FASTENER_CASSETTE|41|translation|x
ANIM_INSPECTION_PROBE|42|translation|x
ANIM_TOOL_DOCK_INDEXER|43|rotation|y
ANIM_GRIP_CONTACT_SCAN|44|indicator|none
ANIM_PAYLOAD_GRAPPLE_LOCK|45|rotation|x
ANIM_FAILED_RADIATOR_RELEASE|46|translation|x
ANIM_REPLACEMENT_RADIATOR_LIFT|47|translation|y
ANIM_RADIATOR_TRUSS_JOINT|48|indicator|none
ANIM_THERMAL_PLATE_INTERFACE|49|translation|x
ANIM_COOLANT_CONNECTOR_A|50|translation|x
ANIM_COOLANT_CONNECTOR_B|51|translation|x
ANIM_COOLANT_VALVE|52|rotation|x
ANIM_COOLANT_PRESSURE_INDICATOR|53|indicator|none
ANIM_COOLANT_FLOW_INDICATOR|54|indicator|none
ANIM_RADIATOR_SENSOR_GRID|55|indicator|none
ANIM_AVIONICS_HEAT_LOAD|56|indicator|none
ANIM_PUMP_ROTOR|57|rotation|x
ANIM_LEAK_TEST_DIAGNOSTIC|58|indicator|none
ANIM_COOLANT_ISOLATION_VALVE|59|rotation|x
ANIM_FASTENER_MOTOR_BANK|60|rotation|x
ANIM_THERMAL_TREND|61|indicator|none
ANIM_DISPOSAL_CRADLE_LATCH|63|rotation|y
ANIM_PAYLOAD_OSCILLATION_FAULT|64|translation|y
ANIM_ENCODER_DRIFT_FAULT|65|rotation|z
ANIM_GRIPPER_ASYMMETRY_FAULT|66|translation|x
ANIM_CONTACT_SAFETY_PLC|67|indicator|none
ANIM_EMERGENCY_ROBOT_STOP|68|translation|z
ANIM_INDEPENDENT_CONTACT_RELAY|71|indicator|none
ANIM_VISION_FUSION|74|indicator|none
ANIM_SERVICE_SEQUENCE_PLC|75|indicator|none
ANIM_ROBOTICS_SCADA|76|indicator|none
ANIM_W129_TRUSTED_FORCE_FEEDBACK|77|indicator|none
ANIM_FINAL_SERVICING_CERTIFICATION|79|indicator|none
ANIM_INSPECTION_CAMERA_GANTRY|80|rotation|y
'''.strip().splitlines()
PIVOTS=[]
for ln in PIVOT_TEXT:
 pv,ind,kind,axis=ln.split('|');ind=int(ind)
 PIVOTS.append(dict(Pivot=pv,Index=ind,Model=NAMES[ind],Kind=kind,Axis=axis))
assert len(PIVOTS)==64,len(PIVOTS)
PIVOT_POS=core.PIVOT_POS={p['Pivot']:P[p['Index']] for p in PIVOTS}
# Move each robot joint pivot to its mechanical rotational centre.
for n,ind in [('ANIM_ROBOT_BASE_ROTATE',8),('ANIM_ROBOT_SHOULDER_PITCH',9),('ANIM_ROBOT_UPPERARM_PITCH',10),('ANIM_ROBOT_ELBOW_PITCH',11),('ANIM_ROBOT_FOREARM_ROLL',12),('ANIM_ROBOT_WRIST_PITCH',13),('ANIM_ROBOT_WRIST_ROLL',14)]:PIVOT_POS[n]=P[ind]
PIVOT_POS['ANIM_GRIPPER_JAW_LEFT']=(1.46,5.22,-1.18)
PIVOT_POS['ANIM_GRIPPER_JAW_RIGHT']=(1.46,5.22,-1.86)

def b(a,p,s,mat='titanium',pivot=None,label='CNCMechanicalPart'):a.box(p,s,mat,pivot,label)
def cyl(a,p,r,h,mat='titanium',axis='z',pivot=None,label='MachinedCylinder',seg=22):a.cylinder(p,r,h,mat,axis=axis,pivot=pivot,label=label,seg=seg)
def tor(a,p,R,r,mat='titanium',axis='z',pivot=None,label='MachinedRing',seg=44):a.torus(p,R,r,mat,axis=axis,pivot=pivot,label=label,seg=seg,tube=8)
def beam(a,p,q,r=.05,mat='structural',pivot=None,label='LoadBearingTruss'):a.beam(p,q,r,mat,pivot=pivot,label=label,sides=9)
def ball(a,p,r,mat='ceramic',pivot=None,label='SphericalShell',subs=2):
 m=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(m.vertices+np.array(p),m.faces,mat,pivot,label)
def taper(a,p,r0,r1,h,mat='titanium',axis='x',pivot=None,label='TaperedMachineCone',seg=32):
 ind={'x':0,'y':1,'z':2}[axis];ii=(ind+1)%3;jj=(ind+2)%3;v=[];f=[]
 for z,r in [(-h/2,r0),(h/2,r1)]:
  for k in range(seg):
   q=np.zeros(3);q[ind]=z;q[ii]=r*math.cos(k*math.tau/seg);q[jj]=r*math.sin(k*math.tau/seg);v.append((np.asarray(p)+q).tolist())
 for k in range(seg):
  n=(k+1)%seg;f.extend([[k,n,k+seg],[n,n+seg,k+seg]])
 a.mesh(v,f,mat,pivot,label)

def make_panel(a,p,scale=1.,theme='cyan',kind='plc',pivot=None):
 core.panel_equipment(a,p,scale,theme,kind)
 x,y,z=p
 for j in range(5):
  xx=x-.40+j*.20*scale
  cyl(a,(xx,y-.52*scale,z+.40*scale),.051*scale,.095*scale,'gold','z',None,'GoldPlatedConnector',12)
 for x0 in [-.46,.46]:b(a,(x+x0*scale,y+.49*scale,z+.17*scale),(.08,.17,.18),'titanium',None,'RackHandle')
 if kind=='fault':
  b(a,(x,y+.51*scale,z+.42*scale),(.49,.045,.04),'red',None,'FaultConditionIndicator')

def robot_joint(a,p,pivot,idx):
 x,y,z=p; rr=.31 if idx<12 else .225
 cyl(a,p,rr,.52,'graphite','z',pivot,'SealedHarmonicDriveMotor',32)
 cyl(a,(x,y,z+.27),rr*.79,.12,'gold','z',pivot,'PrecisionReducerOutputFlange',30)
 tor(a,(x,y,z+.34),rr*.68,.035,'cyan','z',pivot,'AbsoluteEncoderIlluminatedScale',48)
 cyl(a,(x,y,z-.30),rr*.58,.16,'titanium','z',pivot,'JointBearingRace',22)
 for k in range(14):
  angle=k*math.tau/14
  yy=y+rr*.72*math.cos(angle);xx=x+rr*.72*math.sin(angle)
  cyl(a,(xx,yy,z+.35),.027,.045,'copper','z',pivot,'DriveFlangeBolt',10)
 # narrow load frame on each joint
 b(a,(x,y-.31,z),(.54,.09,.57),'ceramic',pivot,'ThermalProtectedJointHousing')

def arm_link(a,start,end,r=.24,pivot=None):
 beam(a,start,end,r,'ceramic',pivot,'TitaniumCompositeArmLink')
 beam(a,(start[0],start[1],start[2]+.15),(end[0],end[1],end[2]+.15),.035,'gold',pivot,'ExternalFeedbackHarnessRail')
 beam(a,(start[0],start[1],start[2]-.15),(end[0],end[1],end[2]-.15),.029,'graphite',pivot,'ArmThermalHarnessSleeve')
 for t in (.22,.49,.76):
  pos=np.asarray(start)*(1-t)+np.asarray(end)*t
  ball(a,pos,.11,'titanium',pivot,'CompositeLinkReinforcementNode',subs=1)

def radiator(a,p,failed=False,pivot=None):
 x,y,z=p;ww=3.26; hh=2.24; thick=.12
 # vertical radiator with metal edge frame and 32 dark vertical thermal panels
 b(a,p,(ww,hh,thick),'graphite',pivot,'RadiatorLaminateSubstrate')
 for xx in (-ww/2,ww/2):beam(a,(x+xx,y-hh/2,z),(x+xx,y+hh/2,z),.09,'gold' if not failed else 'structural',pivot,'RadiatorPerimeterSpar')
 for yy in (-hh/2,hh/2):beam(a,(x-ww/2,y+yy,z),(x+ww/2,y+yy,z),.085,'titanium',pivot,'RadiatorEndFrame')
 for j in range(28):
  xx=x-1.55+j*.115
  b(a,(xx,y,z+.08),(.083,hh-.20,.03),'radiator' if j%2 else 'blue',pivot,'OpticalBlackFinChannel')
  b(a,(xx,y,z+.104),(.016,hh-.24,.019),'red' if failed and j%7==0 else 'titanium',pivot,'HeatPipeBondTrack')
 for row in (-.8,0,.8):
  beam(a,(x-1.51,y+row,z-.09),(x+1.51,y+row,z-.09),.044,'copper',pivot,'EnclosedHeatPipeArray')
 for xx in (-1.42,1.42):
  for yy in (-.94,.94):
   cyl(a,(x+xx,y+yy,z+.09),.09,.11,'gold','z',pivot,'CaptiveRadiatorMountBolt',16)
 # diagonal marking for failed panel with red status
 if failed:
  for k in range(8):b(a,(x-.78+k*.22,y-.72+k*.2,z+.14),(.15,.045,.018),'red',pivot,'FailedRadiatorRedCrossMark')
 else:
  for k in range(8):b(a,(x-1.25+k*.32,y+.96,z+.14),(.17,.04,.018),'green',pivot,'ReplacementRadiatorInspectionBadge')

def scanner(a,p,pivot=None,kind='camera'):
 x,y,z=p
 b(a,p,(.53,.43,.55),'graphite',pivot,'HermeticOpticalHousing')
 cyl(a,(x,y,z+.36),.19,.17,'titanium','z',pivot,'PrecisionOpticalAperture',26)
 cyl(a,(x,y,z+.47),.125,.03,'glass','z',pivot,'CoatedOpticalWindow',26)
 tor(a,(x,y,z+.50),.13,.026,'cyan' if kind=='depth' else 'gold','z',pivot,'OpticalIlluminationRing',26)
 for j in range(4):
  b(a,(x-.22+j*.15,y-.18,z+.27),(.075,.04,.04),'gold',pivot,'CalibrationFiducial')
 for k in range(4):cyl(a,(x-.17+k*.11,y+.17,z-.25),.035,.12,'titanium','z',pivot,'CameraMatingPort',12)

def gadget(a,p,pivot=None):
 x,y,z=p
 b(a,(x,y,z),(.92,.7,.65),'graphite',None,'InstrumentShieldingChassis')
 for j in range(5):
  b(a,(x-.37+j*.19,y+.20,z+.343),(.13,.11,.03),'cyan' if j%2 else 'gold',pivot,'TelemetryStatusElement')
 for j in range(8):
  cyl(a,(x-.34+j*.095,y-.20,z+.35),.034,.08,'titanium','z',None,'InstrumentationPort',10)


def make_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i]
 piv=[p['Pivot'] for p in PIVOTS if p['Index']==i]
 pv=piv[0] if piv else None
 if i==0:
  b(a,(0,-.12,0),(30.,.35,16.8),'graphite',label='OrbitalForgeModuleBase')
  for zz in (-7.8,7.9):b(a,(0,.12,zz),(30,.25,.29),'structural',label='DockFloorEdge')
  for xx in (-14.5,14.5):b(a,(xx,.12,0),(.30,.26,16.0),'titanium',label='DockFloorSupport')
  for j in range(15):
   xx=-14+j*2
   b(a,(xx,.1,0),(.025,.02,15.8),'gold',label='PlatformFloorGridInlay')
  for j in range(11):
   zz=-7.4+j*1.45
   b(a,(0,.12,zz),(28.6,.02,.025),'blue' if j%2 else 'structural',label='WorkstationFloorGrid')
  for j in range(10):
   xx=-13+j*2.9
   b(a,(xx,.12,4.7),(.14,.025,.9),'amber',label='FrontControlStationBoundary')
 elif i==1:
  # non-background texture mesh: star arcs/planet horizon sculpted sphere
  ball(a,(0,5.0,-9.7),7.45,'blue',None,'EarthCurvatureOutsideObservationWindow',subs=3)
  tor(a,(0,6.0,-8.5),6.7,.045,'cyan','z',None,'OrbitalHorizonArc',90)
  for j in range(200):
   xx=random.uniform(-14.2,14.2);yy=random.uniform(2.9,11.1)
   b(a,(xx,yy,-9.65),(.019,.019,.017),'white' if j%5 else 'amber',label='StarLightRefPoint')
 elif i in (2,3):
  xx=9.9 if i==2 else 0
  for k in range(7):
   sx=xx-4.2+k*1.4
   beam(a,(sx,2.8,-4.3),(sx,8.6,-4.3),.10,'titanium',label='StationStructuralColumn')
   beam(a,(sx,2.8,-4.3),(sx+1.4,8.6,-4.3),.055,'gold',label='DiagonalStructuralTie')
  for h in (3.4,5.2,7.8):beam(a,(xx-4.5,h,-4.3),(xx+4.5,h,-4.3),.10,'structural',label='TrussLongeron')
  for k in range(8):
   b(a,(xx-4.2+k*1.15,6.9,-4.3),(.62,.38,.18),'ceramic',label='ThermalProtectionArrayPanel')
 elif i==4:
  taper(a,P[i],1.45,1.28,4.1,'ceramic','x',pv,'PressurizedCargoServiceBus',48)
  for j in range(10):tor(a,(x-1.7+j*.38,y,z),1.33,.045,'gold','x',pv,'CargoSpacecraftReinforcementBand',48)
  for s in (-1,1):
   b(a,(x,y,z+s*1.37),(3.4,.55,.08),'graphite',pv,'CargoVehicleSideEquipmentTrunk')
   for j in range(7):b(a,(x-1.4+j*.47,y,z+s*1.44),(.29,.35,.025),'titanium',pv,'SpacecraftHeatShieldTile')
 elif i==5:
  b(a,P[i],(.2,2.18,2.25),'titanium',pv,'CargoServiceHatchPanel')
  for j in range(14):tor(a,(x+.06,y,z),.89-j*.005,.007,'gold','x',pv,'CargoHatchSealingLines',40)
  for j in range(6):cyl(a,(x+.17,y-1+j*.38,z+.68),.075,.09,'gold','x',pv,'AirlockCaptiveFastener',12)
 elif i==6:
  b(a,(-4.55,1.06,-1.52),(6.7,.45,2.3),'graphite',label='RoboticTraverseRailFoundation')
  for s in (-.88,.88):
   b(a,(-4.55,1.28,-1.52+s),(6.4,.14,.10),'titanium',label='PrecisionLinearGuideRail')
   for k in range(21):b(a,(-7.52+k*.30,1.26,-1.52+s),(.09,.18,.17),'gold',label='LinearGuideTieBlock')
  b(a,(-4.55,1.57,-1.52),(1.8,.43,1.62),'titanium',pv,'MovingRobotRailCarriage')
  for j in range(5):cyl(a,(-5.3+j*.36,1.67,-1.52),.13,.25,'graphite','y',pv,'ServoRailGuideRoller',12)
 elif i==7:
  b(a,P[i],(1.4,1.3,1.5),'ceramic',label='SevenAxisRobotPrimaryBase')
  for j in range(4):
   xx=x+(-.55 if j%2==0 else .55);zz=z+(-.52 if j//2==0 else .52)
   cyl(a,(xx,y+.58,zz),.085,.17,'gold','y',label='RobotGroundingMountBolt')
  tor(a,(x,y+.61,z),.68,.12,'gold','y',label='BaseYawBearingRace')
 elif 8<=i<=14:
  robot_joint(a,P[i],pv,i)
  # enforce chain link shaped from each joint center to NEXT joint pivot for visual assembly
  if i<14:
   arm_link(a,P[i],P[i+1],.22 if i<=11 else .15,pv)
  else:
   cyl(a,(x+.37,y,z),.20,.75,'ceramic','x',pv,'EndEffectorBearingFlange',24)
 elif i==15:
  b(a,P[i],(.9,.64,.68),'graphite',pv,'CounterbalanceSlideCase')
  for j in range(3):cyl(a,(x+.16*j,y+.38,z),.12,.28,'gold','y',pv,'CounterbalanceHydraulicDamper')
 elif i in (16,17,18,19):
  make_panel(a,P[i],.95,'cyan' if i in (16,17) else 'amber','plc',pv)
  for j in range(7):cyl(a,(x-.4+j*.13,y+.62,z+.34),.053,.10,'gold','z',pv,'JointServoDriveTerminal',12)
 elif i in (20,31,61):
  for radius in (.58,1.05,1.62,2.0):tor(a,P[i],radius,.024,'cyan' if radius<1.8 else 'amber','z',pv,'ThreeDimensionalEngineeringDisplay',54)
  for j in range(15):
   t=j*math.tau/15
   beam(a,(x,y,z),(x+1.85*math.cos(t),y+1.1*math.sin(t),z),.013,'green' if j%3 else 'amber',pv,'VectorDisplayRays')
 elif i==21:
  b(a,P[i],(1.35,.6,1.35),'structural',pv,'RobotHomeCalibrationTable')
  for j in range(4):
   cyl(a,(x-.43+(j%2)*.86,y+.4,z-.43+(j//2)*.86),.17,.35,'gold','y',pv,'AbsoluteHomeReferencePin')
 elif i in (22,23,24,27,34,80):
  scanner(a,P[i],pv,'depth' if i in (24,27,34) else 'camera')
  if i==80:
   beam(a,(x,y-1.5,z),(x,y+.6,z),.12,'titanium',None,'InspectionCameraAdjustableGantry')
   beam(a,(x,y+.6,z),(x-1.4,y+.6,z-.4),.10,'gold',pv,'OverheadCameraCantilever')
 elif i in (25,26):
  b(a,P[i],(1.5,1.05,.06),'graphite',pv,'VisionCalibrationPlate')
  for u in range(11):
   for v in range(7):
    b(a,(x-.65+u*.13,y-.39+v*.13,z+.04),(.063,.063,.02),'white' if (u+v)%2 else 'dark',pv,'CheckerboardOrFiducialMark')
 elif i==28:
  cyl(a,P[i],.35,.37,'graphite','x',pv,'SixAxisIndustrialWrenchTransducer',32)
  tor(a,(x+.16,y,z),.28,.038,'gold','x',pv,'CalibratedMultiAxisLoadRing')
  for j in range(12):
   t=j*math.tau/12
   cyl(a,(x+.19,y+.27*math.sin(t),z+.27*math.cos(t)),.028,.09,'cyan','x',pv,'OpticalStrainGauge',10)
 elif i==29:
  b(a,P[i],(.9,.58,.74),'graphite',pv,'ForceTorqueSixAxisCalibrationBed')
  for j in range(6):
   cyl(a,(x-.25+(j%3)*.25,y+.41,z-.17+(j//3)*.30),.064,.19,'gold','y',pv,'LoadCalibrationWeightReference')
 elif i==30:
  b(a,P[i],(.92,.6,.75),'graphite',None,'CompliantAxisControllerModule')
  for j in range(5):
   cyl(a,(x-.31+j*.16,y+.39,z),.09,.34,'titanium','y',pv,'ContactCompliancePiston',12)
 elif i in (36,37):
  gadget(a,P[i],pv)
  for j in range(9):b(a,(x-.40+j*.1,y+.28,z+.35),(.063,.03,.02),'cyan' if j%3 else 'amber',pv,'ModalSensorSpectralBar')
 elif i==38:
  # two independent opposing jaw pivots, gripping an elevated radiator sample
  b(a,(x-.25,y,z),(.85,.31,.75),'graphite',None,'GripperActuatedBaseHousing')
  cyl(a,(x+.12,y,z),.28,.27,'titanium','x',None,'GripperMechanicalMountFlange')
  pivleft='ANIM_GRIPPER_JAW_LEFT';pivright='ANIM_GRIPPER_JAW_RIGHT'
  for side,pt in [(-1,pivleft),(1,pivright)]:
   zc=z+side*.34
   b(a,(x+.48,y,zc),(.84,.20,.17),'ceramic',pt,'RoboticAdaptiveJawFinger')
   b(a,(x+.81,y,zc),(.18,.23,.23),'gold',pt,'ContactPressurePad')
   for j in range(3):b(a,(x+.65,y-.055+j*.055,zc+side*.08),(.065,.026,.025),'cyan',pt,'TactileFeedbackArray')
 elif i==39:
  cyl(a,P[i],.56,.4,'titanium','x',pv,'PrecisionRotatingToolChangerHub',40)
  for j in range(8):
   t=j*math.tau/8;yc=y+.46*math.cos(t);zc=z+.46*math.sin(t)
   b(a,(x+.32,yc,zc),(.25,.12,.11),'gold',pv,'ToolChangerCamLock')
 elif i==40:
  cyl(a,P[i],.17,.76,'ceramic','x',pv,'MotorizedTorqueToolSpindle')
  cyl(a,(x+.43,y,z),.09,.4,'gold','x',pv,'CaptiveBoltDriverBit')
  tor(a,(x-.24,y,z),.20,.055,'graphite','x',pv,'TorqueLimitClutch')
 elif i==41:
  b(a,P[i],(1.7,.38,.66),'titanium',pv,'FastenerMagazinePlate')
  for k in range(20):
   xx=x-.72+(k%10)*.16;zz=z-.18+(k//10)*.36
   cyl(a,(xx,y+.31,zz),.045,.16,'gold','y',pv,'CaptiveTitaniumFastener',10)
 elif i==42:
  cyl(a,P[i],.12,.92,'ceramic','x',pv,'InspectionBorescopeProbe',22)
  ball(a,(x+.55,y,z),.14,'glass',pv,'InspectionOpticalHead',subs=2)
 elif i==43:
  b(a,P[i],(2.4,.5,1.15),'graphite',pv,'IndexedMultiToolStorageBase')
  for j in range(5):
   xx=x-1+j*.49;cyl(a,(xx,y+.45,z),.12,.63,'titanium','y',pv,'DockedPrecisionServiceTool',14)
   cyl(a,(xx,y+.77,z),.16,.07,'gold','y',pv,'ToolIdentificationBand',14)
 elif i==44:
  b(a,P[i],(.72,.34,.7),'graphite',pv,'GripperTactileSensorElectronics')
  for j in range(16):b(a,(x-.30+(j%8)*.083,y+.19,z-.28+(j//8)*.56),(.045,.03,.04),'cyan' if j%4 else 'green',pv,'ContactTactileMatrix')
 elif i==45:
  b(a,P[i],(.35,.75,1.2),'structural',pv,'RadiatorGrappleFixtureHandle')
  tor(a,(x,y+.42,z),.29,.07,'gold','y',pv,'LoadBearingGrappleEye',36)
 elif i in (46,47):
  radiator(a,P[i],failed=i==46,pivot=pv)
 elif i==48:
  for j in range(5):
   xx=x-2.1+j*1.05
   beam(a,(xx,3.68,-2.5),(xx,7.15,-2.5),.11,'structural',label='RadiatorTrussUpright')
   beam(a,(xx,3.68,-2.5),(xx+1.0,7.15,-2.5),.062,'gold',label='RadiatorDiagonalStrut')
  for yy in (3.9,5.45,7.0):beam(a,(x-2.35,yy,-2.5),(x+2.35,yy,-2.5),.10,'titanium',label='RadiatorPrimarySupportBeam')
 elif i==49:
  b(a,P[i],(.22,2.5,3.36),'titanium',pv,'RadiatorThermalFootprintPlate')
  for k in range(6):b(a,(x+.15,y-1.08+k*.42,z),(.1,.15,3.0),'copper',pv,'ConductiveThermalInterfaceBus')
 elif i in (50,51):
  cyl(a,P[i],.17,.67,'titanium','x',pv,'CoolantQuickDisconnectHousing',28)
  for xx in [-.22,.2]:tor(a,(x+xx,y,z),.21,.051,'gold','x',pv,'QuickDisconnectSealingCollar',32)
  for k in range(9):
   t=k*math.tau/9;cy=y+.22*math.cos(t);cz=z+.22*math.sin(t)
   b(a,(x+.20,cy,cz),(.12,.055,.055),'cyan',pv,'CoolantConnectorRetainerJaw')
 elif i in (52,57,59):
  gadget(a,P[i],None)
  cyl(a,(x+.12,y+.45,z+.33),.18,.24,'gold','x',pv,'ControlledCoolantValvingActuator')
  for j in range(3):beam(a,(x-.4,y-.22+j*.17,z+.33),(x+.4,y-.22+j*.17,z+.33),.046,'copper',pv,'SimulatedCoolantMicrochannel')
 elif i in (53,54,55,56,58,60,62):
  make_panel(a,P[i],.94,'amber' if i in (53,56) else 'cyan','instrument',pv)
 elif i==63:
  b(a,(x,y-.15,z),(4.0,.32,3.3),'graphite',None,'DamagedRadiatorServiceDisposalSkid')
  for xx in (-1.69,1.69):
   b(a,(x+xx,y+.31,z),(.18,.89,3.0),'titanium',pv,'RadiatorCradleLockingSideRail')
  for k in range(6):b(a,(x-1.6+k*.63,y-.03,z+1.28),(.2,.07,.08),'red',pv,'SpentRadiatorRetentionLatch')
 elif i in (64,65,66):
  make_panel(a,P[i],.94,'red','fault',pv)
  b(a,(x+.27,y+.44,z+.44),(.14,.14,.1),'red',pv,'FaultInjectionGuardSwitch')
 elif i==76:
  b(a,(0,8.85,-6.32),(11.8,2.7,.24),'dark',None,'MainRoboticServicingHolographicSCADABoard')
  b(a,(0,8.82,-6.15),(11.35,2.35,.035),'panel',pv,'RealTimeRoboticTwinInterfaceGlass')
  for j in range(20):
   b(a,(-5.3+j*.56,8.80+(j%5)*.20,-6.12),(.43,.018,.017),'cyan' if j%3 else 'green',pv,'RobotSCADARealtimeTelemetry')
  for j in range(7):tor(a,(-4.4+j*1.45,8.23,-6.1),.22,.020,'green','z',pv,'QualifiedRoboticAxisStatus',30)
 elif i==79:
  cyl(a,(x,y-.22,z),.51,.42,'gold','y',pv,'OrbitalForgeSixObjectiveBeaconHousing',38)
  tor(a,(x,y+.15,z),.67,.057,'green','y',pv,'RoboticMissionCertifiedRing',60)
  taper(a,(x,y+.32,z),.32,.05,.43,'cyan','y',pv,'OrbitalForgeHolographicCertificationSpear',28)
 elif i==68:
  make_panel(a,P[i],.90,'red','fault',pv)
  cyl(a,(x,y+.53,z+.45),.17,.12,'red','z',pv,'EmergencyRobotStopButton',24)
 elif i==77:
  make_panel(a,P[i],.96,'amber','plc',pv)
  b(a,(x-.18,y-.2,z+.43),(.47,.12,.07),'red',pv,'W129TrustedFeedbackRemovablePatchPort')
  for j in range(5):cyl(a,(x+.23,y-.34+j*.11,z+.48),.04,.08,'gold','z',None,'CriticalFeedbackIsolationConnector',12)
 else:
  theme='green' if i in (67,71,72,73,74) else 'cyan'
  kind='plc' if i in (67,69,70,71,72,73,74,75,78) else 'instrument'
  make_panel(a,P[i],.96,theme,kind,pv)
  if i in (67,71):
   for j in range(9):b(a,(x-.4+j*.10,y-.48,z+.48),(.058,.08,.042),'gold',None,'IndependentSafetyIOTerminal')
 # Every announced animation pivot must own actual geometry; indicators are distinct meshes.
 used={s[3] for s in a.raw if s[3] is not None}
 for p in piv:
  if p not in used:
   pp=PIVOT_POS[p];color='cyan' if next(pv['Kind'] for pv in PIVOTS if pv['Pivot']==p)=='indicator' else 'gold'
   b(a,(pp[0],pp[1],pp[2]+.12),(.10,.09,.045),color,p,'VisualStatePivotMarker')
 return a
# Source-driven conceptual design: these are NOT instantiated electrical wires.
WIRES=[];GROUPS=[]
def w(s,t,p,k='DIGITAL_CONTROL'):
 WIRES.append(dict(WireID='W%03d'%(len(WIRES)+1),SourceModel=NAMES[s],TargetModel=NAMES[t],SignalPurpose=p,SignalKind=k,Status='CONCEPT_ONLY_NOT_PLAYER_CREATED'))
def group(title,entries):
 start=len(WIRES)+1
 for tup in entries:w(*tup)
 GROUPS.append(dict(Group=title,StartWire='W%03d'%start,EndWire='W%03d'%len(WIRES),Count=len(entries)))

group('Educational 5 V control network and independent return checks',[
(69,67,'5 V robotic contact safety PLC logic supply','POWER_CONTROL'),
(69,75,'5 V servicing sequencing PLC supply','POWER_CONTROL'),
(69,72,'5 V seven-axis joint controller power','POWER_CONTROL'),
(69,74,'5 V stereo vision fusion controller supply','POWER_CONTROL'),
(69,73,'5 V force-torque signal conditioning power','POWER_CONTROL'),
(69,16,'5 V joint motor drive control interface supply','POWER_CONTROL'),
(69,76,'5 V live robotics SCADA supply','POWER_CONTROL'),
(69,71,'5 V independent contact relay logic power','POWER_CONTROL'),
(69,58,'5 V coolant leak test controller supply','POWER_CONTROL'),
(69,32,'5 V robotic path planner logic supply','POWER_CONTROL'),
(69,78,'5 V hardware interlock diagnostic supply','POWER_CONTROL'),
(69,77,'5 V critical W129 patch signal station supply','POWER_CONTROL')])
J=[]
for j in range(7):
 s=[8,9,10,11,12,13,14][j]
 J += [(72,s,'Joint %d commanded servo position/torque'%(j+1),'ACTUATOR_COMMAND'),(s,17,'Joint %d actual encoder and homing reference'%(j+1),'ANALOG_FEEDBACK'),(18,72,'Joint %d measured motor current and torque'%(j+1),'ANALOG_FEEDBACK')]
J += [(17,72,'Fused seven-joint angle validity and timestamps','DATA_STREAM'),(72,19,'Protective joint brake release request','SAFETY_CONTROL'),(19,67,'Measured joint brake mechanism actual state','DIGITAL_STATUS')]
assert len(J)==24
group('Seven-axis joint servos, encoders, and motor torque feedback',J)
V=[
(22,74,'Left stereo camera frames and hardware timestamps','DATA_STREAM'),
(23,74,'Right stereo camera frames and hardware timestamps','DATA_STREAM'),
(24,74,'Depth scanner measured structured-light points','DATA_STREAM'),
(25,74,'Optical fiducial location and identity','DATA_STREAM'),
(26,74,'Calibration board geometric reference','DATA_STREAM'),
(27,74,'Laser alignment actual offset measurement','ANALOG_FEEDBACK'),
(74,22,'Left camera timed acquisition request','DIGITAL_CONTROL'),
(74,23,'Right camera timed acquisition request','DIGITAL_CONTROL'),
(74,24,'Depth scanner ranging trigger','DIGITAL_CONTROL'),
(74,27,'Laser alignment measurement request','DIGITAL_CONTROL'),
(74,32,'Fused target relative position and orientation','DATA_STREAM'),
(74,33,'Robot tool calibration covariance','DATA_STREAM'),
(34,35,'Proximity collision risk and range measurement','ANALOG_FEEDBACK'),
(32,35,'Commanded collision-aware trajectory candidates','DATA_STREAM'),
(35,72,'Validated joint trajectory motion permit','SAFETY_CONTROL'),
(33,32,'Inverse kinematic joint candidate solution','DATA_STREAM'),
(32,20,'Robot predicted reach envelope visualization','DATA_STREAM'),
(21,72,'Seven-axis reference homing completion','DIGITAL_STATUS'),
(74,76,'Vision pose and calibration live telemetry','DATA_STREAM'),
(34,76,'Collision clearance and obstacle sensor status','DATA_STREAM')]
assert len(V)==20
group('Stereo vision, depth metrology, kinematics and collision avoidance',V)
F=[
(28,73,'Six-axis wrist load raw strain gauge measurement','ANALOG_FEEDBACK'),
(73,30,'Conditioned measured contact wrench for insertion control','ANALOG_FEEDBACK'),
(30,72,'Compliant contact motion correction request','ACTUATOR_COMMAND'),
(29,73,'Wrist six-axis load calibration reference','ANALOG_FEEDBACK'),
(73,31,'Live force torque wrench holographic vector','DATA_STREAM'),
(73,35,'Force torque contact overload warning','SAFETY_CONTROL'),
(71,30,'Independent contact motion enable','SAFETY_CONTROL'),
(72,30,'Joint servo response for compliant insertion','ANALOG_FEEDBACK'),
(30,75,'Contact insertion state and alignment error','DIGITAL_STATUS'),
(36,37,'Actual flexible radiator vibration sensor response','ANALOG_FEEDBACK'),
(37,32,'Recommended shaped-trajectory acceleration limit','ACTUATOR_COMMAND'),
(64,36,'Controlled flexible payload vibration disturbance','SAFETY_CONTROL'),
(65,17,'Injected joint encoder drift measurement bias','SAFETY_CONTROL'),
(17,35,'Crosschecked individual joint-angle discrepancy','ANALOG_FEEDBACK'),
(44,73,'Independent gripper pressure cell array health','ANALOG_FEEDBACK'),
(73,75,'Grasp-load balance qualification result','DIGITAL_STATUS'),
(28,76,'Non-authoritative force torque SCADA stream','DATA_STREAM'),
(36,76,'Payload flex dominant mode response telemetry','ANALOG_FEEDBACK'),
(37,72,'Mode damping joint acceleration filter','ACTUATOR_COMMAND'),
(71,67,'Independent contact permission relay diagnostic','DIGITAL_STATUS')]
assert len(F)==20
group('Six-axis force-torque, compliant control and flexible payload dynamics',F)
T=[
(75,38,'Robot gripper grasp/release command','ACTUATOR_COMMAND'),
(38,44,'Measured jaw surface pressure and contact state','ANALOG_FEEDBACK'),
(75,39,'Automatic tool changer selected tool request','ACTUATOR_COMMAND'),
(39,75,'Actual end-effector lock confirmation','DIGITAL_STATUS'),
(75,40,'Motorized fastener tightening torque request','ACTUATOR_COMMAND'),
(40,75,'Measured fastener torque and turn count','ANALOG_FEEDBACK'),
(41,40,'Captive bolt stock and insertion status','DIGITAL_STATUS'),
(75,42,'Borescope inspection sequence trigger','DIGITAL_CONTROL'),
(42,74,'Connector bore-scope image evidence','DATA_STREAM'),
(43,39,'Tool changer index/homing identity feedback','DIGITAL_STATUS'),
(45,38,'Payload grapple fixture captured status','ANALOG_FEEDBACK'),
(38,67,'Gripper hold continuity protective status','SAFETY_CONTROL'),
(66,44,'Asymmetric gripper pad contact fault condition','SAFETY_CONTROL'),
(44,75,'Gripper contact imbalance fault diagnostic','ANALOG_FEEDBACK'),
(60,40,'Fastening driver actuator readiness and limit','DIGITAL_STATUS'),
(75,60,'Fastener driver motion enable command','ACTUATOR_COMMAND'),
(63,75,'Defective radiator waste cradle secured state','DIGITAL_STATUS'),
(38,76,'Gripper actual jaw positions and tool health','DATA_STREAM')]
assert len(T)==18
group('End effector, gripper, tool changer and fastening',T)
H=[
(75,59,'Radiator line isolation and vent request','ACTUATOR_COMMAND'),
(59,75,'Measured safe cooled line depressurization','ANALOG_FEEDBACK'),
(75,50,'Coolant connector A safe mating command','ACTUATOR_COMMAND'),
(75,51,'Coolant connector B safe mating command','ACTUATOR_COMMAND'),
(50,75,'Connector A actual sealed latch feedback','ANALOG_FEEDBACK'),
(51,75,'Connector B actual sealed latch feedback','ANALOG_FEEDBACK'),
(75,52,'Cooling valve controlled-open command','ACTUATOR_COMMAND'),
(52,75,'Actual coolant valve position measurement','ANALOG_FEEDBACK'),
(53,58,'Main coolant circuit measured pressure','ANALOG_FEEDBACK'),
(54,58,'Main coolant circuit measured flow rate','ANALOG_FEEDBACK'),
(55,62,'Radiator temperature sensor array samples','ANALOG_FEEDBACK'),
(56,62,'Real station avionics heat demand telemetry','ANALOG_FEEDBACK'),
(57,62,'Coolant pump actual duty and throughput','ANALOG_FEEDBACK'),
(58,75,'Closed-loop coolant qualification leak status','DIGITAL_STATUS'),
(62,75,'Integrated radiator heat rejection qualification','DATA_STREAM'),
(75,57,'Qualified pump startup and control request','ACTUATOR_COMMAND'),
(62,61,'Live station thermal trend and cooling rate','DATA_STREAM'),
(55,76,'Radiator inlet outlet thermistor SCADA data','DATA_STREAM')]
assert len(H)==18
group('Radiator thermal interface, coolant and leak-test system',H)
S=[
(68,67,'Operator emergency manipulator stop hardwired status','SAFETY_CONTROL'), # W113
(71,67,'Independent contact safety protective relay state','SAFETY_CONTROL'),
(34,67,'Independent collision interlock risk status','SAFETY_CONTROL'),
(35,67,'Robot trajectory safe-motion qualification','SAFETY_CONTROL'),
(17,67,'Joint encoder validity disagreement interlock','SAFETY_CONTROL'),
(44,67,'Independent gripper hold pressure safety','SAFETY_CONTROL'),
(75,67,'Contact and tooling motion authorization request','SAFETY_CONTROL'),
(67,72,'Protected joint drive and brake enable','SAFETY_CONTROL'),
(67,38,'Protective gripper hold/release inhibit','SAFETY_CONTROL'),
(67,30,'Protected compliant contact permit','SAFETY_CONTROL'),
(67,75,'Independent service sequence emergency inhibit','SAFETY_CONTROL'),
(65,67,'Encoder drift fault injection diagnostic','DIGITAL_STATUS'),
(64,67,'Flex oscillation overload safety status','DIGITAL_STATUS'),
(66,67,'Asymmetric gripper contact safety status','DIGITAL_STATUS'),
(67,78,'Safety interlock fault latch diagnostic','DIGITAL_STATUS'),
(73,77,'Wrist conditioned load sample diagnostic patch','ANALOG_FEEDBACK'), # W128
(28,67,'CRITICAL trusted six-axis wrist contact wrench W129','ANALOG_FEEDBACK'), # W129
(77,67,'Critical W129 patch continuity diagnostic only','DIGITAL_STATUS'), # W130
(67,78,'W129 trusted-sample freshness and fault status','DIGITAL_STATUS'),
(78,67,'Operator explicit fault reset request','SAFETY_CONTROL'),
(67,75,'Post-repair safety recommissioning clearance','SAFETY_CONTROL'),
(70,72,'Isolated fictional joint motor plant power permit','SAFETY_CONTROL'),
(67,76,'Critical fault latch and motion permission telemetry','DIGITAL_STATUS'),
(67,79,'Safety-qualified mission completion certificate','DIGITAL_STATUS')]
assert len(S)==24
assert len(WIRES)==112
# W113-136; W129 is 17th row.
group('Independent safety, fault recovery and critical W129',S)
assert WIRES[128]['WireID']=='W129' and WIRES[128]['SourceModel']==NAMES[28] and WIRES[128]['TargetModel']==NAMES[67]
SC=[
(75,76,'Servicing mission objective evidence state','DATA_STREAM'),
(72,76,'Real seven-axis servo position and error trace','DATA_STREAM'),
(17,76,'Encoder positions timestamp and health stream','DATA_STREAM'),
(18,76,'Motor winding current and load live trace','DATA_STREAM'),
(73,76,'Conditioned wrist wrench diagnostic telemetry','DATA_STREAM'),
(74,76,'Machine vision calibration and target pose','DATA_STREAM'),
(62,76,'Radiator pressure flow energy balance summary','DATA_STREAM'),
(58,76,'Cooling leak test results and sample time','DATA_STREAM'),
(32,76,'Tool path collision clearance predictions','DATA_STREAM'),
(37,76,'Flexible load oscillation estimate','DATA_STREAM'),
(67,76,'Independent contact safety system live status','DIGITAL_STATUS'),
(76,81,'Mission event recorder chronology and run IDs','DATA_STREAM'),
(81,75,'Requalification run evidence fingerprint','DATA_STREAM'),
(75,79,'Six-objective final completion authorization','DIGITAL_STATUS'),
(79,76,'Mission qualification beacon animation status','DIGITAL_STATUS'),
(77,76,'W129 critical patch diagnostic display only','DIGITAL_STATUS')]
assert len(SC)==16
group('SCADA, event recording and six-objective certification',SC)
assert len(WIRES)==152, len(WIRES)

# 230 visual socket art parents; independently track 304 logical endpoint contacts.
model_usage=Counter()
for W in WIRES:
 model_usage[W['SourceModel']]+=1;model_usage[W['TargetModel']]+=1
eligible=sorted({NAMES.index(n) for n in model_usage})
# apportion anchors proportional to port usage, ensuring at least one on each used model;
# add a few artist sockets to diagnostic stands and service hardware.
counts={i:1 for i in eligible}
for i in (28,67,73,75,77):counts[i]=max(counts.get(i,0),3)
while sum(counts.values())<230:
 idx=max(eligible,key=lambda j:(model_usage[NAMES[j]]/(counts.get(j,0)+.48),-j))
 counts[idx]+=1
assert sum(counts.values())==230
ANCHORS=[];model_anchor=defaultdict(list)
for i in sorted(counts):
 n=counts[i];xx,yy,zz=P[i]
 for j in range(n):
  # Surface socket on mechanical item or instrument (front side +Z)
  width=.46 if i not in (46,47,48,38) else 1.10
  sx=xx+((j%7)-3)*min(.14,width/max(1,n/8));sy=yy+((j//7)%5-1.4)*.108
  sz=zz+(.46 if i>=16 and i not in (28,38,46,47,48) else .22)
  aid='P%03d'%(len(ANCHORS)+1)
  row=dict(AnchorID=aid,Model=NAMES[i],WorldX_m=round(sx,5),WorldY_m=round(sy,5),WorldZ_m=round(sz,5),Status='DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL')
  ANCHORS.append(row);model_anchor[i].append(row)
assert len(ANCHORS)==230
endpoint_counts=defaultdict(int)
LOGICAL=[]
for W in WIRES:
 wn=int(W['WireID'][1:]);W['Critical']='YES' if wn==129 else 'NO'
 W['Group']=next(g['Group'] for g in GROUPS if int(g['StartWire'][1:])<=wn<=int(g['EndWire'][1:]))
 for side in ('Source','Target'):
  i=NAMES.index(W[side+'Model']);inds=model_anchor[i]
  count=endpoint_counts[i];endpoint_counts[i]+=1
  # spread links round modeled socket art without changing their identity
  aid=inds[(count*3+wn)%len(inds)]['AnchorID']
  W[side+'VisualAnchor']=aid
  port='%s_%s_%s'%(W['WireID'],('OUT' if side=='Source' else 'IN'),W['SignalKind'])
  LOGICAL.append(dict(WireID=W['WireID'],Endpoint=side,Model=W[side+'Model'],ArtAnchor=aid,FunctionalPortID=port,
    SignalKind=W['SignalKind'],Direction='OUTPUT' if side=='Source' else 'INPUT',Critical=W['Critical'],
    FunctionalStatus='PROPOSED_PORT_NOT_UNITY_SPARKTERMINAL'))
assert len(LOGICAL)==304
anchor_uses=defaultdict(list)
for lp in LOGICAL:anchor_uses[lp['ArtAnchor']].append(lp)
conflicts=[{'AnchorID':aid,'Model':lp[0]['Model'],'LogicalContactCount':len(lp),
            'Kinds':';'.join(sorted({x['SignalKind'] for x in lp})),
            'DifferentSignalClasses':len({x['SignalKind'] for x in lp})>1,
            'Status':'REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS'}
           for aid,lp in anchor_uses.items() if len(lp)>1]
assert WIRES[128]['Critical']=='YES'

def write_csv(path,rows,fields=None):
 if not rows:return
 with open(path,'w',newline='',encoding='utf-8') as f:
  cols=fields or list(rows[0]);o=csv.DictWriter(f,fieldnames=cols,extrasaction='ignore');o.writeheader();o.writerows(rows)
def write_json(path,d):Path(path).write_text(json.dumps(d,indent=2),encoding='utf8')

OBJECTIVES=[
('1','Build the Orbital Robotics Control Network','All 152 real player wires and real functionally separated SparkTerminal contacts; proper 5V supplies, returns and independent interlocks; W129 valid','Player-connected electrical graph, fresh control inputs, correctly typed ports'),
('2','Commission and Calibrate the Seven-Axis Robot','All seven axes homed; encoder offsets verified; wrist force/torque calibrated; stereo/depth vision aligned; collision-free trial','Physical joint actual states, timestamped calibration record, completed commanded path'),
('3','Extract the Failed Radiator','Coolant isolated and pressure reduced; all bolts disengaged; safe gripper contact; controlled extraction; faulty radiator cradled','Electrical and thermal isolation logs, actual jaw/torque/pose state, safe extraction evidence'),
('4','Install and Restore the Thermal Radiator','Replacement fetched, oscillation damped, insertion aligned and fastened; connectors secured; pump and radiator reject sufficient heat','Real geometry/contact state, fastening evidence, pressure/flow/temperature histories'),
('5','Recover Three Robotic Servicing Failures','Real induced encoder drift, flexible payload vibration, and asymmetric gripper; diagnosis, recovery and fresh service-cassette trial','Deterministic fault logs and new qualified post-repair test run'),
('6','Restore Critical Contact-Force Feedback','After historical 1-5: real W129 disconnect -> stale trusted wrench -> protected stop -> latched fault; reconnect, recalibrate, explicit reset, fresh whole manipulation and cooling qualification','W129 topology evidence, safety event sequence, new runId and actual LevelGamePlayManager 6/6')]

def produce_data():
 write_csv(ROOT/'Art_Assembly.csv',[dict(Index=i,Model=NAMES[i],FBX='%02d_%s.fbx'%(i,NAMES[i]),GLB='%02d_%s.glb'%(i,NAMES[i]),HomeX_m=P[i][0],HomeY_m=P[i][1],HomeZ_m=P[i][2],GeometryPositionSpace='SHARED_WORLD_ORIGIN') for i in range(82)])
 write_csv(ROOT/'Component_Roles.csv',[dict(Index=i,Model=NAMES[i],Role=('Seven-axis robot joint' if 8<=i<=14 else 'Robotic manipulation and sensing' if 15<=i<=45 else 'Radiator and thermal plant' if 46<=i<=63 else 'Fault/safety/power/SCADA' if 64<=i<=81 else 'Station exterior and service arena'),GameplayStatus='ART_ONLY_GAMEPLAY_NOT_YET_IMPLEMENTED') for i in range(82)])
 write_csv(ROOT/'Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2],Group='Robot articulation' if 8<=p['Index']<=14 else 'Mechanical and instrument visual') for p in PIVOTS])
 write_csv(ROOT/'Terminal_Anchor_Plan_230.csv',ANCHORS)
 write_csv(ROOT/'Visual_Terminal_Sockets_230.csv',ANCHORS)
 write_csv(ROOT/'Terminal_Logical_Port_Reconciliation_304.csv',LOGICAL)
 write_csv(ROOT/'Terminal_Anchor_Signal_Conflict_Report.csv',conflicts)
 write_csv(ROOT/'Gameplay_Wiring_Budget.csv',GROUPS)
 write_csv(ROOT/'Gameplay_Wiring_Concept_W001_W152.csv',WIRES)
 write_csv(ROOT/'Objective_Certification_Plan_6.csv',[dict(ObjectiveID=x[0],Title=x[1],RequiredEvidence=x[2],EvidenceCapture=x[3],ImplementationStatus='PLANNED_NOT_IMPLEMENTED') for x in OBJECTIVES])
 (ROOT/'W129_Critical_Contact_Force_Recovery.md').write_text('''# W129 — Trusted Contact Wrench Feedback — Level 86

**SOURCE-CONCEPT ROW (NOT LIVE PLAYER WIRE)**

- Source: `L86_SixAxisWristForceTorqueSensor` at `%s`, logical port `W129_OUT_ANALOG_FEEDBACK`.
- Destination: `L86_RoboticContactSafetyPLC` at `%s`, logical port `W129_IN_ANALOG_FEEDBACK`.
- The real Unity implementation must use a player-created, physically detachable Spark wire and actual solver topology.

## Fault and recovery

When Objectives 1–5 are historically complete, a deliberate actual W129 disconnect invalidates fresh trusted contact wrench. The independent contact safety PLC must prohibit further contact motion, preserve gripper support if holding a payload, withdraw unsafe joint/fastener authorizations and latch a critical fault. No current can become zero by UI convention; previously valid measurements cannot be reused.

After a real wire reconnection, verify power and returns, acquire fresh calibrated six-axis force+torque samples, check overload/clearance/gripper and thermal isolation conditions, then require an **explicit** operator reset. Run a genuinely fresh, complete manipulation, interface and cooling qualification using a replaceable service cassette, recording new `runId`, evidence and event timestamps. Only then may objective 6 certify in the actual `LevelGamePlayManager`.

Objectives 1–5 retain historical certification even when live machine state becomes unsafe. Diagnostic SCADA feedback may never substitute for W129. No Unity runtime, SparkTerminal or actual safety PLC is delivered by this art package.
'''%(WIRES[128]['SourceVisualAnchor'],WIRES[128]['TargetVisualAnchor']),encoding='utf8')
 (ROOT/'Gameplay_Integration_Plan.md').write_text('''# Level 86 — Engineering implementation plan

This is a **design and art kit**, not playable logic.

1. Inspect the actual Project Spark source (`SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `SparkLevelDefinition`, `LevelGamePlayManager`, object shelf, shop, preexisting PLC and safety APIs) before writing adapters. Do not invent signatures.
2. Instantiate **82 modular FBX models at identity origin** under visual-only `L86_HD_VISUAL_ART`; do not instantiate assembled showcase simultaneously. FBX geometry is numeric centimetres, GLB numeric metres, convert exactly once.
3. Register all **230** decorative `SOCKET_P###` art transforms, then reconcile **304 distinct proposed W-wire endpoint contacts**. Shared art anchors can map multiple independent logical ports. Separate power, analog, digital and data domains; NO shorting incompatible signals.
4. Bind **152 actual player-created wires** and verify actual solver continuity and returns. W129 is a deliberately detachable trusted contact force-feedback circuit edge.
5. Implement a single authoritative 7-axis robotic system: actual joint encoder/angle/velocity/torque, motor drive and brake dynamics, forward/inverse kinematics, collision envelope and dynamic joint limits. The 7 `ANIM_ROBOT_*` pivots are separate modular meshes; rebuild a correct parent-child arm hierarchy for visualization and calibration; no direct gameplay qualification based on visual transforms.
6. Model stereo/depth vision sensor bias, calibration and timestamps, actual relative end-effector pose and vision covariance. Real contact must depend on simulated physical geometry, not overlap of visible art alone.
7. Distinguish commanded gripper jaw and joint positions from achieved state; model six-axis wrist wrench, independent hold pressure, tool change, bolt torque and compliant insertion with finite dynamics. A robot cannot release an unsupported payload on a critical sensor fault.
8. Model replacement radiator mass/inertia and flexible oscillation, realistic finite mechanical fastening, coolant isolation pressure/flow/temperature and coherent educational heat balance (`C dT/dt = Q_load - Q_radiator`), not cosmetic HUD cooling.
9. Three real disturbances: encoder bias causes motion error, payload modal excitation produces oscillations and abnormal load, asymmetry changes actual jaw contact. Diagnose/repair and demand new requalification evidence.
10. W129 disconnection must change solver topology; invalidate trusted contact feedback, independently inhibit risky robot motion, latch a fault, then after reconnect require **fresh** sensor calibration, independent safety checks, explicit reset and a new full service-cassette thermal trial. Historical Objectives 1–5 persist.
11. All six objectives require authoritative evidence, `runId` history and real `LevelGamePlayManager` integration. Optional Unity Editor scripts in this ZIP are art-only import/material/validator helpers. They do not constitute a gameplay builder.
12. Validate 82 imported FBXs, 64 named moving/indicator pivots, 230 art sockets, 152 WIDs, 304 independent contacts, collision/LOD/materials and old Levels 1–85. Run actual Unity compilation and Play Mode tests before claiming complete. Procedural reference textures require UV work and shader-channel mapping.
''',encoding='utf8')
 (ROOT/'AAA_Art_Delivery_Notes.md').write_text('''# Project Spark — Level 86 THE ORBITAL FORGE — Original Procedural Mesh Art

Actual triangulated **binary FBX 7400** and **glTF 2.0 GLB** modular meshes. Designed as a high-detail **AAA-inspired production blockout/technical art kit**, not final sculpted/baked cinematic assets. Features seven individually articulated joint modules, unique mechanical structure, servo flange bolts and cabling; two independent multi-channel radiator panels; robotic servicing equipment, thermal plant, service bay, control consoles, holograms, sockets, and pivot-bearing submeshes.

**Not included:** authored production UV unwraps, texture bakes, guaranteed PBR texture mapping, lightmap UVs, finished LODs, collision hulls, gameplay wiring, functional electrical terminals, motor/thermal simulation, actual Unity prefabs or successful Unity compilation/Play Mode tests. Generated 12 procedural texture sets are reference maps only and may require UVs and repacking.

FBX geometry/pivot position numeric units: centimetres (UnitScaleFactor=1); GLB: metres. All modular world-baked geometry has a shared origin. Do not place assembled preview beside individual modular equipment. Any icon or glow geometry is nonfunctional visual art. Never infer completion from the art alone.
''',encoding='utf8')
 (ROOT/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 86 THE ORBITAL FORGE

Contents:
82 modular real 3D FBX files (Models/) and matching GLB files (GLB/).
One assembled showcase FBX/GLB alternative — DO NOT duplicate with modules.
64 actual named animation pivot parent nodes (visual only).
230 actual decorative SOCKET_P001..SOCKET_P230 nodes (NOT SparkTerminal).
152 conceptual W001..W152 wire routes, 304 logical endpoint identities (NOT real wires).
Six objective design definitions, W129 critical contact safety recovery plan.
Optional material/reference textures and Unity art-only scripts.

UNITY IMPORT
1. Copy Models/ to Assets/ProjectSpark/Level86/Models and Textures/ to Assets/ProjectSpark/Level86/Textures.
2. Put Unity_Optional/*Editor.cs in an Editor folder; VisualRig runtime script outside Editor.
3. Select your Level 86 layout GameObject. Run Tools/Project Spark/Levels/86/Place HD Art Under Selected Layout.
4. Run Tools/Project Spark/Levels/86/Validate Imported HD Art.
5. Optionally run Tools/Project Spark/Levels/86/Create & Apply HD Materials To Selected Art.
6. OPTIONAL: attach SparkLevel86VisualRig and bind mechanical/indicator pivots.
7. Build actual gameplay and true SparkTerminal contacts separately. Art import does not produce a playable level.

ALL 82 numbered models are aligned to common world origin. Import FBX vertex/pivot values using ONE centimetre to metre conversion; NEVER scale twice. GLB uses metres.
''',encoding='utf8')

# Bind all decorative sockets to mesh-owning SOCKET_P### pivots.
ANCHOR_BY_MODEL=defaultdict(list)
for an in ANCHORS:ANCHOR_BY_MODEL[an['Model']].append(an)
def art_with_sockets(i):
 a=make_art(i)
 for an in ANCHOR_BY_MODEL[NAMES[i]]:
  a.socket((an['WorldX_m'],an['WorldY_m'],an['WorldZ_m']),an['AnchorID'])
 return a

def homes(i,a):
 out=[(p['Pivot'],a.name,PIVOT_POS[p['Pivot']]) for p in PIVOTS if p['Index']==i]
 out.extend(('SOCKET_'+aid,a.name,pt) for aid,pt in a.sockets)
 return out

def exports():
 start=time.monotonic();models=[];allparts=[];allhome=[]
 bmin=np.array([float('inf')]*3);bmax=-bmin
 for i,n in enumerate(NAMES):
  art=art_with_sockets(i);parts=core.chunk_model(art,max_shapes=16);home=homes(i,art)
  fp=ROOT/'Models'/('%02d_%s.fbx'%(i,n));gp=ROOT/'GLB'/('%02d_%s.glb'%(i,n))
  frep=core.export_fbx(str(fp),[(n,p) for p in parts],[n],home,art.sockets)
  grep=core.export_glb(str(gp),[(n,p) for p in parts],[n],home)
  socket_homes=dict(art.sockets)
  for pt in parts:
   pv=pt['pivot'];center=PIVOT_POS[pv] if pv in PIVOT_POS else socket_homes[pv[7:]] if pv and pv.startswith('SOCKET_') else (0,0,0)
   vv=pt['vertices']+np.asarray(center)
   bmin=np.minimum(bmin,vv.min(axis=0));bmax=np.maximum(bmax,vv.max(axis=0))
  row=dict(index=i,name=n,file=fp.name,glb=gp.name,triangle_count=sum(len(pt['faces']) for pt in parts),
   mesh_parts=len(parts),source_primitive_count=len(art.raw),visual_terminal_sockets=len(art.sockets),
   animation_pivots={pv:[round(float(v),5) for v in xyz] for pv,rt,xyz in home if pv.startswith('ANIM_')},
   fbx_bytes=frep['file_size'],glb_bytes=grep['glb_bytes'])
  models.append(row);allparts.extend((n,p) for p in parts);allhome.extend(home)
  if i%10==0 or i==81:print('EXPORTED',i+1,'/82',n,row['triangle_count'],'triangles',round(time.monotonic()-start,1),'s',flush=True)
 print('EXPORTING assembled FBX and GLB',len(allparts),'mesh groups',flush=True)
 fp=ROOT/'Models'/'Level86_Complete_ArtKit_Showcase.fbx';gp=ROOT/'GLB'/'Level86_Complete_ArtKit_Showcase.glb'
 f=core.export_fbx(str(fp),allparts,NAMES,allhome,[((a['AnchorID']), (a['WorldX_m'],a['WorldY_m'],a['WorldZ_m'])) for a in ANCHORS])
 g=core.export_glb(str(gp),allparts,NAMES,allhome)
 manifest=dict(level_id='level_86',display_name='The Orbital Forge',subtitle='Advanced Space Robotics and Thermal Station Restoration',
  individual_models=len(NAMES),individual_glbs=len(NAMES),assembled_fbx='Models/'+fp.name,assembled_glb='GLB/'+gp.name,
  individual_triangles=sum(m['triangle_count'] for m in models),individual_mesh_parts=sum(m['mesh_parts'] for m in models),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),
  proposed_independent_logical_wire_endpoints=len(LOGICAL),anchor_reconciliation_conflicts=len(conflicts),
  workstation_world_bounds_m=[[round(float(x),5) for x in bmin],[round(float(x),5) for x in bmax]],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up X-width Z-forward',
  assembly_origin=[0,0,0],actual_player_wires_included=False,functional_sparkterminals_included=False,
  actual_robot_dynamics_solver_included=False,actual_thermal_simulator_included=False,six_runtime_objectives_implemented=False,
  unity_compilation_tested=False,unity_playmode_tested=False,assembled_fbx_bytes=f['file_size'],assembled_glb_bytes=g['glb_bytes'],models=models)
 write_json(ROOT/'Model_Manifest.json',manifest)
 return manifest

if __name__=='__main__':
 print('BUILDING PROJECT SPARK LEVEL 86 ORIGINAL FBX & GLB ART',flush=True)
 produce_data();man=exports()
 print('COMPLETE',man['individual_models'],'MODELS',man['individual_triangles'],'TRIANGLES',flush=True)
