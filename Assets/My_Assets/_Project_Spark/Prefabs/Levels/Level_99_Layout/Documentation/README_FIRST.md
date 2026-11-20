# Project Spark — LEVEL 99 SILICON BRAIN

**Board-only physically scaled procedural 3D kit.** No room, UI, workbench, tools or other level objects.

## Contents
- 80 separate modular FBX files and 80 matching GLBs.
- One assembled visual reference FBX and GLB, never for live gameplay.
- 62 separately placeable electronic components: U1–U8, R1–R19, C1–C18, Y1, J1–J4, LED1–LED4, SW1–SW2, D1–D2, W083, FB1–FB3.
- 18 PCB-only structure, layer, via, trace, service and fault detail meshes.
- 21 shared procedural PBR material families / 126 reference maps + one silkscreen atlas.
- BOM, component positions, 82 conceptual copper-route tasks and 18 via requirements.
- Six objective design records, three modeled faults and W083 recovery instructions.
- Unity editor art-assembly and material helper scripts.

## Unity
Import Models_FBX, Textures and Unity/Editor under Assets/ProjectSpark/Level99/. Choose intended visual root and run Tools > Project Spark > Levels > 99 > Board Only > 1. Create EMPTY SBC-7 Board. Use material installer, inspect FBX normals/axes and map generated materials.

## IMPORTANT
FBX files are original binary FBX v7400 with meshes, normals, generated UVs and named material slots. The source geometry is authored in METRES at 190 x 130 mm PCB scale. Asset geometry is realistic-style procedural art, not verified commercial component CAD.

The initial gameplay state is 62 components UNINSTALLED, 82 copper tasks INCOMPLETE, 18 vias MISSING and W083 jumper DISCONNECTED. The assembled model is a visual reference only. **Neither 82 routes nor 18 vias have a verified pin-level netlist.** A physical/electrical schematic must be built before the objective evaluator can certify circuits. Unity import, materials and gameplay were NOT Play Mode tested.
