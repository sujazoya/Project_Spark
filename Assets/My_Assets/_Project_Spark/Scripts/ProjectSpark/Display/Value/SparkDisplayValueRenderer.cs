using System;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayValueRenderer : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text valueText;

        [Header("Engineering Prefix")]
        [SerializeField]
        private TMP_Text prefixText;

        [SerializeField]
        private TMP_Text unitText;

        [Header("Transition")]
        [SerializeField]
        private SparkDisplayValueTransition transition =
            SparkDisplayValueTransition.Instrument;

        [SerializeField, Min(0.01f)]
        private float speed = 10f;

        [SerializeField, Min(0f)]
        private double deadband = 0.001d;

        [SerializeField, Min(0f)]
        private float instrumentNoise = 0.01f;

        [SerializeField, Min(0.01f)]
        private float rollingDuration = 0.12f;

        private double targetValue;
        private double visualValue;

        private bool initialized;

        private int precision = 2;
        private SparkDisplayUnit unit;
        private bool useEngineeringPrefixes;
        private string customSuffix = string.Empty;

        private double rollingStartValue;
        private double rollingTargetValue;
        private float rollingTimer;

        public void Configure(
            SparkDisplayValueTransition mode,
            float transitionSpeed,
            double deadbandValue,
            float noise,
            float rollingDurationValue)
        {
            transition = mode;

            speed =
                Mathf.Max(
                    0.01f,
                    transitionSpeed);

            deadband =
                Math.Max(
                    0d,
                    deadbandValue);

            instrumentNoise =
                Mathf.Max(
                    0f,
                    noise);

            rollingDuration =
                Mathf.Max(
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
                ClearVisual();

                return;
            }

            bool valueChanged =
                !initialized ||
                !ApproximatelyEqual(
                    targetValue,
                    data.value);

            targetValue =
                data.value;

            precision =
                Mathf.Clamp(
                    data.precision,
                    0,
                    8);

            unit =
                data.unit;

            useEngineeringPrefixes =
                data.useEngineeringPrefixes;

            customSuffix =
                data.customSuffix ??
                string.Empty;

            if (!initialized)
            {
                initialized = true;

                visualValue =
                    targetValue;

                rollingStartValue =
                    targetValue;

                rollingTargetValue =
                    targetValue;

                rollingTimer =
                    rollingDuration;

                Refresh();

                return;
            }

            if (!valueChanged)
            {
                Refresh();

                return;
            }

            switch (transition)
            {
                case SparkDisplayValueTransition.Instant:
                    visualValue =
                        targetValue;

                    break;

                case SparkDisplayValueTransition.Rolling:
                    rollingStartValue =
                        visualValue;

                    rollingTargetValue =
                        targetValue;

                    rollingTimer = 0f;

                    break;
            }

            if (transition ==
                SparkDisplayValueTransition.Instant)
            {
                Refresh();
            }
        }

        private void Update()
        {
            if (!initialized)
                return;

            float deltaTime =
                Time.unscaledDeltaTime;

            switch (transition)
            {
                case SparkDisplayValueTransition.Instant:
                    visualValue =
                        targetValue;

                    break;

                case SparkDisplayValueTransition.Smooth:
                    visualValue =
                        SmoothValue(
                            visualValue,
                            targetValue,
                            speed,
                            deltaTime);

                    break;

                case SparkDisplayValueTransition.Instrument:
                    visualValue =
                        SmoothValue(
                            visualValue,
                            targetValue,
                            speed,
                            deltaTime);

                    if (instrumentNoise > 0f)
                    {
                        visualValue +=
                            CalculateInstrumentNoise();
                    }

                    break;

                case SparkDisplayValueTransition.Rolling:
                    UpdateRolling(
                        deltaTime);

                    break;
            }

            if (Math.Abs(
                    targetValue -
                    visualValue) <=
                deadband)
            {
                visualValue =
                    targetValue;
            }

            Refresh();
        }

        private void UpdateRolling(
            float deltaTime)
        {
            rollingTimer +=
                deltaTime;

            float normalized =
                Mathf.Clamp01(
                    rollingTimer /
                    rollingDuration);

            float eased =
                1f -
                Mathf.Pow(
                    1f - normalized,
                    3f);

            visualValue =
                rollingStartValue +
                (
                    rollingTargetValue -
                    rollingStartValue
                ) *
                eased;

            if (normalized >= 1f)
            {
                visualValue =
                    rollingTargetValue;
            }
        }

        private double CalculateInstrumentNoise()
        {
            double time =
                Time.unscaledTime;

            double noise =
                Math.Sin(
                    time * 17.371d) *
                0.55d +

                Math.Sin(
                    time * 7.173d) *
                0.30d +

                Math.Sin(
                    time * 31.217d) *
                0.15d;

            return
                noise *
                instrumentNoise;
        }

        private static double SmoothValue(
            double current,
            double target,
            float smoothing,
            float deltaTime)
        {
            double blend =
                1d -
                Math.Exp(
                    -smoothing *
                    deltaTime);

            return
                current +
                (target - current) *
                blend;
        }

        private void Refresh()
        {
            if (valueText == null)
                return;

            double scaledValue =
                SparkDisplayFormatter.ScaleValue(
                    visualValue,
                    useEngineeringPrefixes);

            string number =
                SparkDisplayFormatter.FormatNumber(
                    scaledValue,
                    precision);

            string prefix =
                SparkDisplayFormatter.PrefixFor(
                    visualValue,
                    useEngineeringPrefixes);

            string unitString =
                SparkDisplayFormatter.UnitText(
                    unit);

            valueText.text =
                number;

            if (prefixText != null)
            {
                prefixText.text =
                    prefix;
            }

            if (unitText != null)
            {
                if (prefixText != null)
                {
                    unitText.text =
                        unitString +
                        customSuffix;
                }
                else
                {
                    unitText.text =
                        prefix +
                        unitString +
                        customSuffix;
                }
            }
        }

        private void ClearVisual()
        {
            if (valueText != null)
                valueText.text = "----";

            if (prefixText != null)
            {
                prefixText.text =
                    string.Empty;
            }

            if (unitText != null)
            {
                unitText.text =
                    string.Empty;
            }
        }

        private bool ApproximatelyEqual(
            double a,
            double b)
        {
            if (double.IsNaN(a) ||
                double.IsNaN(b))
            {
                return false;
            }

            if (double.IsInfinity(a) ||
                double.IsInfinity(b))
            {
                return a.Equals(b);
            }

            double difference =
                Math.Abs(a - b);

            if (difference <= deadband)
                return true;

            double magnitude =
                Math.Max(
                    Math.Abs(a),
                    Math.Abs(b));

            if (magnitude <= deadband)
                return difference <= deadband;

            return
                difference / magnitude <=
                deadband;
        }
    }
}
