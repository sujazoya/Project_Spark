#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class SparkL103BoardArtBuilder
{
    [MenuItem("Tools/Project Spark/Levels/103/1. Build EMPTY CPX-11 Board")]
    public static void BuildEmpty()
    {
        string[] guids = AssetDatabase.FindAssets("001_L103_U1 t:Model");
        // Find package root via any ModelManifest, not hardcoded Assets path.
        string root = null;
        foreach (var g in AssetDatabase.FindAssets("ModelManifest"))
        {
            string p=AssetDatabase.GUIDToAssetPath(g).Replace('\\','/');
            if (p.Contains("Level103") || p.Contains("PowerCore")) { root=Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\','/'); break; }
        }
        if (root == null) { Debug.LogError("[L103] Level 103 Data/ModelManifest.csv not found.");return; }
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/Models_FBX/089_L103_PCB_Main.fbx");
        if (!model) { Debug.LogError("[L103] Missing PCB_Main FBX. Check ModelManifest.csv.");return; }
        var parent=new GameObject("L103_CPX11_EMPTY_GAMEPLAY_ART");
        var go=(GameObject)PrefabUtility.InstantiatePrefab(model);go.transform.SetParent(parent.transform,false);
        go.name="VisualOnly_PCB_Main";
        Selection.activeGameObject=parent;
        Debug.Log("[L103] Empty board created. 88 components, 116 routes, 30 vias and W117 remain NOT installed. Attach Project Spark gameplay systems separately.");
    }
}
#endif
