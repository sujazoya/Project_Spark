using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Production-ready UI fade controller for Project Spark.
    ///
    /// Supports:
    /// - CanvasGroup fading
    /// - Graphic/Image fading
    /// - CanvasGroup + Graphic fading
    /// - Percentage-based ranges
    /// - Speed-based fading
    /// - Duration-based fading
    /// - Multiple easing styles
    /// - Custom AnimationCurve
    /// - Automatic Show On Enable
    /// - Delayed Show On Enable
    /// - Optional audio
    /// - Audio volume fading
    /// - Looping
    /// - Time-scale independent animation
    /// - UnityEvents
    ///
    /// Public API:
    ///
    /// FadeIn()
    /// FadeOut()
    /// Show()
    /// Hide()
    /// Toggle()
    /// Play()
    /// PlayFromCurrent()
    /// SetPercentage()
    /// Stop()
    /// Restart()
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkUIFade : MonoBehaviour
    {
        // =========================================================
        // ENUMS
        // =========================================================

        public enum FadeTarget
        {
            CanvasGroup,
            Graphic,
            CanvasGroupAndGraphic
        }

        public enum FadeMode
        {
            Speed,
            Duration
        }

        public enum FadeStyle
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            SmoothStep,
            SmootherStep,
            CustomCurve
        }

        // =========================================================
        // ON ENABLE
        // =========================================================

        [Header("On Enable")]

        [Tooltip(
            "Automatically fade this UI element when the GameObject becomes enabled.")]
        [SerializeField]
        private bool showOnEnable;

        [Tooltip(
            "Starting opacity percentage when Show On Enable is active.")]
        [Range(0f, 100f)]
        [SerializeField]
        private float onEnableFromPercentage = 0f;

        [Tooltip(
            "Ending opacity percentage when Show On Enable is active.")]
        [Range(0f, 100f)]
        [SerializeField]
        private float onEnableToPercentage = 100f;

        [Tooltip(
            "Delay before the automatic On Enable fade starts.")]
        [Min(0f)]
        [SerializeField]
        private float onEnableDelay;

        // =========================================================
        // TARGET
        // =========================================================

        [Header("Target")]

        [SerializeField]
        private FadeTarget target =
            FadeTarget.CanvasGroup;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Graphic graphic;

        // =========================================================
        // FADE RANGE
        // =========================================================

        [Header("Fade Range")]

        [Range(0f, 100f)]
        [SerializeField]
        private float fromPercentage = 0f;

        [Range(0f, 100f)]
        [SerializeField]
        private float toPercentage = 100f;

        // =========================================================
        // TIMING
        // =========================================================

        [Header("Fade Timing")]

        [SerializeField]
        private FadeMode fadeMode =
            FadeMode.Speed;

        [Tooltip(
            "Opacity percentage points changed per second.")]
        [Min(0.01f)]
        [SerializeField]
        private float speed = 100f;

        [Tooltip(
            "Total fade duration in seconds.")]
        [Min(0.01f)]
        [SerializeField]
        private float duration = 0.5f;

        [Tooltip(
            "Delay before normal Play/FadeIn/FadeOut animations.")]
        [Min(0f)]
        [SerializeField]
        private float delay;

        // =========================================================
        // STYLE
        // =========================================================

        [Header("Fade Style")]

        [SerializeField]
        private FadeStyle style =
            FadeStyle.EaseInOut;

        [SerializeField]
        private AnimationCurve customCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f);

        // =========================================================
        // TIME
        // =========================================================

        [Header("Time")]

        [Tooltip(
            "When enabled, the fade continues while Time.timeScale is 0.")]
        [SerializeField]
        private bool ignoreTimeScale = true;

        // =========================================================
        // AUDIO
        // =========================================================

        [Header("Audio")]

        [SerializeField]
        private bool useAudio;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip fadeInClip;

        [SerializeField]
        private AudioClip fadeOutClip;

        [Range(0f, 1f)]
        [SerializeField]
        private float audioVolume = 1f;

        [Tooltip(
            "When enabled, audio volume follows the fade progress.")]
        [SerializeField]
        private bool fadeAudio = true;

        // =========================================================
        // LOOP
        // =========================================================

        [Header("Loop")]

        [SerializeField]
        private bool loop;

        [Min(0f)]
        [SerializeField]
        private float loopDelay;

        // =========================================================
        // EVENTS
        // =========================================================

        [Header("Events")]

        [SerializeField]
        private UnityEvent onFadeStarted;

        [SerializeField]
        private UnityEvent onFadeCompleted;

        [SerializeField]
        private UnityEvent onFadeStopped;

        // =========================================================
        // RUNTIME
        // =========================================================

        private Coroutine fadeRoutine;

        private float currentPercentage;

        private bool isFading;

        // =========================================================
        // PUBLIC PROPERTIES
        // =========================================================

        public float CurrentPercentage =>
            currentPercentage;

        public bool IsFading =>
            isFading;

        public bool IsVisible =>
            currentPercentage > 0.001f;

        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            CacheReferences();

            currentPercentage =
                ReadCurrentPercentage();
        }

        private void OnEnable()
        {
            CacheReferences();

            /*
             * Automatic entrance animation.
             *
             * We explicitly set the starting opacity first.
             * This prevents the panel from appearing at its
             * previous alpha for one frame before fading.
             */
            if (showOnEnable)
            {
                float start =
                    Mathf.Clamp(
                        onEnableFromPercentage,
                        0f,
                        100f);

                float end =
                    Mathf.Clamp(
                        onEnableToPercentage,
                        0f,
                        100f);

                currentPercentage =
                    start;

                ApplyPercentage(
                    currentPercentage);

                StartFadeInternal(
                    start,
                    end,
                    onEnableDelay);

                return;
            }

            currentPercentage =
                ReadCurrentPercentage();
        }

        private void OnDisable()
        {
            StopFadeInternal(false);
        }

        // =========================================================
        // REFERENCE CACHE
        // =========================================================

        private void CacheReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup =
                    GetComponent<CanvasGroup>();
            }

            if (graphic == null)
            {
                graphic =
                    GetComponent<Graphic>();
            }
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        /// <summary>
        /// Fade using the configured From and To percentages.
        /// </summary>
        public void Play()
        {
            StartFadeInternal(
                fromPercentage,
                toPercentage,
                delay);
        }

        /// <summary>
        /// Fade from the current percentage
        /// to the configured To percentage.
        /// </summary>
        public void PlayFromCurrent()
        {
            StartFadeInternal(
                currentPercentage,
                toPercentage,
                delay);
        }

        /// <summary>
        /// Fade from current percentage to 100%.
        /// </summary>
        public void FadeIn()
        {
            StartFadeInternal(
                currentPercentage,
                100f,
                delay);
        }

        /// <summary>
        /// Fade from current percentage to 0%.
        /// </summary>
        public void FadeOut()
        {
            StartFadeInternal(
                currentPercentage,
                0f,
                delay);
        }

        /// <summary>
        /// Fade from 0% to 100%.
        /// </summary>
        public void Show()
        {
            StartFadeInternal(
                0f,
                100f,
                delay);
        }

        /// <summary>
        /// Fade from 100% to 0%.
        /// </summary>
        public void Hide()
        {
            StartFadeInternal(
                100f,
                0f,
                delay);
        }

        /// <summary>
        /// Toggle between visible and invisible.
        /// </summary>
        public void Toggle()
        {
            if (currentPercentage > 50f)
            {
                FadeOut();
            }
            else
            {
                FadeIn();
            }
        }

        /// <summary>
        /// Immediately set the opacity percentage.
        /// </summary>
        public void SetPercentage(
            float percentage)
        {
            StopFadeInternal(false);

            currentPercentage =
                Mathf.Clamp(
                    percentage,
                    0f,
                    100f);

            ApplyPercentage(
                currentPercentage);
        }

        /// <summary>
        /// Stop the current fade.
        /// </summary>
        public void Stop()
        {
            StopFadeInternal(true);
        }

        /// <summary>
        /// Restart using the configured From and To values.
        /// </summary>
        public void Restart()
        {
            StartFadeInternal(
                fromPercentage,
                toPercentage,
                delay);
        }

        // =========================================================
        // FADE START
        // =========================================================

        private void StartFadeInternal(
            float from,
            float to,
            float fadeDelay)
        {
            StopFadeInternal(false);

            from =
                Mathf.Clamp(
                    from,
                    0f,
                    100f);

            to =
                Mathf.Clamp(
                    to,
                    0f,
                    100f);

            fadeDelay =
                Mathf.Max(
                    0f,
                    fadeDelay);

            currentPercentage =
                from;

            ApplyPercentage(
                currentPercentage);

            fadeRoutine =
                StartCoroutine(
                    FadeRoutine(
                        from,
                        to,
                        fadeDelay));
        }

        // =========================================================
        // FADE ROUTINE
        // =========================================================

        private IEnumerator FadeRoutine(
            float from,
            float to,
            float fadeDelay)
        {
            isFading = true;

            onFadeStarted?.Invoke();

            PlayFadeAudio(
                from,
                to);

            // -----------------------------------------------------
            // DELAY
            // -----------------------------------------------------

            if (fadeDelay > 0f)
            {
                yield return Wait(
                    fadeDelay);
            }

            // -----------------------------------------------------
            // DISTANCE
            // -----------------------------------------------------

            float distance =
                Mathf.Abs(
                    to - from);

            // Nothing to animate.
            if (distance <= 0.001f)
            {
                currentPercentage =
                    to;

                ApplyPercentage(
                    currentPercentage);

                UpdateAudioFade(1f);

                CompleteFade();

                yield break;
            }

            // -----------------------------------------------------
            // DURATION
            // -----------------------------------------------------

            float fadeDuration =
                CalculateDuration(
                    distance);

            float elapsed =
                0f;

            // -----------------------------------------------------
            // ANIMATION
            // -----------------------------------------------------

            while (elapsed < fadeDuration)
            {
                float deltaTime =
                    ignoreTimeScale
                        ? Time.unscaledDeltaTime
                        : Time.deltaTime;

                elapsed +=
                    deltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed /
                        fadeDuration);

                float eased =
                    EvaluateStyle(
                        normalized);

                currentPercentage =
                    Mathf.Lerp(
                        from,
                        to,
                        eased);

                ApplyPercentage(
                    currentPercentage);

                UpdateAudioFade(
                    normalized);

                yield return null;
            }

            // -----------------------------------------------------
            // FINAL VALUE
            // -----------------------------------------------------

            currentPercentage =
                to;

            ApplyPercentage(
                currentPercentage);

            UpdateAudioFade(
                1f);

            // -----------------------------------------------------
            // LOOP
            // -----------------------------------------------------

            if (loop)
            {
                if (loopDelay > 0f)
                {
                    yield return Wait(
                        loopDelay);
                }

                float nextTarget;

                if (Mathf.Approximately(
                    to,
                    0f))
                {
                    nextTarget = 100f;
                }
                else
                {
                    nextTarget = 0f;
                }

                fadeRoutine =
                    StartCoroutine(
                        FadeRoutine(
                            currentPercentage,
                            nextTarget,
                            0f));

                yield break;
            }

            // -----------------------------------------------------
            // COMPLETE
            // -----------------------------------------------------

            CompleteFade();
        }

        // =========================================================
        // DURATION
        // =========================================================

        private float CalculateDuration(
            float distance)
        {
            if (fadeMode ==
                FadeMode.Duration)
            {
                return Mathf.Max(
                    0.01f,
                    duration);
            }

            return Mathf.Max(
                0.01f,
                distance /
                Mathf.Max(
                    0.01f,
                    speed));
        }

        // =========================================================
        // STYLE
        // =========================================================

        private float EvaluateStyle(
            float value)
        {
            value =
                Mathf.Clamp01(
                    value);

            switch (style)
            {
                case FadeStyle.Linear:

                    return value;

                case FadeStyle.EaseIn:

                    return value *
                           value;

                case FadeStyle.EaseOut:

                    return 1f -
                           Mathf.Pow(
                               1f - value,
                               2f);

                case FadeStyle.EaseInOut:

                    return Mathf.SmoothStep(
                        0f,
                        1f,
                        value);

                case FadeStyle.SmoothStep:

                    return value *
                           value *
                           (3f -
                            2f * value);

                case FadeStyle.SmootherStep:

                    return value *
                           value *
                           value *
                           (
                               value *
                               (
                                   value *
                                   6f -
                                   15f
                               ) +
                               10f
                           );

                case FadeStyle.CustomCurve:

                    if (customCurve != null)
                    {
                        return Mathf.Clamp01(
                            customCurve.Evaluate(
                                value));
                    }

                    return value;

                default:

                    return value;
            }
        }

        // =========================================================
        // TARGET APPLICATION
        // =========================================================

        private void ApplyPercentage(
            float percentage)
        {
            float alpha =
                Mathf.Clamp01(
                    percentage /
                    100f);

            // -----------------------------------------------------
            // CANVAS GROUP
            // -----------------------------------------------------

            if (target ==
                    FadeTarget.CanvasGroup ||
                target ==
                    FadeTarget.CanvasGroupAndGraphic)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha =
                        alpha;
                }
            }

            // -----------------------------------------------------
            // GRAPHIC
            // -----------------------------------------------------

            if (target ==
                    FadeTarget.Graphic ||
                target ==
                    FadeTarget.CanvasGroupAndGraphic)
            {
                if (graphic != null)
                {
                    Color color =
                        graphic.color;

                    color.a =
                        alpha;

                    graphic.color =
                        color;
                }
            }
        }

        // =========================================================
        // READ CURRENT VALUE
        // =========================================================

        private float ReadCurrentPercentage()
        {
            if (target ==
                    FadeTarget.Graphic &&
                graphic != null)
            {
                return
                    graphic.color.a *
                    100f;
            }

            if (canvasGroup != null)
            {
                return
                    canvasGroup.alpha *
                    100f;
            }

            if (graphic != null)
            {
                return
                    graphic.color.a *
                    100f;
            }

            return 100f;
        }

        // =========================================================
        // AUDIO
        // =========================================================

        private void PlayFadeAudio(
            float from,
            float to)
        {
            if (!useAudio ||
                audioSource == null)
            {
                return;
            }

            bool fadingIn =
                to > from;

            AudioClip clip =
                fadingIn
                    ? fadeInClip
                    : fadeOutClip;

            if (clip == null)
            {
                return;
            }

            audioSource.clip =
                clip;

            if (fadeAudio)
            {
                audioSource.volume =
                    0f;
            }
            else
            {
                audioSource.volume =
                    audioVolume;
            }

            audioSource.Play();
        }

        private void UpdateAudioFade(
            float normalized)
        {
            if (!useAudio ||
                !fadeAudio ||
                audioSource == null ||
                !audioSource.isPlaying)
            {
                return;
            }

            audioSource.volume =
                Mathf.Lerp(
                    0f,
                    audioVolume,
                    normalized);
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        private void CompleteFade()
        {
            isFading =
                false;

            fadeRoutine =
                null;

            onFadeCompleted?.Invoke();
        }

        // =========================================================
        // STOP
        // =========================================================

        private void StopFadeInternal(
            bool invokeEvent)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(
                    fadeRoutine);

                fadeRoutine =
                    null;
            }

            bool wasFading =
                isFading;

            isFading =
                false;

            if (audioSource != null &&
                audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (invokeEvent &&
                wasFading)
            {
                onFadeStopped?.Invoke();
            }
        }

        // =========================================================
        // WAIT
        // =========================================================

        private IEnumerator Wait(
            float seconds)
        {
            if (seconds <= 0f)
            {
                yield break;
            }

            if (ignoreTimeScale)
            {
                float elapsed =
                    0f;

                while (elapsed < seconds)
                {
                    elapsed +=
                        Time.unscaledDeltaTime;

                    yield return null;
                }

                yield break;
            }

            yield return
                new WaitForSeconds(
                    seconds);
        }
    }
}