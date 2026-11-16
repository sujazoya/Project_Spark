#!/usr/bin/env python3
"""Standalone procedural mesh primitives and original binary FBX 7400/glTF GLB exporters.
Extracted and adapted from the user's Project Spark Level 84 source art generator.
Geometry numeric units are metres internally; FBX vertices exported as centimetres.
This is art-only tech-art generation, not a Unity runtime or a final AAA bake.
"""
import os, csv, json, math, struct, zlib
from collections import defaultdict
import numpy as np
import trimesh
COLORS={}
PIVOT_POS={}
MATERIALS={}
class ModelArt:
 def __init__(self,name):self.name=name;self.raw=[];self.sockets=[]
 def mesh(self,verts,faces,mat='titanium',pivot=None,label='Detail'):
  a=np.asarray(verts,dtype=np.float64); f=np.asarray(faces,dtype=np.int32)
  if len(a)==0 or len(f)==0:return
  self.raw.append((a,f,mat,pivot,label))
 def box(self,p,s,m='titanium',pivot=None,label='MachinedHousing'):
  x,y,z=np.asarray(p,float);dx,dy,dz=np.asarray(s,float)/2
  v=np.array([[x-dx,y-dy,z-dz],[x+dx,y-dy,z-dz],[x+dx,y+dy,z-dz],[x-dx,y+dy,z-dz],[x-dx,y-dy,z+dz],[x+dx,y-dy,z+dz],[x+dx,y+dy,z+dz],[x-dx,y+dy,z+dz]])
  f=np.array([[0,2,1],[0,3,2],[4,5,6],[4,6,7],[0,1,5],[0,5,4],[3,7,6],[3,6,2],[1,2,6],[1,6,5],[0,4,7],[0,7,3]])
  self.mesh(v,f,m,pivot,label)
 def cylinder(self,p,r,h,m='structural',axis='y',seg=18,pivot=None,label='PrecisionCylinder'):
  x,y,z=p;ax={'x':0,'y':1,'z':2}[axis];vv=[]
  for t in (-h/2,h/2):
   for k in range(seg):
    a=2*math.pi*k/seg;v=[0.,0.,0.];v[ax]=t;v[(ax+1)%3]=r*math.cos(a);v[(ax+2)%3]=r*math.sin(a);vv.append((x+v[0],y+v[1],z+v[2]))
  vv += [[x-h/2 if ax==0 else x,y-h/2 if ax==1 else y,z-h/2 if ax==2 else z], [x+h/2 if ax==0 else x,y+h/2 if ax==1 else y,z+h/2 if ax==2 else z]]
  ff=[]
  for k in range(seg):
   n=(k+1)%seg;ff.extend([[k,n,seg+n],[k,seg+n,seg+k],[2*seg,n,k],[2*seg+1,seg+k,seg+n]])
  self.mesh(vv,ff,m,pivot,label)
 def torus(self,p,R,r,m='titanium',axis='z',seg=64,tube=9,pivot=None,label='RingStructure'):
  ax={'x':0,'y':1,'z':2}[axis];i=(ax+1)%3;j=(ax+2)%3;vv=[];ff=[]
  for k in range(seg):
   a=k*2*math.pi/seg
   for l in range(tube):
    b=l*2*math.pi/tube;v=np.zeros(3);v[i]=(R+r*math.cos(b))*math.cos(a);v[j]=(R+r*math.cos(b))*math.sin(a);v[ax]=r*math.sin(b)
    vv.append((np.asarray(p)+v).tolist())
  for k in range(seg):
   for l in range(tube):
    a=k*tube+l;b=k*tube+(l+1)%tube;c=((k+1)%seg)*tube+l;d=((k+1)%seg)*tube+(l+1)%tube
    ff.extend([[a,c,b],[b,c,d]])
  self.mesh(vv,ff,m,pivot,label)
 def beam(self,a,b,r,m='structural',sides=10,pivot=None,label='PrecisionBeam'):
  a=np.asarray(a,float);b=np.asarray(b,float);q=b-a; L=np.linalg.norm(q)
  if L<1e-6:return
  d=q/L; t=np.array([0,1,0]) if abs(d[1])<0.87 else np.array([1,0,0]);u=np.cross(d,t);u/=np.linalg.norm(u);v=np.cross(d,u)
  vert=[];fac=[]
  for p in [a,b]:
   for k in range(sides):
    z=k*math.tau/sides;vert.append((p+r*(math.cos(z)*u+math.sin(z)*v)).tolist())
  for k in range(sides):
   n=(k+1)%sides;fac.extend([[k,n,sides+n],[k,sides+n+n*0,sides+k]]) # n*0 harmless
  # repair second side face triangle: first [k,n,seg+n], next [k,seg+n,seg+k]
  self.mesh(vert,fac,m,pivot,label)
 def textbars(self,p,width,height,count=7,axis='x',m='cyan',pivot=None):
  x,y,z=p
  for j in range(count):
   yy=y-height/2+(j+.5)*height/count
   w=width*(.26+.7*((j*17+5)%11)/11)
   self.box((x,yy,z), (w,height/(count*3),.017),m,pivot,'TelemetryTrace')
 def socket(self,p,aid):
  self.cylinder(p,.065,.042,'gold',axis='z',seg=12,pivot='SOCKET_'+aid,label='SOCKET_'+aid)
  q=(p[0],p[1],p[2]+.035);self.cylinder(q,.040,.014,'dark',axis='z',seg=12,pivot='SOCKET_'+aid,label='SocketInsulator_'+aid)
  self.sockets.append((aid,tuple(p)))

