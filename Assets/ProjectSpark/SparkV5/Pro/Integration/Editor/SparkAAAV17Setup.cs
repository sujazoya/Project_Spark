using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkAAAV17Setup
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string BackupFolder = "Logs/SparkAAA_V17/AssetBackups/";

        [MenuItem(Menu + "36 - Bind AAA Wiring Polish v17 (Prefab + Open Scenes)")]
        private static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Spark AAA v17", "Stop Play Mode before binding. Runtime scene clones are not persistent.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Spark AAA Wiring Polish v17",
                "Bind v17 to SparkV5Pro prefabs and loaded scene avatars? The operation disables legacy overlapping per-wire narration. A prefab backup will be saved first.",
                "Bind", "Cancel")) return;
            int prefabs = 0, scenes = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!Path.GetFileNameWithoutExtension(path).Equals("SparkV5Pro", StringComparison.OrdinalIgnoreCase)) continue;
                BackupFile(path);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    SparkProExecutive[] executives = root.GetComponentsInChildren<SparkProExecutive>(true);
                    for (int e = 0; e < executives.Length; e++) Configure(executives[e].gameObject, false);
                    if (executives.Length > 0) { PrefabUtility.SaveAsPrefabAsset(root, path); prefabs++; }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    SparkProExecutive[] executives = roots[r].GetComponentsInChildren<SparkProExecutive>(true);
                    for (int e = 0; e < executives.Length; e++)
                    {
                        Configure(executives[e].gameObject, true);
                        scenes++;
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark AAA v17] Bound prefabs=" + prefabs + ", loaded scene avatars=" + scenes +
                ". Save scene changes. No electrical solver/native wire sources were replaced.");
            EditorUtility.DisplayDialog("Spark AAA v17", "Binding finished. Prefabs: " + prefabs + ", scene instances: " + scenes +
                ". Save modified scenes, then run menu 37 for diagnostic checks.", "OK");
        }

        private static void Configure(GameObject go, bool undo)
        {
            if (undo)
            {
                if (!go.GetComponent<SparkAAANarrationDirector>()) Undo.AddComponent<SparkAAANarrationDirector>(go);
                if (!go.GetComponent<SparkAAAWireVisualGuard>()) Undo.AddComponent<SparkAAAWireVisualGuard>(go);
            }
            else
            {
                if (!go.GetComponent<SparkAAANarrationDirector>()) go.AddComponent<SparkAAANarrationDirector>();
                if (!go.GetComponent<SparkAAAWireVisualGuard>()) go.AddComponent<SparkAAAWireVisualGuard>();
            }
            // Bind the milestone narrator as the sole wiring speech owner.
            SparkNativeWireCinematic cinematic = go.GetComponent<SparkNativeWireCinematic>();
            if (cinematic) SetBool(cinematic, "announceEachWire", false);
            SparkEngineeringControlHub hub = go.GetComponent<SparkEngineeringControlHub>();
            if (hub) SetBool(hub, "spokenActionNarration", false);
            EditorUtility.SetDirty(go);
        }

        private static void SetBool(UnityEngine.Object target, string name, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning("[Spark AAA v17] " + target.GetType().Name +
                    "." + name + " not found; inspect customized source before claiming v17 integration."); return; }
            if (p.propertyType != SerializedPropertyType.Boolean) return;
            p.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void BackupFile(string path)
        {
            string root = Path.GetDirectoryName(Application.dataPath);
            string target = Path.Combine(root, BackupFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(target);
            File.Copy(Path.Combine(root, path), Path.Combine(target, Path.GetFileName(path)), true);
        }

        [MenuItem(Menu + "37 - Diagnose AAA Wiring Polish v17 (Read-Only)")]
        private static void Diagnose()
        {
            var report = new StringBuilder();
            report.AppendLine("SPARK AAA v17 - NARRATION AND NATIVE WIRE ROUTE");
            report.AppendLine("Read-only diagnostic; no cables or circuit changes.");
            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Active scene executives=" + executives.Length);
            for (int i = 0; i < executives.Length; i++)
            {
                var o = executives[i];
                var narrator = o.GetComponent<SparkAAANarrationDirector>();
                var visual = o.GetComponent<SparkAAAWireVisualGuard>();
                var cinematic = o.GetComponent<SparkNativeWireCinematic>();
                var port = o.GetComponent<SparkAINativeSignalWirePort>();
                report.AppendLine("Character " + o.name + ": v17Narrator=" + (narrator ? "YES" : "NO") +
                    " visualGuard=" + (visual ? "YES" : "NO") + " cinematic=" + (cinematic ? "YES" : "NO") +
                    " nativePort=" + (port ? "YES" : "NO"));
                if (narrator) report.AppendLine("  lastStage=" + narrator.LastStage + ", pending=" + narrator.PendingStages +
                    ", published=" + narrator.PublishedStages + ", suppressed=" + narrator.SuppressedStages +
                    ", textFallback=" + narrator.SpeechInTextFallback);
                if (visual) report.AppendLine("  cableRoute=" + visual.Status);
                if (cinematic) report.AppendLine("  legacyNarration=" + ReadProperty(cinematic, "announceEachWire"));
                if (port)
                {
                    report.AppendLine("  playerPointerSession=" + port.UsesPlayerPointer);
                    var builder = port.ResolvedPlayerBuilder;
                    report.AppendLine("  builder=" + (builder ? builder.name : "NONE") +
                        (builder ? ", routing=" + builder.CableRouting + ", sampledWaypoints=" + builder.DragRoutePointCount : ""));
                }
            }
            string rootPath = Path.GetDirectoryName(Application.dataPath);
            string path = Path.Combine(rootPath, "Logs/SparkAAA_V17/Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report.ToString());
            Debug.Log("[Spark AAA v17] Diagnostic written to " + path + "\n" + report);
        }
        private static string ReadProperty(UnityEngine.Object target, string name)
        {
            SerializedProperty p = new SerializedObject(target).FindProperty(name);
            return p == null ? "MISSING" : (p.propertyType == SerializedPropertyType.Boolean ? p.boolValue.ToString() : "TYPE_MISMATCH");
        }
    }
}
