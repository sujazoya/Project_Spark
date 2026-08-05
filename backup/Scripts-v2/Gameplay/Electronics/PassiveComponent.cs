using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public abstract class PassiveComponent : ElectronicComponent
    {
        [SerializeField]
        protected float resistance = 1000f;

        public virtual float Resistance => resistance;

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