#!/usr/bin/env python3
"""Project Spark Level 84 procedural original modular art generator.
Generates real FBX 7400 and glTF 2 GLB, all positions in metres internally.
FBX geometry numeric positions stored in centimetres for standard FBX import.
No gameplay logic or functional SparkTerminals are generated here.
"""
import os, io, csv, json, math, struct, zlib, random, hashlib, zipfile, time
from collections import defaultdict, Counter
import numpy as np
import trimesh
from PIL import Image
ROOT='/mnt/data/ProjectSpark_Level84_HD_AAA'
D=math.pi/180
random.seed(84084)
NAMES='''
L84_CelestialDynamicsArenaBase
L84_PanoramicStarfieldSphere
L84_ThreeAxisSpacecraftTestRig
L84_RotationalTestRigPedestal
L84_SpacecraftAttitudeTestVehicle
L84_ProtectedAvionicsChassis
L84_FourChipletMissionPayload
L84_CoPackagedOpticalTransmitter
L84_PortSolarArrayWing
L84_StarboardSolarArrayWing
L84_HolographicEarthReferenceSphere
L84_StellarNavigationCatalogDome
L84_InertialReferenceTargetHologram
L84_OpticalGroundStationAimReticle
L84_OrbitalEphemerisDisplayRail
L84_AngularMomentumVectorHologram
L84_SpacecraftBodyAxesHologram
L84_ReactionWheelClusterHousing
L84_ReactionWheelX
L84_ReactionWheelY
L84_ReactionWheelZ
L84_SkewBackupReactionWheel
L84_ReactionWheelMotorDriverRack
L84_ReactionWheelTachometerArray
L84_ReactionWheelMotorCurrentMonitor
L84_ReactionWheelThermalControlModule
L84_MagneticTorqueRodX
L84_MagneticTorqueRodY
L84_MagneticTorqueRodZ
L84_MagneticTorqueRodDriveController
L84_TriaxialMagnetometer
L84_OrbitalMagneticFieldEmulator
L84_HolographicMagneticFieldLines
L84_MomentumDesaturationController
L84_PrecisionGyroscopeIMU
L84_GyroscopeCalibrationStation
L84_GyroBiasThermalFaultEmulator
L84_PrimaryStarTracker
L84_StarTrackerOpticalBaffle
L84_SecondaryDiagnosticStarTracker
L84_StarCatalogPatternMatcher
L84_CoarseSunSensorCluster
L84_EarthHorizonSensor
L84_AttitudeStateEstimator
L84_ComplementaryFusionDisplay
L84_HolographicQuaternionAttitudeSphere
L84_EstimatorUncertaintyDisplay
L84_AttitudeFlightController
L84_ThreeAxisTorqueAllocationComputer
L84_SlewTrajectoryPlanner
L84_PrecisionPointingErrorAnalyzer
L84_InertialReferenceClock
L84_ExternalDisturbanceTorqueInjector
L84_GravityGradientTorqueEmulator
L84_OrbitalDragTorqueEmulator
L84_SolarRadiationPressureTorqueModel
L84_SolarWingFlexibleModeEmulator
L84_StructuralVibrationSensor
L84_ActivePointingVibrationFilter
L84_OpticalLineOfSightSensor
L84_GroundStationTargetProjector
L84_OpticalLinkAcquisitionFixture
L84_OpticalSignalQualityTelemetry
L84_MissionPayloadBoresightReference
L84_SunSafeAttitudeController
L84_AttitudeSafeModeSupervisor
L84_ReactionWheelSaturationAlarm
L84_ReactionWheelDesaturationSupervisor
L84_CriticalAttitudeFeedbackStation
L84_IntegratedADCSPLCRack
L84_Regulated5VControlSupply
L84_IndependentMotionSafetyStation
L84_HolographicAttitudeControlSCADA
L84_ADCSCertificationBeacon
L84_EmergencyAttitudeStopStation
L84_AttitudeFlightDataRecorder'''.strip().splitlines()
assert len(NAMES)==76
PIVOT_GROUPS={
'Celestial arena and three-axis rig':'''ANIM_OUTER_YAW_RING ANIM_MIDDLE_PITCH_RING ANIM_INNER_ROLL_RING ANIM_SPACECRAFT_BODY_ATTITUDE ANIM_EARTH_REFERENCE_ROTATION ANIM_STARFIELD_DOME ANIM_GROUND_STATION_TARGET ANIM_ORBIT_REFERENCE_GRID ANIM_ANGULAR_MOMENTUM_VECTOR ANIM_TEST_RIG_LOCK''',
'Reaction wheels and magnetic rods':'''ANIM_WHEEL_X_ROTOR ANIM_WHEEL_Y_ROTOR ANIM_WHEEL_Z_ROTOR ANIM_WHEEL_SKEW_ROTOR ANIM_TORQUE_ROD_X_VECTOR ANIM_TORQUE_ROD_Y_VECTOR ANIM_TORQUE_ROD_Z_VECTOR ANIM_WHEEL_DRIVER_X_PANEL ANIM_WHEEL_DRIVER_Y_PANEL ANIM_WHEEL_DRIVER_Z_PANEL ANIM_WHEEL_TACHOMETER_SCAN ANIM_WHEEL_COOLING_FAN''',
'Navigation sensors':'''ANIM_PRIMARY_STAR_TRACKER_APERTURE ANIM_STAR_TRACKER_BAFFLE ANIM_SECONDARY_TRACKER ANIM_GYRO_CALIBRATION_SPINDLE ANIM_GYRO_BIAS_DISPLAY ANIM_SUN_SENSOR_SCAN ANIM_EARTH_HORIZON_SCAN ANIM_MAGNETOMETER_SCAN ANIM_STAR_CATALOG_MATCH ANIM_ATTITUDE_QUATERNION ANIM_ESTIMATOR_UNCERTAINTY_CONE ANIM_OPTICAL_LINE_OF_SIGHT''',
'Navigation control and momentum':'''ANIM_FLIGHT_CONTROLLER_HOLOGRAM ANIM_SLEW_TRAJECTORY_DISPLAY ANIM_TORQUE_ALLOCATION_DISPLAY ANIM_REACTION_WHEEL_MOMENTUM_BARS ANIM_MOMENTUM_DESAT_ARC ANIM_MAGNETIC_FIELD_LINES ANIM_POINTING_TARGET_RETICLE ANIM_OPTICAL_LINK_LOCK ANIM_STRUCTURAL_MODE_TRACE ANIM_VIBRATION_FILTER_DISPLAY''',
'Faults and safety':'''ANIM_GYRO_BIAS_FAULT ANIM_WHEEL_SATURATION_FAULT ANIM_SOLAR_WING_FLEX_FAULT ANIM_SUN_SAFE_ACQUISITION ANIM_EMERGENCY_ATTITUDE_STOP ANIM_CRITICAL_W097_INDICATOR ANIM_INDEPENDENT_SAFETY_PANEL ANIM_FLIGHT_DATA_RECORDER''',
'Mission and certification':'''ANIM_OPTICAL_DOWNLINK_DISPLAY ANIM_MISSION_TIMELINE ANIM_GROUND_PASS_WINDOW ANIM_POWER_RESERVE_DISPLAY ANIM_ADCS_SCADA ANIM_ADCS_CERTIFICATION_BEACON''',
}
PIVOTS=[(p,g) for g,ls in PIVOT_GROUPS.items() for p in ls.split()]
assert len(PIVOTS)==58
# color and material labels
COLORS={
'structural':(0.35,0.45,0.56,1),'titanium':(0.57,0.64,0.68,1),'dark':(0.075,0.105,0.15,1),
'graphite':(0.11,0.16,0.2,1),'copper':(0.80,0.40,0.16,1),'gold':(0.95,0.71,0.28,1),
'ceramic':(0.80,0.83,0.81,1),'glass':(0.21,0.56,0.75,.55),'panel':(0.045,0.13,0.29,1),
'pcb':(0.07,0.24,0.20,1),'cyan':(0.22,0.80,0.99,1),'blue':(0.24,0.47,0.96,1),
'green':(0.17,0.94,0.66,1),'amber':(1.0,0.64,0.20,1),'violet':(0.66,0.38,1.0,1),
'red':(1.0,0.23,0.20,1),'orange':(1.0,0.43,0.15,1),'white':(0.91,0.96,1.0,1),
'solar':(0.055,0.16,0.41,1),'stars':(0.62,0.78,1.0,1),
}
# Mesh primitive list: each records material, optional named animation pivot and a triangle mesh in world metres.
class ModelArt:
 def __init__(self,name):self.name=name;self.raw=[];self.sockets=[]
 def mesh(self,verts,faces,mat='titanium',pivot=None,label='Detail'):
  a=np.asarray(verts,dtype=np.float64); f=np.asarray(faces,dtype=np.int32)
  if len(a)==0 or len(f)==0:return
  self.raw.append((a,f,mat,pivot,label))
 def box(self,p,s,m='titanium',pivot=None,label='MachinedHousing'):
  x,y,z=np.asarray(p,float);dx,dy,dz=np.asarray(s,float)/2
  v=np.array([[x-dx,y-dy,z-dz],[x+dx,y-dy,z-dz],[x+dx,y+dy,z-dz],[x-dx,y+dy,z-dz],[x-dx,y-dy,z+dz],[x+dx,y-dy,z+dz],[x+dx,y+dy,z+dz],[x-dx,y+dy,z+dz]])
  f=np.array([[0,2,1],[0,3,2],[4,5,6],[4,6,7],[0,1,5],[0,5,4],[3,7,6],[3,6,2],[1,2,6],[1,6,5],[0,4,7],[0,7,3]])
  self.mesh(v,f,m,pivot,label)
 def cylinder(self,p,r,h,m='structural',axis='y',seg=18,pivot=None,label='PrecisionCylinder'):
  x,y,z=p;ax={'x':0,'y':1,'z':2}[axis];vv=[]
  for t in (-h/2,h/2):
   for k in range(seg):
    a=2*math.pi*k/seg;v=[0.,0.,0.];v[ax]=t;v[(ax+1)%3]=r*math.cos(a);v[(ax+2)%3]=r*math.sin(a);vv.append((x+v[0],y+v[1],z+v[2]))
  vv += [[x-h/2 if ax==0 else x,y-h/2 if ax==1 else y,z-h/2 if ax==2 else z], [x+h/2 if ax==0 else x,y+h/2 if ax==1 else y,z+h/2 if ax==2 else z]]
  ff=[]
  for k in range(seg):
   n=(k+1)%seg;ff.extend([[k,n,seg+n],[k,seg+n,seg+k],[2*seg,n,k],[2*seg+1,seg+k,seg+n]])
  self.mesh(vv,ff,m,pivot,label)
 def torus(self,p,R,r,m='titanium',axis='z',seg=64,tube=9,pivot=None,label='RingStructure'):
  ax={'x':0,'y':1,'z':2}[axis];i=(ax+1)%3;j=(ax+2)%3;vv=[];ff=[]
  for k in range(seg):
   a=k*2*math.pi/seg
   for l in range(tube):
    b=l*2*math.pi/tube;v=np.zeros(3);v[i]=(R+r*math.cos(b))*math.cos(a);v[j]=(R+r*math.cos(b))*math.sin(a);v[ax]=r*math.sin(b)
    vv.append((np.asarray(p)+v).tolist())
  for k in range(seg):
   for l in range(tube):
    a=k*tube+l;b=k*tube+(l+1)%tube;c=((k+1)%seg)*tube+l;d=((k+1)%seg)*tube+(l+1)%tube
    ff.extend([[a,c,b],[b,c,d]])
  self.mesh(vv,ff,m,pivot,label)
 def beam(self,a,b,r,m='structural',sides=10,pivot=None,label='PrecisionBeam'):
  a=np.asarray(a,float);b=np.asarray(b,float);q=b-a; L=np.linalg.norm(q)
  if L<1e-6:return
  d=q/L; t=np.array([0,1,0]) if abs(d[1])<0.87 else np.array([1,0,0]);u=np.cross(d,t);u/=np.linalg.norm(u);v=np.cross(d,u)
  vert=[];fac=[]
  for p in [a,b]:
   for k in range(sides):
    z=k*math.tau/sides;vert.append((p+r*(math.cos(z)*u+math.sin(z)*v)).tolist())
  for k in range(sides):
   n=(k+1)%sides;fac.extend([[k,n,sides+n],[k,sides+n+n*0,sides+k]]) # n*0 harmless
  # repair second side face triangle: first [k,n,seg+n], next [k,seg+n,seg+k]
  self.mesh(vert,fac,m,pivot,label)
 def textbars(self,p,width,height,count=7,axis='x',m='cyan',pivot=None):
  x,y,z=p
  for j in range(count):
   yy=y-height/2+(j+.5)*height/count
   w=width*(.26+.7*((j*17+5)%11)/11)
   self.box((x,yy,z), (w,height/(count*3),.017),m,pivot,'TelemetryTrace')
 def socket(self,p,aid):
  self.cylinder(p,.065,.042,'gold',axis='z',seg=12,pivot='SOCKET_'+aid,label='SOCKET_'+aid)
  q=(p[0],p[1],p[2]+.035);self.cylinder(q,.040,.014,'dark',axis='z',seg=12,pivot='SOCKET_'+aid,label='SocketInsulator_'+aid)
  self.sockets.append((aid,tuple(p)))
