
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayGraph : Graphic
    {
        [SerializeField, Min(2)]
        private int capacity = 128;

        [SerializeField, Min(0.001f)]
        private float lineThickness = 2f;

        [SerializeField]
        private bool clampInput = true;

        [SerializeField]
        private float minimum = 0f;

        [SerializeField]
        private float maximum = 1f;

        [SerializeField]
        private bool autoRange = true;

        [SerializeField, Min(0.001f)]
        private float autoRangePadding = 0.05f;

        [SerializeField]
        private bool showZeroLine = true;

        [SerializeField, Min(0.001f)]
        private float zeroLineThickness = 1f;

        private float[] samples;
        private int sampleCount;
        private int writeIndex;

        private float currentMinimum;
        private float currentMaximum;

        protected override void Awake()
        {
            base.Awake();

            ValidateConfiguration();
            EnsureStorage();

            currentMinimum =
                minimum;

            currentMaximum =
                maximum;
        }

        public void SetCapacity(
            int value)
        {
            value =
                Mathf.Max(
                    2,
                    value);

            if (capacity == value &&
                samples != null &&
                samples.Length == value)
            {
                return;
            }

            capacity =
                value;

            ResizeStorage();
            UpdateRange();

            SetVerticesDirty();
        }

        public void ClearSamples()
        {
            if (samples != null)
            {
                System.Array.Clear(
                    samples,
                    0,
                    samples.Length);
            }

            sampleCount = 0;
            writeIndex = 0;

            currentMinimum =
                minimum;

            currentMaximum =
                maximum;

            SetVerticesDirty();
        }

        public void AddSample(
            double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return;
            }

            float sample =
                (float)value;

            if (float.IsNaN(sample) ||
                float.IsInfinity(sample))
            {
                return;
            }

            if (clampInput &&
                !autoRange)
            {
                sample =
                    Mathf.Clamp(
                        sample,
                        minimum,
                        maximum);
            }

            EnsureStorage();

            samples[writeIndex] =
                sample;

            writeIndex++;

            if (writeIndex >= capacity)
                writeIndex = 0;

            if (sampleCount < capacity)
                sampleCount++;

            UpdateRange();

            SetVerticesDirty();
        }

        public void AddNormalizedSample(
            float value)
        {
            AddSample(
                Mathf.Clamp01(value));
        }

        protected override void OnPopulateMesh(
            VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (sampleCount < 2)
                return;

            Rect rect =
                rectTransform.rect;

            float range =
                currentMaximum -
                currentMinimum;

            if (range <= Mathf.Epsilon)
                range = 1f;

            for (int i = 1;
                 i < sampleCount;
                 i++)
            {
                float previousSample =
                    GetOrderedSample(i - 1);

                float currentSample =
                    GetOrderedSample(i);

                float x0 =
                    Mathf.Lerp(
                        rect.xMin,
                        rect.xMax,
                        (float)(i - 1) /
                        (sampleCount - 1));

                float x1 =
                    Mathf.Lerp(
                        rect.xMin,
                        rect.xMax,
                        (float)i /
                        (sampleCount - 1));

                float normalized0 =
                    Mathf.InverseLerp(
                        currentMinimum,
                        currentMaximum,
                        previousSample);

                float normalized1 =
                    Mathf.InverseLerp(
                        currentMinimum,
                        currentMaximum,
                        currentSample);

                float y0 =
                    Mathf.Lerp(
                        rect.yMin,
                        rect.yMax,
                        normalized0);

                float y1 =
                    Mathf.Lerp(
                        rect.yMin,
                        rect.yMax,
                        normalized1);

                AddSegment(
                    vertexHelper,
                    new Vector2(
                        x0,
                        y0),
                    new Vector2(
                        x1,
                        y1),
                    lineThickness);
            }

            if (showZeroLine &&
                currentMinimum < 0f &&
                currentMaximum > 0f)
            {
                float zeroNormalized =
                    Mathf.InverseLerp(
                        currentMinimum,
                        currentMaximum,
                        0f);

                float y =
                    Mathf.Lerp(
                        rect.yMin,
                        rect.yMax,
                        zeroNormalized);

                AddSegment(
                    vertexHelper,
                    new Vector2(
                        rect.xMin,
                        y),
                    new Vector2(
                        rect.xMax,
                        y),
                    zeroLineThickness);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            ValidateConfiguration();

            if (samples != null &&
                samples.Length != capacity)
            {
                ResizeStorage();
            }

            if (samples != null)
                UpdateRange();

            SetVerticesDirty();
        }

        private void AddSegment(
            VertexHelper vertexHelper,
            Vector2 start,
            Vector2 end,
            float thickness)
        {
            Vector2 direction =
                end - start;

            if (direction.sqrMagnitude <=
                Mathf.Epsilon)
            {
                return;
            }

            direction.Normalize();

            Vector2 normal =
                new Vector2(
                    -direction.y,
                    direction.x) *
                (thickness * 0.5f);

            int index =
                vertexHelper.currentVertCount;

            Color32 vertexColor =
                color;

            vertexHelper.AddVert(
                start + normal,
                vertexColor,
                Vector2.zero);

            vertexHelper.AddVert(
                start - normal,
                vertexColor,
                Vector2.zero);

            vertexHelper.AddVert(
                end - normal,
                vertexColor,
                Vector2.zero);

            vertexHelper.AddVert(
                end + normal,
                vertexColor,
                Vector2.zero);

            vertexHelper.AddTriangle(
                index,
                index + 1,
                index + 2);

            vertexHelper.AddTriangle(
                index,
                index + 2,
                index + 3);
        }

        private float GetOrderedSample(
            int index)
        {
            if (index < 0 ||
                index >= sampleCount)
            {
                return 0f;
            }

            int oldestIndex =
                sampleCount == capacity
                    ? writeIndex
                    : 0;

            int sampleIndex =
                oldestIndex +
                index;

            if (sampleIndex >= capacity)
            {
                sampleIndex -= capacity;
            }

            return samples[sampleIndex];
        }

        private void UpdateRange()
        {
            if (!autoRange ||
                sampleCount == 0)
            {
                currentMinimum =
                    minimum;

                currentMaximum =
                    maximum;

                return;
            }

            float min =
                GetOrderedSample(0);

            float max =
                min;

            for (int i = 1;
                 i < sampleCount;
                 i++)
            {
                float value =
                    GetOrderedSample(i);

                if (value < min)
                    min = value;

                if (value > max)
                    max = value;
            }

            float range =
                Mathf.Max(
                    Mathf.Abs(
                        max - min),
                    0.0001f);

            float padding =
                range *
                autoRangePadding;

            currentMinimum =
                min - padding;

            currentMaximum =
                max + padding;

            if (Mathf.Approximately(
                    currentMinimum,
                    currentMaximum))
            {
                currentMinimum -= 0.5f;
                currentMaximum += 0.5f;
            }
        }

        private void EnsureStorage()
        {
            if (samples != null &&
                samples.Length == capacity)
            {
                return;
            }

            ResizeStorage();
        }

        private void ResizeStorage()
        {
            float[] oldSamples =
                samples;

            int oldCount =
                sampleCount;

            int oldCapacity =
                oldSamples != null
                    ? oldSamples.Length
                    : 0;

            samples =
                new float[capacity];

            sampleCount =
                Mathf.Min(
                    oldCount,
                    capacity);

            if (oldSamples == null ||
                oldCapacity <= 0 ||
                oldCount <= 0)
            {
                writeIndex = 0;
                return;
            }

            int oldWriteIndex =
                writeIndex;

            int samplesToCopy =
                sampleCount;

            int firstOldIndex =
                oldCount == oldCapacity
                    ? oldWriteIndex
                    : 0;

            int skip =
                oldCount -
                samplesToCopy;

            for (int i = 0;
                 i < samplesToCopy;
                 i++)
            {
                int sourceIndex =
                    firstOldIndex +
                    skip +
                    i;

                sourceIndex %=
                    oldCapacity;

                samples[i] =
                    oldSamples[sourceIndex];
            }

            writeIndex =
                samplesToCopy %
                capacity;
        }

        private void ValidateConfiguration()
        {
            capacity =
                Mathf.Max(
                    2,
                    capacity);

            lineThickness =
                Mathf.Max(
                    0.001f,
                    lineThickness);

            autoRangePadding =
                Mathf.Max(
                    0.001f,
                    autoRangePadding);

            zeroLineThickness =
                Mathf.Max(
                    0.001f,
                    zeroLineThickness);

            if (float.IsNaN(minimum) ||
                float.IsInfinity(minimum))
            {
                minimum = 0f;
            }

            if (float.IsNaN(maximum) ||
                float.IsInfinity(maximum))
            {
                maximum = 1f;
            }

            if (maximum <= minimum)
            {
                maximum =
                    minimum + 1f;
            }
        }
    }
}
