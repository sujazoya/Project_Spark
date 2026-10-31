using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Bridges verified prompt generation to an existing assistant through a typed transport.
    /// Does NOT edit the assistant, solver, or visual-teaching scripts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIExplanationAssistantConnector : MonoBehaviour
    {
        [Header("Existing verified explanation bridge")]
        [SerializeField] private SparkAIExplanationRequestBridge explanationBridge;

        [Header("MonoBehaviour implementing ISparkAIExplanationTransport")]
        [SerializeField] private MonoBehaviour assistantTransport;

        [Header("Options")]
        [SerializeField] private bool logConnectionMessages = true;

        private ISparkAIExplanationTransport transport;
        private int requestGeneration;
        private string latestRequestId;
        private bool subscribed;
        private bool activeRequest;

        private void Awake() => Resolve();

        private void OnEnable()
        {
            Resolve();
            Bind();
        }

        private void Start()
        {
            Resolve();
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
            InvalidatePending();
        }

        private void OnDestroy()
        {
            Unbind();
            InvalidatePending();
        }

        private void Resolve()
        {
            if (explanationBridge == null)
                explanationBridge = GetComponent<SparkAIExplanationRequestBridge>();

            if (explanationBridge == null)
                explanationBridge = FindFirstObjectByType<SparkAIExplanationRequestBridge>(
                    FindObjectsInactive.Include);

            if (assistantTransport == null)
            {
                var local = GetComponents<MonoBehaviour>();
                foreach (var component in local)
                {
                    if (component != null && !ReferenceEquals(component, this) &&
                        component is ISparkAIExplanationTransport)
                    {
                        assistantTransport = component;
                        break;
                    }
                }
            }

            transport = assistantTransport as ISparkAIExplanationTransport;
        }

        private void Bind()
        {
            if (subscribed || explanationBridge == null) return;
            explanationBridge.PromptReady.AddListener(HandleVerifiedPrompt);
            subscribed = true;
        }

        private void Unbind()
        {
            if (subscribed && explanationBridge != null)
                explanationBridge.PromptReady.RemoveListener(HandleVerifiedPrompt);
            subscribed = false;
        }

        private void HandleVerifiedPrompt(string prompt)
        {
            Resolve();

            if (explanationBridge == null || !isActiveAndEnabled ||
                !explanationBridge.HasPendingRequest)
                return;

            string id = explanationBridge.PendingRequestId;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(prompt))
                return;

            if (transport == null)
            {
                Debug.LogWarning(
                    "[Spark AI Connection] Assign a component implementing " +
                    "ISparkAIExplanationTransport. The question was not sent.", this);
                return;
            }

            // Increment before dispatch: synchronous completions are supported.
            int generation = ++requestGeneration;
            latestRequestId = id;
            activeRequest = true;

            bool accepted;
            try
            {
                accepted = transport.TrySendExplanation(
                    id, prompt,
                    (replyId, replyJson) => ReceiveReply(generation, replyId, replyJson),
                    (errorId, error) => ReceiveError(generation, errorId, error));
            }
            catch (Exception ex)
            {
                accepted = false;
                Debug.LogError("[Spark AI Connection] Request dispatch exception: " + ex.Message, this);
            }

            if (!accepted && generation == requestGeneration && activeRequest)
            {
                activeRequest = false;
                Debug.LogWarning("[Spark AI Connection] Assistant transport rejected the request.", this);
            }
            else if (accepted && logConnectionMessages)
            {
                Debug.Log("[Spark AI Connection] Verified explanation request dispatched.", this);
            }
        }

        private void ReceiveReply(int generation, string id, string json)
        {
            // Unity main-thread callback is required by the interface.
            if (generation != requestGeneration || !activeRequest ||
                !isActiveAndEnabled || explanationBridge == null ||
                !string.Equals(id, latestRequestId, StringComparison.Ordinal) ||
                !string.Equals(explanationBridge.PendingRequestId, id, StringComparison.Ordinal))
                return;

            activeRequest = false;
            bool accepted = explanationBridge.ReceiveAssistantReplyJson(json);
            if (!accepted)
                Debug.LogWarning("[Spark AI Connection] Assistant reply was rejected by verified bridge.", this);
        }

        private void ReceiveError(int generation, string id, string error)
        {
            if (generation != requestGeneration || !activeRequest ||
                !string.Equals(id, latestRequestId, StringComparison.Ordinal))
                return;

            activeRequest = false;
            Debug.LogWarning("[Spark AI Connection] " +
                (string.IsNullOrEmpty(error) ? "Assistant request failed." : error), this);
        }

        private void InvalidatePending()
        {
            ++requestGeneration;
            latestRequestId = null;
            activeRequest = false;
        }

        /// <summary>Assign a transport at runtime, then optionally call Rebind().</summary>
        public bool SetTransport(MonoBehaviour component)
        {
            if (!(component is ISparkAIExplanationTransport)) return false;
            assistantTransport = component;
            transport = (ISparkAIExplanationTransport)component;
            return true;
        }

        public void Rebind()
        {
            Unbind();
            Resolve();
            if (isActiveAndEnabled) Bind();
        }
    }
}
