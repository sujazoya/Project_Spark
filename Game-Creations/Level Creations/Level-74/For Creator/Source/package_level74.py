#!/usr/bin/env python3
"""Package and integrity-check the real Level 74 3D asset outputs."""
from pathlib import Path
import hashlib,zipfile,json
root=Path('/mnt/data/ProjectSpark_Level74_HD_AAA')
out=Path('/mnt/data/ProjectSpark_Level74_HD_AAA_Models.zip')
skip={'build.log','render.log','verify.log','support.log','SHA256SUMS.txt','Archive_Verification.json'}
files=sorted(x for x in root.rglob('*') if x.is_file() and x.name not in skip)
checks=[]
for p in files:
    checks.append(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.relative_to(root).as_posix())
(root/'SHA256SUMS.txt').write_text('\n'.join(checks)+'\n',encoding='utf-8')
manifest=json.loads((root/'Model_Manifest.json').read_text())
report=json.loads((root/'Verification_Report.json').read_text())
assert manifest['individual_models']==56 and manifest['individual_glbs']==56
assert manifest['animation_parent_pivot_groups']==42
assert report['all_geometry_exports_verified']
files=sorted(x for x in root.rglob('*') if x.is_file() and x.name not in {'build.log','render.log','verify.log','support.log','Archive_Verification.json'})
with zipfile.ZipFile(out,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7) as z:
    for p in files:z.write(p,(Path(root.name)/p.relative_to(root)).as_posix())
with zipfile.ZipFile(out) as z:
    assert z.testzip() is None
    names=z.namelist()
    nfbx=sum(a.endswith('.fbx') for a in names)
    nglb=sum(a.endswith('.glb') for a in names)
    ncs=sum(a.endswith('.cs') for a in names)
    assert nfbx==57 and nglb==57 and ncs==3,(nfbx,nglb,ncs)
    for f in ['Model_Manifest.json','Animation_Rigs.csv','Gameplay_Wiring_Budget.csv','Verification_Report.json','README_Unity.txt','SHA256SUMS.txt']:
        assert root.name+'/'+f in names,f
    assert len(names)==len(set(names))
    info={'archive':str(out),'bytes':out.stat().st_size,'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),
          'file_count':len(names),'fbx_files':nfbx,'glb_files':nglb,'unity_cs_helpers':ncs,
          'preview_png_files':sum(a.endswith('.png') for a in names),
          'actual_individual_geometry_triangles':manifest['individual_triangle_count'],
          'animation_parent_groups':manifest['animation_parent_pivot_groups'],
          'passed_archive_integrity':True,'unity_import_tested':False,'unity_playmode_tested':False,
          'actual_player_wires_included':False,'functional_terminal_components_included':False,
          'level_gameplay_and_objectives_implemented':False}
    (root/'Archive_Verification.json').write_text(json.dumps(info,indent=2),encoding='utf-8')
    print(json.dumps(info,indent=2))
