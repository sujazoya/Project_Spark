using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public struct WireSplinePoint
    {
        public Vector3 Position;

        public Vector3 Forward;

        public float Radius;

        public WireSplinePoint(
            Vector3 position,
            Vector3 forward,
            float radius)
        {
            Position = position;
            Forward = forward;
            Radius = radius;
        }
    }
}
