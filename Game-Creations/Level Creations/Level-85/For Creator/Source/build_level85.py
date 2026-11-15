#!/usr/bin/env python3
"""Project Spark Level 85 — THE LAST METER: modular 3D art/engineering-plan builder.
Creates NEW original geometries as real binary FBX 7400 + GLB (not an illustration).
Workstation coordinates are metres; FBX mesh numbers centimetres. No Unity gameplay.
Requires numpy, trimesh, Pillow; standalone geometry_export_core.py bundled here.
"""
import os,sys,math,csv,json,time,random,hashlib,zipfile
from pathlib import Path
from collections import defaultdict, Counter
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for k in ('Models','GLB','Textures','Unity_Optional'): (ROOT/k).mkdir(parents=True,exist_ok=True)
random.seed(85085)
NAMES='''
OrbitalDockingArenaBase
PanoramicEarthAndStarfieldViewport
StationOrbitalTrussSpine
StationPrimaryDockingPort
DockingOuterGuideRing
DockingPassiveCaptureCone
StationOpticalTargetMarkerArray
StationSolarWingModule
StationThermalRadiatorWing
StationApproachBeaconCluster
StationServiceAirlockModule
StationDockingPowerInterface
StationCaptureStructuralFrame
ThreeDimensionalApproachCorridorHologram
RelativeOrbitTrajectoryHologram
EarthOrbitalReferenceHologram
ExternalInspectionCameraGantry
StationApproachIlluminationRig
AutonomousServiceSpacecraftBody
ServiceSpacecraftExtendableDockingProbe
ProtectedServiceVehicleAvionics
ServiceSpacecraftPortSolarWing
ServiceSpacecraftStarboardSolarWing
ReactionControlPropellantManifold
RCSPlusXThruster
RCSMinusXThruster
RCSPlusYThruster
RCSMinusYThruster
RCSPlusZThruster
RCSMinusZThruster
PropellantTankA
PropellantTankB
ThrusterValveDriverRack
ThrusterFlowAndPressureMonitor
PrecisionThrusterCalibrationFixture
SpacecraftInertialMeasurementUnit
StereoNavigationCameraLeft
StereoNavigationCameraRight
PrimaryScanningLiDAR
SecondaryScanningLiDAR
ShortRangeRendezvousRadar
DockingBeaconReceiver
OpticalTargetPatternTracker
RelativePositionVelocityEstimator
RelativeNavigationFusionComputer
NavigationUncertaintyVolumeDisplay
PrecisionFlightTimeReferenceClock
RelativeTrajectoryPlanningComputer
ProximityOperationsGuidanceComputer
SixDegreeFreedomFlightController
ThrusterForceAllocationComputer
DockingCorridorSafetyGuardian
CollisionAvoidancePredictionMonitor
FuelReserveAndAbortPredictor
ProtectedAutonomousAbortController
ActiveSoftCaptureRing
MotorizedCaptureGuidePetals
SixActuatorDockingLatchArray
SixAxisContactForceSensor
CompliantContactDampingActuator
DockingElectricalUmbilicalCoupler
PressureInterfaceSealGasket
PressureLeakQualificationController
HardDockStructuralLoadMonitor
DockingMechanismSequencingPLC
PrecisionDockingCameraMonitor
LiDARBlindSpotFaultEmulator
StuckThrusterValveFaultEmulator
AsymmetricLatchFaultEmulator
CriticalClosingRateFeedbackStation
DockingSafetyPLC
Protected5VControlSupply
DockingPowerDistributionCabinet
HolographicRendezvousSCADA
ElectricalTerminalDiagnosticRack
EmergencyApproachHoldStation
DockingMissionCertificationBeacon
PrecisionFlightEventDataRecorder'''.strip().splitlines()
NAMES=['L85_'+n for n in NAMES]
assert len(NAMES)==78,len(NAMES)
C=core.COLORS={
'structural':(.32,.45,.57,1),'titanium':(.66,.72,.78,1),'dark':(.065,.09,.14,1),
'graphite':(.10,.14,.205,1),'copper':(.82,.40,.13,1),'gold':(.97,.72,.24,1),
'ceramic':(.80,.85,.89,1),'glass':(.18,.66,.94,.45),'panel':(.026,.11,.23,1),
'pcb':(.060,.22,.19,1),'cyan':(.18,.85,.98,1),'blue':(.20,.44,.94,1),
'green':(.14,.94,.61,1),'amber':(1.,.63,.17,1),'violet':(.62,.42,.98,1),
'red':(1.,.20,.19,1),'orange':(1.,.40,.10,1),'white':(.90,.96,1.,1),
'solar':(.03,.095,.31,1),'stars':(.66,.78,1.0,1),'black':(.022,.026,.036,1),
}
core.MATERIALS={k:core.pbrmat(k) for k in C}
# All modular assets authored at the SAME world origin; per-model geom uses these world locations.
P={
0:(0,.12,0),1:(0,5.3,-6.65),2:(6.6,5.15,-1.45),3:(3.35,5.05,-1.45),4:(3.15,5.05,-1.45),
5:(2.92,5.05,-1.45),6:(3.0,5.05,-1.45),7:(9.1,6.65,-1.3),8:(10.5,3.65,-1.3),
9:(5.8,5.05,-1.4),10:(8.0,5.05,-1.5),11:(5.2,3.8,.15),12:(4.2,5.05,-1.45),
13:(0,5.05,-1.45),14:(0,7.8,-5.3),15:(-9.8,7.65,-5.8),16:(5.8,7.1,-.55),17:(1.0,7.1,-3.1),
18:(-4.8,5.05,-1.45),19:(-2.83,5.05,-1.45),20:(-4.9,5.05,-1.05),21:(-5.3,6.35,-1.4),22:(-5.3,3.75,-1.4),
23:(-5.5,4.65,-.4),24:(-4.2,5.77,-.5),25:(-5.7,4.38,-.5),26:(-4.8,5.95,-1.6),27:(-4.8,4.12,-1.6),28:(-5.45,5.05,.25),29:(-4.25,5.05,-2.9),
30:(-5.68,5.05,-1.9),31:(-5.1,5.05,-1.9),32:(-11.4,1.72,4.6),33:(-10.03,1.72,4.6),34:(-8.63,1.72,4.6),
35:(-3.75,4.55,-.92),36:(-3.25,5.37,-.9),37:(-3.25,4.75,-.9),38:(-2.9,5.57,-1.05),39:(-2.9,4.55,-1.05),
40:(-3.2,5.05,-2.15),41:(-2.3,6.2,-1.3),42:(-2.33,4.7,-1.3),
43:(-7.25,1.72,4.6),44:(-5.91,1.72,4.6),45:(-8.2,6.6,-5.7),46:(-4.57,1.72,4.6),
47:(-3.22,1.72,4.6),48:(-1.87,1.72,4.6),49:(-.52,1.72,4.6),50:(.83,1.72,4.6),
51:(2.18,1.72,4.6),52:(3.53,1.72,4.6),53:(4.88,1.72,4.6),54:(6.23,1.72,4.6),
55:(2.65,5.05,-1.45),56:(2.6,5.05,-1.45),57:(2.65,5.05,-1.45),58:(2.72,5.05,-1.45),59:(2.6,5.05,-1.45),
60:(2.6,5.05,-1.45),61:(2.65,5.05,-1.45),62:(7.62,1.72,4.6),63:(2.75,5.05,-1.45),
64:(9.0,1.72,4.6),65:(10.1,4.2,-5.5),66:(10.32,1.72,4.6),67:(11.68,1.72,4.6),68:(8.35,3.72,4.6),
69:(-11.34,3.6,4.6),70:(-10.00,3.6,4.6),71:(-8.66,3.6,4.6),72:(-7.32,3.6,4.6),
73:(0,8.5,-5.72),74:(-5.97,3.6,4.6),75:(-4.62,3.6,4.6),76:(0,9.2,-5.75),77:(-3.27,3.6,4.6)
}
assert set(P)==set(range(78))
# Each pivot is planned and will have actual mesh-owned transform in FBX + GLB.
PIVOT_DEFS='''
ANIM_STATION_DOCK_RING_GLOW|3|indicator|none
ANIM_OUTER_DOCK_GUIDE|4|rotation|x
ANIM_PASSIVE_CAPTURE_CONE|5|translation|x
ANIM_TARGET_MARKER_SCAN|6|indicator|none
ANIM_STATION_SOLAR_TRACK|7|rotation|y
ANIM_RADIATOR_DEPLOY|8|rotation|x
ANIM_STATION_BEACON_ROTATION|9|rotation|x
ANIM_STATION_SERVICE_HATCH|10|rotation|y
ANIM_POWER_UMBILICAL_PORT|11|translation|x
ANIM_STATION_CAPTURE_FRAME|12|indicator|none
ANIM_APPROACH_CORRIDOR_VECTOR|13|indicator|none
ANIM_RELATIVE_TRAJECTORY|14|indicator|none
ANIM_EARTH_REFERENCE|15|rotation|y
ANIM_INSPECTION_CAMERA_PAN|16|rotation|y
ANIM_INSPECTION_LIGHT_FLOOD|17|rotation|x
ANIM_SERVICE_SPACECRAFT_BODY|18|quaternion|xyz
ANIM_SERVICE_DOCK_PROBE|19|translation|x
ANIM_SERVICE_PORT_SOLAR|21|rotation|x
ANIM_SERVICE_STARBOARD_SOLAR|22|rotation|x
ANIM_RCS_PLUS_X|24|indicator|none
ANIM_RCS_MINUS_X|25|indicator|none
ANIM_RCS_PLUS_Y|26|indicator|none
ANIM_RCS_MINUS_Y|27|indicator|none
ANIM_RCS_PLUS_Z|28|indicator|none
ANIM_RCS_MINUS_Z|29|indicator|none
ANIM_THRUSTER_VALVE_BANK|32|translation|x
ANIM_FLOW_MONITOR|33|indicator|none
ANIM_THRUST_CALIBRATION_RIG|34|rotation|y
ANIM_IMU_REFERENCE|35|rotation|z
ANIM_STEREO_CAMERA_LEFT|36|rotation|y
ANIM_STEREO_CAMERA_RIGHT|37|rotation|y
ANIM_LIDAR_A_SCANNER|38|rotation|y
ANIM_LIDAR_B_SCANNER|39|rotation|y
ANIM_RADAR_DISH|40|rotation|y
ANIM_BEACON_RECEIVER|41|indicator|none
ANIM_TARGET_PATTERN_TRACK|42|indicator|none
ANIM_RELATIVE_VELOCITY_VECTOR|43|indicator|none
ANIM_NAV_SENSOR_FUSION|44|indicator|none
ANIM_NAV_UNCERTAINTY_CONE|45|indicator|none
ANIM_TRAJECTORY_PLANNER|47|indicator|none
ANIM_GUIDANCE_SOLUTION|48|indicator|none
ANIM_SIX_DOF_CONTROL|49|indicator|none
ANIM_THRUSTER_ALLOCATION|50|indicator|none
ANIM_COLLISION_BOUNDARY|52|indicator|none
ANIM_ABORT_TRAJECTORY|54|indicator|none
ANIM_ACTIVE_SOFT_CAPTURE_RING|55|translation|x
ANIM_CAPTURE_GUIDE_PETALS|56|rotation|x
ANIM_DOCKING_LATCH_BANK|57|translation|x
ANIM_CONTACT_DAMPING|59|translation|x
ANIM_ELECTRICAL_UMBILICAL|60|translation|x
ANIM_PRESSURE_SEAL|61|translation|x
ANIM_DOCK_CAMERA_FEED|65|indicator|none
ANIM_LIDAR_BLINDSPOT_FAULT|66|translation|x
ANIM_STUCK_VALVE_FAULT|67|translation|x
ANIM_ASYMMETRIC_LATCH_FAULT|68|translation|x
ANIM_W123_TRUSTED_FEEDBACK|69|indicator|none
ANIM_INDEPENDENT_DOCK_SAFETY|70|indicator|none
ANIM_APPROACH_EMERGENCY_HOLD|75|translation|z
ANIM_RENDEZVOUS_SCADA|73|indicator|none
ANIM_DOCK_CERTIFICATION|76|indicator|none
'''.strip().splitlines()
PIVOTS=[]
for s in PIVOT_DEFS:
 n,i,kind,axis=s.split('|');PIVOTS.append(dict(Pivot=n,Index=int(i),Model=NAMES[int(i)],Kind=kind,Axis=axis))
