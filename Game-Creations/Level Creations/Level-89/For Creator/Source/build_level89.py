#!/usr/bin/env python3
"""Project Spark Level 89: THE HELIOS STORM — original procedural modular FBX/GLB art.
Y up, units metres internally, FBX centimetre numeric units. Art and design plans only,
not a Unity-playable level. Renders no image and imports no external proprietary content.
"""
from pathlib import Path
from collections import Counter,defaultdict
import json,csv,math,random,time
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path(__file__).resolve().parent.parent
for d in ['Models','GLB','Textures','Reports','Unity_Optional']: (ROOT/d).mkdir(exist_ok=True,parents=True)
NAMES=['L89_'+n.strip() for n in (Path(__file__).parent/'model_names.txt').read_text().splitlines() if n.strip()]
assert len(NAMES)==92
random.seed(89909)
core.COLORS={
 'structural':(.40,.50,.61,1),'titanium':(.71,.80,.87,1),'dark':(.039,.054,.078,1),
 'graphite':(.085,.12,.17,1),'copper':(.91,.46,.16,1),'gold':(.96,.63,.18,1),
 'ceramic':(.90,.94,.95,1),'glass':(.17,.66,.84,.48),'panel':(.014,.095,.16,1),
 'pcb':(.025,.26,.19,1),'cyan':(.06,.84,.99,1),'blue':(.20,.43,.96,1),
 'green':(.12,.92,.51,1),'amber':(1,.64,.12,1),'violet':(.73,.42,.99,1),
 'red':(.99,.13,.12,1),'orange':(1,.35,.06,1),'white':(.96,.96,.91,1),
 'water':(.10,.56,.92,.53),'shield':(.69,.75,.82,1),'insulation':(.19,.24,.29,1),
 'floor':(.24,.29,.36,1),'stars':(.81,.91,1,1),'storm':(1,.32,.048,1),
 'solar':(1,.73,.20,1),'black':(.012,.019,.027,1)
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
# Workstation-scale space-habitat art is ~31 m across. The physical radiation simulation is separately scaled.
P={0:(0,-.16,0),1:(0,7.4,-13.25),2:(4.9,10.2,-16.7),3:(0,3.8,-3.3),4:(0,3.8,-1.15),5:(-6.3,9.7,-12.4),
  6:(0,7.5,-13.0),7:(0,0.5,-3.2),8:(0,3.8,-3.0),9:(0,3.8,-6.1),10:(-12.4,4.4,-6.6),11:(11.9,2.0,-6.0),
  12:(-11.1,5.1,-7.3),13:(-8.9,5.1,-7.3),14:(-12.2,2.3,-4.5),15:(-10.4,2.3,-4.5),16:(-8.55,2.3,-4.5),
  17:(-9.8,5.3,-8.3),18:(-11,4.5,-6.6),19:(-10.5,1.85,-3.3),20:(-8.9,1.85,-3.3),
  26:(-6.0,4.1,-6.6),27:(-4.5,4.1,-6.6),28:(-3.2,3.4,-2.0),29:(3.2,3.4,-2.0),30:(-5.4,1.8,-3.3),
  32:(-7.2,1.9,-3.2),36:(-5.9,7.4,-10.0),39:(-2.15,3.8,-3.3),40:(-1.1,3.8,-3.3),41:(1.1,3.8,-3.3),42:(2.15,3.8,-3.3),
  43:(9.0,2.4,-5.5),44:(10.5,3.35,-4.4),45:(10.6,1.75,-2.65),46:(12.2,1.75,-2.65),47:(11.1,1.6,-1.1),
  52:(0,6.8,-3.3),53:(0,1.0,-3.3),54:(-3.35,3.8,-3.3),55:(3.35,3.8,-3.3),56:(0,3.85,-3.2),
  57:(-3.0,2.0,-1.0),58:(-1.0,2.0,-1.0),59:(1.0,2.0,-1.0),60:(3.0,2.0,-1.0),61:(0,6.45,-1.0),
  65:(1.6,2.0,-.85),66:(0,7.3,-3.3),67:(0,8.45,-10.1),68:(5.1,2.2,-5.3),69:(6.9,2.2,-5.3),70:(8.5,2.2,-5.3),
  74:(6.4,1.9,-3.0),75:(-6.4,8.0,-10.5),78:(12.8,1.65,5.2),79:(11.0,1.7,5.4),80:(7.9,1.8,5.4),81:(6.3,1.8,5.4),
  82:(0,8.5,-8.9),83:(0,11.2,-11.1),84:(-3.2,8.0,-11.2),85:(-7.8,1.7,5.4),86:(-6.4,1.7,5.4),
  87:(-4.8,1.7,5.4),88:(0,12.4,-9.9),89:(-3.2,1.7,7.7),90:(-1.55,1.7,7.7),91:(-13.3,5.3,-7.7)}
# All stations retain dedicated visual addresses in reachable instrument rows.
for jj,i in enumerate([21,22,23,24,25,31,33,34,35,37,38]):P[i]=(-13.0+jj*1.44,1.85,2.0)
for jj,i in enumerate([48,49,50,51,62,63,64,71,72,73,76,77]):P[i]=(-12.2+jj*2.13,1.75,3.92)
assert set(P)==set(range(92)), sorted(set(range(92))-set(P))
# 76 genuine named pivot parent nodes. Mechanical pivots own moveable parts; indicators own emissive geometry.
PV=[
 (4,'SHELTER_MAIN_SHIELD_DOOR','translation','y'),(5,'STORM_FORECAST_HOLOGRAM','indicator','none'),
 (9,'EMERGENCY_EGRESS_HATCH','rotation','y'),(12,'TELESCOPE_A_YAW','rotation','y'),(12,'TELESCOPE_A_ELEVATION','rotation','x'),
 (13,'TELESCOPE_B_YAW','rotation','y'),(13,'TELESCOPE_B_ELEVATION','rotation','x'),
 (14,'SPECTROMETER_DETECTOR_SCAN','translation','x'),(15,'PROTON_ENERGY_SELECTOR','rotation','z'),(16,'HEAVY_ION_CHANNEL_SCAN','translation','y'),
 (17,'SUN_TRACKER_AZIMUTH','rotation','y'),(17,'SUN_TRACKER_ELEVATION','rotation','x'),(18,'PARTICLE_COLLIMATOR_APERTURE','rotation','z'),
 (19,'PRIMARY_PARTICLE_PULSE','indicator','none'),(20,'SECONDARY_PARTICLE_PULSE','indicator','none'),
 (21,'DETECTOR_PULSE_CONDITIONING','indicator','none'),(22,'SPECTRAL_CALIBRATION_LAMP','indicator','none'),
 (23,'DETECTOR_THERMAL_SHROUD','rotation','y'),(24,'STORM_FORECAST_CONSOLE','indicator','none'),
 (25,'STORM_ARRIVAL_TIMELINE','indicator','none'),(26,'EXTERNAL_DOSIMETER_SCAN','indicator','none'),
 (27,'HABITAT_DOSIMETER_SCAN','indicator','none'),(28,'SHELTER_REFERENCE_DOSE','indicator','none'),
 (29,'INDEPENDENT_DOSE_MONITOR','indicator','none'),(30,'TRIAXIAL_DOSE_PROBE','rotation','y'),
 (31,'DOSE_TRANSIENT_RECORDER','indicator','none'),(32,'DOSIMETER_CALIBRATION_FRAME','translation','x'),
 (33,'DETECTOR_SATURATION_TEST','translation','x'),(34,'ACCUMULATED_DOSE_DISPLAY','indicator','none'),
 (35,'DETECTOR_NETWORK_GATEWAY','indicator','none'),(36,'RADIATION_HEATMAP_HOLOGRAM','indicator','none'),
 (37,'DETECTOR_COMPARATOR','indicator','none'),(38,'DOSE_LIMIT_EVALUATOR','indicator','none'),
 (39,'WATERWALL_PORT_A_LEVEL','indicator','none'),(40,'WATERWALL_PORT_B_LEVEL','indicator','none'),
 (41,'WATERWALL_STARBOARD_A_LEVEL','indicator','none'),(42,'WATERWALL_STARBOARD_B_LEVEL','indicator','none'),
 (43,'MAIN_WATER_TANK_LEVEL','indicator','none'),(44,'SHIELD_WATER_SENSOR_SCAN','translation','y'),
 (45,'WATER_PUMP_A_ROTOR','rotation','z'),(46,'WATER_PUMP_B_ROTOR','rotation','z'),
 (47,'SHIELD_WATER_DISTRIBUTOR','rotation','y'),(48,'SHIELD_MASS_ALLOCATION','indicator','none'),
 (49,'AREAL_DENSITY_HEATMAP','indicator','none'),(50,'WATER_RESOURCE_GUARD','indicator','none'),
 (51,'WATER_WALL_LEAK_MONITOR','indicator','none'),
 (52,'SHIELD_CASSETTE_TOP','translation','y'),(53,'SHIELD_CASSETTE_BOTTOM','translation','y'),
 (54,'SHIELD_CASSETTE_PORT','translation','x'),(55,'SHIELD_CASSETTE_STARBOARD','translation','x'),
 (56,'SHIELD_CARRIAGE_LOCK','translation','z'),
 (57,'SHIELD_ACTUATOR_A','translation','x'),(58,'SHIELD_ACTUATOR_B','translation','x'),
 (59,'SHIELD_ACTUATOR_C','translation','x'),(60,'SHIELD_ACTUATOR_D','translation','x'),
 (61,'LOCKING_PIN_A','translation','z'),(61,'LOCKING_PIN_B','translation','z'),
 (62,'SHIELD_POSITION_FEEDBACK','indicator','none'),(63,'SHIELD_MOTOR_DRIVE','indicator','none'),
 (64,'SHIELD_JAM_FAULT','translation','x'),(65,'SHELTER_DOOR_INTERLOCK','rotation','x'),
 (66,'SHELTER_SHIELD_QUALITY','indicator','none'),(67,'SHIELD_COVERAGE_TWIN','indicator','none'),
 (68,'SHELTER_AIR_BLOWER','rotation','z'),(69,'OXYGEN_BUFFER_STATUS','indicator','none'),
 (70,'THERMAL_RECLAIMER_VALVE','rotation','y'),(71,'SHELTER_LIFE_SUPPORT_PLC','indicator','none'),
 (75,'SIMULATED_SOLAR_STORM_INTENSITY','indicator','none'),(76,'ENERGY_SURGE_FAULT','translation','x'),
 (78,'EMERGENCY_ISOLATION_LEVER','rotation','x'),(80,'RADIATION_SAFETY_PLC','indicator','none'),
 (82,'RADIATION_DEFENSE_SCADA','indicator','none'),(83,'SOLAR_STORM_3D_HOLOGRAM','indicator','none'),
 (86,'W149_CRITICAL_FEEDBACK_PATCH','indicator','none'),(88,'FINAL_CERTIFICATION','indicator','none'),(91,'SURVEY_ANTENNA_AZIMUTH','rotation','y')]
assert len(PV)==76,len(PV)
PIVOTS=[dict(Pivot='ANIM_'+name,Index=i,Model=NAMES[i],Kind=kind,Axis=axis) for i,name,kind,axis in PV]
PIVOT_POS={p['Pivot']:P[p['Index']] for p in PIVOTS}
PIVOT_POS['ANIM_LOCKING_PIN_A']=(-.6,6.45,-.98);PIVOT_POS['ANIM_LOCKING_PIN_B']=(.6,6.45,-.98)
PIVOT_POS['ANIM_TELESCOPE_A_ELEVATION']=(P[12][0],P[12][1]+.85,P[12][2]);PIVOT_POS['ANIM_TELESCOPE_B_ELEVATION']=(P[13][0],P[13][1]+.85,P[13][2])
PIVOT_POS['ANIM_SUN_TRACKER_ELEVATION']=(P[17][0],P[17][1]+.53,P[17][2]);core.PIVOT_POS=PIVOT_POS
PV_BY_MODEL=defaultdict(list)
for p in PIVOTS:PV_BY_MODEL[p['Index']].append(p['Pivot'])

def b(a,p,dim,mat='titanium',pv=None,label='PrecisionPlate'):a.box(p,dim,mat,pv,label)
def cy(a,p,r,h,mat='titanium',axis='y',pv=None,label='PrecisionCylinder',seg=22):a.cylinder(p,r,h,mat,axis=axis,seg=seg,pivot=pv,label=label)
def tor(a,p,R,r,mat='titanium',axis='z',pv=None,label='CNC_Ring',seg=44,tube=8):a.torus(p,R,r,mat,axis=axis,seg=seg,tube=tube,pivot=pv,label=label)
def beam(a,p,q,r=.045,mat='structural',pv=None,label='Stiffener'):a.beam(p,q,r,mat,pivot=pv,label=label,sides=8)
def sphere(a,p,r,mat='solar',pv=None,label='SphericalInstrument',subs=1):
 m=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(m.vertices+np.asarray(p),m.faces,mat,pv,label)
def pipe(a,pts,r=.07,mat='copper',pv=None):
 for q1,q2 in zip(pts,pts[1:]):beam(a,q1,q2,r,mat,pv,'BoltedFluidPressureTransferLine')
 for pp in (pts[0],pts[-1]):cy(a,pp,r*1.5,.065,'gold','y',None,'PipeGasketAndLockring',16)
def frame(a,p,w=1.8,h=2.3,d=.85):
 x,y,z=p
 for sx in (-1,1):b(a,(x+sx*w/2,y,z),(.10,h,d),'structural',None,'ReinforcedRailVertical')
 for sy in (-1,1):b(a,(x,y+sy*h/2,z),(w,.10,d),'titanium',None,'ReinforcedRailHorizontal')
def panel(a,p,size=1,theme='cyan',special='instrument',pv=None):
 core.panel_equipment(a,p,size,theme,special)
 x,y,z=p
 for j in range(8): cy(a,(x+(j-3.5)*.108*size,y-.47*size,z+.43*size),.034*size,.09*size,'gold','z',None,'GoldRimmedInstrumentationConnector',10)
 for j in range(5): b(a,(x-.30+j*.15*size,y+.57*size,z+.20*size),(.095*size,.038*size,.055*size),'structural',None,'ThermalVentGrille')
 if pv:b(a,(x,y+.46*size,z+.45*size),(.31*size,.06*size,.043*size),theme,pv,'MeasuredStatusVisualRigPivot')
def valve(a,p,pv=None):
 x,y,z=p;cy(a,p,.20,.60,'structural','y',None,'MultiportValveHousing')
 tor(a,(x,y+.35,z),.30,.035,'gold','y',pv,'MotorizedSelectorHandwheel',32)
 for theta in [k*math.pi/2 for k in range(4)]:beam(a,(x,y+.35,z),(x+.29*math.cos(theta),y+.35,z+.29*math.sin(theta)),.022,'copper',pv,'ValveWheelSpoke')
 for sx in (-1,1):cy(a,(x+sx*.35,y,z),.20,.12,'titanium','x',None,'ValveManifoldFlange')
def vessel(a,p,r=.62,h=2.65,mat='structural',pv=None,material='water'):
 x,y,z=p;cy(a,p,r,h,mat,'y',None,'PressurisedServiceVessel',36)
 for sy in (-1,1):
  cy(a,(x,y+sy*(h/2-.04),z),r*.99,.19,'titanium','y',None,'VesselStampedEndDome',32)
  tor(a,(x,y+sy*(h/2-.21),z),r+.06,.048,'gold','y',None,'HighIntegrityFlange',40)
 for j in range(12):
  ang=j*math.tau/12
  cy(a,(x+(r+.06)*math.cos(ang),y+h/2-.17,z+(r+.06)*math.sin(ang)),.034,.16,'gold','y',None,'VesselHighDetailFlangeBolt',9)
 b(a,(x,y,z+r+.06),(.48,1.15,.08),'graphite',None,'InstrumentGlassViewportFrame')
 b(a,(x,y,z+r+.11),(.40,1.05,.025),'glass',None,'TankSightGlass')
 for j in range(12):b(a,(x,y-.40+j*.073,z+r+.135),(.28,.025,.024),material if j<8 else 'gold',pv,'MeasuredWaterInventorySightGauge')
 for sy in (-1,1):cy(a,(x,y+sy*(h/2+.1),z),.19,.40,'gold','y',None,'PressureVesselProcessGland',18)
def finpanel(a,p,wd,ht,pv=None):
 x,y,z=p;b(a,(x,y,z),(wd,ht,.25),'shield',pv,'HydrogenRichShieldCassetteSolidFrame')
 b(a,(x,y,z+.135),(wd-.24,ht-.25,.055),'insulation',pv,'RadiationShieldCassetteInset')
 for k in range(max(4,int(wd/.13))):
  xx=x-wd/2+.13+k*.13
  if xx>x+wd/2-.10:continue
  b(a,(xx,y,z+.17),(.045,ht-.36,.05),'titanium',pv,'ShieldCassetteVerticalRib')
 for j in range(6):
  yy=y-ht/2+.23+j*(ht-.46)/5
  b(a,(x,yy,z+.21),(wd-.3,.030,.025),'gold' if j in (0,5) else 'blue',pv,'ShieldingSegmentInspectionLine')
 for sy in (-1,1):b(a,(x,y+sy*ht/2,z),(.5,.12,.38),'gold',pv,'ArmoredCassetteEndLock')
def telescope(a,p,pv1=None,pv2=None):
 x,y,z=p
 b(a,(x,y-1.05,z),(1.18,.23,1.14),'structural',None,'SensorTurretBoltedPedestal')
 cy(a,(x,y-.47,z),.32,1.15,'titanium','y',pv1,'AzimuthGimbalServoAxis',28)
 tor(a,(x,y+.21,z),.55,.065,'gold','y',pv1,'AzimuthPositionEncoderRing')
 cy(a,(x,y+.72,z),.31,1.18,'structural','x',pv2,'OpticalElevationRotationPin',26)
 cy(a,(x,y+.93,z-.53),.43,2.15,'ceramic','z',pv2,'CollimatedParticleSensorTelescopeBody',34)
 for zz in [-1.52,-1.05,-.54]:tor(a,(x,y+.93,z+zz),.44,.055,'titanium','z',pv2,'TelescopeMultiLayerShieldRings',40)
 cy(a,(x,y+.93,z-1.66),.33,.09,'glass','z',pv2,'DetectorProtectiveSensorWindow',40)
 for j in range(6):
  t=j*math.tau/6
  beam(a,(x+.47*math.cos(t),y+.93+.47*math.sin(t),z-.43),(x+.47*math.cos(t),y+.93+.47*math.sin(t),z-1.54),.032,'gold',pv2,'CollimatorPrecisionTruss')

def make_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i]; pv=PV_BY_MODEL[i][0] if PV_BY_MODEL[i] else None
 if i==0:
  b(a,(0,-.16,.3),(32.5,.46,25.5),'floor',None,'ArmorDeckWithSubstructure')
  for j in range(21):b(a,(-15.7+j*1.55,.095,1.1),(.026,.028,24),'graphite',None,'SegmentedAntiSlipFloorTile')
  for j in range(17):b(a,(0,.095,-11.2+j*1.44),(31.0,.035,.035),'structural',None,'RaisedServiceGrilleTrack')
  for side in (-1,1):
   b(a,(side*15.90,.11,.1),(.17,.3,24.3),'gold',None,'HighVisibilitySafetyBoundary')
   for j in range(14):b(a,(side*15.4,.104,-10.9+j*1.68),(.40,.038,.20),'amber',None,'NonSlipHazardStripe')
  for k in range(9):
   bx=-13.8+k*3.45
   b(a,(bx,.11,7.9),(.18,.09,8.2),'titanium',None,'FlexibleInstrumentCableContainmentRail')
 elif i==1:
  b(a,(0,7.4,-15.1),(32.0,12.0,.18),'glass',None,'PanoramicProtectedSpaceWeatherGlass')
  for j in range(14):
   xx=-15.6+j*2.42
   beam(a,(xx,1.1,-15),(xx,14.1,-15),.072,'titanium',None,'ReinforcedViewportMullion')
  for k in range(4):
   yy=1.1+k*4.1;beam(a,(-16,yy,-15),(16,yy,-15),.080,'structural',None,'InterlockedViewingGlassHorizontalTruss')
  for j in range(90):
   xx=random.uniform(-15,15); yy=random.uniform(1,15)
   b(a,(xx,yy,-15.21),(.015,.015,.013),'stars',None,'ScatteredExteriorReferenceStar')
  for k in range(8):
   xx=-13.7+k*3.90
   b(a,(xx,14.1,-14.8),(.36,.14,.43),'amber',None,'ViewportStormAlertLightUnit')
 elif i==2:
  # Giant distant rotating habitat reference: physical truss, solar assemblies, sector windows and hub.
  tor(a,(x,y,z),5.5,.28,'titanium','z',None,'OrbitalHabitatOuterTorus',90,11)
  tor(a,(x,y,z+.35),4.9,.20,'structural','z',None,'OrbitalHabitatInnerTruss',90,9)
  tor(a,(x,y,z+.62),3.8,.045,'cyan','z',None,'HabitatLifeSupportMarker',82)
  cy(a,(x,y,z+.15),.85,2.4,'titanium','z',None,'RotatingHabitatStationCentralHub',40)
  for j in range(10):
   ang=j*math.tau/10
   beam(a,(x+.72*math.cos(ang),y+.72*math.sin(ang),z+.19),(x+5.2*math.cos(ang),y+5.2*math.sin(ang),z+.19),.11,'structural',None,'HabitatStructuralSpoke')
   for q in (0.78,1.12):
    b(a,(x+5.0*math.cos(ang),y+5.0*math.sin(ang),z+q),(.30,.25,.27),'gold',None,'OrbitalHabitatWindowCluster')
  for j in range(13):
   ang=j*math.tau/13
   b(a,(x+5.5*math.cos(ang),y+5.5*math.sin(ang),z+.22),(.32,.14,.36),'ceramic',None,'SegmentedHabitatPressureBlock')
 elif i==3:
  # Multi-faceted storm shelter tunnel with visible armor and reinforcement.
  for dep in (-4.4,-2.4,-.4):
   for k in range(12):
    a1=k*math.tau/12;a2=(k+1)*math.tau/12
    pt1=(3.15*math.cos(a1),3.85+3.15*math.sin(a1),dep)
    pt2=(3.15*math.cos(a2),3.85+3.15*math.sin(a2),dep)
    beam(a,pt1,pt2,.18,'shield',None,'StormShelterInterlockingBulkheadSegment')
   tor(a,(0,3.85,dep),2.93,.11,'gold','z',None,'RadiationShelterPressureHullLockingRing',64)
  for k in range(18):
   th=k*math.tau/18
   p1=(3.1*math.cos(th),3.85+3.1*math.sin(th),-4.4)
   p2=(3.1*math.cos(th),3.85+3.1*math.sin(th),-.45)
   beam(a,p1,p2,.10,'titanium',None,'ShelterArmorLongitudinalGirder')
  for side in (-1,1):
   b(a,(side*3.0,3.8,-3),(1.0,5.3,3.2),'shield',None,'ShelterPressureContainmentLateralSidewall')
  for k in range(9):b(a,(-2.4+k*.6,1.1,-3.1),(.30,.07,4.2),'insulation',None,'ShelterCompositeDeckPlating')
 elif i==4:
  tor(a,(x,y,z),2.72,.20,'titanium','z',None,'ShelterMainHatchCircularHousing',72)
  cy(a,(x,y,z+.15),2.35,.30,'shield','z',pv,'ShelterSealedBulkheadArmorLeaf',64)
  tor(a,(x,y,z+.41),1.8,.13,'structural','z',pv,'PoweredPressureHatchDynamicSeal',60)
  for j in range(12):
   t=j*math.tau/12
   cy(a,(x+2.45*math.cos(t),y+2.45*math.sin(t),z+.42),.07,.22,'gold','z',None,'MultiPointHatchLockActuator',10)
  b(a,(x,y,z+.52),(1.10,.11,.09),'red',pv,'TrustedCloseBeforeStormVisualIndicator')
 elif i==5:
  tor(a,P[i],2.7,.07,'orange','y',pv,'StormForecastCelestialHologramRing',76)
  for j in range(11):
   t=j*math.tau/11
   tor(a,(x+1.65*math.cos(t),y+.22*math.sin(t),z+1.0*math.sin(t)),.25,.032,'amber','z',pv,'StormWeatherEnergyBandMarker',24)
   sphere(a,(x+2.15*math.cos(t),y+2.15*math.sin(t),z),.09,'solar',pv,'MovingParticleFluxIndicator')
  sphere(a,(x,y,z),.70,'storm',None,'EnergySourceSolarSphere',2)
  for j in range(15):
   t=j*math.tau/15
   beam(a,(x+.8*math.cos(t),y+.8*math.sin(t),z),(x+2.2*math.cos(t+.24),y+2.2*math.sin(t+.24),z-.3),.020,'orange',pv,'SolarStormRadialVectorRays')
 elif i==6:
  # Secondary exterior window structure with structural reinforced glass curvature proxy.
  for j in range(9):
   xx=-12+j*3
   beam(a,(xx,4,-14.8),(xx,13.2,-14.8),.070,'structural',None,'ExteriorProtectedGlassPaneFrame')
  for j in range(5):
   yy=4+j*2.3
   beam(a,(-12.5,yy,-14.8),(12.5,yy,-14.8),.07,'titanium',None,'ExteriorWindowPressureBar')
  for j in range(8):b(a,(-10.5+j*3,5.4,-14.7),(2.6,2.1,.055),'glass',None,'ProtectedLaminatedObservationWindow')
 elif i==7:
  b(a,(0,.30,-3.3),(8.4,.38,8.8),'structural',None,'RefugeDeckPressureHullFloor')
  for k in range(10):b(a,(-3.75+k*.84,.58,-3.0),(.035,.035,7.5),'gold',None,'ShelterDeckMaintenanceServiceGroove')
  for side in (-1,1):
   for j in range(5):
    xx=side*3.5;zz=-6+j*1.2
    b(a,(xx,1.15,zz),(.65,1.25,.86),'ceramic',None,'CrewShelterEnvironmentalCabinet')
 elif i==8:
  for side in (-1,1):
   for j in range(10):
    zz=-6.3+j*.62
    beam(a,(side*3.4,1.0,zz),(side*3.4,6.9,zz),.065,'titanium',None,'ShieldSupportVerticalRail')
  for y0 in (1.0,6.9):
   beam(a,(-3.4,y0,-6.3),(3.4,y0,-6.3),.09,'gold',None,'HydrogenRichMassShieldStructuralRail')
  for j in range(8):b(a,(-3.5+j,7.15,-3.3),(.23,.11,7.0),'structural',None,'ShelterMassPanelSlingTruss')
 elif i==9:
  tor(a,(x,y,z),1.37,.14,'titanium','z',None,'ShelterEmergencyEgressProtectiveFrame',48)
  cy(a,(x,y,z+.20),1.1,.18,'shield','z',pv,'EmergencySecondaryShieldHatch',42)
  tor(a,(x,y,z+.32),.86,.048,'red','z',pv,'EgressDoorRedStatusSealingRing',40)
 elif i==10:
  b(a,(x,y-1.7,z),(4.6,.20,4.0),'structural',None,'ExternalInstrumentDeck')
  for j in range(5):
   px=x-2.15+j*1.07
   beam(a,(px,y-1.6,z-2),(px,y-1.6,z+2),.049,'titanium',None,'PrecisionInstrumentPlatformRib')
  for sz in (-1,1):
   beam(a,(x-2.25,y-.60,z+sz*1.9),(x+2.25,y-.60,z+sz*1.9),.05,'gold',None,'ObservatoryPerimeterHandRail')
  for j in range(6):beam(a,(x-2.1+j*.84,y-1.6,z-1.9),(x-2.1+j*.84,y-.5,z-1.9),.046,'gold',None,'EVAPlatformGuardPost')
 elif i==11:
  b(a,(x,y-1.2,z),(4.0,.24,3.5),'structural',None,'UtilitySkidBase')
  for q in range(7):
   xx=x-1.5+q*.50
   pipe(a,[(xx,y-.5,z-1.3),(xx,y+.6,z-1.3),(xx,y+.6,z+.9)],.05,'copper')
  for j in range(4):cy(a,(x-1.2+j*.78,y+.6,z+.8),.17,.22,'gold','z',None,'WaterShieldUtilityNetworkFlangedPort')
 elif i in (12,13):
  pvs=PV_BY_MODEL[i];telescope(a,P[i],pvs[0],pvs[1])
 elif i==17:
  telescope(a,(x,y,z),pv,PV_BY_MODEL[i][1])
  for j in range(8):
   t=j*math.tau/8
   cy(a,(x+.71*math.cos(t),y+1.5+.71*math.sin(t),z-.3),.065,.2,'gold','z',None,'SunVectorStageIndexPin')
 elif i==18:
  cy(a,(x,y,z),.65,.7,'structural','z',None,'BeamCollimatorMechanicalBody',40)
  tor(a,(x,y,z+.44),.52,.09,'gold','z',None,'ParticleCollimatorOuterGuideRing',40)
  for j in range(12):
   th=j*math.tau/12
   b(a,(x+.33*math.cos(th),y+.33*math.sin(th),z+.48),(.15,.32,.06),'insulation',pv,'CollimatorSteppingBlade')
 elif i in (14,15,16):
  frame(a,P[i],1.55,2.0,.8)
  for j in range(5):
   tor(a,(x,y-.55+j*.27,z+.4),.38,.053,'titanium','z',pv if i!=14 else None,'ParticleEnergyTrackingDetectorAperture',30)
   cy(a,(x,y-.55+j*.27,z+.43),.09,.09,'cyan' if j%2 else 'gold','z',None,'EnergyChannelInstrumentReadoutSensor',12)
  b(a,(x,y+.89,z+.44),(1.05,.07,.07),'orange',pv,'DetectorEnergyBinSweepIndicator')
 elif i in (19,20,21,22,23,24,31,33,34,35,37,38,44,48,49,50,51,62,63,64,65,66,71,72,73,74,75,76,77,78,79,80,81,85,86,87,89):
  kind='fault' if i in (33,64,76) else 'plc' if i in (35,50,63,71,72,73,77,79,80,81) else 'optical' if i in (19,20,22,44,65) else 'instrument'
  theme='red' if i in (33,64,76,78) else 'amber' if i in (38,50,65,77) else 'cyan'
  panel(a,P[i],1.10,theme,kind,pv)
  if i in (19,20):
   cy(a,(x+.13,y+.18,z+.6),.16,.33,'glass','z',None,'DetectorCountingPulseChamber',20)
   for j in range(5):cy(a,(x-.23+j*.105,y-.09,z+.58),.027,.13,'gold','z',None,'PhotomultiplierCounterOutputConnector',10)
  if i==22:
   tor(a,(x+.06,y+.24,z+.6),.21,.045,'violet','z',pv,'FictionalReferenceCalibrationSourceGlow',28)
  if i==44:
   for k in range(5):
    b(a,(x-.35+k*.175,y+.31,z+.47),(.095,.37,.09),'glass',pv,'TransparentWaterLevelSensorChannel')
  if i==65:
   b(a,(x+.29,y-.28,z+.54),(.12,.20,.10),'red',pv,'IndependentDoorOpenContactStatus')
  if i==78:
   cy(a,(x,y+.13,z+.51),.22,.20,'red','z',pv,'EmergencyIsolationMushroomPushButton',24)
  if i==86:
   for j in range(4):
    cy(a,(x-.34+j*.22,y-.40,z+.52),.077,.10,'gold','z',None,'W149PhysicalCriticalFeedbackPatchJack',18)
   b(a,(x,y+.45,z+.52),(.42,.13,.08),'red',pv,'W149CriticalFeedbackLostIndicator')
 elif i in (25,36,67,83):
  rad=2.3 if i in (36,83) else 1.3
  tor(a,(x,y,z),rad,.052,'cyan' if i!=83 else 'orange','z',pv,'ThreeDimensionalStormVisualizationRing',72)
  tor(a,(x,y,z+.15),rad*.70,.035,'amber','y',pv,'StormDirectionalFluxDetectionArc',56)
  for j in range(14):
   ang=j*math.tau/14
   px=x+rad*.85*math.cos(ang);py=y+rad*.85*math.sin(ang)
   beam(a,(x,y,z),(px,py,z+.2*math.sin(3*ang)),.016,'orange' if i==83 else 'cyan',pv,'HolographicRadialRadiationVector')
   sphere(a,(px,py,z+.27),.085,'amber',pv,'MeasuredFluxSegmentPoint')
 elif i in (26,27,28,29,30):
  frame(a,P[i],1.55,2.0,.75)
  cy(a,(x,y+.10,z+.36),.45,.30,'structural','z',None,'RadiationDosimeterSensorBody',28)
  tor(a,(x,y+.10,z+.56),.38,.045,'gold','z',None,'IndependentDosimeterDetectorSeal',36)
  cy(a,(x,y+.10,z+.58),.27,.11,'glass','z',None,'DoseSensorActiveWindow',30)
  for j in range(12):
   a1=j*math.tau/12
   cy(a,(x+.37*math.cos(a1),y+.10+.37*math.sin(a1),z+.65),.038,.09,'gold','z',None,'DosimeterSensorGuardBolt',9)
  b(a,(x,y-.58,z+.5),(.76,.12,.11),'red' if i==28 else 'green',pv,'ActualRadiationMeasurementStatusBar')
  if i==28: b(a,(x,y+.75,z+.46),(.80,.13,.12),'gold',None,'PrimaryTrustedW149SourceInstrumentMarker')
 elif i==32:
  frame(a,P[i],2.15,2.6,1.0)
  tor(a,(x,y,z+.55),.72,.06,'gold','z',pv,'DosimeterPrecisionCalibrationReferenceFrame',48)
  for j in range(6):
   a1=j*math.tau/6
   cy(a,(x+.62*math.cos(a1),y+.62*math.sin(a1),z+.56),.093,.24,'cyan','z',pv,'DosimeterCalibrationMountedProbe',12)
 elif i in (39,40,41,42):
  # Transparent hydrogen-rich water modules arranged as actual separate shield tanks.
  sy=2.85;h=3.7
  b(a,(x,y,z),(.94,h,.72),'water',None,'ProtectedWaterWallTransparentTank')
  b(a,(x,y,z+.39),(1.10,h+.14,.13),'structural',None,'WaterWallPressureContainmentHatch')
  b(a,(x,y,z+.48),(.76,h-.27,.035),'glass',None,'WaterShieldFillLevelObservationWindow')
  for j in range(18):
   yy=y-h*.44+j*h*.053
   b(a,(x,yy,z+.52),(.62,.033,.023),'cyan' if j<12 else 'amber',pv,'MeasuredShieldWaterMassSegment')
  for sy in (-1,1):b(a,(x,y+sy*h/2,z),(1.08,.13,.91),'gold',None,'WaterShieldHydraulicStructuralEndCap')
  for j in range(4):
   b(a,(x+(-.39 if j%2 else .39),y+(-1.2 if j<2 else 1.2),z+.45),(.10,.13,.12),'gold',None,'TankModuleQuickLockingStud')
 elif i==43:
  vessel(a,P[i],.89,4.2,'structural',pv,'water')
  for sy in (-1,1):b(a,(x+sy*.87,y-1.65,z),(.13,.30,1.4),'gold',None,'WaterReserveCradleSupport')
 elif i in (45,46):
  frame(a,P[i],2.15,2.0,1.0)
  cy(a,(x,y,z+.35),.62,.43,'structural','z',None,'HighPressureWaterTransferPumpHousing',34)
  tor(a,(x,y,z+.58),.50,.06,'gold','z',None,'PumpSealInspectionRing',40)
  cy(a,(x,y,z+.60),.19,.20,'copper','z',pv,'PumpActualRotorShaft',22)
  for j in range(12):
   th=j*math.tau/12
   beam(a,(x,y,z+.62),(x+.45*math.cos(th),y+.45*math.sin(th),z+.62),.029,'titanium',pv,'RadialPumpMeasuredFlowImpeller')
  pipe(a,[(x-1.1,y,z+.1),(x-.62,y,z+.1)],.08,'water')
  pipe(a,[(x+.62,y,z+.1),(x+1.1,y,z+.1)],.08,'water')
 elif i==47:
  frame(a,P[i],2.3,2.3,1.1)
  for j in range(5):
   px=x+(j-2)*.41
   valve(a,(px,y+.07,z+.30),pv if j==2 else None)
  for j in range(3):pipe(a,[(x-1.25,y-.65+j*.32,z),(x+1.25,y-.65+j*.32,z)],.042,'water')
 elif i in (52,53,54,55):
  dim=(6.25,1.3) if i in (52,53) else (1.22,5.85)
  finpanel(a,P[i],*dim,pv)
 elif i==56:
  for sx in (-1,1):
   for sy in (-1,1):
    beam(a,(sx*3.4,sy*3.3,-6.3),(sx*3.4,sy*3.3,-.75),.12,'titanium',None,'ShieldCassetteLongTravelServoRail')
  for j in range(10):
   zz=-6.2+j*.56
   for sx in (-1,1):cy(a,(sx*3.4,3.85,zz),.14,.12,'gold','z',None,'RollerShieldCarriageGuideWheel',16)
  b(a,(0,7.25,-.7),(5.8,.18,.50),'gold',pv,'ActualShieldCarriageLockCrossbar')
 elif i in (57,58,59,60):
  frame(a,P[i],1.52,2.2,.8)
  cy(a,(x,y-.25,z+.21),.27,1.4,'titanium','y',None,'ElectromechanicalLinearActuatorCylinder',24)
  cy(a,(x,y+.5,z+.21),.11,.66,'copper','y',pv,'ServoMotorActualPositionRod',20)
  for j in range(5):
   b(a,(x-.54+j*.27,y-.9,z+.3),(.11,.08,.20),'gold' if j%2 else 'cyan',None,'RealizedMotionFeedbackMarker')
  tor(a,(x,y+.83,z+.21),.21,.044,'gold','y',pv,'ShieldActuatorAbsoluteEncoderRing',26)
 elif i==61:
  b(a,(x,y,z),(3.1,.23,.47),'structural',None,'ShieldLockCrossbar')
  for j in range(8):
   xx=x-1.4+j*.40
   pid='ANIM_LOCKING_PIN_A' if j<4 else 'ANIM_LOCKING_PIN_B'
   cy(a,(xx,y-.28,z+.25),.11,.50,'gold','z',pid,'ActualMechanicalLockingPin',18)
   tor(a,(xx,y-.28,z+.51),.14,.032,'green','z',pid,'LockMechanicalFeedbackRing',20)
 elif i==68:
  frame(a,P[i],2.6,2.75,1)
  tor(a,(x,y,z+.42),.96,.12,'structural','z',None,'ShelterCirculationFanBlowerFrame',52)
  cy(a,(x,y,z+.53),.19,.35,'titanium','z',pv,'ShelterVentilationHub',24)
  for j in range(10):
   t=j*math.tau/10
   beam(a,(x+.18*math.cos(t),y+.18*math.sin(t),z+.56),(x+.86*math.cos(t+.32),y+.86*math.sin(t+.32),z+.56),.115,'titanium',pv,'AerodynamicShelterSupplyRotorBlade')
 elif i==69:
  vessel(a,P[i],.62,2.8,'structural',pv,'cyan')
 elif i==70:
  frame(a,P[i],2.3,2.5,.9)
  for j in range(15):
   xx=x-1.0+j*.135
   b(a,(xx,y,z+.37),(.055,1.96,.12),'copper',None,'ThermalShelterHeatReclaimerRadiatorFins')
  valve(a,(x,y-.67,z+.54),pv)
 elif i==82:
  b(a,(0,8.5,-8.8),(7.5,2.9,.12),'structural',None,'SpaceWeatherSCADAHolographicMonitorFrame')
  b(a,(0,8.5,-8.67),(7.0,2.6,.05),'panel',None,'RadiationDefenseMissionSCADADisplayGlass')
  for j in range(5):
   xx=-2.78+j*1.39
   tor(a,(xx,8.8,-8.56),.43,.039,'cyan' if j<3 else 'amber','z',pv,'MeasuredExternalShelterDosimetryGauge',38)
  for j in range(16):
   xx=-3.2+j*.43
   b(a,(xx,7.65+(j*11%8)*.048,-8.55),(.15,.024,.03),'green' if j%3 else 'red',None,'TimeResolvedStormSpectralHistogram')
 elif i==84:
  # Exterior radiation flux field decorative emission beacon, not physically emitted particle/radiation.
  for j in range(14):
   ang=j*math.tau/14
   beam(a,(x,y,z),(x+2.9*math.cos(ang),y+1.2*math.sin(ang),z-.2),.019,'orange',None,'DecorativeStormVectorRay')
   sphere(a,(x+2.8*math.cos(ang),y+1.18*math.sin(ang),z-.25),.09,'solar',None,'DecorativeParticleRenderingNode')
 elif i==88:
  tor(a,(x,y,z),1.15,.084,'gold','z',None,'FinalRadiationProtectionCertificationHalo',58)
  tor(a,(x,y,z+.14),.93,.046,'cyan','z',pv,'MissionQualificationStatusOuterGlow',52)
  b(a,(x,y,z+.22),(1.24,.16,.055),'green',pv,'MissionRadiationQualifiedCenterEmitter')
 elif i==90:
  cy(a,(x,y,z),.34,.77,'structural','z',None,'InspectionCameraPanTiltBase',28)
  cy(a,(x,y,z+.41),.22,.28,'glass','z',None,'InspectionPTZOpticalDome',28)
  for j in range(5):b(a,(x-.42+j*.20,y+.28,z+.35),(.12,.08,.10),'gold',None,'InspectionEquipmentServicePort')
 elif i==91:
  cy(a,(x,y-.8,z),.30,1.6,'titanium','y',None,'SpaceWeatherSurveyDishPedestal',20)
  tor(a,(x,y+.45,z-.10),1.38,.075,'titanium','z',None,'SurveyDirectionalAntennaReflectorRim',56)
  for j in range(16):
   t=j*math.tau/16
   beam(a,(x,y+.45,z+.16),(x+1.38*math.cos(t),y+.45+1.38*math.sin(t),z-.10),.026,'structural',None,'SurveyDishSupportSpoke')
  cy(a,(x,y+.45,z+.55),.19,.45,'gold','z',None,'RadiationObservatoryTelemeteredFeedHorn',24)
 else:
  panel(a,P[i],1.0,'cyan','instrument',pv)
 # Named pivots must own actual mesh vertices and not merely exist as empty transforms.
 owned={pi for _,_,_,pi,_ in a.raw if pi}
 for k,pid in enumerate(PV_BY_MODEL[i]):
  if pid not in owned:
   px,py,pz=PIVOT_POS[pid]
   b(a,(px,py+.46+k*.055,pz+.44),(.17,.05,.055),'cyan' if ('STATUS' in pid or 'MONITOR' in pid) else 'gold',pid,'ActualMeshOwnedAnimationPivotStatus')
 return a
