using System;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    public enum SparkLearningPathStageKind { Lesson, VerifiedPractice }

    [Serializable]
    public sealed class SparkLearningPathStage
    {
        public string id;
        public string title;
        [TextArea(2, 5)] public string description;
        public SparkLearningPathStageKind kind;
        public SparkVisualLesson lesson;
        // The matching actual objective comes from the active LevelGamePlayManager,
        // not from authored lesson diagrams or scene object names.
        public SparkLevelTarget.TargetType requiredTargetType;
    }

    [CreateAssetMenu(menuName = "Project Spark/AI/Guided Learning Path", fileName = "SparkGuidedLearningPath")]
    public sealed class SparkGuidedLearningPathAsset : ScriptableObject
    {
        public string pathId;
        public string title;
        [TextArea(2, 4)] public string description;
        public SparkLearningPathStage[] stages = Array.Empty<SparkLearningPathStage>();
    }
}
