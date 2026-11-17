#!/usr/bin/env python3
"""Project Spark — Level 94 / The Board Foundry.
Original real modular binary FBX 7400 and glTF GLB 2.0 mesh source.
Art coordinates are presentation-scale (14x physical PCB), not circuit dimensions.
Meshes are generated without finished wiring: routing tasks are separately defined in CSV.
"""
from pathlib import Path
from collections import defaultdict,Counter
import math,csv,json,hashlib,time
import numpy as np, trimesh
import geometry_export_core as g
ROOT=Path(__file__).resolve().parents[1]
for d in ('Models','GLB','Textures','Materials','Shaders','Unity','Data','Docs','Reports'): (ROOT/d).mkdir(exist_ok=True)
NAMES=[
'AssemblyBenchFrame','AntiStaticWorkSurface','PrecisionFixtureBase','FixtureClampLeft','FixtureClampRight','InspectionMicroscopeArm','MicroscopeLensAndRingLight','ComponentRepairTray',
'FR4BoardSubstrate','TopGreenSoldermask','BottomGreenSoldermask','TopSMDGoldPads','BottomCopperPadSet','SilkscreenAndTestFiducials','CopperEdgeAndMountBosses','W035DetachableFeedbackJumper',
'U1_MCU_QFN48','U2_LDO_SOT223','U3_I2CTemperatureSensor_SOIC8','U4_CurrentMonitor_SOIC8','Q1_LoadMOSFET_SOT23','D1_InputSchottkySMA','LED1_PowerGreen','LED2_StatusBlue','LED3_LoadAmber',
'R1_SCLPullup_4K7','R2_SDAPullup_4K7','R3_GateSeries_100R','R4_GatePulldown_100K','R5_PowerLED_1K','R6_StatusLED_330R','R7_LoadLED_330R','R8_Shunt_0R1',
'C1_InputBulk_10u','C2_OutputBulk_22u','C3_MCUDecoupling_100n','C4_SensorDecoupling_100n','C5_CurrentMonitorDecoupling_100n','C6_ResetFilter_10n','C7_AnalogFilter_1u',
'SW1_TactileReset','J1_Protected5VTerminal','J2_DebugProgrammingHeader',
'BenchOscilloscope','BenchDigitalMultimeter','ProtectedBenchPowerSupply','FourChannelLogicAnalyzer','PCBAssemblySCADAPanel']
assert len(NAMES)==48
MODEL_NAMES=[f'L94_{n}' for n in NAMES]
CATEGORIES=['BenchAndFixture']*8+['BarePCBAndPads']*8+['BoardComponents']*27+['TestInstruments']*5
assert len(CATEGORIES)==len(MODEL_NAMES)
COLORS={
'frame':(.22,.27,.32,1),'graphite':(.065,.083,.11,1),'steel':(.53,.59,.63,1),'aluminum':(.74,.79,.85,1),'black':(.025,.034,.044,1),'rubber':(.07,.085,.09,1),
'fr4':(.30,.39,.20,1),'mask':(.045,.30,.15,1),'maskdark':(.03,.22,.14,1),'copper':(.85,.49,.20,1),'gold':(.98,.76,.31,1),'solder':(.73,.78,.81,1),
'ceramic':(.90,.91,.86,1),'whiteink':(.87,.93,.9,1),'resistor':(.20,.16,.13,1),'capacitor':(.65,.58,.35,1),'ic':(.055,.057,.060,1),
'green':(.10,.90,.33,1),'blue':(.08,.52,1.,1),'cyan':(.12,.83,.98,1),'amber':(1.,.49,.085,1),'red':(.91,.11,.10,1),
'glass':(.30,.65,.77,.35),'panel':(.020,.073,.13,1),'holo':(.14,.8,1.,.67),'orange':(1.,.42,.10,1)
}
g.COLORS=COLORS;g.MATERIALS={k:g.pbrmat(k) for k in COLORS}
PCB_MM=(100,70,1.6);DISPLAY_SCALE=14.0;W=PCB_MM[0]*DISPLAY_SCALE/1000; D=PCB_MM[1]*DISPLAY_SCALE/1000
Y=1.257; BOTTOM=Y-.028
P=lambda x,z:(x*.014,Y,z*.014) # mm -> presentation metres
CPS=[
('U1','mcu',0,0,10,10,'MCU_QFN48'),('U2','ldo',-25,10,7,7,'3V3_LDO'),('U3','soic',26,9,7,5,'I2C_SENSOR'),('U4','soic',28,-12,7,5,'CURRENT_MONITOR'),
('Q1','sot',12,-22,4,3,'MOSFET'),('D1','diode',-40,20,5,3,'SCHOTTKY'),('LED1','led',-37,-12,3,2,'LED_GREEN'),('LED2','led',-35,-23,3,2,'LED_BLUE'),('LED3','led',40,-27,3,2,'LED_AMBER'),
('R1','res',-7,22,3,1.7,'4K7'),('R2','res',1,22,3,1.7,'4K7'),('R3','res',9,-18,3,1.7,'100R'),('R4','res',21,-25,3,1.7,'100K'),
('R5','res',-30,-9,3,1.7,'1K'),('R6','res',-27,-19,3,1.7,'330R'),('R7','res',33,-24,3,1.7,'330R'),('R8','res',21,-13,3.8,2.2,'0R1'),
('C1','cap',-33,15,4,2.8,'10u'),('C2','cap',-16,13,4,2.8,'22u'),('C3','cap',-9,-1,3,1.7,'100n'),('C4','cap',27,17,3,1.7,'100n'),
('C5','cap',36,-7,3,1.7,'100n'),('C6','cap',0,-19,3,1.7,'10n'),('C7','cap',13,17,3,1.7,'1u'),
('SW1','switch',-22,-24,6,6,'RESET'),('J1','terminal',-45,4,8,12,'5V_INPUT'),('J2','header',43,21,7,12,'DEBUG_HEADER')]
assert len(CPS)==27
PX={ref:dict(Reference=ref,Kind=kind,Xmm=x,Zmm=z,WidthMM=ww,DepthMM=dd,Value=val,Model=MODEL_NAMES[16+i]) for i,(ref,kind,x,z,ww,dd,val) in enumerate(CPS)}
# Named articulation/placement pivots for Unity. All expected art pivots are exported as FBX transform nodes.
PIVOTS=[]
def piv(mname,pivot,home,axis='y',kind='rotation'):
 PIVOTS.append(dict(Model=mname,Pivot='ANIM_'+pivot,LocalHome=tuple(home),Axis=axis,Kind=kind,Description=pivot.replace('_',' ').title()))
