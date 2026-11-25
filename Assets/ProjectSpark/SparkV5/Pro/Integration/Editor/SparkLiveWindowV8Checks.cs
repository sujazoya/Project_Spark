#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Non-destructive inspection of the Spark Live adjustable UI integration.</summary>
    internal static class SparkLiveWindowV8Checks
    {
        [MenuItem("Project Spark/Spark V5 Pro/Spark Live Window/V8 - Verify Setup")]
        private static void Verify()
        {
            int pass = 0, warnings = 0;
            string basePath = "Assets/ProjectSpark/SparkV5/Pro/Integration/";
            string source = basePath + "SparkProLivePanel.cs";
            string adjustable = basePath + "SparkLiveAdjustableWindow.cs";
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(adjustable)) pass++;
            else { warnings++; Debug.LogWarning("[Spark Live v8] Missing adjustable window script."); }
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(source)) pass++;
            else { warnings++; Debug.LogWarning("[Spark Live v8] Missing Spark Live panel script."); }
            string absolute = Path.Combine(Directory.GetCurrentDirectory(), source);
            if (File.Exists(absolute))
            {
                string code = File.ReadAllText(absolute);
                if (code.Contains("SparkLiveAdjustableWindow.Attach(this, shell)")) pass++;
                else { warnings++; Debug.LogWarning("[Spark Live v8] Bind() is not connected to adjustable window."); }
                if (code.Contains("adjustableWindow.RefreshBounds()")) pass++;
                else { warnings++; Debug.LogWarning("[Spark Live v8] Original Fit() can still override layout."); }
            }
            var prefab = Resources.Load<SparkProLivePanel>("SparkProLivePanel");
            if (prefab) pass++;
            else { warnings++; Debug.LogWarning("[Spark Live v8] SparkProLivePanel resource was not found; the runtime builder may be used."); }
            Debug.Log("[Spark Live v8] Setup inspection: " + pass + "/5 checks passed; " + warnings +
                " warnings. Play Mode drag/resize/scale, safe click-through and mobile testing still required.");
        }

        [MenuItem("Project Spark/Spark V5 Pro/Spark Live Window/V8 - Reset Saved Window Layout")]
        private static void Reset()
        {
            if (!EditorUtility.DisplayDialog("Reset Spark Live layout?",
                "Delete only the Spark Live v8 window geometry and appearance settings? The panel will apply defaults when reopened.",
                "Reset", "Cancel")) return;
            string prefix = "ProjectSpark.SparkLive.Layout.v8.";
            string[] keys = { "x", "y", "width", "height", "scale", "opacity", "collapsed", "snap", "locked" };
            foreach (string key in keys) PlayerPrefs.DeleteKey(prefix + key);
            PlayerPrefs.Save();
            Debug.Log("[Spark Live v8] Saved window layout reset. Close and reopen Spark Live.");
        }
    }
}
#endif
