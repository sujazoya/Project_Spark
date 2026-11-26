#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor-only controlled binding and non-mutating diagnostics for the v14 hub.</summary>
    public static class SparkEngineeringControlEditor
    {
        private const string RootMenu = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(RootMenu + "27 - Bind Engineering Modes v14 (Prefab + Open Scenes)")]
        private static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Spark v14", "Stop Play Mode before binding components.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("SPARK Engineering PRO v14",
                "Add the Engineering Control Hub to SparkV5Pro prefabs and open scenes? " +
                "Prefabs will be backed up before modification. Existing cables and AI source files remain unchanged.",
                "Bind with backup", "Cancel")) return;
            string backup = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../AI/SparkEngineering_Backups/BIND_v14_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")));
            Directory.CreateDirectory(backup);
            int changedPrefabs = 0, changedScenes = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (!path.EndsWith("SparkV5Pro.prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject contents = null;
                try
                {
                    contents = PrefabUtility.LoadPrefabContents(path);
                    var executives = contents.GetComponentsInChildren<SparkProExecutive>(true);
                    if (executives.Length == 0) continue;
                    bool changed = false;
                    for (int j = 0; j < executives.Length; j++)
                    {
                        if (executives[j].GetComponent<SparkEngineeringControlHub>()) continue;
                        executives[j].gameObject.AddComponent<SparkEngineeringControlHub>();
                        changed = true;
                    }
                    if (changed)
                    {
                        BackupUnityAsset(path, backup);
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                        changedPrefabs++;
                    }
                }
                catch (Exception error)
                {
                    Debug.LogError("[Spark v14] Could not bind prefab '" + path + "': " + error);
                }
                finally
                {
                    if (contents) PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            // Scenes remain under user save control; do not overwrite a dirty scene.
            SparkProExecutive[] live = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < live.Length; i++)
            {
                var e = live[i];
                if (!e || !e.gameObject.scene.IsValid() || !e.gameObject.scene.isLoaded ||
                    EditorUtility.IsPersistent(e) || e.GetComponent<SparkEngineeringControlHub>()) continue;
                Undo.AddComponent<SparkEngineeringControlHub>(e.gameObject);
                EditorSceneManager.MarkSceneDirty(e.gameObject.scene);
                changedScenes++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark v14] Binder finished: modified prefabs=" + changedPrefabs +
                      ", scene instances=" + changedScenes + ", backup=" + backup +
                      ". SAVE modified scenes manually. Next run menu 28 - Diagnose Engineering Modes v14.");
        }

        private static void BackupUnityAsset(string relativePath, string directory)
        {
            string absolute = Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePath);
            if (!File.Exists(absolute)) throw new FileNotFoundException("Unity prefab missing: " + absolute);
            string safeName = relativePath.Replace('/', '_').Replace('\\', '_');
            File.Copy(absolute, Path.Combine(directory, safeName), false);
        }

        [MenuItem(RootMenu + "28 - Diagnose Engineering Modes v14")]
        private static void Diagnose()
        {
            var hubs = UnityEngine.Object.FindObjectsByType<SparkEngineeringControlHub>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            var sb = new System.Text.StringBuilder("SPARK v14 ENGINEERING MODES — DIAGNOSTIC (read-only)\n")
                .Append("Playing: ").Append(EditorApplication.isPlaying).Append('\n')
                .Append("Hubs in loaded scenes: ").Append(hubs.Length).Append('\n');
            for (int i = 0; i < hubs.Length; i++)
            {
                SparkEngineeringControlHub h = hubs[i];
                sb.Append(h.name).Append(" | ").Append(h.Status()).Append('\n');
                sb.Append("  Executive=").Append((bool)h.GetComponent<SparkProExecutive>())
                    .Append("  Director=").Append((bool)h.GetComponent<SparkNativeWireDirector>())
                    .Append("  v11Recorder=").Append((bool)h.GetComponent<SparkNativeWireAutoEngineer>())
                    .Append("  RuntimePanel=").Append((bool)h.GetComponent<SparkEngineeringControlPanel>())
                    .Append('\n');
            }
            string output = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Logs/SparkEngineering/EngineeringV14_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, sb.ToString());
            Debug.Log("[Spark v14] Diagnostic written to " + output + "\n" + sb);
        }

        [MenuItem(RootMenu + "29 - Show Engineering v14 Command Reference")]
        private static void Commands()
        {
            EditorUtility.DisplayDialog("SPARK v14 Commands",
                "MANUAL: spark manual mode\n" +
                "ASSISTED: spark assisted mode, spark suggest next wire, spark approve suggested wire\n" +
                "AUTO: spark auto mode, preview wiring plan, auto wire practice level\n" +
                "CONTROL: spark pause automation, spark resume automation, spark cancel automation\n" +
                "STATUS: spark engineering status, spark reload engineering plans\n\n" +
                "All wiring still uses the original native player Wire Tool. Independent exams block AI construction.",
                "OK");
        }
    }
}
#endif
