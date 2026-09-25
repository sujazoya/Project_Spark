using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Measurement
{
    /// <summary>
    /// Connects multimeter mode buttons to a SparkMultimeter.
    ///
    /// Assign the buttons in the Inspector:
    /// - Voltage
    /// - Current
    /// - Resistance
    /// - Continuity
    ///
    /// The correct mode is automatically assigned to each button.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterModeButtons : MonoBehaviour
    {
        [Header("Multimeter")]
        [SerializeField]
        private SparkMultimeter multimeter;

        [Header("Mode Buttons")]
        [SerializeField]
        private Button voltageButton;

        [SerializeField]
        private Button currentButton;

        [SerializeField]
        private Button resistanceButton;

        [SerializeField]
        private Button continuityButton;

        private void OnEnable()
        {
            RegisterButtons();
        }

        private void OnDisable()
        {
            UnregisterButtons();
        }

        private void RegisterButtons()
        {
            if (voltageButton != null)
            {
                voltageButton.onClick.AddListener(SetVoltage);
            }

            if (currentButton != null)
            {
                currentButton.onClick.AddListener(SetCurrent);
            }

            if (resistanceButton != null)
            {
                resistanceButton.onClick.AddListener(SetResistance);
            }

            if (continuityButton != null)
            {
                continuityButton.onClick.AddListener(SetContinuity);
            }
        }

        private void UnregisterButtons()
        {
            if (voltageButton != null)
            {
                voltageButton.onClick.RemoveListener(SetVoltage);
            }

            if (currentButton != null)
            {
                currentButton.onClick.RemoveListener(SetCurrent);
            }

            if (resistanceButton != null)
            {
                resistanceButton.onClick.RemoveListener(SetResistance);
            }

            if (continuityButton != null)
            {
                continuityButton.onClick.RemoveListener(SetContinuity);
            }
        }

        public void SetVoltage()
        {
            SetMode(SparkMultimeterMode.VoltageDC);
        }

        public void SetCurrent()
        {
            SetMode(SparkMultimeterMode.CurrentDC);
        }

        public void SetResistance()
        {
            SetMode(SparkMultimeterMode.Resistance);
        }

        public void SetContinuity()
        {
            SetMode(SparkMultimeterMode.Continuity);
        }

        private void SetMode(SparkMultimeterMode mode)
        {
            if (multimeter == null)
            {
                Debug.LogWarning(
                    $"{nameof(SparkMultimeterModeButtons)}: " +
                    "SparkMultimeter is not assigned.",
                    this);

                return;
            }

            if (!multimeter.SetMode(mode, out string reason))
            {
                if (!string.IsNullOrEmpty(reason))
                {
                    Debug.LogWarning(
                        reason,
                        multimeter);
                }

                return;
            }

            if (!string.IsNullOrEmpty(reason))
            {
                Debug.LogWarning(
                    reason,
                    multimeter);
            }
        }
    }
}