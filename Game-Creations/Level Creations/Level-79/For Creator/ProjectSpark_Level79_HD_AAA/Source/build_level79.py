#!/usr/bin/env python3
"""Project Spark Level 79: original vertical six-axis hybrid chiplet bonding, metrology, and 3D integration geometry, real binary FBX/GLB exports.
Maya-like Y-up common-origin art. Visual art only; this is NOT implemented electrical or chiplet-bonding gameplay.
"""
import os,math,csv,json,struct,zlib,zipfile,hashlib,time,random,contextlib
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level79_HD_AAA')
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
    top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Level 79 Vertical Precision Hybrid Bonding Original Modular Geometry')]),
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





# ORIGINAL LEVEL 79 modular 3D hybrid chiplet-bonding industrial art
names='''Industrial3DHybridBondingBase|TransparentVerticalBondingTower|BaseWaferCarrier|BondReadyInterconnectWafer|CopperDielectricLandingPadMap|PrecisionChipletTray|LogicChiplet|MemoryChiplet|IOChiplet|SensorChiplet|KnownGoodDieCertificateScanner|PrecisionWaferLoadLock|DieTransferAirCurtain|HighPrecisionWaferPreAligner|SixAxisHybridBondingGantry|GantryXLinearRail|GantryYCrossbeam|GantryZPrecisionElevator|HexapodFineAlignmentStage|NanoXPositioningActuator|NanoYPositioningActuator|ThetaMicroRotationStage|TipTiltCompensationPlatform|UpperDieElectrostaticChuck|ChuckReleaseConditionMonitor|BaseWaferVacuumChuck|BondHeadMicroforceArray|FiducialOpticalCameraA|FiducialOpticalCameraB|ThroughSiliconInfraredCamera|LaserDisplacementInterferometer|MoireOverlayProjectionSystem|WaferBowInterferometryScanner|DarkfieldParticleInspectionSystem|AtomicScaleSurfaceFlatnessProfiler|SurfaceEnergyInspectionProbe|DrySurfaceCleaningModule|LowDamageSurfaceActivationModule|TransparentBondChamberWindow|ControlledContactInitiationHead|BondFrontOpticalImager|HolographicBondWaveCrossSection|InterfaceGapSensorArray|BondThermalConsolidationChamber|MultiZoneBondHeater|BondTemperatureSensor|DifferentialExpansionAnalyzer|BondInterfaceStressMonitor|ScanningAcousticVoidInspector|InfraredBondVoidTomography|BondSeamInspectionCamera|BondAdhesionWitnessCouponTester|CrossDieKelvinResistanceProbe|VerticalDaisyChainTester|ChipletInterconnectRoutingTester|HighSpeedBERJitterAnalyzer|ThermalCycleQualificationBay|StackWarpageMetrologyScanner|ChipletRejectQuarantineBin|ESDGuardedTransferStation|HybridBondingPLCRack|PrecisionAnalogIOModule|CriticalMotionSafetyStation|Holographic3DStackSCADA|Regulated5VControlSupply|Completed3DChipStackHologram'''.split('|')
assert len(names)==66
PV={}; site=(0,2.18,-.80)
# index, pivot, motion kind, local axis, position (all metres). 48 real hierarchical parent nodes.
pivots=[
(5,'ANIM_CHIPLET_TRAY','translation','x',(-8.45,1.90,1.05)),
(11,'ANIM_LOADLOCK_DOOR','translation','z',(-9.08,2.60,-1.60)),
(12,'ANIM_AIR_CURTAIN','indicator','none',(-7.85,3.04,-1.15)),
(13,'ANIM_BASE_WAFER_ALIGNER','rotation','y',(-7.15,2.05,1.60)),
(14,'ANIM_GANTRY_X','translation','x',(0,7.00,-.80)),
(16,'ANIM_GANTRY_Y','translation','z',(0,6.76,-.80)),
(17,'ANIM_GANTRY_Z','translation','y',(0,5.53,-.80)),
(23,'ANIM_GRIPPER_SLIDE','translation','y',(0,4.55,-.80)),
(23,'ANIM_GRIPPER_ROTATION','rotation','y',(0,4.42,-.80)),
(23,'ANIM_UPPER_CHUCK_CLAMP','translation','x',(0,4.25,-.80)),
(25,'ANIM_LOWER_CHUCK_THETA','rotation','y',(0,2.20,-.80)),
(25,'ANIM_LOWER_WAFER_LIFT','translation','y',(0,2.00,-.80)),
(18,'ANIM_FINE_HEXAPOD','translation','y',(0,5.08,-.80)),
(19,'ANIM_NANO_X','translation','x',(-.75,5.04,-.80)),
(20,'ANIM_NANO_Y','translation','z',(.75,5.04,-.80)),
(21,'ANIM_MICRO_THETA','rotation','y',(0,4.94,-.80)),
(22,'ANIM_TIP_AXIS','rotation','x',(-.48,4.82,-.80)),
(22,'ANIM_TILT_AXIS','rotation','z',(.48,4.82,-.80)),
(27,'ANIM_CAMERA_A','translation','x',(-2.35,4.28,1.08)),
(28,'ANIM_CAMERA_B','translation','x',(2.35,4.28,1.08)),
(29,'ANIM_INFRARED_LENS','translation','y',(-1.80,5.35,-2.30)),
(30,'ANIM_INTERFEROMETER_HEAD','translation','y',(1.90,5.35,-2.30)),
(31,'ANIM_MOIRE_OVERLAY','indicator','none',(0,5.87,1.70)),
(32,'ANIM_WARPAGE_SCANNER','translation','x',(-4.75,3.20,3.30)),
(33,'ANIM_PARTICLE_SCANNER','translation','x',(-6.30,3.05,3.30)),
(37,'ANIM_SURFACE_ACTIVATION','translation','y',(-8.58,2.54,3.60)),
(36,'ANIM_DRY_CLEAN_HEAD','translation','y',(-9.70,2.50,3.60)),
(38,'ANIM_BOND_CHAMBER_LID','translation','y',(0,6.25,-.80)),
(39,'ANIM_CONTACT_INITIATOR','translation','y',(0,3.64,-.80)),
(26,'ANIM_MICROFORCE_ARRAY','translation','y',(0,4.04,-.80)),
(41,'ANIM_BOND_WAVE_HOLOGRAM','indicator','none',(0,5.00,3.85)),
(42,'ANIM_INTERFACE_GAP_SCANNER','translation','x',(1.20,3.33,-.80)),
(43,'ANIM_ANNEAL_CHAMBER_DOOR','translation','z',(3.45,2.58,-2.42)),
(44,'ANIM_UPPER_HEATER','translation','y',(4.45,3.20,-2.90)),
(44,'ANIM_LOWER_HEATER','translation','y',(4.45,1.70,-2.90)),
(46,'ANIM_THERMAL_EXPANSION_SCAN','translation','x',(5.75,3.80,-2.90)),
(48,'ANIM_ACOUSTIC_SCAN_X','translation','x',(6.25,3.18,1.75)),
(48,'ANIM_ACOUSTIC_SCAN_Y','translation','z',(6.25,3.08,1.75)),
(49,'ANIM_IR_TOMOGRAPHY','rotation','y',(7.75,3.28,1.75)),
(50,'ANIM_BOND_SEAM_CAMERA','translation','x',(9.15,3.14,1.75)),
(51,'ANIM_ADHESION_COUPON_TEST','translation','y',(6.10,2.70,3.78)),
(52,'ANIM_KELVIN_PROBE','translation','y',(7.32,2.70,3.78)),
(53,'ANIM_DAISY_CHAIN_PROBE','translation','y',(8.55,2.70,3.78)),
(55,'ANIM_HIGH_SPEED_TEST_HEAD','translation','y',(9.72,2.72,3.78)),
(56,'ANIM_THERMAL_CYCLING_TRAY','translation','z',(9.0,1.78,-2.91)),
(57,'ANIM_STACK_WARPAGE_SCAN','translation','x',(7.20,4.25,-2.80)),
(62,'ANIM_EMERGENCY_STOP','translation','z',(10.35,1.72,5.15)),
(63,'ANIM_3D_STACK_SCADA','indicator','none',(0,6.15,-5.25)),
]
assert len(pivots)==48
for idx,p,kind,axis,pos in pivots:
    key=names[idx];PV.setdefault(key,{})[p]={'position':tuple(map(float,pos)),'kind':kind,'axis':axis}

