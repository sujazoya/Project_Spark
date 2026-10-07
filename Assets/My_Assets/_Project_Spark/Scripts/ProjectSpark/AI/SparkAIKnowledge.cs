using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Project Spark's deterministic educational knowledge base.
    ///
    /// Responsibilities:
    /// - Stores reusable electronics concepts.
    /// - Provides concept lookup by ID.
    /// - Provides category-based knowledge access.
    /// - Stores prerequisites for future adaptive learning.
    ///
    /// This class does NOT:
    /// - Observe the circuit.
    /// - Modify electrical state.
    /// - Make gameplay decisions.
    /// - Replace the electrical solver.
    /// - Generate dialogue.
    /// - Call an LLM.
    ///
    /// Live circuit facts belong to SparkAIWorld.
    /// Previous observations belong to SparkAIMemory.
    /// Educational facts belong here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIKnowledge : MonoBehaviour
    {
        [Header("Knowledge Database")]
        [SerializeField]
        private SparkAIKnowledgeEntry[] entries = Array.Empty<SparkAIKnowledgeEntry>();

        private readonly Dictionary<string, SparkAIKnowledgeEntry> entryById =
            new Dictionary<string, SparkAIKnowledgeEntry>(StringComparer.Ordinal);

        private bool initialized;

        public bool IsInitialized => initialized;

        public int Count => entries != null ? entries.Length : 0;

        private void Awake()
        {
            BuildIndex();
            initialized = true;
        }

        /// <summary>
        /// Rebuilds the runtime lookup index.
        /// Call this only when the knowledge database changes.
        /// </summary>
        public void BuildIndex()
        {
            entryById.Clear();

            if (entries == null)
                return;

            for (int i = 0; i < entries.Length; i++)
            {
                SparkAIKnowledgeEntry entry = entries[i];

                if (!entry.IsValid)
                    continue;

                if (entryById.ContainsKey(entry.Id))
                {
                    Debug.LogWarning(
                        $"[Spark AI Knowledge] Duplicate knowledge ID ignored: {entry.Id}",
                        this);

                    continue;
                }

                entryById.Add(entry.Id, entry);
            }
        }

        /// <summary>
        /// Finds a knowledge entry by its stable ID.
        /// </summary>
        public bool TryGetConcept(
            string conceptId,
            out SparkAIKnowledgeEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(conceptId))
            {
                return entryById.TryGetValue(conceptId, out entry);
            }

            entry = default;
            return false;
        }

        /// <summary>
        /// Returns all knowledge entries.
        /// </summary>
        public IReadOnlyList<SparkAIKnowledgeEntry> GetAllKnowledge()
        {
            return entries;
        }

        /// <summary>
        /// Returns all concepts belonging to a category.
        /// </summary>
        public void GetByCategory(
            SparkAIKnowledgeCategory category,
            List<SparkAIKnowledgeEntry> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (entries == null)
                return;

            for (int i = 0; i < entries.Length; i++)
            {
                SparkAIKnowledgeEntry entry = entries[i];

                if (!entry.IsValid)
                    continue;

                if (entry.Category == category)
                    results.Add(entry);
            }
        }

        /// <summary>
        /// Checks whether a concept exists.
        /// </summary>
        public bool Contains(string conceptId)
        {
            return !string.IsNullOrWhiteSpace(conceptId)
                   && entryById.ContainsKey(conceptId);
        }
    }

    /// <summary>
    /// Broad educational knowledge categories.
    ///
    /// These categories are intentionally aligned with
    /// Project Spark's basic-to-advanced learning progression.
    /// </summary>
    public enum SparkAIKnowledgeCategory
    {
        ElectricityBasics = 0,
        Voltage = 1,
        Current = 2,
        Resistance = 3,
        Power = 4,
        Circuits = 5,
        Components = 6,
        Measurement = 7,
        Polarity = 8,
        Troubleshooting = 9,
        Safety = 10,
        AdvancedElectronics = 11
    }

    /// <summary>
    /// One reusable educational concept.
    /// </summary>
    [Serializable]
    public struct SparkAIKnowledgeEntry
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string title;

        [TextArea(2, 5)]
        [SerializeField]
        private string explanation;

        [SerializeField]
        private SparkAIKnowledgeCategory category;

        [SerializeField]
        private string[] keyFacts;

        [SerializeField]
        private string[] prerequisiteConceptIds;

        public string Id => id;
        public string Title => title;
        public string Explanation => explanation;
        public SparkAIKnowledgeCategory Category => category;
        public string[] KeyFacts => keyFacts;
        public string[] PrerequisiteConceptIds => prerequisiteConceptIds;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id) &&
            !string.IsNullOrWhiteSpace(title);
    }
}