assert len(PIVOTS)==60,len(PIVOTS)
PIVOT_OWNER={p['Pivot']:p['Index'] for p in PIVOTS}
PIVOT_POS=core.PIVOT_POS={p['Pivot']:P[p['Index']] for p in PIVOTS}
# Special pivot centres on animated physics objects, at exact world positions.
for name,xyz in {'ANIM_SERVICE_SPACECRAFT_BODY':P[18],'ANIM_STATION_SOLAR_TRACK':(8.1,5.6,-1.3),'ANIM_ACTIVE_SOFT_CAPTURE_RING':(2.66,5.05,-1.45),'ANIM_CAPTURE_GUIDE_PETALS':(2.66,5.05,-1.45),'ANIM_DOCKING_LATCH_BANK':(2.66,5.05,-1.45),'ANIM_SERVICE_DOCK_PROBE':(-2.83,5.05,-1.45)}.items():PIVOT_POS[name]=xyz

def box(a,p,s,mat='titanium',pivot=None,label='PrecisionCNC'):a.box(p,s,mat,pivot,label)
def cyl(a,p,r,h,mat='titanium',axis='z',pivot=None,label='Cylinder',seg=28):a.cylinder(p,r,h,mat,axis=axis,seg=seg,pivot=pivot,label=label)
def tor(a,p,R,r,mat='titanium',axis='x',pivot=None,label='MachineRing',seg=56):a.torus(p,R,r,mat,axis=axis,seg=seg,tube=9,pivot=pivot,label=label)
def beam(a,p1,p2,r=.04,mat='titanium',pivot=None,label='Brace'):a.beam(p1,p2,r,mat,sides=10,pivot=pivot,label=label)
def ball(a,p,r,mat='ceramic',pivot=None,label='SphericalPressureVessel',subs=2):
 m=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(m.vertices+np.array(p),m.faces,mat,pivot,label)
def cone(a,p,r1,r2,length,mat='titanium',axis='x',pivot=None,label='TaperedHousing',seg=40):
 ax={'x':0,'y':1,'z':2}[axis];u=(ax+1)%3;v=(ax+2)%3;verts=[];faces=[]
 for side,r in [(-length/2,r1),(length/2,r2)]:
  for k in range(seg):
   an=2*math.pi*k/seg;q=np.zeros(3);q[ax]=side;q[u]=r*math.cos(an);q[v]=r*math.sin(an);verts.append((np.array(p)+q).tolist())
 for k in range(seg):
  j=(k+1)%seg;faces.extend([[k,j,seg+k],[j,seg+j,seg+k]])
 a.mesh(verts,faces,mat,pivot,label)

def panel(a,p,scale=1.,theme='cyan',kind='instrument'):
 core.panel_equipment(a,p,scale=scale,theme=theme,special=kind)
 x,y,z=p
 for k in range(4):
  cyl(a,(x-.36+.24*k,y-.52*scale,z+.42*scale),.052*scale,.08*scale,'gold',axis='z',label='GoldRimmedCableGland',seg=12)
 for xoff in (-.43,.43):
  box(a,(x+xoff,y+.6*scale,z-.04),(.08,.09,.45),'titanium',label='RackMechanicalHandle')

def spacecraft_hull(a,p,pivot=None):
 x,y,z=p
 # octagonal fuselage centered at x, longitudinal axis x
 cone(a,(x,y,z),.90,.94,3.45,'ceramic','x',pivot,'SpacecraftPrimaryPressureBus',seg=32)
 cone(a,(x-1.35,y,z),.85,.75,.55,'titanium','x',pivot,'AftThrusterBulkhead')
 cone(a,(x+1.55,y,z),.90,.66,.7,'titanium','x',pivot,'DockingNosePressureHull')
 for j in range(8):
  ang=j*math.tau/8
  yy=y+.88*math.cos(ang);zz=z+.88*math.sin(ang)
  beam(a,(x-1.55,yy,zz),(x+1.65,yy,zz),.052,'gold',pivot,'LongitudinalAvionicsRib')
  for k in (-1.3,-.1,1.2):
   cyl(a,(x+k,yy,zz),.06,.10,'dark','x',pivot,'HullFastener')
 for k in range(6):
  xx=x-1.25+.5*k
  tor(a,(xx,y,z),.94,.026,'graphite','x',pivot,'StructuralHullBand',seg=40)
 for k in range(12):
  xx=x-1.15+(k%6)*.45;yy=y+(.94 if k<6 else -.94)
  box(a,(xx,yy,z),(.24,.035,.42),'solar' if k%3 else 'titanium',pivot,'ThermalTile')
 for sx in (-1,1):
  box(a,(x+.22,y,z+sx*.99),(.96,.46,.10),'graphite',pivot,'ExternalEquipmentRail')
  for k in range(4):box(a,(x-.1+.22*k,y,z+sx*1.06),(.11,.30,.022),'gold',pivot,'EquipmentBusTrack')

def dock_ring(a,p,R=1.5,depth=.30,pivot=None,actuated=False):
 x,y,z=p
 tor(a,(x,y,z),R,.18,'titanium','x',pivot,'DockingStructureRing',seg=80)
 tor(a,(x+.14,y,z),R-.20,.045,'gold','x',pivot,'DockingSealGuide',seg=80)
 tor(a,(x+.24,y,z),R-.27,.035,'cyan','x',pivot,'DockingStatusRing',seg=80)
 for j in range(24):
  t=j*math.tau/24;yy=y+R*math.cos(t);zz=z+R*math.sin(t)
  cyl(a,(x+.20,yy,zz),.046,.11,'gold' if j%3==0 else 'dark','x',pivot,'RadialCaptureBolt',seg=10)
 for j in range(8):
  t=j*math.tau/8
  p1=(x,y+(R+.10)*math.cos(t),z+(R+.10)*math.sin(t))
  p2=(x-depth,y+(R+.48)*math.cos(t),z+(R+.48)*math.sin(t))
  beam(a,p1,p2,.07,'structural',pivot,'MachinedDockSupport')
 if actuated:
  for j in range(6):
   t=j*math.tau/6
   cyl(a,(x+.36,y+R*.89*math.cos(t),z+R*.89*math.sin(t)),.14,.28,'gold','x',pivot,'HardDockCamLatch')

