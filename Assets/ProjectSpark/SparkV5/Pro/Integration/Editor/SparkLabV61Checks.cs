#if UNITY_EDITOR
using System;
using ProjectSpark.SparkV5.Pro;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkLabV61Checks
    {
        private const string Root = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(Root + "3 - Check V6.1 Command Recognition")]
        public static void CheckCommands()
        {
            void Check(string phrase, SparkLabFocusIntent.Kind expected)
            {
                var actual = SparkLabFocusIntent.Classify(phrase);
                if (actual != expected)
                    throw new InvalidOperationException("Lab v6.1 expected " + expected +
                        " but classified '" + phrase + "' as " + actual);
            }
            Check("Highlight selected component.", SparkLabFocusIntent.Kind.Selected);
            Check("Highlight the selected diode", SparkLabFocusIntent.Kind.Selected);
            Check("Point to the anode", SparkLabFocusIntent.Kind.PositiveTerminal);
            Check("Point to the cathode", SparkLabFocusIntent.Kind.NegativeTerminal);
            Check("Show me which connection I should inspect next", SparkLabFocusIntent.Kind.NextConnection);
            Check("Clear the highlight", SparkLabFocusIntent.Kind.Clear);
            Check("Teach me level 1", SparkLabFocusIntent.Kind.None);
            Debug.Log("[Spark Lab v6.1] PASS 7 command-classification checks. Run live selection test in Play Mode.");
        }

        [MenuItem(Root + "4 - Diagnose Active Selection in Play Mode")]
        public static void DiagnoseSelection()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Spark Lab v6.1] Start Play Mode and load a laboratory level first.");
                return;
            }
            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int checkedCount = 0;
            foreach (var executive in executives)
            {
                if (!executive || !executive.isActiveAndEnabled) continue;
                checkedCount++;
                var selection = executive.ActiveSelectionController;
                var selected = selection ? selection.SelectedObject : null;
                var director = executive.GetComponent<SparkLabTeachingDirector>();
                Debug.Log("[Spark Lab v6.1] Executive=" + executive.name +
                    ", Director=" + (director ? "present" : "MISSING") +
                    ", Selection=" + (selection ? selection.name : "MISSING") +
                    ", SelectedObject=" + (selected ? selected.name : "NONE") +
                    ", Mission=" + (executive.ActiveLevelManager ? "bound" : "MISSING"), executive);
                if (director)
                {
                    bool ok = director.TryShowCurrent(SparkLabFocusIntent.Kind.Selected);
                    Debug.Log("[Spark Lab v6.1] Show selected=" + ok +
                        " | " + director.LastInspection, director);
                }
            }
            if (checkedCount == 0)
                Debug.LogWarning("[Spark Lab v6.1] No active SparkProExecutive found. Check character prefab and loaded scene bindings.");
        }
    }
}
#endif
