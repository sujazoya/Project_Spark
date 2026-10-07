using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Presents the current AI teaching session.
    ///
    /// Responsibilities:
    /// - Reads the authoritative teaching session.
    /// - Exposes the current teaching text to UI/character systems.
    /// - Sends teaching text to the voice controller.
    ///
    /// This class does NOT:
    /// - decide what should be taught
    /// - evaluate the player
    /// - evaluate circuits
    /// - modify gameplay
    /// - modify learner mastery
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITeacherController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SparkAITeachingSession teachingSession;

        [SerializeField]
        private SparkAIVoiceController voiceController;

        [Header("Presentation")]
        [SerializeField]
        private bool speakAutomatically = true;

        [SerializeField]
        private bool speakQuestions = true;

        [SerializeField]
        private bool speakHints = true;

        [SerializeField]
        private bool speakStrongHints = true;

        [SerializeField]
        private bool speakExplanations = true;

        [SerializeField]
        private bool speakSuccessMessages = true;

        [SerializeField]
        private bool speakFailureMessages = true;

        [Header("Debug")]
        [SerializeField]
        private bool logTeacherPresentation;

        private string currentConceptId =
            string.Empty;

        private string currentText =
            string.Empty;

        private string currentObjective =
            string.Empty;

        private SparkAITeachingSessionState currentState =
            SparkAITeachingSessionState.Idle;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        public string CurrentConceptId =>
            currentConceptId;

        public string CurrentText =>
            currentText;

        public string CurrentObjective =>
            currentObjective;

        public SparkAITeachingSessionState CurrentState =>
            currentState;

        public bool IsTeaching =>
            teachingSession != null &&
            teachingSession.IsActive;

        public event Action<string> TeacherMessageChanged;

        public event Action<
            SparkAITeachingSessionState> TeacherStateChanged;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Initializes the presentation layer.
        /// </summary>
        public void Initialize()
        {
            if (initialized)
                return;

            if (teachingSession == null)
            {
                Debug.LogWarning(
                    "[AI TEACHER] Teaching session is not assigned.",
                    this);
            }

            if (voiceController == null)
            {
                Debug.LogWarning(
                    "[AI TEACHER] Voice controller is not assigned.",
                    this);
            }

            if (teachingSession != null)
            {
                currentState =
                    teachingSession.State;
            }

            initialized = true;
        }

        private void Subscribe()
        {
            if (teachingSession == null)
                return;

            teachingSession.StateChanged +=
                HandleTeachingStateChanged;

            teachingSession.TeachingChanged +=
                HandleTeachingChanged;
        }

        private void Unsubscribe()
        {
            if (teachingSession == null)
                return;

            teachingSession.StateChanged -=
                HandleTeachingStateChanged;

            teachingSession.TeachingChanged -=
                HandleTeachingChanged;
        }

        // =========================================================
        // TEACHING EVENTS
        // =========================================================

        private void HandleTeachingStateChanged(
            SparkAITeachingSessionState state)
        {
            currentState =
                state;

            TeacherStateChanged?.Invoke(
                state);

            if (state ==
                SparkAITeachingSessionState.Understood)
            {
                RefreshPresentation(false);
                return;
            }

            if (state ==
                SparkAITeachingSessionState.Completed)
            {
                RefreshPresentation(false);
                return;
            }

            if (state ==
                SparkAITeachingSessionState.Cancelled)
            {
                ClearPresentation();
                return;
            }

            RefreshPresentation(
                speakAutomatically);
        }

        private void HandleTeachingChanged(
            SparkAITeachingDecision decision)
        {
            RefreshPresentation(
                speakAutomatically);
        }

        // =========================================================
        // PRESENTATION
        // =========================================================

        /// <summary>
        /// Refreshes the visible teacher information from the
        /// authoritative teaching session.
        /// </summary>
        public bool RefreshPresentation(
            bool speak)
        {
            if (teachingSession == null)
                return false;

            currentState =
                teachingSession.State;

            currentText =
                teachingSession.GetCurrentTeachingText();

            currentObjective =
                teachingSession.GetCurrentObjective();

            string[] conceptIds =
                teachingSession.GetCurrentConceptIds();

            currentConceptId =
                GetPrimaryConceptId(
                    conceptIds);

            TeacherMessageChanged?.Invoke(
                currentText);

            if (logTeacherPresentation)
            {
                Debug.Log(
                    $"[AI TEACHER] " +
                    $"State={currentState} | " +
                    $"Concept={currentConceptId} | " +
                    $"Objective={currentObjective} | " +
                    $"Text={currentText}",
                    this);
            }

            if (!speak)
                return true;

            return SpeakCurrentState();
        }

        /// <summary>
        /// Speaks the appropriate voice line for the current
        /// teaching state.
        /// </summary>
        public bool SpeakCurrentState()
        {
            if (voiceController == null)
                return false;

            if (string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            switch (currentState)
            {
                case SparkAITeachingSessionState.Question:

                    if (!speakQuestions)
                        return false;

                    return voiceController.SpeakQuestion(
                        currentConceptId);

                case SparkAITeachingSessionState.Hint:

                    if (!speakHints)
                        return false;

                    return voiceController.SpeakHint(
                        currentConceptId,
                        false);

                case SparkAITeachingSessionState.StrongHint:

                    if (!speakStrongHints)
                        return false;

                    return voiceController.SpeakHint(
                        currentConceptId,
                        true);

                case SparkAITeachingSessionState.Explanation:

                    if (!speakExplanations)
                        return false;

                    return voiceController.SpeakExplanation(
                        currentConceptId);

                default:
                    return false;
            }
        }

        // =========================================================
        // MANUAL VOICE CONTROL
        // =========================================================

        public bool SpeakQuestion()
        {
            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakQuestion(
                currentConceptId);
        }

        public bool SpeakHint()
        {
            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakHint(
                currentConceptId,
                false);
        }

        public bool SpeakStrongHint()
        {
            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakHint(
                currentConceptId,
                true);
        }

        public bool SpeakExplanation()
        {
            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakExplanation(
                currentConceptId);
        }

        public bool SpeakSuccess()
        {
            if (!speakSuccessMessages)
                return false;

            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakSuccess(
                currentConceptId);
        }

        public bool SpeakFailure()
        {
            if (!speakFailureMessages)
                return false;

            if (voiceController == null ||
                string.IsNullOrWhiteSpace(
                    currentConceptId))
            {
                return false;
            }

            return voiceController.SpeakFailure(
                currentConceptId);
        }

        public void StopVoice()
        {
            if (voiceController == null)
                return;

            voiceController.StopVoice();
        }

        // =========================================================
        // CLEAR
        // =========================================================

        public void ClearPresentation()
        {
            StopVoice();

            currentConceptId =
                string.Empty;

            currentText =
                string.Empty;

            currentObjective =
                string.Empty;

            currentState =
                SparkAITeachingSessionState.Idle;

            TeacherMessageChanged?.Invoke(
                string.Empty);

            TeacherStateChanged?.Invoke(
                currentState);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private string GetPrimaryConceptId(
            string[] conceptIds)
        {
            if (conceptIds == null ||
                conceptIds.Length == 0)
            {
                return string.Empty;
            }

            for (int i = 0;
                 i < conceptIds.Length;
                 i++)
            {
                if (!string.IsNullOrWhiteSpace(
                        conceptIds[i]))
                {
                    return conceptIds[i];
                }
            }

            return string.Empty;
        }
    }
}