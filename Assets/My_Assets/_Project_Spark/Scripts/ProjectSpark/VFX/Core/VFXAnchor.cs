using UnityEngine;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class VFXAnchor : MonoBehaviour
    {
        [SerializeField]
        private Transform anchor;

        public Transform Transform =>
            anchor != null ? anchor : transform;

        public Vector3 Position =>
            Transform.position;

        public Quaternion Rotation =>
            Transform.rotation;

        public void SetAnchor(Transform target)
        {
            anchor = target;
        }

        private void OnDrawGizmosSelected()
        {
            Transform target = Transform;

            Gizmos.DrawWireSphere(
                target.position,
                0.025f
            );

            Gizmos.DrawLine(
                target.position,
                target.position + target.forward * 0.15f
            );
        }
    }
}