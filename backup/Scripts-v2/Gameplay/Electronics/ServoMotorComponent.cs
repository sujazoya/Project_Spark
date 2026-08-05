using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ServoMotorComponent
        : ElectronicComponent
    {
        [SerializeField]
        private Transform shaft;

        [Range(0,180)]
        [SerializeField]
        private float angle;

        public override void Simulate(float deltaTime)
        {
            if (!State.IsActive)
                return;

            shaft.localRotation =
                Quaternion.Euler(
                    0,
                    angle,
                    0);
        }
    }
}
