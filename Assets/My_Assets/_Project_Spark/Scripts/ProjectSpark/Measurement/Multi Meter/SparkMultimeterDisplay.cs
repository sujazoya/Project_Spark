using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Measurement
{
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterDisplay : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private SparkMultimeter multimeter;

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private TMP_Text unitText;

        [SerializeField]
        private TMP_Text modeText;

        [SerializeField]
        private TMP_Text rangeText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private Graphic continuityIndicator;

        [SerializeField]
        private CanvasGroup displayGroup;

        [Header("Animation")]

        [SerializeField]
        private SparkMultimeterAnimation animation;


        private void Awake()
        {
            if (multimeter == null)
            {
                multimeter =
                    GetComponent<SparkMultimeter>();
            }
        }


        private void OnEnable()
        {
            if (multimeter == null)
            {
                return;
            }

            multimeter.ReadingChanged +=
                HandleReadingChanged;

            multimeter.PowerChanged +=
                HandlePowerChanged;

            multimeter.ModeChanged +=
                HandleModeChanged;

            multimeter.RangeChanged +=
                HandleRangeChanged;

            ApplyPower(
                multimeter.IsPowered);

            HandleReadingChanged(
                multimeter.CurrentReading);

            HandleModeChanged(
                multimeter.Mode);

            HandleRangeChanged(
                multimeter.Range);
        }


        private void OnDisable()
        {
            if (multimeter == null)
            {
                return;
            }

            multimeter.ReadingChanged -=
                HandleReadingChanged;

            multimeter.PowerChanged -=
                HandlePowerChanged;

            multimeter.ModeChanged -=
                HandleModeChanged;

            multimeter.RangeChanged -=
                HandleRangeChanged;
        }


        // ============================================================
        // READING
        // ============================================================

        private void HandleReadingChanged(
            SparkMultimeterReading reading)
        {
            if (valueText != null)
            {
                valueText.text =
                    reading.Text;
            }

            if (unitText != null)
            {
                unitText.text =
                    reading.Unit;
            }

            if (rangeText != null)
            {
                rangeText.text =
                    FormatRange(
                        reading.Range);
            }

            if (statusText != null)
            {
                statusText.text =
                    FormatStatus(
                        reading);
            }

            if (continuityIndicator != null)
            {
                bool showContinuity =
                    reading.Continuity &&
                    reading.Valid &&
                    !reading.OverRange &&
                    !float.IsNaN(reading.Value) &&
                    !float.IsInfinity(reading.Value);

                continuityIndicator.enabled =
                    showContinuity;
            }

            animation?.PlayReading(
                reading);
        }


        // ============================================================
        // POWER
        // ============================================================

        private void HandlePowerChanged(
            bool powered)
        {
            ApplyPower(powered);

            if (powered)
            {
                animation?.PlayPowerOn();
            }
            else
            {
                animation?.PlayPowerOff();
            }
        }


        private void ApplyPower(
            bool powered)
        {
            if (displayGroup != null)
            {
                displayGroup.alpha =
                    powered
                        ? 1f
                        : 0.35f;
            }

            if (modeText != null)
            {
                modeText.text =
                    powered
                        ? FormatMode(
                            multimeter.Mode)
                        : "OFF";
            }

            if (rangeText != null)
            {
                rangeText.text =
                    powered
                        ? FormatRange(
                            multimeter.Range)
                        : string.Empty;
            }

            if (!powered)
            {
                if (valueText != null)
                {
                    valueText.text =
                        string.Empty;
                }

                if (unitText != null)
                {
                    unitText.text =
                        string.Empty;
                }

                if (statusText != null)
                {
                    statusText.text =
                        "POWER OFF";
                }

                if (continuityIndicator != null)
                {
                    continuityIndicator.enabled =
                        false;
                }
            }
        }


        // ============================================================
        // MODE
        // ============================================================

        private void HandleModeChanged(
            SparkMultimeterMode value)
        {
            if (modeText != null)
            {
                modeText.text =
                    FormatMode(value);
            }

            animation?.PlayModeChange();
        }


        // ============================================================
        // RANGE
        // ============================================================

        private void HandleRangeChanged(
            SparkMultimeterRange value)
        {
            if (rangeText != null)
            {
                rangeText.text =
                    FormatRange(value);
            }
        }


        // ============================================================
        // FORMAT MODE
        // ============================================================

        private static string FormatMode(
            SparkMultimeterMode value)
        {
            switch (value)
            {
                case SparkMultimeterMode.VoltageDC:
                    return "DC V";

                case SparkMultimeterMode.CurrentDC:
                    return "DC A";

                case SparkMultimeterMode.Resistance:
                    return "Ω";

                case SparkMultimeterMode.Continuity:
                    return "CONT";

                default:
                    return "OFF";
            }
        }


        // ============================================================
        // FORMAT RANGE
        // ============================================================

        private static string FormatRange(
            SparkMultimeterRange value)
        {
            switch (value)
            {
                case SparkMultimeterRange.Auto:
                    return "AUTO";

                case SparkMultimeterRange.Millivolts:
                    return "mV";

                case SparkMultimeterRange.Volts:
                    return "V";

                case SparkMultimeterRange.Milliamps:
                    return "mA";

                case SparkMultimeterRange.Amps:
                    return "A";

                case SparkMultimeterRange.Ohms:
                    return "Ω";

                case SparkMultimeterRange.Kiloohms:
                    return "kΩ";

                case SparkMultimeterRange.Megaohms:
                    return "MΩ";

                default:
                    return string.Empty;
            }
        }


        // ============================================================
        // FORMAT STATUS
        // ============================================================

        private static string FormatStatus(
            SparkMultimeterReading reading)
        {
            switch (reading.State)
            {
                case SparkMultimeterDisplayState.Stable:
                    return "STABLE";

                case SparkMultimeterDisplayState.Measuring:
                    return "MEAS";

                case SparkMultimeterDisplayState.Negative:
                    return "NEG";

                case SparkMultimeterDisplayState.OverRange:
                    return "OVER";

                case SparkMultimeterDisplayState.Open:
                    return "OPEN";

                case SparkMultimeterDisplayState.Ready:
                    return "READY";

                case SparkMultimeterDisplayState.Invalid:
                    return "ERROR";

                default:
                    return string.Empty;
            }
        }
    }
}
