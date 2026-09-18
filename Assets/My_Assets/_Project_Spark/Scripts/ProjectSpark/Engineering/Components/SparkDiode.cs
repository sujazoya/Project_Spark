using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkDiode :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal anodeTerminal;

        [SerializeField]
        private SparkTerminal cathodeTerminal;

        [Header("Electrical")]
        [SerializeField, Min(0f)]
        private float forwardVoltage = 0.7f;

        [SerializeField, Min(0.000001f)]
        private float maximumForwardCurrent = 1f;

        [SerializeField, Min(0f)]
        private float reverseVoltageLimit = 100f;

        [SerializeField, Min(0.01f)]
        private float onResistance = 0.1f;

        public SparkTerminal AnodeTerminal =>
            anodeTerminal;

        public SparkTerminal CathodeTerminal =>
            cathodeTerminal;

        public float ForwardVoltage =>
            forwardVoltage;

        public float MaximumForwardCurrent =>
            maximumForwardCurrent;

        public float ReverseVoltageLimit =>
            reverseVoltageLimit;

        public float OnResistance =>
            onResistance;

        public bool IsForwardConducting =>
            ElectricalEnabled &&
            ElectricalState.Conduction ==
            SparkConductionState.Conducting &&
            ElectricalState.Current > 0f;

        public bool IsReverseFault =>
            ElectricalState.Voltage <
            -reverseVoltageLimit;

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled ||
                from == null ||
                to == null)
            {
                return false;
            }

            if (from != anodeTerminal ||
                to != cathodeTerminal)
            {
                return false;
            }

            return IsForwardConducting;
        }

        private void OnValidate()
        {
            forwardVoltage =
                Mathf.Max(
                    0f,
                    forwardVoltage);

            maximumForwardCurrent =
                Mathf.Max(
                    0.000001f,
                    maximumForwardCurrent);

            reverseVoltageLimit =
                Mathf.Max(
                    0f,
                    reverseVoltageLimit);

            onResistance =
                Mathf.Max(
                    0.01f,
                    onResistance);
        }
    }
}