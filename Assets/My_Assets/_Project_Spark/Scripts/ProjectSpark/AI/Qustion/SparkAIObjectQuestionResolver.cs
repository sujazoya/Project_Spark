using System;
using System.Collections.Generic;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Finds a likely Project Spark object name inside a player question.
    ///
    /// This class does not inspect or modify gameplay state.
    /// It only extracts a possible object reference from the question.
    /// </summary>
    public static class SparkAIObjectQuestionResolver
    {
        public static bool TryFindObject(
            string question,
            SparkAIWorldSnapshot world,
            out SparkAIElectronicObjectSnapshot result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(question))
                return false;

            if (world.ElectronicObjects == null ||
                world.ElectronicObjects.Count == 0)
            {
                return false;
            }

            string normalizedQuestion =
                Normalize(question);

            SparkAIElectronicObjectSnapshot bestMatch =
                default;

            int bestScore = 0;

            for (int i = 0;
                 i < world.ElectronicObjects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot candidate =
                    world.ElectronicObjects[i];

                if (string.IsNullOrWhiteSpace(candidate.Name))
                    continue;

                string objectName =
                    Normalize(candidate.Name);

                int score =
                    CalculateMatchScore(
                        normalizedQuestion,
                        objectName);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = candidate;
                }
            }

            if (bestScore <= 0)
                return false;

            result = bestMatch;
            return true;
        }

        private static int CalculateMatchScore(
            string question,
            string objectName)
        {
            if (string.IsNullOrWhiteSpace(question) ||
                string.IsNullOrWhiteSpace(objectName))
            {
                return 0;
            }

            /*
             * Exact object name.
             *
             * Example:
             * "why isn't led_1 working"
             */
            if (question.Contains(objectName))
                return 100;

            /*
             * Remove common separators so names such as:
             *
             * LED_1
             * LED-1
             * LED 1
             *
             * can still be recognized.
             */
            string compactQuestion =
                RemoveSeparators(question);

            string compactObject =
                RemoveSeparators(objectName);

            if (compactQuestion.Contains(compactObject))
                return 90;

            /*
             * Token-based fallback.
             */
            string[] objectTokens =
                objectName.Split(
                    new[] { ' ', '_', '-', '.', '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            int matchedTokens = 0;

            for (int i = 0; i < objectTokens.Length; i++)
            {
                string token =
                    objectTokens[i];

                if (token.Length < 2)
                    continue;

                if (question.Contains(token))
                    matchedTokens++;
            }

            if (matchedTokens == objectTokens.Length &&
                matchedTokens > 0)
            {
                return 70;
            }

            return 0;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToLowerInvariant();
        }

        private static string RemoveSeparators(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty)
                .Replace("/", string.Empty);
        }
    }
}