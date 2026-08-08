using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [DisallowMultipleComponent]
    public sealed class SnapPoint : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField] private WireConnector connector;

        [Header("Snap Position")]
        [SerializeField] private Transform snapTransform;

        [Header("Detection")]
        [SerializeField] private float snapRadius = 0.08f;

        public WireConnector Connector =>
            connector;

        public Vector3 Position =>
            snapTransform != null
                ? snapTransform.position
                : transform.position;

        public float SnapRadius =>
            snapRadius;

        public bool CanAcceptWire =>
            connector != null &&
            connector.CanConnect;

        private void Awake()
        {
            if (connector == null)
                connector = GetComponentInParent<WireConnector>();

            if (snapTransform == null)
                snapTransform = transform;

            snapRadius = Mathf.Max(0.001f, snapRadius);
        }

        public bool IsInRange(Vector3 worldPosition)
        {
            return Vector3.Distance(
                worldPosition,
                Position
            ) <= snapRadius;
        }

        public bool TrySnap(WireController wire)
        {
            if (wire == null)
                return false;

            if (!CanAcceptWire)
                return false;

            if (!IsInRange(wire.EndTransform.position))
                return false;

            wire.Connect(this);

            return wire.EndConnector == connector;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(
                snapTransform != null
                    ? snapTransform.position
                    : transform.position,
                snapRadius
            );
        }
#endif
    }
}