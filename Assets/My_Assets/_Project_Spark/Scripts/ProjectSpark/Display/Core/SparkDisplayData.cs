using System;
using UnityEngine;

namespace ProjectSpark.Display
{
    [Serializable]
    public sealed class SparkDisplayData
    {
        public SparkDisplayValueData primary;
        public SparkDisplayValueData secondary;
        public SparkDisplayValueData tertiary;

        public SparkDisplayMode mode = SparkDisplayMode.None;
        public SparkDisplayState state = SparkDisplayState.Ready;
        public SparkDisplayQuality quality = SparkDisplayQuality.Unknown;

        public double minimum;
        public double maximum;
        public double average;
        public bool hasMinMaxAverage;

        [Range(0f, 1f)]
        public float normalizedBar;

        public bool hasBar;
        public bool hasRange;
        public double rangeMaximum;

        public string primaryLabel = string.Empty;
        public string secondaryLabel = string.Empty;
        public string statusText = string.Empty;
        public string notificationText = string.Empty;

        public bool hasNotification;
        public bool criticalNotification;

        [Range(0f, 1f)]
        public float signalQuality = 1f;

        public double graphValue;
        public bool hasGraphValue;

        public SparkDisplayData Clone()
        {
            return new SparkDisplayData
            {
                primary = primary,
                secondary = secondary,
                tertiary = tertiary,
                mode = mode,
                state = state,
                quality = quality,
                minimum = minimum,
                maximum = maximum,
                average = average,
                hasMinMaxAverage = hasMinMaxAverage,
                normalizedBar = normalizedBar,
                hasBar = hasBar,
                hasRange = hasRange,
                rangeMaximum = rangeMaximum,
                primaryLabel = primaryLabel,
                secondaryLabel = secondaryLabel,
                statusText = statusText,
                notificationText = notificationText,
                hasNotification = hasNotification,
                criticalNotification = criticalNotification,
                signalQuality = signalQuality,
                graphValue = graphValue,
                hasGraphValue = hasGraphValue
            };
        }

        public static SparkDisplayData CreateDefault()
        {
            return new SparkDisplayData
            {
                primary = SparkDisplayValueData.Invalid,
                secondary = SparkDisplayValueData.Invalid,
                tertiary = SparkDisplayValueData.Invalid
            };
        }
    }
}
