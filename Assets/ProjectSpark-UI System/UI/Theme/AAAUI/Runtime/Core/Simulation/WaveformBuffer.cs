using UnityEngine;

namespace AAAUI
{
    /// <summary>
    /// Lightweight circular buffer for real-time
    /// electronic waveform visualization.
    /// </summary>
    [System.Serializable]
    public sealed class WaveformBuffer
    {
        [SerializeField, Min(2)]
        private int capacity = 512;

        private float[] samples;

        private int writeIndex;
        private int sampleCount;

        public int Capacity => capacity;

        public int Count =>
            sampleCount;

        public WaveformBuffer()
        {
            Initialize();
        }

        public WaveformBuffer(int capacity)
        {
            this.capacity =
                Mathf.Max(2, capacity);

            Initialize();
        }

        // =========================================================
        // INITIALIZE
        // =========================================================

        private void Initialize()
        {
            capacity =
                Mathf.Max(2, capacity);

            samples =
                new float[capacity];

            writeIndex = 0;
            sampleCount = 0;
        }

        // =========================================================
        // ADD SAMPLE
        // =========================================================

        public void Add(float value)
        {
            EnsureInitialized();

            samples[writeIndex] = value;

            writeIndex =
                (writeIndex + 1) %
                samples.Length;

            sampleCount =
                Mathf.Min(
                    sampleCount + 1,
                    samples.Length
                );
        }

        // =========================================================
        // CLEAR
        // =========================================================

        public void Clear()
        {
            EnsureInitialized();

            System.Array.Clear(
                samples,
                0,
                samples.Length
            );

            writeIndex = 0;
            sampleCount = 0;
        }

        // =========================================================
        // GET CHRONOLOGICAL SAMPLE
        // =========================================================

        public float Get(int index)
        {
            EnsureInitialized();

            if (index < 0 ||
                index >= sampleCount)
            {
                return 0f;
            }

            int oldestIndex =
                sampleCount == samples.Length
                    ? writeIndex
                    : 0;

            int actualIndex =
                (oldestIndex + index) %
                samples.Length;

            return samples[actualIndex];
        }

        // =========================================================
        // NORMALIZED SAMPLE
        // =========================================================

        public float GetNormalized(int index)
        {
            if (sampleCount <= 1)
                return 0f;

            return Get(index);
        }

        // =========================================================
        // RESIZE
        // =========================================================

        public void Resize(int newCapacity)
        {
            newCapacity =
                Mathf.Max(2, newCapacity);

            if (newCapacity == capacity &&
                samples != null)
            {
                return;
            }

            float[] oldSamples =
                samples;

            int oldCount =
                sampleCount;

            capacity =
                newCapacity;

            samples =
                new float[capacity];

            writeIndex = 0;
            sampleCount = 0;

            if (oldSamples == null)
                return;

            int copyCount =
                Mathf.Min(
                    oldCount,
                    capacity
                );

            int start =
                oldCount > copyCount
                    ? oldCount - copyCount
                    : 0;

            for (int i = 0; i < copyCount; i++)
            {
                int oldIndex =
                    (start + i) %
                    oldSamples.Length;

                samples[i] =
                    oldSamples[oldIndex];
            }

            sampleCount = copyCount;

            writeIndex =
                copyCount %
                capacity;
        }

        // =========================================================
        // MIN / MAX
        // =========================================================

        public float GetMinimum()
        {
            if (sampleCount == 0)
                return 0f;

            float min =
                Get(0);

            for (int i = 1; i < sampleCount; i++)
            {
                float value =
                    Get(i);

                if (value < min)
                    min = value;
            }

            return min;
        }

        public float GetMaximum()
        {
            if (sampleCount == 0)
                return 0f;

            float max =
                Get(0);

            for (int i = 1; i < sampleCount; i++)
            {
                float value =
                    Get(i);

                if (value > max)
                    max = value;
            }

            return max;
        }

        // =========================================================
        // AVERAGE
        // =========================================================

        public float GetAverage()
        {
            if (sampleCount == 0)
                return 0f;

            float sum = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                sum += Get(i);
            }

            return sum / sampleCount;
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        private void EnsureInitialized()
        {
            if (samples == null ||
                samples.Length != capacity)
            {
                Initialize();
            }
        }
    }
}