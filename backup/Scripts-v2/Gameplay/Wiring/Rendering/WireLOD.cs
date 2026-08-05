using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireLOD
    {
        public int GetSides(
            float distance)
        {
            if (distance < 3f)
                return 12;

            if (distance < 8f)
                return 8;

            return 4;
        }
    }
}
