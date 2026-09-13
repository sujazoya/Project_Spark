using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIAnimator : MonoBehaviour
    {
        [Header("REFERENCES")]

        [SerializeField]
        private RectTransform target;

        [SerializeField]
        private CanvasGroup canvasGroup;


        [Header("ANIMATION PRESET")]

        [SerializeField]
        private UIAnimationPreset preset;


        [Header("INITIAL STATE")]

        [SerializeField]
        private bool startHidden;

        private Vector2 shownPosition;
        private Vector3 shownScale;
        private Vector3 shownRotation;

        private Sequence activeSequence;

        public bool IsVisible { get; private set; }

        public RectTransform Target =>
            target;

        public CanvasGroup CanvasGroup =>
            canvasGroup;

        public UIAnimationPreset Preset =>
            preset;

            private Sequence stateSequence;
private UIAnimationPreset currentStatePreset;
[SerializeField]
private bool useUnscaledTime = true;
public void SetUseUnscaledTime(bool value)
{
    useUnscaledTime = value;
}


private float shownAlpha;


        private void Reset()
        {
            target =
                transform as RectTransform;

            canvasGroup =
                GetComponent<CanvasGroup>();
        }


        private void Awake()
        {
            EnsureReferences();

            CacheInitialState();

            if (startHidden)
                SetHiddenImmediate();
            else
                SetShownImmediate();

                shownPosition = target.anchoredPosition;
shownScale = target.localScale;
shownRotation = target.localEulerAngles;
shownAlpha = canvasGroup.alpha;
        }
        public void SetPreset(
    UIAnimationPreset value)
{
    preset = value;
}



        private void OnDestroy()
        {
            Kill();
        }


        private void EnsureReferences()
        {
            if (target == null)
            {
                target =
                    transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup =
                    GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }
        }
        public void ResetToHidden()
{
    Kill();

    if (target == null)
        return;

    target.anchoredPosition =
        shownPosition +
        GetStatePosition(preset) -
        shownPosition;

    target.localScale =
        GetStateScale(preset);

    target.localEulerAngles =
        GetStateRotation(preset);

    canvasGroup.alpha =
        GetStateAlpha(preset);

    target.gameObject.SetActive(false);
}


        private void CacheInitialState()
        {
            shownPosition =
                target.anchoredPosition;

            shownScale =
                target.localScale;

            shownRotation =
                target.localEulerAngles;
        }


        // =========================================================
        // SHOW
        // =========================================================

        public void Show()
        {
            Show(false);
        }


        public void Show(bool instant)
        {
            Kill();

            EnsureReferences();

            gameObject.SetActive(true);

            if (instant || preset == null)
            {
                SetShownImmediate();
                return;
            }

            ApplyHiddenState();

            if (preset.DisableInteractionDuringAnimation)
            {
                UIAnimationUtility
                    .DisableInteraction(
                        canvasGroup);
            }

            activeSequence =
                DOTween.Sequence()
                    .SetTarget(this)
                    .SetUpdate(
                        preset.IgnoreTimeScale);

            if (preset.Delay > 0f)
            {
                activeSequence.SetDelay(
                    preset.Delay);
            }


            // POSITION
            if (preset.UsePosition)
            {
                activeSequence.Join(
                    target.DOAnchorPos(
                            shownPosition,
                            preset.PositionDuration)
                        .SetEase(
                            preset.PositionEase));
            }


            // SCALE
            if (preset.UseScale)
            {
                activeSequence.Join(
                    target.DOScale(
                            shownScale,
                            preset.ScaleDuration)
                        .SetEase(
                            preset.ScaleEase));
            }


            // ROTATION
            if (preset.UseRotation)
            {
                activeSequence.Join(
                    target.DOLocalRotate(
                            shownRotation,
                            preset.RotationDuration)
                        .SetEase(
                            preset.RotationEase));
            }


            // ALPHA
            if (preset.UseAlpha)
            {
                activeSequence.Join(
                    canvasGroup.DOFade(
                            1f,
                            preset.FadeDuration)
                        .SetEase(
                            preset.FadeEase));
            }


            activeSequence.OnComplete(() =>
            {
                IsVisible = true;

                UIAnimationUtility
                    .EnableInteraction(
                        canvasGroup);
            });
        }


        // =========================================================
        // HIDE
        // =========================================================

        public void Hide()
        {
             
            Hide(false);
        }


        public void Hide(bool instant)
        {
             KillStateSequence();
            Kill();

            EnsureReferences();

            if (instant || preset == null)
            {
                SetHiddenImmediate();
                return;
            }

            UIAnimationUtility
                .DisableInteraction(
                    canvasGroup);

            float multiplier =
                preset.ExitMultiplier;

            activeSequence =
                DOTween.Sequence()
                    .SetTarget(this)
                    .SetUpdate(
                        preset.IgnoreTimeScale);


            // POSITION
            if (preset.UsePosition)
            {
                activeSequence.Join(
                    target.DOAnchorPos(
                            shownPosition +
                            preset.HiddenPositionOffset,
                            preset.PositionDuration *
                            multiplier)
                        .SetEase(
                            Ease.InCubic));
            }


            // SCALE
            if (preset.UseScale)
            {
                activeSequence.Join(
                    target.DOScale(
                            preset.HiddenScale,
                            preset.ScaleDuration *
                            multiplier)
                        .SetEase(
                            Ease.InCubic));
            }


            // ROTATION
            if (preset.UseRotation)
            {
                activeSequence.Join(
                    target.DOLocalRotate(
                            preset.HiddenRotation,
                            preset.RotationDuration *
                            multiplier)
                        .SetEase(
                            Ease.InCubic));
            }


            // ALPHA
            if (preset.UseAlpha)
            {
                activeSequence.Join(
                    canvasGroup.DOFade(
                            preset.HiddenAlpha,
                            preset.FadeDuration *
                            multiplier)
                        .SetEase(
                            Ease.InQuad));
            }


            activeSequence.OnComplete(() =>
            {
                IsVisible = false;

                if (preset.DisableObjectAfterHide)
                    gameObject.SetActive(false);
            });
        }


        // =========================================================
        // EFFECTS
        // =========================================================

        public void PlayPunch()
        {
            if (preset == null ||
                !preset.UsePunch)
                return;

            target.DOPunchScale(
                    preset.PunchScale,
                    preset.PunchDuration,
                    preset.PunchVibrato,
                    preset.PunchElasticity)
                .SetTarget(this)
                .SetUpdate(
                    preset.IgnoreTimeScale);
        }


        public void PlayShake()
        {
            if (preset == null ||
                !preset.UseShake)
                return;

            target.DOShakeAnchorPos(
                    preset.ShakeDuration,
                    preset.ShakeStrength,
                    preset.ShakeVibrato,
                    preset.ShakeRandomness,
                    false,
                    true)
                .SetTarget(this)
                .SetUpdate(
                    preset.IgnoreTimeScale);
        }


        // =========================================================
        // TOGGLE
        // =========================================================

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }


        // =========================================================
        // IMMEDIATE
        // =========================================================

        public void ShowImmediate()
        {
            Kill();

            EnsureReferences();

            gameObject.SetActive(true);

            SetShownImmediate();
        }


        public void HideImmediate()
        {
            Kill();

            EnsureReferences();

            SetHiddenImmediate();
        }


        private void SetShownImmediate()
        {
            target.anchoredPosition =
                shownPosition;

            target.localScale =
                shownScale;

            target.localEulerAngles =
                shownRotation;

            canvasGroup.alpha = 1f;

            UIAnimationUtility
                .EnableInteraction(
                    canvasGroup);

            IsVisible = true;
        }


        private void SetHiddenImmediate()
        {
            if (preset != null)
            {
                if (preset.UsePosition)
                {
                    target.anchoredPosition =
                        shownPosition +
                        preset.HiddenPositionOffset;
                }
                else
                {
                    target.anchoredPosition =
                        shownPosition;
                }


                if (preset.UseScale)
                {
                    target.localScale =
                        preset.HiddenScale;
                }
                else
                {
                    target.localScale =
                        shownScale;
                }


                if (preset.UseRotation)
                {
                    target.localEulerAngles =
                        preset.HiddenRotation;
                }
                else
                {
                    target.localEulerAngles =
                        shownRotation;
                }


                if (preset.UseAlpha)
                {
                    canvasGroup.alpha =
                        preset.HiddenAlpha;
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }
            }
            else
            {
                target.anchoredPosition =
                    shownPosition;

                target.localScale =
                    shownScale;

                target.localEulerAngles =
                    shownRotation;

                canvasGroup.alpha = 0f;
            }

            UIAnimationUtility
                .DisableInteraction(
                    canvasGroup);

            IsVisible = false;
        }


        private void ApplyHiddenState()
        {
            if (preset == null)
                return;

            if (preset.UsePosition)
            {
                target.anchoredPosition =
                    shownPosition +
                    preset.HiddenPositionOffset;
            }

            if (preset.UseScale)
            {
                target.localScale =
                    preset.HiddenScale;
            }

            if (preset.UseRotation)
            {
                target.localEulerAngles =
                    preset.HiddenRotation;
            }

            if (preset.UseAlpha)
            {
                canvasGroup.alpha =
                    preset.HiddenAlpha;
            }
        }


        // =========================================================
        // KILL
        // =========================================================

      public void Kill()
{
    if (activeSequence != null)
    {
         KillStateSequence();
        if (activeSequence.IsActive())
            activeSequence.Kill();

        activeSequence = null;
    }

   

    DOTween.Kill(this);
}
        #if UNITY_EDITOR

