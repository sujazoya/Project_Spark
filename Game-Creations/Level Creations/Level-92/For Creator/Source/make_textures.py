#!/usr/bin/env python3
"""Create 12 procedural reference PBR sets, not real artist UV bake atlases."""
from pathlib import Path
from PIL import Image
import numpy as np,json
ROOT=Path('/mnt/data/ProjectSpark_Level92_HD_AAA');S=1024
SETS={
 'CarbonaceousAsteroidRegolith':((53,57,62),.07,.83,'carbon'),
 'Khepri7MineralStrata':((125,104,84),.08,.76,'floor'),
 'LanderWhiteThermalCeramic':((221,230,231),.30,.32,'ceramic'),
 'AsteroidLanderTitanium':((154,176,195),.78,.30,'brushed'),
 'MicrospineHardenedSteel':((96,111,118),.90,.38,'brushed'),
 'CoreBitCarbideAndGold':((171,163,137),.73,.30,'brushed'),
 'ElectricalGoldTerminal':((240,191,70),.94,.20,'brushed'),
 'GeophysicalCyanHologram':((27,183,236),.03,.24,'holo'),
 'DrillSafetyAmberIndicators':((232,133,33),.10,.29,'holo'),
 'SampleContainmentInspectionGlass':((100,175,200),.04,.10,'glass'),
 'PrecisionAvionicsGraphite':((39,56,73),.34,.48,'carbon'),
 'IndustrialSamplingServiceDeck':((83,102,115),.55,.54,'floor')}

X=np.arange(S,dtype=np.float32)[None,:];Y=np.arange(S,dtype=np.float32)[:,None]
manifest=[]
for k,(name,(color,metal,rough,kind)) in enumerate(SETS.items()):
 rng=np.random.default_rng(92092+k);fine=rng.normal(size=(S,S)).astype(np.float32)
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
