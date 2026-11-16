#!/usr/bin/env python3
"""Structure-only verification of Project Spark Level90 art, never a Unity/runtime test."""
from pathlib import Path
from collections import Counter,defaultdict
import json,csv,hashlib,struct
import trimesh
from PIL import Image
ROOT=Path(__file__).resolve().parent.parent
report={'level_id':'level_90','validation_type':'STRUCTURAL_ART_PACKAGE_ONLY','checks':[],'unity_compilation_tested':False,
        'unity_playmode_tested':False,'real_sparkterminal_tested':False,'runtime_propulsion_tested':False}
def ok(name,p,detail=''):
 report['checks'].append({'name':name,'passed':bool(p),'detail':str(detail)[:500]});print(('PASS' if p else 'FAIL'),name,detail,flush=True)
 return p
m=json.loads((ROOT/'Model_Manifest.json').read_text())
rows=lambda file:list(csv.DictReader((ROOT/file).open(newline='',encoding='utf8')))
wire=rows('Gameplay_Wiring_Concept_W001_W176.csv');anchors=rows('Terminal_Anchor_Plan_254.csv');logical=rows('Terminal_Logical_Port_Reconciliation_352.csv');pivots=rows('Animation_Rigs.csv');conf=rows('Terminal_Anchor_Signal_Conflict_Report.csv');obj=rows('Objective_Certification_Plan_6.csv')
files=list((ROOT/'Models').glob('*.fbx'));glbs=list((ROOT/'GLB').glob('*.glb'))
ok('Model count',m['individual_models']==94,m['individual_models'])
ok('FBX files including showcase',len(files)==95,len(files))
ok('GLB files including showcase',len(glbs)==95,len(glbs))
ok('Modular triangle count',m['individual_triangles']>=100000,m['individual_triangles'])
ok('Source mesh part count',m['individual_mesh_parts']>500,m['individual_mesh_parts'])
ok('Animation pivot count',len(pivots)==80,len(pivots))
ok('Visual socket count',len(anchors)==254,len(anchors))
ok('Conceptual unique wires',len(wire)==176 and len({v['WireID'] for v in wire})==176,len(wire))
ok('Logical endpoints 2 per wire',len(logical)==352,len(logical))
ok('Six design objectives',len(obj)==6,len(obj))
ok('All wires marked conceptual',all(w['ConceptStatus']=='CONCEPT_ONLY_NO_PLAYER_WIRE' for w in wire),'not actual player wires')
ok('All sockets marked decorative',all(a['Status']=='ART_ANCHOR_NOT_SPARKTERMINAL' for a in anchors),'not SparkTerminals')
ok('Unique anchor IDs',len({x['AnchorID'] for x in anchors})==254,'P001...P254')
ok('All anchor references valid',all(x['SourceVisualAnchor'] in {a['AnchorID'] for a in anchors} and x['TargetVisualAnchor'] in {a['AnchorID'] for a in anchors} for x in wire),'all mapped')
ok('Source and target models exist',all(v['SourceModel'] in {z['name'] for z in m['models']} and v['TargetModel'] in {z['name'] for z in m['models']} for v in wire),'all grounded')
ok('Critical one W157',[(w['WireID'],w['Critical']) for w in wire if w['Critical']=='YES']==[('W157','YES')],'W157')
w157=wire[156];ok('W157 physical concept endpoints',w157['SourceModel']=='L90_NeutralizerCurrentTransducer' and w157['TargetModel']=='L90_PropulsionSafetyPLC' and w157['SignalKind']=='ANALOG_FEEDBACK',f"{w157['SourceVisualAnchor']}->{w157['TargetVisualAnchor']}")
ok('W157 port identities',any(z['WireID']=='W157' and z['FunctionalPortID']=='TRUSTED_NEUTRALIZER_CURRENT_OUT' for z in logical) and any(z['WireID']=='W157' and z['FunctionalPortID']=='TRUSTED_NEUTRALIZER_CURRENT_IN' for z in logical),'two separate logical contacts')
ok('Shared anchor reconciliation report',len(conf)==m['anchor_reconciliation_conflicts'],len(conf))
ok('Signal conflicts not silently merged',all(x['Requirement'].startswith('SPLIT_') for x in conf),'per-anchor split required')
ok('No runtime claims',all(not m.get(k,True) for k in ('actual_player_wires_included','functional_sparkterminals_included','real_hall_plasma_solver_included','runtime_safety_plc_included','six_runtime_objectives_implemented','unity_playmode_tested')),'art-only')
fbxbytes=b'Kaydara FBX Binary  \x00\x1a\x00'
allgood=True
for model in m['models']:
 ff=ROOT/'Models'/model['file'];gg=ROOT/'GLB'/model['glb'];fhead=ff.read_bytes()[:27]
 if not (fhead[:23]==fbxbytes and struct.unpack('<I',fhead[23:27])[0]==7400):allgood=False;print('BAD FBX HEADER',ff)
