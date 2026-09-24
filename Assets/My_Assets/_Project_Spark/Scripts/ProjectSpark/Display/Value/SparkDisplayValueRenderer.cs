using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    public sealed class SparkDisplayValueRenderer : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;

        [Header("Transition")]
        [SerializeField]
        private SparkDisplayValueTransition transition =
            SparkDisplayValueTransition.Instrument;

        [SerializeField, Min(0.01f)]
        private float speed = 10f;

        [SerializeField, Min(0f)]
        private float deadband = 0.001f;

        [SerializeField, Min(0f)]
        private float instrumentNoise = 0.01f;

        [Header("Engineering")]
        [SerializeField]
        private TMP_Text prefixText;

        [SerializeField]
        private TMP_Text unitText;

        private double targetValue;
        private double visualValue;

        private bool initialized;

        private int precision = 2;
        private SparkDisplayUnit unit;
        private bool useEngineeringPrefixes;
        private string customSuffix = string.Empty;

        private float rollingTimer;
        private float rollingDuration = 0.12f;

        private double lastNoise;

        public void Configure(
            SparkDisplayValueTransition mode,
            float transitionSpeed,
            float deadbandValue,
            float noise,
            float rollingDurationValue = 0.12f)
        {
            transition = mode;
            speed = Mathf.Max(0.01f, transitionSpeed);
           deadband = Mathf.Max(0f, (float)deadbandValue);
            instrumentNoise = Mathf.Max(0f, noise);
            rollingDuration = Mathf.Max(
                0.01f,
                rollingDurationValue);
        }

        public void SetValue(
            SparkDisplayValueData data)
        {
            if (!data.valid ||
                double.IsNaN(data.value) ||
                double.IsInfinity(data.value))
            {
                initialized = false;

                if (valueText != null)
                    valueText.text = "----";

                if (prefixText != null)
                    prefixText.text = string.Empty;

                if (unitText != null)
                    unitText.text = string.Empty;

                return;
            }

            targetValue = data.value;
            precision = Mathf.Clamp(data.precision, 0, 8);
            unit = data.unit;
            useEngineeringPrefixes =
                data.useEngineeringPrefixes;
            customSuffix =
                data.customSuffix ?? string.Empty;

            if (!initialized)
            {
                visualValue = targetValue;
                initialized = true;
                rollingTimer = 0f;
                Refresh();
                return;
            }

            if (transition ==
                SparkDisplayValueTransition.Instant)
            {
                visualValue = targetValue;
                Refresh();
            }
            else if (transition ==
                     SparkDisplayValueTransition.Rolling)
            {
                rollingTimer = 0f;
            }
        }

        private void Update()
        {
            if (!initialized)
                return;

            double difference =
                targetValue - visualValue;

            if (System.Math.Abs(difference) <= deadband)
            {
                visualValue = targetValue;
                Refresh();
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;

            switch (transition)
            {
                case SparkDisplayValueTransition.Instant:
                    visualValue = targetValue;
                    break;

                case SparkDisplayValueTransition.Smooth:
                    visualValue = SmoothValue(
                        visualValue,
                        targetValue,
                        speed,
                        deltaTime);
                    break;

                case SparkDisplayValueTransition.Instrument:
                    visualValue = SmoothValue(
                        visualValue,
                        targetValue,
                        speed,
                        deltaTime);

                    if (instrumentNoise > 0f)
                        visualValue += CalculateNoise();
                    break;

                case SparkDisplayValueTransition.Rolling:
                    rollingTimer += deltaTime;

                    float duration01 =
                        Mathf.Clamp01(
                            rollingTimer / rollingDuration);

                    float eased =
                        1f -
                        Mathf.Pow(1f - duration01, 3f);

                    visualValue =
                        targetValue -
                        (targetValue - visualValue) *
                        (1d - eased);

                    break;
            }

            Refresh();
        }

        private double CalculateNoise()
        {
            double time =
                Time.unscaledTime * 17.371;

            double noise =
                System.Math.Sin(time) *
                0.65 +
                System.Math.Sin(time * 2.713) *
                0.35;

            lastNoise =
                noise * instrumentNoise;

            return lastNoise;
        }

        private static double SmoothValue(
            double current,
            double target,
            float smoothing,
            float deltaTime)
        {
            double blend =
                1d -
                System.Math.Exp(
                    -smoothing * deltaTime);

            return current +
                   (target - current) * blend;
        }

        private void Refresh()
        {
            if (valueText == null)
                return;

            double scaled =
                SparkDisplayFormatter.ScaleValue(
                    visualValue,
                    useEngineeringPrefixes);

            string number =
                SparkDisplayFormatter.FormatNumber(
                    scaled,
                    precision);

            string prefix =
                SparkDisplayFormatter.PrefixFor(
                    visualValue,
                    useEngineeringPrefixes);

            string unitTextValue =
                SparkDisplayFormatter.UnitText(unit);

            valueText.text = number;

            if (prefixText != null)
                prefixText.text = prefix;

            if (unitText != null)
            {
                unitText.text =
                    prefixText == null
                        ? prefix + unitTextValue + customSuffix
                        : unitTextValue + customSuffix;
            }
        }
    }
}