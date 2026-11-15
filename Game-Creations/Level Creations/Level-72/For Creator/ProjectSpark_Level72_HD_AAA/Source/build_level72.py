#!/usr/bin/env python3
# Project Spark / Level 72 / 52 modular ALD fabrication equipment.
# Original procedural 3D geometry; this program contains all geometry & FBX exporters.
import os,math,csv,json,time,struct,zlib,zipfile,random
from pathlib import Path
import numpy as np, trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level72_HD_AAA')
MD=ROOT/'Models';GD=ROOT/'GLB';PD=ROOT/'Previews';ED=ROOT/'Unity_Optional';SRC=ROOT/'Source'
for d in (MD,GD,PD,ED,SRC):d.mkdir(parents=True,exist_ok=True)
names=['IndustrialAtomicDepositionBase', 'TransparentALDReactorChamber', 'ReactorServiceLid', 'QuartzObservationViewport', 'MultiZoneHeatedWaferChuck', 'PrecisionSiliconWafer', 'AtomicThinFilmVisualization', 'HighAspectRatioTrenchSample', 'WaferLiftPinAssembly', 'WaferCarrierCassette', 'TransparentVacuumLoadLock', 'LoadLockGateValve', 'AutomatedWaferTransferRobot', 'VirtualPrecursorSupplyA', 'VirtualPrecursorSupplyB', 'InertPurgeGasSupply', 'HighSpeedPulseValveA', 'HighSpeedPulseValveB', 'HighSpeedPurgeValve', 'MassFlowControllerA', 'MassFlowControllerB', 'PurgeMassFlowTransmitter', 'SequentialGasDistributionManifold', 'PrecisionDepositionShowerhead', 'HeatedGasDeliveryPipeline', 'TurbomolecularVacuumPump', 'VacuumBackingPump', 'AutomaticVacuumThrottleValve', 'VacuumIsolationValve', 'IndustrialVacuumForeline', 'CapacitancePressureManometer', 'RoughVacuumPressureGauge', 'HeatedDeliveryLineSensor', 'WaferChuckTemperatureSensor', 'MultiZoneHeaterPowerBank', 'QuartzCrystalMicrobalance', 'InSituOpticalEllipsometer', 'OpticalThicknessViewport', 'ConformalFilmMetrologyScanner', 'SurfaceCoverageMonitor', 'ResidualVaporAnalyzer', 'ReactantOverlapMonitor', 'GrowthPerCycleAnalyzer', 'FourStageALDCycleSequencer', 'AtomicDepositionPLCRack', 'WaferTemperaturePIDController', 'VacuumPressurePIDController', 'PurgeInterlockProtectionRelay', 'VirtualExhaustTreatmentModule', 'EmergencyDepositionSafetyStation', 'HolographicAtomicDepositionSCADA', 'Regulated5VControlSupply']
P={'navy':(14,32,52,255),'steel':(66,101,126,255),'light':(139,177,195,255),'silver':(181,199,208,255),'dark':(27,50,69,255),'black':(13,21,30,255),'cyan':(37,211,246,255),'blue':(20,112,218,255),'ice':(133,224,245,255),'copper':(220,120,52,255),'amber':(248,162,45,255),'red':(226,63,70,255),'green':(47,219,139,255),'white':(220,238,248,255),'glass':(93,178,212,135),'frost':(205,235,247,255),'rubber':(34,38,42,255),'gold':(224,176,68,255)}
P.update({'violet':(172,81,244,255),'plasma':(186,73,247,180),'purpleglass':(138,77,211,128),'teal':(66,213,199,255),'ceramic':(230,225,220,255),'iridescent':(106,200,242,255),'warning':(255,105,71,255)})
M={}
def addmesh(k,m,c):
 if m is None or len(m.faces)==0:return
 if k not in M:M[k]=[]
 m.visual.vertex_colors=np.tile(P[c],(len(m.vertices),1))
 M[k].append((m,c))
def box(k,xyz,ext,c,rot=None):
 m=trimesh.creation.box(extents=ext)
 if rot is not None:m.apply_transform(rot)
 m.apply_translation(xyz);addmesh(k,m,c)
def cyl(k,center,r,length,c,axis='z',sec=16):
 m=trimesh.creation.cylinder(radius=r,height=length,sections=sec)
 if axis=='x':m.apply_transform(trimesh.transformations.rotation_matrix(math.pi/2,[0,1,0]))
 if axis=='y':m.apply_transform(trimesh.transformations.rotation_matrix(-math.pi/2,[1,0,0]))
 m.apply_translation(center);addmesh(k,m,c)
def pipe(k,pts,r=.045,c='copper',sec=10):
 for p,q in zip(pts[:-1],pts[1:]):
  p=np.asarray(p,float);q=np.asarray(q,float);v=q-p;L=np.linalg.norm(v)
  if L<1e-5:continue
  m=trimesh.creation.cylinder(radius=r,height=L,sections=sec);tf=trimesh.geometry.align_vectors([0,0,1],v/L);m.apply_transform(tf);m.apply_translation((p+q)/2);addmesh(k,m,c)
 for p in pts[1:-1]:
  m=trimesh.creation.icosphere(subdivisions=1,radius=r);m.apply_translation(p);addmesh(k,m,c)
def ring(k,center,r,inner,depth,axis='z',c='silver',sec=20):
 # ring as thin tube / annulus mesh
 verts=[];faces=[]
 for z in (-depth/2,depth/2):
  for rad in (r,inner):
   for i in range(sec):
    a=2*math.pi*i/sec;verts.append((rad*math.cos(a),rad*math.sin(a),z))
 for layer in range(4):
  for i in range(sec):
   j=(i+1)%sec; layer2={0:2,1:3,2:3,3:0}[layer]
   a=layer*sec+i;b=layer*sec+j;d=layer2*sec+i;e=layer2*sec+j
   faces.extend(((a,b,d),(b,e,d)))
 m=trimesh.Trimesh(vertices=verts,faces=faces,process=False)
 if axis=='x':m.apply_transform(trimesh.transformations.rotation_matrix(math.pi/2,[0,1,0]))
 if axis=='y':m.apply_transform(trimesh.transformations.rotation_matrix(-math.pi/2,[1,0,0]))
 m.apply_translation(center);addmesh(k,m,c)
def screen(k,xyz,w=.58,h=.32,c='blue'):
 box(k,xyz,(w,h,.065),'black');box(k,(xyz[0],xyz[1],xyz[2]+.039),(w*.88,h*.80,.01),c)
 for i in range(5): box(k,(xyz[0]-w*.32+i*w*.16,xyz[1]-h*.20+((i*7)%4)*h*.11,xyz[2]+.05),(.035,.017,.012),'cyan')
def knob(k,x,y,z,c='cyan'):
 cyl(k,(x,y,z),.045,.045,'silver');cyl(k,(x,y,z+.03),.025,.025,c)
def panel(k,center,ex=(.70,.60,.45),label='generic'):
 x,y,z=center;box(k,center,ex,'steel');box(k,(x,y,z+ex[2]/2+.01),(.84*ex[0],.77*ex[1],.015),'dark');screen(k,(x,y+.09,z+ex[2]/2+.05),ex[0]*.60,ex[1]*.34)
 for j in range(3):knob(k,x-.22+j*.21,y-ex[1]*.26,z+ex[2]/2+.035,('green','amber','red')[j])
 for j in range(5):box(k,(x-ex[0]*.36+j*ex[0]*.18,y+ex[1]*.34,z),(.05,.09,.07),'silver')
def fan(k,center,r=.38,axis='z'):
 x,y,z=center
 cyl(k,center,r*.75,.12,'dark',axis,24);ring(k,center,r,r*.88,.10,axis,'silver',24)
 # radial blades in plane XY for z; xz for y
 for i in range(7):
  a=i*2*math.pi/7
  if axis=='z':p=(x+.46*r*math.cos(a),y+.46*r*math.sin(a),z+.08);ang=a
  else:p=(x+.46*r*math.cos(a),y,z+.46*r*math.sin(a));ang=a
  m=trimesh.creation.box(extents=(r*.61,r*.16,.035))
  if axis=='z':m.apply_transform(trimesh.transformations.rotation_matrix(ang,[0,0,1]))
  else:m.apply_transform(trimesh.transformations.rotation_matrix(math.pi/2,[1,0,0]));m.apply_transform(trimesh.transformations.rotation_matrix(ang,[0,1,0]))
  m.apply_translation(p);addmesh(k,m,'light')
 cyl(k,center,r*.14,.18,'steel',axis)
def motor(k,xyz,scale=1):
 x,y,z=xyz
 cyl(k,(x,y,z),.46*scale,1.3*scale,'blue','x',24)
 for t in np.linspace(-.53,.53,11):ring(k,(x+t*scale,y,z),.48*scale,.455*scale,.045*scale,'x','steel',20)
 cyl(k,(x+.77*scale,y,z),.105*scale,.35*scale,'silver','x');cyl(k,(x-.76*scale,y,z),.38*scale,.09*scale,'dark','x')
 box(k,(x,y-.46*scale,z),(1.25*scale,.18*scale,.66*scale),'steel')
 box(k,(x,y+.50*scale,z),(.46*scale,.19*scale,.44*scale),'dark')
