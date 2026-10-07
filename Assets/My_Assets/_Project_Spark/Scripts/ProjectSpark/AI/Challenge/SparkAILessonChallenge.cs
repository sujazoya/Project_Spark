using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Represents one small educational gameplay challenge.
    ///
    /// A challenge describes what the player should accomplish.
    /// It does not modify the circuit or evaluate electrical truth.
    ///
    /// Project Spark's authoritative gameplay/evaluation systems
    /// remain responsible for determining whether the challenge
    /// was actually completed.
    /// </summary>
    [Serializable]
    public readonly struct SparkAILessonChallenge
    {
        public bool IsValid { get; }

        public string ChallengeId { get; }

        public string ConceptId { get; }

        public SparkAITeachingTopic Topic { get; }

        public SparkAIChallengeDifficulty Difficulty { get; }

        public string Title { get; }

        public string Objective { get; }

        public string Instruction { get; }

        public string SuccessDescription { get; }

        public string Hint { get; }

        public string StrongHint { get; }

        public bool RequiresCircuitChange { get; }

        public bool RequiresMeasurement { get; }

        public bool RequiresPlayerAnswer { get; }

        public float CreatedAt { get; }

        public SparkAILessonChallenge(
            bool isValid,
            string challengeId,
            string conceptId,
            SparkAITeachingTopic topic,
            SparkAIChallengeDifficulty difficulty,
            string title,
            string objective,
            string instruction,
            string successDescription,
            string hint,
            string strongHint,
            bool requiresCircuitChange,
            bool requiresMeasurement,
            bool requiresPlayerAnswer,
            float createdAt)
        {
            IsValid =
                isValid;

            ChallengeId =
                challengeId ??
                string.Empty;

            ConceptId =
                conceptId ??
                string.Empty;

            Topic =
                topic;

            Difficulty =
                difficulty;

            Title =
                title ??
                string.Empty;

            Objective =
                objective ??
                string.Empty;

            Instruction =
                instruction ??
                string.Empty;

            SuccessDescription =
                successDescription ??
                string.Empty;

            Hint =
                hint ??
                string.Empty;

            StrongHint =
                strongHint ??
                string.Empty;

            RequiresCircuitChange =
                requiresCircuitChange;

            RequiresMeasurement =
                requiresMeasurement;

            RequiresPlayerAnswer =
                requiresPlayerAnswer;

            CreatedAt =
                createdAt;
        }

        public static SparkAILessonChallenge Invalid()
        {
            return new SparkAILessonChallenge(
                false,
                string.Empty,
                string.Empty,
                SparkAITeachingTopic.CircuitBasics,
                SparkAIChallengeDifficulty.Easy,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                false,
                false,
                UnityEngine.Time.time);
        }
    }

    public enum SparkAIChallengeDifficulty
    {
        Easy = 0,

        Guided = 1,

        Normal = 2,

        Advanced = 3,

        Mastery = 4
    }
}