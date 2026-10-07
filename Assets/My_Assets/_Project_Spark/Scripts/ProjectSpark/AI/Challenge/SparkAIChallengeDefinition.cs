using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Data definition for one educational Project Spark challenge.
    ///
    /// A challenge describes what the player should accomplish.
    /// It does NOT determine whether the electrical circuit is correct.
    ///
    /// Actual success remains authoritative through:
    /// - SparkLevelEvaluator
    /// - SparkAIChallengeEvaluator
    /// - Project Spark gameplay systems
    /// </summary>
    [Serializable]
    public struct SparkAIChallengeDefinition
    {
        [Header("Identity")]
        [SerializeField] private string id;

        [SerializeField] private string title;

        [TextArea(2, 5)]
        [SerializeField] private string description;

        [Header("Educational")]
        [SerializeField] private string conceptId;

        [SerializeField] private SparkAITeachingTopic topic;

        [SerializeField] private SparkAIChallengeDifficulty difficulty;

        [TextArea(2, 5)]
        [SerializeField] private string objective;

        [TextArea(2, 5)]
        [SerializeField] private string instruction;

        [TextArea(2, 5)]
        [SerializeField] private string successDescription;

        [Header("Teaching Support")]
        [TextArea(2, 5)]
        [SerializeField] private string hint;

        [TextArea(2, 5)]
        [SerializeField] private string strongHint;

        [Header("Requirements")]
        [SerializeField] private bool requiresCircuitChange;

        [SerializeField] private bool requiresMeasurement;

        [SerializeField] private bool requiresPlayerAnswer;

        [Header("Progression")]
        [SerializeField] private bool requiredForLesson;

        [SerializeField] private bool allowRepeat;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id);

        public string Id =>
            id;

        public string Title =>
            title;

        public string Description =>
            description;

        public string ConceptId =>
            conceptId;

        public SparkAITeachingTopic Topic =>
            topic;

        public SparkAIChallengeDifficulty Difficulty =>
            difficulty;

        public string Objective =>
            objective;

        public string Instruction =>
            instruction;

        public string SuccessDescription =>
            successDescription;

        public string Hint =>
            hint;

        public string StrongHint =>
            strongHint;

        public bool RequiresCircuitChange =>
            requiresCircuitChange;

        public bool RequiresMeasurement =>
            requiresMeasurement;

        public bool RequiresPlayerAnswer =>
            requiresPlayerAnswer;

        public bool RequiredForLesson =>
            requiredForLesson;

        public bool AllowRepeat =>
            allowRepeat;
    }


    /// <summary>
    /// Difficulty of an educational challenge.
    ///
    /// This is intentionally separate from lesson difficulty.
    /// A single lesson can contain easy, guided and advanced
    /// challenges.
    /// </summary>
   
}