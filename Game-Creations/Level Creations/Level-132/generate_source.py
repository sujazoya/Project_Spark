import os,struct,zlib,math,csv,json,zipfile,random
import numpy as np
import trimesh as t
from pathlib import Path
R=Path(__file__).resolve().parent; A=R/'Assets/ProjectSpark/Level32/B850ILightning'; M=A/'Models'; M.mkdir(parents=True,exist_ok=True)
colors={'PCB':(.035,.038,.04),'Silver':(.36,.38,.40),'Steel':(.42,.45,.48),'Black':(.035,.039,.045),'White':(.83,.85,.86),'Gold':(.8,.58,.19),'Copper':(.52,.25,.10),'Gray':(.27,.29,.31),'Blue':(.04,.22,.65),'Red':(.55,.05,.08),'Green':(.08,.32,.16),'Solder':(.58,.60,.63)}
parts={}; placements=[]; cur=None
# coordinates in mm relative to top left, y up; local asset origin at board surface
def start(name,x,z,category):
 global cur
 cur=name; parts[name]={}; placements.append((name,x,z,category))
def add(mat,mesh): parts[cur].setdefault(mat,[]).append(mesh)
def box(mat,x,y,z,w,h,d):
 q=t.creation.box(extents=[w,h,d]); q.apply_translation([x,y,z]); add(mat,q)
def cyl(mat,x,y,z,r,h,n=24,axis='y'):
 q=t.creation.cylinder(radius=r,height=h,sections=n)
 if axis=='y': q.apply_transform(t.transformations.rotation_matrix(-math.pi/2,[1,0,0]))
 elif axis=='x': q.apply_transform(t.transformations.rotation_matrix(math.pi/2,[0,1,0]))
 q.apply_translation([x,y,z]); add(mat,q)
def ring(mat,x,y,z,r,inner,h,n=32,axis='y'):
 q=t.creation.annulus(r_min=inner,r_max=r,height=h,sections=n)
 if axis=='y': q.apply_transform(t.transformations.rotation_matrix(-math.pi/2,[1,0,0]))
 elif axis=='x': q.apply_transform(t.transformations.rotation_matrix(math.pi/2,[0,1,0]))
 q.apply_translation([x,y,z]); add(mat,q)
def frame(mat,x,y,z,w,h,d,b):
 for s in [-1,1]:
  box(mat,x+s*(w-b)/2,y,z,b,h,d);box(mat,x,y,z+s*(d-b)/2,w-2*b,h,b)
def pins(name,x,z,cols,rows,category='Header',pitch=2.54,rot=False):
 start(name,x,z,category); w=cols*pitch+1; d=rows*pitch+1
 box('Black',0,1,0,d if rot else w,2,w if rot else d)
 for i in range(cols):
  for j in range(rows):
   xx=(i-(cols-1)/2)*pitch; zz=(j-(rows-1)/2)*pitch
   if rot: xx,zz=zz,xx
   box('Gold',xx,3.3,zz,.62,5,.62)
def power(name,x,z,cols,rot=False):
 start(name,x,z,'Power'); p=4.2; w=cols*p+1.8; d=10.2
 if rot:w,d=d,w
 box('Black',0,1,0,w,2,d);frame('Black',0,5.8,0,w,8,d,1.0)
 for i in range(cols):
  for j in range(2):
   a=(i-(cols-1)/2)*p;b=(j-.5)*p
   if rot:a,b=b,a
   frame('Black',a,6,b,3.9,6,3.9,.45);box('Gold',a,4.5,b,1,6,1)
 box('Black',0,7,d/2+1,w*.45,4,2)
def sink(name,x,z,w,d,h):
 start(name,x,z,'Heatsink');box('Steel',0,2,0,w,4,d)
 for i in range(int(d/3)):
  zz=-d/2+1.5+i*3;box('Silver',0,4+h/2,zz,w-.9,h,1.5)
 box('Silver',0,4+h,0,w,1.4,d);box('Gray',0,5+h,0,w*.72,.6,d*.18)
 for a in [-1,1]:cyl('Steel',a*(w/2-3),5+h,0,1.6,1.1)
