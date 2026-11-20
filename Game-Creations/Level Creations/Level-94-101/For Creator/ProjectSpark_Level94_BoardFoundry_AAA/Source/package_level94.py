#!/usr/bin/env python3
from pathlib import Path
import zipfile,hashlib,json,os
R=Path(__file__).resolve().parents[1];OUT=R.parent
# reproducible checksums for all assets, excluding checksum itself and python caches
assetfiles=[p for p in R.rglob('*') if p.is_file() and not ('__pycache__' in p.parts) and p.name!='Checksums_SHA256.txt']
assetfiles.sort(key=lambda p:str(p.relative_to(R)))
with (R/'Checksums_SHA256.txt').open('w') as f:
 for p in assetfiles:f.write(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p.relative_to(R))+'\n')
def zipfiles(path,parts):
 with zipfile.ZipFile(path,'w',zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
  for p in parts:z.write(p,arcname=str(Path(R.name)/p.relative_to(R)))
 with zipfile.ZipFile(path) as z:
  bad=z.testzip()
  if bad:raise ValueError('ZIP TEST FAILED:'+bad)
  return len(z.infolist())
full=OUT/'ProjectSpark_Level94_BoardFoundry_AAA_Complete.zip'
modelonly=OUT/'ProjectSpark_Level94_BoardFoundry_FBX_Only.zip'
count=zipfiles(full,assetfiles+[R/'Checksums_SHA256.txt'])
# FBX-only still includes the most useful design maps, import notes, and source model placement manifest.
fbxparts=sorted((R/'Models').glob('*.fbx'))
for fn in ('Model_Manifest_48.csv','Component_BOM_27.csv','Copper_Trace_Tasks_W001_W034.csv','Required_Via_Placements_V01_V06.csv','Critical_W035_Feedback.csv','Animation_Pivots.csv','Logical_Pad_Locations.csv'):
 fbxparts.append(R/'Data'/fn)
fbxparts += [R/'README_FIRST.md',R/'Reports'/'Offline_Structural_Validation.json']
fbxn=zipfiles(modelonly,fbxparts)
report={'complete_archive':str(full),'complete_size_bytes':full.stat().st_size,'complete_files':count,'fbx_only_archive':str(modelonly),'fbx_only_size_bytes':modelonly.stat().st_size,'fbx_only_files':fbxn,'archive_tests':'BOTH ZIP FILES VALID','real_fbx_count':len(list((R/'Models').glob('[0-9][0-9]_L94_*.fbx'))),'includes_showcase':True}
(OUT/'Level94_Package_Verification.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
