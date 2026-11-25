#if UNITY_EDITOR
using System;
using ProjectSpark.SparkV5.Pro;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkLabV62Checks
    {
        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/5 - Diagnose V6.2 Diode Terminals")]
        public static void Check()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Spark Lab v6.2] Enter Play Mode and select a diode on the workbench first.");
                return;
            }
            var executives = UnityEngine.Object.FindObjectsByType<SparkProExecutive>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int inspected = 0;
            foreach (var executive in executives)
            {
                if (!executive || !executive.isActiveAndEnabled) continue;
                var director = executive.GetComponent<SparkLabTeachingDirector>();
                if (!director) { Debug.LogWarning("[Spark Lab v6.2] Director missing on " + executive.name); continue; }
                var selection = executive.ActiveSelectionController;
                Debug.Log("[Spark Lab v6.2] SelectedObject=" +
                    (selection && selection.SelectedObject ? selection.SelectedObject.name : "NONE") +
                    ". Inspector hierarchy highlighting alone does not set gameplay selection.", executive);
                bool anode = director.TryShowCurrent(SparkLabFocusIntent.Kind.PositiveTerminal);
                Debug.Log("[Spark Lab v6.2] Anode=" + anode + " | " + director.LastInspection, director);
                bool cathode = director.TryShowCurrent(SparkLabFocusIntent.Kind.NegativeTerminal);
                Debug.Log("[Spark Lab v6.2] Cathode=" + cathode + " | " + director.LastInspection, director);
                inspected++;
            }
            if (inspected == 0) Debug.LogWarning("[Spark Lab v6.2] No active Spark executive found.");
        }
    }
}
#endif
