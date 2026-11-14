PROJECT SPARK — LEVEL 62 / HOLD THE VOLTAGE
PROCEDURAL AAA-INSPIRED MODULAR 3D ART — NOT A PLAYABLE LEVEL

CONTENTS
- 32 individual FBX 7.4 binary files; 32 GLB backups
- 1 assembled FBX / 1 assembled GLB showcase
- Level62_Hero_3D_Preview.png; Level62_32_Models_ContactSheet.png
- Model_Manifest.json; Animation_Rigs.csv
- Unity_Optional/SparkLevel62ArtPlacementEditor.cs

UNITY IMPORT
1. Unzip Models/*.fbx into Assets/ProjectSpark/Level62/Art/.
2. In Unity verify import scale 1; X bench width, Y up, Z front, 1 unit = 1 meter.
3. Select your existing Level 62 layout GameObject.
4. Put SparkLevel62ArtPlacementEditor.cs under any Assets/.../Editor folder.
5. Run Tools > Project Spark > Levels > 62 > Place HD Art Under Selected Layout.
6. The helper adds L62_HD_VISUAL_ART and instantiates 32 separate FBX models.
7. Do not render the complete showcase alongside the individual FBX instances.
8. Assign Unity-compatible materials and review geometry/animation pivot use.

IMPORTANT
Art kit only: it does not implement 54 gameplay wires, 84 real SparkTerminal
components, DVR physics, sensors, simulation, protection, or six objectives.
Animation_Rigs.csv lists independent named pivots and center positions.
FBX mesh parts include named material regions, not authored 2K/4K PBR textures.
Unity compilation/import/Play Mode have NOT been verified.

The scene is a low-voltage educational microgrid; no real mains connections.
