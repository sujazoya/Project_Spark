using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkBattery :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal positiveTerminal;

        [SerializeField]
        private SparkTerminal negativeTerminal;

        [Header("Battery")]
        [SerializeField, Min(0f)]
        private float nominalVoltage = 5f;

        [SerializeField, Min(0.000001f)]
        private float internalResistance = 0.1f;

        [SerializeField]
        private bool outputEnabled = true;

        public SparkTerminal PositiveTerminal =>
            positiveTerminal;

        public SparkTerminal NegativeTerminal =>
            negativeTerminal;

        public float NominalVoltage =>
            nominalVoltage;

        public float InternalResistance =>
            internalResistance;

        public bool OutputEnabled =>
            outputEnabled;

        public bool IsOutputActive =>
            outputEnabled &&
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

        public void SetOutputEnabled(
            bool enabled)
        {
            if (outputEnabled == enabled)
            {
                return;
            }

            outputEnabled = enabled;

            NotifyElectricalConfigurationChanged();
        }

        public void SetNominalVoltage(
            float voltage)
        {
            if (!IsFiniteNonNegative(voltage))
            {
                return;
            }

            if (Mathf.Approximately(
                    nominalVoltage,
                    voltage))
            {
                return;
            }

            nominalVoltage = voltage;

            NotifyElectricalConfigurationChanged();
        }

        private void OnValidate()
        {
            nominalVoltage =
                Mathf.Max(
                    0f,
                    nominalVoltage);

            internalResistance =
                Mathf.Max(
                    0.000001f,
                    internalResistance);
        }

        private static bool IsFiniteNonNegative(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value >= 0f;
        }
    }
}