using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Connects the existing Ask Spark UI/assistant to a private AI backend.
    /// Creates no UI and never modifies the circuit. LocalAnswer remains fallback.
    /// Attach alongside SparkAIUnifiedAssistant and assign references in Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIHybridBridge : MonoBehaviour
    {
        [Header("Existing Gameplay Components")]
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private LevelGamePlayManager levelManager;

        [Header("Private Backend (Development: same computer)")]
        [SerializeField] private string endpoint = "http://127.0.0.1:8765/ask";
        [SerializeField, Range(4, 30)] private int requestTimeoutSeconds = 12;
        [SerializeField, Range(0, 12)] private int rememberedTurns = 5;
        [SerializeField, Min(1000)] private int maxContextChars = 24000;
        [SerializeField] private bool debugErrors = true;

        [Serializable]
        private class Turn
        {
            public string role;
            public string content;
            public Turn(string role, string content)
            {
                this.role = role;
                this.content = content;
            }
        }

        [Serializable]
        private class AskRequest
        {
            public string question;
            public string context;
            public string level_id;
            public Turn[] history;
        }

        [Serializable]
        private class AskResponse
        {
            public string answer;
        }

        private readonly List<Turn> history = new List<Turn>();
        private Coroutine currentRequest;
        private string activeLevelId = "";
        private string latestQuestion;
        private int generation;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (assistant != null)
            {
                assistant.ExternalQuestionRequested += OnQuestion;
                assistant.AnswerReady += OnAnswerReady;
            }
        }

        private void OnDisable()
        {
            if (assistant != null)
            {
                assistant.ExternalQuestionRequested -= OnQuestion;
                assistant.AnswerReady -= OnAnswerReady;
            }

            generation++;
            if (currentRequest != null)
            {
                StopCoroutine(currentRequest);
                currentRequest = null;
            }
            latestQuestion = null;
        }

        private void ResolveReferences()
        {
            if (assistant == null)
                assistant = GetComponent<SparkAIUnifiedAssistant>();
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
        }

        private void OnQuestion(string question, string verifiedContext)
        {
            ResolveReferences();
            if (assistant == null) return;

            string currentLevel = levelManager != null &&
                levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId : "";

            if (currentLevel != activeLevelId)
            {
                history.Clear();
                activeLevelId = currentLevel;
            }

            generation++;
            if (currentRequest != null)
                StopCoroutine(currentRequest);

            latestQuestion = question;
            int requestId = generation;
            currentRequest = StartCoroutine(
                SendQuestion(requestId, question, verifiedContext));
        }

        private IEnumerator SendQuestion(
            int requestId, string question, string verifiedContext)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                FinishWithLocalFallback(requestId, "No backend endpoint assigned.");
                yield break;
            }

            // The snapshot is created by the existing game assistant.
            // It describes live solver/evaluator data, not model guesses.
            string context = verifiedContext ?? "";
            if (context.Length > maxContextChars)
                context = context.Substring(0, maxContextChars) +
                    "\n[Snapshot truncated; missing details must not be guessed.]";

            AskRequest payload = new AskRequest
            {
                question = question,
                context = context,
                level_id = activeLevelId,
                history = history.ToArray()
            };

            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonUtility.ToJson(payload));

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = requestTimeoutSeconds;

                yield return request.SendWebRequest();

                if (requestId != generation)
                    yield break;

                currentRequest = null;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    FinishWithLocalFallback(requestId,
                        "Backend error: " + request.error);
                    yield break;
                }

                AskResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<AskResponse>(
                        request.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    if (debugErrors)
                        Debug.LogWarning("[Spark Hybrid] Invalid backend reply: " + ex.Message, this);
                }

                if (response == null ||
                    string.IsNullOrWhiteSpace(response.answer))
                {
                    FinishWithLocalFallback(requestId,
                        "Backend returned an empty or invalid answer.");
                    yield break;
                }

                // Do not call the assistant from a worker thread.
                // Unity coroutines resume on the main thread.
                assistant.DeliverExternalAnswer(response.answer.Trim());
            }
        }

        private void FinishWithLocalFallback(int requestId, string reason)
        {
            if (requestId != generation || assistant == null)
                return;

            currentRequest = null;
            if (debugErrors)
                Debug.LogWarning("[Spark Hybrid] " + reason + " Using local answer.", this);

            // DeliverExternalAnswer(string.Empty) invokes LocalAnswer
            // using the assistant's original pending question.
            assistant.DeliverExternalAnswer(string.Empty);
        }

        private void OnAnswerReady(string question, string answer)
        {
            // Only remember the latest response, not out-of-order replies.
            if (latestQuestion == null || question != latestQuestion)
                return;

            history.Add(new Turn("user", question));
            history.Add(new Turn("assistant", answer ?? ""));
            latestQuestion = null;

            int maxMessages = rememberedTurns * 2;
            if (maxMessages == 0)
                history.Clear();
            else
                while (history.Count > maxMessages)
                    history.RemoveAt(0);
        }
    }
}
