# Project Spark L89 — AAA Production Art Polish and Lighting

## Composition
Left: spectacular dual solar-particle telescopes, calibration instrumentation, orange/amber space-weather holograms.
Center: huge 6.2-m class modular shielded storm shelter and door. Right: blue segmented transparent water-wall
tanks and shield-control valve/pump skid. Front: dense gold-rimmed terminals, PLC racks and usable status panels.
Backdrop: stylized station habitat and panorama with luminous orange solar-event warning motifs (a static art environment).

## Required final production work beyond this generated kit
- Author proper UV unwraps, production trim sheets, normals/roughness/AO, decals and glass transparency depth ordering.
- Add bevels/weighted normals and collision meshes, LOD0/LOD1/LOD2 or mesh simplification and GPU instancing where appropriate.
- Bake texture maps from production geometry. The 36 included maps are procedural **reference maps without finished UVs**.
- Build real material variants for stainless/ceramic water shielding, brass contacts, radiation-detector optics and black-polymer seals.
- Add authentic component engraving and readable electrical terminal numbers as authored textures or UI assets.
- Distinguish amber storm warnings, blue water inventory, red fault lamps and cyan valid sampling without overwhelming readable wiring.
- Create and tune artist-authored VFX (sensor acquisition, flow particles, hologram animation), lightmaps/reflection probes and UI.
- Verify station ring background, shelter geometry, telescope direction and control workstation visibility from the actual game camera.

## Performance
Plan scene bounds from `Model_Manifest.json`. Do not duplicate the assembled visual showcase alongside 92 models.
Use instancing for repetitive bolts, optimized glass and shell geometries, appropriate physics layers and simple colliders.
All animation pivots are art read-only consumers of actual state, with `indicator` types not treated as machine actuators.
