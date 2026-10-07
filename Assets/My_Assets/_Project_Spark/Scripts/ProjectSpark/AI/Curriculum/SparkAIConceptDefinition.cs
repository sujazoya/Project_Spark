using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Defines one Project Spark learning concept.
    ///
    /// Contains:
    /// - Educational meaning
    /// - Learning progression
    /// - Teaching language
    /// - Voice presentation
    /// - Player-facing explanations
    ///
    /// This is data only.
    /// It does not modify gameplay or the electrical solver.
    /// </summary>
    [Serializable]
    public struct SparkAIConceptDefinition
    {
        [Header("Identity")]
        [SerializeField]
        private string id;

        [SerializeField]
        private string displayName;

        [TextArea(3, 8)]
        [SerializeField]
        private string description;

        [Header("Learning")]
        [TextArea(2, 6)]
        [SerializeField]
        private string learningGoal;

        [TextArea(2, 6)]
        [SerializeField]
        private string playerUnderstanding;

        [Header("Progression")]
        [SerializeField]
        private string[] prerequisiteConceptIds;

        [SerializeField]
        private int recommendedLevel;

        [SerializeField]
        private bool requiredForProgression;

        [Header("Teaching")]
        [TextArea(2, 6)]
        [SerializeField]
        private string teacherIntroduction;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherExplanation;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherExample;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherQuestion;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherHint;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherStrongHint;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherSuccessMessage;

        [TextArea(2, 6)]
        [SerializeField]
        private string teacherFailureMessage;

        [Header("Voice")]
        [SerializeField]
        private bool allowVoice;

        [SerializeField]
        private SparkAIVoiceEmotion voiceEmotion;

        [Range(0.5f, 2f)]
        [SerializeField]
        private float voiceSpeed;

        [Range(0f, 1f)]
        [SerializeField]
        private float voicePitch;

        [Header("Teaching Behaviour")]
        [SerializeField]
        private bool allowQuestion;

        [SerializeField]
        private bool allowHint;

        [SerializeField]
        private bool allowStrongHint;

        [SerializeField]
        private bool allowExplanation;

        [SerializeField]
        private bool allowExample;

        [Header("Player Experience")]
        [TextArea(2, 5)]
        [SerializeField]
        private string realWorldConnection;

        [TextArea(2, 5)]
        [SerializeField]
        private string commonMisconception;

        [TextArea(2, 5)]
        [SerializeField]
        private string safetyNote;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id);

        public string Id =>
            id;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;

        public string LearningGoal =>
            learningGoal;

        public string PlayerUnderstanding =>
            playerUnderstanding;

        public string[] PrerequisiteConceptIds =>
            prerequisiteConceptIds;

        public int RecommendedLevel =>
            Mathf.Max(0, recommendedLevel);

        public bool RequiredForProgression =>
            requiredForProgression;

        public string TeacherIntroduction =>
            teacherIntroduction;

        public string TeacherExplanation =>
            teacherExplanation;

        public string TeacherExample =>
            teacherExample;

        public string TeacherQuestion =>
            teacherQuestion;

        public string TeacherHint =>
            teacherHint;

        public string TeacherStrongHint =>
            teacherStrongHint;

        public string TeacherSuccessMessage =>
            teacherSuccessMessage;

        public string TeacherFailureMessage =>
            teacherFailureMessage;

        public bool AllowVoice =>
            allowVoice;

        public SparkAIVoiceEmotion VoiceEmotion =>
            voiceEmotion;

        public float VoiceSpeed =>
            Mathf.Clamp(voiceSpeed, 0.5f, 2f);

        public float VoicePitch =>
            Mathf.Clamp01(voicePitch);

        public bool AllowQuestion =>
            allowQuestion;

        public bool AllowHint =>
            allowHint;

        public bool AllowStrongHint =>
            allowStrongHint;

        public bool AllowExplanation =>
            allowExplanation;

        public bool AllowExample =>
            allowExample;

        public string RealWorldConnection =>
            realWorldConnection;

        public string CommonMisconception =>
            commonMisconception;

        public string SafetyNote =>
            safetyNote;

        public int PrerequisiteCount =>
            prerequisiteConceptIds != null
                ? prerequisiteConceptIds.Length
                : 0;

        public bool HasPrerequisite(
            string conceptId)
        {
            if (string.IsNullOrWhiteSpace(conceptId))
                return false;

            if (prerequisiteConceptIds == null)
                return false;

            for (int i = 0;
                 i < prerequisiteConceptIds.Length;
                 i++)
            {
                if (string.Equals(
                        prerequisiteConceptIds[i],
                        conceptId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum SparkAIVoiceEmotion
    {
        Neutral = 0,
        Friendly = 1,
        Curious = 2,
        Encouraging = 3,
        Excited = 4,
        Calm = 5,
        Serious = 6,
        Warning = 7,
        Celebratory = 8
    }
}