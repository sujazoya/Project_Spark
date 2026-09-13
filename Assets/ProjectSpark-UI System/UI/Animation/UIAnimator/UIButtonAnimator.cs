using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production button-state animation controller.
    ///
    /// Responsibilities:
    /// - Pointer hover
    /// - Pointer press
    /// - Selection/focus
    /// - Disabled visual state
    /// - Success feedback
    /// - Error feedback
    ///
    /// Lifecycle animation remains inside UIAnimator.Show/Hide.
    /// Button states use UIAnimator.TransitionToPreset().
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(UIAnimator))]
    public sealed class UIButtonAnimator :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler
    {
        public enum ButtonVisualState
        {
            Normal,
            Hover,
            Pressed,
            Selected,
            Focused,
            Disabled,
            Success,
            Error
        }

        [Header("References")]
        [SerializeField]
        private Button button;

        [SerializeField]
        private UIAnimator animator;

        [Header("State Presets")]
        [SerializeField]
        private UIAnimationPreset normalPreset;

        [SerializeField]
        private UIAnimationPreset hoverPreset;

        [SerializeField]
        private UIAnimationPreset pressedPreset;

        [SerializeField]
        private UIAnimationPreset selectedPreset;

        [SerializeField]
        private UIAnimationPreset focusedPreset;

        [SerializeField]
        private UIAnimationPreset disabledPreset;

        [SerializeField]
        private UIAnimationPreset successPreset;

        [SerializeField]
        private UIAnimationPreset errorPreset;

        [Header("Behaviour")]
        [SerializeField]
        private bool animateOnEnable = true;

        [SerializeField]
        private bool useHoverState = true;

        [SerializeField]
        private bool usePressedState = true;

        [SerializeField]
        private bool useSelectedState = true;

        [SerializeField]
        private bool useFocusedState = true;

        [SerializeField]
        private bool returnToNormalAfterFeedback = true;

        [SerializeField]
        [Min(0f)]
        private float feedbackReturnDelay = 0.25f;

        [SerializeField]
        private bool fallbackToNormalPreset = true;

        private bool pointerInside;
        private bool pointerPressed;
        private bool isSelected;
        private bool isFocused;

        private ButtonVisualState currentState =
            ButtonVisualState.Normal;

        private Coroutine feedbackCoroutine;

        public ButtonVisualState CurrentState => currentState;

        public Button Button => button;

        public UIAnimator Animator => animator;

        private void Reset()
        {
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            if (animateOnEnable)
            {
                RefreshVisualState(true);
            }
            else
            {
                RefreshVisualState(false);
            }
        }

        private void OnDisable()
        {
            StopFeedbackCoroutine();

            pointerInside = false;
            pointerPressed = false;
            isSelected = false;
            isFocused = false;

            currentState = ButtonVisualState.Normal;
        }

        private void OnDestroy()
        {
            StopFeedbackCoroutine();
        }

        private void CacheReferences()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (animator == null)
                animator = GetComponent<UIAnimator>();
        }

        // ============================================================
        // POINTER
        // ============================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerPressed = true;

            if (!CanUseInteractionStates())
                return;

            if (!usePressedState)
                return;

            SetState(
                ButtonVisualState.Pressed,
                false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerPressed = false;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        // ============================================================
        // SELECTION
        // ============================================================

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        // ============================================================
        // PUBLIC STATE CONTROL
        // ============================================================

        public void RefreshVisualState(bool immediate)
        {
            if (button == null)
                CacheReferences();

            if (button == null || animator == null)
                return;

            if (!button.interactable)
            {
                SetState(
                    ButtonVisualState.Disabled,
                    immediate);

                return;
            }

            ButtonVisualState nextState =
                ResolveInteractionState();

            SetState(nextState, immediate);
        }

        public void SetState(
            ButtonVisualState state,
            bool immediate = false)
        {
            if (animator == null)
                CacheReferences();

            if (animator == null)
                return;

            UIAnimationPreset preset =
                GetPreset(state);

            if (preset == null &&
                fallbackToNormalPreset &&
                state != ButtonVisualState.Normal)
            {
                preset = normalPreset;
            }

            currentState = state;

            animator.TransitionToPreset(
                preset,
                immediate);
        }

        // ============================================================
        // FEEDBACK
        // ============================================================

        public void PlaySuccess()
        {
            StopFeedbackCoroutine();

            SetState(
                ButtonVisualState.Success,
                false);

            if (animator != null)
                animator.PlayPunch();

            if (returnToNormalAfterFeedback)
            {
                feedbackCoroutine =
                    StartCoroutine(
                        ReturnFromFeedback());
            }
        }

        public void PlayError()
        {
            StopFeedbackCoroutine();

            SetState(
                ButtonVisualState.Error,
                false);

            if (animator != null)
                animator.PlayShake();

            if (returnToNormalAfterFeedback)
            {
                feedbackCoroutine =
                    StartCoroutine(
                        ReturnFromFeedback());
            }
        }

        public void PlayFeedback(
            ButtonVisualState feedbackState)
        {
            if (feedbackState != ButtonVisualState.Success &&
                feedbackState != ButtonVisualState.Error)
            {
                return;
            }

            StopFeedbackCoroutine();

            SetState(
                feedbackState,
                false);

            if (animator != null)
            {
                if (feedbackState ==
                    ButtonVisualState.Success)
                {
                    animator.PlayPunch();
                }
                else
                {
                    animator.PlayShake();
                }
            }

            if (returnToNormalAfterFeedback)
            {
                feedbackCoroutine =
                    StartCoroutine(
                        ReturnFromFeedback());
            }
        }

        // ============================================================
        // INTERACTION STATE RESOLUTION
        // ============================================================

        private ButtonVisualState ResolveInteractionState()
        {
            if (!button.interactable)
                return ButtonVisualState.Disabled;

            if (pointerPressed &&
                usePressedState)
            {
                return ButtonVisualState.Pressed;
            }

            if (pointerInside &&
                useHoverState)
            {
                return ButtonVisualState.Hover;
            }

            if (isSelected &&
                useSelectedState)
            {
                return ButtonVisualState.Selected;
            }

            if (isFocused &&
                useFocusedState)
            {
                return ButtonVisualState.Focused;
            }

            return ButtonVisualState.Normal;
        }

        private bool CanUseInteractionStates()
        {
            return button != null &&
                   button.interactable &&
                   enabled &&
                   gameObject.activeInHierarchy;
        }

        // ============================================================
        // PRESET LOOKUP
        // ============================================================

        private UIAnimationPreset GetPreset(
            ButtonVisualState state)
        {
            switch (state)
            {
                case ButtonVisualState.Normal:
                    return normalPreset;

                case ButtonVisualState.Hover:
                    return hoverPreset;

                case ButtonVisualState.Pressed:
                    return pressedPreset;

                case ButtonVisualState.Selected:
                    return selectedPreset;

                case ButtonVisualState.Focused:
                    return focusedPreset;

                case ButtonVisualState.Disabled:
                    return disabledPreset;

                case ButtonVisualState.Success:
                    return successPreset;

                case ButtonVisualState.Error:
                    return errorPreset;

                default:
                    return normalPreset;
            }
        }

        // ============================================================
        // FEEDBACK RETURN
        // ============================================================

        private IEnumerator ReturnFromFeedback()
        {
            if (feedbackReturnDelay > 0f)
                yield return new WaitForSecondsRealtime(
                    feedbackReturnDelay);

            feedbackCoroutine = null;

            if (!isActiveAndEnabled)
                yield break;

            RefreshVisualState(false);
        }

        private void StopFeedbackCoroutine()
        {
            if (feedbackCoroutine == null)
                return;

            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        // ============================================================
        // EXTERNAL INTERACTION
        // ============================================================

        public void SetInteractable(bool interactable)
        {
            if (button == null)
                CacheReferences();

            if (button == null)
                return;

            button.interactable = interactable;

            RefreshVisualState(false);
        }

        public void SetFocused(bool focused)
        {
            isFocused = focused;

            if (!CanUseInteractionStates())
                return;

            RefreshVisualState(false);
        }

        public void ResetToNormal(bool immediate = false)
        {
            pointerInside = false;
            pointerPressed = false;
            isSelected = false;
            isFocused = false;

            StopFeedbackCoroutine();

            SetState(
                ButtonVisualState.Normal,
                immediate);
        }
    }
}