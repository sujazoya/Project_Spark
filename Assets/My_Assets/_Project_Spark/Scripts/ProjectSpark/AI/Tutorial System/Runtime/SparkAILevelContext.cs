using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>Read-only view of the committed layout and its matching evaluator.</summary>
    public readonly struct SparkAILevelContext
    {
        public LevelGamePlayManager Manager { get; }
        public SparkLevelLayout Layout { get; }
        public SparkLevelDefinition Definition => Layout ? Layout.LevelDefinition : null;

        public bool HasEvaluation => Definition && Manager && Manager.ActiveLevel == Definition &&
            Layout.SceneBindings && Manager.SceneBindings == Layout.SceneBindings &&
            Manager.RuntimeState is LevelGamePlayManager.LevelRuntimeState.Playing
                or LevelGamePlayManager.LevelRuntimeState.Completed
                or LevelGamePlayManager.LevelRuntimeState.Failed;

        private SparkAILevelContext(LevelGamePlayManager manager, SparkLevelLayout layout)
        {
            Manager = manager;
            Layout = layout;
        }

        public static SparkAILevelContext Capture(LevelGamePlayManager manager, SparkLevelLayoutLoader loader = null)
        {
            if (manager && manager.LayoutLoader) loader = manager.LayoutLoader;
            return new SparkAILevelContext(manager, loader ? loader.ActiveLayout : null);
        }

        public static string Title(SparkLevelDefinition level)
        {
            if (!level) return "No level";
            int number = level.LevelNumber;
            string identifier = number > 0 ? $"Level {number}" : level.LevelId;
            return string.IsNullOrWhiteSpace(identifier) || string.Equals(identifier, level.DisplayName, StringComparison.OrdinalIgnoreCase)
                ? level.DisplayName : $"{identifier} · {level.DisplayName}";
        }

        public static string TargetName(SparkLevelTarget target) =>
            string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetId : target.DisplayName;

        public bool? Status(SparkLevelTarget target)
        {
            if (!HasEvaluation || target == null || Manager.TargetResults == null) return null;
            foreach (var result in Manager.TargetResults)
                if (string.Equals(result.targetId, target.TargetId, StringComparison.Ordinal)) return result.satisfied;
            return null;
        }

        public SparkLevelTarget NextTarget()
        {
            if (!Definition || Definition.Targets == null || (HasEvaluation && Manager.IsCompleted)) return null;
            foreach (var target in Definition.Targets)
                if (target != null && Status(target) != true) return target;
            return null;
        }

        public string StateText() => !Definition ? "No level layout is loaded." :
            !HasEvaluation ? "The layout is loaded. Its matching gameplay evaluation is unavailable." :
            $"{Manager.RuntimeState} · {Manager.SatisfiedTargetCount}/{Manager.ActiveTargetCount} objectives" +
                (Manager.IsFailed && !string.IsNullOrWhiteSpace(Manager.LastFailureReason) ? "\n" + Manager.LastFailureReason : "");

        public string Describe(SparkLevelDefinition level = null, bool detailed = true)
        {
            if (!level) level = Definition;
            if (!level) return StateText();
            var text = new StringBuilder(Title(level)).AppendLine();
            if (!string.IsNullOrWhiteSpace(level.Description)) text.AppendLine(level.Description);
            if (level == Definition) text.AppendLine(StateText());
            else text.AppendLine("Campaign reference · current play status is available for the loaded level.");
            if (!detailed) return text.ToString().TrimEnd();
            text.AppendLine().AppendLine(CompletionRule(level));
            if (level.Targets == null) return text.ToString().TrimEnd();
            int order = 0;
            foreach (var target in level.Targets)
            {
                if (target == null) continue;
                text.Append(++order).Append(". ").Append(TargetName(target));
                if (level == Definition)
                {
                    bool? passed = Status(target);
                    text.Append(passed == true ? " [passed]" : passed == false ? " [pending]" : " [unverified]");
                }
                text.AppendLine();
                if (!string.IsNullOrWhiteSpace(target.Description)) text.Append("   ").AppendLine(target.Description);
                string conditions = TargetConditions(target);
                if (!string.IsNullOrEmpty(conditions)) text.Append("   ").AppendLine(conditions);
            }
            return text.ToString().TrimEnd();
        }

        public static string CompletionRule(SparkLevelDefinition level)
        {
            var rule = level.CompletionRule switch
            {
                SparkLevelDefinition.CompletionMode.AllTargets => "Complete all configured objectives.",
                SparkLevelDefinition.CompletionMode.AnyTarget => "Complete any one configured objective.",
                _ => $"Complete at least {level.RequiredTargetCount} objectives."
            };
            if (level.RequireClosedReturn) rule += " A closed return path is required.";
            if (level.RequireMinimumVoltage) rule += $" Level minimum {level.MinimumVoltage:G4} V.";
            if (level.RejectShortCircuit) rule += " Source shorts block completion.";
            if (level.RejectTargetShort) rule += " Target shorts block completion.";
            if (level.RequireHealthyInstalledResistors) rule += " Required installed resistors must be healthy.";
            if (level.FailureMode != SparkLevelDefinition.LevelFailureMode.None) rule += $" Failure mode {level.FailureMode}.";
            return rule;
        }

        public static string TargetConditions(SparkLevelTarget target)
        {
            var text = new StringBuilder();
            AppendLimit(text, "voltage", "V", target.MinimumVoltage, target.UseMaximumVoltage, target.MaximumVoltage);
            AppendLimit(text, "current", "mA", target.MinimumCurrent * 1000, target.UseMaximumCurrent, target.MaximumCurrent * 1000);
            AppendLimit(text, "power", "W", target.MinimumPower, target.UseMaximumPower, target.MaximumPower);
            if (target.HasRequiredConnectionPair)
                text.Append($"Connection pair {target.RequiredPositiveTerminalId} and {target.RequiredNegativeTerminalId}. ");
            if (target.RequireConduction) text.Append("Conduction required. ");
            if (target.Inverted) text.Append("The objective checks the inverse condition. ");
            return text.ToString().TrimEnd();
        }

        private static void AppendLimit(StringBuilder text, string name, string unit, float min, bool hasMax, float max)
        {
            if (min > 0) text.Append($"{name} ≥ {min:G4} {unit}. ");
            if (hasMax) text.Append($"{name} ≤ {max:G4} {unit}. ");
        }

        public bool TryAnswer(string question, out string reply)
        {
            reply = null;
            if (string.IsNullOrWhiteSpace(question)) return false;
            string q = question.Trim().ToLowerInvariant();
            if (q.Contains("how many levels") || q.Contains("campaign size"))
            {
                if (!Manager) return false;
                int count = 0;
                foreach (int index in Manager.GetCampaignLevelIndices())
                    count++;
                reply = $"The current campaign has {count} levels. Ask for a level number or title for its objectives and completion conditions.";
                return true;
            }

            if (q is "help" or "what can you do" or "what can i ask")
            {
                reply = "Ask for the current level, its objectives, completion conditions, or what to do next. " +
                    "Ask for a campaign level by ID or title. Say “draw this level in 2D” for the whiteboard or “show this objective in 3D” for a workbench highlight. " +
                    "Ask about wiring, components, or measurements for circuit help.";
                return true;
            }

            // A specific authored ID is a campaign lookup, not a campaign array position.
            var match = Regex.Match(q, @"\blevel[\s_-]*(\d+)\b", RegexOptions.CultureInvariant);
            if (Manager && match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                SparkLevelDefinition found = null;
                foreach (int i in Manager.GetCampaignLevelIndices())
                {
                    var level = Manager.GetLevel(i);
                    if (level && level.LevelNumber == number)
                    {
                        found = level;
                        break;
                    }
                }
                reply = found ? Describe(found) : $"Level {number} is outside the current campaign.";
                return true;
            }

            if (Manager && (q.StartsWith("explain ") || q.StartsWith("teach ") || q.StartsWith("tell me about ")))
            {
                SparkLevelDefinition found = null;
                foreach (int i in Manager.GetCampaignLevelIndices())
                {
                    var level = Manager.GetLevel(i);
                    if (level && !string.IsNullOrEmpty(level.DisplayName) && level.DisplayName.Length >= 5 && q.Contains(level.DisplayName.ToLowerInvariant()) &&
                        (!found || level.DisplayName.Length > found.DisplayName.Length)) found = level;
                }
                if (found)
                {
                    reply = Describe(found);
                    return true;
                }
            }

            if (q.Contains("next level") || q.Contains("previous level") || q.Contains("level progression")) return false;
            if (q.Contains("current level") || q.Contains("what level") || q.Contains("which level") ||
                q.Contains("this level") || q.Contains("completion conditions") || q.Contains("level objectives") ||
                q.Contains("completion requirements") || q.Contains("how do i win") || q.Contains("how do i finish") ||
                q is "objectives" or "teach me" or "am i done" or "my progress")
            {
                bool brief = q.StartsWith("what level") || q.StartsWith("which level") ||
                    q is "am i done" or "my progress" or "current level";
                reply = Describe(detailed: !brief);
                return true;
            }
            return false;
        }
    }
}
