from pathlib import Path
import csv,json,hashlib,zipfile,struct,shutil
R=Path('/mnt/data/L131_complete'); A=R/'Assets/ProjectSpark/Level131/B760MSteelLegend'; E=A/'Editor'; D=R/'Documentation';D.mkdir(exist_ok=True)
(E/'SparkL131NativeImporter.cs').write_text('''using System;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level131.Editor {
 [ScriptedImporter(1,"spark131mesh")]
 public sealed class SparkL131NativeImporter : ScriptedImporter {
  static string ReadName(BinaryReader b) { int n=b.ReadUInt16(); return Encoding.UTF8.GetString(b.ReadBytes(n)); }
  public override void OnImportAsset(AssetImportContext ctx) {
   var root=new GameObject(Path.GetFileNameWithoutExtension(ctx.assetPath));
   using(var b=new BinaryReader(File.OpenRead(ctx.assetPath))) {
    if(Encoding.ASCII.GetString(b.ReadBytes(8))!="SPK131M1") throw new InvalidDataException("Invalid native mesh signature");
    int groups=b.ReadInt32(); if(groups<1||groups>10000) throw new InvalidDataException("Invalid group count");
    for(int g=0;g<groups;g++) {
     string name=ReadName(b), material=ReadName(b); int n=b.ReadInt32(); if(n<3||n%3!=0||n>3000000) throw new InvalidDataException("Invalid vertex count");
     var vertices=new Vector3[n];var uv=new Vector2[n];var indices=new int[n];
     for(int i=0;i<n;i++) { float x=b.ReadSingle(),y=b.ReadSingle(),z=b.ReadSingle(); vertices[i]=new Vector3(-x,y,z); }
     for(int i=0;i<n;i++) uv[i]=new Vector2(b.ReadSingle(),b.ReadSingle());
     for(int i=0;i<n;i+=3) { indices[i]=i;indices[i+1]=i+2;indices[i+2]=i+1; }
     var mesh=new Mesh { name=name, indexFormat=n>65535?IndexFormat.UInt32:IndexFormat.UInt16 };mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
     var child=new GameObject(name);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");ctx.AddObjectToAsset("mesh_"+g,mesh);
    }
    if(b.BaseStream.Position!=b.BaseStream.Length) throw new InvalidDataException("Unexpected trailing bytes");
   }
   ctx.AddObjectToAsset("root",root);ctx.SetMainObject(root);
  }
 }
}
''')
# adapt existing builder, leave original menu paths and use native default
p=E/'SparkL131BoardBuilder.cs';s=p.read_text();s=s.replace('Build(false);','Build(0);').replace('Build(true);','Build(1);').replace('bool obj)','int mode)').replace('string path = Root + (obj ? "/FallbackOBJ/L131_" : "/Models/L131_") + id + (obj ? ".obj" : ".fbx");','string path = Root + (mode == 2 ? "/Models_Native/L131_" : mode == 1 ? "/FallbackOBJ/L131_" : "/Models/L131_") + id + (mode == 2 ? ".spark131mesh" : mode == 1 ? ".obj" : ".fbx");').replace('Require(rows[i].Split(\',\')[0], obj)','Require(rows[i].Split(\',\')[0], mode)').replace('if (obj)','if (mode == 1)').replace('string suffix = obj ? "_OBJ" : "_FBX";','string suffix = mode == 2 ? "_Native" : mode == 1 ? "_OBJ" : "_FBX";');s=s.replace('private static GameObject Require', '''[MenuItem("Tools/Project Spark/Automatic Level Generator/131/1. Build FULL Level 131 (Native recommended)")]
        public static void BuildNative() => Build(2);
        [MenuItem("Tools/Project Spark/Automatic Level Generator/131/2. Validate native sources")]
        public static void ValidateNative() {
            var rows = File.ReadAllLines(Root + "/placements.csv");
            for(int i=1;i<rows.Length;i++) Require(rows[i].Split(',')[0],2);
            Debug.Log("[L131] Validated " + (rows.Length-1) + " imported native assets.");
        }
        private static GameObject Require''');s=s.replace('root.transform.localScale = Vector3.one * (WorkshopWidth / importedWidth);','''SparkL131MaterialTools.ApplyTo(root, mode == 2);
                string[] targets = {"ATX12V1","ATX12V2","ATXPWR1","CPU_LGA1700","DDR5_A2","M2_1","PCIE1","BIOS_ROM","CPU_FAN1","B760_CHIPSET"};
                var diagnostics = new GameObject("DiagnosticTargets_VisualOnly"); diagnostics.transform.SetParent(root.transform,false);
                foreach(string target in targets) { var part=root.transform.Find(target); if(!part) throw new InvalidDataException("Missing diagnostic target: "+target); var anchor=new GameObject(target); anchor.transform.SetParent(diagnostics.transform,false); anchor.transform.position=BoundsOf(part.gameObject).center; }
                root.transform.localScale = Vector3.one * (WorkshopWidth / importedWidth);''');p.write_text(s)
