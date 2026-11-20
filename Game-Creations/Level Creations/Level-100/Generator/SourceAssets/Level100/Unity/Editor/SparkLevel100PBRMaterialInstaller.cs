#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class SparkLevel100PBRMaterialInstaller
{
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/3. Generate Native PBR Materials")]
    public static void Build()
    {
        var texPaths=AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("/Textures/") && p.EndsWith("_Albedo.png",StringComparison.OrdinalIgnoreCase)).ToArray();
        var own=texPaths.Where(p=>p.Contains("Level100_DNX8") || p.Contains("ProjectSpark_Level100_DigitalNexus_BoardOnly_Realistic")).ToArray();
        if(own.Length==0){Debug.LogError("[L100] Textures not found. Import the whole archive together, and retain source root folder.");return;}
        string root=own[0].Substring(0,own[0].LastIndexOf("/Textures/",StringComparison.Ordinal));
        string folder=root+"/UnityGeneratedMaterials";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(root,"UnityGeneratedMaterials");
        Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("HDRP/Lit")??Shader.Find("Standard");
        if(shader==null){Debug.LogError("[L100] No supported Lit shader was found");return;}
        foreach(var albedoPath in own)
        {
            var name=Path.GetFileName(albedoPath).Replace("_Albedo.png","");
            var mat=new Material(shader){name="L100_"+name};
            var t=AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",t);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",t);
            var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Textures/"+name+"_UnityMetallicSmoothness.png");
            if(mask!=null){if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",mask);if(mat.HasProperty("_MaskMap"))mat.SetTexture("_MaskMap",mask);}
            string dest=folder+"/L100_"+name+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(dest)==null)AssetDatabase.CreateAsset(mat,dest);else UnityEngine.Object.DestroyImmediate(mat);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("[L100] Native materials generated; confirm FBX material remapping and render pipeline normal-map compatibility manually.");
    }
}
#endif