def panel_equipment(a,p,scale=1.0,theme='cyan',special='instrument'):
 x,y,z=p; w=1.02*scale;h=1.17*scale;dep=.68*scale
 a.box((x,y,z),(w,h,dep),'graphite',label='InstrumentCabinet')
 a.box((x,y+.20*scale,z+dep/2+.025*scale),(w*.88,h*.43,.06*scale),'titanium',label='MachinedBezel')
 a.box((x,y+.20*scale,z+dep/2+.064*scale),(w*.79,h*.34,.018*scale),'panel',label='DigitalScreen')
 for row in range(4):
  yy=y+.34*scale-row*.093*scale
  a.box((x-.13*scale,yy,z+dep/2+.080*scale),(.30*scale,.011*scale,.012*scale),theme,label='InstrumentTrace')
  a.box((x+.235*scale,yy,z+dep/2+.083*scale),(.11*scale,.010*scale,.009*scale),('gold' if row%2==0 else 'green'),label='ParameterIndicator')
 for k in range(7):
  dx=(k-3)*.12*scale
  a.box((x+dx,y-.18*scale,z+dep/2+.032*scale),(.065*scale,.038*scale,.08*scale),('gold' if k==2 else 'ceramic'),label='ControlButton')
  a.box((x+dx,y-.31*scale,z+dep/2+.024*scale),(.075*scale,.022*scale,.05*scale),'green' if k%3==0 else 'dark',label='StatusLED')
 for sign in (-1,1):
  xx=x+sign*(w/2-.06*scale)
  for j in range(5):a.box((xx,y-.34*scale+j*.12*scale,z+dep/2+.014*scale),(.045*scale,.042*scale,.022*scale),'structural',label='PanelFastener')
 if special=='optical':
  a.cylinder((x+.30*scale,y+.32*scale,z+dep/2+.15*scale),.12*scale,.15*scale,'glass',axis='z',seg=24,label='OpticalSensorHead')
 if special=='plc':
  for k in range(8):a.box((x+(k-3.5)*.10*scale,y-.40*scale,z+dep/2+.050*scale),(.06*scale,.075*scale,.055*scale),'copper',label='IndustrialIOTerminal')
 if special=='fault':
  a.box((x+.21*scale,y+.30*scale,z+dep/2+.13*scale),(.1*scale,.14*scale,.09*scale),'red',label='GuardedFaultSwitch')

def chunk_model(a,max_shapes=14):
 by=defaultdict(list)
 for verts,faces,mat,pivot,label in a.raw:
  by[(pivot,mat)].append((verts,faces,label))
 result=[]
 for (pivot,mat),shapes in by.items():
  for n in range(0,len(shapes),max_shapes):
   group=shapes[n:n+max_shapes]; nv=[];nf=[];offset=0
   pp=np.asarray(PIVOT_POS[pivot],float) if pivot in PIVOT_POS else None
   if pivot and pivot.startswith('SOCKET_'):
    pp=np.asarray({a_id:coord for a_id,coord in a.sockets}[pivot[7:]],float)
   for v,f,_ in group:
    v=v-pp if pp is not None else v
    nv.append(v);nf.append(f+offset);offset+=len(v)
   vv=np.vstack(nv);ff=np.vstack(nf)
   label=group[0][2].replace(' ','_')
   result.append(dict(pivot=pivot,material=mat,vertices=vv,faces=ff,label=label,part_index=len(result)))
 return result

class FN:
 def __init__(self,name,props=None,children=None):self.name=name;self.props=props or [];self.children=children or []

