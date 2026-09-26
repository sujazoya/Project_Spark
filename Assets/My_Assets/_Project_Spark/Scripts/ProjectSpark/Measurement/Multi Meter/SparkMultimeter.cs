using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Measurement
{
    public enum SparkMultimeterMode
    {
        Off,
        VoltageDC,
        CurrentDC,
        Resistance,
        Continuity
    }

    public enum SparkMultimeterRange
    {
        Auto,
        Millivolts,
        Volts,
        Milliamps,
        Amps,
        Ohms,
        Kiloohms,
        Megaohms
    }

    public enum SparkMultimeterDisplayState
    {
        Off,
        Ready,
        Measuring,
        Stable,
        Negative,
        OverRange,
        Open,
        Invalid
    }

    [Serializable]
    public readonly struct SparkMultimeterReading
    {
        public bool Valid { get; }
        public bool Stable { get; }
        public bool Negative { get; }
        public bool OverRange { get; }
        public bool Continuity { get; }

        public float Value { get; }
        public float DisplayValue { get; }

        public SparkMultimeterMode Mode { get; }
        public SparkMultimeterRange Range { get; }

        public string Unit { get; }
        public string Text { get; }

        public SparkMultimeterDisplayState State { get; }

        public SparkMultimeterReading(
            bool valid,
            bool stable,
            bool negative,
            bool overRange,
            bool continuity,
            float value,
            float displayValue,
            SparkMultimeterMode mode,
            SparkMultimeterRange range,
            string unit,
            string text,
            SparkMultimeterDisplayState state)
        {
            Valid = valid;
            Stable = stable;
            Negative = negative;
            OverRange = overRange;
            Continuity = continuity;

            Value = value;
            DisplayValue = displayValue;

            Mode = mode;
            Range = range;

            Unit = unit;
            Text = text;

            State = state;
        }

        public static SparkMultimeterReading Invalid =>
            new SparkMultimeterReading(
                false,
                false,
                false,
                false,
                false,
                0f,
                0f,
                SparkMultimeterMode.Off,
                SparkMultimeterRange.Auto,
                string.Empty,
                "---",
                SparkMultimeterDisplayState.Invalid);
    }

    [DisallowMultipleComponent]
    public sealed class SparkMultimeter : SparkElectronicObject
    {
        // ============================================================
        // MEASUREMENT
        // ============================================================

        [Header("Measurement")]

        [SerializeField]
        private SparkMeasurementSystem measurementSystem;

        /*
         * These are the terminals that the measurement system reads from.
         *
         * They remain SparkTerminal references so the existing electrical
         * topology and probe measurement system are preserved.
         */
        [SerializeField]
        private SparkTerminal redProbeTerminal;

        [SerializeField]
        private SparkTerminal blackProbeTerminal;


        // ============================================================
        // DISPLAY
        // ============================================================
       

        // ============================================================
        // OPERATION
        // ============================================================

        [Header("Operation")]

        [SerializeField]
        private bool poweredOn;

        [SerializeField]
        private SparkMultimeterMode mode =
            SparkMultimeterMode.VoltageDC;

        [SerializeField]
        private SparkMultimeterRange range =
            SparkMultimeterRange.Auto;


        // ============================================================
        // UPDATE
        // ============================================================

        [Header("Measurement Update")]

        [SerializeField, Min(0.01f)]
        private float measurementInterval = 0.05f;

        [SerializeField, Min(0f)]
        private float stabilityTolerance = 0.0025f;

        [SerializeField, Min(1)]
        private int stabilitySamplesRequired = 5;


        // ============================================================
        // RANGES
        // ============================================================

        [Header("Ranges")]

        [SerializeField, Min(0.001f)]
        private float voltageRange = 1000f;

        [SerializeField, Min(0.001f)]
        private float currentRange = 10f;

        [SerializeField, Min(0.001f)]
        private float resistanceRange = 100000000f;

        [SerializeField, Min(0f)]
        private float continuityThreshold = 10f;


        // ============================================================
        // FORMATTING
        // ============================================================

        [Header("Formatting")]

        [SerializeField, Min(0)]
        private int displayDecimals = 3;

        [SerializeField]
        private bool useScientificNotationForVerySmallValues = true;

        [SerializeField, Min(0.0000001f)]
        private float zeroThreshold = 0.000001f;


        // ============================================================
        // RUNTIME
        // ============================================================

        private float measurementTimer;
        private float previousValue;
        private int stableSampleCount;

        private bool hasPreviousValue;

        private SparkMultimeterReading currentReading =
            SparkMultimeterReading.Invalid;


        // ============================================================
        // EVENTS
        // ============================================================

        public event Action<SparkMultimeterReading> ReadingChanged;

        public event Action<SparkMultimeterMode> ModeChanged;

        public event Action<bool> PowerChanged;

        public event Action<SparkMultimeterRange> RangeChanged;


        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsPowered => poweredOn;

        public SparkMultimeterMode Mode => mode;

        public SparkMultimeterRange Range => range;

        public SparkMultimeterReading CurrentReading =>
            currentReading;

        public SparkTerminal RedProbeTerminal =>
            redProbeTerminal;

        public SparkTerminal BlackProbeTerminal =>
            blackProbeTerminal;       


        // ============================================================
        // UNITY
        // ============================================================

        protected override void Awake()
        {
            base.Awake();

            ResolveReferences();

            ResetReading();

            SynchronizeOperationalState();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            measurementInterval =
                Mathf.Max(0.01f, measurementInterval);

            stabilityTolerance =
                Mathf.Max(0f, stabilityTolerance);

            stabilitySamplesRequired =
                Mathf.Max(1, stabilitySamplesRequired);

            voltageRange =
                Mathf.Max(0.001f, voltageRange);

            currentRange =
                Mathf.Max(0.001f, currentRange);

            resistanceRange =
                Mathf.Max(0.001f, resistanceRange);

            continuityThreshold =
                Mathf.Max(0f, continuityThreshold);

            displayDecimals =
                Mathf.Clamp(displayDecimals, 0, 9);

            zeroThreshold =
                Mathf.Max(0.0000001f, zeroThreshold);

            ResolveReferences();

            /*
             * Make sure a manually selected range still matches
             * the selected measurement mode.
             */
            if (!IsRangeCompatible(range))
            {
                range = SparkMultimeterRange.Auto;
            }
        }

        private void Update()
        {
            if (!poweredOn)
            {
                return;
            }

            measurementTimer -= Time.unscaledDeltaTime;

            if (measurementTimer > 0f)
            {
                return;
            }

            measurementTimer = measurementInterval;

            PerformMeasurement();
        }


        // ============================================================
        // REFERENCE RESOLUTION
        // ============================================================

        private void ResolveReferences()
        {
            if (measurementSystem == null)
            {
                measurementSystem =
                    GetComponent<SparkMeasurementSystem>();
            }
        }


        // ============================================================
        // PROJECT SPARK INTERACTION
        // ============================================================

        public override bool CanInteract(
            in SparkInteractionContext context,
            out string reason)
        {
            if (!base.CanInteract(context, out reason))
            {
                return false;
            }

            return true;
        }

        public override SparkResult Inspect(
            in SparkInteractionContext context)
        {
            if (!CanInteract(context, out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            return SparkResult.Success();
        }


        // ============================================================
        // POWER
        // ============================================================

        public void SetPower(bool enabled)
        {
            if (poweredOn == enabled)
            {
                return;
            }

            poweredOn = enabled;

            SynchronizeOperationalState();

            if (!poweredOn)
            {
                ResetReading();
            }
            else
            {
                measurementTimer = 0f;

                ResetStability();

                CreateReadyReading();
            }

            PowerChanged?.Invoke(poweredOn);
        }

        public void TogglePower()
        {
            SetPower(!poweredOn);
        }

        private void SynchronizeOperationalState()
        {
            SetOperationalState(
                poweredOn
                    ? SparkOperationalState.Active
                    : SparkOperationalState.Standby);
        }


        // ============================================================
        // MODE
        // ============================================================

        public bool SetMode(
            SparkMultimeterMode value,
            out string reason)
        {
            if (value == SparkMultimeterMode.Off)
            {
                reason =
                    "Use the power control to turn the multimeter off.";

                return false;
            }

            if (mode == value)
            {
                reason = null;
                return true;
            }

            mode = value;

            /*
             * A mode change invalidates the stability history.
             */
            ResetStability();

            /*
             * Auto range is always safe after changing modes.
             */
            if (!IsRangeCompatible(range))
            {
                range = SparkMultimeterRange.Auto;

                RangeChanged?.Invoke(range);
            }

            if (poweredOn)
            {
                PerformMeasurement();
            }

            ModeChanged?.Invoke(mode);

            reason = null;

            return true;
        }

        public void SetMode(
            SparkMultimeterMode value)
        {
            SetMode(value, out _);
        }


        // ============================================================
        // RANGE
        // ============================================================

        public bool SetRange(
            SparkMultimeterRange value,
            out string reason)
        {
            if (!IsRangeCompatible(value))
            {
                reason =
                    "Selected range is incompatible with the current mode.";

                return false;
            }

            range = value;

            ResetStability();

            RangeChanged?.Invoke(range);

            if (poweredOn)
            {
                PerformMeasurement();
            }

            reason = null;

            return true;
        }

        public void SetRange(
            SparkMultimeterRange value)
        {
            SetRange(value, out _);
        }


        // ============================================================
        // PROBES
        // ============================================================

        public void SetProbeTerminals(
            SparkTerminal red,
            SparkTerminal black)
        {
            redProbeTerminal = red;
            blackProbeTerminal = black;

            ResetStability();

            if (poweredOn)
            {
                PerformMeasurement();
            }
        }

        public void SetRedProbe(
            SparkTerminal terminal)
        {
            redProbeTerminal = terminal;

            ResetStability();

            if (poweredOn)
            {
                PerformMeasurement();
            }
        }

        public void SetBlackProbe(
            SparkTerminal terminal)
        {
            blackProbeTerminal = terminal;

            ResetStability();

            if (poweredOn)
            {
                PerformMeasurement();
            }
        }

        public void ClearProbes()
        {
            redProbeTerminal = null;
            blackProbeTerminal = null;

            ResetReading();
        }

        public bool HasBothProbes =>
            redProbeTerminal != null &&
            blackProbeTerminal != null;


        // ============================================================
        // MEASURE NOW
        // ============================================================

        public SparkResult MeasureNow()
        {
            if (!poweredOn)
            {
                return SparkResult.Blocked(
                    "Multimeter is off.");
            }

            PerformMeasurement();

            if (!currentReading.Valid)
            {
                return SparkResult.Unavailable(
                    "Multimeter cannot obtain a valid reading.");
            }

            return SparkResult.Success();
        }


        // ============================================================
        // MEASUREMENT DISPATCH
        // ============================================================

        private void PerformMeasurement()
        {
            if (!poweredOn)
            {
                return;
            }

            if (redProbeTerminal == null ||
                blackProbeTerminal == null)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Open,
                    "---");

                return;
            }

            switch (mode)
            {
                case SparkMultimeterMode.VoltageDC:
                    MeasureVoltage();
                    break;

                case SparkMultimeterMode.CurrentDC:
                    MeasureCurrent();
                    break;

                case SparkMultimeterMode.Resistance:
                    MeasureResistance();
                    break;

                case SparkMultimeterMode.Continuity:
                    MeasureContinuity();
                    break;

                default:
                    SetInvalid(
                        SparkMultimeterDisplayState.Off,
                        "---");
                    break;
            }
        }


        // ============================================================
        // VOLTAGE
        // ============================================================

        private void MeasureVoltage()
        {
            if (measurementSystem == null)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "ERR");

                return;
            }

            SparkResult redResult =
                measurementSystem.TryMeasureTerminal(
                    redProbeTerminal,
                    SparkMeasurementType.Voltage,
                    out SparkMeasurementReading redReading);

            if (!redResult.Succeeded ||
                !redReading.Valid)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "---");

                return;
            }

            SparkResult blackResult =
                measurementSystem.TryMeasureTerminal(
                    blackProbeTerminal,
                    SparkMeasurementType.Voltage,
                    out SparkMeasurementReading blackReading);

            if (!blackResult.Succeeded ||
                !blackReading.Valid)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "---");

                return;
            }

            /*
             * True differential measurement:
             *
             * V = Vred - Vblack
             */
            float value =
                redReading.Value -
                blackReading.Value;

            PublishNumericReading(
                value,
                SparkMultimeterMode.VoltageDC,
                SparkMultimeterDisplayState.Measuring);
        }


        // ============================================================
        // CURRENT
        // ============================================================

        private void MeasureCurrent()
        {
            if (measurementSystem == null)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "ERR");

                return;
            }

            SparkResult result =
                measurementSystem.TryMeasureTerminal(
                    redProbeTerminal,
                    SparkMeasurementType.Current,
                    out SparkMeasurementReading reading);

            if (!result.Succeeded ||
                !reading.Valid)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "---");

                return;
            }

            PublishNumericReading(
                reading.Value,
                SparkMultimeterMode.CurrentDC,
                SparkMultimeterDisplayState.Measuring);
        }


        // ============================================================
        // RESISTANCE
        // ============================================================

        private void MeasureResistance()
        {
            if (measurementSystem == null)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "ERR");

                return;
            }

            SparkResult result =
                measurementSystem.TryMeasureTerminal(
                    redProbeTerminal,
                    SparkMeasurementType.Resistance,
                    out SparkMeasurementReading reading);

            if (!result.Succeeded ||
                !reading.Valid)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Open,
                    "OL");

                return;
            }

            PublishNumericReading(
                reading.Value,
                SparkMultimeterMode.Resistance,
                SparkMultimeterDisplayState.Measuring);
        }


        // ============================================================
        // CONTINUITY
        // ============================================================

        private void MeasureContinuity()
        {
            if (measurementSystem == null)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "ERR");

                return;
            }

            SparkResult result =
                measurementSystem.TryMeasureTerminal(
                    redProbeTerminal,
                    SparkMeasurementType.Continuity,
                    out SparkMeasurementReading reading);

            if (!result.Succeeded ||
                !reading.Valid)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Invalid,
                    "OL");

                return;
            }

            bool continuous =
                reading.Value <= continuityThreshold;

            float value =
                continuous
                    ? reading.Value
                    : float.PositiveInfinity;

            PublishNumericReading(
                value,
                SparkMultimeterMode.Continuity,
                continuous
                    ? SparkMultimeterDisplayState.Stable
                    : SparkMultimeterDisplayState.Open,
                continuous);
        }


        // ============================================================
        // READING PUBLICATION
        // ============================================================

        private void PublishNumericReading(
            float value,
            SparkMultimeterMode measurementMode,
            SparkMultimeterDisplayState baseState,
            bool continuity = false)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                SetInvalid(
                    SparkMultimeterDisplayState.OverRange,
                    "OL");

                return;
            }

            if (Mathf.Abs(value) < zeroThreshold)
            {
                value = 0f;
            }

            bool negative =
                value < 0f;

            UpdateStability(value);

            float absoluteValue =
                Mathf.Abs(value);

            SparkMultimeterRange resolvedRange =
                ResolveRange(
                    absoluteValue,
                    measurementMode);

            float rangeLimit =
                GetRangeLimit(
                    resolvedRange);

            if (absoluteValue > rangeLimit)
            {
                currentReading =
                    new SparkMultimeterReading(
                        true,
                        false,
                        negative,
                        true,
                        continuity,
                        value,
                        value,
                        measurementMode,
                        resolvedRange,
                        GetUnit(measurementMode),
                        "OL",
                        SparkMultimeterDisplayState.OverRange);

                ReadingChanged?.Invoke(currentReading);

                return;
            }

            string unit =
                GetUnit(measurementMode);

            string text =
                FormatValue(
                    value,
                    measurementMode,
                    resolvedRange);

            SparkMultimeterDisplayState state =
                baseState;

            if (negative)
            {
                state =
                    SparkMultimeterDisplayState.Negative;
            }
            else if (stableSampleCount >=
                     stabilitySamplesRequired)
            {
                state =
                    SparkMultimeterDisplayState.Stable;
            }

            currentReading =
                new SparkMultimeterReading(
                    true,
                    stableSampleCount >=
                    stabilitySamplesRequired,
                    negative,
                    false,
                    continuity,
                    value,
                    ConvertToDisplayValue(
                        value,
                        resolvedRange),
                    measurementMode,
                    resolvedRange,
                    unit,
                    text,
                    state);

            ReadingChanged?.Invoke(currentReading);
        }


        // ============================================================
        // AUTO RANGE
        // ============================================================

        private SparkMultimeterRange ResolveRange(
            float absoluteValue,
            SparkMultimeterMode measurementMode)
        {
            if (range != SparkMultimeterRange.Auto)
            {
                return range;
            }

            switch (measurementMode)
            {
                case SparkMultimeterMode.VoltageDC:

                    if (absoluteValue < 0.1f)
                    {
                        return SparkMultimeterRange.Millivolts;
                    }

                    return SparkMultimeterRange.Volts;

                case SparkMultimeterMode.CurrentDC:

                    if (absoluteValue < 1f)
                    {
                        return SparkMultimeterRange.Milliamps;
                    }

                    return SparkMultimeterRange.Amps;

                case SparkMultimeterMode.Resistance:

                    if (absoluteValue < 1000f)
                    {
                        return SparkMultimeterRange.Ohms;
                    }

                    if (absoluteValue < 1000000f)
                    {
                        return SparkMultimeterRange.Kiloohms;
                    }

                    return SparkMultimeterRange.Megaohms;

                case SparkMultimeterMode.Continuity:
                    return SparkMultimeterRange.Ohms;

                default:
                    return SparkMultimeterRange.Auto;
            }
        }


        // ============================================================
        // RANGE LIMIT
        // ============================================================

        private float GetRangeLimit(
            SparkMultimeterRange selectedRange)
        {
            switch (selectedRange)
            {
                case SparkMultimeterRange.Millivolts:
                    return 999.9f;

                case SparkMultimeterRange.Volts:
                    return voltageRange;

                case SparkMultimeterRange.Milliamps:
                    return 999.9f;

                case SparkMultimeterRange.Amps:
                    return currentRange;

                case SparkMultimeterRange.Ohms:
                    return 999.9f;

                case SparkMultimeterRange.Kiloohms:
                    return 999.9f;

                case SparkMultimeterRange.Megaohms:
                    return resistanceRange;

                default:
                    return float.MaxValue;
            }
        }


        // ============================================================
        // DISPLAY VALUE
        // ============================================================

        private float ConvertToDisplayValue(
            float value,
            SparkMultimeterRange selectedRange)
        {
            switch (selectedRange)
            {
                case SparkMultimeterRange.Millivolts:
                    return value * 1000f;

                case SparkMultimeterRange.Milliamps:
                    return value * 1000f;

                case SparkMultimeterRange.Kiloohms:
                    return value / 1000f;

                case SparkMultimeterRange.Megaohms:
                    return value / 1000000f;

                default:
                    return value;
            }
        }


        // ============================================================
        // FORMATTING
        // ============================================================

        private string FormatValue(
            float value,
            SparkMultimeterMode measurementMode,
            SparkMultimeterRange selectedRange)
        {
            float displayValue =
                ConvertToDisplayValue(
                    value,
                    selectedRange);

            float absolute =
                Mathf.Abs(displayValue);

            if (useScientificNotationForVerySmallValues &&
                absolute > 0f &&
                absolute < 0.001f)
            {
                return displayValue.ToString(
                    "0.###E+0");
            }

            return displayValue.ToString(
                "F" + displayDecimals);
        }

        private string GetUnit(
            SparkMultimeterMode measurementMode)
        {
            switch (measurementMode)
            {
                case SparkMultimeterMode.VoltageDC:
                    return "V";

                case SparkMultimeterMode.CurrentDC:
                    return "A";

                case SparkMultimeterMode.Resistance:
                case SparkMultimeterMode.Continuity:
                    return "Ω";

                default:
                    return string.Empty;
            }
        }


        // ============================================================
        // RANGE VALIDATION
        // ============================================================

        private bool IsRangeCompatible(
            SparkMultimeterRange value)
        {
            if (value == SparkMultimeterRange.Auto)
            {
                return true;
            }

            switch (mode)
            {
                case SparkMultimeterMode.VoltageDC:

                    return value ==
                           SparkMultimeterRange.Millivolts ||
                           value ==
                           SparkMultimeterRange.Volts;

                case SparkMultimeterMode.CurrentDC:

                    return value ==
                           SparkMultimeterRange.Milliamps ||
                           value ==
                           SparkMultimeterRange.Amps;

                case SparkMultimeterMode.Resistance:
                case SparkMultimeterMode.Continuity:

                    return value ==
                           SparkMultimeterRange.Ohms ||
                           value ==
                           SparkMultimeterRange.Kiloohms ||
                           value ==
                           SparkMultimeterRange.Megaohms;

                default:
                    return false;
            }
        }


        // ============================================================
        // STABILITY
        // ============================================================

        private void UpdateStability(
            float value)
        {
            if (!hasPreviousValue)
            {
                previousValue = value;
                stableSampleCount = 1;
                hasPreviousValue = true;

                return;
            }

            if (Mathf.Abs(
                    value -
                    previousValue) <=
                stabilityTolerance)
            {
                stableSampleCount++;
            }
            else
            {
                stableSampleCount = 0;
            }

            previousValue = value;
        }

        private void ResetStability()
        {
            previousValue = 0f;
            stableSampleCount = 0;
            hasPreviousValue = false;
        }


        // ============================================================
        // READY / RESET
        // ============================================================

        private void CreateReadyReading()
        {
            currentReading =
                new SparkMultimeterReading(
                    false,
                    false,
                    false,
                    false,
                    false,
                    0f,
                    0f,
                    mode,
                    range,
                    GetUnit(mode),
                    "---",
                    SparkMultimeterDisplayState.Ready);

            ReadingChanged?.Invoke(
                currentReading);
        }

        private void ResetReading()
        {
            ResetStability();

            currentReading =
                SparkMultimeterReading.Invalid;

            ReadingChanged?.Invoke(
                currentReading);
        }


        // ============================================================
        // INVALID / ERROR
        // ============================================================

        private void SetInvalid(
            SparkMultimeterDisplayState state,
            string text)
        {
            currentReading =
                new SparkMultimeterReading(
                    false,
                    false,
                    false,
                    state ==
                    SparkMultimeterDisplayState.OverRange,
                    false,
                    0f,
                    0f,
                    mode,
                    range,
                    GetUnit(mode),
                    text,
                    state);

            ReadingChanged?.Invoke(
                currentReading);
        }
    }
}