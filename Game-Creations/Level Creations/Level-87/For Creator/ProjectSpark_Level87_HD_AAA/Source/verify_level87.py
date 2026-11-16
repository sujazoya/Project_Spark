#!/usr/bin/env python3
"""Package and exported-geometry checks. Not Unity compilation or Play Mode."""
from pathlib import Path
import csv,json,struct,time,hashlib
from collections import Counter
import trimesh
ROOT=Path(__file__).resolve().parent.parent
m=json.loads((ROOT/'Model_Manifest.json').read_text())
checks=[]
def check(name,ok,details=None):
 checks.append(dict(check=name,passed=bool(ok),details=details));print(('PASS' if ok else 'FAIL'),name,str(details)[:110],flush=True)
files=sorted((ROOT/'Models').glob('??_L87_*.fbx'))
glbs=sorted((ROOT/'GLB').glob('??_L87_*.glb'))
check('86 individual binary FBX model files',len(files)==86,len(files))
check('86 matching GLB geometry model files',len(glbs)==86,len(glbs))
check('Assembled showcase FBX and GLB', (ROOT/'Models/Level87_Complete_ArtKit_Showcase.fbx').exists() and (ROOT/'GLB/Level87_Complete_ArtKit_Showcase.glb').exists())
check('Unique and sequential filenames',all(p.name==f"{i:02d}_{r['name']}.fbx" for i,(p,r) in enumerate(zip(files,m['models']))))
check('FBX binary 7400 headers', all(p.read_bytes()[:27]==b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400) for p in files+[ROOT/'Models/Level87_Complete_ArtKit_Showcase.fbx']))
check('FBX basic objects, geometry, connections structural text',all(all(s in p.read_bytes() for s in [b'Geometry',b'Model',b'Connections',b'Vertices',b'PolygonVertexIndex']) for p in files))
check('GLB binary header', all(p.read_bytes()[:4]==b'glTF' for p in glbs+[ROOT/'GLB/Level87_Complete_ArtKit_Showcase.glb']))
tri=0;parts=0;nodes=set();per=[]
for i,p in enumerate(glbs):
 try:
  scene=trimesh.load(str(p),file_type='glb',force='scene',process=False)
  count=sum(len(g.faces) for g in scene.geometry.values())
  if not scene.geometry or count==0:raise RuntimeError('no geom')
  per.append((i,True,count));tri+=count;parts+=len(scene.geometry)
  nodes.update(scene.graph.nodes)
 except Exception as e:per.append((i,False,str(e)))
check('All 86 GLB exports independently loadable',len(per)==86 and all(r[1] for r in per),str([p for p in per if not p[1]])[:200])
check('Exported triangle sum agrees with manifest',tri==m['individual_triangles'],f'{tri} vs {m["individual_triangles"]}')
check('Exported mesh part sum agrees with manifest',parts==m['individual_mesh_parts'],f'{parts} vs {m["individual_mesh_parts"]}')
pivot=list(csv.DictReader(open(ROOT/'Animation_Rigs.csv')))
check('68 unique planned animation pivot IDs',len(pivot)==68 and len({p['Pivot'] for p in pivot})==68,len(pivot))
check('All 68 pivot transforms present as actual GLB scene nodes',all(p['Pivot'] in nodes for p in pivot),[p['Pivot'] for p in pivot if p['Pivot'] not in nodes][:8])
a=list(csv.DictReader(open(ROOT/'Terminal_Anchor_Plan_236.csv')))
check('236 unique gold socket anchor records',len(a)==236 and len({r['AnchorID'] for r in a})==236,len(a))
check('All 236 SOCKET_P nodes present as GLB scene nodes',all('SOCKET_'+r['AnchorID'] in nodes for r in a),len([r for r in a if 'SOCKET_'+r['AnchorID'] not in nodes]))
wires=list(csv.DictReader(open(ROOT/'Gameplay_Wiring_Concept_W001_W158.csv')))
check('158 sequential unique proposed WIDs',len(wires)==158 and all(x['WireID']==f'W{i+1:03d}' for i,x in enumerate(wires)),len(wires))
check('All wire endpoints map to art sockets on correct owning models',all(any(r['AnchorID']==w[side+'VisualAnchor'] and r['Model']==w[side+'Model'] for r in a) for w in wires for side in ('Source','Target')))
logical=list(csv.DictReader(open(ROOT/'Terminal_Logical_Port_Reconciliation_316.csv')))
check('316 separately identified logical wire ends',len(logical)==316 and len({r['FunctionalPortID'] for r in logical})==316,len(logical))
conflicts=list(csv.DictReader(open(ROOT/'Terminal_Anchor_Signal_Conflict_Report.csv')))
check('All reused visual anchors produce a reconciliation report',len(conflicts)==m['anchor_reconciliation_conflicts'],len(conflicts))
critical=[x for x in wires if x['Critical']=='YES']
check('W137 uniquely marked critical and correct models',len(critical)==1 and critical[0]['WireID']=='W137' and critical[0]['SourceModel']=='L87_PrimaryRadialGapSensor' and critical[0]['TargetModel']=='L87_MagneticBearingSafetyPLC',critical[0] if critical else '')
check('W137 correct planned P019-to-P135 socket route',critical and critical[0]['SourceVisualAnchor']=='P019' and critical[0]['TargetVisualAnchor']=='P135')
check('Six source objective design rows',len(list(csv.DictReader(open(ROOT/'Objective_Certification_Plan_6.csv'))))==6)
check('Exactly 12 original procedural material sets / 36 maps',len(list((ROOT/'Textures').glob('*.png')))==36 and len(json.load(open(ROOT/'Texture_Manifest.json'))['materials'])==12)
check('Four optional Unity art helper scripts',len(list((ROOT/'Unity_Optional').glob('*.cs')))==4)
check('No gameplay falsely represented as implemented',not m['actual_player_wires_included'] and not m['unity_playmode_tested'] and not m['six_runtime_objectives_implemented'])
report=dict(source='LOCAL_GENERATED_LEVEL87_ART',structural_checks_passed=all(c['passed'] for c in checks),total_checks=len(checks),passes=sum(c['passed'] for c in checks),fails=sum(not c['passed'] for c in checks),
 checks=checks,triangles=tri,mesh_parts=parts,unity_compilation_tested=False,unity_play_mode_tested=False,actual_player_wires_included=False,actual_physics_and_objectives_implemented=False,
 independently_verified_binary_fbx_import_in_dcc=False)
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
# checksums over package files excluding checksums and verification itself
paths=sorted(p for p in ROOT.rglob('*') if p.is_file() and p.name not in ('Checksums_SHA256.txt',))
with (ROOT/'Checksums_SHA256.txt').open('w') as f:
 for p in paths:f.write(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p.relative_to(ROOT))+'\n')
print('SUMMARY:',report['passes'],'/',report['total_checks'],'triangles',tri,'parts',parts,flush=True)
if not report['structural_checks_passed']:raise SystemExit(2)
