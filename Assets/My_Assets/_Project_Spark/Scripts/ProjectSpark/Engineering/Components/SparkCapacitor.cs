using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkCapacitor : SparkElectricalComponent
    {
        [SerializeField, Min(0.000000001f)] private float capacitanceFarads = 0.000001f;
        [SerializeField, Min(0f)] private float voltageRating = 16f;
        public float CapacitanceFarads => capacitanceFarads;
        public float VoltageRating => voltageRating;
        public bool IsOverVoltage => Mathf.Abs(ElectricalState.Voltage) > voltageRating;
        private void OnValidate() { capacitanceFarads = Mathf.Max(0.000000001f, capacitanceFarads); voltageRating = Mathf.Max(0, voltageRating); }
    }
}
