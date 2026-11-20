#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
public static class SparkL101ArtBuilder {
    [MenuItem("Tools/Project Spark/Levels/101/Build Empty MLX-9 Board")]
    static void Build() {
        string[] assets = AssetDatabase.FindAssets("L101_PCB_FR4_Core t:Model");
        if (assets.Length == 0) { Debug.LogError("[L101] PCB FBX not found. Import Models_FBX folder first."); return; }
        string path = AssetDatabase.GUIDToAssetPath(assets[0]);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!prefab) { Debug.LogError("[L101] Core FBX is not importable as GameObject."); return; }
        var root = new GameObject("L101_MLX9_Empty_Board_Art_ONLY");
        var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        child.transform.SetParent(root.transform, false);
        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Create L101 Empty Board");
        Debug.Log("[L101] Empty board art created. No components/routes/vias/W101 automatically installed. Check physical size and materials.");
    }
    [MenuItem("Tools/Project Spark/Levels/101/Validate FBX Imports")]
    static void Validate() {
        var all=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(x=>x.Contains("L101_")&&x.EndsWith(".fbx")).ToArray();
        Debug.Log("[L101] Found "+all.Length+" Level 101 FBX assets. Target = 96 modular + one reference; check scale 0.230 m x 0.160 m manually.");
    }
}
#endif
