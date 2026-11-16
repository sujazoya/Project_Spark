from pathlib import Path
import hashlib,zipfile,json,time
R=Path(__file__).resolve().parents[1];OUT=R.parent
all_files=[p for p in R.rglob('*') if p.is_file() and '__pycache__' not in str(p) and p.suffix!='.pyc' and p.name!='Checksums_SHA256.txt']
checks=[]
for f in sorted(all_files):
 dig=hashlib.sha256(f.read_bytes()).hexdigest()
 checks.append(dig+'  '+f.relative_to(R).as_posix())
(R/'Checksums_SHA256.txt').write_text('\n'.join(checks)+'\n')
all_files.append(R/'Checksums_SHA256.txt')
complete=OUT/'ProjectSpark_Level93_HD_AAA_Models.zip'
fbx_only=OUT/'ProjectSpark_Level93_FBX_Only.zip'
with zipfile.ZipFile(complete,'w',zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
 for i,f in enumerate(sorted(all_files)):
  z.write(f,arcname=R.name+'/'+f.relative_to(R).as_posix())
  if i%65==0:print('PACKAGING',i+1,'/',len(all_files),flush=True)
with zipfile.ZipFile(fbx_only,'w',zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
 for f in sorted((R/'Models').glob('*.fbx')):z.write(f,arcname='ProjectSpark_Level93_FBX_Only/'+f.name)
 for fname in ['README_FIRST.md','Art_Assembly.csv','Animation_Rigs.csv','Component_Roles.csv','Model_Manifest.json']:
  z.write(R/fname,arcname='ProjectSpark_Level93_FBX_Only/'+fname)
for f in (complete,fbx_only):
 with zipfile.ZipFile(f,'r') as z:
  bad=z.testzip()
  assert bad is None, (f,bad)
  print(f.name,'OK','files',len(z.namelist()),'MB',round(f.stat().st_size/1048576,2),flush=True)
report={'archives':{p.name:{'bytes':p.stat().st_size,'files':len(zipfile.ZipFile(p).namelist()),'zip_crc_integrity_passed':True} for p in [complete,fbx_only]},'complete_kit_files':len(all_files)}
(R/'Reports'/'Package_Integrity_Report.json').write_text(json.dumps(report,indent=2))
print('PACKAGING COMPLETE')
