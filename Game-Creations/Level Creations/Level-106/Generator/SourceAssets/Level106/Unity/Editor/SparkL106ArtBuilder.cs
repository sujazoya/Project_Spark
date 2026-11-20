#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level106.Editor
{
    public static class SparkL106ArtBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/106/1. Build EMPTY SFX-14 Board")]
        public static void BuildEmptyBoard()
        {
            var guids = AssetDatabase.FindAssets("107_L106_SFX14_BarePCB t:Model");
            if (guids.Length == 0) { Debug.LogError("[L106] Bare PCB FBX not found; import Models_FBX first."); return; }
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) { Debug.LogError("[L106] PCB asset import failed: " + path); return; }
            var root = new GameObject("L106_SFX14_EMPTY_ART_ROOT");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            board.name = "SFX14_Bare_PCB_ArtOnly"; board.transform.SetParent(root.transform, false);
            Selection.activeGameObject = root;
            Undo.RegisterCreatedObjectUndo(root, "Create Level106 Empty Board");
            Debug.Log("[L106] Art-only board built. No electrical components, vias, routes, or gameplay are certified.");
        }
        [MenuItem("Tools/Project Spark/Levels/106/2. Configure FBX Import (Meters)")]
        public static void ConfigureImport()
        {
            var guids = AssetDatabase.FindAssets("L106 t:Model"); int changed = 0;
            foreach (var g in guids) {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (!p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                var importer = AssetImporter.GetAtPath(p) as ModelImporter;
                if (importer == null) continue;
                importer.globalScale = 1f; importer.importMaterials = true;
                importer.SaveAndReimport(); changed++;
            }
            Debug.Log("[L106] Import settings applied to " + changed + " FBXs. Verify meter dimensions and normals manually.");
        }
    }
}
#endif
