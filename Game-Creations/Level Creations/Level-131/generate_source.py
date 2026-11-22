import os,struct,zlib,math,csv,json,zipfile,random
import numpy as np
import trimesh as t
from pathlib import Path
R=Path(__file__).resolve().parent; A=R/'Assets/ProjectSpark/Level131/B760MSteelLegend'; M=A/'Models'; M.mkdir(parents=True,exist_ok=True)
colors={'PCB':(.18,.20,.21),'Silver':(.70,.73,.75),'Steel':(.42,.45,.48),'Black':(.035,.039,.045),'White':(.83,.85,.86),'Gold':(.8,.58,.19),'Copper':(.52,.25,.10),'Gray':(.27,.29,.31),'Blue':(.04,.22,.65),'Red':(.55,.05,.08),'Green':(.08,.32,.16),'Solder':(.58,.60,.63)}
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
start('PCB',122,122,'Board')
# actual through holes using polygon extrusion via triangulation of clipped grid via shapely
from shapely.geometry import Polygon,Point
from shapely.ops import triangulate
holes=[(8,8),(236,8),(8,236),(236,236),(8,115),(236,115),(122,236),(122,8)]
poly=Polygon([(-122,-122),(122,-122),(122,122),(-122,122)])
for x,z in holes:poly=poly.difference(Point(x-122,z-122).buffer(1.7,quad_segs=12))
tris=[np.asarray(q.exterior.coords)[:3] for q in triangulate(poly) if poly.covers(q)]
v=[];f=[]
for tri in tris:
 k=len(v);v.extend([(x,-1.6,z) for x,z in tri]+[(x,0,z) for x,z in tri]);f.extend([(k,k+1,k+2),(k+5,k+4,k+3)])
for loop in [poly.exterior,*poly.interiors]:
 coords=list(loop.coords)
 for (x,z),(a,b) in zip(coords,coords[1:]):
  k=len(v);v.extend([(x,-1.6,z),(a,-1.6,b),(a,0,b),(x,0,z)]);f.extend([(k,k+1,k+2),(k,k+2,k+3)])
q=t.Trimesh(vertices=v,faces=f,process=True);q.fix_normals();add('PCB',q)
for x,z in holes:ring('Gold',x-122,.04,z-122,3.1,1.7,.08)
# schematic decorative surface copper routes; not genuine netlist
random.seed(131)
for i in range(100):
 x=random.uniform(-100,100);z=random.uniform(-110,110);l=random.uniform(2,12)
 box('Gray',x,.025,z,l,.05,.12);box('Gray',x+l/2,.025,z+2,.12,.05,4)
# LGA socket
start('CPU_LGA1700',121,84,'Socket');box('Black',0,1,0,49,2,67);frame('Steel',0,4,0,53,4,73,3);frame('Silver',0,7,0,47,2,64,2.5);box('Gray',0,2.5,0,41,1,56)
for i in range(34):
 for j in range(50):
  cyl('Gold',(i-16.5)*1.04,3.1,(j-24.5)*1.03,.19,.7,6)
for x in [-23,23]:
 for z in [-33,33]:cyl('Silver',x,7,z,2.4,2.2)
box('Silver',29,5,4,2,2,73);box('Silver',25,5,40,10,2,2)
for i,n in enumerate(['DDR5_A1','DDR5_A2','DDR5_B1','DDR5_B2']):
 start(n,175+i*9.6,83,'Memory');frame('Black',0,5,0,6.8,10,141,1.2);box('Black',0,1,0,6.8,2,141)
 for j in range(144):
  for s in [-1,1]:box('Gold',s*1.8,4,(j-71.5)*.94,.33,4,.28)
 for s in [-1,1]:box('Gray',0,6,s*71,7,12,5)
 box('Black',0,5,5,5,9,1.2)
