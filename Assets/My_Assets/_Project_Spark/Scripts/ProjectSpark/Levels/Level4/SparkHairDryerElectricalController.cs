using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Hair dryer appliance-level electrical controller.
    ///
    /// This component does NOT perform electrical calculations.
    ///
    /// SparkElectricalSolver remains responsible for:
    /// - Circuit topology
    /// - Node voltages
    /// - Component currents
    /// - Component power
    /// - Indexed switch voltage
    ///
    /// This controller only reads the already-solved electrical
    /// component states and exposes them to the rest of the
    /// hair-dryer system.
    ///
    /// Visual behavior belongs to:
    ///     SparkHairDryerVisualController
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerElectricalController : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("Electrical Components")]

        [SerializeField]
        private SparkHairDryerMotorElectrical motorElectrical;

        [SerializeField]
        private SparkHairDryerHeaterElectrical heaterElectrical;

        [SerializeField]
        private SparkSwitchIndex speedSwitch;

        // ============================================================
        // THRESHOLDS
        // ============================================================

        [Header("Motor State Thresholds")]

        [SerializeField, Min(0f)]
        private float motorVoltageThreshold = 1f;

        [SerializeField, Min(0f)]
        private float motorCurrentThreshold = 0.001f;

        [Header("Heater State Thresholds")]

        [SerializeField, Min(0f)]
        private float heaterVoltageThreshold = 1f;

        [SerializeField, Min(0f)]
        private float heaterCurrentThreshold = 0.001f;

        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private bool motorRunning;
        private bool heaterHeating;

        private int speedIndex;

        // ============================================================
        // PUBLIC REFERENCES
        // ============================================================

        public SparkHairDryerMotorElectrical MotorElectrical =>
            motorElectrical;

        public SparkHairDryerHeaterElectrical HeaterElectrical =>
            heaterElectrical;

        public SparkSwitchIndex SpeedSwitch =>
            speedSwitch;

        // ============================================================
        // MOTOR ELECTRICAL STATE
        // ============================================================

        public float MotorVoltage =>
            motorElectrical != null
                ? motorElectrical.Voltage
                : 0f;

        public float MotorCurrent =>
            motorElectrical != null
                ? motorElectrical.Current
                : 0f;

        public float MotorPower =>
            motorElectrical != null
                ? motorElectrical.Power
                : 0f;

        public bool MotorHasVoltage =>
            Mathf.Abs(MotorVoltage) >=
            motorVoltageThreshold;

        public bool MotorHasCurrent =>
            Mathf.Abs(MotorCurrent) >=
            motorCurrentThreshold;

        public bool MotorConducting =>
            motorElectrical != null &&
            motorElectrical.Conduction ==
            SparkConductionState.Conducting;

        public bool IsMotorRunning =>
            motorRunning;

        // ============================================================
        // HEATER ELECTRICAL STATE
        // ============================================================

        public float HeaterVoltage =>
            heaterElectrical != null
                ? heaterElectrical.Voltage
                : 0f;

        public float HeaterCurrent =>
            heaterElectrical != null
                ? heaterElectrical.Current
                : 0f;

        public float HeaterPower =>
            heaterElectrical != null
                ? heaterElectrical.Power
                : 0f;

        public bool HeaterHasVoltage =>
            Mathf.Abs(HeaterVoltage) >=
            heaterVoltageThreshold;

        public bool HeaterHasCurrent =>
            Mathf.Abs(HeaterCurrent) >=
            heaterCurrentThreshold;

        public bool HeaterConducting =>
            heaterElectrical != null &&
            heaterElectrical.Conduction ==
            SparkConductionState.Conducting;

        public bool IsHeaterHeating =>
            heaterHeating;

        // ============================================================
        // SPEED SWITCH STATE
        // ============================================================

        /// <summary>
        /// Actual physical index of SparkSwitchIndex.
        ///
        /// 0 = OFF
        /// 1 = LOW
        /// 2 = HIGH
        ///
        /// This value is NOT converted into voltage here.
        /// The electrical solver is authoritative for voltage.
        /// </summary>
        public int SpeedIndex =>
            speedIndex;

        public bool IsOff =>
            speedIndex == 0;

        public bool IsLowSpeed =>
            speedIndex == 1;

        public bool IsHighSpeed =>
            speedIndex >= 2;

        /// <summary>
        /// Returns the actual switch percentage.
        ///
        /// This is useful for diagnostics because the physical
        /// switch index and the actual electrical motor voltage
        /// are not necessarily the same thing.
        /// </summary>
        public float SpeedVoltagePercent
        {
            get
            {
                if (speedSwitch == null)
                    return 0f;

                return speedSwitch.CurrentIndexVoltagePercent;
            }
        }

        // ============================================================
        // APPLIANCE STATE
        // ============================================================

        public bool IsMotorPowered =>
            MotorHasVoltage;

        public bool IsHeaterPowered =>
            HeaterHasVoltage;

        public bool IsOperating =>
            IsMotorRunning;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
            RefreshState();
        }

        private void OnEnable()
        {
            RefreshState();
        }

        // ============================================================
        // REFERENCE RESOLUTION
        // ============================================================

        private void ResolveReferences()
        {
            if (motorElectrical == null)
            {
                motorElectrical =
                    GetComponentInChildren<
                        SparkHairDryerMotorElectrical>(
                            true);
            }

            if (heaterElectrical == null)
            {
                heaterElectrical =
                    GetComponentInChildren<
                        SparkHairDryerHeaterElectrical>(
                            true);
            }

            if (speedSwitch == null)
            {
                speedSwitch =
                    GetComponentInChildren<
                        SparkSwitchIndex>(
                            true);
            }
        }

        // ============================================================
        // STATE UPDATE
        // ============================================================

        /// <summary>
        /// Reads the already-solved electrical state.
        ///
        /// IMPORTANT:
        /// This method does NOT calculate electrical values.
        /// </summary>
        public void RefreshState()
        {
            UpdateMotorState();
            UpdateHeaterState();
            UpdateSpeedState();
        }

        // ============================================================
        // MOTOR STATE
        // ============================================================

        private void UpdateMotorState()
        {
            if (motorElectrical == null)
            {
                motorRunning = false;
                return;
            }

            /*
             * The motor is considered running only when the actual
             * electrical component says:
             *
             * - electrically enabled
             * - has sufficient voltage
             * - has sufficient current
             * - is conducting
             *
             * No switch-index assumption is used here.
             */
            motorRunning =
                motorElectrical.ElectricalEnabled &&
                MotorHasVoltage &&
                MotorHasCurrent &&
                MotorConducting;
        }

        // ============================================================
        // HEATER STATE
        // ============================================================

        private void UpdateHeaterState()
        {
            if (heaterElectrical == null)
            {
                heaterHeating = false;
                return;
            }

            /*
             * Heater state is based entirely on its actual
             * electrical state.
             */
            heaterHeating =
                heaterElectrical.ElectricalEnabled &&
                HeaterHasVoltage &&
                HeaterHasCurrent &&
                HeaterConducting;
        }

        // ============================================================
        // SPEED STATE
        // ============================================================

        private void UpdateSpeedState()
        {
            if (speedSwitch == null)
            {
                speedIndex = 0;
                return;
            }

            /*
             * IMPORTANT:
             *
             * Read the actual physical switch index.
             *
             * Do NOT derive this from motor voltage.
             *
             * Do NOT derive this from motor current.
             *
             * The electrical solver decides the resulting motor
             * voltage independently.
             */
            speedIndex =
                Mathf.Clamp(
                    speedSwitch.CurrentIndex,
                    0,
                    Mathf.Max(
                        0,
                        speedSwitch.PositionCount - 1));
        }

        // ============================================================
        // REFERENCE SETTERS
        // ============================================================

        public void SetMotorElectrical(
            SparkHairDryerMotorElectrical motor)
        {
            motorElectrical = motor;

            RefreshState();
        }

        public void SetHeaterElectrical(
            SparkHairDryerHeaterElectrical heater)
        {
            heaterElectrical = heater;

            RefreshState();
        }

        public void SetSpeedSwitch(
            SparkSwitchIndex switchComponent)
        {
            speedSwitch = switchComponent;

            RefreshState();
        }

        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("Refresh Electrical State")]
        private void TestRefresh()
        {
            RefreshState();
        }

        [ContextMenu("Log Electrical State")]
        private void TestLogState()
        {
            RefreshState();

            Debug.Log(
                $"[HAIR DRYER ELECTRICAL]\n" +

                $"MOTOR\n" +
                $"  Running      = {IsMotorRunning}\n" +
                $"  Voltage      = {MotorVoltage:F3} V\n" +
                $"  Current      = {MotorCurrent:F6} A\n" +
                $"  Power        = {MotorPower:F3} W\n" +
                $"  HasVoltage   = {MotorHasVoltage}\n" +
                $"  HasCurrent   = {MotorHasCurrent}\n" +
                $"  Conducting   = {MotorConducting}\n" +

                $"\nHEATER\n" +
                $"  Heating      = {IsHeaterHeating}\n" +
                $"  Voltage      = {HeaterVoltage:F3} V\n" +
                $"  Current      = {HeaterCurrent:F6} A\n" +
                $"  Power        = {HeaterPower:F3} W\n" +
                $"  HasVoltage   = {HeaterHasVoltage}\n" +
                $"  HasCurrent   = {HeaterHasCurrent}\n" +
                $"  Conducting   = {HeaterConducting}\n" +

                $"\nSWITCH\n" +
                $"  Index        = {SpeedIndex}\n" +
                $"  Percent      = {SpeedVoltagePercent:F1}%\n" +
                $"  Off          = {IsOff}\n" +
                $"  Low          = {IsLowSpeed}\n" +
                $"  High         = {IsHighSpeed}\n",

                this);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            motorVoltageThreshold =
                Mathf.Max(
                    0f,
                    motorVoltageThreshold);

            motorCurrentThreshold =
                Mathf.Max(
                    0f,
                    motorCurrentThreshold);

            heaterVoltageThreshold =
                Mathf.Max(
                    0f,
                    heaterVoltageThreshold);

            heaterCurrentThreshold =
                Mathf.Max(
                    0f,
                    heaterCurrentThreshold);
        }
    }
}