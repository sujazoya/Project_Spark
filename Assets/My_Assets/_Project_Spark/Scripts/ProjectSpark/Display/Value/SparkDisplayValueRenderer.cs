using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    public sealed class SparkDisplayValueRenderer : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private SparkDisplayValueTransition transition = SparkDisplayValueTransition.Instrument;
        [SerializeField, Min(0.01f)] private float speed = 10f;
        [SerializeField, Min(0f)] private float deadband = 0.001f;
        [SerializeField, Min(0f)] private float instrumentNoise = 0.01f;

        private double targetValue;
        private double visualValue;
        private bool initialized;
        private int precision = 2;
        private SparkDisplayUnit unit = SparkDisplayUnit.None;

        public void Configure(SparkDisplayValueTransition mode, float transitionSpeed, float deadbandValue, float noise)
        {
            transition = mode;
            speed = Mathf.Max(0.01f, transitionSpeed);
            deadband = Mathf.Max(0f, deadbandValue);
            instrumentNoise = Mathf.Max(0f, noise);
        }

        public void SetValue(SparkDisplayValueData data)
        {
            if (!data.valid)
            {
                if (valueText != null)
                    valueText.text = "----";
                initialized = false;
                return;
            }

            targetValue = data.value;
            precision = Mathf.Clamp(data.precision, 0, 6);
            unit = data.unit;

            if (!initialized || transition == SparkDisplayValueTransition.Instant)
            {
                visualValue = targetValue;
                initialized = true;
                Refresh();
            }
        }

        private void Update()
        {
            if (!initialized)
                return;

            if (transition == SparkDisplayValueTransition.Instant)
                return;

            double difference = targetValue - visualValue;

            if (Mathf.Abs((float)difference) <= deadband)
            {
                visualValue = targetValue;
            }
            else if (transition == SparkDisplayValueTransition.Smooth ||
                     transition == SparkDisplayValueTransition.Rolling)
            {
                visualValue = Mathf.Lerp((float)visualValue, (float)targetValue,
                    1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            }
            else
            {
                float blend = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
                visualValue = Mathf.Lerp((float)visualValue, (float)targetValue, blend);

                if (instrumentNoise > 0f && Mathf.Abs((float)difference) > deadband)
                    visualValue += Random.Range(-instrumentNoise, instrumentNoise);
            }

            Refresh();
        }

        private void Refresh()
        {
            if (valueText == null)
                return;

            SparkDisplayValueData data = new SparkDisplayValueData
            {
                valid = true,
                value = visualValue,
                unit = unit,
                precision = precision,
                useEngineeringPrefixes = false
            };

            string text = SparkDisplayFormatter.FormatValue(data);
            int separator = text.IndexOf(' ');
            valueText.text = separator >= 0 ? text.Substring(0, separator) : text;
        }
    }
}
