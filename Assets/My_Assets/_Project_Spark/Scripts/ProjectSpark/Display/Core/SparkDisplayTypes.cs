using System;
using UnityEngine;

namespace ProjectSpark.Display
{
    public enum SparkDisplayUnit
    {
        None, V, A, Ohm, W, Hz, F, C, Percent, RPM, Seconds, Volts, Amps, Ohms, Watts, Hertz, Farads
    }

    public enum SparkDisplayMode
    {
        None, DC, AC, Auto, Resistance, Continuity, Diode, Frequency, Temperature, Power, Charge
    }

    public enum SparkDisplayState
    {
        Off, Standby, Initializing, Ready, Measuring, Charging, Full,
        Warning, Fault, Disconnected, OverRange, NoSignal
    }

    public enum SparkDisplayQuality
    {
        Unknown, Stable, Fluctuating, NoSignal, OverRange, Invalid
    }

    public enum SparkDisplayValueTransition
    {
        Instant, Smooth, Instrument, Rolling
    }

    public enum SparkDisplayTextAnimation
    {
        Instant, Reveal, Typewriter, Scramble, Scan, Flicker
    }

    public enum SparkDisplayStatusAnimation
    {
        Instant, Fade, Pulse, Flicker
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
    }
}
