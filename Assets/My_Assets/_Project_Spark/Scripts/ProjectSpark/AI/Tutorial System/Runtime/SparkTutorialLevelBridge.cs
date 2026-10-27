
using System.Collections;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Connects the ProjectSpark level lifecycle
    /// to the AI tutorial.
    ///
    /// The gameplay manager remains authoritative.
    /// This class never completes levels, changes
    /// electrical states, or moves components.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialLevelBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private LevelGamePlayManager levelManager;

        [SerializeField]
        private SparkTutorialDirector director;

        [Header("Startup")]
        [SerializeField]
        private bool automaticallyStartTutorial = true;

        [SerializeField]
        private bool skipTutorialForPreviouslyWonLevels = true;

        [SerializeField]
        private bool resumeExistingTutorialProgress = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logEvents = true;

        private LevelGamePlayManager subscribedManager;
        private Coroutine pendingStart;
        private int generation;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void Start()
        {
            ResolveReferences();
            Subscribe();

            // The level manager may already have started
            // its first level in Awake().
            SynchronizeCurrentLevel();
        }

        private void OnDisable()
        {
            generation++;
            CancelPendingStart();
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (levelManager == null)
            {
                levelManager =
                    FindFirstObjectByType<
                        LevelGamePlayManager>();
            }

            if (director == null)
            {
                director =
                    GetComponent<SparkTutorialDirector>();
            }

            if (director == null)
            {
                director =
                    FindFirstObjectByType<
                        SparkTutorialDirector>();
            }
        }

        private void Subscribe()
        {
            if (subscribedManager == levelManager)
                return;

            Unsubscribe();

            if (levelManager == null)
                return;

            levelManager.LevelStarted += OnLevelStarted;
            levelManager.LevelReset += OnLevelReset;
            levelManager.LevelCompleted += OnLevelCompleted;
            levelManager.LevelFailed += OnLevelFailed;

            subscribedManager = levelManager;
        }

        private void Unsubscribe()
        {
            if (subscribedManager == null)
                return;

            subscribedManager.LevelStarted -= OnLevelStarted;
            subscribedManager.LevelReset -= OnLevelReset;
            subscribedManager.LevelCompleted -= OnLevelCompleted;
            subscribedManager.LevelFailed -= OnLevelFailed;

            subscribedManager = null;
        }

        private void SynchronizeCurrentLevel()
        {
            if (levelManager == null || director == null)
                return;

            SparkLevelDefinition level =
                levelManager.ActiveLevel;

            if (level == null)
                return;

            // Handle startup after the level manager's Awake().
            if (levelManager.IsCompleted)
            {
                HandleCompletion(level);
            }
            else if (levelManager.IsPlaying)
            {
                ScheduleTutorial(level);
            }
        }

        private void OnLevelStarted(
            SparkLevelDefinition level)
        {
            ScheduleTutorial(level);
        }

        private void OnLevelReset(
            SparkLevelDefinition level)
        {
            ScheduleTutorial(level);
        }

        private void OnLevelCompleted(
            SparkLevelDefinition level)
        {
            HandleCompletion(level);
        }

        private void OnLevelFailed(
            SparkLevelDefinition level,
            string reason)
        {
            generation++;
            CancelPendingStart();

            // Stop instructions when the level is failed.
            if (director != null && director.IsRunning)
            {
                director.Close();
            }

            if (logEvents)
            {
                Debug.Log(
                    "[TutorialBridge] Level failed: " +
                    level.DisplayName +
                    " | Reason=" + reason,
                    this);
            }
        }

        private void ScheduleTutorial(
            SparkLevelDefinition level)
        {
            generation++;
            CancelPendingStart();

            if (director == null || level == null)
                return;

            // A new run must clear the previous
            // runtime victory flag, even if the same
            // level is being restarted.
            director.SetActiveLevel(level.LevelId);

            if (director.IsRunning)
                director.Close();

            if (!automaticallyStartTutorial)
                return;

            if (skipTutorialForPreviouslyWonLevels &&
                HasObservedVictory(level.LevelId))
            {
                Log("Skipped previously won level: " +
                    level.LevelId);
                return;
            }

            pendingStart = StartCoroutine(
                StartTutorialAfterLevelReady(
                    level.LevelId,
                    generation));
        }

        private IEnumerator StartTutorialAfterLevelReady(
            string expectedLevelId,
            int requestGeneration)
        {
            // Allow runtime layout and shelf components
            // to finish their initialization.
            yield return null;

            pendingStart = null;

            if (requestGeneration != generation)
                yield break;

            if (levelManager == null || director == null)
                yield break;

            SparkLevelDefinition activeLevel =
                levelManager.ActiveLevel;

            if (activeLevel == null ||
                activeLevel.LevelId != expectedLevelId)
            {
                yield break;
            }

            // A level might have been won immediately
            // after its first electrical evaluation.
            if (levelManager.IsCompleted)
            {
                HandleCompletion(activeLevel);
                yield break;
            }

            if (!levelManager.IsPlaying)
                yield break;

            if (skipTutorialForPreviouslyWonLevels &&
                HasObservedVictory(expectedLevelId))
            {
                yield break;
            }

            if (director.IsRunning)
                yield break;

            if (!resumeExistingTutorialProgress)
            {
                // Checkpoint reset should be performed
                // using your tutorial definition's
                // existing restart behavior.
                Log(
                    "Resume disabled in bridge. " +
                    "Also disable Resume From Checkpoint " +
                    "on the tutorial asset.");
            }

            director.BeginDefault();

            Log("Tutorial started for level: " +
                expectedLevelId);
        }

        private void HandleCompletion(
            SparkLevelDefinition level)
        {
            generation++;
            CancelPendingStart();

            if (level == null)
                return;

            // This marker records level wins observed
            // while the bridge is installed.
            PlayerPrefs.SetInt(
                VictoryKey(level.LevelId),
                1);

            PlayerPrefs.Save();

            if (director != null)
            {
                director.NotifyLevelWon(level.LevelId);
            }

            Log("Tutorial stopped. Level completed: " +
                level.LevelId);
        }

        private void CancelPendingStart()
        {
            if (pendingStart == null)
                return;

            StopCoroutine(pendingStart);
            pendingStart = null;
        }

        private static string VictoryKey(string levelId)
        {
            return "ProjectSpark.Tutorial.LevelWon." +
                   levelId;
        }

        private static bool HasObservedVictory(
            string levelId)
        {
            return !string.IsNullOrWhiteSpace(levelId) &&
                PlayerPrefs.GetInt(
                    VictoryKey(levelId), 0) == 1;
        }

        private void Log(string message)
        {
            if (logEvents)
                Debug.Log("[TutorialBridge] " + message, this);
        }
    }
}