# A concrete authoring plan, intentionally NOT preconnected SparkTerminal wires.
GROUP_CONFIG=[
 ('Educational 5 V control and independent returns',12,'POWER_CONTROL',[
  (79,80,'Protected 5 V radiation safety PLC supply'),(79,81,'Protected 5 V sequencing PLC supply'),
  (79,35,'Radiation detector network logic supply'),(79,63,'Motor-shield cabinet control supply'),
  (79,71,'Shelter ventilation safety control supply'),(79,82,'SCADA logic supply'),
  (79,28,'Primary shelter dosimeter signal conditioning'),(79,29,'Independent shelter dosimeter conditioning'),
  (79,48,'Water-shield distribution controller power'),(79,24,'Forecast instrument control power'),
  (80,79,'Safety PLC protected control return'),(81,79,'Mission PLC protected control return')]),
 ('Particle detection and space-weather acquisition',26,'ANALOG_FEEDBACK',[
  (12,35,'Primary telescope measured particle count'),(13,35,'Redundant telescope measured particle count'),
  (14,15,'Charged particle spectrometer energy channel'),(15,35,'Proton energy-bin spectral feedback'),
  (16,35,'Heavy-ion distribution health'),(17,81,'Sun tracker measured actual azimuth'),
  (18,14,'Collimator aperture measured state'),(19,35,'Primary particle-counter histogram'),
  (20,35,'Secondary particle-counter histogram'),(21,35,'Pulse shaping instrument actual signal quality'),
  (22,35,'Calibrator spectral reference'),(23,35,'Detector thermal shroud sensed state'),
  (35,24,'Aggregated measured solar particle flux'),(24,81,'Qualified space-weather forecasting diagnostic'),
  (25,82,'Forecast-time visualization telemetry'),(33,35,'Detector overrange diagnostic'),
  (81,17,'Solar gimbal tracking setpoint','DIGITAL_CONTROL'),(17,18,'Measured tracking collimator alignment'),
  (81,12,'Enable primary solar particle telescope','DIGITAL_CONTROL'),(81,13,'Enable redundant solar particle telescope','DIGITAL_CONTROL'),
  (81,22,'Request calibration reference','DIGITAL_CONTROL'),(81,19,'Primary counter acquisition gate','DIGITAL_CONTROL'),
  (81,20,'Secondary counter acquisition gate','DIGITAL_CONTROL'),(35,37,'Redundant particle-counter consistency'),
  (37,81,'Measurement health and confidence qualification'),(35,83,'Measured space-weather visualization only')]),
 ('Distributed radiation sensing and calibration',24,'ANALOG_FEEDBACK',[
  (26,35,'Exterior dosimetry measured rate'),(27,35,'Habitat compartment measured dose-rate'),
  (28,80,'Shelter local non-authoritative commissioning measurement'),(29,80,'Independent shelter radiation safety crosscheck'),
  (30,35,'Triaxial reference instrument measured field proxy'),(31,35,'Dose-rate transient recorder'),
  (32,28,'Dosimeter fictional calibration reference'),(32,29,'Independent dosimeter calibration reference'),
  (34,38,'Run-scoped accumulated shelter dose'),(35,37,'Spectral acquisition repeatability'),
  (37,38,'Validated instrument-fusion confidence'),(38,80,'Dose-budget exceedance risk to protection'),
  (26,36,'External dose-area heatmap visualization'),(27,36,'Habitat dose-area heatmap visualization'),
  (28,82,'Display-only shelter dose telemetry'),(29,82,'Display-only independent shelter reading'),
  (81,32,'Initiate dosimeter calibration','DIGITAL_CONTROL'),(81,31,'Capture transient radiation trace','DIGITAL_CONTROL'),
  (81,30,'Select triaxial dosimeter scan mode','DIGITAL_CONTROL'),(35,34,'Validated dose-rate integration samples'),
  (34,82,'Accumulated mission exposure visualization'),(38,81,'Qualified shelter dose budget assessment'),
  (33,81,'Detector overload fault report'),(81,33,'Activate deterministic detector saturation test','DIGITAL_CONTROL')]),
 ('Hydrogen-rich water wall and finite inventory',22,'DIGITAL_CONTROL',[
  (48,39,'Port A tank shield allocation command'),(48,40,'Port B tank shield allocation command'),
  (48,41,'Starboard A tank shield allocation command'),(48,42,'Starboard B tank shield allocation command'),
  (44,48,'Actual compartment water-level distribution','ANALOG_FEEDBACK'),
  (43,44,'Actual main water inventory','ANALOG_FEEDBACK'),
  (48,45,'Port water transfer pump start'),(48,46,'Starboard water transfer pump start'),
  (45,48,'Measured transfer pump A flow','ANALOG_FEEDBACK'),(46,48,'Measured transfer pump B flow','ANALOG_FEEDBACK'),
  (48,47,'Water distributor valve requested position'),(47,48,'Distributor actual valve position','ANALOG_FEEDBACK'),
  (39,49,'Port A measured shielding areal density','ANALOG_FEEDBACK'),
  (40,49,'Port B measured shielding areal density','ANALOG_FEEDBACK'),
  (41,49,'Starboard A measured shielding areal density','ANALOG_FEEDBACK'),
  (42,49,'Starboard B measured shielding areal density','ANALOG_FEEDBACK'),
  (49,77,'Actual mass-cover protection evaluation','ANALOG_FEEDBACK'),
  (50,48,'Water / life support budget permit','SAFETY_FEEDBACK'),
  (51,48,'Water-wall leak inspection','ANALOG_FEEDBACK'),
  (71,50,'Life-support resource reserve constraint','SAFETY_FEEDBACK'),
  (48,73,'Real resource allocation and transfer status','ANALOG_FEEDBACK'),
  (81,48,'Approved shielding resource optimization request')]),
 ('Motorized shield barriers and position feedback',22,'DIGITAL_CONTROL',[
  (63,57,'Port cassette servo A drive command'),(63,58,'Upper cassette servo B drive command'),
  (63,59,'Lower cassette servo C drive command'),(63,60,'Starboard cassette servo D drive command'),
  (57,62,'Servo A actual carriage position','ANALOG_FEEDBACK'),
  (58,62,'Servo B actual carriage position','ANALOG_FEEDBACK'),
  (59,62,'Servo C actual carriage position','ANALOG_FEEDBACK'),
  (60,62,'Servo D actual carriage position','ANALOG_FEEDBACK'),
  (62,77,'Actual shield lockable pose qualification','ANALOG_FEEDBACK'),
  (63,52,'Deploy top radiation shield cassette'),(63,53,'Deploy bottom radiation shield cassette'),
  (63,54,'Deploy port side radiation shield cassette'),(63,55,'Deploy starboard radiation shield cassette'),
  (52,66,'Actual top cassette coverage','ANALOG_FEEDBACK'),
  (53,66,'Actual bottom cassette coverage','ANALOG_FEEDBACK'),
  (54,66,'Actual port cassette coverage','ANALOG_FEEDBACK'),
  (55,66,'Actual starboard cassette coverage','ANALOG_FEEDBACK'),
  (77,61,'Authorize mechanical shield locking'),(61,77,'Mechanical lock engagement sensed','SAFETY_FEEDBACK'),
  (64,63,'Shield jamming actual fault state','SAFETY_FEEDBACK'),
  (81,64,'Inject deterministic shield carriage jam'),
  (66,67,'Coverage summary into 3D shield twin','ANALOG_FEEDBACK')]),
 ('Storm forecasting and essential-system continuity',22,'ANALOG_FEEDBACK',[
  (24,75,'Space weather forecast configuration','DIGITAL_CONTROL'),
  (75,81,'Actual simulated particle event telemetry'),
  (25,81,'Predicted storm onset window'),
  (49,73,'Current shielding mass and reserve margin'),
  (50,73,'Water inventory essential-utility constraints'),
  (68,71,'Actual shelter ventilation airflow'),
  (69,71,'Available oxygen buffer and partial pressure'),
  (70,71,'Shelter thermal recovery status'),
  (71,80,'Independent life-support permissive state','SAFETY_FEEDBACK'),
  (72,71,'Essential shelter power distribution health','SAFETY_FEEDBACK'),
  (73,72,'Protected essential load schedule','DIGITAL_CONTROL'),
  (74,73,'Occupancy-driven essential demand'),
  (75,38,'Measured simulated storm intensity history'),
  (76,81,'High-energy surge event confirmation'),
  (81,76,'Arm and inject higher-energy event','DIGITAL_CONTROL'),
  (81,74,'Start qualified simulated shelter occupation','DIGITAL_CONTROL'),
  (81,68,'Enable shelter ventilation','DIGITAL_CONTROL'),
  (81,70,'Enable controlled thermal recovery','DIGITAL_CONTROL'),
  (77,81,'Calculated shielding and shelter permit','SAFETY_FEEDBACK'),
  (81,85,'Generate contingency response plan','DIGITAL_CONTROL'),
  (85,81,'Contingency handling diagnostic'),
  (75,83,'Solar storm holographic event status')]),
 ('Independent safety critical feedback and fault handling',26,'SAFETY_FEEDBACK',[
  (65,80,'Actual shelter door closed and latched'),(61,80,'Actual shield locking contact'),
  (66,80,'Shield coverage hardware health'),(29,80,'Independent radiation dose monitoring'),
  (71,80,'Independent life support continuity'),(72,80,'Essential shelter electrical power status'),
  (77,80,'Shielding adequacy permit'),(78,80,'Emergency shelter stop input'),
  (80,63,'Protected shield servo authorization','DIGITAL_CONTROL'),(80,81,'Safety PLC overall release'),
  (80,77,'Independent shielding enable request','DIGITAL_CONTROL'),
  (80,71,'Emergency ventilation continuity permit','DIGITAL_CONTROL'),
  (80,78,'Emergency isolation status request','DIGITAL_CONTROL'),
  (80,38,'Qualified sheltered dose ceiling permission'),
  (80,85,'Protected contingency power-mode command','DIGITAL_CONTROL'),
  (87,80,'Fault recorder event timestamp'),(80,87,'Latch safety trip event','DIGITAL_CONTROL'),
  (80,86,'Critical W149 patch circuit diagnostic'),
  (81,80,'Operator explicit protection reset request','DIGITAL_CONTROL'),
  (80,81,'Fault-latch event status'),
  (80,82,'Qualified health indicator to SCADA'),
  (77,67,'Current shield coverage qualification'),
  (80,89,'Emergency crew warning authorisation'),
  (89,80,'Emergency communication equipment status'),
  (80,88,'Authorization to show final certification','DIGITAL_CONTROL'),
  (75,80,'Monitored actual storm state independent verification')]),
 ('SCADA data, diagnostics and qualification',16,'ANALOG_FEEDBACK',[
  (19,82,'Live primary particle detection trend'),(20,82,'Live secondary particle trend'),
  (34,82,'Dose integral run history'),(38,82,'Exposition budget and guard-band data'),
  (44,82,'Finite water shield mass visualization'),(48,82,'Water transfer operation status'),
  (62,82,'Four shield encoders and lock positions'),(66,82,'Actual shielding coverage map'),
  (68,82,'Shelter ventilation status'),(71,82,'Life support capability margin'),
  (75,82,'Solar storm timeline and phase'),(77,82,'Independent shield safety health'),
  (80,82,'Authoritative safety latches and validity'),(81,82,'Six authoritative objective progress'),
  (87,82,'Fault trace and diagnostic record'),(82,88,'Read only final certification display request','DIGITAL_CONTROL')])
]
assert sum(g[1] for g in GROUP_CONFIG)==170
WIRES=[];GROUPS=[]
for group_name,ct,kind,raw in GROUP_CONFIG:
 assert len(raw)==ct,(group_name,ct,len(raw))
 first=len(WIRES)+1
 for r in raw:
  a,bidx,description=r[:3];sig=r[3] if len(r)==4 else kind
  wid='W%03d'%(len(WIRES)+1)
  if wid=='W149':a,bidx,description,sig=28,80,'CRITICAL trusted shelter dose-rate measurement to independent radiation safety PLC','ANALOG_FEEDBACK'
  WIRES.append(dict(WireID=wid,Group=group_name,SourceModel=NAMES[a],TargetModel=NAMES[bidx],SignalKind=sig,
    Purpose=description,Critical='YES' if wid=='W149' else 'NO',SourceVisualAnchor='',TargetVisualAnchor='',State='CONCEPT_ONLY_PLAYER_NOT_CONNECTED'))
 GROUPS.append(dict(WireIDs=f'W{first:03d}–W{len(WIRES):03d}',Group=group_name,Count=ct))
