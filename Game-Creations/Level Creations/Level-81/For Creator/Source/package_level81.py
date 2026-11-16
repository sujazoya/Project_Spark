#!/usr/bin/env python3
"""Create Level 81 reproducible delivery ZIP with file hash manifest and CRC tests."""
from pathlib import Path
import json,zipfile,hashlib,csv
R=Path('/mnt/data/ProjectSpark_Level81_HD_AAA')
Z=Path('/mnt/data/ProjectSpark_Level81_HD_AAA_Models.zip')
files=sorted([p for p in R.rglob('*') if p.is_file() and p.name!='Checksums_SHA256.txt'])
with (R/'Checksums_SHA256.txt').open('w',encoding='utf8') as f:
 for p in files:
  digest=hashlib.sha256(p.read_bytes()).hexdigest()
  f.write(digest+'  '+p.relative_to(R).as_posix()+'\n')
files.append(R/'Checksums_SHA256.txt')
with zipfile.ZipFile(Z,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=5,allowZip64=True) as z:
 for p in files:
  z.write(p,arcname='ProjectSpark_Level81_HD_AAA/'+p.relative_to(R).as_posix())
with zipfile.ZipFile(Z,'r') as z:
 bad=z.testzip()
 assert bad is None,bad
 assert len(z.namelist())==len(files)
print('LEVEL81 PACKAGE VERIFIED',Z,'bytes',Z.stat().st_size,'files',len(files),'zip_sha256',hashlib.sha256(Z.read_bytes()).hexdigest())
