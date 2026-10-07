using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Determines whether educational concepts are ready to teach
    /// based on prerequisite concepts and the player's current
    /// learner mastery.
    ///
    /// Responsibilities:
    /// - Check concept prerequisites.
    /// - Check learner mastery.
    /// - Explain why a concept is or is not ready.
    ///
    /// Does NOT:
    /// - Modify learner mastery.
    /// - Start lessons.
    /// - Start challenges.
    /// - Modify gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIConceptProgression : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SparkAIConceptDatabase conceptDatabase;

        [SerializeField] private SparkAILearnerModel learnerModel;

        [Header("Progression")]
        [SerializeField] private float prerequisiteMasteryRequirement = 0.70f;

        [SerializeField] private bool requireAllPrerequisites = true;

        private readonly List<SparkAIConceptDefinition> prerequisites =
            new List<SparkAIConceptDefinition>();

        private readonly List<string> missingPrerequisiteIds =
            new List<string>();

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public float PrerequisiteMasteryRequirement =>
            Mathf.Clamp01(prerequisiteMasteryRequirement);

        public void Initialize()
        {
            initialized = false;

            if (conceptDatabase == null)
            {
                Debug.LogWarning(
                    "[AI CONCEPT PROGRESSION] Concept database reference is missing.",
                    this);

                return;
            }

            if (learnerModel == null)
            {
                Debug.LogWarning(
                    "[AI CONCEPT PROGRESSION] Learner model reference is missing.",
                    this);

                return;
            }

            conceptDatabase.Initialize();

            prerequisiteMasteryRequirement =
                Mathf.Clamp01(prerequisiteMasteryRequirement);

            initialized = true;
        }

        /// <summary>
        /// Checks whether a concept is ready to be taught.
        /// </summary>
        public bool IsConceptReady(
            string conceptId)
        {
            return EvaluateConcept(
                conceptId).IsReady;
        }

        /// <summary>
        /// Evaluates a concept and returns detailed progression information.
        /// </summary>
        public SparkAIConceptProgressionResult EvaluateConcept(
            string conceptId)
        {
            if (!initialized)
                Initialize();

            if (!initialized)
            {
                return SparkAIConceptProgressionResult.Invalid(
                    conceptId);
            }

            if (!conceptDatabase.TryGetConcept(
                conceptId,
                out SparkAIConceptDefinition concept))
            {
                return SparkAIConceptProgressionResult.Invalid(
                    conceptId);
            }

            missingPrerequisiteIds.Clear();

            bool prerequisitesSatisfied =
                EvaluatePrerequisites(
                    concept,
                    missingPrerequisiteIds);

            bool isReady =
                prerequisitesSatisfied;

            string summary;

            if (isReady)
            {
                summary =
                    $"Concept '{concept.DisplayName}' is ready to teach.";
            }
            else
            {
                summary =
                    $"Concept '{concept.DisplayName}' is waiting for " +
                    "prerequisite mastery.";
            }

            return new SparkAIConceptProgressionResult(
                true,
                concept.Id,
                concept.DisplayName,
                isReady,
                prerequisitesSatisfied,
                concept.RequiredForProgression,
                prerequisiteMasteryRequirement,
                missingPrerequisiteIds.ToArray(),
                summary);
        }

        /// <summary>
        /// Checks all prerequisites for a concept.
        /// </summary>
        private bool EvaluatePrerequisites(
            SparkAIConceptDefinition concept,
            List<string> missingIds)
        {
            string[] prerequisiteIds =
                concept.PrerequisiteConceptIds;

            if (prerequisiteIds == null ||
                prerequisiteIds.Length == 0)
            {
                return true;
            }

            int validPrerequisiteCount = 0;
            int satisfiedPrerequisiteCount = 0;

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
                    missingIds.Add(prerequisiteId);
                    continue;
                }

                validPrerequisiteCount++;

                float mastery =
                    GetConceptMastery(prerequisite.Id);

                bool satisfied =
                    mastery >=
                    prerequisiteMasteryRequirement;

                if (satisfied)
                {
                    satisfiedPrerequisiteCount++;
                }
                else
                {
                    missingIds.Add(prerequisite.Id);

                    if (!requireAllPrerequisites)
                        break;
                }
            }

            if (validPrerequisiteCount == 0)
                return missingIds.Count == 0;

            if (requireAllPrerequisites)
            {
                return
                    satisfiedPrerequisiteCount ==
                    validPrerequisiteCount &&
                    missingIds.Count == 0;
            }

            return
                satisfiedPrerequisiteCount > 0;
        }

        /// <summary>
        /// Gets the learner's current mastery for a concept.
        /// </summary>
       public float GetConceptMastery(
    string conceptId)
{
    if (learnerModel == null)
        return 0f;

    if (string.IsNullOrWhiteSpace(conceptId))
        return 0f;

    return Mathf.Clamp01(
        learnerModel.GetMastery(conceptId));
}

        /// <summary>
        /// Copies the prerequisite concepts that are currently
        /// preventing a concept from becoming ready.
        /// </summary>
        public void CopyMissingPrerequisites(
            string conceptId,
            List<string> results)
        {
            if (results == null)
                return;

            results.Clear();

            SparkAIConceptProgressionResult result =
                EvaluateConcept(conceptId);

            if (!result.IsValid)
                return;

            if (result.MissingPrerequisiteIds == null)
                return;

            for (int i = 0;
                 i < result.MissingPrerequisiteIds.Length;
                 i++)
            {
                results.Add(
                    result.MissingPrerequisiteIds[i]);
            }
        }
    }


    /// <summary>
    /// Immutable result of one concept progression evaluation.
    /// </summary>
    public readonly struct SparkAIConceptProgressionResult
    {
        public bool IsValid { get; }

        public string ConceptId { get; }

        public string ConceptName { get; }

        public bool IsReady { get; }

        public bool PrerequisitesSatisfied { get; }

        public bool RequiredForProgression { get; }

        public float RequiredMastery { get; }

        public string[] MissingPrerequisiteIds { get; }

        public string Summary { get; }

        public SparkAIConceptProgressionResult(
            bool isValid,
            string conceptId,
            string conceptName,
            bool isReady,
            bool prerequisitesSatisfied,
            bool requiredForProgression,
            float requiredMastery,
            string[] missingPrerequisiteIds,
            string summary)
        {
            IsValid = isValid;
            ConceptId = conceptId;
            ConceptName = conceptName;
            IsReady = isReady;
            PrerequisitesSatisfied = prerequisitesSatisfied;
            RequiredForProgression = requiredForProgression;
            RequiredMastery = requiredMastery;
            MissingPrerequisiteIds =
                missingPrerequisiteIds;
            Summary = summary;
        }

        public static SparkAIConceptProgressionResult Invalid(
            string conceptId)
        {
            return new SparkAIConceptProgressionResult(
                false,
                conceptId,
                string.Empty,
                false,
                false,
                false,
                0f,
                Array.Empty<string>(),
                "Concept progression could not be evaluated.");
        }
    }
}