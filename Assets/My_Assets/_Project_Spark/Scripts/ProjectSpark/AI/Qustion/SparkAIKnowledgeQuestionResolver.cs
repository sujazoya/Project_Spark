using System;

namespace ProjectSpark.AI
{
    public enum SparkAIKnowledgeQuestionType
    {
        Unknown,
        Definition,
        Explanation,
        Example,
        Misconception,
        Safety,
        Measurement
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
    /// Determines the type of stable knowledge requested by the player.
    /// Does not search the knowledge database.
    /// </summary>
    public static class SparkAIKnowledgeQuestionResolver
    {

        private static bool IsMeasurementQuestion(string question)
{
    if (string.IsNullOrWhiteSpace(question))
        return false;

    string q = question.Trim().ToLowerInvariant();

    // Explicit measurement wording.
    if (q.Contains("measure") ||
        q.Contains("measured") ||
        q.Contains("measuring") ||
        q.Contains("measurement") ||
        q.Contains("multimeter") ||
        q.Contains("voltmeter") ||
        q.Contains("ammeter") ||
        q.Contains("ohmmeter") ||
        q.Contains("continuity"))
    {
        return true;
    }

    // Testing a particular component or electrical quantity.
    if ((q.Contains("test") ||
         q.Contains("check")) &&
        (q.Contains("led") ||
         q.Contains("source") ||
         q.Contains("battery") ||
         q.Contains("voltage") ||
         q.Contains("current") ||
         q.Contains("resistance") ||
         q.Contains("resistor") ||
         q.Contains("wire")))
    {
        return true;
    }

    return false;
}
        public static SparkAIKnowledgeQuestionResult Resolve(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return SparkAIKnowledgeQuestionResult.Invalid();

            string normalized = Normalize(question);

            if (string.IsNullOrEmpty(normalized))
                return SparkAIKnowledgeQuestionResult.Invalid();

                

            // ========================================================
            // SAFETY
            // ========================================================

            if (ContainsAny(
                    normalized,
                    "is it safe",
                    "is this safe",
                    "is that safe",
                    "is it dangerous",
                    "is this dangerous",
                    "is that dangerous",
                    "danger",
                    "dangerous",
                    "safety",
                    "safe to",
                    "can this damage",
                    "can it damage",
                    "will this damage",
                    "harmful"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Safety,
                    0.98f);
            }

            // ========================================================
            // MISCONCEPTION / COMMON MISTAKE
            // ========================================================

            if (ContainsAny(
                    normalized,
                    "common mistake",
                    "common mistakes",
                    "common misconception",
                    "common misconceptions",
                    "misconception",
                    "misconceptions",
                    "wrong idea",
                    "wrong ideas",
                    "mistake",
                    "mistakes",
                    "what am i getting wrong",
                    "what am i doing wrong",
                    "incorrect idea",
                    "incorrect assumption"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Misconception,
                    0.97f);
            }

            // ========================================================
// CALCULATION
// Check before general explanation phrases.
// ========================================================

if (ContainsAny(
        normalized,
        "calculate current",
        "calculate voltage",
        "calculate resistance",
        "calculate power",
        "calculate using ohm's law",
        "calculate using ohms law",
        "calculate current from",
        "calculate voltage from",
        "calculate resistance from",
        "work out current",
        "work out voltage",
        "work out resistance",
        "solve ohm's law",
        "ohm's law calculation",
        "ohms law calculation"))
{
    return Create(
        SparkAIKnowledgeQuestionType.Measurement,
        0.98f);
}

           // ========================================================
            // MEASUREMENT
            // Check before general explanation phrases such as
            // "how is" and "how do".
            // ========================================================

            if (ContainsAny(
                    normalized,

                    // General measurement wording
                    "measure",
                    "measured",
                    "measuring",
                    "measurement",
                    "multimeter",
                    "voltmeter",
                    "ammeter",
                    "ohmmeter",
                    "continuity",

                    // Testing or checking electrical quantities/components
                    "test led",
                    "check led",
                    "test source",
                    "check source",
                    "test battery",
                    "check battery",
                    "test resistor",
                    "check resistor",
                    "test wire",
                    "check wire",

                    // Existing current measurement phrases
                    "how is current measured",
                    "how do i measure current",
                    "how to measure current",
                    "how can i measure current",
                    "how can current be measured",
                    "how is electric current measured",
                    "how do you measure current",

                    // Existing voltage measurement phrases
                    "how is voltage measured",
                    "how do i measure voltage",
                    "how to measure voltage",
                    "how can i measure voltage",
                    "how can voltage be measured",
                    "how do you measure voltage",

                    // Existing resistance measurement phrases
                    "how is resistance measured",
                    "how do i measure resistance",
                    "how to measure resistance",
                    "how do you measure resistance",

                    // Existing power measurement phrases
                    "how is power measured",
                    "how do i measure power",
                    "how to measure power",
                    "how do you measure power",

                    // Instrument questions
                    "what instrument measures current",
                    "what instrument measures voltage",
                    "what instrument measures resistance",
                    "what measures current",
                    "what measures voltage",
                    "what measures resistance",
                    "which meter measures current",
                    "which meter measures voltage",
                    "which meter measures resistance",
                    "how do i use a multimeter",
                    "how to use a multimeter",
                    "how do you use a multimeter"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Measurement,
                    0.98f);
            }

            // ========================================================
            // EXAMPLE
            // ========================================================

            if (ContainsAny(
                    normalized,
                    "give me an example",
                    "give an example",
                    "show me an example",
                    "show an example",
                    "example of",
                    "examples of",
                    "for example",
                    "real world example",
                    "real-world example",
                    "practical example",
                    "show a practical example"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Example,
                    0.96f);
            }

            // ========================================================
            // EXPLANATION
            // ========================================================

            if (ContainsAny(
                    normalized,
                    "how does",
                    "how do",
                    "how is",
                    "how are",
                    "how can",
                    "how does it work",
                    "how do they work",
                    "how does this work",
                    "how does that work",
                    "how is it used",
                    "how are they used",
                    "explain",
                    "explain how",
                    "explain why",
                    "tell me how",
                    "tell me why",
                    "why does",
                    "why do",
                    "why is",
                    "why are",
                    "why can",
                    "why does it",
                    "what happens when",
                    "what happens if",
                    "what happens to"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Explanation,
                    0.95f);
            }

            // ========================================================
            // DEFINITION
            // ========================================================

            if (ContainsAny(
                    normalized,
                    "what is",
                    "what are",
                    "what's",
                    "define",
                    "definition",
                    "meaning of",
                    "what does mean",
                    "what do you mean by",
                    "tell me about",
                    "tell me what",
                    "can you explain"))
            {
                return Create(
                    SparkAIKnowledgeQuestionType.Definition,
                    0.92f);
            }

            // ========================================================
            // NATURAL FALLBACK
            // ========================================================

            return Create(
                SparkAIKnowledgeQuestionType.Definition,
                0.50f);
        }

        

        private static SparkAIKnowledgeQuestionResult Create(
            SparkAIKnowledgeQuestionType type,
            float confidence)
        {
            return new SparkAIKnowledgeQuestionResult(
                true,
                type,
                confidence);
        }

        private static bool ContainsAny(
            string text,
            params string[] phrases)
        {
            if (string.IsNullOrEmpty(text) || phrases == null)
                return false;

            for (int i = 0; i < phrases.Length; i++)
            {
                string phrase = phrases[i];

                if (string.IsNullOrWhiteSpace(phrase))
                    continue;

                if (text.IndexOf(
                        phrase,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim().ToLowerInvariant()
                .Replace('\u2018', '\'')
                .Replace('\u2019', '\'')
                .Replace('\u201C', '"')
                .Replace('\u201D', '"');
        }
    }
}