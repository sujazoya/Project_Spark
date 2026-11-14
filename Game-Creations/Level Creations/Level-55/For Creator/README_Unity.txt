PROJECT SPARK — LEVEL 55 / FLOAT THE SHAFT
HD / AAA-inspired magnetic bearing laboratory asset kit
===========================================================

CONTENTS
--------
Models/          20 individual FBX 7.4 files + 1 combined assembled FBX
GLB/             20 individually colored GLB backup meshes + 1 assembly GLB
Previews/        Hero, detailed machine view, top-down workbench and 20-model sheet
Exact_Wiring.csv 28 suggested W01-W28 connections with stable proposed terminal IDs
Terminal_Anchors.csv 48 visual locator names/positions in art coordinates
Animation_Rigs.csv Named transform animation pivots
Model_Manifest.json Model, triangle count, group and material summary
Unity_Optional/  Art-only Unity Editor placer and visual animation helper
Source/          Procedural modeling source and preview-rendering source

WHAT THIS IS / IS NOT
--------------------
This archive contains actual geometry with metallic/glowing colored material regions,
named animation pivots, authored 3D labels and visually modeled connector rings.
It is an art kit only. It does NOT include a completed level prefab, actual SparkTerminal
MonoBehaviours, electrical solver stamps, magnetic force calculations, controller
logic, a sensor-signal provider, or six working level objectives. None of those
functions is simulated by the optional visual-only helper.

UNITY IMPORT
------------
1. Create Assets/ProjectSpark/Level55/Art/ or equivalent.
2. Copy Models/*.fbx into that folder. FBX files use FBX 7.4 binary and Y-up,
   UnitScaleFactor = 100 (1 art length unit = 1 meter before any importer setting).
   The GLB files use standard glTF meter units; Unity may require a GLB importer.
3. Confirm the FBX model's imported dimensions in Unity before adjusting import scale.
   Nominal assembly width is about 10 art units; if the project uses a smaller bench,
   scale the WHOLE art-root uniformly, not the individual pieces independently.
4. Select the Level 55 layout root in the Hierarchy.
5. Copy Unity_Optional/SparkLevel55ArtPlacementEditor.cs into an Editor folder.
6. Use Tools > Project Spark > Levels > 55 > Place HD Art Under Selected Layout.
7. The Editor tool requires all 20 individual named FBXs; it does NOT automatically
   set SparkLevelDefinition, SparkCircuitSystem, SparkTerminal, or Shelf slots.
8. Do not enable both the full assembly showcase and the separate 20 models, or the
   geometry will be doubled. The showcase is a reference only.

ORIGIN AND POSITIONS
--------------------
All 20 individual FBXs use one common bench-space coordinate system. Instantiate
all at position (0,0,0), rotation identity, scale (1,1,1) under ONE art parent.
The long rotor shaft lies along +X. +Y is up. +Z faces front controls. The main
shaft nominal center is (0, 1.74, 0). The base nominal span is approximately
10 x 5.75 art units. Actual bounds are listed in the model manifest and GLB.

ELECTRICAL SOCKETS
------------------
The 48 socket rings and matching SOCKET_* transform nodes are VISUAL placeholders.
For each terminal in Terminal_Anchors.csv, register or instantiate your real
Project Spark terminal and bind its ID through SparkLevelSceneBindings. Do not
add duplicate terminal scripts to both the FBX child and an external controller.

Wiring W01-W28 is specified in Exact_Wiring.csv. The +5 V and RETURN anchors each
accept five wires, as supported by the project's real wire/circuit system.
The 28 wire entries use exactly 48 different endpoint IDs after deduplication.
Connect the right X signal W13 LAST for the deliberate final sensor-disconnect test.
Digital encoder signals, position feedback and active magnetic command wiring must
be validated by the existing Project Spark signal architecture. They are NOT
plain resistor branches; do not stamp arbitrary voltages into the DC solver.

ANIMATION / ART RIG
-------------------
Key FBX transform nodes include:
  ANIM_ROTOR_SPIN             main shaft, flywheel, sleeves (rotate about local X)
  ANIM_ENCODER_DISC           encoder disc and graduation marks (rotate about X)
  ANIM_LEFT_COIL_LIGHTS       left station indicator-art parent (stationary)
  ANIM_RIGHT_COIL_LIGHTS      right station indicator-art parent (stationary)
  ANIM_DISTURBANCE_PLUNGER   disturbance actuator plunger
  ANIM_SAFETY_DOOR           swing/open transparent guard (top hinge)

Use optional SparkLevel55VisualRig.cs as an art adapter. It does not create physical
states and never changes circuits or objectives. Its ApplyState method accepts
actual shaft angle, left/right X/Y displacement, guard openness and disturbance
from your authoritative Level 55 runtime systems. Set up references in Inspector.

MATERIALS
---------
FBX material names include: titanium, blue_steel, chrome, silver, dark, carbon,
copper, cyan, amber, electric, mint, green, red and acrylic. GLB files use vertex
colors. Convert imported materials to URP Lit/HDRP Lit as needed and set metallic,
smoothness, optional emission for cyan/amber/green/red. The transparent enclosure
acrylic material has authored opacity ~0.15; in Unity set Surface = Transparent and
alpha from the imported color as necessary. The FBX is not a preconfigured HDRP
shader package; polished final appearance depends on your scene lighting and shaders.

DESIGN GEOMETRY VS GAMEPLAY
---------------------------
20 separate model groups, 28 planned player wires and 48 visual sockets. Nominal
rotor: two radial magnetic stators, two protective touchdown bearings and a
separate shaft rotor. Sensing heads are visual; the four channels X-L, Y-L, X-R,
Y-R must be generated by verified gameplay mechanics rather than animations.
Electrical/control physics, PID tuning, magnetic force/current models and the
actual six objectives are intentionally NOT included here.

SOURCE / REBUILD
----------------
The geometric generator and VTK preview renderer are included in Source/ for
repeatability. They use Python, NumPy, trimesh and VTK. The provided FBX and GLB
files are already exported; Unity users do not need Python to import the pack.

VALIDATION
----------
The pack was structurally checked and the GLB meshes were loaded and rendered.
Actual Unity FBX import, shader setup, scale behavior and Play Mode have NOT been
run in the user's Unity project. Verify those locally before production use.
