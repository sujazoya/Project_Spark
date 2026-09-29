using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual temperature controller for the Project Spark Level 4
    /// hair-dryer heating element.
    ///
    /// Responsibilities:
    /// - Smooth heating and cooling.
    /// - Drives the Shader Graph "_Temperature" property.
    /// - Supports cold, low-heat and high-heat modes.
    /// - Provides runtime temperature information.
    /// - Allows future electrical/thermal systems to provide temperature.
    ///
    /// This component does NOT perform electrical simulation.
    ///
    /// Future flow:
    ///
    /// SparkElectricalSolver
    ///        ↓
    /// Hair Dryer Thermal Simulation
    ///        ↓
    /// SetTemperature()
    ///        ↓
    /// SparkHairDryerHeaterVisual
    ///        ↓
    /// Shader Graph
    ///        ↓
    /// Heater appearance
    ///
    /// Shader Graph requirement:
    ///     Property Name: Temperature
    ///     Reference: _Temperature
    ///     Type: Float
    ///     Range: 0 - 1
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerHeaterVisual : MonoBehaviour
    {
        // ============================================================
        // SHADER PROPERTY
        // ============================================================

        private static readonly int TemperatureID =
            Shader.PropertyToID("_Temperature");

        // ============================================================
        // HEATER
        // ============================================================

        [Header("Heater")]

        [Tooltip(
            "Renderer containing the hair-dryer heating element material.")]
        [SerializeField]
        private Renderer []heaterRenderer;

        // ============================================================
        // TEMPERATURE
        // ============================================================

        [Header("Temperature")]

        [Tooltip(
            "Current visual heater temperature.\n" +
            "0 = cold.\n" +
            "1 = maximum temperature.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float temperature;

        [Tooltip(
            "How quickly the heater visually heats.")]
        [SerializeField]
        [Min(0f)]
        private float heatingSpeed = 0.35f;

        [Tooltip(
            "How quickly the heater visually cools.")]
        [SerializeField]
        [Min(0f)]
        private float coolingSpeed = 0.15f;

        [Tooltip(
            "Maximum allowed temperature.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float maximumTemperature = 1f;

        // ============================================================
        // HEAT PRESETS
        // ============================================================

        [Header("Heat Presets")]

        [Tooltip(
            "Target temperature for low heat.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float lowHeatTemperature = 0.55f;

        [Tooltip(
            "Target temperature for high heat.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float highHeatTemperature = 1f;

        // ============================================================
        // RUNTIME
        // ============================================================

        [Header("Runtime")]

        [Tooltip(
            "Temperature the heater is currently moving toward.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float targetTemperature;

        [Tooltip(
            "Current heater state.")]
        [SerializeField]
        private HeaterVisualState visualState =
            HeaterVisualState.Cold;

        // ============================================================
        // MATERIAL
        // ============================================================

        [Header("Material")]

        [Tooltip(
            "Creates a unique material instance so this heater does not " +
            "change the material of other objects using the same material.")]
        [SerializeField]
        private bool createMaterialInstance = true;

        private Material runtimeMaterial;

        private bool materialInstanceCreated;

        private float lastShaderTemperature = -1f;

        // ============================================================
        // STATE
        // ============================================================

        public enum HeaterVisualState
        {
            Cold,
            Heating,
            Warm,
            Hot,
            Cooling
        }

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        /// <summary>
        /// Current visual temperature from 0 to 1.
        /// </summary>
        public float Temperature =>
            temperature;

        /// <summary>
        /// Current requested target temperature.
        /// </summary>
        public float TargetTemperature =>
            targetTemperature;

        /// <summary>
        /// Current visual heater state.
        /// </summary>
        public HeaterVisualState VisualState =>
            visualState;

        /// <summary>
        /// True when the heater has reached a visibly warm state.
        /// </summary>
        public bool IsHot =>
            temperature > 0.05f;

        /// <summary>
        /// True while the heater temperature is increasing.
        /// </summary>
        public bool IsHeating =>
            targetTemperature > temperature + 0.001f;

        /// <summary>
        /// True while the heater temperature is decreasing.
        /// </summary>
        public bool IsCooling =>
            targetTemperature < temperature - 0.001f;

        /// <summary>
        /// True when current temperature has reached the target.
        /// </summary>
        public bool IsAtTargetTemperature =>
            Mathf.Abs(
                targetTemperature - temperature) <= 0.001f;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            InitializeMaterial();

            UpdateVisualState();

            ApplyTemperatureToShader(true);
        }

        private void Update()
        {
            UpdateTemperature();

            UpdateVisualState();

            ApplyTemperatureToShader(false);
        }

        private void OnDestroy()
        {
            ReleaseRuntimeMaterial();
        }

        // ============================================================
        // MATERIAL INITIALIZATION
        // ============================================================

       private void InitializeMaterial()
        {
            if (heaterRenderer == null || heaterRenderer.Length == 0)
                return;

            Renderer sourceRenderer = null;

            for (int i = 0; i < heaterRenderer.Length; i++)
            {
                if (heaterRenderer[i] != null)
                {
                    sourceRenderer = heaterRenderer[i];
                    break;
                }
            }

            if (sourceRenderer == null)
                return;

            if (createMaterialInstance)
            {
                runtimeMaterial = sourceRenderer.material;
                materialInstanceCreated = runtimeMaterial != null;
            }
            else
            {
                runtimeMaterial = sourceRenderer.sharedMaterial;
                materialInstanceCreated = false;
            }

            if (runtimeMaterial == null)
                return;

            for (int i = 0; i < heaterRenderer.Length; i++)
            {
                if (heaterRenderer[i] == null)
                    continue;

                heaterRenderer[i].material = runtimeMaterial;
            }
        }

        // ============================================================
        // MATERIAL CLEANUP
        // ============================================================

        private void ReleaseRuntimeMaterial()
        {
            if (!materialInstanceCreated)
                return;

            if (runtimeMaterial == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(runtimeMaterial);
            }
            else
            {
                DestroyImmediate(runtimeMaterial);
            }

            runtimeMaterial = null;

            materialInstanceCreated = false;
        }

        // ============================================================
        // TEMPERATURE
        // ============================================================

        private void UpdateTemperature()
        {
            float clampedTarget =
                Mathf.Clamp(
                    targetTemperature,
                    0f,
                    maximumTemperature);

            float speed;

            if (clampedTarget > temperature)
            {
                speed = heatingSpeed;
            }
            else
            {
                speed = coolingSpeed;
            }

            temperature =
                Mathf.MoveTowards(
                    temperature,
                    clampedTarget,
                    speed * Time.deltaTime);

            temperature =
                Mathf.Clamp(
                    temperature,
                    0f,
                    maximumTemperature);
        }

        // ============================================================
        // VISUAL STATE
        // ============================================================

        private void UpdateVisualState()
        {
            if (temperature <= 0.001f)
            {
                if (targetTemperature <= 0.001f)
                {
                    visualState =
                        HeaterVisualState.Cold;
                }
                else
                {
                    visualState =
                        HeaterVisualState.Heating;
                }

                return;
            }

            if (IsHeating)
            {
                visualState =
                    HeaterVisualState.Heating;

                return;
            }

            if (IsCooling)
            {
                visualState =
                    HeaterVisualState.Cooling;

                return;
            }

            if (temperature >= 0.75f)
            {
                visualState =
                    HeaterVisualState.Hot;

                return;
            }

            visualState =
                HeaterVisualState.Warm;
        }

        // ============================================================
        // SHADER
        // ============================================================

        private void ApplyTemperatureToShader(
            bool force)
        {
            if (runtimeMaterial == null)
                return;

            if (!runtimeMaterial.HasProperty(
                    TemperatureID))
            {
                return;
            }

            if (!force &&
                Mathf.Approximately(
                    temperature,
                    lastShaderTemperature))
            {
                return;
            }

            runtimeMaterial.SetFloat(
                TemperatureID,
                temperature);

            lastShaderTemperature =
                temperature;
        }

        // ============================================================
        // CONTROL
        // ============================================================

        /// <summary>
        /// Requests complete heater cooldown.
        /// </summary>
        public void SetCold()
        {
            SetTemperature(0f);
        }

        /// <summary>
        /// Requests low heater temperature.
        /// </summary>
        public void SetLowHeat()
        {
            SetTemperature(
                lowHeatTemperature);
        }

        /// <summary>
        /// Requests maximum heater temperature.
        /// </summary>
        public void SetHighHeat()
        {
            SetTemperature(
                highHeatTemperature);
        }

        /// <summary>
        /// Requests a specific temperature.
        ///
        /// The visual temperature moves toward this value smoothly.
        /// </summary>
        public void SetTemperature(
            float value)
        {
            targetTemperature =
                Mathf.Clamp(
                    value,
                    0f,
                    maximumTemperature);
        }

        /// <summary>
        /// Immediately changes both current and target temperature.
        ///
        /// Useful when synchronizing with another system.
        /// </summary>
        public void SetTemperatureImmediate(
            float value)
        {
            float clamped =
                Mathf.Clamp(
                    value,
                    0f,
                    maximumTemperature);

            temperature =
                clamped;

            targetTemperature =
                clamped;

            UpdateVisualState();

            ApplyTemperatureToShader(true);
        }

        // ============================================================
        // SPEED CONTROL
        // ============================================================

        /// <summary>
        /// Changes heating speed.
        /// </summary>
        public void SetHeatingSpeed(
            float speed)
        {
            heatingSpeed =
                Mathf.Max(
                    0f,
                    speed);
        }

        /// <summary>
        /// Changes cooling speed.
        /// </summary>
        public void SetCoolingSpeed(
            float speed)
        {
            coolingSpeed =
                Mathf.Max(
                    0f,
                    speed);
        }

        // ============================================================
        // LIMIT CONTROL
        // ============================================================

        /// <summary>
        /// Changes maximum allowed temperature.
        /// </summary>
        public void SetMaximumTemperature(
            float value)
        {
            maximumTemperature =
                Mathf.Clamp01(value);

            targetTemperature =
                Mathf.Min(
                    targetTemperature,
                    maximumTemperature);

            temperature =
                Mathf.Min(
                    temperature,
                    maximumTemperature);
        }

        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("Test / Cold")]
        private void TestCold()
        {
            SetCold();
        }

        [ContextMenu("Test / Low Heat")]
        private void TestLowHeat()
        {
            SetLowHeat();
        }

        [ContextMenu("Test / High Heat")]
        private void TestHighHeat()
        {
            SetHighHeat();
        }

        [ContextMenu("Test / Temperature 25%")]
        private void Test25Percent()
        {
            SetTemperature(0.25f);
        }

        [ContextMenu("Test / Temperature 50%")]
        private void Test50Percent()
        {
            SetTemperature(0.50f);
        }

        [ContextMenu("Test / Temperature 75%")]
        private void Test75Percent()
        {
            SetTemperature(0.75f);
        }

        [ContextMenu("Test / Temperature 100%")]
        private void Test100Percent()
        {
            SetTemperature(1f);
        }

        [ContextMenu("Test / Immediate Cold")]
        private void TestImmediateCold()
        {
            SetTemperatureImmediate(0f);
        }

        [ContextMenu("Test / Immediate Hot")]
        private void TestImmediateHot()
        {
            SetTemperatureImmediate(1f);
        }
    }
}