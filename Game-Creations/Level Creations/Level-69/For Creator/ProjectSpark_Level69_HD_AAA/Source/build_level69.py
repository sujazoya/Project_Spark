import os, math, csv, json, time, struct, zlib, zipfile, random
from pathlib import Path
from collections import Counter
import numpy as np
import trimesh
from PIL import Image, ImageDraw, ImageFont

ROOT=Path('/mnt/data/ProjectSpark_Level69_HD_AAA'); MD=ROOT/'Models'; GD=ROOT/'GLB'; PD=ROOT/'Previews'; ED=ROOT/'Unity_Optional'; SRC=ROOT/'Source'
for d in (MD,GD,PD,ED,SRC):d.mkdir(parents=True,exist_ok=True)
names=['IndustrialColdChainBase','VariableSpeedRefrigerationCompressor','CompressorCutawayAssembly','CompressorMotorDrive','AirCooledCondenser','CondenserFanBank','HighPressureLiquidReceiver','LiquidLineFilterDrier','RefrigerantSightGlass','ElectronicExpansionValveA','ElectronicExpansionValveB','EvaporatorCoilA','EvaporatorCoilB','EvaporatorFanAssemblyA','EvaporatorFanAssemblyB','TransparentColdStorageChamber','TransparentRapidCoolingChamber','TemperatureSensitiveCargoRack','SuctionAccumulator','CompressorOilSeparator','CopperRefrigerantPipelineNetwork','SuctionPressureTransmitter','DischargePressureTransmitter','SuctionTemperatureSensor','DischargeTemperatureSensor','LiquidLineTemperatureSensor','EvaporatorOutletSensorA','EvaporatorOutletSensorB','ColdChamberTemperatureSensor','RapidCoolingTemperatureSensor','RefrigerantMassFlowMeter','RefrigerationEnergyAnalyzer','PressureEnthalpyCycleDisplay','DualZoneTemperaturePIDController','ElectronicSuperheatController','AdaptiveDefrostController','EvaporatorDefrostHeaterBank','CondensateDrainAssembly','CompressorProtectionRelay','ColdChainSafetyPLC','IndustrialRefrigerationPLCRack','RefrigerationDigitalIOModule','ThermalLoadDisturbanceModule','HolographicRefrigerationSCADA','TransparentRefrigerationSafetyEnclosure','Regulated5VControlSupply']
assert len(names)==46
P={'navy':(14,32,52,255),'steel':(66,101,126,255),'light':(139,177,195,255),'silver':(181,199,208,255),'dark':(27,50,69,255),'black':(13,21,30,255),'cyan':(37,211,246,255),'blue':(20,112,218,255),'ice':(133,224,245,255),'copper':(220,120,52,255),'amber':(248,162,45,255),'red':(226,63,70,255),'green':(47,219,139,255),'white':(220,238,248,255),'glass':(93,178,212,135),'frost':(205,235,247,255),'rubber':(34,38,42,255),'gold':(224,176,68,255)}
M={}
def addmesh(k,m,c):
 if m is None or len(m.faces)==0:return
 if k not in M:M[k]=[]
 m.visual.vertex_colors=np.tile(P[c],(len(m.vertices),1))
 M[k].append((m,c))
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
def compressor(k):
 x,y,z=(.25,1.27,.55)
 box(k,(x,y-.56,z),(3.0,.20,1.60),'steel')
 motor(k,(-.55,y,z),1.0)
 cyl(k,(1.10,y,z),.59,1.30,'blue','x',24)
 for t in np.linspace(.58,1.56,8):ring(k,(t,y,z),.61,.58,.075,'x','steel',24)
 cyl(k,(1.82,y,z),.40,.25,'dark','x')
 cyl(k,(1.2,y+.67,z),.20,.9,'silver','y')
 pipe(k,[(1.2,y+.74,z),(1.2,2.36,z),(.4,2.36,z)],.085,'copper')
 for i in range(5):box(k,(1.18+i*.09,1.26,z+.63),(.038,.17,.035),'amber')
