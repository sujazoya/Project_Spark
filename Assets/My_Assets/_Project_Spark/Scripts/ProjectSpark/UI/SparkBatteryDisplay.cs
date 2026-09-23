using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Controls the visual state of a Project Spark battery charging display.
    ///
    /// The Shader Graph handles:
    /// - Battery fill
    /// - Charging pulse
    /// - Border glow
    /// - Battery terminal
    ///
    /// This component handles:
    /// - Charge percentage
    /// - Voltage
    /// - Current
    /// - Charging state
    /// - Full state
    /// - Fault state
    /// - Display text
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBatteryDisplay : MonoBehaviour
    {
        public enum BatteryState
        {
            Empty,
            Discharging,
            Charging,
            Full,
            Fault
        }

        [Header("Display")]
        [SerializeField]
        private Image displayImage;

        [SerializeField]
        private TMP_Text percentageText;

        [SerializeField]
        private TMP_Text voltageText;

        [SerializeField]
        private TMP_Text currentText;

        [SerializeField]
        private TMP_Text statusText;

        [Header("Battery")]
        [SerializeField]
        [Range(0f, 1f)]
        private float chargePercent = 0.68f;

        [SerializeField]
        private float voltage = 12.48f;

        [SerializeField]
        private float current = 0.82f;

        [SerializeField]
        private BatteryState state = BatteryState.Charging;

        [Header("Shader Colors")]
        [SerializeField]
        private Color normalColor =
            new Color(0.05f, 0.85f, 1f, 1f);

        [SerializeField]
        private Color fullColor =
            new Color(0.15f, 1f, 0.55f, 1f);

        [SerializeField]
        private Color faultColor =
            new Color(1f, 0.15f, 0.08f, 1f);

        [Header("Shader Animation")]
        [SerializeField]
        [Min(0f)]
        private float chargeSpeed = 1.5f;

        [SerializeField]
        [Min(0f)]
        private float pulseIntensity = 2f;

        [SerializeField]
        [Min(0f)]
        private float glowIntensity = 1.5f;

        private Material runtimeMaterial;

        private static readonly int ChargePercentId =
            Shader.PropertyToID("_ChargePercent");

        private static readonly int ChargeColorId =
            Shader.PropertyToID("_ChargeColor");

        private static readonly int FullColorId =
            Shader.PropertyToID("_FullColor");

        private static readonly int ChargingId =
            Shader.PropertyToID("_Charging");

        private static readonly int ChargeSpeedId =
            Shader.PropertyToID("_ChargeSpeed");

        private static readonly int PulseIntensityId =
            Shader.PropertyToID("_PulseIntensity");

        private static readonly int GlowIntensityId =
            Shader.PropertyToID("_GlowIntensity");

        private void Awake()
        {
            InitializeMaterial();
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }

        /// <summary>
        /// Creates an instance of the display material so changing the battery
        /// does not modify the shared Shader Graph material asset.
        /// </summary>
        private void InitializeMaterial()
        {
            if (displayImage == null)
            {
                return;
            }

            Material sourceMaterial = displayImage.material;

            if (sourceMaterial == null)
            {
                return;
            }

            runtimeMaterial = new Material(sourceMaterial)
            {
                name = sourceMaterial.name + " (Runtime)"
            };

            displayImage.material = runtimeMaterial;
        }

        /// <summary>
        /// Updates the entire display.
        /// </summary>
        private void RefreshDisplay()
        {
            RefreshShader();
            RefreshText();
        }

        /// <summary>
        /// Updates Shader Graph properties.
        /// </summary>
        private void RefreshShader()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetFloat(
                ChargePercentId,
                chargePercent);

            runtimeMaterial.SetFloat(
                ChargeSpeedId,
                chargeSpeed);

            runtimeMaterial.SetFloat(
                PulseIntensityId,
                pulseIntensity);

            runtimeMaterial.SetFloat(
                GlowIntensityId,
                glowIntensity);

            float chargingAmount =
                state == BatteryState.Charging ? 1f : 0f;

            runtimeMaterial.SetFloat(
                ChargingId,
                chargingAmount);

            Color activeColor = normalColor;

            if (state == BatteryState.Full)
            {
                activeColor = fullColor;
            }
            else if (state == BatteryState.Fault)
            {
                activeColor = faultColor;
            }

            runtimeMaterial.SetColor(
                ChargeColorId,
                activeColor);

            runtimeMaterial.SetColor(
                FullColorId,
                fullColor);
        }

        /// <summary>
        /// Updates the TextMeshPro values.
        /// </summary>
        private void RefreshText()
        {
            if (percentageText != null)
            {
                percentageText.text =
                    Mathf.RoundToInt(chargePercent * 100f) + "%";
            }

            if (voltageText != null)
            {
                voltageText.text =
                    voltage.ToString("0.00") + " V";
            }

            if (currentText != null)
            {
                currentText.text =
                    current.ToString("0.00") + " A";
            }

            if (statusText != null)
            {
                statusText.text = GetStatusText();
            }
        }

        private string GetStatusText()
        {
            switch (state)
            {
                case BatteryState.Empty:
                    return "EMPTY";

                case BatteryState.Discharging:
                    return "DISCHARGING";

                case BatteryState.Charging:
                    return "CHARGING";

                case BatteryState.Full:
                    return "FULL";

                case BatteryState.Fault:
                    return "FAULT";

                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Sets the battery charge level.
        /// </summary>
        public void SetChargePercent(float value)
        {
            chargePercent = Mathf.Clamp01(value);
            RefreshDisplay();
        }

        /// <summary>
        /// Sets the battery voltage.
        /// </summary>
        public void SetVoltage(float value)
        {
            voltage = Mathf.Max(0f, value);
            RefreshText();
        }

        /// <summary>
        /// Sets the battery current.
        /// </summary>
        public void SetCurrent(float value)
        {
            current = Mathf.Max(0f, value);
            RefreshText();
        }

        /// <summary>
        /// Sets the battery operating state.
        /// </summary>
        public void SetState(BatteryState newState)
        {
            state = newState;
            RefreshDisplay();
        }

        /// <summary>
        /// Starts charging.
        /// </summary>
        public void StartCharging()
        {
            state = BatteryState.Charging;
            RefreshDisplay();
        }

        /// <summary>
        /// Stops charging and puts the battery into discharging state.
        /// </summary>
        public void StopCharging()
        {
            state = BatteryState.Discharging;
            RefreshDisplay();
        }

        /// <summary>
        /// Marks the battery as full.
        /// </summary>
        public void SetFull()
        {
            chargePercent = 1f;
            state = BatteryState.Full;
            RefreshDisplay();
        }

        /// <summary>
        /// Marks the battery as being in a fault condition.
        /// </summary>
        public void SetFault()
        {
            state = BatteryState.Fault;
            RefreshDisplay();
        }
    }
}