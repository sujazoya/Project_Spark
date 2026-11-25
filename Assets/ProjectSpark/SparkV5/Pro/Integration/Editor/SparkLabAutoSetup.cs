#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;
 using  ProjectSpark.Gameplay;


namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>One-click, idempotent setup of the passive inspector. Does not alter gameplay bindings.</summary>
    public static class SparkLabAutoSetup
    {
        private const string PrefabPath = "Assets/ProjectSpark/SparkV5/Pro/Generated/SparkV5Pro.prefab";
        private const string MenuBase = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        private static int Bind(GameObject root, bool allowSceneReferences)
        {
            if (!root) return 0;
            var executive = root.GetComponentInChildren<SparkProExecutive>(true);
            if (!executive) return 0;
            var director = executive.GetComponent<SparkLabTeachingDirector>();
            bool added = !director;
            if (!director) director = allowSceneReferences ? Undo.AddComponent<SparkLabTeachingDirector>(executive.gameObject) :
                executive.gameObject.AddComponent<SparkLabTeachingDirector>();
            var so = new SerializedObject(director);
            var ex = so.FindProperty("executive");
            if (ex != null) ex.objectReferenceValue = executive;
            var ch = so.FindProperty("character");
            if (ch != null && !ch.objectReferenceValue)
                ch.objectReferenceValue = executive.GetComponent<SparkProCharacter>() ?? executive.GetComponentInParent<SparkProCharacter>();
            var sel = so.FindProperty("selection");
            if (allowSceneReferences && sel != null && !sel.objectReferenceValue)
            {
                // Reference only if unambiguous in the active scene. Prefabs may intentionally remain unbound.
                var selections = UnityEngine.Object.FindObjectsByType<SparkSelectionController>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (selections.Length == 1 && !EditorUtility.IsPersistent(selections[0]))
                    sel.objectReferenceValue = selections[0];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(director);
            return added ? 1 : 0;
        }

        [MenuItem(MenuBase + "1 - AUTO Bind Prefab and Open Scenes")]
        public static void BindAll()
        {
            int bound = 0;
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (source)
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string sourceFile = Path.Combine(projectRoot, PrefabPath);
                string backupFolder = Path.Combine(Directory.GetParent(projectRoot).FullName,
                    "AI", "SparkLab_Backups", "Prefab_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
                Directory.CreateDirectory(backupFolder);
                File.Copy(sourceFile, Path.Combine(backupFolder, "SparkV5Pro.prefab"), false);
                var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
                try { bound += Bind(contents, false); PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath); }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            else Debug.LogWarning("[Spark Lab V6] Known character prefab not found at " + PrefabPath + ". Scene binding still attempted.");

            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < executives.Length; i++)
            {
                var executive = executives[i];
                if (!executive || EditorUtility.IsPersistent(executive)) continue;
                bound += Bind(executive.gameObject, true);
                if (executive.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(executive.gameObject.scene);
            }
            Debug.Log("[Spark Lab V6] Auto bind done. Added " + bound +
                " directors. Review and SAVE modified scenes. No mission, circuit or level data was changed.");
        }

        [MenuItem(MenuBase + "2 - Verify Package and Prefab")]
        public static void Verify()
        {
            int passed = 0;
            void Check(bool value, string label)
            { if (!value) throw new InvalidOperationException("[Spark Lab V6] FAIL: " + label); passed++; }
            Check(SparkLabFocusIntent.Classify("Show me which connection I should inspect next") ==
                SparkLabFocusIntent.Kind.NextConnection, "next connection command");
            Check(SparkLabFocusIntent.Classify("Point to the anode") ==
                SparkLabFocusIntent.Kind.PositiveTerminal, "anode command");
            Check(SparkLabFocusIntent.Classify("Point to the cathode") ==
                SparkLabFocusIntent.Kind.NegativeTerminal, "cathode command");
            Check(SparkLabFocusIntent.Classify("Highlight selected component") ==
                SparkLabFocusIntent.Kind.Selected, "selected command");
            Check(SparkLabFocusIntent.Classify("Clear the highlight") ==
                SparkLabFocusIntent.Kind.Clear, "clear command");
            Check(SparkLabFocusIntent.Classify("Teach me Level 1") ==
                SparkLabFocusIntent.Kind.None, "lesson does not trigger visual action");
            Check(!SparkLabFocusIntent.MayDraw(true,false,true,true,
                SparkLabFocusIntent.Kind.NextConnection), "exam restriction");
            Check(!SparkLabFocusIntent.MayDraw(false,true,true,true,
                SparkLabFocusIntent.Kind.NextConnection), "warning restriction");
            Check(!SparkLabFocusIntent.MayDraw(false,false,true,false,
                SparkLabFocusIntent.Kind.NextConnection), "stale evidence restriction");
            Check(!SparkLabFocusIntent.MayDraw(false,false,false,true,
                SparkLabFocusIntent.Kind.Selected), "layout restriction");
            Check(SparkLabFocusIntent.MayDraw(false,false,true,true,
                SparkLabFocusIntent.Kind.Selected), "fresh target permitted");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab)
            {
                Check(prefab.GetComponentInChildren<SparkProExecutive>(true), "character prefab has executive");
                var inspector = prefab.GetComponentInChildren<SparkLabTeachingDirector>(true);
                Check(inspector, "prefab has Lab Director after auto bind");
            }
            Debug.Log("[Spark Lab V6] PASS " + passed +
                " Editor checks. Scene runtime pointing and beacon display still require Play Mode tests.");
        }

        [MenuItem(MenuBase + "3 - Open Setup Readme")]
        public static void OpenReadme()
        {
            Debug.Log("[Spark Lab V6] Use menu 1 for prefab/scene binding, menu 2 for checks. " +
                "Test pointing with an active laboratory level and fresh evaluator. " +
                "No microphone test is required.");
        }
    }
}
#endif
