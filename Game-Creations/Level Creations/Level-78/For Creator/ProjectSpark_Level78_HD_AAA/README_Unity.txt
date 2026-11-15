PROJECT SPARK — LEVEL 78: SEAL THE SIGNAL
MODULAR FBX+GLB DIELECTRIC CAP / LOW-K PECVD / UV CURE / RELIABILITY ART KIT

CONTENTS: 64 numbered binary FBXs; 64 matching GLBs; 1 assembled reference FBX+GLB; 46 actual named animation parent pivots; manifest and planning CSVs; 4 optional Unity C# scripts; original procedural model source; verification report.

QUICK INSTALL
1. Extract ZIP and import 64 numbered FBX files from Models/ into a Unity Assets folder.
2. Check combined width ~21.2 metres when one Unity unit = one metre. FBX vertex values are numeric centimetres (FBX UnitScaleFactor=1); GLB numeric vertices are metres. Apply importer conversion once only.
3. Put SparkLevel78ArtPlacementEditor.cs, SparkLevel78MaterialUpgradeEditor.cs and SparkLevel78AssetValidatorEditor.cs into a Unity Editor folder. Put SparkLevel78VisualRig.cs into a normal runtime scripts folder.
4. Select Level 78 layout root and run Tools/Project Spark/Levels/78/Place HD Art Under Selected Layout. Creates L78_HD_VISUAL_ART with 64 origin-aligned numbered FBX prefab instances.
5. Select art root, use Tools/Project Spark/Levels/78/Create & Apply HD Materials To Selected Art. Material colors are previews; inspect transparent/emissive appearance in your render pipeline.
6. Run Tools/Project Spark/Levels/78/Validate Imported HD Art. Verify 64 imported models, ~21.2 metres, 46 named pivot transforms.
7. DO NOT also instantiate Models/Level78_Complete_ArtKit_Showcase.fbx; it duplicates all numbered models.

SCOPE: 118 planned player wires and 182 proposed terminal anchor locations are guides, NOT functional gameplay. W083 conceptual critical feedback wire, PECVD, plasma, curing, leakage reliability, PLC protection and six objectives need actual Unity implementation. Visual rig drives only geometry and optional VFX. Unity compile and Play Mode not tested. No baked animation/PBR texture atlases/LOD rigs.
