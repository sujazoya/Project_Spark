# PROJECT SPARK — LEVEL 106: FIRST BOOT
## SFX-14 Board-Only Realistic 3D Source Kit

This kit contains 136 modular FBX meshes, 136 GLB counterparts, and one complete assembled visual reference in both formats.

## Contents
- `Models_FBX/` contains geometry for the bare board, 106 independently placeable components, layer, socket and repair details.
- `Models_GLB/` provides corresponding GLB previews and inspection geometry.
- `Textures/` contains PBR starter maps and a 2K silkscreen reference.
- `Data/` contains component placements, 140 *planned* routes, 42 *planned* vias, objectives, faults and W141 safety definition.
- `Unity/Editor/` has non-destructive editor helpers for importing the empty art board and starter materials.

## IMPORTANT: NOT A FULLY PLAYABLE LEVEL
This package provides original editable *procedural* geometry; it is not an artist-finished AAA asset set.
FBX files use a minimal binary 7400 writer; Unity/Blender import, material slot mapping and mesh quality must be verified in the target Unity version.
Component package dimensions and pads are artistic approximations, not manufacturer-certified footprints.

## Real size and coordinates
Units in FBX / GLB geometry are meters. Bare PCB: 0.285 m x 0.205 m x 0.0016 m.
Component model origins are local; `L106_Components_106.csv` contains placement coordinates in millimeters.
Positive Z is board top. Unity Y-up coordinate conversion may require rotating the imported root. Verify before placement.

## Gameplay restrictions
- ALL 140 route definitions and 42 vias are UNASSIGNED and NOT_CERTIFIABLE until reconciled with an authoritative pin-level netlist.
- W141 endpoints U14/U15 are functional planning labels; actual component pins and net IDs require specification.
- Visual geometry, factory-style decorative copper and completed showcase do NOT certify connections.
- The playable board starts with 106 parts uninstalled, 140 missing routes, 42 missing vias and W141 disconnected.
- Do not use the assembled reference to populate playable gameplay state.
- Reuse existing Project Spark virtual UI-based inspection and repair tools, rather than importing physical instruments.
- Preserve Levels 1–105 and use LevelGamePlayManager as the authoritative objective owner.

## Unity art import
1. Extract this folder anywhere under your project's `Assets/` folder.
2. In Unity, let FBX imports finish and inspect normals/materials/scale.
3. Select Tools > Project Spark > Levels > 106 > 1. Build EMPTY SFX-14 Board.
4. Optionally use the other menu options to apply FBX import settings and generate starter materials.
5. Integrate the gameplay solver only after a reviewed schematic and pin-level netlist exist.
