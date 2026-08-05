using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class DiodeComponent : ElectronicComponent
    {
        [SerializeField]
        private float forwardVoltage = 0.7f;

        public bool IsForwardBiased =>
            State.Voltage >= forwardVoltage;

        public override void Simulate(float deltaTime)
        {
            State.IsActive =
                IsForwardBiased &&
                !State.IsBroken;
        }
    }
}
