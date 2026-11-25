#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>
    /// Imports *authored evidence* from the existing 130-level export into non-published review drafts.
    /// Running twice is safe. New content is never automatically approved or awarded as mastery.
    /// </summary>
    public static class SparkMasterTeacherImporter
    {
        public const string SourcePath = "Assets/ProjectSpark/SparkV5/Pro/Generated/AuthoredObjectiveCards.json";
        public const string DraftRoot = "Assets/ProjectSpark/SparkV5/Pro/MasterTeacher/Authoring/Drafts";
        public const string PublishedRoot = "Assets/ProjectSpark/SparkV5/Pro/Integration/Resources/SparkMasterLessons";
        public const string ReportRoot = "Logs/SparkMasterTeacher";
        public const string MenuRoot = "Project Spark/Spark V5 Pro/Master Teacher/";
        const string AutoFlag = "ProjectSpark.MasterTeacher.AutoReimport.v1";

        [Serializable] public sealed class Cards { public int schemaVersion; public Level[] levels; }
        [Serializable] public sealed class Level { public int level; public string source; public Objective[] objectives; }
        [Serializable] public sealed class Objective
        {
            public string id, title;
            public string[] concepts, prerequisites, explanation, visual, practice, assessment, misconceptions, evidence;
        }
        [Serializable] public sealed class ImportStats
        {
            public string generatedUtc;
            public int levels, objectives, created, updated, unchanged, invalidated, blocked;
            public string[] warnings = Array.Empty<string>();
        }
        private sealed class Row
        {
            public int level;
            public Objective objective;
            public string source;
            public string path;
        }

        [MenuItem(MenuRoot + "1 — Import/Update All Objective Drafts")]
        public static void ImportAllMenu() => ImportAll(true);

        [MenuItem(MenuRoot + "2 — Generate Coverage & Review Queue")]
        public static void AuditMenu() => CreateReports(true);

        [MenuItem(MenuRoot + "3 — Safe Auto Development (Import + Audit)")]
        public static void AutoDevelopment()
        {
            ImportAll(false);
            CreateReports(true);
        }

        [MenuItem(MenuRoot + "4 — Auto-import on Objective Changes [Toggle]")]
        public static void ToggleAuto()
        {
            bool next = !EditorPrefs.GetBool(AutoFlag, false);
            EditorPrefs.SetBool(AutoFlag, next);
            Debug.Log("[Master Teacher] Automatic draft refresh " + (next ? "enabled" : "disabled") + ". New or changed evidence still requires human approval.");
        }

        [MenuItem(MenuRoot + "4 — Auto-import on Objective Changes [Toggle]", true)]
        private static bool ToggleAutoValidation()
        {
            Menu.SetChecked(MenuRoot + "4 — Auto-import on Objective Changes [Toggle]", EditorPrefs.GetBool(AutoFlag, false));
            return true;
        }

        internal static bool AutoEnabled => EditorPrefs.GetBool(AutoFlag, false);

        public static bool TryReadCards(out Cards result, out string error)
        {
            result = null;
            error = null;
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), SourcePath);
            if (!File.Exists(fullPath)) { error = "Missing authoritative source: " + SourcePath; return false; }
            try
            {
                result = JsonUtility.FromJson<Cards>(File.ReadAllText(fullPath, Encoding.UTF8));
                if (result == null || result.schemaVersion != 1 || result.levels == null)
                    throw new InvalidDataException("Expected schemaVersion=1 and levels[].");
                var uniqueLevels = new HashSet<int>();
                foreach (Level level in result.levels)
                {
                    if (level == null || level.level < 1 || level.level > 130 || !uniqueLevels.Add(level.level))
                        throw new InvalidDataException("Duplicate/invalid level number in objective source.");
                    if (string.IsNullOrWhiteSpace(level.source) || !level.source.StartsWith("Assets/", StringComparison.Ordinal))
                        throw new InvalidDataException("Missing/invalid authored source at level " + level.level);
                    if (level.objectives == null || level.objectives.Length == 0)
                        throw new InvalidDataException("No authored objectives for level " + level.level);
                    var uniqueTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (Objective objective in level.objectives)
                        if (objective == null || string.IsNullOrWhiteSpace(objective.id) ||
                            !uniqueTargets.Add(objective.id))
                            throw new InvalidDataException("Invalid/duplicate objective ID at level " + level.level);
                }
                if (uniqueLevels.Count != 130)
                    throw new InvalidDataException("Expected 130 unique authored level records; found " + uniqueLevels.Count);
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException ||
                                               exception is InvalidDataException || exception is NullReferenceException)
            {
                error = exception.Message;
                result = null;
                return false;
            }
        }

        public static ImportStats ImportAll(bool generateReport)
        {
            if (!TryReadCards(out Cards cards, out string error))
            {
                Debug.LogError("[Master Teacher] Import cancelled before changes: " + error);
                return null;
            }
            var stats = new ImportStats { generatedUtc = DateTime.UtcNow.ToString("O") };
            stats.levels = cards.levels.Length;
            var warnings = new List<string>();
            var rows = Extract(cards);
            stats.objectives = rows.Count;
            EnsureFolder(DraftRoot);
            // Validate ALL source references before making any asset changes.
            foreach (Level level in cards.levels)
                if (!AssetDatabase.LoadMainAssetAtPath(level.source))
                    warnings.Add("Level " + level.level + " source asset is unavailable: " + level.source);

            // Folder creation is performed before StartAssetEditing so paths are immediately registered.
            for (int level = 1; level <= 130; level++)
                EnsureFolder(DraftRoot + "/Level_" + level.ToString("D3"));
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (Row row in rows)
                {
                    if (!AssetDatabase.LoadMainAssetAtPath(row.source))
                    { stats.blocked++; continue; }
                    string path = DraftPath(row.level, row.objective.id);
                    SparkProLessonDefinition draft = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(path);
                    bool isNew = !draft;
                    if (isNew)
                    {
                        if (AssetDatabase.LoadMainAssetAtPath(path))
                        { warnings.Add("Unexpected asset exists at " + path); stats.blocked++; continue; }
                        draft = ScriptableObject.CreateInstance<SparkProLessonDefinition>();
                        draft.lessonId = StableLessonId(row.level, row.objective.id);
                        draft.title = "Level " + row.level + " — " + SafeTitle(row.objective);
                        draft.aliases = new[] { SafeTitle(row.objective) };
                        draft.approvedForTeaching = false;
                        AssetDatabase.CreateAsset(draft, path);
                        stats.created++;
                    }
                    string newFingerprint = Fingerprint(row);
                    if (!isNew && draft.sourceFingerprint == newFingerprint)
                    { stats.unchanged++; continue; }
                    if (!isNew && !string.IsNullOrEmpty(draft.sourceFingerprint))
                    {
                        draft.approvedForTeaching = false;
                        draft.reviewedUtc = "";
                        draft.reviewedBy = "";
                        stats.invalidated++;
                        InvalidatePublished(draft.lessonId, warnings);
                    }
                    // These are source references ONLY. Never replace manually edited teaching text or rubrics.
                    draft.sourceFingerprint = newFingerprint;
                    draft.authoredObjectiveExcerpt = Join(row.objective.explanation);
                    draft.authoredVisualReferences = row.objective.visual ?? Array.Empty<string>();
                    draft.authoredPracticeReferences = row.objective.practice ?? Array.Empty<string>();
                    draft.authoredAssessmentReferences = row.objective.assessment ?? Array.Empty<string>();
                    draft.authoredPrerequisites = row.objective.prerequisites ?? Array.Empty<string>();
                    draft.suggestedConceptTerms = row.objective.concepts ?? Array.Empty<string>();
                    draft.sourceReferences = row.objective.evidence != null && row.objective.evidence.Length > 0 ?
                        row.objective.evidence : new[] { row.source + "#" + row.objective.id };
                    draft.applicableLevels = new[] { row.level };
                    draft.objectiveIds = new[] { row.objective.id };
                    if (!isNew) stats.updated++;
                    EditorUtility.SetDirty(draft);
                }
            }
            catch (Exception exception)
            {
                warnings.Add("Importer exception (inspect draft assets before continuing): " + exception);
                Debug.LogException(exception);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            stats.warnings = warnings.ToArray();
            SaveStats(stats);
            Debug.Log($"[Master Teacher] Objective imports: {stats.levels} levels, {stats.objectives} objectives, " +
                $"{stats.created} created, {stats.updated} refreshed, {stats.unchanged} unchanged, " +
                $"{stats.invalidated} re-review required, {stats.blocked} blocked. See {ReportRoot}.");
            if (generateReport) CreateReports(false);
            return stats;
        }

        private static void InvalidatePublished(string lessonId, List<string> warnings)
        {
            string destination = PublishedPath(lessonId);
            var published = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(destination);
            if (!published) return;
            published.approvedForTeaching = false;
            published.reviewedBy = "";
            published.reviewedUtc = "";
            EditorUtility.SetDirty(published);
            warnings.Add("Published lesson invalidated after source change: " + destination);
        }

        public static void Publish(SparkProLessonDefinition draft)
        {
            if (!draft) { Debug.LogError("[Master Teacher] Select an authored lesson draft."); return; }
            string path = AssetDatabase.GetAssetPath(draft);
            if (!path.StartsWith(DraftRoot + "/", StringComparison.Ordinal))
            { Debug.LogError("[Master Teacher] Only reviewed source drafts can be published."); return; }
            if (!draft.IsReady(out string problem) || string.IsNullOrWhiteSpace(draft.reviewedBy))
            { Debug.LogError("[Master Teacher] Review and approve the lesson first: " + (problem ?? "reviewedBy missing")); return; }
            if (!TryReadCards(out Cards cards, out string error))
            { Debug.LogError("[Master Teacher] Cannot verify evidence source: " + error); return; }
            Row row = Extract(cards).Find(x => StableLessonId(x.level, x.objective.id) == draft.lessonId);
            if (row == null || draft.sourceFingerprint != Fingerprint(row) || !AssetDatabase.LoadMainAssetAtPath(row.source))
            { Debug.LogError("[Master Teacher] Authoritative source is missing or has changed. Re-import and re-review."); return; }
            EnsureFolder(PublishedRoot);
            string destination = PublishedPath(draft.lessonId);
            var published = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(destination);
            if (!published)
            {
                if (AssetDatabase.LoadMainAssetAtPath(destination))
                    throw new InvalidDataException("Another asset is occupying " + destination);
                published = ScriptableObject.CreateInstance<SparkProLessonDefinition>();
                AssetDatabase.CreateAsset(published, destination);
            }
            Undo.RecordObject(published, "Publish reviewed Spark teaching lesson");
            EditorUtility.CopySerialized(draft, published);
            published.reviewedUtc = DateTime.UtcNow.ToString("O");
            published.authoredRevision = Math.Max(1, draft.authoredRevision + 1);
            draft.authoredRevision = published.authoredRevision;
            draft.reviewedUtc = published.reviewedUtc;
            EditorUtility.SetDirty(published);
            EditorUtility.SetDirty(draft);
            AssetDatabase.SaveAssets();
            Debug.Log("[Master Teacher] Published reviewed lesson: " + published.lessonId + " at " + destination +
                ". Restart Play Mode to reload runtime catalog.");
        }

        public static void Revoke(SparkProLessonDefinition draft)
        {
            if (!draft) return;
            var published = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(PublishedPath(draft.lessonId));
            if (published)
            {
                Undo.RecordObject(published, "Revoke Spark teaching lesson");
                published.approvedForTeaching = false;
                EditorUtility.SetDirty(published);
            }
            Undo.RecordObject(draft, "Revoke Spark teaching lesson");
            draft.approvedForTeaching = false;
            EditorUtility.SetDirty(draft);
            AssetDatabase.SaveAssets();
        }

        public static void CreateReports(bool reveal)
        {
            if (!TryReadCards(out Cards cards, out string error))
            { Debug.LogError("[Master Teacher] Report unavailable: " + error); return; }
            var rows = Extract(cards);
            string root = Path.Combine(Path.GetDirectoryName(Application.dataPath), ReportRoot);
            Directory.CreateDirectory(root);
            var index = AssetDatabase.FindAssets("t:SparkProLessonDefinition").Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => (path: p, asset: AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(p)))
                .Where(x => x.asset).ToArray();
            var published = index.Where(x => x.path.StartsWith(PublishedRoot + "/", StringComparison.Ordinal))
                .Select(x => x.asset).Where(x => x.IsReady(out _) && !string.IsNullOrWhiteSpace(x.reviewedBy)).ToArray();
            var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicatesFound = new List<string>();
            foreach (var item in published)
                if (!duplicates.Add(item.lessonId)) duplicatesFound.Add(item.lessonId);
            var csv = new StringBuilder("Level,ObjectiveId,Title,Source,HasExplanation,HasVisual,HasPractice,HasAssessment,HasMisconception,ApprovedLesson,Status\n");
            var reviewCsv = new StringBuilder("Priority,Level,ObjectiveId,Title,Status,DraftPath,Source\n");
            int covered = 0, draftCount = 0, stale = 0, noAssessment = 0, noMisconception = 0;
            var htmlRows = new StringBuilder();
            foreach (var row in rows)
            {
                string id = StableLessonId(row.level, row.objective.id);
                var lesson = published.FirstOrDefault(x => x.lessonId == id && x.CoversObjective(row.level, row.objective.id));
                bool validSource = AssetDatabase.LoadMainAssetAtPath(row.source);
                bool isStale = lesson && lesson.sourceFingerprint != Fingerprint(row);
                bool approved = validSource && lesson && !isStale;
                if (approved) covered++;
                if (isStale) stale++;
                if (!Has(row.objective.assessment)) noAssessment++;
                if (!Has(row.objective.misconceptions)) noMisconception++;
                string draftPath = DraftPath(row.level, row.objective.id);
                bool isDraft = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(draftPath);
                if (isDraft) draftCount++;
                string status = approved ? "Reviewed/published" : isStale ? "Published source stale" :
                    !validSource ? "Authored source missing" : !isDraft ? "Draft missing" : "Draft requires review";
                csv.AppendLine(string.Join(",", new[] { Csv(row.level.ToString()), Csv(row.objective.id), Csv(SafeTitle(row.objective)),
                    Csv(row.source), Csv(Has(row.objective.explanation) ? "1" : "0"),
                    Csv(Has(row.objective.visual) ? "1" : "0"), Csv(Has(row.objective.practice) ? "1" : "0"),
                    Csv(Has(row.objective.assessment) ? "1" : "0"), Csv(Has(row.objective.misconceptions) ? "1" : "0"),
                    Csv(approved ? id : ""), Csv(status) }));
                if (!approved)
                    reviewCsv.AppendLine(string.Join(",", new[] { Csv(row.level >= 110 ? "1" : "2"), Csv(row.level.ToString()),
                        Csv(row.objective.id), Csv(SafeTitle(row.objective)), Csv(status), Csv(draftPath), Csv(row.source) }));
                htmlRows.Append("<tr><td>").Append(row.level).Append("</td><td>").Append(Html(row.objective.id))
                    .Append("</td><td>").Append(Html(SafeTitle(row.objective))).Append("</td><td>")
                    .Append(Html(status)).AppendLine("</td></tr>");
            }
            File.WriteAllText(Path.Combine(root, "TeachingCoverage.csv"), csv.ToString(), new UTF8Encoding(true));
            File.WriteAllText(Path.Combine(root, "ReviewQueue.csv"), reviewCsv.ToString(), new UTF8Encoding(true));
            string summary = $"SPARK MASTER TEACHER AUDIT — {DateTime.UtcNow:O}\n" +
                $"Authored levels: {cards.levels.Length}\nAuthored objectives: {rows.Count}\nDraft assets: {draftCount}\n" +
                $"Approved and source-current objective lessons: {covered}\nObjectives needing review: {rows.Count - covered}\n" +
                $"Stale published mappings: {stale}\nAuthored objectives without assessment links: {noAssessment}\n" +
                $"Authored objectives without misconception links: {noMisconception}\nDuplicate published lesson IDs: " +
                (duplicatesFound.Count == 0 ? "none" : string.Join(";", duplicatesFound)) + "\n" +
                "Level catalog inclusion is not equivalent to completed lessons. Approval is mandatory.\n" +
                "No lesson asset grants a mission win, practical exam pass or certificate.\n";
            File.WriteAllText(Path.Combine(root, "Validation.txt"), summary, Encoding.UTF8);
            string html = "<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>Spark Master Teacher</title>" +
                "<style>body{font:15px Segoe UI,Arial,sans-serif;background:#101923;color:#eff4f7;max-width:1100px;margin:35px auto;padding:0 15px}" +
                "h1{color:#60d8e8}table{width:100%;border-collapse:collapse}td,th{padding:9px;border-bottom:1px solid #304153;text-align:left}" +
                "th{background:#233749;position:sticky;top:0}.tag{color:#60d8e8}small{color:#b3c3cf}</style></head><body>" +
                "<h1>SPARK Master Teacher — Authoring Audit</h1><p>Authored levels: " + cards.levels.Length +
                " · Objective cards: " + rows.Count + " · Reviewed &amp; source-current lessons: <span class=tag>" + covered +
                "</span> · Drafts: " + draftCount + "</p><p><small>Draft or published without review does not count as coverage. " +
                "The game evaluator remains authoritative.</small></p><table><thead><tr><th>Level</th><th>Objective ID</th>" +
                "<th>Title</th><th>Teaching status</th></tr></thead><tbody>" + htmlRows + "</tbody></table></body></html>";
            File.WriteAllText(Path.Combine(root, "TeachingCoverage.html"), html, Encoding.UTF8);
            Debug.Log("[Master Teacher] " + summary + "Reports: " + root);
            if (reveal) EditorUtility.RevealInFinder(Path.Combine(root, "TeachingCoverage.html"));
        }

        private static List<Row> Extract(Cards source)
        {
            var rows = new List<Row>();
            foreach (Level level in source.levels)
                foreach (Objective objective in level.objectives)
                    rows.Add(new Row { level = level.level, objective = objective, source = level.source,
                        path = DraftPath(level.level, objective.id) });
            return rows;
        }
        private static string Join(string[] terms) => terms == null ? "" : string.Join("\n", terms.Where(x => !string.IsNullOrWhiteSpace(x)));
        private static bool Has(string[] x) => x != null && x.Any(y => !string.IsNullOrWhiteSpace(y));
        private static string SafeTitle(Objective objective) => string.IsNullOrWhiteSpace(objective.title) ? objective.id : objective.title.Trim();
        private static string Csv(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        private static string Html(string value) => (value ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        public static string StableLessonId(int level, string objectiveId) => "L" + level.ToString("D3") + "/" + objectiveId;
        public static string DraftPath(int level, string objectiveId) => DraftRoot + "/Level_" + level.ToString("D3") +
            "/Lesson_" + FileToken(objectiveId) + "_" + Hash8(objectiveId) + ".asset";
        public static string PublishedPath(string lessonId) => PublishedRoot + "/Lesson_" + FileToken(lessonId) +
            "_" + Hash8(lessonId) + ".asset";
        private static string FileToken(string value) => Regex.Replace(value ?? "", @"[^a-zA-Z0-9_-]", "_").Substring(0,
            Math.Min(40, Regex.Replace(value ?? "", @"[^a-zA-Z0-9_-]", "_").Length));
        private static string Hash8(string text) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""))).Replace("-", "").Substring(0, 8); }
        private static string Fingerprint(Row row) => Hash8(JsonUtility.ToJson(new FingerprintData
        {
            level = row.level, id = row.objective.id, title = row.objective.title, source = row.source,
            concepts = row.objective.concepts, prerequisites = row.objective.prerequisites,
            explanation = row.objective.explanation, visual = row.objective.visual, practice = row.objective.practice,
            assessment = row.objective.assessment, misconceptions = row.objective.misconceptions, evidence = row.objective.evidence
        }));
        [Serializable] sealed class FingerprintData
        {
            public int level;
            public string id, title, source;
            public string[] concepts, prerequisites, explanation, visual, practice, assessment, misconceptions, evidence;
        }
        private static void EnsureFolder(string path)
        {
            string cursor = "Assets";
            foreach (string part in path.Substring("Assets/".Length).Split('/'))
            {
                string target = cursor + "/" + part;
                if (!AssetDatabase.IsValidFolder(target)) AssetDatabase.CreateFolder(cursor, part);
                cursor = target;
            }
        }
        private static void SaveStats(ImportStats stats)
        {
            string directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), ReportRoot);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "ImportSummary.json"), JsonUtility.ToJson(stats, true), Encoding.UTF8);
        }
    }

    // Disabled by default: no automated authoring or approval without explicit opt-in.
    internal sealed class SparkMasterTeacherSourceWatcher : AssetPostprocessor
    {
        private static bool pending;
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] from)
        {
            if (!SparkMasterTeacherImporter.AutoEnabled || pending || EditorApplication.isPlayingOrWillChangePlaymode ||
                !imported.Contains(SparkMasterTeacherImporter.SourcePath)) return;
            pending = true;
            EditorApplication.delayCall += () =>
            {
                pending = false;
                if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
                    SparkMasterTeacherImporter.ImportAll(true);
            };
        }
    }
}
#endif