power('ATX12V1',48,10,4);power('ATX12V2',68,10,4);power('ATXPWR1',234,70,12,True)
for n,x,z,w,count in [('PCIE1',89,164,92,82),('PCIE2',56,204,27,18)]:
 start(n,x,z,'Expansion');frame('Black',0,5,0,w,10,8,1.4);box('Black',0,1,0,w,2,8)
 if n=='PCIE1':frame('Silver',0,6,0,w+1,7,9,.6)
 for i in range(count):
  for s in [-1,1]:box('Gold',(i-(count-1)/2)*(w-5)/count,4,s*2,.32,5,.35)
 box('Black',w/2+2,5,0,4,10,8)
for n,x,z in [('M2_1',70,142),('M2_2',212,225),('M2_3',52,225),('M2_WIFI',126,183)]:
 start(n,x,z,'Storage' if n!='M2_WIFI' else 'Wireless');frame('Black',0,1.8,0,5,3.6,22,.7)
 for j in range(32):box('Gold',0,1.9,(j-15.5)*.6,2.8,1,.22)
for i,(x,z) in enumerate([(130,142),(146,142),(112,220),(128,220),(144,220),(132,183)]):
 start('M2_STANDOFF_%02d'%i,x,z,'Hardware');ring('Gold',0,1.5,0,2,0.8,3,16)
start('CMOS_BATTERY',65,184,'Battery');cyl('Black',0,1,0,11,2);cyl('Silver',0,2.8,0,10,3.2,64);ring('Steel',0,4.5,0,10,9.4,.5,64);box('Gold',11,2,0,3,2,5)
start('B760_CHIPSET',195,186,'Chipset');box('Green',0,.7,0,27,1.4,27);box('Black',0,2,0,19,1.8,19);box('Steel',0,3.2,0,12,1,12)
sink('CHIPSET_HEATSINK',195,186,34,33,5);sink('VRM_TOP_HEATSINK',121,26,75,24,10);sink('VRM_LEFT_HEATSINK',85,78,25,68,12);sink('M2_1_HEATSINK',112,142,86,24,3)
for i in range(14):
 x,z=(94+i*7.5,37) if i<8 else (89,48+(i-8)*11)
 start('VRM_CHOKE_%02d'%i,x,z,'VRM');box('Gray',0,3,0,6,6,6);box('Steel',0,6.1,0,5,.2,5)
 start('DRMOS_%02d'%i,x+(0 if i<8 else -8),z+(-8 if i<8 else 0),'VRM');box('Black',0,1,0,5,2,5)
 for j in range(5):
  for s in [-1,1]:box('Solder',s*2.7,.3,(j-2)*.85,1,.6,.4)
for i in range(16):
 x,z=(95+i*6,46) if i<9 else (97,59+(i-9)*10)
 start('POLYMER_CAP_%02d'%i,x,z,'Capacitor');cyl('Black',0,3,0,2.5,6,32);cyl('Silver',0,6.1,0,2.45,.3,32);box('Gray',0,6.3,0,3,.1,.2)
for i in range(4):
 start('SATA3_%d'%(i+1),234,204+i*8,'Storage');frame('Black',0,3,0,13,6,7,1);box('Black',0,1,0,13,2,7)
 for j in range(7):box('Gold',-2+j*.65,2.4,0,.35,.5,4)
# rear ports face out of board at x=- edge; hollow opening made from strips
for n,z,w,h,rows,inner in [('DP1',13,16,10,1,'Black'),('HDMI1',37,16,10,1,'Black'),('ANTENNA_USB2',58,20,24,2,'Black'),('USB32_12',81,17,21,2,'Blue'),('USB32_3_TC1',104,17,19,2,'Red'),('LAN_USB32_45',127,25,24,2,'Blue'),('AUDIO',146,22,15,1,'Black')]:
 start(n,8,z,'RearIO');box('Silver',0,1,0,16,2,w)
 for s in [-1,1]:box('Steel',0,h/2,s*(w/2-.5),16,h,1)
 box('Silver',0,h,0,16,1,w);box('Gray',7,h/2,0,1,h,w)
 if n=='AUDIO':
  for j in range(3):ring(['Blue','Green','Red'][j],-8,7,(j-1)*7,3.2,2.0,2,24,'x')
 else:
  for j in range(rows):
   y=4+j*9;box('Silver',0,y-2,0,16,1,w);box(inner,-3,y,0,10,1.3,w-4)
   for k in range(8):box('Gold',-3,y+.8,(k-3.5)*(w-5)/8,5,.3,.45)
 if n=='ANTENNA_USB2':
  for s in [-1,1]:ring('Gold',-9,19,s*5,3,1.3,5,32,'x')
