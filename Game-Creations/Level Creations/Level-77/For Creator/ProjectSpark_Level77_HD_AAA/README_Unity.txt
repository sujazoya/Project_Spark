PROJECT SPARK — LEVEL 77: REVEAL THE CIRCUIT
Advanced triple-stage copper CMP and interconnect isolation — modular FBX/GLB ART PACK

CONTENTS: 62 numbered binary FBXs; 62 corresponding GLBs; assembled reference FBX+GLB; 44 real animation parent pivots; manifests, CSV integration design, 4 optional Unity scripts, original procedural source, verification report.

UNITY QUICK IMPORT
1. Extract ZIP to a working folder. Import all 62 numbered files from Models/ into your Unity Assets folder.
2. Validate the assembled equipment is ~20.4 Unity metres wide when 1 Unity unit = 1 m. Binary FBX vertex coordinates are numeric centimetres (UnitScaleFactor=1); GLB vertices use numeric metres. Configure the FBX importer conversion exactly once, do NOT multiply by 0.01 again if already converted.
3. Place SparkLevel77ArtPlacementEditor.cs, SparkLevel77MaterialUpgradeEditor.cs and SparkLevel77AssetValidatorEditor.cs in a Unity Editor folder. Place SparkLevel77VisualRig.cs in a runtime scripts folder.
4. Select your Level 77 layout GameObject. Use Tools/Project Spark/Levels/77/Place HD Art Under Selected Layout to instantiate L77_HD_VISUAL_ART (62 individually origin-aligned FBXs).
5. Select the art root, run Tools/Project Spark/Levels/77/Create & Apply HD Materials To Selected Art. These are preview materials only; inspect URP/HDRP/Built-in transparency and emission yourself.
6. Run Tools/Project Spark/Levels/77/Validate Imported HD Art. Check 62 models, ~20.4 m combined width, 44 named pivots.
7. DO NOT place Models/Level77_Complete_ArtKit_Showcase.fbx in the same scene with all numbered FBXs; it duplicates the entire machine.

SCOPE LIMITS
- Gameplay_Wiring_Concept_W001_W114.csv and Terminal_Anchor_Plan_176.csv are planning-only; they are NOT actual circuit wires or SparkTerminal objects.
- W055 is reserved as the critical optical barrier-endpoint feedback wire; must be implemented through real circuit topology, safe stop and recalibration logic.
- Copper/barrier/dielectric material removal, dishing, erosion, cleaning, corrosion, isolation metrology, PLC safety, disturbances and six authoritative objectives are NOT included.
- Unity compilation and Play Mode have NOT been tested. The models use procedural geometry and simple material palette regions, not final production hand-authored PBR, LODs, optimized atlases or baked animation clips.

See Model_Manifest.json, Animation_Rigs.csv, Component_Roles.csv, Art_Assembly.csv, Material_Palette.json, Gameplay_Integration_Plan.md, Source/*.py and Verification_Report.json.
