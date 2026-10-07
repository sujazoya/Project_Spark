using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Resolves the challenge IDs stored in the active lesson
    /// into actual SparkAIChallengeDefinition objects.
    ///
    /// Responsibilities:
    /// - Resolve required challenge IDs.
    /// - Resolve optional challenge IDs.
    /// - Report missing challenge definitions.
    ///
    /// Does NOT:
    /// - Start challenges.
    /// - Evaluate success.
    /// - Modify the circuit.
    /// - Modify the learner model.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILessonChallengeResolver : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SparkAILesson lesson;

        [SerializeField] private SparkAIChallengeDatabase database;

        [Header("Validation")]
        [SerializeField] private bool logMissingChallenges = true;

        [SerializeField] private bool logResolutionSummary = false;

        private readonly List<SparkAIChallengeDefinition> requiredChallenges =
            new List<SparkAIChallengeDefinition>();

        private readonly List<SparkAIChallengeDefinition> optionalChallenges =
            new List<SparkAIChallengeDefinition>();

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public int RequiredChallengeCount =>
            requiredChallenges.Count;

        public int OptionalChallengeCount =>
            optionalChallenges.Count;

        public void Initialize()
        {
            initialized = false;

            requiredChallenges.Clear();
            optionalChallenges.Clear();

            if (lesson == null)
            {
                Debug.LogWarning(
                    "[AI LESSON CHALLENGE RESOLVER] Lesson reference is missing.",
                    this);

                return;
            }

            if (database == null)
            {
                Debug.LogWarning(
                    "[AI LESSON CHALLENGE RESOLVER] Challenge database reference is missing.",
                    this);

                return;
            }

            database.Initialize();

            initialized = true;
        }

        /// <summary>
        /// Resolves the currently active lesson.
        /// </summary>
        public bool ResolveActiveLesson()
        {
            requiredChallenges.Clear();
            optionalChallenges.Clear();

            if (!initialized)
                Initialize();

            if (!initialized)
                return false;

            if (!lesson.HasActiveLesson)
                return false;

            SparkAILessonDefinition activeLesson =
                lesson.ActiveLesson;

            bool requiredResolved =
                ResolveRequiredChallenges(activeLesson);

            ResolveOptionalChallenges(activeLesson);

            if (logResolutionSummary)
            {
                Debug.Log(
                    $"[AI LESSON CHALLENGE RESOLVER] " +
                    $"Lesson='{activeLesson.Id}' | " +
                    $"Required={requiredChallenges.Count}/" +
                    $"{activeLesson.RequiredChallengeCount} | " +
                    $"Optional={optionalChallenges.Count}/" +
                    $"{activeLesson.OptionalChallengeCount}",
                    this);
            }

            return requiredResolved;
        }

        /// <summary>
        /// Resolves required challenge IDs.
        ///
        /// Returns false if one or more required challenge
        /// definitions are missing.
        /// </summary>
        private bool ResolveRequiredChallenges(
            SparkAILessonDefinition activeLesson)
        {
            string[] challengeIds =
                activeLesson.RequiredChallengeIds;

            if (challengeIds == null ||
                challengeIds.Length == 0)
            {
                return true;
            }

            bool allResolved = true;

            for (int i = 0; i < challengeIds.Length; i++)
            {
                string challengeId =
                    challengeIds[i];

                if (TryResolveChallenge(
                    challengeId,
                    out SparkAIChallengeDefinition challenge))
                {
                    requiredChallenges.Add(challenge);
                }
                else
                {
                    allResolved = false;

                    LogMissingChallenge(
                        activeLesson.Id,
                        challengeId,
                        true);
                }
            }

            return allResolved;
        }

        /// <summary>
        /// Resolves optional challenge IDs.
        ///
        /// Missing optional challenges do not invalidate
        /// the lesson.
        /// </summary>
        private void ResolveOptionalChallenges(
            SparkAILessonDefinition activeLesson)
        {
            string[] challengeIds =
                activeLesson.OptionalChallengeIds;

            if (challengeIds == null ||
                challengeIds.Length == 0)
            {
                return;
            }

            for (int i = 0; i < challengeIds.Length; i++)
            {
                string challengeId =
                    challengeIds[i];

                if (TryResolveChallenge(
                    challengeId,
                    out SparkAIChallengeDefinition challenge))
                {
                    optionalChallenges.Add(challenge);
                }
                else
                {
                    LogMissingChallenge(
                        activeLesson.Id,
                        challengeId,
                        false);
                }
            }
        }

        private bool TryResolveChallenge(
            string challengeId,
            out SparkAIChallengeDefinition challenge)
        {
            challenge = default;

            if (string.IsNullOrWhiteSpace(challengeId))
                return false;

            return database.TryGetChallenge(
                challengeId,
                out challenge);
        }

        private void LogMissingChallenge(
            string lessonId,
            string challengeId,
            bool required)
        {
            if (!logMissingChallenges)
                return;

            string type =
                required
                    ? "required"
                    : "optional";

            Debug.LogWarning(
                $"[AI LESSON CHALLENGE RESOLVER] " +
                $"Missing {type} challenge '{challengeId}' " +
                $"for lesson '{lessonId}'.",
                this);
        }

        /// <summary>
        /// Returns a required challenge by resolved index.
        /// </summary>
        public bool TryGetRequiredChallenge(
            int index,
            out SparkAIChallengeDefinition challenge)
        {
            challenge = default;

            if (index < 0 ||
                index >= requiredChallenges.Count)
            {
                return false;
            }

            challenge =
                requiredChallenges[index];

            return challenge.IsValid;
        }

        /// <summary>
        /// Returns an optional challenge by resolved index.
        /// </summary>
        public bool TryGetOptionalChallenge(
            int index,
            out SparkAIChallengeDefinition challenge)
        {
            challenge = default;

            if (index < 0 ||
                index >= optionalChallenges.Count)
            {
                return false;
            }

            challenge =
                optionalChallenges[index];

            return challenge.IsValid;
        }

        /// <summary>
        /// Copies resolved required challenges.
        /// </summary>
        public void CopyRequiredChallenges(
            List<SparkAIChallengeDefinition> results)
        {
            if (results == null)
                return;

            results.Clear();
            results.AddRange(requiredChallenges);
        }

        /// <summary>
        /// Copies resolved optional challenges.
        /// </summary>
        public void CopyOptionalChallenges(
            List<SparkAIChallengeDefinition> results)
        {
            if (results == null)
                return;

            results.Clear();
            results.AddRange(optionalChallenges);
        }

        /// <summary>
        /// Checks whether a particular challenge belongs
        /// to the currently resolved lesson.
        /// </summary>
        public bool ContainsChallenge(
            string challengeId)
        {
            if (string.IsNullOrWhiteSpace(challengeId))
                return false;

            for (int i = 0; i < requiredChallenges.Count; i++)
            {
                if (string.Equals(
                    requiredChallenges[i].Id,
                    challengeId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            for (int i = 0; i < optionalChallenges.Count; i++)
            {
                if (string.Equals(
                    optionalChallenges[i].Id,
                    challengeId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}