#!/usr/bin/env python3
"""Project Spark Level 75: procedural original ion implantation 3D models, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical/implantation gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level75_HD_AAA')
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



# LEVEL 75: WRITE THE SILICON — unique modular ion implantation beamline
names=[
'IndustrialIonImplantationBase','TransparentBeamlineSafetyEnclosure','PrecisionIonSourceAssembly','VirtualDopantSourceCartridge','IonSourcePlasmaChamber','IonSourceEmissionModule','IonExtractionElectrodeAssembly','VirtualHighVoltagePowerCabinet','MagneticMassAnalyzer','MassResolvingAperture','BeamEnergySelectionModule','CeramicIonAccelerationColumn','NeutralParticleSeparationModule','QuadrupoleBeamFocusingLens','ElectromagneticBeamSteeringAssembly','ProgrammableXYBeamScanner','FastBeamBlankingGate','PrimaryBeamCurrentMonitor','PrecisionFaradayCupDosimeter','FourCornerBeamUniformityCups','WaferChargeNeutralizationModule','TransparentWaferEndstation','ElectrostaticWaferHoldingChuck','WaferTiltRotationStage','PatternedSiliconWafer','DevelopedPhotoresistImplantMask','ImplantedDopantRegionVisualization','HolographicDopantDepthProfile','AutomatedWaferTransferRobot','WaferCarrierCassette','VacuumWaferLoadLock','LoadLockGateValve','TurbomolecularVacuumPump','VacuumBackingPump','AutomaticVacuumThrottleValve','PrecisionVacuumPressureGauge','ResidualGasDiagnosticModule','WaferTemperatureSensor','WaferCoolingSystem','ProtectedBeamDumpCollector','BeamlineSafetyInterlockRelay','BeamProfileInspectionScanner','IonSpeciesPurityAnalyzer','IntegratedIonDoseController','WaferDoseUniformityAnalyzer','WaferSurfacePotentialSensor','CrystalLatticeDamageAnalyzer','RapidThermalActivationChamber','MultiZoneActivationHeater','ActivationTemperatureSensor','SheetResistanceMetrologyProbe','DopantJunctionDepthScanner','WaferRegistrationCamera','IonImplantationPLCRack','BeamPowerIsolationController','EmergencyImplantationSafetyStation','HolographicIonImplantationSCADA','Regulated5VControlSupply']
assert len(names)==58
PV={}
def register(index,pivot,position,kind='rotational',axis='y'):
    PV.setdefault(names[index],{})[pivot]=dict(position=tuple(position),kind=kind,axis=axis)

def linebeam(k,a,b,r=.06,c='cyan'):
    pipe(k,[a,b],r,c,12)

def ringshaft(k,pos,length=1.4,radius=.36,axis='x',material='steel',accent='cyan',collars=8):
    cyl(k,pos,radius,length,material,axis,32)
    for j in range(collars):
        t=(j/(collars-1)-.5)*length*.9
        p=(pos[0]+t,pos[1],pos[2]) if axis=='x' else ((pos[0],pos[1]+t,pos[2]) if axis=='y' else (pos[0],pos[1],pos[2]+t))
        ring(k,p,radius*1.09,radius*.94,.055,axis,accent,28)

def coilwrap(k,pos,r=.74,length=1.1,turns=19,axis='x',col='copper'):
    x,y,z=pos
    pts=[]
    for i in range(turns*12+1):
        t=i/(turns*12);a=2*math.pi*turns*t
        if axis=='x':pts.append((x+length*(t-.5),y+r*math.sin(a),z+r*math.cos(a)))
        else:pts.append((x+r*math.cos(a),y+length*(t-.5),z+r*math.sin(a)))
    pipe(k,pts,.029,col,8)

def flangeX(k,x,y,z,r=.45):
    ring(k,(x,y,z),r,r*.82,.075,'x','steel',32)
    for j in range(10):
        a=j*2*math.pi/10;cyl(k,(x,y+(r-.045)*math.sin(a),z+(r-.045)*math.cos(a)),.023,.085,'silver','x',10)

def beamsegment(k,x0,x1,y=2.85,z=-.65,r=.27):
    cyl(k,((x0+x1)/2,y,z),r,x1-x0,'glass','x',24)
    for x in (x0,x1):flangeX(k,x,y,z,r+.07)
    for x in np.linspace(x0+.25,x1-.25,max(2,int((x1-x0)*3))):
        ring(k,(x,y,z),r*1.07,r*.96,.032,'x','silver',22)

def sideconsole(k,x,z=3.36,accent='cyan',h=1.02):
    hd_cabinet(k,(x,.77+h*.55,z),(1.10,h,.70),accent,1)

def cable(k,pts,c='copper',r=.024):
    pipe(k,pts,r,c,9)

def coaxcoil(k,x,y,z,R=.6,axis='x',n=11):
    for i in range(n):
        tt=(i-(n-1)/2)*.072
        p=(x+tt,y,z) if axis=='x' else (x,y+tt,z)
        ring(k,p,R*(1+.02*math.sin(i)),R*.82,.034,axis,'copper',32)

def sensorbox(k,p,color='cyan'):
    multisensor(k,*p,kind='current',size=.29)
    for i in range(5):
        box(k,(p[0]-.22+i*.11,p[1]+.22,p[2]+.15),(.055,.03,.08),color)

def chamber(k,c,r=.95,length=1.65,axis='x',transparent=True):
    x,y,z=c
    cyl(k,c,r,length,'glass' if transparent else 'steel',axis,40)
    for off in (-length*.46,length*.46):
        pp=(x+off,y,z) if axis=='x' else (x,y+off,z)
        ring(k,pp,r*1.05,r*.95,.12,axis,'silver',40)
    for j in range(15):
        a=2*math.pi*j/15
        for off in (-length*.46,length*.46):
            if axis=='x':cyl(k,(x+off,y+r*math.cos(a),z+r*math.sin(a)),.034,.085,'steel','x',10)

def waferdisk(k,c,r=.83,mask=False):
    x,y,z=c
    cyl(k,c,r,.036,'iridescent' if not mask else 'purpleglass','y',48)
    ring(k,(x,y+.03,z),r,r*.966,.027,'y','silver',48)
    for i in range(4):
        for j in range(4):
            xx=x+(i-1.5)*r*.43;zz=z+(j-1.5)*r*.43
            if (xx-x)**2+(zz-z)**2 < (r*.85)**2:
                box(k,(xx,y+.028,zz),(.24,.011,.24),'violet' if mask else ('blue' if (i+j)%2 else 'cyan'))
                for n in range(3):box(k,(xx-.084+n*.082,y+.040,zz-.078+n*.05),(.039,.009,.045),'black' if mask else 'circuit')

def magnetframe(k,c,size=(1.25,1.7,1.30),cc='copper'):
    x,y,z=c
    frame(k,c,size,beam=.18,glass=False)
    for dy in np.linspace(-.6,.6,17):
        coilwrap(k,(x,y+dy*.06,z),r=.55+abs(dy)*.04,length=.30,turns=2,axis='x',col=cc)
    for yy in (y-.63,y+.63):box(k,(x,yy,z),(size[0]*.83,.17,size[2]*.85),'dark')

# 00 — rugged industrial 19.2-m platform, bolted guards, instrument cable trench.
k=names[0]
tier(k,0,.33,0,19.2,.62,9.65,'navy')
box(k,(0,.665,0),(18.82,.14,9.25),'dark')
for z in (-4.54,4.54):
    box(k,(0,.77,z),(18.94,.18,.19),'steel')
    lightstrip(k,(0,.867,z),(18.6,.025,.032),'cyan')
for x in np.linspace(-9.24,9.24,52):
    screw(k,(x,.85,4.30),.035)
for xx in (-9.25,9.25):
    box(k,(xx,.84,0),(.18,.16,9.0),'silver')
    for zz in np.linspace(-4.30,4.3,19):screw(k,(xx,.92,zz),.040)
for x in np.linspace(-8.75,8.75,9):
    for z in (-3.95,3.95):box(k,(x,-.095,z),(.83,.20,.53),'steel')
for x in (-8,-4.5,-1.4,2.2,5.5,8.1):
    box(k,(x,.80,3.72),(1.32,.10,.64),'steel')
    terminal_block(k,(x,.85,4.05),9,accent='cyan')

# 01 — long transparent segmented vacuum beamline enclosure.
k=names[1]
frame(k,(-2.15,3.07,-.64),(13.45,3.68,3.50),.12,glass=True)
for x in np.linspace(-8.50,4.15,11):
    box(k,(x,1.65,-2.40),(.11,.75,.09),'light')
    box(k,(x,5.04,-2.40),(.11,.14,3.24),'steel')
    lightstrip(k,(x,4.78,1.03),(.04,.36,.035),'cyan')
for zz in (-2.33,1.04):
    lightstrip(k,(-2.15,4.91,zz),(13.15,.032,.04),'cyan')

# 02 — precision source instrument housing, bolted endcaps and extraction frame.
k=names[2]
register(2,'ANIM_ION_SOURCE',(-7.65,2.93,-.65),'visual','x')
tier(k,-7.55,1.61,-.65,2.14,.28,2.16,'navy')
with moving('ANIM_ION_SOURCE'):
    chamber(k,(-7.59,2.90,-.65),.73,1.40,'x')
    coaxcoil(k,-7.58,2.9,-.65,.82,'x',12)
    cyl(k,(-7.05,2.90,-.65),.37,.25,'dark','x',26)
    for j in range(12):
        a=2*math.pi*j/12
        box(k,(-7.60,2.9+.76*math.sin(a),-.65+.76*math.cos(a)),(1.30,.05,.048),'silver')
for x in (-8.25,-6.85):stand(k,x,1.15,-.65,1.20)

# 03 — sealed virtual dopant cartridge.
k=names[3]
register(3,'ANIM_SOURCE_CARTRIDGE',(-8.52,1.63,1.40),'translation','y')
with moving('ANIM_SOURCE_CARTRIDGE'):
    cylvessel(k,-8.52,1.63,1.40,.34,1.36,'violet')
    for yy in (1.19,1.77,2.05):ring(k,(-8.52,yy,1.40),.38,.335,.040,'y','silver')
    screen(k,(-8.52,1.61,1.79),.38,.24,'violet')
cable(k,[(-8.51,2.29,1.40),(-8.51,2.5,.35),(-7.65,2.7,.08)],'copper',.04)

# 04 — ion source plasma vessel.
k=names[4]
register(4,'ANIM_SOURCE_LID',(-8.0,3.38,-.65),'rotation','z')
chamber(k,(-7.66,2.90,-.65),.48,1.21,'x')
with moving('ANIM_SOURCE_LID'):
    cyl(k,(-8.0,3.38,-.65),.43,.19,'silver','y',24)
    ring(k,(-8.0,3.48,-.65),.42,.30,.04,'y','cyan')
for j in range(26):
    a=j*2*math.pi/26
    box(k,(-7.67,2.9+.48*math.cos(a),-.65+.48*math.sin(a)),(.84,.026,.024),'violet' if j%3 else 'copper')

# 05 — source emission control module and illuminated plasma rings.
k=names[5]
register(5,'ANIM_SOURCE_EMISSION',(-6.93,2.90,-.65),'visual','x')
controlbox(k,(-8.55,2.18,2.63),(.91,.74,.52),'steel','violet')
with moving('ANIM_SOURCE_EMISSION'):
    for x in np.linspace(-7.14,-6.63,9):ring(k,(x,2.9,-.65),.275,.17,.055,'x','violet',26)
    cyl(k,(-6.89,2.9,-.65),.15,.58,'plasma','x',24)
for j in range(8):lightstrip(k,(-8.85+j*.086,2.59,2.95),(.042,.02,.03),'violet')

# 06 — extraction electrode stack with ceramic insulation.
k=names[6]
register(6,'ANIM_EXTRACTION',(-6.47,2.90,-.65),'translation','x')
with moving('ANIM_EXTRACTION'):
    for x in np.linspace(-6.69,-6.20,7):
        ring(k,(x,2.90,-.65),.42,.19,.076,'x','ceramic' if int(x*100)%2 else 'silver',32)
    ringshaft(k,(-6.43,2.90,-.65),.58,.19,'x','glass','silver',4)
for x in (-6.74,-6.15):
    box(k,(x,2.11,-.65),(.13,1.24,.20),'steel')

# 07 — HV-looking virtual power cabinet (art only).
k=names[7]
hd_cabinet(k,(-8.05,1.60,-3.53),(1.64,1.74,1.08),'amber',2)
for i in range(20):
    x=-8.63+(i%5)*.30;y=2.40+(i//5)*.07
    box(k,(x,y,-3.56),(.15,.035,.74),'ceramic' if i%2 else 'copper')
for i in range(6):
    cable(k,[(-8.05+i*.09,2.49,-3.5),(-7.5+i*.06,2.67,-2.52),(-7.24,2.74,-1.18)],'copper',.018)

# 08 — heroic arced mass analyzer with magnet yokes, copper coil winding, beam-bend conduit.
k=names[8]
register(8,'ANIM_ANALYZER_FIELD',(-4.95,2.91,-.65),'visual','y')
for i in range(18):
    t=i/17;a=math.pi*(.80+t*.90)
    cx=-5.19+1.32*math.cos(a);cz=-.65+1.32*math.sin(a)
    cyl(k,(cx,2.91,cz),.45,.23,'dark','y',24)
    ring(k,(cx,3.09,cz),.49,.38,.035,'y','copper',24)
for i in range(15):
    a=2*math.pi*i/15
    x=-5.00+.71*math.cos(a);z=-.65+.74*math.sin(a)
    coilwrap(k,(x,2.92,z),.30,.42,3,'x','copper')
with moving('ANIM_ANALYZER_FIELD'):
    for i in range(31):
        t=i/30;a=math.pi*(.80+t*.89)
        cx=-5.08+1.05*math.cos(a);cz=-.65+1.05*math.sin(a)
        if i>0:
            a0=math.pi*(.80+(i-1)/30*.89)
            px=-5.08+1.05*math.cos(a0);pz=-.65+1.05*math.sin(a0)
            pipe(k,[(px,2.90,pz),(cx,2.90,cz)],.11,'glass',12)
for x in (-5.98,-4.09):
    box(k,(x,1.83,-.65),(.25,1.05,1.29),'steel')
    box(k,(x,2.36,-.65),(.60,.10,1.35),'dark')

# 09 — precise species aperture.
k=names[9]
register(9,'ANIM_MASS_APERTURE',(-3.65,2.90,-.65),'translation','y')
box(k,(-3.65,2.9,-.65),(.28,1.17,1.08),'steel')
with moving('ANIM_MASS_APERTURE'):
    for yy in (2.62,3.18):box(k,(-3.48,yy,-.65),(.16,.23,1.01),'dark')
    ring(k,(-3.48,2.90,-.65),.39,.10,.12,'x','silver',32)
for j in range(16):
    yy=2.37+j*.069
    box(k,(-3.79,yy,-.22),(.10,.035,.06),'copper' if j%4 else 'green')

# 10 — beam energy selector and shielded stages.
k=names[10]
register(10,'ANIM_ENERGY_SELECTOR',(-2.95,2.91,-.65),'rotation','x')
beamsegment(k,-3.38,-2.44,r=.23)
with moving('ANIM_ENERGY_SELECTOR'):
    for xx in (-3.19,-2.96,-2.73):ring(k,(xx,2.91,-.65),.48,.27,.095,'x','copper',30)
for xx in (-3.24,-2.69):
    box(k,(xx,2.92,-.65),(.18,1.22,.20),'dark')

# 11 — lengthy ceramic-banded acceleration column.
k=names[11]
register(11,'ANIM_ACCELERATION_COLUMN',(-1.59,2.91,-.65),'visual','x')
beamsegment(k,-2.41,-.56,r=.245)
with moving('ANIM_ACCELERATION_COLUMN'):
    for xx in np.linspace(-2.24,-.69,15):
        ring(k,(xx,2.91,-.65),.42,.285,.067,'x','ceramic' if int(xx*100)%2 else 'copper',32)
for zz in (-1.18,-.12):
    box(k,(-1.50,2.90,zz),(1.90,.14,.14),'silver')
for x in (-2.23,-.71):stand(k,x,1.27,-.65,1.28)

# 12 — neutral particle separator assembly.
k=names[12]
beamsegment(k,-.54,.28,r=.235)
box(k,(-.13,2.91,-.65),(.82,.94,.85),'navy')
for yy in (2.52,3.30):
    box(k,(-.15,yy,-.65),(.79,.16,.73),'copper')
for xx in (-.45,.18):flangeX(k,xx,2.91,-.65,.39)
cable(k,[(-.06,2.91,-.42),(-.16,2.91,.28),(.35,1.9,.66)],'amber',.045)

# 13 — quadrupole focusing lens with long split magnetic coils and fin stacks.
k=names[13]
register(13,'ANIM_QUAD_LENS_A',(.76,2.91,-.65),'visual','x')
register(13,'ANIM_QUAD_LENS_B',(1.27,2.91,-.65),'visual','x')
beamsegment(k,.32,1.66,r=.23)
for idx,(name,cx) in enumerate((('ANIM_QUAD_LENS_A',.76),('ANIM_QUAD_LENS_B',1.27))):
    with moving(name):
        for j in range(4):
            a=j*math.pi/2
            yy=2.91+.44*math.sin(a);zz=-.65+.44*math.cos(a)
            coilwrap(k,(cx,yy,zz),.15,.38,9,'x','copper')
            box(k,(cx,yy,zz),(.45,.16,.18),'dark')
        for xx in (cx-.22,cx+.22):flangeX(k,xx,2.91,-.65,.40)

# 14 — steer coils with independent X and Y nested groups.
k=names[14]
register(14,'ANIM_BEAM_STEER_X',(1.96,2.91,-.65),'rotation','y')
register(14,'ANIM_BEAM_STEER_Y',(2.21,2.91,-.65),'rotation','z')
beamsegment(k,1.72,2.52,r=.23)
with moving('ANIM_BEAM_STEER_X'):
    for yy in (2.47,3.35):box(k,(1.96,yy,-.65),(.42,.18,.82),'copper')
with moving('ANIM_BEAM_STEER_Y'):
    for zz in (-1.08,-.22):box(k,(2.21,2.91,zz),(.43,.90,.18),'copper')

# 15 — electromagnetic X/Y scanner with arrays and beam diagnostics.
k=names[15]
register(15,'ANIM_BEAM_SCAN_X',(2.87,2.91,-.65),'rotation','y')
register(15,'ANIM_BEAM_SCAN_Y',(3.39,2.91,-.65),'rotation','z')
beamsegment(k,2.61,3.80,r=.24)
with moving('ANIM_BEAM_SCAN_X'):
    for xx in (2.74,2.96):
        for zz in (-1.14,-.16):box(k,(xx,2.91,zz),(.19,1.17,.22),'copper')
with moving('ANIM_BEAM_SCAN_Y'):
    for yy in (2.39,3.43):box(k,(3.39,yy,-.65),(.41,.19,1.10),'copper')
for xx in np.linspace(2.70,3.70,12):
    box(k,(xx,3.56,-.66),(.036,.11,.72),'steel')

# 16 — emergency fast beam blanker gate.
k=names[16]
register(16,'ANIM_BEAM_BLANKER',(4.03,2.91,-.65),'translation','y')
mechbox(k,(4.03,2.91,-.65),(.65,1.08,.85),'dark')
with moving('ANIM_BEAM_BLANKER'):
    box(k,(4.05,2.91,-.65),(.14,.82,.72),'steel')
    box(k,(4.18,2.91,-.65),(.08,.43,.40),'red')
for x in (3.73,4.33):flangeX(k,x,2.91,-.65,.39)

# 17 — primary current transformer readout.
k=names[17]
beamsegment(k,4.38,4.91,r=.23)
for xx in (4.50,4.72,4.84):ring(k,(xx,2.91,-.65),.47,.24,.09,'x','copper',32)
sensorbox(k,(4.62,2.00,1.72),'cyan')
cable(k,[(4.62,2.9,-.22),(4.62,2.45,.80),(4.62,2.14,1.65)],'cyan',.028)

# 18 — central Faraday cup on retractable arm, final critical feedback wire target.
k=names[18]
register(18,'ANIM_FARADAY_CUP',(5.19,2.81,-.65),'translation','y')
with moving('ANIM_FARADAY_CUP'):
    cyl(k,(5.19,2.81,-.65),.25,.47,'dark','x',32)
    ring(k,(4.94,2.81,-.65),.28,.21,.09,'x','copper',32)
    cyl(k,(5.46,2.81,-.65),.30,.10,'steel','x',28)
    for j in range(12):
        a=2*math.pi*j/12
        box(k,(5.08,2.81+.20*math.sin(a),-.65+.20*math.cos(a)),(.24,.026,.027),'silver')
box(k,(5.20,2.03,-.65),(.26,1.28,.22),'steel')
sensorbox(k,(5.0,1.49,1.66),'amber')
cable(k,[(5.20,2.05,-.65),(5.30,1.58,.42),(5.00,1.42,1.43)],'copper',.03)

# 19 — four-corner diagnostic cups around wafer.
k=names[19]
register(19,'ANIM_CORNER_CUPS',(6.92,2.70,-.65),'visual','y')
with moving('ANIM_CORNER_CUPS'):
    for xx in (6.16,7.68):
        for zz in (-1.47,.17):
            cyl(k,(xx,2.65,zz),.17,.21,'steel','y',24)
            ring(k,(xx,2.78,zz),.17,.12,.045,'y','copper')
            box(k,(xx,2.40,zz),(.18,.45,.18),'silver')
            for j in range(4):box(k,(xx-.12+j*.08,2.78,zz+.11),(.032,.012,.038),'cyan')

# 20 — wafer charge neutralization plates.
k=names[20]
register(20,'ANIM_NEUTRALIZER',(6.90,3.39,-.65),'visual','y')
with moving('ANIM_NEUTRALIZER'):
    for xx in (6.17,7.60):
        box(k,(xx,3.31,-.65),(.13,.61,1.62),'steel')
        for zz in np.linspace(-1.27,-.03,13):box(k,(xx,3.38,zz),(.13,.36,.036),'violet' if int(zz*10)%3 else 'copper')
    box(k,(6.90,3.79,-.65),(1.75,.12,1.53),'dark')

# 21 — imposing transparent wafer vacuum endstation.
k=names[21]
register(21,'ANIM_ENDSTATION_DOOR',(7.80,2.96,-.65),'rotation','y')
frame(k,(6.86,2.68,-.65),(3.35,3.25,3.56),.14,glass=True)
chamber(k,(6.89,2.78,-.65),1.05,2.56,'x')
for xx in (5.45,8.34):flangeX(k,xx,2.78,-.65,1.15)
with moving('ANIM_ENDSTATION_DOOR'):
    ring(k,(8.25,2.86,-.65),1.17,.88,.15,'x','silver',42)
    cyl(k,(8.34,2.86,-.65),.91,.035,'glass','x',38)
for zz in (-1.98,.68):lightstrip(k,(6.86,4.33,zz),(2.95,.025,.035),'cyan')

# 22 — wafer electrostatic chuck.
k=names[22]
register(22,'ANIM_WAFER_CHUCK',(6.95,2.19,-.65),'translation','y')
with moving('ANIM_WAFER_CHUCK'):
    cyl(k,(6.95,2.03,-.65),.91,.29,'navy','y',44)
    ring(k,(6.95,2.20,-.65),.95,.82,.058,'y','ceramic',44)
    cyl(k,(6.95,2.25,-.65),.82,.06,'ceramic','y',44)
    for i in range(16):
        a=2*math.pi*i/16
        box(k,(6.95+.76*math.cos(a),2.29,-.65+.76*math.sin(a)),(.075,.03,.12),'silver')

# 23 — wafer tilt+rotary stage.
k=names[23]
register(23,'ANIM_WAFER_TILT',(6.94,1.93,-.65),'rotation','x')
register(23,'ANIM_WAFER_ROTATION',(6.94,2.25,-.65),'rotation','y')
with moving('ANIM_WAFER_TILT'):
    cyl(k,(6.94,1.76,-.65),.62,.52,'steel','y',38)
    box(k,(6.94,1.43,-.65),(1.74,.18,1.74),'dark')
with moving('ANIM_WAFER_ROTATION'):
    ring(k,(6.94,2.30,-.65),.88,.82,.045,'y','copper',42)
    for j in range(20):
        a=2*math.pi*j/20
        box(k,(6.94+.77*math.cos(a),2.34,-.65+.77*math.sin(a)),(.035,.033,.09),'light')

# 24 — patterned silicon wafer with 4x4 fields.
k=names[24]
waferdisk(k,(6.94,2.32,-.65),.80,False)
for j in range(6):
    a=j*math.pi/3
    cyl(k,(6.94+.77*math.cos(a),2.357,-.65+.77*math.sin(a)),.032,.013,'gold','y',10)

# 25 — developed implant mask with real holes/gaps represented by segmented tiles.
k=names[25]
cyl(k,(6.94,2.365,-.65),.81,.011,'purpleglass','y',44)
for i in range(5):
    for j in range(5):
        xx=6.94+(i-2)*.23;zz=-.65+(j-2)*.23
        if (xx-6.94)**2+(zz+.65)**2>.60:continue
        if (i+j)%3!=0:
            box(k,(xx,2.381,zz),(.145,.017,.145),'violet')
            for m in range(3):box(k,(xx-.044+m*.044,2.394,zz),(.019,.006,.082),'dark')

# 26 — holographic implanted subsurface regions.
k=names[26]
register(26,'ANIM_DOPANT_REGIONS',(6.94,2.40,-.65),'visual','y')
with moving('ANIM_DOPANT_REGIONS'):
    for i in range(5):
        for j in range(5):
            xx=6.94+(i-2)*.24;zz=-.65+(j-2)*.24
            if (xx-6.94)**2+(zz+.65)**2>.60 or (i+j)%3==0:continue
            cyl(k,(xx,2.385,zz),.075,.015,'gold','y',12)
            cyl(k,(xx,2.402,zz),.041,.026,'cyan','y',10)

# 27 — large floating depth-profile graph at endstation.
k=names[27]
register(27,'ANIM_DOPANT_DEPTH_HOLOGRAM',(7.06,3.93,1.65),'visual','x')
with moving('ANIM_DOPANT_DEPTH_HOLOGRAM'):
    frame(k,(7.06,3.94,1.66),(2.62,1.28,.10),.036,glass=False)
    box(k,(7.06,3.94,1.70),(2.42,1.08,.018),'purpleglass')
    pts=[]
    for i in range(51):
        x=5.98+i*(2.16/50)
        yy=3.58+.86*math.exp(-((i-26)/13)**2)
        pts.append((x,yy,1.74))
    pipe(k,pts,.021,'gold',8)
    for i in range(15):
        xx=5.99+i*.151
        box(k,(xx,3.49,1.75),(.018,.11,.012),'cyan')

# 28 — 3-joint wafer robot.
k=names[28]
register(28,'ANIM_WAFER_ROBOT_BASE',(4.70,1.16,-3.33),'rotation','y')
register(28,'ANIM_WAFER_ROBOT_ARM',(5.08,1.92,-3.33),'rotation','z')
register(28,'ANIM_WAFER_ROBOT_TOOL',(5.92,2.07,-3.33),'rotation','y')
tier(k,4.70,1.05,-3.33,1.45,.33,1.35,'navy')
with moving('ANIM_WAFER_ROBOT_BASE'):
    cyl(k,(4.70,1.52,-3.33),.34,.75,'steel','y',32)
    ring(k,(4.70,1.91,-3.33),.37,.25,.07,'y','copper')
with moving('ANIM_WAFER_ROBOT_ARM'):
    angular_arm(k,(5.08,1.92,-3.33),(5.92,2.07,-3.33),.16,'silver')
    cyl(k,(5.08,1.92,-3.33),.26,.25,'dark','z',24)
with moving('ANIM_WAFER_ROBOT_TOOL'):
    box(k,(6.10,2.04,-3.33),(.77,.15,.50),'steel')
    for dz in (-.34,.34):box(k,(6.47,2.04,-3.33+dz),(.15,.10,.12),'silver')

# 29 — wafer carrier cassette with slotted shelves.
k=names[29]
mechbox(k,(3.68,1.53,-3.44),(1.32,1.39,1.37),'navy')
for yy in np.linspace(1.04,2.03,10):
    box(k,(3.68,yy,-3.44),(1.13,.032,1.23),'silver')
    cyl(k,(3.68,yy+.037,-3.44),.46,.012,'iridescent','y',28)
for xx in (3.13,4.23):box(k,(xx,1.65,-2.72),(.08,1.21,.11),'cyan')

# 30 — vacuum wafer loadlock.
k=names[30]
chamber(k,(5.28,1.48,-2.65),.66,1.56,'x')
for x in (4.45,6.10):flangeX(k,x,1.48,-2.65,.73)
for j in range(9):
    x=4.67+j*.15
    box(k,(x,.96,-2.65),(.06,.12,1.13),'steel')

# 31 — retractable loadlock gate valve.
k=names[31]
register(31,'ANIM_LOADLOCK_GATE',(5.85,1.51,-2.65),'translation','y')
mechbox(k,(5.85,1.51,-2.65),(.46,1.22,1.37),'steel')
with moving('ANIM_LOADLOCK_GATE'):
    box(k,(5.85,1.51,-2.65),(.12,.97,1.15),'dark')
    ring(k,(5.88,1.51,-2.65),.37,.19,.06,'x','silver')

# 32 — large turbomolecular vacuum pump.
k=names[32]
register(32,'ANIM_TURBO_ROTOR',(3.62,1.42,-1.75),'rotation','y')
cylvessel(k,3.62,1.48,-1.75,.45,1.28,'cyan')
with moving('ANIM_TURBO_ROTOR'):
    for yy in np.linspace(1.10,1.82,7):
        ring(k,(3.62,yy,-1.75),.35,.10,.032,'y','silver')
        for j in range(11):
            a=j*2*math.pi/11
            box(k,(3.62+.20*math.cos(a),yy+.03,-1.75+.20*math.sin(a)),(.09,.02,.035),'blue')

# 33 — backing pump.
k=names[33]
register(33,'ANIM_BACKING_ROTOR',(2.83,1.28,-3.09),'rotation','x')
motor(k,(2.83,1.30,-3.09),.73)
with moving('ANIM_BACKING_ROTOR'):
    cyl(k,(3.43,1.30,-3.09),.27,.22,'silver','x',20)
    for j in range(8):
        a=j*2*math.pi/8
        box(k,(3.56,1.30+.14*math.sin(a),-3.09+.14*math.cos(a)),(.10,.048,.044),'copper')

# 34 — auto vacuum throttle valve.
k=names[34]
register(34,'ANIM_THROTTLE_VALVE',(4.0,1.53,-2.81),'rotation','y')
for x in (3.66,4.34):flangeX(k,x,1.53,-2.81,.39)
mechbox(k,(4.0,1.53,-2.81),(.57,.52,.64),'steel')
with moving('ANIM_THROTTLE_VALVE'):
    cyl(k,(4.0,1.93,-2.81),.25,.18,'dark','y',20)
    box(k,(4.0,2.04,-2.81),(.47,.10,.08),'copper')

# 35 — pressure gauge.
k=names[35]
sensorbox(k,(4.72,1.46,1.36),'cyan')
gauge(k,(4.72,1.44,1.62),.15)
cable(k,[(4.72,1.32,1.30),(4.72,1.23,.35),(5.5,1.49,-.80)],'copper',.023)

# 36 — residual gas diagnostic module.
k=names[36]
hd_cabinet(k,(3.59,1.42,2.45),(1.10,1.17,.85),'violet')
for j in range(12):
    box(k,(3.23+j*.064,1.83,2.91),(.04,.035,.035),'cyan' if j%3 else 'violet')

# 37 — wafer contact temp sensor.
k=names[37]
sensorbox(k,(8.51,1.77,-1.22),'amber')
cable(k,[(8.51,1.54,-1.22),(8.12,1.48,-1.40),(7.66,2.12,-1.25)],'copper',.02)

# 38 — heat extraction / wafer cooling module.
k=names[38]
mechbox(k,(8.12,1.22,-3.38),(1.54,.79,1.16),'navy')
for j in range(19):box(k,(7.45+j*.075,1.70,-3.41),(.039,.21,.91),'silver')
for zz in (-3.69,-3.07):
    duct(k,[(8.09,1.26,zz),(7.75,1.66,zz),(7.31,2.01,-1.43)],.055,'blue')

# 39 — isolated beam dump.
k=names[39]
mechbox(k,(3.64,1.40,-3.84),(1.25,.86,1.04),'dark')
for j in range(14):
    x=3.12+j*.079
    box(k,(x,1.93,-3.84),(.05,.15,.78),'copper')
for i in range(6):
    box(k,(3.65,1.36,-3.30+i*.04),(.87,.05,.015),'red' if i%2 else 'amber')

# 40 — beamline safety interlock relay.
k=names[40]
sideconsole(k,1.29,3.40,'red',1.00)
for j in range(8):
    box(k,(.94+j*.1,1.33,3.92),(.071,.12,.052),'amber' if j%3 else 'red')

# 41 — traversing beam profiler.
k=names[41]
register(41,'ANIM_BEAM_PROFILE_SCAN',(4.51,2.72,-.65),'translation','y')
with moving('ANIM_BEAM_PROFILE_SCAN'):
    ring(k,(4.51,2.72,-.65),.41,.29,.061,'x','cyan',32)
    box(k,(4.51,2.72,-.65),(.05,.68,.07),'silver')
    box(k,(4.51,2.72,-.65),(.05,.07,.71),'silver')
box(k,(4.51,2.28,-.65),(.17,1.17,.15),'steel')

# 42 — purity analyzer.
k=names[42]
sideconsole(k,-3.22,3.37,'violet',.91)
for j in range(15):
    box(k,(-3.62+j*.054,1.40,3.83),(.034,.06,.06),'green' if j<13 else 'red')

# 43 — dose controller.
k=names[43]
sideconsole(k,2.45,3.42,'cyan',1.05)
for j in range(18):
    box(k,(2.06+j*.043,1.50,3.83),(.023,.015,.052),'gold' if j%3 else 'green')

# 44 — dose-uniformity heatmap analyzer.
k=names[44]
controlbox(k,(3.94,2.54,3.20),(1.40,1.03,.17),'navy','blue',2)
for i in range(4):
    for j in range(4):
        box(k,(3.52+i*.27,2.34+j*.18,3.33),(.19,.13,.015),'green' if (i+j)%5 else 'amber')

# 45 — wafer surface potential sensing.
k=names[45]
sensorbox(k,(8.53,2.47,.08),'violet')
cable(k,[(8.35,2.47,-.15),(7.82,2.64,-.42)],'cyan',.014)

# 46 — crystal damage analyzer.
k=names[46]
controlbox(k,(5.04,2.50,3.22),(1.22,1.07,.20),'navy','amber',2)
for j in range(23):
    x=4.58+j*.04
    yy=2.18+.25*math.sin(j*.34)+.10*math.sin(j*.84)
    box(k,(x,yy,3.34),(.022,.017,.014),'amber')

# 47 — virtual activation chamber near wafer endstation.
k=names[47]
register(47,'ANIM_ACTIVATION_DOOR',(8.12,1.78,2.20),'rotation','y')
chamber(k,(7.80,1.68,2.32),.68,1.29,'x')
with moving('ANIM_ACTIVATION_DOOR'):
    ring(k,(8.40,1.74,2.32),.74,.54,.12,'x','steel')
    cyl(k,(8.47,1.74,2.32),.53,.038,'glass','x',28)
for yy in (1.25,2.15):
    box(k,(7.81,yy,2.32),(1.18,.10,.90),'dark')

# 48 — multi-zone activation heater.
k=names[48]
register(48,'ANIM_ACTIVATION_HEAT',(7.80,1.68,2.32),'visual','x')
with moving('ANIM_ACTIVATION_HEAT'):
    for xx in np.linspace(7.34,8.27,10):
        ring(k,(xx,1.70,2.32),.49,.39,.044,'x','copper',28)
        for j in range(6):
            aa=j*math.pi/3
            box(k,(xx,1.70+.40*math.sin(aa),2.32+.40*math.cos(aa)),(.08,.035,.04),'amber')

# 49 — activation temp sensor.
k=names[49]
sensorbox(k,(8.70,2.60,2.94),'amber')
pipe(k,[(8.70,2.23,2.94),(8.41,2.10,2.55)],.024,'copper')

# 50 — sheet resistance wafer probe, articulated contact pins.
k=names[50]
controlbox(k,(5.93,1.52,2.52),(1.34,.89,1.06),'steel','green')
for j in range(4):
    x=5.62+j*.21
    pipe(k,[(x,2.06,2.28),(x,2.25,2.18),(x,2.13,2.00)],.025,'silver')
    cyl(k,(x,2.11,2.00),.023,.14,'gold','y',10)

# 51 — metrology scanner for depth profile.
k=names[51]
register(51,'ANIM_JUNCTION_METROLOGY',(6.74,2.67,3.23),'translation','x')
mechbox(k,(6.76,1.50,3.26),(1.30,.65,.94),'navy')
with moving('ANIM_JUNCTION_METROLOGY'):
    box(k,(6.74,2.42,3.23),(1.12,.20,.18),'silver')
    cyl(k,(6.74,2.20,3.23),.19,.48,'dark','y',28)
    ring(k,(6.74,2.0,3.23),.21,.13,.048,'y','cyan')
for xx in (6.2,7.3):stand(k,xx,1.28,3.23,.98)

# 52 — registration camera.
k=names[52]
sensorbox(k,(8.72,3.14,-2.62),'blue')
cyl(k,(8.72,2.57,-2.62),.19,.45,'dark','y',26)
ring(k,(8.72,2.38,-2.62),.22,.15,.03,'y','cyan')

# 53 — large PLC rack with many IO cards.
k=names[53]
hd_cabinet(k,(-1.98,1.39,3.24),(1.85,1.32,.91),'cyan',2)
for j in range(16):
    x=-2.71+j*.095
    box(k,(x,1.44,3.75),(.075,.45,.058),'dark')
    for t in range(4):box(k,(x,1.22+t*.13,3.79),(.040,.025,.025),'green' if (j+t)%3 else 'amber')

# 54 — virtual beam power isolation controller.
k=names[54]
hd_cabinet(k,(-.35,1.40,3.30),(1.27,1.32,.91),'red',1)
for j in range(3):
    cyl(k,(-.70+j*.35,1.06,3.84),.11,.07,'dark','z',20)
    cyl(k,(-.70+j*.35,1.06,3.88),.073,.030,'red' if j==2 else 'amber','z',16)

# 55 — prominent emergency station.
k=names[55]
register(55,'ANIM_IMPLANT_ESTOP',(8.91,1.35,4.04),'translation','y')
hd_cabinet(k,(8.63,1.38,3.56),(1.02,1.20,.82),'red',1)
with moving('ANIM_IMPLANT_ESTOP'):
    cyl(k,(8.91,1.35,4.05),.165,.13,'red','z',30)
    ring(k,(8.91,1.35,4.14),.18,.14,.05,'z','amber',24)
for j in range(6):
    box(k,(8.28+j*.13,.98,4.03),(.063,.035,.03),'red' if j%2 else 'amber')

# 56 — huge standing command SCADA / dopant heatmap and profile charts.
k=names[56]
register(56,'ANIM_IMPLANT_SCADA',(0,4.17,-4.04),'visual','x')
for xx in (-2.45,2.45):
    box(k,(xx,3.14,-4.06),(.21,2.92,.20),'steel')
    for yy in np.linspace(1.80,4.33,14):box(k,(xx,yy,-3.92),(.06,.054,.05),'cyan')
with moving('ANIM_IMPLANT_SCADA'):
    box(k,(0,4.16,-4.06),(5.28,2.38,.19),'navy')
    box(k,(0,4.16,-3.945),(5.07,2.19,.023),'black')
    for ix in range(4):
        for iz in range(4):
            box(k,(-1.92+ix*.31,4.58-iz*.28,-3.92),(.23,.20,.015),'cyan' if (ix+iz)%5 else 'amber')
    for j in range(69):
        x=-.44+j*.059
        yy=4.08+.42*math.exp(-((j-35)/16)**2)
        box(k,(x,yy,-3.919),(.036,.023,.017),'gold')
    for j in range(53):
        x=-.4+j*.073
        yy=3.53+.16*math.sin(j*.22)+.11*math.sin(j*.37)
        box(k,(x,yy,-3.914),(.035,.018,.016),'teal')
    for j in range(28):
        box(k,(-2.36+j*.177,3.11,-3.922),(.10,.035,.02),'green' if j%7 else 'red')

# 57 — isolated 5 V educational supply.
k=names[57]
zpanel(k,(.68,1.22,3.48),1.08,.91,.70,'green',3)
for j,x in enumerate((.41,.68,.95)):
    cyl(k,(x,.75,3.91),.07,.13,'copper','z',16)
    cyl(k,(x,.75,4.0),.034,.04,'red' if j==0 else 'blue','z',10)
finbank(k,(.68,1.71,3.48),.81,16,.42,.20,'light')

# Validate nonempty individual modules and correct registered moving-parent meshes.
for idx,n in enumerate(names):
    assert n in M and M[n],(idx,n,'missing model')
    for mesh,color,grp in M[n]:
        assert len(mesh.faces)>0 and np.isfinite(mesh.vertices).all(),(idx,n,'bad mesh')
        assert grp is None or grp in PV.get(n,{}),(idx,n,'unregistered pivot',grp)
    actual={grp for _,_,grp in M[n] if grp}
    assert set(PV.get(n,{}))<=actual,(idx,n,'empty pivot',set(PV.get(n,{}))-actual)
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
            objects.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(.25*q for q in rgb)),N('P','SpecularColor','Color','','A',.58,.62,.67),N('P','Shininess','Number','','A',54.),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
            mats[color]=matid
        connections.append(N('C','OO',mats[color],model_id))
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 75 Ion Implantation Modular Procedural Geometry')]),
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
    filename=f'{i:02d}_L75_{key}'
    export_fbx(key,MD/(filename+'.fbx'))
    export_glb(key,GD/(filename+'.glb'))
    triangles=sum(len(m.faces) for m,_,_ in M[key]);parts=len(M[key])
    coords=np.concatenate([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L75_'+key,file=filename+'.fbx',glb=filename+'.glb',
         triangle_count=triangles,mesh_parts=parts,
         bbox_min=np.around(coords.min(axis=0),4).tolist(),bbox_max=np.around(coords.max(axis=0),4).tolist(),
         rig_groups=list(PV.get(key,{}).keys()),
         animation_pivots={g:list(info['position']) for g,info in PV.get(key,{}).items()}))
    print(f'[{i+1:02d}/58] {key:42s} {triangles:7,d} triangles {parts:4d} parts',flush=True)

assembled='Level75_Complete_ArtKit_Showcase'
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
            w.writerow(['L75_'+k,pivot,*info['position'],info['kind'],info['axis'],'Named pivot parent Null nodes in FBX and GLB','No baked animation; follow actual simulation'])
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
(ROOT/'Material_Palette.json').write_text(json.dumps({'level_id':'level_75',
 'description':'Procedural colored material regions; NOT production hand-painted PBR textures','materials':palette},indent=2))

wiring=[
 ('Educational 5 V control power and returns',10,'Control supplies, instrument buses, valid returns'),
 ('Ion source and virtual beam-power control',16,'Virtual particle source, extraction, energy and enable feedback'),
 ('Magnetic analyzer and species qualification',14,'Analyzer field/selector, resolving aperture, species analysis'),
 ('Beam focusing, steering and scanning',14,'Lenses, steering, X/Y scan, beam blanker'),
 ('Wafer endstation and positioning',12,'Wafer clamp, tilt, rotation, robotics, loadlock, mask alignment'),
 ('Dosimetry, current sensing and metrology',14,'Critical Faraday-cup feedback, primary beam current, dose maps, depth and resistance'),
 ('Vacuum, thermal and activation systems',10,'Pumps, throttle, pressure, wafer thermal and activation'),
 ('PLC, safety and SCADA coordination',16,'PLC permissions, interlocks, HMI control, events and fault status')]
assert sum(group[1] for group in wiring)==106
with (ROOT/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Purpose','Status'])
    for v in wiring:w.writerow([*v,'PLANNED_NOT_IMPLEMENTED'])

triangles=sum(m['triangle_count'] for m in manifest);parts=sum(m['mesh_parts'] for m in manifest)
pivots=sum(len(PV.get(k,{})) for k in names)
metadata={
    'level_id':'level_75','display_name':'Write the Silicon',
    'asset_kind':'Original procedural HD/AAA-inspired mesh geometry, not final artist-authored photorealistic PBR',
    'individual_models':58,'individual_glbs':58,
    'assembled_models':['Models/'+assembled+'.fbx','GLB/'+assembled+'.glb'],
    'individual_triangle_count':triangles,'individual_mesh_parts':parts,'animation_parent_pivot_groups':pivots,
    'planned_player_wires':106,'planned_unique_functional_terminals':164,
    'actual_player_wires_included':False,'functional_spark_terminals_included':False,
    'ion_implantation_physics_included':False,'six_authoritative_objectives_implemented':False,
    'fbx_units':'numeric centimeters; UnitScaleFactor=1; Unity must import equivalent to meters',
    'glb_units':'numeric meters','axes':'X width, Y up, Z front',
    'approx_workstation_width_m':19.2,'common_origin_for_all_individual_models':True,
    'rig_hierarchy':'Named animated parent Null nodes inside binary FBX and GLB, no baked motion clips',
    'educational_scope':'Fictional ion species, normalized fictional dose, virtual high-power, not real implantation operating instructions',
    'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(metadata,indent=2),encoding='utf-8')
print('EXPORTED: models',len(manifest),'triangles',triangles,'mesh_parts',parts,'pivots',pivots,flush=True)
