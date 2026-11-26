using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Read-only rig and mechanical sleeve diagnostic. Never creates connections.</summary>
    public static class SparkTelescopicAAAEditor
    {
        private const string ReportPath = "Logs/SparkTelescopicArms/v18_4_Precision_Diagnostic.txt";

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/42 - Diagnose Original-Hand Precision v18.4")]
        private static void DiagnosePrecision() { Diagnose(); }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/41 - Diagnose Telescopic AAA v18.3")]
        private static void Diagnose()
        {
            var report = new StringBuilder(4096);
            report.AppendLine("SPARK v18.4 — ORIGINAL HAND PRECISION + MECHANICAL LINK DIAGNOSTIC");
            report.AppendLine("Read-only: does not move Spark or manipulate the electrical circuit.");
            report.AppendLine("PlayMode=" + EditorApplication.isPlaying);
            var controllers = UnityEngine.Object.FindObjectsByType<SparkTelescopicArms>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Controllers=" + controllers.Length);
            foreach (SparkTelescopicArms controller in controllers)
            {
                if (!controller) continue;
                report.AppendLine("Avatar=" + controller.name);
                report.AppendLine("Status=" + controller.Status);
                var character = controller.GetComponent<SparkProCharacter>();
                if (!character) { report.AppendLine("FAIL: SparkProCharacter missing"); continue; }
                var transforms = character.GetComponentsInChildren<Transform>(true);
                int wrists = 0, thumbs = 0, tips = 0, stages = 0, collars = 0, legacyProxies = 0;
                foreach (Transform item in transforms)
                {
                    if (!item) continue;
                    string name = item.name;
                    if (name == "Wrist_R" || name == "Wrist_L") wrists++;
                    if (name == "Thumb_R" || name == "Thumb_L" ||
                        name == "ThumbTip_R" || name == "ThumbTip_L") thumbs++;
                    if (name == "FingerTip0_R" || name == "FingerTip0_L") tips++;
                    if (name.StartsWith("Spark AAA Telescopic ", StringComparison.Ordinal))
                    {
                        if (name.Contains(" Stage ")) stages++;
                        if (name.Contains(" Sliding Collar ")) collars++;
                    }
                    if (name.StartsWith("Spark v18.2 Joint Bridge", StringComparison.Ordinal) ||
                        name.StartsWith("Spark Telescopic ", StringComparison.Ordinal))
                        legacyProxies++;
                }
                report.AppendLine("Existing real wrists=" + wrists + " thumbs=" + thumbs + " fingertip-0=" + tips);
                report.AppendLine("Runtime structural stages=" + stages + " joint collars=" + collars);
                report.AppendLine("Legacy proxy structures=" + legacyProxies);
                report.AppendLine("Grip contact threshold=" + controller.ContactTolerance.ToString("F4") + "m");
                report.AppendLine("Right contact verified=" + controller.RightContactConfirmed);
                report.AppendLine("Left contact verified=" + controller.LeftContactConfirmed);
                report.AppendLine("Active mechanical shaft stages=" + controller.ActiveStageCount);
                report.AppendLine("Right grip-to-target error=" + controller.RightGripError.ToString("F4") + "m");
                report.AppendLine("Left grip-to-target error=" + controller.LeftGripError.ToString("F4") + "m");
                if (wrists < 2) report.AppendLine("WARNING: Original wrist rigs missing");
                if (legacyProxies > 0) report.AppendLine("WARNING: Legacy geometry still loaded; reload the level");
                if (controller.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out Vector3 right))
                    report.AppendLine("Right actual grip world=" + right.ToString("F4"));
                if (controller.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Left, out Vector3 left))
                    report.AppendLine("Left actual grip world=" + left.ToString("F4"));
            }
            string path = Path.GetFullPath(ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report.ToString());
            Debug.Log("[Spark AAA Telescopic v18.4] Diagnostic: " + path + "\n" + report);
        }
    }
}