def compressor(k):
 x,y,z=(.25,1.27,.55)
 box(k,(x,y-.56,z),(3.0,.20,1.60),'steel')
 motor(k,(-.55,y,z),1.0)
 cyl(k,(1.10,y,z),.59,1.30,'blue','x',24)
 for t in np.linspace(.58,1.56,8):ring(k,(t,y,z),.61,.58,.075,'x','steel',24)
 cyl(k,(1.82,y,z),.40,.25,'dark','x')
 cyl(k,(1.2,y+.67,z),.20,.9,'silver','y')
 pipe(k,[(1.2,y+.74,z),(1.2,2.36,z),(.4,2.36,z)],.085,'copper')
 for i in range(5):box(k,(1.18+i*.09,1.26,z+.63),(.038,.17,.035),'amber')
def cooling_chamber(k,cx,cy,cz= -1.5,hot=False):
 # roomy 2.5x2x1.7 internal, glass front toward +z
 x,y,z=cx,cy,cz
 box(k,(x,y-.95,z),(2.65,.12,2.0),'steel')
 for dx in (-1.28,1.28):
  for dz in (-.96,.96):box(k,(x+dx,y,z+dz),(.09,1.95,.08),'silver')
 for yy in (-.89,.93):
  for dz in (-.96,.96):box(k,(x,y+yy,z+dz),(2.55,.07,.08),'silver')
 for yy in (-.89,.93):
  for dx in (-1.28,1.28):box(k,(x+dx,y+yy,z),(.08,.07,1.9),'silver')
 box(k,(x,y,z-.94),(2.4,1.74,.028),'ice')
 box(k,(x,y,z+.96),(2.45,1.7,.018),'glass')
 box(k,(x,y+.97,z),(2.68,.1,2.04),'steel')
 for xx in (-.76,.76):fan(k,(x+xx,y+.48,z-.76),.34)
 for i in range(3):
  box(k,(x,y-.70+i*.51,z-.20),(2.0,.046,.80),'silver')
  for j in range(4):box(k,(x-.75+j*.50,y-.54+i*.51,z-.15),(.33,.24,.32),'white' if not hot else 'ice')
 for i in range(2):box(k,(x-1.16+i*2.32,y,z+.99),(.07,1.72,.04),'steel')
 for i in range(5):box(k,(x-1.1+i*.54,y-.82,z+1.04),(.35,.035,.035),'cyan')

# LEVEL 70 custom modular cleanroom machinery: authored physical assembly in meters.
# The same scene-space coordinates are used for every individual model.

def rbox(k,c,e,col='steel',radius=.04):
    x,y,z=c; X,Y,Z=e
    box(k,c,e,col)
    # thin silver corner/edge strips give each cabinet mechanical definition
    for yy in (-Y/2+.035,Y/2-.035):
        for zz in (-Z/2+.035,Z/2-.035): box(k,(x,y+yy,z+zz),(X+.018,.025,.025),'silver')
    for xx in (-X/2+.035,X/2-.035):
        for zz in (-Z/2+.035,Z/2-.035): box(k,(x+xx,y,z+zz),(.025,Y+.018,.025),'silver')

def frame(k,center,ext,beam=.075,glass=True,glass_front=True):
    x,y,z=center;dx,dy,dz=ext
    for ax in (-1,1):
        for az in (-1,1):box(k,(x+ax*(dx-beam)/2,y,z+az*(dz-beam)/2),(beam,dy,beam),'silver')
    for ay in (-1,1):
        for az in (-1,1):box(k,(x,y+ay*(dy-beam)/2,z+az*(dz-beam)/2),(dx,beam,beam),'silver')
        for ax in (-1,1):box(k,(x+ax*(dx-beam)/2,y+ay*(dy-beam)/2,z),(beam,beam,dz),'silver')
    if glass:
        box(k,(x,y,z-dz/2+.04),(dx-.15,dy-.15,.021),'glass')
        if glass_front:box(k,(x,y,z+dz/2-.04),(dx-.15,dy-.15,.021),'glass')
        for ax in (-1,1):box(k,(x+ax*(dx/2-.055),y,z),(.021,dy-.15,dz-.15),'glass')

def grille(k,center,ext,axis='z',n=18):
    x,y,z=center;w,h,d=ext
    box(k,center,ext,'dark')
    for i in range(n):
        yy=y-h*.44+(i+.5)*h*.88/n
        box(k,(x,yy,z+d/2+.014),(w*.91,h*.45/n,.028),'silver')
    for xx in (x-w*.47,x+w*.47):box(k,(xx,y,z+d/2+.028),(.035,h,.035),'light')

def duct(k,pts,r=.13,cc='steel'):
    pipe(k,pts,r,cc,16)
    # couplings/flanges at endpoints
    for p in [pts[0],pts[-1]]:cyl(k,p,r*1.32,r*.23,'silver',axis='y',sec=18)

def compact_screen(k,center,w=.62,h=.37,c='blue'):
    box(k,center,(w,h,.075),'dark')
    box(k,(center[0],center[1],center[2]+.042),(w*.88,h*.8,.012),c)
    for j in range(5):box(k,(center[0]-w*.33+j*w*.17,center[1]-.08+((j*3)%5)*.035,center[2]+.055),(w*.07,.015,.012),'cyan')

def multisensor(k,x,y,z,kind='pressure',size=.26):
    rbox(k,(x,y,z),(size*1.8,size*1.3,size*.64),'steel')
    compact_screen(k,(x,y,z+size*.36),size*1.35,size*.75,'cyan' if kind!='particle' else 'green')
    for j,c in enumerate(('green','amber','red')):knob(k,x-size*.55+j*size*.55,y-size*.45,z+size*.38,c)
    cyl(k,(x,y-size*.74,z),size*.16,size*.46,'silver','y')
    duct(k,[(x-.12,y-size*.80,z),(x-.22,y-size*1.2,z)],.016,'copper')

def filter_pleats(k,x,y,z,w,h,depth=.16,n=29,filtercol='white'):
    box(k,(x,y,z),(w+.1,h+.1,depth+.06),'steel')
    box(k,(x,y,z+depth*.51),(w,h,.036),'dark')
    for i in range(n):
        px=x-w*.485+(i+.5)*(w*.97/n)
        box(k,(px,y,z+depth*.56+.026),(w*.66/n,h*.89,.028),filtercol if i%4 else 'light')
    for xx in (x-w*.49,x+w*.49):box(k,(xx,y,z+depth*.60+.04),(.025,h,.038),'silver')

def coil(k,x,y,z,w,h,heat=False):
    # dense fin pack + sinuous metal capillaries + perimeter frame
    box(k,(x,y,z),(w+.10,h+.13,.16),'dark')
    for i in range(43):
        px=x-w*.48+i*w*.96/42
        box(k,(px,y,z+.095),(.017,h*.9,.055),'copper' if heat and i%5==0 else 'light')
    for j in range(8):
        yy=y-h*.43+j*h*.86/7
        pipe(k,[(x-w*.52,yy,z+.16),(x+w*.52,yy,z+.16)],.023,'copper' if heat else 'blue',12)
    for px in (x-w*.55,x+w*.55):box(k,(px,y,z+.10),(.063,h+.14,.083),'steel')
    for py in (y-h*.55,y+h*.55):box(k,(x,py,z+.10),(w+.13,.052,.084),'steel')

def airfan(k,center,r=.43,axis='z',c='blue'):
    x,y,z=center
    fan(k,center,r,axis)
    for ax in (-1,1):
        for ay in (-1,1):box(k,(x+ax*r*.83,y+ay*r*.83,z),(.055,.055,.13),'silver')
    # protective fan grille with straight wire bars
    for n in range(-4,5):
        if axis=='z':box(k,(x+n*r/5,y,z+.09),(.013,1.79*r,.018),'light')

def concrete(k,center,e):
    box(k,center,e,'steel')
    for i in range(5):box(k,(center[0]-e[0]/2+.2+i*(e[0]-.4)/4,center[1]+e[1]/2+.014,center[2]),(.075,.028,e[2]*.90),'light')

# --------------------------------------------------------------
# ==============================================================
# PROJECT SPARK LEVEL 71 — ETCH THE FUTURE
# Original, modular, scene-aligned 3-D semiconductor equipment.
# Full geometry in meters; FBX exporter serializes centimeter coords.
# ==============================================================

def mechbox(k,p,ext,col='steel'):
    box(k,p,ext,col)
    x,y,z=p;X,Y,Z=ext
    for xx in (x-X*.48,x+X*.48):
        for zz in (z-Z*.45,z+Z*.45):box(k,(xx,y+Y*.49,zz),(.032,.021,.034),'light')
    for a in (-1,1):box(k,(x+a*X*.47,y,z+Z*.506),(.021,Y*.86,.024),'silver')

def controlbox(k,p,ext=(.72,.56,.50),color='steel',monitor='violet',knobs=4):
    mechbox(k,p,ext,color)
    x,y,z=p;w,h,d=ext
    box(k,(x,y+.10*h,z+.515*d),(.83*w,.52*h,.035),'black')
    box(k,(x,y+.10*h,z+.536*d),(.74*w,.44*h,.01),monitor)
    for i in range(8):box(k,(x-.30*w+i*.085*w,y+.03*h+((i*3)%5)*.054*h,z+.556*d),(.048*w,.02*h,.015),'cyan')
    for i in range(knobs):knob(k,x-.33*w+i*.66*w/max(1,knobs-1),y-.29*h,z+.54*d,['green','amber','red','cyan'][i%4])
    for j in range(3):box(k,(x-w*.38+j*.25*w,y+h*.515,z),(.065,.055,.08),'silver')