def platform(k,pos,ext=(1.6,.30,1.5),highlight='cyan'):
    x,y,z=pos;w,h,d=ext
    box(k,pos,ext,'steel')
    box(k,(x,y+h*.53,z),(w*.91,.04,d*.91),'dark')
    for xx in (-w*.42,w*.42):
        for zz in (-d*.42,d*.42):cyl(k,(x+xx,y-h*.64,z+zz),.085,.12,'silver','y',12)
    for j in range(11):box(k,(x-w*.42+j*w*.085,y+h*.57,z+d*.45),(.045,.032,.04),highlight if j%3 else 'gold')

# BUILDING BLOCKS specific to die stacking and nanometre pose visualization

def copper_pads(k,center,size=(1.3,.9),nx=13,nz=9,scale=.035,phase=0,cc='gold'):
    x,y,z=center;wx,wz=size
    for ix in range(nx):
        for iz in range(nz):
            u=ix/(nx-1)-.5; v=iz/(nz-1)-.5
            # rich but structured different pad maps including asymmetric locator marks
            if (ix*3+iz*5+phase)%17==0:continue
            box(k,(x+u*wx,y,z+v*wz),(scale,.012,scale),cc if (ix+iz+phase)%8 else 'copper')
    for px,pz in [(-wx*.47,-wz*.44),(wx*.47,wz*.44)]:
        box(k,(x+px,y+.008,z+pz),(.10,.018,.038),'cyan')

def chiplet(k,center,size=(1.3,.075,1.00),accent='cyan',typ=0):
    x,y,z=center; sx,sy,sz=size
    box(k,center,size,'ceramic')
    box(k,(x,y+sy*.51,z),(sx*.92,.019,sz*.92),'navy')
    box(k,(x,y+sy*.79,z),(sx*.70,.015,sz*.64),accent)
    for i in range(12):
        xx=x+sx*(-.42+i*.077)
        box(k,(xx,y+sy*.91,z+.25*sz),(sx*.027,.008,sz*.26),'copper')
    for j in range(5):
        zz=z+sz*(-.34+j*.17)
        box(k,(x-.36*sx,y+sy*.90,zz),(sx*.16,.011,sz*.023),'gold')
    for i in range(5):
        px=x+sx*(-.37+i*.18)
        box(k,(px,y+sy*.65,z-sz*.28),(sx*.033,.014,sz*.13),'silver')
    copper_pads(k,(x,y-sy*.56,z),(sx*.84,sz*.77),15 if typ==1 else 13,13 if typ==1 else 9,.023,typ)
    for xx in (-sx*.46,sx*.46):
        for zz in (-sz*.46,sz*.46):
            cyl(k,(x+xx,y+sy*.58,z+zz),.028,.025,'gold','y',12)

def optics(k,p,acc='cyan',lens=.25):
    x,y,z=p;mechbox(k,(x,y,z),(1.08,.72,.78),'steel')
    cyl(k,(x,y-.42,z+.44),lens,.28,'dark','y',32)
    ring(k,(x,y-.59,z+.44),lens*1.08,lens*.65,.05,'y',acc,32)
    cyl(k,(x,y-.65,z+.44),lens*.66,.045,'glass','y',32)
    screen(k,(x,y+.12,z+.43),.67,.23,acc)
    for j in range(6):knob(k,x-.34+j*.13,y-.20,z+.415,acc if j<3 else 'green')

