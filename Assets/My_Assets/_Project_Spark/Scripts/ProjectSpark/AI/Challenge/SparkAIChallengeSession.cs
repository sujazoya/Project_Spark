using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Manages the lifecycle of the currently active AI learning challenge.
    ///
    /// Important:
    /// - Does NOT evaluate electrical correctness.
    /// - Does NOT modify the circuit.
    /// - Does NOT replace LevelGamePlayManager or SparkLevelEvaluator.
    /// - Receives the authoritative result from Project Spark gameplay systems.
    ///
    /// Flow:
    /// Challenge Generator
    ///        ↓
    /// Challenge Session
    ///        ↓
    /// Player performs task
    ///        ↓
    /// Authoritative gameplay evaluation
    ///        ↓
    /// Complete / Fail / Cancel
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIChallengeSession : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIChallengeGenerator challengeGenerator;

        [SerializeField]
        private SparkAIPlayerResponseObserver playerResponseObserver;

        [Header("Behaviour")]
        [SerializeField]
        private bool automaticallyGenerateChallenge = false;

        private SparkAILessonChallenge currentChallenge;

        private SparkAIChallengeSessionState state =
            SparkAIChallengeSessionState.None;

        private float stateChangedAt;

        private string resultMessage = string.Empty;

        public bool IsInitialized { get; private set; }

        public SparkAIChallengeSessionState State => state;

        public SparkAILessonChallenge CurrentChallenge =>
            currentChallenge;

        public float StateElapsedTime =>
            IsInitialized
                ? Mathf.Max(0f, Time.time - stateChangedAt)
                : 0f;

        public string ResultMessage => resultMessage;

        public bool HasActiveChallenge =>
            state == SparkAIChallengeSessionState.Active;

        public bool IsCompleted =>
            state == SparkAIChallengeSessionState.Completed;

        public bool IsFailed =>
            state == SparkAIChallengeSessionState.Failed;

        public bool IsCancelled =>
            state == SparkAIChallengeSessionState.Cancelled;

        public event Action<SparkAIChallengeSessionState> StateChanged;

        public event Action<SparkAILessonChallenge> ChallengeStarted;

        public event Action<SparkAILessonChallenge, string> ChallengeCompleted;

        public event Action<SparkAILessonChallenge, string> ChallengeFailed;

        public event Action<SparkAILessonChallenge, string> ChallengeCancelled;

        private void Awake()
        {
            IsInitialized =
                challengeGenerator != null &&
                playerResponseObserver != null;

            if (!IsInitialized)
            {
                Debug.LogError(
                    "[SPARK AI CHALLENGE SESSION] Missing required references.",
                    this);

                return;
            }

            currentChallenge =
                SparkAILessonChallenge.Invalid();

            state =
                SparkAIChallengeSessionState.None;

            stateChangedAt =
                Time.time;

            resultMessage =
                string.Empty;
        }

        private void Start()
        {
            if (!IsInitialized)
                return;

            if (automaticallyGenerateChallenge)
                GenerateAndStart();
        }

        /// <summary>
        /// Generates the current challenge through the existing
        /// challenge generator and starts it.
        /// </summary>
       /// <summary>
