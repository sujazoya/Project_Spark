#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Human-review-first Academy authoring. Never auto-approves lessons.</summary>
    public sealed class SparkMasterTeacherStudio : EditorWindow
    {
        private string query = "";
        private int levelFilter;
        private int page;
        private Vector2 listScroll, detailScroll;
        private SparkProLessonDefinition selected;
        private UnityEditor.Editor selectedEditor;
        private List<(string path, SparkProLessonDefinition lesson)> entries = new();
        private const int PageSize = 18;

        [MenuItem(SparkMasterTeacherImporter.MenuRoot + "0 — Open AAA Authoring Studio")]
        public static void Open()
        {
            var window = GetWindow<SparkMasterTeacherStudio>("SPARK Master Teacher");
            window.minSize = new Vector2(880, 520);
            window.Refresh();
            window.Show();
        }

        private void OnEnable() => Refresh();
        private void OnDisable() { if (selectedEditor) DestroyImmediate(selectedEditor); }

        private void Refresh()
        {
            entries = AssetDatabase.FindAssets("t:SparkProLessonDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith(SparkMasterTeacherImporter.DraftRoot + "/", StringComparison.Ordinal))
                .Select(p => (path: p, lesson: AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(p)))
                .Where(x => x.lesson != null)
                .OrderBy(x => x.path, StringComparer.Ordinal).ToList();
            Repaint();
        }

        private void Select(SparkProLessonDefinition lesson)
        {
            if (selectedEditor) DestroyImmediate(selectedEditor);
            selected = lesson;
            selectedEditor = selected ? UnityEditor.Editor.CreateEditor(selected) : null;
            Selection.activeObject = selected;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("SPARK MASTER TEACHER · AAA AUTHORING", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Objective files are imported as unapproved drafts. Human-authored teaching, verified rubrics, and explicit review are required before a lesson is available in the game.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import 130-level objective source", GUILayout.Height(28)))
                { SparkMasterTeacherImporter.ImportAll(true); Refresh(); }
                if (GUILayout.Button("Generate review report", GUILayout.Height(28))) SparkMasterTeacherImporter.CreateReports(true);
                if (GUILayout.Button("Refresh", GUILayout.Width(90), GUILayout.Height(28))) Refresh();
            }
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                query = EditorGUILayout.TextField("Search", query);
                levelFilter = EditorGUILayout.IntSlider("Level (0 = all)", levelFilter, 0, 130);
            }
            var matching = entries.Where(x => (levelFilter == 0 || (x.lesson.applicableLevels != null &&
                    Array.IndexOf(x.lesson.applicableLevels, levelFilter) >= 0)) &&
                (string.IsNullOrWhiteSpace(query) ||
                 (x.lesson.title ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 (x.lesson.lessonId ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            int pages = Mathf.Max(1, Mathf.CeilToInt(matching.Count / (float)PageSize));
            page = Mathf.Clamp(page, 0, pages - 1);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Drafts: {matching.Count}  |  Showing page {page + 1} / {pages}", EditorStyles.miniBoldLabel);
                GUI.enabled = page > 0;
                if (GUILayout.Button("Previous", GUILayout.Width(95))) page--;
                GUI.enabled = page < pages - 1;
                if (GUILayout.Button("Next", GUILayout.Width(95))) page++;
                GUI.enabled = true;
            }
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.41f)))
                {
                    listScroll = EditorGUILayout.BeginScrollView(listScroll);
                    foreach (var row in matching.Skip(page * PageSize).Take(PageSize))
                    {
                        bool published = row.lesson.approvedForTeaching && row.lesson.IsReady(out _) &&
                            !string.IsNullOrWhiteSpace(row.lesson.reviewedBy);
                        string label = (published ? "[READY] " : "[DRAFT] ") + row.lesson.lessonId + " · " + row.lesson.title;
                        if (GUILayout.Toggle(selected == row.lesson, label, "Button", GUILayout.Height(28)))
                        { if (selected != row.lesson) Select(row.lesson); }
                    }
                    EditorGUILayout.EndScrollView();
                }
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    if (!selected) { EditorGUILayout.HelpBox("Select a draft on the left. Use the Inspector to edit reviewed learning content and answer rubrics.", MessageType.Info); return; }
                    EditorGUILayout.LabelField(selected.title, EditorStyles.boldLabel);
                    detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
                    EditorGUILayout.HelpBox("AUTHORED SOURCE (reference, not a verified lesson):\n" +
                        (selected.authoredObjectiveExcerpt ?? "No source explanation"), MessageType.None);
                    if (!selectedEditor) selectedEditor = UnityEditor.Editor.CreateEditor(selected);
                    selectedEditor.OnInspectorGUI();
                    if (selected.IsReady(out string reason) && !string.IsNullOrWhiteSpace(selected.reviewedBy))
                        EditorGUILayout.HelpBox("Teaching content and review metadata are ready for publication. Verify each answer rubric first.", MessageType.Info);
                    else EditorGUILayout.HelpBox("Not publishable yet: " + (reason ?? "Reviewer name missing"), MessageType.Warning);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Publish reviewed lesson", GUILayout.Height(30)))
                            SparkMasterTeacherImporter.Publish(selected);
                        if (GUILayout.Button("Revoke", GUILayout.Width(95), GUILayout.Height(30)))
                            SparkMasterTeacherImporter.Revoke(selected);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }
    }
}
#endif
