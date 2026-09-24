using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkAdvancedDisplay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SparkDisplayProfile profile;
        [SerializeField] private SparkDisplayAnimationProfile animationProfile;

        [Header("Renderers")]
        [SerializeField] private SparkDisplayValueRenderer primaryValue;
        [SerializeField] private SparkDisplayTextAnimator modeText;
        [SerializeField] private SparkDisplayStatusRenderer statusRenderer;
        [SerializeField] private SparkDisplayBar bar;
        [SerializeField] private TMP_Text unitText;
        [SerializeField] private TMP_Text minMaxText;
        [SerializeField] private TMP_Text secondaryText;
        [SerializeField] private TMP_Text tertiaryText;
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private CanvasGroup displayGroup;

        [Header("Runtime")]
        [SerializeField] private bool useUnscaledTime = true;

        private SparkDisplayData currentData;
        private bool initialized;

        public SparkDisplayData CurrentData => currentData?.Clone();

        public void SetData(SparkDisplayData data)
        {
            if (data == null)
                return;

            currentData = data.Clone();

            ApplyConfiguration();
            ApplyData();
        }

        public void Clear()
        {
            currentData = SparkDisplayData.CreateDefault();

            if (primaryValue != null)
                primaryValue.SetValue(SparkDisplayValueData.Invalid);

            if (secondaryText != null)
                secondaryText.text = string.Empty;

            if (tertiaryText != null)
                tertiaryText.text = string.Empty;

            if (unitText != null)
                unitText.text = string.Empty;

            if (minMaxText != null)
                minMaxText.text = string.Empty;

            if (notificationText != null)
                notificationText.text = string.Empty;
        }

        public void SetPowered(bool powered)
        {
            if (displayGroup != null)
                displayGroup.alpha = powered ? 1f : 0f;

            if (!powered && statusRenderer != null)
                statusRenderer.SetStatus(SparkDisplayState.Off);
        }

        private void Awake()
        {
            currentData = SparkDisplayData.CreateDefault();
            ApplyConfiguration();
            initialized = true;
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
                    animationProfile.instrumentNoise);
            }
        }

        private void ApplyData()
        {
            if (primaryValue != null && (profile == null || profile.showPrimary))
                primaryValue.SetValue(currentData.primary);

            if (unitText != null && (profile == null || profile.showUnit))
                unitText.text = currentData.primary.valid
                    ? SparkDisplayFormatter.UnitText(currentData.primary.unit)
                    : string.Empty;

            if (modeText != null && (profile == null || profile.showMode))
            {
                string mode = currentData.mode == SparkDisplayMode.None
                    ? string.Empty
                    : currentData.mode.ToString().ToUpperInvariant();

                modeText.SetText(
                    mode,
                    animationProfile != null
                        ? animationProfile.textEntry
                        : SparkDisplayTextAnimation.Instant,
                    animationProfile != null ? animationProfile.textSpeed : 20f);
            }

            if (statusRenderer != null && (profile == null || profile.showStatus))
                statusRenderer.SetStatus(currentData.state, currentData.statusText);

            if (secondaryText != null && (profile == null || profile.showSecondary))
                secondaryText.text = FormatOptional(currentData.secondary);

            if (tertiaryText != null && (profile == null || profile.showTertiary))
                tertiaryText.text = FormatOptional(currentData.tertiary);

            if (minMaxText != null && (profile != null && profile.showMinMaxAverage) && currentData.hasMinMaxAverage)
            {
                minMaxText.text =
                    "MIN " + currentData.minimum.ToString("F2") +
                    "   MAX " + currentData.maximum.ToString("F2") +
                    "   AVG " + currentData.average.ToString("F2");
            }

            if (bar != null && currentData.hasBar && (profile == null || profile.showBar))
                bar.SetTarget(currentData.normalizedBar);

            if (notificationText != null && (profile == null || profile.showNotification))
            {
                notificationText.text = currentData.hasNotification
                    ? currentData.notificationText
                    : string.Empty;
            }
        }

        private static string FormatOptional(SparkDisplayValueData data)
        {
            return data.valid ? SparkDisplayFormatter.FormatValue(data) : string.Empty;
        }
    }
}
