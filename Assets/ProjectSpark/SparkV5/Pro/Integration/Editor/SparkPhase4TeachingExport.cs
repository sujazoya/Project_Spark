using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkPhase4TeachingExport
    {
        internal const string AuditPath = "Logs/SparkPhase4/AuthoredTeachingExport.json";
        internal const string LessonPath = SparkProKnowledgeCompiler.Root + "/AuthoredObjectiveCards.json";

        [Serializable] private sealed class Export { public int schemaVersion = 1; public Level[] levels; }
        [Serializable] private sealed class Level { public int level; public string source; public Objective[] objectives; }
        [Serializable] private sealed class Objective
        {
            public string id, title;
            public string[] concepts, prerequisites, explanation, visual, practice, assessment, misconceptions, evidence;
        }

        [MenuItem("Project Spark/Spark V5 Pro/Phase 4/Export Authored Teaching Coverage")]
        internal static void ExportMenu() => Run(SparkProKnowledgeCompiler.CompileCatalog(false), Debug.Log);

        internal static void Run(SparkProGameCatalog catalog, Action<string> log)
        {
            var banks = AssetDatabase.FindAssets("t:SparkAIAssessmentBank").Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => (path, bank: AssetDatabase.LoadAssetAtPath<SparkAIAssessmentBank>(path)))
                .Where(item => item.bank).ToArray();
            var drawings = AssetDatabase.FindAssets("t:SparkLevelDrawingAsset").Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => (path, drawing: AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(path)))
                .Where(item => item.drawing && item.drawing.PageCount > 0)
                .ToLookup(item => item.drawing.levelId, StringComparer.OrdinalIgnoreCase);
            var levels = new List<Level>();
            int physicalLayouts = 0;
            foreach (var record in catalog.levels)
            {
                var asset = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(record.sourcePath);
                if (!asset || asset.LevelNumber != record.levelNumber || asset.LevelId != record.id ||
                    AssetDatabase.AssetPathToGUID(record.sourcePath) != record.sourceGuid || !record.inCampaign)
                    throw new InvalidDataException($"Level {record.levelNumber} source identity failed.");
                var layout = string.IsNullOrWhiteSpace(record.layoutPath) ? null :
                    AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(record.layoutPath);
                bool linked = layout && layout.LevelDefinition == asset && layout.SceneBindings;
                if (linked) physicalLayouts++;
                var targets = new List<Objective>();
                var identities = new HashSet<string>();
                foreach (var target in asset.Targets ?? Array.Empty<SparkLevelTarget>())
                {
                    if (target == null || string.IsNullOrWhiteSpace(target.TargetId) || !identities.Add(target.TargetId))
                        throw new InvalidDataException($"Level {record.levelNumber} has an invalid objective identity.");
                    string explanation = string.Join(" ", new[] { target.Description, SparkAILevelContext.TargetConditions(target) }
                        .Where(text => !string.IsNullOrWhiteSpace(text)));
                    var assessments = banks.SelectMany(b => (b.bank.items ?? Array.Empty<SparkAssessmentItem>())
                        .Where(item => item != null && item.kind == SparkAssessmentItemKind.VerifiedPractical &&
                            item.requiredLevelId == record.id && item.requiredObjectiveId == target.TargetId &&
                            item.requiredTargetType == target.Type).Select(item => b.path + "#" + item.itemId)).ToArray();
                    string reference = record.sourcePath + "#" + target.TargetId;
                    var pages = drawings[record.id].SelectMany(item => item.drawing.pages
                        .Select((page, index) => (page, index))
                        .Where(entry => entry.page != null && entry.page.parts != null && entry.page.parts.Length > 0 &&
                            string.Equals(entry.page.title?.Trim(), SparkAILevelContext.TargetName(target).Trim(), StringComparison.OrdinalIgnoreCase))
                        .Select(entry => item.path + "#page-" + (entry.index + 1))).ToList();
                    if (linked && target.IsConfigured())
                        pages.Add(reference + " → native objective whiteboard (criteria, no inferred wiring)");
                    targets.Add(new Objective
                    {
                        id = target.TargetId, title = SparkAILevelContext.TargetName(target),
                        // Index terms are level-wide discovery hints. They are not authored per-objective concept mappings.
                        concepts = Array.Empty<string>(),
                        prerequisites = record.previousLevel > 0 ? new[] { $"Campaign order: Level {record.previousLevel}; chapter access: {record.accessRule}" } :
                            record.chapterOpen ? new[] { "Authored campaign entry: chapter open by default." } : Array.Empty<string>(),
                        explanation = string.IsNullOrWhiteSpace(explanation) ? Array.Empty<string>() : new[] { explanation },
                        visual = pages.ToArray(),
                        practice = linked && target.IsConfigured() ? new[] { record.layoutPath + " → native mission practice" } : Array.Empty<string>(),
                        assessment = assessments,
                        misconceptions = Array.Empty<string>(),
                        evidence = new[] { reference }
                    });
                }
                if (targets.Count == 0) throw new InvalidDataException($"Level {record.levelNumber} has no authored objectives.");
                levels.Add(new Level { level = record.levelNumber, source = record.sourcePath, objectives = targets.ToArray() });
            }
            if (!Enumerable.Range(1, 130).SequenceEqual(levels.Select(level => level.level).OrderBy(number => number)))
                throw new InvalidDataException("Expected the 130 unique authored level identities.");

            Directory.CreateDirectory("Logs/SparkPhase4");
            string json = JsonUtility.ToJson(new Export { levels = levels.ToArray() }, true);
            File.WriteAllText(AuditPath, json);
            if (!File.Exists(LessonPath) || File.ReadAllText(LessonPath) != json)
            {
                File.WriteAllText(LessonPath, json);
                AssetDatabase.ImportAsset(LessonPath);
            }
            var rows = levels.SelectMany(level => level.objectives).ToArray();
            log($"PASS 130 authored level definitions, {rows.Length} stable objective records, {physicalLayouts} matching layout assets.");
            log($"Authored objective cards: {rows.Count(row => row.explanation.Length > 0)} explanations, " +
                $"{rows.Count(row => row.visual.Length > 0)} objective whiteboard references, {rows.Count(row => row.practice.Length > 0)} native practice references, {rows.Count(row => row.assessment.Length > 0)} explicit assessment links.");
        }
    }
}
