using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public static class RotationSystem
    {
        public static Quaternion Rotate90(Quaternion rotation)
        {
            return rotation *
                Quaternion.Euler(0,90,0);
        }

        public static Quaternion Rotate180(Quaternion rotation)
        {
            return rotation *
                Quaternion.Euler(0,180,0);
        }
    }
}
