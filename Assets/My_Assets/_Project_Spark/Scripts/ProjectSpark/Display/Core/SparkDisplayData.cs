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

        public bool IsEquivalentTo(
            SparkDisplayData other,
            double valueTolerance = 0.000001d)
        {
            if (other == null)
                return false;

            return
                primary.IsEquivalentTo(other.primary, valueTolerance) &&
                secondary.IsEquivalentTo(other.secondary, valueTolerance) &&
                tertiary.IsEquivalentTo(other.tertiary, valueTolerance) &&

                mode == other.mode &&
                state == other.state &&
                quality == other.quality &&

                Approximately(minimum, other.minimum, valueTolerance) &&
                Approximately(maximum, other.maximum, valueTolerance) &&
                Approximately(average, other.average, valueTolerance) &&

                hasMinMaxAverage == other.hasMinMaxAverage &&
                Mathf.Approximately(normalizedBar, other.normalizedBar) &&
                hasBar == other.hasBar &&

                hasRange == other.hasRange &&
                Approximately(rangeMaximum, other.rangeMaximum, valueTolerance) &&

                string.Equals(primaryLabel, other.primaryLabel, StringComparison.Ordinal) &&
                string.Equals(secondaryLabel, other.secondaryLabel, StringComparison.Ordinal) &&
                string.Equals(statusText, other.statusText, StringComparison.Ordinal) &&
                string.Equals(notificationText, other.notificationText, StringComparison.Ordinal) &&

                hasNotification == other.hasNotification &&
                criticalNotification == other.criticalNotification &&

                Mathf.Approximately(signalQuality, other.signalQuality) &&

                hasGraphValue == other.hasGraphValue &&
                Approximately(graphValue, other.graphValue, valueTolerance);
        }

        public static SparkDisplayData CreateDefault()
        {
            return new SparkDisplayData
            {
                primary = SparkDisplayValueData.Invalid,
                secondary = SparkDisplayValueData.Invalid,
                tertiary = SparkDisplayValueData.Invalid,
                mode = SparkDisplayMode.None,
                state = SparkDisplayState.Off,
                quality = SparkDisplayQuality.Unknown,
                signalQuality = 0f
            };
        }

        private static bool Approximately(
            double a,
            double b,
            double tolerance)
        {
            if (double.IsNaN(a) || double.IsNaN(b))
                return double.IsNaN(a) && double.IsNaN(b);

            if (double.IsInfinity(a) || double.IsInfinity(b))
                return a.Equals(b);

            double difference = Math.Abs(a - b);

            if (difference <= tolerance)
                return true;

            double magnitude = Math.Max(
                Math.Abs(a),
                Math.Abs(b));

            if (magnitude <= tolerance)
                return difference <= tolerance;

            return difference / magnitude <= tolerance;
        }
    }
}