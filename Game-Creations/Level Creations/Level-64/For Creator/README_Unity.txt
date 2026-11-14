PROJECT SPARK - LEVEL 64: HOLD THE SYNCHRONISM
36 individual procedural, AAA-inspired FBX models + 36 corresponding GLB assets.
Also contains assembled FBX/GLB reference files and rendered previews.

UNITY SETUP
1. Copy the Models/*.fbx files and Unity_Optional/SparkLevel64ArtPlacementEditor.cs into Assets/ within your Unity project (Editor script must be in an Editor folder or editor-only assembly).
2. Select the existing Level 64 layout GameObject in the Hierarchy.
3. Use Tools > Project Spark > Levels > 64 > Place HD Art Under Selected Layout.
4. The command verifies availability of ALL 36 FBXs before changing the scene, then builds L64_HD_VISUAL_ART.
5. Imported art uses a common origin, Y-up, front +Z, 1 unit = 1 meter. Verify Unity FBX axis conversion and scale.
6. The complete showcase should be used only for reference; do NOT overlay it with the individual models.
7. Named animation pivots are in Animation_Rigs.csv; animate only relevant model roots, not the stationarity of socket anchors.

SCOPE AND LIMITATIONS
This pack is procedural visual geometry. It is not final artist-authored AAA PBR; no authored 2K/4K textures or Unity material validation are included.
It does NOT contain actual 62 player-wire endpoint definitions, 96 SparkTerminal registrations, dynamic generator/rotor-angle simulation, relay functionality, six objectives or playable scripts.
Unity compilation, import, and Play Mode remain untested. Do not treat the hero preview graph as a physics measurement.
