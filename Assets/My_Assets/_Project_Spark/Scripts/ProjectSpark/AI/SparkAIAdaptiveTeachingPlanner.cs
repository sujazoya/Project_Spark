using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Selects the next educational focus using:
    /// - current AI teaching context
    /// - learner concept mastery
    /// - repeated mistakes
    /// - available Project Spark knowledge concepts
    /// - active lesson concept restrictions
    ///
    /// This component does not modify:
    /// - the electrical solver
    /// - circuit topology
    /// - level evaluation
    /// - player state
    ///
    /// It only recommends what the AI should teach next.
    ///
    /// Lesson restriction:
    /// When a SparkAILesson is assigned and has an active lesson,
    /// only concepts belonging to that lesson are considered.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIAdaptiveTeachingPlanner : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIContextReasoner contextReasoner;

        [SerializeField]
        private SparkAILearnerModel learnerModel;

        [SerializeField]
        private SparkAITeachingReasoner teachingReasoner;

        [SerializeField]
        private SparkAILesson lesson;

        [Header("Planning")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float strugglingThreshold = 0.30f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float developingThreshold = 0.60f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float masteredThreshold = 0.90f;

        [SerializeField]
        private int maximumCandidateConcepts = 16;

        private readonly List<SparkAIAdaptiveCandidate>
            candidates =
                new List<SparkAIAdaptiveCandidate>(16);

        private readonly HashSet<string>
            activeLessonConcepts =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAIAdaptiveTeachingPlan
            CurrentPlan { get; private set; }

        public event Action<SparkAIAdaptiveTeachingPlan>
            PlanChanged;


            [Header("Concept Progression")]
[SerializeField] private SparkAIConceptProgressionPlanner conceptProgressionPlanner;

[SerializeField] private bool respectConceptPrerequisites = true;

        // ---------------------------------------------------------
        // Initialization
        // ---------------------------------------------------------

        private void Awake()
        {
            if (contextReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Adaptive Planner] " +
                    "SparkAIContextReasoner reference is missing.",
                    this);

                return;
            }

            if (learnerModel == null)
            {
                Debug.LogError(
                    "[Spark AI Adaptive Planner] " +
                    "SparkAILearnerModel reference is missing.",
                    this);

                return;
            }

            if (teachingReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Adaptive Planner] " +
                    "SparkAITeachingReasoner reference is missing.",
                    this);

                return;
            }

            maximumCandidateConcepts =
                Mathf.Clamp(
                    maximumCandidateConcepts,
                    4,
                    64);

            initialized = true;
        }

        // ---------------------------------------------------------
        // Public Planning API
        // ---------------------------------------------------------

        /// <summary>
        /// Creates an adaptive teaching plan from the
        /// current Project Spark context.
      public SparkAIAdaptiveTeachingPlan BuildCurrentPlan()
{
    if (!initialized)
    {
        return SparkAIAdaptiveTeachingPlan.Invalid();
    }

    // Refresh the lesson before reasoning about the world.
    // A new level may not have a meaningful circuit context yet.
    RefreshActiveLessonConcepts();

    SparkAIContextReasoning context =
        contextReasoner.ReasonCurrentContext();

    // At level start there may be no circuit to reason about.
    // Fall back to the authored lesson curriculum.
    if (!context.IsValid)
    {
        SparkAIAdaptiveTeachingPlan lessonPlan =
            BuildLessonFallbackPlan();

        CurrentPlan = lessonPlan;
        PlanChanged?.Invoke(lessonPlan);

        return lessonPlan;
    }

    SparkAIAdaptiveTeachingPlan result =
        BuildPlan(context);

    CurrentPlan = result;
    PlanChanged?.Invoke(result);

    return result;
}
private SparkAIAdaptiveTeachingPlan BuildLessonFallbackPlan()
{
    if (activeLessonConcepts.Count == 0)
    {
        Debug.LogWarning(
            "[Spark AI Adaptive Planner] " +
            "Teaching context is unavailable and " +
            "the active lesson contains no concepts.",
            this);

        return SparkAIAdaptiveTeachingPlan.Invalid();
    }

    // Prefer the authored lesson order.
    if (lesson != null)
    {
        SparkAILessonDefinition activeLesson =
            lesson.ActiveLesson;

        string[] conceptIds =
            activeLesson.ConceptIds;

        if (conceptIds != null)
        {
            for (int i = 0; i < conceptIds.Length; i++)
            {
                string conceptId = conceptIds[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                if (!activeLessonConcepts.Contains(conceptId))
                    continue;

                SparkAITeachingTopic topic =
                    ConvertConceptToTopic(conceptId);

                return new SparkAIAdaptiveTeachingPlan(
                    true,
                    Time.time,
                    topic,
                    conceptId,
                    0.25f,
                    DetermineIntensity(conceptId),
                    "Let's begin with the first concept in this lesson.",
                    BuildLearnerObjectiveForConcept(conceptId),
                    BuildQuestion(topic, conceptId),
                    BuildHint(topic, conceptId),
                    BuildExplanation(topic, conceptId));
            }
        }
    }

    // Safety fallback.
    foreach (string conceptId in activeLessonConcepts)
    {
        SparkAITeachingTopic topic =
            ConvertConceptToTopic(conceptId);

        return new SparkAIAdaptiveTeachingPlan(
            true,
            Time.time,
            topic,
            conceptId,
            0.25f,
            DetermineIntensity(conceptId),
            "The active lesson requires this concept.",
            BuildLearnerObjectiveForConcept(conceptId),
            BuildQuestion(topic, conceptId),
            BuildHint(topic, conceptId),
            BuildExplanation(topic, conceptId));
    }

    return SparkAIAdaptiveTeachingPlan.Invalid();
}
            private string ResolveTeachingConcept(string conceptId)
            {
                if (!respectConceptPrerequisites)
                    return conceptId;

                if (conceptProgressionPlanner == null)
                    return conceptId;

                if (string.IsNullOrWhiteSpace(conceptId))
                    return conceptId;

                if (!conceptProgressionPlanner.TryGetNextConcept(
                    conceptId,
                    out SparkAIConceptDefinition nextConcept))
                {
                    return conceptId;
                }

                if (!nextConcept.IsValid)
                    return conceptId;

                return nextConcept.Id;
            }

        /// <summary>
        /// Creates a plan from an already calculated context.
        /// </summary>
        public SparkAIAdaptiveTeachingPlan
            BuildPlan(
                SparkAIContextReasoning context)
        {
            if (!initialized ||
                !context.IsValid)
            {
                return SparkAIAdaptiveTeachingPlan.Invalid();
            }

            RefreshActiveLessonConcepts();

            candidates.Clear();

            AddContextCandidate(
                context);

            AddLearnerCandidates();

            if (candidates.Count == 0)
            {
                return BuildFallbackPlan(
                    context);
            }

            SparkAIAdaptiveCandidate best =
                SelectBestCandidate();

            return new SparkAIAdaptiveTeachingPlan(
                true,
                Time.time,
                best.Topic,
                best.ConceptId,
                best.Priority,
                best.RecommendedIntensity,
                best.Reason,
                best.Objective,
                best.SuggestedQuestion,
                best.SuggestedHint,
                best.SuggestedExplanation);
        }

        // ---------------------------------------------------------
        // Lesson restriction
        // ---------------------------------------------------------

        /// <summary>
        /// Reads the currently active lesson and builds a fast
        /// lookup set of allowed concepts.
        ///
        /// SparkAILessonDefinition is a struct.
        /// Therefore no null comparison is required.
        /// </summary>
        private void RefreshActiveLessonConcepts()
        {
            activeLessonConcepts.Clear();

            if (lesson == null)
                return;

            SparkAILessonDefinition activeLesson =
                lesson.ActiveLesson;

            if (activeLesson.ConceptIds == null)
                return;

            int count =
                Mathf.Min(
                    activeLesson.ConceptIds.Length,
                    maximumCandidateConcepts);

            for (int i = 0; i < count; i++)
            {
                string conceptId =
                    activeLesson.ConceptIds[i];

                if (string.IsNullOrWhiteSpace(
                        conceptId))
                {
                    continue;
                }

                activeLessonConcepts.Add(
                    conceptId);
            }
        }

        /// <summary>
        /// Returns true when the concept is allowed by the
        /// active lesson.
        ///
        /// If no lesson is assigned, the planner behaves exactly
        /// like the original general adaptive planner.
        /// </summary>
        private bool IsConceptAllowed(
            string conceptId)
        {
            if (lesson == null)
                return true;

            /*
             * No active lesson concepts means there is currently
             * no lesson restriction available.
             *
             * This prevents the planner from becoming unusable
             * before a level/lesson has been activated.
             */
            if (activeLessonConcepts.Count == 0)
                return true;

            if (string.IsNullOrEmpty(
                    conceptId))
            {
                return false;
            }

            return activeLessonConcepts.Contains(
                conceptId);
        }

        // ---------------------------------------------------------
        // Candidate Creation
        // ---------------------------------------------------------

        private void AddContextCandidate(
            SparkAIContextReasoning context)
        {
            string conceptId =
                GetConceptForContext(
                    context);

                    conceptId = ResolveTeachingConcept(conceptId);

            /*
             * Critical lesson restriction:
             *
             * If the current circuit suggests a concept that is
             * NOT part of the current lesson, do not let the
             * context candidate override the lesson curriculum.
             */
            if (!IsConceptAllowed(
                    conceptId))
            {
                return;
            }

            SparkAITeachingTopic topic =
                ConvertFocusToTopic(
                    context.Focus);

            float priority =
                GetContextPriority(
                    context.State);

            candidates.Add(
                new SparkAIAdaptiveCandidate(
                    conceptId,
                    topic,
                    priority,
                    DetermineIntensity(
                        conceptId),
                    "Current circuit context requires attention.",
                    context.TeachingGoal,
                    BuildQuestion(
                        topic,
                        conceptId),
                    BuildHint(
                        topic,
                        conceptId),
                    context.Explanation));
        }

      private void AddLearnerCandidates()
{
    IReadOnlyCollection<SparkAILearnerConcept> concepts =
        learnerModel.GetAllConcepts();

    int added = 0;

    foreach (SparkAILearnerConcept concept in concepts)
    {
        if (!concept.IsValid)
            continue;

        string teachingConceptId =
            concept.ConceptId;

        /*
         * If this concept has an unmet prerequisite,
         * teach the prerequisite first.
         */
        if (respectConceptPrerequisites &&
            conceptProgressionPlanner != null)
        {
            if (conceptProgressionPlanner.TryGetNextConcept(
                    concept.ConceptId,
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
         * Only concepts taught by the active lesson
         * are allowed into the adaptive candidate list.
         */
        if (!IsConceptAllowed(teachingConceptId))
            continue;

        if (added >= maximumCandidateConcepts)
            break;

        float priority =
            CalculateLearnerPriority(
                concept);

        if (priority <= 0f)
            continue;

        SparkAITeachingTopic topic =
            ConvertConceptToTopic(
                teachingConceptId);

        candidates.Add(
            new SparkAIAdaptiveCandidate(
                teachingConceptId,
                topic,
                priority,
                DetermineIntensity(
                    concept.ConceptId),
                BuildLearnerReason(
                    concept),
                BuildLearnerObjective(
                    concept),
                BuildQuestion(
                    topic,
                    teachingConceptId),
                BuildHint(
                    topic,
                    teachingConceptId),
                BuildExplanation(
                    topic,
                    teachingConceptId)));

        added++;
    }
}
        // ---------------------------------------------------------
        // Candidate Selection
        // ---------------------------------------------------------

        private SparkAIAdaptiveCandidate
            SelectBestCandidate()
        {
            SparkAIAdaptiveCandidate best =
                candidates[0];

            for (int i = 1;
                 i < candidates.Count;
                 i++)
            {
                SparkAIAdaptiveCandidate candidate =
                    candidates[i];

                if (candidate.Priority >
                    best.Priority)
                {
                    best =
                        candidate;
                }
            }

            return best;
        }

        private SparkAIAdaptiveTeachingPlan
            BuildFallbackPlan(
                SparkAIContextReasoning context)
        {
            /*
             * If a lesson is active but none of the context
             * concepts are allowed and there are no learner
             * candidates, do not invent an unrelated concept.
             *
             * Instead select the first concept explicitly
             * taught by the lesson.
             */
            if (activeLessonConcepts.Count > 0)
            {
                foreach (string conceptId
                    in activeLessonConcepts)
                {
                    SparkAITeachingTopic lessonTopic =
                        ConvertConceptToTopic(
                            conceptId);

                    return new SparkAIAdaptiveTeachingPlan(
                        true,
                        Time.time,
                        lessonTopic,
                        conceptId,
                        0.25f,
                        DetermineIntensity(
                            conceptId),
                        "The active lesson requires practice of this concept.",
                        BuildLearnerObjectiveForConcept(
                            conceptId),
                        BuildQuestion(
                            lessonTopic,
                            conceptId),
                        BuildHint(
                            lessonTopic,
                            conceptId),
                        BuildExplanation(
                            lessonTopic,
                            conceptId));
                }
            }

            /*
             * Original general fallback behaviour.
             */
            SparkAITeachingTopic topic =
                ConvertFocusToTopic(
                    context.Focus);

            return new SparkAIAdaptiveTeachingPlan(
                true,
                Time.time,
                topic,
                GetConceptForContext(
                    context),
                0.25f,
                SparkAITeachingIntensity.Gentle,
                "No strong learner-specific evidence is available yet.",
                context.TeachingGoal,
                BuildQuestion(
                    topic,
                    string.Empty),
                BuildHint(
                    topic,
                    string.Empty),
                context.Explanation);
        }

        // ---------------------------------------------------------
        // Learner Priority
        // ---------------------------------------------------------

        private float CalculateLearnerPriority(
            SparkAILearnerConcept concept)
        {
            float mastery =
                concept.Mastery01;

            if (mastery >= masteredThreshold)
            {
                return 0.05f;
            }

            float weakness;

            if (mastery < strugglingThreshold)
            {
                weakness =
                    1.00f;
            }
            else if (mastery < developingThreshold)
            {
                weakness =
                    0.65f;
            }
            else
            {
                weakness =
                    0.30f;
            }

            float mistakeRatio = 0f;

            if (concept.Attempts > 0)
            {
                mistakeRatio =
                    (float)concept.IncorrectAnswers /
                    concept.Attempts;
            }

            float priority =
                weakness * 0.65f +
                mistakeRatio * 0.35f;

            return Mathf.Clamp01(
                priority);
        }

        private SparkAITeachingIntensity
            DetermineIntensity(
                string conceptId)
        {
            if (string.IsNullOrEmpty(
                    conceptId))
            {
                return SparkAITeachingIntensity.Gentle;
            }

            if (!learnerModel.TryGetConcept(
                    conceptId,
                    out SparkAILearnerConcept concept))
            {
                return SparkAITeachingIntensity.Gentle;
            }

            if (concept.Mastery01 <
                strugglingThreshold)
            {
                return SparkAITeachingIntensity.Strong;
            }

            if (concept.Mastery01 <
                developingThreshold)
            {
                return SparkAITeachingIntensity.Medium;
            }

            return SparkAITeachingIntensity.Gentle;
        }

        // ---------------------------------------------------------
        // Context → Topic
        // ---------------------------------------------------------

        private SparkAITeachingTopic
            ConvertFocusToTopic(
                SparkAIContextFocus focus)
        {
            switch (focus)
            {
                case SparkAIContextFocus.PowerSource:
                    return SparkAITeachingTopic.Source;

                case SparkAIContextFocus.VoltageSource:
                    return SparkAITeachingTopic.Voltage;

                case SparkAIContextFocus.CurrentFlow:
                    return SparkAITeachingTopic.Current;

                case SparkAIContextFocus.ShortCircuit:
                    return SparkAITeachingTopic.ShortCircuit;

                case SparkAIContextFocus.Switch:
                    return SparkAITeachingTopic.Switch;

                case SparkAIContextFocus.Resistance:
                    return SparkAITeachingTopic.Resistance;

                case SparkAIContextFocus.Polarity:
                    return SparkAITeachingTopic.Polarity;

                case SparkAIContextFocus.Measurement:
                    return SparkAITeachingTopic.Measurement;

                case SparkAIContextFocus.CircuitPath:
                    return SparkAITeachingTopic.CircuitPath;

                case SparkAIContextFocus.ElectricalBehavior:
                    return SparkAITeachingTopic.Current;

                case SparkAIContextFocus.CircuitBasics:
                default:
                    return SparkAITeachingTopic.CircuitBasics;
            }
        }

        // ---------------------------------------------------------
        // Concept → Topic
        // ---------------------------------------------------------

        private SparkAITeachingTopic
            ConvertConceptToTopic(
                string conceptId)
        {
            if (string.IsNullOrEmpty(
                    conceptId))
            {
                return SparkAITeachingTopic.CircuitBasics;
            }

            switch (conceptId.ToLowerInvariant())
            {
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
                    return SparkAITeachingTopic.CircuitBasics;

                case "source":
                    return SparkAITeachingTopic.Source;

                default:
                    return SparkAITeachingTopic.CircuitBasics;
            }
        }

        // ---------------------------------------------------------
        // Context → Concept
        // ---------------------------------------------------------

        private string GetConceptForContext(
            SparkAIContextReasoning context)
        {
            switch (context.Focus)
            {
                case SparkAIContextFocus.VoltageSource:
                    return "voltage";

                case SparkAIContextFocus.CurrentFlow:
                    return "current";

                case SparkAIContextFocus.Resistance:
                    return "resistance";

                case SparkAIContextFocus.ShortCircuit:
                    return "short_circuit";

                case SparkAIContextFocus.Polarity:
                    return "polarity";

                case SparkAIContextFocus.PowerSource:
                    return "source";

                case SparkAIContextFocus.CircuitPath:
                    return "closed_circuit";

                default:
                    return "circuit_basics";
            }
        }

        // ---------------------------------------------------------
        // Context Priority
        // ---------------------------------------------------------

        private float GetContextPriority(
            SparkAIContextState state)
        {
            switch (state)
            {
                case SparkAIContextState.ShortCircuit:
                    return 1.00f;

                case SparkAIContextState.NoPowerSource:
                    return 0.95f;

                case SparkAIContextState.IncompletePowerSource:
                    return 0.90f;

                case SparkAIContextState.OpenCircuit:
                    return 0.85f;

                case SparkAIContextState.VoltageWithoutUsefulCurrent:
                    return 0.80f;

                case SparkAIContextState.ClosedCircuit:
                case SparkAIContextState.ActiveCircuit:
                    return 0.40f;

                case SparkAIContextState.NoConnections:
                    return 0.75f;

                case SparkAIContextState.ObservingCircuit:
                    return 0.25f;

                default:
                    return 0.20f;
            }
        }

        // ---------------------------------------------------------
        // Learner Text
        // ---------------------------------------------------------

        private string BuildLearnerReason(
            SparkAILearnerConcept concept)
        {
            if (concept.Mastery01 <
                strugglingThreshold)
            {
                return
                    "The player is struggling with this concept.";
            }

            if (concept.Mastery01 <
                developingThreshold)
            {
                return
                    "The player is still developing this concept.";
            }

            return
                "This concept can be reinforced.";
        }

        private string BuildLearnerObjective(
            SparkAILearnerConcept concept)
        {
            return BuildLearnerObjectiveForConcept(
                concept.ConceptId);
        }

        private string BuildLearnerObjectiveForConcept(
            string conceptId)
        {
            return
                "Strengthen understanding of " +
                conceptId +
                " through a small practical challenge.";
        }

        // ---------------------------------------------------------
        // Questions
        // ---------------------------------------------------------

        private string BuildQuestion(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Voltage:
                    return
                        "What do you think the voltage difference is doing here?";

                case SparkAITeachingTopic.Current:
                    return
                        "What do you think is allowing current to flow?";

                case SparkAITeachingTopic.Resistance:
                    return
                        "What would happen to current if resistance increased?";

                case SparkAITeachingTopic.Power:
                    return
                        "What do you think determines how much electrical power is used?";

                case SparkAITeachingTopic.Polarity:
                    return
                        "Which terminal should connect to the positive side?";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "What path is allowing current to bypass the intended load?";

                case SparkAITeachingTopic.Switch:
                    return
                        "What changes in the circuit when the switch closes?";

                case SparkAITeachingTopic.Source:
                    return
                        "Which part of this circuit is providing electrical energy?";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "Can you identify the complete path for current?";

                default:
                    return
                        "What do you think is happening in this circuit?";
            }
        }

        // ---------------------------------------------------------
        // Hints
        // ---------------------------------------------------------

        private string BuildHint(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Voltage:
                    return
                        "Look at the voltage difference between the two terminals.";

                case SparkAITeachingTopic.Current:
                    return
                        "Current needs a conductive path through the circuit.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "For the same voltage, increasing resistance reduces current.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "Check the positive and negative terminal markings.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "Look for a low-resistance path around the intended component.";

                case SparkAITeachingTopic.Switch:
                    return
                        "Compare the circuit path when the switch is open and closed.";

                case SparkAITeachingTopic.Source:
                    return
                        "Find the component that establishes the circuit voltage.";

                default:
                    return
                        "Trace the circuit from the source and follow the electrical path.";
            }
        }

        // ---------------------------------------------------------
        // Explanations
        // ---------------------------------------------------------

        private string BuildExplanation(
            SparkAITeachingTopic topic,
            string conceptId)
        {
            switch (topic)
            {
                case SparkAITeachingTopic.Voltage:
                    return
                        "Voltage is an electrical potential difference between two points.";

                case SparkAITeachingTopic.Current:
                    return
                        "Current is the flow of electric charge through a conductive path.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "Resistance opposes current. For a fixed voltage, higher resistance means lower current.";

                case SparkAITeachingTopic.Power:
                    return
                        "Electrical power describes the rate at which electrical energy is transferred.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "Polarity identifies the positive and negative sides of an electrical connection.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "A short circuit creates an unintended low-resistance path that can allow excessive current.";

                case SparkAITeachingTopic.Switch:
                    return
                        "A closed switch provides a conductive path, while an open switch interrupts that path.";

                case SparkAITeachingTopic.Source:
                    return
                        "A source establishes electrical potential and supplies energy to the circuit.";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "A complete circuit provides a continuous path through which current can flow.";

                default:
                    return
                        "Electrical behavior depends on the source, the connections, and the components in the circuit.";
            }
        }
    }

    // =============================================================
    // Candidate
    // =============================================================

    public readonly struct SparkAIAdaptiveCandidate
    {
        public string ConceptId { get; }

        public SparkAITeachingTopic Topic { get; }

        public float Priority { get; }

        public SparkAITeachingIntensity RecommendedIntensity { get; }

        public string Reason { get; }

        public string Objective { get; }

        public string SuggestedQuestion { get; }

        public string SuggestedHint { get; }

        public string SuggestedExplanation { get; }

        public SparkAIAdaptiveCandidate(
            string conceptId,
            SparkAITeachingTopic topic,
            float priority,
            SparkAITeachingIntensity recommendedIntensity,
            string reason,
            string objective,
            string suggestedQuestion,
            string suggestedHint,
            string suggestedExplanation)
        {
            ConceptId =
                conceptId ??
                string.Empty;

            Topic =
                topic;

            Priority =
                Mathf.Clamp01(
                    priority);

            RecommendedIntensity =
                recommendedIntensity;

            Reason =
                reason ??
                string.Empty;

            Objective =
                objective ??
                string.Empty;

            SuggestedQuestion =
                suggestedQuestion ??
                string.Empty;

            SuggestedHint =
                suggestedHint ??
                string.Empty;

            SuggestedExplanation =
                suggestedExplanation ??
                string.Empty;
        }
    }

    // =============================================================
    // Teaching Plan
    // =============================================================

    public readonly struct SparkAIAdaptiveTeachingPlan
    {
        public bool IsValid { get; }

        public float CreatedAt { get; }

        public SparkAITeachingTopic Topic { get; }

        public string ConceptId { get; }

        public float Priority { get; }

        public SparkAITeachingIntensity Intensity { get; }

        public string Reason { get; }

        public string Objective { get; }

        public string Question { get; }

        public string Hint { get; }

        public string Explanation { get; }

        public SparkAIAdaptiveTeachingPlan(
            bool isValid,
            float createdAt,
            SparkAITeachingTopic topic,
            string conceptId,
            float priority,
            SparkAITeachingIntensity intensity,
            string reason,
            string objective,
            string question,
            string hint,
            string explanation)
        {
            IsValid =
                isValid;

            CreatedAt =
                createdAt;

            Topic =
                topic;

            ConceptId =
                conceptId ??
                string.Empty;

            Priority =
                Mathf.Clamp01(
                    priority);

            Intensity =
                intensity;

            Reason =
                reason ??
                string.Empty;

            Objective =
                objective ??
                string.Empty;

            Question =
                question ??
                string.Empty;

            Hint =
                hint ??
                string.Empty;

            Explanation =
                explanation ??
                string.Empty;
        }

        public static SparkAIAdaptiveTeachingPlan Invalid()
        {
            return new SparkAIAdaptiveTeachingPlan(
                false,
                UnityEngine.Time.time,
                SparkAITeachingTopic.CircuitBasics,
                string.Empty,
                0f,
                SparkAITeachingIntensity.Gentle,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }
    }
}