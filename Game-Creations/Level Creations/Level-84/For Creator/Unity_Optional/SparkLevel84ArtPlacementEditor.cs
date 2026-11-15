#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Level 84 ART ONLY. Places 76 original origin-aligned imported FBX models.
    /// Does not create circuits, real SparkTerminals, rigid-body physics or objectives.
    /// </summary>
    public static class SparkLevel84ArtPlacementEditor
    {
        private const string Menu = "Tools/Project Spark/Levels/84/Place HD Art Under Selected Layout";
        private const string ArtRoot = "L84_HD_VISUAL_ART";

        [MenuItem(Menu)]
        public static void Place()
        {
            Transform target = Selection.activeTransform;
            if (target == null)
            {
                EditorUtility.DisplayDialog("Project Spark Level 84", "Select your Level 84 layout root GameObject.", "OK");
                return;
            }
            var models = new Dictionary<int, GameObject>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                string basename = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (basename.Length < 8 || basename[2] != '_' || !basename.Substring(3).StartsWith("L84_", StringComparison.Ordinal)) continue;
                int number;
                if (!int.TryParse(basename.Substring(0, 2), out number) || number < 0 || number >= 76) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null && !models.ContainsKey(number)) models.Add(number, prefab);
            }
            var missing = new List<int>();
            for (int i = 0; i < 76; i++) if (!models.ContainsKey(i)) missing.Add(i);
            if (missing.Count > 0)
            {
                EditorUtility.DisplayDialog("Level 84 FBX Import Missing", "Missing model indices: " + string.Join(", ", missing) + ". Import all 76 numbered FBX files first.", "OK");
                return;
            }
            Transform old = target.Find(ArtRoot);
            if (old != null && !EditorUtility.DisplayDialog("Replace Level 84 art?", "Only the L84_HD_VISUAL_ART child will be replaced. Keep gameplay systems, real terminals and wires outside this child.", "Replace Art", "Cancel")) return;
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 84 visual art root");
            root.transform.SetParent(target, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < 76; i++)
            {
                GameObject created = PrefabUtility.InstantiatePrefab(models[i], root.transform) as GameObject;
                if (created == null) throw new InvalidOperationException("Cannot instantiate Level 84 FBX index " + i);
                Undo.RegisterCreatedObjectUndo(created, "Place Level 84 FBX");
                created.transform.localPosition = Vector3.zero;
                created.transform.localRotation = Quaternion.identity;
                created.transform.localScale = Vector3.one;
            }
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(root);
            Debug.Log("[Project Spark L84] Placed 76 modular FBXs under " + ArtRoot + ". Artwork only, not functional circuit or gameplay.");
        }

        [MenuItem(Menu, true)]
        private static bool SelectionValid() { return Selection.activeTransform != null; }
    }
}
#endif