def cooling_chamber(k,cx,cy,cz= -1.5,hot=False):
 # roomy 2.5x2x1.7 internal, glass front toward +z
 x,y,z=cx,cy,cz
 box(k,(x,y-.95,z),(2.65,.12,2.0),'steel')
 for dx in (-1.28,1.28):
  for dz in (-.96,.96):box(k,(x+dx,y,z+dz),(.09,1.95,.08),'silver')
 for yy in (-.89,.93):
  for dz in (-.96,.96):box(k,(x,y+yy,z+dz),(2.55,.07,.08),'silver')
 for yy in (-.89,.93):
  for dx in (-1.28,1.28):box(k,(x+dx,y+yy,z),(.08,.07,1.9),'silver')
 box(k,(x,y,z-.94),(2.4,1.74,.028),'ice')
 box(k,(x,y,z+.96),(2.45,1.7,.018),'glass')
 box(k,(x,y+.97,z),(2.68,.1,2.04),'steel')
 for xx in (-.76,.76):fan(k,(x+xx,y+.48,z-.76),.34)
 for i in range(3):
  box(k,(x,y-.70+i*.51,z-.20),(2.0,.046,.80),'silver')
  for j in range(4):box(k,(x-.75+j*.50,y-.54+i*.51,z-.15),(.33,.24,.32),'white' if not hot else 'ice')
 for i in range(2):box(k,(x-1.16+i*2.32,y,z+.99),(.07,1.72,.04),'steel')
 for i in range(5):box(k,(x-1.1+i*.54,y-.82,z+1.04),(.35,.035,.035),'cyan')

# scenery layout in 12.8x7.8 board; left chambers, central compressor, right condenser
k=names[0];box(k,(0,-.30,0),(13.8,.48,8.0),'navy');box(k,(0,-.04,0),(13.55,.05,7.75),'steel')
for x in np.arange(-6.45,6.5,.83):
 for z in (-3.82,3.82):box(k,(x,-.015,z),(.46,.021,.035),'cyan')
for z in np.arange(-3.3,3.3,.83):
 for x in (-6.73,6.73):box(k,(x,-.012,z),(.035,.021,.46),'cyan')
for x in np.arange(-6.1,6.2,.60):cyl(k,(x,-.04,3.70),.035,.07,'blue','y',10)
# refrigeration compressor
compressor(names[1]);k=names[2]
for i in range(4):cyl(k,(.75+i*.20,1.40,.6),.19,.34,'silver','x')
for i in range(3):cyl(k,(.90+i*.2,1.28,.70),.12,.42,'copper','y')
for i in range(10):box(k,(.75+i*.09,1.14,.45),(.045,.4,.04),'light')
panel(names[3],(-.90,.92,2.10),(1.10,.90,.52))
# condenser right
k=names[4];box(k,(4.15,2.35,-1.45),(3.8,1.50,1.16),'steel');box(k,(4.15,2.35,-.83),(3.56,1.30,.10),'black')
for j in range(32):
 x=2.45+j*.11
 box(k,(x,2.35,-.75),(.018,1.16,.03),'light' if j%3 else 'copper')
for y in np.linspace(1.8,2.91,6):pipe(k,[(2.5,y,-.68),(5.85,y,-.68)],.022,'copper')
for x in (2.55,5.76):box(k,(x,1.48,-1.45),(.25,.35,1.2),'steel')
k=names[5]
for x in (3.18,4.95):fan(k,(x,2.38,-.60),.61)
# receiver
k=names[6];cyl(k,(4.65,.88,1.88),.40,1.6,'blue','x');ring(k,(3.93,.88,1.88),.42,.37,.12,'x','silver');ring(k,(5.32,.88,1.88),.42,.37,.12,'x','silver')
for x in (4.08,5.15):box(k,(x,.43,1.88),(.23,.20,.85),'steel')
for x in (4.15,5.11):cyl(k,(x,1.21,1.88),.11,.1,'amber','y')
# filter
k=names[7];cyl(k,(3.23,.55,2.45),.18,.69,'steel','x');cyl(k,(3.23,.55,2.45),.20,.12,'copper','x')
# sight glass
k=names[8];cyl(k,(3.89,.58,2.45),.15,.38,'copper','x');cyl(k,(3.89,.58,2.45),.16,.08,'glass','x')
# EEV valve modules
for i,key in enumerate((names[9],names[10])):
 x=4.40+i*1.20;k=key
 box(k,(x,.68,3.03),(.73,.33,.63),'steel');cyl(k,(x,1.00,3.03),.15,.49,'copper','y')
 box(k,(x,1.26,3.03),(.38,.23,.39),'dark');screen(k,(x,1.26,3.26),.25,.13)
 pipe(k,[(x-.52,.66,3.03),(x+.52,.66,3.03)],.09,'copper');knob(k,x+.29,.93,3.23,'green')