for i in range(27):
 r=CPS[i];piv(MODEL_NAMES[16+i],r[0]+'_COMPONENT_PLACEMENT',P(r[2],r[3]),'y','installation')
for i,an,pt,axis in [
 (3,'LEFT_CLAMP_RELEASE',(-.82,1.26,0),'x'),(4,'RIGHT_CLAMP_RELEASE',(.82,1.26,0),'x'),
 (5,'MICROSCOPE_ARM_RAISE',(.36,2.2,-.48),'y'),(6,'LENS_FOCUS',(.36,2.1,-.48),'y'),
 (15,'W035_JUMPER_REMOVE',(.57,1.35,-.13),'y'),(43,'OSCILLOSCOPE_CONTROL',( -1.7,1.75,.35),'z'),
 (44,'MULTIMETER_DIAL',(1.84,1.65,.08),'z'),(45,'SUPPLY_KNOB',(-1.75,1.40,.42),'z'),
 (46,'LOGIC_ANALYZER_STATUS',(1.7,1.47,.42),'z'),(47,'SCADA_DISPLAY',(0,2.0,-1.23),'z')]:piv(MODEL_NAMES[i],an,pt,axis)
PIVOT_MAP={pv['Pivot']:pv['LocalHome'] for pv in PIVOTS};g.PIVOT_POS=PIVOT_MAP
BY_MODEL=defaultdict(list)
for x in PIVOTS:BY_MODEL[x['Model']].append(x)

# --- original detailed modular art primitives ---
def box(a,p,d,m='frame',pv=None,label='CNCPanel'):a.box(p,d,m,pv,label)
def cyl(a,p,r,h,m='steel',axis='y',pv=None,label='MachinedCylinder',seg=20):a.cylinder(p,r,h,m,axis,seg,pv,label)
def tor(a,p,R,r,m='aluminum',axis='y',pv=None,label='Bezel',seg=32,tube=8):a.torus(p,R,r,m,axis,seg,tube,pv,label)
def beam(a,p,q,r=.013,m='aluminum',pv=None,label='Rail'):a.beam(p,q,r,m,8,pv,label)

