using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Memory layer for Spark AI.
    ///
    /// Stores deterministic observations produced by SparkAIBrain.
    ///
    /// This system:
    /// - does not simulate electricity
    /// - does not modify gameplay
    /// - does not modify circuit topology
    /// - does not modify level state
    /// - does not call an LLM
    ///
    /// Current memory is runtime memory only.
    /// Persistent disk/database memory can be added later.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIMemory : MonoBehaviour
    {
        // =====================================================================
        // CONFIGURATION
        // =====================================================================

        [Header("Memory Settings")]

        [Tooltip(
            "Maximum number of recent AI observations retained in memory.")]
        [SerializeField]
        private int maximumObservationCount = 128;

        // =====================================================================
        // REFERENCES
        // =====================================================================

        [Header("AI Systems")]

        [SerializeField]
        private SparkAIBrain brain;

        // =====================================================================
        // RUNTIME MEMORY
        // =====================================================================

        private readonly List<SparkAIMemoryEntry>
            observations =
                new List<SparkAIMemoryEntry>(128);

        private bool initialized;

        // =====================================================================
        // PUBLIC STATE
        // =====================================================================

        /// <summary>
        /// Number of observations currently stored.
        /// </summary>
        public int ObservationCount =>
            observations.Count;

        /// <summary>
        /// Gets whether memory has been initialized.
        /// </summary>
        public bool IsInitialized =>
            initialized;

        /// <summary>
        /// Gets the most recent memory entry.
        /// </summary>
        public SparkAIMemoryEntry LatestEntry
        {
            get
            {
                if (observations.Count == 0)
                {
                    return default;
                }

                return observations[
                    observations.Count - 1];
            }
        }

        // =====================================================================
        // UNITY
        // =====================================================================

        private void Awake()
        {
            ResolveReferences();

            if (brain == null)
            {
                Debug.LogError(
                    "[Spark AI] SparkAIMemory requires a " +
                    "SparkAIBrain reference.",
                    this);

                return;
            }

            brain.ReasoningChanged -=
                HandleReasoningChanged;

            brain.ReasoningChanged +=
                HandleReasoningChanged;

            initialized = true;

            StoreCurrentReasoning();
        }

        private void OnDestroy()
        {
            if (brain != null)
            {
                brain.ReasoningChanged -=
                    HandleReasoningChanged;
            }
        }

        // =====================================================================
        // REFERENCES
        // =====================================================================

        private void ResolveReferences()
        {
            /*
             * Deliberately no global object lookup.
             *
             * Spark AI dependencies are explicitly assigned
             * through the Inspector.
             */
        }

        // =====================================================================
        // EVENT HANDLING
        // =====================================================================

        private void HandleReasoningChanged(
            object sender,
            SparkAIReasoningChangedEventArgs args)
        {
            if (!args.Reasoning.IsValid)
            {
                return;
            }

            StoreReasoning(
                args.Reasoning);
        }

        // =====================================================================
        // PUBLIC MEMORY API
        // =====================================================================

        /// <summary>
        /// Stores the current reasoning result as a memory entry.
        /// </summary>
        public void StoreCurrentReasoning()
        {
            if (brain == null)
            {
                return;
            }

            SparkAIReasoningResult reasoning =
                brain.CurrentReasoning;

            if (!reasoning.IsValid)
            {
                return;
            }

            StoreReasoning(
                reasoning);
        }

        /// <summary>
        /// Returns a copy of the current memory contents.
        ///
        /// The returned array does not expose the internal list.
        /// </summary>
        public SparkAIMemoryEntry[] GetRecentMemories()
        {
            return observations.ToArray();
        }

        /// <summary>
        /// Returns the most recent memory entries up to the requested count.
        ///
        /// Entries are returned oldest-to-newest within the selected range.
        /// </summary>
        public SparkAIMemoryEntry[] GetRecentMemories(
            int count)
        {
            if (count <= 0 ||
                observations.Count == 0)
            {
                return Array.Empty<SparkAIMemoryEntry>();
            }

            int actualCount =
                Mathf.Min(
                    count,
                    observations.Count);

            SparkAIMemoryEntry[] result =
                new SparkAIMemoryEntry[actualCount];

            int startIndex =
                observations.Count - actualCount;

            for (int i = 0;
                 i < actualCount;
                 i++)
            {
                result[i] =
                    observations[
                        startIndex + i];
            }

            return result;
        }

        /// <summary>
        /// Clears runtime memory.
        ///
        /// This does not modify Project Spark gameplay state.
        /// </summary>
        public void ClearMemory()
        {
            observations.Clear();
        }

        // =====================================================================
        // STORAGE
        // =====================================================================

        private void StoreReasoning(
            SparkAIReasoningResult reasoning)
        {
            if (!reasoning.IsValid)
            {
                return;
            }

            int capacity =
                Mathf.Max(
                    1,
                    maximumObservationCount);

            SparkAIMemoryEntry entry =
                new SparkAIMemoryEntry(
                    reasoning.CapturedAt,
                    reasoning.State,
                    reasoning.Summary,
                    reasoning.Situation,
                    reasoning.LevelName,
                    reasoning.HasSelection,
                    reasoning.SelectedObjectName);

            observations.Add(
                entry);

            while (observations.Count > capacity)
            {
                observations.RemoveAt(0);
            }
        }
    }

    // =========================================================================
    // MEMORY ENTRY
    // =========================================================================

    /// <summary>
    /// Immutable record of something Spark AI understood at a particular time.
    /// </summary>
    public readonly struct SparkAIMemoryEntry
    {
        public float Time { get; }

        public SparkAIReasoningState State { get; }

        public string Summary { get; }

        public string Situation { get; }

        public string LevelName { get; }

        public bool HasSelection { get; }

        public string SelectedObjectName { get; }

        public SparkAIMemoryEntry(
            float time,
            SparkAIReasoningState state,
            string summary,
            string situation,
            string levelName,
            bool hasSelection,
            string selectedObjectName)
        {
            Time = time;
            State = state;
            Summary =
                summary ?? string.Empty;

            Situation =
                situation ?? string.Empty;

            LevelName =
                levelName ?? string.Empty;

            HasSelection =
                hasSelection;

            SelectedObjectName =
                selectedObjectName ?? string.Empty;
        }

        public override string ToString()
        {
            return
                $"[{Time:0.00}s] " +
                $"[{State}] " +
                Summary;
        }
    }
}