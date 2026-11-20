#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class SparkLevel99PBRMaterialInstaller
{
 [MenuItem("Tools/Project Spark/Levels/99/Board Only/3. Create Native PBR Materials")]
 static void Install()
 {
   var ids=AssetDatabase.FindAssets("PCB_FR4_Albedo t:Texture2D");
   if(ids.Length==0){Debug.LogError("[L99] Shared PBR textures not found.");return;}
   string folder=Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(ids[0])).Replace('\\','/');
   string destination=folder+"/Generated_Unity_Materials";
   if(!AssetDatabase.IsValidFolder(destination))AssetDatabase.CreateFolder(folder,"Generated_Unity_Materials");
   Shader sh=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
   if(!sh){Debug.LogError("[L99] No supported Lit shader found.");return;}
   string[] names={"PCB_FR4","PCB_Soldermask","Copper","ENIG_Gold","Solder_Tin","IC_Epoxy","IC_Marking","Ceramic_Ivory","Resistor_Black","Steel","Polymer_Sleeve","Polymer_Top","Copper_Winding","Ferrite","Connector_Black","Plastic_Red","LED_Green","PCB_Silkscreen","Dielectric","Insulator","Thermal_Ceramic"};
   foreach(string name in names)
   {
     string path=destination+"/L99_"+name+".mat";
     Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
     if(mat==null){mat=new Material(sh);AssetDatabase.CreateAsset(mat,path);}
     Texture2D alb=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_Albedo.png");
     Texture2D mask=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_UnityMetallicSmoothness.png");
     Texture2D ao=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_AO.png");
     if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",alb);
     if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",alb);
     if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",mask);
     if(mat.HasProperty("_MaskMap"))mat.SetTexture("_MaskMap",mask);
     if(mat.HasProperty("_OcclusionMap"))mat.SetTexture("_OcclusionMap",ao);
     EditorUtility.SetDirty(mat);
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();
   Debug.Log("[L99] Created native PBR material assets. Assign/remap FBX material slots; verify URP/HDRP shader maps and imported normal settings.");
 }
}
#endif
