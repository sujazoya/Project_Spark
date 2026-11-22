# Project Spark Level 131 — ASRock B760M Steel Legend WiFi
Expanded visual authoring kit, not a manufacturer-exact or certified AAA replica. 129 modular components, 131 FBX, 131 native mesh assets, 129 OBJ, assembled GLB, 36 procedural 2K maps (flat normal maps), 10 diagnostic visual targets and six mission objectives in data. FBX has no UVs; use native import for textured workshop. GLB has UVs and material factors, not embedded PNGs. PCB art is illustrative, including mirrored backside reuse. Geometry retained from initial Level131; electrical connectivity not provided.

## Install
Close Unity and back up project. Extract ZIP outside Assets. Run from extracted directory in Windows PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File ".\Install_Level131_AAA_AutoLayout.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun
powershell -ExecutionPolicy Bypass -File ".\Install_Level131_AAA_AutoLayout.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"
```
If replacing the initial Level131 kit, explicitly add `-ReplaceExisting` to BOTH commands. Installer backs up differing files and does not touch previous levels.

Open Unity. After scripts compile, reimport Models_Native and Textures if needed. Use Tools > Project Spark > Automatic Level Generator > 131 > 2. Validate native sources, then 1. Build FULL Level 131 (Native recommended). Generated scene and prefab live inside the Level131 Generated folder. Existing FBX and OBJ menus remain available. Preview prefab menus use native sources. Check orientation, collision/clearance and visuals; native meshes are artwork, not electrical authority. Unity compilation, actual import, Play Mode and Windows installer execution not tested here.
