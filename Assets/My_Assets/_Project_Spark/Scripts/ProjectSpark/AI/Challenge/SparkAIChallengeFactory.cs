using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts persistent challenge definitions into runtime
    /// SparkAILessonChallenge objects.
    ///
    /// Persistent data:
    ///     SparkAIChallengeDefinition
    ///
    /// Runtime challenge:
    ///     SparkAILessonChallenge
    ///
    /// This class does not:
    /// - evaluate the circuit
    /// - start a challenge
    /// - modify gameplay
    /// - modify learner progress
    /// </summary>
    public static class SparkAIChallengeFactory
    {
        /// <summary>
        /// Creates a runtime challenge from a stored definition.
        /// </summary>
        public static SparkAILessonChallenge Create(
            SparkAIChallengeDefinition definition)
        {
            if (!definition.IsValid)
                return SparkAILessonChallenge.Invalid();

            return new SparkAILessonChallenge(
                true,
                definition.Id,
                definition.ConceptId,
                definition.Topic,
                definition.Difficulty,
                definition.Title,
                definition.Objective,
                definition.Instruction,
                definition.SuccessDescription,
                definition.Hint,
                definition.StrongHint,
                definition.RequiresCircuitChange,
                definition.RequiresMeasurement,
                definition.RequiresPlayerAnswer,
                Time.time);
        }

        /// <summary>
        /// Creates a runtime challenge and validates the definition
        /// before conversion.
        /// </summary>
        public static bool TryCreate(
            SparkAIChallengeDefinition definition,
            out SparkAILessonChallenge challenge)
        {
            challenge = SparkAILessonChallenge.Invalid();

            if (!definition.IsValid)
                return false;

            challenge = Create(definition);

            return challenge.IsValid;
        }
    }
}