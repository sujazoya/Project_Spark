#!/usr/bin/env python3
"""Project Spark Level 74: procedural original photolithography 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT electrical CMP gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level74_HD_AAA')
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


# ---------------------------------------------------------------
# LEVEL 74: ALIGN THE LIGHT — 56 distinct independently exportable
# photolithography equipment modules, common world-space coordinates.
# Geometric and animation-reference art only. No gameplay physics.
# X width, Y up, Z front. Workstation: 18.2m across.
# ---------------------------------------------------------------
names=[
'IndustrialLithographyWorkstationBase','TransparentLithographySafetyEnclosure','AutomatedPhotoresistTrack','PhotoresistSpinCoatingBowl','PrecisionSpinChuckDrive','VirtualPhotoresistSupplyCartridge','PhotoresistMeteringPump','PrecisionPhotoresistNozzle','ResistThicknessMeasurementSensor','SoftBakeHotplateModule','SoftBakeTemperatureSensor','SiliconWaferCarrierCassette','AutomatedWaferHandlingRobot','WaferPrealignmentStation','PrecisionProjectionScannerFrame','VirtualDUVLightSource','OpticalIlluminationHomogenizer','HighSpeedExposureShutter','PrecisionReticleStorageCassette','HighResolutionCircuitReticle','PrecisionReticleMotionStage','ReticleStageLinearEncoder','ProjectionOpticsLensBarrel','ActiveOpticalFocusCorrector','NumericalApertureSelector','NanometreWaferXYStage','DualAxisLinearServoMotorAssembly','WaferZTiltFocusStage','WaferStageLaserInterferometer','WaferAlignmentMarkCamera','ElectrostaticWaferChuck','PlanarizedSiliconWafer','WaferOverlayAlignmentMarks','PhotosensitiveResistLayer','WaferTopographyFocusMap','WaferHeightLevelingSensor','ActiveVibrationIsolationPlatform','SixAxisVibrationMeasurementSensor','ProjectionLensThermalDriftSensor','ExposureDosePhotodiode','PrecisionScanSlitAssembly','LithographyEmergencySafetyStation','PostExposureBakeModule','PostExposureBakeTemperatureSensor','VirtualPhotoresistDeveloperModule','DeveloperDispensingNozzle','DeveloperFlowTransmitter','WaferRinseSpinDryer','CriticalDimensionInspectionScanner','OverlayAlignmentMetrologySystem','ReticleDefectInspectionModule','PhotoresistPatternDefectCamera','LithographyProcessPLCRack','StageSynchronizationController','HolographicLithographySCADAMonitor','Regulated5VControlSupply']
assert len(names)==56
PV={}
def register(index,pivot,position,kind='rotational',axis='y'):
    PV.setdefault(names[index],{})[pivot]=dict(position=tuple(position),kind=kind,axis=axis)

# Additional precision lithography primitives.
def ledrail(k,p,length,axis='x',col='cyan'):
    if axis=='x':box(k,p,(length,.035,.042),col)
    elif axis=='z':box(k,p,(.042,.035,length),col)
    else:box(k,p,(.04,length,.04),col)

def opticsbarrel(k,c,r=1.05,h=1.45):
    x,y,z=c
    sscylinder(k,c,r,h,'y',18)
    for yy in np.linspace(y-h*.46,y+h*.46,7):
        precision_ring(k,(x,yy,z),r*1.075,r*.94,.07,'y','silver')
        precision_ring(k,(x,yy+.025,z),r*.94,r*.87,.026,'y','dark')
    for rr in (.18,.38,.65,.78):
        ring(k,(x,y-h*.44,z),rr,rr-.03,.028,'y','violet',48)
    for a in np.linspace(0,2*math.pi,12,endpoint=False):
        xx=x+(r+.07)*math.cos(a);zz=z+(r+.07)*math.sin(a)
        box(k,(xx,y,zz),(.07,h*.9,.06),'steel')
        for hh in (y-h*.39,y+h*.39):screw(k,(xx,hh,zz),.024,'y')

def chiptraces(k,center,w=.80,d=.80,level=.012,rows=11,color='circuit'):
    x,y,z=center
    for i in range(rows):
        u=(i-(rows-1)/2)*w/(rows+1)
        a=(i*7)%rows
        z0=z-d*.44+(a+.5)*d*.88/rows
        z1=z+d*.42-(i%3)*.035
        pts=[(x+u,y,z0),(x+u,y,z1)]
        pipe(k,pts,.006+level*.25,color,6)
        box(k,(x+u,y+.002,z1),(.035,.013,.035),'silver')
        if i%2==0:
            box(k,(x+u+w*.06,y,z0),(.10,.008,.008),color)
    for i in range(7):
        xx=x-w*.35+(i+.5)*w*.70/7
        zz=z-d*.19+math.sin(i*1.7)*d*.16
        box(k,(xx,y+.003,zz),(.032,.012,.055),'cyan')

def mirrorhead(k,p,r=.22,axis='y'):
    cyl(k,p,r,.14,'steel',axis,28)
    ring(k,p,r*1.08,r*.86,.055,axis,'silver',32)
    c=list(p); c[1]+=.083 if axis=='y' else 0
    cyl(k,c,r*.78,.022,'violet',axis,28)
    bolt_circle(k,(p[0],p[1]+.1,p[2]),r*.83,8,'dark')

def precision_stage(k,p,ext=(2.5,.25,2.2),color='steel'):
    x,y,z=p;w,h,d=ext
    mechbox(k,p,ext,'dark')
    box(k,(x,y+h*.52,z),(w*.93,.075,d*.93),color)
    for dz in (-d*.36,d*.36):
        box(k,(x,y+h*.69,z+dz),(w*.88,.065,.12),'silver')
        ledrail(k,(x,y+h*.73,z+dz),w*.84,'x','cyan')
    for dx in (-w*.36,w*.36):
        box(k,(x+dx,y+h*.71,z),(.09,.06,d*.85),'silver')
        ledrail(k,(x+dx,y+h*.76,z),d*.8,'z','blue')
    for sx in (-1,1):
        for sz in (-1,1):
            screw(k,(x+sx*w*.4,y+h*.75,z+sz*d*.39),.052,'y')

def lighttower(k,p,height=2.2,r=.52):
    x,y,z=p
    cyl(k,(x,y,z),r,height,'steel','y',32)
    for yy in np.linspace(y-height*.42,y+height*.42,9):
        precision_ring(k,(x,yy,z),r*1.08,r*.95,.045,'y','silver')
    for j in range(12):
        aa=j*2*math.pi/12
        box(k,(x+(r+.05)*math.cos(aa),y,z+(r+.05)*math.sin(aa)),(.055,height*.85,.042),'dark')

# 00 — Full precision lithography lab chassis with bolted rails, work areas, sockets and perimeter LEDs.
k=names[0]
tier(k,0,.33,0,18.20,.57,9.50,'navy')
box(k,(0,.65,0),(17.77,.12,9.12),'dark')
for z in (-4.43,4.43):
    box(k,(0,.75,z),(17.97,.13,.16),'steel')
    ledrail(k,(0,.835,z),17.8,'x','cyan')
for x in (-8.85,8.85):
    box(k,(x,.76,0),(.15,.13,9.17),'silver')
    for z in np.linspace(-4.25,4.25,22):screw(k,(x,.838,z),.040)
for x in np.linspace(-8.5,8.5,40):
    cyl(k,(x,.728,4.37),.025,.024,'green' if int(x*8)%4 else 'red','y',9)
for x in (-7.9,-4.3,0,4.3,7.9):
    for z in (-3.9,3.9):box(k,(x,-.07,z),(1.02,.23,.68),'steel')
for x in (-8.2,8.2):
    for z in (-3.7,3.7):screw(k,(x,.85,z),.11)

# 01 — Safety enclosure around optical scanner, with removable glass and illuminated guard posts.
k=names[1]
frame(k,(0,2.95,-.73),(6.55,4.25,6.76),beam=.13,glass=True)
for x in (-3.2,3.2):
    for z in (-3.9,2.35):ledrail(k,(x,3.0,z),3.6,'y','cyan')
for z in (-4.02,2.55):
    for y in (1.02,4.98):ledrail(k,(0,y,z),6.08,'x','cyan')
for x in (-2.43,-.81,.81,2.43):
    box(k,(x,5.08,-.73),(.12,.16,6.2),'steel')
for x in (-3,3):
    box(k,(x,3.25,2.60),(.2,.49,.08),'navy')
    for j in range(3):cyl(k,(x,3.08+j*.13,2.66),.035,.023,('green','amber','red')[j],'z',12)

# 02 — Photoresist processing track: lateral slots, wafer tracks, sensor/transfer stages.
k=names[2]
tier(k,-6.35,.93,-.95,3.48,.54,5.20,'navy')
for z in (-3.10,-1.50,.45,1.10):
    box(k,(-6.35,1.35,z),(3.12,.13,.28),'silver')
    for x in np.linspace(-7.62,-5.06,14):
        cyl(k,(x,1.42,z),.042,.21,'steel','y',9)
for z in (-3.22,1.18):ledrail(k,(-6.35,1.53,z),3.17,'x','cyan')
for x in (-7.88,-4.83):
    box(k,(x,1.20,-.95),(.14,.47,4.70),'steel')
for i in range(9):
    box(k,(-7.55+i*.30,.96,-3.47),(.19,.17,.14),'light')

# 03 — Photoresist bowl: nested concentric bowls, drain channels and splash-shield rings.
k=names[3]
register(3,'ANIM_RESIST_SPIN_BOWL',(-6.86,1.65,-1.63),'visual','y')
mechbox(k,(-6.86,1.13,-1.63),(2.18,.41,2.23),'navy')
for r,yy,col in [(1.01,1.46,'steel'),(.91,1.49,'light'),(.81,1.55,'dark'),(.67,1.59,'steel')]:
    ring(k,(-6.86,yy,-1.63),r,r-.075,.18,'y',col,48)
with moving('ANIM_RESIST_SPIN_BOWL'):
    axial(k,(-6.86,1.65,-1.63),.64,.11,'rubber',52)
    waferdisc(k,(-6.86,1.75,-1.63),.49,.022,'iridescent',True)
for a in np.linspace(0,2*math.pi,16,endpoint=False):
    x=-6.86+1.05*math.cos(a);z=-1.63+1.05*math.sin(a)
    screw(k,(x,1.66,z),.035)
pipe3(k,[(-6.75,1.06,-2.46),(-6.75,.78,-2.44),(-6.05,.78,-2.32)],.035,'steel')

# 04 — Coater spin chuck motor: modeled shaft stack, fins and rotating drive coupling.
k=names[4]
register(4,'ANIM_RESIST_SPIN_CHUCK',(-6.86,1.19,-1.63))
mechbox(k,(-6.86,.75,-1.63),(1.02,.20,.99),'dark')
with moving('ANIM_RESIST_SPIN_CHUCK'):
    sscylinder(k,(-6.86,1.03,-1.63),.30,.55,'y',14)
    cyl(k,(-6.86,1.37,-1.63),.13,.16,'copper','y',24)
for i in range(13):
    box(k,(-7.27+i*.068,.92,-1.07),(.018,.37,.15),'silver')

# 05 — Photoresist cartridge, fluorescent fluid window, gauge, piping and service cage.
k=names[5]
cylvessel(k,-8.05,1.78,.68,.36,1.54,'resist')
for i in range(12):
    h=1.28+i*.065
    box(k,(-7.70,h,.68),(.025,.025,.48),'violet' if i%2 else 'cyan')
gauge(k,(-8.05,2.50,1.05),.17)
pipe3(k,[(-8.05,2.55,.68),(-8.05,2.84,.68),(-7.0,2.84,.55)],.046,'steel')

# 06 — Metering pump with radial fin pack, drive shaft, transparent line and gauge.
k=names[6]
register(6,'ANIM_RESIST_PUMP',(-7.48,.99,2.28))
mechbox(k,(-7.48,.77,2.28),(1.18,.22,.79),'navy')
with moving('ANIM_RESIST_PUMP'):
    sscylinder(k,(-7.48,1.03,2.28),.22,.60,'x',12)
    cyl(k,(-7.1,1.03,2.28),.095,.18,'copper','x',18)
for yy in (.94,1.12):pipe3(k,[(-7.99,yy,2.28),(-8.2,yy,2.28)],.036,'resist')
gauge(k,(-7.48,1.37,2.68),.12)

# 07 — Articulated resist dispense arm and needle with transparent modeled droplet.
k=names[7]
register(7,'ANIM_RESIST_NOZZLE',(-6.63,2.23,-1.59),'angular','y')
mechbox(k,(-5.14,1.66,-1.61),(.52,1.42,.55),'navy')
with moving('ANIM_RESIST_NOZZLE'):
    angular_arm(k,(-5.14,2.34,-1.61),(-5.86,2.46,-1.60),.11,'steel')
    angular_arm(k,(-5.86,2.46,-1.60),(-6.61,2.39,-1.60),.085,'silver')
    cyl(k,(-6.63,2.21,-1.59),.12,.36,'steel','y',18)
    cyl(k,(-6.63,2.01,-1.59),.035,.14,'silver','y',12)
    cyl(k,(-6.63,1.88,-1.59),.025,.048,'resist','y',12)
pipe3(k,[(-5.15,2.59,-1.62),(-5.8,2.67,-1.61),(-6.64,2.55,-1.61)],.027,'violet')

# 08 — High resolution resist thickness sensor with movable optical probe and LED bank.
k=names[8]
register(8,'ANIM_RESIST_THICKNESS_PROBE',(-5.31,2.24,.15),'linear','y')
stand(k,-5.31,.95,.15,1.12)
with moving('ANIM_RESIST_THICKNESS_PROBE'):
    sensor(k,(-5.31,2.27,.15),'violet',True)
    cyl(k,(-5.31,1.95,.15),.09,.22,'silver','y',20)
    cyl(k,(-5.31,1.79,.15),.053,.10,'violet','y',18)
for j in range(9):box(k,(-5.42+j*.027,2.57,.25),(.018,.018,.035),'cyan')

# 09 — Precision soft-bake hotplate with glowing heating coils and wafer rests.
k=names[9]
register(9,'ANIM_SOFTBAKE_LIFT',(-5.34,1.53,1.68),'linear','y')
mechbox(k,(-5.34,.98,1.68),(2.12,.33,1.81),'dark')
with moving('ANIM_SOFTBAKE_LIFT'):
    axial(k,(-5.34,1.42,1.68),.77,.10,'ceramic',48)
    for rr in (.21,.37,.56,.70):ring(k,(-5.34,1.52,1.68),rr,rr-.018,.020,'y','warm',48)
    for a in np.linspace(0,2*math.pi,8,endpoint=False):
        x=-5.34+.67*math.cos(a);z=1.68+.67*math.sin(a)
        cyl(k,(x,1.57,z),.033,.13,'silver','y',12)
    waferdisc(k,(-5.34,1.63,1.68),.58,.028,'iridescent',False)
box(k,(-5.34,.97,2.62),(1.84,.30,.23),'navy')
for i in range(14):box(k,(-6.13+i*.12,1.10,2.75),(.074,.045,.022),'amber')

# 10 — Hotplate thermocouple with sensor body and inserted temperature stem.
k=names[10]
sensor(k,(-4.09,1.48,1.70),'amber',True)
pipe3(k,[(-4.09,1.25,1.70),(-4.23,1.14,1.67),(-4.67,1.22,1.70)],.030,'copper')

# 11 — Wafer carrier cassette; individual wafer discs and indexed storage rails.
k=names[11]
frame(k,(-8.05,1.68,-3.05),(1.58,1.75,1.65),beam=.09,glass=False)
for n in range(18):
    y=1.03+n*.071
    cyl(k,(-8.05,y,-3.05),.58,.015,'iridescent','y',28)
    box(k,(-8.05,y,-2.35),(1.36,.026,.09),'steel')
for x in (-8.70,-7.40):box(k,(x,1.67,-2.25),(.06,1.45,.045),'cyan')

# 12 — Robotic transfer arm with pivoted articulated subassemblies and tool gripper.
k=names[12]
register(12,'ANIM_WAFER_ROBOT_BASE',(-4.55,1.06,.03))
register(12,'ANIM_WAFER_ROBOT_ARM',(-4.26,1.85,.03),'angular','y')
register(12,'ANIM_WAFER_ROBOT_TOOL',(-3.40,1.98,.04),'angular','y')
mechbox(k,(-4.55,.83,.03),(.90,.30,.90),'navy')
with moving('ANIM_WAFER_ROBOT_BASE'):
    axial(k,(-4.55,1.08,.03),.42,.19,'silver',36)
    cyl(k,(-4.55,1.47,.03),.24,.60,'steel','y',24)
with moving('ANIM_WAFER_ROBOT_ARM'):
    angular_arm(k,(-4.55,1.84,.03),(-3.86,2.03,.06),.13,'light')
    cyl(k,(-4.55,1.87,.03),.21,.17,'steel','y',20)
with moving('ANIM_WAFER_ROBOT_TOOL'):
    box(k,(-3.37,2.01,.05),(.78,.075,.16),'silver')
    for z in (-.15,.25):box(k,(-3.03,2.02,z),(.30,.05,.055),'light')
    ring(k,(-3.28,2.06,.05),.33,.29,.02,'y','cyan',28)

# 13 — Prealigner rotary chuck with CCD cameras and circumference marks.
k=names[13]
register(13,'ANIM_WAFER_PREALIGNER',(-3.60,1.23,1.43))
mechbox(k,(-3.60,.91,1.43),(1.44,.24,1.40),'navy')
with moving('ANIM_WAFER_PREALIGNER'):
    axial(k,(-3.60,1.19,1.43),.58,.10,'steel',40)
    waferdisc(k,(-3.60,1.27,1.43),.51,.028,'iridescent',True)
for x in (-4.29,-2.93):
    box(k,(x,1.59,1.40),(.15,.52,.19),'steel')
    mirrorhead(k,(x,1.75,1.40),.12)

# 14 — Monumental precision scanner frame, gantry rails, thermal panels, foundation supports.
k=names[14]
for x in (-2.58,2.58):
    tier(k,x,2.95,-1.04,.65,4.25,5.02,'steel')
    for z in (-3.26,1.20):
        box(k,(x,3.06,z),(.36,3.72,.22),'dark')
        ledrail(k,(x,3.10,z+.14),3.55,'y','violet')
# Open-frame optical gantry: no solid upper roof obscuring the photolithography lens/source.
box(k,(0,1.24,-1.04),(5.65,.26,5.10),'steel')
for y in (4.53,5.22):
    for z in (-3.40,1.30):
        box(k,(0,y,z),(5.72,.27,.33),'steel')
        for x in np.linspace(-2.32,2.32,10):screw(k,(x,y+.16,z),.036)
    for x in (-2.55,2.55):box(k,(x,y,-1.04),(.29,.27,4.75),'steel')
for z in (-3.40,1.30):
    box(k,(0,4.81,z),(5.75,.23,.20),'navy')
for x in (-1.9,-.96,0,.96,1.9):
    box(k,(x,5.30,-1.04),(.22,.49,4.40),'dark')
for y in np.linspace(1.5,5.0,10):
    for x in (-2.60,2.60):
        box(k,(x,y,1.54),(.095,.043,.11),'cyan' if int(y*10)%3==0 else 'silver')

# 15 — Simulated DUV illumination source with optically emissive central aperture.
k=names[15]
register(15,'ANIM_DUV_SOURCE_IRIS',(0,5.16,-.73),'angular','y')
lighttower(k,(0,5.05,-.73),1.10,.61)
with moving('ANIM_DUV_SOURCE_IRIS'):
    for rr in (.18,.33,.49):ring(k,(0,4.56,-.73),rr,rr-.030,.035,'y','violet',48)
    cyl(k,(0,4.52,-.73),.26,.09,'uv','y',32)
for x in (-.88,.88):
    box(k,(x,5.08,-.73),(.23,1.03,.65),'dark')
    for j in range(12):box(k,(x,4.67+j*.066,-.29),(.09,.027,.08),'silver')
for z in (-1.45,-.04):pipe3(k,[(-.8,5.25,z),(-1.35,5.25,z),(-1.35,4.73,z)],.037,'cyan')

# 16 — Homogenizer with cascaded mirror cells, light-pipe facets and finned enclosure.
k=names[16]
mechbox(k,(0,4.38,-.73),(1.86,.55,1.74),'navy')
for x in np.linspace(-.68,.68,7):
    for z in np.linspace(-1.32,-.14,7):
        box(k,(x,4.71,z),(.13,.042,.13),'violet' if int(x*10+z*10)%3==0 else 'silver')
for i in range(11):box(k,(-.75+i*.15,4.05,.19),(.08,.16,.06),'light')
for x in (-.77,.77):
    pipe3(k,[(x,4.60,-1.42),(x,4.74,-1.62),(x*.7,4.83,-1.62)],.028,'copper')

# 17 — Physical shutter door with rails and warning stripes.
k=names[17]
register(17,'ANIM_EXPOSURE_SHUTTER',(0,3.92,-.73),'linear','x')
mechbox(k,(0,3.99,-.73),(1.85,.20,1.61),'dark')
with moving('ANIM_EXPOSURE_SHUTTER'):
    box(k,(0,3.91,-.73),(1.65,.055,1.35),'black')
    for i in range(10):
        box(k,(-.7+i*.15,3.95,-.1),(.070,.022,.045),'amber' if i%2 else 'violet')
for x in (-.91,.91):
    box(k,(x,4.04,-.73),(.078,.17,1.65),'silver')

# 18 — Reticle storage cabinet with dozens of wafer-mask cassettes and protective slots.
k=names[18]
frame(k,(-4.02,2.40,-3.39),(1.95,2.65,1.17),beam=.075,glass=True)
for n in range(14):
    y=1.41+n*.135
    box(k,(-4.02,y,-3.39),(1.58,.058,.66),'dark')
    box(k,(-4.02,y+.035,-3.35),(1.35,.022,.53),'violet')
    for xx in (-4.75,-3.29):screw(k,(xx,y,-2.94),.025,'z')
for x in (-4.76,-3.27):ledrail(k,(x,2.45,-2.77),2.30,'y','cyan')

# 19 — High resolution reticle mask: modelled glass, circuit pattern, gold markers, clamps.
k=names[19]
box(k,(0,3.48,-.73),(1.94,.095,1.64),'glass')
box(k,(0,3.52,-.73),(1.80,.046,1.49),'dark')
box(k,(0,3.55,-.73),(1.63,.018,1.34),'violet')
chiptraces(k,(0,3.57,-.73),1.46,1.2,.012,21,'circuit')
for x in (-.83,.83):
    for z in (-1.34,-.12):
        box(k,(x,3.60,z),(.14,.046,.14),'gold')
        screw(k,(x,3.64,z),.031,'y')
for z in (-1.49,.03):
    box(k,(0,3.54,z),(1.92,.065,.08),'steel')

# 20 — Independent reticle scanning platform; X/Y servo axes, reflective bearings and carriage.
k=names[20]
register(20,'ANIM_RETICLE_STAGE_X',(0,3.38,-.73),'linear','x')
register(20,'ANIM_RETICLE_STAGE_Y',(0,3.49,-.73),'linear','z')
mechbox(k,(0,3.26,-.73),(3.13,.22,2.51),'navy')
for z in (-1.72,.26):
    box(k,(0,3.45,z),(2.88,.12,.17),'silver')
    for i in range(27):box(k,(-1.34+i*.103,3.53,z),(.052,.013,.028),'cyan')
with moving('ANIM_RETICLE_STAGE_X'):
    box(k,(0,3.50,-.73),(2.18,.15,2.00),'dark')
    for x in (-1.02,1.02):
        box(k,(x,3.56,-.73),(.10,.07,1.84),'steel')
with moving('ANIM_RETICLE_STAGE_Y'):
    box(k,(0,3.60,-.73),(1.94,.095,1.62),'silver')
    ring(k,(0,3.66,-.73),.72,.65,.04,'y','violet',44)

# 21 — Reticle linear encoder strip and optical sensing reader.
k=names[21]
register(21,'ANIM_RETICLE_POSITION_MARKER',(1.13,3.61,.16),'linear','x')
box(k,(0,3.56,.37),(2.85,.10,.22),'black')
for i in range(61):
    box(k,(-1.32+i*.044,3.64,.41),(.021,.016,.025),'silver' if i%3 else 'cyan')
with moving('ANIM_RETICLE_POSITION_MARKER'):
    sensor(k,(1.13,3.61,.16),'cyan',True)

# 22 — Massive projection lens barrel, optics, transparent sapphire-like inspection stripes.
k=names[22]
opticsbarrel(k,(0,2.48,-.73),1.12,1.55)
for y in (1.74,2.10,2.48,2.88,3.22):
    cyl(k,(0,y,-.73),.75,.025,'glass','y',48)
    ring(k,(0,y+.03,-.73),.87,.84,.04,'y','violet',48)
for y in (1.94,2.66,3.02):
    for a in np.linspace(0,2*math.pi,16,endpoint=False):
        x=1.17*math.cos(a);z=-.73+1.17*math.sin(a)
        screw(k,(x,y,z),.028,'y')
for yy in (1.83,3.18):
    for xx in (-.91,.91):box(k,(xx,yy,.12),(.08,.11,.14),'gold')

# 23 — Adjustable optical focus ring and actuated encoder scale.
k=names[23]
register(23,'ANIM_LENS_FOCUS',(0,2.76,-.73),'linear','y')
with moving('ANIM_LENS_FOCUS'):
    precision_ring(k,(0,2.76,-.73),1.22,1.04,.16,'y','silver')
    for i in range(32):
        a=2*math.pi*i/32
        x=1.23*math.cos(a);z=-.73+1.23*math.sin(a)
        box(k,(x,2.81,z),(.045,.12,.041),'blue' if i%5==0 else 'dark')
for xx in (-1.23,1.23):
    box(k,(xx,2.76,-.73),(.17,.66,.26),'steel')
    for i in range(8):box(k,(xx,2.48+i*.07,-.47),(.065,.025,.04),'cyan')

# 24 — Segmented numerical aperture iris assembly.
k=names[24]
register(24,'ANIM_NA_DIAPHRAGM',(0,2.16,-.73),'angular','y')
with moving('ANIM_NA_DIAPHRAGM'):
    ring(k,(0,2.17,-.73),.84,.59,.12,'y','dark',48)
    for i in range(18):
        a=2*math.pi*i/18
        x=.64*math.cos(a);z=-.73+.64*math.sin(a)
        box(k,(x,2.22,z),(.15,.025,.10),'silver' if i%2 else 'violet')
    precision_ring(k,(0,2.23,-.73),.86,.80,.038,'y','steel')

# 25 — Nanometre wafer XY metrology stage, separate nested X/Y carriages, bearings, bolts.
k=names[25]
register(25,'ANIM_WAFER_STAGE_X',(0,1.18,-.73),'linear','x')
register(25,'ANIM_WAFER_STAGE_Y',(0,1.36,-.73),'linear','z')
precision_stage(k,(0,1.00,-.73),(3.60,.34,3.18),'steel')
with moving('ANIM_WAFER_STAGE_X'):
    precision_stage(k,(0,1.33,-.73),(2.70,.23,2.48),'light')
with moving('ANIM_WAFER_STAGE_Y'):
    box(k,(0,1.48,-.73),(1.75,.11,1.75),'steel')
    for z in (-1.36,-.09):
        box(k,(0,1.56,z),(1.60,.050,.080),'cyan')

# 26 — Dual axis linear motor coils, slotted magnetic tracks and cable carriers.
k=names[26]
register(26,'ANIM_X_LINEAR_SERVO',(1.34,1.05,-.73),'linear','x')
register(26,'ANIM_Y_LINEAR_SERVO',(0,1.05,-2.03),'linear','z')
with moving('ANIM_X_LINEAR_SERVO'):
    for x in (-1.35,1.35):
        box(k,(x,1.07,-.73),(.28,.27,2.30),'navy')
        for j in range(24):box(k,(x,1.22,-1.76+j*.088),(.19,.046,.035),'copper')
with moving('ANIM_Y_LINEAR_SERVO'):
    for z in (-2.03,.59):
        box(k,(0,1.07,z),(2.38,.27,.26),'dark')
        for j in range(26):box(k,(-1.07+j*.085,1.22,z),(.032,.039,.18),'copper')
for x in (-1.65,1.65):
    wirebundle(k,[(x,.95,-1.52),(x,1.13,-1.53),(x,1.20,-.85)],4,.025,'cyan',.012)

# 27 — Hexapod-like wafer Z tilt platform with independent telescoping actuator legs.
k=names[27]
register(27,'ANIM_WAFER_STAGE_Z',(0,1.57,-.73),'linear','y')
register(27,'ANIM_WAFER_TILT_X',(0,1.68,-.73),'angular','x')
register(27,'ANIM_WAFER_TILT_Y',(0,1.68,-.73),'angular','z')
with moving('ANIM_WAFER_STAGE_Z'):
    axial(k,(0,1.58,-.73),.84,.13,'steel',40)
    for a in np.linspace(0,2*math.pi,6,endpoint=False):
        x=.68*math.cos(a);z=-.73+.68*math.sin(a)
        cyl(k,(x,1.41,z),.11,.29,'silver','y',16)
        cyl(k,(x,1.53,z),.07,.37,'copper','y',16)
with moving('ANIM_WAFER_TILT_X'):
    ring(k,(0,1.67,-.73),.78,.70,.075,'y','navy',40)
with moving('ANIM_WAFER_TILT_Y'):
    ring(k,(0,1.71,-.73),.69,.62,.040,'y','cyan',40)

# 28 — Laser interferometer with optical head, retroreflector and cyan measurement rays.
k=names[28]
register(28,'ANIM_INTERFEROMETER_HEAD',(-2.26,1.51,-.73),'angular','y')
mechbox(k,(-2.30,1.07,-.73),(.67,.28,.67),'navy')
with moving('ANIM_INTERFEROMETER_HEAD'):
    sensor(k,(-2.26,1.56,-.73),'cyan',True)
    cyl(k,(-1.95,1.54,-.73),.13,.32,'silver','x',24)
    cyl(k,(-1.79,1.54,-.73),.090,.036,'laser','x',20)
pipe3(k,[(-1.80,1.54,-.73),(-.82,1.54,-.73)],.011,'laser',7)
for z in (-1.55,.09):
    box(k,(-1.63,1.46,z),(.24,.19,.16),'silver')
    for j in range(6):box(k,(-1.52,1.47+j*.02,z),(.027,.01,.05),'cyan')

# 29 — Alignment mark camera with mechanical objective, light ring and overhead arm.
k=names[29]
register(29,'ANIM_ALIGNMENT_CAMERA',(1.73,2.17,-.37),'angular','x')
stand(k,1.95,1.24,-.42,1.00)
with moving('ANIM_ALIGNMENT_CAMERA'):
    mechbox(k,(1.73,2.24,-.37),(.60,.43,.54),'navy')
    cyl(k,(1.73,1.91,-.37),.20,.30,'steel','y',24)
    precision_ring(k,(1.73,1.75,-.37),.22,.16,.055,'y','cyan')
    for i in range(12):
        a=i*2*math.pi/12
        cyl(k,(1.73+.19*math.cos(a),1.73,-.37+.19*math.sin(a)),.015,.020,'violet','y',10)

# 30 — Electrostatic wafer chuck with ceramic surface, retaining clamps and electrical contacts.
k=names[30]
register(30,'ANIM_WAFER_CHUCK',(0,1.76,-.73),'linear','y')
with moving('ANIM_WAFER_CHUCK'):
    axial(k,(0,1.74,-.73),.69,.14,'ceramic',44)
    ring(k,(0,1.84,-.73),.69,.63,.033,'y','copper',44)
    for a in np.linspace(0,2*math.pi,12,endpoint=False):
        x=.63*math.cos(a);z=-.73+.63*math.sin(a)
        box(k,(x,1.83,z),(.053,.041,.068),'steel')
for x in (-.45,.45):
    for z in (-1.13,-.33):cyl(k,(x,1.86,z),.035,.022,'silver','y',10)

# 31 — Planarized wafer: 16 field regions with patterned sapphire/iridescent detail.
k=names[31]
waferdisc(k,(0,1.87,-.73),.61,.031,'iridescent',False)
for ix in range(4):
    for iz in range(4):
        x=(ix-1.5)*.255;z=-.73+(iz-1.5)*.255
        if x*x+(z+.73)*(z+.73)>.56**2:continue
        box(k,(x,1.889,z),(.206,.005,.206),'navy')
        chiptraces(k,(x,1.898,z),.18,.18,.004,5,'circuit')
for a in np.linspace(0,2*math.pi,32,endpoint=False):
    x=.56*math.cos(a);z=-.73+.56*math.sin(a)
    cyl(k,(x,1.899,z),.014,.010,'cyan','y',8)

# 32 — Photoalignment targets etched around wafer periphery.
k=names[32]
for x,z in [(-.44,-.73),(.44,-.73),(0,-1.17),(0,-.29),(-.32,-1.04),(.32,-.42)]:
    for dx in (-.06,.06):box(k,(x+dx,1.900,z),(.045,.007,.009),'gold')
    for dz in (-.06,.06):box(k,(x,1.901,z+dz),(.009,.007,.045),'gold')
    ring(k,(x,1.901,z),.09,.085,.01,'y','cyan',20)

# 33 — Photosensitive resist thin transparent/magenta coat with field marking.
k=names[33]
cyl(k,(0,1.903,-.73),.58,.014,'resist','y',48)
for i in range(8):
    zz=-1.18+i*.126
    box(k,(0,1.913,zz),(.93,.004,.009),'violet')
for i in range(8):
    xx=-.46+i*.132
    box(k,(xx,1.913,-.73),(.009,.004,.92),'violet')

# 34 — Hovering topography diagnostic heights and focus map pillars.
k=names[34]
register(34,'ANIM_TOPOGRAPHY_FOCUS_MAP',(2.37,3.22,.82),'visual','y')
with moving('ANIM_TOPOGRAPHY_FOCUS_MAP'):
    box(k,(2.37,3.13,.82),(1.40,.06,1.34),'glass')
    for i in range(12):
        for j in range(12):
            x=1.76+i*.11;z=.22+j*.11
            h=.04+.12*(.5+.5*math.sin(i*.7)*math.cos(j*.62))
            box(k,(x,3.23+h/2,z),(.065,h,.065),'violet' if h>.11 else 'cyan')
    for i in range(5):
        ledrail(k,(2.37,3.42+i*.10,.15),1.32,'x','blue')

# 35 — Height leveling sensor with dual optical heads and lateral gantry.
k=names[35]
register(35,'ANIM_HEIGHT_LEVELING_HEAD',(1.62,2.32,-1.83),'linear','x')
stand(k,1.70,1.27,-1.88,1.02)
with moving('ANIM_HEIGHT_LEVELING_HEAD'):
    for xx in (1.45,1.78):
        sensor(k,(xx,2.36,-1.83),'violet',True)
        cyl(k,(xx,2.11,-1.83),.060,.18,'cyan','y',14)
box(k,(1.61,2.60,-1.83),(.83,.09,.22),'silver')

# 36 — Massive vibration isolation platform under wafer stage; modeled magnetic isolator pedestals.
k=names[36]
register(36,'ANIM_ISOLATION_STAGE',(0,.79,-.73),'linear','y')
with moving('ANIM_ISOLATION_STAGE'):
    mechbox(k,(0,.69,-.73),(4.34,.33,4.11),'navy')
    for x in (-1.73,1.73):
        for z in (-2.36,.91):
            cyl(k,(x,.40,z),.31,.36,'steel','y',24)
            for yy in (.42,.50,.58):ring(k,(x,yy,z),.30,.265,.042,'y','copper',24)
            cyl(k,(x,.23,z),.37,.10,'dark','y',24)
    for x in (-2.03,2.03):ledrail(k,(x,.81,-.73),3.86,'z','cyan')

# 37 — Six-axis acceleration sensing with colored coordinate indicator arms.
k=names[37]
sensor(k,(2.23,1.52,-2.59),'amber',False)
for p,col in [((2.55,1.52,-2.59),'red'),((2.23,1.86,-2.59),'green'),((2.23,1.52,-2.20),'blue')]:
    pipe3(k,[(2.23,1.52,-2.59),p],.025,col)
for z in (-2.80,-2.38):screw(k,(2.23,1.35,z),.055,'y')

# 38 — Lens temperature drift assembly with banded coils and precision thermal sensors.
k=names[38]
sensor(k,(1.60,3.02,-1.57),'amber',True)
for j in range(16):
    yy=2.57+j*.035
    ring(k,(0,yy,-.73),1.29,1.255,.025,'y','copper',36)
pipe3(k,[(1.45,2.89,-1.44),(1.72,3.07,-1.45),(1.86,3.18,-1.64)],.036,'cyan')

# 39 — Photodiode-based dose sensor with photometric aperture and readable status LEDs.
k=names[39]
register(39,'ANIM_DOSE_PHOTODIODE',(1.83,3.89,-1.44),'linear','x')
with moving('ANIM_DOSE_PHOTODIODE'):
    sensor(k,(1.83,3.89,-1.44),'violet',True)
    mirrorhead(k,(1.64,3.89,-1.44),.15,'x')
for j in range(8):
    box(k,(1.83+j*.035,4.13,-1.36),(.019,.020,.026),'cyan' if j%3 else 'amber')

# 40 — Narrow scan illumination slit with shutter-like sliding baffles.
k=names[40]
register(40,'ANIM_SCAN_SLIT',(0,3.77,-.73),'linear','x')
with moving('ANIM_SCAN_SLIT'):
    box(k,(0,3.78,-.73),(1.49,.055,1.53),'dark')
    for x in np.linspace(-.70,.70,24):box(k,(x,3.83,-.73),(.025,.014,1.27),'silver' if round(x*100)%4 else 'violet')
    box(k,(0,3.84,-.73),(.045,.025,1.35),'uv')
for z in (-1.49,.03):box(k,(0,3.85,z),(1.73,.060,.08),'steel')

# 41 — Emergency station with safety switches, status and large mushroom button.
k=names[41]
register(41,'ANIM_LITHO_ESTOP',(8.08,1.11,3.40),'linear','z')
zpanel(k,(8.06,.98,3.07),1.07,.97,.65,'red',4)
with moving('ANIM_LITHO_ESTOP'):
    cyl(k,(8.05,1.08,3.45),.23,.20,'red','z',32)
    ring(k,(8.05,1.08,3.57),.26,.22,.040,'z','amber',28)
for x in (7.79,8.29):
    cyl(k,(x,1.51,3.08),.080,.15,'green' if x>8 else 'amber','y',16)

# 42 — Post exposure bake chamber with visible heated zones and hinged lid.
k=names[42]
register(42,'ANIM_POSTBAKE_LID',(4.33,2.35,-.91),'angular','x')
mechbox(k,(4.33,1.13,-.91),(2.15,.38,1.84),'navy')
axial(k,(4.33,1.46,-.91),.71,.15,'ceramic',44)
for rr in (.18,.38,.60):ring(k,(4.33,1.56,-.91),rr,rr-.022,.025,'y','warm',44)
with moving('ANIM_POSTBAKE_LID'):
    box(k,(4.33,2.35,-.91),(2.12,.17,1.84),'silver')
    box(k,(4.33,2.46,-.91),(1.80,.02,1.55),'dark')
for x in (3.35,5.31):box(k,(x,1.88,-1.63),(.09,.79,.08),'steel')

# 43 — Post exposure bake thermal sensor.
k=names[43]
sensor(k,(5.59,1.68,-.91),'amber',True)
pipe3(k,[(5.59,1.38,-.91),(5.26,1.38,-.91),(5.03,1.51,-.91)],.022,'copper')

# 44 — Fictional developer unit with transparent splash chamber, circular spin chuck and hoses.
k=names[44]
register(44,'ANIM_DEVELOPER_SPIN',(5.97,1.24,1.16))
frame(k,(5.97,1.98,1.16),(2.28,2.43,2.20),beam=.085,glass=True)
mechbox(k,(5.97,.84,1.16),(2.32,.19,2.23),'navy')
with moving('ANIM_DEVELOPER_SPIN'):
    axial(k,(5.97,1.22,1.16),.68,.12,'steel',40)
    waferdisc(k,(5.97,1.32,1.16),.53,.024,'resist',False)
for r in (.83,1.01):ring(k,(5.97,1.33,1.16),r,r-.06,.20,'y','silver',40)
for x in (5.10,6.84):ledrail(k,(x,2.12,2.22),1.68,'y','violet')

# 45 — Developer dispensing nozzle articulating over process bowl.
k=names[45]
register(45,'ANIM_DEVELOPER_NOZZLE',(5.86,2.18,1.10),'angular','y')
mechbox(k,(6.88,2.04,.53),(.39,1.11,.48),'steel')
with moving('ANIM_DEVELOPER_NOZZLE'):
    angular_arm(k,(6.88,2.41,.53),(6.16,2.46,.80),.088,'steel')
    angular_arm(k,(6.16,2.46,.80),(5.87,2.33,1.12),.071,'silver')
    cyl(k,(5.86,2.16,1.10),.063,.31,'steel','y',16)
    cyl(k,(5.86,1.96,1.10),.029,.09,'violet','y',12)
pipe3(k,[(6.9,2.59,.43),(6.15,2.59,.78),(5.84,2.44,1.14)],.028,'cyan')

# 46 — Developer flow transmitter, pressure manifold and calibrated glass flow sight.
k=names[46]
sensor(k,(7.36,1.71,1.71),'cyan',True)
cyl(k,(7.36,1.37,1.71),.095,.35,'glass','y',18)
ring(k,(7.36,1.59,1.71),.13,.095,.05,'y','silver',20)
pipe3(k,[(7.36,1.14,1.71),(7.36,1.04,1.92)],.027,'steel')

# 47 — Rotary wafer rinse/spin-dryer with liquid spray ring, bowl and rotating wafer.
k=names[47]
register(47,'ANIM_RINSE_DRYER',(7.73,1.34,-1.04))
mechbox(k,(7.73,.88,-1.04),(1.75,.22,1.74),'navy')
cyl(k,(7.73,1.20,-1.04),.72,.18,'steel','y',44)
with moving('ANIM_RINSE_DRYER'):
    waferdisc(k,(7.73,1.35,-1.04),.55,.025,'iridescent',True)
    ring(k,(7.73,1.40,-1.04),.62,.59,.075,'y','silver',42)
for a in np.linspace(0,2*math.pi,12,endpoint=False):
    x=7.73+.73*math.cos(a);z=-1.04+.73*math.sin(a)
    cyl(k,(x,1.54,z),.029,.060,'cyan','y',10)
pipe3(k,[(8.36,2.13,-1.08),(8.38,1.81,-1.08),(8.0,1.62,-1.08)],.047,'silver')

# 48 — Critical dimension inspection head on gantry with focusable lens and wafer platform.
k=names[48]
register(48,'ANIM_CD_SCANNER',(4.62,2.45,2.54),'linear','x')
for x in (3.77,5.44):
    box(k,(x,2.10,2.51),(.16,1.78,.16),'steel')
    box(k,(x,1.11,2.51),(.34,.16,.36),'dark')
box(k,(4.62,3.08,2.51),(1.88,.24,.31),'silver')
with moving('ANIM_CD_SCANNER'):
    mechbox(k,(4.62,2.72,2.51),(.42,.41,.45),'navy')
    cyl(k,(4.62,2.42,2.51),.16,.26,'silver','y',24)
    cyl(k,(4.62,2.22,2.51),.090,.10,'violet','y',18)
    for j in range(9):box(k,(4.33+j*.07,2.78,2.77),(.037,.035,.016),'cyan')
box(k,(4.62,1.16,2.51),(1.51,.13,1.29),'black')

# 49 — Overlay metrology with multiple interferometric objectives and crosshair display.
k=names[49]
register(49,'ANIM_OVERLAY_METROLOGY',(6.55,2.47,2.54),'linear','y')
stand(k,6.55,1.15,2.54,1.17)
with moving('ANIM_OVERLAY_METROLOGY'):
    mechbox(k,(6.55,2.47,2.54),(.77,.42,.72),'navy')
    for dx,dz in [(-.22,-.19),(.22,.19),(-.22,.19),(.22,-.19)]:
        mirrorhead(k,(6.55+dx,2.16,2.54+dz),.087)
    ring(k,(6.55,2.17,2.54),.31,.27,.025,'y','cyan',28)
compact_screen(k,(6.55,2.51,2.95),.48,.25,'violet')

# 50 — Reticle defect scanner with separate scanning microscope, edge illumination and cassette.
k=names[50]
register(50,'ANIM_RETICLE_INSPECTION',(-3.37,2.49,-3.34),'linear','x')
mechbox(k,(-3.38,1.18,-3.34),(1.45,.27,1.14),'dark')
box(k,(-3.38,1.35,-3.34),(1.18,.055,.86),'glass')
chiptraces(k,(-3.38,1.39,-3.34),.98,.72,.007,9,'violet')
for x in (-3.98,-2.79):box(k,(x,1.99,-3.34),(.13,1.27,.15),'steel')
box(k,(-3.38,2.64,-3.34),(1.40,.18,.24),'silver')
with moving('ANIM_RETICLE_INSPECTION'):
    mechbox(k,(-3.37,2.48,-3.34),(.35,.40,.33),'navy')
    cyl(k,(-3.37,2.17,-3.34),.12,.28,'silver','y',18)
    cyl(k,(-3.37,2.0,-3.34),.060,.10,'violet','y',14)

# 51 — Fine pitch photoresist pattern defect camera with movable focus element.
k=names[51]
register(51,'ANIM_RESIST_DEFECT_CAMERA',(8.12,2.28,2.47),'angular','x')
stand(k,8.1,1.10,2.47,.99)
with moving('ANIM_RESIST_DEFECT_CAMERA'):
    mechbox(k,(8.12,2.36,2.47),(.59,.41,.56),'navy')
    cyl(k,(8.12,2.06,2.47),.18,.32,'silver','y',22)
    ring(k,(8.12,1.85,2.47),.22,.17,.051,'y','cyan',26)
for x in (7.94,8.30):ledrail(k,(x,2.52,2.81),.29,'y','violet')

# 52 — Large industrial PLC rack, dense removable IO, bus connectors and status LEDs.
k=names[52]
hd_cabinet(k,(-2.91,1.15,3.58),(1.38,1.25,.83),'green',2)
for i in range(10):
    x=-3.46+i*.12
    box(k,(x,1.52,4.05),(.082,.65,.07),'dark')
    for yy in (1.34,1.48,1.64):box(k,(x,yy,4.10),(.042,.031,.02),'green' if i%3==0 else 'cyan')
for i in range(3):terminal_block(k,(-3.45+i*.54,.64,4.02),4,accent='green')

# 53 — Synchronization controller with dual-stage trace screens, tuning knobs and terminal buses.
k=names[53]
hd_cabinet(k,(-.89,1.10,3.58),(1.48,1.17,.84),'violet',2)
for i in range(23):
    x=-1.51+i*.055
    z=4.05
    for c,yy in [('cyan',1.56+.065*math.sin(i*.58)),('violet',1.39+.055*math.sin(i*.58+.2))]:
        box(k,(x,yy,z),(.038,.015,.019),c)
for i in range(6):knob(k,-1.41+i*.20,.73,4.08,'violet' if i%2 else 'cyan')

# 54 — Giant standing SCADA with 4x4 field matrix, focus chart, trace overlays, bezel LEDs.
k=names[54]
register(54,'ANIM_LITHOGRAPHY_SCADA',(5.60,3.83,-3.82),'angular','x')
with moving('ANIM_LITHOGRAPHY_SCADA'):
    giantdisplay(k,(5.6,3.83,-3.82),4.45,2.21)
    box(k,(5.57,3.84,-3.72),(4.18,1.98,.020),'black')
    for ix in range(4):
        for iz in range(4):
            x=4.88+ix*.35;y=4.13-iz*.27
            box(k,(x,y,-3.696),(.31,.225,.018),'green' if ix+iz<6 else 'blue')
            box(k,(x,y,-3.683),(.20,.015,.010),'cyan')
    for i in range(23):
        x=3.77+i*.078
        y=3.1+.15*math.sin(i*.37)+.060*math.sin(i*.91)
        box(k,(x,y,-3.690),(.047,.019,.010),'violet')
    for i in range(22):
        x=5.35+i*.078
        y=3.1+.14*math.sin(i*.39+.1)
        box(k,(x,y,-3.69),(.046,.018,.010),'cyan')
    for x in (3.45,7.75):ledrail(k,(x,3.83,-3.68),2.1,'y','cyan')
    for y in (2.75,4.92):ledrail(k,(5.6,y,-3.68),4.33,'x','cyan')
for x in (3.87,7.30):
    box(k,(x,2.26,-3.95),(.18,.96,.28),'steel')
    box(k,(x,1.74,-3.95),(.47,.12,.50),'dark')

# 55 — Regulated 5V educational control supply, isolated from motor/UV/heater power.
k=names[55]
zpanel(k,(1.12,1.04,3.59),1.06,.90,.71,'green',3)
for j,x in enumerate((.84,1.12,1.40)):
    cyl(k,(x,.61,4.0),.07,.13,'copper','z',16)
    cyl(k,(x,.61,4.09),.034,.04,'red' if j==0 else 'blue','z',10)
finbank(k,(1.12,1.50,3.59),.80,14,.43,.20,'light')

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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 74 Lithography Modular Procedural Geometry')]),
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
    file=f'{i:02d}_L74_{key}'
    export_fbx(key,MD/(file+'.fbx'))
    export_glb(key,GD/(file+'.glb'))
    triangles=sum(len(m.faces) for m,_,_ in M[key]);parts=len(M[key])
    verts=np.concatenate([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L74_'+key,file=file+'.fbx',glb=file+'.glb',triangle_count=triangles,
         mesh_parts=parts,bbox_min=np.around(verts.min(axis=0),4).tolist(),bbox_max=np.around(verts.max(axis=0),4).tolist(),
         rig_groups=list(PV.get(key,{}).keys()),animation_pivots={g:list(obj['position']) for g,obj in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/56] {key:43s} {triangles:7,d} tris {parts:4d} parts',flush=True)

combined='Level74_Complete_ArtKit_Showcase'
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
            writer.writerow(['L74_'+key,pivot,*info['position'],info['kind'],info['axis'],'Animated parent Null group present in FBX and GLB','Pivot is modeled hierarchy transform; no baked animation. Drive from actual Unity gameplay.'])
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
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_74','description':'Procedural colored material regions. No author-painted PBR textures; visual shader settings are suggestions.','materials':palette},indent=2))


wiring=[
('5 V control supply and returns',10,'Educational control power, returns and sensor rails'),
('Wafer and reticle stage servos/encoders',16,'Stage motor requests, enable, stage encoder feedback, drive status'),
('Interferometer, alignment and focus',16,'Critical interferometer signal, alignment mark cameras, wafer height map and Z/tilt'),
('DUV illumination, shutter and dose',14,'Virtual UV source, shutters, photodiode, scan slit and optics'),
('Resist coating and thermal preparation',12,'Coater drive, nozzle, coating sensor, hotplate controls and thermal feedback'),
('Development, cleaning and metrology',12,'Post-exposure bake, developer, rinse/dry, CD and overlay certification'),
('Vibration, thermal and safety protection',10,'Isolation, vibration, lens drift, safety permissions and emergency stop'),
('PLC, stage synchronization and SCADA',12,'Stage matching, interlocks, HMI events, process and objectives')]
assert sum(x[1] for x in wiring)==102
with (ROOT/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Purpose','Status'])
    for row in wiring:w.writerow([*row,'PLANNED_NOT_IMPLEMENTED'])

ntri=sum(m['triangle_count'] for m in manifest)
nparts=sum(m['mesh_parts'] for m in manifest)
npiv=sum(len(PV.get(name,{})) for name in names)
metadata={
'level_id':'level_74','display_name':'Align the Light',
'asset_kind':'Original procedural HD / AAA-inspired 3D geometry; not artist-authored photorealistic PBR textures',
'individual_models':56,'individual_glbs':56,
'assembled_models':['Models/'+combined+'.fbx','GLB/'+combined+'.glb'],
'individual_triangle_count':ntri,'individual_mesh_parts':nparts,'animation_parent_pivot_groups':npiv,
'planned_player_wires':102,'planned_unique_functional_terminals':158,
'actual_player_wires_included':False,'functional_spark_terminals_included':False,
'lithography_physics_included':False,'six_authoritative_objectives_implemented':False,
'fbx_units':'Numeric centimeters, UnitScaleFactor 1; Unity import scale must correspond to meters',
'glb_units':'numeric meters','axes':'X horizontal width, Y up, Z front',
'approx_workstation_width_m':18.2,'common_origin_for_all_individual_models':True,
'rig_hierarchy':'Named animated parent Null nodes in FBX and GLB (no baked animations), actual hierarchy pivot transforms',
'educational_scope':'Fictional DUV source, photoresist and developer; art only, no hazardous UV operation',
'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(metadata,indent=2),encoding='utf-8')
print('EXPORTED',len(manifest),'models /',ntri,'triangles /',nparts,'mesh parts /',npiv,'animated pivots',flush=True)
