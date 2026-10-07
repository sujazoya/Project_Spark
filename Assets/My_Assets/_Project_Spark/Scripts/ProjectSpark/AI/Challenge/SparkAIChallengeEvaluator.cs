using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Evaluates whether the currently active AI learning challenge
    /// has been satisfied by the authoritative Project Spark systems.
    ///
    /// This class does NOT:
    /// - modify the circuit
    /// - modify electrical values
    /// - replace SparkLevelEvaluator
    /// - replace LevelGamePlayManager
    /// - guess electrical correctness
    ///
    /// Instead, authoritative gameplay code explicitly reports the
    /// result through EvaluateSuccess / EvaluateFailure.
    ///
    /// Later, a dedicated bridge can connect LevelGamePlayManager
    /// and SparkLevelEvaluator events to this class.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIChallengeEvaluator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIChallengeSession challengeSession;

        [Header("Evaluation")]
        [SerializeField]
        private bool requireCircuitChangeBeforeSuccess = true;

        [SerializeField]
        private bool requireMeasurementBeforeSuccess = false;

        [SerializeField]
        private bool requirePlayerAnswerBeforeSuccess = false;

        private bool circuitChanged;
        private bool measurementTaken;
        private bool playerAnswered;

        public bool IsInitialized { get; private set; }

        public bool IsChallengeActive =>
            challengeSession != null &&
            challengeSession.HasActiveChallenge;

        public bool CircuitChanged => circuitChanged;

        public bool MeasurementTaken => measurementTaken;

        public bool PlayerAnswered => playerAnswered;

        private void Awake()
        {
            IsInitialized =
                challengeSession != null;

            if (!IsInitialized)
            {
                Debug.LogError(
                    "[SPARK AI CHALLENGE EVALUATOR] " +
                    "Missing SparkAIChallengeSession reference.",
                    this);

                return;
            }

            ResetEvidence();
        }

        /// <summary>
        /// Records that the player changed the circuit.
        ///
        /// This does not mean the circuit is correct.
        /// It only records evidence that the required action occurred.
        /// </summary>
        public void RecordCircuitChanged()
        {
            if (!IsInitialized)
                return;

            circuitChanged = true;
        }

        /// <summary>
        /// Records that the player performed a measurement.
        /// </summary>
        public void RecordMeasurementTaken()
        {
            if (!IsInitialized)
                return;

            measurementTaken = true;
        }

        /// <summary>
        /// Records that the player answered the challenge.
        /// </summary>
        public void RecordPlayerAnswer()
        {
            if (!IsInitialized)
                return;

            playerAnswered = true;
        }

        /// <summary>
        /// Evaluates the challenge after authoritative gameplay
        /// systems have determined that the objective is satisfied.
        ///
        /// Returns true only if the challenge was actually completed.
        /// </summary>
        public bool EvaluateSuccess(
            string successMessage = "")
        {
            if (!IsInitialized)
                return false;

            if (!IsChallengeActive)
                return false;

            SparkAILessonChallenge challenge =
                challengeSession.CurrentChallenge;

            if (!challenge.IsValid)
                return false;

            if (!RequiredEvidenceSatisfied(challenge))
                return false;

            return challengeSession.Complete(
                successMessage);
        }

        /// <summary>
        /// Reports an authoritative failure.
        ///
        /// This should be called by gameplay/level systems when
        /// they determine that the challenge cannot currently
        /// be considered successful.
        /// </summary>
        public bool EvaluateFailure(
            string failureMessage)
        {
            if (!IsInitialized)
                return false;

            if (!IsChallengeActive)
                return false;

            return challengeSession.Fail(
                failureMessage);
        }

        /// <summary>
        /// Checks only the educational action requirements
        /// declared by the challenge.
        ///
        /// Electrical correctness must still come from the
        /// authoritative Project Spark systems.
        /// </summary>
        private bool RequiredEvidenceSatisfied(
            SparkAILessonChallenge challenge)
        {
            if (requireCircuitChangeBeforeSuccess &&
                challenge.RequiresCircuitChange &&
                !circuitChanged)
            {
                return false;
            }

            if (requireMeasurementBeforeSuccess &&
                challenge.RequiresMeasurement &&
                !measurementTaken)
            {
                return false;
            }

            if (requirePlayerAnswerBeforeSuccess &&
                challenge.RequiresPlayerAnswer &&
                !playerAnswered)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clears action evidence for the current/next challenge.
        /// </summary>
        public void ResetEvidence()
        {
            circuitChanged = false;
            measurementTaken = false;
            playerAnswered = false;
        }

        /// <summary>
        /// Manually resets the evaluator and challenge evidence.
        /// </summary>
        public void Reset()
        {
            ResetEvidence();
        }
    }
}