#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace ProjectSpark.Level110.Editor
{
    public static class SparkL110ReferenceArtBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/110/Create PWR-18 Reference Art")]
        private static void CreateReferenceArt()
        {
            const string name = "ProjectSpark_Level110_PWR18_Complete_Assembled_Board";
            string[] guids = AssetDatabase.FindAssets(name + " t:Model", new[] {"Assets/ProjectSpark/Level110"});
            if (guids.Length == 0) { Debug.LogError("[L110] Assembled FBX missing. Import Level 110 model package first."); return; }
            GameObject prefab = null;
            for (int i = 0; i < guids.Length; i++)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) break;
            }
            if (prefab == null) { Debug.LogError("[L110] Reference FBX could not be imported."); return; }
            var root = new GameObject("L110_PWR18_COMPLETED_REFERENCE_ART_ONLY");
            Undo.RegisterCreatedObjectUndo(root,"Add PWR-18 reference");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.SetTransformParent(instance.transform,root.transform,"Parent PWR-18 art");
            // FBX imports vary in axis/units. Inspect resulting bounds manually before changing scale.
            instance.transform.localPosition=Vector3.zero;
            instance.transform.localRotation=Quaternion.identity;
            Selection.activeGameObject=root;
            Debug.Log("[L110] Art reference created. NOT a playable board, electrical circuit, or objective progress.");
        }
    }
}
#endif
