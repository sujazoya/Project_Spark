#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class SparkLevel100BoardOnlyImporter
{
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/1. Build Empty DNX-8 PCB")]
    public static void BuildEmpty()
    {
        string[] paths=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("L100_DNX8_Empty_FourLayer_PCB") && p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)).ToArray();
        if(paths.Length!=1){Debug.LogError("[L100] Expected exactly one empty DNX8 board FBX; found "+paths.Length);return;}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);if(prefab==null){Debug.LogError("[L100] FBX import failed");return;}
        var root=new GameObject("Level100_DNX8_Empty_Playable_Board");
        GameObject.Instantiate(prefab,root.transform).name="EMPTY_PCB_ART_ONLY";
        Selection.activeGameObject=root;
        Debug.Log("[L100] Empty board created. 70 placements, 92 routes, 20 vias and W093 remain uncompleted. Art-only; no certification.");
    }
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/2. Validate FBX Board Import")]
    public static void Validate()
    {
        var paths=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("_L100_") && p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)).ToArray();
        Debug.Log("[L100] Found "+paths.Length+" matching modular FBX assets (expected 88); no Play Mode/electrical verification performed.");
    }
}
#endif
