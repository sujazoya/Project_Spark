// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/SnapPoint.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class SnapPoint : MonoBehaviour
    {
        [SerializeField]
        private WireConnector connector;

        [SerializeField]
        private float snapDistance = 0.025f;

        private SphereCollider trigger;

        public WireConnector Connector => connector;

        public Vector3 Position => connector.Point.position;

        public float SnapDistance => snapDistance;

        private void Awake()
        {
            trigger = GetComponent<SphereCollider>();

            trigger.isTrigger = true;
            trigger.radius = snapDistance * 3f;

            if (connector == null)
                connector = GetComponent<WireConnector>();
        }

        public bool CanSnap(Vector3 worldPosition)
        {
            return Vector3.Distance(worldPosition, Position) <= snapDistance;
        }
    }
}