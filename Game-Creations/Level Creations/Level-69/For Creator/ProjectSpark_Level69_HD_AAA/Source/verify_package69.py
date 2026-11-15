from pathlib import Path
import struct,json,zipfile
import trimesh
root=Path('/mnt/data/ProjectSpark_Level69_HD_AAA');models=root/'Models';glbs=root/'GLB'
manifest=json.loads((root/'Model_Manifest.json').read_text())
def fbxcheck(p):
 data=p.read_bytes();assert data[:23]==b'Kaydara FBX Binary  \x00\x1a\x00',(p.name,data[:23]);assert struct.unpack_from('<I',data,23)[0]==7400
 names=[];cur=27
 while cur+13<len(data):
  end,np,ln,nl=struct.unpack_from('<IIIB',data,cur)
  if not end:break
  assert end>cur and end<=len(data),(p.name,cur,end,len(data))
  names.append(data[cur+13:cur+13+nl].decode('ascii'))
  cur=end
 assert all(x in names for x in ('Objects','Connections','FBXHeaderExtension','GlobalSettings')),names
 return names
report={'level':'69','individual_model_count':46,'individual_triangle_count':manifest['individual_triangle_count'],'pivot_mappings':manifest['pivots_count'],'binary_fbx_verified':[],'glb_verified':[],'unity_import_tested':False,'playmode_tested':False,'actual_gameplay_included':False}
for p in sorted(models.glob('*.fbx')):fbxcheck(p);report['binary_fbx_verified'].append(p.name)
for p in sorted(glbs.glob('*.glb')):
 scene=trimesh.load(p,force='scene');cnt=sum(len(m.faces) for m in scene.geometry.values());assert cnt>0,(p,cnt)
 report['glb_verified'].append({'file':p.name,'triangles':cnt})
assert len(report['binary_fbx_verified'])==47 and len(report['glb_verified'])==47
for m in manifest['models']:
 assert (models/m['file']).exists() and (glbs/m['glb']).exists()
 report_tri=next(e['triangles'] for e in report['glb_verified'] if e['file']==m['glb'])
 assert m['triangle_count']==report_tri,(m['file'],m['triangle_count'],report_tri)
(root/'Verification_Report.json').write_text(json.dumps(report,indent=2))
zipname=Path('/mnt/data/ProjectSpark_Level69_HD_AAA_Models.zip')
with zipfile.ZipFile(zipname,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=7) as z:
 for p in sorted(root.rglob('*')):
  if p.is_file():z.write(p,p.relative_to(root.parent))
with zipfile.ZipFile(zipname) as z:assert z.testzip() is None;print('archive entries',len(z.namelist()))
print('verified',len(report['binary_fbx_verified']),'FBX',len(report['glb_verified']),'GLB','size',zipname.stat().st_size,'triangle total',manifest['individual_triangle_count'])
