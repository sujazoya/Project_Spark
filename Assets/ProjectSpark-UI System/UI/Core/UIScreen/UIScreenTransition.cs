using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Lightweight unscaled CanvasGroup fade used by UI screen transitions.
    /// </summary>
    public sealed class UIScreenTransition : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        [Min(0f)]
        private float fadeDuration = 0.25f;

        public IEnumerator FadeIn()
        {
            yield return Fade(0f, 1f);
        }

        public IEnumerator FadeOut()
        {
            yield return Fade(1f, 0f);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            if (fadeDuration <= 0f)
            {
                canvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(from, to, progress);

                yield return null;
            }

            canvasGroup.alpha = to;
        }
    }
}
