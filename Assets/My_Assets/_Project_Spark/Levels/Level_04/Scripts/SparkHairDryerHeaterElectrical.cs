using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Electrical heater load for the Project Spark hair dryer.
    ///
    /// This is an electrical component only.
    /// Heater appearance is handled by the visual system.
    ///
    /// The electrical solver treats this as a two-terminal
    /// resistive heating element.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerHeaterElectrical :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        // ============================================================
        // TERMINALS
        // ============================================================

        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal liveTerminal;

        [SerializeField]
        private SparkTerminal neutralTerminal;

        // ============================================================
        // ELECTRICAL
        // ============================================================

        [Header("Heater Electrical")]
        [SerializeField, Min(0.000001f)]
        private float resistance = 29.5f;

        [SerializeField, Min(0f)]
        private float voltageThreshold = 1f;

        [SerializeField, Min(0f)]
        private float currentThreshold = 0.001f;

        // ============================================================
        // PUBLIC
        // ============================================================

        public SparkTerminal LiveTerminal =>
            liveTerminal;

        public SparkTerminal NeutralTerminal =>
            neutralTerminal;

        public float Resistance =>
            resistance;

        public float Voltage =>
            ElectricalState.Voltage;

        public float Current =>
            ElectricalState.Current;

        public float Power =>
            ElectricalState.Power;

        public SparkConductionState Conduction =>
            ElectricalState.Conduction;

        public bool HasVoltage =>
            Mathf.Abs(Voltage) >= voltageThreshold;

        public bool HasCurrent =>
            Mathf.Abs(Current) >= currentThreshold;

        public bool IsPowered =>
            HasVoltage;

        public bool IsHeating =>
            IsPowered &&
            HasCurrent &&
            Conduction == SparkConductionState.Conducting;

        public bool IsActuallyConducting =>
            Conduction == SparkConductionState.Conducting;

        // ============================================================
        // CONFIGURATION
        // ============================================================

        public void SetTerminals(
            SparkTerminal live,
            SparkTerminal neutral)
        {
            liveTerminal = live;
            neutralTerminal = neutral;

            NotifyElectricalConfigurationChanged();
        }

        public void SetResistance(float value)
        {
            resistance =
                Mathf.Max(0.000001f, value);

            NotifyElectricalConfigurationChanged();
        }

        // ============================================================
        // CONDUCTIVE DEVICE
        // ============================================================

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled)
                return false;

            if (liveTerminal == null ||
                neutralTerminal == null)
            {
                return false;
            }

            bool validDirection =
                (from == liveTerminal &&
                 to == neutralTerminal) ||
                (from == neutralTerminal &&
                 to == liveTerminal);

            return validDirection;
        }

        // ============================================================
        // ELECTRICAL STATE
        // ============================================================

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            resistance =
                Mathf.Max(0.000001f, resistance);

            voltageThreshold =
                Mathf.Max(0f, voltageThreshold);

            currentThreshold =
                Mathf.Max(0f, currentThreshold);
        }
    }
}
