using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class RelayComponent : ElectronicComponent
    {
        [SerializeField]
        private float activationVoltage = 5f;

        public bool ContactsClosed
            => State.IsActive;

        public override void Simulate(float deltaTime)
        {
            State.IsActive =
                State.Voltage >= activationVoltage &&
                !State.IsBroken;
        }
    }
}
