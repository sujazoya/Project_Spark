using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class MotorComponent : ElectronicComponent
    {
        [SerializeField]
        private Transform rotor;

        [SerializeField]
        private float rpm = 500f;

        public override void Simulate(float deltaTime)
        {
            State.IsActive =
                State.Voltage > 3f &&
                !State.IsBroken;

            if (!State.IsActive)
                return;

            rotor.Rotate(
                Vector3.forward,
                rpm * 6f * deltaTime);
        }
    }
}
