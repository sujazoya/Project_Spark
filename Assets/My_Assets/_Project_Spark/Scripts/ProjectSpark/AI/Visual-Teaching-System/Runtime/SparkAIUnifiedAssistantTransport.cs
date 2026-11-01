using System;
using UnityEngine;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Directly connects the existing SparkAIUnifiedAssistant.Ask/AnswerReady API
    /// to the verified Visual Teaching explanation pipeline.
    /// No modification to the assistant, simulator, or tutorial director is needed.
    /// All methods and callbacks must be called on Unity's main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIUnifiedAssistantTransport : MonoBehaviour, ISparkAIExplanationTransport
    {
        [Serializable]
        private sealed class ReplyEnvelope
        {
            public string requestId;
            public string answer;
            public string reasoning;
            public string nextCheck;
        }

        [Header("Existing Spark AI Assistant")]
        [SerializeField] private SparkAIUnifiedAssistant assistant;

        [Header("Transport")]
        [SerializeField, Range(5f, 90f)] private float timeoutSeconds = 75f;
        [SerializeField] private bool logResponses = false;

        private bool pending;
        private string pendingId;
        private string pendingQuestion;
        private float pendingSince;
        private Action<string, string> replyCallback;
        private Action<string, string> errorCallback;
        private SparkAIUnifiedAssistant subscribedAssistant;

        private void Awake() => Resolve();

        private void OnDisable()
        {
            CancelPending();
            Unsubscribe();
        }

        private void OnDestroy()
        {
            CancelPending();
            Unsubscribe();
        }

        private void Update()
        {
            if (!pending || Time.unscaledTime - pendingSince < timeoutSeconds)
                return;

            string id = pendingId;
            Action<string, string> failed = errorCallback;
            CancelPending();
            failed?.Invoke(id, "Spark AI did not answer before the transport timeout.");
        }

        private void Resolve()
        {
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(
                    FindObjectsInactive.Include);
        }

        public bool TrySendExplanation(
            string requestId,
            string prompt,
            Action<string, string> onJsonReply,
            Action<string, string> onError)
        {
            Resolve();

            if (!isActiveAndEnabled || assistant == null ||
                !assistant.isActiveAndEnabled ||
                string.IsNullOrWhiteSpace(requestId) ||
                string.IsNullOrWhiteSpace(prompt) ||
                onJsonReply == null)
                return false;

            // Do NOT replace an unanswered request: an external assistant's
            // completion event lacks a request ID, so concurrent requests
            // cannot be correlated safely through its existing API.
            if (pending)
                return false;

            Subscribe();
            if (subscribedAssistant == null)
                return false;

            pending = true;
            pendingId = requestId;
            pendingQuestion = prompt;
            pendingSince = Time.unscaledTime;
            replyCallback = onJsonReply;
            errorCallback = onError;

            try
            {
                // AnswerReady can fire synchronously for the local brain.
                // All state and listeners must already be prepared.
                assistant.Ask(prompt);
                return true;
            }
            catch (Exception exception)
            {
                if (pending)
                {
                    string id = pendingId;
                    Action<string, string> failed = errorCallback;
                    CancelPending();
                    failed?.Invoke(id, "Spark AI Ask failed: " + exception.Message);
                }
                return false;
            }
        }

        private void Subscribe()
        {
            if (subscribedAssistant == assistant)
                return;

            Unsubscribe();
            if (assistant == null)
                return;

            subscribedAssistant = assistant;
            subscribedAssistant.AnswerReady += OnAnswerReady;
        }

        private void Unsubscribe()
        {
            if (subscribedAssistant != null)
                subscribedAssistant.AnswerReady -= OnAnswerReady;
            subscribedAssistant = null;
        }

        private void OnAnswerReady(string question, string response)
        {
            // The assistant identifies answers with the original question.
            // The generated verified prompt contains a unique request ID,
            // so matching the full original prompt filters unrelated chat answers.
            if (!pending || !string.Equals(question, pendingQuestion,
                    StringComparison.Ordinal))
                return;

            string id = pendingId;
            Action<string, string> completed = replyCallback;
            Action<string, string> failed = errorCallback;

            if (string.IsNullOrWhiteSpace(response))
            {
                CancelPending();
                failed?.Invoke(id, "Spark AI returned an empty answer.");
                return;
            }

            // If an external brain supplied structured JSON with the matching
            // ID, preserve its actual reasoning and suggested next check.
            // Otherwise, wrap the assistant's existing plain-text reply.
            string payload;
            string trimmed = response.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                ReplyEnvelope structured = null;
                try { structured = JsonUtility.FromJson<ReplyEnvelope>(trimmed); }
                catch (Exception) { /* Treat non-JSON content as plain text. */ }

                if (structured != null &&
                    !string.IsNullOrWhiteSpace(structured.requestId))
                {
                    if (!string.Equals(structured.requestId, id,
                            StringComparison.Ordinal))
                    {
                        CancelPending();
                        failed?.Invoke(id, "Spark AI returned the wrong request ID.");
                        return;
                    }

                    payload = JsonUtility.ToJson(new ReplyEnvelope
                    {
                        requestId = id,
                        answer = Clean(structured.answer, 650),
                        reasoning = Clean(structured.reasoning, 650),
                        nextCheck = Clean(structured.nextCheck, 350)
                    });
                }
                else
                {
                    payload = WrapPlainText(id, response);
                }
            }
            else
            {
                payload = WrapPlainText(id, response);
            }

            CancelPending();
            if (logResponses)
                Debug.Log("[Spark AI Direct] Assistant reply delivered to verified bridge.", this);
            completed?.Invoke(id, payload);
        }

        private static string WrapPlainText(string id, string response)
        {
            // Do not invent reasoning or measurements. The verified
            // explanation panel supplies its own neutral fallback text.
            return JsonUtility.ToJson(new ReplyEnvelope
            {
                requestId = id,
                answer = Clean(response, 650),
                reasoning = "",
                nextCheck = ""
            });
        }

        private static string Clean(string text, int limit)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var builder = new System.Text.StringBuilder(Mathf.Min(text.Length, limit));
            foreach (char c in text)
            {
                if (builder.Length >= limit) break;
                if (!char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
                    builder.Append(c);
            }
            return builder.ToString();
        }

        private void CancelPending()
        {
            pending = false;
            pendingId = null;
            pendingQuestion = null;
            replyCallback = null;
            errorCallback = null;
        }
    }
}
