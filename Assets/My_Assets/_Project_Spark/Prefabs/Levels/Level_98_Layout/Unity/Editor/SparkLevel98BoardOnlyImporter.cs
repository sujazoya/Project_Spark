#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class SparkLevel98BoardOnlyImporter
{
    [MenuItem("Tools/Project Spark/Levels/98/Board Only/1. Build EMPTY PIS-6 Board")]
    static void BuildEmpty()
    {
        var selected = Selection.activeGameObject;
        GameObject root = new GameObject("L98_PIS6_EMPTY_ArtOnly");
        if (selected != null) root.transform.SetParent(selected.transform, false);
        string[] ids = AssetDatabase.FindAssets("PIS6_Empty_FourLayer_PCB t:Model");
        if (ids.Length == 0) { Debug.LogError("[L98] PCB FBX missing. Import Models_FBX first."); UnityEngine.Object.DestroyImmediate(root); return; }
        string path=AssetDatabase.GUIDToAssetPath(ids[0]);
        GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(root.transform,false);
        instance.name="PCB_Empty_No_Components";
        Selection.activeGameObject=root;
        Debug.Log("[L98] Empty board created. Remaining 56 parts are deliberately uninstalled.");
    }

    [MenuItem("Tools/Project Spark/Levels/98/Board Only/2. Generate Native PBR Materials")]
    static void Materials()
    {
        string[] matches=AssetDatabase.FindAssets("PCB_FR4_Albedo t:Texture2D");
        if (matches.Length == 0) { Debug.LogError("[L98] Texture library missing.");return; }
        string textureFolder=Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(matches[0])).Replace('\\','/');
        string destination=textureFolder+"/Generated_Unity_Materials";
        if (!AssetDatabase.IsValidFolder(destination)) AssetDatabase.CreateFolder(textureFolder,"Generated_Unity_Materials");
        string[] names={"PCB_FR4","PCB_Soldermask","Copper","ENIG_Gold","Solder_Tin","IC_Epoxy","IC_Marking","Ceramic_Ivory","Resistor_Black","Steel","Polymer_Sleeve","Polymer_Top","Copper_Winding","Ferrite","Connector_Black","Plastic_Red","LED_Green","PCB_Silkscreen","Dielectric","Insulator","Thermal_Ceramic"};
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");
        if(!shader)shader=Shader.Find("HDRP/Lit");
        if(!shader)shader=Shader.Find("Standard");
        if(!shader) { Debug.LogError("[L98] No compatible Lit shader found.");return; }
        int created=0;
        foreach(string name in names)
        {
            string file=destination+"/L98_"+name+".mat";
            Material mat=AssetDatabase.LoadAssetAtPath<Material>(file);
            if(mat==null) { mat=new Material(shader); AssetDatabase.CreateAsset(mat,file);created++; }
            Texture2D albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(textureFolder+"/"+name+"_Albedo.png");
            Texture2D mask=AssetDatabase.LoadAssetAtPath<Texture2D>(textureFolder+"/"+name+"_UnityMetallicSmoothness.png");
            Texture2D normal=AssetDatabase.LoadAssetAtPath<Texture2D>(textureFolder+"/"+name+"_Normal.png");
            Texture2D ao=AssetDatabase.LoadAssetAtPath<Texture2D>(textureFolder+"/"+name+"_AO.png");
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",albedo);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",albedo);
            if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",mask);
            if(mat.HasProperty("_MaskMap"))mat.SetTexture("_MaskMap",mask);
            if(mat.HasProperty("_BumpMap"))mat.SetTexture("_BumpMap",normal);
            if(mat.HasProperty("_OcclusionMap"))mat.SetTexture("_OcclusionMap",ao);
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("[L98] Native materials ready ("+created+" new). Check shader mask and normal-map importer settings for your pipeline. Materials may need manual FBX remapping.");
    }
}
#endif