start('PCB',85,85,'Board')
# actual through holes using polygon extrusion via triangulation of clipped grid via shapely
from shapely.geometry import Polygon,Point
from shapely.ops import triangulate
holes=[(6,6),(163,6),(6,163),(163,163)]
poly=Polygon([(-85,-85),(85,-85),(85,85),(-85,85)])
for x,z in holes:poly=poly.difference(Point(x-85,z-85).buffer(1.7,resolution=12))
tris=[np.asarray(q.exterior.coords)[:3] for q in triangulate(poly) if poly.covers(q)]
v=[];f=[]
for tri in tris:
 k=len(v);v.extend([(x,-1.6,z) for x,z in tri]+[(x,0,z) for x,z in tri]);f.extend([(k,k+1,k+2),(k+5,k+4,k+3)])
for loop in [poly.exterior,*poly.interiors]:
 coords=list(loop.coords)
 for (x,z),(a,b) in zip(coords,coords[1:]):
  k=len(v);v.extend([(x,-1.6,z),(a,-1.6,b),(a,0,b),(x,0,z)]);f.extend([(k,k+1,k+2),(k,k+2,k+3)])
q=t.Trimesh(vertices=v,faces=f,process=True);q.fix_normals();add('PCB',q)
for x,z in holes:ring('Gold',x-85,.04,z-85,3.1,1.7,.08)
# schematic decorative surface copper routes; not genuine netlist
random.seed(131)
for i in range(100):
 x=random.uniform(-77,77);z=random.uniform(-77,77);l=random.uniform(2,12)
 box('Gray',x,.025,z,l,.05,.12);box('Gray',x+l/2,.025,z+2,.12,.05,4)

start('CPU_AM5',84,63,'Socket');box('Black',0,1,0,48,2,49);frame('Steel',0,4,0,58,4,68,3);frame('Silver',0,7,0,48,2,50,2.5);box('Gray',0,2.5,0,43,1,43)
for i in range(42):
 for j in range(41):
  if i in (0,41) and j in (0,40):continue
  cyl('Gold',(i-20.5)*.96,3.1,(j-20)*.96,.18,.65,6)
box('Steel',31,5,0,2,2,64);box('Steel',27,5,33,10,2,2)
for sign in [-1,1]:
 start('AM5_BRACKET_'+str(sign),84,63+sign*39,'Hardware');box('Black',0,3,0,64,6,12)
 for x in [-27,27]:cyl('Steel',x,6.5,0,3,2)
for i,n in enumerate(['DDR5_A1','DDR5_B1']):
 start(n,140+i*10,77,'Memory');frame('Black',0,5,0,6.8,10,137,1.2);box('Black',0,1,0,6.8,2,137)
 for j in range(144):
  for s in [-1,1]:box('Gold',s*1.8,4,(j-71.5)*.94,.33,4,.28)
 for s in [-1,1]:box('Gray',0,6,s*69,7,12,5)
 box('Black',0,5,5,5,9,1.2)

power('ATX12V1',21,8,4);power('ATXPWR1',162,36,12,True)
start('PCIE1',91,164,'Expansion');frame('Black',0,5,0,101,10,8,1.2);frame('Silver',0,6,0,102,7,9,.6)
for i in range(82):
 for sign in [-1,1]:box('Gold',(i-40.5)*1.15,4,sign*2,.32,5,.35)
box('Black',53,5,0,4,10,8)
for n,x,z,back in [('M2_1',46,143,False),('M2_2',50,145,True)]:
 start(n,x,z,'BackStorage' if back else 'Storage');frame('Black',0,2,0,5,4,22,.7)
 for j in range(32):box('Gold',0,2,(j-15.5)*.6,2.8,1,.22)
 start(n+'_STANDOFF',x+78,z,'BackStorage' if back else 'Hardware');ring('Gold',0,1.5,0,2,.8,3,24)
