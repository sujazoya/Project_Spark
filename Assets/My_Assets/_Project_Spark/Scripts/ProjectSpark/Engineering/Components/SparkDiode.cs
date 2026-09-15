using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkDiode : SparkElectricalComponent
    {
        [SerializeField, Min(0f)] private float forwardVoltage = 0.7f;
        [SerializeField, Min(0.000001f)] private float maximumForwardCurrent = 1f;
        [SerializeField, Min(0f)] private float reverseVoltageLimit = 50f;
        [SerializeField, Min(0.01f)] private float onResistance = 1f;
        [SerializeField, Min(0f)] private float conductionHysteresis = 0.01f;

        public float ForwardVoltage => forwardVoltage;
        public float MaximumForwardCurrent => maximumForwardCurrent;
        public float ReverseVoltageLimit => reverseVoltageLimit;
        public float OnResistance => onResistance;
        public bool IsForwardConducting => ElectricalState.Conduction == SparkConductionState.Conducting && ElectricalState.Current > 0f;

        private void OnValidate()
        {
            forwardVoltage = Mathf.Max(0f, forwardVoltage);
            maximumForwardCurrent = Mathf.Max(0.000001f, maximumForwardCurrent);
            reverseVoltageLimit = Mathf.Max(0f, reverseVoltageLimit);
            onResistance = Mathf.Max(0.01f, onResistance);
            conductionHysteresis = Mathf.Max(0f, conductionHysteresis);
        }
    }
}