# patch typo in function below
# World-space anchors (metres), aligned with the prior user-approved layout.
POS={
0:(0,.20,0),1:(0,6.4,-5.2),2:(0,5.05,-1.25),3:(0,1.3,-1.25),4:(0,5.06,-1.25),
5:(0,5.0,-1.0),6:(0,5.12,-.72),7:(0,5.85,-.5),8:(-5.00,5.07,-1.2),9:(5.00,5.07,-1.2),
10:(8.0,7.55,-5.3),11:(-.7,8.95,-5.7),12:(0,7.7,-5.5),13:(9.5,7.6,-5.0),14:(0,2.4,-5.28),15:(0,7.5,-4.5),16:(0,6.2,-1.2),
17:(-9.4,3.20,.1),18:(-11.55,3.15,.15),19:(-10.18,3.15,.15),20:(-8.80,3.15,.15),21:(-7.43,3.15,.15),
22:(-11.2,1.55,4.0),23:(-9.95,1.55,4.0),24:(-8.70,1.55,4.0),25:(-7.45,1.55,4.0),
26:(-2.8,2.0,-4.8),27:(0,2.0,-4.8),28:(2.8,2.0,-4.8),29:(-5.3,2.1,-4.86),30:(-5.2,3.6,-4.84),31:(-2.0,3.6,-5.15),32:(0,7.9,-5.3),33:(2.15,3.5,-5.1),
34:(7.40,2.3,3.85),35:(8.7,2.3,3.85),36:(10.0,2.3,3.85),
37:(9.1,4.20,.3),38:(10.40,4.12,.3),39:(11.55,4.2,.3),40:(9.15,2.6,.4),41:(10.45,2.6,.4),42:(11.65,2.6,.4),
43:(-5.45,1.65,4.70),44:(-4.18,1.65,4.70),45:(3.75,7.45,-5.12),46:(-2.95,1.65,4.7),
47:(-1.65,1.65,4.7),48:(-.35,1.65,4.7),49:(.95,1.65,4.7),50:(2.25,1.65,4.7),51:(3.55,1.65,4.7),
52:(-4.1,3.6,-4.8),53:(-3.0,3.75,-5.0),54:(-1.9,3.75,-5.0),55:(-.75,3.75,-5.0),56:(4.25,3.75,-5.0),57:(5.45,3.75,-5.0),58:(6.65,3.75,-5.0),
59:(7.9,3.75,-5.0),60:(9.1,3.75,-5.0),61:(10.35,3.75,-5.0),62:(11.55,3.75,-5.0),63:(3.5,5.6,-1.4),
64:(-5.42,3.65,-4.8),65:(-6.4,1.65,4.7),66:(-11.22,4.25,-.55),67:(-10.2,4.25,-.55),68:(10.30,1.64,4.7),69:(5.0,1.65,4.7),70:(6.3,1.65,4.7),71:(7.6,1.65,4.7),72:(0,7.8,-5.3),73:(0,8.60,-5.20),74:(11.15,1.52,4.85),75:(8.95,1.65,4.7),
}
assert set(POS)==set(range(76)), set(POS)
PIVOT_OWNER={
'ANIM_OUTER_YAW_RING':2,'ANIM_MIDDLE_PITCH_RING':2,'ANIM_INNER_ROLL_RING':2,'ANIM_SPACECRAFT_BODY_ATTITUDE':4,
'ANIM_EARTH_REFERENCE_ROTATION':10,'ANIM_STARFIELD_DOME':1,'ANIM_GROUND_STATION_TARGET':13,'ANIM_ORBIT_REFERENCE_GRID':14,
'ANIM_ANGULAR_MOMENTUM_VECTOR':15,'ANIM_TEST_RIG_LOCK':3,
'ANIM_WHEEL_X_ROTOR':18,'ANIM_WHEEL_Y_ROTOR':19,'ANIM_WHEEL_Z_ROTOR':20,'ANIM_WHEEL_SKEW_ROTOR':21,
'ANIM_TORQUE_ROD_X_VECTOR':26,'ANIM_TORQUE_ROD_Y_VECTOR':27,'ANIM_TORQUE_ROD_Z_VECTOR':28,
'ANIM_WHEEL_DRIVER_X_PANEL':22,'ANIM_WHEEL_DRIVER_Y_PANEL':22,'ANIM_WHEEL_DRIVER_Z_PANEL':22,
'ANIM_WHEEL_TACHOMETER_SCAN':23,'ANIM_WHEEL_COOLING_FAN':25,
'ANIM_PRIMARY_STAR_TRACKER_APERTURE':37,'ANIM_STAR_TRACKER_BAFFLE':38,'ANIM_SECONDARY_TRACKER':39,
'ANIM_GYRO_CALIBRATION_SPINDLE':35,'ANIM_GYRO_BIAS_DISPLAY':36,'ANIM_SUN_SENSOR_SCAN':41,'ANIM_EARTH_HORIZON_SCAN':42,
'ANIM_MAGNETOMETER_SCAN':30,'ANIM_STAR_CATALOG_MATCH':40,'ANIM_ATTITUDE_QUATERNION':45,
'ANIM_ESTIMATOR_UNCERTAINTY_CONE':46,'ANIM_OPTICAL_LINE_OF_SIGHT':59,
'ANIM_FLIGHT_CONTROLLER_HOLOGRAM':47,'ANIM_SLEW_TRAJECTORY_DISPLAY':49,'ANIM_TORQUE_ALLOCATION_DISPLAY':48,
'ANIM_REACTION_WHEEL_MOMENTUM_BARS':23,'ANIM_MOMENTUM_DESAT_ARC':33,'ANIM_MAGNETIC_FIELD_LINES':32,
'ANIM_POINTING_TARGET_RETICLE':50,'ANIM_OPTICAL_LINK_LOCK':61,'ANIM_STRUCTURAL_MODE_TRACE':56,'ANIM_VIBRATION_FILTER_DISPLAY':58,
'ANIM_GYRO_BIAS_FAULT':36,'ANIM_WHEEL_SATURATION_FAULT':66,'ANIM_SOLAR_WING_FLEX_FAULT':56,
'ANIM_SUN_SAFE_ACQUISITION':64,'ANIM_EMERGENCY_ATTITUDE_STOP':74,'ANIM_CRITICAL_W097_INDICATOR':68,
'ANIM_INDEPENDENT_SAFETY_PANEL':71,'ANIM_FLIGHT_DATA_RECORDER':75,
'ANIM_OPTICAL_DOWNLINK_DISPLAY':62,'ANIM_MISSION_TIMELINE':14,'ANIM_GROUND_PASS_WINDOW':60,
'ANIM_POWER_RESERVE_DISPLAY':70,'ANIM_ADCS_SCADA':72,'ANIM_ADCS_CERTIFICATION_BEACON':73,
}
assert set(PIVOT_OWNER)==set(p for p,g in PIVOTS)
SPECIAL_PIVOT_POS={
'ANIM_OUTER_YAW_RING':(0,5.06,-1.25),'ANIM_MIDDLE_PITCH_RING':(0,5.06,-1.25),'ANIM_INNER_ROLL_RING':(0,5.06,-1.25),
'ANIM_SPACECRAFT_BODY_ATTITUDE':(0,5.06,-1.25),
'ANIM_WHEEL_DRIVER_X_PANEL':(-11.65,1.55,4.0),'ANIM_WHEEL_DRIVER_Y_PANEL':(-11.2,1.55,4.0),'ANIM_WHEEL_DRIVER_Z_PANEL':(-10.75,1.55,4.0),
'ANIM_REACTION_WHEEL_MOMENTUM_BARS':(-9.95,1.68,4.28),
}
PIVOT_POS={p:SPECIAL_PIVOT_POS.get(p,POS[PIVOT_OWNER[p]]) for p,_ in PIVOTS}
PIVOT_KIND={};PIVOT_AXIS={}
for p,_ in PIVOTS:
 if any(s in p for s in ('HOLOGRAM','DISPLAY','TRACE','VECTOR','BAR','LOCK','SCAN','FAUL','SCADA','CERTIFICATION','TIMELINE','WINDOW','POWER_RESERVE','UNCERTAINTY','RETICLE','ARC','FIELD_LINES','ACQUISITION','LINE_OF_SIGHT')):
  k,ax='indicator','none'
 elif any(s in p for s in ('DOOR','BAFFLE','PANEL','APERTURE','STOP','TACHOMETER','SPINDLE','ROTOR','RING','BODY_ATTITUDE','DOME','ROTATION','TARGET')):
  k,ax='rotation',('x' if 'PITCH' in p or 'ROTOR' in p else 'y')
 else:k,ax='translation','x'
 PIVOT_KIND[p]=k;PIVOT_AXIS[p]=ax
