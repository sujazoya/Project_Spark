using System;

namespace ProjectSpark.Display
{
    public enum SparkDisplayUnit
    {
        None,
        V,
        A,
        Ohm,
        W,
        Hz,
        F,
        C,
        Percent,
        RPM,
        Seconds
    }

    public enum SparkDisplayMode
    {
        None,
        DC,
        AC,
        Auto,
        Resistance,
        Continuity,
        Diode,
        Frequency,
        Temperature,
        Power,
        Charge
    }

    public enum SparkDisplayState
    {
        Off,
        Standby,
        Initializing,
        Ready,
        Measuring,
        Charging,
        Full,
        Warning,
        Fault,
        Disconnected,
        OverRange,
        NoSignal
    }

    public enum SparkDisplayQuality
    {
        Unknown,
        Stable,
        Fluctuating,
        NoSignal,
        OverRange,
        Invalid
    }

    public enum SparkDisplayValueTransition
    {
        Instant,
        Smooth,
        Instrument,
        Rolling
    }

    public enum SparkDisplayTextAnimation
    {
        Instant,
        Reveal,
        Typewriter,
        Scramble,
        Scan,
        Flicker
    }

    public enum SparkDisplayStatusAnimation
    {
        Instant,
        Fade,
        Pulse,
        Flicker
    }

    [Serializable]
    public struct SparkDisplayValueData
    {
        public bool valid;
        public double value;
        public SparkDisplayUnit unit;
        public int precision;
        public bool useEngineeringPrefixes;
        public string customSuffix;

        public static SparkDisplayValueData Invalid =>
            new SparkDisplayValueData
            {
                valid = false,
                value = 0d,
                unit = SparkDisplayUnit.None,
                precision = 2,
                useEngineeringPrefixes = true,
                customSuffix = string.Empty
            };

        public bool IsEquivalentTo(
            SparkDisplayValueData other,
            double tolerance = 0.000001d)
        {
            if (valid != other.valid)
                return false;

            if (!valid)
                return true;

            if (unit != other.unit)
                return false;

            if (precision != other.precision)
                return false;

            if (useEngineeringPrefixes !=
                other.useEngineeringPrefixes)
            {
                return false;
            }

            if (!string.Equals(
                    customSuffix,
                    other.customSuffix,
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (double.IsNaN(value) ||
                double.IsNaN(other.value))
            {
                return
                    double.IsNaN(value) &&
                    double.IsNaN(other.value);
            }

            if (double.IsInfinity(value) ||
                double.IsInfinity(other.value))
            {
                return value.Equals(other.value);
            }

            double difference =
                Math.Abs(
                    value -
                    other.value);

            if (difference <= tolerance)
                return true;

            double magnitude =
                Math.Max(
                    Math.Abs(value),
                    Math.Abs(other.value));

            if (magnitude <= tolerance)
                return difference <= tolerance;

            return
                difference / magnitude <=
                tolerance;
        }
    }
}