assert len(WIRES)==170 and WIRES[148]['SourceModel']==NAMES[28] and WIRES[148]['TargetModel']==NAMES[80]
# 248 primary decorative sockets. Each is a 3D geometry node; real SparkTerminal may need a split logical port.
DEG=Counter()
for w in WIRES:
 DEG[NAMES.index(w['SourceModel'])]+=1;DEG[NAMES.index(w['TargetModel'])]+=1
counts={i:2 for i in range(len(NAMES))}
for i in sorted(range(len(NAMES)),key=lambda i:(-DEG[i],i)):
 if sum(counts.values())>=248:break
 counts[i]+=1
for i in sorted(range(len(NAMES)),key=lambda i:(-DEG[i],i)):
 if sum(counts.values())>=248:break
 counts[i]+=1
assert sum(counts.values())==248
ANCHORS=[];BY_MODEL=defaultdict(list)
for i in range(92):
 x,y,z=P[i];m=counts[i]
 for j in range(m):
  # Fanned gold-rim socket banks on an accessible instrumentation face.
  col=j%5;row=j//5
  xx=x+(col-(min(5,m)-1)/2)*.18;yy=y-.40+row*.20
  zz=z+(.72 if i in (0,1,2,3,4,5,6,7,8,9,10,11,39,40,41,42,43,52,53,54,55,56,57,58,59,60,61) else .58)
  aid='P%03d'%(len(ANCHORS)+1)
  ent=dict(AnchorID=aid,Model=NAMES[i],WorldX_m=round(xx,5),WorldY_m=round(yy,5),WorldZ_m=round(zz,5),Status='MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL')
  ANCHORS.append(ent);BY_MODEL[i].append(ent)
