PROJECT SPARK — LEVEL 85: THE LAST METER
===========================================

REAL 3D ART ASSETS + PLANNING — NOT A FINISHED PLAYABLE UNITY LEVEL

Contents:
- 78 individually importable REAL binary FBX 7400 modular mesh files in Models/.
- 78 matching glTF 2.0 GLB modular mesh files in GLB/.
- An assembled reference showcase FBX and GLB; DO NOT add these together with 78 modules.
- 60 named ANIM_* mesh-owning parent transforms (FBX + GLB).
- 224 named SOCKET_P001..SOCKET_P224 mesh-owning DECORATIVE socket parents.
- 146 *conceptual* player wiring rows, including proposed W123 critical wire.
- 292 independently named logical connection endpoints; a visual socket can represent more than one logical contact. No real SparkTerminals are included.
- Six design-only objective certification specifications.
- 12 procedural PBR reference materials, 36 PNG textures (UV mapping not completed).
- Optional Unity Editor art placement, material color upgrade, import validation and runtime visual-only rig helpers.

Recommended import:
1. Import 78 numbered FBX files into Assets/ProjectSpark/Level85/Models/.
2. Import Textures/ and optional scripts, putting the three Editor helper scripts inside a Unity Editor folder.
3. Select the empty intended Level 85 layout root in the Hierarchy.
4. Run Tools/Project Spark/Levels/85/Place HD Art Under Selected Layout.
5. This idempotently creates ONLY a visual subroot: L85_HD_VISUAL_ART.
6. Run Tools/Project Spark/Levels/85/Validate Imported HD Art.
7. Optional: Tools/Project Spark/Levels/85/Create & Apply HD Materials To Selected Art.
8. Optionally add SparkLevel85VisualRig as a visual adapter. It is NOT a navigation/physics solver.
9. Independently implement circuit terminals, wiring, objectives, physics and tests in existing Project Spark game.

Scale: FBX numeric vertices and pivot translations are centimetres, UnitScaleFactor = 1.
GLB vertex/pivot coordinates are metres. Unity's resulting assembled station must be measured in metres; convert exactly once, never 100x twice.
Axes: Y up, X horizontal station approach, Z depth. ALL modular assets origin aligned at (0,0,0).

NOT INCLUDED:
- No real player wires or actual SparkTerminal nodes.
- No Unity compiled playable scenes/prefabs, actual six objectives, relative orbit simulator or working safety PLC.
- No guarantee of production hand-authored AAA LODs, authored UVs, lightmap baking or Unity GPU optimization.
- No Unity Play Mode/compilation validation.

Refer to Model_Manifest.json, Animation_Rigs.csv, wire and terminal CSVs, and Gameplay_Integration_Plan.md.
