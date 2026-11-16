# Level 91 — final AAA polish and lighting requirements

**Target:** high-precision cosmic observatory: cold titanium, tungsten-toned gold X-ray mirrors, deep graphite and black instrument electronics, violet celestial map accents, ice-blue clock indicators, warm gold safety rails. Maintain contrast and readability for player electrical interaction.

- Bake authored UV maps; inspect the source procedural PBR reference maps. Packed MRAO channels: R metallic, G roughness, B AO. URP/HDRP metallic workflow may need texture repacking and roughness→smoothness inversion.
- Nested X-ray concentrator shells must look like separate precisely polished annuli. Use segmented reflection probes/reflections, crisp normal maps, restrained roughness and no translucent lens implying ordinary optical microscopy. X-ray telescope visuals are grounded in grazing-incidence concentrator geometry, but not manufactured optical specifications.
- Original material sources in FBX/GLB are vertex-independent preview materials. Transparent `glass` and `holo` may need explicit shader mode, depth write and draw sorting in Unity.
- The four telescope gimbals require a **serial transform hierarchy** for real azimuth/elevation kinematics and movable sunshade; the provided named pivots are mesh-owning art groups, not guaranteed prebuilt game mechanics.
- Add selected-area Light references for gold atomic-clock glow, blue X-ray detector event activity, violet four-pulsar hologram and red W163 fault. No actual X-ray beams should be rendered as physically visible through the lab.
- Final HDRP/URP grading: realistic dark/cold polished metals, path-traced-style reflections where viable, local warm task lights and gentle cyan holograms. Avoid overbright bloom that washes out gold sockets.
- Author collision proxies separately: deck/walkways, cabinet boxes, telescope service rigs; no costly mesh colliders for tiny optical shells, bolts, grating or socket visuals.
- Create 50% and 15% LOD variants, GPU instance repeating optics/mirror shells, batch stationary structural objects and use material atlas for repeated instrument status icons.
- Provide responsive VFX for measured photon activity and uncertainty contraction, not synthetic 'pass' animations. Never derive circuit, navigation, clock, protection or objectives from art animations or light colors.
