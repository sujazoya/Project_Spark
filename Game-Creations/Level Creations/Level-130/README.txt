LEVEL 130 H610ME NATIVE MESH HOTFIX

Close Unity. Extract all files from this ZIP, including Payload and the two
RequireModel text files. From the extracted directory run:
powershell -ExecutionPolicy Bypass -File .\Fix_Level130_NativeMesh.ps1 -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark" -DryRun
Then repeat without -DryRun. The script backs up the builder and any replaced
native payload files outside Assets. It leaves original FBX, textures, CSV,
scenes and .meta files unchanged. It refuses to patch a different RequireModel.

Open Unity; let scripts compile and assets import. If required, right-click
Assets/ProjectSpark/Level130/H610ME/Models_Native and choose Reimport. Run:
Tools > Project Spark > Automatic Level Generator > 130 > 3. Validate Sources and Report Blockers
Then run 1. Build FULL Level_130_Layout. Existing generated scene overwrite
confirmation remains in force. Do not overwrite a scene with unsaved work.

This is an alternative importer, not an FBX file repair. Original FBX import
errors may still appear if Unity reimports those files. Level 130 now uses
native meshes exclusively for consistent coordinates. Geometry was extracted
from all original FBX files, retaining hierarchy, translations, triangle
winding, source normals, UVs and material group names. Millimetres converted
to metres. Tangents recalculated in Unity. URP materials are bound by the
original builder. Native asset previews may look untextured before binding.
Board bounds check remains in force. Geometry audit: 458 models, 240180
triangles total including both assembled variants, 1947 mesh groups. Actual
Unity compilation, asset import, PowerShell execution, rendering and Play Mode
have not been tested here. Electrical certification remains blocked.