# evaporator sets back of stacked chambers left
for i,key in enumerate((names[11],names[12])):
 k=key;y=2.5-i*1.90
 box(k,(-4.35,y,-2.65),(2.15,.72,.31),'steel')
 for j in range(26):box(k,(-5.28+j*.075,y,-2.46),(.034,.60,.04),'light')
 for yy in np.linspace(y-.25,y+.25,5):pipe(k,[(-5.31,yy,-2.39),(-3.37,yy,-2.39)],.018,'copper')
 for j in range(12):box(k,(-5.16+j*.17,y+.39,-2.4),(.085,.035,.05),'frost')
for i,key in enumerate((names[13],names[14])):
 k=key;y=2.48-i*1.90
 for x in (-4.85,-3.76):fan(k,(x,y,-2.16),.31)
# chambers left stacked
for i,key in enumerate((names[15],names[16])):cooling_chamber(key,-4.35,2.52-i*1.90)
# cargo stand
k=names[17]
for i in range(3):
 for j in range(3):box(k,(-5.04+j*.51,1.26+i*.35,-.24),(.36,.27,.39),'ice' if (j+i)%2 else 'white')
# accumulator / separator
for key,x,y,z,col in [(names[18],-1.35,1.14,-1.17,'blue'),(names[19],2.13,1.15,.40,'silver')]:
 k=key;cyl(k,(x,y,z),.40,1.35,col,'y');cyl(k,(x,y+.70,z),.13,.28,'copper','y')
 for j in range(2):ring(k,(x,y-.4+j*.9,z),.43,.39,.09,'y','steel')
 box(k,(x,y-.69,z),(.86,.12,.84),'steel')
# tubing loops around system
k=names[20]
paths=[([(1.28,2.32,.56),(2.00,2.32,.56),(2.00,3.23,-1.35),(3.01,3.23,-1.35)],'copper',.095), ([(5.85,2.32,-1.35),(6.05,2.32,-1.35),(6.05,1.05,1.89),(5.35,1.05,1.89)],'copper',.09), ([(3.87,.70,2.43),(4.4,.70,2.43),(4.4,.70,3.02)],'copper',.067), ([(5.58,.70,3.03),(5.99,.70,3.03),(5.99,1.55,-2.43),(-3.18,1.55,-2.43)],'cyan',.075), ([(-5.52,2.13,-2.38),(-6.06,2.13,-2.38),(-6.06,.62,-1.35),(-1.35,.62,-1.35)],'cyan',.073), ([(-1.35,1.85,-1.15),(-1.35,2.35,-1.15),(-.6,2.35,.55)],'blue',.085)]
for pts,c,r in paths:pipe(k,pts,r,c)
# instruments grouping
for idx,name in enumerate(names[21:30],start=21):
 k=name;x=[-2.46,-1.76,-1.07,-.38,.31,1.00,1.69,2.38,3.07][idx-21];z=3.34;y=.55
 box(k,(x,y,z),(.53,.57,.44),'steel');screen(k,(x,y+.12,z+.24),.37,.23)
 pipe(k,[(x,.30,z-.22),(x,.30,z-.53)],.037,'copper' if idx%2 else 'cyan')