/// <summary>
/// Editor-only visual preview.
/// Does not use DOTween.
/// Allows the custom Project Spark Animation Editor
/// to scrub/preview animation safely in edit mode.
/// </summary>
public void EditorPreview(
    float normalizedTime,
    bool entering)
{
    if (preset == null)
        return;

    EnsureReferences();

    normalizedTime =
        Mathf.Clamp01(normalizedTime);

    float delayNormalized = 0f;

    if (preset.Delay > 0f)
    {
        delayNormalized =
            Mathf.Clamp01(
                preset.Delay /
                GetEditorPreviewDuration());
    }

    float t =
        normalizedTime <= delayNormalized
            ? 0f
            : Mathf.InverseLerp(
                delayNormalized,
                1f,
                normalizedTime);

    if (!entering)
        t = 1f - t;

    ApplyEditorPreview(t);
}

/// <summary>
/// Resets the target to its authored visible state.
/// </summary>
public void EditorPreviewReset()
{
    EnsureReferences();

    transform.localScale = shownScale;
    transform.localRotation =
        Quaternion.Euler(shownRotation);

    target.anchoredPosition = shownPosition;
    canvasGroup.alpha = 1f;

    IsVisible = true;
}

/// <summary>
/// Calculates the longest active animation channel.
/// </summary>
private float GetEditorPreviewDuration()
{
    float duration = 0f;

    if (preset.UsePosition)
        duration =
            Mathf.Max(
                duration,
                preset.PositionDuration);

    if (preset.UseScale)
        duration =
            Mathf.Max(
                duration,
                preset.ScaleDuration);

    if (preset.UseRotation)
        duration =
            Mathf.Max(
                duration,
                preset.RotationDuration);

    if (preset.UseAlpha)
        duration =
            Mathf.Max(
                duration,
                preset.FadeDuration);

    return Mathf.Max(
        duration + preset.Delay,
        0.01f);
}

