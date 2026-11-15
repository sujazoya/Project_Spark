#!/usr/bin/env python3
"""Create reproducible Level 75 3D art ZIP and independently test archive integrity."""
import hashlib,json,zipfile,csv
from pathlib import Path
ROOT=Path('/mnt/data/ProjectSpark_Level75_HD_AAA')
ZIP=Path('/mnt/data/ProjectSpark_Level75_HD_AAA_Models.zip')
manifest=json.loads((ROOT/'Model_Manifest.json').read_text())
report=json.loads((ROOT/'Verification_Report.json').read_text())
assert manifest['individual_models']==58 and manifest['individual_glbs']==58
assert len(report['individual_fbx_verified'])==58 and len(report['individual_glb_verified'])==58
assert report['assembled_fbx_verified'] and report['assembled_glb_verified']
assert len(list((ROOT/'Models').glob('*.fbx')))==59
assert len(list((ROOT/'GLB').glob('*.glb')))==59
assert len(list((ROOT/'Unity_Optional').glob('*.cs')))==4
assert sum(int(x['PlannedPlayerWires']) for x in csv.DictReader((ROOT/'Gameplay_Wiring_Budget.csv').open()))==106
assert len(list(csv.DictReader((ROOT/'Gameplay_Wiring_Concept_W01_W106.csv').open())))==106
assert len(list(csv.DictReader((ROOT/'Terminal_Anchor_Plan_164.csv').open())))==164
entries=sorted(p for p in ROOT.rglob('*') if p.is_file() and '__pycache__' not in str(p))
with (ROOT/'SHA256SUMS.txt').open('w',encoding='utf-8') as f:
    for p in entries:
        sha=hashlib.sha256(p.read_bytes()).hexdigest()
        f.write(sha+'  '+str(p.relative_to(ROOT)).replace('\\','/')+'\n')
entries=sorted(p for p in ROOT.rglob('*') if p.is_file() and '__pycache__' not in str(p))
print('PACKAGING',len(entries),'files',flush=True)
with zipfile.ZipFile(ZIP,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7,allowZip64=True) as z:
    for idx,p in enumerate(entries):
        z.write(p,arcname='ProjectSpark_Level75_HD_AAA/'+str(p.relative_to(ROOT)).replace('\\','/'))
        if idx%30==0:print('packed',idx+1,'/',len(entries),flush=True)
with zipfile.ZipFile(ZIP) as z:
    bad=z.testzip()
    assert bad is None,('ZIP corrupted at',bad)
    files=z.namelist()
    assert len(files)==len(entries),(len(files),len(entries))
    assert len([x for x in files if x.endswith('.fbx')])==59
    assert len([x for x in files if x.endswith('.glb')])==59
    assert all(x.startswith('ProjectSpark_Level75_HD_AAA/') for x in files)
check={
'archive':ZIP.name,'archive_bytes':ZIP.stat().st_size,
'archive_sha256':hashlib.sha256(ZIP.read_bytes()).hexdigest(),
'archive_integrity_testzip':'PASS','archive_entries':len(entries),
'individual_binary_fbx':58,'individual_glb':58,'assembled_fbx':1,'assembled_glb':1,
'actual_equipment_triangles':manifest['individual_triangle_count'],
'actual_equipment_mesh_parts':manifest['individual_mesh_parts'],
'actual_pivot_parent_groups':manifest['animation_parent_pivot_groups'],
'actual_gameplay_wires_present':False,'actual_sparkterminals_present':False,
'planned_wire_connections':106,'planned_terminal_anchor_positions':164,
'actual_unity_compilation_tested':False,'actual_unity_play_mode_tested':False,
'notes':'Original procedural 3D art only; gameplay wiring and simulation require implementation in user Unity project.'}
(ROOT/'Archive_Verification.json').write_text(json.dumps(check,indent=2),encoding='utf8')
print('ARCHIVE PASS',json.dumps(check,indent=2),flush=True)
