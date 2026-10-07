using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects Project Spark level progression with the AI lesson/challenge system.
    ///
    /// Responsibilities:
    /// - Activates the lesson belonging to the current level.
    /// - Starts a challenge when the level begins.
    /// - Provides manual challenge control.
    ///
    /// This bridge does NOT:
    /// - evaluate electrical correctness
    /// - modify circuit topology
    /// - decide level success
    /// - decide challenge correctness
    ///
    /// LevelGamePlayManager / SparkLevelEvaluator remain authoritative.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILessonChallengeBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private LevelGamePlayManager levelGamePlayManager;

        [SerializeField]
        private SparkAILesson lesson;

        [SerializeField]
        private SparkAIChallengeController challengeController;

        [Header("Behaviour")]
        [SerializeField]
        private bool startChallengeOnLevelStart = true;

        [SerializeField]
        private bool resetChallengeOnLevelStart = true;

        [SerializeField]
        private bool completeChallengeOnLevelCompletion = true;

        private string activeLevelId = string.Empty;

        // ---------------------------------------------------------
        // Unity
        // ---------------------------------------------------------

        private void OnEnable()
        {
            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted += HandleLevelStarted;
            levelGamePlayManager.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted -= HandleLevelStarted;
            levelGamePlayManager.LevelCompleted -= HandleLevelCompleted;
        }

        // ---------------------------------------------------------
        // Level Events
        // ---------------------------------------------------------

        private void HandleLevelStarted(SparkLevelDefinition level)
        {
            if (level == null)
                return;

            if (string.IsNullOrEmpty(level.LevelId))
                return;

            activeLevelId = level.LevelId;

            if (lesson == null)
                return;

            /*
             * Activate the lesson using the authoritative level ID.
             */
            bool lessonActivated =
                lesson.TrySetActiveLesson(activeLevelId);

            if (!lessonActivated)
            {
                Debug.LogWarning(
                    $"[AI LESSON CHALLENGE] No lesson could be activated for level '{activeLevelId}'.",
                    this);

                return;
            }

            if (challengeController == null)
                return;

            if (resetChallengeOnLevelStart)
                challengeController.Reset();

            if (startChallengeOnLevelStart)
                StartActiveLessonChallenge();
        }

        private void HandleLevelCompleted(SparkLevelDefinition level)
        {
            if (level == null)
                return;

            if (string.IsNullOrEmpty(activeLevelId))
                return;

            if (!string.Equals(
                    activeLevelId,
                    level.LevelId,
                    StringComparison.Ordinal))
            {
                return;
            }

            if (challengeController != null &&
                completeChallengeOnLevelCompletion)
            {
                challengeController.CompleteChallenge(
                    $"Excellent! You completed level '{activeLevelId}'.");
            }

            activeLevelId = string.Empty;
        }

        // ---------------------------------------------------------
        // Challenge Control
        // ---------------------------------------------------------

        /// <summary>
        /// Starts the first challenge for the active lesson.
        /// </summary>
        public bool StartActiveLessonChallenge()
        {
            if (!HasValidActiveLesson())
                return false;

            if (challengeController == null)
                return false;
           return challengeController.StartFirstLessonAwareChallenge();
        }

        /// <summary>
        /// Starts the next challenge for the active lesson.
        /// </summary>
        public bool StartNextLessonChallenge()
        {
            if (!HasValidActiveLesson())
                return false;

            if (challengeController == null)
                return false;

            return challengeController.StartNextChallenge();
        }

        /// <summary>
        /// Completes the current challenge.
        /// </summary>
        public bool CompleteCurrentChallenge(string message)
        {
            if (!HasValidActiveLesson())
                return false;

            if (challengeController == null)
                return false;

            return challengeController.CompleteChallenge(message);
        }

        /// <summary>
        /// Fails the current challenge.
        /// </summary>
        public bool FailCurrentChallenge(string message)
        {
            if (!HasValidActiveLesson())
                return false;

            if (challengeController == null)
                return false;

            return challengeController.FailChallenge(message);
        }

        /// <summary>
        /// Resets the current challenge.
        /// </summary>
        public void ResetCurrentChallenge()
        {
            if (challengeController == null)
                return;

            challengeController.Reset();
        }

        // ---------------------------------------------------------
        // Validation
        // ---------------------------------------------------------

        /// <summary>
        /// Checks whether a usable lesson is currently active.
        ///
        /// SparkAILessonDefinition is a struct, therefore it cannot
        /// be compared with null.
        ///
        /// We only use the known ConceptIds property here.
        /// </summary>
        private bool HasValidActiveLesson()
        {
            if (lesson == null)
                return false;

            SparkAILessonDefinition activeLesson =
                lesson.ActiveLesson;

            if (activeLesson.ConceptIds == null)
                return false;

            if (activeLesson.ConceptIds.Length == 0)
                return false;

            return true;
        }
    }
}