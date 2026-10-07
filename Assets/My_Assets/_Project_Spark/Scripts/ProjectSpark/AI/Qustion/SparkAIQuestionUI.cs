using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AI
{
    /// <summary>
    /// UI bridge for asking Project Spark AI questions.
    ///
    /// This component does not reason about the circuit.
    /// It only sends player text to SparkAIQuestionController
    /// and displays the returned answer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIQuestionUI : MonoBehaviour
    {
        [Header("AI")]

        [SerializeField]
        private SparkAIQuestionController questionController;

        [Header("UI")]

        [SerializeField]
        private TMP_InputField questionInput;

        [SerializeField]
        private Button askButton;

        [SerializeField]
        private TMP_Text answerText;

        [Header("Settings")]

        [SerializeField]
        private bool clearInputAfterAsk = false;

        [SerializeField]
        private bool showPlayerQuestion = false;

        private bool initialized;

        private void Awake()
        {
            initialized = false;

            if (questionController == null)
            {
                Debug.LogError(
                    "[Spark AI Question UI] " +
                    "Question Controller is not assigned.",
                    this);

                return;
            }

            if (questionInput == null)
            {
                Debug.LogError(
                    "[Spark AI Question UI] " +
                    "Question Input is not assigned.",
                    this);

                return;
            }

            if (askButton == null)
            {
                Debug.LogError(
                    "[Spark AI Question UI] " +
                    "Ask Button is not assigned.",
                    this);

                return;
            }

            if (answerText == null)
            {
                Debug.LogError(
                    "[Spark AI Question UI] " +
                    "Answer Text is not assigned.",
                    this);

                return;
            }

            askButton.onClick.AddListener(
                HandleAskButtonClicked);

            questionController.AnswerCreated +=
                HandleAnswerCreated;

            initialized = true;
        }

        private void OnDestroy()
        {
            if (askButton != null)
            {
                askButton.onClick.RemoveListener(
                    HandleAskButtonClicked);
            }

            if (questionController != null)
            {
                questionController.AnswerCreated -=
                    HandleAnswerCreated;
            }
        }

        private void HandleAskButtonClicked()
        {
            if (!initialized)
                return;

            string question =
                questionInput.text;

            if (string.IsNullOrWhiteSpace(question))
            {
                answerText.text =
                    "Please type a question first.";

                return;
            }

            if (showPlayerQuestion)
            {
                answerText.text =
                    "You: " + question;
            }

            questionController.AskQuestion(
                question);

            if (clearInputAfterAsk)
            {
                questionInput.text =
                    string.Empty;
            }
        }

        private void HandleAnswerCreated(
            string answer)
        {
            if (answerText == null)
                return;

            answerText.text =
                "AI: " + answer;
        }
    }
}