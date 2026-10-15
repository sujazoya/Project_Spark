
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkPowerSupplyState
    {
        Off,
        Standby,
        Active,
        Fault
    }

    [DisallowMultipleComponent]
    public sealed class SparkPowerSupply :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        [Header("Output")]
        [SerializeField, Min(0f)]
        private float outputVoltage = 5f;

        [SerializeField, Min(0.0001f)]
        private float currentLimit = 1f;

        [SerializeField]
        private SparkPowerSupplyState state =
            SparkPowerSupplyState.Off;

        private float validatedOutputVoltage;
        private float validatedCurrentLimit;
        private SparkPowerSupplyState validatedState;
        private SparkTerminal validatedPositiveTerminal;
        private SparkTerminal validatedNegativeTerminal;
        private bool validationSnapshotInitialized;

        public SparkTerminal PositiveTerminal => positiveTerminal;
        public SparkTerminal NegativeTerminal => negativeTerminal;

        public float OutputVoltage => outputVoltage;
        public float CurrentLimit => currentLimit;
        public SparkPowerSupplyState State => state;

        public bool IsOutputActive =>
            state == SparkPowerSupplyState.Active &&
            ElectricalEnabled;

        public bool IsCurrentLimited { get; private set; }

        public bool IsOverloaded => IsCurrentLimited;

        public bool IsShortCircuit =>
            IsOutputActive && IsCurrentLimited;

        private void Awake()
        {
            CaptureValidationSnapshot();
        }

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

            return
                (from == positiveTerminal && to == negativeTerminal) ||
                (from == negativeTerminal && to == positiveTerminal);
        }

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
                reason = "Disable output before configuration.";
                return false;
            }

            if (Mathf.Approximately(outputVoltage, voltage) &&
                Mathf.Approximately(currentLimit, limit))
            {
                reason = null;
                return true;
            }

            outputVoltage = voltage;
            currentLimit = limit;
            IsCurrentLimited = false;

            CaptureValidationSnapshot();
            NotifyElectricalConfigurationChanged();

            reason = null;
            return true;
        }

        public SparkResult SetOutput(
            bool enabled,
            in SparkInteractionContext context)
        {
            if (!CanInteract(context, out string reason))
                return SparkResult.Rejected(reason);

            if (state == SparkPowerSupplyState.Fault)
                return SparkResult.Fault("Power supply is faulted.");

            SparkPowerSupplyState nextState = enabled
                ? SparkPowerSupplyState.Active
                : SparkPowerSupplyState.Standby;

            if (state == nextState)
                return SparkResult.Success();

            state = nextState;

            SetOperationalState(
                enabled
                    ? SparkOperationalState.Active
                    : SparkOperationalState.Standby);

            IsCurrentLimited = false;

            CaptureValidationSnapshot();
            NotifyElectricalConfigurationChanged();

            return SparkResult.Success();
        }

        public void SetFault()
        {
            if (state == SparkPowerSupplyState.Fault)
                return;

            state = SparkPowerSupplyState.Fault;
            IsCurrentLimited = false;

            SetOperationalState(SparkOperationalState.Fault);

            CaptureValidationSnapshot();
            NotifyElectricalConfigurationChanged();
        }

        public void ClearFault()
        {
            if (state != SparkPowerSupplyState.Fault)
                return;

            state = SparkPowerSupplyState.Standby;
            IsCurrentLimited = false;

            SetOperationalState(SparkOperationalState.Standby);

            CaptureValidationSnapshot();
            NotifyElectricalConfigurationChanged();
        }

        /// <summary>
        /// Updates solver-owned current-limit status.
        /// This does not invalidate electrical configuration.
        /// </summary>
        public void SetCurrentLimited(bool value)
        {
            IsCurrentLimited = value;
        }

        private void OnValidate()
        {
            outputVoltage = IsFiniteNonNegative(outputVoltage)
                ? outputVoltage
                : 0f;

            currentLimit = IsFinitePositive(currentLimit)
                ? currentLimit
                : 0.0001f;

            // During Edit Mode, just refresh the baseline.
            // During Play Mode, detect Inspector changes and notify
            // the solver if the electrical configuration actually changed.
            if (!Application.isPlaying)
            {
                CaptureValidationSnapshot();
                return;
            }

            if (!validationSnapshotInitialized)
            {
                CaptureValidationSnapshot();
                return;
            }

            bool changed =
                !Mathf.Approximately(outputVoltage, validatedOutputVoltage) ||
                !Mathf.Approximately(currentLimit, validatedCurrentLimit) ||
                state != validatedState ||
                positiveTerminal != validatedPositiveTerminal ||
                negativeTerminal != validatedNegativeTerminal;

            if (!changed)
                return;

            CaptureValidationSnapshot();

            Debug.Log(
                $"[SPARK POWER SUPPLY] Inspector configuration changed | " +
                $"Voltage={outputVoltage} V | " +
                $"CurrentLimit={currentLimit} A | " +
                $"State={state}",
                this);

            NotifyElectricalConfigurationChanged();
        }

        private void CaptureValidationSnapshot()
        {
            validatedOutputVoltage = outputVoltage;
            validatedCurrentLimit = currentLimit;
            validatedState = state;
            validatedPositiveTerminal = positiveTerminal;
            validatedNegativeTerminal = negativeTerminal;
            validationSnapshotInitialized = true;
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value >= 0f;
        }

        private static bool IsFinitePositive(float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value > 0f;
        }
    }
}
