#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Inspects the currently compiled Unity project to identify required integration points.
    /// Type presence is not permission to guess or call undocumented gameplay APIs.
    /// </summary>
    public static class SparkAutoProjectIntegrationAudit
    {
        [MenuItem("Tools/Project Spark/Automatic Level Generator/Audit Project Integrations")]
        public static void AuditMenu() { Debug.Log(Audit()); }

        public static string Audit()
        {
            var result = new StringBuilder();
            result.AppendLine("PROJECT SPARK — GENERATOR V2 PROJECT INTEGRATION AUDIT");
            result.AppendLine("Editor: " + Application.unityVersion);
            var pipeline = GraphicsSettings.currentRenderPipeline;
            result.AppendLine("Render pipeline: " + (pipeline != null ? pipeline.GetType().FullName : "Built-in"));
            var types = new List<Type>();
            foreach (Type t in TypeCache.GetTypesDerivedFrom<MonoBehaviour>()) types.Add(t);
            string[] services = {
                "LevelGamePlayManager", "SparkSelectionController", "SparkCircuitSystem",
                "SparkElectricalNetworkGraph", "SparkMeasurementSystem", "SparkTerminal",
                "SparkAutoAuthorityBridge", "SparkAutoLevelRuntime"
            };
            for (int i = 0; i < services.Length; i++)
            {
                var matches = new List<string>();
                for (int j = 0; j < types.Count; j++)
                    if (string.Equals(types[j].Name, services[i], StringComparison.Ordinal)) matches.Add(types[j].FullName);
                result.AppendLine(services[i] + ": " + (matches.Count == 0 ? "NOT FOUND IN MONOBEHAVIOUR TYPES (may be a plain class)" : string.Join(", ", matches)));
            }
            result.AppendLine("No gameplay adapter has been automatically configured. Runtime authority still requires explicit, verified bindings.");
            return result.ToString();
        }
    }
}
#endif
