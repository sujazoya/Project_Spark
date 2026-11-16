# Level 90 — The Long Burn — 3D Art Delivery Notes

- **94** separately importable binary FBXs / **94** matching GLB files.
- **80** named pivot parents including four three-part Hall-thruster rigs (yaw / pitch / discharge indicator), twin solar wings and xenon valve controls.
- **254** modeled decorative gold-socket marker positions.
- **176** planned, NOT created player wires and **352** independent logical contact roles.
- **67** reused visual anchor locations. Some use incompatible signal classifications. NEVER make these one electrical node.
- **12** separate procedural reference PBR texture materials (36 maps, 1024 square).
- AURORA-1 spacecraft body and deployable solar wings with separate FBX components, four annular Hall thrusters, cathode systems, high-pressure xenon tanks, magnetic coils, power modules, protected command electronics, sensor racks and control bay.

## Real deliverables and remaining work
FBX/glTF files contain actual, editable triangle meshes and spatial pivot nodes. Visual-tech-art quality is **AAA-inspired procedural**; it is NOT a final film-asset, hand-sculpted production AAA model, mesh-optimized game-ready LOD, or validated flight hardware model. Shader authorship, full UV unwrapping, collisions, lighting, emissive VFX, animation rigging hierarchy review and performance profiling remain to be done.

Planned wires, SCADA data, plasma behavior, thermal dynamics, propulsion thrust, finite xenon mass, safety latching and LevelGamePlayManager objective logic **do not exist** in this art package.
