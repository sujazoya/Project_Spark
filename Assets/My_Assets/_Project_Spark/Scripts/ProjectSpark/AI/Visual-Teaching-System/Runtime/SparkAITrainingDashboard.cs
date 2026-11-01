using System;
using System.Collections.Generic;
using System.Text;
using ProjectSpark.Gameplay;
using ProjectSpark.AI.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only learning dashboard for the existing SparkAIInteractiveTrainingChallenges.
    /// Reads only that controller's recorded PlayerPrefs achievements; never completes a
    /// challenge, changes a level, awards currency or guesses at circuit measurements.
    /// Only levels whose objectives were seen by this dashboard contribute to all-level totals.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITrainingDashboard : MonoBehaviour
    {
        [Serializable]
        private sealed class ObjectiveRecord
        {
            public string levelId;
            public string levelTitle;
            public string targetId;
            public string targetTitle;
            public int tier;
            public bool completed;
        }

        [Serializable]
        private sealed class LearningEvent
        {
            public string levelId;
            public string targetId;
            public string title;
            public string observedUtc;
            public bool imported;
        }

        [Serializable]
        private sealed class Store
        {
            public int schema = 1;
            public List<ObjectiveRecord> objectives = new List<ObjectiveRecord>();
            public List<LearningEvent> history = new List<LearningEvent>();
        }

        [Header("Existing Project Spark Systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkAIInteractiveTrainingChallenges training;
        [SerializeField] private SparkAIUnifiedAssistant assistant;

        [Header("Editor-Generated UI")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text completedText;
        [SerializeField] private TMP_Text percentText;
        [SerializeField] private TMP_Text foundationText;
        [SerializeField] private TMP_Text appliedText;
        [SerializeField] private TMP_Text advancedText;
        [SerializeField] private Image foundationFill;
        [SerializeField] private Image appliedFill;
        [SerializeField] private Image advancedFill;
        [SerializeField] private TMP_Text recommendationTitle;
        [SerializeField] private TMP_Text recommendationDetails;
        [SerializeField] private TMP_Text historyText;
        [SerializeField] private RectTransform historyContent;
        [SerializeField] private ScrollRect historyScroll;
        [SerializeField] private TMP_Text footerText;

        [Header("Sampling & History")]
        [SerializeField, Range(0.25f, 5f)] private float refreshIntervalSeconds = 0.7f;
        [SerializeField, Min(4)] private int maxVisibleHistoryEvents = 12;
        [SerializeField] private bool showImportedRecords = true;

        private const string SaveKey = "ProjectSpark.TrainingDashboard.v1";
        private const int MaxObjectiveRecords = 250;
        private const int MaxHistoryEvents = 120;
        private Store store;
        private string observedLevelId = "";
        private float nextRefreshAt;
        private SparkLevelTarget recommendedTarget;
        private string note = "Achievement totals include only levels this dashboard has observed.";

        public int ObservedObjectiveCount => store != null && store.objectives != null
            ? store.objectives.Count : 0;

        private void Awake()
        {
            Load();
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable()
        {
            if (store == null) Load();
            Resolve();
            Scan();
            Render();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshAt) return;
            nextRefreshAt = Time.unscaledTime + Mathf.Max(0.25f, refreshIntervalSeconds);
            Resolve();
            if (Scan() || card != null && card.activeSelf) Render();
        }

        private void OnDisable() { Save(); }
        private void OnApplicationPause(bool paused) { if (paused) Save(); }

        private void Resolve()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (training == null)
                training = FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
        }

        private void Load()
        {
            store = null;
            string saved = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrWhiteSpace(saved))
            {
                try { store = JsonUtility.FromJson<Store>(saved); }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Spark Training Dashboard] Could not read saved dashboard index: " + ex.Message, this);
                }
            }
            if (store == null || store.schema != 1) store = new Store();
            if (store.objectives == null) store.objectives = new List<ObjectiveRecord>();
            if (store.history == null) store.history = new List<LearningEvent>();
        }

        private void Save()
        {
            if (store == null) return;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(store));
            PlayerPrefs.Save();
        }

        // Exact format from SparkAIInteractiveTrainingChallenges.ProgressKey.
        private static string TrainingProgressKey(string level, string target)
        {
            string a = level ?? string.Empty;
            string b = target ?? string.Empty;
            return "ProjectSpark.Training.v1/" + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private static int GetTier(SparkLevelTarget target)
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
                default:
                    return 2;
            }
        }

        private static string Clean(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Unnamed";
            text = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return text.Length > maxLength ? text.Substring(0, maxLength) + "…" : text;
        }

        private ObjectiveRecord FindRecord(string levelId, string targetId)
        {
            foreach (ObjectiveRecord record in store.objectives)
            {
                if (record != null &&
                    string.Equals(record.levelId, levelId, StringComparison.Ordinal) &&
                    string.Equals(record.targetId, targetId, StringComparison.Ordinal))
                    return record;
            }
            return null;
        }

        private static string UtcNow()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private void AddHistory(ObjectiveRecord record, bool imported)
        {
            if (record == null) return;
            store.history.Add(new LearningEvent
            {
                levelId = record.levelId,
                targetId = record.targetId,
                title = Clean(record.targetTitle, 96),
                observedUtc = UtcNow(),
                imported = imported
            });
            while (store.history.Count > MaxHistoryEvents)
                store.history.RemoveAt(0);
        }

        /// <summary>
        /// A new discovered completed key is PREEXISTING, not a new completion. The UI never
        /// assigns its true earned date. A known 0->1 transition is timestamped as observed.
        /// </summary>
        private bool Scan()
        {
            if (store == null) Load();
            string levelId = levelManager != null && levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId : "";
            bool changed = !string.Equals(levelId, observedLevelId, StringComparison.Ordinal);
            observedLevelId = levelId;
            recommendedTarget = null;
            if (string.IsNullOrWhiteSpace(levelId) || levelManager.ActiveLevel.Targets == null)
                return changed;

            string levelTitle = Clean(levelManager.ActiveLevel.DisplayName, 92);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
            {
                if (target == null || string.IsNullOrWhiteSpace(target.TargetId) ||
                    !seen.Add(target.TargetId)) continue;
                string targetId = target.TargetId;
                ObjectiveRecord record = FindRecord(levelId, targetId);
                bool verified = PlayerPrefs.GetInt(TrainingProgressKey(levelId, targetId), 0) == 1;
                if (record == null)
                {
                    if (store.objectives.Count >= MaxObjectiveRecords)
                    {
                        note = "Dashboard index is full (250 objectives). Older records are retained.";
                        continue;
                    }
                    record = new ObjectiveRecord
                    {
                        levelId = levelId,
                        targetId = targetId,
                        levelTitle = levelTitle,
                        targetTitle = Clean(string.IsNullOrWhiteSpace(target.DisplayName)
                            ? targetId : target.DisplayName, 96),
                        tier = GetTier(target),
                        completed = verified
                    };
                    store.objectives.Add(record);
                    if (verified) AddHistory(record, true);
                    changed = true;
                }
                else
                {
                    string newTitle = Clean(string.IsNullOrWhiteSpace(target.DisplayName)
                        ? targetId : target.DisplayName, 96);
                    int newTier = GetTier(target);
                    if (record.tier != newTier || record.targetTitle != newTitle ||
                        record.levelTitle != levelTitle || record.completed != verified)
                    {
                        // Timestamp only a change that is actually observed by this dashboard.
                        if (!record.completed && verified) AddHistory(record, false);
                        record.tier = newTier;
                        record.targetTitle = newTitle;
                        record.levelTitle = levelTitle;
                        record.completed = verified;
                        changed = true;
                    }
                }
            }
            if (changed) Save();
            return changed;
        }

        private bool? EvaluatorResult(string targetId)
        {
            if (levelManager == null || levelManager.TargetResults == null) return null;
            foreach (var result in levelManager.TargetResults)
                if (string.Equals(result.targetId, targetId, StringComparison.Ordinal))
                    return result.satisfied;
            return null;
        }

        private SparkLevelTarget ChooseRecommendation(out string recommendationState)
        {
            recommendationState = "No active level";
            if (levelManager == null || levelManager.ActiveLevel == null ||
                levelManager.ActiveLevel.Targets == null) return null;

            SparkLevelTarget passedButUnrecorded = null;
            SparkLevelTarget alreadyRecorded = null;
            int bestPendingTier = int.MaxValue;
            int bestPassedTier = int.MaxValue;
            foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
            {
                if (target == null || string.IsNullOrWhiteSpace(target.TargetId) ||
                    !target.IsConfigured()) continue;
                ObjectiveRecord record = FindRecord(levelManager.ActiveLevel.LevelId, target.TargetId);
                bool saved = record != null && record.completed;
                bool? evaluator = EvaluatorResult(target.TargetId);
                if (!saved && evaluator.HasValue && !evaluator.Value &&
                    GetTier(target) < bestPendingTier)
                {
                    bestPendingTier = GetTier(target);
                    recommendedTarget = target;
                }
                else if (!saved && evaluator.HasValue && evaluator.Value &&
                    GetTier(target) < bestPassedTier)
                {
                    passedButUnrecorded = target;
                    bestPassedTier = GetTier(target);
                }
                else if (saved && alreadyRecorded == null) alreadyRecorded = target;
            }
            if (recommendedTarget != null)
            {
                recommendationState = "PENDING IN OFFICIAL EVALUATOR · READY TO PRACTICE";
                return recommendedTarget;
            }
            if (passedButUnrecorded != null)
            {
                recommendationState = "ALREADY PASSING · RESET LEVEL TO PRACTICE";
                return passedButUnrecorded;
            }
            recommendationState = alreadyRecorded != null
                ? "CURRENT LEVEL RECORDED · REVIEW OR TRY ANOTHER LEVEL"
                : "AWAITING CONFIGURED, EVALUATED OBJECTIVES";
            return alreadyRecorded;
        }

        private void Render()
        {
            if (store == null) return;
            int[] totalByTier = new int[3];
            int[] doneByTier = new int[3];
            int completed = 0;
            foreach (ObjectiveRecord record in store.objectives)
            {
                if (record == null) continue;
                int tier = Mathf.Clamp(record.tier, 0, 2);
                totalByTier[tier]++;
                if (record.completed)
                {
                    doneByTier[tier]++;
                    completed++;
                }
            }
            int count = totalByTier[0] + totalByTier[1] + totalByTier[2];
            if (totalText != null) totalText.text = count.ToString();
            if (completedText != null) completedText.text = completed.ToString();
            if (percentText != null) percentText.text = count > 0
                ? Mathf.RoundToInt(100f * completed / count) + "%" : "—";
            if (foundationText != null) foundationText.text = "FOUNDATION   " + doneByTier[0] + " / " + totalByTier[0];
            if (appliedText != null) appliedText.text = "APPLIED   " + doneByTier[1] + " / " + totalByTier[1];
            if (advancedText != null) advancedText.text = "ADVANCED   " + doneByTier[2] + " / " + totalByTier[2];
            if (foundationFill != null) foundationFill.fillAmount = Ratio(doneByTier[0], totalByTier[0]);
            if (appliedFill != null) appliedFill.fillAmount = Ratio(doneByTier[1], totalByTier[1]);
            if (advancedFill != null) advancedFill.fillAmount = Ratio(doneByTier[2], totalByTier[2]);
            if (levelText != null)
                levelText.text = levelManager != null && levelManager.ActiveLevel != null
                    ? "CURRENT LEVEL  ·  " + Clean(levelManager.ActiveLevel.DisplayName, 56)
                    : "NO LEVEL ACTIVE  ·  SHOWING PREVIOUSLY OBSERVED RECORDS";

            string state;
            recommendedTarget = null;
            SparkLevelTarget recommended = ChooseRecommendation(out state);
            if (recommendationTitle != null)
                recommendationTitle.text = recommended == null ? "No practice suggestion available"
                    : Clean(string.IsNullOrWhiteSpace(recommended.DisplayName)
                        ? recommended.TargetId : recommended.DisplayName, 100);
            if (recommendationDetails != null)
                recommendationDetails.text = state + "\n" +
                    (recommended == null ? "Open a level and run its official objective evaluation."
                    : Clean(recommended.Description, 160) + "\n" +
                      "Target type: " + recommended.Type +
                      "   |   Difficulty: " + TierName(GetTier(recommended)));

            if (historyText != null)
            {
                var sb = new StringBuilder(800);
                int shown = 0;
                for (int i = store.history.Count - 1; i >= 0 && shown < maxVisibleHistoryEvents; --i)
                {
                    LearningEvent entry = store.history[i];
                    if (entry == null || entry.imported && !showImportedRecords) continue;
                    if (shown > 0) sb.AppendLine().AppendLine();
                    sb.Append(entry.imported ? "PREEXISTING RECORD" : "VERIFIED RECORD OBSERVED");
                    sb.Append("  ·  ").Append(Clean(entry.observedUtc, 34)).AppendLine();
                    sb.Append(Clean(entry.title, 96)).Append("  [").Append(Clean(entry.levelId, 45)).Append(']');
                    shown++;
                }
                if (shown == 0) sb.Append("No recorded achievements observed yet.\nComplete a training challenge to begin.");
                historyText.text = sb.ToString();
                UpdateHistoryHeight();
            }
            if (footerText != null)
                footerText.text = note + "  •  UTC timestamps are first-observed times, not guaranteed completion times.";
        }

        private static string TierName(int tier)
        {
            return tier == 0 ? "Foundation" : tier == 1 ? "Applied" : "Advanced";
        }

        private static float Ratio(int done, int total)
        {
            return total > 0 ? Mathf.Clamp01((float)done / total) : 0f;
        }

        private void UpdateHistoryHeight()
        {
            if (historyText == null || historyContent == null) return;
            float width = historyText.rectTransform.rect.width;
            if (width <= 0f) width = 600f;
            Vector2 preferred = historyText.GetPreferredValues(historyText.text, width, Mathf.Infinity);
            float height = Mathf.Max(148f, Mathf.Ceil(preferred.y) + 24f);
            historyContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            historyText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height - 8f);
        }

        public void RefreshDashboard()
        {
            Resolve();
            Scan();
            Render();
        }

        public void Show()
        {
            RefreshDashboard();
            if (card != null) card.SetActive(true);
            if (historyScroll != null) historyScroll.verticalNormalizedPosition = 1f;
        }

        public void Hide() { if (card != null) card.SetActive(false); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        public void OpenTraining()
        {
            Resolve();
            if (training == null)
            {
                note = "Training controller is missing. Run Create Interactive Training Challenges first.";
                Render();
                return;
            }
            Hide();
            training.Show();
        }

        public void AskSpark()
        {
            Resolve();
            string state;
            recommendedTarget = null;
            SparkLevelTarget chosen = ChooseRecommendation(out state);
            if (assistant == null)
            {
                note = "Spark AI assistant is missing; no question was sent.";
                Render();
                return;
            }
            if (chosen == null)
            {
                note = "Start a level with evaluated targets before requesting specific practice advice.";
                Render();
                return;
            }
            string name = string.IsNullOrWhiteSpace(chosen.DisplayName) ? chosen.TargetId : chosen.DisplayName;
            assistant.Ask("Help me practise the configured objective '" + Clean(name, 70) +
                "' at my current level. Use only verified evaluator and circuit results. " +
                "Explain what I should investigate next without changing the circuit.");
            note = "Ask Spark received the selected practice topic.";
            Render();
        }
    }
}
