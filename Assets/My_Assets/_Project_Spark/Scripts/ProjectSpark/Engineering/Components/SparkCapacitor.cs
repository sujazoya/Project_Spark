
using UnityEngine;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Two-terminal polarized capacitor.
    /// Electrical simulation is handled by
    /// SparkCapacitorSolverDefinition.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkCapacitor : SparkElectricalComponent
    {
        [Header("Electrical Terminals")]
        [SerializeField] private SparkTerminal positiveTerminal;
        [SerializeField] private SparkTerminal negativeTerminal;

        [Header("Capacitor Properties")]
        [SerializeField, Min(0.000000001f)]
        private float capacitanceFarads = 0.001f;

        [SerializeField, Min(0f)]
        private float voltageRating = 16f;

        public SparkTerminal PositiveTerminal => positiveTerminal;
        public SparkTerminal NegativeTerminal => negativeTerminal;

        public float CapacitanceFarads => capacitanceFarads;
        public float VoltageRating => voltageRating;

        public bool HasValidTerminals =>
            positiveTerminal != null &&
            negativeTerminal != null &&
            positiveTerminal != negativeTerminal;

        public bool IsOverVoltage =>
            voltageRating > 0f &&
            Mathf.Abs(ElectricalState.Voltage) > voltageRating;

        public void ConfigureTerminals(
            SparkTerminal positive,
            SparkTerminal negative)
        {
            positiveTerminal = positive;
            negativeTerminal = negative;
        }

        private void OnValidate()
        {
            capacitanceFarads = Mathf.Max(
                0.000000001f,
                capacitanceFarads);

            voltageRating = Mathf.Max(0f, voltageRating);
        }
    }
}
