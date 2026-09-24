using System;
using System.Globalization;

namespace ProjectSpark.Display
{
    public static class SparkDisplayFormatter
    {
        public static string FormatValue(SparkDisplayValueData data)
        {
            if (!data.valid)
                return "----";

            double value = data.value;
            string prefix = string.Empty;
            double scaled = value;

            if (data.useEngineeringPrefixes && value != 0d)
            {
                double abs = Math.Abs(value);
                if (abs >= 1e9) { scaled = value / 1e9; prefix = "G"; }
                else if (abs >= 1e6) { scaled = value / 1e6; prefix = "M"; }
                else if (abs >= 1e3) { scaled = value / 1e3; prefix = "k"; }
                else if (abs < 1e-9) { scaled = value / 1e-12; prefix = "p"; }
                else if (abs < 1e-6) { scaled = value / 1e-9; prefix = "n"; }
                else if (abs < 1e-3) { scaled = value / 1e-6; prefix = "µ"; }
                else if (abs < 1) { scaled = value / 1e-3; prefix = "m"; }
            }

            string number = scaled.ToString("F" + Math.Max(0, data.precision), CultureInfo.InvariantCulture);
            return number + " " + prefix + UnitText(data.unit) + data.customSuffix;
        }

        public static string FormatNumber(double value, int precision)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "----";

            return value.ToString("F" + Math.Max(0, precision), CultureInfo.InvariantCulture);
        }

        public static string UnitText(SparkDisplayUnit unit)
        {
            switch (unit)
            {
                case SparkDisplayUnit.V:
                case SparkDisplayUnit.Volts: return "V";
                case SparkDisplayUnit.A:
                case SparkDisplayUnit.Amps: return "A";
                case SparkDisplayUnit.Ohm:
                case SparkDisplayUnit.Ohms: return "Ω";
                case SparkDisplayUnit.W:
                case SparkDisplayUnit.Watts: return "W";
                case SparkDisplayUnit.Hz:
                case SparkDisplayUnit.Hertz: return "Hz";
                case SparkDisplayUnit.F:
                case SparkDisplayUnit.Farads: return "F";
                case SparkDisplayUnit.C: return "°C";
                case SparkDisplayUnit.Percent: return "%";
                case SparkDisplayUnit.RPM: return "RPM";
                case SparkDisplayUnit.Seconds: return "s";
                default: return string.Empty;
            }
        }
    }
}
