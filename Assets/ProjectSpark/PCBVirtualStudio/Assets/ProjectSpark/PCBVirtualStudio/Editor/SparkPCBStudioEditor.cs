#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.PCBVirtualStudio;

namespace ProjectSpark.PCBVirtualStudio.Editor
{
    public static class SparkPCBStudioEditor
    {
        [MenuItem("Tools/Project Spark/PCB Virtual Studio/1. Build AAA World-Space Studio")]
        private static void CreateWorldStudio()
        {
            Transform parent = Selection.activeTransform;
            Camera previewCamera = Camera.main;
            GameObject root = SparkPCBStudioFactory.Create(parent, previewCamera);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AAA PCB World Space Studio");
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark PCB UI] World-space Canvas built. Press Play, select a tool, then click a target. " +
                "Demo visual repairs are NOT authoritative circuit repairs.");
        }

        [MenuItem("Tools/Project Spark/PCB Virtual Studio/2. Verify Selected Studio")]
        private static void VerifyStudio()
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("[Spark PCB UI] Select a generated studio root first.");
                return;
            }
            var control = root.GetComponent<SparkVirtualPCBWorkstation>();
            var canvas = root.GetComponentInChildren<Canvas>();
            var targets = root.GetComponentsInChildren<SparkVirtualPCBTarget>(true);
            var tools = root.GetComponentsInChildren<SparkPCBToolButton>(true);
            if (control == null || canvas == null || canvas.renderMode != RenderMode.WorldSpace || targets.Length < 16 || tools.Length < 21)
            {
                Debug.LogError("[Spark PCB UI] Selected object is missing workstation, world canvas, demo targets or tools.");
                return;
            }
            Debug.Log("[Spark PCB UI] PASS: world-space canvas, 21 virtual tool controls and " + targets.Length +
                " interactive demo targets. Full solver and Play Mode still require testing.");
        }

        [MenuItem("Tools/Project Spark/PCB Virtual Studio/3. Set Selected Studio to Live Mode")]
        private static void MakeLive()
        {
            var root = Selection.activeGameObject;
            var control = root != null ? root.GetComponent<SparkVirtualPCBWorkstation>() : null;
            if (control == null) { Debug.LogWarning("Select the generated studio root first."); return; }
            Undo.RecordObject(control, "Disable PCB demo modifications");
            control.SetDemoMode(false);
            EditorUtility.SetDirty(control);
            Debug.Log("[Spark PCB UI] Demo mutations disabled. Bind a MonoBehaviour that implements " +
                "ISparkPCBRepairAuthority before trying repair operations in live mode.");
        }
    }
}
#endif
