using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireMaterialController
    {
        public void SetPowered(
            Material material,
            bool powered)
        {
            material.EnableKeyword(
                powered
                    ? "_POWER_ON"
                    : "_POWER_OFF");
        }
    }
}
