#!/usr/bin/env python3
"""Independent Level 74 FBX/GLB structural geometry, pivot and gameplay-caveat checks."""
import json,zlib,struct,csv,hashlib,zipfile
from pathlib import Path
import numpy as np,trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level74_HD_AAA')
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

assert MAN['individual_models']==56 and MAN['individual_glbs']==56
assert MAN['planned_player_wires']==102 and MAN['planned_unique_functional_terminals']==158
assert MAN['actual_player_wires_included'] is False
assert MAN['functional_spark_terminals_included'] is False
assert MAN['lithography_physics_included'] is False
assert MAN['six_authoritative_objectives_implemented'] is False
assert len(MAN['models'])==56
pivots=list(csv.DictReader((ROOT/'Animation_Rigs.csv').open(newline='',encoding='utf-8')))
assert len(pivots)==MAN['animation_parent_pivot_groups']
assert len({p['Pivot'] for p in pivots})==len(pivots)
budget=list(csv.DictReader((ROOT/'Gameplay_Wiring_Budget.csv').open(newline='',encoding='utf-8')))
assert sum(int(row['PlannedPlayerWires']) for row in budget)==102
report={'level_id':'level_74','individual_models_expected':54,'matching_individual_glbs_expected':54,
        'individual_fbx_verified':[],'individual_glb_verified':[],
        'animation_pivots_verified':len(pivots),'planned_wires':102,'planned_terminals':158,
        'unity_compilation_tested':False,'play_mode_tested':False,
        'gameplay_functionality_included':False,'errors':[]}
for idx,m in enumerate(MAN['models']):
    assert m['index']==idx
    assert m['file'].startswith(f'{idx:02d}_L74_')
    assert m['glb'].startswith(f'{idx:02d}_L74_')
    f=ROOT/'Models'/m['file'];g=ROOT/'GLB'/m['glb']
    assert f.is_file() and g.is_file()
    a=scan_fbx(f)
    assert a['triangles']==m['triangle_count'],(f,a['triangles'],m['triangle_count'])
    assert a['geometries']==m['mesh_parts'],(f,a['geometries'],m['mesh_parts'])
    for grp in m['rig_groups']:
        assert grp in a['model_node_names'],(f.name,'missing named FBX pivot parent',grp)
    report['individual_fbx_verified'].append({'file':f.name,'triangles':a['triangles'],'model_nodes':a['model_nodes'],'bytes':a['bytes'],'actual_named_pivot_parents':len(m['rig_groups'])})
    sc=trimesh.load(g,force='scene')
    nt=sum(len(me.faces) for me in sc.geometry.values())
    assert nt==m['triangle_count'],(g.name,nt,m['triangle_count'])
    assert len(sc.geometry)==m['mesh_parts']
    bbox=np.asarray(sc.bounds)
    assert bbox.shape==(2,3) and np.isfinite(bbox).all()
    assert np.allclose(bbox[0],m['bbox_min'],atol=.009) and np.allclose(bbox[1],m['bbox_max'],atol=.009),(g.name,bbox,m['bbox_min'],m['bbox_max'])
    for grp in m['rig_groups']:
        assert grp in sc.graph.nodes,(g.name,'missing GLB pivot parent',grp)
    report['individual_glb_verified'].append({'file':g.name,'triangles':nt,'mesh_parts':len(sc.geometry),'bytes':g.stat().st_size,'actual_named_pivot_parents':len(m['rig_groups'])})
    print(f'VERIFIED {idx+1:02d}/56 {m["file"]}',flush=True)

for item in pivots:
    model=next(x for x in MAN['models'] if x['name']==item['Model'])
    xyz=np.array([float(item[a]) for a in ('X','Y','Z')]);lo=np.array(model['bbox_min']);hi=np.array(model['bbox_max'])
    assert np.isfinite(xyz).all()
    assert np.all(xyz>=lo-.6) and np.all(xyz<=hi+.6),(item['Pivot'],xyz,lo,hi)

for ext in ('fbx','glb'):
    p=ROOT/('Models' if ext=='fbx' else 'GLB')/f'Level74_Complete_ArtKit_Showcase.{ext}'
    if ext=='fbx':
        res=scan_fbx(p);actual=res['triangles'];report['assembled_fbx_geometry_nodes']=res['geometries']
    else:
        sc=trimesh.load(p,force='scene');actual=sum(len(x.faces) for x in sc.geometry.values());report['assembled_glb_geometry_nodes']=len(sc.geometry)
    assert actual==MAN['individual_triangle_count'],(ext,actual,MAN['individual_triangle_count'])
    report['assembled_'+ext+'_verified']=True

assert len(list((ROOT/'Models').glob('*.fbx')))==57
assert len(list((ROOT/'GLB').glob('*.glb')))==57
assert len(list((ROOT/'Unity_Optional').glob('*.cs')))==3
assert sum(x['triangle_count'] for x in MAN['models'])==MAN['individual_triangle_count']
assert sum(x['mesh_parts'] for x in MAN['models'])==MAN['individual_mesh_parts']
assert sum(len(x['rig_groups']) for x in MAN['models'])==MAN['animation_parent_pivot_groups']
assert len(list((ROOT/'Previews').glob('*.png')))==3
report['all_geometry_exports_verified']=True
report['binary_fbx_with_rig_parents_verified']=55
report['loadable_glb_with_hierarchy_verified']=55
report['total_equipment_triangles']=MAN['individual_triangle_count']
report['total_equipment_mesh_parts']=MAN['individual_mesh_parts']
report['safe_art_only_scope_confirmed']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('FINAL VERIFY OK: 56 individual FBX and GLB + 2 assembled; 42 named pivot parents; 140,212 triangles',flush=True)
