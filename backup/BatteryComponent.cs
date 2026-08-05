using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class BatteryComponent : ElectronicComponent
    {
        [SerializeField]
        private float voltage = 9f;

        [SerializeField]
        private float internalResistance = 0.05f;

        public float Voltage => voltage;

        public float InternalResistance => internalResistance;

        public override void Simulate(float deltaTime)
        {
            State.IsPowered = true;
            State.Voltage = voltage;
            State.IsActive = true;
        }
    }
}
