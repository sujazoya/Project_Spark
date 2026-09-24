using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayDemoSource : SparkDisplaySource
    {
        [SerializeField] private double baseValue = 12.48;
        [SerializeField] private SparkDisplayUnit unit = SparkDisplayUnit.V;
        [SerializeField] private SparkDisplayMode mode = SparkDisplayMode.DC;
        [SerializeField] private SparkDisplayState state = SparkDisplayState.Measuring;
        [SerializeField, Range(0f, 1f)] private float bar = 0.78f;

        private float elapsed;

        protected override void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            base.Update();
        }

        protected override void BuildDisplayData(SparkDisplayData data)
        {
            double liveValue = baseValue + System.Math.Sin(elapsed * 2.0) * 0.025;

            data.primary = new SparkDisplayValueData
            {
                valid = true,
                value = liveValue,
                unit = unit,
                precision = 2,
                useEngineeringPrefixes = false
            };

            data.mode = mode;
            data.state = state;
            data.quality = SparkDisplayQuality.Stable;
            data.normalizedBar = bar;
            data.hasBar = true;
            data.hasMinMaxAverage = true;
            data.minimum = baseValue - 0.07;
            data.maximum = baseValue + 0.05;
            data.average = baseValue;
            data.statusText = "MEASURING";
        }
    }
}
