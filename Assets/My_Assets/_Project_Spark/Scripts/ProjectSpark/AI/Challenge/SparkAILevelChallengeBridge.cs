using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects Project Spark's authoritative level progression
    /// to the AI learning challenge system.
    ///
    /// Responsibilities:
    /// - Detect when a Project Spark level starts.
    /// - Start an AI challenge for the active gameplay session.
    /// - Detect authoritative level completion.
    /// - Report that completion to the AI challenge system.
    ///
    /// IMPORTANT:
    /// This component does NOT:
    /// - evaluate the circuit
    /// - modify the circuit
    /// - modify the electrical solver
    /// - determine electrical correctness
    /// - replace SparkLevelEvaluator
    /// - replace LevelGamePlayManager
    ///
    /// LevelGamePlayManager / SparkLevelEvaluator remain authoritative.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILevelChallengeBridge : MonoBehaviour
    {
        [Header("Authoritative Project Spark")]
        [SerializeField]
        private LevelGamePlayManager levelGamePlayManager;

        [Header("Spark AI")]
        [SerializeField]
        private SparkAIChallengeController challengeController;

        [Header("Behaviour")]
        [Tooltip(
            "Automatically start an AI challenge when a Project Spark " +
            "level starts.")]
        [SerializeField]
        private bool startChallengeOnLevelStart = true;

        [Tooltip(
            "When the authoritative level completes, report success " +
            "to the currently active AI challenge.")]
        [SerializeField]
        private bool completeChallengeOnLevelCompletion = true;

        [Tooltip(
            "Reset the previous AI challenge when a new level starts.")]
        [SerializeField]
        private bool resetChallengeOnLevelStart = true;

        private bool initialized;
        private bool subscribed;

        private string activeLevelId = string.Empty;

        public bool IsInitialized =>
            initialized;

        public string ActiveLevelId =>
            activeLevelId;

        public bool HasActiveChallenge
        {
            get
            {
                if (challengeController == null)
                    return false;

                return
                    challengeController.State ==
                    SparkAIChallengeSessionState.Active;
            }
        }

        private void Awake()
        {
            if (levelGamePlayManager == null)
            {
                Debug.LogError(
                    "[Spark AI Level Challenge Bridge] " +
                    "LevelGamePlayManager reference is missing.",
                    this);

                return;
            }

            if (challengeController == null)
            {
                Debug.LogError(
                    "[Spark AI Level Challenge Bridge] " +
                    "SparkAIChallengeController reference is missing.",
                    this);

                return;
            }

            initialized = true;

            Subscribe();

            ActivateCurrentLevelIfAvailable();
        }

        private void OnEnable()
        {
            if (!initialized)
                return;

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // ================================================================
        // SUBSCRIPTION
        // ================================================================

        private void Subscribe()
        {
            if (subscribed)
                return;

            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted -=
                HandleLevelStarted;

            levelGamePlayManager.LevelStarted +=
                HandleLevelStarted;

            levelGamePlayManager.LevelCompleted -=
                HandleLevelCompleted;

            levelGamePlayManager.LevelCompleted +=
                HandleLevelCompleted;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (levelGamePlayManager != null)
            {
                levelGamePlayManager.LevelStarted -=
                    HandleLevelStarted;

                levelGamePlayManager.LevelCompleted -=
                    HandleLevelCompleted;
            }

            subscribed = false;
        }

        // ================================================================
        // CURRENT LEVEL
        // ================================================================

        private void ActivateCurrentLevelIfAvailable()
        {
            if (levelGamePlayManager == null)
                return;

            SparkLevelDefinition level =
                levelGamePlayManager.ActiveLevel;

            if (level == null)
                return;

            HandleLevelStarted(level);
        }

        // ================================================================
        // LEVEL START
        // ================================================================

        private void HandleLevelStarted(
            SparkLevelDefinition level)
        {
            if (!initialized)
                return;

            if (level == null)
                return;

            activeLevelId =
                level.LevelId;

            if (string.IsNullOrWhiteSpace(activeLevelId))
            {
                activeLevelId = string.Empty;

                Debug.LogWarning(
                    "[Spark AI Level Challenge Bridge] " +
                    "Started level has no LevelId.",
                    this);

                return;
            }

            // ------------------------------------------------------------
            // RESET PREVIOUS CHALLENGE
            // ------------------------------------------------------------

            if (resetChallengeOnLevelStart)
            {
                challengeController.Reset();
            }

            // ------------------------------------------------------------
            // START NEW CHALLENGE
            // ------------------------------------------------------------

            if (startChallengeOnLevelStart)
            {
                bool started =
                    challengeController.StartFirstChallenge();

                if (!started)
                {
                    Debug.LogWarning(
                        "[Spark AI Level Challenge Bridge] " +
                        $"Could not start an AI challenge for level " +
                        $"'{activeLevelId}'.",
                        this);
                }
            }
        }

        // ================================================================
        // LEVEL COMPLETE
        // ================================================================

        private void HandleLevelCompleted(
            SparkLevelDefinition level)
        {
            if (!initialized)
                return;

            if (level == null)
                return;

            if (!completeChallengeOnLevelCompletion)
                return;

            string completedLevelId =
                level.LevelId;

            if (string.IsNullOrWhiteSpace(
                    completedLevelId))
            {
                return;
            }

            // ------------------------------------------------------------
            // PROTECT AGAINST STALE EVENTS
            // ------------------------------------------------------------

            if (!string.Equals(
                    activeLevelId,
                    completedLevelId,
                    System.StringComparison.Ordinal))
            {
                return;
            }

            // ------------------------------------------------------------
            // AUTHORITATIVE SUCCESS
            // ------------------------------------------------------------

            if (!HasActiveChallenge)
                return;

            bool completed =
                challengeController.CompleteChallenge(
                    $"Excellent! You completed Project Spark level " +
                    $"'{completedLevelId}'.");

            if (!completed)
            {
                Debug.LogWarning(
                    "[Spark AI Level Challenge Bridge] " +
                    $"Level '{completedLevelId}' completed, but the " +
                    "active AI challenge could not be completed.",
                    this);
            }
        }

        // ================================================================
        // MANUAL CONTROL
        // ================================================================

        /// <summary>
        /// Manually starts an AI challenge for the current level.
        /// </summary>
        public bool StartChallenge()
        {
            if (!initialized)
                return false;

            return challengeController.StartFirstChallenge();
        }

        /// <summary>
        /// Manually completes the active AI challenge.
        ///
        /// Only call this from an authoritative gameplay system.
        /// </summary>
        public bool CompleteChallenge(
            string message = "")
        {
            if (!initialized)
                return false;

            return challengeController.CompleteChallenge(
                message);
        }

        /// <summary>
        /// Manually fails the active AI challenge.
        ///
        /// Only call this from an authoritative gameplay system.
        /// </summary>
        public bool FailChallenge(
            string message)
        {
            if (!initialized)
                return false;

            return challengeController.FailChallenge(
                message);
        }

        /// <summary>
        /// Clears the current AI challenge.
        /// </summary>
        public void ResetChallenge()
        {
            if (!initialized)
                return;

            challengeController.Reset();
        }
    }
}