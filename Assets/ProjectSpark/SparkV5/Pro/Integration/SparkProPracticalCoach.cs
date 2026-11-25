using System;
using System.Text;
using System.Text.RegularExpressions;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Read-only, source-grounded practical tutoring for the currently loaded level.
    /// Invoked by SparkCortex before generic knowledge fallbacks.
    /// Never changes a circuit, mission, student grade, inventory or certificate.
    /// </summary>
    internal static class SparkProPracticalCoach
    {
        internal enum Request { None, Objective, Diagnose, InspectNext, PracticeStatus, ExplainPath }

        internal static Request Classify(string question)
        {
            string q = SparkCortex.Normalize(question);
            if (q.Length == 0) return Request.None;

            // Specific practical questions take priority over generic LED definitions.
            if (Regex.IsMatch(q, @"\b(have i|did i|am i|is my|is the|was the)\b.*\b(complet(e|ed|ion)|finish(ed)?|pass(ed)?|done)\b.*\b(practical|exercise|lab|objective|experiment)\b") ||
                Regex.IsMatch(q, @"\b(have i completed the practical|check my practical progress|is my practical done)\b"))
                return Request.PracticeStatus;

            if (Regex.IsMatch(q, @"\b(show|point|highlight|identify|tell|guide)\b.*\b(connection|wire|terminal|pin|anode|cathode)\b.*\b(next|inspect|check|look|connect|at|to)\b") ||
                Regex.IsMatch(q, @"\b(which|what)\b.*\b(connection|wire|terminal|pin)\b.*\b(next|inspect|check)\b") ||
                Regex.IsMatch(q, @"\b(show me (the |which )?(led )?(anode|cathode)|point to the (anode|cathode))\b"))
                return Request.InspectNext;

            if (Regex.IsMatch(q, @"\b(what(?:'s| is) wrong|why (?:is|isn't|doesn't|does not|won't|hasn't)|diagnose|troubleshoot|find the fault|why (?:is|does) my)\b") &&
                Regex.IsMatch(q, @"\b(led|circuit|wiring|wire|rail|component|power|supply|output|reading|load|diode|resistor|switch|voltage|current|working|light|light up)\b"))
                return Request.Diagnose;

            if (Regex.IsMatch(q, @"\b(check|verify|evaluate|assess|inspect|review)\b.*\b(level\s*\d+|objective|my led|mission objective|my work)\b") ||
                Regex.IsMatch(q, @"\b(how is|what is)\b.*\b(objective|mission)\b.*\b(progress|status)\b"))
                return Request.Objective;

            if (Regex.IsMatch(q, @"\b(why|explain|how)\b.*\b(return path|closed (?:return|circuit|loop)|complete (?:return|current|circuit) path|open circuit|current path)\b"))
                return Request.ExplainPath;

            return Request.None;
        }

        internal static bool TryRespond(SparkCortex.Context context, string activeLesson,
            out SparkCortex.Reply reply)
        {
            reply = default;
            if (context == null || context.Restricted) return false;
            Request request = Classify(context.Question);
            if (request == Request.None) return false;

            // Must never use a conversation about another level to inspect the current scene.
            Match requested = Regex.Match(SparkCortex.Normalize(context.Question), @"\blevel\s*(\d{1,3})\b");
            if (requested.Success && int.TryParse(requested.Groups[1].Value, out int number) &&
                (!context.Level.Definition || context.Level.Definition.LevelNumber != number))
            {
                reply = new SparkCortex.Reply(
                    $"Level {number} is not the loaded laboratory level. I can discuss its authored curriculum, " +
                    "but I cannot inspect or grade its wiring from this scene. Load that level to check live evidence.",
                    SparkState.Teaching, SparkGesture.Explain);
                return true;
            }

            string text;
            SparkState state = SparkState.Observing;
            SparkGesture gesture = SparkGesture.Inspect;
            switch (request)
            {
                case Request.Objective:
                    text = ObjectiveStatus(context);
                    break;
                case Request.PracticeStatus:
                    text = PracticeStatus(context, activeLesson);
                    state = SparkState.Teaching;
                    gesture = SparkGesture.Explain;
                    break;
                case Request.Diagnose:
                    text = Diagnose(context);
                    state = SparkState.Diagnosing;
                    break;
                case Request.InspectNext:
                    text = InspectNext(context);
                    state = SparkState.Teaching;
                    gesture = SparkGesture.Offer;
                    break;
                case Request.ExplainPath:
                    text = ExplainReturnPath(context);
                    state = SparkState.Teaching;
                    gesture = SparkGesture.Explain;
                    break;
                default:
                    return false;
            }

            reply = new SparkCortex.Reply(text, state, gesture);
            return true;
        }

        private static bool EvaluatorFresh(SparkAILevelContext level) =>
            SparkPhase4EvidenceGuard.EvaluatorIsCurrent(level);

        private static string ObjectiveStatus(SparkCortex.Context context)
        {
            SparkAILevelContext level = context.Level;
            if (!level.Definition) return "No level is currently loaded. Open the intended laboratory mission to check its actual objective.";
            if (!level.HasEvaluation)
                return "The current layout has no matching authoritative mission evaluation. I cannot verify objective completion yet.";

            SparkLevelTarget target = FindTarget(level, context.Question);
            bool isFresh = EvaluatorFresh(level);
            var text = new StringBuilder();
            text.Append("Level ").Append(level.Definition.LevelNumber).Append(" — ")
                .Append(level.Manager.SatisfiedTargetCount).Append('/')
                .Append(level.Manager.ActiveTargetCount).Append(" objectives reported by the game evaluator.");

            if (target != null)
            {
                bool? status = level.Status(target);
                text.Append("\nObjective: ").Append(SparkAILevelContext.TargetName(target)).Append(" — ")
                    .Append(status == true ? "satisfied" : status == false ? "not yet satisfied" : "not evaluated").Append('.');
                if (!string.IsNullOrWhiteSpace(target.Description))
                    text.Append("\nAuthored requirement: ").Append(target.Description);
                string conditions = SparkAILevelContext.TargetConditions(target);
                if (!string.IsNullOrWhiteSpace(conditions)) text.Append("\nChecks: ").Append(conditions);
            }
            if (!isFresh) text.Append("\nWarning: the electrical evaluation is not confirmed current for this circuit revision. Re-evaluate before relying on electrical details.");
            else if (level.Manager.SourceShorted || level.Manager.TargetShorted)
                text.Append("\nSafety: the evaluator reports a short. Isolate the virtual supply before inspection.");
            else if (level.Manager.Overloaded)
                text.Append("\nSafety: the evaluator reports an overload. Isolate the virtual supply before inspection.");
            return text.ToString();
        }

        private static string PracticeStatus(SparkCortex.Context context, string activeLesson)
        {
            SparkAILevelContext level = context.Level;
            if (!level.HasEvaluation)
                return "The loaded mission has no matching evaluator result, so I cannot confirm the practical. " +
                       "Guided tutorials do not award a practical pass or Academy mastery.";
            string mission = $"The current game evaluator reports {level.Manager.SatisfiedTargetCount}/{level.Manager.ActiveTargetCount} objectives satisfied.";
            if (!EvaluatorFresh(level))
                mission += " Its electrical evidence is not confirmed current; re-evaluate after any circuit change.";
            if (level.Manager.IsCompleted)
                mission += " The mission is marked complete by the game evaluator.";
            else
                mission += " The mission is not marked complete yet.";
            if (!string.IsNullOrWhiteSpace(activeLesson))
                mission += "\nYour teaching session is still active (" + activeLesson + "). " +
                           "Its guided practice is ungraded and remains separate from mission completion.";
            else
                mission += "\nNo active guided lesson is reported. A completed mission does not automatically grant a teacher assessment or certificate.";
            return mission;
        }

        private static string Diagnose(SparkCortex.Context context)
        {
            SparkAILevelContext level = context.Level;
            if (!level.Definition) return "There is no loaded mission circuit to diagnose. Load a level and select the relevant component.";
            if (!level.HasEvaluation) return "The current circuit does not have a matching evaluator result. I cannot claim a fault without it.";
            if (!EvaluatorFresh(level)) return "The electrical evaluation is stale or incomplete for this revision. Let the circuit settle and request a new evaluation before diagnosing it.";

            var manager = level.Manager;
            if (manager.SourceShorted || manager.TargetShorted || manager.Overloaded || manager.WrongConnection ||
                !manager.HasValidPowerSource || level.Definition.RequireClosedReturn && !manager.ClosedReturn)
                return SparkProDiagnostics.Explain(level, context.Object, context.Question, false);

            if (context.Object is SparkLED led && led.IsReverseVoltageExceeded ||
                context.Object is SparkDiode diode && diode.IsReverseFault ||
                context.Object is SparkSwitch control && control.IsOpen)
                return SparkProDiagnostics.Explain(level, context.Object, context.Question, false);

            if (manager.IsCompleted)
                return "The current evaluator marks this mission complete and does not flag a short, overload, missing power, or incorrect connection. " +
                       "I have not confirmed an LED fault. If you see unexpected behavior, select the affected part " +
                       "and obtain a fresh solver measurement with explicit terminals." +
                       (context.HasFreshSolve ? " A current solve is available." : " A fresh solver reading is not available yet.");

            SparkLevelTarget pending = level.NextTarget();
            if (pending == null) return "No specific fault is verified. Check the mission objective panel and refresh the electrical evaluation.";
            if (!context.HasFreshSolve)
                return "The objective '" + SparkAILevelContext.TargetName(pending) +
                       "' is not satisfied, but the solver has no current reading for this revision. Re-solve the circuit before identifying a cause.";
            return SparkProDiagnostics.Explain(level, context.Object, context.Question, false);
        }

        private static string InspectNext(SparkCortex.Context context)
        {
            SparkAILevelContext level = context.Level;
            if (!level.Definition || !level.Layout)
                return "Open the laboratory level first. I cannot point at a component outside the loaded layout.";
            if (!level.HasEvaluation)
                return "The workbench is loaded, but the matching evaluator is unavailable. Select a component to inspect its actual terminals.";
            if (!EvaluatorFresh(level))
                return "Current electrical evaluation is not confirmed. Re-evaluate first; I won't point to a supposed faulty wire based on stale evidence.";

            SparkLevelTarget target = level.NextTarget();
            bool complete = level.Manager.IsCompleted;
            if (target == null && complete)
                return "The evaluator reports the mission complete; no additional required connection is identified. " +
                       "You can still select a terminal and ask me to point to it, or move to the next lesson.";
            if (target == null)
                return "No specific incomplete target was identified. Open the objective monitor or select a terminal for inspection.";

            string description = SparkAILevelContext.TargetName(target);
            if (target.HasRequiredConnectionPair && level.Layout.SceneBindings &&
                level.Layout.SceneBindings.TryGetTerminal(target.RequiredPositiveTerminalId, out SparkTerminal a) &&
                level.Layout.SceneBindings.TryGetTerminal(target.RequiredNegativeTerminalId, out SparkTerminal b))
            {
                if (!a || !b || !a.transform.IsChildOf(level.Layout.transform) || !b.transform.IsChildOf(level.Layout.transform))
                    return "The target's authored terminals are not both present in the current layout. I cannot identify a safe inspection point.";
                bool connected = level.Manager.CircuitSystem && level.Manager.CircuitSystem.HasConnectionBetween(a, b);
                SparkTerminal focus = ChooseFocus(context.Question, a, b);
                bool pointed = PointTo(context, focus.transform);
                string status = connected ? "A direct connection currently exists between these terminals." :
                    "No direct connection currently exists between these terminals.";
                return $"Next incomplete objective: {description}. Inspect the authored endpoints " +
                       $"{a.name} and {b.name}. {status} " +
                       (pointed ? $"Spark is pointing at {focus.name}." :
                        "Select the terminal in the workshop to inspect it; visual pointing is unavailable.") +
                       " Do not change wires until you confirm the intended routing and power is isolated.";
            }
            if (context.Pin && context.Pin.transform.IsChildOf(level.Layout.transform))
            {
                bool pointed = PointTo(context, context.Pin.transform);
                return $"Next incomplete objective: {description}. Its authored connection pair is not uniquely bound. " +
                       (pointed ? $"Spark is pointing at selected terminal {context.Pin.name}." :
                        "The selected terminal is valid but character pointing is unavailable.");
            }
            return $"Next incomplete objective: {description}. There is no uniquely resolved pair of authored terminals. " +
                   "Select the relevant component or terminal. I will not guess a wire endpoint.";
        }

        private static SparkTerminal ChooseFocus(string question, SparkTerminal a, SparkTerminal b)
        {
            string q = SparkCortex.Normalize(question);
            if (q.Contains("cathode") || q.Contains("negative") || q.Contains("return")) return b;
            return a;
        }

        private static bool PointTo(SparkCortex.Context context, Transform target)
        {
            return target && context.Level.Layout && target.IsChildOf(context.Level.Layout.transform) &&
                context.Point != null && context.Point(target);
        }

        private static string ExplainReturnPath(SparkCortex.Context context)
        {
            string explanation = "Current in a powered circuit needs a continuous conductive loop through the load and back to the source. " +
                                 "Opening the return breaks that loop, so steady current cannot flow through the intended load. " +
                                 "For an LED, correct polarity is necessary, but polarity alone does not make a complete safe circuit.";
            var level = context.Level;
            if (!level.HasEvaluation) return explanation + " No current laboratory evaluator is available for this explanation.";
            if (!EvaluatorFresh(level)) return explanation + " The circuit evaluation is not confirmed fresh, so I cannot state its present return condition.";
            if (level.Definition.RequireClosedReturn)
                return explanation + (level.Manager.ClosedReturn ?
                    " The level evaluator currently confirms the required return path." :
                    " The level evaluator currently reports the required return path as open. With the virtual supply isolated, inspect the load-return connection first.");
            return explanation + " This level does not declare a global closed-return requirement, so an open path cannot be inferred from that rule alone.";
        }

        private static SparkLevelTarget FindTarget(SparkAILevelContext level, string question)
        {
            if (!level.Definition || level.Definition.Targets == null) return null;
            string q = SparkCortex.Normalize(question);
            SparkLevelTarget first = null;
            SparkLevelTarget matched = null;
            int longest = 0;
            foreach (SparkLevelTarget t in level.Definition.Targets)
            {
                if (t == null) continue;
                if (first == null) first = t;
                string name = SparkCortex.Normalize(SparkAILevelContext.TargetName(t));
                string id = SparkCortex.Normalize(t.TargetId);
                if (name.Length > 1 && q.Contains(name) && name.Length > longest)
                { matched = t; longest = name.Length; }
                else if (id.Length > 2 && q.Contains(id) && id.Length > longest)
                { matched = t; longest = id.Length; }
            }
            return matched ?? level.NextTarget() ?? first;
        }
    }
}
