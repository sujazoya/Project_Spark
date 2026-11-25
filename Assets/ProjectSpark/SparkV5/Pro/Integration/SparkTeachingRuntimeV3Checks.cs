#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Editor-side classification regression checks; does not award, grade, or alter lessons.</summary>
    public static class SparkTeachingRuntimeV3Checks
    {
        [MenuItem("Project Spark/Spark V5 Pro/Master Teacher/11 — Verify Interactive Routing V3")]
        public static void Run()
        {
            int count = 0;
            void Check(bool correct, string message)
            {
                if (!correct) throw new InvalidOperationException("[Spark Teacher V3] " + message);
                count++;
            }
            Check(SparkCortex.Classify("Teach me Level 1.") == SparkIntent.Lesson, "Teach me Level 1");
            Check(SparkCortex.Classify("Teach me the first objective of Level 1 step by step.") == SparkIntent.Lesson, "Step-by-step request");
            Check(SparkCortex.Classify("Ask me a question about Level 1 and check my answer.") == SparkIntent.Lesson, "Question request");
            Check(SparkCortex.Classify("Walk me through Level 24.") == SparkIntent.Lesson, "Walkthrough request");
            Check(SparkCortex.Classify("Continue the lesson.") == SparkIntent.Resume, "Resume lesson");
            Check(SparkCortex.Classify("Pause the lesson.") == SparkIntent.Pause, "Pause lesson");
            Check(SparkCortex.Classify("Cancel the lesson.") == SparkIntent.Cancel, "Cancel lesson");
            Check(SparkCortex.Classify("Explain Level 24 objectives.") == SparkIntent.LevelDetails, "Level information stays separate");
            Check(SparkCortex.Classify("What are the incomplete objectives?") == SparkIntent.Objectives, "Live objectives stay separate");
            Check(SparkProCoreConcepts.TryExplain("what is the anode of an led", out string answer) &&
                answer.IndexOf("conventional current", StringComparison.OrdinalIgnoreCase) >= 0,
                "Foundational anode explanation");
            Check(!SparkProCoreConcepts.TryExplain("what voltage is my selected resistor", out _),
                "Primer must not invent solver readings");
            Debug.Log("[Spark Teacher V3] PASS " + count + " routing and concept checks. Test Play Mode separately.");
        }
    }
}
#endif
