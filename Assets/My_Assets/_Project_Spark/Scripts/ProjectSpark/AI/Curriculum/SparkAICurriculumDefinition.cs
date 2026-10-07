using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Project Spark AI learning curriculum.
    ///
    /// This is a data definition only.
    ///
    /// Structure:
    ///
    /// Curriculum
    ///     └── Levels
    ///          └── Lessons
    ///               ├── Concepts
    ///               ├── Objectives
    ///               ├── Required Challenges
    ///               ├── Optional Challenges
    ///               └── Mastery Requirement
    ///
    /// This asset does NOT control the actual Project Spark
    /// electrical level system.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkAICurriculum",
        menuName = "Project Spark/AI/Curriculum Definition")]
    public sealed class SparkAICurriculumDefinition : ScriptableObject
    {
        [Header("Curriculum")]
        [SerializeField] private string curriculumId = "project_spark_basic_electronics";

        [SerializeField] private string displayName =
            "Project Spark Electrical Learning";

        [TextArea(2, 5)]
        [SerializeField] private string description =
            "Progressive electrical learning curriculum.";

        [Header("Levels")]
        [SerializeField] private List<SparkAICurriculumLevel> levels =
            new List<SparkAICurriculumLevel>();

        public string CurriculumId => curriculumId;

        public string DisplayName => displayName;

        public string Description => description;

        public IReadOnlyList<SparkAICurriculumLevel> Levels => levels;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levels == null)
                levels = new List<SparkAICurriculumLevel>();

            curriculumId = curriculumId?.Trim() ?? string.Empty;
            displayName = displayName?.Trim() ?? string.Empty;
        }
#endif
    }


    /// <summary>
    /// One learning level inside the AI curriculum.
    ///
    /// This represents the educational structure of a level.
    /// It is intentionally separate from SparkLevelDefinition,
    /// which remains responsible for actual gameplay/electrical
    /// level evaluation.
    /// </summary>
    [Serializable]
    public struct SparkAICurriculumLevel
    {
        [Header("Identity")]
        [SerializeField] private string levelId;

        [SerializeField] private int levelNumber;

        [SerializeField] private string displayName;

        [TextArea(2, 5)]
        [SerializeField] private string description;

        [Header("Learning")]
        [TextArea(2, 5)]
        [SerializeField] private string learningGoal;

        [TextArea(2, 5)]
        [SerializeField] private string playerOutcome;

        [Header("Progression")]
        [SerializeField] private int minimumMasteryPercent;

        [SerializeField] private bool requiresAllRequiredLessons;

        [Header("Lessons")]
        [SerializeField] private List<SparkAILessonDefinition> lessons;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(levelId);

        public string LevelId => levelId;

        public int LevelNumber => levelNumber;

        public string DisplayName => displayName;

        public string Description => description;

        public string LearningGoal => learningGoal;

        public string PlayerOutcome => playerOutcome;

        public int MinimumMasteryPercent =>
            Mathf.Clamp(minimumMasteryPercent, 0, 100);

        public bool RequiresAllRequiredLessons =>
            requiresAllRequiredLessons;

        public IReadOnlyList<SparkAILessonDefinition> Lessons =>
            lessons;


        /// <summary>
        /// Finds a lesson by its ID.
        /// </summary>
        public bool TryGetLesson(
            string lessonId,
            out SparkAILessonDefinition lesson)
        {
            lesson = default;

            if (string.IsNullOrWhiteSpace(lessonId))
                return false;

            if (lessons == null || lessons.Count == 0)
                return false;

            for (int i = 0; i < lessons.Count; i++)
            {
                SparkAILessonDefinition candidate = lessons[i];

                if (!candidate.IsValid)
                    continue;

                if (string.Equals(
                        candidate.Id,
                        lessonId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    lesson = candidate;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Finds a lesson by its position in the level.
        /// </summary>
        public bool TryGetLesson(
            int index,
            out SparkAILessonDefinition lesson)
        {
            lesson = default;

            if (lessons == null)
                return false;

            if (index < 0 || index >= lessons.Count)
                return false;

            SparkAILessonDefinition candidate = lessons[index];

            if (!candidate.IsValid)
                return false;

            lesson = candidate;
            return true;
        }


        /// <summary>
        /// Returns the number of lessons in this curriculum level.
        /// </summary>
        public int LessonCount =>
            lessons != null ? lessons.Count : 0;
    }
}