using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkVoltageSource :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        [Header("Source")]
        [SerializeField]
        private float voltage = 5f;

        [SerializeField, Min(0.000001f)]
        private float internalResistance = 0.01f;

        [SerializeField]
        private bool enabledOutput = true;

        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        public float Voltage =>
            voltage;

        public float InternalResistance =>
            internalResistance;

        public bool IsOutputActive =>
            enabledOutput &&
            ElectricalEnabled;

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
                (from == positiveTerminal &&
                 to == negativeTerminal) ||
                (from == negativeTerminal &&
                 to == positiveTerminal);
        }

        public void SetVoltage(
            float value)
        {
            if (!IsFinite(value))
            {
                return;
            }

            if (Mathf.Approximately(
                    voltage,
                    value))
            {
                return;
            }

            voltage = value;

            NotifyElectricalConfigurationChanged();
        }

        public void SetOutput(
            bool enabled)
        {
            if (enabledOutput == enabled)
            {
                return;
            }

            enabledOutput = enabled;

            NotifyElectricalConfigurationChanged();
        }

        private void OnValidate()
        {
            internalResistance =
                Mathf.Max(
                    0.000001f,
                    internalResistance);
        }

        private static bool IsFinite(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}