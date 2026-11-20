#!/usr/bin/env python3
"""Original procedural PBR source library for Level 94. 1K maps with Unity mask packing."""
from pathlib import Path
import json,csv
import numpy as np
from PIL import Image
import build_level94 as b
R=Path(__file__).resolve().parents[1];T=R/'Textures';M=R/'Materials';M.mkdir(exist_ok=True)
S=1024
SETS={
'TitaniumBench':((132,145,159),.84,.30,'brushed'),
'AnodizedGraphite':((34,45,55),.65,.43,'brushed'),
'ESDPolymer':((23,29,33),.02,.76,'rubber'),
'FiberglassFR4':((92,106,59),.03,.73,'fiberglass'),
'DeepGreenSoldermask':((13,105,56),.03,.30,'pcbmask'),
'RolledCopper':((212,119,48),.91,.34,'brushed'),
'ENIGGold':((229,183,69),.97,.20,'brushed'),
'LeadFreeSolder':((190,198,202),.87,.27,'brushed'),
'CeramicWhite':((225,226,207),.10,.58,'ceramic'),
'SilkscreenWhite':((229,240,225),.0,.78,'ink'),
'ResistorCarbon':((67,43,32),.05,.61,'carbon'),
'MLCCDielectric':((159,142,86),.13,.42,'ceramic'),
'ICBlackMold':((18,21,26),.06,.47,'carbon'),
'GlassLens':((55,158,192),.04,.14,'glass'),
'SCADADisplay':((9,32,64),.02,.28,'holo'),
'NeonCyan':((21,181,248),.0,.20,'holo'),
'StatusGreen':((31,228,90),.0,.20,'holo'),
'StatusAmber':((244,132,28),.0,.27,'holo'),
'StatusRed':((236,53,43),.0,.24,'holo'),
'StatusBlue':((23,123,240),.0,.21,'holo')}
MAP={'frame':'TitaniumBench','steel':'TitaniumBench','aluminum':'TitaniumBench','graphite':'AnodizedGraphite','rubber':'ESDPolymer',
'fr4':'FiberglassFR4','mask':'DeepGreenSoldermask','maskdark':'DeepGreenSoldermask','copper':'RolledCopper','gold':'ENIGGold','solder':'LeadFreeSolder',
'ceramic':'CeramicWhite','whiteink':'SilkscreenWhite','resistor':'ResistorCarbon','capacitor':'MLCCDielectric','ic':'ICBlackMold','black':'ICBlackMold',
'glass':'GlassLens','panel':'SCADADisplay','cyan':'NeonCyan','holo':'NeonCyan','green':'StatusGreen','amber':'StatusAmber','orange':'StatusAmber','red':'StatusRed','blue':'StatusBlue'}
assert set(MAP)==set(b.COLORS)
X=np.arange(S,dtype=np.float32)[None,:];Y=np.arange(S,dtype=np.float32)[:,None]
rows=[]
for j,(name,(color,metal,rough,kind)) in enumerate(SETS.items()):
 rng=np.random.default_rng(94094+j)
 noise=rng.normal(0,1,(S,S)).astype('float32')
 cnoise=np.asarray(Image.fromarray(rng.integers(0,255,(96,96),dtype=np.uint8)).resize((S,S),Image.Resampling.BICUBIC),dtype=np.float32)/255-.5
 if kind=='brushed':h=.40*np.sin(X*.215)+.10*np.sin(X*.98)+.12*noise+.30*cnoise
 elif kind=='fiberglass':h=.24*np.sin(X*.12)*np.cos(Y*.16)+.28*np.cos(X*.4+Y*.36)+.24*noise+.18*cnoise
 elif kind=='pcbmask':h=.13*np.sin(X*.14)+.12*np.cos(Y*.13)+.09*noise+.19*cnoise
 elif kind=='carbon':h=.24*noise+.28*cnoise
 elif kind=='rubber':h=.33*noise+.31*cnoise+.13*np.sin(X*.26)*np.cos(Y*.27)
 elif kind=='holo':h=.08*noise+.06*cnoise+.20*np.cos(Y*.13)
 elif kind=='glass':h=.035*noise+.04*cnoise
 elif kind=='ink':h=.08*noise+.07*cnoise
 else:h=.14*noise+.20*cnoise
 albedo=np.clip(np.asarray(color,dtype=np.float32)[None,None,:]*(1+.065*h[:,:,None]),0,255).astype('uint8')
 gx=np.gradient(h,axis=1);gy=np.gradient(h,axis=0)
 v=np.dstack((-gx*.24,-gy*.24,np.ones_like(h)));v/=np.maximum(np.linalg.norm(v,axis=2,keepdims=True),1e-7)
 normal=np.clip((v*.5+.5)*255,0,255).astype('uint8')
 r=np.clip(rough+.017*noise+.015*cnoise,0,1);ao=np.clip(.91-.025*np.abs(noise)-.022*np.abs(cnoise),0,1)
 m=np.full((S,S),int(metal*255),dtype='uint8');a=(ao*255).astype('uint8');sm=(np.clip(1-r,0,1)*255).astype('uint8')
 mr=np.dstack((m,(r*255).astype('uint8'),a));mask=np.dstack((m,a,np.zeros_like(m),sm));occ=np.dstack((a,a,a))
 emit=1.0 if kind=='holo' else .0
 emi=np.clip(albedo.astype('float32')*(emit*.80),0,255).astype('uint8')
 for suffix,array,mode in [('BaseColor',albedo,'RGB'),('Normal',normal,'RGB'),('MetalRoughAO',mr,'RGB'),('UnityMaskMap',mask,'RGBA'),('Occlusion',occ,'RGB'),('Emission',emi,'RGB')]:
  Image.fromarray(array,mode).save(T/f'{name}_{suffix}.png',compress_level=5)
 rows.append(dict(MaterialSet=name,AlbedoRGB='#%02X%02X%02X'%tuple(color),Metallic=metal,Roughness=rough,Pattern=kind,TextureResolution=S,Textures=6,UsedBy=';'.join(k for k,v in MAP.items() if v==name)))
 print(f'TEXTURE {j+1:02d}/{len(SETS)} {name}',flush=True)
with (M/'Material_Reference_Map.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['SourceFBXMaterial','PBRMaterialSet','PhysicalMeaning']);
 for k,v in sorted(MAP.items()):w.writerow(['PS_'+k,v,'Original PBR material reference'])
with (M/'PBR_Texture_Manifest.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
(R/'Materials'/'PBR_Material_Bindings.json').write_text(json.dumps({'source_material_map':MAP,'sets':rows,'unity_shader_compatibility':['Built-in Standard','URP Lit','HDRP Lit'], 'unity_masks':'Red=Metallic Green=AO Alpha=Smoothness','normal_maps':'Tangent style procedural reference; final production baking recommended','textures_are_embedded_in_glbs':False},indent=2))
print('DONE',len(SETS)*6,'PBR texture files')
