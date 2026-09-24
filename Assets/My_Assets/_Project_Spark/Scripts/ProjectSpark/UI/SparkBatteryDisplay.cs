
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Displays the runtime state of a SparkBattery.
    ///
    /// This component contains presentation logic only.
    /// It does not simulate charging.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBatteryDisplay : MonoBehaviour
    {
        [Header("Battery")]
        [SerializeField]
        private SparkBatteryCharging battery;

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
        private TMP_Text powerText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text timeText;

        [Header("Colors")]
        [SerializeField]
        private Color normalColor =
            new Color(0.05f, 0.85f, 1f, 1f);

        [SerializeField]
        private Color liveColor =
            new Color(0.1f, 0.9f, 1f, 1f);

        [SerializeField]
        private Color fullColor =
            new Color(0.15f, 1f, 0.55f, 1f);

        [SerializeField]
        private Color faultColor =
            new Color(1f, 0.15f, 0.08f, 1f);

        [Header("Animation")]
        [SerializeField]
        [Min(0f)]
        private float chargeSpeed = 1.5f;

        [SerializeField]
        [Min(0f)]
        private float pulseIntensity = 2f;

        [SerializeField]
        [Min(0f)]
        private float glowIntensity = 1.5f;

        [SerializeField]
        [Min(0f)]
        private float fullPulseSpeed = 2f;

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
            if (battery == null)
            {
                battery =
                    GetComponentInParent<SparkBatteryCharging>();
            }

            InitializeMaterial();
        }

        private void OnEnable()
        {
            Subscribe();

            RefreshDisplay();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }

        private void Update()
        {
            RefreshShaderAnimation();
        }

        private void Subscribe()
        {
            if (battery == null)
            {
                return;
            }

            battery.StateChanged +=
                HandleBatteryChanged;

            battery.BatteryChanged +=
                HandleBatteryChanged;
        }

        private void Unsubscribe()
        {
            if (battery == null)
            {
                return;
            }

            battery.StateChanged -=
                HandleBatteryChanged;

            battery.BatteryChanged -=
                HandleBatteryChanged;
        }

        private void HandleBatteryChanged(
            SparkBatteryCharging source)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Creates an independent runtime material.
        /// </summary>
        private void InitializeMaterial()
        {
            if (displayImage == null)
            {
                return;
            }

            Material sourceMaterial =
                displayImage.material;

            if (sourceMaterial == null)
            {
                return;
            }

            runtimeMaterial =
                new Material(sourceMaterial)
                {
                    name =
                        sourceMaterial.name +
                        " (Runtime)"
                };

            displayImage.material =
                runtimeMaterial;
        }

        /// <summary>
        /// Refreshes all static display information.
        /// </summary>
        private void RefreshDisplay()
        {
            if (battery == null)
            {
                return;
            }

            RefreshShader();
            RefreshText();
        }

        /// <summary>
        /// Updates Shader Graph properties.
        /// </summary>
        private void RefreshShader()
        {
            if (runtimeMaterial == null ||
                battery == null)
            {
                return;
            }

            runtimeMaterial.SetFloat(
                ChargePercentId,
                battery.ChargePercent);

            runtimeMaterial.SetFloat(
                ChargeSpeedId,
                chargeSpeed);

            runtimeMaterial.SetFloat(
                PulseIntensityId,
                pulseIntensity);

            runtimeMaterial.SetFloat(
                GlowIntensityId,
                glowIntensity);

            float charging =
                battery.IsCharging
                    ? 1f
                    : 0f;

            runtimeMaterial.SetFloat(
                ChargingId,
                charging);

            Color activeColor =
                normalColor;

            switch (battery.State)
            {
                case SparkBatteryCharging.BatteryState.Live:
                case SparkBatteryCharging.BatteryState.Charging:
                    activeColor =
                        liveColor;
                    break;

                case SparkBatteryCharging.BatteryState.Full:
                    activeColor =
                        fullColor;
                    break;

                case SparkBatteryCharging.BatteryState.Fault:
                    activeColor =
                        faultColor;
                    break;
            }

            runtimeMaterial.SetColor(
                ChargeColorId,
                activeColor);

            runtimeMaterial.SetColor(
                FullColorId,
                fullColor);
        }

        /// <summary>
        /// Drives visual pulse and glow animation.
        /// </summary>
        private void RefreshShaderAnimation()
        {
            if (runtimeMaterial == null ||
                battery == null)
            {
                return;
            }

            float pulse =
                pulseIntensity;

            float glow =
                glowIntensity;

            if (battery.State ==
                SparkBatteryCharging.BatteryState.Full)
            {
                float value =
                    (Mathf.Sin(
                        Time.time *
                        fullPulseSpeed) +
                     1f) *
                    0.5f;

                pulse *=
                    Mathf.Lerp(
                        0.75f,
                        1.5f,
                        value);

                glow *=
                    Mathf.Lerp(
                        0.8f,
                        1.8f,
                        value);
            }

            if (battery.State ==
                SparkBatteryCharging.BatteryState.Fault)
            {
                float value =
                    (Mathf.Sin(
                        Time.time * 8f) +
                     1f) *
                    0.5f;

                pulse *=
                    Mathf.Lerp(
                        0.4f,
                        1.8f,
                        value);

                glow *=
                    Mathf.Lerp(
                        0.4f,
                        2f,
                        value);
            }

            runtimeMaterial.SetFloat(
                PulseIntensityId,
                pulse);

            runtimeMaterial.SetFloat(
                GlowIntensityId,
                glow);
        }

        /// <summary>
        /// Updates text fields.
        /// </summary>
        private void RefreshText()
        {
            if (battery == null)
            {
                return;
            }

            if (percentageText != null)
            {
                percentageText.text =
                    Mathf.RoundToInt(
                        battery.ChargePercentage) +
                    "%";
            }

            if (voltageText != null)
            {
                voltageText.text =
                    battery.Voltage.ToString("0.00") +
                    " V";
            }

            if (currentText != null)
            {
                currentText.text =
                    battery.Current.ToString("0.00") +
                    " A";
            }

            if (powerText != null)
            {
                powerText.text =
                    battery.Power.ToString("0.00") +
                    " W";
            }

            if (statusText != null)
            {
                statusText.text =
                    GetStatusText();
            }

            if (timeText != null)
            {
                timeText.text =
                    FormatRemainingTime();
            }
        }

        private string GetStatusText()
        {
            switch (battery.State)
            {
                case SparkBatteryCharging.BatteryState.Empty:
                    return "EMPTY";

                case SparkBatteryCharging.BatteryState.Idle:
                    return "STANDBY";

                case SparkBatteryCharging.BatteryState.Live:
                    return "LIVE";

                case SparkBatteryCharging.BatteryState.Charging:
                    return "CHARGING";

                case SparkBatteryCharging.BatteryState.Full:
                    return "FULL";

                case SparkBatteryCharging.BatteryState.Discharging:
                    return "DISCHARGING";

                case SparkBatteryCharging.BatteryState.Fault:
                    return "FAULT";

                default:
                    return "UNKNOWN";
            }
        }

        private string FormatRemainingTime()
        {
            if (battery.IsFull)
            {
                return "CHARGED";
            }

            if (!battery.IsCharging)
            {
                return "--:--";
            }

            int seconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        battery.RemainingChargeTime));

            int minutes =
                seconds / 60;

            int remainingSeconds =
                seconds % 60;

            return minutes.ToString("00") +
                   ":" +
                   remainingSeconds.ToString("00");
        }
    }
}
/*
```

### Final architecture

Now the responsibilities are clean:

| Component               | Owns                             |
| ----------------------- | -------------------------------- |
| `SparkPowerSupply`      | Produces electrical power        |
| `SparkCircuitSystem`    | Connections/topology             |
| `SparkElectricalSolver` | Voltage/current solution         |
| `SparkBattery`          | Actual battery/charge simulation |
| `SparkBatteryDisplay`   | UI + Shader Graph                |
| `LevelGamePlayManager`  | Level objectives/evaluation      |

The important flow becomes:

```text
Power_Bank
    ↓
power_jack
    ↓
plug
    ↓
socket
    ↓
Mobile SparkBattery
    ↓
ApplyElectricalState()
    ↓
SparkBattery
    │
    ├── LIVE
    ├── CHARGING
    ├── charge %
    ├── elapsed time
    ├── remaining time
    └── FULL
           ↓
SparkBatteryDisplay
           ↓
      UI / Shader
```

**One thing I would change next:** don't make `SparkBattery` discover the electrical solver itself. Let your existing solver/device integration push the solved voltage/current into `ApplyElectricalState()`. That keeps the circuit layer independent and avoids circular dependencies.

Also, once this is connected to your actual `Mobile`, the battery's `chargePercent` should become the **real gameplay value**, not a simulated UI value.
*/