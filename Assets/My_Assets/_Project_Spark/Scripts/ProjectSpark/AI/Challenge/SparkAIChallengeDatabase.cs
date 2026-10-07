using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Persistent database of Project Spark educational challenges.
    ///
    /// Responsibilities:
    /// - Store challenge definitions.
    /// - Build a fast ID lookup.
    /// - Resolve challenge IDs used by lessons.
    ///
    /// Does NOT:
    /// - Evaluate circuits.
    /// - Modify gameplay.
    /// - Start challenges.
    /// - Decide whether a player succeeded.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkAIChallengeDatabase",
        menuName = "Project Spark/AI/Challenge Database")]
    public sealed class SparkAIChallengeDatabase : ScriptableObject
    {
        [Header("Database")]
        [SerializeField] private string databaseId =
            "project_spark_challenges";

        [SerializeField] private List<SparkAIChallengeDefinition> challenges =
            new List<SparkAIChallengeDefinition>();

        private readonly Dictionary<string, SparkAIChallengeDefinition> challengeIndex =
            new Dictionary<string, SparkAIChallengeDefinition>(
                StringComparer.OrdinalIgnoreCase);

        private bool initialized;

        public string DatabaseId =>
            databaseId;

        public int ChallengeCount =>
            challenges != null ? challenges.Count : 0;

        public bool IsInitialized =>
            initialized;

        /// <summary>
        /// Builds the runtime lookup table.
        /// Safe to call more than once.
        /// </summary>
        public void Initialize()
        {
            BuildIndex();
        }

        /// <summary>
        /// Rebuilds the challenge ID lookup.
        /// </summary>
        public void BuildIndex()
        {
            challengeIndex.Clear();

            if (challenges == null)
            {
                initialized = true;
                return;
            }

            for (int i = 0; i < challenges.Count; i++)
            {
                SparkAIChallengeDefinition challenge =
                    challenges[i];

                if (!challenge.IsValid)
                    continue;

                string id = challenge.Id;

                if (challengeIndex.ContainsKey(id))
                {
                    Debug.LogWarning(
                        $"[AI CHALLENGE DATABASE] Duplicate challenge ID '{id}'. " +
                        $"The first definition will be kept.",
                        this);

                    continue;
                }

                challengeIndex.Add(id, challenge);
            }

            initialized = true;
        }

        /// <summary>
        /// Finds a challenge definition by ID.
        /// </summary>
        public bool TryGetChallenge(
            string challengeId,
            out SparkAIChallengeDefinition challenge)
        {
            challenge = default;

            if (!initialized)
                BuildIndex();

            if (string.IsNullOrWhiteSpace(challengeId))
                return false;

            return challengeIndex.TryGetValue(
                challengeId.Trim(),
                out challenge);
        }

        /// <summary>
        /// Checks whether a challenge exists.
        /// </summary>
        public bool ContainsChallenge(string challengeId)
        {
            if (!initialized)
                BuildIndex();

            if (string.IsNullOrWhiteSpace(challengeId))
                return false;

            return challengeIndex.ContainsKey(
                challengeId.Trim());
        }

        /// <summary>
        /// Copies all valid challenge definitions into the supplied list.
        /// </summary>
        public void CopyAllChallenges(
            List<SparkAIChallengeDefinition> results)
        {
            if (results == null)
                return;

            results.Clear();

            if (challenges == null)
                return;

            for (int i = 0; i < challenges.Count; i++)
            {
                SparkAIChallengeDefinition challenge =
                    challenges[i];

                if (!challenge.IsValid)
                    continue;

                results.Add(challenge);
            }
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            databaseId =
                databaseId != null
                    ? databaseId.Trim()
                    : string.Empty;

            if (challenges == null)
                challenges =
                    new List<SparkAIChallengeDefinition>();

            ValidateDuplicateIds();
        }

        private void ValidateDuplicateIds()
        {
            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < challenges.Count; i++)
            {
                SparkAIChallengeDefinition challenge =
                    challenges[i];

                if (!challenge.IsValid)
                    continue;

                if (!ids.Add(challenge.Id))
                {
                    Debug.LogWarning(
                        $"[AI CHALLENGE DATABASE] Duplicate challenge ID " +
                        $"'{challenge.Id}' at index {i}.",
                        this);
                }
            }
        }

#endif
    }
}