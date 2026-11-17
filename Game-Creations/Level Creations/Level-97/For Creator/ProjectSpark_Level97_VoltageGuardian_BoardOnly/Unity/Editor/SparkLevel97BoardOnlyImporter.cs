#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class SparkLevel97BoardOnlyImporter
{
    const string MenuBase = "Tools/Project Spark/Levels/97/Board Only/";
    static string FindRoot()
    {
        var guids=AssetDatabase.FindAssets("t:TextAsset Asset_Manifest_68");
        foreach(var guid in guids)
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if(path.EndsWith("Asset_Manifest_68.csv",StringComparison.OrdinalIgnoreCase))
                return Path.GetDirectoryName(Path.GetDirectoryName(path)).Replace('\\','/');
        }
        // avoid assuming a hard-coded install directory
        return null;
    }
    [MenuItem(MenuBase + "1. Create EMPTY VGR-5 PCB")]
    public static void BuildEmpty()
    {
        var root=FindRoot(); if(string.IsNullOrEmpty(root)){Debug.LogError("Level 97 manifest missing. Import entire kit under Assets first.");return;}
        string model=root+"/Models_FBX/00_L97_VGR5_Empty_PCB_160x110mm.fbx";
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(model);
        if(!prefab){Debug.LogError("FBX not imported: "+model);return;}
        GameObject parent=new GameObject("L97_VGR5_GAMEPLAY_EMPTY");
        var board=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        board.name="Physical_Empty_PCB_Factory_Pads_Only";
        board.transform.SetParent(parent.transform,false);
        Selection.activeGameObject=parent;
        Debug.Log("Level 97 EMPTY board spawned. No components, player routes, vias, or W067 are certified. Setup real components via your PCB framework.");
    }
    [MenuItem(MenuBase + "2. Create VISUAL REFERENCE (not gameplay)")]
    public static void BuildReference()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/Models_FBX/L97_COMPLETE_ASSEMBLED_VISUAL_REFERENCE_ONLY.fbx");
        if(!prefab){Debug.LogError("Showcase FBX missing");return;}
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name="L97_VISUAL_REFERENCE_ONLY_DO_NOT_CERTIFY";
        foreach(var c in go.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
        Selection.activeGameObject=go;
    }
    [MenuItem(MenuBase + "3. Create Native PBR Materials")]
    public static void Materials()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        string output=root+"/GeneratedMaterials";if(!AssetDatabase.IsValidFolder(output))AssetDatabase.CreateFolder(root,"GeneratedMaterials");
        var texGuids=AssetDatabase.FindAssets("t:Texture2D",new[]{root+"/Textures"});
        var texPaths=texGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach(string albedo in texPaths.Where(x=>x.EndsWith("_BaseColor.png",StringComparison.OrdinalIgnoreCase)))
        {
            string matName=Path.GetFileNameWithoutExtension(albedo).Replace("_BaseColor","");
            string assetPath=output+"/"+matName+".mat";
            var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if(!shader){Debug.LogError("No Lit shader found");return;}
            Material mat=AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,assetPath);}
            else mat.shader=shader;
            var baseTex=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",baseTex);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",baseTex);
            var normalPath=albedo.Replace("_BaseColor.png","_Normal.png");
            var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if(normal)
            {
                var importer=AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if(importer && importer.textureType!=TextureImporterType.NormalMap){importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();normal=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);}
                if(mat.HasProperty("_BumpMap"))mat.SetTexture("_BumpMap",normal);
                if(mat.HasProperty("_NormalMap"))mat.SetTexture("_NormalMap",normal);
                mat.EnableKeyword("_NORMALMAP");
            }
            var ao=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo.Replace("_BaseColor.png","_AO.png"));
            if(ao && mat.HasProperty("_OcclusionMap"))mat.SetTexture("_OcclusionMap",ao);
            var emission=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo.Replace("_BaseColor.png","_Emission.png"));
            if(emission && matName.StartsWith("LED_"))
            {
                if(mat.HasProperty("_EmissionMap"))mat.SetTexture("_EmissionMap",emission);
                if(mat.HasProperty("_EmissionColor"))mat.SetColor("_EmissionColor",Color.white);
                mat.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("Level 97 native materials created. Assign to matching FBX material slots by name; verify normal maps and metallic/smoothness masks in active pipeline.");
    }
    [MenuItem(MenuBase + "4. Validate Imported Art")]
    public static void ValidateArt()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        int found=0;foreach(string g in AssetDatabase.FindAssets("t:Model",new[]{root+"/Models_FBX"}))
        {if(AssetDatabase.GUIDToAssetPath(g).EndsWith(".fbx"))found++;}
        Debug.Log("Level 97 model assets found: "+found+" expected 69 including showcase. This checks art only, NOT live PCB electrical correctness.");
    }
}
#endif
