using System;
using System.Collections.Generic;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// One navigation hub for existing training/learning panels. Reads official level
    /// evaluation and existing training PlayerPrefs records; never changes them.
    /// No circuit, reward, unlock, training award, or saved achievement writes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIStudentProgressCenter : MonoBehaviour
    {
        [Header("Existing project systems (optional modules)")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private SparkAIInteractiveTrainingChallenges training;
        [SerializeField] private SparkAITrainingDashboard dashboard;
        [SerializeField] private SparkAITrainingAnalytics analytics;
        [SerializeField] private SparkAILearningCoach coach;
        [SerializeField] private SparkAIGuidedRepairMissions repairs;

        [Header("Editor-generated hub UI")]
        [SerializeField] private GameObject hubPanel;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text evaluatorText;
        [SerializeField] private TMP_Text trainingText;
        [SerializeField] private TMP_Text focusText;
        [SerializeField] private Image objectiveFill;
        [SerializeField] private Image trainingFill;
        [SerializeField] private TMP_Text trainingStatus;
        [SerializeField] private TMP_Text dashboardStatus;
        [SerializeField] private TMP_Text analyticsStatus;
        [SerializeField] private TMP_Text coachStatus;
        [SerializeField] private TMP_Text repairStatus;
        [SerializeField] private TMP_Text footerText;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button dashboardButton;
        [SerializeField] private Button analyticsButton;
        [SerializeField] private Button coachButton;
        [SerializeField] private Button repairsButton;
        [SerializeField] private Button exportButton;
        [SerializeField] private Button askButton;

        [Header("Navigation")]
        [Tooltip("Close other module panels on navigation (does not cancel their gameplay sessions).")]
        [SerializeField] private bool closeOtherPanelsOnNavigate = true;
        [SerializeField, Min(0.5f)] private float visibleRefreshSeconds = 1.5f;

        private LevelGamePlayManager subscribedLevel;
        private float nextRefresh;

        private void Awake()
        {
            Resolve();
            if (hubPanel != null) hubPanel.SetActive(false);
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

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Update()
        {
            if (hubPanel == null || !hubPanel.activeSelf) return;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + Mathf.Max(0.5f, visibleRefreshSeconds);
            Refresh();
        }

        private void Resolve()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (training == null)
                training = FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            if (dashboard == null)
                dashboard = FindFirstObjectByType<SparkAITrainingDashboard>(FindObjectsInactive.Include);
            if (analytics == null)
                analytics = FindFirstObjectByType<SparkAITrainingAnalytics>(FindObjectsInactive.Include);
            if (coach == null)
                coach = FindFirstObjectByType<SparkAILearningCoach>(FindObjectsInactive.Include);
            if (repairs == null)
                repairs = FindFirstObjectByType<SparkAIGuidedRepairMissions>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedLevel == levelManager) return;
            Unsubscribe();
            if (levelManager == null) return;
            subscribedLevel = levelManager;
            subscribedLevel.LevelStarted += OnLevelEvent;
            subscribedLevel.LevelReset += OnLevelEvent;
            subscribedLevel.LevelEvaluationChanged += OnLevelEvent;
            subscribedLevel.LevelCompleted += OnLevelEvent;
            subscribedLevel.LevelFailed += OnLevelFailed;
        }

        private void Unsubscribe()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelEvent;
            subscribedLevel.LevelReset -= OnLevelEvent;
            subscribedLevel.LevelEvaluationChanged -= OnLevelEvent;
            subscribedLevel.LevelCompleted -= OnLevelEvent;
            subscribedLevel.LevelFailed -= OnLevelFailed;
            subscribedLevel = null;
        }

        private void OnLevelEvent(SparkLevelDefinition _) => Refresh();
        private void OnLevelFailed(SparkLevelDefinition _, string __) => Refresh();

        private static string AchievementKey(string levelId, string targetId)
        {
            string a = levelId ?? string.Empty;
            string b = targetId ?? string.Empty;
            // Must match SparkAIInteractiveTrainingChallenges.ProgressKey exactly.
            return "ProjectSpark.Training.v1/" + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        [ContextMenu("Spark AI Progress / Refresh")]
        public void Refresh()
        {
            Resolve();
            Subscribe();

            bool hasLevel = levelManager != null && levelManager.ActiveLevel != null;
            Set(levelText, hasLevel
                ? "ACTIVE LEVEL  /  " + levelManager.ActiveLevel.DisplayName
                : "NO ACTIVE LEVEL  /  START A LEVEL TO VIEW PROGRESS");

            int passed = 0;
            int total = 0;
            int achieved = 0;
            string levelId = hasLevel ? levelManager.ActiveLevel.LevelId : string.Empty;

            // Only known, configured targets in the CURRENT level are counted.
            if (hasLevel && levelManager.ActiveLevel.Targets != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
                {
                    if (target == null || string.IsNullOrWhiteSpace(target.TargetId) ||
                        !seen.Add(target.TargetId)) continue;
                    total++;
                    if (PlayerPrefs.GetInt(AchievementKey(levelId, target.TargetId), 0) == 1)
                        achieved++;
                }
            }

            if (hasLevel)
            {
                // Official evaluator is the authority for passing objectives.
                passed = levelManager.SatisfiedTargetCount;
                int reported = levelManager.ActiveTargetCount;
                Set(evaluatorText, "EVALUATOR  " + passed + " / " + reported + " passed");
                Set(focusText, levelManager.IsCompleted
                    ? "LEVEL COMPLETE  /  Official evaluator confirmed"
                    : levelManager.IsFailed
                        ? "LEVEL FAILED  /  Review the official failure reason"
                        : levelManager.SourceShorted || levelManager.TargetShorted
                            ? "PRIORITY  /  Evaluator reports a short circuit"
                            : levelManager.Overloaded
                                ? "PRIORITY  /  Evaluator reports an overload"
                                : "NEXT STEP  /  Use Training or Coach for pending objectives");
                SetFill(objectiveFill, reported > 0 ? (float)passed / reported : 0f);
            }
            else
            {
                Set(evaluatorText, "EVALUATOR  —  No active level");
                Set(focusText, "Start a level, then choose a practice module.");
                SetFill(objectiveFill, 0f);
            }

            Set(trainingText, hasLevel
                ? "TRAINING RECORD  " + achieved + " / " + total + " verified achievements"
                : "TRAINING RECORD  —  No level selected");
            SetFill(trainingFill, total > 0 ? (float)achieved / total : 0f);

            Set(trainingStatus, training == null ? "NOT INSTALLED" :
                training.HasActiveChallenge ? "CHALLENGE IN PROGRESS" : "READY FOR PRACTICE");
            Set(dashboardStatus, dashboard == null ? "NOT INSTALLED" : "ACHIEVEMENTS & HISTORY");
            Set(analyticsStatus, analytics == null ? "NOT INSTALLED" : "TRENDS & EXPORTS");
            Set(coachStatus, coach == null ? "NOT INSTALLED" :
                coach.HasActivePractice ? "PRACTICE IN PROGRESS" : "RECOMMENDED PRACTICE");
            Set(repairStatus, repairs == null ? "NOT INSTALLED" :
                repairs.MissionCompleted ? "LAST MISSION VERIFIED" :
                repairs.MissionActive ? "REPAIR IN PROGRESS" : "FAULT-GUIDED REPAIR");
            Set(footerText, hasLevel
                ? "EVALUATOR = official pass/fail    •    TRAINING = separately recorded achievements"
                : "All achievements, scoring, unlocks and circuit values remain controlled by existing systems.");

            Enabled(trainingButton, training != null && training.gameObject.activeInHierarchy);
            Enabled(dashboardButton, dashboard != null && dashboard.gameObject.activeInHierarchy);
            Enabled(analyticsButton, analytics != null && analytics.gameObject.activeInHierarchy);
            Enabled(coachButton, coach != null && coach.gameObject.activeInHierarchy);
            Enabled(repairsButton, repairs != null && repairs.gameObject.activeInHierarchy);
            Enabled(exportButton, analytics != null && analytics.gameObject.activeInHierarchy);
            Enabled(askButton, assistant != null && assistant.gameObject.activeInHierarchy);
        }

        private static void Set(TMP_Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static void SetFill(Image fill, float value)
        {
            if (fill != null) fill.fillAmount = Mathf.Clamp01(value);
        }

        private static void Enabled(Button button, bool enabled)
        {
            if (button != null) button.interactable = enabled;
        }

        [ContextMenu("Spark AI Progress / Show")]
        public void Show()
        {
            Resolve();
            if (closeOtherPanelsOnNavigate) CloseModulePanels();
            if (hubPanel != null) hubPanel.SetActive(true);
            Refresh();
            nextRefresh = Time.unscaledTime + Mathf.Max(0.5f, visibleRefreshSeconds);
        }

        public void Hide()
        {
            if (hubPanel != null) hubPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (hubPanel != null && hubPanel.activeSelf) Hide();
            else Show();
        }

        private void CloseModulePanels()
        {
            if (training != null) training.Hide();
            if (dashboard != null) dashboard.Hide();
            if (analytics != null) analytics.Hide();
            if (coach != null) coach.Hide();
            if (repairs != null) repairs.Hide();
        }

        private bool PrepareNavigation(MonoBehaviour target, string label)
        {
            Resolve();
            if (target == null)
            {
                Debug.LogWarning("[Spark Student Center] " + label +
                    " is not installed. Run its existing Editor builder first.", this);
                Refresh();
                return false;
            }
            if (!target.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[Spark Student Center] " + label +
                    " GameObject is inactive. Enable its root to navigate.", target);
                return false;
            }
            if (closeOtherPanelsOnNavigate) CloseModulePanels();
            Hide();
            return true;
        }

        public void OpenTraining()
        {
            if (PrepareNavigation(training, "Interactive Training")) training.Show();
        }

        public void OpenDashboard()
        {
            if (PrepareNavigation(dashboard, "Training Dashboard")) dashboard.Show();
        }

        public void OpenAnalytics()
        {
            if (PrepareNavigation(analytics, "Training Analytics")) analytics.Show();
        }

        public void OpenCoach()
        {
            if (PrepareNavigation(coach, "Learning Coach")) coach.Show();
        }

        public void OpenRepairs()
        {
            if (PrepareNavigation(repairs, "Guided Repair Missions")) repairs.Show();
        }

        public void ExportAnalytics()
        {
            Resolve();
            if (analytics == null || !analytics.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[Spark Student Center] Analytics module unavailable.", this);
                return;
            }
            analytics.ExportReport();
        }

        public void AskSpark()
        {
            Resolve();
            if (assistant == null || !assistant.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[Spark Student Center] Spark AI Assistant unavailable.", this);
                return;
            }
            Hide();
            assistant.Show();
            assistant.Ask("Which current level objective should I practice next? Use verified results only.");
        }
    }
}