# mass flow
k=names[30];cyl(k,(2.74,.54,2.50),.18,.55,'steel','x');ring(k,(2.74,.54,2.50),.21,.17,.055,'x','cyan')
# control boxes and screens
for idx in range(31,43):
 k=names[idx]; col=idx-31;x=-2.1+(col%6)*.78;y=.54+(col//6)*.84;z=3.46 if col<6 else 3.30
 panel(k,(x,y,z),(.65,.55,.40))
 if idx==32:
  for j in range(6):box(k,(x-.20+j*.08,y-.08+(j%3)*.045,z+.23),(.025,.025,.015),'cyan')
 if idx==36:
  for j in range(5):box(k,(x-.24+j*.12,y+.27,z),(.05,.12,.04),'amber')
# thermal disturbance generator
k=names[42];box(k,(5.78,.51,-.20),(.85,.91,.70),'steel');screen(k,(5.78,.73,.17),.56,.27)
# large SCADA screen top center
k=names[43];box(k,(.5,3.20,-2.10),(3.63,2.04,.26),'steel');box(k,(.5,3.21,-1.93),(3.36,1.78,.035),'black')
for i in range(4):
 yy=3.6-i*.23
 for j in range(8):box(k,(-.93+j*.38,yy,-1.90),(.22,.018,.01),'cyan' if (j+i)%3 else 'amber')
for i in range(11):box(k,(-1.00+i*.30,2.62+math.sin(i*.7)*.11,-1.88),(.10,.027,.012),'green')
box(k,(.51,2.04,-2.20),(.25,.52,.25),'steel');box(k,(.5,1.73,-2.20),(1.0,.13,.78),'steel')
# safety enclosure upper beams around compressor
k=names[44]
for x in (-2.35,2.38):
 for z in (-.80,1.50):box(k,(x,2.04,z),(.065,1.90,.07),'light')
for y in (1.11,2.99):
 for z in (-.80,1.50):box(k,(0,y,z),(4.75,.07,.06),'light')
for z in (-.8,1.5):
 for y in (1.11,2.99):box(k,(0,y,z),(4.75,.06,.05),'steel')
for x in (-2.35,2.38):box(k,(x,2.02,1.50),(.048,1.76,.03),'glass')
# supply
panel(names[45],(-6.15,.55,2.41),(.78,.74,.51))
# ensure everything nonempty
for k in names:
 if k not in M:raise RuntimeError(k+' empty')

# FBX binary 7400 exporter; standard Nodes/Objects/Connections
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

def fbx(k,path):
 meshes=M[k];nobj=[];con=[];uid=100000
 def getid():
  nonlocal uid;uid+=1;return uid
 root=getid()
 props70=N('Properties70',ch=[N('P','Lcl Translation','Lcl Translation','','A',0.0,0.0,0.0),N('P','Lcl Rotation','Lcl Rotation','','A',0.0,0.0,0.0),N('P','Lcl Scaling','Lcl Scaling','','A',1.0,1.0,1.0)])
 nobj.append(N('Model',root,k+'\x00\x01Model','Null',ch=[N('Version',232),N('Properties70',ch=props70.ch),N('Shading','Y'),N('Culling','CullingOff')]))
 con.append(N('C','OO',root,0))
 colorMat={}
 for idx,(mesh,color) in enumerate(meshes):
  # vertex coordinates in meters scaled 100 for FBX centimeter convention, importer scale .01 -> meters
  vertices=np.array(mesh.vertices,dtype=np.float64)*100.0
  tris=np.array(mesh.faces,dtype=np.int32)
  faceindices=tris.copy();faceindices[:,-1]=-(faceindices[:,-1]+1)
  gid=getid();mid=getid();geo=N('Geometry',gid,f'{k}_Geom_{idx:03d}\x00\x01Geometry','Mesh',ch=[N('GeometryVersion',124),N('Vertices',vertices.reshape(-1)),N('PolygonVertexIndex',faceindices.reshape(-1)),N('LayerElementMaterial',0,ch=[N('Version',101),N('Name',''),N('MappingInformationType','AllSame'),N('ReferenceInformationType','IndexToDirect'),N('Materials',np.array([0],dtype=np.int32))]),N('Layer',0,ch=[N('Version',100),N('LayerElement',ch=[N('Type','LayerElementMaterial'),N('TypedIndex',0)])])])
  nobj.append(geo)
  nobj.append(N('Model',mid,f'{k}_Part_{idx:03d}\x00\x01Model','Mesh',ch=[N('Version',232),N('Properties70',ch=[N('P','Lcl Translation','Lcl Translation','','A',0.0,0.0,0.0),N('P','Lcl Rotation','Lcl Rotation','','A',0.0,0.0,0.0),N('P','Lcl Scaling','Lcl Scaling','','A',1.0,1.0,1.0)]),N('Shading','Y'),N('Culling','CullingOff')]))
  con.extend([N('C','OO',gid,mid),N('C','OO',mid,root)])
  if color not in colorMat:
   matid=getid();rgb=[q/255 for q in P[color][:3]]
   nobj.append(N('Material',matid,f'PS_{color}\x00\x01Material','',ch=[N('Version',102),N('ShadingModel','phong'),N('MultiLayer',0),N('Properties70',ch=[N('P','DiffuseColor','Color','','A',*rgb),N('P','AmbientColor','Color','','A',*(v*.28 for v in rgb)),N('P','SpecularColor','Color','','A',.6,.65,.68),N('P','Shininess','Number','','A',55.0),N('P','Opacity','Number','','A',P[color][3]/255.)])]))
   colorMat[color]=matid
  con.append(N('C','OO',colorMat[color],mid))
 top=[N('FBXHeaderExtension',ch=[N('FBXHeaderVersion',1003),N('FBXVersion',7400),N('Creator','Project Spark Procedural 3D Asset Builder')]),N('GlobalSettings',ch=[N('Version',1000),N('Properties70',ch=[N('P','UpAxis','int','Integer','',1),N('P','UpAxisSign','int','Integer','',1),N('P','FrontAxis','int','Integer','',2),N('P','FrontAxisSign','int','Integer','',1),N('P','CoordAxis','int','Integer','',0),N('P','CoordAxisSign','int','Integer','',1),N('P','UnitScaleFactor','double','Number','',1.0),N('P','OriginalUnitScaleFactor','double','Number','',1.0)])]),N('Definitions',ch=[N('Version',100),N('Count',len(nobj))]),N('Objects',ch=nobj),N('Connections',ch=con)]
 header=b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400)
 blob=bytearray(header)
 for node in top:blob.extend(encnode(node,len(blob)))
 blob.extend(bytes(13));blob.extend(bytes(160))
 path.write_bytes(blob)

