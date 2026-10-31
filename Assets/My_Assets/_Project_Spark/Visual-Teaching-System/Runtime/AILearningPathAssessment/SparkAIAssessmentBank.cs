using System;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    public enum SparkAssessmentItemKind
    {
        MultipleChoice,
        Numeric,
        VerifiedPractical
    }

    [Serializable]
    public sealed class SparkAssessmentItem
    {
        public string itemId;
        public string title;
        [TextArea(2, 5)] public string prompt;
        public SparkAssessmentItemKind kind;
        public string[] choices = Array.Empty<string>();
        [Range(0, 3)] public int correctChoice;
        public float expectedNumber;
        [Min(0f)] public float numericTolerance = 0.01f;
        public string numericUnit;
        public SparkLevelTarget.TargetType requiredTargetType;
        [TextArea(2, 4)] public string hint;
        [TextArea(2, 6)] public string explanation;
    }

    [CreateAssetMenu(menuName = "Project Spark/AI/Learning Path Assessment Bank",
        fileName = "SparkAssessmentBank")]
    public sealed class SparkAIAssessmentBank : ScriptableObject
    {
        public string bankId;
        public string courseId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        [Range(0, 100)] public int passingPercent = 70;
        public SparkAssessmentItem[] items = Array.Empty<SparkAssessmentItem>();
    }
}
