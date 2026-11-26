using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkTelescopicArmsEditor
    {
        private const string Root = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string Report = "Logs/SparkTelescopicArms/v18_Diagnostic.txt";

        [MenuItem(Root + "38 - Bind Telescopic Arms v18 (Prefabs + Open Scenes)")]
        public static void Bind()
        {
            int sceneCount = 0, prefabCount = 0;
            SparkProCharacter[] characters = UnityEngine.Object.FindObjectsByType<SparkProCharacter>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (SparkProCharacter c in characters)
            {
                if (!c || !c.GetComponent<SparkProExecutive>() || c.GetComponent<SparkTelescopicArms>()) continue;
                Undo.AddComponent<SparkTelescopicArms>(c.gameObject);
                if (c.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
                sceneCount++;
            }
            string[] guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != "SparkV5Pro") continue;
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    var character = root.GetComponentInChildren<SparkProCharacter>(true);
                    if (!character || !character.GetComponent<SparkProExecutive>() || character.GetComponent<SparkTelescopicArms>()) continue;
                    character.gameObject.AddComponent<SparkTelescopicArms>();
                    string backupRoot = Path.GetFullPath("Logs/SparkTelescopicArms/PrefabBackups");
                    Directory.CreateDirectory(backupRoot);
                    string backupName = Path.GetFileNameWithoutExtension(path) + "_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".prefab";
                    File.Copy(Path.GetFullPath(path), Path.Combine(backupRoot, backupName));
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    prefabCount++;
                }
                catch (Exception e) { Debug.LogError("[Spark v18] Prefab bind failed: " + path + ": " + e); }
                finally { if (root) PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark v18] Bound telescopic arms: scene=" + sceneCount + ", prefabs=" + prefabCount +
                ". Inspect and save scene changes. Test with a terminal ID before approving wiring integration.");
        }

        [MenuItem(Root + "39 - Diagnose Telescopic Arms v18 (Read Only)")]
        public static void Diagnose()
        {
            var report = new StringBuilder();
            report.AppendLine("SPARK v18 — TELESCOPIC ARM DIAGNOSTIC / READ ONLY");
            report.AppendLine("Compilation and rig-binding inspection only; this does not move an arm or change the circuit.");
            var arms = UnityEngine.Object.FindObjectsByType<SparkTelescopicArms>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Controllers in loaded scenes: " + arms.Length);
            foreach (var arm in arms)
            {
                if (!arm) continue;
                if (Application.isPlaying) arm.BindRig();
                report.AppendLine("  " + arm.name + ": " + arm.Status);
                var character = arm.GetComponent<SparkProCharacter>();
                foreach (string part in new[] { "Shoulder_R", "Elbow_R", "Wrist_R", "Shoulder_L", "Elbow_L", "Wrist_L" })
                    report.AppendLine("    " + part + "=" + (character && character.GetAnchor(part) ? "BOUND" : "NOT BOUND (run in Play Mode)"));
            }
            report.AppendLine("Acceptance: visible telescopic geometry, actual wrist near target, safe retract, no native wire regression.");
            string full = Path.GetFullPath(Report);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, report.ToString());
            Debug.Log("[Spark v18] " + full + "\n" + report);
        }
    }
}
