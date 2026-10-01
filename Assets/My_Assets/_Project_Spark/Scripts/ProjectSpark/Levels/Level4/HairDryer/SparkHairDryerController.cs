using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// High-level behavior controller for the Project Spark hair dryer.
    ///
    /// Electrical calculation remains entirely inside:
    ///     SparkElectricalSolver
    ///
    /// This component only translates the solved electrical state into
    /// appliance behavior and visual behavior.
    ///
    /// Electrical chain:
    ///
    ///     Supply
    ///        |
    ///     SparkSwitchIndex
    ///        |
    ///        +---- Motor
    ///        |
    ///        +---- Heater
    ///
    /// Behavior:
    ///
    ///     Motor running  -> fan + airflow
    ///     Heater heating -> warm/hot airflow
    ///
    /// No electrical calculations are performed here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerController : MonoBehaviour
    {
        // ============================================================
        // ELECTRICAL COMPONENTS
        // ============================================================

        [Header("Electrical")]
        [SerializeField]
        private SparkHairDryerMotorElectrical motorElectrical;

        [SerializeField]
        private SparkHairDryerHeaterElectrical heaterElectrical;

        [SerializeField]
        private SparkSwitchIndex speedSwitch;

        // ============================================================
        // VISUAL COMPONENTS
        // ============================================================

        [Header("Visual")]
        [SerializeField]
        private SparkHairDryerMotorVisual motorVisual;

        [SerializeField]
        private SparkHairDryerAirflowVisual airflowVisual;

        // ============================================================
        // AIRFLOW SETTINGS
        // ============================================================

        [Header("Airflow")]
        [SerializeField, Range(0f, 1f)]
        private float lowSpeedAirflow = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float highSpeedAirflow = 1f;

        // ============================================================
        // TEMPERATURE SETTINGS
        // ============================================================

        [Header("Temperature")]
        [SerializeField, Range(0f, 1f)]
        private float coldAirTemperature = 0f;

        [SerializeField, Range(0f, 1f)]
        private float lowHeatTemperature = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float highHeatTemperature = 1f;

        // ============================================================
        // POWER REFERENCE
        // ============================================================

        [Header("Power Reference")]
        [Tooltip(
            "Expected maximum heater power used to normalize heater " +
            "power into the 0-1 airflow temperature range.")]
        [SerializeField, Min(0.000001f)]
        private float maximumHeaterPower = 1800f;

        // ============================================================
        // STATE
        // ============================================================

        private bool motorRunning;
        private bool heaterHeating;

        private int currentSpeedIndex;

        private float airflowTarget;
        private float temperatureTarget;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsRunning =>
            motorRunning;

        public bool IsHeating =>
            heaterHeating;

        public bool IsOperating =>
            motorRunning;

        public int SpeedIndex =>
            currentSpeedIndex;

        public float AirflowTarget =>
            airflowTarget;

        public float TemperatureTarget =>
            temperatureTarget;

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

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();

            ResetRuntimeState();
        }

        private void Update()
        {
            UpdateElectricalState();
            ApplyBehavior();
        }

        // ============================================================
        // REFERENCES
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

            if (motorVisual == null)
            {
                motorVisual =
                    GetComponentInChildren<
                        SparkHairDryerMotorVisual>(
                            true);
            }

            if (airflowVisual == null)
            {
                airflowVisual =
                    GetComponentInChildren<
                        SparkHairDryerAirflowVisual>(
                            true);
            }
        }

        // ============================================================
        // STATE
        // ============================================================

        private void ResetRuntimeState()
        {
            motorRunning = false;
            heaterHeating = false;

            currentSpeedIndex = 0;

            airflowTarget = 0f;
            temperatureTarget = 0f;

            if (airflowVisual != null)
            {
                airflowVisual.StopAirflow();
                airflowVisual.SetAirTemperature(0f);
            }
        }

        private void UpdateElectricalState()
        {
            motorRunning =
                motorElectrical != null &&
                motorElectrical.IsRunning;

            heaterHeating =
                heaterElectrical != null &&
                heaterElectrical.IsHeating;

            currentSpeedIndex =
                speedSwitch != null
                    ? speedSwitch.Index
                    : (motorRunning ? 1 : 0);
        }

        // ============================================================
        // BEHAVIOR
        // ============================================================

        private void ApplyBehavior()
        {
            ApplyMotorBehavior();
            ApplyAirflowBehavior();
        }

        // ============================================================
        // MOTOR
        // ============================================================

        private void ApplyMotorBehavior()
        {
            if (motorVisual == null)
                return;

            motorVisual.SetRunning(motorRunning);
        }

        // ============================================================
        // AIRFLOW
        // ============================================================

        private void ApplyAirflowBehavior()
        {
            if (airflowVisual == null)
                return;

            if (!motorRunning)
            {
                airflowTarget = 0f;
                temperatureTarget = 0f;

                airflowVisual.StopAirflow();
                return;
            }

            airflowTarget =
                GetAirflowForSpeed();

            temperatureTarget =
                GetTemperatureForHeater();

            airflowVisual.SetAirflowStrength(
                airflowTarget);

            airflowVisual.SetAirTemperature(
                temperatureTarget);
        }

        // ============================================================
        // SPEED
        // ============================================================

        private float GetAirflowForSpeed()
        {
            if (!motorRunning)
                return 0f;

            switch (currentSpeedIndex)
            {
                case 0:
                    return 0f;

                case 1:
                    return Mathf.Clamp01(
                        lowSpeedAirflow);

                case 2:
                    return Mathf.Clamp01(
                        highSpeedAirflow);

                default:
                    return Mathf.Clamp01(
                        highSpeedAirflow);
            }
        }

        // ============================================================
        // HEATER
        // ============================================================

        private float GetTemperatureForHeater()
        {
            if (!motorRunning)
                return 0f;

            if (!heaterHeating ||
                heaterElectrical == null)
            {
                return Mathf.Clamp01(
                    coldAirTemperature);
            }

            float power =
                Mathf.Max(
                    0f,
                    heaterElectrical.Power);

            float normalizedPower =
                Mathf.Clamp01(
                    power /
                    Mathf.Max(
                        0.000001f,
                        maximumHeaterPower));

            /*
             * Use the electrical heater power as the physical
             * heat intensity.
             *
             * This gives the visual system a continuous value
             * rather than simply ON/OFF.
             */

            float temperature =
                Mathf.Lerp(
                    lowHeatTemperature,
                    highHeatTemperature,
                    normalizedPower);

            return Mathf.Clamp01(
                temperature);
        }

        // ============================================================
        // PUBLIC CONTROL / REFRESH
        // ============================================================

        public void RefreshBehavior()
        {
            UpdateElectricalState();
            ApplyBehavior();
        }

        public void SetMotorElectrical(
            SparkHairDryerMotorElectrical motor)
        {
            motorElectrical = motor;
            RefreshBehavior();
        }

        public void SetHeaterElectrical(
            SparkHairDryerHeaterElectrical heater)
        {
            heaterElectrical = heater;
            RefreshBehavior();
        }

        public void SetSpeedSwitch(
            SparkSwitchIndex switchIndex)
        {
            speedSwitch = switchIndex;
            RefreshBehavior();
        }

        public void SetMotorVisual(
            SparkHairDryerMotorVisual visual)
        {
            motorVisual = visual;
            RefreshBehavior();
        }

        public void SetAirflowVisual(
            SparkHairDryerAirflowVisual visual)
        {
            airflowVisual = visual;
            RefreshBehavior();
        }

        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("Refresh Behavior")]
        private void TestRefresh()
        {
            RefreshBehavior();
        }

        [ContextMenu("Test Stop")]
        private void TestStop()
        {
            motorRunning = false;
            heaterHeating = false;

            if (motorVisual != null)
                motorVisual.SetRunning(false);

            if (airflowVisual != null)
                airflowVisual.StopAirflow();
        }
    }
}
