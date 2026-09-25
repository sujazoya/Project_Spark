
using System;
using System.Globalization;

namespace ProjectSpark.Display
{
    public static class SparkDisplayFormatter
    {
        public static string FormatValue(
            SparkDisplayValueData data)
        {
            if (!data.valid)
                return "----";

            if (double.IsNaN(data.value) ||
                double.IsInfinity(data.value))
            {
                return "----";
            }

            EngineeringScale scale =
                GetEngineeringScale(
                    data.value,
                    data.useEngineeringPrefixes);

            string number =
                scale.ScaledValue.ToString(
                    "F" +
                    Math.Max(
                        0,
                        data.precision),
                    CultureInfo.InvariantCulture);

            return number +
                   " " +
                   scale.Prefix +
                   UnitText(data.unit) +
                   (data.customSuffix ??
                    string.Empty);
        }

        public static string FormatNumber(
            double value,
            int precision)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return "----";
            }

            return value.ToString(
                "F" +
                Math.Max(
                    0,
                    precision),
                CultureInfo.InvariantCulture);
        }

        public static string FormatMinMaxAverage(
            SparkDisplayValueData reference,
            double minimum,
            double maximum,
            double average)
        {
            if (!reference.valid)
                return string.Empty;

            SparkDisplayValueData minimumData =
                reference;

            SparkDisplayValueData maximumData =
                reference;

            SparkDisplayValueData averageData =
                reference;

            minimumData.value =
                minimum;

            maximumData.value =
                maximum;

            averageData.value =
                average;

            return
                "MIN " +
                FormatValue(minimumData) +
                "   MAX " +
                FormatValue(maximumData) +
                "   AVG " +
                FormatValue(averageData);
        }

        public static string UnitText(
            SparkDisplayUnit unit)
        {
            switch (unit)
            {
                case SparkDisplayUnit.V:
                    return "V";

                case SparkDisplayUnit.A:
                    return "A";

                case SparkDisplayUnit.Ohm:
                    return "Ω";

                case SparkDisplayUnit.W:
                    return "W";

                case SparkDisplayUnit.Hz:
                    return "Hz";

                case SparkDisplayUnit.F:
                    return "F";

                case SparkDisplayUnit.C:
                    return "°C";

                case SparkDisplayUnit.Percent:
                    return "%";

                case SparkDisplayUnit.RPM:
                    return "RPM";

                case SparkDisplayUnit.Seconds:
                    return "s";

                default:
                    return string.Empty;
            }
        }

        public static string PrefixFor(
            double value,
            bool useEngineeringPrefixes)
        {
            return GetEngineeringScale(
                value,
                useEngineeringPrefixes).Prefix;
        }

        public static double ScaleValue(
            double value,
            bool useEngineeringPrefixes)
        {
            return GetEngineeringScale(
                value,
                useEngineeringPrefixes).ScaledValue;
        }

        private static EngineeringScale GetEngineeringScale(
            double value,
            bool useEngineeringPrefixes)
        {
            if (!useEngineeringPrefixes ||
                value == 0d ||
                double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return new EngineeringScale(
                    value,
                    string.Empty);
            }

            double absolute =
                Math.Abs(value);

            if (absolute >= 1e9)
            {
                return new EngineeringScale(
                    value / 1e9,
                    "G");
            }

            if (absolute >= 1e6)
            {
                return new EngineeringScale(
                    value / 1e6,
                    "M");
            }

            if (absolute >= 1e3)
            {
                return new EngineeringScale(
                    value / 1e3,
                    "k");
            }

            if (absolute >= 1d)
            {
                return new EngineeringScale(
                    value,
                    string.Empty);
            }

            if (absolute >= 1e-3)
            {
                return new EngineeringScale(
                    value * 1e3,
                    "m");
            }

            if (absolute >= 1e-6)
            {
                return new EngineeringScale(
                    value * 1e6,
                    "µ");
            }

            if (absolute >= 1e-9)
            {
                return new EngineeringScale(
                    value * 1e9,
                    "n");
            }

            return new EngineeringScale(
                value * 1e12,
                "p");
        }

        private readonly struct EngineeringScale
        {
            public readonly double ScaledValue;
            public readonly string Prefix;

            public EngineeringScale(
                double scaledValue,
                string prefix)
            {
                ScaledValue = scaledValue;
                Prefix = prefix;
            }
        }
    }
}
