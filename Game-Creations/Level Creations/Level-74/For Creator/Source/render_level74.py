import math,json,time
from pathlib import Path
import numpy as np,trimesh
from PIL import Image,ImageDraw,ImageFont,ImageFilter
R=Path('/mnt/data/ProjectSpark_Level74_HD_AAA'); PD=R/'Previews';PD.mkdir(parents=True,exist_ok=True)
models=json.loads((R/'Model_Manifest.json').read_text())['models']
FONT='/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf'
F=lambda s:ImageFont.truetype(FONT,s)

def drawscene(files,sz=(1500,850),az=31,el=26,omit_transparent=False,margin=(50,95,50,48)):
    w,h=sz
    im=Image.new('RGB',sz,(9,21,34));d=ImageDraw.Draw(im)
    aza=math.radians(az);elev=math.radians(el)
    right=np.array([math.cos(aza),0,-math.sin(aza)])
    up=np.array([-math.sin(aza)*math.sin(elev),math.cos(elev),-math.cos(aza)*math.sin(elev)])
    depth=np.cross(right,up)
    geom=[];vertex_b=[]
    for file in files:
        scene=trimesh.load(file,force='scene')
        for mesh in scene.dump(concatenate=False):
            if not len(mesh.faces):continue
            try:color=np.array(mesh.visual.vertex_colors[0],dtype=int)
            except Exception:color=np.array([140,160,180,255])
            if omit_transparent and color[3]<205:continue
            geom.append((mesh.vertices,mesh.faces,color))
            vertex_b.append(mesh.vertices)
    if not geom:return im
    vv=np.concatenate(vertex_b)
    rightvals=vv@right;upvals=vv@up
    low=np.array([rightvals.min(),upvals.min()]);high=np.array([rightvals.max(),upvals.max()]);span=np.maximum(high-low,1e-3)
    L,T,Rm,B=margin
    scale=min((w-L-Rm)/span[0],(h-T-B)/span[1])
    mid=(high+low)/2
    sun=np.array([.43,.76,.47]);sun/=np.linalg.norm(sun)
    polys=[]
    for vertices,faces,col in geom:
        v=np.asarray(vertices);f=np.asarray(faces)
        tr=v[f];ct=tr.mean(axis=1)
        depths=ct@depth
        normal=np.cross(tr[:,1]-tr[:,0],tr[:,2]-tr[:,0]);normlen=np.linalg.norm(normal,axis=1)
        n=normal/np.maximum(normlen[:,None],1e-9)
        lighting=.50+.50*np.abs(n@sun)
        pix=np.stack(((v@right-mid[0])*scale+w*.5,-(v@up-mid[1])*scale+(h+T-B)*.5),axis=1)
        clr=np.clip(col[:3],0,255)
        for j,face in enumerate(f):
            if normlen[j]<1e-8:continue
            c=tuple(int(q*lighting[j]) for q in clr)
            polys.append((depths[j],[tuple(pix[q]) for q in face],c))
    polys.sort(key=lambda t:t[0])
    for _,pts,c in polys:d.polygon(pts,fill=c)
    return im

def titleband(img,title,sub,foot):
    d=ImageDraw.Draw(img);w,h=img.size
    d.rectangle((0,0,w,94),fill=(8,18,29))
    d.text((36,12),title,font=F(31),fill=(227,244,255))
    d.text((37,61),sub,font=F(16),fill=(123,220,246))
    d.rectangle((0,h-47,w,h),fill=(8,18,29))
    d.text((35,h-36),foot,font=F(16),fill=(122,218,198))
    return img

model_files=[R/'GLB'/a['glb'] for a in models]
print('RENDER: hero',flush=True)
hero=drawscene([R/'GLB'/'Level74_Complete_ArtKit_Showcase.glb'],(1500,900),az=27,el=28,omit_transparent=True,margin=(65,115,65,85))
herotext=f'56 MODULAR FBX  •  56 GLB  •  {sum(x["triangle_count"] for x in models):,} TRIANGLES  •  42 PIVOTS'
titleband(hero,'PROJECT SPARK / LEVEL 74 — ALIGN THE LIGHT','Precision DUV photolithography • projection optics • nano-positioning • wafer 4x4 fields',herotext)
hero.save(PD/'Level74_Hero_3D_Preview.png')
print('RENDER: contact sheet',flush=True)
cellw,cellh=300,195
sheet=Image.new('RGB',(cellw*4,cellh*14),(7,18,29));d=ImageDraw.Draw(sheet)
for i,(m,path) in enumerate(zip(models,model_files)):
    thumb=drawscene([path],(cellw-6,cellh-46),az=27,el=25,omit_transparent=False,margin=(6,6,6,6))
    x=(i%4)*cellw;y=(i//4)*cellh
    sheet.paste(thumb,(x+3,y+3))
    label=f'{i:02d} {m["name"][4:]}'
    if len(label)>31:label=label[:30]+'…'
    d.text((x+10,y+cellh-36),label,font=F(12),fill=(219,237,247))
    d.rectangle((x+1,y+1,x+cellw-2,y+cellh-2),outline=(48,103,143),width=1)
    if i%4==3:print(' rendered',i+1,'/',len(models),flush=True)
sheet.save(PD/'Level74_56_Models_ContactSheet.png')

