#!/usr/bin/env python3
"""Actual structural verifications only. NOT Unity import, Play Mode, protection safety or gameplay certification."""
from pathlib import Path
import csv,json,struct,hashlib,math
from collections import Counter
import trimesh
ROOT=Path(__file__).resolve().parent.parent
m=json.loads((ROOT/'Model_Manifest.json').read_text())
checks=[]
def ck(label,status,detail=''):
 checks.append(dict(name=label,passed=bool(status),detail=str(detail)))
 print(('PASS ' if status else 'FAIL ')+label+' '+str(detail)[:85],flush=True)
def rows(p):return list(csv.DictReader(open(ROOT/p,encoding='utf8')))
models=sorted((ROOT/'Models').glob('??_L89_*.fbx'))
glbs=sorted((ROOT/'GLB').glob('??_L89_*.glb'))
ck('92 individually exported FBX model files',len(models)==92,len(models))
ck('92 corresponding GLB model files',len(glbs)==92,len(glbs))
ck('Both assembled reference models present',all((ROOT/x).is_file() for x in ['Models/Level89_Complete_ArtKit_Showcase.fbx','GLB/Level89_Complete_ArtKit_Showcase.glb']))
ck('Sequential manifest-matched FBX filenames',len(models)==92 and all(models[i].name==f'{i:02d}_{m["models"][i]["name"]}.fbx' for i in range(92)))
ck('Sequential matching GLB filenames',len(glbs)==92 and all(glbs[i].name==f'{i:02d}_{m["models"][i]["name"]}.glb' for i in range(92)))
header=b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400)
ck('All FBX files have correct binary FBX 7400 header',all(p.open('rb').read(27)==header for p in models+[ROOT/'Models/Level89_Complete_ArtKit_Showcase.fbx']))
ck('All FBX files contain mesh/model/connections nodes',all(all(k in p.read_bytes() for k in [b'Geometry',b'Model',b'Connections',b'Vertices',b'PolygonVertexIndex']) for p in models))
ck('All GLBs have glTF 2.0 binary signature',all(p.open('rb').read(4)==b'glTF' for p in glbs+[ROOT/'GLB/Level89_Complete_ArtKit_Showcase.glb']))
tris=0;parts=0;nodes=set();errors=[];finite=True;nonempty=True
for p in glbs:
 try:
  scene=trimesh.load(str(p),file_type='glb',force='scene',process=False)
  if not scene.geometry:nonempty=False
  for mesh in scene.geometry.values():
   tris+=len(mesh.faces)
   finite=finite and bool(math.isfinite(float(mesh.bounds[0].sum()))) and bool(math.isfinite(float(mesh.bounds[1].sum())))
  parts+=len(scene.geometry)
  nodes.update(scene.graph.nodes)
 except Exception as e:errors.append((p.name,str(e)))