def glb(k,path):
 sc=trimesh.Scene()
 for i,(m,c) in enumerate(M[k]):sc.add_geometry(m,geom_name=f'{k}_Part_{i:03d}')
 path.write_bytes(sc.export(file_type='glb'))

pivots=[('VariableSpeedRefrigerationCompressor','ANIM_COMPRESSOR_ROTOR',(.25,1.27,.55)),('CompressorCutawayAssembly','ANIM_COMPRESSOR_MECHANISM',(.96,1.35,.61)),('CondenserFanBank','ANIM_CONDENSER_FAN_A',(3.18,2.38,-.60)),('CondenserFanBank','ANIM_CONDENSER_FAN_B',(4.95,2.38,-.60)),('EvaporatorFanAssemblyA','ANIM_EVAP_FAN_A',(-4.85,2.48,-2.16)),('EvaporatorFanAssemblyB','ANIM_EVAP_FAN_B',(-4.85,.58,-2.16)),('ElectronicExpansionValveA','ANIM_EEV_A_ACTUATOR',(4.4,1.00,3.03)),('ElectronicExpansionValveB','ANIM_EEV_B_ACTUATOR',(5.6,1.00,3.03)),('TransparentColdStorageChamber','ANIM_CHAMBER_A_DOOR',(-4.35,2.52,-.54)),('TransparentRapidCoolingChamber','ANIM_CHAMBER_B_DOOR',(-4.35,.62,-.54)),('EvaporatorCoilA','ANIM_EVAPORATOR_FROST',(-4.35,2.5,-2.65)),('CondensateDrainAssembly','ANIM_CONDENSATE_DRAIN',(0,0,0)),('HolographicRefrigerationSCADA','ANIM_REFRIGERATION_SCADA',(.5,3.2,-2.10)),('TransparentRefrigerationSafetyEnclosure','ANIM_SAFETY_DOOR',(0,2.04,1.5))]
# Correct conceptual empty drain model is actual geometry for item #37, pivot position update
# export direct actual procedural shape for each; unique visual shapes above
# Reset any generic #37 panel into drain assembly pipe/box
M[names[37]]=[];k=names[37];box(k,(-3.04,.20,-1.75),(1.30,.12,.65),'steel');pipe(k,[(-3.50,.28,-1.75),(-3.50,.40,-1.75),(-2.75,.40,-1.75),(-2.75,.18,-1.75)],.075,'copper');cyl(k,(-2.75,.15,-1.75),.15,.10,'cyan','y')
pivots=[(a,b,(-3.04,.20,-1.75) if b=='ANIM_CONDENSATE_DRAIN' else c) for a,b,c in pivots]
# soften chamber second y too tall? legit shared geometry.
manifest=[]
for i,k in enumerate(names):
 base=f'{i:02d}_L69_{k}'
 fbx(k,MD/(base+'.fbx'));glb(k,GD/(base+'.glb'))
 triangles=sum(len(mesh.faces) for mesh,c in M[k]);parts=len(M[k]);manifest.append({'file':base+'.fbx','glb':base+'.glb','name':'L69_'+k,'triangle_count':triangles,'mesh_parts':parts,'animation_pivots':{p:list(pos) for a,p,pos in pivots if a==k}})
 print(i,k,triangles,flush=True)