def vacflange(k,center,r,depth=.11,axis='x',count=12):
    ring(k,center,r,r*.78,depth,axis,'silver',32)
    x,y,z=center
    for i in range(count):
        a=2*math.pi*i/count
        if axis=='x':v=(x,y+r*.89*math.cos(a),z+r*.89*math.sin(a))
        elif axis=='y':v=(x+r*.89*math.cos(a),y,z+r*.89*math.sin(a))
        else:v=(x+r*.89*math.cos(a),y+r*.89*math.sin(a),z)
        cyl(k,v,.036,.035,'dark',axis,8)

def precision_ring(k,c,outer,inner,depth,axis='y',color='silver'):
    ring(k,c,outer,inner,depth,axis,color,32)
    ring(k,c,outer*.90,outer*.86,depth*1.04,axis,'dark',32)

def waferdisc(k,center,r=.58,thick=.035,color='iridescent',grids=True):
    x,y,z=center
    cyl(k,center,r,thick,color,'y',48)
    ring(k,(x,y+thick*.51,z),r*.995,r*.93,.009,'y','silver',48)
    if grids:
        for i in range(-7,8):
            u=i*r*.117
            span=math.sqrt(max(0,r*r-u*u))*.92
            box(k,(x+u,y+thick*.56,z),(min(.010,r*.025),.006,2*span),'cyan')
            box(k,(x,y+thick*.56,z+u),(2*span,.006,min(.010,r*.025)),'cyan')

def sscylinder(k,c,r,ln,axis='y',num=12):
    cyl(k,c,r,ln,'steel',axis,32)
    a={'x':0,'y':1,'z':2}[axis]
    for i in range(num):
        d=(i/(num-1)-.5)*ln*.86
        q=list(c);q[a]+=d
        ring(k,q,r*1.018,r*.98,.038,axis,'light',24)

def wirebundle(k,pts,num=3,sep=.025,col='cyan',r=.009):
    for i in range(num):
        shift=(i-(num-1)/2)*sep
        path=[(p[0]+shift,p[1],p[2]+shift*.5) for p in pts]
        pipe(k,path,r,col,8)

def giantdisplay(k,p,width=3.6,height=1.72):
    x,y,z=p
    box(k,(x,y,z),(width,height,.15),'navy')
    box(k,(x,y,z+.082),(width*.96,height*.90,.014),'black')
    box(k,(x-width*.27,y+.18,z+.100),(width*.37,height*.53,.012),'violet')
    box(k,(x+width*.22,y+.23,z+.100),(width*.34,height*.43,.012),'blue')
    for j in range(4):
        for i in range(12):
            yy=y-height*.28+j*.12+0.022*math.sin(i*.7+j)
            xx=x-width*.44+i*width*.079
            box(k,(xx,yy,z+.112),(.045,.013,.009),'cyan' if j%2 else 'teal')
    for xx in (-width*.46,width*.46):box(k,(x+xx,y,z+.11),(.025,height*.88,.022),'cyan')
    box(k,(x,y-height*.44,z+.11),(width*.89,.025,.022),'cyan')
    box(k,(x,y-height*.58,z-.09),(.12,.43,.18),'silver')

# ================================================================
# PROJECT SPARK LEVEL 72 — LAYER BY LAYER
# Bespoke 52-piece semiconductor ALD visual equipment set.
# Authoring units: metres.  All meshes are scene-aligned.
# ================================================================

def station(k,c,ext=(.95,.75,.66),accent='cyan',knobs=3):
    controlbox(k,c,ext,'steel',accent,knobs)
    x,y,z=c;w,h,d=ext
    for j in range(5):
        box(k,(x-w*.37+j*w*.18,y+h*.515,z-d*.13),(.054,.075,.083),'light')
    for j in range(5):
        box(k,(x-w*.4+j*w*.2,y-h*.36,z+d*.516),(.085,.033,.035),accent if j%2==0 else 'silver')

def cylvessel(k,x,y,z,r=.40,h=1.6,accent='cyan'):
    cyl(k,(x,y,z),r,h,'steel','y',40)
    cyl(k,(x,y,z),r*.935,h*.91,'light','y',36)
    for dy in (-.43,-.18,.23,.44):
        precision_ring(k,(x,y+h*dy,z),r*1.045,r*.97,.055,'y','steel')
    box(k,(x,y+.13,z+r+.018),(r*.98,h*.25,.033),accent)
    cyl(k,(x,y+h*.52,z),r*.53,.10,'steel','y',30)
    cyl(k,(x,y-h*.52,z),r*.71,.13,'dark','y',30)
    cyl(k,(x,y+h*.59,z),.08,.17,'copper','y',14)
    for dx in (-r*.55,r*.55):
        box(k,(x+dx,y-h*.55,z),(r*.19,.14,r*.57),'dark')
    for i in range(7):cyl(k,(x-r*.28+i*r*.095,y+h*.07,z+r+.049),.012,.014,'cyan','z',8)

def pulsevalve(k,pos,cc='cyan'):
    x,y,z=pos
    mechbox(k,(x,y,z),(.50,.48,.43),'steel')
    for xx in (x-.33,x+.33):vacflange(k,(xx,y,z),.19,.045,'x',8)
    cyl(k,(x,y+.39,z),.18,.30,'dark','y',20)
    cyl(k,(x,y+.59,z),.115,.13,cc,'y',20)
    for a in np.linspace(0,2*math.pi,10,endpoint=False):
        box(k,(x+.15*math.cos(a),y+.45,z+.15*math.sin(a)),(.035,.12,.042),'silver')
    cyl(k,(x,y-.34,z),.075,.21,'copper','y',12)
    box(k,(x,y+.02,z+.24),(.30,.12,.03),'dark')
    for j in range(3):cyl(k,(x-.09+j*.09,y+.02,z+.26),.019,.018,cc if j==1 else 'green','z',8)

def sensor(k,p,acc='cyan',mini=False):
    x,y,z=p
    factor=.72 if mini else 1
    station(k,p,(.62*factor,.62*factor,.40*factor),acc,2)
    cyl(k,(x,y-.42*factor,z),.07*factor,.27*factor,'silver','y',16)
    pipe(k,[(x,y-.55*factor,z),(x-.20*factor,y-.74*factor,z)],.026*factor,'copper',12)

def stand(k,x,y,z,h=.8,cc='steel'):
    box(k,(x,y+h*.5,z),(.10,h,.10),cc)
    box(k,(x,y+.045,z),(.42,.09,.40),'dark')
    box(k,(x,y+h,z),(.32,.06,.30),'silver')

def torus_ring_segments(k,c,r=.85,width=.065,accent='copper',axis='y',n=72):
    # A 3D annular tube represented with pipe segments in correct 3-D plane.
    x,y,z=c
    pts=[]
    for i in range(n+1):
        t=2*math.pi*i/n
        if axis=='y':pts.append((x+r*math.cos(t),y,z+r*math.sin(t)))
        else:pts.append((x+r*math.cos(t),y+r*math.sin(t),z))
    pipe(k,pts,width,accent,10)

def statusleds(k,x,y,z,n=8):
    for j in range(n):cyl(k,(x+(j-(n-1)/2)*.12,y,z),.027,.024,['green','cyan','amber','violet','blue'][j%5],'z',10)

# 00 — laboratory floor, embedded socket/status rails and raised bays.
k=names[0]
box(k,(0,-.32,0),(16.1,.52,8.90),'navy')
box(k,(0,-.035,0),(15.82,.055,8.58),'steel')
for zz in (-4.20,4.20):box(k,(0,.013,zz),(15.78,.065,.072),'cyan')
for xx in (-7.83,7.83):box(k,(xx,.013,0),(.070,.065,8.45),'cyan')
for xx in (-7.35,7.35):
    for zz in (-3.75,3.75):cyl(k,(xx,-.63,zz),.19,.17,'dark','y',18)
for j in range(60):
    x=-7.4+j*.251
    cyl(k,(x,.027,4.05),.028,.020,['cyan','blue','green','amber','violet'][j%5],'y',10)
for i in range(7):
    xx=-6.65+i*2.23
    box(k,(xx,-.024,-3.90),(.26,.028,.12),'dark')
for j in range(10):
    box(k,(-6.95+j*1.42,.016,.05),(.075,.030,7.80),'dark')
for j in range(4):
    box(k,(-7.20+j*.12,-.015,3.58),(.055,.017,.35),'amber')

# 01 — central ALD reactor: vertical cylinder, coaxial service flanges and large glass wall.
k=names[1]
rx,ry,rz=(0,2.27,-.80)
mechbox(k,(0,.62,-.80),(3.15,.81,2.95),'steel')
box(k,(0,1.05,-.80),(2.63,.12,2.51),'silver')
cyl(k,(0,ry,-.80),1.13,2.43,'glass','y',48)
for yy in (1.15,1.26,1.43,2.88,3.11,3.34):
    precision_ring(k,(0,yy,-.80),1.18,1.03,.085,'y','steel')
for j in range(24):
    a=j*2*math.pi/24
    xx=1.17*math.cos(a);zz=-.80+1.17*math.sin(a)
    cyl(k,(xx,1.20,zz),.037,.11,'dark','y',10)
    cyl(k,(xx,3.18,zz),.037,.13,'dark','y',10)
for j in range(16):
    a=j*2*math.pi/16
    if math.cos(a)>.72:continue
    xx=1.17*math.cos(a);zz=-.80+1.17*math.sin(a)
    cyl(k,(xx,2.25,zz),.038,2.25,'silver','y',12)
for xx in (-1.4,1.4):
    box(k,(xx,2.16,-.80),(.22,1.40,1.88),'navy')
    for yy in (1.55,2.35,2.80):box(k,(xx,yy,-.80),(.28,.075,2.15),'light')
