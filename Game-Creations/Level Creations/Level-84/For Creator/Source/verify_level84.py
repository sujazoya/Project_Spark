#!/usr/bin/env python3
"""Independent structural verifications for Level84 binary FBX/glTF and art manifests.
This does not test Unity compilation, gameplay or GPU visual quality.
"""
import os,sys,struct,zlib,math,json,csv,hashlib,zipfile,time
import numpy as np,trimesh
ROOT='/mnt/data/ProjectSpark_Level84_HD_AAA'
sys.path.insert(0,os.path.join(ROOT,'Source'))
import build_level84 as src

def parse_children(data,start,end):
 off=start
 while off+13<=end:
  e,n,p,nl=struct.unpack_from('<IIIB',data,off)
  if not e:break
  if e>len(data) or e<=off:raise ValueError('invalid FBX node end '+str(off))
  nm=data[off+13:off+13+nl].decode('utf8','replace')
  propbeg=off+13+nl;propend=propbeg+p
  yield (nm,propbeg,propend,propend,e-13)
  off=e

def decode_props(raw):
 result=[];off=0
 while off<len(raw):
  t=chr(raw[off]);off+=1
  if t=='S':
   n=struct.unpack_from('<I',raw,off)[0];off+=4;v=raw[off:off+n].decode('utf8','replace');off+=n
  elif t=='L':v=struct.unpack_from('<q',raw,off)[0];off+=8
  elif t=='D':v=struct.unpack_from('<d',raw,off)[0];off+=8
  elif t=='C':v=bool(raw[off]);off+=1
  elif t in ('d','i','f'):
   count,enc,blen=struct.unpack_from('<III',raw,off);off+=12
   dat=raw[off:off+blen];off+=blen
   if enc:dat=zlib.decompress(dat)
   dtype={'d':'<f8','i':'<i4','f':'<f4'}[t]
   v=np.frombuffer(dat,dtype=dtype)
   if len(v)!=count:raise ValueError('array element count invalid')
  else:raise ValueError('Unknown FBX property type: '+t)
  result.append(v)
 return result

def check_fbx(path,expected_tri,expected_pivots=None,expected_sockets=None):
 dat=open(path,'rb').read()
 if dat[:23]!=b'Kaydara FBX Binary  \x00\x1a\x00':raise ValueError('not binary FBX 7400')
 if struct.unpack_from('<I',dat,23)[0]!=7400:raise ValueError('wrong FBX version')
 root=list(parse_children(dat,27,len(dat)))
 objnode=next(q for q in root if q[0]=='Objects')
 names=[];gcount=0;tri=0;vertex_count=0;invalid=0;mat=0
 for nm,pb,pe,cb,ce in parse_children(dat,objnode[3],objnode[4]):
  if nm=='Model':
   props=decode_props(dat[pb:pe]);names.append(props[1].split('\x00')[0])
  if nm=='Material':mat+=1
  if nm!='Geometry':continue
  gcount+=1
  subs={}
  for kn,p1,p2,_,_ in parse_children(dat,cb,ce):
   if kn in ('Vertices','PolygonVertexIndex'):subs[kn]=decode_props(dat[p1:p2])[0]
  if not {'Vertices','PolygonVertexIndex'}<=set(subs):raise ValueError('missing mesh arrays')
  v=subs['Vertices'];f=subs['PolygonVertexIndex'];vertex_count+=len(v)//3
  if len(v)%3 or len(f)%3 or not np.isfinite(v).all():invalid+=1
  tri+=len(f)//3
  if not np.all(f.reshape(-1,3)[:,2] < 0):invalid+=1
  if np.any(f.reshape(-1,3)[:,:2]<0):invalid+=1
 if tri!=expected_tri or invalid:raise ValueError('FBX invalid geometry: %s tri %d vs %d, invalid %d'%(path,tri,expected_tri,invalid))
 piv={n for n in names if n.startswith('ANIM_')};socks={n for n in names if n.startswith('SOCKET_P')}
 if expected_pivots is not None and len(piv)!=expected_pivots:raise ValueError('wrong FBX animation pivots '+str(len(piv)))
 if expected_sockets is not None and len(socks)!=expected_sockets:raise ValueError('wrong FBX socket pivots '+str(len(socks)))
 return dict(triangles=tri,geometries=gcount,model_nodes=len(names),material_nodes=mat,vertex_count=vertex_count,pivots=len(piv),sockets=len(socks),bytes=len(dat))

def check_glb(path,expected_tri,expected_pivots=None,expected_sockets=None):
 s=trimesh.load(path,force='scene',process=False)
 tri=sum(len(g.faces) for g in s.geometry.values() if hasattr(g,'faces'))
 if tri!=expected_tri:raise ValueError('GLB triangles mismatch %d vs %d'%(tri,expected_tri))
 nodes=set(s.graph.nodes)
 pv={n for n in nodes if n.startswith('ANIM_')};so={n for n in nodes if n.startswith('SOCKET_P')}
 if expected_pivots is not None and len(pv)!=expected_pivots:raise ValueError('GLB pivot mismatch '+str(len(pv)))
 if expected_sockets is not None and len(so)!=expected_sockets:raise ValueError('GLB socket mismatch '+str(len(so)))
 return dict(triangles=tri,geometries=len(s.geometry),nodes=len(nodes),pivots=len(pv),sockets=len(so),bytes=os.stat(path).st_size)

