using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkOriginalHandRigDiagnostic
    {
        private const string Report = "Logs/SparkTelescopicArms/v18_1_OriginalRig_Diagnostic.txt";

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/40 - Diagnose Original Hands v18.1")]
        private static void Diagnose()
        {
            var text = new StringBuilder(4096);
            text.AppendLine("SPARK v18.1 — ORIGINAL HAND / LOD RIG DIAGNOSTIC (read-only)");
            text.AppendLine("The test inspects joints; it does not extend hands, change power, or create wires.");
            var controllers = UnityEngine.Object.FindObjectsByType<SparkTelescopicArms>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            text.AppendLine("Telescopic controllers in loaded scenes: " + controllers.Length);
            foreach (SparkTelescopicArms c in controllers)
            {
                if (!c) continue;
                var character = c.GetComponent<SparkProCharacter>();
                text.AppendLine("Object: " + c.name + " status=" + c.Status);
                if (!character)
                {
                    text.AppendLine("  FAIL: SparkProCharacter component missing");
                    continue;
                }
                int rights = 0, lefts = 0, realWrists = 0, fingerChildren = 0, oldProxyObjects = 0;
                Transform[] nodes = character.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in nodes)
                {
                    if (t.name == "Shoulder_R") rights++;
                    if (t.name == "Shoulder_L") lefts++;
                    if (t.name == "Wrist_R" || t.name == "Wrist_L")
                    {
                        realWrists++;
                        for (int i = 0; i < t.childCount; i++)
                        {
                            string childName = t.GetChild(i).name;
                            if (childName.StartsWith("Finger", StringComparison.Ordinal) ||
                                childName.StartsWith("Thumb", StringComparison.Ordinal))
                                fingerChildren++;
                        }
                    }
                    if (t.name.StartsWith("Spark Telescopic ", StringComparison.Ordinal) &&
                        t.GetComponent<MeshFilter>()) oldProxyObjects++;
                }
                text.AppendLine("  Existing shoulder copies R/L=" + rights + "/" + lefts);
                text.AppendLine("  Original wrists=" + realWrists + " direct finger/thumb pivots=" + fingerChildren);
                text.AppendLine("  Legacy generated cylinders=" + oldProxyObjects);
                text.AppendLine("  Expected: all LOD copies follow same target; no generated cylinders.");
                if (rights != lefts || rights == 0) text.AppendLine("  WARNING: unbalanced / missing original shoulder chains");
                if (oldProxyObjects > 0) text.AppendLine("  WARNING: old v18 runtime proxy geometry still present");
            }
            string destination = Path.GetFullPath(Report);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllText(destination, text.ToString());
            Debug.Log("[Spark Original Hands v18.1] " + destination + "\n" + text);
        }
    }
}
