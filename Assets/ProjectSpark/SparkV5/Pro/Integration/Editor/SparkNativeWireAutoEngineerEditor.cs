#if UNITY_EDITOR
using System;
using System.IO;
using ProjectSpark.SparkV5.Pro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkNativeWireAutoEngineerEditor
    {
        private const string Prefix = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string PlanDir = "Assets/Resources/SparkNativeWirePlans";

        [MenuItem(Prefix + "19 - Bind Auto Engineer (Prefab + Open Scenes)")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogError("[Spark Auto Engineer] Exit Play Mode before binding."); return; }
            string[] guids = AssetDatabase.FindAssets("t:Prefab SparkV5Pro");
            int prefabs = 0, scenes = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var exec = root.GetComponent<SparkProExecutive>();
                    if (!exec) continue;
                    if (!root.GetComponent<SparkNativeWireAutoEngineer>())
                    {
                        root.AddComponent<SparkNativeWireAutoEngineer>();
                        BackupOriginalPrefab(path);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        prefabs++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    var executives = root.GetComponentsInChildren<SparkProExecutive>(true);
                    foreach (var exec in executives)
                        if (!exec.GetComponent<SparkNativeWireAutoEngineer>())
                        {
                            Undo.AddComponent<SparkNativeWireAutoEngineer>(exec.gameObject);
                            EditorSceneManager.MarkSceneDirty(scene);
                            scenes++;
                        }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Auto Engineer] Bound " + prefabs + " prefab(s) and " + scenes +
                " scene object(s). Save changed scenes. Plans remain unapproved until reviewed.");
        }

        private static void BackupOriginalPrefab(string path)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string original = Path.Combine(project, path);
            string root = Path.GetFullPath(Path.Combine(project, "..", "AI", "SparkNativeWire_Backups", "AUTO_v11_Prefabs"));
            Directory.CreateDirectory(root);
            string unique = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" +
                Path.GetFileNameWithoutExtension(path) + "_" + Math.Abs(path.GetHashCode()) + ".prefab";
            File.Copy(original, Path.Combine(root, unique), false);
        }

        [MenuItem(Prefix + "20 - Import Latest Recorded Practice Wiring Draft")]
        public static void ImportDraft()
        {
            string dir = Path.Combine(Application.persistentDataPath, "SparkNativeWireDrafts");
            if (!Directory.Exists(dir)) { Debug.LogError("No drafts. In Play Mode use 'start wiring recording' and 'stop wiring recording'."); return; }
            string[] files = Directory.GetFiles(dir, "Level_*_WiringDraft.json");
            if (files.Length == 0) { Debug.LogError("No recorded wiring drafts were found."); return; }
            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
            SparkNativeWiringPlan.RecordedDraft draft;
            try { draft = JsonUtility.FromJson<SparkNativeWiringPlan.RecordedDraft>(File.ReadAllText(files[0])); }
            catch (Exception e) { Debug.LogError("Invalid draft: " + e.Message); return; }
            if (draft == null || draft.schemaVersion != 1 || draft.wires == null || draft.wires.Length == 0 ||
                draft.levelNumber < 1 || draft.levelNumber > 130 || string.IsNullOrWhiteSpace(draft.layoutName))
            { Debug.LogError("Draft schema or active-level identity is invalid."); return; }
            if (!Directory.Exists(PlanDir)) Directory.CreateDirectory(PlanDir);
            var plan = ScriptableObject.CreateInstance<SparkNativeWiringPlan>();
            plan.LoadDraft(draft);
            string path = AssetDatabase.GenerateUniqueAssetPath(PlanDir + "/Level_" +
                draft.levelNumber.ToString("000") + "_NativeWiring_DRAFT.asset");
            AssetDatabase.CreateAsset(plan, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = plan;
            EditorGUIUtility.PingObject(plan);
            Debug.Log("[Spark Auto Engineer] Unapproved wiring draft imported: " + path +
                ". Inspect every connection against the authored schematic before approving.");
        }

        [MenuItem(Prefix + "21 - Review and Approve Selected Practice Wiring Plan")]
        public static void Approve()
        {
            var plan = Selection.activeObject as SparkNativeWiringPlan;
            if (!plan) { Debug.LogError("Select a SparkNativeWiringPlan asset in the Project window first."); return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogError("Exit Play Mode before approving a plan."); return; }
            if (!EditorUtility.DisplayDialog("Approve reviewed wiring plan?",
                "Only approve after checking EVERY source/destination against the authored schematic, verifying current limiting, and testing the player's native cable.\n\n" +
                "This permits Spark to execute real wiring in PRACTICE levels. Independent exams are protected.",
                "Proceed to reviewer entry", "Cancel")) return;
            SparkNativeWirePlanApprovalWindow.ShowFor(plan);
        }

        [MenuItem(Prefix + "22 - Verify Auto Engineer Setup")]
        public static void Verify()
        {
            var execs = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var exec in execs)
                Debug.Log("[Spark Auto Engineer] " + exec.name + " autoEngineer=" +
                    (exec.GetComponent<SparkNativeWireAutoEngineer>() != null) +
                    " nativeDirector=" + (exec.GetComponent<SparkNativeWireDirector>() != null));
            var plans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
            int approved = 0;
            foreach (var plan in plans) if (plan && plan.IsApproved) approved++;
            Debug.Log("[Spark Auto Engineer] " + plans.Length + " plan assets, " + approved +
                " approved/digest-current. Editor check only: physical wiring must be tested in Play Mode.");
        }
    }

    internal sealed class SparkNativeWirePlanApprovalWindow : EditorWindow
    {
        private SparkNativeWiringPlan targetPlan;
        private string reviewer = "";
        private string schematic = "";
        public static void ShowFor(SparkNativeWiringPlan plan)
        {
            var window = GetWindow<SparkNativeWirePlanApprovalWindow>(true, "Spark Wiring Plan Approval", true);
            window.targetPlan = plan;
            window.minSize = new Vector2(540, 260);
            window.Show();
        }
        private void OnGUI()
        {
            if (!targetPlan) { EditorGUILayout.HelpBox("Select a plan asset.", MessageType.Warning); return; }
            EditorGUILayout.LabelField("Plan", targetPlan.name);
            EditorGUILayout.LabelField("Level", targetPlan.LevelNumber.ToString());
            EditorGUILayout.LabelField("Wire steps", targetPlan.Wires.Count.ToString());
            EditorGUILayout.HelpBox("Review wiring routes and all power-return/current-limiting constraints. " +
                "This is not permission to bypass exams or force level completion.", MessageType.Warning);
            reviewer = EditorGUILayout.TextField("Reviewed by", reviewer);
            EditorGUILayout.LabelField("Authored schematic / source reference");
            schematic = EditorGUILayout.TextArea(schematic, GUILayout.MinHeight(60));
            if (GUILayout.Button("Approve reviewed practice plan"))
            {
                Undo.RecordObject(targetPlan, "Approve Spark wiring plan");
                if (!targetPlan.TryApprove(reviewer, schematic, out string error))
                { EditorUtility.DisplayDialog("Not approved", error, "OK"); return; }
                EditorUtility.SetDirty(targetPlan);
                AssetDatabase.SaveAssets();
                Debug.Log("[Spark Auto Engineer] Reviewed practice plan approved: " + targetPlan.name);
                Close();
            }
        }
    }
}
#endif
