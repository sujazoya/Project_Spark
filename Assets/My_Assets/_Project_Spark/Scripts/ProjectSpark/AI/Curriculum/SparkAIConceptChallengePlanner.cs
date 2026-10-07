using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects concept prerequisite progression with challenge generation.
    ///
    /// Responsibilities:
    /// - Checks whether the requested concept is ready.
    /// - Redirects to the deepest missing prerequisite when necessary.
    /// - Keeps challenge generation descriptive only.
    /// - Does not modify or evaluate the electrical circuit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIConceptChallengePlanner : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIConceptProgressionPlanner conceptProgressionPlanner;

        [SerializeField]
        private SparkAIChallengeGenerator challengeGenerator;

        [Header("Behaviour")]
        [SerializeField]
        private bool respectConceptPrerequisites = true;

        private bool initialized;

        public bool IsInitialized => initialized;

        public void Initialize()
        {
            if (initialized)
                return;

            if (conceptProgressionPlanner != null)
                conceptProgressionPlanner.Initialize();

            initialized = true;
        }

        public SparkAILessonChallenge GenerateForConcept(
            string requestedConceptId)
        {
            EnsureInitialized();

            if (challengeGenerator == null)
                return SparkAILessonChallenge.Invalid();

            if (string.IsNullOrWhiteSpace(requestedConceptId))
                return SparkAILessonChallenge.Invalid();

            string teachingConceptId =
                requestedConceptId;

            if (respectConceptPrerequisites &&
                conceptProgressionPlanner != null)
            {
                if (conceptProgressionPlanner.TryGetNextConcept(
                        requestedConceptId,
                        out SparkAIConceptDefinition nextConcept))
                {
                    if (nextConcept.IsValid)
                    {
                        teachingConceptId =
                            nextConcept.Id;
                    }
                }
            }

            /*
             * The existing challenge generator already knows how
             * to construct challenges for teaching topics.
             *
             * We therefore let it create the normal adaptive
             * challenge rather than duplicating challenge logic here.
             */
            SparkAIAdaptiveTeachingPlan plan =
                BuildConceptPlan(teachingConceptId);

            if (!plan.IsValid)
                return SparkAILessonChallenge.Invalid();

            return challengeGenerator.GenerateChallenge(plan);
        }

        private SparkAIAdaptiveTeachingPlan BuildConceptPlan(
            string conceptId)
        {
            SparkAITeachingTopic topic =
                ConvertConceptToTopic(conceptId);

            return new SparkAIAdaptiveTeachingPlan(
                true,
                Time.time,
                topic,
                conceptId,
                1f,
                SparkAITeachingIntensity.Gentle,
                "Concept progression requires this concept.",
                BuildObjective(conceptId),
                BuildQuestion(topic, conceptId),
                BuildHint(topic, conceptId),
                BuildExplanation(topic, conceptId));
        }

        private SparkAITeachingTopic ConvertConceptToTopic(
            string conceptId)
        {
            if (string.IsNullOrWhiteSpace(conceptId))
                return SparkAITeachingTopic.CircuitBasics;

            switch (conceptId.Trim().ToLowerInvariant())
            {
                case "source":
                    return SparkAITeachingTopic.Source;

                case "voltage":
                case "multimeter_voltage":
                    return SparkAITeachingTopic.Voltage;

                case "current":
                case "multimeter_current":
                    return SparkAITeachingTopic.Current;

                case "resistance":
                case "resistor":
                case "ohms_law":
                    return SparkAITeachingTopic.Resistance;

                case "power":
                    return SparkAITeachingTopic.Power;

                case "short_circuit":
                    return SparkAITeachingTopic.ShortCircuit;

                case "switch":
                    return SparkAITeachingTopic.Switch;

                case "polarity":
                    return SparkAITeachingTopic.Polarity;

                case "open_circuit":
                case "closed_circuit":
                case "series_circuit":
                case "parallel_circuit":
                case "circuit_basics":
                case "circuit_path":
                    return SparkAITeachingTopic.CircuitBasics;

                case "measurement":
                case "multimeter":
                    return SparkAITeachingTopic.Measurement;

                default:
                    return SparkAITeachingTopic.CircuitBasics;
            }
        }

        private string BuildObjective(string conceptId)
        {
            switch (conceptId)
            {
                case "source":
                    return "Understand where electrical energy comes from.";

                case "voltage":
                    return "Understand voltage as an electrical potential difference.";

                case "current":
                    return "Understand how electrical current flows through a complete path.";

                case "resistance":
                    return "Understand how resistance affects current.";

                case "power":
                    return "Understand how voltage and current determine electrical power.";

                case "polarity":
                    return "Understand why electrical polarity matters when connecting components.";

                case "short_circuit":
                    return "Understand what happens when current gets an unintended low-resistance path.";

                default:
                    return "Understand this electrical concept through practical circuit interaction.";
            }
        }

        private string BuildQuestion(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            return $"What do you think '{conceptId}' does in this circuit?";
        }

        private string BuildHint(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            return $"Look at how '{conceptId}' affects the circuit before making your next connection.";
        }

        private string BuildExplanation(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            return $"Focus on '{conceptId}' first. Once its behavior is understood, the next electrical concept becomes easier to understand.";
        }

        private void EnsureInitialized()
        {
            if (!initialized)
                Initialize();
        }
    }
}