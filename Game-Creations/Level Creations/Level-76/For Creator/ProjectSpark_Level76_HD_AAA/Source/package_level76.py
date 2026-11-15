#!/usr/bin/env python3
"""Build and checksum real Level 76 mesh pack; verify ZIP container and declared art-only scope."""
import csv,json,hashlib,zipfile
from pathlib import Path
R=Path('/mnt/data/ProjectSpark_Level76_HD_AAA')
ZIP=Path('/mnt/data/ProjectSpark_Level76_HD_AAA_Models.zip')
m=json.loads((R/'Model_Manifest.json').read_text())
v=json.loads((R/'Verification_Report.json').read_text())
assert m['individual_models']==60 and m['individual_glbs']==60
assert len(v['individual_fbx_verified'])==60 and len(v['individual_glb_verified'])==60
assert v['assembled_fbx_verified'] and v['assembled_glb_verified']
assert len(list((R/'Models').glob('*.fbx')))==61
assert len(list((R/'GLB').glob('*.glb')))==61
assert len(list((R/'Unity_Optional').glob('*.cs')))==4
assert len(list(csv.DictReader((R/'Animation_Rigs.csv').open())))==42
assert len(list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W110.csv').open())))==110
assert len(list(csv.DictReader((R/'Terminal_Anchor_Plan_170.csv').open())))==170
assert sum(int(row['PlannedPlayerWires']) for row in csv.DictReader((R/'Gameplay_Wiring_Budget.csv').open()))==110
assert m['actual_player_wires_included'] is False and m['functional_spark_terminals_included'] is False
entries=sorted(p for p in R.rglob('*') if p.is_file() and p.name not in ('SHA256SUMS.txt','Archive_Verification.json') and '__pycache__' not in p.parts)
with (R/'SHA256SUMS.txt').open('w') as f:
 for p in entries:f.write(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p.relative_to(R)).replace('\\','/')+'\n')
entries=sorted(p for p in R.rglob('*') if p.is_file() and p.name!='Archive_Verification.json' and '__pycache__' not in p.parts)
print('Packaging',len(entries),'real files',flush=True)
with zipfile.ZipFile(ZIP,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7,allowZip64=True) as z:
 for p in entries:z.write(p,arcname=R.name+'/'+str(p.relative_to(R)).replace('\\','/'))
with zipfile.ZipFile(ZIP) as z:
 assert z.testzip() is None
 names=z.namelist()
 assert len(names)==len(entries)
 assert len([n for n in names if n.endswith('.fbx')])==61
 assert len([n for n in names if n.endswith('.glb')])==61
 assert all(n.startswith(R.name+'/') for n in names)
result=dict(archive=ZIP.name,archive_bytes=ZIP.stat().st_size,
 archive_sha256=hashlib.sha256(ZIP.read_bytes()).hexdigest(),archive_entries=len(names),archive_integrity_testzip='PASS',
 individual_fbx_models=60,individual_glb_models=60,assembled_fbx=1,assembled_glb=1,
 actual_equipment_triangles=m['individual_triangle_count'],actual_equipment_mesh_parts=m['individual_mesh_parts'],
 actual_pivot_groups=m['animation_parent_pivot_groups'],
 planned_player_wire_connections=110,planned_terminal_anchor_positions=170,
 actual_gameplay_wires_present=False,actual_sparkterminals_present=False,
 unity_compilation_tested=False,unity_play_mode_tested=False,
 notes='Procedural HD-inspired art pack only; no real circuit topology, six objectives, safe plating simulation, complete PBR art or baked animation clips.')
(R/'Archive_Verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2),flush=True)
