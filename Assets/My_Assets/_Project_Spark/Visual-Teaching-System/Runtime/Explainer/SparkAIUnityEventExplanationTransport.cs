using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Inspector-friendly transport for an existing assistant that has:
    ///  - a public method taking a string prompt, and
    ///  - a callback that can provide a JSON string on completion.
    /// Wire Send Prompt in Inspector and call ReceiveAssistantJson from that callback.
    /// The response JSON MUST echo its requestId so late replies cannot be misattributed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIUnityEventExplanationTransport : MonoBehaviour, ISparkAIExplanationTransport
    {
        [Serializable] public sealed class PromptEvent : UnityEvent<string> { }
        [Serializable] private sealed class RequestHeader { public string requestId; }

        [Header("Connect to existing AI assistant method accepting a string")]
        [SerializeField] private PromptEvent sendPrompt = new PromptEvent();
        [SerializeField, Range(5f, 300f)] private float timeoutSeconds = 90f;

        private string activeId;
        private float sentAt;
        private Action<string, string> onReply;
        private Action<string, string> onError;

        public PromptEvent SendPrompt => sendPrompt;

        public bool TrySendExplanation(string requestId, string prompt,
            Action<string, string> onJsonReply, Action<string, string> onFailed)
        {
            if (!isActiveAndEnabled || string.IsNullOrWhiteSpace(requestId) ||
                string.IsNullOrWhiteSpace(prompt) || onJsonReply == null)
                return false;

            // New questions supersede previous ones. Old responses are checked by ID.
            activeId = requestId;
            sentAt = Time.unscaledTime;
            onReply = onJsonReply;
            onError = onFailed;

            try
            {
                sendPrompt.Invoke(prompt);
                return true;
            }
            catch (Exception ex)
            {
                string id = activeId;
                ClearPending();
                onFailed?.Invoke(id, "Assistant event failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Call from the existing assistant after it produces strict reply JSON.
        /// Example: {"requestId":"...","answer":"...","reasoning":"...","nextCheck":"..."}
        /// </summary>
        public void ReceiveAssistantJson(string json)
        {
            if (string.IsNullOrWhiteSpace(activeId) || string.IsNullOrWhiteSpace(json))
                return;

            RequestHeader header;
            try { header = JsonUtility.FromJson<RequestHeader>(json); }
            catch (Exception) { return; }
            if (header == null ||
                !string.Equals(header.requestId, activeId, StringComparison.Ordinal))
                return;

            string completedId = activeId;
            Action<string, string> callback = onReply;
            ClearPending();
            callback?.Invoke(completedId, json);
        }

        /// <summary>
        /// For an assistant that returns plain text, use an adapter that supplies the
        /// originating requestId explicitly. Avoid using a global, uncorrelated callback.
        /// </summary>
        public void ReceivePlainText(string requestId, string answer)
        {
            if (!string.Equals(requestId, activeId, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(answer)) return;
            string completedId = activeId;
            Action<string, string> callback = onReply;
            var payload = new TextEnvelope
            {
                requestId = completedId,
                answer = Trim(answer, 650),
                reasoning = "AI-generated explanation. Validate against live measurements.",
                nextCheck = "Check the component and run the circuit simulation again."
            };
            string json = JsonUtility.ToJson(payload);
            ClearPending();
            callback?.Invoke(completedId, json);
        }

        [Serializable] private sealed class TextEnvelope
        {
            public string requestId;
            public string answer;
            public string reasoning;
            public string nextCheck;
        }

        private static string Trim(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return "";
            // Remove invalid control chars that the verifier rejects.
            value = value.Replace("\0", " ");
            return value.Length <= max ? value : value.Substring(0, max);
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(activeId) ||
                Time.unscaledTime - sentAt <= timeoutSeconds) return;
            string id = activeId;
            Action<string, string> failed = onError;
            ClearPending();
            failed?.Invoke(id, "The assistant response timed out.");
        }

        private void OnDisable() => ClearPending();

        private void ClearPending()
        {
            activeId = null;
            onReply = null;
            onError = null;
        }
    }
}
