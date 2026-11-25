using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    public enum SparkProAnswerKind { AnyTerm, AllTerms, ExactPhrase, Numeric }
    public enum SparkProObservationKind { None, IsolatedSwitch, SolvedTwoTerminal }

    /// <summary>Conservative formative assessment. Not a mission evaluator or certificate authority.</summary>
    [Serializable]
    public sealed class SparkProAnswerRubric
    {
        public SparkProAnswerKind kind = SparkProAnswerKind.AnyTerm;
        [Tooltip("For keyword kinds: use unambiguous, independently sufficient terms or phrases.")]
        public string[] acceptedTerms = Array.Empty<string>();
        public double expectedNumber;
        [Min(0)] public double absoluteTolerance = 0.01;
        [Tooltip("For numeric answers: mA, A, V, ohm, kOhm, W, mW, ms, s, Hz, J; leave blank for unitless.")]
        public string expectedUnit = "";

        public bool HasAnswer()
        {
            if (kind == SparkProAnswerKind.Numeric) return !double.IsNaN(expectedNumber) &&
                !double.IsInfinity(expectedNumber) && absoluteTolerance >= 0;
            return acceptedTerms != null && acceptedTerms.Length > 0 &&
                Array.TrueForAll(acceptedTerms, term => !string.IsNullOrWhiteSpace(term));
        }

        public bool Evaluate(string raw)
        {
            if (!HasAnswer() || string.IsNullOrWhiteSpace(raw)) return false;
            string answer = Clean(raw);
            if (kind == SparkProAnswerKind.Numeric)
                return TryQuantity(answer, expectedUnit, out double number) &&
                       Math.Abs(number - expectedNumber) <= Math.Max(0, absoluteTolerance);
            if (kind == SparkProAnswerKind.ExactPhrase)
            {
                foreach (string term in acceptedTerms)
                    if (answer == Clean(term)) return true;
                return false;
            }
            // Deliberately conservative: a negated keyword is not evidence of understanding.
            if (Regex.IsMatch(answer, @"\b(not|never|dont|don't|isn't|isnt|won't|wont|cannot|can't|cant)\b"))
                return false;
            bool any = false;
            foreach (string term in acceptedTerms)
            {
                string escaped = Regex.Escape(Clean(term));
                bool matched = Regex.IsMatch(answer, @"(?<![\p{L}\d])" + escaped + @"(?![\p{L}\d])");
                if (kind == SparkProAnswerKind.AllTerms && !matched) return false;
                any |= matched;
            }
            return any;
        }

        private static string Clean(string s) => Regex.Replace((s ?? "").Trim().ToLowerInvariant(), @"\s+", " ");

        private static bool TryQuantity(string answer, string wanted, out double normalized)
        {
            normalized = 0;
            answer = Regex.Replace(answer, @"^(answer|i think|it is|my answer is)\s+", "");
            // Natural speech from Vosk may produce "twenty milliamps" rather than "20 mA".
            // Use the existing Academy number parser, but never silently mix incompatible units.
            const string suffix = @"(milliamperes?|milliamps?|ma|amperes?|amps?|a|millivolts?|mv|volts?|v|milliwatts?|mw|watts?|w|kiloohms?|kohms?|kω|ohms?|ω|milliseconds?|ms|seconds?|s|hertz|hz|joules?|j)";
            string numericPart = answer;
            string unitPart = "";
            Match detected = Regex.Match(answer, @"^(.+?)\s*" + suffix + @"$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (detected.Success)
            {
                numericPart = detected.Groups[1].Value.Trim();
                unitPart = detected.Groups[2].Value;
            }
            if (!double.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) &&
                !SparkProAcademy.TryNumber(numericPart, out value)) return false;
            if (double.IsNaN(value) || double.IsInfinity(value)) return false;
            string found = CanonUnit(unitPart);
            string expected = CanonUnit(wanted);
            if (expected == "?" || found == "?") return false;
            if (found.Length == 0 || found == expected) { normalized = value; return true; }
            if (TryScale(found, out string baseUnit, out double scale) &&
                TryScale(expected, out string expectedBase, out double targetScale) && baseUnit == expectedBase)
            {
                normalized = value * scale / targetScale;
                return true;
            }
            return false;
        }

        private static string CanonUnit(string v)
        {
            switch ((v ?? "").ToLowerInvariant().Trim())
            {
                case "": return "";
                case "a": case "amp": case "amps": case "ampere": case "amperes": return "a";
                case "ma": case "milliamp": case "milliamps": case "milliampere": case "milliamperes": return "ma";
                case "v": case "volt": case "volts": return "v";
                case "mv": case "millivolt": case "millivolts": return "mv";
                case "w": case "watt": case "watts": return "w";
                case "mw": case "milliwatt": case "milliwatts": return "mw";
                case "ohm": case "ohms": case "ω": case "Ω": return "ohm";
                case "kohm": case "kohms": case "kiloohm": case "kiloohms": case "kω": case "kΩ": return "kohm";
                case "s": case "second": case "seconds": return "s";
                case "ms": case "millisecond": case "milliseconds": return "ms";
                case "hz": case "hertz": return "hz";
                case "j": case "joule": case "joules": return "j";
                default: return "?";
            }
        }

        private static bool TryScale(string unit, out string baseUnit, out double scale)
        {
            scale = 1;
            baseUnit = unit;
            switch (unit)
            {
                case "ma": baseUnit = "a"; scale = .001; break;
                case "mv": baseUnit = "v"; scale = .001; break;
                case "mw": baseUnit = "w"; scale = .001; break;
                case "ms": baseUnit = "s"; scale = .001; break;
                case "kohm": baseUnit = "ohm"; scale = 1000; break;
            }
            return unit != "?";
        }
    }

    [Serializable]
    public sealed class SparkProLessonQuestion
    {
        [TextArea(2, 5)] public string prompt;
        public SparkProAnswerRubric rubric = new SparkProAnswerRubric();
    }

    [CreateAssetMenu(menuName = "Project Spark/Super AI/Reviewed Teaching Lesson", fileName = "SparkLesson_New")]
    public sealed class SparkProLessonDefinition : ScriptableObject
    {
        [Header("Authored Identity")]
        public string lessonId;
        [TextArea(2, 8)] public string authoredObjectiveExcerpt;
        public string[] authoredVisualReferences = Array.Empty<string>();
        public string[] authoredPracticeReferences = Array.Empty<string>();
        public string[] authoredAssessmentReferences = Array.Empty<string>();
        public string[] authoredPrerequisites = Array.Empty<string>();
        public string[] suggestedConceptTerms = Array.Empty<string>();
        public string sourceFingerprint;
        [Header("Manual Approval") ]
        [Tooltip("Name or identifier of the actual human reviewer. Required to publish, not automatically filled.")]
        public string reviewedBy;
        public string reviewedUtc;
        public int authoredRevision;
        public string title;
        [Tooltip("Only manually reviewed and correctly authored lessons are available in game.")]
        public bool approvedForTeaching;
        public string[] aliases = Array.Empty<string>();
        [Tooltip("Optional exact applicable levels; empty means general lesson.")]
        public int[] applicableLevels = Array.Empty<int>();
        [Tooltip("Exact objective identifiers for coverage audit; not automatically inferred.")]
        public string[] objectiveIds = Array.Empty<string>();
        [Tooltip("Curated educational reference or source asset path(s).")]
        public string[] sourceReferences = Array.Empty<string>();
        [Header("Teaching")]
        [TextArea(3, 9)] public string concept;
        [TextArea(3, 9)] public string analogy;
        [TextArea(1, 5)] public string diagram;
        [TextArea(2, 7)] public string misconception;
        public string visualModel;
        public SparkProLessonQuestion baseline = new SparkProLessonQuestion();
        public SparkProLessonQuestion prediction = new SparkProLessonQuestion();
        public SparkProLessonQuestion practice = new SparkProLessonQuestion();
        public SparkProObservationKind observation = SparkProObservationKind.None;

        public bool AppliesTo(int level)
        {
            if (applicableLevels == null || applicableLevels.Length == 0) return true;
            if (level <= 0) return false;
            return Array.IndexOf(applicableLevels, level) >= 0;
        }
        public bool CoversObjective(int level, string id)
        {
            if (!AppliesTo(level) || applicableLevels == null || applicableLevels.Length == 0 ||
                objectiveIds == null || string.IsNullOrEmpty(id)) return false;
            foreach (string candidate in objectiveIds)
                if (string.Equals(candidate, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        public bool IsReady(out string problem)
        {
            problem = null;
            if (!approvedForTeaching) { problem = "Not approved for teaching"; return false; }
            if (string.IsNullOrWhiteSpace(lessonId) || string.IsNullOrWhiteSpace(title))
            { problem = "Missing identity"; return false; }
            if (aliases == null || aliases.Length == 0 || Array.Exists(aliases, string.IsNullOrWhiteSpace))
            { problem = "Missing searchable aliases"; return false; }
            if (string.IsNullOrWhiteSpace(concept) || string.IsNullOrWhiteSpace(misconception))
            { problem = "Missing explanation or misconception"; return false; }
            if (baseline == null || prediction == null || practice == null ||
                string.IsNullOrWhiteSpace(baseline.prompt) || string.IsNullOrWhiteSpace(prediction.prompt) ||
                string.IsNullOrWhiteSpace(practice.prompt) ||
                baseline.rubric == null || prediction.rubric == null || practice.rubric == null ||
                !baseline.rubric.HasAnswer() || !prediction.rubric.HasAnswer() || !practice.rubric.HasAnswer())
            { problem = "Missing question or assessment rubric"; return false; }
            if (sourceReferences == null || sourceReferences.Length == 0 ||
                Array.Exists(sourceReferences, string.IsNullOrWhiteSpace))
            { problem = "Missing provenance"; return false; }
            return true;
        }
    }
}