/// Generates the current challenge through the existing
/// challenge generator and starts it.
/// </summary>
public bool GenerateAndStart()
{
    if (!IsInitialized)
        return false;

    SparkAILessonChallenge challenge =
        challengeGenerator.GenerateCurrentChallenge();

    if (!challenge.IsValid)
        return false;

    return StartChallenge(challenge);
}

        /// <summary>
        /// Starts a specific already-generated challenge.
        /// </summary>
        public bool StartChallenge(
            SparkAILessonChallenge challenge)
        {
            if (!IsInitialized)
                return false;

            if (!challenge.IsValid)
                return false;

            currentChallenge =
                challenge;

            resultMessage =
                string.Empty;

            SetState(
                SparkAIChallengeSessionState.Active);

            playerResponseObserver.RecordAction(
                SparkAIPlayerResponseType.ToolUsed,
                "Started learning challenge: " +
                challenge.Title,
                challenge.ConceptId);

            ChallengeStarted?.Invoke(
                currentChallenge);

            return true;
        }

        /// <summary>
        /// Marks the active challenge as completed.
        ///
        /// The caller is responsible for determining correctness
        /// using the authoritative Project Spark systems.
        /// </summary>
        public bool Complete(
            string message = "")
        {
            if (!IsInitialized)
                return false;

            if (!HasActiveChallenge)
                return false;

            resultMessage =
                string.IsNullOrEmpty(message)
                    ? currentChallenge.SuccessDescription
                    : message;

            SetState(
                SparkAIChallengeSessionState.Completed);

            playerResponseObserver.RecordEvaluation(
                resultMessage,
                currentChallenge.ConceptId,
                true,
                1f,
                currentChallenge.ChallengeId);

            ChallengeCompleted?.Invoke(
                currentChallenge,
                resultMessage);

            return true;
        }

        /// <summary>
        /// Marks the active challenge as failed.
        ///
        /// The caller is responsible for determining failure using
        /// authoritative gameplay/electrical evaluation.
        /// </summary>
        public bool Fail(
            string message)
        {
            if (!IsInitialized)
                return false;

            if (!HasActiveChallenge)
                return false;

            resultMessage =
                string.IsNullOrEmpty(message)
                    ? "The challenge was not completed."
                    : message;

            SetState(
                SparkAIChallengeSessionState.Failed);

            playerResponseObserver.RecordEvaluation(
                resultMessage,
                currentChallenge.ConceptId,
                false,
                1f,
                currentChallenge.ChallengeId);

            ChallengeFailed?.Invoke(
                currentChallenge,
                resultMessage);

            return true;
        }

        /// <summary>
        /// Cancels the current challenge without treating it
        /// as a correct or incorrect answer.
        /// </summary>
        public bool Cancel(
            string message = "")
        {
            if (!IsInitialized)
                return false;

            if (!HasActiveChallenge)
                return false;

            resultMessage =
                string.IsNullOrEmpty(message)
                    ? "Challenge cancelled."
                    : message;

            SetState(
                SparkAIChallengeSessionState.Cancelled);

            playerResponseObserver.RecordAction(
                SparkAIPlayerResponseType.ActionPerformed,
                resultMessage,
                currentChallenge.ConceptId);

            ChallengeCancelled?.Invoke(
                currentChallenge,
                resultMessage);

            return true;
        }

        /// <summary>
        /// Resets the session without generating a new challenge.
        /// </summary>
        public void Reset()
        {
            if (!IsInitialized)
                return;

            currentChallenge =
                SparkAILessonChallenge.Invalid();

            resultMessage =
                string.Empty;

            SetState(
                SparkAIChallengeSessionState.None);
        }

        /// <summary>
        /// Starts the next generated challenge after the current
        /// challenge has finished.
        /// </summary>
      public bool StartNextChallenge()
{
    if (!IsInitialized)
        return false;

    if (state == SparkAIChallengeSessionState.Active)
        return false;

    return GenerateAndStart();
}

/// <summary>
/// Generates and starts a challenge using the active lesson's
/// authored challenge definitions when available.
///
/// Falls back to the existing adaptive challenge generation
/// through the generator when no authored challenge is available.
/// </summary>
public bool GenerateAndStartLessonAware()
{
    if (challengeGenerator == null)
        return false;

    SparkAILessonChallenge challenge =
        challengeGenerator.GenerateLessonAwareChallenge();

    if (!challenge.IsValid)
        return false;

    return StartChallenge(challenge);
}

        private void SetState(
            SparkAIChallengeSessionState newState)
        {
            if (state == newState)
                return;

            state =
                newState;

            stateChangedAt =
                Time.time;

            StateChanged?.Invoke(
                state);
        }
    }

    public enum SparkAIChallengeSessionState
    {
        None,
        Active,
        Completed,
        Failed,
        Cancelled
    }
}