ck('All GLB files independently load as mesh scenes',not errors and len(glbs)==92,errors[:2])
ck('Every GLB contains nonempty geometry',nonempty)
ck('All evaluated geometry bounding values are finite',finite)
ck('GLB triangle total matches actual FBX/GLB manifest',tris==m['individual_triangles'],f'{tris} / {m["individual_triangles"]}')
ck('GLB unique mesh group count matches manifest',parts==m['individual_mesh_parts'],f'{parts} / {m["individual_mesh_parts"]}')
pivots=rows('Animation_Rigs.csv');sockets=rows('Terminal_Anchor_Plan_248.csv')
ck('Exactly 76 unique animation pivot definitions',len(pivots)==76 and len({x['Pivot'] for x in pivots})==76)
ck('Every pivot name appears as exported GLB transform',all(q['Pivot'] in nodes for q in pivots),[p['Pivot'] for p in pivots if p['Pivot'] not in nodes][:3])
ck('Every pivot has a single valid model owner',all(q['Model'] in [n['name'] for n in m['models']] for q in pivots))
ck('Exactly 248 unique decorated socket definitions',len(sockets)==248 and len({x['AnchorID'] for x in sockets})==248)
ck('All 248 socket transform nodes exist in GLBs',all('SOCKET_'+q['AnchorID'] in nodes for q in sockets),[q['AnchorID'] for q in sockets if 'SOCKET_'+q['AnchorID'] not in nodes][:4])
ck('Every socket belongs to a valid equipment model',all(q['Model'] in [x['name'] for x in m['models']] for q in sockets))
wires=rows('Gameplay_Wiring_Concept_W001_W170.csv')
ck('Exactly 170 sequential planned wires',len(wires)==170 and all(w['WireID']==f'W{j+1:03d}' for j,w in enumerate(wires)))
ck('All proposed wire source/target socket IDs match owning equipment',all(any(s['AnchorID']==w[side+'VisualAnchor'] and s['Model']==w[side+'Model'] for s in sockets) for w in wires for side in ('Source','Target')))
logical=rows('Terminal_Logical_Port_Reconciliation_340.csv')
ck('340 unique proposed logical contact endpoints',len(logical)==340 and len({q['FunctionalPortID'] for q in logical})==340)
ck('Each WID has exactly 2 independently identified endpoints',all(Counter(q['WireID'] for q in logical)[w['WireID']]==2 for w in wires))
conflicts=rows('Terminal_Anchor_Signal_Conflict_Report.csv')
ck('Visual socket multi-use reconciliation report matches source',len(conflicts)==m['anchor_reconciliation_conflicts'],len(conflicts))
critical=[w for w in wires if w['Critical']=='YES']
ck('Only critical W149 with correct primary sensor and safety PLC',len(critical)==1 and critical[0]['WireID']=='W149' and critical[0]['SourceModel']=='L89_ShelterReferenceDosimeter' and critical[0]['TargetModel']=='L89_HeliosSafetyPLC',critical[0] if critical else '')
ck('W149 is analog with separate named logical functional ports',len(critical)==1 and critical[0]['SignalKind']=='ANALOG_FEEDBACK' and {x['FunctionalPortID'] for x in logical if x['WireID']=='W149'}=={'TRUSTED_SHELTER_DOSE_RATE_OUT','TRUSTED_SHELTER_DOSE_RATE_IN'})
ck('Six distinct objective design declarations',len(rows('Objective_Certification_Plan_6.csv'))==6)
ck('Eight budget groups sum to exactly 170',len(rows('Gameplay_Wiring_Budget.csv'))==8 and sum(int(q['Count']) for q in rows('Gameplay_Wiring_Budget.csv'))==170)
ck('12 material sets, all 36 PNG texture reference maps',len(list((ROOT/'Textures').glob('*.png')))==36 and len(json.loads((ROOT/'Texture_Manifest.json').read_text())['materials'])==12)
ck('Four optional Unity art scripts',len(list((ROOT/'Unity_Optional').glob('*.cs')))==4)
ck('17 physical shielding process-planning links',len(rows('Physical_Shielding_Process_Concept.csv'))==17)
ck('Collision and LOD production plan covers 92 modular models',len(rows('Collision_LOD_Production_Plan.csv'))==92)
ck('Source build and README delivered',all((ROOT/f).exists() for f in ['Source/build_level89.py','Source/geometry_export_core.py','Source/make_textures.py','README_Unity.txt']))
ck('No false claims of Unity gameplay/compilation/tested solver',all(not m[k] for k in ['actual_player_wires_included','functional_sparkterminals_included','real_particle_transport_solver_included','runtime_safety_plc_included','six_runtime_objectives_implemented','unity_compilation_tested','unity_playmode_tested']))
report=dict(level_id='level_89',structural_checks_passed=all(x['passed'] for x in checks),passes=sum(x['passed'] for x in checks),fails=sum(not x['passed'] for x in checks),total_checks=len(checks),checks=checks,
 modular_triangles=tris,mesh_parts=parts,fbx_dcc_roundtrip_tested=False,unity_import_tested=False,unity_compilation_tested=False,unity_play_mode_tested=False,
 actual_sparkterminal_connectivity_implemented=False,radiation_simulation_implemented=False,objective_6_of_6_tested=False,
 disclaimer='These are source/package structural validations only, not proof of flight safety, AAA artist finish or functioning Unity level.')
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
with (ROOT/'Checksums_SHA256.txt').open('w',encoding='utf8') as out:
 for p in sorted(ROOT.rglob('*')):
  if not p.is_file() or p.name=='Checksums_SHA256.txt' or '__pycache__' in p.parts:continue
  with p.open('rb') as f:sha=hashlib.sha256(f.read()).hexdigest()
  out.write(sha+'  '+str(p.relative_to(ROOT))+'\n')
print('VALIDATION SUMMARY',report['passes'],'/',report['total_checks'],'tris',tris,'parts',parts,flush=True)
if report['fails']:raise SystemExit(2)
