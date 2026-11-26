using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>An explicitly reviewed practice wiring plan. Never grants achievements or modifies evaluators.</summary>
    [CreateAssetMenu(fileName = "SparkNativeWiringPlan", menuName = "Project Spark/Spark AI/Native Wiring Plan")]
    public sealed class SparkNativeWiringPlan : ScriptableObject
    {
        [Serializable]
        public sealed class WireStep
        {
            public string fromId;
            public string toId;
        }

        [Serializable]
        public sealed class RecordedDraft
        {
            public int schemaVersion = 1;
            public int levelNumber;
            public string layoutName;
            public string recordedUtc;
            public WireStep[] wires;
        }

        [SerializeField] private int levelNumber;
        [SerializeField] private string layoutName;
        [SerializeField, TextArea] private string schematicReference;
        [SerializeField] private string reviewedBy;
        [SerializeField] private bool approved;
        [SerializeField, HideInInspector] private string approvedDigest;
        [SerializeField] private List<WireStep> wires = new List<WireStep>();

        public int LevelNumber => levelNumber;
        public string LayoutName => layoutName;
        public IReadOnlyList<WireStep> Wires => wires;
        public bool IsApproved => approved && !string.IsNullOrWhiteSpace(reviewedBy) &&
            !string.IsNullOrWhiteSpace(schematicReference) &&
            string.Equals(approvedDigest, ComputeDigest(), StringComparison.Ordinal);

        public void LoadDraft(RecordedDraft draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            levelNumber = draft.levelNumber;
            layoutName = draft.layoutName;
            wires.Clear();
            if (draft.wires != null)
                for (int i = 0; i < draft.wires.Length; i++)
                    if (draft.wires[i] != null)
                        wires.Add(new WireStep { fromId = draft.wires[i].fromId, toId = draft.wires[i].toId });
            approved = false;
            approvedDigest = string.Empty;
            reviewedBy = string.Empty;
            schematicReference = string.Empty;
        }

        public bool TryApprove(string reviewer, string schematic, out string error)
        {
            reviewedBy = reviewer == null ? string.Empty : reviewer.Trim();
            schematicReference = schematic == null ? string.Empty : schematic.Trim();
            approved = false;
            approvedDigest = string.Empty;
            if (!Validate(out error)) return false;
            approvedDigest = ComputeDigest();
            approved = true;
            return true;
        }

        public bool Validate(out string error)
        {
            error = null;
            if (levelNumber < 1 || levelNumber > 9999 || string.IsNullOrWhiteSpace(layoutName))
                error = "A valid level number and layout name are required.";
            else if (string.IsNullOrWhiteSpace(reviewedBy) || string.IsNullOrWhiteSpace(schematicReference))
                error = "Reviewer and verified schematic reference are required.";
            else if (wires == null || wires.Count == 0)
                error = "The plan contains no wires.";
            else
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < wires.Count; i++)
                {
                    WireStep w = wires[i];
                    if (w == null || string.IsNullOrWhiteSpace(w.fromId) ||
                        string.IsNullOrWhiteSpace(w.toId) || w.fromId == w.toId)
                        { error = "Invalid endpoints at step " + (i + 1); break; }
                    string a = w.fromId.Trim(), b = w.toId.Trim();
                    string pair = string.CompareOrdinal(a, b) < 0 ? a + "\n" + b : b + "\n" + a;
                    if (!seen.Add(pair)) { error = "Duplicate wire at step " + (i + 1); break; }
                }
            }
            return error == null;
        }

        private string ComputeDigest()
        {
            var text = new StringBuilder().Append(levelNumber).Append('|').Append(layoutName)
                .Append('|').Append(schematicReference).Append('|').Append(reviewedBy);
            if (wires != null)
                for (int i = 0; i < wires.Count; i++)
                    text.Append('\n').Append(wires[i]?.fromId).Append("->").Append(wires[i]?.toId);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
                return BitConverter.ToString(digest).Replace("-", "");
            }
        }

        private void OnValidate()
        {
            // An edited plan automatically loses its *effective* approval through the digest mismatch.
        }
    }
}
