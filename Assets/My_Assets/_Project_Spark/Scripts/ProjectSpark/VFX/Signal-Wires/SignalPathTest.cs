using UnityEngine;

namespace AAAUI.VFX
{
    public sealed class SignalPathTest : MonoBehaviour
    {
        [SerializeField]
        private SignalPath path;

        [SerializeField, Range(0f, 1f)]
        private float progress;

        private void OnDrawGizmos()
        {
            if (path == null)
                return;

            Gizmos.DrawSphere(
                path.GetPosition(progress),
                0.08f
            );
        }
    }
}