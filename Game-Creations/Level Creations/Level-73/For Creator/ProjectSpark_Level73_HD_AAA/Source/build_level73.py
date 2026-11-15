#!/usr/bin/env python3
"""Project Spark Level 73: procedural original CMP 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT electrical CMP gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level73_HD_AAA')
MD=ROOT/'Models';GD=ROOT/'GLB';PD=ROOT/'Previews';ED=ROOT/'Unity_Optional';SRC=ROOT/'Source'
for d in (MD,GD,PD,ED,SRC): d.mkdir(parents=True,exist_ok=True)
P={'navy':(13,29,47,255),'steel':(71,101,124,255),'light':(150,179,195,255),'silver':(194,207,214,255),'dark':(29,48,63,255),'black':(12,22,32,255),'cyan':(39,215,246,255),'blue':(27,118,213,255),'ice':(166,232,249,255),'copper':(221,129,62,255),'amber':(252,179,68,255),'red':(238,72,79,255),'green':(50,224,149,255),'white':(229,240,245,255),'glass':(85,178,222,93),'frost':(205,229,244,255),'rubber':(45,48,52,255),'gold':(233,188,86,255),'violet':(166,92,242,255),'plasma':(174,90,242,160),'purpleglass':(150,91,230,115),'teal':(63,214,202,255),'ceramic':(224,222,211,255),'iridescent':(102,197,239,255),'warning':(255,112,67,255),'pad':(60,64,77,255),'padgroove':(26,35,47,255),'slurry':(68,204,238,157),'granite':(87,103,113,255)}
M={}; ACTIVE=None
@contextlib.contextmanager
def moving(pivot):
 global ACTIVE
 prev=ACTIVE;ACTIVE=pivot
 try:yield
 finally:ACTIVE=prev

def addmesh(k,m,c):
 if m is None or len(m.faces)==0:return
 if k not in M:M[k]=[]
 m.visual.vertex_colors=np.tile(P[c],(len(m.vertices),1))
 M[k].append((m,c,ACTIVE))

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

# ---------------------------------------------------------------
# LEVEL 73: POLISH TO PERFECTION — 54 distinctly modeled modules.
# The platen is centered at (0, 1.27, -0.55), radius 2.55 m.
# Every modular FBX is authored against the SAME global stage origin.
# This allows simple prefab placement at localPosition=Vector3.zero.
# ---------------------------------------------------------------
names=['IndustrialCMPWorkstationBase','TransparentCMPSafetyEnclosure','RotatingPolishingPlaten','GroovedPolishingPad','PlatenServoMotorDrive','PrecisionWaferCarrierHead','CarrierGimbalAssembly','MultiZonePressureBladder','WaferRetainingRing','SiliconWaferSubstrate','PatternedThinFilmLayer','MicroscopicSurfaceTopography','WaferCarrierCassette','AutomatedWaferTransferRobot','WaferLiftAndAlignmentAssembly','VirtualSlurrySupplyCabinet','AbrasiveSlurryMixingTank','PrecisionSlurryMeteringPump','SlurryMassFlowTransmitter','MotorizedSlurryDispensingArm','PrecisionSlurryNozzle','SlurryConcentrationSensor','SlurryTemperatureSensor','PadConditioningSwingArm','PadConditionerAbrasiveDisc','PadConditionerMotorDrive','PadWearMeasurementSensor','PadGrooveInspectionScanner','CarrierDownforceLoadCell','CarrierPressureRegulator','PlatenSpeedEncoder','CarrierSpeedEncoder','MotorTorqueCurrentAnalyzer','PolishingVibrationSensor','AcousticEmissionSensor','InterfaceTemperatureProbe','PolishingCoolingLoop','TransparentSlurrySplashGuard','WasteSlurryCollectionModule','CMPEmergencySafetyStation','PostCMPRinseChamber','WaferBrushCleaningAssembly','SpinRinseDryer','InSituOpticalEndpointSensor','EddyCurrentThicknessSensor','OpticalSurfaceProfilometer','WaferDefectInspectionCamera','DishingErosionAnalyzer','MultiZonePressurePIDController','PlatenCarrierMotionController','CMPProcessPLCRack','CMPDigitalIOModule','HolographicCMPSCADA','Regulated5VControlSupply']
assert len(names)==54

# Pivot locations for moving geometry. These are actual FBX parent-null transforms.
PV={}
def register(index,pivot,position,kind='rotational',axis='y'):
 PV.setdefault(names[index],{})[pivot]=dict(position=tuple(position),kind=kind,axis=axis)

def screw(k,p,r=.045,axis='y',head='silver'):
 cyl(k,p,r,.027,head,axis,12)
 # tiny cross on the exposed head, visible in close-up
 x,y,z=p
 if axis=='y':
  box(k,(x,y+.020,z),(r*.95,.005,.010),'dark'); box(k,(x,y+.020,z),(.010,.005,r*.95),'dark')
 elif axis=='z':
  box(k,(x,y,z+.020),(r*.95,.010,.005),'dark');box(k,(x,y,z+.020),(.010,r*.95,.005),'dark')

def tier(k,x,y,z,w,h,d,body='steel'):
 mechbox(k,(x,y,z),(w,h,d),body)
 box(k,(x,y+h*.49,z),(w*.94,.045,d*.93),'light')
 for xx in (x-w*.38,x+w*.38):
  for zz in (z-d*.36,z+d*.36):screw(k,(xx,y+h*.52,zz),.032,'y')

def finbank(k,p,w,n=15,depth=.42,height=.55,cc='silver'):
 x,y,z=p
 for i in range(n):box(k,(x-w*.48+i*w*.96/(n-1),y,z),(.023,height,depth),cc)

def terminal_block(k,p,n=8,axis='x',accent='cyan'):
 x,y,z=p
 box(k,(x,y,z),(n*.14+.15,.21,.18),'black')
 for i in range(n):
  xx=x+(i-(n-1)/2)*.14
  box(k,(xx,y,z+.103),(.10,.15,.034),'steel')
  cyl(k,(xx,y+.025,z+.13),.037,.035,'copper','z',10)
  cyl(k,(xx,y+.025,z+.162),.018,.016,accent if i%3==0 else 'red','z',8)

def lightstrip(k,p,size=(1.0,.025,.028),accent='cyan'):
 box(k,p,size,accent)

def zpanel(k,p,w=1.0,h=.8,d=.55,accent='cyan',knobs=4):
 station(k,p,(w,h,d),accent,knobs)
 terminal_block(k,(p[0],p[1]-h*.41,p[2]+d*.51),min(7,knobs+3),accent=accent)

def axial(k,p,r=.34,height=.12,acc='steel',sector=48):
 cyl(k,p,r,height,acc,'y',sector)
 ring(k,(p[0],p[1]+height*.53,p[2]),r*1.045,r*.97,.035,'y','silver',sector)

def bolt_circle(k,c,r,n=16,head='silver',yoffset=0.0):
 x,y,z=c
 for i in range(n):
  a=2*math.pi*i/n
  screw(k,(x+r*math.cos(a),y+yoffset,z+r*math.sin(a)),.033,'y',head)

def radial_grooves(k,c,rmin,rmax,n=24,y=1.65):
 x,_,z=c
 for i in range(n):
  a=2*math.pi*i/n
  p1=(x+rmin*math.cos(a),y,z+rmin*math.sin(a))
  p2=(x+rmax*math.cos(a),y,z+rmax*math.sin(a))
  pipe(k,[p1,p2],.012,'padgroove',7)

def gauge(k,p,r=.18):
 x,y,z=p
 cyl(k,p,r,.09,'black','z',22)
 ring(k,(x,y,z+.05),r,r*.88,.04,'z','silver',24)
 cyl(k,(x,y,z+.05),r*.82,.016,'white','z',24)
 pipe(k,[(x,y,z+.068),(x+r*.58,y+r*.33,z+.075)],.014,'red',8)
 for i in range(11):
  a=math.pi*.15 + math.pi*.75*i/10
  box(k,(x+r*.67*math.cos(a),y+r*.67*math.sin(a),z+.07),(.018,.035,.017),'black')

def hd_cabinet(k,p,ext=(1.1,1.35,.75),accent='cyan',screens=1):
 x,y,z=p;w,h,d=ext
 tier(k,x,y,z,w,h,d,'navy')
 for yy in (y-h*.32,y+h*.35):
  for zz in (z-d*.49,z+d*.49):lightstrip(k,(x,yy,zz),(w*.95,.015,.02),accent)
 for j in range(screens):
  compact_screen(k,(x+(j-(screens-1)/2)*w*.47,y+h*.18,z+d*.53),w*.45 if screens>1 else w*.73,h*.31,'blue')
 for i in range(7):cyl(k,(x-w*.35+i*w*.12,y-h*.28,z+d*.55),.025,.018,accent if i%3 else 'green','z',10)
 for ix in (-1,1):
  for iz in (-1,1):box(k,(x+ix*w*.43,y-h*.53,z+iz*d*.40),(.105,.11,.13),'dark')
 terminal_block(k,(x,y-h*.43,z+d*.55),min(9,int(w*7)),accent=accent)

def pipe3(k,points,r=.03,col='cyan',sections=10):
 pipe(k,points,r,col,sections)
 for q in points[1:-1]:cyl(k,q,r*1.25,.036,'silver','y',10)

def angular_arm(k,p0,p1,r=.11,accent='steel'):
 pipe(k,[p0,p1],r,accent,20)
 for q in (p0,p1):
  m=trimesh.creation.icosphere(subdivisions=2,radius=r*1.28);m.apply_translation(q);addmesh(k,m,'silver')

def spiral_track(k,c,r0,r1,turns=5,n=175,y=None,accent='cyan',width=.019):
 x,yy,z=c
 if y is None:y=yy
 pts=[]
 for i in range(n+1):
  t=i/n;ang=t*turns*2*math.pi;r=r0+(r1-r0)*t
  pts.append((x+r*math.cos(ang),y,z+r*math.sin(ang)))
 pipe(k,pts,width,accent,7)

# 00 — Industrial CMP main chassis with perimeter rail, instrument slots and bolted supports.
k=names[0]
tier(k,0,.29,0,17.15,.54,8.90,'navy')
box(k,(0,.57,0),(16.62,.11,8.35),'dark')
for z in (-4.15,4.15):
 box(k,(0,.63,z),(16.9,.14,.16),'steel')
 lightstrip(k,(0,.73,z+(.10 if z>0 else -.10)),(16.6,.028,.036),'cyan')
for x in (-8.10,8.10):
 box(k,(x,.65,0),(.15,.16,8.35),'silver')
 for z in np.linspace(-3.8,3.8,16):screw(k,(x,.745,z),.04,'y')
for x in np.linspace(-7.8,7.8,32):
 cyl(k,(x,.617,4.23),.032,.028,'cyan' if round(x*10)%3 else 'red','y',10)
for x in (-6.9,-3.6,0,3.6,6.9):
 for z in (-3.65,3.65):box(k,(x,-.08,z),(1.10,.25,.65),'steel')
for x in (-7.75,7.75):
 for z in (-3.60,3.60):
  cyl(k,(x,.62,z),.20,.040,'silver','y',24)
  screw(k,(x,.655,z),.11,'y','dark')

# 01 — protective enclosure with transparent door panels, heavy steel frame and warning details.
k=names[1]
frame(k,(0,2.38,-.55),(7.30,3.66,6.25),beam=.115,glass=True)
for z in (-3.48,2.38):
 for y in (1.06,3.45):lightstrip(k,(0,y,z),(6.97,.024,.035),'cyan')
for x in (-3.52,3.52):
 box(k,(x,2.45,2.39),(.12,3.2,.11),'silver')
for x in (-2.70,2.70):box(k,(x,4.22,-.55),(.16,.19,5.72),'dark')
for x in (-1.85,0,1.85):
 box(k,(x,4.23,2.48),(.16,.12,.11),'amber')
for x in (-3.48,3.48):
 for y in (1.08,3.70):
  screw(k,(x,y,2.46),.055,'z')

# 02 — massive precision platen: lathe rings, shaft bolts and concentric hub stack.
k=names[2]
register(2,'ANIM_PLATEN_ROTATION',(0,1.23,-.55))
with moving('ANIM_PLATEN_ROTATION'):
 axial(k,(0,1.17,-.55),2.54,.30,'steel',64)
 axial(k,(0,1.34,-.55),2.40,.16,'dark',64)
 ring(k,(0,1.365,-.55),2.48,2.38,.095,'y','silver',64)
 ring(k,(0,1.155,-.55),2.54,2.44,.10,'y','light',64)
 for r in [2.23,2.08,1.84,1.55,1.15]:ring(k,(0,1.445,-.55),r,r-.018,.017,'y','steel',64)
 bolt_circle(k,(0,1.38,-.55),2.45,40,'dark')
 for i in range(28):
  a=2*math.pi*i/28
  box(k,(2.53*math.cos(a),1.24,-.55+2.53*math.sin(a)),(.052,.12,.06),'light')
 axial(k,(0,.92,-.55),.39,.45,'copper',24)
 precision_ring(k,(0,1.03,-.55),.51,.45,.07,'y','silver')

# 03 — dark textured CMP consumable pad, detailed radial and curved slurry grooves.
k=names[3]
register(3,'ANIM_PAD_ROTATION',(0,1.61,-.55))
with moving('ANIM_PAD_ROTATION'):
 axial(k,(0,1.55,-.55),2.33,.105,'pad',64)
 for r in np.linspace(.22,2.23,19):ring(k,(0,1.612,-.55),float(r),float(r-.012),.015,'y','padgroove',64)
 radial_grooves(k,(0,0,-.55),.16,2.25,n=32,y=1.623)
 for i in range(26):
  a=2*math.pi*i/26
  r=1.88
  cyl(k,(r*math.cos(a),1.626,-.55+r*math.sin(a)),.026,.008,'granite','y',10)
 ring(k,(0,1.64,-.55),2.31,2.27,.025,'y','light',64)

# 04 — plated servo + stator housing + fin array and power/control cables.
k=names[4]
mechbox(k,(0,.68,-.55),(1.82,.37,1.60),'dark')
with moving('ANIM_PLATEN_SERVO_ROTOR'):
 sscylinder(k,(0,.95,-.55),.34,.72,'y',11)
 cyl(k,(0,1.29,-.55),.14,.28,'copper','y',24)
register(4,'ANIM_PLATEN_SERVO_ROTOR',(0,.95,-.55))
for x in (-.80,.80):
 for z in (-1.08,-.02):screw(k,(x,.88,z),.055)
finbank(k,(0,.70,.30),1.38,18,.25,.32,'light')
pipe3(k,[(.70,.76,-.40),(1.12,.77,-.38),(1.55,.96,-1.46)],.055,'copper')
terminal_block(k,(0,.72,.32),10,accent='cyan')

# 05 — carrier spindle and tall pressure head with retractable lift.
k=names[5]
register(5,'ANIM_CARRIER_ROTATION',(-.67,2.44,-.80))
register(5,'ANIM_CARRIER_Z',(-.67,3.34,-.80),'linear','y')
mechbox(k,(1.48,3.72,-.80),(2.8,.42,1.35),'steel')
box(k,(2.73,2.88,-.80),(.40,2.0,.60),'navy')
box(k,(2.73,1.88,-.80),(.92,.23,.86),'steel')
with moving('ANIM_CARRIER_Z'):
 cyl(k,(-.67,3.32,-.8),.53,.66,'steel','y',40)
 for h in (3.12,3.35,3.58):ring(k,(-.67,h,-.8),.57,.52,.068,'y','silver',32)
 cyl(k,(-.67,2.86,-.8),.26,.56,'silver','y',30)
with moving('ANIM_CARRIER_ROTATION'):
 axial(k,(-.67,2.37,-.8),.91,.25,'silver',48)
 axial(k,(-.67,2.16,-.8),.79,.16,'dark',48)
 ring(k,(-.67,2.25,-.8),.91,.84,.065,'y','gold',48)
 bolt_circle(k,(-.67,2.53,-.8),.69,16,'silver')
 for r in (.23,.49,.68):ring(k,(-.67,2.16,-.8),r,r-.03,.015,'y','copper',40)
for i in range(4):
 p=(2.75,3.9-i*.1,-.62)
 pipe3(k,[p,(1.55,3.95-i*.13,-.58),(-.45,3.78-i*.14,-.62)],.026,'copper' if i%2 else 'cyan')

# 06 — gimbal pivot, bearings, machined collars and spherical compliance frame.
k=names[6]
register(6,'ANIM_CARRIER_GIMBAL',(-.67,2.86,-.80),'angular','z')
with moving('ANIM_CARRIER_GIMBAL'):
 torus_ring_segments(k,(-.67,2.93,-.8),.64,.077,'silver','y',64)
 torus_ring_segments(k,(-.67,2.93,-.8),.52,.057,'steel','z',50)
 for a in np.linspace(0,2*math.pi,8,endpoint=False):
  px=-.67+.61*math.cos(a);pz=-.8+.61*math.sin(a)
  cyl(k,(px,2.93,pz),.075,.12,'copper','y',14)
  screw(k,(px,3.02,pz),.055)
 axial(k,(-.67,2.87,-.8),.23,.26,'gold',24)

# 07 — three annular pressure bladders, carrier air channels, status LEDs.
k=names[7]
register(7,'ANIM_PRESSURE_ZONE_INDICATOR',(-.67,2.03,-.80))
with moving('ANIM_PRESSURE_ZONE_INDICATOR'):
 for r,w,col in [(0.25,.19,'blue'),(.51,.18,'cyan'),(.75,.14,'green')]:
  ring(k,(-.67,2.08,-.80),r,r-w,.085,'y',col,48)
  for a in np.linspace(0,2*math.pi,12,endpoint=False):
   xx=-.67+(r-w*.48)*math.cos(a);zz=-.8+(r-w*.48)*math.sin(a)
   cyl(k,(xx,2.135,zz),.018,.018,'silver','y',8)
 for a in np.linspace(0,2*math.pi,6,endpoint=False):
  xx=-.67+.87*math.cos(a);zz=-.8+.87*math.sin(a)
  box(k,(xx,2.02,zz),(.085,.115,.075),'steel')

# 08 — wafer retaining ring, abrasion-resistant rim, inspection screws.
k=names[8]
register(8,'ANIM_RETAINING_RING',(-.67,1.99,-.80))
with moving('ANIM_RETAINING_RING'):
 ring(k,(-.67,1.945,-.8),.85,.71,.15,'y','ceramic',64)
 ring(k,(-.67,1.855,-.8),.85,.83,.04,'y','copper',64)
 ring(k,(-.67,2.032,-.8),.79,.74,.038,'y','silver',64)
 bolt_circle(k,(-.67,2.042,-.8),.76,18,'dark')

# 09 — actual wafer mesh under carrier, patterned radial wafer inspection marks.
k=names[9]
axial(k,(-.67,1.84,-.8),.71,.042,'iridescent',64)
ring(k,(-.67,1.81,-.8),.704,.680,.012,'y','silver',64)
for rr in (.18,.41,.59):ring(k,(-.67,1.814,-.8),rr,rr-.007,.006,'y','cyan',64)
for u in np.linspace(-.52,.52,7):
 span=math.sqrt(max(0,.68**2-u*u))*.94
 box(k,(-.67+u,1.81,-.8),(.005,.005,2*span),'light')
 box(k,(-.67,1.81,-.8+u),(2*span,.005,.005),'light')

# 10 — patterned thin film with trench-line regions and microchip metallization features.
k=names[10]
register(10,'ANIM_FILM_THICKNESS',(-.67,1.797,-.80),'shader','y')
with moving('ANIM_FILM_THICKNESS'):
 cyl(k,(-.67,1.798,-.8),.69,.008,'gold','y',64)
 for rr in (.19,.40,.59):ring(k,(-.67,1.786,-.8),rr,rr-.014,.006,'y','copper',48)
 for i in range(12):
  a=2*math.pi*i/12
  x=-.67+.38*math.cos(a);z=-.8+.38*math.sin(a)
  box(k,(x,1.785,z),(.05,.012,.09),'blue')
 for a in range(14):
  xx=-1.24+a*.083
  for b in range(3):
   zz=-.95+b*.135
   if (xx+.67)**2+(zz+.8)**2<.62**2:box(k,(xx,1.782,zz),(.034,.010,.075),'copper')

# 11 — holographic SEM-like wafer topography map, nonflat 24x18 mesh of cell heights.
k=names[11]
register(11,'ANIM_TOPOGRAPHY_DISPLAY',(0.45,3.39,2.70),'shader','z')
with moving('ANIM_TOPOGRAPHY_DISPLAY'):
 box(k,(.45,3.39,2.70),(2.98,1.55,.10),'navy')
 box(k,(.45,3.39,2.764),(2.83,1.42,.021),'black')
 for a in range(23):
  for b in range(12):
   xx=-.82+a*.116;yy=2.90+b*.087
   peak=(math.sin(a*.75)*math.cos(b*.59)+.5*math.sin(a*.29+b*.82))*.075
   col='red' if peak>.065 else ('amber' if peak>.025 else ('green' if peak<-.03 else 'cyan'))
   box(k,(xx,yy,2.782),(.100,.073,.012),col)
 for xx in (-.97,1.86):lightstrip(k,(xx,3.39,2.79),(.022,1.44,.026),'cyan')
 for y in (2.63,4.14):lightstrip(k,(.45,y,2.79),(2.87,.023,.024),'cyan')

# 12 — carrier cassette with 13 individual wafer slots and support frame.
k=names[12]
mechbox(k,(-6.75,1.20,-.50),(1.45,1.10,1.55),'steel')
for i in range(13):
 y=.83+i*.055
 box(k,(-6.75,y,-.50),(1.20,.017,1.30),'iridescent')
 box(k,(-6.75,y+.016,-1.19),(1.15,.012,.033),'cyan' if i%3 else 'gold')
for x in (-7.44,-6.06):
 for z in (-1.23,.23):box(k,(x,1.20,z),(.10,1.29,.11),'silver')
box(k,(-6.75,1.82,-.50),(1.50,.12,1.66),'dark')

# 13 — articulated wafer load robot, joints, gripper and cabling.
k=names[13]
register(13,'ANIM_WAFER_ROBOT_BASE',(-5.10,1.15,-.61))
register(13,'ANIM_WAFER_ROBOT_ARM',(-4.69,1.90,-.61),'angular','z')
register(13,'ANIM_WAFER_ROBOT_TOOL',(-3.69,2.02,-.49),'angular','z')
mechbox(k,(-5.1,.86,-.61),(1.35,.40,1.20),'navy')
with moving('ANIM_WAFER_ROBOT_BASE'):
 axial(k,(-5.10,1.16,-.61),.33,.20,'steel',32)
 sscylinder(k,(-5.10,1.52,-.61),.22,.48,'y',9)
with moving('ANIM_WAFER_ROBOT_ARM'):
 angular_arm(k,(-5.00,1.88,-.61),(-4.00,2.04,-.52),.135,'steel')
 cyl(k,(-4.05,2.04,-.52),.20,.25,'copper','y',20)
with moving('ANIM_WAFER_ROBOT_TOOL'):
 angular_arm(k,(-4.02,2.06,-.52),(-3.36,1.99,-.52),.067,'silver')
 for zz in (-.73,-.31):box(k,(-3.28,1.98,zz),(.56,.048,.078),'cyan')
 box(k,(-3.18,1.98,-.52),(.22,.05,.44),'dark')
wirebundle(k,[(-5.25,1.18,-.90),(-5.55,1.75,-1.02),(-4.69,2.10,-.88),(-3.63,2.15,-.72)],4,.025,'copper',.012)

# 14 — wafer lift alignment cradle, 4-point centering pegs and fiducial sensors.
k=names[14]
register(14,'ANIM_WAFER_LIFT',(-4.02,1.25,1.23),'linear','y')
mechbox(k,(-4.02,.86,1.23),(1.42,.35,1.43),'navy')
with moving('ANIM_WAFER_LIFT'):
 cyl(k,(-4.02,1.19,1.23),.56,.11,'steel','y',36)
 for a in np.linspace(0,2*math.pi,4,endpoint=False):
  x=-4.02+.55*math.cos(a);z=1.23+.55*math.sin(a)
  cyl(k,(x,1.32,z),.036,.27,'silver','y',12)
  cyl(k,(x,1.49,z),.060,.045,'cyan','y',12)
 ring(k,(-4.02,1.23,1.23),.64,.61,.050,'y','gold',40)
for a in np.linspace(0,2*math.pi,8,endpoint=False):
 x=-4.02+.68*math.cos(a);z=1.23+.68*math.sin(a)
 cyl(k,(x,.95,z),.030,.07,'amber','y',10)

# 15 — separate virtual slurry supply cabinet, metering controls, valves and warning accents.
k=names[15]
hd_cabinet(k,(-6.40,1.36,2.66),(1.54,1.63,.91),'cyan',2)
for xx in (-6.89,-6.42,-5.95):
 cyl(k,(xx,2.29,2.55),.15,.18,'silver','y',24)
 cyl(k,(xx,2.37,2.55),.07,.038,'copper','y',16)
for i in range(13):
 box(k,(-7.13+i*.11,.74,3.18),(.065,.030,.023),'cyan' if i%3 else 'amber')
terminal_block(k,(-6.40,.62,3.20),9,'x','cyan')

# 16 — deep transparent slurry mixing tank with stirrer, tank bands and visible liquid layer.
k=names[16]
register(16,'ANIM_SLURRY_MIXER',(-7.72,1.63,1.93))
cyl(k,(-7.72,1.43,1.93),.44,1.30,'glass','y',40)
for y in (.88,1.38,1.93):precision_ring(k,(-7.72,y,1.93),.46,.43,.072,'y','steel')
cyl(k,(-7.72,1.17,1.93),.40,.66,'slurry','y',40)
cyl(k,(-7.72,2.17,1.93),.41,.12,'steel','y',32)
with moving('ANIM_SLURRY_MIXER'):
 cyl(k,(-7.72,1.54,1.93),.04,1.19,'silver','y',14)
 for h in (1.17,1.42,1.67):
  for a in np.linspace(0,2*math.pi,5,endpoint=False):
   xx=-7.72+.21*math.cos(a);zz=1.93+.21*math.sin(a)
   angular_arm(k,(-7.72,h,1.93),(xx,h,zz),.033,'copper')
for h in (.90,1.20,1.50,1.80):
 box(k,(-7.24,h,1.93),(.02,.04,.40),'cyan' if h<1.50 else 'blue')
pipe3(k,[(-7.72,2.28,1.93),(-7.72,2.50,1.93),(-6.56,2.50,1.93)],.05,'steel')

# 17 — precision positive displacement slurry metering pump.
k=names[17]
register(17,'ANIM_SLURRY_PUMP',(-5.64,.94,2.03))
mechbox(k,(-5.64,.72,2.03),(1.14,.22,.75),'dark')
with moving('ANIM_SLURRY_PUMP'):
 sscylinder(k,(-5.64,.99,2.03),.24,.51,'x',10)
 cyl(k,(-5.24,.99,2.03),.11,.19,'copper','x',20)
for dy in (.83,1.0,1.17):
 pipe3(k,[(-6.29,dy,2.03),(-6.49,dy,2.03)],.045,'cyan')
pipe3(k,[(-5.01,1.0,2.03),(-4.71,1.0,2.03),(-4.71,2.11,1.4)],.040,'copper')
box(k,(-5.66,1.31,2.03),(.61,.13,.46),'blue')

# 18 — slurry flow transmitter with visible flow window and connectors.
k=names[18]
sensor(k,(-4.73,1.56,2.04),'cyan',False)
for y in (1.14,1.26,1.38):ring(k,(-4.73,y,2.04),.12,.09,.035,'y','light',18)
cyl(k,(-4.73,1.29,2.04),.093,.33,'glass','y',18)
pipe3(k,[(-4.73,1.07,2.04),(-4.73,.87,2.04)],.028,'steel')

# 19 — cantilevered slurry delivery arm from side support to nozzle.
k=names[19]
register(19,'ANIM_SLURRY_ARM',(-3.35,2.32,.62),'angular','y')
mechbox(k,(-3.36,1.37,.62),(.72,1.49,.75),'navy')
axial(k,(-3.36,2.18,.62),.32,.16,'silver',30)
with moving('ANIM_SLURRY_ARM'):
 angular_arm(k,(-3.36,2.35,.62),(-2.23,2.56,.45),.14,'steel')
 angular_arm(k,(-2.23,2.56,.45),(-1.31,2.42,.25),.10,'silver')
 for q in [(-3.36,2.35,.62),(-2.23,2.56,.45),(-1.31,2.42,.25)]:cyl(k,q,.13,.13,'copper','y',24)
 pipe3(k,[(-3.36,2.68,.62),(-2.29,2.75,.45),(-1.35,2.65,.25)],.044,'cyan')
wirebundle(k,[(-3.63,1.98,.48),(-3.50,2.7,.48),(-2.45,2.87,.38),(-1.34,2.68,.31)],4,.022,'dark',.014)

# 20 — nozzle tip and atomized slurry bead accents (static mesh, not full fluid sim).
k=names[20]
register(20,'ANIM_SLURRY_NOZZLE',(-1.27,2.06,.28),'linear','y')
with moving('ANIM_SLURRY_NOZZLE'):
 cyl(k,(-1.27,2.17,.28),.16,.31,'silver','y',22)
 ring(k,(-1.27,2.06,.28),.16,.095,.06,'y','copper',24)
 cyl(k,(-1.27,1.97,.28),.08,.22,'steel','y',20)
 for a in np.linspace(0,2*math.pi,8,endpoint=False):
  xx=-1.27+.07*math.cos(a);zz=.28+.07*math.sin(a)
  cyl(k,(xx,1.81,zz),.019,.065,'slurry','y',10)
for i in range(7):
 x=-1.30 + (i-3)*.045
 cyl(k,(x,1.66,.32+0.02*math.sin(i)),.019,.026,'cyan','y',10)

# 21/22 — slurry condition sensors with ports and actual 3D readouts.
for idx,pos,cc in [(21,(-5.18,1.65,3.33),'violet'),(22,(-4.19,1.65,3.33),'amber')]:
 k=names[idx];sensor(k,pos,cc)
 x,y,z=pos
 for i in range(7):box(k,(x-.21+i*.068,y+.30,z+.13),(.042,.026,.034),'silver')
 pipe3(k,[(x,y-.55,z),(x,y-.68,z+.36)],.035,'copper')

# 23 — articulated conditioning arm: radial swing + force-adjusting cantilever.
k=names[23]
register(23,'ANIM_CONDITIONER_ARM',(3.31,2.54,-.25),'angular','y')
mechbox(k,(3.31,1.47,-.25),(.87,1.25,.92),'navy')
cyl(k,(3.31,2.28,-.25),.34,.22,'silver','y',32)
with moving('ANIM_CONDITIONER_ARM'):
 angular_arm(k,(3.31,2.58,-.25),(2.70,2.71,-.06),.18,'steel')
 angular_arm(k,(2.70,2.71,-.06),(1.83,2.57,.24),.14,'silver')
 cyl(k,(1.83,2.41,.24),.19,.39,'copper','y',24)
 for p in [(3.31,2.60,-.25),(2.70,2.71,-.06)]:
  cyl(k,p,.20,.14,'dark','y',22)
  ring(k,(p[0],p[1]+.07,p[2]),.21,.18,.038,'y','silver',22)
pipe3(k,[(3.48,1.48,-.25),(3.82,2.71,-.45),(3.1,2.87,-.25),(1.95,2.83,.18)],.045,'cyan')

# 24 — conditioning disc with abrasive radial teeth and bolted bearing.
k=names[24]
register(24,'ANIM_CONDITIONER_DISC',(1.85,1.79,.23))
with moving('ANIM_CONDITIONER_DISC'):
 axial(k,(1.85,1.79,.23),.49,.115,'rubber',48)
 ring(k,(1.85,1.75,.23),.48,.42,.06,'y','copper',48)
 axial(k,(1.85,1.91,.23),.26,.12,'steel',32)
 for rr in (.16,.31,.43):ring(k,(1.85,1.72,.23),rr,rr-.018,.025,'y','granite',48)
 for i in range(24):
  a=2*math.pi*i/24
  xx=1.85+.41*math.cos(a);zz=.23+.41*math.sin(a)
  box(k,(xx,1.70,zz),(.055,.037,.055),'ceramic')
 bolt_circle(k,(1.85,2.02,.23),.21,12,'dark')

# 25 — conditioning drive, torque arm and motor cooling fins.
k=names[25]
register(25,'ANIM_CONDITIONER_MOTOR',(3.40,2.06,-.28))
mechbox(k,(3.39,1.85,-.28),(.82,.36,.83),'dark')
with moving('ANIM_CONDITIONER_MOTOR'):
 sscylinder(k,(3.40,2.17,-.28),.23,.56,'y',10)
 cyl(k,(3.40,2.50,-.28),.11,.17,'gold','y',18)
for i in range(15):
 xx=3.08+i*.046
 box(k,(xx,2.23,.02),(.017,.38,.18),'silver')
for i in range(5):cyl(k,(3.12+i*.11,1.75,.19),.026,.018,'cyan','z',10)

# 26 — pad wear sensor with linear optical probe and depth markings.
k=names[26]
register(26,'ANIM_PAD_WEAR_PROBE',(2.55,2.01,-1.83),'linear','y')
stand(k,2.57,.80,-1.90,1.26,'steel')
with moving('ANIM_PAD_WEAR_PROBE'):
 sensor(k,(2.55,2.12,-1.83),'green',True)
 cyl(k,(2.55,1.81,-1.83),.035,.40,'silver','y',12)
for i in range(9):box(k,(2.69,1.71+i*.08,-1.79),(.08,.015,.025),'amber' if i%3==0 else 'silver')

# 27 — groove inspection scanner with laser line and U-shaped scanning rail.
k=names[27]
register(27,'ANIM_PAD_SCANNER',(2.68,2.66,.84),'linear','x')
mechbox(k,(2.84,2.42,.93),(.69,.55,.71),'dark')
with moving('ANIM_PAD_SCANNER'):
 box(k,(2.67,2.70,.80),(.16,.17,1.02),'steel')
 box(k,(2.65,2.59,.80),(.12,.09,.68),'blue')
 for i in range(13):
  z=.46+i*.052
  box(k,(2.65,2.52,z),(.04,.015,.02),'cyan')
box(k,(2.92,2.82,.80),(.22,.06,1.25),'silver')

# 28 — primary downforce load-cell in the carrier stack, precision toroidal housing.
k=names[28]
register(28,'ANIM_DOWNFORCE_INDICATOR',(-.67,3.36,-.8),'linear','y')
cyl(k,(-.67,3.31,-.8),.41,.28,'steel','y',32)
ring(k,(-.67,3.47,-.8),.42,.31,.055,'y','copper',32)
with moving('ANIM_DOWNFORCE_INDICATOR'):
 axial(k,(-.67,3.52,-.8),.23,.07,'gold',24)
for a in np.linspace(0,2*math.pi,8,endpoint=False):
 xx=-.67+.34*math.cos(a);zz=-.8+.34*math.sin(a)
 cyl(k,(xx,3.47,zz),.034,.080,'silver','y',12)
pipe3(k,[(-.88,3.43,-.57),(-1.23,3.61,-.52),(-1.57,3.62,.20)],.029,'copper')

# 29 — electro-pneumatic pressure regulator cabinet with four output ports.
k=names[29]
zpanel(k,(3.18,.87,3.08),1.15,.83,.70,'cyan',4)
for i in range(4):
 x=2.72+i*.29
 cyl(k,(x,1.35,3.08),.07,.12,'silver','y',14)
 pipe3(k,[(x,1.45,3.08),(x,1.67,3.0),(x-.28,1.71,2.8)],.028,'copper')
gauge(k,(3.18,.88,3.50),.15)

# 30 platen speed encoder with disk and optical fork.
k=names[30]
register(30,'ANIM_PLATEN_ENCODER_DISC',(2.38,1.05,-2.05))
mechbox(k,(2.48,.91,-2.12),(.52,.38,.58),'steel')
with moving('ANIM_PLATEN_ENCODER_DISC'):
 axial(k,(2.38,1.07,-2.05),.25,.034,'dark',32)
 for i in range(24):
  a=2*math.pi*i/24
  cyl(k,(2.38+.21*math.cos(a),1.102,-2.05+.21*math.sin(a)),.012,.014,'cyan','y',8)
box(k,(2.38,1.19,-1.91),(.12,.08,.23),'blue')
pipe3(k,[(2.55,.91,-1.9),(3.0,.93,-1.75)],.022,'copper')

# 31 carrier encoder, mounted high on carrier head with indexed optical wheel.
k=names[31]
register(31,'ANIM_CARRIER_ENCODER_DISC',(-.67,3.74,-.8))
mechbox(k,(-.67,3.71,-.8),(.66,.16,.60),'dark')
with moving('ANIM_CARRIER_ENCODER_DISC'):
 axial(k,(-.67,3.82,-.8),.27,.028,'silver',32)
 for i in range(24):
  a=2*math.pi*i/24
  box(k,(-.67+.23*math.cos(a),3.85,-.8+.23*math.sin(a)),(.035,.013,.055),'black')
box(k,(-.31,3.81,-.8),(.12,.16,.22),'blue')

# 32 — friction torque/current analyzer with heat-sink lattice and chart.
k=names[32]
zpanel(k,(4.43,.88,3.01),1.20,.90,.74,'amber',4)
finbank(k,(4.43,1.39,3.01),1.06,17,.32,.21,'light')
for x in (4.1,4.4,4.7):gauge(k,(x,1.01,3.42),.085)

# 33 — 3-axis accelerometer and rugged mount to polishing machine.
k=names[33]
sensor(k,(1.95,1.61,-2.81),'amber',True)
for xx,yy,zz,cc in [(2.22,1.50,-2.81,'red'),(1.95,1.89,-2.81,'green'),(1.95,1.61,-2.49,'blue')]:
 pipe3(k,[(1.95,1.61,-2.81),(xx,yy,zz)],.014,cc)

# 34 — acoustic-emission sensor, disk pickup and instrument body.
k=names[34]
sensor(k,(-2.53,1.60,-2.54),'violet',True)
cyl(k,(-2.60,1.30,-2.54),.17,.10,'silver','y',22)
ring(k,(-2.60,1.37,-2.54),.17,.12,.02,'y','copper',22)

# 35 — interface temperature probe with slender stainless mount.
k=names[35]
sensor(k,(-.29,1.79,-3.00),'amber',True)
pipe3(k,[(-.29,1.45,-3),(-.29,1.26,-2.78),(-.29,1.52,-2.30)],.028,'copper')

# 36 — dual-line industrial polishing cooling loop with radiators and flow indicator.
k=names[36]
mechbox(k,(4.27,.81,-2.48),(1.46,.38,1.05),'navy')
with moving('ANIM_COOLING_FAN'):
 fan(k,(4.27,1.23,-2.48),.31,'y')
register(36,'ANIM_COOLING_FAN',(4.27,1.23,-2.48))
finbank(k,(4.27,.90,-1.91),1.10,23,.17,.47,'steel')
pipe3(k,[(3.62,1.02,-2.45),(3.17,1.57,-2.40),(2.88,1.60,-1.87)],.054,'cyan')
pipe3(k,[(3.75,.88,-2.53),(3.17,.94,-2.90),(2.90,1.25,-2.80)],.055,'copper')

# 37 — transparent slurry containment ring: annular glass shield plus edge braces.
k=names[37]
ring(k,(0,1.60,-.55),2.85,2.81,.66,'y','glass',72)
ring(k,(0,1.99,-.55),2.91,2.81,.055,'y','silver',64)
ring(k,(0,1.24,-.55),2.91,2.82,.060,'y','steel',64)
for i in range(16):
 a=2*math.pi*i/16
 xx=2.85*math.cos(a);zz=-.55+2.85*math.sin(a)
 box(k,(xx,1.62,zz),(.044,.66,.048),'silver')
 for yy in (1.27,1.95):screw(k,(xx,yy,zz),.034,'y')

# 38 — waste slurry collection module, drain funnel, sediment chamber and gauges.
k=names[38]
mechbox(k,(4.43,.74,-1.05),(1.42,.31,1.04),'dark')
cyl(k,(4.43,1.30,-1.05),.42,.84,'steel','y',32)
cyl(k,(4.43,1.19,-1.05),.37,.53,'glass','y',32)
cyl(k,(4.43,1.08,-1.05),.34,.29,'slurry','y',32)
precision_ring(k,(4.43,1.75,-1.05),.42,.35,.067,'y','light')
pipe3(k,[(2.35,1.34,-1.22),(3.24,1.01,-1.12),(4.06,1.43,-1.05)],.071,'steel')
pipe3(k,[(4.80,1.11,-1.05),(5.05,.87,-1.05)],.040,'copper')
for xx in (4.14,4.72):gauge(k,(xx,.80,-.50),.11)

# 39 — emergency shutdown station with latched E-stop mushroom.
k=names[39]
register(39,'ANIM_ESTOP',(7.59,1.03,3.75),'linear','z')
zpanel(k,(7.56,.96,3.38),.94,.95,.75,'red',2)
with moving('ANIM_ESTOP'):
 cyl(k,(7.56,.85,3.88),.22,.16,'red','z',32)
 ring(k,(7.56,.85,3.99),.25,.20,.036,'z','amber',32)
for x in (7.29,7.76):
 cyl(k,(x,1.46,3.42),.085,.13,'amber' if x<7.5 else 'green','y',18)

# 40 — post-CMP rinse enclosure on the right with liquid capture, inspection doors.
k=names[40]
frame(k,(6.0,1.88,-.72),(2.40,2.32,2.15),beam=.082,glass=True)
box(k,(6.0,.77,-.72),(2.45,.16,2.23),'steel')
axial(k,(6.0,1.00,-.72),.65,.11,'silver',36)
for x in (5.20,6.80):
 box(k,(x,1.91,.38),(.10,2.08,.12),'silver')
for i in range(12):
 a=2*math.pi*i/12
 xx=6+.65*math.cos(a);zz=-.72+.65*math.sin(a)
 cyl(k,(xx,.99,zz),.017,.036,'cyan','y',10)
for x in (5.32,6.68):
 pipe3(k,[(x,2.80,-1.48),(x,2.45,-1.48),(x,2.30,-.84)],.035,'cyan')

# 41 — two rotating cleaning brushes with filament geometry and robotic swing bracket.
k=names[41]
register(41,'ANIM_CLEANING_BRUSH',(5.92,1.39,-.70))
mechbox(k,(6.0,1.17,-.72),(1.62,.20,1.32),'dark')
with moving('ANIM_CLEANING_BRUSH'):
 for x in (5.55,6.45):
  cyl(k,(x,1.39,-.72),.35,.11,'rubber','y',28)
  for i in range(28):
   a=2*math.pi*i/28
   xx=x+.31*math.cos(a);zz=-.72+.31*math.sin(a)
   box(k,(xx,1.51,zz),(.045,.23,.028),'white')
  for rr in (.12,.23,.31):ring(k,(x,1.50,-.72),rr,rr-.015,.019,'y','cyan',32)
box(k,(6.0,1.82,-1.49),(1.72,.15,.15),'steel')
for x in (5.55,6.45):box(k,(x,1.66,-1.49),(.12,.39,.15),'silver')

# 42 — spin rinse/dryer with nested bowls, shaft and gas nozzle manifold.
k=names[42]
register(42,'ANIM_SPIN_DRYER',(7.13,1.27,.47))
mechbox(k,(7.13,.78,.47),(1.85,.34,1.92),'navy')
cyl(k,(7.13,1.12,.47),.71,.29,'steel','y',48)
with moving('ANIM_SPIN_DRYER'):
 axial(k,(7.13,1.36,.47),.58,.08,'iridescent',40)
 ring(k,(7.13,1.34,.47),.63,.59,.075,'y','silver',40)
 bolt_circle(k,(7.13,1.41,.47),.46,12,'dark')
for i in range(6):
 a=2*math.pi*i/6
 x=7.13+.79*math.cos(a);z=.47+.79*math.sin(a)
 box(k,(x,1.34,z),(.07,.12,.07),'silver')
pipe3(k,[(7.85,2.13,.47),(7.85,1.80,.47),(7.35,1.62,.47)],.045,'steel')

# 43 — optical endpoint detector and separate cyan aperture window.
k=names[43]
register(43,'ANIM_ENDPOINT_SENSOR',(2.68,2.63,-2.10),'angular','z')
stand(k,2.85,1.18,-2.21,1.25)
with moving('ANIM_ENDPOINT_SENSOR'):
 sensor(k,(2.68,2.53,-2.10),'cyan',True)
 cyl(k,(2.68,2.20,-2.10),.12,.22,'dark','y',24)
 cyl(k,(2.68,2.08,-2.10),.075,.052,'cyan','y',18)
pipe3(k,[(2.68,2.53,-2.32),(2.93,2.77,-2.49),(3.39,2.78,-2.50)],.019,'blue')

# 44 — eddy-current sensor with copper toroidal coil loops and instrument head.
k=names[44]
register(44,'ANIM_EDDY_CURRENT_HEAD',(3.28,2.47,.68),'linear','y')
stand(k,3.37,1.25,.68,1.05)
with moving('ANIM_EDDY_CURRENT_HEAD'):
 cyl(k,(3.28,2.53,.68),.25,.22,'steel','y',32)
 for r in (.13,.17,.22):torus_ring_segments(k,(3.28,2.40,.68),r,.021,'copper','y',48)
 cyl(k,(3.28,2.36,.68),.10,.09,'blue','y',18)
for yy in (1.37,1.56,1.75):lightstrip(k,(3.63,yy,.68),(.018,.025,.25),'cyan')

# 45 — optical profilometer gantry with moving optical head and surface scan guides.
k=names[45]
register(45,'ANIM_PROFILOMETER',(5.72,2.87,1.70),'linear','x')
for x in (4.93,6.52):
 box(k,(x,2.15,1.70),(.17,1.87,.17),'steel')
 box(k,(x,1.15,1.70),(.36,.14,.40),'dark')
box(k,(5.72,3.14,1.70),(1.92,.23,.35),'silver')
with moving('ANIM_PROFILOMETER'):
 box(k,(5.72,2.81,1.70),(.40,.57,.42),'navy')
 cyl(k,(5.72,2.46,1.70),.14,.25,'silver','y',24)
 cyl(k,(5.72,2.29,1.70),.075,.13,'cyan','y',18)
 for i in range(9):box(k,(5.42+i*.075,2.79,1.94),(.039,.031,.023),'blue')
box(k,(5.72,1.15,1.70),(1.80,.12,1.30),'dark')

# 46 — multi-camera defect station; camera body, telescopic tube and ring LEDs.
k=names[46]
register(46,'ANIM_DEFECT_CAMERA',(7.36,2.34,1.72),'angular','x')
stand(k,7.37,1.17,1.72,.97)
with moving('ANIM_DEFECT_CAMERA'):
 mechbox(k,(7.36,2.38,1.72),(.51,.35,.48),'navy')
 cyl(k,(7.36,2.12,1.72),.17,.31,'steel','y',22)
 ring(k,(7.36,1.94,1.72),.24,.17,.05,'y','cyan',28)
 for i in range(12):
  a=2*math.pi*i/12
  xx=7.36+.20*math.cos(a);zz=1.72+.20*math.sin(a)
  cyl(k,(xx,1.91,zz),.018,.031,'white','y',10)

# 47 — dishing/erosion analyzer with mini heatmap, wafer reference and sockets.
k=names[47]
zpanel(k,(5.78,.86,3.20),1.35,.96,.72,'gold',4)
for i in range(7):
 for j in range(3):
  xx=5.38+i*.13;yy=1.02+j*.085
  box(k,(xx,yy,3.59),(.09,.065,.021),['green','cyan','amber','red'][int((i*3+j)%4)])

# 48 — multi-zone pressure PID unit with 4 actual regulator knob parts.
k=names[48]
zpanel(k,(3.26,.84,3.21),1.29,.95,.73,'blue',4)
for i in range(4):
 x=2.82+i*.29
 gauge(k,(x,1.06,3.61),.10)

# 49 — platen/carrier coordinated motion controller with chart screen.
k=names[49]
zpanel(k,(1.76,.84,3.20),1.35,.95,.73,'cyan',5)
for xx in (1.35,2.13):cyl(k,(xx,1.31,3.20),.09,.16,'copper','y',16)

# 50 — PLC rack, modular CPU/I/O bays, visible channel LEDs and terminal blocks.
k=names[50]
hd_cabinet(k,(-4.82,1.71,-3.45),(2.45,2.16,.74),'blue',2)
for j in range(10):
 x=-5.93+j*.245
 box(k,(x,1.64,-3.02),(.225,1.48,.085),'steel' if j%3 else 'blue')
 for i in range(7):
  cyl(k,(x,2.22-i*.14,-2.965),.019,.016,'green' if (j+i)%5 else 'amber','z',8)
 for i in range(5):
  box(k,(x-.073+i*.036,.96,-2.968),(.026,.06,.042),'black')

# 51 — Digital I/O module, guarded 5V signal terminals and surface status.
k=names[51]
zpanel(k,(-6.90,1.05,-3.21),1.16,1.05,.73,'green',4)
terminal_block(k,(-6.90,.57,-2.81),8,'x','green')
for i in range(2):
 for j in range(7):cyl(k,(-7.25+j*.12,1.45-i*.12,-2.80),.018,.015,'cyan' if i==0 else 'amber','z',8)

# 52 — Large live CMP SCADA monitor, 3D rounded frame, LED charts and support.
k=names[52]
register(52,'ANIM_CMP_SCADA',(5.65,3.60,-3.70),'angular','x')
with moving('ANIM_CMP_SCADA'):
 giantdisplay(k,(5.65,3.60,-3.70),4.26,2.03)
 box(k,(5.65,3.62,-3.59),(3.96,1.79,.016),'black')
 for i in range(17):
  x=3.79+i*.22
  for j in range(6):
   y=3.83+j*.18
   col='green' if ((i+j)%9<6) else 'amber'
   box(k,(x,y,-3.576),(.19,.151,.010),col)
 for i in range(20):
  x=3.90+i*.19
  y=3.05+.12*math.sin(i*.31)+.10*math.sin(i*.84)
  box(k,(x,y,-3.569),(.062,.027,.013),'cyan')
 for x in (3.61,7.70):lightstrip(k,(x,3.60,-3.58),(.035,1.89,.034),'cyan')
 for y in (2.58,4.58):lightstrip(k,(5.65,y,-3.58),(4.19,.030,.034),'cyan')
for x in (3.88,7.42):
 box(k,(x,2.11,-3.80),(.20,1.0,.28),'steel')
 box(k,(x,1.56,-3.80),(.59,.12,.53),'dark')

# 53 — self-contained educational 5V control supply (not enough power for big motor loads).
k=names[53]
zpanel(k,(7.46,.94,-3.13),.94,.83,.69,'green',2)
for x in (7.15,7.47,7.77):
 cyl(k,(x,.54,-2.76),.074,.13,'copper','z',16)
 cyl(k,(x,.54,-2.65),.040,.045,'red' if x<7.30 else 'blue','z',10)
finbank(k,(7.46,1.40,-3.13),.73,12,.46,.18,'light')

# Geometry/pivot validation before exports.
for idx,n in enumerate(names):
 assert n in M and M[n],(idx,n,'missing model')
 for mesh,color,grp in M[n]:
  assert len(mesh.faces)>0 and np.isfinite(mesh.vertices).all(),(idx,n,'bad mesh')
  assert grp is None or grp in PV.get(n,{}),(idx,n,'unregistered pivot',grp)
print('MODELED',len(names),'individual equipment models;',sum(len(M[n]) for n in names),'mesh parts,',sum(sum(len(mesh.faces) for mesh,c,p in M[n]) for n in names),'triangles',flush=True)

# Minimal binary FBX 7400 node writer, preserving mesh material regions and pivot parents.
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

def export_fbx(key,path):
    meshes=M[key];objects=[];connections=[];uid=500000
    def getid():
        nonlocal uid
        uid+=1;return uid
    root_id=getid()
    def props70(x=0.,y=0.,z=0.):
        return N('Properties70',ch=[N('P','Lcl Translation','Lcl Translation','','A',float(x),float(y),float(z)),N('P','Lcl Rotation','Lcl Rotation','','A',0.,0.,0.),N('P','Lcl Scaling','Lcl Scaling','','A',1.,1.,1.)])
    objects.append(N('Model',root_id,key+'\x00\x01Model','Null',ch=[N('Version',232),props70(),N('Shading','Y'),N('Culling','CullingOff')]))
    connections.append(N('C','OO',root_id,0))
    groups={}
    for g in sorted({t for _,_,t in meshes if t is not None}):
        p=PV[key][g]['position'];pid=getid();groups[g]=pid
        objects.append(N('Model',pid,g+'\x00\x01Model','Null',ch=[N('Version',232),props70(*(float(v)*100 for v in p)),N('Shading','Y'),N('Culling','CullingOff')]))
        connections.append(N('C','OO',pid,root_id))
    mats={}
    for index,(mesh,color,grp) in enumerate(meshes):
        pivot=np.array(PV[key][grp]['position'],float) if grp else 0.0
        vv=np.asarray(mesh.vertices,dtype=np.float64)
        coords=(vv-pivot)*100.
        tris=np.asarray(mesh.faces,dtype=np.int32)
        indices=tris.copy();indices[:,-1]=-(indices[:,-1]+1)
        geo_id=getid();model_id=getid()
        geometry=N('Geometry',geo_id,f'{key}_Geometry_{index:04d}\x00\x01Geometry','Mesh',ch=[
            N('GeometryVersion',124),N('Vertices',coords.reshape(-1)),N('PolygonVertexIndex',indices.reshape(-1)),
            N('LayerElementMaterial',0,ch=[N('Version',101),N('Name',''),N('MappingInformationType','AllSame'),N('ReferenceInformationType','IndexToDirect'),N('Materials',np.array([0],dtype=np.int32))]),
            N('Layer',0,ch=[N('Version',100),N('LayerElement',ch=[N('Type','LayerElementMaterial'),N('TypedIndex',0)])])])
        objects.append(geometry)
        objects.append(N('Model',model_id,f'{key}_Part_{index:04d}\x00\x01Model','Mesh',ch=[N('Version',232),props70(),N('Shading','Y'),N('Culling','CullingOff')]))
        connections.extend([N('C','OO',geo_id,model_id),N('C','OO',model_id,groups.get(grp,root_id))])
        if color not in mats:
            matid=getid();rgb=[q/255.0 for q in P[color][:3]]
            objects.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(.25*q for q in rgb)),N('P','SpecularColor','Color','','A',.58,.62,.67),N('P','Shininess','Number','','A',54.),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
            mats[color]=matid
        connections.append(N('C','OO',mats[color],model_id))
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 73 CMP Modular Procedural Geometry')]),
         N('GlobalSettings',ch=[N('Version',1000),N('Properties70',ch=[N('P','UpAxis','int','Integer','',1),N('P','UpAxisSign','int','Integer','',1),N('P','FrontAxis','int','Integer','',2),N('P','FrontAxisSign','int','Integer','',1),N('P','CoordAxis','int','Integer','',0),N('P','CoordAxisSign','int','Integer','',1),N('P','UnitScaleFactor','double','Number','',1.),N('P','OriginalUnitScaleFactor','double','Number','',1.)])]),
         N('Definitions',ch=[N('Version',100),N('Count',len(objects))]),N('Objects',ch=objects),N('Connections',ch=connections)]
    blob=bytearray(b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400))
    for node in top:blob.extend(encnode(node,len(blob)))
    blob.extend(bytes(13));blob.extend(bytes(160))
    path.write_bytes(blob)

def export_glb(key,path):
    sc=trimesh.Scene()
    for g in sorted({t for _,_,t in M[key] if t is not None}):
        transform=np.eye(4);transform[:3,3]=PV[key][g]['position']
        sc.graph.update(frame_to=g,frame_from=sc.graph.base_frame,matrix=transform)
    for i,(mesh,col,grp) in enumerate(M[key]):
        m=mesh.copy()
        if grp:
            m.apply_translation(-np.asarray(PV[key][grp]['position'],float))
        sc.add_geometry(m,geom_name=f'{key}_Part_{i:04d}',node_name=f'{key}_Node_{i:04d}',parent_node_name=grp if grp else sc.graph.base_frame)
    path.write_bytes(sc.export(file_type='glb'))

# All equipment modules are independent; common-origin coordinates give alignment in Unity.
manifest=[]
for i,key in enumerate(names):
    file=f'{i:02d}_L73_{key}'
    export_fbx(key,MD/(file+'.fbx'))
    export_glb(key,GD/(file+'.glb'))
    triangles=sum(len(m.faces) for m,_,_ in M[key]);parts=len(M[key])
    verts=np.concatenate([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L73_'+key,file=file+'.fbx',glb=file+'.glb',triangle_count=triangles,
         mesh_parts=parts,bbox_min=np.around(verts.min(axis=0),4).tolist(),bbox_max=np.around(verts.max(axis=0),4).tolist(),
         rig_groups=list(PV.get(key,{}).keys()),animation_pivots={g:list(obj['position']) for g,obj in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/54] {key:43s} {triangles:7,d} tris {parts:4d} parts',flush=True)

combined='Level73_Complete_ArtKit_Showcase'
M[combined]=[];PV[combined]={}
for k in names:
    for g,obj in PV.get(k,{}).items():PV[combined][k+'__'+g]=obj
    for mesh,col,g in M[k]:M[combined].append((mesh,col,k+'__'+g if g else None))
print('Exporting assembled FBX/GLB...',flush=True)
export_fbx(combined,MD/(combined+'.fbx'))
export_glb(combined,GD/(combined+'.glb'))

with (ROOT/'Animation_Rigs.csv').open('w',newline='',encoding='utf-8') as f:
    writer=csv.writer(f);writer.writerow(['Model','Pivot','X','Y','Z','Kind','Axis','FBXHierarchy','Notes'])
    for i,key in enumerate(names):
        for pivot,info in PV.get(key,{}).items():
            writer.writerow(['L73_'+key,pivot,*info['position'],info['kind'],info['axis'],'Animated parent Null group present in FBX and GLB','Pivot is modeled hierarchy transform; no baked animation. Drive from actual Unity gameplay.'])
with (ROOT/'Art_Assembly.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','FBX','PositionX','PositionY','PositionZ','RotationX','RotationY','RotationZ','Scale','Notes'])
    for m in manifest:w.writerow([m['name'],m['file'],0,0,0,0,0,0,1,'Common-origin; verify FBX cm conversion'])
with (ROOT/'Component_Roles.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Index','Model','FBX','Triangles','MeshParts','PivotGroups'])
    for m in manifest:w.writerow([m['index'],m['name'],m['file'],m['triangle_count'],m['mesh_parts'],';'.join(m['rig_groups'])])

palette={}
for name,rgba in P.items():
    palette['PS_'+name]={'rgba_255':list(rgba),
      'metallic':.86 if name in ('steel','light','silver','copper','gold') else .18 if name in ('dark','black','navy','padgroove','pad') else .08,
      'smoothness':.88 if name in ('silver','steel','copper','gold','glass','iridescent') else .56,
      'transparent':rgba[3]<255,
      'suggested_emissive':name in ('cyan','blue','green','amber','violet','ice','red','gold')}
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_73','description':'Procedural colored material regions. No author-painted PBR textures; visual shader settings are suggestions.','materials':palette},indent=2))

wiring=[('5 V control supply and returns',10,'Control only; motors and pumps require separate power abstraction'),
('Platen and carrier motor controls',16,'RPM commands, encoder feedback, torque and motor status'),
('Downforce and multi-zone pressure',16,'Load cell, regulator, force feedback and zone commands'),
('Slurry flow and concentration',14,'Reservoir/pump commands, flow measurement, concentration and temperature'),
('Pad conditioning and wear',12,'Conditioner arm/disc and pad wear/scanner signals'),
('Endpoint sensors and metrology',12,'Optical and eddy-current measurement, defects, topography'),
('Wafer handling, cleaning and safety',10,'Robot/lift, rinse/dry, interlocks, emergency stop'),
('PLC and SCADA coordination',8,'Process status, supervisory command and certification')]
assert sum(x[1] for x in wiring)==98
with (ROOT/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Purpose','Status'])
    for row in wiring:w.writerow([*row,'PLANNED_NOT_IMPLEMENTED'])

ntri=sum(m['triangle_count'] for m in manifest)
nparts=sum(m['mesh_parts'] for m in manifest)
npiv=sum(len(p) for p in PV.values())-len(PV.get(combined,{}))
metadata={'level_id':'level_73','display_name':'Polish to Perfection','asset_kind':'Procedural HD / AAA-inspired, modular, actual 3D geometry; not artist-authored PBR textures',
'individual_models':54,'individual_glbs':54,'assembled_models':['Models/'+combined+'.fbx','GLB/'+combined+'.glb'],
'individual_triangle_count':ntri,'individual_mesh_parts':nparts,'animation_parent_pivot_groups':npiv,
'planned_player_wires':98,'planned_unique_functional_terminals':152,'actual_player_wires_included':False,'functional_spark_terminals_included':False,
'cmp_physics_included':False,'six_authoritative_objectives_implemented':False,
'fbx_units':'Numeric centimeters (UnitScaleFactor=1); Unity must import at meter-equivalent scale',
'glb_units':'numeric meters', 'axes':'X horizontal width, Y up, Z front',
'approx_workstation_width_m':17.15,'common_origin_for_all_individual_models':True,
'rig_hierarchy':'Animated movable subassemblies organized under explicitly named FBX/GLB parent Null nodes at reference pivot positions. No baked clips or runtime animation.',
'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(metadata,indent=2))
print('EXPORTED',len(manifest),'models /',ntri,'triangles /',nparts,'parts /',npiv,'pivot groups',flush=True)
