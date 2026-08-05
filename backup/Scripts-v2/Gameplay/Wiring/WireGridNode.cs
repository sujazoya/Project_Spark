using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireGridNode
    {
        public Vector2Int GridPosition;

        public Vector3 WorldPosition;

        public bool Walkable = true;

        public bool Occupied;

        public float GCost;

        public float HCost;

        public float FCost => GCost + HCost;

        public WireGridNode Parent;
    }
}
