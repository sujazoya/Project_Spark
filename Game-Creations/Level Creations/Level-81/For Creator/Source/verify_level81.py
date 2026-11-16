#!/usr/bin/env python3
"""Independent Level 81 FBX/GLB structural geometry, pivot and gameplay-caveat checks."""
import json,zlib,struct,csv,hashlib,zipfile
from pathlib import Path
import numpy as np,trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level81_HD_AAA')
MAN=json.loads((ROOT/'Model_Manifest.json').read_text())
MAGIC=b'Kaydara FBX Binary  \x00\x1a\x00'

def decode_array(data,pos):
    tp=chr(data[pos]);n,mode,nbytes=struct.unpack_from('<III',data,pos+1)
    b=data[pos+13:pos+13+nbytes]
    if mode:b=zlib.decompress(b)
    dt={'d':'<f8','i':'<i4','f':'<f4','l':'<i8'}[tp]
    arr=np.frombuffer(b,dtype=dt,count=n)
    if len(arr)!=n:raise ValueError('Array size mismatch')
    return arr

def scan_fbx(path):
    d=path.read_bytes(); assert d[:23]==MAGIC and struct.unpack_from('<I',d,23)[0]==7400,path
    found={'Geometry':0,'Vertices':0,'PolygonVertexIndex':0,'Model':0};names=[];top=[]
    def parse(start,limit,level=0):
        while start+13<=limit:
            end,nprops,plen,nlen=struct.unpack_from('<IIIB',d,start)
            if end==0:return
            assert start<end<=limit and end<=len(d),(path.name,start,end,limit)
            name=d[start+13:start+13+nlen].decode('ascii')
            if level==0:top.append(name)
            pos=start+13+nlen
            if name in ('Vertices','PolygonVertexIndex'):
                a=decode_array(d,pos)
                if name=='Vertices':
                    assert a.size%3==0 and np.isfinite(a).all()
                    found[name]+=a.size//3
                else:
                    assert a.size%3==0
                    assert int(np.count_nonzero(a<0))==a.size//3
                    found[name]+=int(np.count_nonzero(a<0))
            elif name in ('Geometry','Model'):
                found[name]+=1
                if name=='Model':
                    if d[pos:pos+1]==b'L':uid=struct.unpack_from('<q',d,pos+1)[0];p=pos+9
                    else:uid=None;p=pos
                    if p+5<=len(d) and d[p]==ord('S'):
                        n=struct.unpack_from('<I',d,p+1)[0]
                        raw=d[p+5:p+5+n]
                        names.append(raw.split(b'\x00')[0].decode('utf-8','ignore'))
            child=pos+plen
            if child+13<end:parse(child,end,level+1)
            start=end
    parse(27,len(d))
    assert all(k in top for k in ['FBXHeaderExtension','GlobalSettings','Objects','Connections']),path
    assert found['Geometry']>0 and found['PolygonVertexIndex']>0,path
    return {'triangles':found['PolygonVertexIndex'],'vertices':found['Vertices'],'geometries':found['Geometry'],'model_nodes':found['Model'],'model_node_names':names,'bytes':len(d)}

assert MAN['individual_models']==70 and MAN['individual_glbs']==70
assert MAN['planned_player_wires']==130 and MAN['planned_unique_functional_terminals']==200
assert MAN['actual_player_wires_included'] is False
assert MAN['functional_spark_terminals_included'] is False
assert MAN['virtual_fault_simulator_included'] is False
assert MAN['six_authoritative_objectives_implemented'] is False
assert len(MAN['models'])==70
pivots=list(csv.DictReader((ROOT/'Animation_Rigs.csv').open(newline='',encoding='utf8')))
wires=list(csv.DictReader((ROOT/'Gameplay_Wiring_Concept_W001_W130.csv').open(newline='',encoding='utf8')))
sockets=list(csv.DictReader((ROOT/'Terminal_Anchor_Plan_200.csv').open(newline='',encoding='utf8')))
objectives=list(csv.DictReader((ROOT/'Objective_Certification_Plan_6.csv').open(newline='',encoding='utf8')))
assert len(pivots)==52 and len(set(p['Pivot'] for p in pivots))==52
assert len(wires)==130 and [w['WireID'] for w in wires]==[f'W{x:03d}' for x in range(1,131)]
assert len(sockets)==200 and len(set(x['AnchorID'] for x in sockets))==200
assert len(objectives)==6
w=wires[93]
assert (w['WireID'],w['SourceModel'],w['TargetModel'],w['Critical'])==('W094','L81_ECCSyndromeDetectionEngine','L81_CriticalECCFeedbackSupervisor','YES')
assert sum(int(r['PlannedWires']) for r in csv.DictReader((ROOT/'Gameplay_Wiring_Budget.csv').open()))==130
socket_by_id={x['AnchorID']:x for x in sockets}
for wire in wires:
 assert wire['SourceVisualAnchor'] in socket_by_id and wire['TargetVisualAnchor'] in socket_by_id
 assert socket_by_id[wire['SourceVisualAnchor']]['Model']==wire['SourceModel']
 assert socket_by_id[wire['TargetVisualAnchor']]['Model']==wire['TargetModel']