def verify_all():
 manifest=json.load(open(os.path.join(ROOT,'Model_Manifest.json')))
 results=[]
 for item in manifest['models']:
  a=check_fbx(os.path.join(ROOT,'Models',item['file']),item['triangle_count'],len(item['animation_pivots']),item['visual_terminal_sockets'])
  b=check_glb(os.path.join(ROOT,'GLB',item['glb']),item['triangle_count'],len(item['animation_pivots']),item['visual_terminal_sockets'])
  if a['geometries']!=item['mesh_parts'] or b['geometries']!=item['mesh_parts']:raise ValueError('mesh parts mismatch: '+item['name'])
  results.append(dict(model=item['name'],fbx=a,glb=b))
 print('VALIDATED 76/76 FBX + 76/76 GLB',flush=True)
 sa=check_fbx(os.path.join(ROOT,'Models','Level84_Complete_ArtKit_Showcase.fbx'),manifest['individual_triangle_count'],58,218)
 sb=check_glb(os.path.join(ROOT,'GLB','Level84_Complete_ArtKit_Showcase.glb'),manifest['individual_triangle_count'],58,218)
 if sa['geometries']!=manifest['individual_mesh_parts'] or sb['geometries']!=manifest['individual_mesh_parts']:raise ValueError('assembled part count mismatch')
 print('VALIDATED ASSEMBLED FBX/GLB 58 PIVOTS 218 SOCKETS',flush=True)
 with open(os.path.join(ROOT,'Gameplay_Wiring_Concept_W001_W142.csv'),newline='') as f:wires=list(csv.DictReader(f))
 with open(os.path.join(ROOT,'Terminal_Anchor_Plan_218.csv'),newline='') as f:sockets=list(csv.DictReader(f))
 with open(os.path.join(ROOT,'Animation_Rigs.csv'),newline='') as f:pivots=list(csv.DictReader(f))
 if len(wires)!=142 or len(sockets)!=218 or len(pivots)!=58:raise ValueError('planning CSV totals mismatch')
 if [w['WireID'] for w in wires]!=['W%03d'%i for i in range(1,143)]:raise ValueError('wiring IDs not unique/sequential')
 aids={q['AnchorID']:q['Model'] for q in sockets};pnames={v['Pivot'] for v in pivots}
 if set(pnames)!={p for p,g in src.PIVOTS}:raise ValueError('pivot catalog mismatch')
 for w in wires:
  if aids.get(w['SourceVisualAnchor'])!=w['SourceModel'] or aids.get(w['TargetVisualAnchor'])!=w['TargetModel']:
   raise ValueError('wire anchors do not match model owner '+w['WireID'])
 crit=next(w for w in wires if w['WireID']=='W097')
 if crit['SourceModel']!='L84_PrimaryStarTracker' or crit['TargetModel']!='L84_AttitudeStateEstimator':raise ValueError('critical wire incorrect')
 for q in sockets:
  if not all(math.isfinite(float(q[k])) for k in ('WorldX_m','WorldY_m','WorldZ_m')):raise ValueError('nonfinite socket')
 textures=[f for f in os.listdir(os.path.join(ROOT,'Textures')) if f.endswith('.png')]
 if len(textures)!=36:raise ValueError('incorrect number textures')
 for item in textures:
  from PIL import Image
  with Image.open(os.path.join(ROOT,'Textures',item)) as im:
   if im.size!=(1024,1024):raise ValueError('invalid texture size')
 report=dict(level_id='level_84',status='GEOMETRY_AND_ART_PLANNING_STRUCTURALLY_VERIFIED',
  individual_fbx_files=76,individual_glb_files=76,assembled_fbx_files=1,assembled_glb_files=1,
  individual_triangles=manifest['individual_triangle_count'],individual_mesh_parts=manifest['individual_mesh_parts'],
  assembled_fbx=sa,assembled_glb=sb,animation_parent_pivots=58,visual_terminal_sockets=218,
  proposed_player_wires=142,critical_wire='W097',critical_source=crit['SourceModel'],critical_destination=crit['TargetModel'],
  critical_source_socket=crit['SourceVisualAnchor'],critical_target_socket=crit['TargetVisualAnchor'],
  procedural_pbr_material_sets=12,procedural_pbr_maps=36,
  modeled_bounds_metres=manifest['workstation_world_bounds_m'],
  gameplay_actual_player_wires=False,gameplay_actual_functional_spark_terminals=False,
  gameplay_attitude_physics_implemented=False,gameplay_six_objectives_implemented=False,
  unity_compilation_tested=False,unity_play_mode_tested=False,
  validation_note='Checks binary FBX node/array integrity, GLB loading, mesh counts, pivot/visual socket nodes, wiring references, and 36 PNG maps; does not validate artist finish, production UV maps, compiled Unity scripts or working gameplay.',
  models=results)
 with open(os.path.join(ROOT,'Verification_Report.json'),'w') as f:json.dump(report,f,indent=2)
 return report
if __name__=='__main__':
 st=time.time();report=verify_all();print('VERIFICATION COMPLETE',round(time.time()-st,1),'seconds',flush=True)
