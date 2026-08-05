using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIAnimationController :
        MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RectTransform animatedTarget;

        [Header("Open Animation")]
        [SerializeField]
        private float openDuration = 0.2f;

        [SerializeField]
        private Vector2 openOffset =
            new Vector2(0f, -30f);

        [Header("Close Animation")]
        [SerializeField]
        private float closeDuration = 0.15f;

        private Vector2 originalPosition;

        private Coroutine currentAnimation;

        private void Awake()
        {
            if (animatedTarget != null)
            {
                originalPosition =
                    animatedTarget.anchoredPosition;
            }
        }

        public void PlayOpen()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(
                    currentAnimation);
            }

            currentAnimation =
                StartCoroutine(
                    OpenRoutine());
        }

        public void PlayClose()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(
                    currentAnimation);
            }

            currentAnimation =
                StartCoroutine(
                    CloseRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (animatedTarget != null)
            {
                animatedTarget.anchoredPosition =
                    originalPosition +
                    openOffset;
            }

            float time = 0f;

            while (time < openDuration)
            {
                time += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        time /
                        openDuration);

                t = EaseOutCubic(t);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                }

                if (animatedTarget != null)
                {
                    animatedTarget.anchoredPosition =
                        Vector2.LerpUnclamped(
                            originalPosition +
                            openOffset,
                            originalPosition,
                            t);
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (animatedTarget != null)
            {
                animatedTarget.anchoredPosition =
                    originalPosition;
            }

            currentAnimation = null;
        }

        private IEnumerator CloseRoutine()
        {
            float startAlpha =
                canvasGroup != null
                    ? canvasGroup.alpha
                    : 1f;

            Vector2 startPosition =
                animatedTarget != null
                    ? animatedTarget.anchoredPosition
                    : Vector2.zero;

            float time = 0f;

            while (time < closeDuration)
            {
                time +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        time /
                        closeDuration);

                t = EaseInCubic(t);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha =
                        Mathf.Lerp(
                            startAlpha,
                            0f,
                            t);
                }

                if (animatedTarget != null)
                {
                    animatedTarget.anchoredPosition =
                        Vector2.Lerp(
                            startPosition,
                            startPosition +
                            openOffset * 0.5f,
                            t);
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            currentAnimation = null;
        }

        private static float EaseOutCubic(
            float t)
        {
            return 1f -
                Mathf.Pow(
                    1f - t,
                    3f);
        }

        private static float EaseInCubic(
            float t)
        {
            return t * t * t;
        }
    }
}