assert len(ANCHORS)==248
LOGICAL=[];usecounter=Counter()
for w in WIRES:
 for side in ('Source','Target'):
  idx=NAMES.index(w[side+'Model']);n=usecounter[idx];usecounter[idx]+=1
  aa=BY_MODEL[idx][(int(w['WireID'][1:])+n*3)%len(BY_MODEL[idx])]
  w[side+'VisualAnchor']=aa['AnchorID']
  port=(w['WireID']+'_'+('OUT' if side=='Source' else 'IN')+'_'+w['SignalKind'])
  if w['WireID']=='W149':port='TRUSTED_SHELTER_DOSE_RATE_OUT' if side=='Source' else 'TRUSTED_SHELTER_DOSE_RATE_IN'
  LOGICAL.append(dict(WireID=w['WireID'],Endpoint=side,Model=w[side+'Model'],ArtAnchor=aa['AnchorID'],
    FunctionalPortID=port,SignalKind=w['SignalKind'],Direction=('OUTPUT' if side=='Source' else 'INPUT'),
    Critical=w['Critical'],FunctionalStatus='PLANNED_NOT_ACTUAL_SPARKTERMINAL'))
assert len(LOGICAL)==340
shared=defaultdict(list)
for x in LOGICAL:shared[x['ArtAnchor']].append(x)
CONFLICTS=[dict(AnchorID=k,Model=v[0]['Model'],LogicalContactCount=len(v),SignalKinds=';'.join(sorted(set(x['SignalKind'] for x in v))),
 DifferentSignalKinds=len(set(x['SignalKind'] for x in v))>1,Requirement='SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT') for k,v in shared.items() if len(v)>1]
