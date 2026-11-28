using System;
using System.Text;
using UnityEngine;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.SparkV5.Pro
{
    // Authored, editor-audited evidence for read-only previews. It is never an execution plan.
    internal static class SparkConstructionEvidence
    {
        [Serializable] internal sealed class Campaign { public Entry[] levels; }

        [Serializable] internal sealed class Entry
        {
            public int level;
            public string layout;
            public string revision;
            public string evidence;
            public string blocker;
            public string[] runtimeEndpoints;
            public SparkNativeWiringPlan.WireStep[] wires;
        }

        private static Entry[] levels;

        internal static string Preview(SparkAILevelContext context)
        {
            if (levels == null)
            {
                var source = Resources.Load<TextAsset>("SparkConstructionEvidenceV20");
                levels = (source ? JsonUtility.FromJson<Campaign>(source.text)?.levels : null) ?? Array.Empty<Entry>();
            }

            string layout = context.Layout.name.Replace("(Clone)", "").Trim();
            var entry = Array.Find(levels, row => row.level == context.Definition.LevelNumber &&
                string.Equals(row.layout, layout, StringComparison.Ordinal));
            if (entry == null) return "";
            if (!string.IsNullOrWhiteSpace(entry.blocker)) return entry.blocker;
            if (entry.wires == null || entry.wires.Length == 0) return "";

            var result = new StringBuilder($"REVIEW REQUIRED — {entry.wires.Length} authored connection candidates.\n")
                .AppendLine(entry.evidence);
            foreach (var wire in entry.wires)
            {
                bool ready = context.Layout.SceneBindings.TryResolveLivePair(context.Layout, wire.fromId,
                    wire.toId, out _, out _, out string missing);
                result.Append(wire.fromId).Append(" → ").Append(wire.toId);
                if (!ready) result.Append(" · ").Append(missing);
                result.AppendLine();
            }
            return result.Append("Review the complete procedure and explicitly authorize its current revision in the Unity Editor.").ToString();
        }
    }
}
