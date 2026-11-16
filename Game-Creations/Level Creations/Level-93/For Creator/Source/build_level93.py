#!/usr/bin/env python3
"""Project Spark 93 / The Elemental Archive: ORIGINAL procedural science laboratory art kit.
Real binary FBX 7400 + glTF 2 GLB with UV, named materials/pivots. Game wiring remains planned only.
Y-up, geometry in common scene coordinates; all modular models should be placed at origin (0,0,0).
"""
from pathlib import Path
from collections import defaultdict,Counter
import numpy as np, trimesh, csv, json, math, random, time
import geometry_export_core as c
ROOT=Path(__file__).resolve().parents[1]
for folder in ('Models','GLB','Textures','Materials','Shaders','Unity_Optional','Source','Reports'):(ROOT/folder).mkdir(exist_ok=True)
SECTIONS={
'Environment':[
'Aurora1ScienceLaboratoryDeck','PanoramicObservationWindow','TitaniumWallModuleLeft','TitaniumWallModuleRight','CeilingLabLightingArray','LabSafetyRailSet','CentralArchiveDais','PristineSampleVaultOuterShell','PristineSampleVaultInspectionGlass','Khepri7AsteroidCoreArchive','SampleCoreDisplayCradle','ElementalHolographicArchive','AsteroidCompositionWallDisplay','CleanRoomAirlock','ScienceMissionControlDesk','ContaminationFreeFloorGrid','LaboratoryUtilityBridge','ArchiveQualificationBeacon'],
'Tomography':[
'XRayCTMainGantry','XRayCTRotatingBore','CTSourceShieldedHousing','CTBeamCollimator','CTDetectorArc','CTPrecisionRotationStage','CTStageServoDrive','CTAngleEncoder','CTSampleCarrier','CTGantryCoolingManifold','CTReconstructionComputer','CTGeometryCalibrationPhantom','CTInterlockDoor','CTPowerIsolationPanel','CTReferenceFiducialSet','CTProjectionHologram','CTDiagnosticRack'],
'Spectroscopy':[
'RamanMicroscopeBody','RamanLaserHead','RamanOpticalTable','RamanWavelengthCalibrator','RamanSpectralDetector','InfraredSpectrometer','IRInterferometer','IRSampleStage','IRDetectorCoolingHead','XRFEnergyAnalyzer','XRFShieldedSourceModule','XRFDetectorHead','XRFSamplePositioner','SpectralFusionProcessor','MineralReferenceLibrary','OpticalSafetyInterlock','SpectrumHologramProjector'],
'ThermalVacuum':[
'ThermalVacuumChamber','ThermalVacuumAccessHatch','MicroSampleHeater','HeaterPrecisionThermocouple','VacuumTurbomolecularPump','VacuumBackingPump','VacuumIsolationManifold','AnalyticalVacuumPressureTransducer','ResidualGasAnalyzer','QuadrupoleMassSpectrometer','MassIonSource','MassFilterRodArray','MassDetectorMultiplier','SampleGasCaptureTrap','VolatileCollectionReservoir','VacuumVentAndPurgePanel','ThermalProcessController','MassSpectrumReadoutRack'],
'SampleHandling':[
'InertGloveboxMain','InertGloveboxInspectionWindow','DualAccessGlovePorts','CleanSampleTransferArm','CoreCartridgeA','CoreCartridgeB','MicroSampleAblationStage','SampleTransferCarousel','InertPurgeGasManifold','AtmosphereContaminationWitness','PrecisionMicrobalance','SampleSealCompressionTester','SampleCustodyTrackingTerminal','PristineArchiveReturnLatch'],
'SafetyControls':[
'AnalyticalPowerDistribution','AnalyticalSafetyPLC','SCADASystemRack','OpticalInstrumentController','CTSafetyInterlockPLC','VacuumSafetyPowerRelay','InertEnvironmentController','ThermalVacuumFaultInjector','TomographyEncoderFaultInjector','MassDetectorGainFaultInjector','W179CriticalPressurePatchStation','ScienceEvidenceProcessor','ArchiveObjectivePanel','Khepri7GeochemistryReportStation','EmergencyStopMaster','Level93MissionSuccessConsole']
}
assert list(map(len,SECTIONS.values()))==[18,17,17,18,14,16]
NAMES=['L93_'+n for names in SECTIONS.values() for n in names];assert len(NAMES)==100
CATEGORIES=[k for k,group in SECTIONS.items() for _ in group]
random.seed(93093)
c.COLORS={
'dark':(.04,.06,.09,1),'white':(.92,.94,.96,1),'titanium':(.72,.80,.88,1),'silver':(.58,.66,.74,1),'structural':(.28,.39,.47,1),'graphite':(.075,.098,.128,1),
'black':(.020,.030,.041,1),'panel':(.025,.060,.095,1),'floor':(.17,.21,.27,1),'gold':(.92,.72,.32,1),'copper':(.80,.45,.20,1),
'ceramic':(.89,.90,.84,1),'glass':(.25,.62,.77,.38),'cyan':(.07,.84,.97,1),'blue':(.10,.40,.89,1),'amber':(1.,.52,.10,1),
'green':(.15,.85,.50,1),'red':(.94,.15,.12,1),'purple':(.55,.33,.94,1),'holo':(.12,.72,.97,.55),
'core':(.24,.24,.27,1),'mineral':(.68,.40,.24,1),'ore':(.62,.70,.74,1),'regolith':(.36,.34,.31,1),
'inert':(.24,.55,.59,.60),'vacuum':(.31,.33,.48,1),'glassgold':(.84,.64,.25,.48),'spectrum':(.46,.25,.91,1),'laser':(.56,.14,.90,1)
}
c.MATERIALS={k:c.pbrmat(k) for k in c.COLORS}

