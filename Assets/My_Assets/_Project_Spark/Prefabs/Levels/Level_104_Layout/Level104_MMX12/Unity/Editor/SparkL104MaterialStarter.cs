#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level104.Editor {
 public static class SparkL104MaterialStarter {
  [MenuItem("Tools/Project Spark/Levels/104/3. Create PBR Starter Materials")]
  static void Build() {
   string[] ids={"pcb","mask","copper","gold","silver","tin","black","chip","ceramic","resistor","green","red","white","core","dielectric","solder"};
   string texRoot=null;
   foreach(var guid in AssetDatabase.FindAssets("pcb_Albedo t:Texture2D")) {
    var p=AssetDatabase.GUIDToAssetPath(guid);
    if(p.Contains("Level104") || p.Contains("MemoryMatrix")){texRoot=Path.GetDirectoryName(p).Replace('\\','/');break;}
   }
   if (texRoot==null){Debug.LogError("[L104] Missing textures. Import complete kit, then retry.");return;}
   var outDir=Path.GetDirectoryName(texRoot).Replace('\\','/')+"/UnityGeneratedMaterials";
   if(!AssetDatabase.IsValidFolder(outDir))AssetDatabase.CreateFolder(Path.GetDirectoryName(outDir).Replace('\\','/'),"UnityGeneratedMaterials");
   foreach(var id in ids){
    var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    if(shader==null)continue;
    var mat=new Material(shader); mat.name="L104_"+id;
    var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(texRoot+"/"+id+"_Albedo.png");
    if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",albedo);
    if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",albedo);
    if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",(id=="gold"||id=="silver"||id=="tin"||id=="copper"||id=="solder") ? 0.85f : 0f);
    if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",0.48f);
    AssetDatabase.CreateAsset(mat,outDir+"/L104_"+id+".mat");
   }
   AssetDatabase.SaveAssets();Debug.Log("[L104] Starter materials made. Verify and remap FBX material slots manually; metallic/roughness maps require pipeline-specific packing.");
  }
 }
}
#endif
