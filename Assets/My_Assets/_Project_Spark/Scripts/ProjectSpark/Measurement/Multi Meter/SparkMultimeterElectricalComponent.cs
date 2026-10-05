using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Measurement
{
    /// <summary>
    /// Electrical model of the multimeter internal current shunt.
    ///
    /// The shunt is only electrically active while Current mode is enabled.
    ///
    /// Voltage / Resistance / Continuity:
    ///     shunt disabled
    ///
    /// Current:
    ///     RED ── 0.05Ω ── BLACK
    ///
    /// Current is calculated from:
    ///
    ///     I = (Vred - Vblack) / Rshunt
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterElectricalComponent
        : SparkElectricalComponent
    {
        // ---------------------------------------------------------------------
        // SHUNT
        // ---------------------------------------------------------------------

        [Header("Current Shunt")]
        [SerializeField, Min(0.000001f)]
        private float shuntResistanceOhms = 0.05f;

        // ---------------------------------------------------------------------
        // TERMINALS
        // ---------------------------------------------------------------------

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal redTerminal;

        [SerializeField]
        private SparkTerminal blackTerminal;

        // ---------------------------------------------------------------------
        // RUNTIME STATE
        // ---------------------------------------------------------------------

        [Header("Runtime")]
        [SerializeField]
        private bool currentMeasurementEnabled;

        /// <summary>
        /// Internal shunt resistance in ohms.
        /// </summary>
        public float ShuntResistanceOhms =>
            Mathf.Max(
                0.000001f,
                shuntResistanceOhms);

        /// <summary>
        /// Internal RED terminal.
        /// </summary>
        public SparkTerminal RedTerminal =>
            redTerminal;

        /// <summary>
        /// Internal BLACK terminal.
        /// </summary>
        public SparkTerminal BlackTerminal =>
            blackTerminal;

        /// <summary>
        /// True only while the multimeter is being used
        /// as an ammeter.
        /// </summary>
        public bool CurrentMeasurementEnabled =>
            currentMeasurementEnabled;

        /// <summary>
        /// Enables/disables the internal current shunt.
        ///
        /// IMPORTANT:
        /// This changes the electrical topology and therefore
        /// requires the solver to solve again.
        /// </summary>
        public void SetCurrentMeasurementEnabled(bool enabled)
        {
            if (currentMeasurementEnabled == enabled)
                return;

            currentMeasurementEnabled = enabled;

            NotifyElectricalConfigurationChanged();
        }

        /// <summary>
        /// Returns the configured internal shunt terminals.
        /// </summary>
        public bool TryGetTerminals(
            out SparkTerminal red,
            out SparkTerminal black)
        {
            red = redTerminal;
            black = blackTerminal;

            return red != null &&
                   black != null;
        }

        /// <summary>
        /// Clears the runtime ammeter state.
        /// </summary>
        public void DisableCurrentMeasurement()
        {
            SetCurrentMeasurementEnabled(false);
        }

        private void OnValidate()
        {
            shuntResistanceOhms =
                Mathf.Max(
                    0.000001f,
                    shuntResistanceOhms);
        }

        private void OnDisable()
        {
            currentMeasurementEnabled = false;
        }
    }
}