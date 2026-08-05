using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class CollisionValidator
    {
        public bool HasCollision(
            Vector3 center,
            Vector3 size,
            LayerMask mask)
        {
            return Physics.CheckBox(
                center,
                size * 0.5f,
                Quaternion.identity,
                mask);
        }
    }
}
