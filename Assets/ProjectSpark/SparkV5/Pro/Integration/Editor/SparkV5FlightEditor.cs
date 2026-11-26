#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkV5FlightEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        [MenuItem(Menu + "25 - Bind Flyable Spark v13 (Prefab + Scenes)")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Spark Flight] Stop Play Mode before modifying prefabs and scenes.");
                return;
            }
            int prefabs = 0, instances = 0;
            string project = Directory.GetParent(Application.dataPath).FullName;
            string[] guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            for (int idx = 0; idx < guids.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[idx]);
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    SparkNativeWireDirector director = root.GetComponent<SparkNativeWireDirector>();
                    if (!director || root.GetComponent<SparkV5FlightController>()) continue;
                    string backup = Path.Combine(project, "..", "AI", "SparkNativeWire_Backups",
                        "FLIGHT_v13_PREFABS_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
                    Directory.CreateDirectory(backup);
                    File.Copy(Path.Combine(project, path), Path.Combine(backup, Path.GetFileName(path)), true);
                    root.AddComponent<SparkV5FlightController>();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    prefabs++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            for (int k = 0; k < SceneManager.sceneCount; k++)
            {
                Scene scene = SceneManager.GetSceneAt(k);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                foreach (SparkNativeWireDirector director in root.GetComponentsInChildren<SparkNativeWireDirector>(true))
                {
                    if (director.GetComponent<SparkV5FlightController>()) continue;
                    Undo.AddComponent<SparkV5FlightController>(director.gameObject);
                    EditorSceneManager.MarkSceneDirty(scene);
                    instances++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Flight v13] Flight controller added to " + prefabs +
                " prefabs and " + instances + " open scene instances. Save changed scenes. " +
                "Existing wiring controller/solver were not modified.");
        }
        [MenuItem(Menu + "26 - Diagnose Flight v13")]
        public static void Diagnose()
        {
            var report = new StringBuilder();
            report.AppendLine("SPARK v13 FLIGHT / REAL PLAYER CABLE DIAGNOSTIC");
            report.AppendLine("Inspection only. Does not move Spark or create any connection.");
            foreach (SparkNativeWireDirector director in UnityEngine.Object.FindObjectsByType<SparkNativeWireDirector>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var flight = director.GetComponent<SparkV5FlightController>();
                var character = director.GetComponent<SparkProCharacter>();
                var nav = director.GetComponent<NavMeshAgent>();
                var performer = director.GetComponent<SparkNativeWireCinematic>();
                report.AppendLine(director.name + ": flight=" + (flight ? "YES" : "NO") +
                    " cinematic=" + (performer ? "YES" : "NO") +
                    " character=" + (character ? "YES" : "NO") +
                    " navOnMesh=" + (nav && nav.enabled && nav.isOnNavMesh));
                if (flight)
                {
                    report.AppendLine("  " + flight.FlightStatus);
                    report.AppendLine("  " + flight.LastApproachReport);
                }
                if (character)
                    report.AppendLine("  real arm rig R=" +
                        (character.GetAnchor("Shoulder_R") && character.GetAnchor("Elbow_R") &&
                         character.GetAnchor("Wrist_R")) + " L=" +
                        (character.GetAnchor("Shoulder_L") && character.GetAnchor("Elbow_L") &&
                         character.GetAnchor("Wrist_L")));
            }
            string root = Directory.GetParent(Application.dataPath).FullName;
            string dest = Path.Combine(root, "Logs", "SparkNativeWire", "SparkFlightV13_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.WriteAllText(dest, report.ToString(), new UTF8Encoding(false));
            Debug.Log("[Spark Flight v13] " + dest + "\n" + report);
        }
    }
}
#endif