def inspection_bay(k,p,kind='acoustic',accent='cyan'):
    x,y,z=p
    platform(k,(x,1.04,z),(2.10,.26,1.72),accent)
    frame(k,(x,2.2,z),(2.18,2.45,1.90),.085,True)
    cyl(k,(x,1.49,z),.55,.19,'dark','y',40)
    waferdisc(k,(x,1.64,z),.48,.024,'iridescent')
    box(k,(x,3.33,z),(.85,.30,.73),'steel')
    for j in range(14):
        a=j*math.tau/14
        cyl(k,(x+.44*math.cos(a),1.76,z+.44*math.sin(a)),.025,.05,accent,'y',8)
    screen(k,(x+.72,2.44,z+.91),.55,.32,accent)
    for j in range(7):pipe(k,[(x+.97,2.2,z-.58+j*.12),(x+1.2,2.55,z-.58+j*.12)],.012,'silver')

def overhead(k,x,z,width=4.0,height=5.3,acc='cyan'):
    for xx in (x-width/2,x+width/2):
        box(k,(xx,3.10,z),(.20,height,.20),'steel')
        box(k,(xx,3.12,z+.11),(.05,height*.94,.05),acc)
        for iy in range(20):box(k,(xx,1.12+iy*height*.92/20,z+.17),(.13,.065,.08),'light')
    box(k,(x,5.67,z),(width+.48,.25,.55),'steel')
    for j in range(25):box(k,(x-width/2+j*width/24,5.83,z+.28),(.08,.06,.034),acc if j%3 else 'silver')

def rig_part(k,pivot,info):
    x,y,z=info['position'];up=pivot.upper();kind=info['kind'];axis=info['axis']
    with moving(pivot):
        if pivot=='ANIM_GANTRY_X':
            box(k,(x,y,z),(7.10,.28,.46),'steel')
            for j in range(37):box(k,(-3.36+j*.187,y+.19,z+.16),(.065,.05,.10),'cyan')
            for xx in (-3.40,3.40):box(k,(xx,y-.23,z),(.34,.37,.56),'dark')
        elif pivot=='ANIM_GANTRY_Y':
            box(k,(x,y,z),(1.00,.38,3.42),'steel')
            for j in range(20):box(k,(x+.43,y+.05,z-1.5+j*.155),(.06,.055,.075),'silver')
        elif pivot=='ANIM_GANTRY_Z':
            box(k,(x,y,z),(.61,1.80,.62),'steel')
            for dx in (-.24,.24):cyl(k,(x+dx,y,z),.065,1.80,'silver','y',14)
            for j in range(14):box(k,(x,y-.75+j*.12,z+.34),(.42,.038,.02),'cyan')
        elif pivot=='ANIM_FINE_HEXAPOD':
            cyl(k,(x,y,z),.72,.23,'dark','y',36)
            ring(k,(x,y+.10,z),.73,.57,.045,'y','silver',36)
            for a in range(6):
                ang=a*math.tau/6
                p=(x+.58*math.cos(ang),y-.22,z+.58*math.sin(ang));q=(x+.43*math.cos(ang+.27),y+.31,z+.43*math.sin(ang+.27))
                pipe(k,[p,q],.064,'copper',12)
                cyl(k,p,.1,.065,'silver','y',12)
        elif pivot=='ANIM_GRIPPER_ROTATION':
            cyl(k,(x,y,z),.72,.20,'silver','y',36)
            ring(k,(x,y-.14,z),.69,.58,.05,'y','copper',36)
            for i in range(10):
                a=i*math.tau/10
                box(k,(x+.55*math.cos(a),y-.17,z+.55*math.sin(a)),(.09,.09,.11),'dark')
        elif pivot=='ANIM_UPPER_CHUCK_CLAMP':
            for sg in (-1,1):
                box(k,(x+sg*.59,y,z),(.28,.16,1.30),'steel')
                for j in range(6):box(k,(x+sg*.47,y-.10,z-.47+j*.19),(.10,.04,.060),'gold')
        elif pivot=='ANIM_LOWER_CHUCK_THETA' or pivot=='ANIM_LOWER_WAFER_LIFT':
            cyl(k,(x,y,z),1.11,.11,'steel','y',40)
            ring(k,(x,y+.10,z),1.10,.96,.05,'y','gold',36)
            for j in range(8):
                a=j*math.tau/8
                box(k,(x+.91*math.cos(a),y+.11,z+.91*math.sin(a)),(.12,.04,.12),'cyan')
        elif pivot=='ANIM_MOIRE_OVERLAY' or pivot=='ANIM_BOND_WAVE_HOLOGRAM':
            box(k,(x,y,z),(2.70,1.25,.05),'dark')
            box(k,(x,y,z+.035),(2.54,1.10,.016),'glass')
            for j in range(19):
                xx=x-1.17+j*.13
                box(k,(xx,y,z+.06),(.022,1.01,.013),'cyan' if j%2 else 'violet')
            for j in range(9):box(k,(x,y-.48+j*.12,z+.071),(2.40,.012,.012),'gold' if j%2 else 'teal')
        elif pivot=='ANIM_3D_STACK_SCADA':
            giantdisplay(k,(x,y,z),6.0,1.9)
            for j in range(29):
                box(k,(x-2.55+j*.18,y-.62+.22*math.sin(j*.31),z+.20),(.075,.038,.023),'gold' if j%5 else 'green')
        elif 'CAMERA' in up or 'LENS' in up or 'INTERFEROMETER' in up or 'SCAN' in up or 'TOMOGRAPHY' in up:
            box(k,(x,y,z),(.44,.22,.48),'steel')
            cyl(k,(x,y-.21,z),.18,.24,'dark','y',20)
            ring(k,(x,y-.35,z),.20,.15,.035,'y','cyan',24)
            cyl(k,(x,y-.38,z),.13,.035,'glass','y',24)
            box(k,(x+.21,y+.08,z),(.1,.11,.12),'green')
        elif 'PROBE' in up or 'TEST' in up or 'FORCE' in up or 'GAP' in up or 'CONTACT' in up:
            box(k,(x,y,z),(.55,.24,.49),'steel')
            for j in range(7):
                dx=-.20+j*.067
                cyl(k,(x+dx,y-.24,z),.018,.33,'gold','y',8)
            box(k,(x,y+.16,z),(.35,.08,.28),'dark')
        elif 'GRIPPER' in up or 'HEATER' in up or 'THERMAL' in up:
            box(k,(x,y,z),(.90,.14,.85),'steel')
            for j in range(10):box(k,(x-.39+j*.08,y+.09,z),(.042,.06,.62),'amber' if 'HEATER' in up else 'cyan')
        elif 'EMERGENCY' in up:
            box(k,(x,y,z),(.49,.40,.32),'black')
            cyl(k,(x,y,z+.2),.15,.16,'red','z',24)
            ring(k,(x,y,z+.28),.19,.16,.04,'z','amber',24)
        elif 'CHUCK' in up or 'THETA' in up or 'ALIGNER' in up:
            cyl(k,(x,y,z),.67,.19,'steel','y',32)
            ring(k,(x,y+.12,z),.68,.59,.04,'y','silver',32)
            for j in range(12):
                a=j*math.tau/12;box(k,(x+.51*math.cos(a),y+.15,z+.51*math.sin(a)),(.08,.034,.08),'copper')
        elif 'PARTICLE' in up or 'SURFACE' in up or 'AIR_CURTAIN' in up:
            box(k,(x,y,z),(.55,.14,.52),'dark')
            for j in range(13):
                a=j*math.tau/13
                pipe(k,[(x,y-.06,z),(x+.28*math.cos(a),y-.25,z+.28*math.sin(a))],.014,'teal',8)
        elif 'TRAY' in up:
            box(k,(x,y,z),(1.72,.14,1.62),'steel')
            for j in range(4):
                for q in range(4):box(k,(x-.61+j*.40,y+.09,z-.58+q*.38),(.31,.06,.25),'navy')
        elif 'DOOR' in up or 'LID' in up:
            box(k,(x,y,z),(1.13,.17,1.03),'steel')
            box(k,(x,y+.10,z),(.99,.04,.91),'glass')
            for j in range(8):
                a=j*math.tau/8
                cyl(k,(x+.47*math.cos(a),y+.15,z+.47*math.sin(a)),.032,.043,'silver','y',10)
        else:
            box(k,(x,y,z),(.46,.16,.38),'steel')
            for j in range(5):box(k,(x-.17+j*.085,y+.1,z),(.035,.04,.07),'cyan')

