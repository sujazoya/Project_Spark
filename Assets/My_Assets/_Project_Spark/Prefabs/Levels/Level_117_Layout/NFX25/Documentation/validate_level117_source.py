#!/usr/bin/env python3
"""Validate Level117 source file consistency; not a Unity compile or electrical certificate."""
import csv, pathlib, sys
p=pathlib.Path(__file__).resolve().parent.parent

def rows(filename):
    with (p/'Data'/filename).open(newline='',encoding='utf-8') as f:
        return list(csv.DictReader(f))

models=rows('ModelManifest.csv'); bom=rows('ComponentBOM.csv')
placements=rows('ComponentPlacements.csv')
routes=rows('CopperRoutes_304_REQUIRES_SCHEMATIC.csv')
vias=rows('ViaTasks_108_REQUIRES_SCHEMATIC.csv')
assert (len(models),len(bom),len(placements),len(routes),len(vias))==(272,220,220,304,108)
assert len({x['asset_id'] for x in models})==272
assert len({x['ref'] for x in bom})==220
assert len({x['ref'] for x in placements})==220
assert {x['ref'] for x in placements}=={x['ref'] for x in bom}
assert {x['asset_id'] for x in placements} <= {x['asset_id'] for x in models}
for row in models:
    assert row['obj']!='NOT_INCLUDED' and (p/'Models_OBJ'/row['obj']).is_file(),row
    assert (p/'Models_GLB'/row['glb']).is_file(),row
assert sum(x['installable']=='TRUE' for x in models)==220
assert (p/'Models_OBJ'/'273_L117_AssembledReference.obj').is_file()
assert (p/'Models_GLB'/'273_L117_AssembledReference.glb').is_file()
assert all(x['source_pad']==x['dest_pad']==x['net_id']=='UNASSIGNED' and x['certifiable']=='FALSE' for x in routes)
assert all(x['net_id']=='UNASSIGNED' and x['start_layer']==x['end_layer']=='TBD' and x['verified']=='FALSE' for x in vias)
assert len(list((p/'Textures').glob('*_BaseColor.png')))==32
assert len(list((p/'URP_Packed').glob('*_URP_MetallicSmoothness.png')))==32
board=(p/'Models_OBJ'/'221_L117_PCB_Main.obj')
xyz=[]
for line in board.read_text().splitlines():
    if line.startswith('v '): xyz.append([float(v) for v in line.split()[1:4]])
assert xyz
assert abs(max(v[0] for v in xyz)-min(v[0] for v in xyz)-.460)<.00001
assert abs(max(v[2] for v in xyz)-min(v[2] for v in xyz)-.320)<.00001
print('[L117] PASS: 272 models, 220 BOM, 304 blocked routes, 108 blocked vias, 32 PBR families, PCB 460x320 mm.')
print('WARNING: source consistency only; Unity compilation, optical link gameplay and 6/6 NOT TESTED.')