p=E/'SparkL131MaterialTools.cs';s=p.read_text();s=s.replace('const string path =','ApplyTo(root, false);\n        }\n        public static void ApplyTo(GameObject root, bool textured) {\n            const string path =',1);s=s.replace('string file=path+"/L131_"+names[i]+".mat";','string file=path+"/L131_"+names[i]+(textured?"_PBR":"")+".mat";');s=s.replace('AssetDatabase.CreateAsset(mats[i],file); }','AssetDatabase.CreateAsset(mats[i],file); }\n                if(textured) { string tex=Path.GetDirectoryName(path).Replace(\'\\\\\',\'/\')+"/Textures/"+names[i]; mats[i].SetColor("_BaseColor",Color.white); mats[i].SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"_BaseColor.png")); mats[i].SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"_MetallicSmoothness.png")); mats[i].EnableKeyword("_METALLICSPECGLOSSMAP"); mats[i].SetFloat("_Smoothness",1f); EditorUtility.SetDirty(mats[i]); }');p.write_text(s)
# texture importer scopes the package and sets correct color space
(E/'SparkL131TextureImport.cs').write_text('''using UnityEditor;
namespace ProjectSpark.Level131.Editor {
 public sealed class SparkL131TextureImport : AssetPostprocessor {
  void OnPreprocessTexture() { if(!assetPath.StartsWith("Assets/ProjectSpark/Level131/B760MSteelLegend/Textures/")) return; var t=(TextureImporter)assetImporter; t.maxTextureSize=2048; t.mipmapEnabled=true; t.sRGBTexture=assetPath.EndsWith("_BaseColor.png"); if(assetPath.EndsWith("_Normal.png")) t.textureType=TextureImporterType.NormalMap; if(assetPath.EndsWith("_MetallicSmoothness.png")) t.alphaSource=TextureImporterAlphaSource.FromInput; }
 }
}
''')
# preview prefab independent of scene
(E/'SparkL131Showcase.cs').write_text('''using UnityEditor;
using UnityEngine;
using System.IO;
namespace ProjectSpark.Level131.Editor {
 public static class SparkL131Showcase {
  const string Root="Assets/ProjectSpark/Level131/B760MSteelLegend";
  [MenuItem("Tools/Project Spark/Automatic Level Generator/131/3. Create assembled preview prefab")]
  static void Assembled()=>Create("ASSEMBLED");
  [MenuItem("Tools/Project Spark/Automatic Level Generator/131/4. Create batched preview prefab")]
  static void Batched()=>Create("BATCHED");
  static void Create(string id) { var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models_Native/L131_"+id+".spark131mesh"); if(!source) throw new InvalidDataException("Reimport native meshes first"); var go=(GameObject)PrefabUtility.InstantiatePrefab(source); try { SparkL131MaterialTools.ApplyTo(go,true); if(!AssetDatabase.IsValidFolder(Root+"/Generated")) AssetDatabase.CreateFolder(Root,"Generated"); PrefabUtility.SaveAsPrefabAsset(go,Root+"/Generated/L131_"+id+".prefab"); } finally { Object.DestroyImmediate(go); } }
 }
}
''')
rows=list(csv.DictReader((A/'placements.csv').open())); targets=['ATX12V1','ATX12V2','ATXPWR1','CPU_LGA1700','DDR5_A2','M2_1','PCIE1','BIOS_ROM','CPU_FAN1','B760_CHIPSET']; assert all(x in {r['id'] for r in rows} for x in targets)
(A/'DiagnosticTargets.json').write_text(json.dumps({'visual_only':True,'targets':targets},indent=2));(A/'Mission.json').write_text(json.dumps({'level':131,'title':'Steel Legend Qualification','status':'authoring only','objectives':['Inspect board and inventory','Isolate dual EPS power fault','Qualify DDR5 training','Check PCIe and NVMe enumeration','Check cooling and fan feedback','Perform cold start audit'],'faults':[{'target':'ATX12V2','case':'simulated seating fault'},{'target':'DDR5_A2','case':'simulated training fault'},{'target':'M2_1','case':'simulated enumeration fault'}]},indent=2))
installer=Path('/mnt/data/Install_Level130_AAA_AutoLayout(1).ps1').read_text(encoding='utf-8-sig').replace('130','131').replace('H610ME','B760MSteelLegend').replace('Models_FBX','Models').replace('458','131').replace('Data\\ModelManifest.csv','placements.csv').replace('456','129');(R/'Install_Level131_AAA_AutoLayout.ps1').write_text(installer,encoding='utf-8-sig')
(D/'Master_Unity_Implementation_Prompt.md').write_text('''# Level 131 — Steel Legend Qualification\nUse the ASRock B760M Steel Legend WiFi visual authoring assets. Keep all prior levels unchanged. Unity 6 URP, PC/mobile. Build using native mesh import first. PCB 244 x 244 mm; workshop PCB 7.51 x 7.51 units on 15.79 x 8.54 tabletop.\n\nConnect the six objectives in Mission.json to LevelGamePlayManager, SparkSelectionController, existing multimeter, SparkCircuitSystem and verified solver APIs. Diagnostic targets are visual mesh anchors only; never treat these as electrical pins or fabricated measurement evidence. Use approved compatible virtual CPU, memory, storage and cooling models, safe virtual power isolation, revision-aware evidence, and cold restart validation. Do not distribute firmware. Profile mobile and desktop, test import and compilation, verify clearances and board layout against official manual. This package does not supply actual circuit connectivity or scoring integration.\n''')
# offline checks of all native bytes, cross counts and GLB header
native=[]
for p in sorted((A/'Models_Native').glob('*.spark131mesh')):
 b=p.read_bytes();assert b[:8]==b'SPK131M1';g=struct.unpack_from('<i',b,8)[0];pos=12;tri=0
 for _ in range(g):
  for j in range(2):n=struct.unpack_from('<H',b,pos)[0];pos+=2+n
  n=struct.unpack_from('<i',b,pos)[0];pos+=4;assert n>0 and n%3==0;pos+=n*20;tri+=n//3
 assert pos==len(b);native.append({'file':p.name,'groups':g,'triangles':tri})
