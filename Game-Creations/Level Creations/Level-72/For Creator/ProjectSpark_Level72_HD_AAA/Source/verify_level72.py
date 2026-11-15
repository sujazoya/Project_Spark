import json,zlib,struct,zipfile
from pathlib import Path
import numpy as np
import trimesh
ROOT=Path('/mnt/data/ProjectSpark_Level72_HD_AAA')
MAN=json.loads((ROOT/'Model_Manifest.json').read_text())
MAGIC=b'Kaydara FBX Binary  \x00\x1a\x00'

def decode_array(data,pos):
    typ=chr(data[pos]); n,encoding,nbytes=struct.unpack_from('<III',data,pos+1)
    data=data[pos+13:pos+13+nbytes]
    if encoding: data=zlib.decompress(data)
    dt={'d':'<f8','i':'<i4','f':'<f4','l':'<i8'}[typ]
    return np.frombuffer(data,dtype=dt,count=n)

def scan_fbx(path):
    data=path.read_bytes()
    assert data[:23]==MAGIC and struct.unpack_from('<I',data,23)[0]==7400,('Not binary FBX 7400',path)
    counter={'Vertices':0,'PolygonVertexIndex':0,'Geometry':0}
    top=[]
    def parse(start,limit,level=0):
        while start+13<=limit:
            end,nprops,proplen,namelen=struct.unpack_from('<IIIB',data,start)
            if end==0:return
            assert end<=len(data) and end<=limit and end>start,(start,end,limit)
            name=data[start+13:start+13+namelen].decode('ascii')
            if level==0:top.append(name)
            p=start+13+namelen
            if name=='Vertices':
                arr=decode_array(data,p);assert len(arr)%3==0 and np.isfinite(arr).all()
                counter[name]+=len(arr)//3
            elif name=='PolygonVertexIndex':
                arr=decode_array(data,p);assert len(arr)%3==0
                assert int(np.count_nonzero(arr<0))==len(arr)//3
                counter[name]+=int(np.count_nonzero(arr<0))
            elif name=='Geometry':counter[name]+=1
            child=p+proplen
            if child+13<end:parse(child,end,level+1)
            start=end
    parse(27,len(data))
    assert all(x in top for x in ('FBXHeaderExtension','Objects','Connections')),('Missing top nodes',path)
    assert counter['Geometry']>0 and counter['PolygonVertexIndex']>0
    return dict(vertices=counter['Vertices'],triangles=counter['PolygonVertexIndex'],geometries=counter['Geometry'],bytes=len(data))

report={'level_id':'level_72','individual_models_expected':52,'animation_pivot_mappings':MAN['animation_pivot_reference_count'],'individual_triangles':MAN['individual_triangle_count'],'binary_fbx_verified':[],'glb_verified':[],'errors':[]}
assert MAN['individual_models']==52
for m in MAN['models']:
    f=ROOT/'Models'/m['file'];g=ROOT/'GLB'/m['glb']
    assert f.is_file() and g.is_file()
    a=scan_fbx(f)
    assert a['triangles']==m['triangle_count'],(f.name,a['triangles'],m['triangle_count'])
    assert a['geometries']==m['mesh_parts']
    report['binary_fbx_verified'].append({'file':f.name,**a})
    scene=trimesh.load(g,force='scene');tris=sum(len(mesh.faces) for mesh in scene.geometry.values())
    assert tris==m['triangle_count'],(g,tris,m['triangle_count'])
    bounds=np.asarray(scene.bounds)
    assert np.all(np.isfinite(bounds))
    report['glb_verified'].append({'file':g.name,'triangles':tris,'geometries':len(scene.geometry),'bytes':g.stat().st_size})
for ext,d in (('fbx','Models'),('glb','GLB')):
    p=ROOT/d/f'Level72_Complete_ArtKit_Showcase.{ext}'
    if ext=='fbx':r=scan_fbx(p);assert r['triangles']==MAN['individual_triangle_count']
    else:
        sc=trimesh.load(p,force='scene');assert sum(len(m.faces) for m in sc.geometry.values())==MAN['individual_triangle_count']
    report[f'assembled_{ext}_verified']=True
assert len(report['binary_fbx_verified'])==52 and len(report['glb_verified'])==52
assert len(list((ROOT/'Models').glob('*.fbx')))==53
assert len(list((ROOT/'GLB').glob('*.glb')))==53
assert len(list((ROOT/'Unity_Optional').glob('*.cs')))==3
report['all_exports_verified']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))
print('VERIFIED',len(report['binary_fbx_verified']),'FBX +',len(report['glb_verified']),'GLB, assembled models; total',report['individual_triangles'],'triangles')

# Verify pivot count and planning-only data are accurate.
import csv
with (ROOT/'Animation_Rigs.csv').open(newline='',encoding='utf-8') as f: pivot_rows=list(csv.DictReader(f))
assert len(pivot_rows)==MAN['animation_pivot_reference_count']==24
with (ROOT/'Gameplay_Wiring_Budget.csv').open(newline='',encoding='utf-8') as f: budget_rows=list(csv.DictReader(f))
assert sum(int(r['ProposedPlayerWires']) for r in budget_rows)==94
assert MAN['actual_player_wires_included'] is False and MAN['functional_spark_terminals_included'] is False
report['pivot_references_verified']=len(pivot_rows)
report['planned_wiring_budget_verified']=94
report['no_playable_simulation_included']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))
print('FINAL VERIFIED: 52 individual FBX / 52 individual GLB / 2 assembled,',MAN['individual_triangle_count'],'triangles;',len(pivot_rows),'pivots')

# Validate all 52 model names/roles, spatial extents, and 24 pivot refs.
assert len(MAN['models']) == 52
assert MAN['planned_player_wires'] == 94 and MAN['planned_unique_functional_terminals'] == 146
assert MAN['ald_physics_included'] is False and MAN['six_gameplay_objectives_implemented'] is False
for i, item in enumerate(MAN['models']):
    assert item['index']==i
    assert item['file'].startswith(f'{i:02d}_L72_')
    assert item['glb'].startswith(f'{i:02d}_L72_')
    assert len(item['bbox_min'])==3 and len(item['bbox_max'])==3
    assert np.isfinite(item['bbox_min']).all() and np.isfinite(item['bbox_max']).all()
    assert all(x<y for x,y in zip(item['bbox_min'],item['bbox_max']))
from collections import Counter
byname=Counter(m['name'] for m in MAN['models'])
assert all(x==1 for x in byname.values())
assert sum(m['triangle_count'] for m in MAN['models'])==MAN['individual_triangle_count']
assert sum(m['mesh_parts'] for m in MAN['models'])==MAN['individual_mesh_parts']
assert all(row['Model'] in byname for row in pivot_rows)
assert len({(row['Model'],row['Pivot']) for row in pivot_rows})==24
for row in pivot_rows:
    p=np.asarray([float(row[k]) for k in ('X','Y','Z')])
    assert np.isfinite(p).all()
    model=next(m for m in MAN['models'] if m['name']==row['Model'])
    lo=np.array(model['bbox_min']);hi=np.array(model['bbox_max'])
    # Reference points can be up to 0.4 m outside modeled geometry for rig handles.
    assert np.all(p>=lo-.42) and np.all(p<=hi+.42),(row['Pivot'],p,lo,hi)
report['model_names_unique_and_ordered']=True
report['pivot_locations_spatially_plausible']=True
report['art_only_no_gameplay_claim']=True
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('EXTRA QA: model IDs, geometry extents, 24 pivot locations & art-only flags passed')
