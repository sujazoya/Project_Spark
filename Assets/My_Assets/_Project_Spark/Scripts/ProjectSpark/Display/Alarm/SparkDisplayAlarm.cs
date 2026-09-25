using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayAlarm : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text alarmText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)]
        private float pulseSpeed = 7f;

        [SerializeField, Range(0f, 1f)]
        private float pulseAmount = 0.2f;

        [SerializeField, Min(0.01f)]
        private float fadeSpeed = 8f;

        private bool active;
        private bool critical;

        public void SetAlarm(
            bool enabled,
            bool criticalAlarm,
            string message)
        {
            active =
                enabled &&
                !string.IsNullOrWhiteSpace(
                    message);

            critical =
                criticalAlarm;

            if (alarmText != null)
            {
                alarmText.text =
                    active
                        ? message
                        : string.Empty;
            }
        }

        private void Awake()
        {
            pulseSpeed =
                Mathf.Max(
                    0.01f,
                    pulseSpeed);

            pulseAmount =
                Mathf.Clamp01(
                    pulseAmount);

            fadeSpeed =
                Mathf.Max(
                    0.01f,
                    fadeSpeed);
        }

        private void Update()
        {
            if (canvasGroup == null)
                return;

            float deltaTime =
                Time.unscaledDeltaTime;

            if (!active)
            {
                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        0f,
                        fadeSpeed *
                        deltaTime);

                return;
            }

            float pulse =
                Mathf.Abs(
                    Mathf.Sin(
                        Time.unscaledTime *
                        pulseSpeed));

            float targetAlpha;

            if (critical)
            {
                targetAlpha =
                    Mathf.Lerp(
                        0.35f,
                        1f,
                        pulse);
            }
            else
            {
                targetAlpha =
                    Mathf.Lerp(
                        1f - pulseAmount,
                        1f,
                        pulse);
            }

            canvasGroup.alpha =
                Mathf.MoveTowards(
                    canvasGroup.alpha,
                    targetAlpha,
                    fadeSpeed *
                    deltaTime);
        }
    }
}
