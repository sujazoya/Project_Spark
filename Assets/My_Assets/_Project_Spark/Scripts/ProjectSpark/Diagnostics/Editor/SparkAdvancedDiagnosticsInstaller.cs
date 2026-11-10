using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Diagnostics.Editor
{
    public static class SparkAdvancedDiagnosticsInstaller
    {
        [MenuItem("Tools/Project Spark/Diagnostics/Open Advanced Monitor")]
        public static void Install()
        {
            var existing = Object.FindFirstObjectByType<SparkAdvancedDiagnostics>();
            if (existing != null) { Selection.activeGameObject = existing.gameObject; return; }
            var go = new GameObject("Spark Advanced Diagnostics");
            Undo.RegisterCreatedObjectUndo(go, "Add advanced diagnostics");
            Undo.AddComponent<SparkAdvancedDiagnostics>(go);
            Selection.activeGameObject = go;
            Debug.Log("Advanced monitor installed. In Play Mode use F8 or its Diagnostics button; Copy report provides the complete snapshot. Save the scene to retain this object.", go);
        }
    }
}
