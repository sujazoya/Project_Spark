using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ResistorComponent : ElectronicComponent
    {
        [SerializeField]
        private float resistance = 220f;

        public float Resistance => resistance;

        public override void Simulate(float deltaTime)
        {
            // Passive component
        }
    }
}
