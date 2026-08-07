using UnityEngine;

namespace ProjectSpark.Hologram
{
    public sealed class HologramTarget : MonoBehaviour
    {
        [SerializeField]
        private HologramData data;

        [SerializeField]
        private Transform projectionPoint;

        public HologramData Data => data;

        public Transform ProjectionPoint =>
            projectionPoint != null ? projectionPoint : transform;

        public void SetData(HologramData value)
        {
            data = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                ProjectionPoint.position,
                0.05f);
        }
#endif
    }
}