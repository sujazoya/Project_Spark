using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic teaching director for Spark AI.
    ///
    /// Responsibilities:
    /// - Observes the current AI reasoning state.
    /// - Looks at recent AI memory.
    /// - Accesses educational knowledge.
    /// - Produces a teaching recommendation.
    ///
    /// This is NOT an autonomous gameplay controller.
    /// It does not modify:
    /// - Circuit topology
    /// - Electrical solver state
    /// - Player objects
    /// - Level state
    /// - Component state
    ///
    /// Later this layer can become the decision bridge between
    /// deterministic reasoning and character/voice systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIDirector : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField] private SparkAIBrain brain;
        [SerializeField] private SparkAIMemory memory;
        [SerializeField] private SparkAIKnowledge knowledge;

        [Header("Director Settings")]
        [SerializeField]
        private bool observeBrainChanges = true;

        private SparkAIDirective currentDirective;
        private bool initialized;

        public SparkAIDirective CurrentDirective => currentDirective;
        public bool IsInitialized => initialized;       

        private void Awake()
        {
            ResolveReferences();

            if (brain == null)
            {
                Debug.LogError(
                    "[Spark AI Director] SparkAIBrain reference is missing.",
                    this);

                return;
            }

            if (knowledge == null)
            {
                Debug.LogError(
                    "[Spark AI Director] SparkAIKnowledge reference is missing.",
                    this);

                return;
            }

            if (observeBrainChanges)
            {
                brain.ReasoningChanged -= HandleReasoningChanged;
                brain.ReasoningChanged += HandleReasoningChanged;
            }

            initialized = true;

            EvaluateNow();
        }

        private void OnDestroy()
        {
            if (brain != null)
                brain.ReasoningChanged -= HandleReasoningChanged;
        }

        private void ResolveReferences()
        {
            // Explicit inspector references only.
            // No FindObjectOfType / FindFirstObjectByType.
        }

        /// <summary>
        /// Evaluates the current Spark AI situation.
        /// </summary>
        public SparkAIDirective EvaluateNow()
        {
            if (brain == null || !brain.IsInitialized)
            {
                return SetDirective(
                    SparkAIDirective.Unavailable(
                        "Spark AI reasoning is not available."));
            }

            SparkAIReasoningResult reasoning = brain.CurrentReasoning;

            SparkAIDirective directive = BuildDirective(reasoning);

            return SetDirective(directive);
        }

        private void HandleReasoningChanged(
            object sender,
            SparkAIReasoningChangedEventArgs args)
        {
            if (!observeBrainChanges)
                return;

            EvaluateNow();
        }

        private SparkAIDirective BuildDirective(
            SparkAIReasoningResult reasoning)
        {
            if (!reasoning.IsValid)
            {
                return SparkAIDirective.Unavailable(
                    "Spark AI does not currently have enough information to guide the player.");
            }

            switch (reasoning.State)
            {
                case SparkAIReasoningState.Unavailable:
                    return SparkAIDirective.Unavailable(
                        "Spark AI is currently unavailable.");

                case SparkAIReasoningState.NoActiveLevel:
                    return SparkAIDirective.Wait(
                        "There is no active learning level.");

                case SparkAIReasoningState.LevelCompleted:
                    return SparkAIDirective.Celebrate(
                        "The current learning objective has been completed.");

                case SparkAIReasoningState.LevelFailed:
                    return SparkAIDirective.ExplainProblem(
                        "The current level has failed. Review the circuit and determine what went wrong.");

                case SparkAIReasoningState.WrongConnection:
                    return SparkAIDirective.Troubleshoot(
                        "Inspect the circuit connections and identify the incorrect connection.");

                case SparkAIReasoningState.SourceShort:
                    return SparkAIDirective.Troubleshoot(
                        "The power source appears to be shorted. Check the path between its terminals.");

                case SparkAIReasoningState.TargetShort:
                    return SparkAIDirective.Troubleshoot(
                        "The target appears to be shorted. Inspect the target connections before continuing.");

                case SparkAIReasoningState.NoValidPowerSource:
                    return SparkAIDirective.LearnConcept(
                        "voltage",
                        "The circuit does not currently have a valid active power source. Start by understanding what provides electrical potential.");

                case SparkAIReasoningState.InProgress:
                    return SparkAIDirective.GuideNextStep(
                        "The circuit is progressing. Inspect the remaining objective and determine the next connection or action.");

                case SparkAIReasoningState.Active:
                    return SparkAIDirective.Observe(
                        "The level is active. Observe the circuit and determine what the player is trying to accomplish.");

                default:
                    return SparkAIDirective.Observe(
                        "Continue observing the current electrical situation.");
            }
        }

        private SparkAIDirective SetDirective(
            SparkAIDirective directive)
        {
            currentDirective = directive;          

            return directive;
        }
    }

    /// <summary>
    /// High-level action recommendation from the Spark AI director.
    /// </summary>
    public readonly struct SparkAIDirective
    {
        public bool IsValid { get; }
        public SparkAIDirectiveType Type { get; }
        public string Message { get; }
        public string KnowledgeConceptId { get; }

        private SparkAIDirective(
            bool isValid,
            SparkAIDirectiveType type,
            string message,
            string knowledgeConceptId)
        {
            IsValid = isValid;
            Type = type;
            Message = message;
            KnowledgeConceptId = knowledgeConceptId;
        }

        public static SparkAIDirective Unavailable(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.Unavailable,
                message,
                string.Empty);
        }

        public static SparkAIDirective Wait(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.Wait,
                message,
                string.Empty);
        }

        public static SparkAIDirective Observe(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.Observe,
                message,
                string.Empty);
        }

        public static SparkAIDirective Celebrate(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.Celebrate,
                message,
                string.Empty);
        }

        public static SparkAIDirective ExplainProblem(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.ExplainProblem,
                message,
                string.Empty);
        }

        public static SparkAIDirective Troubleshoot(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.Troubleshoot,
                message,
                string.Empty);
        }

        public static SparkAIDirective GuideNextStep(string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.GuideNextStep,
                message,
                string.Empty);
        }

        public static SparkAIDirective LearnConcept(
            string conceptId,
            string message)
        {
            return new SparkAIDirective(
                true,
                SparkAIDirectiveType.LearnConcept,
                message,
                conceptId ?? string.Empty);
        }
    }

    public enum SparkAIDirectiveType
    {
        Unavailable = 0,
        Wait = 1,
        Observe = 2,
        GuideNextStep = 3,
        ExplainProblem = 4,
        Troubleshoot = 5,
        LearnConcept = 6,
        Celebrate = 7
    }


}