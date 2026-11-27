using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ProjectSpark.Gameplay;
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
        [SerializeField, HideInInspector] private SparkLevelDefinition reviewedDefinition;
        [SerializeField, HideInInspector] private SparkLevelLayout reviewedLayout;
        [SerializeField, HideInInspector] private string reviewedRevision;
        [SerializeField, HideInInspector] private string revisionApprovalDigest;
        [SerializeField] private List<WireStep> wires = new List<WireStep>();

        public int LevelNumber => levelNumber;
        public string LayoutName => layoutName;
        public IReadOnlyList<WireStep> Wires => wires;
        public string Reviewer => reviewedBy;
        public string SchematicReference => schematicReference;
        public bool HasRevisionReview => !string.IsNullOrEmpty(revisionApprovalDigest);
        public bool HasRecordedApproval => approved;
        public string ReviewedRevision => reviewedRevision;

        public bool MatchesReviewedLayout(SparkLevelDefinition definition, SparkLevelLayout source, out string error)
        {
            error = null;
            if (!IsApproved || !HasRevisionReview)
                error = "The plan requires an explicit layout revision review.";
            else if (!definition || !source || reviewedDefinition != definition || reviewedLayout != source ||
                     source.LevelDefinition != definition || revisionApprovalDigest != ComputeRevisionDigest())
                error = "Reviewed layout identity or revision has changed.";
            return error == null;
        }

#if UNITY_EDITOR
        public bool ValidateStoredAuthoredRevision(out string error)
        {
            if (!MatchesReviewedLayout(reviewedDefinition, reviewedLayout, out error)) return false;
            if (AuthoredRevision(reviewedDefinition, reviewedLayout, schematicReference) == reviewedRevision) return true;
            error = "Authored layout or definition changed after review.";
            return false;
        }

        public static string AuthoredRevision(SparkLevelDefinition definition, SparkLevelLayout layout, string evidence = null)
        {
            if (!definition || !layout) return string.Empty;
            string entry = UnityEditor.AssetDatabase.GetAssetPath(definition);
            string prefab = UnityEditor.AssetDatabase.GetAssetPath(layout);
            if (string.IsNullOrEmpty(entry) || string.IsNullOrEmpty(prefab)) return string.Empty;
            string[] dependencies = UnityEditor.AssetDatabase.GetDependencies(prefab);
            Array.Sort(dependencies, StringComparer.Ordinal);
            var inputs = new StringBuilder().Append(entry).Append(':').Append(HashAuthoredFile(entry));
            for (int i = 0; i < dependencies.Length; i++)
                if (dependencies[i].EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    inputs.Append('\n').Append(dependencies[i]).Append(':').Append(HashAuthoredFile(dependencies[i]));
            int separator = evidence?.IndexOf('#') ?? -1;
            if (separator > 0)
            {
                string controller = evidence.Substring(0, separator);
                if (controller.StartsWith("Assets/", StringComparison.Ordinal) &&
                    controller.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    if (!UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(controller))
                        return string.Empty;
                    inputs.Append('\n').Append(controller).Append(':').Append(HashAuthoredFile(controller));
                }
            }
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(inputs.ToString()))).Replace("-", "");
        }

        private static string HashAuthoredFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using var file = System.IO.File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }

        public bool ReviewAuthoredRevision(SparkLevelDefinition definition, SparkLevelLayout layout, out string error)
        {
            error = null;
            if (!IsApproved || !definition || !layout || layout.LevelDefinition != definition ||
                LevelNumber != definition.LevelNumber || LayoutName != layout.name)
            { error = "An existing approved plan and its exact authored definition and layout are required."; return false; }
            string revision = AuthoredRevision(definition, layout, schematicReference);
            if (string.IsNullOrEmpty(revision))
            { error = "The authored layout or definition is unsaved."; return false; }
            reviewedDefinition = definition;
            reviewedLayout = layout;
            reviewedRevision = revision;
            revisionApprovalDigest = ComputeRevisionDigest();
            return true;
        }
#endif
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
            reviewedDefinition = null;
            reviewedLayout = null;
            reviewedRevision = revisionApprovalDigest = string.Empty;
        }

        public bool TryApprove(string reviewer, string schematic, out string error)
        {
            reviewedDefinition = null;
            reviewedLayout = null;
            reviewedRevision = revisionApprovalDigest = string.Empty;
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
            if (!ValidateStructure(out error)) return false;
            if (string.IsNullOrWhiteSpace(reviewedBy) || string.IsNullOrWhiteSpace(schematicReference))
                error = "Reviewer and verified schematic reference are required.";
            return error == null;
        }

        public bool ValidateStructure(out string error)
        {
            error = null;
            if (levelNumber < 1 || levelNumber > 9999 || string.IsNullOrWhiteSpace(layoutName))
                error = "A valid level number and layout name are required.";
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

        private string ComputeRevisionDigest()
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                Encoding.UTF8.GetBytes(approvedDigest + "\n" + reviewedRevision))).Replace("-", "");
        }

        private void OnValidate()
        {
            // An edited plan automatically loses its *effective* approval through the digest mismatch.
        }
    }
}
