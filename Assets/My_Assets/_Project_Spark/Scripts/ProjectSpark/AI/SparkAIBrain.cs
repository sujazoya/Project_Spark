using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Central coordinator for Project Spark AI.
    ///
    /// Responsibilities:
    /// - Observes SparkAIWorld.
    /// - Requests deterministic reasoning from SparkAIReasoner.
    /// - Exposes the latest world snapshot.
    /// - Exposes the latest reasoning result.
    /// - Notifies future AI systems when the interpreted situation changes.
    ///
    /// The brain does NOT:
    /// - modify electrical state
    /// - modify circuit topology
    /// - modify level state
    /// - control gameplay
    /// - generate voice
    /// - call an LLM
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIBrain : MonoBehaviour
    {
        // =====================================================================
        // REFERENCES
        // =====================================================================

        [Header("AI Systems")]

        [SerializeField]
        private SparkAIWorld world;

        [SerializeField]
        private SparkAIReasoner reasoner;

        // =====================================================================
        // STATE
        // =====================================================================

        private bool initialized;

        private SparkAIWorldSnapshot currentSnapshot;

        private SparkAIReasoningResult currentReasoning;

        // =====================================================================
        // EVENTS
        // =====================================================================

        /// <summary>
        /// Raised whenever the AI receives a new authoritative world state.
        /// </summary>
        public event EventHandler<SparkAIWorldChangedEventArgs>
            WorldObserved;

        /// <summary>
        /// Raised after the current world state has been interpreted.
        /// </summary>
        public event EventHandler<SparkAIReasoningChangedEventArgs>
            ReasoningChanged;

        // =====================================================================
        // PUBLIC STATE
        // =====================================================================

        /// <summary>
        /// Gets the latest authoritative AI world snapshot.
        /// </summary>
        public SparkAIWorldSnapshot CurrentSnapshot =>
            currentSnapshot;

        /// <summary>
        /// Gets the latest deterministic reasoning result.
        /// </summary>
        public SparkAIReasoningResult CurrentReasoning =>
            currentReasoning;

        /// <summary>
        /// Gets whether the brain has been initialized.
        /// </summary>
        public bool IsInitialized =>
            initialized;

        /// <summary>
        /// Gets the observed Project Spark world.
        /// </summary>
        public SparkAIWorld World =>
            world;

        /// <summary>
        /// Gets the deterministic reasoner.
        /// </summary>
        public SparkAIReasoner Reasoner =>
            reasoner;

        // =====================================================================
        // UNITY
        // =====================================================================

        private void Awake()
        {
            ResolveReferences();

            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI] SparkAIBrain requires a " +
                    "SparkAIWorld reference.",
                    this);

                return;
            }

            if (reasoner == null)
            {
                Debug.LogError(
                    "[Spark AI] SparkAIBrain requires a " +
                    "SparkAIReasoner reference.",
                    this);

                return;
            }

            world.WorldChanged -=
                HandleWorldChanged;

            world.WorldChanged +=
                HandleWorldChanged;

            currentSnapshot =
                world.CaptureSnapshot();

            currentReasoning =
                reasoner.Reason(
                    currentSnapshot);

            initialized = true;

            RaiseWorldObserved(
                currentSnapshot);

            RaiseReasoningChanged(
                currentReasoning);
        }

        private void OnDestroy()
        {
            if (world != null)
            {
                world.WorldChanged -=
                    HandleWorldChanged;
            }
        }

        // =====================================================================
        // REFERENCES
        // =====================================================================

        private void ResolveReferences()
        {
            /*
             * Deliberately no FindObjectOfType,
             * FindFirstObjectByType or global lookup.
             *
             * Spark AI dependencies are explicitly wired in the Inspector.
             */
        }

        // =====================================================================
        // PUBLIC API
        // =====================================================================

        /// <summary>
        /// Forces one authoritative observation and deterministic reasoning pass.
        /// </summary>
        public SparkAIReasoningResult ObserveNow()
        {
            if (world == null ||
                reasoner == null)
            {
                currentReasoning =
                    new SparkAIReasoningResult(
                        false,
                        SparkAIReasoningState.Unavailable,
                        "Spark AI systems are unavailable.",
                        "Spark AI systems are unavailable.",
                        string.Empty,
                        false,
                        string.Empty,
                        Time.time);

                RaiseReasoningChanged(
                    currentReasoning);

                return currentReasoning;
            }

            currentSnapshot =
                world.CaptureSnapshot();

            currentReasoning =
                reasoner.Reason(
                    currentSnapshot);

            RaiseWorldObserved(
                currentSnapshot);

            RaiseReasoningChanged(
                currentReasoning);

            return currentReasoning;
        }

        /// <summary>
        /// Returns a human-readable deterministic description of the
        /// current Project Spark situation.
        ///
        /// This is NOT generated by an LLM.
        /// </summary>
        public string GetCurrentSituation()
        {
            if (!initialized)
            {
                return "Spark AI is not initialized.";
            }

            if (!currentReasoning.IsValid)
            {
                return "Spark AI cannot currently determine the situation.";
            }

            return currentReasoning.Summary;
        }

        // =====================================================================
        // WORLD EVENTS
        // =====================================================================

        private void HandleWorldChanged(
            object sender,
            SparkAIWorldChangedEventArgs args)
        {
            if (world == null ||
                reasoner == null)
            {
                return;
            }

            currentSnapshot =
                world.LatestSnapshot;

            currentReasoning =
                reasoner.Reason(
                    currentSnapshot);

            RaiseWorldObserved(
                currentSnapshot);

            RaiseReasoningChanged(
                currentReasoning);
        }

        // =====================================================================
        // EVENTS
        // =====================================================================

        private void RaiseWorldObserved(
            SparkAIWorldSnapshot snapshot)
        {
            WorldObserved?.Invoke(
                this,
                new SparkAIWorldChangedEventArgs(
                    new SparkAIEvent(
                        SparkAIEventType.WorldStructureChanged,
                        snapshot.CapturedAt,
                        message:
                            "Spark AI observed a new authoritative world state.")));
        }

        private void RaiseReasoningChanged(
            SparkAIReasoningResult reasoning)
        {
            ReasoningChanged?.Invoke(
                this,
                new SparkAIReasoningChangedEventArgs(
                    reasoning));
        }
    }

    // =========================================================================
    // REASONING EVENT ARGS
    // =========================================================================

    public sealed class SparkAIReasoningChangedEventArgs :
        EventArgs
    {
        public SparkAIReasoningResult Reasoning { get; }

        public SparkAIReasoningChangedEventArgs(
            SparkAIReasoningResult reasoning)
        {
            Reasoning = reasoning;
        }
    }
}