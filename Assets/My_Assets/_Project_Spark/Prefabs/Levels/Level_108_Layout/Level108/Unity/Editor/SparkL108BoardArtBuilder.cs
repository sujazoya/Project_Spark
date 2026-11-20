#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level108.Editor
{
    public static class SparkL108BoardArtBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/108/1. Build EMPTY DFX-16 Board")]
        public static void BuildEmptyBoard()
        {
            var guids = AssetDatabase.FindAssets("119_L108_DFX16_BarePCB t:Model");
            if (guids.Length == 0)
            {
                Debug.LogError("[L108] Bare board model not found. Import Models_FBX first.");
                return;
            }
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) { Debug.LogError("[L108] FBX import failed: " + path); return; }
            var root = new GameObject("L108_DFX16_EMPTY_BOARD_ART");
            Undo.RegisterCreatedObjectUndo(root, "Create Level 108 empty board");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (!board) { Object.DestroyImmediate(root); return; }
            Undo.SetTransformParent(board.transform, root.transform, "Parent DFX-16 board");
            board.name = "DFX16_BarePCB_ArtOnly";
            // FBX reuses the 285 x 205 mm source geometry.
            // Scale only the visual footprint here, not the authoritative netlist.
            board.transform.localScale = new Vector3(300f/285f, 220f/205f, 1f);
            Selection.activeGameObject = root;
            Debug.Log("[L108] Empty BOARD ART created. No components/routes/vias/objectives are certified.");
        }
    }
}
#endif
