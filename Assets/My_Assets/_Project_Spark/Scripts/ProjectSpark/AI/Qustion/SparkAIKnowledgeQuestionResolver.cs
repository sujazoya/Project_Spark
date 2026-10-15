
using System;

namespace ProjectSpark.AI
{
    public enum SparkAIKnowledgeQuestionType
    {
        Unknown = 0,
        Definition = 1,
        Explanation = 2,
        Example = 3,
        Misconception = 4,
        Safety = 5,
        Measurement = 6,

        // Appended to preserve existing serialized numeric values.
        Calculation = 7
    }

    public readonly struct SparkAIKnowledgeQuestionResult
    {
        public bool IsValid { get; }
        public SparkAIKnowledgeQuestionType Type { get; }
        public float Confidence { get; }

        public SparkAIKnowledgeQuestionResult(
            bool isValid,
            SparkAIKnowledgeQuestionType type,
            float confidence)
        {
            IsValid = isValid;
            Type = type;
            Confidence = confidence;
        }

        public static SparkAIKnowledgeQuestionResult Invalid()
        {
            return new SparkAIKnowledgeQuestionResult(
                false,
                SparkAIKnowledgeQuestionType.Unknown,
                0f);
        }
    }

    /// <summary>
    /// Classifies the educational task requested by the player.
    /// Does not access the circuit, calculate electrical values,
    /// or search the knowledge database.
    /// </summary>
    public static class SparkAIKnowledgeQuestionResolver
    {
        public static SparkAIKnowledgeQuestionResult Resolve(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return SparkAIKnowledgeQuestionResult.Invalid();

            string text = Normalize(question);

            if (text.Length == 0)
                return SparkAIKnowledgeQuestionResult.Invalid();

            if (ContainsAny(text,
                "is it safe", "is this safe", "is that safe",
                "is it dangerous", "is this dangerous",
                "is that dangerous", "is this connection dangerous",
                "danger", "dangerous", "safety",
                "safe to", "can this damage", "can it damage",
                "will this damage", "harmful"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Safety, 0.97f);
            }

            if (ContainsAny(text,
                "common mistake", "common mistakes",
                "common misconception", "common misconceptions",
                "misconception", "misconceptions",
                "wrong idea", "wrong ideas",
                "what am i getting wrong",
                "what am i doing wrong",
                "incorrect idea", "incorrect assumption"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Misconception, 0.96f);
            }

            // Calculation is its own category, not a kind of
            // instrument/measurement instruction.
            if (SparkAIQuestionIntentResolver
                .IsExplicitCalculationQuestion(text))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Calculation, 0.96f);
            }

            // Recognize instrument-use and measurement-instruction
            // questions. A bare mention of voltage/current does not
            // automatically make a question a measurement question.
            if (IsMeasurementInstruction(text))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Measurement, 0.95f);
            }

            if (ContainsAny(text,
                "give me an example", "give an example",
                "show me an example", "show an example",
                "example of", "examples of", "for example",
                "real world example", "real-world example",
                "practical example", "show a practical example"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Example, 0.94f);
            }

            if (ContainsAny(text,
                "how does", "how do", "how is", "how are",
                "how can", "how does it work",
                "how do they work", "how does this work",
                "how does that work", "how is it used",
                "how are they used", "explain", "explain how",
                "explain why", "tell me how", "tell me why",
                "why does", "why do", "why is", "why are",
                "why can", "what happens when",
                "what happens if", "what happens to"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Explanation, 0.93f);
            }

            if (ContainsAny(text,
                "what is", "what are", "what's", "define",
                "definition", "meaning of", "what does mean",
                "what do you mean by", "tell me about",
                "tell me what", "can you explain"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Definition, 0.90f);
            }

            // A fallback is useful for routing, but its confidence
            // is deliberately low because the wording was unknown.
            return Create(
                SparkAIKnowledgeQuestionType.Definition, 0.40f);
        }

       
        private static bool IsMeasurementInstruction(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Explicit measurement instructions take priority.
            if (ContainsAny(text,
                "how do i measure",
                "how do you measure",
                "how to measure",
                "how can i measure",
                "how can you measure",
                "how is measured",
                "how are measured",
                "how do i use a multimeter",
                "how to use a multimeter",
                "how do you use a multimeter",
                "how do i use the multimeter",
                "how to use the multimeter",
                "which meter measures",
                "what instrument measures",
                "what meter measures",
                "what measures voltage",
                "what measures current",
                "what measures resistance",
                "how to test continuity",
                "how do i test continuity",
                "how to check continuity",
                "how do i check continuity"))
            {
                return true;
            }

            // Do not classify diagnostic, state, or explanation
            // questions as instrument-use instructions.
            if (ContainsAny(text,
                "why ",
                "why does",
                "why do",
                "why is",
                "why are",
                "what is happening",
                "what is wrong",
                "what is the voltage",
                "what is the current",
                "what is the resistance",
                "what is the power",
                "is my ",
                "is the ",
                "is this ",
                "does my ",
                "does the ",
                "does this ",
                "right now",
                "currently",
                "at the moment"))
            {
                return false;
            }

            bool hasInstrument = ContainsAny(text,
                "multimeter",
                "voltmeter",
                "ammeter",
                "ohmmeter",
                "continuity tester");

            bool hasMeasurementAction = ContainsAny(text,
                "measure",
                "measured",
                "measuring",
                "measurement",
                "test",
                "testing",
                "check",
                "checking");

            bool hasElectricalTarget = ContainsAny(text,
                "voltage",
                "current",
                "resistance",
                "power",
                "led",
                "battery",
                "source",
                "resistor",
                "wire",
                "continuity");

            return hasMeasurementAction &&
                (hasInstrument || hasElectricalTarget);
        }


        private static SparkAIKnowledgeQuestionResult Create(
            SparkAIKnowledgeQuestionType type,
            float confidence)
        {
            return new SparkAIKnowledgeQuestionResult(
                true, type, confidence);
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
