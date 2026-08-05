using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public abstract class LoadComponent : ElectronicComponent
    {
        public bool IsRunning => State.IsPowered;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Simulate(float deltaTime)
        {
        }

        public override void Simulate()
        {
            Simulate(Time.deltaTime);
        }
    }
}