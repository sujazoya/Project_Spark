#!/usr/bin/env python3
"""Project Spark Level 82: original fault-tolerant mission computing and digital reliability modular geometry, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical or chiplet-bonding gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level82_HD_AAA')
MD=ROOT/'Models';GD=ROOT/'GLB';PD=ROOT/'Previews';ED=ROOT/'Unity_Optional';SRC=ROOT/'Source'
for d in (MD,GD,PD,ED,SRC): d.mkdir(parents=True,exist_ok=True)
P={'navy':(13,29,47,255),'steel':(71,101,124,255),'light':(150,179,195,255),'silver':(194,207,214,255),'dark':(29,48,63,255),'black':(12,22,32,255),'cyan':(39,215,246,255),'blue':(27,118,213,255),'ice':(166,232,249,255),'copper':(221,129,62,255),'amber':(252,179,68,255),'red':(238,72,79,255),'green':(50,224,149,255),'white':(229,240,245,255),'glass':(85,178,222,93),'frost':(205,229,244,255),'rubber':(45,48,52,255),'gold':(233,188,86,255),'violet':(166,92,242,255),'plasma':(174,90,242,160),'purpleglass':(150,91,230,115),'teal':(63,214,202,255),'ceramic':(224,222,211,255),'iridescent':(102,197,239,255),'warning':(255,112,67,255),'pad':(60,64,77,255),'padgroove':(26,35,47,255),'slurry':(68,204,238,157),'granite':(87,103,113,255),'uv':(192,120,255,150),'resist':(240,114,182,255),'warm':(255,194,101,255),'laser':(85,248,244,255),'circuit':(123,254,209,255)}
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






# Binary FBX 7400 and GLB exporter functions
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
            objects.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(.25*q for q in rgb)),N('P','SpecularColor','Color','','A',.60,.62,.67),N('P','Shininess','Number','','A',54.),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
            mats[color]=matid
        connections.append(N('C','OO',mats[color],model_id))
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 82 Fault Tolerant Computing Original Modular Geometry')]),
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






# -----------------------------------------------------------------------------
# LEVEL 82: SILENCE THE SPECTRUM — ORIGINAL EMC LABORATORY MODELING
# -----------------------------------------------------------------------------
P.update({'absorber':(18,27,40,255),'absorber2':(28,39,53,255),'carbon':(38,43,52,255),'titanium':(122,140,155,255),'rf':(122,70,247,255),'field':(65,175,255,150),'fieldhot':(255,107,61,255),'gridblue':(21,77,104,255),'ground':(217,158,75,255),'chrome':(201,217,230,255),'screen':(25,98,141,255),'warningglass':(246,119,59,118),'hologlass':(70,196,248,100),'cabinet':(29,38,55,255)})
raw_names='''BlackstarEMCChamberBase|SemiAnechoicShieldedHall|RFAbsorberPyramidWallSet|RFSealedChamberDoor|RotatingEUTTurntable|TurntableServoGearbox|FourChipletMissionComputerEUT|SpacecraftAvionicsChassis|CoPackagedOpticalTransceiver|RemovableEMCShieldCanopy|ConductiveSeamGasketSet|AvionicsAccessHatch|ReferenceGroundPlane|ChassisEquipotentialBondingGrid|ShieldedHarnessBundle|HighSpeedDifferentialCable|FilteredBulkheadFeedthrough|DCFeedthroughFilterArray|CommonModeChokeModule|DifferentialLCFilterModule|ClipOnFerriteCoreRack|CableRoutingTray|360DegreeShieldTerminationClamp|ShieldGasketCompressionTester|EnclosureSeamLeakageProbe|NearFieldMagneticHProbe|NearFieldElectricEProbe|NearFieldScannerXYGantry|NearFieldScannerZLift|EMIComplianceMeasurementReceiver|HighDynamicRangeSpectrumAnalyzer|RFPreselectorAndAttenuator|SpectralCalibrationReferenceSource|BroadbandEmissionMeasurementAntenna|MotorizedAntennaHeightMast|AntennaPolarizationRotationHead|TurntableAzimuthEncoder|ThreeDimensionalRadiationPatternHologram|EMIFieldVectorHeatmapDisplay|CommonModeCurrentClamp|DifferentialCurrentProbe|DCPowerArtificialNetwork|ConductedEmissionMeasurementRack|RFImmunityCouplingFixture|ProgrammableRFSweepGenerator|ProtectedRFAmplifier|TriaxialRFFieldStrengthProbe|RFUniformityCalibrationFrame|RFImmunitySafetyPLC|ChamberDoorInterlockSwitch|AnechoicBoundaryConditionMonitor|AvionicsSensorSignalEmulator|AnalogInputSusceptibilityAnalyzer|OpticalLinkJitterAndControlMonitor|PrecisionClockEMIProbe|SwitchModePowerConverterModel|SpreadSpectrumClockController|ProtectedSensorInputFilter|InfraredThermalInspectionCamera|ShieldedCoolingAirDuct|RealTimeMissionWorkloadGenerator|FaultDiagnosisAndRepairRouter|CableInjectionFaultFixture|GroundReturnFaultSwitch|SeamGapFaultActuator|RFDesenseFaultEmulator|HolographicEMCAnalysisSCADA|EMCTestSequencingPLCRack|Regulated5VControlSupply|RFSafetyPowerCabinet|EMCCertificationHologram|EmergencyRFStopStation'''
names=raw_names.split('|');assert len(names)==72
# Clusters: large absorber facility; EUT in center; antenna on left; near-field gantry on right;
# service/repair modules at front; protected RF source at far right.
POS={
 1:(0,4.35,-.70),2:(0,4.30,-.70),3:(-11.85,3.12,2.95),4:(0,1.18,-.65),5:(2.85,1.02,-1.7),6:(0,2.70,-.65),7:(0,2.57,-.65),8:(1.4,2.70,-.65),9:(0,3.35,-.65),10:(0,3.10,-.65),11:(-1.15,2.7,.7),12:(0,.90,-.65),13:(-3.60,.90,-.65),14:(-3.7,2.05,1.60),15:(-3.40,2.04,2.22),16:(-4.60,1.65,3.20),17:(-7.35,1.68,4.58),18:(-5.93,1.52,4.60),19:(-4.75,1.53,4.60),20:(-3.62,1.56,4.62),21:(-2.20,1.27,4.55),22:(-4.40,2.22,3.70),23:(-6.55,3.00,3.45),24:(-4.20,3.73,1.20),25:(5.25,3.95,-1.25),26:(6.02,3.95,-1.20),27:(5.68,4.95,-1.35),28:(5.70,3.34,-1.50),29:(-9.50,2.20,4.00),30:(-8.15,2.22,4.05),31:(-10.45,1.53,4.45),32:(-9.25,1.50,3.25),33:(-8.75,4.20,-2.15),34:(-8.75,4.0,-2.18),35:(-8.75,4.33,-2.18),36:(-2.25,1.22,-2.85),37:(-3.10,6.75,-4.30),38:(5.65,6.70,-4.40),39:(-6.90,2.55,3.80),40:(-5.66,2.52,3.70),41:(-8.05,1.22,2.45),42:(-9.60,1.62,2.50),43:(9.14,4.20,-2.20),44:(8.95,2.75,3.40),45:(10.55,2.76,3.25),46:(8.10,4.17,-.80),47:(10.65,4.00,-2.50),48:(11.15,1.66,4.30),49:(-11.75,2.62,2.30),50:(-10.65,6.30,-3.15),51:(-1.1,1.58,4.57),52:(0.35,1.58,4.65),53:(1.72,1.55,4.62),54:(3.1,1.57,4.45),55:(3.90,2.03,2.00),56:(5.0,1.55,4.53),57:(6.02,1.50,4.60),58:(4.9,4.85,1.75),59:(4.0,3.96,-3.20),60:(0,6.45,-4.75),61:(0,1.27,5.55),62:(6.25,1.58,4.55),63:(-3.3,1.53,5.55),64:(-1.35,3.10,1.35),65:(2.75,2.10,4.85),66:(0,7.30,-4.80),67:(9.00,1.53,5.38),68:(-8.45,1.27,5.30),69:(10.60,1.55,5.20),70:(0,8.30,-4.90),71:(11.48,1.62,5.80)
}
pivot_groups=[
('Chamber and turntable',[
(3,'ANIM_CHAMBER_RF_DOOR','rotation','y'),(4,'ANIM_EUT_TURNTABLE','rotation','y'),(5,'ANIM_TURNTABLE_LOCK','translation','x'),(7,'ANIM_EUT_CHASSIS_LID','rotation','x'),(9,'ANIM_SHIELD_CANOPY_LIFT','translation','y'),(11,'ANIM_ACCESS_HATCH','rotation','x'),(2,'ANIM_ABSORBER_SERVICE_PANEL','translation','z'),(12,'ANIM_GROUND_PLANE_ACCESS','translation','z'),(10,'ANIM_SEAM_GASKET_COMPRESSION','translation','y'),(59,'ANIM_SHIELDED_AIR_DUCT','rotation','z'),(6,'ANIM_EUT_LOAD_TRAY','translation','z'),(36,'ANIM_AZIMUTH_ENCODER','rotation','y')]),
('RF scanning',[
(34,'ANIM_ANTENNA_MAST_VERTICAL','translation','y'),(35,'ANIM_ANTENNA_POLARIZATION','rotation','z'),(33,'ANIM_ANTENNA_DISTANCE_SLIDE','translation','x'),(27,'ANIM_NEARFIELD_SCAN_X','translation','x'),(27,'ANIM_NEARFIELD_SCAN_Y','translation','z'),(28,'ANIM_NEARFIELD_SCAN_Z','translation','y'),(26,'ANIM_EFIELD_PROBE_ARM','translation','y'),(25,'ANIM_HFIELD_PROBE_ARM','translation','y'),(24,'ANIM_SEAM_LEAK_PROBE','translation','x'),(39,'ANIM_COMMONMODE_CLAMP','rotation','z'),(40,'ANIM_DIFFERENTIAL_PROBE','translation','y'),(31,'ANIM_RF_PRESELECTOR_DIAL','rotation','z'),(30,'ANIM_SPECTRUM_SWEEP_INDICATOR','indicator','none'),(37,'ANIM_RADIATION_HOLOGRAM','indicator','none')]),
('Shielding and harness',[
(21,'ANIM_HARNESS_CABLE_TRAY','translation','z'),(15,'ANIM_DIFFERENTIAL_CABLE_ROUTE','translation','x'),(16,'ANIM_FEEDTHROUGH_PANEL','translation','z'),(17,'ANIM_DC_FILTER_TRAY','translation','x'),(18,'ANIM_COMMONMODE_CHOKE_INSERT','translation','y'),(19,'ANIM_LC_FILTER_INSERT','translation','x'),(20,'ANIM_FERRITE_CLIP_LATCH','rotation','x'),(22,'ANIM_SHIELD_TERMINATION_CLAMP','rotation','x'),(13,'ANIM_BONDING_STRAP_LATCH','rotation','z'),(23,'ANIM_GASKET_TEST_HEAD','translation','y'),(55,'ANIM_SWITCHER_POWER_MODULE','translation','x'),(56,'ANIM_SPREAD_SPECTRUM_DIAL','rotation','z'),(57,'ANIM_SENSOR_FILTER_DRAWER','translation','z'),(58,'ANIM_THERMAL_CAMERA_PAN','rotation','y')]),
('Immunity faults and safety',[
(43,'ANIM_RF_COUPLING_FIXTURE','translation','x'),(44,'ANIM_RF_GENERATOR_DIAL','rotation','z'),(45,'ANIM_RF_AMPLIFIER_SHUTTER','translation','y'),(46,'ANIM_FIELD_PROBE_CARRIAGE','translation','y'),(47,'ANIM_RF_CALIBRATION_FRAME','translation','x'),(49,'ANIM_DOOR_INTERLOCK_LEVER','rotation','x'),(71,'ANIM_RF_SAFETY_ESTOP','translation','z'),(51,'ANIM_SENSOR_INPUT_STREAM','indicator','none'),(53,'ANIM_OPTICAL_LINK_MONITOR','indicator','none'),(54,'ANIM_CLOCK_JITTER_DISPLAY','indicator','none'),(62,'ANIM_FAULT_CABLE_INJECTOR','translation','x'),(64,'ANIM_SEAM_FAULT_ACTUATOR','translation','y'),(66,'ANIM_SCADA_EMC_DISPLAY','indicator','none'),(70,'ANIM_CERTIFICATION_BEACON','indicator','none')])]
pivots=[]
for group,records in pivot_groups:
 for i,n,kind,axis in records:pivots.append((i,n,kind,axis,POS[i],group))
assert len(pivots)==54 and len(set(x[1] for x in pivots))==54
PV={k:{} for k in names}
for i,n,kind,axis,p,group in pivots:PV[names[i]][n]={'kind':kind,'axis':axis,'position':p,'group':group}

def pyramid(k,base,wh,height,axis='z',col='absorber',tip=None):
 x,y,z=base;w,d=wh;tip=tip or col
 if axis=='z':
  verts=[(x-w/2,y-d/2,z),(x+w/2,y-d/2,z),(x+w/2,y+d/2,z),(x-w/2,y+d/2,z),(x,y,z+height)]
 elif axis=='x':
  verts=[(x,y-d/2,z-w/2),(x,y+d/2,z-w/2),(x,y+d/2,z+w/2),(x,y-d/2,z+w/2),(x+height,y,z)]
 else:
  verts=[(x-w/2,y,z-d/2),(x+w/2,y,z-d/2),(x+w/2,y,z+d/2),(x-w/2,y,z+d/2),(x,y+height,z)]
 faces=[[0,1,4],[1,2,4],[2,3,4],[3,0,4],[0,2,1],[0,3,2]]
 mesh=trimesh.Trimesh(vertices=verts,faces=faces,process=False);addmesh(k,mesh,col)

def adapter(k,p,r=.18,axis='y',accent='silver'):
 cyl(k,p,r,.14,accent,axis,20);ring(k,p,r*1.22,r,.09,axis,'dark',24)

def tmag(k,p,scale=1):
 x,y,z=p
 ring(k,p,.27*scale,.14*scale,.18*scale,'z','carbon',28)
 for a in range(20):
  t=2*math.pi*a/20
  box(k,(x+.20*scale*math.cos(t),y+.20*scale*math.sin(t),z+.09*scale),(.03*scale,.075*scale,.03*scale),'copper')

def wave(k,center,r=.55,n=48,color='cyan',axis='y'):
 x,y,z=center;pts=[]
 for j in range(n+1):
  a=2*math.pi*j/n;rr=r*(1+.15*math.cos(3*a))
  if axis=='y':pts.append((x+rr*math.cos(a),y+.04*math.sin(5*a),z+rr*math.sin(a)))
  else:pts.append((x+rr*math.cos(a),y+rr*math.sin(a),z+.03*math.sin(4*a)))
 pipe(k,pts,.020,color,7)

def antenna(k,p,scale=1):
 x,y,z=p
 box(k,(x,y,z),(.16*scale,.24*scale,1.55*scale),'titanium')
 for j in range(12):
  d=.18+j*.111;L=(.91-j*.053)*scale
  pipe(k,[(x-L,y,z-.71*scale+d*scale),(x+L,y,z-.71*scale+d*scale)],.020*scale,'chrome',9)
  adapter(k,(x,y,z-.71*scale+d*scale),.055*scale,'z')
 box(k,(x,y+.22*scale,z),(.31*scale,.11*scale,.32*scale),'dark')

def rf_rack(k,p,accent='cyan',num=3,h=1.9):
 x,y,z=p
 rbox(k,(x,y,z),(1.04,h,.81),'cabinet')
 for i in range(num):
  yy=y+h*.33-i*h*.28
  screen(k,(x,yy,z+.437),.79,.29,accent)
  for j in range(5):knob(k,x-.36+j*.16,yy-.23,z+.43,accent if j%2 else 'green')
 for u in (-.37,.37):
  for t in range(13):box(k,(x+u,y-h*.45+t*h*.07,z+.43),(.024,.012,.025),'silver')

def logic_board(k,p,scale=1.0):
 x,y,z=p
 box(k,p,(2.28*scale,.20*scale,1.57*scale),'green')
 for a in range(4):
  xx=x+(-.56 if a%2==0 else .56)*scale
  zz=z+(-.35 if a<2 else .35)*scale
  box(k,(xx,y+.14*scale,zz),(.88*scale,.18*scale,.58*scale),'black')
  box(k,(xx,y+.24*scale,zz),(.67*scale,.012*scale,.45*scale),'titanium')
  for j in range(10):
   px=xx-.37*scale+j*.075*scale
   box(k,(px,y+.105*scale,zz+.32*scale),(.032*scale,.05*scale,.038*scale),'gold')
 for t in range(21):
  a=2*math.pi*t/21
  pipe(k,[(x+.93*scale*math.cos(a),y+.12*scale,z+.64*scale*math.sin(a)),(x+.75*scale*math.cos(a),y+.12*scale,z+.50*scale*math.sin(a))],.010,'copper',6)

def component_detail(k,i,p):
 x,y,z=p
 if i==0:
  box(k,(0,.22,0),(25.4,.44,13.7),'navy')
  box(k,(0,.46,0),(24.9,.065,13.12),'steel')
  for a in range(-10,11):
   box(k,(a*1.12,.51,0),(.035,.018,11.45),'silver')
   if a%2==0:lightstrip(k,(a*1.12,.53,5.7),(.45,.019,.020),'amber')
  for a in range(-5,6):box(k,(0,.51,a*1.0), (24.4,.016,.030),'dark')
  for zz in (-6.68,6.68):box(k,(0,.58,zz),(25.2,.20,.13),'titanium')
  for xx in (-12.55,12.55):box(k,(xx,.55,0),(.13,.18,13.4),'titanium')
 elif i==1:
  # chamber shell: back and side metallic structure, open camera-facing side
  for xx in (-12.25,12.25):
   for yy in (.7,4.25,8.62):box(k,(xx,yy,-.72),(.28,.19,12.1),'steel')
   for zz in (-6.25,-3.7,-1.2,1.6,4.3):box(k,(xx,4.4,zz),(.23,8.10,.19),'titanium')
  for xx in np.linspace(-11.6,11.6,13):box(k,(xx,4.6,-6.17),(.18,8.0,.26),'steel')
  for yy in (1.4,4.6,8.7):box(k,(0,yy,-6.17),(24.5,.20,.24),'steel')
  for zz in np.linspace(-5.6,4.65,11):box(k,(0,8.82,zz),(24.6,.18,.19),'steel')
  box(k,(0,9.02,-.7),(25.2,.15,12.2),'dark')
  for xx in (-11.9,11.9):
   for zz in (-4.7,-1.5,1.8):lightstrip(k,(xx,8.58,zz),(.085,.06,1.12),'cyan')
 elif i==2:
  # 850-plus triangular RF absorber wedges, actual meshes
  for xx in np.arange(-11.25,11.4,.67):
   for yy in np.arange(1.05,8.45,.67):pyramid(k,(xx,yy,-5.97),(.59,.59),.28,'z','absorber' if int((xx+yy)*2)%3 else 'absorber2')
  for xx in (-12.0,12.0):
   direction=1 if xx<0 else -1
   for zz in np.arange(-5.7,4.8,.81):
    for yy in np.arange(1.07,8.25,.79):pyramid(k,(xx,yy,zz),(.68,.68),.28*direction,'x','absorber' if int((zz+yy)*2)%3 else 'absorber2')
  for xx in np.arange(-11.4,11.5,.84):
   for zz in np.arange(-5.45,5.08,.86):pyramid(k,(xx,8.67,zz),(.77,.77),-.21,'y','absorber2')
  box(k,(-4.45,2.55,-5.50),(1.4,2.9,.12),'dark')
 elif i==3:
  frame(k,(x,y,z),(1.75,4.7,.35),.14,glass=False)
  box(k,(x,y,z+.15),(1.45,4.32,.13),'steel')
  for yy in np.linspace(y-1.94,y+1.96,14):box(k,(x,yy,z+.23),(1.38,.045,.020),'dark')
  ring(k,(x,y+.30,z+.26),.37,.32,.065,'z','amber',36)
 elif i==4:
  for rr,yy,col in ((2.48,.71,'steel'),(2.24,.85,'dark'),(2.08,1.05,'titanium')):cyl(k,(x,yy,z),rr,.20,col,'y',64)
  for t in range(48):
   a=2*math.pi*t/48
   pipe(k,[(x+2.24*math.cos(a),1.13,z+2.24*math.sin(a)),(x+2.37*math.cos(a),1.13,z+2.37*math.sin(a))],.027,'cyan',7)
  ring(k,(x,1.20,z),1.8,1.63,.10,'y','gold',64)
 elif i==5:
  motor(k,p,.72);ring(k,(x+.50,y,z),.34,.26,.11,'x','gold',28)
 elif i==6:
  box(k,(x,y-.35,z),(3.28,.40,2.78),'steel')
  logic_board(k,(x,y-.02,z),1.08)
  for xx in (-1.43,1.43):
   for zz in (-1.12,1.12):cyl(k,(x+xx,y-.38,z+zz),.12,.55,'silver','y')
 elif i==7:
  frame(k,(x,y,z),(3.9,2.6,3.15),.14,glass=True)
  box(k,(x,y-.87,z),(3.64,.15,2.98),'dark')
  for j in range(24):
   a=2*math.pi*j/24
   box(k,(x+1.52*math.cos(a),y+.40,z+1.28*math.sin(a)),(.10,.05,.10),'gold')
 elif i==8:
  logic_board(k,(x,y,z),.55)
  for j in range(4):pipe(k,[(x+.42,y+.15,z-.32+j*.22),(x+1.10,y+.20,z-.32+j*.22)],.035,('cyan','green','amber','violet')[j])
 elif i==9:
  frame(k,(x,y,z),(4.55,2.1,3.73),.10,glass=True)
  for xx in np.linspace(-1.8,1.8,11):box(k,(x+xx,y+.96,z),(0.08,.08,3.51),'titanium')
 elif i==10:
  for xx in (-1.85,1.85):
   for zz in (-1.42,1.42):pipe(k,[(x+xx,y-.50,z+zz),(x+xx,y+.50,z+zz)],.04,'copper')
  ring(k,(x,y,z),1.4,1.37,.05,'y','gold',60)
 elif i==11:
  box(k,(x,y,z),(1.07,1.28,.17),'steel')
  for j in range(5):cyl(k,(x-.30+j*.15,y+.20,z+.13),.045,.10,'amber','z')
 elif i==12:
  box(k,p,(6.5,.12,4.0),'copper')
  for xx in np.linspace(-3,3,17):box(k,(x+xx,y+.07,z),(0.022,.015,3.6),'gold')
 elif i==13:
  for j in range(13):pipe(k,[(x-1.0+j*.16,y,z-.40),(x-.75+j*.16,y+.10,z+.40)],.036,'ground',10)
  box(k,(x,y-.09,z),(2.6,.16,1.1),'dark')
 elif i in (14,15,21):
  n=12 if i==14 else 6 if i==15 else 9
  box(k,(x,y-.20,z),(2.9,.15,.64),'dark')
  for j in range(n):
   yy=y+.08*(j%3);zz=z+.07*(j%2)
   data_path=[(x-1.05,yy,zz-.5+j*.025),(x-.25,yy+.08,zz),(x+.45,yy+.05,zz+.35),(x+1.10,yy,zz+.65)]
   pipe(k,data_path,.020,('gold','cyan','copper','green')[j%4],8)
 elif i in (16,22):
  mechbox(k,p,(1.15,.70,.65),'steel')
  for a in range(8):
   xx=x-.44+a*.126
   cyl(k,(xx,y+.16,z+.38),.04,.14,'gold','z')
   pipe(k,[(xx,y+.18,z+.37),(xx,y-.14,z+.37)],.025,'copper')
 elif i==17:
  box(k,(x,y,z),(1.55,.65,.96),'steel')
  for j in range(8):
   xx=x-.56+j*.16
   cyl(k,(xx,y+.20,z),.08,.45,'gold','y')
   coil(k,xx,y+.03,z+.18,.09,.10,True)
 elif i in (18,19,20):
  box(k,(x,y-.20,z),(1.20,.23,.76),'dark')
  if i==18:
   for j in range(3):tmag(k,(x-.36+j*.35,y+.12,z+.18),.62)
  elif i==19:
   for j in range(2):coil(k,x-.25+j*.46,y+.10,z+.23,.34,.26,heat=False)
  else:
   for j in range(7):
    ring(k,(x-.48+j*.16,y+.10,z+.10),.105,.065,.11,'y','carbon',16)
    pipe(k,[(x-.48+j*.16,y+.1,z-.27),(x-.48+j*.16,y+.1,z+.38)],.02,'copper')
 elif i==23:
  stand(k,x,y,z,.60);ring(k,(x,y+.22,z),.24,.18,.05,'y','silver');screen(k,(x,y+.20,z+.21),.46,.28)
 elif i in (24,25,26,28,40,46,54,58):
  cylvessel(k,x,y,z,.18,.56,'cyan')
  pipe(k,[(x,y+.33,z),(x,y+.52,z+.25),(x+.15,y+.65,z+.27)],.05,'chrome')
  if i==46:
   for j,ax in enumerate('xyz'):
    color=('red','green','cyan')[j]
    if ax=='x':pipe(k,[(x-.23,y+.22,z),(x+.23,y+.22,z)],.025,color)
    elif ax=='y':pipe(k,[(x,y-.06,z),(x,y+.45,z)],.025,color)
    else:pipe(k,[(x,y+.22,z-.24),(x,y+.22,z+.24)],.025,color)
 elif i==27:
  for yy in (2.85,5.2):box(k,(x,yy,z),(3.75,.16,.22),'steel')
  for xx in (x-1.70,x+1.70):box(k,(xx,4.0,z),(.18,3.0,.20),'titanium')
  for s in range(10):box(k,(x-1.58+s*.35,5.16,z+.18),(.03,.04,.22),'silver')
 elif i==29 or i==30 or i==31 or i==32 or i==42 or i==44 or i==45 or i==48 or i==52 or i==53 or i==56 or i==57 or i==60 or i==61 or i==65 or i==67 or i==68 or i==69:
  accent='red' if i in (48,65,69) else 'violet' if i in (44,45) else 'cyan'
  rf_rack(k,p,accent,3 if i%3==0 else 2,1.4 if i in (31,32,52,56,57) else 1.8)
  if i in (29,30,42,44,45):
   for j in range(12):
    xx=x-.37+j*.065
    box(k,(xx,y+.18+math.sin(j*.7)*.10,z+.50),(.034,.015,.022),'green' if j%4 else 'amber')
 elif i==33:
  antenna(k,p,1.15)
  for j in range(3):ring(k,(x,y,z-.3+j*.42),.35,.31,.045,'z','silver',32)
 elif i==34:
  box(k,(x,y,z),(.38,7.1,.36),'titanium')
  for yy in np.arange(1.02,7.0,.23):box(k,(x-.24,yy,z),(.12,.055,.48),'steel')
  box(k,(x,y-3.35,z),(1.23,.45,1.20),'dark')
 elif i==35:
  ring(k,p,.52,.45,.14,'z','silver',48)
  antenna(k,(x,y,z+.13),.52)
 elif i==36:
  ring(k,p,.34,.29,.10,'y','gold',48)
  for j in range(12):
   a=2*math.pi*j/12
   box(k,(x+.32*math.cos(a),y+.08,z+.32*math.sin(a)),(.055,.042,.055),'cyan')
 elif i in (37,38,66,70):
  w=3.25 if i in (37,38) else 5.2 if i==66 else 1.1
  h=2.0 if i in (37,38) else 1.55 if i==66 else .95
  box(k,p,(w+.16,h+.15,.09),'dark')
  box(k,(x,y,z+.06),(w,h,.015),'hologlass')
  if i==37:
   for j in range(11):wave(k,(x+(j-5)*.14,y,z+.1),.45+abs(j-5)*.095,32,('cyan','blue','amber','fieldhot')[j%4],'z')
  elif i==38:
   for j in range(17):
    for a in range(9):
     xx=x-1.42+j*.177;yy=y-.85+a*.20
     hot=math.hypot((j-11)*.13,(a-4)*.19)<.6
     box(k,(xx,yy,z+.085),(.09,.078,.009),'fieldhot' if hot else 'cyan' if j%3 else 'blue')
  elif i==66:
   for j in range(45):
    t=j/44;yy=y-.48+.70*(.15+math.sin(t*8*math.pi)**2)*.75
    box(k,(x-2.27+j*.105,yy,z+.09),(.038,.07,.015),'amber' if j%5 else 'red')
  else:
   ring(k,(x,y,z+.1),.38,.31,.07,'z','green',36)
 elif i==39 or i==40:
  ring(k,p,.36,.29,.16,'z','silver',32)
  box(k,(x,y-.37,z),(.25,.26,.32),'dark')
 elif i==41:
  rbox(k,p,(1.42,.63,.95),'steel')
  for j in range(12):box(k,(x-.61+j*.11,y+.36,z),(.07,.05,.75),'gold')
 elif i==43:
  frame(k,p,(2.25,3.45,.40),.16,glass=True)
  for a in np.linspace(-.95,.95,8):pipe(k,[(x+a,y-1.42,z),(x+a,y+1.42,z)],.025,'violet')
 elif i==47:
  frame(k,p,(1.85,2.92,1.42),.12,glass=False)
  for u in range(7):
   yy=y-1.1+u*.38
   for a in (-.62,.62):cyl(k,(x+a,yy,z),.045,.20,'chrome','x')
 elif i==49:
  box(k,p,(.65,1.01,.45),'dark')
  cyl(k,(x,y+.24,z+.25),.14,.16,'red','z')
 elif i==50:
  box(k,p,(1.65,.60,.90),'dark')
  for j in range(18):box(k,(x-.66+j*.075,y+.34,z+.12),(.037,.035,.65),'black')
 elif i==51:
  box(k,p,(.86,.55,.88),'dark')
  for j in range(5):cyl(k,(x-.31+j*.16,y+.36,z),.07,.22,'gold','y')
 elif i==55:
  box(k,p,(1.13,.70,.86),'dark')
  for i2 in range(4):tmag(k,(x-.42+i2*.28,y+.12,z+.25),.48)
  for j in range(14):box(k,(x-.5+j*.075,y+.34,z-.15),(.04,.025,.18),'silver')
 elif i==59:
  box(k,(x,y,z),(2.05,.48,1.48),'steel')
  for j in range(4):airfan(k,(x-.74+j*.49,y+.27,z),.17,'y','blue')
 elif i==62 or i==63 or i==64:
  box(k,p,(1.1,.55,.66),'dark')
  for j in range(7):
   pipe(k,[(x-.48+j*.16,y+.28,z-.2),(x-.35+j*.15,y+.36,z+.3)],.02,'copper')
  for j in range(3):knob(k,x-.30+j*.30,y-.12,z+.40,'red')
 elif i==71:
  mechbox(k,p,(.67,.72,.51),'steel')
  cyl(k,(x,y+.09,z+.34),.225,.18,'red','z',24)
  ring(k,(x,y+.09,z+.46),.24,.22,.05,'z','amber',32)
 else:
  station(k,p,(.95,.75,.64),'cyan')


def pivot_geometry(k,piv,meta):
 x,y,z=meta['position'];kind=meta['kind'];up=piv.upper()
 # All named pivots are actual transform parents with mesh children, not CSV-only markers.
 with moving(piv):
  if 'ANTENNA_MAST_VERTICAL' in up:
   box(k,(x,y,z),(.43,1.15,.41),'steel')
   for yy in np.arange(-.46,.55,.18):box(k,(x+.23,y+yy,z),(.07,.05,.55),'gold')
  elif 'ANTENNA_POLARIZATION' in up or 'ANTENNA_DISTANCE' in up:
   antenna(k,(x,y,z+.23),.52)
  elif 'EUT_TURNTABLE' in up:
   ring(k,(x,y+.13,z),1.44,1.36,.08,'y','silver',64)
   for j in range(24):
    a=2*math.pi*j/24
    box(k,(x+1.28*math.cos(a),y+.17,z+1.28*math.sin(a)),(.065,.035,.055),'cyan')
  elif 'CHAMBER_RF_DOOR' in up:
   box(k,(x,y,z),(.88,2.95,.14),'titanium')
   for j in range(8):box(k,(x-.34+j*.098,y,z+.09),(.032,2.7,.02),'dark')
  elif 'SHIELD_CANOPY' in up:
   box(k,(x,y+.36,z),(3.65,.10,2.70),'titanium')
   for xx in (-1.61,1.61):box(k,(x+xx,y,z),(.06,.6,2.45),'copper')
  elif 'GASKET' in up:
   ring(k,(x,y,z),.38,.32,.045,'y','gold',32)
  elif 'NEARFIELD' in up or 'PROBE_ARM' in up or 'SEAM_LEAK' in up or 'THERMAL_CAMERA' in up:
   box(k,(x,y,z),(.55,.24,.66),'steel')
   pipe(k,[(x,y-.1,z),(x,y-.44,z+.27)],.055,'silver')
   cyl(k,(x,y-.49,z+.30),.15,.17,'cyan','y',16)
  elif 'RF_AMPLIFIER_SHUTTER' in up or 'SAFE' in up and 'SHUTTER' in up:
   box(k,(x,y,z),(.86,.55,.11),'red')
   for j in range(7):box(k,(x-.37+j*.11,y,z+.07),(.028,.48,.020),'black')
  elif 'RF_GENERATOR_DIAL' in up or 'PRESELECTOR_DIAL' in up or 'SPREAD_SPECTRUM_DIAL' in up:
   cyl(k,(x,y,z+.43),.17,.10,'silver','z',30)
   box(k,(x+.08,y+.05,z+.52),(.14,.028,.02),'cyan')
  elif 'FERRITE' in up or 'CHOKE' in up:
   tmag(k,(x,y,z+.12),.88)
  elif 'FILTER' in up:
   box(k,(x,y,z),(.70,.23,.55),'dark')
   for j in range(5):coil(k,x-.27+j*.135,y+.16,z+.17,.10,.19,True)
  elif 'HARNESS' in up or 'CABLE' in up or 'TERMINATION' in up or 'BONDING_STRAP' in up:
   for j in range(7):
    pipe(k,[(x-.38+j*.12,y,z-.28),(x-.18+j*.11,y+.19,z+.02),(x+.22+j*.09,y,z+.28)],.025,('copper','cyan','gold')[j%3],8)
  elif 'TURNTABLE_LOCK' in up or 'DOOR_INTERLOCK' in up or 'EMERGENCY' in up:
   box(k,(x,y,z),(.41,.33,.36),'dark')
   cyl(k,(x,y,z+.26),.125,.14,'red' if 'EMERGENCY' in up else 'amber','z',16)
  elif 'SENSOR' in up or 'OPTICAL_LINK' in up or 'CLOCK_JITTER' in up or 'SCADA' in up:
   compact_screen(k,(x,y,z+.09),.85,.53,'violet' if 'JITTER' in up else 'cyan')
  elif kind=='indicator':
   wave(k,(x,y,z),.36,32,'fieldhot' if 'FAULT' in up else 'cyan','z')
  elif kind=='rotation':
   ring(k,(x,y,z),.26,.20,.10,'z','titanium',30)
   box(k,(x+.20,y,z+.05),(.19,.05,.07),'amber')
  elif kind=='translation':
   box(k,(x,y,z),(.56,.28,.44),'steel')
   for j in range(5):box(k,(x-.21+j*.105,y+.16,z),(.045,.07,.26),'cyan')
  else:
   compact_screen(k,(x,y,z),.7,.43,'cyan')

# Place 206 real visible circular socket meshes, each with coordinate manifest; art-only.
ELIGIBLE=[i for i in range(3,72) if i not in (10,12,37,38,70)]
counts={i:3 for i in ELIGIBLE}
for i in (29,30,41,42,44,45,46,48,51,52,53,54,55,56,57,67,68,69,71):counts[i]+=1
# Resolve target exactly 206 by allocating on actual accessible equipment.
extra=(206-sum(counts.values()))
if extra>=0:
 for j in range(extra):counts[(29,30,42,44,45,46,48,51,67,68,69,71)[j%12]]+=1
else:
 for j in range(-extra):
  i=(20,21,23,28,31,32,43,47,62,63,64)[j%11];counts[i]-=1
assert sum(counts.values())==206
SOCKETS=[]
def add_visual_sockets(k,i):
 n=counts.get(i,0)
 if not n:return
 verts=np.vstack([mesh.vertices for mesh,c,g in M[k]]);lo=verts.min(0);hi=verts.max(0)
 cx=float((lo[0]+hi[0])*.5);cy=float((lo[1]+hi[1])*.5)
 width=max(.40,float(hi[0]-lo[0]));height=max(.35,float(hi[1]-lo[1]));z=float(hi[2]+.065)
 for j in range(n):
  cols=min(6,max(2,math.ceil(n/2)));row=j//cols;column=j%cols
  xx=cx+(column-(cols-1)*.5)*min(.17,width*.72/max(1,cols-1))
  yy=cy+(row-(math.ceil(n/cols)-1)*.5)*min(.14,height*.42)
  center=(round(xx,5),round(yy,5),round(z,5))
  cyl(k,center,.057,.048,'carbon','z',12)
  ring(k,(xx,yy,z+.025),.070,.049,.024,'z','gold',16)
  cyl(k,(xx,yy,z+.050),.028,.02,'cyan' if j%3 else 'amber','z',12)
  SOCKETS.append((i,center,j))

for i,k in enumerate(names):
 component_detail(k,i,POS.get(i,(0,1.5,0)))
 for piv,meta in PV.get(k,{}).items():pivot_geometry(k,piv,meta)
 assert M.get(k),(i,k)
 assert {grp for _,_,grp in M[k] if grp}==set(PV.get(k,{})),('Missing pivot mesh',i,k)
 add_visual_sockets(k,i)

print('MODELED',len(names),'models',sum(len(M[k]) for k in names),'mesh parts',sum(sum(len(m.faces) for m,c,g in M[k]) for k in names),'triangles',len(SOCKETS),'sockets',flush=True)
manifest=[]
for i,k in enumerate(names):
 filename=f'{i:02d}_L82_{k}'
 export_fbx(k,MD/(filename+'.fbx'))
 export_glb(k,GD/(filename+'.glb'))
 parts=M[k];verts=np.vstack([m.vertices for m,c,g in parts]);tr=sum(len(m.faces) for m,c,g in parts)
 manifest.append({'index':i,'name':'L82_'+k,'file':filename+'.fbx','glb':filename+'.glb','triangle_count':tr,'mesh_parts':len(parts),'bbox_min':np.round(verts.min(0),5).tolist(),'bbox_max':np.round(verts.max(0),5).tolist(),'rig_groups':list(PV.get(k,{})),'animation_pivots':{p:list(meta['position']) for p,meta in PV.get(k,{}).items()},'visual_terminal_sockets':counts.get(i,0)})
 print('EXPORTED',f'{i+1:02d}/72',filename,'tris',tr,'parts',len(parts),flush=True)
assem='Level82_Complete_ArtKit_Showcase'
M[assem]=[part for k in names for part in M[k]]
PV[assem]={p:meta for k,group in PV.items() for p,meta in group.items()}
export_fbx(assem,MD/(assem+'.fbx'));export_glb(assem,GD/(assem+'.glb'))
allverts=np.vstack([m.vertices for m,c,g in M[assem]])
bounds=np.stack([allverts.min(0),allverts.max(0)])
config={'level_id':'level_82','display_name':'Silence the Spectrum','subtitle':'Advanced Electromagnetic Compatibility Qualification','individual_models':72,'individual_glbs':72,'assembled_fbx':'Models/'+assem+'.fbx','assembled_glb':'GLB/'+assem+'.glb','individual_triangle_count':sum(r['triangle_count'] for r in manifest),'individual_mesh_parts':sum(r['mesh_parts'] for r in manifest),'animation_parent_pivot_groups':54,'planned_player_wires':134,'planned_unique_functional_terminals':206,'modeled_art_only_terminal_sockets':len(SOCKETS),'workstation_width_m_nominal':25.4,'workstation_world_bounds_m':bounds.round(5).tolist(),'fbx_numeric_units':'centimetres','fbx_unit_scale_factor':1,'glb_numeric_units':'metres','axes':'Y up / X width / Z front','assembly_origin':[0,0,0],'actual_player_wires_included':False,'functional_spark_terminals_included':False,'emc_solver_included':False,'six_authoritative_objectives_implemented':False,'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(config,indent=2))
with (ROOT/'Animation_Rigs.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Pivot','Model','Kind','Axis','X','Y','Z','Group','Description'])
 for i,n,kind,axis,pos,group in pivots:w.writerow([n,'L82_'+names[i],kind,axis,*pos,group,'Actual mesh-owning binary FBX and GLB parent'])
with (ROOT/'Art_Assembly.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Index','File','LocalX','LocalY','LocalZ','Scale','Role'])
 for r in manifest:w.writerow([r['index'],r['file'],0,0,0,1,'Common-origin modular EMC qualification chamber'])
with (ROOT/'Component_Roles.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Model','Category','Function','UnityGameplayStatus'])
 for r in manifest:
  i=r['index'];cat='Chamber and EUT' if i<=13 else 'EMC repairs' if i<=23 else 'Near-field scan' if i<=28 else 'Emission measurement' if i<=42 else 'RF immunity and safety' if i<=50 else 'Mission function monitoring' if i<=60 else 'Fault injection and controls'
  w.writerow([r['name'],cat,r['name'].replace('L82_',''),'Art only, not a functioning Unity simulation component'])
with (ROOT/'Visual_Terminal_Sockets_206.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['AnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','UnityStatus'])
 for j,(i,p,idx) in enumerate(SOCKETS,1):w.writerow([f'P{j:03d}','L82_'+names[i],*p,'Gold-rimmed player-wire socket art','ART_ANCHOR_NOT_SPARKTERMINAL'])
(ROOT/'Material_Palette.json').write_text(json.dumps({k:{'rgba_8bit':list(v),'surface_intent':'transparent preview' if v[3]<255 else 'opaque preview'} for k,v in P.items()},indent=2))
print('LEVEL82 COMPLETE',len(manifest),'models',len(pivots),'pivots',len(SOCKETS),'sockets',bounds.tolist(),flush=True)
