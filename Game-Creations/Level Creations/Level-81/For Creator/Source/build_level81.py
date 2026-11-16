#!/usr/bin/env python3
"""Project Spark Level 81: original fault-tolerant mission computing and digital reliability modular geometry, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical or chiplet-bonding gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level81_HD_AAA')
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 81 Fault Tolerant Computing Original Modular Geometry')]),
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
# ORIGINAL PROJECT SPARK LEVEL 81: OUTLAST THE BITSTORM
# Physical sci-fi digital reliability arena, NOT a re-skin of the optical lab.
# -----------------------------------------------------------------------------
P.update({'magenta':(237,72,239,255),'storm':(172,73,245,170),'deepglass':(77,145,210,92),'emerald':(52,239,160,255),'corrupt':(247,94,69,255),'signal':(54,249,233,255),'vault':(255,194,87,255),'graphite':(22,34,48,255),'platinum':(188,208,227,255)})
raw_names='''IndustrialReliabilityArenaBase|TransparentDigitalFaultObservatory|FourChipletMissionComputerCarrier|CompletedPhotonicChipletComputer|OpticalTelemetryDock|HolographicDeepSpaceMissionDisplay|VirtualParticleStormProjector|TransparentFaultInjectionChamber|ParticleEventPatternDirector|DigitalFaultInjectionController|FaultEventTimelineOscilloscope|SimulatedParticleFluxMonitor|HolographicEventSpatialMap|GiantMemoryBitStateHologram|HardenedMemorySubsystemRack|ECCMemoryBankA|ECCMemoryBankB|MemoryAddressDecoder|SECDEDParityEncoder|ECCSyndromeDetectionEngine|SingleBitCorrectionEngine|DoubleBitErrorAlarmAnalyzer|BackgroundMemoryScrubber|ScrubCycleSequencer|MemoryBandwidthLoadMeter|MemoryPageQuarantineModule|ReadOnlyGoldenImageVault|CheckpointRecoveryController|HolographicCheckpointJournal|RedundantComputeCoreA|RedundantComputeCoreB|RedundantComputeCoreC|TripleLaneExecutionSupervisor|TripleModularMajorityVoter|VoterDiscrepancyAnalyzer|ComputeLaneHealthProbeArray|IndependentReferenceComputeUnit|DeterministicMissionTaskScheduler|HardwareWatchdogTimer|WatchdogChallengeResponseModule|SafeMissionCommandGate|ResilientInterchipCommunicationFabric|PacketCRCEncoder|PacketCRCVerificationEngine|PacketSequenceNumberMonitor|OpticalLinkErrorTelemetryBridge|PacketRetransmissionBuffer|TelemetryFrameIntegrityAnalyzer|ProtectedTelemetryMemoryBuffer|MissionSensorInputEmulator|NavigationStateEstimator|SpacecraftAttitudeActuatorSimulator|GuidanceCommandValidationUnit|CorrelatedFaultPatternAnalyzer|ErrorPropagationTraceAnalyzer|MissionSafetyPLCRack|CriticalECCFeedbackSupervisor|ProtectedPowerDistributionCabinet|Regulated5VControlSupply|GuardedFaultInjectionConsole|EmergencyMissionSafeHoldStation|HolographicDependabilitySCADA|ECCCorrectionArcHologram|TripleLaneVotingHologram|PacketIntegrityFlowDisplay|HolographicMissionHealthDisplay|FaultEventReplayChronicle|IndependentChecksumReferenceModule|PrecisionDigitalProbeBank|MissionReliabilityCertificationBeacon'''
names=raw_names.split('|')
assert len(names)==70
POS={
 1:(0,3.05,0),2:(0,1.74,0),3:(0,2.28,0),4:(-2.75,2.35,1.1),5:(0,6.75,-4.75),6:(0,5.45,0),7:(0,3.2,0),8:(-3.1,3.25,-1.7),9:(-4.35,2.15,2.6),10:(-4.9,4.5,-3.55),11:(-3.25,5.35,-2.3),12:(-4.1,4.82,-1.6),13:(-7.8,5.65,-4.55),14:(-9.45,2.6,-2.35),15:(-9.15,3.24,-.65),16:(-7.73,3.24,-.65),17:(-10.4,1.6,1.8),18:(-9.2,2.2,1.9),19:(-7.75,2.5,2.1),20:(-6.48,2.3,2.5),21:(-7.0,4.35,-2.9),22:(-9.8,3.95,1.5),23:(-10.75,4.35,1.25),24:(-9.3,1.55,3.65),25:(-6.65,1.7,3.5),26:(-5.2,1.65,4.5),27:(-6.05,2.9,4.15),28:(-5.2,5.45,-3.6),29:(6.15,4.6,-2.65),30:(8.0,4.6,-2.65),31:(9.85,4.6,-2.65),32:(7.95,1.7,-2.95),33:(8,3.25,1.0),34:(10.35,3.45,1.7),35:(5.85,3.15,1.7),36:(10.45,2.1,3.45),37:(6.4,1.7,3.25),38:(9.3,1.65,3.25),39:(10.5,1.65,4.05),40:(10.5,3.35,4.55),41:(2.7,2.15,1.35),42:(3.5,2.85,2.7),43:(4.5,2.8,3.75),44:(5.2,2.0,4.1),45:(3.3,1.4,4.2),46:(4.6,1.4,4.55),47:(6,1.4,4.6),48:(3.5,3.4,4.35),49:(-3.1,3.0,4.4),50:(-1.7,2.0,4.45),51:(1.15,5.4,-3.85),52:(2,2.6,4.35),53:(10.3,5.95,.5),54:(5.3,5.5,2.2),55:(-.8,1.5,5.08),56:(-2.1,1.65,5.08),57:(-3.0,1.2,5.1),58:(-4.0,1.15,5.12),59:(0,1.16,5.4),60:(10.85,1.38,5.12),61:(0,6.45,-5.52),62:(-6.6,5.98,-3.15),63:(8.0,6.65,-3.45),64:(4.9,6.0,.35),65:(0,7.50,-4.28),66:(4.4,5.45,-3.75),67:(-5.0,3.2,4.25),68:(-3.6,3.7,3.5),69:(0,7.8,-5.05)
}
# 52 actual mesh-owning parent transforms. Indicator names represent holographic displays.
pivot_spec='''6|ANIM_BITSTORM_PROJECTOR_ROTATION|rotation|y
8|ANIM_EVENT_DIRECTOR_X|translation|x
8|ANIM_EVENT_DIRECTOR_Y|translation|y
7|ANIM_FAULT_INJECTION_CHAMBER_DOOR|rotation|x
59|ANIM_FAULT_CONSOLE_GUARD|rotation|x
6|ANIM_PARTICLE_EVENT_HOLOGRAM|indicator|none
11|ANIM_PARTICLE_FLUX_MONITOR|indicator|none
12|ANIM_EVENT_SPATIAL_MAP|indicator|none
10|ANIM_EVENT_TIMELINE_SCAN|indicator|none
66|ANIM_FAULT_REPLAY_CHRONICLE|indicator|none
15|ANIM_MEMORY_BANK_A_CARTRIDGE|translation|z
16|ANIM_MEMORY_BANK_B_CARTRIDGE|translation|z
17|ANIM_MEMORY_ADDRESS_DECODER|indicator|none
18|ANIM_SECDED_PARITY_ENCODER|indicator|none
19|ANIM_ECC_SYNDROME_SCAN|indicator|none
20|ANIM_SINGLE_BIT_CORRECTION|indicator|none
21|ANIM_DOUBLE_BIT_ERROR_ALARM|indicator|none
22|ANIM_SCRUBBER_SCAN_HEAD|translation|x
23|ANIM_SCRUBBER_CYCLE_DISPLAY|indicator|none
24|ANIM_MEMORY_BANDWIDTH_METER|indicator|none
25|ANIM_PAGE_QUARANTINE_GATE|translation|y
26|ANIM_GOLDEN_CHECKPOINT_RESTORE|translation|z
29|ANIM_COMPUTE_LANE_A|indicator|none
30|ANIM_COMPUTE_LANE_B|indicator|none
31|ANIM_COMPUTE_LANE_C|indicator|none
33|ANIM_TMR_VOTER_GATE|translation|y
34|ANIM_VOTER_DISAGREEMENT_DISPLAY|indicator|none
35|ANIM_COMPUTE_HEALTH_PROBE|translation|y
36|ANIM_REFERENCE_COMPUTE_SCAN|indicator|none
38|ANIM_WATCHDOG_TIMER|rotation|z
39|ANIM_WATCHDOG_CHALLENGE_RESPONSE|indicator|none
40|ANIM_SAFE_COMMAND_GATE|translation|y
41|ANIM_INTERCHIP_DATA_FABRIC|indicator|none
42|ANIM_CRC_ENCODER|indicator|none
43|ANIM_CRC_VALIDATOR|indicator|none
44|ANIM_PACKET_SEQUENCE_SCAN|indicator|none
4|ANIM_OPTICAL_TELEMETRY_DOCK|translation|x
46|ANIM_PACKET_RETRANSMISSION|indicator|none
48|ANIM_TELEMETRY_BUFFER|indicator|none
49|ANIM_SENSOR_INPUT_STREAM|indicator|none
50|ANIM_NAVIGATION_STATE_DISPLAY|indicator|none
51|ANIM_ATTITUDE_ACTUATOR_HOLOGRAM|indicator|none
53|ANIM_CORRELATED_FAULT_ANALYZER|indicator|none
54|ANIM_ERROR_PROPAGATION_TRACE|indicator|none
56|ANIM_CRITICAL_ECC_FEEDBACK|indicator|none
57|ANIM_POWER_DISTRIBUTION_ISOLATOR|translation|y
60|ANIM_EMERGENCY_SAFE_HOLD|translation|z
62|ANIM_MEMORY_REPAIR_ARC|indicator|none
63|ANIM_TRIPLE_VOTING_HOLOGRAM|indicator|none
64|ANIM_PACKET_INTEGRITY_HOLOGRAM|indicator|none
61|ANIM_DEPENDABILITY_SCADA|indicator|none
69|ANIM_MISSION_CERTIFICATION_BEACON|indicator|none'''
pivots=[]
for ln in pivot_spec.strip().splitlines():
 j,n,kind,axis=ln.split('|');idx=int(j);p=POS[idx];pivots.append((idx,n,kind,axis,p))
assert len(pivots)==52 and len({p[1] for p in pivots})==52
PV={k:{} for k in names}
for idx,n,kind,axis,p in pivots:PV[names[idx]][n]={'kind':kind,'axis':axis,'position':p}

# Design-specific modeling primitives

def stencil(k,p,w=1.6,h=.8,accent='cyan',columns=16,rows=6,activity=0):
 x,y,z=p
 box(k,(x,y,z),(w+.11,h+.12,.11),'steel')
 box(k,(x,y,z+.066),(w,h,.012),'black')
 # emissive-style raised false-color digital pixels
 for j in range(rows):
  for i in range(columns):
   xx=x-w*.46+(i+.5)*w*.92/columns; yy=y-h*.43+(j+.5)*h*.86/rows
   ison=(i*13+j*7+activity*3)%11 < 6
   clr=accent if ison else 'dark'
   box(k,(xx,yy,z+.079),(w*.71/columns,h*.67/rows,.013),clr)
 for xx in (-w*.53,w*.53):box(k,(x+xx,y,z),(.034,h+.12,.13),'silver')

def data_path(k,pts,cc='cyan',thickness=.03,segments=1):
 pipe(k,pts,thickness,cc,10)
 for pp in pts[::max(1,segments)]:
  cyl(k,pp,thickness*1.5,thickness*.8,'gold','y',10)

def memory_module(k,x,y,z,accent='cyan',chips=8):
 box(k,(x,y,z),(.36,1.1,.09),'green')
 for t in range(15):box(k,(x-.145+t*.021,y-.55,z+.052),(.015,.10,.015),'gold')
 for j in range(chips):
  yy=y-.38+j*.108
  box(k,(x,yy,z+.067),(.29,.075,.04),'black')
  for n in range(3):box(k,(x-.09+n*.09,yy,z+.092),(.022,.035,.012),'steel')
 box(k,(x,y+.49,z+.052),(.29,.035,.01),accent)

def rack81(k,p,w=1.5,h=2.3,d=1.05,acc='cyan'):
 x,y,z=p
 rbox(k,p,(w,h,d),'graphite')
 box(k,(x,y,z+d/2+.035),(w*.94,h*.90,.035),'dark')
 for j in range(10):
  yy=y-h*.41+j*h*.084
  box(k,(x,yy,z+d/2+.056),(w*.84,h*.065,.025),'steel')
  for q in range(6):box(k,(x-w*.34+q*w*.13,yy,z+d/2+.080),(w*.07,h*.025,.02),acc if (q+j)%4!=0 else 'amber')
 for yy in (y-h*.46,y+h*.46):box(k,(x,yy,z+d/2+.075),(w*.91,.024,.034),'silver')
 for j in range(8):box(k,(x-w*.4+j*w*.11,y+h*.49,z+d/2*.1),(.026,.023,.04),'gold')

def data_core(k,p,acc='cyan'):
 x,y,z=p
 rbox(k,(x,y,z),(1.42,2.72,1.16),'graphite')
 box(k,(x,y,z+.61),(1.12,2.22,.045),'black')
 for yy in np.linspace(y-.9,y+.9,12):
  box(k,(x,yy,z+.649),(1.02,.035,.024),'steel')
  for j in range(8):
   if (j+int((yy-y)*10))%4!=0:
    box(k,(x-.40+j*.116,yy,z+.667),(.049,.024,.019),acc)
 for dx in (-.61,.61):
  for yy in np.linspace(y-1.0,y+1.0,19):box(k,(x+dx,yy,z+.05),(.024,.026,.70),'platinum')
 ring(k,(x,y+.92,z+.70),.32,.263,.04,'z',acc,32)
 for yy in (y-1.15,y+1.15):
  for dx in (-.52,.52):cyl(k,(x+dx,yy,z+.62),.055,.04,'gold','z',10)


def satellite(k,p,scale=1.):
 x,y,z=p
 box(k,(x,y,z),(.66*scale,.38*scale,.65*scale),'silver')
 box(k,(x,y+.20*scale,z),(.50*scale,.05*scale,.48*scale),'gold')
 for s in (-1,1):
  box(k,(x+s*1.10*scale,y,z),(1.45*scale,.035*scale,.64*scale),'blue')
  for u in range(7):box(k,(x+s*(.5+u*.20)*scale,y+.022*scale,z),(.014*scale,.014*scale,.64*scale),'silver')
  for v in range(3):box(k,(x+s*1.1*scale,y+.024*scale,z+(-.22+v*.22)*scale),(1.42*scale,.014*scale,.012*scale),'cyan')
 cyl(k,(x,y,z+.48*scale),.32*scale,.13*scale,'steel','z',24)
 ring(k,(x,y,z+.56*scale),.35*scale,.32*scale,.018*scale,'z','gold',32)
 for i in range(8):
  a=2*math.pi*i/8
  pipe(k,[(x,y,z+.57*scale),(x+math.cos(a)*.27*scale,y+math.sin(a)*.27*scale,z+.90*scale)],.008,'platinum',7)
 cyl(k,(x,y,z+.90*scale),.055*scale,.35*scale,'silver','z')

def bit_tile(k,p,acc='cyan',n=32):
 x,y,z=p
 box(k,p,(1.48,.68,.16),'black')
 for j in range(4):
  for i in range(8):
   col='corrupt' if i==5 and j==1 else ('green' if ((j*8+i*7+n)%13) < 7 else 'cyan')
   box(k,(x-.64+i*.18,y-.26+j*.17,z+.105),(.12,.104,.016),col)
 for j in range(6):box(k,(x-.60+j*.22,y-.36,z+.13),(.04,.02,.012),'gold')


def holo_screen(k,p,w,h,acc='cyan'):
 x,y,z=p
 box(k,p,(w+.11,h+.09,.07),'steel')
 box(k,(x,y,z+.046),(w,h,.011),'glass')
 for j in range(4):
  yy=y-h*.30+j*h*.20
  pipe(k,[(x-w*.40,yy,z+.07),(x-w*.2,yy+.05,z+.07),(x,yy-.05,z+.07),(x+w*.2,yy+.02,z+.07),(x+w*.40,yy+.06,z+.07)],.009,acc,7)
 for i in range(7):box(k,(x-w*.43+i*w*.14,y-h*.40,z+.066),(.05,.015,.014),'green' if i%3 else 'amber')


def steel_brace(k,p,wx,hy,deep,acc='cyan'):
 x,y,z=p
 for dx in (-wx/2,wx/2):box(k,(x+dx,y,z),(.09,hy,.10),'platinum')
 for yy in (-hy/2,hy/2):box(k,(x,y+yy,z),(wx+.13,.10,deep),'steel')
 for dx in (-wx*.43,wx*.43):box(k,(x+dx,y,z+deep*.5+.05),(.03,hy*.80,.035),acc)


def orbit_rings(k,c,r=2.9,height=4.1,columns=18):
 x,y,z=c
 for yy in (y-height*.48,y+height*.47):
  ring(k,(x,yy,z),r,r-.055,.10,'y','steel',72)
  ring(k,(x,yy+.08,z),r-.08,r-.11,.030,'y','cyan',72)
 for j in range(columns):
  a=2*math.pi*j/columns
  xx=x+math.cos(a)*r; zz=z+math.sin(a)*r
  cyl(k,(xx,y,zz),.045,height*.91,'silver','y',10)
  for yy in (y-1.3,y-.4,y+.48,y+1.3):cyl(k,(xx,yy,zz),.052,.04,'gold','y',10)
 for j in range(columns):
  a=2*math.pi*(j+.5)/columns
  xx=x+math.cos(a)*(r-.025);zz=z+math.sin(a)*(r-.025)
  # thin transparent pane tangent to cylindrical enclosure
  m=trimesh.creation.box(extents=(2*r*math.sin(math.pi/columns)*.92,height*.90,.018))
  m.apply_transform(trimesh.transformations.rotation_matrix(-a-math.pi/2,[0,1,0]))
  m.apply_translation((xx,y,zz));addmesh(k,m,'glass')


def cpu_package(k,p):
 x,y,z=p
 box(k,p,(3.34,.18,2.55),'navy')
 box(k,(x,y+.11,z),(3.0,.045,2.28),'silicon' if 'silicon' in P else 'black')
 for index,(xx,zz,cc) in enumerate([(-.71,-.58,'cyan'),(.71,-.58,'green'),(-.71,.58,'amber'),(.71,.58,'violet')]):
  box(k,(x+xx,y+.24,z+zz),(1.28,.18,.86),'black')
  box(k,(x+xx,y+.34,z+zz),(1.13,.038,.72),cc)
  for dx in np.linspace(-.52,.52,15):
   for edge in (-1,1):box(k,(x+xx+dx,y+.23,z+zz+edge*.48),(.030,.035,.080),'gold')
  for dz in np.linspace(-.34,.34,10):
   for edge in (-1,1):box(k,(x+xx+edge*.66,y+.23,z+zz+dz),(.08,.035,.032),'gold')
  for i in range(4):box(k,(x+xx-.4+i*.21,y+.365,z+zz+.14),(.13,.014,.016),'white')
 for r in (.23,.34):ring(k,(x,y+.38,z),r,r-.019,.016,'y','gold',32)
 for edge in (-1,1):
  for i in range(30):
   box(k,(x+edge*1.52,y+.02,z-1.07+i*.074),(.12,.044,.045),'gold')


def component_detail(k,i,p):
 x,y,z=p
 # Original silhouette/structural geometry for each semantic category.
 if i==0:
  box(k,(0,.33,0),(24.0,.62,12.4),'navy')
  box(k,(0,.65,0),(23.92,.04,12.28),'steel')
  for xx in np.linspace(-11.65,11.65,32):
   box(k,(xx,.675,0),(.028,.014,12.0),'dark')
   for zz in np.linspace(-5.80,5.80,12):box(k,(xx,.698,zz),(.055,.024,.09),'cyan' if abs(xx)<3.6 else 'amber')
  for zz in (-6.05,6.05):
   box(k,(0,.77,zz),(23.7,.17,.18),'graphite')
   for xx in np.linspace(-11.5,11.5,80):box(k,(xx,.875,zz),(.065,.03,.045),'cyan' if zz<0 else 'amber')
  for xx in (-11.6,11.6):
   box(k,(xx,.76,0),(.16,.15,11.9),'dark')
   for zz in np.linspace(-5.85,5.85,48):box(k,(xx,.85,zz),(.034,.023,.064),'green')
  for xx,zz in [(-8.5,-3.9),(8.5,-3.9),(-8.5,3.4),(8.5,3.4),(0,-3.2),(0,3.0)]:
   box(k,(xx,.73,zz),(1.65,.14,1.4),'silver')
   for dx in (-.65,.65):
    for dz in (-.46,.46):cyl(k,(xx+dx,.81,zz+dz),.085,.08,'black','y',12)
  return
 if i==1:
  orbit_rings(k,(0,3.05,0),3.07,4.35,20)
  for a in range(18):
   theta=2*math.pi*a/18
   xx=3.10*math.cos(theta);zz=3.10*math.sin(theta)
   for yy in (1.32,3.0,4.78):
    cyl(k,(xx,yy,zz),.092,.09,'steel','y',12)
  for yy in (1.00,1.18,5.13):ring(k,(0,yy,0),3.20,3.10,.09,'y','silver',72)
  return
 if i==2:
  box(k,(x,y,z),(3.75,.44,3.00),'graphite');box(k,(x,y+.27,z),(3.46,.09,2.75),'steel')
  for dx in (-1.53,1.53):
   for dz in (-1.16,1.16):
    cyl(k,(dx,y+.32,dz),.15,.16,'gold','y',20)
  for zz in np.linspace(-1.30,1.30,27):box(k,(x-1.78,y+.10,zz),(.095,.044,.057),'gold')
  return
 if i==3: cpu_package(k,p);return
 if i==4:
  box(k,p,(1.15,.35,.95),'steel')
  for j,col in enumerate(('cyan','green','amber','violet')):
   cyl(k,(x-.36+j*.24,y+.20,z+.35),.072,.24,col,'z',16)
   data_path(k,[(x-.36+j*.24,y+.20,z+.41),(x-.36+j*.24,y+.45,z+.70)],col,.012)
  return
 if i==5:
  holo_screen(k,p,6.9,2.05,'cyan')
  satellite(k,(x-.18,y+.07,z+.22),.62)
  pipe(k,[(x-2.2,y-.65,z+.23),(x-1.2,y-.12,z+.23),(x+.65,y+.12,z+.23),(x+2.5,y+.68,z+.23)],.027,'emerald',10)
  for j in range(12):cyl(k,(x-2.2+j*.40,y-.86,z+.22),.024,.04,'gold','z',8)
  return
 if i==6:
  ring(k,(x,y,z),1.42,1.32,.10,'y','violet',64)
  ring(k,(x,y+.40,z),1.05,.99,.06,'y','magenta',64)
  ring(k,(x,y-.40,z),1.08,1.03,.06,'y','cyan',64)
  for j in range(42):
   a=2*math.pi*j/42; r=.5+(j%9)*.12
   px=x+r*math.cos(a); pz=z+r*math.sin(a)
   data_path(k,[(px,y+.6,pz),(px+math.sin(a)*.12,y-.35,pz+math.cos(a)*.17)],'violet' if j%3 else 'magenta',.009)
  return
 if i==7:
  ring(k,(0,3.5,0),2.55,2.49,.06,'y','steel',64)
  for j in range(12):
   a=2*math.pi*j/12
   box(k,(2.4*math.cos(a),2.0,2.4*math.sin(a)),(.10,1.15,.12),'purpleglass')
  return
 if i==13:
  box(k,p,(6.2,2.1,.19),'steel')
  box(k,(x,y,z+.12),(5.95,1.87,.024),'glass')
  # 6 virtual codewords, 72 bit registers each (visible, separately colored)
  for j in range(6):
   for c in range(72):
    xx=x-2.85+c*(5.70/71); yy=y+.79-j*.29
    clr='corrupt' if c==38 and j==3 else 'green' if (c*13+j*5)%13 < 8 else 'cyan'
    box(k,(xx,yy,z+.151),(.057,.15,.016),clr)
   box(k,(x-3.00,y+.79-j*.29,z+.16),(.030,.20,.02),'gold')
  for i0 in range(18):box(k,(x-2.7+i0*.32,y-.92,z+.16),(.12,.05,.02),'blue')
  return
 if i in (14,):
  rack81(k,p,2.80,3.85,1.25,'amber')
  return
 if i in (15,16):
  box(k,p,(1.16,2.65,.70),'graphite')
  for j in range(6):memory_module(k,x-.39+j*.155,y,z+.42,'green' if i==15 else 'cyan')
  for yy in np.linspace(y-1.15,y+1.10,20):box(k,(x-.59,yy,z+.42),(.04,.025,.06),'gold')
  return
 if i in (29,30,31):
  data_core(k,p,['cyan','amber','green'][i-29]);return
 if i==33:
  frame(k,(x,3.5,z),(2.4,1.7,1.7),.095,True,True)
  box(k,(x,y,z),(1.7,1.1,.87),'glass')
  for j,col in enumerate(('cyan','amber','green')):
   data_path(k,[(x-1.13+j*.8,y+.55,z+.56),(x-.54+j*.52,y+.2,z+.65),(x,y,z+.66)],col,.03)
  ring(k,(x,y,z+.67),.34,.28,.06,'z','emerald',32)
  return
 if i==40:
  box(k,p,(1.18,2.12,.71),'black')
  steel_brace(k,p,1.13,2.1,.20,'cyan')
  for yy in (y-.75,y-.50,y-.25,y+.0,y+.25):box(k,(x,yy,z+.41),(.84,.075,.022),'gold')
  return
 if i in (41,):
  box(k,p,(2.8,.17,1.4),'steel')
  for q in range(4):
   col=('cyan','green','amber','violet')[q]
   data_path(k,[(x-1.16,y+.13,z-.47+q*.28),(x-.1,y+.13,z-.47+q*.28),(x+.88,y+.13,z-.41+q*.28)],col,.021)
  return
 if i==51:
  satellite(k,p,1.05);return
 if i==61:
  holo_screen(k,p,6.9,1.75,'cyan')
  for j,col in enumerate(('green','cyan','amber','violet')):
   box(k,(x-2.25+j*1.47,y-.64,z+.12),(1.05,.10,.022),col)
  return
 if i==69:
  ring(k,(x,y,z),.40,.28,.07,'z','gold',48)
  ring(k,(x,y,z+.04),.26,.20,.04,'z','green',48)
  return
 if i in (26,):
  rbox(k,p,(1.8,1.55,1.28),'graphite')
  for j in range(7):
   yy=y-.50+j*.17
   box(k,(x,yy,z+.685),(1.50,.085,.048),'gold')
   for t in range(12):box(k,(x-.64+t*.12,yy,z+.718),(.052,.033,.018),'amber')
  return
 if i in (25,):
  box(k,p,(1.25,1.45,.86),'steel')
  box(k,(x,y,z+.45),(1.06,1.22,.043),'corrupt')
  for j in range(6):box(k,(x-.45+j*.18,y,z+.48),(.025,1.1,.04),'black')
  return
 if i in (62,63,64):
  col={62:'green',63:'cyan',64:'gold'}[i]
  for r in (1.1,.83,.65):
   ring(k,(x,y,z),r,r-.045,.04,'z',col,64)
  for j in range(24):
   t=2*math.pi*j/24
   box(k,(x+math.cos(t)*.91,y+math.sin(t)*.91,z+.08),(.052,.058,.031),col)
  return
 if i in (9,17,18,19,20,21,22,23,24,27,28,32,34,35,36,37,38,39,42,43,44,45,46,47,48,49,50,52,53,54,55,56,57,58,59,60,66,67,68):
  # Choose a distinct instrument silhouette (rack / chunky analyzer / protected panel).
  if i in (55,57):rack81(k,p,1.62,1.45,.74,'green')
  elif i in (58,60):rack81(k,p,1.14,1.20,.77,'red' if i==60 else 'green')
  elif i in (9,32,37,38,39,46,47,50,56):rack81(k,p,1.18,.98,.72,'amber' if i in (9,38) else 'cyan')
  else:
   box(k,p,(1.00,.65,.76),'steel')
   box(k,(x,y,z+.395),(.90,.56,.022),'graphite')
   for t in range(4):box(k,(x-.35+t*.22,y+.26,z+.42),(.12,.04,.025),'green' if t%3 else 'amber')
   holo_screen(k,(x,y+.04,z+.46),.69,.32,'violet' if i in (28,53,66) else 'cyan')
   for j in range(4):knob(k,x-.30+j*.19,y-.21,z+.44,'red' if i in (21,53) else 'green')
  if i in (22,23):
   for j in range(26):box(k,(x-.58+j*.046,y+.52,z+.37),(.028,.14,.07),'cyan' if j%5 else 'amber')
  if i==59:
   box(k,(x,y+.45,z),(1.45,.11,.95),'dark')
   for dx in (-.52,.52):
    cyl(k,(x+dx,y+.57,z+.33),.17,.12,'red','y',20)
  return
 if i in (10,11,12,28,65):
  holo_screen(k,p,2.0,1.05,'violet' if i in (11,12) else 'cyan')
  return
 # all remaining modules are uniquely shaped instrument/control surfaces.
 box(k,p,(1.15,.86,.84),'steel')
 holo_screen(k,(x,y,z+.46),.85,.45,'green' if i%4==0 else 'cyan')
 for s in (-1,1):box(k,(x+s*.49,y,z+.44),(.025,.66,.038),'silver')


def pivot_geometry(k,piv,meta):
 x,y,z=meta['position'];up=piv.upper(); kind=meta['kind']
 with moving(piv):
  if 'PARTICLE_EVENT_HOLOGRAM' in up:
   for j in range(36):
    t=2*math.pi*j/36;r=.32+(j%9)*.09
    data_path(k,[(x+r*math.cos(t),y+.26,z+r*math.sin(t)),(x+.12+r*math.cos(t),y-.28,z+.12+r*math.sin(t))], 'violet' if j%2 else 'magenta',.014)
  elif 'BITSTORM_PROJECTOR_ROTATION' in up:
   ring(k,(x,y,z),1.75,1.65,.10,'y','violet',72)
   for j in range(12):
    t=2*math.pi*j/12
    cyl(k,(x+1.65*math.cos(t),y,z+1.65*math.sin(t)),.065,.17,'gold','y')
  elif 'MEMORY_BANK_' in up:
   box(k,(x,y,z),(.63,2.0,.18),'graphite')
   for j in range(8):memory_module(k,x-.24+j*.065,y,z+.12,'green' if 'BANK_A' in up else 'cyan',chips=6)
  elif 'SINGLE_BIT_CORRECTION' in up or 'MEMORY_REPAIR_ARC' in up:
   ring(k,(x,y,z),.48,.41,.06,'z','emerald',40)
   for j in range(18):
    a=2*math.pi*j/18
    box(k,(x+math.cos(a)*.46,y+math.sin(a)*.46,z+.06),(.038,.038,.025),'green')
  elif 'ECC_SYNDROME_SCAN' in up or 'SECDED_PARITY' in up or 'MEMORY_ADDRESS' in up:
   bit_tile(k,(x,y,z+.45),'cyan',16)
  elif 'QUARANTINE_GATE' in up or 'SAFE_COMMAND_GATE' in up:
   box(k,(x,y,z),(.95,.86,.18),'dark')
   box(k,(x,y,z+.12),(.86,.75,.04),'red' if 'QUARANTINE' in up else 'green')
   for j in range(8):box(k,(x-.36+j*.105,y,z+.16),(.026,.75,.015),'steel')
  elif 'GOLDEN_CHECKPOINT_RESTORE' in up:
   box(k,(x,y,z),(.92,.35,.18),'vault')
   for j in range(8):box(k,(x-.40+j*.116,y+.20,z+.04),(.05,.05,.03),'green')
  elif 'TMR_VOTER_GATE' in up or 'TRIPLE_VOTING_HOLOGRAM' in up:
   ring(k,(x,y,z),.52,.44,.05,'z','green',40)
   for r,c in ((.33,'cyan'),(.23,'gold')):ring(k,(x,y,z+.09),r,r-.04,.035,'z',c,32)
  elif 'COMPUTE_LANE_' in up:
   for j in range(22):
    box(k,(x-.35+j*.034,y+.30-j*.012,z+.69),(.026,.026,.016),'emerald' if 'LANE_C' in up else 'cyan' if 'LANE_A' in up else 'amber')
  elif 'WATCHDOG_TIMER' in up:
   ring(k,(x,y,z+.46),.26,.20,.045,'z','cyan',32)
   pipe(k,[(x,y,z+.53),(x+.18,y+.08,z+.53)],.015,'gold')
  elif 'FAULT_CONSOLE_GUARD' in up or 'FAULT_INJECTION_CHAMBER_DOOR' in up:
   box(k,(x,y+.14,z),(.96,.11,.66),'platinum')
   box(k,(x,y+.21,z),(.81,.018,.56),'glass')
  elif 'EMERGENCY_SAFE_HOLD' in up:
   box(k,(x,y,z),(.43,.58,.33),'black')
   cyl(k,(x,y,z+.23),.17,.14,'red','z',24)
  elif 'POWER_DISTRIBUTION_ISOLATOR' in up:
   box(k,(x,y,z),(.80,.70,.30),'dark')
   for j in range(6):box(k,(x-.30+j*.12,y+.15,z+.20),(.06,.18,.04),'gold')
  elif 'SCRUBBER_SCAN_HEAD' in up:
   box(k,(x,y,z),(.43,.35,.62),'steel')
   for j in range(16):box(k,(x-.17+j*.022,y-.20,z+.30),(.012,.10,.035),'cyan')
  elif 'OPTICAL_TELEMETRY_DOCK' in up:
   box(k,(x,y,z),(.50,.28,.58),'platinum')
   for j,col in enumerate(('cyan','green','amber','violet')):cyl(k,(x-.18+j*.12,y+.17,z+.32),.040,.14,col,'z',12)
  elif 'EVENT_DIRECTOR' in up or 'COMPUTE_HEALTH_PROBE' in up:
   box(k,(x,y,z),(.56,.21,.40),'steel')
   for j in range(5):cyl(k,(x-.22+j*.11,y-.18,z),.024,.28,'silver','y',10)
  elif 'MISSION_CERTIFICATION_BEACON' in up:
   ring(k,(x,y,z),.41,.33,.055,'z','green',48)
   for j in range(12):
    a=j*2*math.pi/12
    box(k,(x+.38*math.cos(a),y+.38*math.sin(a),z+.04),(.041,.043,.023),'gold')
  elif 'ATTITUDE_ACTUATOR' in up:
   ring(k,(x,y,z),.55,.51,.045,'y','cyan',48)
   satellite(k,(x,y+.16,z),.47)
  elif 'DEPENDABILITY_SCADA' in up or 'MISSION' in up and 'DISPLAY' in up:
   holo_screen(k,(x,y,z),2.2,1.25,'cyan')
  elif 'BIT' in up or 'CRC' in up or 'PACKET' in up or 'TELEMETRY' in up or 'DATA' in up or 'ECC' in up or 'SYNDROME' in up:
   bit_tile(k,(x,y,z+.12),'green' if 'VALID' in up or 'CORRECTION' in up else 'cyan',24)
  elif kind=='rotation':
   ring(k,(x,y,z),.28,.20,.08,'z','steel',28)
   box(k,(x+.26,y,z),(.28,.05,.04),'gold')
  elif kind=='translation':
   box(k,(x,y,z),(.64,.28,.45),'steel')
   for j in range(6):box(k,(x-.25+j*.09,y+.16,z),(.04,.042,.22),'cyan')
  else:
   holo_screen(k,(x,y,z),1.05,.64,'green' if 'WATCHDOG' in up else 'cyan')


# Add 200 visually real sockets on control equipment. These are art-only, not SparkTerminals.
counts={}
for i in range(1,70):
 if i in (5,6,7,11,12,13,28,62,63,64,66):counts[i]=0
 elif i in (0,1,2,3,4):counts[i]=1 if i in (2,3,4) else 0
 elif i in (14,15,16,18,19,20,22,29,30,31,32,33,40,41,42,43,44,45,46,47,48,55,56,57,58):counts[i]=6
 elif i in (17,21,23,24,25,26,27,34,35,36,37,38,39,49,50,52,53,54,59,60,67,68):counts[i]=2
 else:counts[i]=2
# Stable allocation to exactly 200 endpoints, favor key functional equipment.
while sum(counts.values())<200:
 for i in (19,56,55,33,42,43,40,22,45,58,29,30,31,9,18):
  counts[i]=counts.get(i,0)+1
  if sum(counts.values())==200:break
while sum(counts.values())>200:
 for i in reversed(sorted(counts)):
  if counts[i]>1:
   counts[i]-=1
   if sum(counts.values())==200:break
assert sum(counts.values())==200
SOCKETS=[]


def add_visual_sockets(k,i):
 if counts.get(i,0)<=0:return
 # All sockets are part of exported meshes, with exact anchor coordinates in CSV.
 meshes=M[k];v=np.vstack([me.vertices for me,c,g in meshes]);lo=v.min(0);hi=v.max(0)
 n=counts[i]
 cx=float((lo[0]+hi[0])*.5);cy=float((lo[1]+hi[1])*.5)
 width=max(.25,float(hi[0]-lo[0]));height=max(.25,float(hi[1]-lo[1]))
 # Physically accessible z-forward face, with depth padding; sockets never pretend to be functional.
 z=float(hi[2]+.060)
 for j in range(n):
  # Plan a 2-row terminal block; keep within equipment visual footprint.
  cols=min(6,max(2,(n+1)//2)); row=j//cols;column=j%cols
  xx=cx+(column-(cols-1)*.5)*min(.15,width*.72/max(1,cols-1))
  yy=cy+(row-(math.ceil(n/cols)-1)*.5)*min(.13,height*.45)
  center=(float(round(xx,5)),float(round(yy,5)),float(round(z,5)))
  cyl(k,center,.049,.045,'dark','z',12)
  ring(k,(xx,yy,z+.018),.060,.043,.025,'z','gold',16)
  cyl(k,(xx,yy,z+.033),.027,.019,'cyan' if j%4 else 'amber','z',12)
  SOCKETS.append((i,center,j))

for i,k in enumerate(names):
 p=POS.get(i,(0,1.5,0))
 component_detail(k,i,p)
 for piv,meta in PV.get(k,{}).items():pivot_geometry(k,piv,meta)
 assert M.get(k),(i,k)
 assert {grp for _,_,grp in M[k] if grp}==set(PV.get(k,{})),('Missing moving geometry',i,k)
 add_visual_sockets(k,i)

print('MODELED LEVEL81',len(names),'models',sum(len(M[k]) for k in names),'parts',sum(sum(len(mesh.faces) for mesh,c,g in M[k]) for k in names),'triangles',len(SOCKETS),'visual terminal sockets',flush=True)
manifest=[]
for i,k in enumerate(names):
 f=f'{i:02d}_L81_{k}'
 export_fbx(k,MD/(f+'.fbx'))
 export_glb(k,GD/(f+'.glb'))
 parts=M[k];tris=sum(len(mesh.faces) for mesh,c,g in parts);verts=np.vstack([mesh.vertices for mesh,c,g in parts])
 manifest.append({'index':i,'name':'L81_'+k,'file':f+'.fbx','glb':f+'.glb','triangle_count':tris,'mesh_parts':len(parts),'bbox_min':np.round(verts.min(0),5).tolist(),'bbox_max':np.round(verts.max(0),5).tolist(),'rig_groups':list(PV.get(k,{})),'animation_pivots':{p:list(meta['position']) for p,meta in PV.get(k,{}).items()},'visual_terminal_sockets':counts.get(i,0)})
 print('EXPORTED',i+1,'/70',f,len(parts),'mesh parts',tris,'triangles',flush=True)

assemble='Level81_Complete_ArtKit_Showcase'
M[assemble]=[r for k in names for r in M[k]]
PV[assemble]={p:meta for k,gr in PV.items() for p,meta in gr.items()}
export_fbx(assemble,MD/(assemble+'.fbx'))
export_glb(assemble,GD/(assemble+'.glb'))
verts=np.vstack([mesh.vertices for mesh,c,g in M[assemble]])
bounds=np.stack([verts.min(0),verts.max(0)])
config={'level_id':'level_81','display_name':'Outlast the Bitstorm','subtitle':'Advanced Fault-Tolerant Computing and Data Integrity','individual_models':70,'individual_glbs':70,'assembled_fbx':'Models/'+assemble+'.fbx','assembled_glb':'GLB/'+assemble+'.glb','individual_triangle_count':sum(r['triangle_count'] for r in manifest),'individual_mesh_parts':sum(r['mesh_parts'] for r in manifest),'animation_parent_pivot_groups':len(pivots),'planned_player_wires':130,'planned_unique_functional_terminals':200,'modeled_art_only_terminal_sockets':len(SOCKETS),'workstation_width_m_nominal':24.0,'workstation_world_bounds_m':bounds.round(5).tolist(),'fbx_numeric_units':'centimetres','fbx_unit_scale_factor':1,'glb_numeric_units':'metres','axes':'Y up / X width / Z front','assembly_origin':[0,0,0],'actual_player_wires_included':False,'functional_spark_terminals_included':False,'virtual_fault_simulator_included':False,'six_authoritative_objectives_implemented':False,'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(config,indent=2))
with (ROOT/'Animation_Rigs.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Pivot','Model','Kind','Axis','X','Y','Z','Description'])
 for i,p,kind,axis,pos in pivots:w.writerow([p,'L81_'+names[i],kind,axis,*pos,'Actual mesh-owning FBX and GLB parent transform'])
with (ROOT/'Art_Assembly.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Index','File','LocalX','LocalY','LocalZ','Scale','Role'])
 for r in manifest:w.writerow([r['index'],r['file'],0,0,0,1,'Common-origin modular fault-tolerance workstation'])
with (ROOT/'Component_Roles.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Model','Category','Function','UnityGameplayStatus'])
 for r in manifest:
  i=r['index'];cat='Infrastructure' if i<=7 else 'Fault injection' if i<=12 else 'Memory and ECC' if i<=28 else 'TMR and watchdog' if i<=40 else 'Telemetry integrity' if i<=48 else 'Mission guidance' if i<=54 else 'Safety and power' if i<=60 else 'Holograms and diagnostics'
  w.writerow([r['name'],cat,r['name'].replace('L81_',''),'Art only; functional device not included'])
with (ROOT/'Visual_Terminal_Sockets_200.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['AnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','UnityStatus'])
 for j,(i,p,index) in enumerate(SOCKETS,1):w.writerow([f'P{j:03d}','L81_'+names[i],*p,'Visual gold-rimmed wire socket','PROPOSED_ANCHOR_NO_SPARKTERMINAL'])
(ROOT/'Material_Palette.json').write_text(json.dumps({k:{'rgba_8bit':list(v),'surface_intent':'transparent art' if v[3]<255 else 'opaque procedural preview'} for k,v in P.items()},indent=2))
print('LEVEL81 EXPORT COMPLETE',len(manifest),'models',len(pivots),'pivots',len(SOCKETS),'sockets','bounds',bounds.tolist(),flush=True)
