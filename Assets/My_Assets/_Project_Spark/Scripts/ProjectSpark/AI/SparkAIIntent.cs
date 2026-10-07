using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic player-intent inference for Spark AI.
    ///
    /// This layer interprets observed player activity.
    /// It does not control the player or modify the circuit.
    ///
    /// Intent is probabilistic in meaning, but deterministic in
    /// implementation: the same observed state produces the same result.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIIntent : MonoBehaviour
    {
        [Header("Spark AI")]
        [SerializeField] private SparkAIWorld world;
        [SerializeField] private SparkAIBrain brain;
        [SerializeField] private SparkAIPlayerActivity playerActivity;

        private SparkAIIntentResult currentIntent;
        private bool initialized;

        public bool IsInitialized => initialized;
        public SparkAIIntentResult CurrentIntent => currentIntent;

        public event EventHandler<SparkAIIntentChangedEventArgs>
            IntentChanged;

        private void Awake()
        {
            ResolveReferences();

            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Intent] SparkAIWorld reference is missing.",
                    this);

                return;
            }

            if (brain == null)
            {
                Debug.LogError(
                    "[Spark AI Intent] SparkAIBrain reference is missing.",
                    this);

                return;
            }

            if (playerActivity == null)
            {
                Debug.LogError(
                    "[Spark AI Intent] SparkAIPlayerActivity reference is missing.",
                    this);

                return;
            }

            playerActivity.ActivityChanged -= HandleActivityChanged;
            playerActivity.ActivityChanged += HandleActivityChanged;

            brain.ReasoningChanged -= HandleReasoningChanged;
            brain.ReasoningChanged += HandleReasoningChanged;

            initialized = true;

            EvaluateNow();
        }

        private void OnDestroy()
        {
            if (playerActivity != null)
                playerActivity.ActivityChanged -= HandleActivityChanged;

            if (brain != null)
                brain.ReasoningChanged -= HandleReasoningChanged;
        }

        private void ResolveReferences()
        {
            // Explicit references only.
            //
            // No FindObjectOfType.
            // No FindFirstObjectByType.
        }

        /// <summary>
        /// Re-evaluates the player's likely intent.
        /// </summary>
        public SparkAIIntentResult EvaluateNow()
        {
            if (!initialized &&
                (world == null ||
                 brain == null ||
                 playerActivity == null))
            {
                return SetIntent(
                    SparkAIIntentResult.Unavailable(
                        "Player intent cannot currently be determined."));
            }

            SparkAIWorldSnapshot snapshot =
                world.CaptureSnapshot();

            SparkAIReasoningResult reasoning =
                brain.CurrentReasoning;

            SparkAIPlayerActivitySnapshot activity =
                playerActivity.CurrentActivity;

            SparkAIIntentResult result =
                InferIntent(
                    snapshot,
                    reasoning,
                    activity);

            return SetIntent(result);
        }

        private void HandleActivityChanged(
            object sender,
            SparkAIPlayerActivityChangedEventArgs args)
        {
            EvaluateNow();
        }

        private void HandleReasoningChanged(
            object sender,
            SparkAIReasoningChangedEventArgs args)
        {
            EvaluateNow();
        }

        private SparkAIIntentResult InferIntent(
            SparkAIWorldSnapshot snapshot,
            SparkAIReasoningResult reasoning,
            SparkAIPlayerActivitySnapshot activity)
        {
            if (!reasoning.IsValid)
            {
                return SparkAIIntentResult.Unavailable(
                    "Spark AI does not have enough world information to infer player intent.");
            }

            switch (activity.Type)
            {
                case SparkAIPlayerActivityType.ConnectionCreated:
                    return InferConnectionIntent(
                        snapshot,
                        reasoning,
                        activity);

                case SparkAIPlayerActivityType.ConnectionRemoved:
                    return InferDisconnectionIntent(
                        snapshot,
                        reasoning,
                        activity);

                case SparkAIPlayerActivityType.MeasurementTaken:
                    return InferMeasurementIntent(
                        activity);

                case SparkAIPlayerActivityType.ToolUsed:
                    return InferToolIntent(
                        activity);

                case SparkAIPlayerActivityType.ObjectSelected:
                    return InferSelectionIntent(
                        activity);

                case SparkAIPlayerActivityType.SwitchChanged:
                    return new SparkAIIntentResult(
                        true,
                        SparkAIIntentType.ChangingCircuitState,
                        0.80f,
                        "The player is likely trying to change the circuit's electrical state.",
                        activity);

                case SparkAIPlayerActivityType.ComponentInteracted:
                    return new SparkAIIntentResult(
                        true,
                        SparkAIIntentType.InteractingWithComponent,
                        0.65f,
                        "The player is interacting with a circuit component.",
                        activity);

                default:
                    return new SparkAIIntentResult(
                        true,
                        SparkAIIntentType.Unknown,
                        0f,
                        "The player's current intent is not yet clear.",
                        activity);
            }
        }

        private SparkAIIntentResult InferConnectionIntent(
            SparkAIWorldSnapshot snapshot,
            SparkAIReasoningResult reasoning,
            SparkAIPlayerActivitySnapshot activity)
        {
            if (reasoning.State ==
                SparkAIReasoningState.WrongConnection)
            {
                return new SparkAIIntentResult(
                    true,
                    SparkAIIntentType.TryingToConnectCircuit,
                    0.90f,
                    "The player appears to be trying to connect the circuit, but the latest connection is incorrect.",
                    activity);
            }

            if (reasoning.State ==
                SparkAIReasoningState.SourceShort)
            {
                return new SparkAIIntentResult(
                    true,
                    SparkAIIntentType.TryingToPowerCircuit,
                    0.88f,
                    "The player appears to be trying to power the circuit, but the latest connection has created a source short.",
                    activity);
            }

            if (reasoning.State ==
                SparkAIReasoningState.InProgress)
            {
                return new SparkAIIntentResult(
                    true,
                    SparkAIIntentType.TryingToCompleteObjective,
                    0.82f,
                    "The player appears to be building the circuit toward the current objective.",
                    activity);
            }

            if (reasoning.State ==
                SparkAIReasoningState.NoValidPowerSource)
            {
                return new SparkAIIntentResult(
                    true,
                    SparkAIIntentType.TryingToPowerCircuit,
                    0.78f,
                    "The player appears to be trying to establish a usable power path.",
                    activity);
            }

            return new SparkAIIntentResult(
                true,
                SparkAIIntentType.TryingToConnectCircuit,
                0.60f,
                "The player appears to be building or modifying the circuit.",
                activity);
        }

        private SparkAIIntentResult InferDisconnectionIntent(
            SparkAIWorldSnapshot snapshot,
            SparkAIReasoningResult reasoning,
            SparkAIPlayerActivitySnapshot activity)
        {
            if (reasoning.State ==
                SparkAIReasoningState.WrongConnection ||
                reasoning.State ==
                SparkAIReasoningState.SourceShort ||
                reasoning.State ==
                SparkAIReasoningState.TargetShort)
            {
                return new SparkAIIntentResult(
                    true,
                    SparkAIIntentType.TroubleshootingCircuit,
                    0.90f,
                    "The player appears to be removing a connection to troubleshoot the circuit.",
                    activity);
            }

            return new SparkAIIntentResult(
                true,
                SparkAIIntentType.ModifyingCircuit,
                0.65f,
                "The player appears to be modifying the circuit.",
                activity);
        }

        private SparkAIIntentResult InferMeasurementIntent(
            SparkAIPlayerActivitySnapshot activity)
        {
            string measurement =
                string.IsNullOrWhiteSpace(activity.Context)
                    ? "an electrical quantity"
                    : activity.Context;

            return new SparkAIIntentResult(
                true,
                SparkAIIntentType.MeasuringCircuit,
                0.95f,
                $"The player is measuring {measurement}.",
                activity);
        }

        private SparkAIIntentResult InferToolIntent(
            SparkAIPlayerActivitySnapshot activity)
        {
            string tool =
                string.IsNullOrWhiteSpace(activity.Context)
                    ? "a tool"
                    : activity.Context;

            return new SparkAIIntentResult(
                true,
                SparkAIIntentType.UsingTool,
                0.80f,
                $"The player is using {tool}.",
                activity);
        }

        private SparkAIIntentResult InferSelectionIntent(
            SparkAIPlayerActivitySnapshot activity)
        {
            return new SparkAIIntentResult(
                true,
                SparkAIIntentType.InspectingObject,
                0.70f,
                "The player appears to be inspecting or preparing to interact with an object.",
                activity);
        }

        private SparkAIIntentResult SetIntent(
            SparkAIIntentResult result)
        {
            currentIntent = result;

            IntentChanged?.Invoke(
                this,
                new SparkAIIntentChangedEventArgs(result));

            return result;
        }
    }

    /// <summary>
    /// High-level interpretation of what the player is trying to do.
    /// </summary>
    public enum SparkAIIntentType
    {
        Unknown = 0,

        InspectingObject = 1,

        TryingToConnectCircuit = 2,

        TryingToPowerCircuit = 3,

        TryingToCompleteObjective = 4,

        ModifyingCircuit = 5,

        TroubleshootingCircuit = 6,

        MeasuringCircuit = 7,

        UsingTool = 8,

        ChangingCircuitState = 9,

        InteractingWithComponent = 10
    }

    /// <summary>
    /// Immutable result of player-intent inference.
    /// </summary>
    public readonly struct SparkAIIntentResult
    {
        public bool IsValid { get; }

        public SparkAIIntentType Type { get; }

        public float Confidence { get; }

        public string Explanation { get; }

        public SparkAIPlayerActivitySnapshot Activity { get; }

        public SparkAIIntentResult(
            bool isValid,
            SparkAIIntentType type,
            float confidence,
            string explanation,
            SparkAIPlayerActivitySnapshot activity)
        {
            IsValid = isValid;
            Type = type;
            Confidence = Mathf.Clamp01(confidence);
            Explanation = explanation ?? string.Empty;
            Activity = activity;
        }

        public static SparkAIIntentResult Unavailable(
            string explanation)
        {
            return new SparkAIIntentResult(
                false,
                SparkAIIntentType.Unknown,
                0f,
                explanation,
                SparkAIPlayerActivitySnapshot.CreateIdle(Time.time));
        }
    }

    public sealed class SparkAIIntentChangedEventArgs : EventArgs
    {
        public SparkAIIntentResult Intent { get; }

        public SparkAIIntentChangedEventArgs(
            SparkAIIntentResult intent)
        {
            Intent = intent;
        }
    }
}