using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkTransistorType
    {
        NPN,
        PNP
    }

    [DisallowMultipleComponent]
    public sealed class SparkTransistor :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal collectorTerminal;

        [SerializeField]
        private SparkTerminal baseTerminal;

        [SerializeField]
        private SparkTerminal emitterTerminal;

        [Header("Transistor")]
        [SerializeField]
        private SparkTransistorType type =
            SparkTransistorType.NPN;

        [SerializeField, Min(0f)]
        private float baseThreshold = 0.7f;

        [SerializeField, Min(0.000001f)]
        private float minimumCollectorCurrent = 0.000001f;

        [SerializeField, Min(0.000001f)]
        private float onResistance = 10f;

        public SparkTerminal CollectorTerminal =>
            collectorTerminal;

        public SparkTerminal BaseTerminal =>
            baseTerminal;

        public SparkTerminal EmitterTerminal =>
            emitterTerminal;

        public SparkTransistorType Type =>
            type;

        public float BaseThreshold =>
            baseThreshold;

        public float OnResistance =>
            onResistance;

        public bool IsBaseDriven
        {
            get
            {
                if (!ElectricalEnabled)
                {
                    return false;
                }

                return
                    ElectricalState.Voltage >=
                    baseThreshold;
            }
        }

        public bool IsConducting =>
            ElectricalEnabled &&
            ElectricalState.Conduction ==
            SparkConductionState.Conducting;

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

            if (!IsBaseDriven)
            {
                return false;
            }

            return
                (from == collectorTerminal &&
                 to == emitterTerminal) ||
                (from == emitterTerminal &&
                 to == collectorTerminal);
        }

        public bool CanControlBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled ||
                from == null ||
                to == null)
            {
                return false;
            }

            return
                (from == baseTerminal &&
                 to == emitterTerminal) ||
                (from == emitterTerminal &&
                 to == baseTerminal);
        }

        private void OnValidate()
        {
            baseThreshold =
                Mathf.Max(
                    0f,
                    baseThreshold);

            minimumCollectorCurrent =
                Mathf.Max(
                    0.000001f,
                    minimumCollectorCurrent);

            onResistance =
                Mathf.Max(
                    0.000001f,
                    onResistance);
        }
    }
}