# PROJECT SPARK — LEVEL 96 SMX-4 SENSOR MATRIX — BOARD-ONLY REALISTIC ART

## Contents
- 62 independently importable modular binary FBX 7.4 models and 62 matching GLBs
- 1 additional complete-assembly reference in each 3D format
- 44 detailed component meshes plus a real-scale 140 × 95 × 1.6 mm PCB
- Separate board-only manufacturing parts and repair targets, no furniture or instrument FBXs
- Albedo/normal/metallic-roughness reference maps, silkscreen/factory art
- Unity art builder and visual-only repair-target hook
- 58 conceptual player trace requirements, 12 via tasks and critical W059 scenario

## Unity
1. Copy `Models_FBX`, `Textures` and `Unity` inside your Project Spark project.
2. Put `SparkLevel96BoardOnlyArtBuilder.cs` under an Editor folder.
3. Select the intended Level 96 visual root.
4. Use `Tools/Project Spark/Levels/96/Board Only/1. Create Empty SMX-4 Board`.
5. Use menu `3. Generate Native PBR Materials` and remap FBX material slots.
6. The metallic-roughness maps use glTF packing; Unity mask-map packing is pipeline dependent and requires conversion.
5. Build real component and pad prefabs from `Component_BOM_44.csv` using your actual PCB interaction APIs.

## Important constraints
- Units are METERS; PCB is 0.140m × 0.095m × 0.0016m. Enlarge using a visual-only parent without scaling physical simulation.
- The complete reference model is not a playable level or a completed PCB puzzle.
- Top silkscreen/factory details are visuals ONLY; no player-created copper routes are pre-installed.
- Route labels are conceptual; exact component-pin netlist MUST be defined and verified against a real electrical schematic, the Project Spark solver and existing APIs before it can certify objectives.
- W059 is a trusted reference-validity monitor, not the 2.500V reference supply.
- Shader compilation, FBX Unity-import fidelity and Play Mode are not validated by offline structural checks.
- This is realistic procedurally generated art, not manufacturer-approved footprint geometry or a fully authored AAA production model.
