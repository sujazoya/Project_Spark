using System;
using System.Collections;
using UnityEngine;

namespace ProjectSpark.Display.Tests
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayTestHarness : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAdvancedDisplay display;

        [Header("Automated Test")]
        [SerializeField]
        private bool runOnStart;

        [SerializeField, Min(0.05f)]
        private float stepDelay = 0.75f;

        [SerializeField]
        private bool stopOnFailure;

        [Header("Logging")]
        [SerializeField]
        private bool logPassedTests = true;

        [SerializeField]
        private bool logDetailedValues = true;

        private Coroutine testRoutine;

        private int passed;
        private int failed;
        private int total;

        public SparkAdvancedDisplay Display => display;

        public bool IsRunning =>
            testRoutine != null;

        public int Passed => passed;

        public int Failed => failed;

        public int Total => total;

        private void OnEnable()
        {
            if (runOnStart)
            {
                StartAutomatedTest();
            }
        }

        private void OnDisable()
        {
            StopAutomatedTest();
        }

        public void StartAutomatedTest()
        {
            if (display == null)
            {
                Debug.LogError(
                    "SparkDisplayTestHarness requires a SparkAdvancedDisplay reference.",
                    this);

                return;
            }

            StopAutomatedTest();

            testRoutine =
                StartCoroutine(
                    RunAutomatedTest());
        }

        public void StopAutomatedTest()
        {
            if (testRoutine == null)
                return;

            StopCoroutine(
                testRoutine);

            testRoutine = null;
        }

        public void ResetTestResults()
        {
            passed = 0;
            failed = 0;
            total = 0;

            Debug.Log(
                "Spark Display Test Harness results reset.",
                this);
        }

        public void TestBasicValue()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    12.48d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.mode =
                SparkDisplayMode.DC;

            data.state =
                SparkDisplayState.Measuring;

            data.quality =
                SparkDisplayQuality.Stable;

            data.statusText =
                "MEASURING";

            SetTestData(data);

            RecordResult(
                "Basic Value",
                true,
                "12.48 V");
        }

        public void TestEngineeringPrefixes()
        {
            TestEngineeringValue(
                0.0000012d,
                "1.20 µV");

            TestEngineeringValue(
                0.0012d,
                "1.20 mV");

            TestEngineeringValue(
                1.2d,
                "1.20 V");

            TestEngineeringValue(
                1200d,
                "1.20 kV");

            TestEngineeringValue(
                1200000d,
                "1.20 MV");
        }

        public void TestInvalidValue()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                SparkDisplayValueData.Invalid;

            data.mode =
                SparkDisplayMode.None;

            data.state =
                SparkDisplayState.NoSignal;

            data.quality =
                SparkDisplayQuality.Invalid;

            data.statusText =
                "NO SIGNAL";

            SetTestData(data);

            RecordResult(
                "Invalid Value",
                true,
                "Expected display value: ----");
        }

        public void TestNaNValue()
        {
            SetPrimaryValue(
                double.NaN,
                SparkDisplayUnit.V,
                SparkDisplayState.Fault,
                "INVALID");

            RecordResult(
                "NaN Value",
                true,
                "Invalid value should render as ----.");
        }

        public void TestPositiveInfinity()
        {
            SetPrimaryValue(
                double.PositiveInfinity,
                SparkDisplayUnit.V,
                SparkDisplayState.OverRange,
                "OVER RANGE");

            RecordResult(
                "Positive Infinity",
                true,
                "Invalid value should render as ----.");
        }

        public void TestNegativeInfinity()
        {
            SetPrimaryValue(
                double.NegativeInfinity,
                SparkDisplayUnit.V,
                SparkDisplayState.OverRange,
                "OVER RANGE");

            RecordResult(
                "Negative Infinity",
                true,
                "Invalid value should render as ----.");
        }

        public void TestBar()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    12.48d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.hasBar = true;
            data.normalizedBar = 0.75f;

            data.mode =
                SparkDisplayMode.DC;

            data.state =
                SparkDisplayState.Measuring;

            SetTestData(data);

            RecordResult(
                "Bar",
                true,
                "Expected normalized bar: 75%.");
        }

        public void TestSignalQuality()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    12.48d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.signalQuality = 1f;

            data.quality =
                SparkDisplayQuality.Stable;

            data.state =
                SparkDisplayState.Measuring;

            SetTestData(data);

            RecordResult(
                "Signal Quality",
                true,
                "Expected signal quality: 100%.");
        }

        public void TestRange()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    12.48d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.hasRange = true;
            data.rangeMaximum = 24d;

            data.state =
                SparkDisplayState.Measuring;

            SetTestData(data);

            RecordResult(
                "Range",
                true,
                "Expected range: 24.00 V.");
        }

        public void TestAlarm()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    28d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.state =
                SparkDisplayState.OverRange;

            data.quality =
                SparkDisplayQuality.OverRange;

            data.hasNotification = true;
            data.criticalNotification = true;

            data.notificationText =
                "OVER VOLTAGE";

            SetTestData(data);

            RecordResult(
                "Critical Alarm",
                true,
                "Expected alarm: OVER VOLTAGE.");
        }

        public void TestStatistics()
        {
            if (display == null)
                return;

            display.ResetStatistics();

            AddStatisticsSample(10d);
            AddStatisticsSample(20d);
            AddStatisticsSample(30d);
            AddStatisticsSample(40d);
            AddStatisticsSample(50d);

            SparkDisplayStatistics statistics =
                display.GetStatistics();

            bool valid =
                statistics.valid &&
                statistics.sampleCount == 5 &&
                Approximately(
                    statistics.minimum,
                    10d) &&
                Approximately(
                    statistics.maximum,
                    50d) &&
                Approximately(
                    statistics.average,
                    30d) &&
                Approximately(
                    statistics.peak,
                    50d) &&
                Approximately(
                    statistics.latest,
                    50d);

            RecordResult(
                "Statistics",
                valid,
                BuildStatisticsMessage(
                    statistics));
        }

        public void TestStatisticsReset()
        {
            display.ResetStatistics();

            AddStatisticsSample(25d);

            SparkDisplayStatistics statistics =
                display.GetStatistics();

            bool valid =
                statistics.valid &&
                statistics.sampleCount == 1 &&
                Approximately(
                    statistics.minimum,
                    25d) &&
                Approximately(
                    statistics.maximum,
                    25d) &&
                Approximately(
                    statistics.average,
                    25d) &&
                Approximately(
                    statistics.peak,
                    25d) &&
                Approximately(
                    statistics.latest,
                    25d);

            RecordResult(
                "Statistics Reset",
                valid,
                BuildStatisticsMessage(
                    statistics));
        }

        public void TestClear()
        {
            display.Clear();

            SparkDisplayData currentData =
                display.CurrentData;

            bool visualDataCleared =
                currentData != null &&
                !currentData.primary.valid;

            RecordResult(
                "Clear",
                visualDataCleared,
                "Current display data cleared.");
        }

        public void TestPoweredOff()
        {
            display.SetPowered(false);

            RecordResult(
                "Power Off",
                true,
                "Display power disabled.");
        }

        public void TestPoweredOn()
        {
            display.SetPowered(true);

            RecordResult(
                "Power On",
                true,
                "Display power enabled.");
        }

        public void TestGraph()
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    0d,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.hasGraphValue = true;

            data.graphValue = 0d;

            for (int i = 0; i < 20; i++)
            {
                double value =
                    Math.Sin(
                        i * 0.35d) *
                    10d;

                data.graphValue =
                    value;

                data.primary.value =
                    value;

                SetTestData(data);
            }

            RecordResult(
                "Graph",
                true,
                "Generated 20 graph samples.");
        }

        public void TestTolerance()
        {
            display.ResetStatistics();

            AddStatisticsSample(
                12.000d);

            AddStatisticsSample(
                12.001d);

            AddStatisticsSample(
                12.002d);

            SparkDisplayStatistics statistics =
                display.GetStatistics();

            bool valid =
                statistics.valid &&
                statistics.sampleCount == 3 &&
                Approximately(
                    statistics.minimum,
                    12.000d) &&
                Approximately(
                    statistics.maximum,
                    12.002d);

            RecordResult(
                "Tolerance / Statistics",
                valid,
                "Statistics received every valid sample.");
        }

        public void TestAllStatuses()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "Test All Statuses should be run in Play Mode.",
                    this);

                return;
            }

            StartCoroutine(
                RunStatusTest());
        }

        private IEnumerator RunAutomatedTest()
        {
            ResetTestResults();

            Debug.Log(
                "========== PROJECT SPARK DISPLAY TEST START ==========",
                this);

            display.SetPowered(true);

            yield return RunStep(
                "Basic Value",
                TestBasicValue);

            yield return RunStep(
                "Engineering Prefixes",
                TestEngineeringPrefixes);

            yield return RunStep(
                "Invalid Value",
                TestInvalidValue);

            yield return RunStep(
                "NaN",
                TestNaNValue);

            yield return RunStep(
                "Positive Infinity",
                TestPositiveInfinity);

            yield return RunStep(
                "Negative Infinity",
                TestNegativeInfinity);

            yield return RunStep(
                "Bar",
                TestBar);

            yield return RunStep(
                "Signal Quality",
                TestSignalQuality);

            yield return RunStep(
                "Range",
                TestRange);

            yield return RunStep(
                "Alarm",
                TestAlarm);

            yield return RunStep(
                "Statistics",
                TestStatistics);

            yield return RunStep(
                "Statistics Reset",
                TestStatisticsReset);

            yield return RunStep(
                "Graph",
                TestGraph);

            yield return RunStep(
                "Tolerance",
                TestTolerance);

            yield return RunStep(
                "Power Off",
                TestPoweredOff);

            yield return WaitForStep();

            yield return RunStep(
                "Power On",
                TestPoweredOn);

            yield return RunStep(
                "Clear",
                TestClear);

            Debug.Log(
                "========== PROJECT SPARK DISPLAY TEST COMPLETE ==========",
                this);

            Debug.Log(
                $"RESULT: {passed} passed / {failed} failed / {total} total",
                this);

            testRoutine = null;
        }

        private IEnumerator RunStatusTest()
        {
            SparkDisplayState[] states =
            {
                SparkDisplayState.Off,
                SparkDisplayState.Standby,
                SparkDisplayState.Initializing,
                SparkDisplayState.Ready,
                SparkDisplayState.Measuring,
                SparkDisplayState.Charging,
                SparkDisplayState.Full,
                SparkDisplayState.Warning,
                SparkDisplayState.Fault,
                SparkDisplayState.Disconnected,
                SparkDisplayState.OverRange,
                SparkDisplayState.NoSignal
            };

            for (int i = 0; i < states.Length; i++)
            {
                SparkDisplayData data =
                    CreateBaseData();

                data.primary =
                    CreateValue(
                        12.48d,
                        SparkDisplayUnit.V,
                        2,
                        true);

                data.state =
                    states[i];

                data.statusText =
                    states[i].ToString();

                SetTestData(data);

                Debug.Log(
                    $"Display status test: {states[i]}",
                    this);

                yield return WaitForStep();
            }
        }

        private IEnumerator RunStep(
            string testName,
            Action test)
        {
            if (display == null)
            {
                RecordResult(
                    testName,
                    false,
                    "SparkAdvancedDisplay reference is missing.");

                yield break;
            }

            int failedBefore =
                failed;

            try
            {
                test.Invoke();
            }
            catch (Exception exception)
            {
                RecordResult(
                    testName,
                    false,
                    exception.ToString());
            }

            yield return WaitForStep();

            if (stopOnFailure &&
                failed > failedBefore)
            {
                Debug.LogError(
                    $"Display test stopped after failure: {testName}",
                    this);

                StopAutomatedTest();
            }
        }

        private IEnumerator WaitForStep()
        {
            yield return new WaitForSecondsRealtime(
                Mathf.Max(
                    0.05f,
                    stepDelay));
        }

        private void AddStatisticsSample(
            double value)
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    value,
                    SparkDisplayUnit.V,
                    2,
                    false);

            data.state =
                SparkDisplayState.Measuring;

            data.quality =
                SparkDisplayQuality.Stable;

            data.hasGraphValue = true;
            data.graphValue = value;

            SetTestData(data);
        }

        private void SetPrimaryValue(
            double value,
            SparkDisplayUnit unit,
            SparkDisplayState state,
            string status)
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                new SparkDisplayValueData
                {
                    valid = true,
                    value = value,
                    unit = unit,
                    precision = 2,
                    useEngineeringPrefixes = true
                };

            data.state = state;
            data.statusText = status;

            SetTestData(data);
        }

        private void TestEngineeringValue(
            double value,
            string expected)
        {
            SparkDisplayData data =
                CreateBaseData();

            data.primary =
                CreateValue(
                    value,
                    SparkDisplayUnit.V,
                    2,
                    true);

            data.state =
                SparkDisplayState.Measuring;

            SetTestData(data);

            RecordResult(
                "Engineering Prefix",
                true,
                $"Input={value} V; Expected={expected}");
        }

        private SparkDisplayData CreateBaseData()
        {
            SparkDisplayData data =
                SparkDisplayData.CreateDefault();

            data.primary =
                SparkDisplayValueData.Invalid;

            data.secondary =
                SparkDisplayValueData.Invalid;

            data.tertiary =
                SparkDisplayValueData.Invalid;

            data.mode =
                SparkDisplayMode.DC;

            data.state =
                SparkDisplayState.Ready;

            data.quality =
                SparkDisplayQuality.Stable;

            data.hasBar = false;
            data.hasRange = false;
            data.hasNotification = false;
            data.hasGraphValue = false;

            data.normalizedBar = 0f;
            data.signalQuality = 1f;

            data.statusText =
                string.Empty;

            data.notificationText =
                string.Empty;

            return data;
        }

        private static SparkDisplayValueData CreateValue(
            double value,
            SparkDisplayUnit unit,
            int precision,
            bool engineeringPrefixes)
        {
            return new SparkDisplayValueData
            {
                valid =
                    !double.IsNaN(value) &&
                    !double.IsInfinity(value),

                value = value,
                unit = unit,
                precision = precision,
                useEngineeringPrefixes =
                    engineeringPrefixes,

                customSuffix =
                    string.Empty
            };
        }

        private void SetTestData(
            SparkDisplayData data)
        {
            if (display == null)
            {
                RecordResult(
                    "Set Test Data",
                    false,
                    "Display reference is missing.");

                return;
            }

            display.SetData(
                data,
                true,
                0d);
        }

        private void RecordResult(
            string name,
            bool successful,
            string details)
        {
            total++;

            if (successful)
            {
                passed++;

                if (logPassedTests)
                {
                    Debug.Log(
                        $"[PASS] {name} - {details}",
                        this);
                }

                return;
            }

            failed++;

            Debug.LogError(
                $"[FAIL] {name} - {details}",
                this);
        }

        private string BuildStatisticsMessage(
            SparkDisplayStatistics statistics)
        {
            return
                $"Statistics | " +
                $"Valid={statistics.valid} | " +
                $"Count={statistics.sampleCount} | " +
                $"Min={statistics.minimum} | " +
                $"Max={statistics.maximum} | " +
                $"Avg={statistics.average} | " +
                $"Peak={statistics.peak} | " +
                $"Latest={statistics.latest}";
        }

        private static bool Approximately(
            double a,
            double b,
            double tolerance = 0.000001d)
        {
            if (double.IsNaN(a) ||
                double.IsNaN(b))
            {
                return
                    double.IsNaN(a) &&
                    double.IsNaN(b);
            }

            if (double.IsInfinity(a) ||
                double.IsInfinity(b))
            {
                return a.Equals(b);
            }

            double difference =
                Math.Abs(a - b);

            if (difference <= tolerance)
                return true;

            double magnitude =
                Math.Max(
                    Math.Abs(a),
                    Math.Abs(b));

            if (magnitude <= tolerance)
                return difference <= tolerance;

            return
                difference / magnitude <=
                tolerance;
        }
    }
}