using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class TransformerComponent : ElectronicComponent
    {
        [SerializeField]
        private float turnsRatio = 2f;

        public float OutputVoltage =>
            State.Voltage * turnsRatio;

        public override void Simulate(float deltaTime)
        {
            State.IsActive =
                State.Voltage > 0f;
        }
    }
}
