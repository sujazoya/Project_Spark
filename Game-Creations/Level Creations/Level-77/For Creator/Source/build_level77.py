#!/usr/bin/env python3
"""Project Spark Level 77: original modular triple-station copper CMP 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical/implantation gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level77_HD_AAA')
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




# LEVEL 77 - REVEAL THE CIRCUIT - original triple-stage selective copper planarization workstation
names=['IndustrialCopperPlanarizationBase','TransparentTripleCMPEnclosure','CopperPlatedWaferCassette','AutomatedWaferHandlingRobot','PreCMPWaferInspectionScanner','ElectroplatedDamasceneWafer','CopperDielectricLayerCrossSection','HolographicCopperOverburdenMap','BulkCopperPolishingPlaten','BulkCopperPolishingPad','BulkPlatenServoDrive','BulkWaferCarrierHead','BulkCarrierVerticalActuator','BulkDownforceLoadCell','MultiZonePressureControlBank','BulkCopperSlurryReservoir','BulkSlurryDeliveryPump','BulkSlurryFlowTransmitter','MotorizedBulkSlurryArm','BulkSlurryDispenseNozzle','BulkPadConditioningAssembly','BulkPadWearMeasurementProbe','OpticalCopperRemovalEndpointSensor','EddyCurrentCopperThicknessGauge','BulkCopperEndpointController','OverheadWaferTransferGantry','SoftLandingPolishingPlaten','SoftLandingPrecisionPad','SoftLandingCarrierHead','SoftLandingForceRegulator','SelectiveProcessFluidManifold','BarrierClearingPolishingPlaten','BarrierSelectivePolishingPad','BarrierClearingCarrierHead','BarrierProcessFluidReservoir','BarrierFluidMeteringPump','BarrierFluidFlowSensor','BarrierFluidDispenseNozzle','DualWavelengthBarrierEndpointSpectrometer','TransparentOpticalEndpointViewport','BarrierClearanceTomographyMonitor','MaterialSelectivityAnalyzer','PolishingMotorTorqueAnalyzer','WaferInterfaceTemperatureSensor','CopperCorrosionPotentialMonitor','CopperSurfaceProtectionModule','PostCMPBrushCleaningChamber','PrecisionMegasonicCleaningModule','CopperResidueChemistryMonitor','UltrapureRinseWaterModule','HighSpeedWaferSpinDryer','WaferSurfaceDefectInspectionCamera','3DCopperDishingErosionScanner','ResidualCopperInspectionScanner','InterlineLeakageTestInstrument','ViaContinuityKelvinProbe','DielectricIntegrityAnalyzer','ThreeStageCMPProcessController','CopperPlanarizationPLCRack','CMPEmergencySafetyStation','HolographicCopperInterconnectSCADA','Regulated5VControlSupply']
assert len(names)==62
PV={}
def reg(i,n,p,kind='rotation',axis='y'): PV.setdefault(names[i],{})[n]=dict(position=tuple(float(x) for x in p),kind=kind,axis=axis)

# Model dimensions / station placement; all coordinates in metres, X width, Y up, Z front.
SX={ 'A':-5.35,'B':0.,'C':5.35 }
SZ=-1.46

def accent(letter):return {'A':'amber','B':'cyan','C':'violet'}[letter]
def platencenter(letter):return (SX[letter],1.66,SZ)
def carriercenter(letter):return (SX[letter],3.28,SZ)

def detail_rack(k,c,ext=(1.15,1.40,.70),light='cyan'):
 x,y,z=c;hd_cabinet(k,c,ext,light,2)
 for j in range(5):
  yy=y-ext[1]*.38+j*ext[1]*.14
  box(k,(x-.36*ext[0],yy,z+ext[2]*.53),(.12,.042,.017),'silver')
 for j in range(3):pipe(k,[(x-.35+j*.30,y+ext[1]*.52,z),(x-.35+j*.30,y+ext[1]*.59,z-.09)],.022,'copper')

def platform(k,pos,ext=(1.6,.30,1.5),highlight='cyan'):
 x,y,z=pos;concrete(k,pos,ext)
 for xx in (x-ext[0]/2+.13,x+ext[0]/2-.13):
  for zz in (z-ext[2]/2+.13,z+ext[2]/2-.13):
   box(k,(xx,y-ext[1]*.55-.40,zz),(.14,.83,.14),'steel')
 box(k,(x,y+ext[1]*.52,z+ext[2]*.35),(ext[0]*.7,.032,.035),highlight)

def cmp_drum(k,letter,with_pad=False):
 x,y,z=platencenter(letter);c=accent(letter)
 cyl(k,(x,1.41,z),1.57,.34,'steel','y',60)
 for r in (1.48,1.35,1.13,.92,.62):ring(k,(x,1.61,z),r,r-.045,.035,'y','silver',56)
 cyl(k,(x,1.60,z),1.42,.08,'dark','y',60)
 for a in range(56):
  theta=a*2*math.pi/56
  px=x+1.55*math.cos(theta);pz=z+1.55*math.sin(theta)
  cyl(k,(px,1.62,pz),.025,.046,'black','y',8)
 for j in range(4):
  rr=1.17-.20*j
  ring(k,(x,1.65+j*.004,z),rr,rr-.015,.012,'y',c,52)
 if with_pad:
  cyl(k,(x,1.735,z),1.40,.075,'pad','y',56)
  for n in range(48):
   a=2*math.pi*n/48
   for rr in (.36,.70,1.01,1.26):
    bx=x+rr*math.cos(a);bz=z+rr*math.sin(a)
    cyl(k,(bx,1.78,bz),.025,.011,'padgroove','y',8)
  for j in range(8):
   a=j*math.tau/8
   pipe(k,[(x+.17*math.cos(a),1.79,z+.17*math.sin(a)),(x+1.28*math.cos(a),1.79,z+1.28*math.sin(a))],.018,'light',8)

def carrier(k,letter,soft=False):
 x,y,z=carriercenter(letter);cc=accent(letter)
 for dy,r in ((-.47,.79),(-.26,.90),(-.04,.96),(.15,.72)):
  cyl(k,(x,y+dy,z),r,.13,'silver' if dy>-.35 else 'dark','y',40)
  ring(k,(x,y+dy+.075,z),r*1.02,r*.91,.05,'y',cc,40)
 cyl(k,(x,y+.38,z),.35,.55,'steel','y',24)
 cyl(k,(x,y+.74,z),.23,.29,'silver','y',24)
 for j in range(12):
  a=j*math.tau/12
  px=x+.82*math.cos(a);pz=z+.82*math.sin(a)
  cyl(k,(px,y-.15,pz),.045,.22,'dark','y',12)
  cyl(k,(px,y+.01,pz),.025,.09,cc,'y',12)
 waferdisc(k,(x,y-.51,z),.69,.03,'copper',True)
 for j in range(16):
  a=j*math.tau/16
  px=x+.9*math.cos(a);pz=z+.9*math.sin(a)
  box(k,(px,y-.24,pz),(.06,.06,.06),'dark')

def pedestal(k,p,cc='cyan'):
 x,y,z=p;platform(k,(x,.65,z),(1.6,.22,1.35),cc)
 for j in range(3):box(k,(x,y-0.15-j*.12,z+.58),(.64,.052,.09),'silver')

def reservoir(k,p,cc='cyan'):
 x,y,z=p
 cyl(k,(x,y,z),.55,1.25,'steel','y',32)
 cyl(k,(x,y-.06,z),.50,1.04,'glass','y',32)
 cyl(k,(x,y-.22,z),.47,.65,'slurry','y',32)
 for dy in (-.60,.59):ring(k,(x,y+dy,z),.60,.53,.065,'y','silver',32)
 for j in range(8):
  a=j*math.tau/8
  box(k,(x+.61*math.cos(a),y+.31,z+.61*math.sin(a)),(.08,.16,.08),'steel')
 fluidpipe(k,[(x,y+.75,z),(x,y+.91,z+.2),(x+.25,y+.91,z+.34)],.045,cc)
 fluidpipe(k,[(x-.52,y-.4,z),(x-.75,y-.4,z-.24)],.045,cc)
 multisensor(k,x,y+.15,z+.56,'flow',.24)

def fluidpipe(k,pts,r=.065,col='cyan'):
 pipe(k,pts,r,col,12)
 for xyz in (pts[0],pts[-1]):
  cyl(k,xyz,r*1.4,r*.20,'silver','y',16)

def polishbox(k,c,accentcolor='cyan'):
 x,y,z=c
 detail_rack(k,c,(1.18,1.28,.65),accentcolor)
 for dx in (-.42,.42):
  for dy in (-.48,.48):
   screw(k,(x+dx,y+dy,z+.36),.030,'z')

def pad_conditioner(k,letter):
 x=SX[letter]+1.10;y=2.1;z=SZ-1.34
 box(k,(x,y+.68,z),(.22,1.4,.26),'steel')
 pipe(k,[(x,y+1.27,z),(x-.45,y+1.22,z+.46),(x-.87,y+.74,z+.87)],.08,'silver')
 cyl(k,(x-.87,y+.65,z+.87),.30,.16,'dark','y',24)
 for j in range(36):
  a=j*math.tau/36
  cyl(k,(x-.87+.25*math.cos(a),y+.56,z+.87+.25*math.sin(a)),.025,.035,'gold','y',8)

def compact_wafer(k,pos,r=.63,col='copper'):
 x,y,z=pos;waferdisc(k,pos,r,.028,col,True)
 for a in range(0,360,22):
  t=math.radians(a);xx=x+r*.71*math.cos(t);zz=z+r*.71*math.sin(t)
  box(k,(xx,y+.028,zz),(.07,.013,.07),'gold')

def holo(k,pos,w=1.5,h=.85,c='cyan'):
 x,y,z=pos
 box(k,pos,(w,h,.075),'navy')
 box(k,(x,y,z+.048),(w*.93,h*.90,.016),'black')
 for i in range(14):
  bx=x-w*.41+i*w*.065
  q=.22*math.sin(i*.45)+.12*math.sin(i*1.17)
  box(k,(bx,y+q*h*.7,z+.065),(w*.035,h*.15+.04*(i%3),.008),c if i%3 else 'green')
 for j in range(4):box(k,(x-w*.26,y-h*.36+j*.040,z+.067),(w*.35,.014,.009),'silver')

# 44 planned real named parent pivots, each actually containing at least one mesh group.
RIGS={
 3:[('ANIM_WAFER_ROBOT_BASE',(-8.15,1.0,2.2),'rotation','y'),('ANIM_WAFER_ROBOT_ARM',(-8.03,1.9,2.2),'rotation','z'),('ANIM_WAFER_ROBOT_TOOL',(-7.25,2.2,2.2),'rotation','y')],
 6:[('ANIM_LAYER_CROSS_SECTION',(0,4.40,3.35),'indicator','none')],
 8:[('ANIM_BULK_PLATEN_ROTATION',(-5.35,1.66,-1.46),'rotation','y')],
 9:[('ANIM_BULK_PAD_ROTATION',(-5.35,1.74,-1.46),'rotation','y')],
 11:[('ANIM_BULK_CARRIER_SPIN',(-5.35,3.28,-1.46),'rotation','y')],
 12:[('ANIM_BULK_CARRIER_LIFT',(-5.35,3.90,-1.46),'translation','y')],
 13:[('ANIM_BULK_PRESSURE_RING',(-5.35,3.05,-1.46),'translation','y')],
 16:[('ANIM_BULK_SLURRY_PUMP',(-8.40,1.20,-2.97),'rotation','x')],
 19:[('ANIM_BULK_SLURRY_NOZZLE',(-6.48,2.04,-.72),'translation','z')],
 20:[('ANIM_BULK_PAD_CONDITIONER',(-4.25,2.90,-2.80),'rotation','y')],
 22:[('ANIM_BULK_OPTICAL_SENSOR',(-6.28,2.10,-.53),'translation','y')],
 23:[('ANIM_EDDY_CURRENT_SENSOR',(-5.67,2.24,-.55),'translation','y')],
 25:[('ANIM_TRANSFER_GANTRY_X',(0,4.77,-1.46),'translation','x'),('ANIM_TRANSFER_GANTRY_Z',(0,4.77,-1.46),'translation','z'),('ANIM_TRANSFER_GRIPPER',(0,4.20,-1.46),'translation','y')],
 26:[('ANIM_SOFT_PLATEN_ROTATION',(0,1.66,-1.46),'rotation','y')],
 27:[('ANIM_SOFT_PAD_ROTATION',(0,1.74,-1.46),'rotation','y')],
 28:[('ANIM_SOFT_CARRIER_SPIN',(0,3.28,-1.46),'rotation','y')],
 29:[('ANIM_SOFT_CARRIER_LIFT',(0,3.90,-1.46),'translation','y'),('ANIM_SOFT_PRESSURE_ACTUATOR',(0,3.31,-1.46),'translation','y')],
 30:[('ANIM_SOFT_PROCESS_FLUID_NOZZLE',(-1.12,2.10,-.65),'translation','z')],
 31:[('ANIM_BARRIER_PLATEN_ROTATION',(5.35,1.66,-1.46),'rotation','y')],
 32:[('ANIM_BARRIER_PAD_ROTATION',(5.35,1.74,-1.46),'rotation','y')],
 33:[('ANIM_BARRIER_CARRIER_SPIN',(5.35,3.28,-1.46),'rotation','y'),('ANIM_BARRIER_CARRIER_LIFT',(5.35,3.90,-1.46),'translation','y')],
 35:[('ANIM_BARRIER_SLURRY_PUMP',(8.5,1.08,-3.06),'rotation','x')],
 37:[('ANIM_BARRIER_SLURRY_NOZZLE',(4.13,2.10,-.64),'translation','z')],
 38:[('ANIM_BARRIER_SPECTROMETER_HEAD',(6.25,2.64,-.64),'translation','y')],
 39:[('ANIM_BARRIER_OPTICAL_WINDOW',(6.08,2.04,-.64),'indicator','none')],
 40:[('ANIM_BARRIER_TOMOGRAPHY',(7.8,3.2,1.82),'indicator','none')],
 41:[('ANIM_SELECTIVITY_MANIFOLD',(8.05,1.4,-.05),'translation','x')],
 44:[('ANIM_CORROSION_POTENTIAL_PROBE',(7.78,2.35,3.10),'translation','y')],
 46:[('ANIM_BRUSH_CLEANER',(7.52,1.98,2.1),'rotation','y')],
 47:[('ANIM_MEGASONIC_CLEANING',(8.35,2.00,2.2),'indicator','none')],
 49:[('ANIM_RINSE_VALVE',(8.92,2.04,1.26),'rotation','y')],
 50:[('ANIM_SPIN_DRYER',(8.60,1.56,3.32),'rotation','y')],
 52:[('ANIM_DISHING_EROSION_SCANNER',(4.12,2.73,3.20),'translation','x')],
 53:[('ANIM_COPPER_RESIDUE_CAMERA',(5.07,2.68,3.20),'translation','x')],
 54:[('ANIM_INTERLINE_ISOLATION_PROBE',(6.10,2.57,3.18),'translation','y')],
 55:[('ANIM_VIA_KELVIN_PROBE',(7.09,2.55,3.16),'translation','y')],
 59:[('ANIM_ESTOP',(9.15,1.80,4.52),'translation','z')],
 60:[('ANIM_INTERCONNECT_SCADA',(0,5.22,-3.96),'indicator','none')]
}
assert sum(len(v) for v in RIGS.values())==44, sum(len(v) for v in RIGS.values())
for i, entries in RIGS.items():
 for n,p,kind,axis in entries:reg(i,n,p,kind,axis)

def pivot(i,j=0):return RIGS[i][j][0]

def model(i):
 k=names[i]
 if i==0:
  # polished three station base, 20.4m wide, with illuminated front floor inlays
  concrete(k,(0,.10,.12),(20.40,.44,9.00))
  box(k,(0,.44,-.38),(18.8,.36,6.10),'navy')
  box(k,(0,.69,-1.1),(17.8,.13,4.35),'steel')
  for s in 'ABC':
   x=SX[s];cc=accent(s)
   platform(k,(x,.80,SZ),(4.28,.23,4.42),cc)
   for bx in (x-1.95,x+1.95):
    for bz in (-3.36,.44):cyl(k,(bx,.65,bz),.10,.55,'silver','y',12)
   box(k,(x,.32,1.27),(4.03,.06,.05),cc)
  for i2 in range(16):
   x=-9.50+i2*1.26
   box(k,(x,.37,3.54),(.68,.032,.055),'amber' if i2<6 else 'cyan' if i2<11 else 'violet')
  for x in (-9.6,9.6):
   for z in (-3.85,3.85):box(k,(x,.59,z),(.24,.58,.24),'silver')
  for j in range(16):
   x=-9.48+j*1.25
   box(k,(x,.35,4.22),(.56,.019,.08),'light')
  for s in 'ABC':
   x=SX[s]
   for z in (-3.38,.42):
    box(k,(x,.61,z),(3.6,.10,.04),'light')
 elif i==1:
  for s in 'ABC':
   x=SX[s]
   frame(k,(x,2.63,SZ),(4.40,3.66,4.22),.09,True)
   box(k,(x,4.58,SZ),(4.39,.18,4.21),'navy')
   box(k,(x,4.50,.72),(3.78,.12,.08),accent(s))
   for t in (-1,1):
    box(k,(x+t*1.89,2.44,.62),(.08,2.75,.09),'silver')
    box(k,(x+t*1.90,2.50,.69),(.035,2.62,.02),accent(s))
   for j in range(7):
    box(k,(x-1.65+j*.54,4.75,.68),(.35,.085,.08),'ice')
  for t in range(6):
   x=-8.65+t*3.46
   box(k,(x,4.84,-3.45),(.11,.12,1.2),'steel')
 elif i==2:
  x,y,z=-8.8,1.22,1.89
  box(k,(x,y,z),(1.55,1.45,1.55),'steel')
  for n in range(8):
   yy=y-.60+n*.17
   box(k,(x,yy,z+.77),(1.32,.035,.08),'silver')
   cyl(k,(x,yy,z),.60,.032,'copper','y',24)
  for q in (-.70,.70):box(k,(x+q,y,z+.66),(.09,1.31,.12),'light')
  holo(k,(x,2.30,z+.77),1.18,.50,'amber')
 elif i==3:
  x=-8.15;z=2.2
  platform(k,(x,.75,z),(1.30,.24,1.45),'amber')
  with moving(pivot(i,0)):
   cyl(k,(x,1.2,z),.50,.55,'steel','y',32)
   ring(k,(x,1.48,z),.54,.46,.11,'y','silver')
   box(k,(x,1.69,z),(.42,.44,.40),'navy')
  with moving(pivot(i,1)):
   angular_arm(k,(x,1.82,z),(x+.45,2.14,z),.16,'steel')
   angular_arm(k,(x+.45,2.14,z),(x+.96,2.45,z),.13,'light')
   for j in range(4):cyl(k,(x+.20+j*.16,2.05+j*.09,z),.18,.10,'silver','z',12)
  with moving(pivot(i,2)):
   box(k,(x+1.12,2.48,z),(.46,.11,.35),'dark')
   for zoff in (-.12,.12):cyl(k,(x+1.23,2.41,z+zoff),.043,.11,'cyan','y',12)
   compact_wafer(k,(x+1.18,2.36,z),.34)
 elif i==4:
  detail_rack(k,(-8.10,1.55,3.3),(1.12,1.55,.68),'amber')
  cyl(k,(-8.05,2.48,2.88),.22,.24,'silver','y',20)
  for j in range(10):box(k,(-8.47+j*.09,2.02,3.68),(.054,.03,.02),'green' if j%2 else 'amber')
 elif i==5:
  compact_wafer(k,(-8.20,1.75,1.85),.68,'copper')
  for j in range(16):
   a=j*math.tau/16
   box(k,(-8.2+.52*math.cos(a),1.80,1.85+.52*math.sin(a)),(.08,.027,.08),'gold')
 elif i==6:
  x,y,z=0,4.55,3.35
  box(k,(x,y,z),(3.5,1.62,.13),'navy')
  box(k,(x,y,z+.085),(3.34,1.44,.05),'black')
  with moving(pivot(i)):
   for j in range(4):
    ox=x-1.11+j*.75
    box(k,(ox,y-.14,z+.123),(.60,.42,.09),'ceramic')
    box(k,(ox,y+.17,z+.135),(.60,.11,.08),'copper' if j<3 else 'cyan')
    for t in range(3):
     qx=ox-.18+t*.18
     box(k,(qx,y-.07,z+.17),(.10,.33 if j<3 else .25,.035),'copper' if j<3 else 'gold')
    if j<3:box(k,(ox,y+.27,z+.145),(.67,.08,.047),'amber')
  for j in range(12):box(k,(x-1.53+j*.28,y-.61,z+.15),(.16,.021,.012),'cyan' if j>5 else 'amber')
 elif i==7:
  holo(k,(-3.05,5.36,3.17),1.72,.86,'amber')
  for j in range(16):
   xx=-3.76+j*.094
   box(k,(xx,5.43-.16*math.sin(j*math.pi/15),3.25),(.065,.055,.02),'copper')
 elif i in (8,9,26,27,31,32):
  letter='A' if i in (8,9) else 'B' if i in (26,27) else 'C'
  with moving(pivot(i)):
   if i in (8,26,31):cmp_drum(k,letter,False)
   else:
    x,y,z=platencenter(letter)
    cyl(k,(x,1.72,z),1.41,.09,'pad','y',64)
    # three detailed, visually different pad patterns
    for j in range(58):
     a=j*math.tau/58
     for rr in (.34,.64,.94,1.22):
      xx=x+rr*math.cos(a);zz=z+rr*math.sin(a)
      if (i==9 and j%5) or (i==27 and j%3) or (i==32 and j%4):cyl(k,(xx,1.775,zz),.019 if i==32 else .028,.014,'padgroove','y',8)
    for j in range(12 if i==9 else 9 if i==27 else 18):
     a=j*math.tau/(12 if i==9 else 9 if i==27 else 18)
     pipe(k,[(x+.19*math.cos(a),1.782,z+.19*math.sin(a)),(x+1.33*math.cos(a),1.782,z+1.33*math.sin(a))],.011,'amber' if letter=='A' else 'cyan' if letter=='B' else 'violet',8)
 elif i==10:
  x=-5.35;z=SZ
  motor(k,(x,1.06,z-1.37),.75)
  cyl(k,(x,1.26,z),.26,.55,'steel','y',24)
  for j in range(6):box(k,(x-.47+j*.18,.84,z+1.07),(.10,.26,.20),'silver')
 elif i in (11,28,33):
  letter='A' if i==11 else 'B' if i==28 else 'C'
  with moving(pivot(i,0)):carrier(k,letter)
  if i==33:
   with moving(pivot(i,1)):
    x=SX[letter]
    box(k,(x,4.04,SZ),(.74,.58,.68),'steel')
    for j in (-.27,.27):cyl(k,(x+j,4.10,SZ),.12,.43,'silver','y',16)
 elif i==12:
  with moving(pivot(i)):
   x=SX['A']
   box(k,(x,4.05,SZ),(1.0,.57,.78),'steel')
   for j in (-.29,.29):cyl(k,(x+j,4.19,SZ),.11,.6,'silver','y',16)
   box(k,(x,4.55,SZ),(.86,.13,.66),'dark')
 elif i==13:
  with moving(pivot(i)):
   x=SX['A']
   ring(k,(x,3.02,SZ),1.07,.96,.09,'y','gold',40)
   for j in range(8):
    a=j*math.tau/8
    cyl(k,(x+.96*math.cos(a),3.14,SZ+.96*math.sin(a)),.055,.26,'silver','y',12)
 elif i==14:
  detail_rack(k,(-7.72,1.37,3.16),(1.20,1.30,.75),'amber')
  for j in range(6):
   xx=-8.12+j*.16
   pipe(k,[(xx,1.12,3.58),(xx,1.00,3.83),(xx-.1,1.00,4.0)],.021,'copper')
 elif i in (15,34):
  reservoir(k,(-8.92,1.35,-2.10) if i==15 else (8.90,1.33,-2.18),'amber' if i==15 else 'violet')
 elif i in (16,35):
  x=-8.40 if i==16 else 8.50;y=1.20 if i==16 else 1.08;z=-2.97 if i==16 else -3.06
  box(k,(x,.64,z),(1.37,.34,1.15),'steel')
  with moving(pivot(i)):
   motor(k,(x,y,z),.52)
   for q in (-.28,.28):ring(k,(x+q,y,z),.24,.19,.055,'x','light',20)
  fluidpipe(k,[(x-.67,y,z),(x-1.0,y+.37,z)],.047,'cyan' if i==16 else 'violet')
 elif i in (17,36):
  x=-7.55 if i==17 else 7.88
  multisensor(k,x,1.37,-3.30,'flow',.32)
  fluidpipe(k,[(x-.42,1.1,-3.6),(x+.47,1.1,-3.6)],.07,'amber' if i==17 else 'violet')
 elif i==18:
  x=-6.7;z=-.66
  pipe(k,[(x,2.40,z-1.1),(x,2.87,z-1.1),(x,2.87,z)],.10,'steel')
  for j in range(6):cyl(k,(x,2.27,z-1+j*.19),.05,.07,'amber','z',10)
 elif i in (19,30,37):
  x=-6.48 if i==19 else -1.12 if i==30 else 4.13
  z=-.72 if i==19 else -.65 if i==30 else -.64
  with moving(pivot(i)):
   cyl(k,(x,2.18,z),.11,.28,'silver','y',20)
   cyl(k,(x,1.96,z),.060,.22,'gold' if i==19 else 'cyan' if i==30 else 'violet','y',16)
   for j in range(7):
    a=j*math.tau/7
    cyl(k,(x+.05*math.cos(a),1.90,z+.05*math.sin(a)),.01,.065,'slurry','y',8)
 elif i==20:
  with moving(pivot(i)):
   pad_conditioner(k,'A')
 elif i==21:
  multisensor(k,-3.89,2.03,-.15,'wear',.25)
  pipe(k,[(-3.99,1.97,-.57),(-4.21,1.97,-.35),(-4.21,1.85,-.15)],.035,'silver')
 elif i in (22,23):
  x=-6.28 if i==22 else -5.67
  with moving(pivot(i)):
   multisensor(k,x,2.26,-.51,'optics',.29)
   cyl(k,(x,2.01,-.51),.08,.26,'silver','y',18)
   cyl(k,(x,1.81,-.51),.052,.16,'amber' if i==22 else 'cyan','y',16)
 elif i==24:
  detail_rack(k,(-6.45,1.31,3.40),(.98,1.19,.65),'amber')
 elif i==25:
  for x in (-9.15,9.15):
   box(k,(x,4.47,-1.46),(.22,.63,4.4),'steel')
  box(k,(0,4.82,-1.46),(18.4,.23,.58),'steel')
  for j in range(58):
   x=-8.80+j*.31
   box(k,(x,4.86,-1.12),(.045,.055,.07),'silver')
  with moving(pivot(i,0)):
   box(k,(0,4.69,-1.46),(1.4,.40,.79),'dark')
   for j in (-.55,.55):
    cyl(k,(j,4.69,-1.02),.11,.08,'silver','z',16)
  with moving(pivot(i,1)):
   box(k,(0,4.43,-1.46),(.55,.54,.53),'steel')
   for j in (-.21,.21):box(k,(j,4.48,-1.46),(.075,.44,.08),'silver')
  with moving(pivot(i,2)):
   box(k,(0,4.07,-1.46),(.30,.24,.44),'dark')
   for j in (-.16,.16):box(k,(j,3.90,-1.46),(.09,.20,.075),'light')
 elif i==29:
  with moving(pivot(i,0)):
   box(k,(0,4.08,-1.46),(.83,.58,.72),'steel')
   for j in (-.23,.23):cyl(k,(j,4.12,-1.46),.105,.58,'silver','y',16)
  with moving(pivot(i,1)):
   ring(k,(0,3.19,-1.46),1.05,.96,.085,'y','cyan',40)
   for j in range(6):
    a=j*math.tau/6
    cyl(k,(1.00*math.cos(a),3.30,-1.46+1.00*math.sin(a)),.048,.16,'silver','y',10)
 elif i==38:
  with moving(pivot(i)):
   detail_rack(k,(6.24,2.68,-.57),(.56,.72,.54),'violet')
   for dx in (-.14,.14):cyl(k,(6.24+dx,2.30,-.59),.10,.25,'silver','y',16)
   cyl(k,(6.24,2.12,-.58),.09,.13,'violet','y',24)
 elif i==39:
  with moving(pivot(i)):
   box(k,(6.08,2.04,-.64),(.46,.06,.45),'glass')
   ring(k,(6.08,2.08,-.64),.22,.15,.055,'y','silver',26)
 elif i==40:
  with moving(pivot(i)):
   holo(k,(7.76,3.18,1.86),1.06,.89,'violet')
   for q in range(5):box(k,(7.40+q*.16,3.00+(q%3)*.13,1.92),(.10,.06,.021),'amber')
 elif i==41:
  with moving(pivot(i)):
   box(k,(8.05,1.4,-.05),(1.06,.39,.58),'steel')
   for j in range(4):pulsevalve(k,(7.74+j*.20,1.68,-.05),'violet')
   for j in range(4):fluidpipe(k,[(7.74+j*.20,1.37,.10),(7.74+j*.20,1.18,.46)],.025,'violet')
 elif i==42:
  detail_rack(k,(5.12,1.25,2.87),(.85,.99,.57),'violet')
  for j in range(6):cyl(k,(5.0+j*.08,1.85,2.86),.038,.06,'silver','y',12)
 elif i==43:
  multisensor(k,6.15,2.05,1.48,'thermal',.24)
  pipe(k,[(6.13,1.80,1.24),(5.95,1.78,1.04)],.025,'copper')
 elif i==44:
  with moving(pivot(i)):
   multisensor(k,7.78,2.32,3.10,'potential',.24)
   cyl(k,(7.77,2.06,3.08),.04,.29,'silver','y',12)
 elif i==45:
  reservoir(k,(8.63,1.37,1.00),'cyan')
  for j in range(4):box(k,(8.4+j*.12,2.18,1.5),(.065,.035,.09),'green')
 elif i==46:
  frame(k,(7.50,1.94,2.10),(1.58,1.48,1.28),.065,True)
  with moving(pivot(i)):
   for j in (-.30,.30):
    cyl(k,(7.52+j,1.99,2.1),.21,.70,'light','y',24)
    for a in range(32):
     q=a*math.tau/32
     cyl(k,(7.52+j+.21*math.cos(q),1.99,2.1+.21*math.sin(q)),.018,.66,'ice','y',8)
  box(k,(7.52,1.27,2.10),(1.33,.12,1.13),'dark')
 elif i==47:
  with moving(pivot(i)):
   frame(k,(8.30,2.03,2.18),(1.22,1.20,1.18),.06,True)
   for j in range(8):
    xx=7.91+j*.11
    cyl(k,(xx,1.85,2.18),.035,.22,'cyan','y',10)
 elif i==48:
  multisensor(k,7.58,1.43,3.86,'residue',.29)
 elif i==49:
  reservoir(k,(9.05,1.32,1.25),'ice')
  with moving(pivot(i)):
   pulsevalve(k,(8.92,2.04,1.26),'cyan')
 elif i==50:
  frame(k,(8.60,1.78,3.30),(1.48,1.52,1.54),.055,True)
  with moving(pivot(i)):
   cyl(k,(8.60,1.55,3.30),.55,.10,'dark','y',32)
   compact_wafer(k,(8.60,1.63,3.30),.47,'iridescent')
  for j in range(5):
   a=j*math.tau/5
   cyl(k,(8.6+.45*math.cos(a),1.97,3.3+.45*math.sin(a)),.055,.16,'cyan','y',10)
 elif i==51:
  multisensor(k,3.48,1.35,3.56,'defect',.30)
  box(k,(3.47,2.11,3.52),(.94,.12,.35),'dark')
 elif i in (52,53,54,55):
  x={52:4.12,53:5.07,54:6.10,55:7.09}[i]
  with moving(pivot(i)):
   box(k,(x,2.50,3.20),(.56,.32,.48),'steel')
   compact_screen(k,(x,2.54,3.45),.40,.17,'cyan' if i in (52,55) else 'violet')
   for j in (-.14,.14):
    cyl(k,(x+j,2.22,3.20),.025,.36,'silver','y',10)
    cyl(k,(x+j,2.01,3.20),.019,.13,'gold','y',10)
 elif i==56:
  detail_rack(k,(7.38,1.22,4.10),(.96,1.06,.64),'cyan')
 elif i==57:
  detail_rack(k,(-1.55,1.42,3.44),(1.12,1.48,.71),'violet')
 elif i==58:
  detail_rack(k,(1.50,1.42,3.46),(1.20,1.58,.75),'cyan')
  for j in range(11):box(k,(1.03+j*.09,1.01,3.91),(.049,.035,.014),'green' if j%3 else 'amber')
 elif i==59:
  detail_rack(k,(9.17,1.31,4.10),(.86,1.07,.62),'red')
  box(k,(9.16,1.80,4.45),(.44,.32,.05),'red')
  with moving(pivot(i)):
   cyl(k,(9.15,1.80,4.52),.12,.06,'warning','z',24)
 elif i==60:
  with moving(pivot(i)):
   giantdisplay(k,(0,5.41,-3.96),6.68,2.13)
   for j in range(15):
    xx=-2.82+j*.4
    box(k,(xx,5.91-(j%4)*.25,-3.85),(.15,.045,.02),'green' if j%3 else 'amber')
  box(k,(0,4.21,-4.02),(6.92,.18,.46),'steel')
  for j in range(3):
   x=-2.18+j*2.18
   holo(k,(x,3.45,-3.94),1.8,.65,accent('ABC'[j]))
 elif i==61:
  detail_rack(k,(-9.16,1.33,4.03),(.87,1.06,.65),'green')
  for j in range(6):box(k,(-9.47+j*.12,1.82,4.36),(.055,.036,.04),'cyan')
 else:raise RuntimeError((i,k))

for i in range(len(names)):
 model(i)
 assert names[i] in M and len(M[names[i]])>0,(i,names[i])
for k in names:
 grps={g for m,c,g in M[k] if g is not None}
 assert grps==set(PV.get(k,{})),(k,grps,set(PV.get(k,{})))
print('MODELED',len(names),'models;',sum(len(M[n]) for n in names),'parts;',sum(sum(len(m.faces) for m,c,p in M[n]) for n in names),'tris;',sum(map(len,PV.values())),'pivots',flush=True)
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
            objects.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(.25*q for q in rgb)),N('P','SpecularColor','Color','','A',.60,.62,.67),N('P','Shininess','Number','','A',54.),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
            mats[color]=matid
        connections.append(N('C','OO',mats[color],model_id))
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 77 Triple-Station Copper CMP Procedural Modular Geometry')]),
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


# Independently export individual modules from the common-origin geometry.
manifest=[]
for i,key in enumerate(names):
    filename=f'{i:02d}_L77_{key}'
    export_fbx(key,MD/(filename+'.fbx'))
    export_glb(key,GD/(filename+'.glb'))
    triangles=sum(len(m.faces) for m,_,_ in M[key]);parts=len(M[key])
    coords=np.concatenate([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L77_'+key,file=filename+'.fbx',glb=filename+'.glb',
         triangle_count=triangles,mesh_parts=parts,
         bbox_min=np.around(coords.min(axis=0),4).tolist(),bbox_max=np.around(coords.max(axis=0),4).tolist(),
         rig_groups=list(PV.get(key,{}).keys()),
         animation_pivots={g:list(info['position']) for g,info in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/62] {key:42s} {triangles:7,d} triangles {parts:4d} parts',flush=True)

assembled='Level77_Complete_ArtKit_Showcase'
M[assembled]=[];PV[assembled]={}
for key in names:
    for g,info in PV.get(key,{}).items():PV[assembled][key+'__'+g]=info
    for mesh,material,g in M[key]:M[assembled].append((mesh,material,key+'__'+g if g else None))
print('Exporting assembled workstation...',flush=True)
export_fbx(assembled,MD/(assembled+'.fbx'))
export_glb(assembled,GD/(assembled+'.glb'))

with (ROOT/'Animation_Rigs.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','Pivot','X','Y','Z','Kind','Axis','FBXHierarchy','Notes'])
    for k in names:
        for pivot,info in PV.get(k,{}).items():
            w.writerow(['L77_'+k,pivot,*info['position'],info['kind'],info['axis'],'Named pivot parent Null nodes in FBX and GLB','No baked animation; follow actual simulation'])
with (ROOT/'Art_Assembly.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','FBX','PositionX','PositionY','PositionZ','RotationX','RotationY','RotationZ','Scale','Notes'])
    for m in manifest:w.writerow([m['name'],m['file'],0,0,0,0,0,0,1,'Common world origin, confirm FBX importer centimeter conversion'])
with (ROOT/'Component_Roles.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Index','Model','FBX','Triangles','MeshParts','PivotGroups'])
    for m in manifest:w.writerow([m['index'],m['name'],m['file'],m['triangle_count'],m['mesh_parts'],';'.join(m['rig_groups'])])

palette={}
for name,rgba in P.items():
    palette['PS_'+name]={'rgba_255':list(rgba),'metallic':.84 if name in ('steel','light','silver','copper','gold') else .08,
        'smoothness':.86 if name in ('steel','silver','glass','copper','gold','iridescent') else .54,
        'transparent':rgba[3]<255,'suggested_emissive':name in ('cyan','blue','green','amber','violet','ice','red','gold','teal')}
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_77',
 'description':'Procedural colored material regions; NOT production hand-painted PBR textures','materials':palette},indent=2))