ok('94 valid binary FBX 7400 headers',allgood,'binary FBX not glTF or a renamed image')
allgood=True;counttri=0;read_pivots=set();read_sock=set();models_with_pivots=0;errors=[]
for model in m['models']:
 try:
  gg=ROOT/'GLB'/model['glb'];scene=trimesh.load(gg,file_type='glb',force='scene',process=False)
  nodes=set(scene.graph.nodes);tris=sum(len(g.faces) for g in scene.geometry.values())
  counttri+=tris
  pnames={p for p in nodes if p.startswith('ANIM_')};snames={p for p in nodes if p.startswith('SOCKET_P')}
  read_pivots.update(pnames);read_sock.update(snames)
  models_with_pivots+=bool(pnames)
  if tris!=model['triangle_count'] or len(snames)!=model['visual_socket_count']:
   allgood=False;errors.append((model['name'],tris,len(snames),model['triangle_count'],model['visual_socket_count']))
 except Exception as e:allgood=False;errors.append((model['name'],str(e)))
ok('94 GLB independently load with expected triangles/sockets',allgood,errors[:4])
ok('Exact total GLB geometry triangles',counttri==m['individual_triangles'],counttri)
ok('80 pivots present as named GLB scene nodes',len(read_pivots)==80 and read_pivots=={p['Pivot'] for p in pivots},len(read_pivots))
ok('254 gold visual socket parents present in GLBs',len(read_sock)==254,len(read_sock))
assembled=trimesh.load(ROOT/'GLB'/'Level90_Complete_ArtKit_Showcase.glb',force='scene',process=False)
ok('Assembled reference GLB scene loads',len(assembled.geometry)>0,len(assembled.geometry))
ok('Assembled GLB triangle count matches modular total',sum(len(g.faces) for g in assembled.geometry.values())==counttri,counttri)
ok('Assembled FBX header', (ROOT/'Models'/'Level90_Complete_ArtKit_Showcase.fbx').read_bytes()[:23]==fbxbytes,'binary')
textures=list((ROOT/'Textures').glob('*.png'))
ok('36 material reference PNGs',len(textures)==36,len(textures))
ok('Reference maps 1024 square',all(Image.open(p).size==(1024,1024) for p in textures),'1024x1024')
ok('Four optional Unity C# art scripts',len(list((ROOT/'Unity_Optional').glob('*.cs')))==4,'art-only')
ok('Unity menu paths target Level90',all('/90/' in l for p in (ROOT/'Unity_Optional').glob('*Editor.cs') for l in p.read_text().splitlines() if '[MenuItem(' in l),'menu paths')
ok('Original source generator included',all((ROOT/'Source'/f).exists() for f in ('build_level90.py','geometry_export_core.py','model_names.txt','make_textures.py','make_support_docs.py')),'reproducible geometry')
report.update(total_checks=len(report['checks']),passed=sum(c['passed'] for c in report['checks']),failed=sum(not c['passed'] for c in report['checks']),
 visual_pivot_nodes=len(read_pivots),decorative_socket_nodes=len(read_sock),individual_triangle_sum=counttri,
 source_w157={'source':w157['SourceModel'],'source_art_anchor':w157['SourceVisualAnchor'],'destination':w157['TargetModel'],'destination_art_anchor':w157['TargetVisualAnchor']})
(ROOT/'Verification_Report.json').write_text(json.dumps(report,indent=2))
print('VALIDATION',report['passed'],'/',report['total_checks'],'checks passed; NOT Unity-tested',flush=True)
if report['failed']:raise SystemExit(1)
