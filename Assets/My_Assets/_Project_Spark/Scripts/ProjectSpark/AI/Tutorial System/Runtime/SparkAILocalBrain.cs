using System;
using System.Text;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;
using ProjectSpark.Circuit;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Read-only, level-agnostic conversation brain for Ask Spark.
    /// No LLM, networking, Unity UI, or level-specific IDs.
    /// Authoritative pass/fail always comes from LevelGamePlayManager.TargetResults.
    /// </summary>
    public sealed class SparkAILocalBrain
    {
        private enum Topic { None, Level, Progress, Target, Diagnosis }
        private string previousLevelId;
        private Topic previousTopic;
        private string previousTargetId;

        public void Clear()
        {
            previousLevelId = null;
            previousTopic = Topic.None;
            previousTargetId = null;
        }

        public bool TryAnswer(string question, LevelGamePlayManager manager, out string answer)
        {
            answer = null;
            if (string.IsNullOrWhiteSpace(question) || manager == null || manager.ActiveLevel == null)
                return false;

            string id = manager.ActiveLevel.LevelId;
            if (!string.Equals(previousLevelId, id, StringComparison.Ordinal))
            {
                Clear();
                previousLevelId = id;
            }

            string q = Normalize(question);
            if (q.Length == 0) return false;

            bool followUp = EqualsAny(q, "why", "why not", "how", "how so", "explain", "explain more",
                "tell me more", "what next", "what should i do", "what should i do next", "and why");

            if (followUp && previousTopic != Topic.None)
            {
                if (previousTopic == Topic.Target && !string.IsNullOrEmpty(previousTargetId))
                {
                    SparkLevelTarget previousTarget = FindTarget(manager, previousTargetId);
                    if (previousTarget != null)
                    {
                        answer = ExplainTarget(manager, previousTarget, true);
                        return true;
                    }
                }
                answer = Diagnose(manager, previousTopic == Topic.Progress);
                previousTopic = Topic.Diagnosis;
                return true;
            }

            bool navigation = HasAny(q, "next level", "previous level", "all levels", "unlocked", "locked",
                "level progression", "how many levels");
            if (navigation) return false; // Preserve the existing progression handler.

            if (HasAny(q, "what level", "which level", "current level", "my level", "level am i",
                "level i am", "level im", "which stage", "what stage", "currently playing"))
            {
                previousTopic = Topic.Level;
                answer = "You are playing Level " + (manager.ActiveLevelIndex + 1) + " of " +
                    manager.LevelCount + ": " + manager.ActiveLevel.DisplayName + ".\n" +
                    "Status: " + manager.RuntimeState + ". Objectives: " +
                    manager.SatisfiedTargetCount + "/" + manager.ActiveTargetCount + ".";
                return true;
            }

            if (HasAny(q, "which objective", "which target", "pending objective", "pending target",
                "unfinished objective", "what remains", "what is missing", "whats missing", "what is left"))
            {
                SparkLevelTarget target = FirstPendingTarget(manager);
                previousTopic = Topic.Target;
                previousTargetId = target != null ? target.TargetId : null;
                answer = target == null ? "No pending objective was found in the latest evaluation. " +
                    "Level status: " + manager.RuntimeState + "." : ExplainTarget(manager, target, false);
                return true;
            }

            if (HasAny(q, "why havent i won", "why cant i win", "why am i not winning", "not complete",
                "not completed", "why is my level", "diagnose level", "diagnose my circuit",
                "what is wrong", "whats wrong", "what should i fix", "why is the objective failing",
                "why is this objective failing", "why is current low", "why is the current low",
                "safe current", "current requirement", "current objective", "voltage objective", "power objective"))
            {
                previousTopic = Topic.Diagnosis;
                SparkLevelTarget target = FirstPendingTarget(manager);
                previousTargetId = target != null ? target.TargetId : null;
                answer = Diagnose(manager, false);
                return true;
            }

            if (HasAny(q, "my progress", "level progress", "how far am i", "how many objectives",
                "how many targets", "percent complete", "percentage complete"))
            {
                previousTopic = Topic.Progress;
                int total = manager.ActiveTargetCount;
                int satisfied = manager.SatisfiedTargetCount;
                answer = "Level progress: " + satisfied + "/" + total + " objectives satisfied" +
                    (total > 0 ? " (" + (100f * satisfied / total).ToString("0") + "%)." : ".") +
                    "\nEvaluator state: " + manager.RuntimeState + ".";
                return true;
            }

            return false; // Let the original component/electrical explanation code handle this.
        }

        private static string Diagnose(LevelGamePlayManager manager, bool brief)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Level: " + manager.ActiveLevel.DisplayName + " (" + manager.RuntimeState + ")");
            sb.AppendLine("Objectives: " + manager.SatisfiedTargetCount + "/" + manager.ActiveTargetCount);
            if (manager.IsCompleted)
            {
                sb.Append("The game evaluator confirms this level is complete.");
                return sb.ToString();
            }
            if (manager.SourceShorted) sb.AppendLine("Warning: The evaluator detected a source short circuit.");
            if (manager.TargetShorted) sb.AppendLine("Warning: The evaluator detected a target short circuit.");
            if (manager.Overloaded) sb.AppendLine("Warning: The power source is overloaded.");
            if (manager.WrongConnection) sb.AppendLine("Warning: The evaluator detected an incorrect connection.");
            if (!manager.HasValidPowerSource) sb.AppendLine("No valid active power source is confirmed.");
            if (!manager.ClosedReturn) sb.AppendLine("A closed return path is not confirmed.");
            if (manager.IsFailed) sb.AppendLine("Failure: " + manager.LastFailureReason);
            if (!brief)
            {
                SparkLevelTarget target = FirstPendingTarget(manager);
                if (target != null) sb.AppendLine(ExplainTarget(manager, target, true));
                else sb.AppendLine("No pending target is reported. Check other level completion conditions.");
            }
            return sb.ToString().Trim();
        }

        private static string ExplainTarget(LevelGamePlayManager manager, SparkLevelTarget target, bool detail)
        {
            StringBuilder sb = new StringBuilder();
            string name = string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetId : target.DisplayName;
            bool? passed = OfficialResult(manager, target.TargetId);
            sb.AppendLine("Objective: " + name);
            sb.AppendLine("Evaluator: " + (!passed.HasValue ? "NOT YET REPORTED" : passed.Value ? "PASSED" : "PENDING"));
            if (!string.IsNullOrWhiteSpace(target.Description)) sb.AppendLine(target.Description);
            if (!detail) { sb.Append("Ask 'why' for live measurement evidence."); return sb.ToString(); }

            SparkLevelSceneBindings bindings = manager.SceneBindings;
            if (bindings == null)
            {
                sb.Append("Active layout bindings are unavailable; I cannot verify the target measurement.");
                return sb.ToString();
            }
            bool range = target.Type == SparkLevelTarget.TargetType.CurrentRange ||
                target.Type == SparkLevelTarget.TargetType.PowerRange ||
                target.Type == SparkLevelTarget.TargetType.VoltageRange;
            if (!range)
            {
                sb.AppendLine("Target type: " + target.Type + ".");
                sb.Append("The evaluator's pass/fail result is authoritative. Inspect wiring, polarity and component state.");
                return sb.ToString();
            }
            if (!target.IsConfigured() || !target.IsPolarityConfigurationValid())
            {
                sb.Append("The target configuration is incomplete or has invalid polarity settings.");
                return sb.ToString();
            }

            bool isCurrent = target.Type == SparkLevelTarget.TargetType.CurrentRange;
            bool isPower = target.Type == SparkLevelTarget.TargetType.PowerRange;
            float value, minimum, maximum;
            bool useMax;
            string unit;
            float scale;
            SparkElectricalComponent component = null;

            if (isCurrent || isPower || !target.HasRequiredConnectionPair && !string.IsNullOrWhiteSpace(target.TargetComponentId))
            {
                if (string.IsNullOrWhiteSpace(target.TargetComponentId) ||
                    !bindings.TryGetComponent(target.TargetComponentId, out component) || component == null)
                {
                    sb.Append("The active scene binding cannot resolve target component ID '" +
                        target.TargetComponentId + "'. I will not substitute a different resistor.");
                    return sb.ToString();
                }
                if (isCurrent) value = Mathf.Abs(component.ElectricalState.Current);
                else if (isPower) value = Mathf.Abs(component.ElectricalState.Power);
                else value = Mathf.Abs(component.ElectricalState.Voltage);
                sb.AppendLine("Measured component: " + component.name + " (binding " + target.TargetComponentId + ")");
                if (target.RequireElectricalEnabled && !component.ElectricalEnabled)
                    sb.AppendLine("Required electrical-enabled condition is not met.");
                if (target.RequireConduction && component.ElectricalState.Conduction != SparkConductionState.Conducting)
                    sb.AppendLine("Required conduction condition is not met.");
            }
            else if (target.HasRequiredConnectionPair)
            {
                SparkTerminal positive, negative;
                if (!bindings.TryGetTerminal(target.RequiredPositiveTerminalId, out positive) || positive == null ||
                    !bindings.TryGetTerminal(target.RequiredNegativeTerminalId, out negative) || negative == null)
                {
                    sb.Append("The required voltage terminal pair could not be resolved.");
                    return sb.ToString();
                }
                value = Mathf.Abs(positive.ElectricalState.Voltage - negative.ElectricalState.Voltage);
                if (target.RequireElectricalEnabled && (!positive.IsElectricalEnabled || !negative.IsElectricalEnabled))
                    sb.AppendLine("One or more required terminals are electrically disabled.");
            }
            else
            {
                SparkTerminal terminal;
                if (!bindings.TryGetTerminal(target.TargetTerminalId, out terminal) || terminal == null)
                {
                    sb.Append("The target voltage terminal could not be resolved.");
                    return sb.ToString();
                }
                value = Mathf.Abs(terminal.ElectricalState.Voltage);
                if (target.RequireElectricalEnabled && !terminal.IsElectricalEnabled)
                    sb.AppendLine("The required terminal is electrically disabled.");
            }

            if (isCurrent) { minimum = target.MinimumCurrent; maximum = target.MaximumCurrent; useMax = target.UseMaximumCurrent; unit = "mA"; scale = 1000f; }
            else if (isPower) { minimum = target.MinimumPower; maximum = target.MaximumPower; useMax = target.UseMaximumPower; unit = "mW"; scale = 1000f; }
            else { minimum = target.MinimumVoltage; maximum = target.MaximumVoltage; useMax = target.UseMaximumVoltage; unit = "V"; scale = 1f; }
            sb.AppendLine("Measured: " + (value * scale).ToString("0.###") + " " + unit);
            sb.AppendLine("Required: at least " + (minimum * scale).ToString("0.###") + " " + unit +
                (useMax ? ", at most " + (maximum * scale).ToString("0.###") + " " + unit : ""));
            if (float.IsNaN(value) || float.IsInfinity(value)) sb.Append("The measurement is invalid; check the solve state.");
            else if (value < minimum) sb.Append("This measurement is " + ((minimum - value) * scale).ToString("0.###") + " " + unit + " below the minimum.");
            else if (useMax && value > maximum) sb.Append("This measurement is " + ((value - maximum) * scale).ToString("0.###") + " " + unit + " above the maximum.");
            else sb.Append("The numeric range passes. If the objective remains pending, check enabled/conduction requirements and the configured target binding.");
            if (target.Inverted) sb.Append(" Note: this objective inverts the result.");
            return sb.ToString();
        }

        private static SparkLevelTarget FirstPendingTarget(LevelGamePlayManager manager)
        {
            SparkLevelTarget[] targets = manager.ActiveLevel.Targets;
            if (targets == null) return null;
            foreach (SparkLevelTarget target in targets)
                if (target != null && OfficialResult(manager, target.TargetId) == false) return target;
            // No published results? Don't invent a pending target.
            return null;
        }

        private static SparkLevelTarget FindTarget(LevelGamePlayManager manager, string id)
        {
            SparkLevelTarget[] targets = manager.ActiveLevel.Targets;
            if (targets == null) return null;
            foreach (SparkLevelTarget target in targets)
                if (target != null && string.Equals(target.TargetId, id, StringComparison.Ordinal)) return target;
            return null;
        }

        private static bool? OfficialResult(LevelGamePlayManager manager, string targetId)
        {
            foreach (var result in manager.TargetResults)
                if (string.Equals(result.targetId, targetId, StringComparison.Ordinal)) return result.satisfied;
            return null;
        }

        private static bool HasAny(string text, params string[] phrases)
        {
            string padded = " " + text + " ";
            foreach (string phrase in phrases)
                if (padded.IndexOf(" " + phrase + " ", StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static bool EqualsAny(string text, params string[] values)
        {
            foreach (string value in values) if (text == value) return true;
            return false;
        }

        private static string Normalize(string question)
        {
            StringBuilder sb = new StringBuilder(question.Length);
            bool previousSpace = true;
            foreach (char c in question.ToLowerInvariant().Replace("’", "").Replace("'", ""))
            {
                if (char.IsLetterOrDigit(c)) { sb.Append(c); previousSpace = false; }
                else if (!previousSpace) { sb.Append(' '); previousSpace = true; }
            }
            return sb.ToString().Trim();
        }
    }
}