report={'level_id':'level_81','expected_individual_models':70,'expected_matching_glbs':70,
        'individual_fbx_verified':[],'individual_glb_verified':[],
        'pivot_count':52,'planned_wires':130,'visual_terminal_socket_count':200,
        'six_objective_definitions':6,'pbr_maps':36,'unity_compilation_tested':False,
        'unity_play_mode_tested':False,'actual_player_wires_included':False,
        'functional_sparkterminals_included':False,'gameplay_functionality_included':False,
        'errors':[]}
for idx,m in enumerate(MAN['models']):
 assert m['index']==idx
 assert m['file'].startswith(f'{idx:02d}_L81_')
 assert m['glb'].startswith(f'{idx:02d}_L81_')
 f=ROOT/'Models'/m['file'];g=ROOT/'GLB'/m['glb']
 assert f.is_file() and g.is_file()
 a=scan_fbx(f)
 assert a['triangles']==m['triangle_count'],(f.name,a['triangles'],m['triangle_count'])
 assert a['geometries']==m['mesh_parts']
 for pivot in m['rig_groups']:
  assert pivot in a['model_node_names'],(f.name,'missing FBX named pivot',pivot)
 report['individual_fbx_verified'].append({'file':f.name,'triangles':a['triangles'],'mesh_parts':a['geometries'],'bytes':a['bytes'],'named_parent_pivots':len(m['rig_groups'])})
 sc=trimesh.load(g,force='scene')
 tris=sum(len(mesh.faces) for mesh in sc.geometry.values())
 assert tris==m['triangle_count'],(g.name,tris,m['triangle_count'])
 assert len(sc.geometry)==m['mesh_parts']
 b=np.asarray(sc.bounds)
 assert b.shape==(2,3) and np.isfinite(b).all()
 assert np.allclose(b[0],m['bbox_min'],atol=.01) and np.allclose(b[1],m['bbox_max'],atol=.01)
 for pivot in m['rig_groups']:
  assert pivot in sc.graph.nodes,(g.name,'missing GLB named pivot',pivot)
 report['individual_glb_verified'].append({'file':g.name,'triangles':tris,'mesh_parts':len(sc.geometry),'bytes':g.stat().st_size,'named_parent_pivots':len(m['rig_groups'])})
 print(f'VERIFIED {idx+1:02d}/70 {f.name}',flush=True)

for p in pivots:
 m=next(x for x in MAN['models'] if x['name']==p['Model'])
 xyz=np.array([float(p[a]) for a in ('X','Y','Z')])
 lo=np.array(m['bbox_min']);hi=np.array(m['bbox_max'])
 assert np.all(xyz>=lo-1.75) and np.all(xyz<=hi+1.75),(p['Pivot'],xyz,lo,hi)

for ext in ('fbx','glb'):
 p=ROOT/('Models' if ext=='fbx' else 'GLB')/f'Level81_Complete_ArtKit_Showcase.{ext}'
 if ext=='fbx':
  a=scan_fbx(p);tris=a['triangles'];nodes=a['geometries']
  assert all(r['Pivot'] in a['model_node_names'] for r in pivots)
 else:
  sc=trimesh.load(p,force='scene');tris=sum(len(mesh.faces) for mesh in sc.geometry.values());nodes=len(sc.geometry)
  assert all(r['Pivot'] in sc.graph.nodes for r in pivots)
 assert tris==MAN['individual_triangle_count']
 assert nodes==MAN['individual_mesh_parts']
 report['assembled_'+ext+'_verified']=True
 report['assembled_'+ext+'_mesh_parts']=nodes

assert len(list((ROOT/'Models').glob('*.fbx')))==71
assert len(list((ROOT/'GLB').glob('*.glb')))==71
assert len(list((ROOT/'Unity_Optional').glob('*.cs')))==4
assert len(list((ROOT/'Textures').glob('*.png')))==36
assert sum(x['triangle_count'] for x in MAN['models'])==MAN['individual_triangle_count']
assert sum(x['mesh_parts'] for x in MAN['models'])==MAN['individual_mesh_parts']
assert sum(len(x['rig_groups']) for x in MAN['models'])==52
assert sum(x['visual_terminal_sockets'] for x in MAN['models'])==200
bb=np.array(MAN['workstation_world_bounds_m']);assert np.isclose(bb[1,0]-bb[0,0],24.0,atol=.001)
for cs in (ROOT/'Unity_Optional').glob('*.cs'):
 text=cs.read_text()
 assert 'Level80' not in text and '_L80_' not in text and 'Levels/80/' not in text
 assert text.count('{')==text.count('}')
report['all_geometry_exports_verified']=True
report['total_equipment_triangles']=MAN['individual_triangle_count']
report['total_equipment_mesh_parts']=MAN['individual_mesh_parts']
report['assembled_workstation_width_m']=float(bb[1,0]-bb[0,0])
report['independent_structural_checks_passed']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('STRUCTURAL VERIFY PASSED: 70 FBXs + 70 GLBs + two assembled, 52 real pivots, 200 socket positions, 130 wire concepts',flush=True)
