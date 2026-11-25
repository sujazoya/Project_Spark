using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Explicit wiring/switch plan for ONE authored practice level. Unlike objective pairs,
    /// each ConnectWire step is a reviewed DIRECT wire route from the approved schematic.
    /// Anode/cathode on one diode are NOT a wire route.
    /// </summary>
    [CreateAssetMenu(menuName = "Project Spark/Spark AI/Assisted Practice Action Plan",
        fileName = "SparkAssistedPlan")]
    public sealed class SparkLabActionPlan : ScriptableObject
    {
        public enum Operation { ConnectWire, OpenSwitch, CloseSwitch }

        [Serializable]
        public sealed class ActionStep
        {
            public Operation operation;
            public string fromTerminalId;
            public string toTerminalId;
            public string componentId;
            [TextArea] public string technicalReason;
        }

        [Min(1)] public int levelNumber = 1;
        [TextArea] public string schematicReference;
        public List<ActionStep> steps = new List<ActionStep>();
        [SerializeField] private bool approvedForAssistedPractice;
        [SerializeField, HideInInspector] private string reviewedFingerprint;
        [SerializeField, HideInInspector] private string reviewer;

        public string Reviewer => reviewer;
        public bool IsApproved => approvedForAssistedPractice &&
            !string.IsNullOrWhiteSpace(reviewer) &&
            !string.IsNullOrEmpty(reviewedFingerprint) &&
            string.Equals(reviewedFingerprint, ComputeFingerprint(), StringComparison.Ordinal);

        public string ComputeFingerprint()
        {
            var data = new StringBuilder();
            data.Append("SparkAgentPlanV8|").Append(levelNumber).Append('|')
                .Append(schematicReference ?? "").Append('|');
            for (int i = 0; i < steps.Count; i++)
            {
                ActionStep step = steps[i];
                if (step == null) { data.Append("NULL|"); continue; }
                data.Append((int)step.operation).Append('|')
                    .Append(step.fromTerminalId ?? "").Append('|')
                    .Append(step.toTerminalId ?? "").Append('|')
                    .Append(step.componentId ?? "").Append('|')
                    .Append(step.technicalReason ?? "").Append('|');
            }
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(data.ToString()));
                var hex = new StringBuilder(digest.Length * 2);
                for (int i = 0; i < digest.Length; i++) hex.Append(digest[i].ToString("x2"));
                return hex.ToString();
            }
        }

#if UNITY_EDITOR
        public void ReviewAndApprove(string reviewedBy)
        {
            reviewer = reviewedBy == null ? "" : reviewedBy.Trim();
            approvedForAssistedPractice = reviewer.Length > 0 && steps != null && steps.Count > 0 &&
                !string.IsNullOrWhiteSpace(schematicReference);
            reviewedFingerprint = approvedForAssistedPractice ? ComputeFingerprint() : "";
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Revoke()
        {
            approvedForAssistedPractice = false;
            reviewedFingerprint = "";
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
