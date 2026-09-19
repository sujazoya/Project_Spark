using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// UI presenter for SparkLevelSelector.
    ///
    /// This class only presents level-selection state.
    /// It does not own progression or gameplay rules.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelSelectionUI : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("System")]
        [SerializeField]
        private SparkLevelSelector selector;

        [Header("Text")]
        [SerializeField]
        private TMP_Text levelNumberText;

        [SerializeField]
        private TMP_Text levelNameText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text completionText;

        [Header("Buttons")]
        [SerializeField]
        private Button previousButton;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button retryButton;

        [Header("Optional State Objects")]
        [SerializeField]
        private GameObject lockedIndicator;

        [SerializeField]
        private GameObject completedIndicator;

        [SerializeField]
        private GameObject availableIndicator;

        [SerializeField]
        private GameObject noLevelIndicator;

        [Header("Behaviour")]
        [SerializeField]
        private bool refreshOnEnable = true;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveSelector();
            RegisterButtons();
        }

        private void OnEnable()
        {
            if (selector != null)
                selector.SelectionChanged += OnSelectionChanged;

            if (refreshOnEnable)
                Refresh();
        }

        private void OnDisable()
        {
            if (selector != null)
                selector.SelectionChanged -= OnSelectionChanged;
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void ResolveSelector()
        {
            if (selector == null)
                selector = GetComponent<SparkLevelSelector>();

            if (selector == null)
                selector = FindFirstObjectByType<SparkLevelSelector>();
        }

        private void RegisterButtons()
        {
            if (previousButton != null)
                previousButton.onClick.AddListener(OnPreviousClicked);

            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void OnDestroy()
        {
            if (previousButton != null)
                previousButton.onClick.RemoveListener(OnPreviousClicked);

            if (nextButton != null)
                nextButton.onClick.RemoveListener(OnNextClicked);

            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);

            if (retryButton != null)
                retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        // ============================================================
        // UI ACTIONS
        // ============================================================

        private void OnPreviousClicked()
        {
            if (selector == null)
                return;

            selector.SelectPrevious();
        }

        private void OnNextClicked()
        {
            if (selector == null)
                return;

            selector.SelectNext();
        }

        private void OnStartClicked()
        {
            if (selector == null)
                return;

            selector.StartSelectedLevel();

            Refresh();
        }

        private void OnContinueClicked()
        {
            if (selector == null)
                return;

            selector.Continue();

            Refresh();
        }

        private void OnRetryClicked()
        {
            if (selector == null)
                return;

            selector.Retry();

            Refresh();
        }

        // ============================================================
        // EVENTS
        // ============================================================

        private void OnSelectionChanged(int index)
        {
            Refresh();
        }

        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            if (selector == null)
            {
                RefreshNoSelector();
                return;
            }

            if (!selector.HasSelection)
            {
                RefreshNoLevel();
                return;
            }

            RefreshLevel();
        }

        private void RefreshLevel()
        {
            SparkLevelDefinition level =
                selector.SelectedLevel;

            if (level == null)
            {
                RefreshNoLevel();
                return;
            }

            bool unlocked =
                selector.IsSelectedLevelUnlocked;

            bool completed =
                selector.IsSelectedLevelCompleted;

            int index =
                selector.SelectedLevelIndex;

            int count =
                selector.LevelCount;

            // --------------------------------------------------------
            // TEXT
            // --------------------------------------------------------

            if (levelNumberText != null)
            {
                levelNumberText.text =
                    $"LEVEL {index + 1:00}";
            }

            if (levelNameText != null)
            {
                levelNameText.text =
                    string.IsNullOrWhiteSpace(level.DisplayName)
                        ? $"LEVEL {index + 1}"
                        : level.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    string.IsNullOrWhiteSpace(level.Description)
                        ? string.Empty
                        : level.Description;
            }

            if (statusText != null)
            {
                statusText.text =
                    GetStatusText(unlocked, completed);
            }

            if (completionText != null)
            {
                int completionCount =
                    selector.GetCompletionCount(index);

                completionText.text =
                    completionCount > 0
                        ? $"COMPLETIONS  {completionCount}"
                        : "NOT COMPLETED";
            }

            // --------------------------------------------------------
            // BUTTONS
            // --------------------------------------------------------

            if (previousButton != null)
                previousButton.interactable = index > 0;

            if (nextButton != null)
                nextButton.interactable =
                    index < count - 1;

            if (startButton != null)
                startButton.interactable = unlocked;

            if (continueButton != null)
                continueButton.interactable =
                    selector.LevelCount > 0;

            if (retryButton != null)
                retryButton.interactable =
                    unlocked;

            // --------------------------------------------------------
            // STATE INDICATORS
            // --------------------------------------------------------

            SetActive(
                lockedIndicator,
                !unlocked);

            SetActive(
                completedIndicator,
                unlocked && completed);

            SetActive(
                availableIndicator,
                unlocked && !completed);

            SetActive(
                noLevelIndicator,
                false);
        }

        private void RefreshNoLevel()
        {
            ClearText(levelNumberText);
            ClearText(levelNameText);
            ClearText(descriptionText);

            if (statusText != null)
                statusText.text = "NO LEVEL";

            if (completionText != null)
                completionText.text = string.Empty;

            if (previousButton != null)
                previousButton.interactable = false;

            if (nextButton != null)
                nextButton.interactable = false;

            if (startButton != null)
                startButton.interactable = false;

            if (continueButton != null)
                continueButton.interactable = false;

            if (retryButton != null)
                retryButton.interactable = false;

            SetActive(lockedIndicator, false);
            SetActive(completedIndicator, false);
            SetActive(availableIndicator, false);
            SetActive(noLevelIndicator, true);
        }

        private void RefreshNoSelector()
        {
            ClearText(levelNumberText);
            ClearText(levelNameText);
            ClearText(descriptionText);

            if (statusText != null)
                statusText.text = "SYSTEM NOT READY";

            if (completionText != null)
                completionText.text = string.Empty;

            if (previousButton != null)
                previousButton.interactable = false;

            if (nextButton != null)
                nextButton.interactable = false;

            if (startButton != null)
                startButton.interactable = false;

            if (continueButton != null)
                continueButton.interactable = false;

            if (retryButton != null)
                retryButton.interactable = false;

            SetActive(lockedIndicator, false);
            SetActive(completedIndicator, false);
            SetActive(availableIndicator, false);
            SetActive(noLevelIndicator, true);
        }

        // ============================================================
        // STATUS
        // ============================================================

        private string GetStatusText(
            bool unlocked,
            bool completed)
        {
            if (!unlocked)
                return "LOCKED";

            if (completed)
                return "COMPLETED";

            return "AVAILABLE";
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static void SetActive(
            GameObject target,
            bool value)
        {
            if (target != null)
                target.SetActive(value);
        }

        private static void ClearText(TMP_Text target)
        {
            if (target != null)
                target.text = string.Empty;
        }
    }
}