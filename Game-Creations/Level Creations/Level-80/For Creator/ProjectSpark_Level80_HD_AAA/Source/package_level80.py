#!/usr/bin/env python3
"""Create reproducible Level80 art archive, SHA-256 verification and test ZIP integrity."""
from pathlib import Path
import hashlib,json,zipfile
ROOT=Path('/mnt/data/ProjectSpark_Level80_HD_AAA')
ZIP=Path('/mnt/data/ProjectSpark_Level80_HD_AAA_Models.zip')
for path in ROOT.rglob('*'):
    if path.is_file() and ('__pycache__' in path.parts or path.suffix=='.pyc'):
        path.unlink()
for d in ROOT.rglob('__pycache__'):
    if d.is_dir():
        try:d.rmdir()
        except OSError:pass
files=sorted(p for p in ROOT.rglob('*') if p.is_file() and p.name!='SHA256SUMS.txt')
manifest=[]
for p in files:
    data=p.read_bytes()
    manifest.append((hashlib.sha256(data).hexdigest(),p.relative_to(ROOT).as_posix()))
(ROOT/'SHA256SUMS.txt').write_text(''.join(f'{h}  {p}\n' for h,p in manifest),encoding='utf8')
with zipfile.ZipFile(ZIP,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=5,allowZip64=True) as z:
    for p in sorted(x for x in ROOT.rglob('*') if x.is_file()):
        z.write(p,arcname=ROOT.name+'/'+p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(ZIP,'r') as z:
    bad=z.testzip()
    if bad:raise RuntimeError('ZIP CRC check failed '+bad)
    count=len(z.namelist())
assert ZIP.stat().st_size>1000000
print('PACKAGED',ZIP,'files',count,'zipBytes',ZIP.stat().st_size,'zipIntegrity',bad,flush=True)
