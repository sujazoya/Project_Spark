#!/usr/bin/env python3
"""Level 82 original seamless procedural material texture maps; no concept-art image generation."""
import numpy as np,json
from pathlib import Path
from PIL import Image
R=Path('/mnt/data/ProjectSpark_Level82_HD_AAA');D=R/'Textures';D.mkdir(exist_ok=True)
n=1024
rng=np.random.default_rng(82)
y,x=np.mgrid[:n,:n];u=x/n;v=y/n
materials={
'BrushedSteel':((117,141,160),.76,.26,'brushed'),
'RFShieldAlloy':((128,142,159),.83,.31,'brushed'),
'GroundCopper':((211,137,77),.93,.24,'brushed'),
'GoldContact':((229,184,81),.96,.20,'speckle'),
'AbsorberFoam':((29,36,48),.03,.94,'ceramic'),
'FerriteComposite':((48,53,63),.12,.82,'speckle'),
'AntennaTitanium':((186,201,215),.87,.24,'brushed'),
'SpacecraftPCB':((27,83,79),.31,.35,'microgrid'),
'EMCBlueDisplay':((50,131,176),.04,.16,'glass'),
'AnechoicFloor':((104,126,140),.41,.35,'floor'),
'FilterHousing':((52,70,88),.61,.41,'brushed'),
'HazardYellow':((245,175,68),.37,.35,'speckle')}

manifest=[]
for name,(rgb,metal,rough,kind) in materials.items():
    noise=rng.normal(0,1,(n,n)).astype('float32'); noise=(noise+np.roll(noise,1,0)+np.roll(noise,-1,0)+np.roll(noise,1,1)+np.roll(noise,-1,1))/5
    if kind=='brushed': height=.35*np.sin(2*np.pi*(x*37/n))+.13*np.cos(2*np.pi*x*121/n)+.22*noise
    elif kind=='microgrid':height=.13*np.sin(2*np.pi*64*u)+.13*np.sin(2*np.pi*64*v)+.13*noise
    elif kind=='floor':height=.13*(np.sin(2*np.pi*10*u)**30+np.sin(2*np.pi*10*v)**30)+.12*noise
    elif kind=='ceramic':height=.13*np.sin(2*np.pi*7*u)*np.cos(2*np.pi*7*v)+.17*noise
    elif kind=='glass':height=.03*np.sin(2*np.pi*8*u)*np.cos(2*np.pi*7*v)+.06*noise
    else:height=.22*noise
    height=height.astype('float32')
    luminance=np.clip(1+.10*height,.83,1.19)
    col=np.stack([np.clip(rgb[i]*luminance,0,255).astype('uint8') for i in range(3)],axis=-1)
    Image.fromarray(col,'RGB').save(D/f'{name}_Albedo.png',compress_level=1)
    # Standard tangent-space normal map, tileable sine components plus low-amplitude noise variation.
    dy,dx=np.gradient(height)
    nx=-dx*.28;ny=-dy*.28;nz=np.ones_like(nx)
    l=np.sqrt(nx*nx+ny*ny+nz*nz)
    nor=np.stack([((nx/l)*.5+.5)*255,((ny/l)*.5+.5)*255,((nz/l)*.5+.5)*255],axis=-1).astype('uint8')
    Image.fromarray(nor,'RGB').save(D/f'{name}_Normal.png',compress_level=1)
    # RGB metallic-roughness: R metallic, G roughness, B ambient-occlusion proxy.
    m=np.full((n,n),int(metal*255),dtype='uint8')
    r=np.clip((rough+.024*height)*255,0,255).astype('uint8')
    ao=np.clip((.97-.014*height)*255,0,255).astype('uint8')
    Image.fromarray(np.stack((m,r,ao),-1),'RGB').save(D/f'{name}_MetalRoughAO.png',compress_level=1)
    manifest.append({'name':name,'maps':[f'{name}_Albedo.png',f'{name}_Normal.png',f'{name}_MetalRoughAO.png'],'size_px':1024,'quality':'procedural tileable preview PBR maps, not baked final artist textures','packed_metalrough_channels':'R=metallic G=roughness B=ambient occlusion'})
(R/'Texture_Manifest.json').write_text(json.dumps(manifest,indent=2))
print('Original preview maps',len(manifest),'materials',len(manifest)*3,'PNG files')
