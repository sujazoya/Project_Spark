using System;
using System.Collections.Generic;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Resolves a player question to an electronic object in the
    /// current Project Spark world snapshot.
    ///
    /// Supports:
    /// - Exact scene-object names.
    /// - Names containing separators such as LED_1 or Resistor-1.
    /// - Generic component names such as LED, resistor, and switch.
    /// - Common aliases such as battery / power supply.
    ///
    /// This class does not inspect or modify gameplay state.
    /// </summary>
    public static class SparkAIObjectQuestionResolver
    {
        private const int ExactNameScore = 100;
        private const int CompactNameScore = 90;
        private const int TokenMatchScore = 70;
        private const int ComponentAliasScore = 40;

        private static readonly string[][] ComponentAliases =
        {
            new[] { "led", "light emitting diode" },
            new[] { "resistor", "resistance component" },
            new[] { "switch" },
            new[] { "battery", "cell" },
            new[] { "power supply", "powersupply", "supply", "source" },
            new[] { "motor", "dc motor" },
            new[] { "lamp", "bulb", "light bulb" },
            new[] { "diode" },
            new[] { "capacitor", "condenser" },
            new[] { "inductor", "coil" },
            new[] { "transistor" },
            new[] { "fuse" },
            new[] { "wire", "cable", "lead" },
            new[] { "multimeter", "voltmeter", "ammeter" },
            new[] { "potentiometer", "variable resistor" },
            new[] { "transformer" },
            new[] { "relay" }
        };

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

            string normalizedQuestion = Normalize(question);

            if (normalizedQuestion.Length == 0)
                return false;

            SparkAIElectronicObjectSnapshot bestMatch = default;

            int bestScore = 0;
            int bestMatchCount = 0;

            for (int i = 0;
                 i < world.ElectronicObjects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot candidate =
                    world.ElectronicObjects[i];

                if (string.IsNullOrWhiteSpace(candidate.Name))
                    continue;

                string objectName = Normalize(candidate.Name);

                if (objectName.Length == 0)
                    continue;

                int score = CalculateMatchScore(
                    normalizedQuestion,
                    objectName);

                if (score <= 0)
                    continue;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = candidate;
                    bestMatchCount = 1;
                }
                else if (score == bestScore)
                {
                    bestMatchCount++;
                }
            }

            /*
             * A generic name such as "the LED" must not arbitrarily
             * select one of multiple equally matching LEDs.
             *
             * Exact and compact full-name matches are preferred.
             * Generic ambiguous matches return false so another
             * component is never silently selected.
             */
            if (bestScore <= 0)
                return false;

            if (bestMatchCount > 1 &&
                bestScore < CompactNameScore)
            {
                return false;
            }

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

            // 1. Exact normalized object name.
            // Example: "is led 1 working" matches "LED 1".
            if (ContainsPhrase(question, objectName))
                return ExactNameScore;

            // 2. Ignore common separators.
            // LED_1, LED-1 and LED 1 become LED1.
            string compactQuestion = RemoveSeparators(question);
            string compactObject = RemoveSeparators(objectName);

            if (compactObject.Length >= 2 &&
                compactQuestion.Contains(compactObject))
            {
                return CompactNameScore;
            }

            // 3. Match all meaningful words in a multi-word object name.
            string[] objectTokens = objectName.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            int meaningfulTokens = 0;
            int matchedTokens = 0;

            for (int i = 0; i < objectTokens.Length; i++)
            {
                string token = objectTokens[i];

                // Ignore short numeric suffixes such as "1".
                if (token.Length < 2)
                    continue;

                meaningfulTokens++;

                if (ContainsPhrase(question, token))
                    matchedTokens++;
            }

            if (meaningfulTokens > 0 &&
                matchedTokens == meaningfulTokens)
            {
                return TokenMatchScore;
            }

            // 4. Generic component aliases.
            // Example: "the LED" matches an object named "LED_1".
            // It will not automatically choose between multiple LEDs.
            for (int i = 0; i < ComponentAliases.Length; i++)
            {
                string[] aliases = ComponentAliases[i];

                bool questionMentionsAlias = false;

                for (int j = 0; j < aliases.Length; j++)
                {
                    string alias = Normalize(aliases[j]);

                    if (ContainsPhrase(question, alias))
                    {
                        questionMentionsAlias = true;
                        break;
                    }
                }

                if (!questionMentionsAlias)
                    continue;

                for (int j = 0; j < aliases.Length; j++)
                {
                    string alias = Normalize(aliases[j]);

                    if (ContainsPhrase(objectName, alias))
                        return ComponentAliasScore;
                }
            }

            return 0;
        }

        private static bool ContainsPhrase(
            string text,
            string phrase)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(phrase))
            {
                return false;
            }

            string normalizedText = " " + Normalize(text) + " ";
            string normalizedPhrase = " " + Normalize(phrase) + " ";

            return normalizedText.Contains(normalizedPhrase);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToLowerInvariant()
                .Replace("_", " ")
                .Replace("-", " ")
                .Replace(".", " ")
                .Replace("/", " ")
                .Replace("\\", " ")
                .Replace("(", " ")
                .Replace(")", " ")
                .Replace("[", " ")
                .Replace("]", " ")
                .Replace(",", " ")
                .Replace(":", " ")
                .Replace(";", " ")
                .Replace("?", " ")
                .Replace("!", " ")
                .Replace("'", " ")
                .Replace("\"", " ");
        }

        private static string RemoveSeparators(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(".", string.Empty)
                .Replace("/", string.Empty)
                .Replace("\\", string.Empty);
        }
    }
}