using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkRelay :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Coil")]
        [SerializeField]
        private SparkTerminal coilPositiveTerminal;

        [SerializeField]
        private SparkTerminal coilNegativeTerminal;

        [Header("Contact")]
        [SerializeField]
        private SparkTerminal commonTerminal;

        [SerializeField]
        private SparkTerminal normallyOpenTerminal;

        [SerializeField]
        private SparkTerminal normallyClosedTerminal;

        [Header("Electrical")]
        [SerializeField, Min(0f)]
        private float pickupVoltage = 5f;

        [SerializeField]
        private bool energized;

        public SparkTerminal CoilPositiveTerminal =>
            coilPositiveTerminal;

        public SparkTerminal CoilNegativeTerminal =>
            coilNegativeTerminal;

        public SparkTerminal CommonTerminal =>
            commonTerminal;

        public SparkTerminal NormallyOpenTerminal =>
            normallyOpenTerminal;

        public SparkTerminal NormallyClosedTerminal =>
            normallyClosedTerminal;

        public float PickupVoltage =>
            pickupVoltage;

        public bool IsEnergized =>
            energized;

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

            if (energized)
            {
                return
                    IsPair(
                        from,
                        to,
                        commonTerminal,
                        normallyOpenTerminal);
            }

            return
                IsPair(
                    from,
                    to,
                    commonTerminal,
                    normallyClosedTerminal);
        }

        public void SetEnergized(
            bool value)
        {
            if (energized == value)
            {
                return;
            }

            energized = value;

            NotifyElectricalConfigurationChanged();
        }

        public void EvaluateCoil()
        {
            if (!ElectricalEnabled)
            {
                SetEnergized(false);
                return;
            }

            float voltage =
                Mathf.Abs(
                    ElectricalState.Voltage);

            SetEnergized(
                voltage >= pickupVoltage);
        }

        private static bool IsPair(
            SparkTerminal from,
            SparkTerminal to,
            SparkTerminal a,
            SparkTerminal b)
        {
            if (a == null ||
                b == null)
            {
                return false;
            }

            return
                (from == a && to == b) ||
                (from == b && to == a);
        }

        private void OnValidate()
        {
            pickupVoltage =
                Mathf.Max(
                    0f,
                    pickupVoltage);
        }
    }
}