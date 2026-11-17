# Level 95 — AAA-inspired look development guide

## Materials included
- 21 distinct PBR reference families, six PNG maps each at 1024×1024, for 126 shared texture files.
- BaseColor (sRGB), tangent-style reference Normal, MetalRoughAO, UnityMaskMap (R metal / G AO / A smoothness), Occlusion, Emission.
- FBX meshes contain numeric UV coordinates, vertex normals and material slots but not embedded 1K maps. Use the Unity PBR installer to create native editable materials and link shared textures.
- Shaders: `ProjectSpark/Level95/PulseHologramBuiltIn` and `ProjectSpark/Level95/PulseHologramURP`; HDRP uses generated native Lit emission.

## Look
- Blue-black PCB soldermask with yellow-gold ENIG pads, micro silkscreen, realistic black chip package leads and capacitor terminations.
- Neutral brushed metal and graphite bench furniture, restrained cyan / amber edge lighting.
- Avoid over-illuminated footprints; the copper/net inspection mode should be a separate diagnostic overlay.
- Glowing amber PWM and green clock LED must reflect *measured* simulation state once implemented.
- Use reflection probes, contact shadows, subtle AO and antialiasing consistent with mobile/desktop target hardware.

## Physical finishing needed
- Hand-authored UV islands, bevels, normal bakes, micro-decals and realistic solder shape finishing are recommended before 'AAA production-ready' signoff.
- Collision proxies and LOD meshes are plans only, not generated asset geometry.
- Custom shader compilation and material pipeline appearance must be verified inside the user's chosen URP/HDRP/Built-in Unity version.
- No external stock images, font assets or third-party texture licenses are included.
