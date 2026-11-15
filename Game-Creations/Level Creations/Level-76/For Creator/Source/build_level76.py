#!/usr/bin/env python3
"""Project Spark Level 76: original modular copper interconnect electroplating 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical/implantation gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level76_HD_AAA')
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




# LEVEL 76 — FORGE THE CONNECTIONS: modular copper electrochemical deposition workstation
names=[
'IndustrialCopperInterconnectBase','TransparentElectroplatingSafetyEnclosure','PrecisionCopperPlatingCell','TransparentElectrolyteBasin','PatternedDielectricTrenchWafer','ConductiveBarrierSeedLayer','HolographicTrenchViaCrossSection','PrecisionWaferCathodeChuck','CathodeRotarySpindle','CathodeVerticalLiftAssembly','AutomatedWaferTransferRobot','InterconnectWaferCassette','PrewetWaferPreparationChamber','ElectrolyteDegassingModule','VirtualCopperElectrolyteReservoir','ElectrolyteMixingVessel','MicroFiltrationSkid','PrecisionRecirculationPump','ElectrolyteMassFlowController','AnnularFlowDistributionManifold','MultiZoneElectrolyteNozzleArray','SegmentedCopperAnodeAssembly','ProtectedAnodeBasket','AnodeSeparatorMembrane','EdgeCurrentDistributionRing','PulsedCurrentPlatingRectifier','CathodeCurrentShunt','PrecisionCurrentTransducer','PlatingCellVoltageSensor','ReferenceElectrodeProbe','ElectrochemicalPotentialController','MultiZoneCurrentDistributionController','BathConductivitySensor','ElectrolyteTemperatureSensor','ElectrolyteThermalConditioner','VirtualBathConcentrationSensor','VirtualSuppressorCartridge','VirtualAcceleratorCartridge','VirtualLevelerCartridge','AdditiveMeteringManifold','ElectrolyteBubbleDetectionSensor','ElectrolyteLinePressureSensor','WaferCathodeContactResistanceMonitor','FourPointInterconnectTestProbe','OpticalCopperThicknessScanner','ThreeDVoidInspectionScanner','TrenchFillTomographyMonitor','CopperOverburdenAnalyzer','CopperSurfaceRoughnessScanner','PostPlatingWaferRinseChamber','WaferSpinDryer','WasteElectrolyteCollectionModule','ElectrolyteLeakDetectionRelay','ElectroplatingEmergencySafetyStation','CopperElectroplatingPLCRack','IndustrialAnalogIOModule','PlatingWaveformProcessController','HolographicInterconnectSCADA','Regulated5VControlSupply','SeedBarrierContinuityScanner']
assert len(names)==60
PV={}
def reg(i,n,p,kind='rotation',axis='y'): PV.setdefault(names[i],{})[n]=dict(position=tuple(p),kind=kind,axis=axis)
def axis_ring(k,c,r,inner=None,dep=.08,col='silver',axis='y',segments=48):ring(k,c,r,inner if inner is not None else r*.9,dep,axis,col,segments)
def pipesquare(k,center,radius=1.2,height=2.1,segments=12,col='steel'):
 x,y,z=center
 for i in range(segments):
  a=2*math.pi*i/segments
  pipe(k,[(x+radius*math.cos(a),y-height*.47,z+radius*math.sin(a)),(x+radius*math.cos(a),y+height*.47,z+radius*math.sin(a))],.035,col,9)
 for hy in (-.45,.45):torus_ring_segments(k,(x,y+hy*height,z),radius,.045,'silver','y',segments*4)
def bolts(k,center,r,n=18,yoffset=.0):
 x,y,z=center
 for i in range(n):
  a=2*math.pi*i/n;cyl(k,(x+r*math.cos(a),y+yoffset,z+r*math.sin(a)),.036,.05,'dark','y',10)
def fluidpipe(k,pts,r=.11,col='cyan',flange=True):
 pipe(k,pts,r,col,12)
 if flange:
  for p in pts[1:-1]:cyl(k,p,r*1.45,.065,'silver','y',16)
def vessel(k,c,r=.59,h=1.4,fluid='teal',accent='cyan'):
 x,y,z=c
 cyl(k,c,r,h,'glass','y',40)
 cyl(k,(x,y-h*.13,z),r*.86,h*.57,fluid,'y',32)
 for dy in (-h*.50,h*.50):axis_ring(k,(x,y+dy,z),r*1.08,r*.91,.10,'steel')
 for a in range(10):
  t=a*math.tau/10
  cyl(k,(x+r*1.015*math.cos(t),y+h*.51,z+r*1.015*math.sin(t)),.035,.12,'silver','y',10)
 cyl(k,(x,y+h*.59,z),.15,.12,'steel','y')
 box(k,(x,y,z+r+.02),(r*.8,h*.27,.045),'dark')
 compact_screen(k,(x,y,z+r+.055),r*.60,h*.17,accent)
def plating_cell(k,c=(0,2.35,-.70),r=1.77,h=3.20,include_liquid=False):
 x,y,z=c
 cyl(k,c,r,h,'glass','y',64)
 for dy in (-h*.50,-h*.26,h*.25,h*.50):axis_ring(k,(x,y+dy,z),r*1.06,r*.97,.13,'silver',segments=64)
 pipesquare(k,c,r+.12,h,segments=16,col='steel')
 for dy in (-h*.51,h*.50):bolts(k,(x,y+dy,z),r*1.02,32)
 for a in range(12):
  t=a*math.tau/12
  xx=x+(r+.12)*math.cos(t);zz=z+(r+.12)*math.sin(t)
  pipe(k,[(xx,y+.95,zz),(xx,y+1.35,zz)],.075,'copper',10)
 if include_liquid:
  cyl(k,(x,y-.42,z),r*.95,h*.58,'slurry','y',48)
  for j in range(14):
   a=j*2.39;rr=.25+(j%5)*.24
   cyl(k,(x+rr*math.cos(a),y-1.13+(j%6)*.22,z+rr*math.sin(a)),.027,.11,'ice','y',10)
def micron(k,c,scale=1,fill=.6,accent='copper',pattern=True):
 x,y,z=c
 box(k,(x,y,z),(1.2*scale,.13*scale,.9*scale),'ceramic')
 for j in range(4):
  xx=x+(-.37+j*.24)*scale
  box(k,(xx,y+.115*scale,z),(.075*scale,.16*scale,.69*scale),'silver')
  if fill>0:box(k,(xx,y+.077*scale+(fill*.12*scale)*.5,z),(.063*scale,.08*scale+fill*.13*scale,.63*scale),accent)
 if pattern:
  for n in range(3):box(k,(x,y+.18*scale,z+(-.26+n*.24)*scale),(.89*scale,.021*scale,.017*scale),'cyan')
def wafer(k,c,r=.85,material='iridescent',patterns=True):
 x,y,z=c
 cyl(k,c,r,.040,material,'y',56)
 axis_ring(k,(x,y+.032,z),r*1.02,r*.96,.025,'silver',segments=56)
 if patterns:
  for i in range(-4,5):
   for j in range(-4,5):
    px=x+i*.17; pz=z+j*.17
    if (px-x)**2+(pz-z)**2 <(r*.86)**2:
     box(k,(px,y+.028,pz),(.125,.009,.12),'copper' if (i+j)%2 else 'gold')
     for t in range(3):box(k,(px-.04+t*.04,y+.038,pz),(.013,.006,.072),'dark')
def stream(k,x0,z0,x1,z1,y=2.4,col='cyan',r=.03):
 fluidpipe(k,[(x0,y,z0),(x0,y+.5,z0),(x1,y+.5,z1),(x1,y,z1)],r,col,False)
def detail_cabinet(k,pos,accent='cyan',w=1.12,h=1.56,d=.82):
 hd_cabinet(k,pos,(w,h,d),accent,2)
 for i in range(10):box(k,(pos[0]-w*.40+i*w*.09,pos[1]-h*.05,pos[2]+d*.53),(.052,.025,.025),'cyan' if i%2 else 'green')
 for i in range(5):box(k,(pos[0]-w*.33+i*w*.16,pos[1]+h*.52,pos[2]),(.073,.08,.11),'steel')

def model(i):
 k=names[i]
 if i==0:
  box(k,(0,.15,0),(19.6,.37,10.1),'navy')
  for xx in np.linspace(-9.55,9.55,44):box(k,(xx,.357,0),(.025,.035,9.5),'steel')
  for zz in np.linspace(-4.9,4.9,27):box(k,(0,.375,zz),(19.1,.026,.018),'dark')
  for xx in np.linspace(-8.8,8.8,18):
   box(k,(xx,.52,4.75),(.60,.35,.32),'steel');box(k,(xx,.72,4.74),(.42,.02,.08),'amber')
  for xx in (-9.25,9.25):
   for zz in (-4.3,4.25):box(k,(xx,.45,zz),(.45,.48,.45),'dark')
  for xx in (-8.8,-4.8,-1.9,2.0,4.8,7.8):
   box(k,(xx,.75,-3.85),(.19,.76,.16),'steel')
  for xx in (-8.6,-7.1,-5.6,-4.2,4.6,6.4,8.2):
   for zz in (-3.8,3.7):box(k,(xx,.47,zz),(.7,.09,.62),'light')
  for xx in (-3.7,3.7):pipe(k,[(xx,.55,-2.6),(xx,.55,1.9)],.12,'copper',14)
  for j in range(50):
   xx=-9.1+j*.37;box(k,(xx,.388,4.88),(.12,.008,.07),'amber' if j%2 else 'black')
 elif i==1:
  frame(k,(0,2.6,-.8),(5.0,4.20,5.05),.13,True)
  for xx in (-2.41,2.41):
   box(k,(xx,1.0,-.8),(.07,.95,4.68),'steel')
   for yy in (1.8,3.1,4.4):lightstrip(k,(xx,yy,-.80),(.038,.025,4.5),'cyan')
  box(k,(0,4.63,-.8),(4.6,.16,4.6),'dark')
 elif i==2:
  plating_cell(k)
  reg(i,'ANIM_PLATING_CHAMBER_LID',(0,4.03,-.7),'translation','y')
  with moving('ANIM_PLATING_CHAMBER_LID'):
   cyl(k,(0,4.02,-.70),1.85,.22,'dark','y',56)
   axis_ring(k,(0,4.17,-.7),1.84,1.68,.09,'silver',segments=56)
   for j in range(22):
    a=j*math.tau/22;cyl(k,(1.7*math.cos(a),4.21,-.7+1.7*math.sin(a)),.045,.07,'copper','y')
 elif i==3:
  cyl(k,(0,1.69,-.7),1.67,1.32,'slurry','y',56)
  reg(i,'ANIM_ELECTROLYTE_SURFACE',(0,2.31,-.7),'visual','y')
  with moving('ANIM_ELECTROLYTE_SURFACE'):
   cyl(k,(0,2.35,-.7),1.64,.035,'ice','y',56)
   for j in range(9):
    axis_ring(k,(0,2.38,-.7),.25+j*.16,.25+j*.16-.012,.005,'teal',segments=64)
 elif i==4:
  wafer(k,(0,3.14,-.7),.95,'ceramic',True)
  for ix in range(-3,4):
   for iz in range(-3,4):
    xx=ix*.205;zz=-.7+iz*.205
    if xx*xx+(zz+.7)**2<.65:
     for q in range(3):box(k,(xx-.045+q*.035,3.164,zz),(.020,.010,.12),'black')
 elif i==5:
  wafer(k,(0,3.185,-.7),.94,'copper',False)
  for j in range(48):
   a=j*math.tau/48;box(k,(.87*math.cos(a),3.22,-.7+.87*math.sin(a)),(.07,.018,.05),'gold')
 elif i==6:
  frame(k,(0,4.95,2.70),(3.80,1.48,.23),.08,False)
  reg(i,'ANIM_TRENCH_FILL_HOLOGRAM',(0,4.9,2.81),'visual','y')
  with moving('ANIM_TRENCH_FILL_HOLOGRAM'):
   for j in range(6):micron(k,(-1.36+j*.55,4.9,2.89),.40,fill=j/5.)
   for j in range(24):box(k,(-1.62+j*.14,5.40,2.94),(.08,.012,.015),'cyan')
 elif i==7:
  cyl(k,(0,3.40,-.7),1.03,.21,'steel','y',50)
  axis_ring(k,(0,3.32,-.7),1.12,1.01,.16,'copper',segments=64)
  reg(i,'ANIM_WAFER_CLAMP',(0,3.42,-.7),'rotation','y')
  with moving('ANIM_WAFER_CLAMP'):
   for j in range(14):
    a=j*math.tau/14
    box(k,(1.075*math.cos(a),3.35,-.7+1.075*math.sin(a)),(.11,.06,.13),'light')
 elif i==8:
  cyl(k,(0,3.83,-.7),.39,1.25,'silver','y',40)
  reg(i,'ANIM_WAFER_CATHODE_ROTATION',(0,3.38,-.7),'rotation','y')
  with moving('ANIM_WAFER_CATHODE_ROTATION'):
   cyl(k,(0,3.36,-.7),1.06,.15,'dark','y',40)
   for j in range(12):
    a=j*math.tau/12
    pipe(k,[(.55*math.cos(a),3.4,-.7+.55*math.sin(a)),(.99*math.cos(a),3.4,-.7+.99*math.sin(a))],.035,'copper',10)
 elif i==9:
  box(k,(0,4.65,-.7),(1.45,.66,1.28),'steel')
  reg(i,'ANIM_WAFER_CATHODE_Z',(0,4.3,-.7),'translation','y')
  with moving('ANIM_WAFER_CATHODE_Z'):
   cyl(k,(0,4.02,-.7),.25,.78,'silver','y',32)
   for j in range(4):ring(k,(0,3.73+j*.18,-.7),.32,.26,.053,'y','dark',24)
 elif i==10:
  cyl(k,(4.5,.72,-.5),.68,.28,'dark','y',34)
  reg(i,'ANIM_WAFER_ROBOT_BASE',(4.5,1.05,-.5),'rotation','y')
  with moving('ANIM_WAFER_ROBOT_BASE'):
   cyl(k,(4.5,1.19,-.5),.33,.52,'silver','y',30)
   reg(i,'ANIM_WAFER_ROBOT_ARM',(4.5,1.58,-.5),'rotation','z')
   with moving('ANIM_WAFER_ROBOT_ARM'):
    angular_arm(k,(4.5,1.59,-.5),(5.65,1.96,-.5),.16,'light')
    reg(i,'ANIM_WAFER_ROBOT_TOOL',(5.8,1.99,-.5),'rotation','y')
    with moving('ANIM_WAFER_ROBOT_TOOL'):
     box(k,(5.94,1.97,-.5),(.45,.10,.18),'silver')
     for q in (-.11,.11):box(k,(6.2,1.97,-.5+q),(.24,.055,.06),'steel')
 elif i==11:
  box(k,(4.87,1.12,-2.77),(1.05,1.25,.88),'dark')
  for j in range(15):wafer(k,(4.87,1.0+j*.054,-2.77),.39,'iridescent',False)
  frame(k,(4.87,1.55,-2.77),(1.22,2.23,1.08),.07,True)
 elif i==12:
  frame(k,(-3.05,1.6,-2.20),(1.45,1.6,1.25),.10,True)
  reg(i,'ANIM_PREWET_CHAMBER',(-3.05,1.90,-2.2),'translation','y')
  with moving('ANIM_PREWET_CHAMBER'):
   wafer(k,(-3.05,1.67,-2.2),.45,'iridescent',False)
   for j in range(8):
    t=j*math.tau/8;fluidpipe(k,[(-3.05+.7*math.cos(t),2.28,-2.2+.7*math.sin(t)),(-3.05+.5*math.cos(t),1.92,-2.2+.5*math.sin(t))],.026,'cyan',False)
 elif i==13:
  vessel(k,(-4.18,1.52,-2.7),.54,1.45)
  reg(i,'ANIM_DEGASSING_MODULE',(-4.18,1.54,-2.7),'visual','y')
  with moving('ANIM_DEGASSING_MODULE'):
   for j in range(12):
    a=j*2.32;rr=.13+(j%4)*.095
    cyl(k,(-4.18+rr*math.cos(a),1.1+(j%5)*.19,-2.7+rr*math.sin(a)),.026,.07,'white','y',10)
 elif i==14:
  vessel(k,(-7.93,1.60,-1.92),.82,2.10)
  fluidpipe(k,[(-7.93,2.6,-1.92),(-7.93,2.93,-1.92),(-6.92,2.93,-1.92)],.09,'cyan')
 elif i==15:
  vessel(k,(-6.57,1.46,-2.62),.74,1.8)
  reg(i,'ANIM_ELECTROLYTE_MIXER',(-6.57,1.86,-2.62),'rotation','y')
  with moving('ANIM_ELECTROLYTE_MIXER'):
   cyl(k,(-6.57,1.5,-2.62),.085,1.05,'silver','y',16)
   for h in (1.1,1.52):
    for j in range(5):
     a=j*math.tau/5;box(k,(-6.57+.33*math.cos(a),h,-2.62+.33*math.sin(a)),(.23,.037,.14),'silver')
 elif i==16:
  box(k,(-5.54,1.38,-3.68),(1.2,1.52,.94),'dark')
  for j in range(3):
   box(k,(-5.97+j*.42,1.5,-3.14),(.34,.80,.06),'white')
   for l in range(17):box(k,(-5.97+j*.42,1.2+l*.038,-3.09),(.26,.012,.023),'light')
  reg(i,'ANIM_FILTRATION_PUMP',(-5.5,1.2,-3.7),'rotation','x')
  with moving('ANIM_FILTRATION_PUMP'):fan(k,(-5.5,1.2,-3.1),.31,'z')
 elif i==17:
  motor(k,(-5.51,.96,-1.55),.85)
  reg(i,'ANIM_RECIRCULATION_PUMP',(-5.32,.97,-1.55),'rotation','x')
  with moving('ANIM_RECIRCULATION_PUMP'):
   cyl(k,(-4.75,.97,-1.55),.43,.36,'copper','x',24)
   for j in range(9):
    a=j*math.tau/9
    pipe(k,[(-4.82,.97,-1.55),(-4.82,.97+.32*math.cos(a),-1.55+.32*math.sin(a))],.034,'silver',9)
 elif i==18:
  sensor(k,(-3.67,1.2,-3.5),'cyan')
  fluidpipe(k,[(-4.5,1.02,-3.5),(-3.4,1.02,-3.5)],.13,'copper')
  reg(i,'ANIM_FLOW_CONTROLLER',(-3.67,1.20,-3.5),'visual','y')
  with moving('ANIM_FLOW_CONTROLLER'):cyl(k,(-3.67,1.62,-3.5),.09,.18,'cyan','y',14)
 elif i==19:
  for j in range(4):
   rr=1.12+j*.15
   axis_ring(k,(0,1.21+j*.025,-.7),rr,rr-.07,.10,'copper',segments=48)
  for j in range(12):
   a=j*math.tau/12
   fluidpipe(k,[(1.8*math.cos(a),1.4,-.7+1.8*math.sin(a)),(1.5*math.cos(a),1.4,-.7+1.5*math.sin(a))],.045,'cyan',False)
  reg(i,'ANIM_DISTRIBUTION_MANIFOLD',(0,1.39,-.7),'visual','y')
  with moving('ANIM_DISTRIBUTION_MANIFOLD'):axis_ring(k,(0,1.39,-.7),1.77,1.70,.045,'cyan',segments=52)
 elif i==20:
  for j in range(28):
   a=j*math.tau/28;rr=1.5
   cyl(k,(rr*math.cos(a),1.67,-.7+rr*math.sin(a)),.09,.24,'steel','y',14)
   cyl(k,(rr*math.cos(a),1.82,-.7+rr*math.sin(a)),.035,.13,'cyan','y',10)
  reg(i,'ANIM_FLOW_NOZZLE_ARRAY',(0,1.85,-.7),'visual','y')
  with moving('ANIM_FLOW_NOZZLE_ARRAY'):
   for j in range(15):
    a=j*math.tau/15
    pipe(k,[(1.41*math.cos(a),1.90,-.7+1.41*math.sin(a)),(1.21*math.cos(a),2.13,-.7+1.21*math.sin(a))],.020,'ice',8)
 elif i==21:
  cyl(k,(0,1.42,-.7),1.39,.24,'copper','y',56)
  for j in range(20):
   a=j*math.tau/20; rr=.76 if j%2 else 1.08
   cyl(k,(rr*math.cos(a),1.59,-.7+rr*math.sin(a)),.17,.09,'gold','y',16)
  reg(i,'ANIM_ANODE_SEGMENT_A',(0,1.58,-.7),'visual','y')
  with moving('ANIM_ANODE_SEGMENT_A'):axis_ring(k,(0,1.60,-.7),1.18,1.08,.07,'gold',segments=48)
  reg(i,'ANIM_ANODE_SEGMENT_B',(0,1.59,-.7),'visual','y')
  with moving('ANIM_ANODE_SEGMENT_B'):axis_ring(k,(0,1.62,-.7),.8,.69,.07,'copper',segments=48)
 elif i==22:
  axis_ring(k,(0,1.20,-.7),1.51,1.35,.11,'silver',segments=48)
  reg(i,'ANIM_ANODE_SHIELD',(0,1.35,-.7),'rotation','y')
  with moving('ANIM_ANODE_SHIELD'):
   for j in range(16):
    a=j*math.tau/16;box(k,(1.48*math.cos(a),1.36,-.7+1.48*math.sin(a)),(.14,.42,.12),'dark')
 elif i==23:
  cyl(k,(0,1.70,-.7),1.52,.025,'purpleglass','y',56)
  axis_ring(k,(0,1.69,-.7),1.56,1.52,.035,'silver',segments=56)
 elif i==24:
  axis_ring(k,(0,2.66,-.7),1.49,1.30,.12,'copper',segments=56)
  reg(i,'ANIM_CURRENT_DISTRIBUTION_RING',(0,2.66,-.7),'rotation','y')
  with moving('ANIM_CURRENT_DISTRIBUTION_RING'):
   for j in range(24):
    a=j*math.tau/24
    box(k,(1.37*math.cos(a),2.70,-.7+1.37*math.sin(a)),(.075,.06,.14),'gold')
 elif i==25:
  detail_cabinet(k,(-3.95,1.37,3.52),'amber',1.45,1.62,.78)
  reg(i,'ANIM_RECTIFIER_COOLING_FAN',(-3.98,1.33,4.02),'rotation','z')
  with moving('ANIM_RECTIFIER_COOLING_FAN'):fan(k,(-3.98,1.33,4.01),.26,'z')
  terminal_block(k,(-3.95,.70,4.03),11,accent='copper')
 elif i==26:
  sensor(k,(-2.63,1.06,3.53),'amber')
  for j in range(8):box(k,(-2.95+j*.09,1.50,3.7),(.06,.035,.19),'copper')
 elif i==27:
  sensor(k,(-1.72,1.08,3.5),'cyan')
  for j in range(6):pipe(k,[(-2.12+j*.15,.72,3.3),(-2.12+j*.15,.72,3.75)],.025,'copper',9)
 elif i==28:
  sensor(k,(-.78,1.02,3.55),'amber')
  gauge(k,(-.78,1.45,3.95),.16)
 elif i==29:
  sensor(k,(1.12,1.31,1.56),'green')
  reg(i,'ANIM_REFERENCE_ELECTRODE',(1.08,1.98,1.53),'translation','y')
  with moving('ANIM_REFERENCE_ELECTRODE'):
   cyl(k,(1.08,2.22,1.53),.08,.72,'silver','y')
   cyl(k,(1.08,2.58,1.53),.05,.20,'gold','y')
   fluidpipe(k,[(1.08,1.98,1.53),(1.08,1.76,1.0)],.022,'copper',False)
 elif i==30:
  detail_cabinet(k,(.25,1.34,3.6),'cyan')
  reg(i,'ANIM_POTENTIAL_CONTROLLER_DISPLAY',(.25,1.57,4.04),'visual','z')
  with moving('ANIM_POTENTIAL_CONTROLLER_DISPLAY'):lightstrip(k,(.25,1.56,4.045),(.5,.025,.02),'green')
 elif i==31:
  detail_cabinet(k,(1.55,1.35,3.58),'amber')
  for j in range(6):terminal_block(k,(1.25+j*.11,.72,4.01),2,accent='copper')
 elif i in (32,33,35,40,41,42):
  coords={32:(-5.28,1.48,1.47),33:(-5.97,1.48,1.47),35:(-6.75,1.48,1.47),40:(-3.10,1.25,.82),41:(-4.48,1.22,.82),42:(2.54,1.24,3.55)}
  sensor(k,coords[i],'cyan' if i%2 else 'amber')
  if i==40:
   reg(i,'ANIM_BUBBLE_SENSOR',coords[i],'visual','y')
   with moving('ANIM_BUBBLE_SENSOR'):cyl(k,(coords[i][0],coords[i][1]+.44,coords[i][2]),.10,.14,'ice','y',12)
 elif i==34:
  detail_cabinet(k,(-8.05,1.32,.64),'cyan',1.32,1.5,.79)
  reg(i,'ANIM_BATH_THERMAL_FAN',(-8.05,1.24,1.08),'rotation','z')
  with moving('ANIM_BATH_THERMAL_FAN'):fan(k,(-8.05,1.25,1.10),.25,'z')
  coil(k,-8.05,1.74,.55,.85,.35,True)
 elif i in (36,37,38):
  x={36:-8.46,37:-7.41,38:-6.36}[i];co={36:'blue',37:'violet',38:'amber'}[i]
  vessel(k,(x,2.02,1.88),.31,.98,'slurry',co)
  reg(i,{36:'ANIM_SUPPRESSOR_METERING',37:'ANIM_ACCELERATOR_METERING',38:'ANIM_LEVELER_METERING'}[i],(x,2.65,1.88),'translation','y')
  with moving({36:'ANIM_SUPPRESSOR_METERING',37:'ANIM_ACCELERATOR_METERING',38:'ANIM_LEVELER_METERING'}[i]):
   cyl(k,(x,2.65,1.88),.10,.26,co,'y',16)
 elif i==39:
  box(k,(-6.95,1.23,3.17),(2.48,.47,.49),'steel')
  for j in range(3):
   cx=-7.75+j*.77
   cyl(k,(cx,1.65,3.17),.16,.39,'dark','y',20)
   cyl(k,(cx,1.91,3.17),.10,.11,'copper','y')
  terminal_block(k,(-6.97,.94,3.49),12,accent='cyan')
 elif i==43:
  box(k,(5.0,1.62,2.65),(.92,.23,.76),'silver')
  reg(i,'ANIM_FOUR_POINT_PROBE',(5.0,2.06,2.65),'translation','y')
  with moving('ANIM_FOUR_POINT_PROBE'):
   for j in range(4):cyl(k,(4.73+j*.18,1.85,2.65),.037,.36,'gold','y',10)
   box(k,(5.0,2.24,2.65),(.80,.15,.52),'steel')
 elif i in (44,45,46,47,48,59):
  loc={44:(5.25,2.24,3.3),45:(6.65,2.21,3.3),46:(7.95,2.21,3.3),47:(5.5,1.22,-3.4),48:(7.38,1.24,-3.4),59:(4.17,2.38,2.92)}[i]
  sensor(k,loc,'cyan' if i%2==0 else 'amber')
  g={44:'ANIM_COPPER_THICKNESS_SCANNER',45:'ANIM_VOID_INSPECTION_HEAD',46:'ANIM_TRENCH_FILL_TOMOGRAPHY',47:'ANIM_OVERBURDEN_SCANNER',48:'ANIM_SURFACE_ROUGHNESS_SCANNER',59:'ANIM_SEED_CONTINUITY_SCANNER'}[i]
  reg(i,g,(loc[0],loc[1]+.24,loc[2]),'translation','x')
  with moving(g):
   for j in range(6):box(k,(loc[0]-.3+j*.12,loc[1]+.28,loc[2]+.25),(.08,.06,.04),'cyan')
   cyl(k,(loc[0],loc[1]-.5,loc[2]),.09,.22,'silver','y')
 elif i==49:
  frame(k,(6.15,1.69,-1.61),(1.5,2.0,1.40),.11,True)
  reg(i,'ANIM_RINSE_CHAMBER',(6.15,2.1,-1.61),'translation','y')
  with moving('ANIM_RINSE_CHAMBER'):
   wafer(k,(6.15,1.61,-1.61),.47,'copper')
   for j in range(12):
    a=j*math.tau/12;fluidpipe(k,[(6.15+.65*math.cos(a),2.35,-1.61+.65*math.sin(a)),(6.15+.42*math.cos(a),1.85,-1.61+.42*math.sin(a))],.019,'cyan',False)
 elif i==50:
  frame(k,(8.03,1.64,-1.62),(1.42,1.95,1.33),.11,True)
  reg(i,'ANIM_SPIN_DRYER',(8.03,1.41,-1.62),'rotation','y')
  with moving('ANIM_SPIN_DRYER'):
   wafer(k,(8.03,1.48,-1.62),.49,'iridescent')
   for j in range(12):
    a=j*math.tau/12;pipe(k,[(8.03,1.34,-1.62),(8.03+.50*math.cos(a),1.34,-1.62+.50*math.sin(a))],.017,'cyan',8)
 elif i==51:
  vessel(k,(-8.18,1.37,-3.65),.64,1.72,'slurry','amber')
  for p in [(-8.5,2.2,-3.65),(-7.8,2.22,-3.65)]:cyl(k,p,.11,.19,'steel','y')
 elif i==52:
  sensor(k,(3.1,.82,3.55),'red')
  for j in range(5):box(k,(2.86+j*.13,.63,3.87),(.09,.055,.07),'amber')
 elif i==53:
  detail_cabinet(k,(8.72,1.09,4.12),'red',.96,1.28,.66)
  reg(i,'ANIM_ESTOP',(8.72,1.57,4.49),'translation','z')
  with moving('ANIM_ESTOP'):
   cyl(k,(8.72,1.57,4.56),.13,.10,'red','z',24)
   ring(k,(8.72,1.57,4.61),.16,.13,.03,'z','amber',22)
 elif i==54:
  detail_cabinet(k,(3.06,1.26,3.57),'blue',1.44,1.50,.82)
  reg(i,'ANIM_PLC_STATUS_DISPLAY',(3.06,1.62,4.00),'visual','z')
  with moving('ANIM_PLC_STATUS_DISPLAY'):statusleds(k,3.04,1.62,4.03,8)
 elif i==55:
  detail_cabinet(k,(4.52,1.15,3.55),'cyan',1.09,1.44,.73)
  for j in range(3):terminal_block(k,(4.3+j*.25,.65,3.95),3,accent='green')
 elif i==56:
  detail_cabinet(k,(6.33,1.24,3.54),'violet',1.25,1.5,.76)
  reg(i,'ANIM_WAVEFORM_DISPLAY',(6.33,1.61,3.95),'visual','z')
  with moving('ANIM_WAVEFORM_DISPLAY'):
   pts=[(5.95+j*.065,1.62+(.055 if j%3==0 else -.06),3.98) for j in range(12)]
   pipe(k,pts,.015,'cyan',8)
 elif i==57:
  giantdisplay(k,(3.8,4.36,-4.06),4.55,2.0)
  reg(i,'ANIM_INTERCONNECT_SCADA',(3.8,4.35,-3.91),'visual','z')
  with moving('ANIM_INTERCONNECT_SCADA'):
   for j in range(25):box(k,(2.05+j*.148,3.81+(j%4)*.06,-3.915),(.09,.028,.016),'cyan')
 elif i==58:
  detail_cabinet(k,(-8.86,1.18,3.56),'green',1.10,1.24,.76)
  for j in range(6):box(k,(-9.22+j*.14,1.80,3.7),(.07,.06,.12),'copper')
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 76 Copper Electroplating Modular Procedural Geometry')]),
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
    filename=f'{i:02d}_L76_{key}'
    export_fbx(key,MD/(filename+'.fbx'))
    export_glb(key,GD/(filename+'.glb'))
    triangles=sum(len(m.faces) for m,_,_ in M[key]);parts=len(M[key])
    coords=np.concatenate([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L76_'+key,file=filename+'.fbx',glb=filename+'.glb',
         triangle_count=triangles,mesh_parts=parts,
         bbox_min=np.around(coords.min(axis=0),4).tolist(),bbox_max=np.around(coords.max(axis=0),4).tolist(),
         rig_groups=list(PV.get(key,{}).keys()),
         animation_pivots={g:list(info['position']) for g,info in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/60] {key:42s} {triangles:7,d} triangles {parts:4d} parts',flush=True)

assembled='Level76_Complete_ArtKit_Showcase'
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
            w.writerow(['L76_'+k,pivot,*info['position'],info['kind'],info['axis'],'Named pivot parent Null nodes in FBX and GLB','No baked animation; follow actual simulation'])
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
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_76',
 'description':'Procedural colored material regions; NOT production hand-painted PBR textures','materials':palette},indent=2))

