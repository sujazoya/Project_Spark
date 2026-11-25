#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Authoring diagnostics. Does not generate invented mission or electrical requirements.</summary>
    public static class SparkProTeacherAuthoring
    {
        const string ResourceRoot = "Assets/ProjectSpark/SparkV5/Pro/MasterTeacher/Authoring/Examples";
        const string CardsRelativePath = "Assets/ProjectSpark/SparkV5/Pro/Generated/AuthoredObjectiveCards.json";
        [Serializable] sealed class Cards { public int schemaVersion; public Level[] levels; }
        [Serializable] sealed class Level { public int level; public string source; public Objective[] objectives; }
        [Serializable] sealed class Objective { public string id, title; public string[] concepts, assessment, misconceptions; }

        [MenuItem("Project Spark/Spark V5 Pro/Academy/Create Blank Lesson Asset")]
        public static void CreateBlankLesson()
        {
            EnsureDirectories(ResourceRoot);
            var asset = ScriptableObject.CreateInstance<SparkProLessonDefinition>();
            asset.lessonId = "new-teaching-lesson";
            asset.title = "New teaching lesson (edit me)";
            asset.approvedForTeaching = false;
            asset.aliases = new[] { "replace with unique topic" };
            asset.sourceReferences = new[] { "REPLACE WITH REAL AUTHORED SOURCE PATH" };
            string path = AssetDatabase.GenerateUniqueAssetPath(ResourceRoot + "/SparkLesson_Draft.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log("[Spark Teacher] Created an unapproved lesson draft at " + path);
        }

        [MenuItem("Project Spark/Spark V5 Pro/Academy/Create Six Starter Lesson Drafts")]
        public static void CreateStarterDrafts()
        {
            EnsureDirectories(ResourceRoot);
            AddDraft("led polarity", "LED polarity", "led polarity", "diode polarity", "led anode",
                "An LED is a diode. In a typical indicator circuit, correct forward bias and controlled current are required for light emission.",
                "A one-way turnstile represents current direction, but an actual diode has electrical thresholds and maximum ratings.",
                "[DC source +] → [Current limiter] → [LED anode | cathode] → [return]",
                "Reversing a conventional indicator LED in a low-voltage DC circuit normally prevents forward conduction; never exceed its reverse rating.",
                "Which LED terminal is normally the positive side in forward bias?", Terms("anode"),
                "In a normal indicator circuit, will an LED illuminate when reverse connected?", Terms("no", "off", "dark"),
                "Before energizing an LED, what terminal property should you verify?", Terms("polarity", "anode", "cathode"));
            AddDraft("series circuit", "Series circuits", "series resistors", "series circuit", "series resistance",
                "Ideal resistors in series carry the same current. Their resistances add together, assuming the network has no other branches.",
                "A single pathway carries the same queue through every checkpoint.",
                "[Supply] → [R1] → [R2] → [Return]",
                "Adding series resistance at a fixed ideal DC supply reduces current.",
                "Do ideal resistors connected strictly in series carry the same current?", Terms("yes", "same", "equal"),
                "At a fixed voltage, adding series resistance makes current do what?", Terms("decrease", "lower", "less"),
                "What is the total resistance of 100 ohms and 220 ohms in series? Answer in ohms.", Number(320,"ohm",.01));
            AddDraft("parallel circuit", "Parallel circuits", "parallel resistors", "parallel circuit", "parallel resistance",
                "Components sharing both nodes are in parallel and have the same voltage. Adding a finite-resistance branch decreases total equivalent resistance.",
                "Two paths between the same two junctions share the same starting and ending elevations.",
                "[Supply +] → (branch R1 || branch R2) → [Return]",
                "A lower equivalent resistance at fixed voltage draws a larger total current; check source current limits.",
                "For ideal components connected between the same two nodes, what electrical quantity is the same?", Terms("voltage", "potential difference"),
                "Adding another positive-resistance parallel branch reduces or increases equivalent resistance?", Terms("decreases", "reduces", "lower", "less"),
                "Two 200 ohm resistors are in parallel. What is the equivalent resistance in ohms?", Number(100,"ohm",.01));
            AddDraft("voltage measurement", "Measuring DC voltage", "measure voltage", "voltage measurement", "multimeter voltage",
                "A voltmeter measures potential difference between two points. Set the correct mode and connect probes across the points of interest.",
                "Measuring voltage is like comparing the heights of two places relative to the same reference.",
                "[Meter red] → [Rail test point]  ||  [Meter black] → [Documented reference]",
                "A negative DC reading can indicate that probe polarity is reversed relative to the measured potential, not automatically a faulty supply.",
                "Is a voltmeter normally connected across or in series with a component?", Terms("across", "parallel"),
                "If the probes are reversed while measuring a positive DC potential, what sign may the reading have?", Terms("negative", "minus"),
                "Before measuring an unknown voltage rail, what electrical point must you identify?", Terms("reference", "ground", "return"));
            AddDraft("electrical power", "Electrical power", "resistor power", "electrical power", "power calculation",
                "For steady DC conditions, electrical power is P = V × I, using volts and amperes to obtain watts.",
                "Power measures the rate of energy transfer, not the total energy used over time.",
                "[Source] → [Load]  P = V × I",
                "A resistor's power rating must be checked against calculated dissipation, with suitable design margin.",
                "What SI unit is electrical power measured in?", Terms("watt", "watts"),
                "At fixed voltage, doubling current does what to power?", Terms("double", "doubles", "twice"),
                "A load has 5 volts across it and draws 20 milliamps. What is its power in milliwatts?", Number(100,"mW",.01));
            AddDraft("rc time constant", "RC timing", "rc time", "time constant", "resistor capacitor timing",
                "For an ideal first-order RC network, the time constant tau equals R × C; it describes the characteristic response timescale.",
                "A reservoir connected through a narrow pipe takes time to reach the new level.",
                "[Voltage step] → [R] → [C to reference]  τ = RC",
                "One time constant does not mean a capacitor has reached its final charge or discharge voltage.",
                "Which two component quantities determine the ideal RC time constant?", TermsAll("resistance", "capacitance"),
                "If resistance doubles with capacitance fixed, what happens to the time constant?", Terms("doubles", "double", "twice"),
                "A 1 kilohm resistor and a 100 microfarad capacitor have what RC time constant in milliseconds?", Number(100,"ms",.01));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Spark Teacher] Starter draft creation finished. Review, link to authored objectives and approve EACH lesson before runtime use.");
        }

        private static SparkProAnswerRubric Terms(params string[] terms) =>
            new SparkProAnswerRubric { kind = SparkProAnswerKind.AnyTerm, acceptedTerms = terms };
        private static SparkProAnswerRubric TermsAll(params string[] terms) =>
            new SparkProAnswerRubric { kind = SparkProAnswerKind.AllTerms, acceptedTerms = terms };
        private static SparkProAnswerRubric Number(double expected, string unit, double tolerance) =>
            new SparkProAnswerRubric { kind = SparkProAnswerKind.Numeric, expectedNumber = expected,
                expectedUnit = unit, absoluteTolerance = tolerance };

        private static void AddDraft(string id, string title, string alias0, string alias1, string alias2,
            string concept, string analogy, string diagram, string misconception,
            string baseline, SparkProAnswerRubric baselineRubric,
            string prediction, SparkProAnswerRubric predictionRubric,
            string practice, SparkProAnswerRubric practiceRubric)
        {
            string path = ResourceRoot + "/SparkLesson_" + id.Replace(' ', '_') + ".asset";
            if (AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(path)) return;
            var lesson = ScriptableObject.CreateInstance<SparkProLessonDefinition>();
            lesson.lessonId = id; lesson.title = title;
            lesson.approvedForTeaching = false;
            lesson.aliases = new[] { alias0, alias1, alias2 };
            lesson.sourceReferences = new[] { "Editorial starter: verify against authoritative level assets before approval" };
            lesson.concept = concept; lesson.analogy = analogy;
            lesson.diagram = diagram; lesson.misconception = misconception;
            lesson.baseline = new SparkProLessonQuestion { prompt = baseline, rubric = baselineRubric };
            lesson.prediction = new SparkProLessonQuestion { prompt = prediction, rubric = predictionRubric };
            lesson.practice = new SparkProLessonQuestion { prompt = practice, rubric = practiceRubric };
            lesson.observation = SparkProObservationKind.None;
            AssetDatabase.CreateAsset(lesson, path);
            EditorUtility.SetDirty(lesson);
        }

        [MenuItem("Project Spark/Spark V5 Pro/Academy/Audit All Objective Teaching")]
        public static void AuditAllTeaching()
        {
            var root = JsonRoot();
            if (root == null || root.levels == null) return;
            string[] ids = AssetDatabase.FindAssets("t:SparkProLessonDefinition");
            var lessons = new List<SparkProLessonDefinition>();
            var errors = new List<string>();
            var duplicate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in ids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(path);
                if (!asset) continue;
                if (!asset.IsReady(out string reason) || string.IsNullOrWhiteSpace(asset.reviewedBy) ||
                    string.IsNullOrWhiteSpace(asset.reviewedUtc) || asset.authoredRevision <= 0)
                {
                    errors.Add(path + " :: " + (reason ?? "Missing reviewer/stamped publication"));
                    continue;
                }
                if (!duplicate.Add(asset.lessonId))
                {
                    errors.Add(path + " :: duplicate approved lesson ID " + asset.lessonId);
                    continue;
                }
                if (!path.Replace('\\', '/').Contains("/Resources/SparkMasterLessons/"))
                {
                    errors.Add(path + " :: approved lesson is outside runtime Resources/SparkMasterLessons");
                    continue;
                }
                lessons.Add(asset);
            }
            var lines = new List<string> { "Level,ObjectiveId,ObjectiveTitle,ReviewedLessonCount,LessonIds,Source" };
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int levelCount = 0, objectiveCount = 0, mapped = 0, duplicateObjectives = 0;
            foreach (var level in root.levels)
            {
                if (level?.objectives == null) continue;
                levelCount++;
                foreach (var objective in level.objectives)
                {
                    if (objective == null) continue;
                    objectiveCount++;
                    string key = level.level + ":" + objective.id;
                    if (!seenKeys.Add(key)) duplicateObjectives++;
                    string[] related = lessons.Where(x => x.CoversObjective(level.level, objective.id))
                        .Select(x => x.lessonId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                    if (related.Length > 0) mapped++;
                    lines.Add(string.Join(",", new[]
                    {
                        Csv(level.level.ToString()), Csv(objective.id), Csv(objective.title),
                        Csv(related.Length.ToString()), Csv(string.Join(";", related)), Csv(level.source)
                    }));
                }
            }
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string logDir = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDir);
            string csv = Path.Combine(logDir, "SparkMasterTeacherCoverage.csv");
            File.WriteAllLines(csv, lines, new UTF8Encoding(true));
            string summary = "SPARK MASTER TEACHER COVERAGE\n" +
                "Source: " + CardsRelativePath + "\n" +
                "Levels in cards: " + levelCount + "\n" +
                "Objective records: " + objectiveCount + "\n" +
                "Objectives with an explicit approved teaching lesson: " + mapped + "\n" +
                "Objectives still needing reviewed lesson linkage: " + (objectiveCount - mapped) + "\n" +
                "Duplicate objective keys: " + duplicateObjectives + "\n" +
                "Valid approved lesson assets in Resources: " + lessons.Count + "\n" +
                "Rejected/draft lesson assets: " + errors.Count + "\n" +
                "WARNING: a linked approved lesson is NOT proof of independent practical competence, objective completion, or certification.\n\n" +
                string.Join("\n", errors);
            string report = Path.Combine(logDir, "SparkMasterTeacherCoverage.txt");
            File.WriteAllText(report, summary, new UTF8Encoding(true));
            Debug.Log("[Spark Teacher] Teaching audit written: " + report + "\n" + summary);
            EditorUtility.RevealInFinder(report);
        }

        private static Cards JsonRoot()
        {
            // The original asset's literal path is resolved in this specific uploaded project.
            string path = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..")), CardsRelativePath);
            if (!File.Exists(path))
            {
                Debug.LogError("[Spark Teacher] Objective card export not found: " + path +
                    "\nRun Project Spark knowledge compilation first.");
                return null;
            }
            try { return JsonUtility.FromJson<Cards>(File.ReadAllText(path)); }
            catch (Exception ex) { Debug.LogError("[Spark Teacher] Invalid objective-card JSON: " + ex.Message); return null; }
        }

        private static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";

        private static void EnsureDirectories(string path)
        {
            string current = "Assets";
            foreach (string piece in path.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + piece;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, piece);
                current = next;
            }
        }
    }
}
#endif