start('B850_CHIPSET',91,144,'Chipset');box('Green',0,.7,0,23,1.4,23);box('Black',0,2,0,18,2,18);box('Steel',0,3.2,0,12,1,12)
sink('M2_1_HEATSINK',86,143,86,24,8);sink('VRM_LEFT_HEATSINK',39,64,24,102,17)
start('VRM_ARMOR',36,64,'Heatsink');box('Black',0,22,0,30,3,102)
for i in range(7):box('Red',0,23.6,-42+i*4,22,.2,1.2)
for i in range(12):
 x,z=43,21+i*7.8
 start('VRM_CHOKE_%02d'%i,x,z,'VRM');box('Gray',0,3,0,6,6,6);box('Steel',0,6.1,0,5,.2,5)
 start('DRMOS_%02d'%i,x-8,z,'VRM');box('Black',0,1,0,5,2,5)
 for j in range(6):
  for sign in [-1,1]:box('Solder',sign*2.7,.3,(j-2.5)*.7,1,.6,.35)
 start('POLYMER_CAP_%02d'%i,x+8,z,'Capacitor');cyl('Black',0,3,0,2.5,6,32);cyl('Silver',0,6.1,0,2.45,.3,32)
for i in range(2):
 start('SATA3_%d'%(4+i),164,72+i*17,'Storage');frame('Black',0,3,0,7,6,13,1)
 for j in range(7):box('Gold',0,2.4,-2+j*.65,4,.5,.35)
for n,z,w,h,rows,inner in [('LAN_USB_C',26,18,32,3,'Blue'),('HDMI_USB_A',62,19,28,3,'Red'),('USB_1234',87,18,36,4,'Black'),('AUDIO',108,17,32,1,'Black'),('BIOS_FB1',122,7,8,1,'Black'),('ANTENNA',135,12,30,1,'Black')]:
 start(n,7,z,'RearIO');box('Steel',0,1,0,16,2,w)
 for sign in [-1,1]:box('Steel',0,h/2,sign*(w/2-.5),16,h,1)
 box('Steel',0,h,0,16,1,w);box('Gray',7,h/2,0,1,h,w)
 if n=='AUDIO':
  for j in range(2):ring('Gold',-8,15+j*9,0,3.5,1.8,2,32,'x')
  box('Black',-7,5,0,3,7,8)
 elif n=='ANTENNA':
  for j in range(2):ring('Gold',-9,9+j*14,0,3,1.3,5,32,'x')
 else:
  for j in range(rows):
   y=4+j*8;box('Silver',0,y-2,0,16,1,w);box(inner,-3,y,0,10,1.3,w-4)
   for k in range(8):box('Gold',-3,y+.8,(k-3.5)*(w-5)/8,5,.3,.45)
for n,x,z,c,r,rot in [('T_SENSOR1',80,5,2,1,False),('ADDR_LED1',93,5,3,1,False),('ADDR_LED2',106,5,3,1,False),('CPU_FAN1',119,5,4,1,False),('AIO_PUMP',131,5,4,1,False),('CHA_FAN1',31,149,4,1,True),('USB32_4_5',164,112,10,2,True),('PANEL1',165,134,5,2,True),('USB2_1',158,134,5,2,True),('USB32_TC2',162,150,8,2,True),('CLRCMOS1',151,165,2,1,False),('HD_AUDIO1',10,148,5,2,True),('SPK_PLED1',5,148,4,1,True)]:pins(n,x,z,c,r,rot=rot)
for n,x,z in [('BIOS_ROM',12,125),('AUDIO_ALC1220',21,149),('PWM_RAA229620',29,116),('LAN_E3100G',20,36)]:
 start(n,x,z,'IC');box('Black',0,1,0,6,2,6)
 for j in range(8):
  for sign in [-1,1]:box('Solder',sign*3.5,.3,(j-3.5)*.65,1,.6,.25)
