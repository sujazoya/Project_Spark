using System;
using System.Collections.Generic;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only learning coach. Picks practices using configured level targets,
    /// training achievement keys, and observed pending sessions. It never writes
    /// to the level evaluator, electrical solver, training awards, or economy.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearningCoach : MonoBehaviour
    {
        private enum SessionPhase { Idle, Studying, Practicing, WaitingForEvaluation, Verified }

        private sealed class Practice
        {
            public SparkLevelTarget target;
            public string id;
            public string title;
            public int tier;
            public int pendingSessions;
            public int verifiedSessions;
            public bool trainingAwarded;
            public bool? evaluatorPassed;
            public int priority;
        }

        [Serializable] private sealed class HistoryEntry
        {
            public string levelId;
            public string targetId;
            public int observedVerifiedPractices;
            public string lastVerifiedUtc;
        }
        [Serializable] private sealed class HistoryStore
        {
            public int schema = 1;
            public List<HistoryEntry> entries = new List<HistoryEntry>();
        }
        // Mirrors the observation journal written by SparkAITrainingAnalytics.
        [Serializable] private sealed class AnalyticsEntry
        {
            public string levelId;
            public string targetId;
            public int observedPendingSessions;
            public int observedPassingSessions;
        }
        [Serializable] private sealed class AnalyticsStore
        {
            public int schema;
            public List<AnalyticsEntry> records;
        }

        private const string HistoryKey = "ProjectSpark.LearningCoach.v1";
        private const string AnalyticsKey = "ProjectSpark.TrainingAnalytics.Observations.v1";
        private const int MaxHistory = 250;

        [Header("Existing systems (read-only)")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkAIInteractiveTrainingChallenges training;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private SparkTutorialDirector tutorial;

        [Header("Generated panel")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text recommendationText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text studyText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button studiedButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button verifyButton;

        [Header("Verification")]
        [Tooltip("When enabled, mastery additionally requires a new verified solver frame after Start Practice.")]
        [SerializeField] private bool requireFreshSolverFrame = true;
        [SerializeField, Min(1f)] private float maxFrameAgeSeconds = 20f;
        [Tooltip("Number of recommended targets in the plan, including completed targets only when fewer pending targets exist.")]
        [SerializeField, Range(1, 8)] private int planLength = 4;

        private readonly List<Practice> plan = new List<Practice>();
        private HistoryStore history;
        private AnalyticsStore analytics;
        private LevelGamePlayManager subscribedLevel;
        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private string observedLevelId;
        private string sessionLevelId;
        private string sessionTargetId;
        private string sessionCircuitId;
        private string observedCircuitId;
        private int selection;
        private int evaluatorSequence;
        private int frameSequence;
        private int startedEvaluatorSequence;
        private int startedFrameSequence;
        private float lastFrameTime = -10000f;
        private bool studied;
        private bool levelFailed;
        private SessionPhase phase;
        private int hintStage;
        private string statusMessage = "Choose a recommended objective to begin.";

        public int RecommendedCount => plan.Count;
        public bool HasActivePractice => !string.IsNullOrEmpty(sessionTargetId);

        private void Awake()
        {
            LoadHistory();
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable()
        {
            if (history == null) LoadHistory();
            Resolve();
            Subscribe();
            RefreshPlan();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
            RefreshPlan();
        }

        private void OnDisable() { Unsubscribe(); SaveHistory(); }
        private void OnDestroy() { Unsubscribe(); }
        private void OnApplicationPause(bool paused) { if (paused) SaveHistory(); }

        private void Resolve()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (training == null)
                training = FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (tutorial == null)
                tutorial = FindFirstObjectByType<SparkTutorialDirector>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedLevel != levelManager)
            {
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted -= OnLevelReset;
                    subscribedLevel.LevelReset -= OnLevelReset;
                    subscribedLevel.LevelEvaluationChanged -= OnEvaluated;
                    subscribedLevel.LevelCompleted -= OnEvaluated;
                    subscribedLevel.LevelFailed -= OnFailed;
                }
                subscribedLevel = levelManager;
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted += OnLevelReset;
                    subscribedLevel.LevelReset += OnLevelReset;
                    subscribedLevel.LevelEvaluationChanged += OnEvaluated;
                    subscribedLevel.LevelCompleted += OnEvaluated;
                    subscribedLevel.LevelFailed += OnFailed;
                }
            }
            if (subscribedPublisher != publisher)
            {
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished -= OnFramePublished;
                subscribedPublisher = publisher;
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished += OnFramePublished;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedLevel != null)
            {
                subscribedLevel.LevelStarted -= OnLevelReset;
                subscribedLevel.LevelReset -= OnLevelReset;
                subscribedLevel.LevelEvaluationChanged -= OnEvaluated;
                subscribedLevel.LevelCompleted -= OnEvaluated;
                subscribedLevel.LevelFailed -= OnFailed;
                subscribedLevel = null;
            }
            if (subscribedPublisher != null)
            {
                subscribedPublisher.FramePublished -= OnFramePublished;
                subscribedPublisher = null;
            }
        }

        private void OnLevelReset(SparkLevelDefinition level)
        {
            CancelPracticeInternal("Level started or reset. Previous mastery check cancelled.");
            observedCircuitId = null;
            observedLevelId = null;
            levelFailed = false;
            RefreshPlan();
        }

        private void OnFailed(SparkLevelDefinition level, string reason)
        {
            levelFailed = true;
            CancelPracticeInternal("Level failed. Reset the level before starting another check.");
            RefreshPlan();
        }

        private void OnEvaluated(SparkLevelDefinition level)
        {
            evaluatorSequence++;
            if (levelManager == null || levelManager.ActiveLevel == null ||
                !string.Equals(observedLevelId, levelManager.ActiveLevel.LevelId, StringComparison.Ordinal))
            {
                CancelPracticeInternal("Level changed; mastery check cancelled.");
            }
            if (phase == SessionPhase.WaitingForEvaluation)
                TryVerify();
            RefreshPlan();
        }

        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null) return;
            string frameCircuit = frame.circuitId ?? string.Empty;
            if (HasActivePractice && sessionCircuitId != null &&
                !string.Equals(sessionCircuitId, frameCircuit, StringComparison.Ordinal))
                CancelPracticeInternal("Circuit identity changed. Start a new practice check.");
            observedCircuitId = frameCircuit;
            frameSequence++;
            lastFrameTime = Time.unscaledTime;
            if (phase == SessionPhase.WaitingForEvaluation) TryVerify();
            Render();
        }

        private static string CompositeKey(string levelId, string targetId)
        {
            string a = levelId ?? string.Empty;
            string b = targetId ?? string.Empty;
            return a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private void LoadHistory()
        {
            history = null;
            try
            {
                string raw = PlayerPrefs.GetString(HistoryKey, string.Empty);
                if (!string.IsNullOrEmpty(raw)) history = JsonUtility.FromJson<HistoryStore>(raw);
            }
            catch (Exception ex) { Debug.LogWarning("[Spark Learning Coach] History parse: " + ex.Message, this); }
            if (history == null || history.schema != 1) history = new HistoryStore();
            if (history.entries == null) history.entries = new List<HistoryEntry>();
        }

        private void SaveHistory()
        {
            if (history == null) return;
            PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(history));
            PlayerPrefs.Save();
        }

        private void LoadAnalytics()
        {
            analytics = null;
            try
            {
                string raw = PlayerPrefs.GetString(AnalyticsKey, string.Empty);
                if (!string.IsNullOrEmpty(raw)) analytics = JsonUtility.FromJson<AnalyticsStore>(raw);
            }
            catch (Exception ex) { Debug.LogWarning("[Spark Learning Coach] Analytics parse: " + ex.Message, this); }
        }

        private static int Tier(SparkLevelTarget target)
        {
            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.LEDOn:
                case SparkLevelTarget.TargetType.ComponentConducting:
                case SparkLevelTarget.TargetType.ComponentPowered:
                case SparkLevelTarget.TargetType.VoltagePresent:
                case SparkLevelTarget.TargetType.TerminalPowered:
                    return 0;
                case SparkLevelTarget.TargetType.CurrentRange:
                case SparkLevelTarget.TargetType.VoltageRange:
                case SparkLevelTarget.TargetType.PowerRange:
                    return 1;
                default: return 2;
            }
        }

        private bool? OfficialResult(string targetId)
        {
            if (levelManager == null || levelManager.TargetResults == null) return null;
            foreach (var result in levelManager.TargetResults)
                if (string.Equals(result.targetId, targetId, StringComparison.Ordinal)) return result.satisfied;
            return null;
        }

        private static string Safe(string s, int limit = 90)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Unnamed objective";
            s = s.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return s.Length > limit ? s.Substring(0, limit) + "..." : s;
        }

        private int HistoricCount(string levelId, string targetId)
        {
            if (history == null || history.entries == null) return 0;
            foreach (HistoryEntry entry in history.entries)
                if (entry != null && entry.levelId == levelId && entry.targetId == targetId)
                    return entry.observedVerifiedPractices;
            return 0;
        }

        private int PendingSessions(string levelId, string targetId)
        {
            if (analytics == null || analytics.records == null) return 0;
            foreach (AnalyticsEntry entry in analytics.records)
                if (entry != null && entry.levelId == levelId && entry.targetId == targetId)
                    return Mathf.Max(0, entry.observedPendingSessions);
            return 0;
        }

        public void RefreshPlan()
        {
            Resolve();
            LoadAnalytics();
            string selectedId = Selected() != null ? Selected().id : null;
            string levelId = levelManager != null && levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId : null;
            if (HasActivePractice && !string.Equals(sessionLevelId, levelId, StringComparison.Ordinal))
                CancelPracticeInternal("Level changed. Previous practice check cancelled.");
            observedLevelId = levelId;
            plan.Clear();
            selection = 0;
            if (levelManager != null && levelManager.ActiveLevel != null &&
                levelManager.ActiveLevel.Targets != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
                {
                    if (target == null || string.IsNullOrWhiteSpace(target.TargetId) || !seen.Add(target.TargetId)) continue;
                    bool? passed = OfficialResult(target.TargetId);
                    int tier = Tier(target);
                    int pending = PendingSessions(levelId, target.TargetId);
                    bool awarded = PlayerPrefs.GetInt("ProjectSpark.Training.v1/" + CompositeKey(levelId, target.TargetId), 0) == 1;
                    int priority = (passed == false ? 100 : passed == true ? -100 : 20)
                        + Mathf.Min(5, pending) * 10 - tier * 4 - (awarded ? 25 : 0);
                    plan.Add(new Practice
                    {
                        target = target, id = target.TargetId,
                        title = Safe(string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetId : target.DisplayName),
                        tier = tier, pendingSessions = pending,
                        verifiedSessions = HistoricCount(levelId, target.TargetId),
                        trainingAwarded = awarded, evaluatorPassed = passed,
                        priority = priority
                    });
                }
                plan.Sort((a, b) =>
                {
                    int order = b.priority.CompareTo(a.priority);
                    if (order != 0) return order;
                    order = a.tier.CompareTo(b.tier);
                    return order != 0 ? order : string.CompareOrdinal(a.id, b.id);
                });
                if (plan.Count > planLength) plan.RemoveRange(planLength, plan.Count - planLength);
            }
            if (HasActivePractice)
            {
                for (int i = 0; i < plan.Count; i++)
                    if (plan[i].id == sessionTargetId) { selection = i; Render(); return; }
            }
            if (!string.IsNullOrEmpty(selectedId))
            {
                for (int i = 0; i < plan.Count; i++)
                    if (plan[i].id == selectedId) { selection = i; break; }
            }
            Render();
        }

        private Practice Selected() => selection >= 0 && selection < plan.Count ? plan[selection] : null;

        public void Previous() { if (HasActivePractice) CancelPracticeInternal("Practice cancelled when selection changed."); if (plan.Count > 0) selection = (selection - 1 + plan.Count) % plan.Count; Render(); }
        public void Next() { if (HasActivePractice) CancelPracticeInternal("Practice cancelled when selection changed."); if (plan.Count > 0) selection = (selection + 1) % plan.Count; Render(); }

        public void MarkStudied()
        {
            Practice p = Selected();
            if (p == null) { statusMessage = "Start a level with configured objectives."; Render(); return; }
            studied = true;
            hintStage = 0;
            if (!HasActivePractice) phase = SessionPhase.Studying;
            statusMessage = "Requirements reviewed. Press START PRACTICE to begin a fresh mastery check.";
            Render();
        }

        public void StartPractice()
        {
            Practice p = Selected();
            if (p == null) { statusMessage = "No configured objective available."; Render(); return; }
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.IsFailed || levelFailed)
            { statusMessage = "Start a valid, active level before practice."; Render(); return; }
            if (!studied) { statusMessage = "Read the objective and press STUDIED first."; Render(); return; }
            bool? status = OfficialResult(p.id);
            if (status != false)
            {
                statusMessage = status == true
                    ? "Evaluator already reports PASS. Choose a pending objective for a new mastery check."
                    : "Evaluator has not reported this objective. Run a level evaluation first.";
                Render(); return;
            }
            sessionLevelId = observedLevelId;
            sessionTargetId = p.id;
            sessionCircuitId = observedCircuitId;
            startedEvaluatorSequence = evaluatorSequence;
            startedFrameSequence = frameSequence;
            phase = SessionPhase.Practicing;
            statusMessage = "Practice using the normal circuit controls. Then press CHECK MASTERY.";
            Render();
        }

        public void CheckMastery()
        {
            if (!HasActivePractice || (phase != SessionPhase.Practicing && phase != SessionPhase.WaitingForEvaluation))
            { statusMessage = "Begin a pending practice objective first."; Render(); return; }
            phase = SessionPhase.WaitingForEvaluation;
            TryVerify();
            Render();
        }

        private void TryVerify()
        {
            if (phase != SessionPhase.WaitingForEvaluation || !HasActivePractice) return;
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.IsFailed || levelFailed ||
                !string.Equals(sessionLevelId, levelManager.ActiveLevel.LevelId, StringComparison.Ordinal))
            { CancelPracticeInternal("Level unavailable, failed, or changed. Mastery check cancelled."); return; }
            if (evaluatorSequence <= startedEvaluatorSequence)
            { statusMessage = "Waiting for an official level evaluation after practice began."; return; }
            if (requireFreshSolverFrame)
            {
                if (subscribedPublisher == null)
                { statusMessage = "Verified frame publisher missing. Connect it or explicitly disable the extra frame gate."; return; }
                if (frameSequence <= startedFrameSequence || Time.unscaledTime - lastFrameTime > maxFrameAgeSeconds)
                { statusMessage = "Waiting for a recent verified solver frame after practice began."; return; }
                if (sessionCircuitId != null && !string.Equals(sessionCircuitId, observedCircuitId, StringComparison.Ordinal))
                { CancelPracticeInternal("Circuit identity changed. Mastery check cancelled."); return; }
            }
            bool? result = OfficialResult(sessionTargetId);
            if (result != true)
            {
                statusMessage = result == false
                    ? "Evaluator still reports PENDING. Review hints and repair the circuit."
                    : "The evaluator has no result for this objective.";
                return;
            }
            phase = SessionPhase.Verified;
            statusMessage = "PRACTICE VERIFIED: This objective changed from PENDING to PASS after session start.";
            RecordVerifiedPractice(sessionLevelId, sessionTargetId);
            // Only the official training controller can write its achievement key.
        }

        private void RecordVerifiedPractice(string levelId, string targetId)
        {
            if (history == null) LoadHistory();
            HistoryEntry record = null;
            foreach (HistoryEntry entry in history.entries)
                if (entry != null && entry.levelId == levelId && entry.targetId == targetId) { record = entry; break; }
            if (record == null)
            {
                if (history.entries.Count >= MaxHistory) return;
                record = new HistoryEntry { levelId = levelId, targetId = targetId };
                history.entries.Add(record);
            }
            record.observedVerifiedPractices++;
            record.lastVerifiedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");
            SaveHistory();
        }

        private void CancelPracticeInternal(string explanation)
        {
            sessionLevelId = null;
            sessionTargetId = null;
            sessionCircuitId = null;
            phase = SessionPhase.Idle;
            studied = false;
            hintStage = 0;
            statusMessage = explanation;
        }
        public void CancelPractice() { CancelPracticeInternal("Current practice check cancelled."); Render(); }

        private string HintsFor(Practice p)
        {
            if (p == null) return "No selected objective.";
            string targetId = Safe(p.target.TargetComponentId, 70);
            switch (hintStage)
            {
                case 0:
                    return "1. Read the configured objective. 2. Inspect its real component or terminal. " +
                           "3. Practice with existing circuit controls. 4. Check the evaluator after a new solve.";
                case 1:
                    return "Check whether the evaluator's target binding resolves. Target binding ID: " +
                        (string.IsNullOrWhiteSpace(p.target.TargetComponentId) ? "Not configured" : targetId) +
                        ". Do not assume the remembered component is the objective's bound component.";
                case 2:
                    return "Use INSPECT and EXPLAIN CIRCUIT in Ask Spark. Compare solver readings with the configured " +
                        "requirements; never infer a pass from the displayed component alone.";
                default:
                    return "Open AI TRAINING for guided inspection. Your official evaluator must report PASS " +
                        "after this practice session begins; the learning coach cannot force completion.";
            }
        }

        public void NextHint() { hintStage = Mathf.Min(3, hintStage + 1); Render(); }

        public void AskSpark()
        {
            Practice p = Selected();
            Resolve();
            if (p == null || assistant == null) { statusMessage = "Select a level objective and connect SparkAIUnifiedAssistant."; Render(); return; }
            string question = "Help me practice the current level objective '" + p.title + "' (ID " + p.id +
                "). Explain its configured conditions using only the live level evaluator and solver facts. " +
                "Do not assume a component is bound from its name, and do not claim a pass unless the evaluator confirms one.";
            assistant.Ask(question);
            statusMessage = "Question sent to your existing Ask Spark assistant.";
            Render();
        }

        public void OpenTraining()
        {
            Resolve();
            if (training == null) { statusMessage = "Create the Interactive Training Challenges panel first."; Render(); return; }
            Hide();
            training.Show();
        }

        public void OpenTutorial()
        {
            Resolve();
            if (tutorial == null) { statusMessage = "SparkTutorialDirector not found."; Render(); return; }
            // Prefer the director's existing default tutorial. Do not restart active lessons.
            if (!tutorial.IsRunning && tutorial.CanStartGameplayTutorial()) tutorial.BeginDefault();
            statusMessage = tutorial.IsRunning ? "Existing tutorial is active." : "Tutorial unavailable in this level state.";
            Render();
        }

        private static string Requirements(SparkLevelTarget t)
        {
            var sb = new StringBuilder(320);
            if (!string.IsNullOrWhiteSpace(t.Description)) sb.AppendLine(Safe(t.Description, 220));
            sb.Append("Type: ").AppendLine(t.Type.ToString());
            switch (t.Type)
            {
                case SparkLevelTarget.TargetType.CurrentRange:
                    sb.Append("Current >= ").Append((t.MinimumCurrent * 1000f).ToString("F3")).Append(" mA");
                    if (t.UseMaximumCurrent) sb.Append("; <= ").Append((t.MaximumCurrent * 1000f).ToString("F3")).Append(" mA");
                    sb.AppendLine(); break;
                case SparkLevelTarget.TargetType.VoltageRange:
                    sb.Append("Voltage >= ").Append(t.MinimumVoltage.ToString("F3")).Append(" V");
                    if (t.UseMaximumVoltage) sb.Append("; <= ").Append(t.MaximumVoltage.ToString("F3")).Append(" V");
                    sb.AppendLine(); break;
                case SparkLevelTarget.TargetType.PowerRange:
                    sb.Append("Power >= ").Append((t.MinimumPower * 1000f).ToString("F3")).Append(" mW");
                    if (t.UseMaximumPower) sb.Append("; <= ").Append((t.MaximumPower * 1000f).ToString("F3")).Append(" mW");
                    sb.AppendLine(); break;
            }
            if (!string.IsNullOrWhiteSpace(t.TargetComponentId))
                sb.Append("Binding ID: ").AppendLine(Safe(t.TargetComponentId));
            if (t.RequireConduction) sb.AppendLine("Conduction required");
            if (t.RequireElectricalEnabled) sb.AppendLine("Electrical enabled required");
            if (t.Inverted) sb.AppendLine("Inverted evaluation enabled");
            if (!t.IsConfigured()) sb.AppendLine("WARNING: Objective configuration incomplete");
            return sb.ToString().TrimEnd();
        }

        private static string TierName(int tier) => tier == 0 ? "FOUNDATION" : tier == 1 ? "APPLIED" : "ADVANCED";

        private void Render()
        {
            Practice p = Selected();
            if (headerText != null) headerText.text = observedLevelId == null
                ? "NO ACTIVE LEVEL" : "LEVEL  /  " + Safe(levelManager.ActiveLevel.DisplayName, 60);
            if (progressText != null)
            {
                int earned = 0;
                foreach (Practice item in plan) if (item.trainingAwarded) earned++;
                progressText.text = "PRACTICE PLAN " + (p == null ? 0 : selection + 1) + "/" + plan.Count +
                    "  |  TRAINING ACHIEVEMENTS " + earned + "/" + plan.Count;
            }
            if (progressFill != null)
            {
                int earned = 0;
                foreach (Practice item in plan) if (item.trainingAwarded) earned++;
                progressFill.fillAmount = plan.Count == 0 ? 0f : (float)earned / plan.Count;
            }
            if (recommendationText != null)
            {
                recommendationText.text = p == null
                    ? "Start a level with configured objectives to generate a practice plan."
                    : "RECOMMENDED: " + p.title + "  /  " + TierName(p.tier) +
                      "\nObserved pending sessions: " + p.pendingSessions +
                      "  |  Coach-verified practice passes: " + p.verifiedSessions;
            }
            if (objectiveText != null) objectiveText.text = p == null ? "No objective loaded." : Requirements(p.target);
            if (studyText != null) studyText.text = p == null ? "Choose a level." : HintsFor(p);
            if (evidenceText != null)
            {
                string result = p == null ? "UNKNOWN" : p.evaluatorPassed == true ? "PASS" : p.evaluatorPassed == false ? "PENDING" : "NOT REPORTED";
                evidenceText.text = "EVALUATOR: " + result + "    |    PHASE: " + phase +
                    "\nTraining achievement: " + (p != null && p.trainingAwarded ? "Recorded" : "Not recorded");
            }
            if (statusText != null) statusText.text = statusMessage;
            if (studiedButton != null) studiedButton.interactable = p != null && !HasActivePractice;
            if (startButton != null) startButton.interactable = p != null && studied && !HasActivePractice && !levelFailed;
            if (verifyButton != null) verifyButton.interactable = HasActivePractice &&
                (phase == SessionPhase.Practicing || phase == SessionPhase.WaitingForEvaluation);
        }

        public void RefreshCoach() { RefreshPlan(); }
        public void Show() { RefreshPlan(); if (card != null) card.SetActive(true); }
        public void Hide() { if (card != null) card.SetActive(false); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }
    }
}
