using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production notification/toast animation controller.
    ///
    /// This component controls ONE reusable notification item.
    /// A future NotificationManager should own pooling, queueing
    /// and notification lifetime management.
    ///
    /// Responsibilities:
    /// - Show / hide animation
    /// - Auto-dismiss timer
    /// - Progress timer
    /// - Pause / resume
    /// - Manual dismissal
    /// - Notification type state
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimator))]
    public sealed class UINotificationAnimator : MonoBehaviour
    {
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error
        }

        [Header("References")]
        [SerializeField]
        private UIAnimator animator;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RectTransform progressRoot;

        [SerializeField]
        private Image progressFill;

        [Header("Lifetime")]
        [SerializeField]
        [Min(0f)]
        private float defaultDuration = 3f;

        [SerializeField]
        private bool autoDismiss = true;

        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Progress")]
        [SerializeField]
        private bool showProgress = true;

        [SerializeField]
        private bool progressReversed = true;

        [Header("Behaviour")]
        [SerializeField]
        private bool pauseTimerWhenTimeScaleIsZero = false;

        [SerializeField]
        private bool deactivateAfterHide = true;

        private Sequence hideSequence;

        private Tween progressTween;

        private float remainingDuration;

        private float activeDuration;

        private bool isVisible;
        private bool isPaused;

        private NotificationType currentType =
            NotificationType.Info;

        public bool IsVisible => isVisible;

        public bool IsPaused => isPaused;

        public float RemainingDuration =>
            remainingDuration;

        public float ActiveDuration =>
            activeDuration;

        public NotificationType CurrentType =>
            currentType;

            public event Action<UINotificationAnimator> Hidden;



public string NotificationId => notificationId;

public string Title =>
    titleText != null ? titleText.text : string.Empty;

public string Message =>
    messageText != null ? messageText.text : string.Empty;



public float Progress =>
    activeDuration <= 0f
        ? 0f
        : Mathf.Clamp01(
            remainingDuration / activeDuration);


            // ============================================================
// INSPECTOR / RUNTIME DATA
// ============================================================

[SerializeField]
private TMP_Text titleText;

[SerializeField]
private TMP_Text messageText;


private string notificationId;




public void ResetForPool()
{
    KillTweens();

    isVisible = false;
    isPaused = false;

    activeDuration = 0f;
    remainingDuration = 0f;

    if (titleText != null)
        titleText.text = string.Empty;

    if (messageText != null)
        messageText.text = string.Empty;

    if (progressFill != null)
        progressFill.fillAmount = 0f;

    if (animator != null)
        animator.ResetToHidden();
}
public void SetUseUnscaledTime(bool value)
{
    useUnscaledTime = value;
}

public void Show(
    float duration,
    bool immediate = false)
{
    activeDuration = Mathf.Max(
        0.05f,
        duration);

    remainingDuration =
        activeDuration;

    isPaused = false;
    isVisible = true;

    KillLifetimeTween();

    if (immediate)
        animator.Show(true);
    else
        animator.Show();

    StartLifetime();
}


