using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class FuseComponent : ElectronicComponent
    {
        [SerializeField]
        private float maxCurrent = 2f;

        public bool Blown => State.IsBroken;

        public override void Simulate(float deltaTime)
        {
            if (State.Current > maxCurrent)
            {
                State.IsBroken = true;
                State.IsActive = false;
            }
        }
    }
}
