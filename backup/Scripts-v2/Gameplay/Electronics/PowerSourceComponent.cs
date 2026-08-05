using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public abstract class PowerSourceComponent : ElectronicComponent
    {
        [Header("Power Source")]
        [SerializeField]
        private float outputVoltage = 5f;

        [SerializeField]
        private float maxCurrent = 2f;

        public virtual float OutputVoltage => outputVoltage;

        public virtual float MaxCurrent => maxCurrent;

        public override void Simulate(float deltaTime)
        {
            State.IsPowered = true;
            State.Voltage = outputVoltage;
            State.Current = maxCurrent;
        }

        public override void Simulate()
        {
            Simulate(Time.deltaTime);
        }
    }
}