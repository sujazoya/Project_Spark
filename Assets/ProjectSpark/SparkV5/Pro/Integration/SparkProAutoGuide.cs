using System;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Source-grounded, UNGRADED walkthrough of an authored objective.
    /// Separate from reviewed lessons so automatic content cannot award Academy mastery.
    /// Auto assets are generated only by the Editor, under Resources/SparkAutoGuides.
    /// </summary>
    public sealed class SparkProAutoGuide : ScriptableObject
    {
        [Header("Authored source identity")]
        public string lessonId;
        public int level;
        public string objectiveId;
        public string title;
        [TextArea(3, 9)] public string authoredRequirement;
        public string sourceAsset;
        public string sourceFingerprint;
        public string contentSignature;
        public string[] aliases = Array.Empty<string>();
        public string[] authoredVisualReferences = Array.Empty<string>();
        public string[] authoredPracticeReferences = Array.Empty<string>();

        [Header("Auto-generated tutorial (NOT graded)")]
        public string recipeId;
        [TextArea(3, 9)] public string concept;
        [TextArea(2, 6)] public string analogy;
        [TextArea(1, 5)] public string diagram;
        [TextArea(2, 6)] public string misconception;
        [TextArea(2, 5)] public string baseline;
        [TextArea(2, 5)] public string prediction;
        [TextArea(2, 6)] public string practice;
        [Tooltip("This is permanently true for automatically generated guides.")]
        public bool guidedOnly = true;

        public bool IsUsable(out string issue)
        {
            issue = null;
            if (!guidedOnly) { issue = "Auto assets cannot grade answers"; return false; }
            if (level < 1 || level > 130 || string.IsNullOrWhiteSpace(lessonId) ||
                string.IsNullOrWhiteSpace(objectiveId) || string.IsNullOrWhiteSpace(sourceAsset) ||
                string.IsNullOrWhiteSpace(sourceFingerprint) ||
                string.IsNullOrWhiteSpace(contentSignature))
            { issue = "Missing grounded source identity"; return false; }
            if (!string.Equals(lessonId, "L" + level.ToString("D3") + "/" + objectiveId,
                StringComparison.Ordinal))
            { issue = "Objective identity differs from lesson ID"; return false; }
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(authoredRequirement) ||
                string.IsNullOrWhiteSpace(concept) || string.IsNullOrWhiteSpace(misconception) ||
                string.IsNullOrWhiteSpace(baseline) || string.IsNullOrWhiteSpace(prediction) ||
                string.IsNullOrWhiteSpace(practice) || aliases == null || aliases.Length == 0)
            { issue = "Missing instructional content"; return false; }
            return true;
        }
    }
}
