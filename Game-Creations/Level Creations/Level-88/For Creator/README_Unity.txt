PROJECT SPARK LEVEL 88 — THE BREATHING RING — ACTUAL MESH ASSET DELIVERY

Package is original procedural 3D art, NOT a working Unity gameplay level.
Models/: 90 numbered binary FBX 7400 modular mesh scenes + one assembled visual reference.
GLB/: 90 matching glTF GLB scenes + one assembled visual reference.
Textures/: 12 procedural material sets, 36 1024x1024 PNG maps (reference, not UV baked).
Unity_Optional/: 3 Editor scripts + 1 runtime actual-state-only visual-rig script.
CSV/JSON: verified individual model names, common-origin assembly, movable pivots, gold socket anchor plans,
concept-only wires, logical port reconciliation, planned objectives and structural verification.

UNITY QUICK START
1. Copy `Models/*.fbx` (except showcase) into Assets/ProjectSpark/Level88/Models/.
2. Copy Textures/ into Assets/ProjectSpark/Level88/Textures/.
3. Put files ending Editor.cs inside an `Editor` directory; keep SparkLevel88VisualRig.cs OUTSIDE Editor directories.
4. In Unity's FBX model importer, confirm numeric vertex centimetres become scene metres EXACTLY ONCE.
5. Select your Level 88 layout in Hierarchy and run Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout.
6. Run Tools/Project Spark/Levels/88/Create & Apply HD Materials To Selected Art (preview palette only).
7. Run Tools/Project Spark/Levels/88/Validate Imported HD Art. Check 90 models, 72 pivots, 242 art sockets.
8. Use Model_Manifest.json, Gameplay_Wiring_Concept_W001_W164.csv and
   Terminal_Logical_Port_Reconciliation_328.csv to build the REAL gameplay separately.
9. Do NOT place the assembled Level88_Complete_ArtKit_Showcase.fbx next to all 90 modular models: that would double the art.
10. Store all actual SparkTerminals, physics, safety PLC, wiring and objective components OUTSIDE replaceable art root.

All modular pieces authored with common world origin (0,0,0) and no placement transforms.
Axes: Y up, X width, Z forward. GLB geometry metres; FBX numeric coordinates centimetres;
FBX UnitScaleFactor=1 => importer should interpret centimetres; verify geometry bounds after import.

QUALITY STATUS: hard-surface original procedural preview meshes with material assignments and pivots.
Production UVs, hand-authored AAA bakes, custom LODs, collision meshes, custom VFX, authored labels,
shader finishing, Unity compilation, Play Mode and ECLSS physics are NOT included.
