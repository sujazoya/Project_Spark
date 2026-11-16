#!/usr/bin/env python3
"""Project Spark L92 - THE SILENT CORE. Original modular binary FBX 7400 and GLB art.
Art and concept data only. No valid SparkTerminal circuit, Unity gameplay or real safety implementation.
Geometry coordinates in metres, Y-up. FBX vertex coordinates centimetres, GLB metres.
"""
from pathlib import Path
from collections import defaultdict,Counter
import csv,math,random,json,time
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path('/mnt/data/ProjectSpark_Level92_HD_AAA')
for sub in ['Models','GLB','Textures','Unity_Optional','Source','Reports']:(ROOT/sub).mkdir(parents=True,exist_ok=True)
NAMES=['L92_'+s.strip() for s in (ROOT/'Source'/'model_names.txt').read_text().splitlines() if s.strip()]
assert len(NAMES)==98,len(NAMES)
random.seed(92092)
core.COLORS={
 'rock':(.12,.135,.165,1),'regolith':(.21,.23,.25,1),'fractured':(.32,.29,.26,1),'carbon':(.06,.08,.10,1),
 'mineral':(.53,.35,.21,1),'amberrock':(.47,.27,.16,1),'titanium':(.73,.80,.86,1),
 'white':(.92,.94,.96,1),'silver':(.54,.63,.71,1),'structural':(.30,.40,.48,1),
 'graphite':(.065,.09,.13,1),'dark':(.04,.06,.08,1),'black':(.016,.025,.035,1),'ceramic':(.87,.89,.83,1),
 'gold':(.93,.70,.30,1),'copper':(.84,.48,.22,1),'blue':(.14,.51,.96,1),'cyan':(.08,.88,1.,1),
 'amber':(1.,.55,.10,1),'red':(.94,.17,.12,1),'green':(.19,.84,.47,1),
 'glass':(.17,.51,.67,.43),'holo':(.11,.74,1,.52),'yellow':(.95,.74,.20,1),
 'floor':(.16,.21,.27,1),'panel':(.018,.055,.095,1),'pcb':(.09,.25,.19,1),
 'purple':(.45,.28,.75,1),'soilblue':(.10,.58,.86,.55),'soilgreen':(.23,.88,.56,.55),
 'soilgold':(.96,.65,.20,.6),'soilred':(.96,.25,.28,.65),'insulation':(.14,.21,.25,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
P={}
def put(i,p):P[i]=tuple(float(v) for v in p)
base={0:(0,-.20,0),1:(0,8,-14.7),2:(0,-.55,-7.1),3:(0,8,-13.0),4:(3.6,6,-8),
 5:(0,1.35,8.1),6:(-10,2,-7),7:(0,-.1,-8),8:(10,0,6.8),9:(-4,6,-8),10:(-9,3,-6),
 11:(0,4,-14),12:(0,-.2,-7.5),13:(8,2,5.5),14:(0,10.0,-14),15:(16,1,2)}
for i,p in base.items():put(i,p)
put(16,(0,3,-7.5));put(17,(0,3.35,-7.5))
for j in range(6):
 theta=math.pi/2+j*math.tau/6
 put(18+j,(1.5*math.cos(theta),2.4,-7.5+1.5*math.sin(theta)))
 put(24+j,(3.08*math.cos(theta),.55,-7.5+3.08*math.sin(theta)))
for i,p in {30:(-11.4,1.5,3),31:(-9.5,1.5,3),32:(0,4.2,-7.5),33:(6.8,1.8,-6.1),
 34:(0,2.9,-7.5),35:(0,2.3,-7.5),36:(0,.6,-7.5),37:(0,3.8,-7.5),
 38:(0,5,-7.5),39:(0,4.1,-7.5),40:(.65,1.4,-7.5),41:(-11.4,1.55,4.8),
 42:(-9.55,1.55,4.8),43:(-7.7,1.55,4.8),44:(-5.9,1.55,4.8),
 45:(-11.4,1.55,6.6),46:(-9.6,1.55,6.6),47:(0,2.0,-7.5),
 48:(1.3,.48,-7.5),49:(-7.75,1.55,6.6),50:(2.6,4.3,-7.5),
 51:(0,1.2,-7.5),52:(-5.9,1.55,6.6),53:(-4.0,1.55,6.6)}.items():put(i,p)
for i,p in {54:(-5.0,3,-8.6),55:(5.3,3,-8.8),56:(5,4.7,-6.8),57:(-4.8,1.6,-5.2),
 58:(-6.0,.5,-10.8),59:(6.0,.5,-10.8),60:(6.6,1.5,3.4),61:(8.4,1.5,3.4),
 62:(-4.5,1.1,-9),63:(10.2,1.5,3.4),64:(-6.9,4,-6.2),65:(6.7,4,-6.2),
 66:(12.1,1.5,3.4),67:(4.5,6.0,-9.5)}.items():put(i,p)
for i,p in {68:(6.4,1.4,6),69:(8.6,2.0,6),70:(10.8,2.0,6),71:(7.9,1.6,9),
 72:(10.9,1.55,9),73:(12.6,1.55,9),74:(8.6,1.55,10.8),75:(10.3,1.55,10.8),
 76:(5.0,2.3,8.6),77:(-11.4,2.2,-5.3),78:(-12.6,1.55,-3.2),79:(-10.4,2.5,-7.2),
 80:(6.9,1.55,12.5),81:(11.5,2.1,12)}.items():put(i,p)
for i in range(82,98):
 j=i-82;put(i,(-12.0+(j%8)*1.7,1.55,9.4+(j//8)*2.2))
assert len(P)==98

PV=[]
def pv(idx,name,kind='indicator',axis='none'):
 PV.append(dict(Index=idx,Pivot='ANIM_'+name,Model=NAMES[idx],Kind=kind,Axis=axis))
for i,name,kind,ax in [(3,'AURORA_REMOTE_ORBIT','rotation','y'),(4,'GEOLOGICAL_VECTOR_MAP','indicator','none'),
 (5,'MISSION_CONTROL_DISPLAY','indicator','none'),(6,'DEPLOYMENT_GANTRY_HOIST','translation','y'),
 (9,'TOMOGRAPHY_LAYER_DISPLAY','indicator','none'),(10,'SAMPLING_SERVICE_GANTRY','translation','x'),
 (14,'FINAL_MISSION_BEACON','indicator','none')]:pv(i,name,kind,ax)
for i,n,k,ax in [(16,'LANDER_BODY_LEVEL','rotation','y'),(16,'LANDER_DEPLOYMENT_SLIDE','translation','y'),
 (17,'LANDER_AVIONICS_ACCESS','translation','z')]:pv(i,n,k,ax)
for j in range(6):
 l='ABCDEF'[j]
 for name,axis in [('HIP','y'),('KNEE','x'),('FOOT_ROTATOR','x')]:pv(18+j,'LANDER_LEG_'+l+'_'+name,'rotation',axis)
 for name,k,axis in [('MICROSPINE_DEPLOY','translation','y'),('MICROSPINE_LATCH','rotation','z')]:pv(24+j,'ANCHOR_'+l+'_'+name,k,axis)
for i,n,k,a in [(30,'ANCHOR_FORCE_DISTRIBUTION','indicator','none'),(31,'ANCHOR_LOAD_PLC','indicator','none'),
 (32,'LANDER_ATTITUDE_TRIM','rotation','y')]:pv(i,n,k,a)
for i,n,k,a in [
 (34,'DRILL_SPINDLE','rotation','y'),(35,'DRILL_SPINDLE_CLAMP','rotation','y'),(36,'CORE_BIT','rotation','y'),
 (37,'AXIAL_FEED','translation','y'),(38,'DRIVE_GEARBOX_SHAFT','rotation','y'),(39,'PERCUSSIVE_STRIKER','translation','y'),
 (40,'DRILL_THERMAL_PROBE','translation','y'),(41,'MEASURED_TORQUE_NEEDLE','rotation','z'),
 (42,'MEASURED_FEED_FORCE_NEEDLE','rotation','z'),(43,'DRILL_DEPTH_ENCODER','rotation','z'),
 (44,'DRILL_VIBRATION','indicator','none'),(45,'DRIVE_POWER_STATUS','indicator','none'),
 (46,'DRILL_FEED_STATUS','indicator','none'),(47,'GUIDE_SLEEVE','translation','y'),
 (48,'CUTTINGS_GATE','rotation','x'),(49,'JAM_RECOVERY_SLIDER','translation','x'),
 (50,'BIT_INSPECTION_CAMERA','rotation','y'),(51,'CORE_RETENTION_SLEEVE','translation','y'),
 (52,'THERMAL_FAN','rotation','z'),(53,'DRILL_TELEMETRY','indicator','none')]:pv(i,n,k,a)
for i,n,k,a in [(54,'RADAR_A_SCAN','rotation','y'),(55,'RADAR_B_SCAN','rotation','y'),
 (56,'LIDAR_SWEEP','rotation','y'),(57,'POROSITY_PENETRATION','translation','y'),
 (58,'GEOPHONE_A_COUPLING','translation','y'),(59,'GEOPHONE_B_COUPLING','translation','y'),
 (60,'LAYER_FUSION_STATUS','indicator','none'),(61,'GEOLOGICAL_COVARIANCE','indicator','none'),
 (62,'COHESION_LOAD_HEAD','translation','y'),(66,'RADAR_REFERENCE','indicator','none'),
 (67,'3D_STRATIGRAPHY','indicator','none')]:pv(i,n,k,a)
for i,n,k,a in [(68,'SAMPLE_CAPTURE_GRIPPER','translation','y'),(69,'CARTRIDGE_A','translation','z'),
 (70,'CARTRIDGE_B','translation','z'),(71,'CARTRIDGE_CAROUSEL','rotation','y'),
 (72,'SEAL_COMPRESSION','translation','y'),(73,'LEAK_TEST_CHAMBER','translation','z'),
 (74,'CORE_MASS_INDICATOR','indicator','none'),(76,'SAMPLE_TRANSFER_ARM','rotation','y'),
 (77,'TETHER_WINCH_SPOOL','rotation','x'),(81,'SAMPLE_AIRLOCK_HATCH','rotation','y')]:pv(i,n,k,a)
for i,n,k,a in [(82,'SAMPLING_SAFETY_PLC','indicator','none'),(85,'GEOSCIENCE_SCADA','indicator','none'),
 (92,'CRITICAL_ANCHOR_W171_PATCH','indicator','none'),(93,'MISSION_CERTIFIED','indicator','none')]:pv(i,n,k,a)
assert len(PV)==88,len(PV)
PIVOT_POS={p['Pivot']:P[p['Index']] for p in PV}
for j in range(6):
 l='ABCDEF'[j];x,y,z=P[18+j]
 PIVOT_POS['ANIM_LANDER_LEG_'+l+'_KNEE']=(x*1.47,1.5,-7.5+(z+7.5)*1.47)
 PIVOT_POS['ANIM_LANDER_LEG_'+l+'_FOOT_ROTATOR']=(x*1.83,.55,-7.5+(z+7.5)*1.83)
PIVOT_POS['ANIM_DRILL_SPINDLE']=(0,3.7,-7.5)
PIVOT_POS['ANIM_CORE_BIT']=(0,1.3,-7.5)
PIVOT_POS['ANIM_TETHER_WINCH_SPOOL']=P[77]
core.PIVOT_POS=PIVOT_POS
BY_PV=defaultdict(list)
for p in PV:BY_PV[p['Index']].append(p['Pivot'])

def box(a,p,s,m='titanium',pv=None,label='MachinedPart'):a.box(p,s,m,pv,label)
def cyl(a,p,r,h,m='silver',axis='y',pv=None,label='PrecisionCylinder',seg=24):a.cylinder(p,r,h,m,axis,seg,pv,label)
def tor(a,p,R,r,m='titanium',axis='y',pv=None,label='MachinedAnnulus',seg=40,tube=8):a.torus(p,R,r,m,axis,seg,tube,pv,label)
def beam(a,p,q,r=.052,m='structural',pv=None,label='TrussBeam'):a.beam(p,q,r,m,8,pv,label)
def sphere(a,p,r,m='titanium',pv=None,label='Icosphere',subs=1):
 mesh=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(mesh.vertices+np.asarray(p),mesh.faces,m,pv,label)
def rock(a,p,rad=1.,mat='rock',seed=0):
 rng=np.random.default_rng(920+seed);mesh=trimesh.creation.icosphere(subdivisions=2,radius=1.)
 v=mesh.vertices.copy();v*=rng.uniform(.75,1.25,(len(v),1))
 v[:,0]*=rad*rng.uniform(.8,1.7);v[:,1]*=rad*rng.uniform(.35,1.0);v[:,2]*=rad*rng.uniform(.8,1.5)
 v+=np.array(p);a.mesh(v,mesh.faces,mat,None,'FacetedIrregularCarbonaceousBoulder')
def rack(a,p,kind='science',pv=None,scale=1):
 theme='amber' if kind in ('safety','fault','motor') else 'cyan' if kind in ('science','geo') else 'green'
 core.panel_equipment(a,p,scale,theme,'plc' if kind=='safety' else 'fault' if kind=='fault' else 'instrument')
 x,y,z=p
 for j in range(12):
  box(a,(x-.49+(j%6)*.192,y-.63+(j//6)*.12,z+.40),(.09,.05,.09),'gold',None,'GoldPlatedControlSocket')
 for j in range(6):
  box(a,(x-.39+j*.15,y+.02,z+.39),(.08,.028,.07),'green' if j<4 else 'amber',pv,'ControllerStatusLight')
 for j in range(4):
  box(a,(x-.35+j*.23,y-.42,z-.16),(.17,.36,.30),'graphite',None,'RemovableSensorInterfaceModule')
def gauge(a,p,pv=None):
 cyl(a,p,.19,.11,'gold','z',None,'Bezel',32)
 cyl(a,(p[0],p[1],p[2]+.065),.144,.015,'panel','z',None,'DialScreen',32)
 beam(a,(p[0],p[1],p[2]+.075),(p[0]+.09,p[1]+.04,p[2]+.075),.016,'amber',pv,'MeasuredNeedle')
def terrain(a,extentx=23,extentz=13,base=-.2):
 rows,cols=40,58;v=[];faces=[];rng=np.random.default_rng(92)
 for iz in range(rows):
  z=-7.8+((iz/(rows-1))-.5)*extentz
  for ix in range(cols):
   x=((ix/(cols-1))-.5)*extentx
   n=(.31*math.sin(x*.67+z*.43)+.16*math.cos(x*1.53-z*.22)+
      .19*math.sin(x*2.4+z*1.91)+.13*rng.normal())
   # central drilled depression and exposed layered geological fault wall
   centre=.45*math.exp(-((x/3.2)**2+((z+7.8)/2.8)**2))
   y=base+n-centre
   v.append((x,y,z))
 for iz in range(rows-1):
  for ix in range(cols-1):
   k=iz*cols+ix;faces.extend([[k,k+1,k+cols],[k+1,k+cols+1,k+cols]])
 a.mesh(v,faces,'regolith',None,'TriangulatedKhepri7IrregularAsteroidRegolithTerrain')
 for j in range(24):
  x=rng.uniform(-11,11);z=rng.uniform(-13.5,-1.6)
  if abs(x)<3.4 and -11<z<-4.0:continue
  rock(a,(x,base+.3+rng.uniform(-.1,.3),z),rng.uniform(.22,.84),['rock','carbon','fractured','mineral'][j%4],j)
def spacecraft(a,p):
 x,y,z=p;pv='ANIM_AURORA_REMOTE_ORBIT'
 cyl(a,(x,y,z),1.0,5.5,'white','z',pv,'Aurora1AsteroidMissionFuselage',36)
 cyl(a,(x,y,z-2.8),.78,1.2,'graphite','z',pv,'AsteroidOpticalNavigationNose',32)
 for j in range(5):tor(a,(x,y,z-2.3+j*1.1),1.01,.052,'gold','z',pv,'VehicleStructuralRing')
 for s in (-1,1):
  box(a,(x+s*3.0,y,z),(4.2,.07,1.75),'structural',pv,'DeployableSolarPanelSupport')
  box(a,(x+s*3,y+.05,z),(4.02,.025,1.64),'blue',pv,'SolarCells')
  for k in range(12):box(a,(x+s*(.95+k*.33),y+.07,z),(.018,.02,1.58),'gold',pv,'SolarArrayElectricalBus')
 for j in range(4):
  ang=j*math.tau/4
  tor(a,(x+.64*math.cos(ang),y+.64*math.sin(ang),z+2.8),.18,.05,'cyan','z',pv,'ElectricThrusterNozzle',24)
 for k in range(12):
  ang=k*math.tau/12
  beam(a,(x+.87*math.cos(ang),y+.87*math.sin(ang),z-2.1),(x+.87*math.cos(ang),y+.87*math.sin(ang),z+2.1),.025,'silver',pv,'SpacecraftSurfaceUmbilicalGuide')
def landerbody(a):
 pv='ANIM_LANDER_BODY_LEVEL';y=3.05;z=-7.5
 cyl(a,(0,y,z),1.83,.55,'white','y',pv,'TitaniumHexagonalLanderStructuralHub',6)
 cyl(a,(0,y+.34,z),1.53,.29,'graphite','y',pv,'LanderCentralAvionicsStack',6)
 tor(a,(0,y-.18,z),1.68,.11,'gold','y',pv,'SixAxisStructuralLoadRing',48)
 for k in range(6):
  t=k*math.tau/6
  box(a,(1.38*math.cos(t),y+.05,z+1.38*math.sin(t)),(.45,.31,.43),'structural',pv,'SixIndependentLegHipBulkheads')
  cyl(a,(1.46*math.cos(t),y+.1,z+1.46*math.sin(t)),.18,.27,'gold','y',pv,'LoadDistributionPivot')
  box(a,(1.35*math.cos(t),y+.53,z+1.35*math.sin(t)),(.17,.12,.23),'cyan',pv,'LanderTelemetryIllumination')
 for k in range(3):
  box(a,((k-1)*.52,y+.65,z+.1),(.38,.26,.45),'silver',pv,'AutonomousLanderControlAvionics')
 cyl(a,(0,y+1.07,z),.29,.43,'gold','y',pv,'LanderCommunicationsMastBase',24)
 cyl(a,(0,y+1.6,z),.065,.75,'silver','y',pv,'LanderTelemetryAntennaShaft',12)
 sphere(a,(0,y+2.03,z),.24,'gold',pv,'CommunicationsDishRadome')
def leg(a,j):
 t=math.pi/2+j*math.tau/6;l='ABCDEF'[j];pv1=f'ANIM_LANDER_LEG_{l}_HIP';pv2=f'ANIM_LANDER_LEG_{l}_KNEE';pv3=f'ANIM_LANDER_LEG_{l}_FOOT_ROTATOR'
 x1=1.45*math.cos(t);z1=-7.5+1.45*math.sin(t)
 x2=2.22*math.cos(t);z2=-7.5+2.22*math.sin(t)
 x3=3.15*math.cos(t);z3=-7.5+3.15*math.sin(t)
 beam(a,(x1,3.1,z1),(x2,1.68,z2),.19,'silver',pv1,'HydraulicLanderUpperLeg')
 beam(a,(x1+.10,3.0,z1),(x2+.10,1.70,z2),.06,'gold',pv1,'LegActuatorLinkage')
 cyl(a,(x1,3.05,z1),.31,.31,'graphite','y',pv1,'LanderHipServo',32)
 cyl(a,(x2,1.68,z2),.30,.39,'gold','y',pv2,'LanderKneeServoGearbox',32)
 beam(a,(x2,1.7,z2),(x3,.55,z3),.13,'titanium',pv2,'TelescopicPrecisionLowerLeg')
 beam(a,(x2+.1,1.7,z2),(x3+.1,.59,z3),.046,'gold',pv2,'SecondLegFeedbackRod')
 box(a,(x3,.45,z3),(.65,.11,.6),'structural',pv3,'PassiveLandingFootMount')
 for k in range(4):cyl(a,(x3+(-.22+(k%2)*.44),.39,z3+(-.20+(k//2)*.4)),.07,.16,'gold','y',pv3,'ActiveTerrainMicrocontact')
def foot(a,j):
 x,y,z=P[24+j];l='ABCDEF'[j];pv1=f'ANIM_ANCHOR_{l}_MICROSPINE_DEPLOY';pv2=f'ANIM_ANCHOR_{l}_MICROSPINE_LATCH'
 box(a,(x,.35,z),(.95,.18,.82),'graphite',None,'AdaptiveRockMicrospineFootBase')
 for k in range(8):
  t=k*math.tau/8
  beam(a,(x+.15*math.cos(t),.33,z+.15*math.sin(t)),(x+.44*math.cos(t),.05,z+.34*math.sin(t)),.04,'titanium',pv1,'CurvedMicrospineRake')
  cyl(a,(x+.44*math.cos(t),.095,z+.34*math.sin(t)),.045,.15,'gold','y',pv1,'IndividualMicrospineHook',12)
 cyl(a,(x,.47,z),.24,.27,'gold','y',pv2,'AnchorLockingLug')
 tor(a,(x,.42,z),.38,.045,'cyan','y',pv2,'AnchorEngagementOpticalRing')
 for k in range(4):box(a,(x+(.33 if k%2 else -.33),.56,z+(.29 if k//2 else -.29)),(.06,.09,.11),'green',pv2,'AnchorContactSensor')
def drill(a,p,idx):
 x,y,z=p;pp=BY_PV[idx][0] if BY_PV[idx] else None
 if idx==34:
  cyl(a,(0,2.68,-7.5),.53,3.75,'silver','y',pp,'PrecisionCoringRotaryBarrel',48)
  tor(a,(0,4.25,-7.5),.54,.12,'gold','y',pp,'DrillMotorSlipBearing',48)
  for k in range(9):tor(a,(0,1.2+k*.34,-7.5),.53,.032,'titanium','y',pp,'DrillColumnThermalStiffener',40)
  for k in range(25):
   angle=k*math.tau/10
   u=(.38*math.cos(angle),1.05+.115*k,-7.5+.38*math.sin(angle))
   v=(.38*math.cos(angle+math.tau/10),1.05+.115*(k+1),-7.5+.38*math.sin(angle+math.tau/10))
   beam(a,u,v,.065,'gold',pp,'HelicalCuttingsFlight')
 elif idx==35:
  cyl(a,(0,2.25,-7.5),.31,2.8,'graphite','y',pp,'RotaryDriveSpindle')
  for k in range(9):tor(a,(0,1.15+.28*k,-7.5),.29,.028,'copper','y',pp,'PrecisionTorqueTransferRidge')
 elif idx==36:
  cyl(a,(0,.57,-7.5),.42,1.0,'ceramic','y',pp,'CoringCarbideBitBody',40)
  for k in range(20):
   t=k*math.tau/20
   box(a,(.43*math.cos(t),.10,-7.5+.43*math.sin(t)),(.13,.28,.12),'gold',pp,'IndividualTungstenCarbideCuttingTooth')
  cyl(a,(0,.07,-7.5),.22,.11,'black','y',pp,'OpenCoreIntake')
 elif idx==37:
  cyl(a,(0,3.65,-7.5),.16,3.9,'titanium','y',pp,'BallScrewFeedRod',24)
  for k in range(24):tor(a,(0,1.9+k*.16,-7.5),.18,.022,'gold','y',pp,'PrecisionBallScrewThread',24)
  box(a,(0,3.7,-7.5),(.76,.3,.82),'structural',pp,'DrillFeedCarriage')
 elif idx==38:
  cyl(a,(0,5.01,-7.5),.88,.98,'structural','y',pp,'DrillDriveMotorHousing',40)
  for k in range(12):
   t=k*math.tau/12;beam(a,(.89*math.cos(t),4.65,-7.5+.89*math.sin(t)),(.89*math.cos(t),5.30,-7.5+.89*math.sin(t)),.05,'silver',pp,'RadialMotorCoolingFin')
  cyl(a,(0,5.57,-7.5),.40,.18,'copper','y',pp,'DriveMotorShaftCoupler',30)
 elif idx==39:
  cyl(a,(0,4.0,-7.5),.65,.77,'gold','y',pp,'ControlledPercussionHammerHousing',36)
  cyl(a,(0,3.51,-7.5),.31,.71,'titanium','y',pp,'PercussiveImpactPlunger',30)
 elif idx==47:
  for k in range(4):
   t=k*math.tau/4
   beam(a,(.98*math.cos(t),.7,-7.5+.98*math.sin(t)),(.98*math.cos(t),4.2,-7.5+.98*math.sin(t)),.075,'structural',pp,'DrillGuideFrameColumn')
  for h in (.8,1.6,2.4,3.3,4.2):tor(a,(0,h,-7.5),1.02,.047,'gold','y',pp,'FeedFrameAlignmentRing',40)
 elif idx==51:
  cyl(a,(0,1.3,-7.5),.27,1.55,'glass','y',pp,'CoreRetentionHollowSleeve',32)
  for h in (.65,1.2,1.8):tor(a,(0,h,-7.5),.30,.046,'gold','y',pp,'CoreSampleSleeveEndSeal',32)
 else:
  rack(a,p,'motor' if idx in (45,46,49) else 'science',pp)
  if idx in (41,42,43):gauge(a,(x+.10,y+.18,z+.45),pp)
  if idx in (40,50):cyl(a,(x,y+.67,z+.49),.17,.40,'glass','z',pp,'InstrumentOpticalProbe',24)
  if idx in (48,49):box(a,(x+.35,y-.40,z+.55),(.19,.4,.20),'gold',pp,'MechanicalFaultRecoveryActuator')
  if idx==52:
   for j in range(8):
    angle=j*math.tau/8
    beam(a,(x,y,z+.43),(x+.25*math.cos(angle),y+.25*math.sin(angle),z+.43),.05,'silver',pp,'CoolingFanBlade')
def scanner(a,p,idx):
 x,y,z=p;pp=BY_PV[idx][0] if BY_PV[idx] else None
 if idx in (54,55):
  box(a,(x,y-.6,z),(1.7,.45,1.4),'structural',None,'RadarSurfaceSurveyMount')
  cyl(a,(x,y+.18,z),.67,1.24,'titanium','y',pp,'AsteroidGroundPenetratingRadarFeedUnit',40)
  tor(a,(x,y-.40,z),.83,.09,'gold','y',pp,'RadarApertureArrayRing',48)
  for k in range(10):
   t=k*math.tau/10
   cyl(a,(x+.53*math.cos(t),y+.55,z+.53*math.sin(t)),.065,.22,'cyan','y',pp,'RadarAntennaArrayTransducer',12)
 elif idx==56:
  cyl(a,(x,y,z),.35,1.3,'graphite','y',pp,'AsteroidSurfaceLidarTurntable',32)
  for j in range(10):
   t=j*math.tau/10;beam(a,(x,y+.65,z),(x+.8*math.cos(t),y+.65,z+.8*math.sin(t)),.04,'cyan',pp,'LidarScanningBeamDecorative')
 elif idx in (58,59):
  cyl(a,(x,y,z),.42,.35,'graphite','y',pp,'SeismicSurfaceGeophoneHousing')
  for j in range(4):
   t=j*math.tau/4;beam(a,(x+.3*math.cos(t),y-.1,z+.3*math.sin(t)),(x+.5*math.cos(t),y-.65,z+.5*math.sin(t)),.06,'gold',pp,'RockSeismicCouplingPin')
 elif idx==67:
  box(a,(x,y,z),(4.4,3.3,.09),'glass',None,'HolographicSubsurfaceCrossSectionGlass')
  for k in range(5):
   color=['soilblue','soilgreen','soilgold','soilred','carbon'][k]
   x0=x-2.02;zfront=z+.12
   v=[];faces=[]
   for row in range(2):
    for col in range(46):
     xx=x0+col*.088
     yy=y-1.36+k*.55+math.sin(col*.27+k*.4)*.12+math.cos(col*.49)*.06+row*.38
     v.append((xx,yy,zfront+row*.012))
   for col in range(45):faces.extend([[col,col+1,46+col],[col+1,47+col,46+col]])
   a.mesh(v,faces,color,pp,'ActualGeologicalHologramMappedLayerStrip')
  beam(a,(x,y+1.5,z+.20),(x,y-1.4,z+.20),.04,'amber',pp,'ActiveGeologicalDrillPathReticle')
 else:
  rack(a,p,'geo',pp)
  if idx in (57,62):cyl(a,(x+.3,y-.70,z+.38),.12,.55,'gold','y',pp,'GeotechnicalPenetrometerTip')
  if idx in (64,65):
   for sx in (-1,1):cyl(a,(x+sx*.24,y+.36,z+.55),.15,.16,'glass','z',pp,'StereoCameraOpticalElement',26)

def sample(a,p,idx):
 x,y,z=p;pp=BY_PV[idx][0] if BY_PV[idx] else None
 if idx==68:
  box(a,(x,y-.6,z),(2.4,.4,1.5),'structural',None,'CoreAcquisitionStationPlatform')
  for d in (-1,1):
   beam(a,(x+d*.64,y+.6,z),(x+d*.38,y-.08,z+.05),.09,'silver',pp,'SampleCaptureCompliantFinger')
   cyl(a,(x+d*.64,y+.70,z),.20,.26,'gold','z',pp,'SampleGripperMotor')
 elif idx in (69,70):
  cyl(a,(x,y,z),.40,2.25,'glass','z',pp,'SealedPristineCoreCartridgeGlassTube',40)
  for side in (-1,1):cyl(a,(x,y,z+side*1.12),.42,.24,'titanium','z',pp,'VacuumCompatibleCartridgeClosureCap',40)
  for k in range(7):
   rock(a,(x+(k%2)*.06-.03,y-.07,z-.92+k*.28),.14,['regolith','rock','fractured','mineral'][k%4],k+50)
  tor(a,(x,y,z-1.0),.43,.07,'gold','z',pp,'CoreCartridgeSealContact')
 elif idx==71:
  cyl(a,(x,y,z),1.25,.33,'structural','y',pp,'SixCartridgeRotaryIndexer',48)
  for j in range(6):
   t=j*math.tau/6
   cyl(a,(x+.87*math.cos(t),y+.25,z+.87*math.sin(t)),.23,.50,'silver','y',pp,'IndexedSampleReceivingPockets',20)
   tor(a,(x+.87*math.cos(t),y+.51,z+.87*math.sin(t)),.23,.04,'gold','y',pp,'SamplePocketTrimRing',20)
 elif idx==72:
  cyl(a,(x,y,z),.58,1.22,'structural','y',pp,'ContainedCartridgeSealPressBody')
  cyl(a,(x,y+.61,z),.44,.18,'gold','y',pp,'SealCompressionAxialRam')
  for j in range(4):
   t=j*math.tau/4;beam(a,(x+.45*math.cos(t),y-.4,z+.45*math.sin(t)),(x+.45*math.cos(t),y+.62,z+.45*math.sin(t)),.055,'silver',pp,'SealPressTieRod')
 elif idx==76:
  box(a,(x,y-1.1,z),(1.5,.4,1.4),'graphite',None,'SampleTransferManipulatorBase')
  for j in range(3):beam(a,(x-.2+j*.33,y-.9+j*.70,z),(x-.2+j*.33,y-.13+(j+1)*.69,z),.17,'silver',pp,'MultijointSampleTransferArmLink')
  cyl(a,(x+.5,y+1.0,z),.23,.3,'gold','z',pp,'SealedSampleGraspingEndEffector')
 elif idx==77:
  box(a,(x,y-1.3,z),(2.5,.35,1.9),'structural',None,'LanderTetherRetrievalWinchFoundation')
  cyl(a,(x,y,z),.8,1.7,'graphite','x',pp,'CentralCableWinchDrum',48)
  for j in range(36):
   k=j*math.tau/10
   xoff=x-.78+(j%18)*.082
   beam(a,(xoff,y+.86*math.cos(k),z+.86*math.sin(k)),(xoff+.03,y+.86*math.cos(k+0.2),z+.86*math.sin(k+0.2)),.056,'gold',pp,'BraidedTetherCableStoredWinding')
  for xx in (-1.08,1.08):cyl(a,(x+xx,y,z),.94,.10,'titanium','x',None,'PrecisionWinchSideFlange',48)
 elif idx==81:
  box(a,(x,y,z),(2.7,3.2,2.7),'structural',None,'VacuumCompatibleSampleAirlockHousing')
  tor(a,(x,y,z+1.40),1.1,.13,'gold','z',pp,'SampleAirlockPressureSealGasket',48)
  cyl(a,(x,y,z+1.48),1.05,.20,'titanium','z',pp,'MotorizedSampleAirlockDoor',48)
 else:
  rack(a,p,'science',pp,1.05)
  if idx in (73,74):gauge(a,(x+.18,y+.23,z+.47),pp)
  if idx==78:tor(a,(x+.2,y+.25,z+.56),.18,.04,'gold','z',pp,'TetherForceMetrology')

def architecture(a,idx):
 x,y,z=P[idx];pp=BY_PV[idx][0] if BY_PV[idx] else None
 if idx==0:
  box(a,(0,-.30,0),(35,.46,30),'floor',None,'AsteroidMissionIndustrialServiceDeck')
  for j in range(19):box(a,(-17.1+j*1.9,-.06,0),(.028,.025,29.2),'silver',None,'IndustrialTileJoint')
  for j in range(16):box(a,(0,-.05,-14.1+j*1.9),(34.8,.02,.026),'graphite',None,'FloorTileTransverseJoint')
  for s in (-1,1):
   box(a,(s*17.2,-.02,0),(.16,.05,30),'gold',None,'SafetyFloorBoundary')
   for j in range(16):box(a,(s*16.65,.01,-14+j*1.85),(.55,.025,.22),'amber',None,'ServiceDeckHazardLattice')
 elif idx==1:
  box(a,(0,8.0,-14.7),(34,16,.16),'glass',None,'DeepSpacePanoramicWindowPlate')
  for j in range(12):beam(a,(-16.4+j*2.98,.1,-14.62),(-16.4+j*2.98,16,-14.62),.12,'structural',None,'ObservationDomeTitaniumVerticalMullion')
  for yy in (1.6,8.1,15.7):beam(a,(-17,yy,-14.61),(17,yy,-14.61),.16,'titanium',None,'PressureObservationCrossbeam')
 elif idx in (2,7,12):
  if idx==2:terrain(a)
  elif idx==7:
   for j in range(18):
    angle=j*2.4;r=6.+j*.31
    xx=math.cos(angle)*r;zz=-8.+math.sin(angle)*r*.65
    rock(a,(xx,.2,zz),.35+(j%6)*.20,['rock','carbon','fractured','mineral'][j%4],seed=100+j)
  else:
   for j in range(6):
    t=j*math.tau/6
    box(a,(3.3*math.cos(t),-.03,-7.5+3.3*math.sin(t)),(.8,.10,.75),'gold',None,'LanderContactTestPad')
 elif idx==3:spacecraft(a,P[idx])
 elif idx in (4,9):
  box(a,P[idx],(5.1,3.8,.09),'glass',pp,'AsteroidGeophysicalHolographicAnalysisPlane')
  for j in range(11):
   h=(j-5)*.24
   beam(a,(x-2.25,y+h,z+.13),(x+2.25,y+h+math.sin(j*.7)*.18,z+.13),.022,'cyan' if j%3 else 'gold',pp,'GeologyTomographyHolographicLayers')
  for j in range(6):
   xx=x-2.0+j*.8
   beam(a,(xx,y-1.6,z+.2),(xx,y+1.55,z+.2),.013,'green',pp,'TectonicLayerGridVector')
 elif idx in (5,8):
  box(a,(x,y-.45,z),(5.8,.45,2.0),'structural',None,'MissionControlCenterWorkstationFrame')
  for j in range(4):
   core.panel_equipment(a,(x-2.1+j*1.4,y+.43,z-.40),1.1,'cyan','instrument')
  for j in range(20):box(a,(x-2.8+j*.29,y-.17,z+.91),(.17,.05,.19),'amber' if j%5==0 else 'graphite',None,'OperatorConsoleButtons')
 elif idx in (6,10,13):
  for sx in (-1,1):
   beam(a,(x+sx*1.55,.1,z-.9),(x+sx*1.55,5.3,z-.9),.14,'structural',None,'HeavyAsteroidServicingGantryPost')
   beam(a,(x+sx*1.55,.1,z+.9),(x+sx*1.55,5.3,z+.9),.14,'structural',None,'GantrySecondSupportPost')
  for xx in (-1.55,1.55):
   beam(a,(x+xx,5.3,z-.9),(x+xx,5.3,z+.9),.12,'silver',None,'GantryUpperStructuralBridge')
  box(a,(x,5.4,z),(3.35,.22,2.2),'silver',pp,'IndustrialServicingRail')
  box(a,(x,4.7,z),(.84,.8,.94),'gold',pp,'GantryMotorizedRetrievalHead')
 elif idx==11:
  for j in range(20):
   xx=-13.5+j*1.38;rock(a,(xx,4.1,-14.15),1.0+(j%4)*.26,['rock','carbon','regolith'][j%3],j+140)
 elif idx==14:
  cyl(a,(x,y,z),.85,.25,'gold','y',pp,'MissionCertificationBeaconRing',40)
  for j in range(7):tor(a,(x,y-j*.25,z),.45+j*.10,.038,'cyan','y',pp,'GeologicalQualificationHolographicHalo',36)
 elif idx==15:
  for j in range(14):
   xx=-16.1+j*2.45
   beam(a,(xx,.25,12.8),(xx,1.42,12.8),.06,'gold',None,'HazardRailPost')
   if j<13:beam(a,(xx,1.42,12.8),(xx+2.45,1.42,12.8),.06,'gold',None,'HazardRailTop')

def build_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i];pp=BY_PV[i][0] if BY_PV[i] else None
 if i<16:architecture(a,i)
 elif i==16:landerbody(a)
 elif i==17:
  box(a,(0,3.6,-7.5),(2.3,.65,2.1),'graphite',None,'CentralLanderAvionicsSealedRack')
  for j in range(8):box(a,(-.91+j*.26,3.78,-6.4),(.17,.16,.07),'green' if j%3 else 'cyan',pp,'LanderDiagnosticPowerLED')
  for j in range(12):box(a,(-.9+j*.17,3.12,-6.38),(.10,.09,.07),'gold',pp,'LanderElectricalPatchPort')
 elif 18<=i<=23:leg(a,i-18)
 elif 24<=i<=29:foot(a,i-24)
 elif i==32:
  tor(a,(0,4.15,-7.5),.78,.11,'structural','y',pp,'LanderAttitudeControlReactionStructure',48)
  for j in range(3):
   ang=j*math.tau/3;cyl(a,(.51*math.cos(ang),4.20,-7.5+.51*math.sin(ang)),.25,.35,'gold','y',pp,'MicrogravityAttitudeTrimReactionWheel')
 elif 34<=i<=53:drill(a,P[i],i)
 elif 54<=i<=67:scanner(a,P[i],i)
 elif 68<=i<=81:sample(a,P[i],i)
 else:
  rack(a,P[i],'safety' if i in (82,83,86,91,94,95) else 'fault' if i in (87,88,89,90) else 'science',pp,1.10)
  if i==92:
   box(a,(x,y+.55,z+.49),(.98,.17,.12),'red',None,'W171CriticalAnchorFeedbackPatchIdentification')
   for j in range(8):cyl(a,(x-.43+j*.12,y-.52,z+.48),.055,.13,'gold','z',None,'CriticalFeedForceSignalPort',16)
  if i in (87,88,89):
   box(a,(x+.26,y+.22,z+.52),(.18,.33,.12),'red',pp,'GuardedFaultActuationHandle')
  if i==93:
   for j in range(5):tor(a,(x,y+.1,z+.40),.30+j*.05,.014,'cyan','z',pp,'MissionObjectiveCompletionHoloRing',28)
 if i in (30,31,33):
  rack(a,P[i],'safety' if i==31 else 'science',pp,1.04)
  if i==30:
   for j in range(6):
    yy=y-.5+(j%3)*.24;xx=x-.3+(j//3)*.56
    gauge(a,(xx,yy,z+.44),pp)
 return a

# Visual sockets are decorative in the FBX/GLB. One visual P-id can be reused by multiple
# planned logical electrical ports, requiring Unity-side reconciliation.
per=[2]*98
key={5:4,16:5,17:4,30:5,31:4,34:4,35:4,36:4,37:4,38:4,41:4,42:4,45:4,46:4,
     54:4,55:4,60:4,61:4,68:4,71:4,72:4,73:4,77:4,78:4,82:5,83:4,85:5,86:4,90:4,92:4,93:4}
for i,ct in key.items():per[i]=ct
# deterministic distribution until exactly 266
ix=0
while sum(per)<266:
 j=[0,2,4,6,8,9,11,12,13,14,15,18,19,20,21,22,23,24,25,26,27,28,29,40,43,44,47,48,50,51,52,53,56,57,58,59,62,63,64,65,66,67,69,70,74,75,76,79,80,81,84,87,88,89,91,94,95,96,97][ix%59];per[j]+=1;ix+=1
while sum(per)>266:
 j=next(i for i in range(97,-1,-1) if per[i]>2 and i not in (30,82));per[j]-=1
assert sum(per)==266
ANCHORS=[];BY_MODEL=defaultdict(list)
for i,n in enumerate(NAMES):
 x,y,z=P[i];ct=per[i]
 for k in range(ct):
  ident=f'P{len(ANCHORS)+1:03d}'
  if i<16:xx=x+((k%4)-(min(ct,4)-1)/2)*.36;yy=y+(.3+(k//4)*.21);zz=z+.25
  elif i in (16,17) or 18<=i<=29 or 34<=i<=40:xx=x+((k%3)-1)*.22;yy=y+.1+(k//3)*.12;zz=z+.45
  else:xx=x+(k%4-(min(ct,4)-1)/2)*.20;yy=y-.38+(k//4)*.22;zz=z+.48
  rec=dict(AnchorID=ident,OwnerModel=n,WorldX_m=round(xx,5),WorldY_m=round(yy,5),WorldZ_m=round(zz,5),
           Status='ART_ANCHOR_NOT_SPARKTERMINAL')
  ANCHORS.append(rec);BY_MODEL[i].append(rec)
assert len(ANCHORS)==266
WIRE_GROUPS=[(1,12,'5 V educational power and returns',12),(13,40,'Geophysical survey and terrain sensors',28),
 (41,66,'Landing legs and anchoring',26),(67,92,'Drill drives feed and torque',26),
 (93,114,'Sample capture and containment',22),(115,136,'Thermal imaging and tether',22),
 (137,172,'Safety diagnostics and critical feedback',36),(173,188,'SCADA and objective certification',16)]
GROUP_DEVICE=[
 [83,84,95,82,85,31,45,46,60,77],
 [54,55,56,57,58,59,60,61,62,63,64,65,66,67],
 [16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33],
 [34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53],
 [68,69,70,71,72,73,74,75,76,80,81],
 [40,44,50,52,56,64,65,76,77,78,79,80,81],
 [30,31,82,83,86,87,88,89,90,91,92,94,95],
 [5,9,60,61,63,82,85,90,93,96,97]]
WIRES=[];LOGICAL=[]
for g,(start,end,gname,ct) in enumerate(WIRE_GROUPS):
 for number in range(start,end+1):
  j=number-start;devices=GROUP_DEVICE[g]
  sidx=devices[j%len(devices)];tidx=devices[(j*5+3)%len(devices)]
  if sidx==tidx:tidx=devices[(j*5+4)%len(devices)]
  if g==0:sidx=83 if j<10 else 95;tidx=[82,85,45,46,60,31,30,54,77,90,93,95][j]
  if g==6 and number==171:sidx=30;tidx=82
  kind=(['POWER','POWER','POWER','RETURN'][j%4] if g==0 else
        ['ANALOG_FEEDBACK','DIGITAL_COMMAND_OR_STATUS','ENCODER_FEEDBACK'][j%3] if g in (1,2,3) else
        ['ANALOG_FEEDBACK','DIGITAL_COMMAND_OR_STATUS'][j%2] if g in (4,5) else
        ['SAFETY_FEEDBACK','DIGITAL_COMMAND_OR_STATUS'][j%2] if g==6 else 'SCADA_TELEMETRY')
  if number==171:kind='ANALOG_SAFETY_FEEDBACK'
  sid=f'W{number:03d}';aa=BY_MODEL[sidx][(j+g)%len(BY_MODEL[sidx])];bb=BY_MODEL[tidx][(j+g+1)%len(BY_MODEL[tidx])]
  if number==171:aa=BY_MODEL[30][0];bb=BY_MODEL[82][0]
  detail='Critical trusted six-leg anchor load measurement and freshness' if number==171 else f'{gname} signal {j+1:02d}'
  WIRES.append(dict(WireID=sid,Group=gname,SourceModel=NAMES[sidx],SourceVisualAnchor=aa['AnchorID'],
    TargetModel=NAMES[tidx],TargetVisualAnchor=bb['AnchorID'],SignalKind=kind,Purpose=detail,
    Critical='YES' if number==171 else 'NO',Status='CONCEPT_ONLY_NO_LIVE_PLAYER_WIRE'))
  for side,ii,anchor in [('Source',sidx,aa),('Target',tidx,bb)]:
   fid=('TRUSTED_ANCHOR_LOAD_OUT' if side=='Source' else 'TRUSTED_ANCHOR_LOAD_IN') if number==171 else f'{sid}_{side.upper()}_{kind}'
   LOGICAL.append(dict(WireID=sid,Endpoint=side,Model=NAMES[ii],ArtAnchor=anchor['AnchorID'],
    FunctionalPortID=fid,SignalKind=kind,Direction='OUTPUT' if side=='Source' else 'INPUT',
    Critical='YES' if number==171 else 'NO',FunctionalStatus='PLANNED_SEPARATE_LOGICAL_CONTACT_NOT_SPARKTERMINAL'))
assert len(WIRES)==188 and len(LOGICAL)==376
shared=defaultdict(list)
for q in LOGICAL:shared[q['ArtAnchor']].append(q)
CONFLICTS=[]
for aid,rows in shared.items():
 if len(rows)<2:continue
 CONFLICTS.append(dict(AnchorID=aid,OwnerModel=rows[0]['Model'],LogicalContactCount=len(rows),
   SignalKinds=';'.join(sorted({q['SignalKind'] for q in rows})),MixedSignalKinds=len({q['SignalKind'] for q in rows})>1,
   Requirement='RECONCILE_INDEPENDENT_FUNCTIONAL_CONTACTS_NEVER_SHORT_MIXED_SIGNALS'))
OBJECTIVES=[
 ('Build the Asteroid Exploration Network','188 real player-created wires, functional terminals, power/return validation and independent anchor safety including W171'),
 ('Map the Hidden Geological Layers','Calibrated survey instruments; scan coverage; measurable stratigraphy and uncertainty; scientifically defensible drill-site selection'),
 ('Deploy and Stabilize the Microgravity Lander','Finite deployment, measured six-leg terrain contact, microspine attachment, controlled reaction-load testing and valid safety margins'),
 ('Extract and Secure the Asteroid Core','Actual penetration dynamics, safe feed torque and thermal limits, coherent geological core retention, sealed cartridge and valid sample evidence'),
 ('Recover Three Asteroid Sampling Failures','Prove anchor slip, drill bit stall and sample seal failure; diagnose, repair and complete a fresh fully qualified sampling run'),
 ('Restore Critical Anchor Load Feedback','Actual W171 disconnect, fail-safe drilling inhibit, reconnect, fresh load calibration, explicit reset, complete new sampling and containment qualification')]

def csvwrite(name,rows):
 with (ROOT/name).open('w',newline='',encoding='utf-8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def write_tables():
 tables={
  'Animation_Rigs.csv':[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PV],
  'Art_Assembly.csv':[dict(Index=i,Model=n,File=f'{i:02d}_{n}.fbx',LocalPosition='0,0,0',NominalWorldCenter_m=','.join(map(str,P[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres') for i,n in enumerate(NAMES)],
  'Component_Roles.csv':[dict(Index=i,Model=n,WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],Role='Environment' if i<16 else 'Lander and anchors' if i<34 else 'Coring drill' if i<54 else 'Geological sensors' if i<68 else 'Sample and tether' if i<82 else 'PLC SCADA fault safety',Status='ART_ONLY') for i,n in enumerate(NAMES)],
  'Gameplay_Wiring_Budget.csv':[dict(WireIDRange=f'W{s:03d}–W{e:03d}',Subsystem=name,Count=count) for s,e,name,count in WIRE_GROUPS],
  'Gameplay_Wiring_Concept_W001_W188.csv':WIRES,
  'Objective_Certification_Plan_6.csv':[dict(ObjectiveIndex=i+1,Title=name,AcceptanceCriteria=criteria,Implemented='NO') for i,(name,criteria) in enumerate(OBJECTIVES)],
  'Terminal_Anchor_Plan_266.csv':ANCHORS,'Visual_Terminal_Sockets_266.csv':ANCHORS,
  'Terminal_Logical_Port_Reconciliation_376.csv':LOGICAL,
  'Terminal_Anchor_Signal_Conflict_Report.csv':CONFLICTS}
 for name,rows in tables.items():csvwrite(name,rows)
 w=WIRES[170]
 (ROOT/'W171_Critical_Anchor_Load_Recovery.md').write_text(f'''# PROJECT SPARK LEVEL 92 — CRITICAL W171 ANCHOR-LOAD FEEDBACK RECOVERY

## Source-authored proposed circuit
- Source: `{w['SourceModel']}` at visual socket **{w['SourceVisualAnchor']}**, functional output `TRUSTED_ANCHOR_LOAD_OUT`.
- Destination: `{w['TargetModel']}` at visual socket **{w['TargetVisualAnchor']}**, functional input `TRUSTED_ANCHOR_LOAD_IN`.
- `WireID=W171`, `SignalKind=ANALOG_SAFETY_FEEDBACK`, `Critical=YES`.
- In this ZIP the sockets are mesh-only and the connection is **NOT** a real SparkTerminal player wire.

## Required authoritatively simulated sequence
1. Objectives 1–5 are historically certified. Enter a supervised low-load mechanical fault-test condition with no unsupported payload.
2. Player physically disconnects real W171 using the existing Project Spark wire tool; actual circuit graph changes to open.
3. Invalidate trusted anchor load feedback, freeze old sample timestamps; *missing data does not mean zero load*.
4. Revoke permission for new drilling feed/percussion, safely reduce drill motion subject to motor/bit inertia, maintain available anchor hold and tether support.
5. Independently enforce motor disable/emergency protections; record exact latched fault and verified motor/anchor feedback states. Never fake immediate retraction or release.
6. Player reconnects W171; validate correct electrical types, returns, powered force transducer, sampling rate and fresh measured per-leg anchor-load data.
7. Recalibrate load-cell array, verify all six feet and tether, explicitly reset the safety PLC only when conditions pass.
8. Generate a NEW qualification runID. Rerun anchor stability, penetration, core recovery and seal integrity with new timestamps and fresh evidence.
9. Preserve historical Objectives 1–5; award Objective 6 only after **real LevelGamePlayManager 6/6** driven by complete post-fault evidence.

No functional electrical simulation, safety PLC, or objective evaluator is present in the art package; those must be implemented in Unity.
''',encoding='utf8')
 return tables

def add_sockets(i):
 a=build_art(i)
 for s in BY_MODEL[i]:a.socket((s['WorldX_m'],s['WorldY_m'],s['WorldZ_m']),s['AnchorID'])
 return a

def pivothomes(i,a):
 return [(q['Pivot'],a.name,PIVOT_POS[q['Pivot']]) for q in PV if q['Index']==i]+[(f'SOCKET_{aid}',a.name,xyz) for aid,xyz in a.sockets]

def export_models():
 ts=time.monotonic();results=[];chunks_all=[];homes_all=[]
 bmin=np.ones(3)*np.inf;bmax=np.ones(3)*-np.inf
 for i,n in enumerate(NAMES):
  art=add_sockets(i);parts=core.chunk_model(art,max_shapes=18);homes=pivothomes(i,art)
  path_f=ROOT/'Models'/f'{i:02d}_{n}.fbx';path_g=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  f=core.export_fbx(str(path_f),[(n,p) for p in parts],[n],homes,art.sockets)
  g=core.export_glb(str(path_g),[(n,p) for p in parts],[n],homes)
  amap=dict(art.sockets)
  for part in parts:
   p=part['pivot'];ofs=PIVOT_POS[p] if p in PIVOT_POS else amap[p[7:]] if p and p.startswith('SOCKET_') else (0,0,0)
   v=part['vertices']+np.asarray(ofs);bmin=np.minimum(bmin,v.min(axis=0));bmax=np.maximum(bmax,v.max(axis=0))
  results.append(dict(index=i,name=n,fbx=path_f.name,glb=path_g.name,
    triangles=sum(len(p['faces']) for p in parts),mesh_chunks=len(parts),source_primitives=len(art.raw),
    socket_nodes=len(art.sockets),pivot_names=[q['Pivot'] for q in PV if q['Index']==i],
    fbx_bytes=f['file_size'],glb_bytes=g['glb_bytes']))
  chunks_all.extend((n,p) for p in parts);homes_all.extend(homes)
  if (i+1)%12==0 or i in (0,2,16,18,24,34,36,54,67,69,77,82,97):
   print(f'EXPORTED {i+1:02d}/98 {n} tris={results[-1]["triangles"]} sec={time.monotonic()-ts:.1f}',flush=True)
 fname='Level92_Complete_ArtKit_Showcase.fbx';gname='Level92_Complete_ArtKit_Showcase.glb'
 core.export_fbx(str(ROOT/'Models'/fname),chunks_all,NAMES,homes_all,
  [(q['AnchorID'],(q['WorldX_m'],q['WorldY_m'],q['WorldZ_m'])) for q in ANCHORS])
 core.export_glb(str(ROOT/'GLB'/gname),chunks_all,NAMES,homes_all)
 manifest=dict(level_id='level_92',title='The Silent Core',subtitle='Asteroid Geophysics and Microgravity Autonomous Coring',
   modular_fbx_count=len(results),modular_glb_count=len(results),assembled_fbx=fname,assembled_glb=gname,
   triangle_count=sum(m['triangles'] for m in results),mesh_chunk_count=sum(m['mesh_chunks'] for m in results),
   animation_pivots=len(PV),decorative_sockets=len(ANCHORS),planned_player_wires=len(WIRES),
   proposed_independent_logical_endpoints=len(LOGICAL),reused_socket_locations=len(CONFLICTS),
   mixed_signal_socket_locations=sum(bool(q['MixedSignalKinds']) for q in CONFLICTS),
   bounds_min_m=bmin.round(4).tolist(),bounds_max_m=bmax.round(4).tolist(),geometry_axis='Y-up',
   fbx_numeric_units='centimetres',glb_numeric_units='metres',shared_model_origins=True,
   actual_player_wires_included=False,functional_sparkterminals_included=False,
   real_geophysical_solver_included=False,real_drilling_solver_included=False,
   levelgameplaymanager_integration_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
   objective_certifications_implemented=False,models=results)
 (ROOT/'Model_Manifest.json').write_text(json.dumps(manifest,indent=2))
 print('ART KIT GENERATED',manifest['triangle_count'],'triangles in',time.monotonic()-ts,'sec',flush=True)
 return manifest

if __name__=='__main__':
 print('PROJECT SPARK LEVEL 92: GENERATING REAL MODULAR GEOMETRY',flush=True)
 write_tables();export_models()
