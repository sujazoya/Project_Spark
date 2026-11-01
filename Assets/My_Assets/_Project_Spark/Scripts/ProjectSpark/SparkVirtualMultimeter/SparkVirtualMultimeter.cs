using System;
using System.Collections.Generic;
using System.Globalization;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only virtual meter. Never inserts itself into the circuit, injects voltage,
    /// computes new electrical states or advances level objectives.
    /// Voltage: verified node potentials (preferred), OR matching signed component endpoints.
    /// Current: non-invasive signed component reading, NOT a two-lead ammeter.
    /// Ohms: solver-modeled component resistance, NOT a physical live ohmmeter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVirtualMultimeter : MonoBehaviour
    {
        public enum MeterMode { Volts, ComponentAmps, ModelOhms }
        public enum DrawerMode { Hidden, Small, Expanded }
        private enum ProbeSelection { None, Red, Black }

        [Header("Authoritative Sources")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkMeterVerifiedNodeFeed nodeFeed;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private LayerMask probeLayers = ~0;
        [SerializeField, Min(1f)] private float probeRayDistance = 120f;
        [SerializeField, Min(0.5f)] private float maxFrameAgeSeconds = 8f;

        [Header("Generated UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text readingText;
        [SerializeField] private TMP_Text unitText;
        [SerializeField] private TMP_Text redProbeText;
        [SerializeField] private TMP_Text blackProbeText;
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text guidanceText;
        [SerializeField] private TMP_Text comparisonText;
        [SerializeField] private TMP_Text footerText;
        [SerializeField] private Button captureAButton;
        [SerializeField] private Button captureBButton;

        [Header("Three-Mode Drawer (Builder Upgrade)")]
        [SerializeField] private GameObject smallPanel;
        [SerializeField] private TMP_Text smallReadingText;
        [SerializeField] private TMP_Text smallModeText;
        [SerializeField] private TMP_Text smallUnitText;
        [SerializeField] private TMP_Text smallProbeText;
        [SerializeField] private TMP_Text drawerTabText;
        [SerializeField] private CanvasGroup expandedCanvasGroup;
        [SerializeField] private CanvasGroup smallCanvasGroup;
        [SerializeField, Min(0.05f)] private float drawerTransitionSeconds = 0.28f;
        [SerializeField, Range(0f, 80f)] private float slidePixels = 32f;
        [SerializeField, Range(0.8f, 1f)] private float startScale = 0.97f;

        [Header("Verified Reading Text Animation")]
        [SerializeField] private bool animateReadingChanges = true;
        [SerializeField, Range(0.05f, 0.8f)] private float readingPulseSeconds = 0.23f;
        [SerializeField, Range(1f, 1.25f)] private float readingPulseScale = 1.075f;
        [SerializeField] private Color readingPulseTint = new Color(0.49f, 1f, .82f, 1f);

        private DrawerMode drawerMode = DrawerMode.Hidden;
        private float expandedVisibility;
        private float smallVisibility;
        private RectTransform expandedRect;
        private RectTransform smallRect;
        private Vector2 expandedRestPosition;
        private Vector2 smallRestPosition;
        private Vector3 expandedRestScale = Vector3.one;
        private Vector3 smallRestScale = Vector3.one;
        private bool drawerInitialized;
        private string lastRenderedReading;
        private float pulseStartedAt = -100f;
        private Color primaryReadingColor = Color.white;
        private Color compactReadingColor = Color.white;
        private Vector3 primaryReadingScale = Vector3.one;
        private Vector3 compactReadingScale = Vector3.one;

        public DrawerMode CurrentDrawerMode => drawerMode;

        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private SparkMeterVerifiedNodeFeed subscribedNodeFeed;
        private LevelGamePlayManager subscribedLevel;
        private SparkVerifiedElectricalFrame latestFrame;
        private SparkMeterVerifiedNodeFrame latestNodes;
        private SparkMeterProbeTerminal redTerminal;
        private SparkMeterProbeTerminal blackTerminal;
        private ProbeSelection selecting;
        private MeterMode mode;
        private int frameSequence;
        private float lastFrameAt = -100000f;
        private float nextResolveAt;
        private bool holding;
        private string heldDisplay;
        private string message = "Select a mode and click RED, then BLACK probe targets.";
        private Measurement? capturedA;

        private struct Measurement
        {
            public MeterMode mode;
            public string circuitId;
            public string redId;
            public string blackId;
            public int serial;
            public float value;
            public string unit;
            public string origin;
        }

        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private void Awake()
        {
            Resolve();
            InitializeDrawer();
            Refresh();
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            InitializeDrawer();
            Refresh();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
        }

        private void OnDisable() { Unsubscribe(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Update()
        {
            UpdateDrawerAnimation();
            UpdateReadingPulse();

            if ((publisher == null || sceneCamera == null) && Time.unscaledTime >= nextResolveAt)
            {
                nextResolveAt = Time.unscaledTime + 2f;
                Resolve();
                Subscribe();
            }

                if (selecting != ProbeSelection.None &&
            UnityEngine.Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            TryPickTerminal();
        }

            // Refresh stale-reading indicator without rebuilding UI every frame.
            if (drawerMode != DrawerMode.Hidden && Time.frameCount % 30 == 0)
                Refresh();
        }

        private void Resolve()
        {
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (nodeFeed == null)
                nodeFeed = FindFirstObjectByType<SparkMeterVerifiedNodeFeed>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

        private void Subscribe()
        {
            if (publisher != subscribedPublisher)
            {
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished -= OnVerifiedFrame;
                subscribedPublisher = publisher;
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished += OnVerifiedFrame;
            }
            if (nodeFeed != subscribedNodeFeed)
            {
                if (subscribedNodeFeed != null)
                    subscribedNodeFeed.FramePublished -= OnNodeFrame;
                subscribedNodeFeed = nodeFeed;
                if (subscribedNodeFeed != null)
                    subscribedNodeFeed.FramePublished += OnNodeFrame;
            }
            if (levelManager != subscribedLevel)
            {
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted -= OnLevelReloaded;
                    subscribedLevel.LevelReset -= OnLevelReloaded;
                }
                subscribedLevel = levelManager;
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted += OnLevelReloaded;
                    subscribedLevel.LevelReset += OnLevelReloaded;
                }
            }
        }

        private void Unsubscribe()
        {
            if (subscribedPublisher != null)
                subscribedPublisher.FramePublished -= OnVerifiedFrame;
            if (subscribedNodeFeed != null)
                subscribedNodeFeed.FramePublished -= OnNodeFrame;
            if (subscribedLevel != null)
            {
                subscribedLevel.LevelStarted -= OnLevelReloaded;
                subscribedLevel.LevelReset -= OnLevelReloaded;
            }
            subscribedPublisher = null;
            subscribedNodeFeed = null;
            subscribedLevel = null;
        }

        private void OnLevelReloaded(SparkLevelDefinition ignored)
        {
            latestFrame = null;
            latestNodes = null;
            redTerminal = null;
            blackTerminal = null;
            capturedA = null;
            selecting = ProbeSelection.None;
            holding = false;
            message = "Level changed. Awaiting a fresh verified solve.";
            Refresh();
        }

        private void OnVerifiedFrame(SparkVerifiedElectricalFrame frame)
        {
            if (!ValidateFrame(frame)) return;
            if (latestFrame != null &&
                !string.Equals(latestFrame.circuitId, frame.circuitId, StringComparison.Ordinal))
            {
                latestNodes = null;
                redTerminal = null;
                blackTerminal = null;
                capturedA = null;
                holding = false;
                message = "Circuit identity changed; place probes again.";
            }
            else if (latestFrame != null && frame.revision > 0 && latestFrame.revision > 0 &&
                     frame.revision <= latestFrame.revision)
            {
                return; // Ignore older/duplicate positive revisions.
            }

            latestFrame = frame.Copy();
            frameSequence++;
            lastFrameAt = Time.unscaledTime;
            Refresh();
        }

        private void OnNodeFrame(SparkMeterVerifiedNodeFrame frame)
        {
            // Validated by SparkMeterVerifiedNodeFeed. It must match the current
            // electrical-frame revision at measurement time.
            if (frame != null) latestNodes = frame.Copy();
            Refresh();
        }

        private static bool ValidateFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || string.IsNullOrWhiteSpace(frame.circuitId) ||
                frame.circuitId.Length > 80 || frame.revision < 0 || frame.readings == null ||
                frame.readings.Count > 128) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in frame.readings)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.anchorId) ||
                    !ids.Add(r.anchorId) || !Finite(r.voltageVolts) ||
                    !Finite(r.currentAmps) || !Finite(r.resistanceOhms) ||
                    r.resistanceOhms < 0f) return false;
            }
            return true;
        }

        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);

        // Existing API compatibility: Show() still opens the full meter.
        public void Show() => ShowExpanded();
        public void ShowExpanded() => SetDrawerMode(DrawerMode.Expanded);
        public void ShowSmall() => SetDrawerMode(DrawerMode.Small);
        public void Minimize() => ShowSmall();
        public void Hide()
        {
            selecting = ProbeSelection.None;
            SetDrawerMode(DrawerMode.Hidden);
        }

        // Floating tab cycles: Hidden -> Small -> Expanded -> Hidden.
        public void Toggle()
        {
            if (drawerMode == DrawerMode.Hidden) ShowSmall();
            else if (drawerMode == DrawerMode.Small) ShowExpanded();
            else Hide();
        }

        private void InitializeDrawer()
        {
            if (drawerInitialized) return;
            drawerInitialized = true;
            expandedRect = panel != null ? panel.GetComponent<RectTransform>() : null;
            smallRect = smallPanel != null ? smallPanel.GetComponent<RectTransform>() : null;
            if (expandedCanvasGroup == null && panel != null)
                expandedCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (smallCanvasGroup == null && smallPanel != null)
                smallCanvasGroup = smallPanel.GetComponent<CanvasGroup>();
            if (expandedRect != null)
            {
                expandedRestPosition = expandedRect.anchoredPosition;
                expandedRestScale = expandedRect.localScale;
            }
            if (smallRect != null)
            {
                smallRestPosition = smallRect.anchoredPosition;
                smallRestScale = smallRect.localScale;
            }
            if (readingText != null)
            {
                primaryReadingColor = readingText.color;
                primaryReadingScale = readingText.rectTransform.localScale;
            }
            if (smallReadingText != null)
            {
                compactReadingColor = smallReadingText.color;
                compactReadingScale = smallReadingText.rectTransform.localScale;
            }
            expandedVisibility = 0f;
            smallVisibility = 0f;
            ApplyDrawerPanel(panel, expandedCanvasGroup, expandedRect,
                expandedRestPosition, expandedRestScale, 0f, false);
            ApplyDrawerPanel(smallPanel, smallCanvasGroup, smallRect,
                smallRestPosition, smallRestScale, 0f, false);
            UpdateTabLabel();
        }

        private void SetDrawerMode(DrawerMode targetMode)
        {
            InitializeDrawer();
            // Old manually built meters remain usable without the compact UI upgrade.
            if (targetMode == DrawerMode.Small && smallPanel == null)
                targetMode = DrawerMode.Expanded;
            drawerMode = targetMode;
            if (drawerMode == DrawerMode.Expanded && panel != null) panel.SetActive(true);
            if (drawerMode == DrawerMode.Small && smallPanel != null) smallPanel.SetActive(true);
            UpdateTabLabel();
            UpdateDrawerAnimation();
            Refresh();
        }

        private void UpdateTabLabel()
        {
            if (drawerTabText == null) return;
            string label = drawerMode == DrawerMode.Hidden ? "OPEN METER" :
                drawerMode == DrawerMode.Small ? "EXPAND METER" : "HIDE METER";
            if (drawerTabText.text != label) drawerTabText.text = label;
        }

        private void UpdateDrawerAnimation()
        {
            if (!drawerInitialized) return;
            float seconds = Mathf.Max(0.05f, drawerTransitionSeconds);
            float step = Time.unscaledDeltaTime / seconds;
            float expandedTarget = drawerMode == DrawerMode.Expanded ? 1f : 0f;
            float smallTarget = drawerMode == DrawerMode.Small ? 1f : 0f;
            expandedVisibility = Mathf.MoveTowards(expandedVisibility, expandedTarget, step);
            smallVisibility = Mathf.MoveTowards(smallVisibility, smallTarget, step);
            ApplyDrawerPanel(panel, expandedCanvasGroup, expandedRect,
                expandedRestPosition, expandedRestScale, expandedVisibility,
                drawerMode == DrawerMode.Expanded);
            ApplyDrawerPanel(smallPanel, smallCanvasGroup, smallRect,
                smallRestPosition, smallRestScale, smallVisibility,
                drawerMode == DrawerMode.Small);
        }

        private void ApplyDrawerPanel(GameObject target, CanvasGroup group,
            RectTransform rect, Vector2 restPosition, Vector3 restScale,
            float visibility, bool selected)
        {
            if (target == null) return;
            bool shouldBeActive = selected || visibility > .0001f;
            if (target.activeSelf != shouldBeActive) target.SetActive(shouldBeActive);
            // Builder adds groups; keep legacy meter functional even without upgrade.
            if (group == null) return;
            float eased = visibility * visibility * (3f - 2f * visibility);
            group.alpha = eased;
            group.interactable = selected && visibility >= .99f;
            group.blocksRaycasts = selected && visibility >= .99f;
            if (rect != null)
            {
                rect.anchoredPosition = restPosition + Vector2.right * (slidePixels * (1f - eased));
                rect.localScale = restScale * Mathf.Lerp(startScale, 1f, eased);
            }
        }

        private void SetDisplayedReading(string text, bool verifiedAndLive)
        {
            if (readingText != null && readingText.text != text) readingText.text = text;
            if (smallReadingText != null && smallReadingText.text != text) smallReadingText.text = text;
            if (lastRenderedReading == text) return;
            lastRenderedReading = text;
            if (animateReadingChanges && verifiedAndLive &&
                (readingText != null || smallReadingText != null))
                pulseStartedAt = Time.unscaledTime;
            else
                EndReadingPulse();
        }

        private void UpdateReadingPulse()
        {
            if (pulseStartedAt < 0f) return;
            float elapsed = Time.unscaledTime - pulseStartedAt;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.05f, readingPulseSeconds));
            float fade = 1f - (1f - t) * (1f - t) * (1f - t);
            float pulse = 1f - fade;
            if (readingText != null)
            {
                readingText.rectTransform.localScale = primaryReadingScale *
                    (1f + (readingPulseScale - 1f) * pulse);
                readingText.color = Color.Lerp(primaryReadingColor, readingPulseTint, .60f * pulse);
            }
            if (smallReadingText != null)
            {
                smallReadingText.rectTransform.localScale = compactReadingScale *
                    (1f + (readingPulseScale - 1f) * pulse);
                smallReadingText.color = Color.Lerp(compactReadingColor, readingPulseTint, .60f * pulse);
            }
            if (t >= 1f) EndReadingPulse();
        }

        private void EndReadingPulse()
        {
            pulseStartedAt = -100f;
            if (readingText != null)
            {
                readingText.rectTransform.localScale = primaryReadingScale;
                readingText.color = primaryReadingColor;
            }
            if (smallReadingText != null)
            {
                smallReadingText.rectTransform.localScale = compactReadingScale;
                smallReadingText.color = compactReadingColor;
            }
        }

        public void SetVolts() { SwitchMode(MeterMode.Volts); }
        public void SetAmps() { SwitchMode(MeterMode.ComponentAmps); }
        public void SetOhms() { SwitchMode(MeterMode.ModelOhms); }

        private void SwitchMode(MeterMode next)
        {
            mode = next;
            capturedA = null;
            holding = false;
            message = next == MeterMode.Volts
                ? "V: select RED and BLACK terminals. Same-component reference pair, or matching node feed."
                : next == MeterMode.ComponentAmps
                    ? "A: select RED on a component; shows its signed solver current (non-invasive)."
                    : "Ω: select both ends of one component; shows its modeled resistance only when de-energized.";
            Refresh();
        }

        public void ArmRedProbe()
        {
            selecting = ProbeSelection.Red;
            message = "RED PROBE armed. Click a marked electrical terminal in the 3D scene.";
            Refresh();
        }

        public void ArmBlackProbe()
        {
            selecting = ProbeSelection.Black;
            message = "BLACK PROBE armed. Click a marked electrical terminal in the 3D scene.";
            Refresh();
        }

        private void TryPickTerminal()
        {
            if (sceneCamera == null) { message = "Scene camera not assigned."; Refresh(); return; }
            RaycastHit[] hits = Physics.RaycastAll(
            sceneCamera.ScreenPointToRay(
                UnityEngine.Input.mousePosition),
            probeRayDistance,
            probeLayers,
            QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            SparkMeterProbeTerminal found = null;
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                found = hit.collider.GetComponent<SparkMeterProbeTerminal>();
                if (found == null) found = hit.collider.GetComponentInParent<SparkMeterProbeTerminal>();
                if (found != null && found.isActiveAndEnabled) break;
            }
            if (found == null)
            {
                message = "No SparkMeterProbeTerminal found under cursor. Add it to a terminal collider.";
                Refresh();
                return;
            }
            if (string.IsNullOrWhiteSpace(found.TerminalId))
            {
                message = "TerminalId is empty. Assign its stable terminal ID in Inspector.";
                Refresh();
                return;
            }
            if (selecting == ProbeSelection.Red)
            {
                redTerminal = found;
                selecting = mode == MeterMode.ComponentAmps ? ProbeSelection.None : ProbeSelection.Black;
                message = mode == MeterMode.ComponentAmps
                    ? "Component selected. Current comes from the verified solver frame."
                    : "Red probe placed. Click the BLACK terminal next.";
            }
            else if (selecting == ProbeSelection.Black)
            {
                blackTerminal = found;
                selecting = ProbeSelection.None;
                message = "Both probes placed. Showing supported verified measurement.";
            }
            holding = false;
            Refresh();
        }

        public void ClearProbes()
        {
            redTerminal = null;
            blackTerminal = null;
            selecting = ProbeSelection.None;
            capturedA = null;
            holding = false;
            message = "Probes cleared. Select a new measurement position.";
            Refresh();
        }

        public void ToggleHold()
        {
            if (!holding)
            {
                if (!TryMeasure(out Measurement m, out string why))
                {
                    message = why; Refresh(); return;
                }
                heldDisplay = FormatValue(m.value, m.unit);
                holding = true;
                message = "DISPLAY HOLD: frozen value only; not a live reading.";
            }
            else
            {
                holding = false;
                message = "LIVE readings resumed.";
            }
            Refresh();
        }

        private bool TryMeasure(out Measurement reading, out string reason)
        {
            reading = default;
            if (latestFrame == null)
            { reason = "No completed solver frame received yet."; return false; }
            if (Time.unscaledTime - lastFrameAt > maxFrameAgeSeconds)
            { reason = "SOLVER FRAME STALE — recalculate the circuit before measuring."; return false; }
            if (redTerminal == null || string.IsNullOrWhiteSpace(redTerminal.TerminalId))
            { reason = "Select a RED probe terminal first."; return false; }
            if (mode != MeterMode.ComponentAmps && (blackTerminal == null ||
                string.IsNullOrWhiteSpace(blackTerminal.TerminalId)))
            { reason = "Select a BLACK probe terminal."; return false; }

            reading.mode = mode;
            reading.circuitId = latestFrame.circuitId;
            reading.redId = redTerminal.TerminalId;
            reading.blackId = blackTerminal != null ? blackTerminal.TerminalId : "";
            reading.serial = frameSequence;

            if (mode == MeterMode.Volts)
            {
                if (latestNodes != null && latestFrame.revision > 0 &&
                    latestNodes.revision == latestFrame.revision &&
                    string.Equals(latestNodes.circuitId, latestFrame.circuitId, StringComparison.Ordinal) &&
                    TryNodePotential(redTerminal.TerminalId, out float vr) &&
                    TryNodePotential(blackTerminal.TerminalId, out float vb))
                {
                    reading.value = vr - vb;
                    reading.unit = "V";
                    reading.origin = "Verified node potentials (same solve / common reference)";
                    reason = null;
                    return true;
                }
                if (TrySameComponentReading(out SparkVerifiedElectricalReading component, out reason))
                {
                    bool positiveRed = redTerminal.ReferenceEnd == SparkMeterTerminalEnd.PositiveReference &&
                        blackTerminal.ReferenceEnd == SparkMeterTerminalEnd.NegativeReference;
                    bool negativeRed = redTerminal.ReferenceEnd == SparkMeterTerminalEnd.NegativeReference &&
                        blackTerminal.ReferenceEnd == SparkMeterTerminalEnd.PositiveReference;
                    if (!positiveRed && !negativeRed)
                    {
                        reason = "Terminals must mark opposite solver reference ends for component voltage.";
                        return false;
                    }
                    reading.value = positiveRed ? component.voltageVolts : -component.voltageVolts;
                    reading.unit = "V";
                    reading.origin = "Verified component voltage (marked solver reference polarity)";
                    reason = null;
                    return true;
                }
                reason = "Arbitrary node voltage needs a matching SparkMeterVerifiedNodeFeed; " +
                    "otherwise choose both signed reference ends of one component.";
                return false;
            }

            if (mode == MeterMode.ComponentAmps)
            {
                if (!TryFindComponentReading(redTerminal.ComponentAnchorId, out var comp))
                { reason = "Selected terminal has no matching component current in this solver frame."; return false; }
                reading.value = comp.currentAmps;
                reading.unit = "A";
                reading.origin = "Signed solver component current; non-invasive, not a series ammeter";
                reason = null;
                return true;
            }

            if (!TrySameComponentReading(out var ohmComponent, out reason))
                return false;
            if (Mathf.Abs(ohmComponent.voltageVolts) > .05f ||
                Mathf.Abs(ohmComponent.currentAmps) > .000001f)
            {
                reason = "MODEL Ω unavailable while this component has voltage/current. " +
                    "De-energize and publish a new solver frame.";
                return false;
            }
            reading.value = ohmComponent.resistanceOhms;
            reading.unit = "Ω";
            reading.origin = "Solver-modeled component resistance, NOT actual in-circuit ohmmeter";
            reason = null;
            return true;
        }

        private bool TryNodePotential(string id, out float value)
        {
            value = 0f;
            if (latestNodes == null || latestNodes.nodes == null) return false;
            foreach (var node in latestNodes.nodes)
            {
                if (node != null && string.Equals(node.terminalId, id, StringComparison.Ordinal))
                { value = node.potentialVolts; return true; }
            }
            return false;
        }

        private bool TrySameComponentReading(out SparkVerifiedElectricalReading found, out string reason)
        {
            found = null;
            if (redTerminal == null || blackTerminal == null || redTerminal == blackTerminal ||
                string.Equals(redTerminal.TerminalId, blackTerminal.TerminalId, StringComparison.Ordinal))
            { reason = "Choose two distinct physical terminals."; return false; }
            if (string.IsNullOrWhiteSpace(redTerminal.ComponentAnchorId) ||
                !string.Equals(redTerminal.ComponentAnchorId, blackTerminal.ComponentAnchorId, StringComparison.Ordinal))
            { reason = "These terminals do not belong to one verified component."; return false; }
            if (!TryFindComponentReading(redTerminal.ComponentAnchorId, out found))
            { reason = "No verified component reading with the selected componentAnchorId."; return false; }
            reason = null;
            return true;
        }

        private bool TryFindComponentReading(string id, out SparkVerifiedElectricalReading found)
        {
            found = null;
            if (latestFrame == null || latestFrame.readings == null || string.IsNullOrWhiteSpace(id)) return false;
            foreach (var r in latestFrame.readings)
            {
                if (r != null && string.Equals(r.anchorId, id, StringComparison.Ordinal))
                { found = r; return true; }
            }
            return false;
        }

        public void CaptureA()
        {
            if (!TryMeasure(out Measurement result, out string reason))
            { message = reason; Refresh(); return; }
            capturedA = result;
            message = "A captured. Change the real circuit using existing controls; wait for a NEW solve.";
            if (comparisonText != null)
                comparisonText.text = "A  " + FormatValue(result.value, result.unit) +
                    "  /  " + result.origin + "\nCapture B after a fresh solver frame.";
            Refresh();
        }

        public void CaptureB()
        {
            if (!capturedA.HasValue) { message = "Capture A first."; Refresh(); return; }
            if (!TryMeasure(out Measurement result, out string reason))
            { message = reason; Refresh(); return; }
            Measurement a = capturedA.Value;
            if (a.serial == result.serial || result.serial < a.serial)
            { message = "B needs a new completed solver frame since A."; Refresh(); return; }
            if (a.mode != result.mode || a.circuitId != result.circuitId ||
                a.redId != result.redId ||
                (mode != MeterMode.ComponentAmps && a.blackId != result.blackId) ||
                a.origin != result.origin)
            { message = "A and B must use the same mode, component/terminals and evidence type."; Refresh(); return; }
            float delta = result.value - a.value;
            if (comparisonText != null)
                comparisonText.text = "A   " + FormatValue(a.value, a.unit) +
                    "\nB   " + FormatValue(result.value, result.unit) +
                    "\nΔ   " + FormatValue(delta, result.unit) +
                    "\n" + (Mathf.Abs(delta) < 0.000001f ? "No measurable change at this resolution." :
                    "Verified difference in reported solver measurements.") +
                    "\n" + result.origin;
            message = "B captured from a newer verified solve. Comparison complete.";
            Refresh();
        }

        private static string FormatValue(float value, string unit)
        {
            if (unit == "A") return (value * 1000f).ToString("+0.000;-0.000;0.000", Culture) + " mA";
            if (unit == "Ω") return value.ToString("0.###", Culture) + " Ω";
            return value.ToString("+0.000;-0.000;0.000", Culture) + " V";
        }

        private void Refresh()
        {
            string modeLabel = mode == MeterMode.Volts ? "DC VOLTAGE" :
                mode == MeterMode.ComponentAmps ? "COMPONENT CURRENT" : "MODEL RESISTANCE";
            if (modeText != null && modeText.text != modeLabel) modeText.text = modeLabel;
            if (smallModeText != null && smallModeText.text != modeLabel) smallModeText.text = modeLabel;
            if (redProbeText != null)
                redProbeText.text = "RED  " + (redTerminal != null ? redTerminal.TerminalId : "—") +
                    (selecting == ProbeSelection.Red ? "  • SELECTING" : "");
            if (blackProbeText != null)
                blackProbeText.text = "BLACK  " + (blackTerminal != null ? blackTerminal.TerminalId : "—") +
                    (selecting == ProbeSelection.Black ? "  • SELECTING" : "");
            if (smallProbeText != null)
                smallProbeText.text = "R: " + (redTerminal != null ? redTerminal.TerminalId : "—") +
                    "    B: " + (blackTerminal != null ? blackTerminal.TerminalId : "—") +
                    (selecting != ProbeSelection.None ? "  • SELECT" : "");
            if (sourceText != null)
                sourceText.text = latestFrame == null ? "SOLVER: WAITING" :
                    "SOLVER: " + latestFrame.circuitId + "  /  REV " + latestFrame.revision;
            if (guidanceText != null) guidanceText.text = message;

            string nextReading;
            string nextEvidence;
            bool liveVerified = false;
            if (holding)
            {
                nextReading = heldDisplay;
                nextEvidence = "HOLD — NOT LIVE";
            }
            else if (TryMeasure(out Measurement m, out string reason))
            {
                nextReading = FormatValue(m.value, m.unit);
                nextEvidence = m.origin;
                liveVerified = true;
            }
            else
            {
                nextReading = "— — —";
                nextEvidence = reason;
            }
            SetDisplayedReading(nextReading, liveVerified);
            if (unitText != null && unitText.text != nextEvidence) unitText.text = nextEvidence;
            if (smallUnitText != null)
            {
                string compactEvidence = holding ? "HOLD  •  NOT LIVE" :
                    liveVerified ? "VERIFIED  •  LIVE" : "NO VERIFIED READING";
                if (smallUnitText.text != compactEvidence) smallUnitText.text = compactEvidence;
            }
            if (footerText != null)
                footerText.text = "READ ONLY • No injected signals • No physical continuity test";
            if (captureAButton != null) captureAButton.interactable = !holding;
            if (captureBButton != null) captureBButton.interactable = !holding && capturedA.HasValue;
        }

    }
}