def sensor(a,p,typ='optical',pivot=None):
 x,y,z=p
 box(a,(x-.13,y,z-.12),(.70,.63,.78),'graphite',pivot,'AvionicsSensorHousing')
 cyl(a,(x+.26,y,z),.27,.48,'titanium','x',pivot,'OpticalInstrumentBarrel')
 tor(a,(x+.52,y,z),.235,.04,'gold','x',pivot,'OpticsMachinedBezel',seg=44)
 cyl(a,(x+.56,y,z),.194,.034,'glass','x',pivot,'RecessedSensorWindow')
 tor(a,(x+.60,y,z),.17,.015,'cyan','x',pivot,'InstrumentReferenceReticle',seg=40)
 for j in range(10):
  t=j*math.tau/10
  cyl(a,(x+.44,y+.28*math.cos(t),z+.28*math.sin(t)),.032,.045,'gold','x',pivot,'SensorOpticalMountBolt',seg=8)
 if typ=='lidar':
  for k in range(3):tor(a,(x+.17+.16*k,y,z),.28,.028,'blue' if k%2 else 'structural','x',pivot,'LidarScanRing',seg=44)
 elif typ=='radar':
  cone(a,(x+.66,y,z),.35,.18,.22,'titanium','x',pivot,'RendezvousRadarRadome')
 elif typ=='camera':
  for j in range(2):cyl(a,(x+.65,y+(-.16 if j==0 else .16),z),.08,.1,'dark','x',pivot,'StereoSubLens')

def rcs_thruster(a,p,pivot=None):
 x,y,z=p
 cyl(a,(x,y,z),.25,.65,'graphite','x',None,'PropulsionValveSleeve')
 cone(a,(x+.27,y,z),.18,.39,.56,'titanium','x',None,'BellNozzle')
 tor(a,(x+.57,y,z),.37,.045,'gold','x',None,'ThermalNozzleLip')
 cyl(a,(x-.48,y,z),.16,.28,'copper','x',None,'ThrusterRegulatorBody')
 for j in range(8):
  t=j*math.tau/8
  beam(a,(x-.50,y+.22*math.cos(t),z+.22*math.sin(t)),(x+.3,y+.22*math.cos(t),z+.22*math.sin(t)),.018,'gold',None,'InjectorFeedLine')
 box(a,(x+.68,y,z),(.06,.18,.18),'cyan',pivot,'ActualThrusterActivityMesh')

