#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkMasterTeacherAutoTests
    {
        [MenuItem("Project Spark/Spark V5 Pro/Master Teacher/9 — Run Auto Guide Integrity Tests")]
        public static void Run()
        {
            if (!SparkMasterTeacherImporter.TryReadCards(out var catalog, out string problem))
                throw new InvalidOperationException(problem);
            int checks = 0;
            void Check(bool value, string message)
            {
                if (!value) throw new InvalidOperationException("[Auto Teacher Test] " + message);
                checks++;
            }
            Check(catalog.levels.Length == 130, "130 unique level definitions required");
            var objectiveKeys = new HashSet<string>(StringComparer.Ordinal);
            int count = 0;
            foreach (var level in catalog.levels)
            foreach (var objective in level.objectives)
            {
                count++;
                string id = SparkMasterTeacherImporter.StableLessonId(level.level, objective.id);
                Check(objectiveKeys.Add(id), "Duplicate objective " + id);
                var guide = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(
                    SparkMasterTeacherAutoGenerator.GuidePath(level.level, objective.id));
                Check(guide != null, "Missing guide " + id);
                Check(guide.IsUsable(out _), "Guide not usable " + id);
                Check(guide.guidedOnly, "Auto guide might grade the learner: " + id);
                Check(guide.lessonId == id, "Objective mislinked " + id);
                var draft = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(
                    SparkMasterTeacherImporter.DraftPath(level.level, objective.id));
                Check(draft && guide.sourceFingerprint == draft.sourceFingerprint, "Stale guide " + id);
                Check(guide.sourceAsset == level.source, "Wrong authoritative source " + id);
            }
            Check(count == 732, "Expected 732 authored objectives in this project snapshot");
            var sample = ScriptableObject.CreateInstance<SparkProAutoGuide>();
            try
            {
                sample.level = 1; sample.lessonId = "L001/led";
                sample.objectiveId = "led"; sample.title = "Test";
                sample.sourceAsset = "Assets/Test.asset"; sample.sourceFingerprint = "abc";
                sample.authoredRequirement = "Test"; sample.concept = "Test";
                sample.misconception = "Test"; sample.baseline = "Test";
                sample.prediction = "Test"; sample.practice = "Test";
                sample.aliases = new[] { "led" }; sample.guidedOnly = false;
                Check(!sample.IsUsable(out _), "Auto-grading accidentally enabled");
            }
            finally { UnityEngine.Object.DestroyImmediate(sample); }
            Debug.Log("[Spark Auto Teacher] PASS " + checks + " integrity checks; " + count +
                " objective-linked guides. Play Mode verification is still necessary.");
        }
    }
}
#endif
