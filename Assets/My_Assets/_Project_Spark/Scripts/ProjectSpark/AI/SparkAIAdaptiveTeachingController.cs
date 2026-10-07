using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects adaptive teaching plans to the existing
    /// SparkAITeachingSession.
    ///
    /// Responsibilities:
    /// - Ask the adaptive planner for the best current teaching plan.
    /// - Convert that plan into a teaching decision.
    /// - Start or refresh the existing teaching session.
    ///
    /// This component does NOT:
    /// - modify the electrical solver
    /// - modify circuit topology
    /// - evaluate electrical correctness
    /// - directly control the player
    /// - run a continuous Update loop
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIAdaptiveTeachingController
        : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIAdaptiveTeachingPlanner planner;

        [SerializeField]
        private SparkAITeachingSession teachingSession;

        [Header("Behavior")]
        [SerializeField]
        private bool automaticallyBeginTeaching = false;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAIAdaptiveTeachingPlan
            CurrentPlan { get; private set; }

        public event Action<
            SparkAIAdaptiveTeachingPlan>
            PlanApplied;

        private void Awake()
        {
            if (planner == null)
            {
                Debug.LogError(
                    "[Spark AI Adaptive Teaching Controller] " +
                    "SparkAIAdaptiveTeachingPlanner reference is missing.",
                    this);

                return;
            }

            if (teachingSession == null)
            {
                Debug.LogError(
                    "[Spark AI Adaptive Teaching Controller] " +
                    "SparkAITeachingSession reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        private void Start()
        {
            if (!initialized)
                return;

            if (automaticallyBeginTeaching)
            {
                RefreshAndApply();
            }
        }

        /// <summary>
        /// Builds a new adaptive plan and applies it
        /// to the teaching session.
        /// </summary>
        public bool RefreshAndApply()
        {
            if (!initialized)
                return false;

            SparkAIAdaptiveTeachingPlan plan =
                planner.BuildCurrentPlan();

            if (!plan.IsValid)
            {
                return false;
            }

            return ApplyPlan(plan);
        }

        /// <summary>
        /// Applies an already-created adaptive plan.
        /// </summary>
        public bool ApplyPlan(
            SparkAIAdaptiveTeachingPlan plan)
        {
            if (!initialized ||
                !plan.IsValid)
            {
                return false;
            }

            SparkAITeachingDecision decision =
                BuildTeachingDecision(
                    plan);

            if (!decision.IsValid)
            {
                return false;
            }

            CurrentPlan =
                plan;

            teachingSession.Begin(
                decision);

            PlanApplied?.Invoke(
                plan);

            return true;
        }

        /// <summary>
        /// Rebuilds the plan only when the current teaching
        /// interaction has completed or been cancelled.
        /// </summary>
        public bool RefreshAfterTeaching()
        {
            if (!initialized)
                return false;

            if (teachingSession.IsActive)
                return false;

            return RefreshAndApply();
        }

        /// <summary>
        /// Ends the current teaching interaction and optionally
        /// creates a new adaptive plan.
        /// </summary>
        public bool CompleteAndRefresh()
        {
            if (!initialized)
                return false;

            if (teachingSession.IsActive)
            {
                teachingSession.Complete();
            }

            return RefreshAndApply();
        }

        /// <summary>
        /// Cancels the current interaction and optionally
        /// rebuilds the teaching plan.
        /// </summary>
        public bool CancelAndRefresh()
        {
            if (!initialized)
                return false;

            if (teachingSession.IsActive)
            {
                teachingSession.Cancel();
            }

            return RefreshAndApply();
        }

        private SparkAITeachingDecision
            BuildTeachingDecision(
                SparkAIAdaptiveTeachingPlan plan)
        {
            string message =
                BuildMessage(plan);

            string objective =
                plan.Objective;

            string question =
                plan.Question;

            string hint =
                plan.Hint;

            string explanation =
                plan.Explanation;

            string strongHint =
                BuildStrongHint(
                    plan);

            string[] conceptIds;

            if (string.IsNullOrEmpty(
                    plan.ConceptId))
            {
                conceptIds =
                    Array.Empty<string>();
            }
            else
            {
                conceptIds =
                    new[]
                    {
                        plan.ConceptId
                    };
            }

            return new SparkAITeachingDecision(
                true,
                UnityEngine.Time.time,
                DetermineAction(
                    plan),
                plan.Topic,
                plan.Intensity,
                plan.Priority,
                message,
                question,
                hint,
                strongHint,
                explanation,
                objective,
                conceptIds);
        }

        private SparkAITeachingAction
            DetermineAction(
                SparkAIAdaptiveTeachingPlan plan)
        {
            if (plan.Priority >= 0.90f)
            {
                return SparkAITeachingAction.Warning;
            }

            if (plan.Priority >= 0.65f)
            {
                return SparkAITeachingAction.Question;
            }

            if (plan.Priority >= 0.35f)
            {
                return SparkAITeachingAction.Hint;
            }

            return SparkAITeachingAction.Observe;
        }

        private string BuildMessage(
            SparkAIAdaptiveTeachingPlan plan)
        {
            if (!string.IsNullOrEmpty(
                    plan.Reason))
            {
                return plan.Reason;
            }

            return
                "Let's work on this electrical concept.";
        }

        private string BuildStrongHint(
            SparkAIAdaptiveTeachingPlan plan)
        {
            switch (plan.Topic)
            {
                case SparkAITeachingTopic.Voltage:
                    return
                        "Compare the voltage at the two points directly. " +
                        "Ask yourself what creates that difference.";

                case SparkAITeachingTopic.Current:
                    return
                        "Trace the complete conductive path. " +
                        "If the path is interrupted, current cannot continue.";

                case SparkAITeachingTopic.Resistance:
                    return
                        "Use Ohm's law: V = I × R. " +
                        "For the same voltage, increasing resistance reduces current.";

                case SparkAITeachingTopic.Power:
                    return
                        "Look at both voltage and current. " +
                        "Electrical power depends on their relationship.";

                case SparkAITeachingTopic.Polarity:
                    return
                        "Follow the positive and negative markings from the source " +
                        "to the component terminals.";

                case SparkAITeachingTopic.ShortCircuit:
                    return
                        "Look for a very low-resistance path that bypasses the load.";

                case SparkAITeachingTopic.Switch:
                    return
                        "Compare the available electrical path with the switch open " +
                        "and with the switch closed.";

                case SparkAITeachingTopic.Source:
                    return
                        "Find the component establishing the electrical potential. " +
                        "That is the source.";

                case SparkAITeachingTopic.CircuitPath:
                    return
                        "Start at the source positive terminal and trace the path " +
                        "through the circuit back to the source negative terminal.";

                default:
                    return
                        "Trace the circuit from the source, through the components, " +
                        "and back to the source.";
            }
        }
    }
    
}