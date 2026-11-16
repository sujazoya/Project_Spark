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
'RoboticArmCeramicCoating':((217,226,226),.13,.30,'ceramic'),
'MachinedTitaniumLink':((151,165,181),.88,.25,'brushed'),
'ArmActuatorGoldAnodized':((208,143,40),.83,.22,'brushed'),
'StationDarkCarbonComposite':((31,39,51),.05,.65,'carbon'),
'RadiatorOpticalBlackFins':((23,35,48),.19,.46,'panel'),
'RadiatorThermalCopperHeatPipe':((185,106,48),.92,.27,'wire'),
'CoolantPortSteelCeramic':((132,146,159),.79,.31,'brushed'),
'RobotOpticalSensorGlass':((32,103,145),.02,.17,'glass'),
'SpaceStationServiceFloor':((69,82,96),.65,.49,'floor'),
'AvionicsInstrumentPanel':((24,52,80),.29,.46,'panel'),
'RoboticSafetyRedWarning':((189,44,32),.42,.38,'panel'),
'OrbitalThermalHologram':((36,148,213),.06,.18,'holo'),
}
manifest=[]
x=np.arange(S,dtype=np.float32)[None,:];y=np.arange(S,dtype=np.float32)[:,None]
for ii,(name,(base,metal,rough,kind)) in enumerate(SETS.items()):
 rng=np.random.default_rng(86_000+ii)
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
