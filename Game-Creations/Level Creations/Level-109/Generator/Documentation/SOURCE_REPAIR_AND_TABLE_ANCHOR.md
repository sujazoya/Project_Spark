# LEVEL 109 — SOURCE MANIFEST REPAIR + TRANSFORM TABLETOP ANCHOR

This package includes every source manifest required by the Universal Generator V2, with the `SourceAssets/Level109/Data` folder, 160 modular FBXs and GLBs, plus the completed board reference. It also patches the full-layout placement command to accept a `Table_top` GameObject with only a Transform, after an explicit confirmation dialog.

Install:
1. Close Unity and back up the Unity project.
2. Extract this entire ZIP to a folder outside your Unity project.
3. From that extracted folder in PowerShell run:

    powershell -ExecutionPolicy Bypass -File ".\Install_Level109_AutoLayout.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"

4. Reopen Unity and allow imports to finish. In the Universal Generator choose Level 109 and click **1. Validate**. The prior missing-source errors should be gone; W165 physical pin mapping, route/via certification warnings remain by design.
5. For a full `Level_109_Layout.prefab`, use **Tools > Project Spark > Automatic Level Generator > 109 > 1. Build FULL Level_109_Layout**, not the generic V2 `3. Build Imported` command.
6. To place the board, select `Table_top` and run **109 > 2. Place Level 109 on SELECTED tabletop**. If `Table_top` has only a Transform, the confirmation dialog uses that Transform position as the top surface center; make sure its Y is correct.

Target tabletop 15.79 x 8.54 Unity units; target board footprint 10.13 x 7.51 units. Do not parent the board under a scaled table object.

The source electrical nets, 164 routes, 54 vias and W165 physical pins are NOT CERTIFIABLE until actual schematic data is supplied. No Unity compilation or Play Mode testing is claimed here.