# NOTE: modular placement is in industrial metres with Y-up; FBX writer exports numeric cm and GLB metres.
def build_model(i):
    k=names[i]
    if i==0:
        box(k,(0,.31,0),(22.45,.60,12.30),'navy')
        box(k,(0,.63,0),(22.13,.06,12.05),'steel')
        for x in np.linspace(-10.8,10.8,18):
            box(k,(x,.68,0),(.025,.025,11.55),'light')
            for z in np.linspace(-5.5,5.5,10):box(k,(x,.685,z),(.038,.018,.10),'cyan' if abs(x)<3.25 else 'amber')
        for z in (-5.75,5.75):
            box(k,(0,.73,z),(21.8,.17,.20),'dark')
            for x in np.linspace(-10.7,10.7,44):box(k,(x,.83,z),(.045,.035,.04),'cyan' if z<0 else 'gold')
        for x in (-10.65,10.65):
            box(k,(x,.91,0),(.24,.52,11.85),'dark')
            for z in np.linspace(-5.4,5.4,22):box(k,(x,1.19,z),(.05,.045,.12),'amber')
        for x,z in [(-8,-3.6),(8,-3.6),(-8,3.7),(8,3.7),(0,4.65),(0,-4.25)]:
            box(k,(x,.70,z),(1.35,.10,1.10),'silver')
            for dx in (-.4,.4):
                for dz in (-.3,.3):cyl(k,(x+dx,.78,z+dz),.055,.08,'black','y',10)
    elif i==1:
        # Tall slender cathedral, four corner posts and vertical optical rails
        frame(k,(0,4.50,-.80),(5.20,7.15,5.15),.15,True,True)
        for x in (-2.26,2.26):
            for z in (-2.82,1.22):
                box(k,(x,4.65,z),(.18,6.70,.19),'steel')
                for j in range(30):box(k,(x,1.49+j*.205,z+.15),(.10,.065,.05),'cyan' if j%5 else 'amber')
        for y in (1.25,2.22,3.82,5.70,7.86):
            ring(k,(0,y,-.80),2.50,2.37,.10,'y','steel',48)
            for a in range(12):
                t=a*math.tau/12
                box(k,(2.45*math.cos(t),y+.07,-.80+2.45*math.sin(t)),(.18,.06,.18),'gold')
        for z in (-3.13,1.53):
            box(k,(0,4.55,z),(4.7,5.2,.04),'glass')
        box(k,(0,8.12,-.80),(5.2,.35,5.0),'dark')
    elif i==2:
        platform(k,(-6.95,1.05,1.40),(2.0,.29,1.93),'gold')
        cyl(k,(-6.95,1.36,1.40),.89,.20,'steel','y',44)
        ring(k,(-6.95,1.50,1.40),1.00,.88,.09,'y','gold',44)
        for j in range(18):
            a=j*math.tau/18
            box(k,(-6.95+.86*math.cos(a),1.54,1.40+.86*math.sin(a)),(.06,.08,.085),'silver')
    elif i==3:
        waferdisc(k,site,1.00,.042,'iridescent',True)
        for px,pz in [(-.55,-.55),(.55,-.55),(-.55,.55),(.55,.55)]:
            copper_pads(k,(px,2.213,-.80+pz),(.66,.57),10,10,.025,int((px+2)*10))
        for a in range(14):
            t=a*math.tau/14
            box(k,(.90*math.cos(t),2.25,-.80+.90*math.sin(t)),(.032,.020,.065),'gold')
    elif i==4:
        for j in range(5):
            x=-2.5+j*1.25
            waferdisc(k,(x,5.0,2.42),.46,.020,'glass',False)
            copper_pads(k,(x,5.02,2.42),(.70,.70),9,9,.026,j)
        for j in range(18):
            x=-2.65+j*.31
            box(k,(x,5.55,2.45),(.045,.04,.044),'cyan')
    elif i==5:
        mechbox(k,(-8.45,1.22,1.05),(2.36,.65,2.40),'steel')
        for dx in (-.69,.69):
            for dz in (-.70,.70):
                box(k,(-8.45+dx,1.62,1.05+dz),(.88,.11,.73),'navy')
                for a in range(5):
                    for b in range(4):box(k,(-8.78+dx+a*.16,1.69, .78+dz+b*.17),(.045,.015,.056),'gold')
        for j in range(10):box(k,(-9.5+j*.22,1.75,2.11),(.062,.035,.05),'cyan')
    elif i in (6,7,8,9):
        xy=[(-8.95,2.10,.38),(-7.95,2.10,.38),(-8.95,2.10,1.78),(-7.95,2.10,1.78)][i-6]
        size=[(1.30,.09,1.20),(1.65,.11,1.38),(1.2,.10,1.0),(1.1,.10,.94)][i-6]
        chiplet(k,xy,size,['cyan','violet','gold','green'][i-6],i-6)
        if i==7:
            for j in range(5):box(k,(xy[0]-.50+j*.25,xy[1]+.12,xy[2]),(.15,.13,.72),'dark')
        elif i==8:
            for j in range(9):box(k,(xy[0]-.45+j*.11,xy[1]+.09,xy[2]+.38),(.05,.045,.09),'copper')
    elif i==10:
        inspection_bay(k,(-6.75,1.72,3.65),'kgd','green')
        screen(k,(-5.72,2.88,4.63),.55,.48,'green')
    elif i==11:
        frame(k,(-8.98,2.60,-1.60),(2.72,2.92,2.45),.11,True)
        cyl(k,(-8.98,1.73,-1.60),.73,.16,'steel','y',32)
        waferdisc(k,(-8.98,1.85,-1.60),.55,.028,'copper')
        for y in (1.28,3.9):ring(k,(-8.98,y,-1.60),1.22,1.10,.07,'y','silver',38)
    elif i==12:
        for x in (-8.50,-7.20):
            box(k,(x,2.60,-1.15),(.14,2.25,.20),'steel')
            for y in np.linspace(1.9,3.45,12):box(k,(x,y,-1.00),(.052,.033,.055),'cyan')
    elif i==13:
        platform(k,(-7.15,1.35,1.60),(1.70,.25,1.7),'cyan')
        waferdisc(k,(-7.15,2.05,1.60),.68,.03,'copper')
        for j in range(4):optics(k,(-7.15+(-.85 if j%2 else .85),2.56,1.60+(-.71 if j<2 else .71)),'cyan',.08)
    elif i==14:
        overhead(k,0,-.80,5.8,6.10,'cyan')
        for x in (-2.90,2.90):
            box(k,(x,7.34,-.8),(.43,.40,4.6),'dark')
            for z in np.linspace(-2.8,1.25,16):box(k,(x,7.53,z),(.26,.045,.052),'silver')
    elif i==15:
        for z in (-2.50,.90):
            box(k,(0,7.04,z),(6.40,.18,.24),'steel')
            for j in range(38):box(k,(-3.05+j*.165,7.17,z+.12),(.076,.045,.06),'cyan')
        for x in (-3.1,3.1):box(k,(x,6.78,-.8),(.38,.45,4.1),'dark')
    elif i==16:
        box(k,(0,6.76,-.80),(1.60,.35,3.55),'steel')
        for z in np.linspace(-2.45,.85,22):box(k,(0,6.96,z),(.84,.025,.045),'silver')
    elif i==17:
        box(k,(0,5.53,-.8),(.98,2.3,.99),'steel')
        for dx in (-.36,.36):
            for dz in (-.35,.35):cyl(k,(dx,5.48,-.8+dz),.052,2.23,'silver','y',12)
        for j in range(20):box(k,(0,4.55+j*.10,-.29),(.48,.035,.034),'cyan')
    elif i==18:
        cyl(k,(0,5.10,-.8),1.24,.25,'dark','y',48)
        cyl(k,(0,4.65,-.8),.91,.18,'steel','y',40)
        for j in range(6):
            a=j*math.tau/6
            pipe(k,[(1.0*math.cos(a),5.03,-.8+1.0*math.sin(a)),(.75*math.cos(a+.28),4.64,-.8+.75*math.sin(a+.28))],.075,'copper',12)
    elif i in (19,20):
        x=-.77 if i==19 else .77
        box(k,(x,5.06,-.8),(.72,.39,.84),'steel')
        for j in range(20):box(k,(x-.25+j*.026,5.28,-.8),(.019,.025,.61),'gold')
        for s in (-1,1):cyl(k,(x+s*.30,5.08,-.8),.073,.53,'silver','x' if i==19 else 'z',12)
    elif i==21:
        cyl(k,(0,4.94,-.8),.91,.22,'steel','y',44)
        ring(k,(0,5.07,-.8),1.0,.91,.04,'y','gold',40)
        for j in range(48):
            a=j*math.tau/48
            box(k,(.92*math.cos(a),5.10,-.8+.92*math.sin(a)),(.032,.03,.035),'cyan' if j%4 else 'gold')
    elif i==22:
        cyl(k,(0,4.80,-.8),.87,.18,'dark','y',40)
        for x in (-.65,.65):
            box(k,(x,4.80,-.8),(.20,.30,.20),'steel')
            for dz in (-.22,.22):cyl(k,(x,4.73,-.8+dz),.045,.18,'silver','z',12)
    elif i==23:
        cyl(k,(0,4.40,-.8),.86,.23,'steel','y',40)
        ring(k,(0,4.22,-.8),.88,.77,.06,'y','gold',40)
        for a in range(20):
            t=a*math.tau/20
            box(k,(.80*math.cos(t),4.24,-.8+.8*math.sin(t)),(.09,.06,.07),'dark')
        for j in range(10):
            box(k,(-.62+j*.138,4.18,-.8),(.035,.03,1.38),'copper')
    elif i==24:controlbox(k,(1.95,4.11,1.75),(1.0,.71,.61),'steel','green',3)
    elif i==25:
        cyl(k,(0,1.93,-.8),1.12,.29,'steel','y',48)
        ring(k,(0,2.09,-.8),1.17,1.04,.065,'y','cyan',48)
        for j in range(12):
            a=j*math.tau/12
            cyl(k,(1.06*math.cos(a),2.13,-.8+1.06*math.sin(a)),.045,.047,'dark','y',10)
    elif i==26:
        for x in (-.62,0,.62):
            for z in (-1.36,-.8,-.24):
                cyl(k,(x,4.04,z),.092,.19,'silver','y',14)
                box(k,(x,4.17,z),(.15,.07,.16),'gold')
        controlbox(k,(-2.8,3.3,1.7),(.97,.78,.66),'steel','amber')
    elif i in (27,28):
        xx=-2.35 if i==27 else 2.35
        optics(k,(xx,4.28,1.08),'cyan' if i==27 else 'violet',.29)
        pipe(k,[(xx,4.65,.87),(xx,6.23,.87),(xx*.76,6.23,1.25)],.074,'steel',12)
    elif i in (29,30):
        xx=-1.80 if i==29 else 1.90
        optics(k,(xx,5.35,-2.30),'violet' if i==29 else 'laser',.33)
        for j in range(5):
            pipe(k,[(xx-.4+j*.20,5.05,-2.32),(xx-.40+j*.20,4.74,-1.95)],.019,'cyan',10)
    elif i==31:
        box(k,(0,5.87,1.70),(3.38,1.56,.14),'dark')
        box(k,(0,5.87,1.80),(3.15,1.33,.025),'glass')
        for j in range(23):box(k,(-1.42+j*.13,5.87,1.86),(.019,1.18,.015),'cyan' if j%2 else 'gold')
        for j in range(13):box(k,(0,5.30+j*.095,1.87),(2.95,.014,.015),'violet')
    elif i==32:inspection_bay(k,(-4.75,1.74,3.30),'warp','cyan')
    elif i==33:inspection_bay(k,(-6.30,1.74,3.30),'particle','violet')
    elif i==34:
        platform(k,(-7.65,1.07,3.62),(1.25,.26,1.04),'cyan')
        optics(k,(-7.65,2.55,3.6),'green',.23)
        for j in range(10):box(k,(-8.05+j*.09,1.42,3.62),(.028,.02,.62),'copper')
    elif i==35:
        platform(k,(-8.52,1.05,3.65),(1.11,.27,1.06),'amber')
        multisensor(k,-8.52,2.42,3.65,'particle',.29)
        waferdisc(k,(-8.52,1.62,3.65),.48,.025,'copper')
    elif i in (36,37):
        xx=-9.70 if i==36 else -8.58
        frame(k,(xx,2.24,3.60),(1.45,2.00,1.40),.075,True)
        for j in range(14):
            box(k,(xx-.56+j*.085,3.22,3.6),(.053,.07,.70),'cyan' if i==36 else 'violet')
        for j in range(8):
            pipe(k,[(xx-.43+j*.12,3.05,3.60),(xx-.43+j*.12,2.72,3.60)],.013,'gold' if i==37 else 'cyan')
    elif i==38:
        # Outer curved-view-window protection system
        for x in (-2.52,2.52):box(k,(x,4.42,-.8),(.18,5.5,3.63),'steel')
        for y in (1.80,6.88):box(k,(0,y,-.8),(5.15,.17,3.63),'silver')
        for zz in (-2.50,.90):box(k,(0,4.42,zz),(4.91,5.0,.035),'glass')
        ring(k,(0,6.91,-.8),2.50,2.40,.07,'y','gold',48)
    elif i==39:
        cyl(k,(0,3.64,-.8),.55,.20,'dark','y',40)
        ring(k,(0,3.51,-.8),.58,.47,.05,'y','gold',40)
        for a in range(12):
            t=a*math.tau/12
            cyl(k,(.48*math.cos(t),3.48,-.8+.48*math.sin(t)),.036,.035,'cyan','y',10)
    elif i==40:
        optics(k,(-1.65,3.86,.65),'laser',.22)
        for j in range(9):
            t=j*math.tau/9
            pipe(k,[(-1.65,3.65,.65),(-1.65+.62*math.cos(t),3.24,.65+.62*math.sin(t))],.014,'cyan',8)
    elif i==41:
        box(k,(0,5.0,3.85),(3.0,1.65,.12),'dark')
        box(k,(0,5.0,3.93),(2.81,1.43,.027),'glass')
        for r in (.22,.45,.66,.86):ring(k,(0,5.00,4.0),r,r-.035,.02,'z','cyan',48)
        for x in np.linspace(-1.2,1.2,13):box(k,(x,4.58,4.03),(.05,.024,.015),'gold')
    elif i==42:
        for x in (-1.2,0,1.2):
            box(k,(x,3.35,-.8),(.18,.40,.20),'silver')
            for j in range(4):cyl(k,(x,3.05,-1.12+j*.18),.04,.19,'cyan','y',10)
        controlbox(k,(2.7,2.24,-2.0),(1.15,.8,.64),'steel','cyan')
    elif i==43:
        x,z=4.45,-2.90
        frame(k,(x,2.50,z),(3.0,2.93,2.72),.11,True)
        cyl(k,(x,1.61,z),1.11,.21,'dark','y',40)
        ring(k,(x,1.75,z),1.20,1.10,.05,'y','amber',40)
        for j in range(7):box(k,(x-1.20+j*.4,3.72,z),( .14,.07,1.84),'gold')
    elif i==44:
        for yy in (1.70,3.20):
            cyl(k,(4.45,yy,-2.90),1.06,.18,'steel','y',40)
            for j in range(16):
                a=j*math.tau/16
                box(k,(4.45+.84*math.cos(a),yy+.11,-2.90+.84*math.sin(a)),(.1,.05,.12),'gold' if yy>2 else 'amber')
    elif i==45:
        multisensor(k,5.68,2.70,-2.88,'thermal',.28)
        pipe(k,[(5.50,2.62,-2.80),(5.01,2.51,-2.80)],.025,'copper')
    elif i==46:
        inspection_bay(k,(5.75,1.63,-2.90),'expansion','amber')
        for j in range(13):box(k,(5.16+j*.10,3.78,-2.35),(.045,.04,.04),'gold')
    elif i==47:
        controlbox(k,(6.45,1.36,-3.97),(1.20,1.09,.65),'steel','violet')
        for j in range(5):cyl(k,(6.00+j*.20,2.06,-3.97),.04,.16,'silver','y',10)
    elif i in (48,49,50):
        x=6.25+(i-48)*1.45
        inspection_bay(k,(x,1.67,1.75),['acoustic','infrared','seam'][i-48],['cyan','violet','amber'][i-48])
        for j in range(9):
            box(k,(x-.45+j*.112,3.30,1.75),(.055,.055,.12),['cyan','violet','amber'][i-48])
    elif i==51:
        inspection_bay(k,(6.10,1.71,3.78),'coupon','green')
        for j in range(10):box(k,(5.73+j*.08,1.99,3.78),(.035,.045,.15),'gold')
    elif i in (52,53):
        x=7.32 if i==52 else 8.55
        inspection_bay(k,(x,1.68,3.78),'electrical','amber' if i==52 else 'green')
        for j in range(12):
            xx=x-.42+j*.077
            box(k,(xx,1.79,3.78),(.025,.04,.58),'gold' if j%2 else 'copper')
    elif i in (54,55):
        x=9.5
        controlbox(k,(x,1.38,4.68),(.90,1.20,.65),'steel','green' if i==54 else 'cyan')
        for j in range(16):
            box(k,(x-.35+j*.045,2.04,4.68),(.026,.06,.11),'silver')
        if i==55:
            for j in range(15):
                box(k,(x-.34+j*.047,1.50+.19*math.sin(j*.7),5.03),(.028,.025,.018),'gold')
    elif i==56:
        frame(k,(9.0,2.30,-2.91),(2.55,2.55,2.45),.095,True)
        waferdisc(k,(9.0,1.79,-2.91),.70,.025,'copper')
        for j in range(12):
            a=j*math.tau/12
            box(k,(9+.82*math.cos(a),2.25,-2.91+.82*math.sin(a)),(.1,.12,.11),'amber')
    elif i==57:
        inspection_bay(k,(7.20,1.89,-2.80),'warpage','cyan')
    elif i==58:
        mechbox(k,(-10.0,1.31,4.35),(1.68,.82,1.42),'dark')
        box(k,(-10.0,1.75,4.35),(1.47,.1,1.19),'red')
        for j in range(9):box(k,(-10.66+j*.16,1.85,4.35),(.084,.045,.96),'warning')
    elif i==59:
        frame(k,(-10.05,2.22,2.60),(1.54,2.38,1.58),.10,True)
        for j in range(7):box(k,(-10.60+j*.18,1.45,2.60),(.08,.09,1.05),'gold')
        box(k,(-10.05,3.16,2.60),(1.40,.09,1.39),'dark')
    elif i==60:
        hd_cabinet(k,(-5.36,1.68,-4.8),(1.62,1.65,.85),'cyan',2)
        for j in range(11):box(k,(-6.02+j*.13,2.66,-4.8),(.07,.10,.10),'silver')
    elif i==61:
        hd_cabinet(k,(-3.55,1.68,-4.8),(1.39,1.56,.79),'violet',2)
        for j in range(18):box(k,(-4.15+j*.065,2.57,-4.8),(.037,.065,.11),'gold')
    elif i==62:
        controlbox(k,(10.35,1.72,5.15),(1.23,1.45,.68),'steel','red')
        cyl(k,(10.35,1.93,5.54),.20,.13,'red','z',24)
        ring(k,(10.35,1.93,5.64),.26,.23,.04,'z','amber',24)
    elif i==63:
        giantdisplay(k,(0,6.15,-5.25),7.20,2.32)
        for j in range(4):
            x=-2.65+j*1.75
            box(k,(x,6.18,-5.08),(1.30,1.45,.022),'dark')
            for m in range(12):box(k,(x-.53+m*.096,5.77+.33*math.sin(m*.5+j),-5.02),(.041,.024,.02),'cyan' if j%2 else 'gold')
    elif i==64:
        hd_cabinet(k,(-9.35,1.64,-4.75),(1.43,1.34,.85),'green',2)
        for j in range(14):box(k,(-9.91+j*.083,2.52,-4.75),(.035,.09,.11),'gold' if j%4 else 'red')
    elif i==65:
        for j in range(4):
            x=-1.40+j*.94
            chiplet(k,(x,5.60+j*.27,3.25),(1.10,.075,.83),['cyan','violet','gold','green'][j],j)
            for m in range(12):
                a=m*math.tau/12
                pipe(k,[(x+.35*math.cos(a),5.51+j*.27,3.25+.24*math.sin(a)),(x+.31*math.cos(a),5.28+j*.27,3.25+.24*math.sin(a))],.012,'gold',8)
        box(k,(0,5.13,3.25),(5.15,.10,2.05),'glass')
    else: raise AssertionError(i)
    # Physically grouped pivot geometry, not empty dummy parent objects.
    for pname,info in PV.get(k,{}).items():rig_part(k,pname,info)
    if i not in (0,1,3,4,6,7,8,9,14,15,16,17,18,23,25,38,41,43,44,48,49,50,63,65):
        if M[k]:
            verts=np.vstack([np.asarray(me.vertices) for me,_,_ in M[k]])
            lo=verts.min(axis=0);hi=verts.max(axis=0)
            cx,cy,cz=(lo+hi)/2
            if hi[1]<5.1 and (hi[2]-lo[2])>.1:
                for j in range(4):box(k,(cx-.14+j*.09,min(hi[1]-.05,cy+.3),hi[2]+.025),(.045,.04,.025),'green' if j%2 else 'cyan')

