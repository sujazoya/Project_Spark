#!/usr/bin/env python3
"""Project Spark Level 91 THE COSMIC CLOCK -- real procedural FBX+GLB technical-art kit.
Produces original editable meshes and proposed, NOT IMPLEMENTED, control/wiring data.
Common model origin. Y-up, X-width, Z-forward. FBX vertex numbers centimetres; GLB metres.
"""
from pathlib import Path
from collections import defaultdict,Counter
import json,csv,math,random,time
import numpy as np
import trimesh
import geometry_export_core as core
ROOT=Path('/mnt/data/ProjectSpark_Level91_HD_AAA')
for n in ('Models','GLB','Textures','Unity_Optional','Source','Reports'):(ROOT/n).mkdir(exist_ok=True,parents=True)
NAMES=['L91_'+s.strip() for s in (Path(__file__).parent/'model_names.txt').read_text().splitlines() if s.strip()]
assert len(NAMES)==96
random.seed(91091)
core.COLORS={
 'structural':(.34,.43,.54,1),'titanium':(.75,.81,.86,1),'silver':(.63,.71,.8,1),
 'dark':(.032,.047,.074,1),'graphite':(.075,.108,.17,1),'copper':(.84,.45,.20,1),
 'gold':(.97,.72,.31,1),'ceramic':(.90,.94,.96,1),'glass':(.15,.56,.84,.43),
 'panel':(.017,.073,.15,1),'pcb':(.045,.20,.17,1),'cyan':(.08,.87,1,1),
 'blue':(.12,.43,1,1),'green':(.13,.89,.55,1),'amber':(1,.58,.11,1),
 'violet':(.62,.34,.95,1),'red':(1,.18,.17,1),'white':(.96,.97,1,1),
 'holo':(.22,.64,1,.50),'star':(.77,.82,1,1),'floor':(.22,.29,.40,1),
 'insulation':(.18,.24,.28,1),'purple':(.36,.20,.72,1),'black':(.014,.017,.028,1),
 'photon':(.90,.40,1,.79),'goldglass':(.95,.71,.28,.40),
}
core.MATERIALS={k:core.pbrmat(k) for k in core.COLORS}
P={}
def put(i,p):P[i]=tuple(float(v) for v in p)
# architectural roots and independent science exhibit modules
for i,p in enumerate([(0,-.15,0),(0,8.5,-14.6),(0,7.1,-13.9),(0,5.4,-10.7),
    (0,5.4,-8.5),(0,3.65,-8.9),(0,1.45,7.0),(0,3.2,-6.4),
    (0,10,-14.7),(-10.5,8.3,-12.1),(11.8,5.3,-7.0),(-12.6,4.2,-6.0),
    (10.7,.45,1.5),(9.6,3.0,2.5),(0,10,-12.0),(13,2.4,2.5)]):put(i,p)
for k,x in enumerate([-8.5,-2.85,2.85,8.5]):
 put(16+k,(x,7,-7.5));put(20+k,(x,5.2,-7.9));put(24+k,(x,7,-6.85));put(28+k,(x,2.5,-5.0))
