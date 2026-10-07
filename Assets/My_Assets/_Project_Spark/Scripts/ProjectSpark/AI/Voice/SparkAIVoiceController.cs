using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Presentation-level voice controller for Project Spark AI.
    ///
    /// Responsibilities:
    /// - Resolves voice settings from the concept database.
    /// - Stores the current voice request.
    /// - Notifies a future TTS provider.
    /// - Optionally controls an AudioSource when an audio clip is supplied.
    ///
    /// This class does NOT:
    /// - decide what to teach
    /// - evaluate player answers
    /// - evaluate circuits
    /// - modify gameplay
    /// - generate audio by itself
    ///
    /// A future offline/local TTS provider can subscribe to
    /// VoiceRequested without changing the teaching architecture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIVoiceController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAIConceptDatabase conceptDatabase;

        [SerializeField]
        private AudioSource audioSource;

        [Header("Voice")]
        [SerializeField]
        private bool voiceEnabled = true;

        [SerializeField]
        private bool interruptCurrentVoice = true;

        [SerializeField]
        [Range(0f, 1f)]
        private float defaultVolume = 1f;

        [Header("Debug")]
        [SerializeField]
        private bool logVoiceRequests;

        private string currentConceptId =
            string.Empty;

        private string currentText =
            string.Empty;

        private SparkAIVoiceEmotion currentEmotion =
            SparkAIVoiceEmotion.Neutral;

        private float currentSpeed =
            1f;

        private float currentPitch =
            0.5f;

        private bool isSpeaking;

        public bool VoiceEnabled =>
            voiceEnabled;

        public bool IsSpeaking =>
            isSpeaking;

        public string CurrentConceptId =>
            currentConceptId;

        public string CurrentText =>
            currentText;

        public SparkAIVoiceEmotion CurrentEmotion =>
            currentEmotion;

        public float CurrentSpeed =>
            currentSpeed;

        public float CurrentPitch =>
            currentPitch;

        public event Action<string> VoiceRequested;

        public event Action VoiceStopped;

        /// <summary>
        /// Fired whenever a complete voice request is created.
        ///
        /// Subscribers can forward the request to:
        /// - local TTS
        /// - offline speech engine
        /// - cloud TTS
        /// - character voice system
        /// </summary>
        public event Action<
            SparkAIVoiceRequest> VoiceRequestCreated;

        // =========================================================
        // SETTINGS
        // =========================================================

        public void SetVoiceEnabled(
            bool enabled)
        {
            voiceEnabled =
                enabled;

            if (!enabled)
            {
                StopVoice();
            }
        }

        // =========================================================
        // PUBLIC SPEAK API
        // =========================================================

        public bool SpeakIntroduction(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Introduction);
        }

        public bool SpeakExplanation(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Explanation);
        }

        public bool SpeakExample(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Example);
        }

        public bool SpeakQuestion(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Question);
        }

        public bool SpeakHint(
            string conceptId,
            bool strongHint)
        {
            return SpeakConceptText(
                conceptId,
                strongHint
                    ? SparkAIVoiceTextType.StrongHint
                    : SparkAIVoiceTextType.Hint);
        }

        public bool SpeakSuccess(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Success);
        }

        public bool SpeakFailure(
            string conceptId)
        {
            return SpeakConceptText(
                conceptId,
                SparkAIVoiceTextType.Failure);
        }

        // =========================================================
        // CORE VOICE REQUEST
        // =========================================================

        public bool SpeakConceptText(
            string conceptId,
            SparkAIVoiceTextType textType)
        {
            if (!voiceEnabled)
                return false;

            if (string.IsNullOrWhiteSpace(
                    conceptId))
            {
                return false;
            }

            if (conceptDatabase == null)
            {
                Debug.LogWarning(
                    "[AI VOICE] Concept database is not assigned.",
                    this);

                return false;
            }

            string text =
                ResolveText(
                    conceptId,
                    textType);

            if (string.IsNullOrWhiteSpace(text))
            {
                if (logVoiceRequests)
                {
                    Debug.LogWarning(
                        $"[AI VOICE] No text found for " +
                        $"concept '{conceptId}' " +
                        $"and type '{textType}'.",
                        this);
                }

                return false;
            }

            if (!conceptDatabase.TryGetVoiceSettings(
                    conceptId,
                    out SparkAIVoiceEmotion emotion,
                    out float speed,
                    out float pitch,
                    out bool allowVoice))
            {
                return false;
            }

            if (!allowVoice)
                return false;

            if (interruptCurrentVoice)
            {
                StopVoice();
            }

            currentConceptId =
                conceptId;

            currentText =
                text;

            currentEmotion =
                emotion;

            currentSpeed =
                Mathf.Clamp(
                    speed,
                    0.5f,
                    2f);

            currentPitch =
                Mathf.Clamp01(
                    pitch);

            isSpeaking = true;

            if (audioSource != null)
            {
                audioSource.volume =
                    Mathf.Clamp01(
                        defaultVolume);
            }

            SparkAIVoiceRequest request =
                new SparkAIVoiceRequest(
                    true,
                    conceptId,
                    text,
                    textType,
                    emotion,
                    currentSpeed,
                    currentPitch,
                    Time.time);

            if (logVoiceRequests)
            {
                Debug.Log(
                    $"[AI VOICE] " +
                    $"Concept={conceptId} | " +
                    $"Type={textType} | " +
                    $"Emotion={emotion} | " +
                    $"Speed={currentSpeed:0.00} | " +
                    $"Pitch={currentPitch:0.00} | " +
                    $"Text={text}",
                    this);
            }

            VoiceRequested?.Invoke(
                text);

            VoiceRequestCreated?.Invoke(
                request);

            return true;
        }

        // =========================================================
        // STOP / CLEAR
        // =========================================================

        public void StopVoice()
        {
            if (audioSource != null &&
                audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (!isSpeaking)
                return;

            isSpeaking =
                false;

            VoiceStopped?.Invoke();
        }

        public void Clear()
        {
            StopVoice();

            currentConceptId =
                string.Empty;

            currentText =
                string.Empty;

            currentEmotion =
                SparkAIVoiceEmotion.Neutral;

            currentSpeed =
                1f;

            currentPitch =
                0.5f;
        }


        /// <summary>
/// Speaks arbitrary AI-generated text.
///
/// Unlike SpeakConceptText(), this does not require
/// the text to exist in SparkAIConceptDatabase.
///
/// Used for dynamic player-question answers.
/// </summary>
public bool SpeakText(
    string text,
    SparkAIVoiceTextType textType =
        SparkAIVoiceTextType.Explanation)
{
    if (!voiceEnabled)
    {
        return false;
    }

    if (string.IsNullOrWhiteSpace(text))
    {
        return false;
    }

    if (interruptCurrentVoice)
    {
        StopVoice();
    }

    SparkAIVoiceRequest request =
        new SparkAIVoiceRequest(
            true,
            string.Empty,
            text.Trim(),
            textType,
            SparkAIVoiceEmotion.Neutral,
            1.0f,
            0.5f,
            Time.time);

    currentConceptId = string.Empty;
    currentText = request.Text;
    currentEmotion = request.Emotion;
    currentSpeed = request.Speed;
    currentPitch = request.Pitch;
    isSpeaking = true;

    // Your existing events expect the text string.
    VoiceRequestCreated?.Invoke(request);
    VoiceRequested?.Invoke(request.Text);

    return true;
}

        // =========================================================
        // TEXT RESOLUTION
        // =========================================================

        private string ResolveText(
            string conceptId,
            SparkAIVoiceTextType textType)
        {
            switch (textType)
            {
                case SparkAIVoiceTextType.Introduction:

                    return GetIntroduction(
                        conceptId);

                case SparkAIVoiceTextType.Explanation:

                    return GetExplanation(
                        conceptId);

                case SparkAIVoiceTextType.Example:

                    return GetExample(
                        conceptId);

                case SparkAIVoiceTextType.Question:

                    return GetQuestion(
                        conceptId);

                case SparkAIVoiceTextType.Hint:

                    return GetHint(
                        conceptId,
                        false);

                case SparkAIVoiceTextType.StrongHint:

                    return GetHint(
                        conceptId,
                        true);

                case SparkAIVoiceTextType.Success:

                    return GetSuccess(
                        conceptId);

                case SparkAIVoiceTextType.Failure:

                    return GetFailure(
                        conceptId);

                default:

                    return string.Empty;
            }
        }

        private string GetIntroduction(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherIntroduction(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }

        private string GetExplanation(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherExplanation(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }

        private string GetExample(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherExample(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }

        private string GetQuestion(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherQuestion(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }

       private string GetHint(
    string conceptId,
    bool strongHint)
{
    if (conceptDatabase == null)
        return string.Empty;

    return conceptDatabase.TryGetTeacherHint(
        conceptId,
        strongHint,
        out string hint)
        ? hint
        : string.Empty;
}

        private string GetSuccess(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherSuccessMessage(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }

        private string GetFailure(
            string conceptId)
        {
            return conceptDatabase
                .TryGetTeacherFailureMessage(
                    conceptId,
                    out string text)
                ? text
                : string.Empty;
        }
    }

    

    public enum SparkAIVoiceTextType
    {
        Introduction = 0,
        Explanation = 1,
        Example = 2,
        Question = 3,
        Hint = 4,
        StrongHint = 5,
        Success = 6,
        Failure = 7
    }

    /// <summary>
    /// Immutable voice request sent to a future TTS provider.
    /// </summary>
    public readonly struct SparkAIVoiceRequest
    {
        public bool IsValid { get; }

        public string ConceptId { get; }

        public string Text { get; }

        public SparkAIVoiceTextType TextType { get; }

        public SparkAIVoiceEmotion Emotion { get; }

        public float Speed { get; }

        public float Pitch { get; }

        public float CreatedAt { get; }

        public SparkAIVoiceRequest(
            bool isValid,
            string conceptId,
            string text,
            SparkAIVoiceTextType textType,
            SparkAIVoiceEmotion emotion,
            float speed,
            float pitch,
            float createdAt)
        {
            IsValid =
                isValid;

            ConceptId =
                conceptId;

            Text =
                text;

            TextType =
                textType;

            Emotion =
                emotion;

            Speed =
                speed;

            Pitch =
                pitch;

            CreatedAt =
                createdAt;
        }

        public static SparkAIVoiceRequest Invalid()
        {
            return new SparkAIVoiceRequest(
                false,
                string.Empty,
                string.Empty,
                SparkAIVoiceTextType.Explanation,
                SparkAIVoiceEmotion.Neutral,
                1f,
                0.5f,
                0f);
        }
    }
}