def pcbpads(a, bottom=False):
 # actual footprint pad patterns; no preconnected player trace routing.
 z= -1 if bottom else 1
 for r in PX.values():
  x,yy,zz=P(float(r['Xmm']),float(r['Zmm']))
  ww=r['WidthMM']*.014;dd=r['DepthMM']*.014
  k=r['Kind'];ref=r['Reference']
  pads=[]
  if k=='mcu':
   for j in range(12):
    t=(j-5.5)/11
    pads += [(x+t*(ww*.79),zz-dd*.47),(x+t*(ww*.79),zz+dd*.47)]
    pads += [(x-ww*.47,zz+t*dd*.79),(x+ww*.47,zz+t*dd*.79)]
  elif k=='soic':
   for j in range(4):
    pads += [(x-ww*.48,zz+(j-1.5)*dd*.27),(x+ww*.48,zz+(j-1.5)*dd*.27)]
  elif k=='ldo':pads=[(x+(.5-j)*ww*.38,zz-dd*.50) for j in range(3)]+[(x,zz+dd*.5)]
  elif k=='sot':pads=[(x-ww*.34,zz-dd*.46),(x+ww*.34,zz-dd*.46),(x,zz+dd*.48)]
  elif k=='switch':pads=[(x+sx*ww*.4,zz+sz*dd*.4) for sx in (-1,1) for sz in (-1,1)]
  elif k=='terminal':pads=[(x,zz+dd*(j-.5)*.40) for j in range(2)]
  elif k=='header':pads=[(x+sx*ww*.25,zz+(j-2)*dd*.18) for sx in (-1,1) for j in range(5)]
  else:pads=[(x-ww*.48,zz),(x+ww*.48,zz)]
  for j,(xx,zzz) in enumerate(pads):
   if bottom:continue
   sx=.010 if k in ('mcu','soic') else .014
   box(a,(xx,Y+.013,zzz),(sx,.003,.011),'gold',None,f'{ref}_PAD_{j+1:02d}')
 # 10 dedicated probes/test pads, not connected until gameplay if selected
 if not bottom:
  for j,(mx,mz) in enumerate([(-47,-31),(-31,29),(-10,31),(12,31),(40,30),(46,-31),(-45,-30),(0,-31),(30,-30),(-22,30)]):
   x,yy,zz=P(mx,mz);cyl(a,(x,Y+.014,zz),.020,.006,'gold',pv=None,label=f'TP{j+1:02d}_TestPad',seg=14)
 else:
  for j,(mx,mz) in enumerate([(-42,-24),(-20,-12),(0,25),(21,9),(41,-18),(12,-30)]):
   x,_,zz=P(mx,mz);cyl(a,(x,Y-.035,zz),.018,.005,'gold',pv=None,label=f'VIAB{j+1:02d}_BottomTarget',seg=12)

