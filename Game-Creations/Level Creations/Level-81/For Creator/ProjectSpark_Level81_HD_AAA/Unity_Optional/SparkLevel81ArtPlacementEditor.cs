#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 81 ART-ONLY placement of the 70 modular FBX assets; does not build gameplay.</summary>
    public static class SparkLevel81ArtPlacementEditor
    {
        private const string RootName = "L81_HD_VISUAL_ART";
        private const string Menu = "Tools/Project Spark/Levels/81/Place HD Art Under Selected Layout";

        [MenuItem(Menu)]
        public static void Place()
        {
            Transform target = Selection.activeTransform;
            if (target == null)
            {
                EditorUtility.DisplayDialog("Project Spark Level 81", "Select your Level 81 layout root GameObject first.", "OK");
                return;
            }

            var discovered = new Dictionary<int, GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Model");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                string file = System.IO.Path.GetFileNameWithoutExtension(path);
                if (file.Length < 8 || file[2] != '_' || !file.StartsWith(file.Substring(0, 2) + "_L81_", StringComparison.Ordinal)) continue;
                if (!int.TryParse(file.Substring(0, 2), out int index) || index < 0 || index >= 70) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && !discovered.ContainsKey(index)) discovered[index] = prefab;
            }

            var missing = new List<int>();
            for (int i = 0; i < 70; i++) if (!discovered.ContainsKey(i)) missing.Add(i);
            if (missing.Count != 0)
            {
                EditorUtility.DisplayDialog("Missing Level 81 FBX Models",
                    "Import all 70 numbered Level 81 FBX files before placement. Missing model indices:\n" + string.Join(", ", missing), "OK");
                return;
            }

            Transform old = target.Find(RootName);
            if (old != null)
            {
                if (!EditorUtility.DisplayDialog("Replace existing Level 81 visual art?",
                    "This deletes only the L81_HD_VISUAL_ART child and replaces its imported FBXs. Preserve gameplay objects outside this art root.", "Replace Art", "Cancel")) return;
                Undo.DestroyObjectImmediate(old.gameObject);
            }
            GameObject artRoot = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(artRoot, "Create Level 81 visual art root");
            artRoot.transform.SetParent(target, false);
            artRoot.transform.localPosition = Vector3.zero;
            artRoot.transform.localRotation = Quaternion.identity;
            artRoot.transform.localScale = Vector3.one;
            for (int i = 0; i < 70; i++)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(discovered[i], artRoot.transform) as GameObject;
                if (go == null) throw new InvalidOperationException("Could not instantiate Level 81 FBX index " + i);
                Undo.RegisterCreatedObjectUndo(go, "Place Level 81 model");
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            EditorUtility.SetDirty(artRoot);
            Debug.Log("[Project Spark Level 81] Placed 70 distinct FBX models under " + RootName + ". Art only; no terminals or gameplay were created.");
            Selection.activeGameObject = artRoot;
        }
        [MenuItem(Menu, true)]
        private static bool ValidSelection() => Selection.activeTransform != null;
    }
}
#endif
