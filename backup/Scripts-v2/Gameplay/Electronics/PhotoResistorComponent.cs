using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class PhotoResistorComponent
        : ElectronicComponent
    {
        [Range(0,1)]
        [SerializeField]
        private float lightLevel;

        public float Resistance =>
            Mathf.Lerp(
                500000f,
                500f,
                lightLevel);
    }
}
