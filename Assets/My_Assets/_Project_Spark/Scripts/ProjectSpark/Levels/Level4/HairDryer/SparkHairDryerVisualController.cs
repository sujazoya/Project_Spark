using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Complete visual controller for Project Spark Level 4 Hair Dryer.
    ///
    /// Coordinates the already-existing visual components:
    ///
    /// - Power switch
    /// - Heat selector
    /// - Fan-speed selector
    /// - Power indicator
    /// - Fan motor
    /// - Heater
    /// - Airflow VFX
    /// - Thermal protection visual
    ///
    /// VISUAL ONLY.
    ///
    /// This component does NOT perform:
    /// - Electrical simulation
    /// - Voltage calculation
    /// - Current calculation
    /// - Resistance calculation
    /// - Motor electrical simulation
    /// - Heater electrical simulation
    /// - Real thermal protection
    ///
    /// Those systems will be connected later.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerVisualController : MonoBehaviour
    {
        // ============================================================
        // ENUMS
        // ============================================================

        public enum PowerState
        {
            Off,
            On
        }

        public enum HeatMode
        {
            Cold,
            Low,
            High
        }

        public enum FanSpeed
        {
            Off,
            Low,
            High
        }

        // ============================================================
        // COMPONENT REFERENCES
        // ============================================================

        [Header("Fan")]
        [SerializeField]
        private SparkHairDryerMotorVisual motorVisual;

        [Header("Heater")]
        [SerializeField]
        private SparkHairDryerHeaterVisual heaterVisual;

        [Header("Airflow")]
        [SerializeField]
        private SparkHairDryerAirflowVisual airflowVisual;

        [Header("Thermal Protection")]
        [SerializeField]
        private SparkHairDryerThermalProtectionVisual
            thermalProtectionVisual;

        // ============================================================
        // SWITCH VISUALS
        // ============================================================

        [Header("Control Switches")]
        [SerializeField]
        private SparkHairDryerSwitchVisual powerSwitchVisual;

        [SerializeField]
        private SparkHairDryerSwitchVisual heatSwitchVisual;

        [SerializeField]
        private SparkHairDryerSwitchVisual fanSwitchVisual;

        // ============================================================
        // INDICATORS
        // ============================================================

        [Header("Indicators")]
        [SerializeField]
        private SparkHairDryerSwitchIndicatorVisual powerIndicator;

        [SerializeField]
        private SparkHairDryerSwitchIndicatorVisual heatIndicator;

        // ============================================================
        // STATES
        // ============================================================

        [Header("Runtime State")]
        [SerializeField]
        private PowerState powerState =
            PowerState.Off;

        [SerializeField]
        private HeatMode heatMode =
            HeatMode.Cold;

        [SerializeField]
        private FanSpeed fanSpeed =
            FanSpeed.Off;

        // ============================================================
        // FAN SETTINGS
        // ============================================================

        [Header("Fan Settings")]
        [SerializeField]
        [Min(0f)]
        private float lowFanRPM = 4500f;

        [SerializeField]
        [Min(0f)]
        private float highFanRPM = 7500f;

        // ============================================================
        // AIRFLOW SETTINGS
        // ============================================================

        [Header("Airflow Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float lowAirflow = 0.45f;

        [SerializeField]
        [Range(0f, 1f)]
        private float highAirflow = 1f;

        // ============================================================
        // HEAT SETTINGS
        // ============================================================

        [Header("Heat Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float lowHeatTemperature = 0.55f;

        [SerializeField]
        [Range(0f, 1f)]
        private float highHeatTemperature = 1f;

        // ============================================================
        // AIR TEMPERATURE
        // ============================================================

        [Header("Air Temperature")]
        [SerializeField]
        [Range(0f, 1f)]
        private float coldAirTemperature = 0f;

        [SerializeField]
        [Range(0f, 1f)]
        private float lowAirTemperature = 0.45f;

        [SerializeField]
        [Range(0f, 1f)]
        private float highAirTemperature = 1f;

        // ============================================================
        // RUNTIME PROPERTIES
        // ============================================================

        public PowerState CurrentPowerState =>
            powerState;

        public HeatMode CurrentHeatMode =>
            heatMode;

        public FanSpeed CurrentFanSpeed =>
            fanSpeed;

        public bool IsPowered =>
            powerState == PowerState.On;

        public bool IsRunning =>
            IsPowered &&
            fanSpeed != FanSpeed.Off;

        public bool IsHeating =>
            IsPowered &&
            heatMode != HeatMode.Cold;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ApplyCompleteStateImmediate();
        }

        // ============================================================
        // POWER
        // ============================================================

        public void SetPower(PowerState state)
        {
            if (powerState == state)
            {
                ApplyCompleteState();
                return;
            }

            powerState = state;

            ApplyCompleteState();
        }

        public void SetPower(bool enabled)
        {
            SetPower(
                enabled
                    ? PowerState.On
                    : PowerState.Off);
        }

        public void PowerOn()
        {
            SetPower(PowerState.On);
        }

        public void PowerOff()
        {
            SetPower(PowerState.Off);
        }

        // ============================================================
        // HEAT
        // ============================================================

        public void SetHeatMode(HeatMode mode)
        {
            heatMode = mode;

            ApplyCompleteState();
        }

        public void SetCold()
        {
            SetHeatMode(HeatMode.Cold);
        }

        public void SetLowHeat()
        {
            SetHeatMode(HeatMode.Low);
        }

        public void SetHighHeat()
        {
            SetHeatMode(HeatMode.High);
        }

        // ============================================================
        // FAN
        // ============================================================

        public void SetFanSpeed(FanSpeed speed)
        {
            fanSpeed = speed;

            ApplyCompleteState();
        }

        public void SetFanOff()
        {
            SetFanSpeed(FanSpeed.Off);
        }

        public void SetFanLow()
        {
            SetFanSpeed(FanSpeed.Low);
        }

        public void SetFanHigh()
        {
            SetFanSpeed(FanSpeed.High);
        }

        // ============================================================
        // COMPLETE STATE
        // ============================================================

        private void ApplyCompleteState()
        {
            ApplyControlVisuals();
            ApplyPowerIndicator();
            ApplyMotor();
            ApplyHeater();
            ApplyAirflow();
            ApplyThermalProtectionVisual();
        }

        private void ApplyCompleteStateImmediate()
        {
            ApplyControlVisualsImmediate();
            ApplyPowerIndicatorImmediate();

            ApplyMotor();
            ApplyHeater();
            ApplyAirflow();
            ApplyThermalProtectionVisual();
        }

        // ============================================================
        // SWITCH VISUALS
        // ============================================================

        private void ApplyControlVisuals()
        {
            if (powerSwitchVisual != null)
            {
                powerSwitchVisual.SetPosition(
                    powerState == PowerState.On
                        ? 1
                        : 0);
            }

            if (heatSwitchVisual != null)
            {
                heatSwitchVisual.SetPosition(
                    GetHeatSwitchPosition());
            }

            if (fanSwitchVisual != null)
            {
                fanSwitchVisual.SetPosition(
                    GetFanSwitchPosition());
            }
        }

        private void ApplyControlVisualsImmediate()
        {
            if (powerSwitchVisual != null)
            {
                powerSwitchVisual.SetPositionImmediate(
                    powerState == PowerState.On
                        ? 1
                        : 0);
            }

            if (heatSwitchVisual != null)
            {
                heatSwitchVisual.SetPositionImmediate(
                    GetHeatSwitchPosition());
            }

            if (fanSwitchVisual != null)
            {
                fanSwitchVisual.SetPositionImmediate(
                    GetFanSwitchPosition());
            }
        }

        private int GetHeatSwitchPosition()
        {
            switch (heatMode)
            {
                case HeatMode.Cold:
                    return 0;

                case HeatMode.Low:
                    return 1;

                case HeatMode.High:
                    return 2;

                default:
                    return 0;
            }
        }

        private int GetFanSwitchPosition()
        {
            switch (fanSpeed)
            {
                case FanSpeed.Off:
                    return 0;

                case FanSpeed.Low:
                    return 1;

                case FanSpeed.High:
                    return 2;

                default:
                    return 0;
            }
        }

        // ============================================================
        // POWER INDICATOR
        // ============================================================

        private void ApplyPowerIndicator()
        {
            if (powerIndicator == null)
                return;

            powerIndicator.SetState(
                powerState == PowerState.On);
        }

        private void ApplyPowerIndicatorImmediate()
        {
            if (powerIndicator == null)
                return;

            powerIndicator.SetState(
                powerState == PowerState.On);
        }

        // ============================================================
        // MOTOR
        // ============================================================

        private void ApplyMotor()
        {
            if (motorVisual == null)
                return;

            if (powerState == PowerState.Off)
            {
                motorVisual.Stop();
                return;
            }

            switch (fanSpeed)
            {
                case FanSpeed.Off:
                    motorVisual.Stop();
                    break;

                case FanSpeed.Low:
                    motorVisual.SetRPM(lowFanRPM);
                    break;

                case FanSpeed.High:
                    motorVisual.SetRPM(highFanRPM);
                    break;
            }
        }

        // ============================================================
        // HEATER
        // ============================================================

        private void ApplyHeater()
        {
            if (heaterVisual == null)
                return;

            if (powerState == PowerState.Off)
            {
                heaterVisual.SetCold();
                return;
            }

            switch (heatMode)
            {
                case HeatMode.Cold:
                    heaterVisual.SetCold();
                    break;

                case HeatMode.Low:
                    heaterVisual.SetTemperature(
                        lowHeatTemperature);
                    break;

                case HeatMode.High:
                    heaterVisual.SetTemperature(
                        highHeatTemperature);
                    break;
            }
        }

        // ============================================================
        // AIRFLOW
        // ============================================================

        private void ApplyAirflow()
        {
            if (airflowVisual == null)
                return;

            if (powerState == PowerState.Off)
            {
                airflowVisual.StopAirflow();
                airflowVisual.SetAirTemperature(
                    coldAirTemperature);
                return;
            }

            switch (fanSpeed)
            {
                case FanSpeed.Off:

                    airflowVisual.StopAirflow();

                    airflowVisual.SetAirTemperature(
                        coldAirTemperature);

                    break;

                case FanSpeed.Low:

                    airflowVisual.SetAirflowStrength(
                        lowAirflow);

                    ApplyAirTemperature();

                    break;

                case FanSpeed.High:

                    airflowVisual.SetAirflowStrength(
                        highAirflow);

                    ApplyAirTemperature();

                    break;
            }
        }

        private void ApplyAirTemperature()
        {
            switch (heatMode)
            {
                case HeatMode.Cold:

                    airflowVisual.SetAirTemperature(
                        coldAirTemperature);

                    break;

                case HeatMode.Low:

                    airflowVisual.SetAirTemperature(
                        lowAirTemperature);

                    break;

                case HeatMode.High:

                    airflowVisual.SetAirTemperature(
                        highAirTemperature);

                    break;
            }
        }

        // ============================================================
        // THERMAL PROTECTION VISUAL
        // ============================================================

        private void ApplyThermalProtectionVisual()
        {
            if (thermalProtectionVisual == null)
                return;

            if (powerState == PowerState.Off ||
                heatMode == HeatMode.Cold)
            {
                thermalProtectionVisual.SetTemperature(
                    0f);

                return;
            }

            switch (heatMode)
            {
                case HeatMode.Low:

                    thermalProtectionVisual.SetTemperature(
                        0.45f);

                    break;

                case HeatMode.High:

                    thermalProtectionVisual.SetTemperature(
                        0.80f);

                    break;
            }
        }

        // ============================================================
        // PRESET MODES
        // ============================================================

        /// <summary>
        /// Power OFF.
        /// </summary>
        public void PresetOff()
        {
            powerState = PowerState.Off;
            heatMode = HeatMode.Cold;
            fanSpeed = FanSpeed.Off;

            ApplyCompleteState();
        }

        /// <summary>
        /// Cold air, low fan.
        /// </summary>
        public void PresetColdLow()
        {
            powerState = PowerState.On;
            heatMode = HeatMode.Cold;
            fanSpeed = FanSpeed.Low;

            ApplyCompleteState();
        }

        /// <summary>
        /// Cold air, high fan.
        /// </summary>
        public void PresetColdHigh()
        {
            powerState = PowerState.On;
            heatMode = HeatMode.Cold;
            fanSpeed = FanSpeed.High;

            ApplyCompleteState();
        }

        /// <summary>
        /// Low heat, low fan.
        /// </summary>
        public void PresetLowHeatLowFan()
        {
            powerState = PowerState.On;
            heatMode = HeatMode.Low;
            fanSpeed = FanSpeed.Low;

            ApplyCompleteState();
        }

        /// <summary>
        /// Low heat, high fan.
        /// </summary>
        public void PresetLowHeatHighFan()
        {
            powerState = PowerState.On;
            heatMode = HeatMode.Low;
            fanSpeed = FanSpeed.High;

            ApplyCompleteState();
        }

        /// <summary>
        /// High heat, high fan.
        /// </summary>
        public void PresetHighHeatHighFan()
        {
            powerState = PowerState.On;
            heatMode = HeatMode.High;
            fanSpeed = FanSpeed.High;

            ApplyCompleteState();
        }

        // ============================================================
        // TEST CONTEXT MENUS
        // ============================================================

        [ContextMenu("Test / OFF")]
        private void TestOff()
        {
            PresetOff();
        }

        [ContextMenu("Test / Cold + Low Fan")]
        private void TestColdLow()
        {
            PresetColdLow();
        }

        [ContextMenu("Test / Cold + High Fan")]
        private void TestColdHigh()
        {
            PresetColdHigh();
        }

        [ContextMenu("Test / Low Heat + Low Fan")]
        private void TestLowHeatLowFan()
        {
            PresetLowHeatLowFan();
        }

        [ContextMenu("Test / Low Heat + High Fan")]
        private void TestLowHeatHighFan()
        {
            PresetLowHeatHighFan();
        }

        [ContextMenu("Test / High Heat + High Fan")]
        private void TestHighHeatHighFan()
        {
            PresetHighHeatHighFan();
        }
    }
}