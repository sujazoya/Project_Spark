using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal enum SparkIntent
    {
        Legacy, Diagnose, Inspect, Measure, OtherTerminal, Objectives, Campaign, Search, Equipment,
        Progress, LevelDetails, Predict, ExplainAgain, Show, Lesson, Pause, Resume, Observe, Answer, Cancel,
        Qualification, Revision, DemoAction
    }

    internal enum SparkCortexTool
    {
        Mission, Objectives, Campaign, Search, Inspect, Topology, Measurement, Diagnose, Equipment,
        Progress, Equation, Illustration, Point, Practice, Qualification, Demo
    }

    internal enum SparkClaim { Runtime, Authored, Curated, Calculated, Hypothesis, Unavailable }

    internal sealed class SparkCortex
    {
        internal readonly struct Evidence
        {
            public readonly SparkCortexTool Tool;
            public readonly SparkClaim Claim;
            public readonly string Source, Entity, Data, Limit, Session;
            public readonly DateTime CapturedUtc;
            public readonly int Request, Topology, Solve;
            public bool Success => Claim != SparkClaim.Unavailable;

            public Evidence(SparkCortexTool tool, SparkClaim claim, string source, string entity,
                string data, string limit, Context context)
            {
                Tool = tool; Claim = claim; Source = source; Entity = entity; Data = data; Limit = limit;
                Session = context.Session; Request = context.Request; Topology = context.Topology;
                Solve = context.Solve; CapturedUtc = DateTime.UtcNow;
            }
        }

        internal sealed class Context
        {
            public SparkAILevelContext Level;
            public SparkAIWorldSnapshot World;
            public SparkProGameCatalog Catalog;
            public SparkElectronicObject Object;
            public SparkTerminal Pin;
            public string Question, Session, UploadFreeTopic;
            public int Request, Topology, Solve;
            public bool Restricted;
            public SparkProAcademy Academy;
            public Func<string, (string text, bool opened)> Draw;
            public Func<Transform, bool> Point;
            public Func<string, string> Demonstrate;
            public Func<string, (string text, bool opened)> TeachingModel;

            public bool HasFreshSolve
            {
                get
                {
                    if (!Level.HasEvaluation || !Level.Manager.ElectricalSolver || !Level.Manager.CircuitSystem) return false;
                    var solver = Level.Manager.ElectricalSolver;
                    return !solver.IsDirty && !solver.IsSolving && solver.LatestSolveResult != null &&
                        solver.CompletedSolveRevision == Solve && Level.Manager.CircuitSystem.TopologyVersion == Topology &&
                        SparkAILevelContext.Capture(Level.Manager).Layout == Level.Layout;
                }
            }
        }

        internal readonly struct Reply
        {
            public readonly string Text;
            public readonly SparkState State;
            public readonly SparkGesture Gesture;
            public Reply(string text, SparkState state, SparkGesture gesture = SparkGesture.None)
            { Text = text; State = state; Gesture = gesture; }
        }

        const int ToolBudget = 5;
        private readonly Dictionary<SparkCortexTool, Func<Context, Evidence>> tools;
        private readonly List<Evidence> trace = new(ToolBudget);
        private readonly SparkProTeacher teacher = new();
        private SparkElectronicObject discussed;
        private SparkTerminal discussedPin;
        private SparkIntent previous;
        private string lastTopic;
        private Guid session = Guid.NewGuid();
        private int revision;
        public SparkIntent Intent { get; private set; }
        public int Revision => revision;
        public string Session => session.ToString("N");
        public IReadOnlyList<Evidence> Trace => trace;
        public string ActiveLesson => teacher.Active;
        public double LatencyMs { get; private set; }
        public int ToolCount => tools.Count;

        public SparkCortex()
        {
            tools = new()
            {
                [SparkCortexTool.Mission] = LevelDetails,
                [SparkCortexTool.Objectives] = Objectives,
                [SparkCortexTool.Campaign] = Campaign,
                [SparkCortexTool.Search] = Search,
                [SparkCortexTool.Inspect] = Inspect,
                [SparkCortexTool.Topology] = Topology,
                [SparkCortexTool.Measurement] = Measure,
                [SparkCortexTool.Diagnose] = Diagnose,
                [SparkCortexTool.Equipment] = Equipment,
                [SparkCortexTool.Progress] = Progress,
                [SparkCortexTool.Equation] = Equation,
                [SparkCortexTool.Illustration] = Illustration,
                [SparkCortexTool.Point] = Point,
                [SparkCortexTool.Practice] = c => E(c, SparkCortexTool.Practice, SparkClaim.Curated, "Academy",
                    c.Academy ? c.Academy.StartPractice(c.Academy.SelectTopic(c.Question)) : "Academy is unavailable."),
                [SparkCortexTool.Qualification] = Qualification,
                [SparkCortexTool.Demo] = Demo
            };
        }

        public int Begin(Context context)
        {
            trace.Clear();
            context.Session = Session;
            context.Request = ++revision;
            context.Topology = context.Level.Manager && context.Level.Manager.CircuitSystem ? context.Level.Manager.CircuitSystem.TopologyVersion : -1;
            context.Solve = context.Level.Manager && context.Level.Manager.ElectricalSolver ? context.Level.Manager.ElectricalSolver.CompletedSolveRevision : -1;
            return revision;
        }

        public void Cancel() { revision++; trace.Clear(); }
        public void Reset(bool clearLesson)
        {
            Cancel();
            session = Guid.NewGuid();
            previous = Intent = SparkIntent.Legacy;
            discussed = null; discussedPin = null; lastTopic = null;
            if (clearLesson) teacher.Cancel();
        }

        internal static SparkIntent Classify(string value)
        {
            string q = Normalize(value);
            if (Regex.IsMatch(q, @"\b(certification|certificate|motherboard master|qualified to graduate)\b")) return SparkIntent.Qualification;
            if (Regex.IsMatch(q, @"\b(pause|hold) (the |our |this )?lesson\b")) return SparkIntent.Pause;
            if (Regex.IsMatch(q, @"\b(continue from where|continue our lesson|resume|pick up where|keep going)\b")) return SparkIntent.Resume;
            if (Regex.IsMatch(q, @"\b(cancel (the |our )?(lesson|demo|demonstration)|end (the |our )?lesson)\b")) return SparkIntent.Cancel;
            if (Regex.IsMatch(q, @"\b(another way|different way|don't understand|do not understand|say it differently|give me an analogy)\b")) return SparkIntent.ExplainAgain;
            if (Regex.IsMatch(q, @"\b(other|second|opposite|next) (pin|terminal|leg|contact)\b")) return SparkIntent.OtherTerminal;
            if (Regex.IsMatch(q, @"\b(what should i (connect|do) next|what'?s left|incomplete objectives|remaining objectives|what am i missing)\b")) return SparkIntent.Objectives;
            if (Regex.IsMatch(q, @"\b(why|diagnose|troubleshoot|fault find)\b") &&
                Regex.IsMatch(q, @"\b(fail\w*|work\w*|short\w*|open|voltage|led|overload\w*|rail|fault|wrong|dark)\b")) return SparkIntent.Diagnose;
            if (Regex.IsMatch(q, @"\b(is (this|my|the) (voltage|current) (correct|right)|what (is|are) (the |my |this )?(voltage|current|resistance|power)\b|read (the |my )?(voltage|current|resistance|power)|show the reading)")) return SparkIntent.Measure;
            if (Regex.IsMatch(q, @"\b(what (is|are) (this|that|these)( component| terminal)?|identify (this|that)|inspect (this|that|the selected))$")) return SparkIntent.Inspect;
            if (Regex.IsMatch(q, @"\b(what happens if|what if i|suppose i)\b")) return SparkIntent.Predict;
            if (Regex.IsMatch(q, @"\b(show me what you mean|point it out|show the (actual )?(diagram|schematic)|walk me through the diagram)\b")) return SparkIntent.Show;
            if (Regex.IsMatch(q, @"\b(where (can|do) i (learn|practice)|which (level|mission).* (cover|teach|involve)|levels (about|on|with)|find (a )?level (about|with))\b")) return SparkIntent.Search;
            // SPARK_TEACHING_RUNTIME_V3_ROUTING: lesson requests precede general level descriptions.
            if (Regex.IsMatch(q, @"\b(teach me|start (a |the )?(lesson|tutorial)|walk me through|guide me through|show me step by step|step by step|ask me (a |one )?question|quiz me|can you teach)\b"))
                return SparkIntent.Lesson;
            if (Regex.IsMatch(q, @"\b(continue (the|this|our) lesson|go to (the )?next (lesson )?step|next question)\b"))
                return SparkIntent.Resume;
            if (Regex.IsMatch(q, @"\b(level|mission)\s+(\d+|one|two|three|four|five|six|seven|eight|nine|ten|twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety)\b") &&
                Regex.IsMatch(q, @"\b(explain|teach|about|unlock|prerequisite|requirement|objective|complete)\w*\b")) return SparkIntent.LevelDetails;
            if (Regex.IsMatch(q, @"\b(which chapter|current chapter|current campaign|campaign size)\b")) return SparkIntent.Campaign;
            if (Regex.IsMatch(q, @"\b(available equipment|what (equipment|instruments) (can|do) i)\b")) return SparkIntent.Equipment;
            if (Regex.IsMatch(q, @"\b(my progress|have i (completed|passed)|which levels have i)\b")) return SparkIntent.Progress;
            if (Regex.IsMatch(q, @"\b(what should i review|revision|mastery)\b")) return SparkIntent.Revision;
            if (Regex.IsMatch(q, @"^(answer|i predict|my prediction is|i think)\b")) return SparkIntent.Answer;
            if (Regex.IsMatch(q, @"\b(run (the |an )?experiment|observe (the |my )?(result|reading)|perform (the |a )?test)\b")) return SparkIntent.Observe;
            if (Regex.IsMatch(q, @"\b(start (a |the )?(lesson|tutorial)|teach me|walk me through how|can you teach)\b")) return SparkIntent.Lesson;
            if (Regex.IsMatch(q, @"\b(demonstrate|run a demo|place .* for me|wire .* for me|connect .* for me|turn .* on for me)\b")) return SparkIntent.DemoAction;
            return SparkIntent.Legacy;
        }

        public bool TryAnswer(Context context, out Reply reply, SparkIntent? permittedIntent = null)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            try { return Route(context, out reply, permittedIntent); }
            finally { LatencyMs = clock.Elapsed.TotalMilliseconds; }
        }

        private bool Route(Context context, out Reply reply, SparkIntent? permittedIntent)
        {
            reply = default;
            Intent = permittedIntent ?? Classify(context.Question);
            if (context.Restricted)
            {
                reply = new Reply("Independent examination is active. Submit your work using the official examiner.", SparkState.Examining);
                return true;
            }
            // SPARK_PRACTICAL_COACH_V4: narrow, evidence-grounded practical requests.
            // Runs before legacy fallbacks, without interrupting lesson answers or resume.
            if (SparkProPracticalCoach.TryRespond(context, teacher.Active, out Reply practicalReply))
            {
                reply = practicalReply;
                return true;
            }
            if (Intent == SparkIntent.Legacy)
            {
                string q = Normalize(context.Question);
                if (q == "why" || q == "why is that" || q == "what about now")
                    Intent = previous is SparkIntent.Measure or SparkIntent.Diagnose or SparkIntent.Objectives ? previous : SparkIntent.Diagnose;
                if (teacher.HasSession && (q is "next" or "continue" or "next step" or "hint")) Intent = SparkIntent.Resume;
                if (teacher.AcceptsResponse && Regex.IsMatch(q, @"^(more|less|same|open|closed|yes|no|[+\-]?\d|zero|one|two|increase|decrease)\b"))
                    Intent = SparkIntent.Answer;
                // SPARK_TEACHING_RUNTIME_V3_RESPONSES: recognize natural short answers during a lesson.
                // Explicit gameplay/diagnostic intents were already classified before this fallback.
                if (Intent == SparkIntent.Legacy && teacher.AcceptsResponse &&
                    q.Length > 0 && q.Length <= 250 &&
                    !Regex.IsMatch(q, @"^(what|why|how|where|when|who|which|explain|show|teach|start|stop|pause|resume|cancel|next|help|tell|can|could|would|should|will|do|does|did|is|are)\b"))
                    Intent = SparkIntent.Answer;

                if (Intent == SparkIntent.Legacy && SparkProCoreConcepts.TryExplain(q, out string primer))
                {
                    reply = new Reply(primer + (teacher.HasSession
                        ? "\nYour lesson is still active. Answer its current question or say 'continue the lesson'."
                        : string.Empty), SparkState.Teaching, SparkGesture.Explain);
                    return true;
                }
                if (Intent == SparkIntent.Legacy) return false;
            }

            string ambiguity = Resolve(context);
            if (ambiguity != null && Intent is SparkIntent.Inspect or SparkIntent.OtherTerminal or SparkIntent.Measure or SparkIntent.Diagnose or SparkIntent.Show)
            {
                reply = new Reply(ambiguity, SparkState.Observing);
                return true;
            }

            previous = Intent;
            string answer;
            SparkState state = SparkState.Observing;
            SparkGesture gesture = SparkGesture.None;
            switch (Intent)
            {
                case SparkIntent.Diagnose:
                    var component = Run(SparkCortexTool.Inspect, context);
                    var topology = Run(SparkCortexTool.Topology, context);
                    var reading = Run(SparkCortexTool.Measurement, context);
                    var diagnosis = Run(SparkCortexTool.Diagnose, context);
                    answer = string.Join("\n", new[] { component, topology, reading }.Where(item => item.Success).Select(item => item.Data)
                        .Append(diagnosis.Data));
                    state = SparkState.Diagnosing; gesture = SparkGesture.Inspect;
                    break;
                case SparkIntent.OtherTerminal:
                    answer = OtherTerminal(context);
                    state = SparkState.Teaching;
                    break;
                case SparkIntent.Show:
                    context.UploadFreeTopic = teacher.VisualModel;
                    bool requestedDrawing = context.Question.Contains("diagram", StringComparison.OrdinalIgnoreCase) ||
                        context.Question.Contains("schematic", StringComparison.OrdinalIgnoreCase);
                    var result = Run(!requestedDrawing && string.IsNullOrEmpty(context.UploadFreeTopic) && (context.Pin || context.Object)
                        ? SparkCortexTool.Point : SparkCortexTool.Illustration, context);
                    answer = result.Data;
                    state = SparkState.Teaching;
                    break;
                case SparkIntent.Lesson:
                case SparkIntent.Resume:
                case SparkIntent.Pause:
                case SparkIntent.Answer:
                case SparkIntent.ExplainAgain:
                case SparkIntent.Observe:
                case SparkIntent.Cancel:
                    if (!teacher.TryRespond(Intent, context, () => Run(SparkCortexTool.Demo, context).Data, out answer, out gesture))
                    {
                        if (Intent == SparkIntent.ExplainAgain && !string.IsNullOrEmpty(lastTopic))
                        { answer = "Think of the relationship one variable at a time. " + EquationText(lastTopic); gesture = SparkGesture.Explain; }
                        else return false;
                    }
                    state = SparkState.Teaching;
                    break;
                case SparkIntent.Revision:
                    answer = context.Academy ? context.Academy.RevisionReport() : "Academy is unavailable.";
                    state = SparkState.Teaching; gesture = SparkGesture.Offer;
                    break;
                default:
                    SparkCortexTool tool = Intent switch
                    {
                        SparkIntent.Inspect => SparkCortexTool.Inspect, SparkIntent.Measure => SparkCortexTool.Measurement,
                        SparkIntent.LevelDetails => SparkCortexTool.Mission,
                        SparkIntent.Predict => SparkCortexTool.Equation, SparkIntent.Search => SparkCortexTool.Search,
                        SparkIntent.Campaign => SparkCortexTool.Campaign, SparkIntent.Objectives => SparkCortexTool.Objectives,
                        SparkIntent.Equipment => SparkCortexTool.Equipment, SparkIntent.Progress => SparkCortexTool.Progress,
                        SparkIntent.Qualification => SparkCortexTool.Qualification, _ => SparkCortexTool.Demo
                    };
                    answer = Run(tool, context).Data;
                    if (tool == SparkCortexTool.Equation) { lastTopic = context.Question; gesture = SparkGesture.Count; state = SparkState.Teaching; }
                    if (tool == SparkCortexTool.Measurement) gesture = SparkGesture.Inspect;
                    break;
            }
            reply = new Reply(answer, state, gesture);
            return true;
        }

        internal Evidence Run(SparkCortexTool tool, Context context)
        {
            if (context.Request != revision || context.Session != Session || context.Restricted ||
                trace.Count >= ToolBudget || trace.Any(item => item.Tool == tool) || !tools.TryGetValue(tool, out var call))
                return E(context, tool, SparkClaim.Unavailable, "Cortex permissions", "The request is not available in this turn.", "Denied");
            var result = call(context);
            trace.Add(result);
            return result;
        }

        private static Evidence E(Context c, SparkCortexTool tool, SparkClaim claim, string source, string data, string limit = "") =>
            new(tool, claim, source, c.Pin ? c.Pin.GetInstanceID().ToString(CultureInfo.InvariantCulture) :
                c.Object ? c.Object.GetInstanceID().ToString(CultureInfo.InvariantCulture) : c.Level.Definition ? c.Level.Definition.LevelId : "",
                data, limit, c);

        private string Resolve(Context context)
        {
            if (!context.Level.Layout) return null;
            var q = Normalize(context.Question);
            if (context.Object && !context.Object.transform.IsChildOf(context.Level.Layout.transform))
            {
                context.Object = null; context.Pin = null;
            }
            var objects = context.Level.Layout;
            var match = Regex.Match(q, @"\b(led|resistor|switch|capacitor|diode|power supply)\b");
            if (match.Success && Intent is SparkIntent.Inspect or SparkIntent.Diagnose or SparkIntent.Measure)
            {
                string term = match.Value;
                bool Fits(SparkElectronicObject item) => item && (item.name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    item.GetType().Name.Contains(term.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                if (!Fits(context.Object))
                {
                    var matches = objects.GetComponentsInChildren<SparkElectronicObject>().Where(Fits).Take(6).ToArray();
                    var explicitMatch = matches.Where(item => item.name.Length > term.Length &&
                        q.Contains(item.name.ToLowerInvariant())).ToArray();
                    if (explicitMatch.Length == 1) context.Object = explicitMatch[0];
                    else if (matches.Length > 1)
                        return $"Which {term} do you mean? Select it on the workbench. Matches include {string.Join(", ", matches.Take(4).Select(item => item.name))}.";
                    else if (matches.Length == 1) context.Object = matches[0];
                    else return $"I could not identify a {term} in the current layout. Select the intended part.";
                    context.Pin = null;
                }
            }
            if (!context.Object && discussed && discussed.transform.IsChildOf(objects.transform)) context.Object = discussed;
            if (!context.Pin && context.Object == discussed && discussedPin) context.Pin = discussedPin;
            if (context.Object != discussed) discussedPin = null;
            if (context.Object) discussed = context.Object;
            if (context.Pin) discussedPin = context.Pin;
            return null;
        }

        private string OtherTerminal(Context context)
        {
            if (!context.Object || !context.Pin) return "Select one terminal first so I can resolve what “other” refers to.";
            var pins = context.Object.GetComponentsInChildren<SparkTerminal>().Where(pin => pin != context.Pin).Take(2).ToArray();
            if (pins.Length != 1) return "There is no unique opposite terminal. Select the terminal by its authored pin label.";
            discussedPin = context.Pin = pins[0];
            return Run(SparkCortexTool.Point, context).Data;
        }

        private static Evidence LevelDetails(Context c)
        {
            var id = Regex.Match(Normalize(c.Question), @"\b(?:level|mission)\s+(\d+|[a-z]+(?:[- ][a-z]+){0,4})");
            if (!id.Success || !c.Catalog)
                return E(c, SparkCortexTool.Mission, SparkClaim.Unavailable, "Authored catalog", "Name the level by its authored number.");
            string phrase = id.Groups[1].Value;
            int number = 0;
            string[] words = phrase.Split(' ');
            for (int length = words.Length; length > 0; length--)
                if (SparkProAcademy.TryNumber(string.Join(" ", words.Take(length)), out double value) && value >= 1 && value <= 130 && value == Math.Truncate(value))
                { number = (int)value; break; }
            if (!c.Catalog.TryLevel(number, out var record))
                return E(c, SparkCortexTool.Mission, SparkClaim.Unavailable, "Authored catalog", "I could not resolve that campaign level. Try its number.");
            string access = record.chapterOpen ? "Chapter available by default." :
                $"Chapter access rule: {Regex.Replace(record.accessRule ?? "", "(?<!^)([A-Z])", " $1")}.";
            string order = record.previousLevel > 0 ? $" The previous authored mission is Level {record.previousLevel}." : "";
            string live = c.Level.Definition && c.Level.Definition.LevelNumber == number ? "\n" + c.Level.StateText() : "";
            return E(c, SparkCortexTool.Mission, SparkClaim.Authored, record.sourcePath,
                $"Level {number} — {record.title}. Chapter {record.chapterNumber}, {record.chapter}.{live}\n" +
                $"{record.description}\n{access}{order} {record.conditions}\n" + string.Join("\n", record.objectives));
        }

        private static Evidence Objectives(Context c)
        {
            var target = c.Level.NextTarget();
            if (!c.Level.Definition) return E(c, SparkCortexTool.Objectives, SparkClaim.Unavailable, "Level loader", c.Level.StateText());
            if (!c.Level.HasEvaluation) return E(c, SparkCortexTool.Objectives, SparkClaim.Unavailable, "Level evaluator", c.Level.StateText());
            string data = target != null ? $"Next incomplete objective: {SparkAILevelContext.TargetName(target)}. {target.Description}" : c.Level.StateText();
            return E(c, SparkCortexTool.Objectives, SparkClaim.Runtime, "Level evaluator and authored objective", data);
        }

        private static Evidence Campaign(Context c)
        {
            var campaign = c.Level.Manager ? c.Level.Manager.Campaign : null;
            if (!campaign) return E(c, SparkCortexTool.Campaign, SparkClaim.Unavailable, "Campaign", "No campaign is available.");
            var chapter = campaign.FindChapter(c.Level.Definition);
            return E(c, SparkCortexTool.Campaign, SparkClaim.Authored, "Campaign and chapter assets",
                chapter ? $"{campaign.DisplayName}, Chapter {chapter.ChapterNumber} — {chapter.DisplayName}. {chapter.Description}" :
                    $"{campaign.DisplayName} has {c.Level.Manager.GetCampaignLevelIndices().Count()} authored campaign levels. Open a level to identify its chapter.");
        }

        private static Evidence Search(Context c)
        {
            var matches = c.Catalog ? c.Catalog.Search(c.Question) : Array.Empty<SparkProGameCatalog.Record>();
            return E(c, SparkCortexTool.Search, matches.Count > 0 ? SparkClaim.Authored : SparkClaim.Unavailable, "Campaign knowledge catalog",
                matches.Count == 0 ? "I found no relevant authored mission. Try a board name, component or concept." :
                string.Join("\n", matches.Select(r => $"Level {r.levelNumber} — {r.title}. {r.description}")));
        }

        private static Evidence Inspect(Context c)
        {
            if (c.Pin) return E(c, SparkCortexTool.Inspect, SparkClaim.Authored, "Selected terminal",
                $"{c.Pin.name} is marked {c.Pin.Polarity.ToString().ToLowerInvariant()} in the current layout.");
            if (!c.Object) return E(c, SparkCortexTool.Inspect, SparkClaim.Unavailable, "Current selection", "Select the component you mean.");
            var pins = c.Object.GetComponentsInChildren<SparkTerminal>();
            string details = c.Object is SparkResistor resistor ? $" Configured resistance: {resistor.Resistance:G4} Ω." : "";
            return E(c, SparkCortexTool.Inspect, SparkClaim.Runtime, "Current component configuration",
                $"{c.Object.name} ({c.Object.GetType().Name.Replace("Spark", "")}).{details}" +
                (pins.Length == 0 ? "" : $" Terminals: {string.Join(", ", pins.Take(6).Select(pin => pin.name))}."));
        }

        private static Evidence Topology(Context c)
        {
            if (!c.Object || c.World == null || !c.World.CircuitAvailable || c.World.TopologyVersion != c.Topology)
                return E(c, SparkCortexTool.Topology, SparkClaim.Unavailable, "Circuit graph", "Fresh component connectivity is unavailable.");
            var system = c.Level.Manager.CircuitSystem;
            var pins = c.Object.GetComponentsInChildren<SparkTerminal>();
            if (pins.Length == 0) return E(c, SparkCortexTool.Topology, SparkClaim.Unavailable, "Circuit graph", "The component has no exposed terminals.");
            return E(c, SparkCortexTool.Topology, SparkClaim.Runtime, "Circuit graph",
                "Direct links per terminal: " + string.Join(", ", pins.Take(6).Select(pin => $"{pin.name} {system.GetConnectionCount(pin)}")) + ".");
        }

        internal static string RequestedQuantity(string question)
        {
            var match = Regex.Match(Normalize(question),
                @"\b(?:what (?:is|are)|read|show|is)\s+(?:(?:the|my|this)\s+)?(?:current\s+(?=voltage|power|resistance))?(voltage|current|power|resistance)\b");
            return match.Success ? match.Groups[1].Value : "voltage";
        }

        private static Evidence Measure(Context c)
        {
            if (!c.HasFreshSolve || c.Object is not SparkElectricalComponent part ||
                !c.Level.Manager.ElectricalSolver.LatestSolveResult.Context.Graph.TryGetComponentTerminals(part, out var pins) || pins.Length != 2)
                return E(c, SparkCortexTool.Measurement, SparkClaim.Unavailable, "Electrical solver", "Select a solved two-terminal component for a current reading, or use the meter with an explicit probe pair.");
            string quantity = RequestedQuantity(c.Question);
            if (quantity == "resistance")
            {
                if (part is SparkResistor resistor)
                    return E(c, SparkCortexTool.Measurement, SparkClaim.Runtime, "Resistor configuration",
                        $"{part.name}: configured {resistor.Resistance:G4} Ω, effective {resistor.EffectiveResistance:G4} Ω. Disconnect power for a continuity or resistance meter test.");
                return E(c, SparkCortexTool.Measurement, SparkClaim.Unavailable, "Resistor configuration", "This component does not expose a supported resistance value.");
            }
            if (quantity == "power")
            {
                var solver = c.Level.Manager.ElectricalSolver;
                if (solver.LatestSolveResult.TryGetVoltage(pins[0], pins[1], out float difference) &&
                    solver.TryCalculateComponentCurrent(part, out float current) && SparkPhase4EvidenceGuard.CanPublishNumber(c, difference * current, "W"))
                    return E(c, SparkCortexTool.Measurement, SparkClaim.Calculated, "Current solver result and P = V × I",
                        $"{part.name}: {Math.Abs(difference * current):G4} W in magnitude, calculated from {difference:G4} V and {current:G4} A.");
                return E(c, SparkCortexTool.Measurement, SparkClaim.Unavailable, "Current solver result", "Power needs verified voltage and current across the same device.");
            }
            if (quantity == "current")
            {
                if (!c.Level.Manager.ElectricalSolver.TryCalculateComponentCurrent(part, out float amps) || !SparkPhase4EvidenceGuard.CanPublishNumber(c, amps, "A"))
                    return E(c, SparkCortexTool.Measurement, SparkClaim.Unavailable, "Electrical solver", "This device has no verified current reading.");
                return E(c, SparkCortexTool.Measurement, SparkClaim.Runtime, "Current solver result", $"{part.name}: {amps * 1000:G4} mA in the solver's authored terminal direction.");
            }
            string value = SparkProDiagnostics.Measure(c.Level, c.Object);
            bool known = value.StartsWith("Current solver result", StringComparison.Ordinal);
            string qualification = c.Question.Contains("correct", StringComparison.OrdinalIgnoreCase) && known ?
                " " + SparkProDiagnostics.Explain(c.Level, c.Object, c.Question, true) : "";
            return E(c, SparkCortexTool.Measurement, known ? SparkClaim.Runtime : SparkClaim.Unavailable,
                "Current solver result", value + qualification);
        }

        private static Evidence Diagnose(Context c) =>
            E(c, SparkCortexTool.Diagnose, c.HasFreshSolve ? SparkClaim.Hypothesis : SparkClaim.Unavailable,
                "Level evaluator and live electrical evidence", SparkProDiagnostics.Explain(c.Level, c.Object, c.Question, false),
                "Possible causes remain hypotheses.");

        private static Evidence Equipment(Context c)
        {
            if (!c.Level.Layout) return E(c, SparkCortexTool.Equipment, SparkClaim.Unavailable, "Layout loader", "Open a workbench first.");
            var instruments = c.Level.Layout.GetComponentsInChildren<SparkInstrument>().Take(12).Select(item => item.name).ToArray();
            return E(c, SparkCortexTool.Equipment, SparkClaim.Runtime, "Loaded layout", instruments.Length == 0 ?
                "No instrument is registered under this layout. Check the workshop tool tray for its available tools." :
                $"Instruments in the current layout: {string.Join(", ", instruments)}.");
        }

        private static Evidence Progress(Context c)
        {
            var manager = c.Level.Manager;
            if (!manager || !manager.LevelProgression) return E(c, SparkCortexTool.Progress, SparkClaim.Unavailable, "Progression", "Campaign progress is unavailable.");
            var indices = manager.GetCampaignLevelIndices().ToArray();
            return E(c, SparkCortexTool.Progress, SparkClaim.Runtime, "Campaign progression",
                $"Campaign progression records {indices.Count(manager.LevelProgression.IsCompleted)}/{indices.Length} levels completed. {c.Level.StateText()}");
        }

        private static Evidence Equation(Context c) => E(c, SparkCortexTool.Equation, SparkClaim.Curated, "Ideal circuit theory", EquationText(c.Question));

        internal static string EquationText(string question)
        {
            string q = Normalize(question);
            if (q.Contains("resist"))
                return "For an ideal fixed-voltage circuit, I = V/R. At 6 V, increasing resistance from 300 Ω to 600 Ω reduces current from 20 mA to 10 mA. This example assumes a constant supply voltage.";
            if (q.Contains("voltage"))
                return "For a fixed ideal resistance, I = V/R. Doubling the voltage doubles the current. Component ratings and current limiting matter on a real simulated load.";
            return "Which quantity would you change and which would stay fixed: voltage, resistance, or load? I can work through the appropriate equation.";
        }

        private static Evidence Illustration(Context c)
        {
            var model = c.UploadFreeTopic;
            var illustration = !string.IsNullOrEmpty(model) && !c.Question.Contains("diagram", StringComparison.OrdinalIgnoreCase) &&
                !c.Question.Contains("schematic", StringComparison.OrdinalIgnoreCase) && c.TeachingModel != null
                ? c.TeachingModel(model)
                : c.Draw?.Invoke("draw this level in 2D") ?? ("A circuit illustration is unavailable.", false);
            return E(c, SparkCortexTool.Illustration, illustration.opened ? SparkClaim.Authored : SparkClaim.Unavailable,
                "Existing workbench drawing tool", illustration.text);
        }

        private static Evidence Point(Context c)
        {
            var target = c.Pin ? c.Pin.transform : c.Object ? c.Object.transform : null;
            bool valid = target && c.Level.Layout && target.IsChildOf(c.Level.Layout.transform) && c.Point?.Invoke(target) == true;
            return E(c, SparkCortexTool.Point, valid ? SparkClaim.Runtime : SparkClaim.Unavailable, "Current layout and character",
                valid ? $"Pointing at {target.name} in the current workbench." : "Select a visible component or terminal first.");
        }

        private static Evidence Qualification(Context c) =>
            E(c, SparkCortexTool.Qualification, SparkClaim.Authored, "Level 120 qualification source",
                SparkProQualification.Status(c.Catalog, c.Level));

        private static Evidence Demo(Context c)
        {
            if (c.Demonstrate == null) return E(c, SparkCortexTool.Demo, SparkClaim.Unavailable, "Workshop demonstration",
                "An isolated experiment is unavailable. Use the official tools to perform the experiment.");
            string data = c.Demonstrate(c.UploadFreeTopic ?? c.Question);
            return E(c, SparkCortexTool.Demo, data.StartsWith("Isolated solver", StringComparison.Ordinal) ? SparkClaim.Runtime : SparkClaim.Unavailable,
                "Isolated training experiment", data);
        }

        internal static string Normalize(string text) => (text ?? "").ToLowerInvariant().Replace('’', '\'').Trim().TrimEnd('?', '!', '.');
    }
}
