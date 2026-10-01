using System;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;

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

        [Header("Circuit")]
[SerializeField]
private SparkCircuitSystem circuit;


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



            [Header("Electrical Current Path")]
[SerializeField]
private SparkMultimeterElectricalComponent electricalComponent;

[SerializeField]
private SparkTerminal currentRedTerminal;

[SerializeField]
private SparkTerminal currentBlackTerminal;



public SparkMultimeterElectricalComponent ElectricalComponent =>
    electricalComponent;

public SparkTerminal CurrentRedTerminal =>
    currentRedTerminal;

public SparkTerminal CurrentBlackTerminal =>
    currentBlackTerminal;


    // ============================================================
// REAL CURRENT-MEASUREMENT STATE
// ============================================================

public bool IsCurrentMeasurementActive =>
    poweredOn &&
    mode == SparkMultimeterMode.CurrentDC;

private SparkMultimeterProbe currentRedProbe;
private SparkMultimeterProbe currentBlackProbe;

private SparkTerminal currentRedTarget;
private SparkTerminal currentBlackTarget;

private ulong currentRedConnectionId;
private ulong currentBlackConnectionId;


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


        private int probeTipContactCount;

public bool ProbesPhysicallyTouching =>
    probeTipContactCount > 0;

public void SetProbeTipContact(bool touching)
{
    if (touching)
    {
        probeTipContactCount++;
    }
    else
    {
        probeTipContactCount =
            Mathf.Max(0, probeTipContactCount - 1);
    }

/*    Debug.Log(
        $"[MULTIMETER] PROBE TIP CONTACT STATE → " +
        $"Touching={ProbesPhysicallyTouching} | " +
        $"Count={probeTipContactCount}",
        this);*/
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
// PROBE CONNECTION NOTIFICATION
// ============================================================

public void NotifyProbeConnected(
    SparkMultimeterProbe probe,
    SparkTerminal target)
{
    if (probe == null || target == null)
        return;

    if (probe.ProbeColor ==
        SparkMultimeterProbeColor.Red)
    {
        currentRedProbe = probe;
        currentRedTarget = target;
    }
    else
    {
        currentBlackProbe = probe;
        currentBlackTarget = target;
    }

    if (IsCurrentMeasurementActive)
    {
        RebuildCurrentMeasurementPath();
    }
}

public void NotifyProbeDisconnected(
    SparkMultimeterProbe probe,
    SparkTerminal target)
{
    if (probe == null)
        return;

    if (probe.ProbeColor ==
        SparkMultimeterProbeColor.Red)
    {
        RemoveCurrentRedConnection();

        currentRedProbe = null;
        currentRedTarget = null;
    }
    else
    {
        RemoveCurrentBlackConnection();

        currentBlackProbe = null;
        currentBlackTarget = null;
    }
}


// ============================================================
// CURRENT MEASUREMENT TOPOLOGY
// ============================================================

private void RebuildCurrentMeasurementPath()
{
    RemoveCurrentMeasurementConnections();

    if (!IsCurrentMeasurementActive)
        return;

    if (electricalComponent == null)
        return;

    if (currentRedTarget == null ||
        currentBlackTarget == null)
    {
        electricalComponent.SetElectricalEnabled(false);
        return;
    }

    if (currentRedTerminal == null ||
        currentBlackTerminal == null)
    {
        Debug.LogWarning(
            "[MULTIMETER] Current terminals are not configured.",
            this);

        electricalComponent.SetElectricalEnabled(false);
        return;
    }

    if (circuit == null)
    {
        Debug.LogWarning(
            "[MULTIMETER] Circuit system is missing.",
            this);

        electricalComponent.SetElectricalEnabled(false);
        return;
    }

    // ------------------------------------------------------------
    // CIRCUIT A -> METER RED
    // ------------------------------------------------------------

    bool redCreated =
        circuit.TryCreateConnection(
            currentRedTarget,
            currentRedTerminal,
            SparkConnectionKind.Connector,
            SparkConnectionDirection.Bidirectional,
            out SparkCircuitConnection redConnection);

    if (!redCreated ||
        redConnection == null)
    {
        Debug.LogWarning(
            "[MULTIMETER] Failed to connect RED current path.",
            this);

        electricalComponent.SetElectricalEnabled(false);
        return;
    }

    currentRedConnectionId =
        redConnection.Id;

    // ------------------------------------------------------------
    // METER BLACK -> CIRCUIT B
    // ------------------------------------------------------------

    bool blackCreated =
        circuit.TryCreateConnection(
            currentBlackTerminal,
            currentBlackTarget,
            SparkConnectionKind.Connector,
            SparkConnectionDirection.Bidirectional,
            out SparkCircuitConnection blackConnection);

    if (!blackCreated ||
        blackConnection == null)
    {
        circuit.TryRemoveConnection(
            currentRedConnectionId,
            out _);

        currentRedConnectionId = 0UL;

        Debug.LogWarning(
            "[MULTIMETER] Failed to connect BLACK current path.",
            this);

        electricalComponent.SetElectricalEnabled(false);
        return;
    }

    currentBlackConnectionId =
        blackConnection.Id;

    electricalComponent.SetElectricalEnabled(true);

    Debug.Log(
        $"[MULTIMETER] CURRENT PATH ACTIVE → " +
        $"{currentRedTarget.name} -> METER -> {currentBlackTarget.name}",
        this);
}


private void RemoveCurrentMeasurementConnections()
{
    if (circuit != null)
    {
        if (currentRedConnectionId != 0UL)
        {
            circuit.TryRemoveConnection(
                currentRedConnectionId,
                out _);
        }

        if (currentBlackConnectionId != 0UL)
        {
            circuit.TryRemoveConnection(
                currentBlackConnectionId,
                out _);
        }
    }

    currentRedConnectionId = 0UL;
    currentBlackConnectionId = 0UL;

    if (electricalComponent != null)
    {
        electricalComponent.SetElectricalEnabled(false);
    }
}

private void RemoveCurrentRedConnection()
{
    if (circuit != null &&
        currentRedConnectionId != 0UL)
    {
        circuit.TryRemoveConnection(
            currentRedConnectionId,
            out _);
    }

    currentRedConnectionId = 0UL;

    if (currentBlackTarget == null &&
        electricalComponent != null)
    {
        electricalComponent.SetElectricalEnabled(false);
    }
}

private void RemoveCurrentBlackConnection()
{
    if (circuit != null &&
        currentBlackConnectionId != 0UL)
    {
        circuit.TryRemoveConnection(
            currentBlackConnectionId,
            out _);
    }

    currentBlackConnectionId = 0UL;

    if (currentRedTarget == null &&
        electricalComponent != null)
    {
        electricalComponent.SetElectricalEnabled(false);
    }
}

private void ReconfigureCurrentMeasurementMode()
{
    if (IsCurrentMeasurementActive)
    {
        currentRedProbe?.EnterCurrentMeasurementMode();
        currentBlackProbe?.EnterCurrentMeasurementMode();

        RebuildCurrentMeasurementPath();
    }
    else
    {
        RemoveCurrentMeasurementConnections();

        currentRedProbe?.ExitCurrentMeasurementMode();
        currentBlackProbe?.ExitCurrentMeasurementMode();
    }
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

    if (electricalComponent == null)
    {
        electricalComponent =
            GetComponent<SparkMultimeterElectricalComponent>();
    }

    if (electricalComponent != null)
    {
        if (currentRedTerminal == null)
        {
            currentRedTerminal =
                electricalComponent.RedTerminal;
        }

        if (currentBlackTerminal == null)
        {
            currentBlackTerminal =
                electricalComponent.BlackTerminal;
        }
    }

    if (circuit == null)
    {
        circuit =
            FindFirstObjectByType<SparkCircuitSystem>();
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

            ReconfigureCurrentMeasurementMode();

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
 * Current mode uses the real internal shunt path.
 */
ReconfigureCurrentMeasurementMode();

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

            ResetStability();

            if (poweredOn)
            {
                SetInvalid(
                    SparkMultimeterDisplayState.Open,
                    "---");
            }
            else
            {
                ResetReading();
            }
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

    /*
     * Continuity can be measured directly when
     * the two physical probe tips are touching.
     */
    if (mode == SparkMultimeterMode.Continuity &&
        ProbesPhysicallyTouching)
    {
        MeasureContinuity();
        return;
    }

    /*
     * All other measurements require both
     * probe terminals to be assigned.*/
   if (mode == SparkMultimeterMode.CurrentDC)
{
    MeasureCurrent();
    return;
}

/*
 * All other measurements require both
 * probe terminals to be assigned.
 */
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

    if (redProbeTerminal == null ||
        blackProbeTerminal == null)
    {
        SetInvalid(
            SparkMultimeterDisplayState.Open,
            "---");

        return;
    }

    /*
     * Read the solved voltage of each probe terminal
     * through the existing measurement system.
     *
     * Do NOT read SparkTerminal.ElectricalState directly
     * here because some electrical components, such as the
     * LED, do not necessarily publish their component voltage
     * to the terminal state in the same way as resistors.
     */

    SparkResult redResult =
        measurementSystem.TryMeasureTerminal(
            redProbeTerminal,
            SparkMeasurementType.Voltage,
            out SparkMeasurementReading redReading);

    if (!redResult.Succeeded ||
        !redReading.Valid)
    {
        SetInvalid(
            SparkMultimeterDisplayState.Open,
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
            SparkMultimeterDisplayState.Open,
            "---");

        return;
    }

    /*
     * TRUE DIFFERENTIAL DC VOLTAGE
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
    // ============================================================
    // CURRENT DC
    // ============================================================

    if (electricalComponent == null)
    {
        PublishNumericReading(
            0f,
            SparkMultimeterMode.CurrentDC,
            SparkMultimeterDisplayState.Invalid);

        return;
    }

    if (currentRedTerminal == null ||
        currentBlackTerminal == null)
    {
        PublishNumericReading(
            0f,
            SparkMultimeterMode.CurrentDC,
            SparkMultimeterDisplayState.Open);

        return;
    }

    if (currentRedTarget == null ||
        currentBlackTarget == null)
    {
        PublishNumericReading(
            0f,
            SparkMultimeterMode.CurrentDC,
            SparkMultimeterDisplayState.Open);

        return;
    }

    // Both probes must be connected.
    if (currentRedConnectionId == 0UL ||
        currentBlackConnectionId == 0UL)
    {
        PublishNumericReading(
            0f,
            SparkMultimeterMode.CurrentDC,
            SparkMultimeterDisplayState.Open);

        return;
    }

    // ============================================================
    // READ CURRENT FROM THE MULTIMETER'S ELECTRICAL COMPONENT
    // ============================================================

    Debug.Log(
    $"[MULTIMETER CURRENT DEBUG] " +
    $"RedTarget={currentRedTarget.name} " +
    $"V={currentRedTarget.ElectricalState.Voltage:F6} | " +
    $"BlackTarget={currentBlackTarget.name} " +
    $"V={currentBlackTarget.ElectricalState.Voltage:F6} | " +
    $"ShuntV={electricalComponent.ElectricalState.Voltage:F6} | " +
    $"ShuntI={electricalComponent.ElectricalState.Current:F6}",
    this);

    float current =
        electricalComponent.ElectricalState.Current;

    PublishNumericReading(
        current,
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
        measurementSystem.TryMeasureResistance(
            redProbeTerminal,
            blackProbeTerminal,
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
/*    Debug.Log(
        $"[MULTIMETER] CONTINUITY MEASURE → " +
        $"PhysicalContact={ProbesPhysicallyTouching}",
        this);*/

    if (measurementSystem == null)
    {
        SetInvalid(
            SparkMultimeterDisplayState.Invalid,
            "ERR");

        return;
    }

    if (ProbesPhysicallyTouching)
    {
        PublishNumericReading(
            0f,
            SparkMultimeterMode.Continuity,
            SparkMultimeterDisplayState.Measuring,
            true);

        return;
    }

    SparkResult result =
        measurementSystem.TryMeasureContinuity(
            redProbeTerminal,
            blackProbeTerminal,
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
        SparkMultimeterMode.Continuity,
        SparkMultimeterDisplayState.Measuring,
        true);
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

    float displayValue =
        ConvertToDisplayValue(
            value,
            resolvedRange);

    float absoluteDisplayValue =
        Mathf.Abs(displayValue);

    float rangeLimit =
        GetRangeLimit(
            resolvedRange);

    if (absoluteDisplayValue > rangeLimit)
    {
        currentReading =
            new SparkMultimeterReading(
                true,
                false,
                negative,
                true,
                continuity,
                value,
                displayValue,
                measurementMode,
                resolvedRange,
               GetUnit(
                measurementMode,
                resolvedRange),
                "OL",
                SparkMultimeterDisplayState.OverRange);

        Debug.Log(
            $"[MULTIMETER] Reading → " +
            $"State={currentReading.State} | " +
            $"Valid={currentReading.Valid} | " +
            $"Value={currentReading.Value} | " +
            $"Display={currentReading.DisplayValue} | " +
            $"Range={resolvedRange} | " +
            $"Text={currentReading.Text}",
            this);

        ReadingChanged?.Invoke(currentReading);

        return;
    }

   string unit =
    GetUnit(
        measurementMode,
        resolvedRange);

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
            displayValue,
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

    string numberText;

    if (useScientificNotationForVerySmallValues &&
        absolute > 0f &&
        absolute < 0.001f)
    {
        numberText =
            displayValue.ToString(
                "0.###E+0");
    }
    else
    {
        numberText =
            displayValue.ToString(
                "F" + displayDecimals);
    }

    string unit =
        GetUnit(
            measurementMode,
            selectedRange);

    if (string.IsNullOrEmpty(unit))
        return numberText;

    return numberText + " " + unit;
}
       private string GetUnit(
    SparkMultimeterMode measurementMode,
    SparkMultimeterRange selectedRange)
{
    switch (measurementMode)
    {
        case SparkMultimeterMode.VoltageDC:

            switch (selectedRange)
            {
                case SparkMultimeterRange.Millivolts:
                    return "mV";

                case SparkMultimeterRange.Volts:
                default:
                    return "V";
            }


        case SparkMultimeterMode.CurrentDC:

            switch (selectedRange)
            {
                case SparkMultimeterRange.Milliamps:
                    return "mA";

                case SparkMultimeterRange.Amps:
                default:
                    return "A";
            }


        case SparkMultimeterMode.Resistance:
        case SparkMultimeterMode.Continuity:

            switch (selectedRange)
            {
                case SparkMultimeterRange.Kiloohms:
                    return "kΩ";

                case SparkMultimeterRange.Megaohms:
                    return "MΩ";

                case SparkMultimeterRange.Ohms:
                default:
                    return "Ω";
            }


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
            GetUnit(
                mode,
                range),
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

       /* private void SetInvalid(
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
        }*/

        private void SetInvalid(
    SparkMultimeterDisplayState state,
    string text)
{
    /*Debug.Log(
        $"[MULTIMETER] SetInvalid → State={state} | Text={text} | " +
        $"Powered={poweredOn} | Red={redProbeTerminal} | Black={blackProbeTerminal}",
        this);*/

    currentReading =
    new SparkMultimeterReading(
        false,
        false,
        false,
        state == SparkMultimeterDisplayState.OverRange,
        false,
        0f,
        0f,
        mode,
        range,
        GetUnit(
            mode,
            range),
        text,
        state);
    ReadingChanged?.Invoke(currentReading);
}
    }
}