# upper/right/bottom headers based on manual
for n,x,z,c,r,rot in [('CPU_FAN1',213,7,4,1,False),('CPU_FAN2_WP',225,7,4,1,False),('ADDR_LED2',232,20,3,1,True),('ADDR_LED3',232,33,3,1,True),('USB32_4_5',236,111,10,2,True),('USB_C_FRONT',234,132,10,2,True),('CHA_FAN3_WP',236,146,4,1,True),('PANEL1',226,238,5,2,False),('SPEAKER',215,238,4,2,False),('SPI_TPM',181,238,6,2,False),('USB_5_6',152,238,5,2,False),('USB_3_4',133,238,5,2,False),('CHA_FAN2_WP',107,238,4,1,False),('CHA_FAN1_WP',87,238,4,1,False),('RGB_LED1',70,238,4,1,False),('ADDR_LED1',56,238,3,1,False),('CLRCMOS1',44,238,2,1,False),('HD_AUDIO1',25,238,5,2,False),('EDP',234,155,8,1,True)]:pins(n,x,z,c,r,rot=rot)
for n,x,z in [('BIOS_ROM',202,237),('LAN_RTL8125BG',38,143),('AUDIO_ALC897',17,224),('SUPER_IO',165,204)]:
 start(n,x,z,'IC');box('Black',0,1,0,7,2,7)
 for j in range(10):
  for s in [-1,1]:box('Solder',s*4,.3,(j-4.5)*.55,1.5,.6,.25)
# deterministic illustrative passives placed only in conservative free zones
for i in range(100):
 x=random.uniform(20,230);z=random.uniform(20,230)
 if not ((25<x<65 and 155<z<175) or (145<x<165 and 60<z<115) or (155<x<175 and 175<z<220) or (20<x<42 and 175<z<230) or (68<x<150 and 212<z<230)):continue
 start('SMD_%03d'%i,x,z,'Passive');box('Black' if i%2 else 'White',0,.5,0,2.2,1,1.2)
 for s in [-1,1]:box('Solder',s*1.0,.5,0,.6,1,1.2)
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
  mesh=t.util.concatenate(meshes); entries.append((name,mat,mesh,(0,0,0)));allentries.append((name,mat,mesh,(x-122,0,z-122)))
 tris=export(M/('L131_'+name+'.fbx'),entries);report.append((name,x,z,cat,tris))
 # OBJ fallback in metres, with named materials
 O=A/'FallbackOBJ';O.mkdir(exist_ok=True); lines=['mtllib L131.mtl'];base=1
 for _,mat,mesh,_ in entries:
  lines+=['o '+name+'_'+mat,'usemtl '+mat];lines+=['v %.8f %.8f %.8f'%tuple(v*.001) for v in mesh.vertices];lines+=['f %d %d %d'%tuple(f+base) for f in mesh.faces];base+=len(mesh.vertices)
 (O/('L131_'+name+'.obj')).write_text('\n'.join(lines))
(O/'L131.mtl').write_text('\n'.join('newmtl '+k+'\nKd '+' '.join(map(str,v))+'\nKs .3 .3 .3\nNs 35\n' for k,v in colors.items()))
tri=export(M/'L131_ASSEMBLED.fbx',allentries)
with (A/'placements.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['id','x_mm','z_mm','category','triangles']);w.writerows(report)
(A/'materials.json').write_text(json.dumps(colors,indent=2)); (A/'board.json').write_text(json.dumps({'level':131,'board':'ASRock B760M Steel Legend WiFi','width_mm':244,'depth_mm':244,'origin':'board center','layout':'approximate visual interpretation','components':len(report),'triangles':tri},indent=2))
print(len(report),tri)