# Export distinct original die-bonding 3D geometry; all movable pivots are true parent nodes.
for index in range(66):
    build_model(index)
    key=names[index]
    if not M.get(key):raise ValueError('Empty model '+key)
    groups={g for me,mat,g in M[key] if g}
    assert groups==set(PV.get(key,{})),(index,groups,set(PV.get(key,{})))
print('MODELED 79:',len(names),'equipment;',sum(len(M[n]) for n in names),'mesh parts;',sum(sum(len(me.faces) for me,c,p in M[n]) for n in names),'triangles;',sum(len(x) for x in PV.values()),'pivot parents',flush=True)
manifest=[]
for i,key in enumerate(names):
    filename=f'{i:02d}_L79_{key}'
    export_fbx(key,MD/(filename+'.fbx'))
    export_glb(key,GD/(filename+'.glb'))
    tris=sum(len(m.faces) for m,c,p in M[key]);parts=len(M[key]);coords=np.vstack([m.vertices for m,_,_ in M[key]])
    manifest.append(dict(index=i,name='L79_'+key,file=filename+'.fbx',glb=filename+'.glb',triangle_count=tris,mesh_parts=parts,bbox_min=np.around(coords.min(axis=0),5).tolist(),bbox_max=np.around(coords.max(axis=0),5).tolist(),rig_groups=list(PV.get(key,{})),animation_pivots={p:list(d['position']) for p,d in PV.get(key,{}).items()}))
    print('EXPORT',i+1,'/66',filename,'parts',parts,'tris',tris,flush=True)
