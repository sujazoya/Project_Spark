# Level 128 native mesh import hotfix

Requires the installed original Level 128 X870E package. Close Unity and back up your project. Extract this ZIP outside Assets. Run the installer first with -DryRun, then without it.

```powershell
powershell -ExecutionPolicy Bypass -File ".\Install_Level128_NativeMeshHotfix.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun
```

Remove -DryRun for installation. In Unity, run menu 128 / 3. Validate Sources and Report Blockers, 4. Rebuild URP PBR Materials, 5. Create COMPLETE assembled motherboard, then 1. Build FULL Level_128_Layout. The original FBX files remain unchanged and can still emit import warnings; the builder uses the independent native meshes. No electrical mappings or gameplay certifications are changed. Unity and Windows PowerShell were not run in this environment.
