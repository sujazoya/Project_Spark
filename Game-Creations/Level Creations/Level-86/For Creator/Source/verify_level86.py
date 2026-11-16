#!/usr/bin/env python3
"""Verifies Level 86 procedural source files; cannot validate Unity compilation or play.
GLB geometry graph and FBX binary structure checked; texture references and plans.
"""
import os,struct,json,csv,hashlib,time
from collections import Counter
from pathlib import Path
import trimesh
ROOT=Path(__file__).resolve().parent.parent
checks=[]
def check(name,value,details=None):
 checks.append(dict(check=name,passed=bool(value),details=details))
 if not value: print('FAIL',name,details,flush=True)
 else: print('PASS',name,flush=True)
def rows(path):
 with open(ROOT/path,encoding='utf8',newline='') as f:return list(csv.DictReader(f))
m=json.loads((ROOT/'Model_Manifest.json').read_text())
model_files=sorted((ROOT/'Models').glob('??_*.fbx'));glb_files=sorted((ROOT/'GLB').glob('??_*.glb'))
check('82 independent FBX files',len(model_files)==82,len(model_files))
check('82 corresponding GLB files',len(glb_files)==82,len(glb_files))
check('one assembled alternative FBX and GLB',all((ROOT/f).is_file() for f in (m['assembled_fbx'],m['assembled_glb'])))
file_pairs=all((ROOT/'Models'/j['file']).is_file() and (ROOT/'GLB'/j['glb']).is_file() for j in m['models'])
check('manifest matches the exact 82 files',file_pairs)
fbx_structure=[]
for f in model_files+[ROOT/m['assembled_fbx']]:
 with open(f,'rb') as fp:
  header=fp.read(27);data=fp.read()
 okay=header[:23]==b'Kaydara FBX Binary  \x00\x1a\x00' and struct.unpack('<I',header[23:27])[0]==7400
 fbx_structure.append(okay and len(data)>1500 and b'Objects' in data and b'Connections' in data)
check('binary FBX 7400 structural headers and objects',all(fbx_structure),f'{sum(fbx_structure)}/{len(fbx_structure)}')
tri_tot=0;glb_pass=0;glb_nodes=[];all_geometries_positive=True
for f,j in zip(glb_files,m['models']):
 try:
  scene=trimesh.load(str(f),force='scene',process=False)
  tris=sum(len(g.faces) for g in scene.geometry.values())
  tri_tot+=tris
  glb_pass+=int(tris==j['triangle_count'] and len(scene.geometry)>0)
  glb_nodes.extend(scene.graph.nodes)
  all_geometries_positive=all_geometries_positive and all(len(g.faces)>0 for g in scene.geometry.values())
 except Exception as ex:
  print('GLB FAILED',f.name,str(ex),flush=True)
check('all GLB 2 geometry imports and exact triangle totals',glb_pass==82,f'{glb_pass}/82, {tri_tot} tris')
check('nonempty triangulated meshes in every GLB part',all_geometries_positive)
check('manifest individual triangle sum correct',tri_tot==m['individual_triangles'],tri_tot)
piv=rows('Animation_Rigs.csv');anc=rows('Terminal_Anchor_Plan_230.csv');wire=rows('Gameplay_Wiring_Concept_W001_W152.csv');logical=rows('Terminal_Logical_Port_Reconciliation_304.csv')
check('64 unique announced animation pivots',len(piv)==64 and len(set(x['Pivot'] for x in piv))==64)
check('all 64 pivots physically present as GLB nodes',all(x['Pivot'] in glb_nodes for x in piv))
check('230 unique decorative socket art anchors',len(anc)==230 and len(set(x['AnchorID'] for x in anc))==230)
check('all 230 SOCKET_P### art transforms present in GLB files',all('SOCKET_'+x['AnchorID'] in glb_nodes for x in anc))
check('152 sequential, unique conceptual WIDs',len(wire)==152 and [x['WireID'] for x in wire]==['W%03d'%i for i in range(1,153)])
check('304 uniquely named independent wire-end contacts',len(logical)==304 and len(set(x['FunctionalPortID'] for x in logical))==304)
check('wire art anchor references exist',all(x['SourceVisualAnchor'] in {a['AnchorID'] for a in anc} and x['TargetVisualAnchor'] in {a['AnchorID'] for a in anc} for x in wire))
check('W129 trusted feedback exact model pair and analog signal',wire[128]['SourceModel']=='L86_SixAxisWristForceTorqueSensor' and wire[128]['TargetModel']=='L86_RoboticContactSafetyPLC' and wire[128]['SignalKind']=='ANALOG_FEEDBACK' and wire[128]['Critical']=='YES')
check('only W129 marked critical',sum(x['Critical']=='YES' for x in wire)==1)
check('6 exact planned objective definitions',len(rows('Objective_Certification_Plan_6.csv'))==6)
check('36 procedural reference PNG textures',len(list((ROOT/'Textures').glob('*.png')))==36)
check('4 optional Unity C# art helper source files',len(list((ROOT/'Unity_Optional').glob('*.cs')))==4)
check('no runtime or Unity-Play-Mode testing claim',m['unity_compilation_tested'] is False and m['unity_playmode_tested'] is False and m['actual_player_wires_included'] is False)
# Independently load the large reference GLB to prove it is not a 2D mockup.
try:
 s=trimesh.load(str(ROOT/m['assembled_glb']),force='scene',process=False)
 t=sum(len(g.faces) for g in s.geometry.values())
 check('full showcase GLB geometry import and total triangles',t==m['individual_triangles'],t)
except Exception as ex:check('full showcase GLB geometry import and total triangles',False,str(ex))
failed=[x['check'] for x in checks if not x['passed']]
report=dict(source='LOCAL_L86_GENERATED_ART_FILES',checked_utc=time.strftime('%Y-%m-%dT%H:%M:%SZ',time.gmtime()),
  passed=not failed,checks=checks,failed=failed,unity_compilation_tested=False,unity_playmode_tested=False,
  gameplay_wiring_implemented=False,authoritative_level_objectives_implemented=False)
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))
print('REPORT',len(checks),'tests',len(failed),'failures',flush=True)
raise SystemExit(0 if not failed else 1)
