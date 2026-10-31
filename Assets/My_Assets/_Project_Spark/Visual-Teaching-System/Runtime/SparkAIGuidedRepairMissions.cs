using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Player-driven, read-only fault-repair coach. Solver frames are evidence, and a
    /// later level evaluation is the gate for a verified repair. Never changes wires,
    /// switch states, component values, progression or the existing AAA lesson.
    /// All public callbacks must be invoked on Unity's main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIGuidedRepairMissions : MonoBehaviour
    {
        private enum Kind { Short, Failed, Heating, Polarity, OpenSwitch, OpenCircuit, OverCurrent }
        private enum Phase { Idle, Inspect, Repair, AwaitVerification, Completed }

        private sealed class Fault
        {
            public Kind kind;
            public string anchorId;
            public string key;
            public string title;
            public string evidence;
            public string repairHint;
        }

        [Header("Verified sources (existing)")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;
        [SerializeField] private SparkVisualTeachingManager teacher;

        [Header("Generated UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button beginButton;
        [SerializeField] private Button inspectedButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button verifyButton;
        [SerializeField] private Button highlightButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        [Header("Verification")]
        [Tooltip("A completed solve older than this cannot confirm a new repair.")]
        [SerializeField, Min(1f)] private float maxFrameAgeSeconds = 12f;
        [Tooltip("Require a level evaluator event AFTER the mission started.")]
        [SerializeField] private bool requirePostMissionEvaluation = true;

        private readonly List<Fault> candidates = new List<Fault>();
        private readonly List<SparkWorldAnnotationRenderer.Annotation> ownCues =
            new List<SparkWorldAnnotationRenderer.Annotation>();

        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private LevelGamePlayManager subscribedLevel;
        private SparkVerifiedElectricalFrame snapshot;
        private Fault mission;
        private Phase phase;
        private int selected;
        private int hintStage;
        private int frameSerial;
        private int startedFrameSerial;
        private int evaluationSerial;
        private int startedEvaluationSerial;
        private int lastRevision;
        private float lastFrameAt = -10000f;
        private string currentCircuitId;
        private string statusOverride;
        // These are evaluator observations at mission start, not inferred wiring facts.
        private bool beganWithOpenReturn;
        private bool beganWithWrongConnection;
        private bool beganWithOverload;

        public bool MissionActive => mission != null && phase != Phase.Completed;
        public bool MissionCompleted => mission != null && phase == Phase.Completed;
        public int AvailableFaultCount => candidates.Count;

        private void Awake()
        {
            Resolve();
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable() { Resolve(); Subscribe(); Refresh(); }
        private void Start() { Resolve(); Subscribe(); RebuildCandidates(); }
        private void OnDisable() { Unsubscribe(); ClearOwnCues(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (worldRenderer == null)
                worldRenderer = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
            if (teacher == null)
                teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedPublisher != publisher)
            {
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished -= OnFramePublished;
                subscribedPublisher = publisher;
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished += OnFramePublished;
            }
            if (subscribedLevel != levelManager)
            {
                UnsubscribeLevel();
                subscribedLevel = levelManager;
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted += OnLevelReset;
                    subscribedLevel.LevelReset += OnLevelReset;
                    subscribedLevel.LevelEvaluationChanged += OnEvaluationChanged;
                    subscribedLevel.LevelCompleted += OnEvaluationChanged;
                    subscribedLevel.LevelFailed += OnLevelFailed;
                }
            }
        }

        private void UnsubscribeLevel()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelReset;
            subscribedLevel.LevelReset -= OnLevelReset;
            subscribedLevel.LevelEvaluationChanged -= OnEvaluationChanged;
            subscribedLevel.LevelCompleted -= OnEvaluationChanged;
            subscribedLevel.LevelFailed -= OnLevelFailed;
            subscribedLevel = null;
        }

        private void Unsubscribe()
        {
            if (subscribedPublisher != null)
                subscribedPublisher.FramePublished -= OnFramePublished;
            subscribedPublisher = null;
            UnsubscribeLevel();
        }

        private void OnLevelReset(SparkLevelDefinition _)
        {
            // Clearing mission identity prevents a reset from being mistaken for repair.
            CancelAndClear("Level restarted. Waiting for new verified fault evidence.");
        }

        private void OnLevelFailed(SparkLevelDefinition _, string __)
        {
            statusOverride = "Level failed. Reset the level to attempt a new repair.";
            Refresh();
        }

        private void OnEvaluationChanged(SparkLevelDefinition _)
        {
            evaluationSerial++;
            RebuildCandidates();
            CheckRepair();
            Refresh();
        }

        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            SubmitVerifiedFrame(frame);
        }

        /// <summary>Alternative to Publisher: send complete frames from a trusted solver.</summary>
        public bool SubmitVerifiedFrame(SparkVerifiedElectricalFrame frame)
        {
            if (!ValidFrame(frame)) return false;
            string id = frame.circuitId ?? "";
            if (currentCircuitId != null && !string.Equals(currentCircuitId, id, StringComparison.Ordinal))
            {
                CancelAndClear("Circuit identity changed. Previous repair mission cancelled.");
            }
            currentCircuitId = id;
            if (frame.revision > 0 && frame.revision <= lastRevision) return false;
            if (frame.revision > 0) lastRevision = frame.revision;
            snapshot = frame.Copy();
            frameSerial++;
            lastFrameAt = Time.unscaledTime;
            RebuildCandidates();
            CheckRepair();
            Refresh();
            return true;
        }

        private static bool ValidFrame(SparkVerifiedElectricalFrame f)
        {
            if (f == null || f.readings == null || f.readings.Count > 32 || f.revision < 0 ||
                (f.circuitId != null && f.circuitId.Length > 80)) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in f.readings)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.anchorId) || r.anchorId.Length > 80 ||
                    !ids.Add(r.anchorId) || float.IsNaN(r.voltageVolts) || float.IsInfinity(r.voltageVolts) ||
                    float.IsNaN(r.currentAmps) || float.IsInfinity(r.currentAmps) ||
                    float.IsNaN(r.resistanceOhms) || float.IsInfinity(r.resistanceOhms) ||
                    r.resistanceOhms < 0 || !Enum.IsDefined(typeof(SparkElectricalCondition), r.condition))
                    return false;
            }
            return true;
        }

        private static string Brief(string input, int max = 170)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            input = input.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return input.Length > max ? input.Substring(0, max) + "..." : input;
        }

        private static T FindAtAnchor<T>(string anchorId) where T : Component
        {
            if (!SparkTeachingAnchor.TryFind(anchorId, out var anchor)) return null;
            T component = anchor.GetComponentInParent<T>();
            return component != null ? component : anchor.GetComponentInChildren<T>(true);
        }

        private void RebuildCandidates()
        {
            string key = candidates.Count > selected ? candidates[selected].key : null;
            candidates.Clear();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            if (levelManager != null && levelManager.ActiveLevel != null &&
                (levelManager.SourceShorted || levelManager.TargetShorted))
            {
                Add(new Fault {
                    kind = Kind.Short, key = "level:short", title = "VERIFIED SHORT CIRCUIT",
                    evidence = "Level evaluator reports a source and/or target short.",
                    repairHint = "Inspect wire paths that may bypass required components. Confirm endpoints before changing wires."
                }, seen);
            }

            if (snapshot != null)
            {
                foreach (var r in snapshot.readings)
                {
                    if (candidates.Count >= 14) break;
                    Kind? kind = null;
                    string title = null;
                    string hint = null;
                    SparkResistor resistor = FindAtAnchor<SparkResistor>(r.anchorId);
                    SparkLED led = FindAtAnchor<SparkLED>(r.anchorId);
                    SparkSwitch sparkSwitch = FindAtAnchor<SparkSwitch>(r.anchorId);
                    SparkResistorBurn burn = resistor != null ? resistor.BurnSystem : null;

                    if (r.condition == SparkElectricalCondition.Failed)
                    {
                        kind = Kind.Failed; title = "COMPONENT FAILURE";
                        hint = "Inspect the reported failure and replace or repair the affected component using gameplay controls.";
                    }
                    else if (burn != null && (burn.State == SparkResistorBurn.BurnState.Hot ||
                                               burn.State == SparkResistorBurn.BurnState.Smoking))
                    {
                        kind = Kind.Heating; title = "RESISTOR OVERHEATING";
                        hint = "Inspect real dissipated power versus the resistor rating. Reduce heat through supported circuit repairs.";
                    }
                    else if (r.condition == SparkElectricalCondition.ReversePolarity)
                    {
                        kind = Kind.Polarity; title = led != null ? "LED REVERSE POLARITY" : "REVERSE POLARITY";
                        hint = "Verify the component's actual terminal orientation and correct it through gameplay.";
                    }
                    else if (r.condition == SparkElectricalCondition.OpenCircuit && sparkSwitch != null &&
                             !sparkSwitch.IsConducting && levelManager != null && !levelManager.ClosedReturn)
                    {
                        kind = Kind.OpenSwitch; title = "OPEN SWITCH / RETURN PATH";
                        hint = "Inspect the switch state, terminals, and return wiring. An open switch may be intentional.";
                    }
                    else if (r.condition == SparkElectricalCondition.OpenCircuit)
                    {
                        kind = Kind.OpenCircuit; title = "VERIFIED OPEN CIRCUIT";
                        hint = "Follow the verified open-path evidence, checking switch states and actual terminal connections.";
                    }
                    else if (r.condition == SparkElectricalCondition.OverCurrent)
                    {
                        kind = Kind.OverCurrent; title = "VERIFIED OVERCURRENT";
                        hint = "Compare current and component ratings, then correct the load through supported gameplay tools.";
                    }
                    if (!kind.HasValue) continue;
                    Add(new Fault {
                        kind = kind.Value, anchorId = r.anchorId,
                        key = kind.Value + ":" + r.anchorId, title = title,
                        evidence = "Solver reports " + r.condition + " for '" + Brief(r.anchorId, 60) +
                            "'. V=" + r.voltageVolts.ToString("F3") + " V; I=" +
                            (r.currentAmps * 1000f).ToString("F3") + " mA. " + Brief(r.explanation),
                        repairHint = hint
                    }, seen);
                }
            }
            selected = 0;
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i].key == key) { selected = i; break; }
            Refresh();
        }

        private void Add(Fault fault, HashSet<string> seen)
        {
            if (seen.Add(fault.key)) candidates.Add(fault);
        }

        public void BeginMission()
        {
            if (levelManager == null || levelManager.ActiveLevel == null)
            {
                statusOverride = "Start a level before beginning a repair mission.";
                Refresh(); return;
            }
            if (candidates.Count == 0) { statusOverride = "No verified faults available."; Refresh(); return; }
            if (snapshot == null || Time.unscaledTime - lastFrameAt > maxFrameAgeSeconds)
            {
                statusOverride = "Waiting for a fresh completed solver frame.";
                Refresh(); return;
            }
            mission = candidates[selected];
            phase = Phase.Inspect;
            hintStage = 0;
            startedFrameSerial = frameSerial;
            startedEvaluationSerial = evaluationSerial;
            beganWithOpenReturn = !levelManager.ClosedReturn;
            beganWithWrongConnection = levelManager.WrongConnection;
            beganWithOverload = levelManager.Overloaded;
            statusOverride = null;
            ClearOwnCues();
            Refresh();
        }

        public void AcknowledgeInspection()
        {
            if (mission == null || phase != Phase.Inspect) return;
            phase = Phase.Repair;
            hintStage = 0;
            statusOverride = null;
            Refresh();
        }

        public void NextHint()
        {
            if (mission == null || phase == Phase.Completed) return;
            hintStage = Mathf.Min(3, hintStage + 1);
            Refresh();
        }

        public void VerifyRepair()
        {
            if (mission == null || phase == Phase.Inspect || phase == Phase.Completed)
            {
                statusOverride = "First begin a mission and acknowledge the inspection.";
                Refresh(); return;
            }
            phase = Phase.AwaitVerification;
            statusOverride = "Checking completed solver frame and official level reevaluation...";
            CheckRepair();
            Refresh();
        }

        private void CheckRepair()
        {
            if (mission == null || phase != Phase.AwaitVerification) return;
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.IsFailed)
            {
                statusOverride = "Level is unavailable or failed; cannot verify repair.";
                return;
            }
            if (frameSerial <= startedFrameSerial || snapshot == null ||
                Time.unscaledTime - lastFrameAt > maxFrameAgeSeconds)
            {
                statusOverride = "Waiting for a NEW, fresh solved frame after starting this mission.";
                return;
            }
            if (requirePostMissionEvaluation && evaluationSerial <= startedEvaluationSerial)
            {
                statusOverride = "Waiting for LevelEvaluationChanged after this mission started.";
                return;
            }

            if (mission.kind == Kind.Short)
            {
                if (levelManager.SourceShorted || levelManager.TargetShorted)
                {
                    statusOverride = "Evaluator still reports a short circuit. Repair not confirmed.";
                    return;
                }
                // A cleared short flag confirms the short is gone, not that the level is complete.
            }
            else
            {
                SparkVerifiedElectricalReading reading = FindReading(mission.anchorId);
                if (reading == null)
                {
                    statusOverride = "Original anchor is missing from the new frame. No repair claim possible.";
                    return;
                }
                if (reading.condition != SparkElectricalCondition.Normal)
                {
                    statusOverride = "Solver still reports " + reading.condition + ". Repair not confirmed.";
                    return;
                }
                if (mission.kind == Kind.Heating || mission.kind == Kind.Failed)
                {
                    SparkResistor resistor = FindAtAnchor<SparkResistor>(mission.anchorId);
                    if (resistor != null && resistor.IsBurned)
                    {
                        statusOverride = "Resistor is still burned. Repair not confirmed.";
                        return;
                    }
                    SparkResistorBurn burn = resistor != null ? resistor.BurnSystem : null;
                    if (burn != null && (burn.IsFailed || burn.State == SparkResistorBurn.BurnState.Hot ||
                                         burn.State == SparkResistorBurn.BurnState.Smoking))
                    {
                        statusOverride = "Thermal system still reports an active hazard.";
                        return;
                    }
                }
                if (mission.kind == Kind.Polarity && beganWithWrongConnection &&
                    levelManager.WrongConnection)
                {
                    statusOverride = "Evaluator still reports a connection fault.";
                    return;
                }
                if (mission.kind == Kind.OverCurrent && beganWithOverload &&
                    levelManager.Overloaded)
                {
                    statusOverride = "Evaluator still reports overload.";
                    return;
                }
                if (mission.kind == Kind.OpenCircuit && beganWithOpenReturn &&
                    !levelManager.ClosedReturn)
                {
                    statusOverride = "Evaluator still reports the return path open.";
                    return;
                }
                if (mission.kind == Kind.OpenSwitch)
                {
                    SparkSwitch sparkSwitch = FindAtAnchor<SparkSwitch>(mission.anchorId);
                    if (sparkSwitch != null && !sparkSwitch.IsConducting)
                    {
                        statusOverride = "Switch still reports non-conduction.";
                        return;
                    }
                    if (!levelManager.ClosedReturn)
                    {
                        statusOverride = "Evaluator still reports an open return path.";
                        return;
                    }
                }
            }
            phase = Phase.Completed;
            statusOverride = "REPAIR VERIFIED · Fresh solver evidence and evaluator update confirmed. " +
                "Other level objectives may still be pending.";
            ClearOwnCues();
        }

        private SparkVerifiedElectricalReading FindReading(string anchorId)
        {
            if (snapshot == null || snapshot.readings == null) return null;
            foreach (var r in snapshot.readings)
                if (r != null && string.Equals(r.anchorId, anchorId, StringComparison.Ordinal)) return r;
            return null;
        }

        public void FocusFault()
        {
            Fault fault = mission ?? (candidates.Count > 0 ? candidates[selected] : null);
            if (fault == null) return;
            if (string.IsNullOrWhiteSpace(fault.anchorId))
            {
                statusOverride = "Level short has no verified fault anchor; no wire is guessed.";
                Refresh(); return;
            }
            if (!SparkTeachingAnchor.TryFind(fault.anchorId, out _))
            {
                statusOverride = "No scene SparkTeachingAnchor: " + Brief(fault.anchorId, 65);
                Refresh(); return;
            }
            Resolve();
            ClearOwnCues();
            if (worldRenderer == null) return;
            var point = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = fault.anchorId };
            var ring = new SparkVisualCommand {
                shape = SparkVisualShape.Ring, from = point, to = point,
                normalizedSize = new Vector2(.11f, .14f),
                color = new Color(1f, .6f, .25f, 1f), thickness = 4f,
                drawSeconds = .35f, persistent = true
            };
            if (SparkTeachingValidator.Validate(ring, out _))
            {
                var annotation = worldRenderer.Add(ring);
                if (annotation != null) { ownCues.Add(annotation); worldRenderer.SetProgress(annotation, 1f); }
            }
            statusOverride = "Highlighted reported component. Repair using normal game controls.";
            Refresh();
        }

        private void ClearOwnCues()
        {
            if (worldRenderer != null)
                foreach (var cue in ownCues) if (cue != null) worldRenderer.Remove(cue);
            ownCues.Clear();
        }

        public void CancelMission()
        {
            mission = null;
            phase = Phase.Idle;
            statusOverride = "Mission cancelled. Circuit unchanged.";
            ClearOwnCues();
            Refresh();
        }

        private void CancelAndClear(string reason)
        {
            mission = null; phase = Phase.Idle; hintStage = 0;
            candidates.Clear(); selected = 0; snapshot = null;
            frameSerial = 0; evaluationSerial = 0; lastRevision = 0;
            startedFrameSerial = 0; startedEvaluationSerial = 0;
            currentCircuitId = null; lastFrameAt = -10000f;
            statusOverride = reason;
            ClearOwnCues();
            Refresh();
        }

        public void PreviousFault() { if (mission == null && candidates.Count > 0) { selected = (selected + candidates.Count - 1) % candidates.Count; Refresh(); } }
        public void NextFault() { if (mission == null && candidates.Count > 0) { selected = (selected + 1) % candidates.Count; Refresh(); } }
        public void Show() { if (panel != null) panel.SetActive(true); Refresh(); }
        public void Hide() { if (panel != null) panel.SetActive(false); ClearOwnCues(); }
        public void Toggle() { if (panel != null && panel.activeSelf) Hide(); else Show(); }

        private void Refresh()
        {
            Fault fault = mission ?? (candidates.Count > 0 ? candidates[selected] : null);
            if (titleText != null)
                titleText.text = fault != null ? fault.title : "AWAITING VERIFIED CIRCUIT FAULT";
            if (evidenceText != null)
                evidenceText.text = fault != null ? fault.evidence :
                    "Publish a completed solver frame. A short circuit can also be reported by the level evaluator.";
            if (stepText != null)
                stepText.text = mission == null ? "CHOOSE A REPAIR MISSION" :
                    phase == Phase.Inspect ? "STEP 1 / 3 · INSPECT" :
                    phase == Phase.Repair ? "STEP 2 / 3 · REPAIR" :
                    phase == Phase.AwaitVerification ? "STEP 3 / 3 · VERIFY" : "COMPLETE · VERIFIED";
            if (bodyText != null)
            {
                if (mission == null)
                    bodyText.text = fault == null ? "No fault currently reported." :
                        "Review the verified evidence, then press BEGIN to start this mission.";
                else if (phase == Phase.Inspect)
                    bodyText.text = "Inspect the reported component and measurements. " +
                        "Use HIGHLIGHT if a verified scene anchor exists. Press INSPECTED when ready.";
                else if (phase == Phase.Completed)
                    bodyText.text = "This fault was cleared in a new solver frame following a level evaluator update. " +
                        "Check remaining objectives; no automatic reward or level completion was applied.";
                else
                    bodyText.text = hintStage == 0 ?
                        "Use normal gameplay controls to correct the issue. Press VERIFY when you are ready. " +
                        "The mission waits for fresh solver results AND an evaluator update." :
                        hintStage == 1 ? mission.repairHint :
                        hintStage == 2 ? "The highlighted object is an observation target, not an automatically diagnosed wire. " +
                        "Check the official level objectives and valid connections." :
                        "A repair is confirmed only when the original fault clears from a fresh reading " +
                        "and the level evaluator has processed the changed circuit.";
            }
            if (statusText != null)
                statusText.text = !string.IsNullOrWhiteSpace(statusOverride) ? statusOverride :
                    mission != null ? "Read-only coach · No automatic circuit changes" :
                    "Available verified faults: " + candidates.Count;
            if (beginButton != null) beginButton.interactable = mission == null && candidates.Count > 0;
            if (inspectedButton != null) inspectedButton.interactable = phase == Phase.Inspect && mission != null;
            if (hintButton != null) hintButton.interactable = mission != null && phase != Phase.Completed;
            if (verifyButton != null) verifyButton.interactable = mission != null &&
                (phase == Phase.Repair || phase == Phase.AwaitVerification);
            if (highlightButton != null) highlightButton.interactable = fault != null && !string.IsNullOrWhiteSpace(fault.anchorId);
            if (cancelButton != null) cancelButton.interactable = mission != null;
            if (previousButton != null) previousButton.interactable = mission == null && candidates.Count > 1;
            if (nextButton != null) nextButton.interactable = mission == null && candidates.Count > 1;
        }
    }
}
