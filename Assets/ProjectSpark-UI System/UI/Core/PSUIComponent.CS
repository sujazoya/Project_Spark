using UnityEngine;

namespace ProjectSpark.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PSUIComponent : MonoBehaviour
    {
        [Header("Component Settings")]
        [SerializeField]
        private bool visibleOnStart;

        [SerializeField]
        private bool interactableOnStart = true;

        private CanvasGroup canvasGroup;

        public PSUIState State { get; private set; }

        public PSVisibilityState VisibilityState
        {
            get;
            private set;
        }

        public bool IsVisible =>
            VisibilityState == PSVisibilityState.Visible;

        public bool IsInteractable =>
            canvasGroup != null &&
            canvasGroup.interactable;

        protected CanvasGroup CanvasGroup =>
            canvasGroup;

        protected virtual void Awake()
        {
            canvasGroup =
                GetComponent<CanvasGroup>();

            InitializeComponent();
        }

        protected virtual void Start()
        {
            SetInteractable(
                interactableOnStart);

            if (visibleOnStart)
            {
                ShowImmediate();
            }
            else
            {
                HideImmediate();
            }
        }

        protected virtual void InitializeComponent()
        {
            State = PSUIState.Normal;
            VisibilityState =
                PSVisibilityState.Hidden;
        }

        public virtual void Show()
        {
            if (IsVisible)
                return;

            ShowImmediate();
        }

        public virtual void Hide()
        {
            if (!IsVisible)
                return;

            HideImmediate();
        }

        public void ShowImmediate()
        {
            VisibilityState =
                PSVisibilityState.Visible;

            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;

            State = PSUIState.Normal;

            OnShown();
        }

        public void HideImmediate()
        {
            VisibilityState =
                PSVisibilityState.Hidden;

            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            State = PSUIState.Hidden;

            OnHidden();
        }

        public virtual void SetInteractable(
            bool interactable)
        {
            CanvasGroup.interactable =
                interactable;

            CanvasGroup.blocksRaycasts =
                interactable;

            State = interactable
                ? PSUIState.Normal
                : PSUIState.Disabled;
        }

        public virtual void SetState(
            PSUIState state)
        {
            State = state;

            OnStateChanged(state);
        }

        protected virtual void OnStateChanged(
            PSUIState state)
        {
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }

        public virtual void RefreshTheme()
        {
        }
    }
}