assert len(native)==131;assert sum(int(r['triangles']) for r in rows)==85252;assert len(list((A/'Models').glob('*.fbx')))==131;assert len(list((A/'Textures').glob('*.png')))==36;assert (A/'L131_ASSEMBLED.glb').read_bytes()[:4]==b'glTF'
(D/'OfflineValidation.json').write_text(json.dumps({'modular_parts':129,'fbx_files':131,'native_files':131,'native_structure':'PASS','native_details':native,'texture_maps':36,'texture_dimensions':[2048,2048],'triangles':85252,'unity_import':'NOT_RUN','powershell':'NOT_RUN','electrical_integration':'NOT_IMPLEMENTED','geometry':'existing Level131 procedural geometry, not manufacturer CAD','fbx':'original structural checks only; batch exported with same writer'},indent=2))
(R/'README_INSTALL_FIRST.md').write_text('''# Project Spark Level 131 — ASRock B760M Steel Legend WiFi\nExpanded visual authoring kit, not a manufacturer-exact or certified AAA replica. 129 modular components, 131 FBX, 131 native mesh assets, 129 OBJ, assembled GLB, 36 procedural 2K maps (flat normal maps), 10 diagnostic visual targets and six mission objectives in data. FBX has no UVs; use native import for textured workshop. GLB has UVs and material factors, not embedded PNGs. PCB art is illustrative, including mirrored backside reuse. Geometry retained from initial Level131; electrical connectivity not provided.\n\n## Install\nClose Unity and back up project. Extract ZIP outside Assets. Run from extracted directory in Windows PowerShell:\n```powershell\npowershell -ExecutionPolicy Bypass -File ".\\Install_Level131_AAA_AutoLayout.ps1" -UnityProjectPath "D:\\Project_Spark_Master_Project\\Project_Spark" -DryRun\npowershell -ExecutionPolicy Bypass -File ".\\Install_Level131_AAA_AutoLayout.ps1" -UnityProjectPath "D:\\Project_Spark_Master_Project\\Project_Spark"\n```\nIf replacing the initial Level131 kit, explicitly add `-ReplaceExisting` to BOTH commands. Installer backs up differing files and does not touch previous levels.\n\nOpen Unity. After scripts compile, reimport Models_Native and Textures if needed. Use Tools > Project Spark > Automatic Level Generator > 131 > 2. Validate native sources, then 1. Build FULL Level 131 (Native recommended). Generated scene and prefab live inside the Level131 Generated folder. Existing FBX and OBJ menus remain available. Preview prefab menus use native sources. Check orientation, collision/clearance and visuals; native meshes are artwork, not electrical authority. Unity compilation, actual import, Play Mode and Windows installer execution not tested here.\n''')
shutil.copy('/tmp/expand131.py',R/'expand_source.py');shutil.copy('/tmp/finish131.py',R/'integration_source.py')
with (R/'SHA256SUMS.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['relative_path','sha256']);
 for p in sorted(A.rglob('*')):
  if p.is_file():w.writerow([p.relative_to(A).as_posix(),hashlib.sha256(p.read_bytes()).hexdigest()])
out=Path('/mnt/data/ProjectSpark_Level131_ASRock_B760M_SteelLegend_COMPLETE_HD_Kit.zip')
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,compresslevel=5) as z:
 for p in sorted(R.rglob('*')):
  if p.is_file():z.write(p,p.relative_to(R))
with zipfile.ZipFile(out) as z:assert z.testzip() is None;print(out,len(z.namelist()),out.stat().st_size)
