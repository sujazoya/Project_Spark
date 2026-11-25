#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor-only binding and tests. Never changes circuits, missions or grades.</summary>
    public static class SparkLabDemoEditor
    {
        private const string MenuRoot = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string PrefabPath = "Assets/ProjectSpark/SparkV5/Pro/Generated/SparkV5Pro.prefab";

        [MenuItem(MenuRoot + "6 - AUTO Bind Demonstrator")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Spark Lab Demo V7] Exit Play Mode before changing prefab or scenes.");
                return;
            }
            int added = 0;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab)
            {
                var root = PrefabUtility.LoadPrefabContents(PrefabPath);
                try
                {
                    var ex = root.GetComponentInChildren<SparkProExecutive>(true);
                    if (ex && ex.GetComponent<SparkLabTeachingDirector>() && !ex.GetComponent<SparkLabDemonstrationDirector>())
                    {
                        string project = Directory.GetParent(Application.dataPath).FullName;
                        string backup = Path.Combine(Directory.GetParent(project).FullName, "AI", "SparkLab_Backups",
                            "DemoV7_Prefab_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
                        Directory.CreateDirectory(backup);
                        File.Copy(Path.Combine(project, PrefabPath), Path.Combine(backup, "SparkV5Pro.prefab"));
                        ex.gameObject.AddComponent<SparkLabDemonstrationDirector>();
                        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                        added++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            else Debug.LogWarning("[Spark Lab Demo V7] Character prefab missing at " + PrefabPath);

            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < executives.Length; i++)
            {
                var ex = executives[i];
                if (!ex || EditorUtility.IsPersistent(ex)) continue;
                if (!ex.GetComponent<SparkLabTeachingDirector>()) continue;
                if (ex.GetComponent<SparkLabDemonstrationDirector>()) continue;
                Undo.AddComponent<SparkLabDemonstrationDirector>(ex.gameObject);
                if (ex.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(ex.gameObject.scene);
                added++;
            }
            Debug.Log("[Spark Lab Demo V7] Added " + added + " demonstrator component(s). " +
                "Save modified scenes. Existing director instances, assets, missions and wires were preserved.");
        }

        [MenuItem(MenuRoot + "7 - Verify Demonstration Commands")]
        public static void Verify()
        {
            int passed = 0;
            void Check(bool value, string label)
            {
                if (!value) throw new InvalidOperationException("[Spark Lab Demo V7] FAILED: " + label);
                passed++;
            }
            Check(SparkLabDemoIntent.Classify("Start diode demonstration") == SparkLabDemoIntent.Kind.StartDiode, "diode start");
            Check(SparkLabDemoIntent.Classify("Demonstrate diode polarity") == SparkLabDemoIntent.Kind.StartDiode, "diode variation");
            Check(SparkLabDemoIntent.Classify("Show switch demo") == SparkLabDemoIntent.Kind.StartSwitch, "switch start");
            Check(SparkLabDemoIntent.Classify("Demonstrate multimeter measurement") == SparkLabDemoIntent.Kind.StartMeter, "meter start");
            Check(SparkLabDemoIntent.Classify("next demonstration step") == SparkLabDemoIntent.Kind.Next, "next step");
            Check(SparkLabDemoIntent.Classify("repeat demonstration step") == SparkLabDemoIntent.Kind.Repeat, "repeat step");
            Check(SparkLabDemoIntent.Classify("stop demonstration") == SparkLabDemoIntent.Kind.Stop, "stop");
            Check(SparkLabDemoIntent.Classify("demonstration status") == SparkLabDemoIntent.Kind.Status, "status");
            Check(SparkLabDemoIntent.Classify("Teach me Level 1") == SparkLabDemoIntent.Kind.None, "ordinary lesson unaffected");
            Check(SparkLabDemoIntent.Classify("Highlight selected component") == SparkLabDemoIntent.Kind.None, "v6.2 visual routing unaffected");
            Check(SparkLabDemoIntent.Classify("Point to the cathode") == SparkLabDemoIntent.Kind.None, "v6.2 terminal pointing unaffected");
            Check(SparkLabDemoIntent.Classify("Anode and cathode") == SparkLabDemoIntent.Kind.None, "ordinary answer unaffected");
            Check(SparkLabDemoIntent.Classify("Continue the lesson") == SparkLabDemoIntent.Kind.None, "lesson resume unaffected");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab)
            {
                var executive = prefab.GetComponentInChildren<SparkProExecutive>(true);
                Check(executive, "prefab has executive");
                Check(executive.GetComponent<SparkLabTeachingDirector>(), "prefab has v6 lab inspector");
                Check(executive.GetComponent<SparkLabDemonstrationDirector>(), "prefab has v7 demonstration director");
            }
            Debug.Log("[Spark Lab Demo V7] PASS " + passed + " checks. " +
                "Run Play Mode to verify real markers, isolated solver results and mission protections.");
        }

        [MenuItem(MenuRoot + "8 - Diagnose Demonstration in Play Mode")]
        public static void Diagnose()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Spark Lab Demo V7] Enter Play Mode before diagnosing the live scene.");
                return;
            }
            var demos = UnityEngine.Object.FindObjectsByType<SparkLabDemonstrationDirector>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var demo in demos)
            {
                if (!demo) continue;
                Debug.Log("[Spark Lab Demo V7] " + demo.gameObject.name +
                    " | active=" + demo.isActiveAndEnabled + " | " + demo.Status, demo);
            }
            if (demos.Length == 0)
                Debug.LogError("[Spark Lab Demo V7] No demonstrator in the loaded scene. Run menu 6 outside Play Mode.");
        }
    }
}
#endif