def assembly(a,i):
 if i==0:
  box(a,(0,.78,0),(5.5,.18,3.0),'graphite',label='PrecisionBenchMainChassis')
  for x in (-2.3,2.3):
   for z in (-1.16,1.16):
    box(a,(x,.37,z),(.19,.75,.19),'steel',label='AdjustableStabilizingLeg')
    box(a,(x,.12,z),(.29,.10,.30),'rubber',label='NonSlipFoot')
  for j in range(9):
   box(a,(-2.15+j*.54,.70,1.49),(.48,.034,.043),'amber',label='SafetyWarmBacklight')
   box(a,(-2.15+j*.54,.70,-1.49),(.48,.034,.043),'cyan',label='LevelBlueBacklight')
  box(a,(0,.56,-1.16),(2.3,.24,.38),'frame',label='ServiceControlApron')
  for i in range(11):box(a,(-1.05+i*.20,.60,-1.39),(.095,.04,.05),'gold' if i%3==0 else 'black',label='OperatorServiceButton')
 elif i==1:
  box(a,(0,.9,0),(5.38,.065,2.89),'frame',label='SurfaceAnodizedBorder')
  box(a,(0,.94,0),(5.10,.018,2.65),'rubber',label='AntiStaticWorkSurface')
  for sign in (-1,1):box(a,(0,.951,sign*1.32),(4.7,.014,.022),'cyan',label='WorkstationTrimLight')
 elif i==2:
  box(a,(0,1.045,0),(1.90,.16,1.43),'steel',label='PCBFixureMachinedBase')
  box(a,(0,1.138,0),(1.70,.04,1.24),'black',label='ESDSafePlatform')
  for sign in (-1,1):
   box(a,(sign*.88,1.21,0),(.075,.10,1.20),'aluminum',label='PCBFixtureTrack')
   for k in range(5):cyl(a,(sign*.88,1.278,-.46+k*.23),.020,.018,'gold',label='ThumbScrew')
 elif i in (3,4):
  sign=-1 if i==3 else 1;pv='ANIM_'+('LEFT_CLAMP_RELEASE' if i==3 else 'RIGHT_CLAMP_RELEASE')
  x=sign*.82
  box(a,(x,1.255,0),(.10,.09,1.0),'aluminum',pv,'AdjustablePCBClampingJaw')
  for k in range(3):box(a,(x-sign*.053,1.295,-.33+k*.33),(.024,.02,.21),'rubber',pv,'AntiSlipReplaceableClampPad')
  cyl(a,(x,1.25,.49),.06,.09,'steel','y',pv,'ClampScrewHead')
 elif i==5:
  box(a,(.46,1.05,-1.18),(.43,.18,.39),'frame',label='MicroscopeGantryBase')
  cyl(a,(.46,1.60,-1.18),.07,1.07,'steel',label='PrecisionMicroscopeColumn')
  pv='ANIM_MICROSCOPE_ARM_RAISE'
  beam(a,(.46,2.17,-1.18),(.36,2.17,-.45),.055,'aluminum',pv,'MicroscopeCantileverArm')
  for j in range(5):box(a,(.46,1.23+j*.19,-1.1),(.028,.03,.06),'cyan' if j%2 else 'gold',None,'FocusStageMarker')
 elif i==6:
  pv='ANIM_LENS_FOCUS'
  cyl(a,(.36,2.04,-.48),.15,.19,'black','y',pv,'AutofocusObjectiveHousing')
  cyl(a,(.36,1.96,-.48),.116,.026,'glass','y',pv,'ObjectiveInspectionGlass')
  tor(a,(.36,1.95,-.48),.167,.036,'cyan','y',pv,'CoaxialInspectionRingLight')
  for j in range(12):
   t=j*math.tau/12;cyl(a,(.36+.167*math.cos(t),1.943,-.48+.167*math.sin(t)),.016,.023,'whiteink','y',pv,'IndividualInspectionLED',10)
 elif i==7:
  box(a,(-1.73,1.06,-.86),(.91,.10,.58),'frame',label='AntistaticPartsTray')
  for r in range(3):
   for col in range(4):
    xx=-2.06+col*.21;zz=-1.04+r*.18
    box(a,(xx,1.14,zz),(.18,.08,.14),'black',label='OrganizedComponentPocket')
    for j in range(3):box(a,(xx-.038+j*.038,1.19,zz),(.018,.014,.031),'gold' if j==1 else 'resistor',label='IllustrativeSpareParts')

