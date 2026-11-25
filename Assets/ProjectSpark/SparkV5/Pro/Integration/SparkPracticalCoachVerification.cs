#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Editor verification compiled beside Cortex in the same runtime assembly.
    /// Deliberately NOT in an Editor folder: internal Cortex types are not public.
    /// </summary>
    internal static class SparkPracticalCoachVerification
    {
        [MenuItem("Project Spark/Spark V5 Pro/Master Teacher/12 - Verify AAA Practical Coach V4")]
        private static void Run()
        {
            int passed = 0;
            int failed = 0;
            Check("Check my Level 1 LED objective", SparkProPracticalCoach.Request.Objective);
            Check("What is wrong with my LED circuit?", SparkProPracticalCoach.Request.Diagnose);
            Check("Why is my LED not working?", SparkProPracticalCoach.Request.Diagnose);
            Check("Diagnose my circuit", SparkProPracticalCoach.Request.Diagnose);
            Check("Show me which connection I should inspect next.", SparkProPracticalCoach.Request.InspectNext);
            Check("Which terminal should I check next?", SparkProPracticalCoach.Request.InspectNext);
            Check("Point to the anode", SparkProPracticalCoach.Request.InspectNext);
            Check("Have I completed the practical exercise?", SparkProPracticalCoach.Request.PracticeStatus);
            Check("Is my practical done?", SparkProPracticalCoach.Request.PracticeStatus);
            Check("Explain why the LED needs a complete return path.", SparkProPracticalCoach.Request.ExplainPath);
            Check("Why is the return path needed?", SparkProPracticalCoach.Request.ExplainPath);
            Check("Teach me Level 1", SparkProPracticalCoach.Request.None);
            Check("Anode and cathode", SparkProPracticalCoach.Request.None);
            Check("Continue the lesson", SparkProPracticalCoach.Request.None);
            Check("No", SparkProPracticalCoach.Request.None);
            Check("Read the voltage", SparkProPracticalCoach.Request.None);
            Check("Show a schematic", SparkProPracticalCoach.Request.None);
            Check("Pause the lesson", SparkProPracticalCoach.Request.None);
            Check("What is an anode?", SparkProPracticalCoach.Request.None);
            Check("Give me a hint", SparkProPracticalCoach.Request.None);

            if (failed == 0) Debug.Log($"[Spark Practical Coach V4] PASS {passed}/{passed + failed} intent regression checks. Next: actual Play Mode and solver tests.");
            else Debug.LogError($"[Spark Practical Coach V4] FAILED {failed} of {passed + failed} checks. Do not rely on the feature until fixed.");

            void Check(string question, SparkProPracticalCoach.Request expected)
            {
                var result = SparkProPracticalCoach.Classify(question);
                if (result == expected) passed++;
                else { failed++; Debug.LogError($"[Spark Practical Coach V4] '{question}' classified as {result}, expected {expected}"); }
            }
        }
    }
}
#endif
