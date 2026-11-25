#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor-only setup and schematic-reviewed practice-plan authoring.</summary>
    public static class SparkLabAgentEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        private const string Prefab = "Assets/ProjectSpark/SparkV5/Pro/Generated/SparkV5Pro.prefab";
        private const string PlanFolder = "Assets/ProjectSpark/SparkV5/Pro/Integration/Resources/SparkAgentPlans";

        [MenuItem(Menu + "9 - Bind Action Agent (Prefab + Open Scenes)")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogError("[Spark Agent] Exit Play Mode before auto-binding."); return; }
            int added = 0;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (prefab)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(Prefab);
                try
                {
                    SparkProExecutive executive = root.GetComponentInChildren<SparkProExecutive>(true);
                    if (executive && !executive.GetComponent<SparkLabAutonomyAgent>())
                    {
                        string rootProject = Directory.GetParent(Application.dataPath).FullName;
                        string backup = Path.Combine(rootProject, "Logs", "SparkAgentBackups", "Prefab_" +
                            DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
                        Directory.CreateDirectory(backup);
                        File.Copy(Path.Combine(rootProject, Prefab), Path.Combine(backup, "SparkV5Pro.prefab"));
                        executive.gameObject.AddComponent<SparkLabAutonomyAgent>();
                        PrefabUtility.SaveAsPrefabAsset(root, Prefab);
                        added++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            else Debug.LogWarning("[Spark Agent] Optional character prefab not found at " + Prefab);
            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var executive in executives)
            {
                if (!executive || EditorUtility.IsPersistent(executive) ||
                    executive.GetComponent<SparkLabAutonomyAgent>()) continue;
                Undo.AddComponent<SparkLabAutonomyAgent>(executive.gameObject);
                if (executive.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(executive.gameObject.scene);
                added++;
            }
            Debug.Log("[Spark Agent] Agent bound to " + added + " prefab/scene object(s). Save modified scenes.");
        }

        [MenuItem(Menu + "10 - Create Unapproved Plan for Level 24")]
        public static void CreateLevel24() { CreatePlan(24); }

        [MenuItem(Menu + "11 - Generate 130 Unapproved Plan Templates")]
        public static void GenerateTemplates()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogError("[Spark Agent] Exit Play Mode first."); return; }
            if (!EditorUtility.DisplayDialog("Create plan templates?",
                "Create up to 130 EMPTY UNAPPROVED practice plans? They cannot auto-solve levels until real wiring routes are added and reviewed.",
                "Create templates", "Cancel")) return;
            int count = 0;
            for (int level = 1; level <= 130; level++) if (CreatePlan(level, false)) count++;
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Agent] Created " + count +
                " UNAPPROVED plan templates. This is NOT 130 solved levels.");
        }

        private static void CreatePlan(int number) { CreatePlan(number, true); }
        private static bool CreatePlan(int number, bool select)
        {
            EnsureFolder();
            string path = PlanFolder + "/L" + number.ToString("000") + "_AssistedPlan.asset";
            var existing = AssetDatabase.LoadAssetAtPath<SparkLabActionPlan>(path);
            if (existing)
            {
                if (select) Selection.activeObject = existing;
                return false;
            }
            var asset = ScriptableObject.CreateInstance<SparkLabActionPlan>();
            asset.levelNumber = number;
            asset.schematicReference = "REVIEW REQUIRED: cite the actual authored schematic before approval";
            AssetDatabase.CreateAsset(asset, path);
            if (select) { Selection.activeObject = asset; EditorGUIUtility.PingObject(asset); }
            return true;
        }

        private static void EnsureFolder()
        {
            const string integration = "Assets/ProjectSpark/SparkV5/Pro/Integration";
            string resources = integration + "/Resources";
            if (!AssetDatabase.IsValidFolder(resources)) AssetDatabase.CreateFolder(integration, "Resources");
            if (!AssetDatabase.IsValidFolder(PlanFolder)) AssetDatabase.CreateFolder(resources, "SparkAgentPlans");
        }

        [MenuItem(Menu + "12 - Verify Action Command Routing")]
        public static void Verify()
        {
            string[] commands = {
                "draw wire from Battery_positive to SwitchInput",
                "confirm wire", "undo last AI wire", "auto solve practice level",
                "preview level solution", "stop auto solve", "agent status", "show next objective in 3d"
            };
            for (int i = 0; i < commands.Length; i++)
                if (SparkLabActionCommand.Parse(commands[i]).Type == SparkLabActionCommand.Kind.None)
                    throw new InvalidOperationException("[Spark Agent] Missing command " + commands[i]);
            if (SparkLabActionCommand.Parse("Teach me Level 24").Type != SparkLabActionCommand.Kind.None)
                throw new InvalidOperationException("Ordinary lesson intercepted.");
            if (SparkLabActionCommand.Parse("point to cathode").Type != SparkLabActionCommand.Kind.None)
                throw new InvalidOperationException("Existing lab visual command intercepted.");
            Debug.Log("[Spark Agent V8] PASS 10 routing checks. Actual wiring and plan execution need Play Mode tests.");
        }
    }

    [CustomEditor(typeof(SparkLabActionPlan))]
    public sealed class SparkLabActionPlanInspector : UnityEditor.Editor
    {
        private string reviewer = "";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var plan = (SparkLabActionPlan)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(plan.IsApproved
                ? "REVIEWED: the plan fingerprint matches the current steps. Only assisted practice may execute it."
                : "UNAPPROVED: Spark cannot execute this plan. Verify exact wiring against the authored schematic, safety and terminal IDs.",
                plan.IsApproved ? MessageType.Info : MessageType.Warning);
            reviewer = EditorGUILayout.TextField("Reviewed by", reviewer);
            using (new EditorGUI.DisabledScope(reviewer.Trim().Length == 0 || plan.steps.Count == 0 ||
                string.IsNullOrWhiteSpace(plan.schematicReference)))
            {
                if (GUILayout.Button("Approve reviewed schematic plan"))
                {
                    if (EditorUtility.DisplayDialog("Approve practice actions?",
                        "I have checked each direct wire route against the authored schematic and confirm this is assisted practice, NOT an independent exam.",
                        "Approve", "Cancel"))
                    { Undo.RecordObject(plan, "Approve Spark practice plan"); plan.ReviewAndApprove(reviewer); AssetDatabase.SaveAssets(); }
                }
            }
            if (GUILayout.Button("Revoke plan approval"))
            { Undo.RecordObject(plan, "Revoke Spark practice plan"); plan.Revoke(); AssetDatabase.SaveAssets(); }
        }
    }
}
#endif