start('M2_WIFI_1',17,135,'Wireless');box('Green',0,8,0,28,16,1.2);box('Steel',0,8,1,20,12,2)
for ix in range(22):
 for iz in range(9):
  x=55+ix*3.4;z=113+iz*2.4
  if 75<x<107 and z>130:continue
  start('SMD_%02d_%02d'%(ix,iz),x,z,'Passive');box('Gray' if ix%2 else 'Black',0,.4,0,1.6,.8,.8)
  for sign in [-1,1]:box('Solder',sign*.8,.4,0,.4,.8,.8)
for i in range(40):
 start('BACK_SMD_%02d'%i,62+(i%10)*3,48+(i//10)*3,'BackStorage');box('Black',0,.4,0,1.5,.8,.8)
 for sign in [-1,1]:box('Solder',sign*.75,.4,0,.3,.8,.8)
for name,x,z,cat in placements:
 if cat=='BackStorage':
  for meshes in parts[name].values():
   for mesh in meshes:mesh.apply_transform(np.diag([1,-1,1,1]));mesh.apply_translation([0,-1.6,0])
# Binary FBX 7400 writer. FBX coordinate unit = cm, Y up.
def S(x):return b'S'+struct.pack('<I',len(x.encode()))+x.encode()
def I(x):return b'I'+struct.pack('<i',x)
def L(x):return b'L'+struct.pack('<q',x)
def D(x):return b'D'+struct.pack('<d',x)
def Aarr(x,k):
 a=np.asarray(x,dtype={'d':'<f8','i':'<i4'}[k]).ravel(); raw=a.tobytes(); c=zlib.compress(raw);return k.encode()+struct.pack('<III',len(a),1,len(c))+c
def N(n,p=(),c=()):return (n,list(p),list(c))
def serialize(n,offset):
 name,p,c=n; name=name.encode();p=b''.join(p);head=13+len(name)+len(p);buf=b''
 for child in c:buf+=serialize(child,offset+head+len(buf))
 if c:buf+=b'\0'*13
 return struct.pack('<IIIB',offset+head+len(buf),len(n[1]),len(p),len(name))+name+p+buf
def P(name,typ,*v):return N('P',[S(name),S(typ),S(''),S('A')]+[D(x) if isinstance(x,float) else I(x) for x in v])
def export(path, entries):
 objs=[];con=[];uid=1000; mats={}; total=0
 for name,mat,mesh,offset in entries:
  uid+=1; gid=uid;uid+=1;mid=uid
  if mat not in mats:
   uid+=1;mats[mat]=uid; rgb=colors[mat]
   objs.append(N('Material',[L(uid),S('Material::'+mat),S('')],[N('Version',[I(102)]),N('ShadingModel',[S('phong')]),N('Properties70',c=[P('DiffuseColor','Color',*rgb),P('SpecularColor','Color',.35,.35,.35),P('Shininess','double',35.)])]))
  v=np.asarray(mesh.vertices)*.1; f=np.asarray(mesh.faces).copy(); f[:,-1]=-f[:,-1]-1; normals=np.repeat(mesh.face_normals,3,axis=0); total+=len(f)
  layer=N('LayerElementNormal',[I(0)],[N('Version',[I(101)]),N('Name',[S('')]),N('MappingInformationType',[S('ByPolygonVertex')]),N('ReferenceInformationType',[S('Direct')]),N('Normals',[Aarr(normals,'d')])]);ml=N('LayerElementMaterial',[I(0)],[N('Version',[I(101)]),N('Name',[S('')]),N('MappingInformationType',[S('AllSame')]),N('ReferenceInformationType',[S('IndexToDirect')]),N('Materials',[Aarr([0],'i')])]);ly=N('Layer',[I(0)],[N('Version',[I(100)]),*[N('LayerElement',c=[N('Type',[S(k)]),N('TypedIndex',[I(0)])]) for k in ['LayerElementNormal','LayerElementMaterial']]])
  objs.append(N('Geometry',[L(gid),S('Geometry::'+name+'_'+mat),S('Mesh')],[N('GeometryVersion',[I(124)]),N('Vertices',[Aarr(v,'d')]),N('PolygonVertexIndex',[Aarr(f,'i')]),layer,ml,ly]));objs.append(N('Model',[L(mid),S('Model::'+name+'_'+mat),S('Mesh')],[N('Version',[I(232)]),N('Properties70',c=[P('Lcl Translation','Lcl Translation',*[float(x)*.1 for x in offset]),P('Lcl Rotation','Lcl Rotation',0.,0.,0.),P('Lcl Scaling','Lcl Scaling',1.,1.,1.)]),N('Shading',[b'C\x01']),N('Culling',[S('CullingOff')])]))
  for a,b in [(gid,mid),(mid,0),(mats[mat],mid)]:con.append(N('C',[S('OO'),L(a),L(b)]))
 roots=[N('FBXHeaderExtension',c=[N('FBXHeaderVersion',[I(1003)]),N('FBXVersion',[I(7400)]),N('Creator',[S('Project Spark procedural mesh exporter')])]),N('GlobalSettings',c=[N('Version',[I(1000)]),N('Properties70',c=[P('UpAxis','int',1),P('UpAxisSign','int',1),P('FrontAxis','int',2),P('FrontAxisSign','int',-1),P('CoordAxis','int',0),P('CoordAxisSign','int',1),P('UnitScaleFactor','double',1.),P('OriginalUnitScaleFactor','double',1.)])]),N('Documents',c=[N('Count',[I(1)]),N('Document',[L(1),S('Scene'),S('Scene')],[N('RootNode',[L(0)])])]),N('References'),N('Definitions',c=[N('Version',[I(100)]),N('Count',[I(len(objs))]),*[N('ObjectType',[S(k)], [N('Count',[I(sum(o[0]==k for o in objs))])]) for k in ['Model','Geometry','Material']]]),N('Objects',c=objs),N('Connections',c=con)]
 data=b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400)
 for n in roots:data+=serialize(n,len(data))
 data+=b'\0'*13+bytes.fromhex('fabcab09d0c8d466b176fb831cf7267e');data+=b'\0'*((-len(data))%16);data+=struct.pack('<I',7400)+b'\0'*120+bytes.fromhex('f85a8c6adef5d97eece90ce3758f290b');path.write_bytes(data);return total
allentries=[]; report=[]
for name,x,z,cat in placements:
 entries=[]
 for mat,meshes in parts[name].items():
  mesh=t.util.concatenate(meshes); entries.append((name,mat,mesh,(0,0,0)));allentries.append((name,mat,mesh,(x-85,0,z-85)))
 tris=export(M/('L32_'+name+'.fbx'),entries);report.append((name,x,z,cat,tris))
 # OBJ fallback in metres, with named materials
 O=A/'FallbackOBJ';O.mkdir(exist_ok=True); lines=['mtllib L32.mtl'];base=1
 for _,mat,mesh,_ in entries:
  lines+=['o '+name+'_'+mat,'usemtl '+mat];lines+=['v %.8f %.8f %.8f'%tuple(v*.001) for v in mesh.vertices];lines+=['f %d %d %d'%tuple(f+base) for f in mesh.faces];base+=len(mesh.vertices)
 (O/('L32_'+name+'.obj')).write_text('\n'.join(lines))
(O/'L32.mtl').write_text('\n'.join('newmtl '+k+'\nKd '+' '.join(map(str,v))+'\nKs .3 .3 .3\nNs 35\n' for k,v in colors.items()))
tri=export(M/'L32_ASSEMBLED.fbx',allentries)
with (A/'placements.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['id','x_mm','z_mm','category','triangles']);w.writerows(report)
(A/'materials.json').write_text(json.dumps(colors,indent=2)); (A/'board.json').write_text(json.dumps({'level':32,'board':'ASRock B850I Lightning WiFi','width_mm':170,'depth_mm':170,'origin':'board center','layout':'approximate visual interpretation','components':len(report),'triangles':tri},indent=2))
print(len(report),tri)
