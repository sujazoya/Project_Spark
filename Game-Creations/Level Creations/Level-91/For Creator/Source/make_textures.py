#!/usr/bin/env python3
"""Create 12 procedural reference PBR sets, not real artist UV bake atlases."""
from pathlib import Path
from PIL import Image
import numpy as np,json
ROOT=Path('/mnt/data/ProjectSpark_Level91_HD_AAA');S=1024
SETS={
 'XRayGoldMirrorShell':((212,173,79),.91,.19,'brushed'),
 'CosmicObservatoryTitanium':((181,197,212),.73,.36,'brushed'),
 'AtomicClockCeramicVacuum':((223,230,234),.14,.22,'ceramic'),
 'ClockReferenceCopperShield':((182,106,61),.87,.27,'wire'),
 'PhotonDetectorGraphite':((46,65,82),.23,.47,'carbon'),
 'GalacticVioletHologram':((147,81,244),.03,.18,'holo'),
 'CyanPulsarTimingDisplay':((29,156,232),.04,.21,'holo'),
 'DeepSpaceSolarCell':((35,62,137),.21,.24,'solar'),
 'ElectronicsBackplane':((31,92,84),.32,.36,'panel'),
 'GoldElectricalSocket':((242,188,77),.95,.20,'brushed'),
 'DeepSpaceServiceFloor':((72,92,114),.55,.51,'floor'),
 'CryogenicOpticalGlass':((82,150,193),.02,.10,'glass')}
X=np.arange(S,dtype=np.float32)[None,:];Y=np.arange(S,dtype=np.float32)[:,None]
manifest=[]
for k,(name,(color,metal,rough,kind)) in enumerate(SETS.items()):
 rng=np.random.default_rng(91091+k);fine=rng.normal(size=(S,S)).astype(np.float32)
 csmall=rng.random((54,54));coarse=np.asarray(Image.fromarray((csmall*255).astype('uint8')).resize((S,S),Image.Resampling.BICUBIC),dtype=np.float32)/255-.5
 if kind=='brushed':v=.60*np.sin(X*.18)+.24*np.sin(X*.90)+.18*fine+.25*coarse
 elif kind=='wire':v=.55*np.cos(X*.14)+.38*np.sin(Y*.43)+.14*fine+.32*coarse
 elif kind=='carbon':v=.50*np.sin((X+Y)*.12)*np.sin((X-Y)*.10)+.2*fine+.24*coarse
 elif kind=='solar':v=.31*np.cos(X*.10)+.31*np.cos(Y*.09)+.12*fine+.21*coarse
 elif kind=='holo':v=.32*np.sin(Y*.13)+.19*np.cos(X*.07)+.14*fine+.19*coarse
 elif kind=='floor':v=.28*np.sin(X*.03)*np.cos(Y*.025)+.24*fine+.29*coarse
 elif kind=='glass':v=.08*fine+.09*coarse+.12*np.sin(X*.02)
 else:v=.23*fine+.23*coarse
 if kind in ('panel','solar','floor'):
  v=v-.40*(((X%128)<2).astype(np.float32)+((Y%128)<2).astype(np.float32))
 albedo=np.clip(np.asarray(color)[None,None,:]*(1+v[:,:,None]*.08),0,255).astype('uint8')
 gradX=np.gradient(v,axis=1);gradY=np.gradient(v,axis=0);n=np.dstack((-gradX*.12,-gradY*.12,np.ones_like(v)))
 n=n/np.maximum(np.linalg.norm(n,axis=2,keepdims=True),1e-8)
 normals=np.clip((n*.5+.5)*255,0,255).astype('uint8')
 mr=np.dstack((np.full((S,S),int(metal*255),np.uint8),
  np.clip((rough+.026*fine+.025*coarse)*255,0,255).astype('uint8'),
  np.clip((.95-.02*np.abs(fine)-.02*np.abs(coarse))*255,0,255).astype('uint8')))
 files={}
 for typ,arr in [('Albedo',albedo),('Normal',normals),('MetalRoughAO',mr)]:
  path=ROOT/'Textures'/f'{name}_{typ}.png';Image.fromarray(arr,'RGB').save(path,compress_level=3)
  files[typ]='Textures/'+path.name
 manifest.append(dict(name=name,resolution=[S,S],metallic=metal,roughness=rough,procedural_pattern=kind,files=files,
  packed_channels={'R':'Metallic','G':'Roughness','B':'AmbientOcclusion'},
  limitations='Art reference only: meshes do not contain production-authored UV/baked texture atlases'))
 print('TEXTURE',k+1,'of',len(SETS),name,flush=True)
(ROOT/'Texture_Manifest.json').write_text(json.dumps(dict(material_sets=12,map_count=36,uv_baking_completed=False,materials=manifest),indent=2))
