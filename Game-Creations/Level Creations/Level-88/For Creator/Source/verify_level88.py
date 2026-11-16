#!/usr/bin/env python3
"""Structural mesh/export and planning validation; not Unity import/Play Mode."""
from pathlib import Path
import csv,json,struct,hashlib,math
from collections import Counter
import trimesh
ROOT=Path(__file__).resolve().parent.parent
m=json.loads((ROOT/'Model_Manifest.json').read_text())
checks=[]
def ck(title,ok,detail=''):
 checks.append(dict(name=title,passed=bool(ok),details=str(detail)))
 print(('PASS ' if ok else 'FAIL ')+title+' '+str(detail)[:85],flush=True)
files=sorted((ROOT/'Models').glob('??_L88_*.fbx'))
glbs=sorted((ROOT/'GLB').glob('??_L88_*.glb'))
ck('90 separately exported binary FBX models',len(files)==90,len(files))
ck('90 matching GLB models',len(glbs)==90,len(glbs))
ck('Assembled showcase in both formats',(ROOT/'Models/Level88_Complete_ArtKit_Showcase.fbx').is_file() and (ROOT/'GLB/Level88_Complete_ArtKit_Showcase.glb').is_file())
ck('Sequential model filenames',len(files)==90 and all(files[i].name==f'{i:02d}_{m["models"][i]["name"]}.fbx' for i in range(90)))
header=b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400)
ck('FBX 7400 binary headers',all(p.read_bytes()[:27]==header for p in files+[ROOT/'Models/Level88_Complete_ArtKit_Showcase.fbx']))
ck('Mesh hierarchy and geometry blocks in each FBX',all(all(t in p.read_bytes() for t in [b'Geometry',b'Model',b'Connections',b'Vertices',b'PolygonVertexIndex']) for p in files))
ck('GLB headers',all(p.open('rb').read(4)==b'glTF' for p in glbs+[ROOT/'GLB/Level88_Complete_ArtKit_Showcase.glb']))
tris=0;parts=0;nodes=set();fail=[];finite=True;nonempty=True
for i,p in enumerate(glbs):
 try:
  scene=trimesh.load(str(p),file_type='glb',force='scene',process=False)
  if len(scene.geometry)<1:nonempty=False
  for mesh in scene.geometry.values():
   tris+=len(mesh.faces)
   if not math.isfinite(float(mesh.bounds[0].sum())):finite=False
  parts+=len(scene.geometry);nodes.update(scene.graph.nodes)
 except Exception as ex:fail.append((str(p),str(ex)))
ck('All 90 GLB meshes load independently',not fail and len(glbs)==90,fail[:2])
ck('Every GLB has geometry',nonempty)
ck('All GLB vertices finite',finite)
ck('GLB triangle sum matches manifest',tris==m['individual_triangles'],f'{tris} vs {m["individual_triangles"]}')
ck('GLB mesh group count matches manifest',parts==m['individual_mesh_parts'],f'{parts} vs {m["individual_mesh_parts"]}')
pivots=list(csv.DictReader(open(ROOT/'Animation_Rigs.csv')))
ck('Exactly 72 unique animation pivots',len(pivots)==72 and len({p['Pivot'] for p in pivots})==72)
ck('All animation pivots are exported transform nodes',all(p['Pivot'] in nodes for p in pivots),[p['Pivot'] for p in pivots if p['Pivot'] not in nodes][:6])
anchors=list(csv.DictReader(open(ROOT/'Terminal_Anchor_Plan_242.csv')))
ck('242 unique visual gold-rim sockets',len(anchors)==242 and len({a['AnchorID'] for a in anchors})==242)
ck('All 242 visual socket transforms exist in GLB',all('SOCKET_'+a['AnchorID'] in nodes for a in anchors),[a['AnchorID'] for a in anchors if 'SOCKET_'+a['AnchorID'] not in nodes][:6])
wires=list(csv.DictReader(open(ROOT/'Gameplay_Wiring_Concept_W001_W164.csv')))
ck('164 unique sequential conceptual connections',len(wires)==164 and all(w['WireID']==f'W{j+1:03d}' for j,w in enumerate(wires)))
ck('Every conceptual wire anchor owned by the correct model',all(any(a['AnchorID']==w[side+'VisualAnchor'] and a['Model']==w[side+'Model'] for a in anchors) for w in wires for side in ('Source','Target')))
logical=list(csv.DictReader(open(ROOT/'Terminal_Logical_Port_Reconciliation_328.csv')))
ck('328 independent unique logical endpoint declarations',len(logical)==328 and len({p['FunctionalPortID'] for p in logical})==328)
conflicts=list(csv.DictReader(open(ROOT/'Terminal_Anchor_Signal_Conflict_Report.csv')))
ck('All reused visual sockets have reconciliation records',len(conflicts)==m['anchor_reconciliation_conflicts'],len(conflicts))
critical=[w for w in wires if w['Critical']=='YES']
ck('W145 only critical with exact source/destination',len(critical)==1 and critical[0]['WireID']=='W145' and critical[0]['SourceModel']=='L88_CabinPressurePrimaryTransducer' and critical[0]['TargetModel']=='L88_HabitatLifeSupportSafetyPLC',critical[0] if critical else '')
ck('W145 signal is analog and has separate functional ports',len(critical)==1 and critical[0]['SignalKind']=='ANALOG_FEEDBACK' and sum(p['WireID']=='W145' for p in logical)==2)
ck('Six objective specification rows',len(list(csv.DictReader(open(ROOT/'Objective_Certification_Plan_6.csv'))))==6)
ck('Material texture sets and maps',len(list((ROOT/'Textures').glob('*.png')))==36 and len(json.load(open(ROOT/'Texture_Manifest.json'))['materials'])==12)
ck('Four optional Unity art helper C# files',len(list((ROOT/'Unity_Optional').glob('*.cs')))==4)
ck('13 separate physical process-flow concept links',len(list(csv.DictReader(open(ROOT/'Physical_Process_Flow_Concept.csv'))))==13)
ck('No claimed Unity gameplay/compilation/Play Mode tests',not m['actual_player_wires_included'] and not m['unity_compilation_tested'] and not m['six_runtime_objectives_implemented'] and not m['real_atmosphere_gas_solver_included'])
report=dict(level='level_88',structural_checks_passed=all(c['passed'] for c in checks),passes=sum(c['passed'] for c in checks),fails=sum(not c['passed'] for c in checks),total_checks=len(checks),checks=checks,
 individual_model_triangles=tris,individual_mesh_parts=parts,unity_compilation_tested=False,unity_play_mode_tested=False,
 independently_imported_fbx_in_Unity_or_DCC=False,actual_wires=False,actual_process_physics=False,objectives_implemented=False,
 disclaimer='Structural checks are not a substitute for Unity importer, topology, gameplay or AAA hand-authored production qualification')
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
with (ROOT/'Checksums_SHA256.txt').open('w') as f:
 for p in sorted(ROOT.rglob('*')):
  if p.is_file() and p.name!='Checksums_SHA256.txt' and '__pycache__' not in p.parts:
   with p.open('rb') as inp: sha=hashlib.sha256(inp.read()).hexdigest()
   f.write(sha+'  '+str(p.relative_to(ROOT))+'\n')
print('SUMMARY',report['passes'],'/',report['total_checks'],'triangles',tris,'mesh parts',parts,flush=True)
if not report['structural_checks_passed']:raise SystemExit(2)
