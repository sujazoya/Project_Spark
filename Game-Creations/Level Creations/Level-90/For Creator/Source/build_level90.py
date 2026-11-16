#!/usr/bin/env python3
"""Project Spark Level 90 THE LONG BURN: procedural technical 3D asset builder.
Produces genuine modular FBX 7400 and GLB 2.0 geometry and source-grounded art plans.
NOT a Unity gameplay implementation, production sculpt, UV-authored AAA, or tested flight system.
Y-up X-width Z-front. FBX numeric centimetres converted once by importer, GLB metres.
"""
from pathlib import Path
from collections import Counter,defaultdict
import math,random,time,csv,json
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for d in ['Models','GLB','Textures','Reports','Unity_Optional','Source']: (ROOT/d).mkdir(exist_ok=True,parents=True)
NAMES=['L90_'+n.strip() for n in (Path(__file__).parent/'model_names.txt').read_text().splitlines() if n.strip()]
assert len(NAMES)==94
random.seed(90090)
core.COLORS={
 'structural':(.39,.47,.57,1),'titanium':(.72,.79,.85,1),'dark':(.035,.054,.078,1),
 'graphite':(.09,.13,.20,1),'copper':(.88,.47,.19,1),'gold':(.97,.69,.26,1),
 'ceramic':(.92,.94,.95,1),'glass':(.15,.56,.82,.47),'panel':(.018,.082,.16,1),
 'pcb':(.045,.22,.17,1),'cyan':(.08,.85,1,1),'blue':(.17,.40,1,1),
 'green':(.12,.92,.52,1),'amber':(1,.63,.13,1),'violet':(.59,.39,1,1),
 'red':(1,.17,.14,1),'orange':(1,.38,.055,1),'white':(.98,.98,.95,1),
 'plasma':(.18,.52,1,.69),'solar':(.07,.17,.43,1),'radiator':(.07,.11,.17,1),
 'insulation':(.22,.28,.32,1),'floor':(.23,.29,.37,1),'stars':(.80,.9,1,1),'black':(.013,.019,.029,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
# All model art placed at global shared origin: the individual FBX/GLB groups assemble with identity transforms.
P={
0:(0,-.20,0),1:(0,8,-15.5),2:(4.8,7.6,-13.2),3:(0,2.0,-5.4),4:(-9,9.0,-12.1),5:(0,5.15,-4.0),
6:(0,8.0,-12.5),7:(8.5,6.9,-5.3),8:(0,.35,3.2),9:(0,9.0,-10.6),10:(0,3.4,-7.1),11:(12.4,3.2,-3.5),
12:(-10,1.5,-4.9),13:(-12,9.6,-11.6),14:(10.8,9.9,-11.3),15:(0,11.0,-10.6),
16:(0,5.7,-5.5),17:(0,5.7,-11.0),18:(0,5.7,-7.4),19:(-8.0,6.15,-5.9),20:(8.0,6.15,-5.9),
21:(-3.3,5.9,-5.9),22:(3.3,5.9,-5.9),23:(-7.1,1.7,4.2),24:(-2.6,7.25,-5.3),25:(2.6,7.25,-5.3),
26:(0,5.45,-5.7),27:(0,5.65,-2.9),28:(0,7.15,-9.0),29:(-.9,5.6,-8.4),30:(.9,5.6,-8.4),31:(0,4.5,-6.4),
32:(-1.12,6.68,-2.4),33:(1.12,6.68,-2.4),34:(-1.12,4.62,-2.4),35:(1.12,4.62,-2.4),
36:(-10.4,4.4,-5.3),37:(-8.5,3.1,-5.0),38:(-2.1,3.2,-3.0),39:(-.7,3.2,-3.0),40:(.7,3.2,-3.0),41:(2.1,3.2,-3.0),
42:(-2.1,2.1,-4.1),43:(-.7,2.1,-4.1),44:(.7,2.1,-4.1),45:(2.1,2.1,-4.1),
46:(5.6,2.1,-3.4),47:(7.7,2.2,-3.4),48:(-12.1,2.7,-2.5),49:(-9.4,2.7,-2.5),
50:(-12.0,1.7,.65),51:(-10.7,1.7,.65),52:(-9.4,1.7,.65),53:(-8.1,1.7,.65),54:(-6.8,1.7,.65),
55:(-5.5,1.7,.65),56:(-12.0,1.7,3.0),57:(-10.7,1.7,3.0),58:(-9.4,1.7,3.0),59:(-8.1,1.7,3.0),
60:(-6.8,1.7,3.0),61:(-5.5,1.7,3.0),62:(-2.2,1.75,5.7),63:(-.9,1.75,5.7),
64:(.4,1.75,5.7),65:(1.7,1.75,5.7),66:(3,1.75,5.7),67:(4.3,1.75,5.7),
68:(5.6,1.75,5.7),69:(6.9,1.75,5.7),70:(8.2,1.75,5.7),71:(9.5,1.75,5.7),
72:(10.8,1.75,5.7),73:(12.1,1.75,5.7),74:(-12.0,1.75,6.5),75:(-10.7,1.75,6.5),
76:(-9.4,1.75,6.5),77:(-8.1,1.75,6.5),78:(-6.8,1.75,6.5),79:(-5.5,1.75,6.5),
80:(-4.2,1.75,6.5),81:(-2.9,1.75,6.5),82:(-1.6,1.75,6.5),83:(-.3,1.75,6.5),
84:(1.0,1.75,6.5),85:(2.3,1.75,6.5),86:(3.6,1.75,6.5),87:(4.9,1.75,6.5),
88:(6.2,1.75,6.5),89:(7.5,1.75,6.5),90:(0,10.1,-10.35),91:(-7.7,9.0,-10.5),
92:(9,1.8,6.5),93:(10.5,1.8,6.5)
}
assert set(P)==set(range(94)),set(range(94))-set(P)
# Mechanically meaningful and instrument status parent transforms. Dynamic parameters come from runtime systems, not these pivots.
PV=[
(3,'VEHICLE_CLAMP_RELEASE','translation','x'),(4,'TRAJECTORY_VECTOR','indicator','none'),(5,'TEST_CHAMBER_BAFFLE','translation','y'),
(7,'INSPECTION_GANTRY_X','translation','x'),(7,'INSPECTION_GANTRY_Z','translation','z'),(9,'MISSION_HOLOGRAM','indicator','none'),
(10,'DOCKING_RELEASE_RING','rotation','z'),(11,'INTERLOCK_GATE','rotation','y'),(13,'ORBITAL_LIGHTING_SWEEP','rotation','y'),
(14,'TRANSFER_DISPLAY','indicator','none'),(15,'FINAL_CERTIFICATION','indicator','none'),
(16,'SPACECRAFT_INTEGRATION_ROTATION','rotation','y'),(17,'NOSE_SERVICE_HATCH','rotation','x'),(18,'SERVICE_RING','rotation','z'),
(19,'PORT_SOLAR_WING','rotation','z'),(19,'PORT_SOLAR_OUTER','rotation','y'),
(20,'STARBOARD_SOLAR_WING','rotation','z'),(20,'STARBOARD_SOLAR_OUTER','rotation','y'),
(21,'PORT_SOLAR_HINGE','rotation','z'),(22,'STARBOARD_SOLAR_HINGE','rotation','z'),
(24,'RADIATOR_PORT_EXTEND','rotation','y'),(25,'RADIATOR_STARBOARD_EXTEND','rotation','y'),
(27,'THRUSTER_CLUSTER_TRUSS','indicator','none'),(28,'NAVIGATION_HEAD_PAN','rotation','y'),
(32,'ENGINE_A_GIMBAL_YAW','rotation','y'),(32,'ENGINE_A_GIMBAL_PITCH','rotation','x'),(32,'ENGINE_A_PLASMA_DISCHARGE','indicator','none'),
(33,'ENGINE_B_GIMBAL_YAW','rotation','y'),(33,'ENGINE_B_GIMBAL_PITCH','rotation','x'),(33,'ENGINE_B_PLASMA_DISCHARGE','indicator','none'),
(34,'ENGINE_C_GIMBAL_YAW','rotation','y'),(34,'ENGINE_C_GIMBAL_PITCH','rotation','x'),(34,'ENGINE_C_PLASMA_DISCHARGE','indicator','none'),
(35,'ENGINE_D_GIMBAL_YAW','rotation','y'),(35,'ENGINE_D_GIMBAL_PITCH','rotation','x'),(35,'ENGINE_D_PLASMA_DISCHARGE','indicator','none'),
(36,'DISCHARGE_CUTAWAY_ROTATE','rotation','y'),(37,'ANODE_DISTRIBUTOR_VALVE','rotation','z'),
(38,'MAGNET_COIL_A_FIELD','indicator','none'),(39,'MAGNET_COIL_B_FIELD','indicator','none'),
(40,'MAGNET_COIL_C_FIELD','indicator','none'),(41,'MAGNET_COIL_D_FIELD','indicator','none'),
(42,'CATHODE_A_STATUS','indicator','none'),(43,'CATHODE_B_STATUS','indicator','none'),
(44,'CATHODE_C_STATUS','indicator','none'),(45,'CATHODE_D_STATUS','indicator','none'),
(46,'GIMBAL_CALIBRATION_YAW','rotation','y'),(47,'THRUST_CALIBRATION_SLIDE','translation','x'),
(48,'XENON_TANK_A_GAUGE','indicator','none'),(49,'XENON_TANK_B_GAUGE','indicator','none'),
(51,'ISOLATION_VALVE_A','rotation','y'),(52,'ISOLATION_VALVE_B','rotation','y'),
(53,'REGULATOR_A_DIAL','rotation','z'),(54,'REGULATOR_B_DIAL','rotation','z'),
(55,'XENON_MANIFOLD_SELECTOR','rotation','y'),
(56,'FLOW_A_DIAL','rotation','z'),(57,'FLOW_B_DIAL','rotation','z'),
(58,'FLOW_C_DIAL','rotation','z'),(59,'FLOW_D_DIAL','rotation','z'),
(62,'PPU_A_OUTPUT_INDICATOR','indicator','none'),(63,'PPU_B_OUTPUT_INDICATOR','indicator','none'),
(64,'PPU_C_OUTPUT_INDICATOR','indicator','none'),(65,'PPU_D_OUTPUT_INDICATOR','indicator','none'),
(67,'PPU_PRECHARGE_CONTACTOR','translation','y'),(70,'MAGNET_CURRENT_DIAL','rotation','z'),
(71,'CATHODE_KEEPER_ENABLE','translation','x'),(72,'IGNITION_SEQUENCER','indicator','none'),
(73,'NEUTRALIZER_CURRENT_MONITOR','indicator','none'),(74,'PROPULSION_SAFETY_STATUS','indicator','none'),
(76,'THERMAL_PUMP_ROTOR','rotation','z'),(77,'THERMAL_RADIATOR_POSITION','rotation','x'),
(78,'SPECTROMETER_SCAN_HEAD','rotation','y'),(79,'FARADAY_PROBE_TRANSLATION','translation','x'),
(81,'THRUST_BALANCE_FLEXURE','translation','z'),(82,'THRUST_ALLOCATOR_DISPLAY','indicator','none'),
(83,'TRAJECTORY_GUIDANCE_DISPLAY','indicator','none'),(84,'AUTONOMOUS_DEPARTURE_STATUS','indicator','none'),
(85,'STAR_TRACKER_CALIBRATION','rotation','y'),(86,'GIMBAL_FEEDBACK_DISPLAY','indicator','none'),
(87,'XENON_FAULT_VALVE','translation','x'),(88,'MAGNET_THERMAL_FAULT','translation','x'),
(89,'VECTOR_MISALIGNMENT_FAULT','translation','x'),(90,'PROPULSION_SCADA','indicator','none'),
(91,'PLASMA_VECTOR_HOLOGRAM','indicator','none'),(92,'W157_CRITICAL_PATCH','indicator','none'),
(93,'DEPARTURE_EVENT_RECORDER','indicator','none')]
PV=[row for row in PV if row[1] not in {"TRANSFER_DISPLAY","THRUSTER_CLUSTER_TRUSS","CATHODE_KEEPER_ENABLE","THRUST_ALLOCATOR_DISPLAY","GIMBAL_FEEDBACK_DISPLAY","DEPARTURE_EVENT_RECORDER"}]
assert len(PV)==80,len(PV)
PIVOTS=[dict(Pivot='ANIM_'+n,Index=i,Model=NAMES[i],Kind=k,Axis=ax) for i,n,k,ax in PV]
PIVOT_POS={p['Pivot']:P[p['Index']] for p in PIVOTS}
for i in [32,33,34,35]:
 PIVOT_POS[f'ANIM_ENGINE_{"ABCD"[i-32]}_PLASMA_DISCHARGE']=(P[i][0],P[i][1],P[i][2]+.42)
 PIVOT_POS[f'ANIM_ENGINE_{"ABCD"[i-32]}_GIMBAL_PITCH']=(P[i][0],P[i][1],P[i][2]-.31)
PIVOT_POS['ANIM_PORT_SOLAR_OUTER']=(-10.4,6.15,-5.9)
PIVOT_POS['ANIM_STARBOARD_SOLAR_OUTER']=(10.4,6.15,-5.9)
core.PIVOT_POS=PIVOT_POS
PV_BY_MODEL=defaultdict(list)
for pivot in PIVOTS:PV_BY_MODEL[pivot['Index']].append(pivot['Pivot'])

def b(a,p,d,m='titanium',pv=None,l='MachinedPart'):a.box(p,d,m,pv,l)
def cy(a,p,r,h,m='titanium',axis='y',pv=None,l='PrecisionCylinder',seg=22):a.cylinder(p,r,h,m,axis=axis,seg=seg,pivot=pv,label=l)
def tor(a,p,R,r,m='titanium',axis='z',pv=None,l='AnnularEngineRing',seg=52,tube=9):a.torus(p,R,r,m,axis=axis,seg=seg,tube=tube,pivot=pv,label=l)
def beam(a,p,q,r=.05,m='structural',pv=None,l='PrecisionTruss'):a.beam(p,q,r,m,pivot=pv,label=l,sides=8)
def sphere(a,p,r,m='titanium',pv=None,l='PressurizedSphericalVessel',subs=2):
 mesh=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(mesh.vertices+np.asarray(p),mesh.faces,m,pv,l)
def panel(a,p,theme='cyan',sp='plc',pv=None,scale=1):
 core.panel_equipment(a,p,scale,theme,sp)
 x,y,z=p
 for j in range(7):b(a,(x+(j-3)*.118*scale,y-.49*scale,z+.36*scale),(.062,.050,.09),'gold',None,'GoldPlatedSignalTerminal')
 if pv:b(a,(x,y+.42*scale,z+.455*scale),(.32,.075*scale,.03),'green',pv,'ActualStateIndicator')
def valve(a,p,pv=None):
 x,y,z=p;cy(a,p,.17,.62,'structural','y',None,'GasValveBody')
 tor(a,(x,y+.35,z),.28,.042,'gold','y',pv,'MotorizedValveSelector',32)
 for j in range(6):
  t=j*math.tau/6;beam(a,(x,y+.35,z),(x+.26*math.cos(t),y+.35,z+.26*math.sin(t)),.021,'copper',pv,'ValveControlSpoke')
 for sx in (-1,1):cy(a,(x+sx*.26,y,z),.20,.12,'titanium','x',None,'ValveFlangedConnection')
def pipe(a,pts,r=.065,m='copper'):
 for q,w in zip(pts,pts[1:]):beam(a,q,w,r,m,None,'XenonStainlessFeedTube')
 for v in [pts[0],pts[-1]]:cy(a,v,r*1.5,.055,'gold','y',None,'GasSealFerrule')
def vessel(a,p,r=.69,h=3,pv=None):
 x,y,z=p;cy(a,p,r,h,'structural','y',None,'HighPressureXenonVessel',44)
 for s in [-1,1]:
  sphere(a,(x,y+s*h/2,z),r,'titanium',None,'VesselHemisphericalDome',1)
  tor(a,(x,y+s*h*.41,z),r+.06,.065,'gold','y',None,'VesselHighPressureJoint',40)
 for j in range(16):
  theta=j*math.tau/16
  cy(a,(x+(r+.075)*math.cos(theta),y-h/2+.25,z+(r+.075)*math.sin(theta)),.038,.17,'gold','y',None,'XenonTankRetainingBolt',10)
 b(a,(x,y,z+r+.10),(.55,1.05,.085),'panel',None,'TankGlassTelemetryViewportFrame')
 b(a,(x,y,z+r+.15),(.46,.94,.025),'glass',None,'TankTelemetryWindow')
 for j in range(12):b(a,(x,y-.37+j*.068,z+r+.18),(.33,.020,.018),'cyan' if j<9 else 'amber',pv,'TankLevelDigitalGauge')
 for t in [-1,1]:b(a,(x+t*.4,y-h*.38,z),(.15,.32,1.45),'titanium',None,'TitaniumBottleRetentionBracket')
def frame(a,p,w=1.7,h=2.2,d=.9):
 x,y,z=p
 for sx in [-1,1]:b(a,(x+sx*w/2,y,z),(.12,h,d),'structural',None,'CNCFrameVertical')
 for sy in [-1,1]:b(a,(x,y+sy*h/2,z),(w,.12,d),'titanium',None,'CNCFrameHorizontal')

def thruster(a,p,engine='A',pv=None):
 x,y,z=p
 yaw=f'ANIM_ENGINE_{engine}_GIMBAL_YAW';pitch=f'ANIM_ENGINE_{engine}_GIMBAL_PITCH';glow=f'ANIM_ENGINE_{engine}_PLASMA_DISCHARGE'
 cy(a,(x,y,z-.56),.72,.40,'structural','z',None,'HallThrusterGimbalMount',32)
 tor(a,(x,y,z-.34),.81,.11,'gold','z',yaw,'VectoringGimbalAzimuthBearing',48)
 tor(a,(x,y,z-.20),.78,.085,'titanium','z',pitch,'VectoringGimbalElevationRing',48)
 cy(a,(x,y,z+.12),.80,.75,'titanium','z',pitch,'HallThrusterOuterCeramicDischargeBody',48)
 cy(a,(x,y,z+.53),.62,.12,'graphite','z',pitch,'AnnularEngineCeramicChannel',48)
 tor(a,(x,y,z+.58),.62,.11,'ceramic','z',pitch,'HexagonalBoronNitrideAnnularFace',64)
 tor(a,(x,y,z+.62),.45,.080,'cyan','z',glow,'MeasuredDischargeGlowRing',64)
 tor(a,(x,y,z+.64),.30,.058,'blue','z',glow,'ActivePlasmaEmissionAnnulus',48)
 cy(a,(x,y,z+.58),.27,.20,'graphite','z',pitch,'CentralAnnularPolePiece',36)
 tor(a,(x,y,z+.62),.20,.030,'gold','z',pitch,'MagneticCircuitInnerPoleRing',42)
 for j in range(24):
  t=j*math.tau/24
  xx=x+.72*math.cos(t);yy=y+.72*math.sin(t)
  cy(a,(xx,yy,z+.16),.026,.30,'copper','z',pitch,'AnnularDischargeSupportFastener',10)
 for j in range(8):
  theta=j*math.tau/8
  beam(a,(x+.75*math.cos(theta),y+.75*math.sin(theta),z-.56),
          (x+.75*math.cos(theta),y+.75*math.sin(theta),z+.36),.042,'gold',pitch,'DischargeAnodeSupportRod')
 for j in range(4):
  t=j*math.tau/4
  b(a,(x+.63*math.cos(t),y+.63*math.sin(t),z-.05),(.22,.22,.70),'copper',pitch,'QuadrantMagnetWinding')
 # energy plume is ART-ONLY emissive mesh not a real plume or simulated force
 for j in range(3):tor(a,(x,y,z+.70+j*.09),.43-j*.09,.027,'plasma','z',glow,'PlasmaVisualizationOnly',44)

def solar(a,p,side,pv=None):
 x,y,z=p;xs= -1 if side=='port' else 1
 b(a,(x,y,z),(8.45,.12,2.95),'structural',pv,'HighStiffnessPhotovoltaicPanelFrame')
 b(a,(x,y+.10,z),(8.17,.045,2.68),'solar',pv,'BlueBlackMultijunctionSolarCellSubstrate')
 for j in range(34):
  xx=x-4.05+j*.245
  b(a,(xx,y+.14,z),(.021,.025,2.60),'gold',pv,'SolarArrayFineBusbar')
 for j in range(12):
  zz=z-1.26+j*.229
  b(a,(x,y+.147,zz),(8.07,.016,.020),'cyan' if j%4==0 else 'titanium',pv,'SolarCellInterconnectString')
 for j in range(6):
  xx=x-3.75+j*1.5
  b(a,(xx,y-.16,z),(.07,.22,2.98),'titanium',pv,'SolarPanelStructuralBackSpar')
 for t in [-1,1]:
  b(a,(x+t*4.1,y,z),(.10,.24,3.10),'gold',pv,'SolarPanelEdgeRail')
 if side=='port':outer='ANIM_PORT_SOLAR_OUTER'
 else:outer='ANIM_STARBOARD_SOLAR_OUTER'
 b(a,(x+xs*2.4,y,z+1.12),(.83,.11,.58),'structural',outer,'DeployableSolarOuterPanelHingeInspectionBlock')

def make_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i];pv=PV_BY_MODEL[i][0] if PV_BY_MODEL[i] else None
 if i==0:
  b(a,(0,-.20,0),(32,.48,25),'floor',None,'PropulsionServiceDeckStructuralHull')
  for j in range(18):b(a,(-15.7+j*1.85,.05,0),(.034,.03,24),'graphite',None,'AntiSlipCNCMetalGratingSeam')
  for j in range(16):b(a,(0,.05,-11.5+j*1.52),(31,.035,.035),'titanium',None,'LoadBearingServiceDeckGrid')
  for side in [-1,1]:
   b(a,(side*15.55,.05,0),(.18,.25,23.7),'gold',None,'WorkstationSafetyLine')
   for j in range(17):b(a,(side*15.05,.10,-10.9+j*1.35),(.42,.025,.15),'amber',None,'HazardFloorMarker')
  for j in range(6):b(a,(-12.5+j*5,.11,6),(.10,.14,7),'structural',None,'ControlBayCableContainmentChannel')
 elif i==1:
  b(a,(0,8,-15.3),(31.7,13.8,.15),'glass',None,'OrbitObservationPanorama')
  for j in range(13):beam(a,(-15+j*2.5,1.2,-15.2),(-15+j*2.5,15.0,-15.2),.07,'titanium',None,'PressureResistantViewportMullion')
  for yy in (1.3,6.5,11.6,15.1):beam(a,(-15.5,yy,-15.15),(15.5,yy,-15.15),.09,'structural',None,'ObservatoryFrameHorizontalBeam')
  for j in range(115):b(a,(random.uniform(-15,15),random.uniform(1,15),-15.48),(.016,.016,.015),'stars',None,'ExternalStarfieldDot')
 elif i==2:
  for h in [2.5,5,7.5,10.0]:
   beam(a,(-14,h,-11.3),(14,h,-11.3),.10,'structural',None,'OrbitalStationExternalTruss')
  for xx in [-13,-10,-7,-4,-1,2,5,8,11,14]:
   beam(a,(xx,2,-11.3),(xx,11,-11.3),.10,'titanium',None,'OrbitalStationVerticalTruss')
   for j in range(4):b(a,(xx,2.7+j*2.45,-11.17),(.36,.24,.35),'gold',None,'OrbitalStationTrussLatch')
 elif i==3:
  for side in [-1,1]:
   beam(a,(side*2.0,.6,-12),(side*2.0,3.8,-12),.15,'structural',None,'IntegrationStandVerticalLoadPath')
   beam(a,(side*2.0,.6,-2.2),(side*2.0,3.8,-2.2),.15,'structural',None,'IntegrationStandRearSupport')
   beam(a,(side*2.0,3.65,-12),(side*2.0,3.65,-2.2),.12,'titanium',None,'SpacecraftSupportClampBeam')
   b(a,(side*1.55,3.8,-7.0),(.72,.3,.80),'gold',pv,'SpacecraftActualReleaseClamp')
  for zz in np.linspace(-11,-3,9):b(a,(0,.8,float(zz)),(4.4,.25,.27),'graphite',None,'InspectionAccessPlatformRail')
 elif i in (4,14):
  tor(a,(x,y,z),2.2,.07,'cyan','y',pv,'OrbitalTransferRouteHologram',68)
  tor(a,(x,y,z+.2),1.42,.045,'gold','z',pv,'NavigationTargetingDisplay',60)
  for j in range(12):
   t=j*math.tau/12
   beam(a,(x,y,z),(x+2*math.cos(t),y+.55*math.sin(t),z+1.2*math.sin(t)),.02,'cyan',pv,'DecorativeOrbitVector')
 elif i==5:
  for zz in [-4.75,-3.55,-2.35]:
   tor(a,(0,5.2,zz),3.3,.17,'structural','z',None,'PlasmaTestChamberSafetyEnclosureRib',70)
   tor(a,(0,5.2,zz+.06),2.9,.06,'cyan','z',None,'TransparentTestChamberBoundaryLine',60)
  for j in range(12):
   t=j*math.tau/12
   beam(a,(3.12*math.cos(t),5.2+3.12*math.sin(t),-4.7),
            (3.12*math.cos(t),5.2+3.12*math.sin(t),-2.3),.07,'titanium',None,'PlasmaTestChamberAxialReinforcement')
  b(a,(0,8.4,-4),(4.2,.22,.44),'gold',pv,'SlidingProtectedTestBaffle')
 elif i==6:
  for side in [-1,1]:
   b(a,(side*4.1,6,-4),(1.0,7.2,5.3),'insulation',None,'ThrusterProtectedRadiatorTestWall')
   for j in range(17):b(a,(side*4.7,2.9+j*.35,-4),(.15,.10,4.5),'structural',None,'ThermalGuardWallLouver')
 elif i==7:
  for xx in (5.5,11):beam(a,(xx,1,-3.5),(xx,11,-3.5),.10,'structural',None,'InspectionGantryPrecisionRail')
  beam(a,(5.5,10.7,-3.5),(11,10.7,-3.5),.13,'titanium',None,'GantryCrossbarActualMotion')
  b(a,(8.2,10.2,-3.5),(.52,.38,.7),'gold',PV_BY_MODEL[i][0],'InspectionGantryCrossSlide')
  b(a,(8.4,9.2,-3.3),(.30,1.9,.3),'cyan',PV_BY_MODEL[i][1],'InspectionGantryZOilfreeActuator')
 elif i==8:
  b(a,(0,.22,3.2),(29,.25,3.9),'structural',None,'RaisedPropulsionOperationWalkway')
  for j in range(28):b(a,(-13.8+j, .38,3.2),(.018,.025,3.6),'titanium',None,'OperatorWalkwayFloorSeam')
 elif i==9 or i==90:
  b(a,(x,y,z),(7.9,3.4,.18),'structural',None,'CinematicMissionSCADAFrame')
  b(a,(x,y,z+.12),(7.6,3.2,.08),'panel',None,'LargePanoramicDigitalTwinDisplay')
  for j in range(5):tor(a,(x-2.9+j*1.45,y+.40,z+.18),.43,.040,'cyan' if j<3 else 'amber','z',pv,'MissionProcessTelemetryGauge',36)
  for j in range(19):b(a,(x-3.30+j*.35,y-.83+(j*7%8)*.105,z+.19),(.12,.035,.027),'green' if j%3 else 'cyan',None,'HistoricalPropulsionTrendBars')
 elif i==10:
  tor(a,P[i],2.0,.18,'structural','z',None,'DockingReleaseMainRing',60)
  for j in range(8):
   t=j*math.tau/8
   b(a,(x+2*math.cos(t),y+2*math.sin(t),z+.25),(.30,.25,.42),'gold',pv,'DockedVehicleHoldingHook')
 elif i==11:
  b(a,P[i],(3.0,5.2,.7),'structural',None,'ProtectedPropulsionBayIsolationGate')
  b(a,(x,y,z+.38),(2.1,4.4,.18),'glass',pv,'InterlockedGlassSafetyGate')
 elif i==12:
  for j in range(7):beam(a,(-13,-.2,-5.5+j*.75),(-7,-.2,-5.5+j*.75),.07,'titanium',None,'ElectromechanicalInspectionRail')
 elif i==13:
  tor(a,P[i],2.0,.12,'gold','x',pv,'OrbitalEnvironmentalSunlightArc',58)
  for j in range(12):
   t=j*math.tau/12
   b(a,(x,y+2.05*math.cos(t),z+2.05*math.sin(t)),(.14,.18,.15),'amber',None,'LightingArcTechnicalElement')
 elif i==15:
  tor(a,P[i],1.2,.07,'gold','z',None,'FinalEngineMissionCertificationHalo')
  tor(a,(x,y,z+.19),.91,.045,'cyan','z',pv,'FinalMissionConfidenceRing')
  b(a,(x,y,z+.27),(1.08,.13,.09),'green',pv,'CompletedSixObjectiveVisualBeacon')
 elif i==16:
  # Service spacecraft cylindrical fuselage 9m long with structural concentric ribs, true mechanical body.
  cy(a,(0,5.72,-7.45),1.54,8.15,'ceramic','z',pv,'Aurora1MainPressureHull',64)
  for zz in np.linspace(-11.1,-3.9,17):tor(a,(0,5.72,float(zz)),1.57,.062,'titanium','z',pv,'SpacecraftCircularFrameReinforcement',60)
  for j in range(12):
   t=j*math.tau/12
   beam(a,(1.57*math.cos(t),5.72+1.57*math.sin(t),-11.25),
            (1.57*math.cos(t),5.72+1.57*math.sin(t),-3.5),.044,'structural',pv,'SpacecraftLongitudinalLoadStringer')
  for j in range(7):
   zz=-10.6+j*1.05
   b(a,(0,7.25,zz),(.62,.10,.45),'gold',pv,'SpacecraftServiceIdentificationPlate')
 elif i==17:
  # Layered forward avionics and faceted nose sensor mast.
  for j in range(5):
   zz=-11.8-j*.42;rad=1.38-j*.23
   cy(a,(0,5.72,zz),max(.28,rad),.43,'titanium','z',None,'ForwardSpacecraftSteppedNoseFairing',42)
  tor(a,(0,5.72,-11.82),1.34,.065,'gold','z',None,'NoseAvionicsJoint')
  b(a,(0,6.62,-11.5),(1.1,.24,1.2),'glass',pv,'ServiceableForwardInstrumentAccessHatch')
 elif i==18:
  for zz in [-8.2,-7.6,-7.0]:tor(a,(0,5.7,zz),1.81,.15,'titanium','z',pv,'MissionPayloadAnnularRing',64)
  for j in range(12):
   t=j*math.tau/12
   b(a,(1.8*math.cos(t),5.7+1.8*math.sin(t),-7.6),(.33,.26,.37),'gold',pv,'PayloadModuleMountingPort')
 elif i in (19,20):solar(a,P[i],'port' if i==19 else 'starboard',pv)
 elif i in (21,22):
  cy(a,P[i],.44,1.62,'titanium','z',None,'SolarWingHingeRotaryCarrier')
  tor(a,P[i],.52,.07,'gold','z',pv,'SolarArrayDeploymentEncoderRing',40)
  for j in range(6):
   t=j*math.tau/6
   cy(a,(x+.40*math.cos(t),y+.40*math.sin(t),z),.054,.19,'copper','z',pv,'DeployableArrayHingeBolt')
 elif i in (24,25,77):
  width,height=(2.4,2.3) if i in (24,25) else (1.65,2.25)
  b(a,P[i],(width,.18,height),'structural',pv,'HeatRejectionPanelBacking')
  for j in range(26):b(a,(x-width/2+.1+j*(width-.2)/25,y+.12,z),(.048,.10,height-.13),'radiator',pv,'ExtendedRadiatorHeatExchangeFin')
  for j in range(8):b(a,(x,y-.13,z-height/2+.16+j*(height-.32)/7),(width-.1,.056,.035),'copper',pv,'RadiatorHeatPipe')
 elif i==26:
  for side in [-1,1]:
   beam(a,(side*1.3,3.95,-7.5),(side*1.3,7.1,-7.5),.12,'structural',None,'XenonTankFrameVertical')
   beam(a,(side*1.3,3.95,-4),(side*1.3,7.1,-4),.12,'structural',None,'XenonTankFrameRearSupport')
  for j in range(5):beam(a,(-1.35,4.7+j*.50,-5.6),(1.35,4.7+j*.50,-5.6),.10,'gold',None,'PropellantTankRetentionCrossMember')
 elif i==27:
  for xx in (-2.0,0,2.0):
   beam(a,(xx,4.15,-3.9),(xx,7.25,-3.9),.11,'structural',None,'QuadThrusterTrussVertical')
  for yy in (4.1,5.65,7.2):beam(a,(-2.0,yy,-3.9),(2.0,yy,-3.9),.12,'titanium',None,'QuadThrusterTrussCrossbar')
  tor(a,(0,5.7,-3.8),2.0,.07,'gold','z',pv,'ThrusterAssemblyInterfaceAlignmentRing',52)
 elif i==28:
  cy(a,(x,y,z),.26,.65,'titanium','z',pv,'PrecisionNavigationTrackingHead')
  for xx in [-.54,0,.54]:
   cy(a,(x+xx,y,z+.43),.19,.22,'glass','z',pv,'TripleRedundantNavigationLens')
 elif i in (29,30,31):
  for j in range(8):
   zz=z-1.15+j*.33
   b(a,(x, y+.12,zz),(1.35,.2,.23),'graphite',None,'ProtectedAvionicsBayCardModule')
   for k in range(6):b(a,(x-.60+k*.22,y+.25,zz),(.115,.05,.12),'copper',None,'AvionicsHeatSinkAndBondedConnector')
 elif i in (32,33,34,35):thruster(a,P[i],'ABCD'[i-32],pv)
 elif i==36:
  tor(a,P[i],1.1,.13,'ceramic','z',pv,'HallDischargeCutawayOuterAnnulus')
  for j in range(6):tor(a,(x,y,z+.13+j*.10),1.0-j*.11,.041,['gold','copper','titanium','plasma','violet','cyan'][j],'z',pv,'SectionedPlasmaDischargeLayer',52)
 elif i==37:
  cy(a,P[i],.47,.70,'structural','z',None,'AnodeXenonDistributorBody')
  for j in range(12):
   t=j*math.tau/12
   cy(a,(x+.51*math.cos(t),y+.51*math.sin(t),z+.22),.064,.17,'gold','z',None,'DistributedXenonAnodeInjector')
  tor(a,(x,y,z+.51),.52,.06,'copper','z',pv,'DistributorActuatorAnnulus')
 elif i in (38,39,40,41):
  cy(a,P[i],.55,.65,'structural','z',None,'MagnetPoleCore')
  for j in range(15):tor(a,(x,y,z-.3+j*.042),.65,.035,'copper','z',None,'DenseCopperMagnetCoilTurn',44)
  tor(a,(x,y,z+.44),.68,.04,'cyan','z',pv,'ActualMagnetCurrentIndicator',48)
 elif i in (42,43,44,45):
  cy(a,P[i],.36,.68,'structural','z',None,'CathodeNeutralizerShieldBody')
  cy(a,(x,y,z+.41),.17,.36,'ceramic','z',None,'CathodeEmissionOrificeHolder')
  tor(a,(x,y,z+.55),.17,.037,'plasma','z',pv,'MeasuredNeutralizerElectronStatusRing',32)
  for j in range(5):cy(a,(x-.24+j*.12,y+.32,z+.25),.024,.20,'gold','y',None,'CathodeTelemetryConnectorPin')
 elif i==46:
  for j in range(4):
   xx=x+(j-1.5)*.55
   cy(a,(xx,y,z),.28,.72,'structural','z',None,'GimbalMotorCase')
   tor(a,(xx,y,z+.39),.26,.055,'gold','z',pv,'GimbalCalibrationEncoderFace',30)
 elif i==47:
  b(a,P[i],(3.3,.34,2.5),'structural',None,'ThrusterBalanceLinearCalibrationFixture')
  for j in range(4):b(a,(x-1.3+j*.85,y+.23,z),(.38,.20,1.9),'titanium',pv,'ThrustVectorCalibrationFlexureMount')
 elif i in (48,49):vessel(a,P[i],.70,3.2,pv)
 elif i==50:
  for j in range(4):
   xx=x-.45+j*.30;valve(a,(xx,y,z),None)
  b(a,(x,y-.6,z),(1.75,.17,1.0),'structural',None,'ProtectedPressureFillSkid')
 elif i in (51,52,53,54,55):
  valve(a,P[i],pv)
  b(a,(x,y-.56,z),(1.0,.20,.9),'titanium',None,'PrecisionGasRegulatorMountingSkid')
  for j in range(3):pipe(a,[(x-.5+j*.15,y-.40,z-.22),(x-.5+j*.15,y+.1,z-.22)],.028,'copper')
 elif i in (56,57,58,59):
  frame(a,P[i],1.1,1.6,.8);valve(a,(x,y-.23,z+.20),pv)
  tor(a,(x,y+.47,z+.48),.19,.043,'cyan','z',None,'ActualMassFlowResponseTelemetry',30)
 elif i in (60,61):
  panel(a,P[i],'cyan','instrument',pv)
  for j in range(4):cy(a,(x-.35+j*.23,y-.55,z+.44),.043,.18,'gold','z',None,'XenonMassFlowSensorConnector',12)
 elif i in (62,63,64,65):
  frame(a,P[i],1.1,1.8,.83)
  b(a,(x,y,z+.33),(.95,1.45,.20),'graphite',None,'PowerProcessingUnitEnclosure')
  for j in range(12):
   yy=y-.45+j*.085
   b(a,(x-.21,yy,z+.46),(.42,.034,.14),'titanium',None,'PPUPowerConverterCoolingFin')
  b(a,(x+.29,y+.12,z+.49),(.22,.65,.06),'panel',None,'PPUPowerTelemetryScreen')
  for j in range(7):b(a,(x+.29,y-.22+j*.073,z+.54),(.15,.030,.024),'green' if j%2 else 'cyan',pv,'ActualPPUPowerControlIndicator')
  for j in range(6):cy(a,(x-.35+j*.14,y-.70,z+.48),.039,.075,'gold','z',None,'PPUHighIntegrityControlConnector',10)
 elif i==66:
  b(a,(x,y,z),(1.0,1.65,.74),'graphite',None,'ProtectedPropulsionDCBus')
  for j in range(7):b(a,(x-.28+j*.095,y,z+.43),(.060,1.16,.048),'copper',None,'PropulsionDCBusBusbar')
 elif i==67:
  b(a,P[i],(1.05,1.20,.65),'graphite',None,'PrechargeContactorVacuumHousing')
  b(a,(x,y,z+.43),(.51,.38,.19),'gold',pv,'ActualContactorArmature')
 elif i==76:
  frame(a,P[i],1.2,1.4,.86)
  tor(a,(x,y,z+.45),.34,.070,'structural','z',None,'CoolantPumpMotorFrame',38)
  cy(a,(x,y,z+.52),.17,.30,'cyan','z',pv,'MeasuredCoolantPumpRotor',28)
  for j in range(8):
   t=j*math.tau/8
   beam(a,(x,y,z+.56),(x+.29*math.cos(t),y+.29*math.sin(t),z+.56),.048,'titanium',pv,'CentrifugalCoolingImpellerBlade')
 elif i==78:
  cy(a,(x,y,z+.31),.23,.68,'structural','z',None,'OpticalPlumeSpectrometerSensorHead')
  cy(a,(x,y,z+.72),.15,.20,'glass','z',pv,'PlumeSpectrometerTrackingLens')
 elif i==79:
  beam(a,(x-.65,y,z),(x+.65,y,z),.07,'titanium',pv,'FaradayProbeCalibrationSlide')
  cy(a,(x+.59,y,z+.15),.21,.37,'structural','z',pv,'PlasmaProbeCollectorCup')
 elif i==81:
  b(a,P[i],(1.3,.40,.82),'structural',None,'ThrustCalibrationFlexureStage')
  tor(a,(x,y+.48,z),.41,.066,'gold','y',pv,'ActualThrustBalanceFlexure')
 elif i==85:
  cy(a,(x,y,z),.34,.60,'titanium','z',pv,'StarTrackerPrecisionGyroGimbal')
  cy(a,(x,y,z+.42),.26,.30,'glass','z',pv,'OpticalStarFieldTrackingLens')
 elif i==91:
  tor(a,P[i],2.0,.045,'violet','y',pv,'PlasmaFieldVectorHologramRing',68)
  for j in range(15):
   t=j*math.tau/15
   beam(a,(x,y,z),(x+1.8*math.cos(t),y+.7*math.sin(t),z+1.4*math.sin(t)),.024,'cyan',pv,'ConceptualIonStreamVectorLine')
 else:
  theme='red' if i in (87,88,89) else 'cyan'
  panel(a,P[i],theme,'fault' if i in (87,88,89) else 'plc' if i in (72,74,75,82,83,84) else 'instrument',pv)
 # Every ANIM parent must have a mesh-owning descendant. Indicator parents still only drive visual elements.
 owned={pp for _,_,_,pp,_ in a.raw if pp}
 for j,pid in enumerate(PV_BY_MODEL[i]):
  if pid not in owned:
   px,py,pz=PIVOT_POS[pid]
   b(a,(px,py+.44+j*.055,pz+.46),(.19,.052,.045),'cyan' if 'STATUS' in pid or 'DISPLAY' in pid else 'gold',pid,'VisualRigMeshOwnedIndicator')
 return a
# Exact wire identities are authored as CONCEPT_ONLY, not player cables or real Unity SparkTerminals.
GROUP_CONFIG=[
('Protected 5 V logic supply and returns',12,'POWER_CONTROL',[
(75,74,'Protected 5 V propulsion safety PLC supply'),(75,72,'Plasma ignition sequencing logic supply'),
(75,84,'Mission departure sequencing logic supply'),(75,82,'Thrust allocation controller logic supply'),
(75,83,'Deep-space guidance logic supply'),(75,90,'Propulsion SCADA logic supply'),
(75,73,'Neutralizer transducer 5 V measurement conditioning'),(75,60,'Xenon sensor array logic supply'),
(75,70,'Magnetic current control logic supply'),(75,56,'Xenon A mass flow logic power'),
(74,75,'Protected safety logic circuit return'),(72,75,'Ignition controller protected circuit return')]),
('Xenon propellant storage and flow regulation',26,'ANALOG_FEEDBACK',[
(48,61,'Xenon tank A available propellant inventory'),(49,61,'Xenon tank B available propellant inventory'),
(48,53,'Tank A regulated inlet supply measured pressure'),(49,54,'Tank B regulated inlet supply measured pressure'),
(61,83,'Qualified remaining xenon fuel budget'),(50,74,'Fill port closure verified by safety supervisor'),
(84,51,'Tank A isolate valve request','DIGITAL_CONTROL'),(84,52,'Tank B isolate valve request','DIGITAL_CONTROL'),
(51,55,'Tank A isolation valve actual open feedback'),(52,55,'Tank B isolation valve actual open feedback'),
(53,55,'Regulator A measured supply pressure'),(54,55,'Regulator B measured supply pressure'),
(55,56,'Thruster A actual xenon supply pressure'),(55,57,'Thruster B actual xenon supply pressure'),
(55,58,'Thruster C actual xenon supply pressure'),(55,59,'Thruster D actual xenon supply pressure'),
(84,56,'Thruster A mass-flow request','DIGITAL_CONTROL'),(84,57,'Thruster B mass-flow request','DIGITAL_CONTROL'),
(84,58,'Thruster C mass-flow request','DIGITAL_CONTROL'),(84,59,'Thruster D mass-flow request','DIGITAL_CONTROL'),
(56,60,'Actual A xenon mass-flow feedback'),(57,60,'Actual B xenon mass-flow feedback'),
(58,60,'Actual C xenon mass-flow feedback'),(59,60,'Actual D xenon mass-flow feedback'),
(60,74,'Qualified mass-flow feedback safety cross-check'),(87,61,'Injected xenon supply restriction diagnostic')]),
('Four power-processing units and DC bus',26,'DIGITAL_CONTROL',[
(74,67,'Protected precharge permission'),(67,66,'Actual PPU DC bus precharge connection'),
(66,62,'Protected DC bus PPU A enable'),(66,63,'Protected DC bus PPU B enable'),
(66,64,'Protected DC bus PPU C enable'),(66,65,'Protected DC bus PPU D enable'),
(68,74,'Bus current trusted supervisory measurement','ANALOG_FEEDBACK'),
(69,74,'Bus voltage measured supervisory feedback','ANALOG_FEEDBACK'),
(84,62,'PPU A mode enable'),(84,63,'PPU B mode enable'),
(84,64,'PPU C mode enable'),(84,65,'PPU D mode enable'),
(62,72,'PPU A discharge ready confirmation','ANALOG_FEEDBACK'),
(63,72,'PPU B discharge ready confirmation','ANALOG_FEEDBACK'),
(64,72,'PPU C discharge ready confirmation','ANALOG_FEEDBACK'),
(65,72,'PPU D discharge ready confirmation','ANALOG_FEEDBACK'),
(62,90,'PPU A actual electrical power telemetry','ANALOG_FEEDBACK'),
(63,90,'PPU B actual electrical power telemetry','ANALOG_FEEDBACK'),
(64,90,'PPU C actual electrical power telemetry','ANALOG_FEEDBACK'),
(65,90,'PPU D actual electrical power telemetry','ANALOG_FEEDBACK'),
(74,62,'PPU A independent safety shutdown override','SAFETY_FEEDBACK'),
(74,63,'PPU B independent safety shutdown override','SAFETY_FEEDBACK'),
(74,64,'PPU C independent safety shutdown override','SAFETY_FEEDBACK'),
(74,65,'PPU D independent safety shutdown override','SAFETY_FEEDBACK'),
(67,74,'Measured main precharge contactor position','ANALOG_FEEDBACK'),
(66,90,'DC bus state SCADA telemetry','ANALOG_FEEDBACK')]),
('Plasma ignition, discharge and neutralization',24,'DIGITAL_CONTROL',[
(72,32,'Thruster A protected ignition request'),(72,33,'Thruster B protected ignition request'),
(72,34,'Thruster C protected ignition request'),(72,35,'Thruster D protected ignition request'),
(72,42,'Cathode A activation request'),(72,43,'Cathode B activation request'),
(72,44,'Cathode C activation request'),(72,45,'Cathode D activation request'),
(42,72,'Cathode A measured keeper current','ANALOG_FEEDBACK'),
(43,72,'Cathode B measured keeper current','ANALOG_FEEDBACK'),
(44,72,'Cathode C measured keeper current','ANALOG_FEEDBACK'),
(45,72,'Cathode D measured keeper current','ANALOG_FEEDBACK'),
(32,80,'Engine A discharge ripple measured diagnostic','ANALOG_FEEDBACK'),
(33,80,'Engine B discharge ripple measured diagnostic','ANALOG_FEEDBACK'),
(34,80,'Engine C discharge ripple measured diagnostic','ANALOG_FEEDBACK'),
(35,80,'Engine D discharge ripple measured diagnostic','ANALOG_FEEDBACK'),
(37,32,'Shared anode A measured xenon distribution','ANALOG_FEEDBACK'),
(37,33,'Shared anode B measured xenon distribution','ANALOG_FEEDBACK'),
(37,34,'Shared anode C measured xenon distribution','ANALOG_FEEDBACK'),
(37,35,'Shared anode D measured xenon distribution','ANALOG_FEEDBACK'),
(71,72,'Cathode keeper supply available confirmation','SAFETY_FEEDBACK'),
(73,90,'Display only neutralizer current telemetry','ANALOG_FEEDBACK'),
(72,90,'Ignition state telemetry visualization','ANALOG_FEEDBACK'),
(74,72,'Independent ignition permit and stop gate','SAFETY_FEEDBACK')]),
('Electromagnets and thermal management',22,'ANALOG_FEEDBACK',[
(70,38,'Magnetic coil A commanded current','DIGITAL_CONTROL'),
(70,39,'Magnetic coil B commanded current','DIGITAL_CONTROL'),
(70,40,'Magnetic coil C commanded current','DIGITAL_CONTROL'),
(70,41,'Magnetic coil D commanded current','DIGITAL_CONTROL'),
(38,70,'Actual magnet coil A current'),(39,70,'Actual magnet coil B current'),
(40,70,'Actual magnet coil C current'),(41,70,'Actual magnet coil D current'),
(88,70,'Magnet driver overheat disturbance diagnostic'),
(70,74,'Coil current safe operating limits'),
(32,76,'Engine A temperature feedback'),(33,76,'Engine B temperature feedback'),
(34,76,'Engine C temperature feedback'),(35,76,'Engine D temperature feedback'),
(76,77,'Thermal coolant transfer pump output','DIGITAL_CONTROL'),
(77,74,'Radiator temperature and heat rejection safety'),
(76,90,'Real coolant pump temperature telemetry'),(77,90,'Thermal heat rejection display only'),
(84,76,'Thermal system pump start','DIGITAL_CONTROL'),
(74,70,'Magnetic current safety override','SAFETY_FEEDBACK'),
(88,74,'Magnet overtemperature independent fault state','SAFETY_FEEDBACK'),
(80,90,'Discharge instability diagnostics')]),
('Thrust vectoring, navigation and mission guidance',24,'DIGITAL_CONTROL',[
(82,46,'Quad engine gimbal demand'),(82,32,'Thrust setpoint A'),
(82,33,'Thrust setpoint B'),(82,34,'Thrust setpoint C'),(82,35,'Thrust setpoint D'),
(46,86,'Measured gimbal angles and actuator positions','ANALOG_FEEDBACK'),
(86,82,'Actual thrust vector alignment telemetry','ANALOG_FEEDBACK'),
(81,82,'Measured combined thrust validation','ANALOG_FEEDBACK'),
(47,81,'Thrust balance fixture calibration','ANALOG_FEEDBACK'),
(78,82,'Optical plume temperature and spectral status','ANALOG_FEEDBACK'),
(79,82,'Plume Faraday diagnostic current','ANALOG_FEEDBACK'),
(85,83,'Star tracker measured inertial orientation','ANALOG_FEEDBACK'),
(28,83,'Attitude suite measured flight state','ANALOG_FEEDBACK'),
(83,82,'Trajectory command desired thrust vector'),
(82,83,'Actual integrated propulsion response','ANALOG_FEEDBACK'),
(83,84,'Departure burn profile and mission checkpoints'),
(84,10,'Undock station hardpoint release request'),
(10,84,'Confirmed actual spacecraft mechanical release','SAFETY_FEEDBACK'),
(89,86,'Thruster gimbal offset diagnostic','ANALOG_FEEDBACK'),
(84,85,'Star tracker calibration command'),
(85,90,'Actual measured star tracker confidence','ANALOG_FEEDBACK'),
(83,90,'Accumulated deltaV and trajectory error','ANALOG_FEEDBACK'),
(84,90,'Departure mission progression status','ANALOG_FEEDBACK'),
(74,84,'Thrust permission and safe-departure permit','SAFETY_FEEDBACK')]),
('Independent propulsion safety and critical feedback',26,'SAFETY_FEEDBACK',[
(74,84,'Propulsion safety permit for departure'),
(74,82,'Safety inhibit to thrust allocator'),
(72,74,'Actual igniter inactive confirmation'),
(71,74,'Cathode keeper power disabled confirmation'),
(68,74,'Independent current limit comparator'),
(69,74,'Independent voltage protection comparator'),
(10,74,'Station docking release interlock actual state'),
(11,74,'Propulsion test chamber protected gate interlock'),
(76,74,'Independent thermal pump running feedback'),
(48,74,'Tank A reserve safety status'),
(49,74,'Tank B reserve safety status'),
(50,74,'Xenon fill valve closed and secured status'),
(60,74,'Xenon flow independent sensor validity'),
(32,74,'Thruster A actual discharge state feedback'),
(33,74,'Thruster B actual discharge state feedback'),
(34,74,'Thruster C actual discharge state feedback'),
(35,74,'Thruster D actual discharge state feedback'),
(80,74,'Discharge oscillation overlimit protection'),
(87,74,'Xenon restricted fault safety input'),
(88,74,'Coil driver overtemperature safety input'),
(89,74,'Gimbal misalignment fault safety input'),
(74,92,'Critical feedback cable health report'),
(73,74,'CRITICAL trusted measured neutralizer current authorization','ANALOG_FEEDBACK'), # W157
(92,84,'Operator explicit fault-reset request','DIGITAL_CONTROL'),
(74,72,'Independent discharge shutdown control'),
(74,90,'Actual safety latch state telemetry','ANALOG_FEEDBACK')]),
('SCADA diagnostics, event logging and certification',16,'ANALOG_FEEDBACK',[
(60,90,'Xenon pressure and mass flow trending'),
(61,90,'Propellant reserve and consumption trending'),
(66,90,'PPU bus power distribution chart'),
(70,90,'Magnetic coil measured current telemetry'),
(73,90,'Neutralizer current diagnostic telemetry only'),
(78,90,'Measured plasma spectral emissions trend'),
(79,90,'Measured plume diagnostic current trend'),
(80,90,'Discharge high-speed oscillation record'),
(81,90,'Thrust balance measured force'),
(82,90,'Engine thrust allocation status'),
(83,90,'Guidance trajectory progress'),
(84,90,'Sequencer state and safety status'),
(86,90,'Actual gimbal alignment margins'),
(87,93,'Propellant flow fault event record'),
(88,93,'Thermal disturbance event record'),
(93,90,'Authoritative objective evidence timeline')])]
assert [len(g[3]) for g in GROUP_CONFIG]==[12,26,26,24,22,24,26,16],[(g[0],len(g[3])) for g in GROUP_CONFIG]
WIRES=[];GROUPS=[]
for group,num,kind,defs in GROUP_CONFIG:
 start=len(WIRES)+1
 for entry in defs:
  src,dst,purpose=entry[:3];sig=entry[3] if len(entry)>3 else kind
  n=len(WIRES)+1
  WIRES.append(dict(WireID=f'W{n:03d}',Group=group,SourceModel=NAMES[src],TargetModel=NAMES[dst],
   SignalKind=sig,Purpose=purpose,Critical=('YES' if n==157 else 'NO'),
   ConceptStatus='CONCEPT_ONLY_NO_PLAYER_WIRE',SourceVisualAnchor='',TargetVisualAnchor=''))
 GROUPS.append(dict(Group=group,First=f'W{start:03d}',Last=f'W{len(WIRES):03d}',Count=num))
assert len(WIRES)==176
assert WIRES[156]['SourceModel']==NAMES[73] and WIRES[156]['TargetModel']==NAMES[74],WIRES[156]
assert [w['WireID'] for w in WIRES]==[f'W{i:03d}' for i in range(1,177)]
# Exactly 254 original modeled visual sockets (art-only). 3 per initial 66 and 2 remaining.
ANCHORS=[];BY_MODEL=defaultdict(list)
for i in range(94):
 x,y,z=P[i];count=3 if i<66 else 2
 # Key visual art sockets on the physical item. World locations authored in metres.
 for j in range(count):
  aid=f'P{len(ANCHORS)+1:03d}'
  # smaller instruments receive side-by-side sockets; structural models still have service-test anchors.
  if i in (0,1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,24,25,26,27):
   loc=(x+(-.31+j*.31),min(10.2,max(1.4,y)),z+1.03)
  elif i in (32,33,34,35):loc=(x+(-.25+j*.25),y-.55,z-.45)
  elif i in (48,49):loc=(x+(-.29+j*.29),1.62,z+.73)
  else:loc=(x+(-.28+j*.28),y-.45,z+.48)
  row=dict(AnchorID=aid,Model=NAMES[i],WorldX_m=round(float(loc[0]),5),WorldY_m=round(float(loc[1]),5),WorldZ_m=round(float(loc[2]),5),
    Status='ART_ANCHOR_NOT_SPARKTERMINAL')
  ANCHORS.append(row);BY_MODEL[i].append(row)
assert len(ANCHORS)==254
# Explicit physical art anchor reference distinct from independently allocated functional wire endpoints.
LOGICAL=[];usecounter=Counter()
for w in WIRES:
 for side in ('Source','Target'):
  idx=NAMES.index(w[side+'Model']);n=usecounter[idx];usecounter[idx]+=1
  aa=BY_MODEL[idx][(int(w['WireID'][1:])+n*5)%len(BY_MODEL[idx])]
  w[side+'VisualAnchor']=aa['AnchorID']
  port=f"{w['WireID']}_{'OUT' if side=='Source' else 'IN'}_{w['SignalKind']}"
  if w['WireID']=='W157':port='TRUSTED_NEUTRALIZER_CURRENT_OUT' if side=='Source' else 'TRUSTED_NEUTRALIZER_CURRENT_IN'
  LOGICAL.append(dict(WireID=w['WireID'],Endpoint=side,Model=w[side+'Model'],ArtAnchor=aa['AnchorID'],
    FunctionalPortID=port,SignalKind=w['SignalKind'],Direction='OUTPUT' if side=='Source' else 'INPUT',
    Critical=w['Critical'],FunctionalStatus='PLANNED_NOT_ACTUAL_SPARKTERMINAL'))
assert len(LOGICAL)==352
shared=defaultdict(list)
for row in LOGICAL:shared[row['ArtAnchor']].append(row)
CONFLICTS=[dict(AnchorID=k,Model=v[0]['Model'],LogicalContactCount=len(v),SignalKinds=';'.join(sorted({x['SignalKind'] for x in v})),
 MixedSignalKinds=len({x['SignalKind'] for x in v})>1,Requirement='SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_CROSS_DOMAIN_SHORT')
 for k,v in shared.items() if len(v)>1]
OBJ=[
('Build the Deep-Space Propulsion Network','176 actual player-created connections; reconciled functional terminals, protected 5 V and proper safety including W157','Live topology hash, circuit solver and independent protection evidence'),
('Commission the Xenon and Power Systems','Real sensor calibration, finite xenon tanks, valve and flow response, four PPUs, guarded precharge, independent interlocks','Calibrated flow and measured electrical state with timestamps'),
('Ignite and Stabilize the Plasma Engines','Four Hall thrusters demonstrate discharge and neutralization in protected fictional test environment, measurable bounded thrust and thermal response','Four independent engine ignition, current, thrust and heat records'),
('Execute the First Deep-Space Departure','Actual safe mechanical release, plume clearance and integrated low thrust changes trajectory under energy/fuel constraints','New run ID, finite propellant and electrical energy balance, actual deltaV checkpoints'),
('Recover Three Propulsion-System Failures','Real xenon flow restriction, magnetic coil overheating and thrust vector offset; diagnosis/repair, then fresh full propulsion and departure mission','Causal plant changes, diagnostic and repair events and new run ID'),
('Restore Critical Plasma Neutralization Feedback','Historical objectives 1–5, deliberate real W157 disconnect, neutralizer safety inhibit, new sample after reconnect, explicit reset and fresh entire departure run','Physical W157 topology, safety latch and post-repair run-scoped final evidence')]

def csvout(filename,rows):
 with (ROOT/filename).open('w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def jout(filename,data):(ROOT/filename).write_text(json.dumps(data,indent=2),encoding='utf8')
def metadata():
 tables=[
 ('Gameplay_Wiring_Concept_W001_W176.csv',WIRES),('Gameplay_Wiring_Budget.csv',GROUPS),
 ('Terminal_Anchor_Plan_254.csv',ANCHORS),('Visual_Terminal_Sockets_254.csv',ANCHORS),
 ('Terminal_Logical_Port_Reconciliation_352.csv',LOGICAL),('Terminal_Anchor_Signal_Conflict_Report.csv',CONFLICTS),
 ('Objective_Certification_Plan_6.csv',[dict(ObjectiveIndex=j+1,Title=t,AcceptanceCriteria=ac,Evidence=ev,Implemented='NO') for j,(t,ac,ev) in enumerate(OBJ)]),
 ('Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PIVOTS]),
 ('Component_Roles.csv',[dict(Index=i,Model=NAMES[i],WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],
   Role=('Orbital commissioning environment' if i<16 else 'AURORA-1 spacecraft structure' if i<32 else 'Hall-effect plasma thrust engines' if i<48 else 'Xenon propellant flow and storage' if i<62 else 'PPU power, safety and thermal' if i<78 else 'Mission diagnostics, guidance, SCADA and failures'),State='ART_ONLY') for i in range(94)]),
 ('Art_Assembly.csv',[dict(Index=i,Model=NAMES[i],File=f'{i:02d}_{NAMES[i]}.fbx',TransformOrigin='0,0,0',ExpectedWorldCenter_m=','.join(map(str,P[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres') for i in range(94)])]
 for filename,rows in tables:csvout(filename,rows)
 w=WIRES[156]
 (ROOT/'W157_Critical_Neutralizer_Current_Recovery.md').write_text(f'''# Project Spark Level 90: W157 critical neutralizer feedback recovery

**Source-authored concept route:** `{w['SourceModel']}` (`{w['SourceVisualAnchor']}`) → `{w['TargetModel']}` (`{w['TargetVisualAnchor']}`), `ANALOG_FEEDBACK`, `Critical=YES`.
Proposed functional contacts `TRUSTED_NEUTRALIZER_CURRENT_OUT` → `TRUSTED_NEUTRALIZER_CURRENT_IN`.

1. Require Objectives 1–5 to have historical certificates, not decorative UI badges.
2. Run a supervised safe test in fictional game simulation; player physically disconnects live W157. This ZIP contains **no actual SparkTerminal/wire objects**.
3. Validity and timestamp of *trusted* neutralizer current are invalidated immediately. **Do not treat missing feedback as zero current** or assume the actual ion discharge ceases instantly.
4. Withdraw normal discharge/ignition authorization and command safe bounded shutdown of the relevant engines. Observe actual current/power decay in the separate educational plasma plant. Maintain independent electrical and thermal protection.
5. Preserve real tank inventory, spacecraft velocity, pending maneuver and historical Objectives 1–5. An uncontrolled thrust/propellant state is reported honestly rather than silently normalized.
6. Explicitly prevent W157 replacement by display-only telemetry, cached SCADA values, or a separate status indicator.
7. Player reconnects the correct functional wire; validate electrical graph, measurement power/return, polarity, range, calibration, timestamp, safety and independent shutdown confirmation.
8. Require explicit operator safety reset; new ignition cannot start until all safety interlocks pass.
9. Start a fresh post-repair test run ID. Recommission all four thrusters, execute a complete actual safe departure/low-thrust profile and validate propellant, power, thermal and trajectory constraints.
10. Only the authoritative `LevelGamePlayManager` may record Objective 6 and real **6/6**, never on reconnection or a timer alone.

Fictional educational game simulation, **not** spacecraft propulsion qualification or safe real-world plasma setup instructions.
''',encoding='utf8')

def with_sockets(i):
 art=make_art(i)
 for row in BY_MODEL[i]:art.socket((row['WorldX_m'],row['WorldY_m'],row['WorldZ_m']),row['AnchorID'])
 return art

def homes(i,a):
 return [(p['Pivot'],a.name,PIVOT_POS[p['Pivot']]) for p in PIVOTS if p['Index']==i]+[
   ('SOCKET_'+aid,a.name,pos) for aid,pos in a.sockets]

def export_models():
 start=time.monotonic();models=[];allpieces=[];allhomes=[];lower=np.ones(3)*np.inf;upper=-lower
 for i,n in enumerate(NAMES):
  a=with_sockets(i);parts=core.chunk_model(a,max_shapes=19);h=homes(i,a)
  ff=ROOT/'Models'/f'{i:02d}_{n}.fbx';gg=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  f=core.export_fbx(str(ff),[(n,p) for p in parts],[n],h,a.sockets)
  g=core.export_glb(str(gg),[(n,p) for p in parts],[n],h)
  sock=dict(a.sockets)
  for part in parts:
   pv=part['pivot'];offs=PIVOT_POS[pv] if pv in PIVOT_POS else sock[pv[7:]] if pv and pv.startswith('SOCKET_') else (0,0,0)
   vv=part['vertices']+np.asarray(offs)
   lower=np.minimum(lower,vv.min(axis=0));upper=np.maximum(upper,vv.max(axis=0))
  models.append(dict(index=i,name=n,file=ff.name,glb=gg.name,triangle_count=sum(len(q['faces']) for q in parts),
   mesh_parts=len(parts),source_primitive_count=len(a.raw),visual_socket_count=len(a.sockets),
   animation_pivots={pid:xyz for pid,_,xyz in h if pid.startswith('ANIM_')},fbx_bytes=f['file_size'],glb_bytes=g['glb_bytes']))
  allpieces.extend((n,p) for p in parts);allhomes.extend(h)
  if i in (0,16,19,20,32,35,48,49,62,65,73,74,90,91,92,93) or (i+1)%20==0:
   print('EXPORTED',i+1,'/94',n,'TRIS',models[-1]['triangle_count'],'SEC',round(time.monotonic()-start,1),flush=True)
 print('SHOWCASE EXPORT',len(allpieces),'chunks',flush=True)
 fpath=ROOT/'Models'/'Level90_Complete_ArtKit_Showcase.fbx'
 gpath=ROOT/'GLB'/'Level90_Complete_ArtKit_Showcase.glb'
 f=core.export_fbx(str(fpath),allpieces,NAMES,allhomes,[(q['AnchorID'],(q['WorldX_m'],q['WorldY_m'],q['WorldZ_m'])) for q in ANCHORS]);g=core.export_glb(str(gpath),allpieces,NAMES,allhomes)
 manifest=dict(level_id='level_90',display_name='The Long Burn',subtitle='Quad Hall-Effect Propulsion and Deep-Space Departure',
  individual_models=len(models),individual_glbs=len(models),assembled_fbx='Models/'+fpath.name,assembled_glb='GLB/'+gpath.name,
  individual_triangles=sum(m['triangle_count'] for m in models),individual_mesh_parts=sum(m['mesh_parts'] for m in models),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),
  proposed_independent_logical_wire_endpoints=len(LOGICAL),anchor_reconciliation_conflicts=len(CONFLICTS),
  mixed_signal_anchor_conflicts=sum(bool(c['MixedSignalKinds']) for c in CONFLICTS),
  workstation_world_bounds_m=[lower.round(4).tolist(),upper.round(4).tolist()],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up X-width Z-forward',assembly_origin=[0,0,0],
  actual_player_wires_included=False,functional_sparkterminals_included=False,real_hall_plasma_solver_included=False,
  real_propellant_dynamics_included=False,runtime_safety_plc_included=False,six_runtime_objectives_implemented=False,
  unity_compilation_tested=False,unity_playmode_tested=False,assembled_fbx_bytes=f['file_size'],assembled_glb_bytes=g['glb_bytes'],models=models)
 jout('Model_Manifest.json',manifest)
 print('EXPORTED KIT',manifest['individual_triangles'],'triangles',len(allpieces),'chunks',round(time.monotonic()-start,1),'sec',flush=True)
 return manifest
if __name__=='__main__':
 print('PROJECT SPARK LEVEL 90 - REAL FBX GLB BUILD',flush=True)
 metadata();export_models()
