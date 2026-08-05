using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class SwitchComponent : ElectronicComponent
    {
        [SerializeField]
        private bool isClosed;

        public bool IsClosed => isClosed;

        public void Toggle()
        {
            isClosed = !isClosed;
        }

        public override void Simulate(float deltaTime)
        {
            State.IsActive = isClosed;
        }
    }
}
