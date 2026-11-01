using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only end-to-end live electrical integration observer for single-scene levels.
    /// Never calls SolveNow, publishes frames, edits bindings or moves probes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLiveElectricalDiagnostics : MonoBehaviour
    {
        public enum CheckState { Pass, Waiting, Warning, Failure }

        public sealed class Check
        {
            public readonly string name;
            public readonly CheckState state;
            public readonly string detail;
            public Check(string name, CheckState state, string detail)
            {
                this.name = name;
                this.state = state;
                this.detail = detail;
            }
        }

        [Header("Existing systems (observe only)")]
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkLevelLayoutLoader loader;
        [SerializeField] private SparkSolverLiveMeasurementBridge bridge;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkMeterVerifiedNodeFeed nodeFeed;
        [SerializeField] private SparkVirtualMultimeter virtualMeter;

        [Header("Diagnostics settings")]
        [SerializeField, Min(0.5f)] private float staleFrameSeconds = 8f;
        [SerializeField, Min(0.5f)] private float missingNodeFrameGraceSeconds = 1.5f;
        [SerializeField] private bool logErrorsToConsole = false;

        private SparkElectricalSolver observedSolver;
        private SparkLevelLayoutLoader observedLoader;
        private SparkVerifiedElectricalPublisher observedPublisher;
        private SparkMeterVerifiedNodeFeed observedNodeFeed;
        private SparkVerifiedElectricalFrame lastFrame;
        private SparkMeterVerifiedNodeFrame lastNodes;
        private readonly List<string> activity = new List<string>();
        private readonly List<string> issues = new List<string>();
        private float lastFrameAt = -1f;
        private float lastNodeAt = -1f;
        private float nextReferenceCheck;
        private int lastFrameUnityFrame = -1;
        private bool waitingForSolverPublication;
        private int deadlineUnityFrame;
        private int solveEventUnityFrame;
        private string currentLayoutLabel = "(none)";

        public int LoadedLevels { get; private set; }
        public int SuccessfulSolves { get; private set; }
        public int FailedSolves { get; private set; }
        public int ComponentFramesObserved { get; private set; }
        public int NodeFramesObserved { get; private set; }
        public int ViolationsObserved => issues.Count;
        public string ActiveLevelName => currentLayoutLabel;
        public string LatestCircuitId => lastFrame != null ? lastFrame.circuitId : "(none)";
        public int LatestRevision => lastFrame != null ? lastFrame.revision : 0;
        public string LastIssue => issues.Count > 0 ? issues[issues.Count - 1] : "No protocol violation observed";

        private void Awake() => ResolveReferences();

        private void OnEnable()
        {
            ResolveReferences();
            Rebind();
            UpdateActiveLevelName();
        }

        private void Start()
        {
            ResolveReferences();
            Rebind();
        }

        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();

        private void Update()
        {
            if (Time.unscaledTime >= nextReferenceCheck)
            {
                nextReferenceCheck = Time.unscaledTime + 2f;
                ResolveReferences();
                Rebind();
                UpdateActiveLevelName();
            }

            if (waitingForSolverPublication && Time.frameCount >= deadlineUnityFrame)
            {
                waitingForSolverPublication = false;
                // Different event subscription orders are valid. A frame received
                // in the same Unity frame as SolveCompleted is accepted.
                if (lastFrameUnityFrame < solveEventUnityFrame)
                {
                    ReportIssue("SolveCompleted occurred but no verified component " +
                        "frame was observed in that or the following two frames. " +
                        "Check the live bridge's LastIssue and mapping counts.");
                }
            }
        }

        private void ResolveReferences()
        {
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>(FindObjectsInactive.Include);
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>(FindObjectsInactive.Include);
            if (bridge == null) bridge = FindFirstObjectByType<SparkSolverLiveMeasurementBridge>(FindObjectsInactive.Include);
            if (publisher == null) publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (nodeFeed == null) nodeFeed = FindFirstObjectByType<SparkMeterVerifiedNodeFeed>(FindObjectsInactive.Include);
            if (virtualMeter == null) virtualMeter = FindFirstObjectByType<SparkVirtualMultimeter>(FindObjectsInactive.Include);
        }

        private void Rebind()
        {
            if (observedSolver != solver)
            {
                if (observedSolver != null)
                {
                    observedSolver.SolveCompleted -= OnSolveCompleted;
                    observedSolver.SolveFailed -= OnSolveFailed;
                }
                observedSolver = solver;
                if (observedSolver != null)
                {
                    observedSolver.SolveCompleted += OnSolveCompleted;
                    observedSolver.SolveFailed += OnSolveFailed;
                }
            }
            if (observedLoader != loader)
            {
                if (observedLoader != null)
                {
                    observedLoader.LayoutLoaded -= OnLayoutLoaded;
                    observedLoader.LayoutUnloaded -= OnLayoutUnloaded;
                }
                observedLoader = loader;
                if (observedLoader != null)
                {
                    observedLoader.LayoutLoaded += OnLayoutLoaded;
                    observedLoader.LayoutUnloaded += OnLayoutUnloaded;
                }
            }
            if (observedPublisher != publisher)
            {
                if (observedPublisher != null)
                    observedPublisher.FramePublished -= OnComponentFrame;
                observedPublisher = publisher;
                if (observedPublisher != null)
                    observedPublisher.FramePublished += OnComponentFrame;
            }
            if (observedNodeFeed != nodeFeed)
            {
                if (observedNodeFeed != null)
                    observedNodeFeed.FramePublished -= OnNodeFrame;
                observedNodeFeed = nodeFeed;
                if (observedNodeFeed != null)
                    observedNodeFeed.FramePublished += OnNodeFrame;
            }
        }

        private void Unbind()
        {
            if (observedSolver != null)
            {
                observedSolver.SolveCompleted -= OnSolveCompleted;
                observedSolver.SolveFailed -= OnSolveFailed;
            }
            if (observedLoader != null)
            {
                observedLoader.LayoutLoaded -= OnLayoutLoaded;
                observedLoader.LayoutUnloaded -= OnLayoutUnloaded;
            }
            if (observedPublisher != null)
                observedPublisher.FramePublished -= OnComponentFrame;
            if (observedNodeFeed != null)
                observedNodeFeed.FramePublished -= OnNodeFrame;
            observedSolver = null;
            observedLoader = null;
            observedPublisher = null;
            observedNodeFeed = null;
        }

        private void OnLayoutUnloaded()
        {
            currentLayoutLabel = "(unloaded)";
            ClearObservedFrames();
            Log("Layout unloaded: cleared diagnostic snapshots");
        }

        private void OnLayoutLoaded(SparkLevelLayout layout)
        {
            LoadedLevels++;
            currentLayoutLabel = layout != null ? layout.name : "(unknown layout)";
            ClearObservedFrames();
            Log("Loaded level: " + currentLayoutLabel);
        }

        private void ClearObservedFrames()
        {
            lastFrame = null;
            lastNodes = null;
            lastFrameAt = -1f;
            lastNodeAt = -1f;
            lastFrameUnityFrame = -1;
            waitingForSolverPublication = false;
        }

        private void UpdateActiveLevelName()
        {
            if (loader != null && loader.HasActiveLayout && loader.ActiveLayout != null)
                currentLayoutLabel = loader.ActiveLayout.name;
        }

        private void OnSolveCompleted()
        {
            SuccessfulSolves++;
            solveEventUnityFrame = Time.frameCount;
            deadlineUnityFrame = Time.frameCount + 2;
            waitingForSolverPublication = true;
            Log("Solver completed #" + SuccessfulSolves);
        }

        private void OnSolveFailed()
        {
            FailedSolves++;
            waitingForSolverPublication = false;
            Log("Solver failed #" + FailedSolves);
        }

        private void OnComponentFrame(SparkVerifiedElectricalFrame frame)
        {
            ComponentFramesObserved++;
            if (frame == null)
            {
                ReportIssue("Received null component frame");
                return;
            }

            if (lastFrame != null && string.Equals(lastFrame.circuitId, frame.circuitId, StringComparison.Ordinal) &&
                frame.revision <= lastFrame.revision)
                ReportIssue("Component revision did not increase: " + frame.circuitId + " / " + frame.revision);

            if (bridge != null && bridge.HasActiveLevel &&
                !string.Equals(frame.circuitId, bridge.CurrentCircuitId, StringComparison.Ordinal))
                ReportIssue("Frame belongs to unexpected circuit: " + frame.circuitId +
                    " (bridge expects " + bridge.CurrentCircuitId + ")");

            if (loader != null && !loader.HasActiveLayout)
                ReportIssue("Component frame was published while no layout is active");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (frame.revision <= 0 || string.IsNullOrWhiteSpace(frame.circuitId) || frame.readings == null)
                ReportIssue("Component frame contains an invalid header");
            else
            {
                foreach (SparkVerifiedElectricalReading reading in frame.readings)
                {
                    if (reading == null || string.IsNullOrWhiteSpace(reading.anchorId) ||
                        !ids.Add(reading.anchorId) || !Finite(reading.voltageVolts) ||
                        !Finite(reading.currentAmps) || !Finite(reading.resistanceOhms))
                    {
                        ReportIssue("Component frame has a missing/duplicate ID or non-finite measurement");
                        break;
                    }
                }
            }
            lastFrame = frame.Copy();
            lastFrameAt = Time.unscaledTime;
            lastFrameUnityFrame = Time.frameCount;
            Log("Component frame " + frame.circuitId + " / r" + frame.revision +
                " / " + ids.Count + " reading(s)");
        }

        private void OnNodeFrame(SparkMeterVerifiedNodeFrame frame)
        {
            NodeFramesObserved++;
            if (frame == null)
            {
                ReportIssue("Received null node frame");
                return;
            }
            if (lastFrame != null && (!string.Equals(lastFrame.circuitId, frame.circuitId, StringComparison.Ordinal) ||
                lastFrame.revision != frame.revision))
                ReportIssue("Node frame identity/revision does not match latest component frame");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (frame.nodes == null)
                ReportIssue("Node frame has null node collection");
            else
            {
                foreach (SparkMeterVerifiedNode node in frame.nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.terminalId) ||
                        !ids.Add(node.terminalId) || !Finite(node.potentialVolts))
                    {
                        ReportIssue("Node frame has invalid/duplicate terminal ID or non-finite potential");
                        break;
                    }
                }
            }
            lastNodes = frame.Copy();
            lastNodeAt = Time.unscaledTime;
            Log("Node frame " + frame.circuitId + " / r" + frame.revision +
                " / " + ids.Count + " node(s)");
        }

        private void Log(string message)
        {
            if (activity.Count >= 80) activity.RemoveAt(0);
            activity.Add("[" + DateTime.UtcNow.ToString("HH:mm:ss") + " UTC] " + message);
        }

        private void ReportIssue(string message)
        {
            if (issues.Count >= 80) issues.RemoveAt(0);
            issues.Add(message);
            Log("ISSUE: " + message);
            if (logErrorsToConsole) Debug.LogWarning("[Spark Electrical Diagnostics] " + message, this);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        // The two adapters expose their source references as serialized private
        // fields. Reflection below only READS them; it does not modify any system.
        private static T ReadSource<T>(object source, string field) where T : UnityEngine.Object
        {
            if (source == null) return null;
            FieldInfo info = source.GetType().GetField(field,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return info != null ? info.GetValue(source) as T : null;
        }

        private static void AddSourceCheck<T>(List<Check> checks, string label,
            object source, string field, T expected) where T : UnityEngine.Object
        {
            if (source == null || expected == null) return;
            T actual = ReadSource<T>(source, field);
            checks.Add(new Check(label,
                actual == expected ? CheckState.Pass : CheckState.Failure,
                actual == expected ? "References the monitored instance" :
                "Connected to a different or missing instance; open the Inspector and check '" + field + "'"));
        }

        public List<Check> GetChecks()
        {
            var checks = new List<Check>();
            checks.Add(new Check("Solver", solver != null ? CheckState.Pass : CheckState.Failure,
                solver != null ? "SparkElectricalSolver found" : "No SparkElectricalSolver in the shared scene"));
            checks.Add(new Check("Level loader", loader != null ? CheckState.Pass : CheckState.Failure,
                loader != null ? "SparkLevelLayoutLoader found" : "Missing SparkLevelLayoutLoader"));
            checks.Add(new Check("Live adapter", bridge != null ? CheckState.Pass : CheckState.Failure,
                bridge != null ? "Bridge found; following active level=" + bridge.FollowingActiveLevel :
                    "Missing SparkSolverLiveMeasurementBridge"));
            checks.Add(new Check("Publisher", publisher != null ? CheckState.Pass : CheckState.Failure,
                publisher != null ? "Verified component publisher found" : "Missing publisher"));
            checks.Add(new Check("Node feed", nodeFeed != null ? CheckState.Pass : CheckState.Warning,
                nodeFeed != null ? "Verified terminal-potential feed found" :
                    "No node feed; arbitrary two-terminal voltage will be unavailable"));
            checks.Add(new Check("Virtual multimeter", virtualMeter != null ? CheckState.Pass : CheckState.Warning,
                virtualMeter != null ? "Virtual meter found" : "Virtual meter not found in the open scene"));

            AddSourceCheck(checks, "Bridge -> solver", bridge, "solver", solver);
            AddSourceCheck(checks, "Bridge -> loader", bridge, "levelLoader", loader);
            AddSourceCheck(checks, "Bridge -> publisher", bridge, "publisher", publisher);
            AddSourceCheck(checks, "Bridge -> node feed", bridge, "nodeFeed", nodeFeed);
            AddSourceCheck(checks, "Meter -> publisher", virtualMeter, "publisher", publisher);
            AddSourceCheck(checks, "Meter -> node feed", virtualMeter, "nodeFeed", nodeFeed);

            bool active = loader != null && loader.HasActiveLayout && loader.ActiveLayout != null &&
                loader.ActiveBindings != null && loader.ActiveLayout.gameObject.activeInHierarchy;
            checks.Add(new Check("Active layout", active ? CheckState.Pass : CheckState.Waiting,
                active ? loader.ActiveLayout.name + " with own SceneBindings" : "Start a level to test active bindings"));

            bool mapped = active && bridge != null && bridge.CurrentSceneBindings == loader.ActiveBindings;
            checks.Add(new Check("Level bindings", mapped ? CheckState.Pass : CheckState.Waiting,
                mapped ? "Live bridge selected active level's bindings" :
                    "Bridge must follow loader.ActiveBindings (check Follow Active Level and Auto Map)"));

            bool cleanSolve = solver != null && !solver.IsDirty && !solver.IsSolving && solver.LatestSolveResult != null;
            checks.Add(new Check("Completed solve", cleanSolve ? CheckState.Pass : CheckState.Waiting,
                cleanSolve ? "Current solved graph is available" : "Awaiting clean completed solve; no measurement is implied"));

            bool currentFrame = lastFrame != null && active && bridge != null &&
                string.Equals(lastFrame.circuitId, bridge.CurrentCircuitId, StringComparison.Ordinal) &&
                lastFrame.revision > 0;
            bool oldFrame = lastFrame != null && active && bridge != null && !currentFrame;
            checks.Add(new Check("Component publication", currentFrame ? CheckState.Pass :
                oldFrame ? CheckState.Failure : CheckState.Waiting,
                currentFrame ? "Verified frame revision " + lastFrame.revision + " with " + (lastFrame.readings != null ? lastFrame.readings.Count : 0) + " reading(s)" :
                oldFrame ? "Last frame does not belong to active circuit" : "Awaiting first verified frame for active level"));

            if (currentFrame && staleFrameSeconds > 0 && Time.unscaledTime - lastFrameAt > staleFrameSeconds)
                checks.Add(new Check("Frame freshness", CheckState.Warning,
                    "Last frame is " + (Time.unscaledTime - lastFrameAt).ToString("F1") +
                    " seconds old (the meter may label it stale; an unchanged DC circuit need not re-solve)"));
            else
                checks.Add(new Check("Frame freshness", currentFrame ? CheckState.Pass : CheckState.Waiting,
                    currentFrame ? "Within freshness window" : "No current frame to check"));

            if (bridge != null && active && bridge.MappedResistorCount == 0 && bridge.MappedTerminalCount == 0)
                checks.Add(new Check("Level mapping", CheckState.Warning,
                    "Active bindings mapped 0 resistors and 0 terminals"));
            else
                checks.Add(new Check("Level mapping", active && bridge != null ? CheckState.Pass : CheckState.Waiting,
                    bridge != null ? bridge.MappedResistorCount + " resistor(s), " + bridge.MappedTerminalCount +
                        " terminal(s) mapped" : "Not available"));

            if (nodeFeed == null || bridge == null || bridge.MappedTerminalCount == 0)
                checks.Add(new Check("Node synchronization", CheckState.Warning,
                    "No mapped node potentials; probe-to-probe voltage may not be supported for this level"));
            else if (!currentFrame)
                checks.Add(new Check("Node synchronization", CheckState.Waiting, "Awaiting matching completed component frame"));
            else if (lastNodes != null && lastFrame != null &&
                string.Equals(lastNodes.circuitId, lastFrame.circuitId, StringComparison.Ordinal) &&
                lastNodes.revision == lastFrame.revision)
                checks.Add(new Check("Node synchronization", CheckState.Pass,
                    lastNodes.nodes.Count + " terminals synchronized at revision " + lastNodes.revision));
            else
                checks.Add(new Check("Node synchronization",
                    Time.unscaledTime - lastFrameAt < missingNodeFrameGraceSeconds ? CheckState.Waiting : CheckState.Warning,
                    "No node frame matching current circuit/revision; inspect terminal IDs and bridge LastIssue"));

            int markerCount = 0;
            int missingCollider = 0;
            int missingId = 0;
            int unmatchedNodeId = 0;
            var publishedNodeIds = new HashSet<string>(StringComparer.Ordinal);
            if (currentFrame && lastNodes != null && lastFrame != null &&
                lastFrame.revision == lastNodes.revision &&
                string.Equals(lastFrame.circuitId, lastNodes.circuitId, StringComparison.Ordinal) &&
                lastNodes.nodes != null)
            {
                foreach (SparkMeterVerifiedNode node in lastNodes.nodes)
                    if (node != null && !string.IsNullOrWhiteSpace(node.terminalId))
                        publishedNodeIds.Add(node.terminalId);
            }
            if (active)
            {
                SparkMeterProbeTerminal[] markers = loader.ActiveLayout.GetComponentsInChildren<SparkMeterProbeTerminal>(true);
                foreach (SparkMeterProbeTerminal marker in markers)
                {
                    if (marker == null || !marker.gameObject.activeInHierarchy) continue;
                    markerCount++;
                    if (string.IsNullOrWhiteSpace(marker.TerminalId)) missingId++;
                    else if (publishedNodeIds.Count > 0 && !publishedNodeIds.Contains(marker.TerminalId))
                        unmatchedNodeId++;
                    if (marker.GetComponent<Collider>() == null) missingCollider++;
                }
            }
            checks.Add(new Check("Probe markers", !active ? CheckState.Waiting :
                markerCount == 0 || missingCollider > 0 || missingId > 0 || unmatchedNodeId > 0 ? CheckState.Warning : CheckState.Pass,
                !active ? "Start a level to examine probe markers" :
                markerCount + " active marker(s); " + missingId + " missing IDs, " +
                missingCollider + " missing local colliders, " + unmatchedNodeId +
                " not in the matching node frame"));

            if (bridge != null && !string.IsNullOrEmpty(bridge.LastIssue))
                checks.Add(new Check("Bridge issue", CheckState.Warning, bridge.LastIssue));

            return checks;
        }

        public string GetReportMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Project Spark - Live Electrical Diagnostics");
            sb.AppendLine();
            sb.AppendLine("Generated (UTC): " + DateTime.UtcNow.ToString("u"));
            sb.AppendLine("Loaded layout: " + ActiveLevelName);
            sb.AppendLine("Solver successes/failures observed: " + SuccessfulSolves + " / " + FailedSolves);
            sb.AppendLine("Component/node frames observed: " + ComponentFramesObserved + " / " + NodeFramesObserved);
            sb.AppendLine("Protocol issues recorded: " + ViolationsObserved);
            sb.AppendLine("Latest circuit/revision: " + LatestCircuitId + " / " + LatestRevision);
            sb.AppendLine();
            sb.AppendLine("## Checks");
            sb.AppendLine();
            sb.AppendLine("| Check | State | Observation |");
            sb.AppendLine("|---|---|---|");
            foreach (Check check in GetChecks())
                sb.AppendLine("| " + Escape(check.name) + " | " + check.state + " | " + Escape(check.detail) + " |");
            sb.AppendLine();
            sb.AppendLine("## Recent activity");
            sb.AppendLine();
            foreach (string item in activity) sb.AppendLine("- " + item);
            sb.AppendLine();
            sb.AppendLine("## Recorded issues");
            sb.AppendLine();
            if (issues.Count == 0) sb.AppendLine("No protocol issues observed. This is not a substitute for Play Mode tests.");
            else foreach (string issue in issues) sb.AppendLine("- " + issue);
            sb.AppendLine();
            sb.AppendLine("This is a read-only observer: it does not validate physical circuit accuracy, change solver state, or force a solve.");
            return sb.ToString();
        }

        private static string Escape(string value) =>
            (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

        public void ResetObservationHistory()
        {
            activity.Clear();
            issues.Clear();
            SuccessfulSolves = 0;
            FailedSolves = 0;
            ComponentFramesObserved = 0;
            NodeFramesObserved = 0;
            LoadedLevels = 0;
            Log("Observation history cleared; no gameplay state changed");
        }
    }
}
