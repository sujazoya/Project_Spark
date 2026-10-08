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
        Safety
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
    /// Determines what kind of stable knowledge the player is asking for.
    ///
    /// This class does NOT search the knowledge database.
    /// It only determines the requested answer type.
    /// </summary>
    public static class SparkAIKnowledgeQuestionResolver
    {
        public static SparkAIKnowledgeQuestionResult Resolve(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return SparkAIKnowledgeQuestionResult.Invalid();

            string normalized =
                Normalize(question);

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
                    "is this dangerous",
                    "danger",
                    "dangerous",
                    "safety",
                    "safe to",
                    "can this damage",
                    "can it damage",
                    "will this damage",
                    "harmful"))
            {
                return new SparkAIKnowledgeQuestionResult(
                    true,
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
                return new SparkAIKnowledgeQuestionResult(
                    true,
                    SparkAIKnowledgeQuestionType.Misconception,
                    0.97f);
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
                return new SparkAIKnowledgeQuestionResult(
                    true,
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
                return new SparkAIKnowledgeQuestionResult(
                    true,
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
                return new SparkAIKnowledgeQuestionResult(
                    true,
                    SparkAIKnowledgeQuestionType.Definition,
                    0.92f);
            }

            // ========================================================
            // NATURAL FALLBACK
            // ========================================================

            return new SparkAIKnowledgeQuestionResult(
                true,
                SparkAIKnowledgeQuestionType.Definition,
                0.50f);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static bool ContainsAny(
            string text,
            params string[] phrases)
        {
            if (string.IsNullOrEmpty(text) ||
                phrases == null)
            {
                return false;
            }

            for (int i = 0; i < phrases.Length; i++)
            {
                string phrase =
                    phrases[i];

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

        private static string Normalize(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToLowerInvariant();
        }
    }
}