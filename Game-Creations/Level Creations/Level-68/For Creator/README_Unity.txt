PROJECT SPARK — LEVEL 68: TAME THE HEAT
HD PROCEDURAL MODULAR 3D ASSETS — UNITY IMPORT GUIDE

CONTENTS
 Models/ 44 individual binary FBX 7.4 objects + assembled showcase FBX
 GLB/ 44 individual GLB backups + assembled showcase GLB
 Previews/ isometric geometry hero and 44-item contact sheet
 Model_Manifest.json 44 exact filenames, triangle counts, pivots
 Animation_Rigs.csv 14 verified reference pivot mappings
 Unity_Optional/SparkLevel68ArtPlacementEditor.cs art-only placement
 Source/ procedural generation and preview scripts
 Verification_Report.json integrity report

UNITY IMPORT
 1. Extract archive. Copy Models/*.fbx to Assets/ProjectSpark/Levels/Level68/Art/Models/.
 2. Place SparkLevel68ArtPlacementEditor.cs inside a Unity Editor folder.
 3. Inspect model import Scale Factor 1 and axis orientation; source X width, Y up, Z front.
 4. Select existing Level 68 layout root in Unity Hierarchy.
 5. Run Tools > Project Spark > Levels > 68 > Place HD Art Under Selected Layout.
 6. 44 individual prefabs are placed under L68_HD_VISUAL_ART at common origin.
 7. Do NOT also instantiate Level68_Complete_ArtKit_Showcase.fbx: it duplicates geometry.
 8. Reassign transparent/PBR materials appropriate to project's render pipeline if needed.

ANIMATION / ART LIMITATIONS
 Pivot coordinates are references, NOT complete gameplay rigging or authored animations.
 Reparent moving plate doors, pump impellers, valve handles and other geometry as needed.
 Liquid visuals/temperature trends must be driven by your authoritative Unity simulation.
 Art quality: genuine procedural HD/AAA-inspired triangulated geometry and material colors;
 NOT handmade final AAA PBR textured artwork. No authored texture sets are bundled.

GAMEPLAY NOT INCLUDED
 No SparkTerminal registrations; no 78 player wires; no 122 functional sockets.
 No heat-transfer dynamics, exchanger effectiveness, PID, sensor faults, safety or 6 objectives.
 No Level_68.asset / Level_68_Layout.prefab. Unity Play Mode/import not tested.

EXPORT CHECKS
 See Verification_Report.json. Binary FBX structural offsets, GLB mesh/triangle counts,
 pivot CSV, and archive integrity are checked programmatically.
