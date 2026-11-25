#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor self-tests with no external NUnit dependency.</summary>
    public static class SparkMasterTeacherSelfTests
    {
        [MenuItem(SparkMasterTeacherImporter.MenuRoot + "5 — Run Importer & Rubric Self-Tests")]
        public static void Run()
        {
            int checks = 0;
            void Check(bool pass, string name)
            {
                if (!pass) throw new InvalidOperationException("Master Teacher test failed: " + name);
                checks++;
            }
            var volts = new SparkProAnswerRubric { kind = SparkProAnswerKind.Numeric,
                expectedNumber = 5.0, expectedUnit = "V", absoluteTolerance = 0.005 };
            Check(volts.Evaluate("5 V"), "numeric volts");
            Check(volts.Evaluate("5000 mV"), "volts from millivolts");
            Check(!volts.Evaluate("5 A"), "incompatible electrical units rejected");
            var current = new SparkProAnswerRubric { kind = SparkProAnswerKind.Numeric,
                expectedNumber = 20.0, expectedUnit = "mA", absoluteTolerance = .01 };
            Check(current.Evaluate("0.02 A"), "amp to milliamp");
            Check(current.Evaluate("20 mA"), "native milliamp");
            Check(current.Evaluate("twenty milliamps"), "spoken milliamp quantity");
            Check(!current.Evaluate("20 A"), "1000x wrong-unit answer rejected");
            var negative = new SparkProAnswerRubric { kind = SparkProAnswerKind.AnyTerm,
                acceptedTerms = new[] { "closed" } };
            Check(!negative.Evaluate("not closed"), "negated claim rejected");
            Check(negative.Evaluate("closed"), "affirmative matched");
            var terms = new SparkProAnswerRubric { kind = SparkProAnswerKind.AllTerms,
                acceptedTerms = new[] { "voltage", "current" } };
            Check(terms.Evaluate("voltage and current"), "all terms matched");
            Check(!terms.Evaluate("voltage"), "partial list rejected");
            Check(SparkMasterTeacherImporter.DraftPath(1, "led") ==
                  SparkMasterTeacherImporter.DraftPath(1, "led"), "stable asset path");
            Check(SparkMasterTeacherImporter.DraftPath(1, "led") !=
                  SparkMasterTeacherImporter.DraftPath(2, "led"), "level identity isolated");
            Check(SparkMasterTeacherImporter.TryReadCards(out var cards, out var error),
                  "original objective source is accessible: " + error);
            Check(cards.levels.Length == 130, "full authored level count");
            Check(cards.levels.SelectMany(x => x.objectives).Count() >= 1,
                  "authored objectives exist");
            Check(cards.levels.Select(x => x.level).Distinct().Count() == 130,
                  "unique authored level identities");
            var unreviewed = ScriptableObject.CreateInstance<SparkProLessonDefinition>();
            try
            {
                unreviewed.lessonId = "test-only";
                unreviewed.title = "Test";
                unreviewed.approvedForTeaching = false;
                Check(!unreviewed.IsReady(out _), "unreviewed lesson rejected");
            }
            finally { UnityEngine.Object.DestroyImmediate(unreviewed); }
            Debug.Log("[Master Teacher] PASS " + checks + " Editor self-tests. " +
                      "This does not replace Play Mode or Windows/Android device testing.");
        }
    }
}
#endif
