using System;

namespace ProjectSpark.Display
{
    [Serializable]
    public sealed class SparkDisplayPeakHold
    {
        private bool hasPeak;
        private double peak;

        public bool HasPeak => hasPeak;

        public double Peak => peak;

        public void Reset()
        {
            hasPeak = false;
            peak = 0d;
        }

        public void Add(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return;
            }

            if (!hasPeak ||
                value > peak)
            {
                peak = value;
                hasPeak = true;
            }
        }

        public bool TryGetPeak(
            out double value)
        {
            value = peak;

            return hasPeak;
        }
    }
}