# 78 category-tailored, original procedural equipment models.
def art_for(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i]
 ps=[p['Pivot'] for p in PIVOTS if p['Index']==i]
 pivot=ps[0] if ps else None
 if i==0: # massive arena floor + inspection rails and workstations
  box(a,(0,.13,0),(29.2,.26,15.7),'dark',label='DockingQualificationArenaSlab')
  box(a,(0,.277,0),(28.8,.028,15.28),'graphite',label='MachinedCleanroomFloor')
  for ix in range(-13,14):
   for iz in range(-7,8):
    if ix%2==0 and iz%2==0:
     box(a,(ix,.298,iz),(.90,.010,.86),'structural' if (ix+iz)%3==0 else 'dark',label='AccessFloorPlate')
  for ix in (-14.15,14.15):
   for j in range(40):box(a,(ix,.32,-7.2+j*.36),(.10,.026,.24),'amber' if j%2==0 else 'graphite',label='IndustrialCautionMark')
  for iz in (-7.4,7.4):
   for j in range(67):box(a,(-14.15+.43*j,.31,iz),(.21,.028,.05),'cyan' if j%5==0 else 'structural',label='BlueDeckRunway')
  # pedestal row supports six cable galleries
  for yy in (1.00,2.63):
   for sign in (-1,1):beam(a,(-13.6,yy,sign*4.88),(13.6,yy,sign*4.88),.062,'titanium',label='WorkstationServiceRail')
  for j in range(38):
   xx=-13.2+.715*j
   box(a,(xx,1.03,3.95),(.08,1.48,.10),'structural',label='FrontElectronicsPedestalRib')
  for j in range(20):
   xx=-12.8+1.34*j
   box(a,(xx,.61,5.6),(.70,.14,.62),'graphite',label='WorkstationFloorFoot')
 elif i==1:
  # several rim sections and starfield without fake Earth image textures
  box(a,(0,5.2,-6.82),(27.4,9.4,.22),'black',label='DeepSpacePanoramaBackground')
  for rad in (4.1,4.7):tor(a,(0,5.3,-6.64),rad,.048,'structural','z',None,'PanoramaOrbitalReferenceGrid',seg=96)
  for j in range(13):
   xx=-13.0+j*2.16;beam(a,(xx,.65,-6.5),(xx,9.60,-6.5),.055,'structural',label='WindowPressureFrame')
  for j in range(310):
   xx=random.uniform(-13.2,13.2);yy=random.uniform(.65,9.9);r=random.choice([.013,.016,.020,.029])
   box(a,(xx,yy,-6.682),(r,r,.009),'stars' if j%4 else 'white',label='DeepSkyReferenceStar')
  for j in range(9):
   xx=-12.3+j*3.05
   box(a,(xx,9.67,-6.25),(2.75,.13,.19),'titanium',label='PanoramaUpperPressureBeam')
 elif i==2:
  box(a,(7.8,5.05,-1.50),(7.7,1.68,1.85),'ceramic',label='StationPressurizedCentralTrunk')
  for k in range(11):
   xx=4.25+.71*k;tor(a,(xx,5.05,-1.5),1.32,.076,'titanium','x',label='OrbitalStationPressureFrame',seg=50)
  for theta in range(0,360,45):
   t=math.radians(theta);pp=(5.0,5.05+1.28*math.cos(t),-1.5+1.28*math.sin(t));qq=(10.7,pp[1],pp[2])
   beam(a,pp,qq,.075,'structural',label='StationExteriorTrussChord')
  for xx in (5.2,7.1,9.0):
   for theta in range(0,360,90):
    t=math.radians(theta);beam(a,(xx,5.05,-1.5),(xx+1,5.05+1.4*math.cos(t),-1.5+1.4*math.sin(t)),.032,'gold',label='StationCrossBrace')
  for j in range(18):
   box(a,(4.55+.31*j,5.94,-1.5),(.21,.06,.67),'solar' if j%3==0 else 'graphite',label='StationThermalPanel')
 elif i==3:
  dock_ring(a,P[i],1.56,.48,pivot)
  cone(a,(3.7,5.05,-1.45),1.6,1.35,.55,'graphite','x',None,'FixedPassiveDockSleeve')
  tor(a,(3.23,5.05,-1.45),1.07,.064,'green','x',pivot,'DockingQualificationLight',seg=80)
 elif i==4:
  dock_ring(a,P[i],1.89,.45,pivot)
  for k in range(12):
   t=k*math.tau/12
   beam(a,(3.08,5.05+1.75*math.cos(t),-1.45+1.75*math.sin(t)),(2.45,5.05+1.47*math.cos(t),-1.45+1.47*math.sin(t)),.08,'structural',pivot,'ActiveApproachGuideRib')
 elif i==5:
  cone(a,P[i],1.20,1.46,.62,'titanium','x',pivot,'PrecisionPassiveCaptureFunnel',seg=66)
  tor(a,(2.62,5.05,-1.45),1.36,.08,'gold','x',pivot,'FloatingCaptureLip',seg=70)
 elif i==6:
  for k in range(16):
   t=k*math.tau/16
   yy=5.05+1.42*math.cos(t);zz=-1.45+1.42*math.sin(t)
   box(a,(2.93,yy,zz),(.09,.10,.11),'white' if k%2 else 'green',pivot,'DockingOpticalFiducial')
   cyl(a,(2.94,yy,zz),.039,.045,'gold','x',None,'FiducialHolder',seg=10)
 elif i in (7,8):
  xx=9.15 if i==7 else 10.58;yy=6.8 if i==7 else 3.36
  for row in range(2):
   box(a,(xx,yy+row*.57,-1.3),(4.36,.53,.07),'graphite',pivot,'StationArraySupportFrame')
   for k in range(18):
    dx=-2.1+k*.242
    box(a,(xx+dx,yy+row*.57,-1.245),(.210,.45,.014),'solar' if i==7 else 'titanium',pivot,'PVorRadiatorSegment')
    box(a,(xx+dx,yy+row*.57,-1.223),(.009,.42,.009),'gold' if i==7 else 'cyan',pivot,'CellOrRadiatorBondTrack')
  beam(a,(7.3,yy+.3,-1.3),(11.4,yy+.3,-1.3),.056,'titanium',pivot,'SpaceStationWingSpar')
 elif i==9:
  for k in range(8):
   t=k*math.tau/8;yy=5.05+1.94*math.cos(t);zz=-1.45+1.94*math.sin(t)
   cyl(a,(5.0,yy,zz),.135,.42,'titanium','x',pivot,'DockBeaconHousing',seg=20)
   cyl(a,(4.72,yy,zz),.095,.045,'amber' if k%2 else 'green','x',pivot,'StationDockingBeaconGlass',seg=16)
 elif i==10:
  cone(a,(8.6,5.05,-1.45),.98,1.07,2.12,'ceramic','x',None,'ServiceAirlockPressurizedDrum')
  tor(a,(7.48,5.05,-1.45),1.08,.12,'structural','x',pivot,'StationAirlockHinge',seg=60)
  box(a,(7.35,5.05,-1.45),(.20,1.38,1.34),'titanium',pivot,'ServiceHatchOpeningPanel')
  for k in range(12):cyl(a,(7.41,5.05+.94*math.cos(k*math.tau/12),-1.45+.94*math.sin(k*math.tau/12)),.044,.10,'gold','x',pivot,'HatchFastener',seg=10)
 elif i==11:
  panel(a,P[i],.94,'green','plc')
  for k in range(12):box(a,(x-.4+k*.07,y-.37,z+.52),(.05,.07,.08),'copper',label='DockingBusConnectorPin')
 elif i==12:
  dock_ring(a,P[i],2.08,.75,None)
  for k in range(4):
   t=k*math.pi/2
   beam(a,(4.4,5.05+2*math.cos(t),-1.45+2*math.sin(t)),(6.3,5.05+2*math.cos(t),-1.45+2*math.sin(t)),.13,'structural',label='DockCaptureFoundationStrut')
 elif i==13:
  for r in (.35,.7,1.1,1.55):tor(a,P[i],r,.024,'cyan' if r!=1.55 else 'amber','x',pivot,'ApproachDistanceThreshold',seg=72)
  for k in range(14):
   xx=-2.4+k*.35;rr=1.22-(k/14)*.47
   tor(a,(xx,5.05,-1.45),rr,.012,'green' if k%3 else 'cyan','x',pivot,'ApproachCorridorGuidanceRing',seg=44)
  beam(a,(-2.6,5.05,-1.45),(2.6,5.05,-1.45),.019,'green',pivot,'RendezvousBoresightAxis')
 elif i==14:
  for rr in (1.0,1.6,2.3):tor(a,P[i],rr,.028,'cyan','z',pivot,'OrbitalRelativeTrajectoryGrid',seg=92)
  for k in range(23):
   t=k*math.tau/23
   box(a,(2*math.cos(t),7.8+.52*math.sin(t),-5.27),(.07,.09,.015),'green' if k%3 else 'amber',pivot,'OrbitTrajectoryPredictionMarker')
 elif i==15:
  ball(a,P[i],.88,'blue',pivot,'EarthGlobeOrbitalReference',subs=3)
  for rr in (1.1,1.34):tor(a,P[i],rr,.031,'cyan','z',pivot,'OrbitalReferenceVector',seg=70)
 elif i in (16,17):
  pp=P[i]
  beam(a,(pp[0]-.82,pp[1]+.2,pp[2]),(pp[0]+.82,pp[1]+.2,pp[2]),.1,'titanium',None,'InstrumentationCameraBridge')
  sensor(a,pp,'camera',pivot)
  for k in range(3):box(a,(x-.42+k*.38,y-.54,z+.20),(.22,.09,.18),'cyan',pivot,'InspectionLampOrCameraLight')
 elif i==18:
  spacecraft_hull(a,P[i],pivot)
 elif i==19:
  cyl(a,(x+.12,y,z),.34,.95,'titanium','x',pivot,'ExtendableActiveDockingProbe',seg=36)
  tor(a,(x+.67,y,z),.38,.065,'gold','x',pivot,'DockProbeMatingHead')
  cone(a,(x+.83,y,z),.35,.18,.35,'ceramic','x',pivot,'CaptureProbeTaper')
 elif i==20:
  box(a,(x,y,z),(1.63,.75,.72),'graphite',None,'ProtectedFlightAvionicsHousing')
  for k in range(4):
   box(a,(x-.58+.39*k,y+.40,z+.4),(.30,.22,.17),'gold',None,'FourChipletOnboardCompute')
   for j in range(5):box(a,(x-.68+.39*k+j*.035,y+.56,z+.42),(.018,.04,.08),'copper',label='CoPackagedOpticalInterposerBond')
 elif i in (21,22):
  pp=P[i]
  box(a,pp,(2.85,.068,1.07),'graphite',pivot,'DeployableServiceSolarWing')
  for k in range(10):
   for row in range(3):
    box(a,(x-1.29+k*.283,y+.042,z-.37+row*.375),(.25,.016,.325),'solar',pivot,'TripleJunctionPhotovoltaicTile')
    box(a,(x-1.29+k*.283,y+.058,z-.37+row*.375),(.011,.009,.31),'gold',pivot,'PVBusbar')
  for k in range(5):beam(a,(x-1.4+k*.7,y-.05,z-.55),(x-1.4+k*.7,y-.05,z+.55),.035,'titanium',pivot,'SolarFrameCrossSpar')
 elif i==23:
  box(a,(x,y,z),(1.44,.66,.76),'graphite',None,'RCSPropellantDistributionBlock')
  for k in range(6):cyl(a,(x-.58+.23*k,y+.36,z),.11,.24,'copper','y',pivot,'SolenoidPropellantValve',seg=14)
  for k in range(5):beam(a,(x-.52+k*.22,y,z+.42),(x-.52+k*.22,y+.52,z+.42),.037,'gold',label='FeedManifoldPipe')
 elif 24<=i<=29:
  rcs_thruster(a,P[i],pivot)
 elif i in (30,31):
  ball(a,P[i],.48,'structural',pivot,'ServiceHydrazineFreeFictionalPropellantTank',subs=3)
  for yy in (-.32,.32):tor(a,(x,y+yy,z),.36,.04,'gold','y',label='TankMountingRing',seg=44)
  beam(a,(x-.67,y,z),(x+.67,y,z),.06,'copper',label='FictionalPropellantSupplyLine')
 elif 35<=i<=42:
  typ='lidar' if i in (38,39) else 'radar' if i==40 else 'camera'
  sensor(a,P[i],typ,pivot)
 elif 55<=i<=61 or i==63:
  if i==55:dock_ring(a,P[i],1.06,.36,pivot,True)
  elif i==56:
   for j in range(8):
    t=j*math.tau/8;yy=y+.93*math.cos(t);zz=z+.93*math.sin(t)
    beam(a,(x-.2,yy,zz),(x+.42,y+.52*math.cos(t),z+.52*math.sin(t)),.09,'titanium',pivot,'CompliantGuidePetal')
    cyl(a,(x-.26,yy,zz),.14,.16,'gold','x',pivot,'MotorizedPetalPin')
  elif i==57:
   for j in range(6):
    t=j*math.tau/6;yy=y+.92*math.cos(t);zz=z+.92*math.sin(t)
    box(a,(x+.32,yy,zz),(.46,.18,.22),'graphite',pivot,'IndependentHardDockLatchHousing')
    cyl(a,(x+.57,yy,zz),.10,.10,'gold','x',pivot,'LockingLatchCam',seg=18)
    box(a,(x+.38,yy,zz+.17),(.22,.045,.04),'green',pivot,'LatchEngagedSensor')
  elif i==58:
   for j in range(6):cyl(a,(x+.25,y+.87*math.cos(j*math.tau/6),z+.87*math.sin(j*math.tau/6)),.09,.28,'titanium','x',pivot,'SixAxisCompressiveForceSensor',seg=20)
  elif i==59:
   for j in range(8):
    t=j*math.tau/8;yy=y+.82*math.cos(t);zz=z+.82*math.sin(t)
    cyl(a,(x+.05,yy,zz),.095,.42,'graphite','x',pivot,'AxialCaptureDamper',seg=16)
    cyl(a,(x+.29,yy,zz),.055,.27,'gold','x',pivot,'ContactDampingPiston',seg=16)
  elif i==60:
   box(a,(x+.14,y,z),(.36,.35,.46),'graphite',pivot,'DockingUmbilicalConnectorHousing')
   for k in range(16):
    cy=y+((k//4)-1.5)*.07;cz=z+((k%4)-1.5)*.10
    cyl(a,(x+.41,cy,cz),.024,.14,'gold','x',pivot,'HermeticElectricalConnectorPin',seg=10)
  elif i==61:
   tor(a,P[i],1.05,.095,'graphite','x',pivot,'DockingPressureElastomerSeal',seg=80)
   tor(a,(x+.06,y,z),.98,.047,'amber','x',pivot,'SealCompressionStatusRing',seg=80)
  else:
   dock_ring(a,P[i],1.28,.22,None)
   for k in range(12):box(a,(x+.24,y+1.19*math.cos(k*math.tau/12),z+1.19*math.sin(k*math.tau/12)),(.1,.07,.07),'green',label='StructuralStrainBridge')
 elif i==45:
  for r in (.37,.78,1.05):tor(a,P[i],r,.028,'cyan','z',pivot,'NavigationUncertaintyEnvelope',seg=62)
  for j in range(15):
   t=j*math.tau/15
   beam(a,P[i],(x+r*math.cos(t),y+r*math.sin(t),z+.02),.015,'blue',pivot,'CovarianceDirection')
 elif i==73:
  box(a,(0,8.25,-5.78),(9.5,2.9,.24),'dark',None,'OverheadMissionSCADA')
  box(a,(0,8.20,-5.61),(9.11,2.55,.038),'panel',pivot,'RendezvousLiveTelemetryGlass')
  for k in range(17):
   box(a,(-4.3+k*.54,8.55+(.21 if k%2 else -.1),-5.57),(.41,.026,.024),'cyan' if k%3 else 'green',pivot,'MissionVisualChannel')
  for j in range(6):tor(a,(-3.55+j*1.4,7.85,-5.54),.29,.023,'green' if j<4 else 'amber','z',pivot,'ObjectiveStatusWheel',seg=34)
 elif i==76:
  cyl(a,(x,y-.2,z),.45,.44,'gold','y',pivot,'CertifiedDockingMissionBeacon',seg=48)
  tor(a,(x,y+.15,z),.57,.056,'green','y',pivot,'SixOutOfSixAwardRing',seg=70)
  cone(a,(x,y+.25,z),.25,.06,.36,'cyan','y',pivot,'CertificationLuminousSpire',seg=32)
 else:
  # bespoke game-grade cabinets, instrumentation, PLC, diagnostics and disturbance injection.
  theme='red' if i in (66,67,68,75) else ('green' if i in (43,44,64,70) else ('amber' if i in (51,52,53,54,69) else 'cyan'))
  special='fault' if i in (66,67,68,75) else 'plc' if i in (49,50,64,70,71,72,74) else 'instrument'
  panel(a,P[i],.94,theme,special)
  if i in (32,33,34):
   for j in range(6):
    xx=x-.34+j*.137
    cyl(a,(xx,y+.59,z+.16),.049,.22,'gold','z',pivot if i==34 else None,'ActuatorCalibrationGauge',seg=12)
  if i in (69,70,71,72,74,75,77):
   for j in range(9):box(a,(x-.4+j*.10,y-.47,z+.48),(.062,.073,.045),'gold' if j%4==0 else 'titanium',label='SafetyIOFemaleTerminalBlock')
  if i in (66,67,68):
   box(a,(x+.2,y+.43,z+.47),(.15,.15,.11),'red',pivot,'InjectedDisturbanceActuator')
  if i in (47,48,49,50,51,52,53,54):
   tor(a,(x+.14,y+.56,z+.3),.15,.023,'cyan','z',pivot,'GuidanceRealtimeStatusDial',seg=32)
 # Each announced animation pivot MUST own a nonempty actual mesh; use status indicator if none exists.
 used={raw[3] for raw in a.raw if raw[3] is not None}
 for pv in ps:
  if pv not in used:
   xx,yy,zz=PIVOT_POS[pv]
   box(a,(xx,yy,zz+.085),(.17,.12,.06),'cyan' if next(t['Kind'] for t in PIVOTS if t['Pivot']==pv)=='indicator' else 'gold',pv,'PivotMeshAttachment')
 return a

WIRES=[];GROUPS=[]
def w(src,dst,purpose,kind='DIGITAL_CONTROL'):
 WIRES.append(dict(WireID='W%03d'%(len(WIRES)+1),SourceModel=NAMES[src],TargetModel=NAMES[dst],SignalPurpose=purpose,SignalKind=kind,Status='CONCEPT_ONLY_NOT_PLAYER_CREATED_WIRE'))
def group(title,entries):
 first=len(WIRES)+1
 for src,dst,purpose,kind in entries:w(src,dst,purpose,kind)
 GROUPS.append(dict(Group=title,First=first,Last=len(WIRES),WireRange='W%03d-W%03d'%(first,len(WIRES)),Count=len(entries)))

power_targets=[(70,'Docking safety PLC control power'),(64,'Capture sequencing PLC 5V logic'),(44,'Navigation fusion computer 5V'),(49,'Six degree flight control logic 5V'),(50,'Thruster allocation control 5V'),(32,'Valve controller 5V command power'),(38,'LiDAR signal conditioning 5V'),(65,'Docking camera monitor 5V'),(73,'SCADA control and visualization 5V'),(74,'Terminal diagnostic monitor 5V')]
group('Educational 5 V control power and return verification',[(71,t,p,'POWER_CONTROL') for t,p in power_targets])
NAV=[
(35,44,'IMU measured acceleration and angular rate','ANALOG_FEEDBACK'),
(36,42,'Left navigation camera image samples','DATA_STREAM'),
(37,42,'Right navigation camera image samples','DATA_STREAM'),
(42,44,'Stereo triangulated optical target pose','DATA_STREAM'),
(38,43,'Primary LiDAR measured range scan','ANALOG_FEEDBACK'),
(39,43,'Secondary LiDAR measured range scan','ANALOG_FEEDBACK'),
(40,43,'Radar independent relative range rate','ANALOG_FEEDBACK'),
(41,44,'Dock target beacon identity validation','DIGITAL_STATUS'),
(42,44,'Docking fiducial pattern confidence','ANALOG_FEEDBACK'),
(43,44,'Estimated relative position and speed','DATA_STREAM'),
(35,43,'Inertial relative-state propagation samples','ANALOG_FEEDBACK'),
(46,44,'Navigation timestamp and clock synchronization','DIGITAL_STATUS'),
(44,45,'Navigation covariance and confidence ellipsoid','DATA_STREAM'),
(38,66,'Primary LiDAR scan validity diagnostic','DIGITAL_STATUS'),
(39,66,'Secondary LiDAR sensor redundancy diagnostic','DIGITAL_STATUS'),
(66,44,'Injected LiDAR blind spot signal quality','DIGITAL_STATUS'),
(44,38,'Primary LiDAR timed scan trigger','DIGITAL_CONTROL'),
(44,39,'Secondary LiDAR timed scan trigger','DIGITAL_CONTROL'),
(44,40,'Rendezvous radar mode request','DIGITAL_CONTROL'),
(44,42,'Target optical acquisition command','DIGITAL_CONTROL'),
(41,42,'Dock beacon association to optical markers','DATA_STREAM'),
(43,73,'Relative range and velocity telemetry display','DATA_STREAM'),
(45,73,'Covariance visualization and quality trace','DATA_STREAM'),
(35,73,'Raw spacecraft IMU health telemetry','DIGITAL_STATUS')]
group('Sensor acquisition and autonomous navigation',NAV)
PROP=[
(49,50,'Requested six degree thruster wrench','ACTUATOR_COMMAND'),
(50,32,'Thruster command allocation and pulse train','ACTUATOR_COMMAND'),
]+[(32,j,'RCS thruster %s valve pulse command'%('XYZxyz'[j-24]),'ACTUATOR_COMMAND') for j in range(24,30)]+[
(j,33,'RCS thruster %s actual pulse feedback'%('XYZxyz'[j-24]),'ANALOG_FEEDBACK') for j in range(24,30)]+[
(33,49,'Actual delivered impulse and pressure samples','ANALOG_FEEDBACK'),
(30,23,'Propellant tank A available feed pressure','ANALOG_FEEDBACK'),
(31,23,'Propellant tank B available feed pressure','ANALOG_FEEDBACK'),
(23,32,'Propellant manifold delivery readiness','DIGITAL_STATUS'),
(34,50,'Measured thruster calibration thrust table','DATA_STREAM'),
(67,32,'Stuck thruster valve malfunction injection','SAFETY_CONTROL'),
(32,67,'Fault injector configured isolation readiness','DIGITAL_CONTROL'),
(53,50,'Abort fuel reserve thrust allocation limit','SAFETY_CONTROL'),
(50,53,'Predicted commanded thruster fuel usage','ANALOG_FEEDBACK'),
(23,33,'Propellant feed pressure transducer response','ANALOG_FEEDBACK')]
assert len(PROP)==24,len(PROP)
group('Reaction control propulsion and valve feedback',PROP)
GUID=[
(44,47,'Fused relative state for orbital trajectory planning','DATA_STREAM'),
(47,48,'Planned rendezvous trajectory and corridor','DATA_STREAM'),
(48,49,'Desired six degree translational guidance vector','ACTUATOR_COMMAND'),
(49,48,'Actual translational control tracking error','ANALOG_FEEDBACK'),
(43,48,'Measured range-rate and stopping distance','ANALOG_FEEDBACK'),
(46,47,'Trajectory propagation time reference','DIGITAL_STATUS'),
(53,47,'Propellant and abort margin constraint','ANALOG_FEEDBACK'),
(47,53,'Forecasted rendezvous impulse budget','DATA_STREAM'),
(51,48,'Approach hold-point permission and mode','SAFETY_CONTROL'),
(52,48,'Predicted collision-free maneuver certificate','SAFETY_CONTROL'),
(48,54,'Guidance protective abort request','SAFETY_CONTROL'),
(54,49,'Protected autonomous retreat guidance','ACTUATOR_COMMAND'),
(49,54,'Propulsion ability and attitude control feedback','DIGITAL_STATUS'),
(48,14,'Predicted orbital approach overlay','DATA_STREAM'),
(44,45,'Estimated positional uncertainty distribution','DATA_STREAM'),
(48,73,'Guidance controller state and tracking trend','DIGITAL_STATUS'),
(47,73,'Trajectory delta-v forecast telemetry','DATA_STREAM'),
(53,73,'Fuel reserve and retreat envelope','ANALOG_FEEDBACK'),
(54,73,'Abort controller actual mode status','DIGITAL_STATUS'),
(49,50,'Real-time microthruster body-wrench update','ACTUATOR_COMMAND')]
assert len(GUID)==20
group('Relative trajectory planning and 6DOF guidance',GUID)
CORRIDOR=[
(40,51,'Short-range approaching-object radial distance','ANALOG_FEEDBACK'),
(44,51,'Navigation estimation confidence for approach','ANALOG_FEEDBACK'),
(43,51,'Relative target approach speed estimate','ANALOG_FEEDBACK'),
(52,51,'Predictive keep-out-zone intersection status','SAFETY_CONTROL'),
(51,48,'Collision boundary clearance authorization','SAFETY_CONTROL'),
(65,51,'Live final docking camera status','DIGITAL_STATUS'),
(42,65,'Optical docking target video alignment','DATA_STREAM'),
(6,42,'Station target fiducial coordinates','DATA_STREAM'),
(9,41,'Dock beacon coded acquisition signal','DATA_STREAM'),
(16,65,'Inspection gantry monitoring pose','DATA_STREAM'),
(17,65,'Docking approach lighting status','DIGITAL_STATUS'),
(48,13,'Commanded approach corridor overlay','DATA_STREAM'),
(51,13,'Actual proximity warning boundary visual','DATA_STREAM'),
(51,73,'Approach hold point readiness report','DIGITAL_STATUS'),
(52,73,'Collision avoidance prediction timeline','DATA_STREAM'),
(51,54,'Emergency hold/retreat arbitration request','SAFETY_CONTROL'),
(54,51,'Measured retreat maneuver executed status','DIGITAL_STATUS'),
(51,64,'Final docking contact authorization','SAFETY_CONTROL')]
assert len(CORRIDOR)==18
group('Docking approach, beacons, and collision safety',CORRIDOR)
CAPTURE=[
(64,55,'Active soft capture ring actuation request','ACTUATOR_COMMAND'),
(55,64,'Measured active ring extension status','ANALOG_FEEDBACK'),
(64,56,'Guide petal positioning request','ACTUATOR_COMMAND'),
(56,64,'Measured guide petal alignment feedback','ANALOG_FEEDBACK'),
(64,57,'Six independent hard-latch actuation permits','ACTUATOR_COMMAND'),
(57,64,'Six hard-latch actual lock feedback channels','ANALOG_FEEDBACK'),
(58,64,'Six axis captured contact force feedback','ANALOG_FEEDBACK'),
(64,59,'Contact damping request under safe loads','ACTUATOR_COMMAND'),
(59,64,'Actual compliant damper displacement','ANALOG_FEEDBACK'),
(64,60,'Qualified docking umbilical mate command','ACTUATOR_COMMAND'),
(60,64,'Actual umbilical continuity verification','ANALOG_FEEDBACK'),
(64,61,'Pressure gasket compression authorization','ACTUATOR_COMMAND'),
(61,62,'Measured gasket compression and seal health','ANALOG_FEEDBACK'),
(62,64,'Completed fictional pressure leak test results','DIGITAL_STATUS'),
(63,64,'Docked structure load distribution monitor','ANALOG_FEEDBACK'),
(64,65,'Docking mechanism step-by-step camera status','DIGITAL_STATUS'),
(68,64,'Asymmetric mechanical latch fault injection','SAFETY_CONTROL'),
(64,73,'Soft capture to hard docking state events','DIGITAL_STATUS')]
assert len(CAPTURE)==18
group('Mechanical soft capture, locks, and pressure seal',CAPTURE)
SAFETY=[
(71,70,'Independent docking safety PLC protected control supply','POWER_CONTROL'), # W115
(75,70,'Manual emergency approach hold physical contact','SAFETY_CONTROL'),
(70,49,'Independent thruster inhibit and authorization','SAFETY_CONTROL'),
(70,64,'Independent hard capture inhibit output','SAFETY_CONTROL'),
(53,70,'Minimum physical fuel reserve for safe abort','ANALOG_FEEDBACK'),
(52,70,'Independent collision prediction stop request','SAFETY_CONTROL'),
(69,70,'Closing rate monitor calibration health diagnostic','DIGITAL_STATUS'),
(44,69,'Navigation fusion raw range rate calibration channel','ANALOG_FEEDBACK'),
(44,70,'CRITICAL trusted relative closing-rate feedback','ANALOG_FEEDBACK'), # W123
(43,73,'Display-only relative speed telemetry (NOT W123)','ANALOG_FEEDBACK'),
(70,54,'Safety-authorized escape/retreat enable','SAFETY_CONTROL'),
(54,70,'Measured safe abort completion feedback','DIGITAL_STATUS'),
(70,69,'Critical W123 feedback state and fault latch','SAFETY_CONTROL'),
(74,70,'Operator explicit safety reset request','DIGITAL_CONTROL'),
(70,73,'Critical safety status and event telemetry','DIGITAL_STATUS'),
(70,76,'Protected docking mission release authorization','SAFETY_CONTROL')]
assert len(SAFETY)==16
assert len(WIRES)==114
group('Independent docking safety, abort, and W123 recovery',SAFETY)
assert WIRES[122]['SourceModel']==NAMES[44] and WIRES[122]['TargetModel']==NAMES[70]
TELE=[
(44,73,'Fused position and velocity state SCADA','DATA_STREAM'),
(43,73,'Measured target closing rate display summary','ANALOG_FEEDBACK'),
(49,73,'Six degree translation actual control telemetry','DATA_STREAM'),
(32,73,'Thruster valve commanded versus actual status','DIGITAL_STATUS'),
(33,73,'Propellant flow and pulse history trend','ANALOG_FEEDBACK'),
(66,74,'LiDAR dropout event diagnostic fingerprint','DIGITAL_STATUS'),
(67,74,'Stuck valve fault and isolation report','DIGITAL_STATUS'),
(68,74,'Asymmetric latch abnormality diagnosis','DIGITAL_STATUS'),
(62,73,'Pressure seal post docking integrity report','DIGITAL_STATUS'),
(63,73,'Captured structural load safety report','ANALOG_FEEDBACK'),
(65,73,'Synchronized approach camera feed health','DIGITAL_STATUS'),
(64,77,'Docking state transitions flight recorder','DATA_STREAM'),
(70,77,'Latched safety alarm flight recorder','DATA_STREAM'),
(77,73,'Signed fresh mission run event log','DATA_STREAM'),
(73,76,'Six-objective authorized certification display','DIGITAL_STATUS'),
(76,73,'Docking mission 6 of 6 objective certificate','DIGITAL_STATUS')]
assert len(TELE)==16
group('SCADA, diagnostics, and final certification',TELE)
assert len(WIRES)==146 and [x['Count'] for x in GROUPS]==[10,24,24,20,18,18,16,16]
# For every wire there are TWO distinct proposed LOGICAL terminal contacts; modeled sockets are not electrical ports.
for wire in WIRES:
 uid=wire['WireID'];purpose=''.join(ch if ch.isalnum() else '_' for ch in wire['SignalPurpose'].upper())
 purpose='_'.join(p for p in purpose.split('_') if p)[:45]
 wire['SourceTerminalProposed']=uid+'_'+purpose+'_OUT'
 wire['TargetTerminalProposed']=uid+'_'+purpose+'_IN'
 if uid=='W123':wire['SourceTerminalProposed']='TRUSTED_RANGE_RATE_OUT';wire['TargetTerminalProposed']='TRUSTED_RANGE_RATE_IN'
 wire['Critical']='YES' if uid=='W123' else 'NO'
# Group same-class nearby art sockets by owner/kind/role but explicitly do NOT short logical contacts.
port_records=[];key_usage=defaultdict(list)
for wire in WIRES:
 for role,field in [('source','SourceVisualAnchor'),('target','TargetVisualAnchor')]:
  mod=wire['SourceModel'] if role=='source' else wire['TargetModel']
  key=(mod,role,wire['SignalKind']);key_usage[key].append((wire,field))
# assign max 2 visual art anchors per typed-direction group, no more than 224 in total
port_owner={};tmpidx=0
for key,uses in sorted(key_usage.items()):
 for j in range(min(2,len(uses))):
  aid='P%03d'%(tmpidx+1);tmpidx+=1;port_owner[aid]=(key[0],key[1],key[2])
 for j,(wire,field) in enumerate(uses):
  assigned=list(port_owner)[-min(2,len(uses)) + j%min(2,len(uses))]
  wire[field]=assigned
assert tmpidx<=224,('too many socket anchors',tmpidx)
while len(port_owner)<224:
 aid='P%03d'%(len(port_owner)+1)
 idx=(len(port_owner)*17+11)%78
 port_owner[aid]=(NAMES[idx],'unused spare art socket','UNASSIGNED')
assert len(port_owner)==224
SOCKETS=defaultdict(list);ANCHORS=[]
for aid,(model,role,kind) in port_owner.items():
 idx=NAMES.index(model);x,y,z=P[idx];j=len(SOCKETS[model]);
 # Distribute on a panel grid, with no accidental transform duplications.
 xo=(j%6-2.5)*.105;yo=((j//6)%5-2)*.105
 z0=z+(.65 if idx in (18,20) else .38 if idx>=32 else .20)
 # spacecraft-mounted connectors on body side or station electronics landing rack
 if idx in (2,3,4,5,6,7,8,9,10,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,35,36,37,38,39,40,41,42,45,55,56,57,58,59,60,61,63):
  z0=z+.25
 xyz=(x+xo,y+yo,z0)
 SOCKETS[model].append((aid,xyz,role,kind))
 ANCHORS.append(dict(AnchorID=aid,Model=model,WorldX_m=round(xyz[0],5),WorldY_m=round(xyz[1],5),WorldZ_m=round(xyz[2],5),Role=role,SignalKindHint=kind,Status='MESH_ONLY_SOCKET_NOT_SPARKTERMINAL'))
assert len(ANCHORS)==224
# All wire endpoint anchors must actually belong to their specified model.
owner_of={k:v[0] for k,v in port_owner.items()}
for ww in WIRES:
 assert owner_of[ww['SourceVisualAnchor']]==ww['SourceModel'] and owner_of[ww['TargetVisualAnchor']]==ww['TargetModel'],ww
# Multiple WIDs on same anchor require independent child SparkTerminal ports at runtime.
logical_map=[];reuse=defaultdict(set)
for ww in WIRES:
 for role,afield,tfield,mfield in [('OUTPUT','SourceVisualAnchor','SourceTerminalProposed','SourceModel'),('INPUT','TargetVisualAnchor','TargetTerminalProposed','TargetModel')]:
  anchor=ww[afield];reuse[anchor].add(ww['WireID'])
  logical_map.append(dict(WireID=ww['WireID'],Model=ww[mfield],VisualAnchorID=anchor,LogicalTerminalID=ww[tfield],Direction=role,SignalKind=ww['SignalKind'],ElectricalDomain='CONTROL_5V' if ww['SignalKind']=='POWER_CONTROL' else 'ISOLATED_CONTROL_SIGNAL',NeedsSeparateFunctionalPort='YES',Status='PLANNED_NOT_ACTUAL_SPARKTERMINAL'))
for ww in WIRES:
 ww['VisualAnchorReuse']='YES' if len(reuse[ww['SourceVisualAnchor']])>1 or len(reuse[ww['TargetVisualAnchor']])>1 else 'NO'

def art_with_sockets(i):
 a=art_for(i)
 for aid,p,role,kind in SOCKETS.get(NAMES[i],[]):a.socket(p,aid)
 return a

def write_json(path,obj):Path(path).write_text(json.dumps(obj,indent=2),encoding='utf-8')
def data_exports():
 # Persist planning CSVs and no misleading claim of functional wiring.
 for g in GROUPS:g['Status']='PLANNED_NOT_LIVE_PLAYER_WIRES'
 core.write_csv(ROOT/'Gameplay_Wiring_Concept_W001_W146.csv',WIRES,[
  'WireID','SourceModel','SourceVisualAnchor','SourceTerminalProposed','TargetModel','TargetVisualAnchor','TargetTerminalProposed',
  'SignalPurpose','SignalKind','Critical','VisualAnchorReuse','Status'])
 core.write_csv(ROOT/'Gameplay_Wiring_Budget.csv',GROUPS)
 core.write_csv(ROOT/'Terminal_Anchor_Plan_224.csv',ANCHORS)
 core.write_csv(ROOT/'Visual_Terminal_Sockets_224.csv',ANCHORS)
 core.write_csv(ROOT/'Terminal_Logical_Port_Reconciliation_292.csv',logical_map)
 core.write_csv(ROOT/'Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],
  WorldX_m=round(PIVOT_POS[p['Pivot']][0],5),WorldY_m=round(PIVOT_POS[p['Pivot']][1],5),WorldZ_m=round(PIVOT_POS[p['Pivot']][2],5),
  Status='REAL_MESH_OWNING_FBX_AND_GLB_PIVOT') for p in PIVOTS])
 core.write_csv(ROOT/'Art_Assembly.csv',[dict(Index=i,Model=n,Position='0,0,0',Rotation='0,0,0',Scale='1,1,1',
  ArtRoot='L85_HD_VISUAL_ART',Placement='ALL_MODELS_AT_SHARED_ZERO_LOCAL_ORIGIN') for i,n in enumerate(NAMES)])
 roles=[]
 for i,n in enumerate(NAMES):
  cat='Orbital environment and station' if i<=17 else 'Service spacecraft and propulsion' if i<=34 else 'Relative navigation sensor' if i<=46 else 'Rendezvous guidance' if i<=54 else 'Capture docking mechanics' if i<=65 else 'Fault safety and mission controls'
  roles.append(dict(Index=i,Model=n,Category=cat,CenterX=P[i][0],CenterY=P[i][1],CenterZ=P[i][2],Status='MESH_ONLY'))
 core.write_csv(ROOT/'Component_Roles.csv',roles)
 objectives=[
 (1,'Construct the Rendezvous Control Network','146 genuine player-created electrical connections and reconciled functional SparkTerminals, real 5 V control and independent safety','LIVE_ELECTRICAL_GRAPH_AND_DEVICE_HEALTH'),
 (2,'Calibrate the Autonomous Navigation System','Calibrated stereo/LiDAR/radar/IMU, fresh relative navigation data, estimated position velocity, finite thruster calibration','REAL_SENSORS_AND_ACTUATOR_CALIBRATION'),
 (3,'Execute Precision Orbital Rendezvous','Relative-motion physics, commanded and actual position/velocity, safe keep-out zones, hold points and fuel margin','RELATIVE_STATE_AND_MISSION_RUN_EVIDENCE'),
 (4,'Complete Soft Capture and Hard Docking','Measured contact speed and pose, physically verified soft capture and six locks, mate status and pressure seal validation','MECHANICAL_CAPTURE_AND_SEAL_EVIDENCE'),
 (5,'Survive Three Docking-System Failures','LiDAR dropout, stuck propulsion valve, asymmetric latch diagnosed/changed and repaired; fresh full docking requalified','THREE_REAL_FAULT_REPAIRS_AND_FRESH_MISSION'),
 (6,'Restore Critical Closing-Rate Feedback','Deliberate actual W123 disconnection, stale critical measurement, safe inhibit/abort or hold, fault latch; reconnect and fresh sampling, explicit reset, new full docking','W123_RECOVERY_AND_NEW_RUN_QUALIFICATION')]
 core.write_csv(ROOT/'Objective_Certification_Plan_6.csv',[dict(Number=k,Title=title,RequiredEvidence=ev,Authority=auth,Status='DESIGN_ONLY_NOT_IMPLEMENTED') for k,title,ev,auth in objectives])
 w123=WIRES[122]
 doc=f'''# LEVEL 85 — CRITICAL W123 TRUSTED CLOSING-RATE FEEDBACK

This is a **design proposal**, not a prebuilt Unity wire.

- Wire: **W123**, critical `ANALOG_FEEDBACK`.
- Source: `{w123['SourceModel']}`.
- Source logical terminal: `{w123['SourceTerminalProposed']}`.
- Source 3D visual socket: `{w123['SourceVisualAnchor']}`.
- Target: `{w123['TargetModel']}`.
- Target logical terminal: `{w123['TargetTerminalProposed']}`.
- Target 3D visual socket: `{w123['TargetVisualAnchor']}`.

The authoritative continuous feed must include signed/filtered closing speed, sequence number, timestamp, confidence and sensor validity, not just a frozen floating-point display. W124 is **display telemetry only** and W121 is sensor diagnostic health, neither may substitute for W123.

A player must first create the real electrical graph connection. On a deliberate final disconnection after Objectives 1–5, real graph continuity changes, W123 age expires, automated docking/capture permission is withdrawn, and the independent safety PLC commands a controlled HOLD or ABORT through modeled actuators and available fuel. Do not teleport the vehicle to zero velocity, and do not undock an already safely latched vehicle. Latch the fault and log causal evidence. On reconnection require fresh calibrated measurement and timestamp, cross-check radar/LiDAR consistency, perform explicit operator reset, and run a completely new full orbital rendezvous and dock sequence. Historical first five certificates persist; Objective 6 earns 6/6 only after fresh evidence.

The art/CSV package has no real player wires, actual SparkTerminal components, spacecraft physics, pressure simulation, safety PLC state machine or implemented LevelGamePlayManager objectives. Build them in the user's existing Unity project using verified APIs.
'''
 (ROOT/'W123_Critical_Closing_Rate_Recovery.md').write_text(doc,encoding='utf-8')
 (ROOT/'Gameplay_Integration_Plan.md').write_text('''# LEVEL 85 — UNITY GAMEPLAY INTEGRATION PLAN (NOT ALREADY IMPLEMENTED)

1. Assemble the 78 modular FBX models as origin-aligned children of `L85_HD_VISUAL_ART`. Do not duplicate an assembled showcase reference with them.
2. Reconcile 224 visual art sockets `P001–P224` against 292 distinct proposed LOGICAL wire endpoint contacts. Use actual `SparkTerminal` components only in the Unity gameplay hierarchy; never short separately typed signals because they share a visual marker. The accompanying `Terminal_Logical_Port_Reconciliation_292.csv` enumerates every logical endpoint.
3. Make every W001–W146 a *player-created actual circuit edge*, power and returns verified through the real Project Spark solver. Control voltage is educational 5 V. Fictional propulsion/safety power and propellant state belong in separate, coherent plant models.
4. Simulate target-relative translational position and velocity under an orbital relative-motion formulation, bounded thruster valve lag and thrust allocation, spacecraft 6-DOF attitude and genuine fuel usage. A station visual geometry mesh does not own the physics.
5. Model primary/secondary scanning LiDAR, stereo camera baseline, radar range/range-rate, IMU bias and sampling timestamps, beacon confidence and sensor fusion covariance. Separate true plant state, stale/biased samples, estimated state, commands and state certifications.
6. Enforce actual approach hold-points, stopping distance, lateral error, relative closing speed, navigation accuracy, docking attitude, keep-out zone and safe retreat rules. The **100m/30m/5m**, **0.05 m/s**, **0.15m**, **1.5°** values are merely fictional adjustable game targets, not real-world docking guidance.
7. Docking soft capture must respond to contact pose/speed and damping, with measured six-point latch sequence, hard-dock stability, electrical umbilical continuity and fictional pressure seal integrity. Do not confuse visual ring overlap with a certified dock.
8. Implement three actual faults (LiDAR blindspot, stuck thruster valve, asymmetric latch) causing physical/sensor state mutations; diagnose, isolate, recover, then require a fresh docking mission.
9. W123 is the trusted closing-rate feedback into docking safety, not a display feed. Disconnection withdraws autonomous approach permission, controls safe abort/hold according to true physics, latches a fault, and demands fresh samples, calibration, operator reset and another full mission. Preserve historical Objectives 1–5.
10. Bind the optional `SparkLevel85VisualRig` to physical plant state for 60 mechanical/indicator transforms. UI, particles and animation must be read-only depictions.
11. Implement six real `LevelGamePlayManager` objectives and an idempotent **actual gameplay** builder only after inspecting real Unity project signatures. Included optional menu scripts are **ART IMPORT HELPERS**, not a complete gameplay scene.
12. Test imported FBX scale (numeric centimetres) and GLB scale (numeric metres). The files are new original procedural mesh art; finish production-grade UV unwrap, texture baking, hand-tuned PBR, collision, LOD, VFX and realistic Unity Play Mode tests before calling the level AAA-complete.
''',encoding='utf-8')
 (ROOT/'AAA_Art_Delivery_Notes.md').write_text('''# Project Spark Level 85 — 3D Asset Delivery

The kit contains **78 original procedural modular binary FBX models** and **78 corresponding GLB models**, plus a complete FBX/GLB art showcase for reference. Geometric parts contain real triangles, visible bevel-like hardware, concentric docking rings, service spacecraft primary hull, microthruster bells, mechanical capture petals, latch actuators, sensor optics, laboratory computer control panels, cable port art and special holographic meters.

The procedural geometry is a **high-detail AAA-inspired technical-art kit**, not a finalized artist-sculpted AAA production asset drop. Production UV islands, seamless texture baking, lightmap UVs, authored wear/scratch detailing, fully optimized mesh LODs, physics/colliders, animated actuator constraints, real Unity HDRP/URP VFX and comprehensive gameplay require further production work. Generated PBR PNGs are reference maps and are not guaranteed to apply to the geometry without UVs.

FBX and GLB are genuine editable 3D mesh formats. They are not 2D images. The `ANIM_*` nodes and `SOCKET_P###` nodes are true mesh-owning 3D transforms, not functional gameplay terminals.
''',encoding='utf-8')
 (ROOT/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 85: THE LAST METER
===========================================

REAL 3D ART ASSETS + PLANNING — NOT A FINISHED PLAYABLE UNITY LEVEL

Contents:
- 78 individually importable REAL binary FBX 7400 modular mesh files in Models/.
- 78 matching glTF 2.0 GLB modular mesh files in GLB/.
- An assembled reference showcase FBX and GLB; DO NOT add these together with 78 modules.
- 60 named ANIM_* mesh-owning parent transforms (FBX + GLB).
- 224 named SOCKET_P001..SOCKET_P224 mesh-owning DECORATIVE socket parents.
- 146 *conceptual* player wiring rows, including proposed W123 critical wire.
- 292 independently named logical connection endpoints; a visual socket can represent more than one logical contact. No real SparkTerminals are included.
- Six design-only objective certification specifications.
- 12 procedural PBR reference materials, 36 PNG textures (UV mapping not completed).
- Optional Unity Editor art placement, material color upgrade, import validation and runtime visual-only rig helpers.

Recommended import:
1. Import 78 numbered FBX files into Assets/ProjectSpark/Level85/Models/.
2. Import Textures/ and optional scripts, putting the three Editor helper scripts inside a Unity Editor folder.
3. Select the empty intended Level 85 layout root in the Hierarchy.
4. Run Tools/Project Spark/Levels/85/Place HD Art Under Selected Layout.
5. This idempotently creates ONLY a visual subroot: L85_HD_VISUAL_ART.
6. Run Tools/Project Spark/Levels/85/Validate Imported HD Art.
7. Optional: Tools/Project Spark/Levels/85/Create & Apply HD Materials To Selected Art.
8. Optionally add SparkLevel85VisualRig as a visual adapter. It is NOT a navigation/physics solver.
9. Independently implement circuit terminals, wiring, objectives, physics and tests in existing Project Spark game.

Scale: FBX numeric vertices and pivot translations are centimetres, UnitScaleFactor = 1.
GLB vertex/pivot coordinates are metres. Unity's resulting assembled station must be measured in metres; convert exactly once, never 100x twice.
Axes: Y up, X horizontal station approach, Z depth. ALL modular assets origin aligned at (0,0,0).

NOT INCLUDED:
- No real player wires or actual SparkTerminal nodes.
- No Unity compiled playable scenes/prefabs, actual six objectives, relative orbit simulator or working safety PLC.
- No guarantee of production hand-authored AAA LODs, authored UVs, lightmap baking or Unity GPU optimization.
- No Unity Play Mode/compilation validation.

Refer to Model_Manifest.json, Animation_Rigs.csv, wire and terminal CSVs, and Gameplay_Integration_Plan.md.
''',encoding='utf-8')
 return objectives

def homes(i,a):
 result=[]
 for p in PIVOTS:
  if p['Index']==i:result.append((p['Pivot'],a.name,PIVOT_POS[p['Pivot']]))
 for aid,coord in a.sockets:result.append(('SOCKET_'+aid,a.name,coord))
 return result

def geometry_exports():
 manifests=[];all_chunks=[];all_homes=[]
 bmin=np.full(3,np.inf);bmax=np.full(3,-np.inf)
 start=time.monotonic()
 for i,name in enumerate(NAMES):
  a=art_with_sockets(i);chunks=core.chunk_model(a);pivots=homes(i,a)
  fp=ROOT/'Models'/('%02d_%s.fbx'%(i,name));gp=ROOT/'GLB'/('%02d_%s.glb'%(i,name))
  f=core.export_fbx(str(fp),[(name,q) for q in chunks],[name],pivots,a.sockets)
  g=core.export_glb(str(gp),[(name,q) for q in chunks],[name],pivots)
  for p in chunks:
   if p['pivot'] in PIVOT_POS:center=PIVOT_POS[p['pivot']]
   elif p['pivot'] and p['pivot'].startswith('SOCKET_'):center=dict(a.sockets)[p['pivot'][7:]]
   else:center=(0,0,0)
   v=p['vertices']+np.asarray(center)
   bmin=np.minimum(bmin,v.min(axis=0));bmax=np.maximum(bmax,v.max(axis=0))
  manif=dict(index=i,name=name,file=fp.name,glb=gp.name,triangle_count=int(sum(len(p['faces']) for p in chunks)),mesh_parts=len(chunks),source_primitive_count=len(a.raw),visual_terminal_sockets=len(a.sockets),animation_pivots={pv:[round(v,5) for v in xyz] for pv,rt,xyz in pivots if pv.startswith('ANIM_')},fbx_bytes=f['file_size'],glb_bytes=g['glb_bytes'])
  manifests.append(manif);all_chunks.extend((name,p) for p in chunks);all_homes.extend(pivots)
  if i%10==0 or i==77:print('EXPORTED',i+1,'/78 |',name,'triangles',manif['triangle_count'],'mesh groups',len(chunks),'elapsed',round(time.monotonic()-start,1),'s',flush=True)
 print('START ASSEMBLED SHOWCASE',len(all_chunks),'mesh groups',sum(m['triangle_count'] for m in manifests),'triangles',flush=True)
 fpath=ROOT/'Models'/'Level85_Complete_ArtKit_Showcase.fbx';gpath=ROOT/'GLB'/'Level85_Complete_ArtKit_Showcase.glb'
 a=core.export_fbx(str(fpath),all_chunks,NAMES,all_homes,ANCHORS)
 g=core.export_glb(str(gpath),all_chunks,NAMES,all_homes)
 man=dict(level_id='level_85',display_name='The Last Meter',subtitle='Advanced Orbital Rendezvous and Spacecraft Docking',
  individual_models=len(NAMES),individual_glbs=len(NAMES),assembled_fbx='Models/'+fpath.name,assembled_glb='GLB/'+gpath.name,
  individual_triangles=sum(m['triangle_count'] for m in manifests),individual_mesh_parts=sum(m['mesh_parts'] for m in manifests),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),proposed_independent_logical_wire_endpoints=len(logical_map),
  workstation_world_bounds_m=[[round(float(x),5) for x in bmin],[round(float(x),5) for x in bmax]],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up / X station approach / Z depth',assembly_origin=[0,0,0],
  actual_player_wires_included=False,functional_sparkterminals_included=False,actual_spacecraft_orbit_solver_included=False,
  six_runtime_objectives_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
  assembled_fbx_bytes=a['file_size'],assembled_glb_bytes=g['glb_bytes'],models=manifests)
 write_json(ROOT/'Model_Manifest.json',man)
 return man

if __name__=='__main__':
 st=time.monotonic();print('GENERATING PROJECT SPARK LEVEL 85 ART + PLANS',flush=True)
 data_exports();m=geometry_exports()
 print('ART COMPLETE in',round(time.monotonic()-st,1),'seconds',m['individual_triangles'],'triangles',flush=True)
