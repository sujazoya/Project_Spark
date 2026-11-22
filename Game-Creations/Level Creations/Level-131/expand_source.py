from pathlib import Path
import numpy as np,csv,json,struct
from PIL import Image,ImageDraw
import trimesh as t
R=Path('/mnt/data/L131_complete'); A=R/'Assets/ProjectSpark/Level131/B760MSteelLegend'; N=A/'Models_Native'; N.mkdir(exist_ok=True); T=A/'Textures'; T.mkdir(exist_ok=True)
colors=json.loads((A/'materials.json').read_text()); rows=list(csv.DictReader((A/'placements.csv').open())); rng=np.random.default_rng(131)
def read_obj(p):
 v=[]; groups=[]; faces=[]; name=mat=None
 for line in p.read_text().splitlines():
  a=line.split(); k=a[0] if a else ''
  if k=='v':v.append([float(x) for x in a[1:]])
  elif k=='o':
   if faces:groups.append((name,mat,np.array(faces)));faces=[]
   name=a[1]
  elif k=='usemtl':mat=a[1]
  elif k=='f':faces.append([int(x)-1 for x in a[1:]])
 if faces:groups.append((name,mat,np.array(faces)))
 v=np.array(v);return [(name,mat,v[f].reshape(-1,3)) for name,mat,f in groups]
def uv_for(v,mat):
 tri=v.reshape(-1,3,3); axis=np.abs(np.cross(tri[:,1]-tri[:,0],tri[:,2]-tri[:,0])).argmax(1); uv=np.empty((len(tri),3,2))
 for i,axes in enumerate([(2,1),(0,2),(0,1)]):
  mask=axis==i;uv[mask]=tri[mask][:,:,axes]
 return (uv/.244+.5 if mat=='PCB' else uv/.025).reshape(-1,2)
def write_native(path,groups):
 with path.open('wb') as f:
  f.write(b'SPK131M1');f.write(struct.pack('<i',len(groups)))
  for name,mat,v in groups:
   for s in (name,mat):b=s.encode();f.write(struct.pack('<H',len(b)));f.write(b)
   assert len(v)>0 and len(v)%3==0 and np.isfinite(v).all();f.write(struct.pack('<i',len(v)));f.write(np.asarray(v,dtype='<f4').tobytes());f.write(np.asarray(uv_for(v,mat),dtype='<f4').tobytes())
allg=[];count=0
for r in rows:
 id=r['id'];groups=read_obj(A/'FallbackOBJ'/f'L131_{id}.obj');write_native(N/f'L131_{id}.spark131mesh',groups);off=np.array([(float(r['x_mm'])-122)*.001,0,(float(r['z_mm'])-122)*.001]);allg.extend((name,mat,v+off) for name,mat,v in groups);count+=sum(len(v)//3 for _,_,v in groups)
write_native(N/'L131_ASSEMBLED.spark131mesh',allg)
batch=[('Batch_'+mat,mat,np.concatenate([v for _,m,v in allg if m==mat])) for mat in colors if any(m==mat for _,m,v in allg)]
write_native(N/'L131_BATCHED.spark131mesh',batch)
src=(R/'generate_source.py').read_text();env={'np':np,'struct':struct,'zlib':__import__('zlib'),'colors':colors};exec(src[src.index('def S(x):'):src.index('allentries=[]; report=[]')],env)
entries=[];scene=t.Scene()
for name,mat,v in batch:
 mesh=t.Trimesh(vertices=v*1000,faces=np.arange(len(v)).reshape(-1,3),process=False);entries.append((name,mat,mesh,(0,0,0)))
for i,(name,mat,v) in enumerate(allg):
 mesh=t.Trimesh(vertices=v,faces=np.arange(len(v)).reshape(-1,3),process=False);mesh.visual=t.visual.TextureVisuals(uv=uv_for(v,mat),material=t.visual.material.PBRMaterial(name=mat,baseColorFactor=[*colors[mat],1],metallicFactor=.85 if mat in ['Silver','Steel','Gold','Copper','Solder'] else 0,roughnessFactor=.35 if mat in ['Silver','Steel','Gold','Copper','Solder'] else .7));scene.add_geometry(mesh,node_name=f'{name}_{i}',geom_name=f'{name}_{i}')
env['export'](A/'Models/L131_BATCHED.fbx',entries);(A/'L131_ASSEMBLED.glb').write_bytes(scene.export(file_type='glb'))
for mat,rgb in colors.items():
 metal=mat in ['Silver','Steel','Gold','Copper','Solder'];size=2048;noise=rng.normal(0,1.6,(size,size)).astype(np.float32);base=np.clip(np.array(rgb)*255+noise[:,:,None],0,255).astype('uint8');im=Image.fromarray(base,'RGB')
 if mat=='PCB':
  d=ImageDraw.Draw(im);d.polygon([(0,0),(650,0),(2048,1398),(2048,1650)],fill=(83,86,88));d.polygon([(0,1400),(550,850),(800,850),(0,1650)],fill=(92,94,95));d.polygon([(1200,0),(2048,0),(2048,550)],fill=(62,65,67))
  for j in range(220):
   x,y=rng.integers(20,1900,2);l=int(rng.integers(15,100));d.line([(int(x),int(y)),(int(x+l),int(y)),(int(x+l+12),int(y+12))],fill=(63,68,70),width=2)
  d.text((1180,1050),'B760M STEEL LEGEND',fill=(190,192,193))
 im.save(T/f'{mat}_BaseColor.png',compress_level=3);Image.new('RGB',(size,size),(128,128,255)).save(T/f'{mat}_Normal.png');Image.new('RGBA',(size,size),(220 if metal else 0,0,0,165 if metal else 75)).save(T/f'{mat}_MetallicSmoothness.png')
print('native',len(rows)+2,'triangles',count,'batch',len(batch))
