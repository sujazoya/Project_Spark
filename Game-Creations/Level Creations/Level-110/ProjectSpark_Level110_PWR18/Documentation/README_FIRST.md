# PROJECT SPARK — LEVEL 110: POWER INTEGRITY / VOLTAGE DOMINION

**PWR-18 procedural motherboard visual art kit — NOT a completed AAA art pass or functional circuit.**

## Deliverables
- 168 individually authored FBX files and 168 matching GLBs (130 installable component records; 38 supporting structures).
- One complete assembled reference motherboard FBX + GLB placed as a separate asset, including 130 components and mounted structures.
- Empty PCB FBX, PBR starter color/roughness/metallic maps, component model + placement CSVs, route/via planning records, faults, safety and objectives.
- Unity Editor import/scene layout helper, board preview and validation notes.

## Dimensions and import
310 x 230 mm, 12 copper layers intended. Geometry is authored in **millimeters**, **XY board plane**, **Z up**. In Unity validate FBX axis/scale and use 0.001 scale conversion only where required by import units. Do not apply two separate conversions. Verify actual imported mesh bounds. FBX is ASCII FBX 7.4; some pipelines only support binary FBX, so prefer importing GLB into Blender and re-exporting FBX if your importer rejects ASCII.

## Starting gameplay state
0/130 installed, 0/172 certified routes, 0/56 verified vias, W173 disconnected, 0/6 objectives. The complete assembled FBX is **REFERENCE ART ONLY** and must not award gameplay completion.

## Critical limitations
- All 172 routes and 56 vias lack approved pin-level netlists / via spans; they are NOT_CERTIFIABLE.
- W173 physical pin mapping not approved. Actual safety signal must be freshness-qualified, not a fixed HIGH bit.
- No C# electrical simulation, tested Unity scene, hand-authored UVs, manufacturer-certified footprints, full 4K PBR map stack, LOD tuning or AAA photogrammetry is included.
- Multiple decorative portions of the board are stylized; assess and rework component intersections and clearances in Blender / Unity before use as gameplay authoritative footprints.

## Workflow
Extract to `Assets/ProjectSpark/Level110/`, preserve directories, import the models, verify board bounds, then run `Tools/Project Spark/Levels/110/Create PWR-18 Reference Art` (Editor script supplied). This instantiates a visual-only board. Build the playable empty scene separately using approved netlist and existing Project Spark systems.
