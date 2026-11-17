#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class SparkLevel99BoardOnlyArtBuilder
{
    private static GameObject FindModel(string stem)
    {
        var ids=AssetDatabase.FindAssets(stem+" t:Model");
        foreach(string id in ids)
        {
            string p=AssetDatabase.GUIDToAssetPath(id);
            if(p.EndsWith(stem+".fbx")) return AssetDatabase.LoadAssetAtPath<GameObject>(p);
        }
        return null;
    }
    [MenuItem("Tools/Project Spark/Levels/99/Board Only/1. Create EMPTY SBC-7 Board")]
    public static void BuildEmpty()
    {
        GameObject board=FindModel("63_L99_SBC7_Empty_FourLayer_PCB");
        if (!board) {Debug.LogError("[L99] Missing empty PCB FBX. Import Models_FBX first.");return;}
        GameObject root=new GameObject("L99_SBC7_BoardOnly_ArtRoot");
        if(Selection.activeTransform)root.transform.SetParent(Selection.activeTransform,false);
        GameObject visual=(GameObject)PrefabUtility.InstantiatePrefab(board);
        visual.name="Empty_PCB_ArtOnly";
        visual.transform.SetParent(root.transform,false);
        Selection.activeGameObject=root;
        Debug.Log("[L99] Empty board. 62 components, 82 traces, 18 vias and W083 are NOT installed. The visual FBX cannot certify gameplay.");
    }
    [MenuItem("Tools/Project Spark/Levels/99/Board Only/2. Validate 80 FBX Presence")]
    public static void Validate()
    {
        int count=0;
        foreach(string guid in AssetDatabase.FindAssets("t:Model"))
        {
            var path=AssetDatabase.GUIDToAssetPath(guid);
            if(path.EndsWith(".fbx") && Path.GetFileName(path).Contains("_L99_"))count++;
        }
        Debug.Log("[L99] Found "+count+" modular FBX art files. Expected 80. This is an ART ONLY check.");
    }
}
#endif
