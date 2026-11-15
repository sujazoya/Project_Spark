#!/usr/bin/env python3
"""Project Spark Level 80: original four-channel silicon photonics and optical measurement modular geometry, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical or chiplet-bonding gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level80_HD_AAA')
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 80 Four Channel WDM Photonics Original Modular Geometry')]),
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






# Original Level 80 photonic laboratory model set.
names='''IndustrialPhotonicDataAtriumBase|TransparentPhotonicCoreEnclosure|CompletedFourChipletPackageCarrier|BondedFourChipletElectronicAssembly|CoPackagedSiliconPhotonicInterposer|HolographicOpticalWaveguideMap|PhotonicEdgeCouplerArray|FiberRibbonDockingStation|OpticalFiberRibbonCassette|MultiWavelengthLaserSourceRack|LaserCombStabilizationModule|LaserDriveController|LaserTemperatureControlMount|OpticalSafetyShutter|OpticalIsolatorModule|CalibratedOpticalTapSplitter|LaunchOpticalPowerMeter|PolarizationControlModule|VariableOpticalAttenuator|SpectralReferenceEtalon|HighSpeedTransmitterDriver|ElectroOpticalModulatorArray|ModulatorBiasController|FourChannelWDMMultiplexer|HolographicFourWavelengthCombiner|PhotonicWaveguideCrossSection|PhotonicThermalControlPlatform|MicroringResonatorChannelA|MicroringResonatorChannelB|MicroringResonatorChannelC|MicroringResonatorChannelD|FourZoneMicroringHeaterBank|MicroringHeaterDACController|MicroringThermalSensorArray|ThermalCrosstalkAnalyzer|FourChannelWDMDemultiplexer|DemultiplexerResonanceMonitor|FourChannelOpticalDropPortPanel|PhotodiodeReceiverArray|ReceiverTransimpedanceAmplifierBank|ClockAndDataRecoveryModule|ReceiverOpticalPowerMonitor|HighResolutionOpticalSpectrumAnalyzer|OpticalLinkPowerBudgetMeter|HighSpeedEyeDiagramAnalyzer|OpticalLinkBitErrorRateTester|PAM4WaveformSamplingInstrument|ForwardErrorCorrectionDecoder|PseudorandomBitPatternGenerator|ChipletOpticalTestRouter|MemoryOpticalLinkDiagnosticModule|LogicOpticalTransmitterInterface|IOOpticalOutputInterface|SensorOpticalTelemetryInterface|OpticalFiberMicrobendMonitor|ControlledOpticalFaultInjectionModule|OpticalLinkSupervisoryController|PhotonicLaserSafetyPLCRack|EmergencyOpticalShutterStation|PhotonicCoolingAirManifold|HolographicThermalResonanceDisplay|HolographicOpticalDataSCADA|PhotonicWaveguideDefectMicroscope|StrayLightSafetyMonitor|OpticalCalibrationReferenceCoupon|ESDGuardedPhotonicTestFixture|Regulated5VControlSupply|GiantWavelengthRoutingHologram'''.split('|')
assert len(names)==68
# All 50 pivot groups below are mesh-owning FBX/GLB parents.
pivots=[
(9,'ANIM_LASER_SOURCE_DRAWER','translation','z',(-9.20,2.16,-.20)),
(10,'ANIM_LASER_COMB_LOCK','indicator','none',(-8.48,3.27,-1.00)),
(12,'ANIM_LASER_TEMPERATURE_STAGE','translation','y',(-8.25,1.84,-2.40)),
(13,'ANIM_SAFETY_SHUTTER','translation','y',(-7.65,2.92,-.32)),
(14,'ANIM_OPTICAL_ISOLATOR','rotation','z',(-7.23,2.77,-.32)),
(15,'ANIM_TAP_SPLITTER','rotation','y',(-6.77,2.76,-.32)),
(17,'ANIM_POLARIZATION_CONTROLLER','rotation','x',(-7.24,1.48,1.55)),
(18,'ANIM_OPTICAL_ATTENUATOR','translation','x',(-6.55,1.49,1.55)),
(19,'ANIM_SPECTRAL_REFERENCE_SCAN','translation','x',(-5.56,3.58,-1.25)),
(16,'ANIM_LAUNCH_POWER_PROBE','translation','y',(-6.40,3.34,1.20)),
(1,'ANIM_PHOTONIC_CORE_LID','translation','y',(0,4.10,-.95)),
(7,'ANIM_FIBER_DOCK','translation','x',(-4.30,2.35,-.95)),
(6,'ANIM_COUPLER_ARM','translation','x',(-2.76,2.47,-.95)),
(23,'ANIM_WDM_MULTIPLEXER','indicator','none',(-4.22,3.20,-.95)),
(27,'ANIM_RING_A_RESONANCE','indicator','none',(-2.85,5.55,-3.27)),
(28,'ANIM_RING_B_RESONANCE','indicator','none',(-.95,5.55,-3.27)),
(29,'ANIM_RING_C_RESONANCE','indicator','none',(.95,5.55,-3.27)),
(30,'ANIM_RING_D_RESONANCE','indicator','none',(2.85,5.55,-3.27)),
(31,'ANIM_FOUR_ZONE_HEATER_BANK','translation','y',(0,2.30,-2.70)),
(67,'ANIM_WAVELENGTH_ROUTING_HOLOGRAM','indicator','none',(0,6.20,-4.62)),
(35,'ANIM_WDM_DEMULTIPLEXER','indicator','none',(4.08,3.23,-.95)),
(37,'ANIM_DROP_PORT_PANEL','translation','x',(5.10,2.75,-.95)),
(38,'ANIM_PHOTODIODE_ARRAY','translation','y',(6.40,2.90,-.20)),
(39,'ANIM_TRANSIMPEDANCE_DISPLAY','indicator','none',(7.10,3.72,1.35)),
(40,'ANIM_CLOCK_RECOVERY_DISPLAY','indicator','none',(8.85,3.66,1.20)),
(41,'ANIM_RX_POWER_MONITOR','translation','y',(6.85,2.40,1.48)),
(42,'ANIM_SPECTRUM_ANALYZER_SCAN','indicator','none',(7.88,5.70,-3.68)),
(44,'ANIM_EYE_DIAGRAM_DISPLAY','indicator','none',(9.72,5.58,-3.63)),
(45,'ANIM_BER_TEST_DISPLAY','indicator','none',(9.64,3.67,-.55)),
(46,'ANIM_PAM4_SIGNAL_DISPLAY','indicator','none',(9.45,3.60,2.37)),
(48,'ANIM_BIT_PATTERN_GENERATOR','translation','z',(-5.02,1.50,2.74)),
(47,'ANIM_FEC_DECODER_DISPLAY','indicator','none',(9.55,1.66,3.10)),
(49,'ANIM_CHIPLET_DATA_ROUTER','indicator','none',(-.02,2.95,3.30)),
(51,'ANIM_LOGIC_DATA_STREAM','indicator','none',(-1.02,3.75,1.65)),
(50,'ANIM_MEMORY_DATA_STREAM','indicator','none',(-.28,3.75,1.65)),
(52,'ANIM_IO_DATA_STREAM','indicator','none',(.47,3.75,1.65)),
(53,'ANIM_SENSOR_DATA_STREAM','indicator','none',(1.20,3.75,1.65)),
(54,'ANIM_FIBER_BEND_SENSOR','translation','y',(4.75,1.65,2.00)),
(55,'ANIM_FAULT_INJECTION_ARM','translation','x',(5.47,2.67,2.85)),
(62,'ANIM_WAVEGUIDE_MICROSCOPE','translation','y',(3.68,3.86,3.10)),
(59,'ANIM_COOLING_FAN_BANK','rotation','z',(-2.70,1.35,-3.55)),
(60,'ANIM_THERMAL_CROSSTALK_MAP','indicator','none',(0,4.75,4.25)),
(36,'ANIM_RESONANCE_LOCK_DISPLAY','indicator','none',(5.75,4.50,-3.60)),
(43,'ANIM_OPTICAL_POWER_BUDGET','indicator','none',(7.25,1.50,3.56)),
(58,'ANIM_SAFETY_EMERGENCY_SHUTTER','translation','y',(10.52,1.92,4.66)),
(63,'ANIM_STRAY_LIGHT_MONITOR','translation','y',(-9.80,4.12,2.20)),
(64,'ANIM_REFERENCE_COUPON_STAGE','translation','x',(-4.55,1.52,4.00)),
(65,'ANIM_TEST_FIXTURE_LID','rotation','x',(.00,1.68,4.16)),
(61,'ANIM_PHOTONIC_MAIN_SCADA','indicator','none',(.0,5.61,-5.43)),
(67,'ANIM_LEVEL80_CERTIFICATION_HOLOGRAM','indicator','none',(0,6.85,-4.62)),
]
assert len(pivots)==50 and len({p[1] for p in pivots})==50
PV={}
for idx,p,kind,axis,xyz in pivots:
    PV.setdefault(names[idx],{})[p]={'position':np.array(xyz,dtype=float),'kind':kind,'axis':axis}
# Visually accurate color-coded false-color optical channels.
CH=['cyan','green','amber','violet']; CH_POS=[-2.85,-.95,.95,2.85]

# Global anchors of all 68 distinct components (metres, Y-up).
POS={
2:(0,1.70,-.95),3:(0,2.23,-.95),4:(0,2.55,-.95),5:(0,4.10,1.60),6:(-2.76,2.47,-.95),7:(-4.30,2.35,-.95),8:(-5.45,1.50,-2.40),
9:(-9.2,2.32,-.2),10:(-8.48,3.27,-1),11:(-9.4,1.40,-2.25),12:(-8.25,1.84,-2.40),13:(-7.65,2.92,-.32),14:(-7.23,2.77,-.32),15:(-6.77,2.76,-.32),16:(-6.40,3.34,1.20),17:(-7.24,1.48,1.55),18:(-6.55,1.49,1.55),19:(-5.56,3.58,-1.25),
20:(-6.2,2.15,2.70),21:(-3.45,2.15,1.55),22:(-5.20,1.63,1.85),23:(-4.22,3.2,-.95),24:(-2.92,4.08,.95),25:(2.75,3.58,.48),26:(0,1.27,-1.40),
27:(-2.85,5.55,-3.27),28:(-.95,5.55,-3.27),29:(.95,5.55,-3.27),30:(2.85,5.55,-3.27),31:(0,2.30,-2.70),32:(1.8,1.48,-2.78),33:(2.75,1.53,-2.8),34:(3.28,4.15,3.80),
35:(4.08,3.23,-.95),36:(5.75,4.5,-3.60),37:(5.10,2.75,-.95),38:(6.40,2.90,-.20),39:(7.10,3.72,1.35),40:(8.85,3.66,1.2),41:(6.85,2.40,1.48),42:(7.88,5.70,-3.68),43:(7.25,1.5,3.56),44:(9.72,5.58,-3.63),45:(9.64,3.67,-.55),46:(9.45,3.60,2.37),
47:(9.55,1.66,3.10),48:(-5.02,1.50,2.74),49:(-.02,2.95,3.30),50:(-.28,3.75,1.65),51:(-1.02,3.75,1.65),52:(.47,3.75,1.65),53:(1.20,3.75,1.65),54:(4.75,1.65,2.00),55:(5.47,2.67,2.85),56:(8.30,1.48,-2.28),57:(-10.15,1.50,-3.45),58:(10.52,1.92,4.66),59:(-2.70,1.35,-3.55),60:(0,4.75,4.25),61:(0,5.61,-5.43),62:(3.68,3.86,3.1),63:(-9.80,4.12,2.20),64:(-4.55,1.52,4.0),65:(0,1.68,4.16),66:(-10.35,1.35,2.75),67:(0,6.20,-4.62)
}

def emit_holoring(k,p,r,color='cyan',vertical=True):
    axis='z' if vertical else 'y';x,y,z=p
    ring(k,p,r,r*.94,.042,axis,color,48)
    ring(k,p,r*.78,r*.755,.023,axis,'ice',36)
    for i in range(24):
        a=i*math.tau/24
        q=(x+r*.98*math.cos(a),y+r*.98*math.sin(a),z+.045) if vertical else (x+r*.98*math.cos(a),y+.045,z+r*.98*math.sin(a))
        box(k,q,(.045,.043,.033),color if i%6 else 'white')
    for j in range(12):
        a=j*math.tau/12
        if vertical:p0=(x+r*.87*math.cos(a),y+r*.87*math.sin(a),z+.04)
        else:p0=(x+r*.87*math.cos(a),y+.03,z+r*.87*math.sin(a))
        cyl(k,p0,.019,.022,'gold',axis,8)

def optical_fibers(k,p0,p1,colors=CH,width=.045,curved=True):
    for j,col in enumerate(colors):
        offset=(j-1.5)*.12
        pts=[]
        for n in range(11):
            t=n/10
            x=p0[0]*(1-t)+p1[0]*t
            y=p0[1]*(1-t)+p1[1]*t+(math.sin(math.pi*t)*.34 if curved else 0)
            z=p0[2]*(1-t)+p1[2]*t+offset
            pts.append((x,y,z))
        pipe(k,pts,width,col,8)
    for p in (p0,p1):
        for j in range(4):cyl(k,(p[0],p[1],p[2]+(j-1.5)*.12),.075,.16,'silver','x',12)

def chip_pack(k,p):
    x,y,z=p
    box(k,p,(3.40,.16,2.15),'copper')
    box(k,(x,y+.105,z),(3.25,.065,2.0),'black')
    for j,col in enumerate(CH):
        xx=x-1.08+(j%2)*2.18;zz=z-.54+(j//2)*1.08
        box(k,(xx,y+.23,zz),(.98,.22,.86),col)
        box(k,(xx,y+.35,zz),(.84,.018,.72),'black')
        for m in range(8):
            bx=xx-.40+m*.113
            box(k,(bx,y+.365,zz-.34),(.026,.012,.10),'gold')
            box(k,(bx,y+.365,zz+.34),(.026,.012,.10),'gold')
        for az in (-1,1):
            for ax in (-1,1):cyl(k,(xx+ax*.43,y+.13,zz+az*.35),.03,.13,'silver','y',8)
    for j in range(80):
        xx=x-1.5+(j%20)*.158;zz=z-.95+(j//20)*.62
        cyl(k,(xx,y-.1,zz),.018,.025,'gold','y',8)

def photonic_die(k,p):
    x,y,z=p
    box(k,p,(3.25,.18,1.8),'steel')
    box(k,(x,y+.11,z),(3.12,.040,1.65),'navy')
    for j,col in enumerate(CH):
        zz=z-.65+j*.42
        pipe(k,[(x-1.38,y+.145,zz),(x-.68,y+.145,zz),(x-.22,y+.145,zz-.14),(x+.52,y+.145,zz-.14),(x+1.34,y+.145,zz)],.017,col,9)
        ring(k,(x-.08,y+.16,zz-.14),.22,.194,.023,'y',col,32)
        for m in range(7):
            bx=x-.6+m*.18
            box(k,(bx,y+.18,zz+.09),(.055,.025,.04),'gold')
    for j in range(26):
        xx=x-1.40+j*.109
        box(k,(xx,y+.155,z+.81),(.035,.025,.055),'gold')

def spectrum_display(k,p,w=2.4,h=1.15):
    x,y,z=p
    box(k,p,(w,h,.14),'dark');box(k,(x,y,z+.08),(w*.94,h*.88,.011),'black')
    for j,col in enumerate(CH):
        bx=x-w*.35+j*w*.235
        pts=[(bx + u*w*.06,y-h*.29+h*.56*math.exp(-0.5*(u/1.8)**2),z+.10) for u in np.linspace(-3,3,17)]
        pipe(k,pts,.012,col,6)
        box(k,(bx,y-h*.39,z+.11),(.035,.035,.018),col)
    for i in range(9):box(k,(x-w*.44+i*w*.11,y-h*.43,z+.09),(.009,h*.74,.009),'steel')

def eye_display(k,p,w=1.9,h=1.15,color='green'):
    x,y,z=p
    box(k,p,(w,h,.16),'steel');box(k,(x,y,z+.087),(w*.92,h*.84,.012),'black')
    for j in range(7):
        shift=(j-3)*.018
        for flip in (-1,1):
            pts=[]
            for i in range(18):
                t=i/17;yy=y+flip*(h*.33)*(2*math.cos(math.pi*t)**2-1)*.77+shift
                pts.append((x-w*.41+t*w*.82,yy,z+.105))
            pipe(k,pts,.010,color,6)

def rack(k,p,w=1.55,h=2.75,d=1.22,accent='cyan'):
    x,y,z=p
    rbox(k,p,(w,h,d),'navy')
    box(k,(x,y,z+d/2+.02),(w*.90,h*.89,.04),'dark')
    for j in range(4):
        yy=y+h*.34-j*h*.23
        box(k,(x,yy,z+d*.535),(w*.80,h*.17,.055),'steel')
        compact_screen(k,(x-.13,yy+.015,z+d*.60),w*.45,h*.11,accent)
        for n in range(5):knob(k,x+w*.16+n*w*.052,yy-.02,z+d*.60,accent if n%2 else 'green')
    for xx in (-w*.43,w*.43):box(k,(x+xx,y,z+d*.55),(.06,h*.89,.07),'silver')
    for j in range(4):
        box(k,(x-w*.35+j*w*.23,y+h*.47,z+d*.55),(.08,.034,.035),accent)

def instrument(k,p,accent='cyan',w=.92,h=.74):
    x,y,z=p
    controlbox(k,p,(w,h,.74),'steel',accent,4)
    for j in range(6):cyl(k,(x-.33+j*.13,y-h*.38,z+.42),.025,.06,'silver','z',8)
    pipe(k,[(x-.38,y-.15,z-.35),(x-.42,y-.23,z-.65)],.024,'dark',8)

def ring_mesh(k,p,r=.8,color='cyan'):
    emit_holoring(k,p,r,color,True)
    x,y,z=p
    box(k,(x,y-r-0.12,z-.09),(.85,.13,.4),'steel')
    for j in range(12):
        a=j*math.tau/12
        box(k,(x+r*.76*math.cos(a),y+r*.76*math.sin(a),z+.05),(.034,.045,.03),'gold' if j%2 else color)

def pivot_geometry(k,piv,meta):
    x,y,z=meta['position'];up=piv.upper()
    with moving(piv):
        if 'RING_' in up and 'RESONANCE' in up:
            col=CH['ABCD'.find(piv.split('_RING_')[1][0])]
            ring_mesh(k,(x,y,z),.78,col)
        elif 'WAVELENGTH_ROUTING_HOLOGRAM' in up:
            box(k,(x,y,z),(7.7,2.8,.07),'black')
            box(k,(x,y,z+.05),(7.5,2.63,.012),'glass')
            for j,col in enumerate(CH):
                xp=-2.85+j*1.9
                emit_holoring(k,(xp,y+.35,z+.16),.57,col)
                pipe(k,[(xp,y-.3,z+.18),(xp,y-.68,z+.18),(-1.0+j*.68,y-.90,z+.18),(-.28+j*.18,y-1.15,z+.18)],.026,col,7)
        elif 'CERTIFICATION' in up:
            ring_mesh(k,(x,y,z+.1),.42,'green')
        elif 'PHOTONIC_MAIN_SCADA' in up:
            giantdisplay(k,(x,y,z),6.3,1.52)
            for j,col in enumerate(CH):
                box(k,(x-2.45+j*1.62,y-.57,z+.20),(1.05,.13,.025),col)
        elif 'SPECTRUM_ANALYZER' in up or 'RESONANCE_LOCK' in up or 'THERMAL_CROSSTALK_MAP' in up:
            spectrum_display(k,(x,y,z),2.1,1.15)
        elif 'EYE_DIAGRAM' in up or 'BER_TEST' in up or 'PAM4' in up:
            eye_display(k,(x,y,z),1.7,1.06,'green' if 'EYE' in up else 'cyan')
        elif 'DATA_STREAM' in up:
            q={'LOGIC':'cyan','MEMORY':'green','IO':'amber','SENSOR':'violet'}
            col=next((c for label,c in q.items() if label in up),'cyan')
            for j in range(9):
                pipe(k,[(x-.28+j*.07,y-.12,z),(x-.28+j*.07,y+.15,z+.1)],.013,col,8)
        elif 'SHUTTER' in up:
            box(k,(x,y,z),(.65,.75,.22),'dark')
            box(k,(x,y,z+.14),(.55,.62,.025),'amber')
            for j in range(8):box(k,(x-.20+j*.058,y,z+.15),(.030,.62,.012),'silver')
        elif 'FIBER_DOCK' in up or 'COUPLER' in up or 'TAP_SPLITTER' in up or 'OPTICAL_ISOLATOR' in up:
            box(k,(x,y,z),(.60,.25,.62),'steel')
            for j,col in enumerate(CH):
                z0=z-.23+j*.15
                cyl(k,(x-.28,y,z0),.048,.18,col,'x',12)
                cyl(k,(x+.29,y,z0),.053,.18,'silver','x',12)
            for j in range(8):box(k,(x-.24+j*.07,y+.16,z),(.025,.03,.3),'gold')
        elif 'PHOTODIODE' in up:
            box(k,(x,y,z),(1.25,.35,.65),'steel')
            for j,col in enumerate(CH):
                xx=x-.43+j*.29
                cyl(k,(xx,y+.16,z),.10,.09,'black','y',20)
                cyl(k,(xx,y+.22,z),.065,.028,col,'y',20)
        elif 'REFERENCE_COUPON' in up:
            waferdisc(k,(x,y,z),.45,.025,'navy')
        elif 'COOLING_FAN' in up:
            for j in range(3):fan(k,(x-.5+j*.50,y,z),.21,'z')
        elif 'OPTICAL_POWER_BUDGET' in up or 'TRANSIMPEDANCE' in up or 'CLOCK_RECOVERY' in up or 'FEC_DECODER' in up:
            eye_display(k,(x,y,z),1.5,.85,'cyan')
        elif 'LASER_SOURCE_DRAWER' in up:
            box(k,(x,y,z),(1.35,.45,1.0),'navy')
            for j,col in enumerate(CH):
                cyl(k,(x-.45+j*.30,y+.08,z+.56),.095,.22,col,'z',16)
                ring(k,(x-.45+j*.30,y+.08,z+.68),.11,.085,.03,'z','silver',20)
        elif 'TEST_FIXTURE_LID' in up or 'PHOTONIC_CORE_LID' in up:
            box(k,(x,y,z),(3.48,.13,2.08) if 'CORE' in up else (2.25,.13,1.4),'steel')
            box(k,(x,y+.08,z),(3.25,.028,1.80) if 'CORE' in up else (1.95,.028,1.14),'glass')
        elif 'FAULT_INJECTION' in up or 'WAVEGUIDE_MICROSCOPE' in up or 'RX_POWER_MONITOR' in up or 'LAUNCH_POWER_PROBE' in up:
            box(k,(x,y,z),(.46,.3,.46),'steel')
            cyl(k,(x,y-.26,z),.16,.36,'dark','y',18)
            ring(k,(x,y-.45,z),.18,.135,.025,'y','cyan',24)
        elif 'EMERGENCY' in up:
            box(k,(x,y,z),(.45,.55,.34),'black');cyl(k,(x,y,z+.24),.16,.17,'red','z',24)
        else:
            box(k,(x,y,z),(.62,.30,.52),'steel')
            for j in range(6):box(k,(x-.21+j*.082,y+.17,z),(.040,.052,.16),CH[j%4])


def build(i):
    k=names[i]
    p=POS.get(i,(0,2,0))
    x,y,z=p
    if i==0:
        box(k,(0,.32,0),(23.20,.62,12.10),'navy')
        box(k,(0,.64,0),(23.0,.04,11.95),'steel')
        for xx in np.linspace(-11.05,11.05,24):
            box(k,(xx,.68,0),(.026,.023,11.4),'light')
            for zz in np.linspace(-5.3,5.3,8):box(k,(xx,.70,zz),(.035,.017,.10),'cyan' if abs(xx)<3.5 else 'amber')
        for zz in (-5.82,5.82):
            box(k,(0,.74,zz),(22.9,.13,.22),'dark')
            for xx in np.linspace(-11,11,48):box(k,(xx,.83,zz),(.05,.025,.042),'cyan' if zz<0 else 'amber')
        for xx,zz in [(-9.6,-3.1),(9.6,-3.1),(-9.5,3.5),(9.5,3.5),(0,-4.2),(0,3.4)]:
            box(k,(xx,.72,zz),(1.45,.11,1.40),'silver')
            for dx in (-.5,.5):
                for dz in (-.4,.4):cyl(k,(xx+dx,.80,zz+dz),.055,.08,'black','y',8)
    elif i==1:
        frame(k,(0,3.15,-.95),(5.10,4.10,4.40),.11,True,True)
        for xx in (-2.35,2.35):
            for zz in (-2.90,.96):
                box(k,(xx,3.00,zz),(.13,3.60,.14),'steel')
                for a in range(15):box(k,(xx,1.55+a*.20,zz+.09),(.06,.042,.04),'blue' if a%3 else 'gold')
        for xx in (-2.30,2.30):
            box(k,(xx,1.04,-.95),(.25,.40,4.35),'dark')
            for zz in np.linspace(-2.7,.85,16):box(k,(xx,1.30,zz),(.09,.03,.07),'silver')
    elif i==2:
        box(k,p,(4.2,.26,2.80),'steel')
        box(k,(x,y+.16,z),(3.95,.035,2.55),'dark')
        for xx in (-1.65,1.65):
            for zz in (-1.0,1.0):cyl(k,(x+xx,y+.18,z+zz),.11,.08,'silver','y',12)
    elif i==3: chip_pack(k,p)
    elif i==4: photonic_die(k,p)
    elif i==5:
        box(k,p,(4.05,1.08,.08),'navy');box(k,(x,y,z+.055),(3.86,.94,.014),'glass')
        for j,col in enumerate(CH):
            yy=y-.31+j*.22
            pts=[(x-1.7,yy,z+.08),(x-.80,yy,z+.08),(x-.25,yy+.11,z+.08),(x+.35,yy+.11,z+.08),(x+1.68,yy,z+.08)]
            pipe(k,pts,.024,col,9)
            ring(k,(x+.18,yy+.08,z+.11),.11,.085,.02,'z',col,18)
    elif i==8:
        rack(k,p,1.32,1.65,1.15,'cyan')
        for j,col in enumerate(CH):
            optical_fibers(k,(x-.28,y+.22+j*.12,z+.65),(x+.28,y-.15+j*.12,z+.7),[col]*4,.016,True)
    elif i==9: rack(k,(-9.2,2.65,-.2),2.25,3.76,1.44,'amber')
    elif i in (10,11,12,16,17,18,19,22,32,33,34,36,39,40,41,43,45,46,47,48,50,54,55,56,57,58,60,62,63,64,65,66):
        if i in (57,66):rack(k,p,1.50,1.35,.95,'green')
        elif i==58:instrument(k,p,'red',.65,.80)
        elif i in (43,45,46,47):instrument(k,p,'cyan',1.14,.98)
        else:instrument(k,p,'amber' if i in (10,11,12,16,19) else 'blue' if i in (40,45,46,56) else 'cyan')
    elif i in (6,7,13,14,15,23,26,31,35,37,59):
        if i==26:
            box(k,(x,1.12,z),(4.4,.40,3.0),'steel')
            for xx in (-1.75,1.75):
                for zz in (-1.15,1.15):box(k,(x+xx,1.55,z+zz),(.45,.42,.34),'dark')
        elif i==59:
            box(k,p,(2.20,.60,.90),'steel')
            for j in range(4):fan(k,(x-.75+j*.5,y,z+.51),.21,'z')
        else:
            box(k,p,(1.32,.40,1.15),'steel')
            for j,col in enumerate(CH):
                box(k,(x-.45+j*.30,y+.25,z+.44),(.15,.10,.13),col)
                pipe(k,[(x-.60,y+.28,z-.40+(j-1.5)*.16),(x+.57,y+.28,z+.43+(j-1.5)*.12)],.019,col,8)
    elif i in (20,21):
        box(k,p,(1.6,.55,1.35),'navy')
        for j,col in enumerate(CH):
            zz=z-.46+j*.29
            box(k,(x,y+.33,zz),(1.3,.07,.095),'gold')
            ring(k,(x-.28,y+.38,zz),.12,.092,.023,'y',col,28)
            for a in range(9):box(k,(x-.55+a*.13,y+.39,zz+.06),(.035,.02,.035),'cyan')
    elif i in (24,25,49):
        box(k,p,(3.6,.11,1.42) if i==49 else (3.2,.09,1.50),'glass')
        for j,col in enumerate(CH):
            zz=z-.50+j*.31
            pipe(k,[(x-1.28,y+.09,zz),(x-.30,y+.09,zz),(x+.36,y+.09,zz+.06),(x+1.25,y+.09,zz+.06)],.028,col,9)
    elif i in (27,28,29,30):
        # Mesh under its real pivot parent; no duplicate unparented ring.
        pass
    elif i==38:
        box(k,p,(1.52,.40,.74),'navy')
        for j,col in enumerate(CH):
            xx=x-.50+j*.33
            cyl(k,(xx,y+.25,z),.11,.17,'dark','y',20)
            ring(k,(xx,y+.33,z),.12,.077,.025,'y',col,18)
    elif i in (42,44):
        # Spectrometer and eye-chart stations are on distinct upper pylons.
        box(k,(x,y-.78,z-.11),(.19,1.50,.24),'silver')
        box(k,(x,y,z-.08),(2.35,1.36,.16),'steel')
    elif i in (51,52,53):
        box(k,(x,y-.35,z),(.58,.55,.57),'navy')
        for j in range(12):
            box(k,(x-.25+j*.043,y+.02,z+.30),(.027,.15,.024),CH[i-51])
    elif i==61:
        box(k,(x,y-.63,z-.12),(6.5,.20,.40),'steel')
        for xx in (-2.9,2.9):box(k,(xx,y-1.04,z-.28),(.13,.90,.17),'silver')
    elif i==67:
        for j,col in enumerate(CH):
            xp=CH_POS[j]
            pipe(k,[(xp,4.38,-3.1),(xp,4.72,-3.5),(xp,4.98,-3.70)],.026,col,9)
    else: instrument(k,p)
    # Component-specific physical details independent of the pivot meshes.
    if i==9:
        for j,col in enumerate(CH):
            xx=-9.85+(j%2)*1.20;yy=3.15-(j//2)*.65
            cyl(k,(xx,yy,.55),.12,.20,col,'z',24)
            ring(k,(xx,yy,.65),.15,.12,.025,'z','silver',24)
    if i in (27,28,29,30):
        # Huge illuminated WDM resonators, each in an independent animation pivot.
        pass
    if i==42:
        for j in range(5):box(k,(x-1.0+j*.5,y-.56,z+.14),(.028,.11,.018),'silver')
    if i==49:
        # Four electrical chiplet data traffic layers.
        for j,col in enumerate(CH):
            for a in range(12):
                cx=x-1.40+a*.25
                box(k,(cx,y+.13,z-.5+j*.32),(.075,.025,.028),col)
    if i==67:
        # Long display support with a clearly distinct lighting signature.
        for xx in (-3.70,3.70):box(k,(xx,5.54,-4.70),(.12,2.4,.20),'steel')
        for zz in (-4.65,-4.50):box(k,(0,4.31,zz),(7.6,.10,.16),'silver')
    # All declared pivot nodes own geometry. Their transforms are real FBX parents.
    for piv,meta in PV.get(k,{}).items():pivot_geometry(k,piv,meta)
    if i not in (0,1,2,3,4,5,24,25,27,28,29,30,42,44,49,61,67) and M.get(k):
        for j in range(3):
            box(k,(x-.32+j*.30,y-.34,z+.40),(.046,.036,.03),'green' if j==0 else 'amber')

# Export all 68 independently constructed models and a common-origin assembled reference.
for i,k in enumerate(names):
    build(i)
    assert M.get(k),('Empty model',i,k)
    assert {g for _,_,g in M[k] if g}==set(PV.get(k,{})),('Pivot ownership mismatch',i,k)
print('MODELED LEVEL 80',len(names),'models',sum(len(M[k]) for k in names),'parts',sum(sum(len(m.faces) for m,c,g in M[k]) for k in names),'triangles',flush=True)
manifest=[]
for i,k in enumerate(names):
    file=f'{i:02d}_L80_{k}'
    export_fbx(k,MD/(file+'.fbx'))
    export_glb(k,GD/(file+'.glb'))
    tris=sum(len(me.faces) for me,c,g in M[k]);parts=len(M[k]);v=np.vstack([me.vertices for me,c,g in M[k]])
    manifest.append({'index':i,'name':'L80_'+k,'file':file+'.fbx','glb':file+'.glb','triangle_count':tris,'mesh_parts':parts,'bbox_min':np.round(v.min(axis=0),5).tolist(),'bbox_max':np.round(v.max(axis=0),5).tolist(),'rig_groups':list(PV.get(k,{})), 'animation_pivots':{p:list(x['position']) for p,x in PV.get(k,{}).items()}})
    print('EXPORTED',i+1,'/68',file,parts,'parts',tris,'triangles',flush=True)
allkey='Level80_Complete_ArtKit_Showcase'
M[allkey]=[r for key in names for r in M[key]]
PV[allkey]={p:v for group in PV.values() for p,v in group.items()}
export_fbx(allkey,MD/(allkey+'.fbx'))
export_glb(allkey,GD/(allkey+'.glb'))
v=np.vstack([m.vertices for m,c,g in M[allkey]])
bounds=np.stack([v.min(axis=0),v.max(axis=0)])
config={'level_id':'level_80','display_name':'Command the Light','subtitle':'Advanced Four-Channel Silicon Photonic Data Link','individual_models':68,'individual_glbs':68,'assembled_fbx':'Models/'+allkey+'.fbx','assembled_glb':'GLB/'+allkey+'.glb','individual_triangle_count':sum(r['triangle_count'] for r in manifest),'individual_mesh_parts':sum(r['mesh_parts'] for r in manifest),'animation_parent_pivot_groups':50,'planned_player_wires':126,'planned_unique_functional_terminals':194,'workstation_width_m_nominal':23.2,'workstation_world_bounds_m':bounds.round(4).tolist(),'fbx_numeric_units':'centimetres','fbx_unit_scale_factor':1,'glb_numeric_units':'metres','unity_expected_width_m':23.2,'axes':'Y up / X width / Z front','assembly_origin':[0,0,0],'actual_player_wires_included':False,'functional_spark_terminals_included':False,'optical_link_physics_included':False,'six_authoritative_objectives_implemented':False,'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(config,indent=2))
with (ROOT/'Animation_Rigs.csv').open('w',newline='') as f:
    w=csv.writer(f);w.writerow(['Pivot','Model','Kind','Axis','X','Y','Z','Description'])
    for i,p,kind,axis,pos in pivots:w.writerow([p,'L80_'+names[i],kind,axis,*pos,'True FBX/GLB transform parent owning visible independent geometry'])
with (ROOT/'Art_Assembly.csv').open('w',newline='') as f:
    w=csv.writer(f);w.writerow(['Index','File','LocalX','LocalY','LocalZ','Scale','Role'])
    for r in manifest:w.writerow([r['index'],r['file'],0,0,0,1,'Common-origin modular silicon photonics equipment'])
with (ROOT/'Component_Roles.csv').open('w',newline='') as f:
    w=csv.writer(f);w.writerow(['Model','Category','Role','GameplayStatus'])
    for r in manifest:
        i=r['index'];cat='Foundation' if i==0 else 'Photonic core' if i<=8 else 'Laser source' if i<=19 else 'Transmitter' if i<=25 else 'Resonance and WDM' if i<=37 else 'Optical receiver and test' if i<=48 else 'Data and diagnostics' if i<=55 else 'PLC and safety' if i<=59 else 'SCADA and measurement'
        w.writerow([r['name'],cat,r['name'].replace('L80_',''),'Art only; no Unity gameplay component'])
(ROOT/'Material_Palette.json').write_text(json.dumps({k:{'rgba_8bit':c,'surface_intent':'transparent/glass' if c[3]<255 else 'opaque preview material'} for k,c in P.items()},indent=2))
print('LEVEL80 COMPLETE EXPORT',len(M[allkey]),'meshparts',config['individual_triangle_count'],'triangles','bounds',bounds.tolist(),flush=True)
