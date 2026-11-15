#!/usr/bin/env python3
from pathlib import Path
import hashlib,zipfile,json
root=Path('/mnt/data/ProjectSpark_Level73_HD_AAA')
out=Path('/mnt/data/ProjectSpark_Level73_HD_AAA_Models.zip')
skip={'build.log','render.log','verify.log','Archive_Verification.json'}
checksum_skip=skip|{'SHA256SUMS.txt'}
files=sorted([p for p in root.rglob('*') if p.is_file() and p.name not in checksum_skip])
checks=[]
for p in files:
    checks.append(f'{hashlib.sha256(p.read_bytes()).hexdigest()}  {p.relative_to(root).as_posix()}')
(root/'SHA256SUMS.txt').write_text('\n'.join(checks)+'\n')
files=sorted([p for p in root.rglob('*') if p.is_file() and p.name not in skip])
with zipfile.ZipFile(out,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7) as z:
    for p in files:
        z.write(p,(Path(root.name)/p.relative_to(root)).as_posix())
with zipfile.ZipFile(out) as z:
    corrupt=z.testzip()
    assert corrupt is None,corrupt
    names=z.namelist()
    num_fbx=sum(x.endswith('.fbx') for x in names)
    num_glb=sum(x.endswith('.glb') for x in names)
    assert num_fbx==55 and num_glb==55,(num_fbx,num_glb)
    assert 'ProjectSpark_Level73_HD_AAA/Model_Manifest.json' in names
    assert 'ProjectSpark_Level73_HD_AAA/Verification_Report.json' in names
    assert 'ProjectSpark_Level73_HD_AAA/SHA256SUMS.txt' in names
    assert len(names)==len(set(names))
    info={'archive_name':out.name,'archive_bytes':out.stat().st_size,'archive_sha256':hashlib.sha256(out.read_bytes()).hexdigest(),
          'file_count':len(names),'fbx_files':num_fbx,'glb_files':num_glb,'passed_zip_integrity_test':corrupt is None,
          'source_files':sum(x.startswith(root.name+'/Source/') for x in names),
          'unity_support_files':sum(x.endswith('.cs') and '/Unity_Optional/' in x for x in names),
          'preview_png_files':sum(x.endswith('.png') and '/Previews/' in x for x in names)}
    (root/'Archive_Verification.json').write_text(json.dumps(info,indent=2))
    print(json.dumps(info,indent=2))