pipe(k,[(1.10,2.60,-.84),(1.66,2.60,-.84),(1.66,1.85,-1.05),(2.20,1.85,-1.05)],.095,'steel',14)
pipe(k,[(-.84,3.27,-.80),(-.84,3.89,-.80),(-2.00,3.89,-.80)],.075,'copper',12)
statusleds(k,-.40,.72,.68,9)
for i in range(12):box(k,(-.70+i*.13,3.39,-.90),(.056,.024,1.62),'light')

# 02 — service lid, closure ring, compression springs and locking bolts.
k=names[2]
cyl(k,(0,3.51,-.80),1.17,.21,'steel','y',48)
precision_ring(k,(0,3.42,-.80),1.24,1.11,.12,'y','silver')
cyl(k,(0,3.64,-.80),.93,.055,'dark','y',40)
for a in np.linspace(0,2*math.pi,20,endpoint=False):
    x=1.14*math.cos(a);z=-.8+1.14*math.sin(a)
    cyl(k,(x,3.71,z),.055,.10,'silver','y',12)
for x in (-.74,.74):
    box(k,(x,3.78,-.80),(.18,.15,1.3),'silver')
    cyl(k,(x,3.88,-.80),.15,.09,'dark','y',18)
box(k,(1.16,3.61,-.80),(.18,.24,.35),'amber')

# 03 — quartz window and thick perimeter retaining frame at the front.
k=names[3]
box(k,(0,2.25,.40),(2.10,1.63,.027),'glass')
for x in (-1.10,1.10):box(k,(x,2.25,.45),(.095,1.79,.15),'silver')
for yy in (1.42,3.08):box(k,(0,yy,.45),(2.28,.09,.15),'silver')
for x in np.linspace(-1.04,1.04,9):
    for y in (1.42,3.08):cyl(k,(x,y,.52),.023,.050,'dark','z',8)
for i in range(12):box(k,(-.84+i*.14,3.02,.55),(.045,.032,.021),'cyan' if i%2 else 'violet')

# 04 — multi-zone heated wafer chuck, ceramic surface, concentric heating channels.
k=names[4]
cyl(k,(0,1.54,-.80),.86,.32,'steel','y',48)
cyl(k,(0,1.71,-.80),.81,.042,'ceramic','y',48)
for r in (.21,.37,.56,.71):torus_ring_segments(k,(0,1.736,-.80),r,.017,'amber','y',64)
for a in np.linspace(0,2*math.pi,16,endpoint=False):
    cyl(k,(.77*math.cos(a),1.746,-.80+.77*math.sin(a)),.032,.046,'gold','y',10)
for dx in (-.48,.48):
    box(k,(dx,1.40,-.8),(.09,.18,1.44),'copper')
for i in range(4):pipe(k,[(-1.0,1.25,-1.18+i*.19),(-.60,1.25,-1.18+i*.19),(-.52,1.45,-1.18+i*.19)],.035,'copper')

# 05 — silicon wafer disk with grid texture as 3-D geometry.
k=names[5]
waferdisc(k,(0,1.811,-.80),.645,.025,'iridescent',True)
for r in (.25,.46,.58):ring(k,(0,1.828,-.8),r,r-.012,.006,'y','blue',48)

# 06 — actual gold top atomic film, 3D atom-site overlay and radial growth layers.
k=names[6]
cyl(k,(0,1.844,-.80),.64,.014,'gold','y',48)
ring(k,(0,1.853,-.80),.63,.59,.012,'y','amber',48)
for x in np.linspace(-.52,.52,13):
    for z in np.linspace(-1.32,-.28,13):
        if x*x+(z+.80)**2 < .55**2 and (int((x+.55)*10)+int((z+1.32)*10))%3!=0:
            cyl(k,(x,1.862,z),.016,.009,'gold','y',8)

# 07 — cross-section 3D conformality training sample, U-shaped trench array, gold coating.
k=names[7]
mechbox(k,(2.37,2.86,.41),(2.12,.23,1.00),'dark')
for i in range(5):
    x=1.66+i*.36
    box(k,(x,3.30,.43),(.21,.66,.79),'steel')
    box(k,(x,3.66,.43),(.235,.035,.81),'gold')
    for sgn in (-1,1):
        box(k,(x+sgn*.12,3.30,.43),(.032,.65,.81),'gold')
for x in (1.47,3.25):box(k,(x,3.29,.41),(.08,.82,1.07),'silver')
for i in range(9):box(k,(1.57+i*.18,2.88,.95),(.055,.05,.05),'cyan' if i%2 else 'violet')

# 08 — precision wafer lift with pins and motorized pedestal.
k=names[8]
box(k,(0,1.12,-.8),(1.40,.19,1.28),'steel')
for a in np.linspace(0,2*math.pi,3,endpoint=False):
    x=.48*math.cos(a);z=-.8+.48*math.sin(a)
    cyl(k,(x,1.46,z),.043,.55,'steel','y',14)
    cyl(k,(x,1.75,z),.080,.060,'ceramic','y',14)
for x in (-.44,.44):cyl(k,(x,.91,-.8),.075,.31,'dark','y',12)

# 09 — wafer carrier, fifteen visible stored disks and guide columns.
k=names[9]
mechbox(k,(-6.70,.52,-.65),(1.44,.15,1.31),'steel')
for x in (-7.30,-6.10):
    for z in (-1.19,-.11):box(k,(x,1.39,z),(.095,1.62,.10),'silver')
for i,y in enumerate(np.linspace(.65,2.13,15)):
    box(k,(-6.70,y,-.65),(1.11,.016,1.05),'ceramic')
    cyl(k,(-6.70,y+.024,-.65),.45,.011,'iridescent','y',32)
box(k,(-6.70,2.30,-.65),(1.54,.10,1.38),'steel')
for i in range(8):box(k,(-7.26+i*.16,.54,.04),(.080,.034,.035),'green' if i%2 else 'cyan')

# 10 — large transparent vacuum loadlock with framed walls and roof clamps.
k=names[10]
mechbox(k,(-4.61,.69,-.78),(2.65,.24,2.25),'steel')
for x in (-5.86,-3.35):
    for z in (-1.83,.26):box(k,(x,1.66,z),(.12,1.80,.12),'silver')
for y in (.82,2.47):
    for z in (-1.83,.26):box(k,(-4.61,y,z),(2.58,.12,.12),'silver')
box(k,(-4.61,1.66,-1.82),(2.46,1.56,.020),'glass')
box(k,(-4.61,1.66,.25),(2.45,1.59,.022),'glass')
box(k,(-4.61,2.56,-.78),(2.66,.16,2.27),'steel')
for x in (-5.65,-3.57):
    for z in (-1.73,.19):cyl(k,(x,2.65,z),.065,.11,'dark','y',12)
pipe(k,[(-4.62,2.59,-1.85),(-4.62,3.0,-1.85),(-3.16,3.0,-1.85)],.065,'steel')

# 11 — sliding gate valve between loadlock and process reactor.
k=names[11]
mechbox(k,(-2.69,1.66,-.80),(.42,1.12,1.28),'silver')
box(k,(-2.46,1.66,-.80),(.055,.95,1.05),'dark')
box(k,(-2.68,2.46,-.80),(.44,.24,.46),'dark')
cyl(k,(-2.68,2.63,-.80),.14,.22,'copper','y',18)
for y in (1.20,2.13):
    for z in (-1.36,-.23):cyl(k,(-2.43,y,z),.049,.05,'black','x',10)

# 12 — articulated mechanical wafer transfer robot with blade and power harness.
k=names[12]
x,y,z=-4.63,1.03,-.65
mechbox(k,(x,y-.32,z),(.88,.22,.81),'steel')
cyl(k,(x,y,z),.32,.55,'blue','y',24)
cyl(k,(x,y+.38,z),.22,.20,'silver','y',20)
pipe(k,[(x,y+.49,z),(x+.52,y+1.11,z)],.14,'steel',20)
cyl(k,(x+.52,y+1.10,z),.21,.36,'dark','z',20)
pipe(k,[(x+.55,y+1.12,z),(x+1.34,y+1.12,z-.02)],.10,'blue',18)
cyl(k,(x+1.34,y+1.12,z-.02),.20,.15,'steel','y',16)
box(k,(x+1.64,y+1.13,z-.03),(.60,.055,.40),'silver')
for dx in (1.51,1.78):box(k,(x+dx,y+1.18,z-.03),(.19,.02,.42),'cyan')
wirebundle(k,[(x,y+.34,z-.26),(x+.49,y+.98,z-.27),(x+1.28,y+1.00,z-.30)],4,.021,'copper')

# 13/14 — two isolated fictional precursor canisters.
for idx,xx,col in [(13,-7.18,'cyan'),(14,-6.08,'violet')]:
    k=names[idx]
    cylvessel(k,xx,2.50,-2.97,.38,1.70,col)
    pipe(k,[(xx,3.47,-2.97),(xx,3.85,-2.97),(xx+.61,3.85,-2.97)],.055,'copper')
    for yy in (1.74,3.21):box(k,(xx,yy,-2.46),(.50,.065,.075),col)

# 15 — dedicated inert purge reservoir, tall polished tank with white/cyan band.
k=names[15]
cylvessel(k,-4.98,2.47,-3.01,.43,1.93,'white')
for y in (1.92,2.11,2.77,3.14):torus_ring_segments(k,(-4.98,y,-3.01),.445,.022,'cyan')
pipe(k,[(-4.98,3.61,-3.01),(-4.98,3.89,-3.01),(-3.95,3.89,-3.01)],.06,'silver')

