using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkPowerSupplyState { Off, Standby, Active, Fault }

    public sealed class SparkPowerSupply : SparkElectricalComponent
    {
        [SerializeField, Min(0f)] private float outputVoltage = 5f;
        [SerializeField, Min(0.0001f)] private float currentLimit = 1f;
        [SerializeField] private SparkPowerSupplyState state = SparkPowerSupplyState.Off;

        public float OutputVoltage => outputVoltage;
        public float CurrentLimit => currentLimit;
        public SparkPowerSupplyState State => state;
        public bool IsOutputActive => state == SparkPowerSupplyState.Active;
        public bool IsCurrentLimited { get; private set; }

        public bool TryConfigure(float voltage, float limit, out string reason)
        {
            if (!IsFiniteNonNegative(voltage)) { reason = "Invalid voltage."; return false; }
            if (!IsFinitePositive(limit)) { reason = "Invalid current limit."; return false; }
            if (state == SparkPowerSupplyState.Active) { reason = "Disable output before configuration."; return false; }
            outputVoltage = voltage;
            currentLimit = limit;
            IsCurrentLimited = false;
            NotifyElectricalConfigurationChanged();
            reason = null;
            return true;
        }

        public SparkResult SetOutput(bool enabled, in SparkInteractionContext context)
        {
            if (!CanInteract(context, out var reason)) return SparkResult.Rejected(reason);
            if (state == SparkPowerSupplyState.Fault) return SparkResult.Fault("Power supply is faulted.");
            state = enabled ? SparkPowerSupplyState.Active : SparkPowerSupplyState.Standby;
            SetOperationalState(enabled ? SparkOperationalState.Active : SparkOperationalState.Standby);
            IsCurrentLimited = false;
            NotifyElectricalConfigurationChanged();
            return SparkResult.Success();
        }

        public void SetFault()
        {
            state = SparkPowerSupplyState.Fault;
            IsCurrentLimited = false;
            SetOperationalState(SparkOperationalState.Fault);
            NotifyElectricalConfigurationChanged();
        }

        public void ClearFault()
        {
            if (state != SparkPowerSupplyState.Fault) return;
            state = SparkPowerSupplyState.Standby;
            IsCurrentLimited = false;
            SetOperationalState(SparkOperationalState.Standby);
            NotifyElectricalConfigurationChanged();
        }

        public void SetCurrentLimited(bool value) => IsCurrentLimited = value;

        private void OnValidate()
        {
            outputVoltage = Mathf.Max(0f, outputVoltage);
            currentLimit = Mathf.Max(0.0001f, currentLimit);
        }

        private static bool IsFiniteNonNegative(float v) => !float.IsNaN(v) && !float.IsInfinity(v) && v >= 0;
        private static bool IsFinitePositive(float v) => !float.IsNaN(v) && !float.IsInfinity(v) && v > 0;
    }
}
