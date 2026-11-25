#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>
    /// Generates source-grounded, UNGRADED, directly usable tutorials for all 130 authored levels.
    /// It never approves human-reviewed lessons, invents measurements, or awards mission progress.
    /// </summary>
    public static class SparkMasterTeacherAutoGenerator
    {
        public const string GuideRoot = "Assets/ProjectSpark/SparkV5/Pro/Integration/Resources/SparkAutoGuides";
        public const string RecipeVersion = "AutoGuide/2.0/2026-11";
        private const string MenuRoot = "Project Spark/Spark V5 Pro/Master Teacher/";
        private static bool building;

        private sealed class Rule
        {
            public readonly string id, pattern, principle, analogy, caveat;
            public Rule(string id, string pattern, string principle, string analogy, string caveat)
            { this.id = id; this.pattern = pattern; this.principle = principle; this.analogy = analogy; this.caveat = caveat; }
        }

        // Curated general scientific principles, not fabricated assertions about individual level results.
        // Ordered from specific to generic. Never infer electrical measurements from these templates.
        private static readonly Rule[] Rules =
        {
            new Rule("led", @"\bled\b|light.emitting diode", "An LED has a defined anode and cathode; correct polarity and suitable current control matter for normal forward operation.", "A directional gate with a current limit is a useful model, although an LED is not an ideal one-way switch.", "A lit LED alone does not establish that its current and power are within safe ratings."),
            new Rule("diode", @"\bdiodes?\b|rectif|reverse.bias|forward.bias", "Diode behavior depends on polarity, operating point and its device characteristics.", "A direction-sensitive valve is an approximation, not a complete semiconductor model.", "Reverse bias and forward conduction must not be inferred from wire direction alone."),
            new Rule("rc", @"\bcapacitor\b|\bcapacitive\b|\brc\b|\bcharge\b|discharg", "Capacitor voltage and current vary with circuit state; transient observations need a stated time and reference.", "A storage tank illustrates temporary storage, but electrical capacitance has precise circuit equations.", "A transient reading is not automatically a settled reading."),
            new Rule("bridge", @"wheatstone|bridge.balance|midpoint.*bridge|\bbridge\b", "Bridge measurements compare relevant branch conditions and reference points; balance must be verified rather than assumed.", "Comparing two scales is useful only when both use compatible reference points.", "A single matching midpoint voltage is not always sufficient to establish a healthy bridge."),
            new Rule("divider", @"voltage.divider|loaded.divider|divider", "Resistive division depends on the actual network and any connected load; a loaded divider is not the same as an unloaded one.", "Sharing a resource between branches changes when another load is connected.", "Do not apply an unloaded voltage-divider formula to an arbitrary loaded network."),
            new Rule("resistor", @"\bresistor\b|\bohm\b|\bresistance\b", "Resistance, voltage and current are linked by Ohm's law for ideal resistors, while power dissipation needs separate evaluation.", "A flow restriction can suggest resistance; unlike a pipe, a resistor follows electrical laws.", "Current and resistor power cannot be established without voltage/current evidence and component values."),
            new Rule("pcb", @"\bpcb\b|\bfootprint\b|\bvia\b|\btrace\b|\blayer\b|\bnetlist\b|\broute\b|\bschematic\b", "PCB acceptance depends on the authored footprints, nets, layers, clearances and route constraints; visual appearance alone is insufficient.", "A multilayer road map helps visualize routing, but electrical continuity and constraints still require checks.", "A visually connected trace is not proof of correct net connectivity or clearance."),
            new Rule("memory", @"\bdram\b|\bddr[345]?\b|\bmemory\b|\brom\b|\bfirmware\b|\bpost\b|\bnvme\b", "Memory and initialization workflows require checking power, connectivity, configuration and the authored test responses in the correct sequence.", "A staged startup checklist helps distinguish a missing prerequisite from a later failure.", "One passing startup indicator does not guarantee every memory or firmware check has passed."),
            new Rule("microcontroller", @"\bmcu\b|\bmicrocontroller\b|\bgpio\b|\bdac\b|\badc\b|\bpwm\b", "Digital and mixed-signal functions must be checked using the correct supply, reference, timing and observed input/output relationship.", "Sampling a changing signal resembles checking snapshots; timing changes what a snapshot means.", "A configured output setting is not proof of the measured output waveform."),
            new Rule("bus", @"\bi2c\b|\bspi\b|\buart\b|\bpcie\b|\bpcie\b|\bbus\b|\blane\b|\blink\b", "Communication links require compatible endpoints, timing, signal integrity and the authored link tests.", "Two speakers must agree on a language and timing, not merely share a cable.", "Physical connectivity alone does not establish successful protocol communication."),
            new Rule("robotics", @"\baxis\b|\baxes\b|\bservo\b|\btorque\b|\bjoint\b|\bkinematic\b|\brobot\b|\bactuat", "Motion validation must consider correct reference, feedback, interlocks and authored limits before declaring a movement safe or accurate.", "A coordinated team needs agreement on position and timing before everyone moves.", "Motion feedback and a command signal are not interchangeable evidence of successful positioning."),
            new Rule("motor", @"\bmotor\b|\bconveyor\b|\brpm\b|\bdrive\b|\bshaft\b|\bbrake\b", "Motor behavior depends on drive conditions, load, feedback and protective limits; test the observed state at each authored checkpoint.", "A controlled wheel under varying load shows why a command does not always equal actual speed.", "A drive command alone does not establish speed, torque or safe stopping."),
            new Rule("encoder", @"\bencoder\b|\bquadrature\b|\bindex\b|\bhome\b|\bposition\b", "Position and speed feedback depend on sensor reference, signal ordering and calibration.", "A marked ruler is useful only after its zero point and counting direction are established.", "Reported counts without a known reference may not represent true position."),
            new Rule("control", @"\bpid\b|\bfeedback\b|\bgovernor\b|\bcontroller\b|\bclosed.loop\b|\bdroop\b", "A feedback controller compares measured behavior with a reference and acts subject to plant dynamics and limits.", "Steering toward a target works only if the feedback accurately describes where you are.", "Improved command tracking in one condition does not establish stability under disturbances."),
            new Rule("safety", @"\binterlock\b|\bprotect\b|\bemergency.stop\b|\bfuse\b|\btrip\b|\bsafety\b", "Protection logic must be verified under both normal and fault conditions, including reset and recovery behavior.", "A safety lock must prevent unintended activation even when the normal switch would permit it.", "Passing a normal-operation test is not proof that the protection system responds correctly to faults."),
            new Rule("relay", @"\brelay\b|\bcontactor\b|\bswitch\b|\bseal.in\b|\blatch\b", "A switching circuit changes connectivity or state according to its control and the authored interlock logic.", "A gate that opens only when its control condition is met helps explain switch states.", "A control command does not prove that the expected contact or load state actually changed."),
            new Rule("power", @"\bvrm\b|\bregulator\b|\brail\b|\bsupply\b|\bvoltage\b|\bcurrent\b|\binverter\b|\bpower\b", "Electrical measurements are differences or flows that require a correct reference, operating condition and fresh solver evidence.", "A pressure gauge needs a reference; a voltage measurement likewise depends on its defined points.", "An expected rail or current is not a measured rail or current until the live circuit evidence confirms it."),
            new Rule("thermal", @"\bthermal\b|\btemperature\b|\bheat\b|\bcooling\b|\bfan\b|\brefrigerat\b", "Thermal control depends on measured temperatures, loads, feedback and operating limits over time.", "A thermostat illustrates the need to distinguish a measured condition from a chosen setpoint.", "A fan command by itself does not prove that thermal protection is effective."),
            new Rule("sensor", @"\bsensor\b|\blidar\b|\bimu\b|\bcalibrat\b|\bprobe\b|\bmetrology\b", "Sensor outputs require proper calibration, reference and freshness before they can support a diagnosis.", "A measuring ruler is useful only if zero and units are correctly established.", "An old, uncalibrated or missing sensor reading is not reliable evidence."),
            new Rule("optics", @"\boptic\b|\blaser\b|\bphoton\b|\bwavelength\b|\bimage\b|\bcamera\b", "Optical systems need alignment, calibrated detection and verified signal conditions for the authored result.", "Pointing a flashlight in the right direction is a first step, not proof that the receiver measures enough light.", "Visible illumination alone is not a calibrated optical power or quality measurement."),
            new Rule("semiconductor", @"\bwafer\b|\bplasma\b|\betch\b|\bion\b|\bdopant\b|\blithograph\b|\bsemiconductor\b", "Process validation depends on the authored sequence, tool-state evidence and appropriate measurement of the resulting material or device.", "A multi-step recipe needs each stage verified before a final quality check can be trusted.", "Completing a process command does not guarantee that material quality meets the authored target."),
            new Rule("grid", @"\bmicrogrid\b|\bgrid\b|\bgenerator\b|\bfeeder\b|\bblack.start\b|\bfrequency\b", "Power networks require reference conditions, protection coordination and validated dynamic behavior before synchronism or restoration can be declared.", "Two teams must match pace and direction before joining efforts; connected grids also require compatible conditions.", "Successful energization alone does not guarantee stable or protected grid operation."),
            new Rule("space", @"\borbital\b|\bdocking\b|\bspacecraft\b|\bpropulsion\b|\bthruster\b|\bxenon\b|\bavionic\b", "Mission control must respect the authored states, calibrated references and safety constraints; simulation evidence is required before declaring a maneuver complete.", "Navigation uses measured position and timing rather than trusting a command as the result.", "Issued maneuvers are not proof of achieved position, attitude or thrust."),
            new Rule("process", @"\bprocess\b|\bpump\b|\bflow\b|\bpressure\b|\bfactory\b|\bproduction\b|\bsort\b", "Process objectives depend on verified input conditions, feedback, actuator behavior and the authored acceptance criteria.", "A production checklist helps separate setting a machine from verifying a good output.", "A completed machine cycle is not automatically a passing quality measurement."),
            new Rule("diagnostics", @"\bdiagnos\b|\brepair\b|\bfault\b|\brecover\b|\brestore\b|\bfailure\b", "Troubleshooting compares fresh evidence with a known reference, isolates likely causes, repairs the supported issue and retests.", "Debugging a chain is easier when each link is tested before replacing the next.", "An absent output is a symptom, not definitive proof that one specific component has failed."),
            new Rule("assembly", @"\bassemble\b|\bconstruct\b|\bbuild\b|\bwire\b|\binstall\b|\bcommission\b", "Assembly should follow the authored parts and connections and be validated with the game's authoritative inspection or objective checks.", "Following an assembly plan prevents a plausible-looking but incorrectly connected build.", "A visually complete assembly is not proof of every required terminal, orientation or inspection check."),
            new Rule("generic", @".*", "The objective must be evaluated using the original authored acceptance criteria and fresh game evidence; do not infer facts not in the source.", "A checklist describes what to verify; checking a box must still depend on actual evidence.", "A stated target or plan is not proof of actual completion.")
        };

        [Serializable] private sealed class BuildSummary
        {
            public string generatedUtc;
            public string recipeVersion;
            public int levels, objectives, created, updated, unchanged, invalidated, blocked, guidedAvailable;
            public int autoGraded, automaticallyApproved;
            public string[] warnings;
        }

        [MenuItem(MenuRoot + "6 — ONE CLICK AUTO BUILD ALL 130 LEVELS")]
        public static void OneClick()
        {
            if (building) return;
            SparkMasterTeacherImporter.ImportStats imported = SparkMasterTeacherImporter.ImportAll(false);
            if (imported == null || imported.blocked > 0)
            {
                Debug.LogError("[Spark Auto Teacher] Import failed or blocked. Auto build stopped.");
                return;
            }
            Generate(false);
            ValidateAndReport(true);
            SparkMasterTeacherImporter.CreateReports(false);
        }

        [MenuItem(MenuRoot + "7 — Validate Auto Guides")]
        public static void ValidateMenu() => ValidateAndReport(true);

        public static void Generate(bool reveal)
        {
            if (building) return;
            if (!SparkMasterTeacherImporter.TryReadCards(out var cards, out string error))
            { Debug.LogError("[Spark Auto Teacher] Source invalid: " + error); return; }
            building = true;
            var stats = new BuildSummary { generatedUtc = DateTime.UtcNow.ToString("O"), recipeVersion = RecipeVersion,
                levels = cards.levels.Length, warnings = Array.Empty<string>() };
            var warnings = new List<string>();
            try
            {
                EnsureFolder(GuideRoot);
                var expected = new HashSet<string>(StringComparer.Ordinal);
                var records = new List<(SparkMasterTeacherImporter.Level level, SparkMasterTeacherImporter.Objective objective,
                    string path, SparkProLessonDefinition draft, Rule rule, string contentSignature)>();
                foreach (var level in cards.levels)
                {
                    foreach (var objective in level.objectives)
                    {
                        stats.objectives++;
                        string id = SparkMasterTeacherImporter.StableLessonId(level.level, objective.id);
                        var draft = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(
                            SparkMasterTeacherImporter.DraftPath(level.level, objective.id));
                        if (!draft || !AssetDatabase.LoadMainAssetAtPath(level.source) ||
                            string.IsNullOrWhiteSpace(draft.sourceFingerprint))
                        { stats.blocked++; warnings.Add("Missing authoritative draft/source: " + id); continue; }
                        string path = GuidePath(level.level, objective.id);
                        expected.Add(path);
                        Rule rule = Categorize(objective.title, Join(objective.explanation));
                        string signature = Signature(draft.sourceFingerprint + "|" + RecipeVersion + "|" + rule.id);
                        records.Add((level, objective, path, draft, rule, signature));
                    }
                }
                if (stats.blocked > 0)
                {
                    warnings.Add("Prevalidation failed. No auto-generated lesson assets were modified.");
                    stats.warnings = warnings.ToArray(); WriteSummary(stats, reveal); return;
                }
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var row in records)
                    {
                        SparkProAutoGuide guide = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(row.path);
                        if (!guide && AssetDatabase.LoadMainAssetAtPath(row.path))
                        { stats.blocked++; warnings.Add("Non-guide asset occupies " + row.path); continue; }
                        if (guide && guide.contentSignature == row.contentSignature && guide.IsUsable(out _))
                        { stats.unchanged++; continue; }
                        bool created = !guide;
                        if (created)
                        {
                            guide = ScriptableObject.CreateInstance<SparkProAutoGuide>();
                            AssetDatabase.CreateAsset(guide, row.path);
                            stats.created++;
                        }
                        else stats.updated++;
                        Populate(guide, row.level, row.objective, row.draft, row.rule, row.contentSignature);
                        EditorUtility.SetDirty(guide);
                    }
                    foreach (string guid in AssetDatabase.FindAssets("t:SparkProAutoGuide"))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!path.StartsWith(GuideRoot + "/", StringComparison.Ordinal) || expected.Contains(path)) continue;
                        var stale = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(path);
                        if (!stale || !stale.guidedOnly) continue;
                        stale.guidedOnly = false; // Never silently leave withdrawn objectives active.
                        EditorUtility.SetDirty(stale);
                        stats.invalidated++;
                        warnings.Add("Disabled withdrawn auto guide: " + path);
                    }
                }
                finally { AssetDatabase.StopAssetEditing(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            }
            catch (Exception ex)
            {
                stats.blocked++;
                warnings.Add("Unexpected error: " + ex);
                Debug.LogException(ex);
            }
            finally { building = false; }
            stats.warnings = warnings.ToArray();
            stats.guidedAvailable = ValidateCount();
            WriteSummary(stats, reveal);
            Debug.Log($"[Spark Auto Teacher] Built {stats.guidedAvailable}/{stats.objectives} UNGRADED guides: " +
                      $"created {stats.created}, updated {stats.updated}, unchanged {stats.unchanged}, blocked {stats.blocked}. " +
                      "No Academy scores or mission achievements were awarded.");
        }

        public static string GuidePath(int level, string objectiveId)
        {
            // Hash + sanitized IDs prevent cross-level collisions and unsafe filenames.
            string token = Regex.Replace(objectiveId ?? "", @"[^a-zA-Z0-9_-]", "_");
            token = token.Substring(0, Math.Min(45, token.Length));
            return GuideRoot + "/Auto_L" + level.ToString("D3") + "_" + token + "_" +
                   Signature(objectiveId ?? "").Substring(0, 8) + ".asset";
        }

        private static void Populate(SparkProAutoGuide guide, SparkMasterTeacherImporter.Level level,
            SparkMasterTeacherImporter.Objective objective, SparkProLessonDefinition draft, Rule rule,
            string contentSignature)
        {
            string title = string.IsNullOrWhiteSpace(objective.title) ? objective.id : objective.title.Trim();
            string requirement = Join(objective.explanation);
            guide.lessonId = SparkMasterTeacherImporter.StableLessonId(level.level, objective.id);
            guide.level = level.level; guide.objectiveId = objective.id; guide.title = "Level " + level.level + " — " + title;
            guide.sourceAsset = level.source; guide.sourceFingerprint = draft.sourceFingerprint;
            guide.contentSignature = contentSignature;
            guide.authoredRequirement = requirement;
            guide.aliases = new[] { title, objective.id.Replace('_', ' ') };
            guide.authoredVisualReferences = objective.visual ?? Array.Empty<string>();
            guide.authoredPracticeReferences = objective.practice ?? Array.Empty<string>();
            guide.recipeId = rule.id; guide.guidedOnly = true;
            guide.concept = "Authored objective: " + title + ".\nAuthored requirement: " + requirement +
                "\nGeneral teaching principle: " + rule.principle +
                "\nOnly fresh, authoritative game evidence can establish that this objective is complete.";
            guide.analogy = rule.analogy;
            guide.diagram = "[Authored setup] -> [Check prerequisites] -> [Observe the game evidence] -> [Verify objective]";
            guide.misconception = rule.caveat;
            guide.baseline = "Before working on ‘" + title + "’, which component, reference or condition mentioned " +
                "in the authored requirement should you inspect first? Explain your reasoning.";
            guide.prediction = "For ‘" + title + "’, what result or observation would you expect if the required " +
                "setup is correct? Distinguish a predicted result from a measured result.";
            guide.practice = "In Level " + level.level + ", perform the objective ‘" + title + "’ in the actual " +
                "game laboratory. Compare fresh evidence against: " + requirement +
                "\nThe mission evaluator decides completion; this tutorial does not grade your answer.";
        }

        public static string RecipeFor(string title, string excerpt) => Categorize(title, excerpt).id;
        private static Rule Categorize(string title, string excerpt)
        {
            // Objective title is more specific than the long source excerpt; use it first.
            foreach (Rule r in Rules)
                if (r.id != "generic" && Regex.IsMatch(title ?? "", r.pattern,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) return r;
            foreach (Rule r in Rules)
                if (r.id != "generic" && Regex.IsMatch(excerpt ?? "", r.pattern,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) return r;
            return Rules[Rules.Length - 1];
        }
        private static string Join(string[] strings) => strings == null ? "" :
            string.Join("\n", strings.Where(x => !string.IsNullOrWhiteSpace(x)));
        private static string Signature(string raw)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw ?? ""))).Replace("-", "");
        }
        private static void EnsureFolder(string path)
        {
            string cursor = "Assets";
            foreach (string part in path.Substring("Assets/".Length).Split('/'))
            {
                string child = cursor + "/" + part;
                if (!AssetDatabase.IsValidFolder(child)) AssetDatabase.CreateFolder(cursor, part);
                cursor = child;
            }
        }
        private static int ValidateCount()
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:SparkProAutoGuide"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith(GuideRoot + "/", StringComparison.Ordinal)) continue;
                var asset = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(path);
                if (asset && asset.IsUsable(out _)) count++;
            }
            return count;
        }
        private static void WriteSummary(BuildSummary summary, bool reveal)
        {
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                SparkMasterTeacherImporter.ReportRoot);
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "AutoBuildSummary.json");
            File.WriteAllText(path, JsonUtility.ToJson(summary, true), Encoding.UTF8);
            if (reveal) EditorUtility.RevealInFinder(path);
        }
        public static void ValidateAndReport(bool reveal)
        {
            if (!SparkMasterTeacherImporter.TryReadCards(out var cards, out string error))
            { Debug.LogError("[Spark Auto Teacher] " + error); return; }
            var csv = new StringBuilder("Level,ObjectiveId,Title,AutoGuideAvailable,SourceCurrent,Recipe,ReviewedLesson,Status\n");
            var html = new StringBuilder();
            int available = 0, missing = 0, stale = 0, reviewed = 0;
            foreach (var level in cards.levels)
            foreach (var objective in level.objectives)
            {
                string id = SparkMasterTeacherImporter.StableLessonId(level.level, objective.id);
                var guide = AssetDatabase.LoadAssetAtPath<SparkProAutoGuide>(GuidePath(level.level, objective.id));
                var draft = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(
                    SparkMasterTeacherImporter.DraftPath(level.level, objective.id));
                var reviewedLesson = AssetDatabase.LoadAssetAtPath<SparkProLessonDefinition>(
                    SparkMasterTeacherImporter.PublishedPath(id));
                bool ready = guide && guide.IsUsable(out _);
                bool current = ready && draft && guide.sourceFingerprint == draft.sourceFingerprint;
                bool published = reviewedLesson && reviewedLesson.IsReady(out _) &&
                    !string.IsNullOrWhiteSpace(reviewedLesson.reviewedBy) &&
                    !string.IsNullOrWhiteSpace(reviewedLesson.reviewedUtc) &&
                    reviewedLesson.sourceFingerprint == (draft ? draft.sourceFingerprint : "");
                if (current) available++; else if (!ready) missing++; else stale++;
                if (published) reviewed++;
                string status = published ? "REVIEWED LESSON" : current ? "AUTO-GUIDED / UNGRADED" :
                    ready ? "STALE SOURCE" : "NO AUTO GUIDE";
                csv.AppendLine(Csv(level.level.ToString()) + "," + Csv(objective.id) + "," +
                    Csv(objective.title) + "," + (ready ? "1" : "0") + "," + (current ? "1" : "0") + "," +
                    Csv(guide ? guide.recipeId : "") + "," + (published ? "1" : "0") + "," + Csv(status));
                html.Append("<tr><td>").Append(level.level).Append("</td><td>").Append(Html(objective.id))
                    .Append("</td><td>").Append(Html(objective.title)).Append("</td><td>")
                    .Append(Html(status)).Append("</td></tr>");
            }
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                SparkMasterTeacherImporter.ReportRoot);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "AutoCoverage.csv"), csv.ToString(), Encoding.UTF8);
            string summary = "SPARK AUTO TEACHER - SOURCE GROUNDED GUIDANCE\n" +
                "Levels: " + cards.levels.Length + "\nObjectives: " +
                cards.levels.Sum(l => l.objectives.Length) + "\nSource-current generated guides: " + available +
                "\nMissing guides: " + missing + "\nStale guides: " + stale +
                "\nHuman-reviewed objective lessons: " + reviewed +
                "\nAutomatic graded assessments: 0\nAutomatic mission completions: 0\n" +
                "Auto-guides provide grounded orientation and ungraded practice. " +
                "They are NOT verified expert lessons, tests or certification.\n";
            File.WriteAllText(Path.Combine(dir, "AutoValidation.txt"), summary, Encoding.UTF8);
            File.WriteAllText(Path.Combine(dir, "AutoCoverage.html"),
                "<!doctype html><html><head><meta charset='utf-8'><title>Spark Auto Teacher</title>" +
                "<style>body{background:#101820;color:#e9f3f5;font:14px Segoe UI;margin:35px}" +
                "td,th{padding:8px;border-bottom:1px solid #405164;text-align:left}</style></head><body>" +
                "<h1>Spark Auto Teacher</h1><pre>" + Html(summary) + "</pre>" +
                "<table><thead><tr><th>Level</th><th>Objective</th><th>Title</th><th>Status</th>" +
                "</tr></thead><tbody>" + html + "</tbody></table></body></html>", Encoding.UTF8);
            Debug.Log("[Spark Auto Teacher] " + summary + "Reports: " + dir);
            if (reveal) EditorUtility.RevealInFinder(Path.Combine(dir, "AutoCoverage.html"));
        }
        private static string Csv(string raw) => "\"" + (raw ?? "").Replace("\"", "\"\"") + "\"";
        private static string Html(string raw) => (raw ?? "").Replace("&", "&amp;").Replace("<", "&lt;")
            .Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
#endif
