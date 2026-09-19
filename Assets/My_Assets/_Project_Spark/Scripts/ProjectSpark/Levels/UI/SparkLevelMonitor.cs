using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// World-space diagnostic monitor for Project Spark levels.
    /// Presentation only: reads LevelGamePlayManager evaluation data and
    /// updates the assigned UI. It does not perform circuit simulation.
    /// </summary>
    public class SparkLevelMonitor : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private LevelGamePlayManager manager;
        [SerializeField] private bool autoFindManager = false;

        [Header("Theme")]
        [SerializeField] private SparkLevelMonitorTheme theme;

        [Header("Level")]
        [SerializeField] private TMP_Text levelIdText;
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text levelDescriptionText;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text statusMessageText;

        [Header("Electrical")]
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text voltageText;
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private TMP_Text powerText;

        [Header("Connection")]
        [SerializeField] private TMP_Text connectionText;
        [SerializeField] private TMP_Text affectedTerminalText;
        [SerializeField] private TMP_Text affectedTargetText;

        [Header("Objectives")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Slider progressBar;

        [Header("Fault")]
        [SerializeField] private GameObject faultPanel;
        [SerializeField] private TMP_Text faultText;

        private bool subscribed;

        private void Awake()
        {
            if (theme == null)
                theme = GetComponent<SparkLevelMonitorTheme>();

            if (manager == null && autoFindManager)
                manager = FindFirstObjectByType<LevelGamePlayManager>();
        }

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Start()
        {
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (subscribed || manager == null)
                return;

            manager.LevelEvaluationChanged += OnEvaluationChanged;
            manager.LevelStarted += OnLevelStarted;
            manager.LevelCompleted += OnLevelCompleted;
            manager.LevelFailed += OnLevelFailed;
            manager.LevelReset += OnLevelReset;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || manager == null)
                return;

            manager.LevelEvaluationChanged -= OnEvaluationChanged;
            manager.LevelStarted -= OnLevelStarted;
            manager.LevelCompleted -= OnLevelCompleted;
            manager.LevelFailed -= OnLevelFailed;
            manager.LevelReset -= OnLevelReset;

            subscribed = false;
        }

       private void OnEvaluationChanged(SparkLevelDefinition level)
{
    Refresh();
}

        private void OnLevelStarted(SparkLevelDefinition level)
        {
            Refresh();
        }

        private void OnLevelCompleted(SparkLevelDefinition level)
        {
            Refresh();
        }

        private void OnLevelFailed(SparkLevelDefinition level, string reason)
        {
            Refresh();
        }

        private void OnLevelReset(SparkLevelDefinition level)
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the complete monitor from the current gameplay snapshot.
        /// </summary>
        public void Refresh()
        {
            SparkLevelMonitorSnapshot snapshot =
                SparkLevelMonitorSnapshot.FromManager(manager);

            ApplySnapshot(snapshot);
        }

        private void ApplySnapshot(SparkLevelMonitorSnapshot snapshot)
        {
            if (levelIdText != null)
                levelIdText.text = snapshot.levelId;

            if (levelNameText != null)
                levelNameText.text = snapshot.levelName;

            if (levelDescriptionText != null)
                levelDescriptionText.text = snapshot.levelDescription;

            if (statusText != null)
                statusText.text = snapshot.stateText;

            if (statusMessageText != null)
                statusMessageText.text = snapshot.message;

            if (sourceText != null)
                sourceText.text = snapshot.sourceName;

            if (voltageText != null)
                voltageText.text = $"{snapshot.voltage:0.00} V";

            if (currentText != null)
                currentText.text = $"{snapshot.current:0.000} A";

            if (powerText != null)
                powerText.text = $"{snapshot.power:0.000} W";

            if (connectionText != null)
                connectionText.text = snapshot.GetConnectionText();

            if (affectedTerminalText != null)
            {
                affectedTerminalText.text =
                    string.IsNullOrWhiteSpace(snapshot.affectedTerminalName)
                        ? "NONE"
                        : snapshot.affectedTerminalName;
            }

            if (affectedTargetText != null)
            {
                affectedTargetText.text =
                    string.IsNullOrWhiteSpace(snapshot.affectedTargetName)
                        ? "NONE"
                        : snapshot.affectedTargetName;
            }

            if (progressText != null)
                progressText.text = $"TARGETS  {snapshot.GetProgressText()}";

            if (progressBar != null)
                progressBar.value = snapshot.progress01;

            if (faultPanel != null)
                faultPanel.SetActive(snapshot.faultActive);

            if (faultText != null)
                faultText.text = snapshot.faultActive
                    ? snapshot.faultText
                    : "NONE";

            // Keep visual state synchronized with the exact same snapshot.
            if (theme != null)
                theme.Apply(snapshot);
        }

        public LevelGamePlayManager Manager
        {
            get => manager;
            set
            {
                if (manager == value)
                    return;

                Unsubscribe();
                manager = value;

                if (isActiveAndEnabled)
                {
                    TrySubscribe();
                    Refresh();
                }
            }
        }

        [ContextMenu("Refresh Monitor")]
        private void RefreshFromContextMenu()
        {
            Refresh();
        }
    }
}
