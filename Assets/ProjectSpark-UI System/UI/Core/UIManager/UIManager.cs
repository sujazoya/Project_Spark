using System;
using UnityEngine;
using ProjectSpark.UI.Feedback;
/*
 * 12. Example — Publishing an Objective Event

The eventual ObjectiveManager integration should look conceptually like:

UIManager.Instance.Events.PublishObjectiveUpdated(
    new UIObjectiveState(
        objectiveId,
        objectiveTitle,
        objectiveDescription,
        UIObjectiveStatus.Active,
        progress));

When the objective completes:

UIManager.Instance.Events.PublishObjectiveCompleted(
    new UIObjectiveState(
        objectiveId,
        objectiveTitle,
        objectiveDescription,
        UIObjectiveStatus.Completed,
        1f));

The important part is that ObjectiveManager remains the owner of objective logic.

The UI simply receives:

Objective Updated
Objective Completed
Objective Failed
 */

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Central entry point for the Project Spark UI system.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Core Systems")]

        [SerializeField]
        private ScreenManager screenManager;

        [SerializeField]
        private OverlayManager overlayManager;

        [SerializeField]
        private ModalManager modalManager;

        [SerializeField]
        private UIInputBlocker inputBlocker;

        [Header("Startup")]

        [SerializeField]
        private bool persistBetweenScenes = true;

        public ScreenManager Screens =>
            screenManager;

        public OverlayManager Overlays =>
            overlayManager;

        public ModalManager Modals =>
            modalManager;

        public UIInputBlocker Input =>
            inputBlocker;

        private UIIntegrationService integration;

        public UIIntegrationService Integration =>
            integration;

        public UIEventHub Events =>
            integration?.Events;

        public UIStateStore State =>
            integration?.State;

        public UIContext CurrentContext
        {
            get
            {
                if (screenManager == null ||
                    screenManager.CurrentScreen == null)
                {
                    return UIContext.None;
                }

                return screenManager
                    .CurrentScreen
                    .Context;
            }
        }

        public event Action<UIContext> ContextChanged;

        private UIContext previousContext =
            UIContext.None;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Initialize()
        {
            integration =
        new UIIntegrationService();

            if (screenManager == null)
            {
                Debug.LogError(
                    "UIManager requires ScreenManager.",
                    this);
            }

            if (overlayManager == null)
            {
                Debug.LogError(
                    "UIManager requires OverlayManager.",
                    this);
            }

            if (modalManager == null)
            {
                Debug.LogError(
                    "UIManager requires ModalManager.",
                    this);
            }

            if (inputBlocker == null)
            {
                Debug.LogError(
                    "UIManager requires UIInputBlocker.",
                    this);
            }

            screenManager?.Initialize();
            overlayManager?.Initialize();
            modalManager?.Initialize();

            if (screenManager != null)
            {
                screenManager.ScreenChanged +=
                    HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(
            UIScreen previous,
            UIScreen current)
        {
            UIContext newContext =
                current != null
                    ? current.Context
                    : UIContext.None;

            if (newContext == previousContext)
                return;

            previousContext = newContext;

            ContextChanged?.Invoke(
                newContext);
        }

        public bool ShowScreen(
            string screenId)
        {
            if (screenManager == null)
                return false;

            return screenManager.Show(
                screenId);
        }

        public void HideCurrentScreen()
        {
            screenManager?.HideCurrent();
        }

        public void ShowOverlay(
            string overlayId)
        {
            overlayManager?.Show(
                overlayId);
        }

        public void HideOverlay(
            string overlayId)
        {
            overlayManager?.Hide(
                overlayId);
        }

        public void OpenModal(
            UIModalRequest request)
        {
            modalManager?.Show(
                request);

            inputBlocker?.SetState(
                UIInputState.UIOnly);
        }

        public void CloseModal()
        {
            modalManager?.Close();

            inputBlocker?.SetState(
                UIInputState.Gameplay);
        }
    }
}