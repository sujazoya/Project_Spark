using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Cosmetic numeric animation for SparkCapacitorDigitalScreen.valueText.
    /// Source remains owned by the existing display; the overlay is the visible readout.
    /// Never triggers a solve or alters electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkCapacitorRollingDigits : MonoBehaviour
    {
        [Header("Editor-generated text layers")]
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text animatedText;
        [SerializeField] private TMP_Text outgoingText;
        [SerializeField] private Color sourceOriginalColor = Color.cyan;

        [Header("Count-up animation")]
        [SerializeField, Range(0.04f, 1.5f)] private float followSmoothSeconds = 0.22f;
        [SerializeField, Range(0f, 25f)] private float minimumStepPerSecond = 0f;
        [SerializeField] private bool animateNumbers = true;
        [Header("Neon slide / ghost")]
        [SerializeField, Range(0.05f, 1f)] private float slideDuration = 0.22f;
        [SerializeField, Range(0f, 30f)] private float verticalSlidePixels = 12f;
        [SerializeField, Range(0f, 1f)] private float ghostOpacity = 0.42f;
        [SerializeField] private Color neonColor = new Color(.20f, .95f, 1f, 1f);

        private static readonly Regex Numeric = new Regex(
            @"^\s*(?<prefix>[^\d+\-]*)(?<value>[+\-]?(?:\d+\.?\d*|\.\d+))(?<suffix>.*)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private string lastSource = null;
        private string lastOutput = null;
        private string prefix = string.Empty;
        private string suffix = string.Empty;
        private string numberFormat = "0.000";
        private double displayed;
        private double target;
        private float velocity;
        private float slideClock = 100f;
        private Vector2 basePosition;
        private Vector2 ghostBasePosition;
        private Color ghostBaseColor;
        private RectTransform animatedRect;
        private RectTransform ghostRect;
        private bool initialized;

        private void OnEnable()
        {
            initialized = false;
            lastSource = null;
            slideClock = 100f;
            CacheLayers();
            if (sourceText != null && animatedText != null)
            {
                var c = sourceText.color;
                c.a = 0f;
                sourceText.color = c;
            }
        }

        private void CacheLayers()
        {
            animatedRect = animatedText != null ? animatedText.rectTransform : null;
            ghostRect = outgoingText != null ? outgoingText.rectTransform : null;
            if (animatedRect != null) basePosition = animatedRect.anchoredPosition;
            if (ghostRect != null) ghostBasePosition = ghostRect.anchoredPosition;
            if (outgoingText != null)
            {
                ghostBaseColor = neonColor;
                ghostBaseColor.a = ghostOpacity;
                outgoingText.color = Color.clear;
            }
        }

        private void LateUpdate()
        {
            if (sourceText == null || animatedText == null) return;
            string input = sourceText.text ?? string.Empty;
            if (!string.Equals(input, lastSource, StringComparison.Ordinal))
            {
                lastSource = input;
                OnSourceChanged(input);
            }

            if (initialized && animateNumbers)
            {
                float delta = Mathf.Max(0f, Time.unscaledDeltaTime);
                float next = Mathf.SmoothDamp(
                    (float)displayed, (float)target, ref velocity,
                    Mathf.Max(.04f, followSmoothSeconds),
                    float.PositiveInfinity, delta);
                // Skip microscopic floating-point drift at steady state.
                if (Math.Abs(target - next) < 0.00004d)
                {
                    displayed = target;
                    velocity = 0f;
                }
                else displayed = next;

                if (minimumStepPerSecond > 0f && Math.Abs(target - displayed) > 0.001d)
                    displayed = Mathf.MoveTowards((float)displayed, (float)target,
                        minimumStepPerSecond * delta);

                string formatted = prefix + displayed.ToString(numberFormat, Invariant) + suffix;
                SetOutput(formatted);
            }

            AnimateSlide(Time.unscaledDeltaTime);
        }

        private void OnSourceChanged(string input)
        {
            Match match = Numeric.Match(input);
            if (!match.Success || !double.TryParse(match.Groups["value"].Value,
                    NumberStyles.Float, Invariant, out double parsed) ||
                double.IsNaN(parsed) || double.IsInfinity(parsed) ||
                Math.Abs(parsed) > 1000000000d)
            {
                initialized = false;
                velocity = 0f;
                SetOutput(input); // e.g. --.--- / WAIT: don't animate fake data
                return;
            }

            string number = match.Groups["value"].Value;
            int dot = number.IndexOf('.');
            int precision = dot >= 0 ? Mathf.Clamp(number.Length - dot - 1, 0, 6) : 0;
            numberFormat = precision == 0 ? "0" : "0." + new string('0', precision);
            prefix = match.Groups["prefix"].Value;
            suffix = match.Groups["suffix"].Value;
            target = parsed;

            if (!initialized || !animateNumbers)
            {
                displayed = target;
                velocity = 0f;
                initialized = true;
                SetOutput(input);
            }
            else if (Math.Abs(target - displayed) > 0.0000001d)
            {
                // Keep rolling from the currently visible number, not from zero.
                // Rate-limit slide restarts while numeric updates stream in at 10 Hz.
                if (slideClock < slideDuration * .65f) return;
                slideClock = 0f;
                if (outgoingText != null)
                {
                    outgoingText.text = lastOutput ?? string.Empty;
                    outgoingText.color = ghostBaseColor;
                    if (ghostRect != null) ghostRect.anchoredPosition = ghostBasePosition;
                }
            }
        }

        private void SetOutput(string value)
        {
            if (lastOutput == value) return;
            lastOutput = value;
            animatedText.text = value;
        }

        private void AnimateSlide(float dt)
        {
            if (animatedRect == null) return;
            slideClock += Mathf.Max(0f, dt);
            float t = Mathf.Clamp01(slideClock / Mathf.Max(.05f, slideDuration));
            float ease = 1f - (1f - t) * (1f - t) * (1f - t);
            animatedRect.anchoredPosition = basePosition +
                Vector2.down * ((1f - ease) * verticalSlidePixels);
            Color c = neonColor;
            c.a = Mathf.Lerp(.60f, 1f, ease);
            animatedText.color = c;
            if (outgoingText != null)
            {
                Color gc = ghostBaseColor;
                gc.a *= 1f - ease;
                outgoingText.color = gc;
                if (ghostRect != null)
                    ghostRect.anchoredPosition = ghostBasePosition +
                        Vector2.up * (ease * verticalSlidePixels);
            }
        }

        public void Configure(TMP_Text source, TMP_Text main, TMP_Text ghost, Color original)
        {
            sourceText = source;
            animatedText = main;
            outgoingText = ghost;
            sourceOriginalColor = original;
            CacheLayers();
            lastSource = null;
        }

        public void RestoreSourceColor()
        {
            if (sourceText != null) sourceText.color = sourceOriginalColor;
        }
    }
}