# 16/17/18 — individually actuated fast pulse and purge valves.
for idx,xx,cc in [(16,-5.58,'cyan'),(17,-4.67,'violet'),(18,-3.76,'white')]:
    k=names[idx]
    pulsevalve(k,(xx,1.92,.55),cc)
    for t in range(5):box(k,(xx-.24+t*.12,1.61,.79),(.052,.032,.04),cc if t%2==0 else 'dark')

# 19/20 — precision mass flow controllers, displays, injection solenoids.
for idx,xx,cc in [(19,-5.72,'cyan'),(20,-4.61,'violet')]:
    k=names[idx]
    station(k,(xx,1.12,2.43),(.90,.80,.63),cc,3)
    for dx in (-.21,.21):cyl(k,(xx+dx,1.62,2.43),.08,.23,'silver','y',12)
    pipe(k,[(xx-.20,1.62,2.29),(xx-.20,2.14,2.29),(xx-.68,2.14,2.29)],.032,'copper')

# 21 — main purge flow transmitter used for actual final wiring challenge.
k=names[21]
sensor(k,(-3.36,1.17,2.47),'white')
cyl(k,(-3.36,1.66,2.43),.17,.17,'ice','y',16)
for j in range(4):box(k,(-3.60+j*.17,.72,2.75),(.090,.045,.035),'cyan')
pipe(k,[(-3.36,1.60,2.13),(-3.36,2.16,2.13),(-2.49,2.16,2.13)],.034,'silver')

# 22 — separated high-density distribution manifold with color-coded branches.
k=names[22]
mechbox(k,(-2.15,3.76,-2.16),(1.92,.38,.64),'steel')
for j in range(10):
    x=-2.95+j*.18
    cyl(k,(x,3.98,-2.15),.063,.20,'copper','y',12)
    knob(k,x,4.08,-1.82,['cyan','violet','white'][j%3])
for xx in (-2.63,-2.14,-1.65):
    pipe(k,[(xx,3.88,-2.20),(xx,3.88,-1.30),(xx*.48,3.55,-.83)],.035,['cyan','violet','silver'][int((xx+2.63)/.49)%3])

# 23 — precision showerhead: circular multi-port plate, many individual nozzle holes.
k=names[23]
cyl(k,(0,3.11,-.80),.94,.13,'silver','y',52)
ring(k,(0,3.045,-.80),.96,.85,.035,'y','dark',48)
for x in np.linspace(-.79,.79,17):
    for zz in np.linspace(-1.59,-.01,17):
        if x*x+(zz+.80)**2 < .78**2:
            cyl(k,(x,3.035,zz),.013,.018,'dark','y',8)
for a in np.linspace(0,2*math.pi,12,endpoint=False):
    cyl(k,(.86*math.cos(a),3.12,-.8+.86*math.sin(a)),.032,.065,'dark','y',10)
pipe(k,[(0,3.19,-.80),(0,3.50,-.80),(-1.3,3.50,-.80)],.095,'steel')

# 24 — physical triple gas tubing, separate cyan/magenta/purge paths and couplings.
k=names[24]
paths=[([(-7.18,3.86,-2.97),(-6.10,3.86,-2.97),(-6.10,3.35,-1.90),(-3.03,3.35,-1.90)],'cyan'),
       ([(-6.08,3.85,-3.10),(-5.10,3.85,-3.10),(-5.10,3.18,-1.77),(-2.82,3.18,-1.77)],'violet'),
       ([(-4.98,3.90,-3.01),(-4.18,3.90,-3.01),(-4.18,3.52,-2.45),(-2.70,3.52,-2.45)],'silver')]
for p,c in paths:
    pipe(k,p,.052,c,12)
    for xyz in p[1:-1]:cyl(k,xyz,.083,.10,'dark','y',10)
for i in range(18):
    x=-6.6+i*.30
    box(k,(x,3.35,-2.70),(.068,.055,.046),'dark' if i%3 else 'copper')

# 25 — turbomolecular pump with 16 impeller stages and vacuum flanges.
k=names[25]
px,py,pz=(4.08,1.30,-1.15)
mechbox(k,(px,py-.63,pz),(2.04,.20,1.56),'steel')
cyl(k,(px,py,pz),.64,1.52,'steel','x',36)
for x in np.linspace(px-.62,px+.62,16):
    ring(k,(x,py,pz),.655,.598,.038,'x','light',28)
    for a in np.linspace(0,2*math.pi,10,endpoint=False):
        yy=py+.34*math.cos(a);zz=pz+.34*math.sin(a)
        b=trimesh.creation.box(extents=(.026,.41,.115))
        b.apply_transform(trimesh.transformations.rotation_matrix(a,[1,0,0]));b.apply_translation((x,yy,zz));addmesh(k,b,'silver')
for x in (px-.85,px+.85):vacflange(k,(x,py,pz),.73,.12,'x',14)
cyl(k,(px,2.05,pz),.19,.45,'silver','y',16)
for x in np.linspace(px-.60,px+.60,8):box(k,(x,py,pz+.666),(.04,.12,.044),'cyan')

# 26 — backing pump with motor and wrapped cooling fin housing.
k=names[26]
mechbox(k,(6.43,.56,-1.24),(2.05,.22,1.47),'steel')
motor(k,(6.05,1.19,-1.24),.67)
cyl(k,(6.98,1.20,-1.24),.51,.90,'steel','x',28)
for x in np.linspace(6.60,7.33,9):ring(k,(x,1.20,-1.24),.53,.495,.045,'x','light',22)
cyl(k,(6.97,1.83,-1.24),.15,.63,'copper','y',16)
station(k,(6.56,.93,-.47),(.47,.39,.22),'blue',2)

# 27 — automatic vacuum throttle valve, gauge and separate actuator shaft.
k=names[27]
mechbox(k,(2.06,1.78,-1.02),(.40,.79,.60),'steel')
for xx in (1.83,2.30):vacflange(k,(xx,1.78,-1.02),.35,.08,'x',12)
cyl(k,(2.06,2.29,-1.02),.18,.22,'dark','y',20)
box(k,(2.06,2.44,-1.02),(.41,.07,.09),'amber')
for j in range(6):cyl(k,(2.06,1.42+j*.13,-.66),.027,.025,'green' if j>=3 else 'red','z',10)

# 28 — vacuum isolation valve with servo drive and status LEDs.
k=names[28]
mechbox(k,(2.48,1.77,-1.45),(.48,.64,.66),'steel')
for xx in (2.19,2.76):vacflange(k,(xx,1.77,-1.45),.31,.075,'x',10)
box(k,(2.48,2.26,-1.45),(.46,.24,.44),'dark')
cyl(k,(2.48,2.47,-1.45),.14,.20,'copper','y',16)
for j in range(5):box(k,(2.28+j*.10,1.37,-1.07),(.058,.045,.031),'green' if j>1 else 'amber')

# 29 — manifolds, vacuum pipes, tees, clamps and support mounts.
k=names[29]
paths=[[(1.20,1.80,-1.20),(2.12,1.80,-1.20),(3.25,1.80,-1.20)],[(4.86,1.32,-1.15),(5.44,1.32,-1.15),(5.44,1.83,-2.0),(6.42,1.83,-2.0)],[(4.03,2.0,-1.14),(4.03,2.70,-1.14),(1.18,2.70,-1.14)]]
for path in paths:
    pipe(k,path,.105,'silver',14)
    for p in (path[0],path[-1]):vacflange(k,p,.16,.07,'x',8)
for x in (3.15,4.93,5.52):stand(k,x,.19,-1.64,.50)

# 30 — high precision process manometer with digital display and fitting.
k=names[30]
sensor(k,(2.54,3.10,-.22),'cyan')
cyl(k,(2.54,2.72,-.30),.09,.37,'silver','y',18)
pipe(k,[(2.54,2.58,-.30),(2.54,2.36,-.30),(1.18,2.36,-.30)],.043,'copper')

# 31 — mechanical rough-vacuum gauge with radial needle.
k=names[31]
cyl(k,(5.86,2.32,-1.30),.34,.20,'steel','z',32)
cyl(k,(5.86,2.32,-1.18),.29,.018,'white','z',32)
for i in range(13):
    a=.12*math.pi+i*.075*math.pi
    cyl(k,(5.86+.246*math.cos(a),2.32+.246*math.sin(a),-1.153),.011,.028,'dark','z',9)
pipe(k,[(5.86,2.32,-1.14),(6.05,2.52,-1.14)],.018,'red')
cyl(k,(5.86,2.32,-1.145),.032,.024,'dark','z',10)
pipe(k,[(5.86,2.02,-1.30),(5.86,1.83,-1.30),(5.47,1.83,-1.30)],.039,'silver')

# 32 — heated gas delivery thermocouple with corrugated probe sheath.
k=names[32]
sensor(k,(-2.22,2.88,.77),'amber',True)
for y in np.linspace(2.36,2.58,9):ring(k,(-2.22,y,.67),.071,.057,.024,'y','silver',12)
pipe(k,[(-2.22,2.32,.67),(-2.22,1.97,.67),(-1.36,1.97,.67)],.021,'copper')

# 33 — wafer chuck temperature sensor and cable to reactor.
k=names[33]
sensor(k,(1.84,1.10,2.44),'amber',True)
wirebundle(k,[(1.85,.80,2.18),(1.85,.75,1.49),(1.20,1.29,.14)],4,.023,'copper',.010)

