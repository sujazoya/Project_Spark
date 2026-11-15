PROJECT SPARK — LEVEL 76: FORGE THE CONNECTIONS
HD/AAA-inspired original procedural 3D asset kit — 60 FBX + 60 matching GLBs + assembled FBX/GLB.

QUICK IMPORT
1. Extract this package and import all 60 numbered FBX files under an Assets folder.
2. In Unity, verify the entire workstation is approximately 19.6 Unity metres wide (1 Unity unit = 1 metre). FBX numeric geometry is in centimetres with UnitScaleFactor=1; GLBs contain metres. Apply the correct importer conversion exactly once.
3. Place `SparkLevel76ArtPlacementEditor.cs`, `SparkLevel76MaterialUpgradeEditor.cs`, `SparkLevel76AssetValidatorEditor.cs` in a Unity `Editor` folder. Place `SparkLevel76VisualRig.cs` in a regular runtime scripts folder.
4. Select the Level 76 layout GameObject and run Tools/Project Spark/Levels/76/Place HD Art Under Selected Layout. This creates L76_HD_VISUAL_ART containing all 60 common-origin models.
5. Run Tools/Project Spark/Levels/76/Create & Apply HD Materials To Selected Art for preview materials (select L76_HD_VISUAL_ART).
6. Run Tools/Project Spark/Levels/76/Validate Imported HD Art to inspect model counts and bounding size.
7. The assembled reference FBX/GLB is for viewing only; do NOT instantiate it in addition to the modular models.

GAMEPLAY NOT YET IMPLEMENTED
The 110-connection plan and 170-anchor plan are DESIGN ONLY. They are not registered player wires or real SparkTerminal components. W076 is the proposed critical reference-electrode feedback. Six objectives, electroplating physics, sensors, safety and real player wiring need actual Unity project implementation.

Files: Model_Manifest.json, Animation_Rigs.csv, Art_Assembly.csv, Component_Roles.csv, Gameplay_Wiring_Budget.csv, Gameplay_Wiring_Concept_W001_W110.csv, Terminal_Anchor_Plan_170.csv, Gameplay_Integration_Plan.md, Material_Palette.json, Verification_Report.json, optional C# helpers, Source/*.py.

No claim of finished artist-authored PBR, baked animation clips, or Unity Play Mode certification.
