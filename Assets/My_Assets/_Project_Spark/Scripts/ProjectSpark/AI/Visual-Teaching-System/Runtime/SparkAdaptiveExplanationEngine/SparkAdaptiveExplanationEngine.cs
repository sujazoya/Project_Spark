using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    public enum SparkAdaptiveSeverity { Information, Warning, Critical }

    /// <summary>Explanation derived ONLY from a published, verified solver snapshot.</summary>
    public sealed class SparkAdaptiveCue
    {
        public string anchorId;
        public string title;
        public string explanation;
        public string nextAction;
        public SparkAdaptiveSeverity severity;
        public bool hasWorldTarget;
    }

    /// <summary>
    /// Read-only adapter. Does not solve circuits, infer wiring, or modify lessons.
    /// Subscribe to the same verified publisher used by SparkSolverTelemetrySync.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAdaptiveExplanationEngine : MonoBehaviour
    {
        [Header("Verified source")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField, Min(.05f)] private float minimumRefreshSeconds = .25f;
        [SerializeField, Range(1, 32)] private int maximumCues = 12;
        [SerializeField] private bool logInvalidFrames = true;

        private readonly List<SparkAdaptiveCue> cues = new List<SparkAdaptiveCue>();
        private SparkVerifiedElectricalPublisher subscribed;
        private SparkVerifiedElectricalFrame pending;
        private SparkVerifiedElectricalFrame current;
        private float nextRefresh;
        private string circuitId;
        private int lastRevision;
        private int selectedIndex;
        private string signature;
        public event Action Changed;
        public SparkVerifiedElectricalFrame CurrentFrame => current;
        public int CueCount => cues.Count;
        public int SelectedIndex => selectedIndex;
        public SparkAdaptiveCue SelectedCue => selectedIndex >= 0 && selectedIndex < cues.Count ? cues[selectedIndex] : null;
        public string LastError { get; private set; }

        private void Awake() => Resolve();

        private void OnEnable()
        {
            Resolve();
            Subscribe();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            pending = null;
        }

        private void OnDestroy() => Unsubscribe();

        private void Resolve()
        {
            if (publisher != null) return;
            publisher = GetComponent<SparkVerifiedElectricalPublisher>();
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribed == publisher) return;
            Unsubscribe();
            subscribed = publisher;
            if (subscribed != null) subscribed.FramePublished += OnFramePublished;
        }

        private void Unsubscribe()
        {
            if (subscribed != null) subscribed.FramePublished -= OnFramePublished;
            subscribed = null;
        }

        // void event handler: avoids CS0407 from subscribing a bool-returning method.
        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            SubmitVerifiedFrame(frame);
        }

        /// <summary>Optional direct path; only call on Unity main thread after a completed solve.</summary>
        public bool SubmitVerifiedFrame(SparkVerifiedElectricalFrame frame)
        {
            if (!Validate(frame, out string error))
            {
                LastError = error;
                if (logInvalidFrames) Debug.LogWarning("[Spark Adaptive] " + error, this);
                return false;
            }
            pending = frame.Copy();
            LastError = null;
            return true;
        }

        private void Update()
        {
            if (pending == null || Time.unscaledTime < nextRefresh) return;
            SparkVerifiedElectricalFrame frame = pending;
            pending = null;
            nextRefresh = Time.unscaledTime + minimumRefreshSeconds;
            Apply(frame);
        }

        private void Apply(SparkVerifiedElectricalFrame frame)
        {
            string incomingId = frame.circuitId ?? "";
            if (!string.Equals(circuitId, incomingId, StringComparison.Ordinal))
                ResetForCircuit(incomingId);

            // Revisions are optional; reject stale revisions when available.
            if (frame.revision > 0 && frame.revision <= lastRevision) return;
            if (frame.revision > 0) lastRevision = frame.revision;

            string selectedAnchor = SelectedCue != null ? SelectedCue.anchorId : null;
            current = frame;
            cues.Clear();

            if (frame.hasVerifiedFault)
            {
                Add(new SparkAdaptiveCue {
                    anchorId = frame.faultAnchorId,
                    title = "VERIFIED CIRCUIT FAULT",
                    explanation = Shorten(frame.verifiedFaultExplanation, 240),
                    nextAction = "Inspect the indicated component and follow the verified diagnostic advice.",
                    severity = SparkAdaptiveSeverity.Critical
                });
            }

            var sorted = new List<SparkVerifiedElectricalReading>(frame.readings);
            sorted.Sort((a, b) => Priority(b.condition).CompareTo(Priority(a.condition)));
            foreach (SparkVerifiedElectricalReading reading in sorted)
            {
                if (cues.Count >= maximumCues) break;
                // The explicit verified fault takes precedence over a duplicate reading.
                if (frame.hasVerifiedFault && reading.anchorId == frame.faultAnchorId) continue;
                cues.Add(MakeReadingCue(reading));
            }

            if (cues.Count == 0)
            {
                Add(new SparkAdaptiveCue {
                    title = "AWAITING COMPONENT DIAGNOSTICS",
                    explanation = "The solver published a frame without classified component readings.",
                    nextAction = "Run an electrical simulation that provides component measurements.",
                    severity = SparkAdaptiveSeverity.Information
                });
            }

            selectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(frame.focusAnchorId))
                SelectByAnchor(frame.focusAnchorId, false);
            else if (!string.IsNullOrWhiteSpace(selectedAnchor))
                SelectByAnchor(selectedAnchor, false);

            foreach (SparkAdaptiveCue cue in cues)
                cue.hasWorldTarget = !string.IsNullOrWhiteSpace(cue.anchorId) &&
                                     SparkTeachingAnchor.TryFind(cue.anchorId, out _);

            // Refresh on data changes, but UI does not force-open automatically.
            string currentSignature = Signature();
            if (!string.Equals(signature, currentSignature, StringComparison.Ordinal))
            {
                signature = currentSignature;
                Changed?.Invoke();
            }
        }

        private static SparkAdaptiveCue MakeReadingCue(SparkVerifiedElectricalReading r)
        {
            var cue = new SparkAdaptiveCue { anchorId = r.anchorId };
            switch (r.condition)
            {
                case SparkElectricalCondition.Failed:
                    cue.title = "COMPONENT FAILURE";
                    cue.nextAction = "Inspect the failed component and use the simulator's failure details.";
                    cue.severity = SparkAdaptiveSeverity.Critical;
                    break;
                case SparkElectricalCondition.OverCurrent:
                    cue.title = "OVERCURRENT DETECTED";
                    cue.nextAction = "Inspect the measured current and the component's permitted limits.";
                    cue.severity = SparkAdaptiveSeverity.Critical;
                    break;
                case SparkElectricalCondition.ReversePolarity:
                    cue.title = "REVERSE POLARITY";
                    cue.nextAction = "Inspect the component's terminals and verify the intended polarity.";
                    cue.severity = SparkAdaptiveSeverity.Warning;
                    break;
                case SparkElectricalCondition.OpenCircuit:
                    cue.title = "OPEN CIRCUIT";
                    cue.nextAction = "Follow the solver's open-path diagnosis and inspect the connections.";
                    cue.severity = SparkAdaptiveSeverity.Warning;
                    break;
                case SparkElectricalCondition.Normal:
                    cue.title = "NORMAL COMPONENT STATE";
                    cue.nextAction = "Compare this reading with the expected result of the tutorial.";
                    cue.severity = SparkAdaptiveSeverity.Information;
                    break;
                default:
                    cue.title = "CONDITION NOT CLASSIFIED";
                    cue.nextAction = "Review the component's available measurements.";
                    cue.severity = SparkAdaptiveSeverity.Information;
                    break;
            }
            string measured = "Solver reading: " +
                r.voltageVolts.ToString("0.###") + " V, " +
                r.currentAmps.ToString("0.####") + " A, " +
                r.resistanceOhms.ToString("0.###") + " Ω.";
            cue.explanation = measured + (string.IsNullOrWhiteSpace(r.explanation) ? "" :
                "  " + Shorten(r.explanation, 160));
            return cue;
        }

        private void Add(SparkAdaptiveCue cue)
        {
            if (cues.Count < maximumCues) cues.Add(cue);
        }

        public void SelectNext()
        {
            if (cues.Count == 0) return;
            selectedIndex = (selectedIndex + 1) % cues.Count;
            Changed?.Invoke();
        }

        public void SelectPrevious()
        {
            if (cues.Count == 0) return;
            selectedIndex = (selectedIndex - 1 + cues.Count) % cues.Count;
            Changed?.Invoke();
        }

        public bool SelectByAnchor(string anchorId, bool notify = true)
        {
            for (int i = 0; i < cues.Count; i++)
            {
                if (!string.Equals(cues[i].anchorId, anchorId, StringComparison.Ordinal)) continue;
                selectedIndex = i;
                if (notify) Changed?.Invoke();
                return true;
            }
            return false;
        }

        public bool HasVerifiedCurrentPath => current != null && current.hasVerifiedRoute &&
                                              current.routeEnergized &&
                                              current.orderedRouteAnchors != null &&
                                              current.orderedRouteAnchors.Length >= 2;

        /// <summary>Clear when switching/reloading a circuit, even if its ID stays unchanged.</summary>
        public void ResetForCircuit(string newCircuitId = null)
        {
            circuitId = newCircuitId;
            current = null;
            pending = null;
            cues.Clear();
            selectedIndex = 0;
            lastRevision = 0;
            signature = null;
            LastError = null;
            Changed?.Invoke();
        }

        private string Signature()
        {
            var cue = SelectedCue;
            return (current?.circuitId ?? "") + "|" + cues.Count + "|" + selectedIndex + "|" +
                   (cue?.anchorId ?? "") + "|" + (cue?.title ?? "") + "|" +
                   (cue?.explanation ?? "") + "|" + HasVerifiedCurrentPath;
        }

        private static bool Validate(SparkVerifiedElectricalFrame f, out string error)
        {
            error = null;
            if (f == null) { error = "Null solver frame."; return false; }
            if (f.circuitId != null && f.circuitId.Length > 80 || f.revision < 0 ||
                f.readings == null || f.readings.Count > 32)
            { error = "Invalid frame metadata or reading count."; return false; }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkVerifiedElectricalReading r in f.readings)
            {
                if (r == null || !ValidId(r.anchorId) || !ids.Add(r.anchorId) ||
                    !Finite(r.voltageVolts) || !Finite(r.currentAmps) ||
                    !Finite(r.resistanceOhms) || r.resistanceOhms < 0 ||
                    !Enum.IsDefined(typeof(SparkElectricalCondition), r.condition))
                { error = "Invalid or duplicated component reading."; return false; }
            }
            if (f.hasVerifiedFault && (!ValidId(f.faultAnchorId) ||
                string.IsNullOrWhiteSpace(f.verifiedFaultExplanation) || f.verifiedFaultExplanation.Length > 500))
            { error = "Invalid verified fault information."; return false; }
            if (f.hasVerifiedRoute && f.routeEnergized)
            {
                if (f.orderedRouteAnchors == null || f.orderedRouteAnchors.Length < 2 ||
                    f.orderedRouteAnchors.Length > 8)
                { error = "Invalid verified current route."; return false; }
                foreach (string anchor in f.orderedRouteAnchors)
                    if (!ValidId(anchor)) { error = "Invalid route anchor ID."; return false; }
            }
            return true;
        }

        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id.Length <= 80;
        private static string Shorten(string s, int max) => string.IsNullOrEmpty(s) ? "" :
            s.Length > max ? s.Substring(0, max) + "…" : s;
        private static int Priority(SparkElectricalCondition c)
        {
            switch (c)
            {
                case SparkElectricalCondition.Failed: return 6;
                case SparkElectricalCondition.OverCurrent: return 5;
                case SparkElectricalCondition.ReversePolarity: return 4;
                case SparkElectricalCondition.OpenCircuit: return 3;
                case SparkElectricalCondition.Normal: return 1;
                default: return 0;
            }
        }
    }
}
