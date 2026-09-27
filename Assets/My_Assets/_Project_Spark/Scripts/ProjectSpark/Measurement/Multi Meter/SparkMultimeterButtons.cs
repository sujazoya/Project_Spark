using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Measurement
{
    /// <summary>
    /// Connects multimeter mode and range buttons
    /// to an existing SparkMultimeter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterButtons : MonoBehaviour
    {
        [Header("Multimeter")]

        [SerializeField]
        private SparkMultimeter multimeter;


        // ============================================================
        // MODE BUTTONS
        // ============================================================

        [Header("Mode Buttons")]

        [SerializeField]
        private Button voltageButton;

        [SerializeField]
        private Button currentButton;

        [SerializeField]
        private Button resistanceButton;

        [SerializeField]
        private Button continuityButton;


        // ============================================================
        // RANGE BUTTONS
        // ============================================================

        [Header("Range Buttons")]

        [SerializeField]
        private Button autoButton;

        [SerializeField]
        private Button millivoltsButton;

        [SerializeField]
        private Button voltsButton;

        [SerializeField]
        private Button milliampsButton;

        [SerializeField]
        private Button ampsButton;

        [SerializeField]
        private Button ohmsButton;

        [SerializeField]
        private Button kiloohmsButton;

        [SerializeField]
        private Button megaohmsButton;


        // ============================================================
        // SELECTION COLORS
        // ============================================================

        [Header("Selected Button Visual")]

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color normalColor = Color.gray;

        [SerializeField, Min(1f)]
        private float selectedColorMultiplier = 1.15f;


        // ============================================================
        // UNITY
        // ============================================================

        private void OnEnable()
        {
            RegisterButtons();

            if (multimeter != null)
            {
                multimeter.ModeChanged +=
                    HandleModeChanged;

                multimeter.RangeChanged +=
                    HandleRangeChanged;

                UpdateModeSelection(
                    multimeter.Mode);

                UpdateRangeSelection(
                    multimeter.Range);
            }
        }


        private void OnDisable()
        {
            UnregisterButtons();

            if (multimeter != null)
            {
                multimeter.ModeChanged -=
                    HandleModeChanged;

                multimeter.RangeChanged -=
                    HandleRangeChanged;
            }
        }


        // ============================================================
        // REGISTER
        // ============================================================

        private void RegisterButtons()
        {
            // MODE

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


            // RANGE

            if (autoButton != null)
            {
                autoButton.onClick.AddListener(SetAuto);
            }

            if (millivoltsButton != null)
            {
                millivoltsButton.onClick.AddListener(SetMillivolts);
            }

            if (voltsButton != null)
            {
                voltsButton.onClick.AddListener(SetVolts);
            }

            if (milliampsButton != null)
            {
                milliampsButton.onClick.AddListener(SetMilliamps);
            }

            if (ampsButton != null)
            {
                ampsButton.onClick.AddListener(SetAmps);
            }

            if (ohmsButton != null)
            {
                ohmsButton.onClick.AddListener(SetOhms);
            }

            if (kiloohmsButton != null)
            {
                kiloohmsButton.onClick.AddListener(SetKiloohms);
            }

            if (megaohmsButton != null)
            {
                megaohmsButton.onClick.AddListener(SetMegaohms);
            }
        }


        // ============================================================
        // UNREGISTER
        // ============================================================

        private void UnregisterButtons()
        {
            // MODE

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


            // RANGE

            if (autoButton != null)
            {
                autoButton.onClick.RemoveListener(SetAuto);
            }

            if (millivoltsButton != null)
            {
                millivoltsButton.onClick.RemoveListener(SetMillivolts);
            }

            if (voltsButton != null)
            {
                voltsButton.onClick.RemoveListener(SetVolts);
            }

            if (milliampsButton != null)
            {
                milliampsButton.onClick.RemoveListener(SetMilliamps);
            }

            if (ampsButton != null)
            {
                ampsButton.onClick.RemoveListener(SetAmps);
            }

            if (ohmsButton != null)
            {
                ohmsButton.onClick.RemoveListener(SetOhms);
            }

            if (kiloohmsButton != null)
            {
                kiloohmsButton.onClick.RemoveListener(SetKiloohms);
            }

            if (megaohmsButton != null)
            {
                megaohmsButton.onClick.RemoveListener(SetMegaohms);
            }
        }


        // ============================================================
        // MODE
        // ============================================================

        public void SetVoltage()
        {
            SetMode(
                SparkMultimeterMode.VoltageDC);
        }


        public void SetCurrent()
        {
            SetMode(
                SparkMultimeterMode.CurrentDC);
        }


        public void SetResistance()
        {
            SetMode(
                SparkMultimeterMode.Resistance);
        }


        public void SetContinuity()
        {
            SetMode(
                SparkMultimeterMode.Continuity);
        }


        private void SetMode(
            SparkMultimeterMode mode)
        {
            if (multimeter == null)
            {
                Debug.LogWarning(
                    $"{nameof(SparkMultimeterButtons)}: " +
                    "SparkMultimeter is not assigned.",
                    this);

                return;
            }

            if (!multimeter.SetMode(
                    mode,
                    out string reason))
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


        // ============================================================
        // RANGE
        // ============================================================

        public void SetAuto()
        {
            SetRange(
                SparkMultimeterRange.Auto);
        }


        public void SetMillivolts()
        {
            SetRange(
                SparkMultimeterRange.Millivolts);
        }


        public void SetVolts()
        {
            SetRange(
                SparkMultimeterRange.Volts);
        }


        public void SetMilliamps()
        {
            SetRange(
                SparkMultimeterRange.Milliamps);
        }


        public void SetAmps()
        {
            SetRange(
                SparkMultimeterRange.Amps);
        }


        public void SetOhms()
        {
            SetRange(
                SparkMultimeterRange.Ohms);
        }


        public void SetKiloohms()
        {
            SetRange(
                SparkMultimeterRange.Kiloohms);
        }


        public void SetMegaohms()
        {
            SetRange(
                SparkMultimeterRange.Megaohms);
        }


        private void SetRange(
            SparkMultimeterRange selectedRange)
        {
            if (multimeter == null)
            {
                Debug.LogWarning(
                    $"{nameof(SparkMultimeterButtons)}: " +
                    "SparkMultimeter is not assigned.",
                    this);

                return;
            }

            if (!multimeter.SetRange(
                    selectedRange,
                    out string reason))
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


        // ============================================================
        // MODE EVENT
        // ============================================================

        private void HandleModeChanged(
            SparkMultimeterMode mode)
        {
            UpdateModeSelection(mode);
        }


        // ============================================================
        // RANGE EVENT
        // ============================================================

        private void HandleRangeChanged(
            SparkMultimeterRange range)
        {
            UpdateRangeSelection(range);
        }


        // ============================================================
        // MODE VISUAL
        // ============================================================

        private void UpdateModeSelection(
            SparkMultimeterMode selectedMode)
        {
            SetButtonSelected(
                voltageButton,
                selectedMode ==
                SparkMultimeterMode.VoltageDC);

            SetButtonSelected(
                currentButton,
                selectedMode ==
                SparkMultimeterMode.CurrentDC);

            SetButtonSelected(
                resistanceButton,
                selectedMode ==
                SparkMultimeterMode.Resistance);

            SetButtonSelected(
                continuityButton,
                selectedMode ==
                SparkMultimeterMode.Continuity);
        }


        // ============================================================
        // RANGE VISUAL
        // ============================================================

        private void UpdateRangeSelection(
            SparkMultimeterRange selectedRange)
        {
            SetButtonSelected(
                autoButton,
                selectedRange ==
                SparkMultimeterRange.Auto);

            SetButtonSelected(
                millivoltsButton,
                selectedRange ==
                SparkMultimeterRange.Millivolts);

            SetButtonSelected(
                voltsButton,
                selectedRange ==
                SparkMultimeterRange.Volts);

            SetButtonSelected(
                milliampsButton,
                selectedRange ==
                SparkMultimeterRange.Milliamps);

            SetButtonSelected(
                ampsButton,
                selectedRange ==
                SparkMultimeterRange.Amps);

            SetButtonSelected(
                ohmsButton,
                selectedRange ==
                SparkMultimeterRange.Ohms);

            SetButtonSelected(
                kiloohmsButton,
                selectedRange ==
                SparkMultimeterRange.Kiloohms);

            SetButtonSelected(
                megaohmsButton,
                selectedRange ==
                SparkMultimeterRange.Megaohms);
        }


        // ============================================================
        // BUTTON VISUAL
        // ============================================================

        private void SetButtonSelected(
            Button button,
            bool selected)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors =
                button.colors;

            if (selected)
            {
                colors.normalColor =
                    selectedColor *
                    selectedColorMultiplier;

                colors.highlightedColor =
                    selectedColor *
                    selectedColorMultiplier;

                colors.pressedColor =
                    selectedColor;

                colors.selectedColor =
                    selectedColor *
                    selectedColorMultiplier;
            }
            else
            {
                colors.normalColor =
                    normalColor;

                colors.highlightedColor =
                    normalColor;

                colors.pressedColor =
                    normalColor;

                colors.selectedColor =
                    normalColor;
            }

            button.colors =
                colors;
        }
    }
}