for p in ('ANIM_OUTER_YAW_RING','ANIM_MIDDLE_PITCH_RING','ANIM_INNER_ROLL_RING','ANIM_SPACECRAFT_BODY_ATTITUDE'):
 PIVOT_KIND[p]='rotation';PIVOT_AXIS[p]={'ANIM_OUTER_YAW_RING':'y','ANIM_MIDDLE_PITCH_RING':'x','ANIM_INNER_ROLL_RING':'z','ANIM_SPACECRAFT_BODY_ATTITUDE':'quaternion'}[p]
for p in ('ANIM_WHEEL_X_ROTOR','ANIM_WHEEL_Y_ROTOR','ANIM_WHEEL_Z_ROTOR','ANIM_WHEEL_SKEW_ROTOR'):
 PIVOT_KIND[p]='rotation';PIVOT_AXIS[p]='z'
# Each group will have at least one real nonzero triangle mesh parented under its pivot.
def panel_equipment(a,p,scale=1.0,theme='cyan',special='instrument'):
 x,y,z=p; w=1.02*scale;h=1.17*scale;dep=.68*scale
 a.box((x,y,z),(w,h,dep),'graphite',label='InstrumentCabinet')
 a.box((x,y+.20*scale,z+dep/2+.025*scale),(w*.88,h*.43,.06*scale),'titanium',label='MachinedBezel')
 a.box((x,y+.20*scale,z+dep/2+.064*scale),(w*.79,h*.34,.018*scale),'panel',label='DigitalScreen')
 for row in range(4):
  yy=y+.34*scale-row*.093*scale
  a.box((x-.13*scale,yy,z+dep/2+.080*scale),(.30*scale,.011*scale,.012*scale),theme,label='InstrumentTrace')
  a.box((x+.235*scale,yy,z+dep/2+.083*scale),(.11*scale,.010*scale,.009*scale),('gold' if row%2==0 else 'green'),label='ParameterIndicator')
 for k in range(7):
  dx=(k-3)*.12*scale
  a.box((x+dx,y-.18*scale,z+dep/2+.032*scale),(.065*scale,.038*scale,.08*scale),('gold' if k==2 else 'ceramic'),label='ControlButton')
  a.box((x+dx,y-.31*scale,z+dep/2+.024*scale),(.075*scale,.022*scale,.05*scale),'green' if k%3==0 else 'dark',label='StatusLED')
 for sign in (-1,1):
  xx=x+sign*(w/2-.06*scale)
  for j in range(5):a.box((xx,y-.34*scale+j*.12*scale,z+dep/2+.014*scale),(.045*scale,.042*scale,.022*scale),'structural',label='PanelFastener')
 if special=='optical':
  a.cylinder((x+.30*scale,y+.32*scale,z+dep/2+.15*scale),.12*scale,.15*scale,'glass',axis='z',seg=24,label='OpticalSensorHead')
 if special=='plc':
  for k in range(8):a.box((x+(k-3.5)*.10*scale,y-.40*scale,z+dep/2+.050*scale),(.06*scale,.075*scale,.055*scale),'copper',label='IndustrialIOTerminal')
 if special=='fault':
  a.box((x+.21*scale,y+.30*scale,z+dep/2+.13*scale),(.1*scale,.14*scale,.09*scale),'red',label='GuardedFaultSwitch')

def star_grid(a,seed=81,n=250):
 r=random.Random(seed)
 for k in range(n):
  x=r.uniform(-12.7,12.7);y=r.uniform(2.25,9.5);z=-5.95+r.uniform(-.10,.1)
  if (x*x/13**2+(y-5.3)**2/4.9**2)>1.2:continue
  size=r.choice([.011,.015,.02,.027,.035]); mat='stars' if k%4 else 'white'
  a.box((x,y,z),(size,size,.01),mat,label='StarCatalogPoint')

