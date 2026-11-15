#!/usr/bin/env python3
"""Independent structural FBX/GLB and project planning verification, not Unity play-mode tests."""
import os,sys,struct,zlib,math,json,csv,hashlib,time
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'Source'))
import build_level85 as b
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

def verify():
 manifest=json.loads((ROOT/'Model_Manifest.json').read_text())
 checks=[]
 for k,item in enumerate(manifest['models']):
  fp=ROOT/'Models'/item['file']; gp=ROOT/'GLB'/item['glb']
  a=check_fbx(fp,item['triangle_count'],len(item['animation_pivots']),item['visual_terminal_sockets'])
  gl=check_glb(gp,item['triangle_count'],len(item['animation_pivots']),item['visual_terminal_sockets'])
  if a['geometries']!=item['mesh_parts'] or gl['geometries']!=item['mesh_parts']:
   raise ValueError('mesh geometry/manifest mismatch '+item['name'])
  checks.append(dict(model=item['name'],fbx=a,glb=gl))
  if k%15==0 or k==77:print('VERIFIED FBX+GLB',k+1,'/78',flush=True)
 fb=check_fbx(ROOT/'Models'/'Level85_Complete_ArtKit_Showcase.fbx',manifest['individual_triangles'],60,224)
 gl=check_glb(ROOT/'GLB'/'Level85_Complete_ArtKit_Showcase.glb',manifest['individual_triangles'],60,224)
 if fb['geometries']!=manifest['individual_mesh_parts'] or gl['geometries']!=manifest['individual_mesh_parts']:
  raise ValueError('assembled mesh count mismatch')
 assert len(b.NAMES)==78 and len(b.PIVOTS)==60 and len(b.ANCHORS)==224 and len(b.WIRES)==146 and len(b.logical_map)==292
 assert [q['WireID'] for q in b.WIRES]==['W%03d'%i for i in range(1,147)]
 assert len(set(x['Pivot'] for x in b.PIVOTS))==60
 socket_owner={s['AnchorID']:s['Model'] for s in b.ANCHORS}
 for wire in b.WIRES:
  if socket_owner[wire['SourceVisualAnchor']]!=wire['SourceModel'] or socket_owner[wire['TargetVisualAnchor']]!=wire['TargetModel']:
   raise ValueError('wire socket owner mismatch: '+wire['WireID'])
  if not wire['SourceTerminalProposed'] or not wire['TargetTerminalProposed']:raise ValueError('missing logical endpoint')
 critical=[wire for wire in b.WIRES if wire['Critical']=='YES']
 assert len(critical)==1 and critical[0]['WireID']=='W123'
 assert critical[0]['SourceModel']=='L85_RelativeNavigationFusionComputer'
 assert critical[0]['TargetModel']=='L85_DockingSafetyPLC'
 tex=list((ROOT/'Textures').glob('*.png'))
 if len(tex)!=36:raise ValueError('need 36 PNGs, found '+str(len(tex)))
 from PIL import Image
 for p in tex:
  with Image.open(p) as im:
   if im.size!=(1024,1024):raise ValueError('wrong texture size: '+p.name)
 for filename,exp in [('Animation_Rigs.csv',60),('Gameplay_Wiring_Concept_W001_W146.csv',146),('Terminal_Anchor_Plan_224.csv',224),('Terminal_Logical_Port_Reconciliation_292.csv',292),('Objective_Certification_Plan_6.csv',6)]:
  with (ROOT/filename).open(newline='') as f:
   rows=list(csv.DictReader(f))
  if len(rows)!=exp:raise ValueError(filename+' expected '+str(exp)+' found '+str(len(rows)))
 report=dict(level_id='level_85',verification_status='GEOMETRY_AND_SOURCE_PLANNING_STRUCTURALLY_VERIFIED',
  models_fbx=78,models_glb=78,showcase_fbx=1,showcase_glb=1,
  triangle_count=manifest['individual_triangles'],grouped_mesh_parts=manifest['individual_mesh_parts'],
  assembled_fbx=fb,assembled_glb=gl,
  named_mesh_owning_animation_pivots=60,visual_socket_nodes=224,planned_player_wires=146,
  conceptual_logical_terminal_endpoints=292,
  art_anchor_endpoints_reused=sum(len(v)>1 for v in b.reuse.values()),
  critical_wire='W123',critical_source=critical[0]['SourceModel'],critical_destination=critical[0]['TargetModel'],
  critical_source_visual_socket=critical[0]['SourceVisualAnchor'],critical_target_visual_socket=critical[0]['TargetVisualAnchor'],
  pbr_reference_material_sets=12,pbr_reference_texture_maps=36,
  gameplay_actual_player_wires=False,gameplay_actual_sparkterminals=False,
  spacecraft_relative_dynamics_implemented=False,real_docking_mechanism_simulation=False,
  six_objectives_implemented=False,unity_import_tested=False,unity_compilation_tested=False,unity_play_mode_tested=False,
  texture_uv_production_complete=False,artist_authored_final_aaa_finish=False,
  note='Geometry and CSVs parsed and inspected. This does not validate Unity FBX behavior, physically simulated docking, circuit logic, authored UVs, or runtime gameplay.',
  model_validation=checks)
 (ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))
 print('VERIFICATION COMPLETE 78 FBX + 78 GLB, 60 pivots, 224 sockets, 146 planned wires',flush=True)
 return report

if __name__=='__main__':verify()
