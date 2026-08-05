using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireSnapper
    {
        public Vector3 Snap(
            Vector3 world,
            float gridSize)
        {
            return new Vector3(
                Mathf.Round(world.x / gridSize) * gridSize,
                world.y,
                Mathf.Round(world.z / gridSize) * gridSize);
        }
    }
}