# 34 — multi-zone heater control power bank with 6 heat channels and cooling fins.
k=names[34]
mechbox(k,(3.08,.75,2.48),(1.47,1.12,.79),'navy')
for i in range(6):
    x=2.48+i*.235
    box(k,(x,.76,2.90),(.20,.83,.11),'dark')
    box(k,(x,.93,2.98),(.15,.30,.018),'amber')
    for j in range(4):cyl(k,(x,.53+j*.10,2.988),.021,.020,'green' if j<2 else 'red','z',8)
for i in range(18):box(k,(2.45+i*.075,1.34,2.47),(.035,.12,.52),'steel')

# 35 — quartz crystal microbalance: metallic resonator stack, readout and viewport.
k=names[35]
station(k,(4.56,.83,2.46),(.91,.84,.67),'cyan',3)
cyl(k,(4.56,1.45,2.43),.27,.14,'steel','y',32)
cyl(k,(4.56,1.54,2.43),.23,.027,'gold','y',32)
for r in (.08,.15,.21):ring(k,(4.56,1.56,2.43),r,r-.013,.008,'y','white',28)
for j in range(14):box(k,(4.21+j*.050,.89,2.83),(.031,.024,.012),'cyan')

# 36 — in-situ ellipsometer with angled optical emitter, receiver and front screen.
k=names[36]
station(k,(5.68,.82,2.49),(1.05,.86,.71),'violet',3)
for dx in (-.28,.28):
    cyl(k,(5.68+dx,1.43,2.44),.145,.30,'steel','y',22)
    cyl(k,(5.68+dx,1.60,2.44),.080,.13,'cyan' if dx<0 else 'violet','y',18)
pipe(k,[(5.41,1.71,2.44),(5.04,1.89,1.93),(4.43,2.43,.91)],.026,'blue',12)
pipe(k,[(5.95,1.71,2.44),(5.16,2.20,1.98),(4.43,2.43,.91)],.025,'violet',12)

# 37 — optical thickness viewport at right side of reactor, interferometric ring.
k=names[37]
cyl(k,(1.13,2.57,.38),.20,.15,'steel','x',20)
cyl(k,(1.30,2.57,.38),.13,.030,'violet','x',22)
for a in np.linspace(0,2*math.pi,10,endpoint=False):
    cyl(k,(1.32,2.57+.18*math.cos(a),.38+.18*math.sin(a)),.023,.040,'silver','x',10)
pipe(k,[(1.35,2.57,.38),(1.84,2.57,.70),(1.84,1.16,2.20),(5.67,1.16,2.20)],.019,'purpleglass',12)

# 38 — conformal film metrology scanner, wafer turntable and optical U-shaped gantry.
k=names[38]
mechbox(k,(6.89,.52,.70),(1.45,.54,1.42),'steel')
waferdisc(k,(6.88,.89,.66),.46,.029,'iridescent',True)
for dx in (-.56,.56):box(k,(6.88+dx,1.29,.68),(.08,1.17,.10),'silver')
box(k,(6.88,1.86,.68),(1.25,.11,.23),'silver')
cyl(k,(6.88,1.67,.70),.23,.18,'dark','y',20)
cyl(k,(6.88,1.53,.70),.14,.05,'cyan','y',22)
for i in range(6):box(k,(6.4+i*.18,.38,1.44),(.085,.042,.04),'green' if i%2 else 'cyan')

# 39 — giant surface coverage demo display, color-coded mosaic of reaction sites.
k=names[39]
station(k,(-2.12,.87,2.52),(1.03,.91,.71),'teal',2)
for i in range(9):
    for j in range(5):
        x=-2.51+i*.100;y=.97+j*.058
        cyl(k,(x,y,2.92),.020,.012,'cyan' if i<5 else 'violet','z',8)

# 40 — residual vapor analyzer: optical spectra, four inlet probes, pressure couplings.
k=names[40]
station(k,(-.96,.83,2.52),(.99,.84,.67),'violet',3)
for j in range(15):
    x=-1.35+j*.055;h=.07+math.exp(-((j-4)/2.5)**2)*.22+math.exp(-((j-10)/1.5)**2)*.25
    box(k,(x,.90+h*.52,2.90),(.032,h,.014),'cyan' if j<8 else 'violet')
for j in range(4):cyl(k,(-1.27+j*.20,.40,2.94),.042,.075,'copper','z',12)

# 41 — overlap monitor with independent A and B flow bargraph and alarms.
k=names[41]
station(k,(.26,.84,2.51),(.99,.87,.70),'red',3)
for col,x in [('cyan',-.06),('violet',.17)]:
    for j in range(8):box(k,(x+j*.022,.79+j*.028,2.905),(.016,.060,.015),col)
for i in range(5):cyl(k,(.02+i*.12,.47,2.92),.025,.022,'red' if i%2 else 'amber','z',8)

# 42 — cycle growth analyzer with 10-segment bars and nanometre display.
k=names[42]
station(k,(1.42,.83,2.51),(.98,.86,.69),'gold',3)
for j in range(10):box(k,(1.00+j*.093,.62+min(j,7)*.03,2.90),(.064,.13+j*.016,.014),'gold' if j>=5 else 'cyan')

# 43 — four-stage physical sequencer: 4 distinct cyan/white/violet/white illuminated channels.
k=names[43]
mechbox(k,(-4.86,.65,3.50),(3.1,.88,.67),'dark')
for i,(label,c) in enumerate([('A','cyan'),('P','white'),('B','violet'),('P','white')]):
    x=-6.00+i*.76
    box(k,(x,.76,3.86),(.68,.64,.052),'black')
    box(k,(x,.87,3.91),(.56,.29,.015),c)
    for n in range(4):cyl(k,(x-.18+n*.12,.57,3.92),.026,.030,'green' if n<2 else 'amber','z',10)
    box(k,(x,.35,3.80),(.63,.035,.10),c)
for j in range(9):box(k,(-6.17+j*.30,1.15,3.49),(.07,.05,.17),'silver')

# 44 — main ALD PLC rack with CPU, I/O channel LEDs and visible terminal blocks.
k=names[44]
mechbox(k,(5.70,1.27,-3.12),(2.62,2.05,.69),'navy')
box(k,(5.70,1.27,-2.75),(2.45,1.86,.064),'dark')
for i in range(10):
    x=4.58+i*.24
    box(k,(x,1.27,-2.66),(.21,1.69,.099),'steel' if i%3 else 'blue')
    for j in range(6):cyl(k,(x,1.94-j*.19,-2.59),.027,.026,'green' if (i+j)%3 else 'amber','z',9)
    for j in range(5):box(k,(x-.072+j*.034,.51,-2.59),(.022,.06,.042),'silver')
box(k,(5.7,2.32,-3.12),(2.66,.14,.70),'steel')

# 45 — PID wafer temp controller with multi-zone channels.
k=names[45]
station(k,(3.42,.78,3.56),(1.06,.82,.65),'amber',4)
for i in range(4):
    x=3.07+i*.22
    box(k,(x,.96,3.91),(.17,.14,.017),'amber' if i%2 else 'red')

# 46 — vacuum pressure PID controller with gauge ring, tuning knobs.
k=names[46]
station(k,(4.57,.78,3.53),(.94,.84,.66),'cyan',4)
ring(k,(4.58,1.24,3.52),.15,.115,.055,'y','copper',24)

# 47 — purge protection relay with contactors, physically separate red status lens.
k=names[47]
station(k,(5.72,.78,3.54),(.97,.86,.69),'red',2)
for i in range(4):
    x=5.36+i*.23
    box(k,(x,.40,3.92),(.12,.21,.07),'black')
    box(k,(x,.52,3.97),(.055,.085,.042),'red' if i%2==0 else 'amber')

# 48 — fiction-only exhaust treatment canister with baffled filter and scrubber pipe.
k=names[48]
mechbox(k,(2.96,.69,-3.10),(1.25,.33,1.14),'steel')
cyl(k,(2.96,1.55,-3.10),.48,1.42,'steel','y',32)
for i in range(8):ring(k,(2.96,1.00+i*.16,-3.10),.49,.46,.06,'y','light',28)
cyl(k,(2.96,2.34,-3.10),.33,.19,'dark','y',24)
pipe(k,[(2.96,2.49,-3.10),(2.96,2.76,-3.10),(1.53,2.76,-3.10)],.068,'silver')
for i in range(8):box(k,(2.58+i*.10,.63,-2.47),(.055,.06,.043),'green' if i%2 else 'cyan')

# 49 — emergency stop station, industrial robust mushroom button and warning beacons.
k=names[49]
station(k,(6.92,.91,3.52),(.87,.92,.71),'red',2)
cyl(k,(6.92,.76,3.92),.19,.17,'red','z',30)
ring(k,(6.92,.76,4.02),.24,.20,.032,'z','amber',28)
for dx in (-.22,.22):cyl(k,(6.92+dx,1.47,3.47),.10,.13,'amber' if dx<0 else 'green','y',18)

# 50 — tall holographic SCADA with green/cyan process charts and golden atomic map.
k=names[50]
giantdisplay(k,(4.99,3.63,-3.43),4.35,2.08)
for x in (3.39,6.60):
    box(k,(x,2.26,-3.49),(.21,1.05,.28),'steel')
    box(k,(x,1.70,-3.49),(.63,.14,.60),'dark')
for i in range(12):
    xx=3.34+i*.31
    box(k,(xx,3.30,-3.31),(.17,.045,.032),'gold' if i%4 else 'cyan')
