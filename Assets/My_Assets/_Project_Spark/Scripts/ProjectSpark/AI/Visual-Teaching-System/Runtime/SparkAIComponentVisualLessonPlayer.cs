using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Verified, read-only, component-specific teaching. A published solver frame and
    /// authoritative runtime component/level states are the only electrical sources.
    /// Never classifies a short from AI prose, never guesses polarity or circuit wiring.
    /// Prepared lessons do not automatically replace the active AAA visual lesson.
    /// Must run on Unity's main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIComponentVisualLessonPlayer : MonoBehaviour
    {
        private enum Topic { ShortCircuit, ResistorBurned, ResistorHeating, ResistorOvercurrent,
            LEDReversePolarity, SwitchNotConducting, GenericFailure, GenericOpen, GenericPolarity }

        private sealed class Case
        {
            public Topic topic;
            public string anchorId;
            public string headline;
            public string evidence;
            public string action;
            public SparkSchematicKind symbol;
            public Color tint;
            public string sourceKey;
        }

        private sealed class Step
        {
            public string title;
            public string body;
            public string worldLabel;
        }

        [Header("Verified sources")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkVisualTeachingManager teacher;
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;

        [Header("Generated UI (Editor builder)")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text stepCounterText;
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepBodyText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button previousCaseButton;
        [SerializeField] private Button nextCaseButton;
        [SerializeField] private Button previousStepButton;
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button worldButton;
        [SerializeField] private Button boardButton;

        [Header("Options")]
        [SerializeField, Range(2f, 15f)] private float secondsPerStep = 5f;
        [SerializeField] private bool openWhenNewFault = false;
        [SerializeField] private bool inspectLevelShorts = true;
        [SerializeField] private bool includeStandaloneComponentFailures = true;

        private readonly List<Case> cases = new List<Case>();
        private readonly List<Step> steps = new List<Step>();
        private readonly List<SparkWorldAnnotationRenderer.Annotation> ownAnnotations =
            new List<SparkWorldAnnotationRenderer.Annotation>();
        private SparkVerifiedElectricalPublisher subscribed;
        private SparkVerifiedElectricalFrame snapshot;
        private SparkVerifiedElectricalFrame pending;
        private int selectedCase;
        private int selectedStep;
        private int lastRevision;
        private string circuitId;
        private string currentKey;
        private bool paused;
        private bool stale;
        private Coroutine playback;
        private Coroutine drawAnimation;
        private string statusOverride;

        public int CaseCount => cases.Count;
        public string CurrentTopic => cases.Count > 0 ? cases[selectedCase].headline : "No verified lesson";

        private void Awake()
        {
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable() { Resolve(); Subscribe(); Refresh(); }
        private void Start() { Resolve(); Subscribe(); }
        private void OnDisable() { Unsubscribe(); StopLesson(); ClearOwnAnnotations(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (publisher == null) publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (teacher == null) teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (worldRenderer == null) worldRenderer = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
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

        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            if (frame != null) pending = frame.Copy();
        }

        /// <summary>Optional direct handoff if your solver does not use the publisher.</summary>
        public bool SubmitVerifiedFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || frame.readings == null || frame.readings.Count > 32) return false;
            pending = frame.Copy();
            return true;
        }

        private void Update()
        {
            if (pending == null) return;
            SparkVerifiedElectricalFrame incoming = pending;
            pending = null;
            if (!Validate(incoming))
            {
                statusOverride = "Invalid frame ignored. Check circuit IDs, revisions and readings.";
                Refresh();
                return;
            }
            string incomingCircuit = incoming.circuitId ?? "";
            if (!string.Equals(circuitId, incomingCircuit, StringComparison.Ordinal))
            {
                lastRevision = 0;
                circuitId = incomingCircuit;
                currentKey = null;
                snapshot = null;
                cases.Clear();
                StopLesson();
                ClearOwnAnnotations();
            }
            if (incoming.revision > 0 && incoming.revision <= lastRevision) return;
            if (incoming.revision > 0) lastRevision = incoming.revision;
            snapshot = incoming;
            BuildCases(incoming);
        }

        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id.Length <= 80;

        private static bool Validate(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || frame.readings == null || frame.readings.Count > 32 ||
                frame.revision < 0 || (frame.circuitId != null && frame.circuitId.Length > 80)) return false;
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkVerifiedElectricalReading reading in frame.readings)
            {
                if (reading == null || !ValidId(reading.anchorId) || !ids.Add(reading.anchorId) ||
                    !Finite(reading.voltageVolts) || !Finite(reading.currentAmps) ||
                    !Finite(reading.resistanceOhms) || reading.resistanceOhms < 0 ||
                    !Enum.IsDefined(typeof(SparkElectricalCondition), reading.condition)) return false;
            }
            if (frame.hasVerifiedFault && !ValidId(frame.faultAnchorId)) return false;
            if (frame.hasVerifiedRoute && frame.routeEnergized)
            {
                if (frame.orderedRouteAnchors == null || frame.orderedRouteAnchors.Length < 2 ||
                    frame.orderedRouteAnchors.Length > 8) return false;
                foreach (string id in frame.orderedRouteAnchors) if (!ValidId(id)) return false;
            }
            return true;
        }

        private static string Number(float x, string fmt = "0.###") =>
            x.ToString(fmt, CultureInfo.InvariantCulture);

        private void BuildCases(SparkVerifiedElectricalFrame frame)
        {
            string previous = SelectedKey();
            List<Case> next = new List<Case>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            // These booleans come directly from the authoritative level evaluator.
            bool shorted = inspectLevelShorts && levelManager != null && levelManager.ActiveLevel != null &&
                (levelManager.SourceShorted || levelManager.TargetShorted);
            if (shorted)
            {
                next.Add(new Case {
                    topic = Topic.ShortCircuit, sourceKey = "level-short", anchorId = null,
                    headline = "VERIFIED SHORT CIRCUIT", symbol = SparkSchematicKind.Generic,
                    tint = new Color(1f, .35f, .3f, 1f),
                    evidence = "Level evaluator: " + (levelManager.SourceShorted ? "Source short detected. " : "") +
                               (levelManager.TargetShorted ? "Target short detected." : ""),
                    action = "Inspect wiring that bypasses components. Do not assume a specific wire is faulty until verified."
                });
            }

            foreach (SparkVerifiedElectricalReading r in frame.readings)
            {
                if (next.Count >= 12) break;
                SparkTeachingAnchor anchor;
                bool located = SparkTeachingAnchor.TryFind(r.anchorId, out anchor);
                Transform t = located ? anchor.transform : null;
                SparkResistor resistor = t != null ? t.GetComponentInParent<SparkResistor>() : null;
                if (resistor == null && t != null) resistor = t.GetComponentInChildren<SparkResistor>(true);
                SparkLED led = t != null ? t.GetComponentInParent<SparkLED>() : null;
                if (led == null && t != null) led = t.GetComponentInChildren<SparkLED>(true);
                SparkSwitch sparkSwitch = t != null ? t.GetComponentInParent<SparkSwitch>() : null;
                if (sparkSwitch == null && t != null) sparkSwitch = t.GetComponentInChildren<SparkSwitch>(true);
                SparkResistorBurn burn = resistor != null ? resistor.BurnSystem : null;
                // A shelf resistor does not represent the live workstation circuit.
                SparkShelfItem shelf = t != null ? t.GetComponentInParent<SparkShelfItem>() : null;
                if (shelf != null && (!shelf.IsAtWorkbench || shelf.CurrentSlot == null ||
                    shelf.CurrentSlot.Occupant != shelf)) continue;

                Topic? type = null;
                string title = null;
                string note = null;
                string action = null;
                SparkSchematicKind symbol = SparkSchematicKind.Generic;
                Color tint = new Color(.26f, .84f, 1f, 1f);
                string measurements = "Solver: " + Number(r.voltageVolts) + " V, " +
                    Number(r.currentAmps * 1000f) + " mA, " + Number(r.resistanceOhms) + " Ω.";

                if (resistor != null && (resistor.IsBurned || (burn != null && burn.IsFailed)))
                {
                    type = Topic.ResistorBurned; title = "BURNED RESISTOR";
                    symbol = SparkSchematicKind.Resistor; tint = new Color(1f, .33f, .27f, 1f);
                    note = "Runtime resistor reports burned/failed. " + measurements;
                    action = "Inspect the resistor's failure mode, then replace or repair it through gameplay.";
                }
                else if (resistor != null && burn != null &&
                         (burn.State == SparkResistorBurn.BurnState.Hot || burn.State == SparkResistorBurn.BurnState.Smoking))
                {
                    type = Topic.ResistorHeating; title = burn.State == SparkResistorBurn.BurnState.Smoking
                        ? "RESISTOR SMOKING" : "RESISTOR HEATING";
                    symbol = SparkSchematicKind.Resistor; tint = new Color(1f, .68f, .26f, 1f);
                    note = "Runtime burn state: " + burn.State + "; temperature " + Number(burn.TemperatureC) +
                        " °C; thermal power " + Number(burn.PowerWatts * 1000f) + " mW; rating " +
                        Number(burn.RatedWatts * 1000f) + " mW. " + measurements;
                    action = "Check dissipated power against rated power. Reduce loading if the level permits.";
                }
                else if (resistor != null && r.condition == SparkElectricalCondition.OverCurrent)
                {
                    type = Topic.ResistorOvercurrent; title = "RESISTOR OVERCURRENT";
                    symbol = SparkSchematicKind.Resistor; tint = new Color(1f, .67f, .24f, 1f);
                    note = "Solver classified overcurrent. " + measurements + " Heat has NOT been verified from this reading.";
                    action = "Inspect power dissipation and resistor rating; overcurrent alone does not establish burning.";
                }
                else if (led != null && r.condition == SparkElectricalCondition.ReversePolarity)
                {
                    type = Topic.LEDReversePolarity; title = "LED REVERSE POLARITY";
                    symbol = SparkSchematicKind.LED; tint = new Color(1f, .67f, .27f, 1f);
                    note = "Solver classified reverse polarity for this LED. " + measurements;
                    action = "Inspect the verified LED anode/cathode connections. Correct orientation in gameplay.";
                }
                else if (sparkSwitch != null && !sparkSwitch.IsConducting && r.condition == SparkElectricalCondition.OpenCircuit)
                {
                    type = Topic.SwitchNotConducting; title = "SWITCH NOT CONDUCTING";
                    symbol = SparkSchematicKind.Switch; tint = new Color(1f, .65f, .26f, 1f);
                    note = "Solver classified open circuit and runtime switch reports non-conduction. " + measurements;
                    action = "Check switch state and verified terminal connections before changing it.";
                }
                else if (includeStandaloneComponentFailures && r.condition == SparkElectricalCondition.Failed)
                {
                    type = Topic.GenericFailure; title = "VERIFIED COMPONENT FAILURE";
                    tint = new Color(1f, .35f, .28f, 1f);
                    note = "Solver classified component failure. " + measurements;
                    action = "Inspect the reported failure mode and verify replacement requirements.";
                }
                else if (includeStandaloneComponentFailures && r.condition == SparkElectricalCondition.OpenCircuit)
                {
                    type = Topic.GenericOpen; title = "VERIFIED OPEN CIRCUIT";
                    tint = new Color(1f, .72f, .28f, 1f);
                    note = "Solver classified an open circuit for this reading. " + measurements;
                    action = "Inspect verified open-path diagnostics, switches and terminal connections.";
                }
                else if (includeStandaloneComponentFailures && r.condition == SparkElectricalCondition.ReversePolarity)
                {
                    type = Topic.GenericPolarity; title = "VERIFIED REVERSE POLARITY";
                    tint = new Color(1f, .72f, .28f, 1f);
                    note = "Solver classified reverse polarity for this component. " + measurements;
                    action = "Confirm component polarity and terminal assignments against the level requirements.";
                }

                if (!type.HasValue) continue;
                string caseKey = type.Value + ":" + r.anchorId;
                if (!seen.Add(caseKey)) continue;
                if (!string.IsNullOrWhiteSpace(r.explanation))
                    note += " Solver note: " + Trim(r.explanation, 140);
                next.Add(new Case { topic = type.Value, anchorId = r.anchorId, sourceKey = caseKey,
                    headline = title, evidence = note, action = action, symbol = symbol, tint = tint });
            }

            // Explicit fault metadata may target a component not otherwise listed.
            if (frame.hasVerifiedFault && !string.IsNullOrWhiteSpace(frame.verifiedFaultExplanation) &&
                !next.Exists(c => c.anchorId == frame.faultAnchorId) && next.Count < 12)
            {
                next.Add(new Case { topic = Topic.GenericFailure, anchorId = frame.faultAnchorId,
                    sourceKey = "explicit:" + frame.faultAnchorId, headline = "VERIFIED DIAGNOSTIC FAULT",
                    tint = new Color(1f, .37f, .3f, 1f), symbol = SparkSchematicKind.Generic,
                    evidence = "Solver fault: " + Trim(frame.verifiedFaultExplanation, 230),
                    action = "Follow the specific verified fault report before modifying the circuit." });
            }

            cases.Clear();
            cases.AddRange(next);
            selectedCase = 0;
            for (int i = 0; i < cases.Count; i++)
                if (cases[i].sourceKey == previous) { selectedCase = i; break; }

            string nextKey = SelectedKey();
            // Preserve playback only while the selected diagnosis AND its evidence remain stable.
            string fingerprint = nextKey + "|" + (cases.Count > 0 ? cases[selectedCase].evidence : "");
            bool changed = !string.Equals(currentKey, fingerprint, StringComparison.Ordinal);
            if (changed)
            {
                currentKey = fingerprint;
                bool wasPlaying = playback != null;
                StopLesson();
                ClearOwnAnnotations();
                stale = wasPlaying;
                selectedStep = 0;
                MakeSteps();
                statusOverride = stale ? "Verified state changed. Press PLAY to review the updated lesson." : null;
                if (openWhenNewFault && cases.Count > 0 && card != null) card.SetActive(true);
            }
            Refresh();
        }

        private string SelectedKey() => cases.Count > 0 ? cases[selectedCase].sourceKey : null;
        private static string Trim(string value, int limit)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            value = value.Trim().Replace('\n', ' ').Replace('\r', ' ');
            return value.Length > limit ? value.Substring(0, limit) + "…" : value;
        }

        private void MakeSteps()
        {
            steps.Clear();
            if (cases.Count == 0) return;
            Case c = cases[selectedCase];
            steps.Add(new Step { title = "1  VERIFIED CONDITION", body = c.evidence,
                worldLabel = c.headline });
            string concept;
            switch (c.topic)
            {
                case Topic.ResistorBurned:
                    concept = "Excess dissipated power can heat a resistor. This resistor's runtime state confirms failure; its specific electrical failure mode must be checked separately.";
                    break;
                case Topic.ResistorHeating:
                    concept = "A resistor converts electrical power to heat. Current, resistance, cooling, and power rating affect its thermal behavior; temperature and burn state shown here come from the runtime burn system.";
                    break;
                case Topic.ResistorOvercurrent:
                    concept = "Current above the solver's permitted level can threaten components. This is a current diagnosis, not a claim that the resistor is already hot or burned.";
                    break;
                case Topic.LEDReversePolarity:
                    concept = "An LED has an anode and cathode. Correct orientation is required for normal forward conduction; the solver has specifically classified reverse polarity on this LED.";
                    break;
                case Topic.SwitchNotConducting:
                    concept = "An open or non-conducting switch interrupts the path through that switch. The solver also reported an open condition, but this does not locate any other open wires.";
                    break;
                case Topic.ShortCircuit:
                    concept = "A short circuit provides an unintended low-resistance bypass. The level evaluator reports a short; it has not supplied the exact wire location here.";
                    break;
                default:
                    concept = "The electrical solver reported this condition. The displayed component and measurements come from the completed solver frame, not from assistant-generated text.";
                    break;
            }
            steps.Add(new Step { title = "2  HOW IT WORKS", body = concept,
                worldLabel = c.headline });
            steps.Add(new Step { title = "3  WHAT TO INSPECT", body = c.action,
                worldLabel = c.headline });
            bool route = snapshot != null && snapshot.hasVerifiedRoute && snapshot.routeEnergized &&
                         snapshot.orderedRouteAnchors != null && snapshot.orderedRouteAnchors.Length >= 2;
            steps.Add(new Step { title = "4  CHECK THE CIRCUIT", body = route
                ? "A verified conventional-current route is available. Select ROUTE BOARD to inspect its conceptual order. It is not a complete wiring schematic."
                : "No energized conventional-current route has been verified in this frame. The lesson will not draw an invented current path.",
                worldLabel = "USE VERIFIED EVIDENCE" });
        }

        public void Show() { if (card != null) card.SetActive(true); Refresh(); }
        public void Hide() { if (card != null) card.SetActive(false); StopLesson(); ClearOwnAnnotations(); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        public void NextCase() => SelectCase(selectedCase + 1);
        public void PreviousCase() => SelectCase(selectedCase - 1);

        private void SelectCase(int index)
        {
            if (cases.Count == 0) return;
            StopLesson(); ClearOwnAnnotations();
            selectedCase = (index + cases.Count) % cases.Count;
            selectedStep = 0;
            stale = false;
            currentKey = SelectedKey() + "|" + cases[selectedCase].evidence;
            MakeSteps();
            statusOverride = null;
            Refresh();
        }

        public void NextStep()
        {
            if (steps.Count == 0) return;
            StopLesson();
            selectedStep = (selectedStep + 1) % steps.Count;
            RenderStep();
        }

        public void PreviousStep()
        {
            if (steps.Count == 0) return;
            StopLesson();
            selectedStep = (selectedStep + steps.Count - 1) % steps.Count;
            RenderStep();
        }

        public void Play()
        {
            if (steps.Count == 0) return;
            StopLesson();
            stale = false; paused = false; statusOverride = null;
            playback = StartCoroutine(PlayRoutine());
        }

        public void PauseOrResume()
        {
            if (playback == null) return;
            paused = !paused;
            Refresh();
        }

        private IEnumerator PlayRoutine()
        {
            for (int i = selectedStep; i < steps.Count; i++)
            {
                selectedStep = i;
                RenderStep();
                float elapsed = 0f;
                while (elapsed < secondsPerStep)
                {
                    yield return null;
                    if (!paused) elapsed += Time.unscaledDeltaTime;
                }
            }
            playback = null;
            Refresh();
        }

        private void StopLesson()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null;
            paused = false;
        }

        private void RenderStep()
        {
            ClearOwnAnnotations();
            if (steps.Count == 0 || cases.Count == 0) { Refresh(); return; }
            Case c = cases[selectedCase];
            if (worldRenderer != null && ValidId(c.anchorId) && SparkTeachingAnchor.TryFind(c.anchorId, out _))
            {
                var ring = new SparkVisualCommand {
                    shape = SparkVisualShape.Ring,
                    from = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = c.anchorId },
                    to = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = c.anchorId },
                    normalizedSize = new Vector2(.11f, .13f), color = c.tint,
                    thickness = 4f, drawSeconds = .3f, holdSeconds = 0f, persistent = true
                };
                AddOwn(ring);
                var label = new SparkVisualCommand {
                    shape = SparkVisualShape.Label,
                    from = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = c.anchorId,
                        pixelOffset = new Vector2(0f, 85f) },
                    to = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = c.anchorId },
                    color = c.tint, fontSize = 19, text = steps[selectedStep].worldLabel,
                    thickness = 3f, drawSeconds = .3f, holdSeconds = 0f
                };
                AddOwn(label);
            }
            if (ownAnnotations.Count > 0) drawAnimation = StartCoroutine(AnimateOwnAnnotations());
            Refresh();
        }

        private IEnumerator AnimateOwnAnnotations()
        {
            float t = 0f;
            while (t < .45f)
            {
                t += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(t / .45f);
                foreach (SparkWorldAnnotationRenderer.Annotation a in ownAnnotations)
                    if (worldRenderer != null) worldRenderer.SetProgress(a, progress);
                yield return null;
            }
            drawAnimation = null;
        }

        private void AddOwn(SparkVisualCommand command)
        {
            if (worldRenderer == null) return;
            if (!SparkTeachingValidator.Validate(command, out _)) return;
            SparkWorldAnnotationRenderer.Annotation annotation = worldRenderer.Add(command);
            if (annotation != null)
            {
                ownAnnotations.Add(annotation);
                worldRenderer.SetProgress(annotation, 0f);
            }
        }

        private void ClearOwnAnnotations()
        {
            if (drawAnimation != null) { StopCoroutine(drawAnimation); drawAnimation = null; }
            if (worldRenderer != null)
                foreach (SparkWorldAnnotationRenderer.Annotation annotation in ownAnnotations)
                    worldRenderer.Remove(annotation);
            ownAnnotations.Clear();
        }

        public void Show3D()
        {
            if (cases.Count == 0) return;
            Resolve();
            // Explicit opt-in to showing annotations; does not replace the teacher lesson.
            if (teacher != null) { teacher.ViewBoth(); teacher.OpenPanel(); }
            RenderStep();
        }

        /// <summary>
        /// Explicit player action. Changes the shared teaching whiteboard (and replaces its
        /// currently displayed lesson). Never automatically called on incoming solver frames.
        /// This is a conceptual symbol, not an inferred wiring topology.
        /// </summary>
        public void ShowBoard()
        {
            if (cases.Count == 0 || teacher == null || snapshot == null) return;
            Case c = cases[selectedCase];
            List<SparkSchematicPart> parts = new List<SparkSchematicPart>();
            List<SparkSchematicWire> wires = new List<SparkSchematicWire>();
            bool route = snapshot.hasVerifiedRoute && snapshot.routeEnergized &&
                snapshot.orderedRouteAnchors != null && snapshot.orderedRouteAnchors.Length >= 2 &&
                snapshot.orderedRouteAnchors.Length <= 8;

            if (route)
            {
                string[] ids = snapshot.orderedRouteAnchors;
                for (int i = 0; i < ids.Length; i++)
                {
                    Vector2 p = new Vector2(.1f + .8f * i / (ids.Length - 1f), .5f);
                    parts.Add(new SparkSchematicPart { id = ids[i], label = Trim(ids[i], 36),
                        position = p, kind = ids[i] == c.anchorId ? c.symbol : SparkSchematicKind.Generic,
                        color = ids[i] == c.anchorId ? c.tint : new Color(.28f, .85f, 1f, 1f) });
                    if (i > 0)
                    {
                        Vector2 previous = new Vector2(.1f + .8f * (i - 1f) / (ids.Length - 1f), .5f);
                        wires.Add(new SparkSchematicWire { points = new[] { previous, p }, energized = true,
                            color = new Color(1f, .73f, .27f, 1f) });
                    }
                }
            }
            else
            {
                parts.Add(new SparkSchematicPart { id = "lesson-symbol", label = Trim(c.headline, 36),
                    kind = c.symbol, position = new Vector2(.5f, .5f), color = c.tint });
            }

            var plan = new SparkVisualTeachingPlan {
                title = Trim(c.headline, 90),
                explanation = route
                    ? "Solver-verified conventional-current order, drawn conceptually; straight lines are NOT physical wire geometry."
                    : "Single conceptual component symbol only; no physical connection or current path is asserted.",
                annotations = Array.Empty<SparkVisualCommand>(),
                schematic = new SparkSchematicData {
                    caption = route ? "VERIFIED ORDER · NOT FULL WIRING" : "COMPONENT CONCEPT · NO VERIFIED ROUTE",
                    parts = parts.ToArray(), wires = wires.ToArray()
                }
            };
            if (SparkTeachingValidator.Validate(plan, out string reason))
            {
                StopLesson();
                ClearOwnAnnotations();
                if (teacher.PresentValidatedPlan(plan))
                    statusOverride = "Board opened. The previous AAA whiteboard lesson was replaced by this view.";
                else
                    statusOverride = "The teaching manager declined the validated plan.";
            }
            else
            {
                statusOverride = "Board plan rejected: " + reason;
            }
            Refresh();
        }

        /// <summary>Call from the level loader when restarting the SAME circuit ID.</summary>
        public void ResetForLevel()
        {
            StopLesson(); ClearOwnAnnotations();
            pending = null; snapshot = null; cases.Clear(); steps.Clear();
            selectedCase = 0; selectedStep = 0; lastRevision = 0;
            circuitId = null; currentKey = null; stale = false;
            statusOverride = "Level reset; waiting for fresh verified solver data.";
            Refresh();
        }

        private void Refresh()
        {
            if (cases.Count == 0)
            {
                Set(headlineText, "AWAITING VERIFIED COMPONENT CONDITIONS");
                Set(evidenceText, "No resistor/LED/switch fault lesson in the latest valid frame.");
                Set(stepTitleText, "NO LESSON SELECTED");
                Set(stepBodyText, "Publish a completed solver frame to prepare component-specific teaching.");
                Set(stepCounterText, "0 / 0");
            }
            else
            {
                Case c = cases[selectedCase];
                Set(headlineText, c.headline + "  ·  " + (selectedCase + 1) + "/" + cases.Count);
                Set(evidenceText, Trim(c.evidence, 400));
                Step step = selectedStep < steps.Count ? steps[selectedStep] : null;
                Set(stepTitleText, step != null ? step.title : "READY");
                Set(stepBodyText, step != null ? step.body : c.action);
                Set(stepCounterText, steps.Count > 0 ? (selectedStep + 1) + " / " + steps.Count : "0 / 0");
            }
            Set(statusText, statusOverride ?? (playback != null ? paused ? "PAUSED" : "PLAYING VERIFIED LESSON" :
                stale ? "STATE CHANGED · REPLAY WITH NEW EVIDENCE" : "READY · PLAYER-TRIGGERED"));
            bool has = cases.Count > 0;
            if (previousCaseButton != null) previousCaseButton.interactable = has && cases.Count > 1;
            if (nextCaseButton != null) nextCaseButton.interactable = has && cases.Count > 1;
            if (previousStepButton != null) previousStepButton.interactable = has;
            if (nextStepButton != null) nextStepButton.interactable = has;
            if (playButton != null) playButton.interactable = has;
            if (pauseButton != null) pauseButton.interactable = playback != null;
            if (worldButton != null) worldButton.interactable = has && worldRenderer != null &&
                ValidId(cases[selectedCase].anchorId) && SparkTeachingAnchor.TryFind(cases[selectedCase].anchorId, out _);
            if (boardButton != null) boardButton.interactable = has && teacher != null;
        }

        private static void Set(TMP_Text label, string value)
        {
            if (label != null) label.text = value ?? "";
        }
    }
}
