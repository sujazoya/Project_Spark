
using System;

namespace ProjectSpark.Display
{
    [Serializable]
    public struct SparkDisplayStatistics
    {
        public bool valid;
        public int sampleCount;

        public double minimum;
        public double maximum;
        public double average;

        public double peak;
        public double latest;

        public static SparkDisplayStatistics Invalid =>
            new SparkDisplayStatistics
            {
                valid = false,
                sampleCount = 0,

                minimum = 0d,
                maximum = 0d,
                average = 0d,

                peak = 0d,
                latest = 0d
            };
    }
}
