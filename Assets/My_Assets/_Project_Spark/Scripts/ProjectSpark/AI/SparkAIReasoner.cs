using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic reasoning layer for Spark AI.
    ///
    /// Converts the authoritative AI world snapshot into a
    /// high-level interpretation of what is happening.
    ///
    /// This class does not modify Project Spark systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIReasoner : MonoBehaviour
    {
        [SerializeField]
        private SparkAIWorld world;

        private bool initialized;

        public bool IsInitialized => initialized;

        private void Awake()
        {
            if (world == null)
            {
                Debug.LogError(
                    "[Spark AI Reasoner] SparkAIWorld reference is missing.",
                    this);

                return;
            }

            initialized = true;
        }

        /// <summary>
        /// Captures the latest world state and reasons over it.
        /// </summary>
        public SparkAIReasoningResult ReasonNow()
        {
            if (world == null)
            {
                return BuildUnavailableResult();
            }

            SparkAIWorldSnapshot snapshot =
                world.CaptureSnapshot();

            return Reason(snapshot);
        }

        /// <summary>
        /// Reasons over a previously captured world snapshot.
        /// </summary>
        public SparkAIReasoningResult Reason(
            SparkAIWorldSnapshot snapshot)
        {
            return BuildReasoning(snapshot);
        }

        private SparkAIReasoningResult BuildReasoning(
            SparkAIWorldSnapshot snapshot)
        {
            SparkAILevelSnapshot level = snapshot.Level;
            SparkAIPlayerSnapshot player = snapshot.Player;

            if (!level.HasLevel)
            {
                return CreateResult(
                    SparkAIReasoningState.NoActiveLevel,
                    "There is no active Project Spark level.",
                    level,
                    player);
            }

            if (level.Completed)
            {
                return CreateResult(
                    SparkAIReasoningState.LevelCompleted,
                    "The active level has been completed.",
                    level,
                    player);
            }

            if (level.Failed)
            {
                string message =
                    !string.IsNullOrWhiteSpace(level.FailureReason)
                        ? level.FailureReason
                        : "The active level has failed.";

                return CreateResult(
                    SparkAIReasoningState.LevelFailed,
                    message,
                    level,
                    player);
            }

            if (level.WrongConnection)
            {
                return CreateResult(
                    SparkAIReasoningState.WrongConnection,
                    BuildPlayerAwareMessage(
                        "The circuit contains an incorrect connection.",
                        player),
                    level,
                    player);
            }

            if (level.SourceShorted)
            {
                return CreateResult(
                    SparkAIReasoningState.SourceShort,
                    BuildPlayerAwareMessage(
                        "The power source appears to be shorted.",
                        player),
                    level,
                    player);
            }

            if (level.TargetShorted)
            {
                return CreateResult(
                    SparkAIReasoningState.TargetShort,
                    BuildPlayerAwareMessage(
                        "The target appears to be shorted.",
                        player),
                    level,
                    player);
            }

            if (!level.HasValidPowerSource)
            {
                return CreateResult(
                    SparkAIReasoningState.NoValidPowerSource,
                    BuildPlayerAwareMessage(
                        "There is no valid active power source.",
                        player),
                    level,
                    player);
            }

            if (level.TotalTargets > 0 &&
                level.SatisfiedTargets < level.TotalTargets)
            {
                return CreateResult(
                    SparkAIReasoningState.InProgress,
                    BuildProgressMessage(level, player),
                    level,
                    player);
            }

            return CreateResult(
                SparkAIReasoningState.Active,
                BuildActiveMessage(player),
                level,
                player);
        }

        private SparkAIReasoningResult CreateResult(
            SparkAIReasoningState state,
            string summary,
            SparkAILevelSnapshot level,
            SparkAIPlayerSnapshot player)
        {
            return new SparkAIReasoningResult(
                true,
                state,
                summary,
                BuildSituation(summary, player),
                level.LevelName,
                player.HasSelection,
                player.SelectedObjectName,
                capturedAt: Time.time);
        }

        private SparkAIReasoningResult BuildUnavailableResult()
        {
            return new SparkAIReasoningResult(
                false,
                SparkAIReasoningState.Unavailable,
                "Spark AI world information is unavailable.",
                "Spark AI cannot currently observe the Project Spark world.",
                string.Empty,
                false,
                string.Empty,
                Time.time);
        }

        private string BuildProgressMessage(
            SparkAILevelSnapshot level,
            SparkAIPlayerSnapshot player)
        {
            string progress =
                $"The level is in progress. " +
                $"{level.SatisfiedTargets} of {level.TotalTargets} targets are satisfied.";

            if (player.LatestActivity.Type !=
                SparkAIPlayerActivityType.None)
            {
                progress +=
                    $" Latest player activity: " +
                    $"{player.LatestActivity.Description}";
            }

            return progress;
        }

        private string BuildActiveMessage(
            SparkAIPlayerSnapshot player)
        {
            if (player.LatestActivity.Type !=
                SparkAIPlayerActivityType.None)
            {
                return
                    $"The level is active. " +
                    $"Latest player activity: " +
                    $"{player.LatestActivity.Description}";
            }

            if (player.HasSelection)
            {
                return
                    $"The level is active. " +
                    $"The player has selected {player.SelectedObjectName}.";
            }

            return "The level is active and awaiting player progress.";
        }

        private string BuildPlayerAwareMessage(
            string baseMessage,
            SparkAIPlayerSnapshot player)
        {
            if (player.LatestActivity.Type ==
                SparkAIPlayerActivityType.None)
            {
                return baseMessage;
            }

            return
                $"{baseMessage} " +
                $"The player's latest activity was: " +
                $"{player.LatestActivity.Description}";
        }

        private string BuildSituation(
            string summary,
            SparkAIPlayerSnapshot player)
        {
            if (player.LatestActivity.Type ==
                SparkAIPlayerActivityType.None)
            {
                return summary;
            }

            return
                $"{summary} " +
                $"Player activity: {player.LatestActivity.Description}";
        }
    }

    public enum SparkAIReasoningState
    {
        Unavailable = 0,

        NoActiveLevel = 1,

        Active = 2,

        InProgress = 3,

        NoValidPowerSource = 4,

        WrongConnection = 5,

        SourceShort = 6,

        TargetShort = 7,

        LevelCompleted = 8,

        LevelFailed = 9
    }

    /// <summary>
    /// Immutable result of deterministic Spark AI reasoning.
    /// </summary>
    public readonly struct SparkAIReasoningResult
    {
        public bool IsValid { get; }

        public SparkAIReasoningState State { get; }

        public string Summary { get; }

        public string Situation { get; }

        public string LevelName { get; }

        public bool HasSelection { get; }

        public string SelectedObjectName { get; }

        public float CapturedAt { get; }

        public SparkAIReasoningResult(
            bool isValid,
            SparkAIReasoningState state,
            string summary,
            string situation,
            string levelName,
            bool hasSelection,
            string selectedObjectName,
            float capturedAt)
        {
            IsValid = isValid;
            State = state;
            Summary = summary ?? string.Empty;
            Situation = situation ?? string.Empty;
            LevelName = levelName ?? string.Empty;
            HasSelection = hasSelection;
            SelectedObjectName = selectedObjectName ?? string.Empty;
            CapturedAt = capturedAt;
        }
    }
}