OBJ=[
 ('Build the Helios Radiation Defense Network','170 real player-created wires; correct functional returns/terminals and protected 5 V, independent safety including W149','Topology fingerprint and live circuit graph proof'),
 ('Calibrate the Solar Radiation Observatory','External, habitat, shelter and independent dosimeters, two particle telescopes, valid range/reference and fresh samples','Acquisition and calibration record with timestamps, channel health'),
 ('Commission the Radiation Storm Shelter','Finite water allocation and shielding coverage, real actuator positions and locks, life-support and essential power continuity','Mechanical feedback, water inventory, environmental margins and protected readiness'),
 ('Survive the First Complete Solar Storm','Fresh quiet→rise→peak→decay event; measured sheltered dose within fictional budget and essential continuity','New run ID, measured integrated dose and actual protected shelter-state history'),
 ('Recover Three Radiation Defense Failures','Detector saturation, jammed shield cassette and higher-energy storm surge all physically impact plant; repair, then run new qualified storm','Causal disturbance/repair event records and post-disturbance run ID'),
 ('Restore Critical Shelter Radiation Feedback','After historical 1–5 disconnect W149, safety inhibition/latched fault; reconnect, recalibrate, explicit reset, new entire storm','Real W149 live circuit-edge mutation, fresh trusted sample, safety permit and final LevelGamePlayManager 6/6')]

