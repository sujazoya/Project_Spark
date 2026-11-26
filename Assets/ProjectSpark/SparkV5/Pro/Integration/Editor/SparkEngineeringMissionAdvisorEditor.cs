#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor binding and read-only diagnostics. Does not run level objectives or create cables.</summary>
    public static class SparkEngineeringMissionAdvisorEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(Menu + "30 - Bind Mission Advisor v15 (Prefab + Open Scenes)")]
        private static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Spark Mission Advisor", "Exit Play Mode before editing prefabs/scenes.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("SPARK Mission Advisor v15",
                "Add a read-only mission inspector to existing SparkV5Pro prefabs and scene instances? " +
                "Prefab files will be backed up first. No electrical/gameplay code will be changed.",
                "Bind", "Cancel")) return;

            string backup = Path.Combine(Path.GetDirectoryName(Application.dataPath), "../AI/SparkEngineering_Backups",
                "MISSION_v15_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(backup);
            int prefabs = 0, scenes = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab SparkV5Pro"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("SparkV5Pro.prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject prefab = null;
                try
                {
                    prefab = PrefabUtility.LoadPrefabContents(path);
                    bool changed = false;
                    foreach (var executive in prefab.GetComponentsInChildren<SparkProExecutive>(true))
                    {
                        if (!executive.GetComponent<SparkEngineeringControlHub>())
                        {
                            Debug.LogWarning("[Spark Mission v15] v14 hub missing on prefab; run v14 binder first: " + path);
                            continue;
                        }
                        if (!executive.GetComponent<SparkEngineeringMissionAdvisor>())
                        { executive.gameObject.AddComponent<SparkEngineeringMissionAdvisor>(); changed = true; }
                        if (!executive.GetComponent<SparkEngineeringMissionAdvisorPanel>())
                        { executive.gameObject.AddComponent<SparkEngineeringMissionAdvisorPanel>(); changed = true; }
                    }
                    if (changed)
                    {
                        string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), path));
                        File.Copy(fullPath, Path.Combine(backup, path.Replace('/', '_')), false);
                        PrefabUtility.SaveAsPrefabAsset(prefab, path);
                        prefabs++;
                    }
                }
                catch (Exception error) { Debug.LogError("[Spark Mission v15] " + path + ": " + error); }
                finally { if (prefab) PrefabUtility.UnloadPrefabContents(prefab); }
            }
            foreach (var executive in UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!executive || !executive.gameObject.scene.IsValid() ||
                    !executive.gameObject.scene.isLoaded || EditorUtility.IsPersistent(executive)) continue;
                if (!executive.GetComponent<SparkEngineeringControlHub>()) continue;
                bool changed = false;
                if (!executive.GetComponent<SparkEngineeringMissionAdvisor>())
                { Undo.AddComponent<SparkEngineeringMissionAdvisor>(executive.gameObject); changed = true; }
                if (!executive.GetComponent<SparkEngineeringMissionAdvisorPanel>())
                { Undo.AddComponent<SparkEngineeringMissionAdvisorPanel>(executive.gameObject); changed = true; }
                if (changed) { EditorSceneManager.MarkSceneDirty(executive.gameObject.scene); scenes++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Mission v15] Bound prefabs=" + prefabs + " scene instances=" + scenes +
                      ". Backups=" + backup + ". SAVE changed scenes manually. No circuit state modified.");
        }

        [MenuItem(Menu + "31 - Inspect Active Mission v15 (Read-Only)")]
        private static void Inspect()
        {
            var advisers = UnityEngine.Object.FindObjectsByType<SparkEngineeringMissionAdvisor>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            string output = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Logs/SparkEngineering/MissionAdvisorV15_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var sb = new System.Text.StringBuilder("SPARK v15 Mission Advisor (no wires created)\n")
                .Append("Play Mode: ").Append(EditorApplication.isPlaying).Append('\n')
                .Append("Advisors in loaded scenes: ").Append(advisers.Length).Append('\n');
            foreach (var advisor in advisers)
            {
                sb.AppendLine("--- " + advisor.name + " ---");
                sb.AppendLine(advisor.Inspect());
            }
            if (advisers.Length == 0)
                sb.AppendLine("No Mission Advisor found. Run v14 binder then menu 30 to bind v15.");
            File.WriteAllText(output, sb.ToString());
            Debug.Log("[Spark Mission v15] Report: " + output + "\n" + sb);
        }

        [MenuItem(Menu + "32 - Mission Advisor v15 Command Guide")]
        private static void ShowGuide()
        {
            EditorUtility.DisplayDialog("SPARK Mission Advisor v15",
                "INSPECT LEVEL: read actual authored level, power, exam, terminal IDs and approved plan.\n" +
                "EXPORT REPORT: save read-only report in Application.persistentDataPath.\n" +
                "The right-side SPARK MISSION tab appears in Play Mode.\n" +
                "Use v14 Assisted or Auto controls for actual authorized wiring.\n" +
                "Advisor never creates cables, changes power, approves plans or grants objectives.", "OK");
        }
    }
}
#endif
