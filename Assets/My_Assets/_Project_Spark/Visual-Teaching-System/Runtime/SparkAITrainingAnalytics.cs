using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only Project Spark training analytics. The legacy dashboard is the source for
    /// persisted achievements; LevelGamePlayManager is the source for objective evaluation.
    /// A separate, bounded journal records observed pending/passing states per level session.
    /// The journal is NOT a diagnosis, solver, challenge-completion or reward system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITrainingAnalytics : MonoBehaviour
    {
        [Serializable] private sealed class DashboardObjective
        {
            public string levelId, levelTitle, targetId, targetTitle;
            public int tier;
            public bool completed;
        }

        [Serializable] private sealed class DashboardEvent
        {
            public string levelId, targetId, title, observedUtc;
            public bool imported;
        }

        // Mirrors ProjectSpark.TrainingDashboard.v1; does not modify or resave that key.
        [Serializable] private sealed class DashboardStore
        {
            public int schema;
            public List<DashboardObjective> objectives;
            public List<DashboardEvent> history;
        }

        [Serializable] private sealed class AttemptRecord
        {
            public string levelId, targetId, displayName;
            public int tier;
            public int observedPendingSessions;
            public int observedPassingSessions;
            public int pendingToPassTransitions;
            public string lastObservedUtc, lastState;
        }

        [Serializable] private sealed class Journal
        {
            public int schema = 1;
            public List<AttemptRecord> records = new List<AttemptRecord>();
        }

        [Serializable] private sealed class DayTrend
        {
            public string dateUtc;
            public int observedAchievementChanges;
        }

        [Serializable] private sealed class PracticeItem
        {
            public string levelId, targetId, title, reason;
            public int tier, pendingSessions, passingSessions;
            public bool recordedComplete;
        }

        [Serializable] private sealed class Report
        {
            public string schema = "ProjectSpark.TrainingAnalytics.v1";
            public string generatedUtc;
            public string title = "Spark AI - Training Progress Report";
            public int observedObjectives, recordedComplete, recordedIncomplete;
            public int[] objectivesPerTier, completedPerTier;
            public int observedAchievementChangesLast7Days;
            public List<DayTrend> trend;
            public List<PracticeItem> recommendedPractice;
            public List<PracticeItem> recurringPendingObservations;
            public string[] limitations;
        }

        [Header("Existing Project Spark Systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkAITrainingDashboard trainingDashboard;
        [SerializeField] private SparkAIInteractiveTrainingChallenges training;
        [SerializeField] private SparkAIUnifiedAssistant assistant;

        [Header("Editor-Built Analytics UI")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text earnedText;
        [SerializeField] private TMP_Text remainingText;
        [SerializeField] private TMP_Text tierSummaryText;
        [SerializeField] private TMP_Text trendSummaryText;
        [SerializeField] private TMP_Text issueText;
        [SerializeField] private TMP_Text recommendationsText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image[] trendBars;
        [SerializeField] private TMP_Text[] trendLabels;

        [Header("Observation")]
        [SerializeField, Range(0.25f, 5f)] private float updateIntervalSeconds = 1f;
        [Tooltip("Pending in 2+ distinct observed sessions makes a topic eligible for the repeated-pending section. This is not evidence of user error.")]
        [SerializeField, Min(2)] private int repeatedPendingThreshold = 2;

        private const string DashboardSaveKey = "ProjectSpark.TrainingDashboard.v1";
        private const string JournalSaveKey = "ProjectSpark.TrainingAnalytics.Observations.v1";
        private const int MaxRecords = 250;
        private LevelGamePlayManager subscribedLevel;
        private Journal journal;
        private DashboardStore achievementIndex;
        private readonly HashSet<string> pendingThisSession = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> passingThisSession = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, bool> lastEvaluatorStates = new Dictionary<string, bool>(StringComparer.Ordinal);
        private string sessionLevelId = "";
        private float nextUpdateAt;
        private string statusMessage = "Records are read-only. Start a level to observe evaluator history.";
        private string nextAdvice = "";

        private void Awake()
        {
            LoadJournal();
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable()
        {
            if (journal == null) LoadJournal();
            Resolve();
            Subscribe();
            ReadAchievements();
            ObserveEvaluation();
            Render();
        }

        private void OnDisable()
        {
            Unsubscribe();
            SaveJournal();
        }

        private void OnDestroy() { Unsubscribe(); }
        private void OnApplicationPause(bool pause) { if (pause) SaveJournal(); }

        private void Update()
        {
            if (Time.unscaledTime < nextUpdateAt) return;
            nextUpdateAt = Time.unscaledTime + Mathf.Max(0.25f, updateIntervalSeconds);
            Resolve();
            Subscribe();
            ObserveEvaluation();
            if (card != null && card.activeSelf)
            {
                ReadAchievements();
                Render();
            }
        }

        private void Resolve()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (trainingDashboard == null)
                trainingDashboard = FindFirstObjectByType<SparkAITrainingDashboard>(FindObjectsInactive.Include);
            if (training == null)
                training = FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedLevel == levelManager) return;
            Unsubscribe();
            subscribedLevel = levelManager;
            StartNewObservationSession();
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted += OnLevelStartedOrReset;
            subscribedLevel.LevelReset += OnLevelStartedOrReset;
            subscribedLevel.LevelEvaluationChanged += OnLevelEvaluated;
            subscribedLevel.LevelCompleted += OnLevelEvaluated;
        }

        private void Unsubscribe()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelStartedOrReset;
            subscribedLevel.LevelReset -= OnLevelStartedOrReset;
            subscribedLevel.LevelEvaluationChanged -= OnLevelEvaluated;
            subscribedLevel.LevelCompleted -= OnLevelEvaluated;
            subscribedLevel = null;
        }

        private void OnLevelStartedOrReset(SparkLevelDefinition _)
        {
            StartNewObservationSession();
            ObserveEvaluation();
            if (card != null && card.activeSelf) RefreshAnalytics();
        }

        private void OnLevelEvaluated(SparkLevelDefinition _)
        {
            ObserveEvaluation();
            if (card != null && card.activeSelf) RefreshAnalytics();
        }

        private void StartNewObservationSession()
        {
            pendingThisSession.Clear();
            passingThisSession.Clear();
            lastEvaluatorStates.Clear();
            sessionLevelId = levelManager != null && levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId ?? "" : "";
        }

        private static string Key(string a, string b)
        {
            a = a ?? "";
            b = b ?? "";
            return a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private static string Safe(string value, int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Unnamed";
            value = value.Trim().Replace('\n', ' ').Replace('\r', ' ');
            return value.Length > limit ? value.Substring(0, limit) + "..." : value;
        }

        private static string NowUtc()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
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

        private void LoadJournal()
        {
            journal = null;
            string raw = PlayerPrefs.GetString(JournalSaveKey, "");
            if (!string.IsNullOrEmpty(raw))
            {
                try { journal = JsonUtility.FromJson<Journal>(raw); }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Spark Training Analytics] Journal could not be read: " + ex.Message, this);
                }
            }
            if (journal == null || journal.schema != 1) journal = new Journal();
            if (journal.records == null) journal.records = new List<AttemptRecord>();
        }

        private void SaveJournal()
        {
            if (journal == null) return;
            PlayerPrefs.SetString(JournalSaveKey, JsonUtility.ToJson(journal));
            PlayerPrefs.Save();
        }

        private void ReadAchievements()
        {
            achievementIndex = null;
            string raw = PlayerPrefs.GetString(DashboardSaveKey, "");
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try { achievementIndex = JsonUtility.FromJson<DashboardStore>(raw); }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Spark Training Analytics] Dashboard index could not be read: " + ex.Message, this);
                }
            }
            if (achievementIndex == null || achievementIndex.schema != 1)
                achievementIndex = new DashboardStore();
            if (achievementIndex.objectives == null)
                achievementIndex.objectives = new List<DashboardObjective>();
            if (achievementIndex.history == null)
                achievementIndex.history = new List<DashboardEvent>();
        }

        private AttemptRecord FindAttemptRecord(string levelId, string targetId)
        {
            foreach (AttemptRecord record in journal.records)
                if (record != null && record.levelId == levelId && record.targetId == targetId)
                    return record;
            return null;
        }

        // Counts an evaluated target at most once per session per state; no frame-rate inflation.
        // A pending-to-pass transition is counted only if this component observed both states
        // within the same level session. Does not infer correctness of any component/wire.
        private void ObserveEvaluation()
        {
            if (journal == null || levelManager == null || levelManager.ActiveLevel == null ||
                levelManager.TargetResults == null) return;
            string levelId = levelManager.ActiveLevel.LevelId ?? "";
            if (string.IsNullOrEmpty(levelId)) return;
            if (!string.Equals(levelId, sessionLevelId, StringComparison.Ordinal))
                StartNewObservationSession();
            bool changed = false;
            foreach (var result in levelManager.TargetResults)
            {
                if (string.IsNullOrWhiteSpace(result.targetId)) continue;
                string targetId = result.targetId;
                string key = Key(levelId, targetId);
                AttemptRecord record = FindAttemptRecord(levelId, targetId);
                if (record == null)
                {
                    if (journal.records.Count >= MaxRecords) continue;
                    record = new AttemptRecord { levelId = levelId, targetId = targetId };
                    journal.records.Add(record);
                    changed = true;
                }
                string title = string.IsNullOrWhiteSpace(result.displayName)
                    ? targetId : result.displayName;
                title = Safe(title);
                if (record.displayName != title)
                {
                    record.displayName = title;
                    changed = true;
                }
                if (levelManager.ActiveLevel.Targets != null)
                {
                    foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
                    {
                        if (target != null && target.TargetId == targetId)
                        {
                            int tier = Tier(target);
                            if (record.tier != tier) { record.tier = tier; changed = true; }
                            break;
                        }
                    }
                }
                if (!result.satisfied && pendingThisSession.Add(key))
                {
                    record.observedPendingSessions++;
                    changed = true;
                }
                if (result.satisfied && passingThisSession.Add(key))
                {
                    record.observedPassingSessions++;
                    changed = true;
                }
                if (lastEvaluatorStates.TryGetValue(key, out bool earlier) &&
                    !earlier && result.satisfied)
                {
                    record.pendingToPassTransitions++;
                    changed = true;
                }
                if (!lastEvaluatorStates.TryGetValue(key, out bool oldState) ||
                    oldState != result.satisfied)
                {
                    record.lastState = result.satisfied ? "PASS" : "PENDING";
                    record.lastObservedUtc = NowUtc();
                    lastEvaluatorStates[key] = result.satisfied;
                    changed = true;
                }
            }
            if (changed) SaveJournal();
        }

        private bool IsCompleted(string levelId, string targetId)
        {
            foreach (DashboardObjective record in achievementIndex.objectives)
                if (record != null && record.levelId == levelId && record.targetId == targetId)
                    return record.completed;
            // When the dashboard has not observed this level, only the raw training key
            // can confirm completion for an exact active-level objective.
            if (levelManager != null && levelManager.ActiveLevel != null &&
                levelManager.ActiveLevel.LevelId == levelId)
                return PlayerPrefs.GetInt("ProjectSpark.Training.v1/" + Key(levelId, targetId), 0) == 1;
            return false;
        }

        private static bool TryParseObservedUtc(string value, out DateTime date)
        {
            return DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm 'UTC'",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out date);
        }

        private Report CreateReport()
        {
            if (achievementIndex == null) ReadAchievements();
            var report = new Report
            {
                generatedUtc = NowUtc(),
                objectivesPerTier = new int[3],
                completedPerTier = new int[3],
                trend = new List<DayTrend>(),
                recommendedPractice = new List<PracticeItem>(),
                recurringPendingObservations = new List<PracticeItem>(),
                limitations = new[]
                {
                    "Achievement totals cover only objectives indexed by the existing Training Dashboard.",
                    "Trend counts are first-observed saved achievement changes, not guaranteed completion timestamps.",
                    "Preexisting imported achievements have no known earning date and are excluded from trends.",
                    "Pending counts are observed across sessions, not proof of a mistake or a fault cause.",
                    "Hint usage, solve duration and score are not tracked by the current training controller.",
                    "The report neither awards progress nor changes official evaluator results."
                }
            };
            foreach (DashboardObjective item in achievementIndex.objectives)
            {
                if (item == null) continue;
                int tier = Mathf.Clamp(item.tier, 0, 2);
                report.observedObjectives++;
                report.objectivesPerTier[tier]++;
                if (item.completed)
                {
                    report.recordedComplete++;
                    report.completedPerTier[tier]++;
                }
            }
            report.recordedIncomplete = report.observedObjectives - report.recordedComplete;

            DateTime now = DateTime.UtcNow.Date;
            for (int i = 6; i >= 0; --i)
            {
                DateTime day = now.AddDays(-i);
                int changes = 0;
                foreach (DashboardEvent entry in achievementIndex.history)
                {
                    if (entry == null || entry.imported ||
                        !TryParseObservedUtc(entry.observedUtc, out DateTime when)) continue;
                    if (when.Date == day) changes++;
                }
                report.trend.Add(new DayTrend
                {
                    dateUtc = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    observedAchievementChanges = changes
                });
                report.observedAchievementChangesLast7Days += changes;
            }

            var candidates = new List<PracticeItem>();
            foreach (AttemptRecord record in journal.records)
            {
                if (record == null) continue;
                var practice = new PracticeItem
                {
                    levelId = record.levelId,
                    targetId = record.targetId,
                    title = Safe(record.displayName),
                    tier = Mathf.Clamp(record.tier, 0, 2),
                    pendingSessions = record.observedPendingSessions,
                    passingSessions = record.observedPassingSessions,
                    recordedComplete = IsCompleted(record.levelId, record.targetId),
                    reason = record.observedPendingSessions >= repeatedPendingThreshold
                        ? "Evaluator reported PENDING in " + record.observedPendingSessions + " observed sessions."
                        : "Objective has an observed evaluator history."
                };
                if (record.observedPendingSessions >= repeatedPendingThreshold)
                    report.recurringPendingObservations.Add(practice);
                if (!practice.recordedComplete) candidates.Add(practice);
            }
            // An incomplete objective that has been pending in several observed sessions is
            // prioritized; easier tier breaks ties. This is a rule, not an ML model.
            candidates.Sort((a, b) =>
            {
                int pending = b.pendingSessions.CompareTo(a.pendingSessions);
                if (pending != 0) return pending;
                int tier = a.tier.CompareTo(b.tier);
                return tier != 0 ? tier : string.CompareOrdinal(a.title, b.title);
            });
            for (int i = 0; i < candidates.Count && i < 3; ++i)
                report.recommendedPractice.Add(candidates[i]);
            report.recurringPendingObservations.Sort((a, b) =>
                b.pendingSessions.CompareTo(a.pendingSessions));
            if (report.recurringPendingObservations.Count > 6)
                report.recurringPendingObservations.RemoveRange(6,
                    report.recurringPendingObservations.Count - 6);
            return report;
        }

        private static string TierLabel(int tier)
        {
            return tier == 0 ? "Foundation" : tier == 1 ? "Applied" : "Advanced";
        }

        private void Render()
        {
            if (achievementIndex == null) ReadAchievements();
            Report report = CreateReport();
            if (levelText != null)
                levelText.text = levelManager != null && levelManager.ActiveLevel != null
                    ? "CURRENT LEVEL  /  " + Safe(levelManager.ActiveLevel.DisplayName, 70)
                    : "NO ACTIVE LEVEL  /  SAVED TRAINING HISTORY";
            if (totalText != null) totalText.text = report.observedObjectives.ToString();
            if (earnedText != null) earnedText.text = report.recordedComplete.ToString();
            if (remainingText != null) remainingText.text = report.recordedIncomplete.ToString();
            if (tierSummaryText != null)
            {
                tierSummaryText.text =
                    "FOUNDATION   " + report.completedPerTier[0] + "/" + report.objectivesPerTier[0] +
                    "     APPLIED   " + report.completedPerTier[1] + "/" + report.objectivesPerTier[1] +
                    "     ADVANCED   " + report.completedPerTier[2] + "/" + report.objectivesPerTier[2];
            }
            if (trendSummaryText != null)
                trendSummaryText.text = report.observedAchievementChangesLast7Days +
                    " saved achievement changes first observed in the last 7 UTC days";
            int peak = 0;
            foreach (DayTrend item in report.trend)
                peak = Mathf.Max(peak, item.observedAchievementChanges);
            for (int i = 0; i < 7; i++)
            {
                DayTrend day = report.trend[i];
                if (trendBars != null && i < trendBars.Length && trendBars[i] != null)
                    trendBars[i].fillAmount = peak > 0
                        ? (float)day.observedAchievementChanges / peak : 0f;
                if (trendLabels != null && i < trendLabels.Length && trendLabels[i] != null)
                    trendLabels[i].text = day.dateUtc.Substring(5) + "\n" + day.observedAchievementChanges;
            }

            var issues = new StringBuilder();
            if (report.recurringPendingObservations.Count == 0)
                issues.Append("No repeated pending observations yet. This tracking starts when this module is installed.");
            else
            {
                int limit = Mathf.Min(3, report.recurringPendingObservations.Count);
                for (int i = 0; i < limit; i++)
                {
                    PracticeItem item = report.recurringPendingObservations[i];
                    if (i > 0) issues.AppendLine();
                    issues.Append("• ").Append(item.title).Append("  —  ")
                        .Append(item.pendingSessions).Append(" observed pending sessions");
                }
            }
            if (issueText != null) issueText.text = issues.ToString();

            var practiceText = new StringBuilder();
            if (report.recommendedPractice.Count == 0)
                practiceText.Append("No incomplete, evaluator-observed training topics indexed yet. Start a level and evaluate its objectives.");
            else
            {
                for (int i = 0; i < report.recommendedPractice.Count; i++)
                {
                    PracticeItem item = report.recommendedPractice[i];
                    if (i > 0) practiceText.AppendLine();
                    practiceText.Append(i + 1).Append(".  ").Append(item.title).Append("  ·  ")
                        .Append(TierLabel(item.tier)).Append("  ·  ")
                        .Append(item.pendingSessions).Append(" pending session(s)");
                }
            }
            if (recommendationsText != null) recommendationsText.text = practiceText.ToString();
            if (statusText != null)
                statusText.text = statusMessage + "\n" +
                    "Trend = first observed save changes; pending sessions are not diagnoses.";
        }

        public void RefreshAnalytics()
        {
            Resolve();
            if (trainingDashboard != null) trainingDashboard.RefreshDashboard();
            ReadAchievements();
            ObserveEvaluation();
            Render();
        }

        public void Show()
        {
            RefreshAnalytics();
            if (card != null) card.SetActive(true);
        }

        public void Hide() { if (card != null) card.SetActive(false); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        public void OpenTraining()
        {
            Resolve();
            if (training == null)
            {
                statusMessage = "Training controller not found; install Interactive Training Challenges.";
                Render();
                return;
            }
            Hide();
            training.Show();
        }

        public void AskSpark()
        {
            Resolve();
            Report report = CreateReport();
            if (assistant == null || report.recommendedPractice.Count == 0)
            {
                statusMessage = "Start a level with an evaluated pending objective before asking Spark.";
                Render();
                return;
            }
            // Only ask about this level's target; do not imply another level is active.
            string activeId = levelManager != null && levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId : "";
            PracticeItem selected = null;
            foreach (PracticeItem item in report.recommendedPractice)
                if (item.levelId == activeId) { selected = item; break; }
            if (selected == null)
            {
                statusMessage = "Recommended topics are from other levels. Open that level to Ask Spark about it.";
                Render();
                return;
            }
            assistant.Ask("Help me practice the active level objective '" + Safe(selected.title, 70) +
                "'. Base your advice only on current verified evaluator results and circuit measurements. " +
                "Explain what to check next without inventing component bindings.");
            statusMessage = "Asked Spark for verified guidance on the active-level objective.";
            Render();
        }

        /// <summary>Writes JSON and CSV in Application.persistentDataPath; never modifies PlayerPrefs achievements.</summary>
        public void ExportReport()
        {
            try
            {
                RefreshAnalytics();
                Report report = CreateReport();
                string folder = Path.Combine(Application.persistentDataPath, "ProjectSpark", "TrainingReports");
                Directory.CreateDirectory(folder);
                string stem = "SparkTraining_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                string jsonPath = Path.Combine(folder, stem + ".json");
                string csvPath = Path.Combine(folder, stem + ".csv");
                File.WriteAllText(jsonPath, JsonUtility.ToJson(report, true), Encoding.UTF8);
                File.WriteAllText(csvPath, MakeCsv(report), Encoding.UTF8);
                statusMessage = "Exported JSON + CSV to: " + folder;
                Debug.Log("[Spark Training Analytics] Report saved:\n" + jsonPath + "\n" + csvPath, this);
            }
            catch (Exception ex)
            {
                statusMessage = "Report export failed: " + ex.Message;
                Debug.LogWarning("[Spark Training Analytics] " + statusMessage, this);
            }
            Render();
        }

        private static string CsvField(string text)
        {
            text = (text ?? "").Replace("\r", " ").Replace("\n", " ");
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private static string MakeCsv(Report report)
        {
            var sb = new StringBuilder(2048);
            sb.AppendLine("section,date_utc,level_id,target_id,title,value,notes");
            sb.Append("summary,,,,recorded_complete,").Append(report.recordedComplete).AppendLine(",Dashboard indexed achievements");
            sb.Append("summary,,,,observed_objectives,").Append(report.observedObjectives).AppendLine(",Only observed levels");
            foreach (DayTrend trend in report.trend)
                sb.Append("trend,").Append(trend.dateUtc).Append(",,,first_observed_changes,")
                    .Append(trend.observedAchievementChanges).AppendLine(",Imported records excluded");
            foreach (PracticeItem practice in report.recurringPendingObservations)
                sb.Append("recurring_pending,,").Append(CsvField(practice.levelId)).Append(',')
                    .Append(CsvField(practice.targetId)).Append(',').Append(CsvField(practice.title))
                    .Append(',').Append(practice.pendingSessions).AppendLine(",Distinct observed sessions only");
            foreach (PracticeItem practice in report.recommendedPractice)
                sb.Append("recommendation,,").Append(CsvField(practice.levelId)).Append(',')
                    .Append(CsvField(practice.targetId)).Append(',').Append(CsvField(practice.title))
                    .Append(',').Append(practice.pendingSessions).AppendLine(",Rule-based recommendation");
            return sb.ToString();
        }
    }
}
