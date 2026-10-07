using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Manages one AI teaching interaction.
    ///
    /// This layer controls the progression of teaching assistance:
    ///
    /// Observe
    ///     ↓
    /// Question
    ///     ↓
    /// Hint
    ///     ↓
    /// Strong Hint
    ///     ↓
    /// Explanation
    ///
    /// It does not:
    /// - solve circuits
    /// - modify electrical values
    /// - modify level evaluation
    /// - control player objects
    /// - decide whether a level is complete
    ///
    /// It only manages the teaching interaction state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITeachingSession : MonoBehaviour
    {
        [Header("Teaching")]
        [SerializeField]
        private SparkAITeachingReasoner teachingReasoner;

        [Header("Progression")]
        [SerializeField]
        private bool autoStartSession = true;

        [SerializeField]
        private bool allowAutomaticEscalation = false;

        [SerializeField]
        private float minimumEscalationDelay = 8f;

        [SerializeField]
        private int maximumHints = 2;

        private SparkAITeachingSessionState state =
            SparkAITeachingSessionState.Idle;

        private SparkAITeachingDecision decision;

        private int hintCount;

        private float stateStartedAt;

        private string activeTopicKey =
            string.Empty;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public SparkAITeachingSessionState State =>
            state;

        public SparkAITeachingDecision Decision =>
            decision;

        public int HintCount =>
            hintCount;

        public float StateElapsedTime =>
            Mathf.Max(
                0f,
                Time.time - stateStartedAt);

        public bool IsActive =>
            state != SparkAITeachingSessionState.Idle &&
            state != SparkAITeachingSessionState.Completed &&
            state != SparkAITeachingSessionState.Cancelled;

        public event Action<
            SparkAITeachingSessionState> StateChanged;

        public event Action<
            SparkAITeachingDecision> TeachingChanged;

        private void Awake()
        {
            if (teachingReasoner == null)
            {
                Debug.LogError(
                    "[Spark AI Teaching Session] " +
                    "SparkAITeachingReasoner reference is missing.",
                    this);

                return;
            }

            minimumEscalationDelay =
                Mathf.Max(
                    0f,
                    minimumEscalationDelay);

            maximumHints =
                Mathf.Clamp(
                    maximumHints,
                    0,
                    10);

            initialized = true;
        }

        private void Start()
        {
            if (!initialized)
                return;

            if (autoStartSession)
            {
                BeginFromCurrentContext();
            }
        }

        private void Update()
        {
            if (!initialized ||
                !allowAutomaticEscalation ||
                !IsActive)
            {
                return;
            }

            if (StateElapsedTime <
                minimumEscalationDelay)
            {
                return;
            }

            TryAutomaticEscalation();
        }

        /// <summary>
        /// Starts a teaching session using the current deterministic
        /// teaching context.
        /// </summary>
        public bool BeginFromCurrentContext()
        {
            if (!initialized)
                return false;

            SparkAITeachingDecision newDecision =
                teachingReasoner.DecideCurrentTeaching();

            return Begin(newDecision);
        }

        /// <summary>
        /// Starts a teaching session from a supplied teaching decision.
        /// </summary>
        public bool Begin(
            SparkAITeachingDecision newDecision)
        {
            if (!initialized ||
                !newDecision.IsValid)
            {
                return false;
            }

            decision =
                newDecision;

            hintCount = 0;

            activeTopicKey =
                BuildTopicKey(
                    newDecision);

            SetState(
                SparkAITeachingSessionState.Question);

            RaiseTeachingChanged();

            return true;
        }

        /// <summary>
        /// Player requests the first hint.
        /// </summary>
        public bool RequestHint()
        {
            if (!initialized ||
                !IsActive)
            {
                return false;
            }

            if (hintCount >= maximumHints)
            {
                return RequestStrongHint();
            }

            hintCount++;

            SetState(
                SparkAITeachingSessionState.Hint);

            RaiseTeachingChanged();

            return true;
        }

        /// <summary>
        /// Player requests a stronger hint.
        /// </summary>
        public bool RequestStrongHint()
        {
            if (!initialized ||
                !IsActive)
            {
                return false;
            }

            SetState(
                SparkAITeachingSessionState.StrongHint);

            RaiseTeachingChanged();

            return true;
        }

        /// <summary>
        /// Player requests the direct explanation.
        ///
        /// This is intentionally explicit. The system does not
        /// automatically reveal the answer unless automatic
        /// escalation has been enabled.
        /// </summary>
        public bool RequestExplanation()
        {
            if (!initialized ||
                !IsActive)
            {
                return false;
            }

            SetState(
                SparkAITeachingSessionState.Explanation);

            RaiseTeachingChanged();

            return true;
        }

        /// <summary>
        /// Marks the current teaching interaction as understood.
        /// </summary>
        public bool MarkUnderstood()
        {
            if (!initialized ||
                !IsActive)
            {
                return false;
            }

            SetState(
                SparkAITeachingSessionState.Understood);

            return true;
        }

        /// <summary>
        /// Marks the current teaching interaction as completed.
        /// </summary>
        public bool Complete()
        {
            if (!initialized ||
                state ==
                SparkAITeachingSessionState.Idle)
            {
                return false;
            }

            SetState(
                SparkAITeachingSessionState.Completed);

            return true;
        }

        /// <summary>
        /// Cancels the current teaching interaction.
        /// </summary>
        public bool Cancel()
        {
            if (!initialized ||
                !IsActive)
            {
                return false;
            }

            SetState(
                SparkAITeachingSessionState.Cancelled);

            return true;
        }

        /// <summary>
        /// Resets the teaching session without starting a new one.
        /// </summary>
        public void Reset()
        {
            if (!initialized)
                return;

            decision =
                SparkAITeachingDecision.Invalid();

            hintCount = 0;

            activeTopicKey =
                string.Empty;

            SetState(
                SparkAITeachingSessionState.Idle);
        }

        /// <summary>
        /// Checks whether a newly observed teaching context is
        /// sufficiently different to justify a new session.
        /// </summary>
        public bool HasTeachingContextChanged(
            SparkAITeachingDecision newDecision)
        {
            if (!newDecision.IsValid)
                return false;

            string newKey =
                BuildTopicKey(
                    newDecision);

            if (string.IsNullOrEmpty(activeTopicKey))
                return true;

            return !string.Equals(
                activeTopicKey,
                newKey,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// Starts a new session only when the teaching context
        /// meaningfully changed.
        /// </summary>
        public bool RefreshIfContextChanged()
        {
            if (!initialized)
                return false;

            SparkAITeachingDecision newDecision =
                teachingReasoner.DecideCurrentTeaching();

            if (!newDecision.IsValid)
                return false;

            if (!HasTeachingContextChanged(
                    newDecision))
            {
                return false;
            }

            return Begin(
                newDecision);
        }

        /// <summary>
        /// Gets the text appropriate for the current teaching state.
        /// </summary>
        public string GetCurrentTeachingText()
        {
            if (!decision.IsValid)
                return string.Empty;

            switch (state)
            {
                case SparkAITeachingSessionState.Question:

                    return decision.Question;

                case SparkAITeachingSessionState.Hint:

                    return decision.Hint;

                case SparkAITeachingSessionState.StrongHint:

                    return decision.StrongHint;

                case SparkAITeachingSessionState.Explanation:

                    return decision.Explanation;

                case SparkAITeachingSessionState.Understood:

                    return
                        "Good. You understood the concept.";

                case SparkAITeachingSessionState.Completed:

                    return
                        "Teaching step completed.";

                default:

                    return decision.Message;
            }
        }

        /// <summary>
        /// Gets the current teaching objective.
        /// </summary>
        public string GetCurrentObjective()
        {
            if (!decision.IsValid)
                return string.Empty;

            return decision.Objective;
        }

        /// <summary>
        /// Gets the concept IDs associated with the current teaching
        /// interaction.
        /// </summary>
        public string[] GetCurrentConceptIds()
        {
            if (!decision.IsValid ||
                decision.ConceptIds == null)
            {
                return Array.Empty<string>();
            }

            return decision.ConceptIds;
        }

        private void TryAutomaticEscalation()
        {
            switch (state)
            {
                case SparkAITeachingSessionState.Question:

                    RequestHint();

                    break;

                case SparkAITeachingSessionState.Hint:

                    if (hintCount >= maximumHints)
                    {
                        RequestStrongHint();
                    }
                    else
                    {
                        RequestHint();
                    }

                    break;

                case SparkAITeachingSessionState.StrongHint:

                    RequestExplanation();

                    break;

                case SparkAITeachingSessionState.Explanation:

                    Complete();

                    break;
            }
        }

        private string BuildTopicKey(
            SparkAITeachingDecision teachingDecision)
        {
            return
                $"{teachingDecision.Topic}|" +
                $"{teachingDecision.Intensity}|" +
                $"{teachingDecision.Objective}";
        }

        private void SetState(
            SparkAITeachingSessionState newState)
        {
            if (state == newState)
                return;

            state =
                newState;

            stateStartedAt =
                Time.time;

            StateChanged?.Invoke(
                state);
        }

        private void RaiseTeachingChanged()
        {
            TeachingChanged?.Invoke(
                decision);
        }
    }

    public enum SparkAITeachingSessionState
    {
        Idle = 0,

        Question = 1,

        Hint = 2,

        StrongHint = 3,

        Explanation = 4,

        Understood = 5,

        Completed = 6,

        Cancelled = 7
    }
}