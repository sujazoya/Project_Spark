#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UIManagement;

namespace ProjectSpark.UIManagement.Editor
{
    public static class SparkUISetupWizard
    {
        [MenuItem("ProjectSpark/UI Manager/Create Master UI Manager + Monitor", false, 100)]
        public static void CreateMonitor()
        {
            EnsureSprite();
            var manager = Object.FindFirstObjectByType<SparkUIManager>();
            if (!manager)
            {
                var go = new GameObject("Spark UI Manager [Core]");
                Undo.RegisterCreatedObjectUndo(go, "Create Spark UI Manager");
                manager = go.AddComponent<SparkUIManager>();
            }
            var dashboard = Object.FindFirstObjectByType<SparkUIMonitorDashboard>();
            if (!dashboard)
            {
                var go = new GameObject("Spark MASTER UI MONITOR [F8]");
                Undo.RegisterCreatedObjectUndo(go, "Create Spark UI Monitor");
                dashboard = go.AddComponent<SparkUIMonitorDashboard>();
                var serialized = new SerializedObject(dashboard);
                serialized.FindProperty("manager").objectReferenceValue = manager;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FindFramePath());
                serialized.FindProperty("monitorFrame").objectReferenceValue = sprite;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Selection.activeGameObject = dashboard.gameObject;
            Debug.Log("[SparkUI] Manager and monitor created. Add SparkUIPanel to your EXISTING panel roots, then assign them to the Manager's Registered Panels list. Press Play, F8 to toggle console.");
        }
        [MenuItem("ProjectSpark/UI Manager/Add Panel Adapter To Selected", false, 101)]
        public static void AddPanel()
        {
            foreach (var go in Selection.gameObjects)
            {
                if (go.GetComponent<SparkUIPanel>()) continue;
                var adapter = Undo.AddComponent<SparkUIPanel>(go);
                var serialized = new SerializedObject(adapter);
                serialized.FindProperty("panelId").stringValue = go.name;
                serialized.FindProperty("displayName").stringValue = go.name;
                serialized.ApplyModifiedProperties();
            }
        }
        [MenuItem("ProjectSpark/UI Manager/Add Panel Adapter To Selected", true)]
        private static bool CanAddPanel() { return Selection.gameObjects != null && Selection.gameObjects.Length > 0; }
        [MenuItem("ProjectSpark/UI Manager/Configure Monitor Sprite", false, 102)]
        private static void EnsureSprite()
        {
            string path = FindFramePath();
            if (string.IsNullOrEmpty(path)) { Debug.LogError("[SparkUI] SparkMonitorFrame.png not found."); return; }
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
        private static string FindFramePath()
        {
            foreach (var guid in AssetDatabase.FindAssets("SparkMonitorFrame t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ProjectSpark/UIManager/Resources/")) return path;
            }
            return null;
        }
    }
}
#endif
