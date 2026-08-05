using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIAnimationService :
        MonoBehaviour
    {
        public Coroutine Play(
            RectTransform target,
            CanvasGroup canvasGroup,
            UIAnimationProfile profile)
        {
            if (target == null ||
                profile == null)
            {
                return null;
            }

            return StartCoroutine(
                Animate(
                    target,
                    canvasGroup,
                    profile));
        }

        public void Stop(
            Coroutine animation)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }

        private IEnumerator Animate(
            RectTransform target,
            CanvasGroup canvasGroup,
            UIAnimationProfile profile)
        {
            if (profile.delay > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    profile.delay);
            }

            Vector3 initialPosition =
                target.localPosition;

            Vector3 initialScale =
                target.localScale;

            float elapsed = 0f;

            while (elapsed <
                   profile.duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float normalized =
                    profile.duration <= 0f
                        ? 1f
                        : Mathf.Clamp01(
                            elapsed /
                            profile.duration);

                float eased =
                    profile.curve.Evaluate(
                        normalized);

                target.localPosition =
                    Vector3.LerpUnclamped(
                        initialPosition +
                        (Vector3)
                        profile.startOffset,

                        initialPosition +
                        (Vector3)
                        profile.endOffset,

                        eased);

                target.localScale =
                    Vector3.LerpUnclamped(
                        profile.startScale,
                        profile.endScale,
                        eased);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha =
                        Mathf.Lerp(
                            profile.startAlpha,
                            profile.endAlpha,
                            eased);
                }

                yield return null;
            }

            target.localPosition =
                initialPosition +
                (Vector3)
                profile.endOffset;

            target.localScale =
                profile.endScale;

            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    profile.endAlpha;
            }
        }
    }
}