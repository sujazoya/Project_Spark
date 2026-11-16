# Project Spark — Level 92: THE SILENT CORE
## Procedural HD / AAA-inspired 3D art delivery

### Actual supplied files (source-derived)
- **98 original modular binary FBX 7400 models** and **98 matching GLB scenes**.
- One **alternative assembled full-scene FBX** and one GLB showcase; do not instantiate the assembled model together with all numbered modular pieces.
- **165,004 modular triangles**, **1,280 material/pivot mesh chunks**, **88 animation pivot groups** and **266 gold-rimmed visual sockets**.
- **12 procedural reference PBR material groups**, **36 PNG maps**. Maps have *not* been artist-authored, UV-unwrapped or baked to this geometry.
- **188 proposed player-wire rows**, **376 proposed independent logical contacts**, **111 reused art socket positions**, including **97 mixed-kind contacts requiring explicit reconciliation**.
- **6 planned gameplay objective definitions** and fault specification for **W171**. Neither connectivity nor certificate completion is implemented.

### Specific modeled level equipment
- KHEPRI-7 faceted carbonaceous surface, fractured boulders, geophysical cross-sections and terrain scanning platforms.
- AURORA-1 observation vehicle, six-legged lander central hull, six independently named articulated legs, six separately named microspine anchoring feet.
- Multi-assembly rotary-percussive drill, separately movable spindle, carbide teeth, ball-screw feed, motor, percussive striker, axial load and torque instruments.
- Radar A/B, terrain LiDAR, seismic contact probes, subsurface fusion processors and a five-layer holographic geological display.
- Core capture gripper, two sealed core cartridges, six-pocket sample carousel, seal-test chamber, sample transport and tether/winch retrieval equipment.
- Safety PLC, electrical cabinets, patch panels, SCADA, independent emergency controls and scripted visual fault equipment.

### Import convention
- World arrangement is authored inside the geometry: **all numbered models share one world origin**. Import and instantiate all numbered modular models at local `(0,0,0)`, identity rotation and unit object scale **after conversion to metres**.
- FBX numeric vertices are **centimetres** with `UnitScaleFactor=1`; GLB coordinates are **metres**. Apply FBX conversion **once only**, verify a known meter-size cabinet and compare world bounds to manifest.
- Expected modular bounds metres: `[-17.5, -1.3832, -16.9278]` to `[17.5, 16.0, 15.0]`. Models follow **Y-up, X-right, Z-forward** coordinate convention.
- Use `L92_HD_VISUAL_ART` as an independently replaceable child of the Level 92 layout. Keep all real terminals, puzzle wiring, gameplay controls and authored objects elsewhere.

### Unity menu tools (art only)
- `Tools/Project Spark/Levels/92/Place HD Art Under Selected Layout`
- `Tools/Project Spark/Levels/92/Create & Apply HD Materials To Selected Art`
- `Tools/Project Spark/Levels/92/Validate Imported HD Art`
- Runtime visual adapter class `ProjectSpark.Visuals.SparkLevel92VisualRig`.

### Important production limitations
These are **original editable procedural mesh assets**, NOT a complete AAA production-quality level. The ZIP does **not** contain production UV texture bakes, proven collision meshes, LODs, advanced shaders/VFX, rig-tested robotics, simulated regolith, functioning SparkTerminals, actual player wires, gameplay PLC, or objective certification. Asset structure is checked offline; Unity import, compilation, and Play Mode are not verified here.
