using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Records learner responses during AI teaching sessions.
    ///
    /// This is an observation layer only.
    ///
    /// It does not decide correctness unless an external system
    /// explicitly supplies an evaluation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIPlayerResponseObserver : MonoBehaviour
    {
        [Header("Teaching")]
        [SerializeField]
        private SparkAITeachingSession teachingSession;

        [Header("History")]
        [SerializeField]
        private int maximumHistoryEntries = 64;

        private readonly List<SparkAIPlayerResponse>
            responseHistory =
                new List<SparkAIPlayerResponse>(64);

        private SparkAIPlayerResponse latestResponse;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAIPlayerResponse LatestResponse =>
            latestResponse;

        public IReadOnlyList<SparkAIPlayerResponse>
            ResponseHistory =>
            responseHistory;

        public event Action<SparkAIPlayerResponse>
            ResponseRecorded;

        private void Awake()
        {
            if (teachingSession == null)
            {
                Debug.LogError(
                    "[Spark AI Player Response Observer] " +
                    "SparkAITeachingSession reference is missing.",
                    this);

                return;
            }

            maximumHistoryEntries =
                Mathf.Clamp(
                    maximumHistoryEntries,
                    8,
                    512);

            initialized = true;
        }

        /// <summary>
        /// Records a player's textual answer.
        ///
        /// Correctness remains unevaluated unless an external
        /// evaluator later supplies the result.
        /// </summary>
        public SparkAIPlayerResponse RecordTextAnswer(
            string answer)
        {
            return RecordTextAnswer(
                answer,
                string.Empty,
                string.Empty);
        }

        /// <summary>
        /// Records a player's textual answer associated with
        /// a specific concept.
        /// </summary>
        public SparkAIPlayerResponse RecordTextAnswer(
            string answer,
            string conceptId,
            string context)
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    SparkAIPlayerResponseType.TextAnswer,
                    answer,
                    conceptId,
                    context,
                    false,
                    false,
                    0f,
                    0,
                    0);

            return Record(response);
        }

        /// <summary>
        /// Records a player action related to the teaching task.
        /// </summary>
        public SparkAIPlayerResponse RecordAction(
            SparkAIPlayerResponseType type,
            string description,
            string conceptId = "",
            int primaryObjectInstanceId = 0,
            int secondaryObjectInstanceId = 0)
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    type,
                    description,
                    conceptId,
                    string.Empty,
                    false,
                    false,
                    0f,
                    primaryObjectInstanceId,
                    secondaryObjectInstanceId);

            return Record(response);
        }

        /// <summary>
        /// Records that the player requested a hint.
        /// </summary>
        public SparkAIPlayerResponse RecordHintRequested()
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    SparkAIPlayerResponseType.HintRequested,
                    "Player requested a hint.",
                    string.Empty,
                    string.Empty,
                    false,
                    false,
                    0f,
                    0,
                    0);

            return Record(response);
        }

        /// <summary>
        /// Records that the player requested the explanation.
        /// </summary>
        public SparkAIPlayerResponse RecordExplanationRequested()
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    SparkAIPlayerResponseType.ExplanationRequested,
                    "Player requested an explanation.",
                    string.Empty,
                    string.Empty,
                    false,
                    false,
                    0f,
                    0,
                    0);

            return Record(response);
        }

        /// <summary>
        /// Records an externally evaluated answer.
        ///
        /// The caller must explicitly provide the correctness result.
        /// </summary>
        public SparkAIPlayerResponse RecordEvaluation(
            string answer,
            string conceptId,
            bool isCorrect,
            float confidence,
            string context = "")
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    SparkAIPlayerResponseType.AnswerEvaluated,
                    answer,
                    conceptId,
                    context,
                    true,
                    isCorrect,
                    confidence,
                    0,
                    0);

            return Record(response);
        }

        /// <summary>
        /// Records a Project Spark gameplay action as learner evidence.
        /// </summary>
        public SparkAIPlayerResponse RecordGameplayAction(
            SparkAIPlayerResponseType type,
            string description,
            string conceptId,
            int primaryObjectInstanceId,
            int secondaryObjectInstanceId)
        {
            if (!initialized)
                return RecordInvalid();

            SparkAIPlayerResponse response =
                new SparkAIPlayerResponse(
                    true,
                    Time.time,
                    type,
                    description,
                    conceptId,
                    string.Empty,
                    false,
                    false,
                    0f,
                    primaryObjectInstanceId,
                    secondaryObjectInstanceId);

            return Record(response);
        }

        /// <summary>
        /// Returns the most recent evaluated response.
        /// </summary>
        public bool TryGetLatestEvaluation(
            out SparkAIPlayerResponse evaluation)
        {
            for (int i = responseHistory.Count - 1;
                 i >= 0;
                 i--)
            {
                SparkAIPlayerResponse response =
                    responseHistory[i];

                if (!response.IsValid)
                    continue;

                if (!response.HasEvaluation)
                    continue;

                evaluation =
                    response;

                return true;
            }

            evaluation =
                SparkAIPlayerResponse.Invalid();

            return false;
        }

        /// <summary>
        /// Returns whether the latest evaluated response for a
        /// concept was correct.
        /// </summary>
        public bool TryGetLatestConceptEvaluation(
            string conceptId,
            out bool isCorrect,
            out float confidence)
        {
            isCorrect = false;
            confidence = 0f;

            if (string.IsNullOrEmpty(conceptId))
                return false;

            for (int i = responseHistory.Count - 1;
                 i >= 0;
                 i--)
            {
                SparkAIPlayerResponse response =
                    responseHistory[i];

                if (!response.IsValid ||
                    !response.HasEvaluation)
                {
                    continue;
                }

                if (!string.Equals(
                        response.ConceptId,
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                isCorrect =
                    response.IsCorrect;

                confidence =
                    response.EvaluationConfidence;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Counts evaluated correct answers for a concept.
        /// </summary>
        public int CountCorrectAnswers(
            string conceptId)
        {
            if (string.IsNullOrEmpty(conceptId))
                return 0;

            int count = 0;

            for (int i = 0;
                 i < responseHistory.Count;
                 i++)
            {
                SparkAIPlayerResponse response =
                    responseHistory[i];

                if (!response.IsValid ||
                    !response.HasEvaluation ||
                    !response.IsCorrect)
                {
                    continue;
                }

                if (string.Equals(
                        response.ConceptId,
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Counts evaluated incorrect answers for a concept.
        /// </summary>
        public int CountIncorrectAnswers(
            string conceptId)
        {
            if (string.IsNullOrEmpty(conceptId))
                return 0;

            int count = 0;

            for (int i = 0;
                 i < responseHistory.Count;
                 i++)
            {
                SparkAIPlayerResponse response =
                    responseHistory[i];

                if (!response.IsValid ||
                    !response.HasEvaluation ||
                    response.IsCorrect)
                {
                    continue;
                }

                if (string.Equals(
                        response.ConceptId,
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Counts how many hints the player requested for the
        /// current response history.
        /// </summary>
        public int CountHintRequests()
        {
            int count = 0;

            for (int i = 0;
                 i < responseHistory.Count;
                 i++)
            {
                if (responseHistory[i].Type ==
                    SparkAIPlayerResponseType.HintRequested)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Clears learner-response history.
        /// </summary>
        public void ClearHistory()
        {
            responseHistory.Clear();

            latestResponse =
                SparkAIPlayerResponse.Invalid();
        }

        private SparkAIPlayerResponse Record(
            SparkAIPlayerResponse response)
        {
            if (!response.IsValid)
                return RecordInvalid();

            latestResponse =
                response;

            responseHistory.Add(
                response);

            TrimHistory();

            ResponseRecorded?.Invoke(
                response);

            return response;
        }

        private SparkAIPlayerResponse RecordInvalid()
        {
            return SparkAIPlayerResponse.Invalid();
        }

        private void TrimHistory()
        {
            int excess =
                responseHistory.Count -
                maximumHistoryEntries;

            if (excess <= 0)
                return;

            responseHistory.RemoveRange(
                0,
                excess);
        }
    }
}