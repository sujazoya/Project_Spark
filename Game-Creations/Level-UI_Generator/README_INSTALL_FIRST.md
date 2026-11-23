# Project Spark V8 Neon — consolidated installation repair

This combines the ten previously distributed source packages (Visual Architect through V8 Neon) in release order, keeping the newest version of each exact path. 112 payload files, 32 C# files and 76 artwork inputs. No earlier UI package needs to be installed first. Requires the existing Project Spark game project, Unity 6 URP, TMP/uGUI and Input System. Does not supply or replace your game manager, campaign/level definitions, scene assets or save data. Compilation against your local project is not tested.

Close Unity and back up or commit your project first. Extract outside Assets. Use a fresh folder. Do not mix this installer or SHA256SUMS.csv with older packages.

```powershell
powershell -ExecutionPolicy Bypass -File ".\Install_Spark_V8_Neon_Complete.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun -ReplaceExisting
powershell -ExecutionPolicy Bypass -File ".\Install_Spark_V8_Neon_Complete.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -ReplaceExisting
```
Review changes before the second command. Replacement is explicit, backs up existing differing files under SparkV8Neon_CompleteRepair_Backups, and leaves .meta files untouched. Matching files are skipped. No generated assets or settings are deleted. Alternate same-name C# files under Assets block installation; review their actual classes and paths before resolving any duplicate. No changes are made in dry run. Scripts are merged as supplied, not newly compiled.

After Unity compiles without errors, run the following menus under Tools > Project Spark:
1. AAA Studio V5 > 02 Build Studio skin only (legacy fallback skin, chapter icons and settings).
2. AAA Showcase V6 > 01 Build showcase presentation (legacy fallback skin).
3. AAA Professional V8 > 01 Build complete V8 presentation (original V8 fallback sprites and status icons).
4. AAA V8 Neon > 01 Build neon presentation.
5. AAA V8 Neon > 03 Validate neon assets. Check configuration under 02 Configure neon colors: Neon Enabled, Colorful Chapters, Colorful Missions should be on; use intensity 0.88 if desired. Existing settings are retained except original first-build defaults. A custom environment override can hide the neon backdrop.
6. Enter Play Mode and open Chapters using the existing native campaign panel. Do not launch the separate AAA preview scene as a replacement for your gameplay system.

For missing live 3D previews only: run AAA Level Architect audit, approve correct hardware in AAA Level Visual Architect, then AAA Cinematic V3 > 01 Build complete V3. Approve the dedicated preview layer only if appropriate. This step is optional for static UI display and can update generated preview assets. Existing mission artwork is retained.

If you see compilation errors, share the first red error and file/line. No need to reinstall the old packages.
