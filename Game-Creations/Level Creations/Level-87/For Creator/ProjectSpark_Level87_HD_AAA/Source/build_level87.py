#!/usr/bin/env python3
"""Project Spark Level 87 THE GRAVITY ENGINE: original procedural binary FBX/GLB art.
Creates real 3D meshes and source-grounded planning tables; NO actual Unity gameplay.
FBX geometry/pivot numeric coordinates = centimetres; GLB metres. Axis Y up.
"""
from pathlib import Path
import math, random, time, csv, json
from collections import Counter,defaultdict
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for f in ['Models','GLB','Textures','Reports','Unity_Optional']:(ROOT/f).mkdir(exist_ok=True,parents=True)
NAMES=['L87_'+s for s in (Path(__file__).parent/'model_names.txt').read_text().splitlines() if s.strip()]
assert len(NAMES)==86
random.seed(87087)
core.COLORS={
 'structural':(.38,.45,.53,1),'titanium':(.69,.75,.80,1),'dark':(.04,.06,.10,1),
 'graphite':(.10,.14,.19,1),'copper':(.85,.40,.13,1),'gold':(.96,.65,.17,1),
 'ceramic':(.89,.90,.88,1),'glass':(.19,.54,.83,.55),'panel':(.015,.12,.23,1),
 'pcb':(.035,.21,.18,1),'cyan':(.11,.78,1,1),'blue':(.15,.39,.90,1),
 'green':(.06,.91,.54,1),'amber':(1,.58,.12,1),'violet':(.63,.41,1,1),
 'red':(1,.16,.12,1),'orange':(1,.38,.05,1),'white':(.97,.97,.94,1),
 'magnet':(.21,.26,.34,1),'stars':(.87,.91,1,1),'black':(.02,.03,.05,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
P={0:(0,-.05,0),1:(0,5.3,-11.7),2:(0,7.25,-7.2),3:(0,7.25,-7.2),4:(0,7.25,-7.15),11:(0,7.25,-7.2),12:(0,7.25,-7.2),13:(0,7.2,-7.3),14:(0,6.8,-8.2),15:(10.8,6.1,-7.7),
24:(-12.3,1.75,2.2),25:(-11.0,1.75,2.2),26:(-9.7,1.75,2.2),27:(-8.4,1.75,2.2),28:(-2.1,7.5,-4.1),29:(-11.7,4.2,-5.6),30:(-7.1,1.8,2.2),31:(-5.8,1.8,2.2),
32:(0,7.25,-6.9),33:(0,7.25,-7.2),34:(-4.5,1.75,2.2),35:(-3.2,1.75,2.2),36:(-1.9,1.75,2.2),37:(-.6,1.75,2.2),38:(.7,1.75,2.2),39:(2.0,1.75,2.2),40:(3.3,1.75,2.2),41:(4.6,1.75,2.2),42:(5.9,1.75,2.2),43:(7.2,1.75,2.2),
44:(-12.3,1.75,4.9),45:(-11,1.75,4.9),46:(-9.7,1.75,4.9),47:(-8.4,1.75,4.9),48:(-7.1,1.75,4.9),49:(-6.5,7.5,-4.8),50:(6.5,7.5,-4.8),51:(-5.8,1.75,4.9),52:(-4.5,1.75,4.9),53:(-3.2,1.75,4.9),54:(-1.9,1.75,4.9),55:(-.6,1.75,4.9),56:(.7,1.75,4.9),
57:(8.35,2.85,1.7),58:(9.7,2.85,1.7),59:(11.05,2.85,1.7),60:(12.4,2.85,1.7),61:(8.35,1.7,4.65),62:(9.7,1.7,4.65),63:(11.05,1.7,4.65),64:(12.4,1.7,4.65),65:(8.35,3.95,4.65),66:(9.7,3.95,4.65),67:(2.4,8.5,-3.7),68:(11.05,3.95,4.65),
69:(-11.7,3.75,7.2),70:(-10.3,3.75,7.2),71:(-8.9,3.75,7.2),72:(-7.5,3.75,7.2),73:(-6.1,3.75,7.2),74:(-4.7,3.75,7.2),75:(-3.3,3.75,7.2),76:(-1.9,3.75,7.2),77:(-.5,3.75,7.2),78:(.9,3.75,7.2),79:(2.3,3.75,7.2),80:(3.7,3.75,7.2),81:(0,11.8,-8.7),82:(0,12.75,-8.0),83:(5.1,3.75,7.2),84:(8.4,7.4,-4.1),85:(6.5,3.75,7.2)}
for i in range(5,11):
 t=(i-5)*math.tau/6+math.pi/6
 P[i]=(5.25*math.cos(t),7.25+5.25*math.sin(t),-7.2)
for i in range(16,24):
 t=i*math.tau/8
 P[i]=(5.85*math.cos(t),7.25+5.85*math.sin(t),-6.65)
assert set(P)==set(range(86))
# unique animation parents; separate rotor art modules each have a pivot located at ring centre.
PIVOT_SPECS=[
(2,'GRAVITY_RING_MAIN_ROTATION','rotation','z'),(3,'GRAVITY_RING_SPOKES','rotation','z'),
(5,'HABITAT_SEGMENT_A','rotation','z'),(6,'HABITAT_SEGMENT_B','rotation','z'),(7,'HABITAT_SEGMENT_C','rotation','z'),(8,'HABITAT_SEGMENT_D','rotation','z'),(9,'HABITAT_SEGMENT_E','rotation','z'),(10,'HABITAT_SEGMENT_F','rotation','z'),(11,'HABITAT_PRESSURE_SHELL','rotation','z'),
(4,'STATION_HUB_FLYWHEEL','rotation','z'),(12,'STATIC_GUIDE_INDICATOR','indicator','none'),(13,'STATION_ISOLATION_SUPPORT','indicator','none'),
(16,'BEARING_A_FIELD','indicator','none'),(17,'BEARING_B_FIELD','indicator','none'),(18,'BEARING_C_FIELD','indicator','none'),(19,'BEARING_D_FIELD','indicator','none'),(20,'BEARING_E_FIELD','indicator','none'),(21,'BEARING_F_FIELD','indicator','none'),(22,'BEARING_G_FIELD','indicator','none'),(23,'BEARING_H_FIELD','indicator','none'),
(24,'PRIMARY_RADIAL_SENSOR','translation','x'),(25,'SECONDARY_RADIAL_SENSOR','translation','x'),(26,'AXIAL_SENSOR','translation','z'),(27,'BEARING_COIL_CURRENT','indicator','none'),(28,'MAGNETIC_FIELD_LINES','indicator','none'),(29,'EMERGENCY_TOUCHDOWN_RACE','translation','x'),(30,'BEARING_AMPLIFIER_COOLING','rotation','z'),(31,'BEARING_CALIBRATION_PROBE','translation','z'),
(32,'SEGMENTED_RIM_DRIVE','indicator','none'),(33,'ROTOR_MAGNET_ARRAY','rotation','z'),(34,'DRIVE_A_INVERTER','indicator','none'),(35,'DRIVE_B_INVERTER','indicator','none'),(36,'MOTOR_PHASE_SENSOR','indicator','none'),(37,'TORQUE_ESTIMATOR','indicator','none'),(38,'SPEED_ENCODER','rotation','z'),(39,'SAFE_SPINUP_PROFILE','indicator','none'),(40,'MOTOR_PRECHARGE_CONTACTOR','translation','x'),(41,'DRIVE_COOLING_IMPELLER','rotation','x'),(42,'ACCELERATION_PROFILE','indicator','none'),(43,'COUNTERTORQUE_FLYWHEEL','rotation','x'),
(44,'VIBRATION_PROBE_AXES','indicator','none'),(45,'SPECTRAL_ANALYZER','indicator','none'),(46,'SHAFT_ORBIT_PROBE','translation','z'),(47,'IMBALANCE_VECTOR','indicator','none'),(48,'ADAPTIVE_DAMPING','indicator','none'),(49,'BALANCE_WEIGHT_A','translation','x'),(50,'BALANCE_WEIGHT_B','translation','x'),(51,'MASS_SCAN','indicator','none'),(52,'RESONANCE_SWEEP','indicator','none'),(53,'BEARING_TEMPERATURE','indicator','none'),(54,'ROTOR_HEALTH_RECORDER','indicator','none'),(55,'VIBRATION_SCADA','indicator','none'),(56,'FAULT_CALIBRATION','translation','x'),
(57,'PRIMARY_POWER_COUPLER','rotation','z'),(58,'BACKUP_POWER_COUPLER','rotation','z'),(59,'ROTARY_OPTICAL_LINK','indicator','none'),(60,'ROTARY_POWER_CONTACTOR','translation','x'),(61,'HABITAT_LOAD_A','indicator','none'),(62,'HABITAT_EMERGENCY_LIGHT','indicator','none'),(63,'HABITAT_THERMAL_LOAD','indicator','none'),(64,'ROTARY_DATA_QUALITY','indicator','none'),(65,'ROTARY_POWER_MONITOR','indicator','none'),(66,'LOAD_DISTURBANCE_SLIDER','translation','x'),(67,'ARTIFICIAL_GRAVITY_HOLOGRAM','indicator','none'),(68,'POWER_BUDGET','indicator','none'),
(69,'SAFETY_SUPERVISOR','indicator','none'),(70,'TRUSTED_RADIAL_SAFETY','indicator','none'),(71,'INDEPENDENT_OVERSPEED','indicator','none'),(72,'EMERGENCY_STOP','translation','z'),(77,'GAP_SENSOR_DRIFT_FAULT','translation','x'),(78,'AMP_OVERHEAT_FAULT','translation','y'),(79,'MASS_IMBALANCE_FAULT','translation','x'),(80,'W137_CRITICAL_FEEDBACK','indicator','none'),(81,'GRAVITY_MASTER_SCADA','indicator','none'),(82,'FINAL_CERTIFICATION','indicator','none')]
# Keep exactly 68 distinct mechanically useful or live-status pivot groups.
PIVOT_SPECS=[p for p in PIVOT_SPECS if p[0] not in (12,13,27,31,36,42,54)]
assert len(PIVOT_SPECS)==68, len(PIVOT_SPECS)
PIVOTS=[dict(Pivot='ANIM_'+name,Index=i,Model=NAMES[i],Kind=kind,Axis=ax) for i,name,kind,ax in PIVOT_SPECS]
RING_CENTER=(0,7.25,-7.2)
PIVOT_POS=core.PIVOT_POS={p['Pivot']:(RING_CENTER if p['Index'] in [2,3,5,6,7,8,9,10,11,33] else P[p['Index']]) for p in PIVOTS}

def b(a,p,s,mat='titanium',pv=None,label='MachinedSurface'):a.box(p,s,mat,pv,label)
def cy(a,p,r,h,mat='titanium',axis='z',pv=None,label='CylindricalPart',seg=24):a.cylinder(p,r,h,mat,axis=axis,seg=seg,pivot=pv,label=label)
def tor(a,p,R,r,mat='titanium',axis='z',pv=None,label='PrecisionRing',seg=64,tube=9):a.torus(p,R,r,mat,axis=axis,seg=seg,tube=tube,pivot=pv,label=label)
def bm(a,p,q,r=.05,mat='structural',pv=None,label='StructuralMember'):a.beam(p,q,r,mat,pivot=pv,label=label,sides=10)
def ball(a,p,r,mat='blue',pv=None,label='Sphere',subs=2):
 m=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(m.vertices+np.array(p),m.faces,mat,pv,label)
def sector(a,center,R,width,amin,amax,depth,mat,pv,sub=18,label='CurvedSegment'):
 x,y,z=center; vs=[];fs=[]
 for k in range(sub+1):
  th=amin+(amax-amin)*k/sub
  for r,dz in [(R-width/2,-depth/2),(R+width/2,-depth/2),(R-width/2,depth/2),(R+width/2,depth/2)]:
   vs.append((x+r*math.cos(th),y+r*math.sin(th),z+dz))
 for j in range(sub):
  a0=j*4;a1=a0+4
  for q,u,v,w in [(0,1,0,1),(2,3,2,3)]:pass
  for s,t in [(0,1),(2,3),(0,2),(1,3)]:
   fs.extend([[a0+s,a1+s,a1+t],[a0+s,a1+t,a0+t]])
 for base in (0,sub*4):
  fs.extend([[base,base+2,base+1],[base+1,base+2,base+3]])
 a.mesh(vs,fs,mat,pv,label)

def panel(a,p,scale=1,theme='cyan',kind='plc',pv=None):
 x,y,z=p
 core.panel_equipment(a,p,scale,theme,kind)
 for j in range(7):cy(a,(x-.41+j*.135*scale,y-.53*scale,z+.405*scale),.037*scale,.10*scale,'gold','z',None,'TerminalPort',12)
 for j in range(4):b(a,(x-.38+j*.25*scale,y+.59*scale,z+.26*scale),(.14*scale,.09*scale,.055*scale),'structural',pv,'CoolingDuctRib')
 if pv:b(a,(x,y+.35*scale,z+.42*scale),(.23*scale,.04*scale,.023*scale),theme,pv,'PivotLinkedStatusIndicator')

def disk_at(a,p,rad=1.0,mat='titanium',pv=None):
 cy(a,p,rad,.12,mat,'z',pv,'MachinedCircularRotorFlange',36)
 tor(a,(p[0],p[1],p[2]+.07),rad*.85,.045,'gold','z',pv,'ConcentricInstrumentedRace',48)

def make_art(i):
 a=core.ModelArt(NAMES[i]); x,y,z=P[i]; pts=[p['Pivot'] for p in PIVOTS if p['Index']==i];pv=pts[0] if pts else None
 if i==0:
  b(a,(0,-.13,1.2),(30,.32,17.8),'graphite',None,'FactoryDeckMainSlab')
  for xx in [-14.4,14.4]:b(a,(xx,.08,1.2),(.26,.32,17.2),'titanium',None,'OrbitalDeckStructuralSideRail')
  for j in range(19):b(a,(-13.8+j*1.54,.065,2),(.018,.02,15.5),'structural',None,'MachinedPlatformFloorSeam')
  for j in range(12):b(a,(0,.070,-6.3+j*1.35),(29,.021,.02),'blue' if j%2 else 'structural',None,'AntislipFloorGrid')
  for j in range(16):b(a,(-12.7+j*1.6,.085,6.7),(.40,.02,.18),'amber',None,'WorkstationSafetyStripes')
  for xx in [-13.8,13.8]:
   for zz in [-6,0,5.6]:cy(a,(xx,.5,zz),.16,1.0,'titanium','y',None,'DeckSupportPedestal',12)
 elif i==1:
  ball(a,(1.5,4.1,-15.8),7.2,'blue',None,'PlanetEarthCurvature',subs=3)
  tor(a,(1.5,4.1,-15.1),7.3,.055,'cyan','z',None,'AtmosphericLimbGlowArc',96)
  for j in range(160):
   xx=random.uniform(-14,14);yy=random.uniform(2.7,13.5)
   b(a,(xx,yy,-13.5),(.015,.015,.012),'white' if j%5 else 'amber',None,'OrbitalStarfieldPin')
 elif i==2:
  tor(a,RING_CENTER,5.20,.26,'ceramic','z',pv,'PrimaryPressureStiffenedRotorRim',128,14)
  tor(a,(0,7.25,-6.97),5.17,.064,'gold','z',pv,'RotorGoldTensionBand',120)
  tor(a,(0,7.25,-7.46),5.17,.063,'graphite','z',pv,'RotorCarbonReinforcementBand',120)
  for k in range(96):
   t=k*math.tau/96;rr=5.20;pos=(rr*math.cos(t),7.25+rr*math.sin(t),-6.90)
   cy(a,pos,.052,.08,'amber' if k%12==0 else 'cyan','z',pv,'LiveRotationalEncoderMarker',10)
  for k in range(24):
   t=k*math.tau/24
   sector(a,RING_CENTER,5.27,.37,t-.08,t+.08,.13,'titanium',pv,4,'PrecisionMachinedRotorArmourPanel')
 elif i==3:
  cy(a,RING_CENTER,1.05,.65,'titanium','z',pv,'RotatingCentralSpokeRoot',48)
  for j in range(12):
   th=j*math.tau/12; r=5.10
   bm(a,(.9*math.cos(th),7.25+.9*math.sin(th),-7.2), (r*math.cos(th),7.25+r*math.sin(th),-7.2),.11,'structural',pv,'SpaceHabitatPrimarySpokeTruss')
   bm(a,(1.2*math.cos(th+.07),7.25+1.2*math.sin(th+.07),-7.41), (4.92*math.cos(th+.12),7.25+4.92*math.sin(th+.12),-7.41),.039,'gold',pv,'TrussCrossBrace')
   cy(a,(4.9*math.cos(th),7.25+4.9*math.sin(th),-7.08),.17,.30,'gold','z',pv,'SpokeRimAnchorBolt',14)
 elif i==4:
  disk_at(a,RING_CENTER,1.18,'structural',pv)
  for r in [1.32,1.55,1.82]:tor(a,RING_CENTER,r,.047,'gold' if r==1.32 else 'titanium','z',pv,'HubRotarySealAssembly',64)
  for j in range(12):
   th=j*math.tau/12
   cy(a,(.95*math.cos(th),7.25+.95*math.sin(th),-7.05),.08,.14,'copper','z',pv,'HubPowerBusFastener',10)
 elif 5<=i<=10:
  k=i-5;ang=k*math.tau/6+math.pi/6
  sector(a,RING_CENTER,5.25,.47,ang-math.pi/6+.015,ang+math.pi/6-.015,.44,'ceramic',pv,28,'PressurizedHabitatCurvedMainModule')
  sector(a,RING_CENTER,5.25,.57,ang-math.pi/6+.030,ang+math.pi/6-.030,.065,'titanium',pv,25,'HabitatThermalOuterShroud')
  for j in range(11):
   theta=ang-math.pi/6+.11+j*(math.pi/3-.22)/10
   rr=5.57
   cy(a,(rr*math.cos(theta),7.25+rr*math.sin(theta),-6.91),.085,.055,'glass' if j%3==0 else 'gold','z',pv,'HabitatObservationViewport')
  for j in range(8):
   theta=ang-math.pi/6+.15+j*(math.pi/3-.30)/7
   sector(a,RING_CENTER,5.20,.48,theta-.021,theta+.021,.56,'gold',pv,2,'HabitatCircumferentialStructuralRib')
 elif i==11:
  tor(a,RING_CENTER,4.93,.058,'graphite','z',pv,'HabitatInnerRadiationShieldRing',128)
  tor(a,RING_CENTER,5.52,.070,'titanium','z',pv,'OuterThermalProtectionShell',128)
  for j in range(72):
   th=j*math.tau/72
   cy(a,(5.53*math.cos(th),7.25+5.53*math.sin(th),-7.55),.055,.20,'gold' if j%3==0 else 'structural','z',pv,'RepetitiveRadiatorRetentionClamp',10)
 elif i==12:
  tor(a,RING_CENTER,5.85,.16,'structural','z',None,'StationaryMagneticBearingCircumferentialRail',128,11)
  tor(a,(0,7.25,-6.7),5.85,.055,'gold','z',None,'StationaryBearingAlignmentDatum',128)
  b(a,(0,7.25,-6.41),(.26,.25,.04),'cyan',pv,'StationaryGuideSensorMarker')
 elif i==13:
  for xx in (-7.0,7.0):
   bm(a,(xx,.6,-7.1),(xx,13.5,-7.1),.19,'titanium',None,'StationaryRingCradleUpright')
   bm(a,(xx,.6,-7.5),(xx*.73,12.5,-7.5),.10,'gold',None,'IsolationTrussDiagonalStrut')
  for yy in (1.3,12.7):bm(a,(-8,yy,-7.1),(8,yy,-7.1),.18,'structural',None,'FixedSupportFrameCrossMember')
  b(a,(0,11.6,-7),(.27,.12,.08),'cyan',pv,'SupportConditionIndicator')
 elif i==14:
  for zz in (-8.7,-5.9):
   for j in range(8):
    xx=-10.8+j*3
    bm(a,(xx,0.8,zz),(xx,11.7,zz),.05,'structural',None,'LightweightOrbitalIsolationColumn')
    if j<7:bm(a,(xx,1.2,zz),(xx+3,11.3,zz),.036,'gold',None,'CrossBracedStationSupport')
 elif i==15:
  b(a,(x,y-.7,z),(4.8,.32,3.0),'graphite',None,'HabitatObservationDeckFloor')
  for xx in [x-2.2,x+2.2]:
   bm(a,(xx,y-.6,z-1.4),(xx,y+1.7,z-1.4),.07,'titanium',None,'ObservationDeckGlassSupport')
  b(a,(x,y+.2,z-1.35),(4.6,2.2,.08),'glass',None,'ImpactResistantViewPort')
  for j in range(8):b(a,(x-1.8+j*.51,y-1,z+.48),(.25,.11,.025),'amber',None,'HabitatControlFloorMarker')
 elif 16<=i<=23:
  th=(i-16)*math.tau/8
  # stationary magnetic bearing modules encircle the moving rim
  disk_at(a,(x,y,z),.48,'graphite',pv)
  tor(a,(x,y,z+.14),.36,.08,'copper','z',pv,'MagneticBearingCopperFieldCoil',32)
  for k in range(22):
   t=k*math.tau/22
   cy(a,(x+.37*math.cos(t),y+.37*math.sin(t),z+.27),.055,.13,'copper' if k%3 else 'gold','z',pv,'SegmentedMagnetPoleWinding',12)
  for rr in (.56,.69):tor(a,(x,y,z-.28),rr,.045,'titanium','z',None,'FixedBearingStatorRetainer',32)
  b(a,(x,y,z+.37),(.17,.07,.045),'cyan',pv,'BearingLiveGapIndicator')
 elif i in (24,25,26,46):
  panel(a,P[i],.90,'cyan','optical',pv)
  cy(a,(x+.27,y+.28,z+.48),.13,.18,'glass','z',pv,'CapacitiveRadialDisplacementProbe',24)
  for j in range(5):b(a,(x-.37+j*.18,y-.2,z+.48),(.13,.025,.02),'cyan',None,'MeasurementScaleGraticule')
 elif i in (27,31,36,37,38,44,45,48,51,52,53,54,55,56,64,65,66,68):
  panel(a,P[i],.95,'amber' if i in (53,56) else 'cyan','instrument',pv)
  for j in range(11):b(a,(x-.39+j*.072,y+.06,z+.39),(.035,(.06+.018*(j%5)),.016),'green' if j%3 else 'amber',pv,'ResponsiveWaveformBargraph')
 elif i in (28,47,67):
  for R in (.45,.8,1.25):tor(a,P[i],R,.028,'cyan' if R<1 else 'amber','z',pv,'VolumetricStatusHologram',68)
  for j in range(12):
   t=j*math.tau/12
   bm(a,P[i],(x+1.22*math.cos(t),y+.8*math.sin(t),z),.012,'green' if j%2 else 'blue',pv,'HolographicFieldVectorRay')
 elif i==29:
  disk_at(a,P[i],.7,'structural',pv)
  for j in range(16):
   t=j*math.tau/16
   cy(a,(x+.65*math.cos(t),y+.65*math.sin(t),z+.23),.095,.21,'gold','z',pv,'EmergencyMechanicalTouchdownRoller',12)
 elif i==30:
  panel(a,P[i],1.1,'amber','plc',pv)
  for j in range(12):b(a,(x-.45+j*.08,y+.38,z-.39),(.035,.32,.12),'copper',pv,'AmplifierExtrudedHeatSinkFin')
 elif i in (32,33):
  pvv=pv
  R=5.95 if i==32 else 5.53
  for k in range(36 if i==32 else 54):
   t=k*math.tau/(36 if i==32 else 54)
   sector(a,RING_CENTER,R,.19,t-.041,t+.041,.23,'copper' if i==32 else 'magnet',pvv,3,'SegmentedRimDrivePole')
  tor(a,RING_CENTER,R,.074,'gold' if i==32 else 'structural','z',pv,'MotorDriveRotorOrStatorTrack',128)
 elif i in (34,35,39,42,69,70,71,73,74,75,76,81,83,85):
  if i==81:
   b(a,P[i],(12.4,2.15,.24),'dark',None,'OverheadMainGravitySCADABoard')
   b(a,(x,y,z+.16),(12.0,1.90,.06),'panel',pv,'LiveSCADAHighResolutionGlass')
   for j in range(20):b(a,(x-5.4+j*.56,y-.4+(j%6)*.21,z+.2),(.39,.022,.02),'green' if j%5 else 'amber',pv,'RealTimeRingTelemetryPlot')
   for j in range(7):tor(a,(x-4.2+j*1.40,y-.70,z+.22),.21,.021,'cyan','z',pv,'SevenSystemQualificationStatusDial',24)
  else:panel(a,P[i],1.02,'red' if i==72 else 'cyan','plc',pv)
 elif i in (40,60):
  panel(a,P[i],.95,'amber','plc',pv)
  cy(a,(x+.25,y+.15,z+.44),.16,.12,'gold','z',pv,'MotorizedPrechargeContactorSwitch',20)
 elif i in (41,43):
  b(a,P[i],(1.25,.73,.88),'graphite',None,'IndustrialFlywheelOrCoolingManifold')
  for j in range(10):tor(a,(x-.34+j*.08,y+.12,z),.30,.021,'copper','x',pv,'InverterCoolingThermalCoils',22)
  cy(a,(x+.5,y,z),.29,.25,'titanium','x',pv,'ActiveCounterTorqueFlywheelOrFan',28)
 elif i in (49,50):
  b(a,(x,y,z),(1.2,.32,.67),'graphite',None,'AdjustableRotorBalanceMassGuide')
  for j in range(8):b(a,(x-.52+j*.15,y+.21,z),(.047,.07,.46),'gold',pv,'LinearPrecisionBalanceScaleMark')
  b(a,(x+.17,y+.33,z),(.34,.32,.44),'copper',pv,'MovableCounterweightBlock')
 elif i in (57,58):
  b(a,(x,y-.15,z),(1.15,.61,.78),'graphite',None,'RotaryInductivePowerCouplerHousing')
  tor(a,(x,y+.25,z+.3),.44,.07,'copper','z',pv,'TransmitterCopperLitzWinding',42)
  tor(a,(x,y+.25,z+.47),.34,.05,'gold','z',pv,'SecondaryRotatingPickupCoil',42)
  for j in range(10):cy(a,(x-.46+j*.1,y-.35,z+.43),.035,.11,'cyan','z',None,'CouplerActiveTerminal',10)
 elif i==59:
  panel(a,P[i],.95,'cyan','optical',pv)
  tor(a,(x,y+.38,z+.43),.23,.025,'cyan','z',pv,'OpticalRotaryTelemetryEmitterRing',32)
 elif i in (61,62,63):
  panel(a,P[i],1.05,'green' if i==62 else 'amber','instrument',pv)
  for j in range(9):b(a,(x-.38+j*.09,y-.50,z+.39),(.05,.07,.07),'gold',None,'HabitatLoadSafetySwitch')
 elif i in (72,77,78,79):
  panel(a,P[i],.97,'red','fault',pv)
  cy(a,(x+.23,y+.30,z+.47),.12,.14,'red','z',pv,'GuardedCriticalFaultInjectOrStopControl')
 elif i==80:
  panel(a,P[i],.99,'red','plc',pv)
  b(a,(x-.1,y-.24,z+.48),(.62,.12,.08),'red',pv,'W137RemovableTrustedFeedbackPatchPlate')
  for j in range(6):cy(a,(x-.37+j*.15,y-.48,z+.49),.046,.06,'gold','z',None,'W137DisconnectablePatchContact',12)
 elif i==82:
  cy(a,(x,y-.3,z),.66,.45,'gold','y',pv,'MissionQualificationBeaconEnclosure',38)
  tor(a,(x,y+.04,z),.82,.062,'green','y',pv,'QualifiedGravityOrbitIlluminatedRing',64)
  cy(a,(x,y+.42,z),.21,.40,'cyan','y',pv,'GravitySuccessBeaconCore',20)
 elif i==84:
  b(a,(x,y-1,z),(.33,3,.33),'structural',None,'OrbitalInspectionGantryMount')
  panel(a,P[i],.70,'cyan','optical',pv)
 else: panel(a,P[i],1,'cyan','instrument',pv)
 # guarantee each pivot has nonempty mesh geometry, including indicator anchors
 used={t[3] for t in a.raw if t[3]}
 for pivot in pts:
  if pivot not in used:
   xx,yy,zz=PIVOT_POS[pivot]
   b(a,(xx,yy,zz+.12),(.105,.08,.035),'cyan' if next(p['Kind'] for p in PIVOTS if p['Pivot']==pivot)=='indicator' else 'gold',pivot,'NamedMotionOrStatusAnchorMarker')
 return a

# --- Wiring concepts. EXACT 158 planned wires, no gameplay wires in assets. ---
WIRES=[];GROUPS=[]
def add(src,dst,desc,kind='DIGITAL_CONTROL'):
 WIRES.append(dict(WireID='W%03d'%(len(WIRES)+1),SourceModel=NAMES[src],TargetModel=NAMES[dst],SignalPurpose=desc,SignalKind=kind,Status='CONCEPT_ONLY_NOT_PLAYER_WIRE'))
def group(name,rows):
 first=len(WIRES)+1
 for row in rows:add(*row)
 GROUPS.append(dict(Group=name,StartWire='W%03d'%first,EndWire='W%03d'%len(WIRES),Count=len(rows)))
group('Protected educational 5 V and returns verification',[
 (73,69,'Safety supervisory PLC regulated logic supply','POWER_CONTROL'),(73,70,'Bearing safety PLC regulated logic supply','POWER_CONTROL'),
 (73,74,'Magnetic bearing control PLC supply','POWER_CONTROL'),(73,75,'Main motor control PLC supply','POWER_CONTROL'),
 (73,76,'Habitat electrical distribution PLC supply','POWER_CONTROL'),(73,81,'SCADA readout logic supply','POWER_CONTROL'),
 (73,71,'Independent overspeed logic interface','POWER_CONTROL'),(73,30,'Bearing amplifier controller supply','POWER_CONTROL'),
 (73,34,'Motor inverter A control logic supply','POWER_CONTROL'),(73,35,'Motor inverter B control logic supply','POWER_CONTROL'),
 (73,80,'Critical W137 patch monitor supply','POWER_CONTROL'),(73,83,'Mission event recorder digital supply','POWER_CONTROL')])
mag=[]
for i in range(8):
 station=16+i
 mag += [(74,station,f'Bearing {i+1} magnetic current request','ACTUATOR_COMMAND'),
         (station,74,f'Bearing {i+1} measured displacement and field','ANALOG_FEEDBACK')]
mag += [(24,74,'Primary calibrated radial gap feedback','ANALOG_FEEDBACK'),(25,74,'Redundant radial gap comparison','ANALOG_FEEDBACK'),
(26,74,'Axial gap displacement feedback','ANALOG_FEEDBACK'),(27,74,'Measured stator coil current','ANALOG_FEEDBACK'),
(74,30,'Bearing amplifier setpoint','ACTUATOR_COMMAND'),(30,74,'Bearing amplifier actual current limits','ANALOG_FEEDBACK'),
(74,29,'Touchdown bearing actuation command','SAFETY_CONTROL'),(29,70,'Touchdown bearing engaged state','DIGITAL_STATUS'),
(31,74,'Magnetic gap home-reference calibration','ANALOG_FEEDBACK'),(74,28,'Magnetic field vector visualization telemetry','DATA_STREAM')]
assert len(mag)==26
group('Eight active magnetic bearings and levitation sensing',mag)
motor=[(75,34,'Inverter A torque phase modulation request','ACTUATOR_COMMAND'),(75,35,'Inverter B torque phase modulation request','ACTUATOR_COMMAND'),
(34,32,'Motor A stator phase winding request','ACTUATOR_COMMAND'),(35,32,'Motor B stator phase winding request','ACTUATOR_COMMAND'),
(33,38,'Rotor magnet encoder reference','ANALOG_FEEDBACK'),(38,75,'Actual rotating ring angular velocity','ANALOG_FEEDBACK'),
(36,75,'Three phase motor current samples','ANALOG_FEEDBACK'),(37,75,'Measured motor torque estimator output','ANALOG_FEEDBACK'),
(75,40,'Drive precharge contactor request','ACTUATOR_COMMAND'),(40,75,'Drive precharge actual feedback','ANALOG_FEEDBACK'),
(41,34,'Drive cooling manifold health and cooling flow','ANALOG_FEEDBACK'),(41,35,'Inverter B cooling capacity','ANALOG_FEEDBACK'),
(39,75,'Authorized jerk-limited speed profile','DIGITAL_CONTROL'),(42,75,'Actual versus requested acceleration profile','DATA_STREAM'),
(43,69,'Station reaction-torque balance telemetry','ANALOG_FEEDBACK'),(75,43,'Countermoment flywheel command','ACTUATOR_COMMAND'),
(34,36,'Motor A inverter output phase-current readout','ANALOG_FEEDBACK'),(35,36,'Motor B inverter output phase-current readout','ANALOG_FEEDBACK'),
(75,39,'Motor speed ramp request','DIGITAL_CONTROL'),(38,39,'Spinup reference measurement timestamps','ANALOG_FEEDBACK'),
(34,75,'Motor A inverter fault and temperature status','DIGITAL_STATUS'),(35,75,'Motor B inverter fault and temperature status','DIGITAL_STATUS')]
assert len(motor)==22;group('Rim motor inverters and angular speed regulation',motor)
vib=[(44,45,'Actual multi-axis vibration samples','ANALOG_FEEDBACK'),(46,45,'Rotor shaft orbit probe trace','ANALOG_FEEDBACK'),
(45,47,'Measured vibration magnitude and synchronous phase','DATA_STREAM'),(47,48,'Imbalance force vector estimate','DATA_STREAM'),
(48,74,'Adaptive damping bearing current correction','ACTUATOR_COMMAND'),(48,75,'Resonance-aware motor torque limit request','SAFETY_CONTROL'),
(51,47,'Measured rotating mass asymmetry','ANALOG_FEEDBACK'),(52,45,'Vibration-frequency excitation test request','DIGITAL_CONTROL'),
(45,52,'Calibrated frequency sweep measurement','DATA_STREAM'),(47,49,'Balance weight A calibrated position request','ACTUATOR_COMMAND'),
(47,50,'Balance weight B calibrated position request','ACTUATOR_COMMAND'),(49,54,'Balance A achieved actuator position','ANALOG_FEEDBACK'),
(50,54,'Balance B achieved actuator position','ANALOG_FEEDBACK'),(53,54,'Bearing housing temperature history','ANALOG_FEEDBACK'),
(44,54,'Axial vibration cross-check telemetry','ANALOG_FEEDBACK'),(54,55,'Rotor diagnostic validity and run evidence','DATA_STREAM'),
(56,45,'Controlled vibration instrumentation fault diagnostic','DIGITAL_STATUS'),(45,69,'Independent vibration overlimit status','SAFETY_CONTROL'),
(54,81,'Rotating machine vibration trend telemetry','DATA_STREAM'),(48,81,'Damping controller effectiveness status','DATA_STREAM')]
assert len(vib)==20;group('Rotor vibration, resonance and dynamic balance',vib)
rotary=[(76,57,'Primary contactless habitat power authorization','DIGITAL_CONTROL'),(76,58,'Backup contactless power authorization','DIGITAL_CONTROL'),
(57,76,'Primary rotating power transfer measured efficiency','ANALOG_FEEDBACK'),(58,76,'Backup rotating power transfer measured efficiency','ANALOG_FEEDBACK'),
(59,64,'Optical rotary telemetry good packets','DATA_STREAM'),(64,76,'Rotary signal timestamp integrity','DATA_STREAM'),
(76,60,'Contactless supply isolation contactor request','ACTUATOR_COMMAND'),(60,76,'Actual isolation mechanism feedback','ANALOG_FEEDBACK'),
(61,76,'Habitat real electrical load demand','ANALOG_FEEDBACK'),(62,76,'Emergency lighting continuity status','SAFETY_CONTROL'),
(63,76,'Thermal circulation and habitat HVAC load','ANALOG_FEEDBACK'),(66,76,'Habitat cargo/passenger load redistribution','ANALOG_FEEDBACK'),
(76,65,'Rotary power data acquisition request','DIGITAL_CONTROL'),(65,76,'Measured rotary power V I P quality','ANALOG_FEEDBACK'),
(68,76,'Predicted load margin and transfer efficiency','DATA_STREAM'),(76,68,'Power budget recalculation request','DIGITAL_CONTROL'),
(76,61,'Habitat equipment permitted load state','DIGITAL_CONTROL'),(76,62,'Critical habitat lighting permission','SAFETY_CONTROL'),
(76,63,'Habitat thermal load permission','DIGITAL_CONTROL'),(59,76,'Secondary optical rotary communications link','DATA_STREAM'),
(66,68,'Cargo shift and predicted mission demand','DATA_STREAM'),(67,81,'Artificial gravity live radius and RPM hologram','DATA_STREAM')]
assert len(rotary)==22;group('Noncontact rotating power and data transfer',rotary)
thermal=[(53,30,'Bearing temperature amplifier derating signal','ANALOG_FEEDBACK'),(30,69,'Bearing amplifier thermal condition','ANALOG_FEEDBACK'),
(41,69,'Drive cooling actual flow and availability','ANALOG_FEEDBACK'),(61,68,'Habitat essential equipment electrical demand','ANALOG_FEEDBACK'),
(62,68,'Emergency lighting branch power demand','ANALOG_FEEDBACK'),(63,68,'Habitat thermal controller measured demand','ANALOG_FEEDBACK'),
(66,51,'Dynamic cargo mass distribution scan request','DIGITAL_CONTROL'),(51,66,'Measured mass distribution time series','DATA_STREAM'),
(68,75,'Motor drive power reserve forecast','DATA_STREAM'),(68,69,'Thermal and electrical system safety margin','DATA_STREAM'),
(65,81,'Power transfer operational reserve status','DATA_STREAM'),(63,81,'Habitat thermal load historical sample','DATA_STREAM'),
(62,81,'Critical habitat lighting status','DATA_STREAM'),(30,81,'Amplifier current and temperature SCADA telemetry','DATA_STREAM'),
(57,65,'Primary coupler output energy measurement','ANALOG_FEEDBACK'),(58,65,'Backup coupler output energy measurement','ANALOG_FEEDBACK'),
(60,65,'Actual contactor transmission connection','DIGITAL_STATUS'),(67,81,'Gravity field radial display update','DATA_STREAM'),
(68,83,'Power thermal mission interval certificate','DATA_STREAM'),(51,83,'Load profile and mass perturbation event evidence','DATA_STREAM')]
assert len(thermal)==20;group('Thermal management and simulated habitat mission loads',thermal)
safe=[(72,69,'Emergency stop physical wired safety status','SAFETY_CONTROL'), # W123
(85,69,'Independent isolation relay operational state','SAFETY_CONTROL'),
(71,69,'Independent overspeed status','SAFETY_CONTROL'),
(38,71,'Independent angular speed sample','ANALOG_FEEDBACK'),
(69,75,'Main drive safety permission','SAFETY_CONTROL'),
(69,74,'Magnetic bearing stabilization safety permit','SAFETY_CONTROL'),
(69,76,'Rotary power safety isolation authorization','SAFETY_CONTROL'),
(70,74,'Bearing safe-mode command and measured readiness','SAFETY_CONTROL'),
(77,24,'Injected primary radial sensor drift bias','DIGITAL_CONTROL'),
(78,30,'Injected bearing amplifier thermal fault','DIGITAL_CONTROL'),
(79,51,'Injected rotating cargo imbalance','DIGITAL_CONTROL'),
(70,29,'Touchdown support protective engagement','SAFETY_CONTROL'),
(25,70,'Redundant sensor cross-check diagnostic','ANALOG_FEEDBACK'),
(80,70,'Critical feedback patch continuity monitor ONLY','DIGITAL_STATUS'),
(24,70,'CRITICAL trusted radial displacement feedback W137','ANALOG_FEEDBACK'), # W137
(70,69,'Safety critical feedback fault latch status','DIGITAL_STATUS'),
(69,83,'Safety-critical fault and event sequence record','DATA_STREAM'),
(69,80,'W137 recovery safety qualification display','DIGITAL_STATUS'),
(83,69,'Operator explicit fault reset request','SAFETY_CONTROL'),
(70,82,'Independent bearing safety final qualified status','DIGITAL_STATUS')]
assert len(safe)==20;assert len(WIRES)==122
group('Independent safety, W137 and three disturbances',safe)
assert WIRES[136]['WireID']=='W137' and WIRES[136]['SourceModel']==NAMES[24] and WIRES[136]['TargetModel']==NAMES[70]
scada=[(69,81,'Gravity operating state and safety telemetry','DATA_STREAM'),(74,81,'Magnetic bearing actual gap currents and quality','DATA_STREAM'),
(75,81,'Motor RPM torque acceleration live status','DATA_STREAM'),(45,81,'Rotor vibration spectrum and phase traces','DATA_STREAM'),
(65,81,'Rotary transfer power and efficiency readout','DATA_STREAM'),(68,81,'Habitat thermal and power budget','DATA_STREAM'),
(70,81,'W137 trusted feedback timestamp and safety validity','DIGITAL_STATUS'),(71,81,'Independent overspeed shutdown status','DIGITAL_STATUS'),
(81,83,'Mission-wide qualified event and run recording','DATA_STREAM'),(83,69,'Fresh full qualification run evidence fingerprint','DATA_STREAM'),
(69,82,'Final mission permission after all safety passes','DIGITAL_STATUS'),(82,81,'Certification beacon actual status telemetry','DIGITAL_STATUS'),
(80,81,'W137 display patch diagnostic only, no bypass','DATA_STREAM'),(54,81,'Rotor recorder health and sample timeline','DATA_STREAM'),
(67,81,'Gravity digital twin radial field visualization','DATA_STREAM'),(81,82,'Six-authoritative-objective status display','DATA_STREAM')]
assert len(scada)==16;group('SCADA, diagnostics and six-objective certification',scada)
assert len(WIRES)==158,len(WIRES)
# 236 distinct modeled socket anchors; 316 logical endpoint identities.
usage=Counter()
for w in WIRES:usage[w['SourceModel']]+=1;usage[w['TargetModel']]+=1
eligible=sorted(NAMES.index(k) for k in usage)
counts={i:1 for i in eligible}
for i in [24,70,73,74,75,81,80]:counts[i]=max(counts[i],3)
while sum(counts.values())<236:
 i=max(eligible,key=lambda j:(usage[NAMES[j]]/(counts[j]+.5),-j));counts[i]+=1
assert sum(counts.values())==236
ANCHORS=[];model_anchors=defaultdict(list)
for i in sorted(counts):
 x,y,z=P[i];n=counts[i]
 for k in range(n):
  ax=x+((k%9)-4)*(.085 if i in range(5,24) else .105)
  ay=y+((k//9)%5-1.8)*.105
  az=z+(.40 if i>23 and i not in [32,33,49,50,57,58] else .21)
  aid='P%03d'%(len(ANCHORS)+1)
  an=dict(AnchorID=aid,Model=NAMES[i],WorldX_m=round(ax,5),WorldY_m=round(ay,5),WorldZ_m=round(az,5),Status='MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL')
  ANCHORS.append(an);model_anchors[i].append(an)
assert len(ANCHORS)==236
slot_count=defaultdict(int);LOGICAL=[]
for w in WIRES:
 num=int(w['WireID'][1:]);w['Critical']='YES' if num==137 else 'NO'
 w['Group']=next(g['Group'] for g in GROUPS if int(g['StartWire'][1:])<=num<=int(g['EndWire'][1:]))
 for side in ['Source','Target']:
  i=NAMES.index(w[side+'Model']);n=slot_count[i];slot_count[i]+=1
  art_anchor=model_anchors[i][(n*3+num)%len(model_anchors[i])]['AnchorID']
  w[side+'VisualAnchor']=art_anchor
  LOGICAL.append(dict(WireID=w['WireID'],Endpoint=side,Model=w[side+'Model'],ArtAnchor=art_anchor,
    FunctionalPortID=f"{w['WireID']}_{'OUT' if side=='Source' else 'IN'}_{w['SignalKind']}",
    SignalKind=w['SignalKind'],Direction='OUTPUT' if side=='Source' else 'INPUT',Critical=w['Critical'],
    FunctionalStatus='PROPOSED_PORT_NOT_UNITY_SPARKTERMINAL'))
assert len(LOGICAL)==316
anchor_use=defaultdict(list)
for p in LOGICAL:anchor_use[p['ArtAnchor']].append(p)
CONFLICTS=[dict(AnchorID=aid,Model=v[0]['Model'],LogicalContactCount=len(v),SignalKinds=';'.join(sorted({p['SignalKind'] for p in v})),
       DifferentSignalKinds=len(set(p['SignalKind'] for p in v))>1,Status='MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS')
       for aid,v in anchor_use.items() if len(v)>1]

OBJECTIVES=[
 ('1','Build the Gravity Control Network','158 actual player-created correct topology connections with logical socket reconciliation; 5 V and real returns; safety valid; W137 trusted.','Real Spark electrical graph, sources and returns, correct terminal domains, evidence hash'),
 ('2','Commission the Magnetic Levitation System','Eight bearing stations, radial/axial sensing, amplifier limits and touchdown transition physically stable.','Measured actual air gaps, coil currents, qualified temperature and valid timestamped calibration'),
 ('3','Accelerate the Artificial Gravity Ring','Controlled motor torque and angular dynamics, acceptable vibration, independent overspeed check, momentum protection and target RPM.','Actual simulated angular velocity, drive torque and stored momentum history'),
 ('4','Maintain Stable Artificial Gravity','Sustained stable ring speed, load distribution, bearing thermal health, rotary data and mission essential loads preserved.','Fresh complete sustained runID, physics and power balance, load deadlines, vibration/temperature checks'),
 ('5','Recover Three Critical Rotation Failures','Real sensor bias, amplifier overheating and mass imbalance disturbances each diagnosed and repaired; new full stabilization run.','Causal fault/repair record; fresh spin-up, bearing and sustained mission proof'),
 ('6','Restore Critical Magnetic Bearing Feedback','Physical W137 disconnect after 1-5 historical certifications, safe control response, reconnect, fresh calibration, explicit reset, new full operating cycle.','Real topology change; latched fault; independent protection; new runID; actual LevelGamePlayManager 6/6')]

def csv_write(path,rows,fields=None):
 with open(path,'w',newline='',encoding='utf8') as f:
  fieldnames=fields or list(rows[0]);writer=csv.DictWriter(f,fieldnames=fieldnames,extrasaction='ignore');writer.writeheader();writer.writerows(rows)
def dump(p,v):Path(p).write_text(json.dumps(v,indent=2),encoding='utf8')
def metadata():
 csv_write(ROOT/'Gameplay_Wiring_Concept_W001_W158.csv',WIRES)
 csv_write(ROOT/'Gameplay_Wiring_Budget.csv',GROUPS)
 csv_write(ROOT/'Terminal_Anchor_Plan_236.csv',ANCHORS)
 csv_write(ROOT/'Visual_Terminal_Sockets_236.csv',ANCHORS)
 csv_write(ROOT/'Terminal_Logical_Port_Reconciliation_316.csv',LOGICAL)
 csv_write(ROOT/'Terminal_Anchor_Signal_Conflict_Report.csv',CONFLICTS)
 csv_write(ROOT/'Objective_Certification_Plan_6.csv',[dict(ObjectiveIndex=idx,Title=title,AcceptanceCriteria=criteria,Evidence=evidence,Implemented='NO') for idx,title,criteria,evidence in OBJECTIVES])
 csv_write(ROOT/'Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PIVOTS])
 csv_write(ROOT/'Component_Roles.csv',[dict(Index=i,Model=n,WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],Role=('Rotating ring assembly' if i in list(range(2,12))+[33] else 'Bearing system' if 16<=i<=31 else 'Electrical/mechatronic subsystem'),State='ART_ONLY') for i,n in enumerate(NAMES)])
 csv_write(ROOT/'Art_Assembly.csv',[dict(Index=i,Model=n,File=f'{i:02d}_{n}.fbx',TransformOrigin='0,0,0',IntendedWorldCenter_m=','.join(map(str,P[i])),NumericFBXUnits='centimetres',GLBUnits='metres') for i,n in enumerate(NAMES)])
 (ROOT/'W137_Critical_Radial_Feedback_Recovery.md').write_text(f'''# Level 87 — W137 critical trusted radial displacement feedback

**VERIFIED SOURCE PLANNED WIRE:** `W137`: `{NAMES[24]}` `({WIRES[136]['SourceVisualAnchor']})` → `{NAMES[70]}` `({WIRES[136]['TargetVisualAnchor']})`; `ANALOG_FEEDBACK`.

Source logical port: `TRUSTED_RADIAL_GAP_OUT`. Destination logical port: `TRUSTED_RADIAL_GAP_IN`.

1. After historical Objectives 1–5 certify, arm supervised low-speed exercise with independent touchdown/overspeed protection available.
2. Player must disconnect an **actual runtime** W137 SparkCircuitSystem wire; source art contains no connected wire.
3. Safety PLC invalidates *new and cached* primary radial-gap samples; does not interpret missing as zero displacement.
4. Withdraw normal high-speed and contactless-bearing control authorization; command controlled rundown if redundant sensors, energy, motor torque and mechanisms permit; otherwise protective response, retained independent overspeed and touchdown systems.
5. Latch critical safety fault and record event evidence; old Objectives 1–5 remain historically valid. Never magically stop angular velocity or fully disable a physically spinning ring.
6. Player reconnects correct compatible port, restores actual electrical continuity, receives genuinely fresh calibrated samples; verify backup sensor, overspeed relay, bearing temperatures, power and touchdown readiness.
7. Explicit operator reset **after** preconditions; create new full run ID, recommission magnetic levitation, accelerate, qualify sustained spin and habitat loads.
8. Only the real LevelGamePlayManager may reach 6/6 after fresh complete certification. W137 reconnect by itself can never count.

Non-authoritative SCADA, patch monitor and redundant diagnostic display never replace missing trusted W137 feedback. These steps are proposed implementation requirements, not present in the art asset files.
''',encoding='utf8')

ANCHOR_PER_MODEL=defaultdict(list)
for an in ANCHORS:ANCHOR_PER_MODEL[an['Model']].append(an)
def art_with_sockets(i):
 a=make_art(i)
 for s in ANCHOR_PER_MODEL[NAMES[i]]:a.socket((s['WorldX_m'],s['WorldY_m'],s['WorldZ_m']),s['AnchorID'])
 return a

def homes(i,a):
 out=[(p['Pivot'],a.name,PIVOT_POS[p['Pivot']]) for p in PIVOTS if p['Index']==i]
 out.extend(('SOCKET_'+aid,a.name,xyz) for aid,xyz in a.sockets)
 return out

def exports():
 start=time.monotonic();models=[];allparts=[];allhomes=[];bmin=np.ones(3)*float('inf');bmax=-bmin
 for i,n in enumerate(NAMES):
  art=art_with_sockets(i);parts=core.chunk_model(art,max_shapes=18);home=homes(i,art)
  fbx=ROOT/'Models'/f'{i:02d}_{n}.fbx';glb=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  fr=core.export_fbx(str(fbx),[(n,p) for p in parts],[n],home,art.sockets)
  gr=core.export_glb(str(glb),[(n,p) for p in parts],[n],home)
  sockets=dict(art.sockets)
  for part in parts:
   pivot=part['pivot'];offset=PIVOT_POS[pivot] if pivot in PIVOT_POS else sockets[pivot[7:]] if pivot and pivot.startswith('SOCKET_') else (0,0,0)
   pts=part['vertices']+np.asarray(offset)
   bmin=np.minimum(bmin,pts.min(axis=0));bmax=np.maximum(bmax,pts.max(axis=0))
  models.append(dict(index=i,name=n,file=fbx.name,glb=glb.name,triangle_count=sum(len(pt['faces']) for pt in parts),mesh_parts=len(parts),
    source_primitive_count=len(art.raw),visual_terminal_sockets=len(art.sockets),
    animation_pivots={pv:xyz for pv,rt,xyz in home if pv.startswith('ANIM_')},fbx_bytes=fr['file_size'],glb_bytes=gr['glb_bytes']))
  allparts.extend((n,p) for p in parts);allhomes.extend(home)
  if (i+1)%10==0 or i in (0,2,85):print('MODEL',i+1,'/86',n,models[-1]['triangle_count'],'tris',round(time.monotonic()-start,1),'s',flush=True)
 print('EXPORTING ASSEMBLED SHOWCASE',len(allparts),'mesh groups',flush=True)
 ff=ROOT/'Models'/'Level87_Complete_ArtKit_Showcase.fbx';gg=ROOT/'GLB'/'Level87_Complete_ArtKit_Showcase.glb'
 fr=core.export_fbx(str(ff),allparts,NAMES,allhomes,[(a['AnchorID'],(a['WorldX_m'],a['WorldY_m'],a['WorldZ_m'])) for a in ANCHORS]);gr=core.export_glb(str(gg),allparts,NAMES,allhomes)
 manifest=dict(level_id='level_87',display_name='The Gravity Engine',subtitle='Artificial Gravity, Magnetic Bearings and Rotation Control',
  individual_models=len(models),individual_glbs=len(models),assembled_fbx='Models/'+ff.name,assembled_glb='GLB/'+gg.name,
  individual_triangles=sum(m['triangle_count'] for m in models),individual_mesh_parts=sum(m['mesh_parts'] for m in models),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),
  proposed_independent_logical_wire_endpoints=len(LOGICAL),anchor_reconciliation_conflicts=len(CONFLICTS),
  visual_ring_radius_m=5.2,gameplay_proposed_full_ring_radius_m=45,proposed_demo_target_rpm=3,
  workstation_world_bounds_m=[[round(float(x),5) for x in bmin],[round(float(x),5) for x in bmax]],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up X-width Z-forward',
  assembly_origin=[0,0,0],actual_player_wires_included=False,functional_sparkterminals_included=False,
  actual_magnetic_bearing_physics_included=False,actual_gravity_ring_simulator_included=False,
  six_runtime_objectives_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
  assembled_fbx_bytes=fr['file_size'],assembled_glb_bytes=gr['glb_bytes'],models=models)
 dump(ROOT/'Model_Manifest.json',manifest)
 return manifest
if __name__=='__main__':
 print('CREATING LEVEL 87 PHYSICAL FBX/GLB 3D ART',flush=True)
 metadata(); m=exports();print('DONE',m['individual_triangles'],'triangles',flush=True)