for i in range(5):
    for j in range(5):
        x=5.61+i*.17;y=3.76+j*.13
        cyl(k,(x,y,-3.312),.052,.013,['cyan','violet','gold','teal'][int((i+j)%4)],'z',12)

# 51 — standalone 5 V control power supply with connectors and venting.
k=names[51]
station(k,(7.13,1.11,-2.39),(.89,.71,.59),'green',2)
for x in (6.83,7.11,7.37):
    cyl(k,(x,.73,-2.08),.059,.095,'copper','z',12)
    cyl(k,(x,.73,-2.00),.036,.045,'red' if x<7 else 'blue','z',10)
for j in range(9):box(k,(6.75+j*.095,1.48,-2.42),(.035,.12,.27),'light')

assert len(names)==52
for i,n in enumerate(names):
    assert n in M and len(M[n])>0,(i,n,'missing mesh')
    for mesh,col in M[n]:
        assert len(mesh.faces)>0 and np.isfinite(mesh.vertices).all(),(i,n,'invalid mesh')

# World-space references, not bound animation/joint rigs.
pivots=[
('ReactorServiceLid','ANIM_REACTOR_LID',(0.,3.51,-.80)),
('LoadLockGateValve','ANIM_LOADLOCK_GATE',(-2.69,1.66,-.80)),
('AutomatedWaferTransferRobot','ANIM_WAFER_ROBOT_BASE',(-4.63,1.03,-.65)),
('AutomatedWaferTransferRobot','ANIM_WAFER_ROBOT_ARM',(-4.11,2.13,-.65)),
('AutomatedWaferTransferRobot','ANIM_WAFER_ROBOT_TOOL',(-3.29,2.15,-.68)),
('WaferLiftPinAssembly','ANIM_WAFER_LIFT',(0.,1.46,-.8)),
('MultiZoneHeatedWaferChuck','ANIM_WAFER_CHUCK',(0.,1.65,-.8)),
('HighSpeedPulseValveA','ANIM_PULSE_VALVE_A',(-5.58,2.30,.55)),
('HighSpeedPulseValveB','ANIM_PULSE_VALVE_B',(-4.67,2.30,.55)),
('HighSpeedPurgeValve','ANIM_PURGE_VALVE',(-3.76,2.30,.55)),
('MassFlowControllerA','ANIM_MFC_A',(-5.72,1.12,2.43)),
('MassFlowControllerB','ANIM_MFC_B',(-4.61,1.12,2.43)),
('TurbomolecularVacuumPump','ANIM_TURBO_ROTOR',(4.08,1.30,-1.15)),
('VacuumBackingPump','ANIM_BACKING_ROTOR',(6.43,1.20,-1.24)),
('AutomaticVacuumThrottleValve','ANIM_THROTTLE_VALVE',(2.06,1.78,-1.02)),
('VacuumIsolationValve','ANIM_VACUUM_ISOLATION',(2.48,1.77,-1.45)),
('PrecisionDepositionShowerhead','ANIM_SHOWERHEAD_SERVICE',(0.,3.11,-.80)),
('InSituOpticalEllipsometer','ANIM_ELLIPSOMETER_HEAD',(5.68,1.56,2.44)),
('ConformalFilmMetrologyScanner','ANIM_METROLOGY_SCANNER',(6.88,1.72,.68)),
('EmergencyDepositionSafetyStation','ANIM_ESTOP',(6.92,.76,3.99)),
('HolographicAtomicDepositionSCADA','ANIM_ALD_SCADA',(4.99,3.63,-3.43)),
('AtomicThinFilmVisualization','ANIM_ATOMIC_LAYER_GROWTH',(0.,1.85,-.80)),
('FourStageALDCycleSequencer','ANIM_FOUR_STAGE_DISPLAY',(-4.86,.86,3.90)),
('HighAspectRatioTrenchSample','ANIM_TRENCH_CONFORMALITY',(2.37,3.27,.41)),
]
assert len(pivots)==24

# FBX binary 7400 exporter; standard Nodes/Objects/Connections
class N:
 def __init__(self,name,*p,ch=None):self.name=name;self.p=p;self.ch=ch or []
def prop(v):
 if isinstance(v,bool):return b'C'+struct.pack('<?',v)
 if isinstance(v,int):return b'L'+struct.pack('<q',v)
 if isinstance(v,float):return b'D'+struct.pack('<d',v)
 if isinstance(v,str):
  b=v.encode('utf8');return b'S'+struct.pack('<I',len(b))+b
 if isinstance(v,bytes):return b'R'+struct.pack('<I',len(v))+v
 if isinstance(v,np.ndarray):
  if v.dtype.kind=='f':v=v.astype('<f8');t=b'd'
  elif v.dtype.kind in ('i','u'):v=v.astype('<i4');t=b'i'
  else:raise ValueError(v.dtype)
  raw=v.tobytes();comp=zlib.compress(raw,6)
  return t+struct.pack('<III',v.size,1,len(comp))+comp
 raise TypeError(type(v))
def encnode(node,start):
 name=node.name.encode('ascii');ps=b''.join(prop(x) for x in node.p)
 body=bytearray();p0=start+13+len(name)+len(ps)
 for child in node.ch:
  b=encnode(child,p0+len(body));body.extend(b)
 if node.ch:body.extend(bytes(13))
 end=start+13+len(name)+len(ps)+len(body)
 return struct.pack('<III',end,len(node.p),len(ps))+struct.pack('<B',len(name))+name+ps+body

def fbx(k,path):
 meshes=M[k];nobj=[];con=[];uid=100000
 def getid():
  nonlocal uid;uid+=1;return uid
 root=getid()
 props70=N('Properties70',ch=[N('P','Lcl Translation','Lcl Translation','','A',0.0,0.0,0.0),N('P','Lcl Rotation','Lcl Rotation','','A',0.0,0.0,0.0),N('P','Lcl Scaling','Lcl Scaling','','A',1.0,1.0,1.0)])
 nobj.append(N('Model',root,k+'\x00\x01Model','Null',ch=[N('Version',232),N('Properties70',ch=props70.ch),N('Shading','Y'),N('Culling','CullingOff')]))
 con.append(N('C','OO',root,0))
 colorMat={}
 for idx,(mesh,color) in enumerate(meshes):
  # vertex coordinates in meters scaled 100 for FBX centimeter convention, importer scale .01 -> meters
  vertices=np.array(mesh.vertices,dtype=np.float64)*100.0
  tris=np.array(mesh.faces,dtype=np.int32)
  faceindices=tris.copy();faceindices[:,-1]=-(faceindices[:,-1]+1)
  gid=getid();mid=getid();geo=N('Geometry',gid,f'{k}_Geom_{idx:03d}\x00\x01Geometry','Mesh',ch=[N('GeometryVersion',124),N('Vertices',vertices.reshape(-1)),N('PolygonVertexIndex',faceindices.reshape(-1)),N('LayerElementMaterial',0,ch=[N('Version',101),N('Name',''),N('MappingInformationType','AllSame'),N('ReferenceInformationType','IndexToDirect'),N('Materials',np.array([0],dtype=np.int32))]),N('Layer',0,ch=[N('Version',100),N('LayerElement',ch=[N('Type','LayerElementMaterial'),N('TypedIndex',0)])])])
  nobj.append(geo)
  nobj.append(N('Model',mid,f'{k}_Part_{idx:03d}\x00\x01Model','Mesh',ch=[N('Version',232),N('Properties70',ch=[N('P','Lcl Translation','Lcl Translation','','A',0.0,0.0,0.0),N('P','Lcl Rotation','Lcl Rotation','','A',0.0,0.0,0.0),N('P','Lcl Scaling','Lcl Scaling','','A',1.0,1.0,1.0)]),N('Shading','Y'),N('Culling','CullingOff')]))
  con.extend([N('C','OO',gid,mid),N('C','OO',mid,root)])
  if color not in colorMat:
   matid=getid();rgb=[q/255 for q in P[color][:3]]
   nobj.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(v*.28 for v in rgb)),N('P','SpecularColor','Color','','A',.6,.65,.68),N('P','Shininess','Number','','A',55.0),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
   colorMat[color]=matid
  con.append(N('C','OO',colorMat[color],mid))
 top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Procedural 3D Asset Builder')]),N('GlobalSettings',ch=[N('Version',1000),N('Properties70',ch=[N('P','UpAxis','int','Integer','',1),N('P','UpAxisSign','int','Integer','',1),N('P','FrontAxis','int','Integer','',2),N('P','FrontAxisSign','int','Integer','',1),N('P','CoordAxis','int','Integer','',0),N('P','CoordAxisSign','int','Integer','',1),N('P','UnitScaleFactor','double','Number','',1.0),N('P','OriginalUnitScaleFactor','double','Number','',1.0)])]),N('Definitions',ch=[N('Version',100),N('Count',len(nobj))]),N('Objects',ch=nobj),N('Connections',ch=con)]
 header=b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400)
 blob=bytearray(header)
 for node in top:blob.extend(encnode(node,len(blob)))
 blob.extend(bytes(13));blob.extend(bytes(160))
 path.write_bytes(blob)

def glb(k,path):
 sc=trimesh.Scene()
 for i,(m,c) in enumerate(M[k]):sc.add_geometry(m,geom_name=f'{k}_Part_{i:03d}')
 path.write_bytes(sc.export(file_type='glb'))




