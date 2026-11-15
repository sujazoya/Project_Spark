#!/usr/bin/env python3
"""Package Level 82 original binary FBX/GLB exports, textures and planning files; verify checksum and ZIP CRC."""
from pathlib import Path
import hashlib,zipfile
R=Path('/mnt/data/ProjectSpark_Level82_HD_AAA')
Z=Path('/mnt/data/ProjectSpark_Level82_HD_AAA_Models.zip')
files=sorted([p for p in R.rglob('*') if p.is_file() and p.name!='Checksums_SHA256.txt'])
with (R/'Checksums_SHA256.txt').open('w') as f:
 for p in files:
  f.write(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.relative_to(R).as_posix()+'\n')
files.append(R/'Checksums_SHA256.txt')
with zipfile.ZipFile(Z,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=5,allowZip64=True) as z:
 for p in files:z.write(p,arcname='ProjectSpark_Level82_HD_AAA/'+p.relative_to(R).as_posix())
with zipfile.ZipFile(Z) as z:
 assert z.testzip() is None
 assert len(z.namelist())==len(files)
print('PACKAGED',len(files),'files','size',Z.stat().st_size,'ZIP_SHA256',hashlib.sha256(Z.read_bytes()).hexdigest(),flush=True)
