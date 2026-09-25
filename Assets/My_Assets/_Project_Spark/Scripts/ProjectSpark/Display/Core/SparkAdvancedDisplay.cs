using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkAdvancedDisplay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private SparkDisplayProfile profile;

        [SerializeField]
        private SparkDisplayAnimationProfile animationProfile;

        [Header("Renderers")]
        [SerializeField]
        private SparkDisplayValueRenderer primaryValue;

        [SerializeField]
        private SparkDisplayTextAnimator modeText;

        [SerializeField]
        private SparkDisplayStatusRenderer statusRenderer;

        [SerializeField]
        private SparkDisplayBar bar;

        [SerializeField]
        private TMP_Text unitText;

        [SerializeField]
        private TMP_Text minMaxText;

        [SerializeField]
        private TMP_Text secondaryText;

        [SerializeField]
        private TMP_Text tertiaryText;

        [SerializeField]
        private TMP_Text notificationText;

        [SerializeField]
        private CanvasGroup displayGroup;

        [SerializeField, Min(0f)]
        private double dataTolerance = 0.000001d;

        [Header("Advanced Renderers")]
        [SerializeField]
        private SparkDisplayRange rangeRenderer;

        [SerializeField]
        private SparkDisplayAlarm alarmRenderer;

        [SerializeField]
        private SparkDisplaySignalQuality signalQualityRenderer;

        [SerializeField]
        private SparkDisplayGraph graphRenderer;

        [Header("Measurement Statistics")]
        [SerializeField, Min(2)]
        private int historyCapacity = 256;

        [SerializeField]
        private bool collectStatistics = true;

        private SparkDisplayData currentData;
        private SparkDisplayStatisticsTracker statisticsTracker;
        private bool powered = true;

        public SparkDisplayData CurrentData =>
            currentData?.Clone();

        public void SetData(
            SparkDisplayData data)
        {
            SetData(
                data,
                false,
                dataTolerance);
        }

        public void SetData(
            SparkDisplayData data,
            bool force,
            double tolerance)
        {
            if (data == null)
                return;

            tolerance =
                System.Math.Max(
                    0d,
                    tolerance);

            SparkDisplayData nextData =
                data.Clone();

            RecordMeasurementSample(
                nextData);

            if (!force &&
                currentData != null &&
                currentData.IsEquivalentTo(
                    nextData,
                    tolerance))
            {
                ApplyStatistics();
                return;
            }

            currentData =
                nextData;

            ApplyConfiguration();
            ApplyData();
        }
        public SparkDisplayStatistics GetStatistics()
{
    if (statisticsTracker == null)
        return SparkDisplayStatistics.Invalid;

    return statisticsTracker.GetStatistics();
}

        public void Clear()
        {
            currentData =
                SparkDisplayData.CreateDefault();

            if (primaryValue != null)
            {
                primaryValue.SetValue(
                    SparkDisplayValueData.Invalid);
            }

            if (modeText != null)
            {
                modeText.SetImmediate(
                    string.Empty);
            }

            if (statusRenderer != null)
            {
                statusRenderer.SetStatus(
                    SparkDisplayState.Off);
            }

            if (secondaryText != null)
            {
                secondaryText.text =
                    string.Empty;
            }

            if (tertiaryText != null)
            {
                tertiaryText.text =
                    string.Empty;
            }

            if (unitText != null)
            {
                unitText.text =
                    string.Empty;
            }

            if (minMaxText != null)
            {
                minMaxText.text =
                    string.Empty;
            }

            if (notificationText != null)
            {
                notificationText.text =
                    string.Empty;
            }

            if (bar != null)
            {
                bar.SetTarget(0f);
            }

            if (rangeRenderer != null)
            {
                rangeRenderer.SetRange(
                    false,
                    0d,
                    SparkDisplayUnit.None);
            }

            if (alarmRenderer != null)
            {
                alarmRenderer.SetAlarm(
                    false,
                    false,
                    string.Empty);
            }

            if (signalQualityRenderer != null)
            {
                signalQualityRenderer.SetQuality(
                    0f,
                    false);
            }

            if (graphRenderer != null)
            {
                graphRenderer.ClearSamples();
            }
        }

        public void SetPowered(
            bool poweredState)
        {
            powered =
                poweredState;

            if (displayGroup != null)
            {
                displayGroup.alpha =
                    powered
                        ? 1f
                        : 0f;
            }

            if (!powered)
            {
                if (statusRenderer != null)
                {
                    statusRenderer.SetStatus(
                        SparkDisplayState.Off);
                }

                return;
            }

            if (currentData != null)
            {
                ApplyData();
            }
        }

        public void ResetStatistics()
        {
            if (statisticsTracker == null)
                return;

            statisticsTracker.Clear();

            ApplyStatistics();
        }

        private void Awake()
        {
            historyCapacity =
                Mathf.Max(
                    2,
                    historyCapacity);

            dataTolerance =
                System.Math.Max(
                    0d,
                    dataTolerance);

            currentData =
                SparkDisplayData.CreateDefault();

            statisticsTracker =
                new SparkDisplayStatisticsTracker(
                    historyCapacity);

            ApplyConfiguration();
        }

        private void ApplyConfiguration()
        {
            if (animationProfile == null)
                return;

            if (primaryValue != null)
            {
                primaryValue.Configure(
                    animationProfile.valueTransition,
                    animationProfile.valueSpeed,
                    animationProfile.instrumentDeadband,
                    animationProfile.instrumentNoise,
                    animationProfile.rollingDuration);
            }

            if (statusRenderer != null)
            {
                statusRenderer.Configure(
                    animationProfile.statusAnimation,
                    animationProfile.statusSpeed,
                    animationProfile.statusPulseSpeed);
            }
        }

        private void ApplyData()
        {
            if (currentData == null ||
                !powered)
            {
                return;
            }

            bool showPrimary =
                profile == null ||
                profile.showPrimary;

            bool showUnit =
                profile == null ||
                profile.showUnit;

            bool showMode =
                profile == null ||
                profile.showMode;

            bool showStatus =
                profile == null ||
                profile.showStatus;

            bool showSecondary =
                profile == null ||
                profile.showSecondary;

            bool showTertiary =
                profile == null ||
                profile.showTertiary;

            bool showBar =
                profile == null ||
                profile.showBar;

            bool showRange =
                profile == null ||
                profile.showRange;

            bool showNotification =
                profile == null ||
                profile.showNotification;

            if (primaryValue != null)
            {
                primaryValue.SetValue(
                    showPrimary
                        ? currentData.primary
                        : SparkDisplayValueData.Invalid);
            }

            if (unitText != null)
            {
                unitText.text =
                    showUnit &&
                    currentData.primary.valid
                        ? SparkDisplayFormatter.UnitText(
                            currentData.primary.unit)
                        : string.Empty;
            }

            if (modeText != null)
            {
                string mode =
                    showMode &&
                    currentData.mode !=
                    SparkDisplayMode.None
                        ? currentData.mode
                            .ToString()
                            .ToUpperInvariant()
                        : string.Empty;

                modeText.SetText(
                    mode,
                    animationProfile != null
                        ? animationProfile.textEntry
                        : SparkDisplayTextAnimation.Instant,
                    animationProfile != null
                        ? animationProfile.textSpeed
                        : 20f);
            }

            if (statusRenderer != null)
            {
                statusRenderer.SetStatus(
                    showStatus
                        ? currentData.state
                        : SparkDisplayState.Off,
                    showStatus
                        ? currentData.statusText
                        : string.Empty);
            }

            if (secondaryText != null)
            {
                secondaryText.text =
                    showSecondary
                        ? FormatOptional(
                            currentData.secondary)
                        : string.Empty;
            }

            if (tertiaryText != null)
            {
                tertiaryText.text =
                    showTertiary
                        ? FormatOptional(
                            currentData.tertiary)
                        : string.Empty;
            }

            ApplyStatistics();

            if (bar != null)
            {
                if (showBar &&
                    currentData.hasBar)
                {
                    bar.SetTarget(
                        currentData.normalizedBar);
                }
                else
                {
                    bar.SetTarget(0f);
                }
            }

            if (rangeRenderer != null)
            {
                rangeRenderer.SetRange(
                    showRange &&
                    currentData.hasRange,
                    currentData.rangeMaximum,
                    currentData.primary.unit);
            }

            if (alarmRenderer != null)
            {
                bool hasAlarm =
                    showNotification &&
                    currentData.hasNotification &&
                    !string.IsNullOrWhiteSpace(
                        currentData.notificationText);

                alarmRenderer.SetAlarm(
                    hasAlarm,
                    currentData.criticalNotification,
                    currentData.notificationText);
            }

            if (signalQualityRenderer != null)
            {
                bool signalQualityVisible =
                    currentData.signalQuality > 0f;

                signalQualityRenderer.SetQuality(
                    currentData.signalQuality,
                    signalQualityVisible);
            }

            if (notificationText != null)
            {
                notificationText.text =
                    showNotification &&
                    currentData.hasNotification
                        ? currentData.notificationText
                        : string.Empty;
            }
        }

        private void ApplyStatistics()
        {
            if (minMaxText == null)
                return;

            if (profile == null ||
                !profile.showMinMaxAverage ||
                currentData == null ||
                !currentData.primary.valid ||
                statisticsTracker == null)
            {
                minMaxText.text =
                    string.Empty;

                return;
            }

            SparkDisplayStatistics statistics =
                statisticsTracker.GetStatistics();

            if (!statistics.valid)
            {
                minMaxText.text =
                    string.Empty;

                return;
            }

            minMaxText.text =
                SparkDisplayFormatter.FormatMinMaxAverage(
                    currentData.primary,
                    statistics.minimum,
                    statistics.maximum,
                    statistics.average);
        }

        private void RecordMeasurementSample(
            SparkDisplayData data)
        {
            if (data == null)
                return;

            if (collectStatistics &&
                data.primary.valid)
            {
                UpdateStatistics(
                    data.primary.value);
            }

            if (graphRenderer != null &&
                data.hasGraphValue &&
                (profile == null ||
                 profile.showGraph))
            {
                graphRenderer.AddSample(
                    data.graphValue);
            }
        }

        private void UpdateStatistics(
            double value)
        {
            if (!collectStatistics ||
                statisticsTracker == null)
            {
                return;
            }

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                return;
            }

            statisticsTracker.Add(
                value);
        }

        private static string FormatOptional(
            SparkDisplayValueData data)
        {
            return data.valid
                ? SparkDisplayFormatter.FormatValue(
                    data)
                : string.Empty;
        }
    }
}
