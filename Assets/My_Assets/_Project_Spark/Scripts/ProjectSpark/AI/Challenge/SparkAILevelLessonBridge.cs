using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Synchronizes the active Project Spark level with the
    /// corresponding AI lesson.
    ///
    /// Also starts adaptive teaching after the lesson has been
    /// successfully activated.
    ///
    /// This bridge does not:
    /// - evaluate the circuit
    /// - modify circuit topology
    /// - modify the electrical solver
    /// - control the player
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILevelLessonBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private LevelGamePlayManager levelGamePlayManager;

        [SerializeField]
        private SparkAILesson lesson;

        [SerializeField]
        private SparkAIAdaptiveTeachingController
            adaptiveTeachingController;

        [Header("Level Start")]
        [SerializeField]
        private bool activateLessonOnLevelStart = true;

        [SerializeField]
        private bool startTeachingOnLevelStart = true;

        [Header("Level Completion")]
        [SerializeField]
        private bool recordLessonSuccessOnLevelCompletion = true;

        private string activeLevelId = string.Empty;

        private void OnEnable()
        {
            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted +=
                HandleLevelStarted;

            levelGamePlayManager.LevelCompleted +=
                HandleLevelCompleted;
        }

        private void OnDisable()
        {
            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted -=
                HandleLevelStarted;

            levelGamePlayManager.LevelCompleted -=
                HandleLevelCompleted;
        }

        private void HandleLevelStarted(
            SparkLevelDefinition level)
        {
            if (level == null)
                return;

            if (string.IsNullOrEmpty(level.LevelId))
                return;

            activeLevelId =
                level.LevelId;

            bool lessonActivated = true;

            if (activateLessonOnLevelStart)
            {
                lessonActivated =
                    TryActivateLesson(
                        activeLevelId);
            }

            if (!lessonActivated)
                return;

            if (!startTeachingOnLevelStart)
                return;

            StartTeaching();
        }

        private bool TryActivateLesson(
            string levelId)
        {
            if (lesson == null)
            {
                Debug.LogWarning(
                    "[AI LEVEL LESSON] " +
                    "SparkAILesson is not assigned.",
                    this);

                return false;
            }

            bool activated =
                lesson.TrySetActiveLesson(
                    levelId);

            if (!activated)
            {
                Debug.LogWarning(
                    "[AI LEVEL LESSON] " +
                    "No lesson found for level '" +
                    levelId +
                    "'.",
                    this);

                return false;
            }

            Debug.Log(
                "[AI LEVEL LESSON] Active lesson: " +
                lesson.ActiveLesson.DisplayName,
                this);

            return true;
        }

        private bool StartTeaching()
        {
            if (adaptiveTeachingController == null)
            {
                Debug.LogWarning(
                    "[AI LEVEL LESSON] " +
                    "Adaptive Teaching Controller is not assigned.",
                    this);

                return false;
            }

            bool started =
                adaptiveTeachingController.RefreshAndApply();

            if (!started)
            {
                Debug.LogWarning(
                    "[AI LEVEL LESSON] " +
                    "Adaptive teaching could not create a valid plan.",
                    this);

                return false;
            }

            return true;
        }

        private void HandleLevelCompleted(
            SparkLevelDefinition level)
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

            if (recordLessonSuccessOnLevelCompletion &&
                lesson != null &&
                lesson.HasActiveLesson)
            {
                string[] conceptIds =
                    lesson.ActiveLesson.ConceptIds;

                if (conceptIds != null)
                {
                    for (int i = 0;
                         i < conceptIds.Length;
                         i++)
                    {
                        string conceptId =
                            conceptIds[i];

                        if (string.IsNullOrEmpty(conceptId))
                            continue;

                        lesson.RecordConceptSuccess(
                            conceptId,
                            "Level completed.");
                    }
                }
            }

            activeLevelId =
                string.Empty;
        }
    }
}