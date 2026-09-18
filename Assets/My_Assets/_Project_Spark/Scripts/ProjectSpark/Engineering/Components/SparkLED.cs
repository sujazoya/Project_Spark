using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLED :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal anodeTerminal;

        [SerializeField]
        private SparkTerminal cathodeTerminal;

        [Header("LED")]
        [SerializeField, Min(0f)]
        private float forwardVoltage = 2f;

        [SerializeField, Min(0.000001f)]
        private float maximumForwardCurrent = 0.02f;

        [SerializeField, Min(0f)]
        private float reverseVoltageLimit = 5f;

        [SerializeField, Min(0.01f)]
        private float onResistance = 1f;

        [SerializeField, Min(0.01f)]
        private float conductionHysteresis = 0.01f;

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

        public bool IsOn =>
            ElectricalState.Conduction ==
            SparkConductionState.Conducting &&
            ElectricalState.Current > 0f;

        public bool IsOverCurrent =>
            Mathf.Abs(
                ElectricalState.Current) >
            maximumForwardCurrent;

        public bool IsReverseVoltageExceeded =>
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

            if (anodeTerminal == null ||
                cathodeTerminal == null)
            {
                return false;
            }

            if (from != anodeTerminal ||
                to != cathodeTerminal)
            {
                return false;
            }

            return
                ElectricalState.Conduction ==
                SparkConductionState.Conducting;
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

            conductionHysteresis =
                Mathf.Max(
                    0.01f,
                    conductionHysteresis);
        }
    }
}