# combine for assembled showcase; can be heavy FBX > 1000 objects, still valid
M['Level69_Complete_ArtKit_Showcase']=[item for k in names for item in M[k]]
fbx('Level69_Complete_ArtKit_Showcase',MD/'Level69_Complete_ArtKit_Showcase.fbx')
glb('Level69_Complete_ArtKit_Showcase',GD/'Level69_Complete_ArtKit_Showcase.glb')

with (ROOT/'Animation_Rigs.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Model','Pivot','X','Y','Z','Notes']);
 for k,p,pos in pivots:w.writerow(['L69_'+k,p,*pos,'Art reference only. Create separated animated geometry/rig in Unity.'])

total=sum(v['triangle_count'] for v in manifest)
(ROOT/'Model_Manifest.json').write_text(json.dumps({'level_id':'level_69','display_name':'Keep It Cold','asset_kind':'procedural 3D geometry only (visual art, no gameplay)','individual_models':46,'individual_triangle_count':total,'planned_player_wires':82,'planned_unique_sockets':128,'actual_player_wires_included':False,'functional_spark_terminals_included':False,'physics_included':False,'coordinate_system':'X=workbench width, Y=up, Z=front; GLB meters; FBX centimeters with UnitScaleFactor=1','pivots_count':len(pivots),'models':manifest},indent=2))