def board(a,i):
 if i==8:
  box(a,(0,Y-.010,0),(W,.025,D),'fr4',label='PhysicalFR4SubstrateScaledForBoardPresentation')
  # slight chamfers as corner overlays
  for xx in (-W/2+.055,W/2-.055):
   for zz in (-D/2+.055,D/2-.055):
    cyl(a,(xx,Y+.018,zz),.037,.015,'gold',label='PlatedMountingHoleWasher')
    cyl(a,(xx,Y+.025,zz),.020,.018,'black',label='MechanicalClearanceHoleInsert')
 elif i==9:
  box(a,(0,Y+.007,0),(W-.015,.008,D-.015),'mask',label='GreenTopSolderMaskCoating')
  for j in range(5):box(a,(0,Y+.013,-.40+j*.2),(W*.95,.002,.008),'maskdark',label='TopMaskManufacturingPattern')
 elif i==10:
  box(a,(0,Y-.027,0),(W-.015,.006,D-.015),'maskdark',label='BottomSolderMask')
 elif i==11:pcbpads(a,False)
 elif i==12:pcbpads(a,True)
 elif i==13:
  for x in (-.64,.64):
   box(a,(x,Y+.017,0),(.006,.002,.88),'whiteink',label='BoardOutlineLegend')
  for z in (-.44,.44):box(a,(0,Y+.017,z),(1.25,.002,.006),'whiteink',label='SilkscreenLegendBorder')
  # Fiducials in 4 corners and IC orientation mark
  for x,z in [(-.54,.37),(.53,.37),(-.55,-.36),(.53,-.36)]:
   cyl(a,(x,Y+.021,z),.013,.004,'whiteink',label='OpticalPlacementFiducial')
   cyl(a,(x,Y+.024,z),.007,.004,'gold',label='FiducialCore')
  cyl(a,(-.11,Y+.020,-.11),.009,.004,'whiteink',label='U1_OrientationDotSilkscreen')
 elif i==14:
  for x in (-W/2+.04,W/2-.04):
   box(a,(x,Y-.010,0),(.013,.027,D-.12),'copper',label='EdgePlatedCopper')
  for z in (-D/2+.04,D/2-.04):box(a,(0,Y-.010,z),(W-.12,.027,.011),'copper',label='EdgePlatedMountingKeepout')
 elif i==15:
  pv='ANIM_W035_JUMPER_REMOVE'
  # removable jumper with two posts and arch (separate non-connected mesh); actual net connection via gameplay graph
  x,y,z=(.57,1.35,-.13)
  for dx in (-.045,.045):
   cyl(a,(x+dx,y,z),.018,.16,'gold','y',pv,'ProtectedJumperPin')
   box(a,(x+dx,y+.065,z),(.041,.057,.045),'black',pv,'JumperInsulator')
  box(a,(x,y+.12,z),(.15,.070,.082),'red',pv,'W035DetachableRedServiceShunt')
  box(a,(x,y+.16,z),(.07,.015,.073),'whiteink',pv,'CriticalFeedbackKeyingMark')

