using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementSession
    {
        public GameObject Prefab;

        public GameObject Preview;

        public Vector3 Position;

        public Quaternion Rotation;

        public PlacementState State;
    }
}