/// <summary>
/// Applies the editor preview state.
/// </summary>
private void ApplyEditorPreview(float t)
{
    if (preset.UsePosition)
    {
        Vector2 hidden =
            shownPosition +
            preset.HiddenPositionOffset;

        target.anchoredPosition =
            Vector2.LerpUnclamped(
                hidden,
                shownPosition,
                EaseEditor(
                    t,
                    preset.PositionEase));
    }

    if (preset.UseScale)
    {
        transform.localScale =
            Vector3.LerpUnclamped(
                preset.HiddenScale,
                shownScale,
                EaseEditor(
                    t,
                    preset.ScaleEase));
    }

    if (preset.UseRotation)
    {
        Vector3 hiddenRotation =
            shownRotation +
            preset.HiddenRotation;

        Vector3 rotation =
            Vector3.LerpUnclamped(
                hiddenRotation,
                shownRotation,
                EaseEditor(
                    t,
                    preset.RotationEase));

        transform.localRotation =
            Quaternion.Euler(rotation);
    }

    if (preset.UseAlpha)
    {
        canvasGroup.alpha =
            Mathf.LerpUnclamped(
                preset.HiddenAlpha,
                1f,
                EaseEditor(
                    t,
                    preset.FadeEase));
    }
}

/// <summary>
/// Converts DOTween ease values to an editor-preview approximation.
/// </summary>
private float EaseEditor(
    float t,
    DG.Tweening.Ease ease)
{
    switch (ease)
    {
        case DG.Tweening.Ease.Linear:
            return t;

        case DG.Tweening.Ease.InQuad:
            return t * t;

        case DG.Tweening.Ease.OutQuad:
            return 1f - (1f - t) * (1f - t);

        case DG.Tweening.Ease.InOutQuad:
            return t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        case DG.Tweening.Ease.InCubic:
            return t * t * t;

        case DG.Tweening.Ease.OutCubic:
            return 1f - Mathf.Pow(1f - t, 3f);

        case DG.Tweening.Ease.InOutCubic:
            return t < 0.5f
                ? 4f * t * t * t
                : 1f -
                  Mathf.Pow(
                      -2f * t + 2f,
                      3f) / 2f;

        case DG.Tweening.Ease.OutBack:
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return
                1f +
                c3 * Mathf.Pow(t - 1f, 3f) +
                c1 * Mathf.Pow(t - 1f, 2f);
        }

        default:
            return t;
    }
}
/// <summary>
/// Transitions the UI element to a visual state preset.
/// This is separate from Show/Hide and is intended for
/// buttons, selectable controls and interactive UI states.
/// </summary>
public void TransitionToPreset(
    UIAnimationPreset targetPreset,
    bool immediate = false)
{
    EnsureReferences();

    if (target == null || canvasGroup == null)
        return;

    KillStateSequence();

    currentStatePreset = targetPreset;

    if (immediate)
    {
        ApplyStateImmediate(targetPreset);
        return;
    }

    stateSequence = DOTween.Sequence()
        .SetTarget(this)
        .SetUpdate(
            targetPreset != null &&
            targetPreset.IgnoreTimeScale);

    if (targetPreset != null &&
        targetPreset.Delay > 0f)
    {
        stateSequence.AppendInterval(targetPreset.Delay);
    }

    Vector2 targetPosition =
        GetStatePosition(targetPreset);

    Vector3 targetScale =
        GetStateScale(targetPreset);

    Vector3 targetRotation =
        GetStateRotation(targetPreset);

    float targetAlpha =
        GetStateAlpha(targetPreset);

    if (targetPreset != null &&
        targetPreset.UsePosition &&
        targetPreset.PositionDuration > 0f)
    {
        stateSequence.Join(
            target
                .DOAnchorPos(
                    targetPosition,
                    targetPreset.PositionDuration)
                .SetEase(targetPreset.PositionEase));
    }
    else
    {
        target.anchoredPosition = targetPosition;
    }

    if (targetPreset != null &&
        targetPreset.UseScale &&
        targetPreset.ScaleDuration > 0f)
    {
        stateSequence.Join(
            target
                .DOScale(
                    targetScale,
                    targetPreset.ScaleDuration)
                .SetEase(targetPreset.ScaleEase));
    }
    else
    {
        target.localScale = targetScale;
    }

    if (targetPreset != null &&
        targetPreset.UseRotation &&
        targetPreset.RotationDuration > 0f)
    {
        stateSequence.Join(
            target
                .DOLocalRotate(
                    targetRotation,
                    targetPreset.RotationDuration)
                .SetEase(targetPreset.RotationEase));
    }
    else
    {
        target.localEulerAngles = targetRotation;
    }

    if (targetPreset != null &&
        targetPreset.UseAlpha &&
        targetPreset.FadeDuration > 0f)
    {
        stateSequence.Join(
            canvasGroup
                .DOFade(
                    targetAlpha,
                    targetPreset.FadeDuration)
                .SetEase(targetPreset.FadeEase));
    }
    else if (targetPreset == null ||
             !targetPreset.UseAlpha)
    {
        canvasGroup.alpha = targetAlpha;
    }
}
private Vector2 GetStatePosition(UIAnimationPreset preset)
{
    if (preset != null && preset.UsePosition)
    {
        return shownPosition +
               preset.HiddenPositionOffset;
    }

    return shownPosition;
}

private Vector3 GetStateScale(UIAnimationPreset preset)
{
    if (preset != null && preset.UseScale)
    {
        return Vector3.Scale(
            shownScale,
            preset.HiddenScale);
    }

    return shownScale;
}

private Vector3 GetStateRotation(UIAnimationPreset preset)
{
    if (preset != null && preset.UseRotation)
    {
        return shownRotation +
               preset.HiddenRotation;
    }

    return shownRotation;
}

private float GetStateAlpha(UIAnimationPreset preset)
{
    if (preset != null && preset.UseAlpha)
    {
        return preset.HiddenAlpha;
    }

    return shownAlpha;
}
private void ApplyStateImmediate(UIAnimationPreset preset)
{
    target.anchoredPosition =
        GetStatePosition(preset);

    target.localScale =
        GetStateScale(preset);

    target.localEulerAngles =
        GetStateRotation(preset);

    canvasGroup.alpha =
        GetStateAlpha(preset);
}
private void KillStateSequence()
{
    if (stateSequence == null)
        return;

    if (stateSequence.IsActive())
        stateSequence.Kill();

    stateSequence = null;
}

#endif
    }
}