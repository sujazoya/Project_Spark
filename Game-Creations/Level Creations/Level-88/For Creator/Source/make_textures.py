#!/usr/bin/env python3
"""Creates 12 original procedural PBR reference material sets (36 PNGs), 1024².
These maps are for Unity material-upgrade/baking workflow; the mesh exports do not
provide fully authored production UV atlases. Packed R metallic / G roughness / B AO.
"""
import os, json, numpy as np
from PIL import Image
ROOT=str(__import__('pathlib').Path(__file__).resolve().parent.parent)
S=1024
SETS={
'PressurizedHabitatCeramicWhite':((226,235,234),.21,.30,'ceramic'),
'CryogenicPipeBrushedStainless':((152,174,189),.88,.24,'brushed'),
'ElectrolyzerCopperBusbars':((206,117,51),.90,.29,'wire'),
'ECLSSGoldInstrumentContacts':((223,161,49),.82,.25,'brushed'),
'ActivatedCO2SorbentBlue':((41,168,207),.03,.72,'carbon'),
'HabitatBlueGlass':((30,118,177),.02,.14,'glass'),
'HabitatEnvironmentalDarkPanels':((27,45,71),.17,.51,'panel'),
'AtmosphereCyanHolographic':((36,174,229),.03,.15,'holo'),
'CondenserColdSteelFins':((128,151,169),.84,.38,'brushed'),
'HabitatServiceFloor':((71,82,94),.55,.56,'floor'),
'CO2SafetyAmberStatus':((220,131,42),.23,.41,'panel'),
'LifeSupportEmergencyRed':((194,47,40),.34,.45,'panel'),
}

manifest=[]
x=np.arange(S,dtype=np.float32)[None,:];y=np.arange(S,dtype=np.float32)[:,None]
for ii,(name,(base,metal,rough,kind)) in enumerate(SETS.items()):
 rng=np.random.default_rng(88_000+ii)
 fine=rng.normal(0,1,(S,S)).astype(np.float32)
 coarse=rng.random((48,48)).astype(np.float32)
 coarse=np.array(Image.fromarray((coarse*255).astype('uint8')).resize((S,S),Image.Resampling.BICUBIC)).astype(np.float32)/255-.5
 if kind=='brushed': tex=.65*np.sin(x*.16)+.30*np.sin(x*.83)+.25*fine+.35*coarse
 elif kind=='wire':tex=.6*np.sin(x*.09)+.45*np.sin(y*.49)+.25*fine+.25*coarse
 elif kind=='carbon':tex=.5*np.sin((x+y)*.06)*np.sin((x-y)*.08)+.17*fine+.6*coarse
 elif kind=='solar':tex=.35*np.cos(x*.09)+.35*np.cos(y*.095)+.13*fine+.3*coarse
 elif kind=='glass':tex=.12*fine+.12*coarse+.3*np.sin(x*.02)
 elif kind=='floor':tex=.34*np.sin(x*.018)*np.cos(y*.018)+.25*fine+.30*coarse
 elif kind=='holo':tex=.3*np.sin(y*.11)+.16*np.sin(x*.07)+.12*fine+.25*coarse
 else:tex=.18*fine+.30*coarse+.16*np.cos(y*.052)
 # add microscopic etched panel / solar cell grid, mask smooth
 if kind in ('solar','panel','floor'):
  gx=((x%128)<2).astype(np.float32);gy=((y%128)<2).astype(np.float32);tex=tex-(gx+gy)*.32
 color=np.clip(np.asarray(base,dtype=np.float32)[None,None,:]*(1+tex[:,:,None]*.105),0,255).astype('uint8')
 # engineering reference normal map from height gradients
 height=.34*tex;dx=np.gradient(height,axis=1);dy=np.gradient(height,axis=0)
 normal=np.dstack((-dx*.48,-dy*.48,np.ones_like(height)))
 normal/=np.maximum(np.linalg.norm(normal,axis=2,keepdims=True),1e-8)
 normal_img=np.clip((normal*.5+.5)*255,0,255).astype('uint8')
 rough_map=np.clip((rough+.028*fine+.045*coarse)*255,0,255).astype('uint8')
 met_map=np.full((S,S),int(metal*255),np.uint8)
 ao=np.clip((.94-.03*np.abs(coarse)-.02*np.abs(fine))*255,0,255).astype('uint8')
 mr=np.dstack((met_map,rough_map,ao)).astype('uint8')
 paths={}
 for tag,arr in [('Albedo',color),('Normal',normal_img),('MetalRoughAO',mr)]:
  file=name+'_'+tag+'.png';Image.fromarray(arr,'RGB').save(os.path.join(ROOT,'Textures',file),compress_level=3);paths[tag]='Textures/'+file
 manifest.append(dict(name=name,resolution=[S,S],metallic=metal,roughness=rough,procedural_pattern=kind,files=paths,mrao_channels={'R':'Metallic','G':'Roughness','B':'AmbientOcclusion'},use='OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING'))
 print('TEXTURE',ii+1,'/12',name,flush=True)
with open(os.path.join(ROOT,'Texture_Manifest.json'),'w') as f:json.dump(dict(material_sets=12,texture_maps=36,materials=manifest,uv_authoring_required=True),f,indent=2)
