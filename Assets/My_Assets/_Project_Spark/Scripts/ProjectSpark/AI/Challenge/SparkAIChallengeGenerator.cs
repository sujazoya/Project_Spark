using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts an adaptive teaching plan into a playable
    /// educational challenge.
    ///
    /// This generator only creates challenge instructions.
    /// It never changes the electrical circuit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIChallengeGenerator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIAdaptiveTeachingPlanner planner;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAILessonChallenge
            CurrentChallenge { get; private set; }

        public event Action<SparkAILessonChallenge>
            ChallengeCreated;


            [Header("Authored Challenges")]
[SerializeField] private SparkAILessonChallengeResolver lessonChallengeResolver;

[SerializeField] private bool preferAuthoredChallenges = true;

        private void Awake()
        {
            if (planner == null)
            {
                Debug.LogError(
                    "[Spark AI Challenge Generator] " +
                    "SparkAIAdaptiveTeachingPlanner reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Builds a challenge from the current adaptive plan.
        /// </summary>
        public SparkAILessonChallenge
            GenerateCurrentChallenge()
        {
            if (!initialized)
            {
                return SparkAILessonChallenge.Invalid();
            }

            SparkAIAdaptiveTeachingPlan plan =
                planner.CurrentPlan;

            if (!plan.IsValid)
            {
                plan =
                    planner.BuildCurrentPlan();
            }

            if (!plan.IsValid)
            {
                return SparkAILessonChallenge.Invalid();
            }

            return GenerateChallenge(
                plan);
        }

        /// <summary>
/// Creates the first authored challenge belonging to the
/// currently active lesson.
///
/// Returns Invalid when no authored challenge is available.
/// </summary>
public SparkAILessonChallenge GenerateAuthoredChallenge()
{
    if (lessonChallengeResolver == null)
        return SparkAILessonChallenge.Invalid();

    if (!lessonChallengeResolver.IsInitialized)
        lessonChallengeResolver.Initialize();

    if (!lessonChallengeResolver.ResolveActiveLesson())
        return SparkAILessonChallenge.Invalid();

    if (!lessonChallengeResolver.TryGetRequiredChallenge(
            0,
            out SparkAIChallengeDefinition definition))
    {
        return SparkAILessonChallenge.Invalid();
    }

    return SparkAIChallengeFactory.Create(definition);
}
/// <summary>
/// Generates the best available challenge for the current lesson.
///
/// Authored curriculum challenges are preferred when enabled.
/// Adaptive generation remains the fallback.
/// </summary>
public SparkAILessonChallenge GenerateLessonAwareChallenge()
{
    /*
     * Priority 1:
     * Use the authored challenge for the active lesson.
     *
     * This keeps the curriculum designer in control.
     */
    if (preferAuthoredChallenges)
    {
        SparkAILessonChallenge authored =
            GenerateAuthoredChallenge();

        if (authored.IsValid)
            return authored;
    }

    /*
     * Priority 2:
     * Fall back to the existing adaptive teaching system.
     *
     * The AdaptiveTeachingPlanner now respects concept
     * prerequisites, so its generated challenge will
     * naturally follow the learning progression.
     */
    return GenerateCurrentChallenge();
}

        /// <summary>
        /// Builds a challenge directly from an adaptive plan.
        /// </summary>
        public SparkAILessonChallenge
            GenerateChallenge(
                SparkAIAdaptiveTeachingPlan plan)
        {
            if (!initialized ||
                !plan.IsValid)
            {
                return SparkAILessonChallenge.Invalid();
            }

            SparkAIChallengeDifficulty difficulty =
                DetermineDifficulty(
                    plan);

            SparkAILessonChallenge challenge =
                BuildChallenge(
                    plan,
                    difficulty);

            if (!challenge.IsValid)
            {
                return challenge;
            }

            CurrentChallenge =
                challenge;

            ChallengeCreated?.Invoke(
                challenge);

            return challenge;
        }

        private SparkAIChallengeDifficulty
            DetermineDifficulty(
                SparkAIAdaptiveTeachingPlan plan)
        {
            switch (plan.Intensity)
            {
                case SparkAITeachingIntensity.Strong:
                    return SparkAIChallengeDifficulty.Guided;

                case SparkAITeachingIntensity.Medium:
                    return SparkAIChallengeDifficulty.Normal;

                case SparkAITeachingIntensity.Gentle:
                default:
                    return SparkAIChallengeDifficulty.Advanced;
            }
        }

        private SparkAILessonChallenge
            BuildChallenge(
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            string challengeId =
                BuildChallengeId(
                    plan);

            switch (plan.Topic)
            {
                case SparkAITeachingTopic.Source:
                    return BuildSourceChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Voltage:
                    return BuildVoltageChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Current:
                    return BuildCurrentChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Resistance:
                    return BuildResistanceChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Polarity:
                    return BuildPolarityChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.ShortCircuit:
                    return BuildShortCircuitChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Switch:
                    return BuildSwitchChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.CircuitPath:
                    return BuildCircuitPathChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Measurement:
                    return BuildMeasurementChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.Power:
                    return BuildPowerChallenge(
                        challengeId,
                        plan,
                        difficulty);

                case SparkAITeachingTopic.CircuitBasics:
                default:
                    return BuildCircuitBasicsChallenge(
                        challengeId,
                        plan,
                        difficulty);
            }
        }

        private SparkAILessonChallenge
            BuildSourceChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Find the Source",
                "Identify the component that provides electrical energy.",
                "Find the power source and identify its positive and negative terminals.",
                "You correctly identified the electrical source.",
                "Look for the component establishing the circuit voltage.",
                "The source creates the electrical potential that drives the circuit.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            BuildVoltageChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Create a Voltage Difference",
                "Understand voltage as a potential difference.",
                "Connect the two measurement points and determine which has the higher potential.",
                "You correctly identified the voltage relationship.",
                "Compare the electrical potential at the two points.",
                "Voltage describes the difference in electrical potential between two points.",
                true,
                true,
                true);
        }

        private SparkAILessonChallenge
            BuildCurrentChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Make Current Flow",
                "Understand that current requires a conductive path.",
                "Complete the circuit so that current can flow through the intended load.",
                "The circuit now provides a valid path for current.",
                "Trace the path from the source through the load and back.",
                "Current requires a complete conductive path.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            BuildResistanceChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Control the Current",
                "Understand the relationship between resistance and current.",
                "Change the resistance and observe how the circuit current changes.",
                "You correctly observed the effect of resistance on current.",
                "Keep the source voltage the same and change only resistance.",
                "For a fixed voltage, increasing resistance reduces current.",
                true,
                true,
                true);
        }

        private SparkAILessonChallenge
            BuildPolarityChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Match the Polarity",
                "Understand positive and negative electrical connections.",
                "Connect the component with the correct polarity.",
                "The component is connected with the intended polarity.",
                "Check the positive and negative markings.",
                "Trace positive from the source to the positive side of the component.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            BuildShortCircuitChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Find the Bypass",
                "Understand what creates a short circuit.",
                "Identify the unintended low-resistance path and remove it.",
                "The unwanted bypass path has been removed.",
                "Look for a path around the intended load.",
                "A short circuit provides a very low-resistance alternative path.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            BuildSwitchChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Open and Close the Path",
                "Understand how a switch controls a circuit path.",
                "Open and close the switch and observe what happens to the circuit.",
                "You observed the difference between an open and closed switch.",
                "Compare the circuit path in both switch states.",
                "An open switch interrupts the path; a closed switch completes it.",
                true,
                true,
                true);
        }

        private SparkAILessonChallenge
            BuildCircuitPathChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Trace the Complete Path",
                "Understand how the electrical path connects the source and load.",
                "Build a complete path from the source positive terminal through the load and back to the source negative terminal.",
                "The intended electrical path is complete.",
                "Start at source positive and trace the path through every connection.",
                "Current needs a complete path through the circuit.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            BuildMeasurementChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Measure the Circuit",
                "Learn how electrical quantities are observed.",
                "Use the appropriate measurement tool and determine the requested electrical value.",
                "You selected the appropriate measurement method.",
                "First decide whether you need voltage or current.",
                "Different electrical quantities require different measurement methods.",
                false,
                true,
                true);
        }

        private SparkAILessonChallenge
            BuildPowerChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Understand Electrical Power",
                "Understand how voltage and current relate to power.",
                "Measure the circuit and determine which condition produces greater electrical power.",
                "You correctly compared the electrical power.",
                "Look at both voltage and current.",
                "Electrical power is related to voltage and current.",
                false,
                true,
                true);
        }

        private SparkAILessonChallenge
            BuildCircuitBasicsChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty)
        {
            return CreateChallenge(
                challengeId,
                plan,
                difficulty,
                "Build a Simple Circuit",
                "Understand the basic source-to-load circuit relationship.",
                "Connect a source to a load and create a complete return path.",
                "The basic circuit has been completed.",
                "Start with the source, then connect the load, then complete the return path.",
                "A working circuit needs a source, a conductive path, and a load.",
                true,
                false,
                true);
        }

        private SparkAILessonChallenge
            CreateChallenge(
                string challengeId,
                SparkAIAdaptiveTeachingPlan plan,
                SparkAIChallengeDifficulty difficulty,
                string title,
                string objective,
                string instruction,
                string successDescription,
                string hint,
                string strongHint,
                bool requiresCircuitChange,
                bool requiresMeasurement,
                bool requiresPlayerAnswer)
        {
            return new SparkAILessonChallenge(
                true,
                challengeId,
                plan.ConceptId,
                plan.Topic,
                difficulty,
                title,
                objective,
                instruction,
                successDescription,
                hint,
                strongHint,
                requiresCircuitChange,
                requiresMeasurement,
                requiresPlayerAnswer,
                UnityEngine.Time.time);
        }

        private string BuildChallengeId(
            SparkAIAdaptiveTeachingPlan plan)
        {
            string concept =
                string.IsNullOrEmpty(
                    plan.ConceptId)
                    ? "general"
                    : plan.ConceptId;

            return
                "adaptive_" +
                concept +
                "_" +
                plan.Topic;
        }
    }
}