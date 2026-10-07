using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Adaptive learning state for Project Spark.
    ///
    /// Tracks the player's demonstrated understanding of concepts.
    ///
    /// This system does NOT:
    /// - Change level objectives.
    /// - Modify the circuit.
    /// - Modify the solver.
    /// - Automatically complete levels.
    /// - Generate dialogue.
    ///
    /// It only maintains educational progress signals for Spark AI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearning : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField] private SparkAIKnowledge knowledge;
        [SerializeField] private SparkAIIntent intent;
        [SerializeField] private SparkAIBrain brain;

        [Header("Learning Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float successIncrease = 0.12f;

        [SerializeField]
        [Range(0f, 1f)]
        private float struggleDecrease = 0.10f;

        [SerializeField]
        [Range(0f, 1f)]
        private float repeatedMistakeDecrease = 0.05f;

        [SerializeField]
        [Range(0f, 1f)]
        private float masteryThreshold = 0.80f;

        private readonly List<SparkAILearningEntry> entries =
            new List<SparkAILearningEntry>(32);

        private readonly Dictionary<string, int> indexByConceptId =
            new Dictionary<string, int>(StringComparer.Ordinal);

        private bool initialized;

        public bool IsInitialized => initialized;

        public int TrackedConceptCount => entries.Count;

        public event EventHandler<SparkAILearningChangedEventArgs>
            LearningChanged;

        private void Awake()
        {
            ResolveReferences();

            if (knowledge == null)
            {
                Debug.LogError(
                    "[Spark AI Learning] SparkAIKnowledge reference is missing.",
                    this);

                return;
            }

            if (intent == null)
            {
                Debug.LogError(
                    "[Spark AI Learning] SparkAIIntent reference is missing.",
                    this);

                return;
            }

            if (brain == null)
            {
                Debug.LogError(
                    "[Spark AI Learning] SparkAIBrain reference is missing.",
                    this);

                return;
            }

            intent.IntentChanged -= HandleIntentChanged;
            intent.IntentChanged += HandleIntentChanged;

            initialized = true;
        }

        private void OnDestroy()
        {
            if (intent != null)
                intent.IntentChanged -= HandleIntentChanged;
        }

        private void ResolveReferences()
        {
            // Explicit references only.
            //
            // No FindObjectOfType.
            // No FindFirstObjectByType.
        }

        /// <summary>
        /// Returns the current learning state for a concept.
        /// </summary>
        public bool TryGetLearning(
            string conceptId,
            out SparkAILearningEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(conceptId) &&
                indexByConceptId.TryGetValue(
                    conceptId,
                    out int index))
            {
                entry = entries[index];
                return true;
            }

            entry = default;
            return false;
        }

        /// <summary>
        /// Returns the current mastery value for a concept.
        /// </summary>
        public float GetMastery(string conceptId)
        {
            return TryGetLearning(
                       conceptId,
                       out SparkAILearningEntry entry)
                ? entry.Mastery
                : 0f;
        }

        /// <summary>
        /// True when the player's demonstrated mastery
        /// reaches the configured threshold.
        /// </summary>
        public bool IsMastered(string conceptId)
        {
            return GetMastery(conceptId) >= masteryThreshold;
        }

        /// <summary>
        /// Explicitly records successful application of a concept.
        ///
        /// This should be called by authoritative gameplay/level
        /// evaluation when the player demonstrably applies a concept.
        /// </summary>
        public void RecordSuccess(
            string conceptId,
            string reason = null)
        {
            if (!TryGetOrCreate(
                    conceptId,
                    out SparkAILearningEntry entry,
                    out int index))
            {
                return;
            }

            float newMastery =
                Mathf.Clamp01(
                    entry.Mastery + successIncrease);

            entry =
                entry.WithSuccess(
                    newMastery,
                    reason);

            entries[index] = entry;

            RaiseChanged(entry);
        }

        /// <summary>
        /// Records a demonstrated struggle with a concept.
        /// </summary>
        public void RecordStruggle(
            string conceptId,
            string reason = null)
        {
            if (!TryGetOrCreate(
                    conceptId,
                    out SparkAILearningEntry entry,
                    out int index))
            {
                return;
            }

            float newMastery =
                Mathf.Clamp01(
                    entry.Mastery - struggleDecrease);

            entry =
                entry.WithStruggle(
                    newMastery,
                    reason);

            entries[index] = entry;

            RaiseChanged(entry);
        }

        /// <summary>
        /// Records a repeated mistake.
        ///
        /// This is intentionally a smaller penalty than a normal
        /// struggle so one mistake does not destroy learning progress.
        /// </summary>
        public void RecordRepeatedMistake(
            string conceptId,
            string reason = null)
        {
            if (!TryGetOrCreate(
                    conceptId,
                    out SparkAILearningEntry entry,
                    out int index))
            {
                return;
            }

            float newMastery =
                Mathf.Clamp01(
                    entry.Mastery - repeatedMistakeDecrease);

            entry =
                entry.WithRepeatedMistake(
                    newMastery,
                    reason);

            entries[index] = entry;

            RaiseChanged(entry);
        }

        /// <summary>
        /// Returns a reusable copy of all learning states.
        /// </summary>
        public void CopyLearning(
            List<SparkAILearningEntry> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            for (int i = 0; i < entries.Count; i++)
                results.Add(entries[i]);
        }

        private void HandleIntentChanged(
            object sender,
            SparkAIIntentChangedEventArgs args)
        {
            if (!args.Intent.IsValid)
                return;

            /*
             * Intent alone does NOT prove learning success or failure.
             *
             * For example:
             *
             * "TryingToCompleteObjective"
             *
             * does not mean the player understands the concept.
             *
             * Actual success/struggle should eventually be supplied
             * by level evaluation and deterministic gameplay evidence.
             *
             * Therefore this callback currently remains observational.
             */
        }

        private bool TryGetOrCreate(
            string conceptId,
            out SparkAILearningEntry entry,
            out int index)
        {
            entry = default;
            index = -1;

            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            if (!knowledge.Contains(conceptId))
            {
                Debug.LogWarning(
                    $"[Spark AI Learning] Unknown knowledge concept: {conceptId}",
                    this);

                return false;
            }

            if (indexByConceptId.TryGetValue(
                    conceptId,
                    out index))
            {
                entry = entries[index];
                return true;
            }

            entry =
                SparkAILearningEntry.Create(
                    conceptId,
                    Time.time);

            index = entries.Count;

            entries.Add(entry);
            indexByConceptId.Add(
                conceptId,
                index);

            return true;
        }

        private void RaiseChanged(
            SparkAILearningEntry entry)
        {
            LearningChanged?.Invoke(
                this,
                new SparkAILearningChangedEventArgs(entry));
        }
    }

    /// <summary>
    /// Player's demonstrated learning state for one concept.
    /// </summary>
    [Serializable]
    public readonly struct SparkAILearningEntry
    {
        public string ConceptId { get; }

        public float Mastery { get; }

        public int SuccessCount { get; }

        public int StruggleCount { get; }

        public int RepeatedMistakeCount { get; }

        public float LastUpdatedTime { get; }

        public string LastReason { get; }

        public SparkAILearningEntry(
            string conceptId,
            float mastery,
            int successCount,
            int struggleCount,
            int repeatedMistakeCount,
            float lastUpdatedTime,
            string lastReason)
        {
            ConceptId = conceptId ?? string.Empty;
            Mastery = Mathf.Clamp01(mastery);
            SuccessCount = Mathf.Max(0, successCount);
            StruggleCount = Mathf.Max(0, struggleCount);
            RepeatedMistakeCount = Mathf.Max(
                0,
                repeatedMistakeCount);

            LastUpdatedTime = lastUpdatedTime;
            LastReason = lastReason ?? string.Empty;
        }

        public static SparkAILearningEntry Create(
            string conceptId,
            float time)
        {
            return new SparkAILearningEntry(
                conceptId,
                0f,
                0,
                0,
                0,
                time,
                string.Empty);
        }

        public SparkAILearningEntry WithSuccess(
            float mastery,
            string reason)
        {
            return new SparkAILearningEntry(
                ConceptId,
                mastery,
                SuccessCount + 1,
                StruggleCount,
                RepeatedMistakeCount,
                Time.time,
                reason);
        }

        public SparkAILearningEntry WithStruggle(
            float mastery,
            string reason)
        {
            return new SparkAILearningEntry(
                ConceptId,
                mastery,
                SuccessCount,
                StruggleCount + 1,
                RepeatedMistakeCount,
                Time.time,
                reason);
        }

        public SparkAILearningEntry WithRepeatedMistake(
            float mastery,
            string reason)
        {
            return new SparkAILearningEntry(
                ConceptId,
                mastery,
                SuccessCount,
                StruggleCount,
                RepeatedMistakeCount + 1,
                Time.time,
                reason);
        }
    }

    public sealed class SparkAILearningChangedEventArgs : EventArgs
    {
        public SparkAILearningEntry Learning { get; }

        public SparkAILearningChangedEventArgs(
            SparkAILearningEntry learning)
        {
            Learning = learning;
        }
    }
}