def make_art(idx):
 nm=NAMES[idx];a=ModelArt(nm);x,y,z=POS[idx]
 if idx==0:
  a.box((0,.11,0),(27,.22,14.45),'dark',label='StageUnderstructure')
  a.box((0,.237,0),(26.6,.025,14.12),'graphite',label='MachinedStageFloor')
  # modular service-bay grid and edge accents
  for ix in range(-12,13):
   for iz in range(-6,7):
    if ix%2==0 and iz%2==0:a.box((ix,.253,iz),(.88,.009,.88),'structural' if (ix+iz)%4==0 else 'dark',label='FloorMaintenancePanel')
  for xx in (-13.35,13.35):
   for j in range(35):a.box((xx,.285,-6.8+j*.395),(.055,.035,.25),'amber' if j%2 else 'dark',label='SafetyFloorStripe')
  for zz in (-6.95,6.95):
   for j in range(57):a.box((-13.1+j*.467,.283,zz),(.26,.024,.052),'cyan' if j%4==0 else 'titanium',label='GroundGridLight')
  for xx in (-12.85,12.85):
   for zz in (-5.0,0.0,5.0):
    a.cylinder((xx,1.12,zz),.065,1.73,'titanium',axis='y',label='SafetyRailingPost')
    a.box((xx,2.0,zz),(.14,.11,.14),'amber',label='SafetyMarker')
  for xx in (-12.85,12.85):a.beam((xx,2.02,-5),(xx,2.02,5),.038,'titanium',label='SafetyHandrail')
 elif idx==1:
  for ra in (4.12,4.58,4.94):
   a.torus((0,5.5,-5.7),ra,.032,'structural',axis='z',seg=96,tube=8,label='CelestialGridArc')
  for j in range(13):
   xx=-12.5+j*2.08
   a.beam((xx,2.35,-5.65),(xx,9.52,-5.65),.024,'blue',label='PanoramaFramework')
  star_grid(a,84,480)
 elif idx==2:
  p=POS[idx]
  for R,rad,pivot,axis,mat in [(4.22,.15,'ANIM_OUTER_YAW_RING','y','structural'),(3.72,.16,'ANIM_MIDDLE_PITCH_RING','x','titanium'),(3.22,.15,'ANIM_INNER_ROLL_RING','z','structural')]:
   a.torus(p,R,rad,mat,axis,seg=112,tube=12,pivot=pivot,label='PrecisionAttitudeGimbalRing')
   a.torus(p,R-.12,.033,'cyan',axis,seg=112,tube=7,pivot=pivot,label='GimbalEncoderLight')
   for k in range(32):
    t=k*math.tau/32
    if axis=='z':q=(p[0]+R*math.cos(t),p[1]+R*math.sin(t),p[2]+.19)
    elif axis=='x':q=(p[0]+.19,p[1]+R*math.cos(t),p[2]+R*math.sin(t))
    else:q=(p[0]+R*math.cos(t),p[1]+.18,p[2]+R*math.sin(t))
    a.cylinder(q,.057,.045,'gold' if k%4==0 else 'graphite',axis=axis,seg=12,pivot=pivot,label='RingPrecisionBolt')
  for sign in (-1,1):
   a.beam((sign*4.50,1.06,-1.25),(sign*4.22,5.05,-1.25),.13,'titanium',label='GimbalSupportArm')
   a.box((sign*4.5,1.2,-1.25),(.95,.65,1.2),'dark',label='GimbalPedestal')
 elif idx==3:
  for yy,rr,hh,mm in [(.5,2.02,.50,'graphite'),(.9,1.8,.24,'titanium'),(1.18,1.55,.22,'dark'),(1.5,1.1,.50,'structural'),(2.0,.75,.58,'titanium')]:
   a.cylinder((0,yy,-1.25),rr,hh,mm,axis='y',seg=56,label='RotationalBearingStructure')
  a.torus((0,1.34,-1.25),1.48,.055,'amber','y',seg=72,label='CautionTurntableRing')
  for j in range(18):
   t=math.tau*j/18;a.cylinder((1.48*math.cos(t),.96,-1.25+1.48*math.sin(t)),.06,.08,'gold',axis='y',label='TurntableBearingBolt')
 elif idx==4:
  px,py,pz=POS[idx]
  a.box((px,py,pz),(2.30,1.70,1.65),'titanium',pivot='ANIM_SPACECRAFT_BODY_ATTITUDE',label='AvionicsSpacecraftMainBus')
  a.box((px,py,pz+1.0),(1.64,1.22,.35),'dark',pivot='ANIM_SPACECRAFT_BODY_ATTITUDE',label='MissionPayloadAccess')
  for sx in (-1,1):
   for sy in (-1,1):
    for sz in (-1,1):a.box((px+sx*.91,py+sy*.63,pz+sz*.66),(.18,.20,.22),'gold',pivot='ANIM_SPACECRAFT_BODY_ATTITUDE',label='SpaceflightMountingNode')
  for j in range(12):
   a.box((px-.94+(j%6)*.37,py+.59-(j//6)*1.1,pz+.86),(.26,.10,.12),'graphite',pivot='ANIM_SPACECRAFT_BODY_ATTITUDE',label='AvionicsThermalPanel')
  for sx in (-1,1):
   a.cylinder((sx*1.28,py,pz),.30,.65,'graphite',axis='x',seg=20,pivot='ANIM_SPACECRAFT_BODY_ATTITUDE',label='SolarArrayAxleBearing')
 elif idx in (5,6,7):
  sz=(.9,.75,.5)[idx-5]
  a.box((x,y,z),(1.60,1.04,sz),'graphite' if idx!=6 else 'pcb',label='AvionicsPackage')
  for j in range(4):
   dx=(j-1.5)*.33
   a.box((x+dx,y+.14,z+sz/2+.085),(.24,.23,.15),'gold' if idx==6 else 'titanium',label='FourChipletModule')
   for k in range(4):a.box((x+dx-.09+k*.06,y+.29,z+sz/2+.11),(.025,.02,.02),'copper',label='MicroInterconnect')
  if idx==7:
   a.cylinder((x,y+.47,z+.26),.29,.52,'titanium',axis='z',seg=24,label='OpticalTransmitter')
   a.torus((x,y+.47,z+.56),.225,.040,'cyan',axis='z',seg=36,label='OpticalBoresightLens')
 elif idx in (8,9):
  sign=-1 if idx==8 else 1
  # Solar wing 4.5m long, 1.2m tall, in front of concentric rings.
  xx=sign*5.22;yy=5.05;zz=.43
  for r in range(2):
   a.box((xx,yy+(r-.5)*.58,zz),(4.50,.55,.095),'graphite',label='SolarPanelFraming')
  for col in range(16):
   for row in range(4):
    px=xx-2.08+col*.277;py=yy-.84+row*.56
    a.box((px,py,zz+.076),(.246,.49,.016),'solar',label='MonocrystallineSolarCell')
    a.box((px,py,zz+.088),(.008,.49,.007),'gold',label='SolarCellBusbar')
    for j in range(3):
     a.box((px,py-.17+j*.17,zz+.095),(.24,.006,.004),'cyan',label='SolarCellFineContact')
  for rr in (-.91,.91):
   a.beam((xx-2.30,yy+rr,zz),(xx+2.30,yy+rr,zz),.064,'titanium',label='WingSupportRail')
  for col in range(9):a.box((xx-2.22+col*.55,yy,zz-.10),(.045,1.85,.09),'titanium',label='WingFrameRib')
  a.cylinder((sign*2.72,yy,-.20),.23,.72,'gold',axis='x',seg=20,label='SolarWingHinge')
 elif idx==10:
  p=POS[idx]
  a.cylinder(p,1.2,.085,'blue','y',seg=64,pivot='ANIM_EARTH_REFERENCE_ROTATION',label='EarthOrbitDisk')
  for R in (.62,.89,1.11):a.torus(p,R,.021,'cyan',axis='z',seg=72,pivot='ANIM_EARTH_REFERENCE_ROTATION',label='EarthReferenceLatitude')
  for ang in range(0,360,30):
   t=ang*D
   a.box((p[0]+math.cos(t)*.82,p[1]+math.sin(t)*.82,p[2]+.06),(.095,.095,.035),'green' if ang%60==0 else 'blue',pivot='ANIM_EARTH_REFERENCE_ROTATION',label='EarthSurfaceHologram')
 elif idx==11:
  for theta in range(8,173,17):
   yline=8.95+1.2*math.cos(theta*D)
   for k in range(28):
    t=math.tau*k/28;xline=4.3*math.sin(theta*D)*math.cos(t);zline=-5.72+.2*math.sin(theta*D)*math.sin(t)
    a.box((xline,yline,zline),(.018,.025,.012),'stars',label='StarCatalogReferenceDot')
 elif idx in (12,13,15,16,32,45,46):
  p=POS[idx];r=.65 if idx not in (15,45) else 1.13
  a.torus(p,r,.025,'cyan' if idx!=32 else 'violet',axis='z',seg=72,label='HolographicReferenceRing')
  a.torus(p,r*.74,.018,'blue',axis='x',seg=72,label='HolographicReferenceCrossRing')
  a.torus(p,r*.52,.015,'gold',axis='y',seg=72,label='HolographicReferenceLatitude')
  if idx==32:
   for k in range(12):
    t=k*math.tau/12;a.beam((x,y,z),(x+r*math.cos(t),y+r*math.sin(t),z),.01,'cyan',pivot='ANIM_MAGNETIC_FIELD_LINES',label='MagneticFieldVector')
 elif idx==14:
  a.box((0,2.3,-5.4),(14.4,.31,.20),'graphite',label='OrbitCalibrationRail')
  for k in range(35):a.box((-7+k*.42,2.50,-5.29),(.1,.06,.03),'cyan' if k%5==0 else 'titanium',label='OrbitalTimelineMark')
 elif idx==17:
  a.box((x,2.57,z),(5.75,.34,1.80),'graphite',label='WheelVaultBase')
  a.box((x,3.7,z-.38),(5.76,1.97,.20),'titanium',label='WheelVaultBackWall')
  for k in range(4):
   xx=-11.55+k*1.37
   a.torus((xx,3.15,.5),.52,.07,'titanium',axis='z',seg=40,label='WheelVaultGuardRing')
 elif idx in (18,19,20,21):
  px,py,pz=POS[idx];pivot=['ANIM_WHEEL_X_ROTOR','ANIM_WHEEL_Y_ROTOR','ANIM_WHEEL_Z_ROTOR','ANIM_WHEEL_SKEW_ROTOR'][idx-18]
  a.box((px,py,pz-.42),(1.23,1.3,.72),'graphite',label='ReactionWheelStaticHousing')
  a.torus((px,py,pz+.08),.56,.075,'titanium',axis='z',seg=54,label='OuterWheelProtectionRing')
  a.cylinder((px,py,pz+.09),.47,.23,'structural',axis='z',seg=48,pivot=pivot,label='ReactionWheelRotorFlywheel')
  a.torus((px,py,pz+.22),.39,.070,'gold',axis='z',seg=48,pivot=pivot,label='MachinedWheelMomentumRim')
  a.cylinder((px,py,pz+.27),.15,.29,'dark',axis='z',seg=32,pivot=pivot,label='WheelShaftBearing')
  a.cylinder((px,py,pz+.43),.085,.06,'cyan',axis='z',seg=24,pivot=pivot,label='WheelSpeedEncoderCenter')
  for k in range(12):
   t=k*math.tau/12;rx=px+.32*math.cos(t);ry=py+.32*math.sin(t)
   a.beam((px,py,pz+.3),(rx,ry,pz+.3),.026,'gold',pivot=pivot,label='ReactionWheelRotorSpoke')
   a.cylinder((px+.48*math.cos(t),py+.48*math.sin(t),pz+.20),.032,.035,'gold',axis='z',seg=10,label='PrecisionWheelHousingBolt')
  a.box((px,py-.71,pz+.25),(.52,.15,.21),'cyan',label='WheelStatusMonitor')
 elif idx in (26,27,28):
  pivot=['ANIM_TORQUE_ROD_X_VECTOR','ANIM_TORQUE_ROD_Y_VECTOR','ANIM_TORQUE_ROD_Z_VECTOR'][idx-26]
  axis=['x','y','z'][idx-26]
  a.cylinder((x,y,z),.22,1.65,'ceramic',axis,seg=24,label='MagneticTorqueCore')
  kcount=27
  for k in range(kcount):
   t=-.73+k*1.46/(kcount-1)
   pp=[x,y,z];pp[{'x':0,'y':1,'z':2}[axis]]+=t
   a.torus(pp,.235,.025,'copper',axis,seg=22,tube=6,label='CopperMagnetorquerWinding')
  a.cylinder((x,y,z),.30,.065,'graphite',axis,seg=20,label='TorqueRodEndSupport')
  a.beam((x,y,z),(x+.6 if idx==26 else x,y+.6 if idx==27 else y,z+.6 if idx==28 else z),.035,'cyan',pivot=pivot,label='MeasuredMagneticDipole')
 elif idx in (37,38,39,41,42,59,60,61,63):
  px,py,pz=POS[idx]
  a.box((px,py,pz-.22),(.92,.67,.79),'titanium',label='OpticalNavigationInstrument')
  a.cylinder((px,py,pz+.36),.31,.71,'graphite',axis='z',seg=28,label='PrecisionOpticalBaffle')
  a.torus((px,py,pz+.76),.235,.065,'gold',axis='z',seg=32,label='OpticalApertureRetention')
  a.cylinder((px,py,pz+.82),.19,.06,'glass',axis='z',seg=28,label='NavigationOpticalLens')
  for k in range(12):
   t=k*math.tau/12;a.cylinder((px+.34*math.cos(t),py+.34*math.sin(t),pz+.3),.032,.045,'gold',axis='z',seg=8,label='LensMountScrew')
  a.box((px,py-.46,pz+.22),(.60,.18,.11),'cyan',label='SensorIdentityStatus')
 elif idx in (34,35,36,40,43,44,47,48,49,50,51,52,53,54,55,56,57,58,62,64,65,66,67,68,69,70,71,72,74,75):
  th='red' if idx in (36,66,74) else ('amber' if idx in (52,53,54,55) else 'cyan')
  panel_equipment(a,POS[idx],scale=.98,theme=th,special=('fault' if idx in (36,66,74) else 'plc' if idx in (69,70,71) else 'instrument'))
  # bespoke technical additions
  if idx in (34,35):
   for j in range(4):a.cylinder((x-.32+j*.21,y+.54,z+.35),.08,.15,'gold',axis='z',seg=14,label='GyroIMUSensorChamber')
  if idx in (49,50,52,53,54,55,56,57,58):
   for k in range(9):a.box((x-.39+k*.098,y-.54,z+.42),(.04,.11,.04),'titanium',label='SignalConditioningHeader')
 elif idx in (22,23,24,25,29,30,31,33):
  panel_equipment(a,POS[idx],scale=1.05,theme='green',special='plc')
  if idx in (30,31):a.torus((x,y+.55,z+.55),.28,.05,'cyan',axis='z',seg=36,label='MagneticVectorInstrument')
 elif idx==73:
  a.cylinder((0,8.35,-5.2),.46,.52,'gold',axis='y',seg=44,pivot='ANIM_ADCS_CERTIFICATION_BEACON',label='ADCSCertificationGoldBeacon')
  a.torus((0,8.64,-5.2),.55,.042,'green',axis='y',seg=64,pivot='ANIM_ADCS_CERTIFICATION_BEACON',label='SixOfSixCertificationRing')
 else:
  # Highly detailed precision lab modules with a real instrument face, cooling and cable ports.
  panel_equipment(a,POS[idx],scale=.96,theme=('blue' if idx%3 else 'amber'))
 # Each named pivot is a mesh-owning actual geometry parent: add a small glowing physical indicator when no other mesh owns it.
 pivot_names=[p for p in PIVOT_OWNER if PIVOT_OWNER[p]==idx]
 used={r[3] for r in a.raw if r[3] is not None}
 for p in pivot_names:
  if p not in used:
   px,py,pz=PIVOT_POS[p];a.box((px,py,pz+.03),(.12,.10,.05),'cyan' if PIVOT_KIND[p]=='indicator' else 'gold',pivot=p,label='PivotBoundGeometricIndicator')
 # Model must have nonempty positive volume/geometry
 assert len(a.raw)>0,(idx,nm)
 return a
# Semantically mapped wire concepts: planned, not constructed SparkTerminal or gameplay objects.
WIRE_GROUPS=[('Educational 5V control power',10),('Reaction wheels, motors and feedback',22),('Magnetic torquers and magnetic sensing',20),('Gyroscopes, star trackers and Sun sensors',22),('Attitude estimation and pointing control',20),('Critical feedback and motion safety',16),('Optical pointing and mission functions',20),('PLC, SCADA and final certification',12)]
WIRES=[]
def add(src,dst,purpose,kind='DIGITAL_CONTROL'):
 WIRES.append(dict(WireID='W%03d'%(len(WIRES)+1),SourceModel=NAMES[src],TargetModel=NAMES[dst],SignalPurpose=purpose,SignalKind=kind))
# 001..010
for dst,purp in [(69,'ADCS PLC logic power'),(47,'Flight controller 5V supply'),(43,'State estimator 5V logic'),(22,'Wheel motor driver control logic'),(29,'Magnetorquer driver control logic'),(34,'Gyroscope signal conditioning'),(37,'Primary star tracker signal conditioning'),(39,'Secondary star tracker logic'),(72,'Holographic SCADA logic power'),(71,'Independent motion safety logic power')]:add(70,dst,purp,'POWER_CONTROL')
# 011..032 (22)
wheel_ids=[18,19,20,21]
for i,n in enumerate(wheel_ids):
 add(47,22,'Wheel %s requested torque command'%('XYZS'[i]),'ACTUATOR_COMMAND')
 add(22,n,'Wheel %s actual motor drive'%('XYZS'[i]),'ACTUATOR_COMMAND')
 add(n,23,'Wheel %s shaft velocity feedback'%('XYZS'[i]),'ANALOG_FEEDBACK')
 add(n,24,'Wheel %s electrical current feedback'%('XYZS'[i]),'ANALOG_FEEDBACK')
add(23,47,'Wheel cluster speed-limit feedback','ANALOG_FEEDBACK')
add(24,47,'Wheel motor-current protection feedback','ANALOG_FEEDBACK')
add(25,47,'Wheel thermal health feedback','DIGITAL_STATUS')
add(47,25,'Wheel thermal regulation request','DIGITAL_CONTROL')
add(66,47,'Wheel saturation warning','SAFETY_CONTROL')
add(67,47,'Wheel desaturation request','DIGITAL_CONTROL')
# 033..052 (20)
for n,ax in [(26,'X'),(27,'Y'),(28,'Z')]:
 add(33,29,ax+' magnetorquer dipole request','ACTUATOR_COMMAND')
 add(29,n,ax+' torque rod drive current','ACTUATOR_COMMAND')
 add(n,33,ax+' actual magnetic dipole response','ANALOG_FEEDBACK')
add(30,33,'Calibrated local magnetic field vector','ANALOG_FEEDBACK')
add(31,30,'Orbital field environment reference','DATA_STREAM')
add(30,43,'Magnetic vector measurement to estimator','ANALOG_FEEDBACK')
add(43,33,'Attitude and field desaturation coordination','DIGITAL_CONTROL')
add(33,67,'Accumulated wheel momentum dump status','DIGITAL_STATUS')
add(67,29,'Protected momentum-unloading authorization','SAFETY_CONTROL')
add(71,29,'Magnetic torquer safety interlock','SAFETY_CONTROL')
add(69,33,'PLC torque mode selection','DIGITAL_CONTROL')
add(29,69,'Magnetorquer amplifier ready feedback','DIGITAL_STATUS')
add(30,69,'Magnetometer measurement health','DIGITAL_STATUS')
add(33,72,'Desaturation state SCADA feedback','DIGITAL_STATUS')
# 053..074 (22)
for s,d,p in [
(34,43,'Raw gyroscope angular-rate measurement'),(35,34,'Gyroscope zero-rate calibration command'),
(36,34,'Gyro temperature bias disturbance input'),(34,43,'Gyro measurement timestamp/validity'),
(37,40,'Primary tracker star-frame pattern'),(40,37,'Catalog match acceptance'),
(37,43,'Primary tracker diagnostic attitude telemetry'),(38,37,'Primary tracker baffle state'),
(39,40,'Secondary diagnostic tracker star pattern'),(39,43,'Secondary tracker diagnostic confidence'),
(41,43,'Coarse Sun-direction measurement'),(42,43,'Earth horizon direction measurement'),
(30,43,'Magnetometer tri-axis field measurement'),(51,43,'Inertial clock synchronization'),
(43,46,'Estimator covariance/uncertainty output'),(43,44,'Complementary fusion status'),
(40,44,'Star catalog validation summary'),(35,69,'Gyroscope calibration report'),
(37,69,'Primary tracker operational health'),(39,69,'Secondary tracker operational health'),
(41,69,'Sun sensor validity'),(42,69,'Horizon sensor validity')]:add(s,d,p,'ANALOG_FEEDBACK' if ('measurement' in p or 'direction' in p or 'timestamp' in p) else 'DIGITAL_STATUS')
# 075..094 (20)
for s,d,p,k in [
(43,47,'Estimated quaternion to flight controller','DATA_STREAM'),
(46,47,'Estimator confidence gate','ANALOG_FEEDBACK'),
(49,47,'Slew trajectory command','DIGITAL_CONTROL'),
(47,48,'Desired spacecraft body torque','ACTUATOR_COMMAND'),
(48,22,'Four-wheel torque allocation','ACTUATOR_COMMAND'),
(47,49,'Requested target quaternion','DIGITAL_CONTROL'),
(49,50,'Predicted slew endpoint','DATA_STREAM'),
(50,47,'Measured pointing-error magnitude','ANALOG_FEEDBACK'),
(57,58,'Measured structural vibration response','ANALOG_FEEDBACK'),
(58,47,'Qualified jitter filter output','ANALOG_FEEDBACK'),
(52,47,'External disturbance torque estimate','ANALOG_FEEDBACK'),
(53,52,'Gravity-gradient torque contribution','DATA_STREAM'),
(54,52,'Orbital drag torque contribution','DATA_STREAM'),
(55,52,'Solar pressure torque contribution','DATA_STREAM'),
(56,57,'Solar wing flexible-mode amplitude','ANALOG_FEEDBACK'),
(51,47,'Attitude controller precise timing reference','DIGITAL_STATUS'),
(45,72,'Quaternion orientation visualization','DIGITAL_STATUS'),
(46,72,'Estimator uncertainty visualization','DIGITAL_STATUS'),
(50,72,'Pointing accuracy telemetry','ANALOG_FEEDBACK'),
(48,72,'Torque-allocation telemetry','DIGITAL_STATUS')]:add(s,d,p,k)
# 095..110 critical; W097 REQUIRED exactly
add(71,65,'Independent attitude motion safety permission','SAFETY_CONTROL')
add(37,68,'Star tracker sample freshness diagnostic','DIGITAL_STATUS')
add(37,43,'Primary star tracker trusted quaternion absolute-attitude feedback','ANALOG_FEEDBACK') # W097
add(68,65,'Critical absolute-attitude feedback qualification','SAFETY_CONTROL')
add(65,47,'Attitude safety mode authorization','SAFETY_CONTROL')
add(65,64,'Sun-safe acquisition request','SAFETY_CONTROL')
add(64,47,'Validated coarse Sun-safe target','DIGITAL_CONTROL')
add(68,69,'Absolute feedback loss critical event','SAFETY_CONTROL')
add(69,71,'ADCS PLC protected motion reset request','SAFETY_CONTROL')
add(71,22,'Independent wheel driver inhibit','SAFETY_CONTROL')
add(71,29,'Independent torquer current inhibit','SAFETY_CONTROL')
add(74,71,'Emergency stop contact state','SAFETY_CONTROL')
add(71,74,'Emergency stop reset readiness','DIGITAL_STATUS')
add(66,65,'Wheel saturation protection threshold','SAFETY_CONTROL')
add(43,68,'Attitude estimator status summary non-authoritative','DIGITAL_STATUS')
add(65,72,'Latched attitude safe-mode SCADA alarm','DIGITAL_STATUS')
# 111..130 (20)
for s,d,p,k in [
(60,49,'Ground-station predicted line of sight','DATA_STREAM'),
(49,47,'Optical target slew request','DIGITAL_CONTROL'),
(47,59,'Optical pointing stabilization enable','DIGITAL_CONTROL'),
(59,61,'Optical line-of-sight measurement','ANALOG_FEEDBACK'),
(63,59,'Mission payload boresight reference','DATA_STREAM'),
(61,62,'Acquired optical signal quality','ANALOG_FEEDBACK'),
(62,47,'Optical lock qualification feedback','ANALOG_FEEDBACK'),
(62,72,'Downlink receive quality telemetry','DIGITAL_STATUS'),
(60,72,'Moving ground station target telemetry','DIGITAL_STATUS'),
(61,7,'Optical transmitter acquisition permit','DIGITAL_CONTROL'),
(7,62,'Optical transmitter actual emission status','DIGITAL_STATUS'),
(6,7,'Mission chiplet outgoing data packets','DATA_STREAM'),
(5,6,'Avionics supply and health status','DIGITAL_STATUS'),
(62,69,'Verified optical downlink packet delivery','DIGITAL_STATUS'),
(69,60,'Two-pass optical schedule authorization','DIGITAL_CONTROL'),
(59,50,'Beam pointing error to analyzer','ANALOG_FEEDBACK'),
(57,62,'Structural jitter optical quality impact','ANALOG_FEEDBACK'),
(64,8,'Sun-safe port array pointing request','DIGITAL_CONTROL'),
(64,9,'Sun-safe starboard array pointing request','DIGITAL_CONTROL'),
(62,75,'Optical communication qualification log','DIGITAL_STATUS')]:add(s,d,p,k)
# 131..142 (12)
for s,d,p,k in [
(69,72,'ADCS PLC mode and sequencing telemetry','DIGITAL_STATUS'),
(72,69,'Operator mode request','DIGITAL_CONTROL'),
(47,69,'Three-axis control stability qualification','DIGITAL_STATUS'),
(43,69,'Estimator readiness and confidence','DIGITAL_STATUS'),
(23,69,'Wheel speed margin certificate','DIGITAL_STATUS'),
(33,69,'Magnetic momentum dumping certificate','DIGITAL_STATUS'),
(65,69,'Attitude-safe mode and alarm status','SAFETY_CONTROL'),
(69,73,'Certification beacon command','DIGITAL_CONTROL'),
(73,72,'Six-objective certificate state','DIGITAL_STATUS'),
(75,72,'Timestamped flight recorder event telemetry','DIGITAL_STATUS'),
(69,75,'Flight event recording request','DIGITAL_CONTROL'),
(71,69,'Independent motion protection status','SAFETY_CONTROL')]:add(s,d,p,k)
assert len(WIRES)==142,len(WIRES)
assert WIRES[96]['SourceModel']==NAMES[37] and WIRES[96]['TargetModel']==NAMES[43]
# allocate source/target socket IDs with a conservative shared-signal policy.
# This is not a topology short: generated CSV is a conceptual plan, not a functional SparkTerminal graph.
NODE_PORT={};PORT_SEQUENCE=[]
def port_key(model,role,kind,wire_num):
 # power bus and status output fanout can share typed junctions. Other connectors get dedicated sockets.
 common=kind in ('POWER_CONTROL','DIGITAL_STATUS','DATA_STREAM') and role=='source'
 return (model,role,kind,'bus' if common else wire_num)
for w in WIRES:
 wid=int(w['WireID'][1:]);kind=w['SignalKind']
 for role,field in [('source','SourceVisualAnchor'),('target','TargetVisualAnchor')]:
  model=w['SourceModel'] if role=='source' else w['TargetModel']
  key=port_key(model,role,kind,wid)
  if key not in NODE_PORT:
   NODE_PORT[key]='P%03d'%(len(NODE_PORT)+1);PORT_SEQUENCE.append((NODE_PORT[key],model,role,kind))
  w[field]=NODE_PORT[key]
# If more than 218 anchors, share further *typed* ports on device, but explicitly flag reused ports for engineer reconciliation.
if len(PORT_SEQUENCE)>218:
 priority=defaultdict(list)
 for an,mod,role,kind in PORT_SEQUENCE:priority[(mod,role,kind)].append(an)
 merge={}
 for _,arr in priority.items():
  for k,an in enumerate(arr):
   if k>=2:merge[an]=arr[k%2]
 for w in WIRES:
  for f in ('SourceVisualAnchor','TargetVisualAnchor'):w[f]=merge.get(w[f],w[f])
 unique=sorted({w[f] for w in WIRES for f in ('SourceVisualAnchor','TargetVisualAnchor')})
 # assign compact P ids and append filler sockets if fewer than 218
 ren={v:'P%03d'%(i+1) for i,v in enumerate(unique)}
 for w in WIRES:
  for f in ('SourceVisualAnchor','TargetVisualAnchor'):w[f]=ren[w[f]]
 first={}
 for w in WIRES:
  first.setdefault(w['SourceVisualAnchor'],w['SourceModel']);first.setdefault(w['TargetVisualAnchor'],w['TargetModel'])
 PORT_SEQUENCE=[(id,first[id],'typed device port','from wiring') for id in sorted(first)]
# Additional primary sockets distributed across devices, explicitly unused for the conceptual wire plan.
USED_SET={p for w in WIRES for p in (w['SourceVisualAnchor'],w['TargetVisualAnchor'])}
while len(PORT_SEQUENCE)<218:
 pid='P%03d'%(len(PORT_SEQUENCE)+1);model=NAMES[(len(PORT_SEQUENCE)*13)%76]
 PORT_SEQUENCE.append((pid,model,'spare visual connector','unassigned'))
assert len(PORT_SEQUENCE)==218,len(PORT_SEQUENCE)
assert len(USED_SET)<=218
# Every concept wire source/dest model has existing visual anchor.
SOCKETS_BY_MODEL=defaultdict(list);SOCKET_META=[]
for aid,model,role,kind in PORT_SEQUENCE:
 idx=NAMES.index(model);n=sum(1 for p,*_ in SOCKETS_BY_MODEL[model]);x,y,z=POS[idx]
 # Dedicated rear and front cable panels oriented toward the viewer.
 sx=(n%5-2)*.115; sy=((n//5)%5-2)*.105
 if idx==0: z0=5.5
 elif idx in (1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16):z0=z+(.59 if idx in (4,5,6,7) else .20)
 elif idx in (18,19,20,21):z0=z+.7
 else:z0=z+.39
 pp=(x+sx,y+sy,z0)
 SOCKETS_BY_MODEL[model].append((aid,pp,role,kind))
 SOCKET_META.append(dict(AnchorID=aid,Model=model,WorldX_m=round(pp[0],5),WorldY_m=round(pp[1],5),WorldZ_m=round(pp[2],5),Purpose='Gold-rimmed visual wire socket; '+role+' '+kind,Status='ART_ANCHOR_NOT_SPARKTERMINAL'))
# Complete per-wire exact logical names
for w in WIRES:
 raw=w['SignalPurpose'].upper(); ident=''.join((ch if ch.isalnum() else '_') for ch in raw)
 ident='_'.join(x for x in ident.split('_') if x)[:58]
 w['SourceTerminalProposed']=('STAR_TRACKER_QUATERNION_OUT' if w['WireID']=='W097' else ident+'_OUT')
 w['TargetTerminalProposed']=('ADCS_ABSOLUTE_ATTITUDE_IN' if w['WireID']=='W097' else ident+'_IN')
 w['Critical']='YES' if w['WireID']=='W097' else 'NO'
 w['Status']='CONCEPT_ONLY_NOT_YET_PLAYER_WIRE'
# a shared socket used for incompatible signal types must later become separate SparkTerminals.
conflict=defaultdict(set)
for w in WIRES:
 for field in ('SourceVisualAnchor','TargetVisualAnchor'):conflict[w[field]].add(w['SignalKind'])
MIXED_ANCHORS=sorted([k for k,v in conflict.items() if len(v)>1]);
# Append art sockets to models, giving explicit SOCKET_P### mesh-owning FBX pivot parents.
_ORIG_MAKE_ART=make_art
def make_art(idx):
 art=_ORIG_MAKE_ART(idx)
 for aid,pp,role,kind in SOCKETS_BY_MODEL.get(NAMES[idx],[]):
  art.socket(pp,aid)
 return art
# Geometry grouping: preserve real named pivot-parent nodes and stable socket nodes.
def chunk_model(a,max_shapes=14):
 by=defaultdict(list)
 for verts,faces,mat,pivot,label in a.raw:
  by[(pivot,mat)].append((verts,faces,label))
 result=[]
 for (pivot,mat),shapes in by.items():
  for n in range(0,len(shapes),max_shapes):
   group=shapes[n:n+max_shapes]; nv=[];nf=[];offset=0
   pp=np.asarray(PIVOT_POS[pivot],float) if pivot in PIVOT_POS else None
   if pivot and pivot.startswith('SOCKET_'):
    pp=np.asarray({a_id:coord for a_id,coord in a.sockets}[pivot[7:]],float)
   for v,f,_ in group:
    v=v-pp if pp is not None else v
    nv.append(v);nf.append(f+offset);offset+=len(v)
   vv=np.vstack(nv);ff=np.vstack(nf)
   label=group[0][2].replace(' ','_')
   result.append(dict(pivot=pivot,material=mat,vertices=vv,faces=ff,label=label,part_index=len(result)))
 return result

# Standard FBX 7.4 binary node serialization; custom original meshes, not copied geometry.
class FN:
 def __init__(self,name,props=None,children=None):self.name=name;self.props=props or [];self.children=children or []
def ps(s):
 b=str(s).encode('utf-8');return b'S'+struct.pack('<I',len(b))+b
def pl(v):return b'L'+struct.pack('<q',int(v))
def pd(v):return b'D'+struct.pack('<d',float(v))
def pbool(v):return b'C'+bytes([1 if v else 0])
def parr(vals,typ='d'):
 if typ=='d':
  bb=np.asarray(vals,dtype='<f8').tobytes();code=b'd';count=len(vals)
 elif typ=='i':
  bb=np.asarray(vals,dtype='<i4').tobytes();code=b'i';count=len(vals)
 else:raise ValueError(typ)
 compressed=zlib.compress(bb,6)
 return code+struct.pack('<III',count,1,len(compressed))+compressed

def p70(name,tp,sub,flags,vals):
 return FN('P',[ps(name),ps(tp),ps(sub),ps(flags)]+vals)
def model_prop(xyz=(0,0,0)):
 return FN('Properties70',children=[
  p70('Lcl Translation','Lcl Translation','','A',[pd(xyz[0]*100),pd(xyz[1]*100),pd(xyz[2]*100)]),
  p70('Lcl Rotation','Lcl Rotation','','A',[pd(0),pd(0),pd(0)]),
  p70('Lcl Scaling','Lcl Scaling','','A',[pd(1),pd(1),pd(1)])
 ])
def fbx_model(mid,name,kind='Null',xyz=(0,0,0)):
 return FN('Model',[pl(mid),ps(name+'\x00\x01Model'),ps(kind)],children=[FN('Version',[pl(232)]),model_prop(xyz),FN('Shading',[ps('Y')]),FN('Culling',[ps('CullingOff')])])
def fbx_geometry(gid,name,vertices,faces):
 # glTF meshes are in metres. FBX numeric vertex coordinates are centimetres.
 v=(vertices*100.0).reshape(-1)
 ix=np.asarray(faces,dtype=np.int32).copy();ix[:,2]=-ix[:,2]-1;fi=ix.reshape(-1)
 me=FN('LayerElementMaterial',[pl(0)],[FN('Version',[pl(101)]),FN('Name',[ps('')]),FN('MappingInformationType',[ps('AllSame')]),FN('ReferenceInformationType',[ps('IndexToDirect')]),FN('Materials',[parr([0],'i')])])
 layer=FN('Layer',[pl(0)],[FN('Version',[pl(100)]),FN('LayerElement',[ ],[FN('Type',[ps('LayerElementMaterial')]),FN('TypedIndex',[pl(0)])])])
 return FN('Geometry',[pl(gid),ps(name+'\x00\x01Geometry'),ps('Mesh')],[FN('GeometryVersion',[pl(124)]),FN('Vertices',[parr(v,'d')]),FN('PolygonVertexIndex',[parr(fi,'i')]),me,layer])
def fbx_material(mid,name,col):
 r,g,b,_=col
 return FN('Material',[pl(mid),ps('PS_'+name+'\x00\x01Material'),ps('')],[FN('Version',[pl(102)]),FN('ShadingModel',[ps('phong')]),FN('MultiLayer',[pl(0)]),
 FN('Properties70',children=[p70('DiffuseColor','Color','','A',[pd(r),pd(g),pd(b)]),p70('DiffuseFactor','Number','','A',[pd(1.0)]),p70('SpecularColor','Color','','A',[pd(.33),pd(.33),pd(.33)]),p70('Shininess','Number','','A',[pd(32)]),p70('TransparencyFactor','Number','','A',[pd(1-col[3])])])])
def write_node(f,n):
 start=f.tell();name=n.name.encode('utf-8');pb=b''.join(n.props)
 f.write(b'\0'*13);f.write(name);f.write(pb)
 for c in n.children:write_node(f,c)
 if n.children:f.write(b'\0'*13)
 end=f.tell();f.seek(start);f.write(struct.pack('<IIIB',end,len(n.props),len(pb),len(name)));f.seek(end)
def export_fbx(path,chunks,roots,pivot_homes,sockets):
 # (model name, list of chunks, unique named socket nodes): one entire scene (individual or showcase).
 header=FN('FBXHeaderExtension',children=[FN('FBXHeaderVersion',[pl(1003)]),FN('FBXVersion',[pl(7400)]),FN('Creator',[ps('Project Spark Level84 Original AAA-inspired Procedural Modular Asset Builder')])])
 settings=FN('GlobalSettings',children=[FN('Version',[pl(1000)]),FN('Properties70',children=[
  p70('UpAxis','int','Integer','',[pl(1)]),p70('UpAxisSign','int','Integer','',[pl(1)]),
  p70('FrontAxis','int','Integer','',[pl(2)]),p70('FrontAxisSign','int','Integer','',[pl(1)]),
  p70('CoordAxis','int','Integer','',[pl(0)]),p70('CoordAxisSign','int','Integer','',[pl(1)]),
  p70('UnitScaleFactor','double','Number','',[pd(1)]),p70('OriginalUnitScaleFactor','double','Number','',[pd(1)])])])
 objs=[];cons=[]; nextid=840000
 def uid():
  nonlocal nextid;nextid+=1;return nextid
 rootids={}
 for rt in roots:
  rootids[rt]=uid();objs.append(fbx_model(rootids[rt],rt,'Null'));cons.append(FN('C',[ps('OO'),pl(rootids[rt]),pl(0)]))
 node_ids={}
 for group,rt,xyz in pivot_homes:
  id=uid();node_ids[(rt,group)]=id;objs.append(fbx_model(id,group,'Null',xyz));cons.append(FN('C',[ps('OO'),pl(id),pl(rootids[rt])]))
 for root,part in chunks:
  pind=part['pivot'];par=node_ids[(root,pind)] if pind else rootids[root]
  px=uid();gx=uid();mx=uid();pn='L84_'+root.split('_',2)[-1][:36]+'_Part_%04d'%(part['part_index'])
  objs.append(fbx_model(px,pn,'Mesh'))
  objs.append(fbx_geometry(gx,pn+'_Geometry',part['vertices'],part['faces']))
  objs.append(fbx_material(mx,part['material'],COLORS[part['material']]))
  cons.extend([FN('C',[ps('OO'),pl(px),pl(par)]),FN('C',[ps('OO'),pl(gx),pl(px)]),FN('C',[ps('OO'),pl(mx),pl(px)])])
 defs=FN('Definitions',children=[FN('Version',[pl(100)]),FN('Count',[pl(len(objs))])])
 with open(path,'wb') as f:
  f.write(b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400))
  for node in [header,settings,defs,FN('Objects',children=objs),FN('Connections',children=cons)]:write_node(f,node)
  f.write(b'\0'*13);f.write(b'\0'*160)
 return {'objects':len(objs),'connections':len(cons),'file_size':os.stat(path).st_size}

def pbrmat(name):
 c=COLORS[name];metal=.8 if name in ('titanium','structural','gold','copper') else .15 if name in ('dark','graphite') else 0.0
 rough=.24 if name in ('titanium','gold','glass') else .4 if name in ('structural','copper','glass') else .68
 alpha='BLEND' if c[3]<1 else 'OPAQUE'
 em=[0,0,0]
 if name in ('cyan','green','amber','violet','blue','red','orange','stars'):
  em=[round(0.4*c[k],3) for k in range(3)]
 return trimesh.visual.material.PBRMaterial(name='PS_'+name,baseColorFactor=[int(255*v) for v in c],metallicFactor=metal,roughnessFactor=rough,emissiveFactor=em,alphaMode=alpha)
MATERIALS={k:pbrmat(k) for k in COLORS}
def export_glb(path,chunks,roots,pivot_homes):
 scene=trimesh.Scene(base_frame='world')
 I=np.eye(4)
 for rt in roots:scene.graph.update(frame_to=rt,frame_from='world',matrix=I)
 for p,rt,xyz in pivot_homes:
  T=np.eye(4);T[:3,3]=xyz
  scene.graph.update(frame_to=p,frame_from=rt,matrix=T)
 for rt,part in chunks:
  pn='L84_'+rt.split('_',2)[-1][:25]+'_Part_%04d'%part['part_index']
  m=trimesh.Trimesh(vertices=part['vertices'],faces=part['faces'],process=False,validate=False)
  m.visual=trimesh.visual.TextureVisuals(material=MATERIALS[part['material']])
  pr=part['pivot'] if part['pivot'] else rt
  scene.add_geometry(m,node_name=pn,geom_name=pn,parent_node_name=pr)
 data=scene.export(file_type='glb',include_normals=True)
 with open(path,'wb') as f:f.write(data)
 return {'glb_bytes':len(data),'geometry_count':len(scene.geometry),'scene_node_count':len(scene.graph.nodes)}

# Build socket nodes and animation groups into each exported model and showcase.
def homes_for_model(idx,a):
 result=[]
 for p in PIVOT_OWNER:
  if PIVOT_OWNER[p]==idx:result.append((p,a.name,PIVOT_POS[p]))
 for aid,pp in a.sockets:result.append(('SOCKET_'+aid,a.name,pp))
 return result
def write_csv(path,rows,fields=None):
 if not rows:return
 keys=fields or list(rows[0])
 with open(path,'w',newline='',encoding='utf8') as f:
  writer=csv.DictWriter(f,fieldnames=keys,extrasaction='ignore');writer.writeheader();writer.writerows(rows)
def produce_exports():
 model_manifest=[];scene_parts=[];all_homes=[];bounds_min=np.full(3,np.inf);bounds_max=np.full(3,-np.inf)
 for idx,name in enumerate(NAMES):
  start=time.time();a=make_art(idx);parts=chunk_model(a);homes=homes_for_model(idx,a)
  rel='Models/%02d_%s.fbx'%(idx,name);gre='GLB/%02d_%s.glb'%(idx,name)
  fbx=export_fbx(os.path.join(ROOT,rel),[(name,q) for q in parts],[name],homes,a.sockets)
  glb=export_glb(os.path.join(ROOT,gre),[(name,q) for q in parts],[name],homes)
  for q in parts:
   pp=np.array(PIVOT_POS[q['pivot']],float) if q['pivot'] in PIVOT_POS else np.array(dict(a.sockets)[q['pivot'][7:]],float) if q['pivot'] and q['pivot'].startswith('SOCKET_') else np.zeros(3)
   world=q['vertices']+pp;bounds_min=np.minimum(bounds_min,world.min(axis=0));bounds_max=np.maximum(bounds_max,world.max(axis=0))
  model_manifest.append(dict(index=idx,name=name,file='%02d_%s.fbx'%(idx,name),glb='%02d_%s.glb'%(idx,name),triangle_count=int(sum(len(q['faces']) for q in parts)),mesh_parts=len(parts),source_primitive_count=len(a.raw),visual_terminal_sockets=len(a.sockets),animation_pivots={p:[round(v,5) for v in xyz] for p,rt,xyz in homes if p.startswith('ANIM_')},fbx_bytes=fbx['file_size'],glb_bytes=glb['glb_bytes']))
  scene_parts.extend((name,q) for q in parts);all_homes.extend(homes)
  if idx%12==0 or idx==75:print('EXPORT',idx+1,'/76  last',name,'tri',model_manifest[-1]['triangle_count'],'parts',len(parts),'sec',round(time.time()-start,2),flush=True)
 r1='Models/Level84_Complete_ArtKit_Showcase.fbx';r2='GLB/Level84_Complete_ArtKit_Showcase.glb'
 print('EXPORT ASSEMBLED',len(scene_parts),'geometry parts',sum(m['triangle_count'] for m in model_manifest),'triangles',flush=True)
 af=export_fbx(os.path.join(ROOT,r1),scene_parts,NAMES,all_homes,SOCKET_META)
 ag=export_glb(os.path.join(ROOT,r2),scene_parts,NAMES,all_homes)
 man=dict(level_id='level_84',display_name='Defy the Tumble',subtitle='Three-Axis Spacecraft Attitude and Precision Optical Pointing',
  individual_models=76,individual_glbs=76,assembled_fbx=r1,assembled_glb=r2,
  individual_triangle_count=sum(m['triangle_count'] for m in model_manifest),individual_mesh_parts=sum(m['mesh_parts'] for m in model_manifest),
  animation_parent_pivot_groups=58,planned_player_wires=142,planned_unique_functional_terminals=218,
  modeled_art_only_terminal_sockets=218,
  workstation_world_bounds_m=[[round(x,4) for x in bounds_min.tolist()],[round(x,4) for x in bounds_max.tolist()]],
  workstation_width_m_nominal=27.0,fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',
  axes='Y up / X width / Z front',assembly_origin=[0,0,0],
  actual_player_wires_included=False,functional_spark_terminals_included=False,adcs_rigidbody_solver_included=False,
  six_authoritative_objectives_implemented=False,
  fbx_import_notes='FBX vertices and pivot positions are numerically in centimetres; Unity imported workstation must be ~27 metres wide.',
  glb_import_notes='GLB geometry and pivots in metres; do not duplicate it with FBX.',
  assembled_fbx_bytes=af['file_size'],assembled_glb_bytes=ag['glb_bytes'],
  models=model_manifest)
 with open(os.path.join(ROOT,'Model_Manifest.json'),'w') as f:json.dump(man,f,indent=2)
 return man

def produce_data():
 write_csv(os.path.join(ROOT,'Animation_Rigs.csv'),[
  dict(Pivot=p,Model=NAMES[PIVOT_OWNER[p]],Kind=PIVOT_KIND[p],Axis=PIVOT_AXIS[p],X_m=PIVOT_POS[p][0],Y_m=PIVOT_POS[p][1],Z_m=PIVOT_POS[p][2],Group=g,Status='REAL_MESH_OWNING_PARENT_IN_FBX_AND_GLB') for p,g in PIVOTS])
 limits=[];start=1
 for group,n in WIRE_GROUPS:
  limits.append((start,start+n-1,group));start+=n
 for w in WIRES:
  i=int(w['WireID'][1:]);w['Group']=next(g for aa,bb,g in limits if aa<=i<=bb)
 write_csv(os.path.join(ROOT,'Gameplay_Wiring_Concept_W001_W142.csv'),WIRES,
 ['WireID','Group','SourceModel','SourceVisualAnchor','SourceTerminalProposed','TargetModel','TargetVisualAnchor','TargetTerminalProposed','SignalPurpose','SignalKind','Critical','Status'])
 write_csv(os.path.join(ROOT,'Visual_Terminal_Sockets_218.csv'),SOCKET_META)
 write_csv(os.path.join(ROOT,'Terminal_Anchor_Plan_218.csv'),SOCKET_META)
 budget=[dict(Group=g,WireRange='W%03d-W%03d'%(aa,bb),Count=bb-aa+1) for aa,bb,g in limits]
 write_csv(os.path.join(ROOT,'Gameplay_Wiring_Budget.csv'),budget)
 roles=[dict(Index=i,Name=n,LocationXYZ=','.join(str(round(v,3)) for v in POS[i]),Role=('ADCS attitude-control / flight instrument' if i>=17 else 'Orbital arena / spacecraft hardware'),AssetStatus='MODULAR_MESH_NOT_UNITY_GAMEPLAY') for i,n in enumerate(NAMES)]
 write_csv(os.path.join(ROOT,'Component_Roles.csv'),roles)
 write_csv(os.path.join(ROOT,'Art_Assembly.csv'),[dict(Index=i,Model=n,LocalPosition='0,0,0',LocalRotation='0,0,0',LocalScale='1,1,1',PrefabRoot='L84_HD_VISUAL_ART',Placement='COMMON_ORIGIN_MODULAR_FBX') for i,n in enumerate(NAMES)])
 conflicts=[dict(AnchorID=k,SignalKinds='/'.join(sorted(conflict[k])),Finding='DIFFERENT_SIGNAL_KINDS_REFER_TO_SAME_ART_SOCKET; CREATE SEPARATE_FUNCTIONAL_TERMINAL_PORTS') for k in MIXED_ANCHORS]
 write_csv(os.path.join(ROOT,'Terminal_Compatibility_Reconciliation.csv'),conflicts,['AnchorID','SignalKinds','Finding'])
 objectives=[
 (1,'Build the Attitude Control Network','142 actual player wires, 218 functional ports, valid power/sensors/actuators/safety and trusted W097 feedback','REAL_GRAPH_AND_DEVICE_COMMISSIONING'),
 (2,'Stop the Tumble and Find the Stars','Calibrated gyro/magnetic coarse estimate, actual angular rate under threshold, valid primary star-tracker fix','DYNAMICS_AND_SENSOR_EVIDENCE'),
 (3,'Achieve Precision Three-Axis Pointing','Four wheel models allocated; controlled target slew, fine pointing, estimator confidence, momentum and jitter margin','REAL_ATTITUDE_AND_ACTUATOR_EVIDENCE'),
 (4,'Hold the Optical Beam','Two real scheduled ground-station passes, actual beam lock, valid power, tracking/jitter margin and qualified desaturation','TWO_PASS_QUALIFICATION'),
 (5,'Survive Attitude-Control Disturbances','Actual gyro bias, wheel saturation and flexible solar-wing jitter faults repaired followed by fresh two-pass qualification','THREE_RECOVERIES_AND_FRESH_MISSION'),
 (6,'Restore Critical Star-Tracker Feedback','Deliberate real W097 removal, stale quaternion fault, safe coarse-mode, physical reconnect, fresh stars/calibration, explicit reset and new two-pass mission','LATCHED_FAULT_AND_FRESH_POST_REPAIR_QUALIFICATION')]
 write_csv(os.path.join(ROOT,'Objective_Certification_Plan_6.csv'),[dict(ObjectiveNumber=n,Name=title,RequiredEvidence=ev,Authority=au,Status='DESIGN_ONLY_NOT_IMPLEMENTED') for n,title,ev,au in objectives])
 w=WIRES[96]
 doc=f'''# LEVEL 84 — W097 CRITICAL STAR-TRACKER FEEDBACK RECOVERY\n\n## Exact proposed connection\n- Wire: **W097** (planned player-created connection, not an existing `SparkTerminal` wire).\n- Source model: `{w['SourceModel']}`.\n- Source proposed terminal: `{w['SourceTerminalProposed']}`.\n- Source art socket: `{w['SourceVisualAnchor']}`.\n- Destination model: `{w['TargetModel']}`.\n- Destination proposed terminal: `{w['TargetTerminalProposed']}`.\n- Destination art socket: `{w['TargetVisualAnchor']}`.\n- Signal: timestamped absolute-attitude quaternion, calibration, star-match confidence, validity and heartbeat.\n\n## Safety and trust\nThe star-tracker signal is continuously refreshed, not a change-only event. On actual electrical disconnection, stop fresh trusted attitude updates; age/expire cached samples; propagate estimator covariance; withdraw fine optical-pointing authorization; preserve physical angular momentum and attitude evolution. Coarse gyro/Sun-sensor protection may maintain bounded safe pointing; the independent safety station still enforces wheel and actuator limits. Unqualified secondary diagnostic tracker or SCADA telemetry must never bypass W097.\n\n## Recovery\nThe player physically reconnects W097, verifies the graph, obtains fresh star observations and catalog matches, checks gyroscope agreement, recalibrates tracker-to-body alignment and estimator covariance, explicitly resets the safety latch, re-establishes fine control and **completes a fresh two-pass optical pointing mission**. Keep Objectives 1–5 historically certified; Objective 6 requires new qualification evidence, not reconnection alone.\n\n## Integration status\nThis package contains *art and conceptual wiring only*. Unity project circuit solver, attitude dynamics, sensors, safe modes, six objectives, and real electrical terminals must be implemented in the host Unity project.\n'''
 open(os.path.join(ROOT,'W097_Critical_Feedback_Recovery.md'),'w').write(doc)
 open(os.path.join(ROOT,'Gameplay_Integration_Plan.md'),'w').write('''# LEVEL 84 UNITY INTEGRATION PLAN\n\n- Use the 76 numbered FBXs at a single common origin and inspect imported bounds; the assembled FBX or GLB is a standalone reference, **not** additional geometry to combine with all 76 modular assets.\n- Import 12 PBR procedural reference materials (36 PNG textures); original geometry has generated material colors but requires production UV authoring for reliable full texture mapping.\n- Each `ANIM_*` group is a real parent node owning mesh parts. Each `SOCKET_P###` is a mesh-owning visual socket transform, **not** a functional SparkTerminal.\n- Instantiate gameplay equipment separately, connect 142 real user-created wires via real SparkTerminal objects, and reconcile visual sockets with signal-domain rules before creating functional terminals.\n- Use two electrical domains: educational 5 V control graph vs separate modeled spacecraft motor/torquer power.\n- Model a quaternion rigid body, four wheel speeds and torque/momentum conservation, B-field-dependent magnetic torque m x B, gyro bias and uncertainty, star-tracker acquisition, solar-wing flexible modes, and two actual optical ground passes.\n- A ground-test gimbal is an animation visualization, never a source of fictional free flight torque.\n- W097 is the unique trusted primary star-tracker absolute-attitude feedback to the estimator, with timestamp/validity; disconnect must inhibit fine pointing, latch, and require explicit reset and new qualification.\n- The final authoritative six objectives must be certified by the real Project Spark LevelGamePlayManager only. No runtime solver, working player wires, objectives or Unity scene/prefab are included in the art kit.\n- Generated Unity helper scripts are optional Editor visualization tools; test in the user's actual Unity version/render pipeline before claiming compilation.\n''')
 open(os.path.join(ROOT,'AAA_Art_Delivery_Notes.md'),'w').write('''# Level 84 AAA-inspired art delivery notes\n\nProcedurally generated original 3D geometry for an attitude-control arena featuring three gimbal rings, detailed spacecraft/solar wings, four flywheel assemblies, three magnetorquer coil systems, star-tracker cameras, gyroscopes, instrument racks and a celestial backdrop. All files are true mesh exports; they are NOT screenshots or 2D placeholders.\n\nArt remains a polished procedural blockout/tech-art kit, not final artist-sculpted production AAA art. UV layout, baked normal/roughness/occlusion channels, collision meshes, LODs, GPU instancing, C# compilation and Play Mode verification remain future production work.\n''')
 return {'wires':len(WIRES),'sockets':len(SOCKET_META),'pivots':len(PIVOTS),'mixed_signal_socket_reconciliation':len(MIXED_ANCHORS)}

if __name__=='__main__':
 t=time.time();print('BUILDING LEVEL 84 MODULAR BINARY FBX + GLB',flush=True)
 produce_data();manifest=produce_exports()
 print('GEOMETRY DONE',round(time.time()-t,1),'s','triangles',manifest['individual_triangle_count'],'meshes',manifest['individual_mesh_parts'],flush=True)
