
using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayStatusRenderer : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)]
        private float speed = 8f;

        [SerializeField, Min(0.01f)]
        private float pulseSpeed = 8f;

        private SparkDisplayState state =
            SparkDisplayState.Off;

        private SparkDisplayStatusAnimation animationMode =
            SparkDisplayStatusAnimation.Fade;

        public void Configure(
            SparkDisplayStatusAnimation mode,
            float animationSpeed,
            float pulseSpeedValue)
        {
            animationMode =
                mode;

            speed =
                Mathf.Max(
                    0.01f,
                    animationSpeed);

            pulseSpeed =
                Mathf.Max(
                    0.01f,
                    pulseSpeedValue);

            if (canvasGroup != null &&
                animationMode ==
                    SparkDisplayStatusAnimation.Instant)
            {
                canvasGroup.alpha =
                    1f;
            }
        }

        public void SetStatus(
            SparkDisplayState newState,
            string customText = null)
        {
            state =
                newState;

            string text =
                string.IsNullOrEmpty(customText)
                    ? DefaultText(newState)
                    : customText;

            if (statusText != null)
            {
                statusText.text =
                    text;
            }

            if (canvasGroup == null)
                return;

            if (animationMode ==
                SparkDisplayStatusAnimation.Instant)
            {
                canvasGroup.alpha =
                    1f;
            }
        }

        private void Update()
        {
            if (canvasGroup == null)
                return;

            float deltaTime =
                Time.unscaledDeltaTime;

            switch (animationMode)
            {
                case SparkDisplayStatusAnimation.Instant:

                    canvasGroup.alpha =
                        1f;

                    break;

                case SparkDisplayStatusAnimation.Fade:

                    canvasGroup.alpha =
                        Mathf.MoveTowards(
                            canvasGroup.alpha,
                            1f,
                            speed *
                            deltaTime);

                    break;

                case SparkDisplayStatusAnimation.Pulse:

                    UpdatePulse(
                        deltaTime,
                        false);

                    break;

                case SparkDisplayStatusAnimation.Flicker:

                    UpdatePulse(
                        deltaTime,
                        true);

                    break;
            }
        }

        private void UpdatePulse(
            float deltaTime,
            bool flicker)
        {
            bool active =
                state ==
                    SparkDisplayState.Warning ||

                state ==
                    SparkDisplayState.Fault ||

                state ==
                    SparkDisplayState.OverRange ||

                state ==
                    SparkDisplayState.NoSignal;

            float target =
                1f;

            if (active)
            {
                float wave =
                    Mathf.Abs(
                        Mathf.Sin(
                            Time.unscaledTime *
                            pulseSpeed));

                if (flicker)
                {
                    target =
                        wave > 0.78f
                            ? 0.35f
                            : 1f;
                }
                else
                {
                    target =
                        Mathf.Lerp(
                            0.72f,
                            1f,
                            wave);
                }
            }

            canvasGroup.alpha =
                Mathf.MoveTowards(
                    canvasGroup.alpha,
                    target,
                    speed *
                    deltaTime);
        }

        private static string DefaultText(
            SparkDisplayState value)
        {
            switch (value)
            {
                case SparkDisplayState.Off:
                    return "OFF";

                case SparkDisplayState.Standby:
                    return "STANDBY";

                case SparkDisplayState.Initializing:
                    return "INITIALIZING...";

                case SparkDisplayState.Ready:
                    return "READY";

                case SparkDisplayState.Measuring:
                    return "MEASURING";

                case SparkDisplayState.Charging:
                    return "CHARGING";

                case SparkDisplayState.Full:
                    return "FULL";

                case SparkDisplayState.Warning:
                    return "WARNING";

                case SparkDisplayState.Fault:
                    return "FAULT";

                case SparkDisplayState.Disconnected:
                    return "DISCONNECTED";

                case SparkDisplayState.OverRange:
                    return "OVER RANGE";

                case SparkDisplayState.NoSignal:
                    return "NO SIGNAL";

                default:
                    return string.Empty;
            }
        }
    }
}
