#!/usr/bin/env python3
"""Package fully validated modular Level 78 3D art, 4 Unity helpers and truthful plans."""
import csv,hashlib,json,zipfile
from pathlib import Path
R=Path('/mnt/data/ProjectSpark_Level78_HD_AAA')
TARGET=Path('/mnt/data/ProjectSpark_Level78_HD_AAA_Models.zip')
manifest=json.loads((R/'Model_Manifest.json').read_text())
report=json.loads((R/'Verification_Report.json').read_text())
assert manifest['individual_models']==64 and manifest['individual_glbs']==64
assert report['all_geometry_exports_verified'] is True
assert len(report['individual_fbx_verified'])==64 and len(report['individual_glb_verified'])==64
assert report['assembled_fbx_verified'] and report['assembled_glb_verified']
assert len(list((R/'Models').glob('*.fbx')))==65
assert len(list((R/'GLB').glob('*.glb')))==65
assert len(list((R/'Unity_Optional').glob('*.cs')))==4
assert len(list(csv.DictReader((R/'Animation_Rigs.csv').open())))==46
assert len(list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W118.csv').open())))==118
assert len(list(csv.DictReader((R/'Terminal_Anchor_Plan_182.csv').open())))==182
assert len(list(csv.DictReader((R/'Objective_Certification_Plan_6.csv').open())))==6
assert sum(int(row['PlannedPlayerWires']) for row in csv.DictReader((R/'Gameplay_Wiring_Budget.csv').open()))==118
assert manifest['actual_player_wires_included'] is False
assert manifest['functional_spark_terminals_included'] is False
assert manifest['dielectric_pecvd_physics_included'] is False
assert manifest['six_authoritative_objectives_implemented'] is False
assert json.loads((R/'Material_Palette.json').read_text())['level_id']=='level_78'
w=list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W118.csv').open()))
assert w[82]['WireID']=='W083' and w[82]['SourceModel']=='L78_DielectricLeakageCurrentElectrometer' and w[82]['TargetModel']=='L78_DielectricReliabilityController'

def digest(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  while chunk:=f.read(1024*1024):h.update(chunk)
 return h.hexdigest()
basefiles=sorted(p for p in R.rglob('*') if p.is_file() and p.name not in ('SHA256SUMS.txt','Archive_Verification.json') and '__pycache__' not in p.parts)
with (R/'SHA256SUMS.txt').open('w',encoding='utf-8') as f:
 for p in basefiles:f.write(digest(p)+'  '+p.relative_to(R).as_posix()+'\n')
entries=sorted(p for p in R.rglob('*') if p.is_file() and p.name!='Archive_Verification.json' and '__pycache__' not in p.parts)
with zipfile.ZipFile(TARGET,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7,allowZip64=True) as archive:
 for p in entries:archive.write(p,arcname=R.name+'/'+p.relative_to(R).as_posix())
with zipfile.ZipFile(TARGET,'r') as archive:
 names=archive.namelist()
 assert archive.testzip() is None
 assert len(names)==len(entries)
 assert len([n for n in names if n.endswith('.fbx')])==65
 assert len([n for n in names if n.endswith('.glb')])==65
 assert len([n for n in names if n.endswith('.cs')])==4
 assert all(n.startswith(R.name+'/') for n in names)
summary=dict(archive=TARGET.name,archive_bytes=TARGET.stat().st_size,archive_sha256=digest(TARGET),archive_entries=len(names),archive_integrity_testzip='PASS',individual_fbx_models=64,individual_glb_models=64,assembled_fbx_models=1,assembled_glb_models=1,equipment_triangles=manifest['individual_triangle_count'],mesh_parts=manifest['individual_mesh_parts'],named_animation_pivot_groups=46,planned_player_wire_connections=118,planned_terminal_positions=182,actual_gameplay_wires_included=False,actual_sparkterminals_included=False,unity_compilation_tested=False,unity_play_mode_tested=False,notes='Structurally verified original modular procedural HD/AAA-inspired mesh art only. No artist-authored production PBR, LODs, Unity compilation, working electrical circuit, PECVD/UV reliability physics, PLC or six gameplay objectives.')
(R/'Archive_Verification.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
print(json.dumps(summary,indent=2),flush=True)
