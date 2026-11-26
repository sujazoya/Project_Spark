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
    public static class SparkNativeWireCinematicEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(Menu + "23 - AUTO Bind Walking & Hand IK v12.2")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Spark v12.2] Leave Play Mode before adding the performance component.");
                return;
            }
            int prefabs = 0, instances = 0;
            var guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var director = root.GetComponent<SparkNativeWireDirector>();
                    if (!director) continue;
                    var character = root.GetComponent<SparkProCharacter>();
                    var navigation = root.GetComponent<NavMeshAgent>();
                    bool needsPerformer = !root.GetComponent<SparkNativeWireCinematic>();
                    bool needsAgentReference = false;
                    if (character && navigation)
                    {
                        var serializedCharacter = new SerializedObject(character);
                        var navProperty = serializedCharacter.FindProperty("agent");
                        needsAgentReference = navProperty != null && !navProperty.objectReferenceValue;
                    }
                    if (!needsPerformer && !needsAgentReference) continue;
                    string project = Directory.GetParent(Application.dataPath).FullName;
                    string backup = Path.Combine(project, "..", "AI", "SparkNativeWire_Backups", "CINEMATIC_v12", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
                    Directory.CreateDirectory(backup);
                    File.Copy(Path.Combine(project, path), Path.Combine(backup, Path.GetFileName(path)), true);
                    if (needsPerformer) root.AddComponent<SparkNativeWireCinematic>();
                    if (needsAgentReference)
                    {
                        var serializedCharacter = new SerializedObject(character);
                        var navProperty = serializedCharacter.FindProperty("agent");
                        navProperty.objectReferenceValue = navigation;
                        serializedCharacter.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    prefabs++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                foreach (SparkNativeWireDirector director in root.GetComponentsInChildren<SparkNativeWireDirector>(true))
                {
                    bool changed = false;
                    if (!director.GetComponent<SparkNativeWireCinematic>())
                    {
                        Undo.AddComponent<SparkNativeWireCinematic>(director.gameObject);
                        changed = true;
                    }
                    var character = director.GetComponent<SparkProCharacter>();
                    var navigation = director.GetComponent<NavMeshAgent>();
                    if (character && navigation)
                    {
                        var serializedCharacter = new SerializedObject(character);
                        var navProperty = serializedCharacter.FindProperty("agent");
                        if (navProperty != null && !navProperty.objectReferenceValue)
                        {
                            Undo.RecordObject(character, "Bind Spark navigation agent");
                            navProperty.objectReferenceValue = navigation;
                            serializedCharacter.ApplyModifiedProperties();
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        instances++;
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark v12.2] Bound cinematic performer: " + prefabs + " prefabs, " + instances +
                " scene objects. Save changed scenes. NavMesh baking is a separate required setup step.");
        }

        [MenuItem(Menu + "24 - Diagnose Walking, Rig & Native Cable v12.2")]
        public static void Diagnose()
        {
            var report = new StringBuilder(2000);
            report.AppendLine("SPARK V12.2 — REAL HAND-DRIVEN PLAYER WIRE DIAGNOSTIC");
            report.AppendLine("Diagnostic only; does not create a wire or modify the circuit.");
            var directors = UnityEngine.Object.FindObjectsByType<SparkNativeWireDirector>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Directors: " + directors.Length);
            for (int i = 0; i < directors.Length; i++)
            {
                var d = directors[i];
                var p = d.GetComponent<SparkNativeWireCinematic>();
                var c = d.GetComponent<SparkProCharacter>();
                var n = d.GetComponent<NavMeshAgent>();
                var native = d.GetComponent<SparkAINativeSignalWirePort>();
                report.AppendLine("Avatar " + d.name + ": performance=" + (p ? "YES" : "NO") +
                    " rig=" + (c ? "YES" : "NO") + " nativePort=" + (native ? "YES" : "NO") +
                    " navAgent=" + (n ? "YES" : "NO") +
                    " navEnabled=" + (n && n.enabled) +
                    " navOnMesh=" + (n && n.enabled && n.isOnNavMesh) +
                    " characterNavBound=" + (c && c.NavigationAgent == n));
                if (c)
                {
                    string[] bones = { "Shoulder_R", "Elbow_R", "Wrist_R", "Shoulder_L", "Elbow_L", "Wrist_L" };
                    foreach (string bone in bones)
                        report.AppendLine("  " + bone + "=" + (c.GetAnchor(bone) ? "YES" : "NOT BOUND (Play Mode required)"));
                }
                // Do not enable agents or make contact when inspecting.
                if (n && !n.enabled)
                {
                    bool onSurface = NavMesh.SamplePosition(n.transform.position, out NavMeshHit hit, .25f, n.areaMask);
                    report.AppendLine("  NavMesh within 0.25m of avatar=" + onSurface);
                }
            }
            var mesh = NavMesh.CalculateTriangulation();
            report.AppendLine("Baked NavMesh triangles=" + (mesh.indices == null ? 0 : mesh.indices.Length / 3));
            report.AppendLine("Production acceptance: visible walking + real hand cable tracking + graph link + no exam bypass.");
            string rootFolder = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.Combine(rootFolder, "Logs", "SparkNativeWire", "CinematicV12_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, report.ToString(), new UTF8Encoding(false));
            Debug.Log("[Spark v12.2] Diagnostic: " + output + "\n" + report);
        }
    }
}
#endif
