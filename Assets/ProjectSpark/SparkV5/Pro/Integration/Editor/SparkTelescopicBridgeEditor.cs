using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkTelescopicBridgeEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string Report = "Logs/SparkTelescopicArms/v18_5_Bridge_Diagnostic.txt";

        [MenuItem(Menu + "43 - Bind Original-Hand Native Wiring Bridge v18.5")]
        private static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Spark v18.5", "Stop Play Mode before binding persistent objects.", "OK");
                return;
            }
            int scenes = 0, prefabs = 0;
            var existing = UnityEngine.Object.FindObjectsByType<SparkTelescopicArms>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in existing)
            {
                if (!item || item.GetComponent<SparkTelescopicNativeWireBridge>()) continue;
                Undo.AddComponent<SparkTelescopicNativeWireBridge>(item.gameObject);
                if (item.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(item.gameObject.scene);
                scenes++;
            }
            string[] guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    continue;
                GameObject prefabRoot = null;
                try
                {
                    prefabRoot = PrefabUtility.LoadPrefabContents(path);
                    if (!prefabRoot) continue;
                    var arms = prefabRoot.GetComponentsInChildren<SparkTelescopicArms>(true);
                    bool modified = false;
                    foreach (var arm in arms)
                    {
                        if (arm.GetComponent<SparkTelescopicNativeWireBridge>()) continue;
                        arm.gameObject.AddComponent<SparkTelescopicNativeWireBridge>();
                        modified = true;
                    }
                    if (modified)
                    {
                        string dir = Path.GetFullPath("Logs/SparkTelescopicArms/PrefabBackups");
                        Directory.CreateDirectory(dir);
                        string name = Path.GetFileNameWithoutExtension(path) + "_" +
                            DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".prefab";
                        File.Copy(Path.GetFullPath(path), Path.Combine(dir, name), false);
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                        prefabs++;
                    }
                }
                finally { if (prefabRoot) PrefabUtility.UnloadPrefabContents(prefabRoot); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark v18.5] Bound scene components=" + scenes +
                " modified prefab assets=" + prefabs + ". Save any modified scenes. " +
                "This binder does not patch cinematic C# code.");
        }

        [MenuItem(Menu + "44 - Diagnose Original-Hand Native Wiring Bridge v18.5")]
        private static void Diagnose()
        {
            var text = new StringBuilder();
            text.AppendLine("SPARK v18.5 — NATIVE HAND-TO-WIRE INTEGRATION DIAGNOSTIC");
            text.AppendLine("Read-only. No wire operation, no movement, no circuit mutation.");
            text.AppendLine("PlayMode=" + EditorApplication.isPlaying);
            var bridges = UnityEngine.Object.FindObjectsByType<SparkTelescopicNativeWireBridge>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            text.AppendLine("Bridges=" + bridges.Length);
            foreach (var bridge in bridges)
            {
                if (!bridge) continue;
                text.AppendLine("Spark=" + bridge.name + " configured=" + bridge.IsConfigured +
                    " cinematicEnabled=" + bridge.CinematicExtensionEnabled +
                    " contactActive=" + bridge.IsContactActive +
                    " last=" + bridge.LastError);
                var arms = bridge.GetComponent<SparkTelescopicArms>();
                text.AppendLine("Arms=" + (arms ? arms.Status : "MISSING"));
                var director = bridge.GetComponent<SparkNativeWireDirector>();
                text.AppendLine("NativeWireDirector=" + (director ? "bound" : "missing"));
            }
            string path = Path.GetFullPath(Report);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text.ToString());
            Debug.Log("[Spark v18.5] Diagnostic saved: " + path + "\n" + text);
        }
    }
}
