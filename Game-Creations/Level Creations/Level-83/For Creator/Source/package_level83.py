"""Package verified Level 83 original FBX/GLB assets and integration guides."""
from pathlib import Path
import hashlib, zipfile,json
R=Path('/mnt/data/ProjectSpark_Level83_HD_AAA');D=R/'Source';D.mkdir(exist_ok=True)
for n in ['build_level83.py','build_level83_textures.py','make_level83_support.py','verify_level83.py','package_level83.py']:
    src=Path('/mnt/data/level83_work')/n
    if src.exists(): (D/n).write_bytes(src.read_bytes())
files=sorted(p for p in R.rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.name!='Checksums_SHA256.txt')
(R/'Checksums_SHA256.txt').write_text(''.join(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p.relative_to(R)).replace('\\','/')+'\n' for p in files))
archive=Path('/mnt/data/ProjectSpark_Level83_HD_AAA_Models.zip')
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=5,allowZip64=True) as z:
    for p in sorted(R.rglob('*')):
        if not p.is_file() or '__pycache__' in p.parts:continue
        z.write(p,p.relative_to(R.parent))
with zipfile.ZipFile(archive) as z:
    assert z.testzip() is None
    paths=z.namelist()
    assert len(paths)==len([p for p in R.rglob('*') if p.is_file() and '__pycache__' not in p.parts])
    assert sum(x.endswith('.fbx') for x in paths)==75
    assert sum(x.endswith('.glb') for x in paths)==75
    assert sum(x.endswith('.png') for x in paths)==36
    assert sum(x.endswith('.cs') for x in paths)==4
    assert 'ProjectSpark_Level83_HD_AAA/Model_Manifest.json' in paths
    assert 'ProjectSpark_Level83_HD_AAA/W104_Critical_Feedback_Recovery.md' in paths
    print('ARCHIVE VERIFIED',archive,'bytes',archive.stat().st_size,'files',len(paths),'fbx',75,'glb',75,'texture maps',36,'unity scripts',4)
print('PACKAGE DONE')
