#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level102.Editor
{
    public static class SparkL102BoardBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/102/1. Build EMPTY HSX-10 Board")]
        public static void BuildEmpty()
        {
            string[] matches = AssetDatabase.FindAssets("t:Model 083_L102_PCB_Base");
            if (matches.Length == 0) { Debug.LogError("[L102] Import the FBX Models_FBX directory first."); return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(matches[0]));
            if (!prefab) { Debug.LogError("[L102] Board FBX not found."); return; }
            var root = new GameObject("L102_HSX10_EMPTY_GAMEPLAY_ART");
            Undo.RegisterCreatedObjectUndo(root,"Create L102 empty art");
            var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab,root.scene);
            Undo.SetTransformParent(child.transform,root.transform,"Parent board");
            child.transform.localPosition=Vector3.zero;
            child.transform.localRotation=Quaternion.identity;
            child.transform.localScale=Vector3.one;
            Selection.activeGameObject=root;
            Debug.Log("[L102] Created empty visual board only. No placed parts, completed routes or working game logic.");
        }
        [MenuItem("Tools/Project Spark/Levels/102/2. Validate Imported Models")]
        public static void ValidateModels()
        {
            int count=AssetDatabase.FindAssets("t:Model").Count(g=>AssetDatabase.GUIDToAssetPath(g).Contains("L102_"));
            Debug.Log($"[L102] Imported L102-prefixed FBX assets found: {count}. Expected 104 modular + 1 reference. This is ART validation only.");
        }
    }
}
#endif
