#!/usr/bin/env python3
"""Level 83 original seamless procedural material texture maps; no concept-art image generation."""
import numpy as np,json
from pathlib import Path
from PIL import Image
R=Path('/mnt/data/ProjectSpark_Level83_HD_AAA');D=R/'Textures';D.mkdir(exist_ok=True)
n=1024
rng=np.random.default_rng(83)
y,x=np.mgrid[:n,:n];u=x/n;v=y/n
materials={
'SolarCells':((26,62,151),.33,.21,'microgrid'),
'GoldContact':((235,195,91),.96,.22,'speckle'),
'CopperBus':((212,132,67),.92,.28,'brushed'),
'BatteryCells':((37,117,183),.42,.39,'brushed'),
'BrushedTitanium':((155,171,190),.82,.30,'brushed'),
'OpticalGlass':((83,185,235),.07,.14,'glass'),
'SpacecraftAlloy':((40,63,96),.68,.39,'brushed'),
'BlackCeramic':((31,41,52),.14,.81,'ceramic'),
'ElectronicsPCB':((27,101,95),.24,.41,'microgrid'),
'OrbitDisplay':((106,76,183),.07,.17,'glass'),
'SafetyCoating':((234,128,66),.33,.39,'speckle'),
'InstrumentPanel':((75,107,130),.51,.33,'microgrid')}


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
