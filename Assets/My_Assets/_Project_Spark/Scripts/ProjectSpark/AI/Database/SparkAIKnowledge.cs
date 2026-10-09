using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Project Spark's stable electronics knowledge database.
    ///
    /// This is NOT live circuit state.
    /// It contains reusable knowledge such as:
    /// - Definitions
    /// - Explanations
    /// - Examples
    /// - Important facts
    /// - Common misconceptions
    /// - Safety information
    /// - Aliases used by the AI question system
    ///
    /// Runtime world information belongs to SparkAIWorld.
    /// Teaching-specific progression belongs to SparkAIConceptDatabase.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkAIKnowledge",
        menuName = "Project Spark/AI/Knowledge Database")]
    public sealed class SparkAIKnowledge : ScriptableObject
    {
        // ============================================================
        // ENTRY
        // ============================================================

        [Serializable]
        public sealed class Entry
        {
            // --------------------------------------------------------
            // Identity
            // --------------------------------------------------------

            [Header("Identity")]

            [Tooltip("Unique ID used by the AI knowledge system.")]
            [SerializeField]
            private string id;

            [Tooltip("Human-readable knowledge title.")]
            [SerializeField]
            private string title;

            [Tooltip("General category of this knowledge entry.")]
            [SerializeField]
            private string category;

            [TextArea(3, 10)]
            [Tooltip("Practical instructions for measuring this concept.")]
            [SerializeField]
            private string measurement;

            // --------------------------------------------------------
            // Search
            // --------------------------------------------------------

            [Header("Search")]

            [Tooltip(
                "Alternative words or phrases the player may use " +
                "when asking about this entry.")]
            [SerializeField]
            private List<string> aliases =
                new List<string>();

            // --------------------------------------------------------
            // Core Knowledge
            // --------------------------------------------------------

            [Header("Core Knowledge")]

            [TextArea(2, 6)]
            [Tooltip("Short definition of the concept or component.")]
            [SerializeField]
            private string definition;

            [TextArea(3, 10)]
            [Tooltip("Detailed explanation used when the player asks how or why.")]
            [SerializeField]
            private string explanation;

            [TextArea(2, 8)]
            [Tooltip("Practical or real-world example.")]
            [SerializeField]
            private string example;

            // --------------------------------------------------------
            // Important Facts
            // --------------------------------------------------------

            [Header("Core Facts")]

            [Tooltip(
                "Short factual statements the AI can use when reasoning " +
                "about this concept or component.")]
            [SerializeField]
            private List<string> importantFacts =
                new List<string>();

            // --------------------------------------------------------
            // Misconception
            // --------------------------------------------------------

            [Header("Misconception")]

            [TextArea(2, 8)]
            [Tooltip(
                "Common incorrect idea the player may have " +
                "about this concept.")]
            [SerializeField]
            private string misconception;

             // --------------------------------------------------------
            // Safety
            // --------------------------------------------------------

            [Header("Safety")]

            [TextArea(2, 8)]
            [Tooltip(
                "Safety guidance associated with this concept " +
                "or component.")]
            [SerializeField]
            private string safety;

            // --------------------------------------------------------
            // Related Knowledge
            // --------------------------------------------------------

            [Header("Related Knowledge")]

            [Tooltip(
                "IDs of other knowledge entries that are useful when " +
                "reasoning about this entry.")]
            [SerializeField]
            private List<string> relatedEntryIds =
                new List<string>();

           

            // --------------------------------------------------------
            // Public Properties
            // --------------------------------------------------------

            public string Id =>
                id;

            public string Title =>
                title;

            public string Category =>
                category;

            public IReadOnlyList<string> Aliases =>
                aliases;

            public string Definition =>
                definition;

            public string Explanation =>
                explanation;

            public string Example =>
                example;
            public IReadOnlyList<string> ImportantFacts =>
                importantFacts;

            public IReadOnlyList<string> RelatedEntryIds =>
                relatedEntryIds;
            public string Misconception =>
                misconception;

            public string Safety =>
                safety;

            public bool IsValid =>
                !string.IsNullOrWhiteSpace(id) &&
                !string.IsNullOrWhiteSpace(title);

            public int ImportantFactCount =>
                importantFacts != null
                    ? importantFacts.Count
                    : 0;

            public string Measurement => measurement;
        }

        // ============================================================
        // DATABASE
        // ============================================================

        [Header("Database")]

        [Tooltip("Unique identifier for this knowledge database.")]
        [SerializeField]
        private string databaseId = "project_spark_knowledge";

        [Tooltip(
            "Stable electronics knowledge entries used by Project Spark AI.")]
        [SerializeField]
        private List<Entry> entries =
            new List<Entry>();

        // ============================================================
        // RUNTIME INDEX
        // ============================================================

        private readonly Dictionary<string, Entry> entryLookup =
            new Dictionary<string, Entry>(
                StringComparer.OrdinalIgnoreCase);

        private bool initialized;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public string DatabaseId =>
            databaseId;

        public int EntryCount =>
            entries != null
                ? entries.Count
                : 0;

        public IReadOnlyList<Entry> Entries =>
            entries;

        // ============================================================
        // UNITY
        // ============================================================

        private void OnEnable()
        {
            initialized = false;
            Initialize();
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        public void Initialize()
        {
            if (initialized)
                return;

            BuildIndex();

            initialized = true;
        }

        private void EnsureInitialized()
        {
            if (!initialized)
                Initialize();
        }

        public void BuildIndex()
        {
            entryLookup.Clear();

            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (entry == null)
                    continue;

                if (!entry.IsValid)
                    continue;

                if (entryLookup.ContainsKey(entry.Id))
                {
                    Debug.LogWarning(
                        "[SPARK AI KNOWLEDGE] Duplicate entry ID: " +
                        entry.Id,
                        this);

                    continue;
                }

                entryLookup.Add(
                    entry.Id,
                    entry);
            }
        }

                public string GetMeasurement(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Measurement;
        }

        // ============================================================
        // ENTRY LOOKUP
        // ============================================================

        public bool TryGetEntry(
            string id,
            out Entry entry)
        {
            entry = null;

            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return entryLookup.TryGetValue(
                id.Trim(),
                out entry);
        }

        public bool Contains(string id)
        {
            Entry entry;

            return TryGetEntry(
                id,
                out entry);
        }

        // ============================================================
        // KNOWLEDGE ACCESS
        // ============================================================

        public string GetDefinition(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Definition;
        }

        public string GetExplanation(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Explanation;
        }

        public string GetExample(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Example;
        }

        public string GetMisconception(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Misconception;
        }

        public string GetSafety(string id)
        {
            Entry entry;

            if (!TryGetEntry(id, out entry))
                return string.Empty;

            return entry.Safety;
        }

        public bool TryGetImportantFacts(
            string id,
            out IReadOnlyList<string> facts)
        {
            facts = null;

            Entry entry;

            if (!TryGetEntry(id, out entry))
                return false;

            facts = entry.ImportantFacts;

            return facts != null &&
                   facts.Count > 0;
        }

        // ============================================================
        // TITLE SEARCH
        // ============================================================

        public bool TryFindByTitle(
            string title,
            out Entry result)
        {
            result = null;

            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(title))
                return false;

            string normalized =
                Normalize(title);

            if (string.IsNullOrEmpty(normalized))
                return false;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (entry == null)
                    continue;

                if (Normalize(entry.Title) == normalized)
                {
                    result = entry;
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // QUESTION SEARCH
        // ============================================================

        /// <summary>
        /// Finds the most relevant knowledge entry for a player question.
        ///
        /// Search priority:
        /// 1. Exact ID
        /// 2. Exact title
        /// 3. Title phrase contained in question
        /// 4. Alias phrase contained in question
        ///
        /// Longer phrases receive higher scores so that:
        /// "short circuit"
        /// beats:
        /// "circuit"
        /// </summary>
        

        public bool TryFindEntryForQuestion(
    string question,
    out Entry result)
{
    result = null;

    EnsureInitialized();

    if (string.IsNullOrWhiteSpace(question))
        return false;

    string normalizedQuestion =
        Normalize(question);

    if (string.IsNullOrEmpty(normalizedQuestion))
        return false;

    // ============================================================
    // 1. EXACT ID
    // ============================================================

    Entry exactEntry;

    if (entryLookup.TryGetValue(
            normalizedQuestion,
            out exactEntry))
    {
        result = exactEntry;
        return true;
    }

    // ============================================================
    // 2. EXACT TITLE
    // ============================================================

    for (int i = 0; i < entries.Count; i++)
    {
        Entry entry = entries[i];

        if (entry == null)
            continue;

        string title =
            Normalize(entry.Title);

        if (normalizedQuestion == title)
        {
            result = entry;
            return true;
        }
    }

    // ============================================================
    // 3. SCORE ALL POSSIBLE ENTRIES
    // ============================================================

    Entry bestEntry = null;
    int bestScore = 0;

    for (int i = 0; i < entries.Count; i++)
    {
        Entry entry = entries[i];

        if (entry == null || !entry.IsValid)
            continue;

        int score = 0;

        // --------------------------------------------------------
        // TITLE
        // --------------------------------------------------------

        string title =
            Normalize(entry.Title);

        if (!string.IsNullOrEmpty(title) &&
            ContainsPhrase(
                normalizedQuestion,
                title))
        {
            score += CalculatePhraseScore(
                title,
                1000);
        }

        // --------------------------------------------------------
        // ID
        // --------------------------------------------------------

        string id =
            Normalize(entry.Id);

        if (!string.IsNullOrEmpty(id) &&
            ContainsPhrase(
                normalizedQuestion,
                id))
        {
            score += CalculatePhraseScore(
                id,
                900);
        }

        // --------------------------------------------------------
        // ALIASES
        // --------------------------------------------------------

        IReadOnlyList<string> aliases =
            entry.Aliases;

        if (aliases != null)
        {
            for (int aliasIndex = 0;
                 aliasIndex < aliases.Count;
                 aliasIndex++)
            {
                string alias =
                    Normalize(aliases[aliasIndex]);

                if (string.IsNullOrEmpty(alias))
                    continue;

                if (!ContainsPhrase(
                        normalizedQuestion,
                        alias))
                {
                    continue;
                }

                score += CalculatePhraseScore(
                    alias,
                    800);
            }
        }

        // --------------------------------------------------------
        // EXACT WORD MATCH BONUS
        // --------------------------------------------------------

        if (ContainsExactWord(
                normalizedQuestion,
                title))
        {
            score += 250;
        }

        // --------------------------------------------------------
        // SPECIFICITY BONUS
        //
        // Multi-word concepts should beat generic one-word
        // concepts when both are present.
        // --------------------------------------------------------

        int titleWordCount =
            CountWords(title);

        if (titleWordCount >= 2)
        {
            score +=
                titleWordCount * 150;
        }

        // --------------------------------------------------------
        // LONGER PHRASE BONUS
        // --------------------------------------------------------

        score +=
            Mathf.Min(
                title.Length,
                40);

        // --------------------------------------------------------
        // BEST MATCH
        // --------------------------------------------------------

        if (score > bestScore)
        {
            bestScore = score;
            bestEntry = entry;
        }
    }

    if (bestEntry == null)
        return false;

    result = bestEntry;
    return true;
}

private static int CalculatePhraseScore(
    string phrase,
    int baseScore)
{
    if (string.IsNullOrWhiteSpace(phrase))
        return 0;

    int wordCount =
        CountWords(phrase);

    int lengthBonus =
        Mathf.Min(
            phrase.Length,
            50);

    int wordBonus =
        wordCount * 100;

    return
        baseScore +
        lengthBonus +
        wordBonus;
}

private static bool ContainsExactWord(
    string text,
    string word)
{
    if (string.IsNullOrWhiteSpace(text) ||
        string.IsNullOrWhiteSpace(word))
    {
        return false;
    }

    string paddedText =
        " " + text + " ";

    string paddedWord =
        " " + word + " ";

    return paddedText.IndexOf(
               paddedWord,
               StringComparison.OrdinalIgnoreCase) >= 0;
}
private static int CountWords(
    string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return 0;

    string[] words =
        value.Split(
            new[]
            {
                ' ',
                '\t',
                '\r',
                '\n'
            },
            StringSplitOptions.RemoveEmptyEntries);

    return words.Length;
}

        // ============================================================
        // COPY
        // ============================================================

        public IReadOnlyList<Entry> CopyAllEntries()
        {
            EnsureInitialized();

            return new List<Entry>(entries);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static bool ContainsPhrase(
            string text,
            string phrase)
        {
            if (string.IsNullOrEmpty(text) ||
                string.IsNullOrEmpty(phrase))
            {
                return false;
            }

            return text.IndexOf(
                       phrase,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

       private static string Normalize(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return string.Empty;

    return value
        .Trim()
        .ToLowerInvariant()
        .Replace('\u2018', '\'')
        .Replace('\u2019', '\'')
        .Replace('\u201C', '"')
        .Replace('\u201D', '"');
}
        public bool TryGetRelatedEntries(
    string id,
    out IReadOnlyList<Entry> relatedEntries)
{
    relatedEntries = null;

    Entry entry;

    if (!TryGetEntry(
            id,
            out entry))
    {
        return false;
    }

    IReadOnlyList<string> relatedIds =
        entry.RelatedEntryIds;

    if (relatedIds == null ||
        relatedIds.Count == 0)
    {
        return false;
    }

    List<Entry> results =
        new List<Entry>();

    for (int i = 0;
         i < relatedIds.Count;
         i++)
    {
        string relatedId =
            relatedIds[i];

        if (string.IsNullOrWhiteSpace(
                relatedId))
        {
            continue;
        }

        Entry relatedEntry;

        if (TryGetEntry(
                relatedId,
                out relatedEntry))
        {
            results.Add(
                relatedEntry);
        }
    }

    if (results.Count == 0)
        return false;

    relatedEntries = results;

    return true;
}


        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (entries == null)
                return;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.Id))
                    continue;

                if (!ids.Add(entry.Id.Trim()))
                {
                    Debug.LogWarning(
                        "[SPARK AI KNOWLEDGE] Duplicate ID: " +
                        entry.Id,
                        this);
                }
            }

            initialized = false;
        }

#endif
    }
}