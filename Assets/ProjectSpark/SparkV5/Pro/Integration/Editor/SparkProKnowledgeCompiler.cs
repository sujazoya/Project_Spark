using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectSpark.UI.VisualTeaching;
using System.Text;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkProKnowledgeCompiler
    {
        internal const string Root = "Assets/ProjectSpark/SparkV5/Pro/Generated";
        internal const string CatalogPath = Root + "/SparkProGameCatalog.asset";
        private const string CampaignPath = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Campaign/ProjectSparkAcademy.asset";
        private static readonly string[] IndexedTypes =
        {
            "SparkAIKnowledge", "SparkAICurriculumDefinition", "SparkAIChallengeDefinition",
            "SparkAIAssessmentBank", "SparkGuidedLearningPathAsset", "SparkLevelDefinition",
            "SparkChapterDefinition", "SparkCampaignDefinition", "SparkMissionDefinition",
            "SparkComponentDefinition", "SparkTutorialDefinition", "SparkLevelLayoutCatalog", "SparkVisualLesson", "SparkProTeachingLibrary"
        };

        [MenuItem("Project Spark/Spark V5 Pro/Compile Complete Game Knowledge")]
        public static void Compile() => CompileCatalog(true);

        internal static SparkProGameCatalog CompileCatalog(bool select)
        {
            var campaign = AssetDatabase.LoadAssetAtPath<SparkCampaignDefinition>(CampaignPath);
            if (!campaign)
                throw new InvalidOperationException("The game campaign asset is missing. Check the chapter binding before compiling.");
            const string game = "Assets/My_Assets/_Project_Spark/";
            var layoutCatalog = AssetDatabase.LoadAssetAtPath<SparkLevelLayoutCatalog>(
                game + "Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset");
            var ordered = campaign.Chapters.Where(chapter => chapter).SelectMany(chapter => chapter.Levels)
                .Where(level => level).ToArray();
            var predecessors = new Dictionary<SparkLevelDefinition, int>();
            for (int i = 1; i < ordered.Length; i++) predecessors.TryAdd(ordered[i], ordered[i - 1].LevelNumber);
            var resources = IndexAssets();
            var curriculum = resources.Where(asset => asset.kind == nameof(SparkVisualLesson) ||
                asset.kind == nameof(SparkGuidedLearningPathAsset) || asset.kind == nameof(SparkAIAssessmentBank))
                .Select(asset => (asset, terms: SparkProGameCatalog.Words(asset.name).Distinct().ToArray())).ToArray();
            var records = new List<SparkProGameCatalog.Record>();
            var findings = new List<string>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var grouped = new SortedDictionary<int, List<SparkLevelDefinition>>();
            foreach (string guid in AssetDatabase.FindAssets("t:SparkLevelDefinition"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(path);
                if (!level)
                    continue;
                if (level.LevelNumber <= 0 || string.IsNullOrWhiteSpace(level.LevelId))
                {
                    findings.Add("Invalid level identity: " + path);
                    continue;
                }
                if (!grouped.TryGetValue(level.LevelNumber, out var candidates))
                    grouped.Add(level.LevelNumber, candidates = new List<SparkLevelDefinition>());
                candidates.Add(level);
            }

            foreach (var pair in grouped)
            {
                SparkLevelDefinition level = null;
                foreach (var candidate in pair.Value)
                    if (campaign.FindChapter(candidate))
                    {
                        if (level != null)
                            throw new InvalidOperationException($"Campaign contains multiple definitions for Level {pair.Key}.");
                        level = candidate;
                    }

                if (pair.Value.Count == 1)
                    level = pair.Value[0];
                else
                    foreach (var candidate in pair.Value)
                        if (candidate != level)
                            findings.Add($"Duplicate Level {pair.Key}: {AssetDatabase.GetAssetPath(candidate)}");

                if (!level)
                    continue;
                string path = AssetDatabase.GetAssetPath(level);
                if (!ids.Add(level.LevelId))
                {
                    findings.Add("Duplicate level ID: " + path);
                    continue;
                }

                var chapter = campaign.FindChapter(level);
                var layout = layoutCatalog ? layoutCatalog.GetPrefab(level) : null;
                var objectives = new List<string>();
                var targets = new List<SparkProGameCatalog.Objective>();
                if (level.Targets != null)
                {
                    var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var target in level.Targets)
                    {
                        if (target == null)
                        {
                            findings.Add($"{level.LevelId}: null objective");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(target.TargetId) || !objectiveIds.Add(target.TargetId))
                            findings.Add($"{level.LevelId}: empty or repeated objective identity {target.TargetId}");
                        targets.Add(new SparkProGameCatalog.Objective
                        {
                            id = target.TargetId, title = SparkAILevelContext.TargetName(target), description = target.Description,
                            kind = target.Type.ToString(), componentId = target.TargetComponentId, terminalId = target.TargetTerminalId,
                            positiveId = target.RequiredPositiveTerminalId, negativeId = target.RequiredNegativeTerminalId
                        });
                        objectives.Add($"{SparkAILevelContext.TargetName(target)}. {target.Description} {SparkAILevelContext.TargetConditions(target)}".Trim());
                    }
                }
                if (objectives.Count == 0)
                    findings.Add($"{level.LevelId}: no authored objectives");
                if (string.IsNullOrWhiteSpace(level.Description))
                    findings.Add($"{level.LevelId}: missing authored description");

                var terms = new HashSet<string>(SparkProGameCatalog.Words(level.DisplayName + " " + level.Description + " " +
                    string.Join(" ", objectives)), StringComparer.Ordinal);
                string[] concepts = ConceptTags(level.LevelNumber, terms);
                records.Add(new SparkProGameCatalog.Record
                {
                    levelNumber = level.LevelNumber,
                    id = level.LevelId,
                    title = level.DisplayName,
                    description = level.Description,
                    conditions = SparkAILevelContext.CompletionRule(level),
                    sourceGuid = AssetDatabase.AssetPathToGUID(path),
                    sourcePath = path,
                    chapter = chapter ? chapter.DisplayName : "",
                    chapterNumber = chapter ? chapter.ChapterNumber : 0,
                    inCampaign = chapter != null,
                    objectives = objectives.ToArray(), targets = targets.ToArray(),
                    layoutPath = layout ? AssetDatabase.GetAssetPath(layout) : "",
                    previousLevel = predecessors.GetValueOrDefault(level),
                    accessRule = chapter ? chapter.AccessMode.ToString() : "",
                    chapterOpen = chapter && chapter.UnlockedByDefault,
                    accessScore = chapter ? chapter.RequiredAssessmentScore : 0,
                    equipment = layout ? layout.GetComponentsInChildren<SparkInstrument>(true).Select(item => item.name).Distinct().OrderBy(name => name).ToArray()
                        : Array.Empty<string>(),
                    concepts = concepts,
                    teachingResources = curriculum.Select(entry => (entry.asset, rank: entry.terms.Count(terms.Contains)))
                        .Where(entry => entry.rank >= 2).OrderByDescending(entry => entry.rank).Take(5)
                        .Select(entry => entry.asset.guid).ToArray()
                });
            }

            int inRange = 0;
            int campaignCount = 0;
            var registered = new HashSet<int>();
            var deferred = new List<int>();
            foreach (var record in records)
            {
                if (record.inCampaign) campaignCount++;
                if (record.levelNumber < 1 || record.levelNumber > 130) continue;
                inRange++;
                registered.Add(record.levelNumber);
                if (!record.inCampaign) deferred.Add(record.levelNumber);
            }
            for (int number = 1; number <= 130; number++)
                if (!registered.Contains(number))
                    findings.Add("No authored Unity level definition: Level " + number);

            var assets = resources;
            var catalog = AssetDatabase.LoadAssetAtPath<SparkProGameCatalog>(CatalogPath);
            if (!catalog)
            {
                if (AssetDatabase.LoadMainAssetAtPath(CatalogPath))
                    throw new InvalidOperationException("An unrelated asset exists at the game catalog path.");
                catalog = ScriptableObject.CreateInstance<SparkProGameCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var result = new SparkProGameCatalog.Record[records.Count];
            records.CopyTo(result);
            catalog.levels = result;
            catalog.assets = assets.ToArray();
            catalog.gaps = findings.ToArray();
            catalog.motherboardQualificationSource = AssetDatabase.LoadAssetAtPath<TextAsset>(
                game + "Prefabs/Levels/Level_120_Layout/RAX28/Data/CertificationGates.json");
            catalog.generatedUtc = DateTime.UtcNow.ToString("O");
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);

            var report = new StringBuilder("SPARK GAME KNOWLEDGE AUDIT\n");
            report.AppendLine($"Authored set 1–130: {inRange}/130");
            report.AppendLine($"Campaign members: {campaignCount}; indexed authored levels: {records.Count}");
            report.AppendLine("Authored reference only: " + (deferred.Count == 0 ? "none" : string.Join(", ", deferred)));
            report.AppendLine("Chapter source: " + CampaignPath);
            report.AppendLine($"Related assets: {assets.Count}. Findings: {findings.Count}.");
            foreach (string finding in findings) report.AppendLine(finding);
            File.WriteAllText(Root + "/KnowledgeAudit.txt", report.ToString());
            AssetDatabase.ImportAsset(Root + "/KnowledgeAudit.txt");
            if (select) Selection.activeObject = catalog;
            Debug.Log($"[Spark Pro] Compiled {inRange}/130 authored definitions and {campaignCount} campaign levels.");
            return catalog;
        }

        private static string[] ConceptTags(int levelNumber, HashSet<string> terms)
        {
            var topics = new (string title, string[] keywords)[]
            {
                ("Closed path", new[] { "continuity", "return", "switch" }),
                ("Ohm law", new[] { "resistance", "resistor", "current" }),
                ("Polarity", new[] { "polarity", "diode", "anode" }),
                ("Measurement", new[] { "measure", "probe", "multimeter" }),
                ("Rail qualification", new[] { "rail", "regulator", "pmic" }),
                ("Redundancy", new[] { "failover", "reserve", "redundancy" }),
                ("Error correction", new[] { "ecc", "syndrome", "scrub" }),
                ("Timing and interlocks", new[] { "watchdog", "interlock", "clock" })
            };
            return topics.Where(topic => topic.keywords.Any(terms.Contains) &&
                (levelNumber > 20 || topic.title is "Closed path" or "Ohm law" or "Polarity" or "Measurement"))
                .Select(topic => topic.title).ToArray();
        }

        private static List<SparkProGameCatalog.IndexRecord> IndexAssets()
        {
            var results = new List<SparkProGameCatalog.IndexRecord>();
            var seen = new HashSet<string>();
            foreach (string type in IndexedTypes)
                foreach (string guid in AssetDatabase.FindAssets("t:" + type))
                {
                    if (!seen.Add(guid)) continue;
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadMainAssetAtPath(path);
                    if (!(obj is ScriptableObject)) continue;
                    var serialized = new SerializedObject(obj);
                    var title = ReadText(serialized, "displayName", "title", "Title");
                    string summary = obj is SparkAIKnowledge knowledge ? $"Curated entries: {knowledge.EntryCount}" :
                        ReadText(serialized, "description", "summary", "Description");
                    results.Add(new SparkProGameCatalog.IndexRecord
                    {
                        kind = obj.GetType().Name, name = string.IsNullOrEmpty(title) ? obj.name : title,
                        guid = guid, path = path, summary = summary
                    });
                }
            results.Sort((a, b) => string.CompareOrdinal(a.path, b.path));
            return results;
        }

        private static string ReadText(SerializedObject serialized, params string[] fields)
        {
            foreach (string field in fields)
            {
                var property = serialized.FindProperty(field);
                if (property?.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(property.stringValue))
                    return property.stringValue;
            }
            return "";
        }

        internal static bool IsIndexedType(string name) => Array.IndexOf(IndexedTypes, name) >= 0;
    }

    internal sealed class SparkProKnowledgeWatcher : AssetPostprocessor
    {
        private static bool pending;
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] original)
        {
            if (pending || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            foreach (string path in imported)
                if (WasRelevant(path, true)) { Queue(); return; }
            foreach (string path in moved)
                if (WasRelevant(path, true)) { Queue(); return; }
            foreach (string path in deleted)
                if (WasRelevant(path, false)) { Queue(); return; }
            foreach (string path in original)
                if (WasRelevant(path, false)) { Queue(); return; }
        }

        private static bool WasRelevant(string path, bool exists)
        {
            if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(SparkProKnowledgeCompiler.Root + "/", StringComparison.Ordinal))
                return false;
            if (exists)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                return type != null && SparkProKnowledgeCompiler.IsIndexedType(type.Name);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<SparkProGameCatalog>(SparkProKnowledgeCompiler.CatalogPath);
            if (!catalog) return false;
            foreach (var record in catalog.assets)
                if (record != null && record.path == path) return true;
            return false;
        }

        private static void Queue()
        {
            pending = true;
            EditorApplication.delayCall += Rebuild;
        }

        private static void Rebuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Rebuild;
                return;
            }
            pending = false;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                SparkProKnowledgeCompiler.CompileCatalog(false);
        }
    }
}

