#!/usr/bin/env python3
"""Project Spark Level 88 — THE BREATHING RING.
Author original editable, procedural binary FBX7400/glTF GLB modular meshes and
manifest-driven ART CONCEPT files. No functional circuits/simulation are built.
All internal coordinates are metres; FBX geometry and pivots use centimetres.
"""
from pathlib import Path
from collections import defaultdict,Counter
import csv,json,math,random,time
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for f in ['Models','GLB','Textures','Reports','Unity_Optional']:(ROOT/f).mkdir(exist_ok=True,parents=True)
NAMES=['L88_'+x for x in (Path(__file__).parent/'model_names.txt').read_text().splitlines() if x.strip()]
assert len(NAMES)==90
random.seed(88088)
core.COLORS={
 'structural':(.35,.43,.52,1),'titanium':(.66,.75,.82,1),'dark':(.045,.065,.09,1),
 'graphite':(.08,.12,.17,1),'copper':(.89,.47,.17,1),'gold':(.97,.65,.20,1),
 'ceramic':(.89,.93,.91,1),'glass':(.20,.65,.84,.37),'panel':(.012,.11,.20,1),
 'pcb':(.025,.25,.20,1),'cyan':(.07,.82,.98,1),'blue':(.17,.43,.93,1),
 'green':(.08,.90,.49,1),'amber':(1,.60,.12,1),'violet':(.63,.39,.99,1),
 'red':(.99,.14,.12,1),'orange':(1,.38,.09,1),'white':(.97,.97,.91,1),
 'water':(.11,.54,.93,.57),'adsorbent':(.15,.68,.89,1),'foliage':(.10,.52,.17,1),
 'foliage2':(.17,.64,.24,1),'floor':(.23,.30,.39,1),'stars':(.85,.90,1,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
# All modular art originates at (0,0,0) when imported; P entries are world design centres.
P={0:(0,-.10,0),1:(0,6.5,-11.5),2:(0,8.2,-9.8),3:(-6.5,7.9,-9.4),4:(-2,7.9,-9.4),5:(2.5,7.9,-9.4),6:(7,7.9,-9.4),7:(0,11.6,-5.7),8:(0,5.0,-7.3),9:(-11.4,4.5,-7.6),10:(0,.10,0),11:(0,6.0,-3.0),
   12:(-11.0,3.2,-.7),13:(-8.35,3.2,-.7),14:(-11,1.85,1.3),15:(-8.35,1.85,1.3),16:(-11,2.7,-.5),17:(-8.35,2.7,-.5),18:(-11,1.7,-2),19:(-8.35,1.7,-2),20:(-9.7,2.0,2.0),
   24:(-10.7,1.45,3.8),25:(-8.9,1.7,3.8),28:(-4.0,2.95,-1.15),29:(-5.25,2.7,-.6),30:(-2.75,2.7,-.6),31:(-1.65,2.0,1),32:(-5.5,1.75,1.7),33:(-.2,2.65,-1),38:(-2.2,1.35,3.1),39:(-4.3,1.1,3.2),40:(-.2,1.7,1.6),41:(1.4,2.3,1.1),44:(4.0,2.2,-1.3),45:(4,2.8,-1.3),46:(5.9,1.5,1.4),47:(5.3,1.6,3.1),48:(6.9,1.4,3.15),54:(4,1.65,3.3),56:(9.0,2.1,-1),57:(11.1,2.1,-1),60:(10,4.25,-3.0),61:(10,3.6,1.5),69:(9.7,1.5,3.6),70:(11.3,1.5,3.6),72:(11.8,2.5,2.1),75:(13.2,2.0,1),
   82:(0,7.4,-5.75),83:(4.3,7.8,-5.8),88:(0,11.75,-5.8),89:(12.2,5.7,-2.3)}
# Additional instruments arranged in accessible front/side rows.
for jj,i in enumerate([21,22,23,26,27,34,35,36,37,42,43]):P[i]=(-12.6+jj*1.32,1.65,5.5)
for jj,i in enumerate([49,50,51,52,53,55,58,59,62,63,64,65,66,67,68]):P[i]=(-12.5+jj*1.62,1.65,7.65)
for jj,i in enumerate([71,73,74,76,77,78,79,80,81,84,85,86,87]):P[i]=(-11.4+jj*1.83,1.8,9.7)
assert set(P)==set(range(90)),set(range(90))-set(P)
# 72 unique mesh-owning pivot groups. Indicator pivots stay fixed in animation.
PIVOT_RAW=[
 (2,'HABITAT_HOLOGRAM_RING','indicator','none'),(3,'HABITAT_A_STATUS','indicator','none'),(4,'HABITAT_B_STATUS','indicator','none'),(5,'HABITAT_C_STATUS','indicator','none'),(6,'HABITAT_D_STATUS','indicator','none'),
 (7,'COMPARTMENT_AIR_QUALITY','indicator','none'),(9,'AIRLOCK_DOOR','translation','x'),(11,'GAS_FLOW_HEADER','indicator','none'),
 (12,'ADSORBER_A_CYCLE','rotation','z'),(13,'ADSORBER_B_CYCLE','rotation','z'),(14,'ADSORBER_A_INLET_VALVE','rotation','y'),(15,'ADSORBER_B_INLET_VALVE','rotation','y'),
 (16,'ADSORBENT_A_COLUMN_SCAN','translation','y'),(17,'ADSORBENT_B_COLUMN_SCAN','translation','y'),(18,'ADSORBER_A_HEATER','indicator','none'),(19,'ADSORBER_B_HEATER','indicator','none'),
 (20,'ADSORBER_SWITCHING_MANIFOLD','rotation','y'),(21,'CO2_INLET_INSTRUMENT','indicator','none'),(22,'CO2_OUTLET_INSTRUMENT','indicator','none'),(23,'CO2_CAPACITY','indicator','none'),
 (24,'REGEN_EXHAUST_CONDENSER','rotation','x'),(25,'CO2_CANISTER_VALVE','rotation','y'),(26,'CO2_BREAKTHROUGH_FAULT','translation','x'),(27,'ADSORBER_CONTROL_STATUS','indicator','none'),
 (28,'ELECTROLYSIS_BUBBLE_COLUMN','indicator','none'),(29,'O2_CELL_A_SCAN','translation','y'),(30,'O2_CELL_B_SCAN','translation','y'),(31,'O2_SEPARATOR','rotation','y'),
 (32,'BYPRODUCT_ISOLATION','rotation','y'),(33,'O2_BUFFER_LEVEL','indicator','none'),(34,'O2_FLOW_CONTROL','rotation','z'),(35,'O2_PARTIAL_PRESSURE','indicator','none'),
 (36,'O2_PURITY_MONITOR','indicator','none'),(37,'O2_POWER_CONVERTER','indicator','none'),(38,'O2_COOLING_PUMP','rotation','z'),(39,'WATER_FEED_PUMP','rotation','z'),
 (40,'O2_DELIVERY_REGULATOR','rotation','y'),(41,'O2_BACKUP_ISOLATION','rotation','y'),(42,'CELL_MONITOR','indicator','none'),(43,'O2_PROCESS_PLC','indicator','none'),
 (44,'CONDENSER_PROCESS','indicator','none'),(45,'CONDENSER_COOLANT','indicator','none'),(46,'CONDENSATE_SEPARATOR','rotation','y'),(47,'CONDENSATE_TANK_LEVEL','indicator','none'),
 (48,'RECOVERED_WATER_PUMP','rotation','z'),(49,'WATER_QUALITY','indicator','none'),(50,'FREEZE_FAULT','translation','x'),(51,'HUMIDITY_PLC','indicator','none'),
 (52,'HUMIDITY_SENSOR','indicator','none'),(53,'COOLING_VALVE','rotation','y'),(54,'WATER_FILTER_DRAWER','translation','x'),(55,'WATER_BUDGET','indicator','none'),
 (56,'PRIMARY_BLOWER_ROTOR','rotation','z'),(57,'BACKUP_BLOWER_ROTOR','rotation','z'),(58,'FAN_A_FREQUENCY','indicator','none'),(59,'FAN_B_FREQUENCY','indicator','none'),
 (60,'MAIN_DUCT_AIR_FLOW','indicator','none'),(61,'DAMPER_A','rotation','z'),(61,'DAMPER_B','rotation','z'),(61,'DAMPER_C','rotation','z'),(61,'DAMPER_D','rotation','z'),
 (62,'ZONE_A_PRESSURE','indicator','none'),(63,'ZONE_B_PRESSURE','indicator','none'),(64,'ZONE_C_PRESSURE','indicator','none'),(65,'ZONE_D_PRESSURE','indicator','none'),
 (66,'TRUSTED_PRESSURE_MONITOR','indicator','none'),(67,'INDEPENDENT_PRESSURE','indicator','none'),(69,'LEAK_ISOLATION','rotation','y'),(70,'PRESSURE_REGULATOR','rotation','y'),
 (71,'MICROLEAK_FAULT','translation','x'),(72,'FILTER_EXTRACT','translation','x'),(73,'FLOW_MEASUREMENT','indicator','none'),(75,'PRESSURE_RELIEF','translation','y'),
 (77,'LIFE_SUPPORT_SAFETY','indicator','none'),(82,'MASTER_ECLSS_SCADA','indicator','none'),(83,'ATMOS_FLOW_HOLOGRAM','indicator','none'),(86,'W145_TRUSTED_PATCH','indicator','none'),(87,'EMERGENCY_ISOLATION','translation','z'),(88,'FINAL_CERTIFICATION','indicator','none')]
PIVOT_RAW=[p for p in PIVOT_RAW if p[0] not in (3,4,5,6,21,22,23)]
assert len(PIVOT_RAW)==72,len(PIVOT_RAW)
PIVOTS=[dict(Pivot='ANIM_'+name,Index=i,Model=NAMES[i],Kind=kind,Axis=ax) for i,name,kind,ax in PIVOT_RAW]
# Special simultaneous parts such as damper blades share one owner model but different origins.
PIVOT_POS={p['Pivot']:(P[p['Index']][0],P[p['Index']][1],P[p['Index']][2]) for p in PIVOTS}
for j,tag in enumerate(('A','B','C','D')):PIVOT_POS['ANIM_DAMPER_'+tag]=(P[61][0]+(j-1.5)*.58,P[61][1],P[61][2]+.25)
core.PIVOT_POS=PIVOT_POS
PV_BY_MODEL=defaultdict(list)
for p in PIVOTS:PV_BY_MODEL[p['Index']].append(p['Pivot'])

def b(a,p,s,m='titanium',pv=None,label='PrecisionMachinedPanel'):a.box(p,s,m,pv,label)
def cy(a,p,r,h,m='titanium',axis='z',pv=None,label='PrecisionCylinder',seg=20):a.cylinder(p,r,h,m,axis=axis,seg=seg,pivot=pv,label=label)
def tor(a,p,R,r,m='titanium',axis='z',pv=None,label='InstrumentedRing',seg=42,tube=8):a.torus(p,R,r,m,axis=axis,seg=seg,tube=tube,pivot=pv,label=label)
def beam(a,p,q,r=.046,m='structural',pv=None,label='PlumbedTube'):a.beam(p,q,r,m,pivot=pv,label=label,sides=9)
def sphere(a,p,r,m='green',pv=None,label='RoundedElement',subs=1):
 g=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(g.vertices+np.asarray(p),g.faces,m,pv,label)
def panel(a,p,scale=1.0,theme='cyan',special='plc',pv=None):
 core.panel_equipment(a,p,scale,theme,special)
 x,y,z=p
 for j in range(9):
  cy(a,(x-.42+j*.105*scale,y-.50*scale,z+.39*scale),.027*scale,.10*scale,'gold','z',None,'GoldRimFieldTerminal',10)
 for j in range(4): b(a,(x-.39+j*.25*scale,y+.56*scale,z+.16*scale),(.09*scale,.055*scale,.06*scale),'structural',None,'CabinetVentilationRib')
 if pv: b(a,(x,y+.42*scale,z+.42*scale),(.22*scale,.04*scale,.03*scale),theme,pv,'MeasuredStatusLightbar')
def rail(a,x,y,z,h=2.0,depth=.7,width=1.8):
 for xx in (x-width/2,x+width/2):
  b(a,(xx,y,z),(0.10,h,depth),'structural',None,'VerticalServiceSkidBeam')
 for yy in (y-h/2,y+h/2):b(a,(x,yy,z),(width+.10,.09,depth),'titanium',None,'HorizontalServiceSkidRail')
def pipe(a,points,r=.075,m='copper',pv=None):
 for p,q in zip(points[:-1],points[1:]):beam(a,p,q,r,m,pv,'FlangedServiceFluidLine')
 for p in (points[0],points[-1]):cy(a,p,r*1.65,.045,'gold','y',pv,'PipeGasketedFlange',12)
def valve(a,p,pv=None,scale=1):
 x,y,z=p
 cy(a,p,.17*scale,.42*scale,'structural','y',None,'ValveBody',20)
 tor(a,(x,y+.23*scale,z),.21*scale,.026*scale,'gold','y',pv,'ManualValveWheel',28)
 for th in range(4):
  t=th*math.pi/2
  beam(a,(x,y+.22*scale,z),(x+.20*scale*math.cos(t),y+.22*scale,z+.20*scale*math.sin(t)),.019*scale,'copper',pv,'ValveWheelSpoke')
 cy(a,(x,y+.11*scale,z),.06*scale,.27*scale,'titanium','y',pv,'ValveStem')
def fan(a,p,r=.85,pv=None):
 x,y,z=p
 tor(a,(x,y,z),r,.11,'structural','z',None,'VentilationBlowerCasing',48,9)
 tor(a,(x,y,z+.12),r*.79,.04,'gold','z',None,'BlowerProtectiveRing',48)
 cy(a,(x,y,z+.16),r*.18,.30,'structural','z',pv,'BlowerHub',28)
 for k in range(9):
  th=k*math.tau/9
  # swept compact fan blade with leading/trailing outline, each blade a real 3D plate
  cth=th+.18
  v=[(x+.15*math.cos(th),y+.15*math.sin(th),z+.15),
     (x+.80*r*math.cos(th+.34),y+.80*r*math.sin(th+.34),z+.20),
     (x+.83*r*math.cos(th+.67),y+.83*r*math.sin(th+.67),z+.30),
     (x+.20*math.cos(th+.33),y+.20*math.sin(th+.33),z+.27)]
  a.mesh(v,[[0,1,2],[0,2,3]],'titanium',pv,'CurvedTurboFanRotorBlade')
 for k in range(12):
  th=k*math.tau/12
  beam(a,(x+r*.92*math.cos(th),y+r*.92*math.sin(th),z+.30),(x+r*.92*math.cos(th),y+r*.92*math.sin(th),z+.43),.02,'gold',None,'ProtectiveFanGuard')
def vessel(a,p,r=0.68,h=3.3,mat='titanium',pv=None,name='AdsorptionProcessTower',contents='adsorbent'):
 x,y,z=p
 cy(a,(x,y,z),r,h,mat,'y',None,name,36)
 for side in (-1,1):
  cy(a,(x,y+side*h*.5,z),r*.92,.22,'structural','y',None,'DishedPressureVesselEndCap',30)
  tor(a,(x,y+side*(h/2-.18),z),r+.03,.052,'gold','y',None,'BoltedVesselEndFlange',48)
 for j in range(12):
  th=j*math.tau/12
  cy(a,(x+(r+.02)*math.cos(th),y+h*.5-.18,z+(r+.02)*math.sin(th)),.032,.14,'gold','y',None,'PressureFlangeStud',9)
 for j in range(9):
  th=j*math.tau/9
  beam(a,(x+(r+.075)*math.cos(th),y-h*.39,z+(r+.075)*math.sin(th)),(x+(r+.075)*math.cos(th),y+h*.39,z+(r+.075)*math.sin(th)),.024,'structural',None,'VesselReinforcementRod')
 # equipment viewport, cartridge load and functional panel separate
 b(a,(x,y,z+r+.01),(.49,.93,.04),'graphite',None,'MachinedDiagnosticViewportFrame')
 b(a,(x,y,z+r+.045),(.40,.84,.013),'glass',None,'PolycarbonateInspectionGlazing')
 for j in range(10):
  b(a,(x,y-.35+j*.08,z+r+.067),(.26,.019,.017),'cyan' if j%2==0 else 'amber',pv,'BedLoadingTelemetryMarker')
 for s in (-1,1):
  cy(a,(x,y+s*(h*.5+.18),z),.23,.40,'gold','y',None,'VesselProcessPort',20)
 for yy in (-h*.25,h*.25):
  for xx in (-.4,.4):
   b(a,(x+xx,y+yy,z-r-.18),(.09,.16,.32),'structural',None,'VesselSupportBrackets')
def make_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i]; piv=PV_BY_MODEL[i];pv=piv[0] if piv else None
 if i==0:
  b(a,(0,-.18,1.4),(30.8,.42,21.6),'floor',None,'LifeSupportDeckCompositeSlab')
  for j in range(18):b(a,(-14.2+j*1.68,.063,1.2),(.021,.018,20.1),'structural',None,'FloorModularServiceSeam')
  for j in range(14):b(a,(0,.065,-8.3+j*1.48),(30.0,.02,.026),'graphite',None,'FloorAntiSlipDiamondSeam')
  for side in (-1,1):
   b(a,(side*14.8,.11,1.1),(.15,.3,20),'gold',None,'SafetyFloorEdgeStripe')
   for j in range(12):b(a,(side*14.1,.080,-8+j*1.7),(.40,.035,.19),'amber',None,'WarningHazardChevron')
  for j in range(8):
   xp=-13+j*3.7
   b(a,(xp,.07,-3.7),(.21,.085,14),'titanium',None,'RaisedCableManagementTray')
 elif i==1:
  # Exterior simulated orbital viewport and greenhouse lighting wall, not a 2D image.
  b(a,(0,6.4,-13.15),(27,10,.10),'glass',None,'PanoramicCurvedViewGlass')
  for j in range(13):
   xx=-13+j*2.15
   beam(a,(xx,1.5,-13.08),(xx,12,-13.08),.065,'titanium',None,'ViewportReinforcedWindowMullion')
  for j in range(9):
   z0=-13.5;x0=-12+j*3
   cy(a,(x0,4.1,z0),.26,1.05,'structural','y',None,'BioDomeSupportPedestal',12)
  for j in range(115):
   xx=random.uniform(-14,14);yy=random.uniform(7,14)
   b(a,(xx,yy,-14),(.016,.016,.012),'stars' if j%3 else 'cyan',None,'ExteriorOrbitalStarGlint')
 elif i==2:
  # Greenhouse curved glazing tunnel: repeated rings, segmented transparent panels, trellis and plant silhouettes.
  for k in range(13):
   xx=-10+k*1.64
   for j in range(17):
    theta=j*math.pi/16
    yy=6.5+3.45*math.sin(theta);zz=-9.4-3.4*math.cos(theta)
    if j<16:
     t2=(j+1)*math.pi/16
     beam(a,(xx,yy,zz),(xx,6.5+3.45*math.sin(t2),-9.4-3.4*math.cos(t2)),.050,'structural',None,'ArchedHabitatGlazingFrame')
    if k in (0,12) or j%4==0:cy(a,(xx,yy,zz),.057,.085,'gold','x',None,'HabitatPanelJoiningNode',10)
  for k in range(12):
   xx=-9.18+k*1.64
   for j in range(16):
    t=(j+.5)*math.pi/16; yy=6.5+3.45*math.sin(t); zz=-9.4-3.4*math.cos(t)
    b(a,(xx,yy,zz),(1.45,.075,.075),'glass',None,'TransparentHabitatGlazingTile')
  for j in range(75):
   xx=random.uniform(-9.5,9.5);zz=random.uniform(-12.4,-9.0)
   stemh=random.uniform(.4,1.4)
   beam(a,(xx,6.2,zz),(xx,6.2+stemh,zz),.027,'copper',None,'HabitatGardenPlantStem')
   sphere(a,(xx,6.2+stemh,zz),random.uniform(.22,.45),'foliage2' if j%3==0 else 'foliage',None,'HabitatBiodomePlantFoliage',1)
  tor(a,(0,8.2,-9.5),5.5,.05,'cyan','z',pv,'HabitatAtmosphereHolographicSchematicRing',64)
 elif 3<=i<=6:
  # Four physically distinct compartment façade modules.
  idx=i-3
  b(a,(x,6.9,-9.08),(3.6,3.15,.29),'ceramic',None,'PressurizedHabitatCompartmentShell')
  for side in (-1,1):b(a,(x+side*1.75,7,-8.82),(.13,3.45,.15),'gold',None,'GasTightCompartmentStiffeningFrame')
  for j in range(5):
   xx=x-1.22+j*.61
   b(a,(xx,7.25,-8.89),(.40,.63,.065),'glass',None,'HabitatViewWindow')
   b(a,(xx,6.3,-8.86),(.28,.08,.06),'cyan' if i%2==0 else 'green',None,'EnvironmentalCompartmentStatusBar')
  tor(a,(x,6.7,-8.72),.68,.065,'titanium','z',None,'HabitatBulkheadRoundAccessDoor',40)
  b(a,(x,9.12,-8.83),(2.9,.16,.07),'blue',pv,'FourZoneAirQualitySignage')
 elif i==7:
  for j in range(4):
   xx=x+(j-1.5)*1.55
   tor(a,(xx,y,z),.64,.043,'cyan','z',None,'FourCompartmentPressureHologramRing',52)
   tor(a,(xx,y,z+.01),.40,.024,'green' if j==2 else 'blue','z',pv,'HabitatOxygenPartialPressureArc',40)
   for k in range(8):
    th=k*math.tau/8
    beam(a,(xx,y,z),(xx+.58*math.cos(th),y+.58*math.sin(th),z),.010,'cyan',pv,'HolographicAirflowVector')
 elif i==8:
  b(a,(0,5.3,-7.4),(25,6.2,.40),'titanium',None,'StationHabitatRearStructuralBulkhead')
  for j in range(10):b(a,(-11+j*2.5,5.3,-7.1),(.09,5.9,.15),'gold',None,'PressureVesselReinforcementBeam')
  for j in range(3):
   tor(a,(-7+j*7,5.4,-6.97),1.13,.06,'structural','z',None,'AccessHatchSealedRing',40)
 elif i==9:
  tor(a,(x,y,z),1.32,.12,'titanium','z',None,'AirlockPressureRatedRoundFrame',60)
  cy(a,(x,y,z+.16),1.17,.20,'structural','z',pv,'AirlockSlidingSafetyDoorLeaf',56)
  for j in range(12):
   th=j*math.tau/12;cy(a,(x+1.2*math.cos(th),y+1.2*math.sin(th),z+.26),.065,.15,'gold','z',None,'DoorSealRetainingStud',12)
 elif i==10:
  for xx in (-13.5,13.5):
   for k in range(10):
    zz=-4+k*1.30
    beam(a,(xx,.3,zz),(xx,1.15,zz),.06,'gold',None,'ServiceWalkwayGuardPost')
   beam(a,(xx,1.15,-4),(xx,1.15,7.7),.065,'structural',None,'ContinuousServicingHandRail')
  for k in range(16):
   zz=-4+k*.73
   b(a,(0,.05,zz),(27.2,.025,.06),'structural',None,'MaintenanceWorkstationFloorGrating')
 elif i==11:
  for dx in (-10.6,-7.9,-3.8,.4,4.4,9.3):
   pipe(a,[(dx,4.75,-4),(dx,6.6,-4),(dx,6.6,2.6)],.090,'copper')
   for k in range(3):tor(a,(dx,6.6,-2+k*2.1),.18,.028,'gold','x',None,'OverheadPipeClamp',22)
  for yy in (5.1,5.55):
   pipe(a,[(-13.3,yy,-4),(13.3,yy,-4)],.075,'titanium')
 elif i in (12,13):
  vessel(a,(x,y,z),.78,3.9,'titanium',pv,'RegenerativeCO2MolecularSieveTower')
  rail(a,x,y,z,4.6,.95,1.98)
  for j in range(30):
   th=j*math.tau/30;yy=y-1.0+(j%9)*.27
   sphere(a,(x+.36*math.cos(th),yy,z+.34*math.sin(th)),.10,'adsorbent' if i==12 else 'amber',None,'ModeledSorbentBedBead',1)
  valve(a,(x,y+2.25,z),pv,.88)
 elif i in (14,15,20,40,53,61,69,70):
  if i==61:
   rail(a,x,y,z,2.7,.72,3.3)
   for j,letter in enumerate(('A','B','C','D')):
    xx=PIVOT_POS['ANIM_DAMPER_'+letter][0]
    cy(a,(xx,y,z+.20),.27,.14,'titanium','z',None,'IndividualMotorizedZoneDamperBore',20)
    b(a,(xx,y,z+.31),(.37,.47,.05),'gold','ANIM_DAMPER_'+letter,'ActualDamperBlade')
    b(a,(xx,y+.48,z+.35),(.30,.07,.09),'cyan',None,'ZoneDamperFeedbackLED')
  else:
   valve(a,(x,y,z),pv)
   for j in range(3):
    offset=(-.48+j*.48)
    pipe(a,[(x+offset,y-.52,z),(x+offset,y-.12,z)],.052,'titanium')
    cy(a,(x+offset,y+.25,z),.13,.10,'gold','z',None,'FlowManifoldInstrumentTap',14)
 elif i in (16,17,29,30):
  rail(a,x,y,z,2.4,.62,1.24)
  for row in range(5):
   for col in range(6):
    xx=x+(col-2.5)*.16;yy=y+(row-2)*.32
    cy(a,(xx,yy,z+.22),.063,.23,'adsorbent' if i in (16,17) else 'cyan','z',None,'RegenerativeBeadCartridge' if i<18 else 'ElectrolyzerMembraneCell',12)
  b(a,(x,y+.98,z+.41),(.64,.04,.08),'cyan',pv,'CartridgeActiveScanBar')
 elif i in (18,19,24,38,39,48):
  rail(a,x,y,z,1.8,.7,1.40)
  cy(a,(x,y,z+.20),.58,.35,'graphite','z',None,'FluidProcessImpellerHousing',32)
  tor(a,(x,y,z+.38),.48,.050,'copper','z',None,'HeatExchangeFlowChannel',36)
  for j in range(12):
   th=j*math.tau/12
   beam(a,(x,y,z+.40),(x+.40*math.cos(th),y+.40*math.sin(th),z+.40),.024,'titanium',pv,'PumpOrHeaterRotorVane')
  pipe(a,[(x-.66,y,z+.15),(x-1.1,y+.32,z+.1)],.056,'copper')
  pipe(a,[(x+.66,y,z+.15),(x+1.1,y+.32,z+.1)],.056,'copper')
 elif i in (25,33,41,47):
  vessel(a,(x,y,z),.47,2.2,'structural',pv,'HighIntegrityAtmosphereStorageVessel')
  tor(a,(x,y+.38,z+.48),.27,.024,'cyan' if i!=47 else 'blue','z',pv,'TankMeasuredCapacityGauge',32)
  for j in range(7):b(a,(x,y-.62+j*.16,z+.50),(.20,.032,.020),'cyan' if j<4 else 'gold',None,'InventoryLevelScaleMark')
 elif i==28:
  rail(a,x,y,z,4.2,1.4,4.4)
  b(a,(x,y,z),(3.9,3.5,1.32),'titanium',None,'OxygenPlantRadiationShieldedMainCabinet')
  b(a,(x,y+.28,z+.72),(2.95,2.0,.070),'glass',None,'ElectrolysisProcessObservationWindow')
  for j in range(14):
   xx=x-1.25+j*.195
   cy(a,(xx,y-.4,z+.72),.067,.78,'water','y',None,'TransparentCellStackWaterChannel',12)
   for k in range(6):
    yy=y-.60+k*.19
    sphere(a,(xx,yy,z+.80),.045,'cyan',pv,'ElectrolysisBubbleEmissionNode',1)
  for j in range(8):b(a,(x-1.35+j*.38,y+1.68,z+.45),(.23,.05,.08),'cyan' if j%2 else 'green',None,'CellOutputPowerDistributionLED')
 elif i==31:
  vessel(a,(x,y,z),.46,2.1,'titanium',pv,'OxygenGasLiquidPhaseSeparator')
  for j in range(3):
   pipe(a,[(x-.6,y-.5+j*.3,z),(x-.9,y-.5+j*.3,z)],.042,'copper')
 elif i in (32,34,35,36,37,42,43,49,50,51,52,55,58,59,62,63,64,65,66,67,68,71,73,74,76,77,78,79,80,81,84,85,86,87):
  kind='fault' if i in (26,50,71) else 'optical' if i in (35,52,66,67,68,73) else 'plc' if i in (37,43,51,58,59,76,77,78,79,80,81) else 'instrument'
  panel(a,(x,y,z),1.02,'red' if i in (50,71) else 'amber' if i in (77,81,87) else 'cyan',kind,pv)
  if i==66:
   cy(a,(x+.25,y+.38,z+.44),.15,.22,'glass','z',None,'PrimaryCabinPressureAbsoluteSensorHead',24)
   b(a,(x,y-.19,z+.50),(.68,.10,.027),'gold',pv,'TrustedPressureSampleTrace')
  if i==77:b(a,(x+.37,y-.42,z+.45),(.20,.10,.08),'red',pv,'IndependentSafetyInhibitLamp')
  if i==86:
   for j in range(4):
    cy(a,(x-.33+j*.20,y-.45,z+.50),.057,.07,'gold','z',None,'PlayerWireCriticalPatchJack',14)
   b(a,(x,y+.43,z+.45),(.36,.10,.06),'red',pv,'W145VisualFaultAlarm')
 elif i in (26,27):
  panel(a,(x,y,z),1.02,'amber','fault' if i==26 else 'plc',pv)
 elif i in (44,45):
  rail(a,x,y,z,3.1,.8,2.35)
  b(a,(x,y,z),(1.95,2.6,.82),'titanium',None,'HumidityCondenserHeatExchangerHousing')
  for j in range(13):
   yy=y-1.1+j*.18
   pipe(a,[(x-.86,yy,z+.44),(x+.85,yy,z+.44)],.045,'water' if i==45 else 'copper')
  for j in range(8):
   xx=x-.73+j*.21
   b(a,(xx,y,z-.4),(.035,2.44,.05),'structural',None,'CondenserThermalExchangeFin')
  b(a,(x,y,z+.52),(1.3,1.68,.06),'glass',None,'CondenserServiceGlassInspectionDoor')
 elif i==46:
  vessel(a,(x,y,z),.46,1.92,'titanium',pv,'CondensedMoistureSeparator')
  for j in range(7):cy(a,(x,y-0.62+j*.20,z+.5),.06,.07,'water','z',pv,'WaterCondensateTelemetryDroplet',10)
 elif i==54:
  rail(a,x,y,z,2.4,.8,2.4)
  for j in range(7):
   xx=x-.85+j*.28
   cy(a,(xx,y,z+.2),.12,1.3,'glass','y',None,'UltrapureWaterFiltrationCartridge',14)
  b(a,(x+.4,y-.40,z+.6),(.74,.22,.11),'titanium',pv,'RemovableWaterFilterServiceDrawer')
 elif i in (56,57):
  fan(a,(x,y,z),1.05,pv)
  rail(a,x,y,z,2.7,.6,2.65)
  for j in range(8):
   th=j*math.tau/8
   cy(a,(x+1.07*math.cos(th),y+1.07*math.sin(th),z+.12),.10,.25,'gold','z',None,'TurboBlowerMagneticBearingBolt',12)
 elif i==60:
  for dy in (0,.58):
   pipe(a,[(7.3,y+dy,z),(12.7,y+dy,z)],.28,'structural')
   for j in range(6):
    xx=7.35+j*1.03
    tor(a,(xx,y+dy,z),.29,.045,'gold','x',None,'MainHabitatDuctFlange',24)
  for j in range(5):
   xx=7.7+j*.90
   b(a,(xx,y+.3,z+.23),(.13,.11,.08),'cyan',pv,'DuctFlowDirectionalBeacon')
 elif i==72:
  rail(a,x,y,z,2.6,.72,2.65)
  for j in range(10):
   xx=x-1.1+j*.245
   b(a,(xx,y,z+.3),(.14,1.9,.25),'ceramic',None,'ReplaceableCabinParticulateFilterPanel')
  b(a,(x,y+.40,z+.58),(.74,.24,.05),'cyan',pv,'AirFilterExtractionCarriage')
 elif i==75:
  cy(a,(x,y,z),.44,1.10,'structural','y',None,'PressureSafetyValveBody',28)
  cy(a,(x,y+.58,z),.22,.30,'gold','y',pv,'SpringPressureReliefActuator',22)
  tor(a,(x,y+.65,z),.25,.035,'red','y',None,'IndependentSafetyReliefIndicator',22)
 elif i==82:
  b(a,(0,7.5,-5.6),(6.5,2.45,.13),'structural',None,'HabitatEnvironmentalSCADAHologramMount')
  b(a,(0,7.5,-5.45),(5.9,2.17,.08),'panel',None,'LiveAirQualityMultiCompartmentScreen')
  for j in range(4):
   xx=-2.15+j*1.42
   tor(a,(xx,7.6,-5.36),.50,.036,'cyan' if j!=3 else 'amber','z',pv,'CompartmentFourColourAirQualityGauge',40)
  for j in range(10):b(a,(-2.6+j*.56,6.82,-5.33),(.24,.04,.025),'green' if j%3 else 'gold',None,'OperationalTrendTrace')
 elif i==83:
  for k in range(4):
   tor(a,(x+(k-1.5)*1.2,y,z),.53,.028,'cyan','z',pv,'AtmosphericFlowVisualDigitalTwin',46)
   for j in range(6):
    t=j*math.tau/6
    beam(a,(x+(k-1.5)*1.2,y,z),(x+(k-1.5)*1.2+.50*math.cos(t),y+.50*math.sin(t),z),.013,'blue',pv,'AtmosphericHologramArrow')
 elif i==88:
  tor(a,(x,y,z),1.05,.068,'gold','z',None,'FinalMissionCertificationBeaconOuterRing',52)
  tor(a,(x,y,z+.11),.82,.048,'cyan','z',pv,'FinalMissionCertificationStatusRing',52)
  b(a,(x,y,z+.19),(1.10,.13,.05),'green',None,'FinalAtmosphereQualifiedReadout')
 elif i==89:
  cy(a,(x,y,z),.35,.70,'structural','z',None,'AtmosphereInspectionPTZCamera')
  cy(a,(x,y,z+.4),.18,.20,'glass','z',None,'AtmosphereInspectionOpticalLens',30)
  for j in range(4):b(a,(x+(.26 if j%2 else -.26),y+(j-1.5)*.22,z+.35),(.08,.12,.16),'gold',None,'InspectionCameraShieldedServicePod')
 else:
  panel(a,(x,y,z),1.0,'cyan','instrument',pv)
 # Guarantee every planned pivot group owns at least one real piece of geometry.
 used={k for _,_,_,k,_ in a.raw if k}
 for pi,pid in enumerate(piv):
  if pid not in used:
   px,py,pz=PIVOT_POS[pid]
   b(a,(px,py+.56+(pi*.055),pz+.48),(.16,.050,.065),'cyan' if PIVOTS[[p['Pivot'] for p in PIVOTS].index(pid)]['Kind']=='indicator' else 'gold',pid,'VisualAnimationOwnedPivotIndicator')
 return a
# Engineering wiring proposal: these are source planning connections, not existing Unity runtime wires.
GROUP_SPEC=[
('Educational 5V Control and Return',12,'POWER_CONTROL',[
 (78,80,'Primary ECLSS PLC logic power'),(78,77,'Independent life-support safety controller power'),(78,27,'Adsorption-cycle PLC control power'),(78,43,'Oxygen process controller power'),(78,51,'Humidity recovery controller power'),(78,76,'Ventilation and air-quality controller power'),(78,82,'Atmosphere SCADA control power'),(78,66,'Primary cabin pressure transducer conditioning power'),(78,67,'Backup pressure monitor logic power'),(78,79,'Telemetry gateway logic power'),(77,78,'Life-support safety control return'),(80,78,'Main ECLSS supervisory control return')]),
('Cabin Gas and Pressure Instrumentation',24,'ANALOG_FEEDBACK',[
 (62,80,'Crew compartment measured cabin pressure'),(63,80,'Research compartment measured cabin pressure'),(64,80,'Garden compartment measured cabin pressure'),(65,80,'Habitat compartment measured cabin pressure'),(66,80,'Primary cabin total-pressure telemetry diagnostic'),(67,77,'Independent cabin-pressure safety monitor'),(68,80,'Differential compartment pressure trend'),(21,27,'CO2 absorption intake actual measurement'),(22,27,'CO2 absorption exhaust actual measurement'),(35,43,'Oxygen partial-pressure sensor feedback'),(52,51,'Water-vapor humidity measurement'),(73,76,'Actual air-duct flow velocity'),(74,80,'Mixed sample gas analyzer output'),(79,82,'Atmosphere sampling gateway display'),(80,62,'Zone A pressure sampling request'),(80,63,'Zone B pressure sampling request'),(80,64,'Zone C pressure sampling request'),(80,65,'Zone D pressure sampling request'),(77,68,'Safety differential pressure diagnostic'),(76,73,'Airflow transducer calibration command'),(80,35,'Oxygen analyzer measurement request'),(80,21,'CO2 intake calibration request'),(80,22,'CO2 outlet calibration request'),(67,82,'Non-authoritative backup pressure diagnostic display')]),
('Dual CO2 Scrubbing and Regeneration',24,'DIGITAL_CONTROL',[
 (27,12,'Adsorber A process start permit'),(27,13,'Adsorber B process start permit'),(27,14,'Adsorber A inlet command'),(27,15,'Adsorber B inlet command'),(14,27,'Adsorber A actual valve feedback'),(15,27,'Adsorber B actual valve feedback'),(16,23,'Adsorber A sorbent loading measurement'),(17,23,'Adsorber B sorbent loading measurement'),(23,27,'Measured capture capacity qualification'),(27,18,'Adsorber A regeneration heater enable'),(27,19,'Adsorber B regeneration heater enable'),(18,27,'Adsorber A actual heat feedback'),(19,27,'Adsorber B actual heat feedback'),(27,20,'Selector valve switching command'),(20,27,'Selector valve actual transition feedback'),(27,24,'Exhaust regeneration condenser enable'),(24,27,'Exhaust condenser active status'),(27,25,'Captured CO2 storage isolation command'),(25,27,'CO2 canister isolation actual position'),(26,27,'Breakthrough fault injection status'),(21,23,'Adsorbent upstream CO2 sample'),(22,23,'Adsorbent downstream CO2 sample'),(27,82,'Measured scrubber cycle health telemetry'),(80,27,'Master scrubber operating setpoint')]),
('Oxygen Generation and Gas Conditioning',22,'DIGITAL_CONTROL',[
 (43,28,'Water electrolysis generation enable'),(43,29,'Electrolysis cell stack A drive request'),(43,30,'Electrolysis cell stack B drive request'),(29,42,'Cell stack A voltage and health'),(30,42,'Cell stack B voltage and health'),(42,43,'Individual electrolysis cell diagnostics'),(43,31,'Oxygen product gas separator command'),(31,43,'Gas separator measured qualification'),(43,32,'Isolate byproduct stream safety command'),(32,77,'Byproduct containment safety status'),(43,33,'Oxygen buffer fill request'),(33,43,'Measured oxygen buffer inventory'),(43,34,'Oxygen outlet flow demand'),(34,43,'Measured delivered oxygen flow'),(35,43,'Oxygen partial pressure control feedback'),(36,43,'Oxygen product quality feedback'),(43,37,'Electrolysis power converter current limit'),(37,43,'Measured electrolyzer power and losses'),(43,38,'Oxygen plant thermal cooling command'),(38,43,'Coolant flow and temperature feedback'),(43,40,'Oxygen delivery regulation permit'),(41,77,'Emergency reserve isolation safety state')]),
('Humidity and Water-Recovery Process',20,'ANALOG_FEEDBACK',[
 (51,44,'Humidity condenser process enable'),(51,45,'Cooling coil heat exchange command'),(45,51,'Cooling circuit actual temperature feedback'),(44,51,'Measured condensate collection rate'),(51,46,'Condensate separation command'),(46,51,'Separator actual throughput feedback'),(47,51,'Recovered water tank level'),(51,48,'Reclaimed water pump run command'),(48,51,'Actual reclaimed water pump flow'),(49,51,'Reclaimed water quality validity'),(51,54,'Water cartridge filtration enable'),(54,51,'Water filtration actual health'),(52,51,'Compartment humidity actual measurement'),(51,53,'Condensation cooling valve actuator command'),(53,51,'Actual cooling valve position feedback'),(50,51,'Frozen condenser diagnostic fault state'),(55,51,'Water reserve budget constraint'),(51,55,'Measured recovered water inventory'),(55,39,'Water supply allowance to oxygen plant'),(39,43,'Water pump actual feed flow to electrolysis')]),
('Ventilation, Valves and Filtration',18,'DIGITAL_CONTROL',[
 (76,56,'Main circulation blower enable'),(76,57,'Backup circulation blower enable'),(76,58,'Main fan speed request'),(76,59,'Backup fan speed request'),(58,56,'Main fan VFD actual output'),(59,57,'Backup fan VFD actual output'),(56,76,'Measured primary blower speed'),(57,76,'Measured standby blower speed'),(76,60,'Main duct distribution flow setpoint'),(60,76,'Actual main duct pressure/flow'),(76,61,'Four-zone motorized damper requests'),(61,76,'Measured zone damper positions'),(76,72,'Cabin trace filtration airflow command'),(72,76,'Measured replaceable filter pressure drop'),(73,76,'Measured air velocity after filtration'),(76,74,'Compartment gas mixing sampling command'),(74,76,'Sample station calibrated gas quality'),(80,76,'Habitat occupancy demand ventilation plan')]),
('Independent Safety and Pressure Disturbances',26,'SAFETY_INTERLOCK',[
 (77,69,'Compartment leak isolation authorization'),(69,77,'Actual leak isolation valve feedback'),(77,70,'Normal pressure regulator safety permit'),(70,77,'Actual pressure regulator operation state'),(71,77,'Micro-leak fault source declaration'),(75,77,'Independent relief valve mechanically sensed state'),(67,77,'Independent backup pressure protector health'),(68,77,'Compartments differential pressure safety'),(32,77,'Byproduct stream segregation interlock'),(41,77,'Backup oxygen supply isolation state'),(81,77,'Water resource independent safety inhibit'),(77,81,'Safety PLC water-energy permit status'),(80,77,'Atmosphere supervisor safety run request'),(77,80,'Actual independent safety authorization'),(77,87,'Emergency isolation inhibit output'),(87,77,'Emergency station actual stop state'),(77,80,'Safety fault latch event report'),(85,77,'Simulated occupancy test safety status'),(50,77,'Condenser freeze critical inhibit'),(26,77,'CO2 breakthrough safety condition'),(77,83,'Safety status for hologram readout'),(77,82,'Safety latched fault SCADA output'),(80,87,'Operator explicit safety reset request'),(87,77,'Operator explicit reset demand feedback'),(66,77,'CRITICAL trusted cabin pressure feedback'),(77,88,'Six-objective final safety permission')]),
('SCADA Diagnostics and Certification',18,'ANALOG_FEEDBACK',[
 (21,82,'Inlet CO2 analyzers SCADA sampled data'),(22,82,'Outlet CO2 analyzers SCADA sampled data'),(23,82,'CO2 capture-bed lifetime telemetry'),(27,82,'Adsorption cycle and regeneration status'),(33,82,'Oxygen buffer inventory telemetry'),(35,82,'Oxygen partial pressure trend'),(36,82,'Oxygen product quality display'),(43,82,'Oxygen generation process status'),(47,82,'Condensate inventory telemetry'),(51,82,'Humidity recovery process telemetry'),(55,82,'Water reserve forecast telemetry'),(56,82,'Main blower actual response trend'),(61,82,'Compartment damper actual state trend'),(68,82,'Differential pressure survey'),(80,82,'Authoritative life support mission state'),(82,83,'Atmosphere digital twin display updates'),(84,82,'Process history trend recording'),(82,88,'Objective certification visualization only')])]
assert [e[1] for e in GROUP_SPEC]==[12,24,24,22,20,18,26,18]
WIRES=[];GROUPS=[]
for title,cnt,kind,edges in GROUP_SPEC:
 assert len(edges)==cnt,(title,len(edges),cnt)
 first=len(WIRES)+1
 for j,(s,t,reason) in enumerate(edges):
  wid=f'W{first+j:03d}'
  signal = ('GROUND_RETURN' if first+j in (11,12) else 'POWER_CONTROL') if first==1 else (
   'ANALOG_FEEDBACK' if any(k in reason.lower() for k in ('measured','actual','feedback','status','health','sampling','quality','inventory','monitor','diagnostic','telemetry','flow','trend','fault','condition','response')) else kind)
  if first+j==145: signal='ANALOG_FEEDBACK'
  WIRES.append(dict(WireID=wid,Group=title,SourceModel=NAMES[s],TargetModel=NAMES[t],SourceVisualAnchor='',TargetVisualAnchor='',SignalKind=signal,Purpose=reason,
   Critical='YES' if first+j==145 else 'NO',ImplementationStatus='CONCEPT_ONLY_NO_PLAYER_WIRE'))
 GROUPS.append(dict(Group=title,StartWire=f'W{first:03d}',EndWire=f'W{len(WIRES):03d}',Count=cnt))
assert len(WIRES)==164
assert WIRES[144]['SourceModel']==NAMES[66] and WIRES[144]['TargetModel']==NAMES[77]
# All 90 models have at least one visible socket; busier models receive more.
usage=Counter()
for w in WIRES:usage[w['SourceModel']]+=1;usage[w['TargetModel']]+=1
counts={i:1 for i in range(90)}
while sum(counts.values())<242:
 i=max(range(90),key=lambda ii:(usage[NAMES[ii]]/(counts[ii]+.65),-ii))
 counts[i]+=1
assert sum(counts.values())==242
ANCHORS=[];PER_MODEL=defaultdict(list)
for i in range(90):
 x,y,z=P[i]
 for j in range(counts[i]):
  # Equipment-specific connector strip positions, decorative only.
  xw=x+((j%9)-4)*(.092 if i>=12 else .16)
  yw=y+((j//9)%4-1.4)*.105
  zw=z+(.48 if i>=12 else .27)
  aid=f'P{len(ANCHORS)+1:03d}'
  r=dict(AnchorID=aid,Model=NAMES[i],WorldX_m=round(xw,5),WorldY_m=round(yw,5),WorldZ_m=round(zw,5),Status='MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL')
  ANCHORS.append(r);PER_MODEL[i].append(r)
assert len(ANCHORS)==242
LOGICAL=[];slot_count=Counter()
for w in WIRES:
 for side in ('Source','Target'):
  mid=NAMES.index(w[side+'Model']);idx=slot_count[mid];slot_count[mid]+=1
  aa=PER_MODEL[mid][(idx*3+int(w['WireID'][1:]))%len(PER_MODEL[mid])]
  w[side+'VisualAnchor']=aa['AnchorID']
  port_id=w['WireID']+'_'+('OUT' if side=='Source' else 'IN')+'_'+w['SignalKind']
  if w['WireID']=='W145':port_id='TRUSTED_CABIN_PRESSURE_OUT' if side=='Source' else 'TRUSTED_CABIN_PRESSURE_IN'
  LOGICAL.append(dict(WireID=w['WireID'],Endpoint=side,Model=w[side+'Model'],ArtAnchor=aa['AnchorID'],FunctionalPortID=port_id,
     SignalKind=w['SignalKind'],Direction='OUTPUT' if side=='Source' else 'INPUT',Critical=w['Critical'],FunctionalStatus='PROPOSED_NOT_ACTUAL_SPARKTERMINAL'))
assert len(LOGICAL)==328
reuse=defaultdict(list)
for p in LOGICAL:reuse[p['ArtAnchor']].append(p)
CONFLICTS=[dict(AnchorID=aid,Model=ports[0]['Model'],LogicalContactCount=len(ports),SignalKinds=';'.join(sorted(set(p['SignalKind'] for p in ports))),
 DifferentSignalKinds=len(set(p['SignalKind'] for p in ports))>1,Requirement='RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT') for aid,ports in reuse.items() if len(ports)>1]
OBJ=[
 ('Build the Atmospheric Control Network','All 164 actual player-created live electrical wires, compatible terminals/returns, safety, critical W145, no bypass','Electrical topology fingerprint, energy/safety qualification'),
 ('Calibrate the Habitat Atmosphere','All four zone pressure, O2 partial pressure, CO2, humidity and airflow instruments calibrated; actual fans and dampers proven','Timestamped calibration coefficients, fresh sample and actual actuator evidence'),
 ('Commission Oxygen and CO2 Regeneration','Finite water-fed oxygen generator and two physically cycling CO2 beds achieve qualified mass/energy accounting','Captured gas inventory, water/energy balance, valid actual flows and safe stream separation'),
 ('Sustain the Living Atmosphere','Fresh full low-demand→peak-demand→recovery scenario meets protected fictional gas partial pressures and humidity limits','Fresh run ID with compartment mass/thermal balance, actual ventilation and resource margin'),
 ('Recover Three Life-Support Failures','CO2 bed breakthrough, condenser freeze and micro-leak each change plant, repaired, then fresh full endurance run','Causal fault/repair history, no unresolved safety latch, new run ID'),
 ('Restore Critical Cabin Pressure Feedback','Physically disconnect W145 after historical 1–5, safety-inhibit, reconnect, calibrate, explicit reset, new full qualification','Real W145 circuit-edge mutation, sample validity, independent protection, post-fault runID, LevelGamePlayManager 6/6')]

def write_csv(path,rows):
 with open(path,'w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def dump(file,obj):Path(file).write_text(json.dumps(obj,indent=2),encoding='utf8')
def metadata():
 for file,rows in [(f'Gameplay_Wiring_Concept_W001_W164.csv',WIRES),('Gameplay_Wiring_Budget.csv',GROUPS),('Terminal_Anchor_Plan_242.csv',ANCHORS),('Visual_Terminal_Sockets_242.csv',ANCHORS),
  ('Terminal_Logical_Port_Reconciliation_328.csv',LOGICAL),('Terminal_Anchor_Signal_Conflict_Report.csv',CONFLICTS),('Objective_Certification_Plan_6.csv',[
   dict(ObjectiveIndex=j+1,Title=t,AcceptanceCriteria=criteria,Evidence=ev,Implemented='NO') for j,(t,criteria,ev) in enumerate(OBJ)]),
  ('Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PIVOTS]),
  ('Component_Roles.csv',[dict(Index=i,Model=NAMES[i],WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],Role=('Environment' if i<12 else 'Dual CO2 processing' if i<28 else 'Oxygen system' if i<44 else 'Water recovery' if i<56 else 'Ventilation and pressure' if i<76 else 'PLC, fault, SCADA and certification'),State='ART_ONLY') for i in range(90)]),
  ('Art_Assembly.csv',[dict(Index=i,Model=NAMES[i],File=f'{i:02d}_{NAMES[i]}.fbx',TransformOrigin='0,0,0',ExpectedWorldCenter_m=','.join(map(str,P[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres') for i in range(90)])]:write_csv(ROOT/file,rows)
 w=WIRES[144]
 (ROOT/'W145_Critical_Cabin_Pressure_Recovery.md').write_text(f'''# Level 88 — W145 Trusted Cabin Pressure Feedback Recovery

Planned critical row: `W145`, `L88_CabinPressurePrimaryTransducer` (`{w['SourceVisualAnchor']}`) → `L88_HabitatLifeSupportSafetyPLC` (`{w['TargetVisualAnchor']}`), `ANALOG_FEEDBACK`. Functional contacts: `TRUSTED_CABIN_PRESSURE_OUT` → `TRUSTED_CABIN_PRESSURE_IN`.

1. Historical Objectives 1–5 must be certified in the real LevelGamePlayManager first.
2. In a supervised fault exercise, player unplugs the real W145 runtime wire (art kit ships no connected gameplay wires), mutating the Spark electrical circuit graph.
3. Treat the trusted pressure sample as **invalid**, not zero pressure. Actual pressure continues via mass balance, P*V=n*R*T; do not teleport gas mass.
4. Withdraw normal pressure-regulation authorization; restrict unverified actuator commands and enter a protected safe hold with independent pressure relief and backup physical monitoring still effective. A severe actual gas fault can still cause unsafe atmosphere—do not claim immortal protection.
5. Latch critical safety fault and log event, preserve historical 1–5 objective records. SCADA cached readings, backup diagnostic pressure and W145 health bits cannot impersonate trusted feedback.
6. Repair correct live wire, verify 5 V source and return, acquire fresh range-valid sensor samples, calibrate primary pressure measurement and independently cross-check safety protection.
7. Require operator **explicit reset** only after qualified preconditions; start a **new runID** and perform full gas/humidity/CO2/O2 and four-compartment endurance test.
8. Only after a complete clean post-fault test may actual LevelGamePlayManager certify 6/6; reconnection alone can never complete Objective 6.

All units/limits are configurable fictional educational scenarios, not real spacecraft ECLSS safety specifications. These instructions are design requirements, not existing gameplay scripts.
''',encoding='utf8')

def with_sockets(i):
 a=make_art(i)
 for r in PER_MODEL[i]:a.socket((r['WorldX_m'],r['WorldY_m'],r['WorldZ_m']),r['AnchorID'])
 return a

def homes(i,art):
 rr=[(p['Pivot'],art.name,PIVOT_POS[p['Pivot']]) for p in PIVOTS if p['Index']==i]
 rr.extend(('SOCKET_'+aid,art.name,xyz) for aid,xyz in art.sockets)
 return rr

def exports():
 start=time.monotonic();models=[];chunks_all=[];homes_all=[];bmin=np.ones(3)*float('inf');bmax=-bmin
 for i,n in enumerate(NAMES):
  art=with_sockets(i); parts=core.chunk_model(art,max_shapes=20); home=homes(i,art)
  ff=ROOT/'Models'/f'{i:02d}_{n}.fbx'; gg=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  fr=core.export_fbx(str(ff),[(n,p) for p in parts],[n],home,art.sockets)
  gr=core.export_glb(str(gg),[(n,p) for p in parts],[n],home)
  sockets=dict(art.sockets)
  for part in parts:
   pv=part['pivot'];offset=PIVOT_POS[pv] if pv in PIVOT_POS else sockets[pv[7:]] if pv and pv.startswith('SOCKET_') else (0,0,0)
   vv=part['vertices']+np.asarray(offset)
   bmin=np.minimum(bmin,vv.min(axis=0));bmax=np.maximum(bmax,vv.max(axis=0))
  models.append(dict(index=i,name=n,file=ff.name,glb=gg.name,triangle_count=sum(len(p['faces']) for p in parts),mesh_parts=len(parts),
   source_primitive_count=len(art.raw),visual_terminal_sockets=len(art.sockets),animation_pivots={pv:xyz for pv,_,xyz in home if pv.startswith('ANIM_')},
   fbx_bytes=fr['file_size'],glb_bytes=gr['glb_bytes']))
  chunks_all.extend((n,p) for p in parts);homes_all.extend(home)
  if (i+1)%10==0 or i in (0,2,89):print('MODEL',i+1,'/90',n,models[-1]['triangle_count'],'triangles',round(time.monotonic()-start,1),'seconds',flush=True)
 print('ASSEMBLED REFERENCE',len(chunks_all),'mesh groups',flush=True)
 ff=ROOT/'Models'/'Level88_Complete_ArtKit_Showcase.fbx';gg=ROOT/'GLB'/'Level88_Complete_ArtKit_Showcase.glb'
 fr=core.export_fbx(str(ff),chunks_all,NAMES,homes_all,[(a['AnchorID'],(a['WorldX_m'],a['WorldY_m'],a['WorldZ_m'])) for a in ANCHORS]);gr=core.export_glb(str(gg),chunks_all,NAMES,homes_all)
 manifest=dict(level_id='level_88',display_name='The Breathing Ring',subtitle='Autonomous Life Support, Atmospheric Regeneration and Pressure Safety',
  individual_models=len(models),individual_glbs=len(models),assembled_fbx='Models/'+ff.name,assembled_glb='GLB/'+gg.name,
  individual_triangles=sum(m['triangle_count'] for m in models),individual_mesh_parts=sum(m['mesh_parts'] for m in models),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),
  proposed_independent_logical_wire_endpoints=len(LOGICAL),anchor_reconciliation_conflicts=len(CONFLICTS),
  workstation_world_bounds_m=[[round(float(v),5) for v in bmin],[round(float(v),5) for v in bmax]],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up X-width Z-forward',assembly_origin=[0,0,0],
  actual_player_wires_included=False,functional_sparkterminals_included=False,real_atmosphere_gas_solver_included=False,
  runtime_safety_plc_included=False,six_runtime_objectives_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
  assembled_fbx_bytes=fr['file_size'],assembled_glb_bytes=gr['glb_bytes'],models=models)
 dump(ROOT/'Model_Manifest.json',manifest)
 return manifest

if __name__=='__main__':
 print('LEVEL88 ART ASSET GENERATION',flush=True)
 metadata();manifest=exports();print('MODEL EXPORT COMPLETE',manifest['individual_triangles'],'triangles',flush=True)