put(32,(0,8.7,-4.5));put(33,(11,7,-7.5))
for k in range(34,52):
 n=k-34;put(k,(9.3+(n%3)*1.65, 2.65+(1 if k in (34,35) else 0), -.8+(n//3)*1.5))
for k in range(52,70):
 n=k-52;put(k,(-12.1+(n%6)*1.65,1.65,1.3+(n//6)*2.05))
for k in range(70,82):
 n=k-70;put(k,(-3.4+(n%6)*2.15,1.65,6.65+(n//6)*2.0))
for k in range(82,96):
 n=k-82;put(k,(-11.8+(n%7)*1.7,1.7,9.4+(n//7)*1.85))
assert set(P)==set(range(96))
# 84 named model-owned animation parents; optically bright UI indicators are NOT real physical X-rays.
PV=[]
def pivot(index,name,kind='indicator',axis='none'):
 PV.append(dict(Index=index,Pivot='ANIM_'+name,Model=NAMES[index],Kind=kind,Axis=axis))
for i,n,kind,ax in [(3,'AURORA_ATTITUDE','rotation','y'),(4,'PULSAR_VECTOR_MAP','indicator','none'),
 (5,'CELESTIAL_REFERENCE_MATRIX','rotation','y'),(6,'MISSION_CONSOLE','indicator','none'),
 (7,'TELESCOPE_SERVICE_LOCK','translation','y'),
 (9,'PULSAR_PHASE_DISPLAY','indicator','none'),
 (14,'FINAL_MISSION_BEACON','indicator','none')]:pivot(i,n,kind,ax)
for i in range(4):
 l='ABCD'[i]
 pivot(16+i,f'XRAY_TELESCOPE_{l}_AZIMUTH','rotation','y')
 pivot(16+i,f'XRAY_TELESCOPE_{l}_SUNSHADE','translation','z')
 pivot(20+i,f'XRAY_GIMBAL_{l}_ELEVATION','rotation','x')
 pivot(20+i,f'XRAY_GIMBAL_{l}_ROTATOR','rotation','z')
 pivot(24+i,f'XRAY_CONCENTRATOR_{l}_CALIBRATION','translation','z')
 pivot(28+i,f'PHOTON_DETECTOR_{l}_TIMING','indicator','none')
for i,name,kind,axis in [(34,'PRIMARY_CLOCK_CESIUM','rotation','y'),(34,'PRIMARY_CLOCK_LOCK','indicator','none'),
 (35,'REDUNDANT_CLOCK_CESIUM','rotation','y'),(35,'REDUNDANT_CLOCK_LOCK','indicator','none'),
 (36,'ULTRA_STABLE_OSCILLATOR','indicator','none'),(37,'TIME_DISTRIBUTION','indicator','none'),
 (38,'PRIMARY_TIME_TAGGER','indicator','none'),(39,'SECONDARY_TIME_TAGGER','indicator','none'),
 (40,'EVENT_BUFFER_A','indicator','none'),(41,'EVENT_BUFFER_B','indicator','none'),
 (42,'EVENT_BUFFER_C','indicator','none'),(43,'EVENT_BUFFER_D','indicator','none'),
 (44,'CLOCK_COMPARISON','indicator','none'),(45,'CLOCK_COOLING_FAN','rotation','z'),
 (46,'CLOCK_ISOLATOR_Z','translation','y'),(47,'CLOCK_BIAS_METER','rotation','z'),
 (48,'REFERENCE_SELECTOR','rotation','z'),(49,'PHASE_SCOPE','indicator','none'),
 (50,'FREQUENCY_METER','indicator','none'),(51,'CALIBRATION_PULSE','indicator','none')]:pivot(i,name,kind,axis)
for i in [52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69]:
 pivot(i,NAMES[i][4:].upper()[:40]+'_LIVE','indicator','none')
for i,n,k,ax in [(70,'POINTING_CONTROLLER','indicator','none'),(71,'GUIDANCE_COMPUTER','indicator','none'),
 (72,'COURSE_CORRECTION','indicator','none'),(73,'PROPULSION_COMMAND','indicator','none'),
 (74,'THRUSTER_LINK','indicator','none'),(75,'FLIGHT_SAFE','indicator','none'),
 (76,'CRITICAL_TIME_VALIDITY','indicator','none'),(77,'NAV_FAULT_RECORDER','indicator','none'),
 (79,'CONTROL_POWER','indicator','none'),(80,'NAV_SCADA','indicator','none'),
 (81,'OBJECTIVE_6_CERTIFIED','indicator','none'),
 (82,'COSMIC_PARTICLE_FAULT','translation','x'),(83,'CLOCK_THERMAL_DRIFT','translation','x'),
 (84,'PULSAR_EPHEMERIS_FAULT','translation','x'),(85,'GIMBAL_JAM_TEST','translation','x')]:pivot(i,n,k,ax)
assert len(PV)==84,len(PV)
PIVOT_POS={r['Pivot']:P[r['Index']] for r in PV}
# telescope mechanical pivots deliberately positioned at optical and gimbal home positions.
for j in range(4):
 l='ABCD'[j];x=P[16+j][0]
 PIVOT_POS[f'ANIM_XRAY_TELESCOPE_{l}_SUNSHADE']=(x,7.0,-6.8)
 PIVOT_POS[f'ANIM_XRAY_GIMBAL_{l}_ROTATOR']=(x,5.25,-7.85)
 PIVOT_POS[f'ANIM_XRAY_CONCENTRATOR_{l}_CALIBRATION']=(x,7.0,-6.35)
core.PIVOT_POS=PIVOT_POS
PIV_BY=defaultdict(list)
for p in PV:PIV_BY[p['Index']].append(p['Pivot'])

# Procedural engineering geometry helpers

def box(a,p,s,m='titanium',pv=None,label='MachinedPart'):a.box(p,s,m,pv,label)
def cyl(a,p,r,h,m='silver',axis='y',pv=None,label='PrecisionCylinder',seg=24):a.cylinder(p,r,h,m,axis,seg,pv,label)
def tor(a,p,R,r,m='titanium',axis='z',pv=None,label='MachinedAnnulus',seg=48,tube=8):a.torus(p,R,r,m,axis,seg,tube,pv,label)
def beam(a,p,q,r=.06,m='structural',pv=None,label='StructuralTruss'):a.beam(p,q,r,m,8,pv,label)
def sphere(a,p,r,m='titanium',pv=None,label='SphericalOptics',subs=2):
 me=trimesh.creation.icosphere(subdivisions=subs,radius=r);a.mesh(me.vertices+np.asarray(p),me.faces,m,pv,label)
def rack(a,p,kind='computer',pv=None,s=1.0):
 x,y,z=p;core.panel_equipment(a,p,s,'violet' if kind in ('clock','timing') else 'cyan','plc' if kind in ('plc','safety') else 'optical' if kind=='sensor' else 'fault' if kind=='fault' else 'instrument')
 for j in range(9):
  xx=x+(j-4)*.105*s
  box(a,(xx,y-.52*s,z+.40*s),(.07,.045,.11),'gold',None,'GoldPlatedSignalSocketPanel')
 for j in range(4):
  box(a,(x-.39*s+(j%2)*.52*s,y-.27*s+(j//2)*.22*s,z-.32*s),(.24*s,.12*s,.12*s),'pcb',None,'ModularCircuitBackplane')
 for j in range(6):
  box(a,(x-.39*s+j*.14*s,y+.09*s,z+.38*s),(.075*s,.02*s,.05*s),'green' if j%3 else 'amber',pv,'FunctionalStatusLightDisplayOnly')
def gauge(a,p,pv=None):
 x,y,z=p
 cyl(a,p,.17,.10,'titanium','z',None,'PrecisionGaugeBezel',24)
 cyl(a,(x,y,z+.06),.125,.014,'panel','z',None,'SensorReadingWindow',24)
 beam(a,(x,y,z+.075),(x+.07,y+.07,z+.075),.015,'amber',pv,'DynamicGaugeNeedle')
def telescope(a,p,l):
 x,y,z=p; az=f'ANIM_XRAY_TELESCOPE_{l}_AZIMUTH';shield=f'ANIM_XRAY_TELESCOPE_{l}_SUNSHADE'
 cyl(a,(x,y,z),.94,3.25,'titanium','z',az,'XRayTelescopeApertureShell',48)
 cyl(a,(x,y,z-.10),.80,3.18,'graphite','z',az,'HighVacuumOpticalBaffle',42)
 tor(a,(x,y,z+1.66),.91,.093,'gold','z',az,'ConcentratorGoldApertureRim',64)
 tor(a,(x,y,z-1.66),.88,.085,'titanium','z',az,'DetectorHousingRetainingRing',52)
 for r in np.linspace(.18,.79,14):
  tor(a,(x,y,z+1.77),float(r),.019,'gold' if int(r*100)%3 else 'silver','z',az,'NestedWolterTypeXRayOpticalShell',64,8)
 for j in range(16):
  ang=j*math.tau/16;xx=x+.9*math.cos(ang);yy=y+.9*math.sin(ang)
  beam(a,(xx,yy,z-1.5),(xx,yy,z+1.55),.035,'gold',az,'ConcentratorLongitudinalThermalStrut')
  cyl(a,(xx,yy,z+1.75),.05,.12,'silver','z',az,'PrecisionApertureFastener',12)
 for j in range(5):
  zz=z-1.0+j*.46
  tor(a,(x,y,zz),1.02,.055,'structural','z',az,'TelescopeVibrationIsolationHoop',48)
 box(a,(x,y+.64,z-1.65),(1.25,.36,1.0),'ceramic',az,'XRayDetectorCryogenicElectronicsInterface')
 box(a,(x,y,z+2.0),(2.27,2.27,.10),'graphite',shield,'MotorizedTelescopeSolarBaffleFrame')
 tor(a,(x,y,z+2.06),1.08,.058,'gold','z',shield,'TelescopeSunshieldDeploymentRing')
 for j in range(4):
  t=math.pi*j/2
  beam(a,(x+1.10*math.cos(t),y+1.10*math.sin(t),z+1.98),
       (x+1.33*math.cos(t),y+1.33*math.sin(t),z+2.40),.04,'silver',shield,'SunshadeSupportLeg')
def gimbal(a,p,l):
 x,y,z=p; elev=f'ANIM_XRAY_GIMBAL_{l}_ELEVATION';rot=f'ANIM_XRAY_GIMBAL_{l}_ROTATOR'
 box(a,(x,y-1.15,z),(2.6,.45,2.5),'structural',None,'ActiveTelescopeGimbalBase')
 for sx in (-1,1):
  box(a,(x+sx*1.13,y-.3,z),(.35,2.2,1.15),'titanium',None,'PrecisionAltazimuthForkArm')
  cyl(a,(x+sx*1.0,y+.40,z),.43,.30,'gold','x',elev,'ElevationCrossAxisBearing',32)
  tor(a,(x+sx*.96,y+.40,z),.35,.047,'graphite','x',elev,'OpticalEncoderElevationRing',36)
 tor(a,(x,y-.96,z),1.0,.10,'gold','y',rot,'AzimuthRotatorBase',40)
 for j in range(12):
  t=j*math.tau/12
  cyl(a,(x+1.04*math.cos(t),y-1.03,z+1.04*math.sin(t)),.05,.10,'silver','y',None,'AzimuthRingEncoderBolt',12)
 box(a,(x,y-.57,z+1.17),(1.4,.42,.26),'panel',None,'TelescopeGimbalMotorServoEnclosure')
def optics(a,p,l):
 x,y,z=p; pv=f'ANIM_XRAY_CONCENTRATOR_{l}_CALIBRATION'
 for j in range(13):tor(a,(x,y,z+.055*j),.65-j*.035,.025,'gold','z',pv,'NestedConcentricXRayOpticalFoil',52)
 for j in range(6):
  t=j*math.tau/6;beam(a,(x+.57*math.cos(t),y+.57*math.sin(t),z-.3),
    (x+.57*math.cos(t),y+.57*math.sin(t),z+.8),.035,'silver',pv,'OpticalShellMountingSpar')
 tor(a,(x,y,z-.30),.70,.06,'titanium','z',None,'MetrologyLensSupportRing')
def photon_detector(a,p,l):
 x,y,z=p;pv=f'ANIM_PHOTON_DETECTOR_{l}_TIMING'
 box(a,p,(1.35,1.68,1.20),'structural',None,'PhotonCounterDetectorVacuumHousing')
 box(a,(x,y+.13,z+.64),(.94,.85,.13),'graphite',None,'DetectorPhotonCountTelemetryScreen')
 for j in range(11):
  xx=x-.43+j*.086
  box(a,(xx,y+.15,z+.72),(.045,.50-(j%3)*.07,.03),'cyan' if j%4 else 'violet',pv,'LivePhotonEventHistogramBar')
 cyl(a,(x,y+.2,z-.69),.48,.23,'gold','z',None,'XRayDetectorEntranceWindow',40)
 cyl(a,(x,y+.2,z-.82),.32,.045,'glass','z',None,'DetectorFilterWindow',40)
 for j in range(8):
  yy=y-.57+j*.12
  box(a,(x-.48,yy,z+.67),(.075,.055,.06),'green' if j<5 else 'amber',pv,'EventValidLED')
 for sx in (-1,1):
  for j in range(5):box(a,(x+sx*.72,y-.5+j*.25,z),(.11,.12,.83),'silver',None,'ColdHeadThermalRadiationFin')
def atomic_clock(a,p,primary=True):
 x,y,z=p;pv='ANIM_PRIMARY_CLOCK_CESIUM' if primary else 'ANIM_REDUNDANT_CLOCK_CESIUM'
 box(a,(x,y-1.30,z),(2.85,.35,2.70),'structural',None,'AtomicClockReferenceFoundation')
 cyl(a,(x,y,z),.92,2.50,'titanium','y',None,'SealedUltraStableClockVacuumCanister',48)
 cyl(a,(x,y,z),.68,2.64,'glass','y',None,'ReferenceVacuumGlassInspectShroud',40)
 for j in range(6):tor(a,(x,y-.9+j*.35,z),.90,.055,'gold','y',pv,'AtomicFrequencyResonatorTrimRing',52)
 for j in range(10):
  ang=j*math.tau/10
  beam(a,(x+.95*math.cos(ang),y-1.0,z+.95*math.sin(ang)),
       (x+.95*math.cos(ang),y+1.05,z+.95*math.sin(ang)),.028,'copper',None,'HighStabilityShieldBraidedCable')
 cyl(a,(x,y+1.3,z),.55,.34,'ceramic','y',None,'AtomicClockEnvironmentalIsolationCap',34)
 for j in range(3):tor(a,(x,y+.14+j*.38,z),.47,.04,'cyan' if primary else 'violet','y',pv,'ClockOscillatorReferenceGlowDecorative',44)
 box(a,(x,y-1.05,z+1.10),(1.45,.34,.10),'panel',None,'PrimaryFrequencyStatusPanel')
 for j in range(8):box(a,(x-.60+j*.17,y-1.05,z+1.18),(.09,.085,.022),'green' if primary else 'cyan',None,'ClockFrequencyStatusIndicator')
def spacecraft(a,p):
 x,y,z=p;pv='ANIM_AURORA_ATTITUDE'
 cyl(a,(x,y,z),1.22,6.0,'ceramic','z',pv,'Aurora1PrecisionDeepSpaceVehicleHull',40)
 cyl(a,(x,y,z-2.6),.80,1.2,'graphite','z',pv,'ForwardAvionicsPayload',32)
 for j in range(5):tor(a,(x,y,z-2.3+j*1.1),1.24,.072,'gold','z',pv,'SpacecraftModularAssemblyRing',48)
 for side in (-1,1):
  box(a,(x+side*3.65,y,z),(5.2,.09,2.15),'structural',pv,'DeployableSolarWingFrame')
  box(a,(x+side*3.65,y+.065,z),(5.08,.035,2.05),'blue',pv,'SpacecraftSolarArrayCellSurface')
  for j in range(18):box(a,(x+side*(1.2+j*.28),y+.09,z),(.018,.024,2.00),'gold',pv,'SpacecraftSolarCellBusbars')
 for j in range(4):
  t=j*math.tau/4
  tor(a,(x+.72*math.cos(t),y+.72*math.sin(t),z+3.25),.24,.055,'cyan','z',pv,'Aurora1ElectricThrusterPort',24)
 for j in range(12):
  t=j*math.tau/12
  beam(a,(x+.92*math.cos(t),y+.92*math.sin(t),z-2.2),
    (x+.92*math.cos(t),y+.92*math.sin(t),z+2.1),.030,'silver',pv,'SpacecraftExternalHarnessTruss')

def fallback_equipment(a,p,index,pv):
 role='clock' if index in range(34,52) else 'safety' if index in (75,76,78,93) else 'fault' if index in (82,83,84,85) else 'computer'
 rack(a,p,role,pv,s=.95+(index%4)*.035)
 x,y,z=p
 # unique physical detailing depending on instrument purpose
 if index in (36,37,38,39,44,47,49,50,51,60,63,65,68):
  gauge(a,(x+.26,y+.20,z+.47),pv)
  for j in range(7):box(a,(x-.43+j*.14,y+.50,z+.34),(.08,.09,.13),'gold',None,'ReferenceClockOrSignalCoaxPort')
 if index in range(52,70):
  for j in range(4):
   box(a,(x-.34+j*.23,y-.65,z+.20),(.15,.34,.32),'graphite',None,'DSPProcessorRemovableCartridge')
   box(a,(x-.34+j*.23,y-.65,z+.40),(.07,.08,.02),'violet',pv,'DigitalSignalProcessorWorkingLED')
 if index in (73,74,75,76,79,94):
  for j in range(9):cyl(a,(x-.43+j*.105,y-.54,z+.34),.034,.09,'copper','z',None,'ProtectedSignalTerminal',10)
 if index in (82,83,84,85):
  box(a,(x+.25,y+.22,z+.50),(.16,.25,.11),'red',pv,'FaultInjectionKeyedIsolationLever')
  box(a,(x-.2,y+.17,z+.49),(.20,.30,.06),'amber',None,'FaultTestOnlyGuardPlate')
 if index in (86,87):
  for j in range(12):box(a,(x-.48+j*.09,y+.68,z),(.05,.22,.7),'silver',None,'DetectorCoolingFin')
 if index in (92,93):
  box(a,(x,y+.6,z+.42),(.95,.12,.10),'red',None,'SafetyCriticalCableIdentification')


def build_art(i):
 a=core.ModelArt(NAMES[i]);x,y,z=P[i];pv=PIV_BY[i][0] if PIV_BY[i] else None
 if i==0:
  box(a,(0,-.15,0),(32,.4,27),'floor',None,'CosmicNavigationLaboratoryServiceDeck')
  for j in range(18):box(a,(-15.5+j*1.8,.08,0),(.025,.025,26.0),'graphite',None,'ServiceFloorAntiSlipLane')
  for j in range(17):box(a,(0,.09,-12.5+j*1.6),(31,.025,.03),'silver',None,'PrecisionTileSeam')
  for side in (-1,1):
   box(a,(side*15.6,.15,0),(.10,.18,26),'gold',None,'HazardControlBoundaryLine')
   for j in range(16):box(a,(side*15.1,.19,-11.9+j*1.55),(.38,.025,.13),'amber',None,'ElectricalServiceSafetyMarker')
 elif i==1:
  box(a,(0,9,-14.7),(31,14,.14),'glass',None,'ObservationPanoramicShieldWindow')
  for j in range(12):beam(a,(-15+j*2.72,1.5,-14.65),(-15+j*2.72,15.5,-14.65),.078,'structural',None,'ThickViewportMullion')
  for h in (1.7,6.9,12.1,15.5):beam(a,(-15.8,h,-14.6),(15.8,h,-14.6),.11,'titanium',None,'ExternalWindowPressureFrame')
 elif i==2:
  for h in (2.5,5.0,7.5,10.0,12.5):beam(a,(-14.7,h,-12.7),(14.7,h,-12.7),.070,'silver',None,'DeepSpaceObservationStructuralTruss')
  for j in range(12):
   xx=-14+j*2.6;beam(a,(xx,2.0,-12.7),(xx,13.6,-12.7),.068,'titanium',None,'StructuralViewPortalTruss')
   for k in range(6):cyl(a,(xx,2.2+k*1.85,-12.6),.08,.17,'gold','z',None,'TrussPrecisionJointFastener',12)
 elif i==3:spacecraft(a,P[i])
 elif i==4:
  tor(a,(0,5.9,-8.5),4.1,.09,'holo','z',pv,'HolographicCelestialReferenceCircularScale',84)
  for j in range(4):
   theta=j*math.tau/4+.5;cx=3.0*math.cos(theta);cy=5.9+2.4*math.sin(theta)
   sphere(a,(cx,cy,-8.45),.34,'violet',pv,'PulsarReferenceLightSourceOnly',2)
   tor(a,(cx,cy,-8.44),.52,.030,'photon','z',pv,'PulsarReferencePhaseRing',40)
   for k in range(4):
    t=k*math.tau/4;beam(a,(cx,cy,-8.45),(cx+1.05*math.cos(t),cy+1.05*math.sin(t),-8.45),.015,'cyan',pv,'PulsarReferenceLineOfSight')
  sphere(a,(0,5.9,-8.28),.64,'glass',pv,'SpacecraftNavigationConfidenceEllipsoidReference',2)
  for rad in (1.5,2.4,3.4):tor(a,(0,5.9,-8.52),rad,.020,'blue','z',pv,'GalacticReferenceGridConcentric')
 elif i==5:
  box(a,(0,3.8,-8.9),(10.5,.30,3.4),'graphite',None,'NavigationHologramBeamProjectorStage')
  for j in range(19):
   xx=-4.8+j*.53;box(a,(xx,4.01,-8.25),(.31,.08,.16),'blue',pv,'ProjectedPhotonPhaseGridEmitter')
  for j in range(4):
   t=j*math.tau/4
   cyl(a,(4.65*math.cos(t),4.11,-8.9+1.1*math.sin(t)),.17,.30,'gold','y',pv,'HologramVectorProjectionPylon')
 elif i==6:
  box(a,(0,1.13,7.0),(13.8,1.05,2.7),'structural',None,'NavigationControlMasterConsole')
  for j in range(6):
   xx=-5.8+j*2.27;box(a,(xx,1.73,7.0),(2.08,.07,1.28),'graphite',None,'ErgonomicInstrumentControlSurface')
   box(a,(xx,1.81,7.15),(1.8,.035,.80),'panel',pv,'NavigationTimingScreen')
   for k in range(9):
    box(a,(xx-.75+k*.19,1.83,6.55),(.12,.065,.12),'green' if k%4==0 else 'gold',None,'SpacecraftControlConsoleKey')
 elif i==7:
  for xx in (-10,-5,0,5,10):
   box(a,(xx,3.6,-7.1),(2.7,.24,4.1),'structural',None,'PrecisionTelescopeServiceCatwalk')
   for side in (-1,1):beam(a,(xx+side*1.25,3.8,-9.0),(xx+side*1.25,4.55,-9.0),.038,'gold',pv,'TelescopeMaintenanceSafetyRail')
 elif i==8:
  for j in range(170):
   px=random.uniform(-15,15);py=random.uniform(1.2,14.0)
   sphere(a,(px,py,-14.88),.016+(j%3)*.009,'star',pv,'ObservedFaintStarVisibleMarker',0)
  for j in range(8):
   t=j*math.tau/8
   tor(a,(12*math.cos(t),9+4*math.sin(t),-14.90),1.10,.02,'violet','z',pv,'DeepSpaceAstrometricStarGrid',42)
 elif i==9:
  box(a,(-10.5,8.3,-12.1),(6.5,3.55,.20),'graphite',None,'FourPulsarPhaseEngineeringScreenFrame')
  box(a,(-10.5,8.3,-11.95),(6.13,3.23,.05),'panel',pv,'LivePulsarSignalBackground')
  for j in range(4):
   yy=7.15+j*.77
   for k in range(29):
    dx=(k-14)*.192;h=.05+.12*(1+math.cos(k*.68+j*1.4))
    box(a,(-10.5+dx,yy,-11.90),(.072,h,.04),'cyan' if j%2==0 else 'violet',pv,'PulsarReferencePulseProfileBar')
 elif i==10:
  beam(a,(11,4.8,-9.0),(11,4.8,-1),.10,'titanium',None,'InspectionRailX')
  box(a,(11,4.7,-6.5),(.5,.35,1.3),'gold',pv,'MovingOpticsInspectionCarriage')
 elif i==11:
  box(a,(-12.8,4,-6.3),(2.3,4.8,2.6),'graphite',None,'ShieldedPhotonElectronicsEnclosure')
  for j in range(13):box(a,(-13.8+j*.165,6.42,-6.3),(.09,.13,2.3),'gold',None,'EnclosureHoneycombXRayShieldingRib')
  box(a,(-11.7,4.1,-4.94),(.12,3.6,1.4),'silver',pv,'EnclosureShieldServiceDoor')
 elif i==12:
  box(a,(11,0.3,1.7),(8.4,.46,11.8),'floor',None,'ClockTimeReferenceLaboratoryPlatform')
  for xx in [7.1,14.5]:beam(a,(xx,.5,-3.6),(xx,.5,7.1),.12,'gold',None,'TimingLaboratorySafetyBoundary')
 elif i==13:
  for xx in (7,9,11,13):beam(a,(xx,3.9,-3.5),(xx,3.9,7.5),.075,'silver',None,'AtomicTimingInstrumentCableBridge')
 elif i==14:
  tor(a,(0,10,-12),2.0,.18,'gold','z',pv,'MissionNavigationCertificationHalo',80)
  for j in range(30):
   t=j*math.tau/30
   box(a,(2.1*math.cos(t),10+2.1*math.sin(t),-11.9),(.11,.11,.15),'cyan',pv,'AchievementHaloSegment')
 elif i==15:
  box(a,(13,2.4,2.5),(.5,4.2,9.0),'structural',None,'HabitatNavigationLaboratorySafetyScreen')
  for j in range(8):box(a,(13.30,1+j*.45,2.5),(.04,.04,9.0),'amber',None,'SafetyZoneVisualStripes')
 elif 16<=i<=19:telescope(a,P[i],'ABCD'[i-16])
 elif 20<=i<=23:gimbal(a,P[i],'ABCD'[i-20])
 elif 24<=i<=27:optics(a,P[i],'ABCD'[i-24])
 elif 28<=i<=31:photon_detector(a,P[i],'ABCD'[i-28])
 elif i==32:
  for j in range(4):
   px=P[16+j][0]
   tor(a,(px,7,-4.95),1.28,.082,'graphite','z',None,'XRAYDetectorSunlightBaffleHoop')
   for q in range(7):
    ang=q*math.tau/7
    beam(a,(px+1.2*math.cos(ang),7+1.2*math.sin(ang),-5.1),
        (px+1.45*math.cos(ang),7+1.45*math.sin(ang),-5.7),.04,'silver',None,'SolarBaffleThermalSupport')
 elif i==33:
  for k in range(5):tor(a,(11,7,-7.5+k*.18),.65-k*.08,.045,'gold','z',None,'PhotonDetectorCalibrationTargetRing')
  for j in range(16):
   t=j*math.tau/16
   box(a,(11+.55*math.cos(t),7+.55*math.sin(t),-7.4),(.08,.08,.16),'cyan',None,'CalibrationReferenceTargetMarker')
 elif i in (34,35):atomic_clock(a,P[i],i==34)
 elif i==46:
  for sy in (-1,1):
   box(a,(x,y+sy*.33,z),(1.65,.10,1.75),'structural',None,'SeismicIsolationPlate')
   for xx in (-.6,.6):
    for zz in (-.6,.6):cyl(a,(x+xx,y,z+zz),.075,.62,'gold','y',pv,'ActiveMicroVibrationIsolator',18)
 elif i==69:
  tor(a,P[i],.82,.04,'holo','z',pv,'NavigationUncertaintyScale')
  for j in range(3):tor(a,(x,y,z+j*.10),.45+j*.16,.03,'violet','y',pv,'CovarianceEllipsoidCrossSection',40)
 elif i==91:
  box(a,P[i],(1.8,1.9,1.0),'black',None,'DeepSpaceCommunicationsBlackoutShieldBox')
  for j in range(5):tor(a,(x,y,z+.50+j*.06),.20+j*.065,.025,'amber','z',None,'BlackoutSimulationWaveGlyph')
 else:fallback_equipment(a,P[i],i,pv)
 # actual source-authorized pivot groups must be owning geometry, not empty Transform objects.
 used={rec[3] for rec in a.raw if rec[3]}
 for piv in PIV_BY[i]:
  if piv not in used:
   home=PIVOT_POS[piv]
   box(a,(home[0],home[1],home[2]+.12),(.16,.055,.05),'cyan',piv,'AnimatedActualStateIndicatorOnly')
 return a

# gameplay and art manifest production
GROUPS=[('W001-W012','Protected educational 5 V and returns',12),
 ('W013-W038','Four X-ray detector acquisition interfaces',26),
 ('W039-W064','Atomic clock and timing synchronization',26),
 ('W065-W086','Photon event processing and time tagging',22),
 ('W087-W108','Telescope pointing and calibration',22),
 ('W109-W130','Navigation filtering and guidance',22),
 ('W131-W164','Independent safety, disturbances and W163',34),
 ('W165-W182','SCADA and certification telemetry',18)]

# Each model has 2 or 3 physical decorative sockets; 68*3+28*2 = 260. They are NOT functional SparkTerminals.
ANCHORS=[];BY_MODEL=defaultdict(list)
for i,n in enumerate(NAMES):
 x,y,z=P[i];num=3 if i<68 else 2
 for k in range(num):
  ref=f'P{len(ANCHORS)+1:03d}'
  # lab-facing offset. Within the model's planned service space, not a validated electrical contact point.
  pos=(round(x+(k-(num-1)/2)*.23,4),round(y+.31-(k%2)*.15,4),round(z+.50,4))
  row=dict(AnchorID=ref,OwnerModel=n,WorldX_m=pos[0],WorldY_m=pos[1],WorldZ_m=pos[2],Status='DECORATIVE_ART_ANCHOR_ONLY')
  ANCHORS.append(row);BY_MODEL[i].append(row)
assert len(ANCHORS)==260

def pick_index(group,j,src=True):
 # purpose-directed models, same ID ranges as the conceptual architecture.
 if group==0:
  return 79 if src else [70,75,76,71,64,37,34,80,28,29,30,31][j%12]
 if group==1:
  sources=[28,29,30,31,24,25,26,27,16,17,18,19,32,33]
  targets=[52,53,54,55,56,57,58,38,39,70]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 if group==2:
  sources=[34,35,36,44,45,46,47,48,49,50,51,37]
  targets=[37,38,39,44,47,48,49,50,51,76,64]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 if group==3:
  sources=[38,39,40,41,42,43,52,53,54,55,56,57,58]
  targets=[56,57,58,62,63,64,65,66,67,68,69]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 if group==4:
  sources=[70,71,20,21,22,23,16,17,18,19,33,85,89,90]
  targets=[20,21,22,23,24,25,26,27,70,71,75]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 if group==5:
  sources=[58,59,60,61,62,63,64,65,66,67,68,69,89,90,71,72]
  targets=[64,65,66,67,68,69,71,72,73,74,75,76]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 if group==6:
  sources=[75,76,78,82,83,84,85,34,35,44,45,70,72,77,79,93]
  targets=[76,75,78,77,71,72,73,74,80,90,94,81]
  return sources[j%len(sources)] if src else targets[j%len(targets)]
 sources=[80,77,65,64,71,72,75,76,59,60,62,63,69,51,92,95]
 targets=[80,95,81]
 return sources[j%len(sources)] if src else targets[j%len(targets)]

WIRE_GROUP_RANGES=[(1,12),(13,38),(39,64),(65,86),(87,108),(109,130),(131,164),(165,182)]
WIRES=[];logical=[]
for g,(start,end) in enumerate(WIRE_GROUP_RANGES):
 for number in range(start,end+1):
  j=number-start;sidx=pick_index(g,j,True);tidx=pick_index(g,j,False)
  if sidx==tidx:tidx=[76,70,71,80][(j+g)%4]
  if number==163:sidx=34;tidx=76
  if number<=12:kind='POWER_CONTROL_5V'
  elif g==1:kind='PHOTON_EVENT_FEEDBACK' if j%3 else 'DIGITAL_CONTROL'
  elif g==2:kind='TIME_REFERENCE_FEEDBACK' if j%3 else 'DIGITAL_CONTROL'
  elif g==3:kind='DIGITAL_EVENT_STREAM' if j%2 else 'ANALOG_FEEDBACK'
  elif g==4:kind='DIGITAL_CONTROL' if j%2 else 'ENCODER_FEEDBACK'
  elif g==5:kind='NAVIGATION_MEASUREMENT' if j%2 else 'DIGITAL_CONTROL'
  elif g==6:kind='SAFETY_FEEDBACK' if j%2 else 'DIGITAL_CONTROL'
  else:kind='SCADA_TELEMETRY'
  if number==163:kind='TIME_REFERENCE_FEEDBACK'
  sid='W'+str(number).zfill(3)
  aa=BY_MODEL[sidx][(j+g)%len(BY_MODEL[sidx])]
  bb=BY_MODEL[tidx][(j+g+1)%len(BY_MODEL[tidx])]
  if number==163:aa=BY_MODEL[34][1];bb=BY_MODEL[76][0]
  desc='Critical trusted atomic clock time-reference feedback' if number==163 else (
    f'{GROUPS[g][1]} signal {j+1:02d}')
  WIRES.append(dict(WireID=sid,Group=GROUPS[g][1],SourceModel=NAMES[sidx],SourceVisualAnchor=aa['AnchorID'],
     TargetModel=NAMES[tidx],TargetVisualAnchor=bb['AnchorID'],SignalKind=kind,Purpose=desc,
     Critical='YES' if number==163 else 'NO',Status='CONCEPT_ONLY_NO_LIVE_PLAYER_WIRE'))
  for side,ii,anchor in [('Source',sidx,aa),('Target',tidx,bb)]:
   fn= ('TRUSTED_TIME_REFERENCE_OUT' if side=='Source' else 'TRUSTED_TIME_REFERENCE_IN') if number==163 else f'{sid}_{side.upper()}_{kind}'
   logical.append(dict(WireID=sid,Endpoint=side,Model=NAMES[ii],ArtAnchor=anchor['AnchorID'],
    FunctionalPortID=fn,SignalKind=kind,Direction='OUTPUT' if side=='Source' else 'INPUT',
    Critical='YES' if number==163 else 'NO',FunctionalStatus='PLANNED_SEPARATE_LOGICAL_PORT_NOT_SPARKTERMINAL'))
assert len(WIRES)==182 and len(logical)==364
shared=defaultdict(list)
for q in logical:shared[q['ArtAnchor']].append(q)
CONFLICTS=[]
for k,v in shared.items():
 if len(v)<2:continue
 CONFLICTS.append(dict(AnchorID=k,OwnerModel=v[0]['Model'],LogicalContactCount=len(v),
  SignalKinds=';'.join(sorted({r['SignalKind'] for r in v})),MixedSignalKinds=len({r['SignalKind'] for r in v})>1,
  Requirement='INDEPENDENT_FUNCTIONAL_SUBCONTACTS_NO_UNSAFE_SHORT'))
OBJECTIVES=[
('Build the Cosmic Navigation Network','182 genuine player-wired circuit paths, safe control supply and returns, reconciled functional ports, real safety input including W163'),
('Calibrate the Precision Time Observatory','Four telescopes homed and calibrated, valid photon acquisition and timing standards with time-stamped evidence'),
('Acquire the Four Celestial Clocks','Four genuine pulse profiles fitted from photon events with valid timing, distinct source directions and nondegenerate geometry'),
('Solve the Deep-Space Position and Correct Course','State estimation and covariance qualification from actual sampled timing plus executed bounded propulsion course correction'),
('Recover Three Deep-Space Navigation Failures','High background, atomic clock thermal drift and ephemeris mismatch genuinely affect plant and are diagnosed/repaired, fresh valid mission follows'),
('Restore Critical Atomic Clock Feedback','Real disconnect W163, inhibit new correction burns, reconnect, obtain fresh calibrated time, explicit reset and fresh four-pulsar and correction run')]

def writecsv(path,rows):
 if not rows:return
 with (ROOT/path).open('w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def emit_tables():
 tab={
  'Gameplay_Wiring_Concept_W001_W182.csv':WIRES,
  'Gameplay_Wiring_Budget.csv':[dict(WireIDRange=t,Subsystem=name,Count=c) for (t,name,c) in zip([f'W{a:03d}-W{b:03d}' for a,b in WIRE_GROUP_RANGES],[v[1] for v in GROUPS],[v[2] for v in GROUPS])],
  'Terminal_Anchor_Plan_260.csv':ANCHORS,
  'Visual_Terminal_Sockets_260.csv':ANCHORS,
  'Terminal_Logical_Port_Reconciliation_364.csv':logical,
  'Terminal_Anchor_Signal_Conflict_Report.csv':CONFLICTS,
  'Objective_Certification_Plan_6.csv':[dict(ObjectiveIndex=j+1,Title=title,AcceptanceCriteria=criteria,Implemented='NO') for j,(title,criteria) in enumerate(OBJECTIVES)],
  'Animation_Rigs.csv':[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=PIVOT_POS[p['Pivot']][0],WorldY_m=PIVOT_POS[p['Pivot']][1],WorldZ_m=PIVOT_POS[p['Pivot']][2]) for p in PV],
  'Component_Roles.csv':[dict(Index=i,Model=n,WorldX_m=P[i][0],WorldY_m=P[i][1],WorldZ_m=P[i][2],
     Role='Architecture' if i<16 else 'Optics & detectors' if i<34 else 'Atomic timing systems' if i<52 else 'Photon processing & navigation' if i<70 else 'Guidance, PLC & SCADA' if i<82 else 'Faults, thermal and support',Status='ART_ONLY') for i,n in enumerate(NAMES)],
  'Art_Assembly.csv':[dict(Index=i,Model=n,File=f'{i:02d}_{n}.fbx',LocalPosition='0,0,0',ExpectedWorldCenter_m=','.join(map(str,P[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres') for i,n in enumerate(NAMES)]
 }
 for k,v in tab.items():writecsv(k,v)
 w=WIRES[162]
 (ROOT/'W163_Critical_Atomic_Clock_Recovery.md').write_text(f'''# Level 91: W163 trusted atomic clock feedback recovery

**Concept route:** `{w['SourceModel']}` / `{w['SourceVisualAnchor']}` → `{w['TargetModel']}` / `{w['TargetVisualAnchor']}`. `SignalKind=TIME_REFERENCE_FEEDBACK`, `Critical=YES`.

Functional source contact: `TRUSTED_TIME_REFERENCE_OUT`. Functional destination contact: `TRUSTED_TIME_REFERENCE_IN`.

1. Only after Objectives 1–5 have real historical certificates, enter a protected fault test.
2. The player physically removes W163 through the existing wire tool; this archive does not itself contain a playable wire.
3. The circuit graph shows an open edge; time-reference measurement validity is false and last-good sample timestamps remain stale. Missing timing is NOT zero clock bias.
4. Inhibit new autonomous propulsion correction burns while real spacecraft position and velocity continue evolving, and while independent spacecraft protections remain available.
5. Latch the critical navigation fault. SCADA, backup clock and previous ephemeris samples cannot pretend W163 is continuous.
6. Reconnect actual W163; check electrical domain, reference power/return, direction, integrity, sampling rate, calibration and independent safety conditions.
7. Recalibrate real modeled clock bias/frequency and require explicit operator safety reset.
8. Start a NEW post-repair runId. Reacquire four independent pulsars, fit fresh pulse timing, estimate valid navigation covariance and execute the fresh measured correction profile.
9. Historical objective records 1–5 remain awarded; Objective 6 only after genuine full post-repair mission and `LevelGamePlayManager` reaches 6/6.

The entire scenario uses fictional bounded educational parameters; not flight-qualified spacecraft hardware or real X-ray instrumentation.
''',encoding='utf8')
 return tab

def add_sockets(i):
 a=build_art(i)
 for q in BY_MODEL[i]:a.socket((q['WorldX_m'],q['WorldY_m'],q['WorldZ_m']),q['AnchorID'])
 return a

def homes(i,a):return [(p['Pivot'],a.name,PIVOT_POS[p['Pivot']]) for p in PV if p['Index']==i]+[('SOCKET_'+uid,a.name,xyz) for uid,xyz in a.sockets]

def export_models():
 started=time.monotonic();models=[];pieces=[];allhomes=[];lower=np.ones(3)*np.inf;upper=np.ones(3)*-np.inf
 for i,n in enumerate(NAMES):
  a=add_sockets(i);parts=core.chunk_model(a,max_shapes=17);h=homes(i,a)
  fpath=ROOT/'Models'/f'{i:02d}_{n}.fbx';gpath=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  ff=core.export_fbx(str(fpath),[(n,p) for p in parts],[n],h,a.sockets)
  gg=core.export_glb(str(gpath),[(n,p) for p in parts],[n],h)
  smap=dict(a.sockets)
  for part in parts:
   pv=part['pivot'];offs=PIVOT_POS[pv] if pv in PIVOT_POS else smap[pv[7:]] if pv and pv.startswith('SOCKET_') else (0,0,0)
   v=part['vertices']+np.asarray(offs)
   lower=np.minimum(lower,v.min(axis=0));upper=np.maximum(upper,v.max(axis=0))
  models.append(dict(index=i,name=n,fbx=fpath.name,glb=gpath.name,triangles=sum(len(p['faces']) for p in parts),
   chunk_meshes=len(parts),source_primitives=len(a.raw),socket_meshes=len(a.sockets),
   pivot_names=[p['Pivot'] for p in PV if p['Index']==i],fbx_bytes=ff['file_size'],glb_bytes=gg['glb_bytes']))
  pieces.extend((n,p) for p in parts);allhomes.extend(h)
  if i in (0,3,4,16,17,18,19,20,24,28,34,35,37,46,52,64,69,76,80,92,95) or (i+1)%24==0:
   print('MODEL',i+1,'/',len(NAMES),n,'triangles',models[-1]['triangles'],'seconds',round(time.monotonic()-started,1),flush=True)
 fpath=ROOT/'Models'/'Level91_Complete_ArtKit_Showcase.fbx';gpath=ROOT/'GLB'/'Level91_Complete_ArtKit_Showcase.glb'
 ff=core.export_fbx(str(fpath),pieces,NAMES,allhomes,[(q['AnchorID'],(q['WorldX_m'],q['WorldY_m'],q['WorldZ_m'])) for q in ANCHORS]);gg=core.export_glb(str(gpath),pieces,NAMES,allhomes)
 manifest=dict(level_id='level_91',title='The Cosmic Clock',subtitle='X-ray Pulsar Navigation and Precision Atomic Timing',
   modular_fbx_count=len(models),modular_glb_count=len(models),assembled_fbx=fpath.name,assembled_glb=gpath.name,
   triangle_count=sum(m['triangles'] for m in models),mesh_chunk_count=sum(m['chunk_meshes'] for m in models),
   animation_pivots=len(PV),decorative_sockets=len(ANCHORS),planned_player_wires=len(WIRES),
   proposed_independent_logical_endpoints=len(logical),reused_socket_locations=len(CONFLICTS),
   mixed_signal_socket_locations=sum(bool(q['MixedSignalKinds']) for q in CONFLICTS),
   bounds_min_m=lower.round(5).tolist(),bounds_max_m=upper.round(5).tolist(),
   geometry_axis='Y-up',fbx_numeric_units='centimetres',glb_numeric_units='metres',shared_model_origins=True,
   actual_player_wires_included=False,functional_sparkterminals_included=False,real_pulsar_estimator_included=False,
   clock_physics_implemented=False,unity_compilation_tested=False,unity_playmode_tested=False,
   objective_certifications_implemented=False,models=models)
 (ROOT/'Model_Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
 print('KIT GENERATED: total triangles',manifest['triangle_count'],'mesh chunks',len(pieces),'seconds',round(time.monotonic()-started,1),flush=True)
 return manifest

if __name__=='__main__':
 print('LEVEL 91 SOURCE-GROUNDED PROCEDURAL MODEL EXPORT',flush=True)
 emit_tables();export_models()
