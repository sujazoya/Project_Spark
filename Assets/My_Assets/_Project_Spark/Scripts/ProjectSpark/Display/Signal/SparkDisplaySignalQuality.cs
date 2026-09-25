using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplaySignalQuality : MonoBehaviour
    {
        [SerializeField]
        private Image fill;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)]
        private float responseSpeed = 8f;

        [SerializeField, Min(0f)]
        private float minimumVisibleQuality = 0.01f;

        private float targetQuality;
        private bool visible;

        public void SetQuality(
            float quality,
            bool showIndicator)
        {
            targetQuality =
                Mathf.Clamp01(
                    quality);

            visible =
                showIndicator &&
                targetQuality >=
                minimumVisibleQuality;
        }

        private void Awake()
        {
            responseSpeed =
                Mathf.Max(
                    0.01f,
                    responseSpeed);

            minimumVisibleQuality =
                Mathf.Clamp01(
                    minimumVisibleQuality);
        }

        private void Update()
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            if (fill != null)
            {
                fill.fillAmount =
                    Mathf.MoveTowards(
                        fill.fillAmount,
                        targetQuality,
                        responseSpeed *
                        deltaTime);
            }

            if (canvasGroup != null)
            {
                float targetAlpha =
                    visible
                        ? 1f
                        : 0f;

                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        targetAlpha,
                        responseSpeed *
                        deltaTime);
            }
        }
    }
}
