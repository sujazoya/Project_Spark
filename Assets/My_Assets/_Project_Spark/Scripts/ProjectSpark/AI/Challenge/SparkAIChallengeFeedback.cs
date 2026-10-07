using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts challenge results into educational feedback.
    ///
    /// Responsibilities:
    /// - Explain success.
    /// - Explain failure.
    /// - Provide the next learning direction.
    /// - Record learning evidence through SparkAILearnerModel.
    ///
    /// Does NOT:
    /// - evaluate circuits
    /// - modify electrical state
    /// - modify level state
    /// - replace SparkLevelEvaluator
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIChallengeFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIChallengeSession challengeSession;

        [SerializeField]
        private SparkAILearnerModel learnerModel;

        [SerializeField]
        private SparkAIAdaptiveTeachingController adaptiveTeachingController;

        private SparkAIChallengeFeedbackResult latestResult;

        public bool IsInitialized { get; private set; }

        public SparkAIChallengeFeedbackResult LatestResult =>
            latestResult;

        public event Action<SparkAIChallengeFeedbackResult> FeedbackCreated;

        private void Awake()
        {
            IsInitialized =
                challengeSession != null &&
                learnerModel != null &&
                adaptiveTeachingController != null;

            if (!IsInitialized)
            {
                Debug.LogError(
                    "[SPARK AI CHALLENGE FEEDBACK] " +
                    "Missing required references.",
                    this);

                return;
            }

            latestResult =
                SparkAIChallengeFeedbackResult.Invalid();
        }

        private void OnEnable()
        {
            if (!IsInitialized)
                return;

            challengeSession.ChallengeCompleted +=
                HandleChallengeCompleted;

            challengeSession.ChallengeFailed +=
                HandleChallengeFailed;
        }

        private void OnDisable()
        {
            if (!IsInitialized)
                return;

            challengeSession.ChallengeCompleted -=
                HandleChallengeCompleted;

            challengeSession.ChallengeFailed -=
                HandleChallengeFailed;
        }

        private void HandleChallengeCompleted(
            SparkAILessonChallenge challenge,
            string message)
        {
            CreateSuccessFeedback(
                challenge,
                message);
        }

        private void HandleChallengeFailed(
            SparkAILessonChallenge challenge,
            string message)
        {
            CreateFailureFeedback(
                challenge,
                message);
        }

        private void CreateSuccessFeedback(
            SparkAILessonChallenge challenge,
            string resultMessage)
        {
            string explanation =
                BuildSuccessExplanation(
                    challenge);

            string nextStep =
                BuildSuccessNextStep(
                    challenge);

            latestResult =
                new SparkAIChallengeFeedbackResult(
                    true,
                    true,
                    UnityEngine.Time.time,
                    challenge.ChallengeId,
                    challenge.ConceptId,
                    challenge.Title,
                    resultMessage,
                    explanation,
                    nextStep);

            learnerModel.RecordSuccessfulAction(
                challenge.ConceptId);

            FeedbackCreated?.Invoke(
                latestResult);
        }

        private void CreateFailureFeedback(
            SparkAILessonChallenge challenge,
            string resultMessage)
        {
            string explanation =
                BuildFailureExplanation(
                    challenge);

            string nextStep =
                BuildFailureNextStep(
                    challenge);

            latestResult =
                new SparkAIChallengeFeedbackResult(
                    true,
                    false,
                    UnityEngine.Time.time,
                    challenge.ChallengeId,
                    challenge.ConceptId,
                    challenge.Title,
                    resultMessage,
                    explanation,
                    nextStep);

            learnerModel.RecordIncorrectAction(
                challenge.ConceptId);

            FeedbackCreated?.Invoke(
                latestResult);
        }

        private string BuildSuccessExplanation(
            SparkAILessonChallenge challenge)
        {
            switch (challenge.Topic)
            {
                case SparkAITeachingTopic.Source:
                    return
                        "You correctly identified the electrical source. " +
                        "A source provides the potential difference that can drive current.";

                case SparkAITeachingTopic.Voltage:
                    return
                        "You created the required voltage difference. " +
                        "Voltage is the electrical potential difference between two points.";

                case SparkAITeachingTopic.Current:
                    return
                        "You created a complete path for current. " +
                        "Current can flow when the circuit provides a closed conducting path.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "You used resistance to control the circuit. " +
                        "Resistance opposes current for a given voltage.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "You matched the required polarity. " +
                        "Polarity determines which terminal is positive and which is negative.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "You correctly handled the short-circuit problem. " +
                        "A short circuit provides an unintended low-resistance path.";

                case SparkAITeachingTopic.Switch:
                    return
                        "You used the switch correctly. " +
                        "A closed switch provides a conducting path while an open switch interrupts it.";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "You completed the required circuit path. " +
                        "A useful circuit normally provides a complete path from the source through the load and back.";

                case SparkAITeachingTopic.Measurement:
                    return
                        "You performed the required measurement. " +
                        "Measurements help us observe electrical behavior without guessing.";

                case SparkAITeachingTopic.Power:
                    return
                        "You correctly worked with electrical power. " +
                        "Power describes how quickly electrical energy is transferred.";

                case SparkAITeachingTopic.CircuitBasics:
                    return
                        "You built the required circuit successfully. " +
                        "A useful circuit combines a source, a conducting path, and a load.";

                default:
                    return
                        "You completed the challenge successfully. " +
                        "The electrical behavior matched the required objective.";
            }
        }

        private string BuildFailureExplanation(
            SparkAILessonChallenge challenge)
        {
            switch (challenge.Topic)
            {
                case SparkAITeachingTopic.Source:
                    return
                        "The source requirement has not been satisfied yet. " +
                        "First identify where the circuit receives electrical energy.";

                case SparkAITeachingTopic.Voltage:
                    return
                        "The required voltage condition has not been achieved yet. " +
                        "Check the potential difference between the required points.";

                case SparkAITeachingTopic.Current:
                    return
                        "Current is not behaving as required yet. " +
                        "Check whether the circuit provides a complete conducting path.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "The resistance requirement has not been satisfied yet. " +
                        "Consider how changing resistance affects current.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "The polarity is not correct yet. " +
                        "Check which terminal must connect to the positive and negative sides.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "There is still a short-circuit problem. " +
                        "Look for an unintended low-resistance bypass around the load.";

                case SparkAITeachingTopic.Switch:
                    return
                        "The switch state is not producing the required behavior. " +
                        "Check whether the path needs to be open or closed.";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "The required path is incomplete. " +
                        "Trace the circuit from the source through the required components and back.";

                case SparkAITeachingTopic.Measurement:
                    return
                        "The required measurement has not been completed correctly yet. " +
                        "Check the measurement method and the points being tested.";

                case SparkAITeachingTopic.Power:
                    return
                        "The required power behavior has not been achieved yet. " +
                        "Remember that electrical power depends on voltage and current.";

                case SparkAITeachingTopic.CircuitBasics:
                    return
                        "The basic circuit is not correct yet. " +
                        "Check the source, connections, load, and return path.";

                default:
                    return
                        "The challenge is not complete yet. " +
                        "Check the circuit behavior and try again.";
            }
        }

        private string BuildSuccessNextStep(
            SparkAILessonChallenge challenge)
        {
            switch (challenge.Topic)
            {
                case SparkAITeachingTopic.Source:
                    return
                        "Next, learn what happens when the source is connected to a load.";

                case SparkAITeachingTopic.Voltage:
                    return
                        "Next, compare voltage at different points in the circuit.";

                case SparkAITeachingTopic.Current:
                    return
                        "Next, investigate how resistance changes the current.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "Next, experiment with different resistance values.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "Next, investigate what happens when polarity is reversed.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "Next, learn how resistance affects the severity of a short circuit.";

                case SparkAITeachingTopic.Switch:
                    return
                        "Next, observe how opening and closing the path changes current.";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "Next, trace current through a more complex circuit.";

                case SparkAITeachingTopic.Measurement:
                    return
                        "Next, compare measurements before and after changing the circuit.";

                case SparkAITeachingTopic.Power:
                    return
                        "Next, investigate how voltage and current combine to determine power.";

                default:
                    return
                        "Next, try a slightly more advanced electrical challenge.";
            }
        }

        private string BuildFailureNextStep(
            SparkAILessonChallenge challenge)
        {
            if (!string.IsNullOrEmpty(challenge.Hint))
                return challenge.Hint;

            return
                "Review the circuit behavior, make one change, and try again.";
        }

        public void Clear()
        {
            latestResult =
                SparkAIChallengeFeedbackResult.Invalid();
        }
    }

    public readonly struct SparkAIChallengeFeedbackResult
    {
        public bool IsValid { get; }

        public bool IsSuccess { get; }

        public float CreatedAt { get; }

        public string ChallengeId { get; }

        public string ConceptId { get; }

        public string ChallengeTitle { get; }

        public string ResultMessage { get; }

        public string Explanation { get; }

        public string NextStep { get; }

        public SparkAIChallengeFeedbackResult(
            bool isValid,
            bool isSuccess,
            float createdAt,
            string challengeId,
            string conceptId,
            string challengeTitle,
            string resultMessage,
            string explanation,
            string nextStep)
        {
            IsValid = isValid;
            IsSuccess = isSuccess;
            CreatedAt = createdAt;
            ChallengeId = challengeId ?? string.Empty;
            ConceptId = conceptId ?? string.Empty;
            ChallengeTitle = challengeTitle ?? string.Empty;
            ResultMessage = resultMessage ?? string.Empty;
            Explanation = explanation ?? string.Empty;
            NextStep = nextStep ?? string.Empty;
        }

        public static SparkAIChallengeFeedbackResult Invalid()
        {
            return new SparkAIChallengeFeedbackResult(
                false,
                false,
                UnityEngine.Time.time,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }
    }
}