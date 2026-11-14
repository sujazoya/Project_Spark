PROJECT SPARK - LEVEL 66 - SAVE THE FACTORY
HD PROCEDURAL 3D MODEL PACK / UNITY ART-ONLY PLACEMENT

CONTENTS
  Models/             40 independent binary FBX 7.4 meshes + one assembled FBX
  GLB/                40 corresponding GLB 3D backups + one assembled GLB
  Previews/           hero 3D render and 40-item contact sheet
  Animation_Rigs.csv  12 named animation pivots with coordinates
  Model_Manifest.json exact filenames, face counts, and pivot mapping
  Unity_Optional/     visual-only art placer
  Source/             scripts for reproducible procedural geometry/preview
  Verification_Report.json output validation and limitations

IMPORTING INTO UNITY
  1. Extract the ZIP to your desktop/workstation.
  2. Copy Models/*.fbx to Assets/ProjectSpark/Levels/65-66/Level66/Art/Models/.
  3. Copy SparkLevel66ArtPlacementEditor.cs to any Unity Editor-only folder,
     e.g. Assets/ProjectSpark/Editor/Levels/.
  4. Import FBX models at Scale Factor 1; inspect orientation and materials.
  5. Select the existing Level 66 layout GameObject in the Hierarchy.
  6. Menu Tools > Project Spark > Levels > 66 > Place HD Art Under Selected Layout.
  7. The script creates L66_HD_VISUAL_ART and places all 40 models at local origin.
  8. Do NOT also instantiate the assembled showcase FBX. It duplicates all objects.

COORDINATES AND PIVOTS
  X = bench width, Y = vertical, Z = front. One modeled unit = one Unity unit.
  Each individual asset is authored at the same common scene origin.
  The animation pivot nodes are geometry references, NOT physics controllers.
  Inspect imported rigs before attaching animations; for multi-gear motion,
  split/reparent individual moving meshes as needed to apply correct ratios.

GAMEPLAY NOT INCLUDED
  NO SparkTerminal component anchors.
  NO 70-player-wire map (planned design target only).
  NO 110 functional socket anchors (planned target only).
  NO physical motor/gearbox/vibration/thermal solver.
  NO predictive AI, condition monitoring logic, repair interaction or safety.
  NO Level_66.asset / Level_66_Layout.prefab / authoritative 6 objectives.
  No Unity compilation or Play Mode validation was possible in this environment.

ART QUALITY
  Real multi-part triangulated 3D geometry and material-color regions.
  Procedural HD/AAA-inspired, NOT handcrafted final AAA with PBR texture maps.
  Glass uses transparent color where supported by importer; Unity may need
  a transparent shader/material reassignment. Meshes are importable backups.

VALIDATION
  See Verification_Report.json for exact tests and file counts.
