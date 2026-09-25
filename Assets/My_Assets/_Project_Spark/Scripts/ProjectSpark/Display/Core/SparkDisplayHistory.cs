using System;
using UnityEngine;

namespace ProjectSpark.Display
{
    [Serializable]
    public sealed class SparkDisplayHistory
    {
        [SerializeField, Min(2)]
        private int capacity = 256;

        private double[] values;
        private int count;
        private int writeIndex;
        private double sum;
        private double minimum;
        private double maximum;

        public int Count => count;

        public int Capacity => capacity;

        public double Latest =>
            count > 0
                ? values[
                    (writeIndex - 1 + values.Length) %
                    values.Length]
                : 0d;

        public double Minimum =>
            count > 0
                ? minimum
                : 0d;

        public double Maximum =>
            count > 0
                ? maximum
                : 0d;

        public double Average =>
            count > 0
                ? sum / count
                : 0d;

        public SparkDisplayHistory(
            int capacity = 256)
        {
            this.capacity =
                Mathf.Max(2, capacity);

            values =
                new double[this.capacity];
        }

        public void Configure(
            int newCapacity)
        {
            newCapacity =
                Mathf.Max(2, newCapacity);

            if (values != null &&
                values.Length == newCapacity)
            {
                capacity =
                    newCapacity;

                return;
            }

            if (values == null ||
                count == 0)
            {
                capacity =
                    newCapacity;

                values =
                    new double[capacity];

                count = 0;
                writeIndex = 0;
                sum = 0d;
                minimum = 0d;
                maximum = 0d;

                return;
            }

            int samplesToKeep =
                Mathf.Min(
                    count,
                    newCapacity);

            double[] newValues =
                new double[newCapacity];

            int firstIndex =
                count == capacity
                    ? writeIndex
                    : 0;

            int skip =
                count -
                samplesToKeep;

            for (int i = 0;
                 i < samplesToKeep;
                 i++)
            {
                int sourceIndex =
                    (firstIndex +
                     skip +
                     i) %
                    capacity;

                newValues[i] =
                    values[sourceIndex];
            }

            capacity =
                newCapacity;

            values =
                newValues;

            count =
                samplesToKeep;

            writeIndex =
                samplesToKeep %
                capacity;

            RecalculateStatistics();
        }

        public void Clear()
        {
            if (values != null)
            {
                Array.Clear(
                    values,
                    0,
                    values.Length);
            }

            count = 0;
            writeIndex = 0;
            sum = 0d;
            minimum = 0d;
            maximum = 0d;
        }

        public void Add(double value)
        {
            if (!IsFinite(value))
                return;

            EnsureStorage();

            bool replacing =
                count == capacity;

            if (replacing)
            {
                double removed =
                    values[writeIndex];

                sum -= removed;
            }
            else
            {
                count++;
            }

            values[writeIndex] =
                value;

            writeIndex++;

            if (writeIndex >= capacity)
                writeIndex = 0;

            sum += value;

            if (count == 1)
            {
                minimum = value;
                maximum = value;
                return;
            }

            if (replacing)
            {
                RecalculateExtrema();
                return;
            }

            if (value < minimum)
                minimum = value;

            if (value > maximum)
                maximum = value;
        }

        public double GetOrdered(int index)
        {
            if (index < 0 ||
                index >= count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index));
            }

            int oldest =
                count == capacity
                    ? writeIndex
                    : 0;

            return values[
                (oldest + index) %
                capacity];
        }

        public bool TryGetStatistics(
            out double minimumValue,
            out double maximumValue,
            out double averageValue)
        {
            if (count == 0)
            {
                minimumValue = 0d;
                maximumValue = 0d;
                averageValue = 0d;

                return false;
            }

            minimumValue = minimum;
            maximumValue = maximum;
            averageValue = sum / count;

            return true;
        }

        private void RecalculateStatistics()
        {
            if (count <= 0)
            {
                sum = 0d;
                minimum = 0d;
                maximum = 0d;

                return;
            }

            sum = 0d;

            double first =
                GetOrdered(0);

            minimum = first;
            maximum = first;

            for (int i = 0;
                 i < count;
                 i++)
            {
                double value =
                    GetOrdered(i);

                sum += value;

                if (value < minimum)
                    minimum = value;

                if (value > maximum)
                    maximum = value;
            }
        }

        private void RecalculateExtrema()
        {
            if (count <= 0)
            {
                minimum = 0d;
                maximum = 0d;

                return;
            }

            double first =
                GetOrdered(0);

            minimum = first;
            maximum = first;

            for (int i = 1;
                 i < count;
                 i++)
            {
                double value =
                    GetOrdered(i);

                if (value < minimum)
                    minimum = value;

                if (value > maximum)
                    maximum = value;
            }
        }

        private void EnsureStorage()
        {
            if (values != null &&
                values.Length == capacity)
            {
                return;
            }

            Configure(capacity);
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