def ps(s):
 b=str(s).encode('utf-8');return b'S'+struct.pack('<I',len(b))+b

def pl(v):return b'L'+struct.pack('<q',int(v))

def pd(v):return b'D'+struct.pack('<d',float(v))

def pbool(v):return b'C'+bytes([1 if v else 0])

def parr(vals,typ='d'):
 if typ=='d':
  bb=np.asarray(vals,dtype='<f8').tobytes();code=b'd';count=len(vals)
 elif typ=='i':
  bb=np.asarray(vals,dtype='<i4').tobytes();code=b'i';count=len(vals)
 else:raise ValueError(typ)
 compressed=zlib.compress(bb,6)
 return code+struct.pack('<III',count,1,len(compressed))+compressed

def p70(name,tp,sub,flags,vals):
 return FN('P',[ps(name),ps(tp),ps(sub),ps(flags)]+vals)

def model_prop(xyz=(0,0,0)):
 return FN('Properties70',children=[
  p70('Lcl Translation','Lcl Translation','','A',[pd(xyz[0]*100),pd(xyz[1]*100),pd(xyz[2]*100)]),
  p70('Lcl Rotation','Lcl Rotation','','A',[pd(0),pd(0),pd(0)]),
  p70('Lcl Scaling','Lcl Scaling','','A',[pd(1),pd(1),pd(1)])
 ])

def fbx_model(mid,name,kind='Null',xyz=(0,0,0)):
 return FN('Model',[pl(mid),ps(name+'\x00\x01Model'),ps(kind)],children=[FN('Version',[pl(232)]),model_prop(xyz),FN('Shading',[ps('Y')]),FN('Culling',[ps('CullingOff')])])

def fbx_geometry(gid,name,vertices,faces):
 # glTF meshes are in metres. FBX numeric vertex coordinates are centimetres.
 v=(vertices*100.0).reshape(-1)
 ix=np.asarray(faces,dtype=np.int32).copy();ix[:,2]=-ix[:,2]-1;fi=ix.reshape(-1)
 me=FN('LayerElementMaterial',[pl(0)],[FN('Version',[pl(101)]),FN('Name',[ps('')]),FN('MappingInformationType',[ps('AllSame')]),FN('ReferenceInformationType',[ps('IndexToDirect')]),FN('Materials',[parr([0],'i')])])
 layer=FN('Layer',[pl(0)],[FN('Version',[pl(100)]),FN('LayerElement',[ ],[FN('Type',[ps('LayerElementMaterial')]),FN('TypedIndex',[pl(0)])])])
 return FN('Geometry',[pl(gid),ps(name+'\x00\x01Geometry'),ps('Mesh')],[FN('GeometryVersion',[pl(124)]),FN('Vertices',[parr(v,'d')]),FN('PolygonVertexIndex',[parr(fi,'i')]),me,layer])

def fbx_material(mid,name,col):
 r,g,b,_=col
 return FN('Material',[pl(mid),ps('PS_'+name+'\x00\x01Material'),ps('')],[FN('Version',[pl(102)]),FN('ShadingModel',[ps('phong')]),FN('MultiLayer',[pl(0)]),
 FN('Properties70',children=[p70('DiffuseColor','Color','','A',[pd(r),pd(g),pd(b)]),p70('DiffuseFactor','Number','','A',[pd(1.0)]),p70('SpecularColor','Color','','A',[pd(.33),pd(.33),pd(.33)]),p70('Shininess','Number','','A',[pd(32)]),p70('TransparencyFactor','Number','','A',[pd(1-col[3])])])])

def write_node(f,n):
 start=f.tell();name=n.name.encode('utf-8');pb=b''.join(n.props)
 f.write(b'\0'*13);f.write(name);f.write(pb)
 for c in n.children:write_node(f,c)
 if n.children:f.write(b'\0'*13)
 end=f.tell();f.seek(start);f.write(struct.pack('<IIIB',end,len(n.props),len(pb),len(name)));f.seek(end)

