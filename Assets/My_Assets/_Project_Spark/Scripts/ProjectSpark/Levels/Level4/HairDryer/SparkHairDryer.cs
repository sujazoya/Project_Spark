using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Main visual/behavior controller for the Level 4 Hair Dryer.
    ///
    /// This first version intentionally does NOT perform electrical solving.
    /// Electrical state will later come from SparkCircuitSystem /
    /// SparkElectricalSolver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryer : MonoBehaviour
    {
        // ============================================================
        // POWER / CONTROL STATE
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
        // INSPECTOR
        // ============================================================

        [Header("Controls")]
        [SerializeField]
        private PowerState powerState = PowerState.Off;

        [SerializeField]
        private HeatMode heatMode = HeatMode.Cold;

        [SerializeField]
        private FanSpeed fanSpeed = FanSpeed.Off;

        [Header("Fan")]
        [SerializeField]
        private Transform fanRotor;

        [SerializeField]
        private Vector3 fanRotationAxis = Vector3.forward;

        [SerializeField]
        private float lowFanRPM = 3500f;

        [SerializeField]
        private float highFanRPM = 6500f;

        [Header("Heater")]
        [SerializeField]
        private Renderer heaterRenderer;

        [SerializeField]
        private string heaterGlowProperty = "_GlowIntensity";

        [SerializeField]
        private float heaterGlowSpeed = 2f;

        [Header("Runtime")]
        [SerializeField]
        private float currentFanRPM;

        [SerializeField]
        [Range(0f, 1f)]
        private float heaterTemperature;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public bool IsPowered =>
            powerState == PowerState.On;

        public HeatMode CurrentHeatMode =>
            heatMode;

        public FanSpeed CurrentFanSpeed =>
            fanSpeed;

        public float CurrentFanRPM =>
            currentFanRPM;

        public float HeaterTemperature =>
            heaterTemperature;

        public bool IsHeating =>
            IsPowered && heatMode != HeatMode.Cold;

        // ============================================================
        // UNITY
        // ============================================================

        private void Update()
        {
            UpdateFan();
            UpdateHeater();
        }

        // ============================================================
        // FAN
        // ============================================================

        private void UpdateFan()
        {
            float targetRPM = 0f;

            if (IsPowered)
            {
                switch (fanSpeed)
                {
                    case FanSpeed.Low:
                        targetRPM = lowFanRPM;
                        break;

                    case FanSpeed.High:
                        targetRPM = highFanRPM;
                        break;
                }
            }

            currentFanRPM = Mathf.MoveTowards(
                currentFanRPM,
                targetRPM,
                10000f * Time.deltaTime);

            if (fanRotor == null)
                return;

            float degreesPerSecond =
                currentFanRPM * 6f;

            fanRotor.Rotate(
                fanRotationAxis.normalized,
                degreesPerSecond * Time.deltaTime,
                Space.Self);
        }

        // ============================================================
        // HEATER
        // ============================================================

        private void UpdateHeater()
        {
            float targetTemperature = 0f;

            if (IsPowered)
            {
                switch (heatMode)
                {
                    case HeatMode.Low:
                        targetTemperature = 0.55f;
                        break;

                    case HeatMode.High:
                        targetTemperature = 1f;
                        break;
                }
            }

            float speed =
                IsHeating
                    ? heaterGlowSpeed
                    : heaterGlowSpeed * 0.5f;

            heaterTemperature = Mathf.MoveTowards(
                heaterTemperature,
                targetTemperature,
                speed * Time.deltaTime);

            UpdateHeaterMaterial();
        }

        // ============================================================
        // HEATER MATERIAL
        // ============================================================

        private void UpdateHeaterMaterial()
        {
            if (heaterRenderer == null)
                return;

            Material material =
                heaterRenderer.material;

            if (!material.HasProperty(heaterGlowProperty))
                return;

            material.SetFloat(
                heaterGlowProperty,
                heaterTemperature);
        }

        // ============================================================
        // CONTROL API
        // ============================================================

        public void SetPower(bool enabled)
        {
            powerState =
                enabled
                    ? PowerState.On
                    : PowerState.Off;

            if (!enabled)
            {
                fanSpeed = FanSpeed.Off;
            }
        }

        public void SetHeatMode(HeatMode mode)
        {
            heatMode = mode;
        }

        public void SetFanSpeed(FanSpeed speed)
        {
            fanSpeed = speed;
        }

        // ============================================================
        // DEBUG / TEST
        // ============================================================

        [ContextMenu("Test Power ON")]
        private void TestPowerOn()
        {
            SetPower(true);
            SetFanSpeed(FanSpeed.High);
            SetHeatMode(HeatMode.High);
        }

        [ContextMenu("Test Power OFF")]
        private void TestPowerOff()
        {
            SetPower(false);
        }

        [ContextMenu("Test Cold / High Air")]
        private void TestCold()
        {
            SetPower(true);
            SetFanSpeed(FanSpeed.High);
            SetHeatMode(HeatMode.Cold);
        }

        [ContextMenu("Test Low Heat")]
        private void TestLowHeat()
        {
            SetPower(true);
            SetFanSpeed(FanSpeed.Low);
            SetHeatMode(HeatMode.Low);
        }

        [ContextMenu("Test High Heat")]
        private void TestHighHeat()
        {
            SetPower(true);
            SetFanSpeed(FanSpeed.High);
            SetHeatMode(HeatMode.High);
        }
    }
}