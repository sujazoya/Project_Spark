#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level96.BoardOnly
{
 public static class SparkLevel96PBRMaterialInstaller
 {
  [MenuItem("Tools/Project Spark/Levels/96/Board Only/3. Generate Native PBR Materials")]
  public static void Generate()
  {
   var folders=AssetDatabase.FindAssets("SMX4_Top_Silkscreen_FactoryOnly t:Texture2D");
   if(folders.Length==0){Debug.LogError("[L96] Import Textures folder first.");return;}
   string basePath=Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(folders[0]));
   string output=basePath+"/Generated_Materials";
   if(!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(basePath,"Generated_Materials");
   var shader=Shader.Find("Universal Render Pipeline/Lit");
   bool urp=shader!=null;
   if(!urp)shader=Shader.Find("HDRP/Lit");
   bool hdrp=!urp && shader!=null;
   if(shader==null) shader=Shader.Find("Standard");
   if(shader==null){Debug.LogError("[L96] No compatible Lit/Standard shader found.");return;}
   string[] families={"FR4","Mask","Gold","Copper","Solder","Chip","Ceramic","ResBody","Connector","White","RedPlastic","LEDGreen","Ink","Glass"};
   int created=0;
   foreach(var family in families)
   {
    var m=new Material(shader){name="L96_"+family};
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(basePath+"/"+family+"_Albedo.png");
    var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(basePath+"/"+family+"_Normal.png");
    var mr=AssetDatabase.LoadAssetAtPath<Texture2D>(basePath+"/"+family+"_MetallicRoughness.png");
    if(tex!=null){if(urp||hdrp)m.SetTexture("_BaseColorMap",tex); if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",tex); if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",tex);}
    if(normal!=null){if(m.HasProperty("_BumpMap"))m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
    // Reference map is glTF-style ORM (G=roughness, B=metallic): conversion to
    // pipeline-specific metallic/smoothness packing is required before using it as a mask.
    if(mr!=null) Debug.Log("[L96] ORM texture available for "+family+"; configure metallic/smoothness packing for chosen pipeline.");
    var path=output+"/L96_"+family+".mat";
    var old=AssetDatabase.LoadAssetAtPath<Material>(path);
    if(old!=null){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);}
    else{AssetDatabase.CreateAsset(m,path);created++;}
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();
   Debug.Log("[L96] Generated/updated PBR material files. New: "+created+". Remap model slots by name using Unity Model Importer Materials tab.");
  }
 }
}
#endif
