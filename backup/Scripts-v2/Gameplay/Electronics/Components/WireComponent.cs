using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class WireComponent : ElectronicComponent
    {
        [Header("Wire")]
        [SerializeField]
        private float resistance = 0.01f;

        [SerializeField]
        private ConnectionPoint endA;

        [SerializeField]
        private ConnectionPoint endB;

        public ConnectionPoint EndA => endA;
        public ConnectionPoint EndB => endB;
        public float Resistance => resistance;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Simulate(float deltaTime)
        {
            State.Resistance = resistance;

            State.IsPowered =
                endA != null &&
                endB != null &&
                endA.IsConnected &&
                endB.IsConnected;
        }

        public override void Simulate()
        {
            Simulate(Time.deltaTime);
        }
    }
}