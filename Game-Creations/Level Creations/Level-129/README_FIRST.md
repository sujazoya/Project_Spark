# Level 129 native-mesh import hotfix

The original FBX contains mesh records but Unity reported no imported mesh. The exact FBX importer rejection has not been established. This patch reconstructs the source geometry directly into Unity-native Mesh subassets via a level-specific ScriptedImporter. It retains original FBX files, material textures, CSVs and all other levels. It is a workaround, not a claim that the original FBX importer has been repaired.

1. Close Unity; back up your project, including any edits to SparkL129BoardBuilder.cs.
2. Extract this ZIP outside Assets, preserving directories.
3. Run `powershell -ExecutionPolicy Bypass -File ".\Install_Level129_NativeMeshHotfix.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun`.
4. Repeat without `-DryRun`.
5. Open Unity and wait for script compilation and imports. Select `Tools > Project Spark > Automatic Level Generator > 129 > 3. Validate Sources and Report Blockers`. Then run `4. Rebuild URP PBR Materials`, `5. Create COMPLETE assembled motherboard (VISUAL ONLY)`, and `1. Build FULL Level_129_Layout`.

If a native asset has not imported after compilation, select Models_Native in the Project window and choose Reimport. Inspect the Console for compiler/importer errors. The builder still requires valid nonempty meshes and checks PCB bounds (0.200 x 0.23616 m). No generated scene is overwritten by the installer, but rebuilding in Unity may replace generated Level 129 assets; preserve any edits first.

458 native model files, 2 C# files, hash verification, offline geometry readback. Unity compilation/import and Play Mode not run. Original FBX importer warnings may remain; the patched builder no longer depends on its results. This is visual authoring only.
