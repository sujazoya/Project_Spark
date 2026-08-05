using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class PotentiometerComponent
        : ElectronicComponent
    {
        [SerializeField]
        private float maximumResistance = 10000f;

        [Range(0,1)]
        [SerializeField]
        private float knob;

        public float Resistance =>
            maximumResistance * knob;
    }
}
