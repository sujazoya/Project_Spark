# PROJECT SPARK — LEVEL 98 TRANSIENT SHIELD
## Board-only realistic 3D geometry

Actual geometry: 74 separate FBX + 74 GLB modular files; 56 separately placeable components; 18 board-only structural and diagnostic assets; assembled visualization FBX + GLB. All units use METRES; PCB 0.180 x 0.120 x 0.0016 m.

### Scope
Only PCB substrate, components, copper layers, solder contacts, vias, repair details, W075. NO room, furniture, test instrument FBXs, UI, or game mechanics. Use existing Project Spark world-space tool interfaces.

### Unity import
Copy package content under Assets/ProjectSpark/Level98/. Ensure Editor script is in an Editor folder. Run Tools > Project Spark > Levels > 98 > Board Only > 1. Build EMPTY PIS-6 Board. Then generate native materials. If the FBX import reports geometry or axis issues, use the matching GLBs as reference and inspect Unity's Model Import settings.

### Crucial gameplay guard
The assembled showcase is VISUAL ONLY. Actual gameplay starts with 56 components UNINSTALLED, 74 routes INCOMPLETE, 16 vias MISSING, W075 UNINSTALLED. Some factory copper pads and paths are decorative; do not use them to certify circuit topology. The route and via planning CSV files do NOT constitute a pin-verified netlist.

### Quality limits
Original procedural realistic-style meshes and basic authored PBR maps. FBX export is FBX Binary version 7400 with geometry, UV layers and material name slots. Offline validation checks file structures and GLB loading, not Unity FBX import, assembly fit, Play Mode, manufacturer footprints or authoritative electrical operation.
