#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level104.Editor {
 public static class SparkL104BoardArtBuilder {
  const string Menu="Tools/Project Spark/Levels/104/";
  static string Find(string filename) {
   foreach (string g in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename))) {
    string p=AssetDatabase.GUIDToAssetPath(g);
    if (p.EndsWith(filename, StringComparison.OrdinalIgnoreCase)) return p;
   }
   return null;
  }
  [MenuItem(Menu+"1. Build EMPTY MMX-12 Board")]
  public static void Build() {
   GameObject root=new GameObject("L104_MMX12_EmptyGameplayBoard");
   string name="095_L104_PCB_Substrate_260x185mm.fbx";
   string path=Find(name);
   if (path!=null) { var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path); if(prefab!=null){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(root.transform,false);go.name="PCB_VISUAL_ONLY";} }
   else Debug.LogError("[L104] Cannot find "+name+". Import Models_FBX folder first.");
   root.AddComponent<SparkL104VisualOnly>();
   Selection.activeGameObject=root;
   Debug.Log("[L104] Visual board created. Components, routes, vias and W125 are NOT automatically installed or certified.");
  }
  [MenuItem(Menu+"2. Show Assembled ART Reference")]
  public static void Reference() {
   string p=Find("121_L104_AssembledReference.fbx");
   if (p==null) {Debug.LogError("L104 reference not found.");return;}
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p));
   go.name="L104_REFERENCE_DO_NOT_USE_FOR_GAMEPLAY";Selection.activeGameObject=go;
  }
 }
}
#endif
