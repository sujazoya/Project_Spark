PROJECT SPARK — LEVEL 79: BOND THE WORLDS
66 modular binary FBX and matching GLB models; one combined FBX and GLB showcase; 48 real named animation pivots.

INSTALL
1. Import all 66 numbered FBXs from Models/ into Unity Assets.
2. Verify combined equipment width approximately 22.45 metres with Y-up. FBX numeric units are cm (FBX UnitScaleFactor=1). GLB numeric units are metres. Convert FBX exactly once in Unity importer.
3. Put SparkLevel79ArtPlacementEditor.cs, SparkLevel79MaterialUpgradeEditor.cs, SparkLevel79AssetValidatorEditor.cs under a Unity Editor folder.
4. Put SparkLevel79VisualRig.cs under a normal runtime scripts folder.
5. Select Level 79 root and run Tools/Project Spark/Levels/79/Place HD Art Under Selected Layout. Creates L79_HD_VISUAL_ART and 66 origin-aligned modular model instances.
6. Apply preview materials via Tools/Project Spark/Levels/79/Create & Apply HD Materials To Selected Art.
7. Validate with Tools/Project Spark/Levels/79/Validate Imported HD Art (66 FBX, 48 pivots, ~22.45m).
8. Do not also instantiate combined `Level79_Complete_ArtKit_Showcase.fbx`; it duplicates the entire kit.

SCOPE: 122 wire descriptions and 188 proposed anchors are planning files. They are not functioning player wires, `SparkTerminal` components or playable objectives. W037 is a proposed critical laser feedback wire requiring real circuit integration. Optional visual rig is art-only. Unity compile and Play Mode have NOT been tested.