def csvout(file,rows):
 with open(ROOT/file,'w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def jout(file,obj): (ROOT/file).write_text(json.dumps(obj,indent=2),encoding='utf8')
def metadata():
 for file,rows in [
 ('Gameplay_Wiring_Concept_W001_W170.csv',WIRES),('Gameplay_Wiring_Budget.csv',GROUPS),
 ('Terminal_Anchor_Plan_248.csv',ANCHORS),('Visual_Terminal_Sockets_248.csv',ANCHORS),
 ('Terminal_Logical_Port_Reconciliation_340.csv',LOGICAL),('Terminal_Anchor_Signal_Conflict_Report.csv',CONFLICTS),
 ('Objective_Certification_Plan_6.csv',[dict(ObjectiveIndex=j+1,Title=t,AcceptanceCriteria=c,Evidence=e,Implemented='NO') for j,(t,c,e) in enumerate(OBJ)]),
 ('Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PIVOTS]),
 ('Component_Roles.csv',[dict(Index=i,Model=NAMES[i],WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],Role=('Orbital environment and shelter structure' if i<12 else 'Space weather and particle detection' if i<26 else 'Dosimetry and calibration' if i<39 else 'Water-wall finite resource shielding' if i<52 else 'Motorized shield mechanical systems' if i<68 else 'Shelter essentials and safety control' if i<82 else 'Holographic displays, diagnostics and final qualification'),State='ART_ONLY') for i in range(92)]),
 ('Art_Assembly.csv',[dict(Index=i,Model=NAMES[i],File=f'{i:02d}_{NAMES[i]}.fbx',TransformOrigin='0,0,0',ExpectedWorldCenter_m=','.join(map(str,P[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres') for i in range(92)])]:csvout(file,rows)
 w=WIRES[148]
 (ROOT/'W149_Critical_Shelter_Dose_Recovery.md').write_text(f'''# Project Spark Level 89 — W149 trusted shelter dose feedback recovery

**Source-grounded proposed concept route:** `{w['SourceModel']}` (`{w['SourceVisualAnchor']}`) → `{w['TargetModel']}` (`{w['TargetVisualAnchor']}`). Signal: `ANALOG_FEEDBACK`.

Functional port identities: `TRUSTED_SHELTER_DOSE_RATE_OUT` → `TRUSTED_SHELTER_DOSE_RATE_IN`.

1. Complete Objectives 1–5 with actual historical certification, not simply UI toggles.
2. During a protected training test, player physically unplugs actual live W149. It must change the real Spark circuit topology. **This art ZIP does not implement such a wire.**
3. Trusted shelter dose-rate sample becomes invalid/stale, **not zero radiation**. Storm particle environment and physical shielding keep evolving in the authoritative plant.
4. Safety PLC withdraws permission to open the shelter or certify exposure. Preserve existing locked shielding and independent safety/ventilation/power where physically feasible. Latch critical fault.
5. Diagnostic W149 status bits, SCADA cached measurements and independent sensors must not be accepted as substitute for W149 normal certification. Independent emergency shutdown remains active.
6. Repair correct real W149 wire. Confirm 5 V conditioning, returns, valid physical topology and new calibrated within-range timestamped measurements.
7. Require operator **explicit safety reset** after cross-checking independent shelter protection. Issue brand-new post-repair `runId`.
8. Complete full simulated quiet→rise→peak→decay storm with finite protection resources, valid trusted dose-rate, acceptable accumulated fictional dose, essential-systems service and no critical fault.
9. Only the actual `LevelGamePlayManager` may authorize Objective 6 and `6/6`. Never certify on mere reconnection or timer.

Educational game scenario, not a radiation qualification procedure or statement of spacecraft safety.
''',encoding='utf8')
def with_sockets(i):
 art=make_art(i)
 for x in BY_MODEL[i]:art.socket((x['WorldX_m'],x['WorldY_m'],x['WorldZ_m']),x['AnchorID'])
 return art

def homes(i,art):
 h=[(p['Pivot'],art.name,PIVOT_POS[p['Pivot']]) for p in PIVOTS if p['Index']==i]
 return h+[('SOCKET_'+aa,art.name,pos) for aa,pos in art.sockets]

def export_models():
 t0=time.monotonic();models=[];allpieces=[];allhomes=[];lower=np.ones(3)*np.inf;upper=-lower
 for i,n in enumerate(NAMES):
  art=with_sockets(i)
  parts=core.chunk_model(art,max_shapes=19)
  pivot_nodes=homes(i,art)
  ff=ROOT/'Models'/f'{i:02d}_{n}.fbx';gg=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  fbx=core.export_fbx(str(ff),[(n,p) for p in parts],[n],pivot_nodes,art.sockets)
  glb=core.export_glb(str(gg),[(n,p) for p in parts],[n],pivot_nodes)
  sock=dict(art.sockets)
  for pt in parts:
   pivot=pt['pivot'];offs=PIVOT_POS[pivot] if pivot in PIVOT_POS else sock[pivot[7:]] if pivot and pivot.startswith('SOCKET_') else (0,0,0)
   verts=pt['vertices']+np.asarray(offs)
   lower=np.minimum(lower,verts.min(axis=0));upper=np.maximum(upper,verts.max(axis=0))
  models.append(dict(index=i,name=n,file=ff.name,glb=gg.name,triangle_count=sum(len(p['faces']) for p in parts),mesh_parts=len(parts),
    source_primitive_count=len(art.raw),visual_socket_count=len(art.sockets),animation_pivots={k:xyz for k,_,xyz in pivot_nodes if k.startswith('ANIM_')},
    fbx_bytes=fbx['file_size'],glb_bytes=glb['glb_bytes']))
  allpieces.extend((n,p) for p in parts);allhomes.extend(pivot_nodes)
  if i in (0,3,5,28,43,52,55,82,88,91) or (i+1)%15==0:
   print('EXPORTED',i+1,'of 92',n,'triangles',models[-1]['triangle_count'],'sec',round(time.monotonic()-t0,1),flush=True)
 print('BUILD ASSEMBLED SHOWCASE...',len(allpieces),'submeshes',flush=True)
 showcase=ROOT/'Models'/'Level89_Complete_ArtKit_Showcase.fbx';showglb=ROOT/'GLB'/'Level89_Complete_ArtKit_Showcase.glb'
 f=core.export_fbx(str(showcase),allpieces,NAMES,allhomes,[(q['AnchorID'],(q['WorldX_m'],q['WorldY_m'],q['WorldZ_m'])) for q in ANCHORS])
 g=core.export_glb(str(showglb),allpieces,NAMES,allhomes)
 manifest=dict(level_id='level_89',display_name='The Helios Storm',subtitle='Solar Radiation Protection, Dosimetry, and Adaptive Shielding',
  individual_models=len(models),individual_glbs=len(models),assembled_fbx='Models/'+showcase.name,assembled_glb='GLB/'+showglb.name,
  individual_triangles=sum(m['triangle_count'] for m in models),individual_mesh_parts=sum(m['mesh_parts'] for m in models),
  animation_parent_pivot_groups=len(PIVOTS),planned_player_wires=len(WIRES),planned_visual_terminal_sockets=len(ANCHORS),
  proposed_independent_logical_wire_endpoints=len(LOGICAL),anchor_reconciliation_conflicts=len(CONFLICTS),
  workstation_world_bounds_m=[list(map(lambda x:round(float(x),5),lower)),list(map(lambda x:round(float(x),5),upper))],
  fbx_numeric_units='centimetres',fbx_unit_scale_factor=1,glb_numeric_units='metres',axes='Y-up X-width Z-forward',assembly_origin=[0,0,0],
  actual_player_wires_included=False,functional_sparkterminals_included=False,real_particle_transport_solver_included=False,
  runtime_safety_plc_included=False,six_runtime_objectives_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
  assembled_fbx_bytes=f['file_size'],assembled_glb_bytes=g['glb_bytes'],models=models)
 jout('Model_Manifest.json',manifest)
 print('ART EXPORT DONE',manifest['individual_triangles'],'tris',len(allpieces),'chunks',round(time.monotonic()-t0,1),'seconds',flush=True)
 return manifest
if __name__=='__main__':
 print('LEVEL89 ORIGINAL MODULAR ASSET BUILDER',flush=True)
 metadata();export_models()
