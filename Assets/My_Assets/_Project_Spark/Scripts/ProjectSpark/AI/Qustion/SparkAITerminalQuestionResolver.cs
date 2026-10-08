using System;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Resolves a player question to the correct Project Spark terminal.
    ///
    /// Resolution:
    /// 1. Find the explicitly mentioned electronic object.
    /// 2. Resolve its InstanceId.
    /// 3. Find terminals belonging to that object using OwnerInstanceId.
    /// 4. Match the requested terminal/polarity.
    ///
    /// This avoids relying on OwnerName because the visible object
    /// name and terminal owner name may be different.
    /// </summary>
    public static class SparkAITerminalQuestionResolver
    {
        public static bool TryFindTerminal(
            string question,
            SparkAIWorldSnapshot world,
            out SparkAITerminalSnapshot terminal)
        {
            terminal = default;

            if (string.IsNullOrWhiteSpace(question))
                return false;

            if (world.Terminals == null ||
                world.Terminals.Count == 0)
            {
                return false;
            }

            string normalizedQuestion =
                Normalize(question);

            /*
             * =====================================================
             * STEP 1
             * Find the object explicitly mentioned in the question.
             * =====================================================
             */
            SparkAIElectronicObjectSnapshot objectTarget;

            if (TryFindObject(
                    normalizedQuestion,
                    world,
                    out objectTarget))
            {
                /*
                 * =================================================
                 * STEP 2
                 * Search ONLY terminals belonging to this object.
                 * =================================================
                 */
                int bestScore = 0;

                SparkAITerminalSnapshot bestTerminal =
                    default;

                for (int i = 0;
                     i < world.Terminals.Count;
                     i++)
                {
                    SparkAITerminalSnapshot candidate =
                        world.Terminals[i];

                    if (candidate.OwnerInstanceId !=
                        objectTarget.InstanceId)
                    {
                        continue;
                    }

                    int score =
                        CalculateTerminalScore(
                            normalizedQuestion,
                            candidate);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTerminal = candidate;
                    }
                }

                if (bestScore > 0)
                {
                    terminal = bestTerminal;
                    return true;
                }
            }

            /*
             * =====================================================
             * FALLBACK
             *
             * Only used when the question does not identify a
             * specific electronic object.
             * =====================================================
             */
            return TryFindGenericTerminal(
                normalizedQuestion,
                world,
                out terminal);
        }

        private static bool TryFindObject(
            string question,
            SparkAIWorldSnapshot world,
            out SparkAIElectronicObjectSnapshot result)
        {
            result = default;

            if (world.ElectronicObjects == null ||
                world.ElectronicObjects.Count == 0)
            {
                return false;
            }

            int bestScore = 0;

            SparkAIElectronicObjectSnapshot best =
                default;

            for (int i = 0;
                 i < world.ElectronicObjects.Count;
                 i++)
            {
                SparkAIElectronicObjectSnapshot candidate =
                    world.ElectronicObjects[i];

                string name =
                    Normalize(candidate.Name);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                /*
                 * Exact object name.
                 *
                 * Example:
                 *
                 * "What is connected to LED_1 anode?"
                 *
                 * If ElectronicObject.Name == "LED_1",
                 * this gets the strongest score.
                 */
                if (question.Contains(name))
                {
                    int score =
                        1000 + name.Length;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }
            }

            if (bestScore <= 0)
                return false;

            result = best;
            return true;
        }

        private static int CalculateTerminalScore(
            string question,
            SparkAITerminalSnapshot terminal)
        {
            int score = 100;

            string terminalName =
                Normalize(terminal.Name);

            string polarity =
                Normalize(terminal.Polarity);

            string effectivePolarity =
                Normalize(terminal.EffectivePolarity);

            /*
             * Exact terminal name.
             */
            if (!string.IsNullOrWhiteSpace(terminalName) &&
                question.Contains(terminalName))
            {
                score += 300;
            }

            /*
             * Explicit polarity.
             */
            if (!string.IsNullOrWhiteSpace(polarity) &&
                question.Contains(polarity))
            {
                score += 250;
            }

            if (!string.IsNullOrWhiteSpace(effectivePolarity) &&
                question.Contains(effectivePolarity))
            {
                score += 200;
            }

            /*
             * Semantic terminal matching.
             *
             * Some terminals have:
             *
             * Name = Anode
             * Polarity = Positive
             *
             * while others may only have the terminal name.
             */
            if (question.Contains("anode"))
            {
                if (IsAnodeTerminal(terminal))
                    score += 500;
                else
                    return 0;
            }

            if (question.Contains("cathode"))
            {
                if (IsCathodeTerminal(terminal))
                    score += 500;
                else
                    return 0;
            }

            if (question.Contains("positive"))
            {
                if (IsPositiveTerminal(terminal))
                    score += 500;
                else
                    return 0;
            }

            if (question.Contains("negative"))
            {
                if (IsNegativeTerminal(terminal))
                    score += 500;
                else
                    return 0;
            }

            return score;
        }

        private static bool IsAnodeTerminal(
            SparkAITerminalSnapshot terminal)
        {
            string name =
                Normalize(terminal.Name);

            string polarity =
                Normalize(terminal.Polarity);

            string effective =
                Normalize(terminal.EffectivePolarity);

            return name.Contains("anode") ||
                   polarity.Contains("anode") ||
                   effective.Contains("anode");
        }

        private static bool IsCathodeTerminal(
            SparkAITerminalSnapshot terminal)
        {
            string name =
                Normalize(terminal.Name);

            string polarity =
                Normalize(terminal.Polarity);

            string effective =
                Normalize(terminal.EffectivePolarity);

            return name.Contains("cathode") ||
                   polarity.Contains("cathode") ||
                   effective.Contains("cathode");
        }

        private static bool IsPositiveTerminal(
            SparkAITerminalSnapshot terminal)
        {
            string name =
                Normalize(terminal.Name);

            string polarity =
                Normalize(terminal.Polarity);

            string effective =
                Normalize(terminal.EffectivePolarity);

            return name.Contains("positive") ||
                   name == "+" ||
                   polarity.Contains("positive") ||
                   polarity == "+" ||
                   effective.Contains("positive") ||
                   effective == "+";
        }

        private static bool IsNegativeTerminal(
            SparkAITerminalSnapshot terminal)
        {
            string name =
                Normalize(terminal.Name);

            string polarity =
                Normalize(terminal.Polarity);

            string effective =
                Normalize(terminal.EffectivePolarity);

            return name.Contains("negative") ||
                   name == "-" ||
                   polarity.Contains("negative") ||
                   polarity == "-" ||
                   effective.Contains("negative") ||
                   effective == "-";
        }

        private static bool TryFindGenericTerminal(
            string question,
            SparkAIWorldSnapshot world,
            out SparkAITerminalSnapshot terminal)
        {
            terminal = default;

            int bestScore = 0;

            SparkAITerminalSnapshot best =
                default;

            for (int i = 0;
                 i < world.Terminals.Count;
                 i++)
            {
                SparkAITerminalSnapshot candidate =
                    world.Terminals[i];

                int score = 0;

                if (question.Contains("anode") &&
                    IsAnodeTerminal(candidate))
                {
                    score += 300;
                }

                if (question.Contains("cathode") &&
                    IsCathodeTerminal(candidate))
                {
                    score += 300;
                }

                if (question.Contains("positive") &&
                    IsPositiveTerminal(candidate))
                {
                    score += 300;
                }

                if (question.Contains("negative") &&
                    IsNegativeTerminal(candidate))
                {
                    score += 300;
                }

                string name =
                    Normalize(candidate.Name);

                if (!string.IsNullOrWhiteSpace(name) &&
                    question.Contains(name))
                {
                    score += 100;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            if (bestScore <= 0)
                return false;

            terminal = best;
            return true;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToLowerInvariant();
        }
    }
}