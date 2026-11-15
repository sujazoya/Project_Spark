#!/usr/bin/env python3
"""Project Spark Level 83: original fault-tolerant mission computing and digital reliability modular geometry, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical or chiplet-bonding gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level83_HD_AAA')
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 83 Fault Tolerant Computing Original Modular Geometry')]),
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
# PROJECT SPARK LEVEL 83 — BEYOND THE SHADOW: ORIGINAL ORBITAL POWER SYSTEM
# Real modular procedural geometry, FBX 7400 and meter-unit GLB.
# No Unity gameplay or functional SparkTerminal is asserted.
# -----------------------------------------------------------------------------
P.update({
 'titanium':(142,156,176,255), 'solar':(26,63,161,255),'solarblue':(26,110,194,255),'solarcell':(19,36,97,255),
 'sun':(251,194,79,255),'space':(13,26,54,255),'battery':(33,208,151,255),
 'batteryblue':(31,145,245,255),'shadow':(95,83,202,255),'hologram':(50,199,250,130),
 'orbitgold':(244,170,66,255),'orbitviolet':(125,76,213,255),
 'metallic':(100,127,155,255),'powerrack':(29,48,76,255),'safety':(251,76,64,255),
 'emissive':(60,245,241,255),'palegold':(246,207,126,255),
 'cellglass':(44,83,158,200),'vacuum':(63,90,139,255)})
raw_names='''OrbitalEnergyQualificationArenaBase|PanoramicOrbitShadowDome|TransparentSpacecraftPowerTestChamber|FourChipletMissionComputerPayload|SpacecraftServiceBusStructure|DeployableSolarWingPort|DeployableSolarWingStarboard|SolarWingPortHingeAssembly|SolarWingStarboardHingeAssembly|SolarArrayPortTrackingGimbal|SolarArrayStarboardTrackingGimbal|SolarArrayAzimuthEncoder|SolarIncidenceVectorProjector|ProgrammableSolarIlluminationArc|EarthShadowOccluderStage|OrbitalDayNightHologram|SolarArrayCellStringMap|PVStringPortIVTracer|PVStringStarboardIVTracer|PVStringBypassDiodeBank|SolarArrayCombinerBox|SolarArrayCurrentSensors|SolarArrayVoltageSensors|MPPTControllerPort|MPPTControllerStarboard|MPPTPerturbObserveDisplay|PVMaximumPowerOperatingPointHologram|PowerConditioningConverterA|PowerConditioningConverterB|RegulatedMainDCBus|BusVoltageTransducer|BusCurrentShuntMonitor|BusBulkCapacitorModule|HighSpeedPowerTransientRecorder|BatteryPackA|BatteryPackB|ProtectedBatteryCellTray|BatteryCellVoltageTapArray|BatteryCurrentHallSensor|BatteryThermalSensorGrid|BatteryManagementController|BatteryBalancingCircuit|BatteryChargeController|BatteryDischargeController|BatteryIsolationContactor|BatteryPrechargeResistorModule|SupercapacitorRideThroughModule|BatteryThermalControlPlate|BatteryRadiatorFinAssembly|PowerDistributionUnit|PriorityLoadSwitchingRack|SafetyEssentialLoadBus|AttitudeControlLoadEmulator|MissionComputerLoadEmulator|OpticalTelemetryLoadEmulator|SciencePayloadLoadEmulator|HeaterThermalLoadEmulator|MissionTaskPowerScheduler|LoadForecastEnergyBudgetAnalyzer|EclipseEntryPredictionConsole|SunlightReturnChargePlanner|ContingencyPowerFaultInjector|SolarGimbalJamSimulator|BatteryCellImbalanceSimulator|OpticalBurstDemandInjector|IndependentUndervoltageProtector|EPSPowerSafetyPLC|MainBusEnergyManagementController|Protected5VControlSupply|OrbitalPowerSCADA|ThreeDimensionalEnergyFlowHologram|MissionSurvivalCertificationBeacon|EmergencyPowerIsolationStation|SpacecraftPowerThermalInspectionCamera'''
names=raw_names.split('|');assert len(names)==74
# Anchors are common-origin scene-space metres. x width, y up, z front.
POS={
0:(0,0,0),1:(0,4.4,-4.15),2:(0,4.35,-1.55),3:(0,4.6,-.1),4:(0,4.35,-.9),
5:(-6.43,5.17,-.85),6:(6.43,5.17,-.85),7:(-2.45,5.04,-.85),8:(2.45,5.04,-.85),
9:(-3.1,4.95,-.85),10:(3.1,4.95,-.85),11:(-2.75,5.20,-.75),12:(-5.85,8.18,-3.80),
13:(-10.5,7.88,-3.4),14:(8.8,7.78,-3.75),15:(0,7.62,-4.45),16:(-6.15,5.6,-.62),
17:(-11.25,2.35,2.85),18:(-9.92,2.35,2.85),19:(-8.6,1.72,3.9),20:(-7.1,2.08,3.65),
21:(-10.5,1.6,3.83),22:(-9.17,1.6,3.83),23:(-10.8,2.15,5.08),24:(-9.45,2.15,5.08),
25:(-7.95,3.48,3.72),26:(-7.0,7.76,-4.35),27:(-8.06,2.04,5.05),28:(-6.8,2.04,5.05),
29:(0,1.58,1.25),30:(-5.28,1.65,5.06),31:(-4.2,1.65,5.06),32:(-3.15,1.65,5.06),33:(-1.95,1.65,5.15),
34:(7.78,2.4,1.3),35:(10.1,2.4,1.3),36:(8.92,1.35,2.42),37:(7.54,3.55,2.10),
38:(9.0,3.55,2.10),39:(10.46,3.55,2.1),40:(7.54,1.58,3.86),41:(9.0,1.58,3.86),
42:(10.45,1.58,3.86),43:(7.58,1.55,5.17),44:(9.02,1.55,5.17),45:(10.45,1.55,5.17),
46:(7.2,2.45,4.22),47:(8.55,1.08,4.1),48:(10.7,1.07,4.08),49:(.6,2.0,3.65),
50:(2.3,2.0,3.72),51:(4.5,1.65,3.75),52:(-1.9,2.3,3.93),53:(-.7,2.3,3.93),
54:(.55,2.3,3.93),55:(1.8,2.3,3.93),56:(3.12,2.3,3.93),57:(-3.9,1.65,5.65),
58:(-2.4,1.65,5.65),59:(-1.0,1.65,5.65),60:(.4,1.65,5.65),61:(2.05,1.60,5.7),
62:(-3.5,5.6,-.85),63:(8.85,4.1,1.45),64:(3.25,1.38,5.72),65:(5.1,1.66,5.35),
66:(10.8,1.65,6.0),67:(5.8,2.35,4.95),68:(-5.5,1.18,5.90),69:(0,7.45,-5.06),
70:(0,6.80,-4.85),71:(0,8.8,-5.10),72:(11.83,1.65,5.9),73:(5.53,4.5,2.2)
}
assert len(POS)==74
pivgroups=[
('Orbital simulation',[
(15,'ANIM_ORBIT_EARTH_TURN','rotation','y'),(13,'ANIM_SUN_VECTOR_ARC','rotation','y'),(14,'ANIM_ECLIPSE_OCCLUDER','translation','x'),(15,'ANIM_ORBIT_TIME_MARKER','rotation','y'),(13,'ANIM_SOLAR_SIMULATOR_YAW','rotation','y'),(13,'ANIM_SOLAR_SIMULATOR_TILT','rotation','x'),(4,'ANIM_MISSION_BUS_ROTATION','rotation','y'),(14,'ANIM_SHADOW_BOUNDARY','translation','x')]),
('Solar array mechanisms',[
(5,'ANIM_PORT_WING_DEPLOY','rotation','z'),(6,'ANIM_STARBOARD_WING_DEPLOY','rotation','z'),(5,'ANIM_PORT_OUTER_PANEL','rotation','y'),(6,'ANIM_STARBOARD_OUTER_PANEL','rotation','y'),(9,'ANIM_PORT_GIMBAL_AZIMUTH','rotation','y'),(9,'ANIM_PORT_GIMBAL_ELEVATION','rotation','x'),(10,'ANIM_STARBOARD_GIMBAL_AZIMUTH','rotation','y'),(10,'ANIM_STARBOARD_GIMBAL_ELEVATION','rotation','x'),(16,'ANIM_PORT_STRING_SCAN','indicator','none'),(16,'ANIM_STARBOARD_STRING_SCAN','indicator','none'),(19,'ANIM_BYPASS_DIODE_DISPLAY','indicator','none'),(20,'ANIM_SOLAR_COMBINER_SHUTTER','translation','y')]),
('MPPT and bus electronics',[
(23,'ANIM_MPPT_PORT_CONTROL','rotation','z'),(24,'ANIM_MPPT_STARBOARD_CONTROL','rotation','z'),(25,'ANIM_MPPT_SWEEP_PROBE','translation','x'),(26,'ANIM_PV_CURVE_HOLOGRAM','indicator','none'),(27,'ANIM_CONVERTER_A_COOLING','rotation','z'),(28,'ANIM_CONVERTER_B_COOLING','rotation','z'),(29,'ANIM_MAIN_BUS_PRECHARGE','translation','x'),(32,'ANIM_BUS_CAPACITOR_DISPLAY','indicator','none'),(33,'ANIM_BUS_TRANSIENT_RECORDER','indicator','none'),(70,'ANIM_ENERGY_FLOW_HOLOGRAM','indicator','none')]),
('Battery storage and protection',[
(36,'ANIM_BATTERY_VAULT_DOOR','rotation','y'),(34,'ANIM_BATTERY_PACK_A_TRAY','translation','z'),(35,'ANIM_BATTERY_PACK_B_TRAY','translation','z'),(41,'ANIM_CELL_BALANCE_SCAN','translation','x'),(37,'ANIM_CELL_VOLTAGE_SCAN','translation','x'),(38,'ANIM_BATTERY_CURRENT_PROBE','translation','y'),(44,'ANIM_BATTERY_CONTACTOR','translation','y'),(45,'ANIM_BATTERY_PRECHARGE_SLIDE','translation','x'),(47,'ANIM_THERMAL_PLATE_VALVE','rotation','y'),(48,'ANIM_RADIATOR_FIN_POSITION','rotation','y'),(46,'ANIM_SUPERCAP_RIDE_THROUGH','indicator','none'),(40,'ANIM_BATTERY_HEALTH_HOLOGRAM','indicator','none')]),
('Mission load management',[
(50,'ANIM_LOAD_PRIORITY_SWITCHBANK','translation','x'),(52,'ANIM_ATTITUDE_LOAD','indicator','none'),(53,'ANIM_COMPUTER_LOAD','indicator','none'),(54,'ANIM_OPTICAL_TELEMETRY_LOAD','indicator','none'),(55,'ANIM_SCIENCE_LOAD','indicator','none'),(56,'ANIM_HEATER_LOAD','indicator','none'),(57,'ANIM_MISSION_SCHEDULER','indicator','none'),(58,'ANIM_BUDGET_FORECAST','indicator','none')]),
('Faults safety and SCADA',[
(62,'ANIM_GIMBAL_JAM_TEST','translation','x'),(63,'ANIM_CELL_IMBALANCE_TEST','translation','x'),(64,'ANIM_BURST_LOAD_TEST','translation','x'),(65,'ANIM_POWER_SAFE_MODE_SWITCH','rotation','z'),(69,'ANIM_EPS_SCADA','indicator','none'),(71,'ANIM_FINAL_CERTIFICATION','indicator','none')])]
pivots=[]
for group,entries in pivgroups:
 for ix,name,kind,axis in entries:pivots.append((ix,name,kind,axis,POS[ix],group))
assert len(pivots)==56 and len(set(v[1] for v in pivots))==56
PV={k:{} for k in names}
for ix,n,kind,axis,p,group in pivots:PV[names[ix]][n]={'kind':kind,'axis':axis,'position':p,'group':group}

def solar_panel(k,p,w=1.55,h=2.7,rows=13,cols=10,side='port'):
    x,y,z=p
    # Engineered industrial panel: carbon sandwich, honeycomb rails, individual cell glass and traces.
    box(k,p,(w,h,.085),'dark')
    box(k,(x,y,z+.054),(w*.96,h*.96,.029),'metallic')
    for yy in (-h*.495,h*.495):box(k,(x,y+yy,z+.07),(w,.076,.12),'gold')
    for xx in (-w*.495,w*.495):box(k,(x+xx,y,z+.07),(.067,h,.12),'gold')
    cw=w*.88/cols;ch=h*.87/rows
    for a in range(cols):
     for b in range(rows):
      xx=x+(a-(cols-1)/2)*cw;yy=y+(b-(rows-1)/2)*ch
      box(k,(xx,yy,z+.095),(cw*.925,ch*.93,.018),'solarcell' if (a+b)%5 else 'solar')
      # visually visible silver grid finger, 1 per cell
      box(k,(xx,yy,z+.110),(.009,ch*.79,.008),'solarblue')
    for a in range(0,cols,3):box(k,(x+(a-(cols-1)/2)*cw,y,z+.114),(.011,h*.84,.009),'silver')
    for yoff in (-h*.51,h*.51):
     for xoff in (-w*.45,w*.45):cyl(k,(x+xoff,y+yoff,z),.065,.13,'steel','z',12)

def solar_wing(k,sign):
    # Extended full-scale solar wing with 4 large individually detailed framed panels.
    # All panel meshes are attached to an actual ANIM wing deploy parent.
    root='ANIM_PORT_WING_DEPLOY' if sign<0 else 'ANIM_STARBOARD_WING_DEPLOY'
    with moving(root):
      for j in range(4):
       xx=sign*(3.65+j*1.67)
       solar_panel(k,(xx,5.25,-.78),1.56,2.78,13,10)
       for yy in (4.0,6.5):box(k,(xx,yy,-.88),(1.47,.10,.14),'steel')
       for off in (-.72,.72):box(k,(xx+off,5.25,-.85),(.078,2.7,.15),'steel')
       for y0 in (4.05,6.45):pipe(k,[(xx-.67,y0,-1.05),(xx+.67,y0,-1.05)],.025,'gold',9)
      pipe(k,[(sign*2.35,5.08,-1.0),(sign*3.0,5.10,-1.0),(sign*9.62,5.1,-1.0)],.11,'titanium',18)
      for j in range(5):
       xx=sign*(2.75+j*1.67)
       cyl(k,(xx,5.08,-.96),.15,.22,'silver','x',18)
    op='ANIM_PORT_OUTER_PANEL' if sign<0 else 'ANIM_STARBOARD_OUTER_PANEL'
    with moving(op):
     cyl(k,(sign*9.75,5.25,-.85),.16,.25,'gold','x',20)
     box(k,(sign*10.0,5.25,-.84),(.22,.38,.21),'metallic')

def board(k,p,scale=1):
 x,y,z=p
 box(k,(x,y,z),(1.58*scale,.14*scale,1.22*scale),'teal')
 for a in range(4):
  dx=(-.38 if a%2==0 else .38)*scale;dz=(-.27 if a<2 else .27)*scale
  box(k,(x+dx,y+.14*scale,z+dz),(.58*scale,.17*scale,.46*scale),'black')
  box(k,(x+dx,y+.24*scale,z+dz),(.48*scale,.015*scale,.36*scale),'metallic')
  for j in range(10):
   box(k,(x+dx-.28*scale+j*.06*scale,y+.135*scale,z+dz+.25*scale),(.028*scale,.04*scale,.04*scale),'gold')
 for i in range(12):
  xx=x+(i-5.5)*.108*scale
  pipe(k,[(xx,y+.078*scale,z-.55*scale),(xx+.04*scale,y+.085*scale,z+.55*scale)],.008,'gold',7)

def big_orbit_ring(k,cen,rx,rz,axis='y',n=100,col='orbitgold',radius=.024):
 x,y,z=cen
 pts=[]
 for i in range(n+1):
  a=2*math.pi*i/n
  pts.append((x+rx*math.cos(a),y,z+rz*math.sin(a)) if axis=='y' else (x+rx*math.cos(a),y+rz*math.sin(a),z))
 pipe(k,pts,radius,col,8)

def cabinet(k,p,w=1.15,h=1.65,d=.75,color='cyan'):
 x,y,z=p
 mechbox(k,p,(w,h,d),'powerrack')
 box(k,(x,y+h*.21,z+d*.52),(w*.78,h*.28,.04),'black')
 box(k,(x,y+h*.21,z+d*.54),(w*.71,h*.23,.015),'blue')
 for j in range(12):
  xx=x+(j-5.5)*w*.057
  box(k,(xx,y+h*.22+math.sin(j*.5)*h*.046,z+d*.56),(.035,.025,.01),color)
 for j in range(5):
  yy=y-h*.28+j*h*.075
  box(k,(x,yy,z+d*.515),(w*.78,h*.037,.03),'metallic')
 for i in range(4):knob(k,x+(i-1.5)*w*.20,y-h*.40,z+d*.54,('green','amber','cyan','red')[i])
 for xx in (-w*.38,w*.38):
  for yy in (-h*.43,h*.42):screw(k,(x+xx,y+yy,z+d*.545),.029,'z')
 terminal_block(k,(x,y-h*.44,z+d*.55),7,accent=color)

def battery_cells(k,p,nx=8,ny=3):
 x,y,z=p
 box(k,(x,y,z),(1.58,.17,.67),'dark')
 for a in range(nx):
  for b in range(ny):
   xx=x+(a-(nx-1)*.5)*.184;zz=z+(b-(ny-1)*.5)*.185
   cyl(k,(xx,y+.20,zz),.067,.40,'batteryblue' if (a+b)%3 else 'battery','y',14)
   cyl(k,(xx,y+.42,zz),.051,.015,'gold','y',12)
   ring(k,(xx,y+.40,zz),.068,.058,.012,'y','silver',12)
 for ix in range(4):box(k,(x+(ix-1.5)*.36,y+.49,z),(.03,.025,.58),'copper')

def pv_analyzer(k,p):
 cabinet(k,p,1.12,1.55,.74,'sun');x,y,z=p
 for q in range(4):
  pipe(k,[(x-.42+q*.10,y+.31,z+.406),(x-.22+q*.11,y+.55,z+.43),(x+.35-q*.07,y+.47,z+.44)],.014,'sun' if q==0 else 'cyan',7)

def power_rack(k,p):
 cabinet(k,p,1.10,1.55,.77,'battery')
 for j in range(5):
  yy=p[1]-.55+j*.20
  for i in range(6):box(k,(p[0]-.39+i*.15,yy,p[2]+.41),(.10,.065,.052),'silver' if (i+j)%5 else 'gold')

def battery_vault(k,p,pack='A'):
 x,y,z=p
 frame(k,(x,y,z),(2.05,2.7,1.42),.095,True)
 for j in range(4):
  battery_cells(k,(x,y-.86+j*.52,z),8,3)
 for xx in (x-.91,x+.91):
  for yy in (-1.10,.0,1.10):box(k,(xx,y+yy,z+.75),(.12,.05,.14),'gold')
 box(k,(x,y+1.32,z+0.74),(1.8,.16,.11),'battery')
 cabinet(k,(x,y-1.00,z+.89),.70,.42,.25,'battery')

def component_detail(k,i,p):
 x,y,z=p
 if i==0:
  # 26.2m wide, high detail futuristic plinth and grid floor
  box(k,(0,.22,0),(26.2,.44,13.25),'navy')
  box(k,(0,.465,0),(25.78,.048,12.83),'metallic')
  for a in np.linspace(-12.3,12.3,25):
   box(k,(float(a),.50,0),(.036,.012,12.30),'dark')
  for zz in np.linspace(-6.08,6.08,13):box(k,(0,.5,float(zz)),(25.4,.012,.028),'dark')
  for xx in (-12.85,12.85):box(k,(xx,.62,0),(.11,.24,12.95),'gold')
  for zz in (-6.43,6.43):box(k,(0,.62,zz),(25.9,.24,.11),'metallic')
  for j in range(30):
   xx=-12.2+j*.84
   lightstrip(k,(xx,.54,6.12),(.25,.025,.035),'cyan' if j%2 else 'amber')
  for zz in (-2.5,1.9):
   box(k,(0,.69,zz),(25.4,.08,.07),'dark')
 elif i==1:
  # Panoramic orbital dome with open front for front-of-scene inspection
  for a in range(29):
   xx=-12.7+a*.905
   box(k,(xx,7.9,-5.75),(.11,1.3,.12),'metallic')
   box(k,(xx,8.58,-5.75),(.085,.085,1.2),'steel')
  for yy in (1.15,3.6,6.5,8.6):box(k,(0,yy,-5.9),(25.6,.13,.15),'metallic')
  for z0 in (-5.7,-3.2,-.4,2.6):box(k,(-12.7,5.15,z0),(.19,7.0,.14),'metallic');box(k,(12.7,5.15,z0),(.19,7.0,.14),'metallic')
  for xx in (-12.7,12.7):box(k,(xx,8.60,-1.20),(.12,.16,9.6),'metallic')
  # orbital horizon radiating arc
  big_orbit_ring(k,(0,7.58,-5.76),6.05,1.04,'z',100,'orbitgold',.036)
  big_orbit_ring(k,(0,7.58,-5.73),4.90,.74,'z',100,'orbitviolet',.025)
  for a in range(21):
   t=2*math.pi*a/21
   box(k,(5.05*math.cos(t),7.58+.75*math.sin(t),-5.7),(.055,.045,.025),'gold' if a%2 else 'cyan')
 elif i==2:
  # Clear partial enclosure allowing front view of 4-chiplet satellite module
  frame(k,(0,4.37,-.95),(5.4,4.25,4.05),.105,True,False)
  box(k,(0,6.57,-.95),(5.55,.22,4.24),'dark')
  for xx in (-2.63,2.63):
   for zz in (-2.8,.8):lightstrip(k,(xx,2.52,zz),(.09,.06,.81),'cyan')
 elif i==3:
  board(k,(0,4.68,.12),1.10)
  for j in range(4):
   xx=(-.43 if j%2==0 else .43);zz=(-.35 if j<2 else .35)
   box(k,(xx,5.07,zz),(.62,.13,.50),'black')
  for a in range(12):
   pipe(k,[(a*.12-.65,4.8,.85),(a*.12-.66,5.15,1.05)],.015,'gold',8)
 elif i==4:
  mechbox(k,(0,4.18,-.90),(3.65,2.3,2.3),'steel')
  for xx in (-1.9,1.9):
   for yy in (3.6,4.0,4.4):box(k,(xx,yy,-.9),(.08,.10,2.22),'gold')
  for j in range(12):
   box(k,(-1.62+j*.30,5.38,-.94),(.12,.06,1.85),'dark')
  for zz in (-1.82,-.05):
   box(k,(0,4.15,zz),(3.25,.05,.042),'cyan')
  with moving('ANIM_MISSION_BUS_ROTATION'):
   ring(k,(0,5.5,-.90),1.10,1.02,.068,'y','silver',40)
   for j in range(8):
    a=j*2*math.pi/8;box(k,(1.03*math.cos(a),5.5,-.9+1.03*math.sin(a)),(.07,.06,.07),'cyan')
 elif i in (5,6):solar_wing(k,-1 if i==5 else 1)
 elif i in (7,8):
  sign=-1 if i==7 else 1
  for j in range(4):
   xx=sign*(2.10+j*.24)
   cyl(k,(xx,5.04,-.9),.33,.16,'metallic','x',32)
   ring(k,(xx,5.04,-.9),.36,.28,.045,'x','gold',28)
  motor(k,(sign*2.20,5.04,-.90),.42)
 elif i in (9,10):
  sign=-1 if i==9 else 1
  mechbox(k,(sign*3.1,5.02,-.9),(.95,.72,.62),'steel')
  with moving('ANIM_PORT_GIMBAL_AZIMUTH' if sign<0 else 'ANIM_STARBOARD_GIMBAL_AZIMUTH'):
   ring(k,(sign*3.1,5.40,-.9),.40,.32,.10,'y','gold',30)
  with moving('ANIM_PORT_GIMBAL_ELEVATION' if sign<0 else 'ANIM_STARBOARD_GIMBAL_ELEVATION'):
   ring(k,(sign*3.1,5.18,-.9),.36,.29,.11,'z','silver',30)
 elif i==11:
  cyl(k,p,.23,.15,'steel','x');ring(k,p,.31,.25,.075,'x','gold',28)
 elif i in (12,13):
  if i==13:
   for j in range(9):
    a=-.8+j*.20
    box(k,(-11.2+math.sin(a)*1.3,7.6+math.cos(a)*.36,-3.7),(.14,.28,.14),'gold')
   ring(k,(-10.5,7.9,-3.5),.64,.59,.07,'z','sun',36)
  else:
   big_orbit_ring(k,p,1.05,.65,'z',80,'sun',.026)
 elif i==14:
  box(k,p,(3.2,1.2,.25),'shadow')
  for a in range(8):box(k,(x-1.4+a*.4,y,z+.15),(.055,1.05,.035),'violet')
 elif i==15:
  cyl(k,(x,y,-4.48),1.18,.065,'blue','z',64)
  big_orbit_ring(k,(x,y,-4.39),1.55,.66,'z',100,'orbitgold',.028)
  big_orbit_ring(k,(x,y,-4.40),1.72,.82,'z',100,'violet',.018)
  with moving('ANIM_ORBIT_EARTH_TURN'):
   ring(k,(x,y,-4.37),1.0,.955,.040,'z','ice',56)
 elif i==16:
  box(k,(x,y,z),(2.0,1.10,.07),'dark')
  for a in range(12):
   for b in range(6):box(k,(x+(a-5.5)*.15,y+(b-2.5)*.15,z+.045),(.11,.12,.014),'solar' if (a+b)%3 else 'solarblue')
 elif i in (17,18):pv_analyzer(k,p)
 elif i==19:
  cabinet(k,p,1.0,1.0,.55,'amber')
  for j in range(12):cyl(k,(x-.36+j*.066,y+.56,z+.1),.027,.14,'gold','y',12)
 elif i==20:
  mechbox(k,p,(1.1,.88,.66),'steel')
  terminal_block(k,(x,y+.46,z+.36),9,'x','gold')
 elif i in (21,22,30,31):
  mechbox(k,p,(.61,.61,.43),'metallic')
  compact_screen(k,(x,y+.10,z+.24),.45,.26,'cyan')
  cyl(k,(x,y-.43,z),.065,.23,'silver','y')
 elif i in (23,24):cabinet(k,p,1.17,1.84,.8,'sun')
 elif i==25:
  giantdisplay(k,p,2.1,1.2)
 elif i==26:
  giantdisplay(k,p,3.05,1.22)
  big_orbit_ring(k,(x,y,z+.14),.66,.34,'z',70,'gold',.020)
 elif i in (27,28):
  power_rack(k,p)
  for a in range(5):pipe(k,[(x-.35+a*.16,y-.85,z),(x-.35+a*.16,y-.98,z+.37)],.021,'copper',8)
 elif i==29:
  for xx in np.linspace(-2.1,2.1,9):
   box(k,(xx,1.48,1.20),(.29,.25,1.36),'copper')
   box(k,(xx,1.65,1.20),(.20,.035,1.19),'gold')
  box(k,(0,1.3,1.24),(4.7,.17,1.55),'powerrack')
 elif i==32:
  mechbox(k,p,(.92,1.03,.68),'dark')
  for a in range(6):cyl(k,(x-.32+a*.126,y+.15,z+.39),.057,.38,'blue','y',18)
 elif i==33:
  cabinet(k,p,1.0,1.35,.7,'cyan')
 elif i in (34,35):battery_vault(k,p,'A' if i==34 else 'B')
 elif i==36:
  frame(k,p,(1.8,1.1,1.2),.05,True)
  with moving('ANIM_BATTERY_VAULT_DOOR'):box(k,(x,y,z+.66),(1.58,.90,.035),'glass')
 elif i==37:
  for a in range(16):
   xx=x+(a-7.5)*.08
   box(k,(xx,y,z),(.046,.43,.25),'steel')
   box(k,(xx,y+.23,z),(.032,.07,.26),'gold')
 elif i==38:
  cyl(k,p,.3,.33,'steel','y',26)
  ring(k,p,.35,.28,.09,'y','gold',26)
 elif i==39:
  for u in range(4):
   multisensor(k,x-.56+u*.38,y,z,'temperature',.23)
 elif i in (40,41,42,43,45):
  cabinet(k,p,1.05,1.4,.78,'battery')
 elif i==44:
  mechbox(k,p,(1.05,.72,.8),'black')
  cyl(k,(x,y+.44,z),.18,.24,'copper','y',22)
  with moving('ANIM_BATTERY_CONTACTOR'):box(k,(x,y+.17,z+.43),(.55,.10,.09),'gold')
 elif i==46:
  mechbox(k,p,(1.30,.52,.68),'metallic')
  for a in range(8):cyl(k,(x-.51+a*.146,y+.35,z),.063,.54,'batteryblue','y',14)
 elif i==47:
  box(k,p,(1.2,.25,.8),'metallic')
  for a in range(6):pipe(k,[(x-.55+a*.2,y+.18,z-.34),(x-.55+a*.2,y+.18,z+.34)],.027,'cyan',8)
 elif i==48:
  for u in range(15):box(k,(x-.68+u*.098,y,z),(.026,.78,.58),'silver')
  box(k,(x,y-.46,z),(1.51,.15,.76),'dark')
 elif i in (49,50):
  cabinet(k,p,1.6,1.8,.76,'cyan')
  for u in range(4):
   xx=x+(u-1.5)*.34
   box(k,(xx,y-.25,z+.42),(.26,.36,.052),('green','amber','cyan','blue')[u])
 elif i==51:
  box(k,p,(1.7,.65,1.06),'dark')
  for a in range(8):box(k,(x-.65+a*.19,y+.33,z+.35),(.11,.08,.14),'green')
 elif 52<=i<=56:
  cabinet(k,p,1.02,1.34,.68,'battery' if i<54 else 'cyan')
 elif 57<=i<=61:
  cabinet(k,p,1.12,1.48,.74,'sun' if i in (59,60) else 'cyan')
 elif 62<=i<=64:
  mechbox(k,p,(1.03,.52,.62),'dark')
  for a in range(9):box(k,(x-.42+a*.104,y+.32,z+.10),(.067,.06,.14),'gold' if a%2 else 'red')
 elif i==65:
  cabinet(k,p,1.07,1.5,.77,'red')
 elif i in (66,67,68):
  cabinet(k,p,1.09,1.66,.76,'cyan' if i==68 else 'amber')
 elif i==69:
  giantdisplay(k,p,6.1,1.83)
 elif i==70:
  big_orbit_ring(k,p,2.16,.85,'z',100,'cyan',.045)
  for u in range(12):
   a=math.pi*2*u/12
   box(k,(x+2.17*math.cos(a),y+.85*math.sin(a),z+.06),(.06,.07,.04),'gold' if u%2 else 'blue')
 elif i==71:
  ring(k,p,.49,.41,.08,'z','gold',48)
  cyl(k,p,.33,.065,'green','z',32)
 elif i==72:
  mechbox(k,p,(.84,.98,.63),'steel')
  cyl(k,(x,y+.14,z+.38),.19,.12,'red','z',24)
  ring(k,(x,y+.14,z+.45),.21,.18,.06,'z','gold',24)
 elif i==73:
  mechbox(k,p,(.82,.63,.88),'metallic')
  ring(k,(x,y,z+.51),.32,.25,.13,'z','dark',32)
  cyl(k,(x,y,z+.55),.22,.10,'red','z',24)
 else:
  cabinet(k,p)

def pivot_geometry(k,n,meta):
 x,y,z=meta['position'];kind=meta['kind'];name=n.upper()
 with moving(n):
  if 'PORT_WING_DEPLOY' in name or 'STARBOARD_WING_DEPLOY' in name:
   return # main mesh is already under this group
  elif 'OUTER_PANEL' in name or 'GIMBAL_' in name or 'HINGE' in name:
   ring(k,(x,y,z),.32,.23,.12,'x' if 'OUTER' in name else 'y','silver',30)
   for a in range(6):
    ang=2*math.pi*a/6
    cyl(k,(x+.25*math.cos(ang),y+.24*math.sin(ang),z+.06),.027,.024,'gold','z',10)
  elif 'ORBIT_EARTH' in name:
   ring(k,(x,y,z),.69,.64,.035,'z','ice',40)
  elif 'SUN_VECTOR' in name or 'SHADOW' in name or 'ECLIPSE' in name:
   big_orbit_ring(k,(x,y,z),.50,.25,'z',56,'sun' if 'SUN' in name else 'violet',.028)
  elif 'CELL_BALANCE' in name or 'CELL_VOLTAGE' in name or 'SCAN' in name:
   box(k,(x,y,z),(.40,.15,.55),'silver')
   for a in range(6):cyl(k,(x-.16+a*.065,y+.09,z+.13),.027,.035,'battery','y',9)
  elif 'BATTERY_PACK_A_TRAY' in name or 'BATTERY_PACK_B_TRAY' in name:
   box(k,(x,y-.22,z),(1.55,.15,.82),'metallic')
  elif 'VAULT_DOOR' in name:
   box(k,(x,y,z+.2),(1.30,1.10,.045),'glass')
  elif 'CONTactor' in name or 'PRECHARGE' in name or 'SHUTTER' in name or 'SAFE_MODE' in name:
   box(k,(x,y,z),(.57,.20,.44),'gold')
  elif 'COOLING' in name or 'RADIATOR' in name:
   fan(k,(x,y,z),.27,'z')
  elif 'GIMBAL_JAM' in name or 'IMBALANCE' in name or 'BURST_LOAD' in name:
   box(k,(x,y,z+.24),(.42,.19,.18),'red')
  elif 'EStop' in name:
   cyl(k,(x,y,z+.25),.13,.10,'red','z')
  elif kind=='indicator':
   compact_screen(k,(x,y,z+.06),.64,.44,'cyan' if 'LOAD' in name else 'blue')
  elif kind=='rotation':
   ring(k,(x,y,z),.24,.17,.12,'z','gold',26)
   box(k,(x+.16,y,z+.1),(.16,.04,.08),'cyan')
  elif kind=='translation':
   box(k,(x,y,z),(.55,.23,.45),'metallic')
   for a in range(4):box(k,(x-.20+a*.13,y+.145,z),(.062,.04,.36),'cyan')
  else:compact_screen(k,(x,y,z+.04),.53,.32,'cyan')

# 212 visible socket meshes with stable source positions.
ELIGIBLE=[i for i in range(5,74) if i not in (12,26,70)]
counts={i:3 for i in ELIGIBLE}
weighted=[23,24,27,28,29,30,31,34,35,37,38,39,40,41,42,43,44,49,50,57,58,59,60,65,66,67,68,72]
q=0
while sum(counts.values())<212:
 i=weighted[q%len(weighted)];counts[i]+=1;q+=1
assert sum(counts.values())==212
SOCKETS=[]
def add_sockets(k,i):
 n=counts.get(i,0)
 if not n:return
 # Based on visual bounds, front-facing socket panel; avoid failing with 0 geometry.
 pieces=M[k];verts=np.vstack([m.vertices for m,_,_ in pieces]);mn=verts.min(0);mx=verts.max(0)
 cx=float((mn[0]+mx[0])/2);cy=float((mn[1]+mx[1])/2);z=float(mx[2]+.058)
 w=max(.4,float(mx[0]-mn[0]));h=max(.38,float(mx[1]-mn[1]))
 cols=min(5,max(2,math.ceil(n/2)))
 for j in range(n):
  a=j%cols;b=j//cols;rows=math.ceil(n/cols)
  xx=cx+(a-(cols-1)*.5)*min(.17,w*.73/max(1,cols-1))
  yy=cy+(b-(rows-1)*.5)*min(.16,h*.46)
  center=(round(xx,5),round(yy,5),round(z,5))
  cyl(k,center,.048,.042,'dark','z',12)
  ring(k,(xx,yy,z+.022),.061,.042,.021,'z','gold',16)
  cyl(k,(xx,yy,z+.045),.025,.013,'cyan' if j%3 else 'amber','z',12)
  SOCKETS.append((i,center,j))

for i,k in enumerate(names):
 component_detail(k,i,POS[i])
 for piv,meta in PV[k].items():pivot_geometry(k,piv,meta)
 assert M.get(k),(i,k)
 assert {grp for m,c,grp in M[k] if grp}==set(PV[k]),('pivot mesh not populated',i,k)
 add_sockets(k,i)

print('MODELED',len(names),'parts',sum(len(M[k]) for k in names),'tris',sum(len(mesh.faces) for k in names for mesh,c,g in M[k]),'sockets',len(SOCKETS),flush=True)
manifest=[]
for i,k in enumerate(names):
 prefix=f'{i:02d}_L83_{k}'
 export_fbx(k,MD/(prefix+'.fbx'))
 export_glb(k,GD/(prefix+'.glb'))
 pieces=M[k];coords=np.vstack([m.vertices for m,c,g in pieces])
 row={'index':i,'name':'L83_'+k,'file':prefix+'.fbx','glb':prefix+'.glb','triangle_count':sum(len(m.faces) for m,c,g in pieces),'mesh_parts':len(pieces),'bbox_min':coords.min(0).round(5).tolist(),'bbox_max':coords.max(0).round(5).tolist(),'rig_groups':list(PV[k]),'animation_pivots':{p:list(meta['position']) for p,meta in PV[k].items()},'visual_terminal_sockets':counts.get(i,0)}
 manifest.append(row)
 print('EXPORTED',f'{i+1}/74',prefix, row['triangle_count'],'tris',row['mesh_parts'],'parts',flush=True)
assembled='Level83_Complete_ArtKit_Showcase'
M[assembled]=[rec for k in names for rec in M[k]]
PV[assembled]={p:meta for k in names for p,meta in PV[k].items()}
export_fbx(assembled,MD/(assembled+'.fbx'))
export_glb(assembled,GD/(assembled+'.glb'))
coords=np.vstack([m.vertices for m,c,g in M[assembled]]);bounds=np.stack([coords.min(0),coords.max(0)])
config={'level_id':'level_83','display_name':'Beyond the Shadow','subtitle':'Advanced Orbital Power and Eclipse Survival','individual_models':74,'individual_glbs':74,'assembled_fbx':'Models/'+assembled+'.fbx','assembled_glb':'GLB/'+assembled+'.glb','individual_triangle_count':sum(r['triangle_count'] for r in manifest),'individual_mesh_parts':sum(r['mesh_parts'] for r in manifest),'animation_parent_pivot_groups':56,'planned_player_wires':138,'planned_unique_functional_terminals':212,'modeled_art_only_terminal_sockets':len(SOCKETS),'workstation_width_m_nominal':26.2,'workstation_world_bounds_m':bounds.round(5).tolist(),'fbx_numeric_units':'centimetres','fbx_unit_scale_factor':1,'glb_numeric_units':'metres','axes':'Y up / X width / Z front','assembly_origin':[0,0,0],'actual_player_wires_included':False,'functional_spark_terminals_included':False,'orbital_eps_solver_included':False,'six_authoritative_objectives_implemented':False,'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(config,indent=2))
with (ROOT/'Animation_Rigs.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Pivot','Model','Kind','Axis','X','Y','Z','Group','Description'])
 for i,n,kind,axis,p,group in pivots:w.writerow([n,'L83_'+names[i],kind,axis,*p,group,'Real pivot parent in binary FBX and GLB geometry'])
with (ROOT/'Art_Assembly.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Index','File','LocalX','LocalY','LocalZ','Scale','Role'])
 for r in manifest:w.writerow([r['index'],r['file'],0,0,0,1,'Origin-aligned orbital EPS modular art'])
with (ROOT/'Component_Roles.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Model','Category','Function','UnityGameplayStatus'])
 for i,r in enumerate(manifest):
  category='Orbital theater' if i<=4 else 'Deployable solar wings' if i<=16 else 'Solar PV and MPPT' if i<=28 else 'Main DC bus' if i<=33 else 'Battery and BMS' if i<=48 else 'Power distribution and mission loads' if i<=60 else 'Faults and EPS safety'
  w.writerow([r['name'],category,r['name'].replace('L83_',''),'ART ONLY: gameplay physics and SparkTerminal not implemented'])
with (ROOT/'Visual_Terminal_Sockets_212.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['AnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','UnityStatus'])
 for j,(i,p,idx) in enumerate(SOCKETS,1):w.writerow([f'P{j:03d}','L83_'+names[i],*p,'Gold-rimmed player-wire socket visual','ART_ANCHOR_NOT_SPARKTERMINAL'])
(ROOT/'Material_Palette.json').write_text(json.dumps({k:{'rgba_8bit':list(v),'surface_intent':'Transparent preview' if v[3]<255 else 'Opaque preview'} for k,v in P.items()},indent=2))
print('COMPLETE LEVEL 83 GEOMETRY',len(manifest),len(pivots),len(SOCKETS),bounds.tolist(),flush=True)
