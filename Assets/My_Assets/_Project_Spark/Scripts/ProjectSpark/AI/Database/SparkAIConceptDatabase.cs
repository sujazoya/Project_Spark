using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Authoritative database of Project Spark learning concepts.
    ///
    /// Stores:
    /// - Concept identity
    /// - Educational description
    /// - Learning goals
    /// - Prerequisites
    /// - Recommended progression level
    /// - Teacher dialogue
    /// - Teaching hints and explanations
    /// - Voice configuration
    /// - Player experience information
    ///
    /// This class only stores and retrieves concept definitions.
    /// It does not evaluate the circuit and does not modify gameplay.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkAIConceptDatabase",
        menuName = "Project Spark/AI/Concept Database")]
    public sealed class SparkAIConceptDatabase : ScriptableObject
    {
        [Header("Database")]
        [SerializeField]
        private string databaseId =
            "project_spark_concepts";

        [SerializeField]
        private List<SparkAIConceptDefinition> concepts =
            new List<SparkAIConceptDefinition>();

        private readonly Dictionary<string, SparkAIConceptDefinition>
            conceptIndex =
                new Dictionary<string, SparkAIConceptDefinition>(
                    StringComparer.OrdinalIgnoreCase);

        private bool initialized;

        public string DatabaseId =>
            databaseId;

        public int ConceptCount =>
            concepts != null
                ? concepts.Count
                : 0;

        public bool IsInitialized =>
            initialized;

        // =========================================================
        // INITIALIZATION
        // =========================================================

        /// <summary>
        /// Initializes the runtime lookup index.
        /// Safe to call multiple times.
        /// </summary>
        public void Initialize()
        {
            if (initialized)
                return;

            BuildIndex();
        }

        /// <summary>
        /// Rebuilds the runtime lookup index.
        /// </summary>
        public void BuildIndex()
        {
            conceptIndex.Clear();

            if (concepts == null)
            {
                initialized = true;
                return;
            }

            for (int i = 0;
                 i < concepts.Count;
                 i++)
            {
                SparkAIConceptDefinition concept =
                    concepts[i];

                if (!concept.IsValid)
                    continue;

                string id =
                    concept.Id.Trim();

                if (string.IsNullOrEmpty(id))
                    continue;

                if (conceptIndex.ContainsKey(id))
                {
                    Debug.LogWarning(
                        $"[AI CONCEPT DATABASE] Duplicate concept ID '{id}' in '{name}'.",
                        this);

                    continue;
                }

                conceptIndex.Add(
                    id,
                    concept);
            }

            initialized = true;
        }

        private void EnsureInitialized()
        {
            if (!initialized)
                Initialize();
        }

        // =========================================================
        // CONCEPT LOOKUP
        // =========================================================

        /// <summary>
        /// Gets a concept by ID.
        /// </summary>
        public bool TryGetConcept(
            string conceptId,
            out SparkAIConceptDefinition concept)
        {
            concept = default;

            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            EnsureInitialized();

            return conceptIndex.TryGetValue(
                conceptId.Trim(),
                out concept);
        }

        /// <summary>
        /// Returns true when a valid concept exists.
        /// </summary>
        public bool ContainsConcept(
            string conceptId)
        {
            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            EnsureInitialized();

            return conceptIndex.ContainsKey(
                conceptId.Trim());
        }

        /// <summary>
        /// Copies all valid concepts into the supplied list.
        /// </summary>
        public void CopyAllConcepts(
            List<SparkAIConceptDefinition> results)
        {
            if (results == null)
                return;

            results.Clear();

            if (concepts == null)
                return;

            for (int i = 0;
                 i < concepts.Count;
                 i++)
            {
                SparkAIConceptDefinition concept =
                    concepts[i];

                if (!concept.IsValid)
                    continue;

                results.Add(concept);
            }
        }
        public bool TryGetTeacherStrongHint(
    string conceptId,
    out string text)
{
    text = string.Empty;

    if (!TryGetConcept(
            conceptId,
            out SparkAIConceptDefinition concept))
    {
        return false;
    }

    text =
        concept.TeacherStrongHint;

    return !string.IsNullOrWhiteSpace(
        text);
}

        // =========================================================
        // PREREQUISITES
        // =========================================================

        /// <summary>
        /// Copies the prerequisite IDs of a concept.
        /// </summary>
        public bool CopyPrerequisites(
            string conceptId,
            List<string> results)
        {
            if (results == null)
                return false;

            results.Clear();

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            string[] prerequisites =
                concept.PrerequisiteConceptIds;

            if (prerequisites == null ||
                prerequisites.Length == 0)
            {
                return true;
            }

            for (int i = 0;
                 i < prerequisites.Length;
                 i++)
            {
                string prerequisite =
                    prerequisites[i];

                if (string.IsNullOrWhiteSpace(
                        prerequisite))
                {
                    continue;
                }

                results.Add(
                    prerequisite.Trim());
            }

            return true;
        }

        /// <summary>
        /// Checks whether one concept requires another concept.
        /// </summary>
        public bool HasPrerequisite(
            string conceptId,
            string prerequisiteConceptId)
        {
            if (string.IsNullOrWhiteSpace(
                    conceptId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    prerequisiteConceptId))
            {
                return false;
            }

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.HasPrerequisite(
                prerequisiteConceptId.Trim());
        }

        // =========================================================
        // TEACHING CONTENT
        // =========================================================

        /// <summary>
        /// Gets the teacher's introduction for a concept.
        /// </summary>
        public bool TryGetTeacherIntroduction(
            string conceptId,
            out string introduction)
        {
            introduction = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            introduction =
                concept.TeacherIntroduction;

            return !string.IsNullOrWhiteSpace(
                introduction);
        }

        /// <summary>
        /// Gets the teacher's explanation for a concept.
        /// </summary>
        public bool TryGetTeacherExplanation(
            string conceptId,
            out string explanation)
        {
            explanation = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            explanation =
                concept.TeacherExplanation;

            return !string.IsNullOrWhiteSpace(
                explanation);
        }

        /// <summary>
        /// Gets the teacher's example for a concept.
        /// </summary>
        public bool TryGetTeacherExample(
            string conceptId,
            out string example)
        {
            example = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            example =
                concept.TeacherExample;

            return !string.IsNullOrWhiteSpace(
                example);
        }

        /// <summary>
        /// Gets the teacher's question for a concept.
        /// </summary>
        public bool TryGetTeacherQuestion(
            string conceptId,
            out string question)
        {
            question = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            question =
                concept.TeacherQuestion;

            return !string.IsNullOrWhiteSpace(
                question);
        }

        /// <summary>
        /// Gets either the normal or strong teaching hint.
        /// </summary>
        public bool TryGetTeacherHint(
            string conceptId,
            bool strongHint,
            out string hint)
        {
            hint = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            hint = strongHint
                ? concept.TeacherStrongHint
                : concept.TeacherHint;

            return !string.IsNullOrWhiteSpace(
                hint);
        }

        /// <summary>
        /// Gets the success message for a concept.
        /// </summary>
        public bool TryGetTeacherSuccessMessage(
            string conceptId,
            out string message)
        {
            message = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            message =
                concept.TeacherSuccessMessage;

            return !string.IsNullOrWhiteSpace(
                message);
        }

        /// <summary>
        /// Gets the failure message for a concept.
        /// </summary>
        public bool TryGetTeacherFailureMessage(
            string conceptId,
            out string message)
        {
            message = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            message =
                concept.TeacherFailureMessage;

            return !string.IsNullOrWhiteSpace(
                message);
        }

        // =========================================================
        // VOICE
        // =========================================================

        /// <summary>
        /// Gets the voice configuration for a concept.
        ///
        /// The database does not perform TTS.
        /// It only supplies the configuration to the voice system.
        /// </summary>
        public bool TryGetVoiceSettings(
            string conceptId,
            out SparkAIVoiceEmotion emotion,
            out float speed,
            out float pitch,
            out bool allowVoice)
        {
            emotion =
                SparkAIVoiceEmotion.Neutral;

            speed = 1f;
            pitch = 0.5f;
            allowVoice = false;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            emotion =
                concept.VoiceEmotion;

            speed =
                concept.VoiceSpeed;

            pitch =
                concept.VoicePitch;

            allowVoice =
                concept.AllowVoice;

            return true;
        }

        /// <summary>
        /// Returns whether voice is enabled for this concept.
        /// </summary>
        public bool IsVoiceEnabled(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowVoice;
        }

        // =========================================================
        // TEACHING BEHAVIOUR
        // =========================================================

        public bool AllowsQuestion(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowQuestion;
        }

        public bool AllowsHint(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowHint;
        }

        public bool AllowsStrongHint(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowStrongHint;
        }

        public bool AllowsExplanation(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowExplanation;
        }

        public bool AllowsExample(
            string conceptId)
        {
            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            return concept.AllowExample;
        }

        // =========================================================
        // PLAYER EXPERIENCE
        // =========================================================

        /// <summary>
        /// Gets the real-world connection for a concept.
        /// </summary>
        public bool TryGetRealWorldConnection(
            string conceptId,
            out string connection)
        {
            connection = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            connection =
                concept.RealWorldConnection;

            return !string.IsNullOrWhiteSpace(
                connection);
        }

        /// <summary>
        /// Gets a common misconception associated with a concept.
        /// </summary>
        public bool TryGetCommonMisconception(
            string conceptId,
            out string misconception)
        {
            misconception = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            misconception =
                concept.CommonMisconception;

            return !string.IsNullOrWhiteSpace(
                misconception);
        }

        /// <summary>
        /// Gets the safety note associated with a concept.
        /// </summary>
        public bool TryGetSafetyNote(
            string conceptId,
            out string safetyNote)
        {
            safetyNote = string.Empty;

            if (!TryGetConcept(
                    conceptId,
                    out SparkAIConceptDefinition concept))
            {
                return false;
            }

            safetyNote =
                concept.SafetyNote;

            return !string.IsNullOrWhiteSpace(
                safetyNote);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (concepts == null)
            {
                concepts =
                    new List<SparkAIConceptDefinition>();
            }

            databaseId =
                databaseId?.Trim() ??
                string.Empty;

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < concepts.Count;
                 i++)
            {
                SparkAIConceptDefinition concept =
                    concepts[i];

                if (!concept.IsValid)
                    continue;

                string id =
                    concept.Id?.Trim();

                if (string.IsNullOrEmpty(id))
                    continue;

                if (!ids.Add(id))
                {
                    Debug.LogWarning(
                        $"[AI CONCEPT DATABASE] Duplicate concept ID '{id}' in '{name}'.",
                        this);
                }
            }

            initialized = false;
        }

#endif
    }
}