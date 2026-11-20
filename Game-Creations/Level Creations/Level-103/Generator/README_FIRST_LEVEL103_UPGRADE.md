# Level 103 — The Power Core / CPX-11: realistic PBR full-layout upgrade

**Complete package**: original 112 modular FBX/GLB pairs plus assembled reference, full original texture and data source, 113 renderable OBJ mesh fallbacks, 23 URP mask maps, additive Unity 6.3 Editor builder, minimal art-record Runtime components, 100-entry NOT_RUN acceptance checklist, safe installer.

**Upgrade-only package**: excludes original source asset ZIP; requires the original `Level103_CPX11` folder already installed in Unity. Both packages have identical generated assets and scripts.

## Installation

Close Unity; back up project; extract ZIP outside Assets; open PowerShell in extracted folder:

`powershell -ExecutionPolicy Bypass -File ".\Install_Level103_AAA_AutoLayout.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"`

Run Unity menu `Tools > Project Spark > Automatic Level Generator > 103 > 3. Validate Source and 3D Models`, then `1. Build FULL Level_103_Layout`. Select `Table_top`, then `2. Place Level 103 on SELECTED tabletop` (visual-only preview). The existing original source menus remain available.

### Engineering scope

Original FBX is centimetres, GLB is metres, Y-up. The builder uses metre-accurate OBJ from original GLB. Procedurally generated UVs and PBR materials are an **AAA-style starting point**, not a production-grade baked asset. All 116 routes and 30 vias explicitly lack approved pin-level electrical assignments; W117 is a conceptual monitored signal. CPU/VRM electrical gameplay, repairs, UI actions, and 6/6 certification are NOT implemented or tested in Unity. No existing levels or global Project Spark systems are overwritten.