public string ActivePresetName
{
    get
    {
        if (animator == null)
            return "---";

        if (animator.Preset == null)
            return "---";

        return animator.Preset.name;
    }
}

        private void Reset()
        {
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }
        public void SetContent(
    string title,
    string message)
{
    if (titleText != null)
        titleText.text = title ?? string.Empty;

    if (messageText != null)
        messageText.text = message ?? string.Empty;
}
public void SetType(
    NotificationType type)
{
    currentType = type;
}
public void SetNotificationId(
    string id)
{
    notificationId =
        string.IsNullOrWhiteSpace(id)
            ? name
            : id;
}

        private void OnDisable()
        {
            KillTweens();

            isVisible = false;
            isPaused = false;
            remainingDuration = 0f;
            activeDuration = 0f;
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void CacheReferences()
        {
            if (animator == null)
                animator = GetComponent<UIAnimator>();

            if (canvasGroup == null)
                canvasGroup =
                    GetComponent<CanvasGroup>();
        }

        // ============================================================
        // SHOW
        // ============================================================

        public void Show(
            NotificationType type)
        {
            Show(
                type,
                defaultDuration,
                false);
        }

        public void Show(
            NotificationType type,
            float duration)
        {
            Show(
                type,
                duration,
                false);
        }

        public void Show(
            NotificationType type,
            float duration,
            bool immediate)
        {
            CacheReferences();

            KillTweens();

            currentType = type;

            activeDuration =
                Mathf.Max(0f, duration);

            remainingDuration =
                activeDuration;

            isPaused = false;
            isVisible = true;

            gameObject.SetActive(true);

            ResetProgress();

            if (immediate)
            {
                animator.ShowImmediate();
            }
            else
            {
                animator.Show(false);
            }

            if (autoDismiss &&
                activeDuration > 0f)
            {
                StartLifetime();
            }
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
            KillTweens();

            isPaused = false;
            remainingDuration = 0f;

            if (!isVisible &&
                !gameObject.activeSelf)
            {
                return;
            }

            if (immediate)
            {
                animator.HideImmediate();

                isVisible = false;

                if (deactivateAfterHide)
                    gameObject.SetActive(false);

                return;
            }

            animator.Hide(false);

            hideSequence =
                DOTween.Sequence()
                    .SetTarget(this)
                    .SetUpdate(useUnscaledTime);

            hideSequence.AppendInterval(
                GetHideDuration());

            hideSequence.OnComplete(() =>
            {
                hideSequence = null;
                isVisible = false;

                if (deactivateAfterHide)
                    gameObject.SetActive(false);
            });
        }

        // ============================================================
        // LIFETIME
        // ============================================================
private Tween lifetimeTween;

        private void StartLifetime()
        {
            KillLifetimeTween();

            if (activeDuration <= 0f)
                return;

            lifetimeTween =
                DOVirtual.Float(
                    activeDuration,
                    0f,
                    activeDuration,
                    value =>
                    {
                        if (isPaused)
                            return;

                        remainingDuration = value;
                    })
                .SetEase(Ease.Linear)
                .SetUpdate(useUnscaledTime)
                .SetTarget(this)
                .OnComplete(() =>
                {
                    lifetimeTween = null;

                    if (!isVisible || isPaused)
                        return;

                    remainingDuration = 0f;

                    Hide();
                });
        }
        private void KillLifetimeTween()
        {
            if (lifetimeTween == null)
                return;

            if (lifetimeTween.IsActive())
                lifetimeTween.Kill();

            lifetimeTween = null;
        }

        private void AutoDismiss()
        {
            if (!isVisible)
                return;

            if (isPaused)
            {
                return;
            }

            remainingDuration = 0f;

            Hide();
        }

        // ============================================================
        // PROGRESS
        // ============================================================

        private void StartProgress()
        {
            KillProgressTween();

            if (progressFill == null)
                return;

            float start =
                progressReversed ? 1f : 0f;

            float end =
                progressReversed ? 0f : 1f;

            progressFill.fillAmount = start;

            progressTween =
                DOTween.To(
                    () => progressFill.fillAmount,
                    value =>
                    {
                        if (!isPaused)
                            progressFill.fillAmount =
                                value;
                    },
                    end,
                    activeDuration)
                .SetEase(Ease.Linear)
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);
        }

        private void ResetProgress()
        {
            KillProgressTween();

            if (progressFill == null)
                return;

            progressFill.fillAmount =
                progressReversed ? 1f : 0f;
        }

        // ============================================================
        // PAUSE / RESUME
        // ============================================================

       public void Pause()
{
    if (!isVisible || isPaused)
        return;

    isPaused = true;

    if (progressTween != null &&
        progressTween.IsActive())
    {
        progressTween.Pause();
    }

    if (lifetimeTween != null &&
        lifetimeTween.IsActive())
    {
        lifetimeTween.Pause();
    }
}

        public void Resume()
{
    if (!isVisible || !isPaused)
        return;

    isPaused = false;

    if (progressTween != null &&
        progressTween.IsActive())
    {
        progressTween.Play();
    }

    if (lifetimeTween != null &&
        lifetimeTween.IsActive())
    {
        lifetimeTween.Play();
    }
}

        // ============================================================
        // REFRESH
        // ============================================================

        public void RefreshDuration(
            float duration)
        {
            activeDuration =
                Mathf.Max(0f, duration);

            remainingDuration =
                activeDuration;

            if (!isVisible)
                return;

            StartProgress();

            StartLifetime();
        }

        // ============================================================
        // HIDE DURATION
        // ============================================================

        private float GetHideDuration()
        {
            if (animator == null)
                return 0.20f;

            UIAnimationPreset preset =
                animator.Preset;

            if (preset == null)
                return 0.20f;

            float duration = 0f;

            if (preset.UsePosition)
            {
                duration =
                    Mathf.Max(
                        duration,
                        preset.PositionDuration);
            }

            if (preset.UseScale)
            {
                duration =
                    Mathf.Max(
                        duration,
                        preset.ScaleDuration);
            }

            if (preset.UseRotation)
            {
                duration =
                    Mathf.Max(
                        duration,
                        preset.RotationDuration);
            }

            if (preset.UseAlpha)
            {
                duration =
                    Mathf.Max(
                        duration,
                        preset.FadeDuration);
            }

            duration *=
                Mathf.Max(
                    0.01f,
                    preset.ExitMultiplier);

            return Mathf.Max(
                0.01f,
                duration);
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void KillTweens()
{
    if (hideSequence != null)
    {
        if (hideSequence.IsActive())
            hideSequence.Kill();

        hideSequence = null;
    }

    KillProgressTween();
    KillLifetimeTween();

    DOTween.Kill(this);
}

        private void KillProgressTween()
        {
            if (progressTween == null)
                return;

            if (progressTween.IsActive())
                progressTween.Kill();

            progressTween = null;
        }

        // ============================================================
        // PUBLIC CONTROL
        // ============================================================

        public void Dismiss()
        {
            Hide(false);
        }

        public void DismissImmediate()
        {
            Hide(true);
        }

        public void SetVisibleImmediate(
            bool visible)
        {
            if (visible)
                Show(currentType, activeDuration, true);
            else
                Hide(true);
        }
    }
}