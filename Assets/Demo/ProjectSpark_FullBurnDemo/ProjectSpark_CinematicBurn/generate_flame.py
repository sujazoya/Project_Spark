from PIL import Image
import numpy as np
from scipy.ndimage import gaussian_filter
size=128; n=4
atlas=Image.new('RGBA',(n*size,n*size),(0,0,0,0))
Y,X=np.mgrid[0:size,0:size]
x=(X+0.5)/size*2-1; y=Y/size
for frame in range(16):
    rng=np.random.default_rng(8200+frame)
    phase=frame/16*2*np.pi
    wobble=0.14*np.sin(y*9+phase)+0.05*np.sin(y*23-phase*1.7)
    taper=np.maximum(.035,(1-y)**1.35*.45)
    dist=np.abs(x-wobble)
    plume=np.exp(-(dist/(taper+0.05))**2.4)*np.maximum(0,1-y)**0.55
    tip = np.maximum(0, np.sin(19*y+3*x+phase)*.18+.82)
    fuzz=gaussian_filter(rng.random((size,size)),3.2)
    fuzz=np.clip((fuzz-.38)*3.1,0,1)
    alpha=np.clip(plume*tip*(.48+.85*fuzz)*1.35,0,1)
    alpha*=np.clip(y*18,0,1)
    core=np.exp(-(dist/(taper*.36+.03))**2)
    heat=np.clip((1-y)*.75+core*.7,0,1)
    r=np.ones_like(alpha)
    g=np.clip(.04+heat*.87,0,1)
    b=np.clip(.015+heat**3*.32,0,1)
    rgba=np.stack([r,g,b,alpha],axis=-1)
    img=Image.fromarray((rgba*255).astype('uint8'),'RGBA')
    atlas.paste(img,((frame%n)*size,(frame//n)*size))
atlas.save('/mnt/data/ProjectSpark_CinematicBurn/PS_FlameFlipbook_4x4.png')
