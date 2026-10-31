using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Non-invasive, read-only bridge from published VERIFIED circuit snapshots
    /// to SparkElectricalTelemetryBridge + SparkVisualTeachingLiveBridge.
    /// It never changes circuit state, purchases, components, or shelf placement.
    /// Attach alongside the existing AAA teaching manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSolverTelemetrySync : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;

        [Header("Existing Project Spark visual teaching")]
        [SerializeField] private SparkElectricalTelemetryBridge telemetry;
        [SerializeField] private SparkVisualTeachingLiveBridge liveTeaching;

        [Header("Delivery")]
        [SerializeField, Range(0.02f, 1f)] private float minimumUpdateSeconds = 0.1f;
        [SerializeField] private bool showVerifiedFaultCues = true;
        [SerializeField] private bool forwardVerifiedTutorialActions = true;
        [SerializeField] private bool clearWhenDisabled = false;
        [SerializeField] private bool logRejectedFrames = true;

        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private SparkVerifiedElectricalFrame pending;
        private string currentCircuitId;
        private string lastFaultKey;
        private string lastActionKey;
        private int lastRevision;
        private float nextDispatchTime;
        private readonly HashSet<string> validIds = new HashSet<string>(StringComparer.Ordinal);

        public int AppliedFrameCount { get; private set; }
        public string LastError { get; private set; }
        public string CurrentCircuitId => currentCircuitId;

        private void Awake() => Resolve();

        private void OnEnable()
        {
            Resolve();
            Subscribe();
        }

        private void Start()
        {
            // Handles a publisher initialized later in Unity's startup order.
            Resolve();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            pending = null;
            if (clearWhenDisabled) ClearVisualTelemetry();
        }

        private void OnDestroy() => Unsubscribe();

        private void Resolve()
        {
            if (publisher == null)
                publisher = GetComponent<SparkVerifiedElectricalPublisher>();
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (telemetry == null)
                telemetry = GetComponent<SparkElectricalTelemetryBridge>();
            if (telemetry == null)
                telemetry = FindFirstObjectByType<SparkElectricalTelemetryBridge>(FindObjectsInactive.Include);
            if (liveTeaching == null)
                liveTeaching = GetComponent<SparkVisualTeachingLiveBridge>();
            if (liveTeaching == null)
                liveTeaching = FindFirstObjectByType<SparkVisualTeachingLiveBridge>(FindObjectsInactive.Include);
        }

private void Subscribe()
{
    if (publisher == subscribedPublisher)
        return;

    Unsubscribe();

    subscribedPublisher = publisher;

    if (subscribedPublisher != null)
        subscribedPublisher.FramePublished += OnFramePublished;
}

private void Unsubscribe()
{
    if (subscribedPublisher != null)
        subscribedPublisher.FramePublished -= OnFramePublished;

    subscribedPublisher = null;
}

