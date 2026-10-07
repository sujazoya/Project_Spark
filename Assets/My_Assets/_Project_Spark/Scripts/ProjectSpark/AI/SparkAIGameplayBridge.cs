using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Bridge between authoritative Project Spark gameplay systems
    /// and the AI learning/challenge system.
    ///
    /// IMPORTANT:
    /// This class does not evaluate electrical correctness.
    /// It only forwards authoritative gameplay facts to the AI.
    ///
    /// Intended callers:
    /// - LevelGamePlayManager
    /// - SparkLevelEvaluator
    /// - Measurement systems
    /// - Connection systems
    /// - UI/gameplay controllers
    ///
    /// No Update loop.
    /// No circuit modification.
    /// No solver modification.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIGameplayBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIChallengeController challengeController;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            IsInitialized =
                challengeController != null;

            if (!IsInitialized)
            {
                Debug.LogError(
                    "[SPARK AI GAMEPLAY BRIDGE] " +
                    "Missing SparkAIChallengeController reference.",
                    this);
            }
        }

        /// <summary>
        /// Called when the player creates a real circuit connection.
        /// </summary>
        public void NotifyConnectionCreated()
        {
            if (!IsInitialized)
                return;

            challengeController.RecordCircuitChanged();
        }

        /// <summary>
        /// Called when the player removes a real circuit connection.
        /// </summary>
        public void NotifyConnectionRemoved()
        {
            if (!IsInitialized)
                return;

            challengeController.RecordCircuitChanged();
        }

        /// <summary>
        /// Called when the player changes a switch/component
        /// configuration.
        /// </summary>
        public void NotifyComponentChanged()
        {
            if (!IsInitialized)
                return;

            challengeController.RecordCircuitChanged();
        }

        /// <summary>
        /// Called after a real measurement is performed.
        /// </summary>
        public void NotifyMeasurementTaken()
        {
            if (!IsInitialized)
                return;

            challengeController.RecordMeasurementTaken();
        }

        /// <summary>
        /// Called when the player submits an answer to an
        /// educational question.
        /// </summary>
        public void NotifyPlayerAnswer()
        {
            if (!IsInitialized)
                return;

            challengeController.RecordPlayerAnswer();
        }

        /// <summary>
        /// Called by an authoritative gameplay system when the
        /// current challenge objective has actually been satisfied.
        ///
        /// The bridge does not determine correctness.
        /// </summary>
        public bool NotifyChallengeSucceeded(
            string message = "")
        {
            if (!IsInitialized)
                return false;

            return challengeController.CompleteChallenge(
                message);
        }

        /// <summary>
        /// Called by an authoritative gameplay system when the
        /// current challenge has failed.
        /// </summary>
        public bool NotifyChallengeFailed(
            string message)
        {
            if (!IsInitialized)
                return false;

            return challengeController.FailChallenge(
                message);
        }

        /// <summary>
        /// Starts the first AI challenge.
        /// </summary>
        public bool StartChallenge()
        {
            if (!IsInitialized)
                return false;

            return challengeController.StartFirstChallenge();
        }

        /// <summary>
        /// Starts the next AI challenge.
        /// </summary>
        public bool StartNextChallenge()
        {
            if (!IsInitialized)
                return false;

            return challengeController.StartNextChallenge();
        }

        /// <summary>
        /// Resets AI challenge state.
        /// </summary>
        public void ResetChallenge()
        {
            if (!IsInitialized)
                return;

            challengeController.Reset();
        }
    }
}