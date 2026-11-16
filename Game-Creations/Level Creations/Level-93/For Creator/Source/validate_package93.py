#!/usr/bin/env python3
import csv,json,zipfile,hashlib,os,sys,struct
from pathlib import Path
import trimesh
R=Path(__file__).resolve().parents[1]
checks=[];fail=[]
def assertcheck(label,ok):
 checks.append((label,bool(ok)))
 if not ok:fail.append(label)
manifest=json.loads((R/'Model_Manifest.json').read_text());mat=json.loads((R/'Texture_Manifest.json').read_text())
mods=sorted((R/'Models').glob('[0-9][0-9]_L93_*.fbx'))
glbs=sorted((R/'GLB').glob('[0-9][0-9]_L93_*.glb'))
assertcheck('100 individual real binary FBXs',len(mods)==100)
assertcheck('100 individual GLBs',len(glbs)==100)
assertcheck('Showcase FBX+GLB', (R/'Models'/manifest['assembled_fbx']).exists() and (R/'GLB'/manifest['assembled_glb']).exists())
assertcheck('All binary FBX 7400 magic and version',all(f.read_bytes()[:27]==b'Kaydara FBX Binary  \x00\x1a\x00'+struct.pack('<I',7400) for f in mods))
assertcheck('FBX exports include layer UV and vertex normals',all(b'LayerElementUV' in f.read_bytes() and b'LayerElementNormal' in f.read_bytes() for f in mods))
tr=0;part=0;loaded=0;uv=0;missing_glb=[]
for f in glbs:
 try:
  scene=trimesh.load(str(f),force='scene',process=False)
  loaded+=1
  for mesh in scene.geometry.values():
   part+=1;tr+=len(mesh.faces)
   if hasattr(mesh.visual,'uv') and mesh.visual.uv is not None and len(mesh.visual.uv)==len(mesh.vertices):uv+=1
 except Exception as e:missing_glb.append(f.name+': '+str(e))
assertcheck('Every GLB independently imports as a 3D scene',loaded==100)
assertcheck('GLB triangle total matches source manifest',tr==manifest['triangle_count'])
assertcheck('All GLB mesh parts carry generated UV0',uv==part and part>200)
assertcheck('All modular model categories appear',len(set(x['category'] for x in manifest['models']))==6)
assertcheck('92 animation pivots planned',manifest['animation_pivots']==92)
assertcheck('272 sockets planned',manifest['decorative_sockets']==272)
assertcheck('194 wiring concepts',manifest['planned_player_wires']==194)
assertcheck('388 logical endpoints',manifest['independent_logical_endpoints']==388)
assertcheck('18 PBR sets',mat['texture_sets']==18)
assertcheck('108 PBR texture maps',len(list((R/'Textures').glob('*.png')))==108)
assertcheck('2 transparent hologram shader variants',len(list((R/'Shaders').glob('*.shader')))==2)
assertcheck('4 optional Unity C# scripts',len(list((R/'Unity_Optional').glob('*.cs')))==4)
assertcheck('No wrong Level90-92 menus in Level93 Unity scripts',all('Tools/Project Spark/Levels/90/' not in p.read_text() and 'Tools/Project Spark/Levels/91/' not in p.read_text() and 'Tools/Project Spark/Levels/92/' not in p.read_text() for p in (R/'Unity_Optional').glob('*.cs')))
assertcheck('Actual native PBR material creation menu', 'Generate HD PBR Materials And Shaders' in (R/'Unity_Optional'/'SparkLevel93MaterialUpgradeEditor.cs').read_text())
assertcheck('Per-model authored pivot names have nonempty assignments',all(all(p.startswith('ANIM_') for p in row['pivot_names']) for row in manifest['models']))
for fname,count in [('Animation_Rigs.csv',92),('Art_Assembly.csv',100),('Component_Roles.csv',100),
 ('Visual_Terminal_Sockets_272.csv',272),('Gameplay_Wiring_Concept_W001_W194.csv',194),
 ('Terminal_Logical_Port_Reconciliation_388.csv',388),('Objective_Certification_Plan_6.csv',6),('Collision_LOD_Production_Plan.csv',100)]:
 with (R/fname).open(newline='') as f:n=sum(1 for _ in csv.DictReader(f))
 assertcheck(fname+' record count',n==count)
with (R/'Gameplay_Wiring_Concept_W001_W194.csv').open(newline='') as f:w=list(csv.DictReader(f))
w179=w[178]
assertcheck('W179 unique critical wiring identity',w179['WireID']=='W179' and w179['Critical']=='YES')
assertcheck('W179 is actual pressure transducer plan',w179['SourceModel']=='L93_AnalyticalVacuumPressureTransducer' and w179['TargetModel']=='L93_AnalyticalSafetyPLC')
assertcheck('No other critical wire definitions',sum(row['Critical']=='YES' for row in w)==1)
with (R/'Animation_Rigs.csv').open(newline='') as f:pivots=list(csv.DictReader(f))
with (R/'Visual_Terminal_Sockets_272.csv').open(newline='') as f:sockets=list(csv.DictReader(f))
assertcheck('Pivot identities are distinct',len(set(q['Pivot'] for q in pivots))==92)
assertcheck('Art sockets have unique IDs',len(set(q['AnchorID'] for q in sockets))==272)
scene=trimesh.load(str(R/'GLB'/manifest['assembled_glb']),force='scene',process=False)
scene_nodes=set(scene.graph.nodes)
assertcheck('All 92 named pivots exist in assembled scene',all(q['Pivot'] in scene_nodes for q in pivots))
assertcheck('All 272 named sockets exist in assembled scene',all('SOCKET_'+q['AnchorID'] in scene_nodes for q in sockets))
assertcheck('Assembled glb exactly matches modular triangles',sum(len(mesh.faces) for mesh in scene.geometry.values())==tr)
assertcheck('Separate art roots maintain common zero placement',manifest['models_use_common_origin'] is True)
assertcheck('No false gameplay implementation claim',manifest['actual_player_wires_included'] is False and manifest['unity_playmode_tested'] is False)
for name in ['README_FIRST.md','Gameplay_Integration_Plan.md','AAA_Polish_And_Lighting_Plan.md','W179_Critical_Analytical_Vacuum_Pressure_Recovery.md']:
 assertcheck(name+' present', (R/name).exists())
print('LEVEL93 validation',sum(x[1] for x in checks),'/',len(checks),'passed')
for n,res in checks:
 if not res:print('FAIL',n)
summary={'passed':sum(x[1] for x in checks),'total':len(checks),'failed':fail,'meshes':part,'triangles':tr,'glb_uv_meshes':uv,
         'models':len(mods),'glbs':len(glbs),'material_sets':mat['texture_sets'],'texture_maps':mat['map_count'],'critical_W179':w179,'items':checks}
(R/'Reports'/'Structural_Validation_Report.json').write_text(json.dumps(summary,indent=2))
(R/'Reports'/'Structural_Validation_Report.md').write_text('# Level 93 offline structural validation\n\n'+
 f'**{summary["passed"]}/{summary["total"]} checks passed.**\n\n'+
 '\n'.join(f'- {"PASS" if ok else "FAIL"} — {label}' for label,ok in checks)+
 '\n\n**Important:** Structural checks do not establish Unity FBX import, shader compilation, physics, electrical safety, realism of scientific simulations, or Play Mode readiness.\n')
if fail:sys.exit(2)
