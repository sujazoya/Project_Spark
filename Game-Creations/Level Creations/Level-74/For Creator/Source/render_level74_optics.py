#!/usr/bin/env python3
"""Technical visualization of actual Level 74 FBX/GLB mesh geometry, excluding occluding enclosure."""
from pathlib import Path
import json
src=(Path(__file__).parent/'render_level74.py').read_text()
ns={'__name__':'level74_renderer_helpers'}
exec(src[:src.index('model_files=')],ns)
R=ns['R'];PD=ns['PD']
manifest=json.loads((R/'Model_Manifest.json').read_text())['models']
focus_ids=[15,16,17,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,38,39,40]
img=ns['drawscene']([R/'GLB'/manifest[i]['glb'] for i in focus_ids],(1400,900),az=30,el=21,omit_transparent=False,margin=(60,110,60,65))
ns['titleband'](img,'LEVEL 74 — PRECISION LITHOGRAPHY SCANNER','DUV source / 4:1 optics / reticle stage / nanometre wafer stage','ACTUAL PROCEDURAL 3D MESHES  •  CONTROL SIMULATION NOT INCLUDED')
out=PD/'Level74_Optical_Scanner_Detail_Preview.png'
img.save(out)
print('Technical detail preview:',out)
