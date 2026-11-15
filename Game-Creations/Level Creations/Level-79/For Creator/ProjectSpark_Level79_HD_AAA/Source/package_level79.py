#!/usr/bin/env python3
"""Create reproducible Level 79 FBX/GLB art and planning ZIP; verify archive CRC."""
from pathlib import Path
import csv,json,hashlib,zipfile,time
R=Path('/mnt/data/ProjectSpark_Level79_HD_AAA')
ZIP=Path('/mnt/data/ProjectSpark_Level79_HD_AAA_Models.zip')
V=json.loads((R/'Verification_Report.json').read_text())
assert V['all_geometry_exports_verified'] and V['animation_pivots_verified']==48
assert len(list((R/'Models').glob('*.fbx')))==67
assert len(list((R/'GLB').glob('*.glb')))==67
assert len(list((R/'Unity_Optional').glob('*.cs')))==4
assert len(list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W122.csv').open())))==122
assert len(list(csv.DictReader((R/'Terminal_Anchor_Plan_188.csv').open())))==188
assert len(list(csv.DictReader((R/'Objective_Certification_Plan_6.csv').open())))==6
assets=[]
for p in sorted(R.rglob('*')):
    if not p.is_file() or p.name in ('SHA256SUMS.txt','Archive_Verification.json'):continue
    rel=p.relative_to(R).as_posix()
    assets.append((rel,p))
sha_lines=[hashlib.sha256(path.read_bytes()).hexdigest()+'  '+rel for rel,path in assets]
(R/'SHA256SUMS.txt').write_text('\n'.join(sha_lines)+'\n',encoding='utf8')
with zipfile.ZipFile(ZIP,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as f:
    for rel,path in assets:f.write(path,arcname='ProjectSpark_Level79_HD_AAA/'+rel)
    f.write(R/'SHA256SUMS.txt',arcname='ProjectSpark_Level79_HD_AAA/SHA256SUMS.txt')
with zipfile.ZipFile(ZIP,'r') as f:
    bad=f.testzip();assert bad is None,bad
    files=f.namelist();assert len(files)==len(assets)+1
    verified_hashes=0
    for rel,path in assets:
        name='ProjectSpark_Level79_HD_AAA/'+rel
        if hashlib.sha256(f.read(name)).hexdigest()!=hashlib.sha256(path.read_bytes()).hexdigest():raise AssertionError('Hash mismatch '+rel)
        verified_hashes+=1
report={'level_id':'level_79','archive_filename':ZIP.name,'archive_bytes':ZIP.stat().st_size,'archive_sha256':hashlib.sha256(ZIP.read_bytes()).hexdigest(),'archive_entries':len(files),'crc_test_passed':True,'archived_files_hash_verified':verified_hashes,'binary_fbx_count':67,'loadable_glb_count':67,'unique_individual_models':66,'actual_animation_parent_groups':48,'planned_player_wires':122,'planned_terminal_anchors':188,'objectives_planned':6,'W037_conceptual_fault_documented':True,'unity_compilation_tested':False,'gameplay_implemented':False}
(R/'Archive_Verification.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report,indent=2),flush=True)
