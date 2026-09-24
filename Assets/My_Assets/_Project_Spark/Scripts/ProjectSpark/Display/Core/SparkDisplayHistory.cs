using System;
using UnityEngine;

namespace ProjectSpark.Display
{
    [Serializable]
    public sealed class SparkDisplayHistory
    {
        [SerializeField, Min(2)] private int capacity = 256;
        private double[] values;
        private int count;
        private int writeIndex;

        public int Count => count;
        public int Capacity => capacity;

        public SparkDisplayHistory(int capacity = 256)
        {
            this.capacity = Mathf.Max(2, capacity);
            values = new double[this.capacity];
        }

        public void Configure(int newCapacity)
        {
            newCapacity = Mathf.Max(2, newCapacity);
            if (values != null && values.Length == newCapacity)
                return;

            capacity = newCapacity;
            values = new double[capacity];
            count = 0;
            writeIndex = 0;
        }

        public void Clear()
        {
            if (values != null)
                Array.Clear(values, 0, values.Length);
            count = 0;
            writeIndex = 0;
        }

        public void Add(double value)
        {
            if (values == null || values.Length != capacity)
                Configure(capacity);

            values[writeIndex] = value;
            writeIndex = (writeIndex + 1) % capacity;
            count = Mathf.Min(count + 1, capacity);
        }

        public double GetOrdered(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int oldest = count == capacity ? writeIndex : 0;
            return values[(oldest + index) % capacity];
        }
    }
}
