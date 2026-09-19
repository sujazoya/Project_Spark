
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Represents the operating state of a Project Spark power supply.
    /// </summary>
    public enum SparkPowerSupplyState
    {
        Off,
        Standby,
        Active,
        Fault
    }

    /// <summary>
    /// Provides a configurable DC electrical power source.
    ///
    /// The supply exposes positive and negative terminals and can
    /// conduct between them while its output is active.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkPowerSupply :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        // =========================================================
        // TERMINALS
        // =========================================================

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        // =========================================================
        // OUTPUT
        // =========================================================

        [Header("Output")]
        [SerializeField, Min(0f)]
        private float outputVoltage = 5f;

        [SerializeField, Min(0.0001f)]
        private float currentLimit = 1f;

        [SerializeField]
        private SparkPowerSupplyState state =
            SparkPowerSupplyState.Off;

        // =========================================================
        // TERMINAL ACCESS
        // =========================================================

        /// <summary>
        /// Gets the positive output terminal.
        /// </summary>
        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        /// <summary>
        /// Gets the negative output terminal.
        /// </summary>
        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        // =========================================================
        // OUTPUT ACCESS
        // =========================================================

        /// <summary>
        /// Gets the configured output voltage in volts.
        /// </summary>
        public float OutputVoltage =>
            outputVoltage;

        /// <summary>
        /// Gets the configured maximum output current in amps.
        /// </summary>
        public float CurrentLimit =>
            currentLimit;

        /// <summary>
        /// Gets the current operating state of the power supply.
        /// </summary>
        public SparkPowerSupplyState State =>
            state;

        /// <summary>
        /// Returns true when the supply is actively providing output
        /// and the underlying electrical component is enabled.
        /// </summary>
        public bool IsOutputActive =>
            state == SparkPowerSupplyState.Active &&
            ElectricalEnabled;

        /// <summary>
        /// Returns true when the supply has reached its configured
        /// current limit.
        /// </summary>
        public bool IsCurrentLimited
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true when the supply is currently overloaded.
        ///
        /// Project Spark represents an overload through the
        /// current-limit condition.
        /// </summary>
        public bool IsOverloaded =>
            IsCurrentLimited;

        // =========================================================
        // CONDUCTION
        // =========================================================

        /// <summary>
/// Returns true when the supply has reached its configured
/// current limit.
/// </summary>

public bool IsShortCircuit =>
    IsOutputActive &&
    IsCurrentLimited;

        /// <summary>
        /// Determines whether electrical conduction is allowed
        /// between two terminals of this power supply.
        /// </summary>
        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!IsOutputActive ||
                from == null ||
                to == null)
            {
                return false;
            }

            bool validForwardPath =
                from == positiveTerminal &&
                to == negativeTerminal;

            bool validReversePath =
                from == negativeTerminal &&
                to == positiveTerminal;

            return
                validForwardPath ||
                validReversePath;
        }

        // =========================================================
        // CONFIGURATION
        // =========================================================

        /// <summary>
        /// Configures the output voltage and current limit.
        ///
        /// Configuration is rejected while the supply is active.
        /// </summary>
        public bool TryConfigure(
            float voltage,
            float limit,
            out string reason)
        {
            if (!IsFiniteNonNegative(voltage))
            {
                reason = "Invalid voltage.";
                return false;
            }

            if (!IsFinitePositive(limit))
            {
                reason = "Invalid current limit.";
                return false;
            }

            if (state == SparkPowerSupplyState.Active)
            {
                reason =
                    "Disable output before configuration.";

                return false;
            }

            outputVoltage = voltage;
            currentLimit = limit;

            IsCurrentLimited = false;

            NotifyElectricalConfigurationChanged();

            reason = null;
            return true;
        }

        // =========================================================
        // OUTPUT CONTROL
        // =========================================================

        /// <summary>
        /// Enables or disables the power supply output.
        /// </summary>
        public SparkResult SetOutput(
            bool enabled,
            in SparkInteractionContext context)
        {
            if (!CanInteract(
                    context,
                    out string reason))
            {
                return SparkResult.Rejected(reason);
            }

            if (state == SparkPowerSupplyState.Fault)
            {
                return SparkResult.Fault(
                    "Power supply is faulted.");
            }

            state =
                enabled
                    ? SparkPowerSupplyState.Active
                    : SparkPowerSupplyState.Standby;

            SetOperationalState(
                enabled
                    ? SparkOperationalState.Active
                    : SparkOperationalState.Standby);

            IsCurrentLimited = false;

            NotifyElectricalConfigurationChanged();

            return SparkResult.Success();
        }

        // =========================================================
        // FAULT CONTROL
        // =========================================================

        /// <summary>
        /// Places the power supply into the fault state.
        /// </summary>
        public void SetFault()
        {
            state = SparkPowerSupplyState.Fault;

            IsCurrentLimited = false;

            SetOperationalState(
                SparkOperationalState.Fault);

            NotifyElectricalConfigurationChanged();
        }

        /// <summary>
        /// Clears the current fault and places the supply into
        /// standby mode.
        /// </summary>
        public void ClearFault()
        {
            if (state != SparkPowerSupplyState.Fault)
            {
                return;
            }

            state = SparkPowerSupplyState.Standby;

            IsCurrentLimited = false;

            SetOperationalState(
                SparkOperationalState.Standby);

            NotifyElectricalConfigurationChanged();
        }

        // =========================================================
        // CURRENT LIMIT
        // =========================================================

        /// <summary>
        /// Updates the current-limit condition.
        /// </summary>
        public void SetCurrentLimited(bool value)
        {
            IsCurrentLimited = value;
        }

        // =========================================================
        // UNITY VALIDATION
        // =========================================================

        private void OnValidate()
        {
            outputVoltage =
                Mathf.Max(
                    0f,
                    outputVoltage);

            currentLimit =
                Mathf.Max(
                    0.0001f,
                    currentLimit);
        }

        // =========================================================
        // NUMERIC VALIDATION
        // =========================================================

        /// <summary>
        /// Returns true when the value is finite and zero or greater.
        /// </summary>
        private static bool IsFiniteNonNegative(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value >= 0f;
        }

        /// <summary>
        /// Returns true when the value is finite and greater than zero.
        /// </summary>
        private static bool IsFinitePositive(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value > 0f;
        }
    }
}
