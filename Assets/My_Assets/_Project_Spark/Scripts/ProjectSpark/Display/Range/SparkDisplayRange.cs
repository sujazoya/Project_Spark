
using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayRange : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text rangeText;

        [SerializeField]
        private TMP_Text rangeValueText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)]
        private float fadeSpeed = 8f;

        private bool visible;
        private double maximum;
        private SparkDisplayUnit unit;

        public void SetRange(
            bool showRange,
            double maximumValue,
            SparkDisplayUnit displayUnit)
        {
            visible =
                showRange &&
                maximumValue > 0d &&
                !double.IsNaN(maximumValue) &&
                !double.IsInfinity(maximumValue);

            maximum =
                maximumValue;

            unit =
                displayUnit;

            RefreshText();
        }

        private void Awake()
        {
            fadeSpeed =
                Mathf.Max(
                    0.01f,
                    fadeSpeed);
        }

        private void Update()
        {
            if (canvasGroup == null)
                return;

            float target =
                visible
                    ? 1f
                    : 0f;

            canvasGroup.alpha =
                Mathf.MoveTowards(
                    canvasGroup.alpha,
                    target,
                    fadeSpeed *
                    Time.unscaledDeltaTime);
        }

        private void RefreshText()
        {
            if (!visible)
            {
                if (rangeText != null)
                    rangeText.text =
                        string.Empty;

                if (rangeValueText != null)
                    rangeValueText.text =
                        string.Empty;

                return;
            }

            if (rangeText != null)
                rangeText.text =
                    "RANGE";

            if (rangeValueText != null)
            {
                SparkDisplayValueData data =
                    new SparkDisplayValueData
                    {
                        valid = true,
                        value = maximum,
                        unit = unit,
                        precision = 2,
                        useEngineeringPrefixes = true,
                        customSuffix = string.Empty
                    };

                rangeValueText.text =
                    SparkDisplayFormatter.FormatValue(
                        data);
            }
        }
    }
}