# --------------------------------------------------------------
# Export 52 modular models and combined art-only workstation.
manifest=[]
for i,k in enumerate(names):
    bn=f'{i:02d}_L72_{k}'
    fbx(k,MD/(bn+'.fbx'))
    glb(k,GD/(bn+'.glb'))
    triangles=sum(len(mesh.faces) for mesh,c in M[k])
    vv=np.concatenate([mesh.vertices for mesh,c in M[k]])
    manifest.append(dict(index=i,file=bn+'.fbx',glb=bn+'.glb',name='L72_'+k,triangle_count=triangles,
       mesh_parts=len(M[k]),bbox_min=np.round(vv.min(axis=0),4).tolist(),bbox_max=np.round(vv.max(axis=0),4).tolist(),
       animation_pivots={p:list(coord) for label,p,coord in pivots if label==k}))
    print(f'[{i+1:02d}/52] {k:40} {triangles:7,d} triangles {len(M[k]):4d} parts',flush=True)

combined='Level72_Complete_ArtKit_Showcase'
M[combined]=[piece for k in names for piece in M[k]]
fbx(combined,MD/(combined+'.fbx'))
glb(combined,GD/(combined+'.glb'))
total=sum(m['triangle_count'] for m in manifest)
parts=sum(m['mesh_parts'] for m in manifest)
with (ROOT/'Animation_Rigs.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','Pivot','X','Y','Z','Notes'])
    for name,p,xyz in pivots:w.writerow(['L72_'+name,p,*xyz,'World-space animation/visual reference; create moving mesh subhierarchies in Unity as needed.'])
with (ROOT/'Art_Assembly.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','FBX','PositionX','PositionY','PositionZ','RotationX','RotationY','RotationZ','Scale','Notes'])
    for model in manifest:w.writerow([model['name'],model['file'],0,0,0,0,0,0,1,'Shared origin; FBX mesh numeric cm; verify Unity importer scale'])
with (ROOT/'Component_Roles.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Index','Model','FBX','Triangles','MeshParts','AnimationRefs'])
    for m in manifest:w.writerow([m['index'],m['name'],m['file'],m['triangle_count'],m['mesh_parts'],';'.join(m['animation_pivots'])])
palette={}
for c,rgba in P.items():
    palette['PS_'+c]={'rgba_255':list(rgba),
      'metallic':(.85 if c in ('steel','light','silver','copper','gold') else .2 if c in ('dark','black','navy') else .10),
      'smoothness':(.84 if c in ('steel','silver','copper','gold','glass','purpleglass') else .55),
      'transparent':c in ('glass','purpleglass','plasma'),
      'emission_suggested':c in ('cyan','violet','amber','green','teal','gold','blue')}
(ROOT/'Material_Palette.json').write_text(json.dumps({'level':'72','description':'Procedural material color regions with suggested Unity settings; not handmade PBR texture maps','materials':palette},indent=2))

wiring_budget=[
 ('Control power and returns',10,'5V controls, sensor power, returns'),
 ('Vacuum pumps and pressure',14,'Pumps, valve commands, pressure sensing'),
 ('Virtual precursors and pulse valves',18,'Fictional reactants A/B, pulse actuation, MFCs'),
 ('Purge flow and sensing',14,'Purge valve, flow transmitter and sequencing interlocks'),
 ('Wafer heating and loadlock',12,'Heater zones, thermometry, wafer robotics'),
 ('In-situ metrology',12,'QCM, ellipsometer, conformality checks'),
 ('PLC, safety and SCADA',14,'Process sequencing, protection, operator display')]
assert sum(r[1] for r in wiring_budget)==94
with (ROOT/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Group','ProposedPlayerWires','Purpose','Status'])
    for name,count,purpose in wiring_budget:w.writerow([name,count,purpose,'PLANNED_ONLY_NOT_IMPLEMENTED'])

manifest_obj={'level_id':'level_72','display_name':'Layer by Layer',
 'asset_kind':'Procedural HD / AAA-inspired modular 3D geometry; no handmade PBR texture atlas',
 'individual_models':52,'matching_individual_glbs':52,'individual_triangle_count':total,'individual_mesh_parts':parts,
 'animation_pivot_reference_count':len(pivots), 'planned_player_wires':94,'planned_unique_functional_terminals':146,
 'actual_player_wires_included':False,'functional_spark_terminals_included':False,
 'ald_physics_included':False,'six_gameplay_objectives_implemented':False,
 'glb_scale':'numeric metres','fbx_numeric_scale':'numeric centimetres with UnitScaleFactor=1; Unity import must convert to metres',
 'coordinate_system':'X=width Y=up Z=front; all models share the same world origin',
 'approx_workbench_width_m':16.1,
 'assembled_showcase':'Models/'+combined+'.fbx',
 'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(manifest_obj,indent=2),encoding='utf-8')
(ROOT/'Gameplay_Integration_Plan.md').write_text('''# Project Spark — Level 72 Gameplay Integration Plan

## What is in this package
- 52 **actual independent modular binary FBX** models and 52 matching GLB geometry assets.
- One assembled FBX and GLB reference. Do not instantiate them together with the 52 individual art assets.
- Unity art placement, material preview/upgrading, and a visual-only rig adapter.
- World-space pivot references, material palette, assembled placements, geometry export sources, asset verification.

## NOT INCLUDED: gameplay systems
- **94 player wires**: design budget only; no W01-W94 terminal endpoints created.
- **146 actual `SparkTerminal` sockets**: not included; must be created and validated against existing Project Spark source.
- Physics: pumpdown, inert purge, self-limiting surface reactions, nanofilm, sensor signal, temperature and pressure PID: not implemented.
- Authoritative 6/6 objectives: not implemented.

## Proposed process storyline
**Level 72 — LAYER BY LAYER**: an educational fictitious ALD machine, A dose / purge / B dose / purge. Process targets: 12 nm film, ~0.10 nm nominal per valid cycle, within 0.3 nm; <=3% proposed nonuniformity, >=95% bottom/top trench conformality. All are fictional balance targets, not real chemical instructions.

A cleanroom wafer is loaded; the player commissions vacuum, wafer temperature, and fictional precursor flow, executes a safe four-stage ALD cycle, deposits a conformal coating, recovers from purge restriction/heater-zone drift/stuck A valve, then physically disconnects a purge-flow feedback wire, repairs, recalibrates, resets, requalifies and reaches authoritative `LevelGamePlayManager` 6/6.

## Build and scale
FBX numeric geometry is centimetres with UnitScaleFactor=1; GLBs use metres. Verify Unity import dimensions (~16.1 m platform width), avoid double scaling. Use 52 numbered FBXs at a **common origin**, no manually offsetting each model.

The animation pivot CSV contains location **references**, not joints, clips or guaranteed separately rigged moving meshes. Rig components in Unity before animating parts. Artwork materials have color regions but need production shader/PBR work. Unity import and runtime not tested.
''',encoding='utf-8')
(ROOT/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 72: LAYER BY LAYER
==========================================

ART PACKAGE: 52 modular binary FBX + 52 GLB matching backup models; assembled FBX and GLB reference.

1. Extract this ZIP and import Models/*.fbx (the 52 numbered FBXs) into your Unity Assets folder.
2. Put *ArtPlacementEditor.cs and *MaterialUpgradeEditor.cs into Assets/.../Editor/ (or another Editor-only asmdef).
3. Select the existing Level 72 layout root GameObject in the Unity hierarchy.
4. Run Tools > Project Spark > Levels > 72 > Place HD Art Under Selected Layout.
5. The helper creates L72_HD_VISUAL_ART and instantiates the 52 numbered FBXs at (0,0,0), preserving the shared origin.
6. Do not also instantiate Level72_Complete_ArtKit_Showcase.fbx; that model is an assembled reference.
7. Run Tools > Project Spark > Levels > 72 > Create & Apply HD Materials To Selected Art if desired. Check shader compatibility with your built-in/URP/HDRP project.
8. VisualRig.cs is optional, visual-only, and expects explicitly bound child moving parts. The pivot CSV lists world-space REFERENCES, not baked skeletons.
9. Mesh authoring: X width, Y up, Z front. Construction in metres; GLB numeric metres, binary FBX numeric centimetres with UnitScaleFactor=1. Check FBX importer: 16.1 m workstation width expected in Unity.

NOTE: Geometry is procedural HD / AAA-inspired but not handmade AAA PBR textured. Material regions are imported; artistic PBR maps and LODs are not included. Unity compilation / Play Mode import was NOT tested.

IMPORTANT: This is ART ONLY. No 94 functional player wires, 146 SparkTerminal objects, ALD physics, four-stage sequencer, valid purge feedback or LevelGamePlayManager 6/6 objectives exist in this pack. Gameplay_Wiring_Budget.csv only allocates proposed groups, it is not an actual wire endpoint manifest.

DEVELOPMENT: For code integration, inspect the actual Unity Project Spark source and APIs (SparkTerminal, SparkElectricalSolver, SparkLevelEvaluator, LevelGamePlayManager) before creating gameplay controllers. Keep 5V educational controls separate from simulated heater and pump energy. The precursors A/B are fictional nonhazardous virtual chemicals.

Files: Model_Manifest.json, Animation_Rigs.csv, Art_Assembly.csv, Component_Roles.csv, Material_Palette.json, Gameplay_Wiring_Budget.csv, Gameplay_Integration_Plan.md, Verification_Report.json, Source/.
''',encoding='utf-8')
print('EXPORTED',len(manifest),'models + assembled,',total,'triangles,',parts,'parts,',len(pivots),'reference pivots',flush=True)
