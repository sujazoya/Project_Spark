PROJECT SPARK / LEVEL 113 / RDX-21 CSV PARSER HOTFIX

ISSUE:
The original SparkL113FullLayoutBuilder.cs calls lines[i].Split(',') for
CSV parsing. Eight model descriptions, J1-J8 (manifest records 125-132,
CSV physical lines 126-133), contain commas inside quoted fields. Naive
splitting shifts the obj field to NOT_INCLUDED (the FBX placeholder).

VERIFIED ORIGINAL ZIP:
- Manifest records: 206
- Model OBJ assets referenced: 206 / 206 present
- Assembled reference OBJ: 207_L113_AssembledReference.obj present
- Other data CSV record counts: BOM 166; placements 166; routes 232; vias 76

RECOMMENDED: STANDALONE POWERSHELL HOTFIX
1. Close Unity.
2. Put Fix_Level113_RDX21_CSV_Standalone.ps1 in any local folder, e.g.:
   D:\Project_Spark_Master_Project\Project_Spark\Game-Creations\Level Creations\Level-113\Generator
3. In PowerShell 5.1, run:
   powershell -ExecutionPolicy Bypass -File .\Fix_Level113_RDX21_CSV_Standalone.ps1 -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"
4. For a check without changes, add -DryRun.
5. Reopen Unity and run:
   Tools > Project Spark > Automatic Level Generator > 113 > 3. Validate Sources and Report Blockers
6. Then run: 113 > 1. Build FULL Level_113_Layout.

The hotfix works directly on the existing Unity builder. It does not need a
second .cs or .csv beside the .ps1 and backs up the original builder.

ALTERNATIVE: FULL CORRECTED KIT
The FULL fixed archive contains the entire original kit with ONLY the
layout builder C# parser corrected (plus this README). To install, first
extract the full archive, then run Install_Level113_AAA_AutoLayout.ps1 from
the extracted folder. Do not install the old original package afterward,
or it could overwrite the corrected builder.

NOTE: The CSV fix does not certify any electrical gameplay or resolve
possible Unity OBJ import geometry/scale issues. Routes, vias, and W233 still
require a verified schematic and project-side integration.
