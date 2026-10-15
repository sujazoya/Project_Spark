
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
        NextStep = 11,

        // Appended to preserve existing serialized numeric values.
        Calculation = 12
    }

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
                return SparkAIQuestionIntentResult.Invalid();

            string text = Normalize(question);

            if (text.Length == 0)
                return SparkAIQuestionIntentResult.Invalid();

            // Troubleshooting has priority over a quantity mentioned
            // in the same question.
            if (ContainsAny(text,
                "why isn't", "why isnt", "why is not",
                "why doesn't", "why doesnt",
                "not working", "isn't working", "isnt working",
                "won't work", "wont work",
                "not turning on", "not lighting up",
                "not glowing", "not responding",
                "why no current", "why no voltage",
                "why is there no current",
                "why is there no voltage",
                "why is my led off", "why is my led dark",
                "why is the led off", "why is the led dark",
                "why is my bulb off", "why is my bulb dark"))
            {
                return Create(
                    SparkAIQuestionIntent.WhyNotWorking, 0.96f);
            }

            if (ContainsAny(text,
                "what is wrong", "what's wrong", "whats wrong",
                "where is the problem", "where is wrong",
                "find the problem", "find problem",
                "identify the fault", "find the fault",
                "what should i fix", "what do i fix",
                "what is the fault"))
            {
                return Create(
                    SparkAIQuestionIntent.FindProblem, 0.96f);
            }

            if (IsExplicitCalculationQuestion(text))
            {
                return Create(
                    SparkAIQuestionIntent.Calculation, 0.96f);
            }

            if (ContainsAny(text,
                "what should i do", "what do i do",
                "what next", "next step",
                "how do i continue", "what should i connect",
                "what do i connect next",
                "how can i complete this circuit"))
            {
                return Create(
                    SparkAIQuestionIntent.NextStep, 0.92f);
            }

            // Identify the main electrical subject only after
            // higher-priority tasks have been checked.
            if (ContainsAny(text,
                "voltage", "volt", "volts", "potential difference"))
            {
                return Create(
                    SparkAIQuestionIntent.Voltage, 0.90f);
            }

            if (ContainsAny(text,
                "current", "amp", "amps", "ampere", "amperes",
                "amperage"))
            {
                return Create(
                    SparkAIQuestionIntent.Current, 0.90f);
            }

            if (ContainsAny(text, "power", "watt", "watts"))
            {
                return Create(
                    SparkAIQuestionIntent.Power, 0.90f);
            }

            if (ContainsAny(text,
                "power supply", "battery", "voltage source",
                "electrical source", "source"))
            {
                return Create(
                    SparkAIQuestionIntent.Source, 0.88f);
            }

            if (ContainsAny(text,
                "polarity", "positive terminal", "negative terminal",
                "positive wire", "negative wire",
                "anode", "cathode"))
            {
                return Create(
                    SparkAIQuestionIntent.Polarity, 0.92f);
            }

            if (ContainsAny(text,
                "circuit path", "complete circuit",
                "closed circuit", "open circuit",
                "circuit complete", "current path",
                "path for current", "return path"))
            {
                return Create(
                    SparkAIQuestionIntent.CircuitPath, 0.90f);
            }

            if (ContainsAny(text,
                "switch", "open the switch", "close the switch",
                "turn on", "turn off", "turn it on", "turn it off"))
            {
                return Create(
                    SparkAIQuestionIntent.Switch, 0.88f);
            }

            // Do not claim a confident specific intent when no
            // supported subject or task was identified.
            return Create(
                SparkAIQuestionIntent.Explanation, 0.50f);
        }

        public static bool IsExplicitCalculationQuestion(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return false;

            string text = Normalize(question);

            return ContainsAny(text,
                "calculate current", "calculate the current",
                "calculate voltage", "calculate the voltage",
                "calculate resistance", "calculate the resistance",
                "calculate power", "calculate the power",
                "compute current", "compute voltage",
                "compute resistance", "compute power",
                "work out current", "work out the current",
                "work out voltage", "work out the voltage",
                "work out resistance", "work out the resistance",
                "find the current using",
                "find the voltage using",
                "find the resistance using",
                "find the power using",
                "solve using ohm's law",
                "solve using ohms law",
                "calculate using ohm's law",
                "calculate using ohms law",
                "ohm's law calculation",
                "ohms law calculation");
        }

        private static SparkAIQuestionIntentResult Create(
            SparkAIQuestionIntent intent,
            float confidence)
        {
            return new SparkAIQuestionIntentResult(
                true, intent, confidence);
        }

        private static string Normalize(string value)
        {
            return value.Trim()
                .ToLowerInvariant()
                .Replace('\u2018', '\'')
                .Replace('\u2019', '\'')
                .Replace('\u201C', '"')
                .Replace('\u201D', '"')
                .Replace('\u2013', '-')
                .Replace('\u2014', '-');
        }

        private static bool ContainsAny(
            string text,
            params string[] phrases)
        {
            for (int i = 0; i < phrases.Length; i++)
            {
                if (ContainsPhrase(text, phrases[i]))
                    return true;
            }

            return false;
        }

        // Matches whole words/phrases rather than arbitrary
        // substrings. For example, "amp" does not match "example".
        private static bool ContainsPhrase(
            string text,
            string phrase)
        {
            if (string.IsNullOrEmpty(text) ||
                string.IsNullOrEmpty(phrase))
                return false;

            int start = 0;

            while (start < text.Length)
            {
                int index = text.IndexOf(
                    phrase, start, StringComparison.Ordinal);

                if (index < 0)
                    return false;

                int end = index + phrase.Length;

                bool leftBoundary =
                    index == 0 ||
                    !IsWordCharacter(text[index - 1]);

                bool rightBoundary =
                    end >= text.Length ||
                    !IsWordCharacter(text[end]);

                if (leftBoundary && rightBoundary)
                    return true;

                start = index + 1;
            }

            return false;
        }

        private static bool IsWordCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}
