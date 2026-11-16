# Level 86 — Final AAA Finish Production Checklist

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
