#!/usr/bin/env python3
"""Project Spark Level 78: original modular dielectric PECVD, UV cure, and reliability cluster 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical/implantation gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level78_HD_AAA')
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





# LEVEL 78 CUSTOM MODEL SYSTEM
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 78 Multi-Chamber Dielectric Protection Procedural Modular Geometry')]),
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




# ORIGINAL LEVEL 78 equipment modules. Common-origin real polygon geometry (not images).
names='''IndustrialDielectricProtectionBase|TransparentMultichamberSafetyEnclosure|InterconnectWaferCassette|PlanarizedCopperInterconnectWafer|CopperInterconnectLayerHologram|HighVacuumWaferLoadLock|PrecisionVacuumGateValve|CentralVacuumTransferHub|MultiAxisVacuumTransferRobot|OpticalWaferAlignmentStage|InterconnectSurfaceConditionScanner|CopperSurfacePreconditioningModule|CopperCapPECVDReactor|CopperCapReactorLid|CeramicCapGasShowerhead|CapPlasmaRFSourceAssembly|CapRFMatchingNetwork|CapElectrodeDistributionAssembly|CapHeatedWaferSusceptor|CapVacuumExhaustThrottle|CapPlasmaSheathVisualization|VirtualCapPrecursorCartridge|CapPrecursorDeliveryManifold|CapGasMassFlowController|CapFilmThicknessInterferometer|CapDepositionUniformityMonitor|CapContinuityInspectionModule|HolographicCopperDiffusionShield|LowKPECVDReactor|LowKDielectricGasShowerhead|LowKPlasmaSource|LowKRFMatchingAssembly|LowKHeatedWaferChuck|VirtualLowKPrecursorCartridge|VirtualPorogenCartridge|DualChannelLowKGasManifold|LowKGasFlowController|PlasmaIonEnergyMonitor|LowKSurfaceChargingSensor|LowKFTIRBondingAnalyzer|LowKFilmGrowthOpticalMonitor|UVThermalCureReactor|MultiZoneUVLampArray|PrecisionUVCureShutter|RotatingCureWaferPlaten|CureWaferTemperatureSensor|CureOutgassingMonitor|HolographicLowKPoreNetwork|WaferStressBowScanner|DielectricEllipsometryStation|InterlineCombTestFixture|DielectricLeakageCurrentElectrometer|AcceleratedBiasStressTestSource|DielectricReliabilityController|HolographicTDDBReliabilityGraph|SurfaceMoistureInspectionAnalyzer|InterconnectRCDelayAnalyzer|EmergencyIsolationSafetyStation|TurbomolecularVacuumPump|VacuumBackingPump|PrecisionChamberPressureGauge|DielectricProcessPLCRack|HolographicDielectricSCADA|Regulated5VControlSupply'''.split('|')
assert len(names)==64
PV={}
C=(-5.35,2.40,-.55); L=(5.30,2.40,-.55); U=(0,2.43,-4.05);H=(0,2.30,-.55); T=(6.7,1.65,3.4)
# id, parent, kind, axis, XYZ: must be exactly 46 real animation parent groups.
pivots=[
(5,'ANIM_LOADLOCK_DOOR','translation','z',(-8.45,2.0,1.55)),
(6,'ANIM_GATE_VALVE','translation','y',(-6.45,2.35,-.55)),
(8,'ANIM_TRANSFER_ROBOT_BASE','rotation','y',(0,1.70,-.55)),
(8,'ANIM_TRANSFER_ROBOT_ARM','rotation','y',(.25,2.20,-.55)),
(8,'ANIM_TRANSFER_ROBOT_BLADE','rotation','y',(1.20,2.30,-.55)),
(9,'ANIM_WAFER_ALIGNER','rotation','y',(-7.55,2.04,2.05)),
(11,'ANIM_PRECONDITION_SHUTTER','translation','y',(-6.82,2.4,2.0)),
(13,'ANIM_CAP_CHAMBER_LID','translation','y',(-5.35,3.95,-.55)),
(14,'ANIM_CAP_SHOWERHEAD','translation','y',(-5.35,3.14,-.55)),
(15,'ANIM_CAP_RF_PLASMA','indicator','none',(-5.35,2.85,-.55)),
(16,'ANIM_CAP_RF_MATCHING','rotation','y',(-7.95,1.82,-1.95)),
(18,'ANIM_CAP_HEATED_SUSCEPTOR','translation','y',(-5.35,2.12,-.55)),
(18,'ANIM_CAP_WAFER_ROTATION','rotation','y',(-5.35,2.23,-.55)),
(19,'ANIM_CAP_EXHAUST_THROTTLE','rotation','y',(-7.05,1.40,-1.15)),
(21,'ANIM_CAP_PRECURSOR_VALVE','rotation','y',(-8.55,2.35,-2.7)),
(23,'ANIM_CAP_GAS_FLOW','indicator','none',(-7.60,2.70,-2.15)),
(24,'ANIM_CAP_THICKNESS_SCAN','translation','x',(-5.35,4.30,.72)),
(27,'ANIM_CAP_COVERAGE_HOLOGRAM','indicator','none',(-5.5,4.90,2.68)),
(28,'ANIM_LOWK_CHAMBER_LID','translation','y',(5.3,3.95,-.55)),
(29,'ANIM_LOWK_SHOWERHEAD','translation','y',(5.3,3.14,-.55)),
(30,'ANIM_LOWK_RF_PLASMA','indicator','none',(5.3,2.86,-.55)),
(31,'ANIM_LOWK_MATCHING','rotation','y',(7.85,1.82,-1.95)),
(32,'ANIM_LOWK_SUSCEPTOR','translation','y',(5.3,2.10,-.55)),
(33,'ANIM_LOWK_PRECURSOR_VALVE','rotation','y',(8.65,2.37,-2.72)),
(34,'ANIM_LOWK_POROGEN_VALVE','rotation','y',(9.55,2.37,-2.72)),
(35,'ANIM_LOWK_GAS_FLOW','indicator','none',(8.05,2.68,-2.05)),
(38,'ANIM_LOWK_CHARGE_MONITOR','translation','y',(6.25,3.0,.64)),
(40,'ANIM_LOWK_FILM_SCAN','translation','x',(5.3,4.32,.76)),
(41,'ANIM_UV_CURE_DOOR','translation','y',(0,2.32,-2.63)),
(42,'ANIM_UV_LAMP_ARRAY','indicator','none',(0,3.43,-4.05)),
(43,'ANIM_UV_SHUTTER','translation','x',(0,3.25,-4.05)),
(44,'ANIM_CURE_ROTATING_PLATEN','rotation','y',(0,2.08,-4.05)),
(45,'ANIM_CURE_THERMAL_PLATEN','translation','y',(0,1.81,-4.05)),
(46,'ANIM_CURE_OUTGAS','indicator','none',(.85,3.68,-4.05)),
(47,'ANIM_PORE_NETWORK_HOLOGRAM','indicator','none',(0,5.21,2.75)),
(48,'ANIM_FILM_BOW_SCAN','translation','x',(3.80,2.8,3.2)),
(49,'ANIM_ELLIPSOMETRY_HEAD','translation','y',(4.93,2.85,3.2)),
(55,'ANIM_MOISTURE_PROBE','translation','y',(8.38,2.46,3.1)),
(50,'ANIM_LEAKAGE_TEST_PROBE','translation','y',(5.92,2.64,3.25)),
(52,'ANIM_BIAS_STRESS_GUARD','translation','z',(7.02,2.45,3.25)),
(54,'ANIM_TDDB_GRAPH','indicator','none',(7.2,4.42,2.6)),
(56,'ANIM_RC_TIMING_PROBE','translation','y',(9.12,2.50,3.15)),
(58,'ANIM_TURBO_ROTOR','rotation','y',(-3.25,1.30,-3.7)),
(59,'ANIM_BACKING_ROTOR','rotation','x',(-2.3,1.00,-4.60)),
(57,'ANIM_EMERGENCY_ISOLATION','translation','z',(9.8,1.55,4.62)),
(62,'ANIM_DIELECTRIC_SCADA','indicator','none',(0,5.3,-6.0))]
assert len(pivots)==46,len(pivots)
for i,name,kind,axis,xyz in pivots:
    PV.setdefault(names[i],{})[name]={'position':xyz,'kind':kind,'axis':axis}

# Art detail primitives for NEW Level 78 machinery.
def feet(k,x,z,w,d,n=4):
 for dx in (-w*.39,w*.39):
  for dz in (-d*.39,d*.39):
   box(k,(x+dx,.48,z+dz),(.19,.72,.19),'steel')
   box(k,(x+dx,.095,z+dz),(.32,.08,.32),'dark')

def bigreactor(k,x,z,glow='violet'):
 # massive transparent cylindrical reactor, three-tier flanges and hundreds of precision fasteners
 feet(k,x,z,3.65,3.55)
 cyl(k,(x,1.21,z),1.78,.31,'dark','y',64)
 cyl(k,(x,1.44,z),1.62,.15,'steel','y',64)
 cyl(k,(x,2.55,z),1.57,2.15,'glass','y',64)
 for y in (1.46,1.82,2.92,3.67):
  ring(k,(x,y,z),1.77,1.62,.095,'y','steel',48)
  bolt_circle(k,(x,y+.07,z),1.73,24,'dark')
 for angle in np.linspace(0,2*math.pi,12,endpoint=False):
  dx,dz=1.72*math.cos(angle),1.72*math.sin(angle)
  box(k,(x+dx,2.58,z+dz),(.065,1.94,.07),'silver')
 for y,r in ((1.68,1.38),(2.01,1.23),(2.57,1.42),(3.14,1.23)):
  ring(k,(x,y,z),r,r-.045,.027,'y',glow,48)
 cyl(k,(x,3.74,z),1.73,.32,'steel','y',48)
 ring(k,(x,3.95,z),1.37,1.24,.075,'y','silver',48)
 for t in range(20):
  a=t*math.tau/20
  cyl(k,(x+1.38*math.cos(a),3.87,z+1.38*math.sin(a)),.032,.1,'dark','y',8)
 # subtle power/status accessories
 for dx in (-1.6,1.6):
  controlbox(k,(x+dx,2.28,z+1.41),(.45,.63,.26),monitor=glow,knobs=3)


def microdetail_reactor(k,x,z,accent='violet'):
 for idx in range(22):
  a=idx*math.tau/22
  px=x+1.58*math.cos(a);pz=z+1.58*math.sin(a)
  cyl(k,(px,3.41,pz),.03,.09,'silver','y',9)
 for idx in range(15):
  a=idx*math.tau/15
  p=(x+1.93*math.cos(a),1.50,z+1.93*math.sin(a))
  box(k,p,(.08,.13,.08),'dark')
 for sign in (-1,1):
  fluidpipe(k,[(x+sign*1.69,2.80,z),(x+sign*2.04,2.8,z),(x+sign*2.04,1.39,z-.33)],.062,'silver')

def uv_chamber(k,x,z):
 feet(k,x,z,3.2,3.1)
 cyl(k,(x,1.26,z),1.56,.35,'steel','y',48)
 cyl(k,(x,2.45,z),1.42,2.10,'glass','y',56)
 for yy in (1.51,2.44,3.50):ring(k,(x,yy,z),1.57,1.47,.085,'y','gold',48)
 cyl(k,(x,3.56,z),1.54,.27,'steel','y',48)
 for a in range(16):
  t=a*math.tau/16
  p=(x+1.37*math.cos(t),2.45,z+1.37*math.sin(t))
  box(k,p,(.055,1.77,.055),'gold')
 # UV safety glass frames
 frame(k,(x,2.38,z+1.27),(1.80,1.55,.13),.06,True)

def reliability_fixture(k,p,cc='cyan'):
 x,y,z=p
 platform(k,(x,.77,z),(1.18,.21,1.30),cc)
 box(k,(x,1.45,z),(1.1,.12,1.05),'ceramic')
 for ii in range(12):
  xx=x-.43+ii*.08
  box(k,(xx,1.52,z),(.034,.025,.78),'copper' if ii%2 else 'gold')
 box(k,(x,2.06,z-.43),(.13,1.0,.18),'steel')
 box(k,(x,2.47,z),(1.11,.13,.2),'light')
 cyl(k,(x,2.23,z),.12,.37,'silver','y',16)
 cyl(k,(x,1.98,z),.055,.19,cc,'y',12)

def core_console(k,p,accent='cyan',wide=1.0):
 x,y,z=p
 hd_cabinet(k,p,(wide,1.5,.68),accent,2)
 for m in range(6):
  xx=x-wide*.4+m*wide*.16
  box(k,(xx,y-.50,z+.36),(.034,.025,.014),'green' if m<5 else 'amber')
 for t in range(2):
  terminal_block(k,(x+(-.32 if t==0 else .32)*wide,y+.68,z-.22),8,accent=accent)

def smallsensor(k,p,style='cyan'):
 sensor(k,p,style,mini=True)
 x,y,z=p
 pipe(k,[(x,y-.18,z),(x,y-.38,z+.13)],.019,'silver')
 for dy in (-.13,.13):box(k,(x+.18,y+dy,z),(.095,.025,.10),'silver')

def lampbank(k,x,y,z,n=12):
 box(k,(x,y,z),(2.10,.20,2.12),'steel')
 for i in range(n):
  a=i*math.tau/n;r=.76
  xx=x+r*math.cos(a);zz=z+r*math.sin(a)
  cyl(k,(xx,y-.10,zz),.10,.09,'uv','y',16)
  ring(k,(xx,y-.18,zz),.13,.10,.035,'y','gold',18)
 for j in range(9):
  a=j*math.tau/9
  box(k,(x+.35*math.cos(a),y-.13,z+.35*math.sin(a)),(.095,.06,.095),'amber')

def porous_holo(k,p):
 x,y,z=p
 box(k,p,(2.0,1.50,.055),'navy')
 box(k,(x,y,z+.04),(1.9,1.38,.018),'blue')
 for i in range(10):
  for j in range(6):
   xx=x-.83+i*.185; yy=y-.52+j*.19
   cyl(k,(xx,yy,z+.07),.055,.013,'cyan' if (i+j)%3 else 'ice','z',12)
 for j in range(6):
  box(k,(x-.72+j*.28,y+.66,z+.07),(.12,.025,.01),'teal')




def platform(k,pos,ext=(1.6,.30,1.5),highlight='cyan'):
 x,y,z=pos
 box(k,pos,ext,'steel')
 for xx in (x-ext[0]/2+.13,x+ext[0]/2-.13):
  for zz in (z-ext[2]/2+.13,z+ext[2]/2-.13):
   box(k,(xx,y-ext[1]*.55-.35,zz),(.14,.73,.14),'steel')
 box(k,(x,y+ext[1]*.52,z+ext[2]*.35),(ext[0]*.7,.032,.035),highlight)

def fluidpipe(k,pts,r=.065,col='cyan'):
 pipe(k,pts,r,col,12)
 for xyz in (pts[0],pts[-1]):cyl(k,xyz,r*1.4,r*.20,'silver','y',16)

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

def giantdisplay(k,p,width=3.6,height=1.72):
 x,y,z=p
 box(k,(x,y,z),(width+.18,height+.20,.18),'dark')
 box(k,(x,y,z+.11),(width,height,.026),'black')
 for j in range(4):
  px=x-width*.37+j*width*.245
  box(k,(px,y+.20,z+.13),(width*.20,height*.58,.02),'blue' if j%2 else 'navy')
  for n in range(14):
   xx=px-width*.085+n*width*.013
   yy=y+.14+height*.15*math.sin(n*.32+j*.6)
   box(k,(xx,yy,z+.148),(.027,.031,.012),'cyan' if n%4 else 'green')
 for n in range(4):
  xx=x-width*.35+n*width*.22
  box(k,(xx,y-height*.38,z+.14),(width*.17,.028,.02),'cyan' if n%3 else 'amber')
 for sgn in (-1,1):
  box(k,(x+sgn*width*.49,y,z-.07),(.07,height+.22,.2),'light')
 box(k,(x,y+height*.59,z),(width+.28,.11,.19),'silver')

def build_model(i):
 k=names[i]
 if i==0:
  box(k,(0,.37,0),(21.15,.66,12.2),'navy')
  box(k,(0,.72,0),(20.85,.055,11.95),'steel')
  for xx in np.arange(-9.5,10.0,1.75):
   box(k,(xx,.76,4.94),(.73,.018,.055),'amber')
   box(k,(xx,.76,-5.0),(.73,.018,.055),'cyan')
  for xx in (-9.8,-7.2,-4.5,-1.5,1.5,4.5,7.2,9.8):
   box(k,(xx,1.19,-5.84),(.14,.95,.14),'light')
   box(k,(xx,1.70,-5.84),(.28,.06,.30),'ice')
  # cable trenches and bolted deck
  for zz in (-4.8,4.2):
   for x in np.arange(-9.6,9.8,.63):box(k,(x,.755,zz),(.14,.013,.16),'dark')
  for xx in (-5.35,0,5.30):
   cyl(k,(xx,.84,-.55),1.99,.24,'dark','y',56)
   ring(k,(xx,.98,-.55),1.93,1.85,.05,'y','cyan' if xx>0 else 'violet',44)
  cyl(k,(0,.87,-4.05),1.76,.26,'dark','y',48)
  for x in (-10.36,10.36):
   for z in (-5.87,5.87):box(k,(x,.85,z),(.19,.32,.19),'warning')
 elif i==1:
  frame(k,(0,3.02,-1.1),(20.0,4.3,9.4),.12,True)
  for x in np.arange(-9,10,2):
   box(k,(x,5.13,-1.1),(.06,.14,9.0),'silver')
   for z in (-5.6,3.3):box(k,(x,4.94,z),(.09,.25,.09),'cyan')
  for x in (-9.80,9.80):
   for z in (-5.5,3.2):box(k,(x,2.90,z),(.1,3.7,.1),'silver')
  for x in (-8.3,8.3):
   controlbox(k,(x,1.44,4.0),(.85,.66,.43),monitor='green')
 elif i==2:
  platform(k,(-9.00,.77,3.45),(1.65,.26,1.55),'cyan')
  box(k,(-9.0,1.6,3.45),(1.30,1.33,1.28),'steel')
  for j in range(11):
   yy=1.05+j*.097
   box(k,(-9.00,yy,3.47),(1.13,.025,1.02),'dark')
   waferdisc(k,(-9.0,yy+.025,3.47),.49,.019,'copper',False)
  box(k,(-9.0,2.32,3.45),(1.43,.13,1.41),'silver')
 elif i==3:
  waferdisc(k,(-7.62,2.04,3.15),.77,.040,'copper',False)
  for a in range(24):
   t=a*math.tau/24
   xx=-7.62+.61*math.cos(t);zz=3.15+.61*math.sin(t)
   box(k,(xx,2.064,zz),(.095,.016,.12),'gold')
  for ix in range(-5,6):
   for iz in range(-5,6):
    if ix*ix+iz*iz<29:
     col='copper' if (ix+iz)%2 else 'gold'
     box(k,(-7.62+ix*.105,2.065,3.15+iz*.105),(.056,.015,.056),col)
 elif i==4:
  porous_holo(k,(-5.15,4.97,2.82))
  for j,col in enumerate(('copper','gold','cyan','ice')):
   box(k,(-5.91+j*.51,4.32,2.89),(.38,.06,.025),col)
 elif i==5:
  x,z=-8.25,1.3
  cyl(k,(x,1.64,z),1.10,.58,'dark','y',44)
  cyl(k,(x,2.23,z),1.0,1.22,'glass','y',48)
  ring(k,(x,1.74,z),1.14,1.02,.12,'y','silver',44)
  ring(k,(x,2.79,z),1.14,1.02,.12,'y','steel',44)
  for aa in range(16):
   t=aa*math.tau/16
   cyl(k,(x+1.1*math.cos(t),2.89,z+1.1*math.sin(t)),.031,.11,'black','y',8)
  frame(k,(x,2.18,z+1.01),(1.1,1.0,.12),.06,True)
 elif i==6:
  x,y,z=-6.45,2.35,-.55
  ring(k,(x,y,z),.94,.75,.25,'x','steel',36)
  ring(k,(x,y,z),.75,.70,.27,'x','cyan',36)
  box(k,(x,y+1.0,z),(.34,.70,.48),'dark')
  for t in (-1,1):box(k,(x,y+t*.64,z),(.38,.04,1.08),'silver')
 elif i==7:
  x,z=0,-.55
  cyl(k,(x,1.62,z),1.67,.42,'steel','y',56)
  cyl(k,(x,2.46,z),1.55,1.58,'glass','y',64)
  for yy in (1.82,2.25,3.05):ring(k,(x,yy,z),1.68,1.55,.12,'y','light',48)
  cyl(k,(x,3.28,z),1.68,.29,'steel','y',56)
  for a in range(12):
   t=a*math.tau/12
   box(k,(x+1.54*math.cos(t),2.48,z+1.54*math.sin(t)),(.06,1.40,.06),'silver')
 elif i==8:
  x,z=0,-.55
  cyl(k,(x,1.77,z),.60,.36,'dark','y',30)
  cyl(k,(x,2.05,z),.30,.51,'silver','y',24)
  box(k,(x+.51,2.17,z),(1.25,.22,.31),'steel')
  box(k,(x+1.36,2.18,z),(.88,.17,.20),'silver')
  for j in range(8):
   a=j*math.tau/8
   cyl(k,(x+.42*math.cos(a),1.91,z+.42*math.sin(a)),.04,.12,'cyan','y',8)
 elif i==9:
  platform(k,(-7.55,1.1,2.05),(1.55,.25,1.55),'cyan')
  waferdisc(k,(-7.55,2.04,2.05),.70,.028,'iridescent')
  ring(k,(-7.55,1.98,2.05),.87,.81,.09,'y','steel',44)
  for j in range(4):smallsensor(k,(-7.55+(.88 if j%2 else -.88),2.2,2.05+(j//2-.5)*.62),'green')
 elif i==10:
  platform(k,(-7.17,1.0,3.3),(1.35,.20,1.15),'cyan')
  multisensor(k,-7.18,2.00,3.77,'pressure',.31)
  box(k,(-7.18,1.68,3.4),(1.16,.15,.81),'silver')
  for j in range(8):
   box(k,(-7.62+j*.125,1.78,3.32),(.08,.02,.40),'copper')
 elif i==11:
  x,z=-6.82,2.0
  cyl(k,(x,1.58,z),.77,.36,'dark','y',32)
  cyl(k,(x,2.13,z),.72,.95,'glass','y',40)
  ring(k,(x,2.66,z),.84,.69,.10,'y','silver',36)
  cyl(k,(x,2.56,z),.54,.14,'ceramic','y',24)
  fluidpipe(k,[(x-.4,2.9,z-.14),(x-.4,3.10,z+.3)],.05,'cyan')
 elif i==12:bigreactor(k,C[0],C[2],'violet');microdetail_reactor(k,C[0],C[2],'violet')
 elif i==13:
  x,z=C[0],C[2]
  cyl(k,(x,3.95,z),1.76,.38,'steel','y',56)
  ring(k,(x,4.16,z),1.84,1.62,.08,'y','silver',48)
  for j in range(20):
   a=j*math.tau/20
   cyl(k,(x+1.72*math.cos(a),4.17,z+1.72*math.sin(a)),.034,.065,'dark','y',10)
  for dx in (-1.1,1.1):box(k,(x+dx,4.29,z),(.28,.22,.28),'steel')
 elif i==14:
  x,z=C[0],C[2]
  cyl(k,(x,3.10,z),1.24,.14,'ceramic','y',48)
  cyl(k,(x,3.2,z),1.32,.12,'steel','y',48)
  for rr,n in ((.3,9),(.6,19),(.94,31)):
   for j in range(n):
    a=j*math.tau/n
    cyl(k,(x+rr*math.cos(a),3.006,z+rr*math.sin(a)),.038,.017,'dark','y',8)
 elif i==15:
  for yy,r in ((2.72,1.10),(2.94,1.24),(3.09,.88)):
   ring(k,(C[0],yy,C[2]),r,r-.11,.05,'y','plasma',48)
  for j in range(14):
   a=j*math.tau/14
   pipe(k,[(C[0],2.69,C[2]),(C[0]+1.15*math.cos(a),2.91,C[2]+1.15*math.sin(a))],.020,'violet')
 elif i==16:core_console(k,(-7.95,1.55,-1.95),'violet',1.20);motor(k,(-8.05,2.05,-2.58),.36)
 elif i==17:
  for y,r in ((2.96,1.18),(2.79,1.08)):
   precision_ring(k,(C[0],y,C[2]),r,r-.18,.05,'y','copper')
  for j in range(18):
   a=j*math.tau/18
   box(k,(C[0]+.98*math.cos(a),2.82,C[2]+.98*math.sin(a)),(.07,.15,.06),'gold')
 elif i==18:
  x,z=C[0],C[2]
  cyl(k,(x,2.04,z),1.03,.21,'steel','y',48)
  ring(k,(x,2.20,z),1.08,.96,.04,'y','amber',40)
  waferdisc(k,(x,2.27,z),.84,.025,'copper',True)
  for j in range(16):
   a=j*math.tau/16
   box(k,(x+.90*math.cos(a),2.09,z+.90*math.sin(a)),(.11,.15,.11),'dark')
 elif i==19:
  duct(k,[(-7.05,2.17,-.55),(-7.05,1.40,-1.15),(-7.49,1.22,-1.15)],.24,'steel')
  cyl(k,(-7.05,1.40,-1.15),.32,.25,'dark','y',32)
  box(k,(-7.05,1.77,-1.15),(.31,.42,.35),'steel')
 elif i==20:
  x,z=C[0],C[2]
  for j in range(17):
   a=j*math.tau/17
   pt=[(x+.65*math.cos(a),2.42,z+.65*math.sin(a)),(x+1.21*math.cos(a),2.83,z+1.21*math.sin(a))]
   pipe(k,pt,.029,'plasma',8)
 elif i==21:
  x,z=-8.55,-2.70
  reservoir(k,(x,1.62,z),'violet')
  cyl(k,(x,2.52,z),.31,.20,'ceramic','y',24)
  ring(k,(x,2.58,z),.37,.28,.04,'y','gold',28)
 elif i==22:
  x=-7.46;z=-2.70
  for dx in (-.5,.5):
   fluidpipe(k,[(x+dx,1.6,z),(x+dx,2.7,z),(x,2.7,z+.62)],.051,'violet')
  box(k,(x,1.2,z),(1.48,.58,.74),'steel')
  for j in range(6):pulsevalve(k,(x-.6+j*.24,2.65,z),'violet')
 elif i==23:
  for j in range(3):smallsensor(k,(-7.75+j*.32,2.62,-2.15),'violet')
  fluidpipe(k,[(-7.53,2.6,-2.35),(-7.52,3.46,-2.0),(-6.45,3.46,-1.05)],.06,'copper')
 elif i==24:
  x,z=-5.35,.72
  box(k,(x,4.20,z),(2.0,.16,.18),'steel')
  for j in range(10):
   xx=x-.87+j*.19
   cyl(k,(xx,4.12,z),.03,.14,'cyan','y',10)
  multisensor(k,x,4.40,z+.25,'pressure',.22)
 elif i==25:
  porous_holo(k,(-5.25,4.7,2.05))
  for j in range(16):
   a=j*math.tau/16
   box(k,(-5.25+.64*math.cos(a),4.70+.48*math.sin(a),2.11),(.055,.05,.025),'green')
 elif i==26:
  hd_cabinet(k,(-7.25,1.9,2.0),(1.18,1.40,.84),'green',2)
  for j in range(7):
   box(k,(-7.65+j*.13,2.48,2.47),(.04,.045,.03),'cyan')
 elif i==27:
  porous_holo(k,(-5.50,4.90,2.68))
  for j in range(13):
   box(k,(-6.26+j*.12,4.36,2.74),(.07,.09,.018),'gold' if j%5==0 else 'green')
 elif i==28:bigreactor(k,L[0],L[2],'cyan');microdetail_reactor(k,L[0],L[2],'cyan')
 elif i==29:
  x,z=L[0],L[2]
  cyl(k,(x,3.12,z),1.32,.16,'ceramic','y',48)
  cyl(k,(x,3.24,z),1.41,.13,'steel','y',48)
  for rr,n in ((.32,10),(.68,18),(1.07,35)):
   for j in range(n):
    a=j*math.tau/n
    cyl(k,(x+rr*math.cos(a),3.005,z+rr*math.sin(a)),.033,.022,'black','y',8)
 elif i==30:
  x,z=L[0],L[2]
  for yy,rr in ((2.64,.91),(2.83,1.2),(3.06,1.04)):
   ring(k,(x,yy,z),rr,rr-.09,.036,'y','ice',48)
  for i2 in range(15):
   a=i2*math.tau/15
   pipe(k,[(x+.31*math.cos(a),2.64,z+.31*math.sin(a)),(x+1.03*math.cos(a),3.02,z+1.03*math.sin(a))],.028,'cyan',8)
 elif i==31:core_console(k,(7.85,1.55,-1.95),'cyan',1.15);motor(k,(8.10,2.0,-2.55),.35)
 elif i==32:
  x,z=L[0],L[2]
  cyl(k,(x,2.03,z),1.03,.19,'steel','y',48)
  ring(k,(x,2.18,z),1.08,.98,.041,'y','cyan',48)
  waferdisc(k,(x,2.26,z),.85,.028,'iridescent')
  for j in range(8):
   a=j*math.tau/8
   pipe(k,[(x+.82*math.cos(a),1.96,z+.82*math.sin(a)),(x+1.14*math.cos(a),1.70,z+1.14*math.sin(a))],.040,'copper')
 elif i in (33,34):
  x=8.65 if i==33 else 9.55;z=-2.72
  reservoir(k,(x,1.64,z),'cyan' if i==33 else 'gold')
  for y in (2.24,2.52):ring(k,(x,y,z),.37,.31,.036,'y','silver',28)
 elif i==35:
  for j in range(2):
   x=7.90+j*.75
   fluidpipe(k,[(x,1.72,-2.52),(x,3.08,-2.52),(6.62,3.08,-1.32)],.065,'cyan' if j==0 else 'gold')
  controlbox(k,(8.2,1.08,-1.98),(1.40,.70,.57),monitor='cyan')
 elif i==36:
  for j in range(4):smallsensor(k,(7.7+j*.27,2.55,-2.05),'cyan')
  pipe(k,[(7.65,2.65,-1.86),(6.94,3.32,-1.40)],.040,'silver')
 elif i==37:
  hd_cabinet(k,(7.58,1.68,1.35),(1.10,1.35,.71),'amber',2)
  for j in range(5):
   cyl(k,(7.14+j*.22,2.56,1.2),.03,.22,'gold','y',8)
 elif i==38:
  smallsensor(k,(6.25,3.0,.64),'amber')
  for t in range(3):pipe(k,[(6.15+t*.07,2.74,.5),(5.84,2.55,.37)],.019,'amber')
 elif i==39:core_console(k,(9.55,1.44,.90),'cyan',1.1)
 elif i==40:
  x,z=5.30,.76
  box(k,(x,4.19,z),(2.13,.18,.18),'steel')
  for j in range(12):
   xx=x-1.0+j*.18
   cyl(k,(xx,4.12,z),.027,.14,'cyan','y',10)
  multisensor(k,x,4.42,z+.27,'flow',.2)
 elif i==41:uv_chamber(k,U[0],U[2])
 elif i==42:lampbank(k,0,3.43,-4.05,16)
 elif i==43:
  box(k,(0,3.25,-4.05),(2.06,.09,2.05),'dark')
  for j in range(12):
   box(k,(-.90+j*.165,3.19,-4.05),(.07,.02,1.92),'gold')
 elif i==44:
  x,z=0,-4.05
  cyl(k,(x,2.08,z),1.02,.20,'dark','y',40)
  waferdisc(k,(x,2.21,z),.78,.027,'iridescent')
  ring(k,(x,2.12,z),1.13,1.08,.055,'y','gold',44)
 elif i==45:
  x,z=0,-4.05
  cyl(k,(x,1.78,z),1.18,.21,'steel','y',40)
  for j in range(12):
   a=j*math.tau/12
   box(k,(x+.99*math.cos(a),1.91,z+.99*math.sin(a)),(.09,.044,.08),'warm')
 elif i==46:
  for j in range(12):
   a=j*math.tau/12
   pipe(k,[(.55*math.cos(a),2.63,-4.05+.55*math.sin(a)),(.78*math.cos(a),3.54,-4.05+.78*math.sin(a))],.022,'uv',8)
 elif i==47:
  porous_holo(k,(0,5.21,2.75))
  for j in range(14):
   t=j*math.tau/14
   cyl(k,(.82*math.cos(t),5.18+.43*math.sin(t),2.82),.055,.02,'gold','z',10)
 elif i==48:
  reliability_fixture(k,(3.80,1.75,3.2),'cyan')
  sensor(k,(3.80,2.80,3.2),'cyan',True)
 elif i==49:
  reliability_fixture(k,(4.93,1.75,3.2),'cyan')
  for j in range(9):
   a=j*math.tau/9
   pipe(k,[(4.93+.5*math.cos(a),2.32,3.2+.5*math.sin(a)),(4.93,2.76,3.2)],.015,'laser',8)
 elif i==50:
  reliability_fixture(k,(5.92,1.75,3.25),'amber')
  for j in range(18):
   x=5.56+j*.041
   box(k,(x,1.64,3.25),(.019,.012,.71),'copper' if j%2 else 'gold')
 elif i==51:
  core_console(k,(6.18,1.45,4.88),'cyan',1.0)
  for j in range(5):
   box(k,(5.85+j*.16,2.15,4.72),(.07,.06,.095),'cyan')
 elif i==52:
  core_console(k,(7.08,1.44,4.88),'amber',1.12)
  for j in range(8):
   box(k,(6.62+j*.12,2.31,4.68),(.08,.07,.10),'warning' if j%3==0 else 'gold')
 elif i==53:core_console(k,(8.04,1.52,4.88),'cyan',1.2)
 elif i==54:
  giantdisplay(k,(7.15,4.47,2.58),2.58,1.30)
  for j in range(28):
   xx=6.09+j*.082; yy=4.07+.30*math.sin(j*.18)+.12*(j/28)**3
   box(k,(xx,yy,2.68),(.041,.026,.018),'green' if j<18 else 'amber')
 elif i==55:
  reliability_fixture(k,(8.38,1.75,3.1),'cyan')
  smallsensor(k,(8.38,2.46,3.1),'cyan')
 elif i==56:
  reliability_fixture(k,(9.12,1.76,3.15),'amber')
  for j in range(10):
   x=8.84+j*.064
   box(k,(x,1.55,3.15),(.022,.02,.64),'copper')
 elif i==57:
  controlbox(k,(9.8,1.55,4.62),(.9,1.16,.60),'steel','red',4)
  cyl(k,(9.8,1.59,4.97),.19,.13,'red','z',24)
  ring(k,(9.8,1.59,5.05),.27,.24,.048,'z','amber',24)
 elif i==58:
  x,z=-3.25,-3.70
  cyl(k,(x,1.32,z),.65,1.51,'steel','y',32)
  cyl(k,(x,2.20,z),.43,.25,'black','y',28)
  ring(k,(x,2.10,z),.67,.58,.09,'y','silver',28)
  for j in range(12):
   a=j*math.tau/12
   box(k,(x+.59*math.cos(a),1.45,z+.59*math.sin(a)),(.063,.83,.045),'light')
  pipe(k,[(x,2.36,z),(x,2.69,z+.40),(x-1.35,2.69,z+.40)],.18,'steel')
 elif i==59:
  motor(k,(-2.3,1.05,-4.60),.91)
  pipe(k,[(-2.75,1.35,-4.60),(-3.3,1.35,-4.60)],.18,'steel')
  for j in range(7):
   box(k,(-2.5+j*.25,1.66,-4.25),(.09,.15,.10),'light')
 elif i==60:
  controlbox(k,(-4.23,1.45,4.85),(1.1,1.18,.67),'steel','cyan',3)
  for j in range(3):gauge(k,(-4.59+j*.3,2.10,5.16),.14)
 elif i==61:
  core_console(k,(-1.90,1.65,4.91),'cyan',1.40)
  for j in range(10):
   box(k,(-2.48+j*.13,2.54,4.96),(.07,.08,.10),'silver')
 elif i==62:
  giantdisplay(k,(0,5.30,-6.0),6.65,2.08)
  # seven separate tall telemetry window trims
  for j,col in enumerate(('violet','cyan','gold','green','red','teal')):
   x=-2.68+j*1.04
   ring(k,(x,5.35,-5.80),.22,.18,.04,'z',col,20)
   for m in range(8):
    yy=4.67+m*.12
    box(k,(x+.25,yy,-5.80),(.20,.028,.022),col if m%2 else 'silver')
 elif i==63:
  core_console(k,(-8.82,1.53,4.93),'green',1.15)
  for j in range(14):
   box(k,(-9.31+j*.075,2.15,5.2),(.025,.12,.035),'gold' if j%3 else 'red')
 else: raise AssertionError(i)
 # Add real named movable subassemblies (not dummy empty pivot parents).
 for pivot,info in PV.get(k,{}).items():
  with moving(pivot):
   x,y,z=info['position'];kind=info['kind'];axis=info['axis'];up=pivot.upper()
   if kind=='indicator':
    # Emissive physical status housing or microstructured hologram bound to parent.
    if 'GRAPH' in up or 'SCADA' in up or 'HOLOGRAM' in up:
     box(k,(x,y,z+.082),(.48,.28,.03),'cyan')
     for j in range(11):
      box(k,(x-.19+j*.039,y+.02+.08*math.sin(j*.5),z+.113),(.025,.024,.011),'green')
    elif 'PLASMA' in up or 'GAS' in up or 'OUTGAS' in up:
     for j in range(13):
      a=j*math.tau/13
      pipe(k,[(x+.15*math.cos(a),y-.08,z+.15*math.sin(a)),(x+.44*math.cos(a),y+.11,z+.44*math.sin(a))],.017,'uv' if 'CURE' in up else 'cyan',8)
    elif 'UV_LAMP' in up:lampbank(k,x,y,z,10)
    else:
     box(k,(x,y,z+.04),(.25,.13,.068),'cyan')
     for j in range(5):box(k,(x-.095+j*.042,y,z+.080),(.02,.039,.013),'green')
   elif kind=='rotation':
    # Fine machined rotor/fan/valve component with offset scale based on equipment.
    if 'ROBOT' in up:
     cyl(k,(x,y,z),.25,.19,'silver','y',24)
     box(k,(x+.26,y+.025,z),(.65,.13,.24),'steel')
     box(k,(x+.54,y+.05,z),(.25,.052,.35),'cyan')
    elif 'ROTOR' in up:
     fan(k,(x,y,z),.27,axis)
    elif 'VALVE' in up or 'THROTTLE' in up:
     cyl(k,(x,y,z),.30,.08,'steel','y',24)
     for j in range(6):box(k,(x+.14*math.cos(j*math.tau/6),y+.07,z+.14*math.sin(j*math.tau/6)),(.09,.055,.08),'cyan')
    else:
     cyl(k,(x,y,z),.27,.075,'dark','y',28)
     ring(k,(x,y+.06,z),.29,.245,.035,'y','silver',28)
     for j in range(8):
      a=j*math.tau/8
      box(k,(x+.22*math.cos(a),y+.08,z+.22*math.sin(a)),(.06,.025,.09),'cyan')
   else:
    # Real movable lids/shutters, optical probes, lamp slides, and safety actuator blocks.
    if 'DOOR' in up or 'LID' in up:
     box(k,(x,y,z),(.78,.16,.78),'steel')
     box(k,(x,y+.10,z),(.63,.035,.59),'silver')
     for j in range(8):
      a=j*math.tau/8
      box(k,(x+.30*math.cos(a),y+.15,z+.30*math.sin(a)),(.07,.045,.07),'gold')
    elif 'SHOWER' in up:
     cyl(k,(x,y,z),.58,.13,'ceramic','y',36)
     for j in range(20):
      a=j*math.tau/20
      cyl(k,(x+.42*math.cos(a),y-.08,z+.42*math.sin(a)),.025,.015,'dark','y',8)
    elif 'SUSCEPTOR' in up or 'PLATEN' in up:
     cyl(k,(x,y,z),.65,.12,'steel','y',36)
     ring(k,(x,y+.07,z),.68,.57,.029,'y','gold',32)
    elif 'PROBE' in up or 'MONITOR' in up or 'SCAN' in up or 'TEST' in up:
     box(k,(x,y,z),(.40,.27,.28),'steel')
     cyl(k,(x,y-.26,z),.04,.37,'silver','y',12)
     cyl(k,(x,y-.47,z),.02,.075,'cyan','y',10)
    elif 'SHUTTER' in up or 'GATE' in up:
     box(k,(x,y,z),(.69,.05,.63),'steel')
     box(k,(x,y+.04,z),(.51,.025,.48),'dark')
    else:
     box(k,(x,y,z),(.41,.20,.28),'steel')
     for j in range(3):box(k,(x-.14+j*.14,y+.12,z),(.053,.035,.068),'amber')
 # add tasteful manufacturing status LEDs to each compact module (not assembled base)
 if i not in (0,1,3,4,7,8,12,13,14,15,20,27,28,29,30,41,42,47,54,62):
  if M[k]:
   vertices=np.vstack([np.asarray(m.vertices) for m,col,grp in M[k]])
   center=np.mean(np.stack([vertices.min(0),vertices.max(0)]),axis=0)
   for d in range(4):
    box(k,(center[0]-.12+d*.08,min(4.6,center[1]+.24),center[2]+.20),(.038,.043,.025),'green' if d%3 else 'cyan')

# Export verified binary FBX 7400 with actual pivot parent nodes, and matching GLB scene graphs.
for idx in range(64):
    build_model(idx)
    key=names[idx]
    if not M.get(key):raise ValueError('Empty model: '+key)
    rigged={g for mesh,mat,g in M[key] if g}
    assert rigged==set(PV.get(key,{})),(key,rigged,set(PV.get(key,{})))
print('MODELED:',len(names),'equipment;',sum(len(M[n]) for n in names),'parts;',sum(sum(len(m.faces) for m,c,p in M[n]) for n in names),'tris;',sum(map(len,PV.values())),'pivots',flush=True)
manifest=[]
for i,key in enumerate(names):
    prefix=f'{i:02d}_L78_{key}'
    export_fbx(key,MD/(prefix+'.fbx'))
    export_glb(key,GD/(prefix+'.glb'))
    triangles=sum(len(mesh.faces) for mesh,_,_ in M[key]);parts=len(M[key])
    coords=np.vstack([mesh.vertices for mesh,_,_ in M[key]])
    manifest.append(dict(index=i,name='L78_'+key,file=prefix+'.fbx',glb=prefix+'.glb',triangle_count=triangles,mesh_parts=parts,bbox_min=np.around(coords.min(axis=0),5).tolist(),bbox_max=np.around(coords.max(axis=0),5).tolist(),rig_groups=list(PV.get(key,{})),animation_pivots={g:list(v['position']) for g,v in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/64] {key:41s} {triangles:7,d} triangles {parts:4d} parts',flush=True)
assembled='Level78_Complete_ArtKit_Showcase'
M[assembled]=[];PV[assembled]={}
for key in names:
    for g,info in PV.get(key,{}).items():PV[assembled][key+'__'+g]=info
    for mesh,material,g in M[key]:M[assembled].append((mesh,material,key+'__'+g if g else None))
print('Exporting assembled FBX + GLB...',flush=True)
export_fbx(assembled,MD/(assembled+'.fbx'))
export_glb(assembled,GD/(assembled+'.glb'))
with (ROOT/'Animation_Rigs.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','Pivot','X','Y','Z','Kind','Axis','FBXHierarchy','Notes'])
    for key in names:
        for pivot,info in PV.get(key,{}).items():w.writerow(['L78_'+key,pivot,*info['position'],info['kind'],info['axis'],'Named pivot parent Null nodes in FBX and GLB','Visual-only pivots; no baked clips'])
with (ROOT/'Art_Assembly.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','FBX','PositionX','PositionY','PositionZ','RotationX','RotationY','RotationZ','Scale','Notes'])
    for m in manifest:w.writerow([m['name'],m['file'],0,0,0,0,0,0,1,'Common origin; verify FBX cm->m importer conversion once'])
with (ROOT/'Component_Roles.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Index','Model','FBX','Triangles','MeshParts','PivotGroups'])
    for m in manifest:w.writerow([m['index'],m['name'],m['file'],m['triangle_count'],m['mesh_parts'],';'.join(m['rig_groups'])])
palette={}
for name,rgba in P.items():
    palette['PS_'+name]={'rgba_255':list(rgba),'metallic':.84 if name in ('steel','light','silver','copper','gold') else .08,'smoothness':.86 if name in ('steel','silver','glass','copper','gold','iridescent') else .54,'transparent':rgba[3]<255,'suggested_emissive':name in ('cyan','blue','green','amber','violet','ice','red','gold','teal')}
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_78','description':'Procedural material-region colors only, not final artist-authored PBR','materials':palette},indent=2))
verts=np.vstack([np.asarray(mesh.vertices) for mesh,_,_ in M[assembled]])
outline=np.around([verts.min(axis=0),verts.max(axis=0)],4)
meta=dict(level_id='level_78',title='Seal the Signal',individual_models=64,individual_glbs=64,assembled_fbx='Models/Level78_Complete_ArtKit_Showcase.fbx',assembled_glb='GLB/Level78_Complete_ArtKit_Showcase.glb',animation_parent_pivot_groups=sum(map(len,PV.values()))-len(PV[assembled]),planned_player_wires=118,planned_unique_functional_terminals=182,critical_feedback_wire='W083',critical_feedback_source='L78_DielectricLeakageCurrentElectrometer',critical_feedback_destination='L78_DielectricReliabilityController',workstation_nominal_width_m=21.2,actual_equipment_bbox_m=outline.tolist(),actual_equipment_width_m=float(outline[1,0]-outline[0,0]),individual_triangle_count=sum(m['triangle_count'] for m in manifest),individual_mesh_parts=sum(m['mesh_parts'] for m in manifest),models=manifest,actual_player_wires_included=False,functional_spark_terminals_included=False,dielectric_pecvd_physics_included=False,six_authoritative_objectives_implemented=False,unity_compilation_tested=False,unity_play_mode_tested=False,art_scope='procedural HD/AAA-inspired mesh art with preview material colors; geometry only')
assert meta['animation_parent_pivot_groups']==46,meta['animation_parent_pivot_groups']
(ROOT/'Model_Manifest.json').write_text(json.dumps(meta,indent=2),encoding='utf-8')
print('FINAL ART METADATA',meta['individual_triangle_count'],'triangles,',meta['individual_mesh_parts'],'parts;',meta['actual_equipment_width_m'],'m width;',meta['animation_parent_pivot_groups'],'pivots',flush=True)
