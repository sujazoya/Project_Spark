using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Represents one learner response to an AI teaching interaction.
    ///
    /// IMPORTANT:
    /// This class records and transports learner responses.
    /// It does not assume that a player's answer is correct.
    ///
    /// Correctness can later come from:
    /// - Project Spark gameplay/evaluation systems
    /// - lesson-specific deterministic checks
    /// - a future language/AI evaluator
    /// </summary>
    public readonly struct SparkAIPlayerResponse
    {
        public bool IsValid { get; }

        public float Time { get; }

        public SparkAIPlayerResponseType Type { get; }

        public string Text { get; }

        public string ConceptId { get; }

        public string Context { get; }

        public bool HasEvaluation { get; }

        public bool IsCorrect { get; }

        public float EvaluationConfidence { get; }

        public int PrimaryObjectInstanceId { get; }

        public int SecondaryObjectInstanceId { get; }

        public SparkAIPlayerResponse(
            bool isValid,
            float time,
            SparkAIPlayerResponseType type,
            string text,
            string conceptId,
            string context,
            bool hasEvaluation,
            bool isCorrect,
            float evaluationConfidence,
            int primaryObjectInstanceId,
            int secondaryObjectInstanceId)
        {
            IsValid = isValid;
            Time = time;
            Type = type;

            Text =
                text ??
                string.Empty;

            ConceptId =
                conceptId ??
                string.Empty;

            Context =
                context ??
                string.Empty;

            HasEvaluation = hasEvaluation;
            IsCorrect = isCorrect;

            EvaluationConfidence =
                Mathf.Clamp01(
                    evaluationConfidence);

            PrimaryObjectInstanceId =
                primaryObjectInstanceId;

            SecondaryObjectInstanceId =
                secondaryObjectInstanceId;
        }

        public static SparkAIPlayerResponse Invalid()
        {
            return new SparkAIPlayerResponse(
                false,
                UnityEngine.Time.time,
                SparkAIPlayerResponseType.None,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                false,
                0f,
                0,
                0);
        }
    }

    public enum SparkAIPlayerResponseType
    {
        None = 0,

        TextAnswer = 1,

        ActionPerformed = 2,

        ObjectSelected = 3,

        ComponentConnected = 4,

        ComponentDisconnected = 5,

        ToolUsed = 6,

        MeasurementTaken = 7,

        HintRequested = 8,

        ExplanationRequested = 9,

        AnswerEvaluated = 10
    }
}