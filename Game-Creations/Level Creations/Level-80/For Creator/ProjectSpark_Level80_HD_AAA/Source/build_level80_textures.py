#!/usr/bin/env python3
"""Optional original seamless procedural material texture maps; no concept-art image generation."""
import numpy as np,json
from pathlib import Path
from PIL import Image
R=Path('/mnt/data/ProjectSpark_Level80_HD_AAA');D=R/'Textures';D.mkdir(exist_ok=True)
n=1024
rng=np.random.default_rng(80)
y,x=np.mgrid[:n,:n];u=x/n;v=y/n
materials={
'BrushedSteel':((114,140,157),.78,.26,'brushed'),
'CopperBus':((202,119,60),.94,.28,'brushed'),
'GoldContact':((222,179,79),.97,.20,'speckle'),
'AnodizedPanel':((29,51,72),.69,.40,'brushed'),
'BlackCeramic':((33,42,49),.06,.33,'ceramic'),
'SiliconInterposer':((55,77,105),.44,.22,'microgrid'),
'Elastomer':((47,51,60),.04,.68,'speckle'),
'OpticalGlass':((102,184,221),.02,.07,'glass'),
'CleanroomFloor':((119,145,161),.47,.29,'floor')}
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
