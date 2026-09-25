using System;

namespace ProjectSpark.Display
{
    [Serializable]
    public sealed class SparkDisplayStatisticsTracker
    {
        private readonly SparkDisplayHistory history;
        private readonly SparkDisplayPeakHold peakHold;

        public SparkDisplayStatisticsTracker(
            int historyCapacity)
        {
            history =
                new SparkDisplayHistory(
                    historyCapacity);

            peakHold =
                new SparkDisplayPeakHold();
        }

        public int SampleCount =>
            history.Count;

        public void ConfigureHistory(
            int capacity)
        {
            history.Configure(capacity);
        }

        public void Clear()
        {
            history.Clear();
            peakHold.Reset();
        }

        public void Add(double value)
        {
            if (!IsFinite(value))
                return;

            history.Add(value);
            peakHold.Add(value);
        }

        public SparkDisplayStatistics GetStatistics()
        {
            if (history.Count == 0)
                return SparkDisplayStatistics.Invalid;

            return new SparkDisplayStatistics
            {
                valid = true,
                sampleCount = history.Count,
                minimum = history.Minimum,
                maximum = history.Maximum,
                average = history.Average,
                peak = peakHold.HasPeak
                    ? peakHold.Peak
                    : history.Maximum,
                latest = history.Latest
            };
        }

        public bool TryGetLatest(
            out double value)
        {
            if (history.Count == 0)
            {
                value = 0d;
                return false;
            }

            value = history.Latest;
            return true;
        }

        private static bool IsFinite(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
