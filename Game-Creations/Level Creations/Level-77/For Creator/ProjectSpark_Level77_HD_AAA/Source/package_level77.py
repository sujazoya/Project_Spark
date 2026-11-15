#!/usr/bin/env python3
"""Final Level 77 archive with SHA-256 inventory; independently validate all asset counts and ZIP integrity."""
import csv,hashlib,json,zipfile
from pathlib import Path
R=Path('/mnt/data/ProjectSpark_Level77_HD_AAA')
TARGET=Path('/mnt/data/ProjectSpark_Level77_HD_AAA_Models.zip')
manifest=json.loads((R/'Model_Manifest.json').read_text())
report=json.loads((R/'Verification_Report.json').read_text())
assert manifest['individual_models']==62 and manifest['individual_glbs']==62
assert report['all_geometry_exports_verified'] is True
assert len(report['individual_fbx_verified'])==62 and len(report['individual_glb_verified'])==62
assert report['assembled_fbx_verified'] and report['assembled_glb_verified']
assert len(list((R/'Models').glob('*.fbx')))==63
assert len(list((R/'GLB').glob('*.glb')))==63
assert len(list((R/'Unity_Optional').glob('*.cs')))==4
assert len(list(csv.DictReader((R/'Animation_Rigs.csv').open())))==44
assert len(list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W114.csv').open())))==114
assert len(list(csv.DictReader((R/'Terminal_Anchor_Plan_176.csv').open())))==176
assert len(list(csv.DictReader((R/'Objective_Certification_Plan_6.csv').open())))==6
assert sum(int(row['PlannedPlayerWires']) for row in csv.DictReader((R/'Gameplay_Wiring_Budget.csv').open()))==114
assert manifest['actual_player_wires_included'] is False
assert manifest['functional_spark_terminals_included'] is False
assert manifest['selective_cmp_physics_included'] is False
assert manifest['six_authoritative_objectives_implemented'] is False
assert (R/'Material_Palette.json').is_file()
assert json.loads((R/'Material_Palette.json').read_text())['level_id']=='level_77'
w=list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W114.csv').open()))
assert w[54]['WireID']=='W055' and w[54]['SourceModel']=='L77_DualWavelengthBarrierEndpointSpectrometer'

def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        while chunk:=f.read(1024*1024):h.update(chunk)
    return h.hexdigest()
basefiles=sorted(p for p in R.rglob('*') if p.is_file() and p.name not in ('SHA256SUMS.txt','Archive_Verification.json') and '__pycache__' not in p.parts)
with (R/'SHA256SUMS.txt').open('w',encoding='utf-8') as f:
    for p in basefiles:
        f.write(digest(p)+'  '+p.relative_to(R).as_posix()+'\n')
entries=sorted(p for p in R.rglob('*') if p.is_file() and p.name!='Archive_Verification.json' and '__pycache__' not in p.parts)
with zipfile.ZipFile(TARGET,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7,allowZip64=True) as archive:
    for p in entries:archive.write(p,arcname=R.name+'/'+p.relative_to(R).as_posix())
with zipfile.ZipFile(TARGET,'r') as archive:
    names=archive.namelist()
    assert archive.testzip() is None
    assert len(names)==len(entries)
    assert len([p for p in names if p.endswith('.fbx')])==63
    assert len([p for p in names if p.endswith('.glb')])==63
    assert len([p for p in names if p.endswith('.cs')])==4
    assert all(p.startswith(R.name+'/') for p in names)
summary=dict(archive=TARGET.name,archive_bytes=TARGET.stat().st_size,archive_sha256=digest(TARGET),
 archive_entries=len(names),archive_integrity_testzip='PASS',
 individual_fbx_models=62,individual_glb_models=62,assembled_fbx_models=1,assembled_glb_models=1,
 equipment_triangles=manifest['individual_triangle_count'],mesh_parts=manifest['individual_mesh_parts'],
 named_animation_pivot_groups=44,planned_player_wire_connections=114,planned_terminal_positions=176,
 actual_gameplay_wires_included=False,actual_sparkterminals_included=False,unity_compilation_tested=False,unity_play_mode_tested=False,
 notes='Structurally valid modular procedural HD/AAA-inspired art ONLY; final artist-authored PBR materials, Unity compilation/Play Mode, selectable circuits, CMP material-removal physics, PLC and six level objectives are not included.')
(R/'Archive_Verification.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,indent=2),flush=True)