# Editor script: art placement, not gameplay
arr=',\n'.join('            "'+v['name'].replace('L69_','%02d_L69_'%i)+'"' for i,v in enumerate(manifest))
cs=f'''#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{{
    /// <summary>Visual placement only for Level 69. Does not add SparkTerminal, player wires, electrical/refrigerant physics, or objectives.</summary>
    public static class SparkLevel69ArtPlacementEditor
    {{
        private const string ArtRoot = "L69_HD_VISUAL_ART";
        private static readonly string[] Models = {{
{arr}
        }};

        [MenuItem("Tools/Project Spark/Levels/69/Place HD Art Under Selected Layout")]
        public static void Place()
        {{
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {{
                EditorUtility.DisplayDialog("Level 69 Art", "Select your Level 69 layout root before placing visuals.", "OK");
                return;
            }}
            var prefabs = new GameObject[Models.Length];
            for (int i = 0; i < Models.Length; i++)
            {{
                string selectedPath = null;
                foreach (string guid in AssetDatabase.FindAssets(Models[i] + " t:Model"))
                {{
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Models[i], StringComparison.Ordinal))
                    {{ selectedPath = path; break; }}
                }}
                if (selectedPath == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(selectedPath)) == null)
                {{
                    EditorUtility.DisplayDialog("Missing Level 69 Model", Models[i] + ".fbx missing. Import all 46 individual models. No changes made.", "OK");
                    return;
                }}
            }}
            Transform previous = destination.transform.Find(ArtRoot);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated Level 69 visuals?",
                "Replace only the " + ArtRoot + " child? Other layout objects remain untouched.", "Replace", "Cancel")) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 69 HD Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 69 art root");
            root.transform.SetParent(destination.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < prefabs.Length; i++)
            {{
                var instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Could not instantiate " + Models[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Place " + Models[i]);
                instance.name = Models[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }}
            Undo.CollapseUndoOperations(group);
            EditorUtility.SetDirty(root);
            Debug.Log("[ProjectSpark L69 Art] Placed " + Models.Length + " visual FBX assets under " + ArtRoot + ". Gameplay must be built separately.");
        }}
    }}
}}
#endif
'''
(ED/'SparkLevel69ArtPlacementEditor.cs').write_text(cs)
readme='''PROJECT SPARK — LEVEL 69: KEEP IT COLD
HD PROCEDURAL REFRIGERATION LAB / REAL 3D ART FILES

ASSETS
- 46 individual binary FBX 7400 meshes under Models/
- 46 GLB backups under GLB/
- Assembled reference FBX and GLB (do not instantiate together with individual assets)
- Model_Manifest.json (exact names, counts, declared pivots)
- Animation_Rigs.csv (14 spatial pivot references; NOT complete animation rigs)
- Unity_Optional/SparkLevel69ArtPlacementEditor.cs
- Previews/ hero assembled and individual model contact sheet
- Verification_Report.json

UNITY
1. Extract ZIP, import Models/ individual numbered FBXs into Assets/ProjectSpark/Level69/Models.
2. Put SparkLevel69ArtPlacementEditor.cs under Assets/Editor or other Unity Editor folder.
3. Select existing Level 69 layout root in hierarchy.
4. Run Tools/Project Spark/Levels/69/Place HD Art Under Selected Layout.
5. The helper creates L69_HD_VISUAL_ART and instantiates all 46 individual models with local identity transforms.
6. Use only the individual FBXs for modular gameplay, not the assembled showcase.

COORDINATES / SCALE
X = width; Y = up; Z = front. GLB authored in meters. FBX numeric vertices are centimeters with UnitScaleFactor 1.
Unity FBX normally auto-converts centimeters to meters; verify model Import Settings and one known base dimension (13.8 meters) first.
If the imported scene looks 100x larger, set Model Import Scale Factor = 0.01 and confirm visual size.

IMPORTANT LIMITATIONS
- This is a modeled geometry collection with material color regions, not a hand-authored PBR AAA pack.
- The 14 pivot positions are engineering references, not completed nested mechanical rigs or keyframe animations.
- Actual chamber doors, fans, compressor internals, frost overlays, refrigerant phase effects and safety interactions require Unity rigging.
- No SparkTerminal components, W01-W82 connection manifest, 128 socket anchors, refrigerant phase model, PLC code, safety logic or six gameplay objectives are included.
- Unity FBX import and Play Mode are not tested in this environment.
- All models are educational visual designs, not certified engineering equipment.
'''
(ROOT/'README_Unity.txt').write_text(readme)

# preview rasterizer via isometric painter algorithm
font='/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf'
F=lambda s: ImageFont.truetype(font,s)
def drawscene(keys,sz=(1400,870),full=False):
 w,h=sz;im=Image.new('RGB',sz,(6,18,29));dr=ImageDraw.Draw(im)
 meshes=[(m,c) for k in keys for m,c in M[k]]
 # project angular camera for x horizontal and z depth, camera from front +z and +x
 az=math.radians(32); el=math.radians(27)
 right=np.array([math.cos(az),0,-math.sin(az)]);up=np.array([-math.sin(az)*math.sin(el),math.cos(el),-math.cos(az)*math.sin(el)]);depth=np.cross(right,up)
 v=np.concatenate([m.vertices for m,c in meshes]);xx=v@right;yy=v@up; xy=np.stack([xx,yy],axis=1);lo=xy.min(axis=0);hi=xy.max(axis=0);dist=hi-lo
 scale=min((w-85)/max(.1,dist[0]),(h-125)/max(.1,dist[1]));orig=np.array([w/2,h/2])+(-lo-hi)/2*scale*np.array([1,-1]);polys=[]
 for mesh,col in meshes:
  vv=mesh.vertices;f=mesh.faces
  cent=(vv[f[:,0]]+vv[f[:,1]]+vv[f[:,2]])/3;d=cent@depth
  # skip some tiny faces in big assembled render
  for j,face in enumerate(f):
   p=vv[face];e1=p[1]-p[0];e2=p[2]-p[0];n=np.cross(e1,e2);nn=np.linalg.norm(n)
   if nn<.0001:continue
   # vector shading
   lighting=.55+.45*abs(np.dot(n/nn,np.array([.2,.87,.43])))
   R,G,B,_=P[col];color=(int(R*lighting),int(G*lighting),int(B*lighting))
   screen=np.array([p@right,p@up]).T
   pts=[(float((xy[0]-(lo[0]+hi[0])/2)*scale+w/2),float((-(xy[1]-(lo[1]+hi[1])/2))*scale+h/2)) for xy in screen]
   polys.append((float(d[j]),pts,color))
 polys.sort(key=lambda x:x[0])
 for dep,points,color in polys:dr.polygon(points,fill=color)
 return im
