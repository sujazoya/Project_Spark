using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only experiment notebook for real, completed solver frames.
    /// The player changes the actual circuit with existing gameplay controls.
    /// This script never changes voltage, wires, components, money or objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkCircuitExperimentLab : MonoBehaviour
    {
        [Header("Verified sources")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField, Min(0.5f)] private float maxFrameAgeSeconds = 8f;
        [SerializeField, Range(1, 40)] private int retainedExperiments = 16;

        [Header("Generated UI (optional)")]
        [SerializeField] private GameObject window;
        [SerializeField] private TMP_Text sourceStatusText;
        [SerializeField] private TMP_Text experimentStatusText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text comparisonText;
        [SerializeField] private RectTransform comparisonContent;
        [SerializeField] private ScrollRect comparisonScroll;
        [SerializeField] private Button captureBeforeButton;
        [SerializeField] private Button captureAfterButton;
        [SerializeField] private Button exportButton;

        [Serializable]
        private sealed class TargetObservation
        {
            public string id;
            public bool passed;
        }

        [Serializable]
        private sealed class Capture
        {
            public string takenUtc;
            public string circuitId;
            public int frameRevision;
            public int frameSerial;
            public int evaluationSerial;
            public bool evaluatorCurrent;
            public int satisfied;
            public int total;
            public List<TargetObservation> targets = new List<TargetObservation>();
            public SparkVerifiedElectricalFrame frame;
        }

        [Serializable]
        private sealed class ComparisonRow
        {
            public string componentId;
            public string state;
            public float beforeVoltage;
            public float afterVoltage;
            public float beforeCurrent;
            public float afterCurrent;
            public float beforeResistance;
            public float afterResistance;
            public string beforeCondition;
            public string afterCondition;
            public bool matched;
        }

        [Serializable]
        private sealed class ExperimentRecord
        {
            public string capturedUtc;
            public string circuitId;
            public int beforeRevision;
            public int afterRevision;
            public bool afterEvaluatorCurrent;
            public int beforePassed;
            public int afterPassed;
            public int objectiveCount;
            public List<ComparisonRow> rows = new List<ComparisonRow>();
        }

        [Serializable]
        private sealed class ExperimentExport
        {
            public string format = "ProjectSpark.CircuitExperiment.v1";
            public string exportedUtc;
            public List<ExperimentRecord> experiments = new List<ExperimentRecord>();
        }

        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private LevelGamePlayManager subscribedLevel;
        private SparkVerifiedElectricalFrame latest;
        private Capture before;
        private ExperimentRecord result;
        private readonly List<ExperimentRecord> history = new List<ExperimentRecord>();
        private int frameSerial;
        private int evaluationSerial;
        private float lastFrameTime = -10000f;
        private float lastEvaluationTime = -10000f;
        private float nextResolveTime;
        private string currentCircuitId;
        private string message = "Capture a verified BEFORE frame, change the circuit, then capture AFTER.";

        public bool HasBaseline => before != null;
        public bool HasComparison => result != null;
        public int HistoryCount => history.Count;
        public string CurrentCircuitId => currentCircuitId;

        private void Awake()
        {
            Resolve();
            if (window != null) window.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            Refresh();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if ((publisher == null || levelManager == null) &&
                Time.unscaledTime >= nextResolveTime)
            {
                nextResolveTime = Time.unscaledTime + 2f;
                Resolve();
                Subscribe();
                Refresh();
            }
            // Only the source-status line changes with age; no per-frame layout rebuilds.
        }

        private void Resolve()
        {
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
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
                if (subscribedLevel != null) UnsubscribeLevel();
                subscribedLevel = levelManager;
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted += OnLevelRestarted;
                    subscribedLevel.LevelReset += OnLevelRestarted;
                    subscribedLevel.LevelEvaluationChanged += OnEvaluationChanged;
                }
            }
        }

        private void Unsubscribe()
        {
            if (subscribedPublisher != null)
                subscribedPublisher.FramePublished -= OnFramePublished;
            subscribedPublisher = null;
            UnsubscribeLevel();
        }

        private void UnsubscribeLevel()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelRestarted;
            subscribedLevel.LevelReset -= OnLevelRestarted;
            subscribedLevel.LevelEvaluationChanged -= OnEvaluationChanged;
            subscribedLevel = null;
        }

        private void OnLevelRestarted(SparkLevelDefinition unused)
        {
            // A new attempt cannot be compared to a previous attempt.
            ResetMeasurements("Level started or reset. Capture a new baseline.");
        }

        private void OnEvaluationChanged(SparkLevelDefinition unused)
        {
            ++evaluationSerial;
            lastEvaluationTime = Time.unscaledTime;
            // Evaluator observation is recorded; do not disturb a scrolling report.
        }

        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || !ValidateFrame(frame)) return;
            string id = frame.circuitId ?? string.Empty;
            if (latest != null && !string.Equals(id, currentCircuitId, StringComparison.Ordinal))
                ResetMeasurements("Circuit identity changed. Capture a new baseline.");

            if (latest != null && string.Equals(id, currentCircuitId, StringComparison.Ordinal) &&
                frame.revision > 0 && latest.revision > 0 && frame.revision <= latest.revision)
                return; // Old or duplicate revision.

            currentCircuitId = id;
            latest = frame.Copy();
            ++frameSerial;
            lastFrameTime = Time.unscaledTime;
            if (window != null && window.activeSelf && sourceStatusText != null)
                sourceStatusText.text = "VERIFIED SOURCE  /  " + latest.circuitId +
                    "  /  REV " + latest.revision.ToString(CultureInfo.InvariantCulture);
        }

        private static bool ValidateFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame.readings == null || frame.readings.Count > 32 || frame.revision < 0 ||
                (frame.circuitId != null && frame.circuitId.Length > 80)) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkVerifiedElectricalReading r in frame.readings)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.anchorId) || r.anchorId.Length > 80 ||
                    !ids.Add(r.anchorId) || !Finite(r.voltageVolts) ||
                    !Finite(r.currentAmps) || !Finite(r.resistanceOhms) ||
                    r.resistanceOhms < 0f || !Enum.IsDefined(typeof(SparkElectricalCondition), r.condition))
                    return false;
            }
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private bool HasFreshFrame(out string reason)
        {
            if (publisher == null)
            {
                reason = "No verified solver publisher found. Connect the existing publisher.";
                return false;
            }
            if (latest == null)
            {
                reason = "No solver frame received yet. Finish a circuit solve first.";
                return false;
            }
            if (Time.unscaledTime - lastFrameTime > maxFrameAgeSeconds)
            {
                reason = "Last solver frame is stale. Change or reevaluate the circuit.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(latest.circuitId))
            {
                reason = "Solver frame needs a stable circuitId to prevent cross-circuit comparison.";
                return false;
            }
            reason = null;
            return true;
        }

        private Capture TakeCapture()
        {
            var capture = new Capture
            {
                takenUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                circuitId = latest.circuitId,
                frameRevision = latest.revision,
                frameSerial = frameSerial,
                evaluationSerial = evaluationSerial,
                evaluatorCurrent = levelManager != null &&
                    lastEvaluationTime >= lastFrameTime && evaluationSerial > 0,
                frame = latest.Copy()
            };

            if (levelManager != null && levelManager.ActiveLevel != null)
            {
                capture.satisfied = levelManager.SatisfiedTargetCount;
                capture.total = levelManager.ActiveTargetCount;
                if (levelManager.TargetResults != null)
                    foreach (var target in levelManager.TargetResults)
                        capture.targets.Add(new TargetObservation
                        {
                            id = target.targetId,
                            passed = target.satisfied
                        });
            }
            return capture;
        }

        public void CaptureBefore()
        {
            if (!HasFreshFrame(out string error)) { SetMessage(error); return; }
            before = TakeCapture();
            result = null;
            SetMessage("BEFORE saved. Change a component or wire using normal gameplay, then run the solver and capture AFTER.");
        }

        public void CaptureAfter()
        {
            if (before == null) { SetMessage("Capture BEFORE first."); return; }
            if (!HasFreshFrame(out string error)) { SetMessage(error); return; }
            if (!string.Equals(before.circuitId, latest.circuitId, StringComparison.Ordinal))
            {
                ResetMeasurements("Circuit changed. BEFORE and AFTER must be from the same circuitId.");
                return;
            }
            if (frameSerial <= before.frameSerial ||
                (before.frameRevision > 0 && latest.revision > 0 &&
                 latest.revision <= before.frameRevision))
            {
                SetMessage("AFTER needs a NEW completed solver frame after BEFORE. No new reading has arrived.");
                return;
            }

            Capture after = TakeCapture();
            result = Compare(before, after);
            history.Add(result);
            while (history.Count > retainedExperiments) history.RemoveAt(0);
            string evaluated = result.afterEvaluatorCurrent
                ? "Evaluator refreshed after this solve."
                : "Evaluator not confirmed after this solve; objective delta is withheld.";
            SetMessage("Comparison recorded. " + evaluated);
        }

        private ExperimentRecord Compare(Capture baseline, Capture after)
        {
            bool evaluatorConfirmed = after.evaluatorCurrent &&
                after.evaluationSerial > baseline.evaluationSerial;
            var record = new ExperimentRecord
            {
                capturedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                circuitId = after.circuitId,
                beforeRevision = baseline.frameRevision,
                afterRevision = after.frameRevision,
                afterEvaluatorCurrent = evaluatorConfirmed && baseline.evaluatorCurrent,
                beforePassed = baseline.satisfied,
                afterPassed = after.satisfied,
                objectiveCount = after.total
            };
            var oldMap = new Dictionary<string, SparkVerifiedElectricalReading>(StringComparer.Ordinal);
            foreach (var reading in baseline.frame.readings) oldMap.Add(reading.anchorId, reading);
            var newMap = new Dictionary<string, SparkVerifiedElectricalReading>(StringComparer.Ordinal);
            foreach (var reading in after.frame.readings) newMap.Add(reading.anchorId, reading);
            var ids = new List<string>(oldMap.Keys);
            foreach (string id in newMap.Keys) if (!oldMap.ContainsKey(id)) ids.Add(id);
            ids.Sort(StringComparer.Ordinal);

            foreach (string id in ids)
            {
                bool had = oldMap.TryGetValue(id, out SparkVerifiedElectricalReading left);
                bool has = newMap.TryGetValue(id, out SparkVerifiedElectricalReading right);
                var row = new ComparisonRow { componentId = id, matched = had && has,
                    state = had && has ? "COMPARED" : had ? "REMOVED FROM REPORT" : "ADDED TO REPORT" };
                if (had)
                {
                    row.beforeVoltage = left.voltageVolts;
                    row.beforeCurrent = left.currentAmps;
                    row.beforeResistance = left.resistanceOhms;
                    row.beforeCondition = left.condition.ToString();
                }
                if (has)
                {
                    row.afterVoltage = right.voltageVolts;
                    row.afterCurrent = right.currentAmps;
                    row.afterResistance = right.resistanceOhms;
                    row.afterCondition = right.condition.ToString();
                }
                record.rows.Add(row);
            }
            return record;
        }

        public void ResetExperiment()
        {
            before = null;
            result = null;
            SetMessage("Comparison cleared. Previous results remain in session history.");
        }

        private void ResetMeasurements(string reason)
        {
            latest = null;
            before = null;
            result = null;
            lastFrameTime = -10000f;
            lastEvaluationTime = -10000f;
            currentCircuitId = null;
            frameSerial = 0;
            evaluationSerial = 0;
            SetMessage(reason);
        }

        public void Show()
        {
            if (window != null) window.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (window != null) window.SetActive(false);
        }

        public void Toggle()
        {
            if (window == null) return;
            if (window.activeSelf) Hide(); else Show();
        }

        private void SetMessage(string value)
        {
            message = value;
            Refresh();
        }

        private void Refresh()
        {
            if (sourceStatusText != null)
                sourceStatusText.text = latest == null
                    ? "VERIFIED SOURCE  /  NO FRAME"
                    : "VERIFIED SOURCE  /  " + latest.circuitId +
                      "  /  REV " + latest.revision.ToString(CultureInfo.InvariantCulture);

            if (experimentStatusText != null) experimentStatusText.text = message;
            if (captureBeforeButton != null) captureBeforeButton.interactable = latest != null;
            if (captureAfterButton != null) captureAfterButton.interactable = before != null && latest != null;
            if (exportButton != null) exportButton.interactable = history.Count > 0;
            if (summaryText != null)
                summaryText.text = "BEFORE  " + (before != null ? "RECORDED" : "NOT CAPTURED") +
                                   "    AFTER  " + (result != null ? "COMPARED" : "NOT CAPTURED") +
                                   "    HISTORY  " + history.Count.ToString(CultureInfo.InvariantCulture);
            if (comparisonText == null) return;

            var sb = new StringBuilder(3000);
            if (result != null)
            {
                sb.AppendLine("VERIFIED MEASUREMENT DIFFERENCES");
                sb.AppendLine("Circuit: " + result.circuitId +
                    "   Before rev: " + result.beforeRevision +
                    "   After rev: " + result.afterRevision);
                sb.AppendLine();
                foreach (ComparisonRow row in result.rows)
                {
                    sb.AppendLine("<color=#55E5F8>" + EscapeRichText(row.componentId) + "</color>  [" + row.state + "]");
                    if (row.matched)
                    {
                        sb.AppendLine(" Voltage   " + Format(row.beforeVoltage, " V") + "  →  " +
                            Format(row.afterVoltage, " V") + "    Δ " + Format(row.afterVoltage - row.beforeVoltage, " V"));
                        sb.AppendLine(" Current   " + Format(row.beforeCurrent * 1000f, " mA") + "  →  " +
                            Format(row.afterCurrent * 1000f, " mA") + "    Δ " +
                            Format((row.afterCurrent - row.beforeCurrent) * 1000f, " mA"));
                        sb.AppendLine(" Resistance   " + Format(row.beforeResistance, " Ω") + "  →  " +
                            Format(row.afterResistance, " Ω") + "    Δ " +
                            Format(row.afterResistance - row.beforeResistance, " Ω"));
                        if (row.beforeCondition != row.afterCondition)
                            sb.AppendLine(" Condition   " + row.beforeCondition + " → " + row.afterCondition);
                    }
                    else sb.AppendLine(" No fabricated delta: component missing from one report.");
                    sb.AppendLine();
                }
                if (result.afterEvaluatorCurrent)
                    sb.AppendLine("OFFICIAL OBJECTIVES  " + result.beforePassed + "/" + result.objectiveCount +
                        " → " + result.afterPassed + "/" + result.objectiveCount);
                else
                    sb.AppendLine("OFFICIAL OBJECTIVES  NOT COMPARED: evaluator has not been verified after this solve.");
                sb.AppendLine();
                sb.AppendLine("Numbers are solver snapshots. Differences are observations, not proof of which edit caused them.");
            }
            else if (before != null)
            {
                sb.AppendLine("BEFORE CAPTURED  /  REV " + before.frameRevision);
                sb.AppendLine("Circuit: " + EscapeRichText(before.circuitId));
                sb.AppendLine("Readings: " + before.frame.readings.Count);
                sb.AppendLine();
                sb.AppendLine("1. Change the circuit using existing gameplay controls.");
                sb.AppendLine("2. Wait for a new completed solver frame.");
                sb.AppendLine("3. Press CAPTURE AFTER to compare.");
            }
            else
            {
                sb.AppendLine("EXPERIMENT WORKFLOW");
                sb.AppendLine();
                sb.AppendLine("1. Run the circuit and press CAPTURE BEFORE.");
                sb.AppendLine("2. Change resistor value, switch state or wiring in gameplay.");
                sb.AppendLine("3. Run the solver again and press CAPTURE AFTER.");
                sb.AppendLine("4. Review actual voltage, current, and resistance differences.");
                sb.AppendLine("5. Export recorded experiments for study.");
                sb.AppendLine();
                sb.AppendLine("No solver connection? Connect your existing SparkVerifiedElectricalPublisher first.");
            }
            comparisonText.text = sb.ToString();
            if (comparisonContent != null)
            {
                // Rect is set by the Editor builder. Update only after content changes.
                float textWidth = Mathf.Max(120f, comparisonText.rectTransform.rect.width);
                float textHeight = Mathf.Max(290f,
                    comparisonText.GetPreferredValues(comparisonText.text, textWidth, Mathf.Infinity).y + 18f);
                comparisonText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
                comparisonContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight + 24f);
            }
            if (comparisonScroll != null)
                comparisonScroll.verticalNormalizedPosition = 1f;
        }

        private static string Format(float value, string unit) =>
            value.ToString("+0.###;-0.###;0", CultureInfo.InvariantCulture) + unit;

        private static string EscapeRichText(string source) =>
            (source ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;");

        public void ExportExperiments()
        {
            if (history.Count == 0) { SetMessage("No comparisons to export."); return; }
            try
            {
                string directory = Path.Combine(Application.persistentDataPath,
                    "ProjectSpark", "CircuitExperiments");
                Directory.CreateDirectory(directory);
                string stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                string jsonPath = Path.Combine(directory, "experiments_" + stamp + ".json");
                string csvPath = Path.Combine(directory, "experiments_" + stamp + ".csv");
                var payload = new ExperimentExport { exportedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) };
                payload.experiments.AddRange(history);
                File.WriteAllText(jsonPath, JsonUtility.ToJson(payload, true), Encoding.UTF8);
                var csv = new StringBuilder();
                csv.AppendLine("recordedUtc,circuitId,beforeRevision,afterRevision,componentId,state,beforeVoltageV,afterVoltageV,beforeCurrentA,afterCurrentA,beforeResistanceOhms,afterResistanceOhms,beforeCondition,afterCondition,evaluatorConfirmed");
                foreach (ExperimentRecord experiment in history)
                    foreach (ComparisonRow row in experiment.rows)
                    {
                        csv.Append(Csv(experiment.capturedUtc)).Append(',').Append(Csv(experiment.circuitId)).Append(',')
                           .Append(experiment.beforeRevision).Append(',').Append(experiment.afterRevision).Append(',')
                           .Append(Csv(row.componentId)).Append(',').Append(Csv(row.state)).Append(',')
                           .Append(row.matched ? Numeric(row.beforeVoltage) : "").Append(',')
                           .Append(row.matched ? Numeric(row.afterVoltage) : "").Append(',')
                           .Append(row.matched ? Numeric(row.beforeCurrent) : "").Append(',')
                           .Append(row.matched ? Numeric(row.afterCurrent) : "").Append(',')
                           .Append(row.matched ? Numeric(row.beforeResistance) : "").Append(',')
                           .Append(row.matched ? Numeric(row.afterResistance) : "").Append(',')
                           .Append(Csv(row.beforeCondition)).Append(',').Append(Csv(row.afterCondition)).Append(',')
                           .AppendLine(experiment.afterEvaluatorCurrent ? "true" : "false");
                    }
                File.WriteAllText(csvPath, csv.ToString(), Encoding.UTF8);
                Debug.Log("[Circuit Experiment Lab] Exported:\n" + jsonPath + "\n" + csvPath, this);
                SetMessage("Exported " + history.Count + " experiments. File paths printed in Unity Console.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                SetMessage("Export failed: " + exception.Message);
            }
        }

        private static string Numeric(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            // Avoid spreadsheet formula injection in exported, potentially solver-supplied labels.
            if (safe.Length > 0 && (safe[0] == '=' || safe[0] == '+' || safe[0] == '-' || safe[0] == '@' || safe[0] == '\t'))
                safe = "'" + safe;
            return "\"" + safe.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
        }
    }
}
