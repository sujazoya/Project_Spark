using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    public sealed class SparkDisplayStatusRenderer : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0.01f)] private float speed = 8f;
        [SerializeField] private float pulseSpeed = 8f;

        private SparkDisplayState state;
        private string text = string.Empty;

        public void SetStatus(SparkDisplayState newState, string customText = null)
        {
            state = newState;
            text = string.IsNullOrEmpty(customText) ? DefaultText(newState) : customText;

            if (statusText != null)
                statusText.text = text;
        }

        private void Update()
        {
            if (canvasGroup == null)
                return;

            if (state == SparkDisplayState.Warning || state == SparkDisplayState.Fault)
            {
                float target = 0.72f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed)) * 0.28f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, target, speed * Time.unscaledDeltaTime);
            }
            else
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, speed * Time.unscaledDeltaTime);
            }
        }

        private static string DefaultText(SparkDisplayState value)
        {
            switch (value)
            {
                case SparkDisplayState.Off: return "OFF";
                case SparkDisplayState.Standby: return "STANDBY";
                case SparkDisplayState.Initializing: return "INITIALIZING...";
                case SparkDisplayState.Ready: return "READY";
                case SparkDisplayState.Measuring: return "MEASURING";
                case SparkDisplayState.Charging: return "CHARGING";
                case SparkDisplayState.Full: return "FULL";
                case SparkDisplayState.Warning: return "WARNING";
                case SparkDisplayState.Fault: return "FAULT";
                case SparkDisplayState.Disconnected: return "DISCONNECTED";
                case SparkDisplayState.OverRange: return "OVER RANGE";
                case SparkDisplayState.NoSignal: return "NO SIGNAL";
                default: return string.Empty;
            }
        }
    }
}