def P(v):return tuple(float(q) for q in v)
POS={}
# 26m x 20m lab, 8m tall. Main glass sample archive centered near rear.
loc0={0:(0,-.17,0),1:(0,4.8,-10),2:(-13,3.4,0),3:(13,3.4,0),4:(0,7.5,0),5:(0,.8,0),6:(0,.4,-1.5),
7:(0,2.25,-1.5),8:(0,2.6,-1.5),9:(0,2.45,-1.5),10:(0,1.3,-1.5),11:(0,5.0,-1.5),12:(0,5.9,-9.2),13:(-11.4,2.0,7),
14:(0,1.35,7.5),15:(0,.02,0),16:(7,3.6,-8.7),17:(0,6.8,-1.5)}
POS.update(loc0)
# CT equipment left side, aligned with bore around (-8,2,-3)
for j in range(17):POS[18+j]=P((-8.4+(j%4)*.86,2.2+(j//4)*.28,-3.2+(j%3)*.36))
POS.update({18:(-8.4,2.25,-3.3),19:(-8.4,2.25,-3.3),20:(-8.4,2.25,-3.3),21:(-8.4,2.25,-3.3),22:(-8.4,2.25,-3.3),23:(-8.4,1.5,-3.3),24:(-8.4,1.1,-3.3),25:(-7.3,1.6,-3.3),26:(-8.4,2.25,-3.3),27:(-8.5,2,-5),28:(-10.9,1.4,-5.0),29:(-7.3,1.4,-5.0),30:(-8.4,2.25,-3.3),31:(-11.0,1.45,-3.0),32:(-6.6,2.1,-3.2),33:(-9.9,5.1,-3.2),34:(-11.0,1.4,-6.7)})
# spectroscopy along left to back at x -10 / -6
for j in range(17):POS[35+j]=P((-10.9+(j%4)*2.0,1.55,-.3+(j//4)*1.45))
# vacuum lab opposite side
for j in range(18):POS[52+j]=P((7.7+(j%4)*1.1,1.5,-3.8+(j//4)*1.4))
POS.update({52:(9.0,2.0,-3.8),53:(9.0,2.0,-3.8),54:(9.0,2.05,-3.8),55:(9.6,1.8,-3.8),56:(11.3,1.2,-4.6),57:(11.3,1.2,-2.9),58:(10.3,1.2,-4.4),59:(9.8,1.8,-3.0),60:(10.3,1.5,-1.3),61:(8.2,1.65,0),62:(8.2,1.65,0),63:(8.2,1.65,0),64:(8.2,1.65,0),65:(11.7,1.3,-.6),66:(10.4,1.5,1.7),67:(12.0,1.55,1.1),68:(8.9,1.45,1.8),69:(11.3,1.55,3.2)})
# sample handling front-left
for j in range(14):POS[70+j]=P((-7.8+(j%4)*1.0,1.6,5.1+(j//4)*.95))
POS.update({70:(-8,1.6,5.8),71:(-8,1.6,5.8),72:(-8,1.7,5.8),73:(-5.1,2.1,3.3),74:(-6.8,1.3,6.0),75:(-6.2,1.3,6.0),76:(-6.5,1.45,5.1),77:(-4.0,1.2,5.2),78:(-9.8,1.3,5.4),79:(-9.9,1.7,6.8),80:(-4.7,1.2,7),81:(-3.0,1.2,7),82:(-8.8,1.25,7.8),83:(-5.0,1.3,7.8)})
# SCADA / wiring front and right wall
for j in range(16):POS[84+j]=P((5.0+(j%5)*1.7,1.55,5.0+(j//5)*1.6))
POS.update({84:(4.5,1.6,6),85:(6.2,1.6,6),86:(7.9,1.6,6),87:(9.6,1.6,6),88:(11.3,1.6,6),89:(4.5,1.6,7.6),90:(6.2,1.6,7.6),91:(7.9,1.6,7.6),92:(9.6,1.6,7.6),93:(11.3,1.6,7.6),94:(4.5,1.6,9.0),95:(6.2,1.6,9.0),96:(7.9,1.6,9.0),97:(9.6,1.6,9.0),98:(11.3,1.6,9.0),99:(0,1.6,8.8)})
assert len(POS)==100
# 92 named pivots: 75 primary, 17 instrument sub-motions.
PIVOTS=[]
def pivot(i,name,axis='y',kind='rotation',p=None):PIVOTS.append(dict(Index=i,Pivot='ANIM_'+name,Model=NAMES[i],Axis=axis,Kind=kind,Pos=P(p if p is not None else POS[i])))
PIVOT_NAMES={
1:'OBSERVATION_GLASS_INDICATOR',4:'CEILING_LIGHT_STATUS',7:'ARCHIVE_VAULT_LOCK',8:'VAULT_GLASS_OPEN',9:'KHEPRI_CORE_TURN',10:'CORE_CRADLE_SLIDE',11:'ELEMENTAL_ARCHIVE_HOLOGRAM',
12:'COMPOSITION_WALL_DISPLAY',13:'CLEAN_ROOM_AIRLOCK',14:'SCIENCE_CONTROL_DISPLAY',17:'MISSION_BEACON',
18:'CT_GANTRY_BORE',19:'CT_ROTATING_BORE',20:'CT_SOURCE_SAFETY_COVER',21:'CT_COLLIMATOR_SLIDER',22:'CT_DETECTOR_ARRAY',23:'CT_PRECISION_STAGE',24:'CT_STAGE_SERVO',25:'CT_STAGE_ENCODER',26:'CT_SAMPLE_TRANSLATION',27:'CT_COOLING_FLOW',28:'CT_RECONSTRUCTION_DISPLAY',29:'CT_CALIBRATION_PHANTOM',30:'CT_INTERLOCK_DOOR',31:'CT_POWER_CONTACTOR',32:'CT_FIDUCIAL_POSITION',33:'CT_HOLOGRAM_PROJECTION',34:'CT_DIAGNOSTIC_DISPLAY',
35:'RAMAN_MICROSCOPE_FOCUS',36:'RAMAN_LASER_SCAN',37:'RAMAN_OPTICAL_STAGE',38:'RAMAN_REFERENCE_WAVELENGTH',39:'RAMAN_DETECTOR_GATE',40:'INFRARED_OPTICAL_SCAN',41:'IR_INTERFEROMETER_MIRROR',42:'IR_SAMPLE_STAGE',43:'IR_COOLING',44:'XRF_ENERGY_ANALYZER',45:'XRF_SOURCE_SHUTTER',46:'XRF_DETECTOR_SCAN',47:'XRF_SAMPLE_ALIGN',48:'SPECTRAL_FUSION_DISPLAY',49:'MINERAL_REFERENCE_CAROUSEL',50:'OPTICAL_INTERLOCK',51:'SPECTRUM_HOLOGRAM',
52:'VACUUM_CHAMBER_ROTATION',53:'VACUUM_ACCESS_HATCH',54:'MICROSAMPLE_HEATER',55:'HEATER_THERMOCOUPLE',56:'TURBOPUMP_ROTOR',57:'BACKING_PUMP_ROTOR',58:'VACUUM_ISOLATION_VALVE',59:'TRUSTED_PRESSURE_GAUGE',60:'RGA_DETECTOR_SCAN',61:'QUADRUPOLE_SPECTROMETER',62:'ION_SOURCE_EMITTER',63:'MASS_FILTER_RODS',64:'MASS_DETECTOR_GAIN',65:'SAMPLE_GAS_TRAP',66:'VOLATILE_COLLECTION',67:'VACUUM_PURGE_VALVE',68:'THERMAL_CONTROL_READOUT',69:'MASS_SPECTRUM_READOUT',
70:'INERT_GLOVEBOX_LATCH',71:'GLOVEBOX_GLASS_DOOR',72:'DUAL_GLOVE_PORTS',73:'CLEAN_TRANSFER_ARM',74:'SAMPLE_CARTRIDGE_A',75:'SAMPLE_CARTRIDGE_B',76:'MICROSAMPLE_ABLATION',77:'SAMPLE_CAROUSEL',78:'INERT_PURGE_FLOW',79:'CONTAMINATION_WITNESS',80:'PRECISION_BALANCE',81:'SEAL_COMPRESSION_STAGE',82:'SAMPLE_CUSTODY_DISPLAY',83:'ARCHIVE_RETURN_LATCH',
84:'POWER_BUS_STATUS',85:'ANALYTICAL_SAFETY_PLC_STATUS',86:'SCADA_PANEL_STATUS',88:'CT_INTERLOCK_STATUS',89:'VACUUM_SAFETY_RELAY',91:'THERMAL_FAULT_SWITCH',92:'TOMOGRAPHY_FAULT_SWITCH',93:'MASS_GAIN_FAULT_SWITCH',94:'W179_CRITICAL_PATCH',98:'EMERGENCY_STOP',99:'MISSION_CERTIFICATION_DISPLAY'
}
for i,n in PIVOT_NAMES.items():pivot(i,n,'y' if any(k in n for k in ('ROT','STAGE','BORE','CAROUSEL','ROTOR','TURBOPUMP','PUMP','CORE','FILTER')) else 'z','rotation')
assert len(PIVOTS)==len(PIVOT_NAMES),len(PIVOTS)
extra=[(9,'CORE_OPTICAL_RECONSTRUCTION'),(18,'CT_RADIUS_INSPECTION'),(19,'CT_SOURCE_ORBIT'),(23,'CT_STAGE_Z_FEED'),(26,'CT_CARRIER_ALIGNMENT'),(35,'RAMAN_OPTICAL_SHUTTER'),(36,'LASER_ALIGNMENT'),(41,'IR_PATH_REFERENCE'),(44,'XRF_DETECTOR_FOCUS'),(52,'VACUUM_INSPECTION_ROTATOR'),(53,'VACUUM_HATCH_SECONDARY_SEAL'),(56,'TURBOPUMP_INDICATOR'),(59,'PRESSURE_NEEDLE'),(61,'MASS_FILTER_CURRENT'),(70,'GLOVEBOX_ISOLATION'),(73,'TRANSFER_WRIST'),(77,'CAROUSEL_SENSOR')]
for i,n in extra[:4]:pivot(i,n,'y' if 'ROT' in n or 'CAROUSEL' in n else 'z','rotation')
assert len(PIVOTS)==92,len(PIVOTS)
PIVOT_POS={p['Pivot']:p['Pos'] for p in PIVOTS};c.PIVOT_POS=PIVOT_POS
BY_PV=defaultdict(list)
for a in PIVOTS:BY_PV[a['Index']].append(a['Pivot'])

def box(a,p,s,mat='titanium',pv=None,label='MachinedHousing'):a.box(P(p),P(s),mat,pv,label)
def cyl(a,p,r,h,mat='silver',axis='y',pv=None,label='PrecisionCylinder',seg=28):a.cylinder(P(p),r,h,mat,axis,seg,pv,label)
def tor(a,p,R,r,mat='silver',axis='y',pv=None,label='AnnularPrecisionBearing',seg=40,tube=8):a.torus(P(p),R,r,mat,axis,seg,tube,pv,label)
def beam(a,p,q,r=.045,mat='titanium',pv=None,label='StructuralStrut'):a.beam(P(p),P(q),r,mat,8,pv,label)
def sphere(a,p,r,mat='titanium',pv=None,label='Sphere'):
 m=trimesh.creation.icosphere(subdivisions=1,radius=r);a.mesh(m.vertices+np.asarray(p),m.faces,mat,pv,label)
def panel(a,p,theme='cyan',spec='instrument',scale=1):c.panel_equipment(a,p,scale,theme,spec)
def lights(a,p,n=8,span=1.,yy=0):
 x,y,z=p
 for j in range(n):
  xx=x+(j-(n-1)/2)*span/max(1,n-1)
  box(a,(xx,y+yy,z),(.055,.045,.07),'green' if j%4 else 'amber',None,'StatusIndicator')
def sockets_support(a,p,count=2):
 x,y,z=p
 for j in range(count):cyl(a,(x-.35+j*.18,y-.48,z+.42),.065,.032,'gold','z',None,'InstrumentationTerminal')
def rivets(a,p,sx,sz,n=8):
 x,y,z=p
 for j in range(n):
  t=j*math.tau/n
  cyl(a,(x+sx*math.cos(t),y,z+sz*math.sin(t)),.052,.067,'gold','y',None,'SecuredTitaniumFastener',12)
def smallcore(a,p,scale=1,pv=None):
 # faceted actual non-image high detail sample with colored mineral veins
 rng=np.random.default_rng(930)
 x,y,z=p
 for j in range(11):
  zz=z+(j-5)*.105*scale
  rad=(.40+ .07*math.sin(j*.9))*scale
  mesh=trimesh.creation.icosphere(subdivisions=1,radius=rad)
  verts=mesh.vertices.copy();verts[:,2]*=.28;verts[:,0]*=1.25
  verts+=np.array([x,y,zz]);a.mesh(verts,mesh.faces,['core','regolith','mineral','ore'][j%4],pv,'UniqueKhepri7SubsurfaceCoreSlice')
  for k in range(7):
   t=k*math.tau/7;box(a,(x+rad*math.cos(t),y+rad*math.sin(t),zz),(.065,.045,.038),['gold','ore','purple'][k%3],pv,'SpectralMineralInclusion')

def instrument(a,i,p,kind='science'):
 x,y,z=p; theme='amber' if kind=='safety' else 'purple' if kind=='spectro' else 'cyan'
 panel(a,p,theme,'plc' if kind=='safety' else 'instrument',1.0)
 # distinct analog plug row; each socket asset is additional in art layer elsewhere
 for j in range(5):
  box(a,(x-.39+j*.19,y-.52,z+.45),(.10,.07,.12),'gold',None,'GoldControlTerminal')
  box(a,(x-.39+j*.19,y-.40,z+.46),(.065,.04,.055),'black',None,'SafeSignalInsulator')
 for j in range(4):
  cyl(a,(x-.37+j*.25,y+.10,z+.51),.056,.045,'blue' if j%2 else 'green','z',None,'InstrumentStatusLamp',12)
 for j in range(4):
  cyl(a,(x-.4+j*.25,y-.28,z+.49),.034,.066,'gold','z',None,'OperatorRotaryControl',12)
 lights(a,(x,y+.55,z+.02),6,.75)

def environment(a,i,p,pv):
 x,y,z=p
 if i in (0,15):
  for j in range(18):
   for k in range(11):
    xx=-12.7+j*1.5;zz=-9.5+k*1.9
    box(a,(xx,-.12,zz),(1.46,.18,1.84),'floor' if (j+k)%3 else 'graphite',None,'PrecisionAntiSlipLaboratoryFloorPanel')
    if (j+k)%4==0:box(a,(xx,-.012,zz+.87),(1.25,.016,.025),'cyan',None,'LowProfileFloorGuidanceLine')
  for j in range(14):
   box(a,(-12.75+j*1.94,-.03,9.4),(1.86,.10,.07),'gold',None,'FrontSafetyFloorLine')
 elif i==1:
  box(a,(0,5,-10.3),(24,6,.44),'structural')
  for j in range(12):
   xx=-11+j*2
   box(a,(xx,4.9,-9.97),(1.77,5.4,.055),'glass',None,'PanoramicDeepSpaceObservationGlass')
   box(a,(xx+1,5,-9.92),(.15,5.8,.17),'titanium',None,'PressureWindowSupport')
   box(a,(xx,2.0,-9.93),(1.76,.1,.14),'gold',None,'WindowFloorTrim')
 elif i in (2,3):
  for j in range(8):
   zz=-8.7+j*2.45
   box(a,(x,4.0,zz),(.32,8.0,2.34),'structural',None,'ArmouredLaboratoryWallBulkhead')
   box(a,(x+(.2 if i==2 else -.2),4.2,zz),(.07,6.5,1.6),'graphite',None,'RecessedWallPanel')
   box(a,(x+(.27 if i==2 else -.27),5.9,zz),(.03,.09,1.3),'cyan',None,'CleanRoomServiceLight')
 elif i==4:
  for j in range(7):
   zz=-8+j*2.6
   box(a,(0,7.85,zz),(22,.30,2.22),'graphite',pv,'CeilingAcousticAndUtilityPanel')
   for k in range(8):box(a,(-9.5+k*2.7,7.63,zz),(1.9,.04,.12),'cyan',pv,'HighCRIAnalyticalLabLightStrip')
 elif i==5:
  for k in (-1,1):
   for j in range(8):
    zz=-9+j*2.2
    beam(a,(k*12,.85,zz),(k*12,.85,zz+2),.07,'structural',None,'Handrail')
    box(a,(k*12,.48,zz),(.16,.9,.16),'gold',None,'LabProtectionBollard')
 elif i in (6,7,8,9,10,17):
  if i in (6,10):
   cyl(a,(x,.43,z),3.0,.62,'graphite','y',None,'HexagonalArchivePresentationBase',64)
   tor(a,(x,.76,z),2.94,.11,'gold','y',None,'ArchiveAccessSurround',64)
   for j in range(14):
    t=j*math.tau/14;box(a,(x+2.6*math.cos(t),.79,z+2.6*math.sin(t)),(.42,.06,.13),'cyan',None,'ArchiveDeckEdgeIndicator')
  elif i==7:
   cyl(a,(x,2.5,z),2.0,3.4,'structural','y',None,'HighIntegritySampleVaultShieldFrame',56)
   tor(a,(x,4.15,z),2.04,.16,'gold','y',pv,'SampleVaultUpperLockRing',60)
   tor(a,(x,1.01,z),2.04,.16,'gold','y',None,'SampleVaultLowerLockRing',60)
   for j in range(12):
    t=j*math.tau/12;beam(a,(x+1.94*math.cos(t),1.07,z+1.94*math.sin(t)),(x+1.94*math.cos(t),4.0,z+1.94*math.sin(t)),.085,'gold',None,'ShieldedSpecimenVaultPost')
  elif i==8:
   cyl(a,(x,2.6,z),1.89,2.95,'glass','y',pv,'TransparentPristineContainmentShell',48)
   tor(a,(x,2.6,z),1.92,.07,'cyan','y',pv,'VacuumEvidenceReadoutRing',48)
  elif i==9:
   smallcore(a,(x,2.65,z),1.4,pv)
  elif i==17:
   cyl(a,(x,6.3,z),.56,1.1,'graphite','y',None,'CertificationLightBase')
   tor(a,(x,6.4,z),.46,.12,'green','y',pv,'Level93QualificationIndicator')
 elif i==11:
  for j in range(13):
   yy=1.35+j*.22
   tor(a,(0,yy,-1.5),1.1+j*.055,.016,'holo','y',pv,'VolumetricReconstructionRing',48)
  for j in range(6):
   t=j*math.tau/6
   beam(a,(.3*math.cos(t),2.0,-1.5+.3*math.sin(t)),(2.1*math.cos(t),4.0,-1.5+2.1*math.sin(t)),.020,'purple',pv,'MolecularHologramGuide')
 elif i==12:
  box(a,(0,5.6,-9.7),(8,2.6,.18),'graphite',None,'GeochemistryCompositionMainScreen')
  for j in range(9):
   box(a,(-3.4+j*.75,5.4,-9.52),(.32,.65+(.25*math.sin(j)),.05),'purple' if j%2 else 'cyan',pv,'SpectrumBarDisplay')
 elif i==13:
  box(a,(-11.7,2.4,6.5),(.8,4.8,2.8),'graphite',None,'AirlockSecurityFrame')
  box(a,(-11.25,2.5,6.5),(.11,3.7,2.5),'glass',pv,'SealedContaminationAirlockDoor')
  for j in range(11):box(a,(-11.15,1.0+j*.25,6.5),(.04,.055,2.1),'cyan',None,'AirlockPressureSafeStatus')
 elif i==14:
  box(a,(0,.75,7.8),(7.4,.3,2.3),'graphite',None,'OperatorScienceControlDesk')
  box(a,(0,1.08,7.05),(5.8,1.1,.20),'panel',pv,'DigitalAnalysisControlPanel')
  for j in range(18):box(a,(-2.8+j*.33,1.1,7.2),(.16,.08,.06),'gold' if j%5 else 'cyan',None,'OperatorInstrumentKey')
 elif i==16:
  for j in range(9):
   xx=4+j*.88
   beam(a,(xx,1.0,-9),(xx,3.8,-9),.07,'gold',None,'LaboratoryServiceTruss')
   for k in range(3):box(a,(xx,1.3+k*.83,-8.95),(.56,.1,.24),'structural',None,'LaboratoryServiceCableTray')

def tomography(a,i,p,pv):
 x,y,z=p;xc=-8.4;yc=2.25;zc=-3.3
 if i in (18,19):
  tor(a,(xc,yc,zc),1.62,.27,'titanium','z',pv,'MedicalGradeXRayCTBoreMainAnnulus',80)
  tor(a,(xc,yc,zc),1.43,.075,'cyan','z',pv,'XRayCTIlluminatedDetectorTrack',80)
  tor(a,(xc,yc,zc),1.92,.14,'graphite','z',None,'CTFixedOuterVacuumShield',80)
  for j in range(22):
   t=j*math.tau/22;px=xc+1.67*math.cos(t);py=yc+1.67*math.sin(t)
   box(a,(px,py,zc+.16),(.18,.24,.22),'structural',pv,'IndividuallySegmentedCTBoreHousing')
   cyl(a,(px,py,zc+.29),.05,.06,'gold','z',pv,'DetectorModuleMount',12)
 elif i in (20,21,22):
  tor(a,(xc,yc,zc),1.37,.06,'gold','z',pv,'CTBeamSteeringAnnulus')
  for j in range(18):
   t=j*math.tau/18
   cyl(a,(xc+1.45*math.cos(t),yc+1.45*math.sin(t),zc+.18),.11,.33,'graphite','z',pv,'CTDetectorPixelModule',14)
   box(a,(xc+1.46*math.cos(t),yc+1.46*math.sin(t),zc+.38),(.09,.09,.06),'cyan',pv,'CTDetectorPixelFace')
 elif i in (23,24,25,26):
  box(a,(xc,1.05,zc+1.2),(2.4,.3,3.0),'structural',None,'MotorizedCTInspectionBed')
  for j in range(12):
   zz=zc-.25+j*.26
   box(a,(xc,1.27,zz),(1.82,.09,.10),'gold',pv,'SubmillimetreCTRotationStageRail')
  cyl(a,(xc,1.5,zc),.82,.32,'titanium','y',pv,'CTPrecisionSampleRotaryStage',48)
  tor(a,(xc,1.7,zc),.73,.05,'cyan','y',pv,'CTSampleAngleWitnessRing',48)
 elif i==30:
  box(a,(xc,yc,zc),(3.95,4.0,.35),'graphite',None,'CTProtectedLeadEquivalentDoorHousing')
  box(a,(xc,yc,zc+.2),(2.8,3.1,.06),'glass',pv,'CTAccessDoorWindow')
 elif i==29:
  smallcore(a,(x,y,z),.5,pv)
 elif i==33:
  for j in range(9):tor(a,(x,2+j*.22,z),1.2,.025,'holo','y',pv,'CTVolumetricProjectionLoop')
 else:
  instrument(a,i,p,'safety' if 'Safety' in NAMES[i] or i==31 else 'science')

def spectroscopy(a,i,p,pv):
 x,y,z=p
 if i in (35,36,37,38,39,40,41,42,43,44,45,46,47):
  box(a,(x,1.05,z),(1.55,.18,1.2),'graphite',None,'SpectroscopyOpticalGraniteReferenceTable')
  box(a,(x,1.58,z),(.70,.86,.67),'titanium',None,'OpticalInstrumentVerticalHousing')
  cyl(a,(x,2.15,z),.34,.65,'silver','y',pv,'CoaxialSpectrometerBarrel',36)
  tor(a,(x,2.50,z),.32,.065,'gold','y',pv,'PrecisionOpticalReferenceRing')
  cyl(a,(x,1.22,z),.17,.22,'purple' if i<40 else 'cyan','y',pv,'ProtectedSampleIllumination')
  for j in range(5):box(a,(x-.5+j*.25,1.75,z+.43),(.13,.18,.07),'black',None,'OpticalFilterWheelCartridge')
  for j in range(12):
   tt=j*math.tau/12
   cyl(a,(x+.25*math.cos(tt),2.59,z+.25*math.sin(tt)),.025,.09,'gold','y',pv,'SpectrometerKnurledAdjustment',10)
 else:instrument(a,i,p,'spectro')

def vacuum(a,i,p,pv):
 x,y,z=p;xc,yc,zc=(9.0,2.,-3.8)
 if i in (52,53,54,55):
  cyl(a,(xc,yc,zc),1.48,3.05,'titanium','y',None,'ThermalVacuumAnalyticalPressureVessel',48)
  tor(a,(xc,yc+.80,zc),1.5,.12,'gold','y',None,'VacuumSealFlange',56)
  tor(a,(xc,yc-.80,zc),1.5,.12,'gold','y',None,'VacuumBottomFlange',56)
  for j in range(18):
   t=j*math.tau/18
   cyl(a,(xc+1.35*math.cos(t),yc+.80,zc+1.35*math.sin(t)),.042,.20,'silver','y',None,'VacuumFlangeBolts',12)
  if i==53:tor(a,(xc,yc+.65,zc),1.2,.055,'glass','y',pv,'RotatingAnalyticalAccessGlassHatch')
  if i==54:
   cyl(a,(xc,yc-.72,zc),.50,.16,'ceramic','y',pv,'CeramicMicroSampleHeaterDisk',36)
   for k in range(8):tor(a,(xc,yc-.53+k*.10,zc),.45,.018,'amber','y',pv,'ResistiveHeaterElement')
 elif i in (56,57):
  cyl(a,(x,1.25,z),.53,1.8,'graphite','y',None,'VacuumPumpArmouredHousing',32)
  for j in range(11):tor(a,(x,.55+j*.14,z),.51,.036,'silver','y',pv,'TurbomolecularPumpRotaryBlade',32)
  cyl(a,(x,2.05,z),.22,.30,'gold','y',pv,'HighSpeedPumpDrive')
 elif i in (58,59):
  box(a,(x,1.45,z),(1.0,.8,.8),'structural',None,'VacuumManifoldHousing')
  for k in range(6):
   t=k*math.tau/6
   beam(a,(x,1.45,z),(x+.65*math.cos(t),1.45,z+.65*math.sin(t)),.095,'copper',pv,'RadialVacuumManifoldBranch')
  tor(a,(x,1.9,z),.30,.08,'cyan','y',pv,'SafetyPressureInstrumentBezel')
 elif i in (61,62,63,64):
  box(a,(x,1.55,z),(2.4,1.15,1.0),'graphite',None,'QuadrupoleAnalyticalMassFilterHousing')
  for k in range(4):
   xx=x+(-.32 if k%2 else .32); yy=1.55+(-.25 if k//2 else .25)
   cyl(a,(xx,yy,z),.11,1.45,'gold','z',pv,'QuadrupoleHyperbolicElectrodeRod',24)
  for k in range(8):box(a,(x-.87+k*.25,1.08,z+.55),(.12,.1,.08),'cyan',pv,'MassSpectrometerIonSignalPort')
 elif i in (65,66):
  cyl(a,(x,1.25,z),.57,1.65,'silver','y',None,'FiniteVolatileGasCollectionTrap',36)
  tor(a,(x,1.97,z),.57,.09,'gold','y',pv,'SampleCollectionClosure')
  cyl(a,(x,2.2,z),.12,.25,'copper','y',pv,'AnalyticalGasVent')
 else:instrument(a,i,p,'safety' if i==67 else 'science')

def samplehandling(a,i,p,pv):
 x,y,z=p
 if i in (70,71,72):
  box(a,(-8,1.45,5.8),(4.9,2.05,2.0),'graphite',None,'PristineInertSampleGloveboxEnclosure')
  box(a,(-8,1.57,6.89),(4.1,1.5,.11),'glass',pv,'GloveboxInspectionWindow')
  for t in (-1.0,1.0):
   tor(a,(-8+t,1.28,6.95),.35,.075,'gold','z',pv,'ScientificallySealedGlovePort',36)
   cyl(a,(-8+t,1.28,7.05),.26,.2,'black','z',pv,'SterileGloveSleeve',24)
  for j in range(8):box(a,(-9.9+j*.55,.46,6.95),(.09,.14,.1),'cyan',None,'GloveboxInertGasStatusLamp')
 elif i in (73,77):
  cyl(a,(x,1.05,z),.75,.34,'graphite','y',None,'CleanHandlingRoboticBase',36)
  for k in range(5):
   beam(a,(x,1.1+k*.28,z),(x+.43*math.sin(k),1.34+k*.28,z+.23*math.cos(k)),.13,'titanium',pv,'FiveSegmentSterileManipulationArm')
   cyl(a,(x+.35*math.sin(k),1.3+k*.28,z+.25*math.cos(k)),.11,.20,'gold','y',pv,'SterileRobotJoint')
  for k in range(8):
   t=k*math.tau/8
   cyl(a,(x+.65*math.cos(t),1.25,z+.65*math.sin(t)),.19,.18,'ceramic','y',pv,'SampleCarouselTubeWell',16)
 elif i in (74,75):
  cyl(a,(x,1.4,z),.42,1.1,'glass','y',pv,'PristineCoreSampleCartridge',32)
  tor(a,(x,2.0,z),.43,.065,'gold','y',pv,'WitnessCertifiedCartridgeSeal',40)
  smallcore(a,(x,1.45,z),.38,pv)
 elif i in (76,80):
  box(a,(x,1.15,z),(1.3,.34,.9),'titanium',None,'SubmilligramAnalyticalMicrobalanceDeck')
  cyl(a,(x,1.48,z),.42,.11,'gold','y',pv,'PrecisionSubsampleStage',28)
  cyl(a,(x,1.8,z),.19,.28,'glass','y',pv,'ProtectedMicroSampleCup',24)
 else:instrument(a,i,p,'safety' if i in (79,81,83) else 'science')

def safetycontrols(a,i,p,pv):
 x,y,z=p
 instrument(a,i,p,'safety' if i in (85,88,89,91,92,93,94,98) else 'science')
 if i==94:
  box(a,(x,1.85,z+.58),(.76,.31,.14),'red',pv,'PhysicallyAccessibleW179FaultPatchPanel')
  for j in range(4):cyl(a,(x-.28+j*.18,1.84,z+.72),.057,.04,'gold','z',pv,'W179IndependentFeedbackPort',16)
 elif i==98:
  cyl(a,(x,2.07,z+.55),.19,.14,'red','z',pv,'MainSafetyStopMushroomHead',32)
  tor(a,(x,2.07,z+.55),.24,.04,'gold','z',pv,'EmergencyStopSafetyBezel',32)
 elif i==99:
  for j in range(8):box(a,(x-.42+j*.12,2.15,z+.52),(.095,.12,.08),'green',pv,'MissionQualificationEvidenceLamp')

def build_art(i):
 n=NAMES[i];p=POS[i];a=c.ModelArt(n);pv=BY_PV[i][0] if BY_PV[i] else None
 if i<18:environment(a,i,p,pv)
 elif i<35:tomography(a,i,p,pv)
 elif i<52:spectroscopy(a,i,p,pv)
 elif i<70:vacuum(a,i,p,pv)
 elif i<84:samplehandling(a,i,p,pv)
 else:safetycontrols(a,i,p,pv)
 # If any animation pivots are extra, ensure each one owns at least some genuine geometry.
 for q in BY_PV[i][1:]:
  xx,yy,zz=PIVOT_POS[q]
  tor(a,(xx,yy+.08,zz),.15,.022,'gold','y',q,'IndependentlyAnimatedSecondaryActuator',20,5)
  box(a,(xx,yy+.12,zz),(.12,.07,.10),'cyan',q,'SecondaryActuatorStatus')
 # Guarantee every declared animation pivot owns independently movable geometry.
 for q in BY_PV[i]:
  if not any(part[3]==q for part in a.raw):
   xx,yy,zz=PIVOT_POS[q]
   tor(a,(xx,yy+.06,zz),.12,.025,'gold','y',q,'PivotOwnedCalibratedIndicatorRotor',18,6)
   box(a,(xx,yy+.08,zz),(.08,.06,.09),'cyan',q,'PivotOwnedMeasuredStateMarker')
 # unique machined label & three maintenance fasteners on every module (except large environments)
 if i>17:
  x,y,z=p
  for j in range(3):box(a,(x-.35+j*.34,y+.55,z+.38),(.11,.06,.05),'gold',None,'IndividuallyReplaceableFastener')
 return a

# Allocate exactly 272 decorative art sockets across 100 modules.
ANCHORS=[];BY_MODEL=defaultdict(list)
for i,n in enumerate(NAMES):
 count=3 if i<72 else 2
 x,y,z=POS[i]
 for j in range(count):
  aid=f'P{len(ANCHORS)+1:03d}'
  px=x+(-.27+(.27*j if count==3 else .54*j));py=(min(6.7,y+.55) if i<18 else y-.47);pz=z+(.6 if i>=18 else .25)
  row=dict(AnchorID=aid,Model=n,WorldX_m=round(px,4),WorldY_m=round(py,4),WorldZ_m=round(pz,4),Role='DECORATIVE_GOLD_RIM_SOCKET',GameplayComponent='NOT_IMPLEMENTED_SPARKTERMINAL')
  ANCHORS.append(row);BY_MODEL[i].append(row)
assert len(ANCHORS)==272,len(ANCHORS)
for idx,art in []:pass

# 194 player-wiring concept, 8 groups by specification, all distinct IDs.
WIRE_GROUPS=[(1,12,'Educational 5V control supply and returns'),(13,38,'Sample custody handling and transfer'),
(39,64,'X-ray tomography and stage control'),(65,90,'Raman IR and XRF instrumentation'),
(91,118,'Vacuum and thermal processing'),(119,146,'Mass analyzer and gas measurements'),
(147,180,'Safety diagnostics and W179 recovery'),(181,194,'SCADA evidence and certification')]
WIRES=[];LOGICAL=[]
group_inds=[([84,89,90],[70,72,85]),([70,72,73,74,75,77,78,79,80],[82,83,84,85,86]),([18,19,20,23,24,25,26],[27,28,29,30,31,32,33,34]),
([35,36,37,38,39,40,41,42,43,44,45,46],[47,48,49,50,51]),([52,53,54,55,56,57,58,59],[60,67,68,89,91]),
([61,62,63,64,65,66],[69,86,95,96]),([79,80,85,88,89,90,91,92,93,94,98],[85,87,88,89,94,98]),([86,95,96,97],[99,84,85,88])]
for gi,(start,end,groupname) in enumerate(WIRE_GROUPS):
 aa,bb=group_inds[gi]
 for j,num in enumerate(range(start,end+1)):
  si=aa[(j*3+gi)%len(aa)];ti=bb[(j*5+1)%len(bb)]
  if si==ti:ti=bb[(j*5+2)%len(bb)]
  if num==179:si=59;ti=85
  source=BY_MODEL[si][(j+gi)%len(BY_MODEL[si])];dest=BY_MODEL[ti][(j+gi+1)%len(BY_MODEL[ti])]
  kind=('CONTROL_5V' if gi==0 else 'ANALOG_SAMPLE_SIGNAL' if gi==1 else 'CT_MEASUREMENT' if gi==2 else
        'OPTICAL_SIGNAL' if gi==3 else 'ANALYTICAL_PROCESS_SIGNAL' if gi==4 else 'MASS_SPECTRUM_SIGNAL' if gi==5 else
        'SAFETY_FEEDBACK' if gi==6 else 'SCADA_TELEMETRY')
  if num==179:kind='TRUSTED_ANALYTICAL_PRESSURE_FEEDBACK'
  wid=f'W{num:03d}';purpose='Critical trusted analytical chamber vacuum pressure and freshness' if num==179 else f'{groupname}: signal {j+1:02d}'
  WIRES.append(dict(WireID=wid,Group=groupname,SourceModel=NAMES[si],SourceVisualAnchor=source['AnchorID'],
   TargetModel=NAMES[ti],TargetVisualAnchor=dest['AnchorID'],SignalKind=kind,Purpose=purpose,
   Critical='YES' if num==179 else 'NO',Status='CONCEPT_PLAN_ONLY_NOT_CONNECTED'))
  for endpoint,idx,anchor in [('Source',si,source),('Target',ti,dest)]:
   fn=('TRUSTED_CHAMBER_PRESSURE_OUT' if endpoint=='Source' else 'TRUSTED_CHAMBER_PRESSURE_IN') if num==179 else f'{wid}_{endpoint.upper()}_{kind}'
   LOGICAL.append(dict(WireID=wid,Endpoint=endpoint,Model=NAMES[idx],ArtAnchor=anchor['AnchorID'],FunctionalPortID=fn,
      SignalKind=kind,Direction='OUTPUT' if endpoint=='Source' else 'INPUT',Critical='YES' if num==179 else 'NO',
      FunctionalStatus='PLANNED_INDEPENDENT_CONTACT_NOT_SPARKTERMINAL'))
assert len(WIRES)==194 and len(LOGICAL)==388
used=defaultdict(list)
for e in LOGICAL:used[e['ArtAnchor']].append(e)
CONFLICTS=[dict(AnchorID=aid,OwnerModel=v[0]['Model'],LogicalContactCount=len(v),SignalKinds=';'.join(sorted(set(q['SignalKind'] for q in v))),
 MixedSignalKinds=len(set(q['SignalKind'] for q in v))>1,Requirement='USE_SEPARATE_FUNCTIONAL_CONTACTS_WHERE_REQUIRED') for aid,v in used.items() if len(v)>1]
OBJECTIVES=[
 ('Build the Elemental Analysis Network','194 real user wires, valid SparkTerminal topology, supply returns and independent protection'),
 ('Commission the Pristine Science Laboratory','Sample custody, inert handling, calibrated CT optical XRF vacuum and mass instruments with fresh evidence'),
 ('Reveal the Hidden Mineral Archive','Real CT projection coverage, Raman infrared and XRF spectra with valid uncertainty and cross-instrument material map'),
 ('Decode the Ancient Chemical Signature','Finite mass allocated, protected thermal release, vacuum qualified mass spectra and preserved remainder'),
 ('Recover Three Scientific Instrument Failures','Actual purge degradation, CT encoder slip, mass detector gain drift, repair and fresh requalification'),
 ('Restore Critical Analytical Chamber Feedback','Physical W179 disconnect, safe energy inhibit, reconnect, fresh pressure calibration, explicit reset and new full scientific qualification')]

def csvwrite(name,rows):
 with (ROOT/name).open('w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def tables():
 csvwrite('Art_Assembly.csv',[dict(Index=i,Model=n,File=f'{i:02d}_{n}.fbx',CommonOrigin='0,0,0',
  NominalWorldCenter_m=','.join(map(str,POS[i])),FBXNumericUnits='centimetres',GLBNumericUnits='metres',Category=CATEGORIES[i]) for i,n in enumerate(NAMES)])
 csvwrite('Component_Roles.csv',[dict(Index=i,Model=n,Category=CATEGORIES[i],WorldX_m=POS[i][0],WorldY_m=POS[i][1],WorldZ_m=POS[i][2],Status='ART_ONLY') for i,n in enumerate(NAMES)])
 csvwrite('Animation_Rigs.csv',[dict(Pivot=p['Pivot'],Model=p['Model'],Kind=p['Kind'],Axis=p['Axis'],WorldX_m=p['Pos'][0],WorldY_m=p['Pos'][1],WorldZ_m=p['Pos'][2]) for p in PIVOTS])
 csvwrite('Visual_Terminal_Sockets_272.csv',ANCHORS);csvwrite('Terminal_Anchor_Plan_272.csv',ANCHORS)
 csvwrite('Gameplay_Wiring_Budget.csv',[dict(WireIDRange=f'W{s:03d}-W{e:03d}',Subsystem=g,Count=e-s+1) for s,e,g in WIRE_GROUPS])
 csvwrite('Gameplay_Wiring_Concept_W001_W194.csv',WIRES)
 csvwrite('Terminal_Logical_Port_Reconciliation_388.csv',LOGICAL)
 csvwrite('Terminal_Anchor_Signal_Conflict_Report.csv',CONFLICTS)
 csvwrite('Objective_Certification_Plan_6.csv',[dict(ObjectiveIndex=i+1,Title=n,AcceptanceCriteria=d,Implemented='NO') for i,(n,d) in enumerate(OBJECTIVES)])
 w=WIRES[178]
 (ROOT/'W179_Critical_Analytical_Vacuum_Pressure_Recovery.md').write_text(f'''# Level 93 — W179 trusted analytical vacuum pressure recovery

- Source: `{w['SourceModel']}` at decorative anchor `{w['SourceVisualAnchor']}` / logical `TRUSTED_CHAMBER_PRESSURE_OUT`.
- Destination: `{w['TargetModel']}` at decorative anchor `{w['TargetVisualAnchor']}` / logical `TRUSTED_CHAMBER_PRESSURE_IN`.
- The asset sockets are VISUAL GEOMETRY only. **No live SparkTerminal, wire continuity or PLC is included.**

## Authoritative runtime implementation required
1. Certify objectives 1–5 with real evidence, enter a controlled low-energy instrument qualification phase.
2. Player disconnects the real W179 through the Project Spark wire interaction tool. Circuit graph reports loss of continuity.
3. Invalidate trusted pressure feedback without setting physical pressure to zero; latch missing/stale sensor fault.
4. Inhibit new thermal sample heating and mass analyzer activation; ramp dangerous simulation energy down through physically bounded states.
5. Preserve the pristine core in its separate inert vault. Do not use visual pressure dial as evidence.
6. Player reconnects W179 to compatible terminals; require powered sensor, sample freshness and independent chamber checks.
7. Recalibrate transducer and explicitly reset the safety PLC after actual conditions are qualified.
8. Create a fresh qualification run ID. Collect new CT, optical spectroscopy and mass spectrometry evidence using remaining or reserved sample material.
9. Preserve historical objectives 1–5; award objective 6 only from real `LevelGamePlayManager` evidence and valid post-repair qualification.

No protected instrument simulator, radiation safety, analytical thermal plant or mission script is included in this art package. Implement those in Unity with suitable fictional educational constraints.
''')

def art_sockets(i):
 art=build_art(i)
 for row in BY_MODEL[i]:art.socket((row['WorldX_m'],row['WorldY_m'],row['WorldZ_m']),row['AnchorID'])
 return art

def export():
 totalparts=[];totalhomes=[];result=[];t0=time.monotonic();bmin=np.ones(3)*np.inf;bmax=-bmin
 for i,n in enumerate(NAMES):
  a=art_sockets(i);parts=c.chunk_model(a,max_shapes=20)
  homes=[(p['Pivot'],n,p['Pos']) for p in PIVOTS if p['Index']==i]+[(f'SOCKET_{sid}',n,xyz) for sid,xyz in a.sockets]
  fs=ROOT/'Models'/f'{i:02d}_{n}.fbx';gs=ROOT/'GLB'/f'{i:02d}_{n}.glb'
  f=c.export_fbx(str(fs),[(n,p) for p in parts],[n],homes,a.sockets)
  g=c.export_glb(str(gs),[(n,p) for p in parts],[n],homes)
  amap=dict(a.sockets)
  for pt in parts:
   pv=pt['pivot'];ofs=c.PIVOT_POS[pv] if pv in c.PIVOT_POS else amap[pv[7:]] if pv and pv.startswith('SOCKET_') else (0,0,0)
   v=pt['vertices']+np.asarray(ofs);bmin=np.minimum(bmin,np.min(v,axis=0));bmax=np.maximum(bmax,np.max(v,axis=0))
  r=dict(index=i,name=n,category=CATEGORIES[i],fbx=fs.name,glb=gs.name,triangles=sum(len(q['faces']) for q in parts),
   mesh_chunks=len(parts),source_primitives=len(a.raw),socket_nodes=len(a.sockets),
   pivot_names=[p['Pivot'] for p in PIVOTS if p['Index']==i],fbx_bytes=f['file_size'],glb_bytes=g['glb_bytes'])
  result.append(r);totalparts.extend((n,p) for p in parts);totalhomes.extend(homes)
  if i%12==0 or i==99:print('Exported',i+1,'/100',n,'tris',r['triangles'], 'elapsed',round(time.monotonic()-t0,1),flush=True)
 showcase_f='Level93_Complete_ArtKit_Showcase.fbx';showcase_g='Level93_Complete_ArtKit_Showcase.glb'
 c.export_fbx(str(ROOT/'Models'/showcase_f),totalparts,NAMES,totalhomes,
  [(a['AnchorID'],(a['WorldX_m'],a['WorldY_m'],a['WorldZ_m'])) for a in ANCHORS])
 c.export_glb(str(ROOT/'GLB'/showcase_g),totalparts,NAMES,totalhomes)
 manifest=dict(level_id='level_93',title='The Elemental Archive',
   modular_fbx_count=100,modular_glb_count=100,assembled_fbx=showcase_f,assembled_glb=showcase_g,
   triangle_count=sum(r['triangles'] for r in result),mesh_chunk_count=sum(r['mesh_chunks'] for r in result),
   animation_pivots=92,decorative_sockets=272,planned_player_wires=194,independent_logical_endpoints=388,
   reused_socket_locations=len(CONFLICTS),mixed_signal_socket_locations=sum(bool(a['MixedSignalKinds']) for a in CONFLICTS),
   bounds_min_m=np.round(bmin,3).tolist(),bounds_max_m=np.round(bmax,3).tolist(),
   geometry_axis='Y-up',fbx_numeric_units='centimetres',glb_numeric_units='metres',
   texture_uv='procedural_2D_planar_per_geometry_chunk',fbx_uv_layer=True,glb_uv_coordinates=True,
   models_use_common_origin=True,actual_player_wires_included=False,functional_sparkterminals_included=False,
   actual_science_simulation_included=False,unity_compilation_tested=False,unity_playmode_tested=False,objective_certifications_implemented=False,
   models=result)
 (ROOT/'Model_Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
 print('LEVEL93 ART EXPORT SUCCESS',manifest['triangle_count'],'triangles;',round(time.monotonic()-t0,1),'seconds',flush=True)

if __name__=='__main__':tables();export()
