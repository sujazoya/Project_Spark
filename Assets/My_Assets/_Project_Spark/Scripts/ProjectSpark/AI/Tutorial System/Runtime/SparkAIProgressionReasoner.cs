
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Global read-only progression intelligence.
    /// Uses LevelGamePlayManager and SparkLevelProgression.
    /// No hardcoded level identifiers.
    /// </summary>
    public static class SparkAIProgressionReasoner
    {
        public static string Explain(
            LevelGamePlayManager manager)
        {
            if (manager == null)
            {
                return
                    "The level manager is unavailable.";
            }

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                "SPARK AI - LEVEL PROGRESSION");

            sb.AppendLine();

            SparkLevelDefinition active =
                manager.ActiveLevel;

            if (active == null)
            {
                sb.AppendLine(
                    "No active level is configured.");

                return sb.ToString();
            }

            int currentIndex =
                manager.ActiveLevelIndex;

            int levelCount =
                manager.LevelCount;

            sb.AppendLine(
                "Current level: " +
                active.DisplayName);

            sb.AppendLine(
                "Level position: " +
                (currentIndex + 1) +
                "/" + levelCount);

            sb.AppendLine(
                "Runtime state: " +
                manager.RuntimeState);

            sb.AppendLine(
                "Objectives: " +
                manager.SatisfiedTargetCount +
                "/" +
                manager.ActiveTargetCount);

            sb.AppendLine();

            // ==========================================
            // CURRENT LEVEL
            // ==========================================

            if (manager.IsCompleted)
            {
                sb.AppendLine(
                    "The current level is complete.");
            }
            else if (manager.IsFailed)
            {
                sb.AppendLine(
                    "The current level has failed.");

                if (!string.IsNullOrWhiteSpace(
                        manager.LastFailureReason))
                {
                    sb.AppendLine(
                        "Reason: " +
                        manager.LastFailureReason);
                }
            }
            else
            {
                sb.AppendLine(
                    "The current level is not yet complete.");
            }

            // ==========================================
            // NEXT LEVEL
            // ==========================================

            int nextIndex = currentIndex + 1;

            SparkLevelDefinition next =
                manager.GetLevel(nextIndex);

            sb.AppendLine();

            if (next == null)
            {
                sb.AppendLine(
                    "There is no next level " +
                    "in the manager's level list.");
            }
            else
            {
                sb.AppendLine(
                    "Next level: " +
                    next.DisplayName);

                if (manager.LevelProgression == null)
                {
                    sb.AppendLine(
                        "Progression access data unavailable.");
                }
                else
                {
                    bool canPlay =
                        manager.LevelProgression
                            .CanPlayLevel(nextIndex);

                    sb.AppendLine(
                        canPlay
                            ? "Next level access: UNLOCKED"
                            : "Next level access: LOCKED");

                    if (!canPlay)
                    {
                        sb.AppendLine(
                            "The progression system has " +
                            "not granted access to this level.");
                    }
                }
            }

            // ==========================================
            // ALL CONFIGURED LEVELS
            // ==========================================

            sb.AppendLine();
            sb.AppendLine("LEVEL ACCESS");

            for (int i = 0; i < levelCount; i++)
            {
                SparkLevelDefinition level =
                    manager.GetLevel(i);

                if (level == null)
                    continue;

                bool? available = null;

                if (manager.LevelProgression != null)
                {
                    available =
                        manager.LevelProgression
                            .CanPlayLevel(i);
                }

                string access =
                    available.HasValue
                        ? available.Value
                            ? "UNLOCKED"
                            : "LOCKED"
                        : "UNKNOWN";

                string current =
                    i == currentIndex
                        ? " [ACTIVE]"
                        : string.Empty;

                sb.AppendLine(
                    (i + 1) + ". " +
                    level.DisplayName +
                    " - " + access +
                    current);
            }

            sb.AppendLine();

            sb.AppendLine(
                "Level access comes from " +
                "SparkLevelProgression.");

            sb.AppendLine(
                "UNLOCKED does not necessarily mean " +
                "COMPLETED. Historical completion " +
                "requires progression completion data.");

            return sb.ToString();
        }
    }
}
