import sys,json,csv
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import build_level86 as b
R=b.ROOT
palette=dict(level='level_86',palette=[dict(material=k,RGBA=v,role=('Emissive technical indicator' if k in ('cyan','green','amber','red','orange','violet','blue') else 'PBR render reference')) for k,v in b.C.items()],
  shader_notes='FBX and GLB use primary material colors; 12 optional procedural PNG sets need authored UVs and correct shader channel import, they are not baked/assigned production atlases.')
(R/'Material_Palette.json').write_text(json.dumps(palette,indent=2))
with (R/'Robot_Joint_Articulation_Chain.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=['Order','Model','Pivot','Axis','JointCenterX_m','JointCenterY_m','JointCenterZ_m','ParentPivot','Note']);w.writeheader()
 prev='ANIM_LINEAR_RAIL_CARRIAGE'
 for j,ind in enumerate(range(8,15)):
  pv=['ANIM_ROBOT_BASE_ROTATE','ANIM_ROBOT_SHOULDER_PITCH','ANIM_ROBOT_UPPERARM_PITCH','ANIM_ROBOT_ELBOW_PITCH','ANIM_ROBOT_FOREARM_ROLL','ANIM_ROBOT_WRIST_PITCH','ANIM_ROBOT_WRIST_ROLL'][j]
  pt=b.PIVOT_POS[pv]
  w.writerow(dict(Order=j+1,Model=b.NAMES[ind],Pivot=pv,Axis=['y','z','z','z','x','z','x'][j],JointCenterX_m=pt[0],JointCenterY_m=pt[1],JointCenterZ_m=pt[2],ParentPivot=prev,Note='Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle'))
  prev=pv
with (R/'Collision_LOD_Production_Plan.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=['Model','CollisionGuidance','LODGuidance','ProductionStatus']);w.writeheader()
 for i,n in enumerate(b.NAMES):
  kind='Box/truss simplification' if i<8 else 'Articulated joint convex collider, avoid per-bolt mesh colliders' if 8<=i<=15 else 'Simple convex or compound colliders' if 16<=i<=45 else 'Planar fin collider plus structural frame colliders' if 46<=i<=49 else 'Box/compound collider; no separate fastener meshes' if i<64 else 'Simplified selection and trigger volumes'
  l='LOD0 existing decorative geometry, author LOD1/LOD2 + impostor where justified; no LOD files included'
  w.writerow(dict(Model=n,CollisionGuidance=kind,LODGuidance=l,ProductionStatus='PLANNED_NOT_AUTHORED'))
(R/'AAA_Polish_And_Lighting_Plan.md').write_text('''# Level 86 — Final AAA Finish Production Checklist

The supplied work is **original procedural 3D geometry** with excellent modular coverage and readable machinery; it is not a fully artist-authored cinematic AAA mesh/bake. Finish these steps for production:

1. Author second UV channel for lightmaps, reduce mesh part count and atlas/control UV seams, rebake normal/AO and wear effects. Procedural reference textures are already available, but unwrapped production meshes are needed to apply them correctly.
2. Harden/soften normals and author precision bevels on major edges. The FBX meshes contain triangulated mechanical details; refine joint housing panel gaps and print logos/labels as mesh or decals.
3. Make LOD1/LOD2 and combine tiny fasteners where appropriate. Current assembled set has hundreds of groups; dynamic batching and instancing are needed in Unity.
4. Create convex joint collision proxies, compound robot links, non-interpenetrating tool and radiator contacts. Current package has no physics colliders.
5. Bake emissive status lighting and animate from actual robot/thermal states only. Add subtle contact spark and clamp light FX only for meaningful results.
6. Use physically-based scene exposure with warm amber hazard illumination, cool cyan supervisory instrumentation and white hard station-work lamps. Avoid overly saturated UI that obscures physical wiring and task affordances.
7. Make a scripted cinematic for panel extraction, installation and station cooling restoration; none of those effects grant an actual objective.
8. Create actual electrical terminal components, motor controllers, thermal plant, safety PLC and Objective evidence in the *existing* Unity project. This ZIP contains no playable wiring or objectives.

Material reference channels: packed R metallic, G roughness, B ambient occlusion; in Unity invert roughness as needed for smoothness. FBX centi-unit geometry and GLB metre geometry must be converted once.
''')
print('Support documents created')
