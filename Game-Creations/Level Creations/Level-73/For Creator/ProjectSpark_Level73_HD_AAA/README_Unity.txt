PROJECT SPARK — LEVEL 73: POLISH TO PERFECTION
Actual modular 3D art assets: 54 individual binary FBX + 54 matching GLB + assembled FBX/GLB.

QUICK START
1. Extract this ZIP. The Models/ directory contains all 54 numbered .fbx files.
2. Inside Unity, create/import an art asset folder (e.g. Assets/ProjectSpark/Level73/Models).
3. Import the 54 numbered FBXs. Do not use the assembled showcase together with individual equipment.
4. IMPORTANT MODEL SCALE: FBX vertex positions were serialized in centimeters; GLB positions are meters. The FBX declares UnitScaleFactor=1. Check Unity ModelImporter scaling before placement. The workstation should measure ~17.15 meters (X) after import, not 1715 Unity units. Apply a single 0.01 import conversion if needed, and avoid doubling it.
5. Copy SparkLevel73ArtPlacementEditor.cs and SparkLevel73MaterialUpgradeEditor.cs into an Assets/.../Editor folder. They are Editor-only tools.
6. Select an existing Level 73 layout GameObject in the Unity hierarchy.
7. Run: Tools/Project Spark/Levels/73/Place HD Art Under Selected Layout
   This creates/replaces ONLY L73_HD_VISUAL_ART under the selected layout.
8. Select the art root (or its parent) and run:
   Tools/Project Spark/Levels/73/Create & Apply HD Materials To Selected Art
   This creates editable materials in Assets/ProjectSpark/Level73/Materials.
9. Attach SparkLevel73VisualRig to the art root (runtime script, outside an Editor folder); its Awake method automatically searches for known ANIM_ pivot transforms in its descendants. Call its public SetActual... methods from actual gameplay logic. It does not simulate polishing.
10. Verify renderer transparency, materials, and lighting for your render pipeline (URP/HDRP/Standard).

FILE STRUCTURE
Models/                    — 54 numbered modular FBXs and one assembled 3D showcase FBX.
GLB/                       — 54 numbered modular GLBs and one assembled showcase GLB.
Unity_Optional/            — Art placement editor, material editor, visual-only rig adapter.
Source/build_level73.py    — Original self-contained source code for all geometry and binary FBX/GLB export.
Source/make_unity_support.py — Code used to regenerate the Unity art support scripts.
Source/verify_level73.py   — Independent FBX/GLB structural verification.
Model_Manifest.json        — All model IDs, filenames, triangle counts, bounding boxes, exact pivot groups, and gameplay caveats.
Animation_Rigs.csv         — Actual animated Null parent names and coordinates for 35 references.
Art_Assembly.csv           — All individual models at local Position (0,0,0), Rot (0,0,0), Scale 1.
Component_Roles.csv        — Per-model triangle, part and animation group counts.
Material_Palette.json      — Suggested material regions, colors and metallic/smoothness values.
Gameplay_Wiring_Budget.csv — Proposed 98-wire allocation ONLY.
Gameplay_Integration_Plan.md — Suggestions for functional Unity integration.
Verification_Report.json  — Export scan results.
SHA256SUMS.txt              — File checksums.

COORDINATES
X: workstation width, Y: upward, Z: toward front.
All component meshes share a COMMON WORLD ORIGIN — placement under one transform should use identity local transforms.
Asset origin: center of workstation floor.
Workstation width: approximately 17.15 meters.
Polishing platen center: approximately (0, 1.23, -0.55), radius 2.54m.
The animation Null groups are at real world pivot locations; their child mesh vertex positions were made relative to their pivots during FBX export. Rigging references in GLB are also parent transforms.
The named pivot groups contain actual moving geometry when feasible, not merely empty labels. No baked animation clips are included.

LEVEL 73 APPROVED GAMEPLAY PLAN — NOT IN THIS ZIP
54 art assets, planned 98 PLAYER-CREATED WIRES, planned 152 UNIQUE FUNCTIONAL TERMINALS, SIX authoritative objectives.
Those functional gameplay systems do NOT ship in this art pack:
- no SparkTerminal or SparkElectricalComponent components;
- no 98 gameplay wire endpoints or 152 functional socket objects;
- no platen/carrier motor physics, Preston-law material removal, slurry transport, pad-wear model, pressure control, metrology, safety PLC, six objective evaluations;
- no shop integration, load-cell disconnection/repair logic, LevelGamePlayManager completion.
Do not treat art-only outputs as an already playable Unity level. Build those in actual Unity project source.

QUALITY / VERIFICATION
Original procedural HD/AAA-inspired industrial 3D geometry: machined metal regions, industrial fittings, textured polishing-pad grooves, stainless/carbon surfaces, multiple UI/equipment details, animation pivot groups.
These are not handmade cinematic AAA assets, fully UV-unwrapped PBR surfaces, completed colliders, skeleton clips or physically simulated polished-wafer effects.
No Unity Editor or Play Mode test was performed in the current runtime. Verify compatibility in your actual project.
