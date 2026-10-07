using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Builds a runtime learner profile from educational evidence.
    ///
    /// This component does not determine electrical truth.
    /// It only tracks how well the player appears to understand
    /// individual educational concepts.
    ///
    /// Evidence can come from:
    /// - evaluated answers
    /// - successful actions
    /// - incorrect answers
    /// - repeated attempts
    /// - hint usage
    ///
    /// The model is intentionally simple and deterministic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearnerModel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIPlayerResponseObserver responseObserver;

        [Header("Learning")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float correctLearningGain = 0.12f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float incorrectLearningLoss = 0.10f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float hintPenalty = 0.02f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float successfulActionGain = 0.04f;

        [Header("Profile")]
        [SerializeField]
        private int maximumConcepts = 128;

        private readonly Dictionary<string, SparkAILearnerConcept>
            concepts =
                new Dictionary<string, SparkAILearnerConcept>(
                    StringComparer.OrdinalIgnoreCase);

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public event Action<SparkAILearnerConcept>
            ConceptChanged;

        private void Awake()
        {
            if (responseObserver == null)
            {
                Debug.LogError(
                    "[Spark AI Learner Model] " +
                    "SparkAIPlayerResponseObserver reference is missing.",
                    this);

                return;
            }

            maximumConcepts =
                Mathf.Clamp(
                    maximumConcepts,
                    8,
                    512);

            initialized = true;

            responseObserver.ResponseRecorded +=
                HandleResponseRecorded;
        }

        private void OnDestroy()
        {
            if (responseObserver != null)
            {
                responseObserver.ResponseRecorded -=
                    HandleResponseRecorded;
            }
        }

        /// <summary>
        /// Returns the current state of a concept.
        /// </summary>
        public bool TryGetConcept(
            string conceptId,
            out SparkAILearnerConcept concept)
        {
            if (string.IsNullOrEmpty(conceptId))
            {
                concept = SparkAILearnerConcept.Invalid();
                return false;
            }

            return concepts.TryGetValue(
                conceptId,
                out concept);
        }

        /// <summary>
        /// Returns the player's current mastery value.
        /// </summary>
        public float GetMastery(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAILearnerConcept concept))
            {
                return 0f;
            }

            return concept.Mastery01;
        }

        /// <summary>
        /// Returns the player's current learning state.
        /// </summary>
        public SparkAILearningState GetLearningState(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAILearnerConcept concept))
            {
                return SparkAILearningState.Unknown;
            }

            return concept.LearningState;
        }

        /// <summary>
        /// Returns a snapshot of all tracked concepts.
        /// </summary>
        public IReadOnlyCollection<SparkAILearnerConcept>
            GetAllConcepts()
        {
            return concepts.Values;
        }

        /// <summary>
        /// Applies a successful learning event directly.
        ///
        /// This is useful for authoritative gameplay events where
        /// completing an action demonstrates understanding.
        /// </summary>
        public void RecordSuccessfulAction(
            string conceptId)
        {
            if (!initialized ||
                string.IsNullOrEmpty(conceptId))
            {
                return;
            }

            SparkAILearnerConcept concept =
                GetOrCreateConcept(
                    conceptId);

            concept.ApplyPositiveEvidence(
                successfulActionGain);

            concepts[conceptId] =
                concept;

            ConceptChanged?.Invoke(
                concept);
        }

        /// <summary>
        /// Applies an incorrect learning event directly.
        ///
        /// Useful when a deterministic lesson evaluator knows
        /// that the player's action demonstrated a misconception.
        /// </summary>
        public void RecordIncorrectAction(
            string conceptId)
        {
            if (!initialized ||
                string.IsNullOrEmpty(conceptId))
            {
                return;
            }

            SparkAILearnerConcept concept =
                GetOrCreateConcept(
                    conceptId);

            concept.ApplyNegativeEvidence(
                incorrectLearningLoss);

            concepts[conceptId] =
                concept;

            ConceptChanged?.Invoke(
                concept);
        }

        /// <summary>
        /// Clears the current runtime learner profile.
        /// </summary>
        public void ClearProfile()
        {
            concepts.Clear();
        }

        private void HandleResponseRecorded(
            SparkAIPlayerResponse response)
        {
            if (!initialized ||
                !response.IsValid)
            {
                return;
            }

            if (response.Type ==
                SparkAIPlayerResponseType.HintRequested)
            {
                HandleHint(response);
                return;
            }

            if (response.HasEvaluation)
            {
                HandleEvaluation(response);
                return;
            }

            if (response.Type ==
                SparkAIPlayerResponseType.ActionPerformed ||
                response.Type ==
                SparkAIPlayerResponseType.ComponentConnected ||
                response.Type ==
                SparkAIPlayerResponseType.MeasurementTaken ||
                response.Type ==
                SparkAIPlayerResponseType.ToolUsed)
            {
                if (!string.IsNullOrEmpty(
                        response.ConceptId))
                {
                    RecordSuccessfulAction(
                        response.ConceptId);
                }
            }
        }

        private void HandleEvaluation(
            SparkAIPlayerResponse response)
        {
            if (string.IsNullOrEmpty(
                    response.ConceptId))
            {
                return;
            }

            SparkAILearnerConcept concept =
                GetOrCreateConcept(
                    response.ConceptId);

            if (response.IsCorrect)
            {
                float gain =
                    correctLearningGain *
                    Mathf.Clamp01(
                        response.EvaluationConfidence);

                if (gain <= 0f)
                {
                    gain =
                        correctLearningGain;
                }

                concept.ApplyPositiveEvidence(
                    gain);
            }
            else
            {
                concept.ApplyNegativeEvidence(
                    incorrectLearningLoss);
            }

            concepts[response.ConceptId] =
                concept;

            ConceptChanged?.Invoke(
                concept);
        }

        private void HandleHint(
            SparkAIPlayerResponse response)
        {
            SparkAIPlayerResponse latestEvaluation;

            if (!responseObserver.TryGetLatestEvaluation(
                    out latestEvaluation))
            {
                return;
            }

            string conceptId =
                latestEvaluation.ConceptId;

            if (string.IsNullOrEmpty(conceptId))
            {
                return;
            }

            SparkAILearnerConcept concept =
                GetOrCreateConcept(
                    conceptId);

            concept.ApplyNegativeEvidence(
                hintPenalty);

            concepts[conceptId] =
                concept;

            ConceptChanged?.Invoke(
                concept);
        }

        private SparkAILearnerConcept GetOrCreateConcept(
            string conceptId)
        {
            SparkAILearnerConcept concept;

            if (concepts.TryGetValue(
                    conceptId,
                    out concept))
            {
                return concept;
            }

            if (concepts.Count >= maximumConcepts)
            {
                RemoveLowestPriorityConcept();
            }

            concept =
                SparkAILearnerConcept.Create(
                    conceptId);

            concepts.Add(
                conceptId,
                concept);

            return concept;
        }

        private void RemoveLowestPriorityConcept()
        {
            if (concepts.Count == 0)
                return;

            string removeId = null;

            float lowestPriority =
                float.MaxValue;

            foreach (
                KeyValuePair<
                    string,
                    SparkAILearnerConcept> pair
                in concepts)
            {
                if (pair.Value.Priority <
                    lowestPriority)
                {
                    lowestPriority =
                        pair.Value.Priority;

                    removeId =
                        pair.Key;
                }
            }

            if (!string.IsNullOrEmpty(removeId))
            {
                concepts.Remove(
                    removeId);
            }
        }
    }

    /// <summary>
    /// Runtime learner state for one educational concept.
    /// </summary>
    [Serializable]
    public struct SparkAILearnerConcept
    {
        public bool IsValid { get; }

        public string ConceptId { get; }

        public float Mastery01 { get; }

        public SparkAILearningState LearningState { get; }

        public int CorrectAnswers { get; }

        public int IncorrectAnswers { get; }

        public int SuccessfulActions { get; }

        public int Attempts { get; }

        public float LastEvidenceTime { get; }

        /// <summary>
        /// Used internally to decide which concepts are least
        /// important to retain when the profile reaches capacity.
        /// </summary>
        public float Priority
        {
            get
            {
                float activity =
                    Attempts * 1f;

                float mastery =
                    Mastery01 * 0.5f;

                return activity + mastery;
            }
        }

        private SparkAILearnerConcept(
            bool isValid,
            string conceptId,
            float mastery01,
            SparkAILearningState learningState,
            int correctAnswers,
            int incorrectAnswers,
            int successfulActions,
            int attempts,
            float lastEvidenceTime)
        {
            IsValid =
                isValid;

            ConceptId =
                conceptId ??
                string.Empty;

            Mastery01 =
                Mathf.Clamp01(
                    mastery01);

            LearningState =
                learningState;

            CorrectAnswers =
                Mathf.Max(
                    0,
                    correctAnswers);

            IncorrectAnswers =
                Mathf.Max(
                    0,
                    incorrectAnswers);

            SuccessfulActions =
                Mathf.Max(
                    0,
                    successfulActions);

            Attempts =
                Mathf.Max(
                    0,
                    attempts);

            LastEvidenceTime =
                lastEvidenceTime;
        }

        public static SparkAILearnerConcept Create(
            string conceptId)
        {
            return new SparkAILearnerConcept(
                true,
                conceptId,
                0f,
                SparkAILearningState.Beginner,
                0,
                0,
                0,
                0,
                UnityEngine.Time.time);
        }

        public static SparkAILearnerConcept Invalid()
        {
            return new SparkAILearnerConcept(
                false,
                string.Empty,
                0f,
                SparkAILearningState.Unknown,
                0,
                0,
                0,
                0,
                0f);
        }

        public void ApplyPositiveEvidence(
            float amount)
        {
            amount =
                Mathf.Max(
                    0f,
                    amount);

            float mastery =
                Mathf.Clamp01(
                    Mastery01 + amount);

            this =
                new SparkAILearnerConcept(
                    true,
                    ConceptId,
                    mastery,
                    DetermineState(mastery),
                    CorrectAnswers + 1,
                    IncorrectAnswers,
                    SuccessfulActions + 1,
                    Attempts + 1,
                    UnityEngine.Time.time);
        }

        public void ApplyNegativeEvidence(
            float amount)
        {
            amount =
                Mathf.Max(
                    0f,
                    amount);

            float mastery =
                Mathf.Clamp01(
                    Mastery01 - amount);

            this =
                new SparkAILearnerConcept(
                    true,
                    ConceptId,
                    mastery,
                    DetermineState(mastery),
                    CorrectAnswers,
                    IncorrectAnswers + 1,
                    SuccessfulActions,
                    Attempts + 1,
                    UnityEngine.Time.time);
        }

        private static SparkAILearningState DetermineState(
            float mastery)
        {
            if (mastery < 0.20f)
            {
                return SparkAILearningState.Beginner;
            }

            if (mastery < 0.45f)
            {
                return SparkAILearningState.Developing;
            }

            if (mastery < 0.70f)
            {
                return SparkAILearningState.Competent;
            }

            if (mastery < 0.90f)
            {
                return SparkAILearningState.Proficient;
            }

            return SparkAILearningState.Mastered;
        }
    }

    public enum SparkAILearningState
    {
        Unknown = 0,

        Beginner = 1,

        Developing = 2,

        Competent = 3,

        Proficient = 4,

        Mastered = 5
    }
}