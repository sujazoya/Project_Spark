using System;
using System.Linq;
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

        private Lesson active;
        private Step step;
        private bool paused, assisted;
        private int mistakes;
        public string VisualModel => active?.Id switch { "ohm" => "resistor", "closed path" => "switch", "rail diagnosis" => "rdx-21 board", _ => null };
        public bool HasSession => active != null;
        public string Active => active == null ? "" : $"{active.Name} · {step}{(paused ? " · paused" : "")}";
        public bool AcceptsResponse => active != null && !paused && step is Step.Baseline or Step.Prediction or Step.Practice;

        public void Cancel() { active = null; paused = assisted = false; mistakes = 0; }

        public bool TryRespond(SparkIntent intent, SparkCortex.Context c, Func<string> experiment,
            out string text, out SparkGesture gesture)
        {
            text = null;
            gesture = SparkGesture.Explain;
            if (intent == SparkIntent.Lesson)
            {
                string topic = SparkCortex.Normalize(c.Question);
                if (Regex.IsMatch(topic, @"\blevel[\s_-]*\d")) return false;
                active = topic.Contains("rail") || topic.Contains("motherboard") ? Lessons[2] :
                    topic.Contains("ohm") || topic.Contains("resist") || topic.Contains("current") ? Lessons[1] : Lessons[0];
                if (active == Lessons[2] && c.Level.Definition && c.Level.Definition.LevelNumber <= 20)
                    active = Lessons[0];
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
            if (intent == SparkIntent.Observe && step == Step.Experiment)
            {
                c.UploadFreeTopic = active.Id;
                string result = active.Id == "rail diagnosis"
                    ? SparkProDiagnostics.Measure(c.Level, c.Object) : experiment();
                if (!result.StartsWith("Isolated solver", StringComparison.Ordinal) &&
                    !result.StartsWith("Current solver result", StringComparison.Ordinal))
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
                bool recognized = Matches(q, active.BaselineTerms);
                c.Academy?.RecordLesson(active.Id, "recognition", recognized, assisted, recognized ? "" : active.Misconception);
                step = Step.Prediction;
                string drawing = active.Diagram;
                text = (recognized ? active.Concept : active.Concept + " " + active.Analogy) + "\n" + drawing + "\n" + active.Prediction;
                gesture = SparkGesture.Explain;
                return true;
            }
            bool correct = step == Step.Prediction ? Matches(q, active.PredictionTerms) :
                active.Id == "ohm" ? SparkProAcademy.TryNumber(Regex.Replace(q, @"\s*(ma|milliamps?)$", ""), out double value) && Math.Abs(value - 20d) < .01 :
                Matches(q, active.PracticeTerms);
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
                step = Step.Experiment;
                text = "Prediction recorded. " + Prompt();
                gesture = SparkGesture.Nod;
                return true;
            }
            text = assisted ? "Correct with guidance. Academy scheduled another review." :
                "Correct independently. Academy recorded this practice and scheduled a spaced review.";
            gesture = SparkGesture.Celebrate;
            Cancel();
            return true;
        }

        private string Prompt() => step switch
        {
            Step.Baseline => active.Baseline,
            Step.Prediction => active.Prediction,
            Step.Experiment => active.Id == "rail diagnosis" ?
                "Select a solved two-terminal test component, then say “observe the reading”." :
                "Say “run the experiment” to test this in an isolated circuit.",
            Step.Discussion => "Compare your prediction with the observation. Say “next” to try one yourself.",
            _ => active.Practice
        };

        private static bool Matches(string answer, string[] terms) => terms.Any(term =>
            Regex.IsMatch(answer, @"\b" + Regex.Escape(term) + @"\b"));
    }
}
