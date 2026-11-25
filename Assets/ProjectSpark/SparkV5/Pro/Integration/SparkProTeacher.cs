using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal sealed class SparkProTeacher
    {
        private enum Step { Baseline, Prediction, Experiment, Discussion, Practice }
        private sealed class Lesson
        {
            public string Id, Name, Baseline, Concept, Analogy, Diagram, Prediction, Misconception, Practice;
            public string[] BaselineTerms, PredictionTerms, PracticeTerms;
            public SparkProAnswerRubric BaselineRubric, PredictionRubric, PracticeRubric;
            public SparkProObservationKind Observation = SparkProObservationKind.IsolatedSwitch;
            public string Visual;
            public SparkProLessonDefinition Asset;
            public SparkProAutoGuide AutoGuide;
            public bool GuidedOnly;
        }

        private static readonly Lesson[] Lessons =
        {
            new()
            {
                Id = "closed path", Name = "Closed paths and switching",
                Baseline = "What has to connect back to the battery for a lamp to glow?",
                BaselineTerms = new[] { "return", "loop", "path", "circuit", "negative" },
                Concept = "A lamp needs a complete path from one battery terminal through the lamp and back to the other terminal.",
                Analogy = "Picture a loop of train track. A single missing section stops trains from traveling around the loop, even with the station still open.",
                Diagram = "[Battery +] → [Switch] → [Load] → [Battery −]",
                Prediction = "With the switch open, do you predict current will flow or stop?",
                PredictionTerms = new[] { "stop", "zero", "no", "none", "open" },
                Misconception = "A connected positive lead alone cannot sustain current. Follow the path through the load and back to the supply.",
                Practice = "A closed switch and a good battery still leave the lamp dark. The return wire is missing. What should you inspect first?",
                PracticeTerms = new[] { "return", "negative", "wire", "path" }
            },
            new()
            {
                Id = "ohm", Name = "Voltage, resistance and current",
                Baseline = "Which quantity describes opposition to current? Answer with its name.",
                BaselineTerms = new[] { "resistance", "ohm" },
                Concept = "Ohm’s law relates voltage, resistance and current for an ideal resistor. At a fixed voltage, a higher resistance gives less current.",
                Analogy = "With the same pressure difference, a narrower pipe allows less flow. In this analogy, pressure difference stands for voltage and the pipe restriction stands for resistance.",
                Diagram = "[5 V source] → [series switch] → [1 kΩ resistor] → [return]",
                Prediction = "At the same supply voltage, will a larger resistance make the current higher, lower or unchanged?",
                PredictionTerms = new[] { "less", "lower", "decrease", "smaller", "drops" },
                Misconception = "Keep voltage fixed when comparing the resistors. Current is voltage divided by resistance.",
                Practice = "An ideal 6 V supply drives a 300 Ω resistor. What current flows in milliamps?",
                PracticeTerms = new[] { "20" }
            },
            new()
            {
                Id = "rail diagnosis", Name = "Motherboard rail diagnosis",
                Observation = SparkProObservationKind.SolvedTwoTerminal,
                Baseline = "Which reference terminal should you identify before measuring a rail?",
                BaselineTerms = new[] { "ground", "return", "reference" },
                Concept = "A rail voltage is a difference between a named rail and its documented reference. Boards can have several reference domains.",
                Analogy = "Two heights need the same datum before their difference means anything. A motherboard reference can differ between domains in the same way.",
                Diagram = "[Input supply] → [Protection or enable] → [Regulator] → [Rail test point] ↔ [Documented return]",
                Prediction = "If the supply input is missing, should you first replace the regulator or verify its input?",
                PredictionTerms = new[] { "verify", "input", "measure", "check" },
                Misconception = "An absent rail output alone does not identify the fault. Verify input and enable before blaming a regulator.",
                Practice = "A verified rail input is present and its output is absent. What would you check before replacing the regulator?",
                PracticeTerms = new[] { "enable", "reference", "load", "protection", "fault" }
            }
        };

        // Optional authored content is loaded from Resources/SparkMasterLessons.
        // The three existing curated lessons remain available without any extra assets.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetLessonIndex() { authoredLessons = null; }

        private static Lesson[] authoredLessons;
        private Lesson active;
        private Step step;
        private bool paused, assisted;
        private int mistakes;
        public string VisualModel => active == null ? null : !string.IsNullOrWhiteSpace(active.Visual) ? active.Visual :
            active.Id switch { "ohm" => "resistor", "closed path" => "switch", "rail diagnosis" => "rdx-21 board", _ => null };
        public bool HasSession => active != null;
        public string Active => active == null ? "" : $"{active.Name} · {step}{(paused ? " · paused" : "")}";
        public bool AcceptsResponse => active != null && !paused &&
            (step is Step.Baseline or Step.Prediction or Step.Practice);

        public void Cancel() { active = null; paused = assisted = false; mistakes = 0; }

        public bool TryRespond(SparkIntent intent, SparkCortex.Context c, Func<string> experiment,
            out string text, out SparkGesture gesture)
        {
            text = null;
            gesture = SparkGesture.Explain;
            if (intent == SparkIntent.Lesson)
            {
                string topic = SparkCortex.Normalize(c.Question);
                int currentLevel = c.Level.Definition ? c.Level.Definition.LevelNumber : 0;
                // Auto guides can teach any explicitly requested level without changing the active scene.
                Match requestedLevel = Regex.Match(topic, @"\blevel[\s_-]*(\d{1,3})\b");
                int lookupLevel = currentLevel;
                if (requestedLevel.Success)
                {
                    if (!int.TryParse(requestedLevel.Groups[1].Value, out lookupLevel) ||
                        lookupLevel < 1 || lookupLevel > 130) return false;
                }
                active = FindLesson(topic, lookupLevel);
                // Off-level tutorials may ONLY use ungraded auto guides; never run a reviewed
                // laboratory experiment against another level's live circuit context.
                if (active != null && lookupLevel != currentLevel && !active.GuidedOnly) active = null;
                // Unknown topics fall back to the existing Cortex/knowledge responder.
                if (active == null) return false;
                step = Step.Baseline;
                paused = assisted = false;
                mistakes = 0;
                gesture = SparkGesture.Offer;
                text = $"{active.Name}. Let’s check what you already know. {active.Baseline}";
                return true;
            }
            if (!HasSession)
            {
                if (intent is SparkIntent.Resume or SparkIntent.Pause && c.Academy && c.Academy.IsPracticing)
                {
                    text = intent == SparkIntent.Pause ? c.Academy.PausePractice() : c.Academy.ResumePractice();
                    return true;
                }
                return false;
            }
            if (intent == SparkIntent.Cancel) { Cancel(); text = "Lesson ended. Your completed practice remains in Academy."; return true; }
            if (intent == SparkIntent.Pause) { paused = true; text = "Lesson paused. Say “pick up where we stopped” to return."; return true; }
            if (intent == SparkIntent.Resume)
            {
                paused = false;
                if (step == Step.Discussion)
                {
                    assisted = false; mistakes = 0;
                    step = Step.Practice;
                    text = "Try one independently. " + active.Practice;
                }
                else text = Prompt();
                return true;
            }
            if (paused) { text = "The lesson is paused. Say “resume the lesson” when ready."; return true; }
            if (intent == SparkIntent.ExplainAgain)
            {
                assisted = true;
                text = active.Analogy + " " + Prompt();
                gesture = SparkGesture.Offer;
                return true;
            }
            // Automatically authored guidance is explanatory, never a scored Academy lesson.
            if (active.GuidedOnly)
                return RespondGuided(intent, c, out text, out gesture);
            if (intent == SparkIntent.Observe && step == Step.Experiment)
            {
                c.UploadFreeTopic = active.Id;
                string result = active.Observation == SparkProObservationKind.SolvedTwoTerminal
                    ? SparkProDiagnostics.Measure(c.Level, c.Object) : experiment != null ? experiment() : "Experiment unavailable.";
                bool verifiedIsolated = result != null && result.StartsWith("Isolated solver", StringComparison.Ordinal);
                bool verifiedCurrent = result != null && result.StartsWith("Current solver result", StringComparison.Ordinal) && c.HasFreshSolve;
                if (!verifiedIsolated && !verifiedCurrent)
                {
                    text = result + " A verified observation is still needed before we discuss the outcome.";
                    gesture = SparkGesture.Inspect;
                    return true;
                }
                c.Academy?.RecordLesson(active.Id, "measurement", true, false, "");
                step = Step.Discussion;
                text = result + "\n" + active.Concept + " Say “next” for independent practice.";
                gesture = SparkGesture.Inspect;
                return true;
            }
            if (intent != SparkIntent.Answer || !AcceptsResponse) { text = Prompt(); return true; }
            string q = Regex.Replace(SparkCortex.Normalize(c.Question), @"^(answer|i predict|my prediction is|i think)\s+", "");
            if (step == Step.Baseline)
            {
                bool recognized = Grade(q, active.BaselineRubric, active.BaselineTerms);
                c.Academy?.RecordLesson(active.Id, "recognition", recognized, assisted, recognized ? "" : active.Misconception);
                if (!recognized) assisted = true;
                step = Step.Prediction;
                string drawing = active.Diagram;
                text = (recognized ? active.Concept : active.Concept + " " + active.Analogy) + "\n" + drawing + "\n" + active.Prediction;
                gesture = SparkGesture.Explain;
                return true;
            }
            bool correct = step == Step.Prediction
                ? Grade(q, active.PredictionRubric, active.PredictionTerms)
                : active.PracticeRubric != null
                    ? active.PracticeRubric.Evaluate(q)
                    : active.Id == "ohm"
                        ? GradeLegacyOhm(q)
                        : Matches(q, active.PracticeTerms);
            if (!correct)
            {
                c.Academy?.RecordLesson(active.Id, step == Step.Practice ? "application" : "understanding", false, assisted, active.Misconception);
                mistakes++;
                assisted = true;
                text = (mistakes == 1 ? active.Misconception : active.Analogy) + " " + Prompt();
                return true;
            }
            c.Academy?.RecordLesson(active.Id, step == Step.Practice ? "application" : "understanding", true, assisted, "");
            if (step == Step.Prediction)
            {
                step = active.Observation == SparkProObservationKind.None ? Step.Discussion : Step.Experiment;
                text = active.Observation == SparkProObservationKind.None
                    ? "Prediction recorded. This lesson has no verified simulation experiment yet; the explanation is theoretical. " + active.Concept + " Say “next” for independent practice."
                    : "Prediction recorded. " + Prompt();
                gesture = SparkGesture.Nod;
                return true;
            }
            text = assisted ? "Correct with guidance. Academy scheduled another review." :
                "Correct independently. Academy recorded this practice and scheduled a spaced review.";
            gesture = SparkGesture.Celebrate;
            Cancel();
            return true;
        }

        private bool RespondGuided(SparkIntent intent, SparkCortex.Context context,
            out string text, out SparkGesture gesture)
        {
            gesture = SparkGesture.Explain;
            text = null;
            // No scoring, no RecordLesson, no solver mutation, no awarding qualifications.
            if (intent == SparkIntent.Observe)
            {
                text = "Use the real mission instruments and compare fresh observations with the authored objective. " +
                    "I won't claim a measurement that has not been obtained from the solver. " + Prompt();
                gesture = SparkGesture.Inspect;
                return true;
            }
            if (intent != SparkIntent.Answer)
            {
                text = Prompt();
                return true;
            }
            if (step == Step.Baseline)
            {
                step = Step.Prediction;
                text = "Here's the evidence we can trust: " + active.Concept + "\n" + active.Diagram +
                    "\nThink about this next: " + active.Prediction;
                return true;
            }
            if (step == Step.Prediction)
            {
                step = Step.Discussion;
                text = "Compare your prediction with this principle: " + active.Misconception +
                    "\nNo actual circuit reading has been verified in this guided lesson. " +
                    "Say ‘next’ to try the authored mission exercise.";
                return true;
            }
            if (step == Step.Practice)
            {
                text = "Use the authoritative game objective checker to verify the result. " +
                    "This automatically generated guide does not evaluate answers or award mastery.";
                gesture = SparkGesture.Nod;
                Cancel();
                return true;
            }
            text = Prompt();
            return true;
        }

        private string Prompt() => step switch
        {
            Step.Baseline => active.Baseline,
            Step.Prediction => active.Prediction,
            Step.Experiment => active.Observation == SparkProObservationKind.SolvedTwoTerminal ?
                "Select a solved two-terminal test component, then say “observe the reading”." :
                "Say “run the experiment” to test this in an isolated circuit.",
            Step.Discussion => "Compare your prediction with the observation. Say “next” to try one yourself.",
            _ => active.Practice
        };

        private static bool Grade(string answer, SparkProAnswerRubric rubric, string[] legacyTerms) =>
            rubric != null ? rubric.Evaluate(answer) : Matches(answer, legacyTerms);

        private static bool GradeLegacyOhm(string answer)
        {
            // This lesson asks for milliamps. Accept 20 mA or 0.02 A, never 20 A.
            return new SparkProAnswerRubric
            {
                kind = SparkProAnswerKind.Numeric, expectedNumber = 20d,
                expectedUnit = "mA", absoluteTolerance = .01d
            }.Evaluate(answer);
        }

        private static bool Matches(string answer, string[] terms)
        {
            if (string.IsNullOrWhiteSpace(answer) || terms == null || terms.Length == 0) return false;
            if (Regex.IsMatch(answer, @"\b(not|never|dont|don't|isn't|isnt|cannot|can't|cant)\b")) return false;
            return terms.Any(term => !string.IsNullOrWhiteSpace(term) &&
                Regex.IsMatch(answer, @"(?<![\p{L}\d])" + Regex.Escape(term) + @"(?![\p{L}\d])"));
        }

        private static Lesson FindLesson(string topic, int currentLevel)
        {
            if (authoredLessons == null) LoadAuthoredLessons();
            // Longest alias wins. Built-in lessons are preserved for legacy requests.
            Lesson best = null;
            int longest = 0;
            Lesson defaultCurrentGuide = null;
            foreach (Lesson lesson in authoredLessons)
            {
                if (lesson == null) continue;
                SparkProLessonDefinition reviewed = lesson.Asset;
                SparkProAutoGuide guide = lesson.AutoGuide;
                if (reviewed != null && !reviewed.AppliesTo(currentLevel)) continue;
                if (guide != null && (currentLevel <= 0 || guide.level != currentLevel)) continue;
                if (reviewed == null && guide == null) continue;
                if (guide != null && defaultCurrentGuide == null) defaultCurrentGuide = lesson;
                string[] aliases = reviewed != null ? reviewed.aliases : guide.aliases;
                if (aliases == null) continue;
                foreach (string alias in aliases)
                {
                    string cleaned = SparkCortex.Normalize(alias);
                    if (cleaned.Length > longest && ContainsPhrase(topic, cleaned))
                    { best = lesson; longest = cleaned.Length; }
                }
            }
            if (best == null && defaultCurrentGuide != null &&
                (topic == "teach me" || topic == "start lesson" || topic == "start a lesson" ||
                 topic == "start the lesson" || topic == "tutorial" ||
                 Regex.IsMatch(topic, @"\b(teach|explain|tutorial|lesson|question|quiz|walk)\b.*\blevel[\s_-]*\d+\b" /* SPARK_TEACHING_RUNTIME_V3_SELECTION */)))
                best = defaultCurrentGuide;
            if (best != null) return best;
            if (ContainsPhrase(topic, "rail") || ContainsPhrase(topic, "motherboard"))
                return currentLevel > 0 && currentLevel <= 20 ? Lessons[0] : Lessons[2];
            if (ContainsPhrase(topic, "ohm") || ContainsPhrase(topic, "resistance") ||
                ContainsPhrase(topic, "resistor") || ContainsPhrase(topic, "current")) return Lessons[1];
            if (ContainsPhrase(topic, "closed path") || ContainsPhrase(topic, "closed paths") || ContainsPhrase(topic, "switch") ||
                ContainsPhrase(topic, "closed circuit") || ContainsPhrase(topic, "complete circuit")) return Lessons[0];
            return topic == "teach me" || topic == "start lesson" || topic == "start a lesson" ||
                topic == "start the lesson" || topic == "tutorial" ? Lessons[0] : null;
        }

        private static bool ContainsPhrase(string haystack, string phrase) => !string.IsNullOrWhiteSpace(phrase) &&
            Regex.IsMatch(haystack ?? "", @"(?<![\p{L}\d])" + Regex.Escape(phrase) + @"(?![\p{L}\d])");

        private static void LoadAuthoredLessons()
        {
            var definitions = Resources.LoadAll<SparkProLessonDefinition>("SparkMasterLessons");
            var accepted = new List<Lesson>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in definitions)
            {
                if (!asset || !asset.IsReady(out _) || string.IsNullOrWhiteSpace(asset.reviewedBy) ||
                    string.IsNullOrWhiteSpace(asset.reviewedUtc) || asset.authoredRevision <= 0 ||
                    string.IsNullOrWhiteSpace(asset.sourceFingerprint)) continue;
                if (!ids.Add(asset.lessonId))
                {
                    Debug.LogWarning("[Spark Teacher] Duplicate lesson ID: " + asset.lessonId);
                    continue;
                }
                accepted.Add(new Lesson
                {
                    Id = asset.lessonId, Name = asset.title, Baseline = asset.baseline.prompt,
                    BaselineRubric = asset.baseline.rubric, Prediction = asset.prediction.prompt,
                    PredictionRubric = asset.prediction.rubric, Practice = asset.practice.prompt,
                    PracticeRubric = asset.practice.rubric, Concept = asset.concept,
                    Analogy = asset.analogy, Diagram = asset.diagram, Misconception = asset.misconception,
                    Visual = asset.visualModel, Observation = asset.observation, Asset = asset
                });
            }
            // Preserve manually reviewed lessons first. Auto guides are deliberately ungraded.
            SparkProAutoGuide[] guides = Resources.LoadAll<SparkProAutoGuide>("SparkAutoGuides");
            Array.Sort(guides, (a, b) => string.CompareOrdinal(a ? a.lessonId : "", b ? b.lessonId : ""));
            foreach (SparkProAutoGuide guide in guides)
            {
                if (!guide || !guide.IsUsable(out _) || !ids.Add(guide.lessonId)) continue;
                accepted.Add(new Lesson
                {
                    Id = guide.lessonId, Name = guide.title, Baseline = guide.baseline,
                    Prediction = guide.prediction, Practice = guide.practice,
                    Concept = guide.concept, Analogy = guide.analogy,
                    Diagram = guide.diagram, Misconception = guide.misconception,
                    Visual = null, Observation = SparkProObservationKind.None,
                    AutoGuide = guide, GuidedOnly = true
                });
            }
            authoredLessons = accepted.ToArray();
        }
    }
}
