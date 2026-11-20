# PROJECT SPARK LEVEL 97 — VOLTAGE GUARDIAN BOARD-ONLY KIT

Actual procedural binary FBX 7400 + GLB geometry. Exactly 68 modular assets (1 empty board, 50 installable parts, 17 extra board-only details) and a separate assembled showcase reference. Designed for a 160×110 mm, four-layer board. 20 PBR texture families with shared maps plus a silkscreen and four reference layer textures. No room or physical UI instruments.

## Import
Import the entire folder under Assets/ (any subfolder), keep Models_FBX, Textures, Data, and Unity intact. Put the C# Editor file in a Unity Editor folder as supplied. Menu: Tools > Project Spark > Levels > 97 > Board Only > 1. Create EMPTY VGR-5 PCB. Then generate native materials and assign them to matching imported FBX material slots. The reference FBX is visual-only and intentionally populated. Unity and render-pipeline import have not been tested here.

## Critical distinction
The artwork is not a validated electrical netlist. `Copper_Routing_Concept_W001_W066.csv` is a route budget with placeholder pads, not a proven schematic; reconcile every pin, supply domain and layer via with Project Spark's real circuit graph. All components, 66 player routes, 14 vias and removable W067 start incomplete/uninstalled. Displayed static factory detail cannot complete player routing. W067 is a signal-validity jumper, not the 5 V power output.

## Physical metrics
FBX and GLB vertices are authored in meters, with Y up, X board width (0.160 m), Z depth (0.110 m). Component local geometry origins are at the board-side contact reference; board top = 0.0008 m. Place each component using `Component_BOM_50.csv`: world local position (x_m, 0.00092, z_m). For table displays, scale only a mesh-only preview, not electrical simulation.

## Production limits
Procedurally modeled realistic-style parts with separate metal pins and contacts, SMDs, copper turns on inductors, plated pad features, board layering and repair coupons. Manufacturing DRC, UV baking, collision/LOD, actual PCB simulation and Unity Play Mode require development. Textures are reference materials; manually confirm slots and metal/smoothness conventions under your Unity pipeline.