// Event-compatible wrapper.
private void OnFramePublished(
    SparkVerifiedElectricalFrame verifiedFrame)
{
    SubmitVerifiedFrame(verifiedFrame);
}

        /// <summary>
        /// Alternate API: a trusted solver may call this directly, without Publisher.
        /// Must be called from Unity's main thread, after a completed solve.
        /// Only the most recent frame is retained if solver updates faster than UI.
        /// </summary>
        public bool SubmitVerifiedFrame(SparkVerifiedElectricalFrame verifiedFrame)
        {
            if (verifiedFrame == null) return Reject("Null solver frame.");
            if (!Validate(verifiedFrame, out string error)) return Reject(error);
            pending = verifiedFrame.Copy();
            LastError = null;
            return true;
        }

        private void Update()
        {
            if (pending == null || Time.unscaledTime < nextDispatchTime) return;
            SparkVerifiedElectricalFrame frame = pending;
            pending = null;
            nextDispatchTime = Time.unscaledTime + minimumUpdateSeconds;
            Apply(frame);
        }

        private bool Validate(SparkVerifiedElectricalFrame frame, out string error)
        {
            error = null;
            if (frame.readings == null || frame.readings.Count > 32)
            { error = "Frame needs 0–32 readings."; return false; }
            if (frame.circuitId != null && frame.circuitId.Length > 80)
            { error = "Circuit ID too long."; return false; }
            if (frame.revision < 0)
            { error = "Revision must be nonnegative."; return false; }

            validIds.Clear();
            foreach (SparkVerifiedElectricalReading r in frame.readings)
            {
                if (r == null || !ValidId(r.anchorId) || !validIds.Add(r.anchorId) ||
                    !Finite(r.voltageVolts) || !Finite(r.currentAmps) ||
                    !Finite(r.resistanceOhms) || r.resistanceOhms < 0 ||
                    !Enum.IsDefined(typeof(SparkElectricalCondition), r.condition))
                { error = "Invalid / duplicate component reading."; return false; }
            }
            if (!string.IsNullOrEmpty(frame.focusAnchorId) &&
                !validIds.Contains(frame.focusAnchorId))
            { error = "Focus component is not in this frame."; return false; }
            if (frame.hasVerifiedRoute && frame.routeEnergized)
            {
                if (frame.orderedRouteAnchors == null || frame.orderedRouteAnchors.Length < 2 ||
                    frame.orderedRouteAnchors.Length > 8)
                { error = "Energized route needs 2–8 anchors."; return false; }
                foreach (string id in frame.orderedRouteAnchors)
                    if (!ValidId(id))
                    { error = "Invalid route anchor."; return false; }
            }
            if (frame.hasVerifiedFault &&
                (!ValidId(frame.faultAnchorId) ||
                 string.IsNullOrWhiteSpace(frame.verifiedFaultExplanation)))
            { error = "Verified fault needs anchor ID and explanation."; return false; }
            if (frame.verifiedActionId != null && frame.verifiedActionId.Length > 100)
            { error = "Tutorial action ID too long."; return false; }
            return true;
        }

        private void Apply(SparkVerifiedElectricalFrame frame)
        {
            Resolve();
            if (telemetry == null)
            {
                Reject("SparkElectricalTelemetryBridge not found or not installed.");
                return;
            }
            string circuitId = frame.circuitId ?? "";
            if (!string.Equals(currentCircuitId, circuitId, StringComparison.Ordinal))
                ResetForCircuit(circuitId);

            if (frame.revision > 0 && frame.revision <= lastRevision)
                return; // Rejected old / duplicate solver revision.
            if (frame.revision > 0) lastRevision = frame.revision;

            foreach (SparkVerifiedElectricalReading reading in frame.readings)
            {
                telemetry.ReportVerifiedReading(
                    reading.anchorId,
                    reading.voltageVolts,
                    reading.currentAmps,
                    reading.resistanceOhms,
                    reading.condition,
                    reading.explanation ?? "");
            }

            if (!string.IsNullOrWhiteSpace(frame.focusAnchorId))
                telemetry.SelectComponent(frame.focusAnchorId);

            if (frame.hasVerifiedRoute)
            {
                if (frame.routeEnergized)
                    telemetry.SetVerifiedCurrentRoute(frame.orderedRouteAnchors, true);
                else
                    telemetry.SetVerifiedCurrentRoute(null, false);
            }

            if (showVerifiedFaultCues && liveTeaching != null && frame.hasVerifiedFault)
            {
                // Avoid recreating animations for an unchanged fault each solver tick.
                string key = frame.faultAnchorId + ":" + frame.verifiedFaultExplanation;
                if (!string.Equals(key, lastFaultKey, StringComparison.Ordinal))
                {
                    if (SparkTeachingAnchor.TryFind(frame.faultAnchorId, out _))
                    {
                        liveTeaching.ReportVerifiedFault(frame.faultAnchorId,
                            frame.verifiedFaultExplanation);
                        lastFaultKey = key;
                    }
                }
            }
            else
            {
                lastFaultKey = null;
                // Do NOT clear shared liveTeaching cues; other systems own them.
            }

            if (forwardVerifiedTutorialActions && liveTeaching != null &&
                !string.IsNullOrWhiteSpace(frame.verifiedActionId))
            {
                string key = circuitId + ":" + frame.verifiedActionId;
                if (!string.Equals(lastActionKey, key, StringComparison.Ordinal))
                {
                    liveTeaching.SignalVerifiedAction(frame.verifiedActionId);
                    lastActionKey = key;
                }
            }
            AppliedFrameCount++;
            LastError = null;
        }

        /// <summary>Call from your level loader when the previous circuit is removed.</summary>
        public void OnLevelUnloaded() => ResetForCircuit(null);

        /// <summary>Call when the circuit layout is replaced (even with the same level ID).</summary>
        public void ResetForCircuit(string newCircuitId)
        {
            currentCircuitId = newCircuitId;
            lastRevision = 0;
            lastFaultKey = null;
            lastActionKey = null;
            pending = null;
            ClearVisualTelemetry();
        }

        private void ClearVisualTelemetry()
        {
            if (telemetry != null) telemetry.ClearTelemetry();
            // Intentionally never clears liveTeaching: it may contain unrelated lesson cues.
        }

        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool ValidId(string s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 80;

        private bool Reject(string message)
        {
            LastError = message;
            if (logRejectedFrames)
                Debug.LogWarning("[Spark Solver Sync] " + message, this);
            return false;
        }
    }
}
