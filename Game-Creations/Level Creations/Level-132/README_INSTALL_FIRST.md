# Project Spark Level 32 — ASRock B850I Lightning WiFi
Procedural visual authoring kit, not official CAD or photorealistic AAA certification. Coordinates and contacts are illustrative. No genuine pin map, netlist or firmware. Textures are procedural; normal maps flat. Native and GLB have UVs; FBX/OBJ geometry lacks UVs. Review physical alignments before release.

## Install
Close Unity. Extract outside Assets. Windows PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File ".\Install_Level32_B850I.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun
powershell -ExecutionPolicy Bypass -File ".\Install_Level32_B850I.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"
```
If replacing differing Level32/B850ILightning files, add -ReplaceExisting; installer backs them up. Does not replace other board folders. Existing unrelated Level 32 content is not removed.
Unity: Tools > Project Spark > Automatic Level Generator > 32 B850I > 2. Validate native sources then 1. Build FULL Level 32 (Native recommended). Reimport Models_Native after scripts compile if needed. Native importer mirrors X consistently with placement builder; verify FBX/OBJ orientation separately. Generated authoring scene has no camera/lighting setup. Select the workshop root and enter Play Mode to use inspector visualization controls. Rear flip is for isolated viewing, not a tabletop clearance guarantee. Diagnostic anchors reference assembled positions only; do not use them while exploded.

Physical board 170 x 170 mm; uniformly enlarged to 7.51 x 7.51 Unity units. Board origin centered, Y up. Back models extend below PCB. Unity compile, scene generation, installer execution, collision and visual validation not tested here.

Sources: https://www.tomshardware.com/pc-components/motherboards/asrock-b850i-lightning-wifi-review ; https://www.asrock.com/mb/AMD/B850I%20Lightning%20WiFi/index.asp ; https://download.asrock.com/Manual/B850I%20Lightning%20WiFi.pdf (layout pages 6–7, I/O page 9).
