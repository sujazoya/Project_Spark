using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production screen-level animation controller.
    ///
    /// Owns screen transition timing and prevents conflicting
    /// enter/exit operations.
    ///
    /// Actual visual animation remains inside UIAnimator.
    /// Panel orchestration remains inside UIPanelAnimator.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimator))]
    public sealed class UIScreenAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private UIAnimator screenAnimator;

        [Header("Optional Root Panel")]
        [SerializeField]
        private UIPanelAnimator rootPanelAnimator;

        [Header("Transition")]
        [SerializeField]
        private bool useRootPanel = true;

        [SerializeField]
        private bool ignoreTimeScale = true;

        [SerializeField]
        [Min(0f)]
        private float transitionLockDuration = 0.35f;

        [Header("Behaviour")]
        [SerializeField]
        private bool blockInteractionDuringTransition = true;

        [SerializeField]
        private bool deactivateAfterExit = true;

        private Sequence transitionSequence;

        private bool isTransitioning;
        private bool isShown;

        public bool IsTransitioning => isTransitioning;

        public bool IsShown => isShown;

        public UIAnimator ScreenAnimator => screenAnimator;

        public UIPanelAnimator RootPanelAnimator =>
            rootPanelAnimator;

        private void Reset()
        {
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnDisable()
        {
            KillTransition();

            isTransitioning = false;
            isShown = false;
        }

        private void OnDestroy()
        {
            KillTransition();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void CacheReferences()
        {
            if (screenAnimator == null)
                screenAnimator =
                    GetComponent<UIAnimator>();

            if (rootPanelAnimator == null)
                rootPanelAnimator =
                    GetComponent<UIPanelAnimator>();
        }

        // ============================================================
        // SHOW
        // ============================================================

        public void Show()
        {
            Show(false);
        }

        public void Show(bool immediate)
        {
            CacheReferences();

            if (screenAnimator == null)
                return;

            KillTransition();

            gameObject.SetActive(true);

            isTransitioning = !immediate;

            if (blockInteractionDuringTransition)
                SetInteraction(false);

            if (immediate)
            {
                ShowImmediate();
                return;
            }

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.Show(false);
            }
            else
            {
                screenAnimator.Show(false);
            }

            CreateTransitionLock(
                true);
        }

        // ============================================================
        // HIDE
        // ============================================================

        public void Hide()
        {
            Hide(false);
        }

        public void Hide(bool immediate)
        {
            CacheReferences();

            if (screenAnimator == null)
                return;

            KillTransition();

            isTransitioning = !immediate;

            if (blockInteractionDuringTransition)
                SetInteraction(false);

            if (immediate)
            {
                HideImmediate();
                return;
            }

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.Hide(false);
            }
            else
            {
                screenAnimator.Hide(false);
            }

            CreateTransitionLock(
                false);
        }

        // ============================================================
        // IMMEDIATE
        // ============================================================

        public void ShowImmediate()
        {
            KillTransition();

            gameObject.SetActive(true);

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.ShowImmediate();
            }
            else
            {
                screenAnimator.ShowImmediate();
            }

            isTransitioning = false;
            isShown = true;

            if (blockInteractionDuringTransition)
                SetInteraction(true);
        }

        public void HideImmediate()
        {
            KillTransition();

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.HideImmediate();
            }
            else
            {
                screenAnimator.HideImmediate();
            }

            isTransitioning = false;
            isShown = false;

            if (deactivateAfterExit)
                gameObject.SetActive(false);
        }

        // ============================================================
        // TRANSITION LOCK
        // ============================================================

        private void CreateTransitionLock(
            bool entering)
        {
            transitionSequence =
                DOTween.Sequence()
                    .SetTarget(this)
                    .SetUpdate(ignoreTimeScale);

            float duration =
                Mathf.Max(
                    0.01f,
                    transitionLockDuration);

            transitionSequence.AppendInterval(
                duration);

            transitionSequence.OnComplete(() =>
            {
                transitionSequence = null;

                isTransitioning = false;

                if (entering)
                {
                    isShown = true;

                    if (blockInteractionDuringTransition)
                        SetInteraction(true);
                }
                else
                {
                    isShown = false;

                    if (deactivateAfterExit)
                    {
                        gameObject.SetActive(false);
                        return;
                    }

                    if (blockInteractionDuringTransition)
                        SetInteraction(true);
                }
            });
        }

        // ============================================================
        // CONTROL
        // ============================================================

        public bool CanInteract()
        {
            return isShown &&
                   !isTransitioning &&
                   gameObject.activeInHierarchy;
        }

        public void CancelTransition()
        {
            KillTransition();

            isTransitioning = false;

            if (isShown &&
                blockInteractionDuringTransition)
            {
                SetInteraction(true);
            }
        }

        public void Stop()
        {
            KillTransition();

            if (screenAnimator != null)
                screenAnimator.Kill();

            if (rootPanelAnimator != null)
                rootPanelAnimator.Stop();

            isTransitioning = false;
        }

        public void ResetToShown()
        {
            Stop();

            gameObject.SetActive(true);

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.ResetToShown();
            }
            else
            {
                screenAnimator.ShowImmediate();
            }

            isShown = true;

            if (blockInteractionDuringTransition)
                SetInteraction(true);
        }

        public void ResetToHidden()
        {
            Stop();

            if (useRootPanel &&
                rootPanelAnimator != null)
            {
                rootPanelAnimator.ResetToHidden();
            }
            else
            {
                screenAnimator.HideImmediate();
            }

            isShown = false;

            if (deactivateAfterExit)
                gameObject.SetActive(false);
        }

        // ============================================================
        // INTERACTION
        // ============================================================

        private void SetInteraction(bool enabled)
        {
            CanvasGroup group =
                screenAnimator.CanvasGroup;

            if (group == null)
                return;

            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void KillTransition()
        {
            if (transitionSequence == null)
                return;

            if (transitionSequence.IsActive())
                transitionSequence.Kill();

            transitionSequence = null;
        }
    }
}