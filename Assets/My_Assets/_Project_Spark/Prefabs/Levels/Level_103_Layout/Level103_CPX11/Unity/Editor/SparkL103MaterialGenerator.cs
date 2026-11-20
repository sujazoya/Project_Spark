#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
public static class SparkL103MaterialGenerator
{
 [MenuItem("Tools/Project Spark/Levels/103/2. Create Starter Materials")]
 public static void Build()
 {
  string folder=null;
  foreach (var guid in AssetDatabase.FindAssets("PCB_Navy_BaseColor t:Texture2D"))
  {
   var path=AssetDatabase.GUIDToAssetPath(guid).Replace('\\','/');
   if(path.Contains("Level103") || path.Contains("PowerCore")) {folder=Path.GetDirectoryName(path).Replace('\\','/');break;}
  }
  if(folder==null){Debug.LogError("[L103] Texture folder not found. Keep Textures with this package.");return;}
  var dest=Path.GetDirectoryName(folder).Replace('\\','/')+"/UnityMaterials";
  if(!AssetDatabase.IsValidFolder(dest))AssetDatabase.CreateFolder(Path.GetDirectoryName(dest).Replace('\\','/'),"UnityMaterials");
  string[] names={"PCB_Navy","PCB_Fiberglass","Copper","Gold_ENIG","Solder_Tin","IC_Mold","IC_LaserMark","Ceramic","Resistor_Black","Cap_Aluminum","Cap_Top","Ferrite","Plastic_Black","Plastic_Grey","Copper_Winding","Inductor_Core","LED_Green","LED_Amber","Connector_Metal","Warning_Red","Thermal_Pad","Rubber","Glass"};
  foreach(var n in names)
  {
   Shader shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard");
   if(shader==null){Debug.LogError("[L103] Compatible shader not found");return;}
   Material mat=new Material(shader){name=n};
   var col=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+n+"_BaseColor.png");
   if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",col);
   if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",col);
   var save=dest+"/"+n+".mat";
   if(AssetDatabase.LoadAssetAtPath<Material>(save)==null)AssetDatabase.CreateAsset(mat,save);else Object.DestroyImmediate(mat);
  }
  AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
  Debug.Log("[L103] Starter materials generated. FBX slot remapping and pipeline-specific normal/metallic packing require review.");
 }
}
#endif
