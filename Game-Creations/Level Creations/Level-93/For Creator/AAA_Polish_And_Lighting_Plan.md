# Level 93 — Professional Art Finishing Plan

## Target visual identity
Clean scientific science-fiction aesthetic: white ceramic shell, brushed titanium, sealed glass archive cylinder, gilded pressure clamps, violet spectra and cyan volumetric reconstruction. The core is visually dominant; stations line both sides with safe player circulation.

## Suggested lighting (author in Unity)
- Cinematic contrast: warm key on sample vault, cool indirect fill on instrument racks, subtle violet on Raman and XRF heads.
- Realtime area-style highlights for specimen vault and science console; prefer baked lighting for static surfaces and URP/HDRP probes for reflective metal.
- Use volumetric beams only as bounded aesthetic holograms; X-ray radiation itself is not visible.
- Controlled Bloom for cyan/violet indicators, no overexposed floor, readable status text.

## Mesh finishing
- Keep hard mechanical silhouettes; add bevel normals, realistic bolted interfaces, improved UV unwrapping/texture atlas and secondary lightmap UVs.
- Apply actual URP/HDRP glass configuration including refraction where supported.
- Improve LOD and colliders; consider transparent glass double-sorting and shadow settings.
- Use consistent texel density, trim sheets and selective decals. Keep major labels in 2D Unity TMPro for legibility.

## Engine targets
- Full physical PBR using supplied maps as a starting point, not a claim of artist-authored baking.
- All 92 named moving assemblies should be driven from actual measured state, never from objective-complete booleans.
- Include performance budget, culling, batched static props, LODs and optional URP/HDRP advanced effects.
