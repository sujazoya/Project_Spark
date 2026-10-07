using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Chooses the next educational concept to reinforce or teach.
    ///
    /// Responsibilities:
    /// - Check concept readiness.
    /// - Find missing prerequisites.
    /// - Choose the weakest prerequisite.
    ///
    /// Does NOT:
    /// - Change learner mastery.
    /// - Start lessons.
    /// - Start challenges.
    /// - Modify gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIConceptProgressionPlanner : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SparkAIConceptProgression progression;

        [SerializeField] private SparkAIConceptDatabase conceptDatabase;

        [SerializeField] private SparkAILearnerModel learnerModel;

        [Header("Planning")]
        [SerializeField] private int maximumPrerequisiteDepth = 8;

        private readonly List<string> missingPrerequisites =
            new List<string>();

        private readonly HashSet<string> visitedConcepts =
            new HashSet<string>();

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public void Initialize()
        {
            initialized = false;

            if (progression == null)
            {
                Debug.LogWarning(
                    "[AI CONCEPT PROGRESSION PLANNER] " +
                    "Progression reference is missing.",
                    this);

                return;
            }

            if (conceptDatabase == null)
            {
                Debug.LogWarning(
                    "[AI CONCEPT PROGRESSION PLANNER] " +
                    "Concept database reference is missing.",
                    this);

                return;
            }

            if (learnerModel == null)
            {
                Debug.LogWarning(
                    "[AI CONCEPT PROGRESSION PLANNER] " +
                    "Learner model reference is missing.",
                    this);

                return;
            }

            progression.Initialize();
            conceptDatabase.Initialize();

            maximumPrerequisiteDepth =
                Mathf.Clamp(
                    maximumPrerequisiteDepth,
                    1,
                    32);

            initialized = true;
        }

        /// <summary>
        /// Determines the best concept to teach next.
        ///
        /// If the requested concept is ready, the requested
        /// concept itself is returned.
        ///
        /// If it is blocked, the weakest missing prerequisite
        /// is returned.
        /// </summary>
        public bool TryGetNextConcept(
            string requestedConceptId,
            out SparkAIConceptDefinition nextConcept)
        {
            nextConcept = default;

            if (!EnsureInitialized())
                return false;

            if (string.IsNullOrWhiteSpace(requestedConceptId))
                return false;

            if (!conceptDatabase.TryGetConcept(
                requestedConceptId,
                out SparkAIConceptDefinition requestedConcept))
            {
                return false;
            }

            SparkAIConceptProgressionResult result =
                progression.EvaluateConcept(
                    requestedConceptId);

            if (!result.IsValid)
                return false;

            if (result.IsReady)
            {
                nextConcept = requestedConcept;
                return true;
            }

            return TryFindWeakestPrerequisite(
                requestedConcept,
                out nextConcept);
        }

        /// <summary>
        /// Returns the weakest direct prerequisite that is
        /// currently preventing the requested concept.
        /// </summary>
        private bool TryFindWeakestPrerequisite(
            SparkAIConceptDefinition concept,
            out SparkAIConceptDefinition weakest)
        {
            weakest = default;

            string[] prerequisiteIds =
                concept.PrerequisiteConceptIds;

            if (prerequisiteIds == null ||
                prerequisiteIds.Length == 0)
            {
                return false;
            }

            float weakestMastery =
                float.MaxValue;

            bool found = false;

            for (int i = 0; i < prerequisiteIds.Length; i++)
            {
                string prerequisiteId =
                    prerequisiteIds[i];

                if (string.IsNullOrWhiteSpace(prerequisiteId))
                    continue;

                if (!conceptDatabase.TryGetConcept(
                    prerequisiteId,
                    out SparkAIConceptDefinition prerequisite))
                {
                    continue;
                }

                if (progression.IsConceptReady(
                    prerequisite.Id))
                {
                    continue;
                }

                float mastery =
                    progression.GetConceptMastery(
                        prerequisite.Id);

                if (!found ||
                    mastery < weakestMastery)
                {
                    weakest =
                        prerequisite;

                    weakestMastery =
                        mastery;

                    found = true;
                }
            }

            return found;
        }

        /// <summary>
        /// Finds the deepest currently unresolved prerequisite.
        ///
        /// Example:
        ///
        /// power
        ///   ↓
        /// current
        ///   ↓
        /// voltage
        ///   ↓
        /// circuit_basics
        ///
        /// If circuit_basics is the weakest missing concept,
        /// it will be returned.
        /// </summary>
        public bool TryGetDeepestMissingPrerequisite(
            string requestedConceptId,
            out SparkAIConceptDefinition nextConcept)
        {
            nextConcept = default;

            if (!EnsureInitialized())
                return false;

            if (!conceptDatabase.TryGetConcept(
                requestedConceptId,
                out SparkAIConceptDefinition requestedConcept))
            {
                return false;
            }

            visitedConcepts.Clear();

            return FindDeepestMissingPrerequisite(
                requestedConcept,
                0,
                out nextConcept);
        }

        private bool FindDeepestMissingPrerequisite(
            SparkAIConceptDefinition concept,
            int depth,
            out SparkAIConceptDefinition result)
        {
            result = default;

            if (depth >= maximumPrerequisiteDepth)
                return false;

            if (!visitedConcepts.Add(concept.Id))
                return false;

            string[] prerequisiteIds =
                concept.PrerequisiteConceptIds;

            if (prerequisiteIds == null ||
                prerequisiteIds.Length == 0)
            {
                visitedConcepts.Remove(concept.Id);

                result = concept;

                return true;
            }

            SparkAIConceptDefinition weakest =
                default;

            float weakestMastery =
                float.MaxValue;

            bool foundMissing = false;

            for (int i = 0; i < prerequisiteIds.Length; i++)
            {
                string prerequisiteId =
                    prerequisiteIds[i];

                if (string.IsNullOrWhiteSpace(prerequisiteId))
                    continue;

                if (!conceptDatabase.TryGetConcept(
                    prerequisiteId,
                    out SparkAIConceptDefinition prerequisite))
                {
                    continue;
                }

                float mastery =
                    progression.GetConceptMastery(
                        prerequisite.Id);

                if (progression.IsConceptReady(
                    prerequisite.Id))
                {
                    continue;
                }

                if (!foundMissing ||
                    mastery < weakestMastery)
                {
                    weakest =
                        prerequisite;

                    weakestMastery =
                        mastery;

                    foundMissing = true;
                }
            }

            if (!foundMissing)
            {
                visitedConcepts.Remove(concept.Id);
                return false;
            }

            if (FindDeepestMissingPrerequisite(
                weakest,
                depth + 1,
                out SparkAIConceptDefinition deeper))
            {
                visitedConcepts.Remove(concept.Id);

                result = deeper;

                return true;
            }

            visitedConcepts.Remove(concept.Id);

            result = weakest;

            return true;
        }

        /// <summary>
        /// Gets all currently unresolved direct prerequisites.
        /// </summary>
        public void CopyMissingPrerequisites(
            string conceptId,
            List<string> results)
        {
            if (results == null)
                return;

            results.Clear();

            if (!EnsureInitialized())
                return;

            SparkAIConceptProgressionResult evaluation =
                progression.EvaluateConcept(
                    conceptId);

            if (!evaluation.IsValid)
                return;

            string[] missing =
                evaluation.MissingPrerequisiteIds;

            if (missing == null)
                return;

            for (int i = 0; i < missing.Length; i++)
            {
                results.Add(missing[i]);
            }
        }

        private bool EnsureInitialized()
        {
            if (initialized)
                return true;

            Initialize();

            return initialized;
        }
    }
}