def export_fbx(path,chunks,roots,pivot_homes,sockets):
 # (model name, list of chunks, unique named socket nodes): one entire scene (individual or showcase).
 header=FN('FBXHeaderExtension',children=[FN('FBXHeaderVersion',[pl(1003)]),FN('FBXVersion',[pl(7400)]),FN('Creator',[ps('Project Spark Level92 Original AAA-inspired Procedural Modular Asset Builder')])])
 settings=FN('GlobalSettings',children=[FN('Version',[pl(1000)]),FN('Properties70',children=[
  p70('UpAxis','int','Integer','',[pl(1)]),p70('UpAxisSign','int','Integer','',[pl(1)]),
  p70('FrontAxis','int','Integer','',[pl(2)]),p70('FrontAxisSign','int','Integer','',[pl(1)]),
  p70('CoordAxis','int','Integer','',[pl(0)]),p70('CoordAxisSign','int','Integer','',[pl(1)]),
  p70('UnitScaleFactor','double','Number','',[pd(1)]),p70('OriginalUnitScaleFactor','double','Number','',[pd(1)])])])
 objs=[];cons=[]; nextid=840000
 def uid():
  nonlocal nextid;nextid+=1;return nextid
 rootids={}
 for rt in roots:
  rootids[rt]=uid();objs.append(fbx_model(rootids[rt],rt,'Null'));cons.append(FN('C',[ps('OO'),pl(rootids[rt]),pl(0)]))
 node_ids={}
 for group,rt,xyz in pivot_homes:
  id=uid();node_ids[(rt,group)]=id;objs.append(fbx_model(id,group,'Null',xyz));cons.append(FN('C',[ps('OO'),pl(id),pl(rootids[rt])]))
 for root,part in chunks:
  pind=part['pivot'];par=node_ids[(root,pind)] if pind else rootids[root]
  px=uid();gx=uid();mx=uid();pn='L92_'+root.split('_',2)[-1][:36]+'_Part_%04d'%(part['part_index'])
  objs.append(fbx_model(px,pn,'Mesh'))
  objs.append(fbx_geometry(gx,pn+'_Geometry',part['vertices'],part['faces']))
  objs.append(fbx_material(mx,part['material'],COLORS[part['material']]))
  cons.extend([FN('C',[ps('OO'),pl(px),pl(par)]),FN('C',[ps('OO'),pl(gx),pl(px)]),FN('C',[ps('OO'),pl(mx),pl(px)])])
 defs=FN('Definitions',children=[FN('Version',[pl(100)]),FN('Count',[pl(len(objs))])])
 with open(path,'wb') as f:
  f.write(b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400))
  for node in [header,settings,defs,FN('Objects',children=objs),FN('Connections',children=cons)]:write_node(f,node)
  f.write(b'\0'*13);f.write(b'\0'*160)
 return {'objects':len(objs),'connections':len(cons),'file_size':os.stat(path).st_size}

def pbrmat(name):
 c=COLORS[name];metal=.8 if name in ('titanium','structural','gold','copper') else .15 if name in ('dark','graphite') else 0.0
 rough=.24 if name in ('titanium','gold','glass') else .4 if name in ('structural','copper','glass') else .68
 alpha='BLEND' if c[3]<1 else 'OPAQUE'
 em=[0,0,0]
 if name in ('cyan','green','amber','violet','blue','red','orange','stars'):
  em=[round(0.4*c[k],3) for k in range(3)]
 return trimesh.visual.material.PBRMaterial(name='PS_'+name,baseColorFactor=[int(255*v) for v in c],metallicFactor=metal,roughnessFactor=rough,emissiveFactor=em,alphaMode=alpha)

def export_glb(path,chunks,roots,pivot_homes):
 scene=trimesh.Scene(base_frame='world')
 I=np.eye(4)
 for rt in roots:scene.graph.update(frame_to=rt,frame_from='world',matrix=I)
 for p,rt,xyz in pivot_homes:
  T=np.eye(4);T[:3,3]=xyz
  scene.graph.update(frame_to=p,frame_from=rt,matrix=T)
 for rt,part in chunks:
  pn='L92_'+rt.split('_',2)[-1][:25]+'_Part_%04d'%part['part_index']
  m=trimesh.Trimesh(vertices=part['vertices'],faces=part['faces'],process=False,validate=False)
  m.visual=trimesh.visual.TextureVisuals(material=MATERIALS[part['material']])
  pr=part['pivot'] if part['pivot'] else rt
  scene.add_geometry(m,node_name=pn,geom_name=pn,parent_node_name=pr)
 data=scene.export(file_type='glb',include_normals=True)
 with open(path,'wb') as f:f.write(data)
 return {'glb_bytes':len(data),'geometry_count':len(scene.geometry),'scene_node_count':len(scene.graph.nodes)}

def write_csv(path,rows,fields=None):
 if not rows:return
 keys=fields or list(rows[0])
 with open(path,'w',newline='',encoding='utf8') as f:
  writer=csv.DictWriter(f,fieldnames=keys,extrasaction='ignore');writer.writeheader();writer.writerows(rows)
