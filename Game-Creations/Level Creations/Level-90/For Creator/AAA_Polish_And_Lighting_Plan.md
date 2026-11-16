# Level 90 — AAA Finish and Lighting Plan

## Art direction
Blue-violet Hall-thruster plasma, white/titanium spacecraft armor, deep blue solar cells, warm gold industrial mechanics and cyan scientific holograms. Keep the spacecraft central, maintain clear player circulation around racks, and distinguish actual engine-plasma emission from unrelated decorative highlights.

## PBR production
1. Create authored UV sets for every reusable modular mesh. Current generated PNGs are reference maps, not complete baked PBR material application.
2. Repack MetalRoughAO (R metal / G roughness / B AO) correctly for the project's actual URP/HDRP shader. Invert roughness to smoothness where needed.
3. Bake curvature, AO and detailed normal maps; author wear/edge treatment only where industrially plausible.
4. Use physically plausible translucent displays/optical windows, double-sided glass only where needed and carefully controlled alpha sorting.
5. Convert static trim to modular atlases, instanced detail for repeating cell/busbar geometry and cut LODs for long-distance ship views.
6. Add measured-state-driven runtime VFX: low-power discharge initiation, steady blue annular emission, hot surfaces, status LEDs and alerts. No animated geometry is evidence of live propulsion.

## Lighting
- HDR-space environment with gold practical workstation light; keep cockpit and precision control instruments readable.
- Engine glow must be enabled only from authoritative discharge state, fade on shutdown and respect safety inhibition.
- Restrained blue rimlight on spacecraft, real-time contact shadows on movable engine rigs, baked static fixtures.
- Limit cockpit emissive textures to avoid bloom washing out signal labels and hazard warnings.

## Performance
Separate near/far ship views and lab views. Use LODGroups and combine stationary hull trim. Profile draw calls and transparency; do not assume procedural mesh density automatically equals AAA quality.
