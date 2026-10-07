using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Coordinates AI learning challenge progression.
    ///
    /// Responsibilities:
    /// - Generate the current challenge.
    /// - Start the challenge session.
    /// - Wait for the authoritative evaluator result.
    /// - Refresh adaptive teaching after completion/failure.
    /// - Start the next challenge when requested.
    ///
    /// This class does NOT:
    /// - evaluate electrical circuits
    /// - modify the solver
    /// - modify LevelGamePlayManager
    /// - determine whether a circuit is electrically correct
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIChallengeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIChallengeSession challengeSession;

        [SerializeField]
        private SparkAIChallengeEvaluator challengeEvaluator;

        [SerializeField]
        private SparkAIAdaptiveTeachingController adaptiveTeachingController;

        [Header("Behaviour")]
        [SerializeField]
        private bool automaticallyStartFirstChallenge = false;

        [SerializeField]
        private bool automaticallyStartNextChallenge = false;
        

        public bool IsInitialized { get; private set; }

        public SparkAILessonChallenge CurrentChallenge
        {
            get
            {
                if (challengeSession == null)
                    return SparkAILessonChallenge.Invalid();

                return challengeSession.CurrentChallenge;
            }
        }

        public SparkAIChallengeSessionState State
        {
            get
            {
                if (challengeSession == null)
                    return SparkAIChallengeSessionState.None;

                return challengeSession.State;
            }
        }

        private void Awake()
        {
            IsInitialized =
                challengeSession != null &&
                challengeEvaluator != null &&
                adaptiveTeachingController != null;

            if (!IsInitialized)
            {
                Debug.LogError(
                    "[SPARK AI CHALLENGE CONTROLLER] " +
                    "Missing required references.",
                    this);

                return;
            }

            challengeSession.ChallengeCompleted +=
                HandleChallengeCompleted;

            challengeSession.ChallengeFailed +=
                HandleChallengeFailed;

            challengeSession.ChallengeCancelled +=
                HandleChallengeCancelled;
        }

        private void Start()
        {
            if (!IsInitialized)
                return;

            if (automaticallyStartFirstChallenge)
                StartFirstChallenge();
        }

        private void OnDestroy()
        {
            if (!IsInitialized)
                return;

            challengeSession.ChallengeCompleted -=
                HandleChallengeCompleted;

            challengeSession.ChallengeFailed -=
                HandleChallengeFailed;

            challengeSession.ChallengeCancelled -=
                HandleChallengeCancelled;
        }

        /// <summary>
        /// Generates and starts the first challenge.
        /// </summary>
        public bool StartFirstChallenge()
        {
            if (!IsInitialized)
                return false;

            if (challengeSession.HasActiveChallenge)
                return false;

            return challengeSession.GenerateAndStart();
        }

        /// <summary>
        /// Starts another challenge after the previous one
        /// has finished.
        /// </summary>
        public bool StartNextChallenge()
        {
            if (!IsInitialized)
                return false;

            if (challengeSession.HasActiveChallenge)
                return false;

            return challengeSession.StartNextChallenge();
        }

        /// <summary>
        /// Completes the current challenge after an authoritative
        /// gameplay system has verified the objective.
        /// </summary>
        public bool CompleteChallenge(
            string successMessage = "")
        {
            if (!IsInitialized)
                return false;

            return challengeEvaluator.EvaluateSuccess(
                successMessage);
        }

        /// <summary>
        /// Fails the current challenge after an authoritative
        /// gameplay system has reported failure.
        /// </summary>
        public bool FailChallenge(
            string failureMessage)
        {
            if (!IsInitialized)
                return false;

            return challengeEvaluator.EvaluateFailure(
                failureMessage);
        }

        /// <summary>
        /// Records a circuit modification made by the player.
        /// </summary>
        public void RecordCircuitChanged()
        {
            if (!IsInitialized)
                return;

            challengeEvaluator.RecordCircuitChanged();
        }

        /// <summary>
        /// Records a measurement made by the player.
        /// </summary>
        public void RecordMeasurementTaken()
        {
            if (!IsInitialized)
                return;

            challengeEvaluator.RecordMeasurementTaken();
        }

        /// <summary>
        /// Records that the player answered the challenge.
        /// </summary>
        public void RecordPlayerAnswer()
        {
            if (!IsInitialized)
                return;

            challengeEvaluator.RecordPlayerAnswer();
        }

        private void HandleChallengeCompleted(
            SparkAILessonChallenge challenge,
            string message)
        {
            if (!IsInitialized)
                return;

            adaptiveTeachingController
                .RefreshAfterTeaching();

            if (automaticallyStartNextChallenge)
                StartNextChallenge();
        }

        private void HandleChallengeFailed(
            SparkAILessonChallenge challenge,
            string message)
        {
            if (!IsInitialized)
                return;

            adaptiveTeachingController
                .RefreshAfterTeaching();
        }

        private void HandleChallengeCancelled(
            SparkAILessonChallenge challenge,
            string message)
        {
            if (!IsInitialized)
                return;

            adaptiveTeachingController
                .RefreshAfterTeaching();
        }


        /// <summary>
/// Starts the first challenge using the active lesson's
/// authored curriculum challenge when available.
///
/// Falls back to adaptive generation when no authored
/// challenge is available.
/// </summary>
/// 
/// 
public bool StartFirstLessonAwareChallenge()
{
    if (challengeSession == null)
        return false;

    return challengeSession.GenerateAndStartLessonAware();
}

        /// <summary>
        /// Resets the current challenge and evaluator evidence.
        /// </summary>
        public void Reset()
        {
            if (!IsInitialized)
                return;

            challengeEvaluator.Reset();
            challengeSession.Reset();
        }
    }
}