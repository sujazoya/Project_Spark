# Project Spark Level 118 TSX26 — Route Count Validation Hotfix

**Fix:** SparkL118FullLayoutBuilder.cs incorrectly tests for 340 routes. The original
CopperRoutes_320_REQUIRES_SCHEMATIC.csv correctly has 320 records.
The fix changes only `routes.Count != 340` to `routes.Count != 320`.

## Standalone installation (recommended for an existing imported Level 118)

Close Unity, extract the standalone hotfix ZIP, and run from PowerShell 5.1:

```powershell
powershell -ExecutionPolicy Bypass -File ".\Fix_Level118_TSX26_RouteCount_Standalone.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"
```

Run with `-DryRun` to check the files without changing them. The hotfix
validates the five source CSV counts, makes a timestamped backup under
`ProjectSpark_Backups`, patches the installed C# builder, and verifies the write.
It does not modify Unity scenes, .meta files, models, CSV data, or other levels.

## Full corrected kit

The FULL ZIP is the original package with the identical one-line builder fix.
Extract and follow the included original installer instructions. For an already
installed Level 118, the standalone patch avoids copying other files.

## Unity validation

Tools > Project Spark > Automatic Level Generator > 118 >
3. Validate Sources and Report Blockers

Then run 1. Build FULL Level_118_Layout. The builder's visual scene generation,
URP material loading and electrical integration still require testing in Unity.
Source routes/vias have unassigned netlists; no electrical certification is implied.
