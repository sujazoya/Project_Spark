using System;

namespace ProjectSpark.AI
{
    public enum SparkAIQuestionIntent
    {
        Unknown = 0,
        WhyNotWorking = 1,
        FindProblem = 2,
        Voltage = 3,
        Current = 4,
        Power = 5,
        Source = 6,
        Polarity = 7,
        CircuitPath = 8,
        Switch = 9,
        Explanation = 10,
        NextStep = 11
    }

    /// <summary>
    /// Represents what the player is trying to learn from a question.
    /// </summary>
    public readonly struct SparkAIQuestionIntentResult
    {
        public bool IsValid { get; }
        public SparkAIQuestionIntent Intent { get; }
        public float Confidence { get; }

        public SparkAIQuestionIntentResult(
            bool isValid,
            SparkAIQuestionIntent intent,
            float confidence)
        {
            IsValid = isValid;
            Intent = intent;
            Confidence = confidence;
        }

        public static SparkAIQuestionIntentResult Invalid()
        {
            return new SparkAIQuestionIntentResult(
                false,
                SparkAIQuestionIntent.Unknown,
                0f);
        }
    }

    public static class SparkAIQuestionIntentResolver
    {
        public static SparkAIQuestionIntentResult Resolve(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return SparkAIQuestionIntentResult.Invalid();
            }

            string text =
                question.Trim().ToLowerInvariant();

            // Highest priority: troubleshooting.
            if (ContainsAny(
                    text,
                    "why isn't",
                    "why isnt",
                    "why is not",
                    "why doesn't",
                    "why doesnt",
                    "not working",
                    "isn't working",
                    "isnt working",
                    "won't work",
                    "wont work",
                    "why no current",
                    "why no voltage"))
            {
                return Create(
                    SparkAIQuestionIntent.WhyNotWorking,
                    0.95f);
            }

            if (ContainsAny(
                    text,
                    "what is wrong",
                    "whats wrong",
                    "where is the problem",
                    "where is wrong",
                    "find the problem",
                    "find problem",
                    "what should i fix",
                    "what do i fix"))
            {
                return Create(
                    SparkAIQuestionIntent.FindProblem,
                    0.95f);
            }

            if (ContainsAny(
                    text,
                    "what should i do",
                    "what do i do",
                    "what next",
                    "next step",
                    "how do i continue",
                    "what should i connect"))
            {
                return Create(
                    SparkAIQuestionIntent.NextStep,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "voltage",
                    "volt",
                    "volts"))
            {
                return Create(
                    SparkAIQuestionIntent.Voltage,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "current",
                    "amp",
                    "amps",
                    "ampere",
                    "amperes"))
            {
                return Create(
                    SparkAIQuestionIntent.Current,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "power",
                    "watt",
                    "watts"))
            {
                return Create(
                    SparkAIQuestionIntent.Power,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "source",
                    "battery",
                    "power supply"))
            {
                return Create(
                    SparkAIQuestionIntent.Source,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "polarity",
                    "positive",
                    "negative",
                    "anode",
                    "cathode"))
            {
                return Create(
                    SparkAIQuestionIntent.Polarity,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "path",
                    "complete circuit",
                    "closed circuit",
                    "open circuit",
                    "circuit complete"))
            {
                return Create(
                    SparkAIQuestionIntent.CircuitPath,
                    0.90f);
            }

            if (ContainsAny(
                    text,
                    "switch",
                    "open the switch",
                    "close the switch",
                    "turn on",
                    "turn off"))
            {
                return Create(
                    SparkAIQuestionIntent.Switch,
                    0.90f);
            }

            return Create(
                SparkAIQuestionIntent.Explanation,
                0.50f);
        }

        private static SparkAIQuestionIntentResult Create(
            SparkAIQuestionIntent intent,
            float confidence)
        {
            return new SparkAIQuestionIntentResult(
                true,
                intent,
                confidence);
        }

        private static bool ContainsAny(
            string text,
            params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (text.Contains(values[i]))
                    return true;
            }

            return false;
        }
    }
}