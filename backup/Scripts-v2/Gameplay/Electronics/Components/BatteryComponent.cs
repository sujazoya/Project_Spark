using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class BatteryComponent : ElectronicComponent
    {
        [Header("Battery")]
        [SerializeField]
        private float voltage = 9f;

        [SerializeField]
        private ConnectionPoint positive;

        [SerializeField]
        private ConnectionPoint negative;

        public float Voltage => voltage;

        public ConnectionPoint Positive => positive;

        public ConnectionPoint Negative => negative;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Simulate(float deltaTime)
        {
            State.IsPowered = true;
            State.Voltage = voltage;
        }

        public override void Simulate()
        {
            Simulate(Time.deltaTime);
        }
    }
}