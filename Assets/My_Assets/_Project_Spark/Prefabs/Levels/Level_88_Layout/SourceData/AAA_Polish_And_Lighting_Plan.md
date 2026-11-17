# Level 88 — AAA Production Polish Plan

**Composition:** Premium ECLSS laboratory, left twin CO2 beds, central electrolysis machine and O2 buffer,
right water recovery + blowers, rear garden habitat under arched glass, front PLC/SCADA consoles.
Warm amber cable accent against cyan glass and blue glow; avoid excessive bloom over legible wiring terminals.

**Production normals and UV:** Add proper chamfers and weighted vertex normals, unwrap each numbered FBX,
bake high→low normal/AO, create trim sheets, correctly map roughness as smoothness (one minus roughness)
per URP/HDRP shader. Texture_Manifest.json's R metallic / G roughness / B AO maps are visual references,
not currently UV-authored on meshes. Transparent materials require refraction/depth sorting review.

**Detailed physical polish:** Pressure-vessel nameplates, actual service labels, gas-line color standard,
fastener heads, valve handles, instrument markings, gasket edges, overhead pipe anchors, accurate cable
harness routes, internal transparent water tubes, textured foliage, gas-flow LED rails, rotating blower hubs.

**VFX:** Process-reactor bubbles, cyclical adsorber indicator gradients, pressure trend strips,
steam/condensate effects, duct directional current, warning beacons and W145 safety alarm. Bind only to
actual values from the Unity authority; do not pretend blue hologram geometry alone proves operation.

**Optimization:** For 90 modular pieces, mesh combine static floor/piping only, instancing for repeated
coils, cartridges, vents and garden foliage; do not static-batch animation pivot children; create
LOD0/1/2 and simple proxy colliders before shipping. Author occlusion zones and lighting reflections
suited to the user's actual rendering pipeline and target platform.
