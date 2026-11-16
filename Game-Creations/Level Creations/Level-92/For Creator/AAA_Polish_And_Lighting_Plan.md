# Level 92 — AAA art finishing and lighting plan

## Signature visual direction
Dark porous carbonaceous asteroid terrain with irregular faceted silhouettes, sharp grazing sunlight, cold cyan subsurface scanning overlays, amber mechanical warnings, white titanium lander and gold contact hardware. Retain clean visual contrast between the dark rock zone and interior instrumentation.

## PBR authoring to complete
- Unwrap meshes with sensible texel density, bake high-poly normal/AO details and author production albedo, metallic, roughness and masks (current 36 textures are only 2D procedural reference swatches).
- Add geometric high-frequency details on anchors (hooks, flexures, joints), drilling teeth, cable strain relief, flange bolts, structural lattice and service handholds.
- Build readable decals and safety labels as actual textured surfaces in Unity (do not assume procedural geometry contains finished text).
- Apply relevant wear: rock dust on drill/feet, brushed titanium contact, darkened bit edges, anodized black electronics, glass cartridge containment and clean medical-like lab interior.

## HDRP/URP stage lighting
- Off-axis low sun and cold ambient from deep-space exterior; amber near-drill work lights and cyan scan indicators. Use reflection probes/SSR judiciously.
- Realistic glancing rock roughness and subtle sub-surface geological false-color data visualization. Holograms should have depth and occlusion but never obscure critical terminal sockets.
- Dust/debris must be small and slow-moving in low gravity and driven by actual drilling state, not continuous decorative spraying.
- Real thermal and engine VFX require authoring; included FBX files have only static material regions, not full particles/VFX.

## Geometry optimization
- Static combine/instancing only for nonmoving structure. Never merge a named `ANIM_*` pivot into a static mesh.
- Add collisions only where interaction/obstruction matters; landscape and rock walls need simplified proxy collision rather than per-triangle dynamic MeshColliders.
- Create 3-level LOD for distant boulder clusters, spacecraft, and high-frequency drill details; disable unseen high-detail sample tubes.
- Evaluate CPU and draw calls in Unity Frame Debugger, monitor memory after textures and explicit shader setup.