allkey='Level79_Complete_ArtKit_Showcase'
M[allkey]=[row for name in names for row in M[name]]
PV[allkey]={p:v for pv in PV.values() for p,v in pv.items()}
export_fbx(allkey,MD/(allkey+'.fbx'))
export_glb(allkey,GD/(allkey+'.glb'))
coords=np.vstack([m.vertices for m,c,p in M[allkey]])
bounds=np.stack((coords.min(axis=0),coords.max(axis=0)))
config={'level_id':'level_79','display_name':'Bond the Worlds','subtitle':'Precision 3D Hybrid Chiplet Integration','individual_models':66,'individual_glbs':66,'assembled_fbx':'Models/'+allkey+'.fbx','assembled_glb':'GLB/'+allkey+'.glb','individual_triangle_count':sum(m['triangle_count'] for m in manifest),'individual_mesh_parts':sum(m['mesh_parts'] for m in manifest),'animation_parent_pivot_groups':48,'planned_player_wires':122,'planned_unique_functional_terminals':188,'workstation_width_m_nominal':22.5,'workstation_world_bounds_m':bounds.round(4).tolist(),'fbx_numeric_units':'centimetres','fbx_unit_scale_factor':1,'glb_numeric_units':'metres','unity_expected_width_m':22.5,'axes':'Y up / X width / Z front','assembly_origin':[0,0,0],'actual_player_wires_included':False,'functional_spark_terminals_included':False,'bonding_physics_included':False,'six_authoritative_objectives_implemented':False,'models':manifest}
(ROOT/'Model_Manifest.json').write_text(json.dumps(config,indent=2),encoding='utf-8')
with (ROOT/'Animation_Rigs.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Pivot','Model','Kind','Axis','X','Y','Z','Description'])
    for idx,p,kind,axis,pos in pivots:w.writerow([p,'L79_'+names[idx],kind,axis,*pos,'Existing FBX parent transform - geometry truly nested and independently movable'])
with (ROOT/'Art_Assembly.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Index','File','LocalX','LocalY','LocalZ','Scale','Role'])
    for m in manifest:w.writerow([m['index'],m['file'],0,0,0,1,'Modular hybrid chiplet-bonding equipment'])
with (ROOT/'Component_Roles.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['Model','Category','Role','GameplayStatus'])
    for m in manifest:
        i=m['index'];role=['Workstation','Tower','Wafer','Wafer','Hologram','Chiplet Inventory'][i] if i<6 else 'Chiplet' if i<10 else 'Precision robotics / optical / bonding / metrology instrument'
        w.writerow([m['name'],role,m['name'].replace('L79_',''),'Art only, gameplay component not implemented'])
(ROOT/'Material_Palette.json').write_text(json.dumps({k:{'rgba_8bit':v,'surface_intent':('transparent' if v[3]<255 else 'opaque metallic or equipment color')} for k,v in P.items()},indent=2),encoding='utf-8')
print('COMBINED EXPORTED',len(M[allkey]),'mesh parts',config['individual_triangle_count'],'tris','width',bounds[1,0]-bounds[0,0],flush=True)
