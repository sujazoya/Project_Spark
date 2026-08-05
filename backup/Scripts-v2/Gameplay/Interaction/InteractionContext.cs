using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class InteractionContext
    {
        public UnityEngine.Camera Camera;

        public Ray Ray;

        public RaycastHit Hit;

        public Vector3 WorldPosition;

        public float DeltaTime;
    }
}