def make_smd(a,r):
 ref=r['Reference'];kind=r['Kind'];x,yy,z=P(float(r['Xmm']),float(r['Zmm'])); ww=r['WidthMM']*.014; dd=r['DepthMM']*.014
 pv='ANIM_'+ref+'_COMPONENT_PLACEMENT'
 # shaped body slightly above pad plane
 base_y=Y+.044
 if kind=='mcu':
  box(a,(x,base_y,z),(ww,.065,dd),'ic',pv,'QFN48_OvermoldBody')
  box(a,(x,base_y+.035,z),(ww*.81,.006,dd*.81),'graphite',pv,'LaserMarkedSiliconTop')
  for j in range(12):
   t=(j-5.5)/11
   for sx in (-1,1):box(a,(x+sx*ww*.49,base_y-.025,z+t*dd*.79),(.011,.010,.011),'solder',pv,'QFNPerimeterExposedCopper')
   for sz in (-1,1):box(a,(x+t*ww*.79,base_y-.025,z+sz*dd*.49),(.011,.010,.011),'solder',pv,'QFNPerimeterExposedCopper')
  cyl(a,(x-ww*.32,base_y+.041,z-dd*.30),.010,.006,'whiteink','y',pv,'PinOneMarker')
 elif kind in ('soic','ldo'):
  box(a,(x,base_y+.014,z),(ww*.70,.060,dd*.80),'ic',pv,'IndustrialIntegratedCircuit')
  n=4 if kind=='soic' else 3
  for j in range(n):
   v=(j-(n-1)/2)/max(n,3)
   for sx in (-1,1):box(a,(x+sx*ww*.48,base_y-.018,z+v*dd*.86),(.024,.016,.012),'solder',pv,'GullWingICLead')
  cyl(a,(x-ww*.22,base_y+.051,z-dd*.23),.010,.004,'whiteink','y',pv,'PackagePinOneDot')
 elif kind=='sot':
  box(a,(x,base_y,z),(ww*.75,.035,dd*.75),'ic',pv,'SOT23_MOSFETBody')
  for dx,dz in [(-.3,-.4),(.3,-.4),(0,.42)]:box(a,(x+dx*ww,base_y-.019,z+dz*dd),(.012,.014,.016),'solder',pv,'MOSFETContactLeg')
 elif kind=='diode':
  box(a,(x,base_y,z),(ww*.83,.044,dd*.65),'black',pv,'SMAReversePolarityDiode')
  for dx in (-1,1):box(a,(x+dx*ww*.45,base_y-.02,z),(.019,.019,dd*.68),'solder',pv,'DiodeSolderTerminations')
  box(a,(x-ww*.20,base_y+.025,z),(.009,.005,dd*.65),'whiteink',pv,'CathodeBand')
 elif kind=='led':
  clr={'LED1':'green','LED2':'blue','LED3':'amber'}[ref]
  box(a,(x,base_y,z),(ww*.75,.034,dd*.85),'ceramic',pv,'LEDWhitePackage')
  box(a,(x,base_y+.023,z),(ww*.39,.017,dd*.43),clr,pv,'LEDDieEmitter')
  for dx in (-1,1):box(a,(x+dx*ww*.44,base_y-.012,z),(.011,.012,dd*.8),'solder',pv,'LEDTerminal')
 elif kind in ('res','cap'):
  bodymat='resistor' if kind=='res' else 'capacitor'
  box(a,(x,base_y,z),(ww*.75,.026,dd*.85),bodymat,pv,'PrecisionSMD'+('Resistor' if kind=='res' else 'Capacitor'))
  for dx in (-1,1):box(a,(x+dx*ww*.39,base_y,z),(ww*.20,.029,dd*.88),'solder',pv,'GoldPlatedSMDTermination')
  if kind=='res':box(a,(x,base_y+.014,z),(ww*.35,.004,dd*.30),'whiteink',pv,'LaserValueMark')
 elif kind=='switch':
  box(a,(x,base_y,z),(ww*.86,.055,dd*.86),'steel',pv,'SealedTactileSwitchBase')
  cyl(a,(x,base_y+.047,z),ww*.29,.055,'black','y',pv,'ResetSwitchPlunger')
  for sx in (-1,1):
   for sz in (-1,1):box(a,(x+sx*ww*.41,base_y-.025,z+sz*dd*.41),(.022,.014,.027),'gold',pv,'SwitchContactFoot')
 elif kind=='terminal':
  box(a,(x,base_y+.10,z),(ww*.85,.26,dd*.96),'green',pv,'TwoPole5VTerminalBlock')
  for j in (-1,1):
   cyl(a,(x,base_y+.245,z+j*dd*.22),.031,.022,'solder','y',pv,'InsulatedClampScrew')
   cyl(a,(x-ww*.37,base_y+.095,z+j*dd*.22),.026,.10,'black','x',pv,'WireEntryPort')
 elif kind=='header':
  box(a,(x,base_y+.052,z),(ww*.74,.084,dd*.87),'black',pv,'ShroudedDebugHeader')
  for j in range(5):
   for sx in (-1,1):
    box(a,(x+sx*ww*.21,base_y+.119,z+(j-2)*dd*.16),(.014,.105,.014),'gold',pv,'GoldDebugHeaderPin')

