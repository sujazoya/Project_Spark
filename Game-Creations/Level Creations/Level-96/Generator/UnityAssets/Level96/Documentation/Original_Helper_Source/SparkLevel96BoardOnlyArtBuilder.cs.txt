#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level96.BoardOnly
{
 public static class SparkLevel96BoardOnlyArtBuilder
 {
  const string Menu="Tools/Project Spark/Levels/96/Board Only/";
  static string FindPrefab(string prefix) {
    foreach(var guid in AssetDatabase.FindAssets("t:Model")) {
      var p=AssetDatabase.GUIDToAssetPath(guid);
      if(p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase) && System.IO.Path.GetFileNameWithoutExtension(p).Contains(prefix)) return p;
    }
    return null;
  }
  [MenuItem(Menu+"1. Create Empty SMX-4 Board")]
  public static void CreateEmpty() {
   var target=Selection.activeGameObject;
   if(!target){Debug.LogError("[L96] Select a parent GameObject.");return;}
   var path=FindPrefab("L96_SMX4_PCB_140x95mm_Empty");
   if(string.IsNullOrEmpty(path)){Debug.LogError("[L96] PCB FBX missing.");return;}
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,target.transform);
   Undo.RegisterCreatedObjectUndo(go,"Create L96 empty board");go.name="L96_SMX4_Empty_PCB_ART_ONLY";
   go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
   Selection.activeGameObject=go;
   Debug.Log("[L96] Empty PCB created. 44 components, 58 routes, 12 vias, W059 NOT INSTALLED.");
  }
  [MenuItem(Menu+"2. Create Completed Reference (Visual Only)")]
  public static void Reference() {
   var target=Selection.activeGameObject;
   if(!target){Debug.LogError("Select a parent GameObject.");return;}
   var path=FindPrefab("L96_COMPLETE_REFERENCE_VISUAL_ONLY");
   if(string.IsNullOrEmpty(path)){Debug.LogError("Reference model missing");return;}
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),target.transform);
   Undo.RegisterCreatedObjectUndo(go,"Create L96 reference art");go.name="L96_REFERENCE_ONLY_DO_NOT_CERTIFY";
   go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;
   Debug.Log("[L96] Reference ART ONLY. Not a valid gameplay build.");
  }
 }
}
#endif
