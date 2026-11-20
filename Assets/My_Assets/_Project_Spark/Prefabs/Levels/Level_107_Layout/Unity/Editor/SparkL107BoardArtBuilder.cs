#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level107.Editor
{
    public static class SparkL107BoardArtBuilder
    {
        private const string BoardSearch = "113_L107_PFX15_BarePCB t:Model";

        [MenuItem("Tools/Project Spark/Levels/107/1. Build EMPTY PFX-15 Board")]
        public static void BuildEmpty()
        {
            var guids = AssetDatabase.FindAssets(BoardSearch);
            if (guids.Length == 0)
            {
                Debug.LogError("[L107] Import the Level 107 Models_FBX before building.");
                return;
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (prefab == null) { Debug.LogError("[L107] Bare board FBX is not an importable GameObject."); return; }

            var root = new GameObject("L107_PFX15_EMPTY_ART_ROOT");
            Undo.RegisterCreatedObjectUndo(root, "Create Level107 board art");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (board == null) { Object.DestroyImmediate(root); return; }
            Undo.SetTransformParent(board.transform,root.transform,"Parent Level107 art");
            board.name = "PFX15_BarePCB_ArtOnly";
            // Underlying source board: 285 x 205 mm. Target: 290 x 210 mm.
            board.transform.localScale = new Vector3(290f/285f,1f,210f/205f);
            Selection.activeGameObject = root;
            Debug.Log("[L107] EMPTY visual root created. NO components, routes, vias, firmware or objectives were completed.");
        }

        [MenuItem("Tools/Project Spark/Levels/107/2. Select Level 107 Art FBXs")]
        public static void SelectModels()
        {
            var guids=AssetDatabase.FindAssets("L107_ t:Model");
            var assets = new System.Collections.Generic.List<Object>();
            foreach(var guid in guids)
            {
                var p=AssetDatabase.GUIDToAssetPath(guid);
                if(p.Contains("Models_FBX/"))
                {
                    var a=AssetDatabase.LoadMainAssetAtPath(p);
                    if(a!=null) assets.Add(a);
                }
            }
            Selection.objects=assets.ToArray();
            Debug.Log("[L107] Found "+assets.Count+" FBX art assets.");
        }
    }
}
#endif
