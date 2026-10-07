using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects a Project Spark learning lesson to educational concepts.
    ///
    /// This is a data-driven bridge between:
    ///
    /// Project Spark level progression
    ///          ↓
    /// Educational concepts
    ///          ↓
    /// Spark AI learning
    ///
    /// It does not modify the level, circuit, or solver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILesson : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField] private SparkAIKnowledge knowledge;
        [SerializeField] private SparkAILearning learning;

        [Header("Lesson Database")]
        [SerializeField]
        private SparkAILessonDefinition[] lessons =
            Array.Empty<SparkAILessonDefinition>();

        private readonly Dictionary<string, SparkAILessonDefinition>
            lessonById =
                new Dictionary<string, SparkAILessonDefinition>(
                    StringComparer.Ordinal);

        private SparkAILessonDefinition activeLesson;

        private bool initialized;

        public bool IsInitialized => initialized;

        public bool HasActiveLesson =>
            activeLesson.IsValid;

        public SparkAILessonDefinition ActiveLesson =>
            activeLesson;

        public int LessonCount =>
            lessons != null ? lessons.Length : 0;

        private void Awake()
        {
            BuildIndex();

            if (knowledge == null)
            {
                Debug.LogError(
                    "[Spark AI Lesson] SparkAIKnowledge reference is missing.",
                    this);

                return;
            }

            if (learning == null)
            {
                Debug.LogError(
                    "[Spark AI Lesson] SparkAILearning reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Builds the runtime lesson lookup.
        /// </summary>
        public void BuildIndex()
        {
            lessonById.Clear();

            if (lessons == null)
                return;

            for (int i = 0; i < lessons.Length; i++)
            {
                SparkAILessonDefinition lesson =
                    lessons[i];

                if (!lesson.IsValid)
                    continue;

                if (lessonById.ContainsKey(lesson.Id))
                {
                    Debug.LogWarning(
                        $"[Spark AI Lesson] Duplicate lesson ID ignored: {lesson.Id}",
                        this);

                    continue;
                }

                lessonById.Add(
                    lesson.Id,
                    lesson);
            }
        }

        /// <summary>
        /// Activates a lesson by its stable lesson ID.
        /// </summary>
        public bool TrySetActiveLesson(
            string lessonId)
        {
            if (string.IsNullOrWhiteSpace(lessonId))
            {
                ClearActiveLesson();
                return false;
            }

            if (!lessonById.TryGetValue(
                    lessonId,
                    out SparkAILessonDefinition lesson))
            {
                Debug.LogWarning(
                    $"[Spark AI Lesson] Lesson not found: {lessonId}",
                    this);

                return false;
            }

            activeLesson = lesson;

            return true;
        }

        /// <summary>
        /// Clears the active educational lesson.
        /// </summary>
        public void ClearActiveLesson()
        {
            activeLesson = default(SparkAILessonDefinition);
        }

        /// <summary>
        /// Checks whether the active lesson teaches a concept.
        /// </summary>
        public bool TeachesConcept(
            string conceptId)
        {
            if (!activeLesson.IsValid ||
                string.IsNullOrWhiteSpace(conceptId))
            {
                return false;
            }

            string[] concepts =
                activeLesson.ConceptIds;

            if (concepts == null)
                return false;

            for (int i = 0; i < concepts.Length; i++)
            {
                if (string.Equals(
                        concepts[i],
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Records a successful application of one of the
        /// active lesson's concepts.
        ///
        /// Actual gameplay/evaluation code should call this only
        /// when there is authoritative evidence of success.
        /// </summary>
        public bool RecordConceptSuccess(
            string conceptId,
            string reason = null)
        {
            if (!TeachesConcept(conceptId))
                return false;

            if (learning == null)
                return false;

            learning.RecordSuccess(
                conceptId,
                reason);

            return true;
        }

        /// <summary>
        /// Records a demonstrated struggle with one of the
        /// active lesson's concepts.
        /// </summary>
        public bool RecordConceptStruggle(
            string conceptId,
            string reason = null)
        {
            if (!TeachesConcept(conceptId))
                return false;

            if (learning == null)
                return false;

            learning.RecordStruggle(
                conceptId,
                reason);

            return true;
        }

        /// <summary>
        /// Records a repeated mistake related to the lesson.
        /// </summary>
        public bool RecordConceptMistake(
            string conceptId,
            string reason = null)
        {
            if (!TeachesConcept(conceptId))
                return false;

            if (learning == null)
                return false;

            learning.RecordRepeatedMistake(
                conceptId,
                reason);

            return true;
        }

        /// <summary>
        /// Gets the mastery of a concept currently taught
        /// by the active lesson.
        /// </summary>
        public float GetConceptMastery(
            string conceptId)
        {
            if (!TeachesConcept(conceptId))
                return 0f;

            if (learning == null)
                return 0f;

            return learning.GetMastery(
                conceptId);
        }

        /// <summary>
        /// Returns whether the player has mastered a concept
        /// currently taught by this lesson.
        /// </summary>
        public bool IsConceptMastered(
            string conceptId)
        {
            if (!TeachesConcept(conceptId))
                return false;

            if (learning == null)
                return false;

            return learning.IsMastered(
                conceptId);
        }

        /// <summary>
        /// Gets all concepts taught by the active lesson.
        /// </summary>
        public void CopyActiveConceptIds(
            List<string> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (!activeLesson.IsValid ||
                activeLesson.ConceptIds == null)
            {
                return;
            }

            string[] concepts =
                activeLesson.ConceptIds;

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                results.Add(conceptId);
            }
        }

        /// <summary>
        /// Finds a lesson definition.
        /// </summary>
        public bool TryGetLesson(
            string lessonId,
            out SparkAILessonDefinition lesson)
        {
            if (!string.IsNullOrWhiteSpace(lessonId) &&
                lessonById.TryGetValue(
                    lessonId,
                    out lesson))
            {
                return true;
            }

            lesson = default(SparkAILessonDefinition);
            return false;
        }
    }


    /// <summary>
    /// Educational definition for one Project Spark lesson.
    ///
    /// The Project Spark level itself remains authoritative.
    /// This only describes what the AI should teach from that level.
    ///
    /// C# 9 compatible.
    /// </summary>
    [Serializable]
    public struct SparkAILessonDefinition
    {
        [Header("Identity")]
        [SerializeField] private string id;

        [SerializeField] private string displayName;

        [TextArea(2, 5)]
        [SerializeField] private string description;

        [Header("Learning")]
        [TextArea(2, 5)]
        [SerializeField] private string learningObjective;

        [TextArea(2, 5)]
        [SerializeField] private string successOutcome;

        [Header("Concepts")]
        [Tooltip(
            "Concept IDs taught by this lesson. " +
            "Example: circuit_basics, voltage, current.")]
        [SerializeField]
        private string[] conceptIds;

        [Header("Difficulty")]
        [SerializeField]
        private SparkAILessonDifficulty difficulty;

        [Header("Mastery")]
        [Range(0f, 1f)]
        [SerializeField]
        private float masteryRequirement;

        [Header("Progression")]
        [SerializeField]
        private bool requiredForLevelCompletion;

        [SerializeField]
        private bool allowOptionalPractice;

        [Header("Challenges")]
        [Tooltip(
            "Challenge IDs that must be completed for this lesson.")]
        [SerializeField]
        private string[] requiredChallengeIds;

        [Tooltip(
            "Optional challenge IDs available for additional practice.")]
        [SerializeField]
        private string[] optionalChallengeIds;


        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id);

        public string Id =>
            id;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;

        public string LearningObjective =>
            learningObjective;

        public string SuccessOutcome =>
            successOutcome;

        public string[] ConceptIds =>
            conceptIds;

        public SparkAILessonDifficulty Difficulty =>
            difficulty;

        public float MasteryRequirement =>
            Mathf.Clamp01(masteryRequirement);

        public bool RequiredForLevelCompletion =>
            requiredForLevelCompletion;

        public bool AllowOptionalPractice =>
            allowOptionalPractice;

        public string[] RequiredChallengeIds =>
            requiredChallengeIds;

        public string[] OptionalChallengeIds =>
            optionalChallengeIds;

        public int ConceptCount =>
            conceptIds != null
                ? conceptIds.Length
                : 0;

        public int RequiredChallengeCount =>
            requiredChallengeIds != null
                ? requiredChallengeIds.Length
                : 0;

        public int OptionalChallengeCount =>
            optionalChallengeIds != null
                ? optionalChallengeIds.Length
                : 0;


        /// <summary>
        /// Checks whether this lesson teaches a concept.
        /// </summary>
        public bool ContainsConcept(
            string conceptId)
        {
            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            if (conceptIds == null)
                return false;

            for (int i = 0; i < conceptIds.Length; i++)
            {
                if (string.Equals(
                        conceptIds[i],
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Checks whether a challenge is required.
        /// </summary>
        public bool ContainsRequiredChallenge(
            string challengeId)
        {
            return ContainsId(
                requiredChallengeIds,
                challengeId);
        }


        /// <summary>
        /// Checks whether a challenge is optional.
        /// </summary>
        public bool ContainsOptionalChallenge(
            string challengeId)
        {
            return ContainsId(
                optionalChallengeIds,
                challengeId);
        }


        private static bool ContainsId(
            string[] ids,
            string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
                return false;

            if (ids == null)
                return false;

            for (int i = 0; i < ids.Length; i++)
            {
                if (string.Equals(
                        ids[i],
                        targetId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }


    /// <summary>
    /// Educational difficulty of a lesson.
    ///
    /// This is separate from challenge difficulty because a lesson
    /// may contain several challenges with different difficulties.
    /// </summary>
    public enum SparkAILessonDifficulty
    {
        Introduction = 0,
        Beginner = 1,
        Developing = 2,
        Intermediate = 3,
        Advanced = 4,
        Mastery = 5
    }
}