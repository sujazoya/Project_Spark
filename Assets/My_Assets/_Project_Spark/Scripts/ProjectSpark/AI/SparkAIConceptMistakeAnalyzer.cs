using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Converts authoritative Project Spark world/evaluation evidence
    /// into concept-specific learning evidence.
    ///
    /// This component does NOT decide whether the electrical circuit
    /// is correct. SparkAIWorld and LevelGamePlayManager remain
    /// observers of the authoritative Project Spark systems.
    ///
    /// The analyzer only answers:
    ///
    /// "Given the evidence we already have, which educational concept
    /// is most directly related to the problem?"
    ///
    /// It is intentionally conservative.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIConceptMistakeAnalyzer : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField]
        private SparkAIWorld world;

        [SerializeField]
        private SparkAILesson lesson;

        [SerializeField]
        private SparkAILearning learning;

        [Header("Concept IDs")]
        [SerializeField]
        private string openCircuitConceptId = "open_circuit";

        [SerializeField]
        private string closedCircuitConceptId = "closed_circuit";

        [SerializeField]
        private string shortCircuitConceptId = "short_circuit";

        [SerializeField]
        private string polarityConceptId = "polarity";

        [SerializeField]
        private string voltageConceptId = "voltage";

        [SerializeField]
        private string currentConceptId = "current";

        [SerializeField]
        private string powerConceptId = "power";

        [Header("Analysis")]
        [SerializeField]
        private bool analyzeOnlyActiveLessonConcepts = true;

        [SerializeField]
        private bool allowFallbackToLessonConcept = false;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Concept Analyzer] " +
                    "SparkAIWorld reference is missing.",
                    this);

                return;
            }

            if (lesson == null)
            {
                Debug.LogError(
                    "[Spark AI Concept Analyzer] " +
                    "SparkAILesson reference is missing.",
                    this);

                return;
            }

            if (learning == null)
            {
                Debug.LogError(
                    "[Spark AI Concept Analyzer] " +
                    "SparkAILearning reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Analyzes the current authoritative Project Spark state.
        ///
        /// Returns true when a specific concept was identified and
        /// learning evidence was recorded.
        /// </summary>
        public bool AnalyzeCurrentState(
            bool repeatedMistake = false)
        {
            if (!initialized)
                return false;

            SparkAIWorldSnapshot snapshot =
                world.LatestSnapshot;

            return Analyze(
                snapshot,
                repeatedMistake);
        }

        /// <summary>
        /// Analyzes an explicitly supplied world snapshot.
        /// </summary>
        public bool Analyze(
            SparkAIWorldSnapshot snapshot,
            bool repeatedMistake = false)
        {
            if (!initialized)
                return false;

            if (!lesson.HasActiveLesson)
                return false;

            SparkAILevelSnapshot level =
                snapshot.Level;

            if (!level.HasLevel)
                return false;

            /*
             * Highest-priority electrical fault:
             * source short.
             */
            if (level.SourceShorted)
            {
                return RecordConceptEvidence(
                    shortCircuitConceptId,
                    repeatedMistake,
                    "The power source is shorted.");
            }

            /*
             * Target short is also primarily a short-circuit concept.
             */
            if (level.TargetShorted)
            {
                return RecordConceptEvidence(
                    shortCircuitConceptId,
                    repeatedMistake,
                    "The target is shorted.");
            }

            /*
             * Wrong connection is more ambiguous.
             *
             * We only map it to polarity when the level itself
             * provides enough evidence through the active lesson.
             *
             * Otherwise we deliberately avoid guessing.
             */
            if (level.WrongConnection)
            {
                if (lesson.TeachesConcept(polarityConceptId))
                {
                    return RecordConceptEvidence(
                        polarityConceptId,
                        repeatedMistake,
                        "The circuit contains an incorrect connection and the active lesson teaches polarity.");
                }

                if (lesson.TeachesConcept(openCircuitConceptId))
                {
                    return RecordConceptEvidence(
                        openCircuitConceptId,
                        repeatedMistake,
                        "The circuit contains an incorrect connection while learning circuit continuity.");
                }

                if (allowFallbackToLessonConcept)
                {
                    return RecordFallbackEvidence(
                        repeatedMistake,
                        "The circuit contains an incorrect connection.");
                }

                return false;
            }

            /*
             * No valid power source is primarily related to
             * source/circuit fundamentals.
             */
            if (!level.HasValidPowerSource)
            {
                if (lesson.TeachesConcept(voltageConceptId))
                {
                    return RecordConceptEvidence(
                        voltageConceptId,
                        repeatedMistake,
                        "The circuit does not currently have a valid active power source.");
                }

                if (lesson.TeachesConcept(closedCircuitConceptId))
                {
                    return RecordConceptEvidence(
                        closedCircuitConceptId,
                        repeatedMistake,
                        "The circuit does not currently have a valid powered path.");
                }

                if (allowFallbackToLessonConcept)
                {
                    return RecordFallbackEvidence(
                        repeatedMistake,
                        "The circuit does not currently have a valid active power source.");
                }
            }

            return false;
        }

        /// <summary>
        /// Records learning evidence for the selected concept.
        /// </summary>
        private bool RecordConceptEvidence(
            string conceptId,
            bool repeatedMistake,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            if (analyzeOnlyActiveLessonConcepts &&
                !lesson.TeachesConcept(conceptId))
            {
                return false;
            }

            if (repeatedMistake)
            {
                learning.RecordRepeatedMistake(
                    conceptId,
                    reason);
            }
            else
            {
                learning.RecordStruggle(
                    conceptId,
                    reason);
            }

            return true;
        }

        /// <summary>
        /// Optional conservative fallback.
        ///
        /// This should normally remain disabled because assigning
        /// every failure to every lesson concept is educationally weak.
        /// </summary>
        private bool RecordFallbackEvidence(
            bool repeatedMistake,
            string reason)
        {
            if (!allowFallbackToLessonConcept)
                return false;

            string[] concepts =
                lesson.ActiveLesson.ConceptIds;

            if (concepts == null ||
                concepts.Length == 0)
            {
                return false;
            }

            string selectedConcept = FindMostRelevantFallbackConcept();

            if (string.IsNullOrWhiteSpace(selectedConcept))
                return false;

            if (repeatedMistake)
            {
                learning.RecordRepeatedMistake(
                    selectedConcept,
                    reason);
            }
            else
            {
                learning.RecordStruggle(
                    selectedConcept,
                    reason);
            }

            return true;
        }

        /// <summary>
        /// Chooses a conservative fallback concept.
        ///
        /// Earlier concepts are preferred because Project Spark's
        /// curriculum is intended to progress from fundamentals
        /// toward advanced electronics.
        /// </summary>
        private string FindMostRelevantFallbackConcept()
        {
            string[] concepts =
                lesson.ActiveLesson.ConceptIds;

            if (concepts == null)
                return string.Empty;

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                if (string.Equals(
                        conceptId,
                        closedCircuitConceptId,
                        StringComparison.Ordinal))
                {
                    return conceptId;
                }
            }

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                return conceptId;
            }

            return string.Empty;
        }
    }
}