def instrument(a,i):
 if i==43:pos=(-1.7,1.59,.25);width=.89
 elif i==44:pos=(1.81,1.55,.24);width=.53
 elif i==45:pos=(-1.81,1.25,.80);width=.73
 elif i==46:pos=(1.78,1.19,.80);width=.80
 else:pos=(0,2.03,-1.23);width=1.60
 x,y,z=pos
 if i==47:
  box(a,(x,y,z),(width,.77,.10),'frame',label='HighResolutionSCADAWallModule')
  box(a,(x,y,z+.063),(width*.92,.69,.020),'panel',label='LargeSCADAGlass')
  for row in range(6):
   yy=y-.25+row*.11
   box(a,(x-.38,yy,z+.083),(.38,.018,.005),'cyan',label='OscilloscopeWaveformOverlay')
   box(a,(x+.47,yy,z+.083),(.08,.027,.005),'green' if row%2 else 'amber',label='QualificationStatusOverlay')
 else:
  box(a,(x,y,z),(width,.49,.56),'graphite',label='IndustrialInstrumentCase')
  box(a,(x,y+.067,z+.286),(width*.86,.35,.015),'panel',label='ColorInstrumentReadout')
  for j in range(6):
   yy=y-.07+(j%3)*.09
   xx=x-width*.26+(j//3)*width*.49
   box(a,(xx,yy,z+.3),(width*.22,.011,.005),'cyan' if j%2 else 'green',label='MeasurementWaveform')
  for j in range(4):
   xx=x-width*.35+j*width*.22
   cyl(a,(xx,y-.185,z+.3),.023,.030,'gold','z',label='GoldFrontPanelControl',seg=12)
  if i==43:
   for j in range(4):cyl(a,(x-.29+j*.18,y-.14,z+.315),.025,.016,['blue','amber','green','red'][j],'z',label='ChannelProbeJack')
  elif i==44:
   cyl(a,(x,y-.15,z+.31),.075,.017,'steel','z',pv='ANIM_MULTIMETER_DIAL',label='RotaryMultimeterRangeDial')
  elif i==45:
   for j in range(2):cyl(a,(x+.23-j*.21,y-.18,z+.31),.031,.023,'steel','z',pv='ANIM_SUPPLY_KNOB',label='DCProtectionSetPointKnob')
  elif i==46:
   for j in range(8):box(a,(x+(j-3.5)*.075,y-.20,z+.305),(.037,.046,.035),'gold' if j%2 else 'black',label='LogicAnalyzerHeader')

def build():
 out=[];total_tri=0;full=[];allroots=[];allhomes=[];all_sockets=[];stats=[]
 for i,name in enumerate(MODEL_NAMES):
  a=g.ModelArt(name)
  if i<8:assembly(a,i)
  elif i<16:board(a,i)
  elif i<43:make_smd(a,PX[CPS[i-16][0]])
  else:instrument(a,i)
  cs=g.chunk_model(a,max_shapes=24)
  pv=[(x['Pivot'],name,x['LocalHome']) for x in BY_MODEL[name]]
  fbx=ROOT/'Models'/f'{i:02d}_{name}.fbx';glb=ROOT/'GLB'/f'{i:02d}_{name}.glb'
  ef=g.export_fbx(fbx,[(name,t) for t in cs],[name],pv,[])
  eg=g.export_glb(glb,[(name,t) for t in cs],[name],pv)
  tri=sum(len(t['faces']) for t in cs);total_tri+=tri
  allroots.append(name);full += [(name,t) for t in cs];allhomes+=pv
  out.append({'Index':i,'Model':name,'Group':CATEGORIES[i],'FBX':str(fbx.relative_to(ROOT)),'GLB':str(glb.relative_to(ROOT)),'MeshParts':len(cs),'Triangles':tri,'FBXBytes':ef['file_size'],'GLBBytes':eg['glb_bytes'],'SceneScale':DISPLAY_SCALE,'PhysicalBoardMM':f'{PCB_MM[0]} x {PCB_MM[1]}','GameplayReady':'NO, visual only'})
  print(f'GEOM {i+1:02d}/48 {name} meshparts={len(cs)} triangles={tri}',flush=True)
 f=g.export_fbx(ROOT/'Models'/'L94_BoardFoundry_CompleteShowcase.fbx',full,allroots,allhomes,[])
 gl=g.export_glb(ROOT/'GLB'/'L94_BoardFoundry_CompleteShowcase.glb',full,allroots,allhomes)
 g.write_csv(ROOT/'Data'/'Model_Manifest_48.csv',out)
 g.write_csv(ROOT/'Data'/'Component_Placement_27.csv',list(PX.values()))
 g.write_csv(ROOT/'Data'/'Animation_Pivots.csv',PIVOTS)
 info={'level':94,'name':'The Board Foundry','model_count':48,'component_count':27,'art_scale_to_physical':DISPLAY_SCALE,'physical_board_mm':PCB_MM,'presentation_board_metres':[W,D],'triangles':total_tri,'fbx_export_version':7400,'assembled_fbx_bytes':f['file_size'],'assembled_glb_bytes':gl['glb_bytes'],'pivots':len(PIVOTS),'art_only':True}
 (ROOT/'Reports'/'Geometry_Build_Report.json').write_text(json.dumps(info,indent=2))
 print('COMPLETE',json.dumps(info),flush=True)
if __name__=='__main__':build()
