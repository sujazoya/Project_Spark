using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Runtime QA/diagnostic inspector for the Project Spark notification system.
    ///
    /// This component does not own notifications.
    /// It observes and controls the existing notification system.
    ///
    /// Intended for:
    /// - development
    /// - QA
    /// - animation tuning
    /// - notification testing
    ///
    /// Disable or strip this component from release builds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotificationInspector : MonoBehaviour
    {
        // ============================================================
        // TYPES
        // ============================================================

        [Serializable]
        private sealed class NotificationEntry
        {
            public UINotificationAnimator Animator;
            public int Index;
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("SYSTEM")]

        [SerializeField]
        private UINotificationManager notificationManager;

        [SerializeField]
        private bool autoFindManager = true;

        [Header("PANEL")]

        [SerializeField]
        private GameObject inspectorRoot;

        [SerializeField]
        private CanvasGroup inspectorCanvasGroup;

        [Header("CURRENT NOTIFICATION")]

        [SerializeField]
        private TMP_Text currentIdText;

        [SerializeField]
        private TMP_Text currentTypeText;

        [SerializeField]
        private TMP_Text currentStateText;

        [SerializeField]
        private TMP_Text currentTitleText;

        [SerializeField]
        private TMP_Text currentMessageText;

        [Header("LIFETIME")]

        [SerializeField]
        private TMP_Text durationText;

        [SerializeField]
        private TMP_Text remainingText;

        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private Image progressFill;

        [Header("ANIMATION")]

        [SerializeField]
        private TMP_Text animationStateText;

        [SerializeField]
        private TMP_Text presetText;

        [Header("QUEUE / POOL")]

        [SerializeField]
        private TMP_Text activeCountText;

        [SerializeField]
        private TMP_Text queuedCountText;

        [SerializeField]
        private TMP_Text poolCountText;

        [Header("SELECTION")]

        [SerializeField]
        private Transform notificationListRoot;

        [SerializeField]
        private Button refreshButton;

        [Header("CONTROLS")]

        [SerializeField]
        private Button pauseButton;

        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button dismissButton;

        [SerializeField]
        private Button dismissImmediateButton;

        [Header("TEST BUTTONS")]

        [SerializeField]
        private Button infoButton;

        [SerializeField]
        private Button successButton;

        [SerializeField]
        private Button warningButton;

        [SerializeField]
        private Button errorButton;

        [Header("TEST DURATIONS")]

        [SerializeField]
        [Min(0.1f)]
        private float shortTestDuration = 1f;

        [SerializeField]
        [Min(0.1f)]
        private float normalTestDuration = 3f;

        [SerializeField]
        [Min(0.1f)]
        private float longTestDuration = 5f;

        [Header("BEHAVIOUR")]

        [SerializeField]
        [Min(0.05f)]
        private float refreshInterval = 0.10f;

        [SerializeField]
        private bool refreshWhileVisible = true;

        [SerializeField]
        private bool hideWhenNoNotification;

        [SerializeField]
        private bool developmentOnly = true;

        // ============================================================
        // STATE
        // ============================================================

        private readonly List<UINotificationAnimator> activeNotifications = new();

        private UINotificationAnimator selectedNotification;

        private float refreshTimer;

        private bool initialized;

        // ============================================================
        // PUBLIC API
        // ============================================================

        public UINotificationAnimator SelectedNotification =>
            selectedNotification;

        public bool IsVisible =>
            inspectorRoot != null && inspectorRoot.activeSelf;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (developmentOnly)
            {
                gameObject.SetActive(false);
                return;
            }
#endif

            ResolveManager();

            BindButtons();

            initialized = true;

            Refresh();
        }

        private void OnEnable()
        {
            if (!initialized)
                return;

            Refresh();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void Update()
        {
            if (!refreshWhileVisible)
                return;

            if (!IsVisible)
                return;

            refreshTimer += Time.unscaledDeltaTime;

            if (refreshTimer < refreshInterval)
                return;

            refreshTimer = 0f;

            Refresh();
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void ResolveManager()
        {
            if (notificationManager != null)
                return;

            if (!autoFindManager)
                return;

            notificationManager =
                FindFirstObjectByType<UINotificationManager>();
        }

        private void BindButtons()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(Refresh);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(PauseSelected);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeSelected);

            if (dismissButton != null)
                dismissButton.onClick.AddListener(DismissSelected);

            if (dismissImmediateButton != null)
                dismissImmediateButton.onClick.AddListener(
                    DismissSelectedImmediate);

            if (infoButton != null)
                infoButton.onClick.AddListener(TestInfo);

            if (successButton != null)
                successButton.onClick.AddListener(TestSuccess);

            if (warningButton != null)
                warningButton.onClick.AddListener(TestWarning);

            if (errorButton != null)
                errorButton.onClick.AddListener(TestError);
        }

        private void UnbindButtons()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(Refresh);

            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(PauseSelected);

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(ResumeSelected);

            if (dismissButton != null)
                dismissButton.onClick.RemoveListener(DismissSelected);

            if (dismissImmediateButton != null)
                dismissImmediateButton.onClick.RemoveListener(
                    DismissSelectedImmediate);

            if (infoButton != null)
                infoButton.onClick.RemoveListener(TestInfo);

            if (successButton != null)
                successButton.onClick.RemoveListener(TestSuccess);

            if (warningButton != null)
                warningButton.onClick.RemoveListener(TestWarning);

            if (errorButton != null)
                errorButton.onClick.RemoveListener(TestError);
        }

        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            ResolveManager();

            CollectNotifications();

            ValidateSelectedNotification();

            RefreshCurrentNotification();

            RefreshStatistics();

            RefreshControls();
        }

        private void CollectNotifications()
        {
            activeNotifications.Clear();

            UINotificationAnimator[] found =
                FindObjectsByType<UINotificationAnimator>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            for (int i = 0; i < found.Length; i++)
            {
                UINotificationAnimator notification = found[i];

                if (notification == null)
                    continue;

                if (!notification.IsVisible)
                    continue;

                activeNotifications.Add(notification);
            }
        }

        private void ValidateSelectedNotification()
        {
            if (selectedNotification == null)
            {
                if (activeNotifications.Count > 0)
                    selectedNotification = activeNotifications[0];

                return;
            }

            if (!activeNotifications.Contains(selectedNotification))
            {
                selectedNotification = null;

                if (activeNotifications.Count > 0)
                    selectedNotification = activeNotifications[0];
            }
        }

        // ============================================================
        // CURRENT NOTIFICATION
        // ============================================================

        private void RefreshCurrentNotification()
        {
            if (selectedNotification == null)
            {
                ClearCurrentNotification();

                if (hideWhenNoNotification)
                    SetInspectorVisible(false);

                return;
            }

            SetInspectorVisible(true);

            // --------------------------------------------------------
            // ID
            // --------------------------------------------------------

            if (currentIdText != null)
                currentIdText.text =
                    selectedNotification.name;

            // --------------------------------------------------------
            // TYPE
            // --------------------------------------------------------

            if (currentTypeText != null)
            {
                currentTypeText.text =
                    selectedNotification.CurrentType.ToString()
                        .ToUpperInvariant();
            }

            // --------------------------------------------------------
            // STATE
            // --------------------------------------------------------

            if (currentStateText != null)
            {
                currentStateText.text =
                    GetNotificationState(selectedNotification);
            }

            // --------------------------------------------------------
            // CONTENT
            // --------------------------------------------------------

            if (currentTitleText != null)
                currentTitleText.text =
                    selectedNotification.Title;

            if (currentMessageText != null)
                currentMessageText.text =
                    selectedNotification.Message;

            // --------------------------------------------------------
            // LIFETIME
            // --------------------------------------------------------

            if (durationText != null)
            {
                durationText.text =
                    FormatSeconds(selectedNotification.ActiveDuration);
            }

            if (remainingText != null)
            {
                remainingText.text =
                    FormatSeconds(selectedNotification.RemainingDuration);
            }

            float progress =
                selectedNotification.Progress;

            if (progressText != null)
            {
                progressText.text =
                    $"{progress * 100f:0}%";
            }

            if (progressFill != null)
                progressFill.fillAmount = progress;

            // --------------------------------------------------------
            // ANIMATION
            // --------------------------------------------------------

            if (animationStateText != null)
            {
                animationStateText.text =
                    selectedNotification.IsPaused
                        ? "PAUSED"
                        : selectedNotification.IsVisible
                            ? "VISIBLE"
                            : "HIDDEN";
            }

            if (presetText != null)
            {
                presetText.text =
                    selectedNotification.ActivePresetName;
            }
        }

        private void ClearCurrentNotification()
        {
            SetText(currentIdText, "---");
            SetText(currentTypeText, "---");
            SetText(currentStateText, "NO ACTIVE NOTIFICATION");

            SetText(currentTitleText, "---");
            SetText(currentMessageText, "---");

            SetText(durationText, "---");
            SetText(remainingText, "---");
            SetText(progressText, "---");

            SetText(animationStateText, "---");
            SetText(presetText, "---");

            if (progressFill != null)
                progressFill.fillAmount = 0f;
        }

        // ============================================================
        // STATISTICS
        // ============================================================

        private void RefreshStatistics()
        {
            if (activeCountText != null)
            {
                activeCountText.text =
                    activeNotifications.Count.ToString();
            }

            if (queuedCountText != null)
            {
                int queued =
                    notificationManager != null
                        ? notificationManager.QueuedCount
                        : 0;

                queuedCountText.text =
                    queued.ToString();
            }

            if (poolCountText != null)
            {
                if (notificationManager != null)
                {
                    poolCountText.text =
                        $"{notificationManager.AvailablePoolCount} / " +
                        $"{notificationManager.PoolCapacity}";
                }
                else
                {
                    poolCountText.text = "---";
                }
            }
        }

        // ============================================================
        // CONTROLS
        // ============================================================

        private void RefreshControls()
        {
            bool hasSelection =
                selectedNotification != null;

            if (pauseButton != null)
                pauseButton.interactable =
                    hasSelection &&
                    !selectedNotification.IsPaused;

            if (resumeButton != null)
                resumeButton.interactable =
                    hasSelection &&
                    selectedNotification.IsPaused;

            if (dismissButton != null)
                dismissButton.interactable =
                    hasSelection;

            if (dismissImmediateButton != null)
                dismissImmediateButton.interactable =
                    hasSelection;
        }

        private void PauseSelected()
        {
            if (selectedNotification == null)
                return;

            selectedNotification.Pause();

            Refresh();
        }

        private void ResumeSelected()
        {
            if (selectedNotification == null)
                return;

            selectedNotification.Resume();

            Refresh();
        }

        private void DismissSelected()
        {
            if (selectedNotification == null)
                return;

            selectedNotification.Hide();

            selectedNotification = null;

            Refresh();
        }

        private void DismissSelectedImmediate()
        {
            if (selectedNotification == null)
                return;

            selectedNotification.DismissImmediate();

            selectedNotification = null;

            Refresh();
        }

        // ============================================================
        // TESTING
        // ============================================================

        public void TestInfo()
        {
            ShowTest(
                "System Information",
                "Project Spark notification system is operating normally.",
                UINotificationAnimator.NotificationType.Info);
        }

        public void TestSuccess()
        {
            ShowTest(
                "Repair Complete",
                "Component successfully repaired and verified.",
                UINotificationAnimator.NotificationType.Success);
        }

        public void TestWarning()
        {
            ShowTest(
                "Voltage Warning",
                "Input voltage is outside the recommended range.",
                UINotificationAnimator.NotificationType.Warning);
        }

        public void TestError()
        {
            ShowTest(
                "Repair Failed",
                "The circuit test detected an electrical fault.",
                UINotificationAnimator.NotificationType.Error);
        }

        private void ShowTest(
            string title,
            string message,
            UINotificationAnimator.NotificationType type)
        {
            ResolveManager();

            if (notificationManager == null)
            {
                Debug.LogWarning(
                    "[Project Spark] NotificationInspector could not find UINotificationManager.",
                    this);

                return;
            }

            notificationManager.Show(
                title,
                message,
                type,
                normalTestDuration);
        }

        public void TestShort()
        {
            ResolveManager();

            if (notificationManager == null)
                return;

            notificationManager.Show(
                "Quick Test",
                "Short notification lifetime.",
                UINotificationAnimator.NotificationType.Info,
                shortTestDuration);
        }

        public void TestLong()
        {
            ResolveManager();

            if (notificationManager == null)
                return;

            notificationManager.Show(
                "Extended Test",
                "Long notification lifetime.",
                UINotificationAnimator.NotificationType.Info,
                longTestDuration);
        }

        // ============================================================
        // SELECTION
        // ============================================================

        public void SelectNotification(
            UINotificationAnimator notification)
        {
            if (notification == null)
                return;

            selectedNotification = notification;

            Refresh();
        }

        public void SelectFirst()
        {
            Refresh();

            if (activeNotifications.Count == 0)
                return;

            selectedNotification =
                activeNotifications[0];

            Refresh();
        }

        public void SelectNext()
        {
            Refresh();

            if (activeNotifications.Count == 0)
            {
                selectedNotification = null;
                return;
            }

            if (selectedNotification == null)
            {
                selectedNotification =
                    activeNotifications[0];

                return;
            }

            int index =
                activeNotifications.IndexOf(
                    selectedNotification);

            index++;

            if (index >= activeNotifications.Count)
                index = 0;

            selectedNotification =
                activeNotifications[index];

            Refresh();
        }

        // ============================================================
        // PANEL
        // ============================================================

        public void ToggleInspector()
        {
            if (inspectorRoot == null)
                return;

            SetInspectorVisible(
                !inspectorRoot.activeSelf);
        }

        public void SetInspectorVisible(bool visible)
        {
            if (inspectorRoot != null)
                inspectorRoot.SetActive(visible);

            if (inspectorCanvasGroup != null)
            {
                inspectorCanvasGroup.alpha =
                    visible ? 1f : 0f;

                inspectorCanvasGroup.interactable =
                    visible;

                inspectorCanvasGroup.blocksRaycasts =
                    visible;
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static string GetNotificationState(
            UINotificationAnimator notification)
        {
            if (notification == null)
                return "NONE";

            if (notification.IsPaused)
                return "PAUSED";

            if (!notification.IsVisible)
                return "HIDDEN";

            return "VISIBLE";
        }

        private static string FormatSeconds(float value)
        {
            return $"{Mathf.Max(0f, value):0.00}s";
        }

        private static void SetText(
            TMP_Text target,
            string value)
        {
            if (target != null)
                target.text = value;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            shortTestDuration =
                Mathf.Max(0.1f, shortTestDuration);

            normalTestDuration =
                Mathf.Max(0.1f, normalTestDuration);

            longTestDuration =
                Mathf.Max(0.1f, longTestDuration);

            refreshInterval =
                Mathf.Max(0.05f, refreshInterval);
        }

#endif
    }
}