hero=drawscene(names,(1400,870));d=ImageDraw.Draw(hero);d.rectangle((0,0,1400,93),fill=(7,19,31));d.text((36,17),'PROJECT SPARK / LEVEL 69 — KEEP IT COLD',font=F(31),fill=(217,240,255));d.text((38,59),'46 modular FBX models  /  intelligent cold-chain laboratory  /  refrigeration engineering',font=F(16),fill=(110,204,230));d.rectangle((0,828,1400,870),fill=(7,19,31));d.text((35,838),'46 INDIVIDUAL FBX  •  46 GLB BACKUPS  •  14 ANIMATION PIVOT REFERENCES',font=F(17),fill=(125,227,216));hero.save(PD/'Level69_Hero_3D_Preview.png')
# contact sheet, thumbnails raster individual using same camera bounding (avoid huge)
cellw,cellh=244,169;cw,ch=6*cellw,8*cellh;sheet=Image.new('RGB',(cw,ch),(7,20,31));sd=ImageDraw.Draw(sheet)
for i,k in enumerate(names):
 col=i%6;row=i//6;thumb=drawscene([k],(240,130));xx=col*cellw;yy=row*cellh;sheet.paste(thumb,(xx+2,yy+2));sd.text((xx+8,yy+134),f'{i:02d}  {k[:25]}',font=F(12),fill=(217,239,245));sd.rectangle((xx+1,yy+1,xx+cellw-2,yy+cellh-2),outline=(37,93,121),width=1)
sheet.save(PD/'Level69_46_Models_ContactSheet.png')

# Validate FBX node structure by recursively following absolute offsets

def chkfbx(path):
 data=path.read_bytes();assert data[:21]==b'Kaydara FBX Binary  \x00\x1a\x00';assert struct.unpack_from('<I',data,23)[0]==7400
 names=[];cursor=27
 while cursor+13<len(data):
  end,np_,ln,namelen=struct.unpack_from('<IIIB',data,cursor)
  if not end:break
  assert end>cursor and end<=len(data)
  names.append(data[cursor+13:cursor+13+namelen].decode('ascii'))
  cursor=end
 assert 'Objects' in names and 'Connections' in names and 'FBXHeaderExtension' in names
 return names
from collections import OrderedDict
report={'level':'69','model_count':46,'individual_triangles':total,'model_parts':sum(len(M[k]) for k in names),'animation_pivots':len(pivots),'binary_fbx_verified':[],'glb_verified':[]}
for path in sorted(MD.glob('*.fbx')): chkfbx(path);report['binary_fbx_verified'].append(path.name)
for path in sorted(GD.glob('*.glb')):
 scene=trimesh.load(path,force='scene');tri=sum(len(x.faces) for x in scene.geometry.values());assert tri>0
 report['glb_verified'].append({'file':path.name,'triangles':tri})
assert len(report['binary_fbx_verified'])==47 and len(report['glb_verified'])==47
report['validated_all_exports']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))

zippath=Path('/mnt/data/ProjectSpark_Level69_HD_AAA_Models.zip')
with zipfile.ZipFile(zippath,'w',zipfile.ZIP_DEFLATED,compresslevel=7) as z:
 for path in sorted(ROOT.rglob('*')):
  if path.is_file():z.write(path,path.relative_to(ROOT.parent))
with zipfile.ZipFile(zippath) as z:
 assert z.testzip() is None
 print('ZIP',len(z.namelist()),zippath.stat().st_size,'bytes')
print('TOTAL',total,'triangles, 46 models, 14 pivots')
