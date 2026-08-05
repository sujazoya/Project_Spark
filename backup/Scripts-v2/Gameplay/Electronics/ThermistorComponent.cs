using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ThermistorComponent
        : ElectronicComponent
    {
        [SerializeField]
        private float resistance25 = 10000f;

        [SerializeField]
        private float beta = 3950f;

        public float GetResistance(
            float temperature)
        {
            float kelvin =
                temperature + 273.15f;

            return resistance25 *
                Mathf.Exp(
                    beta *
                    ((1f / kelvin) -
                    (1f / 298.15f)));
        }
    }
}
