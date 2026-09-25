#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Display.Tests.Editor
{
    [CustomEditor(typeof(SparkDisplayTestHarness))]
    public sealed class SparkDisplayTestHarnessEditor :
        UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12f);

            SparkDisplayTestHarness harness =
                (SparkDisplayTestHarness)target;

            EditorGUILayout.LabelField(
                "Display Test Results",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Total",
                harness.Total.ToString());

            EditorGUILayout.LabelField(
                "Passed",
                harness.Passed.ToString());

            EditorGUILayout.LabelField(
                "Failed",
                harness.Failed.ToString());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6f);

            string stateText =
                harness.IsRunning
                    ? "RUNNING"
                    : "IDLE";

            EditorGUILayout.LabelField(
                "Test State",
                stateText);

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();

                using (new EditorGUI.DisabledScope(
                           harness.IsRunning))
                {
                    if (GUILayout.Button(
                            "RUN ALL TESTS",
                            GUILayout.Height(36f)))
                    {
                        harness.StartAutomatedTest();
                    }
                }

                using (new EditorGUI.DisabledScope(
                           !harness.IsRunning))
                {
                    if (GUILayout.Button(
                            "STOP TEST",
                            GUILayout.Height(36f)))
                    {
                        harness.StopAutomatedTest();
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4f);

                if (GUILayout.Button(
                        "RESET RESULTS"))
                {
                    harness.ResetTestResults();
                }

                EditorGUILayout.Space(12f);

                DrawIndividualTests(harness);

                EditorGUILayout.Space(12f);

                DrawStatisticsTests(harness);

                EditorGUILayout.Space(12f);

                DrawStateTests(harness);
            }

            EditorGUILayout.Space(12f);

            EditorGUILayout.HelpBox(
                "Display tests must be run in Play Mode because SparkAdvancedDisplay is a runtime system.",
                MessageType.Info);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(harness);
            }
        }

        private static void DrawIndividualTests(
            SparkDisplayTestHarness harness)
        {
            EditorGUILayout.LabelField(
                "Individual Tests",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                    "Test Basic Value"))
            {
                harness.TestBasicValue();
            }

            if (GUILayout.Button(
                    "Test Engineering Prefixes"))
            {
                harness.TestEngineeringPrefixes();
            }

            if (GUILayout.Button(
                    "Test Invalid Value"))
            {
                harness.TestInvalidValue();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "NaN"))
            {
                harness.TestNaNValue();
            }

            if (GUILayout.Button(
                    "+Infinity"))
            {
                harness.TestPositiveInfinity();
            }

            if (GUILayout.Button(
                    "-Infinity"))
            {
                harness.TestNegativeInfinity();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(
                    "Test Bar"))
            {
                harness.TestBar();
            }

            if (GUILayout.Button(
                    "Test Signal Quality"))
            {
                harness.TestSignalQuality();
            }

            if (GUILayout.Button(
                    "Test Range"))
            {
                harness.TestRange();
            }

            if (GUILayout.Button(
                    "Test Alarm"))
            {
                harness.TestAlarm();
            }
        }

        private static void DrawStatisticsTests(
            SparkDisplayTestHarness harness)
        {
            EditorGUILayout.LabelField(
                "Statistics",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                    "Test Statistics"))
            {
                harness.TestStatistics();
            }

            if (GUILayout.Button(
                    "Test Statistics Reset"))
            {
                harness.TestStatisticsReset();
            }

            if (GUILayout.Button(
                    "Test Tolerance / Statistics"))
            {
                harness.TestTolerance();
            }
        }

        private static void DrawStateTests(
            SparkDisplayTestHarness harness)
        {
            EditorGUILayout.LabelField(
                "Graph / Power / State",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                    "Test Graph"))
            {
                harness.TestGraph();
            }

            if (GUILayout.Button(
                    "Test All Statuses"))
            {
                harness.TestAllStatuses();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Power OFF"))
            {
                harness.TestPoweredOff();
            }

            if (GUILayout.Button(
                    "Power ON"))
            {
                harness.TestPoweredOn();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(
                    "Clear Display"))
            {
                harness.TestClear();
            }
        }
    }
}

#endif
