using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Hair dryer appliance-level electrical controller.
    ///
    /// This component does NOT perform electrical calculations.
    /// SparkElectricalSolver remains responsible for solving the circuit.
    ///
    /// Responsibilities:
    /// - Read SparkHairDryerMotorElectrical
    /// - Read SparkHairDryerHeaterElectrical
    /// - Read SparkSwitchIndex
    /// - Expose the final hair-dryer electrical state
    ///
    /// Visual behavior belongs to SparkHairDryerVisualController.
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

        [Header("State Thresholds")]
        [SerializeField, Min(0f)]
        private float motorVoltageThreshold = 1f;

        [SerializeField, Min(0f)]
        private float motorCurrentThreshold = 0.001f;

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
        // PUBLIC ELECTRICAL STATE
        // ============================================================

        public SparkHairDryerMotorElectrical MotorElectrical =>
            motorElectrical;

        public SparkHairDryerHeaterElectrical HeaterElectrical =>
            heaterElectrical;

        public SparkSwitchIndex SpeedSwitch =>
            speedSwitch;

        // ------------------------------------------------------------
        // Motor
        // ------------------------------------------------------------

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
            Mathf.Abs(MotorVoltage) >= motorVoltageThreshold;

        public bool MotorHasCurrent =>
            Mathf.Abs(MotorCurrent) >= motorCurrentThreshold;

        public bool MotorConducting =>
            motorElectrical != null &&
            motorElectrical.Conduction ==
            SparkConductionState.Conducting;

        public bool IsMotorRunning =>
            motorRunning;

        // ------------------------------------------------------------
        // Heater
        // ------------------------------------------------------------

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
            Mathf.Abs(HeaterVoltage) >= heaterVoltageThreshold;

        public bool HeaterHasCurrent =>
            Mathf.Abs(HeaterCurrent) >= heaterCurrentThreshold;

        public bool HeaterConducting =>
            heaterElectrical != null &&
            heaterElectrical.Conduction ==
            SparkConductionState.Conducting;

        public bool IsHeaterHeating =>
            heaterHeating;

        // ------------------------------------------------------------
        // Speed
        // ------------------------------------------------------------

        public int SpeedIndex =>
            speedIndex;

        public bool IsOff =>
            speedIndex == 0;

        public bool IsLowSpeed =>
            speedIndex == 1;

        public bool IsHighSpeed =>
            speedIndex >= 2;

        // ------------------------------------------------------------
        // Appliance
        // ------------------------------------------------------------

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
                        SparkHairDryerMotorElectrical>(true);
            }

            if (heaterElectrical == null)
            {
                heaterElectrical =
                    GetComponentInChildren<
                        SparkHairDryerHeaterElectrical>(true);
            }

            if (speedSwitch == null)
            {
                speedSwitch =
                    GetComponentInChildren<
                        SparkSwitchIndex>(true);
            }
        }

        // ============================================================
        // STATE UPDATE
        // ============================================================

        /// <summary>
        /// Reads the already-solved electrical state.
        ///
        /// No electrical calculations are performed here.
        /// </summary>
        public void RefreshState()
        {
            UpdateMotorState();
            UpdateHeaterState();
            UpdateSpeedState();
        }

        private void UpdateMotorState()
        {
            if (motorElectrical == null)
            {
                motorRunning = false;
                return;
            }

            motorRunning =
                motorElectrical.ElectricalEnabled &&
                MotorHasVoltage &&
                MotorHasCurrent &&
                MotorConducting;
        }

        private void UpdateHeaterState()
        {
            if (heaterElectrical == null)
            {
                heaterHeating = false;
                return;
            }

            heaterHeating =
                heaterElectrical.ElectricalEnabled &&
                HeaterHasVoltage &&
                HeaterHasCurrent &&
                HeaterConducting;
        }

        private void UpdateSpeedState()
        {
            if (speedSwitch == null)
            {
                speedIndex = motorRunning ? 1 : 0;
                return;
            }

            speedIndex = Mathf.Max(
                0,
                speedSwitch.Index);
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
        // DEBUG / TEST
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
                $"[Hair Dryer Electrical]\n" +
                $"Motor: " +
                $"Running={IsMotorRunning}, " +
                $"V={MotorVoltage:F2}, " +
                $"I={MotorCurrent:F3}, " +
                $"P={MotorPower:F2}\n" +
                $"Heater: " +
                $"Heating={IsHeaterHeating}, " +
                $"V={HeaterVoltage:F2}, " +
                $"I={HeaterCurrent:F3}, " +
                $"P={HeaterPower:F2}\n" +
                $"Speed Index={SpeedIndex}",
                this);
        }

        private void OnValidate()
        {
            motorVoltageThreshold =
                Mathf.Max(0f, motorVoltageThreshold);

            motorCurrentThreshold =
                Mathf.Max(0f, motorCurrentThreshold);

            heaterVoltageThreshold =
                Mathf.Max(0f, heaterVoltageThreshold);

            heaterCurrentThreshold =
                Mathf.Max(0f, heaterCurrentThreshold);
        }
    }
}