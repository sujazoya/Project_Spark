#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public sealed class SparkMasterTeacherAutoDashboard : EditorWindow
    {
        private readonly List<SparkProAutoGuide> results = new List<SparkProAutoGuide>();
        private Vector2 scroll;
        private string search = "";
        private int levelFilter;
        private int currentPage;
        private const int PageSize = 30;

        [MenuItem("Project Spark/Spark V5 Pro/Master Teacher/10 — Open AUTO Curriculum Dashboard")]
        private static void Open()
        {
            var win = GetWindow<SparkMasterTeacherAutoDashboard>("Spark Auto Curriculum");
            win.minSize = new Vector2(900, 520);
            win.Refresh();
            win.Show();
        }
        private void OnEnable() => Refresh();
        private void Refresh()
        {
            results.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:SparkProAutoGuide"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith(SparkMasterTeacherAutoGenerator.GuideRoot + "/", StringComparison.Ordinal)) continue;
                var guide = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(path);
                if (guide) results.Add(guide);
            }
            results.Sort((a, b) => a.level != b.level ? a.level.CompareTo(b.level) :
                string.CompareOrdinal(a.lessonId, b.lessonId));
            Repaint();
        }
        private void OnGUI()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("SPARK MASTER TEACHER — AUTO CURRICULUM", EditorStyles.boldLabel);
            int valid = results.Count(x => x && x.IsUsable(out _));
            EditorGUILayout.HelpBox($"Guided, ungraded objective walkthroughs: {valid} / 732 expected. " +
                "Human-reviewed graded lessons remain separate and take precedence. " +
                "Generated guides cannot award mission completion, Academy scores or certificates.",
                valid == 732 ? MessageType.Info : MessageType.Warning);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("AUTO BUILD ALL LEVELS", GUILayout.Height(32)))
                {
                    SparkMasterTeacherAutoGenerator.OneClick();
                    Refresh();
                }
                if (GUILayout.Button("Validate + HTML Report", GUILayout.Height(32)))
                    SparkMasterTeacherAutoGenerator.ValidateAndReport(true);
                if (GUILayout.Button("Integrity Tests", GUILayout.Height(32))) SparkMasterTeacherAutoTests.Run();
                if (GUILayout.Button("Refresh", GUILayout.Width(100), GUILayout.Height(32))) Refresh();
            }
            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                search = EditorGUILayout.TextField("Search title / objective", search);
                levelFilter = EditorGUILayout.IntSlider("Level (0 = all)", levelFilter, 0, 130);
            }
            var visible = results.Where(x => x && (levelFilter == 0 || levelFilter == x.level) &&
                (string.IsNullOrWhiteSpace(search) ||
                 (x.lessonId ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 (x.title ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            int pages = Mathf.Max(1, Mathf.CeilToInt(visible.Count / (float)PageSize));
            currentPage = Mathf.Clamp(currentPage, 0, pages - 1);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Showing {visible.Count} records · page {currentPage + 1}/{pages}");
                GUI.enabled = currentPage > 0;
                if (GUILayout.Button("Previous", GUILayout.Width(100))) currentPage--;
                GUI.enabled = currentPage < pages - 1;
                if (GUILayout.Button("Next", GUILayout.Width(100))) currentPage++;
                GUI.enabled = true;
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (SparkProAutoGuide guide in visible.Skip(currentPage * PageSize).Take(PageSize))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(guide.IsUsable(out _) ? "[GUIDED]" : "[INVALID]", GUILayout.Width(85));
                    EditorGUILayout.LabelField(guide.title, GUILayout.MinWidth(320));
                    EditorGUILayout.LabelField(guide.recipeId, GUILayout.Width(100));
                    if (GUILayout.Button("Inspect", GUILayout.Width(80))) Selection.activeObject = guide;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(4);
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Logs/SparkMasterTeacher/AutoCoverage.html");
            if (File.Exists(path))
            {
                if (GUILayout.Button("Open last HTML report")) EditorUtility.RevealInFinder(path);
            }
        }
    }
}
#endif
