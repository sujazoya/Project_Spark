PROJECT SPARK / LEVEL 74 — ALIGN THE LIGHT
REAL MODULAR 3D ASSET PACKAGE (VISUAL ART ONLY)

Inventory: 56 individual FBX files, 56 GLB backups, 1 assembled FBX, 1 assembled GLB.
Triangles: 140,212; separate mesh parts: 4,168; animated FBX/GLB pivot-parent references: 42.
Workstation approx 18.2m across (X axis). Y=up, Z=front, all models share original world origin.

WHAT IS INCLUDED
- Models/*.fbx — 56 separately importable REAL Kaydara FBX binary 7.4 files; one assembled full-scene showcase.
- GLB/*.glb — 56 matching loadable scene models; assembled reference GLB.
- Model_Manifest.json — model names, real mesh counts, geometry bounds, pivots.
- Animation_Rigs.csv — named FBX parent-null transform reference groups.
- Art_Assembly.csv / Component_Roles.csv — common-origin art placement and roles.
- Material_Palette.json — suggested Unity color/metal/glass preview values.
- Gameplay_Wiring_Budget.csv — PLANNED 102 player wires only, NOT playable connections.
- Unity_Optional/SparkLevel74ArtPlacementEditor.cs — optional art placement under selected Unity parent.
- Unity_Optional/SparkLevel74MaterialUpgradeEditor.cs — creates preview materials for pipeline adjustment.
- Unity_Optional/SparkLevel74VisualRig.cs — art-only visual motion methods; NOT lithography physics.
- Source/*.py — original geometry export, preview, verify and package scripts.
- Previews/*.png — optional previews of actual modeled geometry, NOT the cinematic concept render.

IMPORTANT SCALE / FBX IMPORT
- Authored in meters in Python.
- FBX vertices are numeric CENTIMETERS with FBX UnitScaleFactor=1; GLB is numeric meters.
- Verify Unity's FBX ModelImporter scale; chassis should be about 18.2 Unity units wide at 1 unit=1m.
- Do not multiply the corrected FBX scale twice.

PLACEMENT
1. Extract the ZIP, import all 56 numbered FBXs to Unity. For GLB previews, use an appropriate importer if needed.
2. Put the two Unity Editor-only scripts in an Editor folder. Put the visual runtime adapter in a runtime folder.
3. Select your Level 74 layout root GameObject.
4. Menu: Tools/Project Spark/Levels/74/Place HD Art Under Selected Layout
   -> creates L74_HD_VISUAL_ART and instantiates all 56 models with local zero position and identity rotation.
5. Menu: Tools/Project Spark/Levels/74/Create & Apply HD Materials To Selected Art
6. Verify FBX hierarchy and connect visual rig to your ACTUAL simulation states.

HIERARCHY / PIVOTS
- Moving mesh parts under named pivot parent Null groups are present in individual binary FBXs and GLBs.
- No baked animations or Unity animation controller; pivot transforms are authored for runtime animation integration.
- Wafer X/Y and reticle X/Y groups are distinct in the geometry source. Confirm Blender/Unity importer retains parent node transforms.
- Optical violet light is only a visible diagnostic representation; real 193nm DUV is not visible.

NOT INCLUDED / MUST BE BUILT IN THE UNITY GAME
- Exactly 102 real player-created wires and an explicit W01–W102 endpoint manifest.
- Exactly 158 real SparkTerminal IDs and power/signal compatibility.
- A real lithography simulation, stage motion control, position sensor validity, UV dose physics, focus and overlay control,
  simulated wafer coating/bake/development, 4x4 field processing, metrology, PLC safety and six authoritative objectives.
- Real unity project integration / import, Unity compilation and Play Mode were NOT tested in this container.
- Functional colliders, UV shaders, photorealistic hand-painted PBR texture maps or production LODs are NOT included.

QUALITY SCOPE
This is high-detail original procedural, AAA-inspired geometry, not an artist-authored final AAA PBR production model.
The geometry exporter validates polygon counts and FBX/GLB importability; a real Unity project must integrate and test functionality.
